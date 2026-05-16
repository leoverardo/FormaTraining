using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class StudentHabit : BaseEntity
{
    public Guid StudentId { get; set; }
    public Guid TrainerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public HabitCategory Category { get; set; } = HabitCategory.Custom;
    public HabitFrequency Frequency { get; set; } = HabitFrequency.Daily;
    public decimal? TargetValue { get; set; }
    public string? TargetUnit { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? InactivatedAt { get; set; }

    public Student Student { get; set; } = null!;
    public Trainer Trainer { get; set; } = null!;
    public ICollection<StudentHabitLog> Logs { get; set; } = new List<StudentHabitLog>();
}
