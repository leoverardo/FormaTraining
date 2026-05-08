using FitPlatform.Domain.Common;

namespace FitPlatform.Domain.Entities;

public class PlatformFeature : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Active { get; set; } = true;

    public ICollection<PlatformPlanFeature> PlanFeatures { get; set; } = new List<PlatformPlanFeature>();
}

public class PlatformPlanFeature : BaseEntity
{
    public Guid PlatformPlanId { get; set; }
    public Guid PlatformFeatureId { get; set; }
    public bool Enabled { get; set; } = true;

    public PlatformPlan PlatformPlan { get; set; } = null!;
    public PlatformFeature Feature { get; set; } = null!;
}
