using System.Text.Json;
using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Subscription;
using FitPlatform.Application.DTOs.Payment;
using FitPlatform.Application.Interfaces;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class PaymentService
{
    private readonly AppDbContext _db;
    private readonly IPaymentProvider _paymentProvider;

    public PaymentService(AppDbContext db, IPaymentProvider paymentProvider)
    {
        _db = db;
        _paymentProvider = paymentProvider;
    }

    public async Task<ApiResponse<SubscriptionResponse>> CreateSubscriptionAsync(Guid trainerId, CreateSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        var trainer = await _db.Trainers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == trainerId, cancellationToken);
        if (trainer == null)
            return ApiResponse<SubscriptionResponse>.Fail("Trainer não encontrado.");

        var plan = await _db.PlatformPlans.Include(p => p.Prices).FirstOrDefaultAsync(p => p.Id == request.PlatformPlanId, cancellationToken);
        if (plan == null || !plan.Active)
            return ApiResponse<SubscriptionResponse>.Fail("Plano não encontrado ou inativo.");

        var existing = await _db.TrainerSubscriptions
            .Where(ts => ts.TrainerId == trainerId && ts.Status == TrainerSubscriptionStatus.Active)
            .FirstOrDefaultAsync(cancellationToken);
        if (existing != null)
            return ApiResponse<SubscriptionResponse>.Fail("Você já possui uma assinatura ativa.");

        PlatformPlanPrice? price = null;
        if (request.PlatformPlanPriceId.HasValue)
        {
            price = plan.Prices.FirstOrDefault(p => p.Id == request.PlatformPlanPriceId && p.Active);
            if (price == null)
            {
                return ApiResponse<SubscriptionResponse>.Fail("Preço do plano não encontrado.");
            }
        }

        var cycleEndDate = CalculateCycleEndDate(request.BillingCycle);
        var amount = price?.Price ?? plan.MonthlyPrice;

        var subscription = new TrainerSubscription
        {
            TrainerId = trainerId,
            PlatformPlanId = plan.Id,
            PlatformPlanPriceId = price?.Id,
            BillingCycle = request.BillingCycle,
            Status = TrainerSubscriptionStatus.Pending,
            StartDate = DateTime.UtcNow,
            EndDate = cycleEndDate,
            LastPaymentStatus = "pending"
        };
        _db.TrainerSubscriptions.Add(subscription);
        await _db.SaveChangesAsync(cancellationToken);

        var providerResult = await _paymentProvider.CreateSubscriptionAsync(new CreateProviderSubscriptionRequest
        {
            LocalSubscriptionId = subscription.Id,
            TrainerId = trainerId,
            PlanId = plan.Id,
            PlanName = plan.Name,
            BillingCycle = request.BillingCycle,
            Amount = amount,
            PayerEmail = trainer.User.Email
        }, cancellationToken);

        subscription.MercadoPagoSubscriptionId = providerResult.ProviderSubscriptionId;
        subscription.MercadoPagoPayerId = providerResult.PayerId;
        subscription.InitPoint = providerResult.CheckoutUrl;
        subscription.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return ApiResponse<SubscriptionResponse>.Ok(MapResponse(subscription, plan, price, new List<TrainerPayment>()));
    }

    public async Task<ApiResponse> SimulateApprovedAsync(Guid trainerId, CancellationToken cancellationToken = default)
    {
        var subscription = await _db.TrainerSubscriptions
            .Where(ts => ts.TrainerId == trainerId)
            .OrderByDescending(ts => ts.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription == null) return ApiResponse.Fail("Nenhuma assinatura encontrada.");

        await MarkSubscriptionStatusAsync(subscription, "authorized", null, "simulation.approved", cancellationToken);
        return ApiResponse.Ok("Pagamento simulado com sucesso. Assinatura ativada.");
    }

    public async Task<ApiResponse> SimulateExpiredAsync(Guid trainerId, CancellationToken cancellationToken = default)
    {
        var subscription = await _db.TrainerSubscriptions
            .Where(ts => ts.TrainerId == trainerId)
            .OrderByDescending(ts => ts.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription == null) return ApiResponse.Fail("Nenhuma assinatura encontrada.");

        await MarkSubscriptionStatusAsync(subscription, "cancelled", null, "simulation.expired", cancellationToken);
        return ApiResponse.Ok("Assinatura marcada como vencida.");
    }

    public async Task<ApiResponse> HandleWebhookAsync(JsonElement payload, string? eventId, string? eventType, string? resourceId, CancellationToken cancellationToken = default)
    {
        var normalizedEventId = eventId?.Trim();
        var normalizedType = eventType?.Trim() ?? "unknown";
        var normalizedResourceId = resourceId?.Trim();
        var rawPayload = payload.ValueKind == JsonValueKind.Undefined ? "{}" : payload.GetRawText();

        if (string.IsNullOrWhiteSpace(normalizedEventId))
        {
            normalizedEventId = ExtractPayloadValue(payload, "id") ?? $"local-{Guid.NewGuid():N}";
        }

        var existingLog = await _db.PaymentWebhookLogs
            .FirstOrDefaultAsync(x => x.Provider == "MercadoPago" && x.EventId == normalizedEventId, cancellationToken);
        if (existingLog != null)
        {
            return ApiResponse.Ok("Webhook já processado.");
        }

        var log = new PaymentWebhookLog
        {
            Provider = "MercadoPago",
            EventId = normalizedEventId,
            Type = normalizedType,
            ResourceId = normalizedResourceId,
            RawPayload = rawPayload
        };
        _db.PaymentWebhookLogs.Add(log);
        await _db.SaveChangesAsync(cancellationToken);

        normalizedResourceId ??= ExtractPayloadValue(payload, "data.id");
        if (string.IsNullOrWhiteSpace(normalizedResourceId))
        {
            log.ProcessedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return ApiResponse.Ok("Webhook recebido sem resource id.");
        }

        var details = await _paymentProvider.GetSubscriptionAsync(normalizedResourceId, cancellationToken);
        if (details == null)
        {
            log.ProcessedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return ApiResponse.Ok("Assinatura não encontrada no provider.");
        }

        var localSubscription = await _db.TrainerSubscriptions
            .Where(s => s.MercadoPagoSubscriptionId == details.ProviderSubscriptionId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (localSubscription == null)
        {
            log.ProcessedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return ApiResponse.Ok("Sem assinatura local para conciliação.");
        }

        await MarkSubscriptionStatusAsync(localSubscription, details.Status, details, rawPayload, cancellationToken);
        log.ProcessedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return ApiResponse.Ok("Webhook processado.");
    }

    private async Task MarkSubscriptionStatusAsync(TrainerSubscription subscription, string providerStatus, ProviderSubscriptionDetails? details, string paymentRawPayload, CancellationToken cancellationToken)
    {
        var normalizedStatus = providerStatus.Trim().ToLowerInvariant();
        subscription.LastPaymentStatus = normalizedStatus;
        subscription.MercadoPagoPayerId = details?.PayerId ?? subscription.MercadoPagoPayerId;
        subscription.UpdatedAt = DateTime.UtcNow;

        if (normalizedStatus is "authorized" or "approved")
        {
            subscription.Status = TrainerSubscriptionStatus.Active;
            subscription.StartDate = DateTime.UtcNow;
            subscription.EndDate = CalculateCycleEndDate(subscription.BillingCycle);

            var planPrice = subscription.PlatformPlanPriceId.HasValue
                ? await _db.PlatformPlanPrices.FindAsync([subscription.PlatformPlanPriceId.Value], cancellationToken)
                : null;
            var plan = await _db.PlatformPlans.FindAsync([subscription.PlatformPlanId], cancellationToken);

            var existingPayment = !string.IsNullOrWhiteSpace(details?.LastPaymentId)
                ? await _db.TrainerPayments.FirstOrDefaultAsync(
                    p => p.TrainerSubscriptionId == subscription.Id
                         && p.Provider == "MercadoPago"
                         && p.ProviderPaymentId == details!.LastPaymentId,
                    cancellationToken)
                : null;

            if (existingPayment == null)
            {
                _db.TrainerPayments.Add(new TrainerPayment
                {
                    TrainerId = subscription.TrainerId,
                    TrainerSubscriptionId = subscription.Id,
                    Amount = details?.LastPaymentAmount ?? planPrice?.Price ?? plan?.MonthlyPrice ?? 0,
                    Status = PaymentStatus.Approved,
                    Provider = "MercadoPago",
                    ProviderPaymentId = details?.LastPaymentId,
                    ProviderSubscriptionId = subscription.MercadoPagoSubscriptionId,
                    RawPayload = details?.RawPayload ?? paymentRawPayload,
                    PaidAt = details?.LastPaymentDate ?? DateTime.UtcNow
                });
            }
        }
        else if (normalizedStatus is "cancelled" or "paused")
        {
            subscription.Status = TrainerSubscriptionStatus.Canceled;
            subscription.EndDate = DateTime.UtcNow;
        }
        else if (normalizedStatus is "expired")
        {
            subscription.Status = TrainerSubscriptionStatus.Expired;
            subscription.EndDate = DateTime.UtcNow;
        }
    }

    private static DateTime CalculateCycleEndDate(BillingFrequency cycle)
    {
        var now = DateTime.UtcNow;
        return cycle switch
        {
            BillingFrequency.Quarterly => now.AddMonths(3),
            BillingFrequency.Yearly => now.AddMonths(12),
            _ => now.AddMonths(1)
        };
    }

    private static string? ExtractPayloadValue(JsonElement payload, string path)
    {
        try
        {
            var current = payload;
            foreach (var segment in path.Split('.'))
            {
                if (!current.TryGetProperty(segment, out current))
                {
                    return null;
                }
            }
            return current.ValueKind == JsonValueKind.String ? current.GetString() : current.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static SubscriptionResponse MapResponse(TrainerSubscription subscription, PlatformPlan plan, PlatformPlanPrice? planPrice, List<TrainerPayment> payments)
    {
        return new SubscriptionResponse
        {
            Id = subscription.Id,
            PlanName = plan.Name,
            MonthlyPrice = plan.MonthlyPrice,
            MaxActiveStudents = plan.MaxActiveStudents,
            Status = subscription.Status.ToString(),
            BillingCycle = subscription.BillingCycle.ToString(),
            CurrentCyclePrice = planPrice?.Price,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            MercadoPagoSubscriptionId = subscription.MercadoPagoSubscriptionId,
            CheckoutUrl = subscription.InitPoint,
            Payments = payments.Select(p => new PaymentHistoryItem
            {
                Id = p.Id,
                Amount = p.Amount,
                Status = p.Status.ToString(),
                Provider = p.Provider,
                PaidAt = p.PaidAt,
                CreatedAt = p.CreatedAt
            }).ToList()
        };
    }
}
