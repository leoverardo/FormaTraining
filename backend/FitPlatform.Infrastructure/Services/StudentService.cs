using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Students;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class StudentService
{
    private readonly AppDbContext _db;
    private readonly PasswordSetupService _passwordSetup;

    public StudentService(AppDbContext db, PasswordSetupService passwordSetup)
    {
        _db = db;
        _passwordSetup = passwordSetup;
    }

    public async Task<ApiResponse<List<StudentResponse>>> GetAllAsync(Guid trainerId)
    {
        var students = await _db.Students
            .Include(s => s.User)
            .Where(s => s.TrainerId == trainerId)
            .OrderBy(s => s.User.Name)
            .ToListAsync();

        return ApiResponse<List<StudentResponse>>.Ok(students.Select(MapResponse).ToList());
    }

    public async Task<ApiResponse<StudentResponse>> GetByIdAsync(Guid id, Guid trainerId)
    {
        var student = await _db.Students.Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id && s.TrainerId == trainerId);

        if (student == null) return ApiResponse<StudentResponse>.Fail("Aluno não encontrado.");
        return ApiResponse<StudentResponse>.Ok(MapResponse(student));
    }

    public async Task<ApiResponse<StudentResponse>> CreateAsync(CreateStudentRequest request, Guid trainerId)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email.ToLower()))
            return ApiResponse<StudentResponse>.Fail("E-mail já está em uso.");

        if (request.ActiveOnCreate)
        {
            var limitCheck = await CheckStudentLimitAsync(trainerId);
            if (!limitCheck.Success) return ApiResponse<StudentResponse>.Fail(limitCheck.Message!);
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
            Role = UserRole.Student,
            IsActive = true,
            MustChangePassword = true
        };
        _db.Users.Add(user);

        var student = new Student
        {
            UserId = user.Id,
            TrainerId = trainerId,
            Phone = request.Phone,
            BirthDate = request.BirthDate,
            Goal = request.Goal,
            Notes = request.Notes,
            Status = request.ActiveOnCreate ? StudentStatus.Active : StudentStatus.Inactive
        };
        _db.Students.Add(student);
        await _db.SaveChangesAsync();

        var trainer = await _db.Trainers.FirstOrDefaultAsync(t => t.Id == trainerId);
        var brand = trainer?.BrandName ?? "seu personal trainer";

        // Fire-and-forget: send access email (logs to console in MVP)
        _ = Task.Run(async () =>
        {
            try { await _passwordSetup.GenerateAndSendSetupTokenAsync(user, isStudent: true, trainerBrand: brand); }
            catch { /* log in production */ }
        });

        await _db.Entry(student).Reference(s => s.User).LoadAsync();
        return ApiResponse<StudentResponse>.Ok(MapResponse(student), "Aluno cadastrado. Link de acesso enviado por e-mail.");
    }

    public async Task<ApiResponse<StudentResponse>> UpdateAsync(Guid id, UpdateStudentRequest request, Guid trainerId)
    {
        var student = await _db.Students.Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id && s.TrainerId == trainerId);

        if (student == null) return ApiResponse<StudentResponse>.Fail("Aluno não encontrado.");

        student.User.Name = request.Name;
        student.User.UpdatedAt = DateTime.UtcNow;
        student.Phone = request.Phone;
        student.BirthDate = request.BirthDate;
        student.Goal = request.Goal;
        student.Notes = request.Notes;
        student.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ApiResponse<StudentResponse>.Ok(MapResponse(student));
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, Guid trainerId)
    {
        var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == id && s.TrainerId == trainerId);
        if (student == null) return ApiResponse.Fail("Aluno não encontrado.");
        _db.Students.Remove(student);
        await _db.SaveChangesAsync();
        return ApiResponse.Ok("Aluno removido.");
    }

    public async Task<ApiResponse<StudentResponse>> ActivateAsync(Guid id, Guid trainerId)
    {
        var student = await _db.Students.Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id && s.TrainerId == trainerId);
        if (student == null) return ApiResponse<StudentResponse>.Fail("Aluno não encontrado.");

        var limitCheck = await CheckStudentLimitAsync(trainerId);
        if (!limitCheck.Success) return ApiResponse<StudentResponse>.Fail(limitCheck.Message!);

        student.Status = StudentStatus.Active;
        student.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ApiResponse<StudentResponse>.Ok(MapResponse(student));
    }

    public async Task<ApiResponse<StudentResponse>> DeactivateAsync(Guid id, Guid trainerId)
    {
        var student = await _db.Students.Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id && s.TrainerId == trainerId);
        if (student == null) return ApiResponse<StudentResponse>.Fail("Aluno não encontrado.");

        student.Status = StudentStatus.Inactive;
        student.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ApiResponse<StudentResponse>.Ok(MapResponse(student));
    }

    public async Task<ApiResponse> ResendAccessEmailAsync(Guid id, Guid trainerId)
    {
        var student = await _db.Students
            .Include(s => s.User)
            .Include(s => s.Trainer)
            .FirstOrDefaultAsync(s => s.Id == id && s.TrainerId == trainerId);
        if (student == null) return ApiResponse.Fail("Aluno não encontrado.");

        var brand = student.Trainer?.BrandName ?? "seu personal trainer";
        await _passwordSetup.GenerateAndSendSetupTokenAsync(student.User, isStudent: true, trainerBrand: brand);
        return ApiResponse.Ok("E-mail de acesso reenviado. Verifique o console.");
    }

    private async Task<ApiResponse> CheckStudentLimitAsync(Guid trainerId)
    {
        var subscription = await _db.TrainerSubscriptions
            .Include(ts => ts.PlatformPlan)
            .Where(ts => ts.TrainerId == trainerId && ts.Status == TrainerSubscriptionStatus.Active)
            .OrderByDescending(ts => ts.CreatedAt)
            .FirstOrDefaultAsync();

        if (subscription == null)
            return ApiResponse.Fail("Nenhuma assinatura ativa encontrada. Assine um plano para gerenciar alunos.");

        var activeCount = await _db.Students.CountAsync(s => s.TrainerId == trainerId && s.Status == StudentStatus.Active);
        if (!subscription.PlatformPlan.HasUnlimitedStudents && activeCount >= subscription.PlatformPlan.MaxActiveStudents)
            return ApiResponse.Fail($"Seu plano atual permite até {subscription.PlatformPlan.MaxActiveStudents} alunos ativos. Para adicionar mais alunos, faça upgrade do plano.");

        return ApiResponse.Ok();
    }

    private static StudentResponse MapResponse(Student s) => new()
    {
        Id = s.Id,
        UserId = s.UserId,
        Name = s.User.Name,
        Email = s.User.Email,
        Phone = s.Phone,
        BirthDate = s.BirthDate,
        Goal = s.Goal,
        Notes = s.Notes,
        Status = s.Status.ToString(),
        CreatedAt = s.CreatedAt
    };
}
