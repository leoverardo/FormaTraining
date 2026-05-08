using FitPlatform.Application.Interfaces;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

[ApiController]
[Route("api/trainer/reports")]
[Authorize(Roles = "Trainer")]
public class ReportsController : ControllerBase
{
    private readonly ReportsService _service;
    private readonly ICurrentUserService _currentUser;

    public ReportsController(ReportsService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview()
    {
        var result = await _service.GetOverviewAsync(_currentUser.TrainerId!.Value, _currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
