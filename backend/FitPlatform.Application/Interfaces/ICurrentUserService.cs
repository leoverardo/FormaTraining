namespace FitPlatform.Application.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string Role { get; }
    Guid? TrainerId { get; }
    Guid? StudentId { get; }
}
