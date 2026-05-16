namespace FitPlatform.Application.DTOs.Testimonials;

public class TestimonialRequest
{
    public string Text { get; set; } = string.Empty;
    public int? Rating { get; set; }
}

public class TestimonialResponse
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int? Rating { get; set; }
    public bool ApprovedByStudent { get; set; }
    public bool Published { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TransformationRequest
{
    public Guid? BeforeMediaId { get; set; }
    public Guid? AfterMediaId { get; set; }
    public string? Description { get; set; }
}

public class TransformationResponse
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? BeforePhotoUrl { get; set; }
    public string? AfterPhotoUrl { get; set; }
    public Guid? BeforeMediaId { get; set; }
    public Guid? AfterMediaId { get; set; }
    public string? Description { get; set; }
    public bool ApprovedByStudent { get; set; }
    public bool Published { get; set; }
    public DateTime CreatedAt { get; set; }
}
