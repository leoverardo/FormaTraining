namespace FitPlatform.Application.DTOs.Posts;

public class PostResponse
{
    public Guid Id { get; set; }
    public Guid TrainerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Content { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
