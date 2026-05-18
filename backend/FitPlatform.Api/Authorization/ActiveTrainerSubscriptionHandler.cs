using FitPlatform.Application.Interfaces;
using FitPlatform.Domain.Enums;
using FitPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace FitPlatform.Api.Authorization;

public sealed class ActiveTrainerSubscriptionHandler : AuthorizationHandler<ActiveTrainerSubscriptionRequirement>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ActiveTrainerSubscriptionHandler(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ActiveTrainerSubscriptionRequirement requirement)
    {
        if (string.Equals(_currentUser.Role, "Owner", StringComparison.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
            return;
        }

        if (!string.Equals(_currentUser.Role, "Trainer", StringComparison.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
            return;
        }

        if (!_currentUser.TrainerId.HasValue)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var hasActiveSubscription = await _db.TrainerSubscriptions
            .AsNoTracking()
            .AnyAsync(s => s.TrainerId == _currentUser.TrainerId.Value
                           && s.Status == TrainerSubscriptionStatus.Active
                           && s.EndDate >= now);

        if (hasActiveSubscription)
        {
            context.Succeed(requirement);
        }
    }
}

