using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FitPlatform.Application.Configuration;
using FitPlatform.Application.DTOs.Payment;
using FitPlatform.Application.Exceptions;
using FitPlatform.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitPlatform.Infrastructure.PaymentProviders;

/// <summary>
/// Cliente da API v2 da AbacatePay (https://api.abacatepay.com/v2).
///
/// Endpoints usados (todos documentados oficialmente):
///   POST /customers/create
///   POST /subscriptions/create      — items (1 produto), customerId, externalId,
///                                     returnUrl, completionUrl, methods, metadata,
///                                     coupons, retryPolicy
///   GET  /subscriptions/list        — after, before, limit, startDate, endDate, status
///   POST /subscriptions/cancel      — { id }
///   POST /subscriptions/change-plan — { id, productId, quantity }
///   POST /coupons/create            — code, discountKind, discount, maxRedeems, startsAt, expiresAt
///
/// Nenhum log deste cliente inclui API key, corpo de requisição ou payload de resposta.
/// </summary>
public class AbacatePayPaymentProvider : IPaymentProvider
{
    private readonly HttpClient _httpClient;
    private readonly AbacatePayOptions _options;
    private readonly ILogger<AbacatePayPaymentProvider> _logger;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public AbacatePayPaymentProvider(HttpClient httpClient, IOptions<AbacatePayOptions> options, ILogger<AbacatePayPaymentProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Criação de checkout de assinatura
    // ═══════════════════════════════════════════════════════════════════════════

    public async Task<ProviderSubscriptionCreated> CreateSubscriptionAsync(CreateProviderSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        EnsureApiKey();

        if (string.IsNullOrWhiteSpace(request.AbacatePayProductId))
            throw new PaymentProviderException("Produto AbacatePay não informado para o checkout.");

        var customerId = request.ExistingCustomerId ?? await CreateCustomerAsync(request, cancellationToken);

        var coupons = request.CouponCodes
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var payload = new
        {
            items = new[] { new { id = request.AbacatePayProductId, quantity = 1 } },
            customerId,
            externalId = request.ExternalId,
            returnUrl = string.IsNullOrWhiteSpace(request.ReturnUrl) ? _options.ReturnUrl : request.ReturnUrl,
            completionUrl = string.IsNullOrWhiteSpace(request.CompletionUrl) ? _options.SuccessUrl : request.CompletionUrl,
            metadata = request.Metadata,
            methods = new[] { NormalizePaymentMethod(_options.PaymentMethod) },
            coupons = coupons.Length > 0 ? coupons : null,
            retryPolicy = new
            {
                maxRetry = _options.RetryPolicy.MaxRetry,
                retryEvery = _options.RetryPolicy.RetryEvery
            }
        };

        var raw = await SendAsync(HttpMethod.Post, "subscriptions/create", payload, cancellationToken);
        using var doc = JsonDocument.Parse(raw);

        if (!doc.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object)
            throw new PaymentProviderException("Resposta da AbacatePay em subscriptions/create não contém 'data'.");

        var checkoutId = ReadString(data, "id");
        var checkoutUrl = ReadString(data, "url");

        if (string.IsNullOrWhiteSpace(checkoutUrl))
            throw new PaymentProviderException("AbacatePay não retornou a URL de checkout.");

        _logger.LogInformation(
            "Checkout AbacatePay criado. ExternalId={ExternalId} CheckoutId={CheckoutId} CouponCount={CouponCount}",
            request.ExternalId, checkoutId ?? "N/A", coupons.Length);

        return new ProviderSubscriptionCreated
        {
            // Antes do pagamento a AbacatePay ainda não emitiu o subs_...; o id retornado é o do checkout.
            ProviderSubscriptionId = ReadString(data, "subscriptionId") ?? string.Empty,
            ProviderCheckoutId = checkoutId,
            ProviderCustomerId = customerId,
            CheckoutUrl = checkoutUrl,
            RawPayload = raw
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Consulta / reconciliação
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Consulta uma assinatura específica por id de checkout (<c>bill_...</c>), id de
    /// assinatura (<c>subs_...</c>) ou <c>externalId</c>.
    ///
    /// LIMITAÇÃO CONHECIDA DA API: a v2 não possui um endpoint confiável de busca
    /// individual. A página de referência menciona <c>GET /subscriptions/get</c> em prosa,
    /// mas esse endpoint não tem página própria no índice oficial (<c>llms.txt</c>),
    /// <c>/pages/subscriptions/get</c> responde 404, nenhum SDK oficial o implementa e o
    /// contrato de requisição (nome do parâmetro) não está documentado em lugar nenhum.
    /// Implementá-lo seria construir sobre documentação ambígua, então a consulta usa
    /// <c>GET /subscriptions/list</c>, que é documentado e estável.
    ///
    /// Mitigações de custo, já que <c>list</c> não aceita filtro por id:
    ///   • a paginação para assim que o item é encontrado (não lê todas as páginas);
    ///   • <see cref="ProviderSubscriptionListQuery.StartDate"/>/<c>EndDate</c> restringem
    ///     a janela quando o chamador sabe a data de criação;
    ///   • a reconciliação NÃO usa este método por assinatura: resolve o lote inteiro
    ///     com uma única varredura em <see cref="ListSubscriptionsAsync"/>.
    /// </summary>
    public async Task<ProviderSubscriptionDetails?> GetSubscriptionAsync(
        string providerSubscriptionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(providerSubscriptionId)) return null;

        var query = new ProviderSubscriptionListQuery
        {
            Limit = 100,
            MaxPages = _options.Reconciliation.MaxListPages
        };

        var match = await EnumerateSubscriptionsAsync(
            query,
            stopWhen: x =>
                string.Equals(x.ProviderCheckoutId, providerSubscriptionId, StringComparison.Ordinal) ||
                string.Equals(x.ProviderSubscriptionId, providerSubscriptionId, StringComparison.Ordinal) ||
                string.Equals(x.ExternalId, providerSubscriptionId, StringComparison.Ordinal),
            cancellationToken);

        return match.Match;
    }

    public async Task<IReadOnlyList<ProviderSubscriptionDetails>> ListSubscriptionsAsync(
        ProviderSubscriptionListQuery query, CancellationToken cancellationToken = default)
    {
        var result = await EnumerateSubscriptionsAsync(query, stopWhen: null, cancellationToken);

        _logger.LogDebug(
            "AbacatePay subscriptions/list concluído. Itens={Count} Paginas={Pages} StartDate={StartDate} Status={Status}",
            result.Items.Count, result.PagesRead,
            query.StartDate?.ToString("yyyy-MM-dd") ?? "N/A", query.Status ?? "N/A");

        return result.Items;
    }

    /// <summary>
    /// Percorre <c>GET /subscriptions/list</c> com paginação limitada.
    /// Quando <paramref name="stopWhen"/> é informado, interrompe assim que o primeiro
    /// item correspondente aparece — evitando ler páginas desnecessárias.
    /// </summary>
    private async Task<(IReadOnlyList<ProviderSubscriptionDetails> Items, ProviderSubscriptionDetails? Match, int PagesRead)>
        EnumerateSubscriptionsAsync(
            ProviderSubscriptionListQuery query,
            Func<ProviderSubscriptionDetails, bool>? stopWhen,
            CancellationToken cancellationToken)
    {
        EnsureApiKey();

        var results = new List<ProviderSubscriptionDetails>();
        var limit = Math.Clamp(query.Limit, 1, 100);
        var maxPages = Math.Clamp(query.MaxPages, 1, 50);
        string? cursor = null;
        var pagesRead = 0;

        for (var page = 0; page < maxPages; page++)
        {
            var path = BuildListPath(query, limit, cursor);
            var raw = await SendAsync(HttpMethod.Get, path, body: null, cancellationToken);
            pagesRead++;

            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in data.EnumerateArray())
                {
                    var mapped = MapSubscription(item);
                    results.Add(mapped);

                    if (stopWhen is not null && stopWhen(mapped))
                        return (results, mapped, pagesRead);
                }
            }

            cursor = null;
            if (root.TryGetProperty("pagination", out var pagination) && pagination.ValueKind == JsonValueKind.Object)
            {
                var hasMore = pagination.TryGetProperty("hasMore", out var hasMoreNode)
                              && hasMoreNode.ValueKind == JsonValueKind.True;
                if (hasMore) cursor = ReadString(pagination, "next");
            }

            if (string.IsNullOrWhiteSpace(cursor)) break;
        }

        return (results, null, pagesRead);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Cancelamento e troca de plano
    // ═══════════════════════════════════════════════════════════════════════════

    public async Task CancelSubscriptionAsync(string providerSubscriptionId, CancellationToken cancellationToken = default)
    {
        EnsureApiKey();
        if (string.IsNullOrWhiteSpace(providerSubscriptionId))
            throw new PaymentProviderException("Id de assinatura AbacatePay não informado para cancelamento.");

        await SendAsync(HttpMethod.Post, "subscriptions/cancel", new { id = providerSubscriptionId }, cancellationToken);
        _logger.LogInformation("Assinatura cancelada na AbacatePay. ProviderSubscriptionId={ProviderSubscriptionId}", providerSubscriptionId);
    }

    public async Task<ProviderPlanChangeResult> ChangeSubscriptionPlanAsync(
        ChangeProviderSubscriptionPlanRequest request, CancellationToken cancellationToken = default)
    {
        EnsureApiKey();
        if (string.IsNullOrWhiteSpace(request.ProviderSubscriptionId))
            throw new PaymentProviderException("Id de assinatura AbacatePay não informado para troca de plano.");
        if (string.IsNullOrWhiteSpace(request.ProductId))
            throw new PaymentProviderException("Produto AbacatePay de destino não informado.");

        var raw = await SendAsync(HttpMethod.Post, "subscriptions/change-plan", new
        {
            id = request.ProviderSubscriptionId,
            productId = request.ProductId,
            quantity = Math.Max(1, request.Quantity)
        }, cancellationToken);

        using var doc = JsonDocument.Parse(raw);
        var data = doc.RootElement.TryGetProperty("data", out var node) && node.ValueKind == JsonValueKind.Object
            ? node
            : default;

        var result = new ProviderPlanChangeResult
        {
            UpdateId = data.ValueKind == JsonValueKind.Object ? ReadString(data, "id") : null,
            ProviderSubscriptionId = data.ValueKind == JsonValueKind.Object
                ? ReadString(data, "subscriptionId") ?? request.ProviderSubscriptionId
                : request.ProviderSubscriptionId,
            Status = (data.ValueKind == JsonValueKind.Object ? ReadString(data, "status") : null) ?? "PENDING",
            ProductId = data.ValueKind == JsonValueKind.Object ? ReadString(data, "productId") : request.ProductId,
            Quantity = data.ValueKind == JsonValueKind.Object ? ReadInt(data, "quantity") ?? request.Quantity : request.Quantity,
            NewAmountInCents = data.ValueKind == JsonValueKind.Object ? ReadInt(data, "newAmount") : null,
            RequestedAt = data.ValueKind == JsonValueKind.Object ? ReadDate(data, "requestedAt") : null,
            RawPayload = raw
        };

        _logger.LogInformation(
            "Troca de plano agendada na AbacatePay. ProviderSubscriptionId={ProviderSubscriptionId} UpdateId={UpdateId} Status={Status}",
            result.ProviderSubscriptionId, result.UpdateId ?? "N/A", result.Status);

        return result;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Cupons
    // ═══════════════════════════════════════════════════════════════════════════

    public async Task<ProviderCouponSyncResult> EnsureCouponAsync(
        ProviderCouponDefinition coupon, CancellationToken cancellationToken = default)
    {
        EnsureApiKey();
        if (string.IsNullOrWhiteSpace(coupon.Code))
            throw new PaymentProviderException("Código de cupom não informado.");

        var payload = new
        {
            code = coupon.Code,
            discountKind = coupon.DiscountKind,
            discount = coupon.Discount,
            maxRedeems = coupon.MaxRedeems,
            notes = string.IsNullOrWhiteSpace(coupon.Notes) ? coupon.Code : coupon.Notes,
            startsAt = coupon.StartsAt?.ToUniversalTime().ToString("o"),
            expiresAt = coupon.ExpiresAt?.ToUniversalTime().ToString("o")
        };

        try
        {
            var raw = await SendAsync(HttpMethod.Post, "coupons/create", payload, cancellationToken);
            _logger.LogInformation("Cupom espelhado na AbacatePay. Code={CouponCode}", coupon.Code);
            return new ProviderCouponSyncResult { Code = coupon.Code, AlreadyExisted = false, RawPayload = raw };
        }
        catch (PaymentProviderException ex) when (ex.IndicatesAlreadyExists)
        {
            // Idempotência: o cupom já está cadastrado no provedor com este código.
            _logger.LogInformation("Cupom já existia na AbacatePay — reaproveitado. Code={CouponCode}", coupon.Code);
            return new ProviderCouponSyncResult { Code = coupon.Code, AlreadyExisted = true };
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Infra HTTP
    // ═══════════════════════════════════════════════════════════════════════════

    private async Task<string> CreateCustomerAsync(CreateProviderSubscriptionRequest request, CancellationToken cancellationToken)
    {
        var raw = await SendAsync(HttpMethod.Post, "customers/create", new
        {
            name = request.PayerName ?? request.PayerEmail,
            email = request.PayerEmail,
            cellphone = request.Phone
        }, cancellationToken);

        using var doc = JsonDocument.Parse(raw);
        if (!doc.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object)
            throw new PaymentProviderException("AbacatePay não retornou 'data' em customers/create.");

        return ReadString(data, "id")
            ?? throw new PaymentProviderException("AbacatePay não retornou customerId.");
    }

    private async Task<string> SendAsync(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        using var req = new HttpRequestMessage(method, path);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (body is not null)
        {
            req.Content = new StringContent(
                JsonSerializer.Serialize(body, SerializerOptions), Encoding.UTF8, "application/json");
        }

        HttpResponseMessage res;
        try
        {
            res = await _httpClient.SendAsync(req, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            // Falha de rede/DNS/TLS. Não inclui corpo nem credenciais.
            _logger.LogError(ex, "Falha de rede ao chamar AbacatePay. Path={Path}", path);
            throw new PaymentProviderException($"Não foi possível comunicar com a AbacatePay em {path}.", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Timeout ao chamar AbacatePay. Path={Path}", path);
            throw new PaymentProviderException($"Tempo esgotado ao chamar a AbacatePay em {path}.", ex);
        }

        using (res)
        {
            var raw = await res.Content.ReadAsStringAsync(cancellationToken);

            if (!res.IsSuccessStatusCode)
            {
                // Log estruturado sem payload: status + path apenas.
                _logger.LogError(
                    "Chamada AbacatePay falhou. Path={Path} StatusCode={StatusCode}", path, (int)res.StatusCode);

                throw new PaymentProviderException(
                    $"Falha na AbacatePay em {path}. HTTP {(int)res.StatusCode}.",
                    statusCode: res.StatusCode,
                    indicatesAlreadyExists: IndicatesAlreadyExists(res.StatusCode, raw));
            }

            // A API responde { data, success, error }. Um success=false com HTTP 200 também é erro.
            if (TryReadApiError(raw, out var apiError))
            {
                _logger.LogError("AbacatePay retornou erro de aplicação. Path={Path}", path);
                throw new PaymentProviderException(
                    $"Falha na AbacatePay em {path}.",
                    statusCode: res.StatusCode,
                    indicatesAlreadyExists: MentionsAlreadyExists(apiError));
            }

            return raw;
        }
    }

    private string BuildListPath(ProviderSubscriptionListQuery query, int limit, string? cursor)
    {
        var parts = new List<string> { $"limit={limit}" };

        if (query.StartDate.HasValue) parts.Add($"startDate={query.StartDate.Value:yyyy-MM-dd}");
        if (query.EndDate.HasValue) parts.Add($"endDate={query.EndDate.Value:yyyy-MM-dd}");
        if (!string.IsNullOrWhiteSpace(query.Status)) parts.Add($"status={Uri.EscapeDataString(query.Status)}");
        if (!string.IsNullOrWhiteSpace(cursor)) parts.Add($"after={Uri.EscapeDataString(cursor)}");

        return $"subscriptions/list?{string.Join('&', parts)}";
    }

    private static ProviderSubscriptionDetails MapSubscription(JsonElement item)
    {
        var status = ReadString(item, "status") ?? string.Empty;
        return new ProviderSubscriptionDetails
        {
            ProviderSubscriptionId = ReadString(item, "subscriptionId") ?? string.Empty,
            ProviderCheckoutId = ReadString(item, "id"),
            ExternalId = ReadString(item, "externalId"),
            CustomerId = ReadString(item, "customerId"),
            Status = status,
            State = MapState(status),
            AmountInCents = ReadInt(item, "amount"),
            PaidAmountInCents = ReadInt(item, "paidAmount"),
            LastPaymentDate = ReadDate(item, "updatedAt"),
            RawPayload = item.GetRawText()
        };
    }

    /// <summary>Mapeia os status da AbacatePay para o vocabulário interno.</summary>
    private static ProviderSubscriptionState MapState(string status) => status.ToUpperInvariant() switch
    {
        "PENDING" => ProviderSubscriptionState.Pending,
        "PAID" or "ACTIVE" => ProviderSubscriptionState.Paid,
        "EXPIRED" => ProviderSubscriptionState.Expired,
        "CANCELLED" or "CANCELED" => ProviderSubscriptionState.Cancelled,
        "REFUNDED" => ProviderSubscriptionState.Refunded,
        _ => ProviderSubscriptionState.Unknown
    };

    private static string NormalizePaymentMethod(string method) =>
        string.Equals(method, "PIX", StringComparison.OrdinalIgnoreCase) ? "PIX" : "CARD";

    private static bool TryReadApiError(string raw, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(raw)) return false;

        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return false;

            if (root.TryGetProperty("success", out var success) && success.ValueKind == JsonValueKind.False)
            {
                error = root.TryGetProperty("error", out var err) ? err.ToString() : "erro não especificado";
                return true;
            }

            if (root.TryGetProperty("error", out var errorNode) &&
                errorNode.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined)
            {
                var text = errorNode.ToString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    error = text;
                    return true;
                }
            }
        }
        catch (JsonException)
        {
            // Resposta não-JSON de sucesso: nada a reportar aqui.
        }

        return false;
    }

    private static bool IndicatesAlreadyExists(HttpStatusCode statusCode, string raw) =>
        statusCode == HttpStatusCode.Conflict ||
        (statusCode == HttpStatusCode.BadRequest && MentionsAlreadyExists(raw));

    private static bool MentionsAlreadyExists(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        return text.Contains("already exists", StringComparison.OrdinalIgnoreCase)
            || text.Contains("already registered", StringComparison.OrdinalIgnoreCase)
            || text.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
            || text.Contains("já existe", StringComparison.OrdinalIgnoreCase)
            || text.Contains("ja existe", StringComparison.OrdinalIgnoreCase);
    }

    private static string? ReadString(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var node)) return null;
        return node.ValueKind switch
        {
            JsonValueKind.String => node.GetString(),
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            _ => node.ToString()
        };
    }

    private static int? ReadInt(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var node)) return null;
        return node.ValueKind == JsonValueKind.Number && node.TryGetInt32(out var value) ? value : null;
    }

    private static DateTime? ReadDate(JsonElement element, string property)
    {
        var text = ReadString(element, property);
        return DateTime.TryParse(text, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var parsed)
            ? parsed
            : null;
    }

    private void EnsureApiKey()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey) || _options.ApiKey.StartsWith("SET_", StringComparison.Ordinal))
        {
            throw new PaymentProviderException("AbacatePay:ApiKey não configurada.");
        }
    }
}
