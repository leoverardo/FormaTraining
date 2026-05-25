using System.Security.Cryptography;
using System.Text;
using FitPlatform.Application.Configuration;
using FitPlatform.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace FitPlatform.Infrastructure.Services;

/// <summary>
/// Valida autenticidade de webhooks AbacatePay via secret (query string)
/// e assinatura HMAC-SHA256 (header X-Webhook-Signature).
/// Todas as comparações usam CryptographicOperations.FixedTimeEquals para evitar timing attacks.
/// </summary>
public sealed class AbacatePaySignatureValidator : IAbacatePaySignatureValidator
{
    private readonly AbacatePayOptions _options;

    public AbacatePaySignatureValidator(IOptions<AbacatePayOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
    public bool IsSignatureValidationEnabled => _options.EnableSignatureValidation;

    /// <inheritdoc />
    public bool ValidateSecret(string? querySecret)
    {
        var configured = _options.WebhookSecret;

        // Secret vazio na config é tratado como não configurado — rejeitar sempre.
        if (string.IsNullOrWhiteSpace(configured))
            return false;

        if (string.IsNullOrWhiteSpace(querySecret))
            return false;

        var expectedBytes = Encoding.UTF8.GetBytes(configured);
        var providedBytes = Encoding.UTF8.GetBytes(querySecret);

        // Comprimentos diferentes → não pode ser igual (mas não vaza info por timing).
        if (expectedBytes.Length != providedBytes.Length)
            return false;

        return CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }

    /// <inheritdoc />
    public bool ValidateSignature(string rawBody, string? signatureHeader)
    {
        if (string.IsNullOrWhiteSpace(signatureHeader))
            return false;

        if (string.IsNullOrWhiteSpace(_options.WebhookPublicKey))
            return false;

        // HMACSHA256(publicKey, rawBody) → Base64
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.WebhookPublicKey));
        var computedBase64 = Convert.ToBase64String(
            hmac.ComputeHash(Encoding.UTF8.GetBytes(rawBody)));

        var computedBytes = Encoding.UTF8.GetBytes(computedBase64);
        var signatureBytes = Encoding.UTF8.GetBytes(signatureHeader.Trim());

        if (computedBytes.Length != signatureBytes.Length)
            return false;

        return CryptographicOperations.FixedTimeEquals(computedBytes, signatureBytes);
    }
}
