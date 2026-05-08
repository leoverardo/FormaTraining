using FitPlatform.Domain.Enums;

namespace FitPlatform.Application.DTOs.Onboarding;

public class StartOnboardingRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? CPF { get; set; }
    public DateTime? BirthDate { get; set; }
}

public class UpdateProfessionalDataRequest
{
    public string BrandName { get; set; } = string.Empty;
    public string? CREF { get; set; }
    public string? Bio { get; set; }
    public string? Specialties { get; set; }
    public string? Instagram { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
}

public class UpdateAddressRequest
{
    public string? ZipCode { get; set; }
    public string? Street { get; set; }
    public string? AddressNumber { get; set; }
    public string? Complement { get; set; }
    public string? Neighborhood { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
}

public class SelectPlanRequest
{
    public Guid PlatformPlanId { get; set; }
    public Guid PlatformPlanPriceId { get; set; }
    public BillingFrequency BillingCycle { get; set; }
}
