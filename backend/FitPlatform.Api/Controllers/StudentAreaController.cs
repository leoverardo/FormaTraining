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
    private readonly StudentAreaService _service;
    private readonly ICurrentUserService _currentUser;

    public StudentAreaController(StudentAreaService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("access-status")]
    public async Task<IActionResult> AccessStatus()
    {
        var result = await _service.GetAccessStatusAsync(_currentUser.StudentId!.Value);
        return Ok(result);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var result = await _service.GetDashboardAsync(_currentUser.StudentId!.Value);
        return result.Success ? Ok(result) : Forbid();
    }

    [HttpGet("workouts")]
    public async Task<IActionResult> GetWorkouts()
    {
        var result = await _service.GetWorkoutsAsync(_currentUser.StudentId!.Value);
        return result.Success ? Ok(result) : Forbid();
    }

    [HttpGet("workouts/{id:guid}")]
    public async Task<IActionResult> GetWorkout(Guid id)
    {
        var result = await _service.GetWorkoutByIdAsync(_currentUser.StudentId!.Value, id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("posts")]
    public async Task<IActionResult> GetPosts()
    {
        var result = await _service.GetPostsAsync(_currentUser.StudentId!.Value);
        return result.Success ? Ok(result) : Forbid();
    }

    [HttpGet("posts/{id:guid}")]
    public async Task<IActionResult> GetPost(Guid id)
    {
        var result = await _service.GetPostByIdAsync(_currentUser.StudentId!.Value, id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
