using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Feed;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class FeedSocialService
{
    private readonly AppDbContext _db;

    public FeedSocialService(AppDbContext db) => _db = db;

    // ── Likes ─────────────────────────────────────────────────────────────────

    public async Task<ApiResponse<object>> AddLikeAsync(string feedItemKey, Guid userId, Guid? trainerId, Guid? studentId)
    {
        var (entityType, entityId) = ParseKey(feedItemKey);
        if (entityType == null) return ApiResponse<object>.Fail("Feed item inválido.");

        var exists = await _db.FeedReactions
            .AnyAsync(r => r.FeedItemKey == feedItemKey && r.UserId == userId && r.ReactionType == ReactionType.Like);

        if (exists) return ApiResponse<object>.Ok(new { Message = "Já curtido." });

        _db.FeedReactions.Add(new FeedReaction
        {
            UserId = userId,
            TrainerId = trainerId,
            StudentId = studentId,
            FeedItemKey = feedItemKey,
            RelatedEntityType = entityType,
            RelatedEntityId = entityId,
            ReactionType = ReactionType.Like
        });

        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { Message = "Like adicionado." });
    }

    public async Task<ApiResponse<object>> RemoveLikeAsync(string feedItemKey, Guid userId)
    {
        var reaction = await _db.FeedReactions
            .FirstOrDefaultAsync(r => r.FeedItemKey == feedItemKey && r.UserId == userId && r.ReactionType == ReactionType.Like);

        if (reaction == null) return ApiResponse<object>.Fail("Like não encontrado.");

        _db.FeedReactions.Remove(reaction);
        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { Message = "Like removido." });
    }

    // ── Saves ─────────────────────────────────────────────────────────────────

    public async Task<ApiResponse<object>> SaveItemAsync(string feedItemKey, Guid userId, Guid? trainerId, Guid? studentId)
    {
        var (entityType, entityId) = ParseKey(feedItemKey);
        if (entityType == null) return ApiResponse<object>.Fail("Feed item inválido.");

        var exists = await _db.FeedSavedItems
            .AnyAsync(s => s.FeedItemKey == feedItemKey && s.UserId == userId);

        if (exists) return ApiResponse<object>.Ok(new { Message = "Já salvo." });

        _db.FeedSavedItems.Add(new FeedSavedItem
        {
            UserId = userId,
            TrainerId = trainerId,
            StudentId = studentId,
            FeedItemKey = feedItemKey,
            RelatedEntityType = entityType,
            RelatedEntityId = entityId
        });

        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { Message = "Item salvo." });
    }

    public async Task<ApiResponse<object>> UnsaveItemAsync(string feedItemKey, Guid userId)
    {
        var saved = await _db.FeedSavedItems
            .FirstOrDefaultAsync(s => s.FeedItemKey == feedItemKey && s.UserId == userId);

        if (saved == null) return ApiResponse<object>.Fail("Item não encontrado nos salvos.");

        _db.FeedSavedItems.Remove(saved);
        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { Message = "Item removido dos salvos." });
    }

    // ── Comments ──────────────────────────────────────────────────────────────

    public async Task<ApiResponse<List<FeedCommentDto>>> GetCommentsAsync(string feedItemKey)
    {
        var (entityType, _) = ParseKey(feedItemKey);
        if (entityType == null) return ApiResponse<List<FeedCommentDto>>.Fail("Feed item inválido.");

        var comments = await _db.FeedComments
            .Include(c => c.User)
            .Where(c => c.FeedItemKey == feedItemKey)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        var result = comments.Select(c => new FeedCommentDto
        {
            Id = c.Id,
            UserId = c.UserId,
            AuthorName = c.User.Name,
            AuthorRole = c.TrainerId.HasValue ? "Trainer" : "Student",
            Comment = c.Comment,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }).ToList();

        return ApiResponse<List<FeedCommentDto>>.Ok(result);
    }

    public async Task<ApiResponse<FeedCommentDto>> AddCommentAsync(
        string feedItemKey, string text, Guid userId, Guid? trainerId, Guid? studentId)
    {
        var (entityType, entityId) = ParseKey(feedItemKey);
        if (entityType == null) return ApiResponse<FeedCommentDto>.Fail("Feed item inválido.");

        if (string.IsNullOrWhiteSpace(text))
            return ApiResponse<FeedCommentDto>.Fail("Comentário não pode ser vazio.");

        var comment = new FeedComment
        {
            UserId = userId,
            TrainerId = trainerId,
            StudentId = studentId,
            FeedItemKey = feedItemKey,
            RelatedEntityType = entityType,
            RelatedEntityId = entityId,
            Comment = text.Trim()
        };

        _db.FeedComments.Add(comment);
        await _db.SaveChangesAsync();

        await _db.Entry(comment).Reference(c => c.User).LoadAsync();

        return ApiResponse<FeedCommentDto>.Ok(new FeedCommentDto
        {
            Id = comment.Id,
            UserId = comment.UserId,
            AuthorName = comment.User.Name,
            AuthorRole = trainerId.HasValue ? "Trainer" : "Student",
            Comment = comment.Comment,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        });
    }

    public async Task<ApiResponse<object>> DeleteCommentAsync(Guid commentId, Guid userId)
    {
        var comment = await _db.FeedComments.FirstOrDefaultAsync(c => c.Id == commentId);
        if (comment == null) return ApiResponse<object>.Fail("Comentário não encontrado.");
        if (comment.UserId != userId) return ApiResponse<object>.Fail("Sem permissão para deletar este comentário.");

        _db.FeedComments.Remove(comment);
        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { Message = "Comentário removido." });
    }

    // ── Helper ─────────────────────────────────────────────────────────────────

    private static (string? entityType, Guid entityId) ParseKey(string feedItemKey)
    {
        if (string.IsNullOrWhiteSpace(feedItemKey)) return (null, Guid.Empty);

        var parts = feedItemKey.Split(':', 2);
        if (parts.Length != 2) return (null, Guid.Empty);

        if (!Guid.TryParse(parts[1], out var id)) return (null, Guid.Empty);

        var entityType = parts[0] switch
        {
            "post" => "Post",
            "progress" => "StudentProgress",
            "photo" => "StudentProgressPhoto",
            "checkin" => "StudentWeeklyCheckIn",
            "workout-session" => "WorkoutSession",
            _ => null
        };

        return (entityType, id);
    }
}
