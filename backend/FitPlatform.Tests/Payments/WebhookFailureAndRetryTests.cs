using System.Text.Json;
using FitPlatform.Application.Interfaces;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Tests.Payments;

/// <summary>
/// Testes de sobrevivência do log a rollback, elegibilidade de retry e
/// não-retentativa de estados terminais.
/// </summary>
public class WebhookFailureAndRetryTests : PaymentsTestHarness
{
    private static string MakePayload(string eventId, string eventType, object? data = null, bool devMode = false)
        => JsonSerializer.Serialize(new
        {
            id = eventId,
            @event = eventType,
            apiVersion = 2,
            devMode,
            data = data ?? new { }
        });

    // ═══════════════════════════════════════════════════════════════════════════
    // Log sobrevive a rollback
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Cria uma assinatura órfã (TrainerId sem Trainer correspondente) desligando
    /// temporariamente a checagem de FK do SQLite. Com a FK religada, a gravação do
    /// TrainerPayment durante o processamento estoura — que é o ponto que queremos
    /// exercitar, e não a inserção do fixture.
    /// </summary>
    private async Task CreateOrphanSubscriptionAsync(string checkoutId)
    {
        var plan = await CreatePlanAsync();

        await Db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = OFF;");
        try
        {
            Db.TrainerSubscriptions.Add(new TrainerSubscription
            {
                TrainerId = Guid.NewGuid(), // sem Trainer correspondente
                PlatformPlanId = plan.Id,
                Status = TrainerSubscriptionStatus.Pending,
                BillingCycle = BillingFrequency.Monthly,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Provider = "AbacatePay",
                FinalAmountInCents = 5990,
                AbacatePayCheckoutId = checkoutId
            });
            await Db.SaveChangesAsync();
        }
        finally
        {
            await Db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;");
        }

        Db.ChangeTracker.Clear();
    }

    [Fact]
    public async Task Falha_no_processamento_preserva_o_log_e_marca_Failed()
    {
        var svc = BuildWebhookService();
        await CreateOrphanSubscriptionAsync("chk_boom");

        var raw = MakePayload("log_boom", "subscription.completed", new
        {
            checkout = new { id = "chk_boom" },
            payment = new { id = "pay_boom", amount = 5990 }
        });

        await Assert.ThrowsAnyAsync<Exception>(() => svc.HandleAsync(raw, null, "s", true));

        // O rastro do webhook NÃO pode desaparecer com o rollback do negócio.
        var log = await Db.PaymentWebhookLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.EventId == "log_boom");

        Assert.NotNull(log);
        Assert.Equal(WebhookProcessingStatus.Failed, log.ProcessingStatus);
        Assert.False(string.IsNullOrWhiteSpace(log.ErrorMessage));
        Assert.NotNull(log.LastAttemptAt);
        Assert.NotNull(log.NextRetryAt);          // agendado para retry
        Assert.Equal(0, log.RetryCount);          // primeira tentativa
    }

    [Fact]
    public async Task Falhas_sucessivas_incrementam_RetryCount_e_afastam_o_NextRetryAt()
    {
        var svc = BuildWebhookService();
        await CreateOrphanSubscriptionAsync("chk_boom2");

        var raw = MakePayload("log_boom2", "subscription.completed", new
        {
            checkout = new { id = "chk_boom2" },
            payment = new { id = "pay_boom2", amount = 5990 }
        });

        await Assert.ThrowsAnyAsync<Exception>(() => svc.HandleAsync(raw, null, "s", true));

        var afterFirst = await Db.PaymentWebhookLogs.AsNoTracking().FirstAsync(x => x.EventId == "log_boom2");
        var firstNextRetry = afterFirst.NextRetryAt;

        // Reentrega do mesmo evento (ainda Failed, fora da janela in-flight).
        var log = await Db.PaymentWebhookLogs.FirstAsync(x => x.EventId == "log_boom2");
        log.LastAttemptAt = DateTime.UtcNow.AddHours(-1);
        await Db.SaveChangesAsync();

        await Assert.ThrowsAnyAsync<Exception>(() => svc.HandleAsync(raw, null, "s", true));

        var afterSecond = await Db.PaymentWebhookLogs.AsNoTracking().FirstAsync(x => x.EventId == "log_boom2");
        Assert.Equal(WebhookProcessingStatus.Failed, afterSecond.ProcessingStatus);
        Assert.Equal(1, afterSecond.RetryCount);
        Assert.True(afterSecond.NextRetryAt > firstNextRetry, "backoff deve crescer a cada falha");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Elegibilidade de retry
    // ═══════════════════════════════════════════════════════════════════════════

    private async Task<PaymentWebhookLog> AddLogAsync(
        string eventId, WebhookProcessingStatus status, int retryCount = 0, DateTime? nextRetryAt = null)
    {
        var log = new PaymentWebhookLog
        {
            Provider = "AbacatePay",
            EventId = eventId,
            Type = "subscription.completed",
            RawPayload = MakePayload(eventId, "subscription.completed",
                new { checkout = new { id = "chk_retry_elig" } }),
            ProcessingStatus = status,
            RetryCount = retryCount,
            NextRetryAt = nextRetryAt ?? DateTime.UtcNow.AddMinutes(-1),
            LastAttemptAt = DateTime.UtcNow.AddMinutes(-30)
        };
        Db.PaymentWebhookLogs.Add(log);
        await Db.SaveChangesAsync();
        return log;
    }

    [Theory]
    [InlineData(WebhookProcessingStatus.Processed)]
    [InlineData(WebhookProcessingStatus.Ignored)]
    [InlineData(WebhookProcessingStatus.IgnoredDevMode)]
    [InlineData(WebhookProcessingStatus.Abandoned)]
    [InlineData(WebhookProcessingStatus.Duplicate)]
    public async Task Retry_nunca_seleciona_estados_terminais(WebhookProcessingStatus terminalStatus)
    {
        await CreatePendingSubscriptionAsync("chk_retry_elig");
        await AddLogAsync($"log_elig_{(int)terminalStatus}", terminalStatus);

        var report = await BuildReconciliationRunner(new FakePaymentProvider()).RunAsync();

        Assert.Equal(0, report.WebhooksRetried);
    }

    [Fact]
    public async Task ReprocessAsync_recusa_estado_terminal_mesmo_se_chamado_diretamente()
    {
        var sub = await CreatePendingSubscriptionAsync("chk_retry_elig");
        var log = await AddLogAsync("log_direct_terminal", WebhookProcessingStatus.Abandoned);

        IAbacatePayWebhookService svc = BuildWebhookService();
        var result = await svc.ReprocessAsync(log.Id);

        Assert.False(result.IsUnauthorized);
        Assert.False(result.Applied);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Pending, sub.Status);

        await Db.Entry(log).ReloadAsync();
        Assert.Equal(WebhookProcessingStatus.Abandoned, log.ProcessingStatus);
    }

    [Fact]
    public async Task ReprocessAsync_recusa_estado_nao_Failed()
    {
        await CreatePendingSubscriptionAsync("chk_retry_elig");
        var log = await AddLogAsync("log_direct_processing", WebhookProcessingStatus.Processing);

        IAbacatePayWebhookService svc = BuildWebhookService();
        var result = await svc.ReprocessAsync(log.Id);

        Assert.False(result.Applied);

        await Db.Entry(log).ReloadAsync();
        Assert.Equal(WebhookProcessingStatus.Processing, log.ProcessingStatus);
    }

    [Fact]
    public async Task ReprocessAsync_abandona_quando_limite_de_tentativas_ja_foi_atingido()
    {
        await CreatePendingSubscriptionAsync("chk_retry_elig");
        var log = await AddLogAsync("log_direct_maxed", WebhookProcessingStatus.Failed, retryCount: 5);

        IAbacatePayWebhookService svc = BuildWebhookService();
        await svc.ReprocessAsync(log.Id);

        await Db.Entry(log).ReloadAsync();
        Assert.Equal(WebhookProcessingStatus.Abandoned, log.ProcessingStatus);
        Assert.Null(log.NextRetryAt);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // IgnoredDevMode
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Evento_IgnoredDevMode_nao_altera_assinatura_nem_entra_no_retry()
    {
        var svc = BuildWebhookService(configureOptions: o => o.DevMode = false);
        var sub = await CreatePendingSubscriptionAsync("chk_devmode_retry");

        var result = await svc.HandleAsync(
            MakePayload("log_devmode_retry", "subscription.completed", new
            {
                checkout = new { id = "chk_devmode_retry" },
                payment = new { id = "pay_devmode_retry", amount = 5990 }
            }, devMode: true),
            null, "s", true);

        Assert.False(result.IsUnauthorized);
        Assert.False(result.Applied);

        // Assinatura intocada.
        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Pending, sub.Status);
        Assert.Equal(0, await Db.TrainerPayments.CountAsync(
            p => p.TrainerSubscriptionId == sub.Id && p.Status == PaymentStatus.Approved));

        var log = await Db.PaymentWebhookLogs.AsNoTracking().FirstAsync(x => x.EventId == "log_devmode_retry");
        Assert.Equal(WebhookProcessingStatus.IgnoredDevMode, log.ProcessingStatus);
        Assert.Null(log.NextRetryAt);

        // E não é selecionado pelo retry.
        var report = await BuildReconciliationRunner(new FakePaymentProvider()).RunAsync();
        Assert.Equal(0, report.WebhooksRetried);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Pending, sub.Status);
    }

    [Fact]
    public async Task Reentrega_devMode_nao_rebaixa_um_evento_ja_processado()
    {
        var svc = BuildWebhookService(configureOptions: o => o.DevMode = false);

        Db.PaymentWebhookLogs.Add(new PaymentWebhookLog
        {
            Provider = "AbacatePay",
            EventId = "log_devmode_downgrade",
            Type = "subscription.completed",
            RawPayload = "{}",
            ProcessingStatus = WebhookProcessingStatus.Processed,
            ProcessedAt = DateTime.UtcNow.AddMinutes(-5)
        });
        await Db.SaveChangesAsync();

        await svc.HandleAsync(
            MakePayload("log_devmode_downgrade", "subscription.completed", new { }, devMode: true),
            null, "s", true);

        var log = await Db.PaymentWebhookLogs.AsNoTracking()
            .FirstAsync(x => x.EventId == "log_devmode_downgrade");
        Assert.Equal(WebhookProcessingStatus.Processed, log.ProcessingStatus);
    }
}
