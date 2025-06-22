// ==========================================================================
// MarketGroupRepository.cs - Market Group Repository Implementation
// ==========================================================================
// Concrete implementation of market group repository with specialized operations.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Core.Domain.Interfaces;
using Gideon.WPF.Core.Infrastructure.Data;

namespace Gideon.WPF.Core.Infrastructure.Persistence;

/// <summary>
/// Market group repository implementation with specialized operations
/// </summary>
public class MarketGroupRepository : Repository<EveMarketGroup>, IMarketGroupRepository
{
    public MarketGroupRepository(GideonDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get all root market groups (no parent)
    /// </summary>
    public async Task<IEnumerable<EveMarketGroup>> GetRootMarketGroupsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(mg => mg.ChildGroups)
            .Where(mg => mg.ParentGroupId == null)
            .OrderBy(mg => mg.MarketGroupName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get all child market groups for a parent
    /// </summary>
    public async Task<IEnumerable<EveMarketGroup>> GetChildMarketGroupsAsync(int parentGroupId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(mg => mg.ChildGroups)
            .Where(mg => mg.ParentGroupId == parentGroupId)
            .OrderBy(mg => mg.MarketGroupName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get market group hierarchy with specified depth
    /// </summary>
    public async Task<IEnumerable<EveMarketGroup>> GetMarketGroupHierarchyAsync(int? rootGroupId = null, int maxDepth = 5, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        // Start from root if no specific group specified
        if (rootGroupId.HasValue)
        {
            query = query.Where(mg => mg.MarketGroupId == rootGroupId.Value || 
                                     IsDescendantOf(mg.MarketGroupId, rootGroupId.Value));
        }
        else
        {
            query = query.Where(mg => mg.ParentGroupId == null || 
                                     GetDepthFromRoot(mg.MarketGroupId) <= maxDepth);
        }

        return await query
            .Include(mg => mg.ParentGroup)
            .Include(mg => mg.ChildGroups)
            .OrderBy(mg => mg.MarketGroupName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get market groups that contain types
    /// </summary>
    public async Task<IEnumerable<EveMarketGroup>> GetMarketGroupsWithTypesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(mg => mg.Types)
            .Where(mg => mg.HasTypes && mg.Types.Any())
            .OrderBy(mg => mg.MarketGroupName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get all types in a market group (including child groups)
    /// </summary>
    public async Task<IEnumerable<EveType>> GetTypesInMarketGroupAsync(int marketGroupId, bool includeChildGroups = true, CancellationToken cancellationToken = default)
    {
        var query = _context.EveTypes.AsQueryable();

        if (includeChildGroups)
        {
            // Get all descendant group IDs
            var descendantGroupIds = await GetDescendantGroupIdsAsync(marketGroupId, cancellationToken);
            descendantGroupIds.Add(marketGroupId);

            query = query.Where(t => t.MarketGroupId.HasValue && descendantGroupIds.Contains(t.MarketGroupId.Value));
        }
        else
        {
            query = query.Where(t => t.MarketGroupId == marketGroupId);
        }

        return await query
            .Include(t => t.Group)
            .Include(t => t.MarketGroup)
            .OrderBy(t => t.TypeName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Search market groups by name
    /// </summary>
    public async Task<IEnumerable<EveMarketGroup>> SearchMarketGroupsByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Enumerable.Empty<EveMarketGroup>();

        var normalizedSearchTerm = searchTerm.Trim().ToLower();

        return await _dbSet
            .Include(mg => mg.ParentGroup)
            .Include(mg => mg.ChildGroups)
            .Where(mg => mg.MarketGroupName.ToLower().Contains(normalizedSearchTerm) ||
                        (mg.Description != null && mg.Description.ToLower().Contains(normalizedSearchTerm)))
            .OrderBy(mg => mg.MarketGroupName.ToLower().IndexOf(normalizedSearchTerm))
            .ThenBy(mg => mg.MarketGroupName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get market group path from root to specified group
    /// </summary>
    public async Task<List<EveMarketGroup>> GetMarketGroupPathAsync(int marketGroupId, CancellationToken cancellationToken = default)
    {
        var path = new List<EveMarketGroup>();
        var currentGroup = await GetByIdAsync(marketGroupId, cancellationToken);

        while (currentGroup != null)
        {
            path.Insert(0, currentGroup);
            
            if (currentGroup.ParentGroupId.HasValue)
            {
                currentGroup = await _dbSet
                    .Include(mg => mg.ParentGroup)
                    .FirstOrDefaultAsync(mg => mg.MarketGroupId == currentGroup.ParentGroupId.Value, cancellationToken);
            }
            else
            {
                break;
            }
        }

        return path;
    }

    /// <summary>
    /// Get market group statistics
    /// </summary>
    public async Task<MarketGroupStatistics> GetMarketGroupStatisticsAsync(int marketGroupId, CancellationToken cancellationToken = default)
    {
        var marketGroup = await _dbSet
            .Include(mg => mg.ChildGroups)
            .Include(mg => mg.Types)
            .FirstOrDefaultAsync(mg => mg.MarketGroupId == marketGroupId, cancellationToken);

        if (marketGroup == null)
        {
            return new MarketGroupStatistics { MarketGroupId = marketGroupId };
        }

        var descendantGroupIds = await GetDescendantGroupIdsAsync(marketGroupId, cancellationToken);
        var totalChildGroups = descendantGroupIds.Count;
        var totalTypes = await _context.EveTypes
            .CountAsync(t => t.MarketGroupId.HasValue && 
                           (t.MarketGroupId == marketGroupId || descendantGroupIds.Contains(t.MarketGroupId.Value)), 
                       cancellationToken);

        var maxDepth = await CalculateMaxDepthAsync(marketGroupId, cancellationToken);

        return new MarketGroupStatistics
        {
            MarketGroupId = marketGroupId,
            MarketGroupName = marketGroup.MarketGroupName,
            DirectChildGroups = marketGroup.ChildGroups.Count,
            TotalChildGroups = totalChildGroups,
            DirectTypes = marketGroup.Types.Count,
            TotalTypes = totalTypes,
            MaxDepth = maxDepth,
            LastUpdated = marketGroup.LastUpdated
        };
    }

    /// <summary>
    /// Bulk update market groups from ESI data
    /// </summary>
    public async Task BulkUpdateMarketGroupsAsync(IEnumerable<EveMarketGroup> marketGroups, CancellationToken cancellationToken = default)
    {
        var marketGroupList = marketGroups.ToList();
        var existingGroupIds = await _dbSet
            .Where(mg => marketGroupList.Select(m => m.MarketGroupId).Contains(mg.MarketGroupId))
            .Select(mg => mg.MarketGroupId)
            .ToListAsync(cancellationToken);

        var newGroups = marketGroupList
            .Where(mg => !existingGroupIds.Contains(mg.MarketGroupId))
            .ToList();

        var updatedGroups = marketGroupList
            .Where(mg => existingGroupIds.Contains(mg.MarketGroupId))
            .ToList();

        // Add new groups
        if (newGroups.Any())
        {
            await _dbSet.AddRangeAsync(newGroups, cancellationToken);
        }

        // Update existing groups
        foreach (var updatedGroup in updatedGroups)
        {
            var existingGroup = await _dbSet
                .FirstOrDefaultAsync(mg => mg.MarketGroupId == updatedGroup.MarketGroupId, cancellationToken);
            
            if (existingGroup != null)
            {
                existingGroup.MarketGroupName = updatedGroup.MarketGroupName;
                existingGroup.Description = updatedGroup.Description;
                existingGroup.ParentGroupId = updatedGroup.ParentGroupId;
                existingGroup.HasTypes = updatedGroup.HasTypes;
                existingGroup.IconId = updatedGroup.IconId;
                existingGroup.LastUpdated = DateTime.UtcNow;
            }
        }
    }

    /// <summary>
    /// Get popular market groups based on trading activity
    /// </summary>
    public async Task<IEnumerable<PopularMarketGroup>> GetPopularMarketGroupsAsync(int count = 20, CancellationToken cancellationToken = default)
    {
        // This would typically join with transaction data, but for now we'll use type counts
        var popularGroups = await _dbSet
            .Include(mg => mg.Types)
            .Where(mg => mg.HasTypes && mg.Types.Any())
            .Select(mg => new PopularMarketGroup
            {
                MarketGroupId = mg.MarketGroupId,
                MarketGroupName = mg.MarketGroupName,
                FullPath = GetFullPath(mg),
                TypeCount = mg.Types.Count,
                TransactionCount = 0, // Would be calculated from actual transaction data
                TotalValue = 0, // Would be calculated from actual transaction data
                PopularityScore = mg.Types.Count // Simple popularity based on type count for now
            })
            .OrderByDescending(pg => pg.PopularityScore)
            .Take(count)
            .ToListAsync(cancellationToken);

        return popularGroups;
    }

    /// <summary>
    /// Get market groups by category
    /// </summary>
    public async Task<IEnumerable<EveMarketGroup>> GetMarketGroupsByCategoryAsync(string categoryName, CancellationToken cancellationToken = default)
    {
        // This would need to be enhanced with actual category mapping
        return await _dbSet
            .Include(mg => mg.ParentGroup)
            .Include(mg => mg.ChildGroups)
            .Where(mg => mg.MarketGroupName.ToLower().Contains(categoryName.ToLower()) ||
                        (mg.Description != null && mg.Description.ToLower().Contains(categoryName.ToLower())))
            .OrderBy(mg => mg.MarketGroupName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Validate market group hierarchy integrity
    /// </summary>
    public async Task<MarketGroupValidationResult> ValidateMarketGroupHierarchyAsync(CancellationToken cancellationToken = default)
    {
        var result = new MarketGroupValidationResult { IsValid = true };

        // Check for orphaned groups (parent ID that doesn't exist)
        var orphanedGroups = await _dbSet
            .Where(mg => mg.ParentGroupId.HasValue && 
                        !_dbSet.Any(parent => parent.MarketGroupId == mg.ParentGroupId))
            .CountAsync(cancellationToken);

        result.OrphanedGroups = orphanedGroups;
        if (orphanedGroups > 0)
        {
            result.IsValid = false;
            result.Errors.Add($"Found {orphanedGroups} orphaned market groups with invalid parent IDs");
        }

        // Check for circular references
        var circularReferences = await DetectCircularReferencesAsync(cancellationToken);
        result.CircularReferences = circularReferences;
        if (circularReferences > 0)
        {
            result.IsValid = false;
            result.Errors.Add($"Found {circularReferences} circular references in market group hierarchy");
        }

        // Check for groups with missing parent references
        var missingParents = await _dbSet
            .Where(mg => mg.ParentGroupId.HasValue)
            .GroupBy(mg => mg.ParentGroupId)
            .Where(g => !_dbSet.Any(mg => mg.MarketGroupId == g.Key))
            .CountAsync(cancellationToken);

        result.MissingParents = missingParents;
        if (missingParents > 0)
        {
            result.Warnings.Add($"Found {missingParents} missing parent groups");
        }

        return result;
    }

    #region Private Helper Methods

    /// <summary>
    /// Check if a group is descendant of another group
    /// </summary>
    private bool IsDescendantOf(int groupId, int ancestorId)
    {
        // This is a simplified check - in practice, you'd implement proper tree traversal
        return _dbSet.Any(mg => mg.MarketGroupId == groupId && mg.ParentGroupId == ancestorId);
    }

    /// <summary>
    /// Calculate depth from root for a group
    /// </summary>
    private int GetDepthFromRoot(int groupId)
    {
        // This would need to be implemented with recursive query or iteration
        // For now, return a default value
        return 1;
    }

    /// <summary>
    /// Get all descendant group IDs for a parent group
    /// </summary>
    private async Task<List<int>> GetDescendantGroupIdsAsync(int parentGroupId, CancellationToken cancellationToken)
    {
        var descendants = new List<int>();
        var toProcess = new Queue<int>();
        toProcess.Enqueue(parentGroupId);

        while (toProcess.Count > 0)
        {
            var currentGroupId = toProcess.Dequeue();
            var childGroups = await _dbSet
                .Where(mg => mg.ParentGroupId == currentGroupId)
                .Select(mg => mg.MarketGroupId)
                .ToListAsync(cancellationToken);

            foreach (var childId in childGroups)
            {
                if (!descendants.Contains(childId))
                {
                    descendants.Add(childId);
                    toProcess.Enqueue(childId);
                }
            }
        }

        return descendants;
    }

    /// <summary>
    /// Calculate maximum depth from a group
    /// </summary>
    private async Task<int> CalculateMaxDepthAsync(int groupId, CancellationToken cancellationToken)
    {
        int maxDepth = 0;
        var childGroups = await _dbSet
            .Where(mg => mg.ParentGroupId == groupId)
            .Select(mg => mg.MarketGroupId)
            .ToListAsync(cancellationToken);

        foreach (var childId in childGroups)
        {
            var childDepth = await CalculateMaxDepthAsync(childId, cancellationToken);
            maxDepth = Math.Max(maxDepth, childDepth + 1);
        }

        return maxDepth;
    }

    /// <summary>
    /// Get full path string for a market group
    /// </summary>
    private static string GetFullPath(EveMarketGroup group)
    {
        // This would need to traverse up the hierarchy to build the full path
        // For now, return just the group name
        return group.MarketGroupName;
    }

    /// <summary>
    /// Detect circular references in the hierarchy
    /// </summary>
    private async Task<int> DetectCircularReferencesAsync(CancellationToken cancellationToken)
    {
        var allGroups = await _dbSet
            .Select(mg => new { mg.MarketGroupId, mg.ParentGroupId })
            .ToListAsync(cancellationToken);

        int circularCount = 0;
        var visited = new HashSet<int>();

        foreach (var group in allGroups)
        {
            if (visited.Contains(group.MarketGroupId))
                continue;

            var path = new HashSet<int>();
            var current = group;

            while (current != null && current.ParentGroupId.HasValue)
            {
                if (path.Contains(current.MarketGroupId))
                {
                    circularCount++;
                    break;
                }

                path.Add(current.MarketGroupId);
                visited.Add(current.MarketGroupId);

                current = allGroups.FirstOrDefault(g => g.MarketGroupId == current.ParentGroupId);
            }
        }

        return circularCount;
    }

    #endregion
}