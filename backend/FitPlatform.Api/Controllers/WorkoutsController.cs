using FitPlatform.Application.DTOs.Workouts;
using FitPlatform.Api.Authorization;
using FitPlatform.Application.Interfaces;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

[ApiController]
[Route("api/workouts")]
[Authorize(Roles = "Trainer")]
[RequireActiveTrainerSubscription]
public class WorkoutsController : ControllerBase
{
    private readonly WorkoutService _service;
    private readonly ICurrentUserService _currentUser;

    public WorkoutsController(WorkoutService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync(_currentUser.TrainerId!.Value);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] WorkoutRequest request)
    {
        var result = await _service.CreateAsync(request, _currentUser.TrainerId!.Value);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] WorkoutRequest request)
    {
        var result = await _service.UpdateAsync(id, request, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{id:guid}/exercises")]
    public async Task<IActionResult> AddExercise(Guid id, [FromBody] WorkoutExerciseRequest request)
    {
        var result = await _service.AddExerciseAsync(id, request, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}/exercises/{workoutExerciseId:guid}")]
    public async Task<IActionResult> UpdateExercise(Guid id, Guid workoutExerciseId, [FromBody] WorkoutExerciseRequest request)
    {
        var result = await _service.UpdateExerciseAsync(id, workoutExerciseId, request, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}/exercises/{workoutExerciseId:guid}")]
    public async Task<IActionResult> RemoveExercise(Guid id, Guid workoutExerciseId)
    {
        var result = await _service.RemoveExerciseAsync(id, workoutExerciseId, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
