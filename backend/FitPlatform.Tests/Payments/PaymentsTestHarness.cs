using FitPlatform.Application.Configuration;
using FitPlatform.Application.DTOs.Payment;
using FitPlatform.Application.Exceptions;
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

namespace FitPlatform.Tests.Payments;

/// <summary>
/// Base compartilhada dos testes de pagamento/assinatura.
/// Usa SQLite in-memory porque o provider InMemory do EF não suporta transações,
/// que são exigidas pelo processamento de webhook e pela criação de checkout.
/// </summary>
public abstract class PaymentsTestHarness : IDisposable
{
    protected readonly SqliteConnection Connection;
    protected readonly AppDbContext Db;

    protected PaymentsTestHarness()
    {
        Connection = new SqliteConnection("DataSource=:memory:");
        Connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(Connection)
            .Options;

        Db = new AppDbContext(options);
        Db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Db.Dispose();
        Connection.Dispose();
        GC.SuppressFinalize(this);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Fábricas de serviço
    // ═══════════════════════════════════════════════════════════════════════════

    protected static IOptions<AbacatePayOptions> BuildOptions(Action<AbacatePayOptions>? configure = null)
    {
        var options = new AbacatePayOptions
        {
            ApiKey = "test-key",
            WebhookSecret = "test-secret",
            WebhookPublicKey = "test-public-key",
            SuccessUrl = "https://app.example.test/onboarding/success",
            ReturnUrl = "https://app.example.test/onboarding/return",
            TrainerPanelUrl = "https://app.example.test/trainer/subscription",
            DevMode = false,
            EnableSignatureValidation = true,
            EnableCouponSync = true
        };
        configure?.Invoke(options);
        return Options.Create(options);
    }

    protected PasswordSetupService BuildPasswordSetupService()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["FrontendUrl"] = "http://localhost:5173" })
            .Build();
        return new PasswordSetupService(Db, new FakeEmailService(), config);
    }

    protected SubscriptionLifecycleService BuildLifecycle() =>
        new(Db, BuildPasswordSetupService(), NullLogger<SubscriptionLifecycleService>.Instance);

    protected AbacatePayWebhookService BuildWebhookService(
        IAbacatePaySignatureValidator? validator = null,
        Action<AbacatePayOptions>? configureOptions = null)
        => new(
            Db,
            validator ?? new AlwaysValidValidator(),
            BuildLifecycle(),
            BuildOptions(configureOptions),
            NullLogger<AbacatePayWebhookService>.Instance);

    protected PaymentService BuildPaymentService(
        IPaymentProvider provider, Action<AbacatePayOptions>? configureOptions = null)
        => new(
            Db,
            provider,
            BuildLifecycle(),
            BuildOptions(configureOptions),
            NullLogger<PaymentService>.Instance);

    protected SubscriptionReconciliationRunner BuildReconciliationRunner(
        IPaymentProvider provider,
        IAbacatePayWebhookService? webhookService = null,
        Action<AbacatePayOptions>? configureOptions = null)
        => new(
            Db,
            provider,
            BuildLifecycle(),
            webhookService ?? BuildWebhookService(),
            BuildOptions(configureOptions),
            NullLogger<SubscriptionReconciliationRunner>.Instance);

    // ═══════════════════════════════════════════════════════════════════════════
    // Fábricas de dados
    // ═══════════════════════════════════════════════════════════════════════════

    protected async Task<PlatformPlan> CreatePlanAsync(
        string name = "Basic",
        decimal monthlyPrice = 59.90m,
        string? productId = "prod_basic_monthly",
        BillingFrequency cycle = BillingFrequency.Monthly)
    {
        var plan = new PlatformPlan
        {
            Code = $"PLAN_{Guid.NewGuid():N}",
            Name = name,
            MonthlyPrice = monthlyPrice,
            Active = true,
            IsAvailableForPurchase = true
        };
        Db.PlatformPlans.Add(plan);
        await Db.SaveChangesAsync();

        if (productId is not null)
        {
            // FinalPriceInCents deve espelhar o preço do ciclo calculado pelo backend.
            var (months, discountPercent) = cycle switch
            {
                BillingFrequency.Semiannual => (6, 15m),
                BillingFrequency.Yearly => (12, 20m),
                _ => (1, 0m)
            };
            var baseAmount = (int)Math.Round(monthlyPrice * 100m * months, MidpointRounding.AwayFromZero);
            var discount = (int)Math.Round(baseAmount * (discountPercent / 100m), MidpointRounding.AwayFromZero);

            Db.PlanBillingOptions.Add(new PlanBillingOption
            {
                PlatformPlanId = plan.Id,
                BillingCycle = cycle,
                MonthsCount = months,
                CycleDiscountPercent = discountPercent,
                BasePriceInCents = baseAmount,
                FinalPriceInCents = baseAmount - discount,
                AbacatePayProductId = productId,
                IsActive = true
            });
            await Db.SaveChangesAsync();
        }

        return plan;
    }

    protected async Task<Trainer> CreateTrainerAsync()
    {
        var user = new User
        {
            Email = $"trainer_{Guid.NewGuid():N}@test.com",
            Name = "Trainer Test",
            PasswordHash = "x",
            Role = UserRole.Trainer
        };
        Db.Users.Add(user);

        var trainer = new Trainer { UserId = user.Id, Phone = "11999999999" };
        Db.Trainers.Add(trainer);
        await Db.SaveChangesAsync();
        return trainer;
    }

    /// <summary>Cria uma assinatura Pending com pagamento Pending associado.</summary>
    protected async Task<TrainerSubscription> CreatePendingSubscriptionAsync(
        string checkoutId = "chk_test",
        string? abacateSubId = null,
        Trainer? trainer = null,
        PlatformPlan? plan = null,
        DateTime? createdAt = null)
    {
        trainer ??= await CreateTrainerAsync();
        plan ??= await CreatePlanAsync();

        var sub = new TrainerSubscription
        {
            TrainerId = trainer.Id,
            PlatformPlanId = plan.Id,
            Status = TrainerSubscriptionStatus.Pending,
            BillingCycle = BillingFrequency.Monthly,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1),
            Provider = "AbacatePay",
            BaseAmountInCents = 5990,
            FinalAmountInCents = 5990,
            AbacatePayCheckoutId = checkoutId,
            AbacatePaySubscriptionId = abacateSubId,
            LastPaymentStatus = "pending"
        };
        if (createdAt.HasValue) sub.CreatedAt = createdAt.Value;
        Db.TrainerSubscriptions.Add(sub);

        Db.TrainerPayments.Add(new TrainerPayment
        {
            TrainerId = trainer.Id,
            TrainerSubscriptionId = sub.Id,
            Amount = 59.90m,
            Status = PaymentStatus.Pending,
            Provider = "AbacatePay",
            AbacatePayCheckoutId = checkoutId
        });

        await Db.SaveChangesAsync();
        return sub;
    }

    protected async Task<TrainerSubscription> CreateActiveSubscriptionAsync(
        string abacateSubId = "subs_active",
        Trainer? trainer = null,
        PlatformPlan? plan = null)
    {
        var sub = await CreatePendingSubscriptionAsync("chk_active", abacateSubId, trainer, plan);
        sub.Status = TrainerSubscriptionStatus.Active;
        sub.LastPaymentStatus = "paid";
        sub.EndDate = DateTime.UtcNow.AddMonths(1);
        await Db.SaveChangesAsync();
        return sub;
    }

    protected async Task<DiscountCoupon> CreateCouponAsync(
        string code,
        DiscountType type = DiscountType.Percentage,
        decimal value = 20m,
        bool isActive = true,
        DateTime? expiresAt = null,
        int? maxUsesTotal = null,
        int currentUses = 0,
        Guid? appliesToPlanId = null)
    {
        var coupon = new DiscountCoupon
        {
            Code = code,
            DiscountType = type,
            DiscountValue = value,
            IsActive = isActive,
            ExpiresAt = expiresAt,
            MaxUsesTotal = maxUsesTotal,
            CurrentUses = currentUses,
            AppliesToPlanId = appliesToPlanId
        };
        Db.DiscountCoupons.Add(coupon);
        await Db.SaveChangesAsync();
        return coupon;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Fakes
    // ═══════════════════════════════════════════════════════════════════════════

    protected sealed class FakeEmailService : IEmailService
    {
        public Task SendPasswordSetupAsync(string toEmail, string toName, string setupLink, string planName)
            => Task.CompletedTask;
        public Task SendStudentWelcomeAsync(string toEmail, string studentName, string trainerBrand, string setupLink)
            => Task.CompletedTask;
        public Task SendPasswordResetAsync(string toEmail, string toName, string resetLink)
            => Task.CompletedTask;
    }

    protected class AlwaysValidValidator : IAbacatePaySignatureValidator
    {
        public bool IsSignatureValidationEnabled => true;
        public bool ValidateSecret(string? querySecret) => true;
        public bool ValidateSignature(string rawBody, string? signatureHeader) => true;
    }

    protected class InvalidSecretValidator : IAbacatePaySignatureValidator
    {
        public bool IsSignatureValidationEnabled => true;
        public bool ValidateSecret(string? querySecret) => false;
        public bool ValidateSignature(string rawBody, string? signatureHeader) => true;
    }

    protected class InvalidSignatureValidator : IAbacatePaySignatureValidator
    {
        public bool IsSignatureValidationEnabled => true;
        public bool ValidateSecret(string? querySecret) => true;
        public bool ValidateSignature(string rawBody, string? signatureHeader) => false;
    }

    protected class SignatureDisabledValidator : IAbacatePaySignatureValidator
    {
        public bool IsSignatureValidationEnabled => false;
        public bool ValidateSecret(string? querySecret) => true;
        public bool ValidateSignature(string rawBody, string? signatureHeader) => false;
    }

    /// <summary>
    /// Provider de pagamento controlável. Registra as chamadas recebidas para que os
    /// testes verifiquem exatamente o que foi enviado ao provedor (cupons, produto, ids).
    /// </summary>
    protected sealed class FakePaymentProvider : IPaymentProvider
    {
        public List<CreateProviderSubscriptionRequest> CreateCalls { get; } = new();
        public List<string> CancelCalls { get; } = new();
        public List<ChangeProviderSubscriptionPlanRequest> ChangePlanCalls { get; } = new();
        public List<ProviderCouponDefinition> CouponCalls { get; } = new();
        public List<ProviderSubscriptionListQuery> ListCalls { get; } = new();

        public List<ProviderSubscriptionDetails> RemoteSubscriptions { get; } = new();

        public Exception? CreateException { get; set; }
        public Exception? CancelException { get; set; }
        public Exception? ChangePlanException { get; set; }
        public Exception? CouponException { get; set; }
        public Exception? ListException { get; set; }

        public string CheckoutUrl { get; set; } = "https://app.abacatepay.com/pay/bill_fake";
        public string CheckoutId { get; set; } = "bill_fake";

        public Task<ProviderSubscriptionCreated> CreateSubscriptionAsync(
            CreateProviderSubscriptionRequest request, CancellationToken cancellationToken = default)
        {
            CreateCalls.Add(request);
            if (CreateException is not null) throw CreateException;

            return Task.FromResult(new ProviderSubscriptionCreated
            {
                ProviderSubscriptionId = string.Empty,
                ProviderCheckoutId = CheckoutId,
                ProviderCustomerId = "cust_fake",
                CheckoutUrl = CheckoutUrl,
                RawPayload = "{}"
            });
        }

        public Task<ProviderSubscriptionDetails?> GetSubscriptionAsync(
            string providerSubscriptionId, CancellationToken cancellationToken = default)
            => Task.FromResult(RemoteSubscriptions.FirstOrDefault(x =>
                x.ProviderCheckoutId == providerSubscriptionId ||
                x.ProviderSubscriptionId == providerSubscriptionId ||
                x.ExternalId == providerSubscriptionId));

        public Task<IReadOnlyList<ProviderSubscriptionDetails>> ListSubscriptionsAsync(
            ProviderSubscriptionListQuery query, CancellationToken cancellationToken = default)
        {
            ListCalls.Add(query);
            if (ListException is not null) throw ListException;
            return Task.FromResult<IReadOnlyList<ProviderSubscriptionDetails>>(RemoteSubscriptions);
        }

        public Task CancelSubscriptionAsync(string providerSubscriptionId, CancellationToken cancellationToken = default)
        {
            CancelCalls.Add(providerSubscriptionId);
            if (CancelException is not null) throw CancelException;
            return Task.CompletedTask;
        }

        public Task<ProviderPlanChangeResult> ChangeSubscriptionPlanAsync(
            ChangeProviderSubscriptionPlanRequest request, CancellationToken cancellationToken = default)
        {
            ChangePlanCalls.Add(request);
            if (ChangePlanException is not null) throw ChangePlanException;

            return Task.FromResult(new ProviderPlanChangeResult
            {
                UpdateId = "subu_fake",
                ProviderSubscriptionId = request.ProviderSubscriptionId,
                Status = "PENDING",
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                RequestedAt = DateTime.UtcNow
            });
        }

        public Task<ProviderCouponSyncResult> EnsureCouponAsync(
            ProviderCouponDefinition coupon, CancellationToken cancellationToken = default)
        {
            CouponCalls.Add(coupon);
            if (CouponException is not null) throw CouponException;

            return Task.FromResult(new ProviderCouponSyncResult { Code = coupon.Code, AlreadyExisted = false });
        }
    }

    protected static PaymentProviderException ProviderFailure(string message = "falha simulada") => new(message);
}
