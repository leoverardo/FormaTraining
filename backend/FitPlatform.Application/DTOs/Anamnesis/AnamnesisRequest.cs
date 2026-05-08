namespace FitPlatform.Application.DTOs.Anamnesis;

public class AnamnesisRequest
{
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
}

public class AnamnesisResponse
{
    public Guid Id { get; set; }
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
    public DateTime UpdatedAt { get; set; }
}
