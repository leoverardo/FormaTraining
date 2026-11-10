using FitPlatform.Application.DTOs.Payment;

namespace FitPlatform.Application.Interfaces;

public interface IPaymentProvider
{
    Task<ProviderSubscriptionCreated> CreateSubscriptionAsync(CreateProviderSubscriptionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta a situação de uma assinatura/checkout no provedor.
    /// Aceita tanto o identificador do checkout (bill_...) quanto o externalId
    /// gravado na criação. Retorna null quando não encontrado.
    /// </summary>
    Task<ProviderSubscriptionDetails?> GetSubscriptionAsync(string providerSubscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista assinaturas do provedor com filtros e paginação limitada.
    /// Usado pela rotina de reconciliação para resolver várias assinaturas em poucas chamadas.
    /// </summary>
    Task<IReadOnlyList<ProviderSubscriptionDetails>> ListSubscriptionsAsync(ProviderSubscriptionListQuery query, CancellationToken cancellationToken = default);

    Task CancelSubscriptionAsync(string providerSubscriptionId, CancellationToken cancellationToken = default);

    Task<ProviderPlanChangeResult> ChangeSubscriptionPlanAsync(ChangeProviderSubscriptionPlanRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Garante que um cupom exista no provedor com os parâmetros informados, de forma idempotente.
    /// Necessário para que o desconto calculado pelo backend seja efetivamente aplicado no checkout.
    /// </summary>
    Task<ProviderCouponSyncResult> EnsureCouponAsync(ProviderCouponDefinition coupon, CancellationToken cancellationToken = default);
}
