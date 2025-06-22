// ==========================================================================
// HistoricalDataCollectionService.cs - Historical Market Data Collection Service
// ==========================================================================
// Service for collecting and aggregating historical market data from real-time market orders.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Core.Domain.Interfaces;

namespace Gideon.WPF.Core.Application.Services;

/// <summary>
/// Service interface for historical market data collection
/// </summary>
public interface IHistoricalDataCollectionService
{
    /// <summary>
    /// Collect and aggregate market data for specified time periods
    /// </summary>
    Task CollectHistoricalDataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Aggregate raw market data into historical data points
    /// </summary>
    Task AggregateMarketDataAsync(
        int typeId, 
        int regionId, 
        DateTime fromDate, 
        DateTime toDate,
        HistoricalDataInterval interval,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Process and store historical data for specific item/region
    /// </summary>
    Task ProcessHistoricalDataAsync(
        int typeId, 
        int regionId, 
        HistoricalDataInterval interval,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clean up old historical data based on retention policy
    /// </summary>
    Task CleanupOldDataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get data collection statistics
    /// </summary>
    Task<HistoricalDataCollectionStats> GetCollectionStatsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Historical market data collection service implementation
/// </summary>
public class HistoricalDataCollectionService : IHistoricalDataCollectionService
{
    private readonly IMarketDataRepository _marketDataRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<HistoricalDataCollectionService> _logger;
    private readonly IServiceScope _serviceScope;

    public HistoricalDataCollectionService(
        IMarketDataRepository marketDataRepository,
        IUnitOfWork unitOfWork,
        ILogger<HistoricalDataCollectionService> logger,
        IServiceProvider serviceProvider)
    {
        _marketDataRepository = marketDataRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _serviceScope = serviceProvider.CreateScope();
    }

    /// <summary>
    /// Collect and aggregate market data for specified time periods
    /// </summary>
    public async Task CollectHistoricalDataAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting historical data collection...");

        try
        {
            // Get all active market regions
            var activeRegions = await GetActiveMarketRegionsAsync(cancellationToken);
            
            // Get top traded items for each region
            var topItems = await GetTopTradedItemsAsync(cancellationToken);

            var processedCount = 0;
            var totalItems = activeRegions.Count * topItems.Count;

            foreach (var regionId in activeRegions)
            {
                foreach (var typeId in topItems)
                {
                    await ProcessHistoricalDataAsync(typeId, regionId, HistoricalDataInterval.Hourly, cancellationToken);
                    await ProcessHistoricalDataAsync(typeId, regionId, HistoricalDataInterval.Daily, cancellationToken);
                    
                    processedCount++;
                    
                    if (processedCount % 100 == 0)
                    {
                        _logger.LogInformation("Processed {ProcessedCount}/{TotalItems} historical data aggregations", 
                            processedCount, totalItems);
                    }

                    // Check for cancellation
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            _logger.LogInformation("Historical data collection completed. Processed {ProcessedCount} items", processedCount);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Historical data collection was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during historical data collection");
            throw;
        }
    }

    /// <summary>
    /// Aggregate raw market data into historical data points
    /// </summary>
    public async Task AggregateMarketDataAsync(
        int typeId, 
        int regionId, 
        DateTime fromDate, 
        DateTime toDate,
        HistoricalDataInterval interval,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Aggregating market data for Type: {TypeId}, Region: {RegionId}, Interval: {Interval}", 
            typeId, regionId, interval);

        try
        {
            // Get raw market data for the period
            var marketData = await _marketDataRepository.GetHistoryAsync(typeId, regionId, fromDate, toDate, cancellationToken);
            var dataList = marketData.ToList();

            if (!dataList.Any())
            {
                _logger.LogDebug("No market data found for aggregation");
                return;
            }

            // Group data by the specified interval
            var groupedData = GroupDataByInterval(dataList, interval);
            var historicalDataPoints = new List<HistoricalMarketData>();

            foreach (var group in groupedData)
            {
                var periodData = group.Value.OrderBy(m => m.RecordedDate).ToList();
                
                var historicalData = new HistoricalMarketData
                {
                    TypeId = typeId,
                    RegionId = regionId,
                    Date = group.Key,
                    Interval = interval,
                    
                    // OHLC (Open, High, Low, Close) prices
                    OpenPrice = periodData.First().Price,
                    ClosePrice = periodData.Last().Price,
                    HighPrice = periodData.Max(m => m.Price),
                    LowPrice = periodData.Min(m => m.Price),
                    
                    // Volume data
                    Volume = periodData.Sum(m => m.Volume),
                    IskVolume = periodData.Sum(m => m.Price * m.Volume),
                    
                    // Order counts
                    OrderCount = periodData.Count,
                    BuyOrderCount = periodData.Count(m => m.IsBuyOrder),
                    SellOrderCount = periodData.Count(m => !m.IsBuyOrder),
                    
                    // Statistical data
                    AveragePrice = periodData.Average(m => m.Price),
                    MedianPrice = CalculateMedian(periodData.Select(m => m.Price).ToList()),
                    
                    CreatedAt = DateTime.UtcNow
                };

                historicalDataPoints.Add(historicalData);
            }

            // Store the aggregated data
            if (historicalDataPoints.Any())
            {
                await _marketDataRepository.StoreHistoricalDataAsync(historicalDataPoints, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                _logger.LogDebug("Stored {Count} historical data points for Type: {TypeId}, Region: {RegionId}", 
                    historicalDataPoints.Count, typeId, regionId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aggregating market data for Type: {TypeId}, Region: {RegionId}", typeId, regionId);
            throw;
        }
    }

    /// <summary>
    /// Process and store historical data for specific item/region
    /// </summary>
    public async Task ProcessHistoricalDataAsync(
        int typeId, 
        int regionId, 
        HistoricalDataInterval interval,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Determine the date range based on interval
            var (fromDate, toDate) = GetDateRangeForInterval(interval);
            
            // Check if we already have data for this period
            var existingData = await _marketDataRepository.GetHistoricalPricesAsync(
                typeId, regionId, fromDate, toDate, interval, cancellationToken);
            
            if (existingData.Any())
            {
                _logger.LogDebug("Historical data already exists for Type: {TypeId}, Region: {RegionId}, Interval: {Interval}", 
                    typeId, regionId, interval);
                return;
            }

            // Aggregate the data
            await AggregateMarketDataAsync(typeId, regionId, fromDate, toDate, interval, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing historical data for Type: {TypeId}, Region: {RegionId}, Interval: {Interval}", 
                typeId, regionId, interval);
            throw;
        }
    }

    /// <summary>
    /// Clean up old historical data based on retention policy
    /// </summary>
    public async Task CleanupOldDataAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting historical data cleanup...");

        try
        {
            // Define retention policies by interval
            var retentionPolicies = new Dictionary<HistoricalDataInterval, TimeSpan>
            {
                { HistoricalDataInterval.Hourly, TimeSpan.FromDays(30) },     // Keep hourly data for 30 days
                { HistoricalDataInterval.SixHourly, TimeSpan.FromDays(90) },  // Keep 6-hourly data for 90 days
                { HistoricalDataInterval.Daily, TimeSpan.FromDays(365) },     // Keep daily data for 1 year
                { HistoricalDataInterval.Weekly, TimeSpan.FromDays(1095) },   // Keep weekly data for 3 years
                { HistoricalDataInterval.Monthly, TimeSpan.FromDays(3650) }   // Keep monthly data for 10 years
            };

            var totalCleaned = 0;

            foreach (var policy in retentionPolicies)
            {
                var cutoffDate = DateTime.UtcNow - policy.Value;
                var cleanedCount = await CleanupDataForInterval(policy.Key, cutoffDate, cancellationToken);
                totalCleaned += cleanedCount;
                
                _logger.LogDebug("Cleaned {Count} {Interval} historical data records older than {CutoffDate}", 
                    cleanedCount, policy.Key, cutoffDate);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Historical data cleanup completed. Cleaned {TotalCleaned} records", totalCleaned);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during historical data cleanup");
            throw;
        }
    }

    /// <summary>
    /// Get data collection statistics
    /// </summary>
    public async Task<HistoricalDataCollectionStats> GetCollectionStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // This would require additional repository methods to get statistics
            // For now, return basic stats
            return new HistoricalDataCollectionStats
            {
                TotalRecords = 0, // Would need to implement repository method
                LastCollectionRun = DateTime.UtcNow,
                CollectionStatus = "Active",
                DataRetentionDays = 365
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting collection statistics");
            throw;
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Get active market regions (major trade hubs)
    /// </summary>
    private async Task<List<int>> GetActiveMarketRegionsAsync(CancellationToken cancellationToken)
    {
        // Major EVE Online trade hub regions
        return new List<int>
        {
            10000002, // The Forge (Jita)
            10000043, // Domain (Amarr)
            10000032, // Sinq Laison (Dodixie)
            10000030, // Heimatar (Rens)
            10000042  // Metropolis (Hek)
        };
    }

    /// <summary>
    /// Get top traded items for data collection
    /// </summary>
    private async Task<List<int>> GetTopTradedItemsAsync(CancellationToken cancellationToken)
    {
        // Common EVE Online item types (this would normally come from database analysis)
        return new List<int>
        {
            34,    // Tritanium
            35,    // Pyerite
            36,    // Mexallon
            37,    // Isogen
            38,    // Nocxium
            39,    // Zydrine
            40,    // Megacyte
            11399, // Morphite
            44992, // PLEX (30 Day)
            29668  // Skill Injector
        };
    }

    /// <summary>
    /// Group market data by the specified interval
    /// </summary>
    private Dictionary<DateTime, List<MarketData>> GroupDataByInterval(
        List<MarketData> marketData, 
        HistoricalDataInterval interval)
    {
        return interval switch
        {
            HistoricalDataInterval.Hourly => marketData
                .GroupBy(m => new DateTime(m.RecordedDate.Year, m.RecordedDate.Month, m.RecordedDate.Day, m.RecordedDate.Hour, 0, 0))
                .ToDictionary(g => g.Key, g => g.ToList()),
                
            HistoricalDataInterval.SixHourly => marketData
                .GroupBy(m => new DateTime(m.RecordedDate.Year, m.RecordedDate.Month, m.RecordedDate.Day, (m.RecordedDate.Hour / 6) * 6, 0, 0))
                .ToDictionary(g => g.Key, g => g.ToList()),
                
            HistoricalDataInterval.Daily => marketData
                .GroupBy(m => m.RecordedDate.Date)
                .ToDictionary(g => g.Key, g => g.ToList()),
                
            HistoricalDataInterval.Weekly => marketData
                .GroupBy(m => GetWeekStart(m.RecordedDate))
                .ToDictionary(g => g.Key, g => g.ToList()),
                
            HistoricalDataInterval.Monthly => marketData
                .GroupBy(m => new DateTime(m.RecordedDate.Year, m.RecordedDate.Month, 1))
                .ToDictionary(g => g.Key, g => g.ToList()),
                
            _ => throw new ArgumentException($"Unsupported interval: {interval}")
        };
    }

    /// <summary>
    /// Calculate median price from a list of prices
    /// </summary>
    private static decimal CalculateMedian(List<decimal> prices)
    {
        var sortedPrices = prices.OrderBy(p => p).ToList();
        var count = sortedPrices.Count;
        
        if (count % 2 == 0)
        {
            return (sortedPrices[count / 2 - 1] + sortedPrices[count / 2]) / 2;
        }
        else
        {
            return sortedPrices[count / 2];
        }
    }

    /// <summary>
    /// Get the start of the week for a given date
    /// </summary>
    private static DateTime GetWeekStart(DateTime date)
    {
        var diff = date.DayOfWeek - DayOfWeek.Monday;
        if (diff < 0) diff += 7;
        return date.AddDays(-diff).Date;
    }

    /// <summary>
    /// Get date range for the specified interval
    /// </summary>
    private static (DateTime fromDate, DateTime toDate) GetDateRangeForInterval(HistoricalDataInterval interval)
    {
        var now = DateTime.UtcNow;
        
        return interval switch
        {
            HistoricalDataInterval.Hourly => (now.AddHours(-1), now),
            HistoricalDataInterval.SixHourly => (now.AddHours(-6), now),
            HistoricalDataInterval.Daily => (now.AddDays(-1), now),
            HistoricalDataInterval.Weekly => (now.AddDays(-7), now),
            HistoricalDataInterval.Monthly => (now.AddDays(-30), now),
            _ => throw new ArgumentException($"Unsupported interval: {interval}")
        };
    }

    /// <summary>
    /// Clean up data for specific interval
    /// </summary>
    private async Task<int> CleanupDataForInterval(
        HistoricalDataInterval interval, 
        DateTime cutoffDate, 
        CancellationToken cancellationToken)
    {
        // This would require additional repository methods
        // For now, return 0 as placeholder
        return 0;
    }

    #endregion

    #region IDisposable Implementation

    public void Dispose()
    {
        _serviceScope?.Dispose();
    }

    #endregion
}

/// <summary>
/// Historical data collection statistics
/// </summary>
public class HistoricalDataCollectionStats
{
    public long TotalRecords { get; set; }
    public DateTime LastCollectionRun { get; set; }
    public string CollectionStatus { get; set; } = string.Empty;
    public int DataRetentionDays { get; set; }
    public Dictionary<HistoricalDataInterval, long> RecordsByInterval { get; set; } = new();
    public Dictionary<int, long> RecordsByRegion { get; set; } = new();
    public long TotalSizeBytes { get; set; }
}