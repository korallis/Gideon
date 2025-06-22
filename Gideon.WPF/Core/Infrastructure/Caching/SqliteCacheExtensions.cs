// ==========================================================================
// SqliteCacheExtensions.cs - SQLite Distributed Cache Extensions
// ==========================================================================
// Extension methods for SQLite-based distributed caching.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;

namespace Gideon.WPF.Core.Infrastructure.Caching;

/// <summary>
/// Options for SQLite distributed cache
/// </summary>
public class SqliteCacheOptions
{
    public string CachePath { get; set; } = "cache.db";
    public TimeSpan DefaultSlidingExpiration { get; set; } = TimeSpan.FromMinutes(20);
}

/// <summary>
/// Extension methods for SQLite distributed caching
/// </summary>
public static class SqliteCacheExtensions
{
    /// <summary>
    /// Add SQLite-based distributed cache
    /// </summary>
    public static IServiceCollection AddDistributedSqliteCache(this IServiceCollection services, Action<SqliteCacheOptions> setupAction)
    {
        var options = new SqliteCacheOptions();
        setupAction(options);

        // For now, use memory cache as a placeholder
        // In a real implementation, this would be a SQLite-backed cache
        services.AddDistributedMemoryCache();

        return services;
    }
}