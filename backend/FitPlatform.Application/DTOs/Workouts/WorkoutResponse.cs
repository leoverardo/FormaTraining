using FitPlatform.Application.DTOs.Exercises;

namespace FitPlatform.Application.DTOs.Workouts;

public class WorkoutResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public string Level { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<WorkoutExerciseResponse> Exercises { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class WorkoutExerciseResponse
{
    public Guid Id { get; set; }
    public int Sets { get; set; }
    public string? Reps { get; set; }
    public string? SuggestedLoad { get; set; }
    public int? RestSeconds { get; set; }
    public string? Notes { get; set; }
    public int OrderIndex { get; set; }
    public ExerciseResponse Exercise { get; set; } = null!;
}
