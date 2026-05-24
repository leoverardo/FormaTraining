using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Subscription;
using FitPlatform.Application.DTOs.Trainer;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FitPlatform.Infrastructure.Services;

public class TrainerService
{
    private readonly AppDbContext _db;
    private readonly FeedBuilderService _feedBuilder;
    private readonly AppointmentService _appointments;
    private readonly IConfiguration _config;
    private readonly ILogger<TrainerService> _logger;

    public TrainerService(AppDbContext db, FeedBuilderService feedBuilder, AppointmentService appointments, IConfiguration config, ILogger<TrainerService> logger)
    {
        _db = db;
        _feedBuilder = feedBuilder;
        _appointments = appointments;
        _config = config;
        _logger = logger;
    }

    public async Task<ApiResponse<TrainerDashboardResponse>> GetDashboardAsync(Guid trainerId)
    {
        try
        {
            var trainer = await _db.Trainers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == trainerId);
            if (trainer == null)
            {
                _logger.LogWarning("Dashboard requested for non-existing trainer. TrainerId: {TrainerId}", trainerId);
                return ApiResponse<TrainerDashboardResponse>.Fail("Trainer não encontrado.");
            }

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
            var (appointmentsToday, pendingConfirmations) = await _appointments.GetTrainerDashboardSummaryAsync(trainerId);

            var subscription = await _db.TrainerSubscriptions
                .Include(ts => ts.PlatformPlan)
                .Include(ts => ts.PlatformPlanPrice)
                .Where(ts => ts.TrainerId == trainerId)
                .OrderByDescending(ts => ts.CreatedAt)
                .FirstOrDefaultAsync();

            var hasUnlimitedStudents = subscription?.PlatformPlan?.HasUnlimitedStudents ?? false;
            var maxStudents = hasUnlimitedStudents ? 0 : (subscription?.PlatformPlan?.MaxActiveStudents ?? 0);
            var usagePct = maxStudents > 0 ? (double)activeStudents / maxStudents * 100 : 0;
            var hasActiveSubscription = subscription?.Status == TrainerSubscriptionStatus.Active;

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
                HasUnlimitedStudents = hasUnlimitedStudents,
                StudentsUsagePercentage = Math.Round(usagePct, 1),
                BillingCycle = subscription?.BillingCycle.ToString() ?? string.Empty,
                SubscriptionStatus = subscription?.Status.ToString() ?? "None",
                SubscriptionEndDate = subscription?.EndDate,
                CheckInsThisWeekCount = checkInsThisWeek,
                MissingCheckInsCount = Math.Max(0, missingCheckIns),
                StudentsNeedingAttentionCount = studentsNeedingAttention,
                AppointmentsTodayCount = appointmentsToday,
                PendingAppointmentConfirmationsCount = pendingConfirmations,
                PublicPageUrl = publicPageUrl,
                RecentActivities = recentActivities.Data ?? new(),
                HasActiveSubscription = hasActiveSubscription,
                Subscription = subscription == null
                    ? null
                    : new TrainerDashboardSubscriptionDto
                    {
                        Id = subscription.Id,
                        PlanName = subscription.PlatformPlan?.Name ?? "Sem plano",
                        BillingCycle = subscription.BillingCycle.ToString(),
                        Status = subscription.Status.ToString(),
                        EndDate = subscription.EndDate,
                        CurrentCyclePrice = subscription.PlatformPlanPrice?.Price,
                        MercadoPagoSubscriptionId = subscription.MercadoPagoSubscriptionId,
                        MercadoPagoPayerId = subscription.MercadoPagoPayerId,
                        LastPaymentStatus = subscription.LastPaymentStatus
                    }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while building trainer dashboard. TrainerId: {TrainerId}", trainerId);
            return ApiResponse<TrainerDashboardResponse>.Fail("Não foi possível carregar o dashboard.");
        }
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

        if (!string.IsNullOrWhiteSpace(request.ProfilePhotoUrl) || !string.IsNullOrWhiteSpace(request.LogoUrl))
            return ApiResponse<TrainerProfileResponse>.Fail("Envio por URL nÃ£o Ã© permitido. Use upload de mÃ­dia e informe os MediaIds.");

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
        if (request.ProfilePhotoMediaId.HasValue)
        {
            var profileMedia = await _db.MediaFiles.FirstOrDefaultAsync(m => m.Id == request.ProfilePhotoMediaId.Value);
            if (profileMedia == null) return ApiResponse<TrainerProfileResponse>.Fail("MÃ­dia de foto de perfil nÃ£o encontrada.");
            trainer.ProfilePhotoMediaId = profileMedia.Id;
            trainer.ProfilePhotoUrl = profileMedia.SecureUrl ?? profileMedia.Url;
        }
        if (request.LogoMediaId.HasValue)
        {
            var logoMedia = await _db.MediaFiles.FirstOrDefaultAsync(m => m.Id == request.LogoMediaId.Value);
            if (logoMedia == null) return ApiResponse<TrainerProfileResponse>.Fail("MÃ­dia de logo nÃ£o encontrada.");
            trainer.LogoMediaId = logoMedia.Id;
            trainer.LogoUrl = logoMedia.SecureUrl ?? logoMedia.Url;
        }
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
            HasUnlimitedStudents = subscription.PlatformPlan.HasUnlimitedStudents,
            Status = subscription.Status.ToString(),
            BillingCycle = subscription.BillingCycle.ToString(),
            CurrentCyclePrice = subscription.PlatformPlanPrice?.Price,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            MercadoPagoSubscriptionId = subscription.MercadoPagoSubscriptionId,
            AbacatePaySubscriptionId = subscription.AbacatePaySubscriptionId,
            AbacatePayCheckoutId = subscription.AbacatePayCheckoutId,
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
        ProfilePhotoMediaId = t.ProfilePhotoMediaId,
        LogoUrl = t.LogoUrl,
        LogoMediaId = t.LogoMediaId,
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
