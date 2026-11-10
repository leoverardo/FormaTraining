using System.Text.Json;
using FitPlatform.Application.DTOs.Payment;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Tests.Payments;

/// <summary>
/// Testes da rotina de reconciliação: assinaturas Pending sem webhook e
/// retry de webhooks que falharam.
/// </summary>
public class SubscriptionReconciliationTests : PaymentsTestHarness
{
    /// <summary>Assinatura antiga o bastante para entrar na janela de reconciliação.</summary>
    private Task<TrainerSubscription> CreateStalePendingAsync(string checkoutId) =>
        CreatePendingSubscriptionAsync(checkoutId, createdAt: DateTime.UtcNow.AddHours(-2));

    private static ProviderSubscriptionDetails Remote(
        string checkoutId, ProviderSubscriptionState state, string status, string? externalId = null) => new()
        {
            ProviderCheckoutId = checkoutId,
            ExternalId = externalId,
            Status = status,
            State = state,
            AmountInCents = 5990,
            PaidAmountInCents = state == ProviderSubscriptionState.Paid ? 5990 : null,
            CustomerId = "cust_x",
            RawPayload = "{}"
        };

    // ═══════════════════════════════════════════════════════════════════════════
    // Assinaturas pendentes
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Reconciliacao_ativa_assinatura_pendente_paga_no_provedor()
    {
        var sub = await CreateStalePendingAsync("bill_recon_paid");

        var provider = new FakePaymentProvider();
        provider.RemoteSubscriptions.Add(Remote("bill_recon_paid", ProviderSubscriptionState.Paid, "PAID"));

        var report = await BuildReconciliationRunner(provider).RunAsync();

        Assert.Equal(1, report.SubscriptionsInspected);
        Assert.Equal(1, report.SubscriptionsActivated);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Active, sub.Status);
        Assert.Equal("PAID", sub.ProviderStatus);
        Assert.NotNull(sub.LastReconciledAt);
        Assert.Equal(1, sub.ReconciliationAttempts);

        var payment = await Db.TrainerPayments.FirstAsync(p => p.TrainerSubscriptionId == sub.Id);
        Assert.Equal(PaymentStatus.Approved, payment.Status);
    }

    [Fact]
    public async Task Reconciliacao_resolve_por_externalId_quando_checkoutId_nao_bate()
    {
        var sub = await CreateStalePendingAsync("bill_local_only");

        var provider = new FakePaymentProvider();
        provider.RemoteSubscriptions.Add(Remote(
            "bill_outro", ProviderSubscriptionState.Paid, "PAID",
            externalId: $"trainer:{sub.TrainerId}:sub:{sub.Id}"));

        var report = await BuildReconciliationRunner(provider).RunAsync();

        Assert.Equal(1, report.SubscriptionsActivated);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Active, sub.Status);
    }

    [Fact]
    public async Task Reconciliacao_marca_expirada_quando_provedor_reporta_EXPIRED()
    {
        var sub = await CreateStalePendingAsync("bill_expired");

        var provider = new FakePaymentProvider();
        provider.RemoteSubscriptions.Add(Remote("bill_expired", ProviderSubscriptionState.Expired, "EXPIRED"));

        var report = await BuildReconciliationRunner(provider).RunAsync();

        Assert.Equal(1, report.SubscriptionsExpired);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Expired, sub.Status);
    }

    [Fact]
    public async Task Reconciliacao_mantem_pendente_quando_provedor_ainda_reporta_PENDING()
    {
        var sub = await CreateStalePendingAsync("bill_still_pending");

        var provider = new FakePaymentProvider();
        provider.RemoteSubscriptions.Add(Remote("bill_still_pending", ProviderSubscriptionState.Pending, "PENDING"));

        var report = await BuildReconciliationRunner(provider).RunAsync();

        Assert.Equal(0, report.SubscriptionsActivated);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Pending, sub.Status);
        Assert.Equal(1, sub.ReconciliationAttempts);
    }

    [Fact]
    public async Task Reconciliacao_ignora_assinaturas_recentes()
    {
        // Criada agora: dentro da janela MinPendingAgeMinutes, não deve ser tocada
        // para não competir com o webhook que ainda pode chegar.
        var sub = await CreatePendingSubscriptionAsync("bill_fresh");

        var provider = new FakePaymentProvider();
        provider.RemoteSubscriptions.Add(Remote("bill_fresh", ProviderSubscriptionState.Paid, "PAID"));

        var report = await BuildReconciliationRunner(provider).RunAsync();

        Assert.Equal(0, report.SubscriptionsInspected);
        Assert.Empty(provider.ListCalls);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Pending, sub.Status);
    }

    [Fact]
    public async Task Reconciliacao_respeita_o_limite_de_tentativas_por_assinatura()
    {
        var sub = await CreateStalePendingAsync("bill_maxed");
        sub.ReconciliationAttempts = 8; // == MaxAttemptsPerSubscription padrão
        await Db.SaveChangesAsync();

        var provider = new FakePaymentProvider();
        provider.RemoteSubscriptions.Add(Remote("bill_maxed", ProviderSubscriptionState.Paid, "PAID"));

        var report = await BuildReconciliationRunner(provider).RunAsync();

        Assert.Equal(0, report.SubscriptionsInspected);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Pending, sub.Status);
        Assert.Equal(8, sub.ReconciliationAttempts);
    }

    [Fact]
    public async Task Reconciliacao_respeita_o_tamanho_do_lote()
    {
        for (var i = 0; i < 5; i++)
            await CreateStalePendingAsync($"bill_batch_{i}");

        var provider = new FakePaymentProvider();

        var report = await BuildReconciliationRunner(provider, configureOptions: o => o.Reconciliation.BatchSize = 2)
            .RunAsync();

        Assert.Equal(2, report.SubscriptionsInspected);
    }

    [Fact]
    public async Task Reconciliacao_sem_correspondencia_apenas_incrementa_tentativa()
    {
        var sub = await CreateStalePendingAsync("bill_orphan");

        var provider = new FakePaymentProvider(); // nenhuma assinatura remota

        var report = await BuildReconciliationRunner(provider).RunAsync();

        Assert.Equal(1, report.SubscriptionsUnmatched);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Pending, sub.Status);
        Assert.Equal(1, sub.ReconciliationAttempts);
    }

    [Fact]
    public async Task Provedor_indisponivel_nao_consome_tentativas()
    {
        var sub = await CreateStalePendingAsync("bill_provider_down");

        var provider = new FakePaymentProvider { ListException = ProviderFailure("HTTP 503") };

        var report = await BuildReconciliationRunner(provider).RunAsync();

        Assert.True(report.ProviderUnavailable);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(0, sub.ReconciliationAttempts);
        Assert.Equal(TrainerSubscriptionStatus.Pending, sub.Status);
    }

    [Fact]
    public async Task Reconciliacao_e_idempotente_para_assinatura_ja_ativada()
    {
        var sub = await CreateStalePendingAsync("bill_twice");

        var provider = new FakePaymentProvider();
        provider.RemoteSubscriptions.Add(Remote("bill_twice", ProviderSubscriptionState.Paid, "PAID"));

        await BuildReconciliationRunner(provider).RunAsync();

        // Segunda execução: a assinatura já não é Pending, portanto sai do filtro.
        var second = await BuildReconciliationRunner(provider).RunAsync();
        Assert.Equal(0, second.SubscriptionsInspected);

        var approved = await Db.TrainerPayments.CountAsync(
            p => p.TrainerSubscriptionId == sub.Id && p.Status == PaymentStatus.Approved);
        Assert.Equal(1, approved);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Assinaturas ativas — rede de segurança sem depender de payment_failed
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Assinatura ativa elegível à verificação periódica.</summary>
    private async Task<TrainerSubscription> CreateStaleActiveAsync(string checkoutId, DateTime? endDate = null)
    {
        var sub = await CreateActiveSubscriptionAsync(checkoutId);
        sub.AbacatePayCheckoutId = checkoutId;
        sub.LastReconciledAt = DateTime.UtcNow.AddDays(-2);
        if (endDate.HasValue) sub.EndDate = endDate.Value;
        await Db.SaveChangesAsync();
        return sub;
    }

    [Fact]
    public async Task Ativa_localmente_mas_cancelada_no_provedor_e_corrigida()
    {
        // Cenário: webhook subscription.cancelled nunca chegou.
        var sub = await CreateStaleActiveAsync("bill_ghost_cancel");
        var originalEndDate = sub.EndDate;

        var provider = new FakePaymentProvider();
        provider.RemoteSubscriptions.Add(Remote("bill_ghost_cancel", ProviderSubscriptionState.Cancelled, "CANCELLED"));

        var report = await BuildReconciliationRunner(provider).RunAsync();

        Assert.Equal(1, report.ActiveSubscriptionsInspected);
        Assert.Equal(1, report.SubscriptionsCanceled);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Canceled, sub.Status);
        Assert.Equal("CANCELLED", sub.ProviderStatus);
        // EndDate preservado, como no caminho do webhook.
        Assert.Equal(originalEndDate, sub.EndDate);
    }

    [Fact]
    public async Task Ativa_localmente_mas_expirada_no_provedor_e_corrigida()
    {
        var sub = await CreateStaleActiveAsync("bill_ghost_expired");

        var provider = new FakePaymentProvider();
        provider.RemoteSubscriptions.Add(Remote("bill_ghost_expired", ProviderSubscriptionState.Expired, "EXPIRED"));

        var report = await BuildReconciliationRunner(provider).RunAsync();

        Assert.Equal(1, report.SubscriptionsExpired);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Expired, sub.Status);
    }

    [Fact]
    public async Task Periodo_vencido_com_provedor_PAID_sinaliza_divergencia_sem_alterar_status()
    {
        // É este o cenário que substitui subscription.payment_failed: a cobrança
        // recorrente não entrou, o EndDate venceu e nenhum webhook chegou.
        var sub = await CreateStaleActiveAsync("bill_lapsed", endDate: DateTime.UtcNow.AddDays(-3));

        var provider = new FakePaymentProvider();
        provider.RemoteSubscriptions.Add(Remote("bill_lapsed", ProviderSubscriptionState.Paid, "PAID"));

        var report = await BuildReconciliationRunner(provider).RunAsync();

        Assert.Equal(1, report.DivergencesFlagged);

        // Nada é alterado especulativamente — estender o EndDate daria acesso indevido.
        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Active, sub.Status);
        Assert.True(sub.EndDate < DateTime.UtcNow);

        // O acesso já está bloqueado pela regra Status == Active && EndDate >= now.
        var temAcesso = await Db.TrainerSubscriptions.AnyAsync(
            x => x.TrainerId == sub.TrainerId
                 && x.Status == TrainerSubscriptionStatus.Active
                 && x.EndDate >= DateTime.UtcNow);
        Assert.False(temAcesso);
    }

    [Fact]
    public async Task Estorno_no_provedor_apenas_sinaliza_para_decisao_manual()
    {
        var sub = await CreateStaleActiveAsync("bill_refunded");

        var provider = new FakePaymentProvider();
        provider.RemoteSubscriptions.Add(Remote("bill_refunded", ProviderSubscriptionState.Refunded, "REFUNDED"));

        var report = await BuildReconciliationRunner(provider).RunAsync();

        Assert.Equal(1, report.DivergencesFlagged);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Active, sub.Status);
        Assert.Equal("REFUNDED", sub.ProviderStatus);
    }

    [Fact]
    public async Task Ativa_coerente_com_o_provedor_nao_sofre_alteracao()
    {
        var sub = await CreateStaleActiveAsync("bill_healthy");

        var provider = new FakePaymentProvider();
        provider.RemoteSubscriptions.Add(Remote("bill_healthy", ProviderSubscriptionState.Paid, "PAID"));

        var report = await BuildReconciliationRunner(provider).RunAsync();

        Assert.Equal(1, report.ActiveSubscriptionsInspected);
        Assert.Equal(0, report.DivergencesFlagged);
        Assert.Equal(0, report.SubscriptionsCanceled);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Active, sub.Status);
        Assert.NotNull(sub.LastReconciledAt);
    }

    [Fact]
    public async Task Ativa_sem_correspondencia_no_provedor_nao_e_alterada_especulativamente()
    {
        var sub = await CreateStaleActiveAsync("bill_no_match");

        var provider = new FakePaymentProvider(); // nada remoto

        var report = await BuildReconciliationRunner(provider).RunAsync();

        Assert.Equal(1, report.DivergencesFlagged);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Active, sub.Status);
    }

    [Fact]
    public async Task Verificacao_de_ativas_respeita_a_janela_de_recheck()
    {
        // Verificada há pouco: fora da janela ActiveRecheckHours.
        var sub = await CreateActiveSubscriptionAsync("bill_recent_check");
        sub.LastReconciledAt = DateTime.UtcNow.AddMinutes(-5);
        await Db.SaveChangesAsync();

        var provider = new FakePaymentProvider();
        provider.RemoteSubscriptions.Add(Remote("bill_recent_check", ProviderSubscriptionState.Cancelled, "CANCELLED"));

        var report = await BuildReconciliationRunner(provider).RunAsync();

        Assert.Equal(0, report.ActiveSubscriptionsInspected);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Active, sub.Status);
    }

    [Fact]
    public async Task Verificacao_de_ativas_pode_ser_desligada_por_configuracao()
    {
        var sub = await CreateStaleActiveAsync("bill_disabled");

        var provider = new FakePaymentProvider();
        provider.RemoteSubscriptions.Add(Remote("bill_disabled", ProviderSubscriptionState.Cancelled, "CANCELLED"));

        var report = await BuildReconciliationRunner(
            provider, configureOptions: o => o.Reconciliation.VerifyActiveSubscriptions = false).RunAsync();

        Assert.Equal(0, report.ActiveSubscriptionsInspected);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Active, sub.Status);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Retry de webhooks
    // ═══════════════════════════════════════════════════════════════════════════

    private async Task<PaymentWebhookLog> CreateFailedWebhookLogAsync(
        string eventId, string eventType, object data, int retryCount = 1)
    {
        var raw = JsonSerializer.Serialize(new
        {
            id = eventId,
            @event = eventType,
            apiVersion = 2,
            devMode = false,
            data
        });

        var log = new PaymentWebhookLog
        {
            Provider = "AbacatePay",
            EventId = eventId,
            Type = eventType,
            RawPayload = raw,
            ProcessingStatus = WebhookProcessingStatus.Failed,
            RetryCount = retryCount,
            ErrorMessage = "falha anterior",
            LastAttemptAt = DateTime.UtcNow.AddMinutes(-30),
            NextRetryAt = DateTime.UtcNow.AddMinutes(-1)
        };
        Db.PaymentWebhookLogs.Add(log);
        await Db.SaveChangesAsync();
        return log;
    }

    [Fact]
    public async Task Retry_reprocessa_webhook_que_falhou_e_aplica_o_evento()
    {
        var sub = await CreatePendingSubscriptionAsync("chk_retry");
        var log = await CreateFailedWebhookLogAsync("evt_retry_001", "subscription.completed", new
        {
            checkout = new { id = "chk_retry" },
            payment = new { id = "pay_retry", amount = 5990 }
        });

        var report = await BuildReconciliationRunner(new FakePaymentProvider()).RunAsync();

        Assert.Equal(1, report.WebhooksRetried);
        Assert.Equal(1, report.WebhooksRecovered);

        await Db.Entry(log).ReloadAsync();
        Assert.Equal(WebhookProcessingStatus.Processed, log.ProcessingStatus);
        Assert.Null(log.NextRetryAt);
        Assert.Equal(2, log.RetryCount);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Active, sub.Status);
    }

    [Fact]
    public async Task Retry_nao_reprocessa_antes_da_janela_de_backoff()
    {
        await CreatePendingSubscriptionAsync("chk_backoff");
        var log = await CreateFailedWebhookLogAsync("evt_backoff", "subscription.completed", new
        {
            checkout = new { id = "chk_backoff" }
        });
        log.NextRetryAt = DateTime.UtcNow.AddMinutes(30);
        await Db.SaveChangesAsync();

        var report = await BuildReconciliationRunner(new FakePaymentProvider()).RunAsync();

        Assert.Equal(0, report.WebhooksRetried);

        await Db.Entry(log).ReloadAsync();
        Assert.Equal(WebhookProcessingStatus.Failed, log.ProcessingStatus);
    }

    [Fact]
    public async Task Retry_nao_reprocessa_webhook_que_excedeu_o_limite()
    {
        await CreatePendingSubscriptionAsync("chk_exhausted");
        var log = await CreateFailedWebhookLogAsync("evt_exhausted", "subscription.completed", new
        {
            checkout = new { id = "chk_exhausted" }
        }, retryCount: 5); // == WebhookMaxRetries padrão

        var report = await BuildReconciliationRunner(new FakePaymentProvider()).RunAsync();

        Assert.Equal(0, report.WebhooksRetried);

        await Db.Entry(log).ReloadAsync();
        Assert.Equal(WebhookProcessingStatus.Failed, log.ProcessingStatus);
        Assert.Equal(5, log.RetryCount);
    }

    [Fact]
    public async Task Retry_de_webhook_ja_processado_e_ignorado()
    {
        var sub = await CreatePendingSubscriptionAsync("chk_processed");
        var log = await CreateFailedWebhookLogAsync("evt_processed", "subscription.completed", new
        {
            checkout = new { id = "chk_processed" }
        });
        log.ProcessingStatus = WebhookProcessingStatus.Processed;
        await Db.SaveChangesAsync();

        var report = await BuildReconciliationRunner(new FakePaymentProvider()).RunAsync();

        Assert.Equal(0, report.WebhooksRetried);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Pending, sub.Status);
    }
}
