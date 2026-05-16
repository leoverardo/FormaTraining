using FitPlatform.Domain.Common;

namespace FitPlatform.Domain.Entities;

public class WorkoutSessionSet : BaseEntity
{
    public Guid WorkoutSessionExerciseId { get; set; }
    public int SetNumber { get; set; }
    public string? PrescribedReps { get; set; }
    public string? PrescribedLoad { get; set; }
    public int? PrescribedRestSeconds { get; set; }
    public string? ActualReps { get; set; }
    public string? ActualLoad { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }

    public WorkoutSessionExercise WorkoutSessionExercise { get; set; } = null!;
}
