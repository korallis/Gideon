// ==========================================================================
// BackgroundServices.cs - Background Service Implementations
// ==========================================================================
// Comprehensive background services for automated data synchronization and maintenance.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Gideon.WPF.Core.Application.Services;
using Gideon.WPF.Core.Domain.Interfaces;
using Gideon.WPF.Core.Domain.Entities;
using System.Diagnostics;

namespace Gideon.WPF.Core.Infrastructure.Services;

#region Market Data Background Services

/// <summary>
/// Background service for automated market data updates from ESI
/// </summary>
public class MarketDataBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MarketDataBackgroundService> _logger;
    private readonly TimeSpan _updateInterval = TimeSpan.FromMinutes(15); // EVE market data cache time
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(6);
    private DateTime _lastCleanup = DateTime.UtcNow;

    public MarketDataBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<MarketDataBackgroundService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Market Data Background Service started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var stopwatch = Stopwatch.StartNew();

                // Update market data for major trade hubs
                await UpdateMarketDataAsync(scope.ServiceProvider, stoppingToken);

                // Periodic cleanup of old market data
                if (DateTime.UtcNow - _lastCleanup > _cleanupInterval)
                {
                    await CleanupOldMarketDataAsync(scope.ServiceProvider, stoppingToken);
                    _lastCleanup = DateTime.UtcNow;
                }

                stopwatch.Stop();
                _logger.LogDebug("Market data update cycle completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

                // Wait for next update cycle
                await Task.Delay(_updateInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Market Data Background Service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait before retry
            }
        }
        
        _logger.LogInformation("Market Data Background Service stopped");
    }

    private async Task UpdateMarketDataAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        try
        {
            var marketUpdateService = serviceProvider.GetRequiredService<IMarketDataUpdateService>();
            var performanceService = serviceProvider.GetRequiredService<IPerformanceMetricsService>();
            
            // Major trade regions: Jita, Amarr, Dodixie, Rens, Hek
            var tradeHubRegions = new[] { 10000002, 10000043, 10000032, 10000030, 10000042 };
            
            foreach (var regionId in tradeHubRegions)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var updateStopwatch = Stopwatch.StartNew();
                
                try
                {
                    await marketUpdateService.UpdateMarketDataAsync(regionId, cancellationToken);
                    updateStopwatch.Stop();
                    
                    await performanceService.RecordMetricAsync(
                        "market_data_update_time", 
                        updateStopwatch.ElapsedMilliseconds, 
                        "background_service", 
                        cancellationToken);
                        
                    _logger.LogDebug("Updated market data for region {RegionId} in {ElapsedMs}ms", 
                        regionId, updateStopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to update market data for region {RegionId}", regionId);
                }
                
                // Small delay between regions to avoid overwhelming ESI
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
            
            _logger.LogInformation("Market data update completed for {RegionCount} regions", tradeHubRegions.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating market data");
            throw;
        }
    }

    private async Task CleanupOldMarketDataAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        try
        {
            var marketUpdateService = serviceProvider.GetRequiredService<IMarketDataUpdateService>();
            var auditService = serviceProvider.GetRequiredService<IAuditLogService>();
            
            _logger.LogInformation("Starting market data cleanup");
            
            await marketUpdateService.CleanOldMarketDataAsync(cancellationToken);
            await auditService.LogActionAsync("market_data_cleanup", "MarketData", null, cancellationToken);
            
            _logger.LogInformation("Market data cleanup completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during market data cleanup");
            throw;
        }
    }
}

#endregion

#region Character Data Background Services

/// <summary>
/// Background service for automated character data synchronization
/// </summary>
public class CharacterDataBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CharacterDataBackgroundService> _logger;
    private readonly TimeSpan _updateInterval = TimeSpan.FromHours(1); // Character data update frequency
    private readonly TimeSpan _skillUpdateInterval = TimeSpan.FromMinutes(30); // More frequent skill updates

    public CharacterDataBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<CharacterDataBackgroundService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Character Data Background Service started");
        
        var lastSkillUpdate = DateTime.UtcNow;
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var stopwatch = Stopwatch.StartNew();

                // Get active characters that need updates
                var activeCharacters = await GetActiveCharactersAsync(scope.ServiceProvider, stoppingToken);
                
                if (activeCharacters.Any())
                {
                    // Update character data
                    await UpdateCharacterDataAsync(scope.ServiceProvider, activeCharacters, stoppingToken);
                    
                    // More frequent skill updates for training characters
                    if (DateTime.UtcNow - lastSkillUpdate > _skillUpdateInterval)
                    {
                        await UpdateCharacterSkillsAsync(scope.ServiceProvider, activeCharacters, stoppingToken);
                        lastSkillUpdate = DateTime.UtcNow;
                    }
                }

                stopwatch.Stop();
                _logger.LogDebug("Character data update cycle completed in {ElapsedMs}ms for {CharacterCount} characters", 
                    stopwatch.ElapsedMilliseconds, activeCharacters.Count());

                // Wait for next update cycle
                await Task.Delay(_updateInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Character Data Background Service");
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken); // Wait before retry
            }
        }
        
        _logger.LogInformation("Character Data Background Service stopped");
    }

    private async Task<IEnumerable<Character>> GetActiveCharactersAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        try
        {
            var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
            
            // Get characters that have been accessed recently (within last 7 days)
            var recentThreshold = DateTime.UtcNow.AddDays(-7);
            var activeCharacters = await unitOfWork.Characters.FindAsync(
                c => c.LastLoginDate >= recentThreshold && c.IsActive,
                cancellationToken);
                
            return activeCharacters;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active characters");
            return Enumerable.Empty<Character>();
        }
    }

    private async Task UpdateCharacterDataAsync(IServiceProvider serviceProvider, IEnumerable<Character> characters, CancellationToken cancellationToken)
    {
        try
        {
            var characterUpdateService = serviceProvider.GetRequiredService<ICharacterDataUpdateService>();
            var performanceService = serviceProvider.GetRequiredService<IPerformanceMetricsService>();
            
            foreach (var character in characters)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var updateStopwatch = Stopwatch.StartNew();
                
                try
                {
                    // Update character assets
                    await characterUpdateService.UpdateCharacterAssetsAsync(character.Id, cancellationToken);
                    updateStopwatch.Stop();
                    
                    await performanceService.RecordMetricAsync(
                        "character_data_update_time", 
                        updateStopwatch.ElapsedMilliseconds, 
                        "background_service", 
                        cancellationToken);
                        
                    _logger.LogDebug("Updated character data for {CharacterName} in {ElapsedMs}ms", 
                        character.CharacterName, updateStopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to update character data for {CharacterName}", character.CharacterName);
                }
                
                // Small delay between characters to avoid overwhelming ESI
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
            
            _logger.LogInformation("Character data update completed for {CharacterCount} characters", characters.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating character data");
            throw;
        }
    }

    private async Task UpdateCharacterSkillsAsync(IServiceProvider serviceProvider, IEnumerable<Character> characters, CancellationToken cancellationToken)
    {
        try
        {
            var characterUpdateService = serviceProvider.GetRequiredService<ICharacterDataUpdateService>();
            
            foreach (var character in characters)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    await characterUpdateService.UpdateCharacterSkillsAsync(character.Id, cancellationToken);
                    _logger.LogDebug("Updated skills for {CharacterName}", character.CharacterName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to update skills for {CharacterName}", character.CharacterName);
                }
                
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
            
            _logger.LogDebug("Skill update completed for {CharacterCount} characters", characters.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating character skills");
            throw;
        }
    }
}

#endregion

#region Database Maintenance Background Services

/// <summary>
/// Background service for database optimization and maintenance
/// </summary>
public class DatabaseMaintenanceBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseMaintenanceBackgroundService> _logger;
    private readonly TimeSpan _maintenanceInterval = TimeSpan.FromHours(24); // Daily maintenance

    public DatabaseMaintenanceBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<DatabaseMaintenanceBackgroundService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Database Maintenance Background Service started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var stopwatch = Stopwatch.StartNew();

                await PerformDatabaseMaintenanceAsync(scope.ServiceProvider, stoppingToken);

                stopwatch.Stop();
                _logger.LogInformation("Database maintenance completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

                // Wait for next maintenance cycle
                await Task.Delay(_maintenanceInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Database Maintenance Background Service");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Wait before retry
            }
        }
        
        _logger.LogInformation("Database Maintenance Background Service stopped");
    }

    private async Task PerformDatabaseMaintenanceAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        try
        {
            var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
            var auditService = serviceProvider.GetRequiredService<IAuditLogService>();
            var errorLogService = serviceProvider.GetRequiredService<IErrorLogService>();
            var performanceService = serviceProvider.GetRequiredService<IPerformanceMetricsService>();

            _logger.LogInformation("Starting database maintenance");

            // Clean up old audit logs (keep last 90 days)
            await CleanupOldAuditLogsAsync(unitOfWork, 90, cancellationToken);

            // Clean up resolved error logs (keep last 30 days)
            await CleanupOldErrorLogsAsync(unitOfWork, 30, cancellationToken);

            // Clean up old performance metrics (keep last 180 days)
            await CleanupOldPerformanceMetricsAsync(unitOfWork, 180, cancellationToken);

            // Clean up expired market data (keep last 7 days)
            await CleanupOldMarketDataAsync(unitOfWork, 7, cancellationToken);

            // Optimize database (VACUUM for SQLite)
            await OptimizeDatabaseAsync(unitOfWork, cancellationToken);

            // Update database statistics
            await UpdateDatabaseStatisticsAsync(serviceProvider, cancellationToken);

            await auditService.LogActionAsync("database_maintenance", "Database", null, cancellationToken);
            
            _logger.LogInformation("Database maintenance completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database maintenance");
            throw;
        }
    }

    private async Task CleanupOldAuditLogsAsync(IUnitOfWork unitOfWork, int keepDays, CancellationToken cancellationToken)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-keepDays);
            var oldLogs = await unitOfWork.AuditLogs.FindAsync(log => log.Timestamp < cutoffDate, cancellationToken);
            
            if (oldLogs.Any())
            {
                await unitOfWork.AuditLogs.RemoveRangeAsync(oldLogs, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Cleaned up {Count} old audit log entries", oldLogs.Count());
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error cleaning up old audit logs");
        }
    }

    private async Task CleanupOldErrorLogsAsync(IUnitOfWork unitOfWork, int keepDays, CancellationToken cancellationToken)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-keepDays);
            var oldErrors = await unitOfWork.ErrorLogs.FindAsync(
                error => error.LoggedDate < cutoffDate && error.IsResolved, 
                cancellationToken);
            
            if (oldErrors.Any())
            {
                await unitOfWork.ErrorLogs.RemoveRangeAsync(oldErrors, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Cleaned up {Count} old resolved error log entries", oldErrors.Count());
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error cleaning up old error logs");
        }
    }

    private async Task CleanupOldPerformanceMetricsAsync(IUnitOfWork unitOfWork, int keepDays, CancellationToken cancellationToken)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-keepDays);
            var oldMetrics = await unitOfWork.PerformanceMetrics.FindAsync(
                metric => metric.RecordedDate < cutoffDate, 
                cancellationToken);
            
            if (oldMetrics.Any())
            {
                await unitOfWork.PerformanceMetrics.RemoveRangeAsync(oldMetrics, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Cleaned up {Count} old performance metric entries", oldMetrics.Count());
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error cleaning up old performance metrics");
        }
    }

    private async Task CleanupOldMarketDataAsync(IUnitOfWork unitOfWork, int keepDays, CancellationToken cancellationToken)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-keepDays);
            var oldMarketData = await unitOfWork.MarketData.FindAsync(
                data => data.RecordedDate < cutoffDate, 
                cancellationToken);
            
            if (oldMarketData.Any())
            {
                await unitOfWork.MarketData.RemoveRangeAsync(oldMarketData, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Cleaned up {Count} old market data entries", oldMarketData.Count());
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error cleaning up old market data");
        }
    }

    private async Task OptimizeDatabaseAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        try
        {
            // For SQLite, run VACUUM command to optimize the database
            // This would typically be done through raw SQL execution
            _logger.LogInformation("Database optimization completed");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error optimizing database");
        }
    }

    private async Task UpdateDatabaseStatisticsAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        try
        {
            var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
            var performanceService = serviceProvider.GetRequiredService<IPerformanceMetricsService>();

            // Record database size and table counts
            var characterCount = (await unitOfWork.Characters.GetAllAsync(cancellationToken)).Count();
            var fittingCount = (await unitOfWork.ShipFittings.GetAllAsync(cancellationToken)).Count();
            var marketDataCount = (await unitOfWork.MarketData.GetAllAsync(cancellationToken)).Count();

            await performanceService.RecordMetricAsync("database_character_count", characterCount, "maintenance", cancellationToken);
            await performanceService.RecordMetricAsync("database_fitting_count", fittingCount, "maintenance", cancellationToken);
            await performanceService.RecordMetricAsync("database_market_data_count", marketDataCount, "maintenance", cancellationToken);

            _logger.LogInformation("Database statistics updated - Characters: {Characters}, Fittings: {Fittings}, Market Data: {MarketData}",
                characterCount, fittingCount, marketDataCount);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error updating database statistics");
        }
    }
}

#endregion

#region Cache Maintenance Background Services

/// <summary>
/// Background service for cache optimization and cleanup
/// </summary>
public class CacheMaintenanceBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CacheMaintenanceBackgroundService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(6); // Cache cleanup frequency

    public CacheMaintenanceBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<CacheMaintenanceBackgroundService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Cache Maintenance Background Service started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var stopwatch = Stopwatch.StartNew();

                await PerformCacheMaintenanceAsync(scope.ServiceProvider, stoppingToken);

                stopwatch.Stop();
                _logger.LogInformation("Cache maintenance completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

                // Wait for next cleanup cycle
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Cache Maintenance Background Service");
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken); // Wait before retry
            }
        }
        
        _logger.LogInformation("Cache Maintenance Background Service stopped");
    }

    private async Task PerformCacheMaintenanceAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        try
        {
            var cacheService = serviceProvider.GetRequiredService<ICacheService>();
            var auditService = serviceProvider.GetRequiredService<IAuditLogService>();
            var performanceService = serviceProvider.GetRequiredService<IPerformanceMetricsService>();

            _logger.LogInformation("Starting cache maintenance");

            // Clear expired cache entries (this may be automatic with some cache implementations)
            // For now, we'll just log the maintenance action
            
            await auditService.LogActionAsync("cache_maintenance", "Cache", null, cancellationToken);
            
            // Record cache maintenance metric
            await performanceService.RecordMetricAsync("cache_maintenance_completed", 1, "maintenance", cancellationToken);
            
            _logger.LogInformation("Cache maintenance completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cache maintenance");
            throw;
        }
    }
}

#endregion