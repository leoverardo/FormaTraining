using System.ComponentModel.DataAnnotations;

namespace FitPlatform.Application.DTOs.Chat;

public class ConversationListItemResponse
{
    public Guid ConversationId { get; set; }
    public Guid TrainerId { get; set; }
    public Guid StudentId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string? ParticipantAvatarUrl { get; set; }
    public string? LastMessagePreview { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
}

public class ChatMessageResponse
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderUserId { get; set; }
    public string SenderRole { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

public class ConversationDetailsResponse
{
    public Guid ConversationId { get; set; }
    public Guid TrainerId { get; set; }
    public Guid StudentId { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public List<ChatMessageResponse> Messages { get; set; } = new();
}

public class SendChatMessageRequest
{
    public Guid? ConversationId { get; set; }
    public Guid? StudentId { get; set; }
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
}
