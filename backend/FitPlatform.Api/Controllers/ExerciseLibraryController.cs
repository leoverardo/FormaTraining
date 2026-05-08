using FitPlatform.Application.DTOs.Library;
using FitPlatform.Application.Interfaces;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

[ApiController]
[Route("api/exercise-library")]
public class ExerciseLibraryController : ControllerBase
{
    private readonly ExerciseLibraryService _service;
    private readonly ICurrentUserService _currentUser;

    public ExerciseLibraryController(ExerciseLibraryService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpPost]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Create([FromBody] ExerciseLibraryRequest request)
    {
        var result = await _service.CreateAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ExerciseLibraryRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{id:guid}/duplicate-to-my-library")]
    [Authorize(Roles = "Trainer")]
    public async Task<IActionResult> Duplicate(Guid id)
    {
        var result = await _service.DuplicateToTrainerLibraryAsync(id, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

[ApiController]
[Route("api/workout-templates")]
public class WorkoutTemplatesController : ControllerBase
{
    private readonly ExerciseLibraryService _service;
    private readonly ICurrentUserService _currentUser;

    public WorkoutTemplatesController(ExerciseLibraryService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllTemplatesAsync());

    [HttpPost]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Create([FromBody] WorkoutTemplateRequest request)
    {
        var result = await _service.CreateTemplateAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }



    [HttpPost("{id:guid}/duplicate-to-my-workouts")]
    [Authorize(Roles = "Trainer")]
    public async Task<IActionResult> Duplicate(Guid id)
    {
        var result = await _service.DuplicateTemplateToWorkoutsAsync(id, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
