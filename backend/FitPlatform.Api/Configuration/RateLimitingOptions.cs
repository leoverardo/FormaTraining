namespace FitPlatform.Api.Configuration;

public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";
    public RateLimitingPolicies Policies { get; set; } = new();
}

public sealed class RateLimitingPolicies
{
    public RateLimitingPolicyOptions AuthLogin { get; set; } = new();
    public RateLimitingPolicyOptions StudentRegister { get; set; } = new();
    public RateLimitingPolicyOptions TrainerOnboarding { get; set; } = new();
    public RateLimitingPolicyOptions PublicLead { get; set; } = new();
    public RateLimitingPolicyOptions ExplorePublicSearch { get; set; } = new();
}

public sealed class RateLimitingPolicyOptions
{
    public int PermitLimit { get; set; }
    public int WindowSeconds { get; set; }
    public int QueueLimit { get; set; }
}
