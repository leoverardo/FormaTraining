namespace FitPlatform.Application.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateTime LocalNow { get; }
    DateTime LocalDate { get; }
}
