using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class ExerciseLibraryItem : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? MuscleGroup { get; set; }
    public string? Description { get; set; }
    public string? Instructions { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public ExerciseLevel Level { get; set; } = ExerciseLevel.Beginner;
    public bool IsActive { get; set; } = true;

    public ICollection<WorkoutTemplateExercise> WorkoutTemplateExercises { get; set; } = new List<WorkoutTemplateExercise>();
}
