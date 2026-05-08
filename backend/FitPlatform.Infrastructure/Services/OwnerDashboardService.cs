using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Owner;
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
        var totalTrainers = await _db.Trainers.CountAsync();
        var activeTrainers = await _db.TrainerSubscriptions
            .Where(ts => ts.Status == TrainerSubscriptionStatus.Active)
            .Select(ts => ts.TrainerId)
            .Distinct()
            .CountAsync();

        var totalStudents = await _db.Students.CountAsync();
        var activeStudents = await _db.Students.CountAsync(s => s.Status == StudentStatus.Active);
        var activeSubscriptions = await _db.TrainerSubscriptions.CountAsync(ts => ts.Status == TrainerSubscriptionStatus.Active);
        var plansCount = await _db.PlatformPlans.CountAsync(p => p.Active);

        var monthlyCount = await _db.TrainerSubscriptions
            .CountAsync(ts => ts.Status == TrainerSubscriptionStatus.Active && ts.BillingCycle == BillingFrequency.Monthly);
        var quarterlyCount = await _db.TrainerSubscriptions
            .CountAsync(ts => ts.Status == TrainerSubscriptionStatus.Active && ts.BillingCycle == BillingFrequency.Quarterly);
        var yearlyCount = await _db.TrainerSubscriptions
            .CountAsync(ts => ts.Status == TrainerSubscriptionStatus.Active && ts.BillingCycle == BillingFrequency.Yearly);

        var recentTrainers = await _db.Trainers
            .Include(t => t.User)
            .Include(t => t.Subscriptions).ThenInclude(s => s.PlatformPlan)
            .OrderByDescending(t => t.CreatedAt)
            .Take(10)
            .ToListAsync();

        var recentTrainerDtos = new List<RecentTrainerDto>();
        foreach (var t in recentTrainers)
        {
            var activeSub = t.Subscriptions.FirstOrDefault(s => s.Status == TrainerSubscriptionStatus.Active)
                         ?? t.Subscriptions.OrderByDescending(s => s.CreatedAt).FirstOrDefault();
            var studentCount = await _db.Students.CountAsync(s => s.TrainerId == t.Id && s.Status == StudentStatus.Active);

            recentTrainerDtos.Add(new RecentTrainerDto
            {
                Id = t.Id,
                Name = t.User.Name,
                BrandName = t.BrandName,
                SubscriptionStatus = activeSub?.Status.ToString() ?? "None",
                PlanName = activeSub?.PlatformPlan?.Name ?? "Sem plano",
                ActiveStudentsCount = studentCount,
                CreatedAt = t.CreatedAt
            });
        }

        return ApiResponse<OwnerDashboardResponse>.Ok(new OwnerDashboardResponse
        {
            TotalTrainers = totalTrainers,
            ActiveTrainers = activeTrainers,
            TotalStudents = totalStudents,
            ActiveStudents = activeStudents,
            ActiveSubscriptions = activeSubscriptions,
            PlatformPlansCount = plansCount,
            MonthlySubscriptionsCount = monthlyCount,
            QuarterlySubscriptionsCount = quarterlyCount,
            YearlySubscriptionsCount = yearlyCount,
            RecentTrainers = recentTrainerDtos
        });
    }
}
