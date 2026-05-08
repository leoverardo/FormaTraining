using FitPlatform.Application.DTOs.Feed;
using FitPlatform.Application.Interfaces;
using FitPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

// ── Public feed ───────────────────────────────────────────────────────────────

[ApiController]
[Route("api/public/trainers")]
[AllowAnonymous]
public class PublicFeedController : ControllerBase
{
    private readonly FeedBuilderService _feedBuilder;

    public PublicFeedController(FeedBuilderService feedBuilder) => _feedBuilder = feedBuilder;

    [HttpGet("{slug}/feed")]
    public async Task<IActionResult> GetPublicFeed(string slug, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _feedBuilder.GetPublicFeedAsync(slug, page, pageSize);
        return result.Success ? Ok(result) : NotFound(result);
    }
}

// ── Student feed ──────────────────────────────────────────────────────────────

[ApiController]
[Route("api/student")]
[Authorize(Roles = "Student")]
public class StudentFeedController : ControllerBase
{
    private readonly FeedBuilderService _feedBuilder;
    private readonly ICurrentUserService _currentUser;

    public StudentFeedController(FeedBuilderService feedBuilder, ICurrentUserService currentUser)
    {
        _feedBuilder = feedBuilder;
        _currentUser = currentUser;
    }

    [HttpGet("feed")]
    public async Task<IActionResult> GetFeed([FromQuery] string? type, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _feedBuilder.GetStudentFeedAsync(
            _currentUser.StudentId!.Value, _currentUser.UserId, type, page, pageSize);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

// ── Trainer feed ──────────────────────────────────────────────────────────────

[ApiController]
[Route("api/trainer")]
[Authorize(Roles = "Trainer")]
public class TrainerFeedController : ControllerBase
{
    private readonly FeedBuilderService _feedBuilder;
    private readonly ICurrentUserService _currentUser;

    public TrainerFeedController(FeedBuilderService feedBuilder, ICurrentUserService currentUser)
    {
        _feedBuilder = feedBuilder;
        _currentUser = currentUser;
    }

    [HttpGet("feed/recent-activity")]
    public async Task<IActionResult> GetRecentActivity([FromQuery] int limit = 20)
    {
        var result = await _feedBuilder.GetTrainerRecentActivityAsync(_currentUser.TrainerId!.Value, limit);
        return Ok(result);
    }
}

// ── Social actions (like, save, comment) ─────────────────────────────────────

[ApiController]
[Route("api/feed")]
[Authorize]
public class FeedSocialController : ControllerBase
{
    private readonly FeedSocialService _social;
    private readonly ICurrentUserService _currentUser;

    public FeedSocialController(FeedSocialService social, ICurrentUserService currentUser)
    {
        _social = social;
        _currentUser = currentUser;
    }

    [HttpPost("{feedItemId}/like")]
    public async Task<IActionResult> Like(string feedItemId)
    {
        var result = await _social.AddLikeAsync(feedItemId, _currentUser.UserId, _currentUser.TrainerId, _currentUser.StudentId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{feedItemId}/like")]
    public async Task<IActionResult> Unlike(string feedItemId)
    {
        var result = await _social.RemoveLikeAsync(feedItemId, _currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{feedItemId}/save")]
    public async Task<IActionResult> Save(string feedItemId)
    {
        var result = await _social.SaveItemAsync(feedItemId, _currentUser.UserId, _currentUser.TrainerId, _currentUser.StudentId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{feedItemId}/save")]
    public async Task<IActionResult> Unsave(string feedItemId)
    {
        var result = await _social.UnsaveItemAsync(feedItemId, _currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{feedItemId}/comments")]
    public async Task<IActionResult> GetComments(string feedItemId)
    {
        var result = await _social.GetCommentsAsync(feedItemId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{feedItemId}/comments")]
    public async Task<IActionResult> AddComment(string feedItemId, [FromBody] AddFeedCommentRequest request)
    {
        var result = await _social.AddCommentAsync(
            feedItemId, request.Comment, _currentUser.UserId, _currentUser.TrainerId, _currentUser.StudentId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("comments/{commentId:guid}")]
    public async Task<IActionResult> DeleteComment(Guid commentId)
    {
        var result = await _social.DeleteCommentAsync(commentId, _currentUser.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
