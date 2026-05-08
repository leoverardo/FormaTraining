using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class StudentProgress : BaseEntity
{
    public Guid StudentId { get; set; }
    public Guid TrainerId { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }
    public decimal? Chest { get; set; }
    public decimal? Waist { get; set; }
    public decimal? Abdomen { get; set; }
    public decimal? Hip { get; set; }
    public decimal? RightArm { get; set; }
    public decimal? LeftArm { get; set; }
    public decimal? RightThigh { get; set; }
    public decimal? LeftThigh { get; set; }
    public decimal? BodyFatPercentage { get; set; }
    public string? Notes { get; set; }
    public DateTime ProgressDate { get; set; } = DateTime.UtcNow;
    public Guid CreatedByUserId { get; set; }
    public ProgressCreatedByRole CreatedByRole { get; set; }

    public Student Student { get; set; } = null!;
    public Trainer Trainer { get; set; } = null!;
}
