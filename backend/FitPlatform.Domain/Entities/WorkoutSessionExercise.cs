using FitPlatform.Domain.Common;

namespace FitPlatform.Domain.Entities;

public class WorkoutSessionExercise : BaseEntity
{
    public Guid WorkoutSessionId { get; set; }
    public Guid? WorkoutExerciseId { get; set; }
    public Guid ExerciseId { get; set; }
    public int PrescribedSets { get; set; }
    public string? PrescribedReps { get; set; }
    public string? PrescribedLoad { get; set; }
    public int? PrescribedRestSeconds { get; set; }
    public string? PrescribedNotes { get; set; }
    public int OrderIndex { get; set; }
    public int? SetsCompleted { get; set; }
    public string? RepsCompleted { get; set; }
    public string? LoadUsed { get; set; }
    public int? DifficultyLevel { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }

    public WorkoutSession WorkoutSession { get; set; } = null!;
    public WorkoutExercise? WorkoutExercise { get; set; }
    public Exercise Exercise { get; set; } = null!;
    public ICollection<WorkoutSessionSet> Sets { get; set; } = new List<WorkoutSessionSet>();
}
