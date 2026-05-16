using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Library;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class ExerciseLibraryService
{
    private readonly AppDbContext _db;

    public ExerciseLibraryService(AppDbContext db) => _db = db;

    public async Task<ApiResponse<List<ExerciseLibraryResponse>>> GetAllAsync()
    {
        var items = await _db.ExerciseLibraryItems.Where(e => e.IsActive).OrderBy(e => e.Name).ToListAsync();
        return ApiResponse<List<ExerciseLibraryResponse>>.Ok(items.Select(MapLib).ToList());
    }

    public async Task<ApiResponse<ExerciseLibraryResponse>> CreateAsync(ExerciseLibraryRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.ImageUrl) || !string.IsNullOrWhiteSpace(request.VideoUrl))
            return ApiResponse<ExerciseLibraryResponse>.Fail("Envio por URL nÃ£o Ã© permitido. Use upload de mÃ­dia e informe os MediaIds.");

        string? imageUrl = null;
        string? videoUrl = null;
        if (request.ImageMediaId.HasValue)
        {
            var imageMedia = await _db.MediaFiles.FirstOrDefaultAsync(m => m.Id == request.ImageMediaId.Value);
            if (imageMedia == null) return ApiResponse<ExerciseLibraryResponse>.Fail("MÃ­dia de imagem nÃ£o encontrada.");
            imageUrl = imageMedia.SecureUrl ?? imageMedia.Url;
        }
        if (request.VideoMediaId.HasValue)
        {
            var videoMedia = await _db.MediaFiles.FirstOrDefaultAsync(m => m.Id == request.VideoMediaId.Value);
            if (videoMedia == null) return ApiResponse<ExerciseLibraryResponse>.Fail("MÃ­dia de vÃ­deo nÃ£o encontrada.");
            videoUrl = videoMedia.SecureUrl ?? videoMedia.Url;
        }

        var item = new ExerciseLibraryItem
        {
            Name = request.Name, MuscleGroup = request.MuscleGroup, Description = request.Description,
            Instructions = request.Instructions, ImageUrl = imageUrl, VideoUrl = videoUrl,
            Level = (ExerciseLevel)request.Level, IsActive = request.IsActive
        };
        _db.ExerciseLibraryItems.Add(item);
        await _db.SaveChangesAsync();
        return ApiResponse<ExerciseLibraryResponse>.Ok(MapLib(item));
    }

    public async Task<ApiResponse<ExerciseLibraryResponse>> UpdateAsync(Guid id, ExerciseLibraryRequest request)
    {
        var item = await _db.ExerciseLibraryItems.FindAsync(id);
        if (item == null) return ApiResponse<ExerciseLibraryResponse>.Fail("Exercício não encontrado.");

        if (!string.IsNullOrWhiteSpace(request.ImageUrl) || !string.IsNullOrWhiteSpace(request.VideoUrl))
            return ApiResponse<ExerciseLibraryResponse>.Fail("Envio por URL nÃ£o Ã© permitido. Use upload de mÃ­dia e informe os MediaIds.");

        if (request.ImageMediaId.HasValue)
        {
            var imageMedia = await _db.MediaFiles.FirstOrDefaultAsync(m => m.Id == request.ImageMediaId.Value);
            if (imageMedia == null) return ApiResponse<ExerciseLibraryResponse>.Fail("MÃ­dia de imagem nÃ£o encontrada.");
            item.ImageUrl = imageMedia.SecureUrl ?? imageMedia.Url;
        }
        if (request.VideoMediaId.HasValue)
        {
            var videoMedia = await _db.MediaFiles.FirstOrDefaultAsync(m => m.Id == request.VideoMediaId.Value);
            if (videoMedia == null) return ApiResponse<ExerciseLibraryResponse>.Fail("MÃ­dia de vÃ­deo nÃ£o encontrada.");
            item.VideoUrl = videoMedia.SecureUrl ?? videoMedia.Url;
        }

        item.Name = request.Name; item.MuscleGroup = request.MuscleGroup; item.Description = request.Description;
        item.Instructions = request.Instructions;
        item.Level = (ExerciseLevel)request.Level; item.IsActive = request.IsActive; item.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ApiResponse<ExerciseLibraryResponse>.Ok(MapLib(item));
    }

    public async Task<ApiResponse> DeleteAsync(Guid id)
    {
        var item = await _db.ExerciseLibraryItems.FindAsync(id);
        if (item == null) return ApiResponse.Fail("Exercício não encontrado.");
        item.IsActive = false; item.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ApiResponse.Ok("Exercício desativado.");
    }

    public async Task<ApiResponse<object>> DuplicateToTrainerLibraryAsync(Guid id, Guid trainerId)
    {
        var item = await _db.ExerciseLibraryItems.FindAsync(id);
        if (item == null || !item.IsActive) return ApiResponse<object>.Fail("Exercício não encontrado.");

        var exercise = new Exercise
        {
            TrainerId = trainerId, Name = item.Name, MuscleGroup = item.MuscleGroup,
            Description = item.Description, Instructions = item.Instructions,
            ImageUrl = item.ImageUrl, VideoUrl = item.VideoUrl, Level = item.Level
        };
        _db.Exercises.Add(exercise);
        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { exercise.Id, exercise.Name }, "Exercício adicionado à sua biblioteca.");
    }

    // ── Templates ─────────────────────────────────────────────────────────────

    public async Task<ApiResponse<List<WorkoutTemplateResponse>>> GetAllTemplatesAsync()
    {
        var templates = await _db.WorkoutTemplates
            .Include(t => t.TemplateExercises).ThenInclude(te => te.ExerciseLibraryItem)
            .Where(t => t.IsActive).OrderBy(t => t.Name).ToListAsync();
        return ApiResponse<List<WorkoutTemplateResponse>>.Ok(templates.Select(MapTemplate).ToList());
    }

    public async Task<ApiResponse<WorkoutTemplateResponse>> CreateTemplateAsync(WorkoutTemplateRequest request)
    {
        var template = new WorkoutTemplate
        {
            Name = request.Name, Goal = request.Goal, Level = (ExerciseLevel)request.Level,
            Description = request.Description, IsActive = request.IsActive
        };
        _db.WorkoutTemplates.Add(template);
        await _db.SaveChangesAsync();
        await _db.Entry(template).Collection(t => t.TemplateExercises).LoadAsync();
        return ApiResponse<WorkoutTemplateResponse>.Ok(MapTemplate(template));
    }

    public async Task<ApiResponse<object>> DuplicateTemplateToWorkoutsAsync(Guid templateId, Guid trainerId)
    {
        var template = await _db.WorkoutTemplates
            .Include(t => t.TemplateExercises).ThenInclude(te => te.ExerciseLibraryItem)
            .FirstOrDefaultAsync(t => t.Id == templateId && t.IsActive);
        if (template == null) return ApiResponse<object>.Fail("Template não encontrado.");

        var workout = new Workout
        {
            TrainerId = trainerId, Name = template.Name, Goal = template.Goal,
            Level = template.Level, Description = template.Description, Status = WorkoutStatus.Active
        };
        _db.Workouts.Add(workout);
        await _db.SaveChangesAsync();

        foreach (var te in template.TemplateExercises.OrderBy(e => e.OrderIndex))
        {
            var exercise = await _db.Exercises.FirstOrDefaultAsync(e => e.TrainerId == trainerId && e.Name == te.ExerciseLibraryItem.Name);
            if (exercise == null)
            {
                exercise = new Exercise
                {
                    TrainerId = trainerId, Name = te.ExerciseLibraryItem.Name,
                    MuscleGroup = te.ExerciseLibraryItem.MuscleGroup, Level = te.ExerciseLibraryItem.Level,
                    Instructions = te.ExerciseLibraryItem.Instructions
                };
                _db.Exercises.Add(exercise);
                await _db.SaveChangesAsync();
            }
            _db.WorkoutExercises.Add(new WorkoutExercise
            {
                WorkoutId = workout.Id, ExerciseId = exercise.Id, Sets = te.Sets,
                Reps = te.Reps, SuggestedLoad = te.SuggestedLoad, RestSeconds = te.RestSeconds,
                Notes = te.Notes, OrderIndex = te.OrderIndex
            });
        }
        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { workout.Id, workout.Name }, "Treino criado a partir do template.");
    }

    private static ExerciseLibraryResponse MapLib(ExerciseLibraryItem e) => new()
    {
        Id = e.Id, Name = e.Name, MuscleGroup = e.MuscleGroup, Description = e.Description,
        Instructions = e.Instructions, ImageUrl = e.ImageUrl, VideoUrl = e.VideoUrl,
        Level = e.Level.ToString(), IsActive = e.IsActive
    };

    private static WorkoutTemplateResponse MapTemplate(WorkoutTemplate t) => new()
    {
        Id = t.Id, Name = t.Name, Goal = t.Goal, Level = t.Level.ToString(),
        Description = t.Description, IsActive = t.IsActive,
        Exercises = t.TemplateExercises.OrderBy(te => te.OrderIndex).Select(te => new TemplateExerciseResponse
        {
            Id = te.Id, ExerciseLibraryItemId = te.ExerciseLibraryItemId,
            ExerciseName = te.ExerciseLibraryItem?.Name ?? "", MuscleGroup = te.ExerciseLibraryItem?.MuscleGroup,
            Sets = te.Sets, Reps = te.Reps, SuggestedLoad = te.SuggestedLoad,
            RestSeconds = te.RestSeconds, Notes = te.Notes, OrderIndex = te.OrderIndex
        }).ToList()
    };
}
