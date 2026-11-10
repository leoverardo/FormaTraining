using System.Net;

namespace FitPlatform.Application.Exceptions;

/// <summary>
/// Falha na comunicação com o provedor de pagamento (rede, timeout, HTTP de erro
/// ou resposta com <c>success: false</c>).
///
/// A mensagem nunca carrega corpo de requisição/resposta, credenciais ou assinatura —
/// apenas endpoint e código de status, para que possa ser logada com segurança.
/// </summary>
public class PaymentProviderException : Exception
{
    public HttpStatusCode? StatusCode { get; }

    /// <summary>
    /// True quando o provedor indicou que o recurso já existe (409 ou mensagem equivalente).
    /// Permite tratar criações idempotentes — por exemplo, espelhar um cupom já cadastrado.
    /// </summary>
    public bool IndicatesAlreadyExists { get; }

    public PaymentProviderException(string message) : base(message)
    {
    }

    public PaymentProviderException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public PaymentProviderException(string message, HttpStatusCode? statusCode, bool indicatesAlreadyExists = false)
        : base(message)
    {
        StatusCode = statusCode;
        IndicatesAlreadyExists = indicatesAlreadyExists;
    }
}
