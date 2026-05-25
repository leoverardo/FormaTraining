using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class PaymentWebhookLog : BaseEntity
{
    public string Provider { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? ResourceId { get; set; }
    public string RawPayload { get; set; } = string.Empty;
    public DateTime? ProcessedAt { get; set; }

    /// <summary>Status do ciclo de processamento do evento.</summary>
    public WebhookProcessingStatus ProcessingStatus { get; set; } = WebhookProcessingStatus.Pending;

    /// <summary>Mensagem de erro se ProcessingStatus == Failed.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Contador de tentativas de reprocessamento.</summary>
    public int RetryCount { get; set; } = 0;
}
