using FitPlatform.Domain.Common;

namespace FitPlatform.Domain.Entities;

public class DataProcessorVendor : BaseEntity
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
