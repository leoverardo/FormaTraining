using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class WorkoutSession : BaseEntity
{
    public Guid StudentId { get; set; }
    public Guid TrainerId { get; set; }
    public Guid WorkoutId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public WorkoutSessionStatus Status { get; set; } = WorkoutSessionStatus.Scheduled;
    public string? Notes { get; set; }

    public Student Student { get; set; } = null!;
    public Trainer Trainer { get; set; } = null!;
    public Workout Workout { get; set; } = null!;
    public ICollection<WorkoutSessionExercise> ExerciseSessions { get; set; } = new List<WorkoutSessionExercise>();
}
