using FitPlatform.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace FitPlatform.Infrastructure.ExternalServices;

// Fake email service that logs to console. Replace with SMTP/SendGrid/Resend in production.
public class ConsoleEmailService : IEmailService
{
    private readonly ILogger<ConsoleEmailService> _logger;

    public ConsoleEmailService(ILogger<ConsoleEmailService> logger) => _logger = logger;

    public Task SendPasswordSetupAsync(string toEmail, string toName, string setupLink, string planName)
    {
        _logger.LogInformation(
            """
            ═══════════════════════════════════════════════════════════
            [E-MAIL FAKE] Para: {Email}
            Assunto: Defina sua senha e acesse seu painel — FitPlatform
            ───────────────────────────────────────────────────────────
            Olá, {Name}!

            Seu pagamento foi aprovado. Bem-vindo ao FitPlatform!
            Plano contratado: {Plan}

            Clique no link abaixo para definir sua senha:
            {Link}

            O link expira em 24 horas.
            ═══════════════════════════════════════════════════════════
            """,
            toEmail, toName, planName, setupLink);
        return Task.CompletedTask;
    }

    public Task SendStudentWelcomeAsync(string toEmail, string studentName, string trainerBrand, string setupLink)
    {
        _logger.LogInformation(
            """
            ═══════════════════════════════════════════════════════════
            [E-MAIL FAKE] Para: {Email}
            Assunto: Seu acesso aos treinos foi criado — {Brand}
            ───────────────────────────────────────────────────────────
            Olá, {Name}!

            {Brand} criou um acesso para você na FitPlatform.
            Clique no link abaixo para definir sua senha:
            {Link}

            O link expira em 7 dias.
            ═══════════════════════════════════════════════════════════
            """,
            toEmail, trainerBrand, studentName, trainerBrand, setupLink);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string toEmail, string toName, string resetLink)
    {
        _logger.LogInformation(
            """
            ═══════════════════════════════════════════════════════════
            [E-MAIL FAKE] Para: {Email}
            Assunto: Redefinição de senha — FitPlatform
            ───────────────────────────────────────────────────────────
            Olá, {Name}!

            Clique no link abaixo para redefinir sua senha:
            {Link}

            O link expira em 1 hora.
            ═══════════════════════════════════════════════════════════
            """,
            toEmail, toName, resetLink);
        return Task.CompletedTask;
    }
}
