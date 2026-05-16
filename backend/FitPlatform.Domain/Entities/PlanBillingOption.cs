using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class PlanBillingOption : BaseEntity
{
    public Guid PlatformPlanId { get; set; }
    public BillingFrequency BillingCycle { get; set; }
    public int MonthsCount { get; set; }
    public decimal CycleDiscountPercent { get; set; }
    public int BasePriceInCents { get; set; }
    public int FinalPriceInCents { get; set; }
    public string? AbacatePayProductId { get; set; }
    public bool IsActive { get; set; } = true;

    public PlatformPlan PlatformPlan { get; set; } = null!;
}
