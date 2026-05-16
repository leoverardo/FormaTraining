namespace FitPlatform.Application.Configuration;

public class AbacatePayOptions
{
    public const string SectionName = "AbacatePay";

    public string BaseUrl { get; set; } = "https://api.abacatepay.com/v2";
    public string ApiKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string WebhookPublicKey { get; set; } = string.Empty;
    public string SuccessUrl { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public bool DevMode { get; set; }
}
