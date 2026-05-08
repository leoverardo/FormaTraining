using FitPlatform.Application.DTOs.Notes;
using FitPlatform.Application.Interfaces;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

[ApiController]
[Authorize(Roles = "Trainer")]
public class InternalNotesController : ControllerBase
{
    private readonly InternalNotesService _service;
    private readonly ICurrentUserService _currentUser;

    public InternalNotesController(InternalNotesService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("api/students/{studentId:guid}/internal-notes")]
    public async Task<IActionResult> GetByStudent(Guid studentId)
    {
        var result = await _service.GetByStudentAsync(studentId, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("api/students/{studentId:guid}/internal-notes")]
    public async Task<IActionResult> Create(Guid studentId, [FromBody] NoteRequest request)
    {
        var result = await _service.CreateAsync(studentId, request, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("api/students/{studentId:guid}/internal-notes/{noteId:guid}")]
    public async Task<IActionResult> Update(Guid studentId, Guid noteId, [FromBody] NoteRequest request)
    {
        var result = await _service.UpdateAsync(noteId, studentId, request, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("api/students/{studentId:guid}/internal-notes/{noteId:guid}")]
    public async Task<IActionResult> Delete(Guid studentId, Guid noteId)
    {
        var result = await _service.DeleteAsync(noteId, studentId, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
