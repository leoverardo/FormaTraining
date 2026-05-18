using FitPlatform.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace FitPlatform.Api.Authorization;

public sealed class SubscriptionAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Forbidden && context.User.Identity?.IsAuthenticated == true)
        {
            var failed = authorizeResult.AuthorizationFailure?.FailedRequirements
                .OfType<ActiveTrainerSubscriptionRequirement>()
                .Any() == true;

            if (failed)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(ApiResponse.Fail(
                    "Este recurso exige assinatura ativa do trainer.",
                    ["ACTIVE_SUBSCRIPTION_REQUIRED"]));
                return;
            }
        }

        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}

