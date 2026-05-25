namespace FitPlatform.Application.Interfaces;

/// <summary>
/// Valida a autenticidade de requisições de webhook recebidas da AbacatePay.
/// Implementação deve usar comparação de tempo constante para evitar timing attacks.
/// </summary>
public interface IAbacatePaySignatureValidator
{
    /// <summary>
    /// Indica se a validação de assinatura HMAC está habilitada nas configurações
    /// (AbacatePay:EnableSignatureValidation).
    /// </summary>
    bool IsSignatureValidationEnabled { get; }

    /// <summary>
    /// Valida o secret recebido na query string (?webhookSecret=...) comparando com
    /// AbacatePay:WebhookSecret usando CryptographicOperations.FixedTimeEquals.
    /// Retorna false se o secret configurado ou o fornecido estiver vazio.
    /// </summary>
    bool ValidateSecret(string? querySecret);

    /// <summary>
    /// Valida a assinatura HMAC-SHA256 do header X-Webhook-Signature.
    /// Computa HMACSHA256(AbacatePay:WebhookPublicKey, rawBody), converte para Base64
    /// e compara com o header usando CryptographicOperations.FixedTimeEquals.
    /// </summary>
    bool ValidateSignature(string rawBody, string? signatureHeader);
}
