using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Leads;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class TrainerLeadService
{
    private readonly AppDbContext _db;

    public TrainerLeadService(AppDbContext db) => _db = db;

    public async Task<ApiResponse<TrainerLeadResponse>> CreateByTrainerIdAsync(Guid trainerId, CreateTrainerLeadRequest request, Guid? studentProfileId)
    {
        var trainer = await _db.Trainers.FirstOrDefaultAsync(t => t.Id == trainerId && t.PublicPageEnabled);
        if (trainer == null) return ApiResponse<TrainerLeadResponse>.Fail("Trainer não encontrado.");

        var lead = new TrainerLead
        {
            TrainerId = trainerId,
            StudentProfileId = studentProfileId,
            Name = request.Name,
            Email = request.Email.ToLower(),
            Phone = request.Phone,
            Goal = request.Goal,
            Message = request.Message,
            Source = request.Source,
            Status = TrainerLeadStatus.New
        };
        _db.TrainerLeads.Add(lead);
        await _db.SaveChangesAsync();
        return ApiResponse<TrainerLeadResponse>.Ok(Map(lead), "Interesse enviado com sucesso.");
    }

    public async Task<ApiResponse<TrainerLeadResponse>> CreateBySlugAsync(string slug, CreateTrainerLeadRequest request, Guid? studentProfileId)
    {
        var trainer = await _db.Trainers.FirstOrDefaultAsync(t => t.PublicSlug == slug && t.PublicPageEnabled);
        if (trainer == null) return ApiResponse<TrainerLeadResponse>.Fail("Trainer não encontrado.");
        return await CreateByTrainerIdAsync(trainer.Id, request, studentProfileId);
    }

    public async Task<ApiResponse<List<TrainerLeadResponse>>> GetForTrainerAsync(Guid trainerId)
    {
        var leads = await _db.TrainerLeads
            .AsNoTracking()
            .Where(l => l.TrainerId == trainerId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
        return ApiResponse<List<TrainerLeadResponse>>.Ok(leads.Select(Map).ToList());
    }

    public async Task<ApiResponse<TrainerLeadResponse>> UpdateStatusAsync(Guid trainerId, Guid leadId, TrainerLeadStatus status)
    {
        var lead = await _db.TrainerLeads.FirstOrDefaultAsync(l => l.Id == leadId && l.TrainerId == trainerId);
        if (lead == null) return ApiResponse<TrainerLeadResponse>.Fail("Lead não encontrado.");
        lead.Status = status;
        lead.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ApiResponse<TrainerLeadResponse>.Ok(Map(lead));
    }

    public async Task<ApiResponse<object>> ConvertToStudentAsync(Guid trainerId, Guid leadId)
    {
        var lead = await _db.TrainerLeads.FirstOrDefaultAsync(l => l.Id == leadId && l.TrainerId == trainerId);
        if (lead == null) return ApiResponse<object>.Fail("Lead não encontrado.");

        StudentProfile? profile = null;
        if (lead.StudentProfileId.HasValue)
        {
            profile = await _db.StudentProfiles.FirstOrDefaultAsync(s => s.Id == lead.StudentProfileId.Value);
        }

        if (profile == null)
        {
            profile = await _db.StudentProfiles.FirstOrDefaultAsync(s => s.User.Email == lead.Email);
        }

        if (profile == null)
            return ApiResponse<object>.Fail("Lead sem perfil de aluno explorador vinculado.");

        var existingStudent = await _db.Students.FirstOrDefaultAsync(s => s.UserId == profile.UserId && s.TrainerId == trainerId);
        if (existingStudent != null)
            return ApiResponse<object>.Fail("Aluno já vinculado a este trainer.");

        var sub = await _db.TrainerSubscriptions
            .Include(x => x.PlatformPlan)
            .Where(x => x.TrainerId == trainerId && x.Status == TrainerSubscriptionStatus.Active)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (sub == null) return ApiResponse<object>.Fail("Nenhuma assinatura ativa para vincular aluno.");

        var activeCount = await _db.Students.CountAsync(s => s.TrainerId == trainerId && s.Status == StudentStatus.Active);
        if (activeCount >= sub.PlatformPlan.MaxActiveStudents)
            return ApiResponse<object>.Fail($"Limite do plano atingido ({sub.PlatformPlan.MaxActiveStudents} alunos ativos).");

        var student = new Student
        {
            UserId = profile.UserId,
            TrainerId = trainerId,
            Phone = profile.Phone,
            BirthDate = profile.BirthDate,
            Goal = profile.Goal,
            Status = StudentStatus.Active,
            Notes = "Vinculado via conversão de lead"
        };
        _db.Students.Add(student);

        profile.AccountStatus = StudentAccountStatus.Linked;
        profile.UpdatedAt = DateTime.UtcNow;

        lead.Status = TrainerLeadStatus.Converted;
        lead.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { studentId = student.Id, message = "Lead convertido em aluno com sucesso." });
    }

    private static TrainerLeadResponse Map(TrainerLead lead) => new()
    {
        Id = lead.Id,
        TrainerId = lead.TrainerId,
        StudentProfileId = lead.StudentProfileId,
        Name = lead.Name,
        Email = lead.Email,
        Phone = lead.Phone,
        Goal = lead.Goal,
        Message = lead.Message,
        Status = lead.Status.ToString(),
        Source = lead.Source.ToString(),
        CreatedAt = lead.CreatedAt
    };
}
