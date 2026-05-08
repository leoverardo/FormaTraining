using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.WorkoutSession;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class WorkoutSessionService
{
    private readonly AppDbContext _db;
    private readonly NotificationService _notifications;

    public WorkoutSessionService(AppDbContext db, NotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<ApiResponse<List<WorkoutSessionResponse>>> GetByStudentAsync(Guid studentId, Guid trainerId)
    {
        if (!await _db.Students.AnyAsync(s => s.Id == studentId && s.TrainerId == trainerId))
            return ApiResponse<List<WorkoutSessionResponse>>.Fail("Aluno não encontrado.");

        var sessions = await _db.WorkoutSessions
            .Include(ws => ws.Workout)
            .Include(ws => ws.ExerciseSessions).ThenInclude(es => es.Exercise)
            .Where(ws => ws.StudentId == studentId && ws.TrainerId == trainerId)
            .OrderByDescending(ws => ws.ScheduledDate)
            .Take(30)
            .ToListAsync();

        return ApiResponse<List<WorkoutSessionResponse>>.Ok(sessions.Select(MapResponse).ToList());
    }

    public async Task<ApiResponse<List<WorkoutSessionResponse>>> GetOwnAsync(Guid studentId)
    {
        var sessions = await _db.WorkoutSessions
            .Include(ws => ws.Workout)
            .Include(ws => ws.ExerciseSessions).ThenInclude(es => es.Exercise)
            .Where(ws => ws.StudentId == studentId)
            .OrderByDescending(ws => ws.ScheduledDate)
            .Take(50)
            .ToListAsync();

        return ApiResponse<List<WorkoutSessionResponse>>.Ok(sessions.Select(MapResponse).ToList());
    }

    public async Task<ApiResponse<WorkoutSessionResponse>> StartAsync(Guid studentId, StartWorkoutSessionRequest request)
    {
        var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == studentId);
        if (student == null) return ApiResponse<WorkoutSessionResponse>.Fail("Aluno não encontrado.");

        var workout = await _db.Workouts.Include(w => w.WorkoutExercises).ThenInclude(we => we.Exercise)
            .FirstOrDefaultAsync(w => w.Id == request.WorkoutId && w.TrainerId == student.TrainerId);
        if (workout == null) return ApiResponse<WorkoutSessionResponse>.Fail("Treino não encontrado.");

        var session = new WorkoutSession
        {
            StudentId = studentId, TrainerId = student.TrainerId, WorkoutId = request.WorkoutId,
            ScheduledDate = request.ScheduledDate ?? DateTime.UtcNow,
            StartedAt = DateTime.UtcNow, Status = WorkoutSessionStatus.Started
        };
        _db.WorkoutSessions.Add(session);
        await _db.SaveChangesAsync();

        session.Workout = workout;
        return ApiResponse<WorkoutSessionResponse>.Ok(MapResponse(session), "Treino iniciado!");
    }

    public async Task<ApiResponse<WorkoutSessionResponse>> CompleteAsync(Guid sessionId, Guid studentId, CompleteWorkoutSessionRequest request)
    {
        var session = await _db.WorkoutSessions
            .Include(ws => ws.Workout)
            .FirstOrDefaultAsync(ws => ws.Id == sessionId && ws.StudentId == studentId);

        if (session == null) return ApiResponse<WorkoutSessionResponse>.Fail("Sessão não encontrada.");

        session.Status = WorkoutSessionStatus.Completed;
        session.CompletedAt = DateTime.UtcNow;
        session.Notes = request.Notes;
        session.UpdatedAt = DateTime.UtcNow;

        foreach (var ex in request.Exercises)
        {
            var existing = await _db.WorkoutSessionExercises.FirstOrDefaultAsync(e => e.WorkoutSessionId == sessionId && e.ExerciseId == ex.ExerciseId);
            if (existing != null)
            {
                existing.SetsCompleted = ex.SetsCompleted;
                existing.RepsCompleted = ex.RepsCompleted;
                existing.LoadUsed = ex.LoadUsed;
                existing.DifficultyLevel = ex.DifficultyLevel;
                existing.Notes = ex.Notes;
            }
            else
            {
                _db.WorkoutSessionExercises.Add(new WorkoutSessionExercise
                {
                    WorkoutSessionId = sessionId, ExerciseId = ex.ExerciseId,
                    SetsCompleted = ex.SetsCompleted, RepsCompleted = ex.RepsCompleted,
                    LoadUsed = ex.LoadUsed, DifficultyLevel = ex.DifficultyLevel, Notes = ex.Notes
                });
            }
        }

        await _db.SaveChangesAsync();

        var trainerUser = await _db.Trainers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == session.TrainerId);
        if (trainerUser != null)
        {
            var studentUser = await _db.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == studentId);
            await _notifications.CreateAsync(trainerUser.UserId, "Treino concluído", $"{studentUser?.User.Name} concluiu o treino {session.Workout?.Name}.", NotificationType.WorkoutCompleted, session.TrainerId, studentId);
        }

        await _db.Entry(session).Collection(s => s.ExerciseSessions).LoadAsync();
        foreach (var es in session.ExerciseSessions)
            await _db.Entry(es).Reference(e => e.Exercise).LoadAsync();

        return ApiResponse<WorkoutSessionResponse>.Ok(MapResponse(session), "Treino concluído!");
    }

    public async Task<ApiResponse<WorkoutSessionResponse>> SkipAsync(Guid sessionId, Guid studentId)
    {
        var session = await _db.WorkoutSessions.Include(ws => ws.Workout)
            .FirstOrDefaultAsync(ws => ws.Id == sessionId && ws.StudentId == studentId);
        if (session == null) return ApiResponse<WorkoutSessionResponse>.Fail("Sessão não encontrada.");

        session.Status = WorkoutSessionStatus.Skipped;
        session.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ApiResponse<WorkoutSessionResponse>.Ok(MapResponse(session));
    }

    public static WorkoutSessionResponse MapResponse(WorkoutSession ws) => new()
    {
        Id = ws.Id, StudentId = ws.StudentId, WorkoutId = ws.WorkoutId,
        WorkoutName = ws.Workout?.Name ?? "", ScheduledDate = ws.ScheduledDate,
        StartedAt = ws.StartedAt, CompletedAt = ws.CompletedAt,
        Status = ws.Status.ToString(), Notes = ws.Notes, CreatedAt = ws.CreatedAt,
        Exercises = ws.ExerciseSessions.Select(es => new ExerciseSessionResponse
        {
            Id = es.Id, ExerciseId = es.ExerciseId, ExerciseName = es.Exercise?.Name ?? "",
            SetsCompleted = es.SetsCompleted, RepsCompleted = es.RepsCompleted,
            LoadUsed = es.LoadUsed, DifficultyLevel = es.DifficultyLevel, Notes = es.Notes
        }).ToList()
    };
}
