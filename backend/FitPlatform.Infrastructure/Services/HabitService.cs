using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Habits;
using FitPlatform.Application.Interfaces;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class HabitService
{
    private readonly AppDbContext _db;
    private readonly NotificationService _notifications;
    private readonly IDateTimeProvider _clock;
    private readonly GamificationService _gamification;

    public HabitService(AppDbContext db, NotificationService notifications, IDateTimeProvider clock, GamificationService gamification)
    {
        _db = db;
        _notifications = notifications;
        _clock = clock;
        _gamification = gamification;
    }

    private async Task<Student?> GetTrainerStudentAsync(Guid trainerId, Guid studentId) =>
        await _db.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == studentId && s.TrainerId == trainerId);

    public async Task<ApiResponse<List<StudentHabitResponse>>> GetTrainerHabitsAsync(Guid trainerId, Guid studentId)
    {
        if (await GetTrainerStudentAsync(trainerId, studentId) == null)
            return ApiResponse<List<StudentHabitResponse>>.Fail("Aluno não encontrado.");

        var items = await _db.StudentHabits
            .Where(h => h.StudentId == studentId && h.TrainerId == trainerId)
            .OrderByDescending(h => h.IsActive)
            .ThenBy(h => h.Title)
            .ToListAsync();

        return ApiResponse<List<StudentHabitResponse>>.Ok(items.Select(MapHabit).ToList());
    }

    public async Task<ApiResponse<StudentHabitResponse>> CreateHabitAsync(Guid trainerId, Guid studentId, StudentHabitRequest request)
    {
        var student = await GetTrainerStudentAsync(trainerId, studentId);
        if (student == null) return ApiResponse<StudentHabitResponse>.Fail("Aluno não encontrado.");

        var habit = new StudentHabit
        {
            StudentId = studentId,
            TrainerId = trainerId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Category = ParseCategory(request.Category),
            Frequency = HabitFrequency.Daily,
            TargetValue = request.TargetValue,
            TargetUnit = request.TargetUnit?.Trim(),
            IsActive = true,
            InactivatedAt = null
        };
        _db.StudentHabits.Add(habit);
        await _db.SaveChangesAsync();

        await _notifications.CreateAsync(student.UserId, "Novo hábito configurado", $"Seu personal adicionou o hábito: {habit.Title}.", NotificationType.HabitUpdated, trainerId, studentId);
        return ApiResponse<StudentHabitResponse>.Ok(MapHabit(habit), "Hábito criado.");
    }

    public async Task<ApiResponse<StudentHabitResponse>> UpdateHabitAsync(Guid trainerId, Guid studentId, Guid habitId, StudentHabitRequest request)
    {
        var habit = await _db.StudentHabits.FirstOrDefaultAsync(h => h.Id == habitId && h.StudentId == studentId && h.TrainerId == trainerId);
        if (habit == null) return ApiResponse<StudentHabitResponse>.Fail("Hábito não encontrado.");

        habit.Title = request.Title.Trim();
        habit.Description = request.Description?.Trim();
        habit.Category = ParseCategory(request.Category);
        habit.TargetValue = request.TargetValue;
        habit.TargetUnit = request.TargetUnit?.Trim();
        habit.UpdatedAt = _clock.UtcNow;
        await _db.SaveChangesAsync();

        var studentUserId = await _db.Students.Where(s => s.Id == studentId).Select(s => s.UserId).FirstAsync();
        await _notifications.CreateAsync(studentUserId, "Hábito atualizado", $"Seu personal atualizou o hábito: {habit.Title}.", NotificationType.HabitUpdated, trainerId, studentId);
        return ApiResponse<StudentHabitResponse>.Ok(MapHabit(habit), "Hábito atualizado.");
    }

    public async Task<ApiResponse<StudentHabitResponse>> UpdateHabitStatusAsync(Guid trainerId, Guid studentId, Guid habitId, StudentHabitStatusRequest request)
    {
        var habit = await _db.StudentHabits.FirstOrDefaultAsync(h => h.Id == habitId && h.StudentId == studentId && h.TrainerId == trainerId);
        if (habit == null) return ApiResponse<StudentHabitResponse>.Fail("Hábito não encontrado.");

        habit.IsActive = request.IsActive;
        habit.InactivatedAt = request.IsActive ? null : _clock.LocalDate.AddDays(-1);
        habit.UpdatedAt = _clock.UtcNow;
        await _db.SaveChangesAsync();
        return ApiResponse<StudentHabitResponse>.Ok(MapHabit(habit));
    }

    public async Task<ApiResponse> DeleteHabitAsync(Guid trainerId, Guid studentId, Guid habitId)
    {
        var habit = await _db.StudentHabits.FirstOrDefaultAsync(h => h.Id == habitId && h.StudentId == studentId && h.TrainerId == trainerId);
        if (habit == null) return ApiResponse.Fail("Hábito não encontrado.");

        habit.IsActive = false;
        habit.InactivatedAt = _clock.LocalDate.AddDays(-1);
        habit.UpdatedAt = _clock.UtcNow;
        await _db.SaveChangesAsync();
        return ApiResponse.Ok("Hábito arquivado.");
    }

    public async Task<ApiResponse<HabitAdherenceResponse>> GetAdherenceAsync(Guid trainerId, Guid studentId, int days)
    {
        if (await GetTrainerStudentAsync(trainerId, studentId) == null)
            return ApiResponse<HabitAdherenceResponse>.Fail("Aluno não encontrado.");
        return ApiResponse<HabitAdherenceResponse>.Ok(await BuildAdherenceAsync(studentId, days));
    }

    public async Task<ApiResponse<StudentNutritionGuidanceResponse?>> GetTrainerGuidanceAsync(Guid trainerId, Guid studentId)
    {
        if (await GetTrainerStudentAsync(trainerId, studentId) == null)
            return ApiResponse<StudentNutritionGuidanceResponse?>.Fail("Aluno não encontrado.");

        var item = await _db.StudentNutritionGuidances.FirstOrDefaultAsync(n => n.StudentId == studentId && n.TrainerId == trainerId);
        return ApiResponse<StudentNutritionGuidanceResponse?>.Ok(item == null ? null : MapGuidance(item));
    }

    public async Task<ApiResponse<StudentNutritionGuidanceResponse>> UpsertTrainerGuidanceAsync(Guid trainerId, Guid studentId, StudentNutritionGuidanceRequest request)
    {
        var student = await GetTrainerStudentAsync(trainerId, studentId);
        if (student == null) return ApiResponse<StudentNutritionGuidanceResponse>.Fail("Aluno não encontrado.");

        var item = await _db.StudentNutritionGuidances.FirstOrDefaultAsync(n => n.StudentId == studentId);
        if (item == null)
        {
            item = new StudentNutritionGuidance
            {
                StudentId = studentId,
                TrainerId = trainerId,
                GuidanceText = request.GuidanceText.Trim(),
                StrategicNotes = request.StrategicNotes?.Trim(),
                MediaId = request.MediaId
            };
            _db.StudentNutritionGuidances.Add(item);
        }
        else
        {
            item.GuidanceText = request.GuidanceText.Trim();
            item.StrategicNotes = request.StrategicNotes?.Trim();
            item.MediaId = request.MediaId;
            item.UpdatedAt = _clock.UtcNow;
        }
        await _db.SaveChangesAsync();

        await _notifications.CreateAsync(student.UserId, "Orientação alimentar atualizada", "Seu personal atualizou sua orientação alimentar.", NotificationType.NutritionGuidanceUpdated, trainerId, studentId);
        return ApiResponse<StudentNutritionGuidanceResponse>.Ok(MapGuidance(item));
    }

    public async Task<ApiResponse<StudentHabitTodayResponse>> GetStudentTodayAsync(Guid studentId)
    {
        var today = _clock.LocalDate;
        var habits = await _db.StudentHabits
            .Where(h => h.StudentId == studentId
                && h.IsActive
                && h.CreatedAt.Date <= today
                && (!h.InactivatedAt.HasValue || h.InactivatedAt.Value.Date >= today))
            .OrderBy(h => h.Title)
            .ToListAsync();

        var logs = await _db.StudentHabitLogs
            .Where(l => l.StudentId == studentId && l.Date == today)
            .ToDictionaryAsync(l => l.HabitId, l => l);

        var items = habits.Select(h =>
        {
            logs.TryGetValue(h.Id, out var log);
            return new StudentHabitTodayItemResponse
            {
                HabitId = h.Id,
                Title = h.Title,
                Description = h.Description,
                Category = h.Category.ToString(),
                TargetValue = h.TargetValue,
                TargetUnit = h.TargetUnit,
                IsCompleted = log?.IsCompleted ?? false,
                Value = log?.Value,
                Note = log?.Note,
                CompletedAt = log?.CompletedAt
            };
        }).ToList();

        return ApiResponse<StudentHabitTodayResponse>.Ok(new StudentHabitTodayResponse
        {
            Date = today,
            TotalHabits = items.Count,
            CompletedHabits = items.Count(i => i.IsCompleted),
            Items = items
        });
    }

    public async Task<ApiResponse<StudentHabitTodayItemResponse>> UpsertStudentTodayAsync(Guid studentId, Guid habitId, StudentHabitTodayUpdateRequest request)
    {
        var today = _clock.LocalDate;
        var habit = await _db.StudentHabits.FirstOrDefaultAsync(h =>
            h.Id == habitId
            && h.StudentId == studentId
            && h.IsActive
            && h.CreatedAt.Date <= today
            && (!h.InactivatedAt.HasValue || h.InactivatedAt.Value.Date >= today));
        if (habit == null) return ApiResponse<StudentHabitTodayItemResponse>.Fail("Hábito não encontrado.");

        var log = await _db.StudentHabitLogs.FirstOrDefaultAsync(l => l.HabitId == habitId && l.Date == today);
        if (log == null)
        {
            log = new StudentHabitLog
            {
                HabitId = habitId,
                StudentId = studentId,
                Date = today
            };
            _db.StudentHabitLogs.Add(log);
        }

        log.IsCompleted = request.IsCompleted;
        log.Value = request.Value;
        log.Note = request.Note?.Trim();
        log.CompletedAt = request.IsCompleted ? _clock.UtcNow : null;
        log.UpdatedAt = _clock.UtcNow;
        await _db.SaveChangesAsync();
        await _gamification.EvaluateForHabitUpdatedAsync(studentId);

        return ApiResponse<StudentHabitTodayItemResponse>.Ok(new StudentHabitTodayItemResponse
        {
            HabitId = habit.Id,
            Title = habit.Title,
            Description = habit.Description,
            Category = habit.Category.ToString(),
            TargetValue = habit.TargetValue,
            TargetUnit = habit.TargetUnit,
            IsCompleted = log.IsCompleted,
            Value = log.Value,
            Note = log.Note,
            CompletedAt = log.CompletedAt
        });
    }

    public async Task<ApiResponse<StudentNutritionGuidanceResponse?>> GetStudentGuidanceAsync(Guid studentId)
    {
        var item = await _db.StudentNutritionGuidances.FirstOrDefaultAsync(n => n.StudentId == studentId);
        return ApiResponse<StudentNutritionGuidanceResponse?>.Ok(item == null ? null : MapGuidance(item));
    }

    public async Task<HabitAdherenceResponse> BuildAdherenceAsync(Guid studentId, int days)
    {
        var normalizedDays = Math.Clamp(days, 1, 30);
        var end = _clock.LocalDate;
        var start = end.AddDays(-(normalizedDays - 1));

        var habits = await _db.StudentHabits
            .Where(h => h.StudentId == studentId && h.CreatedAt.Date <= end)
            .ToListAsync();
        var logs = await _db.StudentHabitLogs
            .Where(l => l.StudentId == studentId && l.Date >= start && l.Date <= end)
            .ToListAsync();

        var dayList = Enumerable.Range(0, normalizedDays).Select(i => start.AddDays(i)).ToList();
        var habitItems = new List<HabitAdherenceHabitItem>();
        var daySummary = new List<HabitAdherenceDayItem>();

        foreach (var day in dayList)
        {
            var expectedDay = habits.Count(h => IsHabitActiveOnDay(h, day));
            var completedDay = logs.Count(l => l.Date == day && l.IsCompleted);
            daySummary.Add(new HabitAdherenceDayItem { Date = day, Expected = expectedDay, Completed = completedDay });
        }

        foreach (var habit in habits)
        {
            var expected = dayList.Count(d => IsHabitActiveOnDay(habit, d));
            var completed = logs.Count(l => l.HabitId == habit.Id && l.IsCompleted);
            habitItems.Add(new HabitAdherenceHabitItem
            {
                HabitId = habit.Id,
                Title = habit.Title,
                Expected = expected,
                Completed = completed,
                CompletionRate = expected == 0 ? 0 : Math.Round((decimal)completed / expected * 100m, 1)
            });
        }

        var totalExpected = daySummary.Sum(d => d.Expected);
        var totalCompleted = daySummary.Sum(d => d.Completed);
        var lowest = habitItems.Where(h => h.Expected > 0).OrderBy(h => h.CompletionRate).FirstOrDefault();

        return new HabitAdherenceResponse
        {
            StudentId = studentId,
            Days = normalizedDays,
            TotalExpected = totalExpected,
            TotalCompleted = totalCompleted,
            CompletionRate = totalExpected == 0 ? 0 : Math.Round((decimal)totalCompleted / totalExpected * 100m, 1),
            LowestHabitTitle = lowest?.Title,
            LowestHabitRate = lowest?.CompletionRate,
            Habits = habitItems.OrderBy(h => h.Title).ToList(),
            DaysSummary = daySummary
        };
    }

    private static bool IsHabitActiveOnDay(StudentHabit habit, DateTime day)
    {
        if (habit.CreatedAt.Date > day) return false;
        return !habit.InactivatedAt.HasValue || habit.InactivatedAt.Value.Date >= day;
    }

    private static HabitCategory ParseCategory(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return HabitCategory.Custom;
        return Enum.TryParse<HabitCategory>(value, true, out var parsed) ? parsed : HabitCategory.Custom;
    }

    private static StudentHabitResponse MapHabit(StudentHabit h) => new()
    {
        Id = h.Id,
        StudentId = h.StudentId,
        Title = h.Title,
        Description = h.Description,
        Category = h.Category.ToString(),
        Frequency = h.Frequency.ToString(),
        TargetValue = h.TargetValue,
        TargetUnit = h.TargetUnit,
        IsActive = h.IsActive,
        InactivatedAt = h.InactivatedAt,
        CreatedAt = h.CreatedAt,
        UpdatedAt = h.UpdatedAt
    };

    private static StudentNutritionGuidanceResponse MapGuidance(StudentNutritionGuidance n) => new()
    {
        StudentId = n.StudentId,
        GuidanceText = n.GuidanceText,
        StrategicNotes = n.StrategicNotes,
        MediaId = n.MediaId,
        UpdatedAt = n.UpdatedAt
    };
}
