namespace FitPlatform.Application.DTOs.Explore;

public class ExploreTrainerQuery
{
    public string? Name { get; set; }
    public string? Search { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Neighborhood { get; set; }
    public string? Specialty { get; set; }
    public string? ServiceMode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public decimal? RadiusKm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
