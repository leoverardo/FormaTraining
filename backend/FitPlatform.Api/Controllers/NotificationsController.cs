using FitPlatform.Application.Interfaces;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly NotificationService _service;
    private readonly ICurrentUserService _currentUser;

    public NotificationsController(NotificationService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetForUserAsync(_currentUser.UserId);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount()
    {
        var result = await _service.GetUnreadCountAsync(_currentUser.UserId);
        return Ok(result);
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var result = await _service.MarkReadAsync(id, _currentUser.UserId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var result = await _service.MarkAllReadAsync(_currentUser.UserId);
        return Ok(result);
    }
}
