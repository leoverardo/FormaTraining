using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class ChatMessage : BaseEntity
{
    public Guid ConversationId { get; set; }
    public Guid SenderUserId { get; set; }
    public UserRole SenderRole { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime? ReadAt { get; set; }
    public Guid? AttachmentMediaId { get; set; }

    public Conversation Conversation { get; set; } = null!;
    public User SenderUser { get; set; } = null!;
    public MediaFile? AttachmentMedia { get; set; }
}

