using FitPlatform.Domain.Common;

namespace FitPlatform.Domain.Entities;

public class TrainerStudentNote : BaseEntity
{
    public Guid TrainerId { get; set; }
    public Guid StudentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public bool IsPinned { get; set; } = false;

    public Trainer Trainer { get; set; } = null!;
    public Student Student { get; set; } = null!;
}
