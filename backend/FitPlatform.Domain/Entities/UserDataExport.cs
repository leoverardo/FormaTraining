using FitPlatform.Domain.Common;

namespace FitPlatform.Domain.Entities;

public class UserDataExport : BaseEntity
{
    public Guid UserId { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? GeneratedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? FileUrl { get; set; }
    public string Status { get; set; } = "Pending";
    public string? ErrorMessage { get; set; }
    public string? PayloadJson { get; set; }

    public User User { get; set; } = null!;
}
