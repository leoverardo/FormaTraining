using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class TrainerOnboarding : BaseEntity
{
    // Personal data
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? CPF { get; set; }
    public DateTime? BirthDate { get; set; }

    // Professional data
    public string? BrandName { get; set; }
    public string? CREF { get; set; }
    public string? Bio { get; set; }
    public string? Specialties { get; set; }
    public string? Instagram { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }

    // Address
    public string? ZipCode { get; set; }
    public string? Street { get; set; }
    public string? AddressNumber { get; set; }
    public string? Complement { get; set; }
    public string? Neighborhood { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }

    // Plan selection
    public Guid? SelectedPlatformPlanId { get; set; }
    public Guid? SelectedPlatformPlanPriceId { get; set; }
    public BillingFrequency? BillingCycle { get; set; }

    // Status & linking
    public OnboardingStatus Status { get; set; } = OnboardingStatus.Draft;
    public Guid? CreatedUserId { get; set; }
    public Guid? CreatedTrainerId { get; set; }

    public PlatformPlan? SelectedPlan { get; set; }
    public PlatformPlanPrice? SelectedPlanPrice { get; set; }
}
