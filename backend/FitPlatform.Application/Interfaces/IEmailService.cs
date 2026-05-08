namespace FitPlatform.Application.Interfaces;

public interface IEmailService
{
    Task SendPasswordSetupAsync(string toEmail, string toName, string setupLink, string planName);
    Task SendStudentWelcomeAsync(string toEmail, string studentName, string trainerBrand, string setupLink);
    Task SendPasswordResetAsync(string toEmail, string toName, string resetLink);
}
