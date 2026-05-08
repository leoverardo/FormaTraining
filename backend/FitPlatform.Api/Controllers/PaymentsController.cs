using FitPlatform.Application.DTOs.Subscription;
using FitPlatform.Application.Interfaces;
using FitPlatform.Application.Common;
using FitPlatform.Infrastructure.Services;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly IMercadoPagoWebhookValidator _webhookValidator;

    public PaymentsController(PaymentService service, ICurrentUserService currentUser, IMercadoPagoWebhookValidator webhookValidator)
    {
        _service = service;
        _currentUser = currentUser;
        _webhookValidator = webhookValidator;
    }

    [HttpPost("create-trainer-subscription")]
    [Authorize(Roles = "Trainer")]
    public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionRequest request)
    {
        var result = await _service.CreateSubscriptionAsync(_currentUser.TrainerId!.Value, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook([FromBody] JsonElement payload, CancellationToken cancellationToken)
    {
        if (!_webhookValidator.IsValid(Request))
        {
            return Unauthorized(ApiResponse.Fail("Assinatura do webhook inválida."));
        }

        var eventId = Request.Query["id"].FirstOrDefault() ?? Request.Headers["x-id"].FirstOrDefault();
        var eventType = Request.Query["type"].FirstOrDefault() ?? Request.Query["topic"].FirstOrDefault();
        var resourceId = Request.Query["data.id"].FirstOrDefault();

        var result = await _service.HandleWebhookAsync(payload, eventId, eventType, resourceId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("simulate-approved")]
    [Authorize(Roles = "Trainer")]
    public async Task<IActionResult> SimulateApproved()
    {
        var result = await _service.SimulateApprovedAsync(_currentUser.TrainerId!.Value, HttpContext.RequestAborted);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("simulate-expired")]
    [Authorize(Roles = "Trainer")]
    public async Task<IActionResult> SimulateExpired()
    {
        var result = await _service.SimulateExpiredAsync(_currentUser.TrainerId!.Value, HttpContext.RequestAborted);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
