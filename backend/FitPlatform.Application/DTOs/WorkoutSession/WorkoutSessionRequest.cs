namespace FitPlatform.Application.DTOs.WorkoutSession;

public class StartWorkoutSessionRequest
{
    public Guid WorkoutId { get; set; }
    public DateTime? ScheduledDate { get; set; }
}

public class CompleteWorkoutSessionRequest
{
    public string? Notes { get; set; }
    public List<ExerciseSessionUpdate> Exercises { get; set; } = new();
}

public class ExerciseSessionUpdate
{
    public Guid ExerciseId { get; set; }
    public int? SetsCompleted { get; set; }
    public string? RepsCompleted { get; set; }
    public string? LoadUsed { get; set; }
    public int? DifficultyLevel { get; set; }
    public string? Notes { get; set; }
}
