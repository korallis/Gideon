// ==========================================================================
// IRegionalMarketService.cs - Regional Market Service Interface
// ==========================================================================
// Service interface for regional market coverage and trade hub management.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Core.Domain.Interfaces;

namespace Gideon.WPF.Core.Application.Services;

/// <summary>
/// Service interface for regional market management
/// </summary>
public interface IRegionalMarketService
{
    /// <summary>
    /// Get comprehensive market coverage overview
    /// </summary>
    Task<MarketCoverageOverview> GetMarketCoverageOverviewAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Set up market coverage for a region and market group
    /// </summary>
    Task<MarketCoverageSetupResult> SetupMarketCoverageAsync(MarketCoverageSetupRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get trade hub recommendations for a region
    /// </summary>
    Task<List<TradeHubRecommendation>> GetTradeHubRecommendationsAsync(int regionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyze trade route profitability
    /// </summary>
    Task<TradeRouteAnalysis> AnalyzeTradeRouteAsync(int originHubId, int destinationHubId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedule market data synchronization
    /// </summary>
    Task<MarketSyncScheduleResult> ScheduleMarketSyncAsync(MarketSyncScheduleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get arbitrage opportunities
    /// </summary>
    Task<ArbitrageAnalysisResult> GetArbitrageOpportunitiesAsync(ArbitrageSearchCriteria criteria, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update trade hub status and metrics
    /// </summary>
    Task UpdateTradeHubStatusAsync(int tradeHubId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get regional market health assessment
    /// </summary>
    Task<RegionalMarketHealth> GetRegionalMarketHealthAsync(int regionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate optimal trading strategy
    /// </summary>
    Task<TradingStrategy> CalculateOptimalTradingStrategyAsync(TradingStrategyRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get market synchronization status
    /// </summary>
    Task<MarketSyncStatus> GetMarketSyncStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate market coverage configuration
    /// </summary>
    Task<MarketCoverageValidationResult> ValidateMarketCoverageAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Market coverage overview
/// </summary>
public class MarketCoverageOverview
{
    public int TotalRegionsCovered { get; set; }
    public int TotalTradeHubs { get; set; }
    public int ActiveTradeHubs { get; set; }
    public int MarketGroupsCovered { get; set; }
    public double OverallCoveragePercentage { get; set; }
    public DateTime LastUpdateTime { get; set; }
    
    public List<RegionCoverageSummary> RegionSummaries { get; set; } = new();
    public List<TradeHubSummary> TopTradeHubs { get; set; } = new();
    public List<MarketGroupCoverageSummary> MarketGroupSummaries { get; set; } = new();
    public MarketCoverageHealth Health { get; set; } = new();
}

/// <summary>
/// Region coverage summary
/// </summary>
public class RegionCoverageSummary
{
    public int RegionId { get; set; }
    public string RegionName { get; set; } = string.Empty;
    public double CoveragePercentage { get; set; }
    public int TradeHubCount { get; set; }
    public int MarketGroupsCovered { get; set; }
    public DateTime LastSyncDate { get; set; }
    public MarketDataQuality DataQuality { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Trade hub summary
/// </summary>
public class TradeHubSummary
{
    public int TradeHubId { get; set; }
    public string HubName { get; set; } = string.Empty;
    public string RegionName { get; set; } = string.Empty;
    public TradeHubTier Tier { get; set; }
    public double LiquidityScore { get; set; }
    public decimal DailyVolume { get; set; }
    public int ActiveTraders { get; set; }
    public bool IsAccessible { get; set; }
    public DateTime LastUpdate { get; set; }
}

/// <summary>
/// Market group coverage summary
/// </summary>
public class MarketGroupCoverageSummary
{
    public int MarketGroupId { get; set; }
    public string MarketGroupName { get; set; } = string.Empty;
    public double AverageCoverage { get; set; }
    public int RegionsCovered { get; set; }
    public int ItemTypesCovered { get; set; }
    public DateTime LastSyncDate { get; set; }
}

/// <summary>
/// Market coverage health
/// </summary>
public class MarketCoverageHealth
{
    public string OverallStatus { get; set; } = string.Empty;
    public double HealthScore { get; set; }
    public int IssuesCount { get; set; }
    public int WarningsCount { get; set; }
    public List<string> Issues { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Market coverage setup request
/// </summary>
public class MarketCoverageSetupRequest
{
    public int RegionId { get; set; }
    public int MarketGroupId { get; set; }
    public int TradeHubId { get; set; }
    public TimeSpan SyncFrequency { get; set; } = TimeSpan.FromMinutes(15);
    public int Priority { get; set; } = 1;
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// Market coverage setup result
/// </summary>
public class MarketCoverageSetupResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int CoverageId { get; set; }
    public double EstimatedCoveragePercentage { get; set; }
    public List<string> Warnings { get; set; } = new();
    public DateTime NextSyncDue { get; set; }
}

/// <summary>
/// Trade hub recommendation
/// </summary>
public class TradeHubRecommendation
{
    public int TradeHubId { get; set; }
    public string HubName { get; set; } = string.Empty;
    public string RecommendationReason { get; set; } = string.Empty;
    public double Score { get; set; }
    public double LiquidityScore { get; set; }
    public decimal AverageDailyVolume { get; set; }
    public int ActiveTraders { get; set; }
    public TradeHubTier Tier { get; set; }
    public List<string> Advantages { get; set; } = new();
    public List<string> Disadvantages { get; set; } = new();
}

/// <summary>
/// Trade route analysis
/// </summary>
public class TradeRouteAnalysis
{
    public int OriginHubId { get; set; }
    public string OriginHubName { get; set; } = string.Empty;
    public int DestinationHubId { get; set; }
    public string DestinationHubName { get; set; } = string.Empty;
    
    public double Distance { get; set; }
    public int JumpCount { get; set; }
    public TradeRouteRisk RiskLevel { get; set; }
    public TimeSpan EstimatedTravelTime { get; set; }
    
    public decimal AverageProfitMargin { get; set; }
    public decimal MaxProfitMargin { get; set; }
    public decimal DailyProfitPotential { get; set; }
    
    public List<RouteItemAnalysis> TopItems { get; set; } = new();
    public RouteRiskAssessment RiskAssessment { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Route item analysis
/// </summary>
public class RouteItemAnalysis
{
    public int TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public decimal BuyPrice { get; set; }
    public decimal SellPrice { get; set; }
    public decimal ProfitPerUnit { get; set; }
    public double ProfitMargin { get; set; }
    public long DailyVolume { get; set; }
    public decimal DailyPotentialProfit { get; set; }
    public double CompetitionLevel { get; set; }
    public MarketTrend PriceTrend { get; set; }
}

/// <summary>
/// Route risk assessment
/// </summary>
public class RouteRiskAssessment
{
    public TradeRouteRisk RiskLevel { get; set; }
    public double SecurityRating { get; set; }
    public List<string> RiskFactors { get; set; } = new();
    public List<string> SafetyRecommendations { get; set; } = new();
    public string PreferredShipType { get; set; } = string.Empty;
    public decimal RecommendedCargoValue { get; set; }
}

/// <summary>
/// Market sync schedule request
/// </summary>
public class MarketSyncScheduleRequest
{
    public MarketSyncJobType JobType { get; set; }
    public int? RegionId { get; set; }
    public int? TradeHubId { get; set; }
    public int? MarketGroupId { get; set; }
    public DateTime ScheduledTime { get; set; } = DateTime.UtcNow;
    public int Priority { get; set; } = 1;
    public string? Configuration { get; set; }
}

/// <summary>
/// Market sync schedule result
/// </summary>
public class MarketSyncScheduleResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int JobId { get; set; }
    public DateTime ScheduledTime { get; set; }
    public int EstimatedDurationMinutes { get; set; }
    public int QueuePosition { get; set; }
}

/// <summary>
/// Arbitrage search criteria
/// </summary>
public class ArbitrageSearchCriteria
{
    public decimal MinProfitMargin { get; set; } = 0.05m;
    public decimal MinProfitPerUnit { get; set; } = 1000m;
    public TradeRouteRisk MaxRiskLevel { get; set; } = TradeRouteRisk.Medium;
    public double MaxDistance { get; set; } = 50.0;
    public int MaxJumps { get; set; } = 10;
    public long MinDailyVolume { get; set; } = 100;
    public decimal MaxInvestment { get; set; } = 1000000000m;
    public List<int>? IncludeMarketGroups { get; set; }
    public List<int>? ExcludeMarketGroups { get; set; }
    public bool IncludeHighSecOnly { get; set; } = false;
}

/// <summary>
/// Arbitrage analysis result
/// </summary>
public class ArbitrageAnalysisResult
{
    public List<ArbitrageOpportunity> Opportunities { get; set; } = new();
    public int TotalOpportunities { get; set; }
    public decimal TotalPotentialProfit { get; set; }
    public ArbitrageMarketSummary MarketSummary { get; set; } = new();
    public List<string> MarketInsights { get; set; } = new();
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Arbitrage market summary
/// </summary>
public class ArbitrageMarketSummary
{
    public decimal AverageProfitMargin { get; set; }
    public decimal MedianProfitMargin { get; set; }
    public decimal HighestProfitMargin { get; set; }
    public int TopRegionId { get; set; }
    public string TopRegionName { get; set; } = string.Empty;
    public int TopMarketGroupId { get; set; }
    public string TopMarketGroupName { get; set; } = string.Empty;
    public List<string> TrendingItems { get; set; } = new();
}

/// <summary>
/// Regional market health
/// </summary>
public class RegionalMarketHealth
{
    public int RegionId { get; set; }
    public string RegionName { get; set; } = string.Empty;
    public double HealthScore { get; set; }
    public string HealthStatus { get; set; } = string.Empty;
    
    public MarketLiquidityHealth Liquidity { get; set; } = new();
    public MarketCompetitionHealth Competition { get; set; } = new();
    public MarketStabilityHealth Stability { get; set; } = new();
    public MarketGrowthHealth Growth { get; set; } = new();
    
    public List<string> StrengthAreas { get; set; } = new();
    public List<string> WeaknessAreas { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    
    public DateTime AssessmentDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Market health components
/// </summary>
public class MarketLiquidityHealth
{
    public double Score { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal AverageDailyVolume { get; set; }
    public double OrderDepth { get; set; }
    public TimeSpan AverageOrderLifetime { get; set; }
}

public class MarketCompetitionHealth
{
    public double Score { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ActiveTraders { get; set; }
    public double MarketConcentration { get; set; }
    public decimal AverageSpread { get; set; }
}

public class MarketStabilityHealth
{
    public double Score { get; set; }
    public string Status { get; set; } = string.Empty;
    public double PriceVolatility { get; set; }
    public double VolumeVolatility { get; set; }
    public int PriceShocks { get; set; }
}

public class MarketGrowthHealth
{
    public double Score { get; set; }
    public string Status { get; set; } = string.Empty;
    public double VolumeGrowthRate { get; set; }
    public double TraderGrowthRate { get; set; }
    public double NewItemsRate { get; set; }
}

/// <summary>
/// Trading strategy request
/// </summary>
public class TradingStrategyRequest
{
    public int CharacterId { get; set; }
    public decimal AvailableCapital { get; set; }
    public TradeRouteRisk RiskTolerance { get; set; } = TradeRouteRisk.Medium;
    public TimeSpan TimeHorizon { get; set; } = TimeSpan.FromDays(7);
    public TradingStyle PreferredStyle { get; set; } = TradingStyle.Balanced;
    public List<int>? PreferredRegions { get; set; }
    public List<int>? PreferredMarketGroups { get; set; }
    public bool IncludeArbitrage { get; set; } = true;
    public bool IncludeSpeculation { get; set; } = false;
}

/// <summary>
/// Trading strategy
/// </summary>
public class TradingStrategy
{
    public int CharacterId { get; set; }
    public string StrategyName { get; set; } = string.Empty;
    public TradingStyle Style { get; set; }
    public TradeRouteRisk RiskLevel { get; set; }
    public decimal ExpectedReturn { get; set; }
    public decimal MaximumLoss { get; set; }
    
    public List<TradingRecommendation> Recommendations { get; set; } = new();
    public PortfolioAllocation PortfolioAllocation { get; set; } = new();
    public RiskManagement RiskManagement { get; set; } = new();
    
    public DateTime StrategyDate { get; set; } = DateTime.UtcNow;
    public DateTime ValidUntil { get; set; }
}

/// <summary>
/// Trading recommendation
/// </summary>
public class TradingRecommendation
{
    public string RecommendationType { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal ExpectedProfit { get; set; }
    public double Confidence { get; set; }
    public string Action { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Portfolio allocation
/// </summary>
public class PortfolioAllocation
{
    public decimal ArbitrageAllocation { get; set; }
    public decimal SpeculationAllocation { get; set; }
    public decimal LiquidityAllocation { get; set; }
    public List<RegionAllocation> RegionAllocations { get; set; } = new();
    public List<MarketGroupAllocation> MarketGroupAllocations { get; set; } = new();
}

/// <summary>
/// Region allocation
/// </summary>
public class RegionAllocation
{
    public int RegionId { get; set; }
    public string RegionName { get; set; } = string.Empty;
    public decimal AllocationPercentage { get; set; }
    public string Rationale { get; set; } = string.Empty;
}

/// <summary>
/// Market group allocation
/// </summary>
public class MarketGroupAllocation
{
    public int MarketGroupId { get; set; }
    public string MarketGroupName { get; set; } = string.Empty;
    public decimal AllocationPercentage { get; set; }
    public string Rationale { get; set; } = string.Empty;
}

/// <summary>
/// Risk management
/// </summary>
public class RiskManagement
{
    public decimal MaxPositionSize { get; set; }
    public decimal StopLossPercentage { get; set; }
    public decimal TakeProfitPercentage { get; set; }
    public int MaxConcurrentTrades { get; set; }
    public TimeSpan MaxHoldingPeriod { get; set; }
    public List<string> RiskControls { get; set; } = new();
}

/// <summary>
/// Market sync status
/// </summary>
public class MarketSyncStatus
{
    public int TotalJobs { get; set; }
    public int PendingJobs { get; set; }
    public int RunningJobs { get; set; }
    public int CompletedJobs { get; set; }
    public int FailedJobs { get; set; }
    
    public DateTime LastSyncTime { get; set; }
    public DateTime NextScheduledSync { get; set; }
    public TimeSpan AverageSyncDuration { get; set; }
    
    public List<MarketSyncJob> RecentJobs { get; set; } = new();
    public List<MarketSyncJob> UpcomingJobs { get; set; } = new();
    public SyncPerformanceMetrics Performance { get; set; } = new();
}

/// <summary>
/// Sync performance metrics
/// </summary>
public class SyncPerformanceMetrics
{
    public double SuccessRate { get; set; }
    public TimeSpan AverageJobDuration { get; set; }
    public int ItemsPerMinute { get; set; }
    public double ErrorRate { get; set; }
    public string BottleneckArea { get; set; } = string.Empty;
}

/// <summary>
/// Market coverage validation result
/// </summary>
public class MarketCoverageValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    
    public int OrphanedCoverageRecords { get; set; }
    public int MissingTradeHubs { get; set; }
    public int OutdatedSyncSchedules { get; set; }
    public int InaccessibleHubs { get; set; }
    
    public DateTime ValidationDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Enumerations
/// </summary>
public enum TradingStyle
{
    Conservative,
    Balanced,
    Aggressive,
    Arbitrage,
    Speculation,
    LongTerm
}