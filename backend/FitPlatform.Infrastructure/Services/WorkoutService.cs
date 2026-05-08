using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Exercises;
using FitPlatform.Application.DTOs.Workouts;
using FitPlatform.Domain.Entities;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class WorkoutService
{
    private readonly AppDbContext _db;

    public WorkoutService(AppDbContext db) => _db = db;

    public async Task<ApiResponse<List<WorkoutResponse>>> GetAllAsync(Guid trainerId)
    {
        var workouts = await _db.Workouts
            .Include(w => w.WorkoutExercises).ThenInclude(we => we.Exercise)
            .Where(w => w.TrainerId == trainerId)
            .OrderBy(w => w.Name)
            .ToListAsync();
        return ApiResponse<List<WorkoutResponse>>.Ok(workouts.Select(MapResponse).ToList());
    }

    public async Task<ApiResponse<WorkoutResponse>> GetByIdAsync(Guid id, Guid trainerId)
    {
        var workout = await _db.Workouts
            .Include(w => w.WorkoutExercises).ThenInclude(we => we.Exercise)
            .FirstOrDefaultAsync(w => w.Id == id && w.TrainerId == trainerId);
        if (workout == null) return ApiResponse<WorkoutResponse>.Fail("Treino não encontrado.");
        return ApiResponse<WorkoutResponse>.Ok(MapResponse(workout));
    }

    public async Task<ApiResponse<WorkoutResponse>> CreateAsync(WorkoutRequest request, Guid trainerId)
    {
        var workout = new Workout
        {
            TrainerId = trainerId,
            Name = request.Name,
            Goal = request.Goal,
            Level = request.Level,
            Description = request.Description,
            Status = request.Status
        };
        _db.Workouts.Add(workout);
        await _db.SaveChangesAsync();
        return ApiResponse<WorkoutResponse>.Ok(MapResponse(workout), "Treino criado com sucesso.");
    }

    public async Task<ApiResponse<WorkoutResponse>> UpdateAsync(Guid id, WorkoutRequest request, Guid trainerId)
    {
        var workout = await _db.Workouts
            .Include(w => w.WorkoutExercises).ThenInclude(we => we.Exercise)
            .FirstOrDefaultAsync(w => w.Id == id && w.TrainerId == trainerId);
        if (workout == null) return ApiResponse<WorkoutResponse>.Fail("Treino não encontrado.");

        workout.Name = request.Name;
        workout.Goal = request.Goal;
        workout.Level = request.Level;
        workout.Description = request.Description;
        workout.Status = request.Status;
        workout.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ApiResponse<WorkoutResponse>.Ok(MapResponse(workout));
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, Guid trainerId)
    {
        var workout = await _db.Workouts.FirstOrDefaultAsync(w => w.Id == id && w.TrainerId == trainerId);
        if (workout == null) return ApiResponse.Fail("Treino não encontrado.");
        _db.Workouts.Remove(workout);
        await _db.SaveChangesAsync();
        return ApiResponse.Ok("Treino removido.");
    }

    public async Task<ApiResponse<WorkoutExerciseResponse>> AddExerciseAsync(Guid workoutId, WorkoutExerciseRequest request, Guid trainerId)
    {
        var workout = await _db.Workouts.FirstOrDefaultAsync(w => w.Id == workoutId && w.TrainerId == trainerId);
        if (workout == null) return ApiResponse<WorkoutExerciseResponse>.Fail("Treino não encontrado.");

        var exercise = await _db.Exercises.FirstOrDefaultAsync(e => e.Id == request.ExerciseId && e.TrainerId == trainerId);
        if (exercise == null) return ApiResponse<WorkoutExerciseResponse>.Fail("Exercício não encontrado ou não pertence ao seu treinador.");

        var we = new WorkoutExercise
        {
            WorkoutId = workoutId,
            ExerciseId = request.ExerciseId,
            Sets = request.Sets,
            Reps = request.Reps,
            SuggestedLoad = request.SuggestedLoad,
            RestSeconds = request.RestSeconds,
            Notes = request.Notes,
            OrderIndex = request.OrderIndex
        };
        _db.WorkoutExercises.Add(we);
        await _db.SaveChangesAsync();

        we.Exercise = exercise;
        return ApiResponse<WorkoutExerciseResponse>.Ok(MapWeResponse(we));
    }

    public async Task<ApiResponse<WorkoutExerciseResponse>> UpdateExerciseAsync(Guid workoutId, Guid weId, WorkoutExerciseRequest request, Guid trainerId)
    {
        var workout = await _db.Workouts.FirstOrDefaultAsync(w => w.Id == workoutId && w.TrainerId == trainerId);
        if (workout == null) return ApiResponse<WorkoutExerciseResponse>.Fail("Treino não encontrado.");

        var we = await _db.WorkoutExercises.Include(x => x.Exercise).FirstOrDefaultAsync(x => x.Id == weId && x.WorkoutId == workoutId);
        if (we == null) return ApiResponse<WorkoutExerciseResponse>.Fail("Exercício não encontrado no treino.");

        we.Sets = request.Sets;
        we.Reps = request.Reps;
        we.SuggestedLoad = request.SuggestedLoad;
        we.RestSeconds = request.RestSeconds;
        we.Notes = request.Notes;
        we.OrderIndex = request.OrderIndex;
        we.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ApiResponse<WorkoutExerciseResponse>.Ok(MapWeResponse(we));
    }

    public async Task<ApiResponse> RemoveExerciseAsync(Guid workoutId, Guid weId, Guid trainerId)
    {
        var workout = await _db.Workouts.FirstOrDefaultAsync(w => w.Id == workoutId && w.TrainerId == trainerId);
        if (workout == null) return ApiResponse.Fail("Treino não encontrado.");

        var we = await _db.WorkoutExercises.FirstOrDefaultAsync(x => x.Id == weId && x.WorkoutId == workoutId);
        if (we == null) return ApiResponse.Fail("Exercício não encontrado no treino.");

        _db.WorkoutExercises.Remove(we);
        await _db.SaveChangesAsync();
        return ApiResponse.Ok("Exercício removido do treino.");
    }

    private static WorkoutResponse MapResponse(Workout w) => new()
    {
        Id = w.Id,
        Name = w.Name,
        Goal = w.Goal,
        Level = w.Level.ToString(),
        Description = w.Description,
        Status = w.Status.ToString(),
        CreatedAt = w.CreatedAt,
        Exercises = w.WorkoutExercises.OrderBy(we => we.OrderIndex).Select(MapWeResponse).ToList()
    };

    private static WorkoutExerciseResponse MapWeResponse(WorkoutExercise we) => new()
    {
        Id = we.Id,
        Sets = we.Sets,
        Reps = we.Reps,
        SuggestedLoad = we.SuggestedLoad,
        RestSeconds = we.RestSeconds,
        Notes = we.Notes,
        OrderIndex = we.OrderIndex,
        Exercise = ExerciseService.MapResponse(we.Exercise)
    };
}
