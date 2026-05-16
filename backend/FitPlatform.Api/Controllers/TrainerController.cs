using System;
using FitPlatform.Application.DTOs.Subscription;
using FitPlatform.Application.DTOs.Trainer;
using FitPlatform.Application.Interfaces;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

[ApiController]
[Route("api/trainer")]
[Authorize(Roles = "Trainer")]
public class TrainerController : ControllerBase
{
    private readonly TrainerService _service;
    private readonly ICurrentUserService _currentUser;

    public TrainerController(TrainerService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var trainerId = _currentUser.TrainerId;
        if (!trainerId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _service.GetDashboardAsync(trainerId.Value);
        if (result.Success) return Ok(result);

        if (string.Equals(result.Message, "Trainer não encontrado.", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(result);
        }

        return StatusCode(500, result);
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _service.GetProfileAsync(_currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] TrainerProfileRequest request)
    {
        var result = await _service.UpdateProfileAsync(_currentUser.TrainerId!.Value, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("subscription")]
    public async Task<IActionResult> GetSubscription()
    {
        var result = await _service.GetSubscriptionAsync(_currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("subscription/create")]
    public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionRequest request, [FromServices] PaymentService paymentService)
    {
        var result = await paymentService.CreateSubscriptionAsync(_currentUser.TrainerId!.Value, request, HttpContext.RequestAborted);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
