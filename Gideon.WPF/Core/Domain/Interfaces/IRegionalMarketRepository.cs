// ==========================================================================
// IRegionalMarketRepository.cs - Regional Market Repository Interface
// ==========================================================================
// Repository interface for regional market coverage and trade hub management.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Gideon.WPF.Core.Domain.Entities;

namespace Gideon.WPF.Core.Domain.Interfaces;

/// <summary>
/// Repository interface for regional market management
/// </summary>
public interface IRegionalMarketRepository : IRepository<RegionalMarketCoverage>
{
    /// <summary>
    /// Get all trade hubs by tier
    /// </summary>
    Task<IEnumerable<TradeHub>> GetTradeHubsByTierAsync(TradeHubTier tier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get trade hubs in a specific region
    /// </summary>
    Task<IEnumerable<TradeHub>> GetTradeHubsByRegionAsync(int regionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get market coverage for a region
    /// </summary>
    Task<IEnumerable<RegionalMarketCoverage>> GetMarketCoverageByRegionAsync(int regionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get market coverage that needs synchronization
    /// </summary>
    Task<IEnumerable<RegionalMarketCoverage>> GetCoverageDueForSyncAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get trade routes from a specific hub
    /// </summary>
    Task<IEnumerable<TradeRoute>> GetTradeRoutesFromHubAsync(int originHubId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get profitable trade routes by risk level
    /// </summary>
    Task<IEnumerable<TradeRoute>> GetProfitableRoutesByRiskAsync(TradeRouteRisk maxRisk, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get route profitability for specific items
    /// </summary>
    Task<IEnumerable<RouteItemProfitability>> GetRouteProfitabilityAsync(int routeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get top profitable items on a route
    /// </summary>
    Task<IEnumerable<RouteItemProfitability>> GetTopProfitableItemsAsync(int routeId, int count = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create or update market sync job
    /// </summary>
    Task<MarketSyncJob> CreateSyncJobAsync(MarketSyncJob job, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pending sync jobs
    /// </summary>
    Task<IEnumerable<MarketSyncJob>> GetPendingSyncJobsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Update sync job status
    /// </summary>
    Task UpdateSyncJobStatusAsync(int jobId, MarketSyncJobStatus status, string? errorMessage = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get regional market statistics
    /// </summary>
    Task<RegionalMarketStatistics?> GetRegionalStatisticsAsync(int regionId, DateTime date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update regional market statistics
    /// </summary>
    Task UpsertRegionalStatisticsAsync(RegionalMarketStatistics statistics, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get trade hub performance metrics
    /// </summary>
    Task<TradeHubMetrics> GetTradeHubMetricsAsync(int tradeHubId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update trade hub accessibility status
    /// </summary>
    Task UpdateTradeHubAccessibilityAsync(int tradeHubId, bool isAccessible, string? notes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get arbitrage opportunities between hubs
    /// </summary>
    Task<IEnumerable<ArbitrageOpportunity>> GetArbitrageOpportunitiesAsync(decimal minProfitMargin = 0.05m, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk update route item profitability
    /// </summary>
    Task BulkUpdateRouteProfitabilityAsync(IEnumerable<RouteItemProfitability> profitabilities, CancellationToken cancellationToken = default);
}

/// <summary>
/// Trade hub performance metrics
/// </summary>
public class TradeHubMetrics
{
    public int TradeHubId { get; set; }
    public string HubName { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    
    public decimal TotalVolume { get; set; }
    public decimal TotalValue { get; set; }
    public int TransactionCount { get; set; }
    public int UniqueTraders { get; set; }
    public int ActiveItems { get; set; }
    
    public decimal AverageSpread { get; set; }
    public double LiquidityIndex { get; set; }
    public double ActivityLevel { get; set; }
    
    public decimal TopItemVolume { get; set; }
    public string? TopItemName { get; set; }
    
    public List<HourlyActivity> HourlyBreakdown { get; set; } = new();
    public List<ItemTypeActivity> TopItems { get; set; } = new();
}

/// <summary>
/// Hourly trading activity
/// </summary>
public class HourlyActivity
{
    public int Hour { get; set; }
    public decimal Volume { get; set; }
    public int TransactionCount { get; set; }
    public double ActivityScore { get; set; }
}

/// <summary>
/// Item type activity
/// </summary>
public class ItemTypeActivity
{
    public int TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public decimal Volume { get; set; }
    public decimal Value { get; set; }
    public int TransactionCount { get; set; }
    public double MarketShare { get; set; }
}

/// <summary>
/// Arbitrage opportunity
/// </summary>
public class ArbitrageOpportunity
{
    public int ItemTypeId { get; set; }
    public string ItemTypeName { get; set; } = string.Empty;
    
    public int BuyHubId { get; set; }
    public string BuyHubName { get; set; } = string.Empty;
    public decimal BuyPrice { get; set; }
    public long BuyVolume { get; set; }
    
    public int SellHubId { get; set; }
    public string SellHubName { get; set; } = string.Empty;
    public decimal SellPrice { get; set; }
    public long SellVolume { get; set; }
    
    public decimal ProfitPerUnit { get; set; }
    public double ProfitMargin { get; set; }
    public long MaxVolume { get; set; }
    public decimal MaxProfit { get; set; }
    
    public double Distance { get; set; }
    public int JumpCount { get; set; }
    public TradeRouteRisk RiskLevel { get; set; }
    
    public DateTime OpportunityDate { get; set; } = DateTime.UtcNow;
    public TimeSpan EstimatedDuration { get; set; }
}