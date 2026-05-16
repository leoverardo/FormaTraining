using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class DataPrivacyRequest : BaseEntity
{
    public Guid? UserId { get; set; }
    public string RequesterEmail { get; set; } = string.Empty;
    public DataPrivacyRequestType RequestType { get; set; }
    public DataPrivacyRequestStatus Status { get; set; } = DataPrivacyRequestStatus.Pending;
    public string? Description { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }

    public User? User { get; set; }
}
