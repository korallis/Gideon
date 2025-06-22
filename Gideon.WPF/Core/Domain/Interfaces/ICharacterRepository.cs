// ==========================================================================
// ICharacterRepository.cs - Character Repository Interface
// ==========================================================================
// Specialized repository interface for character-specific operations.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Gideon.WPF.Core.Domain.Entities;

namespace Gideon.WPF.Core.Domain.Interfaces;

/// <summary>
/// Repository interface for character-specific operations
/// </summary>
public interface ICharacterRepository : IRepository<Character>
{
    /// <summary>
    /// Get character by EVE character ID
    /// </summary>
    Task<Character?> GetByCharacterIdAsync(long characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get character with skills loaded
    /// </summary>
    Task<Character?> GetWithSkillsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get character with full profile (skills, assets, locations, etc.)
    /// </summary>
    Task<Character?> GetFullProfileAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get characters by corporation
    /// </summary>
    Task<IEnumerable<Character>> GetByCorporationAsync(int corporationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get characters by alliance
    /// </summary>
    Task<IEnumerable<Character>> GetByAllianceAsync(int allianceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active characters
    /// </summary>
    Task<IEnumerable<Character>> GetActiveCharactersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Update character last login
    /// </summary>
    Task UpdateLastLoginAsync(int characterId, DateTime loginTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get character skill plan count
    /// </summary>
    Task<int> GetSkillPlanCountAsync(int characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get character ship fitting count
    /// </summary>
    Task<int> GetShipFittingCountAsync(int characterId, CancellationToken cancellationToken = default);
}