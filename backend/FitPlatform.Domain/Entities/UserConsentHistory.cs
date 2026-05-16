using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class UserConsentHistory : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ConsentDefinitionId { get; set; }
    public ConsentChangeAction Action { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? MetadataJson { get; set; }

    public User User { get; set; } = null!;
    public ConsentDefinition ConsentDefinition { get; set; } = null!;
}
