using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Anamnesis;
using FitPlatform.Domain.Entities;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class AnamnesisService
{
    private readonly AppDbContext _db;

    public AnamnesisService(AppDbContext db) => _db = db;

    public async Task<ApiResponse<AnamnesisResponse>> GetByStudentAsync(Guid studentId, Guid trainerId)
    {
        if (!await _db.Students.AnyAsync(s => s.Id == studentId && s.TrainerId == trainerId))
            return ApiResponse<AnamnesisResponse>.Fail("Aluno não encontrado.");

        var a = await _db.StudentAnamnesisRecords.FirstOrDefaultAsync(x => x.StudentId == studentId);
        if (a == null) return ApiResponse<AnamnesisResponse>.Fail("Anamnese não preenchida.");
        return ApiResponse<AnamnesisResponse>.Ok(MapResponse(a));
    }

    public async Task<ApiResponse<AnamnesisResponse>> GetOwnAsync(Guid studentId)
    {
        var a = await _db.StudentAnamnesisRecords.FirstOrDefaultAsync(x => x.StudentId == studentId);
        if (a == null) return ApiResponse<AnamnesisResponse>.Fail("Anamnese não preenchida.");
        return ApiResponse<AnamnesisResponse>.Ok(MapResponse(a));
    }

    public async Task<ApiResponse<AnamnesisResponse>> SaveOwnAsync(Guid studentId, AnamnesisRequest request)
    {
        var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == studentId);
        if (student == null) return ApiResponse<AnamnesisResponse>.Fail("Aluno não encontrado.");

        var a = await _db.StudentAnamnesisRecords.FirstOrDefaultAsync(x => x.StudentId == studentId);
        if (a == null)
        {
            a = new StudentAnamnesis { StudentId = studentId, TrainerId = student.TrainerId };
            _db.StudentAnamnesisRecords.Add(a);
        }

        a.MainGoal = request.MainGoal; a.TrainingExperience = request.TrainingExperience;
        a.Injuries = request.Injuries; a.HealthRestrictions = request.HealthRestrictions;
        a.AvailableDaysPerWeek = request.AvailableDaysPerWeek; a.TrainingLocation = request.TrainingLocation;
        a.AvailableEquipment = request.AvailableEquipment; a.SleepQuality = request.SleepQuality;
        a.StressLevel = request.StressLevel; a.FoodRoutineNotes = request.FoodRoutineNotes;
        a.AdditionalNotes = request.AdditionalNotes;
        a.SubmittedAt = DateTime.UtcNow; a.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ApiResponse<AnamnesisResponse>.Ok(MapResponse(a));
    }

    private static AnamnesisResponse MapResponse(StudentAnamnesis a) => new()
    {
        Id = a.Id, MainGoal = a.MainGoal, TrainingExperience = a.TrainingExperience,
        Injuries = a.Injuries, HealthRestrictions = a.HealthRestrictions,
        AvailableDaysPerWeek = a.AvailableDaysPerWeek, TrainingLocation = a.TrainingLocation,
        AvailableEquipment = a.AvailableEquipment, SleepQuality = a.SleepQuality,
        StressLevel = a.StressLevel, FoodRoutineNotes = a.FoodRoutineNotes,
        AdditionalNotes = a.AdditionalNotes, SubmittedAt = a.SubmittedAt, UpdatedAt = a.UpdatedAt
    };
}
