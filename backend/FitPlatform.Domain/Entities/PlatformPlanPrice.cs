using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class PlatformPlanPrice : BaseEntity
{
    public Guid PlatformPlanId { get; set; }
    public BillingFrequency BillingCycle { get; set; }
    public decimal Price { get; set; }
    public bool Active { get; set; } = true;

    public PlatformPlan PlatformPlan { get; set; } = null!;
    public ICollection<TrainerSubscription> Subscriptions { get; set; } = new List<TrainerSubscription>();
}
