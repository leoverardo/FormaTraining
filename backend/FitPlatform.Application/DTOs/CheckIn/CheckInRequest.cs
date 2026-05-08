namespace FitPlatform.Application.DTOs.CheckIn;

public class CommentRequest
{
    public string Comment { get; set; } = string.Empty;
}

public class CheckInRequest
{
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
}
