using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class TermsDocument : BaseEntity
{
    public TermsType Type { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool Active { get; set; } = true;

    public ICollection<UserConsent> Consents { get; set; } = new List<UserConsent>();
}

public class UserConsent : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid TermsDocumentId { get; set; }
    public DateTime AcceptedAt { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public User User { get; set; } = null!;
    public TermsDocument TermsDocument { get; set; } = null!;
}
