using Microsoft.AspNetCore.Http;

namespace FitPlatform.Application.Interfaces;

public interface IPaymentWebhookValidator
{
    bool IsValid(HttpRequest request, string rawBody);
}
