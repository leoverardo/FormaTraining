using FitPlatform.Domain.Enums;

namespace FitPlatform.Application.DTOs.Subscription;

public class CreateSubscriptionRequest
{
    public Guid PlatformPlanId { get; set; }
    public Guid? PlatformPlanPriceId { get; set; }
    public BillingFrequency BillingCycle { get; set; } = BillingFrequency.Monthly;
    public string? CouponCode { get; set; }
}
