using FitPlatform.Application.DTOs.PublicPage;
using FitPlatform.Application.DTOs.Testimonials;
using FitPlatform.Application.DTOs.Leads;
using FitPlatform.Api.Authorization;
using FitPlatform.Application.Interfaces;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FitPlatform.Api.Controllers;

// ─── Public endpoint ─────────────────────────────────────────────────────────
[ApiController]
[Route("api/public/trainers")]
[AllowAnonymous]
public class PublicTrainerPageController : ControllerBase
{
    private readonly PublicPageService _service;
    private readonly TrainerLeadService _leadService;
    private readonly ICurrentUserService _currentUser;

    public PublicTrainerPageController(PublicPageService service, TrainerLeadService leadService, ICurrentUserService currentUser)
    {
        _service = service;
        _leadService = leadService;
        _currentUser = currentUser;
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var result = await _service.GetBySlugAsync(slug);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{slug}/lead")]
    [EnableRateLimiting("PublicLead")]
    public async Task<IActionResult> CreateLeadBySlug(string slug, [FromBody] CreateTrainerLeadRequest request)
    {
        var studentProfileId = User.Identity?.IsAuthenticated == true ? _currentUser.StudentProfileId : null;
        var result = await _leadService.CreateBySlugAsync(slug, request, studentProfileId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{trainerId:guid}/lead")]
    [EnableRateLimiting("PublicLead")]
    public async Task<IActionResult> CreateLeadByTrainerId(Guid trainerId, [FromBody] CreateTrainerLeadRequest request)
    {
        var studentProfileId = User.Identity?.IsAuthenticated == true ? _currentUser.StudentProfileId : null;
        var result = await _leadService.CreateByTrainerIdAsync(trainerId, request, studentProfileId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

// ─── Trainer management ───────────────────────────────────────────────────────
[ApiController]
[Authorize(Roles = "Trainer")]
[RequireActiveTrainerSubscription]
public class TrainerPublicPageController : ControllerBase
{
    private readonly PublicPageService _service;
    private readonly ICurrentUserService _currentUser;

    public TrainerPublicPageController(PublicPageService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpPut("api/trainer/public-page")]
    public async Task<IActionResult> UpdatePage([FromBody] PublicPageRequest request)
    {
        var result = await _service.UpdatePageAsync(_currentUser.TrainerId!.Value, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("api/trainer/public-page")]
    public async Task<IActionResult> GetPageSettings()
    {
        var result = await _service.GetTrainerSettingsAsync(_currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("api/testimonials")]
    public async Task<IActionResult> GetTestimonials()
    {
        var result = await _service.GetTestimonialsAsync(_currentUser.TrainerId!.Value);
        return Ok(result);
    }

    [HttpPost("api/students/{studentId:guid}/testimonials")]
    public async Task<IActionResult> CreateTestimonial(Guid studentId, [FromBody] TestimonialRequest request)
    {
        var result = await _service.CreateTestimonialAsync(studentId, request, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("api/testimonials/{id:guid}")]
    public async Task<IActionResult> UpdateTestimonial(Guid id, [FromBody] TestimonialRequest request)
    {
        var result = await _service.UpdateTestimonialAsync(id, request, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("api/transformations")]
    public async Task<IActionResult> GetTransformations()
    {
        var result = await _service.GetTransformationsAsync(_currentUser.TrainerId!.Value);
        return Ok(result);
    }

    [HttpPost("api/students/{studentId:guid}/transformations")]
    public async Task<IActionResult> CreateTransformation(Guid studentId, [FromBody] TransformationRequest request)
    {
        var result = await _service.CreateTransformationAsync(studentId, request, _currentUser.TrainerId!.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("api/trainer/leads")]
    public async Task<IActionResult> GetLeads([FromServices] TrainerLeadService leadService)
    {
        var result = await leadService.GetForTrainerAsync(_currentUser.TrainerId!.Value);
        return Ok(result);
    }

    [HttpPut("api/trainer/leads/{id:guid}/status")]
    public async Task<IActionResult> UpdateLeadStatus(Guid id, [FromBody] UpdateLeadStatusRequest request, [FromServices] TrainerLeadService leadService)
    {
        var result = await leadService.UpdateStatusAsync(_currentUser.TrainerId!.Value, id, request.Status);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("api/trainer/leads/{id:guid}/convert-to-student")]
    public async Task<IActionResult> ConvertLead(Guid id, [FromServices] TrainerLeadService leadService)
    {
        var result = await leadService.ConvertToStudentAsync(_currentUser.TrainerId!.Value, id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

// ─── Student approvals ────────────────────────────────────────────────────────
[ApiController]
[Route("api/student")]
[Authorize(Roles = "Student")]
public class StudentTestimonialsController : ControllerBase
{
    private readonly PublicPageService _service;
    private readonly ICurrentUserService _currentUser;

    public StudentTestimonialsController(PublicPageService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("testimonial-requests")]
    public async Task<IActionResult> GetTestimonials()
    {
        var result = await _service.GetStudentTestimonialsAsync(_currentUser.StudentId!.Value);
        return Ok(result);
    }

    [HttpPut("testimonials/{id:guid}/approve")]
    public async Task<IActionResult> ApproveTestimonial(Guid id)
    {
        var result = await _service.ApproveTestimonialAsync(id, _currentUser.StudentId!.Value, true);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("testimonials/{id:guid}/revoke")]
    public async Task<IActionResult> RevokeTestimonial(Guid id)
    {
        var result = await _service.ApproveTestimonialAsync(id, _currentUser.StudentId!.Value, false);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("transformations/{id:guid}/approve")]
    public async Task<IActionResult> ApproveTransformation(Guid id)
    {
        var result = await _service.ApproveTransformationAsync(id, _currentUser.StudentId!.Value, true);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("transformations/{id:guid}/revoke")]
    public async Task<IActionResult> RevokeTransformation(Guid id)
    {
        var result = await _service.ApproveTransformationAsync(id, _currentUser.StudentId!.Value, false);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
