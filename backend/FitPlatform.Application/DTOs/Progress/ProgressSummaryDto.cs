namespace FitPlatform.Application.DTOs.Progress;

public class ProgressSummaryDto
{
    public ProgressRecordDto? LatestProgress { get; set; }
    public ProgressRecordDto? PreviousProgress { get; set; }
    public ProgressComparisonDto? Comparison { get; set; }
    public List<ProgressTimelineItemDto> Timeline { get; set; } = new();
}

public class ProgressRecordDto
{
    public Guid Id { get; set; }
    public DateTime ProgressDate { get; set; }
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
    public DateTime CreatedAt { get; set; }
}

public class ProgressComparisonDto
{
    public decimal? WeightDifference { get; set; }
    public decimal? HeightDifference { get; set; }
    public decimal? WaistDifference { get; set; }
    public decimal? ChestDifference { get; set; }
    public decimal? AbdomenDifference { get; set; }
    public decimal? HipDifference { get; set; }
    public decimal? BodyFatDifference { get; set; }
    public DateTime? ComparedToDate { get; set; }
}

public class ProgressTimelineItemDto
{
    public Guid Id { get; set; }
    public DateTime ProgressDate { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Waist { get; set; }
    public decimal? Chest { get; set; }
    public decimal? BodyFatPercentage { get; set; }
}
