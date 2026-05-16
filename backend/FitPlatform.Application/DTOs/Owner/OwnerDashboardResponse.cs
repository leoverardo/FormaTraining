namespace FitPlatform.Application.DTOs.Owner;

public class OwnerDashboardResponse
{
    public int Range { get; set; } = 30;

    // Bloco 1 + 2: Visão geral e SaaS
    public OwnerSummaryDto Summary { get; set; } = new();
    public OwnerRevenueDto Revenue { get; set; } = new();
    public OwnerSubscriptionsDto Subscriptions { get; set; } = new();
    public List<PlanDistributionDto> PlanDistribution { get; set; } = new();
    public List<BillingCycleDistributionDto> BillingCycleDistribution { get; set; } = new();
    public List<UpcomingExpirationDto> UpcomingExpirations { get; set; } = new();
    public List<RecentPaymentDto> RecentPayments { get; set; } = new();

    // Bloco 3: Aquisição e ativação de trainers
    public int NewTrainersInPeriod { get; set; }
    public OwnerOnboardingDto Onboarding { get; set; } = new();
    public List<RecentTrainerDto> RecentTrainers { get; set; } = new();

    // Bloco 4: Base de alunos
    public OwnerStudentMetricsDto StudentMetrics { get; set; } = new();
    public List<TrainerStudentRankingDto> TopTrainersByStudents { get; set; } = new();

    // Bloco 5: Funil público
    public OwnerLeadFunnelDto LeadFunnel { get; set; } = new();
    public List<TrainerLeadRankingDto> TopTrainersByLeads { get; set; } = new();

    // Bloco 6: Vendas B2C
    public OwnerServiceSalesDto ServiceSales { get; set; } = new();
    public List<TrainerB2cRankingDto> TopTrainersByB2C { get; set; } = new();
    public List<ServiceRankingDto> TopServicesByOrders { get; set; } = new();

    // Bloco 7: Alertas operacionais
    public List<AttentionItemDto> AttentionItems { get; set; } = new();
}

// ─── Bloco 1/2: Visão geral e SaaS ───────────────────────────────────────────

public class OwnerSummaryDto
{
    public int TotalTrainers { get; set; }
    public int ActiveTrainers { get; set; }
    public int InactiveTrainers { get; set; }
    public int TrainersWithPublicPage { get; set; }
    public int TrainersInExplore { get; set; }
    public int TrainersAcceptingStudents { get; set; }
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
    public decimal RevenueInPeriod { get; set; }
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
    public int NewInPeriod { get; set; }
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

// ─── Bloco 3: Aquisição e ativação ────────────────────────────────────────────

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

public class OwnerOnboardingDto
{
    public int TotalAllTime { get; set; }
    public int InPeriod { get; set; }
    public int Draft { get; set; }
    public int WaitingPayment { get; set; }
    public int PaymentApproved { get; set; }
    public int AccountCreated { get; set; }
    public int Completed { get; set; }
    public int Canceled { get; set; }
    public decimal CompletionRate { get; set; }
}

// ─── Bloco 4: Base de alunos ──────────────────────────────────────────────────

public class OwnerStudentMetricsDto
{
    public int TotalStudents { get; set; }
    public int ActiveStudents { get; set; }
    public int InactiveStudents { get; set; }
    public decimal AveragePerActiveTrainer { get; set; }
    public int TrainersAtCapacity { get; set; }
    public int TrainersNearCapacity { get; set; }
    public int NewStudentsInPeriod { get; set; }
}

public class TrainerStudentRankingDto
{
    public Guid TrainerId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public int ActiveStudents { get; set; }
    public int MaxStudents { get; set; }
    public decimal OccupancyRate { get; set; }
}

// ─── Bloco 5: Funil público ───────────────────────────────────────────────────

public class OwnerLeadFunnelDto
{
    public int TotalAllTime { get; set; }
    public int InPeriod { get; set; }
    public int NewLeads { get; set; }
    public int ContactedLeads { get; set; }
    public int ConvertedLeads { get; set; }
    public int ArchivedLeads { get; set; }
    public decimal ConversionRate { get; set; }
    public int TrainersWithPublicPage { get; set; }
    public int TrainersInExplore { get; set; }
    public int TrainersAcceptingStudents { get; set; }
}

public class TrainerLeadRankingDto
{
    public Guid TrainerId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public int TotalLeads { get; set; }
    public int ConvertedLeads { get; set; }
}

// ─── Bloco 6: Vendas B2C ──────────────────────────────────────────────────────

public class OwnerServiceSalesDto
{
    public int TotalOffers { get; set; }
    public int ActivePublicOffers { get; set; }
    public int TrainersWithActiveOffers { get; set; }
    public int OrdersInPeriod { get; set; }
    public int ApprovedInPeriod { get; set; }
    public int PendingInPeriod { get; set; }
    public int RejectedOrCancelledInPeriod { get; set; }
    public decimal VolumeApprovedInPeriod { get; set; }
    public decimal AverageTicket { get; set; }
    public decimal ApprovalRate { get; set; }
    public int PendingManualLinking { get; set; }
    public int TotalOrdersAllTime { get; set; }
    public decimal VolumeApprovedAllTime { get; set; }
}

public class TrainerB2cRankingDto
{
    public Guid TrainerId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public int ApprovedOrders { get; set; }
    public decimal VolumeApproved { get; set; }
}

public class ServiceRankingDto
{
    public Guid OfferId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string TrainerBrandName { get; set; } = string.Empty;
    public int ApprovedOrders { get; set; }
    public decimal TotalVolume { get; set; }
}

// ─── Bloco 7: Alertas ─────────────────────────────────────────────────────────

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
