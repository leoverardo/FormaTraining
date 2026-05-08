using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class Workout : BaseEntity
{
    public Guid TrainerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public ExerciseLevel Level { get; set; } = ExerciseLevel.Beginner;
    public string? Description { get; set; }
    public WorkoutStatus Status { get; set; } = WorkoutStatus.Active;

    public Trainer Trainer { get; set; } = null!;
    public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = new List<WorkoutExercise>();
    public ICollection<StudentWorkoutSchedule> Schedules { get; set; } = new List<StudentWorkoutSchedule>();
}
