using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FitPlatform.Application.Interfaces;
using FitPlatform.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace FitPlatform.Infrastructure.Storage;

public class CloudinaryStorageService : IStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly string _rootFolder;

    public CloudinaryStorageService(IConfiguration config)
    {
        var cloudName = config["Cloudinary:CloudName"] ?? string.Empty;
        var apiKey = config["Cloudinary:ApiKey"] ?? string.Empty;
        var apiSecret = config["Cloudinary:ApiSecret"] ?? string.Empty;

        _cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
        _rootFolder = config["Cloudinary:Folder"] ?? "fitplatform";
    }

    public async Task<MediaUploadResult> UploadAsync(IFormFile file, MediaUploadOptions options, CancellationToken cancellationToken)
    {
        var folder = BuildFolder(options);
        await using var stream = file.OpenReadStream();

        if (options.MediaType == MediaType.Image)
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                UseFilename = false,
                UniqueFilename = true,
                Overwrite = false
            };

            var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
            if (result.Error is not null)
            {
                throw new InvalidOperationException($"Cloudinary upload failed: {result.Error.Message}");
            }

            return new MediaUploadResult
            {
                Url = result.Url?.ToString() ?? string.Empty,
                SecureUrl = result.SecureUrl?.ToString(),
                ProviderKey = result.PublicId,
                PublicId = result.PublicId,
                Folder = folder,
                FileName = Path.GetFileName(result.PublicId),
                ContentType = file.ContentType,
                SizeInBytes = file.Length,
                Provider = StorageProvider.Cloudinary
            };
        }

        var videoUploadParams = new VideoUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folder,
            UseFilename = false,
            UniqueFilename = true,
            Overwrite = false
        };

        var videoResult = await _cloudinary.UploadAsync(videoUploadParams, cancellationToken);
        if (videoResult.Error is not null)
        {
            throw new InvalidOperationException($"Cloudinary upload failed: {videoResult.Error.Message}");
        }

        return new MediaUploadResult
        {
            Url = videoResult.Url?.ToString() ?? string.Empty,
            SecureUrl = videoResult.SecureUrl?.ToString(),
            ProviderKey = videoResult.PublicId,
            PublicId = videoResult.PublicId,
            Folder = folder,
            FileName = Path.GetFileName(videoResult.PublicId),
            ContentType = file.ContentType,
            SizeInBytes = file.Length,
            Provider = StorageProvider.Cloudinary
        };
    }

    public async Task DeleteAsync(string providerKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(providerKey)) return;

        var deleteParams = new DeletionParams(providerKey)
        {
            ResourceType = ResourceType.Auto
        };

        await _cloudinary.DestroyAsync(deleteParams);
    }

    private string BuildFolder(MediaUploadOptions options)
    {
        var trainerSegment = options.TrainerId.HasValue ? $"trainers/{options.TrainerId}" : "global";

        var categorySegment = options.Category switch
        {
            MediaCategory.TrainerProfilePhoto => "profile",
            MediaCategory.TrainerLogo => "logo",
            MediaCategory.TrainerBanner or MediaCategory.PublicPageBanner => "banner",
            MediaCategory.ExerciseImage or MediaCategory.ExerciseVideo => "exercises",
            MediaCategory.PostCoverImage or MediaCategory.PostVideo => "posts",
            MediaCategory.ProgressPhoto when options.StudentId.HasValue => $"students/{options.StudentId}/progress",
            MediaCategory.TransformationBefore or MediaCategory.TransformationAfter => "transformations",
            _ => "other"
        };

        return $"{_rootFolder}/{trainerSegment}/{categorySegment}";
    }

}
