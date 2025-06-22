// ==========================================================================
// IShipFittingRepository.cs - Ship Fitting Repository Interface
// ==========================================================================
// Specialized repository interface for ship fitting operations.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Gideon.WPF.Core.Domain.Entities;

namespace Gideon.WPF.Core.Domain.Interfaces;

/// <summary>
/// Repository interface for ship fitting operations
/// </summary>
public interface IShipFittingRepository : IRepository<ShipFitting>
{
    /// <summary>
    /// Get fittings by character
    /// </summary>
    Task<IEnumerable<ShipFitting>> GetByCharacterAsync(int characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get fittings by ship type
    /// </summary>
    Task<IEnumerable<ShipFitting>> GetByShipTypeAsync(int shipId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get public fittings
    /// </summary>
    Task<IEnumerable<ShipFitting>> GetPublicFittingsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get shared fittings
    /// </summary>
    Task<IEnumerable<ShipFitting>> GetSharedFittingsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get fittings by category
    /// </summary>
    Task<IEnumerable<ShipFitting>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get fittings by activity type
    /// </summary>
    Task<IEnumerable<ShipFitting>> GetByActivityTypeAsync(string activityType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get favorite fittings for character
    /// </summary>
    Task<IEnumerable<ShipFitting>> GetFavoritesByCharacterAsync(int characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recently used fittings
    /// </summary>
    Task<IEnumerable<ShipFitting>> GetRecentlyUsedAsync(int characterId, int count = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search fittings by name or description
    /// </summary>
    Task<IEnumerable<ShipFitting>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update fitting usage statistics
    /// </summary>
    Task UpdateUsageStatsAsync(Guid fittingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get fitting with calculations
    /// </summary>
    Task<ShipFitting?> GetWithCalculationsAsync(Guid fittingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clone fitting
    /// </summary>
    Task<ShipFitting> CloneFittingAsync(Guid sourceFittingId, string newName, int? characterId = null, CancellationToken cancellationToken = default);
}