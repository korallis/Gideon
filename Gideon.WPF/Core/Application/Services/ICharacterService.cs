// ==========================================================================
// ICharacterService.cs - Character Service Interface
// ==========================================================================
// Service interface for character management and operations.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Gideon.WPF.Core.Domain.Entities;

namespace Gideon.WPF.Core.Application.Services;

/// <summary>
/// Service interface for character management operations
/// </summary>
public interface ICharacterService
{
    /// <summary>
    /// Get character by ID
    /// </summary>
    Task<Character?> GetCharacterAsync(int characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get character with full profile
    /// </summary>
    Task<Character?> GetCharacterWithFullProfileAsync(int characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active characters
    /// </summary>
    Task<IEnumerable<Character>> GetActiveCharactersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Update character data from ESI
    /// </summary>
    Task<Character> UpdateCharacterFromEsiAsync(long eveCharacterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add or update character
    /// </summary>
    Task<Character> SaveCharacterAsync(Character character, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete character
    /// </summary>
    Task DeleteCharacterAsync(int characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update character skills
    /// </summary>
    Task UpdateCharacterSkillsAsync(int characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update character assets
    /// </summary>
    Task UpdateCharacterAssetsAsync(int characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate character statistics
    /// </summary>
    Task<CharacterStatistic> CalculateCharacterStatisticsAsync(int characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get character skill points total
    /// </summary>
    Task<long> GetTotalSkillPointsAsync(int characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get character net worth
    /// </summary>
    Task<decimal> GetCharacterNetWorthAsync(int characterId, CancellationToken cancellationToken = default);
}