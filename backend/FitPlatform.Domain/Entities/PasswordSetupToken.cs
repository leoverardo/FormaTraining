using FitPlatform.Domain.Common;

namespace FitPlatform.Domain.Entities;

public class PasswordSetupToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }

    public User User { get; set; } = null!;
}
