using FitPlatform.Domain.Common;

namespace FitPlatform.Domain.Entities;

public class WorkoutExercise : BaseEntity
{
    public Guid WorkoutId { get; set; }
    public Guid ExerciseId { get; set; }
    public int Sets { get; set; }
    public string? Reps { get; set; }
    public string? SuggestedLoad { get; set; }
    public int? RestSeconds { get; set; }
    public string? Notes { get; set; }
    public int OrderIndex { get; set; }

    public Workout Workout { get; set; } = null!;
    public Exercise Exercise { get; set; } = null!;
}
