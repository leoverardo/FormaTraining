namespace FitPlatform.Application.DTOs.Notes;

public class NoteRequest
{
    public string Title { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public bool IsPinned { get; set; } = false;
}

public class NoteResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
