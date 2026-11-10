using FitPlatform.Application.Configuration;
using FitPlatform.Application.DTOs.Payment;
using FitPlatform.Application.Exceptions;
using FitPlatform.Application.Interfaces;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitPlatform.Infrastructure.Services;

/// <summary>Resumo de uma execução da reconciliação. Usado em log e em testes.</summary>
public sealed class ReconciliationReport
{
    public int SubscriptionsInspected { get; set; }
    public int SubscriptionsActivated { get; set; }
    public int SubscriptionsCanceled { get; set; }
    public int SubscriptionsExpired { get; set; }
    public int SubscriptionsUnmatched { get; set; }

    /// <summary>Assinaturas ativas conferidas contra o provedor.</summary>
    public int ActiveSubscriptionsInspected { get; set; }

    /// <summary>
    /// Divergências observadas mas deliberadamente não corrigidas por falta de
    /// informação confiável — apenas registradas em log para investigação.
    /// </summary>
    public int DivergencesFlagged { get; set; }

    public int WebhooksRetried { get; set; }
    public int WebhooksRecovered { get; set; }
    public int WebhooksAbandoned { get; set; }
    public bool ProviderUnavailable { get; set; }
}

/// <summary>
/// Reconciliação entre o estado local e o da AbacatePay.
///
/// É a rede de segurança para webhook perdido ou evento não suportado pelo provedor —
/// em particular, NÃO depende de <c>subscription.payment_failed</c>, que não consta no
/// catálogo público de webhooks da AbacatePay.
///
/// Resolve três buracos do fluxo puramente reativo a webhook:
///   1. Assinaturas que ficaram <c>Pending</c> porque o webhook nunca chegou.
///   2. Assinaturas <c>Active</c> localmente cujo estado no provedor divergiu
///      (cancelada, expirada ou estornada lá, ainda ativa aqui).
///   3. Webhooks recebidos e autenticados cujo processamento falhou (<c>Failed</c>).
///
/// Princípio: nunca alterar status de forma especulativa. Só há mutação quando a
/// AbacatePay devolve um status inequívoco; divergências sem informação suficiente
/// são apenas registradas em log estruturado.
///
/// Princípios de contenção — nunca varre tudo:
///   • filtra por Status + janela de datas (MinPendingAgeMinutes .. MaxPendingAgeDays);
///   • limita a quantidade por execução (BatchSize / WebhookRetryBatchSize);
///   • limita tentativas por assinatura (MaxAttemptsPerSubscription) e por webhook
///     (WebhookMaxRetries), marcando o que excede para investigação manual;
///   • limita a paginação do provedor (MaxListPages);
///   • resolve o lote inteiro com uma única varredura paginada, e não uma chamada por assinatura.
/// </summary>
public sealed class SubscriptionReconciliationRunner
{
    private readonly AppDbContext _db;
    private readonly IPaymentProvider _paymentProvider;
    private readonly SubscriptionLifecycleService _lifecycle;
    private readonly IAbacatePayWebhookService _webhookService;
    private readonly AbacatePayOptions _options;
    private readonly ILogger<SubscriptionReconciliationRunner> _logger;

    public SubscriptionReconciliationRunner(
        AppDbContext db,
        IPaymentProvider paymentProvider,
        SubscriptionLifecycleService lifecycle,
        IAbacatePayWebhookService webhookService,
        IOptions<AbacatePayOptions> options,
        ILogger<SubscriptionReconciliationRunner> logger)
    {
        _db = db;
        _paymentProvider = paymentProvider;
        _lifecycle = lifecycle;
        _webhookService = webhookService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ReconciliationReport> RunAsync(CancellationToken cancellationToken = default)
    {
        var report = new ReconciliationReport();

        await ReconcilePendingSubscriptionsAsync(report, cancellationToken);
        await VerifyActiveSubscriptionsAsync(report, cancellationToken);
        await RetryFailedWebhooksAsync(report, cancellationToken);

        _logger.LogInformation(
            "Reconciliação concluída. PendentesInspecionadas={Inspected} Ativadas={Activated} Canceladas={Canceled} " +
            "Expiradas={Expired} SemCorrespondencia={Unmatched} AtivasVerificadas={ActiveInspected} " +
            "DivergenciasSinalizadas={Divergences} WebhooksRetentados={Retried} " +
            "WebhooksRecuperados={Recovered} WebhooksAbandonados={Abandoned}",
            report.SubscriptionsInspected, report.SubscriptionsActivated, report.SubscriptionsCanceled,
            report.SubscriptionsExpired, report.SubscriptionsUnmatched, report.ActiveSubscriptionsInspected,
            report.DivergencesFlagged, report.WebhooksRetried,
            report.WebhooksRecovered, report.WebhooksAbandoned);

        return report;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // 1. Assinaturas pendentes
    // ═══════════════════════════════════════════════════════════════════════════

    private async Task ReconcilePendingSubscriptionsAsync(ReconciliationReport report, CancellationToken ct)
    {
        var settings = _options.Reconciliation;
        var now = DateTime.UtcNow;
        var createdBefore = now.AddMinutes(-settings.MinPendingAgeMinutes);
        var createdAfter = now.AddDays(-settings.MaxPendingAgeDays);

        var candidates = await _db.TrainerSubscriptions
            .Where(x => x.Status == TrainerSubscriptionStatus.Pending
                        && x.Provider == "AbacatePay"
                        && x.CreatedAt <= createdBefore
                        && x.CreatedAt >= createdAfter
                        && x.ReconciliationAttempts < settings.MaxAttemptsPerSubscription)
            .OrderBy(x => x.CreatedAt)
            .Take(settings.BatchSize)
            .ToListAsync(ct);

        if (candidates.Count == 0) return;

        report.SubscriptionsInspected = candidates.Count;

        _logger.LogInformation(
            "Reconciliação iniciada. AssinaturasPendentes={Count} JanelaDe={From:yyyy-MM-dd} JanelaAte={To:yyyy-MM-dd}",
            candidates.Count,
            candidates.Min(x => x.CreatedAt),
            now);

        IReadOnlyList<ProviderSubscriptionDetails> remote;
        try
        {
            remote = await _paymentProvider.ListSubscriptionsAsync(new ProviderSubscriptionListQuery
            {
                // Janela restrita ao intervalo de criação das assinaturas do lote.
                StartDate = DateOnly.FromDateTime(candidates.Min(x => x.CreatedAt).Date.AddDays(-1)),
                EndDate = DateOnly.FromDateTime(now.Date.AddDays(1)),
                Limit = 100,
                MaxPages = settings.MaxListPages
            }, ct);
        }
        catch (PaymentProviderException ex)
        {
            // Indisponibilidade do provedor não deve consumir tentativas nem derrubar o serviço.
            report.ProviderUnavailable = true;
            _logger.LogError(ex, "Reconciliação abortada: provedor indisponível.");
            return;
        }

        var byCheckoutId = new Dictionary<string, ProviderSubscriptionDetails>(StringComparer.Ordinal);
        var byExternalId = new Dictionary<string, ProviderSubscriptionDetails>(StringComparer.Ordinal);

        foreach (var item in remote)
        {
            if (!string.IsNullOrWhiteSpace(item.ProviderCheckoutId))
                byCheckoutId[item.ProviderCheckoutId] = item;
            if (!string.IsNullOrWhiteSpace(item.ExternalId))
                byExternalId[item.ExternalId] = item;
        }

        foreach (var sub in candidates)
        {
            ct.ThrowIfCancellationRequested();

            sub.ReconciliationAttempts += 1;
            sub.LastReconciledAt = DateTime.UtcNow;
            sub.UpdatedAt = DateTime.UtcNow;

            var match = ResolveRemote(sub, byCheckoutId, byExternalId);

            if (match is null)
            {
                report.SubscriptionsUnmatched++;
                _logger.LogWarning(
                    "Reconciliação sem correspondência no provedor. SubscriptionId={SubscriptionId} " +
                    "CheckoutId={CheckoutId} Tentativa={Attempt}/{MaxAttempts}",
                    sub.Id, sub.AbacatePayCheckoutId ?? "N/A", sub.ReconciliationAttempts, settings.MaxAttemptsPerSubscription);

                await _db.SaveChangesAsync(ct);
                continue;
            }

            sub.ProviderStatus = match.Status;

            _logger.LogInformation(
                "Reconciliação: estado remoto obtido. SubscriptionId={SubscriptionId} ProviderStatus={ProviderStatus} " +
                "State={State} Tentativa={Attempt}",
                sub.Id, match.Status, match.State, sub.ReconciliationAttempts);

            switch (match.State)
            {
                case ProviderSubscriptionState.Paid:
                    var activated = await _lifecycle.ActivateAsync(sub, new SubscriptionPaymentContext(
                        ProviderSubscriptionId: string.IsNullOrWhiteSpace(match.ProviderSubscriptionId) ? null : match.ProviderSubscriptionId,
                        ProviderCheckoutId: match.ProviderCheckoutId,
                        ProviderCustomerId: match.CustomerId,
                        ProviderPaymentId: null,
                        AmountInCents: match.PaidAmountInCents ?? match.AmountInCents,
                        RawPayload: match.RawPayload,
                        Source: SubscriptionTransitionSource.Reconciliation), ct);

                    if (activated) report.SubscriptionsActivated++;
                    break;

                case ProviderSubscriptionState.Cancelled:
                    if (await _lifecycle.CancelAsync(sub, SubscriptionTransitionSource.Reconciliation, ct))
                        report.SubscriptionsCanceled++;
                    break;

                case ProviderSubscriptionState.Expired:
                    if (await _lifecycle.MarkExpiredAsync(sub, SubscriptionTransitionSource.Reconciliation, ct))
                        report.SubscriptionsExpired++;
                    break;

                case ProviderSubscriptionState.Refunded:
                    // Estorno após ativação é tratado pelo fluxo de webhook (checkout.refunded);
                    // aqui apenas registramos o status remoto observado.
                    _logger.LogWarning(
                        "Reconciliação: assinatura marcada como estornada no provedor. SubscriptionId={SubscriptionId}", sub.Id);
                    break;

                case ProviderSubscriptionState.Pending:
                case ProviderSubscriptionState.Unknown:
                default:
                    // Continua aguardando — nada a aplicar.
                    break;
            }

            await _db.SaveChangesAsync(ct);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // 2. Assinaturas ativas — detecção de divergência sem depender de payment_failed
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Confere assinaturas já ativas contra o provedor. É o que garante a detecção de
    /// problemas de cobrança mesmo que a AbacatePay nunca envie um evento de falha:
    ///
    ///   • provedor <c>CANCELLED</c> e local <c>Active</c> → cancela localmente
    ///     (preservando EndDate, como faz o webhook);
    ///   • provedor <c>EXPIRED</c> e local <c>Active</c> → marca expirada;
    ///   • provedor <c>REFUNDED</c> → apenas sinaliza (estorno exige decisão humana);
    ///   • provedor <c>PAID</c> mas <c>EndDate</c> local já venceu → renovação não
    ///     processada OU cobrança falhando. Nenhuma das duas é distinguível pelo
    ///     <c>list</c>, então NÃO se altera status: registra-se a divergência.
    ///     O acesso já está corretamente bloqueado, porque
    ///     <c>ActiveTrainerSubscriptionHandler</c> exige <c>EndDate &gt;= agora</c>.
    /// </summary>
    private async Task VerifyActiveSubscriptionsAsync(ReconciliationReport report, CancellationToken ct)
    {
        var settings = _options.Reconciliation;
        if (!settings.VerifyActiveSubscriptions) return;

        var now = DateTime.UtcNow;
        var recheckBefore = now.AddHours(-settings.ActiveRecheckHours);

        var candidates = await _db.TrainerSubscriptions
            .Where(x => x.Status == TrainerSubscriptionStatus.Active
                        && x.Provider == "AbacatePay"
                        && x.AbacatePayCheckoutId != null
                        && (x.LastReconciledAt == null || x.LastReconciledAt <= recheckBefore))
            .OrderBy(x => x.LastReconciledAt)
            .Take(settings.ActiveBatchSize)
            .ToListAsync(ct);

        if (candidates.Count == 0) return;

        report.ActiveSubscriptionsInspected = candidates.Count;

        IReadOnlyList<ProviderSubscriptionDetails> remote;
        try
        {
            remote = await _paymentProvider.ListSubscriptionsAsync(new ProviderSubscriptionListQuery
            {
                StartDate = DateOnly.FromDateTime(candidates.Min(x => x.CreatedAt).Date.AddDays(-1)),
                EndDate = DateOnly.FromDateTime(now.Date.AddDays(1)),
                Limit = 100,
                MaxPages = settings.MaxListPages
            }, ct);
        }
        catch (PaymentProviderException ex)
        {
            report.ProviderUnavailable = true;
            _logger.LogError(ex, "Verificação de assinaturas ativas abortada: provedor indisponível.");
            return;
        }

        var byCheckoutId = new Dictionary<string, ProviderSubscriptionDetails>(StringComparer.Ordinal);
        var byExternalId = new Dictionary<string, ProviderSubscriptionDetails>(StringComparer.Ordinal);

        foreach (var item in remote)
        {
            if (!string.IsNullOrWhiteSpace(item.ProviderCheckoutId))
                byCheckoutId[item.ProviderCheckoutId] = item;
            if (!string.IsNullOrWhiteSpace(item.ExternalId))
                byExternalId[item.ExternalId] = item;
        }

        foreach (var sub in candidates)
        {
            ct.ThrowIfCancellationRequested();

            sub.LastReconciledAt = DateTime.UtcNow;
            sub.UpdatedAt = DateTime.UtcNow;

            var match = ResolveRemote(sub, byCheckoutId, byExternalId);
            if (match is null)
            {
                // Sem informação confiável: não especular sobre uma assinatura ativa.
                _logger.LogWarning(
                    "Assinatura ativa sem correspondência no provedor — nenhuma ação tomada. " +
                    "SubscriptionId={SubscriptionId} TrainerId={TrainerId} CheckoutId={CheckoutId}",
                    sub.Id, sub.TrainerId, sub.AbacatePayCheckoutId ?? "N/A");
                report.DivergencesFlagged++;
                await _db.SaveChangesAsync(ct);
                continue;
            }

            sub.ProviderStatus = match.Status;

            switch (match.State)
            {
                case ProviderSubscriptionState.Cancelled:
                    _logger.LogWarning(
                        "Divergência corrigida: cancelada no provedor e ativa localmente (webhook perdido). " +
                        "SubscriptionId={SubscriptionId} TrainerId={TrainerId} ProviderStatus={ProviderStatus}",
                        sub.Id, sub.TrainerId, match.Status);

                    if (await _lifecycle.CancelAsync(sub, SubscriptionTransitionSource.Reconciliation, ct))
                        report.SubscriptionsCanceled++;
                    break;

                case ProviderSubscriptionState.Expired:
                    _logger.LogWarning(
                        "Divergência corrigida: expirada no provedor e ativa localmente. " +
                        "SubscriptionId={SubscriptionId} TrainerId={TrainerId} ProviderStatus={ProviderStatus}",
                        sub.Id, sub.TrainerId, match.Status);

                    if (await _lifecycle.ForceExpireAsync(sub, SubscriptionTransitionSource.Reconciliation, ct))
                        report.SubscriptionsExpired++;
                    break;

                case ProviderSubscriptionState.Refunded:
                    _logger.LogError(
                        "Divergência sinalizada: pagamento estornado no provedor e assinatura ativa localmente. " +
                        "Requer decisão manual. SubscriptionId={SubscriptionId} TrainerId={TrainerId}",
                        sub.Id, sub.TrainerId);
                    report.DivergencesFlagged++;
                    break;

                case ProviderSubscriptionState.Paid when sub.EndDate < now:
                    // Renovação não processada ou cobrança recorrente falhando. O `list`
                    // devolve o checkout inicial, não os ciclos, então não há como
                    // distinguir — e alterar o EndDate aqui concederia acesso indevido.
                    _logger.LogError(
                        "Divergência sinalizada: período vencido com assinatura ainda ativa. " +
                        "Possível renovação não processada ou cobrança recusada. " +
                        "SubscriptionId={SubscriptionId} TrainerId={TrainerId} EndDate={EndDate:O} " +
                        "ProviderStatus={ProviderStatus} FalhasRegistradas={FailureCount}",
                        sub.Id, sub.TrainerId, sub.EndDate, match.Status, sub.PaymentFailureCount);
                    report.DivergencesFlagged++;
                    break;

                case ProviderSubscriptionState.Paid:
                case ProviderSubscriptionState.Pending:
                case ProviderSubscriptionState.Unknown:
                default:
                    // Coerente com o estado local — nada a fazer.
                    break;
            }

            await _db.SaveChangesAsync(ct);
        }
    }

    private static ProviderSubscriptionDetails? ResolveRemote(
        TrainerSubscription sub,
        IReadOnlyDictionary<string, ProviderSubscriptionDetails> byCheckoutId,
        IReadOnlyDictionary<string, ProviderSubscriptionDetails> byExternalId)
    {
        if (!string.IsNullOrWhiteSpace(sub.AbacatePayCheckoutId)
            && byCheckoutId.TryGetValue(sub.AbacatePayCheckoutId, out var byCheckout))
        {
            return byCheckout;
        }

        var externalId = $"trainer:{sub.TrainerId}:sub:{sub.Id}";
        return byExternalId.TryGetValue(externalId, out var byExternal) ? byExternal : null;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // 3. Retry de webhooks que falharam
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Elegibilidade estrita: apenas <c>Failed</c>, dentro do limite de tentativas e com a
    /// janela de backoff vencida. <c>Processed</c>, <c>Duplicate</c>, <c>Ignored</c>,
    /// <c>IgnoredDevMode</c> e <c>Abandoned</c> são terminais e nunca entram nesta consulta.
    /// </summary>
    private async Task RetryFailedWebhooksAsync(ReconciliationReport report, CancellationToken ct)
    {
        var settings = _options.Reconciliation;
        var now = DateTime.UtcNow;

        var pending = await _db.PaymentWebhookLogs
            .Where(x => x.Provider == "AbacatePay"
                        && x.ProcessingStatus == WebhookProcessingStatus.Failed
                        && x.RetryCount < settings.WebhookMaxRetries
                        && (x.NextRetryAt == null || x.NextRetryAt <= now))
            .OrderBy(x => x.CreatedAt)
            .Take(settings.WebhookRetryBatchSize)
            .Select(x => new { x.Id, x.EventId, x.Type })
            .ToListAsync(ct);

        if (pending.Count == 0) return;

        _logger.LogInformation("Retry de webhooks iniciado. Pendentes={Count}", pending.Count);

        foreach (var item in pending)
        {
            ct.ThrowIfCancellationRequested();
            report.WebhooksRetried++;

            try
            {
                var result = await _webhookService.ReprocessAsync(item.Id, ct);
                report.WebhooksRecovered++;

                _logger.LogInformation(
                    "Retry de webhook bem-sucedido. EventId={EventId} EventType={EventType} Aplicado={Applied}",
                    item.EventId, item.Type, result.Applied);
            }
            catch (Exception ex)
            {
                // ReprocessAsync já persistiu Failed/Abandoned com backoff antes de propagar.
                _logger.LogError(ex,
                    "Retry de webhook falhou novamente. EventId={EventId} EventType={EventType}",
                    item.EventId, item.Type);
            }
        }

        report.WebhooksAbandoned = await _db.PaymentWebhookLogs.CountAsync(
            x => x.Provider == "AbacatePay" && x.ProcessingStatus == WebhookProcessingStatus.Abandoned, ct);
    }
}
