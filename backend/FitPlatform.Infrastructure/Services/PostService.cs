using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Posts;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class PostService
{
    private readonly AppDbContext _db;

    public PostService(AppDbContext db) => _db = db;

    public async Task<ApiResponse<List<PostResponse>>> GetAllAsync(Guid trainerId)
    {
        var posts = await _db.Posts.Where(p => p.TrainerId == trainerId).OrderByDescending(p => p.CreatedAt).ToListAsync();
        return ApiResponse<List<PostResponse>>.Ok(posts.Select(MapResponse).ToList());
    }

    public async Task<ApiResponse<PostResponse>> GetByIdAsync(Guid id, Guid trainerId)
    {
        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id && p.TrainerId == trainerId);
        if (post == null) return ApiResponse<PostResponse>.Fail("Post não encontrado.");
        return ApiResponse<PostResponse>.Ok(MapResponse(post));
    }

    public async Task<ApiResponse<PostResponse>> CreateAsync(PostRequest request, Guid trainerId)
    {
        var isPublishing = request.Status == PostStatus.Published;
        var post = new Post
        {
            TrainerId = trainerId,
            Title = request.Title,
            Description = request.Description,
            Content = request.Content,
            ImageUrl = request.ImageUrl,
            VideoUrl = request.VideoUrl,
            Status = request.Status,
            Visibility = request.Visibility,
            Tags = request.Tags,
            PublishedAt = isPublishing ? DateTime.UtcNow : null
        };
        _db.Posts.Add(post);
        await _db.SaveChangesAsync();

        // Notify active students when a post is published
        if (isPublishing && request.Visibility != PostVisibility.Private)
            await NotifyStudentsAboutNewPostAsync(trainerId, post.Id, post.Title);

        return ApiResponse<PostResponse>.Ok(MapResponse(post), "Post criado com sucesso.");
    }

    public async Task<ApiResponse<PostResponse>> UpdateAsync(Guid id, PostRequest request, Guid trainerId)
    {
        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id && p.TrainerId == trainerId);
        if (post == null) return ApiResponse<PostResponse>.Fail("Post não encontrado.");

        var wasNotPublished = post.Status != PostStatus.Published;
        var isNowPublishing = request.Status == PostStatus.Published;

        post.Title = request.Title;
        post.Description = request.Description;
        post.Content = request.Content;
        post.ImageUrl = request.ImageUrl;
        post.VideoUrl = request.VideoUrl;
        post.Status = request.Status;
        post.Visibility = request.Visibility;
        post.Tags = request.Tags;
        post.UpdatedAt = DateTime.UtcNow;

        if (wasNotPublished && isNowPublishing)
        {
            post.PublishedAt = DateTime.UtcNow;
            if (request.Visibility != PostVisibility.Private)
                await NotifyStudentsAboutNewPostAsync(trainerId, post.Id, post.Title);
        }

        await _db.SaveChangesAsync();
        return ApiResponse<PostResponse>.Ok(MapResponse(post));
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, Guid trainerId)
    {
        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id && p.TrainerId == trainerId);
        if (post == null) return ApiResponse.Fail("Post não encontrado.");
        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();
        return ApiResponse.Ok("Post removido.");
    }

    public static PostResponse MapResponse(Post p) => new()
    {
        Id = p.Id,
        TrainerId = p.TrainerId,
        Title = p.Title,
        Description = p.Description,
        Content = p.Content,
        ImageUrl = p.ImageUrl,
        VideoUrl = p.VideoUrl,
        Status = p.Status.ToString(),
        Visibility = p.Visibility.ToString(),
        Tags = FeedBuilderService.ParseTags(p.Tags),
        PublishedAt = p.PublishedAt,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };

    private async Task NotifyStudentsAboutNewPostAsync(Guid trainerId, Guid postId, string postTitle)
    {
        var activeStudents = await _db.Students
            .Include(s => s.User)
            .Where(s => s.TrainerId == trainerId && s.Status == StudentStatus.Active)
            .ToListAsync();

        var notifications = activeStudents.Select(s => new Notification
        {
            UserId = s.UserId,
            TrainerId = trainerId,
            StudentId = s.Id,
            Title = "Novo conteúdo publicado",
            Message = $"Seu personal publicou: {postTitle}",
            Type = NotificationType.NewPost
        }).ToList();

        if (notifications.Any())
        {
            await _db.Notifications.AddRangeAsync(notifications);
            await _db.SaveChangesAsync();
        }
    }
}
