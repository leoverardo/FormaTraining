using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class TrainerPayment : BaseEntity
{
    public Guid TrainerId { get; set; }
    public Guid TrainerSubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string Provider { get; set; } = "MercadoPago";
    public string? ProviderPaymentId { get; set; }
    public string? ProviderSubscriptionId { get; set; }
    public string? RawPayload { get; set; }
    public DateTime? PaidAt { get; set; }

    public Trainer Trainer { get; set; } = null!;
    public TrainerSubscription Subscription { get; set; } = null!;
}
