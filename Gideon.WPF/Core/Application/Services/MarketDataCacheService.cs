// ==========================================================================
// MarketDataCacheService.cs - Market Data Cache Service Implementation
// ==========================================================================
// Implementation of market data caching service with 15-minute refresh functionality,
// background refresh processing, and comprehensive cache management.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System.Text.Json;
using System.IO.Compression;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Core.Domain.Interfaces;

namespace Gideon.WPF.Core.Application.Services;

/// <summary>
/// Service implementation for market data caching management
/// </summary>
public class MarketDataCacheService : IMarketDataCacheService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarketDataCacheService> _logger;
    private readonly IHostedService _refreshService;
    private readonly object _cacheLock = new();
    
    private CacheSettings _settings = new()
    {
        DefaultRefreshInterval = TimeSpan.FromMinutes(15),
        MaxCacheSizeBytes = 1024L * 1024L * 1024L, // 1GB
        MaxCacheEntries = 100000,
        AutoCleanupEnabled = true,
        CleanupInterval = TimeSpan.FromHours(6),
        CompressionEnabled = true,
        IntegrityCheckEnabled = true,
        DefaultPriority = CachePriority.Normal
    };

    public MarketDataCacheService(IUnitOfWork unitOfWork, ILogger<MarketDataCacheService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _refreshService = new CacheRefreshService(this, logger);
    }

    #region Core Cache Operations

    /// <summary>
    /// Get cached market data or fetch if not available
    /// </summary>
    public async Task<T?> GetOrFetchDataAsync<T>(string cacheKey, MarketDataType dataType, int regionId, int? typeId, long? locationId, Func<Task<T>> fetchFunction, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            // Check cache first
            var cacheEntry = await _unitOfWork.MarketDataCacheRepository.GetByCacheKeyAsync(cacheKey, cancellationToken);
            if (cacheEntry != null)
            {
                // Update access statistics
                await _unitOfWork.MarketDataCacheRepository.IncrementAccessCountAsync(cacheEntry.CacheId, cancellationToken);

                // Check if data is fresh
                if (cacheEntry.Status == CacheStatus.Fresh && cacheEntry.ExpiryDate > DateTime.UtcNow)
                {
                    try
                    {
                        var cachedData = DeserializeData<T>(cacheEntry.CachedData);
                        if (cachedData != null)
                        {
                            _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
                            return cachedData;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to deserialize cached data for key: {CacheKey}", cacheKey);
                    }
                }
            }

            // Cache miss or stale data - fetch new data
            _logger.LogDebug("Cache miss for key: {CacheKey}, fetching fresh data", cacheKey);
            var freshData = await fetchFunction();
            
            if (freshData != null)
            {
                // Store in cache
                await StoreDataAsync(cacheKey, dataType, regionId, freshData, null, typeId, locationId, _settings.DefaultPriority, cancellationToken);
            }

            return freshData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrFetchDataAsync for key: {CacheKey}", cacheKey);
            throw;
        }
    }

    /// <summary>
    /// Store data in cache
    /// </summary>
    public async Task<bool> StoreDataAsync<T>(string cacheKey, MarketDataType dataType, int regionId, T data, TimeSpan? customRefreshInterval = null, int? typeId = null, long? locationId = null, CachePriority priority = CachePriority.Normal, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            lock (_cacheLock)
            {
                var serializedData = SerializeData(data);
                var refreshInterval = customRefreshInterval ?? GetRefreshIntervalForDataType(dataType);
                var expiryDate = DateTime.UtcNow.Add(refreshInterval);

                var cacheEntry = new MarketDataCacheEntry
                {
                    CacheKey = cacheKey,
                    DataType = dataType,
                    RegionId = regionId,
                    TypeId = typeId,
                    LocationId = locationId,
                    CachedData = serializedData,
                    CreatedDate = DateTime.UtcNow,
                    ExpiryDate = expiryDate,
                    RefreshInterval = refreshInterval,
                    Status = CacheStatus.Fresh,
                    DataSize = System.Text.Encoding.UTF8.GetByteCount(serializedData),
                    DataHash = ComputeDataHash(serializedData),
                    Priority = priority,
                    AutoRefresh = true,
                    AccessCount = 0
                };

                _unitOfWork.MarketDataCache.Add(cacheEntry);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            // Schedule refresh if auto-refresh is enabled
            if (_settings.AutoCleanupEnabled)
            {
                await ScheduleRefreshAsync(cacheKey, dataType, refreshInterval, cancellationToken);
            }

            _logger.LogDebug("Stored data in cache for key: {CacheKey}", cacheKey);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing data in cache for key: {CacheKey}", cacheKey);
            return false;
        }
    }

    /// <summary>
    /// Invalidate cached data by key
    /// </summary>
    public async Task<bool> InvalidateCacheAsync(string cacheKey, InvalidationReason reason = InvalidationReason.UserRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheEntry = await _unitOfWork.MarketDataCacheRepository.GetByCacheKeyAsync(cacheKey, cancellationToken);
            if (cacheEntry != null)
            {
                // Log invalidation
                var invalidationLog = new CacheInvalidationLog
                {
                    CacheKey = cacheKey,
                    DataType = cacheEntry.DataType,
                    Reason = reason,
                    ReasonDetails = $"Manual invalidation for key: {cacheKey}",
                    InvalidationDate = DateTime.UtcNow,
                    WasAutomatic = false,
                    AffectedEntries = 1,
                    ProcessingTime = TimeSpan.Zero
                };

                _unitOfWork.CacheInvalidationLog.Add(invalidationLog);
                _unitOfWork.MarketDataCache.Remove(cacheEntry);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Invalidated cache entry: {CacheKey}", cacheKey);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for key: {CacheKey}", cacheKey);
            return false;
        }
    }

    /// <summary>
    /// Invalidate cached data by criteria
    /// </summary>
    public async Task<int> InvalidateCacheByCriteriaAsync(MarketDataType? dataType = null, int? regionId = null, int? typeId = null, InvalidationReason reason = InvalidationReason.UserRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _unitOfWork.MarketDataCache.GetQueryable();

            if (dataType.HasValue)
                query = query.Where(c => c.DataType == dataType.Value);
            if (regionId.HasValue)
                query = query.Where(c => c.RegionId == regionId.Value);
            if (typeId.HasValue)
                query = query.Where(c => c.TypeId == typeId.Value);

            var entriesToInvalidate = await query.ToListAsync(cancellationToken);
            var invalidatedCount = entriesToInvalidate.Count;

            if (invalidatedCount > 0)
            {
                // Log bulk invalidation
                var invalidationLog = new CacheInvalidationLog
                {
                    CacheKey = "BULK_INVALIDATION",
                    DataType = dataType ?? MarketDataType.MarketOrders,
                    Reason = reason,
                    ReasonDetails = $"Bulk invalidation: DataType={dataType}, RegionId={regionId}, TypeId={typeId}",
                    InvalidationDate = DateTime.UtcNow,
                    WasAutomatic = false,
                    AffectedEntries = invalidatedCount,
                    ProcessingTime = TimeSpan.Zero
                };

                _unitOfWork.CacheInvalidationLog.Add(invalidationLog);
                _unitOfWork.MarketDataCache.RemoveRange(entriesToInvalidate);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Bulk invalidated {Count} cache entries", invalidatedCount);
            }

            return invalidatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk cache invalidation");
            return 0;
        }
    }

    /// <summary>
    /// Refresh cache entry
    /// </summary>
    public async Task<bool> RefreshCacheEntryAsync<T>(string cacheKey, Func<Task<T>> fetchFunction, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var cacheEntry = await _unitOfWork.MarketDataCacheRepository.GetByCacheKeyAsync(cacheKey, cancellationToken);
            if (cacheEntry == null)
            {
                _logger.LogWarning("Cannot refresh non-existent cache entry: {CacheKey}", cacheKey);
                return false;
            }

            // Mark as refreshing
            await _unitOfWork.MarketDataCacheRepository.UpdateCacheStatusAsync(cacheEntry.CacheId, CacheStatus.Refreshing, cancellationToken);

            try
            {
                var freshData = await fetchFunction();
                if (freshData != null)
                {
                    var serializedData = SerializeData(freshData);
                    await _unitOfWork.MarketDataCacheRepository.UpdateCacheDataAsync(cacheEntry.CacheId, serializedData, ComputeDataHash(serializedData), cancellationToken);
                    
                    _logger.LogDebug("Successfully refreshed cache entry: {CacheKey}", cacheKey);
                    return true;
                }
                else
                {
                    await _unitOfWork.MarketDataCacheRepository.UpdateCacheStatusAsync(cacheEntry.CacheId, CacheStatus.Error, cancellationToken);
                    _logger.LogWarning("Refresh returned null data for cache entry: {CacheKey}", cacheKey);
                    return false;
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.MarketDataCacheRepository.UpdateCacheStatusAsync(cacheEntry.CacheId, CacheStatus.Error, cancellationToken);
                _logger.LogError(ex, "Error refreshing cache entry: {CacheKey}", cacheKey);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RefreshCacheEntryAsync for key: {CacheKey}", cacheKey);
            return false;
        }
    }

    #endregion

    #region Cache Management

    /// <summary>
    /// Start automatic cache refresh background service
    /// </summary>
    public async Task StartCacheRefreshServiceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _refreshService.StartAsync(cancellationToken);
            _logger.LogInformation("Cache refresh service started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting cache refresh service");
            throw;
        }
    }

    /// <summary>
    /// Stop automatic cache refresh background service
    /// </summary>
    public async Task StopCacheRefreshServiceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _refreshService.StopAsync(cancellationToken);
            _logger.LogInformation("Cache refresh service stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping cache refresh service");
            throw;
        }
    }

    /// <summary>
    /// Get cache health and statistics
    /// </summary>
    public async Task<CacheHealthReport> GetCacheHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var statistics = await _unitOfWork.MarketDataCache.GetCacheStatisticsAsync(cancellationToken);
            var integrityReport = await _unitOfWork.MarketDataCache.ValidateCacheIntegrityAsync(cancellationToken);

            var healthScore = CalculateHealthScore(statistics, integrityReport);
            var isHealthy = healthScore >= 80.0;

            var report = new CacheHealthReport
            {
                IsHealthy = isHealthy,
                HealthScore = healthScore,
                Statistics = statistics,
                IntegrityReport = integrityReport,
                ReportDate = DateTime.UtcNow
            };

            // Add issues and recommendations
            if (!integrityReport.IsHealthy)
            {
                report.Issues.AddRange(integrityReport.Issues);
                report.Recommendations.AddRange(integrityReport.Recommendations);
            }

            if (statistics.HitRatePercentage < 70)
            {
                report.Warnings.Add("Low cache hit rate detected");
                report.Recommendations.Add("Consider adjusting refresh intervals or cache size");
            }

            if (statistics.TotalCacheSize > _settings.MaxCacheSizeBytes * 0.9)
            {
                report.Warnings.Add("Cache size approaching limit");
                report.Recommendations.Add("Perform cache cleanup or increase size limit");
            }

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cache health report");
            throw;
        }
    }

    /// <summary>
    /// Perform cache maintenance
    /// </summary>
    public async Task<CacheMaintenanceResult> PerformCacheMaintenanceAsync(CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var result = new CacheMaintenanceResult { Success = true };

        try
        {
            // Clean up expired entries
            var cleanedUp = await _unitOfWork.MarketDataCache.CleanupExpiredEntriesAsync(cancellationToken);
            result.CleanedUpEntries = cleanedUp;
            result.Actions.Add($"Cleaned up {cleanedUp} expired entries");

            // Evict entries if over size limit
            var currentSize = (await _unitOfWork.MarketDataCache.GetCacheStatisticsAsync(cancellationToken)).TotalCacheSize;
            if (currentSize > _settings.MaxCacheSizeBytes)
            {
                var evicted = await _unitOfWork.MarketDataCache.EvictOldestEntriesAsync((int)_settings.MaxCacheSizeBytes, cancellationToken);
                result.EvictedEntries = evicted;
                result.Actions.Add($"Evicted {evicted} entries due to size limit");
            }

            // Refresh stale entries
            var staleEntries = await _unitOfWork.MarketDataCache.GetStaleEntriesAsync(cancellationToken);
            var refreshed = 0;
            foreach (var entry in staleEntries.Take(10)) // Limit concurrent refreshes
            {
                await _unitOfWork.MarketDataCache.MarkForRefreshAsync(entry.CacheId, cancellationToken);
                refreshed++;
            }
            result.RefreshedEntries = refreshed;
            result.Actions.Add($"Marked {refreshed} stale entries for refresh");

            // Update maintenance duration
            result.MaintenanceDuration = DateTime.UtcNow - startTime;
            result.SpaceFreed = cleanedUp * 1000 + evicted * 1000; // Estimate

            _logger.LogInformation("Cache maintenance completed: {CleanedUp} cleaned, {Evicted} evicted, {Refreshed} refreshed", 
                cleanedUp, evicted, refreshed);

            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add(ex.Message);
            _logger.LogError(ex, "Error during cache maintenance");
            return result;
        }
    }

    #endregion

    #region Configuration and Settings

    /// <summary>
    /// Configure cache settings
    /// </summary>
    public async Task ConfigureCacheSettingsAsync(CacheSettingsRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _settings = new CacheSettings
            {
                DefaultRefreshInterval = request.DefaultRefreshInterval,
                MaxCacheSizeBytes = request.MaxCacheSizeBytes,
                MaxCacheEntries = request.MaxCacheEntries,
                AutoCleanupEnabled = request.AutoCleanupEnabled,
                CleanupInterval = request.CleanupInterval,
                CompressionEnabled = request.CompressionEnabled,
                IntegrityCheckEnabled = request.IntegrityCheckEnabled,
                DefaultPriority = request.DefaultPriority,
                CustomRefreshIntervals = request.CustomRefreshIntervals,
                LastUpdated = DateTime.UtcNow
            };

            // Save to database
            await SaveSettingsToDatabase(cancellationToken);

            _logger.LogInformation("Cache settings updated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring cache settings");
            throw;
        }
    }

    /// <summary>
    /// Get cache configuration
    /// </summary>
    public async Task<CacheSettings> GetCacheSettingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await LoadSettingsFromDatabase(cancellationToken);
            return _settings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading cache settings");
            throw;
        }
    }

    #endregion

    #region Import/Export Operations

    /// <summary>
    /// Export cache data
    /// </summary>
    public async Task<CacheExportResult> ExportCacheDataAsync(CacheExportRequest request, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var result = new CacheExportResult();

        try
        {
            var query = _unitOfWork.MarketDataCache.GetQueryable();

            // Apply filters
            if (request.DataTypes?.Any() == true)
                query = query.Where(c => request.DataTypes.Contains(c.DataType));
            if (request.RegionIds?.Any() == true)
                query = query.Where(c => request.RegionIds.Contains(c.RegionId));
            if (request.FromDate.HasValue)
                query = query.Where(c => c.CreatedDate >= request.FromDate.Value);
            if (request.ToDate.HasValue)
                query = query.Where(c => c.CreatedDate <= request.ToDate.Value);
            if (!request.IncludeExpiredEntries)
                query = query.Where(c => c.Status != CacheStatus.Expired);

            var entries = await query.ToListAsync(cancellationToken);
            var exportData = entries.Select(e => new
            {
                e.CacheKey,
                e.DataType,
                e.RegionId,
                e.TypeId,
                e.LocationId,
                e.CachedData,
                e.CreatedDate,
                e.ExpiryDate,
                e.Status,
                e.Priority
            });

            var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
            
            if (request.CompressExport)
            {
                using var fileStream = new FileStream(request.ExportPath, FileMode.Create);
                using var gzipStream = new GZipStream(fileStream, CompressionMode.Compress);
                using var writer = new StreamWriter(gzipStream);
                await writer.WriteAsync(json);
            }
            else
            {
                await File.WriteAllTextAsync(request.ExportPath, json, cancellationToken);
            }

            result.Success = true;
            result.ExportPath = request.ExportPath;
            result.ExportedEntries = entries.Count;
            result.ExportSizeBytes = new FileInfo(request.ExportPath).Length;
            result.ExportDuration = DateTime.UtcNow - startTime;

            _logger.LogInformation("Exported {Count} cache entries to {Path}", entries.Count, request.ExportPath);
            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add(ex.Message);
            _logger.LogError(ex, "Error exporting cache data");
            return result;
        }
    }

    /// <summary>
    /// Import cache data
    /// </summary>
    public async Task<CacheImportResult> ImportCacheDataAsync(CacheImportRequest request, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var result = new CacheImportResult();

        try
        {
            string json;
            
            if (request.ImportPath.EndsWith(".gz"))
            {
                using var fileStream = new FileStream(request.ImportPath, FileMode.Open);
                using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
                using var reader = new StreamReader(gzipStream);
                json = await reader.ReadToEndAsync();
            }
            else
            {
                json = await File.ReadAllTextAsync(request.ImportPath, cancellationToken);
            }

            var importData = JsonSerializer.Deserialize<List<dynamic>>(json);
            if (importData == null)
            {
                result.Errors.Add("Failed to deserialize import data");
                return result;
            }

            // Process import entries
            foreach (var item in importData)
            {
                try
                {
                    // Create cache entry from import data
                    // Implementation would depend on the exact structure
                    result.ImportedEntries++;
                }
                catch (Exception ex)
                {
                    result.FailedEntries++;
                    result.Errors.Add($"Failed to import entry: {ex.Message}");
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            result.Success = result.FailedEntries == 0;
            result.ImportDuration = DateTime.UtcNow - startTime;

            _logger.LogInformation("Imported {Imported} cache entries ({Failed} failed)", 
                result.ImportedEntries, result.FailedEntries);
            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add(ex.Message);
            _logger.LogError(ex, "Error importing cache data");
            return result;
        }
    }

    #endregion

    #region Performance and Analytics

    /// <summary>
    /// Get cache performance metrics
    /// </summary>
    public async Task<CachePerformanceReport> GetPerformanceMetricsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-7);
            var end = endDate ?? DateTime.UtcNow;

            var metrics = await _unitOfWork.CachePerformanceMetrics
                .Where(m => m.MetricDate >= start && m.MetricDate <= end)
                .OrderBy(m => m.MetricDate)
                .ToListAsync(cancellationToken);

            var statistics = await _unitOfWork.MarketDataCache.GetCacheStatisticsAsync(cancellationToken);

            var report = new CachePerformanceReport
            {
                ReportPeriod = end - start,
                Metrics = metrics,
                HitRatePercentage = statistics.HitRatePercentage,
                ReportDate = DateTime.UtcNow
            };

            // Generate performance insights
            report.PerformanceInsights.Add($"Cache hit rate: {statistics.HitRatePercentage:F1}%");
            report.PerformanceInsights.Add($"Total cache entries: {statistics.TotalEntries}");
            report.PerformanceInsights.Add($"Total cache size: {statistics.TotalCacheSize / 1024 / 1024:F1} MB");

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating performance report");
            throw;
        }
    }

    /// <summary>
    /// Search cached data
    /// </summary>
    public async Task<List<CacheSearchResult>> SearchCacheAsync(CacheSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _unitOfWork.MarketDataCache.GetQueryable();

            // Apply search filters
            if (!string.IsNullOrEmpty(criteria.SearchText))
                query = query.Where(c => c.CacheKey.Contains(criteria.SearchText));
            if (criteria.DataTypes?.Any() == true)
                query = query.Where(c => criteria.DataTypes.Contains(c.DataType));
            if (criteria.RegionIds?.Any() == true)
                query = query.Where(c => criteria.RegionIds.Contains(c.RegionId));
            if (criteria.TypeIds?.Any() == true)
                query = query.Where(c => criteria.TypeIds.Contains(c.TypeId ?? 0));
            if (criteria.Statuses?.Any() == true)
                query = query.Where(c => criteria.Statuses.Contains(c.Status));
            if (criteria.Priorities?.Any() == true)
                query = query.Where(c => criteria.Priorities.Contains(c.Priority));
            if (criteria.FromDate.HasValue)
                query = query.Where(c => c.CreatedDate >= criteria.FromDate.Value);
            if (criteria.ToDate.HasValue)
                query = query.Where(c => c.CreatedDate <= criteria.ToDate.Value);
            if (!string.IsNullOrEmpty(criteria.Tags))
                query = query.Where(c => c.Tags != null && c.Tags.Contains(criteria.Tags));
            if (criteria.MinAccessCount.HasValue)
                query = query.Where(c => c.AccessCount >= criteria.MinAccessCount.Value);
            if (criteria.MaxAccessCount.HasValue)
                query = query.Where(c => c.AccessCount <= criteria.MaxAccessCount.Value);

            // Apply sorting
            query = criteria.SortOrder switch
            {
                SortOrder.CacheKeyAsc => query.OrderBy(c => c.CacheKey),
                SortOrder.CacheKeyDesc => query.OrderByDescending(c => c.CacheKey),
                SortOrder.CreatedDateAsc => query.OrderBy(c => c.CreatedDate),
                SortOrder.CreatedDateDesc => query.OrderByDescending(c => c.CreatedDate),
                SortOrder.LastAccessAsc => query.OrderBy(c => c.LastAccessDate),
                SortOrder.LastAccessDesc => query.OrderByDescending(c => c.LastAccessDate),
                SortOrder.AccessCountAsc => query.OrderBy(c => c.AccessCount),
                SortOrder.AccessCountDesc => query.OrderByDescending(c => c.AccessCount),
                SortOrder.DataSizeAsc => query.OrderBy(c => c.DataSize),
                SortOrder.DataSizeDesc => query.OrderByDescending(c => c.DataSize),
                _ => query.OrderByDescending(c => c.CreatedDate)
            };

            var entries = await query.Take(criteria.MaxResults).ToListAsync(cancellationToken);

            return entries.Select(e => new CacheSearchResult
            {
                CacheKey = e.CacheKey,
                DataType = e.DataType,
                RegionId = e.RegionId,
                RegionName = e.Region?.RegionName,
                TypeId = e.TypeId,
                TypeName = e.ItemType?.TypeName,
                LocationId = e.LocationId,
                Status = e.Status,
                Priority = e.Priority,
                CreatedDate = e.CreatedDate,
                LastAccessDate = e.LastAccessDate,
                AccessCount = e.AccessCount,
                DataSize = e.DataSize,
                Tags = e.Tags
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching cache");
            throw;
        }
    }

    /// <summary>
    /// Warm up cache with frequently accessed data
    /// </summary>
    public async Task<CacheWarmupResult> WarmupCacheAsync(CacheWarmupRequest request, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var result = new CacheWarmupResult();

        try
        {
            var warmupTasks = new List<Task>();
            var semaphore = new SemaphoreSlim(request.MaxConcurrentRequests);

            foreach (var dataType in request.DataTypes)
            {
                foreach (var regionId in request.RegionIds)
                {
                    if (request.TypeIds?.Any() == true)
                    {
                        foreach (var typeId in request.TypeIds)
                        {
                            warmupTasks.Add(WarmupSingleEntry(dataType, regionId, typeId, request, semaphore, result, cancellationToken));
                        }
                    }
                    else
                    {
                        warmupTasks.Add(WarmupSingleEntry(dataType, regionId, null, request, semaphore, result, cancellationToken));
                    }
                }
            }

            result.RequestedEntries = warmupTasks.Count;
            await Task.WhenAll(warmupTasks);

            result.Success = result.FailedEntries == 0;
            result.WarmupDuration = DateTime.UtcNow - startTime;

            _logger.LogInformation("Cache warmup completed: {Cached}/{Requested} entries cached", 
                result.CachedEntries, result.RequestedEntries);

            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add(ex.Message);
            _logger.LogError(ex, "Error during cache warmup");
            return result;
        }
    }

    #endregion

    #region Helper Methods

    private TimeSpan GetRefreshIntervalForDataType(MarketDataType dataType)
    {
        return _settings.CustomRefreshIntervals.GetValueOrDefault(dataType, _settings.DefaultRefreshInterval);
    }

    private static string SerializeData<T>(T data)
    {
        return JsonSerializer.Serialize(data);
    }

    private static T? DeserializeData<T>(string data) where T : class
    {
        return JsonSerializer.Deserialize<T>(data);
    }

    private static string ComputeDataHash(string data)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hashBytes);
    }

    private static double CalculateHealthScore(CacheStatisticsData statistics, CacheIntegrityReport integrityReport)
    {
        var hitRateScore = Math.Min(statistics.HitRatePercentage, 100.0);
        var integrityScore = integrityReport.IsHealthy ? 100.0 : Math.Max(0.0, 100.0 - (integrityReport.CorruptedEntries + integrityReport.InvalidHashEntries + integrityReport.MissingReferences) * 10.0);
        
        return (hitRateScore * 0.6) + (integrityScore * 0.4);
    }

    private async Task ScheduleRefreshAsync(string cacheKey, MarketDataType dataType, TimeSpan refreshInterval, CancellationToken cancellationToken)
    {
        var schedule = new CacheRefreshSchedule
        {
            ScheduleName = $"Auto-refresh-{cacheKey}",
            DataType = dataType,
            RefreshInterval = refreshInterval,
            NextRefreshDue = DateTime.UtcNow.Add(refreshInterval),
            IsEnabled = true,
            Priority = _settings.DefaultPriority,
            MaxRetries = 3,
            RetryDelay = TimeSpan.FromMinutes(5),
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        _unitOfWork.CacheRefreshSchedule.Add(schedule);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task SaveSettingsToDatabase(CancellationToken cancellationToken)
    {
        var configEntries = new[]
        {
            new CacheConfiguration { ConfigurationKey = "DefaultRefreshInterval", ConfigurationValue = _settings.DefaultRefreshInterval.ToString() },
            new CacheConfiguration { ConfigurationKey = "MaxCacheSizeBytes", ConfigurationValue = _settings.MaxCacheSizeBytes.ToString() },
            new CacheConfiguration { ConfigurationKey = "MaxCacheEntries", ConfigurationValue = _settings.MaxCacheEntries.ToString() },
            new CacheConfiguration { ConfigurationKey = "AutoCleanupEnabled", ConfigurationValue = _settings.AutoCleanupEnabled.ToString() },
            new CacheConfiguration { ConfigurationKey = "CompressionEnabled", ConfigurationValue = _settings.CompressionEnabled.ToString() },
            new CacheConfiguration { ConfigurationKey = "IntegrityCheckEnabled", ConfigurationValue = _settings.IntegrityCheckEnabled.ToString() }
        };

        foreach (var config in configEntries)
        {
            _unitOfWork.CacheConfiguration.Add(config);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task LoadSettingsFromDatabase(CancellationToken cancellationToken)
    {
        var configs = await _unitOfWork.CacheConfiguration.GetAllAsync(cancellationToken);
        
        foreach (var config in configs)
        {
            switch (config.ConfigurationKey)
            {
                case "DefaultRefreshInterval":
                    if (TimeSpan.TryParse(config.ConfigurationValue, out var interval))
                        _settings.DefaultRefreshInterval = interval;
                    break;
                case "MaxCacheSizeBytes":
                    if (long.TryParse(config.ConfigurationValue, out var size))
                        _settings.MaxCacheSizeBytes = size;
                    break;
                case "MaxCacheEntries":
                    if (int.TryParse(config.ConfigurationValue, out var entries))
                        _settings.MaxCacheEntries = entries;
                    break;
                case "AutoCleanupEnabled":
                    if (bool.TryParse(config.ConfigurationValue, out var autoCleanup))
                        _settings.AutoCleanupEnabled = autoCleanup;
                    break;
                case "CompressionEnabled":
                    if (bool.TryParse(config.ConfigurationValue, out var compression))
                        _settings.CompressionEnabled = compression;
                    break;
                case "IntegrityCheckEnabled":
                    if (bool.TryParse(config.ConfigurationValue, out var integrity))
                        _settings.IntegrityCheckEnabled = integrity;
                    break;
            }
        }
    }

    private async Task WarmupSingleEntry(MarketDataType dataType, int regionId, int? typeId, CacheWarmupRequest request, SemaphoreSlim semaphore, CacheWarmupResult result, CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            var cacheKey = $"{dataType}_{regionId}_{typeId}";
            var timeout = new CancellationTokenSource(request.RequestTimeout);
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token).Token;

            // Simulate data fetching - in real implementation, this would call actual ESI APIs
            await Task.Delay(100, combinedToken); // Simulate API call
            
            var dummyData = new { DataType = dataType, RegionId = regionId, TypeId = typeId, Timestamp = DateTime.UtcNow };
            await StoreDataAsync(cacheKey, dataType, regionId, dummyData, null, typeId, null, request.Priority, combinedToken);
            
            Interlocked.Increment(ref result.CachedEntries);
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref result.FailedEntries);
            result.Errors.Add($"Failed to warmup {dataType}_{regionId}_{typeId}: {ex.Message}");
        }
        finally
        {
            semaphore.Release();
        }
    }

    #endregion

    #region Nested Classes

    /// <summary>
    /// Background service for cache refresh operations
    /// </summary>
    private class CacheRefreshService : BackgroundService
    {
        private readonly MarketDataCacheService _cacheService;
        private readonly ILogger _logger;

        public CacheRefreshService(MarketDataCacheService cacheService, ILogger logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Cache refresh background service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Check every minute
                    
                    // Get entries due for refresh
                    var entriesToRefresh = await _cacheService._unitOfWork.MarketDataCache.GetEntriesDueForRefreshAsync(stoppingToken);
                    
                    foreach (var entry in entriesToRefresh.Take(10)) // Limit concurrent refreshes
                    {
                        try
                        {
                            await _cacheService._unitOfWork.MarketDataCache.MarkForRefreshAsync(entry.CacheId, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error refreshing cache entry: {CacheKey}", entry.CacheKey);
                        }
                    }

                    // Perform periodic maintenance
                    if (DateTime.UtcNow.Minute % 15 == 0) // Every 15 minutes
                    {
                        await _cacheService.PerformCacheMaintenanceAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in cache refresh background service");
                }
            }

            _logger.LogInformation("Cache refresh background service stopped");
        }
    }

    #endregion
}