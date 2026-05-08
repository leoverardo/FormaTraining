using FitPlatform.Domain.Enums;

namespace FitPlatform.Application.DTOs.Onboarding;

public class TrainerOnboardingResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? CPF { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? BrandName { get; set; }
    public string? CREF { get; set; }
    public string? Bio { get; set; }
    public string? Specialties { get; set; }
    public string? Instagram { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? ZipCode { get; set; }
    public string? Street { get; set; }
    public string? AddressNumber { get; set; }
    public string? Complement { get; set; }
    public string? Neighborhood { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public Guid? SelectedPlatformPlanId { get; set; }
    public string? SelectedPlanName { get; set; }
    public Guid? SelectedPlatformPlanPriceId { get; set; }
    public string? BillingCycle { get; set; }
    public decimal? SelectedPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
