using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class TrainerServiceOffer : BaseEntity
{
    public Guid TrainerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public TrainerServiceBillingType BillingType { get; set; } = TrainerServiceBillingType.OneTime;
    public int? DurationDays { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPublic { get; set; } = true;
    public int DisplayOrder { get; set; }

    public Trainer Trainer { get; set; } = null!;
    public ICollection<TrainerServiceOrder> Orders { get; set; } = new List<TrainerServiceOrder>();
}
