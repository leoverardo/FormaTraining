using FitPlatform.Domain.Enums;

namespace FitPlatform.Application.DTOs.Privacy;

public class LegalAcceptanceRequest
{
    public bool AcceptPrivacyPolicy { get; set; }
    public bool AcceptTermsOfUse { get; set; }
    public string? Email { get; set; }
    public string? Source { get; set; }
}

public class UpdateConsentRequest
{
    public bool IsGranted { get; set; }
}

public class DataSubjectRequestCreateDto
{
    public string? Description { get; set; }
}

public class UpdateDataSubjectRequestStatusDto
{
    public DataPrivacyRequestStatus Status { get; set; }
    public string? AdminNotes { get; set; }
    public string? RejectionReason { get; set; }
}

public class SecurityIncidentUpsertDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SecurityIncidentSeverity Severity { get; set; } = SecurityIncidentSeverity.Medium;
    public SecurityIncidentStatus Status { get; set; } = SecurityIncidentStatus.Identified;
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? ReportedToAuthorityAt { get; set; }
    public DateTime? ReportedToUsersAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? Notes { get; set; }
}

public class DataProcessorVendorUpsertDto
{
    public string Name { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string DataCategories { get; set; } = string.Empty;
    public string CountryOrRegion { get; set; } = string.Empty;
    public bool HasInternationalTransfer { get; set; }
    public string? PrivacyPolicyReference { get; set; }
    public string? ContractualBasisNotes { get; set; }
    public bool IsActive { get; set; } = true;
}
