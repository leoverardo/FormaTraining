using FitPlatform.Application.DTOs.ServiceSales;
using FitPlatform.Api.Authorization;
using FitPlatform.Application.Interfaces;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

[ApiController]
[Route("api/trainer")]
[Authorize(Roles = "Trainer")]
[RequireActiveTrainerSubscription]
public class TrainerServiceSalesController : ControllerBase
{
    private readonly ServiceSalesService _service;
    private readonly ICurrentUserService _currentUser;

    public TrainerServiceSalesController(ServiceSalesService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("service-offers")]
    public async Task<IActionResult> GetOffers() => Ok(await _service.GetTrainerOffersAsync(_currentUser.TrainerId!.Value));

    [HttpGet("service-offers/{offerId:guid}")]
    public async Task<IActionResult> GetOfferById(Guid offerId)
    {
        var result = await _service.GetTrainerOfferByIdAsync(_currentUser.TrainerId!.Value, offerId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("service-offers")]
    public async Task<IActionResult> CreateOffer([FromBody] TrainerServiceOfferRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var result = await _service.CreateOfferAsync(_currentUser.TrainerId!.Value, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("service-offers/{offerId:guid}")]
    public async Task<IActionResult> UpdateOffer(Guid offerId, [FromBody] TrainerServiceOfferRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var result = await _service.UpdateOfferAsync(_currentUser.TrainerId!.Value, offerId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("service-offers/{offerId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid offerId, [FromBody] TrainerServiceOfferStatusRequest request)
    {
        var result = await _service.UpdateOfferStatusAsync(_currentUser.TrainerId!.Value, offerId, request.IsActive);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("service-offers/{offerId:guid}/visibility")]
    public async Task<IActionResult> UpdateVisibility(Guid offerId, [FromBody] TrainerServiceOfferVisibilityRequest request)
    {
        var result = await _service.UpdateOfferVisibilityAsync(_currentUser.TrainerId!.Value, offerId, request.IsPublic);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("service-orders")]
    public async Task<IActionResult> GetOrders([FromQuery] TrainerServiceOrderQuery query) => Ok(await _service.GetTrainerOrdersAsync(_currentUser.TrainerId!.Value, query));

    [HttpGet("service-orders/{orderId:guid}")]
    public async Task<IActionResult> GetOrderById(Guid orderId)
    {
        var result = await _service.GetTrainerOrderByIdAsync(_currentUser.TrainerId!.Value, orderId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("service-orders-summary")]
    public async Task<IActionResult> GetOrdersSummary() => Ok(await _service.GetTrainerOrdersSummaryAsync(_currentUser.TrainerId!.Value));
}

[ApiController]
[Route("api/public/trainers")]
[AllowAnonymous]
public class PublicServiceSalesController : ControllerBase
{
    private readonly ServiceSalesService _service;

    public PublicServiceSalesController(ServiceSalesService service) => _service = service;

    [HttpGet("{slugOrId}/service-offers")]
    public async Task<IActionResult> GetPublicOffers(string slugOrId)
    {
        var result = await _service.GetPublicOffersAsync(slugOrId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{slugOrId}/service-orders")]
    public async Task<IActionResult> CreatePublicOrder(string slugOrId, [FromQuery] Guid offerId, [FromBody] CreateServiceOrderPublicRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var result = await _service.CreatePublicOrderAsync(slugOrId, offerId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

[ApiController]
[Route("api/student/trainers/{trainerId:guid}/service-orders")]
[Authorize(Roles = "Student")]
public class StudentServiceSalesController : ControllerBase
{
    private readonly ServiceSalesService _service;
    private readonly ICurrentUserService _currentUser;

    public StudentServiceSalesController(ServiceSalesService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid trainerId, [FromBody] CreateServiceOrderStudentRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var result = await _service.CreateStudentOrderAsync(trainerId, _currentUser.StudentId!.Value, request.ServiceOfferId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
