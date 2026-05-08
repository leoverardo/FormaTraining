using FitPlatform.Domain.Common;

namespace FitPlatform.Domain.Entities;

public class StudentWorkoutSchedule : BaseEntity
{
    public Guid TrainerId { get; set; }
    public Guid StudentId { get; set; }
    public Guid WorkoutId { get; set; }
    public int DayOfWeek { get; set; }
    public string? Notes { get; set; }

    public Trainer Trainer { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public Workout Workout { get; set; } = null!;
}
