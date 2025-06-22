// ==========================================================================
// HistoricalDataBackgroundService.cs - Background Service for Historical Data Collection
// ==========================================================================
// Background service that continuously collects and aggregates historical market data.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gideon.WPF.Core.Application.Services;

/// <summary>
/// Background service for continuous historical market data collection
/// </summary>
public class HistoricalDataBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HistoricalDataBackgroundService> _logger;
    private readonly TimeSpan _collectionInterval = TimeSpan.FromMinutes(15); // Collect every 15 minutes
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24); // Cleanup daily
    
    private DateTime _lastCollectionRun = DateTime.MinValue;
    private DateTime _lastCleanupRun = DateTime.MinValue;

    public HistoricalDataBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<HistoricalDataBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Historical Data Background Service starting...");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;

                // Check if it's time for data collection
                if (now - _lastCollectionRun >= _collectionInterval)
                {
                    await PerformDataCollectionAsync(stoppingToken);
                    _lastCollectionRun = now;
                }

                // Check if it's time for cleanup
                if (now - _lastCleanupRun >= _cleanupInterval)
                {
                    await PerformDataCleanupAsync(stoppingToken);
                    _lastCleanupRun = now;
                }

                // Wait before next iteration
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Historical Data Background Service stopping...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Historical Data Background Service");
        }
    }

    /// <summary>
    /// Perform historical data collection
    /// </summary>
    private async Task PerformDataCollectionAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug("Starting scheduled historical data collection...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var collectionService = scope.ServiceProvider.GetRequiredService<IHistoricalDataCollectionService>();
            
            await collectionService.CollectHistoricalDataAsync(stoppingToken);
            
            _logger.LogDebug("Scheduled historical data collection completed");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Historical data collection was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during scheduled historical data collection");
            // Don't rethrow - let the service continue running
        }
    }

    /// <summary>
    /// Perform data cleanup
    /// </summary>
    private async Task PerformDataCleanupAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug("Starting scheduled historical data cleanup...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var collectionService = scope.ServiceProvider.GetRequiredService<IHistoricalDataCollectionService>();
            
            await collectionService.CleanupOldDataAsync(stoppingToken);
            
            _logger.LogDebug("Scheduled historical data cleanup completed");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Historical data cleanup was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during scheduled historical data cleanup");
            // Don't rethrow - let the service continue running
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Historical Data Background Service is stopping");
        return base.StopAsync(cancellationToken);
    }
}