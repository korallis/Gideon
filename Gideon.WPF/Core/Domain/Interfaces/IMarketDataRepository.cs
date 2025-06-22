// ==========================================================================
// IMarketDataRepository.cs - Market Data Repository Interface
// ==========================================================================
// Specialized repository interface for market data operations.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Gideon.WPF.Core.Domain.Entities;

namespace Gideon.WPF.Core.Domain.Interfaces;

/// <summary>
/// Repository interface for market data operations
/// </summary>
public interface IMarketDataRepository : IRepository<MarketData>
{
    /// <summary>
    /// Get market data by type and region
    /// </summary>
    Task<IEnumerable<MarketData>> GetByTypeAndRegionAsync(int typeId, int regionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get latest market data for type
    /// </summary>
    Task<MarketData?> GetLatestAsync(int typeId, int regionId, bool isBuyOrder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get market history for type
    /// </summary>
    Task<IEnumerable<MarketData>> GetHistoryAsync(int typeId, int regionId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get best buy/sell prices
    /// </summary>
    Task<(decimal? bestBuy, decimal? bestSell)> GetBestPricesAsync(int typeId, int regionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get trading opportunities
    /// </summary>
    Task<IEnumerable<TradingOpportunity>> GetTradingOpportunitiesAsync(double minProfitMargin = 0.1, OpportunityRisk maxRisk = OpportunityRisk.Medium, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get market predictions for type
    /// </summary>
    Task<IEnumerable<MarketPrediction>> GetPredictionsAsync(int typeId, int regionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get trending items
    /// </summary>
    Task<IEnumerable<MarketData>> GetTrendingItemsAsync(int regionId, int count = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clean old market data
    /// </summary>
    Task CleanOldDataAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update market data batch
    /// </summary>
    Task BulkUpdateMarketDataAsync(IEnumerable<MarketData> marketData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get market summary for region
    /// </summary>
    Task<MarketRegion?> GetRegionSummaryAsync(int regionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get historical price data with aggregation
    /// </summary>
    Task<IEnumerable<HistoricalPriceData>> GetHistoricalPricesAsync(
        int typeId, 
        int regionId, 
        DateTime fromDate, 
        DateTime toDate, 
        HistoricalDataInterval interval = HistoricalDataInterval.Daily,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get historical volume data with aggregation
    /// </summary>
    Task<IEnumerable<HistoricalVolumeData>> GetHistoricalVolumeAsync(
        int typeId, 
        int regionId, 
        DateTime fromDate, 
        DateTime toDate, 
        HistoricalDataInterval interval = HistoricalDataInterval.Daily,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Store historical market data points
    /// </summary>
    Task StoreHistoricalDataAsync(IEnumerable<HistoricalMarketData> historicalData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get aggregated market statistics for time period
    /// </summary>
    Task<MarketStatistics> GetMarketStatisticsAsync(
        int typeId, 
        int regionId, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default);
}