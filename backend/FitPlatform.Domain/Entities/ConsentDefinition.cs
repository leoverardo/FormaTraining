using FitPlatform.Domain.Common;

namespace FitPlatform.Domain.Entities;

public class ConsentDefinition : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsActive { get; set; } = true;
    public string Category { get; set; } = string.Empty;
}
