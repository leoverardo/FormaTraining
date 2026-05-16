using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Owner;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class OwnerDashboardService
{
    private readonly AppDbContext _db;

    public OwnerDashboardService(AppDbContext db) => _db = db;

    public async Task<ApiResponse<OwnerDashboardResponse>> GetDashboardAsync(int range = 30)
    {
        range = range is 7 or 30 or 90 ? range : 30;

        var now = DateTime.UtcNow;
        var periodStart = now.AddDays(-range);
        var todayStart = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var nextMonthStart = monthStart.AddMonths(1);
        var lastMonthStart = monthStart.AddMonths(-1);
        var yearStart = new DateTime(now.Year, 1, 1);
        var next30Days = now.Date.AddDays(30);

        // ── Carrega dados ──────────────────────────────────────────────────────
        var subscriptions = await _db.TrainerSubscriptions
            .AsNoTracking()
            .Include(ts => ts.Trainer).ThenInclude(t => t.User)
            .Include(ts => ts.PlatformPlan)
            .Include(ts => ts.PlatformPlanPrice)
            .ToListAsync();

        var payments = await _db.TrainerPayments
            .AsNoTracking()
            .Include(p => p.Trainer).ThenInclude(t => t.User)
            .Include(p => p.Subscription).ThenInclude(s => s.PlatformPlan)
            .ToListAsync();

        var trainers = await _db.Trainers
            .AsNoTracking()
            .Include(t => t.User)
            .Where(t => t.User.IsActive)
            .ToListAsync();

        var students = await _db.Students
            .AsNoTracking()
            .ToListAsync();

        var plans = await _db.PlatformPlans.AsNoTracking().ToListAsync();

        var leads = await _db.TrainerLeads
            .AsNoTracking()
            .Include(l => l.Trainer)
            .ToListAsync();

        var onboardings = await _db.TrainerOnboardings
            .AsNoTracking()
            .ToListAsync();

        var serviceOffers = await _db.TrainerServiceOffers
            .AsNoTracking()
            .ToListAsync();

        var serviceOrders = await _db.TrainerServiceOrders
            .AsNoTracking()
            .Include(o => o.Trainer)
            .Include(o => o.ServiceOffer)
            .ToListAsync();

        // ── Categorização de assinaturas ───────────────────────────────────────
        var activeSubscriptions = subscriptions.Where(s => s.Status == TrainerSubscriptionStatus.Active).ToList();
        var pendingSubscriptions = subscriptions.Where(s => s.Status == TrainerSubscriptionStatus.Pending).ToList();
        var expiredSubscriptions = subscriptions.Where(s => s.Status == TrainerSubscriptionStatus.Expired).ToList();
        var canceledSubscriptions = subscriptions.Where(s => s.Status == TrainerSubscriptionStatus.Canceled).ToList();

        var activeTrainerIds = activeSubscriptions.Select(s => s.TrainerId).Distinct().ToHashSet();
        var totalTrainers = trainers.Count;
        var activeTrainersCount = activeTrainerIds.Count;

        // ── Receita ────────────────────────────────────────────────────────────
        var approvedPayments = payments.Where(p => p.Status == PaymentStatus.Approved).ToList();
        var revenueThisMonth = approvedPayments.Where(p => p.PaidAt >= monthStart && p.PaidAt < nextMonthStart).Sum(p => p.Amount);
        var revenueLastMonth = approvedPayments.Where(p => p.PaidAt >= lastMonthStart && p.PaidAt < monthStart).Sum(p => p.Amount);
        var revenueToday = approvedPayments.Where(p => p.PaidAt >= todayStart && p.PaidAt < todayStart.AddDays(1)).Sum(p => p.Amount);
        var revenueThisYear = approvedPayments.Where(p => p.PaidAt >= yearStart).Sum(p => p.Amount);
        var revenueInPeriod = approvedPayments.Where(p => p.PaidAt >= periodStart).Sum(p => p.Amount);

        var mrr = activeSubscriptions.Sum(GetMonthlyEquivalent);
        var expectedRevenueNext30Days = activeSubscriptions
            .Where(s => s.EndDate.Date >= now.Date && s.EndDate.Date <= next30Days)
            .Sum(GetSubscriptionCycleAmount);

        var overdueAmount = payments.Where(p => p.Status is PaymentStatus.Pending or PaymentStatus.Rejected).Sum(p => p.Amount)
            + expiredSubscriptions.Sum(GetSubscriptionCycleAmount);

        var avgRevenuePerTrainer = activeTrainersCount > 0 ? Math.Round(revenueThisMonth / activeTrainersCount, 2) : 0m;
        var growth = revenueLastMonth == 0m
            ? (revenueThisMonth > 0m ? 100m : 0m)
            : Math.Round(((revenueThisMonth - revenueLastMonth) / revenueLastMonth) * 100m, 2);

        // ── Vencimentos ────────────────────────────────────────────────────────
        var expiringIn7 = activeSubscriptions.Count(s => s.EndDate.Date >= now.Date && s.EndDate.Date <= now.Date.AddDays(7));
        var expiringIn15 = activeSubscriptions.Count(s => s.EndDate.Date >= now.Date && s.EndDate.Date <= now.Date.AddDays(15));
        var expiringIn30 = activeSubscriptions.Count(s => s.EndDate.Date >= now.Date && s.EndDate.Date <= now.Date.AddDays(30));
        var newSubsInPeriod = subscriptions.Count(s => s.CreatedAt >= periodStart);

        var response = new OwnerDashboardResponse
        {
            Range = range,

            Summary = new OwnerSummaryDto
            {
                TotalTrainers = totalTrainers,
                ActiveTrainers = activeTrainersCount,
                InactiveTrainers = Math.Max(0, totalTrainers - activeTrainersCount),
                TrainersWithPublicPage = trainers.Count(t => t.PublicPageEnabled),
                TrainersInExplore = trainers.Count(t => t.PublicSearchEnabled),
                TrainersAcceptingStudents = trainers.Count(t => t.AcceptingStudents),
                TotalStudents = students.Count,
                ActiveStudents = students.Count(s => s.Status == StudentStatus.Active),
                TotalPlans = plans.Count,
                ActivePlans = plans.Count(p => p.Active),
                TotalSubscriptions = subscriptions.Count,
                ActiveSubscriptions = activeSubscriptions.Count,
                PendingSubscriptions = pendingSubscriptions.Count,
                ExpiredSubscriptions = expiredSubscriptions.Count,
                CanceledSubscriptions = canceledSubscriptions.Count
            },

            Revenue = new OwnerRevenueDto
            {
                MonthlyRecurringRevenue = Math.Round(mrr, 2),
                RevenueInPeriod = Math.Round(revenueInPeriod, 2),
                RevenueThisMonth = Math.Round(revenueThisMonth, 2),
                RevenueLastMonth = Math.Round(revenueLastMonth, 2),
                RevenueToday = Math.Round(revenueToday, 2),
                RevenueThisYear = Math.Round(revenueThisYear, 2),
                ExpectedRevenueNext30Days = Math.Round(expectedRevenueNext30Days, 2),
                OverdueAmount = Math.Round(overdueAmount, 2),
                AverageRevenuePerTrainer = avgRevenuePerTrainer,
                GrowthPercentageComparedToLastMonth = growth
            },

            Subscriptions = new OwnerSubscriptionsDto
            {
                Active = activeSubscriptions.Count,
                Pending = pendingSubscriptions.Count,
                Expired = expiredSubscriptions.Count,
                Canceled = canceledSubscriptions.Count,
                Trialing = 0,
                NewInPeriod = newSubsInPeriod,
                ExpiringIn7Days = expiringIn7,
                ExpiringIn15Days = expiringIn15,
                ExpiringIn30Days = expiringIn30
            },

            PlanDistribution = BuildPlanDistribution(activeSubscriptions, mrr),
            BillingCycleDistribution = BuildCycleDistribution(activeSubscriptions, mrr),
            UpcomingExpirations = BuildUpcomingExpirations(now, activeSubscriptions, next30Days),
            RecentPayments = BuildRecentPayments(payments),
            RecentTrainers = BuildRecentTrainers(trainers, subscriptions),
            NewTrainersInPeriod = trainers.Count(t => t.CreatedAt >= periodStart),

            Onboarding = BuildOnboardingMetrics(onboardings, periodStart),

            StudentMetrics = BuildStudentMetrics(students, activeSubscriptions, plans, periodStart),
            TopTrainersByStudents = BuildTopTrainersByStudents(students, trainers, activeSubscriptions, plans),

            LeadFunnel = BuildLeadFunnel(leads, trainers, periodStart),
            TopTrainersByLeads = BuildTopTrainersByLeads(leads),

            ServiceSales = BuildServiceSalesMetrics(serviceOffers, serviceOrders, periodStart),
            TopTrainersByB2C = BuildTopTrainersByB2C(serviceOrders),
            TopServicesByOrders = BuildTopServicesByOrders(serviceOrders),

            AttentionItems = BuildAttentionItems(now, subscriptions, payments, students, trainers, serviceOrders)
        };

        return ApiResponse<OwnerDashboardResponse>.Ok(response);
    }

    // ── Cálculos de assinatura ─────────────────────────────────────────────────

    private static decimal GetSubscriptionCycleAmount(TrainerSubscription s)
    {
        if (s.FinalAmountInCents > 0) return s.FinalAmountInCents / 100m;
        if (s.PlatformPlanPrice?.Price is { } price && price > 0) return price;
        var monthly = s.PlatformPlan.MonthlyPrice;
        return s.BillingCycle switch
        {
            BillingFrequency.Monthly => monthly,
            BillingFrequency.Quarterly => monthly * 3,
            BillingFrequency.Semiannual => monthly * 6,
            BillingFrequency.Yearly => monthly * 12,
            _ => monthly
        };
    }

    private static decimal GetMonthlyEquivalent(TrainerSubscription s)
    {
        var cycle = GetSubscriptionCycleAmount(s);
        return s.BillingCycle switch
        {
            BillingFrequency.Monthly => cycle,
            BillingFrequency.Quarterly => cycle / 3m,
            BillingFrequency.Semiannual => cycle / 6m,
            BillingFrequency.Yearly => cycle / 12m,
            _ => cycle
        };
    }

    // ── Builders — SaaS ───────────────────────────────────────────────────────

    private static List<PlanDistributionDto> BuildPlanDistribution(List<TrainerSubscription> activeSubs, decimal mrr)
    {
        if (activeSubs.Count == 0) return new();
        return activeSubs
            .GroupBy(s => new { s.PlatformPlanId, s.PlatformPlan.Name })
            .Select(g =>
            {
                var rev = g.Sum(GetMonthlyEquivalent);
                return new PlanDistributionDto
                {
                    PlanId = g.Key.PlatformPlanId,
                    PlanName = g.Key.Name,
                    ActiveSubscriptions = g.Count(),
                    Revenue = Math.Round(rev, 2),
                    Percentage = mrr == 0 ? 0 : Math.Round(rev / mrr * 100m, 2)
                };
            })
            .OrderByDescending(x => x.Revenue)
            .ToList();
    }

    private static List<BillingCycleDistributionDto> BuildCycleDistribution(List<TrainerSubscription> activeSubs, decimal mrr)
    {
        if (activeSubs.Count == 0) return new();
        return activeSubs
            .GroupBy(s => s.BillingCycle)
            .Select(g =>
            {
                var rev = g.Sum(GetMonthlyEquivalent);
                return new BillingCycleDistributionDto
                {
                    BillingCycle = g.Key.ToString(),
                    Count = g.Count(),
                    Revenue = Math.Round(rev, 2),
                    Percentage = mrr == 0 ? 0 : Math.Round(rev / mrr * 100m, 2)
                };
            })
            .OrderByDescending(x => x.Revenue)
            .ToList();
    }

    private static List<UpcomingExpirationDto> BuildUpcomingExpirations(
        DateTime now, List<TrainerSubscription> activeSubs, DateTime next30Days)
    {
        return activeSubs
            .Where(s => s.EndDate.Date >= now.Date && s.EndDate.Date <= next30Days)
            .OrderBy(s => s.EndDate)
            .Take(20)
            .Select(s => new UpcomingExpirationDto
            {
                SubscriptionId = s.Id,
                TrainerId = s.TrainerId,
                TrainerName = s.Trainer.User.Name,
                BrandName = s.Trainer.BrandName,
                PlanName = s.PlatformPlan.Name,
                BillingCycle = s.BillingCycle.ToString(),
                Amount = GetSubscriptionCycleAmount(s),
                Status = s.Status.ToString(),
                EndDate = s.EndDate,
                DaysRemaining = (int)(s.EndDate.Date - now.Date).TotalDays,
                Email = s.Trainer.User.Email,
                Phone = s.Trainer.Phone
            })
            .ToList();
    }

    private static List<RecentPaymentDto> BuildRecentPayments(List<TrainerPayment> payments)
    {
        return payments
            .OrderByDescending(p => p.PaidAt ?? p.CreatedAt)
            .Take(20)
            .Select(p => new RecentPaymentDto
            {
                PaymentId = p.Id,
                TrainerId = p.TrainerId,
                TrainerName = p.Trainer.User.Name,
                BrandName = p.Trainer.BrandName,
                PlanName = p.Subscription.PlatformPlan.Name,
                BillingCycle = p.Subscription.BillingCycle.ToString(),
                Amount = p.Amount,
                Status = p.Status.ToString(),
                Provider = p.Provider,
                PaidAt = p.PaidAt,
                CreatedAt = p.CreatedAt
            })
            .ToList();
    }

    private static List<RecentTrainerDto> BuildRecentTrainers(
        List<Trainer> trainers, List<TrainerSubscription> subscriptions)
    {
        return trainers
            .OrderByDescending(t => t.CreatedAt)
            .Take(10)
            .Select(t =>
            {
                var sub = subscriptions
                    .Where(s => s.TrainerId == t.Id)
                    .OrderByDescending(s => s.CreatedAt)
                    .FirstOrDefault();
                return new RecentTrainerDto
                {
                    TrainerId = t.Id,
                    Name = t.User.Name,
                    BrandName = t.BrandName,
                    Email = t.User.Email,
                    CreatedAt = t.CreatedAt,
                    CurrentPlan = sub?.PlatformPlan?.Name ?? "Sem plano",
                    SubscriptionStatus = sub?.Status.ToString() ?? "None"
                };
            })
            .ToList();
    }

    // ── Builder — Onboarding ───────────────────────────────────────────────────

    private static OwnerOnboardingDto BuildOnboardingMetrics(List<TrainerOnboarding> onboardings, DateTime periodStart)
    {
        var total = onboardings.Count;
        var completed = onboardings.Count(o => o.Status == OnboardingStatus.Completed);
        var completionRate = total > 0 ? Math.Round((decimal)completed / total * 100m, 1) : 0m;

        return new OwnerOnboardingDto
        {
            TotalAllTime = total,
            InPeriod = onboardings.Count(o => o.CreatedAt >= periodStart),
            Draft = onboardings.Count(o => o.Status == OnboardingStatus.Draft),
            WaitingPayment = onboardings.Count(o => o.Status == OnboardingStatus.WaitingPayment),
            PaymentApproved = onboardings.Count(o => o.Status == OnboardingStatus.PaymentApproved),
            AccountCreated = onboardings.Count(o => o.Status == OnboardingStatus.AccountCreated),
            Completed = completed,
            Canceled = onboardings.Count(o => o.Status == OnboardingStatus.Canceled),
            CompletionRate = completionRate
        };
    }

    // ── Builder — Alunos ──────────────────────────────────────────────────────

    private static OwnerStudentMetricsDto BuildStudentMetrics(
        List<Student> students,
        List<TrainerSubscription> activeSubscriptions,
        List<PlatformPlan> plans,
        DateTime periodStart)
    {
        var activeStudents = students.Where(s => s.Status == StudentStatus.Active).ToList();
        var activeTrainersCount = activeSubscriptions.Select(s => s.TrainerId).Distinct().Count();
        var avgPerTrainer = activeTrainersCount > 0
            ? Math.Round((decimal)activeStudents.Count / activeTrainersCount, 1)
            : 0m;

        var (atCapacity, nearCapacity) = GetCapacityMetrics(students, activeSubscriptions, plans);

        return new OwnerStudentMetricsDto
        {
            TotalStudents = students.Count,
            ActiveStudents = activeStudents.Count,
            InactiveStudents = students.Count(s => s.Status == StudentStatus.Inactive),
            AveragePerActiveTrainer = avgPerTrainer,
            TrainersAtCapacity = atCapacity,
            TrainersNearCapacity = nearCapacity,
            NewStudentsInPeriod = students.Count(s => s.CreatedAt >= periodStart)
        };
    }

    private static List<TrainerStudentRankingDto> BuildTopTrainersByStudents(
        List<Student> students,
        List<Trainer> trainers,
        List<TrainerSubscription> activeSubscriptions,
        List<PlatformPlan> plans)
    {
        var activeStudents = students.Where(s => s.Status == StudentStatus.Active).ToList();
        var studentsByTrainer = activeStudents.GroupBy(s => s.TrainerId).ToDictionary(g => g.Key, g => g.Count());
        var planById = plans.ToDictionary(p => p.Id, p => p.MaxActiveStudents);
        var trainerMaxStudents = activeSubscriptions
            .GroupBy(s => s.TrainerId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(s => s.CreatedAt)
                       .Select(s => planById.GetValueOrDefault(s.PlatformPlanId, 0))
                       .FirstOrDefault());

        return trainers
            .Select(t =>
            {
                var active = studentsByTrainer.GetValueOrDefault(t.Id, 0);
                var max = trainerMaxStudents.GetValueOrDefault(t.Id, 0);
                var occupancy = max > 0 ? Math.Round((decimal)active / max * 100m, 1) : 0m;
                return new TrainerStudentRankingDto
                {
                    TrainerId = t.Id,
                    BrandName = t.BrandName,
                    ActiveStudents = active,
                    MaxStudents = max,
                    OccupancyRate = occupancy
                };
            })
            .Where(x => x.ActiveStudents > 0)
            .OrderByDescending(x => x.ActiveStudents)
            .Take(10)
            .ToList();
    }

    private static (int atCapacity, int nearCapacity) GetCapacityMetrics(
        List<Student> students,
        List<TrainerSubscription> activeSubscriptions,
        List<PlatformPlan> plans)
    {
        var activeStudents = students.Where(s => s.Status == StudentStatus.Active).ToList();
        var studentsByTrainer = activeStudents.GroupBy(s => s.TrainerId).ToDictionary(g => g.Key, g => g.Count());
        var planById = plans.ToDictionary(p => p.Id, p => p.MaxActiveStudents);
        var trainerMaxStudents = activeSubscriptions
            .GroupBy(s => s.TrainerId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(s => s.CreatedAt)
                       .Select(s => planById.GetValueOrDefault(s.PlatformPlanId, 0))
                       .FirstOrDefault());

        int at = 0, near = 0;
        foreach (var (trainerId, max) in trainerMaxStudents)
        {
            if (max <= 0) continue;
            var current = studentsByTrainer.GetValueOrDefault(trainerId, 0);
            var ratio = (double)current / max;
            if (ratio >= 1.0) at++;
            else if (ratio >= 0.8) near++;
        }
        return (at, near);
    }

    // ── Builder — Funil de leads ───────────────────────────────────────────────

    private static OwnerLeadFunnelDto BuildLeadFunnel(
        List<TrainerLead> leads, List<Trainer> trainers, DateTime periodStart)
    {
        var total = leads.Count;
        var converted = leads.Count(l => l.Status == TrainerLeadStatus.Converted);
        var conversionRate = total > 0 ? Math.Round((decimal)converted / total * 100m, 1) : 0m;

        return new OwnerLeadFunnelDto
        {
            TotalAllTime = total,
            InPeriod = leads.Count(l => l.CreatedAt >= periodStart),
            NewLeads = leads.Count(l => l.Status == TrainerLeadStatus.New),
            ContactedLeads = leads.Count(l => l.Status == TrainerLeadStatus.Contacted),
            ConvertedLeads = converted,
            ArchivedLeads = leads.Count(l => l.Status == TrainerLeadStatus.Archived),
            ConversionRate = conversionRate,
            TrainersWithPublicPage = trainers.Count(t => t.PublicPageEnabled),
            TrainersInExplore = trainers.Count(t => t.PublicSearchEnabled),
            TrainersAcceptingStudents = trainers.Count(t => t.AcceptingStudents)
        };
    }

    private static List<TrainerLeadRankingDto> BuildTopTrainersByLeads(List<TrainerLead> leads)
    {
        return leads
            .GroupBy(l => new { l.TrainerId, l.Trainer.BrandName })
            .Select(g => new TrainerLeadRankingDto
            {
                TrainerId = g.Key.TrainerId,
                BrandName = g.Key.BrandName,
                TotalLeads = g.Count(),
                ConvertedLeads = g.Count(l => l.Status == TrainerLeadStatus.Converted)
            })
            .OrderByDescending(x => x.TotalLeads)
            .Take(10)
            .ToList();
    }

    // ── Builder — Vendas B2C ───────────────────────────────────────────────────

    private static OwnerServiceSalesDto BuildServiceSalesMetrics(
        List<TrainerServiceOffer> offers,
        List<TrainerServiceOrder> orders,
        DateTime periodStart)
    {
        var activePublic = offers.Where(o => o.IsActive && o.IsPublic).ToList();
        var trainersWithOffers = activePublic.Select(o => o.TrainerId).Distinct().Count();

        var inPeriod = orders.Where(o => o.CreatedAt >= periodStart).ToList();
        var approvedInPeriod = inPeriod.Where(o => o.Status == TrainerServiceOrderStatus.Approved).ToList();
        var volumeInPeriod = approvedInPeriod.Sum(o => o.Amount);
        var avgTicket = approvedInPeriod.Count > 0 ? Math.Round(volumeInPeriod / approvedInPeriod.Count, 2) : 0m;
        var approvalRate = inPeriod.Count > 0
            ? Math.Round((decimal)approvedInPeriod.Count / inPeriod.Count * 100m, 1)
            : 0m;

        var allApproved = orders.Where(o => o.Status == TrainerServiceOrderStatus.Approved).ToList();

        return new OwnerServiceSalesDto
        {
            TotalOffers = offers.Count,
            ActivePublicOffers = activePublic.Count,
            TrainersWithActiveOffers = trainersWithOffers,
            OrdersInPeriod = inPeriod.Count,
            ApprovedInPeriod = approvedInPeriod.Count,
            PendingInPeriod = inPeriod.Count(o => o.Status == TrainerServiceOrderStatus.PendingPayment),
            RejectedOrCancelledInPeriod = inPeriod.Count(o =>
                o.Status is TrainerServiceOrderStatus.Rejected
                    or TrainerServiceOrderStatus.Cancelled
                    or TrainerServiceOrderStatus.Expired),
            VolumeApprovedInPeriod = Math.Round(volumeInPeriod, 2),
            AverageTicket = avgTicket,
            ApprovalRate = approvalRate,
            PendingManualLinking = orders.Count(o =>
                o.RequiresManualStudentLinking && o.Status == TrainerServiceOrderStatus.Approved),
            TotalOrdersAllTime = orders.Count,
            VolumeApprovedAllTime = Math.Round(allApproved.Sum(o => o.Amount), 2)
        };
    }

    private static List<TrainerB2cRankingDto> BuildTopTrainersByB2C(List<TrainerServiceOrder> orders)
    {
        return orders
            .Where(o => o.Status == TrainerServiceOrderStatus.Approved)
            .GroupBy(o => new { o.TrainerId, o.Trainer.BrandName })
            .Select(g => new TrainerB2cRankingDto
            {
                TrainerId = g.Key.TrainerId,
                BrandName = g.Key.BrandName,
                ApprovedOrders = g.Count(),
                VolumeApproved = Math.Round(g.Sum(o => o.Amount), 2)
            })
            .OrderByDescending(x => x.VolumeApproved)
            .Take(10)
            .ToList();
    }

    private static List<ServiceRankingDto> BuildTopServicesByOrders(List<TrainerServiceOrder> orders)
    {
        return orders
            .Where(o => o.Status == TrainerServiceOrderStatus.Approved)
            .GroupBy(o => new { o.ServiceOfferId, o.ServiceTitleSnapshot, TrainerBrand = o.Trainer.BrandName })
            .Select(g => new ServiceRankingDto
            {
                OfferId = g.Key.ServiceOfferId,
                Title = g.Key.ServiceTitleSnapshot,
                TrainerBrandName = g.Key.TrainerBrand,
                ApprovedOrders = g.Count(),
                TotalVolume = Math.Round(g.Sum(o => o.Amount), 2)
            })
            .OrderByDescending(x => x.ApprovedOrders)
            .Take(10)
            .ToList();
    }

    // ── Builder — Alertas operacionais ────────────────────────────────────────

    private static List<AttentionItemDto> BuildAttentionItems(
        DateTime now,
        List<TrainerSubscription> subscriptions,
        List<TrainerPayment> payments,
        List<Student> students,
        List<Trainer> trainers,
        List<TrainerServiceOrder> serviceOrders)
    {
        var items = new List<AttentionItemDto>();

        // Assinaturas vencidas
        foreach (var s in subscriptions.Where(s => s.Status == TrainerSubscriptionStatus.Expired).Take(8))
        {
            items.Add(new AttentionItemDto
            {
                Type = "SubscriptionExpired",
                Title = "Assinatura vencida",
                Description = $"{s.Trainer.BrandName} está com assinatura vencida.",
                Severity = "High",
                TrainerId = s.TrainerId,
                SubscriptionId = s.Id,
                CreatedAt = s.UpdatedAt
            });
        }

        // Pagamentos rejeitados
        foreach (var p in payments.Where(p => p.Status == PaymentStatus.Rejected).OrderByDescending(p => p.CreatedAt).Take(6))
        {
            items.Add(new AttentionItemDto
            {
                Type = "PaymentRejected",
                Title = "Pagamento rejeitado",
                Description = $"{p.Trainer.BrandName} teve pagamento rejeitado de {p.Amount:C}.",
                Severity = "High",
                TrainerId = p.TrainerId,
                SubscriptionId = p.TrainerSubscriptionId,
                CreatedAt = p.CreatedAt
            });
        }

        // Pedidos B2C aguardando vinculação manual
        foreach (var o in serviceOrders
            .Where(o => o.RequiresManualStudentLinking && o.Status == TrainerServiceOrderStatus.Approved)
            .Take(8))
        {
            items.Add(new AttentionItemDto
            {
                Type = "B2cPendingLinking",
                Title = "Pedido aguardando vinculação",
                Description = $"{o.Trainer.BrandName}: '{o.ServiceTitleSnapshot}' pago, aguarda aluno.",
                Severity = "Medium",
                TrainerId = o.TrainerId,
                CreatedAt = o.PaidAt ?? o.CreatedAt
            });
        }

        // Pagamentos pendentes
        foreach (var p in payments.Where(p => p.Status == PaymentStatus.Pending).OrderByDescending(p => p.CreatedAt).Take(6))
        {
            items.Add(new AttentionItemDto
            {
                Type = "PaymentPending",
                Title = "Pagamento pendente",
                Description = $"{p.Trainer.BrandName} tem pagamento pendente de {p.Amount:C}.",
                Severity = "Medium",
                TrainerId = p.TrainerId,
                SubscriptionId = p.TrainerSubscriptionId,
                CreatedAt = p.CreatedAt
            });
        }

        // Assinaturas vencendo em 7 dias
        foreach (var s in subscriptions
            .Where(s => s.Status == TrainerSubscriptionStatus.Active
                && s.EndDate.Date >= now.Date
                && s.EndDate.Date <= now.Date.AddDays(7))
            .Take(6))
        {
            items.Add(new AttentionItemDto
            {
                Type = "SubscriptionExpiringSoon",
                Title = "Assinatura vencendo",
                Description = $"{s.Trainer.BrandName} vence em {(int)(s.EndDate.Date - now.Date).TotalDays} dia(s).",
                Severity = "Medium",
                TrainerId = s.TrainerId,
                SubscriptionId = s.Id,
                CreatedAt = s.UpdatedAt
            });
        }

        // Trainers sem plano ativo
        var activeSubTrainerIds = subscriptions
            .Where(s => s.Status == TrainerSubscriptionStatus.Active)
            .Select(s => s.TrainerId).ToHashSet();
        foreach (var t in trainers.Where(t => !activeSubTrainerIds.Contains(t.Id)).Take(6))
        {
            items.Add(new AttentionItemDto
            {
                Type = "NoActivePlan",
                Title = "Personal sem plano ativo",
                Description = $"{t.BrandName} não possui assinatura ativa.",
                Severity = "Medium",
                TrainerId = t.Id,
                CreatedAt = t.UpdatedAt
            });
        }

        // Trainers sem alunos ativos
        var activeStudentsByTrainer = students
            .Where(s => s.Status == StudentStatus.Active)
            .GroupBy(s => s.TrainerId)
            .ToDictionary(g => g.Key, g => g.Count());
        foreach (var t in trainers.Where(t => !activeStudentsByTrainer.ContainsKey(t.Id)).Take(5))
        {
            items.Add(new AttentionItemDto
            {
                Type = "TrainerWithoutStudents",
                Title = "Personal sem alunos ativos",
                Description = $"{t.BrandName} ainda não possui alunos ativos.",
                Severity = "Low",
                TrainerId = t.Id,
                CreatedAt = t.UpdatedAt
            });
        }

        return items
            .OrderByDescending(i => i.Severity == "High" ? 2 : i.Severity == "Medium" ? 1 : 0)
            .ThenByDescending(i => i.CreatedAt)
            .Take(20)
            .ToList();
    }
}
