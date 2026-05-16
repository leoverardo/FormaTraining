using FitPlatform.Domain.Enums;

namespace FitPlatform.Application.DTOs.Payment;

public class CreateProviderSubscriptionRequest
{
    public Guid LocalSubscriptionId { get; set; }
    public Guid TrainerId { get; set; }
    public Guid PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public BillingFrequency BillingCycle { get; set; }
    public int AmountInCents { get; set; }
    public string PayerEmail { get; set; } = string.Empty;
    public string? PayerName { get; set; }
    public string? Phone { get; set; }
    public string? CouponCode { get; set; }
    public string? ExistingCustomerId { get; set; }
    public string? AbacatePayProductId { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class ProviderSubscriptionCreated
{
    public string ProviderSubscriptionId { get; set; } = string.Empty;
    public string? ProviderCheckoutId { get; set; }
    public string? ProviderCustomerId { get; set; }
    public string? PayerId { get; set; }
    public string? CheckoutUrl { get; set; }
    public string RawPayload { get; set; } = "{}";
}

public class ProviderSubscriptionDetails
{
    public string ProviderSubscriptionId { get; set; } = string.Empty;
    public string? PayerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? LastPaymentId { get; set; }
    public string? LastPaymentStatus { get; set; }
    public decimal? LastPaymentAmount { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public string RawPayload { get; set; } = "{}";
}

public class ChangeProviderSubscriptionPlanRequest
{
    public string ProviderSubscriptionId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
}
