using FitPlatform.Domain.Common;

namespace FitPlatform.Domain.Entities;

public class PaymentWebhookLog : BaseEntity
{
    public string Provider { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? ResourceId { get; set; }
    public string RawPayload { get; set; } = string.Empty;
    public DateTime? ProcessedAt { get; set; }
}
