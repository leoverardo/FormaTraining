namespace FitPlatform.Application.DTOs.Feed;

public class FeedCommentDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorAvatarUrl { get; set; }
    public string AuthorRole { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AddFeedCommentRequest
{
    public string Comment { get; set; } = string.Empty;
}
