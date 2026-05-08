namespace FitPlatform.Application.DTOs.Trainer;

public class TrainerProfileResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? CPF { get; set; }
    public DateTime? BirthDate { get; set; }
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
}
