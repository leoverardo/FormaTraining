using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FitPlatform.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace FitPlatform.Infrastructure.Storage;

public interface ICloudinaryUrlService
{
    string? GetThumbnailUrl(string? publicId);
    string? GetOptimizedImageUrl(string? publicId, int width = 800);
}

public class CloudinaryUrlService : ICloudinaryUrlService
{
    private readonly Cloudinary _cloudinary;
    private readonly bool _isConfigured;

    public CloudinaryUrlService(IConfiguration configuration)
    {
        var cloudName = configuration["Cloudinary:CloudName"] ?? string.Empty;
        var apiKey = configuration["Cloudinary:ApiKey"] ?? string.Empty;
        var apiSecret = configuration["Cloudinary:ApiSecret"] ?? string.Empty;

        _isConfigured = !string.IsNullOrWhiteSpace(cloudName) && !string.IsNullOrWhiteSpace(apiKey) && !string.IsNullOrWhiteSpace(apiSecret);
        _cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
    }

    public string? GetThumbnailUrl(string? publicId)
    {
        if (!_isConfigured || string.IsNullOrWhiteSpace(publicId)) return null;

        return _cloudinary.Api.UrlImgUp
            .Transform(new Transformation().Width(300).Height(300).Crop("fill").Gravity("auto").Quality("auto"))
            .BuildUrl(publicId);
    }

    public string? GetOptimizedImageUrl(string? publicId, int width = 800)
    {
        if (!_isConfigured || string.IsNullOrWhiteSpace(publicId)) return null;

        return _cloudinary.Api.UrlImgUp
            .Transform(new Transformation().Width(width).Crop("limit").Quality("auto").FetchFormat("auto"))
            .BuildUrl(publicId);
    }
}
