using FitPlatform.Application.Common;
using FitPlatform.Application.Interfaces;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FitPlatform.Infrastructure.Services;

public class MediaService : IMediaService
{
    private static readonly HashSet<string> DangerousExtensions = [".exe", ".bat", ".cmd", ".ps1", ".sh", ".js", ".jar", ".msi", ".com", ".scr"];

    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    public MediaService(AppDbContext db, IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _db = db;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    public async Task<ApiResponse<MediaAssetDto>> UploadMediaAsync(IFormFile file, MediaCategory category, Guid ownerUserId, Guid? trainerId, Guid? studentId, bool isPublic, string role, Guid? requesterStudentId, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0) return ApiResponse<MediaAssetDto>.Fail("Arquivo vazio ou ausente.");

        if (role == "Student" && category != MediaCategory.ProgressPhoto)
            return ApiResponse<MediaAssetDto>.Fail("Aluno pode enviar apenas foto de progresso.");

        if (role == "Student") studentId = requesterStudentId;
        if (category == MediaCategory.ProgressPhoto) isPublic = false;

        var mediaType = ResolveMediaType(file.ContentType);
        if (mediaType is null) return ApiResponse<MediaAssetDto>.Fail("Content-Type invalido para upload.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (DangerousExtensions.Contains(extension)) return ApiResponse<MediaAssetDto>.Fail("Extensao de arquivo bloqueada por seguranca.");

        var sizeValidation = ValidateSize(file.Length, mediaType.Value);
        if (!sizeValidation.Success) return ApiResponse<MediaAssetDto>.Fail(sizeValidation.Message!);

        var provider = ResolveStorageProvider();
        var storageService = provider == StorageProvider.Cloudinary
            ? _serviceProvider.GetRequiredService<Storage.CloudinaryStorageService>() as IStorageService
            : _serviceProvider.GetRequiredService<Storage.LocalStorageService>();

        var uploadResult = await storageService.UploadAsync(file, new MediaUploadOptions
        {
            OwnerUserId = ownerUserId,
            TrainerId = trainerId,
            StudentId = studentId,
            Category = category,
            MediaType = mediaType.Value,
            IsPublic = isPublic
        }, cancellationToken);

        var media = new MediaFile
        {
            OwnerUserId = ownerUserId,
            TrainerId = trainerId,
            StudentId = studentId,
            FileName = uploadResult.FileName,
            OriginalFileName = Path.GetFileName(file.FileName),
            ContentType = uploadResult.ContentType,
            SizeInBytes = uploadResult.SizeInBytes,
            Url = uploadResult.Url,
            SecureUrl = uploadResult.SecureUrl,
            ThumbnailUrl = uploadResult.ThumbnailUrl,
            Provider = uploadResult.Provider,
            ProviderKey = uploadResult.ProviderKey,
            PublicId = uploadResult.PublicId,
            Folder = uploadResult.Folder,
            MediaType = mediaType.Value,
            Category = category,
            IsPublic = isPublic
        };

        _db.MediaFiles.Add(media);
        await _db.SaveChangesAsync(cancellationToken);

        return ApiResponse<MediaAssetDto>.Ok(ToDto(media));
    }

    public async Task<ApiResponse> DeleteMediaAsync(Guid mediaId, Guid requestingUserId, string role, Guid? requestingTrainerId, CancellationToken cancellationToken)
    {
        var media = await _db.MediaFiles.FirstOrDefaultAsync(x => x.Id == mediaId, cancellationToken);
        if (media is null) return ApiResponse.Fail("Midia nao encontrada.");

        if (!CanDelete(media, requestingUserId, role, requestingTrainerId)) return ApiResponse.Fail("Acesso negado.");

        var storageService = media.Provider == StorageProvider.Cloudinary
            ? _serviceProvider.GetRequiredService<Storage.CloudinaryStorageService>() as IStorageService
            : _serviceProvider.GetRequiredService<Storage.LocalStorageService>();

        await storageService.DeleteAsync(media.ProviderKey ?? media.PublicId ?? string.Empty, cancellationToken);

        _db.MediaFiles.Remove(media);
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Ok("Midia removida com sucesso.");
    }

    public async Task<ApiResponse> ValidateMediaAccessAsync(Guid mediaId, Guid requestingUserId, string role, Guid? requestingTrainerId, Guid? requestingStudentId, CancellationToken cancellationToken)
    {
        var media = await _db.MediaFiles.FirstOrDefaultAsync(x => x.Id == mediaId, cancellationToken);
        if (media is null) return ApiResponse.Fail("Midia nao encontrada.");

        if (!CanAccess(media, requestingUserId, role, requestingTrainerId, requestingStudentId)) return ApiResponse.Fail("Acesso negado.");

        return ApiResponse.Ok();
    }

    private StorageProvider ResolveStorageProvider()
    {
        var provider = _configuration["Storage:Provider"];
        var fallback = _configuration["Storage:FallbackProvider"] ?? "Local";

        if (!string.Equals(provider, "Cloudinary", StringComparison.OrdinalIgnoreCase)) return StorageProvider.Local;

        var configured = !string.IsNullOrWhiteSpace(_configuration["Cloudinary:CloudName"])
            && !string.IsNullOrWhiteSpace(_configuration["Cloudinary:ApiKey"])
            && !string.IsNullOrWhiteSpace(_configuration["Cloudinary:ApiSecret"]);

        if (configured) return StorageProvider.Cloudinary;

        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        if (string.Equals(environmentName, "Development", StringComparison.OrdinalIgnoreCase)
            && string.Equals(fallback, "Local", StringComparison.OrdinalIgnoreCase))
            return StorageProvider.Local;

        throw new InvalidOperationException("Cloudinary configurado como provider principal, mas credenciais ausentes.");
    }

    private MediaType? ResolveMediaType(string contentType)
    {
        var allowedImages = _configuration.GetSection("Storage:AllowedImageTypes").GetChildren().Select(x => x.Value).Where(x => !string.IsNullOrWhiteSpace(x)).Cast<string>().ToArray();
        var allowedVideos = _configuration.GetSection("Storage:AllowedVideoTypes").GetChildren().Select(x => x.Value).Where(x => !string.IsNullOrWhiteSpace(x)).Cast<string>().ToArray();
        if (allowedImages.Length == 0) allowedImages = ["image/jpeg", "image/png", "image/webp"];
        if (allowedVideos.Length == 0) allowedVideos = ["video/mp4", "video/webm", "video/quicktime"];

        if (allowedImages.Contains(contentType, StringComparer.OrdinalIgnoreCase)) return MediaType.Image;
        if (allowedVideos.Contains(contentType, StringComparer.OrdinalIgnoreCase)) return MediaType.Video;
        return null;
    }

    private ApiResponse ValidateSize(long sizeInBytes, MediaType mediaType)
    {
        var maxImageMb = int.TryParse(_configuration["Storage:MaxImageSizeMB"], out var imageSize) ? imageSize : 5;
        var maxVideoMb = int.TryParse(_configuration["Storage:MaxVideoSizeMB"], out var videoSize) ? videoSize : 100;
        var maxBytes = mediaType == MediaType.Image ? maxImageMb * 1024L * 1024L : maxVideoMb * 1024L * 1024L;

        return sizeInBytes > maxBytes
            ? ApiResponse.Fail($"Arquivo excede o limite de {(mediaType == MediaType.Image ? maxImageMb : maxVideoMb)} MB.")
            : ApiResponse.Ok();
    }

    private static bool CanAccess(MediaFile media, Guid userId, string role, Guid? trainerId, Guid? studentId)
    {
        if (role == "Owner") return true;
        if (media.IsPublic) return true;
        if (media.OwnerUserId == userId) return true;
        if (role == "Trainer" && trainerId.HasValue && media.TrainerId == trainerId) return true;
        if (role == "Student" && studentId.HasValue && media.StudentId == studentId) return true;
        return false;
    }

    private static bool CanDelete(MediaFile media, Guid userId, string role, Guid? trainerId)
    {
        if (role == "Owner") return true;
        if (media.OwnerUserId == userId) return true;
        if (role == "Trainer" && trainerId.HasValue && media.TrainerId == trainerId) return true;
        return false;
    }

    private static MediaAssetDto ToDto(MediaFile media) => new()
    {
        Id = media.Id,
        Url = media.Url,
        SecureUrl = media.SecureUrl ?? media.Url,
        ThumbnailUrl = media.ThumbnailUrl,
        MediaType = media.MediaType.ToString(),
        Category = media.Category.ToString(),
        ContentType = media.ContentType,
        SizeInBytes = media.SizeInBytes,
        Provider = media.Provider.ToString(),
        CreatedAt = media.CreatedAt
    };
}
