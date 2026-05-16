using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class UserLegalAcceptance : BaseEntity
{
    public Guid? UserId { get; set; }
    public string? Email { get; set; }
    public Guid PrivacyPolicyVersionId { get; set; }
    public Guid TermsOfUseVersionId { get; set; }
    public DateTime AcceptedAt { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public LegalAcceptanceSource Source { get; set; } = LegalAcceptanceSource.Registration;

    public User? User { get; set; }
    public PrivacyPolicyVersion PrivacyPolicyVersion { get; set; } = null!;
    public PrivacyPolicyVersion TermsOfUseVersion { get; set; } = null!;
}
