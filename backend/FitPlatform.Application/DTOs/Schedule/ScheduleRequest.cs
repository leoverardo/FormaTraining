namespace FitPlatform.Application.DTOs.Schedule;

public class ScheduleRequest
{
    public Guid WorkoutId { get; set; }
    public int DayOfWeek { get; set; }
    public string? Notes { get; set; }
}
