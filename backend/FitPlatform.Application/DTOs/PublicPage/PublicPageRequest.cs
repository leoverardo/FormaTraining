namespace FitPlatform.Application.DTOs.PublicPage;

public class PublicPageRequest
{
    public string? PublicSlug { get; set; }
    public bool PublicPageEnabled { get; set; }
    public bool PublicSearchEnabled { get; set; } = false;
    public bool AcceptingStudents { get; set; } = true;
    public string? PublicHeadline { get; set; }
    public string? PublicDescription { get; set; }
    public string? WhatsappNumber { get; set; }
    public bool ShowInstagram { get; set; } = true;
    public bool ShowTestimonials { get; set; } = true;
    public string? BannerUrl { get; set; }
    public string? PublicBannerUrl { get; set; }
    public Guid? PublicBannerMediaId { get; set; }
    public string? WelcomeMessage { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
}

public class PublicPageResponse
{
    public Guid TrainerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string? PublicSlug { get; set; }
    public bool PublicPageEnabled { get; set; }
    public bool PublicSearchEnabled { get; set; }
    public bool AcceptingStudents { get; set; }
    public string? PublicHeadline { get; set; }
    public string? PublicDescription { get; set; }
    public string? Bio { get; set; }
    public string? Specialties { get; set; }
    public string? Instagram { get; set; }
    public string? WhatsappNumber { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string? PublicBannerUrl { get; set; }
    public Guid? PublicBannerMediaId { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public bool ShowInstagram { get; set; }
    public bool ShowTestimonials { get; set; }
    public string? WelcomeMessage { get; set; }
    public PublicPageStatsDto Stats { get; set; } = new();
    public List<PublicPostItemDto> RecentPosts { get; set; } = new();
    public List<TestimonialPublicItem> Testimonials { get; set; } = new();
    public List<TransformationPublicItem> Transformations { get; set; } = new();
}

public class PublicPageStatsDto
{
    public int ActiveStudentsCount { get; set; }
    public int PostsCount { get; set; }
    public int TestimonialsCount { get; set; }
    public int TransformationsCount { get; set; }
}

public class PublicPostItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TestimonialPublicItem
{
    public string Text { get; set; } = string.Empty;
    public int? Rating { get; set; }
}

public class TransformationPublicItem
{
    public string? BeforePhotoUrl { get; set; }
    public string? AfterPhotoUrl { get; set; }
    public string? Description { get; set; }
}
