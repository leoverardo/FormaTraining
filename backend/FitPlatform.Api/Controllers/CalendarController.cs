using FitPlatform.Application.Common;
using FitPlatform.Application.Interfaces;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Api.Controllers;

[ApiController]
public class CalendarController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CalendarController(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet("api/student/calendar")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetStudentCalendar([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var studentId = _currentUser.StudentId!.Value;
        var start = startDate ?? DateTime.UtcNow.AddDays(-7);
        var end = endDate ?? DateTime.UtcNow.AddDays(14);

        var sessions = await _db.WorkoutSessions
            .Include(ws => ws.Workout)
            .Where(ws => ws.StudentId == studentId && ws.ScheduledDate >= start && ws.ScheduledDate <= end)
            .ToListAsync();

        var schedules = await _db.StudentWorkoutSchedules
            .Include(s => s.Workout)
            .Where(s => s.StudentId == studentId)
            .ToListAsync();

        var calendarItems = sessions.Select(s => new
        {
            Date = s.ScheduledDate.Date,
            WorkoutName = s.Workout?.Name ?? "Treino",
            Status = s.Status.ToString(),
            Type = "session",
            s.Id
        }).ToList<object>();

        var result = new { Sessions = calendarItems, WeeklySchedule = schedules.Select(s => new { s.DayOfWeek, WorkoutName = s.Workout?.Name, s.WorkoutId }) };
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("api/students/{studentId:guid}/calendar")]
    [Authorize(Roles = "Trainer")]
    public async Task<IActionResult> GetStudentCalendarForTrainer(Guid studentId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        if (!await _db.Students.AnyAsync(s => s.Id == studentId && s.TrainerId == _currentUser.TrainerId!.Value))
            return NotFound(ApiResponse.Fail("Aluno não encontrado."));

        var start = startDate ?? DateTime.UtcNow.AddDays(-7);
        var end = endDate ?? DateTime.UtcNow.AddDays(14);

        var sessions = await _db.WorkoutSessions
            .Include(ws => ws.Workout)
            .Where(ws => ws.StudentId == studentId && ws.ScheduledDate >= start && ws.ScheduledDate <= end)
            .ToListAsync();

        var schedules = await _db.StudentWorkoutSchedules
            .Include(s => s.Workout)
            .Where(s => s.StudentId == studentId)
            .ToListAsync();

        var result = new
        {
            Sessions = sessions.Select(s => new { Date = s.ScheduledDate.Date, WorkoutName = s.Workout?.Name, Status = s.Status.ToString(), s.Id }),
            WeeklySchedule = schedules.Select(s => new { s.DayOfWeek, WorkoutName = s.Workout?.Name, s.WorkoutId })
        };
        return Ok(ApiResponse<object>.Ok(result));
    }
}
