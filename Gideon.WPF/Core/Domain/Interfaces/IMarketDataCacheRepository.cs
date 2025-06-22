// ==========================================================================
// IMarketDataCacheRepository.cs - Market Data Cache Repository Interface
// ==========================================================================
// Repository interface for market data caching operations with specialized methods
// for cache management, performance monitoring, and refresh scheduling.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Gideon.WPF.Core.Domain.Entities;

namespace Gideon.WPF.Core.Domain.Interfaces;

/// <summary>
/// Repository interface for market data cache management
/// </summary>
public interface IMarketDataCacheRepository : IRepository<MarketDataCacheEntry>
{
    /// <summary>
    /// Get cache entry by cache key
    /// </summary>
    Task<MarketDataCacheEntry?> GetByCacheKeyAsync(string cacheKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get cache entries by data type and region
    /// </summary>
    Task<List<MarketDataCacheEntry>> GetByDataTypeAndRegionAsync(MarketDataType dataType, int regionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get expired cache entries that need refresh
    /// </summary>
    Task<List<MarketDataCacheEntry>> GetExpiredEntriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get stale cache entries based on refresh interval
    /// </summary>
    Task<List<MarketDataCacheEntry>> GetStaleEntriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get cache entries by priority for eviction
    /// </summary>
    Task<List<MarketDataCacheEntry>> GetEntriesByPriorityAsync(CachePriority priority, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get cache entries by status
    /// </summary>
    Task<List<MarketDataCacheEntry>> GetEntriesByStatusAsync(CacheStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update cache entry status
    /// </summary>
    Task UpdateCacheStatusAsync(int cacheId, CacheStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update cache entry data
    /// </summary>
    Task UpdateCacheDataAsync(int cacheId, string cachedData, string? dataHash = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Increment access count and update last access date
    /// </summary>
    Task IncrementAccessCountAsync(int cacheId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark cache entry for refresh
    /// </summary>
    Task MarkForRefreshAsync(int cacheId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clean up expired cache entries
    /// </summary>
    Task<int> CleanupExpiredEntriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get cache statistics
    /// </summary>
    Task<CacheStatisticsData> GetCacheStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Evict cache entries based on size limit
    /// </summary>
    Task<int> EvictOldestEntriesAsync(int maxSizeBytes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search cache entries by tags
    /// </summary>
    Task<List<MarketDataCacheEntry>> SearchByTagsAsync(string tags, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get cache entries due for refresh
    /// </summary>
    Task<List<MarketDataCacheEntry>> GetEntriesDueForRefreshAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate cache integrity
    /// </summary>
    Task<CacheIntegrityReport> ValidateCacheIntegrityAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Cache statistics data
/// </summary>
public class CacheStatisticsData
{
    public int TotalEntries { get; set; }
    public int FreshEntries { get; set; }
    public int StaleEntries { get; set; }
    public int ExpiredEntries { get; set; }
    public int ErrorEntries { get; set; }
    public long TotalCacheSize { get; set; }
    public double HitRatePercentage { get; set; }
    public double MissRatePercentage { get; set; }
    public TimeSpan AverageAge { get; set; }
    public Dictionary<MarketDataType, int> EntriesByType { get; set; } = new();
    public Dictionary<CacheStatus, int> EntriesByStatus { get; set; } = new();
    public Dictionary<CachePriority, int> EntriesByPriority { get; set; } = new();
}

/// <summary>
/// Cache integrity report
/// </summary>
public class CacheIntegrityReport
{
    public bool IsHealthy { get; set; }
    public int TotalEntries { get; set; }
    public int CorruptedEntries { get; set; }
    public int MissingReferences { get; set; }
    public int InvalidHashEntries { get; set; }
    public List<string> Issues { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public DateTime ReportDate { get; set; } = DateTime.UtcNow;
}