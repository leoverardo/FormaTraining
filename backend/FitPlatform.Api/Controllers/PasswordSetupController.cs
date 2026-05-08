using FitPlatform.Application.DTOs.Auth;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class PasswordSetupController : ControllerBase
{
    private readonly PasswordSetupService _service;

    public PasswordSetupController(PasswordSetupService service) => _service = service;

    [HttpPost("set-password")]
    public async Task<IActionResult> SetPassword([FromBody] SetPasswordRequest request)
    {
        var result = await _service.SetPasswordAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("request-password-reset")]
    public async Task<IActionResult> RequestReset([FromBody] RequestPasswordResetRequest request)
    {
        var result = await _service.RequestPasswordResetAsync(request);
        return Ok(result);
    }
}
