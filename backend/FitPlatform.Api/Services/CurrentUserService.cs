using System.Security.Claims;
using FitPlatform.Application.Interfaces;

namespace FitPlatform.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _http;

    public CurrentUserService(IHttpContextAccessor http) => _http = http;

    public Guid UserId => Guid.Parse(_http.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    public string Role => _http.HttpContext!.User.FindFirstValue(ClaimTypes.Role)!;

    public Guid? TrainerId
    {
        get
        {
            var val = _http.HttpContext!.User.FindFirstValue("TrainerId");
            return val != null ? Guid.Parse(val) : null;
        }
    }

    public Guid? StudentId
    {
        get
        {
            var val = _http.HttpContext!.User.FindFirstValue("StudentId");
            return val != null ? Guid.Parse(val) : null;
        }
    }
}
