using FitPlatform.Application.DTOs.Appointments;
using FitPlatform.Api.Authorization;
using FitPlatform.Application.Interfaces;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

[ApiController]
[Route("api/trainer/appointments")]
[Authorize(Roles = "Trainer")]
[RequireActiveTrainerSubscription]
public class TrainerAppointmentsController : ControllerBase
{
    private readonly AppointmentService _service;
    private readonly ICurrentUserService _currentUser;

    public TrainerAppointmentsController(AppointmentService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] AppointmentQuery query)
    {
        var result = await _service.GetTrainerAsync(_currentUser.TrainerId!.Value, query);
        return Ok(result);
    }

    [HttpGet("{appointmentId:guid}")]
    public async Task<IActionResult> GetById(Guid appointmentId)
    {
        var result = await _service.GetTrainerByIdAsync(_currentUser.TrainerId!.Value, appointmentId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AppointmentRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var result = await _service.CreateAsync(_currentUser.TrainerId!.Value, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{appointmentId:guid}")]
    public async Task<IActionResult> Update(Guid appointmentId, [FromBody] AppointmentRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var result = await _service.UpdateAsync(_currentUser.TrainerId!.Value, appointmentId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("{appointmentId:guid}/reschedule")]
    public async Task<IActionResult> Reschedule(Guid appointmentId, [FromBody] AppointmentRescheduleRequest request)
    {
        var result = await _service.RescheduleAsync(_currentUser.TrainerId!.Value, appointmentId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("{appointmentId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid appointmentId, [FromBody] AppointmentCancelRequest request)
    {
        var result = await _service.CancelAsync(_currentUser.TrainerId!.Value, appointmentId, request.Reason);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("{appointmentId:guid}/complete")]
    public async Task<IActionResult> Complete(Guid appointmentId)
    {
        var result = await _service.CompleteAsync(_currentUser.TrainerId!.Value, appointmentId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

[ApiController]
[Route("api/student/appointments")]
[Authorize(Roles = "Student")]
public class StudentAppointmentsController : ControllerBase
{
    private readonly AppointmentService _service;
    private readonly ICurrentUserService _currentUser;

    public StudentAppointmentsController(AppointmentService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] AppointmentQuery query)
    {
        var result = await _service.GetStudentAsync(_currentUser.StudentId!.Value, query);
        return Ok(result);
    }

    [HttpGet("{appointmentId:guid}")]
    public async Task<IActionResult> GetById(Guid appointmentId)
    {
        var result = await _service.GetStudentByIdAsync(_currentUser.StudentId!.Value, appointmentId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPatch("{appointmentId:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid appointmentId)
    {
        var result = await _service.ConfirmByStudentAsync(_currentUser.StudentId!.Value, appointmentId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
