namespace FitPlatform.Application.DTOs.Auth;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public Guid Id { get; set; }
    public Guid UserId => Id;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public Guid? TrainerId { get; set; }
    public Guid? StudentId { get; set; }
    public Guid? StudentProfileId { get; set; }
    public Guid? ActiveTrainerId { get; set; }
    public bool HasActiveTrainerLink { get; set; }
    public bool IsExplorer { get; set; }
    public bool MustChangePassword { get; set; }
}
