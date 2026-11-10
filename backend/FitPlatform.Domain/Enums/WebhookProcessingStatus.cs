namespace FitPlatform.Domain.Enums;

/// <summary>
/// Ciclo de vida de um <c>PaymentWebhookLog</c>.
///
/// Terminais (nunca reprocessados pelo retry, e respondidos como duplicata em uma
/// reentrega): <see cref="Processed"/>, <see cref="Duplicate"/>, <see cref="Ignored"/>,
/// <see cref="IgnoredDevMode"/>, <see cref="Abandoned"/>.
///
/// Elegível a retry: apenas <see cref="Failed"/>, e ainda assim somente quando
/// <c>RetryCount &lt; WebhookMaxRetries</c> e <c>NextRetryAt &lt;= agora</c>.
/// </summary>
public enum WebhookProcessingStatus
{
    /// <summary>
    /// Evento recebido e registrado, ainda não processado ("Received").
    /// Estado inicial da entidade; na prática o recebimento já grava
    /// <see cref="Processing"/> por ser imediatamente seguido do processamento.
    /// </summary>
    Pending = 0,

    /// <summary>Processamento em andamento (proteção contra re-entradas concorrentes).</summary>
    Processing = 1,

    /// <summary>Evento processado com sucesso. Terminal.</summary>
    Processed = 2,

    /// <summary>Processamento falhou; elegível a retry até esgotar as tentativas.</summary>
    Failed = 3,

    /// <summary>
    /// Reentrega concorrente do mesmo evento, descartada enquanto outra execução
    /// já detinha o registro. Terminal.
    /// </summary>
    Duplicate = 4,

    /// <summary>
    /// Excedeu o número máximo de tentativas de reprocessamento e deixou de ser retentado.
    /// Exige investigação manual — consultar ErrorMessage e RawPayload. Terminal.
    /// </summary>
    Abandoned = 5,

    /// <summary>
    /// Evento recebido em modo sandbox (devMode) por uma instância configurada como produção.
    /// Registrado para auditoria, mas deliberadamente não aplicado ao domínio. Terminal.
    /// </summary>
    IgnoredDevMode = 6,

    /// <summary>
    /// Evento autêntico e íntegro, porém sem ação de negócio associada — tipo de evento
    /// que a plataforma não trata, ou payload sem conteúdo aplicável.
    /// Registrado para auditoria e nunca retentado. Terminal.
    /// </summary>
    Ignored = 7
}
