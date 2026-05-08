using Microsoft.AspNetCore.Http;

namespace FitPlatform.Application.Interfaces;

public interface IMercadoPagoWebhookValidator
{
    bool IsValid(HttpRequest request);
}
