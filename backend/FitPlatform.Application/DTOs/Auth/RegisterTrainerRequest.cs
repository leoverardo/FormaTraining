namespace FitPlatform.Application.DTOs.Auth;

public class RegisterTrainerRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string? Phone { get; set; }
}
