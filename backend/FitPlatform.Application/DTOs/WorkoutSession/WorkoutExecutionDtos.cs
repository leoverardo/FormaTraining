using System.ComponentModel.DataAnnotations;

namespace FitPlatform.Application.DTOs.WorkoutSession;

public class WorkoutSessionExecutionResponse
{
    public Guid SessionId { get; set; }
    public Guid WorkoutId { get; set; }
    public string WorkoutName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TotalExercises { get; set; }
    public int CompletedExercises { get; set; }
    public int TotalSets { get; set; }
    public int CompletedSets { get; set; }
    public int DurationSeconds { get; set; }
    public List<WorkoutExecutionExerciseResponse> Exercises { get; set; } = new();
}

public class WorkoutExecutionExerciseResponse
{
    public Guid WorkoutSessionExerciseId { get; set; }
    public Guid ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public string? ExerciseImageUrl { get; set; }
    public string? ExerciseVideoUrl { get; set; }
    public string? ExerciseInstructions { get; set; }
    public string? PrescribedNotes { get; set; }
    public string? ExecutionNotes { get; set; }
    public int OrderIndex { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int PrescribedSets { get; set; }
    public string? PrescribedReps { get; set; }
    public string? PrescribedLoad { get; set; }
    public int? PrescribedRestSeconds { get; set; }
    public string? LastExecutionSummary { get; set; }
    public DateTime? LastExecutionDate { get; set; }
    public List<WorkoutExecutionSetResponse> Sets { get; set; } = new();
}

public class WorkoutExecutionSetResponse
{
    public Guid Id { get; set; }
    public int SetNumber { get; set; }
    public string? PrescribedReps { get; set; }
    public string? PrescribedLoad { get; set; }
    public int? PrescribedRestSeconds { get; set; }
    public string? ActualReps { get; set; }
    public string? ActualLoad { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
}

public class UpdateWorkoutSessionSetRequest
{
    [MaxLength(50)]
    public string? ActualReps { get; set; }

    [MaxLength(50)]
    public string? ActualLoad { get; set; }

    public bool? IsCompleted { get; set; }

    [MaxLength(400)]
    public string? Notes { get; set; }
}

public class CompleteWorkoutSessionExerciseRequest
{
    public bool IsCompleted { get; set; } = true;

    [MaxLength(400)]
    public string? Notes { get; set; }
}
