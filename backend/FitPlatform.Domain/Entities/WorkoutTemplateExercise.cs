using FitPlatform.Domain.Common;

namespace FitPlatform.Domain.Entities;

public class WorkoutTemplateExercise : BaseEntity
{
    public Guid WorkoutTemplateId { get; set; }
    public Guid ExerciseLibraryItemId { get; set; }
    public int Sets { get; set; }
    public string? Reps { get; set; }
    public string? SuggestedLoad { get; set; }
    public int? RestSeconds { get; set; }
    public string? Notes { get; set; }
    public int OrderIndex { get; set; }

    public WorkoutTemplate WorkoutTemplate { get; set; } = null!;
    public ExerciseLibraryItem ExerciseLibraryItem { get; set; } = null!;
}
