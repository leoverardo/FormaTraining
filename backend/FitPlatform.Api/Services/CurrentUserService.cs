using System.Security.Claims;
using FitPlatform.Application.Interfaces;

namespace FitPlatform.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _http;

    public CurrentUserService(IHttpContextAccessor http) => _http = http;

    public Guid UserId
    {
        get
        {
            var value = _http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }
    public string Role => _http.HttpContext?.User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    public Guid? TrainerId
    {
        get
        {
            var val = _http.HttpContext?.User.FindFirstValue("TrainerId");
            return Guid.TryParse(val, out var trainerId) ? trainerId : null;
        }
    }

    public Guid? StudentId
    {
        get
        {
            var val = _http.HttpContext?.User.FindFirstValue("StudentId");
            return Guid.TryParse(val, out var studentId) ? studentId : null;
        }
    }

    public Guid? StudentProfileId
    {
        get
        {
            var val = _http.HttpContext?.User.FindFirstValue("StudentProfileId");
            return Guid.TryParse(val, out var studentProfileId) ? studentProfileId : null;
        }
    }
}
