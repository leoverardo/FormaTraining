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
    private readonly ServiceSalesService _serviceSales;

    public OwnerDashboardController(OwnerDashboardService service, ServiceSalesService serviceSales)
    {
        _service = service;
        _serviceSales = serviceSales;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard([FromQuery] int range = 30)
    {
        var result = await _service.GetDashboardAsync(range);
        return Ok(result);
    }

    [HttpGet("service-sales/summary")]
    public async Task<IActionResult> ServiceSalesSummary()
    {
        var result = await _serviceSales.GetOwnerSummaryAsync();
        return Ok(result);
    }
}
