using System.Security.Cryptography;
using System.Text;
using FitPlatform.Application.Configuration;
using FitPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace FitPlatform.Infrastructure.Services;

public class AbacatePayWebhookValidator : IPaymentWebhookValidator
{
    private readonly AbacatePayOptions _options;

    public AbacatePayWebhookValidator(IOptions<AbacatePayOptions> options)
    {
        _options = options.Value;
    }

    public bool IsValid(HttpRequest request, string rawBody)
    {
        if (string.IsNullOrWhiteSpace(_options.WebhookPublicKey))
        {
            return false;
        }

        var secretHeader = request.Headers["x-webhook-secret"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(_options.WebhookSecret) && !string.Equals(secretHeader, _options.WebhookSecret, StringComparison.Ordinal))
        {
            return false;
        }

        var signature = request.Headers["X-Webhook-Signature"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.WebhookPublicKey));
        var expected = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(rawBody)));
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(signature));
    }
}
