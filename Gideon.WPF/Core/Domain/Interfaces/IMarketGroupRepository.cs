// ==========================================================================
// IMarketGroupRepository.cs - Market Group Repository Interface
// ==========================================================================
// Repository interface for EVE Online market group management with specialized operations.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Gideon.WPF.Core.Domain.Entities;

namespace Gideon.WPF.Core.Domain.Interfaces;

/// <summary>
/// Repository interface for market group management
/// </summary>
public interface IMarketGroupRepository : IRepository<EveMarketGroup>
{
    /// <summary>
    /// Get all root market groups (no parent)
    /// </summary>
    Task<IEnumerable<EveMarketGroup>> GetRootMarketGroupsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all child market groups for a parent
    /// </summary>
    Task<IEnumerable<EveMarketGroup>> GetChildMarketGroupsAsync(int parentGroupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get market group hierarchy with specified depth
    /// </summary>
    Task<IEnumerable<EveMarketGroup>> GetMarketGroupHierarchyAsync(int? rootGroupId = null, int maxDepth = 5, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get market groups that contain types
    /// </summary>
    Task<IEnumerable<EveMarketGroup>> GetMarketGroupsWithTypesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all types in a market group (including child groups)
    /// </summary>
    Task<IEnumerable<EveType>> GetTypesInMarketGroupAsync(int marketGroupId, bool includeChildGroups = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search market groups by name
    /// </summary>
    Task<IEnumerable<EveMarketGroup>> SearchMarketGroupsByNameAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get market group path from root to specified group
    /// </summary>
    Task<List<EveMarketGroup>> GetMarketGroupPathAsync(int marketGroupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get market group statistics
    /// </summary>
    Task<MarketGroupStatistics> GetMarketGroupStatisticsAsync(int marketGroupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk update market groups from ESI data
    /// </summary>
    Task BulkUpdateMarketGroupsAsync(IEnumerable<EveMarketGroup> marketGroups, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get popular market groups based on trading activity
    /// </summary>
    Task<IEnumerable<PopularMarketGroup>> GetPopularMarketGroupsAsync(int count = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get market groups by category
    /// </summary>
    Task<IEnumerable<EveMarketGroup>> GetMarketGroupsByCategoryAsync(string categoryName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate market group hierarchy integrity
    /// </summary>
    Task<MarketGroupValidationResult> ValidateMarketGroupHierarchyAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Market group statistics
/// </summary>
public class MarketGroupStatistics
{
    public int MarketGroupId { get; set; }
    public string MarketGroupName { get; set; } = string.Empty;
    public int DirectChildGroups { get; set; }
    public int TotalChildGroups { get; set; }
    public int DirectTypes { get; set; }
    public int TotalTypes { get; set; }
    public int MaxDepth { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Popular market group based on trading activity
/// </summary>
public class PopularMarketGroup
{
    public int MarketGroupId { get; set; }
    public string MarketGroupName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public int TypeCount { get; set; }
    public long TransactionCount { get; set; }
    public decimal TotalValue { get; set; }
    public double PopularityScore { get; set; }
}

/// <summary>
/// Market group hierarchy validation result
/// </summary>
public class MarketGroupValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public int OrphanedGroups { get; set; }
    public int CircularReferences { get; set; }
    public int MissingParents { get; set; }
    public DateTime ValidationDate { get; set; } = DateTime.UtcNow;
}