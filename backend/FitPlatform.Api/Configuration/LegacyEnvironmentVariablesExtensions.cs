namespace FitPlatform.Api.Configuration;

/// <summary>
/// Compatibilidade com os nomes planos de variáveis de ambiente usados antes da
/// padronização (ABACATEPAY_API_KEY, CLOUDINARY_API_KEY, ...).
///
/// A forma canônica passa a ser o separador padrão do ASP.NET Core:
///   AbacatePay__ApiKey, AbacatePay__EnableSignatureValidation, Platform__FrontendBaseUrl, ...
///
/// Regras aplicadas aqui:
///   • variáveis legadas vazias/ausentes são IGNORADAS — antes elas eram inseridas como
///     null no provider de maior precedência e apagavam os valores do appsettings.json;
///   • a forma canônica tem prioridade: uma chave já definida por AbacatePay__* ou
///     appsettings não é sobrescrita pelo nome legado.
/// </summary>
public static class LegacyEnvironmentVariablesExtensions
{
    private static readonly (string LegacyVariable, string ConfigurationKey)[] Mappings =
    [
        ("CLOUDINARY_CLOUD_NAME", "Cloudinary:CloudName"),
        ("CLOUDINARY_API_KEY", "Cloudinary:ApiKey"),
        ("CLOUDINARY_API_SECRET", "Cloudinary:ApiSecret"),
        ("CLOUDINARY_FOLDER", "Cloudinary:Folder"),
        ("ABACATEPAY_BASE_URL", "AbacatePay:BaseUrl"),
        ("ABACATEPAY_API_KEY", "AbacatePay:ApiKey"),
        ("ABACATEPAY_WEBHOOK_SECRET", "AbacatePay:WebhookSecret"),
        ("ABACATEPAY_WEBHOOK_PUBLIC_KEY", "AbacatePay:WebhookPublicKey"),
        ("ABACATEPAY_SUCCESS_URL", "AbacatePay:SuccessUrl"),
        ("ABACATEPAY_RETURN_URL", "AbacatePay:ReturnUrl"),
        ("ABACATEPAY_DEV_MODE", "AbacatePay:DevMode"),
        ("ABACATEPAY_ENABLE_SIGNATURE_VALIDATION", "AbacatePay:EnableSignatureValidation"),
        ("PLATFORM_FRONTEND_BASE_URL", "Platform:FrontendBaseUrl"),
        ("PLATFORM_API_BASE_URL", "Platform:ApiBaseUrl")
    ];

    /// <summary>
    /// Aplica os nomes legados sem sobrescrever o que já estiver configurado.
    /// </summary>
    public static IConfigurationBuilder AddLegacyEnvironmentVariableAliases(
        this IConfigurationBuilder builder, IConfiguration current)
    {
        var overrides = new Dictionary<string, string?>();

        foreach (var (legacyVariable, configurationKey) in Mappings)
        {
            var value = Environment.GetEnvironmentVariable(legacyVariable);

            // Nunca inserir null/vazio: isso apagaria o valor vindo do appsettings.
            if (string.IsNullOrWhiteSpace(value)) continue;

            // A forma canônica (AbacatePay__ApiKey / appsettings) vence o alias legado.
            // Placeholders "SET_..." do appsettings contam como não configurados.
            var currentValue = current[configurationKey];
            if (!string.IsNullOrWhiteSpace(currentValue) &&
                !currentValue.StartsWith("SET_", StringComparison.Ordinal)) continue;

            overrides[configurationKey] = value;
        }

        if (overrides.Count > 0) builder.AddInMemoryCollection(overrides);

        return builder;
    }
}
