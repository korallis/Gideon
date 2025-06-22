// ==========================================================================
// CharacterRepository.cs - Character Repository Implementation
// ==========================================================================
// Concrete implementation of character repository with specialized operations.
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
/// Character repository implementation with specialized operations
/// </summary>
public class CharacterRepository : Repository<Character>, ICharacterRepository
{
    public CharacterRepository(GideonDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get character by EVE character ID
    /// </summary>
    public async Task<Character?> GetByCharacterIdAsync(long characterId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.CharacterId == characterId, cancellationToken);
    }

    /// <summary>
    /// Get character with skills loaded
    /// </summary>
    public async Task<Character?> GetWithSkillsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Skills)
                .ThenInclude(s => s.SkillType)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <summary>
    /// Get character with full profile (skills, assets, locations, etc.)
    /// </summary>
    public async Task<Character?> GetFullProfileAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Skills)
                .ThenInclude(s => s.SkillType)
            .Include(c => c.Assets)
                .ThenInclude(a => a.ItemType)
            .Include(c => c.Locations)
                .ThenInclude(l => l.SolarSystem)
                    .ThenInclude(s => s!.Region)
            .Include(c => c.SkillPlans)
                .ThenInclude(sp => sp.Entries)
            .Include(c => c.ShipFittings)
            .Include(c => c.Goals)
            .Include(c => c.Statistics)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <summary>
    /// Get characters by corporation
    /// </summary>
    public async Task<IEnumerable<Character>> GetByCorporationAsync(int corporationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.CorporationId == corporationId && c.IsActive)
            .OrderBy(c => c.CharacterName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get characters by alliance
    /// </summary>
    public async Task<IEnumerable<Character>> GetByAllianceAsync(int allianceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.AllianceId == allianceId && c.IsActive)
            .OrderBy(c => c.CharacterName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get active characters
    /// </summary>
    public async Task<IEnumerable<Character>> GetActiveCharactersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IsActive)
            .OrderBy(c => c.CharacterName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Update character last login
    /// </summary>
    public async Task UpdateLastLoginAsync(int characterId, DateTime loginTime, CancellationToken cancellationToken = default)
    {
        var character = await _dbSet.FindAsync(new object[] { characterId }, cancellationToken);
        if (character != null)
        {
            character.LastLogin = loginTime;
            character.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(character);
        }
    }

    /// <summary>
    /// Get character skill plan count
    /// </summary>
    public async Task<int> GetSkillPlanCountAsync(int characterId, CancellationToken cancellationToken = default)
    {
        return await _context.SkillPlans
            .Where(sp => sp.CharacterId == characterId && sp.IsActive)
            .CountAsync(cancellationToken);
    }

    /// <summary>
    /// Get character ship fitting count
    /// </summary>
    public async Task<int> GetShipFittingCountAsync(int characterId, CancellationToken cancellationToken = default)
    {
        return await _context.ShipFittings
            .Where(sf => sf.CharacterId == characterId && sf.IsActive)
            .CountAsync(cancellationToken);
    }
}