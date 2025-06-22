// ==========================================================================
// MarketDataCache.cs - Market Data Caching Entities
// ==========================================================================
// Domain entities for market data caching with configurable refresh intervals.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gideon.WPF.Core.Domain.Entities;

/// <summary>
/// Market data cache entry
/// </summary>
[Table("MarketDataCache")]
public class MarketDataCacheEntry
{
    [Key]
    public int CacheId { get; set; }

    [Required]
    [StringLength(200)]
    public string CacheKey { get; set; } = string.Empty;

    public MarketDataType DataType { get; set; }

    public int RegionId { get; set; }

    public int? TypeId { get; set; }

    public long? LocationId { get; set; }

    [Required]
    public string CachedData { get; set; } = string.Empty; // JSON serialized data

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime ExpiryDate { get; set; }

    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromMinutes(15);

    public DateTime? LastAccessDate { get; set; }

    public int AccessCount { get; set; }

    public CacheStatus Status { get; set; } = CacheStatus.Fresh;

    public int DataSize { get; set; }

    public string? DataHash { get; set; }

    public DateTime? LastRefreshAttempt { get; set; }

    public DateTime? LastSuccessfulRefresh { get; set; }

    public int RefreshFailureCount { get; set; }

    public string? LastError { get; set; }

    public CachePriority Priority { get; set; } = CachePriority.Normal;

    public bool AutoRefresh { get; set; } = true;

    public string? Tags { get; set; } // Comma-separated cache tags

    // Navigation Properties
    [ForeignKey("RegionId")]
    public virtual EveRegion? Region { get; set; }

    [ForeignKey("TypeId")]
    public virtual EveType? ItemType { get; set; }
}

/// <summary>
/// Cache statistics and metrics
/// </summary>
[Table("CacheStatistics")]
public class CacheStatistics
{
    [Key]
    public int StatisticsId { get; set; }

    public DateTime StatisticsDate { get; set; }

    public int TotalCacheEntries { get; set; }

    public int FreshEntries { get; set; }

    public int StaleEntries { get; set; }

    public int ExpiredEntries { get; set; }

    public int ErrorEntries { get; set; }

    public long TotalCacheSize { get; set; }

    public double CacheHitRate { get; set; }

    public double CacheMissRate { get; set; }

    public TimeSpan AverageResponseTime { get; set; }

    public int RefreshOperations { get; set; }

    public int SuccessfulRefreshes { get; set; }

    public int FailedRefreshes { get; set; }

    public int CacheEvictions { get; set; }

    public int AutoRefreshCount { get; set; }

    public int ManualRefreshCount { get; set; }

    public MarketDataType MostAccessedDataType { get; set; }

    public int MostAccessedRegionId { get; set; }

    public string? MostAccessedRegionName { get; set; }

    public TimeSpan AverageRefreshDuration { get; set; }

    public DateTime CalculationDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Cache refresh schedule
/// </summary>
[Table("CacheRefreshSchedule")]
public class CacheRefreshSchedule
{
    [Key]
    public int ScheduleId { get; set; }

    [Required]
    [StringLength(100)]
    public string ScheduleName { get; set; } = string.Empty;

    public MarketDataType DataType { get; set; }

    public int? RegionId { get; set; }

    public int? TypeId { get; set; }

    public long? LocationId { get; set; }

    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromMinutes(15);

    public RefreshStrategy Strategy { get; set; } = RefreshStrategy.Incremental;

    public DateTime NextRefreshDue { get; set; }

    public DateTime? LastRefreshTime { get; set; }

    public bool IsEnabled { get; set; } = true;

    public CachePriority Priority { get; set; } = CachePriority.Normal;

    public int MaxRetries { get; set; } = 3;

    public int CurrentRetryCount { get; set; }

    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromMinutes(5);

    public string? Configuration { get; set; } // JSON configuration

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("RegionId")]
    public virtual EveRegion? Region { get; set; }

    [ForeignKey("TypeId")]
    public virtual EveType? ItemType { get; set; }
}

/// <summary>
/// Cache performance metrics
/// </summary>
[Table("CachePerformanceMetrics")]
public class CachePerformanceMetrics
{
    [Key]
    public int MetricId { get; set; }

    public DateTime MetricDate { get; set; }

    [Required]
    [StringLength(100)]
    public string MetricName { get; set; } = string.Empty;

    public MarketDataType? DataType { get; set; }

    public int? RegionId { get; set; }

    public double MetricValue { get; set; }

    public string? MetricUnit { get; set; }

    public string? MetricDescription { get; set; }

    public PerformanceMetricType MetricType { get; set; }

    public DateTime RecordedDate { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("RegionId")]
    public virtual EveRegion? Region { get; set; }
}

/// <summary>
/// Cache invalidation log
/// </summary>
[Table("CacheInvalidationLog")]
public class CacheInvalidationLog
{
    [Key]
    public int LogId { get; set; }

    [Required]
    [StringLength(200)]
    public string CacheKey { get; set; } = string.Empty;

    public MarketDataType DataType { get; set; }

    public InvalidationReason Reason { get; set; }

    public string? ReasonDetails { get; set; }

    public DateTime InvalidationDate { get; set; } = DateTime.UtcNow;

    public bool WasAutomatic { get; set; }

    public string? TriggeredBy { get; set; }

    public int AffectedEntries { get; set; }

    public TimeSpan ProcessingTime { get; set; }
}

/// <summary>
/// Cache configuration settings
/// </summary>
[Table("CacheConfiguration")]
public class CacheConfiguration
{
    [Key]
    public int ConfigurationId { get; set; }

    [Required]
    [StringLength(100)]
    public string ConfigurationKey { get; set; } = string.Empty;

    [Required]
    public string ConfigurationValue { get; set; } = string.Empty;

    public MarketDataType? AppliesTo { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsSystemConfiguration { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Enumerations
/// </summary>
public enum MarketDataType
{
    MarketOrders,
    MarketHistory,
    MarketStatistics,
    PersonalOrders,
    TransactionHistory,
    MarketPredictions,
    TradingOpportunities,
    ArbitrageOpportunities,
    MarketTrends,
    PriceAnalysis,
    VolumeAnalysis,
    RegionalStatistics,
    TradeRouteData,
    MarketDepth,
    FittingCalculation,
    ShipStatistics,
    ModuleEffects
}

public enum CacheStatus
{
    Fresh,          // Within refresh interval
    Stale,          // Past refresh interval but still usable
    Expired,        // Past expiry date, needs refresh
    Refreshing,     // Currently being refreshed
    Error,          // Refresh failed
    Evicted         // Removed from cache
}

public enum CachePriority
{
    Critical,       // Never evict, always refresh
    High,           // High priority refresh
    Normal,         // Standard priority
    Low,            // Lower priority, can be evicted
    Disposable      // Can be evicted anytime
}

public enum RefreshStrategy
{
    Incremental,    // Update only changed data
    Full,           // Complete refresh
    Delta,          // Fetch only differences
    Conditional,    // Refresh based on conditions
    OnDemand        // Refresh only when accessed
}

public enum PerformanceMetricType
{
    HitRate,
    MissRate,
    ResponseTime,
    RefreshDuration,
    CacheSize,
    EvictionRate,
    ErrorRate,
    ThroughputOps,
    MemoryUsage,
    NetworkBandwidth
}

public enum InvalidationReason
{
    Expired,
    ManualRefresh,
    DataChange,
    SizeLimit,
    MemoryPressure,
    ConfigurationChange,
    ErrorThreshold,
    UserRequest,
    SystemMaintenance,
    DataCorruption
}