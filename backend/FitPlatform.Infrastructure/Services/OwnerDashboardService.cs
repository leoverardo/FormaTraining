using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Owner;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FitPlatform.Infrastructure.Services;

public class OwnerDashboardService
{
    private readonly AppDbContext _db;
    private readonly ILogger<OwnerDashboardService> _logger;

    public OwnerDashboardService(AppDbContext db, ILogger<OwnerDashboardService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ApiResponse<OwnerDashboardResponse>> GetDashboardAsync(int range = 30)
    {
        try
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

        var totalTrainers = await _db.Trainers.AsNoTracking().CountAsync(t => t.User.IsActive);
        var totalStudents = await _db.Students.AsNoTracking().CountAsync();
        var activeStudents = await _db.Students.AsNoTracking().CountAsync(s => s.Status == StudentStatus.Active);
        var totalPlans = await _db.PlatformPlans.AsNoTracking().CountAsync();
        var activePlans = await _db.PlatformPlans.AsNoTracking().CountAsync(p => p.Active);

        var subscriptionStatusCounts = await _db.TrainerSubscriptions
            .AsNoTracking()
            .GroupBy(s => s.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync();

        var activeSubscriptionsCount = subscriptionStatusCounts.FirstOrDefault(x => x.Key == TrainerSubscriptionStatus.Active)?.Count ?? 0;
        var pendingSubscriptionsCount = subscriptionStatusCounts.FirstOrDefault(x => x.Key == TrainerSubscriptionStatus.Pending)?.Count ?? 0;
        var expiredSubscriptionsCount = subscriptionStatusCounts.FirstOrDefault(x => x.Key == TrainerSubscriptionStatus.Expired)?.Count ?? 0;
        var canceledSubscriptionsCount = subscriptionStatusCounts.FirstOrDefault(x => x.Key == TrainerSubscriptionStatus.Canceled)?.Count ?? 0;
        var totalSubscriptions = subscriptionStatusCounts.Sum(x => x.Count);

        var activeTrainerIds = await _db.TrainerSubscriptions
            .AsNoTracking()
            .Where(s => s.Status == TrainerSubscriptionStatus.Active)
            .Select(s => s.TrainerId)
            .Distinct()
            .ToListAsync();
        var activeTrainersCount = activeTrainerIds.Count;

        var approvedPayments = _db.TrainerPayments.AsNoTracking().Where(p => p.Status == PaymentStatus.Approved);
        var revenueThisMonth = await approvedPayments.Where(p => p.PaidAt >= monthStart && p.PaidAt < nextMonthStart).SumAsync(p => (decimal?)p.Amount) ?? 0m;
        var revenueLastMonth = await approvedPayments.Where(p => p.PaidAt >= lastMonthStart && p.PaidAt < monthStart).SumAsync(p => (decimal?)p.Amount) ?? 0m;
        var revenueToday = await approvedPayments.Where(p => p.PaidAt >= todayStart && p.PaidAt < todayStart.AddDays(1)).SumAsync(p => (decimal?)p.Amount) ?? 0m;
        var revenueThisYear = await approvedPayments.Where(p => p.PaidAt >= yearStart).SumAsync(p => (decimal?)p.Amount) ?? 0m;
        var revenueInPeriod = await approvedPayments.Where(p => p.PaidAt >= periodStart).SumAsync(p => (decimal?)p.Amount) ?? 0m;

        var activeSubSnapshots = await _db.TrainerSubscriptions
            .AsNoTracking()
            .Where(s => s.Status == TrainerSubscriptionStatus.Active)
            .Select(s => new SubscriptionSnapshot
            {
                Id = s.Id,
                TrainerId = s.TrainerId,
                PlatformPlanId = s.PlatformPlanId,
                PlanName = s.PlatformPlan.Name,
                PlanMonthlyPrice = s.PlatformPlan.MonthlyPrice,
                PlanMaxStudents = s.PlatformPlan.MaxActiveStudents,
                BillingCycle = s.BillingCycle,
                FinalAmountInCents = s.FinalAmountInCents,
                EndDate = s.EndDate,
                TrainerName = s.Trainer.User.Name,
                BrandName = s.Trainer.BrandName,
                Email = s.Trainer.User.Email,
                Phone = s.Trainer.Phone,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        var mrr = activeSubSnapshots.Sum(GetMonthlyEquivalent);
        var expectedRevenueNext30Days = activeSubSnapshots
            .Where(s => s.EndDate.Date >= now.Date && s.EndDate.Date <= next30Days)
            .Sum(GetSubscriptionCycleAmount);

        var overduePaymentsAmount = await _db.TrainerPayments
            .AsNoTracking()
            .Where(p => p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Rejected)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;
        var overdueAmount = overduePaymentsAmount + activeSubSnapshots.Where(s => s.EndDate < now.Date).Sum(GetSubscriptionCycleAmount);

        var avgRevenuePerTrainer = activeTrainersCount > 0 ? Math.Round(revenueThisMonth / activeTrainersCount, 2) : 0m;
        var growth = revenueLastMonth == 0m
            ? (revenueThisMonth > 0m ? 100m : 0m)
            : Math.Round(((revenueThisMonth - revenueLastMonth) / revenueLastMonth) * 100m, 2);

        var expiringIn7 = activeSubSnapshots.Count(s => s.EndDate.Date >= now.Date && s.EndDate.Date <= now.Date.AddDays(7));
        var expiringIn15 = activeSubSnapshots.Count(s => s.EndDate.Date >= now.Date && s.EndDate.Date <= now.Date.AddDays(15));
        var expiringIn30 = activeSubSnapshots.Count(s => s.EndDate.Date >= now.Date && s.EndDate.Date <= now.Date.AddDays(30));
        var newSubsInPeriod = await _db.TrainerSubscriptions.AsNoTracking().CountAsync(s => s.CreatedAt >= periodStart);

        var trainersWithPublicPage = await _db.Trainers.AsNoTracking().CountAsync(t => t.User.IsActive && t.PublicPageEnabled);
        var trainersInExplore = await _db.Trainers.AsNoTracking().CountAsync(t => t.User.IsActive && t.PublicSearchEnabled);
        var trainersAcceptingStudents = await _db.Trainers.AsNoTracking().CountAsync(t => t.User.IsActive && t.AcceptingStudents);
        var newTrainersInPeriod = await _db.Trainers.AsNoTracking().CountAsync(t => t.User.IsActive && t.CreatedAt >= periodStart);

            var recentPayments = await _db.TrainerPayments
            .AsNoTracking()
            .OrderByDescending(p => p.PaidAt ?? p.CreatedAt)
            .Take(20)
            .Select(p => new RecentPaymentDto
            {
                PaymentId = p.Id,
                TrainerId = p.TrainerId,
                TrainerName = p.Trainer != null && p.Trainer.User != null ? (p.Trainer.User.Name ?? "Treinador sem nome") : "Treinador indisponivel",
                BrandName = p.Trainer != null ? (p.Trainer.BrandName ?? "Sem marca") : "Sem marca",
                PlanName = p.Subscription != null && p.Subscription.PlatformPlan != null ? (p.Subscription.PlatformPlan.Name ?? "Plano indisponivel") : "Plano indisponivel",
                BillingCycle = p.Subscription != null ? p.Subscription.BillingCycle.ToString() : "Unknown",
                Amount = p.Amount,
                Status = p.Status.ToString(),
                Provider = p.Provider ?? "Unknown",
                PaidAt = p.PaidAt,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        var recentTrainers = await _db.Trainers
            .AsNoTracking()
            .Where(t => t.User.IsActive)
            .OrderByDescending(t => t.CreatedAt)
            .Take(10)
            .Select(t => new RecentTrainerDto
            {
                TrainerId = t.Id,
                Name = t.User.Name,
                BrandName = t.BrandName,
                Email = t.User.Email,
                CreatedAt = t.CreatedAt,
                CurrentPlan = t.Subscriptions.OrderByDescending(s => s.CreatedAt).Select(s => s.PlatformPlan.Name).FirstOrDefault() ?? "Sem plano",
                SubscriptionStatus = t.Subscriptions.OrderByDescending(s => s.CreatedAt).Select(s => s.Status.ToString()).FirstOrDefault() ?? "None"
            })
            .ToListAsync();

        var onboardingStats = await _db.TrainerOnboardings
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                InPeriod = g.Count(x => x.CreatedAt >= periodStart),
                Draft = g.Count(x => x.Status == OnboardingStatus.Draft),
                WaitingPayment = g.Count(x => x.Status == OnboardingStatus.WaitingPayment),
                PaymentApproved = g.Count(x => x.Status == OnboardingStatus.PaymentApproved),
                AccountCreated = g.Count(x => x.Status == OnboardingStatus.AccountCreated),
                Completed = g.Count(x => x.Status == OnboardingStatus.Completed),
                Canceled = g.Count(x => x.Status == OnboardingStatus.Canceled)
            })
            .FirstOrDefaultAsync();

        var leadStats = await _db.TrainerLeads
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                InPeriod = g.Count(x => x.CreatedAt >= periodStart),
                NewLeads = g.Count(x => x.Status == TrainerLeadStatus.New),
                Contacted = g.Count(x => x.Status == TrainerLeadStatus.Contacted),
                Converted = g.Count(x => x.Status == TrainerLeadStatus.Converted),
                Archived = g.Count(x => x.Status == TrainerLeadStatus.Archived)
            })
            .FirstOrDefaultAsync();

        var topTrainersByLeads = await _db.TrainerLeads
            .AsNoTracking()
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
            .ToListAsync();

        var serviceSalesStats = await _db.TrainerServiceOrders
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalOrders = g.Count(),
                VolumeApprovedAllTime = g.Where(x => x.Status == TrainerServiceOrderStatus.Approved).Sum(x => (decimal?)x.Amount) ?? 0m,
                PendingManualLinking = g.Count(x => x.RequiresManualStudentLinking && x.Status == TrainerServiceOrderStatus.Approved),
                OrdersInPeriod = g.Count(x => x.CreatedAt >= periodStart),
                ApprovedInPeriod = g.Count(x => x.CreatedAt >= periodStart && x.Status == TrainerServiceOrderStatus.Approved),
                PendingInPeriod = g.Count(x => x.CreatedAt >= periodStart && x.Status == TrainerServiceOrderStatus.PendingPayment),
                RejectedOrCancelledInPeriod = g.Count(x => x.CreatedAt >= periodStart && (x.Status == TrainerServiceOrderStatus.Rejected || x.Status == TrainerServiceOrderStatus.Cancelled || x.Status == TrainerServiceOrderStatus.Expired)),
                VolumeApprovedInPeriod = g.Where(x => x.CreatedAt >= periodStart && x.Status == TrainerServiceOrderStatus.Approved).Sum(x => (decimal?)x.Amount) ?? 0m
            })
            .FirstOrDefaultAsync();

        var totalOffers = await _db.TrainerServiceOffers.AsNoTracking().CountAsync();
        var activePublicOffers = await _db.TrainerServiceOffers.AsNoTracking().CountAsync(o => o.IsActive && o.IsPublic);
        var trainersWithOffers = await _db.TrainerServiceOffers.AsNoTracking().Where(o => o.IsActive && o.IsPublic).Select(o => o.TrainerId).Distinct().CountAsync();

        var topTrainersByB2C = await _db.TrainerServiceOrders
            .AsNoTracking()
            .Where(o => o.Status == TrainerServiceOrderStatus.Approved)
            .GroupBy(o => new { o.TrainerId, o.Trainer.BrandName })
            .Select(g => new TrainerB2cRankingDto
            {
                TrainerId = g.Key.TrainerId,
                BrandName = g.Key.BrandName,
                ApprovedOrders = g.Count(),
                VolumeApproved = Math.Round(g.Sum(x => x.Amount), 2)
            })
            .OrderByDescending(x => x.VolumeApproved)
            .Take(10)
            .ToListAsync();

        var topServicesByOrders = await _db.TrainerServiceOrders
            .AsNoTracking()
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
            .ToListAsync();

        var topTrainerStudentCounts = await _db.Students
            .AsNoTracking()
            .Where(s => s.Status == StudentStatus.Active)
            .GroupBy(s => new { s.TrainerId, s.Trainer.BrandName })
            .Select(g => new
            {
                g.Key.TrainerId,
                g.Key.BrandName,
                ActiveStudents = g.Count()
            })
            .ToListAsync();

        var latestPlanByTrainer = activeSubSnapshots
            .GroupBy(s => s.TrainerId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.CreatedAt).Select(x => x.PlanMaxStudents).FirstOrDefault());

        var topTrainersByStudents = topTrainerStudentCounts
            .Select(x =>
            {
                var maxStudents = latestPlanByTrainer.GetValueOrDefault(x.TrainerId, 0);
                return new TrainerStudentRankingDto
                {
                    TrainerId = x.TrainerId,
                    BrandName = x.BrandName,
                    ActiveStudents = x.ActiveStudents,
                    MaxStudents = maxStudents,
                    OccupancyRate = maxStudents > 0 ? Math.Round((decimal)x.ActiveStudents / maxStudents * 100m, 1) : 0m
                };
            })
            .OrderByDescending(x => x.ActiveStudents)
            .Take(10)
            .ToList();

        var studentsPerTrainer = await _db.Students
            .AsNoTracking()
            .Where(s => s.Status == StudentStatus.Active)
            .GroupBy(s => s.TrainerId)
            .Select(g => new { TrainerId = g.Key, ActiveStudents = g.Count() })
            .ToListAsync();

        var atCapacity = 0;
        var nearCapacity = 0;
        foreach (var snapshot in activeSubSnapshots.GroupBy(s => s.TrainerId).Select(g => g.OrderByDescending(x => x.CreatedAt).First()))
        {
            if (snapshot.PlanMaxStudents <= 0) continue;
            var activeByTrainer = studentsPerTrainer.FirstOrDefault(x => x.TrainerId == snapshot.TrainerId)?.ActiveStudents ?? 0;
            var ratio = (double)activeByTrainer / snapshot.PlanMaxStudents;
            if (ratio >= 1) atCapacity++;
            else if (ratio >= 0.8) nearCapacity++;
        }

        var averagePerActiveTrainer = activeTrainersCount > 0 ? Math.Round((decimal)activeStudents / activeTrainersCount, 1) : 0m;

        var upcomingExpirations = activeSubSnapshots
            .Where(s => s.EndDate.Date >= now.Date && s.EndDate.Date <= next30Days)
            .OrderBy(s => s.EndDate)
            .Take(20)
            .Select(s => new UpcomingExpirationDto
            {
                SubscriptionId = s.Id,
                TrainerId = s.TrainerId,
                TrainerName = s.TrainerName,
                BrandName = s.BrandName,
                PlanName = s.PlanName,
                BillingCycle = s.BillingCycle.ToString(),
                Amount = GetSubscriptionCycleAmount(s),
                Status = TrainerSubscriptionStatus.Active.ToString(),
                EndDate = s.EndDate,
                DaysRemaining = (int)(s.EndDate.Date - now.Date).TotalDays,
                Email = s.Email,
                Phone = s.Phone
            })
            .ToList();

        var planDistribution = activeSubSnapshots
            .GroupBy(s => new { s.PlatformPlanId, s.PlanName })
            .Select(g =>
            {
                var rev = g.Sum(GetMonthlyEquivalent);
                return new PlanDistributionDto
                {
                    PlanId = g.Key.PlatformPlanId,
                    PlanName = g.Key.PlanName,
                    ActiveSubscriptions = g.Count(),
                    Revenue = Math.Round(rev, 2),
                    Percentage = mrr == 0 ? 0 : Math.Round(rev / mrr * 100m, 2)
                };
            })
            .OrderByDescending(x => x.Revenue)
            .ToList();

        var cycleDistribution = activeSubSnapshots
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

        var attentionItems = BuildAttentionItems(activeSubSnapshots, now);

        var onboardingCompleted = onboardingStats?.Completed ?? 0;
        var onboardingTotal = onboardingStats?.Total ?? 0;
        var onboardingRate = onboardingTotal > 0 ? Math.Round((decimal)onboardingCompleted / onboardingTotal * 100m, 1) : 0m;

        var leadTotal = leadStats?.Total ?? 0;
        var leadConverted = leadStats?.Converted ?? 0;
        var leadRate = leadTotal > 0 ? Math.Round((decimal)leadConverted / leadTotal * 100m, 1) : 0m;

        var approvedInPeriod = serviceSalesStats?.ApprovedInPeriod ?? 0;
        var ordersInPeriod = serviceSalesStats?.OrdersInPeriod ?? 0;
        var volumeInPeriod = serviceSalesStats?.VolumeApprovedInPeriod ?? 0m;

            var response = new OwnerDashboardResponse
            {
            Range = range,
            Summary = new OwnerSummaryDto
            {
                TotalTrainers = totalTrainers,
                ActiveTrainers = activeTrainersCount,
                InactiveTrainers = Math.Max(0, totalTrainers - activeTrainersCount),
                TrainersWithPublicPage = trainersWithPublicPage,
                TrainersInExplore = trainersInExplore,
                TrainersAcceptingStudents = trainersAcceptingStudents,
                TotalStudents = totalStudents,
                ActiveStudents = activeStudents,
                TotalPlans = totalPlans,
                ActivePlans = activePlans,
                TotalSubscriptions = totalSubscriptions,
                ActiveSubscriptions = activeSubscriptionsCount,
                PendingSubscriptions = pendingSubscriptionsCount,
                ExpiredSubscriptions = expiredSubscriptionsCount,
                CanceledSubscriptions = canceledSubscriptionsCount
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
                Active = activeSubscriptionsCount,
                Pending = pendingSubscriptionsCount,
                Expired = expiredSubscriptionsCount,
                Canceled = canceledSubscriptionsCount,
                Trialing = 0,
                NewInPeriod = newSubsInPeriod,
                ExpiringIn7Days = expiringIn7,
                ExpiringIn15Days = expiringIn15,
                ExpiringIn30Days = expiringIn30
            },
            PlanDistribution = planDistribution,
            BillingCycleDistribution = cycleDistribution,
            UpcomingExpirations = upcomingExpirations,
            RecentPayments = recentPayments,
            RecentTrainers = recentTrainers,
            NewTrainersInPeriod = newTrainersInPeriod,
            Onboarding = new OwnerOnboardingDto
            {
                TotalAllTime = onboardingTotal,
                InPeriod = onboardingStats?.InPeriod ?? 0,
                Draft = onboardingStats?.Draft ?? 0,
                WaitingPayment = onboardingStats?.WaitingPayment ?? 0,
                PaymentApproved = onboardingStats?.PaymentApproved ?? 0,
                AccountCreated = onboardingStats?.AccountCreated ?? 0,
                Completed = onboardingCompleted,
                Canceled = onboardingStats?.Canceled ?? 0,
                CompletionRate = onboardingRate
            },
            StudentMetrics = new OwnerStudentMetricsDto
            {
                TotalStudents = totalStudents,
                ActiveStudents = activeStudents,
                InactiveStudents = totalStudents - activeStudents,
                AveragePerActiveTrainer = averagePerActiveTrainer,
                TrainersAtCapacity = atCapacity,
                TrainersNearCapacity = nearCapacity,
                NewStudentsInPeriod = await _db.Students.AsNoTracking().CountAsync(s => s.CreatedAt >= periodStart)
            },
            TopTrainersByStudents = topTrainersByStudents,
            LeadFunnel = new OwnerLeadFunnelDto
            {
                TotalAllTime = leadTotal,
                InPeriod = leadStats?.InPeriod ?? 0,
                NewLeads = leadStats?.NewLeads ?? 0,
                ContactedLeads = leadStats?.Contacted ?? 0,
                ConvertedLeads = leadConverted,
                ArchivedLeads = leadStats?.Archived ?? 0,
                ConversionRate = leadRate,
                TrainersWithPublicPage = trainersWithPublicPage,
                TrainersInExplore = trainersInExplore,
                TrainersAcceptingStudents = trainersAcceptingStudents
            },
            TopTrainersByLeads = topTrainersByLeads,
            ServiceSales = new OwnerServiceSalesDto
            {
                TotalOffers = totalOffers,
                ActivePublicOffers = activePublicOffers,
                TrainersWithActiveOffers = trainersWithOffers,
                OrdersInPeriod = ordersInPeriod,
                ApprovedInPeriod = approvedInPeriod,
                PendingInPeriod = serviceSalesStats?.PendingInPeriod ?? 0,
                RejectedOrCancelledInPeriod = serviceSalesStats?.RejectedOrCancelledInPeriod ?? 0,
                VolumeApprovedInPeriod = Math.Round(volumeInPeriod, 2),
                AverageTicket = approvedInPeriod > 0 ? Math.Round(volumeInPeriod / approvedInPeriod, 2) : 0m,
                ApprovalRate = ordersInPeriod > 0 ? Math.Round((decimal)approvedInPeriod / ordersInPeriod * 100m, 1) : 0m,
                PendingManualLinking = serviceSalesStats?.PendingManualLinking ?? 0,
                TotalOrdersAllTime = serviceSalesStats?.TotalOrders ?? 0,
                VolumeApprovedAllTime = Math.Round(serviceSalesStats?.VolumeApprovedAllTime ?? 0m, 2)
            },
            TopTrainersByB2C = topTrainersByB2C,
            TopServicesByOrders = topServicesByOrders,
            AttentionItems = attentionItems
            };

            return ApiResponse<OwnerDashboardResponse>.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Owner dashboard failed for range {Range}. Returning fallback payload.", range);

            return ApiResponse<OwnerDashboardResponse>.Ok(
                new OwnerDashboardResponse { Range = range is 7 or 30 or 90 ? range : 30 },
                "Dashboard indisponivel temporariamente. Retornando valores padrao.");
        }
    }

    private static decimal GetSubscriptionCycleAmount(SubscriptionSnapshot s)
    {
        if (s.FinalAmountInCents > 0) return s.FinalAmountInCents / 100m;
        var monthly = s.PlanMonthlyPrice;
        return s.BillingCycle switch
        {
            BillingFrequency.Monthly => monthly,
            BillingFrequency.Semiannual => monthly * 6,
            BillingFrequency.Yearly => monthly * 12,
            _ => monthly
        };
    }

    private static decimal GetMonthlyEquivalent(SubscriptionSnapshot s)
    {
        var cycle = GetSubscriptionCycleAmount(s);
        return s.BillingCycle switch
        {
            BillingFrequency.Monthly => cycle,
            BillingFrequency.Semiannual => cycle / 6m,
            BillingFrequency.Yearly => cycle / 12m,
            _ => cycle
        };
    }

    private static List<AttentionItemDto> BuildAttentionItems(IEnumerable<SubscriptionSnapshot> activeSubSnapshots, DateTime now)
    {
        return activeSubSnapshots
            .Where(s => s.EndDate.Date >= now.Date && s.EndDate.Date <= now.Date.AddDays(7))
            .OrderBy(s => s.EndDate)
            .Take(20)
            .Select(s => new AttentionItemDto
            {
                Type = "SubscriptionExpiringSoon",
                Title = "Assinatura vencendo",
                Description = $"{s.BrandName} vence em {(int)(s.EndDate.Date - now.Date).TotalDays} dia(s).",
                Severity = "Medium",
                TrainerId = s.TrainerId,
                SubscriptionId = s.Id,
                CreatedAt = s.EndDate
            })
            .ToList();
    }

    private sealed class SubscriptionSnapshot
    {
        public Guid Id { get; set; }
        public Guid TrainerId { get; set; }
        public Guid PlatformPlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal PlanMonthlyPrice { get; set; }
        public int PlanMaxStudents { get; set; }
        public BillingFrequency BillingCycle { get; set; }
        public int FinalAmountInCents { get; set; }
        public DateTime EndDate { get; set; }
        public string TrainerName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
