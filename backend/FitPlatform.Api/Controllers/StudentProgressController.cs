using FitPlatform.Application.DTOs.Progress;
using FitPlatform.Application.Interfaces;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

// ─── Trainer routes ────────────────────────────────────────────────────────

[ApiController]
[Authorize(Roles = "Trainer")]
public class StudentProgressTrainerController : ControllerBase
{
    private readonly StudentProgressService _service;
    private readonly ICurrentUserService _currentUser;

    public StudentProgressTrainerController(StudentProgressService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("api/students/{studentId:guid}/progress")]
    public async Task<IActionResult> GetProgress(Guid studentId)
    {
        var result = await _service.GetProgressByStudentAsync(studentId, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("api/students/{studentId:guid}/progress")]
    public async Task<IActionResult> CreateProgress(Guid studentId, [FromBody] StudentProgressRequest request)
    {
        var result = await _service.CreateProgressForStudentAsync(studentId, request, _currentUser.TrainerId!.Value, _currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("api/students/{studentId:guid}/progress/{progressId:guid}")]
    public async Task<IActionResult> UpdateProgress(Guid studentId, Guid progressId, [FromBody] StudentProgressRequest request)
    {
        var result = await _service.UpdateProgressAsync(studentId, progressId, request, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("api/students/{studentId:guid}/progress/{progressId:guid}")]
    public async Task<IActionResult> DeleteProgress(Guid studentId, Guid progressId)
    {
        var result = await _service.DeleteProgressAsync(studentId, progressId, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("api/students/{studentId:guid}/progress-photos")]
    public async Task<IActionResult> GetPhotos(Guid studentId)
    {
        var result = await _service.GetPhotosByStudentAsync(studentId, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("api/students/{studentId:guid}/progress-photos")]
    public async Task<IActionResult> AddPhoto(Guid studentId, [FromBody] StudentProgressPhotoRequest request)
    {
        var result = await _service.AddPhotoForStudentAsync(studentId, request, _currentUser.TrainerId!.Value, _currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("api/students/{studentId:guid}/progress-photos/{photoId:guid}")]
    public async Task<IActionResult> DeletePhoto(Guid studentId, Guid photoId)
    {
        var result = await _service.DeletePhotoForStudentAsync(studentId, photoId, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("api/students/{studentId:guid}/progress/summary")]
    public async Task<IActionResult> GetProgressSummary(Guid studentId)
    {
        var result = await _service.GetProgressSummaryAsync(studentId, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("api/students/{studentId:guid}/progress-photos/compare")]
    public async Task<IActionResult> GetPhotoCompare(Guid studentId)
    {
        var result = await _service.GetPhotoCompareAsync(studentId, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }
}

// ─── Student routes ────────────────────────────────────────────────────────

[ApiController]
[Route("api/student")]
[Authorize(Roles = "Student")]
public class StudentProgressStudentController : ControllerBase
{
    private readonly StudentProgressService _service;
    private readonly ICurrentUserService _currentUser;

    public StudentProgressStudentController(StudentProgressService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("progress")]
    public async Task<IActionResult> GetProgress()
    {
        var result = await _service.GetOwnProgressAsync(_currentUser.StudentId!.Value);
        return Ok(result);
    }

    [HttpPost("progress")]
    public async Task<IActionResult> CreateProgress([FromBody] StudentProgressRequest request)
    {
        var result = await _service.CreateOwnProgressAsync(_currentUser.StudentId!.Value, request, _currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("progress/{id:guid}")]
    public async Task<IActionResult> UpdateProgress(Guid id, [FromBody] StudentProgressRequest request)
    {
        var result = await _service.UpdateOwnProgressAsync(_currentUser.StudentId!.Value, id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("progress/{id:guid}")]
    public async Task<IActionResult> DeleteProgress(Guid id)
    {
        var result = await _service.DeleteOwnProgressAsync(_currentUser.StudentId!.Value, id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("progress/summary")]
    public async Task<IActionResult> GetProgressSummary()
    {
        var result = await _service.GetProgressSummaryAsync(_currentUser.StudentId!.Value);
        return Ok(result);
    }

    [HttpGet("progress-photos")]
    public async Task<IActionResult> GetPhotos()
    {
        var result = await _service.GetOwnPhotosAsync(_currentUser.StudentId!.Value);
        return Ok(result);
    }

    [HttpPost("progress-photos")]
    public async Task<IActionResult> AddPhoto([FromBody] StudentProgressPhotoRequest request)
    {
        var result = await _service.AddOwnPhotoAsync(_currentUser.StudentId!.Value, request, _currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("progress-photos/{id:guid}")]
    public async Task<IActionResult> DeletePhoto(Guid id)
    {
        var result = await _service.DeleteOwnPhotoAsync(_currentUser.StudentId!.Value, id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("progress-photos/compare")]
    public async Task<IActionResult> GetPhotoCompare()
    {
        var result = await _service.GetPhotoCompareAsync(_currentUser.StudentId!.Value);
        return Ok(result);
    }
}
