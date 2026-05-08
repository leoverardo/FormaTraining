using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class StudentProgressPhoto : BaseEntity
{
    public Guid StudentId { get; set; }
    public Guid TrainerId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime PhotoDate { get; set; } = DateTime.UtcNow;
    public Guid CreatedByUserId { get; set; }
    public ProgressCreatedByRole CreatedByRole { get; set; }

    // Media FK (nullable — fallback to ImageUrl)
    public Guid? MediaAssetId { get; set; }
    public MediaFile? MediaAsset { get; set; }

    public Student Student { get; set; } = null!;
    public Trainer Trainer { get; set; } = null!;
}
