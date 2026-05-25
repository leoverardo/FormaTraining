namespace FitPlatform.Application.Interfaces;

/// <summary>
/// Processa webhooks do AbacatePay de forma segura, idempotente e com rastreamento de status.
/// </summary>
public interface IAbacatePayWebhookService
{
    /// <summary>
    /// Valida, desserializa e processa um evento de webhook do AbacatePay.
    /// </summary>
    /// <param name="rawBody">Body raw da requisição (necessário para validação HMAC).</param>
    /// <param name="signatureHeader">Valor do header X-Webhook-Signature.</param>
    /// <param name="querySecret">Valor do query param webhookSecret.</param>
    /// <param name="isDevelopmentEnvironment">True se o ambiente for Development.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    Task<WebhookHandlerResult> HandleAsync(
        string rawBody,
        string? signatureHeader,
        string? querySecret,
        bool isDevelopmentEnvironment,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Resultado do processamento de um webhook.
/// </summary>
public sealed class WebhookHandlerResult
{
    /// <summary>True se a requisição foi rejeitada por falha de autenticação/assinatura.</summary>
    public bool IsUnauthorized { get; private init; }

    /// <summary>Mensagem descritiva do resultado.</summary>
    public string Message { get; private init; } = string.Empty;

    /// <summary>Cria um resultado de rejeição (HTTP 401).</summary>
    public static WebhookHandlerResult Unauthorized(string reason) =>
        new() { IsUnauthorized = true, Message = reason };

    /// <summary>Cria um resultado de aceitação (HTTP 200).</summary>
    public static WebhookHandlerResult Accepted(string message) =>
        new() { IsUnauthorized = false, Message = message };
}
