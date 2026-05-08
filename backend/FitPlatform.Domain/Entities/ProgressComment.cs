using FitPlatform.Domain.Common;

namespace FitPlatform.Domain.Entities;

public class ProgressComment : BaseEntity
{
    public Guid? StudentProgressId { get; set; }
    public Guid? StudentWeeklyCheckInId { get; set; }
    public Guid TrainerId { get; set; }
    public Guid StudentId { get; set; }
    public string Comment { get; set; } = string.Empty;

    public StudentProgress? StudentProgress { get; set; }
    public StudentWeeklyCheckIn? WeeklyCheckIn { get; set; }
    public Trainer Trainer { get; set; } = null!;
    public Student Student { get; set; } = null!;
}
