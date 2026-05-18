using System.Data;
using System.Text.Json;
using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Payment;
using FitPlatform.Application.DTOs.Subscription;
using FitPlatform.Application.Interfaces;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FitPlatform.Infrastructure.Services;

public class PaymentService
{
    private readonly AppDbContext _db;
    private readonly IPaymentProvider _paymentProvider;
    private readonly ILogger<PaymentService> _logger;
    private readonly PasswordSetupService _passwordSetupService;

    public PaymentService(AppDbContext db, IPaymentProvider paymentProvider, ILogger<PaymentService> logger, PasswordSetupService passwordSetupService)
    {
        _db = db;
        _paymentProvider = paymentProvider;
        _logger = logger;
        _passwordSetupService = passwordSetupService;
    }

    public async Task<ApiResponse<List<PlanBillingOptionResponse>>> GetBillingOptionsAsync(Guid planId, CancellationToken cancellationToken = default)
    {
        var plan = await _db.PlatformPlans.FirstOrDefaultAsync(x => x.Id == planId && x.Active, cancellationToken);
        if (plan == null) return ApiResponse<List<PlanBillingOptionResponse>>.Fail("Plano nÃ£o encontrado.");

        var list = Enum.GetValues<BillingFrequency>().Select(c => Calculate(plan.MonthlyPrice, c)).ToList();
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
        var plan = await _db.PlatformPlans.FirstOrDefaultAsync(x => x.Id == request.PlanId && x.Active, cancellationToken);
        if (plan == null) return ApiResponse<ValidateCouponResponse>.Fail("Plano nÃ£o encontrado.");

        var calc = await CalculateWithCouponAsync(plan, request.BillingCycle, request.CouponCode, trainerId, cancellationToken);
        return ApiResponse<ValidateCouponResponse>.Ok(ToCouponResponse(calc, calc.CouponError ?? "Cupom vÃ¡lido."));
    }

    public async Task<ApiResponse<SubscriptionResponse>> CreateSubscriptionAsync(Guid trainerId, CreateSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        var trainer = await _db.Trainers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == trainerId, cancellationToken);
        if (trainer == null) return ApiResponse<SubscriptionResponse>.Fail("Trainer nÃ£o encontrado.");

        var plan = await _db.PlatformPlans.FirstOrDefaultAsync(x => x.Id == request.PlatformPlanId && x.Active, cancellationToken);
        if (plan == null) return ApiResponse<SubscriptionResponse>.Fail("Plano nÃ£o encontrado.");

        var active = await _db.TrainerSubscriptions.FirstOrDefaultAsync(
            ts => ts.TrainerId == trainerId && ts.Status == TrainerSubscriptionStatus.Active,
            cancellationToken);
        if (active != null) return ApiResponse<SubscriptionResponse>.Fail("VocÃª jÃ¡ possui uma assinatura ativa.");

        var option = await _db.PlanBillingOptions.FirstOrDefaultAsync(
            x => x.PlatformPlanId == plan.Id && x.BillingCycle == request.BillingCycle && x.IsActive,
            cancellationToken);
        if (option == null || string.IsNullOrWhiteSpace(option.AbacatePayProductId))
            return ApiResponse<SubscriptionResponse>.Fail("Produto da AbacatePay nÃ£o configurado para esse plano/ciclo.");

        var onboarding = await _db.TrainerOnboardings
            .Where(x => x.Email == trainer.User.Email && x.Status == OnboardingStatus.WaitingPayment)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var pricing = await CalculateWithCouponAsync(plan, request.BillingCycle, request.CouponCode, trainerId, cancellationToken);
        if (pricing.CouponError != null) return ApiResponse<SubscriptionResponse>.Fail(pricing.CouponError);
        if (pricing.FinalAmountInCents != option.FinalPriceInCents)
        {
            return ApiResponse<SubscriptionResponse>.Fail(
                "O valor final calculado difere do valor do produto recorrente configurado na AbacatePay para este plano/ciclo. Ajuste o produto externo ou prossiga sem cupom.");
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
                EndDate = CalculateCycleEndDate(request.BillingCycle),
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

            var providerResult = await _paymentProvider.CreateSubscriptionAsync(new CreateProviderSubscriptionRequest
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
                AbacatePayProductId = option.AbacatePayProductId,
                ExternalId = $"trainer:{trainerId}:sub:{subscription.Id}",
                Metadata = metadata
            }, cancellationToken);

            subscription.AbacatePayCustomerId = providerResult.ProviderCustomerId;
            subscription.AbacatePayCheckoutId = providerResult.ProviderCheckoutId;
            subscription.AbacatePaySubscriptionId = providerResult.ProviderSubscriptionId;
            subscription.InitPoint = providerResult.CheckoutUrl;
            subscription.UpdatedAt = DateTime.UtcNow;

            _db.TrainerPayments.Add(new TrainerPayment
            {
                TrainerId = trainerId,
                TrainerSubscriptionId = subscription.Id,
                Amount = pricing.FinalAmountInCents / 100m,
                Status = PaymentStatus.Pending,
                Provider = "AbacatePay",
                ProviderPaymentId = providerResult.ProviderCheckoutId,
                ProviderSubscriptionId = providerResult.ProviderSubscriptionId,
                AbacatePayCheckoutId = providerResult.ProviderCheckoutId,
                RawPayload = providerResult.RawPayload
            });

            await _db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            return ApiResponse<SubscriptionResponse>.Ok(new SubscriptionResponse
            {
                Id = subscription.Id,
                PlanName = plan.Name,
                MonthlyPrice = plan.MonthlyPrice,
                MaxActiveStudents = plan.MaxActiveStudents,
                Status = subscription.Status.ToString(),
                BillingCycle = subscription.BillingCycle.ToString(),
                CheckoutUrl = subscription.InitPoint,
                AbacatePayCheckoutId = subscription.AbacatePayCheckoutId,
                AbacatePaySubscriptionId = subscription.AbacatePaySubscriptionId
            });
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ApiResponse> HandleWebhookAsync(JsonElement payload, string? eventId, string? eventType, string? resourceId, CancellationToken cancellationToken = default)
    {
        var evtId = eventId?.Trim()
            ?? Extract(payload, "id")
            ?? $"local-{Guid.NewGuid():N}";

        var evtType = (eventType?.Trim()
            ?? Extract(payload, "event")
            ?? "unknown").Trim();
        _logger.LogInformation("Webhook received. Provider=AbacatePay EventId={EventId} EventType={EventType} ResourceId={ResourceId}",
            evtId, evtType, resourceId);

        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            var existingLog = await _db.PaymentWebhookLogs.FirstOrDefaultAsync(
                x => x.Provider == "AbacatePay" && x.EventId == evtId,
                cancellationToken);
            if (existingLog != null)
            {
                _logger.LogInformation("Webhook ignored by idempotency. Provider=AbacatePay EventId={EventId}", evtId);
                await tx.CommitAsync(cancellationToken);
                return ApiResponse.Ok("Webhook jÃ¡ processado.");
            }

            var log = new PaymentWebhookLog
            {
                Provider = "AbacatePay",
                EventId = evtId,
                Type = evtType,
                ResourceId = resourceId ?? Extract(payload, "data.subscription.id") ?? Extract(payload, "data.checkout.id"),
                RawPayload = payload.GetRawText()
            };
            _db.PaymentWebhookLogs.Add(log);
            await _db.SaveChangesAsync(cancellationToken);

            var subscription = await ResolveSubscriptionFromWebhookAsync(payload, cancellationToken);
            if (subscription == null)
            {
                _logger.LogWarning("Webhook AbacatePay sem correlaÃ§Ã£o interna. EventId={EventId} EventType={EventType}", evtId, evtType);
                log.ProcessedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);
                return ApiResponse.Ok("Webhook recebido sem correlaÃ§Ã£o local.");
            }

            await ApplyWebhookToSubscriptionAsync(subscription, payload, evtType, cancellationToken);
            log.ProcessedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return ApiResponse.Ok("Webhook processado.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar webhook AbacatePay.");
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task ApplyWebhookToSubscriptionAsync(TrainerSubscription subscription, JsonElement payload, string eventType, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var eventPaymentId = Extract(payload, "data.payment.id") ?? Extract(payload, "data.checkout.id");
        var eventSubscriptionId = Extract(payload, "data.subscription.id") ?? subscription.AbacatePaySubscriptionId;
        var previousStatus = subscription.Status;

        subscription.AbacatePaySubscriptionId ??= eventSubscriptionId;
        subscription.UpdatedAt = now;

        if (eventType == "subscription.completed" || eventType == "subscription.renewed")
        {
            subscription.Status = TrainerSubscriptionStatus.Active;
            subscription.LastPaymentStatus = "paid";
            subscription.StartDate = subscription.Status == TrainerSubscriptionStatus.Active ? subscription.StartDate : now;
            subscription.EndDate = CalculateCycleEndDate(subscription.BillingCycle);

            var payment = await ResolvePaymentForCompletionAsync(subscription, eventPaymentId, eventType, cancellationToken);
            payment.Status = PaymentStatus.Approved;
            payment.PaidAt ??= now;
            payment.ProviderSubscriptionId = subscription.AbacatePaySubscriptionId;
            payment.RawPayload = payload.GetRawText();
            payment.UpdatedAt = now;

            if (eventType == "subscription.completed")
            {
                await TryRedeemCouponAsync(subscription, payment, cancellationToken);
                await AdvanceOnboardingAsync(subscription, cancellationToken);
            }
        }
        else if (eventType == "subscription.cancelled")
        {
            subscription.Status = TrainerSubscriptionStatus.Canceled;
            subscription.LastPaymentStatus = "cancelled";
            subscription.EndDate = now;
        }
        else if (eventType == "subscription.payment_failed")
        {
            subscription.LastPaymentStatus = "failed";
            var payment = await ResolvePaymentForFailureAsync(subscription, eventPaymentId, cancellationToken);
            if (payment != null)
            {
                payment.Status = PaymentStatus.Rejected;
                payment.RawPayload = payload.GetRawText();
                payment.UpdatedAt = now;
            }
        }

        _logger.LogInformation("Subscription webhook transition applied. SubscriptionId={SubscriptionId} EventType={EventType} PreviousStatus={PreviousStatus} NewStatus={NewStatus}",
            subscription.Id, eventType, previousStatus, subscription.Status);
    }

    private async Task<TrainerPayment> ResolvePaymentForCompletionAsync(TrainerSubscription subscription, string? providerPaymentId, string eventType, CancellationToken cancellationToken)
    {
        var payment = await _db.TrainerPayments
            .Where(x => x.TrainerSubscriptionId == subscription.Id)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (eventType == "subscription.renewed")
        {
            var existingRenewal = !string.IsNullOrWhiteSpace(providerPaymentId)
                ? await _db.TrainerPayments.FirstOrDefaultAsync(
                    x => x.TrainerSubscriptionId == subscription.Id && x.Provider == "AbacatePay" && x.ProviderPaymentId == providerPaymentId,
                    cancellationToken)
                : null;
            if (existingRenewal != null) return existingRenewal;

            var created = new TrainerPayment
            {
                TrainerId = subscription.TrainerId,
                TrainerSubscriptionId = subscription.Id,
                Amount = subscription.FinalAmountInCents / 100m,
                Status = PaymentStatus.Pending,
                Provider = "AbacatePay",
                ProviderPaymentId = providerPaymentId,
                ProviderSubscriptionId = subscription.AbacatePaySubscriptionId,
                AbacatePayCheckoutId = subscription.AbacatePayCheckoutId
            };
            _db.TrainerPayments.Add(created);
            await _db.SaveChangesAsync(cancellationToken);
            return created;
        }

        if (payment == null)
        {
            payment = new TrainerPayment
            {
                TrainerId = subscription.TrainerId,
                TrainerSubscriptionId = subscription.Id,
                Amount = subscription.FinalAmountInCents / 100m,
                Status = PaymentStatus.Pending,
                Provider = "AbacatePay",
                ProviderPaymentId = providerPaymentId,
                ProviderSubscriptionId = subscription.AbacatePaySubscriptionId,
                AbacatePayCheckoutId = subscription.AbacatePayCheckoutId
            };
            _db.TrainerPayments.Add(payment);
            await _db.SaveChangesAsync(cancellationToken);
        }

        payment.ProviderPaymentId ??= providerPaymentId;
        return payment;
    }

    private async Task<TrainerPayment?> ResolvePaymentForFailureAsync(TrainerSubscription subscription, string? providerPaymentId, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(providerPaymentId))
        {
            var exact = await _db.TrainerPayments.FirstOrDefaultAsync(
                x => x.TrainerSubscriptionId == subscription.Id && x.Provider == "AbacatePay" && x.ProviderPaymentId == providerPaymentId,
                cancellationToken);
            if (exact != null) return exact;
        }

        return await _db.TrainerPayments
            .Where(x => x.TrainerSubscriptionId == subscription.Id && x.Status == PaymentStatus.Pending)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task TryRedeemCouponAsync(TrainerSubscription subscription, TrainerPayment payment, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(subscription.CouponCodeApplied) || subscription.CouponDiscountAmountInCents <= 0)
            return;

        var coupon = await _db.DiscountCoupons.FirstOrDefaultAsync(
            c => c.Code.ToUpper() == subscription.CouponCodeApplied!.ToUpper(),
            cancellationToken);
        if (coupon == null) return;

        var existing = await _db.DiscountCouponRedemptions.FirstOrDefaultAsync(
            r => r.CouponId == coupon.Id && r.TrainerId == subscription.TrainerId && r.SubscriptionId == subscription.Id,
            cancellationToken);
        if (existing != null) return;

        if (coupon.MaxUsesTotal.HasValue && coupon.CurrentUses >= coupon.MaxUsesTotal.Value)
        {
            _logger.LogWarning("Cupom sem saldo no momento do resgate final. Coupon={Coupon} Subscription={SubscriptionId}", coupon.Code, subscription.Id);
            return;
        }

        if (coupon.MaxUsesPerCustomer.HasValue)
        {
            var uses = await _db.DiscountCouponRedemptions.CountAsync(
                x => x.CouponId == coupon.Id && x.TrainerId == subscription.TrainerId,
                cancellationToken);
            if (uses >= coupon.MaxUsesPerCustomer.Value)
            {
                _logger.LogWarning("Limite por cliente excedido no resgate final. Coupon={Coupon} Trainer={TrainerId}", coupon.Code, subscription.TrainerId);
                return;
            }
        }

        _db.DiscountCouponRedemptions.Add(new DiscountCouponRedemption
        {
            CouponId = coupon.Id,
            TrainerId = subscription.TrainerId,
            SubscriptionId = subscription.Id,
            PaymentId = payment.Id,
            RedeemedAt = DateTime.UtcNow,
            DiscountAmountInCents = subscription.CouponDiscountAmountInCents
        });
        coupon.CurrentUses += 1;
        coupon.UpdatedAt = DateTime.UtcNow;
    }

    private async Task AdvanceOnboardingAsync(TrainerSubscription subscription, CancellationToken cancellationToken)
    {
        if (!subscription.TrainerOnboardingId.HasValue) return;

        var onboarding = await _db.TrainerOnboardings.FirstOrDefaultAsync(x => x.Id == subscription.TrainerOnboardingId.Value, cancellationToken);
        if (onboarding == null) return;

        if (onboarding.Status == OnboardingStatus.Canceled || onboarding.Status == OnboardingStatus.Completed) return;
        TryAdvanceOnboardingStatus(onboarding, OnboardingStatus.PaymentApproved);
        onboarding.UpdatedAt = DateTime.UtcNow;

        if (!onboarding.CreatedTrainerId.HasValue)
            onboarding.CreatedTrainerId = subscription.TrainerId;

        if (!onboarding.CreatedUserId.HasValue)
        {
            var trainer = await _db.Trainers.FirstOrDefaultAsync(x => x.Id == subscription.TrainerId, cancellationToken);
            if (trainer != null) onboarding.CreatedUserId = trainer.UserId;
        }

        if (onboarding.CreatedUserId.HasValue)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == onboarding.CreatedUserId.Value, cancellationToken);
            if (user != null && !user.IsActive)
            {
                user.IsActive = true;
                user.UpdatedAt = DateTime.UtcNow;
                await _passwordSetupService.GenerateAndSendSetupTokenAsync(user, "FitPlatform", isStudent: false);
            }
        }

        TryAdvanceOnboardingStatus(onboarding, OnboardingStatus.AccountCreated);
        TryAdvanceOnboardingStatus(onboarding, OnboardingStatus.Completed);
        _logger.LogInformation("Onboarding advanced by webhook. OnboardingId={OnboardingId} NewStatus={Status}", onboarding.Id, onboarding.Status);
    }

    private static void TryAdvanceOnboardingStatus(TrainerOnboarding onboarding, OnboardingStatus targetStatus)
    {
        if (GetOnboardingRank(targetStatus) > GetOnboardingRank(onboarding.Status))
            onboarding.Status = targetStatus;
    }

    private static int GetOnboardingRank(OnboardingStatus status) => status switch
    {
        OnboardingStatus.Draft => 0,
        OnboardingStatus.WaitingPayment => 1,
        OnboardingStatus.PaymentApproved => 2,
        OnboardingStatus.AccountCreated => 3,
        OnboardingStatus.Completed => 4,
        OnboardingStatus.Canceled => -1,
        _ => 0
    };

    private async Task<TrainerSubscription?> ResolveSubscriptionFromWebhookAsync(JsonElement payload, CancellationToken cancellationToken)
    {
        var byMetadataSubscriptionId = Extract(payload, "data.metadata.trainerSubscriptionId");
        if (Guid.TryParse(byMetadataSubscriptionId, out var subscriptionIdFromMetadata))
        {
            var sub = await _db.TrainerSubscriptions.FirstOrDefaultAsync(x => x.Id == subscriptionIdFromMetadata, cancellationToken);
            if (sub != null) return sub;
        }

        var subscriptionId = Extract(payload, "data.subscription.id");
        if (!string.IsNullOrWhiteSpace(subscriptionId))
        {
            var sub = await _db.TrainerSubscriptions
                .Where(x => x.AbacatePaySubscriptionId == subscriptionId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
            if (sub != null) return sub;
        }

        var checkoutId = Extract(payload, "data.checkout.id") ?? Extract(payload, "data.id");
        if (!string.IsNullOrWhiteSpace(checkoutId))
        {
            var sub = await _db.TrainerSubscriptions
                .Where(x => x.AbacatePayCheckoutId == checkoutId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
            if (sub != null) return sub;
        }

        var externalId = Extract(payload, "data.externalId");
        if (!string.IsNullOrWhiteSpace(externalId))
        {
            var subToken = externalId.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var maybeSub = subToken.LastOrDefault();
            if (Guid.TryParse(maybeSub, out var subId))
            {
                var sub = await _db.TrainerSubscriptions.FirstOrDefaultAsync(x => x.Id == subId, cancellationToken);
                if (sub != null) return sub;
            }
        }

        return null;
    }

    private async Task<PricingSnapshot> CalculateWithCouponAsync(PlatformPlan plan, BillingFrequency cycle, string? couponCode, Guid trainerId, CancellationToken cancellationToken)
    {
        var snapshot = Calculate(plan.MonthlyPrice, cycle);
        if (string.IsNullOrWhiteSpace(couponCode)) return snapshot;

        var coupon = await _db.DiscountCoupons.FirstOrDefaultAsync(
            c => c.Code.ToUpper() == couponCode.Trim().ToUpper(),
            cancellationToken);
        if (coupon == null || !coupon.IsActive) return snapshot with { CouponError = "Cupom invÃ¡lido." };

        var now = DateTime.UtcNow;
        if (coupon.StartsAt.HasValue && coupon.StartsAt > now) return snapshot with { CouponError = "Cupom fora da vigÃªncia." };
        if (coupon.ExpiresAt.HasValue && coupon.ExpiresAt < now) return snapshot with { CouponError = "Cupom expirado." };
        if (coupon.AppliesToPlanId.HasValue && coupon.AppliesToPlanId != plan.Id) return snapshot with { CouponError = "Cupom nÃ£o aplicÃ¡vel a este plano." };
        if (coupon.AppliesToBillingCycle.HasValue && coupon.AppliesToBillingCycle != cycle) return snapshot with { CouponError = "Cupom nÃ£o aplicÃ¡vel a este ciclo." };
        if (coupon.MaxUsesTotal.HasValue && coupon.CurrentUses >= coupon.MaxUsesTotal.Value) return snapshot with { CouponError = "Cupom esgotado." };
        if (coupon.MinimumPurchaseAmountInCents.HasValue && snapshot.SubtotalAfterCycleDiscountInCents < coupon.MinimumPurchaseAmountInCents.Value)
            return snapshot with { CouponError = "Valor mÃ­nimo nÃ£o atingido para o cupom." };

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

        couponDiscount = Math.Max(0, Math.Min(couponDiscount, snapshot.SubtotalAfterCycleDiscountInCents));
        return snapshot with
        {
            Coupon = coupon,
            CouponDiscountAmountInCents = couponDiscount,
            FinalAmountInCents = snapshot.SubtotalAfterCycleDiscountInCents - couponDiscount
        };
    }

    private static PricingSnapshot Calculate(decimal monthlyPrice, BillingFrequency cycle)
    {
        var months = cycle switch
        {
            BillingFrequency.Quarterly => 3,
            BillingFrequency.Semiannual => 6,
            BillingFrequency.Yearly => 12,
            _ => 1
        };
        var cycleDiscount = cycle switch
        {
            BillingFrequency.Quarterly => 10m,
            BillingFrequency.Semiannual => 15m,
            BillingFrequency.Yearly => 20m,
            _ => 0m
        };
        var baseAmount = (int)Math.Round(monthlyPrice * 100m * months, MidpointRounding.AwayFromZero);
        var cycleAmount = (int)Math.Round(baseAmount * (cycleDiscount / 100m), MidpointRounding.AwayFromZero);
        var subtotal = Math.Max(0, baseAmount - cycleAmount);
        return new PricingSnapshot(cycle, months, cycleDiscount, baseAmount, cycleAmount, subtotal, 0, subtotal, null, null);
    }

    private static DateTime CalculateCycleEndDate(BillingFrequency cycle)
    {
        var now = DateTime.UtcNow;
        return cycle switch
        {
            BillingFrequency.Quarterly => now.AddMonths(3),
            BillingFrequency.Semiannual => now.AddMonths(6),
            BillingFrequency.Yearly => now.AddMonths(12),
            _ => now.AddMonths(1)
        };
    }

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

    private static string? Extract(JsonElement payload, string path)
    {
        var current = payload;
        foreach (var p in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!current.TryGetProperty(p, out current)) return null;
        }
        return current.ValueKind == JsonValueKind.String ? current.GetString() : current.ToString();
    }

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






