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

    /// <summary>
    /// Códigos de cupom a serem aplicados pelo provedor no checkout (campo <c>coupons</c>
    /// da AbacatePay). O backend permanece como fonte da verdade sobre elegibilidade;
    /// o provedor apenas executa o desconto já validado.
    /// </summary>
    public List<string> CouponCodes { get; set; } = new();

    /// <summary>Redirect após pagamento concluído. Quando nulo, o provedor usa o padrão configurado.</summary>
    public string? CompletionUrl { get; set; }

    /// <summary>Redirect quando o cliente desiste do checkout. Quando nulo, o provedor usa o padrão configurado.</summary>
    public string? ReturnUrl { get; set; }
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

    /// <summary>Status bruto retornado pelo provedor (ex.: PENDING, PAID, CANCELLED).</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Status normalizado para o domínio interno. Null quando não foi possível mapear.</summary>
    public ProviderSubscriptionState State { get; set; } = ProviderSubscriptionState.Unknown;

    public string? ProviderCheckoutId { get; set; }
    public string? ExternalId { get; set; }
    public string? CustomerId { get; set; }
    public int? AmountInCents { get; set; }
    public int? PaidAmountInCents { get; set; }
    public string? LastPaymentId { get; set; }
    public string? LastPaymentStatus { get; set; }
    public decimal? LastPaymentAmount { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public string RawPayload { get; set; } = "{}";
}

/// <summary>
/// Estados de assinatura normalizados a partir dos status da AbacatePay
/// (PENDING, PAID, EXPIRED, CANCELLED, REFUNDED).
/// </summary>
public enum ProviderSubscriptionState
{
    Unknown = 0,
    Pending = 1,
    Paid = 2,
    Expired = 3,
    Cancelled = 4,
    Refunded = 5
}

/// <summary>Filtros de <c>GET /subscriptions/list</c>. Sempre limitado — nunca varre tudo.</summary>
public class ProviderSubscriptionListQuery
{
    /// <summary>Data inicial de criação (inclusive).</summary>
    public DateOnly? StartDate { get; set; }

    /// <summary>Data final de criação (inclusive).</summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>Status bruto do provedor para filtrar (ex.: "PENDING").</summary>
    public string? Status { get; set; }

    /// <summary>Itens por página (1-100).</summary>
    public int Limit { get; set; } = 100;

    /// <summary>Número máximo de páginas percorridas — proteção contra paginação infinita.</summary>
    public int MaxPages { get; set; } = 5;
}

public class ChangeProviderSubscriptionPlanRequest
{
    public string ProviderSubscriptionId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
}

/// <summary>
/// Resultado de <c>POST /subscriptions/change-plan</c>. A AbacatePay agenda a troca
/// como PENDING e a aplica no início do próximo ciclo de cobrança.
/// </summary>
public class ProviderPlanChangeResult
{
    /// <summary>Id da solicitação de atualização (subu_...).</summary>
    public string? UpdateId { get; set; }
    public string? ProviderSubscriptionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ProductId { get; set; }
    public int Quantity { get; set; }
    public int? NewAmountInCents { get; set; }
    public DateTime? RequestedAt { get; set; }
    public string RawPayload { get; set; } = "{}";
}

/// <summary>Definição de cupom a ser espelhada no provedor.</summary>
public class ProviderCouponDefinition
{
    public string Code { get; set; } = string.Empty;

    /// <summary>PERCENTAGE ou FIXED.</summary>
    public string DiscountKind { get; set; } = "PERCENTAGE";

    /// <summary>Percentual (1-100) quando PERCENTAGE; valor em centavos quando FIXED.</summary>
    public int Discount { get; set; }

    /// <summary>Limite total de resgates. -1 = ilimitado.</summary>
    public int MaxRedeems { get; set; } = -1;

    public string? Notes { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class ProviderCouponSyncResult
{
    /// <summary>Código efetivamente aceito pelo provedor.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>True quando o cupom já existia no provedor e foi reaproveitado.</summary>
    public bool AlreadyExisted { get; set; }

    public string RawPayload { get; set; } = "{}";
}
