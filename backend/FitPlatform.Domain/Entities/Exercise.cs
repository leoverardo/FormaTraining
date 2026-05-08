using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class Exercise : BaseEntity
{
    public Guid TrainerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? MuscleGroup { get; set; }
    public string? Description { get; set; }
    public string? Instructions { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public ExerciseLevel Level { get; set; } = ExerciseLevel.Beginner;

    // Media FKs (nullable — fallback to URL fields)
    public Guid? ImageMediaId { get; set; }
    public Guid? VideoMediaId { get; set; }
    public MediaFile? ImageMedia { get; set; }
    public MediaFile? VideoMedia { get; set; }

    public Trainer Trainer { get; set; } = null!;
    public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = new List<WorkoutExercise>();
}
