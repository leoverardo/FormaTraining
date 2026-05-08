using FitPlatform.Domain.Common;

namespace FitPlatform.Domain.Entities;

public class StudentAnamnesis : BaseEntity
{
    public Guid StudentId { get; set; }
    public Guid TrainerId { get; set; }
    public string? MainGoal { get; set; }
    public string? TrainingExperience { get; set; }
    public string? Injuries { get; set; }
    public string? HealthRestrictions { get; set; }
    public int? AvailableDaysPerWeek { get; set; }
    public string? TrainingLocation { get; set; }
    public string? AvailableEquipment { get; set; }
    public int? SleepQuality { get; set; }
    public int? StressLevel { get; set; }
    public string? FoodRoutineNotes { get; set; }
    public string? AdditionalNotes { get; set; }
    public DateTime? SubmittedAt { get; set; }

    public Student Student { get; set; } = null!;
    public Trainer Trainer { get; set; } = null!;
}
