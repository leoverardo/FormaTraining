using FitPlatform.Application.DTOs.Schedule;
using FitPlatform.Application.Interfaces;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

[ApiController]
[Authorize(Roles = "Trainer")]
public class ScheduleController : ControllerBase
{
    private readonly ScheduleService _service;
    private readonly ICurrentUserService _currentUser;

    public ScheduleController(ScheduleService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("api/students/{studentId:guid}/schedule")]
    public async Task<IActionResult> GetByStudent(Guid studentId)
    {
        var result = await _service.GetByStudentAsync(studentId, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("api/students/{studentId:guid}/schedule")]
    public async Task<IActionResult> Create(Guid studentId, [FromBody] ScheduleRequest request)
    {
        var result = await _service.CreateAsync(studentId, request, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("api/schedule/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ScheduleRequest request)
    {
        var result = await _service.UpdateAsync(id, request, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("api/schedule/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
