using FitPlatform.Domain.Enums;

namespace FitPlatform.Application.DTOs.Posts;

public class PostRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Content { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public PostStatus Status { get; set; } = PostStatus.Draft;
    public PostVisibility Visibility { get; set; } = PostVisibility.StudentsOnly;
    public string? Tags { get; set; }
}
