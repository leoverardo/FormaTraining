using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Notifications;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class NotificationService
{
    private readonly AppDbContext _db;

    public NotificationService(AppDbContext db) => _db = db;

    public async Task CreateAsync(Guid userId, string title, string message, NotificationType type, Guid? trainerId = null, Guid? studentId = null)
    {
        _db.Notifications.Add(new Notification
        {
            UserId = userId, Title = title, Message = message,
            Type = type, TrainerId = trainerId, StudentId = studentId
        });
        await _db.SaveChangesAsync();
    }

    public async Task<ApiResponse<List<NotificationResponse>>> GetForUserAsync(Guid userId)
    {
        var items = await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync();
        return ApiResponse<List<NotificationResponse>>.Ok(items.Select(Map).ToList());
    }

    public async Task<ApiResponse<int>> GetUnreadCountAsync(Guid userId)
    {
        var count = await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        return ApiResponse<int>.Ok(count);
    }

    public async Task<ApiResponse> MarkReadAsync(Guid id, Guid userId)
    {
        var n = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        if (n == null) return ApiResponse.Fail("Notificação não encontrada.");
        n.IsRead = true; n.ReadAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ApiResponse.Ok();
    }

    public async Task<ApiResponse> MarkAllReadAsync(Guid userId)
    {
        var items = await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        items.ForEach(n => { n.IsRead = true; n.ReadAt = DateTime.UtcNow; });
        await _db.SaveChangesAsync();
        return ApiResponse.Ok();
    }

    private static NotificationResponse Map(Notification n) => new()
    {
        Id = n.Id, Title = n.Title, Message = n.Message, Type = n.Type.ToString(),
        IsRead = n.IsRead, ReadAt = n.ReadAt, CreatedAt = n.CreatedAt
    };
}
