using FitPlatform.Application.Interfaces;
using FitPlatform.Api.Authorization;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

[ApiController]
[Route("api/trainer")]
[Authorize(Roles = "Trainer")]
[RequireActiveTrainerSubscription]
public class StudentMonitoringController : ControllerBase
{
    private readonly StudentMonitoringService _service;
    private readonly ICurrentUserService _currentUser;

    public StudentMonitoringController(StudentMonitoringService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("students-monitoring")]
    public async Task<IActionResult> GetMonitoring()
    {
        var result = await _service.GetStudentsMonitoringAsync(_currentUser.TrainerId!.Value);
        return Ok(result);
    }

    [HttpPost("students-monitoring/recalculate")]
    public async Task<IActionResult> Recalculate()
    {
        var result = await _service.RecalculateForTrainerAsync(_currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
