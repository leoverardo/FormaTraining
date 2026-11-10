namespace FitPlatform.Application.Configuration;

/// <summary>
/// URLs públicas da plataforma. Centraliza os domínios para que nenhuma URL de
/// produção fique hardcoded em código ou espalhada por serviços.
///
/// Variáveis de ambiente:
///   Platform__FrontendBaseUrl=https://app.seu-dominio.com
///   Platform__ApiBaseUrl=https://api.seu-dominio.com
///
/// <see cref="FrontendBaseUrl"/> alimenta os redirects de checkout da AbacatePay
/// (completionUrl/returnUrl) e os links de e-mail; <see cref="ApiBaseUrl"/> é o host
/// que deve ser cadastrado no painel da AbacatePay para receber o webhook.
/// </summary>
public class PlatformUrlOptions
{
    public const string SectionName = "Platform";

    /// <summary>Base do frontend (SPA). Sem barra final.</summary>
    public string FrontendBaseUrl { get; set; } = string.Empty;

    /// <summary>Base da API pública. Sem barra final.</summary>
    public string ApiBaseUrl { get; set; } = string.Empty;

    /// <summary>Caminho relativo da tela de retorno após pagamento concluído.</summary>
    public string OnboardingSuccessPath { get; set; } = "/onboarding/success";

    /// <summary>Caminho relativo da tela de retorno quando o cliente desiste do checkout.</summary>
    public string OnboardingReturnPath { get; set; } = "/onboarding/return";

    /// <summary>Caminho relativo da tela de assinatura do painel do trainer (retorno de checkouts feitos pelo painel).</summary>
    public string TrainerSubscriptionPath { get; set; } = "/trainer/subscription";

    /// <summary>Caminho do endpoint oficial de webhook da AbacatePay.</summary>
    public const string AbacatePayWebhookPath = "/api/webhooks/abacatepay";

    /// <summary>Concatena <see cref="FrontendBaseUrl"/> com um caminho relativo.</summary>
    public string BuildFrontendUrl(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(FrontendBaseUrl)) return string.Empty;
        return $"{FrontendBaseUrl.TrimEnd('/')}/{relativePath.TrimStart('/')}";
    }
}
