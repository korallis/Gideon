// ==========================================================================
// MarketDataCacheRepository.cs - Market Data Cache Repository Implementation
// ==========================================================================
// Implementation of market data cache repository with specialized caching operations,
// performance monitoring, and refresh scheduling capabilities.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Core.Domain.Interfaces;
using Gideon.WPF.Core.Infrastructure.Data;
using System.Security.Cryptography;
using System.Text;

namespace Gideon.WPF.Core.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for market data cache management
/// </summary>
public class MarketDataCacheRepository : Repository<MarketDataCacheEntry>, IMarketDataCacheRepository
{
    private readonly GideonDbContext _context;

    public MarketDataCacheRepository(GideonDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    #region Cache Entry Retrieval

    /// <summary>
    /// Get cache entry by cache key
    /// </summary>
    public async Task<MarketDataCacheEntry?> GetByCacheKeyAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        return await _context.MarketDataCache
            .Include(c => c.Region)
            .Include(c => c.ItemType)
            .FirstOrDefaultAsync(c => c.CacheKey == cacheKey, cancellationToken);
    }

    /// <summary>
    /// Get cache entries by data type and region
    /// </summary>
    public async Task<List<MarketDataCacheEntry>> GetByDataTypeAndRegionAsync(MarketDataType dataType, int regionId, CancellationToken cancellationToken = default)
    {
        return await _context.MarketDataCache
            .Include(c => c.Region)
            .Include(c => c.ItemType)
            .Where(c => c.DataType == dataType && c.RegionId == regionId)
            .OrderByDescending(c => c.LastAccessDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get expired cache entries that need refresh
    /// </summary>
    public async Task<List<MarketDataCacheEntry>> GetExpiredEntriesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.MarketDataCache
            .Where(c => c.Status != CacheStatus.Refreshing && 
                       c.Status != CacheStatus.Evicted &&
                       c.ExpiryDate <= now)
            .OrderBy(c => c.Priority)
            .ThenBy(c => c.ExpiryDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get stale cache entries based on refresh interval
    /// </summary>
    public async Task<List<MarketDataCacheEntry>> GetStaleEntriesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.MarketDataCache
            .Where(c => c.Status == CacheStatus.Fresh && 
                       c.AutoRefresh &&
                       c.CreatedDate.Add(c.RefreshInterval) <= now)
            .OrderBy(c => c.Priority)
            .ThenBy(c => c.CreatedDate.Add(c.RefreshInterval))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get cache entries by priority for eviction
    /// </summary>
    public async Task<List<MarketDataCacheEntry>> GetEntriesByPriorityAsync(CachePriority priority, CancellationToken cancellationToken = default)
    {
        return await _context.MarketDataCache
            .Where(c => c.Priority == priority)
            .OrderBy(c => c.LastAccessDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get cache entries by status
    /// </summary>
    public async Task<List<MarketDataCacheEntry>> GetEntriesByStatusAsync(CacheStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.MarketDataCache
            .Where(c => c.Status == status)
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region Cache Entry Updates

    /// <summary>
    /// Update cache entry status
    /// </summary>
    public async Task UpdateCacheStatusAsync(int cacheId, CacheStatus status, CancellationToken cancellationToken = default)
    {
        var entry = await _context.MarketDataCache.FindAsync(new object[] { cacheId }, cancellationToken);
        if (entry != null)
        {
            entry.Status = status;
            if (status == CacheStatus.Refreshing)
            {
                entry.LastRefreshAttempt = DateTime.UtcNow;
            }
            else if (status == CacheStatus.Fresh)
            {
                entry.LastSuccessfulRefresh = DateTime.UtcNow;
                entry.RefreshFailureCount = 0;
                entry.LastError = null;
                entry.ExpiryDate = DateTime.UtcNow.Add(entry.RefreshInterval);
            }
            else if (status == CacheStatus.Error)
            {
                entry.RefreshFailureCount++;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Update cache entry data
    /// </summary>
    public async Task UpdateCacheDataAsync(int cacheId, string cachedData, string? dataHash = null, CancellationToken cancellationToken = default)
    {
        var entry = await _context.MarketDataCache.FindAsync(new object[] { cacheId }, cancellationToken);
        if (entry != null)
        {
            entry.CachedData = cachedData;
            entry.DataSize = Encoding.UTF8.GetByteCount(cachedData);
            entry.DataHash = dataHash ?? ComputeDataHash(cachedData);
            entry.LastSuccessfulRefresh = DateTime.UtcNow;
            entry.Status = CacheStatus.Fresh;
            entry.ExpiryDate = DateTime.UtcNow.Add(entry.RefreshInterval);
            entry.RefreshFailureCount = 0;
            entry.LastError = null;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Increment access count and update last access date
    /// </summary>
    public async Task IncrementAccessCountAsync(int cacheId, CancellationToken cancellationToken = default)
    {
        var entry = await _context.MarketDataCache.FindAsync(new object[] { cacheId }, cancellationToken);
        if (entry != null)
        {
            entry.AccessCount++;
            entry.LastAccessDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Mark cache entry for refresh
    /// </summary>
    public async Task MarkForRefreshAsync(int cacheId, CancellationToken cancellationToken = default)
    {
        var entry = await _context.MarketDataCache.FindAsync(new object[] { cacheId }, cancellationToken);
        if (entry != null)
        {
            entry.Status = CacheStatus.Stale;
            entry.ExpiryDate = DateTime.UtcNow; // Mark as needing immediate refresh
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    #endregion

    #region Cache Maintenance

    /// <summary>
    /// Clean up expired cache entries
    /// </summary>
    public async Task<int> CleanupExpiredEntriesAsync(CancellationToken cancellationToken = default)
    {
        var expiredEntries = await GetExpiredEntriesAsync(cancellationToken);
        var evictableEntries = expiredEntries.Where(e => e.Priority != CachePriority.Critical).ToList();

        if (evictableEntries.Any())
        {
            // Log invalidation
            foreach (var entry in evictableEntries)
            {
                var invalidationLog = new CacheInvalidationLog
                {
                    CacheKey = entry.CacheKey,
                    DataType = entry.DataType,
                    Reason = InvalidationReason.Expired,
                    ReasonDetails = "Automatic cleanup of expired entries",
                    InvalidationDate = DateTime.UtcNow,
                    WasAutomatic = true,
                    AffectedEntries = 1,
                    ProcessingTime = TimeSpan.Zero
                };
                _context.CacheInvalidationLog.Add(invalidationLog);
            }

            _context.MarketDataCache.RemoveRange(evictableEntries);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return evictableEntries.Count;
    }

    /// <summary>
    /// Evict cache entries based on size limit
    /// </summary>
    public async Task<int> EvictOldestEntriesAsync(int maxSizeBytes, CancellationToken cancellationToken = default)
    {
        var totalSize = await _context.MarketDataCache.SumAsync(c => c.DataSize, cancellationToken);
        
        if (totalSize <= maxSizeBytes)
            return 0;

        var targetReduction = totalSize - maxSizeBytes;
        var evictableEntries = await _context.MarketDataCache
            .Where(c => c.Priority == CachePriority.Low || c.Priority == CachePriority.Disposable)
            .OrderBy(c => c.Priority)
            .ThenBy(c => c.LastAccessDate)
            .ToListAsync(cancellationToken);

        var evictedCount = 0;
        var currentReduction = 0;

        foreach (var entry in evictableEntries)
        {
            if (currentReduction >= targetReduction)
                break;

            currentReduction += entry.DataSize;
            evictedCount++;

            // Log eviction
            var invalidationLog = new CacheInvalidationLog
            {
                CacheKey = entry.CacheKey,
                DataType = entry.DataType,
                Reason = InvalidationReason.SizeLimit,
                ReasonDetails = $"Evicted to maintain size limit of {maxSizeBytes} bytes",
                InvalidationDate = DateTime.UtcNow,
                WasAutomatic = true,
                AffectedEntries = 1,
                ProcessingTime = TimeSpan.Zero
            };
            _context.CacheInvalidationLog.Add(invalidationLog);

            _context.MarketDataCache.Remove(entry);
        }

        if (evictedCount > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return evictedCount;
    }

    #endregion

    #region Cache Analytics

    /// <summary>
    /// Get cache statistics
    /// </summary>
    public async Task<CacheStatisticsData> GetCacheStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var allEntries = await _context.MarketDataCache.ToListAsync(cancellationToken);

        var totalEntries = allEntries.Count;
        var freshEntries = allEntries.Count(e => e.Status == CacheStatus.Fresh);
        var staleEntries = allEntries.Count(e => e.Status == CacheStatus.Stale);
        var expiredEntries = allEntries.Count(e => e.Status == CacheStatus.Expired);
        var errorEntries = allEntries.Count(e => e.Status == CacheStatus.Error);
        var totalSize = allEntries.Sum(e => e.DataSize);

        var totalAccesses = allEntries.Sum(e => e.AccessCount);
        var hitRate = totalAccesses > 0 ? (double)freshEntries / totalAccesses * 100 : 0;
        var missRate = 100 - hitRate;

        var averageAge = totalEntries > 0 
            ? TimeSpan.FromTicks((long)allEntries.Average(e => (DateTime.UtcNow - e.CreatedDate).Ticks))
            : TimeSpan.Zero;

        return new CacheStatisticsData
        {
            TotalEntries = totalEntries,
            FreshEntries = freshEntries,
            StaleEntries = staleEntries,
            ExpiredEntries = expiredEntries,
            ErrorEntries = errorEntries,
            TotalCacheSize = totalSize,
            HitRatePercentage = hitRate,
            MissRatePercentage = missRate,
            AverageAge = averageAge,
            EntriesByType = allEntries.GroupBy(e => e.DataType).ToDictionary(g => g.Key, g => g.Count()),
            EntriesByStatus = allEntries.GroupBy(e => e.Status).ToDictionary(g => g.Key, g => g.Count()),
            EntriesByPriority = allEntries.GroupBy(e => e.Priority).ToDictionary(g => g.Key, g => g.Count())
        };
    }

    /// <summary>
    /// Search cache entries by tags
    /// </summary>
    public async Task<List<MarketDataCacheEntry>> SearchByTagsAsync(string tags, CancellationToken cancellationToken = default)
    {
        var searchTags = tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim().ToLowerInvariant())
            .ToList();

        return await _context.MarketDataCache
            .Where(c => searchTags.Any(tag => c.Tags != null && c.Tags.ToLower().Contains(tag)))
            .OrderByDescending(c => c.LastAccessDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get cache entries due for refresh
    /// </summary>
    public async Task<List<MarketDataCacheEntry>> GetEntriesDueForRefreshAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.MarketDataCache
            .Where(c => c.AutoRefresh && 
                       c.Status != CacheStatus.Refreshing && 
                       c.Status != CacheStatus.Evicted &&
                       (c.ExpiryDate <= now || c.CreatedDate.Add(c.RefreshInterval) <= now))
            .OrderBy(c => c.Priority)
            .ThenBy(c => c.ExpiryDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Validate cache integrity
    /// </summary>
    public async Task<CacheIntegrityReport> ValidateCacheIntegrityAsync(CancellationToken cancellationToken = default)
    {
        var report = new CacheIntegrityReport { IsHealthy = true };
        var allEntries = await _context.MarketDataCache.Include(c => c.Region).Include(c => c.ItemType).ToListAsync(cancellationToken);

        report.TotalEntries = allEntries.Count;

        foreach (var entry in allEntries)
        {
            // Check for corrupted data
            if (string.IsNullOrEmpty(entry.CachedData))
            {
                report.CorruptedEntries++;
                report.Issues.Add($"Cache entry {entry.CacheKey} has empty data");
                report.IsHealthy = false;
            }

            // Check hash integrity
            if (!string.IsNullOrEmpty(entry.DataHash))
            {
                var computedHash = ComputeDataHash(entry.CachedData);
                if (entry.DataHash != computedHash)
                {
                    report.InvalidHashEntries++;
                    report.Issues.Add($"Cache entry {entry.CacheKey} has invalid hash");
                    report.IsHealthy = false;
                }
            }

            // Check missing references
            if (entry.RegionId > 0 && entry.Region == null)
            {
                report.MissingReferences++;
                report.Issues.Add($"Cache entry {entry.CacheKey} references missing region {entry.RegionId}");
                report.IsHealthy = false;
            }

            if (entry.TypeId.HasValue && entry.TypeId > 0 && entry.ItemType == null)
            {
                report.MissingReferences++;
                report.Issues.Add($"Cache entry {entry.CacheKey} references missing type {entry.TypeId}");
                report.IsHealthy = false;
            }
        }

        // Generate recommendations
        if (report.CorruptedEntries > 0)
        {
            report.Recommendations.Add("Remove corrupted cache entries and refresh data");
        }
        if (report.InvalidHashEntries > 0)
        {
            report.Recommendations.Add("Recompute hashes for entries with invalid checksums");
        }
        if (report.MissingReferences > 0)
        {
            report.Recommendations.Add("Update static data or remove entries with missing references");
        }

        var staleTreshold = DateTime.UtcNow.AddHours(-1);
        var staleCount = allEntries.Count(e => e.LastAccessDate < staleTreshold);
        if (staleCount > allEntries.Count * 0.5)
        {
            report.Recommendations.Add("Consider increasing cache refresh frequency or reducing cache size");
        }

        return report;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Compute hash for cache data integrity
    /// </summary>
    private static string ComputeDataHash(string data)
    {
        if (string.IsNullOrEmpty(data))
            return string.Empty;

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hashBytes);
    }

    #endregion
}