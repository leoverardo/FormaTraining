using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FitPlatform.Infrastructure.Services;

/// <summary>
/// Origem de uma transição de estado de assinatura. Usada apenas em log estruturado.
/// </summary>
public enum SubscriptionTransitionSource
{
    Webhook,
    Reconciliation,
    TrainerAction
}

/// <summary>
/// Dados externos de um evento de pagamento, independentes de virem de um webhook
/// ou de uma consulta de reconciliação.
/// </summary>
public sealed record SubscriptionPaymentContext(
    string? ProviderSubscriptionId = null,
    string? ProviderCheckoutId = null,
    string? ProviderCustomerId = null,
    string? ProviderPaymentId = null,
    int? AmountInCents = null,
    string? RawPayload = null,
    SubscriptionTransitionSource Source = SubscriptionTransitionSource.Webhook);

/// <summary>
/// Regras de transição de estado de uma <see cref="TrainerSubscription"/>.
///
/// Este serviço é o único lugar onde o estado de assinatura e pagamento é alterado
/// em resposta ao provedor — é consumido tanto pelo processamento de webhooks quanto
/// pela rotina de reconciliação, garantindo que os dois caminhos não divirjam.
///
/// Todas as operações são idempotentes: aplicar o mesmo evento duas vezes não duplica
/// pagamentos, não resgata cupom duas vezes e não reativa onboarding já concluído.
/// O chamador é responsável pela transação e pelo SaveChanges final.
/// </summary>
public sealed class SubscriptionLifecycleService
{
    private readonly AppDbContext _db;
    private readonly PasswordSetupService _passwordSetupService;
    private readonly ILogger<SubscriptionLifecycleService> _logger;

    public SubscriptionLifecycleService(
        AppDbContext db,
        PasswordSetupService passwordSetupService,
        ILogger<SubscriptionLifecycleService> logger)
    {
        _db = db;
        _passwordSetupService = passwordSetupService;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Ativação (primeiro pagamento confirmado)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Ativa a assinatura após a confirmação do primeiro pagamento.
    /// Retorna false quando nada foi aplicado por já estar ativo/duplicado.
    /// </summary>
    public async Task<bool> ActivateAsync(
        TrainerSubscription sub, SubscriptionPaymentContext ctx, CancellationToken ct)
    {
        // Idempotência 1: pagamento já registrado como aprovado com o mesmo id externo.
        if (await HasApprovedPaymentAsync(sub.Id, ctx.ProviderPaymentId, ct))
        {
            _logger.LogInformation(
                "Ativação ignorada: pagamento já aprovado. SubscriptionId={SubscriptionId} ProviderPaymentId={ProviderPaymentId} Source={Source}",
                sub.Id, ctx.ProviderPaymentId, ctx.Source);
            return false;
        }

        // Idempotência 2: assinatura já ativa e vigente, sem novo pagamento para registrar.
        if (sub.Status == TrainerSubscriptionStatus.Active
            && sub.EndDate > DateTime.UtcNow
            && string.IsNullOrWhiteSpace(ctx.ProviderPaymentId))
        {
            _logger.LogInformation(
                "Ativação ignorada: assinatura já ativa. SubscriptionId={SubscriptionId} Source={Source}",
                sub.Id, ctx.Source);
            return false;
        }

        var now = DateTime.UtcNow;
        var previousStatus = sub.Status;

        sub.Status = TrainerSubscriptionStatus.Active;
        sub.LastPaymentStatus = "paid";
        sub.EndDate = CalculateCycleEndDate(sub.BillingCycle);
        sub.PaymentFailureCount = 0;
        sub.LastPaymentFailureAt = null;
        sub.UpdatedAt = now;

        ApplyProviderIdentifiers(sub, ctx);

        var payment = await ResolvePaymentForActivationAsync(sub, ctx, ct);
        payment.Status = PaymentStatus.Approved;
        payment.PaidAt ??= now;
        payment.ProviderPaymentId ??= ctx.ProviderPaymentId;
        payment.ProviderSubscriptionId ??= ctx.ProviderSubscriptionId ?? sub.AbacatePaySubscriptionId;
        payment.AbacatePayCheckoutId ??= ctx.ProviderCheckoutId ?? sub.AbacatePayCheckoutId;
        if (ctx.RawPayload is not null) payment.RawPayload = ctx.RawPayload;
        payment.UpdatedAt = now;

        await _db.SaveChangesAsync(ct);

        await TryRedeemCouponAsync(sub, payment, ct);
        await AdvanceOnboardingAsync(sub, ct);

        _logger.LogInformation(
            "Assinatura ativada. SubscriptionId={SubscriptionId} TrainerId={TrainerId} " +
            "PreviousStatus={PreviousStatus} NewStatus={NewStatus} ProviderSubscriptionId={ProviderSubscriptionId} " +
            "ProviderPaymentId={ProviderPaymentId} EndDate={EndDate:O} Source={Source}",
            sub.Id, sub.TrainerId, previousStatus, sub.Status,
            sub.AbacatePaySubscriptionId ?? "N/A", ctx.ProviderPaymentId ?? "N/A", sub.EndDate, ctx.Source);

        return true;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Renovação
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Registra uma renovação paga: estende o período, zera o contador de falhas e,
    /// quando houver troca de plano agendada, aplica-a — a AbacatePay efetiva a troca
    /// no início do novo ciclo.
    /// </summary>
    public async Task<bool> RegisterRenewalAsync(
        TrainerSubscription sub, SubscriptionPaymentContext ctx, CancellationToken ct)
    {
        if (await HasApprovedPaymentAsync(sub.Id, ctx.ProviderPaymentId, ct))
        {
            _logger.LogInformation(
                "Renovação ignorada: pagamento já registrado. SubscriptionId={SubscriptionId} ProviderPaymentId={ProviderPaymentId}",
                sub.Id, ctx.ProviderPaymentId);
            return false;
        }

        var now = DateTime.UtcNow;

        await ApplyPendingPlanChangeAsync(sub, ct);

        sub.Status = TrainerSubscriptionStatus.Active;
        sub.LastPaymentStatus = "paid";
        sub.EndDate = CalculateCycleEndDate(sub.BillingCycle);
        sub.PaymentFailureCount = 0;
        sub.LastPaymentFailureAt = null;
        sub.UpdatedAt = now;

        ApplyProviderIdentifiers(sub, ctx);

        _db.TrainerPayments.Add(new TrainerPayment
        {
            TrainerId = sub.TrainerId,
            TrainerSubscriptionId = sub.Id,
            Amount = ResolveAmount(ctx.AmountInCents, sub.FinalAmountInCents),
            Status = PaymentStatus.Approved,
            Provider = "AbacatePay",
            ProviderPaymentId = ctx.ProviderPaymentId,
            ProviderSubscriptionId = ctx.ProviderSubscriptionId ?? sub.AbacatePaySubscriptionId,
            AbacatePayCheckoutId = sub.AbacatePayCheckoutId,
            PaidAt = now,
            RawPayload = ctx.RawPayload
        });

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Renovação registrada. SubscriptionId={SubscriptionId} TrainerId={TrainerId} " +
            "ProviderPaymentId={ProviderPaymentId} NovaDataFim={EndDate:O} Source={Source}",
            sub.Id, sub.TrainerId, ctx.ProviderPaymentId ?? "N/A", sub.EndDate, ctx.Source);

        return true;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Falha de cobrança recorrente
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Registra uma falha de cobrança recorrente.
    ///
    /// Decisão de domínio: o enum <see cref="TrainerSubscriptionStatus"/> não possui
    /// PastDue/Suspended e a AbacatePay retenta conforme o retryPolicy configurado,
    /// emitindo subscription.cancelled só ao esgotar as tentativas. Portanto uma falha
    /// isolada NÃO altera o Status nem trunca o EndDate: o acesso continua válido até o
    /// fim do período já pago (regra de <c>ActiveTrainerSubscriptionHandler</c>), e a
    /// falha fica registrada em LastPaymentStatus / PaymentFailureCount / TrainerPayment.
    /// </summary>
    public async Task<bool> RegisterPaymentFailureAsync(
        TrainerSubscription sub, SubscriptionPaymentContext ctx, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        // Idempotência: a mesma tentativa de cobrança não é contabilizada duas vezes.
        if (!string.IsNullOrWhiteSpace(ctx.ProviderPaymentId))
        {
            var alreadyRejected = await _db.TrainerPayments.AnyAsync(
                x => x.TrainerSubscriptionId == sub.Id
                     && x.Provider == "AbacatePay"
                     && x.ProviderPaymentId == ctx.ProviderPaymentId
                     && x.Status == PaymentStatus.Rejected,
                ct);

            if (alreadyRejected)
            {
                _logger.LogInformation(
                    "Falha de pagamento ignorada: já registrada. SubscriptionId={SubscriptionId} ProviderPaymentId={ProviderPaymentId}",
                    sub.Id, ctx.ProviderPaymentId);
                return false;
            }
        }

        sub.LastPaymentStatus = "failed";
        sub.PaymentFailureCount += 1;
        sub.LastPaymentFailureAt = now;
        sub.UpdatedAt = now;
        ApplyProviderIdentifiers(sub, ctx);

        var payment = await ResolvePaymentForFailureAsync(sub, ctx.ProviderPaymentId, ct);
        if (payment is null)
        {
            payment = new TrainerPayment
            {
                TrainerId = sub.TrainerId,
                TrainerSubscriptionId = sub.Id,
                Amount = ResolveAmount(ctx.AmountInCents, sub.FinalAmountInCents),
                Provider = "AbacatePay",
                ProviderSubscriptionId = ctx.ProviderSubscriptionId ?? sub.AbacatePaySubscriptionId,
                AbacatePayCheckoutId = sub.AbacatePayCheckoutId
            };
            _db.TrainerPayments.Add(payment);
        }

        payment.Status = PaymentStatus.Rejected;
        payment.ProviderPaymentId ??= ctx.ProviderPaymentId;
        if (ctx.RawPayload is not null) payment.RawPayload = ctx.RawPayload;
        payment.UpdatedAt = now;

        await _db.SaveChangesAsync(ct);

        _logger.LogWarning(
            "Falha de cobrança recorrente registrada. SubscriptionId={SubscriptionId} TrainerId={TrainerId} " +
            "FailureCount={FailureCount} ProviderPaymentId={ProviderPaymentId} Status={Status} AcessoAte={EndDate:O}",
            sub.Id, sub.TrainerId, sub.PaymentFailureCount, ctx.ProviderPaymentId ?? "N/A", sub.Status, sub.EndDate);

        return true;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Cancelamento e expiração
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Marca a assinatura como cancelada preservando o EndDate, para que o trainer
    /// mantenha acesso até o fim do período já pago. Idempotente.
    /// </summary>
    public async Task<bool> CancelAsync(
        TrainerSubscription sub, SubscriptionTransitionSource source, CancellationToken ct)
    {
        if (sub.Status == TrainerSubscriptionStatus.Canceled)
        {
            _logger.LogInformation(
                "Cancelamento ignorado: assinatura já cancelada. SubscriptionId={SubscriptionId} Source={Source}",
                sub.Id, source);
            return false;
        }

        var previousStatus = sub.Status;
        sub.Status = TrainerSubscriptionStatus.Canceled;
        sub.LastPaymentStatus = "cancelled";
        sub.UpdatedAt = DateTime.UtcNow;

        // Troca de plano agendada deixa de fazer sentido após o cancelamento.
        ClearPendingPlanChange(sub);

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Assinatura cancelada. SubscriptionId={SubscriptionId} TrainerId={TrainerId} " +
            "PreviousStatus={PreviousStatus} AcessoAte={EndDate:O} Source={Source}",
            sub.Id, sub.TrainerId, previousStatus, sub.EndDate, source);

        return true;
    }

    /// <summary>
    /// Marca a assinatura como expirada (checkout venceu sem pagamento). Idempotente.
    /// Deliberadamente NÃO expira uma assinatura ativa ou cancelada — para esse caso,
    /// use <see cref="ForceExpireAsync"/>, que exige confirmação explícita do provedor.
    /// </summary>
    public async Task<bool> MarkExpiredAsync(
        TrainerSubscription sub, SubscriptionTransitionSource source, CancellationToken ct)
    {
        if (sub.Status is TrainerSubscriptionStatus.Expired or TrainerSubscriptionStatus.Active or TrainerSubscriptionStatus.Canceled)
            return false;

        return await ApplyExpirationAsync(sub, source, ct);
    }

    /// <summary>
    /// Expira uma assinatura mesmo que esteja ativa localmente. Reservado ao caso em que
    /// o provedor afirma explicitamente que a assinatura está <c>EXPIRED</c> — nunca deve
    /// ser chamado por inferência.
    /// </summary>
    public async Task<bool> ForceExpireAsync(
        TrainerSubscription sub, SubscriptionTransitionSource source, CancellationToken ct)
    {
        if (sub.Status is TrainerSubscriptionStatus.Expired or TrainerSubscriptionStatus.Canceled)
            return false;

        return await ApplyExpirationAsync(sub, source, ct);
    }

    private async Task<bool> ApplyExpirationAsync(
        TrainerSubscription sub, SubscriptionTransitionSource source, CancellationToken ct)
    {
        var previousStatus = sub.Status;
        sub.Status = TrainerSubscriptionStatus.Expired;
        sub.LastPaymentStatus = "expired";
        sub.UpdatedAt = DateTime.UtcNow;
        ClearPendingPlanChange(sub);

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Assinatura expirada. SubscriptionId={SubscriptionId} TrainerId={TrainerId} PreviousStatus={PreviousStatus} Source={Source}",
            sub.Id, sub.TrainerId, previousStatus, source);

        return true;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Troca de plano agendada
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Aplica uma troca de plano previamente agendada. Chamado na renovação, que é o
    /// momento em que a AbacatePay passa a cobrar o novo valor.
    /// </summary>
    public async Task<bool> ApplyPendingPlanChangeAsync(TrainerSubscription sub, CancellationToken ct)
    {
        if (!sub.PendingPlatformPlanId.HasValue) return false;

        var newPlanId = sub.PendingPlatformPlanId.Value;
        var newCycle = sub.PendingBillingCycle ?? sub.BillingCycle;
        var previousPlanId = sub.PlatformPlanId;

        var plan = await _db.PlatformPlans.FirstOrDefaultAsync(x => x.Id == newPlanId, ct);
        if (plan is null)
        {
            _logger.LogWarning(
                "Troca de plano agendada descartada: plano inexistente. SubscriptionId={SubscriptionId} PendingPlanId={PendingPlanId}",
                sub.Id, newPlanId);
            ClearPendingPlanChange(sub);
            return false;
        }

        sub.PlatformPlanId = newPlanId;
        sub.BillingCycle = newCycle;
        // O preço do novo ciclo já foi calculado e validado pelo backend no momento da solicitação.
        if (sub.PendingFinalAmountInCents.HasValue)
        {
            sub.FinalAmountInCents = sub.PendingFinalAmountInCents.Value;
            sub.BaseAmountInCents = sub.PendingFinalAmountInCents.Value;
            sub.CycleDiscountAmountInCents = 0;
            sub.CouponDiscountAmountInCents = 0;
            sub.CouponCodeApplied = null;
        }
        // O preço por ciclo deixa de referenciar a tabela antiga de PlatformPlanPrice.
        sub.PlatformPlanPriceId = null;

        ClearPendingPlanChange(sub);
        sub.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Troca de plano aplicada no novo ciclo. SubscriptionId={SubscriptionId} TrainerId={TrainerId} " +
            "PlanoAnterior={PreviousPlanId} NovoPlano={NewPlanId} NovoCiclo={NewCycle} NovoValorCentavos={Amount}",
            sub.Id, sub.TrainerId, previousPlanId, newPlanId, newCycle, sub.FinalAmountInCents);

        return true;
    }

    private static void ClearPendingPlanChange(TrainerSubscription sub)
    {
        sub.PendingPlatformPlanId = null;
        sub.PendingBillingCycle = null;
        sub.PendingPlanChangeProviderId = null;
        sub.PendingPlanChangeRequestedAt = null;
        sub.PendingFinalAmountInCents = null;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Cupom
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Registra o resgate do cupom aplicado na assinatura. Protegido por índice único
    /// (CouponId, TrainerId, SubscriptionId) e por verificação prévia — nunca conta duas vezes.
    /// </summary>
    private async Task TryRedeemCouponAsync(TrainerSubscription sub, TrainerPayment payment, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(sub.CouponCodeApplied) || sub.CouponDiscountAmountInCents <= 0)
            return;

        var normalized = sub.CouponCodeApplied.Trim().ToUpperInvariant();
        var coupon = await _db.DiscountCoupons.FirstOrDefaultAsync(c => c.Code.ToUpper() == normalized, ct);
        if (coupon is null) return;

        var alreadyRedeemed = await _db.DiscountCouponRedemptions.AnyAsync(
            r => r.CouponId == coupon.Id && r.TrainerId == sub.TrainerId && r.SubscriptionId == sub.Id, ct);
        if (alreadyRedeemed) return;

        if (coupon.MaxUsesTotal.HasValue && coupon.CurrentUses >= coupon.MaxUsesTotal.Value)
        {
            _logger.LogWarning(
                "Cupom sem saldo no resgate. Coupon={CouponCode} SubscriptionId={SubscriptionId}", coupon.Code, sub.Id);
            return;
        }

        if (coupon.MaxUsesPerCustomer.HasValue)
        {
            var uses = await _db.DiscountCouponRedemptions
                .CountAsync(x => x.CouponId == coupon.Id && x.TrainerId == sub.TrainerId, ct);
            if (uses >= coupon.MaxUsesPerCustomer.Value)
            {
                _logger.LogWarning(
                    "Limite de cupom por cliente excedido no resgate. Coupon={CouponCode} TrainerId={TrainerId}",
                    coupon.Code, sub.TrainerId);
                return;
            }
        }

        _db.DiscountCouponRedemptions.Add(new DiscountCouponRedemption
        {
            CouponId = coupon.Id,
            TrainerId = sub.TrainerId,
            SubscriptionId = sub.Id,
            PaymentId = payment.Id,
            RedeemedAt = DateTime.UtcNow,
            DiscountAmountInCents = sub.CouponDiscountAmountInCents
        });
        coupon.CurrentUses += 1;
        coupon.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Cupom resgatado. Coupon={CouponCode} SubscriptionId={SubscriptionId} DescontoCentavos={Discount}",
            coupon.Code, sub.Id, sub.CouponDiscountAmountInCents);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Onboarding
    // ═══════════════════════════════════════════════════════════════════════════

    private async Task AdvanceOnboardingAsync(TrainerSubscription sub, CancellationToken ct)
    {
        if (!sub.TrainerOnboardingId.HasValue) return;

        var onboarding = await _db.TrainerOnboardings
            .FirstOrDefaultAsync(x => x.Id == sub.TrainerOnboardingId.Value, ct);
        if (onboarding is null) return;
        if (onboarding.Status is OnboardingStatus.Canceled or OnboardingStatus.Completed) return;

        TryAdvanceStatus(onboarding, OnboardingStatus.PaymentApproved);
        onboarding.UpdatedAt = DateTime.UtcNow;
        onboarding.CreatedTrainerId ??= sub.TrainerId;

        if (!onboarding.CreatedUserId.HasValue)
        {
            var trainer = await _db.Trainers.FirstOrDefaultAsync(x => x.Id == sub.TrainerId, ct);
            if (trainer is not null) onboarding.CreatedUserId = trainer.UserId;
        }

        if (onboarding.CreatedUserId.HasValue)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == onboarding.CreatedUserId.Value, ct);
            if (user is not null && !user.IsActive)
            {
                user.IsActive = true;
                user.UpdatedAt = DateTime.UtcNow;
                await _passwordSetupService.GenerateAndSendSetupTokenAsync(user, "FitPlatform", isStudent: false);
            }
        }

        TryAdvanceStatus(onboarding, OnboardingStatus.AccountCreated);
        TryAdvanceStatus(onboarding, OnboardingStatus.Completed);

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Onboarding avançado. OnboardingId={OnboardingId} NovoStatus={Status} SubscriptionId={SubscriptionId}",
            onboarding.Id, onboarding.Status, sub.Id);
    }

    private static void TryAdvanceStatus(TrainerOnboarding o, OnboardingStatus target)
    {
        if (OnboardingRank(target) > OnboardingRank(o.Status)) o.Status = target;
    }

    private static int OnboardingRank(OnboardingStatus s) => s switch
    {
        OnboardingStatus.Draft => 0,
        OnboardingStatus.WaitingPayment => 1,
        OnboardingStatus.PaymentApproved => 2,
        OnboardingStatus.AccountCreated => 3,
        OnboardingStatus.Completed => 4,
        OnboardingStatus.Canceled => -1,
        _ => 0
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // Helpers
    // ═══════════════════════════════════════════════════════════════════════════

    private async Task<bool> HasApprovedPaymentAsync(Guid subscriptionId, string? providerPaymentId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(providerPaymentId)) return false;

        return await _db.TrainerPayments.AnyAsync(
            x => x.TrainerSubscriptionId == subscriptionId
                 && x.Provider == "AbacatePay"
                 && x.ProviderPaymentId == providerPaymentId
                 && x.Status == PaymentStatus.Approved,
            ct);
    }

    private async Task<TrainerPayment> ResolvePaymentForActivationAsync(
        TrainerSubscription sub, SubscriptionPaymentContext ctx, CancellationToken ct)
    {
        // Reaproveita o registro Pending criado no checkout, se existir.
        var payment = await _db.TrainerPayments
            .Where(x => x.TrainerSubscriptionId == sub.Id && x.Provider == "AbacatePay")
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (payment is not null) return payment;

        payment = new TrainerPayment
        {
            TrainerId = sub.TrainerId,
            TrainerSubscriptionId = sub.Id,
            Amount = ResolveAmount(ctx.AmountInCents, sub.FinalAmountInCents),
            Provider = "AbacatePay",
            ProviderPaymentId = ctx.ProviderPaymentId,
            ProviderSubscriptionId = ctx.ProviderSubscriptionId ?? sub.AbacatePaySubscriptionId,
            AbacatePayCheckoutId = ctx.ProviderCheckoutId ?? sub.AbacatePayCheckoutId
        };
        _db.TrainerPayments.Add(payment);
        return payment;
    }

    private async Task<TrainerPayment?> ResolvePaymentForFailureAsync(
        TrainerSubscription sub, string? providerPaymentId, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(providerPaymentId))
        {
            var exact = await _db.TrainerPayments.FirstOrDefaultAsync(
                x => x.TrainerSubscriptionId == sub.Id
                     && x.Provider == "AbacatePay"
                     && x.ProviderPaymentId == providerPaymentId,
                ct);
            if (exact is not null) return exact;
        }

        return await _db.TrainerPayments
            .Where(x => x.TrainerSubscriptionId == sub.Id
                        && x.Provider == "AbacatePay"
                        && x.Status == PaymentStatus.Pending)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    private static void ApplyProviderIdentifiers(TrainerSubscription sub, SubscriptionPaymentContext ctx)
    {
        if (!string.IsNullOrWhiteSpace(ctx.ProviderSubscriptionId)) sub.AbacatePaySubscriptionId ??= ctx.ProviderSubscriptionId;
        if (!string.IsNullOrWhiteSpace(ctx.ProviderCustomerId)) sub.AbacatePayCustomerId ??= ctx.ProviderCustomerId;
        if (!string.IsNullOrWhiteSpace(ctx.ProviderCheckoutId)) sub.AbacatePayCheckoutId ??= ctx.ProviderCheckoutId;
    }

    private static decimal ResolveAmount(int? amountInCents, int fallbackInCents) =>
        amountInCents.HasValue && amountInCents.Value > 0
            ? amountInCents.Value / 100m
            : fallbackInCents / 100m;

    /// <summary>Data-fim do ciclo a partir de agora.</summary>
    public static DateTime CalculateCycleEndDate(BillingFrequency cycle) => cycle switch
    {
        BillingFrequency.Semiannual => DateTime.UtcNow.AddMonths(6),
        BillingFrequency.Yearly => DateTime.UtcNow.AddMonths(12),
        _ => DateTime.UtcNow.AddMonths(1)
    };
}
