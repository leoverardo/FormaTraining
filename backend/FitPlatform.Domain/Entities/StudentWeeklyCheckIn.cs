using FitPlatform.Domain.Common;

namespace FitPlatform.Domain.Entities;

public class StudentWeeklyCheckIn : BaseEntity
{
    public Guid StudentId { get; set; }
    public Guid TrainerId { get; set; }
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public decimal? Weight { get; set; }
    public int? MoodLevel { get; set; }
    public int? EnergyLevel { get; set; }
    public int? SleepQuality { get; set; }
    public int? DietAdherence { get; set; }
    public int? TrainingAdherence { get; set; }
    public int? CompletedWorkoutsCount { get; set; }
    public bool HasPain { get; set; } = false;
    public string? PainDescription { get; set; }
    public string? Notes { get; set; }
    public string? PhotoUrl { get; set; }

    public Student Student { get; set; } = null!;
    public Trainer Trainer { get; set; } = null!;
    public ICollection<ProgressComment> Comments { get; set; } = new List<ProgressComment>();
}
