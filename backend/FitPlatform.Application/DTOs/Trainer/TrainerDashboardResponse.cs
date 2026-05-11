using FitPlatform.Application.DTOs.Feed;

namespace FitPlatform.Application.DTOs.Trainer;

public class TrainerDashboardResponse
{
    public string TrainerName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int ActiveStudents { get; set; }
    public int InactiveStudents { get; set; }
    public int TotalWorkouts { get; set; }
    public int TotalExercises { get; set; }
    public int TotalPublishedPosts { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public int MaxActiveStudents { get; set; }
    public double StudentsUsagePercentage { get; set; }
    public string BillingCycle { get; set; } = string.Empty;
    public string SubscriptionStatus { get; set; } = string.Empty;
    public DateTime? SubscriptionEndDate { get; set; }
    public int CheckInsThisWeekCount { get; set; }
    public int MissingCheckInsCount { get; set; }
    public int StudentsNeedingAttentionCount { get; set; }
    public string? PublicPageUrl { get; set; }
    public List<TrainerActivityDto> RecentActivities { get; set; } = new();
    public bool HasActiveSubscription { get; set; }
    public TrainerDashboardSubscriptionDto? Subscription { get; set; }
}

public class TrainerDashboardSubscriptionDto
{
    public Guid Id { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string BillingCycle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? EndDate { get; set; }
    public decimal? CurrentCyclePrice { get; set; }
    public string? MercadoPagoSubscriptionId { get; set; }
    public string? MercadoPagoPayerId { get; set; }
    public string? LastPaymentStatus { get; set; }
}
