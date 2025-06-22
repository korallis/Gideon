// ==========================================================================
// IMarketGroupService.cs - Market Group Service Interface
// ==========================================================================
// Service interface for market group management and hierarchy operations.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Core.Domain.Interfaces;

namespace Gideon.WPF.Core.Application.Services;

/// <summary>
/// Service interface for market group management
/// </summary>
public interface IMarketGroupService
{
    /// <summary>
    /// Get market group tree structure
    /// </summary>
    Task<MarketGroupTree> GetMarketGroupTreeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Search market groups with filters
    /// </summary>
    Task<MarketGroupSearchResult> SearchMarketGroupsAsync(MarketGroupSearchCriteria criteria, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get breadcrumb navigation for a market group
    /// </summary>
    Task<List<MarketGroupBreadcrumb>> GetMarketGroupBreadcrumbAsync(int marketGroupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get suggested market groups based on user activity
    /// </summary>
    Task<List<SuggestedMarketGroup>> GetSuggestedMarketGroupsAsync(int characterId, int count = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronize market groups from ESI
    /// </summary>
    Task<MarketGroupSyncResult> SynchronizeMarketGroupsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get market group analytics
    /// </summary>
    Task<MarketGroupAnalytics> GetMarketGroupAnalyticsAsync(int marketGroupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get favorite market groups for a user
    /// </summary>
    Task<List<EveMarketGroup>> GetFavoriteMarketGroupsAsync(int characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add market group to favorites
    /// </summary>
    Task AddToFavoritesAsync(int characterId, int marketGroupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove market group from favorites
    /// </summary>
    Task RemoveFromFavoritesAsync(int characterId, int marketGroupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get market group recommendations based on current market conditions
    /// </summary>
    Task<List<MarketGroupRecommendation>> GetMarketGroupRecommendationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate market group hierarchy integrity
    /// </summary>
    Task<MarketGroupValidationResult> ValidateMarketGroupHierarchyAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Market group tree structure
/// </summary>
public class MarketGroupTree
{
    public List<MarketGroupNode> RootGroups { get; set; } = new();
    public int TotalGroups { get; set; }
    public int MaxDepth { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Market group tree node
/// </summary>
public class MarketGroupNode
{
    public int MarketGroupId { get; set; }
    public string MarketGroupName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconId { get; set; }
    public bool HasTypes { get; set; }
    public int TypeCount { get; set; }
    public List<MarketGroupNode> Children { get; set; } = new();
    public int Depth { get; set; }
    public bool IsExpanded { get; set; }
}

/// <summary>
/// Market group search criteria
/// </summary>
public class MarketGroupSearchCriteria
{
    public string? SearchTerm { get; set; }
    public int? ParentGroupId { get; set; }
    public bool? HasTypes { get; set; }
    public int? MinTypeCount { get; set; }
    public int? MaxDepth { get; set; }
    public MarketGroupSortOrder SortOrder { get; set; } = MarketGroupSortOrder.Name;
    public int PageSize { get; set; } = 50;
    public int PageNumber { get; set; } = 1;
}

/// <summary>
/// Market group search result
/// </summary>
public class MarketGroupSearchResult
{
    public List<MarketGroupSearchItem> Groups { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public string? SearchTerm { get; set; }
}

/// <summary>
/// Market group search item
/// </summary>
public class MarketGroupSearchItem
{
    public int MarketGroupId { get; set; }
    public string MarketGroupName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FullPath { get; set; } = string.Empty;
    public int TypeCount { get; set; }
    public double RelevanceScore { get; set; }
    public bool HasTypes { get; set; }
    public string? IconId { get; set; }
}

/// <summary>
/// Market group breadcrumb item
/// </summary>
public class MarketGroupBreadcrumb
{
    public int MarketGroupId { get; set; }
    public string MarketGroupName { get; set; } = string.Empty;
    public int Level { get; set; }
    public bool IsLeaf { get; set; }
}

/// <summary>
/// Suggested market group
/// </summary>
public class SuggestedMarketGroup
{
    public int MarketGroupId { get; set; }
    public string MarketGroupName { get; set; } = string.Empty;
    public string ReasonCode { get; set; } = string.Empty;
    public string ReasonDescription { get; set; } = string.Empty;
    public double Score { get; set; }
    public int TypeCount { get; set; }
    public decimal RecentTradingVolume { get; set; }
}

/// <summary>
/// Market group synchronization result
/// </summary>
public class MarketGroupSyncResult
{
    public bool Success { get; set; }
    public DateTime SyncStartTime { get; set; }
    public DateTime SyncEndTime { get; set; }
    public TimeSpan Duration => SyncEndTime - SyncStartTime;
    
    public int GroupsRetrieved { get; set; }
    public int GroupsAdded { get; set; }
    public int GroupsUpdated { get; set; }
    public int GroupsSkipped { get; set; }
    public int GroupsErrored { get; set; }
    
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Market group analytics
/// </summary>
public class MarketGroupAnalytics
{
    public int MarketGroupId { get; set; }
    public string MarketGroupName { get; set; } = string.Empty;
    public MarketGroupStatistics Statistics { get; set; } = new();
    public List<TopTradedType> TopTypes { get; set; } = new();
    public PriceAnalysis PriceAnalysis { get; set; } = new();
    public VolumeAnalysis VolumeAnalysis { get; set; } = new();
    public TrendAnalysis TrendAnalysis { get; set; } = new();
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Top traded type in market group
/// </summary>
public class TopTradedType
{
    public int TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public long VolumeTraded { get; set; }
    public decimal ValueTraded { get; set; }
    public decimal AveragePrice { get; set; }
    public int TransactionCount { get; set; }
    public double MarketShare { get; set; }
}

/// <summary>
/// Price analysis for market group
/// </summary>
public class PriceAnalysis
{
    public decimal AveragePrice { get; set; }
    public decimal MedianPrice { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public decimal PriceRange { get; set; }
    public double PriceVolatility { get; set; }
    public decimal WeightedAveragePrice { get; set; }
}

/// <summary>
/// Volume analysis for market group
/// </summary>
public class VolumeAnalysis
{
    public long TotalVolume { get; set; }
    public long AverageVolume { get; set; }
    public long MedianVolume { get; set; }
    public long PeakVolume { get; set; }
    public DateTime PeakVolumeDate { get; set; }
    public double VolumeGrowthRate { get; set; }
    public double VolumeVolatility { get; set; }
}

/// <summary>
/// Trend analysis for market group
/// </summary>
public class TrendAnalysis
{
    public TrendDirection PriceTrend { get; set; }
    public TrendDirection VolumeTrend { get; set; }
    public double PriceTrendStrength { get; set; }
    public double VolumeTrendStrength { get; set; }
    public int TrendPeriodDays { get; set; }
    public List<TrendDataPoint> TrendPoints { get; set; } = new();
}

/// <summary>
/// Trend data point
/// </summary>
public class TrendDataPoint
{
    public DateTime Date { get; set; }
    public decimal Price { get; set; }
    public long Volume { get; set; }
    public decimal Value { get; set; }
}

/// <summary>
/// Market group recommendation
/// </summary>
public class MarketGroupRecommendation
{
    public int MarketGroupId { get; set; }
    public string MarketGroupName { get; set; } = string.Empty;
    public string RecommendationType { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public decimal PotentialProfit { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public DateTime ValidUntil { get; set; }
}

/// <summary>
/// Enumerations
/// </summary>
public enum MarketGroupSortOrder
{
    Name,
    TypeCount,
    TradingVolume,
    Popularity,
    RecentActivity
}

public enum TrendDirection
{
    Unknown,
    Up,
    Down,
    Stable,
    Volatile
}