namespace FitPlatform.Application.DTOs.Progress;

public class ProgressPhotoCompareDto
{
    public ProgressPhotoItemDto? BeforePhoto { get; set; }
    public ProgressPhotoItemDto? AfterPhoto { get; set; }
    public int TotalPhotos { get; set; }
    public DateTime? FirstPhotoDate { get; set; }
    public DateTime? LastPhotoDate { get; set; }
    public List<ProgressPhotoItemDto> AllPhotos { get; set; } = new();
}

public class ProgressPhotoItemDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime PhotoDate { get; set; }
    public string CreatedByRole { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
