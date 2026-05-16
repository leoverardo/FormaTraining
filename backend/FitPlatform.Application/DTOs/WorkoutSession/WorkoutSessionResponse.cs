namespace FitPlatform.Application.DTOs.WorkoutSession;

public class WorkoutSessionResponse
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid WorkoutId { get; set; }
    public string WorkoutName { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ExerciseSessionResponse> Exercises { get; set; } = new();
}

public class ExerciseSessionResponse
{
    public Guid Id { get; set; }
    public Guid? WorkoutExerciseId { get; set; }
    public Guid ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
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
}
