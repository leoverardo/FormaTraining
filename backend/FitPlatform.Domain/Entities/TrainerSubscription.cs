using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class TrainerSubscription : BaseEntity
{
    public Guid TrainerId { get; set; }
    public Guid? TrainerOnboardingId { get; set; }
    public Guid PlatformPlanId { get; set; }
    public Guid? PlatformPlanPriceId { get; set; }
    public BillingFrequency BillingCycle { get; set; } = BillingFrequency.Monthly;
    public TrainerSubscriptionStatus Status { get; set; } = TrainerSubscriptionStatus.Pending;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Provider { get; set; } = "AbacatePay";
    public int BaseAmountInCents { get; set; }
    public int CycleDiscountAmountInCents { get; set; }
    public int CouponDiscountAmountInCents { get; set; }
    public int FinalAmountInCents { get; set; }
    public string? CouponCodeApplied { get; set; }
    public string? AbacatePayCustomerId { get; set; }
    public string? AbacatePayCheckoutId { get; set; }
    public string? AbacatePaySubscriptionId { get; set; }
    public string? MercadoPagoSubscriptionId { get; set; }
    public string? MercadoPagoPayerId { get; set; }
    public string? InitPoint { get; set; }
    public string? LastPaymentStatus { get; set; }

    public Trainer Trainer { get; set; } = null!;
    public PlatformPlan PlatformPlan { get; set; } = null!;
    public PlatformPlanPrice? PlatformPlanPrice { get; set; }
    public ICollection<TrainerPayment> Payments { get; set; } = new List<TrainerPayment>();
}
