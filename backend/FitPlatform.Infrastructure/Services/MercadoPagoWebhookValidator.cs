using System.Security.Cryptography;
using System.Text;
using FitPlatform.Application.Configuration;
using FitPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace FitPlatform.Infrastructure.Services;

public class MercadoPagoWebhookValidator : IMercadoPagoWebhookValidator
{
    private readonly MercadoPagoOptions _options;

    public MercadoPagoWebhookValidator(IOptions<MercadoPagoOptions> options)
    {
        _options = options.Value;
    }

    public bool IsValid(HttpRequest request)
    {
        if (string.IsNullOrWhiteSpace(_options.WebhookSecret))
        {
            return true;
        }

        var signature = request.Headers["x-signature"].FirstOrDefault();
        var requestId = request.Headers["x-request-id"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(signature) || string.IsNullOrWhiteSpace(requestId))
        {
            return false;
        }

        var parts = signature.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var ts = parts.FirstOrDefault(p => p.StartsWith("ts=", StringComparison.OrdinalIgnoreCase))?.Split('=')[1];
        var v1 = parts.FirstOrDefault(p => p.StartsWith("v1=", StringComparison.OrdinalIgnoreCase))?.Split('=')[1];
        if (string.IsNullOrWhiteSpace(ts) || string.IsNullOrWhiteSpace(v1))
        {
            return false;
        }

        var dataId = request.Query["data.id"].FirstOrDefault() ?? string.Empty;
        var manifest = $"id:{dataId};request-id:{requestId};ts:{ts};";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.WebhookSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(manifest));
        var computed = Convert.ToHexString(hash).ToLowerInvariant();

        return string.Equals(computed, v1, StringComparison.OrdinalIgnoreCase);
    }
}
