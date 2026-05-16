using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Auth;
using FitPlatform.Application.Interfaces;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtService _jwt;
    private readonly PrivacyLgpdService _privacyLgpdService;

    public AuthService(AppDbContext db, IJwtService jwt, PrivacyLgpdService privacyLgpdService)
    {
        _db = db;
        _jwt = jwt;
        _privacyLgpdService = privacyLgpdService;
    }

    public async Task<ApiResponse<AuthResponse>> RegisterTrainerAsync(RegisterTrainerRequest request)
    {
        if (!request.AcceptPrivacyPolicy || !request.AcceptTermsOfUse)
            return ApiResponse<AuthResponse>.Fail("VocÃª precisa aceitar os Termos de Uso e a PolÃ­tica de Privacidade.");

        if (await _db.Users.AnyAsync(u => u.Email == request.Email.ToLower()))
            return ApiResponse<AuthResponse>.Fail("E-mail jÃ¡ estÃ¡ em uso.");

        var user = new User
        {
            Name = request.Name,
            Email = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Trainer,
            IsActive = true
        };
        _db.Users.Add(user);

        var trainer = new Trainer
        {
            UserId = user.Id,
            BrandName = request.BrandName,
            Phone = request.Phone
        };
        _db.Trainers.Add(trainer);

        await _db.SaveChangesAsync();
        await _privacyLgpdService.RegisterAcceptanceAsync(new()
        {
            AcceptPrivacyPolicy = true,
            AcceptTermsOfUse = true,
            Email = user.Email,
            Source = "Registration"
        }, null, null, user.Id);

        var token = _jwt.GenerateToken(user, trainer.Id, null, null);
        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            Token = token,
            User = MapUserDto(user, trainer.Id, null, null, null)
        });
    }

    public async Task<ApiResponse<AuthResponse>> RegisterStudentAsync(RegisterStudentRequest request)
    {
        if (!request.AcceptPrivacyPolicy || !request.AcceptTermsOfUse)
            return ApiResponse<AuthResponse>.Fail("VocÃª precisa aceitar os Termos de Uso e a PolÃ­tica de Privacidade.");
        var email = request.Email.Trim().ToLower();
        if (await _db.Users.AnyAsync(u => u.Email == email))
            return ApiResponse<AuthResponse>.Fail("E-mail jÃ¡ estÃ¡ em uso.");

        var user = new User
        {
            Name = request.FullName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Student,
            IsActive = true
        };
        _db.Users.Add(user);

        var profile = new StudentProfile
        {
            UserId = user.Id,
            FullName = request.FullName.Trim(),
            Phone = request.Phone,
            City = request.City,
            State = request.State,
            Goal = request.Goal,
            Interests = request.Interests,
            TrainingLevel = request.TrainingLevel,
            PreferredTrainingMode = request.PreferredTrainingMode,
            AccountStatus = StudentAccountStatus.Explorer
        };
        _db.StudentProfiles.Add(profile);
        await _db.SaveChangesAsync();
        await _privacyLgpdService.RegisterAcceptanceAsync(new()
        {
            AcceptPrivacyPolicy = true,
            AcceptTermsOfUse = true,
            Email = user.Email,
            Source = "Registration"
        }, null, null, user.Id);
        await _privacyLgpdService.UpdateConsentAsync("MARKETING_EMAIL", request.MarketingEmail, null, null, user.Id);
        await _privacyLgpdService.UpdateConsentAsync("MARKETING_WHATSAPP", request.MarketingWhatsapp, null, null, user.Id);
        await _privacyLgpdService.UpdateConsentAsync("HEALTH_RELATED_DATA_PROCESSING", request.HealthRelatedDataProcessingAcknowledged, null, null, user.Id);

        var token = _jwt.GenerateToken(user, null, null, profile.Id);
        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            Token = token,
            User = MapUserDto(user, null, null, profile.Id, null)
        });
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return ApiResponse<AuthResponse>.Fail("E-mail ou senha incorretos.");

        var normalizedEmail = request.Email.Trim().ToLower();
        var user = await FindUserByEmailSafeAsync(normalizedEmail);

        if (user == null)
            return ApiResponse<AuthResponse>.Fail("E-mail ou senha incorretos.");

        var isPasswordValid = false;
        try
        {
            isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        }
        catch
        {
            isPasswordValid = false;
        }

        if (!isPasswordValid)
            return ApiResponse<AuthResponse>.Fail("E-mail ou senha incorretos.");

        if (!user.IsActive)
            return ApiResponse<AuthResponse>.Fail("Conta inativa. Entre em contato com o suporte.");

        var trainerId = user.Trainer?.Id;
        var studentId = user.Student?.Id;
        var studentProfileId = user.StudentProfile?.Id;
        Guid? activeTrainerId = user.Student?.Status == StudentStatus.Active ? user.Student.TrainerId : null;

        var token = _jwt.GenerateToken(user, trainerId, studentId, studentProfileId);
        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            Token = token,
            User = MapUserDto(user, trainerId, studentId, studentProfileId, activeTrainerId)
        });
    }

    public async Task<ApiResponse<UserDto>> MeAsync(Guid userId)
    {
        var user = await FindUserByIdSafeAsync(userId);

        if (user == null)
            return ApiResponse<UserDto>.Fail("UsuÃ¡rio nÃ£o encontrado.");

        Guid? activeTrainerId = user.Student?.Status == StudentStatus.Active ? user.Student.TrainerId : null;
        return ApiResponse<UserDto>.Ok(MapUserDto(user, user.Trainer?.Id, user.Student?.Id, user.StudentProfile?.Id, activeTrainerId));
    }

    private static UserDto MapUserDto(User user, Guid? trainerId, Guid? studentId, Guid? studentProfileId, Guid? activeTrainerId) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        Role = user.Role.ToString(),
        TrainerId = trainerId,
        StudentId = studentId,
        StudentProfileId = studentProfileId,
        ActiveTrainerId = activeTrainerId,
        HasActiveTrainerLink = activeTrainerId.HasValue && studentId.HasValue,
        IsExplorer = user.Role == UserRole.Student && !activeTrainerId.HasValue,
        MustChangePassword = user.MustChangePassword
    };

    private async Task<User?> FindUserByEmailSafeAsync(string normalizedEmail)
    {
        try
        {
            return await _db.Users
                .Include(u => u.Trainer)
                .Include(u => u.Student)
                .Include(u => u.StudentProfile)
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail);
        }
        catch (SqlException ex) when (ex.Number == 208 && ex.Message.Contains("StudentProfiles"))
        {
            return await _db.Users
                .Include(u => u.Trainer)
                .Include(u => u.Student)
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail);
        }
    }

    private async Task<User?> FindUserByIdSafeAsync(Guid userId)
    {
        try
        {
            return await _db.Users
                .Include(u => u.Trainer)
                .Include(u => u.Student)
                .Include(u => u.StudentProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
        catch (SqlException ex) when (ex.Number == 208 && ex.Message.Contains("StudentProfiles"))
        {
            return await _db.Users
                .Include(u => u.Trainer)
                .Include(u => u.Student)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}
