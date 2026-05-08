using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class Post : BaseEntity
{
    public Guid TrainerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Content { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public PostStatus Status { get; set; } = PostStatus.Draft;
    public PostVisibility Visibility { get; set; } = PostVisibility.StudentsOnly;
    public string? Tags { get; set; }
    public DateTime? PublishedAt { get; set; }

    // Media FKs (nullable — fallback to URL fields)
    public Guid? CoverMediaId { get; set; }
    public Guid? VideoMediaId { get; set; }
    public MediaFile? CoverMedia { get; set; }
    public MediaFile? VideoMedia { get; set; }

    public Trainer Trainer { get; set; } = null!;
}
