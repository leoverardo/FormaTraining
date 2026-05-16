using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Progress;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class StudentProgressService
{
    private readonly AppDbContext _db;

    public StudentProgressService(AppDbContext db) => _db = db;

    // ─── Progress (trainer) ──────────────────────────────────────────────────

    public async Task<ApiResponse<List<StudentProgressResponse>>> GetProgressByStudentAsync(Guid studentId, Guid trainerId)
    {
        if (!await _db.Students.AnyAsync(s => s.Id == studentId && s.TrainerId == trainerId))
            return ApiResponse<List<StudentProgressResponse>>.Fail("Aluno não encontrado.");

        var records = await _db.StudentProgressRecords
            .Where(p => p.StudentId == studentId && p.TrainerId == trainerId)
            .OrderByDescending(p => p.ProgressDate)
            .ToListAsync();

        return ApiResponse<List<StudentProgressResponse>>.Ok(records.Select(MapProgress).ToList());
    }

    public async Task<ApiResponse<StudentProgressResponse>> CreateProgressForStudentAsync(Guid studentId, StudentProgressRequest request, Guid trainerId, Guid createdByUserId)
    {
        var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == studentId && s.TrainerId == trainerId);
        if (student == null) return ApiResponse<StudentProgressResponse>.Fail("Aluno não encontrado.");

        var record = BuildProgress(request, studentId, trainerId, createdByUserId, ProgressCreatedByRole.Trainer);
        _db.StudentProgressRecords.Add(record);
        await _db.SaveChangesAsync();
        return ApiResponse<StudentProgressResponse>.Ok(MapProgress(record));
    }

    public async Task<ApiResponse<StudentProgressResponse>> UpdateProgressAsync(Guid studentId, Guid progressId, StudentProgressRequest request, Guid trainerId)
    {
        var record = await _db.StudentProgressRecords.FirstOrDefaultAsync(p => p.Id == progressId && p.StudentId == studentId && p.TrainerId == trainerId);
        if (record == null) return ApiResponse<StudentProgressResponse>.Fail("Registro não encontrado.");

        ApplyProgressUpdate(record, request);
        await _db.SaveChangesAsync();
        return ApiResponse<StudentProgressResponse>.Ok(MapProgress(record));
    }

    public async Task<ApiResponse> DeleteProgressAsync(Guid studentId, Guid progressId, Guid trainerId)
    {
        var record = await _db.StudentProgressRecords.FirstOrDefaultAsync(p => p.Id == progressId && p.StudentId == studentId && p.TrainerId == trainerId);
        if (record == null) return ApiResponse.Fail("Registro não encontrado.");
        _db.StudentProgressRecords.Remove(record);
        await _db.SaveChangesAsync();
        return ApiResponse.Ok("Registro removido.");
    }

    // ─── Progress (student) ──────────────────────────────────────────────────

    public async Task<ApiResponse<List<StudentProgressResponse>>> GetOwnProgressAsync(Guid studentId)
    {
        var records = await _db.StudentProgressRecords
            .Where(p => p.StudentId == studentId)
            .OrderByDescending(p => p.ProgressDate)
            .ToListAsync();
        return ApiResponse<List<StudentProgressResponse>>.Ok(records.Select(MapProgress).ToList());
    }

    public async Task<ApiResponse<StudentProgressResponse>> CreateOwnProgressAsync(Guid studentId, StudentProgressRequest request, Guid userId)
    {
        var student = await _db.Students.Include(s => s.Trainer).ThenInclude(t => t.User)
            .FirstOrDefaultAsync(s => s.Id == studentId);
        if (student == null) return ApiResponse<StudentProgressResponse>.Fail("Aluno não encontrado.");

        var record = BuildProgress(request, studentId, student.TrainerId, userId, ProgressCreatedByRole.Student);
        _db.StudentProgressRecords.Add(record);
        await _db.SaveChangesAsync();

        // Notify trainer
        var studentUser = await _db.Users.FindAsync(userId);
        _db.Notifications.Add(new Notification
        {
            UserId = student.Trainer.UserId,
            TrainerId = student.TrainerId,
            StudentId = studentId,
            Title = "Novo progresso registrado",
            Message = $"{studentUser?.Name ?? "Aluno"} registrou novo progresso.",
            Type = NotificationType.ProgressSubmitted
        });
        await _db.SaveChangesAsync();

        return ApiResponse<StudentProgressResponse>.Ok(MapProgress(record));
    }

    public async Task<ApiResponse<StudentProgressResponse>> UpdateOwnProgressAsync(Guid studentId, Guid progressId, StudentProgressRequest request)
    {
        var record = await _db.StudentProgressRecords.FirstOrDefaultAsync(p => p.Id == progressId && p.StudentId == studentId);
        if (record == null) return ApiResponse<StudentProgressResponse>.Fail("Registro não encontrado.");

        ApplyProgressUpdate(record, request);
        await _db.SaveChangesAsync();
        return ApiResponse<StudentProgressResponse>.Ok(MapProgress(record));
    }

    public async Task<ApiResponse> DeleteOwnProgressAsync(Guid studentId, Guid progressId)
    {
        var record = await _db.StudentProgressRecords.FirstOrDefaultAsync(p => p.Id == progressId && p.StudentId == studentId);
        if (record == null) return ApiResponse.Fail("Registro não encontrado.");
        _db.StudentProgressRecords.Remove(record);
        await _db.SaveChangesAsync();
        return ApiResponse.Ok("Registro removido.");
    }

    // ─── Photos (trainer) ────────────────────────────────────────────────────

    public async Task<ApiResponse<List<StudentProgressPhotoResponse>>> GetPhotosByStudentAsync(Guid studentId, Guid trainerId)
    {
        if (!await _db.Students.AnyAsync(s => s.Id == studentId && s.TrainerId == trainerId))
            return ApiResponse<List<StudentProgressPhotoResponse>>.Fail("Aluno não encontrado.");

        var photos = await _db.StudentProgressPhotos
            .Where(p => p.StudentId == studentId && p.TrainerId == trainerId)
            .OrderByDescending(p => p.PhotoDate)
            .ToListAsync();
        return ApiResponse<List<StudentProgressPhotoResponse>>.Ok(photos.Select(MapPhoto).ToList());
    }

    public async Task<ApiResponse<StudentProgressPhotoResponse>> AddPhotoForStudentAsync(Guid studentId, StudentProgressPhotoRequest request, Guid trainerId, Guid userId)
    {
        var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == studentId && s.TrainerId == trainerId);
        if (student == null) return ApiResponse<StudentProgressPhotoResponse>.Fail("Aluno não encontrado.");

        var media = await _db.MediaFiles.FirstOrDefaultAsync(m => m.Id == request.MediaAssetId);
        if (media == null) return ApiResponse<StudentProgressPhotoResponse>.Fail("MÃ­dia nÃ£o encontrada.");

        var photo = new StudentProgressPhoto
        {
            StudentId = studentId, TrainerId = trainerId,
            ImageUrl = media.SecureUrl ?? media.Url, MediaAssetId = media.Id, Description = request.Description,
            PhotoDate = request.PhotoDate ?? DateTime.UtcNow,
            CreatedByUserId = userId, CreatedByRole = ProgressCreatedByRole.Trainer
        };
        _db.StudentProgressPhotos.Add(photo);
        await _db.SaveChangesAsync();
        return ApiResponse<StudentProgressPhotoResponse>.Ok(MapPhoto(photo));
    }

    public async Task<ApiResponse> DeletePhotoForStudentAsync(Guid studentId, Guid photoId, Guid trainerId)
    {
        var photo = await _db.StudentProgressPhotos.FirstOrDefaultAsync(p => p.Id == photoId && p.StudentId == studentId && p.TrainerId == trainerId);
        if (photo == null) return ApiResponse.Fail("Foto não encontrada.");
        _db.StudentProgressPhotos.Remove(photo);
        await _db.SaveChangesAsync();
        return ApiResponse.Ok("Foto removida.");
    }

    // ─── Photos (student) ────────────────────────────────────────────────────

    public async Task<ApiResponse<List<StudentProgressPhotoResponse>>> GetOwnPhotosAsync(Guid studentId)
    {
        var photos = await _db.StudentProgressPhotos
            .Where(p => p.StudentId == studentId)
            .OrderByDescending(p => p.PhotoDate)
            .ToListAsync();
        return ApiResponse<List<StudentProgressPhotoResponse>>.Ok(photos.Select(MapPhoto).ToList());
    }

    public async Task<ApiResponse<StudentProgressPhotoResponse>> AddOwnPhotoAsync(Guid studentId, StudentProgressPhotoRequest request, Guid userId)
    {
        var student = await _db.Students.Include(s => s.Trainer)
            .FirstOrDefaultAsync(s => s.Id == studentId);
        if (student == null) return ApiResponse<StudentProgressPhotoResponse>.Fail("Aluno não encontrado.");

        var media = await _db.MediaFiles.FirstOrDefaultAsync(m => m.Id == request.MediaAssetId);
        if (media == null) return ApiResponse<StudentProgressPhotoResponse>.Fail("MÃ­dia nÃ£o encontrada.");

        var photo = new StudentProgressPhoto
        {
            StudentId = studentId, TrainerId = student.TrainerId,
            ImageUrl = media.SecureUrl ?? media.Url, MediaAssetId = media.Id, Description = request.Description,
            PhotoDate = request.PhotoDate ?? DateTime.UtcNow,
            CreatedByUserId = userId, CreatedByRole = ProgressCreatedByRole.Student
        };
        _db.StudentProgressPhotos.Add(photo);
        await _db.SaveChangesAsync();

        // Notify trainer
        var studentUser = await _db.Users.FindAsync(userId);
        _db.Notifications.Add(new Notification
        {
            UserId = student.Trainer.UserId,
            TrainerId = student.TrainerId,
            StudentId = studentId,
            Title = "Nova foto de progresso",
            Message = $"{studentUser?.Name ?? "Aluno"} enviou uma nova foto de progresso.",
            Type = NotificationType.ProgressSubmitted
        });
        await _db.SaveChangesAsync();

        return ApiResponse<StudentProgressPhotoResponse>.Ok(MapPhoto(photo));
    }

    public async Task<ApiResponse> DeleteOwnPhotoAsync(Guid studentId, Guid photoId)
    {
        var photo = await _db.StudentProgressPhotos.FirstOrDefaultAsync(p => p.Id == photoId && p.StudentId == studentId);
        if (photo == null) return ApiResponse.Fail("Foto não encontrada.");
        _db.StudentProgressPhotos.Remove(photo);
        await _db.SaveChangesAsync();
        return ApiResponse.Ok("Foto removida.");
    }

    // ─── Progress Summary ────────────────────────────────────────────────────

    public async Task<ApiResponse<ProgressSummaryDto>> GetProgressSummaryAsync(Guid studentId, Guid? callerTrainerId = null)
    {
        if (callerTrainerId.HasValue && !await _db.Students.AnyAsync(s => s.Id == studentId && s.TrainerId == callerTrainerId))
            return ApiResponse<ProgressSummaryDto>.Fail("Aluno não encontrado.");

        var records = await _db.StudentProgressRecords
            .Where(p => p.StudentId == studentId)
            .OrderByDescending(p => p.ProgressDate)
            .ToListAsync();

        var latest = records.FirstOrDefault();
        var previous = records.Skip(1).FirstOrDefault();

        ProgressComparisonDto? comparison = null;
        if (latest != null && previous != null)
        {
            comparison = new ProgressComparisonDto
            {
                WeightDifference = latest.Weight - previous.Weight,
                HeightDifference = latest.Height - previous.Height,
                WaistDifference = latest.Waist - previous.Waist,
                ChestDifference = latest.Chest - previous.Chest,
                AbdomenDifference = latest.Abdomen - previous.Abdomen,
                HipDifference = latest.Hip - previous.Hip,
                BodyFatDifference = latest.BodyFatPercentage - previous.BodyFatPercentage,
                ComparedToDate = previous.ProgressDate
            };
        }

        var timeline = records.Select(r => new ProgressTimelineItemDto
        {
            Id = r.Id,
            ProgressDate = r.ProgressDate,
            Weight = r.Weight,
            Waist = r.Waist,
            Chest = r.Chest,
            BodyFatPercentage = r.BodyFatPercentage
        }).ToList();

        var dto = new ProgressSummaryDto
        {
            LatestProgress = latest != null ? MapToRecordDto(latest) : null,
            PreviousProgress = previous != null ? MapToRecordDto(previous) : null,
            Comparison = comparison,
            Timeline = timeline
        };

        return ApiResponse<ProgressSummaryDto>.Ok(dto);
    }

    // ─── Photo Compare ────────────────────────────────────────────────────────

    public async Task<ApiResponse<ProgressPhotoCompareDto>> GetPhotoCompareAsync(Guid studentId, Guid? callerTrainerId = null)
    {
        if (callerTrainerId.HasValue && !await _db.Students.AnyAsync(s => s.Id == studentId && s.TrainerId == callerTrainerId))
            return ApiResponse<ProgressPhotoCompareDto>.Fail("Aluno não encontrado.");

        var photos = await _db.StudentProgressPhotos
            .Where(p => p.StudentId == studentId)
            .OrderBy(p => p.PhotoDate)
            .ToListAsync();

        var allPhotos = photos.Select(MapToPhotoItem).ToList();

        var dto = new ProgressPhotoCompareDto
        {
            BeforePhoto = allPhotos.FirstOrDefault(),
            AfterPhoto = allPhotos.LastOrDefault(),
            TotalPhotos = allPhotos.Count,
            FirstPhotoDate = allPhotos.FirstOrDefault()?.PhotoDate,
            LastPhotoDate = allPhotos.LastOrDefault()?.PhotoDate,
            AllPhotos = allPhotos
        };

        return ApiResponse<ProgressPhotoCompareDto>.Ok(dto);
    }

    private static ProgressRecordDto MapToRecordDto(StudentProgress p) => new()
    {
        Id = p.Id,
        ProgressDate = p.ProgressDate,
        Weight = p.Weight, Height = p.Height, Chest = p.Chest,
        Waist = p.Waist, Abdomen = p.Abdomen, Hip = p.Hip,
        RightArm = p.RightArm, LeftArm = p.LeftArm,
        RightThigh = p.RightThigh, LeftThigh = p.LeftThigh,
        BodyFatPercentage = p.BodyFatPercentage, Notes = p.Notes,
        CreatedAt = p.CreatedAt
    };

    private static ProgressPhotoItemDto MapToPhotoItem(StudentProgressPhoto p) => new()
    {
        Id = p.Id,
        ImageUrl = p.ImageUrl,
        MediaAssetId = p.MediaAssetId,
        Description = p.Description,
        PhotoDate = p.PhotoDate,
        CreatedByRole = p.CreatedByRole.ToString(),
        CreatedAt = p.CreatedAt
    };

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static StudentProgress BuildProgress(StudentProgressRequest req, Guid studentId, Guid trainerId, Guid userId, ProgressCreatedByRole role) => new()
    {
        StudentId = studentId, TrainerId = trainerId,
        Weight = req.Weight, Height = req.Height, Chest = req.Chest,
        Waist = req.Waist, Abdomen = req.Abdomen, Hip = req.Hip,
        RightArm = req.RightArm, LeftArm = req.LeftArm,
        RightThigh = req.RightThigh, LeftThigh = req.LeftThigh,
        BodyFatPercentage = req.BodyFatPercentage, Notes = req.Notes,
        ProgressDate = req.ProgressDate ?? DateTime.UtcNow,
        CreatedByUserId = userId, CreatedByRole = role
    };

    private static void ApplyProgressUpdate(StudentProgress r, StudentProgressRequest req)
    {
        r.Weight = req.Weight; r.Height = req.Height; r.Chest = req.Chest;
        r.Waist = req.Waist; r.Abdomen = req.Abdomen; r.Hip = req.Hip;
        r.RightArm = req.RightArm; r.LeftArm = req.LeftArm;
        r.RightThigh = req.RightThigh; r.LeftThigh = req.LeftThigh;
        r.BodyFatPercentage = req.BodyFatPercentage; r.Notes = req.Notes;
        if (req.ProgressDate.HasValue) r.ProgressDate = req.ProgressDate.Value;
        r.UpdatedAt = DateTime.UtcNow;
    }

    public static StudentProgressResponse MapProgress(StudentProgress p) => new()
    {
        Id = p.Id, StudentId = p.StudentId,
        Weight = p.Weight, Height = p.Height, Chest = p.Chest,
        Waist = p.Waist, Abdomen = p.Abdomen, Hip = p.Hip,
        RightArm = p.RightArm, LeftArm = p.LeftArm,
        RightThigh = p.RightThigh, LeftThigh = p.LeftThigh,
        BodyFatPercentage = p.BodyFatPercentage, Notes = p.Notes,
        ProgressDate = p.ProgressDate, CreatedByRole = p.CreatedByRole.ToString(),
        CreatedAt = p.CreatedAt
    };

    public static StudentProgressPhotoResponse MapPhoto(StudentProgressPhoto p) => new()
    {
        Id = p.Id, StudentId = p.StudentId,
        ImageUrl = p.ImageUrl, MediaAssetId = p.MediaAssetId, Description = p.Description,
        PhotoDate = p.PhotoDate, CreatedByRole = p.CreatedByRole.ToString(),
        CreatedAt = p.CreatedAt
    };
}
