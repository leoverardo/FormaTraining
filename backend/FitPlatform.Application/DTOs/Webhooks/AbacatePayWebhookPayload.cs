using System.Text.Json;
using System.Text.Json.Serialization;

namespace FitPlatform.Application.DTOs.Webhooks;

/// <summary>
/// Payload base de eventos de webhook do AbacatePay.
/// O campo <see cref="Data"/> é mantido como <see cref="JsonElement"/> para suportar
/// campos adicionais futuros sem quebrar a desserialização.
/// </summary>
public sealed class AbacatePayWebhookPayload
{
    /// <summary>ID único do evento de log (usar para idempotência).</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Tipo do evento, ex: subscription.completed, subscription.renewed,
    /// subscription.cancelled, checkout.completed.
    /// </summary>
    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty;

    /// <summary>Versão da API do webhook (ex: 2).</summary>
    [JsonPropertyName("apiVersion")]
    public int ApiVersion { get; set; }

    /// <summary>
    /// True quando o evento foi gerado em modo de desenvolvimento (sandbox).
    /// Eventos devMode nunca devem gerar cobranças reais.
    /// </summary>
    [JsonPropertyName("devMode")]
    public bool DevMode { get; set; }

    /// <summary>
    /// Dados específicos do evento. A estrutura varia por tipo de evento e pode
    /// receber novos campos em versões futuras da API — nunca validar de forma rígida.
    /// </summary>
    [JsonPropertyName("data")]
    public JsonElement Data { get; set; }
}
