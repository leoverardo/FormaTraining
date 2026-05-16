namespace FitPlatform.Application.DTOs.Subscription;

public class SubscriptionResponse
{
    public Guid Id { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public int MaxActiveStudents { get; set; }
    public string Status { get; set; } = string.Empty;
    public string BillingCycle { get; set; } = string.Empty;
    public decimal? CurrentCyclePrice { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? MercadoPagoSubscriptionId { get; set; }
    public string? AbacatePaySubscriptionId { get; set; }
    public string? AbacatePayCheckoutId { get; set; }
    public string? CheckoutUrl { get; set; }
    public List<PaymentHistoryItem> Payments { get; set; } = new();
}

public class PaymentHistoryItem
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
