using System.Text.Json;
using FitPlatform.Application.Interfaces;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Tests.Payments;

/// <summary>
/// Testes focados nas DUAS camadas de idempotência e na concorrência de entrega:
///
///   • EventId          → idempotência do EVENTO (PaymentWebhookLog, índice UNIQUE
///                        (Provider, EventId)).
///   • ProviderPaymentId → idempotência da OPERAÇÃO FINANCEIRA (TrainerPayment).
///
/// As duas precisam coexistir: eventos distintos podem citar o mesmo pagamento.
/// </summary>
public class WebhookIdempotencyTests : PaymentsTestHarness
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
    // Camada 1 — idempotência por EventId
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Mesmo_EventId_duas_vezes_executa_negocio_apenas_uma_vez()
    {
        var svc = BuildWebhookService();
        var sub = await CreatePendingSubscriptionAsync("chk_same_event");

        var raw = MakePayload("log_same_event", "subscription.completed", new
        {
            checkout = new { id = "chk_same_event" },
            payment = new { id = "pay_same_event", amount = 5990 }
        });

        var first = await svc.HandleAsync(raw, null, "s", true);
        var second = await svc.HandleAsync(raw, null, "s", true);

        // Primeira aplica; segunda retorna sucesso sem reexecutar a lógica.
        Assert.True(first.Applied);
        Assert.False(second.IsUnauthorized);
        Assert.False(second.Applied);
        Assert.Contains("Duplicate webhook ignored", second.Message, StringComparison.OrdinalIgnoreCase);

        // Uma única linha de log e um único pagamento aprovado.
        Assert.Equal(1, await Db.PaymentWebhookLogs.CountAsync(x => x.EventId == "log_same_event"));
        Assert.Equal(1, await Db.TrainerPayments.CountAsync(
            p => p.TrainerSubscriptionId == sub.Id && p.Status == PaymentStatus.Approved));

        // RetryCount não é incrementado por uma reentrega de evento já terminal.
        var log = await Db.PaymentWebhookLogs.FirstAsync(x => x.EventId == "log_same_event");
        Assert.Equal(WebhookProcessingStatus.Processed, log.ProcessingStatus);
        Assert.Equal(0, log.RetryCount);
    }

    [Fact]
    public async Task Duplicata_de_evento_ja_Processed_retorna_sucesso_sem_reexecutar()
    {
        var svc = BuildWebhookService();
        var sub = await CreateActiveSubscriptionAsync("subs_dup_processed");

        // Log já marcado como Processed, sem nunca ter tocado o domínio.
        Db.PaymentWebhookLogs.Add(new PaymentWebhookLog
        {
            Provider = "AbacatePay",
            EventId = "log_already_processed",
            Type = "subscription.cancelled",
            RawPayload = "{}",
            ProcessingStatus = WebhookProcessingStatus.Processed,
            ProcessedAt = DateTime.UtcNow.AddMinutes(-10)
        });
        await Db.SaveChangesAsync();

        var result = await svc.HandleAsync(
            MakePayload("log_already_processed", "subscription.cancelled",
                new { subscription = new { id = "subs_dup_processed" } }),
            null, "s", true);

        Assert.False(result.IsUnauthorized);
        Assert.False(result.Applied);

        // A assinatura NÃO foi cancelada — a lógica não rodou de novo.
        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Active, sub.Status);
    }

    [Theory]
    [InlineData(WebhookProcessingStatus.Processed)]
    [InlineData(WebhookProcessingStatus.Ignored)]
    [InlineData(WebhookProcessingStatus.IgnoredDevMode)]
    [InlineData(WebhookProcessingStatus.Abandoned)]
    [InlineData(WebhookProcessingStatus.Duplicate)]
    public async Task Reentrega_de_evento_terminal_nunca_reexecuta_a_logica(WebhookProcessingStatus terminalStatus)
    {
        var svc = BuildWebhookService();
        var sub = await CreateActiveSubscriptionAsync($"subs_term_{(int)terminalStatus}");

        Db.PaymentWebhookLogs.Add(new PaymentWebhookLog
        {
            Provider = "AbacatePay",
            EventId = $"log_terminal_{(int)terminalStatus}",
            Type = "subscription.cancelled",
            RawPayload = "{}",
            ProcessingStatus = terminalStatus
        });
        await Db.SaveChangesAsync();

        var result = await svc.HandleAsync(
            MakePayload($"log_terminal_{(int)terminalStatus}", "subscription.cancelled",
                new { subscription = new { id = sub.AbacatePaySubscriptionId } }),
            null, "s", true);

        Assert.False(result.IsUnauthorized);
        Assert.False(result.Applied);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Active, sub.Status);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Camada 2 — idempotência por ProviderPaymentId
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task EventIds_diferentes_com_mesmo_ProviderPaymentId_nao_duplicam_o_pagamento()
    {
        var svc = BuildWebhookService();
        var sub = await CreateActiveSubscriptionAsync("subs_two_events");

        var data = new
        {
            subscription = new { id = "subs_two_events" },
            payment = new { id = "pay_shared_001", amount = 5990 }
        };

        // Dois eventos distintos referenciando o MESMO pagamento.
        var first = await svc.HandleAsync(MakePayload("log_evt_A", "subscription.renewed", data), null, "s", true);
        var second = await svc.HandleAsync(MakePayload("log_evt_B", "subscription.renewed", data), null, "s", true);

        Assert.False(first.IsUnauthorized);
        Assert.False(second.IsUnauthorized);

        // Os DOIS eventos são registrados (camada 1 não os confunde)...
        Assert.Equal(1, await Db.PaymentWebhookLogs.CountAsync(x => x.EventId == "log_evt_A"));
        Assert.Equal(1, await Db.PaymentWebhookLogs.CountAsync(x => x.EventId == "log_evt_B"));

        // ...mas a operação financeira ocorre uma única vez (camada 2).
        Assert.Equal(1, await Db.TrainerPayments.CountAsync(p => p.ProviderPaymentId == "pay_shared_001"));
        Assert.True(first.Applied);
        Assert.False(second.Applied);
    }

    [Fact]
    public async Task EventIds_diferentes_com_pagamentos_diferentes_registram_os_dois()
    {
        var svc = BuildWebhookService();
        var sub = await CreateActiveSubscriptionAsync("subs_two_payments");

        await svc.HandleAsync(MakePayload("log_p1", "subscription.renewed", new
        {
            subscription = new { id = "subs_two_payments" },
            payment = new { id = "pay_cycle_1", amount = 5990 }
        }), null, "s", true);

        await svc.HandleAsync(MakePayload("log_p2", "subscription.renewed", new
        {
            subscription = new { id = "subs_two_payments" },
            payment = new { id = "pay_cycle_2", amount = 5990 }
        }), null, "s", true);

        // Renovações legítimas de ciclos distintos não podem ser suprimidas.
        var approved = await Db.TrainerPayments.CountAsync(
            p => p.TrainerSubscriptionId == sub.Id && p.Status == PaymentStatus.Approved);
        Assert.Equal(2, approved);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Concorrência
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Duas_entregas_simultaneas_do_mesmo_evento_aplicam_o_negocio_uma_vez()
    {
        var sub = await CreatePendingSubscriptionAsync("chk_concurrent");

        var raw = MakePayload("log_concurrent", "subscription.completed", new
        {
            checkout = new { id = "chk_concurrent" },
            payment = new { id = "pay_concurrent", amount = 5990 }
        });

        // Dois serviços independentes sobre a MESMA conexão SQLite, como duas
        // requisições concorrentes atingindo o mesmo banco.
        var svcA = BuildWebhookService();
        var svcB = BuildWebhookService();

        var results = await Task.WhenAll(
            RunGuardedAsync(svcA, raw),
            RunGuardedAsync(svcB, raw));

        // Ao menos uma das duas precisa ter concluído com sucesso.
        Assert.Contains(results, r => r is not null && !r.IsUnauthorized);

        // Exatamente um log e exatamente um pagamento aprovado — sem efeito financeiro duplo.
        Assert.Equal(1, await Db.PaymentWebhookLogs.CountAsync(x => x.EventId == "log_concurrent"));
        Assert.Equal(1, await Db.TrainerPayments.CountAsync(
            p => p.TrainerSubscriptionId == sub.Id && p.Status == PaymentStatus.Approved));

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Active, sub.Status);
    }

    [Fact]
    public async Task Entrega_concorrente_sobre_registro_em_Processing_e_tratada_como_duplicada()
    {
        var svc = BuildWebhookService();
        var sub = await CreatePendingSubscriptionAsync("chk_inflight");

        // Simula outra execução que reivindicou o evento há instantes e ainda não terminou.
        Db.PaymentWebhookLogs.Add(new PaymentWebhookLog
        {
            Provider = "AbacatePay",
            EventId = "log_inflight",
            Type = "subscription.completed",
            RawPayload = "{}",
            ProcessingStatus = WebhookProcessingStatus.Processing,
            LastAttemptAt = DateTime.UtcNow
        });
        await Db.SaveChangesAsync();

        var result = await svc.HandleAsync(
            MakePayload("log_inflight", "subscription.completed", new
            {
                checkout = new { id = "chk_inflight" },
                payment = new { id = "pay_inflight", amount = 5990 }
            }), null, "s", true);

        // 2xx, sem executar o negócio em paralelo.
        Assert.False(result.IsUnauthorized);
        Assert.False(result.Applied);
        Assert.Contains("Duplicate webhook ignored", result.Message, StringComparison.OrdinalIgnoreCase);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Pending, sub.Status);
    }

    [Fact]
    public async Task Registro_orfao_em_Processing_antigo_volta_a_ser_processavel()
    {
        var svc = BuildWebhookService();
        var sub = await CreatePendingSubscriptionAsync("chk_orphan");

        // Processo anterior morreu sem atualizar o status (fora da janela de in-flight).
        Db.PaymentWebhookLogs.Add(new PaymentWebhookLog
        {
            Provider = "AbacatePay",
            EventId = "log_orphan",
            Type = "subscription.completed",
            RawPayload = "{}",
            ProcessingStatus = WebhookProcessingStatus.Processing,
            LastAttemptAt = DateTime.UtcNow.AddHours(-1)
        });
        await Db.SaveChangesAsync();

        var result = await svc.HandleAsync(
            MakePayload("log_orphan", "subscription.completed", new
            {
                checkout = new { id = "chk_orphan" },
                payment = new { id = "pay_orphan", amount = 5990 }
            }), null, "s", true);

        Assert.True(result.Applied);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Active, sub.Status);
    }

    private static async Task<WebhookHandlerResult?> RunGuardedAsync(IAbacatePayWebhookService svc, string raw)
    {
        try
        {
            return await svc.HandleAsync(raw, null, "s", true);
        }
        catch
        {
            // Uma das execuções pode perder a corrida de escrita e propagar exceção;
            // o que importa é que o efeito financeiro não se duplique.
            return null;
        }
    }
}
