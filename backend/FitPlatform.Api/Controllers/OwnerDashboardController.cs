using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

[ApiController]
[Route("api/owner")]
[Authorize(Roles = "Owner")]
public class OwnerDashboardController : ControllerBase
{
    private readonly OwnerDashboardService _service;

    public OwnerDashboardController(OwnerDashboardService service) => _service = service;

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var result = await _service.GetDashboardAsync();
        return Ok(result);
    }
}
