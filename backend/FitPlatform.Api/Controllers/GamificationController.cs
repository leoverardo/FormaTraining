using FitPlatform.Application.DTOs.Gamification;
using FitPlatform.Api.Authorization;
using FitPlatform.Application.Interfaces;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

[ApiController]
[Route("api/student/gamification")]
[Authorize(Roles = "Student")]
public class StudentGamificationController : ControllerBase
{
    private readonly GamificationService _service;
    private readonly ICurrentUserService _currentUser;

    public StudentGamificationController(GamificationService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary()
    {
        var result = await _service.GetStudentSummaryAsync(_currentUser.StudentId!.Value);
        return Ok(result);
    }

    [HttpGet("achievements")]
    public async Task<IActionResult> Achievements()
    {
        var result = await _service.GetStudentAchievementsAsync(_currentUser.StudentId!.Value);
        return Ok(result);
    }

    [HttpGet("monthly-goals")]
    public async Task<IActionResult> MonthlyGoals([FromQuery] int? year, [FromQuery] int? month)
    {
        var result = await _service.GetMonthlyGoalsAsync(_currentUser.StudentId!.Value, year, month);
        return Ok(result);
    }
}

[ApiController]
[Route("api/trainer/students/{studentId:guid}/gamification")]
[Authorize(Roles = "Trainer")]
[RequireActiveTrainerSubscription]
public class TrainerGamificationController : ControllerBase
{
    private readonly GamificationService _service;
    private readonly ICurrentUserService _currentUser;

    public TrainerGamificationController(GamificationService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary(Guid studentId)
    {
        var result = await _service.GetTrainerStudentSummaryAsync(_currentUser.TrainerId!.Value, studentId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("achievements")]
    public async Task<IActionResult> Achievements(Guid studentId)
    {
        var result = await _service.GetTrainerStudentAchievementsAsync(_currentUser.TrainerId!.Value, studentId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("monthly-goals")]
    public async Task<IActionResult> MonthlyGoals(Guid studentId, [FromQuery] int? year, [FromQuery] int? month)
    {
        var result = await _service.GetTrainerStudentMonthlyGoalsAsync(_currentUser.TrainerId!.Value, studentId, year, month);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("monthly-goals/{year:int}/{month:int}")]
    public async Task<IActionResult> UpsertMonthlyGoals(Guid studentId, int year, int month, [FromBody] StudentMonthlyGoalRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var result = await _service.UpsertMonthlyGoalsAsync(_currentUser.TrainerId!.Value, studentId, year, month, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
