using FitPlatform.Application.DTOs.Onboarding;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

[ApiController]
[Route("api/public/trainer-onboarding")]
[AllowAnonymous]
public class PublicOnboardingController : ControllerBase
{
    private readonly OnboardingService _service;

    public PublicOnboardingController(OnboardingService service) => _service = service;

    [HttpPost]
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
    public async Task<IActionResult> UpdateProfessional(Guid id, [FromBody] UpdateProfessionalDataRequest request)
    {
        var result = await _service.UpdateProfessionalDataAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("{id:guid}/address")]
    public async Task<IActionResult> UpdateAddress(Guid id, [FromBody] UpdateAddressRequest request)
    {
        var result = await _service.UpdateAddressAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("{id:guid}/select-plan")]
    public async Task<IActionResult> SelectPlan(Guid id, [FromBody] SelectPlanRequest request)
    {
        var result = await _service.SelectPlanAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/simulate-payment-approved")]
    public async Task<IActionResult> SimulatePaymentApproved(Guid id)
    {
        var result = await _service.SimulatePaymentApprovedAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
