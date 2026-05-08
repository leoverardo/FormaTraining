using FitPlatform.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace FitPlatform.Application.Interfaces;

public class MediaUploadOptions
{
    public Guid OwnerUserId { get; set; }
    public Guid? TrainerId { get; set; }
    public Guid? StudentId { get; set; }
    public MediaCategory Category { get; set; }
    public MediaType MediaType { get; set; }
    public bool IsPublic { get; set; }
}

public class MediaUploadResult
{
    public string Url { get; set; } = string.Empty;
    public string? SecureUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string ProviderKey { get; set; } = string.Empty;
    public string? PublicId { get; set; }
    public string? Folder { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }
    public StorageProvider Provider { get; set; }
}

public interface IStorageService
{
    Task<MediaUploadResult> UploadAsync(IFormFile file, MediaUploadOptions options, CancellationToken cancellationToken);
    Task DeleteAsync(string providerKey, CancellationToken cancellationToken);
}
