namespace FitPlatform.Application.DTOs.Plans;

public class PlatformPlanResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal MonthlyPrice { get; set; }
    public int MaxActiveStudents { get; set; }
    public bool HasUnlimitedStudents { get; set; }
    public bool Active { get; set; }
    public bool IsPublic { get; set; }
    public bool IsComingSoon { get; set; }
    public bool IsAvailableForPurchase { get; set; }
    public List<PlanPriceResponse> Prices { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class PlanPriceResponse
{
    public Guid Id { get; set; }
    public string BillingCycle { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool Active { get; set; }
}
