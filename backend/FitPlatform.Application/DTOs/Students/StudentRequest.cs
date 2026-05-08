namespace FitPlatform.Application.DTOs.Students;

public class CreateStudentRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Goal { get; set; }
    public string? Notes { get; set; }
    public bool ActiveOnCreate { get; set; } = true;
}

public class UpdateStudentRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Goal { get; set; }
    public string? Notes { get; set; }
}
