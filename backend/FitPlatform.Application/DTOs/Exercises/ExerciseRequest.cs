using FitPlatform.Domain.Enums;

namespace FitPlatform.Application.DTOs.Exercises;

public class ExerciseRequest
{
    public string Name { get; set; } = string.Empty;
    public string? MuscleGroup { get; set; }
    public string? Description { get; set; }
    public string? Instructions { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? ImageMediaId { get; set; }
    public string? VideoUrl { get; set; }
    public Guid? VideoMediaId { get; set; }
    public ExerciseLevel Level { get; set; } = ExerciseLevel.Beginner;
}
