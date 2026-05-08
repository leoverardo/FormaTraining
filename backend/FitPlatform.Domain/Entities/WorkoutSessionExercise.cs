using FitPlatform.Domain.Common;

namespace FitPlatform.Domain.Entities;

public class WorkoutSessionExercise : BaseEntity
{
    public Guid WorkoutSessionId { get; set; }
    public Guid ExerciseId { get; set; }
    public int? SetsCompleted { get; set; }
    public string? RepsCompleted { get; set; }
    public string? LoadUsed { get; set; }
    public int? DifficultyLevel { get; set; }
    public string? Notes { get; set; }

    public WorkoutSession WorkoutSession { get; set; } = null!;
    public Exercise Exercise { get; set; } = null!;
}
