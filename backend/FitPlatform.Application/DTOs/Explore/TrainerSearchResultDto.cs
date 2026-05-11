namespace FitPlatform.Application.DTOs.Explore;

public class TrainerSearchResultDto
{
    public Guid TrainerId { get; set; }
    public string? Slug { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string? Headline { get; set; }
    public string? Bio { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Neighborhood { get; set; }
    public List<string> Specialties { get; set; } = new();
    public string? ServiceMode { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public decimal? Rating { get; set; }
    public int ReviewsCount { get; set; }
    public int PublicPostsCount { get; set; }
    public int ActiveStudentsCountPublic { get; set; }
    public decimal? DistanceKm { get; set; }
    public bool IsFollowedByCurrentUser { get; set; }
    public bool IsSavedByCurrentUser { get; set; }
    public bool AcceptingStudents { get; set; }
}

public class TrainerSearchResponseDto
{
    public List<TrainerSearchResultDto> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
