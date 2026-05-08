using FitPlatform.Domain.Common;

namespace FitPlatform.Domain.Entities;

public class FeedComment : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? TrainerId { get; set; }
    public Guid? StudentId { get; set; }
    public string FeedItemKey { get; set; } = string.Empty;
    public string RelatedEntityType { get; set; } = string.Empty;
    public Guid RelatedEntityId { get; set; }
    public string Comment { get; set; } = string.Empty;

    public User User { get; set; } = null!;
}
