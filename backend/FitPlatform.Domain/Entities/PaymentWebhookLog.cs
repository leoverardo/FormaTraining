using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

/// <summary>
/// Registro de auditoria e de idempotência de um evento de webhook.
///
/// Duas camadas independentes de idempotência convivem no sistema:
///   • <see cref="EventId"/> — idempotência do EVENTO (este registro).
///   • <c>TrainerPayment.ProviderPaymentId</c> — idempotência da OPERAÇÃO FINANCEIRA,
///     aplicada em <c>SubscriptionLifecycleService</c>.
/// Ambas são necessárias: eventos distintos podem referenciar o mesmo pagamento.
/// </summary>
public class PaymentWebhookLog : BaseEntity
{
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Identificador único do evento no provedor — o campo <c>id</c> do envelope da
    /// AbacatePay (ex.: "log_abc123"). É o "ProviderEventId" do sistema.
    ///
    /// Protegido pelo índice UNIQUE <c>(Provider, EventId)</c>, que é a última barreira
    /// de idempotência contra entregas concorrentes do mesmo evento.
    /// </summary>
    public string EventId { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    /// <summary>Id do recurso citado pelo evento (subscription.id, checkout.id ou id).</summary>
    public string? ResourceId { get; set; }

    public string RawPayload { get; set; } = string.Empty;
    public DateTime? ProcessedAt { get; set; }

    /// <summary>Status do ciclo de processamento do evento.</summary>
    public WebhookProcessingStatus ProcessingStatus { get; set; } = WebhookProcessingStatus.Pending;

    /// <summary>Mensagem de erro se ProcessingStatus == Failed.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Contador de tentativas de processamento além da primeira (AttemptCount - 1).
    /// Comparado com <c>AbacatePay:Reconciliation:WebhookMaxRetries</c> para decidir
    /// entre nova tentativa e <see cref="WebhookProcessingStatus.Abandoned"/>.
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>Momento da última tentativa de processamento (bem-sucedida ou não).</summary>
    public DateTime? LastAttemptAt { get; set; }

    /// <summary>
    /// Momento a partir do qual o evento pode ser reprocessado pela rotina de retry.
    /// Preenchido com backoff crescente a cada falha.
    /// </summary>
    public DateTime? NextRetryAt { get; set; }
}
