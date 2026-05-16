namespace FitPlatform.Application.DTOs.Exercises;

public class ExerciseResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? MuscleGroup { get; set; }
    public string? Description { get; set; }
    public string? Instructions { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? ImageMediaId { get; set; }
    public string? VideoUrl { get; set; }
    public Guid? VideoMediaId { get; set; }
    public string Level { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
