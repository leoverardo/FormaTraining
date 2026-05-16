namespace FitPlatform.Application.DTOs.Progress;

public class StudentProgressRequest
{
    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }
    public decimal? Chest { get; set; }
    public decimal? Waist { get; set; }
    public decimal? Abdomen { get; set; }
    public decimal? Hip { get; set; }
    public decimal? RightArm { get; set; }
    public decimal? LeftArm { get; set; }
    public decimal? RightThigh { get; set; }
    public decimal? LeftThigh { get; set; }
    public decimal? BodyFatPercentage { get; set; }
    public string? Notes { get; set; }
    public DateTime? ProgressDate { get; set; }
}

public class StudentProgressPhotoRequest
{
    public Guid MediaAssetId { get; set; }
    public string? Description { get; set; }
    public DateTime? PhotoDate { get; set; }
}
