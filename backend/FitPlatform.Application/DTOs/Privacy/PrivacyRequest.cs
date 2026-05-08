namespace FitPlatform.Application.DTOs.Privacy;

public class PrivacyRequestDto
{
    public string RequestType { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class ConsentRequest
{
    public Guid TermsDocumentId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public class TermsDocumentResponse
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool Active { get; set; }
}
