using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class TrainerLead : BaseEntity
{
    public Guid TrainerId { get; set; }
    public Guid? StudentProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Goal { get; set; }
    public string? Message { get; set; }
    public TrainerLeadStatus Status { get; set; } = TrainerLeadStatus.New;
    public TrainerLeadSource Source { get; set; } = TrainerLeadSource.PublicPage;

    public Trainer Trainer { get; set; } = null!;
    public StudentProfile? StudentProfile { get; set; }
}
