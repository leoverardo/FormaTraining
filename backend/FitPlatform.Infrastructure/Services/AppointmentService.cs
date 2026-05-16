using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Appointments;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class AppointmentService
{
    private readonly AppDbContext _db;
    private readonly NotificationService _notifications;

    public AppointmentService(AppDbContext db, NotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<ApiResponse<List<AppointmentResponse>>> GetTrainerAsync(Guid trainerId, AppointmentQuery query)
    {
        var q = _db.Appointments.Include(a => a.Student).ThenInclude(s => s!.User).Where(a => a.TrainerId == trainerId).AsQueryable();
        q = ApplyFilters(q, query);
        var items = await q.OrderBy(a => a.StartAt).Take(300).ToListAsync();
        return ApiResponse<List<AppointmentResponse>>.Ok(items.Select(Map).ToList());
    }

    public async Task<ApiResponse<AppointmentResponse>> GetTrainerByIdAsync(Guid trainerId, Guid id)
    {
        var item = await _db.Appointments.Include(a => a.Student).ThenInclude(s => s!.User).FirstOrDefaultAsync(a => a.Id == id && a.TrainerId == trainerId);
        return item == null ? ApiResponse<AppointmentResponse>.Fail("Compromisso não encontrado.") : ApiResponse<AppointmentResponse>.Ok(Map(item));
    }

    public async Task<ApiResponse<AppointmentResponse>> CreateAsync(Guid trainerId, AppointmentRequest request)
    {
        if (request.StartAt >= request.EndAt) return ApiResponse<AppointmentResponse>.Fail("Horário inválido.");
        Student? student = null;
        if (request.StudentId.HasValue)
        {
            student = await _db.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == request.StudentId.Value && s.TrainerId == trainerId);
            if (student == null) return ApiResponse<AppointmentResponse>.Fail("Aluno não encontrado.");
        }

        if (await HasConflictAsync(trainerId, request.StartAt, request.EndAt, null))
            return ApiResponse<AppointmentResponse>.Fail("Conflito de horário com outro compromisso.");

        var entity = new Appointment
        {
            TrainerId = trainerId,
            StudentId = request.StudentId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Type = ParseType(request.Type),
            Status = AppointmentStatus.Scheduled,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            Location = request.Location?.Trim(),
            OnlineMeetingUrl = request.OnlineMeetingUrl?.Trim()
        };
        _db.Appointments.Add(entity);
        await _db.SaveChangesAsync();

        if (student != null)
            await _notifications.CreateAsync(student.UserId, "Novo compromisso agendado", $"Seu personal agendou: {entity.Title}.", NotificationType.AppointmentCreated, trainerId, student.Id);

        entity.Student = student;
        return ApiResponse<AppointmentResponse>.Ok(Map(entity), "Compromisso criado.");
    }

    public async Task<ApiResponse<AppointmentResponse>> UpdateAsync(Guid trainerId, Guid id, AppointmentRequest request)
    {
        var entity = await _db.Appointments.Include(a => a.Student).ThenInclude(s => s!.User).FirstOrDefaultAsync(a => a.Id == id && a.TrainerId == trainerId);
        if (entity == null) return ApiResponse<AppointmentResponse>.Fail("Compromisso não encontrado.");
        if (entity.Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed) return ApiResponse<AppointmentResponse>.Fail("Compromisso finalizado não pode ser editado.");
        if (request.StartAt >= request.EndAt) return ApiResponse<AppointmentResponse>.Fail("Horário inválido.");
        if (await HasConflictAsync(trainerId, request.StartAt, request.EndAt, id))
            return ApiResponse<AppointmentResponse>.Fail("Conflito de horário com outro compromisso.");

        Student? student = null;
        if (request.StudentId.HasValue)
        {
            student = await _db.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == request.StudentId.Value && s.TrainerId == trainerId);
            if (student == null) return ApiResponse<AppointmentResponse>.Fail("Aluno não encontrado.");
        }

        entity.StudentId = request.StudentId;
        entity.Title = request.Title.Trim();
        entity.Description = request.Description?.Trim();
        entity.Type = ParseType(request.Type);
        entity.StartAt = request.StartAt;
        entity.EndAt = request.EndAt;
        entity.Location = request.Location?.Trim();
        entity.OnlineMeetingUrl = request.OnlineMeetingUrl?.Trim();
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        if (entity.StudentId.HasValue)
        {
            var studentUserId = await _db.Students.Where(s => s.Id == entity.StudentId.Value).Select(s => s.UserId).FirstAsync();
            await _notifications.CreateAsync(studentUserId, "Compromisso atualizado", $"Seu compromisso \"{entity.Title}\" foi atualizado.", NotificationType.AppointmentRescheduled, trainerId, entity.StudentId);
        }

        entity.Student = student ?? entity.Student;
        return ApiResponse<AppointmentResponse>.Ok(Map(entity));
    }

    public async Task<ApiResponse<AppointmentResponse>> RescheduleAsync(Guid trainerId, Guid id, AppointmentRescheduleRequest request)
    {
        var entity = await _db.Appointments.Include(a => a.Student).ThenInclude(s => s!.User).FirstOrDefaultAsync(a => a.Id == id && a.TrainerId == trainerId);
        if (entity == null) return ApiResponse<AppointmentResponse>.Fail("Compromisso não encontrado.");
        if (entity.Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed) return ApiResponse<AppointmentResponse>.Fail("Compromisso finalizado não pode ser reagendado.");
        if (request.StartAt >= request.EndAt) return ApiResponse<AppointmentResponse>.Fail("Horário inválido.");
        if (await HasConflictAsync(trainerId, request.StartAt, request.EndAt, id))
            return ApiResponse<AppointmentResponse>.Fail("Conflito de horário com outro compromisso.");

        entity.RescheduledFromAppointmentId = entity.RescheduledFromAppointmentId ?? entity.Id;
        entity.StartAt = request.StartAt;
        entity.EndAt = request.EndAt;
        entity.UpdatedAt = DateTime.UtcNow;
        if (entity.Status == AppointmentStatus.Confirmed) entity.Status = AppointmentStatus.Scheduled;
        await _db.SaveChangesAsync();

        if (entity.StudentId.HasValue)
            await _notifications.CreateAsync(entity.Student!.UserId, "Compromisso reagendado", $"Seu compromisso \"{entity.Title}\" foi reagendado.", NotificationType.AppointmentRescheduled, trainerId, entity.StudentId);

        return ApiResponse<AppointmentResponse>.Ok(Map(entity));
    }

    public async Task<ApiResponse<AppointmentResponse>> CancelAsync(Guid trainerId, Guid id, string? reason)
    {
        var entity = await _db.Appointments.Include(a => a.Student).ThenInclude(s => s!.User).FirstOrDefaultAsync(a => a.Id == id && a.TrainerId == trainerId);
        if (entity == null) return ApiResponse<AppointmentResponse>.Fail("Compromisso não encontrado.");
        if (entity.Status == AppointmentStatus.Completed) return ApiResponse<AppointmentResponse>.Fail("Compromisso concluído não pode ser cancelado.");
        if (entity.Status == AppointmentStatus.Cancelled) return ApiResponse<AppointmentResponse>.Fail("Compromisso já cancelado.");

        entity.Status = AppointmentStatus.Cancelled;
        entity.CancellationReason = reason?.Trim();
        entity.CancelledAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        if (entity.StudentId.HasValue)
            await _notifications.CreateAsync(entity.Student!.UserId, "Compromisso cancelado", $"Seu compromisso \"{entity.Title}\" foi cancelado.", NotificationType.AppointmentCancelled, trainerId, entity.StudentId);

        return ApiResponse<AppointmentResponse>.Ok(Map(entity));
    }

    public async Task<ApiResponse<AppointmentResponse>> CompleteAsync(Guid trainerId, Guid id)
    {
        var entity = await _db.Appointments.Include(a => a.Student).ThenInclude(s => s!.User).FirstOrDefaultAsync(a => a.Id == id && a.TrainerId == trainerId);
        if (entity == null) return ApiResponse<AppointmentResponse>.Fail("Compromisso não encontrado.");
        if (entity.Status == AppointmentStatus.Cancelled) return ApiResponse<AppointmentResponse>.Fail("Compromisso cancelado não pode ser concluído.");
        if (entity.Status == AppointmentStatus.Completed) return ApiResponse<AppointmentResponse>.Fail("Compromisso já concluído.");
        entity.Status = AppointmentStatus.Completed;
        entity.CompletedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        if (entity.StudentId.HasValue)
            await _notifications.CreateAsync(entity.Student!.UserId, "Compromisso concluído", $"Seu compromisso \"{entity.Title}\" foi concluído.", NotificationType.AppointmentCompleted, trainerId, entity.StudentId);
        return ApiResponse<AppointmentResponse>.Ok(Map(entity));
    }

    public async Task<ApiResponse<List<AppointmentResponse>>> GetStudentAsync(Guid studentId, AppointmentQuery query)
    {
        var q = _db.Appointments.Include(a => a.Student).ThenInclude(s => s!.User).Where(a => a.StudentId == studentId).AsQueryable();
        q = ApplyFilters(q, query);
        var items = await q.OrderBy(a => a.StartAt).Take(300).ToListAsync();
        return ApiResponse<List<AppointmentResponse>>.Ok(items.Select(Map).ToList());
    }

    public async Task<ApiResponse<AppointmentResponse>> GetStudentByIdAsync(Guid studentId, Guid id)
    {
        var item = await _db.Appointments.Include(a => a.Student).ThenInclude(s => s!.User).FirstOrDefaultAsync(a => a.Id == id && a.StudentId == studentId);
        return item == null ? ApiResponse<AppointmentResponse>.Fail("Compromisso não encontrado.") : ApiResponse<AppointmentResponse>.Ok(Map(item));
    }

    public async Task<ApiResponse<AppointmentResponse>> ConfirmByStudentAsync(Guid studentId, Guid id)
    {
        var entity = await _db.Appointments.Include(a => a.Trainer).ThenInclude(t => t.User).FirstOrDefaultAsync(a => a.Id == id && a.StudentId == studentId);
        if (entity == null) return ApiResponse<AppointmentResponse>.Fail("Compromisso não encontrado.");
        if (entity.Status == AppointmentStatus.Cancelled) return ApiResponse<AppointmentResponse>.Fail("Compromisso cancelado não pode ser confirmado.");
        if (entity.Status == AppointmentStatus.Completed) return ApiResponse<AppointmentResponse>.Fail("Compromisso concluído não pode ser confirmado.");
        if (entity.Status == AppointmentStatus.Confirmed) return ApiResponse<AppointmentResponse>.Fail("Compromisso já confirmado.");

        entity.Status = AppointmentStatus.Confirmed;
        entity.ConfirmationAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _notifications.CreateAsync(entity.Trainer.UserId, "Aluno confirmou presença", $"O aluno confirmou presença em \"{entity.Title}\".", NotificationType.AppointmentConfirmed, entity.TrainerId, studentId);
        return ApiResponse<AppointmentResponse>.Ok(Map(entity));
    }

    public async Task<(int todayCount, int pendingConfirmations)> GetTrainerDashboardSummaryAsync(Guid trainerId)
    {
        var start = DateTime.UtcNow.Date;
        var end = start.AddDays(1);
        var todayCount = await _db.Appointments.CountAsync(a => a.TrainerId == trainerId && a.StartAt >= start && a.StartAt < end && a.Status != AppointmentStatus.Cancelled);
        var pending = await _db.Appointments.CountAsync(a => a.TrainerId == trainerId && a.StudentId != null && a.Status == AppointmentStatus.Scheduled && a.StartAt >= DateTime.UtcNow);
        return (todayCount, pending);
    }

    public async Task<AppointmentResponse?> GetNextStudentAppointmentAsync(Guid studentId)
    {
        var item = await _db.Appointments.Include(a => a.Student).ThenInclude(s => s!.User)
            .Where(a => a.StudentId == studentId && (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Confirmed) && a.StartAt >= DateTime.UtcNow)
            .OrderBy(a => a.StartAt)
            .FirstOrDefaultAsync();
        return item == null ? null : Map(item);
    }

    private static IQueryable<Appointment> ApplyFilters(IQueryable<Appointment> q, AppointmentQuery query)
    {
        if (query.Start.HasValue) q = q.Where(a => a.StartAt >= query.Start.Value);
        if (query.End.HasValue) q = q.Where(a => a.StartAt <= query.End.Value);
        if (query.StudentId.HasValue) q = q.Where(a => a.StudentId == query.StudentId.Value);
        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<AppointmentStatus>(query.Status, true, out var s)) q = q.Where(a => a.Status == s);
        if (!string.IsNullOrWhiteSpace(query.Type) && Enum.TryParse<AppointmentType>(query.Type, true, out var t)) q = q.Where(a => a.Type == t);
        return q;
    }

    private async Task<bool> HasConflictAsync(Guid trainerId, DateTime startAt, DateTime endAt, Guid? ignoreId)
    {
        return await _db.Appointments.AnyAsync(a =>
            a.TrainerId == trainerId
            && a.Status != AppointmentStatus.Cancelled
            && (!ignoreId.HasValue || a.Id != ignoreId.Value)
            && startAt < a.EndAt
            && endAt > a.StartAt);
    }

    private static AppointmentType ParseType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return AppointmentType.Other;
        return Enum.TryParse<AppointmentType>(value, true, out var parsed) ? parsed : AppointmentType.Other;
    }

    private static AppointmentResponse Map(Appointment a) => new()
    {
        Id = a.Id,
        TrainerId = a.TrainerId,
        StudentId = a.StudentId,
        StudentName = a.Student?.User?.Name,
        Title = a.Title,
        Description = a.Description,
        Type = a.Type.ToString(),
        Status = a.Status.ToString(),
        StartAt = a.StartAt,
        EndAt = a.EndAt,
        Location = a.Location,
        OnlineMeetingUrl = a.OnlineMeetingUrl,
        CancellationReason = a.CancellationReason,
        ConfirmationAt = a.ConfirmationAt,
        CancelledAt = a.CancelledAt,
        CompletedAt = a.CompletedAt,
        RescheduledFromAppointmentId = a.RescheduledFromAppointmentId,
        CreatedAt = a.CreatedAt
    };
}
