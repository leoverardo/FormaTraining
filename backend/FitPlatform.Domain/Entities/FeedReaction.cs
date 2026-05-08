using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class FeedReaction : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? TrainerId { get; set; }
    public Guid? StudentId { get; set; }
    public string FeedItemKey { get; set; } = string.Empty;
    public string RelatedEntityType { get; set; } = string.Empty;
    public Guid RelatedEntityId { get; set; }
    public ReactionType ReactionType { get; set; } = ReactionType.Like;

    public User User { get; set; } = null!;
}
