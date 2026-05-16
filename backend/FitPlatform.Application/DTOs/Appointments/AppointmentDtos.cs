using System.ComponentModel.DataAnnotations;

namespace FitPlatform.Application.DTOs.Appointments;

public class AppointmentRequest
{
    public Guid? StudentId { get; set; }
    [Required, MaxLength(140)]
    public string Title { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string? Description { get; set; }
    public string Type { get; set; } = "Other";
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    [MaxLength(200)]
    public string? Location { get; set; }
    [MaxLength(500)]
    public string? OnlineMeetingUrl { get; set; }
}

public class AppointmentRescheduleRequest
{
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
}

public class AppointmentCancelRequest
{
    [MaxLength(300)]
    public string? Reason { get; set; }
}

public class AppointmentResponse
{
    public Guid Id { get; set; }
    public Guid TrainerId { get; set; }
    public Guid? StudentId { get; set; }
    public string? StudentName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string? Location { get; set; }
    public string? OnlineMeetingUrl { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? ConfirmationAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? RescheduledFromAppointmentId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AppointmentQuery
{
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
    public string? Status { get; set; }
    public string? Type { get; set; }
    public Guid? StudentId { get; set; }
}
