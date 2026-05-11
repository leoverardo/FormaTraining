using FitPlatform.Domain.Common;

namespace FitPlatform.Domain.Entities;

public class SavedTrainer : BaseEntity
{
    public Guid TrainerId { get; set; }
    public Guid StudentProfileId { get; set; }

    public Trainer Trainer { get; set; } = null!;
    public StudentProfile StudentProfile { get; set; } = null!;
}
