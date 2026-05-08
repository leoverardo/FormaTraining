using FitPlatform.Domain.Entities;

namespace FitPlatform.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user, Guid? trainerId, Guid? studentId);
}
