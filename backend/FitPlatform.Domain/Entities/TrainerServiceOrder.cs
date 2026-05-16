using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class TrainerServiceOrder : BaseEntity
{
    public Guid TrainerId { get; set; }
    public Guid ServiceOfferId { get; set; }
    public Guid? StudentId { get; set; }
    public Guid? LeadId { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;
    public string? BuyerPhone { get; set; }
    public decimal Amount { get; set; }
    public TrainerServiceOrderStatus Status { get; set; } = TrainerServiceOrderStatus.PendingPayment;
    public string PaymentProvider { get; set; } = "MercadoPago";
    public string? ProviderPaymentId { get; set; }
    public string? ProviderPreferenceId { get; set; }
    public string? ProviderPaymentStatus { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string ServiceTitleSnapshot { get; set; } = string.Empty;
    public string? ServiceDescriptionSnapshot { get; set; }
    public TrainerServiceBillingType BillingTypeSnapshot { get; set; } = TrainerServiceBillingType.OneTime;
    public int? DurationDaysSnapshot { get; set; }
    public bool RequiresManualStudentLinking { get; set; }
    public string? InternalNotes { get; set; }

    public Trainer Trainer { get; set; } = null!;
    public TrainerServiceOffer ServiceOffer { get; set; } = null!;
    public Student? Student { get; set; }
}
