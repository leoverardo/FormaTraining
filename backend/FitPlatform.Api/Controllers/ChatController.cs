using System;
using FitPlatform.Application.DTOs.Chat;
using FitPlatform.Api.Authorization;
using FitPlatform.Application.Interfaces;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize(Roles = "Trainer,Student")]
[RequireActiveTrainerSubscription]
public class ChatController : ControllerBase
{
    private readonly ChatService _service;
    private readonly ICurrentUserService _currentUser;

    public ChatController(ChatService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        if (_currentUser.Role == "Trainer")
        {
            var trainerId = _currentUser.TrainerId;
            if (!trainerId.HasValue) return Unauthorized();
            var result = await _service.GetConversationsForTrainerAsync(trainerId.Value, _currentUser.UserId);
            return Ok(result);
        }

        if (_currentUser.Role == "Student")
        {
            var studentId = _currentUser.StudentId;
            if (!studentId.HasValue) return Unauthorized();
            var result = await _service.GetConversationsForStudentAsync(studentId.Value, _currentUser.UserId);
            return Ok(result);
        }

        return Unauthorized();
    }

    [HttpGet("conversations/{conversationId:guid}")]
    public async Task<IActionResult> GetConversation(Guid conversationId)
    {
        var result = await _service.GetConversationAsync(conversationId, _currentUser.UserId, _currentUser.Role);
        if (result.Success) return Ok(result);
        if (string.Equals(result.Message, "Conversa não encontrada.", StringComparison.OrdinalIgnoreCase)) return NotFound(result);
        if (string.Equals(result.Message, "Acesso negado.", StringComparison.OrdinalIgnoreCase)) return Forbid();
        return BadRequest(result);
    }

    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage([FromBody] SendChatMessageRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var result = await _service.SendMessageAsync(_currentUser.UserId, _currentUser.Role, request);
        if (result.Success) return Ok(result);
        if (string.Equals(result.Message, "Acesso negado.", StringComparison.OrdinalIgnoreCase)) return Forbid();
        if (string.Equals(result.Message, "Conversa não encontrada.", StringComparison.OrdinalIgnoreCase)) return NotFound(result);
        return BadRequest(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var result = await _service.GetUnreadCountAsync(_currentUser.UserId, _currentUser.Role);
        return Ok(result);
    }
}
