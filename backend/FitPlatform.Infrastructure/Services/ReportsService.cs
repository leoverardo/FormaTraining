using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Reports;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class ReportsService
{
    private readonly AppDbContext _db;

    public ReportsService(AppDbContext db) => _db = db;

    public async Task<ApiResponse<OverviewReportResponse>> GetOverviewAsync(Guid trainerId, Guid userId)
    {
        var weekStart = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        var totalStudents = await _db.Students.CountAsync(s => s.TrainerId == trainerId);
        var activeStudents = await _db.Students.CountAsync(s => s.TrainerId == trainerId && s.Status == StudentStatus.Active);
        var checkInsThisWeek = await _db.StudentWeeklyCheckIns.CountAsync(c => c.TrainerId == trainerId && c.WeekStartDate >= weekStart);
        var activeStudentIds = await _db.Students.Where(s => s.TrainerId == trainerId && s.Status == StudentStatus.Active).Select(s => s.Id).ToListAsync();
        var studentsWithoutCheckIn = activeStudentIds.Count - await _db.StudentWeeklyCheckIns.CountAsync(c => c.TrainerId == trainerId && c.WeekStartDate >= weekStart && activeStudentIds.Contains(c.StudentId));
        var sessionsThisWeek = await _db.WorkoutSessions.CountAsync(ws => ws.TrainerId == trainerId && ws.ScheduledDate >= weekStart);
        var completedThisWeek = await _db.WorkoutSessions.CountAsync(ws => ws.TrainerId == trainerId && ws.CompletedAt >= weekStart && ws.Status == WorkoutSessionStatus.Completed);
        var progressThisWeek = await _db.StudentProgressRecords.CountAsync(p => p.TrainerId == trainerId && p.CreatedAt >= weekStart);
        var unreadNotifications = await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

        var students = await _db.Students.Include(s => s.User)
            .Where(s => s.TrainerId == trainerId).ToListAsync();

        var engagement = new List<StudentEngagementItem>();
        foreach (var s in students)
        {
            var checkIns = await _db.StudentWeeklyCheckIns.CountAsync(c => c.StudentId == s.Id && c.WeekStartDate >= monthStart);
            var workoutsCompleted = await _db.WorkoutSessions.CountAsync(ws => ws.StudentId == s.Id && ws.Status == WorkoutSessionStatus.Completed && ws.CompletedAt >= monthStart);
            var lastCheckIn = await _db.StudentWeeklyCheckIns.Where(c => c.StudentId == s.Id).OrderByDescending(c => c.CreatedAt).Select(c => (DateTime?)c.CreatedAt).FirstOrDefaultAsync();

            engagement.Add(new StudentEngagementItem
            {
                StudentId = s.Id, Name = s.User.Name, Status = s.Status.ToString(),
                MonitoringStatus = s.MonitoringStatus.ToString(), LastLogin = s.User.LastLoginAt,
                LastCheckIn = lastCheckIn, CheckInsThisMonth = checkIns, WorkoutsCompletedThisMonth = workoutsCompleted
            });
        }

        return ApiResponse<OverviewReportResponse>.Ok(new OverviewReportResponse
        {
            TotalStudents = totalStudents, ActiveStudents = activeStudents,
            InactiveStudents = totalStudents - activeStudents,
            StudentsWithCheckInThisWeek = checkInsThisWeek,
            StudentsWithoutCheckInThisWeek = studentsWithoutCheckIn,
            WorkoutSessionsThisWeek = sessionsThisWeek, WorkoutsCompletedThisWeek = completedThisWeek,
            ProgressRecordsThisWeek = progressThisWeek, UnreadNotifications = unreadNotifications,
            StudentEngagement = engagement
        });
    }
}
