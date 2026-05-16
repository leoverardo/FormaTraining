namespace FitPlatform.Application.DTOs.Library;

public class ExerciseLibraryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? MuscleGroup { get; set; }
    public string? Description { get; set; }
    public string? Instructions { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string Level { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class ExerciseLibraryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? MuscleGroup { get; set; }
    public string? Description { get; set; }
    public string? Instructions { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? ImageMediaId { get; set; }
    public string? VideoUrl { get; set; }
    public Guid? VideoMediaId { get; set; }
    public int Level { get; set; } = 1;
    public bool IsActive { get; set; } = true;
}

public class WorkoutTemplateResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public string Level { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public List<TemplateExerciseResponse> Exercises { get; set; } = new();
}

public class TemplateExerciseResponse
{
    public Guid Id { get; set; }
    public Guid ExerciseLibraryItemId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public string? MuscleGroup { get; set; }
    public int Sets { get; set; }
    public string? Reps { get; set; }
    public string? SuggestedLoad { get; set; }
    public int? RestSeconds { get; set; }
    public string? Notes { get; set; }
    public int OrderIndex { get; set; }
}

public class WorkoutTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public int Level { get; set; } = 1;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
