using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class DataPrivacyRequest : BaseEntity
{
    public Guid UserId { get; set; }
    public DataPrivacyRequestType RequestType { get; set; }
    public DataPrivacyRequestStatus Status { get; set; } = DataPrivacyRequestStatus.Pending;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }

    public User User { get; set; } = null!;
}
