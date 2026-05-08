using FitPlatform.Application.DTOs.Anamnesis;
using FitPlatform.Application.Interfaces;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

[ApiController]
public class AnamnesisController : ControllerBase
{
    private readonly AnamnesisService _service;
    private readonly ICurrentUserService _currentUser;

    public AnamnesisController(AnamnesisService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("api/students/{studentId:guid}/anamnesis")]
    [Authorize(Roles = "Trainer")]
    public async Task<IActionResult> GetByStudent(Guid studentId)
    {
        var result = await _service.GetByStudentAsync(studentId, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("api/student/anamnesis")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetOwn()
    {
        var result = await _service.GetOwnAsync(_currentUser.StudentId!.Value);
        return Ok(result);
    }

    [HttpPost("api/student/anamnesis")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Save([FromBody] AnamnesisRequest request)
    {
        var result = await _service.SaveOwnAsync(_currentUser.StudentId!.Value, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
