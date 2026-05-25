using System.Data;
using System.Text.Json;
using FitPlatform.Application.DTOs.Webhooks;
using FitPlatform.Application.Interfaces;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FitPlatform.Infrastructure.Services;

/// <summary>
/// Processa webhooks do AbacatePay de forma segura, idempotente e rastreável.
/// Responsabilidades:
///   1. Validar secret (query param) e assinatura HMAC-SHA256 (header).
///   2. Verificar idempotência via PaymentWebhookLog (Provider + EventId únicos).
///   3. Despachar para o handler correto por tipo de evento.
///   4. Atualizar status do log (Pending → Processing → Processed | Failed).
/// </summary>
public sealed class AbacatePayWebhookService : IAbacatePayWebhookService
{
    private readonly AppDbContext _db;
    private readonly IAbacatePaySignatureValidator _signatureValidator;
    private readonly PasswordSetupService _passwordSetupService;
    private readonly ILogger<AbacatePayWebhookService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public AbacatePayWebhookService(
        AppDbContext db,
        IAbacatePaySignatureValidator signatureValidator,
        PasswordSetupService passwordSetupService,
        ILogger<AbacatePayWebhookService> logger)
    {
        _db = db;
        _signatureValidator = signatureValidator;
        _passwordSetupService = passwordSetupService;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Entry point
    // ═══════════════════════════════════════════════════════════════════════════

    public async Task<WebhookHandlerResult> HandleAsync(
        string rawBody,
        string? signatureHeader,
        string? querySecret,
        bool isDevelopmentEnvironment,
        CancellationToken cancellationToken = default)
    {
        // ── 1. Validar secret na query string ────────────────────────────────
        if (!_signatureValidator.ValidateSecret(querySecret))
        {
            _logger.LogWarning("Webhook AbacatePay rejeitado: secret inválido ou ausente.");
            return WebhookHandlerResult.Unauthorized("Secret inválido ou ausente.");
        }

        // ── 2. Validar assinatura HMAC-SHA256 ────────────────────────────────
        var signatureOk = _signatureValidator.ValidateSignature(rawBody, signatureHeader);

        if (_signatureValidator.IsSignatureValidationEnabled)
        {
            if (!signatureOk)
            {
                if (!isDevelopmentEnvironment)
                {
                    _logger.LogWarning("Webhook AbacatePay rejeitado em produção: assinatura HMAC inválida.");
                    return WebhookHandlerResult.Unauthorized("Assinatura HMAC inválida.");
                }
                // Em Development, logar aviso mas prosseguir (útil para testes locais).
                _logger.LogWarning(
                    "Webhook AbacatePay: assinatura inválida, mas ambiente é Development — prosseguindo com aviso.");
            }
        }
        else
        {
            // EnableSignatureValidation = false — NUNCA aceitar fora de Development.
            if (!isDevelopmentEnvironment)
            {
                _logger.LogError(
                    "EnableSignatureValidation=false detectado em ambiente não-Development. Rejeitando webhook por segurança obrigatória.");
                return WebhookHandlerResult.Unauthorized(
                    "Validação de assinatura é obrigatória fora do ambiente Development.");
            }
            _logger.LogWarning(
                "Validação de assinatura HMAC desabilitada em Development (EnableSignatureValidation=false). " +
                "Webhook aceito sem verificar assinatura.");
        }

        // ── 3. Desserializar payload ─────────────────────────────────────────
        AbacatePayWebhookPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<AbacatePayWebhookPayload>(rawBody, JsonOpts);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Webhook AbacatePay: falha ao desserializar payload.");
            return WebhookHandlerResult.Accepted("Payload inválido — evento ignorado.");
        }

        if (payload is null)
            return WebhookHandlerResult.Accepted("Payload nulo — evento ignorado.");

        var eventId = string.IsNullOrWhiteSpace(payload.Id)
            ? $"unknown-{Guid.NewGuid():N}"
            : payload.Id.Trim();

        var eventType = string.IsNullOrWhiteSpace(payload.Event)
            ? "unknown"
            : payload.Event.Trim();

        _logger.LogInformation(
            "Webhook AbacatePay recebido. EventId={EventId} EventType={EventType} DevMode={DevMode}",
            eventId, eventType, payload.DevMode);

        // ── 4. Idempotência + processamento ──────────────────────────────────
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            var existingLog = await _db.PaymentWebhookLogs
                .FirstOrDefaultAsync(x => x.Provider == "AbacatePay" && x.EventId == eventId, cancellationToken);

            // Evento já processado com sucesso → retornar 200 imediatamente.
            if (existingLog?.ProcessingStatus == WebhookProcessingStatus.Processed)
            {
                _logger.LogInformation(
                    "Webhook duplicado ignorado por idempotência. EventId={EventId}", eventId);
                await tx.CommitAsync(cancellationToken);
                return WebhookHandlerResult.Accepted("Webhook já processado (idempotência).");
            }

            // Criar ou atualizar log para Processing.
            var log = BuildOrUpdateLog(existingLog, eventId, eventType, rawBody);
            if (existingLog is null) _db.PaymentWebhookLogs.Add(log);
            await _db.SaveChangesAsync(cancellationToken);

            // ── 5. Resolver assinatura interna e despachar evento ────────────
            var subscription = await ResolveSubscriptionAsync(payload, cancellationToken);

            log.ResourceId =
                ExtractString(payload.Data, "subscription.id")
                ?? ExtractString(payload.Data, "checkout.id")
                ?? ExtractString(payload.Data, "id");

            await DispatchAsync(payload, rawBody, subscription, eventType, cancellationToken);

            // ── 6. Marcar log como processado ────────────────────────────────
            log.ProcessingStatus = WebhookProcessingStatus.Processed;
            log.ProcessedAt = DateTime.UtcNow;
            log.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Webhook processado com sucesso. EventId={EventId} EventType={EventType} InternalSubscriptionId={SubId}",
                eventId, eventType, subscription?.Id.ToString() ?? "não vinculado");

            return WebhookHandlerResult.Accepted("Webhook processado.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno ao processar webhook. EventId={EventId} EventType={EventType}", eventId, eventType);
            try
            {
                await tx.RollbackAsync(CancellationToken.None);
                await MarkLogAsFailedAsync(eventId, ex.Message, CancellationToken.None);
            }
            catch (Exception inner)
            {
                _logger.LogError(inner, "Falha secundária ao marcar log como Failed. EventId={EventId}", eventId);
            }
            throw; // Propagar para o controller retornar 500 (AbacatePay retentar).
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Event dispatching
    // ═══════════════════════════════════════════════════════════════════════════

    private async Task DispatchAsync(
        AbacatePayWebhookPayload payload,
        string rawBody,
        TrainerSubscription? sub,
        string eventType,
        CancellationToken ct)
    {
        switch (eventType)
        {
            case "subscription.completed":
                await HandleSubscriptionCompletedAsync(sub, payload, rawBody, isFirstCompletion: true, ct);
                break;

            case "subscription.renewed":
                await HandleSubscriptionRenewedAsync(sub, payload, rawBody, ct);
                break;

            case "subscription.cancelled":
                await HandleSubscriptionCancelledAsync(sub, payload, ct);
                break;

            case "checkout.completed":
                await HandleCheckoutCompletedAsync(sub, payload, rawBody, ct);
                break;

            default:
                // Evento desconhecido: salvar log e retornar 200 (não quebrar).
                _logger.LogInformation(
                    "Evento AbacatePay desconhecido salvo sem processamento. EventType={EventType}", eventType);
                break;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // subscription.completed
    // ═══════════════════════════════════════════════════════════════════════════

    private async Task HandleSubscriptionCompletedAsync(
        TrainerSubscription? sub,
        AbacatePayWebhookPayload payload,
        string rawBody,
        bool isFirstCompletion,
        CancellationToken ct)
    {
        if (sub is null)
        {
            _logger.LogWarning("subscription.completed: nenhuma assinatura interna encontrada para vincular.");
            return;
        }

        var now = DateTime.UtcNow;
        var abacateSubId = ExtractString(payload.Data, "subscription.id");
        var checkoutId   = ExtractString(payload.Data, "checkout.id");
        var customerId   = ExtractString(payload.Data, "customer.id")
                        ?? ExtractString(payload.Data, "customerId");
        var paymentId    = ExtractString(payload.Data, "payment.id");
        var amountCents  = ExtractInt(payload.Data, "payment.amount")
                        ?? ExtractInt(payload.Data, "amount");

        // Atualizar assinatura
        sub.Status = TrainerSubscriptionStatus.Active;
        sub.LastPaymentStatus = "paid";
        sub.EndDate = CalculateCycleEndDate(sub.BillingCycle);
        sub.UpdatedAt = now;

        if (!string.IsNullOrWhiteSpace(abacateSubId)) sub.AbacatePaySubscriptionId ??= abacateSubId;
        if (!string.IsNullOrWhiteSpace(customerId))   sub.AbacatePayCustomerId     ??= customerId;
        if (!string.IsNullOrWhiteSpace(checkoutId))   sub.AbacatePayCheckoutId     ??= checkoutId;

        // Resolver ou criar pagamento
        var payment = await _db.TrainerPayments
            .Where(x => x.TrainerSubscriptionId == sub.Id && x.Provider == "AbacatePay")
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (payment is null)
        {
            payment = new TrainerPayment
            {
                TrainerId              = sub.TrainerId,
                TrainerSubscriptionId  = sub.Id,
                Amount                 = amountCents.HasValue ? amountCents.Value / 100m : sub.FinalAmountInCents / 100m,
                Status                 = PaymentStatus.Approved,
                Provider               = "AbacatePay",
                ProviderPaymentId      = paymentId,
                ProviderSubscriptionId = abacateSubId ?? sub.AbacatePaySubscriptionId,
                AbacatePayCheckoutId   = checkoutId   ?? sub.AbacatePayCheckoutId,
                PaidAt                 = now,
                RawPayload             = rawBody
            };
            _db.TrainerPayments.Add(payment);
        }
        else
        {
            payment.Status                 = PaymentStatus.Approved;
            payment.PaidAt               ??= now;
            payment.ProviderPaymentId    ??= paymentId;
            payment.ProviderSubscriptionId ??= abacateSubId ?? sub.AbacatePaySubscriptionId;
            payment.RawPayload             = rawBody;
            payment.UpdatedAt              = now;
        }

        await _db.SaveChangesAsync(ct);

        if (isFirstCompletion)
        {
            await TryRedeemCouponAsync(sub, payment, ct);
            await AdvanceOnboardingAsync(sub, ct);
        }

        _logger.LogInformation(
            "subscription.completed: assinatura ativada. SubscriptionId={SubId} TrainerId={TrainerId} " +
            "AbacateSubId={AbacateSubId} CheckoutId={CheckoutId} PaymentId={PaymentId}",
            sub.Id, sub.TrainerId,
            abacateSubId ?? "N/A",
            checkoutId   ?? "N/A",
            paymentId    ?? "N/A");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // subscription.renewed
    // ═══════════════════════════════════════════════════════════════════════════

    private async Task HandleSubscriptionRenewedAsync(
        TrainerSubscription? sub,
        AbacatePayWebhookPayload payload,
        string rawBody,
        CancellationToken ct)
    {
        if (sub is null)
        {
            _logger.LogWarning("subscription.renewed: nenhuma assinatura interna encontrada para vincular.");
            return;
        }

        var now          = DateTime.UtcNow;
        var abacateSubId = ExtractString(payload.Data, "subscription.id") ?? sub.AbacatePaySubscriptionId;
        var paymentId    = ExtractString(payload.Data, "payment.id");
        var amountCents  = ExtractInt(payload.Data, "payment.amount") ?? ExtractInt(payload.Data, "amount");

        // Idempotência por payment.id: não duplicar pagamento de renovação.
        if (!string.IsNullOrWhiteSpace(paymentId))
        {
            var alreadyRenewed = await _db.TrainerPayments.AnyAsync(
                x => x.TrainerSubscriptionId == sub.Id
                     && x.Provider == "AbacatePay"
                     && x.ProviderPaymentId == paymentId
                     && x.Status == PaymentStatus.Approved,
                ct);

            if (alreadyRenewed)
            {
                _logger.LogInformation(
                    "subscription.renewed: renovação já registrada. PaymentId={PaymentId}", paymentId);
                return;
            }
        }

        // Manter ativo e estender período.
        sub.Status = TrainerSubscriptionStatus.Active;
        sub.LastPaymentStatus = "paid";
        sub.EndDate = CalculateCycleEndDate(sub.BillingCycle);
        sub.UpdatedAt = now;
        if (!string.IsNullOrWhiteSpace(abacateSubId)) sub.AbacatePaySubscriptionId ??= abacateSubId;

        // Novo registro de pagamento para a renovação.
        var renewal = new TrainerPayment
        {
            TrainerId              = sub.TrainerId,
            TrainerSubscriptionId  = sub.Id,
            Amount                 = amountCents.HasValue ? amountCents.Value / 100m : sub.FinalAmountInCents / 100m,
            Status                 = PaymentStatus.Approved,
            Provider               = "AbacatePay",
            ProviderPaymentId      = paymentId,
            ProviderSubscriptionId = abacateSubId,
            AbacatePayCheckoutId   = sub.AbacatePayCheckoutId,
            PaidAt                 = now,
            RawPayload             = rawBody
        };
        _db.TrainerPayments.Add(renewal);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "subscription.renewed: renovação registrada. SubscriptionId={SubId} TrainerId={TrainerId} " +
            "PaymentId={PaymentId} NovaDataFim={EndDate:O}",
            sub.Id, sub.TrainerId, paymentId ?? "N/A", sub.EndDate);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // subscription.cancelled
    // ═══════════════════════════════════════════════════════════════════════════

    private async Task HandleSubscriptionCancelledAsync(
        TrainerSubscription? sub,
        AbacatePayWebhookPayload payload,
        CancellationToken ct)
    {
        if (sub is null)
        {
            _logger.LogWarning("subscription.cancelled: nenhuma assinatura interna encontrada para vincular.");
            return;
        }

        sub.Status = TrainerSubscriptionStatus.Canceled;
        sub.LastPaymentStatus = "cancelled";
        sub.UpdatedAt = DateTime.UtcNow;
        // EndDate é preservado deliberadamente para que o handler de autorização possa
        // conceder acesso gracioso até o fim do período já pago.

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "subscription.cancelled: assinatura cancelada. SubscriptionId={SubId} TrainerId={TrainerId} " +
            "AcessoAte={EndDate:O}",
            sub.Id, sub.TrainerId, sub.EndDate);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // checkout.completed  (fallback para primeiro pagamento)
    // ═══════════════════════════════════════════════════════════════════════════

    private async Task HandleCheckoutCompletedAsync(
        TrainerSubscription? sub,
        AbacatePayWebhookPayload payload,
        string rawBody,
        CancellationToken ct)
    {
        if (sub is null)
        {
            _logger.LogWarning("checkout.completed: nenhuma assinatura interna encontrada para vincular.");
            return;
        }

        // Assinatura já ativa → subscription.completed já foi processado, nada a fazer.
        if (sub.Status == TrainerSubscriptionStatus.Active)
        {
            _logger.LogInformation(
                "checkout.completed: assinatura já está ativa — fallback desnecessário. SubscriptionId={SubId}", sub.Id);
            return;
        }

        // Verificar pagamento duplicado pelo payment.id.
        var paymentId = ExtractString(payload.Data, "payment.id") ?? ExtractString(payload.Data, "id");
        if (!string.IsNullOrWhiteSpace(paymentId))
        {
            var dup = await _db.TrainerPayments.AnyAsync(
                x => x.TrainerSubscriptionId == sub.Id
                     && x.Provider == "AbacatePay"
                     && x.ProviderPaymentId == paymentId
                     && x.Status == PaymentStatus.Approved,
                ct);

            if (dup)
            {
                _logger.LogInformation(
                    "checkout.completed: pagamento já registrado, fallback ignorado. PaymentId={PaymentId}", paymentId);
                return;
            }
        }

        _logger.LogInformation(
            "checkout.completed: ativando assinatura via fallback. SubscriptionId={SubId}", sub.Id);

        await HandleSubscriptionCompletedAsync(sub, payload, rawBody, isFirstCompletion: true, ct);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Resolução de assinatura interna
    // ═══════════════════════════════════════════════════════════════════════════

    private async Task<TrainerSubscription?> ResolveSubscriptionAsync(
        AbacatePayWebhookPayload payload, CancellationToken ct)
    {
        // 1. metadata.trainerSubscriptionId (mais preciso — definido no createSubscription)
        var metaId = ExtractString(payload.Data, "metadata.trainerSubscriptionId");
        if (Guid.TryParse(metaId, out var internalId))
        {
            var s = await _db.TrainerSubscriptions.FirstOrDefaultAsync(x => x.Id == internalId, ct);
            if (s is not null) return s;
        }

        // 2. data.subscription.id → AbacatePaySubscriptionId
        var abacateSubId = ExtractString(payload.Data, "subscription.id");
        if (!string.IsNullOrWhiteSpace(abacateSubId))
        {
            var s = await _db.TrainerSubscriptions
                .Where(x => x.AbacatePaySubscriptionId == abacateSubId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(ct);
            if (s is not null) return s;
        }

        // 3. data.checkout.id ou data.id → AbacatePayCheckoutId
        var checkoutId = ExtractString(payload.Data, "checkout.id") ?? ExtractString(payload.Data, "id");
        if (!string.IsNullOrWhiteSpace(checkoutId))
        {
            var s = await _db.TrainerSubscriptions
                .Where(x => x.AbacatePayCheckoutId == checkoutId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(ct);
            if (s is not null) return s;
        }

        // 4. data.externalId (formato "trainer:{trainerId}:sub:{subscriptionId}")
        var externalId = ExtractString(payload.Data, "externalId");
        if (!string.IsNullOrWhiteSpace(externalId))
        {
            var parts = externalId.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (Guid.TryParse(parts.LastOrDefault(), out var extSubId))
            {
                var s = await _db.TrainerSubscriptions.FirstOrDefaultAsync(x => x.Id == extSubId, ct);
                if (s is not null) return s;
            }
        }

        return null;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Coupon redemption (copiado do PaymentService para manter o service autônomo)
    // ═══════════════════════════════════════════════════════════════════════════

    private async Task TryRedeemCouponAsync(
        TrainerSubscription sub, TrainerPayment payment, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(sub.CouponCodeApplied) || sub.CouponDiscountAmountInCents <= 0)
            return;

        var coupon = await _db.DiscountCoupons.FirstOrDefaultAsync(
            c => c.Code.ToUpper() == sub.CouponCodeApplied!.ToUpper(), ct);
        if (coupon is null) return;

        var alreadyRedeemed = await _db.DiscountCouponRedemptions.AnyAsync(
            r => r.CouponId == coupon.Id && r.TrainerId == sub.TrainerId && r.SubscriptionId == sub.Id, ct);
        if (alreadyRedeemed) return;

        if (coupon.MaxUsesTotal.HasValue && coupon.CurrentUses >= coupon.MaxUsesTotal.Value)
        {
            _logger.LogWarning(
                "Cupom sem saldo no resgate. Coupon={Coupon} SubscriptionId={SubId}", coupon.Code, sub.Id);
            return;
        }

        if (coupon.MaxUsesPerCustomer.HasValue)
        {
            var uses = await _db.DiscountCouponRedemptions
                .CountAsync(x => x.CouponId == coupon.Id && x.TrainerId == sub.TrainerId, ct);
            if (uses >= coupon.MaxUsesPerCustomer.Value)
            {
                _logger.LogWarning(
                    "Limite de cupom por cliente excedido. Coupon={Coupon} TrainerId={TrainerId}",
                    coupon.Code, sub.TrainerId);
                return;
            }
        }

        _db.DiscountCouponRedemptions.Add(new DiscountCouponRedemption
        {
            CouponId              = coupon.Id,
            TrainerId             = sub.TrainerId,
            SubscriptionId        = sub.Id,
            PaymentId             = payment.Id,
            RedeemedAt            = DateTime.UtcNow,
            DiscountAmountInCents = sub.CouponDiscountAmountInCents
        });
        coupon.CurrentUses += 1;
        coupon.UpdatedAt = DateTime.UtcNow;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Avanço de onboarding
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
                await _passwordSetupService.GenerateAndSendSetupTokenAsync(
                    user, "FitPlatform", isStudent: false);
            }
        }

        TryAdvanceStatus(onboarding, OnboardingStatus.AccountCreated);
        TryAdvanceStatus(onboarding, OnboardingStatus.Completed);

        _logger.LogInformation(
            "Onboarding avançado via webhook. OnboardingId={OnboardingId} NovoStatus={Status}",
            onboarding.Id, onboarding.Status);
    }

    private static void TryAdvanceStatus(TrainerOnboarding o, OnboardingStatus target)
    {
        if (OnboardingRank(target) > OnboardingRank(o.Status)) o.Status = target;
    }

    private static int OnboardingRank(OnboardingStatus s) => s switch
    {
        OnboardingStatus.Draft          => 0,
        OnboardingStatus.WaitingPayment => 1,
        OnboardingStatus.PaymentApproved => 2,
        OnboardingStatus.AccountCreated => 3,
        OnboardingStatus.Completed      => 4,
        OnboardingStatus.Canceled       => -1,
        _                               => 0
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // Helpers
    // ═══════════════════════════════════════════════════════════════════════════

    private async Task MarkLogAsFailedAsync(string eventId, string error, CancellationToken ct)
    {
        try
        {
            var log = await _db.PaymentWebhookLogs
                .FirstOrDefaultAsync(x => x.Provider == "AbacatePay" && x.EventId == eventId, ct);
            if (log is null) return;

            log.ProcessingStatus = WebhookProcessingStatus.Failed;
            log.ErrorMessage     = error.Length > 2000 ? error[..2000] : error;
            log.UpdatedAt        = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Não foi possível atualizar log para Failed. EventId={EventId}", eventId);
        }
    }

    private static PaymentWebhookLog BuildOrUpdateLog(
        PaymentWebhookLog? existing, string eventId, string eventType, string rawBody)
    {
        if (existing is not null)
        {
            existing.ProcessingStatus = WebhookProcessingStatus.Processing;
            existing.RetryCount++;
            existing.UpdatedAt = DateTime.UtcNow;
            return existing;
        }

        return new PaymentWebhookLog
        {
            Provider         = "AbacatePay",
            EventId          = eventId,
            Type             = eventType,
            RawPayload       = rawBody,
            ProcessingStatus = WebhookProcessingStatus.Processing
        };
    }

    private static DateTime CalculateCycleEndDate(BillingFrequency cycle) => cycle switch
    {
        BillingFrequency.Semiannual => DateTime.UtcNow.AddMonths(6),
        BillingFrequency.Yearly     => DateTime.UtcNow.AddMonths(12),
        _                           => DateTime.UtcNow.AddMonths(1)
    };

    /// <summary>Extrai um valor string de um JsonElement dado um caminho com pontos.</summary>
    private static string? ExtractString(JsonElement element, string path)
    {
        var current = element;
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!current.TryGetProperty(segment, out current)) return null;
        }
        return current.ValueKind == JsonValueKind.String ? current.GetString() : current.ToString();
    }

    /// <summary>Extrai um valor int de um JsonElement dado um caminho com pontos.</summary>
    private static int? ExtractInt(JsonElement element, string path)
    {
        var current = element;
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!current.TryGetProperty(segment, out current)) return null;
        }
        return current.TryGetInt32(out var value) ? value : null;
    }
}
