using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Subscription;
using FitPlatform.Application.DTOs.Trainer;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FitPlatform.Infrastructure.Services;

public class TrainerService
{
    private readonly AppDbContext _db;
    private readonly FeedBuilderService _feedBuilder;
    private readonly IConfiguration _config;

    public TrainerService(AppDbContext db, FeedBuilderService feedBuilder, IConfiguration config)
    {
        _db = db;
        _feedBuilder = feedBuilder;
        _config = config;
    }

    public async Task<ApiResponse<TrainerDashboardResponse>> GetDashboardAsync(Guid trainerId)
    {
        var trainer = await _db.Trainers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == trainerId);
        if (trainer == null) return ApiResponse<TrainerDashboardResponse>.Fail("Trainer não encontrado.");

        var totalStudents = await _db.Students.CountAsync(s => s.TrainerId == trainerId);
        var activeStudents = await _db.Students.CountAsync(s => s.TrainerId == trainerId && s.Status == StudentStatus.Active);
        var totalWorkouts = await _db.Workouts.CountAsync(w => w.TrainerId == trainerId);
        var totalExercises = await _db.Exercises.CountAsync(e => e.TrainerId == trainerId);
        var totalPosts = await _db.Posts.CountAsync(p => p.TrainerId == trainerId && p.Status == PostStatus.Published);

        var weekStart = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek).Date;
        var checkInsThisWeek = await _db.StudentWeeklyCheckIns
            .CountAsync(c => c.TrainerId == trainerId && c.WeekStartDate >= weekStart);
        var missingCheckIns = activeStudents - checkInsThisWeek;

        var studentsNeedingAttention = await _db.Students
            .CountAsync(s => s.TrainerId == trainerId && s.MonitoringStatus == StudentMonitoringStatus.NeedsAttention);

        var subscription = await _db.TrainerSubscriptions
            .Include(ts => ts.PlatformPlan)
            .Include(ts => ts.PlatformPlanPrice)
            .Where(ts => ts.TrainerId == trainerId)
            .OrderByDescending(ts => ts.CreatedAt)
            .FirstOrDefaultAsync();

        var maxStudents = subscription?.PlatformPlan?.MaxActiveStudents ?? 0;
        var usagePct = maxStudents > 0 ? (double)activeStudents / maxStudents * 100 : 0;

        var recentActivities = await _feedBuilder.GetTrainerRecentActivityAsync(trainerId, 15);

        var frontendUrl = _config["FrontendUrl"] ?? "http://localhost:5173";
        var publicPageUrl = !string.IsNullOrEmpty(trainer.PublicSlug)
            ? $"{frontendUrl}/p/{trainer.PublicSlug}"
            : null;

        return ApiResponse<TrainerDashboardResponse>.Ok(new TrainerDashboardResponse
        {
            TrainerName = trainer.User.Name,
            BrandName = trainer.BrandName,
            TotalStudents = totalStudents,
            ActiveStudents = activeStudents,
            InactiveStudents = totalStudents - activeStudents,
            TotalWorkouts = totalWorkouts,
            TotalExercises = totalExercises,
            TotalPublishedPosts = totalPosts,
            PlanName = subscription?.PlatformPlan?.Name ?? "Sem plano",
            MaxActiveStudents = maxStudents,
            StudentsUsagePercentage = Math.Round(usagePct, 1),
            BillingCycle = subscription?.BillingCycle.ToString() ?? "",
            SubscriptionStatus = subscription?.Status.ToString() ?? "None",
            SubscriptionEndDate = subscription?.EndDate,
            CheckInsThisWeekCount = checkInsThisWeek,
            MissingCheckInsCount = Math.Max(0, missingCheckIns),
            StudentsNeedingAttentionCount = studentsNeedingAttention,
            PublicPageUrl = publicPageUrl,
            RecentActivities = recentActivities.Data ?? new()
        });
    }

    public async Task<ApiResponse<TrainerProfileResponse>> GetProfileAsync(Guid trainerId)
    {
        var trainer = await _db.Trainers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == trainerId);
        if (trainer == null) return ApiResponse<TrainerProfileResponse>.Fail("Trainer não encontrado.");
        return ApiResponse<TrainerProfileResponse>.Ok(MapProfile(trainer));
    }

    public async Task<ApiResponse<TrainerProfileResponse>> UpdateProfileAsync(Guid trainerId, TrainerProfileRequest request)
    {
        var trainer = await _db.Trainers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == trainerId);
        if (trainer == null) return ApiResponse<TrainerProfileResponse>.Fail("Trainer não encontrado.");

        trainer.User.Name = request.Name;
        trainer.User.UpdatedAt = DateTime.UtcNow;
        trainer.BrandName = request.BrandName;
        trainer.Phone = request.Phone;
        trainer.CPF = request.CPF;
        trainer.BirthDate = request.BirthDate;
        trainer.CREF = request.CREF;
        trainer.Bio = request.Bio;
        trainer.Specialties = request.Specialties;
        trainer.Instagram = request.Instagram;
        trainer.ProfilePhotoUrl = request.ProfilePhotoUrl;
        trainer.LogoUrl = request.LogoUrl;
        trainer.PrimaryColor = request.PrimaryColor;
        trainer.SecondaryColor = request.SecondaryColor;
        trainer.ZipCode = request.ZipCode;
        trainer.Street = request.Street;
        trainer.AddressNumber = request.AddressNumber;
        trainer.Complement = request.Complement;
        trainer.Neighborhood = request.Neighborhood;
        trainer.City = request.City;
        trainer.State = request.State;
        trainer.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ApiResponse<TrainerProfileResponse>.Ok(MapProfile(trainer));
    }

    public async Task<ApiResponse<SubscriptionResponse>> GetSubscriptionAsync(Guid trainerId)
    {
        var subscription = await _db.TrainerSubscriptions
            .Include(ts => ts.PlatformPlan)
            .Include(ts => ts.PlatformPlanPrice)
            .Include(ts => ts.Payments)
            .Where(ts => ts.TrainerId == trainerId)
            .OrderByDescending(ts => ts.CreatedAt)
            .FirstOrDefaultAsync();

        if (subscription == null) return ApiResponse<SubscriptionResponse>.Fail("Nenhuma assinatura encontrada.");

        return ApiResponse<SubscriptionResponse>.Ok(new SubscriptionResponse
        {
            Id = subscription.Id,
            PlanName = subscription.PlatformPlan.Name,
            MonthlyPrice = subscription.PlatformPlan.MonthlyPrice,
            MaxActiveStudents = subscription.PlatformPlan.MaxActiveStudents,
            Status = subscription.Status.ToString(),
            BillingCycle = subscription.BillingCycle.ToString(),
            CurrentCyclePrice = subscription.PlatformPlanPrice?.Price,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            MercadoPagoSubscriptionId = subscription.MercadoPagoSubscriptionId,
            CheckoutUrl = subscription.InitPoint,
            Payments = subscription.Payments.OrderByDescending(p => p.CreatedAt).Select(p => new PaymentHistoryItem
            {
                Id = p.Id,
                Amount = p.Amount,
                Status = p.Status.ToString(),
                Provider = p.Provider,
                PaidAt = p.PaidAt,
                CreatedAt = p.CreatedAt
            }).ToList()
        });
    }

    private static TrainerProfileResponse MapProfile(Domain.Entities.Trainer t) => new()
    {
        Id = t.Id,
        UserId = t.UserId,
        Name = t.User.Name,
        Email = t.User.Email,
        BrandName = t.BrandName,
        Phone = t.Phone,
        CPF = t.CPF,
        BirthDate = t.BirthDate,
        CREF = t.CREF,
        Bio = t.Bio,
        Specialties = t.Specialties,
        Instagram = t.Instagram,
        ProfilePhotoUrl = t.ProfilePhotoUrl,
        LogoUrl = t.LogoUrl,
        PrimaryColor = t.PrimaryColor,
        SecondaryColor = t.SecondaryColor,
        ZipCode = t.ZipCode,
        Street = t.Street,
        AddressNumber = t.AddressNumber,
        Complement = t.Complement,
        Neighborhood = t.Neighborhood,
        City = t.City,
        State = t.State
    };
}
