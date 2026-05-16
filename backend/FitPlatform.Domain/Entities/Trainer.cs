using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class Trainer : BaseEntity
{
    public Guid UserId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Bio { get; set; }
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }

    // Professional
    public string? CPF { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? CREF { get; set; }
    public string? Specialties { get; set; }
    public string? Instagram { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? SecondaryColor { get; set; }

    // Address
    public string? ZipCode { get; set; }
    public string? Street { get; set; }
    public string? AddressNumber { get; set; }
    public string? Complement { get; set; }
    public string? Neighborhood { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ServiceMode { get; set; }
    public bool PublicSearchEnabled { get; set; } = false;
    public bool AcceptingStudents { get; set; } = true;

    // Public page
    public string? PublicSlug { get; set; }
    public bool PublicPageEnabled { get; set; } = false;
    public string? PublicHeadline { get; set; }
    public string? PublicDescription { get; set; }
    public string? WhatsappNumber { get; set; }
    public bool ShowInstagram { get; set; } = true;
    public bool ShowTestimonials { get; set; } = true;
    public string? BannerUrl { get; set; }

    // Visual customization
    public string? WelcomeMessage { get; set; }

    // Media FKs (nullable — fallback to URL fields if null)
    public Guid? ProfilePhotoMediaId { get; set; }
    public Guid? LogoMediaId { get; set; }
    public Guid? BannerMediaId { get; set; }
    public Guid? PublicBannerMediaId { get; set; }
    public MediaFile? ProfilePhotoMedia { get; set; }
    public MediaFile? LogoMedia { get; set; }
    public MediaFile? BannerMedia { get; set; }
    public MediaFile? PublicBannerMedia { get; set; }

    public User User { get; set; } = null!;
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
    public ICollection<Workout> Workouts { get; set; } = new List<Workout>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<TrainerSubscription> Subscriptions { get; set; } = new List<TrainerSubscription>();
    public ICollection<TrainerServiceOffer> ServiceOffers { get; set; } = new List<TrainerServiceOffer>();
    public ICollection<TrainerServiceOrder> ServiceOrders { get; set; } = new List<TrainerServiceOrder>();
}
