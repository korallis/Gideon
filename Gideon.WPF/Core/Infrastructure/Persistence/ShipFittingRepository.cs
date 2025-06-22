// ==========================================================================
// ShipFittingRepository.cs - Ship Fitting Repository Implementation
// ==========================================================================
// Concrete implementation of ship fitting repository with specialized operations.
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
/// Ship fitting repository implementation with specialized operations
/// </summary>
public class ShipFittingRepository : Repository<ShipFitting>, IShipFittingRepository
{
    public ShipFittingRepository(GideonDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get fittings by character
    /// </summary>
    public async Task<IEnumerable<ShipFitting>> GetByCharacterAsync(int characterId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Ship)
            .Where(f => f.CharacterId == characterId && f.IsActive)
            .OrderByDescending(f => f.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get fittings by ship type
    /// </summary>
    public async Task<IEnumerable<ShipFitting>> GetByShipTypeAsync(int shipId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Ship)
            .Include(f => f.Character)
            .Where(f => f.ShipId == shipId && f.IsActive)
            .OrderByDescending(f => f.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get public fittings
    /// </summary>
    public async Task<IEnumerable<ShipFitting>> GetPublicFittingsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Ship)
            .Include(f => f.Character)
            .Where(f => f.IsPublic && f.IsActive)
            .OrderByDescending(f => f.UseCount)
            .ThenByDescending(f => f.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get shared fittings
    /// </summary>
    public async Task<IEnumerable<ShipFitting>> GetSharedFittingsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Ship)
            .Include(f => f.Character)
            .Where(f => f.IsShared && f.IsActive)
            .OrderByDescending(f => f.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get fittings by category
    /// </summary>
    public async Task<IEnumerable<ShipFitting>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Ship)
            .Include(f => f.Character)
            .Where(f => f.FittingCategory == category && f.IsActive)
            .OrderByDescending(f => f.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get fittings by activity type
    /// </summary>
    public async Task<IEnumerable<ShipFitting>> GetByActivityTypeAsync(string activityType, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Ship)
            .Include(f => f.Character)
            .Where(f => f.ActivityType == activityType && f.IsActive)
            .OrderByDescending(f => f.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get favorite fittings for character
    /// </summary>
    public async Task<IEnumerable<ShipFitting>> GetFavoritesByCharacterAsync(int characterId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Ship)
            .Where(f => f.CharacterId == characterId && f.IsFavorite && f.IsActive)
            .OrderByDescending(f => f.LastUsed)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get recently used fittings
    /// </summary>
    public async Task<IEnumerable<ShipFitting>> GetRecentlyUsedAsync(int characterId, int count = 10, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Ship)
            .Where(f => f.CharacterId == characterId && f.LastUsed.HasValue && f.IsActive)
            .OrderByDescending(f => f.LastUsed)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Search fittings by name or description
    /// </summary>
    public async Task<IEnumerable<ShipFitting>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLower().Trim();
        return await _dbSet
            .Include(f => f.Ship)
            .Include(f => f.Character)
            .Where(f => f.IsActive && (
                f.Name.ToLower().Contains(term) ||
                (f.Description != null && f.Description.ToLower().Contains(term)) ||
                (f.Tags != null && f.Tags.ToLower().Contains(term))
            ))
            .OrderByDescending(f => f.UseCount)
            .ThenByDescending(f => f.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Update fitting usage statistics
    /// </summary>
    public async Task UpdateUsageStatsAsync(Guid fittingId, CancellationToken cancellationToken = default)
    {
        var fitting = await _dbSet.FindAsync(new object[] { fittingId }, cancellationToken);
        if (fitting != null)
        {
            fitting.UseCount++;
            fitting.LastUsed = DateTime.UtcNow;
            fitting.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(fitting);
        }
    }

    /// <summary>
    /// Get fitting with calculations
    /// </summary>
    public async Task<ShipFitting?> GetWithCalculationsAsync(Guid fittingId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Ship)
            .Include(f => f.Character)
            .FirstOrDefaultAsync(f => f.Id == fittingId, cancellationToken);
    }

    /// <summary>
    /// Clone fitting
    /// </summary>
    public async Task<ShipFitting> CloneFittingAsync(Guid sourceFittingId, string newName, int? characterId = null, CancellationToken cancellationToken = default)
    {
        var sourceFitting = await _dbSet.FindAsync(new object[] { sourceFittingId }, cancellationToken);
        if (sourceFitting == null)
            throw new ArgumentException($"Source fitting {sourceFittingId} not found");

        var clonedFitting = new ShipFitting
        {
            Id = Guid.NewGuid(),
            Name = newName,
            Description = $"Cloned from: {sourceFitting.Name}",
            ShipId = sourceFitting.ShipId,
            CharacterId = characterId ?? sourceFitting.CharacterId,
            HighSlotConfiguration = sourceFitting.HighSlotConfiguration,
            MidSlotConfiguration = sourceFitting.MidSlotConfiguration,
            LowSlotConfiguration = sourceFitting.LowSlotConfiguration,
            RigSlotConfiguration = sourceFitting.RigSlotConfiguration,
            SubsystemConfiguration = sourceFitting.SubsystemConfiguration,
            DroneConfiguration = sourceFitting.DroneConfiguration,
            CargoConfiguration = sourceFitting.CargoConfiguration,
            FittingCategory = sourceFitting.FittingCategory,
            ActivityType = sourceFitting.ActivityType,
            Tags = sourceFitting.Tags,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            ParentFittingId = sourceFittingId
        };

        await _dbSet.AddAsync(clonedFitting, cancellationToken);
        return clonedFitting;
    }
}