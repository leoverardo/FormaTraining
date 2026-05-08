using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Schedule;
using FitPlatform.Domain.Entities;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class ScheduleService
{
    private readonly AppDbContext _db;
    private static readonly string[] DayNames = ["Domingo", "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira", "Sábado"];

    public ScheduleService(AppDbContext db) => _db = db;

    public async Task<ApiResponse<List<ScheduleResponse>>> GetByStudentAsync(Guid studentId, Guid trainerId)
    {
        var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == studentId && s.TrainerId == trainerId);
        if (student == null) return ApiResponse<List<ScheduleResponse>>.Fail("Aluno não encontrado.");

        var schedules = await _db.StudentWorkoutSchedules
            .Include(s => s.Workout)
            .Where(s => s.StudentId == studentId && s.TrainerId == trainerId)
            .OrderBy(s => s.DayOfWeek)
            .ToListAsync();

        return ApiResponse<List<ScheduleResponse>>.Ok(schedules.Select(MapResponse).ToList());
    }

    public async Task<ApiResponse<ScheduleResponse>> CreateAsync(Guid studentId, ScheduleRequest request, Guid trainerId)
    {
        var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == studentId && s.TrainerId == trainerId);
        if (student == null) return ApiResponse<ScheduleResponse>.Fail("Aluno não encontrado.");

        var workout = await _db.Workouts.FirstOrDefaultAsync(w => w.Id == request.WorkoutId && w.TrainerId == trainerId);
        if (workout == null) return ApiResponse<ScheduleResponse>.Fail("Treino não encontrado.");

        var schedule = new StudentWorkoutSchedule
        {
            TrainerId = trainerId,
            StudentId = studentId,
            WorkoutId = request.WorkoutId,
            DayOfWeek = request.DayOfWeek,
            Notes = request.Notes
        };
        _db.StudentWorkoutSchedules.Add(schedule);
        await _db.SaveChangesAsync();

        schedule.Workout = workout;
        return ApiResponse<ScheduleResponse>.Ok(MapResponse(schedule));
    }

    public async Task<ApiResponse<ScheduleResponse>> UpdateAsync(Guid id, ScheduleRequest request, Guid trainerId)
    {
        var schedule = await _db.StudentWorkoutSchedules.Include(s => s.Workout)
            .FirstOrDefaultAsync(s => s.Id == id && s.TrainerId == trainerId);
        if (schedule == null) return ApiResponse<ScheduleResponse>.Fail("Agendamento não encontrado.");

        var workout = await _db.Workouts.FirstOrDefaultAsync(w => w.Id == request.WorkoutId && w.TrainerId == trainerId);
        if (workout == null) return ApiResponse<ScheduleResponse>.Fail("Treino não encontrado.");

        schedule.WorkoutId = request.WorkoutId;
        schedule.DayOfWeek = request.DayOfWeek;
        schedule.Notes = request.Notes;
        schedule.UpdatedAt = DateTime.UtcNow;
        schedule.Workout = workout;

        await _db.SaveChangesAsync();
        return ApiResponse<ScheduleResponse>.Ok(MapResponse(schedule));
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, Guid trainerId)
    {
        var schedule = await _db.StudentWorkoutSchedules.FirstOrDefaultAsync(s => s.Id == id && s.TrainerId == trainerId);
        if (schedule == null) return ApiResponse.Fail("Agendamento não encontrado.");
        _db.StudentWorkoutSchedules.Remove(schedule);
        await _db.SaveChangesAsync();
        return ApiResponse.Ok("Agendamento removido.");
    }

    private static ScheduleResponse MapResponse(StudentWorkoutSchedule s) => new()
    {
        Id = s.Id,
        StudentId = s.StudentId,
        WorkoutId = s.WorkoutId,
        WorkoutName = s.Workout?.Name ?? "",
        DayOfWeek = s.DayOfWeek,
        DayName = s.DayOfWeek >= 0 && s.DayOfWeek <= 6 ? DayNames[s.DayOfWeek] : "",
        Notes = s.Notes
    };
}
