using FitPlatform.Application.Interfaces;

namespace FitPlatform.Api.Services;

public class SaoPauloDateTimeProvider : IDateTimeProvider
{
    private static readonly TimeZoneInfo SaoPauloTz = ResolveTimeZone();

    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime LocalNow => TimeZoneInfo.ConvertTimeFromUtc(UtcNow, SaoPauloTz);
    public DateTime LocalDate => LocalNow.Date;

    private static TimeZoneInfo ResolveTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        }
        catch
        {
            return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        }
    }
}
