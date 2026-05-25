namespace FitPlatform.Domain.Enums;

public enum BillingFrequency
{
    Monthly = 1,
    [Obsolete("Quarterly cycle is deprecated and not available for new subscriptions.")]
    Quarterly = 2,
    Semiannual = 3,
    Yearly = 4
}
