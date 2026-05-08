namespace FitPlatform.Application.DTOs.Plans;

public class PlatformPlanRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal MonthlyPrice { get; set; }
    public int MaxActiveStudents { get; set; }
    public bool Active { get; set; } = true;
}
