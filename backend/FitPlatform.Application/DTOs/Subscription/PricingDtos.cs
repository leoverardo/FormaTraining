using FitPlatform.Domain.Enums;

namespace FitPlatform.Application.DTOs.Subscription;

public class PlanBillingOptionResponse
{
    public BillingFrequency BillingCycle { get; set; }
    public int Months { get; set; }
    public int BaseAmountInCents { get; set; }
    public int CycleDiscountAmountInCents { get; set; }
    public int FinalAmountInCents { get; set; }
    public decimal SavingsPercent { get; set; }
}

public class ValidateCouponRequest
{
    public Guid PlanId { get; set; }
    public BillingFrequency BillingCycle { get; set; }
    public string? CouponCode { get; set; }
}

public class ValidateCouponResponse
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? CouponCode { get; set; }
    public int CycleBaseAmountInCents { get; set; }
    public int CycleDiscountAmountInCents { get; set; }
    public int SubtotalAfterCycleDiscountInCents { get; set; }
    public int CouponDiscountAmountInCents { get; set; }
    public int FinalAmountInCents { get; set; }
}
