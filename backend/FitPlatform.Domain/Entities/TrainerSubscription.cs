using FitPlatform.Domain.Common;
using FitPlatform.Domain.Enums;

namespace FitPlatform.Domain.Entities;

public class TrainerSubscription : BaseEntity
{
    public Guid TrainerId { get; set; }
    public Guid? TrainerOnboardingId { get; set; }
    public Guid PlatformPlanId { get; set; }
    public Guid? PlatformPlanPriceId { get; set; }
    public BillingFrequency BillingCycle { get; set; } = BillingFrequency.Monthly;
    public TrainerSubscriptionStatus Status { get; set; } = TrainerSubscriptionStatus.Pending;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Provider { get; set; } = "AbacatePay";
    public int BaseAmountInCents { get; set; }
    public int CycleDiscountAmountInCents { get; set; }
    public int CouponDiscountAmountInCents { get; set; }
    public int FinalAmountInCents { get; set; }
    public string? CouponCodeApplied { get; set; }
    public string? AbacatePayCustomerId { get; set; }
    public string? AbacatePayCheckoutId { get; set; }
    public string? AbacatePaySubscriptionId { get; set; }
    public string? MercadoPagoSubscriptionId { get; set; }
    public string? MercadoPagoPayerId { get; set; }
    public string? InitPoint { get; set; }
    public string? LastPaymentStatus { get; set; }

    // ── Falhas de cobrança recorrente ────────────────────────────────────────
    // Uma falha isolada NÃO cancela a assinatura: a AbacatePay retenta conforme o
    // retryPolicy e só emite subscription.cancelled ao esgotar as tentativas.

    /// <summary>Quantidade de cobranças recorrentes que falharam desde o último pagamento aprovado.</summary>
    public int PaymentFailureCount { get; set; }

    /// <summary>Momento da última falha de cobrança reportada pelo provedor.</summary>
    public DateTime? LastPaymentFailureAt { get; set; }

    // ── Troca de plano agendada ──────────────────────────────────────────────
    // A AbacatePay aplica a troca no início do próximo ciclo; até lá o plano vigente
    // continua sendo o atual e a mudança fica registrada aqui.

    /// <summary>Plano para o qual a assinatura migrará no próximo ciclo.</summary>
    public Guid? PendingPlatformPlanId { get; set; }

    /// <summary>Ciclo de cobrança que passará a valer junto com o plano pendente.</summary>
    public BillingFrequency? PendingBillingCycle { get; set; }

    /// <summary>Id da solicitação de atualização no provedor (subu_...).</summary>
    public string? PendingPlanChangeProviderId { get; set; }

    /// <summary>Momento em que a troca de plano foi solicitada.</summary>
    public DateTime? PendingPlanChangeRequestedAt { get; set; }

    /// <summary>Valor final (centavos) que passará a ser cobrado após a troca.</summary>
    public int? PendingFinalAmountInCents { get; set; }

    // ── Reconciliação ────────────────────────────────────────────────────────

    /// <summary>Último momento em que a assinatura foi confrontada com o provedor.</summary>
    public DateTime? LastReconciledAt { get; set; }

    /// <summary>Tentativas de reconciliação já realizadas (limita o processamento infinito).</summary>
    public int ReconciliationAttempts { get; set; }

    /// <summary>Último status bruto observado no provedor (ex.: PENDING, PAID).</summary>
    public string? ProviderStatus { get; set; }

    public Trainer Trainer { get; set; } = null!;
    public PlatformPlan PlatformPlan { get; set; } = null!;
    public PlatformPlanPrice? PlatformPlanPrice { get; set; }
    public ICollection<TrainerPayment> Payments { get; set; } = new List<TrainerPayment>();
}
