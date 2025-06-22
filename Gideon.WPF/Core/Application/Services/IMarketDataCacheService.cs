// ==========================================================================
// IMarketDataCacheService.cs - Market Data Cache Service Interface
// ==========================================================================
// Service interface for market data caching with 15-minute refresh functionality.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Core.Domain.Interfaces;

namespace Gideon.WPF.Core.Application.Services;

/// <summary>
/// Service interface for market data caching management
/// </summary>
public interface IMarketDataCacheService
{
    /// <summary>
    /// Get cached market data or fetch if not available
    /// </summary>
    Task<T?> GetOrFetchDataAsync<T>(string cacheKey, MarketDataType dataType, int regionId, int? typeId, long? locationId, Func<Task<T>> fetchFunction, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Store data in cache
    /// </summary>
    Task<bool> StoreDataAsync<T>(string cacheKey, MarketDataType dataType, int regionId, T data, TimeSpan? customRefreshInterval = null, int? typeId = null, long? locationId = null, CachePriority priority = CachePriority.Normal, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Invalidate cached data by key
    /// </summary>
    Task<bool> InvalidateCacheAsync(string cacheKey, InvalidationReason reason = InvalidationReason.UserRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidate cached data by criteria
    /// </summary>
    Task<int> InvalidateCacheByCriteriaAsync(MarketDataType? dataType = null, int? regionId = null, int? typeId = null, InvalidationReason reason = InvalidationReason.UserRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refresh cache entry
    /// </summary>
    Task<bool> RefreshCacheEntryAsync<T>(string cacheKey, Func<Task<T>> fetchFunction, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Start automatic cache refresh background service
    /// </summary>
    Task StartCacheRefreshServiceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop automatic cache refresh background service
    /// </summary>
    Task StopCacheRefreshServiceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get cache health and statistics
    /// </summary>
    Task<CacheHealthReport> GetCacheHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Perform cache maintenance
    /// </summary>
    Task<CacheMaintenanceResult> PerformCacheMaintenanceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Configure cache settings
    /// </summary>
    Task ConfigureCacheSettingsAsync(CacheSettingsRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get cache configuration
    /// </summary>
    Task<CacheSettings> GetCacheSettingsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Export cache data
    /// </summary>
    Task<CacheExportResult> ExportCacheDataAsync(CacheExportRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Import cache data
    /// </summary>
    Task<CacheImportResult> ImportCacheDataAsync(CacheImportRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get cache performance metrics
    /// </summary>
    Task<CachePerformanceReport> GetPerformanceMetricsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search cached data
    /// </summary>
    Task<List<CacheSearchResult>> SearchCacheAsync(CacheSearchCriteria criteria, CancellationToken cancellationToken = default);

    /// <summary>
    /// Warm up cache with frequently accessed data
    /// </summary>
    Task<CacheWarmupResult> WarmupCacheAsync(CacheWarmupRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Cache health report
/// </summary>
public class CacheHealthReport
{
    public bool IsHealthy { get; set; }
    public double HealthScore { get; set; }
    public CacheStatisticsData Statistics { get; set; } = new();
    public CacheIntegrityReport IntegrityReport { get; set; } = new();
    public List<string> Issues { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public DateTime ReportDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Cache maintenance result
/// </summary>
public class CacheMaintenanceResult
{
    public bool Success { get; set; }
    public int CleanedUpEntries { get; set; }
    public int EvictedEntries { get; set; }
    public int RefreshedEntries { get; set; }
    public int ErrorsFixed { get; set; }
    public long SpaceFreed { get; set; }
    public TimeSpan MaintenanceDuration { get; set; }
    public List<string> Actions { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public DateTime MaintenanceDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Cache settings request
/// </summary>
public class CacheSettingsRequest
{
    public TimeSpan DefaultRefreshInterval { get; set; } = TimeSpan.FromMinutes(15);
    public long MaxCacheSizeBytes { get; set; } = 1024L * 1024L * 1024L; // 1GB
    public int MaxCacheEntries { get; set; } = 100000;
    public bool AutoCleanupEnabled { get; set; } = true;
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromHours(6);
    public bool CompressionEnabled { get; set; } = true;
    public bool IntegrityCheckEnabled { get; set; } = true;
    public CachePriority DefaultPriority { get; set; } = CachePriority.Normal;
    public Dictionary<MarketDataType, TimeSpan> CustomRefreshIntervals { get; set; } = new();
}

/// <summary>
/// Cache settings
/// </summary>
public class CacheSettings
{
    public TimeSpan DefaultRefreshInterval { get; set; }
    public long MaxCacheSizeBytes { get; set; }
    public int MaxCacheEntries { get; set; }
    public bool AutoCleanupEnabled { get; set; }
    public TimeSpan CleanupInterval { get; set; }
    public bool CompressionEnabled { get; set; }
    public bool IntegrityCheckEnabled { get; set; }
    public CachePriority DefaultPriority { get; set; }
    public Dictionary<MarketDataType, TimeSpan> CustomRefreshIntervals { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Cache export request
/// </summary>
public class CacheExportRequest
{
    public string ExportPath { get; set; } = string.Empty;
    public List<MarketDataType>? DataTypes { get; set; }
    public List<int>? RegionIds { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool IncludeExpiredEntries { get; set; } = false;
    public bool CompressExport { get; set; } = true;
    public ExportFormat Format { get; set; } = ExportFormat.Json;
}

/// <summary>
/// Cache export result
/// </summary>
public class CacheExportResult
{
    public bool Success { get; set; }
    public string ExportPath { get; set; } = string.Empty;
    public int ExportedEntries { get; set; }
    public long ExportSizeBytes { get; set; }
    public TimeSpan ExportDuration { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime ExportDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Cache import request
/// </summary>
public class CacheImportRequest
{
    public string ImportPath { get; set; } = string.Empty;
    public bool OverwriteExisting { get; set; } = false;
    public bool ValidateIntegrity { get; set; } = true;
    public ImportStrategy Strategy { get; set; } = ImportStrategy.Merge;
}

/// <summary>
/// Cache import result
/// </summary>
public class CacheImportResult
{
    public bool Success { get; set; }
    public int ImportedEntries { get; set; }
    public int SkippedEntries { get; set; }
    public int OverwrittenEntries { get; set; }
    public int FailedEntries { get; set; }
    public TimeSpan ImportDuration { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public DateTime ImportDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Cache performance report
/// </summary>
public class CachePerformanceReport
{
    public TimeSpan ReportPeriod { get; set; }
    public int TotalRequests { get; set; }
    public int CacheHits { get; set; }
    public int CacheMisses { get; set; }
    public double HitRatePercentage { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
    public TimeSpan AverageRefreshTime { get; set; }
    public List<CachePerformanceMetrics> Metrics { get; set; } = new();
    public Dictionary<MarketDataType, PerformanceStats> StatsByDataType { get; set; } = new();
    public List<string> PerformanceInsights { get; set; } = new();
    public DateTime ReportDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Performance statistics
/// </summary>
public class PerformanceStats
{
    public int Requests { get; set; }
    public int Hits { get; set; }
    public int Misses { get; set; }
    public double HitRate { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
}

/// <summary>
/// Cache search criteria
/// </summary>
public class CacheSearchCriteria
{
    public string? SearchText { get; set; }
    public List<MarketDataType>? DataTypes { get; set; }
    public List<int>? RegionIds { get; set; }
    public List<int>? TypeIds { get; set; }
    public List<CacheStatus>? Statuses { get; set; }
    public List<CachePriority>? Priorities { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Tags { get; set; }
    public int? MinAccessCount { get; set; }
    public int? MaxAccessCount { get; set; }
    public int MaxResults { get; set; } = 100;
    public SortOrder SortOrder { get; set; } = SortOrder.CreatedDateDesc;
}

/// <summary>
/// Cache search result
/// </summary>
public class CacheSearchResult
{
    public string CacheKey { get; set; } = string.Empty;
    public MarketDataType DataType { get; set; }
    public int RegionId { get; set; }
    public string? RegionName { get; set; }
    public int? TypeId { get; set; }
    public string? TypeName { get; set; }
    public long? LocationId { get; set; }
    public CacheStatus Status { get; set; }
    public CachePriority Priority { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastAccessDate { get; set; }
    public int AccessCount { get; set; }
    public int DataSize { get; set; }
    public string? Tags { get; set; }
}

/// <summary>
/// Cache warmup request
/// </summary>
public class CacheWarmupRequest
{
    public List<MarketDataType> DataTypes { get; set; } = new();
    public List<int> RegionIds { get; set; } = new();
    public List<int>? TypeIds { get; set; }
    public int MaxConcurrentRequests { get; set; } = 5;
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromMinutes(2);
    public CachePriority Priority { get; set; } = CachePriority.Normal;
}

/// <summary>
/// Cache warmup result
/// </summary>
public class CacheWarmupResult
{
    public bool Success { get; set; }
    public int RequestedEntries { get; set; }
    public int CachedEntries { get; set; }
    public int FailedEntries { get; set; }
    public TimeSpan WarmupDuration { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime WarmupDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Enumerations
/// </summary>
public enum ExportFormat
{
    Json,
    Xml,
    Csv,
    Binary
}

public enum ImportStrategy
{
    Merge,
    Replace,
    SkipExisting
}

public enum SortOrder
{
    CacheKeyAsc,
    CacheKeyDesc,
    CreatedDateAsc,
    CreatedDateDesc,
    LastAccessAsc,
    LastAccessDesc,
    AccessCountAsc,
    AccessCountDesc,
    DataSizeAsc,
    DataSizeDesc
}