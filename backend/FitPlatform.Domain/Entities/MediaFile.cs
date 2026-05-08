using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class MediaFile : BaseEntity
{
    public Guid OwnerUserId { get; set; }
    public Guid? TrainerId { get; set; }
    public Guid? StudentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? SecureUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? ProviderKey { get; set; }
    public string? PublicId { get; set; }
    public string? Folder { get; set; }
    public MediaCategory Category { get; set; }
    public MediaType MediaType { get; set; } = MediaType.Image;
    public StorageProvider Provider { get; set; } = StorageProvider.Local;
    public bool IsPublic { get; set; } = false;

    public User OwnerUser { get; set; } = null!;
}
