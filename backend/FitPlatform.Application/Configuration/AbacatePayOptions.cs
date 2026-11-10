namespace FitPlatform.Application.Configuration;

/// <summary>
/// Configuração da integração AbacatePay.
///
/// Todas as chaves podem ser definidas por variável de ambiente usando o separador
/// padrão do ASP.NET Core (duplo underscore), por exemplo:
///   AbacatePay__ApiKey=abc_dev_xxx
///   AbacatePay__EnableSignatureValidation=true
///   AbacatePay__RetryPolicy__MaxRetry=3
///
/// Os nomes legados (ABACATEPAY_API_KEY, ABACATEPAY_WEBHOOK_SECRET, ...) continuam
/// sendo aceitos como fallback — ver <c>AbacatePayConfigurationExtensions</c>.
/// </summary>
public class AbacatePayOptions
{
    public const string SectionName = "AbacatePay";

    /// <summary>Base da API. Produção: https://api.abacatepay.com/v2</summary>
    public string BaseUrl { get; set; } = "https://api.abacatepay.com/v2";

    /// <summary>API key (Bearer). Nunca versionar — definir por variável de ambiente.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Secret recebido na query string do webhook (?webhookSecret=...).
    /// </summary>
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>
    /// Chave pública usada para validar a assinatura HMAC-SHA256 do header X-Webhook-Signature.
    /// </summary>
    public string WebhookPublicKey { get; set; } = string.Empty;

    /// <summary>
    /// URL para onde a AbacatePay redireciona após o pagamento concluído (completionUrl).
    /// Se vazia, é derivada de <c>Platform:FrontendBaseUrl</c> + <c>/onboarding/success</c>.
    /// </summary>
    public string SuccessUrl { get; set; } = string.Empty;

    /// <summary>
    /// URL para onde a AbacatePay redireciona quando o cliente desiste (returnUrl).
    /// Se vazia, é derivada de <c>Platform:FrontendBaseUrl</c> + <c>/onboarding/return</c>.
    /// </summary>
    public string ReturnUrl { get; set; } = string.Empty;

    /// <summary>
    /// Base dos redirects de checkouts iniciados pelo painel do trainer (sem onboarding).
    /// Recebe <c>?checkout=success</c> ou <c>?checkout=canceled</c>.
    /// Se vazia, é derivada de <c>Platform:FrontendBaseUrl</c> + <c>/trainer/subscription</c>.
    /// </summary>
    public string TrainerPanelUrl { get; set; } = string.Empty;

    /// <summary>
    /// Declara que esta instância opera contra o ambiente de testes da AbacatePay
    /// (API key de desenvolvimento, que marca todos os objetos com <c>devMode: true</c>).
    ///
    /// Comportamento real: quando <c>false</c>, webhooks que chegarem com
    /// <c>devMode: true</c> são registrados e descartados sem alterar assinaturas —
    /// impede que eventos de sandbox ativem assinaturas reais em produção.
    /// Quando <c>true</c>, eventos devMode são processados normalmente.
    /// </summary>
    public bool DevMode { get; set; }

    /// <summary>
    /// Habilita a validação da assinatura HMAC-SHA256 (header X-Webhook-Signature).
    /// DEVE ser true em produção. Em Development, se false, permite receber webhooks sem assinar
    /// (útil para testes locais com curl ou com a CLI da AbacatePay).
    /// Nunca desabilitar em produção — o sistema rejeita automaticamente webhooks não assinados
    /// fora do ambiente Development, independentemente desse flag.
    /// </summary>
    public bool EnableSignatureValidation { get; set; } = true;

    /// <summary>
    /// Método de pagamento usado no checkout de assinatura. A AbacatePay aceita
    /// "CARD" ou "PIX" para assinaturas.
    /// </summary>
    public string PaymentMethod { get; set; } = "CARD";

    /// <summary>
    /// Quando true, cupons locais aprovados são espelhados na AbacatePay (POST /coupons/create)
    /// e enviados no campo <c>coupons</c> do checkout, de modo que o valor efetivamente
    /// cobrado reflita o desconto calculado pelo backend.
    /// </summary>
    public bool EnableCouponSync { get; set; } = true;

    /// <summary>Política de retentativa de cobrança recorrente enviada à AbacatePay.</summary>
    public AbacatePayRetryPolicyOptions RetryPolicy { get; set; } = new();

    /// <summary>Parâmetros da rotina de reconciliação de assinaturas e webhooks.</summary>
    public AbacatePayReconciliationOptions Reconciliation { get; set; } = new();
}

/// <summary>
/// retryPolicy enviado em POST /v2/subscriptions/create.
/// Limites definidos pela API: maxRetry 1..10, retryEvery 1..30 (dias).
/// </summary>
public class AbacatePayRetryPolicyOptions
{
    public int MaxRetry { get; set; } = 3;
    public int RetryEvery { get; set; } = 1;
}

/// <summary>
/// Controles da rotina de reconciliação. Todos os limites existem para evitar
/// varredura indiscriminada e processamento infinito.
/// </summary>
public class AbacatePayReconciliationOptions
{
    /// <summary>Liga/desliga o BackgroundService de reconciliação.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Intervalo entre execuções.</summary>
    public int IntervalMinutes { get; set; } = 15;

    /// <summary>Idade mínima de uma assinatura Pending para ser reconciliada (evita corrida com o webhook).</summary>
    public int MinPendingAgeMinutes { get; set; } = 20;

    /// <summary>Idade máxima: assinaturas mais antigas que isso deixam de ser consultadas.</summary>
    public int MaxPendingAgeDays { get; set; } = 7;

    /// <summary>Máximo de assinaturas reconciliadas por execução.</summary>
    public int BatchSize { get; set; } = 25;

    /// <summary>Máximo de tentativas de reconciliação por assinatura antes de desistir.</summary>
    public int MaxAttemptsPerSubscription { get; set; } = 8;

    /// <summary>Máximo de páginas lidas de /subscriptions/list por execução (proteção contra loop).</summary>
    public int MaxListPages { get; set; } = 5;

    /// <summary>
    /// Liga a verificação de divergência em assinaturas já ativas (rede de segurança para
    /// webhooks de cancelamento/expiração perdidos e para a ausência de um evento de
    /// falha de cobrança no catálogo do provedor).
    /// </summary>
    public bool VerifyActiveSubscriptions { get; set; } = true;

    /// <summary>Máximo de assinaturas ativas verificadas por execução.</summary>
    public int ActiveBatchSize { get; set; } = 25;

    /// <summary>Intervalo mínimo entre duas verificações da mesma assinatura ativa.</summary>
    public int ActiveRecheckHours { get; set; } = 12;

    /// <summary>Máximo de webhooks em Failed reprocessados por execução.</summary>
    public int WebhookRetryBatchSize { get; set; } = 25;

    /// <summary>Máximo de tentativas de um webhook antes de marcá-lo como Abandoned.</summary>
    public int WebhookMaxRetries { get; set; } = 5;

    /// <summary>Backoff base (minutos) entre tentativas de reprocessamento de webhook.</summary>
    public int WebhookRetryBackoffMinutes { get; set; } = 5;
}
