using FitPlatform.Application.DTOs.WorkoutSession;
using FitPlatform.Application.Interfaces;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

// ─── Trainer routes ───────────────────────────────────────────────────────────
[ApiController]
[Authorize(Roles = "Trainer")]
public class WorkoutSessionsTrainerController : ControllerBase
{
    private readonly WorkoutSessionService _service;
    private readonly ICurrentUserService _currentUser;

    public WorkoutSessionsTrainerController(WorkoutSessionService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("api/students/{studentId:guid}/workout-sessions")]
    public async Task<IActionResult> GetByStudent(Guid studentId)
    {
        var result = await _service.GetByStudentAsync(studentId, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }
}

// ─── Student routes ───────────────────────────────────────────────────────────
[ApiController]
[Route("api/student/workout-sessions")]
[Authorize(Roles = "Student")]
public class WorkoutSessionsStudentController : ControllerBase
{
    private readonly WorkoutSessionService _service;
    private readonly ICurrentUserService _currentUser;

    public WorkoutSessionsStudentController(WorkoutSessionService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetOwn()
    {
        var result = await _service.GetOwnAsync(_currentUser.StudentId!.Value);
        return Ok(result);
    }

    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] StartWorkoutSessionRequest request)
    {
        var result = await _service.StartAsync(_currentUser.StudentId!.Value, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteWorkoutSessionRequest request)
    {
        var result = await _service.CompleteAsync(id, _currentUser.StudentId!.Value, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}/skip")]
    public async Task<IActionResult> Skip(Guid id)
    {
        var result = await _service.SkipAsync(id, _currentUser.StudentId!.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
