using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FitPlatform.Application.Configuration;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FitPlatform.Tests.Payments;

/// <summary>
/// Testes do único processador de webhooks da AbacatePay.
/// Cobre autenticação, idempotência, cada evento tratado, política de devMode e retry.
/// </summary>
public class AbacatePayWebhookTests : PaymentsTestHarness
{
    /// <summary>Cria payload JSON mínimo de um evento.</summary>
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
    // Segurança — autenticação do webhook
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Secret_invalido_retorna_Unauthorized()
    {
        var svc = BuildWebhookService(new InvalidSecretValidator());
        var raw = MakePayload("evt_001", "subscription.completed");

        var result = await svc.HandleAsync(raw, "sig", "wrong-secret", isDevelopmentEnvironment: false);

        Assert.True(result.IsUnauthorized);
    }

    [Fact]
    public async Task Assinatura_invalida_em_producao_retorna_Unauthorized()
    {
        var svc = BuildWebhookService(new InvalidSignatureValidator());
        var raw = MakePayload("evt_002", "subscription.completed");

        var result = await svc.HandleAsync(raw, "bad-sig", "secret", isDevelopmentEnvironment: false);

        Assert.True(result.IsUnauthorized);
    }

    [Fact]
    public async Task Assinatura_invalida_em_development_nao_rejeita()
    {
        var svc = BuildWebhookService(new InvalidSignatureValidator());
        var raw = MakePayload("evt_003", "unknown.event");

        var result = await svc.HandleAsync(raw, "bad-sig", "secret", isDevelopmentEnvironment: true);

        Assert.False(result.IsUnauthorized);
    }

    [Fact]
    public async Task EnableSignatureValidation_false_em_producao_rejeita()
    {
        var svc = BuildWebhookService(new SignatureDisabledValidator());
        var raw = MakePayload("evt_004", "subscription.completed");

        var result = await svc.HandleAsync(raw, null, "secret", isDevelopmentEnvironment: false);

        Assert.True(result.IsUnauthorized);
    }

    [Fact]
    public async Task EnableSignatureValidation_false_em_development_aceita()
    {
        var svc = BuildWebhookService(new SignatureDisabledValidator());
        var raw = MakePayload("evt_005", "unknown.event");

        var result = await svc.HandleAsync(raw, null, "secret", isDevelopmentEnvironment: true);

        Assert.False(result.IsUnauthorized);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Idempotência
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Evento_duplicado_retorna_200_e_nao_processa_novamente()
    {
        var svc = BuildWebhookService();
        await CreatePendingSubscriptionAsync("chk_dup");

        var data = new { checkout = new { id = "chk_dup" } };
        var raw = MakePayload("evt_dup_001", "subscription.completed", data);

        var r1 = await svc.HandleAsync(raw, null, "s", isDevelopmentEnvironment: true);
        Assert.False(r1.IsUnauthorized);
        Assert.True(r1.Applied);

        var r2 = await svc.HandleAsync(raw, null, "s", isDevelopmentEnvironment: true);
        Assert.False(r2.IsUnauthorized);
        Assert.False(r2.Applied);
        Assert.Contains("Duplicate webhook ignored", r2.Message, StringComparison.OrdinalIgnoreCase);

        var logs = await Db.PaymentWebhookLogs.Where(x => x.EventId == "evt_dup_001").ToListAsync();
        Assert.Single(logs);
    }

    [Fact]
    public async Task Evento_duplicado_nao_duplica_pagamento_nem_resgate_de_cupom()
    {
        var svc = BuildWebhookService();
        var sub = await CreatePendingSubscriptionAsync("chk_idem");

        var coupon = await CreateCouponAsync("IDEMP20");
        sub.CouponCodeApplied = coupon.Code;
        sub.CouponDiscountAmountInCents = 1198;
        await Db.SaveChangesAsync();

        var data = new { checkout = new { id = "chk_idem" }, payment = new { id = "pay_idem", amount = 4792 } };

        await svc.HandleAsync(MakePayload("evt_idem_1", "subscription.completed", data), null, "s", true);
        // Mesmo pagamento chegando com outro EventId (reentrega do provedor).
        await svc.HandleAsync(MakePayload("evt_idem_2", "subscription.completed", data), null, "s", true);

        var approved = await Db.TrainerPayments.CountAsync(
            p => p.TrainerSubscriptionId == sub.Id && p.Status == PaymentStatus.Approved);
        Assert.Equal(1, approved);

        var redemptions = await Db.DiscountCouponRedemptions.CountAsync(r => r.SubscriptionId == sub.Id);
        Assert.Equal(1, redemptions);

        await Db.Entry(coupon).ReloadAsync();
        Assert.Equal(1, coupon.CurrentUses);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // subscription.completed
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task SubscriptionCompleted_ativa_assinatura()
    {
        var svc = BuildWebhookService();
        var sub = await CreatePendingSubscriptionAsync("chk_sc01");
        Assert.Equal(TrainerSubscriptionStatus.Pending, sub.Status);

        var data = new
        {
            checkout = new { id = "chk_sc01" },
            subscription = new { id = "subs_external_01" },
            payment = new { id = "pay_001", amount = 5990 },
            customer = new { id = "cus_001" }
        };

        var result = await svc.HandleAsync(
            MakePayload("evt_sc_001", "subscription.completed", data), null, "s", true);

        Assert.False(result.IsUnauthorized);
        Assert.True(result.Applied);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Active, sub.Status);
        Assert.Equal("subs_external_01", sub.AbacatePaySubscriptionId);
        Assert.Equal("cus_001", sub.AbacatePayCustomerId);
        Assert.Equal("paid", sub.LastPaymentStatus);

        var payment = await Db.TrainerPayments.FirstOrDefaultAsync(
            p => p.TrainerSubscriptionId == sub.Id && p.Status == PaymentStatus.Approved);
        Assert.NotNull(payment);
        Assert.Equal("pay_001", payment.ProviderPaymentId);
    }

    [Fact]
    public async Task SubscriptionCompleted_via_metadata_trainerSubscriptionId()
    {
        var svc = BuildWebhookService();
        var sub = await CreatePendingSubscriptionAsync("chk_meta");

        var data = new
        {
            metadata = new { trainerSubscriptionId = sub.Id.ToString() },
            payment = new { id = "pay_meta", amount = 5990 }
        };

        await svc.HandleAsync(MakePayload("evt_sc_meta", "subscription.completed", data), null, "s", true);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Active, sub.Status);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // subscription.renewed
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task SubscriptionRenewed_registra_pagamento_e_mantem_ativo()
    {
        var svc = BuildWebhookService();
        var sub = await CreateActiveSubscriptionAsync("subs_rn_ext");

        var data = new
        {
            subscription = new { id = "subs_rn_ext" },
            payment = new { id = "pay_renewal_001", amount = 5990 }
        };

        var result = await svc.HandleAsync(MakePayload("evt_rn_001", "subscription.renewed", data), null, "s", true);

        Assert.False(result.IsUnauthorized);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Active, sub.Status);
        Assert.Equal("paid", sub.LastPaymentStatus);

        var renewal = await Db.TrainerPayments.FirstOrDefaultAsync(p => p.ProviderPaymentId == "pay_renewal_001");
        Assert.NotNull(renewal);
        Assert.Equal(PaymentStatus.Approved, renewal.Status);
    }

    [Fact]
    public async Task SubscriptionRenewed_nao_duplica_pagamento_ja_registrado()
    {
        var svc = BuildWebhookService();
        await CreateActiveSubscriptionAsync("subs_rn2_ext");

        var data = new
        {
            subscription = new { id = "subs_rn2_ext" },
            payment = new { id = "pay_dup_renewal", amount = 5990 }
        };

        await svc.HandleAsync(MakePayload("evt_rn_p1", "subscription.renewed", data), null, "s", true);
        await svc.HandleAsync(MakePayload("evt_rn_p2", "subscription.renewed", data), null, "s", true);

        var count = await Db.TrainerPayments.CountAsync(p => p.ProviderPaymentId == "pay_dup_renewal");
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task SubscriptionRenewed_aplica_troca_de_plano_agendada()
    {
        var svc = BuildWebhookService();
        var sub = await CreateActiveSubscriptionAsync("subs_planchange");
        var newPlan = await CreatePlanAsync("Pro", 99.90m, "prod_pro_monthly");

        sub.PendingPlatformPlanId = newPlan.Id;
        sub.PendingBillingCycle = BillingFrequency.Monthly;
        sub.PendingFinalAmountInCents = 9990;
        sub.PendingPlanChangeProviderId = "subu_x";
        sub.PendingPlanChangeRequestedAt = DateTime.UtcNow.AddDays(-2);
        await Db.SaveChangesAsync();

        var data = new
        {
            subscription = new { id = "subs_planchange" },
            payment = new { id = "pay_after_change", amount = 9990 }
        };

        await svc.HandleAsync(MakePayload("evt_rn_change", "subscription.renewed", data), null, "s", true);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(newPlan.Id, sub.PlatformPlanId);
        Assert.Equal(9990, sub.FinalAmountInCents);
        Assert.Null(sub.PendingPlatformPlanId);
        Assert.Null(sub.PendingPlanChangeProviderId);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // subscription.payment_failed
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task SubscriptionPaymentFailed_registra_falha_sem_cancelar_assinatura()
    {
        var svc = BuildWebhookService();
        var sub = await CreateActiveSubscriptionAsync("subs_pf");
        var originalEndDate = sub.EndDate;

        var data = new
        {
            subscription = new { id = "subs_pf" },
            payment = new { id = "pay_failed_001", amount = 5990 }
        };

        var result = await svc.HandleAsync(
            MakePayload("evt_pf_001", "subscription.payment_failed", data), null, "s", true);

        Assert.False(result.IsUnauthorized);
        Assert.True(result.Applied);

        await Db.Entry(sub).ReloadAsync();

        // A assinatura NÃO é cancelada por uma falha isolada — a AbacatePay ainda vai retentar.
        Assert.Equal(TrainerSubscriptionStatus.Active, sub.Status);
        Assert.Equal("failed", sub.LastPaymentStatus);
        Assert.Equal(1, sub.PaymentFailureCount);
        Assert.NotNull(sub.LastPaymentFailureAt);

        // O acesso já pago é preservado.
        Assert.Equal(originalEndDate, sub.EndDate);

        var payment = await Db.TrainerPayments.FirstOrDefaultAsync(p => p.ProviderPaymentId == "pay_failed_001");
        Assert.NotNull(payment);
        Assert.Equal(PaymentStatus.Rejected, payment.Status);
    }

    [Fact]
    public async Task SubscriptionPaymentFailed_e_idempotente_para_o_mesmo_pagamento()
    {
        var svc = BuildWebhookService();
        var sub = await CreateActiveSubscriptionAsync("subs_pf2");

        var data = new
        {
            subscription = new { id = "subs_pf2" },
            payment = new { id = "pay_failed_dup", amount = 5990 }
        };

        await svc.HandleAsync(MakePayload("evt_pf_a", "subscription.payment_failed", data), null, "s", true);
        await svc.HandleAsync(MakePayload("evt_pf_b", "subscription.payment_failed", data), null, "s", true);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(1, sub.PaymentFailureCount);

        var rejected = await Db.TrainerPayments.CountAsync(
            p => p.ProviderPaymentId == "pay_failed_dup" && p.Status == PaymentStatus.Rejected);
        Assert.Equal(1, rejected);
    }

    [Fact]
    public async Task SubscriptionPaymentFailed_seguido_de_renovacao_zera_contador()
    {
        var svc = BuildWebhookService();
        var sub = await CreateActiveSubscriptionAsync("subs_pf3");

        await svc.HandleAsync(MakePayload("evt_pf_x", "subscription.payment_failed", new
        {
            subscription = new { id = "subs_pf3" },
            payment = new { id = "pay_f1", amount = 5990 }
        }), null, "s", true);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(1, sub.PaymentFailureCount);

        await svc.HandleAsync(MakePayload("evt_rn_x", "subscription.renewed", new
        {
            subscription = new { id = "subs_pf3" },
            payment = new { id = "pay_ok_1", amount = 5990 }
        }), null, "s", true);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(0, sub.PaymentFailureCount);
        Assert.Equal("paid", sub.LastPaymentStatus);
        Assert.Null(sub.LastPaymentFailureAt);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // subscription.cancelled
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task SubscriptionCancelled_cancela_e_mantem_enddate()
    {
        var svc = BuildWebhookService();
        var sub = await CreateActiveSubscriptionAsync("subs_cx_ext");
        var originalEndDate = sub.EndDate;

        var result = await svc.HandleAsync(
            MakePayload("evt_cx_001", "subscription.cancelled", new { subscription = new { id = "subs_cx_ext" } }),
            null, "s", true);

        Assert.False(result.IsUnauthorized);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Canceled, sub.Status);
        Assert.Equal(originalEndDate, sub.EndDate);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // checkout.completed
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task CheckoutCompleted_ativa_assinatura_pendente()
    {
        var svc = BuildWebhookService();
        var sub = await CreatePendingSubscriptionAsync("chk_co01");

        var data = new { id = "chk_co01", payment = new { id = "pay_co_001", amount = 5990 } };
        await svc.HandleAsync(MakePayload("evt_co_001", "checkout.completed", data), null, "s", true);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Active, sub.Status);
    }

    [Fact]
    public async Task CheckoutCompleted_nao_duplica_se_assinatura_ja_ativa()
    {
        var svc = BuildWebhookService();
        var sub = await CreateActiveSubscriptionAsync("subs_co02");

        var before = await Db.TrainerPayments.CountAsync(
            p => p.TrainerSubscriptionId == sub.Id && p.Status == PaymentStatus.Approved);

        var data = new { id = sub.AbacatePayCheckoutId, payment = new { id = "pay_co_002" } };
        await svc.HandleAsync(MakePayload("evt_co_002", "checkout.completed", data), null, "s", true);

        var after = await Db.TrainerPayments.CountAsync(
            p => p.TrainerSubscriptionId == sub.Id && p.Status == PaymentStatus.Approved);

        Assert.Equal(before, after);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Evento desconhecido
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Evento_desconhecido_salva_log_como_Ignored_e_retorna_200()
    {
        var svc = BuildWebhookService();
        var raw = MakePayload("evt_unk_001", "subscription.trial_started");

        var result = await svc.HandleAsync(raw, null, "s", true);

        Assert.False(result.IsUnauthorized);
        Assert.False(result.Applied);

        var log = await Db.PaymentWebhookLogs.FirstOrDefaultAsync(x => x.EventId == "evt_unk_001");
        Assert.NotNull(log);
        Assert.Equal("subscription.trial_started", log.Type);

        // Ignored é terminal: auditável, mas nunca retentado.
        Assert.Equal(WebhookProcessingStatus.Ignored, log.ProcessingStatus);
        Assert.Null(log.NextRetryAt);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DevMode
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Evento_devMode_e_ignorado_quando_instancia_e_producao()
    {
        var svc = BuildWebhookService(configureOptions: o => o.DevMode = false);
        var sub = await CreatePendingSubscriptionAsync("chk_devmode");

        var data = new { checkout = new { id = "chk_devmode" }, payment = new { id = "pay_dev", amount = 5990 } };
        var result = await svc.HandleAsync(
            MakePayload("evt_dev_001", "subscription.completed", data, devMode: true), null, "s", true);

        Assert.False(result.IsUnauthorized);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Pending, sub.Status);

        var log = await Db.PaymentWebhookLogs.FirstOrDefaultAsync(x => x.EventId == "evt_dev_001");
        Assert.NotNull(log);
        Assert.Equal(WebhookProcessingStatus.IgnoredDevMode, log.ProcessingStatus);
    }

    [Fact]
    public async Task Evento_devMode_e_processado_quando_instancia_e_sandbox()
    {
        var svc = BuildWebhookService(configureOptions: o => o.DevMode = true);
        var sub = await CreatePendingSubscriptionAsync("chk_devmode_ok");

        var data = new { checkout = new { id = "chk_devmode_ok" }, payment = new { id = "pay_dev_ok", amount = 5990 } };
        await svc.HandleAsync(
            MakePayload("evt_dev_002", "subscription.completed", data, devMode: true), null, "s", true);

        await Db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Active, sub.Status);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // AbacatePaySignatureValidator — unit tests diretos
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void SignatureValidator_ValidateSecret_correto()
    {
        var v = new AbacatePaySignatureValidator(Options.Create(new AbacatePayOptions { WebhookSecret = "meu-secret" }));

        Assert.True(v.ValidateSecret("meu-secret"));
        Assert.False(v.ValidateSecret("secret-errado"));
        Assert.False(v.ValidateSecret(null));
        Assert.False(v.ValidateSecret(""));
    }

    [Fact]
    public void SignatureValidator_ValidateSignature_correto()
    {
        const string publicKey = "chave-publica-teste";
        const string body = "{\"id\":\"evt_001\",\"event\":\"subscription.completed\"}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(publicKey));
        var expectedSig = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(body)));

        var v = new AbacatePaySignatureValidator(Options.Create(new AbacatePayOptions { WebhookPublicKey = publicKey }));

        Assert.True(v.ValidateSignature(body, expectedSig));
        Assert.False(v.ValidateSignature(body, "assinatura-invalida"));
        Assert.False(v.ValidateSignature(body, null));
    }

    [Fact]
    public void SignatureValidator_EnableSignatureValidation_reflete_config()
    {
        Assert.True(new AbacatePaySignatureValidator(
            Options.Create(new AbacatePayOptions { EnableSignatureValidation = true })).IsSignatureValidationEnabled);
        Assert.False(new AbacatePaySignatureValidator(
            Options.Create(new AbacatePayOptions { EnableSignatureValidation = false })).IsSignatureValidationEnabled);
    }
}
