using System.Security.Cryptography;
using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Auth;
using FitPlatform.Application.Interfaces;
using FitPlatform.Domain.Entities;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FitPlatform.Infrastructure.Services;

public class PasswordSetupService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    private readonly IConfiguration _config;

    public PasswordSetupService(AppDbContext db, IEmailService email, IConfiguration config)
    {
        _db = db;
        _email = email;
        _config = config;
    }

    public async Task<string> GenerateAndSendSetupTokenAsync(User user, string? planName = null, bool isStudent = false, string? trainerBrand = null)
    {
        // Invalidate existing tokens for this user
        var existing = await _db.PasswordSetupTokens
            .Where(t => t.UserId == user.Id && t.UsedAt == null && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();
        _db.PasswordSetupTokens.RemoveRange(existing);

        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        var tokenHash = BCrypt.Net.BCrypt.HashPassword(rawToken);

        var expiry = isStudent ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddHours(24);

        _db.PasswordSetupTokens.Add(new PasswordSetupToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = expiry
        });
        await _db.SaveChangesAsync();

        var frontendUrl = _config["FrontendUrl"] ?? "http://localhost:5173";
        var link = $"{frontendUrl}/set-password?token={Uri.EscapeDataString(rawToken)}&email={Uri.EscapeDataString(user.Email)}";

        if (isStudent)
            await _email.SendStudentWelcomeAsync(user.Email, user.Name, trainerBrand ?? "seu personal trainer", link);
        else
            await _email.SendPasswordSetupAsync(user.Email, user.Name, link, planName ?? "FitPlatform");

        return rawToken;
    }

    public async Task<ApiResponse> SetPasswordAsync(SetPasswordRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Token || true);

        // Find valid token by comparing hash
        var tokens = await _db.PasswordSetupTokens
            .Include(t => t.User)
            .Where(t => t.UsedAt == null && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        PasswordSetupToken? matchedToken = null;
        foreach (var t in tokens)
        {
            if (BCrypt.Net.BCrypt.Verify(request.Token, t.TokenHash))
            {
                matchedToken = t;
                break;
            }
        }

        if (matchedToken == null)
            return ApiResponse.Fail("Token inválido ou expirado.");

        matchedToken.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        matchedToken.User.MustChangePassword = false;
        matchedToken.User.IsActive = true;
        matchedToken.User.UpdatedAt = DateTime.UtcNow;
        matchedToken.UsedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ApiResponse.Ok("Senha definida com sucesso.");
    }

    public async Task<ApiResponse> RequestPasswordResetAsync(RequestPasswordResetRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());
        if (user == null)
            return ApiResponse.Ok("Se o e-mail estiver cadastrado, você receberá um link.");

        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        var tokenHash = BCrypt.Net.BCrypt.HashPassword(rawToken);

        _db.PasswordSetupTokens.Add(new PasswordSetupToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        });
        await _db.SaveChangesAsync();

        var frontendUrl = _config["FrontendUrl"] ?? "http://localhost:5173";
        var link = $"{frontendUrl}/set-password?token={Uri.EscapeDataString(rawToken)}&email={Uri.EscapeDataString(user.Email)}";
        await _email.SendPasswordResetAsync(user.Email, user.Name, link);

        return ApiResponse.Ok("Se o e-mail estiver cadastrado, você receberá um link.");
    }
}
