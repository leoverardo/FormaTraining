using FitPlatform.Domain.Enums;

namespace FitPlatform.Application.DTOs.Subscription;

/// <summary>
/// Solicitação de troca de plano. Recebe apenas o que é estritamente necessário:
/// o backend resolve preço, produto do provedor e regras de upgrade/downgrade.
/// Nenhum valor monetário é aceito do frontend.
/// </summary>
public class ChangeSubscriptionPlanRequest
{
    public Guid PlatformPlanId { get; set; }

    /// <summary>Ciclo desejado. Quando ausente, mantém o ciclo atual da assinatura.</summary>
    public BillingFrequency? BillingCycle { get; set; }
}

/// <summary>Resultado de um cancelamento de assinatura.</summary>
public class CancelSubscriptionResponse
{
    public Guid SubscriptionId { get; set; }
    public string Status { get; set; } = string.Empty;

    /// <summary>Acesso permanece liberado até esta data (período já pago).</summary>
    public DateTime AccessValidUntil { get; set; }

    /// <summary>True quando a assinatura já estava cancelada (chamada idempotente).</summary>
    public bool AlreadyCanceled { get; set; }
}

/// <summary>Resultado de uma solicitação de troca de plano.</summary>
public class ChangeSubscriptionPlanResponse
{
    public Guid SubscriptionId { get; set; }

    public Guid CurrentPlanId { get; set; }
    public string CurrentPlanName { get; set; } = string.Empty;
    public string CurrentBillingCycle { get; set; } = string.Empty;
    public int CurrentAmountInCents { get; set; }

    public Guid NewPlanId { get; set; }
    public string NewPlanName { get; set; } = string.Empty;
    public string NewBillingCycle { get; set; } = string.Empty;
    public int NewAmountInCents { get; set; }

    /// <summary>Upgrade, Downgrade ou CycleChange — calculado pelo backend.</summary>
    public string ChangeKind { get; set; } = string.Empty;

    /// <summary>
    /// A AbacatePay agenda a troca e a aplica no início do próximo ciclo de cobrança;
    /// até lá o plano vigente continua sendo o atual.
    /// </summary>
    public DateTime EffectiveAt { get; set; }

    public string ProviderStatus { get; set; } = string.Empty;
}
