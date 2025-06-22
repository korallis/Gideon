// ==========================================================================
// MarketGroupService.cs - Market Group Service Implementation
// ==========================================================================
// Service implementation for market group management and hierarchy operations.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.Extensions.Logging;
using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Core.Domain.Interfaces;

namespace Gideon.WPF.Core.Application.Services;

/// <summary>
/// Market group service implementation
/// </summary>
public class MarketGroupService : IMarketGroupService
{
    private readonly IMarketGroupRepository _marketGroupRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarketGroupService> _logger;

    public MarketGroupService(
        IMarketGroupRepository marketGroupRepository,
        IUnitOfWork unitOfWork,
        ILogger<MarketGroupService> logger)
    {
        _marketGroupRepository = marketGroupRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Get market group tree structure
    /// </summary>
    public async Task<MarketGroupTree> GetMarketGroupTreeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Building market group tree structure");

        var rootGroups = await _marketGroupRepository.GetRootMarketGroupsAsync(cancellationToken);
        var tree = new MarketGroupTree
        {
            LastUpdated = DateTime.UtcNow
        };

        foreach (var rootGroup in rootGroups)
        {
            var rootNode = await BuildMarketGroupNodeAsync(rootGroup, 0, cancellationToken);
            tree.RootGroups.Add(rootNode);
        }

        tree.TotalGroups = await CountTotalGroupsInTree(tree.RootGroups);
        tree.MaxDepth = CalculateMaxDepth(tree.RootGroups);

        _logger.LogDebug("Market group tree built with {GroupCount} groups and max depth {MaxDepth}",
            tree.TotalGroups, tree.MaxDepth);

        return tree;
    }

    /// <summary>
    /// Search market groups with filters
    /// </summary>
    public async Task<MarketGroupSearchResult> SearchMarketGroupsAsync(MarketGroupSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Searching market groups with criteria: {SearchTerm}", criteria.SearchTerm);

        var result = new MarketGroupSearchResult
        {
            PageNumber = criteria.PageNumber,
            PageSize = criteria.PageSize,
            SearchTerm = criteria.SearchTerm
        };

        // Get all matching groups
        IEnumerable<EveMarketGroup> groups;
        
        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            groups = await _marketGroupRepository.SearchMarketGroupsByNameAsync(criteria.SearchTerm, cancellationToken);
        }
        else if (criteria.ParentGroupId.HasValue)
        {
            groups = await _marketGroupRepository.GetChildMarketGroupsAsync(criteria.ParentGroupId.Value, cancellationToken);
        }
        else
        {
            groups = await _marketGroupRepository.GetRootMarketGroupsAsync(cancellationToken);
        }

        // Apply filters
        var filteredGroups = groups.AsQueryable();

        if (criteria.HasTypes.HasValue)
        {
            filteredGroups = filteredGroups.Where(g => g.HasTypes == criteria.HasTypes.Value);
        }

        if (criteria.MinTypeCount.HasValue)
        {
            filteredGroups = filteredGroups.Where(g => g.Types.Count >= criteria.MinTypeCount.Value);
        }

        // Apply sorting
        filteredGroups = criteria.SortOrder switch
        {
            MarketGroupSortOrder.Name => filteredGroups.OrderBy(g => g.MarketGroupName),
            MarketGroupSortOrder.TypeCount => filteredGroups.OrderByDescending(g => g.Types.Count),
            MarketGroupSortOrder.TradingVolume => filteredGroups.OrderBy(g => g.MarketGroupName), // Would need actual volume data
            MarketGroupSortOrder.Popularity => filteredGroups.OrderBy(g => g.MarketGroupName), // Would need popularity metrics
            MarketGroupSortOrder.RecentActivity => filteredGroups.OrderBy(g => g.LastUpdated),
            _ => filteredGroups.OrderBy(g => g.MarketGroupName)
        };

        var groupList = filteredGroups.ToList();
        result.TotalCount = groupList.Count;

        // Apply pagination
        var pagedGroups = groupList
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize);

        // Convert to search items
        foreach (var group in pagedGroups)
        {
            var searchItem = new MarketGroupSearchItem
            {
                MarketGroupId = group.MarketGroupId,
                MarketGroupName = group.MarketGroupName,
                Description = group.Description,
                TypeCount = group.Types.Count,
                HasTypes = group.HasTypes,
                IconId = group.IconId,
                FullPath = await BuildFullPathAsync(group.MarketGroupId, cancellationToken),
                RelevanceScore = CalculateRelevanceScore(group, criteria.SearchTerm)
            };

            result.Groups.Add(searchItem);
        }

        _logger.LogDebug("Market group search returned {Count} groups from {Total} total",
            result.Groups.Count, result.TotalCount);

        return result;
    }

    /// <summary>
    /// Get breadcrumb navigation for a market group
    /// </summary>
    public async Task<List<MarketGroupBreadcrumb>> GetMarketGroupBreadcrumbAsync(int marketGroupId, CancellationToken cancellationToken = default)
    {
        var path = await _marketGroupRepository.GetMarketGroupPathAsync(marketGroupId, cancellationToken);
        var breadcrumbs = new List<MarketGroupBreadcrumb>();

        for (int i = 0; i < path.Count; i++)
        {
            var group = path[i];
            breadcrumbs.Add(new MarketGroupBreadcrumb
            {
                MarketGroupId = group.MarketGroupId,
                MarketGroupName = group.MarketGroupName,
                Level = i,
                IsLeaf = i == path.Count - 1
            });
        }

        return breadcrumbs;
    }

    /// <summary>
    /// Get suggested market groups based on user activity
    /// </summary>
    public async Task<List<SuggestedMarketGroup>> GetSuggestedMarketGroupsAsync(int characterId, int count = 10, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting suggested market groups for character {CharacterId}", characterId);

        var suggestions = new List<SuggestedMarketGroup>();

        // Get popular market groups
        var popularGroups = await _marketGroupRepository.GetPopularMarketGroupsAsync(count * 2, cancellationToken);

        foreach (var popularGroup in popularGroups.Take(count))
        {
            suggestions.Add(new SuggestedMarketGroup
            {
                MarketGroupId = popularGroup.MarketGroupId,
                MarketGroupName = popularGroup.MarketGroupName,
                ReasonCode = "POPULAR",
                ReasonDescription = "Popular trading category",
                Score = popularGroup.PopularityScore,
                TypeCount = popularGroup.TypeCount,
                RecentTradingVolume = popularGroup.TotalValue
            });
        }

        // TODO: Add more sophisticated suggestions based on:
        // - Character's trading history
        // - Similar characters' preferences
        // - Market trends
        // - Character's skills and assets

        return suggestions.OrderByDescending(s => s.Score).Take(count).ToList();
    }

    /// <summary>
    /// Synchronize market groups from ESI
    /// </summary>
    public async Task<MarketGroupSyncResult> SynchronizeMarketGroupsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting market group synchronization from ESI");

        var result = new MarketGroupSyncResult
        {
            Success = false,
            SyncStartTime = DateTime.UtcNow
        };

        try
        {
            // TODO: In a real implementation, this would call ESI API
            // var esiMarketGroups = await _esiClient.GetMarketGroupsAsync();
            
            // For now, simulate ESI call with empty list
            var esiMarketGroups = new List<EveMarketGroup>();

            result.GroupsRetrieved = esiMarketGroups.Count;

            if (esiMarketGroups.Any())
            {
                await _marketGroupRepository.BulkUpdateMarketGroupsAsync(esiMarketGroups, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Count the changes (simplified for demo)
                result.GroupsAdded = esiMarketGroups.Count;
                result.GroupsUpdated = 0;
            }

            result.Success = true;
            result.SyncEndTime = DateTime.UtcNow;

            _logger.LogInformation("Market group synchronization completed. Retrieved: {Retrieved}, Added: {Added}, Updated: {Updated}",
                result.GroupsRetrieved, result.GroupsAdded, result.GroupsUpdated);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.Errors.Add(ex.ToString());
            result.SyncEndTime = DateTime.UtcNow;

            _logger.LogError(ex, "Error during market group synchronization");
        }

        return result;
    }

    /// <summary>
    /// Get market group analytics
    /// </summary>
    public async Task<MarketGroupAnalytics> GetMarketGroupAnalyticsAsync(int marketGroupId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Generating analytics for market group {MarketGroupId}", marketGroupId);

        var marketGroup = await _marketGroupRepository.GetByIdAsync(marketGroupId, cancellationToken);
        if (marketGroup == null)
        {
            throw new ArgumentException($"Market group {marketGroupId} not found");
        }

        var statistics = await _marketGroupRepository.GetMarketGroupStatisticsAsync(marketGroupId, cancellationToken);
        
        var analytics = new MarketGroupAnalytics
        {
            MarketGroupId = marketGroupId,
            MarketGroupName = marketGroup.MarketGroupName,
            Statistics = statistics,
            AnalysisDate = DateTime.UtcNow
        };

        // TODO: Calculate actual analytics from market data
        // - Get top traded types
        // - Calculate price and volume analysis
        // - Generate trend analysis
        // - This would require integration with market data and transaction history

        return analytics;
    }

    /// <summary>
    /// Get favorite market groups for a user
    /// </summary>
    public async Task<List<EveMarketGroup>> GetFavoriteMarketGroupsAsync(int characterId, CancellationToken cancellationToken = default)
    {
        // TODO: This would require a user preferences/favorites table
        // For now, return empty list
        return new List<EveMarketGroup>();
    }

    /// <summary>
    /// Add market group to favorites
    /// </summary>
    public async Task AddToFavoritesAsync(int characterId, int marketGroupId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Adding market group {MarketGroupId} to favorites for character {CharacterId}",
            marketGroupId, characterId);

        // TODO: Implement favorites functionality
        // This would require a user preferences table to store favorite market groups
    }

    /// <summary>
    /// Remove market group from favorites
    /// </summary>
    public async Task RemoveFromFavoritesAsync(int characterId, int marketGroupId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Removing market group {MarketGroupId} from favorites for character {CharacterId}",
            marketGroupId, characterId);

        // TODO: Implement favorites functionality
    }

    /// <summary>
    /// Get market group recommendations based on current market conditions
    /// </summary>
    public async Task<List<MarketGroupRecommendation>> GetMarketGroupRecommendationsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Generating market group recommendations");

        var recommendations = new List<MarketGroupRecommendation>();

        // TODO: Implement sophisticated recommendation engine based on:
        // - Market volatility
        // - Price trends
        // - Volume trends
        // - Profit opportunities
        // - Risk analysis
        // - Current market conditions

        return recommendations;
    }

    /// <summary>
    /// Validate market group hierarchy integrity
    /// </summary>
    public async Task<MarketGroupValidationResult> ValidateMarketGroupHierarchyAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating market group hierarchy integrity");
        
        var result = await _marketGroupRepository.ValidateMarketGroupHierarchyAsync(cancellationToken);
        
        _logger.LogInformation("Market group validation completed. Valid: {IsValid}, Errors: {ErrorCount}, Warnings: {WarningCount}",
            result.IsValid, result.Errors.Count, result.Warnings.Count);

        return result;
    }

    #region Private Helper Methods

    /// <summary>
    /// Build a market group node with its children
    /// </summary>
    private async Task<MarketGroupNode> BuildMarketGroupNodeAsync(EveMarketGroup group, int depth, CancellationToken cancellationToken)
    {
        var node = new MarketGroupNode
        {
            MarketGroupId = group.MarketGroupId,
            MarketGroupName = group.MarketGroupName,
            Description = group.Description,
            IconId = group.IconId,
            HasTypes = group.HasTypes,
            TypeCount = group.Types.Count,
            Depth = depth,
            IsExpanded = depth < 2 // Auto-expand first two levels
        };

        // Get child groups
        var childGroups = await _marketGroupRepository.GetChildMarketGroupsAsync(group.MarketGroupId, cancellationToken);
        
        foreach (var childGroup in childGroups)
        {
            var childNode = await BuildMarketGroupNodeAsync(childGroup, depth + 1, cancellationToken);
            node.Children.Add(childNode);
        }

        return node;
    }

    /// <summary>
    /// Count total groups in tree
    /// </summary>
    private async Task<int> CountTotalGroupsInTree(List<MarketGroupNode> nodes)
    {
        int count = nodes.Count;
        foreach (var node in nodes)
        {
            count += await CountTotalGroupsInTree(node.Children);
        }
        return count;
    }

    /// <summary>
    /// Calculate maximum depth in tree
    /// </summary>
    private static int CalculateMaxDepth(List<MarketGroupNode> nodes)
    {
        if (!nodes.Any()) return 0;
        
        int maxDepth = 0;
        foreach (var node in nodes)
        {
            int childDepth = CalculateMaxDepth(node.Children);
            maxDepth = Math.Max(maxDepth, node.Depth + childDepth + 1);
        }
        return maxDepth;
    }

    /// <summary>
    /// Build full path string for a market group
    /// </summary>
    private async Task<string> BuildFullPathAsync(int marketGroupId, CancellationToken cancellationToken)
    {
        var path = await _marketGroupRepository.GetMarketGroupPathAsync(marketGroupId, cancellationToken);
        return string.Join(" > ", path.Select(g => g.MarketGroupName));
    }

    /// <summary>
    /// Calculate relevance score for search results
    /// </summary>
    private static double CalculateRelevanceScore(EveMarketGroup group, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return 1.0;

        double score = 0.0;
        var lowerSearchTerm = searchTerm.ToLower();
        var lowerGroupName = group.MarketGroupName.ToLower();

        // Exact match gets highest score
        if (lowerGroupName == lowerSearchTerm)
            score += 100.0;
        // Starts with search term
        else if (lowerGroupName.StartsWith(lowerSearchTerm))
            score += 50.0;
        // Contains search term
        else if (lowerGroupName.Contains(lowerSearchTerm))
            score += 25.0;

        // Description match
        if (!string.IsNullOrWhiteSpace(group.Description))
        {
            var lowerDescription = group.Description.ToLower();
            if (lowerDescription.Contains(lowerSearchTerm))
                score += 10.0;
        }

        // Boost score for groups with types
        if (group.HasTypes)
            score += 5.0;

        return score;
    }

    #endregion
}