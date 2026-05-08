using FitPlatform.Domain.Common;

namespace FitPlatform.Domain.Entities;

public class StudentTestimonial : BaseEntity
{
    public Guid TrainerId { get; set; }
    public Guid StudentId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int? Rating { get; set; }
    public bool ApprovedByStudent { get; set; } = false;
    public bool Published { get; set; } = false;

    public Trainer Trainer { get; set; } = null!;
    public Student Student { get; set; } = null!;
}

public class StudentTransformation : BaseEntity
{
    public Guid TrainerId { get; set; }
    public Guid StudentId { get; set; }
    public string? BeforePhotoUrl { get; set; }
    public string? AfterPhotoUrl { get; set; }
    public Guid? BeforeMediaId { get; set; }
    public Guid? AfterMediaId { get; set; }
    public string? Description { get; set; }
    public bool ApprovedByStudent { get; set; } = false;
    public bool Published { get; set; } = false;

    public Trainer Trainer { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public MediaFile? BeforeMedia { get; set; }
    public MediaFile? AfterMedia { get; set; }
}
