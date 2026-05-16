using FitPlatform.Application.Common;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class StudentMonitoringService
{
    private readonly AppDbContext _db;
    private readonly HabitService _habitService;

    public StudentMonitoringService(AppDbContext db, HabitService habitService)
    {
        _db = db;
        _habitService = habitService;
    }

    public async Task<ApiResponse<List<object>>> GetStudentsMonitoringAsync(Guid trainerId)
    {
        var students = await _db.Students.Include(s => s.User)
            .Where(s => s.TrainerId == trainerId).ToListAsync();

        var result = students.Select(s => (object)new
        {
            s.Id,
            Name = s.User.Name,
            s.Status,
            MonitoringStatus = s.MonitoringStatus.ToString(),
            LastLogin = s.User.LastLoginAt,
            s.LastMonitoringStatusCalculatedAt
        }).ToList();

        return ApiResponse<List<object>>.Ok(result);
    }

    public async Task<ApiResponse> RecalculateForTrainerAsync(Guid trainerId)
    {
        var students = await _db.Students.Include(s => s.User)
            .Where(s => s.TrainerId == trainerId).ToListAsync();

        var weekStart = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek);

        foreach (var student in students)
        {
            student.MonitoringStatus = await CalculateStatusAsync(student.Id, weekStart);
            student.LastMonitoringStatusCalculatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return ApiResponse.Ok($"{students.Count} alunos recalculados.");
    }

    private async Task<StudentMonitoringStatus> CalculateStatusAsync(Guid studentId, DateTime weekStart)
    {
        var student = await _db.Students.Include(s => s.User).FirstAsync(s => s.Id == studentId);

        if (student.Status == StudentStatus.Inactive) return StudentMonitoringStatus.Inactive;

        var hasCheckInThisWeek = await _db.StudentWeeklyCheckIns.AnyAsync(c => c.StudentId == studentId && c.WeekStartDate >= weekStart);
        if (!hasCheckInThisWeek) return StudentMonitoringStatus.MissingCheckIn;

        var lastLogin = student.User.LastLoginAt;
        if (lastLogin.HasValue && lastLogin.Value < DateTime.UtcNow.AddDays(-7)) return StudentMonitoringStatus.NeedsAttention;

        var recentProgress = await _db.StudentProgressRecords.AnyAsync(p => p.StudentId == studentId && p.CreatedAt >= DateTime.UtcNow.AddDays(-30));
        var recentCompletions = await _db.WorkoutSessions.CountAsync(ws => ws.StudentId == studentId && ws.Status == WorkoutSessionStatus.Completed && ws.CompletedAt >= DateTime.UtcNow.AddDays(-7));
        var habitAdherence = await _habitService.BuildAdherenceAsync(studentId, 7);

        if (habitAdherence.TotalExpected > 0 && habitAdherence.CompletionRate < 40m)
            return StudentMonitoringStatus.NeedsAttention;
        if (recentProgress && recentCompletions >= 2) return StudentMonitoringStatus.ProgressingWell;
        if (recentCompletions >= 1) return StudentMonitoringStatus.OnTrack;

        return StudentMonitoringStatus.WorkoutDelayed;
    }
}
