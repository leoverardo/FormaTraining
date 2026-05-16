using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class SecurityIncident : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SecurityIncidentSeverity Severity { get; set; } = SecurityIncidentSeverity.Medium;
    public SecurityIncidentStatus Status { get; set; } = SecurityIncidentStatus.Identified;
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? ReportedToAuthorityAt { get; set; }
    public DateTime? ReportedToUsersAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? Notes { get; set; }
    public Guid? CreatedByUserId { get; set; }

    public User? CreatedByUser { get; set; }
}
