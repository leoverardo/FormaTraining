using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Exercises;
using FitPlatform.Domain.Entities;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class ExerciseService
{
    private readonly AppDbContext _db;

    public ExerciseService(AppDbContext db) => _db = db;

    public async Task<ApiResponse<List<ExerciseResponse>>> GetAllAsync(Guid trainerId)
    {
        var exercises = await _db.Exercises
            .Where(e => e.TrainerId == trainerId)
            .OrderBy(e => e.Name)
            .ToListAsync();
        return ApiResponse<List<ExerciseResponse>>.Ok(exercises.Select(MapResponse).ToList());
    }

    public async Task<ApiResponse<ExerciseResponse>> GetByIdAsync(Guid id, Guid trainerId)
    {
        var exercise = await _db.Exercises.FirstOrDefaultAsync(e => e.Id == id && e.TrainerId == trainerId);
        if (exercise == null) return ApiResponse<ExerciseResponse>.Fail("Exercício não encontrado.");
        return ApiResponse<ExerciseResponse>.Ok(MapResponse(exercise));
    }

    public async Task<ApiResponse<ExerciseResponse>> CreateAsync(ExerciseRequest request, Guid trainerId)
    {
        var exercise = new Exercise
        {
            TrainerId = trainerId,
            Name = request.Name,
            MuscleGroup = request.MuscleGroup,
            Description = request.Description,
            Instructions = request.Instructions,
            ImageUrl = request.ImageUrl,
            VideoUrl = request.VideoUrl,
            Level = request.Level
        };
        _db.Exercises.Add(exercise);
        await _db.SaveChangesAsync();
        return ApiResponse<ExerciseResponse>.Ok(MapResponse(exercise), "Exercício criado com sucesso.");
    }

    public async Task<ApiResponse<ExerciseResponse>> UpdateAsync(Guid id, ExerciseRequest request, Guid trainerId)
    {
        var exercise = await _db.Exercises.FirstOrDefaultAsync(e => e.Id == id && e.TrainerId == trainerId);
        if (exercise == null) return ApiResponse<ExerciseResponse>.Fail("Exercício não encontrado.");

        exercise.Name = request.Name;
        exercise.MuscleGroup = request.MuscleGroup;
        exercise.Description = request.Description;
        exercise.Instructions = request.Instructions;
        exercise.ImageUrl = request.ImageUrl;
        exercise.VideoUrl = request.VideoUrl;
        exercise.Level = request.Level;
        exercise.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ApiResponse<ExerciseResponse>.Ok(MapResponse(exercise));
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, Guid trainerId)
    {
        var exercise = await _db.Exercises.FirstOrDefaultAsync(e => e.Id == id && e.TrainerId == trainerId);
        if (exercise == null) return ApiResponse.Fail("Exercício não encontrado.");

        _db.Exercises.Remove(exercise);
        await _db.SaveChangesAsync();
        return ApiResponse.Ok("Exercício removido.");
    }

    public static ExerciseResponse MapResponse(Exercise e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        MuscleGroup = e.MuscleGroup,
        Description = e.Description,
        Instructions = e.Instructions,
        ImageUrl = e.ImageUrl,
        VideoUrl = e.VideoUrl,
        Level = e.Level.ToString(),
        CreatedAt = e.CreatedAt
    };
}
