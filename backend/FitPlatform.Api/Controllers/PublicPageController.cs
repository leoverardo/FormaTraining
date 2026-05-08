using FitPlatform.Application.DTOs.PublicPage;
using FitPlatform.Application.DTOs.Testimonials;
using FitPlatform.Application.Interfaces;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

// ─── Public endpoint ─────────────────────────────────────────────────────────
[ApiController]
[Route("api/public/trainers")]
[AllowAnonymous]
public class PublicTrainerPageController : ControllerBase
{
    private readonly PublicPageService _service;

    public PublicTrainerPageController(PublicPageService service) => _service = service;

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var result = await _service.GetBySlugAsync(slug);
        return result.Success ? Ok(result) : NotFound(result);
    }
}

// ─── Trainer management ───────────────────────────────────────────────────────
[ApiController]
[Authorize(Roles = "Trainer")]
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
