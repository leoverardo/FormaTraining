namespace FitPlatform.Application.DTOs.Owner;

public class OwnerDashboardResponse
{
    public int TotalTrainers { get; set; }
    public int ActiveTrainers { get; set; }
    public int TotalStudents { get; set; }
    public int ActiveStudents { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int PlatformPlansCount { get; set; }
    public int MonthlySubscriptionsCount { get; set; }
    public int QuarterlySubscriptionsCount { get; set; }
    public int YearlySubscriptionsCount { get; set; }
    public List<RecentTrainerDto> RecentTrainers { get; set; } = new();
}

public class RecentTrainerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string SubscriptionStatus { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public int ActiveStudentsCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
