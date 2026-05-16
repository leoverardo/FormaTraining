using FitPlatform.Domain.Common;

namespace FitPlatform.Domain.Entities;

public class StudentNutritionGuidance : BaseEntity
{
    public Guid StudentId { get; set; }
    public Guid TrainerId { get; set; }
    public string GuidanceText { get; set; } = string.Empty;
    public string? StrategicNotes { get; set; }
    public Guid? MediaId { get; set; }

    public Student Student { get; set; } = null!;
    public Trainer Trainer { get; set; } = null!;
    public MediaFile? Media { get; set; }
}
