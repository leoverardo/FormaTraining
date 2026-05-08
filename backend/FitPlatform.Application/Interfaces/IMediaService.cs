using FitPlatform.Application.Common;
using FitPlatform.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace FitPlatform.Application.Interfaces;

public class MediaAssetDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? SecureUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string MediaType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }
    public string Provider { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public interface IMediaService
{
    Task<ApiResponse<MediaAssetDto>> UploadMediaAsync(IFormFile file, MediaCategory category, Guid ownerUserId, Guid? trainerId, Guid? studentId, bool isPublic, string role, Guid? requesterStudentId, CancellationToken cancellationToken);
    Task<ApiResponse> DeleteMediaAsync(Guid mediaId, Guid requestingUserId, string role, Guid? requestingTrainerId, CancellationToken cancellationToken);
    Task<ApiResponse> ValidateMediaAccessAsync(Guid mediaId, Guid requestingUserId, string role, Guid? requestingTrainerId, Guid? requestingStudentId, CancellationToken cancellationToken);
}
