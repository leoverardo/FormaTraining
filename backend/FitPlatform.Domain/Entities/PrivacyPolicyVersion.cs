using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class PrivacyPolicyVersion : BaseEntity
{
    public LegalDocumentType DocumentType { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ContentMarkdown { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
}
