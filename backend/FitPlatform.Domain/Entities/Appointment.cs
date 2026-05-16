using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class Appointment : BaseEntity
{
    public Guid TrainerId { get; set; }
    public Guid? StudentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AppointmentType Type { get; set; } = AppointmentType.Other;
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string? Location { get; set; }
    public string? OnlineMeetingUrl { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? ConfirmationAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? RescheduledFromAppointmentId { get; set; }

    public Trainer Trainer { get; set; } = null!;
    public Student? Student { get; set; }
    public Appointment? RescheduledFromAppointment { get; set; }
}
