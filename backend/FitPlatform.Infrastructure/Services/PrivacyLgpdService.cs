using System.Text.Json;
using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Privacy;
using FitPlatform.Application.Interfaces;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FitPlatform.Infrastructure.Services;

public class PrivacyLgpdService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<PrivacyLgpdService> _logger;

    public PrivacyLgpdService(AppDbContext db, ICurrentUserService currentUser, ILogger<PrivacyLgpdService> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<List<PrivacyPolicyVersion>> GetActiveDocumentsAsync()
        => await _db.PrivacyPolicyVersions.Where(x => x.IsActive).OrderBy(x => x.DocumentType).ToListAsync();

    public async Task<ApiResponse<object>> RegisterAcceptanceAsync(LegalAcceptanceRequest request, string? ipAddress, string? userAgent, Guid? userId = null)
    {
        if (!request.AcceptPrivacyPolicy || !request.AcceptTermsOfUse)
            return ApiResponse<object>.Fail("Privacy Policy and Terms acceptance is required.");

        var privacy = await _db.PrivacyPolicyVersions.FirstOrDefaultAsync(x => x.DocumentType == LegalDocumentType.PrivacyPolicy && x.IsActive);
        var terms = await _db.PrivacyPolicyVersions.FirstOrDefaultAsync(x => x.DocumentType == LegalDocumentType.TermsOfUse && x.IsActive);
        if (privacy == null || terms == null)
            return ApiResponse<object>.Fail("Active legal documents not found.");

        var source = Enum.TryParse<LegalAcceptanceSource>(request.Source ?? "Registration", true, out var s) ? s : LegalAcceptanceSource.Registration;

        _db.UserLegalAcceptances.Add(new UserLegalAcceptance
        {
            UserId = userId,
            Email = request.Email,
            PrivacyPolicyVersionId = privacy.Id,
            TermsOfUseVersionId = terms.Id,
            AcceptedAt = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Source = source
        });
        await _db.SaveChangesAsync();

        _logger.LogInformation("Legal acceptance registered. UserId={UserId} Email={Email}", userId, request.Email);
        return ApiResponse<object>.Ok(new { PrivacyPolicyVersion = privacy.Version, TermsOfUseVersion = terms.Version });
    }

    public async Task<List<object>> GetMyConsentsAsync()
    {
        var defs = await _db.ConsentDefinitions.Where(x => x.IsActive).OrderBy(x => x.Code).ToListAsync();
        var statuses = await _db.UserPrivacyConsents.Where(x => x.UserId == _currentUser.UserId).ToDictionaryAsync(x => x.ConsentDefinitionId);
        return defs.Select(d =>
        {
            statuses.TryGetValue(d.Id, out var st);
            return (object)new { d.Code, d.Name, d.Description, d.IsRequired, d.Category, IsGranted = st?.IsGranted ?? false, st?.GrantedAt, st?.RevokedAt, st?.LastChangedAt };
        }).ToList();
    }

    public async Task<ApiResponse<object>> UpdateConsentAsync(string code, bool isGranted, string? ipAddress, string? userAgent, Guid? targetUserId = null)
    {
        var userId = targetUserId ?? _currentUser.UserId;
        var definition = await _db.ConsentDefinitions.FirstOrDefaultAsync(x => x.Code == code && x.IsActive);
        if (definition == null) return ApiResponse<object>.Fail("Consent not found.");

        var status = await _db.UserPrivacyConsents.FirstOrDefaultAsync(x => x.UserId == userId && x.ConsentDefinitionId == definition.Id);
        if (status == null)
        {
            status = new UserPrivacyConsent { UserId = userId, ConsentDefinitionId = definition.Id };
            _db.UserPrivacyConsents.Add(status);
        }

        status.IsGranted = isGranted;
        status.LastChangedAt = DateTime.UtcNow;
        status.IpAddress = ipAddress;
        status.UserAgent = userAgent;
        if (isGranted) { status.GrantedAt = DateTime.UtcNow; status.RevokedAt = null; }
        else { status.RevokedAt = DateTime.UtcNow; }

        _db.UserConsentHistories.Add(new UserConsentHistory
        {
            UserId = userId,
            ConsentDefinitionId = definition.Id,
            Action = isGranted ? ConsentChangeAction.Granted : ConsentChangeAction.Revoked,
            ChangedAt = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent
        });

        await _db.SaveChangesAsync();
        _logger.LogInformation("Consent changed. UserId={UserId} Code={Code} IsGranted={IsGranted}", userId, code, isGranted);
        return ApiResponse<object>.Ok(new { code, isGranted });
    }

    public async Task<ApiResponse<UserDataExport>> RequestExportAsync()
    {
        var export = new UserDataExport { UserId = _currentUser.UserId, RequestedAt = DateTime.UtcNow, Status = "Pending" };
        _db.UserDataExports.Add(export);
        await _db.SaveChangesAsync();
        return ApiResponse<UserDataExport>.Ok(export);
    }

    public async Task<UserDataExport?> GenerateLatestExportAsync()
    {
        var export = await _db.UserDataExports.Where(x => x.UserId == _currentUser.UserId).OrderByDescending(x => x.RequestedAt).FirstOrDefaultAsync();
        if (export == null) return null;
        if (export.Status == "Generated" && !string.IsNullOrWhiteSpace(export.PayloadJson)) return export;

        var user = await _db.Users.Include(x => x.Trainer).Include(x => x.Student).Include(x => x.StudentProfile).FirstAsync(x => x.Id == _currentUser.UserId);
        var payload = new
        {
            generatedAt = DateTime.UtcNow,
            applicationName = "Forma Training",
            exportScope = "user-personal-data",
            user = new { user.Id, user.Name, user.Email, Role = user.Role.ToString(), user.CreatedAt },
            trainer = user.Trainer,
            student = user.Student,
            studentProfile = user.StudentProfile,
            habits = await _db.StudentHabits.Where(x => x.Student.UserId == _currentUser.UserId).ToListAsync(),
            habitLogs = await _db.StudentHabitLogs.Where(x => x.Student.UserId == _currentUser.UserId).ToListAsync(),
            nutrition = await _db.StudentNutritionGuidances.Where(x => x.Student.UserId == _currentUser.UserId).ToListAsync(),
            progress = await _db.StudentProgressRecords.Where(x => x.Student.UserId == _currentUser.UserId).ToListAsync(),
            checkIns = await _db.StudentWeeklyCheckIns.Where(x => x.Student.UserId == _currentUser.UserId).ToListAsync(),
            workouts = await _db.WorkoutSessions.Where(x => x.Student.UserId == _currentUser.UserId).ToListAsync(),
            appointments = await _db.Appointments.Where(x => x.Student != null && x.Student.UserId == _currentUser.UserId).ToListAsync(),
            conversations = await _db.Conversations.Where(x => x.Student.UserId == _currentUser.UserId).Select(x => new { x.Id, x.CreatedAt, x.UpdatedAt }).ToListAsync(),
            messages = await _db.ChatMessages.Where(x => x.SenderUserId == _currentUser.UserId).ToListAsync(),
            leads = await _db.TrainerLeads.Where(x => x.StudentProfile != null && x.StudentProfile.UserId == _currentUser.UserId).ToListAsync(),
            consents = await _db.UserPrivacyConsents.Include(x => x.ConsentDefinition).Where(x => x.UserId == _currentUser.UserId).ToListAsync(),
            legalAcceptances = await _db.UserLegalAcceptances.Where(x => x.UserId == _currentUser.UserId).ToListAsync(),
            subscriptions = await _db.TrainerSubscriptions.Where(x => x.Trainer.UserId == _currentUser.UserId).Select(x => new { x.Id, x.Status, x.StartDate, x.EndDate }).ToListAsync()
        };

        export.PayloadJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
        export.Status = "Generated";
        export.GeneratedAt = DateTime.UtcNow;
        export.ExpiresAt = DateTime.UtcNow.AddDays(7);
        await _db.SaveChangesAsync();
        return export;
    }
}

