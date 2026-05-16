using System.Text;
using System.Text.Json;
using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Subscription;
using FitPlatform.Application.Interfaces;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly IPaymentWebhookValidator _webhookValidator;

    public PaymentsController(PaymentService service, ICurrentUserService currentUser, IPaymentWebhookValidator webhookValidator)
    {
        _service = service;
        _currentUser = currentUser;
        _webhookValidator = webhookValidator;
    }

    [HttpPost("subscriptions/checkout")]
    [Authorize(Roles = "Trainer")]
    public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateSubscriptionAsync(_currentUser.TrainerId!.Value, request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("plans/{planId:guid}/billing-options")]
    [Authorize]
    public async Task<IActionResult> GetBillingOptions(Guid planId, CancellationToken cancellationToken)
    {
        var result = await _service.GetBillingOptionsAsync(planId, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("subscriptions/validate-coupon")]
    [Authorize]
    public async Task<IActionResult> ValidateCoupon([FromBody] ValidateCouponRequest request, CancellationToken cancellationToken)
    {
        var trainerId = _currentUser.TrainerId ?? Guid.Empty;
        var result = await _service.ValidateCouponAsync(trainerId, request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("webhooks/abacatepay")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var raw = await reader.ReadToEndAsync(cancellationToken);
        Request.Body.Position = 0;

        if (!_webhookValidator.IsValid(Request, raw))
        {
            return Unauthorized(ApiResponse.Fail("Assinatura do webhook inválida."));
        }

        using var doc = JsonDocument.Parse(raw);
        var payload = doc.RootElement.Clone();

        var result = await _service.HandleWebhookAsync(payload, null, null, null, cancellationToken);
        return Ok(result);
    }
}
