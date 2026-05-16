using FitPlatform.Application.DTOs.Onboarding;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Hosting;

namespace FitPlatform.Api.Controllers;

[ApiController]
[Route("api/public/trainer-onboarding")]
[AllowAnonymous]
public class PublicOnboardingController : ControllerBase
{
    private readonly OnboardingService _service;
    private readonly IHostEnvironment _environment;

    public PublicOnboardingController(OnboardingService service, IHostEnvironment environment)
    {
        _service = service;
        _environment = environment;
    }

    [HttpPost]
    [EnableRateLimiting("TrainerOnboarding")]
    public async Task<IActionResult> Start([FromBody] StartOnboardingRequest request)
    {
        var result = await _service.StartAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _service.GetAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("{id:guid}/professional-data")]
    [EnableRateLimiting("TrainerOnboarding")]
    public async Task<IActionResult> UpdateProfessional(Guid id, [FromBody] UpdateProfessionalDataRequest request)
    {
        var result = await _service.UpdateProfessionalDataAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("{id:guid}/address")]
    [EnableRateLimiting("TrainerOnboarding")]
    public async Task<IActionResult> UpdateAddress(Guid id, [FromBody] UpdateAddressRequest request)
    {
        var result = await _service.UpdateAddressAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("{id:guid}/select-plan")]
    [EnableRateLimiting("TrainerOnboarding")]
    public async Task<IActionResult> SelectPlan(Guid id, [FromBody] SelectPlanRequest request)
    {
        var result = await _service.SelectPlanAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/simulate-payment-approved")]
    public async Task<IActionResult> SimulatePaymentApproved(Guid id)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }
        var result = await _service.SimulatePaymentApprovedAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/validate-coupon")]
    [EnableRateLimiting("TrainerOnboarding")]
    public async Task<IActionResult> ValidateCoupon(Guid id, [FromBody] CreateOnboardingCheckoutRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.ValidateCouponPreviewAsync(id, request.CouponCode, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/checkout")]
    [EnableRateLimiting("TrainerOnboarding")]
    public async Task<IActionResult> CreateCheckout(Guid id, [FromBody] CreateOnboardingCheckoutRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateCheckoutAsync(id, request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:guid}/payment-status")]
    public async Task<IActionResult> PaymentStatus(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetPaymentStatusAsync(id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
