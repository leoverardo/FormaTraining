using FitPlatform.Application.DTOs.Habits;
using FitPlatform.Application.Interfaces;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

[ApiController]
[Route("api/trainer/students/{studentId:guid}")]
[Authorize(Roles = "Trainer")]
public class TrainerHabitsController : ControllerBase
{
    private readonly HabitService _service;
    private readonly ICurrentUserService _currentUser;

    public TrainerHabitsController(HabitService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("habits")]
    public async Task<IActionResult> GetHabits(Guid studentId)
    {
        var result = await _service.GetTrainerHabitsAsync(_currentUser.TrainerId!.Value, studentId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("habits")]
    public async Task<IActionResult> CreateHabit(Guid studentId, [FromBody] StudentHabitRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var result = await _service.CreateHabitAsync(_currentUser.TrainerId!.Value, studentId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("habits/{habitId:guid}")]
    public async Task<IActionResult> UpdateHabit(Guid studentId, Guid habitId, [FromBody] StudentHabitRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var result = await _service.UpdateHabitAsync(_currentUser.TrainerId!.Value, studentId, habitId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("habits/{habitId:guid}/status")]
    public async Task<IActionResult> UpdateHabitStatus(Guid studentId, Guid habitId, [FromBody] StudentHabitStatusRequest request)
    {
        var result = await _service.UpdateHabitStatusAsync(_currentUser.TrainerId!.Value, studentId, habitId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("habits/{habitId:guid}")]
    public async Task<IActionResult> DeleteHabit(Guid studentId, Guid habitId)
    {
        var result = await _service.DeleteHabitAsync(_currentUser.TrainerId!.Value, studentId, habitId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("habits/adherence")]
    public async Task<IActionResult> GetAdherence(Guid studentId, [FromQuery] int days = 7)
    {
        var result = await _service.GetAdherenceAsync(_currentUser.TrainerId!.Value, studentId, days);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("nutrition-guidance")]
    public async Task<IActionResult> GetGuidance(Guid studentId)
    {
        var result = await _service.GetTrainerGuidanceAsync(_currentUser.TrainerId!.Value, studentId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("nutrition-guidance")]
    public async Task<IActionResult> UpsertGuidance(Guid studentId, [FromBody] StudentNutritionGuidanceRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var result = await _service.UpsertTrainerGuidanceAsync(_currentUser.TrainerId!.Value, studentId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

[ApiController]
[Route("api/student")]
[Authorize(Roles = "Student")]
public class StudentHabitsController : ControllerBase
{
    private readonly HabitService _service;
    private readonly ICurrentUserService _currentUser;

    public StudentHabitsController(HabitService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("habits/today")]
    public async Task<IActionResult> GetToday()
    {
        var result = await _service.GetStudentTodayAsync(_currentUser.StudentId!.Value);
        return Ok(result);
    }

    [HttpPatch("habits/{habitId:guid}/today")]
    public async Task<IActionResult> UpdateToday(Guid habitId, [FromBody] StudentHabitTodayUpdateRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var result = await _service.UpsertStudentTodayAsync(_currentUser.StudentId!.Value, habitId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("nutrition-guidance")]
    public async Task<IActionResult> GetGuidance()
    {
        var result = await _service.GetStudentGuidanceAsync(_currentUser.StudentId!.Value);
        return Ok(result);
    }
}
