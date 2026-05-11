using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; } = false;
    public DateTime? LastLoginAt { get; set; }
    public DateTime? LastActivityAt { get; set; }

    public Trainer? Trainer { get; set; }
    public Student? Student { get; set; }
    public StudentProfile? StudentProfile { get; set; }
}
