using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Explore;
using FitPlatform.Application.DTOs.Feed;
using FitPlatform.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FitPlatform.Infrastructure.Services;

public class ExploreService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ExploreService> _logger;

    public ExploreService(AppDbContext db, ILogger<ExploreService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ApiResponse<List<FeedItemDto>>> GetExploreFeedAsync(Guid? userId, int page = 1, int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var posts = await _db.Posts
            .AsNoTracking()
            .Include(p => p.Trainer).ThenInclude(t => t.User)
            .Where(p => p.Status == Domain.Enums.PostStatus.Published
                        && p.Visibility == Domain.Enums.PostVisibility.Public
                        && p.Trainer.PublicPageEnabled
                        && p.Trainer.PublicSearchEnabled
                        && p.Trainer.User.IsActive
                        && _db.TrainerSubscriptions.Any(s => s.TrainerId == p.TrainerId
                                                              && s.Status == Domain.Enums.TrainerSubscriptionStatus.Active
                                                              && s.EndDate >= DateTime.UtcNow))
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = posts.Select(p => FeedBuilderService.BuildPostItem(p, p.Trainer.User.Name, p.Trainer.ProfilePhotoUrl)).ToList();
        if (userId.HasValue && items.Count > 0)
            await EnrichWithSocialDataAsync(items, userId.Value);

        return ApiResponse<List<FeedItemDto>>.Ok(items);
    }

    public async Task<ApiResponse<TrainerSearchResponseDto>> SearchTrainersAsync(ExploreTrainerQuery query, Guid? studentProfileId)
    {
        try
        {
            return await SearchTrainersCoreAsync(query, studentProfileId);
        }
        catch (SqlException ex) when (ex.Number is 207 or 208)
        {
            _logger.LogWarning("Explore search fallback due to schema mismatch: {Message}", ex.Message);
            return await SearchTrainersLegacyAsync(query, studentProfileId);
        }
    }

    private async Task<ApiResponse<TrainerSearchResponseDto>> SearchTrainersCoreAsync(ExploreTrainerQuery query, Guid? studentProfileId)
    {
        query.Page = Math.Max(1, query.Page);
        query.PageSize = Math.Clamp(query.PageSize, 1, 50);

        var normalizedSearch = (query.Search ?? query.Name)?.Trim();
        var totalTrainers = await _db.Trainers.CountAsync();

        var trainerQuery = _db.Trainers
            .AsNoTracking()
            .Include(t => t.User)
            .Where(t => t.User.IsActive && t.PublicPageEnabled && t.PublicSearchEnabled && t.AcceptingStudents
                        && _db.TrainerSubscriptions.Any(s => s.TrainerId == t.Id
                                                              && s.Status == Domain.Enums.TrainerSubscriptionStatus.Active
                                                              && s.EndDate >= DateTime.UtcNow));

        if (!string.IsNullOrWhiteSpace(normalizedSearch))
            trainerQuery = trainerQuery.Where(t => t.User.Name.Contains(normalizedSearch) || t.BrandName.Contains(normalizedSearch));
        if (!string.IsNullOrWhiteSpace(query.City))
            trainerQuery = trainerQuery.Where(t => t.City == query.City);
        if (!string.IsNullOrWhiteSpace(query.State))
            trainerQuery = trainerQuery.Where(t => t.State == query.State);
        if (!string.IsNullOrWhiteSpace(query.Neighborhood))
            trainerQuery = trainerQuery.Where(t => t.Neighborhood == query.Neighborhood);

        if (!string.IsNullOrWhiteSpace(query.Specialty))
        {
            var terms = query.Specialty
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => t.Length > 1)
                .ToList();

            if (terms.Count > 0)
                trainerQuery = trainerQuery.Where(t => t.Specialties != null && terms.Any(term => t.Specialties.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(query.ServiceMode))
            trainerQuery = trainerQuery.Where(t => t.ServiceMode == query.ServiceMode);

        var trainers = await trainerQuery.OrderByDescending(t => t.CreatedAt).ToListAsync();
        var totalVisible = trainers.Count;

        if (trainers.Count == 0)
        {
            return ApiResponse<TrainerSearchResponseDto>.Ok(new TrainerSearchResponseDto
            {
                Items = new(),
                Total = 0,
                Page = query.Page,
                PageSize = query.PageSize
            });
        }

        var trainerIds = trainers.Select(t => t.Id).ToList();
        var postsByTrainer = await _db.Posts
            .AsNoTracking()
            .Where(p => trainerIds.Contains(p.TrainerId)
                        && p.Status == Domain.Enums.PostStatus.Published
                        && p.Visibility == Domain.Enums.PostVisibility.Public)
            .GroupBy(p => p.TrainerId)
            .Select(g => new { TrainerId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TrainerId, x => x.Count);

        var activeStudentsByTrainer = await _db.Students
            .AsNoTracking()
            .Where(s => trainerIds.Contains(s.TrainerId) && s.Status == Domain.Enums.StudentStatus.Active)
            .GroupBy(s => s.TrainerId)
            .Select(g => new { TrainerId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TrainerId, x => x.Count);

        HashSet<Guid> followed = new();
        HashSet<Guid> saved = new();
        if (studentProfileId.HasValue)
        {
            followed = (await _db.TrainerFollowers.Where(f => f.StudentProfileId == studentProfileId.Value).Select(f => f.TrainerId).ToListAsync()).ToHashSet();
            saved = (await _db.SavedTrainers.Where(s => s.StudentProfileId == studentProfileId.Value).Select(s => s.TrainerId).ToListAsync()).ToHashSet();
        }

        var results = trainers.Select(t =>
        {
            var distanceKm = CalculateDistanceIfPossible(query.Latitude, query.Longitude, t.Latitude, t.Longitude);
            return new TrainerSearchResultDto
            {
                TrainerId = t.Id,
                Slug = t.PublicSlug,
                FullName = t.User.Name,
                BrandName = t.BrandName,
                Headline = t.PublicHeadline,
                Bio = t.Bio,
                City = t.City,
                State = t.State,
                Neighborhood = t.Neighborhood,
                Specialties = ParseSpecialties(t.Specialties),
                ServiceMode = t.ServiceMode,
                ProfilePhotoUrl = t.ProfilePhotoUrl,
                BannerUrl = t.BannerUrl,
                Rating = null,
                ReviewsCount = 0,
                PublicPostsCount = postsByTrainer.TryGetValue(t.Id, out var postCount) ? postCount : 0,
                ActiveStudentsCountPublic = activeStudentsByTrainer.TryGetValue(t.Id, out var studentCount) ? studentCount : 0,
                DistanceKm = distanceKm,
                IsFollowedByCurrentUser = followed.Contains(t.Id),
                IsSavedByCurrentUser = saved.Contains(t.Id),
                AcceptingStudents = t.AcceptingStudents
            };
        }).ToList();

        if (query.Latitude.HasValue && query.Longitude.HasValue)
        {
            var radiusKm = query.RadiusKm.GetValueOrDefault(0);
            if (radiusKm > 0)
            {
                results = results.Where(r =>
                    !r.DistanceKm.HasValue ||
                    IsOnlineMode(r.ServiceMode) ||
                    r.DistanceKm.Value <= radiusKm
                ).ToList();
            }

            results = results
                .OrderBy(r => IsOnlineMode(r.ServiceMode) ? 1 : 0)
                .ThenBy(r => r.DistanceKm ?? decimal.MaxValue)
                .ThenByDescending(r => r.PublicPostsCount)
                .ToList();
        }
        else
        {
            results = results
                .OrderByDescending(r => r.PublicPostsCount)
                .ThenBy(r => r.BrandName)
                .ToList();
        }

        var total = results.Count;
        var paged = results
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        _logger.LogInformation(
            "Explore trainers: total={TotalTrainers} visible={TotalVisible} returned={Returned} filters search={Search} city={City} state={State} specialty={Specialty} mode={Mode} lat={Lat} lng={Lng} radius={Radius}",
            totalTrainers, totalVisible, paged.Count, normalizedSearch, query.City, query.State, query.Specialty, query.ServiceMode, query.Latitude, query.Longitude, query.RadiusKm);

        return ApiResponse<TrainerSearchResponseDto>.Ok(new TrainerSearchResponseDto
        {
            Items = paged,
            Total = total,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    private async Task<ApiResponse<TrainerSearchResponseDto>> SearchTrainersLegacyAsync(ExploreTrainerQuery query, Guid? studentProfileId)
    {
        query.Page = Math.Max(1, query.Page);
        query.PageSize = Math.Clamp(query.PageSize, 1, 50);
        var normalizedSearch = (query.Search ?? query.Name)?.Trim();

        var trainerQuery = _db.Trainers
            .AsNoTracking()
            .Include(t => t.User)
            .Where(t => t.User.IsActive && t.PublicPageEnabled
                        && _db.TrainerSubscriptions.Any(s => s.TrainerId == t.Id
                                                              && s.Status == Domain.Enums.TrainerSubscriptionStatus.Active
                                                              && s.EndDate >= DateTime.UtcNow));

        if (!string.IsNullOrWhiteSpace(normalizedSearch))
            trainerQuery = trainerQuery.Where(t => t.User.Name.Contains(normalizedSearch) || t.BrandName.Contains(normalizedSearch));
        if (!string.IsNullOrWhiteSpace(query.City))
            trainerQuery = trainerQuery.Where(t => t.City == query.City);
        if (!string.IsNullOrWhiteSpace(query.State))
            trainerQuery = trainerQuery.Where(t => t.State == query.State);

        var trainers = await trainerQuery.OrderByDescending(t => t.CreatedAt).ToListAsync();
        var trainerIds = trainers.Select(t => t.Id).ToList();
        var postsByTrainer = await _db.Posts.AsNoTracking()
            .Where(p => trainerIds.Contains(p.TrainerId) && p.Status == Domain.Enums.PostStatus.Published && p.Visibility == Domain.Enums.PostVisibility.Public)
            .GroupBy(p => p.TrainerId)
            .Select(g => new { TrainerId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TrainerId, x => x.Count);

        HashSet<Guid> followed = new();
        HashSet<Guid> saved = new();
        if (studentProfileId.HasValue)
        {
            followed = (await _db.TrainerFollowers.Where(f => f.StudentProfileId == studentProfileId.Value).Select(f => f.TrainerId).ToListAsync()).ToHashSet();
            saved = (await _db.SavedTrainers.Where(s => s.StudentProfileId == studentProfileId.Value).Select(s => s.TrainerId).ToListAsync()).ToHashSet();
        }

        var allItems = trainers.Select(t => new TrainerSearchResultDto
        {
            TrainerId = t.Id,
            Slug = t.PublicSlug,
            FullName = t.User.Name,
            BrandName = t.BrandName,
            Headline = t.PublicHeadline,
            Bio = t.Bio,
            City = t.City,
            State = t.State,
            Neighborhood = t.Neighborhood,
            Specialties = ParseSpecialties(t.Specialties),
            ServiceMode = null,
            ProfilePhotoUrl = t.ProfilePhotoUrl,
            BannerUrl = t.BannerUrl,
            PublicPostsCount = postsByTrainer.TryGetValue(t.Id, out var postCount) ? postCount : 0,
            IsFollowedByCurrentUser = followed.Contains(t.Id),
            IsSavedByCurrentUser = saved.Contains(t.Id),
            AcceptingStudents = true
        }).ToList();

        var total = allItems.Count;
        var items = allItems.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList();
        return ApiResponse<TrainerSearchResponseDto>.Ok(new TrainerSearchResponseDto
        {
            Items = items,
            Total = total,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    private static List<string> ParseSpecialties(string? specialties)
    {
        if (string.IsNullOrWhiteSpace(specialties)) return new();
        return specialties.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool IsOnlineMode(string? mode) =>
        string.Equals(mode, "Online", StringComparison.OrdinalIgnoreCase);

    private static decimal? CalculateDistanceIfPossible(decimal? baseLat, decimal? baseLng, double? targetLat, double? targetLng)
    {
        if (!baseLat.HasValue || !baseLng.HasValue || !targetLat.HasValue || !targetLng.HasValue)
            return null;

        const double earthRadiusKm = 6371d;
        var lat1 = DegreesToRadians((double)baseLat.Value);
        var lon1 = DegreesToRadians((double)baseLng.Value);
        var lat2 = DegreesToRadians((double)targetLat.Value);
        var lon2 = DegreesToRadians((double)targetLng.Value);

        var dLat = lat2 - lat1;
        var dLon = lon2 - lon1;

        var haversine =
            Math.Pow(Math.Sin(dLat / 2), 2) +
            Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dLon / 2), 2);

        var c = 2 * Math.Atan2(Math.Sqrt(haversine), Math.Sqrt(1 - haversine));
        return Math.Round((decimal)(earthRadiusKm * c), 1);
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180d;

    private async Task EnrichWithSocialDataAsync(List<FeedItemDto> items, Guid userId)
    {
        var keys = items.Select(i => i.Id).ToList();
        var reactions = await _db.FeedReactions.Where(r => keys.Contains(r.FeedItemKey)).ToListAsync();
        var saved = await _db.FeedSavedItems.Where(s => keys.Contains(s.FeedItemKey) && s.UserId == userId).Select(s => s.FeedItemKey).ToListAsync();
        var comments = await _db.FeedComments.Where(c => keys.Contains(c.FeedItemKey)).GroupBy(c => c.FeedItemKey).Select(g => new { g.Key, Count = g.Count() }).ToListAsync();

        var commentMap = comments.ToDictionary(x => x.Key, x => x.Count);
        var likeMap = reactions.Where(r => r.ReactionType == Domain.Enums.ReactionType.Like).GroupBy(r => r.FeedItemKey).ToDictionary(g => g.Key, g => g.Count());
        var likedByUser = reactions.Where(r => r.UserId == userId && r.ReactionType == Domain.Enums.ReactionType.Like).Select(r => r.FeedItemKey).ToHashSet();

        foreach (var item in items)
        {
            item.LikesCount = likeMap.GetValueOrDefault(item.Id, 0);
            item.CommentsCount = commentMap.GetValueOrDefault(item.Id, 0);
            item.IsLikedByCurrentUser = likedByUser.Contains(item.Id);
            item.IsSavedByCurrentUser = saved.Contains(item.Id);
        }
    }
}
