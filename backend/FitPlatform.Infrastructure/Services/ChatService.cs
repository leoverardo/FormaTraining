using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Chat;
using FitPlatform.Domain.Entities;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class ChatService
{
    private readonly AppDbContext _db;
    private readonly NotificationService _notifications;

    public ChatService(AppDbContext db, NotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<ApiResponse<List<ConversationListItemResponse>>> GetConversationsForTrainerAsync(Guid trainerId, Guid currentUserId)
    {
        var conversations = await _db.Conversations
            .AsNoTracking()
            .Include(c => c.Student).ThenInclude(s => s.User)
            .Where(c => c.TrainerId == trainerId && c.Student.Status == StudentStatus.Active)
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync();

        var ids = conversations.Select(c => c.Id).ToList();
        var previews = await _db.ChatMessages
            .AsNoTracking()
            .Where(m => ids.Contains(m.ConversationId))
            .GroupBy(m => m.ConversationId)
            .Select(g => g.OrderByDescending(x => x.CreatedAt).Select(x => new { x.ConversationId, x.Content, x.CreatedAt }).First())
            .ToDictionaryAsync(x => x.ConversationId, x => x.Content);

        var unread = await _db.ChatMessages
            .AsNoTracking()
            .Where(m => ids.Contains(m.ConversationId) && m.SenderUserId != currentUserId && m.ReadAt == null)
            .GroupBy(m => m.ConversationId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

        var data = conversations.Select(c => new ConversationListItemResponse
        {
            ConversationId = c.Id,
            TrainerId = c.TrainerId,
            StudentId = c.StudentId,
            ParticipantName = c.Student.User.Name,
            ParticipantAvatarUrl = null,
            LastMessageAt = c.LastMessageAt,
            LastMessagePreview = previews.GetValueOrDefault(c.Id),
            UnreadCount = unread.GetValueOrDefault(c.Id, 0)
        }).ToList();

        return ApiResponse<List<ConversationListItemResponse>>.Ok(data);
    }

    public async Task<ApiResponse<List<ConversationListItemResponse>>> GetConversationsForStudentAsync(Guid studentId, Guid currentUserId)
    {
        var conversations = await _db.Conversations
            .AsNoTracking()
            .Include(c => c.Trainer).ThenInclude(t => t.User)
            .Include(c => c.Student)
            .Where(c => c.StudentId == studentId && c.Student.Status == StudentStatus.Active)
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync();

        var ids = conversations.Select(c => c.Id).ToList();
        var previews = await _db.ChatMessages
            .AsNoTracking()
            .Where(m => ids.Contains(m.ConversationId))
            .GroupBy(m => m.ConversationId)
            .Select(g => g.OrderByDescending(x => x.CreatedAt).Select(x => new { x.ConversationId, x.Content }).First())
            .ToDictionaryAsync(x => x.ConversationId, x => x.Content);

        var unread = await _db.ChatMessages
            .AsNoTracking()
            .Where(m => ids.Contains(m.ConversationId) && m.SenderUserId != currentUserId && m.ReadAt == null)
            .GroupBy(m => m.ConversationId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

        var data = conversations.Select(c => new ConversationListItemResponse
        {
            ConversationId = c.Id,
            TrainerId = c.TrainerId,
            StudentId = c.StudentId,
            ParticipantName = c.Trainer.User.Name,
            ParticipantAvatarUrl = c.Trainer.ProfilePhotoUrl,
            LastMessageAt = c.LastMessageAt,
            LastMessagePreview = previews.GetValueOrDefault(c.Id),
            UnreadCount = unread.GetValueOrDefault(c.Id, 0)
        }).ToList();

        return ApiResponse<List<ConversationListItemResponse>>.Ok(data);
    }

    public async Task<ApiResponse<ConversationDetailsResponse>> GetConversationAsync(Guid conversationId, Guid userId, string role)
    {
        var convo = await _db.Conversations
            .Include(c => c.Trainer).ThenInclude(t => t.User)
            .Include(c => c.Student).ThenInclude(s => s.User)
            .FirstOrDefaultAsync(c => c.Id == conversationId);
        if (convo == null) return ApiResponse<ConversationDetailsResponse>.Fail("Conversa não encontrada.");
        if (!await HasAccessAsync(convo, userId, role)) return ApiResponse<ConversationDetailsResponse>.Fail("Acesso negado.");
        if (convo.Student.Status != StudentStatus.Active) return ApiResponse<ConversationDetailsResponse>.Fail("Conversa indisponível para aluno inativo.");

        await MarkAsReadAsync(conversationId, userId);
        var messages = await _db.ChatMessages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        return ApiResponse<ConversationDetailsResponse>.Ok(new ConversationDetailsResponse
        {
            ConversationId = convo.Id,
            TrainerId = convo.TrainerId,
            StudentId = convo.StudentId,
            TrainerName = convo.Trainer.User.Name,
            StudentName = convo.Student.User.Name,
            Messages = messages.Select(MapMessage).ToList()
        });
    }

    public async Task<ApiResponse<ChatMessageResponse>> SendMessageAsync(Guid userId, string role, SendChatMessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return ApiResponse<ChatMessageResponse>.Fail("Mensagem vazia.");

        Conversation? convo = null;
        if (request.ConversationId.HasValue)
        {
            convo = await _db.Conversations
                .Include(c => c.Trainer).ThenInclude(t => t.User)
                .Include(c => c.Student).ThenInclude(s => s.User)
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId.Value);
            if (convo == null) return ApiResponse<ChatMessageResponse>.Fail("Conversa não encontrada.");
        }
        else
        {
            if (role != "Trainer" || !request.StudentId.HasValue)
                return ApiResponse<ChatMessageResponse>.Fail("Dados inválidos para iniciar conversa.");

            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == request.StudentId.Value);
            if (student == null || student.Status != StudentStatus.Active)
                return ApiResponse<ChatMessageResponse>.Fail("Aluno não disponível para conversa.");

            var trainer = await _db.Trainers.FirstOrDefaultAsync(t => t.UserId == userId);
            if (trainer == null || trainer.Id != student.TrainerId)
                return ApiResponse<ChatMessageResponse>.Fail("Você só pode conversar com seus próprios alunos.");

            convo = await _db.Conversations
                .Include(c => c.Trainer).ThenInclude(t => t.User)
                .Include(c => c.Student).ThenInclude(s => s.User)
                .FirstOrDefaultAsync(c => c.TrainerId == trainer.Id && c.StudentId == student.Id);

            if (convo == null)
            {
                convo = new Conversation
                {
                    TrainerId = trainer.Id,
                    StudentId = student.Id,
                    LastMessageAt = DateTime.UtcNow
                };
                _db.Conversations.Add(convo);
                try
                {
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    convo = await _db.Conversations
                        .Include(c => c.Trainer).ThenInclude(t => t.User)
                        .Include(c => c.Student).ThenInclude(s => s.User)
                        .FirstOrDefaultAsync(c => c.TrainerId == trainer.Id && c.StudentId == student.Id);
                    if (convo == null) throw;
                }
            }
        }

        if (!await HasAccessAsync(convo, userId, role))
            return ApiResponse<ChatMessageResponse>.Fail("Acesso negado.");
        if (convo.Student.Status != StudentStatus.Active)
            return ApiResponse<ChatMessageResponse>.Fail("Conversa indisponível para aluno inativo.");

        var senderRole = role == "Trainer" ? UserRole.Trainer : UserRole.Student;
        var message = new ChatMessage
        {
            ConversationId = convo.Id,
            SenderUserId = userId,
            SenderRole = senderRole,
            Content = request.Content.Trim()
        };

        convo.LastMessageAt = DateTime.UtcNow;
        convo.UpdatedAt = DateTime.UtcNow;

        _db.ChatMessages.Add(message);
        await _db.SaveChangesAsync();

        var recipientUserId = role == "Trainer" ? convo.Student.UserId : convo.Trainer.UserId;
        var senderName = role == "Trainer" ? convo.Trainer.User.Name : convo.Student.User.Name;
        await _notifications.CreateAsync(
            recipientUserId,
            "Nova mensagem",
            $"{senderName} enviou uma nova mensagem.",
            NotificationType.ChatMessage,
            convo.TrainerId,
            convo.StudentId);

        return ApiResponse<ChatMessageResponse>.Ok(MapMessage(message), "Mensagem enviada.");
    }

    public async Task<ApiResponse<int>> GetUnreadCountAsync(Guid userId, string role)
    {
        var query = _db.ChatMessages
            .Where(m => m.SenderUserId != userId && m.ReadAt == null);

        if (role == "Trainer")
        {
            var trainer = await _db.Trainers.FirstOrDefaultAsync(t => t.UserId == userId);
            if (trainer == null) return ApiResponse<int>.Ok(0);
            query = query.Where(m => m.Conversation.TrainerId == trainer.Id && m.Conversation.Student.Status == StudentStatus.Active);
        }
        else if (role == "Student")
        {
            var student = await _db.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null || student.Status != StudentStatus.Active) return ApiResponse<int>.Ok(0);
            query = query.Where(m => m.Conversation.StudentId == student.Id);
        }
        else
        {
            return ApiResponse<int>.Ok(0);
        }

        var count = await query.CountAsync();
        return ApiResponse<int>.Ok(count);
    }

    private async Task MarkAsReadAsync(Guid conversationId, Guid userId)
    {
        var unread = await _db.ChatMessages
            .Where(m => m.ConversationId == conversationId && m.SenderUserId != userId && m.ReadAt == null)
            .ToListAsync();

        if (unread.Count == 0) return;
        foreach (var item in unread)
            item.ReadAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    private async Task<bool> HasAccessAsync(Conversation convo, Guid userId, string role)
    {
        if (role == "Trainer")
        {
            var trainerId = await _db.Trainers
                .Where(t => t.UserId == userId)
                .Select(t => t.Id)
                .FirstOrDefaultAsync();
            return trainerId != Guid.Empty && trainerId == convo.TrainerId;
        }
        if (role == "Student")
        {
            var student = await _db.Students
                .Where(s => s.UserId == userId)
                .Select(s => new { s.Id, s.Status })
                .FirstOrDefaultAsync();
            return student != null && student.Id == convo.StudentId && student.Status == StudentStatus.Active;
        }
        return false;
    }

    private static ChatMessageResponse MapMessage(ChatMessage m) => new()
    {
        Id = m.Id,
        ConversationId = m.ConversationId,
        SenderUserId = m.SenderUserId,
        SenderRole = m.SenderRole.ToString(),
        Content = m.Content,
        CreatedAt = m.CreatedAt,
        ReadAt = m.ReadAt
    };
}
