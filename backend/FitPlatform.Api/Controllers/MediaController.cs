using FitPlatform.Application.Interfaces;
using FitPlatform.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitPlatform.Api.Controllers;

[ApiController]
[Route("api/media")]
[Authorize]
public class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;
    private readonly ICurrentUserService _currentUser;

    public MediaController(IMediaService mediaService, ICurrentUserService currentUser)
    {
        _mediaService = mediaService;
        _currentUser = currentUser;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(200_000_000)]
    public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] MediaCategory category, [FromForm] Guid? studentId = null, [FromForm] bool isPublic = false, CancellationToken cancellationToken = default)
    {
        var result = await _mediaService.UploadMediaAsync(file, category, _currentUser.UserId, _currentUser.TrainerId, studentId, isPublic, _currentUser.Role, _currentUser.StudentId, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("upload/profile-photo")]
    public Task<IActionResult> UploadProfilePhoto([FromForm] IFormFile file, CancellationToken cancellationToken = default)
        => Upload(file, MediaCategory.TrainerProfilePhoto, null, false, cancellationToken);

    [HttpPost("upload/logo")]
    public Task<IActionResult> UploadLogo([FromForm] IFormFile file, CancellationToken cancellationToken = default)
        => Upload(file, MediaCategory.TrainerLogo, null, false, cancellationToken);

    [HttpPost("upload/banner")]
    public Task<IActionResult> UploadBanner([FromForm] IFormFile file, [FromForm] bool isPublic = false, CancellationToken cancellationToken = default)
        => Upload(file, MediaCategory.TrainerBanner, null, isPublic, cancellationToken);

    [HttpPost("upload/exercise-image")]
    public Task<IActionResult> UploadExerciseImage([FromForm] IFormFile file, CancellationToken cancellationToken = default)
        => Upload(file, MediaCategory.ExerciseImage, null, false, cancellationToken);

    [HttpPost("upload/exercise-video")]
    public Task<IActionResult> UploadExerciseVideo([FromForm] IFormFile file, CancellationToken cancellationToken = default)
        => Upload(file, MediaCategory.ExerciseVideo, null, false, cancellationToken);

    [HttpPost("upload/post-cover")]
    public Task<IActionResult> UploadPostCover([FromForm] IFormFile file, [FromForm] bool isPublic = false, CancellationToken cancellationToken = default)
        => Upload(file, MediaCategory.PostCoverImage, null, isPublic, cancellationToken);

    [HttpPost("upload/progress-photo")]
    public Task<IActionResult> UploadProgressPhoto([FromForm] IFormFile file, [FromForm] Guid? studentId = null, CancellationToken cancellationToken = default)
        => Upload(file, MediaCategory.ProgressPhoto, studentId, false, cancellationToken);

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediaService.DeleteMediaAsync(id, _currentUser.UserId, _currentUser.Role, _currentUser.TrainerId, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
