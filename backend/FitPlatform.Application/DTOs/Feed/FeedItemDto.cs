namespace FitPlatform.Application.DTOs.Feed;

public class FeedItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorAvatarUrl { get; set; }
    public string AuthorRole { get; set; } = string.Empty;
    public Guid TrainerId { get; set; }
    public Guid? StudentId { get; set; }
    public string? Title { get; set; }
    public string? Text { get; set; }
    public string? MediaUrl { get; set; }
    public string MediaType { get; set; } = "None";
    public string? ThumbnailUrl { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string Visibility { get; set; } = string.Empty;
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public bool IsSavedByCurrentUser { get; set; }
    public string RelatedEntityType { get; set; } = string.Empty;
    public Guid RelatedEntityId { get; set; }
}
