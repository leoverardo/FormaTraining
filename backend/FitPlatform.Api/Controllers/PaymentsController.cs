using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Subscription;
using FitPlatform.Application.Interfaces;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

/// <summary>
/// Precificação e criação de checkout de assinatura.
///
/// O webhook da AbacatePay NÃO é atendido aqui. O endpoint oficial e único é
/// <c>POST /api/webhooks/abacatepay</c>, servido por <see cref="WebhooksController"/>.
/// </summary>
[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentService _service;
    private readonly ICurrentUserService _currentUser;

    public PaymentsController(PaymentService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpPost("subscriptions/checkout")]
    [Authorize(Roles = "Trainer")]
    public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionRequest request, CancellationToken cancellationToken)
    {
        if (!_currentUser.TrainerId.HasValue)
            return Unauthorized(ApiResponse.Fail("Trainer não identificado no token."));

        var result = await _service.CreateSubscriptionAsync(_currentUser.TrainerId.Value, request, cancellationToken);
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
    [Authorize(Roles = "Trainer")]
    public async Task<IActionResult> ValidateCoupon([FromBody] ValidateCouponRequest request, CancellationToken cancellationToken)
    {
        var trainerId = _currentUser.TrainerId ?? Guid.Empty;
        var result = await _service.ValidateCouponAsync(trainerId, request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
