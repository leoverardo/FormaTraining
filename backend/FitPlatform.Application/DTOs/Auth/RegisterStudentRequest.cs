namespace FitPlatform.Application.DTOs.Auth;

public class RegisterStudentRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Goal { get; set; }
    public string? Interests { get; set; }
    public string? TrainingLevel { get; set; }
    public string? PreferredTrainingMode { get; set; }
    public bool AcceptPrivacyPolicy { get; set; }
    public bool AcceptTermsOfUse { get; set; }
    public bool MarketingEmail { get; set; }
    public bool MarketingWhatsapp { get; set; }
    public bool HealthRelatedDataProcessingAcknowledged { get; set; }
}
