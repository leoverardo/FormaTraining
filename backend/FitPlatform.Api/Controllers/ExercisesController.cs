using FitPlatform.Application.DTOs.Exercises;
using FitPlatform.Api.Authorization;
using FitPlatform.Application.Interfaces;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

[ApiController]
[Route("api/exercises")]
[Authorize(Roles = "Trainer")]
[RequireActiveTrainerSubscription]
public class ExercisesController : ControllerBase
{
    private readonly ExerciseService _service;
    private readonly ICurrentUserService _currentUser;

    public ExercisesController(ExerciseService service, ICurrentUserService currentUser)
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
    public async Task<IActionResult> Create([FromBody] ExerciseRequest request)
    {
        var result = await _service.CreateAsync(request, _currentUser.TrainerId!.Value);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ExerciseRequest request)
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
}
