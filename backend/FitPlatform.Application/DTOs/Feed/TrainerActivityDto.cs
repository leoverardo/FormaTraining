namespace FitPlatform.Application.DTOs.Feed;

public class TrainerActivityDto
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? StudentName { get; set; }
    public string? StudentAvatarUrl { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public DateTime CreatedAt { get; set; }
}
