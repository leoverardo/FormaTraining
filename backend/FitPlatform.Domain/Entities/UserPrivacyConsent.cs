using FitPlatform.Domain.Common;

namespace FitPlatform.Domain.Entities;

public class UserPrivacyConsent : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ConsentDefinitionId { get; set; }
    public bool IsGranted { get; set; }
    public DateTime? GrantedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime LastChangedAt { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public User User { get; set; } = null!;
    public ConsentDefinition ConsentDefinition { get; set; } = null!;
}
