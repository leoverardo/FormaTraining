using Microsoft.Extensions.Options;

namespace FitPlatform.Application.Configuration;

/// <summary>
/// Validação de startup das opções da AbacatePay. Registrada com ValidateOnStart()
/// para que uma configuração inválida derrube a aplicação no boot em vez de falhar
/// silenciosamente no primeiro checkout ou — pior — aceitar webhooks não autenticados.
///
/// Nenhuma mensagem de erro inclui o valor dos secrets; apenas o nome da chave ausente.
/// </summary>
public sealed class AbacatePayOptionsValidator : IValidateOptions<AbacatePayOptions>
{
    private static readonly string[] AllowedPaymentMethods = ["CARD", "PIX"];

    private readonly bool _requireProductionHardening;

    /// <param name="requireProductionHardening">
    /// True fora do ambiente Development. Ativa as exigências de secrets presentes e
    /// de validação de assinatura habilitada.
    /// </param>
    public AbacatePayOptionsValidator(bool requireProductionHardening)
    {
        _requireProductionHardening = requireProductionHardening;
    }

    public ValidateOptionsResult Validate(string? name, AbacatePayOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.BaseUrl) ||
            !Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri) ||
            (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps))
        {
            failures.Add("AbacatePay:BaseUrl deve ser uma URL absoluta http(s).");
        }

        if (!AllowedPaymentMethods.Contains(options.PaymentMethod, StringComparer.OrdinalIgnoreCase))
        {
            failures.Add("AbacatePay:PaymentMethod deve ser 'CARD' ou 'PIX'.");
        }

        if (options.RetryPolicy.MaxRetry is < 1 or > 10)
        {
            failures.Add("AbacatePay:RetryPolicy:MaxRetry deve estar entre 1 e 10.");
        }

        if (options.RetryPolicy.RetryEvery is < 1 or > 30)
        {
            failures.Add("AbacatePay:RetryPolicy:RetryEvery deve estar entre 1 e 30 dias.");
        }

        var reconciliation = options.Reconciliation;
        if (reconciliation.IntervalMinutes < 1)
            failures.Add("AbacatePay:Reconciliation:IntervalMinutes deve ser >= 1.");
        if (reconciliation.BatchSize is < 1 or > 500)
            failures.Add("AbacatePay:Reconciliation:BatchSize deve estar entre 1 e 500.");
        if (reconciliation.MaxAttemptsPerSubscription < 1)
            failures.Add("AbacatePay:Reconciliation:MaxAttemptsPerSubscription deve ser >= 1.");
        if (reconciliation.MaxListPages is < 1 or > 50)
            failures.Add("AbacatePay:Reconciliation:MaxListPages deve estar entre 1 e 50.");
        if (reconciliation.ActiveBatchSize is < 1 or > 500)
            failures.Add("AbacatePay:Reconciliation:ActiveBatchSize deve estar entre 1 e 500.");
        if (reconciliation.ActiveRecheckHours < 1)
            failures.Add("AbacatePay:Reconciliation:ActiveRecheckHours deve ser >= 1.");
        if (reconciliation.WebhookMaxRetries < 1)
            failures.Add("AbacatePay:Reconciliation:WebhookMaxRetries deve ser >= 1.");
        if (reconciliation.MinPendingAgeMinutes < 0)
            failures.Add("AbacatePay:Reconciliation:MinPendingAgeMinutes não pode ser negativo.");
        if (reconciliation.MaxPendingAgeDays < 1)
            failures.Add("AbacatePay:Reconciliation:MaxPendingAgeDays deve ser >= 1.");

        if (_requireProductionHardening)
        {
            if (string.IsNullOrWhiteSpace(options.ApiKey) || options.ApiKey.StartsWith("SET_", StringComparison.Ordinal))
                failures.Add("AbacatePay:ApiKey não configurada (defina AbacatePay__ApiKey).");

            if (string.IsNullOrWhiteSpace(options.WebhookSecret) || options.WebhookSecret.StartsWith("SET_", StringComparison.Ordinal))
                failures.Add("AbacatePay:WebhookSecret não configurado (defina AbacatePay__WebhookSecret).");

            if (string.IsNullOrWhiteSpace(options.WebhookPublicKey) || options.WebhookPublicKey.StartsWith("SET_", StringComparison.Ordinal))
                failures.Add("AbacatePay:WebhookPublicKey não configurada (defina AbacatePay__WebhookPublicKey).");

            if (!options.EnableSignatureValidation)
                failures.Add("AbacatePay:EnableSignatureValidation não pode ser false fora do ambiente Development.");

            if (string.IsNullOrWhiteSpace(options.SuccessUrl))
                failures.Add("AbacatePay:SuccessUrl vazia — defina AbacatePay__SuccessUrl ou Platform__FrontendBaseUrl.");

            if (string.IsNullOrWhiteSpace(options.ReturnUrl))
                failures.Add("AbacatePay:ReturnUrl vazia — defina AbacatePay__ReturnUrl ou Platform__FrontendBaseUrl.");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
