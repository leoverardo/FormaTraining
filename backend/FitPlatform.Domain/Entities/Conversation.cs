using FitPlatform.Domain.Common;

namespace FitPlatform.Domain.Entities;

public class Conversation : BaseEntity
{
    public Guid TrainerId { get; set; }
    public Guid StudentId { get; set; }
    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

    public Trainer Trainer { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}

