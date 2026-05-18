using Microsoft.AspNetCore.Authorization;

namespace FitPlatform.Api.Authorization;

public sealed class ActiveTrainerSubscriptionRequirement : IAuthorizationRequirement
{
    public const string PolicyName = "ActiveTrainerSubscription";
}

