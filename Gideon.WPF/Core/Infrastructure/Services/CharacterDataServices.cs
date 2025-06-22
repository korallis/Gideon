// ==========================================================================
// CharacterDataServices.cs - Character Data Synchronization Services
// ==========================================================================
// Comprehensive character data retrieval and synchronization from ESI API.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.Extensions.Logging;
using Gideon.WPF.Core.Application.Services;
using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Core.Domain.Interfaces;
using System.Collections.Concurrent;

namespace Gideon.WPF.Core.Infrastructure.Services;

#region Character Data Synchronization Services

/// <summary>
/// Comprehensive character skill data retrieval service
/// </summary>
public class CharacterSkillDataService : ICharacterSkillDataService
{
    private readonly IEsiApiService _esiApiService;
    private readonly IEsiCharacterService _esiCharacterService;
    private readonly IEsiSkillsService _esiSkillsService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CharacterSkillDataService> _logger;
    private readonly IAuditLogService _auditService;
    private readonly IEsiDegradationService _degradationService;
    
    private readonly ConcurrentDictionary<long, DateTime> _lastSyncTimes = new();

    public CharacterSkillDataService(
        IEsiApiService esiApiService,
        IEsiCharacterService esiCharacterService,
        IEsiSkillsService esiSkillsService,
        IUnitOfWork unitOfWork,
        ILogger<CharacterSkillDataService> logger,
        IAuditLogService auditService,
        IEsiDegradationService degradationService)
    {
        _esiApiService = esiApiService ?? throw new ArgumentNullException(nameof(esiApiService));
        _esiCharacterService = esiCharacterService ?? throw new ArgumentNullException(nameof(esiCharacterService));
        _esiSkillsService = esiSkillsService ?? throw new ArgumentNullException(nameof(esiSkillsService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _degradationService = degradationService ?? throw new ArgumentNullException(nameof(degradationService));
    }

    /// <summary>
    /// Retrieve complete character skill data from ESI
    /// </summary>
    public async Task<CharacterSkillData> GetCompleteSkillDataAsync(long characterId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting complete skill data retrieval for character {CharacterId}", characterId);
            
            var skillData = new CharacterSkillData
            {
                CharacterId = characterId,
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.InProgress
            };
            
            // Use degradation service for fallback to cached data
            var result = await _degradationService.ExecuteWithFallbackAsync(
                // ESI call
                async (ct) => await RetrieveFromEsiAsync(characterId, accessToken, ct),
                // Fallback call
                async (ct) => await RetrieveFromCacheAsync(characterId, ct),
                $"character_skills_{characterId}",
                cancellationToken);
            
            if (result != null)
            {
                skillData = result;
                skillData.SyncStatus = SyncStatus.Success;
                
                // Store in database
                await StoreSkillDataAsync(skillData, cancellationToken);
                
                _lastSyncTimes[characterId] = DateTime.UtcNow;
                
                await _auditService.LogActionAsync("character_skills_synced", "Character", 
                    characterId.ToString(), cancellationToken);
            }
            else
            {
                skillData.SyncStatus = SyncStatus.Failed;
                skillData.ErrorMessage = "Failed to retrieve skill data from both ESI and cache";
            }
            
            return skillData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving skill data for character {CharacterId}", characterId);
            
            return new CharacterSkillData
            {
                CharacterId = characterId,
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Get character skill queue from ESI
    /// </summary>
    public async Task<IEnumerable<SkillQueueEntry>> GetSkillQueueAsync(long characterId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var skillQueue = await _esiApiService.GetAsync<List<EsiSkillQueueEntry>>(
                $"/latest/characters/{characterId}/skillqueue/",
                accessToken,
                cancellationToken);
            
            if (skillQueue == null)
                return Enumerable.Empty<SkillQueueEntry>();
            
            var queueEntries = skillQueue.Select(entry => new SkillQueueEntry
            {
                CharacterId = (int)characterId,
                SkillId = entry.skill_id,
                QueuePosition = entry.queue_position,
                FinishedLevel = entry.finished_level,
                StartDate = entry.start_date,
                FinishDate = entry.finish_date,
                TrainingStartSp = entry.training_start_sp,
                LevelStartSp = entry.level_start_sp,
                LevelEndSp = entry.level_end_sp
            }).ToList();
            
            _logger.LogDebug("Retrieved {QueueCount} skill queue entries for character {CharacterId}", 
                queueEntries.Count, characterId);
            
            return queueEntries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving skill queue for character {CharacterId}", characterId);
            return Enumerable.Empty<SkillQueueEntry>();
        }
    }

    /// <summary>
    /// Get character attributes from ESI
    /// </summary>
    public async Task<CharacterAttributes> GetCharacterAttributesAsync(long characterId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var attributes = await _esiApiService.GetAsync<EsiCharacterAttributes>(
                $"/latest/characters/{characterId}/attributes/",
                accessToken,
                cancellationToken);
            
            if (attributes == null)
            {
                _logger.LogWarning("No attributes found for character {CharacterId}", characterId);
                return new CharacterAttributes { CharacterId = (int)characterId };
            }
            
            var characterAttributes = new CharacterAttributes
            {
                CharacterId = (int)characterId,
                Intelligence = attributes.intelligence,
                Memory = attributes.memory,
                Charisma = attributes.charisma,
                Perception = attributes.perception,
                Willpower = attributes.willpower,
                BonusRemaps = attributes.bonus_remaps,
                LastRemapDate = attributes.last_remap_date,
                AccruedRemapCooldownDate = attributes.accrued_remap_cooldown_date
            };
            
            _logger.LogDebug("Retrieved attributes for character {CharacterId}", characterId);
            return characterAttributes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attributes for character {CharacterId}", characterId);
            return new CharacterAttributes { CharacterId = (int)characterId };
        }
    }

    /// <summary>
    /// Get character implants from ESI
    /// </summary>
    public async Task<IEnumerable<CharacterImplant>> GetCharacterImplantsAsync(long characterId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var implants = await _esiApiService.GetAsync<List<int>>(
                $"/latest/characters/{characterId}/implants/",
                accessToken,
                cancellationToken);
            
            if (implants == null)
                return Enumerable.Empty<CharacterImplant>();
            
            var characterImplants = implants.Select(implantId => new CharacterImplant
            {
                CharacterId = (int)characterId,
                ImplantTypeId = implantId,
                InstalledDate = DateTime.UtcNow // ESI doesn't provide this, so we use current time
            }).ToList();
            
            _logger.LogDebug("Retrieved {ImplantCount} implants for character {CharacterId}", 
                characterImplants.Count, characterId);
            
            return characterImplants;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving implants for character {CharacterId}", characterId);
            return Enumerable.Empty<CharacterImplant>();
        }
    }

    /// <summary>
    /// Synchronize all skill-related data for a character
    /// </summary>
    public async Task<bool> SynchronizeAllSkillDataAsync(long characterId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting complete skill data synchronization for character {CharacterId}", characterId);
            
            // Get all skill-related data in parallel
            var skillDataTask = GetCompleteSkillDataAsync(characterId, accessToken, cancellationToken);
            var skillQueueTask = GetSkillQueueAsync(characterId, accessToken, cancellationToken);
            var attributesTask = GetCharacterAttributesAsync(characterId, accessToken, cancellationToken);
            var implantsTask = GetCharacterImplantsAsync(characterId, accessToken, cancellationToken);
            
            await Task.WhenAll(skillDataTask, skillQueueTask, attributesTask, implantsTask);
            
            var skillData = await skillDataTask;
            var skillQueue = await skillQueueTask;
            var attributes = await attributesTask;
            var implants = await implantsTask;
            
            // Store all data in database
            using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            
            try
            {
                // Update skill queue
                await UpdateSkillQueueAsync(characterId, skillQueue, cancellationToken);
                
                // Update attributes
                await UpdateCharacterAttributesAsync(attributes, cancellationToken);
                
                // Update implants
                await UpdateCharacterImplantsAsync(characterId, implants, cancellationToken);
                
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                
                _logger.LogInformation("Successfully synchronized all skill data for character {CharacterId}", characterId);
                
                await _auditService.LogActionAsync("character_skill_sync_complete", "Character", 
                    characterId.ToString(), cancellationToken);
                
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error saving skill synchronization data for character {CharacterId}", characterId);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing skill data for character {CharacterId}", characterId);
            return false;
        }
    }

    /// <summary>
    /// Check if character skill data needs synchronization
    /// </summary>
    public async Task<bool> NeedsSynchronizationAsync(long characterId, TimeSpan maxAge, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_lastSyncTimes.TryGetValue(characterId, out var lastSync))
            {
                return true; // Never synchronized
            }
            
            return DateTime.UtcNow - lastSync > maxAge;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking synchronization status for character {CharacterId}", characterId);
            return true; // Err on the side of synchronization
        }
    }

    #region Private Methods

    private async Task<CharacterSkillData> RetrieveFromEsiAsync(long characterId, string accessToken, CancellationToken cancellationToken)
    {
        var skills = await _esiSkillsService.GetCharacterSkillsAsync(characterId, accessToken, cancellationToken);
        var skillPoints = await GetTotalSkillPointsAsync(characterId, accessToken, cancellationToken);
        var unallocatedSp = await GetUnallocatedSkillPointsAsync(characterId, accessToken, cancellationToken);
        
        return new CharacterSkillData
        {
            CharacterId = characterId,
            Skills = skills.ToList(),
            TotalSkillPoints = skillPoints,
            UnallocatedSkillPoints = unallocatedSp,
            LastUpdated = DateTime.UtcNow,
            SyncStatus = SyncStatus.Success,
            DataSource = DataSource.ESI
        };
    }

    private async Task<CharacterSkillData> RetrieveFromCacheAsync(long characterId, CancellationToken cancellationToken)
    {
        var character = await _unitOfWork.Characters.GetByIdAsync((int)characterId, cancellationToken);
        if (character == null)
        {
            return new CharacterSkillData { CharacterId = characterId, SyncStatus = SyncStatus.Failed };
        }
        
        var skills = await _unitOfWork.CharacterSkills.FindAsync(s => s.CharacterId == (int)characterId, cancellationToken);
        
        return new CharacterSkillData
        {
            CharacterId = characterId,
            Skills = skills.ToList(),
            TotalSkillPoints = character.TotalSp,
            UnallocatedSkillPoints = 0, // Would need to be cached separately
            LastUpdated = character.LastLoginDate ?? DateTime.MinValue,
            SyncStatus = SyncStatus.Success,
            DataSource = DataSource.Cache
        };
    }

    private async Task<long> GetTotalSkillPointsAsync(long characterId, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            var skillsResponse = await _esiApiService.GetAsync<EsiSkillsResponse>(
                $"/latest/characters/{characterId}/skills/",
                accessToken,
                cancellationToken);
            
            return skillsResponse?.total_sp ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving total skill points for character {CharacterId}", characterId);
            return 0;
        }
    }

    private async Task<long> GetUnallocatedSkillPointsAsync(long characterId, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            var skillsResponse = await _esiApiService.GetAsync<EsiSkillsResponse>(
                $"/latest/characters/{characterId}/skills/",
                accessToken,
                cancellationToken);
            
            return skillsResponse?.unallocated_sp ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unallocated skill points for character {CharacterId}", characterId);
            return 0;
        }
    }

    private async Task StoreSkillDataAsync(CharacterSkillData skillData, CancellationToken cancellationToken)
    {
        try
        {
            // Update character total SP
            var character = await _unitOfWork.Characters.GetByIdAsync((int)skillData.CharacterId, cancellationToken);
            if (character != null)
            {
                character.TotalSp = skillData.TotalSkillPoints;
                character.LastSkillUpdate = skillData.LastUpdated;
                await _unitOfWork.Characters.UpdateAsync(character, cancellationToken);
            }
            
            // Update individual skills
            foreach (var skill in skillData.Skills)
            {
                var existingSkill = await _unitOfWork.CharacterSkills.FindFirstAsync(
                    s => s.CharacterId == skill.CharacterId && s.SkillId == skill.SkillId,
                    cancellationToken);
                
                if (existingSkill == null)
                {
                    await _unitOfWork.CharacterSkills.AddAsync(skill, cancellationToken);
                }
                else
                {
                    existingSkill.ActiveSkillLevel = skill.ActiveSkillLevel;
                    existingSkill.TrainedSkillLevel = skill.TrainedSkillLevel;
                    existingSkill.SkillPointsInSkill = skill.SkillPointsInSkill;
                    await _unitOfWork.CharacterSkills.UpdateAsync(existingSkill, cancellationToken);
                }
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing skill data for character {CharacterId}", skillData.CharacterId);
            throw;
        }
    }

    private async Task UpdateSkillQueueAsync(long characterId, IEnumerable<SkillQueueEntry> skillQueue, CancellationToken cancellationToken)
    {
        try
        {
            // Remove existing queue entries
            var existingEntries = await _unitOfWork.SkillQueueEntries.FindAsync(
                e => e.CharacterId == (int)characterId, cancellationToken);
            
            foreach (var entry in existingEntries)
            {
                await _unitOfWork.SkillQueueEntries.RemoveAsync(entry, cancellationToken);
            }
            
            // Add new queue entries
            foreach (var entry in skillQueue)
            {
                await _unitOfWork.SkillQueueEntries.AddAsync(entry, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating skill queue for character {CharacterId}", characterId);
            throw;
        }
    }

    private async Task UpdateCharacterAttributesAsync(CharacterAttributes attributes, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _unitOfWork.CharacterAttributes.FindFirstAsync(
                a => a.CharacterId == attributes.CharacterId, cancellationToken);
            
            if (existing == null)
            {
                await _unitOfWork.CharacterAttributes.AddAsync(attributes, cancellationToken);
            }
            else
            {
                existing.Intelligence = attributes.Intelligence;
                existing.Memory = attributes.Memory;
                existing.Charisma = attributes.Charisma;
                existing.Perception = attributes.Perception;
                existing.Willpower = attributes.Willpower;
                existing.BonusRemaps = attributes.BonusRemaps;
                existing.LastRemapDate = attributes.LastRemapDate;
                existing.AccruedRemapCooldownDate = attributes.AccruedRemapCooldownDate;
                
                await _unitOfWork.CharacterAttributes.UpdateAsync(existing, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating attributes for character {CharacterId}", attributes.CharacterId);
            throw;
        }
    }

    private async Task UpdateCharacterImplantsAsync(long characterId, IEnumerable<CharacterImplant> implants, CancellationToken cancellationToken)
    {
        try
        {
            // Remove existing implants
            var existingImplants = await _unitOfWork.CharacterImplants.FindAsync(
                i => i.CharacterId == (int)characterId, cancellationToken);
            
            foreach (var implant in existingImplants)
            {
                await _unitOfWork.CharacterImplants.RemoveAsync(implant, cancellationToken);
            }
            
            // Add new implants
            foreach (var implant in implants)
            {
                await _unitOfWork.CharacterImplants.AddAsync(implant, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating implants for character {CharacterId}", characterId);
            throw;
        }
    }

    #endregion
}

/// <summary>
/// Character asset management service with location tracking
/// </summary>
public class CharacterAssetService : ICharacterAssetService
{
    private readonly IEsiApiService _esiApiService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CharacterAssetService> _logger;
    private readonly IAuditLogService _auditService;
    private readonly IEsiDegradationService _degradationService;
    
    private readonly ConcurrentDictionary<long, DateTime> _lastAssetSyncTimes = new();

    public CharacterAssetService(
        IEsiApiService esiApiService,
        IUnitOfWork unitOfWork,
        ILogger<CharacterAssetService> logger,
        IAuditLogService auditService,
        IEsiDegradationService degradationService)
    {
        _esiApiService = esiApiService ?? throw new ArgumentNullException(nameof(esiApiService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _degradationService = degradationService ?? throw new ArgumentNullException(nameof(degradationService));
    }

    /// <summary>
    /// Get all character assets from ESI with location tracking
    /// </summary>
    public async Task<CharacterAssetData> GetCharacterAssetsAsync(long characterId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting asset retrieval for character {CharacterId}", characterId);
            
            var result = await _degradationService.ExecuteWithFallbackAsync(
                // ESI call
                async (ct) => await RetrieveAssetsFromEsiAsync(characterId, accessToken, ct),
                // Fallback call
                async (ct) => await RetrieveAssetsFromCacheAsync(characterId, ct),
                $"character_assets_{characterId}",
                cancellationToken);
            
            if (result != null)
            {
                await StoreAssetDataAsync(result, cancellationToken);
                _lastAssetSyncTimes[characterId] = DateTime.UtcNow;
                
                await _auditService.LogActionAsync("character_assets_synced", "Character", 
                    characterId.ToString(), cancellationToken);
            }
            
            return result ?? new CharacterAssetData
            {
                CharacterId = characterId,
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = "Failed to retrieve assets from both ESI and cache"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assets for character {CharacterId}", characterId);
            
            return new CharacterAssetData
            {
                CharacterId = characterId,
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Get character asset locations from ESI
    /// </summary>
    public async Task<IEnumerable<AssetLocation>> GetAssetLocationsAsync(long characterId, string accessToken, IEnumerable<long> assetIds, CancellationToken cancellationToken = default)
    {
        try
        {
            var assetIdList = assetIds.ToList();
            if (!assetIdList.Any())
                return Enumerable.Empty<AssetLocation>();
            
            // ESI has a limit on how many asset IDs can be requested at once
            const int batchSize = 1000;
            var locations = new List<AssetLocation>();
            
            for (int i = 0; i < assetIdList.Count; i += batchSize)
            {
                var batch = assetIdList.Skip(i).Take(batchSize).ToList();
                
                var batchLocations = await _esiApiService.PostAsync<List<EsiAssetLocation>>(
                    $"/latest/characters/{characterId}/assets/locations/",
                    batch,
                    accessToken,
                    cancellationToken);
                
                if (batchLocations != null)
                {
                    locations.AddRange(batchLocations.Select(loc => new AssetLocation
                    {
                        ItemId = loc.item_id,
                        Position = new AssetPosition
                        {
                            X = loc.position.x,
                            Y = loc.position.y,
                            Z = loc.position.z
                        }
                    }));
                }
            }
            
            _logger.LogDebug("Retrieved locations for {LocationCount} assets for character {CharacterId}", 
                locations.Count, characterId);
            
            return locations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving asset locations for character {CharacterId}", characterId);
            return Enumerable.Empty<AssetLocation>();
        }
    }

    /// <summary>
    /// Get character asset names for items that have custom names
    /// </summary>
    public async Task<IEnumerable<AssetName>> GetAssetNamesAsync(long characterId, string accessToken, IEnumerable<long> assetIds, CancellationToken cancellationToken = default)
    {
        try
        {
            var assetIdList = assetIds.ToList();
            if (!assetIdList.Any())
                return Enumerable.Empty<AssetName>();
            
            const int batchSize = 1000;
            var names = new List<AssetName>();
            
            for (int i = 0; i < assetIdList.Count; i += batchSize)
            {
                var batch = assetIdList.Skip(i).Take(batchSize).ToList();
                
                var batchNames = await _esiApiService.PostAsync<List<EsiAssetName>>(
                    $"/latest/characters/{characterId}/assets/names/",
                    batch,
                    accessToken,
                    cancellationToken);
                
                if (batchNames != null)
                {
                    names.AddRange(batchNames.Select(name => new AssetName
                    {
                        ItemId = name.item_id,
                        Name = name.name
                    }));
                }
            }
            
            _logger.LogDebug("Retrieved names for {NameCount} assets for character {CharacterId}", 
                names.Count, characterId);
            
            return names;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving asset names for character {CharacterId}", characterId);
            return Enumerable.Empty<AssetName>();
        }
    }

    /// <summary>
    /// Synchronize all asset data including locations and names
    /// </summary>
    public async Task<bool> SynchronizeAllAssetDataAsync(long characterId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting complete asset synchronization for character {CharacterId}", characterId);
            
            // Get base asset data
            var assetData = await GetCharacterAssetsAsync(characterId, accessToken, cancellationToken);
            if (assetData.SyncStatus != SyncStatus.Success)
            {
                return false;
            }
            
            // Get asset IDs for location and name lookup
            var assetIds = assetData.Assets
                .Where(a => a.IsLocationFlag && a.TypeId > 0)
                .Select(a => a.ItemId)
                .ToList();
            
            // Get locations and names in parallel
            var locationsTask = GetAssetLocationsAsync(characterId, accessToken, assetIds, cancellationToken);
            var namesTask = GetAssetNamesAsync(characterId, accessToken, assetIds, cancellationToken);
            
            await Task.WhenAll(locationsTask, namesTask);
            
            var locations = await locationsTask;
            var names = await namesTask;
            
            // Store additional data
            using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            
            try
            {
                await StoreAssetLocationsAsync(characterId, locations, cancellationToken);
                await StoreAssetNamesAsync(characterId, names, cancellationToken);
                
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                
                _logger.LogInformation("Successfully synchronized all asset data for character {CharacterId}", characterId);
                
                await _auditService.LogActionAsync("character_asset_sync_complete", "Character", 
                    characterId.ToString(), cancellationToken);
                
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error saving asset synchronization data for character {CharacterId}", characterId);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing asset data for character {CharacterId}", characterId);
            return false;
        }
    }

    /// <summary>
    /// Get asset summary statistics for a character
    /// </summary>
    public async Task<AssetSummary> GetAssetSummaryAsync(long characterId, CancellationToken cancellationToken = default)
    {
        try
        {
            var assets = await _unitOfWork.CharacterAssets.FindAsync(
                a => a.CharacterId == (int)characterId, cancellationToken);
            
            var summary = new AssetSummary
            {
                CharacterId = characterId,
                TotalAssets = assets.Count(),
                TotalValue = assets.Sum(a => a.EstimatedValue ?? 0),
                LastUpdated = assets.Max(a => a.LastUpdated) ?? DateTime.MinValue
            };
            
            // Group by location
            summary.LocationBreakdown = assets
                .GroupBy(a => a.LocationId)
                .ToDictionary(g => g.Key, g => g.Count());
            
            // Group by type
            summary.TypeBreakdown = assets
                .GroupBy(a => a.TypeId)
                .ToDictionary(g => g.Key, g => new AssetTypeInfo
                {
                    Count = g.Sum(a => a.Quantity),
                    Value = g.Sum(a => a.EstimatedValue ?? 0)
                });
            
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating asset summary for character {CharacterId}", characterId);
            
            return new AssetSummary
            {
                CharacterId = characterId,
                ErrorMessage = ex.Message
            };
        }
    }

    #region Private Methods

    private async Task<CharacterAssetData> RetrieveAssetsFromEsiAsync(long characterId, string accessToken, CancellationToken cancellationToken)
    {
        var assets = await _esiApiService.GetAsync<List<EsiAsset>>(
            $"/latest/characters/{characterId}/assets/",
            accessToken,
            cancellationToken);
        
        if (assets == null)
        {
            return new CharacterAssetData
            {
                CharacterId = characterId,
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = "No asset data received from ESI"
            };
        }
        
        var characterAssets = assets.Select(asset => new CharacterAsset
        {
            CharacterId = (int)characterId,
            ItemId = asset.item_id,
            TypeId = asset.type_id,
            LocationId = asset.location_id,
            LocationType = MapLocationFlag(asset.location_flag),
            LocationFlag = asset.location_flag,
            Quantity = asset.quantity,
            IsSingleton = asset.is_singleton,
            LastUpdated = DateTime.UtcNow,
            IsLocationFlag = IsLocationFlag(asset.location_flag)
        }).ToList();
        
        return new CharacterAssetData
        {
            CharacterId = characterId,
            Assets = characterAssets,
            TotalAssets = characterAssets.Count,
            LastUpdated = DateTime.UtcNow,
            SyncStatus = SyncStatus.Success,
            DataSource = DataSource.ESI
        };
    }

    private async Task<CharacterAssetData> RetrieveAssetsFromCacheAsync(long characterId, CancellationToken cancellationToken)
    {
        var assets = await _unitOfWork.CharacterAssets.FindAsync(
            a => a.CharacterId == (int)characterId, cancellationToken);
        
        return new CharacterAssetData
        {
            CharacterId = characterId,
            Assets = assets.ToList(),
            TotalAssets = assets.Count(),
            LastUpdated = assets.Max(a => a.LastUpdated) ?? DateTime.MinValue,
            SyncStatus = SyncStatus.Success,
            DataSource = DataSource.Cache
        };
    }

    private async Task StoreAssetDataAsync(CharacterAssetData assetData, CancellationToken cancellationToken)
    {
        try
        {
            // Remove existing assets
            var existingAssets = await _unitOfWork.CharacterAssets.FindAsync(
                a => a.CharacterId == (int)assetData.CharacterId, cancellationToken);
            
            foreach (var asset in existingAssets)
            {
                await _unitOfWork.CharacterAssets.RemoveAsync(asset, cancellationToken);
            }
            
            // Add new assets
            foreach (var asset in assetData.Assets)
            {
                await _unitOfWork.CharacterAssets.AddAsync(asset, cancellationToken);
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing asset data for character {CharacterId}", assetData.CharacterId);
            throw;
        }
    }

    private async Task StoreAssetLocationsAsync(long characterId, IEnumerable<AssetLocation> locations, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var location in locations)
            {
                var asset = await _unitOfWork.CharacterAssets.FindFirstAsync(
                    a => a.CharacterId == (int)characterId && a.ItemId == location.ItemId,
                    cancellationToken);
                
                if (asset != null)
                {
                    asset.PositionX = location.Position.X;
                    asset.PositionY = location.Position.Y;
                    asset.PositionZ = location.Position.Z;
                    await _unitOfWork.CharacterAssets.UpdateAsync(asset, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing asset locations for character {CharacterId}", characterId);
            throw;
        }
    }

    private async Task StoreAssetNamesAsync(long characterId, IEnumerable<AssetName> names, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var name in names)
            {
                var asset = await _unitOfWork.CharacterAssets.FindFirstAsync(
                    a => a.CharacterId == (int)characterId && a.ItemId == name.ItemId,
                    cancellationToken);
                
                if (asset != null)
                {
                    asset.CustomName = name.Name;
                    await _unitOfWork.CharacterAssets.UpdateAsync(asset, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing asset names for character {CharacterId}", characterId);
            throw;
        }
    }

    private static AssetLocationType MapLocationFlag(string locationFlag)
    {
        return locationFlag.ToLowerInvariant() switch
        {
            "hangar" => AssetLocationType.Hangar,
            "cargo" => AssetLocationType.Cargo,
            "dronebay" => AssetLocationType.DroneBay,
            "shiphangar" => AssetLocationType.ShipHangar,
            "skillqueue" => AssetLocationType.SkillQueue,
            "wallet" => AssetLocationType.Wallet,
            "wardrobe" => AssetLocationType.Wardrobe,
            "deliveries" => AssetLocationType.Deliveries,
            _ => AssetLocationType.Unknown
        };
    }

    private static bool IsLocationFlag(string locationFlag)
    {
        var locationFlags = new[] { "Hangar", "Cargo", "DroneBay", "ShipHangar" };
        return locationFlags.Contains(locationFlag, StringComparer.OrdinalIgnoreCase);
    }

    #endregion
}

#endregion

#region Supporting Data Structures

/// <summary>
/// Character skill data container
/// </summary>
public class CharacterSkillData
{
    public long CharacterId { get; set; }
    public List<CharacterSkill> Skills { get; set; } = new();
    public long TotalSkillPoints { get; set; }
    public long UnallocatedSkillPoints { get; set; }
    public DateTime LastUpdated { get; set; }
    public SyncStatus SyncStatus { get; set; }
    public DataSource DataSource { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// ESI skills response structure
/// </summary>
public class EsiSkillsResponse
{
    public List<EsiSkill> skills { get; set; } = new();
    public long total_sp { get; set; }
    public long unallocated_sp { get; set; }
}

/// <summary>
/// ESI skill queue entry
/// </summary>
public class EsiSkillQueueEntry
{
    public int skill_id { get; set; }
    public int queue_position { get; set; }
    public int finished_level { get; set; }
    public DateTime? start_date { get; set; }
    public DateTime? finish_date { get; set; }
    public int training_start_sp { get; set; }
    public int level_start_sp { get; set; }
    public int level_end_sp { get; set; }
}

/// <summary>
/// ESI character attributes
/// </summary>
public class EsiCharacterAttributes
{
    public int intelligence { get; set; }
    public int memory { get; set; }
    public int charisma { get; set; }
    public int perception { get; set; }
    public int willpower { get; set; }
    public int bonus_remaps { get; set; }
    public DateTime? last_remap_date { get; set; }
    public DateTime? accrued_remap_cooldown_date { get; set; }
}

/// <summary>
/// Synchronization status
/// </summary>
public enum SyncStatus
{
    None,
    InProgress,
    Success,
    Failed,
    PartialSuccess
}

/// <summary>
/// Data source type
/// </summary>
public enum DataSource
{
    Unknown,
    ESI,
    Cache,
    Manual
}

/// <summary>
/// Character skill data service interface
/// </summary>
public interface ICharacterSkillDataService
{
    Task<CharacterSkillData> GetCompleteSkillDataAsync(long characterId, string accessToken, CancellationToken cancellationToken = default);
    Task<IEnumerable<SkillQueueEntry>> GetSkillQueueAsync(long characterId, string accessToken, CancellationToken cancellationToken = default);
    Task<CharacterAttributes> GetCharacterAttributesAsync(long characterId, string accessToken, CancellationToken cancellationToken = default);
    Task<IEnumerable<CharacterImplant>> GetCharacterImplantsAsync(long characterId, string accessToken, CancellationToken cancellationToken = default);
    Task<bool> SynchronizeAllSkillDataAsync(long characterId, string accessToken, CancellationToken cancellationToken = default);
    Task<bool> NeedsSynchronizationAsync(long characterId, TimeSpan maxAge, CancellationToken cancellationToken = default);
}

/// <summary>
/// Character asset service interface
/// </summary>
public interface ICharacterAssetService
{
    Task<CharacterAssetData> GetCharacterAssetsAsync(long characterId, string accessToken, CancellationToken cancellationToken = default);
    Task<IEnumerable<AssetLocation>> GetAssetLocationsAsync(long characterId, string accessToken, IEnumerable<long> assetIds, CancellationToken cancellationToken = default);
    Task<IEnumerable<AssetName>> GetAssetNamesAsync(long characterId, string accessToken, IEnumerable<long> assetIds, CancellationToken cancellationToken = default);
    Task<bool> SynchronizeAllAssetDataAsync(long characterId, string accessToken, CancellationToken cancellationToken = default);
    Task<AssetSummary> GetAssetSummaryAsync(long characterId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Character asset data container
/// </summary>
public class CharacterAssetData
{
    public long CharacterId { get; set; }
    public List<CharacterAsset> Assets { get; set; } = new();
    public int TotalAssets { get; set; }
    public DateTime LastUpdated { get; set; }
    public SyncStatus SyncStatus { get; set; }
    public DataSource DataSource { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Asset location information
/// </summary>
public class AssetLocation
{
    public long ItemId { get; set; }
    public AssetPosition Position { get; set; } = new();
}

/// <summary>
/// Asset position in 3D space
/// </summary>
public class AssetPosition
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
}

/// <summary>
/// Asset custom name
/// </summary>
public class AssetName
{
    public long ItemId { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Asset summary statistics
/// </summary>
public class AssetSummary
{
    public long CharacterId { get; set; }
    public int TotalAssets { get; set; }
    public decimal TotalValue { get; set; }
    public DateTime LastUpdated { get; set; }
    public Dictionary<long, int> LocationBreakdown { get; set; } = new();
    public Dictionary<int, AssetTypeInfo> TypeBreakdown { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Asset type information
/// </summary>
public class AssetTypeInfo
{
    public int Count { get; set; }
    public decimal Value { get; set; }
}

/// <summary>
/// Asset location types
/// </summary>
public enum AssetLocationType
{
    Unknown,
    Hangar,
    Cargo,
    DroneBay,
    ShipHangar,
    SkillQueue,
    Wallet,
    Wardrobe,
    Deliveries
}

/// <summary>
/// ESI asset response structure
/// </summary>
public class EsiAsset
{
    public long item_id { get; set; }
    public int type_id { get; set; }
    public long location_id { get; set; }
    public string location_flag { get; set; } = string.Empty;
    public int quantity { get; set; }
    public bool is_singleton { get; set; }
}

/// <summary>
/// ESI asset location response
/// </summary>
public class EsiAssetLocation
{
    public long item_id { get; set; }
    public EsiPosition position { get; set; } = new();
}

/// <summary>
/// ESI position structure
/// </summary>
public class EsiPosition
{
    public double x { get; set; }
    public double y { get; set; }
    public double z { get; set; }
}

/// <summary>
/// ESI asset name response
/// </summary>
public class EsiAssetName
{
    public long item_id { get; set; }
    public string name { get; set; } = string.Empty;
}

#endregion

#region Corporation and Alliance Data Services

/// <summary>
/// Corporation and alliance data synchronization service
/// </summary>
public class CorporationAllianceDataService : ICorporationAllianceDataService
{
    private readonly IEsiApiService _esiApiService;
    private readonly IEsiCharacterService _esiCharacterService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CorporationAllianceDataService> _logger;
    private readonly IAuditLogService _auditService;
    private readonly IEsiDegradationService _degradationService;
    private readonly ICacheService _cacheService;
    
    private readonly ConcurrentDictionary<int, DateTime> _corporationSyncTimes = new();
    private readonly ConcurrentDictionary<int, DateTime> _allianceSyncTimes = new();

    public CorporationAllianceDataService(
        IEsiApiService esiApiService,
        IEsiCharacterService esiCharacterService,
        IUnitOfWork unitOfWork,
        ILogger<CorporationAllianceDataService> logger,
        IAuditLogService auditService,
        IEsiDegradationService degradationService,
        ICacheService cacheService)
    {
        _esiApiService = esiApiService ?? throw new ArgumentNullException(nameof(esiApiService));
        _esiCharacterService = esiCharacterService ?? throw new ArgumentNullException(nameof(esiCharacterService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _degradationService = degradationService ?? throw new ArgumentNullException(nameof(degradationService));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    /// <summary>
    /// Synchronize corporation data for character
    /// </summary>
    public async Task<CorporationData> SynchronizeCorporationDataAsync(long characterId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting corporation data synchronization for character {CharacterId}", characterId);
            
            // Get character's corporation ID
            var character = await _esiCharacterService.GetCharacterPublicInfoAsync(characterId, cancellationToken);
            if (character?.CorporationId == null)
            {
                throw new InvalidOperationException($"Unable to retrieve corporation ID for character {characterId}");
            }
            
            var corporationId = character.CorporationId.Value;
            
            // Check if we need to update (once per hour)
            if (_corporationSyncTimes.TryGetValue(corporationId, out var lastSync) && 
                DateTime.UtcNow - lastSync < TimeSpan.FromHours(1))
            {
                var cached = await GetCachedCorporationDataAsync(corporationId, cancellationToken);
                if (cached != null)
                {
                    _logger.LogDebug("Using cached corporation data for {CorporationId}", corporationId);
                    return cached;
                }
            }
            
            var corporationData = await _degradationService.ExecuteWithFallbackAsync(
                async (ct) => await RetrieveCorporationFromEsiAsync(corporationId, accessToken, ct),
                async (ct) => await GetCachedCorporationDataAsync(corporationId, ct),
                $"corporation_data_{corporationId}",
                cancellationToken);
            
            if (corporationData != null)
            {
                corporationData.LastUpdated = DateTime.UtcNow;
                corporationData.SyncStatus = SyncStatus.Success;
                
                // Store in database
                await StoreCorporationDataAsync(corporationData, cancellationToken);
                
                _corporationSyncTimes[corporationId] = DateTime.UtcNow;
                
                await _auditService.LogActionAsync("corporation_data_synced", "Corporation", 
                    corporationId.ToString(), cancellationToken);
                
                _logger.LogInformation("Corporation data synchronized successfully for {CorporationId}: {CorporationName}", 
                    corporationId, corporationData.Name);
            }
            
            return corporationData ?? new CorporationData 
            { 
                CorporationId = corporationId, 
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = "Failed to retrieve corporation data"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing corporation data for character {CharacterId}", characterId);
            
            return new CorporationData 
            { 
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = ex.Message,
                LastUpdated = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Synchronize alliance data for corporation
    /// </summary>
    public async Task<AllianceData?> SynchronizeAllianceDataAsync(int corporationId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting alliance data synchronization for corporation {CorporationId}", corporationId);
            
            // Get corporation's alliance ID
            var corporation = await GetCachedCorporationDataAsync(corporationId, cancellationToken);
            if (corporation?.AllianceId == null)
            {
                _logger.LogDebug("Corporation {CorporationId} is not in an alliance", corporationId);
                return null;
            }
            
            var allianceId = corporation.AllianceId.Value;
            
            // Check if we need to update (once per 4 hours)
            if (_allianceSyncTimes.TryGetValue(allianceId, out var lastSync) && 
                DateTime.UtcNow - lastSync < TimeSpan.FromHours(4))
            {
                var cached = await GetCachedAllianceDataAsync(allianceId, cancellationToken);
                if (cached != null)
                {
                    _logger.LogDebug("Using cached alliance data for {AllianceId}", allianceId);
                    return cached;
                }
            }
            
            var allianceData = await _degradationService.ExecuteWithFallbackAsync(
                async (ct) => await RetrieveAllianceFromEsiAsync(allianceId, accessToken, ct),
                async (ct) => await GetCachedAllianceDataAsync(allianceId, ct),
                $"alliance_data_{allianceId}",
                cancellationToken);
            
            if (allianceData != null)
            {
                allianceData.LastUpdated = DateTime.UtcNow;
                allianceData.SyncStatus = SyncStatus.Success;
                
                // Store in database
                await StoreAllianceDataAsync(allianceData, cancellationToken);
                
                _allianceSyncTimes[allianceId] = DateTime.UtcNow;
                
                await _auditService.LogActionAsync("alliance_data_synced", "Alliance", 
                    allianceId.ToString(), cancellationToken);
                
                _logger.LogInformation("Alliance data synchronized successfully for {AllianceId}: {AllianceName}", 
                    allianceId, allianceData.Name);
            }
            
            return allianceData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing alliance data for corporation {CorporationId}", corporationId);
            
            return new AllianceData 
            { 
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = ex.Message,
                LastUpdated = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Get corporation members (requires director+ roles)
    /// </summary>
    public async Task<List<CorporationMember>> GetCorporationMembersAsync(int corporationId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving corporation members for {CorporationId}", corporationId);
            
            var members = await _degradationService.ExecuteWithFallbackAsync(
                async (ct) => await RetrieveMembersFromEsiAsync(corporationId, accessToken, ct),
                async (ct) => await GetCachedMembersAsync(corporationId, ct),
                $"corporation_members_{corporationId}",
                cancellationToken);
            
            if (members != null && members.Any())
            {
                await StoreCorporationMembersAsync(corporationId, members, cancellationToken);
                
                await _auditService.LogActionAsync("corporation_members_synced", "Corporation", 
                    corporationId.ToString(), cancellationToken);
                
                _logger.LogInformation("Retrieved {MemberCount} members for corporation {CorporationId}", 
                    members.Count, corporationId);
            }
            
            return members ?? new List<CorporationMember>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving corporation members for {CorporationId}", corporationId);
            return new List<CorporationMember>();
        }
    }

    /// <summary>
    /// Get corporation standings
    /// </summary>
    public async Task<List<CorporationStanding>> GetCorporationStandingsAsync(int corporationId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving corporation standings for {CorporationId}", corporationId);
            
            var standings = await _degradationService.ExecuteWithFallbackAsync(
                async (ct) => await RetrieveStandingsFromEsiAsync(corporationId, accessToken, ct),
                async (ct) => await GetCachedStandingsAsync(corporationId, ct),
                $"corporation_standings_{corporationId}",
                cancellationToken);
            
            if (standings != null && standings.Any())
            {
                await StoreCorporationStandingsAsync(corporationId, standings, cancellationToken);
                
                await _auditService.LogActionAsync("corporation_standings_synced", "Corporation", 
                    corporationId.ToString(), cancellationToken);
                
                _logger.LogInformation("Retrieved {StandingCount} standings for corporation {CorporationId}", 
                    standings.Count, corporationId);
            }
            
            return standings ?? new List<CorporationStanding>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving corporation standings for {CorporationId}", corporationId);
            return new List<CorporationStanding>();
        }
    }

    /// <summary>
    /// Get complete corporation and alliance summary
    /// </summary>
    public async Task<CorporationAllianceSummary> GetCompleteSummaryAsync(long characterId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving complete corporation/alliance summary for character {CharacterId}", characterId);
            
            var summary = new CorporationAllianceSummary
            {
                CharacterId = characterId,
                LastUpdated = DateTime.UtcNow
            };
            
            // Get corporation data
            summary.Corporation = await SynchronizeCorporationDataAsync(characterId, accessToken, cancellationToken);
            
            // Get alliance data if corporation is in alliance
            if (summary.Corporation?.AllianceId != null)
            {
                summary.Alliance = await SynchronizeAllianceDataAsync(summary.Corporation.CorporationId, accessToken, cancellationToken);
            }
            
            // Get corporation members (if permissions allow)
            try
            {
                summary.Members = await GetCorporationMembersAsync(summary.Corporation?.CorporationId ?? 0, accessToken, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Could not retrieve corporation members (likely insufficient permissions): {Error}", ex.Message);
                summary.Members = new List<CorporationMember>();
            }
            
            // Get corporation standings (if permissions allow)
            try
            {
                summary.Standings = await GetCorporationStandingsAsync(summary.Corporation?.CorporationId ?? 0, accessToken, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Could not retrieve corporation standings (likely insufficient permissions): {Error}", ex.Message);
                summary.Standings = new List<CorporationStanding>();
            }
            
            summary.SyncStatus = SyncStatus.Success;
            
            await _auditService.LogActionAsync("corporation_alliance_summary_retrieved", "Character", 
                characterId.ToString(), cancellationToken);
            
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving corporation/alliance summary for character {CharacterId}", characterId);
            
            return new CorporationAllianceSummary
            {
                CharacterId = characterId,
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = ex.Message,
                LastUpdated = DateTime.UtcNow
            };
        }
    }

    #region Private ESI Retrieval Methods

    private async Task<CorporationData?> RetrieveCorporationFromEsiAsync(int corporationId, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            // Get corporation info from ESI
            var corporationInfo = await _esiApiService.GetAsync<EsiCorporationInfo>(
                $"/latest/corporations/{corporationId}/", 
                accessToken, 
                cancellationToken);
            
            if (corporationInfo == null) return null;
            
            return new CorporationData
            {
                CorporationId = corporationId,
                Name = corporationInfo.Name,
                Ticker = corporationInfo.Ticker,
                Description = corporationInfo.Description,
                AllianceId = corporationInfo.AllianceId,
                CeoId = corporationInfo.CeoId,
                CreatorId = corporationInfo.CreatorId,
                DateFounded = corporationInfo.DateFounded,
                FactionId = corporationInfo.FactionId,
                HomeStationId = corporationInfo.HomeStationId,
                MemberCount = corporationInfo.MemberCount,
                Shares = corporationInfo.Shares,
                TaxRate = corporationInfo.TaxRate,
                Url = corporationInfo.Url,
                WarEligible = corporationInfo.WarEligible,
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.Success
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving corporation {CorporationId} from ESI", corporationId);
            return null;
        }
    }

    private async Task<AllianceData?> RetrieveAllianceFromEsiAsync(int allianceId, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            // Get alliance info from ESI
            var allianceInfo = await _esiApiService.GetAsync<EsiAllianceInfo>(
                $"/latest/alliances/{allianceId}/", 
                accessToken, 
                cancellationToken);
            
            if (allianceInfo == null) return null;
            
            return new AllianceData
            {
                AllianceId = allianceId,
                Name = allianceInfo.Name,
                Ticker = allianceInfo.Ticker,
                CreatorCorporationId = allianceInfo.CreatorCorporationId,
                CreatorId = allianceInfo.CreatorId,
                DateFounded = allianceInfo.DateFounded,
                ExecutorCorporationId = allianceInfo.ExecutorCorporationId,
                FactionId = allianceInfo.FactionId,
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.Success
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alliance {AllianceId} from ESI", allianceId);
            return null;
        }
    }

    private async Task<List<CorporationMember>?> RetrieveMembersFromEsiAsync(int corporationId, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            // Get corporation members from ESI (requires director+ roles)
            var members = await _esiApiService.GetAsync<List<EsiCorporationMember>>(
                $"/latest/corporations/{corporationId}/members/", 
                accessToken, 
                cancellationToken);
            
            if (members == null) return null;
            
            return members.Select(m => new CorporationMember
            {
                CharacterId = m.CharacterId,
                CorporationId = corporationId,
                JoinDate = m.StartDate,
                LastUpdated = DateTime.UtcNow
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving members for corporation {CorporationId} from ESI", corporationId);
            return null;
        }
    }

    private async Task<List<CorporationStanding>?> RetrieveStandingsFromEsiAsync(int corporationId, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            // Get corporation standings from ESI
            var standings = await _esiApiService.GetAsync<List<EsiCorporationStanding>>(
                $"/latest/corporations/{corporationId}/standings/", 
                accessToken, 
                cancellationToken);
            
            if (standings == null) return null;
            
            return standings.Select(s => new CorporationStanding
            {
                CorporationId = corporationId,
                FromId = s.FromId,
                FromType = s.FromType,
                Standing = s.Standing,
                LastUpdated = DateTime.UtcNow
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving standings for corporation {CorporationId} from ESI", corporationId);
            return null;
        }
    }

    #endregion

    #region Private Cache Methods

    private async Task<CorporationData?> GetCachedCorporationDataAsync(int corporationId, CancellationToken cancellationToken)
    {
        try
        {
            return await _cacheService.GetAsync<CorporationData>($"corp_data_{corporationId}", cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private async Task<AllianceData?> GetCachedAllianceDataAsync(int allianceId, CancellationToken cancellationToken)
    {
        try
        {
            return await _cacheService.GetAsync<AllianceData>($"alliance_data_{allianceId}", cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private async Task<List<CorporationMember>?> GetCachedMembersAsync(int corporationId, CancellationToken cancellationToken)
    {
        try
        {
            return await _cacheService.GetAsync<List<CorporationMember>>($"corp_members_{corporationId}", cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private async Task<List<CorporationStanding>?> GetCachedStandingsAsync(int corporationId, CancellationToken cancellationToken)
    {
        try
        {
            return await _cacheService.GetAsync<List<CorporationStanding>>($"corp_standings_{corporationId}", cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region Private Storage Methods

    private async Task StoreCorporationDataAsync(CorporationData corporationData, CancellationToken cancellationToken)
    {
        try
        {
            // Store in database
            var existing = await _unitOfWork.Corporations.GetByIdAsync(corporationData.CorporationId, cancellationToken);
            if (existing != null)
            {
                // Update existing
                existing.Name = corporationData.Name;
                existing.Ticker = corporationData.Ticker;
                existing.Description = corporationData.Description;
                existing.AllianceId = corporationData.AllianceId;
                existing.CeoId = corporationData.CeoId;
                existing.MemberCount = corporationData.MemberCount;
                existing.TaxRate = corporationData.TaxRate;
                existing.LastUpdated = DateTime.UtcNow;
                
                await _unitOfWork.Corporations.UpdateAsync(existing, cancellationToken);
            }
            else
            {
                // Create new
                await _unitOfWork.Corporations.AddAsync(corporationData, cancellationToken);
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            // Cache for 1 hour
            await _cacheService.SetAsync($"corp_data_{corporationData.CorporationId}", 
                corporationData, TimeSpan.FromHours(1), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing corporation data for {CorporationId}", corporationData.CorporationId);
        }
    }

    private async Task StoreAllianceDataAsync(AllianceData allianceData, CancellationToken cancellationToken)
    {
        try
        {
            // Store in database
            var existing = await _unitOfWork.Alliances.GetByIdAsync(allianceData.AllianceId, cancellationToken);
            if (existing != null)
            {
                // Update existing
                existing.Name = allianceData.Name;
                existing.Ticker = allianceData.Ticker;
                existing.ExecutorCorporationId = allianceData.ExecutorCorporationId;
                existing.LastUpdated = DateTime.UtcNow;
                
                await _unitOfWork.Alliances.UpdateAsync(existing, cancellationToken);
            }
            else
            {
                // Create new
                await _unitOfWork.Alliances.AddAsync(allianceData, cancellationToken);
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            // Cache for 4 hours
            await _cacheService.SetAsync($"alliance_data_{allianceData.AllianceId}", 
                allianceData, TimeSpan.FromHours(4), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing alliance data for {AllianceId}", allianceData.AllianceId);
        }
    }

    private async Task StoreCorporationMembersAsync(int corporationId, List<CorporationMember> members, CancellationToken cancellationToken)
    {
        try
        {
            // Clear existing members
            var existingMembers = await _unitOfWork.CorporationMembers.GetAllAsync(cancellationToken);
            var corporationMembers = existingMembers.Where(m => m.CorporationId == corporationId).ToList();
            
            foreach (var member in corporationMembers)
            {
                await _unitOfWork.CorporationMembers.DeleteAsync(member, cancellationToken);
            }
            
            // Add new members
            foreach (var member in members)
            {
                await _unitOfWork.CorporationMembers.AddAsync(member, cancellationToken);
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            // Cache for 1 hour
            await _cacheService.SetAsync($"corp_members_{corporationId}", 
                members, TimeSpan.FromHours(1), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing corporation members for {CorporationId}", corporationId);
        }
    }

    private async Task StoreCorporationStandingsAsync(int corporationId, List<CorporationStanding> standings, CancellationToken cancellationToken)
    {
        try
        {
            // Clear existing standings
            var existingStandings = await _unitOfWork.CorporationStandings.GetAllAsync(cancellationToken);
            var corporationStandings = existingStandings.Where(s => s.CorporationId == corporationId).ToList();
            
            foreach (var standing in corporationStandings)
            {
                await _unitOfWork.CorporationStandings.DeleteAsync(standing, cancellationToken);
            }
            
            // Add new standings
            foreach (var standing in standings)
            {
                await _unitOfWork.CorporationStandings.AddAsync(standing, cancellationToken);
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            // Cache for 4 hours
            await _cacheService.SetAsync($"corp_standings_{corporationId}", 
                standings, TimeSpan.FromHours(4), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing corporation standings for {CorporationId}", corporationId);
        }
    }

    #endregion
}

#endregion

#region Supporting Data Structures

/// <summary>
/// Corporation data from ESI
/// </summary>
public class CorporationData
{
    public int CorporationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Ticker { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? AllianceId { get; set; }
    public long CeoId { get; set; }
    public long CreatorId { get; set; }
    public DateTime DateFounded { get; set; }
    public int? FactionId { get; set; }
    public long? HomeStationId { get; set; }
    public int MemberCount { get; set; }
    public long Shares { get; set; }
    public float TaxRate { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool WarEligible { get; set; }
    public DateTime LastUpdated { get; set; }
    public SyncStatus SyncStatus { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Alliance data from ESI
/// </summary>
public class AllianceData
{
    public int AllianceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Ticker { get; set; } = string.Empty;
    public int CreatorCorporationId { get; set; }
    public long CreatorId { get; set; }
    public DateTime DateFounded { get; set; }
    public int? ExecutorCorporationId { get; set; }
    public int? FactionId { get; set; }
    public DateTime LastUpdated { get; set; }
    public SyncStatus SyncStatus { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Corporation member information
/// </summary>
public class CorporationMember
{
    public long CharacterId { get; set; }
    public int CorporationId { get; set; }
    public DateTime JoinDate { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Corporation standing information
/// </summary>
public class CorporationStanding
{
    public int CorporationId { get; set; }
    public int FromId { get; set; }
    public string FromType { get; set; } = string.Empty;
    public float Standing { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Complete corporation and alliance summary
/// </summary>
public class CorporationAllianceSummary
{
    public long CharacterId { get; set; }
    public CorporationData? Corporation { get; set; }
    public AllianceData? Alliance { get; set; }
    public List<CorporationMember> Members { get; set; } = new();
    public List<CorporationStanding> Standings { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public SyncStatus SyncStatus { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// ESI corporation info response
/// </summary>
public class EsiCorporationInfo
{
    public string Name { get; set; } = string.Empty;
    public string Ticker { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? AllianceId { get; set; }
    public long CeoId { get; set; }
    public long CreatorId { get; set; }
    public DateTime DateFounded { get; set; }
    public int? FactionId { get; set; }
    public long? HomeStationId { get; set; }
    public int MemberCount { get; set; }
    public long Shares { get; set; }
    public float TaxRate { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool WarEligible { get; set; }
}

/// <summary>
/// ESI alliance info response
/// </summary>
public class EsiAllianceInfo
{
    public string Name { get; set; } = string.Empty;
    public string Ticker { get; set; } = string.Empty;
    public int CreatorCorporationId { get; set; }
    public long CreatorId { get; set; }
    public DateTime DateFounded { get; set; }
    public int? ExecutorCorporationId { get; set; }
    public int? FactionId { get; set; }
}

/// <summary>
/// ESI corporation member response
/// </summary>
public class EsiCorporationMember
{
    public long CharacterId { get; set; }
    public DateTime StartDate { get; set; }
}

/// <summary>
/// ESI corporation standing response
/// </summary>
public class EsiCorporationStanding
{
    public int FromId { get; set; }
    public string FromType { get; set; } = string.Empty;
    public float Standing { get; set; }
}

/// <summary>
/// Corporation and alliance data service interface
/// </summary>
public interface ICorporationAllianceDataService
{
    Task<CorporationData> SynchronizeCorporationDataAsync(long characterId, string accessToken, CancellationToken cancellationToken = default);
    Task<AllianceData?> SynchronizeAllianceDataAsync(int corporationId, string accessToken, CancellationToken cancellationToken = default);
    Task<List<CorporationMember>> GetCorporationMembersAsync(int corporationId, string accessToken, CancellationToken cancellationToken = default);
    Task<List<CorporationStanding>> GetCorporationStandingsAsync(int corporationId, string accessToken, CancellationToken cancellationToken = default);
    Task<CorporationAllianceSummary> GetCompleteSummaryAsync(long characterId, string accessToken, CancellationToken cancellationToken = default);
}

#endregion

#region Jump Clone and Implant Services

/// <summary>
/// Jump clone and implant tracking service
/// </summary>
public class JumpCloneImplantService : IJumpCloneImplantService
{
    private readonly IEsiApiService _esiApiService;
    private readonly IEsiCharacterService _esiCharacterService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<JumpCloneImplantService> _logger;
    private readonly IAuditLogService _auditService;
    private readonly IEsiDegradationService _degradationService;
    private readonly ICacheService _cacheService;
    
    private readonly ConcurrentDictionary<long, DateTime> _jumpCloneSyncTimes = new();
    private readonly ConcurrentDictionary<long, DateTime> _implantSyncTimes = new();

    public JumpCloneImplantService(
        IEsiApiService esiApiService,
        IEsiCharacterService esiCharacterService,
        IUnitOfWork unitOfWork,
        ILogger<JumpCloneImplantService> logger,
        IAuditLogService auditService,
        IEsiDegradationService degradationService,
        ICacheService cacheService)
    {
        _esiApiService = esiApiService ?? throw new ArgumentNullException(nameof(esiApiService));
        _esiCharacterService = esiCharacterService ?? throw new ArgumentNullException(nameof(esiCharacterService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _degradationService = degradationService ?? throw new ArgumentNullException(nameof(degradationService));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    /// <summary>
    /// Synchronize jump clone data for character
    /// </summary>
    public async Task<JumpCloneData> SynchronizeJumpClonesAsync(long characterId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting jump clone synchronization for character {CharacterId}", characterId);
            
            // Check if we need to update (once per 30 minutes)
            if (_jumpCloneSyncTimes.TryGetValue(characterId, out var lastSync) && 
                DateTime.UtcNow - lastSync < TimeSpan.FromMinutes(30))
            {
                var cached = await GetCachedJumpCloneDataAsync(characterId, cancellationToken);
                if (cached != null)
                {
                    _logger.LogDebug("Using cached jump clone data for character {CharacterId}", characterId);
                    return cached;
                }
            }
            
            var jumpCloneData = await _degradationService.ExecuteWithFallbackAsync(
                async (ct) => await RetrieveJumpClonesFromEsiAsync(characterId, accessToken, ct),
                async (ct) => await GetCachedJumpCloneDataAsync(characterId, ct),
                $"jump_clones_{characterId}",
                cancellationToken);
            
            if (jumpCloneData != null)
            {
                jumpCloneData.LastUpdated = DateTime.UtcNow;
                jumpCloneData.SyncStatus = SyncStatus.Success;
                
                // Store in database
                await StoreJumpCloneDataAsync(jumpCloneData, cancellationToken);
                
                _jumpCloneSyncTimes[characterId] = DateTime.UtcNow;
                
                await _auditService.LogActionAsync("jump_clones_synced", "Character", 
                    characterId.ToString(), cancellationToken);
                
                _logger.LogInformation("Jump clone data synchronized successfully for character {CharacterId}: {CloneCount} clones", 
                    characterId, jumpCloneData.JumpClones.Count);
            }
            
            return jumpCloneData ?? new JumpCloneData 
            { 
                CharacterId = characterId, 
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = "Failed to retrieve jump clone data"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing jump clones for character {CharacterId}", characterId);
            
            return new JumpCloneData 
            { 
                CharacterId = characterId,
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = ex.Message,
                LastUpdated = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Synchronize implant data for character
    /// </summary>
    public async Task<ImplantData> SynchronizeImplantsAsync(long characterId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting implant synchronization for character {CharacterId}", characterId);
            
            // Check if we need to update (once per 15 minutes for active implants)
            if (_implantSyncTimes.TryGetValue(characterId, out var lastSync) && 
                DateTime.UtcNow - lastSync < TimeSpan.FromMinutes(15))
            {
                var cached = await GetCachedImplantDataAsync(characterId, cancellationToken);
                if (cached != null)
                {
                    _logger.LogDebug("Using cached implant data for character {CharacterId}", characterId);
                    return cached;
                }
            }
            
            var implantData = await _degradationService.ExecuteWithFallbackAsync(
                async (ct) => await RetrieveImplantsFromEsiAsync(characterId, accessToken, ct),
                async (ct) => await GetCachedImplantDataAsync(characterId, ct),
                $"implants_{characterId}",
                cancellationToken);
            
            if (implantData != null)
            {
                implantData.LastUpdated = DateTime.UtcNow;
                implantData.SyncStatus = SyncStatus.Success;
                
                // Store in database
                await StoreImplantDataAsync(implantData, cancellationToken);
                
                _implantSyncTimes[characterId] = DateTime.UtcNow;
                
                await _auditService.LogActionAsync("implants_synced", "Character", 
                    characterId.ToString(), cancellationToken);
                
                _logger.LogInformation("Implant data synchronized successfully for character {CharacterId}: {ImplantCount} implants", 
                    characterId, implantData.ActiveImplants.Count);
            }
            
            return implantData ?? new ImplantData 
            { 
                CharacterId = characterId, 
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = "Failed to retrieve implant data"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing implants for character {CharacterId}", characterId);
            
            return new ImplantData 
            { 
                CharacterId = characterId,
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = ex.Message,
                LastUpdated = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Get complete clone and implant summary for character
    /// </summary>
    public async Task<CloneImplantSummary> GetCompleteSummaryAsync(long characterId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving complete clone/implant summary for character {CharacterId}", characterId);
            
            var summary = new CloneImplantSummary
            {
                CharacterId = characterId,
                LastUpdated = DateTime.UtcNow
            };
            
            // Get jump clone data
            summary.JumpClones = await SynchronizeJumpClonesAsync(characterId, accessToken, cancellationToken);
            
            // Get implant data
            summary.Implants = await SynchronizeImplantsAsync(characterId, accessToken, cancellationToken);
            
            // Calculate implant statistics
            summary.ImplantStats = CalculateImplantStatistics(summary.Implants, summary.JumpClones);
            
            summary.SyncStatus = SyncStatus.Success;
            
            await _auditService.LogActionAsync("clone_implant_summary_retrieved", "Character", 
                characterId.ToString(), cancellationToken);
            
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving clone/implant summary for character {CharacterId}", characterId);
            
            return new CloneImplantSummary
            {
                CharacterId = characterId,
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = ex.Message,
                LastUpdated = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Get implant analysis and recommendations
    /// </summary>
    public async Task<ImplantAnalysis> GetImplantAnalysisAsync(long characterId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating implant analysis for character {CharacterId}", characterId);
            
            var implantData = await GetCachedImplantDataAsync(characterId, cancellationToken);
            if (implantData == null)
            {
                throw new InvalidOperationException("No implant data available for analysis");
            }
            
            var analysis = new ImplantAnalysis
            {
                CharacterId = characterId,
                LastUpdated = DateTime.UtcNow
            };
            
            // Analyze current implants
            analysis.CurrentImplantValue = CalculateImplantValue(implantData.ActiveImplants);
            analysis.AttributeBonuses = CalculateAttributeBonuses(implantData.ActiveImplants);
            analysis.ImplantGrades = AnalyzeImplantGrades(implantData.ActiveImplants);
            
            // Generate recommendations
            analysis.Recommendations = GenerateImplantRecommendations(implantData.ActiveImplants);
            
            // Analyze jump clone implants
            analysis.JumpCloneComparison = await AnalyzeJumpCloneImplantsAsync(characterId, cancellationToken);
            
            await _auditService.LogActionAsync("implant_analysis_generated", "Character", 
                characterId.ToString(), cancellationToken);
            
            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating implant analysis for character {CharacterId}", characterId);
            
            return new ImplantAnalysis
            {
                CharacterId = characterId,
                ErrorMessage = ex.Message,
                LastUpdated = DateTime.UtcNow
            };
        }
    }

    #region Private ESI Retrieval Methods

    private async Task<JumpCloneData?> RetrieveJumpClonesFromEsiAsync(long characterId, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            // Get jump clones from ESI
            var jumpClones = await _esiApiService.GetAsync<EsiJumpCloneResponse>(
                $"/latest/characters/{characterId}/clones/", 
                accessToken, 
                cancellationToken);
            
            if (jumpClones == null) return null;
            
            var jumpCloneData = new JumpCloneData
            {
                CharacterId = characterId,
                HomeLocationId = jumpClones.home_location?.location_id,
                HomeLocationType = jumpClones.home_location?.location_type,
                LastCloneJumpDate = jumpClones.last_clone_jump_date,
                LastStationChangeDate = jumpClones.last_station_change_date,
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.Success
            };
            
            // Process jump clones
            if (jumpClones.jump_clones != null)
            {
                jumpCloneData.JumpClones = jumpClones.jump_clones.Select(jc => new JumpClone
                {
                    JumpCloneId = jc.jump_clone_id,
                    CharacterId = characterId,
                    Name = jc.name,
                    LocationId = jc.location_id,
                    LocationType = jc.location_type,
                    Implants = jc.implants?.Select(implantId => new JumpCloneImplant
                    {
                        JumpCloneId = jc.jump_clone_id,
                        ImplantTypeId = implantId,
                        LastUpdated = DateTime.UtcNow
                    }).ToList() ?? new List<JumpCloneImplant>(),
                    LastUpdated = DateTime.UtcNow
                }).ToList();
            }
            
            return jumpCloneData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving jump clones for character {CharacterId} from ESI", characterId);
            return null;
        }
    }

    private async Task<ImplantData?> RetrieveImplantsFromEsiAsync(long characterId, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            // Get implants from ESI
            var implants = await _esiApiService.GetAsync<List<int>>(
                $"/latest/characters/{characterId}/implants/", 
                accessToken, 
                cancellationToken);
            
            if (implants == null) return null;
            
            var implantData = new ImplantData
            {
                CharacterId = characterId,
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.Success
            };
            
            // Process active implants
            implantData.ActiveImplants = implants.Select(implantId => new CharacterImplant
            {
                CharacterId = characterId,
                ImplantTypeId = implantId,
                IsActive = true,
                LastUpdated = DateTime.UtcNow
            }).ToList();
            
            return implantData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving implants for character {CharacterId} from ESI", characterId);
            return null;
        }
    }

    #endregion

    #region Private Cache Methods

    private async Task<JumpCloneData?> GetCachedJumpCloneDataAsync(long characterId, CancellationToken cancellationToken)
    {
        try
        {
            return await _cacheService.GetAsync<JumpCloneData>($"jump_clones_{characterId}", cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private async Task<ImplantData?> GetCachedImplantDataAsync(long characterId, CancellationToken cancellationToken)
    {
        try
        {
            return await _cacheService.GetAsync<ImplantData>($"implants_{characterId}", cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region Private Storage Methods

    private async Task StoreJumpCloneDataAsync(JumpCloneData jumpCloneData, CancellationToken cancellationToken)
    {
        try
        {
            // Clear existing jump clones
            var existingClones = await _unitOfWork.JumpClones.FindAsync(
                jc => jc.CharacterId == jumpCloneData.CharacterId, cancellationToken);
            
            foreach (var clone in existingClones)
            {
                await _unitOfWork.JumpClones.DeleteAsync(clone, cancellationToken);
            }
            
            // Add new jump clones
            foreach (var clone in jumpCloneData.JumpClones)
            {
                await _unitOfWork.JumpClones.AddAsync(clone, cancellationToken);
                
                // Add jump clone implants
                foreach (var implant in clone.Implants)
                {
                    await _unitOfWork.JumpCloneImplants.AddAsync(implant, cancellationToken);
                }
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            // Cache for 30 minutes
            await _cacheService.SetAsync($"jump_clones_{jumpCloneData.CharacterId}", 
                jumpCloneData, TimeSpan.FromMinutes(30), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing jump clone data for character {CharacterId}", jumpCloneData.CharacterId);
        }
    }

    private async Task StoreImplantDataAsync(ImplantData implantData, CancellationToken cancellationToken)
    {
        try
        {
            // Clear existing implants
            var existingImplants = await _unitOfWork.CharacterImplants.FindAsync(
                i => i.CharacterId == implantData.CharacterId, cancellationToken);
            
            foreach (var implant in existingImplants)
            {
                await _unitOfWork.CharacterImplants.DeleteAsync(implant, cancellationToken);
            }
            
            // Add new implants
            foreach (var implant in implantData.ActiveImplants)
            {
                await _unitOfWork.CharacterImplants.AddAsync(implant, cancellationToken);
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            // Cache for 15 minutes
            await _cacheService.SetAsync($"implants_{implantData.CharacterId}", 
                implantData, TimeSpan.FromMinutes(15), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing implant data for character {CharacterId}", implantData.CharacterId);
        }
    }

    #endregion

    #region Private Analysis Methods

    private ImplantStatistics CalculateImplantStatistics(ImplantData implantData, JumpCloneData jumpCloneData)
    {
        try
        {
            var stats = new ImplantStatistics();
            
            if (implantData?.ActiveImplants != null)
            {
                stats.TotalActiveImplants = implantData.ActiveImplants.Count;
                stats.TotalImplantValue = CalculateImplantValue(implantData.ActiveImplants);
                stats.AttributeBonuses = CalculateAttributeBonuses(implantData.ActiveImplants);
            }
            
            if (jumpCloneData?.JumpClones != null)
            {
                stats.TotalJumpClones = jumpCloneData.JumpClones.Count;
                stats.JumpClonesWithImplants = jumpCloneData.JumpClones.Count(jc => jc.Implants.Any());
                
                var allJumpCloneImplants = jumpCloneData.JumpClones
                    .SelectMany(jc => jc.Implants)
                    .Select(jci => new CharacterImplant { ImplantTypeId = jci.ImplantTypeId })
                    .ToList();
                
                stats.TotalJumpCloneImplantValue = CalculateImplantValue(allJumpCloneImplants);
            }
            
            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating implant statistics");
            return new ImplantStatistics();
        }
    }

    private decimal CalculateImplantValue(List<CharacterImplant> implants)
    {
        // This would integrate with market data to calculate real values
        // For now, return placeholder calculation
        return implants.Count * 50_000_000m; // Rough estimate
    }

    private Dictionary<string, int> CalculateAttributeBonuses(List<CharacterImplant> implants)
    {
        // This would integrate with EVE static data to calculate attribute bonuses
        // For now, return placeholder data
        var bonuses = new Dictionary<string, int>
        {
            ["Intelligence"] = 0,
            ["Memory"] = 0,
            ["Charisma"] = 0,
            ["Perception"] = 0,
            ["Willpower"] = 0
        };
        
        // Placeholder calculation - would need EVE type data integration
        foreach (var implant in implants)
        {
            // Basic grade estimation based on type ID patterns
            var grade = EstimateImplantGrade(implant.ImplantTypeId);
            bonuses["Intelligence"] += grade; // Simplified
        }
        
        return bonuses;
    }

    private Dictionary<string, int> AnalyzeImplantGrades(List<CharacterImplant> implants)
    {
        var grades = new Dictionary<string, int>
        {
            ["Low Grade"] = 0,
            ["Mid Grade"] = 0,
            ["High Grade"] = 0,
            ["Elite"] = 0
        };
        
        foreach (var implant in implants)
        {
            var grade = EstimateImplantGradeText(implant.ImplantTypeId);
            if (grades.ContainsKey(grade))
            {
                grades[grade]++;
            }
        }
        
        return grades;
    }

    private List<ImplantRecommendation> GenerateImplantRecommendations(List<CharacterImplant> currentImplants)
    {
        var recommendations = new List<ImplantRecommendation>();
        
        // Basic recommendations
        if (currentImplants.Count < 5)
        {
            recommendations.Add(new ImplantRecommendation
            {
                Type = "Missing Implants",
                Description = $"You have {currentImplants.Count}/5 implant slots filled. Consider adding more implants for better bonuses.",
                Priority = "Medium",
                EstimatedCost = 100_000_000m
            });
        }
        
        // Grade upgrade recommendations
        var lowGradeCount = currentImplants.Count(i => EstimateImplantGrade(i.ImplantTypeId) < 3);
        if (lowGradeCount > 0)
        {
            recommendations.Add(new ImplantRecommendation
            {
                Type = "Upgrade Recommendation",
                Description = $"Consider upgrading {lowGradeCount} low-grade implants to higher grades for better bonuses.",
                Priority = "Low",
                EstimatedCost = lowGradeCount * 200_000_000m
            });
        }
        
        return recommendations;
    }

    private async Task<JumpCloneComparison> AnalyzeJumpCloneImplantsAsync(long characterId, CancellationToken cancellationToken)
    {
        try
        {
            var jumpCloneData = await GetCachedJumpCloneDataAsync(characterId, cancellationToken);
            var implantData = await GetCachedImplantDataAsync(characterId, cancellationToken);
            
            var comparison = new JumpCloneComparison
            {
                CharacterId = characterId,
                LastUpdated = DateTime.UtcNow
            };
            
            if (jumpCloneData?.JumpClones != null && implantData?.ActiveImplants != null)
            {
                comparison.CurrentCloneValue = CalculateImplantValue(implantData.ActiveImplants);
                
                comparison.JumpCloneValues = jumpCloneData.JumpClones.ToDictionary(
                    jc => jc.JumpCloneId,
                    jc => CalculateImplantValue(jc.Implants.Select(i => new CharacterImplant { ImplantTypeId = i.ImplantTypeId }).ToList())
                );
                
                comparison.OptimalCloneRecommendation = comparison.JumpCloneValues
                    .OrderByDescending(kvp => kvp.Value)
                    .FirstOrDefault().Key;
            }
            
            return comparison;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing jump clone implants for character {CharacterId}", characterId);
            return new JumpCloneComparison { CharacterId = characterId };
        }
    }

    private int EstimateImplantGrade(int implantTypeId)
    {
        // Placeholder estimation based on type ID patterns
        // Would need integration with EVE static data
        return (implantTypeId % 10) + 1; // Returns 1-10
    }

    private string EstimateImplantGradeText(int implantTypeId)
    {
        var grade = EstimateImplantGrade(implantTypeId);
        return grade switch
        {
            <= 3 => "Low Grade",
            <= 6 => "Mid Grade",
            <= 8 => "High Grade",
            _ => "Elite"
        };
    }

    #endregion
}

#endregion

#region Jump Clone and Implant Data Structures

/// <summary>
/// Jump clone data container
/// </summary>
public class JumpCloneData
{
    public long CharacterId { get; set; }
    public long? HomeLocationId { get; set; }
    public string HomeLocationType { get; set; } = string.Empty;
    public DateTime? LastCloneJumpDate { get; set; }
    public DateTime? LastStationChangeDate { get; set; }
    public List<JumpClone> JumpClones { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public SyncStatus SyncStatus { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Individual jump clone information
/// </summary>
public class JumpClone
{
    public int JumpCloneId { get; set; }
    public long CharacterId { get; set; }
    public string Name { get; set; } = string.Empty;
    public long LocationId { get; set; }
    public string LocationType { get; set; } = string.Empty;
    public List<JumpCloneImplant> Implants { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Jump clone implant information
/// </summary>
public class JumpCloneImplant
{
    public int JumpCloneId { get; set; }
    public int ImplantTypeId { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Implant data container
/// </summary>
public class ImplantData
{
    public long CharacterId { get; set; }
    public List<CharacterImplant> ActiveImplants { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public SyncStatus SyncStatus { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Character implant information
/// </summary>
public class CharacterImplant
{
    public long CharacterId { get; set; }
    public int ImplantTypeId { get; set; }
    public bool IsActive { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Combined clone and implant summary
/// </summary>
public class CloneImplantSummary
{
    public long CharacterId { get; set; }
    public JumpCloneData JumpClones { get; set; } = new();
    public ImplantData Implants { get; set; } = new();
    public ImplantStatistics ImplantStats { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public SyncStatus SyncStatus { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Implant statistics and analysis
/// </summary>
public class ImplantStatistics
{
    public int TotalActiveImplants { get; set; }
    public decimal TotalImplantValue { get; set; }
    public int TotalJumpClones { get; set; }
    public int JumpClonesWithImplants { get; set; }
    public decimal TotalJumpCloneImplantValue { get; set; }
    public Dictionary<string, int> AttributeBonuses { get; set; } = new();
}

/// <summary>
/// Implant analysis and recommendations
/// </summary>
public class ImplantAnalysis
{
    public long CharacterId { get; set; }
    public decimal CurrentImplantValue { get; set; }
    public Dictionary<string, int> AttributeBonuses { get; set; } = new();
    public Dictionary<string, int> ImplantGrades { get; set; } = new();
    public List<ImplantRecommendation> Recommendations { get; set; } = new();
    public JumpCloneComparison JumpCloneComparison { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Implant recommendation
/// </summary>
public class ImplantRecommendation
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public decimal EstimatedCost { get; set; }
}

/// <summary>
/// Jump clone comparison analysis
/// </summary>
public class JumpCloneComparison
{
    public long CharacterId { get; set; }
    public decimal CurrentCloneValue { get; set; }
    public Dictionary<int, decimal> JumpCloneValues { get; set; } = new();
    public int OptimalCloneRecommendation { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// ESI jump clone response structure
/// </summary>
public class EsiJumpCloneResponse
{
    public EsiHomeLocation? home_location { get; set; }
    public List<EsiJumpClone>? jump_clones { get; set; }
    public DateTime? last_clone_jump_date { get; set; }
    public DateTime? last_station_change_date { get; set; }
}

/// <summary>
/// ESI home location structure
/// </summary>
public class EsiHomeLocation
{
    public long location_id { get; set; }
    public string location_type { get; set; } = string.Empty;
}

/// <summary>
/// ESI jump clone structure
/// </summary>
public class EsiJumpClone
{
    public int jump_clone_id { get; set; }
    public string name { get; set; } = string.Empty;
    public long location_id { get; set; }
    public string location_type { get; set; } = string.Empty;
    public List<int>? implants { get; set; }
}

/// <summary>
/// Jump clone and implant service interface
/// </summary>
public interface IJumpCloneImplantService
{
    Task<JumpCloneData> SynchronizeJumpClonesAsync(long characterId, string accessToken, CancellationToken cancellationToken = default);
    Task<ImplantData> SynchronizeImplantsAsync(long characterId, string accessToken, CancellationToken cancellationToken = default);
    Task<CloneImplantSummary> GetCompleteSummaryAsync(long characterId, string accessToken, CancellationToken cancellationToken = default);
    Task<ImplantAnalysis> GetImplantAnalysisAsync(long characterId, CancellationToken cancellationToken = default);
}

#endregion

#region Character Wallet and Transaction Services

/// <summary>
/// Character wallet and transaction history service
/// </summary>
public class CharacterWalletService : ICharacterWalletService
{
    private readonly IEsiApiService _esiApiService;
    private readonly IEsiCharacterService _esiCharacterService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CharacterWalletService> _logger;
    private readonly IAuditLogService _auditService;
    private readonly IEsiDegradationService _degradationService;
    private readonly ICacheService _cacheService;
    
    private readonly ConcurrentDictionary<long, DateTime> _walletSyncTimes = new();
    private readonly ConcurrentDictionary<long, DateTime> _transactionSyncTimes = new();

    public CharacterWalletService(
        IEsiApiService esiApiService,
        IEsiCharacterService esiCharacterService,
        IUnitOfWork unitOfWork,
        ILogger<CharacterWalletService> logger,
        IAuditLogService auditService,
        IEsiDegradationService degradationService,
        ICacheService cacheService)
    {
        _esiApiService = esiApiService ?? throw new ArgumentNullException(nameof(esiApiService));
        _esiCharacterService = esiCharacterService ?? throw new ArgumentNullException(nameof(esiCharacterService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _degradationService = degradationService ?? throw new ArgumentNullException(nameof(degradationService));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    /// <summary>
    /// Synchronize character wallet information
    /// </summary>
    public async Task<CharacterWalletData> SynchronizeWalletAsync(long characterId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting wallet synchronization for character {CharacterId}", characterId);
            
            // Check if we need to update (once per 5 minutes)
            if (_walletSyncTimes.TryGetValue(characterId, out var lastSync) && 
                DateTime.UtcNow - lastSync < TimeSpan.FromMinutes(5))
            {
                var cached = await GetCachedWalletDataAsync(characterId, cancellationToken);
                if (cached != null)
                {
                    _logger.LogDebug("Using cached wallet data for character {CharacterId}", characterId);
                    return cached;
                }
            }
            
            var walletData = await _degradationService.ExecuteWithFallbackAsync(
                async (ct) => await RetrieveWalletFromEsiAsync(characterId, accessToken, ct),
                async (ct) => await GetCachedWalletDataAsync(characterId, ct),
                $"wallet_{characterId}",
                cancellationToken);
            
            if (walletData != null)
            {
                walletData.LastUpdated = DateTime.UtcNow;
                walletData.SyncStatus = SyncStatus.Success;
                
                // Store in database
                await StoreWalletDataAsync(walletData, cancellationToken);
                
                _walletSyncTimes[characterId] = DateTime.UtcNow;
                
                await _auditService.LogActionAsync("wallet_synced", "Character", 
                    characterId.ToString(), cancellationToken);
                
                _logger.LogInformation("Wallet data synchronized successfully for character {CharacterId}: {Balance:C}", 
                    characterId, walletData.Balance);
            }
            
            return walletData ?? new CharacterWalletData 
            { 
                CharacterId = characterId, 
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = "Failed to retrieve wallet data"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing wallet for character {CharacterId}", characterId);
            
            return new CharacterWalletData 
            { 
                CharacterId = characterId,
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = ex.Message,
                LastUpdated = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Synchronize character transaction history
    /// </summary>
    public async Task<TransactionHistoryData> SynchronizeTransactionsAsync(long characterId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting transaction history synchronization for character {CharacterId}", characterId);
            
            // Check if we need to update (once per 15 minutes)
            if (_transactionSyncTimes.TryGetValue(characterId, out var lastSync) && 
                DateTime.UtcNow - lastSync < TimeSpan.FromMinutes(15))
            {
                var cached = await GetCachedTransactionDataAsync(characterId, cancellationToken);
                if (cached != null)
                {
                    _logger.LogDebug("Using cached transaction data for character {CharacterId}", characterId);
                    return cached;
                }
            }
            
            var transactionData = await _degradationService.ExecuteWithFallbackAsync(
                async (ct) => await RetrieveTransactionsFromEsiAsync(characterId, accessToken, ct),
                async (ct) => await GetCachedTransactionDataAsync(characterId, ct),
                $"transactions_{characterId}",
                cancellationToken);
            
            if (transactionData != null)
            {
                transactionData.LastUpdated = DateTime.UtcNow;
                transactionData.SyncStatus = SyncStatus.Success;
                
                // Store in database
                await StoreTransactionDataAsync(transactionData, cancellationToken);
                
                _transactionSyncTimes[characterId] = DateTime.UtcNow;
                
                await _auditService.LogActionAsync("transactions_synced", "Character", 
                    characterId.ToString(), cancellationToken);
                
                _logger.LogInformation("Transaction history synchronized successfully for character {CharacterId}: {TransactionCount} transactions", 
                    characterId, transactionData.Transactions.Count);
            }
            
            return transactionData ?? new TransactionHistoryData 
            { 
                CharacterId = characterId, 
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = "Failed to retrieve transaction data"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing transactions for character {CharacterId}", characterId);
            
            return new TransactionHistoryData 
            { 
                CharacterId = characterId,
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = ex.Message,
                LastUpdated = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Get wallet journal entries for character
    /// </summary>
    public async Task<WalletJournalData> SynchronizeWalletJournalAsync(long characterId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting wallet journal synchronization for character {CharacterId}", characterId);
            
            var journalData = await _degradationService.ExecuteWithFallbackAsync(
                async (ct) => await RetrieveWalletJournalFromEsiAsync(characterId, accessToken, ct),
                async (ct) => await GetCachedWalletJournalDataAsync(characterId, ct),
                $"wallet_journal_{characterId}",
                cancellationToken);
            
            if (journalData != null)
            {
                journalData.LastUpdated = DateTime.UtcNow;
                journalData.SyncStatus = SyncStatus.Success;
                
                // Store in database
                await StoreWalletJournalDataAsync(journalData, cancellationToken);
                
                await _auditService.LogActionAsync("wallet_journal_synced", "Character", 
                    characterId.ToString(), cancellationToken);
                
                _logger.LogInformation("Wallet journal synchronized successfully for character {CharacterId}: {EntryCount} entries", 
                    characterId, journalData.JournalEntries.Count);
            }
            
            return journalData ?? new WalletJournalData 
            { 
                CharacterId = characterId, 
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = "Failed to retrieve wallet journal data"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing wallet journal for character {CharacterId}", characterId);
            
            return new WalletJournalData 
            { 
                CharacterId = characterId,
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = ex.Message,
                LastUpdated = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Get complete wallet summary including balance, transactions, and journal
    /// </summary>
    public async Task<WalletSummary> GetCompleteSummaryAsync(long characterId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving complete wallet summary for character {CharacterId}", characterId);
            
            var summary = new WalletSummary
            {
                CharacterId = characterId,
                LastUpdated = DateTime.UtcNow
            };
            
            // Get wallet balance
            summary.WalletData = await SynchronizeWalletAsync(characterId, accessToken, cancellationToken);
            
            // Get transaction history
            summary.TransactionHistory = await SynchronizeTransactionsAsync(characterId, accessToken, cancellationToken);
            
            // Get wallet journal
            summary.WalletJournal = await SynchronizeWalletJournalAsync(characterId, accessToken, cancellationToken);
            
            // Calculate financial statistics
            summary.FinancialStats = CalculateFinancialStatistics(summary.WalletData, summary.TransactionHistory, summary.WalletJournal);
            
            summary.SyncStatus = SyncStatus.Success;
            
            await _auditService.LogActionAsync("wallet_summary_retrieved", "Character", 
                characterId.ToString(), cancellationToken);
            
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving wallet summary for character {CharacterId}", characterId);
            
            return new WalletSummary
            {
                CharacterId = characterId,
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = ex.Message,
                LastUpdated = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Get trading performance analysis
    /// </summary>
    public async Task<TradingAnalysis> GetTradingAnalysisAsync(long characterId, TimeSpan period, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating trading analysis for character {CharacterId} over {Period}", characterId, period);
            
            var transactionData = await GetCachedTransactionDataAsync(characterId, cancellationToken);
            if (transactionData == null)
            {
                throw new InvalidOperationException("No transaction data available for analysis");
            }
            
            var cutoffDate = DateTime.UtcNow - period;
            var recentTransactions = transactionData.Transactions
                .Where(t => t.Date >= cutoffDate)
                .ToList();
            
            var analysis = new TradingAnalysis
            {
                CharacterId = characterId,
                AnalysisPeriod = period,
                LastUpdated = DateTime.UtcNow
            };
            
            // Calculate trading metrics
            analysis.TotalTransactions = recentTransactions.Count;
            analysis.TotalVolume = recentTransactions.Sum(t => t.UnitPrice * t.Quantity);
            analysis.BuyTransactions = recentTransactions.Count(t => !t.IsBuy);
            analysis.SellTransactions = recentTransactions.Count(t => t.IsBuy);
            
            // Calculate profit/loss
            var buyValue = recentTransactions.Where(t => !t.IsBuy).Sum(t => t.UnitPrice * t.Quantity);
            var sellValue = recentTransactions.Where(t => t.IsBuy).Sum(t => t.UnitPrice * t.Quantity);
            analysis.NetProfit = sellValue - buyValue;
            
            // Most traded items
            analysis.TopTradedItems = recentTransactions
                .GroupBy(t => t.TypeId)
                .OrderByDescending(g => g.Sum(t => t.UnitPrice * t.Quantity))
                .Take(10)
                .ToDictionary(g => g.Key, g => new TradingItemStats
                {
                    TransactionCount = g.Count(),
                    TotalVolume = g.Sum(t => t.UnitPrice * t.Quantity),
                    AveragePrice = g.Average(t => t.UnitPrice),
                    ProfitLoss = g.Where(t => t.IsBuy).Sum(t => t.UnitPrice * t.Quantity) - 
                                g.Where(t => !t.IsBuy).Sum(t => t.UnitPrice * t.Quantity)
                });
            
            // Trading locations
            analysis.TradingLocations = recentTransactions
                .GroupBy(t => t.LocationId)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .ToDictionary(g => g.Key, g => g.Count());
            
            await _auditService.LogActionAsync("trading_analysis_generated", "Character", 
                characterId.ToString(), cancellationToken);
            
            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating trading analysis for character {CharacterId}", characterId);
            
            return new TradingAnalysis
            {
                CharacterId = characterId,
                AnalysisPeriod = period,
                ErrorMessage = ex.Message,
                LastUpdated = DateTime.UtcNow
            };
        }
    }

    #region Private ESI Retrieval Methods

    private async Task<CharacterWalletData?> RetrieveWalletFromEsiAsync(long characterId, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            // Get wallet balance from ESI
            var walletBalance = await _esiApiService.GetAsync<decimal>(
                $"/latest/characters/{characterId}/wallet/", 
                accessToken, 
                cancellationToken);
            
            return new CharacterWalletData
            {
                CharacterId = characterId,
                Balance = walletBalance,
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.Success
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving wallet for character {CharacterId} from ESI", characterId);
            return null;
        }
    }

    private async Task<TransactionHistoryData?> RetrieveTransactionsFromEsiAsync(long characterId, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            // Get transactions from ESI
            var transactions = await _esiApiService.GetAsync<List<EsiTransaction>>(
                $"/latest/characters/{characterId}/wallet/transactions/", 
                accessToken, 
                cancellationToken);
            
            if (transactions == null) return null;
            
            var transactionData = new TransactionHistoryData
            {
                CharacterId = characterId,
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.Success
            };
            
            // Process transactions
            transactionData.Transactions = transactions.Select(t => new CharacterTransaction
            {
                TransactionId = t.transaction_id,
                CharacterId = characterId,
                ClientId = t.client_id,
                Date = t.date,
                IsBuy = t.is_buy,
                IsPersonal = t.is_personal,
                JournalRefId = t.journal_ref_id,
                LocationId = t.location_id,
                Quantity = t.quantity,
                TypeId = t.type_id,
                UnitPrice = t.unit_price,
                LastUpdated = DateTime.UtcNow
            }).ToList();
            
            return transactionData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions for character {CharacterId} from ESI", characterId);
            return null;
        }
    }

    private async Task<WalletJournalData?> RetrieveWalletJournalFromEsiAsync(long characterId, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            // Get wallet journal from ESI
            var journalEntries = await _esiApiService.GetAsync<List<EsiWalletJournalEntry>>(
                $"/latest/characters/{characterId}/wallet/journal/", 
                accessToken, 
                cancellationToken);
            
            if (journalEntries == null) return null;
            
            var journalData = new WalletJournalData
            {
                CharacterId = characterId,
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.Success
            };
            
            // Process journal entries
            journalData.JournalEntries = journalEntries.Select(j => new WalletJournalEntry
            {
                Id = j.id,
                CharacterId = characterId,
                Amount = j.amount,
                Balance = j.balance,
                ContextId = j.context_id,
                ContextIdType = j.context_id_type,
                Date = j.date,
                Description = j.description,
                FirstPartyId = j.first_party_id,
                Reason = j.reason,
                RefType = j.ref_type,
                SecondPartyId = j.second_party_id,
                Tax = j.tax,
                TaxReceiverId = j.tax_receiver_id,
                LastUpdated = DateTime.UtcNow
            }).ToList();
            
            return journalData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving wallet journal for character {CharacterId} from ESI", characterId);
            return null;
        }
    }

    #endregion

    #region Private Cache Methods

    private async Task<CharacterWalletData?> GetCachedWalletDataAsync(long characterId, CancellationToken cancellationToken)
    {
        try
        {
            return await _cacheService.GetAsync<CharacterWalletData>($"wallet_{characterId}", cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private async Task<TransactionHistoryData?> GetCachedTransactionDataAsync(long characterId, CancellationToken cancellationToken)
    {
        try
        {
            return await _cacheService.GetAsync<TransactionHistoryData>($"transactions_{characterId}", cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private async Task<WalletJournalData?> GetCachedWalletJournalDataAsync(long characterId, CancellationToken cancellationToken)
    {
        try
        {
            return await _cacheService.GetAsync<WalletJournalData>($"wallet_journal_{characterId}", cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region Private Storage Methods

    private async Task StoreWalletDataAsync(CharacterWalletData walletData, CancellationToken cancellationToken)
    {
        try
        {
            // Store in database
            var existing = await _unitOfWork.CharacterWallets.GetByIdAsync(walletData.CharacterId, cancellationToken);
            if (existing != null)
            {
                // Update existing
                existing.Balance = walletData.Balance;
                existing.LastUpdated = DateTime.UtcNow;
                
                await _unitOfWork.CharacterWallets.UpdateAsync(existing, cancellationToken);
            }
            else
            {
                // Create new
                await _unitOfWork.CharacterWallets.AddAsync(walletData, cancellationToken);
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            // Cache for 5 minutes
            await _cacheService.SetAsync($"wallet_{walletData.CharacterId}", 
                walletData, TimeSpan.FromMinutes(5), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing wallet data for character {CharacterId}", walletData.CharacterId);
        }
    }

    private async Task StoreTransactionDataAsync(TransactionHistoryData transactionData, CancellationToken cancellationToken)
    {
        try
        {
            // Clear existing transactions for last 30 days (ESI limit)
            var cutoffDate = DateTime.UtcNow.AddDays(-30);
            var existingTransactions = await _unitOfWork.CharacterTransactions.FindAsync(
                t => t.CharacterId == transactionData.CharacterId && t.Date >= cutoffDate, cancellationToken);
            
            foreach (var transaction in existingTransactions)
            {
                await _unitOfWork.CharacterTransactions.DeleteAsync(transaction, cancellationToken);
            }
            
            // Add new transactions
            foreach (var transaction in transactionData.Transactions)
            {
                await _unitOfWork.CharacterTransactions.AddAsync(transaction, cancellationToken);
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            // Cache for 15 minutes
            await _cacheService.SetAsync($"transactions_{transactionData.CharacterId}", 
                transactionData, TimeSpan.FromMinutes(15), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing transaction data for character {CharacterId}", transactionData.CharacterId);
        }
    }

    private async Task StoreWalletJournalDataAsync(WalletJournalData journalData, CancellationToken cancellationToken)
    {
        try
        {
            // Clear existing journal entries for last 30 days (ESI limit)
            var cutoffDate = DateTime.UtcNow.AddDays(-30);
            var existingEntries = await _unitOfWork.WalletJournalEntries.FindAsync(
                j => j.CharacterId == journalData.CharacterId && j.Date >= cutoffDate, cancellationToken);
            
            foreach (var entry in existingEntries)
            {
                await _unitOfWork.WalletJournalEntries.DeleteAsync(entry, cancellationToken);
            }
            
            // Add new journal entries
            foreach (var entry in journalData.JournalEntries)
            {
                await _unitOfWork.WalletJournalEntries.AddAsync(entry, cancellationToken);
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            // Cache for 15 minutes
            await _cacheService.SetAsync($"wallet_journal_{journalData.CharacterId}", 
                journalData, TimeSpan.FromMinutes(15), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing wallet journal data for character {CharacterId}", journalData.CharacterId);
        }
    }

    #endregion

    #region Private Analysis Methods

    private FinancialStatistics CalculateFinancialStatistics(CharacterWalletData walletData, TransactionHistoryData transactionData, WalletJournalData journalData)
    {
        try
        {
            var stats = new FinancialStatistics
            {
                CurrentBalance = walletData?.Balance ?? 0,
                LastUpdated = DateTime.UtcNow
            };
            
            if (transactionData?.Transactions != null)
            {
                var recentTransactions = transactionData.Transactions.Where(t => t.Date >= DateTime.UtcNow.AddDays(-30)).ToList();
                
                stats.TotalTransactionsLast30Days = recentTransactions.Count;
                stats.TotalVolumeLast30Days = recentTransactions.Sum(t => t.UnitPrice * t.Quantity);
                stats.BuyOrdersLast30Days = recentTransactions.Count(t => !t.IsBuy);
                stats.SellOrdersLast30Days = recentTransactions.Count(t => t.IsBuy);
                
                var buyValue = recentTransactions.Where(t => !t.IsBuy).Sum(t => t.UnitPrice * t.Quantity);
                var sellValue = recentTransactions.Where(t => t.IsBuy).Sum(t => t.UnitPrice * t.Quantity);
                stats.NetTradingProfitLast30Days = sellValue - buyValue;
            }
            
            if (journalData?.JournalEntries != null)
            {
                var recentEntries = journalData.JournalEntries.Where(j => j.Date >= DateTime.UtcNow.AddDays(-30)).ToList();
                
                stats.TotalIncomeLastl30Days = recentEntries.Where(j => j.Amount > 0).Sum(j => j.Amount);
                stats.TotalExpensesLast30Days = recentEntries.Where(j => j.Amount < 0).Sum(j => Math.Abs(j.Amount));
                stats.NetIncomeLastl30Days = stats.TotalIncomeLastl30Days - stats.TotalExpensesLast30Days;
                
                // Tax statistics
                stats.TotalTaxesPaidLast30Days = recentEntries.Sum(j => j.Tax ?? 0);
            }
            
            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating financial statistics");
            return new FinancialStatistics { LastUpdated = DateTime.UtcNow };
        }
    }

    #endregion
}

#endregion

#region Wallet and Transaction Data Structures

/// <summary>
/// Character wallet data container
/// </summary>
public class CharacterWalletData
{
    public long CharacterId { get; set; }
    public decimal Balance { get; set; }
    public DateTime LastUpdated { get; set; }
    public SyncStatus SyncStatus { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Transaction history data container
/// </summary>
public class TransactionHistoryData
{
    public long CharacterId { get; set; }
    public List<CharacterTransaction> Transactions { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public SyncStatus SyncStatus { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Individual character transaction
/// </summary>
public class CharacterTransaction
{
    public long TransactionId { get; set; }
    public long CharacterId { get; set; }
    public int ClientId { get; set; }
    public DateTime Date { get; set; }
    public bool IsBuy { get; set; }
    public bool IsPersonal { get; set; }
    public long JournalRefId { get; set; }
    public long LocationId { get; set; }
    public int Quantity { get; set; }
    public int TypeId { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Wallet journal data container
/// </summary>
public class WalletJournalData
{
    public long CharacterId { get; set; }
    public List<WalletJournalEntry> JournalEntries { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public SyncStatus SyncStatus { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Individual wallet journal entry
/// </summary>
public class WalletJournalEntry
{
    public long Id { get; set; }
    public long CharacterId { get; set; }
    public decimal? Amount { get; set; }
    public decimal? Balance { get; set; }
    public long? ContextId { get; set; }
    public string ContextIdType { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? FirstPartyId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string RefType { get; set; } = string.Empty;
    public int? SecondPartyId { get; set; }
    public decimal? Tax { get; set; }
    public int? TaxReceiverId { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Complete wallet summary
/// </summary>
public class WalletSummary
{
    public long CharacterId { get; set; }
    public CharacterWalletData WalletData { get; set; } = new();
    public TransactionHistoryData TransactionHistory { get; set; } = new();
    public WalletJournalData WalletJournal { get; set; } = new();
    public FinancialStatistics FinancialStats { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public SyncStatus SyncStatus { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Financial statistics and analysis
/// </summary>
public class FinancialStatistics
{
    public decimal CurrentBalance { get; set; }
    public int TotalTransactionsLast30Days { get; set; }
    public decimal TotalVolumeLast30Days { get; set; }
    public int BuyOrdersLast30Days { get; set; }
    public int SellOrdersLast30Days { get; set; }
    public decimal NetTradingProfitLast30Days { get; set; }
    public decimal TotalIncomeLastl30Days { get; set; }
    public decimal TotalExpensesLast30Days { get; set; }
    public decimal NetIncomeLastl30Days { get; set; }
    public decimal TotalTaxesPaidLast30Days { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Trading performance analysis
/// </summary>
public class TradingAnalysis
{
    public long CharacterId { get; set; }
    public TimeSpan AnalysisPeriod { get; set; }
    public int TotalTransactions { get; set; }
    public decimal TotalVolume { get; set; }
    public int BuyTransactions { get; set; }
    public int SellTransactions { get; set; }
    public decimal NetProfit { get; set; }
    public Dictionary<int, TradingItemStats> TopTradedItems { get; set; } = new();
    public Dictionary<long, int> TradingLocations { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Trading statistics for a specific item
/// </summary>
public class TradingItemStats
{
    public int TransactionCount { get; set; }
    public decimal TotalVolume { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal ProfitLoss { get; set; }
}

/// <summary>
/// ESI transaction response structure
/// </summary>
public class EsiTransaction
{
    public long transaction_id { get; set; }
    public int client_id { get; set; }
    public DateTime date { get; set; }
    public bool is_buy { get; set; }
    public bool is_personal { get; set; }
    public long journal_ref_id { get; set; }
    public long location_id { get; set; }
    public int quantity { get; set; }
    public int type_id { get; set; }
    public decimal unit_price { get; set; }
}

/// <summary>
/// ESI wallet journal entry response structure
/// </summary>
public class EsiWalletJournalEntry
{
    public long id { get; set; }
    public decimal? amount { get; set; }
    public decimal? balance { get; set; }
    public long? context_id { get; set; }
    public string context_id_type { get; set; } = string.Empty;
    public DateTime date { get; set; }
    public string description { get; set; } = string.Empty;
    public int? first_party_id { get; set; }
    public string reason { get; set; } = string.Empty;
    public string ref_type { get; set; } = string.Empty;
    public int? second_party_id { get; set; }
    public decimal? tax { get; set; }
    public int? tax_receiver_id { get; set; }
}

/// <summary>
/// Character wallet service interface
/// </summary>
public interface ICharacterWalletService
{
    Task<CharacterWalletData> SynchronizeWalletAsync(long characterId, string accessToken, CancellationToken cancellationToken = default);
    Task<TransactionHistoryData> SynchronizeTransactionsAsync(long characterId, string accessToken, CancellationToken cancellationToken = default);
    Task<WalletJournalData> SynchronizeWalletJournalAsync(long characterId, string accessToken, CancellationToken cancellationToken = default);
    Task<WalletSummary> GetCompleteSummaryAsync(long characterId, string accessToken, CancellationToken cancellationToken = default);
    Task<TradingAnalysis> GetTradingAnalysisAsync(long characterId, TimeSpan period, CancellationToken cancellationToken = default);
}

/// <summary>
/// Character location and ship information tracking service
/// </summary>
public class CharacterLocationService : ICharacterLocationService
{
    private readonly IEsiApiService _esiApiService;
    private readonly IEsiCharacterService _esiCharacterService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CharacterLocationService> _logger;
    private readonly IAuditLogService _auditService;
    private readonly IEsiDegradationService _degradationService;
    
    private readonly ConcurrentDictionary<long, DateTime> _locationSyncTimes = new();
    private readonly ConcurrentDictionary<long, DateTime> _shipSyncTimes = new();

    public CharacterLocationService(
        IEsiApiService esiApiService,
        IEsiCharacterService esiCharacterService,
        IUnitOfWork unitOfWork,
        ILogger<CharacterLocationService> logger,
        IAuditLogService auditService,
        IEsiDegradationService degradationService)
    {
        _esiApiService = esiApiService ?? throw new ArgumentNullException(nameof(esiApiService));
        _esiCharacterService = esiCharacterService ?? throw new ArgumentNullException(nameof(esiCharacterService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _degradationService = degradationService ?? throw new ArgumentNullException(nameof(degradationService));
    }

    /// <summary>
    /// Synchronize character location data from ESI
    /// </summary>
    public async Task<CharacterLocationData> SynchronizeLocationAsync(long characterId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Synchronizing location data for character {CharacterId}", characterId);
            
            // Check if we need to update (once per 2 minutes for location)
            if (_locationSyncTimes.TryGetValue(characterId, out var lastSync) && 
                DateTime.UtcNow - lastSync < TimeSpan.FromMinutes(2))
            {
                var cached = await GetCachedLocationDataAsync(characterId, cancellationToken);
                if (cached != null) return cached;
            }
            
            var locationData = await _degradationService.ExecuteWithFallbackAsync(
                async (ct) => await RetrieveLocationFromEsiAsync(characterId, accessToken, ct),
                async (ct) => await GetCachedLocationDataAsync(characterId, ct),
                $"location_{characterId}",
                cancellationToken);
            
            if (locationData != null)
            {
                // Store in database
                await StoreLocationDataAsync(locationData, cancellationToken);
                _locationSyncTimes[characterId] = DateTime.UtcNow;
                
                await _auditService.LogActionAsync("character_location_synced", "Character", 
                    characterId.ToString(), cancellationToken);
            }
            
            return locationData ?? new CharacterLocationData 
            { 
                CharacterId = characterId, 
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = "Failed to retrieve location data"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing location for character {CharacterId}", characterId);
            
            return new CharacterLocationData 
            { 
                CharacterId = characterId, 
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Synchronize character ship information from ESI
    /// </summary>
    public async Task<CharacterShipData> SynchronizeShipAsync(long characterId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Synchronizing ship data for character {CharacterId}", characterId);
            
            // Check if we need to update (once per 5 minutes for ship)
            if (_shipSyncTimes.TryGetValue(characterId, out var lastSync) && 
                DateTime.UtcNow - lastSync < TimeSpan.FromMinutes(5))
            {
                var cached = await GetCachedShipDataAsync(characterId, cancellationToken);
                if (cached != null) return cached;
            }
            
            var shipData = await _degradationService.ExecuteWithFallbackAsync(
                async (ct) => await RetrieveShipFromEsiAsync(characterId, accessToken, ct),
                async (ct) => await GetCachedShipDataAsync(characterId, ct),
                $"ship_{characterId}",
                cancellationToken);
            
            if (shipData != null)
            {
                // Store in database
                await StoreShipDataAsync(shipData, cancellationToken);
                _shipSyncTimes[characterId] = DateTime.UtcNow;
                
                await _auditService.LogActionAsync("character_ship_synced", "Character", 
                    characterId.ToString(), cancellationToken);
            }
            
            return shipData ?? new CharacterShipData 
            { 
                CharacterId = characterId, 
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = "Failed to retrieve ship data"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing ship for character {CharacterId}", characterId);
            
            return new CharacterShipData 
            { 
                CharacterId = characterId, 
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Get character movement history
    /// </summary>
    public async Task<List<CharacterLocationHistory>> GetLocationHistoryAsync(long characterId, int days = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            
            var locationHistory = await _unitOfWork.LocationHistory
                .FindAsync(lh => lh.CharacterId == characterId && lh.Timestamp >= cutoffDate, cancellationToken);
            
            return locationHistory.OrderByDescending(lh => lh.Timestamp).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving location history for character {CharacterId}", characterId);
            return new List<CharacterLocationHistory>();
        }
    }

    /// <summary>
    /// Get ship change history
    /// </summary>
    public async Task<List<CharacterShipHistory>> GetShipHistoryAsync(long characterId, int days = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            
            var shipHistory = await _unitOfWork.ShipHistory
                .FindAsync(sh => sh.CharacterId == characterId && sh.Timestamp >= cutoffDate, cancellationToken);
            
            return shipHistory.OrderByDescending(sh => sh.Timestamp).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ship history for character {CharacterId}", characterId);
            return new List<CharacterShipHistory>();
        }
    }

    /// <summary>
    /// Get current online status
    /// </summary>
    public async Task<CharacterOnlineStatus> GetOnlineStatusAsync(long characterId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var onlineData = await _degradationService.ExecuteWithFallbackAsync(
                async (ct) => await RetrieveOnlineStatusFromEsiAsync(characterId, accessToken, ct),
                async (ct) => await GetCachedOnlineStatusAsync(characterId, ct),
                $"online_status_{characterId}",
                cancellationToken);
            
            return onlineData ?? new CharacterOnlineStatus 
            { 
                CharacterId = characterId, 
                IsOnline = false,
                LastSeen = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving online status for character {CharacterId}", characterId);
            
            return new CharacterOnlineStatus 
            { 
                CharacterId = characterId, 
                IsOnline = false,
                LastSeen = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                ErrorMessage = ex.Message
            };
        }
    }

    #region Private Methods

    private async Task<CharacterLocationData?> RetrieveLocationFromEsiAsync(long characterId, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            var locationResponse = await _esiCharacterService.GetCharacterLocationAsync(characterId, accessToken, cancellationToken);
            
            var locationData = new CharacterLocationData
            {
                CharacterId = characterId,
                SolarSystemId = locationResponse.SolarSystemId,
                SolarSystemName = await GetSolarSystemNameAsync(locationResponse.SolarSystemId, cancellationToken),
                StationId = locationResponse.StationId,
                StationName = locationResponse.StationId.HasValue ? 
                    await GetStationNameAsync(locationResponse.StationId.Value, cancellationToken) : null,
                StructureId = locationResponse.StructureId,
                StructureName = locationResponse.StructureId.HasValue ? 
                    await GetStructureNameAsync(locationResponse.StructureId.Value, accessToken, cancellationToken) : null,
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.Success
            };
            
            return locationData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving location from ESI for character {CharacterId}", characterId);
            return null;
        }
    }

    private async Task<CharacterShipData?> RetrieveShipFromEsiAsync(long characterId, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            var shipResponse = await _esiCharacterService.GetCharacterShipAsync(characterId, accessToken, cancellationToken);
            
            var shipData = new CharacterShipData
            {
                CharacterId = characterId,
                ShipTypeId = shipResponse.ShipTypeId,
                ShipTypeName = await GetShipTypeNameAsync(shipResponse.ShipTypeId, cancellationToken),
                ShipItemId = shipResponse.ShipItemId,
                ShipName = shipResponse.ShipName,
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.Success
            };
            
            return shipData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ship from ESI for character {CharacterId}", characterId);
            return null;
        }
    }

    private async Task<CharacterOnlineStatus?> RetrieveOnlineStatusFromEsiAsync(long characterId, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            var onlineResponse = await _esiCharacterService.GetCharacterOnlineAsync(characterId, accessToken, cancellationToken);
            
            var onlineStatus = new CharacterOnlineStatus
            {
                CharacterId = characterId,
                IsOnline = onlineResponse.Online,
                LastLogin = onlineResponse.LastLogin,
                LastLogout = onlineResponse.LastLogout,
                LoginsToday = onlineResponse.Logins,
                LastSeen = onlineResponse.Online ? DateTime.UtcNow : (onlineResponse.LastLogout ?? DateTime.UtcNow),
                LastUpdated = DateTime.UtcNow
            };
            
            return onlineStatus;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving online status from ESI for character {CharacterId}", characterId);
            return null;
        }
    }

    private async Task<CharacterLocationData?> GetCachedLocationDataAsync(long characterId, CancellationToken cancellationToken)
    {
        try
        {
            var cachedLocation = await _unitOfWork.CharacterLocations
                .FirstOrDefaultAsync(cl => cl.CharacterId == characterId, cancellationToken);
            
            return cachedLocation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached location for character {CharacterId}", characterId);
            return null;
        }
    }

    private async Task<CharacterShipData?> GetCachedShipDataAsync(long characterId, CancellationToken cancellationToken)
    {
        try
        {
            var cachedShip = await _unitOfWork.CharacterShips
                .FirstOrDefaultAsync(cs => cs.CharacterId == characterId, cancellationToken);
            
            return cachedShip;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached ship for character {CharacterId}", characterId);
            return null;
        }
    }

    private async Task<CharacterOnlineStatus?> GetCachedOnlineStatusAsync(long characterId, CancellationToken cancellationToken)
    {
        try
        {
            var cachedStatus = await _unitOfWork.CharacterOnlineStatus
                .FirstOrDefaultAsync(cos => cos.CharacterId == characterId, cancellationToken);
            
            return cachedStatus;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached online status for character {CharacterId}", characterId);
            return null;
        }
    }

    private async Task StoreLocationDataAsync(CharacterLocationData locationData, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _unitOfWork.CharacterLocations
                .FirstOrDefaultAsync(cl => cl.CharacterId == locationData.CharacterId, cancellationToken);
            
            if (existing != null)
            {
                // Check if location changed - if so, add to history
                if (existing.SolarSystemId != locationData.SolarSystemId ||
                    existing.StationId != locationData.StationId ||
                    existing.StructureId != locationData.StructureId)
                {
                    var historyEntry = new CharacterLocationHistory
                    {
                        CharacterId = locationData.CharacterId,
                        SolarSystemId = existing.SolarSystemId,
                        SolarSystemName = existing.SolarSystemName,
                        StationId = existing.StationId,
                        StationName = existing.StationName,
                        StructureId = existing.StructureId,
                        StructureName = existing.StructureName,
                        Timestamp = existing.LastUpdated
                    };
                    
                    await _unitOfWork.LocationHistory.AddAsync(historyEntry, cancellationToken);
                }
                
                // Update existing record
                existing.SolarSystemId = locationData.SolarSystemId;
                existing.SolarSystemName = locationData.SolarSystemName;
                existing.StationId = locationData.StationId;
                existing.StationName = locationData.StationName;
                existing.StructureId = locationData.StructureId;
                existing.StructureName = locationData.StructureName;
                existing.LastUpdated = locationData.LastUpdated;
                existing.SyncStatus = locationData.SyncStatus;
                existing.ErrorMessage = locationData.ErrorMessage;
                
                _unitOfWork.CharacterLocations.Update(existing);
            }
            else
            {
                await _unitOfWork.CharacterLocations.AddAsync(locationData, cancellationToken);
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing location data for character {CharacterId}", locationData.CharacterId);
            throw;
        }
    }

    private async Task StoreShipDataAsync(CharacterShipData shipData, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _unitOfWork.CharacterShips
                .FirstOrDefaultAsync(cs => cs.CharacterId == shipData.CharacterId, cancellationToken);
            
            if (existing != null)
            {
                // Check if ship changed - if so, add to history
                if (existing.ShipTypeId != shipData.ShipTypeId ||
                    existing.ShipItemId != shipData.ShipItemId)
                {
                    var historyEntry = new CharacterShipHistory
                    {
                        CharacterId = shipData.CharacterId,
                        ShipTypeId = existing.ShipTypeId,
                        ShipTypeName = existing.ShipTypeName,
                        ShipItemId = existing.ShipItemId,
                        ShipName = existing.ShipName,
                        Timestamp = existing.LastUpdated
                    };
                    
                    await _unitOfWork.ShipHistory.AddAsync(historyEntry, cancellationToken);
                }
                
                // Update existing record
                existing.ShipTypeId = shipData.ShipTypeId;
                existing.ShipTypeName = shipData.ShipTypeName;
                existing.ShipItemId = shipData.ShipItemId;
                existing.ShipName = shipData.ShipName;
                existing.LastUpdated = shipData.LastUpdated;
                existing.SyncStatus = shipData.SyncStatus;
                existing.ErrorMessage = shipData.ErrorMessage;
                
                _unitOfWork.CharacterShips.Update(existing);
            }
            else
            {
                await _unitOfWork.CharacterShips.AddAsync(shipData, cancellationToken);
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing ship data for character {CharacterId}", shipData.CharacterId);
            throw;
        }
    }

    private async Task<string> GetSolarSystemNameAsync(int solarSystemId, CancellationToken cancellationToken)
    {
        try
        {
            var solarSystem = await _unitOfWork.EveSolarSystems
                .FirstOrDefaultAsync(ss => ss.SolarSystemId == solarSystemId, cancellationToken);
            
            return solarSystem?.SolarSystemName ?? $"Unknown System ({solarSystemId})";
        }
        catch
        {
            return $"Unknown System ({solarSystemId})";
        }
    }

    private async Task<string?> GetStationNameAsync(long stationId, CancellationToken cancellationToken)
    {
        try
        {
            var station = await _unitOfWork.EveStations
                .FirstOrDefaultAsync(s => s.StationId == stationId, cancellationToken);
            
            return station?.StationName;
        }
        catch
        {
            return null;
        }
    }

    private async Task<string?> GetStructureNameAsync(long structureId, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            // Try to get structure name from ESI (requires auth)
            var structureResponse = await _esiApiService.GetAsync<EsiStructureResponse>(
                $"/universe/structures/{structureId}/", accessToken, cancellationToken);
            
            return structureResponse?.Name;
        }
        catch
        {
            return null;
        }
    }

    private async Task<string> GetShipTypeNameAsync(int shipTypeId, CancellationToken cancellationToken)
    {
        try
        {
            var shipType = await _unitOfWork.EveTypes
                .FirstOrDefaultAsync(t => t.TypeId == shipTypeId, cancellationToken);
            
            return shipType?.TypeName ?? $"Unknown Ship ({shipTypeId})";
        }
        catch
        {
            return $"Unknown Ship ({shipTypeId})";
        }
    }

    #endregion
}

#region Location and Ship Data Structures

/// <summary>
/// Character location data
/// </summary>
public class CharacterLocationData
{
    public long CharacterId { get; set; }
    public int SolarSystemId { get; set; }
    public string SolarSystemName { get; set; } = string.Empty;
    public long? StationId { get; set; }
    public string? StationName { get; set; }
    public long? StructureId { get; set; }
    public string? StructureName { get; set; }
    public DateTime LastUpdated { get; set; }
    public SyncStatus SyncStatus { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Character ship data
/// </summary>
public class CharacterShipData
{
    public long CharacterId { get; set; }
    public int ShipTypeId { get; set; }
    public string ShipTypeName { get; set; } = string.Empty;
    public long ShipItemId { get; set; }
    public string ShipName { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
    public SyncStatus SyncStatus { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Character online status
/// </summary>
public class CharacterOnlineStatus
{
    public long CharacterId { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastLogin { get; set; }
    public DateTime? LastLogout { get; set; }
    public int LoginsToday { get; set; }
    public DateTime LastSeen { get; set; }
    public DateTime LastUpdated { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Character location history entry
/// </summary>
public class CharacterLocationHistory
{
    public long CharacterId { get; set; }
    public int SolarSystemId { get; set; }
    public string SolarSystemName { get; set; } = string.Empty;
    public long? StationId { get; set; }
    public string? StationName { get; set; }
    public long? StructureId { get; set; }
    public string? StructureName { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Character ship history entry
/// </summary>
public class CharacterShipHistory
{
    public long CharacterId { get; set; }
    public int ShipTypeId { get; set; }
    public string ShipTypeName { get; set; } = string.Empty;
    public long ShipItemId { get; set; }
    public string ShipName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// ESI Location Response
/// </summary>
public class EsiLocationResponse
{
    public int SolarSystemId { get; set; }
    public long? StationId { get; set; }
    public long? StructureId { get; set; }
}

/// <summary>
/// ESI Ship Response
/// </summary>
public class EsiShipResponse
{
    public int ShipTypeId { get; set; }
    public long ShipItemId { get; set; }
    public string ShipName { get; set; } = string.Empty;
}

/// <summary>
/// ESI Online Response
/// </summary>
public class EsiOnlineResponse
{
    public bool Online { get; set; }
    public DateTime? LastLogin { get; set; }
    public DateTime? LastLogout { get; set; }
    public int Logins { get; set; }
}

/// <summary>
/// ESI Structure Response
/// </summary>
public class EsiStructureResponse
{
    public string Name { get; set; } = string.Empty;
    public int OwnerId { get; set; }
    public int SolarSystemId { get; set; }
    public int TypeId { get; set; }
}

/// <summary>
/// Character location service interface
/// </summary>
public interface ICharacterLocationService
{
    Task<CharacterLocationData> SynchronizeLocationAsync(long characterId, string accessToken, CancellationToken cancellationToken = default);
    Task<CharacterShipData> SynchronizeShipAsync(long characterId, string accessToken, CancellationToken cancellationToken = default);
    Task<List<CharacterLocationHistory>> GetLocationHistoryAsync(long characterId, int days = 30, CancellationToken cancellationToken = default);
    Task<List<CharacterShipHistory>> GetShipHistoryAsync(long characterId, int days = 30, CancellationToken cancellationToken = default);
    Task<CharacterOnlineStatus> GetOnlineStatusAsync(long characterId, string accessToken, CancellationToken cancellationToken = default);
}

/// <summary>
/// Skill queue monitoring and real-time updates service
/// </summary>
public class CharacterSkillQueueService : ICharacterSkillQueueService
{
    private readonly IEsiApiService _esiApiService;
    private readonly IEsiSkillsService _esiSkillsService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CharacterSkillQueueService> _logger;
    private readonly IAuditLogService _auditService;
    private readonly IEsiDegradationService _degradationService;
    
    private readonly ConcurrentDictionary<long, DateTime> _queueSyncTimes = new();
    private readonly ConcurrentDictionary<long, Timer> _realTimeTimers = new();

    public CharacterSkillQueueService(
        IEsiApiService esiApiService,
        IEsiSkillsService esiSkillsService,
        IUnitOfWork unitOfWork,
        ILogger<CharacterSkillQueueService> logger,
        IAuditLogService auditService,
        IEsiDegradationService degradationService)
    {
        _esiApiService = esiApiService ?? throw new ArgumentNullException(nameof(esiApiService));
        _esiSkillsService = esiSkillsService ?? throw new ArgumentNullException(nameof(esiSkillsService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _degradationService = degradationService ?? throw new ArgumentNullException(nameof(degradationService));
    }

    /// <summary>
    /// Synchronize skill queue from ESI with real-time updates
    /// </summary>
    public async Task<SkillQueueData> SynchronizeSkillQueueAsync(long characterId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Synchronizing skill queue for character {CharacterId}", characterId);
            
            // Check if we need to update (once per 10 minutes for skill queue)
            if (_queueSyncTimes.TryGetValue(characterId, out var lastSync) && 
                DateTime.UtcNow - lastSync < TimeSpan.FromMinutes(10))
            {
                var cached = await GetCachedSkillQueueAsync(characterId, cancellationToken);
                if (cached != null) return cached;
            }
            
            var queueData = await _degradationService.ExecuteWithFallbackAsync(
                async (ct) => await RetrieveSkillQueueFromEsiAsync(characterId, accessToken, ct),
                async (ct) => await GetCachedSkillQueueAsync(characterId, ct),
                $"skill_queue_{characterId}",
                cancellationToken);
            
            if (queueData != null)
            {
                // Store in database
                await StoreSkillQueueAsync(queueData, cancellationToken);
                _queueSyncTimes[characterId] = DateTime.UtcNow;
                
                // Set up real-time monitoring
                await SetupRealTimeMonitoringAsync(characterId, queueData, cancellationToken);
                
                await _auditService.LogActionAsync("skill_queue_synced", "Character", 
                    characterId.ToString(), cancellationToken);
            }
            
            return queueData ?? new SkillQueueData 
            { 
                CharacterId = characterId, 
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = "Failed to retrieve skill queue data"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing skill queue for character {CharacterId}", characterId);
            
            return new SkillQueueData 
            { 
                CharacterId = characterId, 
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Get real-time skill queue progress with training estimates
    /// </summary>
    public async Task<SkillQueueProgress> GetRealTimeProgressAsync(long characterId, CancellationToken cancellationToken = default)
    {
        try
        {
            var queueData = await GetCachedSkillQueueAsync(characterId, cancellationToken);
            if (queueData == null)
            {
                return new SkillQueueProgress
                {
                    CharacterId = characterId,
                    QueueEmpty = true,
                    LastUpdated = DateTime.UtcNow
                };
            }
            
            var progress = new SkillQueueProgress
            {
                CharacterId = characterId,
                QueueEmpty = !queueData.QueueEntries.Any(),
                LastUpdated = DateTime.UtcNow
            };
            
            if (!progress.QueueEmpty)
            {
                var now = DateTime.UtcNow;
                var activeSkill = queueData.QueueEntries.FirstOrDefault();
                
                if (activeSkill != null)
                {
                    progress.CurrentlyTraining = new CurrentSkillTraining
                    {
                        SkillId = activeSkill.SkillId,
                        SkillName = activeSkill.SkillName,
                        TrainedLevel = activeSkill.TrainedLevel,
                        QueuedLevel = activeSkill.QueuedLevel,
                        TrainingStartDate = activeSkill.TrainingStartDate,
                        TrainingEndDate = activeSkill.TrainingEndDate,
                        LevelStartSP = activeSkill.LevelStartSP,
                        LevelEndSP = activeSkill.LevelEndSP,
                        CurrentSP = CalculateCurrentSP(activeSkill, now),
                        RemainingTime = activeSkill.TrainingEndDate - now,
                        CompletionPercentage = CalculateCompletionPercentage(activeSkill, now)
                    };
                }
                
                // Calculate queue statistics
                progress.TotalSkillsInQueue = queueData.QueueEntries.Count;
                progress.TotalTrainingTime = queueData.QueueEntries.LastOrDefault()?.TrainingEndDate - DateTime.UtcNow;
                progress.EstimatedCompletion = queueData.QueueEntries.LastOrDefault()?.TrainingEndDate;
                
                // Upcoming skills
                progress.UpcomingSkills = queueData.QueueEntries.Skip(1).Take(5)
                    .Select(entry => new UpcomingSkill
                    {
                        SkillId = entry.SkillId,
                        SkillName = entry.SkillName,
                        QueuedLevel = entry.QueuedLevel,
                        TrainingEndDate = entry.TrainingEndDate,
                        TimeToStart = entry.TrainingStartDate - now
                    }).ToList();
            }
            
            return progress;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting real-time progress for character {CharacterId}", characterId);
            
            return new SkillQueueProgress
            {
                CharacterId = characterId,
                QueueEmpty = true,
                LastUpdated = DateTime.UtcNow,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Get detailed queue analysis and optimization suggestions
    /// </summary>
    public async Task<SkillQueueAnalysis> GetQueueAnalysisAsync(long characterId, CancellationToken cancellationToken = default)
    {
        try
        {
            var queueData = await GetCachedSkillQueueAsync(characterId, cancellationToken);
            var characterData = await GetCharacterAttributesAsync(characterId, cancellationToken);
            
            var analysis = new SkillQueueAnalysis
            {
                CharacterId = characterId,
                LastUpdated = DateTime.UtcNow
            };
            
            if (queueData != null && queueData.QueueEntries.Any())
            {
                // Queue efficiency analysis
                analysis.QueueEfficiency = CalculateQueueEfficiency(queueData, characterData);
                
                // Training time analysis
                analysis.TrainingTimeBreakdown = CalculateTrainingTimeBreakdown(queueData);
                
                // Optimization suggestions
                analysis.OptimizationSuggestions = GenerateOptimizationSuggestions(queueData, characterData);
                
                // Attribute recommendations
                analysis.AttributeRecommendations = GenerateAttributeRecommendations(queueData, characterData);
                
                // Queue warnings
                analysis.QueueWarnings = DetectQueueWarnings(queueData);
            }
            
            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing skill queue for character {CharacterId}", characterId);
            
            return new SkillQueueAnalysis
            {
                CharacterId = characterId,
                LastUpdated = DateTime.UtcNow,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Get skill training history and statistics
    /// </summary>
    public async Task<SkillTrainingHistory> GetTrainingHistoryAsync(long characterId, int days = 90, CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            
            var completedSkills = await _unitOfWork.SkillTrainingHistory
                .FindAsync(sth => sth.CharacterId == characterId && sth.CompletedDate >= cutoffDate, cancellationToken);
            
            var history = new SkillTrainingHistory
            {
                CharacterId = characterId,
                AnalysisPeriod = TimeSpan.FromDays(days),
                LastUpdated = DateTime.UtcNow,
                CompletedSkills = completedSkills.OrderByDescending(cs => cs.CompletedDate).ToList()
            };
            
            // Calculate statistics
            history.TotalSkillsCompleted = completedSkills.Count();
            history.TotalSPGained = completedSkills.Sum(cs => cs.SPGained);
            history.AverageTrainingTime = completedSkills.Any() ? 
                TimeSpan.FromTicks((long)completedSkills.Average(cs => cs.TrainingDuration.Ticks)) : 
                TimeSpan.Zero;
            
            // Skills by category
            history.SkillsByCategory = completedSkills
                .GroupBy(cs => cs.SkillCategory)
                .ToDictionary(g => g.Key, g => g.Count());
            
            // Training velocity (SP per day)
            history.TrainingVelocity = days > 0 ? (double)history.TotalSPGained / days : 0;
            
            return history;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving training history for character {CharacterId}", characterId);
            
            return new SkillTrainingHistory
            {
                CharacterId = characterId,
                LastUpdated = DateTime.UtcNow,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Start real-time monitoring for a character's skill queue
    /// </summary>
    public async Task StartRealTimeMonitoringAsync(long characterId, CancellationToken cancellationToken = default)
    {
        try
        {
            var queueData = await GetCachedSkillQueueAsync(characterId, cancellationToken);
            if (queueData != null)
            {
                await SetupRealTimeMonitoringAsync(characterId, queueData, cancellationToken);
                _logger.LogInformation("Started real-time monitoring for character {CharacterId}", characterId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting real-time monitoring for character {CharacterId}", characterId);
        }
    }

    /// <summary>
    /// Stop real-time monitoring for a character
    /// </summary>
    public void StopRealTimeMonitoring(long characterId)
    {
        try
        {
            if (_realTimeTimers.TryRemove(characterId, out var timer))
            {
                timer.Dispose();
                _logger.LogInformation("Stopped real-time monitoring for character {CharacterId}", characterId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping real-time monitoring for character {CharacterId}", characterId);
        }
    }

    #region Private Methods

    private async Task<SkillQueueData?> RetrieveSkillQueueFromEsiAsync(long characterId, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            var queueResponse = await _esiSkillsService.GetCharacterSkillQueueAsync(characterId, accessToken, cancellationToken);
            
            var queueData = new SkillQueueData
            {
                CharacterId = characterId,
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.Success,
                QueueEntries = new List<SkillQueueEntry>()
            };
            
            foreach (var entry in queueResponse.OrderBy(e => e.queue_position))
            {
                var skillName = await GetSkillNameAsync(entry.skill_id, cancellationToken);
                
                queueData.QueueEntries.Add(new SkillQueueEntry
                {
                    CharacterId = characterId,
                    QueuePosition = entry.queue_position,
                    SkillId = entry.skill_id,
                    SkillName = skillName,
                    TrainedLevel = entry.trained_skill_level,
                    QueuedLevel = entry.finished_level,
                    TrainingStartDate = entry.start_date,
                    TrainingEndDate = entry.finish_date,
                    LevelStartSP = entry.level_start_sp,
                    LevelEndSP = entry.level_end_sp
                });
            }
            
            return queueData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving skill queue from ESI for character {CharacterId}", characterId);
            return null;
        }
    }

    private async Task<SkillQueueData?> GetCachedSkillQueueAsync(long characterId, CancellationToken cancellationToken)
    {
        try
        {
            var cachedQueue = await _unitOfWork.SkillQueues
                .FirstOrDefaultAsync(sq => sq.CharacterId == characterId, cancellationToken);
            
            if (cachedQueue != null)
            {
                var entries = await _unitOfWork.SkillQueueEntries
                    .FindAsync(sqe => sqe.CharacterId == characterId, cancellationToken);
                
                cachedQueue.QueueEntries = entries.OrderBy(e => e.QueuePosition).ToList();
            }
            
            return cachedQueue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached skill queue for character {CharacterId}", characterId);
            return null;
        }
    }

    private async Task StoreSkillQueueAsync(SkillQueueData queueData, CancellationToken cancellationToken)
    {
        try
        {
            // Update main queue record
            var existing = await _unitOfWork.SkillQueues
                .FirstOrDefaultAsync(sq => sq.CharacterId == queueData.CharacterId, cancellationToken);
            
            if (existing != null)
            {
                existing.LastUpdated = queueData.LastUpdated;
                existing.SyncStatus = queueData.SyncStatus;
                existing.ErrorMessage = queueData.ErrorMessage;
                _unitOfWork.SkillQueues.Update(existing);
            }
            else
            {
                await _unitOfWork.SkillQueues.AddAsync(queueData, cancellationToken);
            }
            
            // Clear existing queue entries
            var existingEntries = await _unitOfWork.SkillQueueEntries
                .FindAsync(sqe => sqe.CharacterId == queueData.CharacterId, cancellationToken);
            
            foreach (var entry in existingEntries)
            {
                await _unitOfWork.SkillQueueEntries.DeleteAsync(entry, cancellationToken);
            }
            
            // Add new queue entries
            foreach (var entry in queueData.QueueEntries)
            {
                await _unitOfWork.SkillQueueEntries.AddAsync(entry, cancellationToken);
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing skill queue for character {CharacterId}", queueData.CharacterId);
            throw;
        }
    }

    private async Task SetupRealTimeMonitoringAsync(long characterId, SkillQueueData queueData, CancellationToken cancellationToken)
    {
        try
        {
            // Stop existing timer if any
            StopRealTimeMonitoring(characterId);
            
            if (!queueData.QueueEntries.Any()) return;
            
            var activeSkill = queueData.QueueEntries.FirstOrDefault();
            if (activeSkill?.TrainingEndDate == null) return;
            
            // Calculate next update interval (every 30 seconds for active training)
            var updateInterval = TimeSpan.FromSeconds(30);
            
            var timer = new Timer(async _ =>
            {
                try
                {
                    await UpdateRealTimeProgressAsync(characterId, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in real-time progress update for character {CharacterId}", characterId);
                }
            }, null, updateInterval, updateInterval);
            
            _realTimeTimers[characterId] = timer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up real-time monitoring for character {CharacterId}", characterId);
        }
    }

    private async Task UpdateRealTimeProgressAsync(long characterId, CancellationToken cancellationToken)
    {
        try
        {
            var queueData = await GetCachedSkillQueueAsync(characterId, cancellationToken);
            if (queueData?.QueueEntries?.Any() != true) return;
            
            var activeSkill = queueData.QueueEntries.FirstOrDefault();
            if (activeSkill?.TrainingEndDate == null) return;
            
            var now = DateTime.UtcNow;
            
            // Check if skill completed
            if (now >= activeSkill.TrainingEndDate)
            {
                await HandleSkillCompletionAsync(characterId, activeSkill, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating real-time progress for character {CharacterId}", characterId);
        }
    }

    private async Task HandleSkillCompletionAsync(long characterId, SkillQueueEntry completedSkill, CancellationToken cancellationToken)
    {
        try
        {
            // Record skill completion
            var historyEntry = new SkillTrainingHistoryEntry
            {
                CharacterId = characterId,
                SkillId = completedSkill.SkillId,
                SkillName = completedSkill.SkillName,
                CompletedLevel = completedSkill.QueuedLevel,
                CompletedDate = DateTime.UtcNow,
                TrainingDuration = completedSkill.TrainingEndDate - completedSkill.TrainingStartDate,
                SPGained = completedSkill.LevelEndSP - completedSkill.LevelStartSP,
                SkillCategory = await GetSkillCategoryAsync(completedSkill.SkillId, cancellationToken)
            };
            
            await _unitOfWork.SkillTrainingHistory.AddAsync(historyEntry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            await _auditService.LogActionAsync("skill_completed", "Character", 
                $"{characterId}:{completedSkill.SkillId}:{completedSkill.QueuedLevel}", cancellationToken);
            
            _logger.LogInformation("Skill {SkillName} Level {Level} completed for character {CharacterId}", 
                completedSkill.SkillName, completedSkill.QueuedLevel, characterId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling skill completion for character {CharacterId}", characterId);
        }
    }

    private long CalculateCurrentSP(SkillQueueEntry skill, DateTime now)
    {
        if (now <= skill.TrainingStartDate) return skill.LevelStartSP;
        if (now >= skill.TrainingEndDate) return skill.LevelEndSP;
        
        var totalTrainingTime = skill.TrainingEndDate - skill.TrainingStartDate;
        var elapsedTime = now - skill.TrainingStartDate;
        var progress = (double)elapsedTime.Ticks / totalTrainingTime.Ticks;
        
        var totalSP = skill.LevelEndSP - skill.LevelStartSP;
        return skill.LevelStartSP + (long)(totalSP * progress);
    }

    private double CalculateCompletionPercentage(SkillQueueEntry skill, DateTime now)
    {
        if (now <= skill.TrainingStartDate) return 0.0;
        if (now >= skill.TrainingEndDate) return 100.0;
        
        var totalTrainingTime = skill.TrainingEndDate - skill.TrainingStartDate;
        var elapsedTime = now - skill.TrainingStartDate;
        return (double)elapsedTime.Ticks / totalTrainingTime.Ticks * 100.0;
    }

    private async Task<CharacterAttributeData?> GetCharacterAttributesAsync(long characterId, CancellationToken cancellationToken)
    {
        try
        {
            return await _unitOfWork.CharacterAttributes
                .FirstOrDefaultAsync(ca => ca.CharacterId == characterId, cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private async Task<string> GetSkillNameAsync(int skillId, CancellationToken cancellationToken)
    {
        try
        {
            var skill = await _unitOfWork.EveTypes
                .FirstOrDefaultAsync(t => t.TypeId == skillId, cancellationToken);
            
            return skill?.TypeName ?? $"Unknown Skill ({skillId})";
        }
        catch
        {
            return $"Unknown Skill ({skillId})";
        }
    }

    private async Task<string> GetSkillCategoryAsync(int skillId, CancellationToken cancellationToken)
    {
        try
        {
            var skill = await _unitOfWork.EveTypes
                .FirstOrDefaultAsync(t => t.TypeId == skillId, cancellationToken);
            
            if (skill?.GroupId != null)
            {
                var group = await _unitOfWork.EveGroups
                    .FirstOrDefaultAsync(g => g.GroupId == skill.GroupId, cancellationToken);
                
                return group?.GroupName ?? "Unknown Category";
            }
            
            return "Unknown Category";
        }
        catch
        {
            return "Unknown Category";
        }
    }

    private QueueEfficiencyMetrics CalculateQueueEfficiency(SkillQueueData queueData, CharacterAttributeData? attributeData)
    {
        // Calculate training efficiency based on attributes and skill requirements
        var metrics = new QueueEfficiencyMetrics();
        
        if (attributeData != null && queueData.QueueEntries.Any())
        {
            var totalOptimalTime = 0.0;
            var totalActualTime = 0.0;
            
            foreach (var skill in queueData.QueueEntries)
            {
                var trainingTime = (skill.TrainingEndDate - skill.TrainingStartDate).TotalHours;
                totalActualTime += trainingTime;
                
                // Calculate optimal time with perfect attributes (simplified)
                var optimalTime = trainingTime * 0.8; // Assume 20% improvement possible
                totalOptimalTime += optimalTime;
            }
            
            metrics.EfficiencyRating = totalOptimalTime > 0 ? (totalOptimalTime / totalActualTime) * 100 : 100;
            metrics.PotentialTimeSavings = TimeSpan.FromHours(totalActualTime - totalOptimalTime);
        }
        
        return metrics;
    }

    private TrainingTimeBreakdown CalculateTrainingTimeBreakdown(SkillQueueData queueData)
    {
        var breakdown = new TrainingTimeBreakdown();
        
        foreach (var skill in queueData.QueueEntries)
        {
            var trainingTime = skill.TrainingEndDate - skill.TrainingStartDate;
            
            switch (skill.QueuedLevel)
            {
                case 1: breakdown.Level1Time += trainingTime; break;
                case 2: breakdown.Level2Time += trainingTime; break;
                case 3: breakdown.Level3Time += trainingTime; break;
                case 4: breakdown.Level4Time += trainingTime; break;
                case 5: breakdown.Level5Time += trainingTime; break;
            }
        }
        
        return breakdown;
    }

    private List<OptimizationSuggestion> GenerateOptimizationSuggestions(SkillQueueData queueData, CharacterAttributeData? attributeData)
    {
        var suggestions = new List<OptimizationSuggestion>();
        
        // Check for attribute optimization opportunities
        if (attributeData != null)
        {
            suggestions.Add(new OptimizationSuggestion
            {
                Type = "Attribute Optimization",
                Description = "Consider remapping attributes to optimize training time",
                PotentialSavings = TimeSpan.FromDays(5), // Estimated
                Priority = "Medium"
            });
        }
        
        // Check for skill order optimization
        var level5Skills = queueData.QueueEntries.Where(e => e.QueuedLevel == 5).Count();
        if (level5Skills > 3)
        {
            suggestions.Add(new OptimizationSuggestion
            {
                Type = "Queue Order",
                Description = "Consider training Level 4 skills first for immediate benefits",
                Priority = "Low"
            });
        }
        
        return suggestions;
    }

    private List<AttributeRecommendation> GenerateAttributeRecommendations(SkillQueueData queueData, CharacterAttributeData? attributeData)
    {
        var recommendations = new List<AttributeRecommendation>();
        
        // Simplified attribute analysis
        var primaryAttributes = queueData.QueueEntries
            .GroupBy(e => "Intelligence") // Would need real skill attribute data
            .OrderByDescending(g => g.Count())
            .Take(2)
            .Select(g => g.Key)
            .ToList();
        
        if (primaryAttributes.Any())
        {
            recommendations.Add(new AttributeRecommendation
            {
                AttributeName = primaryAttributes.First(),
                CurrentValue = attributeData?.Intelligence ?? 20,
                RecommendedValue = 27,
                TimeSavings = TimeSpan.FromDays(3)
            });
        }
        
        return recommendations;
    }

    private List<QueueWarning> DetectQueueWarnings(SkillQueueData queueData)
    {
        var warnings = new List<QueueWarning>();
        
        // Check queue length
        if (queueData.QueueEntries.Count < 5)
        {
            warnings.Add(new QueueWarning
            {
                Type = "Queue Length",
                Message = "Skill queue is running low - consider adding more skills",
                Severity = "Low"
            });
        }
        
        // Check for very long skills
        var longSkills = queueData.QueueEntries
            .Where(e => (e.TrainingEndDate - e.TrainingStartDate).TotalDays > 30)
            .ToList();
        
        if (longSkills.Any())
        {
            warnings.Add(new QueueWarning
            {
                Type = "Long Training",
                Message = $"{longSkills.Count} skills will take over 30 days to train",
                Severity = "Medium"
            });
        }
        
        return warnings;
    }

    #endregion

    public void Dispose()
    {
        foreach (var timer in _realTimeTimers.Values)
        {
            timer?.Dispose();
        }
        _realTimeTimers.Clear();
    }
}

#region Skill Queue Data Structures

/// <summary>
/// Complete skill queue data
/// </summary>
public class SkillQueueData
{
    public long CharacterId { get; set; }
    public List<SkillQueueEntry> QueueEntries { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public SyncStatus SyncStatus { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Individual skill queue entry
/// </summary>
public class SkillQueueEntry
{
    public long CharacterId { get; set; }
    public int QueuePosition { get; set; }
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int TrainedLevel { get; set; }
    public int QueuedLevel { get; set; }
    public DateTime TrainingStartDate { get; set; }
    public DateTime TrainingEndDate { get; set; }
    public long LevelStartSP { get; set; }
    public long LevelEndSP { get; set; }
}

/// <summary>
/// Real-time skill queue progress
/// </summary>
public class SkillQueueProgress
{
    public long CharacterId { get; set; }
    public bool QueueEmpty { get; set; }
    public CurrentSkillTraining? CurrentlyTraining { get; set; }
    public int TotalSkillsInQueue { get; set; }
    public TimeSpan? TotalTrainingTime { get; set; }
    public DateTime? EstimatedCompletion { get; set; }
    public List<UpcomingSkill> UpcomingSkills { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Currently training skill details
/// </summary>
public class CurrentSkillTraining
{
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int TrainedLevel { get; set; }
    public int QueuedLevel { get; set; }
    public DateTime TrainingStartDate { get; set; }
    public DateTime TrainingEndDate { get; set; }
    public long LevelStartSP { get; set; }
    public long LevelEndSP { get; set; }
    public long CurrentSP { get; set; }
    public TimeSpan? RemainingTime { get; set; }
    public double CompletionPercentage { get; set; }
}

/// <summary>
/// Upcoming skill in queue
/// </summary>
public class UpcomingSkill
{
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int QueuedLevel { get; set; }
    public DateTime TrainingEndDate { get; set; }
    public TimeSpan? TimeToStart { get; set; }
}

/// <summary>
/// Skill queue analysis and optimization
/// </summary>
public class SkillQueueAnalysis
{
    public long CharacterId { get; set; }
    public QueueEfficiencyMetrics QueueEfficiency { get; set; } = new();
    public TrainingTimeBreakdown TrainingTimeBreakdown { get; set; } = new();
    public List<OptimizationSuggestion> OptimizationSuggestions { get; set; } = new();
    public List<AttributeRecommendation> AttributeRecommendations { get; set; } = new();
    public List<QueueWarning> QueueWarnings { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Queue efficiency metrics
/// </summary>
public class QueueEfficiencyMetrics
{
    public double EfficiencyRating { get; set; }
    public TimeSpan PotentialTimeSavings { get; set; }
}

/// <summary>
/// Training time breakdown by level
/// </summary>
public class TrainingTimeBreakdown
{
    public TimeSpan Level1Time { get; set; }
    public TimeSpan Level2Time { get; set; }
    public TimeSpan Level3Time { get; set; }
    public TimeSpan Level4Time { get; set; }
    public TimeSpan Level5Time { get; set; }
}

/// <summary>
/// Optimization suggestion
/// </summary>
public class OptimizationSuggestion
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TimeSpan? PotentialSavings { get; set; }
    public string Priority { get; set; } = string.Empty;
}

/// <summary>
/// Attribute recommendation
/// </summary>
public class AttributeRecommendation
{
    public string AttributeName { get; set; } = string.Empty;
    public int CurrentValue { get; set; }
    public int RecommendedValue { get; set; }
    public TimeSpan TimeSavings { get; set; }
}

/// <summary>
/// Queue warning
/// </summary>
public class QueueWarning
{
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
}

/// <summary>
/// Skill training history
/// </summary>
public class SkillTrainingHistory
{
    public long CharacterId { get; set; }
    public TimeSpan AnalysisPeriod { get; set; }
    public List<SkillTrainingHistoryEntry> CompletedSkills { get; set; } = new();
    public int TotalSkillsCompleted { get; set; }
    public long TotalSPGained { get; set; }
    public TimeSpan AverageTrainingTime { get; set; }
    public Dictionary<string, int> SkillsByCategory { get; set; } = new();
    public double TrainingVelocity { get; set; }
    public DateTime LastUpdated { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Skill training history entry
/// </summary>
public class SkillTrainingHistoryEntry
{
    public long CharacterId { get; set; }
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int CompletedLevel { get; set; }
    public DateTime CompletedDate { get; set; }
    public TimeSpan TrainingDuration { get; set; }
    public long SPGained { get; set; }
    public string SkillCategory { get; set; } = string.Empty;
}

/// <summary>
/// Character attribute data
/// </summary>
public class CharacterAttributeData
{
    public long CharacterId { get; set; }
    public int Intelligence { get; set; }
    public int Memory { get; set; }
    public int Charisma { get; set; }
    public int Perception { get; set; }
    public int Willpower { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// ESI skill queue response entry
/// </summary>
public class EsiSkillQueueEntry
{
    public int queue_position { get; set; }
    public int skill_id { get; set; }
    public int trained_skill_level { get; set; }
    public int finished_level { get; set; }
    public DateTime start_date { get; set; }
    public DateTime finish_date { get; set; }
    public long level_start_sp { get; set; }
    public long level_end_sp { get; set; }
}

/// <summary>
/// Character skill queue service interface
/// </summary>
public interface ICharacterSkillQueueService : IDisposable
{
    Task<SkillQueueData> SynchronizeSkillQueueAsync(long characterId, string accessToken, CancellationToken cancellationToken = default);
    Task<SkillQueueProgress> GetRealTimeProgressAsync(long characterId, CancellationToken cancellationToken = default);
    Task<SkillQueueAnalysis> GetQueueAnalysisAsync(long characterId, CancellationToken cancellationToken = default);
    Task<SkillTrainingHistory> GetTrainingHistoryAsync(long characterId, int days = 90, CancellationToken cancellationToken = default);
    Task StartRealTimeMonitoringAsync(long characterId, CancellationToken cancellationToken = default);
    void StopRealTimeMonitoring(long characterId);
}

/// <summary>
/// Contact lists and standings synchronization service
/// </summary>
public class CharacterContactsService : ICharacterContactsService
{
    private readonly IEsiApiService _esiApiService;
    private readonly IEsiCharacterService _esiCharacterService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CharacterContactsService> _logger;
    private readonly IAuditLogService _auditService;
    private readonly IEsiDegradationService _degradationService;
    
    private readonly ConcurrentDictionary<long, DateTime> _contactSyncTimes = new();
    private readonly ConcurrentDictionary<long, DateTime> _standingsSyncTimes = new();

    public CharacterContactsService(
        IEsiApiService esiApiService,
        IEsiCharacterService esiCharacterService,
        IUnitOfWork unitOfWork,
        ILogger<CharacterContactsService> logger,
        IAuditLogService auditService,
        IEsiDegradationService degradationService)
    {
        _esiApiService = esiApiService ?? throw new ArgumentNullException(nameof(esiApiService));
        _esiCharacterService = esiCharacterService ?? throw new ArgumentNullException(nameof(esiCharacterService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _degradationService = degradationService ?? throw new ArgumentNullException(nameof(degradationService));
    }

    /// <summary>
    /// Synchronize character contacts from ESI
    /// </summary>
    public async Task<ContactsData> SynchronizeContactsAsync(long characterId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Synchronizing contacts for character {CharacterId}", characterId);
            
            // Check if we need to update (once per 30 minutes for contacts)
            if (_contactSyncTimes.TryGetValue(characterId, out var lastSync) && 
                DateTime.UtcNow - lastSync < TimeSpan.FromMinutes(30))
            {
                var cached = await GetCachedContactsAsync(characterId, cancellationToken);
                if (cached != null) return cached;
            }
            
            var contactsData = await _degradationService.ExecuteWithFallbackAsync(
                async (ct) => await RetrieveContactsFromEsiAsync(characterId, accessToken, ct),
                async (ct) => await GetCachedContactsAsync(characterId, ct),
                $"contacts_{characterId}",
                cancellationToken);
            
            if (contactsData != null)
            {
                // Store in database
                await StoreContactsAsync(contactsData, cancellationToken);
                _contactSyncTimes[characterId] = DateTime.UtcNow;
                
                await _auditService.LogActionAsync("contacts_synced", "Character", 
                    characterId.ToString(), cancellationToken);
            }
            
            return contactsData ?? new ContactsData 
            { 
                CharacterId = characterId, 
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = "Failed to retrieve contacts data"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing contacts for character {CharacterId}", characterId);
            
            return new ContactsData 
            { 
                CharacterId = characterId, 
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Synchronize character standings from ESI
    /// </summary>
    public async Task<StandingsData> SynchronizeStandingsAsync(long characterId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Synchronizing standings for character {CharacterId}", characterId);
            
            // Check if we need to update (once per 60 minutes for standings)
            if (_standingsSyncTimes.TryGetValue(characterId, out var lastSync) && 
                DateTime.UtcNow - lastSync < TimeSpan.FromMinutes(60))
            {
                var cached = await GetCachedStandingsAsync(characterId, cancellationToken);
                if (cached != null) return cached;
            }
            
            var standingsData = await _degradationService.ExecuteWithFallbackAsync(
                async (ct) => await RetrieveStandingsFromEsiAsync(characterId, accessToken, ct),
                async (ct) => await GetCachedStandingsAsync(characterId, ct),
                $"standings_{characterId}",
                cancellationToken);
            
            if (standingsData != null)
            {
                // Store in database
                await StoreStandingsAsync(standingsData, cancellationToken);
                _standingsSyncTimes[characterId] = DateTime.UtcNow;
                
                await _auditService.LogActionAsync("standings_synced", "Character", 
                    characterId.ToString(), cancellationToken);
            }
            
            return standingsData ?? new StandingsData 
            { 
                CharacterId = characterId, 
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = "Failed to retrieve standings data"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing standings for character {CharacterId}", characterId);
            
            return new StandingsData 
            { 
                CharacterId = characterId, 
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Get comprehensive diplomatic summary
    /// </summary>
    public async Task<DiplomaticSummary> GetDiplomaticSummaryAsync(long characterId, CancellationToken cancellationToken = default)
    {
        try
        {
            var contactsData = await GetCachedContactsAsync(characterId, cancellationToken);
            var standingsData = await GetCachedStandingsAsync(characterId, cancellationToken);
            
            var summary = new DiplomaticSummary
            {
                CharacterId = characterId,
                LastUpdated = DateTime.UtcNow
            };
            
            if (contactsData != null)
            {
                summary.TotalContacts = contactsData.Contacts.Count;
                summary.ContactsByType = contactsData.Contacts
                    .GroupBy(c => c.ContactType)
                    .ToDictionary(g => g.Key, g => g.Count());
                
                summary.ContactsByStanding = contactsData.Contacts
                    .GroupBy(c => GetStandingCategory(c.Standing))
                    .ToDictionary(g => g.Key, g => g.Count());
                
                summary.RecentContacts = contactsData.Contacts
                    .Where(c => c.IsWatched || Math.Abs(c.Standing) >= 5.0)
                    .OrderByDescending(c => Math.Abs(c.Standing))
                    .Take(10)
                    .ToList();
            }
            
            if (standingsData != null)
            {
                summary.TotalStandings = standingsData.Standings.Count;
                summary.PositiveStandings = standingsData.Standings.Count(s => s.Standing > 0);
                summary.NegativeStandings = standingsData.Standings.Count(s => s.Standing < 0);
                summary.NeutralStandings = standingsData.Standings.Count(s => s.Standing == 0);
                
                summary.CorporationStandings = standingsData.Standings
                    .Where(s => s.FromType == "corporation")
                    .OrderByDescending(s => s.Standing)
                    .Take(10)
                    .ToList();
                
                summary.AllianceStandings = standingsData.Standings
                    .Where(s => s.FromType == "alliance")
                    .OrderByDescending(s => s.Standing)
                    .Take(10)
                    .ToList();
            }
            
            // Security analysis
            summary.SecurityAnalysis = await AnalyzeSecurityRisksAsync(contactsData, standingsData, cancellationToken);
            
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating diplomatic summary for character {CharacterId}", characterId);
            
            return new DiplomaticSummary
            {
                CharacterId = characterId,
                LastUpdated = DateTime.UtcNow,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Search contacts by name or standing criteria
    /// </summary>
    public async Task<List<ContactEntry>> SearchContactsAsync(long characterId, string searchTerm, double? minStanding = null, double? maxStanding = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var contactsData = await GetCachedContactsAsync(characterId, cancellationToken);
            if (contactsData == null) return new List<ContactEntry>();
            
            var query = contactsData.Contacts.AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c => c.ContactName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }
            
            if (minStanding.HasValue)
            {
                query = query.Where(c => c.Standing >= minStanding.Value);
            }
            
            if (maxStanding.HasValue)
            {
                query = query.Where(c => c.Standing <= maxStanding.Value);
            }
            
            return query.OrderByDescending(c => Math.Abs(c.Standing))
                       .ThenBy(c => c.ContactName)
                       .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching contacts for character {CharacterId}", characterId);
            return new List<ContactEntry>();
        }
    }

    /// <summary>
    /// Get contact and standing history for analysis
    /// </summary>
    public async Task<ContactHistory> GetContactHistoryAsync(long characterId, int days = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            
            var contactChanges = await _unitOfWork.ContactChanges
                .FindAsync(cc => cc.CharacterId == characterId && cc.ChangeDate >= cutoffDate, cancellationToken);
            
            var standingChanges = await _unitOfWork.StandingChanges
                .FindAsync(sc => sc.CharacterId == characterId && sc.ChangeDate >= cutoffDate, cancellationToken);
            
            var history = new ContactHistory
            {
                CharacterId = characterId,
                AnalysisPeriod = TimeSpan.FromDays(days),
                LastUpdated = DateTime.UtcNow,
                ContactChanges = contactChanges.OrderByDescending(cc => cc.ChangeDate).ToList(),
                StandingChanges = standingChanges.OrderByDescending(sc => sc.ChangeDate).ToList()
            };
            
            // Calculate statistics
            history.TotalContactChanges = contactChanges.Count();
            history.TotalStandingChanges = standingChanges.Count();
            history.ContactsAdded = contactChanges.Count(cc => cc.ChangeType == "Added");
            history.ContactsRemoved = contactChanges.Count(cc => cc.ChangeType == "Removed");
            history.ContactsModified = contactChanges.Count(cc => cc.ChangeType == "Modified");
            
            return history;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contact history for character {CharacterId}", characterId);
            
            return new ContactHistory
            {
                CharacterId = characterId,
                LastUpdated = DateTime.UtcNow,
                ErrorMessage = ex.Message
            };
        }
    }

    #region Private Methods

    private async Task<ContactsData?> RetrieveContactsFromEsiAsync(long characterId, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            var contactsResponse = await _esiCharacterService.GetCharacterContactsAsync(characterId, accessToken, cancellationToken);
            
            var contactsData = new ContactsData
            {
                CharacterId = characterId,
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.Success,
                Contacts = new List<ContactEntry>()
            };
            
            foreach (var contact in contactsResponse)
            {
                var contactName = await GetContactNameAsync(contact.contact_id, contact.contact_type, cancellationToken);
                
                contactsData.Contacts.Add(new ContactEntry
                {
                    CharacterId = characterId,
                    ContactId = contact.contact_id,
                    ContactName = contactName,
                    ContactType = contact.contact_type,
                    Standing = contact.standing,
                    IsWatched = contact.is_watched,
                    IsBlocked = contact.is_blocked,
                    LabelIds = contact.label_ids ?? new List<long>()
                });
            }
            
            return contactsData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contacts from ESI for character {CharacterId}", characterId);
            return null;
        }
    }

    private async Task<StandingsData?> RetrieveStandingsFromEsiAsync(long characterId, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            var standingsResponse = await _esiCharacterService.GetCharacterStandingsAsync(characterId, accessToken, cancellationToken);
            
            var standingsData = new StandingsData
            {
                CharacterId = characterId,
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.Success,
                Standings = new List<StandingEntry>()
            };
            
            foreach (var standing in standingsResponse)
            {
                var fromName = await GetContactNameAsync(standing.from_id, standing.from_type, cancellationToken);
                
                standingsData.Standings.Add(new StandingEntry
                {
                    CharacterId = characterId,
                    FromId = standing.from_id,
                    FromName = fromName,
                    FromType = standing.from_type,
                    Standing = standing.standing
                });
            }
            
            return standingsData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving standings from ESI for character {CharacterId}", characterId);
            return null;
        }
    }

    private async Task<ContactsData?> GetCachedContactsAsync(long characterId, CancellationToken cancellationToken)
    {
        try
        {
            var cachedContacts = await _unitOfWork.CharacterContacts
                .FirstOrDefaultAsync(cc => cc.CharacterId == characterId, cancellationToken);
            
            if (cachedContacts != null)
            {
                var contacts = await _unitOfWork.ContactEntries
                    .FindAsync(ce => ce.CharacterId == characterId, cancellationToken);
                
                cachedContacts.Contacts = contacts.ToList();
            }
            
            return cachedContacts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached contacts for character {CharacterId}", characterId);
            return null;
        }
    }

    private async Task<StandingsData?> GetCachedStandingsAsync(long characterId, CancellationToken cancellationToken)
    {
        try
        {
            var cachedStandings = await _unitOfWork.CharacterStandings
                .FirstOrDefaultAsync(cs => cs.CharacterId == characterId, cancellationToken);
            
            if (cachedStandings != null)
            {
                var standings = await _unitOfWork.StandingEntries
                    .FindAsync(se => se.CharacterId == characterId, cancellationToken);
                
                cachedStandings.Standings = standings.ToList();
            }
            
            return cachedStandings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached standings for character {CharacterId}", characterId);
            return null;
        }
    }

    private async Task StoreContactsAsync(ContactsData contactsData, CancellationToken cancellationToken)
    {
        try
        {
            // Update main contacts record
            var existing = await _unitOfWork.CharacterContacts
                .FirstOrDefaultAsync(cc => cc.CharacterId == contactsData.CharacterId, cancellationToken);
            
            if (existing != null)
            {
                existing.LastUpdated = contactsData.LastUpdated;
                existing.SyncStatus = contactsData.SyncStatus;
                existing.ErrorMessage = contactsData.ErrorMessage;
                _unitOfWork.CharacterContacts.Update(existing);
            }
            else
            {
                await _unitOfWork.CharacterContacts.AddAsync(contactsData, cancellationToken);
            }
            
            // Track changes for history
            var existingContacts = await _unitOfWork.ContactEntries
                .FindAsync(ce => ce.CharacterId == contactsData.CharacterId, cancellationToken);
            
            await TrackContactChangesAsync(contactsData.CharacterId, existingContacts.ToList(), contactsData.Contacts, cancellationToken);
            
            // Clear existing contact entries
            foreach (var entry in existingContacts)
            {
                await _unitOfWork.ContactEntries.DeleteAsync(entry, cancellationToken);
            }
            
            // Add new contact entries
            foreach (var contact in contactsData.Contacts)
            {
                await _unitOfWork.ContactEntries.AddAsync(contact, cancellationToken);
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing contacts for character {CharacterId}", contactsData.CharacterId);
            throw;
        }
    }

    private async Task StoreStandingsAsync(StandingsData standingsData, CancellationToken cancellationToken)
    {
        try
        {
            // Update main standings record
            var existing = await _unitOfWork.CharacterStandings
                .FirstOrDefaultAsync(cs => cs.CharacterId == standingsData.CharacterId, cancellationToken);
            
            if (existing != null)
            {
                existing.LastUpdated = standingsData.LastUpdated;
                existing.SyncStatus = standingsData.SyncStatus;
                existing.ErrorMessage = standingsData.ErrorMessage;
                _unitOfWork.CharacterStandings.Update(existing);
            }
            else
            {
                await _unitOfWork.CharacterStandings.AddAsync(standingsData, cancellationToken);
            }
            
            // Track changes for history
            var existingStandings = await _unitOfWork.StandingEntries
                .FindAsync(se => se.CharacterId == standingsData.CharacterId, cancellationToken);
            
            await TrackStandingChangesAsync(standingsData.CharacterId, existingStandings.ToList(), standingsData.Standings, cancellationToken);
            
            // Clear existing standing entries
            foreach (var entry in existingStandings)
            {
                await _unitOfWork.StandingEntries.DeleteAsync(entry, cancellationToken);
            }
            
            // Add new standing entries
            foreach (var standing in standingsData.Standings)
            {
                await _unitOfWork.StandingEntries.AddAsync(standing, cancellationToken);
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing standings for character {CharacterId}", standingsData.CharacterId);
            throw;
        }
    }

    private async Task TrackContactChangesAsync(long characterId, List<ContactEntry> oldContacts, List<ContactEntry> newContacts, CancellationToken cancellationToken)
    {
        try
        {
            var oldDict = oldContacts.ToDictionary(c => c.ContactId);
            var newDict = newContacts.ToDictionary(c => c.ContactId);
            
            // Track additions
            foreach (var newContact in newContacts.Where(nc => !oldDict.ContainsKey(nc.ContactId)))
            {
                var change = new ContactChangeEntry
                {
                    CharacterId = characterId,
                    ContactId = newContact.ContactId,
                    ContactName = newContact.ContactName,
                    ChangeType = "Added",
                    ChangeDate = DateTime.UtcNow,
                    NewStanding = newContact.Standing
                };
                
                await _unitOfWork.ContactChanges.AddAsync(change, cancellationToken);
            }
            
            // Track removals
            foreach (var oldContact in oldContacts.Where(oc => !newDict.ContainsKey(oc.ContactId)))
            {
                var change = new ContactChangeEntry
                {
                    CharacterId = characterId,
                    ContactId = oldContact.ContactId,
                    ContactName = oldContact.ContactName,
                    ChangeType = "Removed",
                    ChangeDate = DateTime.UtcNow,
                    PreviousStanding = oldContact.Standing
                };
                
                await _unitOfWork.ContactChanges.AddAsync(change, cancellationToken);
            }
            
            // Track modifications
            foreach (var newContact in newContacts.Where(nc => oldDict.ContainsKey(nc.ContactId)))
            {
                var oldContact = oldDict[newContact.ContactId];
                if (Math.Abs(oldContact.Standing - newContact.Standing) > 0.01)
                {
                    var change = new ContactChangeEntry
                    {
                        CharacterId = characterId,
                        ContactId = newContact.ContactId,
                        ContactName = newContact.ContactName,
                        ChangeType = "Modified",
                        ChangeDate = DateTime.UtcNow,
                        PreviousStanding = oldContact.Standing,
                        NewStanding = newContact.Standing
                    };
                    
                    await _unitOfWork.ContactChanges.AddAsync(change, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking contact changes for character {CharacterId}", characterId);
        }
    }

    private async Task TrackStandingChangesAsync(long characterId, List<StandingEntry> oldStandings, List<StandingEntry> newStandings, CancellationToken cancellationToken)
    {
        try
        {
            var oldDict = oldStandings.ToDictionary(s => s.FromId);
            var newDict = newStandings.ToDictionary(s => s.FromId);
            
            // Track changes
            foreach (var newStanding in newStandings)
            {
                if (oldDict.TryGetValue(newStanding.FromId, out var oldStanding))
                {
                    if (Math.Abs(oldStanding.Standing - newStanding.Standing) > 0.01)
                    {
                        var change = new StandingChangeEntry
                        {
                            CharacterId = characterId,
                            FromId = newStanding.FromId,
                            FromName = newStanding.FromName,
                            FromType = newStanding.FromType,
                            ChangeDate = DateTime.UtcNow,
                            PreviousStanding = oldStanding.Standing,
                            NewStanding = newStanding.Standing
                        };
                        
                        await _unitOfWork.StandingChanges.AddAsync(change, cancellationToken);
                    }
                }
                else
                {
                    // New standing
                    var change = new StandingChangeEntry
                    {
                        CharacterId = characterId,
                        FromId = newStanding.FromId,
                        FromName = newStanding.FromName,
                        FromType = newStanding.FromType,
                        ChangeDate = DateTime.UtcNow,
                        NewStanding = newStanding.Standing
                    };
                    
                    await _unitOfWork.StandingChanges.AddAsync(change, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking standing changes for character {CharacterId}", characterId);
        }
    }

    private async Task<string> GetContactNameAsync(long contactId, string contactType, CancellationToken cancellationToken)
    {
        try
        {
            switch (contactType.ToLower())
            {
                case "character":
                    var character = await _unitOfWork.Characters
                        .FirstOrDefaultAsync(c => c.CharacterId == contactId, cancellationToken);
                    return character?.CharacterName ?? $"Unknown Character ({contactId})";
                    
                case "corporation":
                    var corporation = await _unitOfWork.Corporations
                        .FirstOrDefaultAsync(c => c.CorporationId == contactId, cancellationToken);
                    return corporation?.CorporationName ?? $"Unknown Corporation ({contactId})";
                    
                case "alliance":
                    var alliance = await _unitOfWork.Alliances
                        .FirstOrDefaultAsync(a => a.AllianceId == contactId, cancellationToken);
                    return alliance?.AllianceName ?? $"Unknown Alliance ({contactId})";
                    
                default:
                    return $"Unknown ({contactType}: {contactId})";
            }
        }
        catch
        {
            return $"Unknown ({contactType}: {contactId})";
        }
    }

    private string GetStandingCategory(double standing)
    {
        return standing switch
        {
            >= 5.0 => "Excellent",
            >= 0.1 => "Good",
            > -0.1 and < 0.1 => "Neutral",
            > -5.0 => "Bad",
            _ => "Terrible"
        };
    }

    private async Task<SecurityAnalysis> AnalyzeSecurityRisksAsync(ContactsData? contactsData, StandingsData? standingsData, CancellationToken cancellationToken)
    {
        try
        {
            var analysis = new SecurityAnalysis();
            
            if (contactsData != null)
            {
                analysis.HighRiskContacts = contactsData.Contacts
                    .Where(c => c.Standing <= -5.0)
                    .Count();
                
                analysis.WatchedContacts = contactsData.Contacts
                    .Where(c => c.IsWatched)
                    .Count();
                
                analysis.BlockedContacts = contactsData.Contacts
                    .Where(c => c.IsBlocked)
                    .Count();
            }
            
            if (standingsData != null)
            {
                analysis.NegativeStandingEntities = standingsData.Standings
                    .Where(s => s.Standing < -5.0)
                    .Count();
                
                analysis.HostileCorporations = standingsData.Standings
                    .Where(s => s.FromType == "corporation" && s.Standing <= -5.0)
                    .Count();
                
                analysis.HostileAlliances = standingsData.Standings
                    .Where(s => s.FromType == "alliance" && s.Standing <= -5.0)
                    .Count();
            }
            
            // Calculate overall risk level
            var riskFactors = analysis.HighRiskContacts + analysis.NegativeStandingEntities + 
                             analysis.HostileCorporations + analysis.HostileAlliances;
            
            analysis.OverallRiskLevel = riskFactors switch
            {
                0 => "Low",
                <= 5 => "Medium",
                <= 15 => "High",
                _ => "Critical"
            };
            
            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing security risks");
            return new SecurityAnalysis { OverallRiskLevel = "Unknown" };
        }
    }

    #endregion
}

#region Contacts and Standings Data Structures

/// <summary>
/// Complete contacts data
/// </summary>
public class ContactsData
{
    public long CharacterId { get; set; }
    public List<ContactEntry> Contacts { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public SyncStatus SyncStatus { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Individual contact entry
/// </summary>
public class ContactEntry
{
    public long CharacterId { get; set; }
    public long ContactId { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string ContactType { get; set; } = string.Empty;
    public double Standing { get; set; }
    public bool IsWatched { get; set; }
    public bool IsBlocked { get; set; }
    public List<long> LabelIds { get; set; } = new();
}

/// <summary>
/// Complete standings data
/// </summary>
public class StandingsData
{
    public long CharacterId { get; set; }
    public List<StandingEntry> Standings { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public SyncStatus SyncStatus { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Individual standing entry
/// </summary>
public class StandingEntry
{
    public long CharacterId { get; set; }
    public long FromId { get; set; }
    public string FromName { get; set; } = string.Empty;
    public string FromType { get; set; } = string.Empty;
    public double Standing { get; set; }
}

/// <summary>
/// Comprehensive diplomatic summary
/// </summary>
public class DiplomaticSummary
{
    public long CharacterId { get; set; }
    public int TotalContacts { get; set; }
    public int TotalStandings { get; set; }
    public int PositiveStandings { get; set; }
    public int NegativeStandings { get; set; }
    public int NeutralStandings { get; set; }
    public Dictionary<string, int> ContactsByType { get; set; } = new();
    public Dictionary<string, int> ContactsByStanding { get; set; } = new();
    public List<ContactEntry> RecentContacts { get; set; } = new();
    public List<StandingEntry> CorporationStandings { get; set; } = new();
    public List<StandingEntry> AllianceStandings { get; set; } = new();
    public SecurityAnalysis SecurityAnalysis { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Security risk analysis
/// </summary>
public class SecurityAnalysis
{
    public int HighRiskContacts { get; set; }
    public int WatchedContacts { get; set; }
    public int BlockedContacts { get; set; }
    public int NegativeStandingEntities { get; set; }
    public int HostileCorporations { get; set; }
    public int HostileAlliances { get; set; }
    public string OverallRiskLevel { get; set; } = "Low";
}

/// <summary>
/// Contact and standing history
/// </summary>
public class ContactHistory
{
    public long CharacterId { get; set; }
    public TimeSpan AnalysisPeriod { get; set; }
    public List<ContactChangeEntry> ContactChanges { get; set; } = new();
    public List<StandingChangeEntry> StandingChanges { get; set; } = new();
    public int TotalContactChanges { get; set; }
    public int TotalStandingChanges { get; set; }
    public int ContactsAdded { get; set; }
    public int ContactsRemoved { get; set; }
    public int ContactsModified { get; set; }
    public DateTime LastUpdated { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Contact change history entry
/// </summary>
public class ContactChangeEntry
{
    public long CharacterId { get; set; }
    public long ContactId { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
    public DateTime ChangeDate { get; set; }
    public double? PreviousStanding { get; set; }
    public double? NewStanding { get; set; }
}

/// <summary>
/// Standing change history entry
/// </summary>
public class StandingChangeEntry
{
    public long CharacterId { get; set; }
    public long FromId { get; set; }
    public string FromName { get; set; } = string.Empty;
    public string FromType { get; set; } = string.Empty;
    public DateTime ChangeDate { get; set; }
    public double? PreviousStanding { get; set; }
    public double NewStanding { get; set; }
}

/// <summary>
/// ESI Contacts Response
/// </summary>
public class EsiContactsResponse
{
    public long contact_id { get; set; }
    public string contact_type { get; set; } = string.Empty;
    public double standing { get; set; }
    public bool is_watched { get; set; }
    public bool is_blocked { get; set; }
    public List<long>? label_ids { get; set; }
}

/// <summary>
/// ESI Standings Response
/// </summary>
public class EsiStandingsResponse
{
    public long from_id { get; set; }
    public string from_type { get; set; } = string.Empty;
    public double standing { get; set; }
}

/// <summary>
/// Character contacts service interface
/// </summary>
public interface ICharacterContactsService
{
    Task<ContactsData> SynchronizeContactsAsync(long characterId, string accessToken, CancellationToken cancellationToken = default);
    Task<StandingsData> SynchronizeStandingsAsync(long characterId, string accessToken, CancellationToken cancellationToken = default);
    Task<DiplomaticSummary> GetDiplomaticSummaryAsync(long characterId, CancellationToken cancellationToken = default);
    Task<List<ContactEntry>> SearchContactsAsync(long characterId, string searchTerm, double? minStanding = null, double? maxStanding = null, CancellationToken cancellationToken = default);
    Task<ContactHistory> GetContactHistoryAsync(long characterId, int days = 30, CancellationToken cancellationToken = default);
}

#endregion

#region Market Data Services

/// <summary>
/// Market order retrieval and analysis service for all regions
/// </summary>
public class MarketOrderService : IMarketOrderService
{
    private readonly IEsiApiService _esiApiService;
    private readonly IEsiMarketService _esiMarketService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarketOrderService> _logger;
    private readonly IAuditLogService _auditService;
    private readonly IEsiDegradationService _degradationService;
    private readonly ICacheService _cacheService;
    
    private readonly ConcurrentDictionary<int, DateTime> _regionSyncTimes = new();
    private readonly ConcurrentDictionary<int, DateTime> _typeSyncTimes = new();

    public MarketOrderService(
        IEsiApiService esiApiService,
        IEsiMarketService esiMarketService,
        IUnitOfWork unitOfWork,
        ILogger<MarketOrderService> logger,
        IAuditLogService auditService,
        IEsiDegradationService degradationService,
        ICacheService cacheService)
    {
        _esiApiService = esiApiService ?? throw new ArgumentNullException(nameof(esiApiService));
        _esiMarketService = esiMarketService ?? throw new ArgumentNullException(nameof(esiMarketService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _degradationService = degradationService ?? throw new ArgumentNullException(nameof(degradationService));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    /// <summary>
    /// Retrieve all market orders for a specific region
    /// </summary>
    public async Task<RegionMarketData> GetRegionMarketOrdersAsync(int regionId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving market orders for region {RegionId}", regionId);
            
            // Check if we need to update (once per 15 minutes for market data)
            if (_regionSyncTimes.TryGetValue(regionId, out var lastSync) && 
                DateTime.UtcNow - lastSync < TimeSpan.FromMinutes(15))
            {
                var cached = await GetCachedRegionMarketDataAsync(regionId, cancellationToken);
                if (cached != null)
                {
                    _logger.LogDebug("Using cached market data for region {RegionId}", regionId);
                    return cached;
                }
            }
            
            var marketData = await _degradationService.ExecuteWithFallbackAsync(
                async (ct) => await RetrieveRegionOrdersFromEsiAsync(regionId, ct),
                async (ct) => await GetCachedRegionMarketDataAsync(regionId, ct),
                $"region_market_{regionId}",
                cancellationToken);
            
            if (marketData != null)
            {
                marketData.LastUpdated = DateTime.UtcNow;
                marketData.SyncStatus = SyncStatus.Success;
                
                // Store in database and cache
                await StoreRegionMarketDataAsync(marketData, cancellationToken);
                
                _regionSyncTimes[regionId] = DateTime.UtcNow;
                
                await _auditService.LogActionAsync("region_market_synced", "Market", 
                    regionId.ToString(), cancellationToken);
                
                _logger.LogInformation("Market data synchronized for region {RegionId}: {OrderCount} orders", 
                    regionId, marketData.Orders.Count);
            }
            
            return marketData ?? new RegionMarketData 
            { 
                RegionId = regionId, 
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = "Failed to retrieve market data"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving market orders for region {RegionId}", regionId);
            
            return new RegionMarketData 
            { 
                RegionId = regionId, 
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Get market orders for specific type across all major trade hub regions
    /// </summary>
    public async Task<TypeMarketData> GetTypeMarketOrdersAsync(int typeId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving market orders for type {TypeId} across major trade hubs", typeId);
            
            // Check if we need to update (once per 10 minutes for specific types)
            if (_typeSyncTimes.TryGetValue(typeId, out var lastSync) && 
                DateTime.UtcNow - lastSync < TimeSpan.FromMinutes(10))
            {
                var cached = await GetCachedTypeMarketDataAsync(typeId, cancellationToken);
                if (cached != null)
                {
                    _logger.LogDebug("Using cached market data for type {TypeId}", typeId);
                    return cached;
                }
            }
            
            var majorTradeHubs = GetMajorTradeHubRegions();
            var typeMarketData = new TypeMarketData
            {
                TypeId = typeId,
                LastUpdated = DateTime.UtcNow,
                RegionalData = new List<RegionalTypeData>()
            };
            
            // Retrieve data from all major trade hubs concurrently
            var tasks = majorTradeHubs.Select(async regionId =>
            {
                try
                {
                    var regionalOrders = await _degradationService.ExecuteWithFallbackAsync(
                        async (ct) => await RetrieveTypeOrdersFromEsiAsync(regionId, typeId, ct),
                        async (ct) => await GetCachedTypeRegionalDataAsync(regionId, typeId, ct),
                        $"type_market_{regionId}_{typeId}",
                        cancellationToken);
                    
                    return regionalOrders;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to retrieve market data for type {TypeId} in region {RegionId}", typeId, regionId);
                    return null;
                }
            });
            
            var results = await Task.WhenAll(tasks);
            
            foreach (var result in results.Where(r => r != null))
            {
                typeMarketData.RegionalData.Add(result!);
            }
            
            // Calculate aggregated statistics
            typeMarketData.Statistics = CalculateTypeMarketStatistics(typeMarketData.RegionalData);
            typeMarketData.SyncStatus = SyncStatus.Success;
            
            // Store in database and cache
            await StoreTypeMarketDataAsync(typeMarketData, cancellationToken);
            
            _typeSyncTimes[typeId] = DateTime.UtcNow;
            
            await _auditService.LogActionAsync("type_market_synced", "Market", 
                typeId.ToString(), cancellationToken);
            
            return typeMarketData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving market orders for type {TypeId}", typeId);
            
            return new TypeMarketData 
            { 
                TypeId = typeId, 
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Get comprehensive market overview for all major regions
    /// </summary>
    public async Task<MarketOverview> GetMarketOverviewAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating comprehensive market overview");
            
            var overview = new MarketOverview
            {
                LastUpdated = DateTime.UtcNow,
                RegionalOverviews = new List<RegionalMarketOverview>(),
                GlobalStatistics = new MarketStatistics()
            };
            
            var majorRegions = GetMajorTradeHubRegions();
            
            // Get overview for each major region
            foreach (var regionId in majorRegions)
            {
                try
                {
                    var regionOverview = await GetRegionalMarketOverviewAsync(regionId, cancellationToken);
                    if (regionOverview != null)
                    {
                        overview.RegionalOverviews.Add(regionOverview);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get market overview for region {RegionId}", regionId);
                }
            }
            
            // Calculate global statistics
            overview.GlobalStatistics = CalculateGlobalMarketStatistics(overview.RegionalOverviews);
            
            // Cache overview for 20 minutes
            await _cacheService.SetAsync("market_overview", overview, TimeSpan.FromMinutes(20), cancellationToken);
            
            await _auditService.LogActionAsync("market_overview_generated", "Market", null, cancellationToken);
            
            return overview;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating market overview");
            
            return new MarketOverview
            {
                LastUpdated = DateTime.UtcNow,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Search for market opportunities across regions
    /// </summary>
    public async Task<List<ArbitrageOpportunity>> FindArbitrageOpportunitiesAsync(
        List<int> typeIds, 
        double minProfitMargin = 0.1, 
        long minProfit = 1_000_000,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Searching for arbitrage opportunities for {TypeCount} types with {MinMargin}% margin", 
                typeIds.Count, minProfitMargin * 100);
            
            var opportunities = new List<ArbitrageOpportunity>();
            var majorRegions = GetMajorTradeHubRegions();
            
            foreach (var typeId in typeIds)
            {
                try
                {
                    var typeMarketData = await GetTypeMarketOrdersAsync(typeId, cancellationToken);
                    
                    if (typeMarketData.SyncStatus == SyncStatus.Success)
                    {
                        var typeOpportunities = AnalyzeArbitrageOpportunities(
                            typeMarketData, minProfitMargin, minProfit);
                        
                        opportunities.AddRange(typeOpportunities);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to analyze arbitrage for type {TypeId}", typeId);
                }
            }
            
            // Sort by profit potential
            var sortedOpportunities = opportunities
                .OrderByDescending(o => o.ProfitMargin)
                .ThenByDescending(o => o.EstimatedProfit)
                .Take(50) // Limit to top 50 opportunities
                .ToList();
            
            await _auditService.LogActionAsync("arbitrage_opportunities_found", "Market", 
                $"{sortedOpportunities.Count} opportunities", cancellationToken);
            
            return sortedOpportunities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding arbitrage opportunities");
            return new List<ArbitrageOpportunity>();
        }
    }

    /// <summary>
    /// Get market data updates for watchlist items
    /// </summary>
    public async Task<WatchlistMarketData> GetWatchlistMarketDataAsync(
        List<int> watchlistTypeIds, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving market data for {WatchlistCount} watchlist items", watchlistTypeIds.Count);
            
            var watchlistData = new WatchlistMarketData
            {
                LastUpdated = DateTime.UtcNow,
                TypeMarketData = new List<TypeMarketData>()
            };
            
            // Process types in batches to avoid overwhelming the API
            const int batchSize = 10;
            var batches = watchlistTypeIds
                .Select((typeId, index) => new { typeId, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.typeId).ToList());
            
            foreach (var batch in batches)
            {
                var batchTasks = batch.Select(typeId => GetTypeMarketOrdersAsync(typeId, cancellationToken));
                var batchResults = await Task.WhenAll(batchTasks);
                
                watchlistData.TypeMarketData.AddRange(batchResults);
                
                // Small delay between batches to respect rate limits
                await Task.Delay(100, cancellationToken);
            }
            
            // Calculate watchlist statistics
            watchlistData.Summary = CalculateWatchlistSummary(watchlistData.TypeMarketData);
            
            return watchlistData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving watchlist market data");
            
            return new WatchlistMarketData
            {
                LastUpdated = DateTime.UtcNow,
                ErrorMessage = ex.Message
            };
        }
    }

    #region Private ESI Retrieval Methods

    private async Task<RegionMarketData?> RetrieveRegionOrdersFromEsiAsync(int regionId, CancellationToken cancellationToken)
    {
        try
        {
            var orders = await _esiMarketService.GetRegionOrdersAsync(regionId, cancellationToken);
            
            var regionData = new RegionMarketData
            {
                RegionId = regionId,
                LastUpdated = DateTime.UtcNow,
                SyncStatus = SyncStatus.Success,
                Orders = orders.Select(order => new MarketOrder
                {
                    OrderId = order.order_id,
                    TypeId = order.type_id,
                    LocationId = order.location_id,
                    SystemId = order.system_id,
                    VolumeTotal = order.volume_total,
                    VolumeRemain = order.volume_remain,
                    MinVolume = order.min_volume,
                    Price = order.price,
                    IsBuyOrder = order.is_buy_order,
                    Duration = order.duration,
                    Issued = order.issued,
                    Range = order.range
                }).ToList()
            };
            
            // Calculate region statistics
            regionData.Statistics = CalculateRegionStatistics(regionData.Orders);
            
            return regionData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving market orders from ESI for region {RegionId}", regionId);
            return null;
        }
    }

    private async Task<RegionalTypeData?> RetrieveTypeOrdersFromEsiAsync(int regionId, int typeId, CancellationToken cancellationToken)
    {
        try
        {
            var orders = await _esiMarketService.GetRegionTypeOrdersAsync(regionId, typeId, cancellationToken);
            
            var regionalData = new RegionalTypeData
            {
                RegionId = regionId,
                TypeId = typeId,
                LastUpdated = DateTime.UtcNow,
                Orders = orders.Select(order => new MarketOrder
                {
                    OrderId = order.order_id,
                    TypeId = order.type_id,
                    LocationId = order.location_id,
                    SystemId = order.system_id,
                    VolumeTotal = order.volume_total,
                    VolumeRemain = order.volume_remain,
                    MinVolume = order.min_volume,
                    Price = order.price,
                    IsBuyOrder = order.is_buy_order,
                    Duration = order.duration,
                    Issued = order.issued,
                    Range = order.range
                }).ToList()
            };
            
            // Calculate type-specific statistics for this region
            regionalData.BuyOrders = regionalData.Orders.Where(o => o.IsBuyOrder).ToList();
            regionalData.SellOrders = regionalData.Orders.Where(o => !o.IsBuyOrder).ToList();
            regionalData.HighestBuy = regionalData.BuyOrders.Any() ? regionalData.BuyOrders.Max(o => o.Price) : 0;
            regionalData.LowestSell = regionalData.SellOrders.Any() ? regionalData.SellOrders.Min(o => o.Price) : 0;
            regionalData.TotalVolume = regionalData.Orders.Sum(o => o.VolumeRemain);
            
            return regionalData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving type orders from ESI for region {RegionId}, type {TypeId}", regionId, typeId);
            return null;
        }
    }

    #endregion

    #region Private Cache Methods

    private async Task<RegionMarketData?> GetCachedRegionMarketDataAsync(int regionId, CancellationToken cancellationToken)
    {
        try
        {
            return await _cacheService.GetAsync<RegionMarketData>($"region_market_{regionId}", cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private async Task<TypeMarketData?> GetCachedTypeMarketDataAsync(int typeId, CancellationToken cancellationToken)
    {
        try
        {
            return await _cacheService.GetAsync<TypeMarketData>($"type_market_{typeId}", cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private async Task<RegionalTypeData?> GetCachedTypeRegionalDataAsync(int regionId, int typeId, CancellationToken cancellationToken)
    {
        try
        {
            return await _cacheService.GetAsync<RegionalTypeData>($"regional_type_{regionId}_{typeId}", cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region Private Storage Methods

    private async Task StoreRegionMarketDataAsync(RegionMarketData marketData, CancellationToken cancellationToken)
    {
        try
        {
            // Clear existing orders for region
            var existingOrders = await _unitOfWork.MarketOrders.FindAsync(
                mo => mo.RegionId == marketData.RegionId, cancellationToken);
            
            foreach (var order in existingOrders)
            {
                await _unitOfWork.MarketOrders.DeleteAsync(order, cancellationToken);
            }
            
            // Add new orders
            foreach (var order in marketData.Orders)
            {
                order.RegionId = marketData.RegionId;
                await _unitOfWork.MarketOrders.AddAsync(order, cancellationToken);
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            // Cache for 15 minutes
            await _cacheService.SetAsync($"region_market_{marketData.RegionId}", 
                marketData, TimeSpan.FromMinutes(15), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing market data for region {RegionId}", marketData.RegionId);
        }
    }

    private async Task StoreTypeMarketDataAsync(TypeMarketData typeMarketData, CancellationToken cancellationToken)
    {
        try
        {
            // Store individual regional data
            foreach (var regionalData in typeMarketData.RegionalData)
            {
                await _cacheService.SetAsync($"regional_type_{regionalData.RegionId}_{regionalData.TypeId}", 
                    regionalData, TimeSpan.FromMinutes(10), cancellationToken);
            }
            
            // Cache complete type data for 10 minutes
            await _cacheService.SetAsync($"type_market_{typeMarketData.TypeId}", 
                typeMarketData, TimeSpan.FromMinutes(10), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing type market data for type {TypeId}", typeMarketData.TypeId);
        }
    }

    #endregion

    #region Private Analysis Methods

    private static List<int> GetMajorTradeHubRegions()
    {
        return new List<int>
        {
            10000002, // The Forge (Jita)
            10000032, // Sinq Laison (Dodixie)
            10000043, // Domain (Amarr)
            10000030, // Heimatar (Rens)
            10000042  // Metropolis (Hek)
        };
    }

    private async Task<RegionalMarketOverview?> GetRegionalMarketOverviewAsync(int regionId, CancellationToken cancellationToken)
    {
        try
        {
            var regionData = await GetCachedRegionMarketDataAsync(regionId, cancellationToken);
            if (regionData == null) return null;
            
            return new RegionalMarketOverview
            {
                RegionId = regionId,
                TotalOrders = regionData.Orders.Count,
                BuyOrders = regionData.Orders.Count(o => o.IsBuyOrder),
                SellOrders = regionData.Orders.Count(o => !o.IsBuyOrder),
                TotalVolume = regionData.Orders.Sum(o => o.VolumeRemain),
                TotalValue = regionData.Orders.Sum(o => o.Price * o.VolumeRemain),
                UniqueTypes = regionData.Orders.Select(o => o.TypeId).Distinct().Count(),
                AveragePrice = regionData.Orders.Any() ? regionData.Orders.Average(o => o.Price) : 0,
                LastUpdated = regionData.LastUpdated
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating regional market overview for region {RegionId}", regionId);
            return null;
        }
    }

    private static RegionStatistics CalculateRegionStatistics(List<MarketOrder> orders)
    {
        return new RegionStatistics
        {
            TotalOrders = orders.Count,
            BuyOrders = orders.Count(o => o.IsBuyOrder),
            SellOrders = orders.Count(o => !o.IsBuyOrder),
            TotalVolume = orders.Sum(o => o.VolumeRemain),
            TotalValue = orders.Sum(o => o.Price * o.VolumeRemain),
            AveragePrice = orders.Any() ? orders.Average(o => o.Price) : 0,
            MedianPrice = CalculateMedianPrice(orders),
            UniqueTypes = orders.Select(o => o.TypeId).Distinct().Count()
        };
    }

    private static TypeMarketStatistics CalculateTypeMarketStatistics(List<RegionalTypeData> regionalData)
    {
        var allOrders = regionalData.SelectMany(rd => rd.Orders).ToList();
        
        return new TypeMarketStatistics
        {
            TotalOrders = allOrders.Count,
            TotalRegions = regionalData.Count,
            GlobalHighestBuy = regionalData.Any() ? regionalData.Max(rd => rd.HighestBuy) : 0,
            GlobalLowestSell = regionalData.Any() ? regionalData.Where(rd => rd.LowestSell > 0).Min(rd => rd.LowestSell) : 0,
            TotalVolume = allOrders.Sum(o => o.VolumeRemain),
            AveragePrice = allOrders.Any() ? allOrders.Average(o => o.Price) : 0,
            PriceSpread = CalculatePriceSpread(regionalData),
            BestArbitrageOpportunity = FindBestArbitrageForType(regionalData)
        };
    }

    private static MarketStatistics CalculateGlobalMarketStatistics(List<RegionalMarketOverview> regionalOverviews)
    {
        return new MarketStatistics
        {
            TotalOrders = regionalOverviews.Sum(ro => ro.TotalOrders),
            TotalVolume = regionalOverviews.Sum(ro => ro.TotalVolume),
            TotalValue = regionalOverviews.Sum(ro => ro.TotalValue),
            AveragePrice = regionalOverviews.Any() ? regionalOverviews.Average(ro => ro.AveragePrice) : 0,
            ActiveRegions = regionalOverviews.Count,
            UniqueTypes = regionalOverviews.Sum(ro => ro.UniqueTypes) // Approximate, may have duplicates
        };
    }

    private static List<ArbitrageOpportunity> AnalyzeArbitrageOpportunities(
        TypeMarketData typeMarketData, 
        double minProfitMargin, 
        long minProfit)
    {
        var opportunities = new List<ArbitrageOpportunity>();
        
        foreach (var buyRegion in typeMarketData.RegionalData)
        {
            foreach (var sellRegion in typeMarketData.RegionalData)
            {
                if (buyRegion.RegionId == sellRegion.RegionId) continue;
                
                if (buyRegion.HighestBuy > 0 && sellRegion.LowestSell > 0)
                {
                    var profit = buyRegion.HighestBuy - sellRegion.LowestSell;
                    var profitMargin = profit / sellRegion.LowestSell;
                    
                    if (profitMargin >= minProfitMargin && profit >= minProfit)
                    {
                        opportunities.Add(new ArbitrageOpportunity
                        {
                            TypeId = typeMarketData.TypeId,
                            BuyRegionId = buyRegion.RegionId,
                            SellRegionId = sellRegion.RegionId,
                            BuyPrice = buyRegion.HighestBuy,
                            SellPrice = sellRegion.LowestSell,
                            ProfitMargin = profitMargin,
                            EstimatedProfit = profit,
                            PotentialVolume = Math.Min(buyRegion.TotalVolume, sellRegion.TotalVolume),
                            LastUpdated = DateTime.UtcNow
                        });
                    }
                }
            }
        }
        
        return opportunities;
    }

    private static WatchlistSummary CalculateWatchlistSummary(List<TypeMarketData> typeMarketData)
    {
        return new WatchlistSummary
        {
            TotalTypes = typeMarketData.Count,
            ActiveTypes = typeMarketData.Count(tmd => tmd.SyncStatus == SyncStatus.Success),
            TotalVolume = typeMarketData.SelectMany(tmd => tmd.RegionalData)
                                     .SelectMany(rd => rd.Orders)
                                     .Sum(o => o.VolumeRemain),
            AveragePrice = typeMarketData.SelectMany(tmd => tmd.RegionalData)
                                        .SelectMany(rd => rd.Orders)
                                        .Average(o => o.Price),
            LastUpdated = DateTime.UtcNow
        };
    }

    private static double CalculateMedianPrice(List<MarketOrder> orders)
    {
        if (!orders.Any()) return 0;
        
        var sortedPrices = orders.Select(o => o.Price).OrderBy(p => p).ToList();
        var count = sortedPrices.Count;
        
        if (count % 2 == 0)
        {
            return (sortedPrices[count / 2 - 1] + sortedPrices[count / 2]) / 2;
        }
        else
        {
            return sortedPrices[count / 2];
        }
    }

    private static double CalculatePriceSpread(List<RegionalTypeData> regionalData)
    {
        var highestBuy = regionalData.Max(rd => rd.HighestBuy);
        var lowestSell = regionalData.Where(rd => rd.LowestSell > 0).Min(rd => rd.LowestSell);
        
        return lowestSell > 0 ? (lowestSell - highestBuy) / lowestSell : 0;
    }

    private static ArbitrageOpportunity? FindBestArbitrageForType(List<RegionalTypeData> regionalData)
    {
        var opportunities = new List<ArbitrageOpportunity>();
        
        foreach (var buyRegion in regionalData)
        {
            foreach (var sellRegion in regionalData)
            {
                if (buyRegion.RegionId != sellRegion.RegionId && 
                    buyRegion.HighestBuy > sellRegion.LowestSell && 
                    sellRegion.LowestSell > 0)
                {
                    var profit = buyRegion.HighestBuy - sellRegion.LowestSell;
                    var profitMargin = profit / sellRegion.LowestSell;
                    
                    opportunities.Add(new ArbitrageOpportunity
                    {
                        BuyRegionId = buyRegion.RegionId,
                        SellRegionId = sellRegion.RegionId,
                        BuyPrice = buyRegion.HighestBuy,
                        SellPrice = sellRegion.LowestSell,
                        ProfitMargin = profitMargin,
                        EstimatedProfit = profit
                    });
                }
            }
        }
        
        return opportunities.OrderByDescending(o => o.ProfitMargin).FirstOrDefault();
    }

    #endregion
}

/// <summary>
/// Market order service interface
/// </summary>
public interface IMarketOrderService
{
    Task<RegionMarketData> GetRegionMarketOrdersAsync(int regionId, CancellationToken cancellationToken = default);
    Task<TypeMarketData> GetTypeMarketOrdersAsync(int typeId, CancellationToken cancellationToken = default);
    Task<MarketOverview> GetMarketOverviewAsync(CancellationToken cancellationToken = default);
    Task<List<ArbitrageOpportunity>> FindArbitrageOpportunitiesAsync(List<int> typeIds, double minProfitMargin = 0.1, long minProfit = 1_000_000, CancellationToken cancellationToken = default);
    Task<WatchlistMarketData> GetWatchlistMarketDataAsync(List<int> watchlistTypeIds, CancellationToken cancellationToken = default);
}

#endregion

#endregion