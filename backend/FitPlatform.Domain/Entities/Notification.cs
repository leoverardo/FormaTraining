using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? TrainerId { get; set; }
    public Guid? StudentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    public User User { get; set; } = null!;
}
