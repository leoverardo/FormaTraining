using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class StudentInvite : BaseEntity
{
    public Guid StudentId { get; set; }
    public Guid TrainerId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public StudentInviteStatus Status { get; set; } = StudentInviteStatus.Pending;
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }

    public Student Student { get; set; } = null!;
    public Trainer Trainer { get; set; } = null!;
}
