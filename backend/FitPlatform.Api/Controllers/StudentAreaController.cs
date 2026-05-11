using FitPlatform.Application.Common;
using FitPlatform.Application.Interfaces;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

[ApiController]
[Route("api/student")]
[Authorize(Roles = "Student")]
public class StudentAreaController : ControllerBase
{
    private const string ExplorerForbiddenMessage = "Student is not linked to an active trainer.";
    private readonly StudentAreaService _service;
    private readonly ICurrentUserService _currentUser;

    public StudentAreaController(StudentAreaService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    private IActionResult LinkedStudentForbidden() =>
        StatusCode(StatusCodes.Status403Forbidden, ApiResponse.Fail(ExplorerForbiddenMessage));

    [HttpGet("access-status")]
    public async Task<IActionResult> AccessStatus()
    {
        if (!_currentUser.StudentId.HasValue)
            return LinkedStudentForbidden();

        var result = await _service.GetAccessStatusAsync(_currentUser.StudentId!.Value);
        return Ok(result);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        if (!_currentUser.StudentId.HasValue)
            return LinkedStudentForbidden();

        var result = await _service.GetDashboardAsync(_currentUser.StudentId!.Value);
        return result.Success ? Ok(result) : LinkedStudentForbidden();
    }

    [HttpGet("workouts")]
    public async Task<IActionResult> GetWorkouts()
    {
        if (!_currentUser.StudentId.HasValue)
            return LinkedStudentForbidden();

        var result = await _service.GetWorkoutsAsync(_currentUser.StudentId!.Value);
        return result.Success ? Ok(result) : LinkedStudentForbidden();
    }

    [HttpGet("workouts/{id:guid}")]
    public async Task<IActionResult> GetWorkout(Guid id)
    {
        if (!_currentUser.StudentId.HasValue)
            return LinkedStudentForbidden();

        var result = await _service.GetWorkoutByIdAsync(_currentUser.StudentId!.Value, id);
        if (result.Success) return Ok(result);
        if (string.Equals(result.Message, ExplorerForbiddenMessage, StringComparison.Ordinal))
            return LinkedStudentForbidden();
        return NotFound(result);
    }

    [HttpGet("posts")]
    public async Task<IActionResult> GetPosts()
    {
        if (!_currentUser.StudentId.HasValue)
            return LinkedStudentForbidden();

        var result = await _service.GetPostsAsync(_currentUser.StudentId!.Value);
        return result.Success ? Ok(result) : LinkedStudentForbidden();
    }

    [HttpGet("posts/{id:guid}")]
    public async Task<IActionResult> GetPost(Guid id)
    {
        if (!_currentUser.StudentId.HasValue)
            return LinkedStudentForbidden();

        var result = await _service.GetPostByIdAsync(_currentUser.StudentId!.Value, id);
        if (result.Success) return Ok(result);
        if (string.Equals(result.Message, ExplorerForbiddenMessage, StringComparison.Ordinal))
            return LinkedStudentForbidden();
        return NotFound(result);
    }
}
