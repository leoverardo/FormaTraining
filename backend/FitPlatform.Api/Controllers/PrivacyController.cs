using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Privacy;
using FitPlatform.Application.Interfaces;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Api.Controllers;

// ─── Terms / public ──────────────────────────────────────────────────────────
[ApiController]
[Route("api/public/terms")]
[AllowAnonymous]
public class TermsController : ControllerBase
{
    private readonly AppDbContext _db;

    public TermsController(AppDbContext db) => _db = db;

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var terms = await _db.TermsDocuments.Where(t => t.Active).OrderBy(t => t.Type).ToListAsync();
        var result = terms.Select(t => new TermsDocumentResponse
        {
            Id = t.Id, Type = t.Type.ToString(), Version = t.Version,
            Title = t.Title, Content = t.Content, Active = t.Active
        }).ToList();
        return Ok(ApiResponse<List<TermsDocumentResponse>>.Ok(result));
    }
}

// ─── Consent / authenticated ──────────────────────────────────────────────────
[ApiController]
[Route("api/consents")]
[Authorize]
public class ConsentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ConsentsController(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpPost("accept")]
    public async Task<IActionResult> Accept([FromBody] ConsentRequest request)
    {
        var termsDoc = await _db.TermsDocuments.FindAsync(request.TermsDocumentId);
        if (termsDoc == null || !termsDoc.Active)
            return BadRequest(ApiResponse.Fail("Documento de termos não encontrado ou inativo."));

        var existing = await _db.UserConsents.FirstOrDefaultAsync(
            c => c.UserId == _currentUser.UserId && c.TermsDocumentId == request.TermsDocumentId);

        if (existing != null)
            return Ok(ApiResponse.Ok("Consentimento já registrado."));

        _db.UserConsents.Add(new UserConsent
        {
            UserId = _currentUser.UserId,
            TermsDocumentId = request.TermsDocumentId,
            AcceptedAt = DateTime.UtcNow,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent
        });
        await _db.SaveChangesAsync();
        return Ok(ApiResponse.Ok("Consentimento registrado."));
    }

    [HttpGet("~/api/me/consents")]
    public async Task<IActionResult> GetMyConsents()
    {
        var consents = await _db.UserConsents
            .Include(c => c.TermsDocument)
            .Where(c => c.UserId == _currentUser.UserId)
            .ToListAsync();

        var result = consents.Select(c => new
        {
            c.Id, TermsType = c.TermsDocument?.Type.ToString(),
            TermsVersion = c.TermsDocument?.Version, c.AcceptedAt
        }).ToList();
        return Ok(ApiResponse<object>.Ok(result));
    }
}

// ─── Privacy / LGPD ───────────────────────────────────────────────────────────
[ApiController]
[Route("api/privacy")]
[Authorize]
public class PrivacyController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public PrivacyController(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet("my-data")]
    public async Task<IActionResult> GetMyData()
    {
        var user = await _db.Users.Include(u => u.Student).Include(u => u.Trainer).FirstOrDefaultAsync(u => u.Id == _currentUser.UserId);
        if (user == null) return NotFound();

        var data = new { user.Name, user.Email, user.Role, user.CreatedAt, user.LastLoginAt };
        return Ok(ApiResponse<object>.Ok(data));
    }

    [HttpPost("request-export")]
    public async Task<IActionResult> RequestExport()
    {
        _db.DataPrivacyRequests.Add(new DataPrivacyRequest
        {
            UserId = _currentUser.UserId,
            RequestType = DataPrivacyRequestType.ExportData,
            Status = DataPrivacyRequestStatus.Pending,
            RequestedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        return Ok(ApiResponse.Ok("Solicitação de exportação registrada. Entraremos em contato em breve."));
    }

    [HttpPost("request-delete")]
    public async Task<IActionResult> RequestDelete()
    {
        _db.DataPrivacyRequests.Add(new DataPrivacyRequest
        {
            UserId = _currentUser.UserId,
            RequestType = DataPrivacyRequestType.DeleteAccount,
            Status = DataPrivacyRequestStatus.Pending,
            RequestedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        return Ok(ApiResponse.Ok("Solicitação de exclusão registrada. Processaremos em até 15 dias úteis."));
    }
}

// ─── Trainer anonymize student ────────────────────────────────────────────────
[ApiController]
[Route("api/students")]
[Authorize(Roles = "Trainer")]
public class StudentAnonymizationController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public StudentAnonymizationController(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpPost("{id:guid}/anonymize")]
    public async Task<IActionResult> Anonymize(Guid id)
    {
        var student = await _db.Students.Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id && s.TrainerId == _currentUser.TrainerId!.Value);
        if (student == null) return NotFound(ApiResponse.Fail("Aluno não encontrado."));

        student.User.Name = "Aluno Anonimizado";
        student.User.Email = $"anon_{student.Id:N}@deleted.invalid";
        student.User.IsActive = false;
        student.User.UpdatedAt = DateTime.UtcNow;
        student.Phone = null;
        student.Notes = null;
        student.Status = StudentStatus.Inactive;
        student.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(ApiResponse.Ok("Dados do aluno anonimizados conforme LGPD."));
    }
}
