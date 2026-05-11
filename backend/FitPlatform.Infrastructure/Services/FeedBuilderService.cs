using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Feed;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class FeedBuilderService
{
    private readonly AppDbContext _db;

    public FeedBuilderService(AppDbContext db) => _db = db;

    // ── Public feed for trainer slug ─────────────────────────────────────────

    public async Task<ApiResponse<List<FeedItemDto>>> GetPublicFeedAsync(string slug, int page = 1, int pageSize = 20)
    {
        var trainer = await _db.Trainers.Include(t => t.User)
            .FirstOrDefaultAsync(t => t.PublicSlug == slug && t.PublicPageEnabled);

        if (trainer == null)
            return ApiResponse<List<FeedItemDto>>.Fail("Página não encontrada.");

        var posts = await _db.Posts
            .Where(p => p.TrainerId == trainer.Id
                     && p.Status == PostStatus.Published
                     && p.Visibility == PostVisibility.Public)
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = posts.Select(p => BuildPostItem(p, trainer.User.Name, trainer.ProfilePhotoUrl)).ToList();
        return ApiResponse<List<FeedItemDto>>.Ok(items);
    }

    // ── Private feed for authenticated student ────────────────────────────────

    public async Task<ApiResponse<List<FeedItemDto>>> GetStudentFeedAsync(
        Guid studentId, Guid userId, string? type = null, int page = 1, int pageSize = 20)
    {
        var student = await _db.Students.Include(s => s.Trainer).ThenInclude(t => t.User)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == studentId);

        if (student == null) return ApiResponse<List<FeedItemDto>>.Fail("Aluno não encontrado.");

        var trainerId = student.TrainerId;
        var trainer = student.Trainer;
        var studentName = student.User?.Name ?? "Aluno";
        var trainerName = trainer?.User?.Name ?? "Personal Trainer";
        var trainerPhotoUrl = trainer?.ProfilePhotoUrl;

        var items = new List<FeedItemDto>();

        if (type == null || type == "Post")
        {
            var posts = await _db.Posts
                .Where(p => p.TrainerId == trainerId
                         && p.Status == PostStatus.Published
                         && p.Visibility != PostVisibility.Private)
                .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
                .ToListAsync();

            items.AddRange(posts.Select(p => BuildPostItem(p, trainerName, trainerPhotoUrl)));
        }

        if (type == null || type == "Progress")
        {
            var progresses = await _db.StudentProgressRecords
                .Where(p => p.StudentId == studentId)
                .OrderByDescending(p => p.ProgressDate)
                .Take(10)
                .ToListAsync();

            items.AddRange(progresses.Select(p => BuildProgressItem(p, studentName)));
        }

        if (type == null || type == "CheckIn")
        {
            var checkIns = await _db.StudentWeeklyCheckIns
                .Where(c => c.StudentId == studentId)
                .OrderByDescending(c => c.WeekStartDate)
                .Take(5)
                .ToListAsync();

            items.AddRange(checkIns.Select(c => BuildCheckInItem(c, studentName, trainerId)));
        }

        if (type == null || type == "ProgressPhoto")
        {
            var photos = await _db.StudentProgressPhotos
                .Where(p => p.StudentId == studentId)
                .OrderByDescending(p => p.PhotoDate)
                .Take(10)
                .ToListAsync();

            items.AddRange(photos.Select(p => BuildPhotoItem(p, studentName, trainerId)));
        }

        if (type == null || type == "WorkoutCompleted")
        {
            var sessions = await _db.WorkoutSessions
                .Include(ws => ws.Workout)
                .Where(ws => ws.StudentId == studentId && ws.Status == WorkoutSessionStatus.Completed)
                .OrderByDescending(ws => ws.CompletedAt)
                .Take(10)
                .ToListAsync();

            items.AddRange(sessions.Select(ws => BuildWorkoutSessionItem(ws, studentName, trainerId)));
        }

        // Enrich with social data
        await EnrichWithSocialDataAsync(items, userId);

        var paged = items
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return ApiResponse<List<FeedItemDto>>.Ok(paged);
    }

    // ── Trainer recent activity feed ──────────────────────────────────────────

    public async Task<ApiResponse<List<TrainerActivityDto>>> GetTrainerRecentActivityAsync(Guid trainerId, int limit = 20)
    {
        var activities = new List<TrainerActivityDto>();

        var recentProgress = await _db.StudentProgressRecords
            .Include(p => p.Student).ThenInclude(s => s.User)
            .Where(p => p.TrainerId == trainerId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(10)
            .ToListAsync();

        activities.AddRange(recentProgress.Select(p => new TrainerActivityDto
        {
            Type = "Progress",
            Title = "Novo progresso registrado",
            Description = $"Peso: {p.Weight}kg",
            StudentName = p.Student?.User?.Name ?? "Aluno",
            RelatedEntityId = p.Id,
            RelatedEntityType = "StudentProgress",
            CreatedAt = p.CreatedAt
        }));

        var recentPhotos = await _db.StudentProgressPhotos
            .Include(p => p.Student).ThenInclude(s => s.User)
            .Where(p => p.TrainerId == trainerId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToListAsync();

        activities.AddRange(recentPhotos.Select(p => new TrainerActivityDto
        {
            Type = "ProgressPhoto",
            Title = "Nova foto de progresso",
            Description = p.Description ?? "Foto de progresso enviada",
            StudentName = p.Student?.User?.Name ?? "Aluno",
            RelatedEntityId = p.Id,
            RelatedEntityType = "StudentProgressPhoto",
            CreatedAt = p.CreatedAt
        }));

        var recentCheckIns = await _db.StudentWeeklyCheckIns
            .Include(c => c.Student).ThenInclude(s => s.User)
            .Where(c => c.TrainerId == trainerId)
            .OrderByDescending(c => c.CreatedAt)
            .Take(10)
            .ToListAsync();

        activities.AddRange(recentCheckIns.Select(c => new TrainerActivityDto
        {
            Type = "CheckIn",
            Title = "Check-in semanal enviado",
            Description = $"Humor: {c.MoodLevel}/5 | Energia: {c.EnergyLevel}/5",
            StudentName = c.Student?.User?.Name ?? "Aluno",
            RelatedEntityId = c.Id,
            RelatedEntityType = "StudentWeeklyCheckIn",
            CreatedAt = c.CreatedAt
        }));

        var recentSessions = await _db.WorkoutSessions
            .Include(ws => ws.Student).ThenInclude(s => s.User)
            .Include(ws => ws.Workout)
            .Where(ws => ws.TrainerId == trainerId && ws.Status == WorkoutSessionStatus.Completed)
            .OrderByDescending(ws => ws.CompletedAt)
            .Take(10)
            .ToListAsync();

        activities.AddRange(recentSessions.Select(ws => new TrainerActivityDto
        {
            Type = "WorkoutCompleted",
            Title = "Treino concluído",
            Description = ws.Workout?.Name ?? "Treino",
            StudentName = ws.Student?.User?.Name ?? "Aluno",
            RelatedEntityId = ws.Id,
            RelatedEntityType = "WorkoutSession",
            CreatedAt = ws.CompletedAt ?? ws.CreatedAt
        }));

        var result = activities
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToList();

        return ApiResponse<List<TrainerActivityDto>>.Ok(result);
    }

    // ── Social data enrichment ─────────────────────────────────────────────────

    private async Task EnrichWithSocialDataAsync(List<FeedItemDto> items, Guid userId)
    {
        if (!items.Any()) return;

        var keys = items.Select(i => i.Id).ToList();

        var reactions = await _db.FeedReactions
            .Where(r => keys.Contains(r.FeedItemKey))
            .ToListAsync();

        var saved = await _db.FeedSavedItems
            .Where(s => keys.Contains(s.FeedItemKey) && s.UserId == userId)
            .Select(s => s.FeedItemKey)
            .ToListAsync();

        var commentCounts = await _db.FeedComments
            .Where(c => keys.Contains(c.FeedItemKey))
            .GroupBy(c => c.FeedItemKey)
            .Select(g => new { Key = g.Key, Count = g.Count() })
            .ToListAsync();

        var commentCountDict = commentCounts.ToDictionary(x => x.Key, x => x.Count);
        var userLikedKeys = reactions
            .Where(r => r.UserId == userId && r.ReactionType == ReactionType.Like)
            .Select(r => r.FeedItemKey)
            .ToHashSet();
        var likeCountDict = reactions
            .Where(r => r.ReactionType == ReactionType.Like)
            .GroupBy(r => r.FeedItemKey)
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var item in items)
        {
            item.LikesCount = likeCountDict.GetValueOrDefault(item.Id, 0);
            item.CommentsCount = commentCountDict.GetValueOrDefault(item.Id, 0);
            item.IsLikedByCurrentUser = userLikedKeys.Contains(item.Id);
            item.IsSavedByCurrentUser = saved.Contains(item.Id);
        }
    }

    // ── Item builders ─────────────────────────────────────────────────────────

    public static FeedItemDto BuildPostItem(Domain.Entities.Post p, string authorName, string? avatarUrl) => new()
    {
        Id = $"post:{p.Id}",
        Type = "Post",
        AuthorId = p.TrainerId,
        AuthorName = authorName,
        AuthorAvatarUrl = avatarUrl,
        AuthorRole = "Trainer",
        TrainerId = p.TrainerId,
        Title = p.Title,
        Text = p.Description,
        MediaUrl = p.ImageUrl ?? p.CoverMedia?.Url,
        MediaType = p.ImageUrl != null || p.CoverMedia != null ? "Image"
                  : p.VideoUrl != null || p.VideoMedia != null ? "Video" : "None",
        Tags = ParseTags(p.Tags),
        CreatedAt = p.PublishedAt ?? p.CreatedAt,
        Visibility = p.Visibility.ToString(),
        RelatedEntityType = "Post",
        RelatedEntityId = p.Id
    };

    private static FeedItemDto BuildProgressItem(Domain.Entities.StudentProgress p, string studentName) => new()
    {
        Id = $"progress:{p.Id}",
        Type = "Progress",
        AuthorId = p.StudentId,
        AuthorName = studentName,
        AuthorRole = "Student",
        TrainerId = p.TrainerId,
        StudentId = p.StudentId,
        Title = "Progresso registrado",
        Text = p.Notes ?? $"Peso: {p.Weight}kg",
        MediaType = "None",
        CreatedAt = p.CreatedAt,
        Visibility = "StudentsOnly",
        RelatedEntityType = "StudentProgress",
        RelatedEntityId = p.Id
    };

    private static FeedItemDto BuildCheckInItem(Domain.Entities.StudentWeeklyCheckIn c, string studentName, Guid trainerId) => new()
    {
        Id = $"checkin:{c.Id}",
        Type = "CheckIn",
        AuthorId = c.StudentId,
        AuthorName = studentName,
        AuthorRole = "Student",
        TrainerId = trainerId,
        StudentId = c.StudentId,
        Title = "Check-in semanal",
        Text = c.Notes ?? $"Semana de {c.WeekStartDate:dd/MM} - Treino: {c.TrainingAdherence}/5",
        MediaType = "None",
        CreatedAt = c.CreatedAt,
        Visibility = "StudentsOnly",
        RelatedEntityType = "StudentWeeklyCheckIn",
        RelatedEntityId = c.Id
    };

    private static FeedItemDto BuildPhotoItem(Domain.Entities.StudentProgressPhoto p, string studentName, Guid trainerId) => new()
    {
        Id = $"photo:{p.Id}",
        Type = "ProgressPhoto",
        AuthorId = p.StudentId,
        AuthorName = studentName,
        AuthorRole = "Student",
        TrainerId = trainerId,
        StudentId = p.StudentId,
        Title = "Foto de progresso",
        Text = p.Description,
        MediaUrl = p.ImageUrl ?? p.MediaAsset?.SecureUrl ?? p.MediaAsset?.Url,
        MediaType = "Image",
        CreatedAt = p.CreatedAt,
        Visibility = "Private",
        RelatedEntityType = "StudentProgressPhoto",
        RelatedEntityId = p.Id
    };

    private static FeedItemDto BuildWorkoutSessionItem(Domain.Entities.WorkoutSession ws, string studentName, Guid trainerId) => new()
    {
        Id = $"workout-session:{ws.Id}",
        Type = "WorkoutCompleted",
        AuthorId = ws.StudentId,
        AuthorName = studentName,
        AuthorRole = "Student",
        TrainerId = trainerId,
        StudentId = ws.StudentId,
        Title = "Treino concluído",
        Text = ws.Workout?.Name ?? "Treino",
        MediaType = "None",
        CreatedAt = ws.CompletedAt ?? ws.CreatedAt,
        Visibility = "StudentsOnly",
        RelatedEntityType = "WorkoutSession",
        RelatedEntityId = ws.Id
    };

    public static List<string> ParseTags(string? tags)
    {
        if (string.IsNullOrWhiteSpace(tags)) return new();
        return tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                   .Select(t => t.Trim())
                   .Where(t => !string.IsNullOrEmpty(t))
                   .ToList();
    }
}
