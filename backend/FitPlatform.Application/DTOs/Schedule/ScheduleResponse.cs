namespace FitPlatform.Application.DTOs.Schedule;

public class ScheduleResponse
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid WorkoutId { get; set; }
    public string WorkoutName { get; set; } = string.Empty;
    public int DayOfWeek { get; set; }
    public string DayName { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
