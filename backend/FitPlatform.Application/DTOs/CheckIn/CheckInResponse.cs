namespace FitPlatform.Application.DTOs.CheckIn;

public class CheckInResponse
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public decimal? Weight { get; set; }
    public int? MoodLevel { get; set; }
    public int? EnergyLevel { get; set; }
    public int? SleepQuality { get; set; }
    public int? DietAdherence { get; set; }
    public int? TrainingAdherence { get; set; }
    public int? CompletedWorkoutsCount { get; set; }
    public bool HasPain { get; set; }
    public string? PainDescription { get; set; }
    public string? Notes { get; set; }
    public string? PhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CommentResponse> Comments { get; set; } = new();
}

public class CommentResponse
{
    public Guid Id { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorRole { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
