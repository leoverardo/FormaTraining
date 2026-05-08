using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Posts;
using FitPlatform.Application.DTOs.Schedule;
using FitPlatform.Application.DTOs.Workouts;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class StudentAreaService
{
    private readonly AppDbContext _db;
    private readonly FeedBuilderService _feedBuilder;

    public StudentAreaService(AppDbContext db, FeedBuilderService feedBuilder)
    {
        _db = db;
        _feedBuilder = feedBuilder;
    }

    public async Task<ApiResponse<object>> GetAccessStatusAsync(Guid studentId)
    {
        var student = await _db.Students.Include(s => s.Trainer).ThenInclude(t => t.Subscriptions)
            .FirstOrDefaultAsync(s => s.Id == studentId);

        if (student == null) return ApiResponse<object>.Fail("Aluno não encontrado.");

        if (student.Status == StudentStatus.Inactive)
            return ApiResponse<object>.Ok(new { Allowed = false, Reason = "Inactive", Message = "Seu acesso está inativo. Entre em contato com seu personal trainer." });

        var activeSubscription = student.Trainer.Subscriptions
            .Any(ts => ts.Status == TrainerSubscriptionStatus.Active);

        if (!activeSubscription)
            return ApiResponse<object>.Ok(new { Allowed = false, Reason = "TrainerSubscriptionExpired", Message = "O acesso à plataforma está temporariamente indisponível. Entre em contato com seu personal trainer." });

        return ApiResponse<object>.Ok(new { Allowed = true, Reason = "Active", Message = "Acesso liberado." });
    }

    private async Task<bool> IsAccessAllowedAsync(Guid studentId)
    {
        var student = await _db.Students.Include(s => s.Trainer).ThenInclude(t => t.Subscriptions)
            .FirstOrDefaultAsync(s => s.Id == studentId);

        if (student == null || student.Status == StudentStatus.Inactive) return false;

        return student.Trainer.Subscriptions.Any(ts => ts.Status == TrainerSubscriptionStatus.Active);
    }

    public async Task<ApiResponse<object>> GetDashboardAsync(Guid studentId)
    {
        if (!await IsAccessAllowedAsync(studentId))
            return ApiResponse<object>.Fail("Acesso bloqueado.");

        var student = await _db.Students
            .Include(s => s.User)
            .Include(s => s.Trainer).ThenInclude(t => t.User)
            .FirstOrDefaultAsync(s => s.Id == studentId);

        var today = (int)DateTime.UtcNow.DayOfWeek;
        var todaySchedule = await _db.StudentWorkoutSchedules
            .Include(s => s.Workout).ThenInclude(w => w.WorkoutExercises)
            .Where(s => s.StudentId == studentId && s.DayOfWeek == today)
            .FirstOrDefaultAsync();

        var weekSchedules = await _db.StudentWorkoutSchedules
            .Include(s => s.Workout)
            .Where(s => s.StudentId == studentId)
            .OrderBy(s => s.DayOfWeek)
            .ToListAsync();

        var latestProgress = await _db.StudentProgressRecords
            .Where(p => p.StudentId == studentId)
            .OrderByDescending(p => p.ProgressDate)
            .FirstOrDefaultAsync();

        var previousProgress = await _db.StudentProgressRecords
            .Where(p => p.StudentId == studentId)
            .OrderByDescending(p => p.ProgressDate)
            .Skip(1)
            .FirstOrDefaultAsync();

        var weekStart = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek).Date;
        var currentWeekCheckIn = await _db.StudentWeeklyCheckIns
            .FirstOrDefaultAsync(c => c.StudentId == studentId && c.WeekStartDate == weekStart);

        var latestPhoto = await _db.StudentProgressPhotos
            .Where(p => p.StudentId == studentId)
            .OrderByDescending(p => p.PhotoDate)
            .FirstOrDefaultAsync();

        var unreadNotifications = await _db.Notifications
            .CountAsync(n => n.UserId == student!.UserId && !n.IsRead);

        // Recent feed items (posts from trainer + student activities)
        var feedResult = await _feedBuilder.GetStudentFeedAsync(studentId, student!.UserId, null, 1, 5);
        var recentFeedItems = feedResult.Data ?? new();

        return ApiResponse<object>.Ok(new
        {
            StudentName = student!.User.Name,
            TrainerName = student.Trainer.User.Name,
            TrainerBrandName = student.Trainer.BrandName,
            TrainerAvatarUrl = student.Trainer.ProfilePhotoUrl,
            AccessStatus = "Active",
            TodayWorkout = todaySchedule != null ? new
            {
                todaySchedule.Workout.Id,
                todaySchedule.Workout.Name,
                ExerciseCount = todaySchedule.Workout.WorkoutExercises?.Count ?? 0,
                IsRestDay = false
            } : new object[] { }.Cast<object>().FirstOrDefault(),
            WeekSchedule = weekSchedules.Select(s => new
            {
                s.DayOfWeek,
                WorkoutName = s.Workout.Name,
                s.WorkoutId,
                IsRestDay = false
            }),
            LatestProgress = latestProgress != null ? new
            {
                latestProgress.Id,
                latestProgress.ProgressDate,
                latestProgress.Weight,
                latestProgress.Height,
                latestProgress.Waist,
                latestProgress.Chest,
                latestProgress.Abdomen,
                latestProgress.Notes
            } : null,
            LatestProgressComparison = latestProgress != null && previousProgress != null ? new
            {
                WeightDifference = latestProgress.Weight - previousProgress.Weight,
                WaistDifference = latestProgress.Waist - previousProgress.Waist,
                ChestDifference = latestProgress.Chest - previousProgress.Chest,
                AbdomenDifference = latestProgress.Abdomen - previousProgress.Abdomen,
                ComparedToDate = previousProgress.ProgressDate
            } : null,
            LatestProgressPhoto = latestPhoto != null ? new
            {
                latestPhoto.Id,
                latestPhoto.ImageUrl,
                latestPhoto.PhotoDate
            } : null,
            CurrentWeekCheckIn = currentWeekCheckIn != null ? new
            {
                currentWeekCheckIn.Id,
                currentWeekCheckIn.WeekStartDate,
                Completed = true,
                currentWeekCheckIn.MoodLevel,
                currentWeekCheckIn.EnergyLevel,
                currentWeekCheckIn.SleepQuality,
                currentWeekCheckIn.TrainingAdherence
            } : null,
            NotificationsCount = unreadNotifications,
            RecentFeedItems = recentFeedItems
        });
    }

    public async Task<ApiResponse<List<WorkoutResponse>>> GetWorkoutsAsync(Guid studentId)
    {
        if (!await IsAccessAllowedAsync(studentId))
            return ApiResponse<List<WorkoutResponse>>.Fail("Acesso bloqueado.");

        var schedules = await _db.StudentWorkoutSchedules
            .Include(s => s.Workout).ThenInclude(w => w.WorkoutExercises).ThenInclude(we => we.Exercise)
            .Where(s => s.StudentId == studentId)
            .Select(s => s.Workout)
            .Distinct()
            .ToListAsync();

        return ApiResponse<List<WorkoutResponse>>.Ok(schedules.Select(w => new WorkoutResponse
        {
            Id = w.Id,
            Name = w.Name,
            Goal = w.Goal,
            Level = w.Level.ToString(),
            Description = w.Description,
            Status = w.Status.ToString(),
            CreatedAt = w.CreatedAt,
            Exercises = w.WorkoutExercises.OrderBy(we => we.OrderIndex).Select(we => new WorkoutExerciseResponse
            {
                Id = we.Id,
                Sets = we.Sets,
                Reps = we.Reps,
                SuggestedLoad = we.SuggestedLoad,
                RestSeconds = we.RestSeconds,
                Notes = we.Notes,
                OrderIndex = we.OrderIndex,
                Exercise = ExerciseService.MapResponse(we.Exercise)
            }).ToList()
        }).ToList());
    }

    public async Task<ApiResponse<WorkoutResponse>> GetWorkoutByIdAsync(Guid studentId, Guid workoutId)
    {
        if (!await IsAccessAllowedAsync(studentId))
            return ApiResponse<WorkoutResponse>.Fail("Acesso bloqueado.");

        var inSchedule = await _db.StudentWorkoutSchedules
            .AnyAsync(s => s.StudentId == studentId && s.WorkoutId == workoutId);

        if (!inSchedule) return ApiResponse<WorkoutResponse>.Fail("Treino não encontrado na sua rotina.");

        var workout = await _db.Workouts
            .Include(w => w.WorkoutExercises).ThenInclude(we => we.Exercise)
            .FirstOrDefaultAsync(w => w.Id == workoutId);

        if (workout == null) return ApiResponse<WorkoutResponse>.Fail("Treino não encontrado.");

        return ApiResponse<WorkoutResponse>.Ok(new WorkoutResponse
        {
            Id = workout.Id,
            Name = workout.Name,
            Goal = workout.Goal,
            Level = workout.Level.ToString(),
            Description = workout.Description,
            Status = workout.Status.ToString(),
            CreatedAt = workout.CreatedAt,
            Exercises = workout.WorkoutExercises.OrderBy(we => we.OrderIndex).Select(we => new WorkoutExerciseResponse
            {
                Id = we.Id,
                Sets = we.Sets,
                Reps = we.Reps,
                SuggestedLoad = we.SuggestedLoad,
                RestSeconds = we.RestSeconds,
                Notes = we.Notes,
                OrderIndex = we.OrderIndex,
                Exercise = ExerciseService.MapResponse(we.Exercise)
            }).ToList()
        });
    }

    public async Task<ApiResponse<List<PostResponse>>> GetPostsAsync(Guid studentId)
    {
        if (!await IsAccessAllowedAsync(studentId))
            return ApiResponse<List<PostResponse>>.Fail("Acesso bloqueado.");

        var student = await _db.Students.FindAsync(studentId);
        var posts = await _db.Posts
            .Where(p => p.TrainerId == student!.TrainerId && p.Status == PostStatus.Published)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return ApiResponse<List<PostResponse>>.Ok(posts.Select(p => new PostResponse
        {
            Id = p.Id,
            Title = p.Title,
            Description = p.Description,
            ImageUrl = p.ImageUrl,
            VideoUrl = p.VideoUrl,
            Status = p.Status.ToString(),
            CreatedAt = p.CreatedAt
        }).ToList());
    }

    public async Task<ApiResponse<PostResponse>> GetPostByIdAsync(Guid studentId, Guid postId)
    {
        if (!await IsAccessAllowedAsync(studentId))
            return ApiResponse<PostResponse>.Fail("Acesso bloqueado.");

        var student = await _db.Students.FindAsync(studentId);
        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == postId && p.TrainerId == student!.TrainerId && p.Status == PostStatus.Published);
        if (post == null) return ApiResponse<PostResponse>.Fail("Post não encontrado.");

        return ApiResponse<PostResponse>.Ok(new PostResponse
        {
            Id = post.Id,
            Title = post.Title,
            Description = post.Description,
            ImageUrl = post.ImageUrl,
            VideoUrl = post.VideoUrl,
            Status = post.Status.ToString(),
            CreatedAt = post.CreatedAt
        });
    }
}
