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
        if (!string.IsNullOrWhiteSpace(request.ImageUrl) || !string.IsNullOrWhiteSpace(request.VideoUrl))
            return ApiResponse<ExerciseResponse>.Fail("Envio por URL nÃ£o Ã© permitido. Use upload de mÃ­dia e informe os MediaIds.");

        string? imageUrl = null;
        string? videoUrl = null;
        if (request.ImageMediaId.HasValue)
        {
            var imageMedia = await _db.MediaFiles.FirstOrDefaultAsync(m => m.Id == request.ImageMediaId.Value);
            if (imageMedia == null) return ApiResponse<ExerciseResponse>.Fail("MÃ­dia de imagem nÃ£o encontrada.");
            imageUrl = imageMedia.SecureUrl ?? imageMedia.Url;
        }
        if (request.VideoMediaId.HasValue)
        {
            var videoMedia = await _db.MediaFiles.FirstOrDefaultAsync(m => m.Id == request.VideoMediaId.Value);
            if (videoMedia == null) return ApiResponse<ExerciseResponse>.Fail("MÃ­dia de vÃ­deo nÃ£o encontrada.");
            videoUrl = videoMedia.SecureUrl ?? videoMedia.Url;
        }

        var exercise = new Exercise
        {
            TrainerId = trainerId,
            Name = request.Name,
            MuscleGroup = request.MuscleGroup,
            Description = request.Description,
            Instructions = request.Instructions,
            ImageUrl = imageUrl,
            ImageMediaId = request.ImageMediaId,
            VideoUrl = videoUrl,
            VideoMediaId = request.VideoMediaId,
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

        if (!string.IsNullOrWhiteSpace(request.ImageUrl) || !string.IsNullOrWhiteSpace(request.VideoUrl))
            return ApiResponse<ExerciseResponse>.Fail("Envio por URL nÃ£o Ã© permitido. Use upload de mÃ­dia e informe os MediaIds.");

        exercise.Name = request.Name;
        exercise.MuscleGroup = request.MuscleGroup;
        exercise.Description = request.Description;
        exercise.Instructions = request.Instructions;
        if (request.ImageMediaId.HasValue)
        {
            var imageMedia = await _db.MediaFiles.FirstOrDefaultAsync(m => m.Id == request.ImageMediaId.Value);
            if (imageMedia == null) return ApiResponse<ExerciseResponse>.Fail("MÃ­dia de imagem nÃ£o encontrada.");
            exercise.ImageMediaId = imageMedia.Id;
            exercise.ImageUrl = imageMedia.SecureUrl ?? imageMedia.Url;
        }
        if (request.VideoMediaId.HasValue)
        {
            var videoMedia = await _db.MediaFiles.FirstOrDefaultAsync(m => m.Id == request.VideoMediaId.Value);
            if (videoMedia == null) return ApiResponse<ExerciseResponse>.Fail("MÃ­dia de vÃ­deo nÃ£o encontrada.");
            exercise.VideoMediaId = videoMedia.Id;
            exercise.VideoUrl = videoMedia.SecureUrl ?? videoMedia.Url;
        }
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
        ImageMediaId = e.ImageMediaId,
        VideoUrl = e.VideoUrl,
        VideoMediaId = e.VideoMediaId,
        Level = e.Level.ToString(),
        CreatedAt = e.CreatedAt
    };
}
