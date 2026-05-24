namespace FitPlatform.Application.DTOs.Plans;

public class PlatformPlanRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal MonthlyPrice { get; set; }
    public int MaxActiveStudents { get; set; }
    public bool HasUnlimitedStudents { get; set; }
    public bool Active { get; set; } = true;
    public bool IsPublic { get; set; } = true;
    public bool IsComingSoon { get; set; }
    public bool IsAvailableForPurchase { get; set; } = true;
}
