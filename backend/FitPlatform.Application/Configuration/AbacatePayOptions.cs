namespace FitPlatform.Application.Configuration;

public class AbacatePayOptions
{
    public const string SectionName = "AbacatePay";

    public string BaseUrl { get; set; } = "https://api.abacatepay.com/v2";
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Secret recebido na query string do webhook (?webhookSecret=...).
    /// Configure também como variável de ambiente ABACATEPAY_WEBHOOK_SECRET.
    /// </summary>
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>
    /// Chave pública usada para validar a assinatura HMAC-SHA256 do header X-Webhook-Signature.
    /// Configure também como variável de ambiente ABACATEPAY_WEBHOOK_PUBLIC_KEY.
    /// </summary>
    public string WebhookPublicKey { get; set; } = string.Empty;

    public string SuccessUrl { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public bool DevMode { get; set; }

    /// <summary>
    /// Habilita a validação da assinatura HMAC-SHA256 (header X-Webhook-Signature).
    /// DEVE ser true em produção. Em Development, se false, permite receber webhooks sem assinar
    /// (útil para testes locais com ferramentas como curl ou CLI da AbacatePay).
    /// Nunca desabilitar em produção — o sistema rejeita automaticamente webhooks não assinados
    /// fora do ambiente Development, independentemente desse flag.
    /// </summary>
    public bool EnableSignatureValidation { get; set; } = true;
}
