using FitPlatform.Domain.Common;

namespace FitPlatform.Domain.Entities;

public class DiscountCouponRedemption : BaseEntity
{
    public Guid CouponId { get; set; }
    public Guid? TrainerId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public Guid? PaymentId { get; set; }
    public DateTime RedeemedAt { get; set; }
    public int DiscountAmountInCents { get; set; }

    public DiscountCoupon Coupon { get; set; } = null!;
}
