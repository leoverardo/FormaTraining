namespace FitPlatform.Application.DTOs.Owner;

public class OwnerDashboardResponse
{
    public OwnerSummaryDto Summary { get; set; } = new();
    public OwnerRevenueDto Revenue { get; set; } = new();
    public OwnerSubscriptionsDto Subscriptions { get; set; } = new();
    public List<UpcomingExpirationDto> UpcomingExpirations { get; set; } = new();
    public List<RecentPaymentDto> RecentPayments { get; set; } = new();
    public List<PlanDistributionDto> PlanDistribution { get; set; } = new();
    public List<BillingCycleDistributionDto> BillingCycleDistribution { get; set; } = new();
    public List<RecentTrainerDto> RecentTrainers { get; set; } = new();
    public List<AttentionItemDto> AttentionItems { get; set; } = new();
}

public class OwnerSummaryDto
{
    public int TotalTrainers { get; set; }
    public int ActiveTrainers { get; set; }
    public int InactiveTrainers { get; set; }
    public int TotalStudents { get; set; }
    public int ActiveStudents { get; set; }
    public int TotalPlans { get; set; }
    public int ActivePlans { get; set; }
    public int TotalSubscriptions { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int PendingSubscriptions { get; set; }
    public int ExpiredSubscriptions { get; set; }
    public int CanceledSubscriptions { get; set; }
}

public class OwnerRevenueDto
{
    public decimal MonthlyRecurringRevenue { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal RevenueLastMonth { get; set; }
    public decimal RevenueToday { get; set; }
    public decimal RevenueThisYear { get; set; }
    public decimal ExpectedRevenueNext30Days { get; set; }
    public decimal OverdueAmount { get; set; }
    public decimal AverageRevenuePerTrainer { get; set; }
    public decimal GrowthPercentageComparedToLastMonth { get; set; }
}

public class OwnerSubscriptionsDto
{
    public int Active { get; set; }
    public int Pending { get; set; }
    public int Expired { get; set; }
    public int Canceled { get; set; }
    public int Trialing { get; set; }
    public int ExpiringIn7Days { get; set; }
    public int ExpiringIn15Days { get; set; }
    public int ExpiringIn30Days { get; set; }
}

public class UpcomingExpirationDto
{
    public Guid SubscriptionId { get; set; }
    public Guid TrainerId { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string BillingCycle { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime EndDate { get; set; }
    public int DaysRemaining { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

public class RecentPaymentDto
{
    public Guid PaymentId { get; set; }
    public Guid TrainerId { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string BillingCycle { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PlanDistributionDto
{
    public Guid PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public int ActiveSubscriptions { get; set; }
    public decimal Revenue { get; set; }
    public decimal Percentage { get; set; }
}

public class BillingCycleDistributionDto
{
    public string BillingCycle { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Revenue { get; set; }
    public decimal Percentage { get; set; }
}

public class RecentTrainerDto
{
    public Guid TrainerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CurrentPlan { get; set; } = string.Empty;
    public string SubscriptionStatus { get; set; } = string.Empty;
}

public class AttentionItemDto
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public Guid? TrainerId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
}
