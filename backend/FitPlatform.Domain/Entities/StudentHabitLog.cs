using FitPlatform.Domain.Common;

namespace FitPlatform.Domain.Entities;

public class StudentHabitLog : BaseEntity
{
    public Guid HabitId { get; set; }
    public Guid StudentId { get; set; }
    public DateTime Date { get; set; }
    public bool IsCompleted { get; set; }
    public decimal? Value { get; set; }
    public string? Note { get; set; }
    public DateTime? CompletedAt { get; set; }

    public StudentHabit Habit { get; set; } = null!;
    public Student Student { get; set; } = null!;
}
