using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.PublicPage;
using FitPlatform.Application.DTOs.Testimonials;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class PublicPageService
{
    private readonly AppDbContext _db;

    public PublicPageService(AppDbContext db) => _db = db;

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

        return ApiResponse<PublicPageResponse>.Ok(new PublicPageResponse
        {
            TrainerId = trainer.Id,
            FullName = trainer.User.Name,
            BrandName = trainer.BrandName,
            PublicSlug = trainer.PublicSlug,
            PublicPageEnabled = trainer.PublicPageEnabled,
            PublicHeadline = trainer.PublicHeadline,
            PublicDescription = trainer.PublicDescription,
            Bio = trainer.Bio,
            Specialties = trainer.Specialties,
            Instagram = trainer.ShowInstagram ? trainer.Instagram : null,
            WhatsappNumber = trainer.WhatsappNumber,
            ProfilePhotoUrl = trainer.ProfilePhotoUrl,
            LogoUrl = trainer.LogoUrl,
            BannerUrl = trainer.BannerUrl,
            PrimaryColor = trainer.PrimaryColor,
            SecondaryColor = trainer.SecondaryColor,
            ShowTestimonials = trainer.ShowTestimonials,
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
            }).ToList()
        });
    }

    public async Task<ApiResponse<PublicPageResponse>> UpdatePageAsync(Guid trainerId, PublicPageRequest request)
    {
        var trainer = await _db.Trainers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == trainerId);
        if (trainer == null) return ApiResponse<PublicPageResponse>.Fail("Trainer não encontrado.");

        if (!string.IsNullOrWhiteSpace(request.PublicSlug))
        {
            var slug = request.PublicSlug.Trim().ToLower().Replace(" ", "-");
            var exists = await _db.Trainers.AnyAsync(t => t.PublicSlug == slug && t.Id != trainerId);
            if (exists) return ApiResponse<PublicPageResponse>.Fail("Este slug já está em uso.");
            trainer.PublicSlug = slug;
        }

        trainer.PublicPageEnabled = request.PublicPageEnabled;
        trainer.PublicHeadline = request.PublicHeadline;
        trainer.PublicDescription = request.PublicDescription;
        trainer.WhatsappNumber = request.WhatsappNumber;
        trainer.ShowInstagram = request.ShowInstagram;
        trainer.ShowTestimonials = request.ShowTestimonials;
        trainer.BannerUrl = request.BannerUrl;
        trainer.WelcomeMessage = request.WelcomeMessage;
        trainer.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ApiResponse<PublicPageResponse>.Ok(new PublicPageResponse
        {
            TrainerId = trainer.Id, BrandName = trainer.BrandName, PublicSlug = trainer.PublicSlug,
            PublicPageEnabled = trainer.PublicPageEnabled, PublicHeadline = trainer.PublicHeadline,
            Bio = trainer.Bio, PrimaryColor = trainer.PrimaryColor
        });
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

        var t = new StudentTransformation { TrainerId = trainerId, StudentId = studentId, BeforePhotoUrl = request.BeforePhotoUrl, AfterPhotoUrl = request.AfterPhotoUrl, Description = request.Description };
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

    private static TestimonialResponse MapTestimonial(StudentTestimonial t) => new()
    {
        Id = t.Id, StudentId = t.StudentId, StudentName = t.Student?.User?.Name ?? "",
        Text = t.Text, Rating = t.Rating, ApprovedByStudent = t.ApprovedByStudent,
        Published = t.Published, CreatedAt = t.CreatedAt
    };

    private static TransformationResponse MapTransformation(StudentTransformation t) => new()
    {
        Id = t.Id, StudentId = t.StudentId, StudentName = t.Student?.User?.Name ?? "",
        BeforePhotoUrl = t.BeforePhotoUrl, AfterPhotoUrl = t.AfterPhotoUrl, Description = t.Description,
        ApprovedByStudent = t.ApprovedByStudent, Published = t.Published, CreatedAt = t.CreatedAt
    };
}
