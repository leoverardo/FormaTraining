using FitPlatform.Domain.Enums;

namespace FitPlatform.Application.DTOs.Workouts;

public class WorkoutRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public ExerciseLevel Level { get; set; } = ExerciseLevel.Beginner;
    public string? Description { get; set; }
    public WorkoutStatus Status { get; set; } = WorkoutStatus.Active;
}

public class WorkoutExerciseRequest
{
    public Guid ExerciseId { get; set; }
    public int Sets { get; set; }
    public string? Reps { get; set; }
    public string? SuggestedLoad { get; set; }
    public int? RestSeconds { get; set; }
    public string? Notes { get; set; }
    public int OrderIndex { get; set; }
}
