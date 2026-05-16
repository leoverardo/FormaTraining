using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Privacy;
using FitPlatform.Application.Interfaces;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Api.Controllers;

[ApiController]
[Route("api/legal")]
public class LegalController : ControllerBase
{
    private readonly PrivacyLgpdService _service;

    public LegalController(PrivacyLgpdService service) => _service = service;

    [HttpGet("documents/active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActive()
    {
        var docs = await _service.GetActiveDocumentsAsync();
        return Ok(ApiResponse<object>.Ok(docs));
    }

    [HttpGet("privacy-policy")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPrivacyPolicy()
        => Ok(ApiResponse<object>.Ok((await _service.GetActiveDocumentsAsync()).FirstOrDefault(x => x.DocumentType == LegalDocumentType.PrivacyPolicy)));

    [HttpGet("terms-of-use")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTerms()
        => Ok(ApiResponse<object>.Ok((await _service.GetActiveDocumentsAsync()).FirstOrDefault(x => x.DocumentType == LegalDocumentType.TermsOfUse)));

    [HttpPost("acceptance")]
    [AllowAnonymous]
    public async Task<IActionResult> Accept([FromBody] LegalAcceptanceRequest request)
    {
        var result = await _service.RegisterAcceptanceAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString());
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

[ApiController]
[Route("api/privacy")]
[Authorize]
public class PrivacyLgpdController : ControllerBase
{
    private readonly PrivacyLgpdService _service;
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public PrivacyLgpdController(PrivacyLgpdService service, AppDbContext db, ICurrentUserService currentUser)
    {
        _service = service;
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet("consents")]
    public async Task<IActionResult> GetConsents() => Ok(ApiResponse<object>.Ok(await _service.GetMyConsentsAsync()));

    [HttpPut("consents/{code}")]
    public async Task<IActionResult> UpdateConsent(string code, [FromBody] UpdateConsentRequest request)
    {
        if (code == "HEALTH_RELATED_DATA_PROCESSING")
            return BadRequest(ApiResponse.Fail("Este item e informativo e depende de validacao juridica antes de alteracoes pela UI."));

        var result = await _service.UpdateConsentAsync(code, request.IsGranted, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString());

        if (result.Success && code == "PUBLIC_PROFILE_VISIBILITY" && _currentUser.TrainerId.HasValue)
        {
            var trainer = await _db.Trainers.FirstOrDefaultAsync(x => x.Id == _currentUser.TrainerId.Value);
            if (trainer != null)
            {
                if (!request.IsGranted)
                {
                    trainer.PublicPageEnabled = false;
                    trainer.PublicSearchEnabled = false;
                }

                trainer.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("data-export/request")]
    public async Task<IActionResult> RequestExport() => Ok(await _service.RequestExportAsync());

    [HttpGet("data-export/latest")]
    public async Task<IActionResult> LatestExport() => Ok(ApiResponse<object>.Ok(await _service.GenerateLatestExportAsync()));

    [HttpGet("data-export/download/{id:guid}")]
    public async Task<IActionResult> DownloadExport(Guid id)
    {
        var export = await _db.UserDataExports.FirstOrDefaultAsync(x => x.Id == id && x.UserId == _currentUser.UserId);
        if (export == null || string.IsNullOrWhiteSpace(export.PayloadJson)) return NotFound(ApiResponse.Fail("Exportacao nao encontrada."));
        return File(System.Text.Encoding.UTF8.GetBytes(export.PayloadJson), "application/json", $"forma-training-export-{id}.json");
    }

    [HttpPost("account-deletion/request")]
    public async Task<IActionResult> RequestDeletion([FromBody] DataSubjectRequestCreateDto dto)
    {
        var user = await _db.Users.FirstAsync(x => x.Id == _currentUser.UserId);
        _db.DataPrivacyRequests.Add(new DataPrivacyRequest
        {
            UserId = _currentUser.UserId,
            RequesterEmail = user.Email,
            RequestType = DataPrivacyRequestType.Deletion,
            Status = DataPrivacyRequestStatus.Pending,
            RequestedAt = DateTime.UtcNow,
            Description = dto.Description
        });
        await _db.SaveChangesAsync();
        return Ok(ApiResponse.Ok("Solicitacao de exclusao registrada."));
    }

    [HttpGet("requests/my")]
    public async Task<IActionResult> MyRequests()
        => Ok(ApiResponse<object>.Ok(await _db.DataPrivacyRequests.Where(x => x.UserId == _currentUser.UserId).OrderByDescending(x => x.RequestedAt).ToListAsync()));
}

[ApiController]
[Route("api/owner/privacy")]
[Authorize(Roles = "Owner")]
public class OwnerPrivacyController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public OwnerPrivacyController(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet("requests")]
    public async Task<IActionResult> Requests() => Ok(ApiResponse<object>.Ok(await _db.DataPrivacyRequests.OrderByDescending(x => x.RequestedAt).ToListAsync()));

    [HttpPut("requests/{id:guid}/status")]
    public async Task<IActionResult> UpdateRequest(Guid id, [FromBody] UpdateDataSubjectRequestStatusDto dto)
    {
        var req = await _db.DataPrivacyRequests.FirstOrDefaultAsync(x => x.Id == id);
        if (req == null) return NotFound(ApiResponse.Fail("Solicitacao nao encontrada."));
        req.Status = dto.Status;
        req.AdminNotes = dto.AdminNotes;
        req.RejectionReason = dto.RejectionReason;
        req.CompletedAt = dto.Status == DataPrivacyRequestStatus.Completed ? DateTime.UtcNow : req.CompletedAt;
        req.RejectedAt = dto.Status == DataPrivacyRequestStatus.Rejected ? DateTime.UtcNow : req.RejectedAt;
        req.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse.Ok("Status atualizado."));
    }

    [HttpGet("incidents")]
    public async Task<IActionResult> Incidents() => Ok(ApiResponse<object>.Ok(await _db.SecurityIncidents.OrderByDescending(x => x.DetectedAt).ToListAsync()));

    [HttpPost("incidents")]
    public async Task<IActionResult> CreateIncident([FromBody] SecurityIncidentUpsertDto dto)
    {
        var i = new SecurityIncident
        {
            Title = dto.Title,
            Description = dto.Description,
            Severity = dto.Severity,
            Status = dto.Status,
            DetectedAt = dto.DetectedAt,
            ConfirmedAt = dto.ConfirmedAt,
            ReportedToAuthorityAt = dto.ReportedToAuthorityAt,
            ReportedToUsersAt = dto.ReportedToUsersAt,
            ClosedAt = dto.ClosedAt,
            Notes = dto.Notes,
            CreatedByUserId = _currentUser.UserId
        };
        _db.SecurityIncidents.Add(i);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(i));
    }

    [HttpPut("incidents/{id:guid}")]
    public async Task<IActionResult> UpdateIncident(Guid id, [FromBody] SecurityIncidentUpsertDto dto)
    {
        var i = await _db.SecurityIncidents.FirstOrDefaultAsync(x => x.Id == id);
        if (i == null) return NotFound(ApiResponse.Fail("Incidente nao encontrado."));
        i.Title = dto.Title;
        i.Description = dto.Description;
        i.Severity = dto.Severity;
        i.Status = dto.Status;
        i.DetectedAt = dto.DetectedAt;
        i.ConfirmedAt = dto.ConfirmedAt;
        i.ReportedToAuthorityAt = dto.ReportedToAuthorityAt;
        i.ReportedToUsersAt = dto.ReportedToUsersAt;
        i.ClosedAt = dto.ClosedAt;
        i.Notes = dto.Notes;
        i.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(i));
    }

    [HttpGet("vendors")]
    public async Task<IActionResult> Vendors() => Ok(ApiResponse<object>.Ok(await _db.DataProcessorVendors.OrderBy(x => x.Name).ToListAsync()));

    [HttpPost("vendors")]
    public async Task<IActionResult> CreateVendor([FromBody] DataProcessorVendorUpsertDto dto)
    {
        var v = new DataProcessorVendor
        {
            Name = dto.Name,
            Purpose = dto.Purpose,
            DataCategories = dto.DataCategories,
            CountryOrRegion = dto.CountryOrRegion,
            HasInternationalTransfer = dto.HasInternationalTransfer,
            PrivacyPolicyReference = dto.PrivacyPolicyReference,
            ContractualBasisNotes = dto.ContractualBasisNotes,
            IsActive = dto.IsActive
        };
        _db.DataProcessorVendors.Add(v);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(v));
    }

    [HttpPut("vendors/{id:guid}")]
    public async Task<IActionResult> UpdateVendor(Guid id, [FromBody] DataProcessorVendorUpsertDto dto)
    {
        var v = await _db.DataProcessorVendors.FirstOrDefaultAsync(x => x.Id == id);
        if (v == null) return NotFound(ApiResponse.Fail("Vendor nao encontrado."));
        v.Name = dto.Name;
        v.Purpose = dto.Purpose;
        v.DataCategories = dto.DataCategories;
        v.CountryOrRegion = dto.CountryOrRegion;
        v.HasInternationalTransfer = dto.HasInternationalTransfer;
        v.PrivacyPolicyReference = dto.PrivacyPolicyReference;
        v.ContractualBasisNotes = dto.ContractualBasisNotes;
        v.IsActive = dto.IsActive;
        v.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(v));
    }
}
