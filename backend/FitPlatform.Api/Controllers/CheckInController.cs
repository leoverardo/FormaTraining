using FitPlatform.Application.DTOs.CheckIn;
using FitPlatform.Api.Authorization;
using FitPlatform.Application.Interfaces;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

// ─── Trainer routes ──────────────────────────────────────────────────────────
[ApiController]
[Authorize(Roles = "Trainer")]
[RequireActiveTrainerSubscription]
public class CheckInTrainerController : ControllerBase
{
    private readonly CheckInService _service;
    private readonly ICurrentUserService _currentUser;

    public CheckInTrainerController(CheckInService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("api/students/{studentId:guid}/check-ins")]
    public async Task<IActionResult> GetByStudent(Guid studentId)
    {
        var result = await _service.GetByStudentAsync(studentId, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("api/trainer/check-ins/recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int limit = 10)
    {
        var result = await _service.GetRecentForTrainerAsync(_currentUser.TrainerId!.Value, limit);
        return Ok(result);
    }

    [HttpGet("api/trainer/check-ins/missing-current-week")]
    public async Task<IActionResult> GetMissingCurrentWeek()
    {
        var result = await _service.GetMissingCurrentWeekAsync(_currentUser.TrainerId!.Value);
        return Ok(result);
    }

    // AddCommentAsync signature: (checkInId, trainerId, studentId, comment)
    [HttpPost("api/students/{studentId:guid}/check-ins/{checkInId:guid}/comments")]
    public async Task<IActionResult> AddComment(Guid studentId, Guid checkInId, [FromBody] CommentRequest request)
    {
        var result = await _service.AddCommentAsync(checkInId, _currentUser.TrainerId!.Value, studentId, request.Comment);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

// ─── Student routes ───────────────────────────────────────────────────────────
[ApiController]
[Route("api/student/check-ins")]
[Authorize(Roles = "Student")]
public class CheckInStudentController : ControllerBase
{
    private readonly CheckInService _service;
    private readonly ICurrentUserService _currentUser;

    public CheckInStudentController(CheckInService service, ICurrentUserService currentUser)
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

    [HttpGet("current-week")]
    public async Task<IActionResult> GetCurrentWeek()
    {
        var result = await _service.GetOwnCurrentWeekAsync(_currentUser.StudentId!.Value);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CheckInRequest request)
    {
        var result = await _service.CreateOwnAsync(_currentUser.StudentId!.Value, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CheckInRequest request)
    {
        var result = await _service.UpdateOwnAsync(id, _currentUser.StudentId!.Value, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
