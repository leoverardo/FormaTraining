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
    private readonly GamificationService _gamification;

    public WorkoutSessionService(AppDbContext db, NotificationService notifications, GamificationService gamification)
    {
        _db = db;
        _notifications = notifications;
        _gamification = gamification;
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

        var workout = await _db.Workouts
            .Include(w => w.WorkoutExercises.OrderBy(we => we.OrderIndex))
            .ThenInclude(we => we.Exercise)
            .FirstOrDefaultAsync(w => w.Id == request.WorkoutId && w.TrainerId == student.TrainerId);
        if (workout == null) return ApiResponse<WorkoutSessionResponse>.Fail("Treino não encontrado.");

        var existingStarted = await _db.WorkoutSessions
            .Include(ws => ws.Workout)
            .Include(ws => ws.ExerciseSessions).ThenInclude(es => es.Exercise)
            .FirstOrDefaultAsync(ws => ws.StudentId == studentId && ws.WorkoutId == request.WorkoutId && ws.Status == WorkoutSessionStatus.Started);

        if (existingStarted != null)
            return ApiResponse<WorkoutSessionResponse>.Ok(MapResponse(existingStarted), "Você já possui uma sessão em andamento para este treino.");

        var session = new WorkoutSession
        {
            StudentId = studentId,
            TrainerId = student.TrainerId,
            WorkoutId = request.WorkoutId,
            ScheduledDate = request.ScheduledDate ?? DateTime.UtcNow,
            StartedAt = DateTime.UtcNow,
            Status = WorkoutSessionStatus.Started
        };

        foreach (var we in workout.WorkoutExercises.OrderBy(x => x.OrderIndex))
        {
            var exerciseSession = new WorkoutSessionExercise
            {
                WorkoutExerciseId = we.Id,
                ExerciseId = we.ExerciseId,
                PrescribedSets = we.Sets,
                PrescribedReps = we.Reps,
                PrescribedLoad = we.SuggestedLoad,
                PrescribedRestSeconds = we.RestSeconds,
                PrescribedNotes = we.Notes,
                OrderIndex = we.OrderIndex,
                WorkoutSession = session
            };

            for (var setNumber = 1; setNumber <= Math.Max(1, we.Sets); setNumber++)
            {
                exerciseSession.Sets.Add(new WorkoutSessionSet
                {
                    SetNumber = setNumber,
                    PrescribedReps = we.Reps,
                    PrescribedLoad = we.SuggestedLoad,
                    PrescribedRestSeconds = we.RestSeconds
                });
            }

            session.ExerciseSessions.Add(exerciseSession);
        }

        _db.WorkoutSessions.Add(session);
        await _db.SaveChangesAsync();

        session.Workout = workout;
        foreach (var item in session.ExerciseSessions)
            item.Exercise = workout.WorkoutExercises.First(x => x.ExerciseId == item.ExerciseId).Exercise;

        return ApiResponse<WorkoutSessionResponse>.Ok(MapResponse(session), "Treino iniciado!");
    }

    public async Task<ApiResponse<WorkoutSessionExecutionResponse>> GetExecutionAsync(Guid sessionId, Guid studentId)
    {
        var session = await _db.WorkoutSessions
            .Include(ws => ws.Workout)
            .Include(ws => ws.ExerciseSessions.OrderBy(es => es.OrderIndex))
                .ThenInclude(es => es.Exercise)
            .Include(ws => ws.ExerciseSessions)
                .ThenInclude(es => es.Sets)
            .FirstOrDefaultAsync(ws => ws.Id == sessionId && ws.StudentId == studentId);

        if (session == null)
            return ApiResponse<WorkoutSessionExecutionResponse>.Fail("Sessão não encontrada.");

        var exerciseIds = session.ExerciseSessions.Select(es => es.ExerciseId).Distinct().ToList();
        var previousExecutions = await _db.WorkoutSessionExercises
            .Include(wse => wse.WorkoutSession)
            .Where(wse => wse.WorkoutSession.StudentId == studentId
                && wse.WorkoutSession.Status == WorkoutSessionStatus.Completed
                && wse.WorkoutSessionId != sessionId
                && (!session.StartedAt.HasValue || (wse.WorkoutSession.CompletedAt.HasValue && wse.WorkoutSession.CompletedAt < session.StartedAt))
                && exerciseIds.Contains(wse.ExerciseId))
            .OrderByDescending(wse => wse.WorkoutSession.CompletedAt)
            .ToListAsync();

        var previousMap = previousExecutions
            .GroupBy(x => x.ExerciseId)
            .ToDictionary(g => g.Key, g => g.First());

        var exercises = session.ExerciseSessions.OrderBy(es => es.OrderIndex).Select(es =>
        {
            previousMap.TryGetValue(es.ExerciseId, out var last);
            var summary = last == null ? null : $"{last.LoadUsed ?? "-"} x {last.RepsCompleted ?? "-"}";
            return new WorkoutExecutionExerciseResponse
            {
                WorkoutSessionExerciseId = es.Id,
                ExerciseId = es.ExerciseId,
                ExerciseName = es.Exercise.Name,
                ExerciseImageUrl = es.Exercise.ImageUrl,
                ExerciseVideoUrl = es.Exercise.VideoUrl,
                ExerciseInstructions = es.Exercise.Instructions,
                PrescribedNotes = es.PrescribedNotes,
                ExecutionNotes = es.Notes,
                OrderIndex = es.OrderIndex,
                IsCompleted = es.IsCompleted,
                CompletedAt = es.CompletedAt,
                PrescribedSets = es.PrescribedSets,
                PrescribedReps = es.PrescribedReps,
                PrescribedLoad = es.PrescribedLoad,
                PrescribedRestSeconds = es.PrescribedRestSeconds,
                LastExecutionSummary = summary,
                LastExecutionDate = last?.WorkoutSession.CompletedAt,
                Sets = es.Sets.OrderBy(s => s.SetNumber).Select(s => new WorkoutExecutionSetResponse
                {
                    Id = s.Id,
                    SetNumber = s.SetNumber,
                    PrescribedReps = s.PrescribedReps,
                    PrescribedLoad = s.PrescribedLoad,
                    PrescribedRestSeconds = s.PrescribedRestSeconds,
                    ActualReps = s.ActualReps,
                    ActualLoad = s.ActualLoad,
                    IsCompleted = s.IsCompleted,
                    CompletedAt = s.CompletedAt,
                    Notes = s.Notes
                }).ToList()
            };
        }).ToList();

        var totalSets = exercises.Sum(e => e.Sets.Count);
        var completedSets = exercises.Sum(e => e.Sets.Count(s => s.IsCompleted));
        var completedExercises = exercises.Count(e => e.IsCompleted);
        var durationSeconds = 0;
        if (session.StartedAt.HasValue)
        {
            var end = session.CompletedAt ?? DateTime.UtcNow;
            durationSeconds = (int)Math.Max(0, (end - session.StartedAt.Value).TotalSeconds);
        }

        return ApiResponse<WorkoutSessionExecutionResponse>.Ok(new WorkoutSessionExecutionResponse
        {
            SessionId = session.Id,
            WorkoutId = session.WorkoutId,
            WorkoutName = session.Workout.Name,
            Status = session.Status.ToString(),
            StartedAt = session.StartedAt,
            CompletedAt = session.CompletedAt,
            TotalExercises = exercises.Count,
            CompletedExercises = completedExercises,
            TotalSets = totalSets,
            CompletedSets = completedSets,
            DurationSeconds = durationSeconds,
            Exercises = exercises
        });
    }

    public async Task<ApiResponse<WorkoutExecutionSetResponse>> UpdateSetAsync(Guid sessionId, Guid setId, Guid studentId, UpdateWorkoutSessionSetRequest request)
    {
        var set = await _db.WorkoutSessionSets
            .Include(s => s.WorkoutSessionExercise)
            .ThenInclude(e => e.WorkoutSession)
            .Include(s => s.WorkoutSessionExercise)
            .ThenInclude(e => e.Sets)
            .FirstOrDefaultAsync(s => s.Id == setId && s.WorkoutSessionExercise.WorkoutSessionId == sessionId);

        if (set == null || set.WorkoutSessionExercise.WorkoutSession.StudentId != studentId)
            return ApiResponse<WorkoutExecutionSetResponse>.Fail("Série não encontrada.");

        if (set.WorkoutSessionExercise.WorkoutSession.Status != WorkoutSessionStatus.Started)
            return ApiResponse<WorkoutExecutionSetResponse>.Fail("A sessão não está em andamento.");

        set.ActualLoad = request.ActualLoad?.Trim();
        set.ActualReps = request.ActualReps?.Trim();
        set.Notes = request.Notes?.Trim();

        if (request.IsCompleted.HasValue)
        {
            set.IsCompleted = request.IsCompleted.Value;
            set.CompletedAt = set.IsCompleted ? DateTime.UtcNow : null;
        }

        var exercise = set.WorkoutSessionExercise;
        exercise.SetsCompleted = exercise.Sets.Count(x => x.IsCompleted);
        exercise.RepsCompleted = string.Join(" / ", exercise.Sets.OrderBy(x => x.SetNumber).Select(x => x.ActualReps ?? "-"));
        exercise.LoadUsed = string.Join(" / ", exercise.Sets.OrderBy(x => x.SetNumber).Select(x => x.ActualLoad ?? "-"));
        exercise.IsCompleted = exercise.Sets.All(x => x.IsCompleted);
        exercise.CompletedAt = exercise.IsCompleted ? DateTime.UtcNow : null;
        exercise.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return ApiResponse<WorkoutExecutionSetResponse>.Ok(new WorkoutExecutionSetResponse
        {
            Id = set.Id,
            SetNumber = set.SetNumber,
            PrescribedReps = set.PrescribedReps,
            PrescribedLoad = set.PrescribedLoad,
            PrescribedRestSeconds = set.PrescribedRestSeconds,
            ActualReps = set.ActualReps,
            ActualLoad = set.ActualLoad,
            IsCompleted = set.IsCompleted,
            CompletedAt = set.CompletedAt,
            Notes = set.Notes
        });
    }

    public async Task<ApiResponse<object>> CompleteExerciseAsync(Guid sessionId, Guid exerciseSessionId, Guid studentId, CompleteWorkoutSessionExerciseRequest request)
    {
        var exercise = await _db.WorkoutSessionExercises
            .Include(e => e.WorkoutSession)
            .Include(e => e.Sets)
            .FirstOrDefaultAsync(e => e.Id == exerciseSessionId && e.WorkoutSessionId == sessionId);

        if (exercise == null || exercise.WorkoutSession.StudentId != studentId)
            return ApiResponse<object>.Fail("Exercício da sessão não encontrado.");

        if (exercise.WorkoutSession.Status != WorkoutSessionStatus.Started)
            return ApiResponse<object>.Fail("A sessão não está em andamento.");

        exercise.IsCompleted = request.IsCompleted;
        exercise.CompletedAt = request.IsCompleted ? DateTime.UtcNow : null;
        if (!string.IsNullOrWhiteSpace(request.Notes)) exercise.Notes = request.Notes.Trim();

        if (request.IsCompleted)
        {
            foreach (var set in exercise.Sets.Where(s => !s.IsCompleted))
            {
                set.IsCompleted = true;
                set.CompletedAt ??= DateTime.UtcNow;
            }
        }

        exercise.SetsCompleted = exercise.Sets.Count(x => x.IsCompleted);
        exercise.RepsCompleted = string.Join(" / ", exercise.Sets.OrderBy(x => x.SetNumber).Select(x => x.ActualReps ?? "-"));
        exercise.LoadUsed = string.Join(" / ", exercise.Sets.OrderBy(x => x.SetNumber).Select(x => x.ActualLoad ?? "-"));
        exercise.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { ExerciseSessionId = exercise.Id, exercise.IsCompleted, exercise.CompletedAt });
    }

    public async Task<ApiResponse<WorkoutSessionResponse>> CompleteAsync(Guid sessionId, Guid studentId, CompleteWorkoutSessionRequest request)
    {
        var session = await _db.WorkoutSessions
            .Include(ws => ws.Workout)
            .Include(ws => ws.ExerciseSessions).ThenInclude(es => es.Exercise)
            .Include(ws => ws.ExerciseSessions).ThenInclude(es => es.Sets)
            .FirstOrDefaultAsync(ws => ws.Id == sessionId && ws.StudentId == studentId);

        if (session == null) return ApiResponse<WorkoutSessionResponse>.Fail("Sessão não encontrada.");
        if (session.Status == WorkoutSessionStatus.Completed) return ApiResponse<WorkoutSessionResponse>.Fail("Sessão já concluída.");
        if (session.Status == WorkoutSessionStatus.Skipped) return ApiResponse<WorkoutSessionResponse>.Fail("Sessão já foi pulada.");
        if (session.Status != WorkoutSessionStatus.Started) return ApiResponse<WorkoutSessionResponse>.Fail("A sessão não está em andamento.");

        foreach (var exercise in session.ExerciseSessions)
        {
            exercise.SetsCompleted = exercise.Sets.Count(x => x.IsCompleted);
            exercise.RepsCompleted = string.Join(" / ", exercise.Sets.OrderBy(x => x.SetNumber).Select(x => x.ActualReps ?? "-"));
            exercise.LoadUsed = string.Join(" / ", exercise.Sets.OrderBy(x => x.SetNumber).Select(x => x.ActualLoad ?? "-"));
            exercise.IsCompleted = exercise.IsCompleted || exercise.Sets.All(x => x.IsCompleted);
            exercise.CompletedAt ??= exercise.IsCompleted ? DateTime.UtcNow : null;
            exercise.UpdatedAt = DateTime.UtcNow;
        }

        foreach (var ex in request.Exercises)
        {
            var existing = session.ExerciseSessions.FirstOrDefault(e => e.ExerciseId == ex.ExerciseId);
            if (existing == null) continue;

            existing.SetsCompleted = ex.SetsCompleted ?? existing.SetsCompleted;
            existing.RepsCompleted = ex.RepsCompleted ?? existing.RepsCompleted;
            existing.LoadUsed = ex.LoadUsed ?? existing.LoadUsed;
            existing.DifficultyLevel = ex.DifficultyLevel ?? existing.DifficultyLevel;
            existing.Notes = ex.Notes ?? existing.Notes;
            existing.IsCompleted = true;
            existing.CompletedAt ??= DateTime.UtcNow;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        var hasAnyCompletedSet = session.ExerciseSessions.SelectMany(es => es.Sets).Any(s => s.IsCompleted);
        if (!hasAnyCompletedSet)
            return ApiResponse<WorkoutSessionResponse>.Fail("Conclua pelo menos uma série antes de finalizar o treino.");

        session.Status = WorkoutSessionStatus.Completed;
        session.CompletedAt = DateTime.UtcNow;
        session.Notes = request.Notes;
        session.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var trainerUser = await _db.Trainers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == session.TrainerId);
        if (trainerUser != null)
        {
            var studentUser = await _db.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == studentId);
            await _notifications.CreateAsync(trainerUser.UserId, "Treino concluído", $"{studentUser?.User.Name} concluiu o treino {session.Workout?.Name}.", NotificationType.WorkoutCompleted, session.TrainerId, studentId);
        }

        await _gamification.EvaluateForWorkoutCompletedAsync(studentId);

        return ApiResponse<WorkoutSessionResponse>.Ok(MapResponse(session), "Treino concluído!");
    }

    public async Task<ApiResponse<WorkoutSessionResponse>> SkipAsync(Guid sessionId, Guid studentId)
    {
        var session = await _db.WorkoutSessions.Include(ws => ws.Workout)
            .FirstOrDefaultAsync(ws => ws.Id == sessionId && ws.StudentId == studentId);
        if (session == null) return ApiResponse<WorkoutSessionResponse>.Fail("Sessão não encontrada.");

        if (session.Status == WorkoutSessionStatus.Completed)
            return ApiResponse<WorkoutSessionResponse>.Fail("Sessão já concluída.");

        session.Status = WorkoutSessionStatus.Skipped;
        session.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ApiResponse<WorkoutSessionResponse>.Ok(MapResponse(session));
    }

    public static WorkoutSessionResponse MapResponse(WorkoutSession ws) => new()
    {
        Id = ws.Id,
        StudentId = ws.StudentId,
        WorkoutId = ws.WorkoutId,
        WorkoutName = ws.Workout?.Name ?? string.Empty,
        ScheduledDate = ws.ScheduledDate,
        StartedAt = ws.StartedAt,
        CompletedAt = ws.CompletedAt,
        Status = ws.Status.ToString(),
        Notes = ws.Notes,
        CreatedAt = ws.CreatedAt,
        Exercises = ws.ExerciseSessions.OrderBy(es => es.OrderIndex).Select(es => new ExerciseSessionResponse
        {
            Id = es.Id,
            WorkoutExerciseId = es.WorkoutExerciseId,
            ExerciseId = es.ExerciseId,
            ExerciseName = es.Exercise?.Name ?? string.Empty,
            PrescribedSets = es.PrescribedSets,
            PrescribedReps = es.PrescribedReps,
            PrescribedLoad = es.PrescribedLoad,
            PrescribedRestSeconds = es.PrescribedRestSeconds,
            PrescribedNotes = es.PrescribedNotes,
            OrderIndex = es.OrderIndex,
            SetsCompleted = es.SetsCompleted,
            RepsCompleted = es.RepsCompleted,
            LoadUsed = es.LoadUsed,
            DifficultyLevel = es.DifficultyLevel,
            IsCompleted = es.IsCompleted,
            CompletedAt = es.CompletedAt,
            Notes = es.Notes
        }).ToList()
    };
}
