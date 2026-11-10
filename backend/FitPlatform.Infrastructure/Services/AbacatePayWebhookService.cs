using System.Data;
using System.Text.Json;
using FitPlatform.Application.Configuration;
using FitPlatform.Application.DTOs.Webhooks;
using FitPlatform.Application.Interfaces;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitPlatform.Infrastructure.Services;

/// <summary>
/// Único processador de webhooks do AbacatePay da plataforma.
///
/// Responsabilidades:
///   1. Validar secret (query param) e assinatura HMAC-SHA256 (header X-Webhook-Signature).
///   2. Aplicar a política de devMode (eventos sandbox não afetam uma instância de produção).
///   3. Garantir idempotência via PaymentWebhookLog (índice único Provider + EventId).
///   4. Resolver a assinatura interna e delegar a transição de estado ao
///      <see cref="SubscriptionLifecycleService"/> — compartilhado com a reconciliação.
///   5. Manter o ciclo do log (Processing → Processed | Failed | Abandoned) com dados de retry.
///
/// Nenhum log inclui secret, assinatura ou payload bruto.
/// </summary>
public sealed class AbacatePayWebhookService : IAbacatePayWebhookService
{
    private const string ProviderName = "AbacatePay";

    private readonly AppDbContext _db;
    private readonly IAbacatePaySignatureValidator _signatureValidator;
    private readonly SubscriptionLifecycleService _lifecycle;
    private readonly AbacatePayOptions _options;
    private readonly ILogger<AbacatePayWebhookService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public AbacatePayWebhookService(
        AppDbContext db,
        IAbacatePaySignatureValidator signatureValidator,
        SubscriptionLifecycleService lifecycle,
        IOptions<AbacatePayOptions> options,
        ILogger<AbacatePayWebhookService> logger)
    {
        _db = db;
        _signatureValidator = signatureValidator;
        _lifecycle = lifecycle;
        _options = options.Value;
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

        // ── 4. Política de devMode ───────────────────────────────────────────
        // Uma instância configurada como produção (DevMode=false) não deve permitir que
        // eventos gerados no sandbox da AbacatePay alterem assinaturas reais.
        if (payload.DevMode && !_options.DevMode)
        {
            await PersistIgnoredDevModeEventAsync(eventId, eventType, rawBody, cancellationToken);
            _logger.LogWarning(
                "Webhook AbacatePay em devMode recebido por instância de produção — registrado e ignorado. " +
                "EventId={EventId} EventType={EventType}", eventId, eventType);
            return WebhookHandlerResult.Accepted("Evento devMode ignorado nesta instância.");
        }

        // ── 5. Registrar o evento ANTES de processar ─────────────────────────
        // O log é gravado e commitado fora da transação de negócio para que uma falha de
        // processamento deixe rastro e possa ser retentada.
        var claim = await ClaimEventAsync(eventId, eventType, rawBody, cancellationToken);

        if (claim.Outcome is EventClaimOutcome.AlreadyTerminal)
        {
            _logger.LogInformation(
                "Duplicate webhook ignored. EventId={EventId} EventType={EventType} ExistingStatus={ExistingStatus}",
                eventId, eventType, claim.ExistingStatus);
            return WebhookHandlerResult.Accepted("Duplicate webhook ignored.");
        }

        if (claim.Outcome is EventClaimOutcome.InFlight)
        {
            // Outra execução (entrega concorrente do mesmo evento) já detém o registro.
            // Responder 200 evita que a AbacatePay retente algo que está sendo tratado.
            _logger.LogInformation(
                "Duplicate webhook ignored: entrega concorrente em processamento. EventId={EventId} EventType={EventType}",
                eventId, eventType);
            return WebhookHandlerResult.Accepted("Duplicate webhook ignored (concurrent delivery).");
        }

        return await ProcessAsync(claim.Log!, payload, rawBody, eventType, cancellationToken);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Reprocessamento interno (retry) — sem revalidar assinatura
    // ═══════════════════════════════════════════════════════════════════════════

    public async Task<WebhookHandlerResult> ReprocessAsync(Guid webhookLogId, CancellationToken cancellationToken = default)
    {
        var log = await _db.PaymentWebhookLogs
            .FirstOrDefaultAsync(x => x.Id == webhookLogId && x.Provider == ProviderName, cancellationToken);

        if (log is null)
            return WebhookHandlerResult.Accepted("Log de webhook não encontrado.");

        // Defesa em profundidade: mesmo que a consulta do runner mude, um estado terminal
        // (Processed, Duplicate, Ignored, IgnoredDevMode, Abandoned) nunca é reprocessado.
        if (IsTerminal(log.ProcessingStatus))
        {
            _logger.LogInformation(
                "Retry recusado: evento em estado terminal. EventId={EventId} Status={Status}",
                log.EventId, log.ProcessingStatus);
            return WebhookHandlerResult.Accepted("Duplicate webhook ignored.");
        }

        // Só eventos que efetivamente falharam são elegíveis a retry.
        if (log.ProcessingStatus != WebhookProcessingStatus.Failed)
        {
            _logger.LogInformation(
                "Retry recusado: evento não está em Failed. EventId={EventId} Status={Status}",
                log.EventId, log.ProcessingStatus);
            return WebhookHandlerResult.Accepted("Evento não elegível a retry.");
        }

        if (log.RetryCount >= _options.Reconciliation.WebhookMaxRetries)
        {
            await AbandonLogAsync(log, "Limite de tentativas excedido.", cancellationToken);
            return WebhookHandlerResult.Accepted("Limite de tentativas excedido — evento abandonado.");
        }

        AbacatePayWebhookPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<AbacatePayWebhookPayload>(log.RawPayload, JsonOpts);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Retry de webhook: payload persistido inválido. EventId={EventId}", log.EventId);
            await AbandonLogAsync(log, "Payload persistido inválido.", cancellationToken);
            return WebhookHandlerResult.Accepted("Payload inválido — retry abandonado.");
        }

        if (payload is null)
        {
            await AbandonLogAsync(log, "Payload persistido nulo.", cancellationToken);
            return WebhookHandlerResult.Accepted("Payload nulo — retry abandonado.");
        }

        log.ProcessingStatus = WebhookProcessingStatus.Processing;
        log.RetryCount++;
        log.LastAttemptAt = DateTime.UtcNow;
        log.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Retry de webhook iniciado. EventId={EventId} EventType={EventType} RetryCount={RetryCount}",
            log.EventId, log.Type, log.RetryCount);

        return await ProcessAsync(log, payload, log.RawPayload, log.Type, cancellationToken);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Processamento transacional
    // ═══════════════════════════════════════════════════════════════════════════

    private async Task<WebhookHandlerResult> ProcessAsync(
        PaymentWebhookLog log,
        AbacatePayWebhookPayload payload,
        string rawBody,
        string eventType,
        CancellationToken cancellationToken)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            var subscription = await ResolveSubscriptionAsync(payload, cancellationToken);

            log.ResourceId =
                ExtractString(payload.Data, "subscription.id")
                ?? ExtractString(payload.Data, "checkout.id")
                ?? ExtractString(payload.Data, "id");

            if (subscription is not null)
            {
                _logger.LogInformation(
                    "Assinatura interna localizada para o evento. EventId={EventId} EventType={EventType} " +
                    "SubscriptionId={SubscriptionId} TrainerId={TrainerId}",
                    log.EventId, eventType, subscription.Id, subscription.TrainerId);
            }

            var outcome = await DispatchAsync(payload, rawBody, subscription, eventType, cancellationToken);

            // Tipo de evento que a plataforma não trata é terminal como Ignored — fica
            // auditável e jamais entra na fila de retry.
            log.ProcessingStatus = outcome == DispatchOutcome.UnsupportedEvent
                ? WebhookProcessingStatus.Ignored
                : WebhookProcessingStatus.Processed;

            log.ProcessedAt = DateTime.UtcNow;
            log.LastAttemptAt = DateTime.UtcNow;
            log.NextRetryAt = null;
            log.ErrorMessage = null;
            log.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            var applied = outcome == DispatchOutcome.Applied;

            _logger.LogInformation(
                "Webhook finalizado. EventId={EventId} EventType={EventType} " +
                "InternalSubscriptionId={SubscriptionId} Status={Status} Applied={Applied}",
                log.EventId, eventType, subscription?.Id.ToString() ?? "não vinculado",
                log.ProcessingStatus, applied);

            return WebhookHandlerResult.Accepted(
                outcome == DispatchOutcome.UnsupportedEvent
                    ? "Evento não tratado — registrado e ignorado."
                    : "Webhook processado.",
                applied);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erro interno ao processar webhook. EventId={EventId} EventType={EventType} RetryCount={RetryCount}",
                log.EventId, eventType, log.RetryCount);

            try
            {
                await tx.RollbackAsync(CancellationToken.None);
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "Falha ao efetuar rollback. EventId={EventId}", log.EventId);
            }

            // O log foi persistido fora desta transação, portanto sobrevive ao rollback
            // e pode ser retentado pela rotina de reconciliação.
            await MarkLogAsFailedAsync(log, ex.Message, CancellationToken.None);

            throw; // Propagar para o controller retornar 500 (AbacatePay retenta).
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Event dispatching
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Resultado do despacho, usado para decidir o status final do log.</summary>
    private enum DispatchOutcome
    {
        /// <summary>Evento conhecido que alterou o domínio.</summary>
        Applied,

        /// <summary>Evento conhecido, porém sem efeito (duplicado, sem assinatura vinculada, já ativo).</summary>
        NoOp,

        /// <summary>Tipo de evento que a plataforma não trata.</summary>
        UnsupportedEvent
    }

    private async Task<DispatchOutcome> DispatchAsync(
        AbacatePayWebhookPayload payload,
        string rawBody,
        TrainerSubscription? sub,
        string eventType,
        CancellationToken ct)
    {
        switch (eventType)
        {
            case "subscription.completed":
                return ToOutcome(await HandleSubscriptionCompletedAsync(sub, payload, rawBody, ct));

            case "subscription.renewed":
                return ToOutcome(await HandleSubscriptionRenewedAsync(sub, payload, rawBody, ct));

            case "subscription.cancelled":
            case "subscription.canceled":
                return ToOutcome(await HandleSubscriptionCancelledAsync(sub, eventType, ct));

            // Falha de cobrança recorrente.
            //
            // Este evento NÃO consta no catálogo público de webhooks da AbacatePay, que
            // documenta subscription.completed / renewed / cancelled. O handler é mantido
            // de forma puramente defensiva — herdado do fluxo legado e tolerante à evolução
            // da API. Se o evento nunca chegar, este ramo não é exercido e nada se perde:
            // a detecção de divergência de cobrança é responsabilidade da reconciliação
            // (SubscriptionReconciliationRunner), que não depende deste evento.
            case "subscription.payment_failed":
            case "subscription.payment.failed":
                return ToOutcome(await HandleSubscriptionPaymentFailedAsync(sub, payload, rawBody, eventType, ct));

            case "checkout.completed":
                return ToOutcome(await HandleCheckoutCompletedAsync(sub, payload, rawBody, ct));

            default:
                // Evento não tratado: log persistido como Ignored e 200 OK — nunca retentado.
                _logger.LogInformation(
                    "Evento AbacatePay não tratado — registrado como Ignored. EventType={EventType}", eventType);
                return DispatchOutcome.UnsupportedEvent;
        }
    }

    private static DispatchOutcome ToOutcome(bool applied) =>
        applied ? DispatchOutcome.Applied : DispatchOutcome.NoOp;

    private async Task<bool> HandleSubscriptionCompletedAsync(
        TrainerSubscription? sub, AbacatePayWebhookPayload payload, string rawBody, CancellationToken ct)
    {
        if (sub is null)
        {
            _logger.LogWarning("subscription.completed: nenhuma assinatura interna encontrada para vincular.");
            return false;
        }

        return await _lifecycle.ActivateAsync(sub, BuildContext(payload, rawBody), ct);
    }

    private async Task<bool> HandleSubscriptionRenewedAsync(
        TrainerSubscription? sub, AbacatePayWebhookPayload payload, string rawBody, CancellationToken ct)
    {
        if (sub is null)
        {
            _logger.LogWarning("subscription.renewed: nenhuma assinatura interna encontrada para vincular.");
            return false;
        }

        return await _lifecycle.RegisterRenewalAsync(sub, BuildContext(payload, rawBody), ct);
    }

    private async Task<bool> HandleSubscriptionCancelledAsync(
        TrainerSubscription? sub, string eventType, CancellationToken ct)
    {
        if (sub is null)
        {
            _logger.LogWarning("{EventType}: nenhuma assinatura interna encontrada para vincular.", eventType);
            return false;
        }

        return await _lifecycle.CancelAsync(sub, SubscriptionTransitionSource.Webhook, ct);
    }

    private async Task<bool> HandleSubscriptionPaymentFailedAsync(
        TrainerSubscription? sub, AbacatePayWebhookPayload payload, string rawBody, string eventType, CancellationToken ct)
    {
        if (sub is null)
        {
            _logger.LogWarning("{EventType}: nenhuma assinatura interna encontrada para vincular.", eventType);
            return false;
        }

        return await _lifecycle.RegisterPaymentFailureAsync(sub, BuildContext(payload, rawBody), ct);
    }

    /// <summary>
    /// Fallback do primeiro pagamento: usado quando checkout.completed chega antes de
    /// (ou no lugar de) subscription.completed.
    /// </summary>
    private async Task<bool> HandleCheckoutCompletedAsync(
        TrainerSubscription? sub, AbacatePayWebhookPayload payload, string rawBody, CancellationToken ct)
    {
        if (sub is null)
        {
            _logger.LogWarning("checkout.completed: nenhuma assinatura interna encontrada para vincular.");
            return false;
        }

        if (sub.Status == TrainerSubscriptionStatus.Active)
        {
            _logger.LogInformation(
                "checkout.completed: assinatura já está ativa — fallback desnecessário. SubscriptionId={SubscriptionId}", sub.Id);
            return false;
        }

        _logger.LogInformation(
            "checkout.completed: ativando assinatura via fallback. SubscriptionId={SubscriptionId}", sub.Id);

        return await _lifecycle.ActivateAsync(sub, BuildContext(payload, rawBody), ct);
    }

    private static SubscriptionPaymentContext BuildContext(AbacatePayWebhookPayload payload, string rawBody) =>
        new(
            ProviderSubscriptionId: ExtractString(payload.Data, "subscription.id"),
            ProviderCheckoutId: ExtractString(payload.Data, "checkout.id") ?? ExtractString(payload.Data, "id"),
            ProviderCustomerId: ExtractString(payload.Data, "customer.id") ?? ExtractString(payload.Data, "customerId"),
            ProviderPaymentId: ExtractString(payload.Data, "payment.id"),
            AmountInCents: ExtractInt(payload.Data, "payment.amount") ?? ExtractInt(payload.Data, "amount"),
            RawPayload: rawBody,
            Source: SubscriptionTransitionSource.Webhook);

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
    // Persistência do log
    // ═══════════════════════════════════════════════════════════════════════════

    private enum EventClaimOutcome
    {
        /// <summary>Esta execução detém o evento e deve processá-lo.</summary>
        Claimed,

        /// <summary>O evento já atingiu um estado terminal — nada a reprocessar.</summary>
        AlreadyTerminal,

        /// <summary>Outra execução concorrente detém o evento neste momento.</summary>
        InFlight
    }

    private sealed record EventClaim(EventClaimOutcome Outcome, PaymentWebhookLog? Log, WebhookProcessingStatus? ExistingStatus);

    /// <summary>
    /// Janela em que um registro em <c>Processing</c> é considerado "em voo" por outra
    /// execução. Passado esse tempo, presume-se que o processo anterior morreu sem
    /// atualizar o status e o evento volta a ser reivindicável.
    /// </summary>
    private static readonly TimeSpan InFlightWindow = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Reivindica o evento para processamento, aplicando a idempotência por
    /// <c>EventId</c> (o "ProviderEventId" da AbacatePay).
    ///
    /// Três barreiras, da mais barata à mais forte:
    ///   1. leitura do registro existente → estado terminal encerra o fluxo;
    ///   2. registro em <c>Processing</c> dentro da janela → entrega concorrente;
    ///   3. índice UNIQUE (Provider, EventId) → última barreira, trata a corrida real
    ///      entre dois INSERTs simultâneos via <see cref="DbUpdateException"/>.
    ///
    /// O commit acontece aqui, fora da transação de negócio, para que o rastro do
    /// webhook sobreviva a um rollback do processamento.
    /// </summary>
    private async Task<EventClaim> ClaimEventAsync(
        string eventId, string eventType, string rawBody, CancellationToken ct)
    {
        var existing = await _db.PaymentWebhookLogs
            .FirstOrDefaultAsync(x => x.Provider == ProviderName && x.EventId == eventId, ct);

        if (existing is not null)
            return await EvaluateExistingAsync(existing, ct);

        var log = new PaymentWebhookLog
        {
            Provider = ProviderName,
            EventId = eventId,
            Type = eventType,
            RawPayload = rawBody,
            ProcessingStatus = WebhookProcessingStatus.Processing,
            LastAttemptAt = DateTime.UtcNow
        };
        _db.PaymentWebhookLogs.Add(log);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Corrida real entre duas entregas simultâneas: o índice único
            // (Provider, EventId) garante que apenas uma linha exista. O perdedor
            // relê o registro vencedor e desiste do processamento.
            _db.Entry(log).State = EntityState.Detached;

            var concurrent = await _db.PaymentWebhookLogs
                .FirstOrDefaultAsync(x => x.Provider == ProviderName && x.EventId == eventId, ct);

            if (concurrent is null) throw; // violação de unicidade de outra natureza

            _logger.LogInformation(
                "Entrega concorrente detectada pelo índice único. EventId={EventId} EventType={EventType}",
                eventId, eventType);

            return await EvaluateExistingAsync(concurrent, ct, cameFromUniqueViolation: true);
        }

        return new EventClaim(EventClaimOutcome.Claimed, log, null);
    }

    /// <summary>Decide o que fazer com um registro que já existe para este evento.</summary>
    private async Task<EventClaim> EvaluateExistingAsync(
        PaymentWebhookLog existing, CancellationToken ct, bool cameFromUniqueViolation = false)
    {
        if (IsTerminal(existing.ProcessingStatus))
            return new EventClaim(EventClaimOutcome.AlreadyTerminal, existing, existing.ProcessingStatus);

        var inFlight = existing.ProcessingStatus == WebhookProcessingStatus.Processing
                       && existing.LastAttemptAt.HasValue
                       && DateTime.UtcNow - existing.LastAttemptAt.Value < InFlightWindow;

        if (inFlight)
        {
            // Não sobrescreve o registro nem processa em paralelo: apenas registra a duplicata.
            if (cameFromUniqueViolation)
            {
                _logger.LogInformation(
                    "Entrega concorrente descartada. EventId={EventId} EventType={EventType}",
                    existing.EventId, existing.Type);
            }
            return new EventClaim(EventClaimOutcome.InFlight, existing, existing.ProcessingStatus);
        }

        // Registro órfão (Processing antigo) ou Failed: reivindicar para nova tentativa.
        existing.ProcessingStatus = WebhookProcessingStatus.Processing;
        existing.RetryCount++;
        existing.LastAttemptAt = DateTime.UtcNow;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new EventClaim(EventClaimOutcome.Claimed, existing, null);
    }

    /// <summary>
    /// Estados a partir dos quais um evento nunca mais é reprocessado — tanto em uma
    /// reentrega do provedor quanto pela rotina de retry.
    /// </summary>
    private static bool IsTerminal(WebhookProcessingStatus status) => status
        is WebhookProcessingStatus.Processed
        or WebhookProcessingStatus.Duplicate
        or WebhookProcessingStatus.Ignored
        or WebhookProcessingStatus.IgnoredDevMode
        or WebhookProcessingStatus.Abandoned;

    private async Task PersistIgnoredDevModeEventAsync(
        string eventId, string eventType, string rawBody, CancellationToken ct)
    {
        try
        {
            var existing = await _db.PaymentWebhookLogs
                .FirstOrDefaultAsync(x => x.Provider == ProviderName && x.EventId == eventId, ct);

            if (existing is not null)
            {
                // Nunca rebaixar um estado terminal já alcançado.
                if (!IsTerminal(existing.ProcessingStatus))
                {
                    existing.ProcessingStatus = WebhookProcessingStatus.IgnoredDevMode;
                    existing.ProcessedAt ??= DateTime.UtcNow;
                    existing.NextRetryAt = null;
                    existing.LastAttemptAt = DateTime.UtcNow;
                    existing.UpdatedAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync(ct);
                }
                return;
            }

            _db.PaymentWebhookLogs.Add(new PaymentWebhookLog
            {
                Provider = ProviderName,
                EventId = eventId,
                Type = eventType,
                RawPayload = rawBody,
                ProcessingStatus = WebhookProcessingStatus.IgnoredDevMode,
                ProcessedAt = DateTime.UtcNow,
                LastAttemptAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Não foi possível registrar evento devMode ignorado. EventId={EventId}", eventId);
        }
    }

    /// <summary>
    /// Registra a falha no log do webhook depois de um rollback do processamento.
    ///
    /// Crítico: a exceção que derrubou a transação normalmente deixou entidades inválidas
    /// rastreadas no <c>ChangeTracker</c> (por exemplo, o pagamento cuja gravação falhou).
    /// Salvar sem limpar reenviaria essas mesmas entidades e a atualização do log falharia
    /// de novo — apagando exatamente o rastro que precisamos preservar.
    /// Por isso o rastreamento é descartado e o log é relido do banco antes da escrita.
    /// </summary>
    private async Task MarkLogAsFailedAsync(PaymentWebhookLog log, string error, CancellationToken ct)
    {
        var logId = log.Id;
        var eventId = log.EventId;

        try
        {
            _db.ChangeTracker.Clear();

            var fresh = await _db.PaymentWebhookLogs.FirstOrDefaultAsync(x => x.Id == logId, ct);
            if (fresh is null)
            {
                _logger.LogError(
                    "Log do webhook não encontrado ao registrar falha. EventId={EventId}", eventId);
                return;
            }

            var maxRetries = _options.Reconciliation.WebhookMaxRetries;
            var backoffMinutes = Math.Max(1, _options.Reconciliation.WebhookRetryBackoffMinutes);

            fresh.ErrorMessage = Sanitize(error);
            fresh.LastAttemptAt = DateTime.UtcNow;
            fresh.UpdatedAt = DateTime.UtcNow;

            if (fresh.RetryCount >= maxRetries)
            {
                fresh.ProcessingStatus = WebhookProcessingStatus.Abandoned;
                fresh.NextRetryAt = null;
                _logger.LogError(
                    "Webhook abandonado após exceder o limite de tentativas. EventId={EventId} EventType={EventType} RetryCount={RetryCount} MaxRetries={MaxRetries}",
                    fresh.EventId, fresh.Type, fresh.RetryCount, maxRetries);
            }
            else
            {
                fresh.ProcessingStatus = WebhookProcessingStatus.Failed;
                // Backoff exponencial limitado (5, 10, 20, 40... minutos).
                var delay = backoffMinutes * Math.Pow(2, Math.Min(fresh.RetryCount, 6));
                fresh.NextRetryAt = DateTime.UtcNow.AddMinutes(delay);
                _logger.LogWarning(
                    "Webhook marcado para retry. EventId={EventId} EventType={EventType} RetryCount={RetryCount} NextRetryAt={NextRetryAt:O}",
                    fresh.EventId, fresh.Type, fresh.RetryCount, fresh.NextRetryAt);
            }

            await _db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Não foi possível atualizar log para Failed. EventId={EventId}", eventId);
        }
    }

    /// <summary>
    /// Trunca a mensagem de erro para o tamanho da coluna. A mensagem vem de exceções
    /// internas (EF/SQL) e nunca carrega secret, assinatura ou API key — o cliente HTTP
    /// da AbacatePay só lança mensagens com endpoint e status code.
    /// </summary>
    private static string Sanitize(string error) =>
        string.IsNullOrWhiteSpace(error) ? "Erro não especificado."
        : error.Length > 2000 ? error[..2000]
        : error;

    private async Task AbandonLogAsync(PaymentWebhookLog log, string reason, CancellationToken ct)
    {
        log.ProcessingStatus = WebhookProcessingStatus.Abandoned;
        log.ErrorMessage = reason;
        log.NextRetryAt = null;
        log.LastAttemptAt = DateTime.UtcNow;
        log.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Helpers de leitura de JSON
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Extrai um valor string de um JsonElement dado um caminho com pontos.</summary>
    private static string? ExtractString(JsonElement element, string path)
    {
        if (element.ValueKind != JsonValueKind.Object) return null;

        var current = element;
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out current))
                return null;
        }

        return current.ValueKind switch
        {
            JsonValueKind.String => current.GetString(),
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            _ => current.ToString()
        };
    }

    /// <summary>Extrai um valor int de um JsonElement dado um caminho com pontos.</summary>
    private static int? ExtractInt(JsonElement element, string path)
    {
        if (element.ValueKind != JsonValueKind.Object) return null;

        var current = element;
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out current))
                return null;
        }

        return current.ValueKind == JsonValueKind.Number && current.TryGetInt32(out var value) ? value : null;
    }
}
