using System.Text;
using FitPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

/// <summary>
/// Controller dedicado a webhooks de provedores externos.
/// Thin controller: lê o body raw, extrai headers/query params e delega ao serviço.
/// </summary>
[ApiController]
[Route("api/webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly IAbacatePayWebhookService _webhookService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        IAbacatePayWebhookService webhookService,
        IWebHostEnvironment environment,
        ILogger<WebhooksController> logger)
    {
        _webhookService = webhookService;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Endpoint de webhook AbacatePay.
    /// URL: POST /api/webhooks/abacatepay?webhookSecret={secret}
    /// Headers obrigatórios em produção: X-Webhook-Signature (HMAC-SHA256 do body).
    /// Retorna 200 para todos os eventos aceitos (incluindo duplicados).
    /// Retorna 401 apenas para falhas de autenticação/assinatura.
    /// Retorna 500 em erros internos (para que a AbacatePay retente).
    /// </summary>
    [HttpPost("abacatepay")]
    [AllowAnonymous]
    public async Task<IActionResult> AbacatePayWebhook(
        [FromQuery] string? webhookSecret,
        CancellationToken cancellationToken)
    {
        // Ler body raw (necessário para validação HMAC e para salvar no log).
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);
        Request.Body.Position = 0;

        var signatureHeader = Request.Headers["X-Webhook-Signature"].FirstOrDefault();
        var isDev = _environment.IsDevelopment();

        _logger.LogDebug(
            "Webhook AbacatePay recebido. HasSignature={HasSignature} IsDev={IsDev}",
            !string.IsNullOrWhiteSpace(signatureHeader), isDev);

        var result = await _webhookService.HandleAsync(
            rawBody,
            signatureHeader,
            webhookSecret,
            isDev,
            cancellationToken);

        if (result.IsUnauthorized)
            return Unauthorized(new { error = result.Message });

        return Ok(new { message = result.Message });
    }
}
