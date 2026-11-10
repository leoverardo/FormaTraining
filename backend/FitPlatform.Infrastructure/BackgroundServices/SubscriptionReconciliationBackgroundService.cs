using FitPlatform.Application.Configuration;
using FitPlatform.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitPlatform.Infrastructure.BackgroundServices;

/// <summary>
/// Dispara a reconciliação de assinaturas e o retry de webhooks em intervalos fixos.
///
/// Infraestrutura deliberadamente mínima: um <see cref="BackgroundService"/> com
/// <see cref="PeriodicTimer"/>, sem agendador externo. Cada execução abre seu próprio
/// escopo de DI (o runner e o DbContext são scoped) e é totalmente contida pelos limites
/// de <see cref="AbacatePayReconciliationOptions"/>.
///
/// Uma execução que falhe nunca derruba o host: a exceção é logada e a próxima
/// iteração segue normalmente.
/// </summary>
public sealed class SubscriptionReconciliationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AbacatePayOptions _options;
    private readonly ILogger<SubscriptionReconciliationBackgroundService> _logger;

    public SubscriptionReconciliationBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<AbacatePayOptions> options,
        ILogger<SubscriptionReconciliationBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = _options.Reconciliation;

        if (!settings.Enabled)
        {
            _logger.LogInformation("Reconciliação de assinaturas desabilitada por configuração.");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Max(1, settings.IntervalMinutes));

        _logger.LogInformation(
            "Reconciliação de assinaturas habilitada. Intervalo={IntervalMinutes}min Lote={BatchSize} " +
            "MaxTentativasPorAssinatura={MaxAttempts}",
            settings.IntervalMinutes, settings.BatchSize, settings.MaxAttemptsPerSubscription);

        // Aguarda um ciclo antes da primeira execução para não competir com o startup
        // (migrations e seed) nem com webhooks que ainda estão a caminho.
        using var timer = new PeriodicTimer(interval);

        while (await SafeWaitAsync(timer, stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var runner = scope.ServiceProvider.GetRequiredService<SubscriptionReconciliationRunner>();
                await runner.RunAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Execução da reconciliação falhou. A próxima iteração seguirá normalmente.");
            }
        }
    }

    private static async Task<bool> SafeWaitAsync(PeriodicTimer timer, CancellationToken stoppingToken)
    {
        try
        {
            return await timer.WaitForNextTickAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}
