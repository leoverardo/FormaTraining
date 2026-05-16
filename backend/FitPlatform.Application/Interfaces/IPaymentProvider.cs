using FitPlatform.Application.DTOs.Payment;

namespace FitPlatform.Application.Interfaces;

public interface IPaymentProvider
{
    Task<ProviderSubscriptionCreated> CreateSubscriptionAsync(CreateProviderSubscriptionRequest request, CancellationToken cancellationToken = default);
    Task<ProviderSubscriptionDetails?> GetSubscriptionAsync(string providerSubscriptionId, CancellationToken cancellationToken = default);
    Task CancelSubscriptionAsync(string providerSubscriptionId, CancellationToken cancellationToken = default);
    Task ChangeSubscriptionPlanAsync(ChangeProviderSubscriptionPlanRequest request, CancellationToken cancellationToken = default);
}
