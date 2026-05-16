using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FitPlatform.Application.Configuration;
using FitPlatform.Application.DTOs.Payment;
using FitPlatform.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitPlatform.Infrastructure.PaymentProviders;

public class AbacatePayPaymentProvider : IPaymentProvider
{
    private readonly HttpClient _httpClient;
    private readonly AbacatePayOptions _options;
    private readonly ILogger<AbacatePayPaymentProvider> _logger;

    public AbacatePayPaymentProvider(HttpClient httpClient, IOptions<AbacatePayOptions> options, ILogger<AbacatePayPaymentProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
    }

    public async Task<ProviderSubscriptionCreated> CreateSubscriptionAsync(CreateProviderSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        EnsureApiKey();
        var customerId = request.ExistingCustomerId ?? await CreateCustomerAsync(request, cancellationToken);

        var payload = new
        {
            items = new[] { new { id = request.AbacatePayProductId, quantity = 1 } },
            customerId,
            externalId = request.ExternalId,
            returnUrl = _options.ReturnUrl,
            completionUrl = _options.SuccessUrl,
            metadata = request.Metadata,
            methods = new[] { "CARD" }
        };

        var raw = await SendAsync("subscriptions/create", payload, cancellationToken);
        using var doc = JsonDocument.Parse(raw);
        var data = doc.RootElement.GetProperty("data");
        return new ProviderSubscriptionCreated
        {
            ProviderSubscriptionId = data.TryGetProperty("subscriptionId", out var s) ? s.GetString() ?? string.Empty : string.Empty,
            ProviderCheckoutId = data.TryGetProperty("id", out var id) ? id.GetString() : null,
            ProviderCustomerId = customerId,
            CheckoutUrl = data.TryGetProperty("url", out var url) ? url.GetString() : null,
            RawPayload = raw
        };
    }

    public async Task<ProviderSubscriptionDetails?> GetSubscriptionAsync(string providerSubscriptionId, CancellationToken cancellationToken = default)
    {
        var raw = await SendAsync("subscriptions/list", new { id = providerSubscriptionId }, cancellationToken);
        return new ProviderSubscriptionDetails { ProviderSubscriptionId = providerSubscriptionId, RawPayload = raw, Status = "unknown" };
    }

    public async Task CancelSubscriptionAsync(string providerSubscriptionId, CancellationToken cancellationToken = default)
        => await SendAsync("subscriptions/cancel", new { id = providerSubscriptionId }, cancellationToken);

    public async Task ChangeSubscriptionPlanAsync(ChangeProviderSubscriptionPlanRequest request, CancellationToken cancellationToken = default)
        => await SendAsync("subscriptions/change-plan", new { id = request.ProviderSubscriptionId, productId = request.ProductId, quantity = request.Quantity }, cancellationToken);

    private async Task<string> CreateCustomerAsync(CreateProviderSubscriptionRequest request, CancellationToken cancellationToken)
    {
        var raw = await SendAsync("customers/create", new
        {
            name = request.PayerName ?? request.PayerEmail,
            email = request.PayerEmail,
            cellphone = request.Phone
        }, cancellationToken);
        using var doc = JsonDocument.Parse(raw);
        return doc.RootElement.GetProperty("data").GetProperty("id").GetString()
            ?? throw new InvalidOperationException("AbacatePay não retornou customerId.");
    }

    private async Task<string> SendAsync(string path, object payload, CancellationToken cancellationToken)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, path);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        using var res = await _httpClient.SendAsync(req, cancellationToken);
        var raw = await res.Content.ReadAsStringAsync(cancellationToken);
        if (!res.IsSuccessStatusCode)
        {
            _logger.LogError("AbacatePay call failed {Path} {StatusCode}", path, (int)res.StatusCode);
            throw new InvalidOperationException($"Falha na AbacatePay em {path}. HTTP {(int)res.StatusCode}.");
        }
        return raw;
    }

    private void EnsureApiKey()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("AbacatePay:ApiKey não configurada.");
        }
    }
}
