using FitPlatform.Domain.Common;

namespace FitPlatform.Domain.Entities;

public class StudentMonthlyGoal : BaseEntity
{
    public Guid StudentId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int WorkoutTarget { get; set; } = 8;
    public int HabitDaysTarget { get; set; } = 20;
    public int CheckInTarget { get; set; } = 4;

    public Student Student { get; set; } = null!;
}
