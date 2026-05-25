namespace FitPlatform.Domain.Enums;

public enum WebhookProcessingStatus
{
    /// <summary>Evento recebido, ainda não processado.</summary>
    Pending = 0,

    /// <summary>Processamento em andamento (proteção contra re-entradas concorrentes).</summary>
    Processing = 1,

    /// <summary>Evento processado com sucesso.</summary>
    Processed = 2,

    /// <summary>Processamento falhou; pode ser tentado novamente.</summary>
    Failed = 3,

    /// <summary>Evento duplicado; ignorado por idempotência.</summary>
    Duplicate = 4
}
