using System.Data;
using FitPlatform.Application.Common;
using FitPlatform.Application.Configuration;
using FitPlatform.Application.DTOs.Payment;
using FitPlatform.Application.DTOs.Subscription;
using FitPlatform.Application.Exceptions;
using FitPlatform.Application.Interfaces;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitPlatform.Infrastructure.Services;

/// <summary>
/// Regras de negócio de assinatura da plataforma: precificação, cupons, criação de
/// checkout, cancelamento e troca de plano.
///
/// O backend é sempre a fonte da verdade de preço — nenhum valor monetário é aceito
/// do frontend. O provedor executa o que o backend calculou.
///
/// O processamento de webhooks NÃO pertence a este serviço: é responsabilidade
/// exclusiva de <see cref="AbacatePayWebhookService"/>.
/// </summary>
public class PaymentService
{
    private readonly AppDbContext _db;
    private readonly IPaymentProvider _paymentProvider;
    private readonly SubscriptionLifecycleService _lifecycle;
    private readonly AbacatePayOptions _options;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        AppDbContext db,
        IPaymentProvider paymentProvider,
        SubscriptionLifecycleService lifecycle,
        IOptions<AbacatePayOptions> options,
        ILogger<PaymentService> logger)
    {
        _db = db;
        _paymentProvider = paymentProvider;
        _lifecycle = lifecycle;
        _options = options.Value;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Consultas de precificação
    // ═══════════════════════════════════════════════════════════════════════════

    public async Task<ApiResponse<List<PlanBillingOptionResponse>>> GetBillingOptionsAsync(Guid planId, CancellationToken cancellationToken = default)
    {
        var plan = await _db.PlatformPlans.FirstOrDefaultAsync(x => x.Id == planId && x.Active, cancellationToken);
        if (plan == null) return ApiResponse<List<PlanBillingOptionResponse>>.Fail("Plano não encontrado.");
        if (!plan.IsAvailableForPurchase)
            return ApiResponse<List<PlanBillingOptionResponse>>.Fail("Este plano ainda não está disponível para contratação.");

        var list = SupportedBillingCycles.Select(c => Calculate(plan.MonthlyPrice, c)).ToList();
        return ApiResponse<List<PlanBillingOptionResponse>>.Ok(list.Select(x => new PlanBillingOptionResponse
        {
            BillingCycle = x.BillingCycle,
            Months = x.Months,
            BaseAmountInCents = x.BaseAmountInCents,
            CycleDiscountAmountInCents = x.CycleDiscountAmountInCents,
            FinalAmountInCents = x.FinalAmountInCents,
            SavingsPercent = x.CycleDiscountPercent
        }).ToList());
    }

    public async Task<ApiResponse<ValidateCouponResponse>> ValidateCouponAsync(Guid trainerId, ValidateCouponRequest request, CancellationToken cancellationToken = default)
    {
        if (!IsSupportedBillingCycle(request.BillingCycle))
            return ApiResponse<ValidateCouponResponse>.Fail("Ciclo de cobrança inválido. Escolha mensal, semestral ou anual.");

        var plan = await _db.PlatformPlans.FirstOrDefaultAsync(x => x.Id == request.PlanId && x.Active, cancellationToken);
        if (plan == null) return ApiResponse<ValidateCouponResponse>.Fail("Plano não encontrado.");
        if (!plan.IsAvailableForPurchase)
            return ApiResponse<ValidateCouponResponse>.Fail("Este plano ainda não está disponível para contratação.");

        var calc = await CalculateWithCouponAsync(plan, request.BillingCycle, request.CouponCode, trainerId, cancellationToken);
        return ApiResponse<ValidateCouponResponse>.Ok(ToCouponResponse(calc, calc.CouponError ?? "Cupom válido."));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Criação de checkout
    // ═══════════════════════════════════════════════════════════════════════════

    public async Task<ApiResponse<SubscriptionResponse>> CreateSubscriptionAsync(Guid trainerId, CreateSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        if (!IsSupportedBillingCycle(request.BillingCycle))
            return ApiResponse<SubscriptionResponse>.Fail("Ciclo de cobrança inválido. Escolha mensal, semestral ou anual.");

        var trainer = await _db.Trainers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == trainerId, cancellationToken);
        if (trainer == null) return ApiResponse<SubscriptionResponse>.Fail("Trainer não encontrado.");

        var plan = await _db.PlatformPlans.FirstOrDefaultAsync(x => x.Id == request.PlatformPlanId && x.Active, cancellationToken);
        if (plan == null) return ApiResponse<SubscriptionResponse>.Fail("Plano não encontrado.");
        if (!plan.IsAvailableForPurchase)
            return ApiResponse<SubscriptionResponse>.Fail("Este plano ainda não está disponível para contratação.");

        var active = await _db.TrainerSubscriptions.FirstOrDefaultAsync(
            ts => ts.TrainerId == trainerId && ts.Status == TrainerSubscriptionStatus.Active,
            cancellationToken);
        if (active != null) return ApiResponse<SubscriptionResponse>.Fail("Você já possui uma assinatura ativa.");

        var option = await ResolveBillingOptionAsync(plan, request.BillingCycle, cancellationToken);
        if (option == null)
            return ApiResponse<SubscriptionResponse>.Fail("Plano ou ciclo de cobrança indisponível para pagamento. Contate o suporte.");

        var onboarding = await _db.TrainerOnboardings
            .Where(x => x.Email == trainer.User.Email && x.Status == OnboardingStatus.WaitingPayment)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var pricing = await CalculateWithCouponAsync(plan, request.BillingCycle, request.CouponCode, trainerId, cancellationToken);
        if (pricing.CouponError != null) return ApiResponse<SubscriptionResponse>.Fail(pricing.CouponError);

        // Integridade de preço: o produto cadastrado na AbacatePay precisa corresponder ao
        // valor do ciclo calculado pelo backend ANTES do cupom. O desconto do cupom é
        // aplicado pelo próprio provedor (campo `coupons` do checkout), por isso não entra
        // nesta comparação — era exatamente essa confusão que rejeitava cupons legítimos.
        if (pricing.SubtotalAfterCycleDiscountInCents != option.FinalPriceInCents)
        {
            _logger.LogError(
                "Preço divergente do produto AbacatePay. PlanId={PlanId} BillingCycle={BillingCycle} " +
                "CalculadoCentavos={CalculatedCents} ConfiguradoCentavos={ConfiguredCents}",
                plan.Id, request.BillingCycle, pricing.SubtotalAfterCycleDiscountInCents, option.FinalPriceInCents);
            return ApiResponse<SubscriptionResponse>.Fail(
                "O valor do ciclo não corresponde ao produto configurado para este plano. Contate o suporte.");
        }

        // Cupom validado localmente precisa existir no provedor para que o desconto
        // seja efetivamente aplicado na cobrança.
        var couponCodes = new List<string>();
        if (pricing.Coupon is not null && pricing.CouponDiscountAmountInCents > 0)
        {
            var sync = await EnsureProviderCouponAsync(pricing.Coupon, cancellationToken);
            if (!sync.Success)
                return ApiResponse<SubscriptionResponse>.Fail(sync.Message!);

            couponCodes.Add(pricing.Coupon.Code);
        }

        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            var subscription = new TrainerSubscription
            {
                TrainerId = trainerId,
                TrainerOnboardingId = onboarding?.Id,
                PlatformPlanId = plan.Id,
                BillingCycle = request.BillingCycle,
                Status = TrainerSubscriptionStatus.Pending,
                StartDate = DateTime.UtcNow,
                EndDate = SubscriptionLifecycleService.CalculateCycleEndDate(request.BillingCycle),
                Provider = "AbacatePay",
                BaseAmountInCents = pricing.BaseAmountInCents,
                CycleDiscountAmountInCents = pricing.CycleDiscountAmountInCents,
                CouponDiscountAmountInCents = pricing.CouponDiscountAmountInCents,
                FinalAmountInCents = pricing.FinalAmountInCents,
                CouponCodeApplied = pricing.Coupon?.Code,
                LastPaymentStatus = "pending"
            };
            _db.TrainerSubscriptions.Add(subscription);
            await _db.SaveChangesAsync(cancellationToken);

            var metadata = new Dictionary<string, string>
            {
                ["trainerId"] = trainerId.ToString(),
                ["internalPlanId"] = plan.Id.ToString(),
                ["billingCycle"] = request.BillingCycle.ToString(),
                ["trainerSubscriptionId"] = subscription.Id.ToString(),
                ["couponCode"] = pricing.Coupon?.Code ?? string.Empty,
                ["cycleDiscountAmountInCents"] = pricing.CycleDiscountAmountInCents.ToString(),
                ["couponDiscountAmountInCents"] = pricing.CouponDiscountAmountInCents.ToString(),
                ["finalAmountInCents"] = pricing.FinalAmountInCents.ToString()
            };
            if (onboarding != null) metadata["onboardingId"] = onboarding.Id.ToString();

            var (completionUrl, returnUrl) = BuildCheckoutRedirects(onboarding?.Id);

            ProviderSubscriptionCreated providerResult;
            try
            {
                providerResult = await _paymentProvider.CreateSubscriptionAsync(new CreateProviderSubscriptionRequest
                {
                    LocalSubscriptionId = subscription.Id,
                    TrainerId = trainerId,
                    PlanId = plan.Id,
                    PlanName = plan.Name,
                    BillingCycle = request.BillingCycle,
                    AmountInCents = pricing.FinalAmountInCents,
                    PayerEmail = trainer.User.Email,
                    PayerName = trainer.User.Name,
                    Phone = trainer.Phone,
                    CouponCode = pricing.Coupon?.Code,
                    CouponCodes = couponCodes,
                    AbacatePayProductId = option.AbacatePayProductId,
                    ExternalId = $"trainer:{trainerId}:sub:{subscription.Id}",
                    Metadata = metadata,
                    CompletionUrl = completionUrl,
                    ReturnUrl = returnUrl
                }, cancellationToken);
            }
            catch (PaymentProviderException ex)
            {
                await tx.RollbackAsync(cancellationToken);
                _logger.LogError(ex,
                    "Falha do provedor ao criar checkout. TrainerId={TrainerId} PlanId={PlanId} BillingCycle={BillingCycle}",
                    trainerId, plan.Id, request.BillingCycle);
                return ApiResponse<SubscriptionResponse>.Fail(
                    "Não foi possível gerar o checkout no provedor de pagamento. Tente novamente em instantes.");
            }

            subscription.AbacatePayCustomerId = providerResult.ProviderCustomerId;
            subscription.AbacatePayCheckoutId = providerResult.ProviderCheckoutId;
            subscription.AbacatePaySubscriptionId = string.IsNullOrWhiteSpace(providerResult.ProviderSubscriptionId)
                ? null
                : providerResult.ProviderSubscriptionId;
            subscription.InitPoint = providerResult.CheckoutUrl;
            subscription.UpdatedAt = DateTime.UtcNow;

            _db.TrainerPayments.Add(new TrainerPayment
            {
                TrainerId = trainerId,
                TrainerSubscriptionId = subscription.Id,
                Amount = pricing.FinalAmountInCents / 100m,
                Status = PaymentStatus.Pending,
                Provider = "AbacatePay",
                ProviderPaymentId = null, // só é conhecido quando o pagamento ocorre
                ProviderSubscriptionId = subscription.AbacatePaySubscriptionId,
                AbacatePayCheckoutId = providerResult.ProviderCheckoutId,
                RawPayload = providerResult.RawPayload
            });

            await _db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Checkout criado. SubscriptionId={SubscriptionId} TrainerId={TrainerId} PlanId={PlanId} " +
                "BillingCycle={BillingCycle} ValorFinalCentavos={Amount} CupomAplicado={HasCoupon}",
                subscription.Id, trainerId, plan.Id, request.BillingCycle,
                pricing.FinalAmountInCents, pricing.Coupon is not null);

            return ApiResponse<SubscriptionResponse>.Ok(MapSubscriptionResponse(subscription, plan));
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Define para onde a AbacatePay redireciona o cliente. Checkout vinculado a um
    /// onboarding volta à tela de onboarding com o id explícito na URL; checkout feito
    /// pelo painel volta à tela de assinatura do trainer. Retorna nulos quando a base
    /// não está configurada, para o provedor cair no padrão.
    /// </summary>
    private (string? CompletionUrl, string? ReturnUrl) BuildCheckoutRedirects(Guid? onboardingId)
    {
        if (onboardingId is { } id)
        {
            return (
                AppendQuery(_options.SuccessUrl, "onboardingId", id.ToString()),
                AppendQuery(_options.ReturnUrl, "onboardingId", id.ToString()));
        }

        return (
            AppendQuery(_options.TrainerPanelUrl, "checkout", "success"),
            AppendQuery(_options.TrainerPanelUrl, "checkout", "canceled"));
    }

    private static string? AppendQuery(string baseUrl, string key, string value)
    {
        if (string.IsNullOrWhiteSpace(baseUrl)) return null;
        var separator = baseUrl.Contains('?') ? '&' : '?';
        return $"{baseUrl}{separator}{key}={Uri.EscapeDataString(value)}";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Cancelamento
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Cancela a assinatura vigente do trainer autenticado.
    ///
    /// O trainerId vem sempre do token (nunca do corpo da requisição), de modo que
    /// não é possível cancelar a assinatura de outro trainer.
    /// Idempotente: cancelar uma assinatura já cancelada retorna sucesso sem
    /// chamar o provedor novamente.
    /// </summary>
    public async Task<ApiResponse<CancelSubscriptionResponse>> CancelSubscriptionAsync(
        Guid trainerId, CancellationToken cancellationToken = default)
    {
        var subscription = await _db.TrainerSubscriptions
            .Where(x => x.TrainerId == trainerId
                        && (x.Status == TrainerSubscriptionStatus.Active || x.Status == TrainerSubscriptionStatus.Pending))
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription is null)
        {
            // Pode já ter sido cancelada: devolve o estado atual em vez de erro genérico.
            var canceled = await _db.TrainerSubscriptions
                .Where(x => x.TrainerId == trainerId && x.Status == TrainerSubscriptionStatus.Canceled)
                .OrderByDescending(x => x.UpdatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (canceled is not null)
            {
                _logger.LogInformation(
                    "Cancelamento idempotente: assinatura já estava cancelada. SubscriptionId={SubscriptionId} TrainerId={TrainerId}",
                    canceled.Id, trainerId);

                return ApiResponse<CancelSubscriptionResponse>.Ok(new CancelSubscriptionResponse
                {
                    SubscriptionId = canceled.Id,
                    Status = canceled.Status.ToString(),
                    AccessValidUntil = canceled.EndDate,
                    AlreadyCanceled = true
                }, "Assinatura já estava cancelada.");
            }

            return ApiResponse<CancelSubscriptionResponse>.Fail("Nenhuma assinatura ativa encontrada para cancelar.");
        }

        // Só chama o provedor quando existe a assinatura recorrente lá (subs_...).
        // Um checkout ainda Pending não gerou assinatura no provedor.
        if (!string.IsNullOrWhiteSpace(subscription.AbacatePaySubscriptionId))
        {
            try
            {
                await _paymentProvider.CancelSubscriptionAsync(subscription.AbacatePaySubscriptionId, cancellationToken);
            }
            catch (PaymentProviderException ex)
            {
                _logger.LogError(ex,
                    "Falha do provedor ao cancelar assinatura. SubscriptionId={SubscriptionId} TrainerId={TrainerId}",
                    subscription.Id, trainerId);
                return ApiResponse<CancelSubscriptionResponse>.Fail(
                    "Não foi possível cancelar a assinatura no provedor de pagamento. Tente novamente em instantes.");
            }
        }
        else
        {
            _logger.LogInformation(
                "Cancelamento local: assinatura ainda sem id no provedor. SubscriptionId={SubscriptionId} Status={Status}",
                subscription.Id, subscription.Status);
        }

        await _lifecycle.CancelAsync(subscription, SubscriptionTransitionSource.TrainerAction, cancellationToken);

        return ApiResponse<CancelSubscriptionResponse>.Ok(new CancelSubscriptionResponse
        {
            SubscriptionId = subscription.Id,
            Status = subscription.Status.ToString(),
            AccessValidUntil = subscription.EndDate,
            AlreadyCanceled = false
        }, "Assinatura cancelada. O acesso permanece liberado até o fim do período já pago.");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Troca de plano
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Solicita a troca do plano/ciclo da assinatura vigente do trainer autenticado.
    ///
    /// O backend resolve o produto e o preço do destino; o frontend envia apenas o
    /// identificador do plano e, opcionalmente, o ciclo.
    /// A AbacatePay agenda a troca e a aplica no início do próximo ciclo de cobrança —
    /// até lá a assinatura mantém o plano atual e o registro fica em Pending*.
    /// </summary>
    public async Task<ApiResponse<ChangeSubscriptionPlanResponse>> ChangePlanAsync(
        Guid trainerId, ChangeSubscriptionPlanRequest request, CancellationToken cancellationToken = default)
    {
        var subscription = await _db.TrainerSubscriptions
            .Include(x => x.PlatformPlan)
            .Where(x => x.TrainerId == trainerId && x.Status == TrainerSubscriptionStatus.Active)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription is null)
            return ApiResponse<ChangeSubscriptionPlanResponse>.Fail("Nenhuma assinatura ativa encontrada para alterar.");

        if (string.IsNullOrWhiteSpace(subscription.AbacatePaySubscriptionId))
            return ApiResponse<ChangeSubscriptionPlanResponse>.Fail(
                "A assinatura ainda não foi confirmada pelo provedor. Aguarde a confirmação do pagamento para trocar de plano.");

        var targetCycle = request.BillingCycle ?? subscription.BillingCycle;
        if (!IsSupportedBillingCycle(targetCycle))
            return ApiResponse<ChangeSubscriptionPlanResponse>.Fail("Ciclo de cobrança inválido. Escolha mensal, semestral ou anual.");

        var newPlan = await _db.PlatformPlans.FirstOrDefaultAsync(
            x => x.Id == request.PlatformPlanId && x.Active, cancellationToken);
        if (newPlan is null)
            return ApiResponse<ChangeSubscriptionPlanResponse>.Fail("Plano não encontrado.");
        if (!newPlan.IsAvailableForPurchase)
            return ApiResponse<ChangeSubscriptionPlanResponse>.Fail("Este plano ainda não está disponível para contratação.");

        if (subscription.PlatformPlanId == newPlan.Id && subscription.BillingCycle == targetCycle)
            return ApiResponse<ChangeSubscriptionPlanResponse>.Fail("A assinatura já está neste plano e ciclo.");

        var option = await ResolveBillingOptionAsync(newPlan, targetCycle, cancellationToken);
        if (option is null)
            return ApiResponse<ChangeSubscriptionPlanResponse>.Fail(
                "Plano ou ciclo de destino indisponível para pagamento. Contate o suporte.");

        // Preço do destino calculado pelo backend e conferido contra o produto do provedor.
        var targetPricing = Calculate(newPlan.MonthlyPrice, targetCycle);
        if (targetPricing.FinalAmountInCents != option.FinalPriceInCents)
        {
            _logger.LogError(
                "Preço divergente do produto AbacatePay na troca de plano. PlanId={PlanId} BillingCycle={BillingCycle} " +
                "CalculadoCentavos={CalculatedCents} ConfiguradoCentavos={ConfiguredCents}",
                newPlan.Id, targetCycle, targetPricing.FinalAmountInCents, option.FinalPriceInCents);
            return ApiResponse<ChangeSubscriptionPlanResponse>.Fail(
                "O valor do plano de destino não corresponde ao produto configurado. Contate o suporte.");
        }

        var currentPlanName = subscription.PlatformPlan?.Name ?? string.Empty;
        var changeKind = ResolveChangeKind(subscription, newPlan.Id, targetCycle, targetPricing.FinalAmountInCents);

        ProviderPlanChangeResult providerResult;
        try
        {
            providerResult = await _paymentProvider.ChangeSubscriptionPlanAsync(new ChangeProviderSubscriptionPlanRequest
            {
                ProviderSubscriptionId = subscription.AbacatePaySubscriptionId,
                ProductId = option.AbacatePayProductId!,
                Quantity = 1
            }, cancellationToken);
        }
        catch (PaymentProviderException ex)
        {
            _logger.LogError(ex,
                "Falha do provedor na troca de plano. SubscriptionId={SubscriptionId} TrainerId={TrainerId} NovoPlano={NewPlanId}",
                subscription.Id, trainerId, newPlan.Id);
            return ApiResponse<ChangeSubscriptionPlanResponse>.Fail(
                "Não foi possível solicitar a troca de plano no provedor de pagamento. Tente novamente em instantes.");
        }
        catch (NotSupportedException)
        {
            return ApiResponse<ChangeSubscriptionPlanResponse>.Fail(
                "O provedor de pagamento configurado não suporta troca de plano.");
        }

        // A troca só vale a partir do próximo ciclo: registra como pendente.
        subscription.PendingPlatformPlanId = newPlan.Id;
        subscription.PendingBillingCycle = targetCycle;
        subscription.PendingPlanChangeProviderId = providerResult.UpdateId;
        subscription.PendingPlanChangeRequestedAt = providerResult.RequestedAt ?? DateTime.UtcNow;
        subscription.PendingFinalAmountInCents = targetPricing.FinalAmountInCents;
        subscription.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Troca de plano solicitada. SubscriptionId={SubscriptionId} TrainerId={TrainerId} " +
            "PlanoAtual={CurrentPlanId} NovoPlano={NewPlanId} NovoCiclo={NewCycle} Tipo={ChangeKind} " +
            "ProviderUpdateId={ProviderUpdateId} EfetivaEm={EffectiveAt:O}",
            subscription.Id, trainerId, subscription.PlatformPlanId, newPlan.Id, targetCycle, changeKind,
            providerResult.UpdateId ?? "N/A", subscription.EndDate);

        return ApiResponse<ChangeSubscriptionPlanResponse>.Ok(new ChangeSubscriptionPlanResponse
        {
            SubscriptionId = subscription.Id,
            CurrentPlanId = subscription.PlatformPlanId,
            CurrentPlanName = currentPlanName,
            CurrentBillingCycle = subscription.BillingCycle.ToString(),
            CurrentAmountInCents = subscription.FinalAmountInCents,
            NewPlanId = newPlan.Id,
            NewPlanName = newPlan.Name,
            NewBillingCycle = targetCycle.ToString(),
            NewAmountInCents = targetPricing.FinalAmountInCents,
            ChangeKind = changeKind,
            EffectiveAt = subscription.EndDate,
            ProviderStatus = providerResult.Status
        }, "Troca de plano agendada. O novo valor passa a valer no próximo ciclo de cobrança.");
    }

    private static string ResolveChangeKind(
        TrainerSubscription subscription, Guid newPlanId, BillingFrequency newCycle, int newAmountInCents)
    {
        if (subscription.PlatformPlanId == newPlanId && subscription.BillingCycle != newCycle)
            return "CycleChange";

        return newAmountInCents > subscription.FinalAmountInCents ? "Upgrade" : "Downgrade";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Cupons
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Garante que o cupom local exista no provedor com os mesmos parâmetros de desconto.
    ///
    /// A elegibilidade (plano, ciclo, limite por cliente, vigência) continua sendo
    /// decidida exclusivamente pelo backend; o provedor apenas executa a aritmética
    /// do desconto sobre o valor do produto.
    /// </summary>
    private async Task<(bool Success, string? Message)> EnsureProviderCouponAsync(
        DiscountCoupon coupon, CancellationToken cancellationToken)
    {
        if (!_options.EnableCouponSync)
        {
            _logger.LogError(
                "Cupom aplicado mas a sincronização com o provedor está desabilitada. Coupon={CouponCode}", coupon.Code);
            return (false, "Cupons estão temporariamente indisponíveis. Tente novamente sem cupom ou contate o suporte.");
        }

        // Já sincronizado anteriormente: nada a fazer.
        if (coupon.ProviderSyncedAt.HasValue &&
            string.Equals(coupon.ProviderCouponCode, coupon.Code, StringComparison.Ordinal))
        {
            return (true, null);
        }

        var definition = new ProviderCouponDefinition
        {
            Code = coupon.Code,
            DiscountKind = coupon.DiscountType == DiscountType.Percentage ? "PERCENTAGE" : "FIXED",
            Discount = (int)Math.Round(coupon.DiscountValue, MidpointRounding.AwayFromZero),
            MaxRedeems = coupon.MaxUsesTotal ?? -1,
            Notes = coupon.Description,
            StartsAt = coupon.StartsAt,
            ExpiresAt = coupon.ExpiresAt
        };

        try
        {
            var result = await _paymentProvider.EnsureCouponAsync(definition, cancellationToken);

            coupon.ProviderCouponCode = result.Code;
            coupon.ProviderSyncedAt = DateTime.UtcNow;
            coupon.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Cupom sincronizado com o provedor. Coupon={CouponCode} JaExistia={AlreadyExisted}",
                coupon.Code, result.AlreadyExisted);

            return (true, null);
        }
        catch (PaymentProviderException ex)
        {
            // Falhar explicitamente é mais seguro do que cobrar o valor cheio silenciosamente.
            _logger.LogError(ex, "Falha ao sincronizar cupom com o provedor. Coupon={CouponCode}", coupon.Code);
            return (false, "Não foi possível aplicar o cupom no provedor de pagamento. Tente novamente em instantes.");
        }
        catch (NotSupportedException)
        {
            _logger.LogError("Provedor configurado não suporta cupons. Coupon={CouponCode}", coupon.Code);
            return (false, "O provedor de pagamento configurado não suporta cupons.");
        }
    }

    private async Task<PricingSnapshot> CalculateWithCouponAsync(
        PlatformPlan plan, BillingFrequency cycle, string? couponCode, Guid trainerId, CancellationToken cancellationToken)
    {
        var snapshot = Calculate(plan.MonthlyPrice, cycle);
        if (string.IsNullOrWhiteSpace(couponCode)) return snapshot;

        var normalized = couponCode.Trim().ToUpperInvariant();
        var coupon = await _db.DiscountCoupons.FirstOrDefaultAsync(c => c.Code.ToUpper() == normalized, cancellationToken);
        if (coupon == null || !coupon.IsActive) return snapshot with { CouponError = "Cupom inválido." };

        var now = DateTime.UtcNow;
        if (coupon.StartsAt.HasValue && coupon.StartsAt > now) return snapshot with { CouponError = "Cupom fora da vigência." };
        if (coupon.ExpiresAt.HasValue && coupon.ExpiresAt < now) return snapshot with { CouponError = "Cupom expirado." };
        if (coupon.AppliesToPlanId.HasValue && coupon.AppliesToPlanId != plan.Id) return snapshot with { CouponError = "Cupom não aplicável a este plano." };
        if (coupon.AppliesToBillingCycle.HasValue && coupon.AppliesToBillingCycle != cycle) return snapshot with { CouponError = "Cupom não aplicável a este ciclo." };
        if (coupon.MaxUsesTotal.HasValue && coupon.CurrentUses >= coupon.MaxUsesTotal.Value) return snapshot with { CouponError = "Cupom esgotado." };
        if (coupon.MinimumPurchaseAmountInCents.HasValue && snapshot.SubtotalAfterCycleDiscountInCents < coupon.MinimumPurchaseAmountInCents.Value)
            return snapshot with { CouponError = "Valor mínimo não atingido para o cupom." };

        // Guardas de integridade do cadastro do cupom.
        if (coupon.DiscountValue <= 0)
            return snapshot with { CouponError = "Cupom com desconto inválido." };
        if (coupon.DiscountType == DiscountType.Percentage && coupon.DiscountValue > 100m)
            return snapshot with { CouponError = "Cupom com desconto inválido." };

        if (coupon.MaxUsesPerCustomer.HasValue)
        {
            var trainerUses = await _db.DiscountCouponRedemptions.CountAsync(
                x => x.CouponId == coupon.Id && x.TrainerId == trainerId,
                cancellationToken);
            if (trainerUses >= coupon.MaxUsesPerCustomer.Value)
                return snapshot with { CouponError = "Limite de usos por cliente atingido." };
        }

        var couponDiscount = coupon.DiscountType == DiscountType.Percentage
            ? (int)Math.Round(snapshot.SubtotalAfterCycleDiscountInCents * (coupon.DiscountValue / 100m), MidpointRounding.AwayFromZero)
            : (int)Math.Round(coupon.DiscountValue, MidpointRounding.AwayFromZero);

        // Nunca negativo, nunca maior que o subtotal (desconto de 100% resulta em zero, não em valor negativo).
        couponDiscount = Math.Max(0, Math.Min(couponDiscount, snapshot.SubtotalAfterCycleDiscountInCents));

        return snapshot with
        {
            Coupon = coupon,
            CouponDiscountAmountInCents = couponDiscount,
            FinalAmountInCents = snapshot.SubtotalAfterCycleDiscountInCents - couponDiscount
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Helpers
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Resolve a opção de cobrança do plano/ciclo, exigindo produto configurado na AbacatePay.
    /// </summary>
    private async Task<PlanBillingOption?> ResolveBillingOptionAsync(
        PlatformPlan plan, BillingFrequency cycle, CancellationToken cancellationToken)
    {
        var option = await _db.PlanBillingOptions.FirstOrDefaultAsync(
            x => x.PlatformPlanId == plan.Id && x.BillingCycle == cycle && x.IsActive,
            cancellationToken);

        if (option != null && !string.IsNullOrWhiteSpace(option.AbacatePayProductId))
            return option;

        _logger.LogError(
            "Produto AbacatePay não configurado. PlanId={PlanId} PlanName={PlanName} BillingCycle={BillingCycle} " +
            "OptionFound={OptionFound}",
            plan.Id, plan.Name, cycle, option != null);

        return null;
    }

    private static SubscriptionResponse MapSubscriptionResponse(TrainerSubscription subscription, PlatformPlan plan) => new()
    {
        Id = subscription.Id,
        PlanName = plan.Name,
        MonthlyPrice = plan.MonthlyPrice,
        MaxActiveStudents = plan.MaxActiveStudents,
        HasUnlimitedStudents = plan.HasUnlimitedStudents,
        Status = subscription.Status.ToString(),
        BillingCycle = subscription.BillingCycle.ToString(),
        StartDate = subscription.StartDate,
        EndDate = subscription.EndDate,
        CheckoutUrl = subscription.InitPoint,
        AbacatePayCheckoutId = subscription.AbacatePayCheckoutId,
        AbacatePaySubscriptionId = subscription.AbacatePaySubscriptionId
    };

    private static PricingSnapshot Calculate(decimal monthlyPrice, BillingFrequency cycle)
    {
        var months = cycle switch
        {
            BillingFrequency.Semiannual => 6,
            BillingFrequency.Yearly => 12,
            _ => 1
        };
        var cycleDiscount = cycle switch
        {
            BillingFrequency.Semiannual => 15m,
            BillingFrequency.Yearly => 20m,
            _ => 0m
        };
        var baseAmount = (int)Math.Round(monthlyPrice * 100m * months, MidpointRounding.AwayFromZero);
        var cycleAmount = (int)Math.Round(baseAmount * (cycleDiscount / 100m), MidpointRounding.AwayFromZero);
        var subtotal = Math.Max(0, baseAmount - cycleAmount);
        return new PricingSnapshot(cycle, months, cycleDiscount, baseAmount, cycleAmount, subtotal, 0, subtotal, null, null);
    }

    private static readonly BillingFrequency[] SupportedBillingCycles =
    [
        BillingFrequency.Monthly,
        BillingFrequency.Semiannual,
        BillingFrequency.Yearly
    ];

    private static bool IsSupportedBillingCycle(BillingFrequency cycle) =>
        cycle is BillingFrequency.Monthly or BillingFrequency.Semiannual or BillingFrequency.Yearly;

    private static ValidateCouponResponse ToCouponResponse(PricingSnapshot x, string message) => new()
    {
        IsValid = x.CouponError == null,
        Message = x.CouponError ?? message,
        CouponCode = x.Coupon?.Code,
        CycleBaseAmountInCents = x.BaseAmountInCents,
        CycleDiscountAmountInCents = x.CycleDiscountAmountInCents,
        SubtotalAfterCycleDiscountInCents = x.SubtotalAfterCycleDiscountInCents,
        CouponDiscountAmountInCents = x.CouponDiscountAmountInCents,
        FinalAmountInCents = x.FinalAmountInCents
    };

    private sealed record PricingSnapshot(
        BillingFrequency BillingCycle,
        int Months,
        decimal CycleDiscountPercent,
        int BaseAmountInCents,
        int CycleDiscountAmountInCents,
        int SubtotalAfterCycleDiscountInCents,
        int CouponDiscountAmountInCents,
        int FinalAmountInCents,
        DiscountCoupon? Coupon,
        string? CouponError);
}
