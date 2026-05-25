using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FitPlatform.Application.Configuration;
using FitPlatform.Application.Interfaces;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using FitPlatform.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace FitPlatform.Tests;

/// <summary>
/// Testes do AbacatePayWebhookService.
/// Usa SQLite in-memory para suportar transações (InMemory EF não suporta).
/// </summary>
public class AbacatePayWebhookTests : IDisposable
{
    // ── SQLite in-memory compartilhado por instância de teste ─────────────────
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;

    public AbacatePayWebhookTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Helpers / Fakes
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Sempre valida secret e assinatura como corretos.</summary>
    private class AlwaysValidValidator : IAbacatePaySignatureValidator
    {
        public bool IsSignatureValidationEnabled => true;
        public bool ValidateSecret(string? querySecret) => true;
        public bool ValidateSignature(string rawBody, string? signatureHeader) => true;
    }

    /// <summary>Rejeita secret (secret inválido).</summary>
    private class InvalidSecretValidator : IAbacatePaySignatureValidator
    {
        public bool IsSignatureValidationEnabled => true;
        public bool ValidateSecret(string? querySecret) => false;
        public bool ValidateSignature(string rawBody, string? signatureHeader) => true;
    }

    /// <summary>Aceita secret mas rejeita assinatura HMAC.</summary>
    private class InvalidSignatureValidator : IAbacatePaySignatureValidator
    {
        public bool IsSignatureValidationEnabled => true;
        public bool ValidateSecret(string? querySecret) => true;
        public bool ValidateSignature(string rawBody, string? signatureHeader) => false;
    }

    /// <summary>Validação de assinatura desabilitada (EnableSignatureValidation=false).</summary>
    private class SignatureDisabledValidator : IAbacatePaySignatureValidator
    {
        public bool IsSignatureValidationEnabled => false;
        public bool ValidateSecret(string? querySecret) => true;
        public bool ValidateSignature(string rawBody, string? signatureHeader) => false;
    }

    private PasswordSetupService BuildPasswordSetupService()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["FrontendUrl"] = "http://localhost:5173" })
            .Build();
        return new PasswordSetupService(_db, new FakeEmailService(), config);
    }

    private AbacatePayWebhookService BuildService(IAbacatePaySignatureValidator? validator = null)
        => new(
            _db,
            validator ?? new AlwaysValidValidator(),
            BuildPasswordSetupService(),
            NullLogger<AbacatePayWebhookService>.Instance);

    /// <summary>Cria payload JSON mínimo de um evento.</summary>
    private static string MakePayload(string eventId, string eventType, object? data = null)
    {
        var payload = new
        {
            id = eventId,
            @event = eventType,
            apiVersion = 2,
            devMode = false,
            data = data ?? new { }
        };
        return JsonSerializer.Serialize(payload);
    }

    /// <summary>Cria subscription interna em estado Pending com CheckoutId definido.</summary>
    private async Task<TrainerSubscription> CreatePendingSubscriptionAsync(
        string checkoutId = "chk_test", string? abacateSubId = null)
    {
        var user = new User
        {
            Email = $"trainer_{Guid.NewGuid():N}@test.com",
            Name = "Trainer Test",
            PasswordHash = "x",
            Role = UserRole.Trainer
        };
        _db.Users.Add(user);

        var trainer = new Trainer { UserId = user.Id };
        _db.Trainers.Add(trainer);

        var plan = new PlatformPlan
        {
            Code = $"PLAN_{Guid.NewGuid():N}",
            Name = "Basic",
            MonthlyPrice = 59.90m,
            Active = true,
            IsAvailableForPurchase = true
        };
        _db.PlatformPlans.Add(plan);
        await _db.SaveChangesAsync();

        var sub = new TrainerSubscription
        {
            TrainerId             = trainer.Id,
            PlatformPlanId        = plan.Id,
            Status                = TrainerSubscriptionStatus.Pending,
            BillingCycle          = BillingFrequency.Monthly,
            StartDate             = DateTime.UtcNow,
            EndDate               = DateTime.UtcNow.AddMonths(1),
            Provider              = "AbacatePay",
            FinalAmountInCents    = 5990,
            AbacatePayCheckoutId  = checkoutId,
            AbacatePaySubscriptionId = abacateSubId,
            LastPaymentStatus     = "pending"
        };
        _db.TrainerSubscriptions.Add(sub);

        var payment = new TrainerPayment
        {
            TrainerId             = trainer.Id,
            TrainerSubscriptionId = sub.Id,
            Amount                = 59.90m,
            Status                = PaymentStatus.Pending,
            Provider              = "AbacatePay",
            AbacatePayCheckoutId  = checkoutId
        };
        _db.TrainerPayments.Add(payment);

        await _db.SaveChangesAsync();
        return sub;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Segurança — autenticação
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Secret_invalido_retorna_Unauthorized()
    {
        var svc = BuildService(new InvalidSecretValidator());
        var raw = MakePayload("evt_001", "subscription.completed");

        var result = await svc.HandleAsync(raw, "sig", "wrong-secret", isDevelopmentEnvironment: false);

        Assert.True(result.IsUnauthorized);
    }

    [Fact]
    public async Task Assinatura_invalida_em_producao_retorna_Unauthorized()
    {
        var svc = BuildService(new InvalidSignatureValidator());
        var raw = MakePayload("evt_002", "subscription.completed");

        var result = await svc.HandleAsync(raw, "bad-sig", "secret", isDevelopmentEnvironment: false);

        Assert.True(result.IsUnauthorized);
    }

    [Fact]
    public async Task Assinatura_invalida_em_development_nao_rejeita()
    {
        var svc = BuildService(new InvalidSignatureValidator());
        var raw = MakePayload("evt_003", "unknown.event");

        // Deve aceitar mesmo com assinatura inválida em Development.
        var result = await svc.HandleAsync(raw, "bad-sig", "secret", isDevelopmentEnvironment: true);

        Assert.False(result.IsUnauthorized);
    }

    [Fact]
    public async Task EnableSignatureValidation_false_em_producao_rejeita()
    {
        var svc = BuildService(new SignatureDisabledValidator());
        var raw = MakePayload("evt_004", "subscription.completed");

        var result = await svc.HandleAsync(raw, null, "secret", isDevelopmentEnvironment: false);

        Assert.True(result.IsUnauthorized);
    }

    [Fact]
    public async Task EnableSignatureValidation_false_em_development_aceita()
    {
        var svc = BuildService(new SignatureDisabledValidator());
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
        var svc = BuildService();
        var sub = await CreatePendingSubscriptionAsync("chk_dup");

        var data = new { checkout = new { id = "chk_dup" } };
        var raw  = MakePayload("evt_dup_001", "subscription.completed", data);

        // Primeira chamada — deve processar.
        var r1 = await svc.HandleAsync(raw, null, "s", isDevelopmentEnvironment: true);
        Assert.False(r1.IsUnauthorized);

        // Segunda chamada com o mesmo EventId — deve ser idempotente.
        var r2 = await svc.HandleAsync(raw, null, "s", isDevelopmentEnvironment: true);
        Assert.False(r2.IsUnauthorized);
        Assert.Contains("idempotência", r2.Message, StringComparison.OrdinalIgnoreCase);

        // Log deve conter exatamente 1 registro.
        var logs = await _db.PaymentWebhookLogs
            .Where(x => x.EventId == "evt_dup_001")
            .ToListAsync();
        Assert.Single(logs);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // subscription.completed
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task SubscriptionCompleted_ativa_assinatura()
    {
        var svc = BuildService();
        var sub = await CreatePendingSubscriptionAsync("chk_sc01");
        Assert.Equal(TrainerSubscriptionStatus.Pending, sub.Status);

        var data = new
        {
            checkout     = new { id = "chk_sc01" },
            subscription = new { id = "sub_external_01" },
            payment      = new { id = "pay_001", amount = 5990 },
            customer     = new { id = "cus_001" }
        };
        var raw = MakePayload("evt_sc_001", "subscription.completed", data);

        var result = await svc.HandleAsync(raw, null, "s", isDevelopmentEnvironment: true);

        Assert.False(result.IsUnauthorized);

        await _db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Active, sub.Status);
        Assert.Equal("sub_external_01", sub.AbacatePaySubscriptionId);
        Assert.Equal("cus_001", sub.AbacatePayCustomerId);
        Assert.Equal("paid", sub.LastPaymentStatus);

        var payment = await _db.TrainerPayments
            .Where(p => p.TrainerSubscriptionId == sub.Id && p.Status == PaymentStatus.Approved)
            .FirstOrDefaultAsync();
        Assert.NotNull(payment);
        Assert.Equal("pay_001", payment.ProviderPaymentId);
    }

    [Fact]
    public async Task SubscriptionCompleted_via_metadata_trainerSubscriptionId()
    {
        var svc = BuildService();
        var sub = await CreatePendingSubscriptionAsync("chk_meta");

        var data = new
        {
            metadata = new { trainerSubscriptionId = sub.Id.ToString() },
            payment  = new { id = "pay_meta", amount = 5990 }
        };
        var raw = MakePayload("evt_sc_meta", "subscription.completed", data);

        await svc.HandleAsync(raw, null, "s", isDevelopmentEnvironment: true);

        await _db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Active, sub.Status);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // subscription.renewed
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task SubscriptionRenewed_registra_pagamento_e_mantém_ativo()
    {
        var svc = BuildService();
        var sub = await CreatePendingSubscriptionAsync("chk_rn01", "sub_rn_ext");

        // Ativar primeiro.
        sub.Status = TrainerSubscriptionStatus.Active;
        sub.AbacatePaySubscriptionId = "sub_rn_ext";
        await _db.SaveChangesAsync();

        var data = new
        {
            subscription = new { id = "sub_rn_ext" },
            payment      = new { id = "pay_renewal_001", amount = 5990 }
        };
        var raw = MakePayload("evt_rn_001", "subscription.renewed", data);

        var result = await svc.HandleAsync(raw, null, "s", isDevelopmentEnvironment: true);

        Assert.False(result.IsUnauthorized);

        await _db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Active, sub.Status);
        Assert.Equal("paid", sub.LastPaymentStatus);

        var renewalPayment = await _db.TrainerPayments
            .FirstOrDefaultAsync(p => p.ProviderPaymentId == "pay_renewal_001");
        Assert.NotNull(renewalPayment);
        Assert.Equal(PaymentStatus.Approved, renewalPayment.Status);
    }

    [Fact]
    public async Task SubscriptionRenewed_nao_duplica_pagamento_ja_registrado()
    {
        var svc = BuildService();
        var sub = await CreatePendingSubscriptionAsync("chk_rn02", "sub_rn2_ext");
        sub.Status = TrainerSubscriptionStatus.Active;
        await _db.SaveChangesAsync();

        var data = new
        {
            subscription = new { id = "sub_rn2_ext" },
            payment      = new { id = "pay_dup_renewal", amount = 5990 }
        };
        var raw1 = MakePayload("evt_rn_p1", "subscription.renewed", data);
        var raw2 = MakePayload("evt_rn_p2", "subscription.renewed", data); // mesmo payment.id, eventId diferente

        await svc.HandleAsync(raw1, null, "s", isDevelopmentEnvironment: true);
        await svc.HandleAsync(raw2, null, "s", isDevelopmentEnvironment: true); // idempotência de payment

        var count = await _db.TrainerPayments
            .CountAsync(p => p.ProviderPaymentId == "pay_dup_renewal");
        Assert.Equal(1, count);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // subscription.cancelled
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task SubscriptionCancelled_cancela_e_mantem_enddate()
    {
        var svc = BuildService();
        var sub = await CreatePendingSubscriptionAsync("chk_cx01", "sub_cx_ext");
        sub.Status = TrainerSubscriptionStatus.Active;
        var originalEndDate = sub.EndDate;
        await _db.SaveChangesAsync();

        var data = new { subscription = new { id = "sub_cx_ext" } };
        var raw  = MakePayload("evt_cx_001", "subscription.cancelled", data);

        var result = await svc.HandleAsync(raw, null, "s", isDevelopmentEnvironment: true);

        Assert.False(result.IsUnauthorized);

        await _db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Canceled, sub.Status);
        // EndDate preservado para acesso gracioso até fim do período pago.
        Assert.Equal(originalEndDate, sub.EndDate);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // checkout.completed
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task CheckoutCompleted_ativa_assinatura_pendente()
    {
        var svc = BuildService();
        var sub = await CreatePendingSubscriptionAsync("chk_co01");
        Assert.Equal(TrainerSubscriptionStatus.Pending, sub.Status);

        var data = new
        {
            id      = "chk_co01",
            payment = new { id = "pay_co_001", amount = 5990 }
        };
        var raw = MakePayload("evt_co_001", "checkout.completed", data);

        var result = await svc.HandleAsync(raw, null, "s", isDevelopmentEnvironment: true);

        Assert.False(result.IsUnauthorized);

        await _db.Entry(sub).ReloadAsync();
        Assert.Equal(TrainerSubscriptionStatus.Active, sub.Status);
    }

    [Fact]
    public async Task CheckoutCompleted_nao_duplica_se_assinatura_ja_ativa()
    {
        var svc = BuildService();
        var sub = await CreatePendingSubscriptionAsync("chk_co02");
        sub.Status = TrainerSubscriptionStatus.Active;
        await _db.SaveChangesAsync();

        var countBefore = await _db.TrainerPayments
            .CountAsync(p => p.TrainerSubscriptionId == sub.Id && p.Status == PaymentStatus.Approved);

        var data = new { id = "chk_co02", payment = new { id = "pay_co_002" } };
        var raw  = MakePayload("evt_co_002", "checkout.completed", data);

        await svc.HandleAsync(raw, null, "s", isDevelopmentEnvironment: true);

        var countAfter = await _db.TrainerPayments
            .CountAsync(p => p.TrainerSubscriptionId == sub.Id && p.Status == PaymentStatus.Approved);

        // Nenhum novo pagamento criado — assinatura já estava ativa.
        Assert.Equal(countBefore, countAfter);
    }

    [Fact]
    public async Task CheckoutCompleted_nao_duplica_pagamento_ja_aprovado()
    {
        var svc = BuildService();
        var sub = await CreatePendingSubscriptionAsync("chk_co03");

        // Simular pagamento já aprovado com o mesmo payment.id.
        _db.TrainerPayments.Add(new TrainerPayment
        {
            TrainerId             = sub.TrainerId,
            TrainerSubscriptionId = sub.Id,
            Amount                = 59.90m,
            Status                = PaymentStatus.Approved,
            Provider              = "AbacatePay",
            ProviderPaymentId     = "pay_co_003"
        });
        await _db.SaveChangesAsync();

        var data = new { id = "chk_co03", payment = new { id = "pay_co_003" } };
        var raw  = MakePayload("evt_co_003", "checkout.completed", data);

        await svc.HandleAsync(raw, null, "s", isDevelopmentEnvironment: true);

        var count = await _db.TrainerPayments
            .CountAsync(p => p.ProviderPaymentId == "pay_co_003");
        Assert.Equal(1, count); // Sem duplicata.
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Evento desconhecido
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Evento_desconhecido_salva_log_e_retorna_200()
    {
        var svc = BuildService();
        var raw = MakePayload("evt_unk_001", "subscription.trial_started");

        var result = await svc.HandleAsync(raw, null, "s", isDevelopmentEnvironment: true);

        Assert.False(result.IsUnauthorized);

        var log = await _db.PaymentWebhookLogs
            .FirstOrDefaultAsync(x => x.EventId == "evt_unk_001");
        Assert.NotNull(log);
        Assert.Equal("subscription.trial_started", log.Type);
        Assert.Equal(WebhookProcessingStatus.Processed, log.ProcessingStatus);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // AbacatePaySignatureValidator — unit tests diretos
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void SignatureValidator_ValidateSecret_correto()
    {
        var opts = Options.Create(new AbacatePayOptions { WebhookSecret = "meu-secret" });
        var v = new AbacatePaySignatureValidator(opts);

        Assert.True(v.ValidateSecret("meu-secret"));
        Assert.False(v.ValidateSecret("secret-errado"));
        Assert.False(v.ValidateSecret(null));
        Assert.False(v.ValidateSecret(""));
    }

    [Fact]
    public void SignatureValidator_ValidateSignature_correto()
    {
        const string publicKey = "chave-publica-teste";
        const string body      = "{\"id\":\"evt_001\",\"event\":\"subscription.completed\"}";

        using var hmac     = new HMACSHA256(Encoding.UTF8.GetBytes(publicKey));
        var expectedSig    = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(body)));

        var opts = Options.Create(new AbacatePayOptions { WebhookPublicKey = publicKey });
        var v    = new AbacatePaySignatureValidator(opts);

        Assert.True(v.ValidateSignature(body, expectedSig));
        Assert.False(v.ValidateSignature(body, "assinatura-invalida"));
        Assert.False(v.ValidateSignature(body, null));
    }

    [Fact]
    public void SignatureValidator_EnableSignatureValidation_reflete_config()
    {
        var optsEnabled  = Options.Create(new AbacatePayOptions { EnableSignatureValidation = true });
        var optsDisabled = Options.Create(new AbacatePayOptions { EnableSignatureValidation = false });

        Assert.True(new AbacatePaySignatureValidator(optsEnabled).IsSignatureValidationEnabled);
        Assert.False(new AbacatePaySignatureValidator(optsDisabled).IsSignatureValidationEnabled);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Fake email service para testes
    // ═══════════════════════════════════════════════════════════════════════════

    private sealed class FakeEmailService : FitPlatform.Application.Interfaces.IEmailService
    {
        public Task SendPasswordSetupAsync(string toEmail, string toName, string setupLink, string planName)
            => Task.CompletedTask;
        public Task SendStudentWelcomeAsync(string toEmail, string studentName, string trainerBrand, string setupLink)
            => Task.CompletedTask;
        public Task SendPasswordResetAsync(string toEmail, string toName, string resetLink)
            => Task.CompletedTask;
    }
}
