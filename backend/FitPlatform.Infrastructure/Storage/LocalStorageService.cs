using FitPlatform.Application.Interfaces;
using FitPlatform.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace FitPlatform.Infrastructure.Storage;

public class LocalStorageService : IStorageService
{
    private readonly string _basePath;
    private readonly string _publicBaseUrl;

    public LocalStorageService(IConfiguration config)
    {
        var relativeBasePath = config["Storage:LocalBasePath"] ?? "wwwroot/uploads";
        _basePath = Path.IsPathRooted(relativeBasePath)
            ? relativeBasePath
            : Path.Combine(Directory.GetCurrentDirectory(), relativeBasePath);
        _publicBaseUrl = config["Storage:PublicBaseUrl"] ?? "https://localhost:5001/uploads";
        Directory.CreateDirectory(_basePath);
    }

    public async Task<MediaUploadResult> UploadAsync(IFormFile file, MediaUploadOptions options, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var safeName = $"{Guid.NewGuid():N}{extension}";
        var folderPath = Path.Combine(_basePath, options.Category.ToString());
        Directory.CreateDirectory(folderPath);

        var filePath = Path.Combine(folderPath, safeName);
        await using var fileStream = File.Create(filePath);
        await file.CopyToAsync(fileStream, cancellationToken);

        var providerKey = $"{options.Category}/{safeName}";
        var url = $"{_publicBaseUrl.TrimEnd('/')}/{providerKey}";

        return new MediaUploadResult
        {
            Url = url,
            SecureUrl = url,
            ProviderKey = providerKey,
            PublicId = providerKey,
            Folder = options.Category.ToString(),
            FileName = safeName,
            ContentType = file.ContentType,
            SizeInBytes = file.Length,
            Provider = StorageProvider.Local
        };
    }

    public Task DeleteAsync(string providerKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(providerKey)) return Task.CompletedTask;

        var fullPath = Path.Combine(_basePath, providerKey.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}
