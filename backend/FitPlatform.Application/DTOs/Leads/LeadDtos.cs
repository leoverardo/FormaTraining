using FitPlatform.Domain.Enums;

namespace FitPlatform.Application.DTOs.Leads;

public class CreateTrainerLeadRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Goal { get; set; }
    public string? Message { get; set; }
    public TrainerLeadSource Source { get; set; } = TrainerLeadSource.PublicPage;
}

public class UpdateLeadStatusRequest
{
    public TrainerLeadStatus Status { get; set; }
}

public class TrainerLeadResponse
{
    public Guid Id { get; set; }
    public Guid TrainerId { get; set; }
    public Guid? StudentProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Goal { get; set; }
    public string? Message { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
