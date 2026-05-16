using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.PublicPage;
using FitPlatform.Application.DTOs.ServiceSales;
using FitPlatform.Application.DTOs.Testimonials;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FitPlatform.Infrastructure.Services;

public class PublicPageService
{
    private readonly AppDbContext _db;
    private readonly ILogger<PublicPageService> _logger;
    private readonly PrivacyLgpdService _privacyLgpdService;

    public PublicPageService(AppDbContext db, ILogger<PublicPageService> logger, PrivacyLgpdService privacyLgpdService)
    {
        _db = db;
        _logger = logger;
        _privacyLgpdService = privacyLgpdService;
    }

    public async Task<ApiResponse<PublicPageResponse>> GetBySlugAsync(string slug)
    {
        var trainer = await _db.Trainers.Include(t => t.User)
            .FirstOrDefaultAsync(t => t.PublicSlug == slug && t.PublicPageEnabled);
        if (trainer == null) return ApiResponse<PublicPageResponse>.Fail("Página não encontrada.");

        var testimonials = await _db.StudentTestimonials
            .Where(t => t.TrainerId == trainer.Id && t.ApprovedByStudent && t.Published)
            .ToListAsync();

        var transformations = await _db.StudentTransformations
            .Where(t => t.TrainerId == trainer.Id && t.ApprovedByStudent && t.Published)
            .ToListAsync();

        var recentPosts = await _db.Posts
            .Where(p => p.TrainerId == trainer.Id && p.Status == PostStatus.Published && p.Visibility == PostVisibility.Public)
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .Take(6)
            .ToListAsync();

        var activeStudentsCount = await _db.Students
            .CountAsync(s => s.TrainerId == trainer.Id && s.Status == StudentStatus.Active);

        var postsCount = await _db.Posts
            .CountAsync(p => p.TrainerId == trainer.Id && p.Status == PostStatus.Published && p.Visibility == PostVisibility.Public);

        var offers = await _db.TrainerServiceOffers
            .Where(o => o.TrainerId == trainer.Id && o.IsActive && o.IsPublic && o.BillingType == TrainerServiceBillingType.OneTime)
            .OrderBy(o => o.DisplayOrder).ThenBy(o => o.Title)
            .ToListAsync();

        return ApiResponse<PublicPageResponse>.Ok(new PublicPageResponse
        {
            TrainerId = trainer.Id,
            FullName = trainer.User.Name,
            BrandName = trainer.BrandName,
            PublicSlug = trainer.PublicSlug,
            PublicPageEnabled = trainer.PublicPageEnabled,
            PublicSearchEnabled = trainer.PublicSearchEnabled,
            AcceptingStudents = trainer.AcceptingStudents,
            PublicHeadline = trainer.PublicHeadline,
            PublicDescription = trainer.PublicDescription,
            Bio = trainer.Bio,
            Specialties = trainer.Specialties,
            Instagram = trainer.ShowInstagram ? trainer.Instagram : null,
            WhatsappNumber = trainer.WhatsappNumber,
            ProfilePhotoUrl = trainer.ProfilePhotoUrl,
            LogoUrl = trainer.LogoUrl,
            BannerUrl = trainer.BannerUrl,
            PublicBannerUrl = trainer.BannerUrl,
            PublicBannerMediaId = trainer.PublicBannerMediaId,
            PrimaryColor = trainer.PrimaryColor,
            SecondaryColor = trainer.SecondaryColor,
            ShowInstagram = trainer.ShowInstagram,
            ShowTestimonials = trainer.ShowTestimonials,
            WelcomeMessage = trainer.WelcomeMessage,
            Stats = new PublicPageStatsDto
            {
                ActiveStudentsCount = activeStudentsCount,
                PostsCount = postsCount,
                TestimonialsCount = testimonials.Count,
                TransformationsCount = transformations.Count
            },
            RecentPosts = recentPosts.Select(p => new PublicPostItemDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                Tags = FeedBuilderService.ParseTags(p.Tags),
                PublishedAt = p.PublishedAt,
                CreatedAt = p.CreatedAt
            }).ToList(),
            Testimonials = trainer.ShowTestimonials
                ? testimonials.Select(t => new TestimonialPublicItem { Text = t.Text, Rating = t.Rating }).ToList()
                : new(),
            Transformations = transformations.Select(t => new TransformationPublicItem
            {
                BeforePhotoUrl = t.BeforePhotoUrl,
                AfterPhotoUrl = t.AfterPhotoUrl,
                Description = t.Description
            }).ToList(),
            ServiceOffers = offers.Select(o => new PublicServiceOfferResponse
            {
                Id = o.Id,
                Title = o.Title,
                Description = o.Description,
                Price = o.Price,
                BillingType = o.BillingType.ToString(),
                DurationDays = o.DurationDays
            }).ToList()
        });
    }

    public async Task<ApiResponse<PublicPageResponse>> GetTrainerSettingsAsync(Guid trainerId)
    {
        var trainer = await _db.Trainers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == trainerId);
        if (trainer == null) return ApiResponse<PublicPageResponse>.Fail("Trainer não encontrado.");

        _logger.LogInformation("Public page settings loaded. TrainerId={TrainerId}, PublicPageEnabled={PublicPageEnabled}, PublicSlug={PublicSlug}",
            trainerId, trainer.PublicPageEnabled, trainer.PublicSlug);

        return ApiResponse<PublicPageResponse>.Ok(MapTrainerSettings(trainer));
    }

    public async Task<ApiResponse<PublicPageResponse>> UpdatePageAsync(Guid trainerId, PublicPageRequest request)
    {
        var trainer = await _db.Trainers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == trainerId);
        if (trainer == null) return ApiResponse<PublicPageResponse>.Fail("Trainer não encontrado.");

        if (request.PublicPageEnabled && string.IsNullOrWhiteSpace(request.PublicSlug) && string.IsNullOrWhiteSpace(trainer.PublicSlug))
            return ApiResponse<PublicPageResponse>.Fail("Informe um slug para ativar a página pública.");

        if (!string.IsNullOrWhiteSpace(request.PublicSlug))
        {
            var slug = NormalizeSlug(request.PublicSlug);
            var exists = await _db.Trainers.AnyAsync(t => t.PublicSlug == slug && t.Id != trainerId);
            if (exists) return ApiResponse<PublicPageResponse>.Fail("Este slug já está em uso.");
            trainer.PublicSlug = slug;
        }

        _logger.LogInformation("Public page save requested. TrainerId={TrainerId}, PublicPageEnabled={PublicPageEnabled}, PublicSlug={PublicSlug}, PublicHeadline={PublicHeadline}",
            trainerId, request.PublicPageEnabled, request.PublicSlug, request.PublicHeadline);

        trainer.PublicPageEnabled = request.PublicPageEnabled;
        trainer.PublicSearchEnabled = request.PublicSearchEnabled;
        trainer.AcceptingStudents = request.AcceptingStudents;
        trainer.PublicHeadline = request.PublicHeadline;
        trainer.PublicDescription = request.PublicDescription;
        trainer.WhatsappNumber = request.WhatsappNumber;
        trainer.ShowInstagram = request.ShowInstagram;
        trainer.ShowTestimonials = request.ShowTestimonials;
        if (request.PublicBannerMediaId.HasValue)
        {
            var bannerMedia = await _db.MediaFiles.FirstOrDefaultAsync(m => m.Id == request.PublicBannerMediaId.Value);
            if (bannerMedia == null) return ApiResponse<PublicPageResponse>.Fail("MÃ­dia de banner nÃ£o encontrada.");
            trainer.PublicBannerMediaId = bannerMedia.Id;
            trainer.BannerUrl = bannerMedia.SecureUrl ?? bannerMedia.Url;
        }
        trainer.WelcomeMessage = request.WelcomeMessage;
        trainer.PrimaryColor = request.PrimaryColor ?? trainer.PrimaryColor;
        trainer.SecondaryColor = request.SecondaryColor ?? trainer.SecondaryColor;
        trainer.UpdatedAt = DateTime.UtcNow;

        var affected = await _db.SaveChangesAsync();
        await _privacyLgpdService.UpdateConsentAsync("PUBLIC_PROFILE_VISIBILITY", trainer.PublicPageEnabled, null, null, trainer.UserId);
        _logger.LogInformation("Public page save completed. TrainerId={TrainerId}, SaveChangesAffected={SaveChangesAffected}", trainerId, affected);

        return ApiResponse<PublicPageResponse>.Ok(MapTrainerSettings(trainer));
    }

    // ── Testimonials ─────────────────────────────────────────────────────────
    public async Task<ApiResponse<List<TestimonialResponse>>> GetTestimonialsAsync(Guid trainerId)
    {
        var list = await _db.StudentTestimonials.Include(t => t.Student).ThenInclude(s => s.User)
            .Where(t => t.TrainerId == trainerId).OrderByDescending(t => t.CreatedAt).ToListAsync();
        return ApiResponse<List<TestimonialResponse>>.Ok(list.Select(MapTestimonial).ToList());
    }

    public async Task<ApiResponse<TestimonialResponse>> CreateTestimonialAsync(Guid studentId, TestimonialRequest request, Guid trainerId)
    {
        if (!await _db.Students.AnyAsync(s => s.Id == studentId && s.TrainerId == trainerId))
            return ApiResponse<TestimonialResponse>.Fail("Aluno não encontrado.");

        var t = new StudentTestimonial { TrainerId = trainerId, StudentId = studentId, Text = request.Text, Rating = request.Rating };
        _db.StudentTestimonials.Add(t);
        await _db.SaveChangesAsync();
        await _db.Entry(t).Reference(x => x.Student).LoadAsync();
        await _db.Entry(t.Student).Reference(s => s.User).LoadAsync();
        return ApiResponse<TestimonialResponse>.Ok(MapTestimonial(t));
    }

    public async Task<ApiResponse<TestimonialResponse>> UpdateTestimonialAsync(Guid id, TestimonialRequest request, Guid trainerId)
    {
        var t = await _db.StudentTestimonials.Include(x => x.Student).ThenInclude(s => s.User)
            .FirstOrDefaultAsync(x => x.Id == id && x.TrainerId == trainerId);
        if (t == null) return ApiResponse<TestimonialResponse>.Fail("Depoimento não encontrado.");
        t.Text = request.Text; t.Rating = request.Rating; t.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ApiResponse<TestimonialResponse>.Ok(MapTestimonial(t));
    }

    public async Task<ApiResponse<TestimonialResponse>> ApproveTestimonialAsync(Guid id, Guid studentId, bool approve)
    {
        var t = await _db.StudentTestimonials.Include(x => x.Student).ThenInclude(s => s.User)
            .FirstOrDefaultAsync(x => x.Id == id && x.StudentId == studentId);
        if (t == null) return ApiResponse<TestimonialResponse>.Fail("Depoimento não encontrado.");
        t.ApprovedByStudent = approve; t.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ApiResponse<TestimonialResponse>.Ok(MapTestimonial(t));
    }

    public async Task<ApiResponse<List<TestimonialResponse>>> GetStudentTestimonialsAsync(Guid studentId)
    {
        var list = await _db.StudentTestimonials.Include(t => t.Student).ThenInclude(s => s.User)
            .Where(t => t.StudentId == studentId).ToListAsync();
        return ApiResponse<List<TestimonialResponse>>.Ok(list.Select(MapTestimonial).ToList());
    }

    public async Task<ApiResponse<List<TransformationResponse>>> GetTransformationsAsync(Guid trainerId)
    {
        var list = await _db.StudentTransformations.Include(t => t.Student).ThenInclude(s => s.User)
            .Where(t => t.TrainerId == trainerId).OrderByDescending(t => t.CreatedAt).ToListAsync();
        return ApiResponse<List<TransformationResponse>>.Ok(list.Select(MapTransformation).ToList());
    }

    public async Task<ApiResponse<TransformationResponse>> CreateTransformationAsync(Guid studentId, TransformationRequest request, Guid trainerId)
    {
        if (!await _db.Students.AnyAsync(s => s.Id == studentId && s.TrainerId == trainerId))
            return ApiResponse<TransformationResponse>.Fail("Aluno não encontrado.");

        string? beforePhotoUrl = null;
        string? afterPhotoUrl = null;
        if (request.BeforeMediaId.HasValue)
        {
            var beforeMedia = await _db.MediaFiles.FirstOrDefaultAsync(m => m.Id == request.BeforeMediaId.Value);
            if (beforeMedia == null) return ApiResponse<TransformationResponse>.Fail("MÃ­dia 'antes' nÃ£o encontrada.");
            beforePhotoUrl = beforeMedia.SecureUrl ?? beforeMedia.Url;
        }
        if (request.AfterMediaId.HasValue)
        {
            var afterMedia = await _db.MediaFiles.FirstOrDefaultAsync(m => m.Id == request.AfterMediaId.Value);
            if (afterMedia == null) return ApiResponse<TransformationResponse>.Fail("MÃ­dia 'depois' nÃ£o encontrada.");
            afterPhotoUrl = afterMedia.SecureUrl ?? afterMedia.Url;
        }
        if (!request.BeforeMediaId.HasValue && !request.AfterMediaId.HasValue)
            return ApiResponse<TransformationResponse>.Fail("Informe ao menos uma mÃ­dia (before/after).");

        var t = new StudentTransformation
        {
            TrainerId = trainerId,
            StudentId = studentId,
            BeforeMediaId = request.BeforeMediaId,
            AfterMediaId = request.AfterMediaId,
            BeforePhotoUrl = beforePhotoUrl,
            AfterPhotoUrl = afterPhotoUrl,
            Description = request.Description
        };
        _db.StudentTransformations.Add(t);
        await _db.SaveChangesAsync();
        await _db.Entry(t).Reference(x => x.Student).LoadAsync();
        await _db.Entry(t.Student).Reference(s => s.User).LoadAsync();
        return ApiResponse<TransformationResponse>.Ok(MapTransformation(t));
    }

    public async Task<ApiResponse<TransformationResponse>> ApproveTransformationAsync(Guid id, Guid studentId, bool approve)
    {
        var t = await _db.StudentTransformations.Include(x => x.Student).ThenInclude(s => s.User)
            .FirstOrDefaultAsync(x => x.Id == id && x.StudentId == studentId);
        if (t == null) return ApiResponse<TransformationResponse>.Fail("Transformação não encontrada.");
        t.ApprovedByStudent = approve; t.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ApiResponse<TransformationResponse>.Ok(MapTransformation(t));
    }

    private static string NormalizeSlug(string slug)
    {
        var normalized = slug.Trim().ToLowerInvariant();
        normalized = normalized.Replace(" ", "-");
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"[^a-z0-9\-]", string.Empty);
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\-{2,}", "-").Trim('-');
        return normalized;
    }

    private static PublicPageResponse MapTrainerSettings(Trainer trainer) => new()
    {
        TrainerId = trainer.Id,
        FullName = trainer.User.Name,
        BrandName = trainer.BrandName,
        PublicSlug = trainer.PublicSlug,
        PublicPageEnabled = trainer.PublicPageEnabled,
        PublicSearchEnabled = trainer.PublicSearchEnabled,
        AcceptingStudents = trainer.AcceptingStudents,
        PublicHeadline = trainer.PublicHeadline,
        PublicDescription = trainer.PublicDescription,
        Bio = trainer.Bio,
        Specialties = trainer.Specialties,
        Instagram = trainer.ShowInstagram ? trainer.Instagram : null,
        WhatsappNumber = trainer.WhatsappNumber,
        ProfilePhotoUrl = trainer.ProfilePhotoUrl,
        LogoUrl = trainer.LogoUrl,
        BannerUrl = trainer.BannerUrl,
        PublicBannerUrl = trainer.BannerUrl,
        PublicBannerMediaId = trainer.PublicBannerMediaId,
        PrimaryColor = trainer.PrimaryColor,
        SecondaryColor = trainer.SecondaryColor,
        ShowInstagram = trainer.ShowInstagram,
        ShowTestimonials = trainer.ShowTestimonials,
        WelcomeMessage = trainer.WelcomeMessage
    };

    private static TestimonialResponse MapTestimonial(StudentTestimonial t) => new()
    {
        Id = t.Id, StudentId = t.StudentId, StudentName = t.Student?.User?.Name ?? "",
        Text = t.Text, Rating = t.Rating, ApprovedByStudent = t.ApprovedByStudent,
        Published = t.Published, CreatedAt = t.CreatedAt
    };

    private static TransformationResponse MapTransformation(StudentTransformation t) => new()
    {
        Id = t.Id, StudentId = t.StudentId, StudentName = t.Student?.User?.Name ?? "",
        BeforePhotoUrl = t.BeforePhotoUrl, AfterPhotoUrl = t.AfterPhotoUrl, BeforeMediaId = t.BeforeMediaId, AfterMediaId = t.AfterMediaId, Description = t.Description,
        ApprovedByStudent = t.ApprovedByStudent, Published = t.Published, CreatedAt = t.CreatedAt
    };
}
