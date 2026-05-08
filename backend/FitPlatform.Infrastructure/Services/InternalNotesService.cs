using FitPlatform.Application.Common;
using FitPlatform.Application.DTOs.Notes;
using FitPlatform.Domain.Entities;
using FitPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Infrastructure.Services;

public class InternalNotesService
{
    private readonly AppDbContext _db;

    public InternalNotesService(AppDbContext db) => _db = db;

    public async Task<ApiResponse<List<NoteResponse>>> GetByStudentAsync(Guid studentId, Guid trainerId)
    {
        if (!await _db.Students.AnyAsync(s => s.Id == studentId && s.TrainerId == trainerId))
            return ApiResponse<List<NoteResponse>>.Fail("Aluno não encontrado.");

        var notes = await _db.TrainerStudentNotes
            .Where(n => n.StudentId == studentId && n.TrainerId == trainerId)
            .OrderByDescending(n => n.IsPinned).ThenByDescending(n => n.UpdatedAt)
            .ToListAsync();

        return ApiResponse<List<NoteResponse>>.Ok(notes.Select(Map).ToList());
    }

    public async Task<ApiResponse<NoteResponse>> CreateAsync(Guid studentId, NoteRequest request, Guid trainerId)
    {
        if (!await _db.Students.AnyAsync(s => s.Id == studentId && s.TrainerId == trainerId))
            return ApiResponse<NoteResponse>.Fail("Aluno não encontrado.");

        var note = new TrainerStudentNote
        {
            TrainerId = trainerId, StudentId = studentId,
            Title = request.Title, Note = request.Note, IsPinned = request.IsPinned
        };
        _db.TrainerStudentNotes.Add(note);
        await _db.SaveChangesAsync();
        return ApiResponse<NoteResponse>.Ok(Map(note));
    }

    public async Task<ApiResponse<NoteResponse>> UpdateAsync(Guid noteId, Guid studentId, NoteRequest request, Guid trainerId)
    {
        var note = await _db.TrainerStudentNotes.FirstOrDefaultAsync(n => n.Id == noteId && n.StudentId == studentId && n.TrainerId == trainerId);
        if (note == null) return ApiResponse<NoteResponse>.Fail("Nota não encontrada.");

        note.Title = request.Title; note.Note = request.Note; note.IsPinned = request.IsPinned;
        note.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ApiResponse<NoteResponse>.Ok(Map(note));
    }

    public async Task<ApiResponse> DeleteAsync(Guid noteId, Guid studentId, Guid trainerId)
    {
        var note = await _db.TrainerStudentNotes.FirstOrDefaultAsync(n => n.Id == noteId && n.StudentId == studentId && n.TrainerId == trainerId);
        if (note == null) return ApiResponse.Fail("Nota não encontrada.");
        _db.TrainerStudentNotes.Remove(note);
        await _db.SaveChangesAsync();
        return ApiResponse.Ok("Nota removida.");
    }

    private static NoteResponse Map(TrainerStudentNote n) => new()
    {
        Id = n.Id, Title = n.Title, Note = n.Note, IsPinned = n.IsPinned,
        CreatedAt = n.CreatedAt, UpdatedAt = n.UpdatedAt
    };
}
