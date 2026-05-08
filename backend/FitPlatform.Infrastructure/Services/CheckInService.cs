using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.CheckIn;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class CheckInService
{
    private readonly AppDbContext _db;
    private readonly NotificationService _notifications;

    public CheckInService(AppDbContext db, NotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<ApiResponse<List<CheckInResponse>>> GetByStudentAsync(Guid studentId, Guid trainerId)
    {
        if (!await _db.Students.AnyAsync(s => s.Id == studentId && s.TrainerId == trainerId))
            return ApiResponse<List<CheckInResponse>>.Fail("Aluno não encontrado.");

        var items = await _db.StudentWeeklyCheckIns
            .Include(c => c.Student).ThenInclude(s => s.User)
            .Include(c => c.Comments).ThenInclude(pc => pc.Trainer).ThenInclude(t => t.User)
            .Where(c => c.StudentId == studentId && c.TrainerId == trainerId)
            .OrderByDescending(c => c.WeekStartDate)
            .ToListAsync();

        return ApiResponse<List<CheckInResponse>>.Ok(items.Select(MapResponse).ToList());
    }

    public async Task<ApiResponse<List<CheckInResponse>>> GetRecentForTrainerAsync(Guid trainerId, int limit = 10)
    {
        var items = await _db.StudentWeeklyCheckIns
            .Include(c => c.Student).ThenInclude(s => s.User)
            .Include(c => c.Comments)
            .Where(c => c.TrainerId == trainerId)
            .OrderByDescending(c => c.CreatedAt)
            .Take(limit)
            .ToListAsync();

        return ApiResponse<List<CheckInResponse>>.Ok(items.Select(MapResponse).ToList());
    }

    public async Task<ApiResponse<List<object>>> GetMissingCurrentWeekAsync(Guid trainerId)
    {
        var weekStart = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
        var activeStudents = await _db.Students.Include(s => s.User)
            .Where(s => s.TrainerId == trainerId && s.Status == StudentStatus.Active).ToListAsync();

        var checkedIn = await _db.StudentWeeklyCheckIns
            .Where(c => c.TrainerId == trainerId && c.WeekStartDate >= weekStart)
            .Select(c => c.StudentId).ToListAsync();

        var missing = activeStudents.Where(s => !checkedIn.Contains(s.Id))
            .Select(s => (object)new { s.Id, Name = s.User.Name, s.Status }).ToList();

        return ApiResponse<List<object>>.Ok(missing);
    }

    public async Task<ApiResponse<CheckInResponse>> GetOwnCurrentWeekAsync(Guid studentId)
    {
        var weekStart = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
        var item = await _db.StudentWeeklyCheckIns
            .Include(c => c.Student).ThenInclude(s => s.User)
            .Include(c => c.Comments)
            .FirstOrDefaultAsync(c => c.StudentId == studentId && c.WeekStartDate >= weekStart);

        if (item == null) return ApiResponse<CheckInResponse>.Fail("Nenhum check-in esta semana.");
        return ApiResponse<CheckInResponse>.Ok(MapResponse(item));
    }

    public async Task<ApiResponse<List<CheckInResponse>>> GetOwnAsync(Guid studentId)
    {
        var items = await _db.StudentWeeklyCheckIns
            .Include(c => c.Student).ThenInclude(s => s.User)
            .Include(c => c.Comments)
            .Where(c => c.StudentId == studentId)
            .OrderByDescending(c => c.WeekStartDate)
            .ToListAsync();
        return ApiResponse<List<CheckInResponse>>.Ok(items.Select(MapResponse).ToList());
    }

    public async Task<ApiResponse<CheckInResponse>> CreateOwnAsync(Guid studentId, CheckInRequest request)
    {
        var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == studentId);
        if (student == null) return ApiResponse<CheckInResponse>.Fail("Aluno não encontrado.");

        var weekStart = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek).Date;
        if (await _db.StudentWeeklyCheckIns.AnyAsync(c => c.StudentId == studentId && c.WeekStartDate == weekStart))
            return ApiResponse<CheckInResponse>.Fail("Você já fez check-in esta semana.");

        var checkIn = new StudentWeeklyCheckIn
        {
            StudentId = studentId, TrainerId = student.TrainerId,
            WeekStartDate = weekStart, WeekEndDate = weekStart.AddDays(6),
            Weight = request.Weight, MoodLevel = request.MoodLevel,
            EnergyLevel = request.EnergyLevel, SleepQuality = request.SleepQuality,
            DietAdherence = request.DietAdherence, TrainingAdherence = request.TrainingAdherence,
            CompletedWorkoutsCount = request.CompletedWorkoutsCount,
            HasPain = request.HasPain, PainDescription = request.PainDescription,
            Notes = request.Notes, PhotoUrl = request.PhotoUrl
        };
        _db.StudentWeeklyCheckIns.Add(checkIn);
        await _db.SaveChangesAsync();

        await _db.Entry(checkIn).Reference(c => c.Student).LoadAsync();
        await _db.Entry(checkIn.Student).Reference(s => s.User).LoadAsync();

        // Notify trainer
        var trainerUser = await _db.Trainers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == student.TrainerId);
        if (trainerUser != null)
            await _notifications.CreateAsync(trainerUser.UserId, "Check-in recebido", $"{checkIn.Student.User.Name} enviou o check-in semanal.", NotificationType.CheckInSubmitted, student.TrainerId, studentId);

        return ApiResponse<CheckInResponse>.Ok(MapResponse(checkIn));
    }

    public async Task<ApiResponse<CheckInResponse>> UpdateOwnAsync(Guid checkInId, Guid studentId, CheckInRequest request)
    {
        var checkIn = await _db.StudentWeeklyCheckIns.Include(c => c.Student).ThenInclude(s => s.User)
            .FirstOrDefaultAsync(c => c.Id == checkInId && c.StudentId == studentId);
        if (checkIn == null) return ApiResponse<CheckInResponse>.Fail("Check-in não encontrado.");

        checkIn.Weight = request.Weight; checkIn.MoodLevel = request.MoodLevel;
        checkIn.EnergyLevel = request.EnergyLevel; checkIn.SleepQuality = request.SleepQuality;
        checkIn.DietAdherence = request.DietAdherence; checkIn.TrainingAdherence = request.TrainingAdherence;
        checkIn.CompletedWorkoutsCount = request.CompletedWorkoutsCount;
        checkIn.HasPain = request.HasPain; checkIn.PainDescription = request.PainDescription;
        checkIn.Notes = request.Notes; checkIn.PhotoUrl = request.PhotoUrl;
        checkIn.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ApiResponse<CheckInResponse>.Ok(MapResponse(checkIn));
    }

    public async Task<ApiResponse<CommentResponse>> AddCommentAsync(Guid checkInId, Guid trainerId, Guid studentId, string comment)
    {
        var checkIn = await _db.StudentWeeklyCheckIns.FirstOrDefaultAsync(c => c.Id == checkInId && c.TrainerId == trainerId);
        if (checkIn == null) return ApiResponse<CommentResponse>.Fail("Check-in não encontrado.");

        var trainer = await _db.Trainers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == trainerId);
        var pc = new ProgressComment { StudentWeeklyCheckInId = checkInId, TrainerId = trainerId, StudentId = studentId, Comment = comment };
        _db.ProgressComments.Add(pc);
        await _db.SaveChangesAsync();

        var studentUser = await _db.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == studentId);
        if (studentUser != null)
            await _notifications.CreateAsync(studentUser.UserId, "Comentário do seu personal", $"{trainer?.BrandName ?? "Seu personal"} comentou no seu check-in.", NotificationType.TrainerComment, trainerId, studentId);

        return ApiResponse<CommentResponse>.Ok(new CommentResponse { Id = pc.Id, Comment = pc.Comment, AuthorName = trainer?.User.Name ?? "", AuthorRole = "Trainer", CreatedAt = pc.CreatedAt });
    }

    public static CheckInResponse MapResponse(StudentWeeklyCheckIn c) => new()
    {
        Id = c.Id, StudentId = c.StudentId, StudentName = c.Student?.User?.Name ?? "",
        WeekStartDate = c.WeekStartDate, WeekEndDate = c.WeekEndDate,
        Weight = c.Weight, MoodLevel = c.MoodLevel, EnergyLevel = c.EnergyLevel,
        SleepQuality = c.SleepQuality, DietAdherence = c.DietAdherence,
        TrainingAdherence = c.TrainingAdherence, CompletedWorkoutsCount = c.CompletedWorkoutsCount,
        HasPain = c.HasPain, PainDescription = c.PainDescription, Notes = c.Notes,
        PhotoUrl = c.PhotoUrl, CreatedAt = c.CreatedAt,
        Comments = c.Comments.Select(pc => new CommentResponse { Id = pc.Id, Comment = pc.Comment, AuthorName = pc.Trainer?.User?.Name ?? "", AuthorRole = "Trainer", CreatedAt = pc.CreatedAt }).ToList()
    };
}
