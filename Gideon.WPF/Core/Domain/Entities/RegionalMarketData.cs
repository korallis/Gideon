// ==========================================================================
// RegionalMarketData.cs - Regional Market Coverage Entities
// ==========================================================================
// Domain entities for regional market coverage and trade hub management.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gideon.WPF.Core.Domain.Entities;

/// <summary>
/// Major trade hub information
/// </summary>
[Table("TradeHubs")]
public class TradeHub
{
    [Key]
    public int TradeHubId { get; set; }

    [Required]
    [StringLength(100)]
    public string HubName { get; set; } = string.Empty;

    public int RegionId { get; set; }

    public int SolarSystemId { get; set; }

    public long? StationId { get; set; }

    public long? StructureId { get; set; }

    [Required]
    [StringLength(50)]
    public string HubType { get; set; } = string.Empty; // Station, Citadel, Engineering Complex

    public TradeHubTier Tier { get; set; } = TradeHubTier.Regional;

    public decimal AverageDailyVolume { get; set; }

    public int ActiveTraders { get; set; }

    public double MarketDepth { get; set; }

    public double LiquidityScore { get; set; }

    public decimal TransactionFeeRate { get; set; }

    public decimal BrokerFeeRate { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsPlayerOwned { get; set; }

    public string? OwningCorporation { get; set; }

    public string? OwningAlliance { get; set; }

    public DateTime LastMarketUpdate { get; set; }

    public DateTime LastAccessibilityCheck { get; set; }

    public bool IsAccessible { get; set; } = true;

    public string? AccessibilityNotes { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("RegionId")]
    public virtual EveRegion? Region { get; set; }

    [ForeignKey("SolarSystemId")]
    public virtual EveSolarSystem? SolarSystem { get; set; }

    [ForeignKey("StationId")]
    public virtual EveStation? Station { get; set; }

    public virtual ICollection<RegionalMarketCoverage> MarketCoverage { get; set; } = new List<RegionalMarketCoverage>();
    public virtual ICollection<TradeRoute> OriginRoutes { get; set; } = new List<TradeRoute>();
    public virtual ICollection<TradeRoute> DestinationRoutes { get; set; } = new List<TradeRoute>();
    public virtual ICollection<MarketData> MarketData { get; set; } = new List<MarketData>();
}

/// <summary>
/// Regional market coverage tracking
/// </summary>
[Table("RegionalMarketCoverage")]
public class RegionalMarketCoverage
{
    [Key]
    public int CoverageId { get; set; }

    public int RegionId { get; set; }

    public int TradeHubId { get; set; }

    public int MarketGroupId { get; set; }

    public double CoveragePercentage { get; set; }

    public int TrackedTypes { get; set; }

    public int TotalTypesInGroup { get; set; }

    public DateTime LastSyncDate { get; set; }

    public TimeSpan SyncFrequency { get; set; } = TimeSpan.FromMinutes(15);

    public DateTime NextSyncDue { get; set; }

    public bool IsEnabled { get; set; } = true;

    public int Priority { get; set; } = 1; // 1=High, 2=Medium, 3=Low

    public MarketDataQuality DataQuality { get; set; } = MarketDataQuality.Good;

    public string? QualityNotes { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("RegionId")]
    public virtual EveRegion? Region { get; set; }

    [ForeignKey("TradeHubId")]
    public virtual TradeHub? TradeHub { get; set; }

    [ForeignKey("MarketGroupId")]
    public virtual EveMarketGroup? MarketGroup { get; set; }
}

/// <summary>
/// Trade route between hubs
/// </summary>
[Table("TradeRoutes")]
public class TradeRoute
{
    [Key]
    public int RouteId { get; set; }

    public int OriginHubId { get; set; }

    public int DestinationHubId { get; set; }

    [Required]
    [StringLength(100)]
    public string RouteName { get; set; } = string.Empty;

    public double DistanceLightYears { get; set; }

    public int JumpCount { get; set; }

    public double SecurityStatus { get; set; }

    public TradeRouteRisk RiskLevel { get; set; } = TradeRouteRisk.Low;

    public TimeSpan EstimatedTravelTime { get; set; }

    public decimal TransportCostPerM3 { get; set; }

    public decimal MaxProfitMargin { get; set; }

    public decimal AverageProfitMargin { get; set; }

    public long DailyVolumeM3 { get; set; }

    public bool IsActive { get; set; } = true;

    public bool RequiresSpecialShip { get; set; }

    public string? SpecialRequirements { get; set; }

    public DateTime LastProfitabilityCheck { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("OriginHubId")]
    public virtual TradeHub? OriginHub { get; set; }

    [ForeignKey("DestinationHubId")]
    public virtual TradeHub? DestinationHub { get; set; }

    public virtual ICollection<RouteItemProfitability> ItemProfitability { get; set; } = new List<RouteItemProfitability>();
}

/// <summary>
/// Item profitability on specific route
/// </summary>
[Table("RouteItemProfitability")]
public class RouteItemProfitability
{
    [Key]
    public int ProfitabilityId { get; set; }

    public int RouteId { get; set; }

    public int TypeId { get; set; }

    public decimal BuyPrice { get; set; }

    public decimal SellPrice { get; set; }

    public decimal ProfitPerUnit { get; set; }

    public double ProfitMargin { get; set; }

    public long DailyVolume { get; set; }

    public decimal DailyPotentialProfit { get; set; }

    public long BuyOrderCount { get; set; }

    public long SellOrderCount { get; set; }

    public double CompetitionIndex { get; set; }

    public double VelocityScore { get; set; } // How fast items sell

    public MarketTrend PriceTrend { get; set; } = MarketTrend.Stable;

    public bool IsRecommended { get; set; }

    public string? Warnings { get; set; }

    public DateTime CalculationDate { get; set; } = DateTime.UtcNow;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("RouteId")]
    public virtual TradeRoute? Route { get; set; }

    [ForeignKey("TypeId")]
    public virtual EveType? ItemType { get; set; }
}

/// <summary>
/// Market sync job tracking
/// </summary>
[Table("MarketSyncJobs")]
public class MarketSyncJob
{
    [Key]
    public int JobId { get; set; }

    [Required]
    [StringLength(100)]
    public string JobName { get; set; } = string.Empty;

    public MarketSyncJobType JobType { get; set; }

    public int? RegionId { get; set; }

    public int? TradeHubId { get; set; }

    public int? MarketGroupId { get; set; }

    public MarketSyncJobStatus Status { get; set; } = MarketSyncJobStatus.Pending;

    public DateTime ScheduledTime { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? CompletionTime { get; set; }

    public TimeSpan? Duration => CompletionTime - StartTime;

    public int ItemsToSync { get; set; }

    public int ItemsCompleted { get; set; }

    public int ItemsErrored { get; set; }

    public double ProgressPercentage => ItemsToSync > 0 ? (double)ItemsCompleted / ItemsToSync * 100 : 0;

    public int Priority { get; set; } = 1;

    public int RetryCount { get; set; }

    public int MaxRetries { get; set; } = 3;

    public string? ErrorMessage { get; set; }

    public string? Configuration { get; set; } // JSON configuration

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("RegionId")]
    public virtual EveRegion? Region { get; set; }

    [ForeignKey("TradeHubId")]
    public virtual TradeHub? TradeHub { get; set; }

    [ForeignKey("MarketGroupId")]
    public virtual EveMarketGroup? MarketGroup { get; set; }
}

/// <summary>
/// Regional market statistics
/// </summary>
[Table("RegionalMarketStatistics")]
public class RegionalMarketStatistics
{
    [Key]
    public int StatisticsId { get; set; }

    public int RegionId { get; set; }

    public DateTime StatisticsDate { get; set; }

    public decimal TotalDailyVolume { get; set; }

    public decimal TotalDailyValue { get; set; }

    public int ActiveOrders { get; set; }

    public int UniqueTraders { get; set; }

    public int TrackedItems { get; set; }

    public decimal AverageSpread { get; set; }

    public double MarketEfficiency { get; set; }

    public double LiquidityIndex { get; set; }

    public double VolatilityIndex { get; set; }

    public decimal TopItemVolume { get; set; }

    public int TopItemTypeId { get; set; }

    public string? TopItemName { get; set; }

    public decimal AverageOrderSize { get; set; }

    public TimeSpan AverageOrderLifetime { get; set; }

    public double RegionActivityScore { get; set; }

    public DateTime CalculationDate { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("RegionId")]
    public virtual EveRegion? Region { get; set; }

    [ForeignKey("TopItemTypeId")]
    public virtual EveType? TopItem { get; set; }
}

/// <summary>
/// Enumerations
/// </summary>
public enum TradeHubTier
{
    Primary,     // Jita, Amarr, Dodixie, Rens
    Secondary,   // Regional hubs
    Regional,    // Smaller regional centers
    Local,       // Local trading posts
    Specialized  // Industry/PvP hubs
}

public enum TradeRouteRisk
{
    VeryLow,    // High-sec only
    Low,        // Mostly high-sec
    Medium,     // Some low-sec
    High,       // Low-sec or null-sec
    VeryHigh,   // Dangerous null-sec
    Extreme     // Wormhole or hostile territory
}

public enum MarketDataQuality
{
    Excellent,  // Fresh, complete data
    Good,       // Recent, mostly complete
    Fair,       // Somewhat stale or incomplete
    Poor,       // Old or very incomplete
    Unknown     // No data quality assessment
}

public enum MarketTrend
{
    Unknown,
    StronglyBullish,
    Bullish,
    Stable,
    Bearish,
    StronglyBearish,
    Volatile
}

public enum MarketSyncJobType
{
    FullRegionSync,
    TradeHubSync,
    MarketGroupSync,
    PriorityItemsSync,
    HistoricalDataSync,
    StatisticsUpdate
}

public enum MarketSyncJobStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled,
    Retrying
}