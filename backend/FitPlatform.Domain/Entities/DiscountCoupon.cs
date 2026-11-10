using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class DiscountCoupon : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public int? MaxUsesTotal { get; set; }
    public int? MaxUsesPerCustomer { get; set; }
    public int CurrentUses { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? AppliesToPlanId { get; set; }
    public BillingFrequency? AppliesToBillingCycle { get; set; }
    public int? MinimumPurchaseAmountInCents { get; set; }

    /// <summary>
    /// Código do cupom espelhado no provedor de pagamento, quando já sincronizado.
    /// Enquanto nulo, o cupom ainda não existe na AbacatePay e será criado sob demanda
    /// no primeiro checkout que o utilizar.
    /// </summary>
    public string? ProviderCouponCode { get; set; }

    /// <summary>Momento da última sincronização bem-sucedida com o provedor.</summary>
    public DateTime? ProviderSyncedAt { get; set; }
}
