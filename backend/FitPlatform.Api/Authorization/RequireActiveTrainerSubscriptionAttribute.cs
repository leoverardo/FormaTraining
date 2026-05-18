using Microsoft.AspNetCore.Authorization;

namespace FitPlatform.Api.Authorization;

public sealed class RequireActiveTrainerSubscriptionAttribute : AuthorizeAttribute
{
    public RequireActiveTrainerSubscriptionAttribute()
    {
        Policy = ActiveTrainerSubscriptionRequirement.PolicyName;
    }
}

