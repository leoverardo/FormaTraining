using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FitPlatform.Application.Configuration;
using FitPlatform.Application.DTOs.Payment;
using FitPlatform.Application.Interfaces;
using FitPlatform.Domain.Enums;
using Microsoft.Extensions.Options;

namespace FitPlatform.Infrastructure.PaymentProviders;

public class MercadoPagoPaymentProvider : IPaymentProvider
{
    private const string ApiBaseUrl = "https://api.mercadopago.com";
    private readonly HttpClient _httpClient;
    private readonly MercadoPagoOptions _options;

    public MercadoPagoPaymentProvider(HttpClient httpClient, IOptions<MercadoPagoOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<ProviderSubscriptionCreated> CreateSubscriptionAsync(CreateProviderSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        EnsureAccessToken();

        var (frequency, frequencyType) = MapFrequency(request.BillingCycle);
        var payload = new
        {
            reason = $"FitPlatform - {request.PlanName}",
            auto_recurring = new
            {
                frequency,
                frequency_type = frequencyType,
                transaction_amount = request.AmountInCents / 100m,
                currency_id = "BRL"
            },
            back_url = string.IsNullOrWhiteSpace(_options.PendingUrl) ? _options.SuccessUrl : _options.PendingUrl,
            payer_email = request.PayerEmail,
            external_reference = request.LocalSubscriptionId.ToString(),
            notification_url = _options.NotificationUrl,
            status = "pending"
        };

        using var message = CreateRequest(HttpMethod.Post, "/preapproval", payload);
        using var response = await _httpClient.SendAsync(message, cancellationToken);
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Falha ao criar assinatura no Mercado Pago. HTTP {(int)response.StatusCode}.");
        }

        using var doc = JsonDocument.Parse(raw);
        var root = doc.RootElement;

        var providerSubscriptionId = root.TryGetProperty("id", out var idNode) ? idNode.GetString() : null;
        if (string.IsNullOrWhiteSpace(providerSubscriptionId))
        {
            throw new InvalidOperationException("Mercado Pago não retornou ID de assinatura.");
        }

        return new ProviderSubscriptionCreated
        {
            ProviderSubscriptionId = providerSubscriptionId,
            PayerId = root.TryGetProperty("payer_id", out var payerIdNode) ? payerIdNode.ToString() : null,
            CheckoutUrl = root.TryGetProperty("init_point", out var initPointNode) ? initPointNode.GetString() : null,
            RawPayload = raw
        };
    }

    public async Task<ProviderSubscriptionDetails?> GetSubscriptionAsync(string providerSubscriptionId, CancellationToken cancellationToken = default)
    {
        EnsureAccessToken();
        using var message = CreateRequest(HttpMethod.Get, $"/preapproval/{providerSubscriptionId}");
        using var response = await _httpClient.SendAsync(message, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Falha ao consultar assinatura no Mercado Pago. HTTP {(int)response.StatusCode}.");
        }

        using var doc = JsonDocument.Parse(raw);
        var root = doc.RootElement;
        string? lastPaymentId = null;
        string? lastPaymentStatus = null;
        decimal? lastPaymentAmount = null;
        DateTime? lastPaymentDate = null;

        if (root.TryGetProperty("last_charged_date", out var chargedDateNode) && chargedDateNode.ValueKind == JsonValueKind.String && DateTime.TryParse(chargedDateNode.GetString(), out var chargedAt))
        {
            lastPaymentDate = chargedAt;
        }

        if (root.TryGetProperty("next_payment_date", out var nextPaymentDateNode) && lastPaymentDate == null && nextPaymentDateNode.ValueKind == JsonValueKind.String && DateTime.TryParse(nextPaymentDateNode.GetString(), out var nextAt))
        {
            lastPaymentDate = nextAt;
        }

        if (root.TryGetProperty("status", out var statusNode))
        {
            lastPaymentStatus = statusNode.GetString();
        }

        if (root.TryGetProperty("auto_recurring", out var autoRecurring) &&
            autoRecurring.TryGetProperty("transaction_amount", out var amountNode) &&
            amountNode.TryGetDecimal(out var amount))
        {
            lastPaymentAmount = amount;
        }

        return new ProviderSubscriptionDetails
        {
            ProviderSubscriptionId = root.TryGetProperty("id", out var idNode) ? idNode.GetString() ?? providerSubscriptionId : providerSubscriptionId,
            PayerId = root.TryGetProperty("payer_id", out var payerNode) ? payerNode.ToString() : null,
            Status = root.TryGetProperty("status", out var rootStatusNode) ? rootStatusNode.GetString() ?? "unknown" : "unknown",
            LastPaymentId = lastPaymentId,
            LastPaymentStatus = lastPaymentStatus,
            LastPaymentAmount = lastPaymentAmount,
            LastPaymentDate = lastPaymentDate,
            RawPayload = raw
        };
    }

    public async Task CancelSubscriptionAsync(string providerSubscriptionId, CancellationToken cancellationToken = default)
    {
        EnsureAccessToken();
        using var message = CreateRequest(HttpMethod.Put, $"/preapproval/{providerSubscriptionId}", new { status = "cancelled" });
        using var response = await _httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Falha ao cancelar assinatura no Mercado Pago. HTTP {(int)response.StatusCode}.");
        }
    }

    public Task ChangeSubscriptionPlanAsync(ChangeProviderSubscriptionPlanRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("MercadoPago provider legado não suporta troca de plano via esta integração.");
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path, object? body = null)
    {
        var message = new HttpRequestMessage(method, $"{ApiBaseUrl}{path}");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);
        if (body != null)
        {
            message.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        }
        return message;
    }

    private void EnsureAccessToken()
    {
        if (string.IsNullOrWhiteSpace(_options.AccessToken))
        {
            throw new InvalidOperationException("MercadoPago:AccessToken não configurado.");
        }
    }

    private static (int frequency, string frequencyType) MapFrequency(BillingFrequency billingCycle) =>
        billingCycle switch
        {
            BillingFrequency.Quarterly => (3, "months"),
            BillingFrequency.Yearly => (12, "months"),
            _ => (1, "months")
        };
}
