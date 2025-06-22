// ==========================================================================
// MarketDataRepository.cs - Market Data Repository Implementation
// ==========================================================================
// Concrete implementation of market data repository with specialized operations.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Core.Domain.Interfaces;
using Gideon.WPF.Core.Infrastructure.Data;

namespace Gideon.WPF.Core.Infrastructure.Persistence;

/// <summary>
/// Market data repository implementation with specialized operations
/// </summary>
public class MarketDataRepository : Repository<MarketData>, IMarketDataRepository
{
    public MarketDataRepository(GideonDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get market data by type and region
    /// </summary>
    public async Task<IEnumerable<MarketData>> GetByTypeAndRegionAsync(int typeId, int regionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(m => m.ItemType)
            .Include(m => m.Region)
            .Where(m => m.TypeId == typeId && m.RegionId == regionId)
            .OrderByDescending(m => m.RecordedDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get latest market data for type
    /// </summary>
    public async Task<MarketData?> GetLatestAsync(int typeId, int regionId, bool isBuyOrder, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(m => m.ItemType)
            .Include(m => m.Region)
            .Where(m => m.TypeId == typeId && m.RegionId == regionId && m.IsBuyOrder == isBuyOrder)
            .OrderByDescending(m => m.RecordedDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Get market history for type
    /// </summary>
    public async Task<IEnumerable<MarketData>> GetHistoryAsync(int typeId, int regionId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(m => m.ItemType)
            .Include(m => m.Region)
            .Where(m => m.TypeId == typeId && 
                       m.RegionId == regionId && 
                       m.RecordedDate >= fromDate && 
                       m.RecordedDate <= toDate)
            .OrderBy(m => m.RecordedDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get best buy/sell prices
    /// </summary>
    public async Task<(decimal? bestBuy, decimal? bestSell)> GetBestPricesAsync(int typeId, int regionId, CancellationToken cancellationToken = default)
    {
        var latestData = await _dbSet
            .Where(m => m.TypeId == typeId && m.RegionId == regionId)
            .Where(m => m.RecordedDate >= DateTime.UtcNow.AddHours(-24)) // Last 24 hours
            .ToListAsync(cancellationToken);

        var bestBuy = latestData
            .Where(m => m.IsBuyOrder)
            .OrderByDescending(m => m.Price)
            .FirstOrDefault()?.Price;

        var bestSell = latestData
            .Where(m => !m.IsBuyOrder)
            .OrderBy(m => m.Price)
            .FirstOrDefault()?.Price;

        return (bestBuy, bestSell);
    }

    /// <summary>
    /// Get trading opportunities
    /// </summary>
    public async Task<IEnumerable<TradingOpportunity>> GetTradingOpportunitiesAsync(double minProfitMargin = 0.1, OpportunityRisk maxRisk = OpportunityRisk.Medium, CancellationToken cancellationToken = default)
    {
        return await _context.TradingOpportunities
            .Include(t => t.ItemType)
            .Include(t => t.SourceRegion)
            .Include(t => t.DestinationRegion)
            .Where(t => t.IsActive && 
                       t.ProfitMargin >= minProfitMargin && 
                       t.Risk <= maxRisk)
            .OrderByDescending(t => t.ProfitMargin)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get market predictions for type
    /// </summary>
    public async Task<IEnumerable<MarketPrediction>> GetPredictionsAsync(int typeId, int regionId, CancellationToken cancellationToken = default)
    {
        return await _context.MarketPredictions
            .Include(p => p.ItemType)
            .Include(p => p.Region)
            .Where(p => p.TypeId == typeId && p.RegionId == regionId)
            .OrderByDescending(p => p.PredictionDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get trending items
    /// </summary>
    public async Task<IEnumerable<MarketData>> GetTrendingItemsAsync(int regionId, int count = 20, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-7); // Last week
        
        return await _dbSet
            .Include(m => m.ItemType)
            .Where(m => m.RegionId == regionId && m.RecordedDate >= cutoffDate)
            .GroupBy(m => m.TypeId)
            .Select(g => new
            {
                TypeId = g.Key,
                VolumeChange = g.Sum(m => m.Volume),
                LatestData = g.OrderByDescending(m => m.RecordedDate).First()
            })
            .OrderByDescending(x => x.VolumeChange)
            .Take(count)
            .Select(x => x.LatestData)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Clean old market data
    /// </summary>
    public async Task CleanOldDataAsync(DateTime cutoffDate, CancellationToken cancellationToken = default)
    {
        var oldData = await _dbSet
            .Where(m => m.RecordedDate < cutoffDate)
            .ToListAsync(cancellationToken);

        if (oldData.Any())
        {
            _dbSet.RemoveRange(oldData);
        }
    }

    /// <summary>
    /// Update market data batch
    /// </summary>
    public async Task BulkUpdateMarketDataAsync(IEnumerable<MarketData> marketData, CancellationToken cancellationToken = default)
    {
        var dataList = marketData.ToList();
        
        // Remove existing data for the same types/regions/dates to avoid duplicates
        var typeRegionPairs = dataList
            .Select(m => new { m.TypeId, m.RegionId, Date = m.RecordedDate.Date })
            .Distinct()
            .ToList();

        foreach (var pair in typeRegionPairs)
        {
            var existingData = await _dbSet
                .Where(m => m.TypeId == pair.TypeId && 
                           m.RegionId == pair.RegionId && 
                           m.RecordedDate.Date == pair.Date)
                .ToListAsync(cancellationToken);

            if (existingData.Any())
            {
                _dbSet.RemoveRange(existingData);
            }
        }

        // Add new data
        await _dbSet.AddRangeAsync(dataList, cancellationToken);
    }

    /// <summary>
    /// Get market summary for region
    /// </summary>
    public async Task<MarketRegion?> GetRegionSummaryAsync(int regionId, CancellationToken cancellationToken = default)
    {
        return await _context.MarketRegions
            .Include(r => r.MarketData)
            .Include(r => r.TradingOpportunities)
            .FirstOrDefaultAsync(r => r.RegionId == regionId, cancellationToken);
    }

    /// <summary>
    /// Get historical price data with aggregation
    /// </summary>
    public async Task<IEnumerable<HistoricalPriceData>> GetHistoricalPricesAsync(
        int typeId, 
        int regionId, 
        DateTime fromDate, 
        DateTime toDate, 
        HistoricalDataInterval interval = HistoricalDataInterval.Daily,
        CancellationToken cancellationToken = default)
    {
        var historicalData = await _context.HistoricalMarketData
            .Where(h => h.TypeId == typeId && 
                       h.RegionId == regionId && 
                       h.Date >= fromDate && 
                       h.Date <= toDate &&
                       h.Interval == interval)
            .OrderBy(h => h.Date)
            .ToListAsync(cancellationToken);

        return historicalData.Select(h => new HistoricalPriceData
        {
            Date = h.Date,
            OpenPrice = h.OpenPrice,
            HighPrice = h.HighPrice,
            LowPrice = h.LowPrice,
            ClosePrice = h.ClosePrice,
            AveragePrice = h.AveragePrice,
            MedianPrice = h.MedianPrice,
            OrderCount = h.OrderCount
        });
    }

    /// <summary>
    /// Get historical volume data with aggregation
    /// </summary>
    public async Task<IEnumerable<HistoricalVolumeData>> GetHistoricalVolumeAsync(
        int typeId, 
        int regionId, 
        DateTime fromDate, 
        DateTime toDate, 
        HistoricalDataInterval interval = HistoricalDataInterval.Daily,
        CancellationToken cancellationToken = default)
    {
        var historicalData = await _context.HistoricalMarketData
            .Where(h => h.TypeId == typeId && 
                       h.RegionId == regionId && 
                       h.Date >= fromDate && 
                       h.Date <= toDate &&
                       h.Interval == interval)
            .OrderBy(h => h.Date)
            .ToListAsync(cancellationToken);

        return historicalData.Select(h => new HistoricalVolumeData
        {
            Date = h.Date,
            Volume = h.Volume,
            IskVolume = h.IskVolume,
            BuyOrderCount = h.BuyOrderCount,
            SellOrderCount = h.SellOrderCount,
            TotalOrderCount = h.OrderCount
        });
    }

    /// <summary>
    /// Store historical market data points
    /// </summary>
    public async Task StoreHistoricalDataAsync(IEnumerable<HistoricalMarketData> historicalData, CancellationToken cancellationToken = default)
    {
        var dataList = historicalData.ToList();
        
        // Remove existing data for the same types/regions/dates/intervals to avoid duplicates
        var dataKeys = dataList
            .Select(h => new { h.TypeId, h.RegionId, h.Date, h.Interval })
            .Distinct()
            .ToList();

        foreach (var key in dataKeys)
        {
            var existingData = await _context.HistoricalMarketData
                .Where(h => h.TypeId == key.TypeId && 
                           h.RegionId == key.RegionId && 
                           h.Date == key.Date &&
                           h.Interval == key.Interval)
                .ToListAsync(cancellationToken);

            if (existingData.Any())
            {
                _context.HistoricalMarketData.RemoveRange(existingData);
            }
        }

        // Add new historical data
        await _context.HistoricalMarketData.AddRangeAsync(dataList, cancellationToken);
    }

    /// <summary>
    /// Get aggregated market statistics for time period
    /// </summary>
    public async Task<MarketStatistics> GetMarketStatisticsAsync(
        int typeId, 
        int regionId, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default)
    {
        var marketData = await _dbSet
            .Where(m => m.TypeId == typeId && 
                       m.RegionId == regionId && 
                       m.RecordedDate >= fromDate && 
                       m.RecordedDate <= toDate)
            .ToListAsync(cancellationToken);

        if (!marketData.Any())
        {
            return new MarketStatistics
            {
                TypeId = typeId,
                RegionId = regionId,
                FromDate = fromDate,
                ToDate = toDate
            };
        }

        var prices = marketData.Select(m => m.Price).ToList();
        var volumes = marketData.Select(m => m.Volume).ToList();
        var iskVolumes = marketData.Select(m => m.Price * m.Volume).ToList();

        var dayCount = (toDate - fromDate).Days + 1;
        var totalVolume = volumes.Sum();
        var totalIskVolume = iskVolumes.Sum();

        // Calculate standard deviation
        var averagePrice = prices.Average();
        var variance = prices.Sum(p => Math.Pow((double)(p - averagePrice), 2)) / prices.Count;
        var standardDeviation = Math.Sqrt(variance);

        // Calculate volatility (coefficient of variation)
        var volatility = standardDeviation / (double)averagePrice;

        // Calculate trend direction (simple linear regression slope)
        var trendDirection = CalculateTrendDirection(marketData);

        // Calculate liquidity score based on order frequency and volume
        var liquidityScore = CalculateLiquidityScore(marketData);

        return new MarketStatistics
        {
            TypeId = typeId,
            RegionId = regionId,
            FromDate = fromDate,
            ToDate = toDate,

            MinPrice = prices.Min(),
            MaxPrice = prices.Max(),
            AveragePrice = averagePrice,
            MedianPrice = CalculateMedian(prices),
            StandardDeviation = (decimal)standardDeviation,

            TotalVolume = totalVolume,
            TotalIskVolume = totalIskVolume,
            AverageDailyVolume = totalVolume / dayCount,
            AverageDailyIskVolume = totalIskVolume / dayCount,

            TotalOrders = marketData.Count,
            TotalBuyOrders = marketData.Count(m => m.IsBuyOrder),
            TotalSellOrders = marketData.Count(m => !m.IsBuyOrder),

            Volatility = volatility,
            TrendDirection = trendDirection,
            LiquidityScore = liquidityScore
        };
    }

    /// <summary>
    /// Calculate median price from list
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
    /// Calculate trend direction using linear regression
    /// </summary>
    private static double CalculateTrendDirection(List<MarketData> marketData)
    {
        if (marketData.Count < 2) return 0;

        var orderedData = marketData.OrderBy(m => m.RecordedDate).ToList();
        var n = orderedData.Count;
        
        // Convert dates to numeric values (days since first date)
        var firstDate = orderedData.First().RecordedDate;
        var x = orderedData.Select((m, i) => (double)(m.RecordedDate - firstDate).TotalDays).ToList();
        var y = orderedData.Select(m => (double)m.Price).ToList();

        var sumX = x.Sum();
        var sumY = y.Sum();
        var sumXY = x.Zip(y, (xi, yi) => xi * yi).Sum();
        var sumX2 = x.Sum(xi => xi * xi);

        var slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        return slope;
    }

    /// <summary>
    /// Calculate liquidity score based on order frequency and volume
    /// </summary>
    private static double CalculateLiquidityScore(List<MarketData> marketData)
    {
        if (!marketData.Any()) return 0;

        var totalVolume = marketData.Sum(m => m.Volume);
        var orderCount = marketData.Count;
        var averageVolume = totalVolume / (double)orderCount;
        
        // Simple liquidity score: combination of order frequency and average volume
        var liquidityScore = Math.Log10(averageVolume + 1) * Math.Log10(orderCount + 1);
        
        return Math.Min(liquidityScore, 100); // Cap at 100
    }
}