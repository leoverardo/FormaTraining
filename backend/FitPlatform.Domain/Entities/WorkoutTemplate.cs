using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class WorkoutTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public ExerciseLevel Level { get; set; } = ExerciseLevel.Beginner;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<WorkoutTemplateExercise> TemplateExercises { get; set; } = new List<WorkoutTemplateExercise>();
}
