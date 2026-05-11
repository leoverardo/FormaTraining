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

    public async Task<ApiResponse<OwnerDashboardResponse>> GetDashboardAsync()
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var nextMonthStart = monthStart.AddMonths(1);
        var lastMonthStart = monthStart.AddMonths(-1);
        var yearStart = new DateTime(now.Year, 1, 1);
        var next30Days = now.Date.AddDays(30);

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
            .ToListAsync();

        var students = await _db.Students
            .AsNoTracking()
            .ToListAsync();

        var plans = await _db.PlatformPlans.AsNoTracking().ToListAsync();

        var activeSubscriptions = subscriptions.Where(s => s.Status == TrainerSubscriptionStatus.Active).ToList();
        var pendingSubscriptions = subscriptions.Where(s => s.Status == TrainerSubscriptionStatus.Pending).ToList();
        var expiredSubscriptions = subscriptions.Where(s => s.Status == TrainerSubscriptionStatus.Expired).ToList();
        var canceledSubscriptions = subscriptions.Where(s => s.Status == TrainerSubscriptionStatus.Canceled).ToList();

        var activeTrainerIds = activeSubscriptions.Select(s => s.TrainerId).Distinct().ToHashSet();
        var totalTrainers = trainers.Count;
        var activeTrainers = activeTrainerIds.Count;
        var inactiveTrainers = Math.Max(0, totalTrainers - activeTrainers);

        var approvedPayments = payments.Where(p => p.Status == PaymentStatus.Approved).ToList();
        var revenueThisMonth = approvedPayments.Where(p => p.PaidAt >= monthStart && p.PaidAt < nextMonthStart).Sum(p => p.Amount);
        var revenueLastMonth = approvedPayments.Where(p => p.PaidAt >= lastMonthStart && p.PaidAt < monthStart).Sum(p => p.Amount);
        var revenueToday = approvedPayments.Where(p => p.PaidAt >= todayStart && p.PaidAt < todayStart.AddDays(1)).Sum(p => p.Amount);
        var revenueThisYear = approvedPayments.Where(p => p.PaidAt >= yearStart).Sum(p => p.Amount);

        var mrr = activeSubscriptions.Sum(GetMonthlyEquivalent);
        var expectedRevenueNext30Days = activeSubscriptions
            .Where(s => s.EndDate.Date >= now.Date && s.EndDate.Date <= next30Days)
            .Sum(GetSubscriptionCycleAmount);

        var pendingRejectedPayments = payments.Where(p => p.Status is PaymentStatus.Pending or PaymentStatus.Rejected).ToList();
        var overdueAmount = pendingRejectedPayments.Sum(p => p.Amount)
            + expiredSubscriptions.Sum(GetSubscriptionCycleAmount);

        var avgRevenuePerTrainer = activeTrainers > 0 ? Math.Round(revenueThisMonth / activeTrainers, 2) : 0m;
        var growth = revenueLastMonth == 0m
            ? (revenueThisMonth > 0m ? 100m : 0m)
            : Math.Round(((revenueThisMonth - revenueLastMonth) / revenueLastMonth) * 100m, 2);

        var expiringIn7 = activeSubscriptions.Count(s => s.EndDate.Date >= now.Date && s.EndDate.Date <= now.Date.AddDays(7));
        var expiringIn15 = activeSubscriptions.Count(s => s.EndDate.Date >= now.Date && s.EndDate.Date <= now.Date.AddDays(15));
        var expiringIn30 = activeSubscriptions.Count(s => s.EndDate.Date >= now.Date && s.EndDate.Date <= now.Date.AddDays(30));

        var upcomingExpirations = activeSubscriptions
            .Where(s => s.EndDate.Date >= now.Date && s.EndDate.Date <= now.Date.AddDays(30))
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

        var recentPayments = payments
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

        var planDistribution = BuildPlanDistribution(activeSubscriptions, mrr);
        var cycleDistribution = BuildCycleDistribution(activeSubscriptions, mrr);

        var recentTrainers = trainers
            .OrderByDescending(t => t.CreatedAt)
            .Take(10)
            .Select(t =>
            {
                var trainerSub = subscriptions
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
                    CurrentPlan = trainerSub?.PlatformPlan?.Name ?? "Sem plano",
                    SubscriptionStatus = trainerSub?.Status.ToString() ?? "None"
                };
            })
            .ToList();

        var attentionItems = BuildAttentionItems(now, subscriptions, payments, students, trainers);

        var response = new OwnerDashboardResponse
        {
            Summary = new OwnerSummaryDto
            {
                TotalTrainers = totalTrainers,
                ActiveTrainers = activeTrainers,
                InactiveTrainers = inactiveTrainers,
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
                ExpiringIn7Days = expiringIn7,
                ExpiringIn15Days = expiringIn15,
                ExpiringIn30Days = expiringIn30
            },
            UpcomingExpirations = upcomingExpirations,
            RecentPayments = recentPayments,
            PlanDistribution = planDistribution,
            BillingCycleDistribution = cycleDistribution,
            RecentTrainers = recentTrainers,
            AttentionItems = attentionItems
        };

        return ApiResponse<OwnerDashboardResponse>.Ok(response);
    }

    private static decimal GetSubscriptionCycleAmount(TrainerSubscription subscription)
    {
        if (subscription.PlatformPlanPrice?.Price is { } price && price > 0) return price;
        var monthly = subscription.PlatformPlan.MonthlyPrice;
        return subscription.BillingCycle switch
        {
            BillingFrequency.Monthly => monthly,
            BillingFrequency.Quarterly => monthly * 3,
            BillingFrequency.Yearly => monthly * 12,
            _ => monthly
        };
    }

    private static decimal GetMonthlyEquivalent(TrainerSubscription subscription)
    {
        var cycleAmount = GetSubscriptionCycleAmount(subscription);
        return subscription.BillingCycle switch
        {
            BillingFrequency.Monthly => cycleAmount,
            BillingFrequency.Quarterly => cycleAmount / 3m,
            BillingFrequency.Yearly => cycleAmount / 12m,
            _ => cycleAmount
        };
    }

    private static List<PlanDistributionDto> BuildPlanDistribution(List<TrainerSubscription> activeSubscriptions, decimal mrr)
    {
        if (activeSubscriptions.Count == 0) return new();

        return activeSubscriptions
            .GroupBy(s => new { s.PlatformPlanId, s.PlatformPlan.Name })
            .Select(g =>
            {
                var groupRevenue = g.Sum(GetMonthlyEquivalent);
                return new PlanDistributionDto
                {
                    PlanId = g.Key.PlatformPlanId,
                    PlanName = g.Key.Name,
                    ActiveSubscriptions = g.Count(),
                    Revenue = Math.Round(groupRevenue, 2),
                    Percentage = mrr == 0 ? 0 : Math.Round((groupRevenue / mrr) * 100m, 2)
                };
            })
            .OrderByDescending(x => x.Revenue)
            .ToList();
    }

    private static List<BillingCycleDistributionDto> BuildCycleDistribution(List<TrainerSubscription> activeSubscriptions, decimal mrr)
    {
        if (activeSubscriptions.Count == 0) return new();

        return activeSubscriptions
            .GroupBy(s => s.BillingCycle)
            .Select(g =>
            {
                var groupRevenue = g.Sum(GetMonthlyEquivalent);
                return new BillingCycleDistributionDto
                {
                    BillingCycle = g.Key.ToString(),
                    Count = g.Count(),
                    Revenue = Math.Round(groupRevenue, 2),
                    Percentage = mrr == 0 ? 0 : Math.Round((groupRevenue / mrr) * 100m, 2)
                };
            })
            .OrderByDescending(x => x.Revenue)
            .ToList();
    }

    private static List<AttentionItemDto> BuildAttentionItems(
        DateTime now,
        List<TrainerSubscription> subscriptions,
        List<TrainerPayment> payments,
        List<Student> students,
        List<Trainer> trainers)
    {
        var items = new List<AttentionItemDto>();

        var expiredSubs = subscriptions.Where(s => s.Status == TrainerSubscriptionStatus.Expired).Take(10);
        foreach (var subscription in expiredSubs)
        {
            items.Add(new AttentionItemDto
            {
                Type = "SubscriptionExpired",
                Title = "Assinatura vencida",
                Description = $"{subscription.Trainer.BrandName} está com assinatura vencida.",
                Severity = "High",
                TrainerId = subscription.TrainerId,
                SubscriptionId = subscription.Id,
                CreatedAt = subscription.UpdatedAt
            });
        }

        var pendingPayments = payments.Where(p => p.Status == PaymentStatus.Pending).OrderByDescending(p => p.CreatedAt).Take(10);
        foreach (var payment in pendingPayments)
        {
            items.Add(new AttentionItemDto
            {
                Type = "PaymentPending",
                Title = "Pagamento pendente",
                Description = $"{payment.Trainer.BrandName} possui pagamento pendente de {payment.Amount:C}.",
                Severity = "Medium",
                TrainerId = payment.TrainerId,
                SubscriptionId = payment.TrainerSubscriptionId,
                CreatedAt = payment.CreatedAt
            });
        }

        var rejectedPayments = payments.Where(p => p.Status == PaymentStatus.Rejected).OrderByDescending(p => p.CreatedAt).Take(10);
        foreach (var payment in rejectedPayments)
        {
            items.Add(new AttentionItemDto
            {
                Type = "PaymentRejected",
                Title = "Pagamento rejeitado",
                Description = $"{payment.Trainer.BrandName} teve pagamento rejeitado.",
                Severity = "High",
                TrainerId = payment.TrainerId,
                SubscriptionId = payment.TrainerSubscriptionId,
                CreatedAt = payment.CreatedAt
            });
        }

        var expiringSoon = subscriptions
            .Where(s => s.Status == TrainerSubscriptionStatus.Active && s.EndDate.Date >= now.Date && s.EndDate.Date <= now.Date.AddDays(7))
            .Take(10);
        foreach (var subscription in expiringSoon)
        {
            items.Add(new AttentionItemDto
            {
                Type = "SubscriptionExpiringSoon",
                Title = "Assinatura vencendo",
                Description = $"{subscription.Trainer.BrandName} vence em {(subscription.EndDate.Date - now.Date).TotalDays:0} dia(s).",
                Severity = "Medium",
                TrainerId = subscription.TrainerId,
                SubscriptionId = subscription.Id,
                CreatedAt = subscription.UpdatedAt
            });
        }

        var activeSubTrainerIds = subscriptions
            .Where(s => s.Status == TrainerSubscriptionStatus.Active)
            .Select(s => s.TrainerId)
            .Distinct()
            .ToHashSet();

        foreach (var trainer in trainers.Where(t => !activeSubTrainerIds.Contains(t.Id)).Take(10))
        {
            items.Add(new AttentionItemDto
            {
                Type = "NoActivePlan",
                Title = "Personal sem plano ativo",
                Description = $"{trainer.BrandName} não possui assinatura ativa.",
                Severity = "Medium",
                TrainerId = trainer.Id,
                CreatedAt = trainer.UpdatedAt
            });
        }

        var studentsByTrainer = students
            .Where(s => s.Status == StudentStatus.Active)
            .GroupBy(s => s.TrainerId)
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var trainer in trainers.Take(20))
        {
            var activeStudents = studentsByTrainer.GetValueOrDefault(trainer.Id, 0);
            if (activeStudents == 0)
            {
                items.Add(new AttentionItemDto
                {
                    Type = "TrainerWithoutStudents",
                    Title = "Personal sem alunos ativos",
                    Description = $"{trainer.BrandName} ainda não possui alunos ativos.",
                    Severity = "Low",
                    TrainerId = trainer.Id,
                    CreatedAt = trainer.UpdatedAt
                });
            }
        }

        return items
            .OrderByDescending(i => i.CreatedAt)
            .Take(20)
            .ToList();
    }
}
