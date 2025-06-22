// ==========================================================================
// ValidationServices.cs - Data Validation Service Implementations
// ==========================================================================
// Comprehensive validation services for ensuring data integrity and business rules.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.Extensions.Logging;
using Gideon.WPF.Core.Application.Services;
using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Core.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Gideon.WPF.Core.Infrastructure.Services;

#region Data Validation Services

/// <summary>
/// Data validation service for ensuring data integrity
/// </summary>
public class DataValidationService : IDataValidationService
{
    private readonly ILogger<DataValidationService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public DataValidationService(ILogger<DataValidationService> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Validate character data integrity and business rules
    /// </summary>
    public async Task<bool> ValidateCharacterDataAsync(Character character, CancellationToken cancellationToken = default)
    {
        if (character == null)
        {
            _logger.LogWarning("Character data validation failed: null character");
            return false;
        }

        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(character);

        // Validate data annotations
        if (!Validator.TryValidateObject(character, context, validationResults, true))
        {
            _logger.LogWarning("Character data validation failed for {CharacterName}: {Errors}", 
                character.CharacterName, 
                string.Join(", ", validationResults.Select(r => r.ErrorMessage)));
            return false;
        }

        // Business rule validation
        if (!await ValidateCharacterBusinessRulesAsync(character, cancellationToken))
        {
            return false;
        }

        // Validate character skills
        if (!await ValidateCharacterSkillsAsync(character, cancellationToken))
        {
            return false;
        }

        _logger.LogDebug("Character data validation successful for {CharacterName}", character.CharacterName);
        return true;
    }

    /// <summary>
    /// Validate market data integrity and business rules
    /// </summary>
    public async Task<bool> ValidateMarketDataAsync(MarketData marketData, CancellationToken cancellationToken = default)
    {
        if (marketData == null)
        {
            _logger.LogWarning("Market data validation failed: null market data");
            return false;
        }

        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(marketData);

        // Validate data annotations
        if (!Validator.TryValidateObject(marketData, context, validationResults, true))
        {
            _logger.LogWarning("Market data validation failed: {Errors}", 
                string.Join(", ", validationResults.Select(r => r.ErrorMessage)));
            return false;
        }

        // Business rule validation
        if (!await ValidateMarketDataBusinessRulesAsync(marketData, cancellationToken))
        {
            return false;
        }

        _logger.LogDebug("Market data validation successful for Type {TypeId} in Region {RegionId}", 
            marketData.TypeId, marketData.RegionId);
        return true;
    }

    #region Character Validation Helper Methods

    private async Task<bool> ValidateCharacterBusinessRulesAsync(Character character, CancellationToken cancellationToken)
    {
        // Validate character ID is positive
        if (character.CharacterId <= 0)
        {
            _logger.LogWarning("Invalid character ID: {CharacterId}", character.CharacterId);
            return false;
        }

        // Validate character name format
        if (!IsValidCharacterName(character.CharacterName))
        {
            _logger.LogWarning("Invalid character name format: {CharacterName}", character.CharacterName);
            return false;
        }

        // Check for duplicate character IDs
        var existingCharacter = await _unitOfWork.Characters.FindFirstAsync(
            c => c.CharacterId == character.CharacterId && c.Id != character.Id, 
            cancellationToken);

        if (existingCharacter != null)
        {
            _logger.LogWarning("Duplicate character ID detected: {CharacterId}", character.CharacterId);
            return false;
        }

        // Validate corporation and alliance IDs
        if (character.CorporationId.HasValue && character.CorporationId <= 0)
        {
            _logger.LogWarning("Invalid corporation ID: {CorporationId}", character.CorporationId);
            return false;
        }

        if (character.AllianceId.HasValue && character.AllianceId <= 0)
        {
            _logger.LogWarning("Invalid alliance ID: {AllianceId}", character.AllianceId);
            return false;
        }

        // Validate security status is within EVE's bounds
        if (character.SecurityStatus < -10.0 || character.SecurityStatus > 5.0)
        {
            _logger.LogWarning("Security status out of range: {SecurityStatus}", character.SecurityStatus);
            return false;
        }

        return true;
    }

    private async Task<bool> ValidateCharacterSkillsAsync(Character character, CancellationToken cancellationToken)
    {
        foreach (var skill in character.Skills)
        {
            // Validate skill level is within bounds (0-5)
            if (skill.SkillLevel < 0 || skill.SkillLevel > 5)
            {
                _logger.LogWarning("Skill level out of range for skill {SkillId}: {SkillLevel}", 
                    skill.SkillId, skill.SkillLevel);
                return false;
            }

            // Validate skill points are reasonable for the level
            if (!IsValidSkillPointsForLevel(skill.SkillPoints, skill.SkillLevel))
            {
                _logger.LogWarning("Invalid skill points for level - Skill {SkillId}: {SkillPoints} SP at level {SkillLevel}", 
                    skill.SkillId, skill.SkillPoints, skill.SkillLevel);
                return false;
            }

            // Validate skill exists in EVE data
            var eveSkill = await _unitOfWork.EveTypes.GetByIdAsync(skill.SkillId, cancellationToken);
            if (eveSkill == null)
            {
                _logger.LogWarning("Unknown skill ID: {SkillId}", skill.SkillId);
                return false;
            }
        }

        return true;
    }

    private static bool IsValidCharacterName(string characterName)
    {
        if (string.IsNullOrWhiteSpace(characterName))
            return false;

        // EVE character names: 3-37 characters, alphanumeric plus spaces and single quotes
        var nameRegex = new Regex(@"^[a-zA-Z0-9\s']{3,37}$");
        if (!nameRegex.IsMatch(characterName))
            return false;

        // Cannot start or end with space
        if (characterName.StartsWith(' ') || characterName.EndsWith(' '))
            return false;

        // Cannot have consecutive spaces
        if (characterName.Contains("  "))
            return false;

        return true;
    }

    private static bool IsValidSkillPointsForLevel(long skillPoints, int skillLevel)
    {
        // EVE skill point requirements per level
        var requiredSP = skillLevel switch
        {
            0 => 0L,
            1 => 250L,
            2 => 1414L,   // sqrt(2) * 1000
            3 => 8000L,
            4 => 45255L,  // sqrt(32) * 8000
            5 => 256000L,
            _ => long.MaxValue
        };

        var nextLevelSP = skillLevel switch
        {
            0 => 250L,
            1 => 1414L,
            2 => 8000L,
            3 => 45255L,
            4 => 256000L,
            5 => long.MaxValue,
            _ => long.MaxValue
        };

        return skillPoints >= requiredSP && skillPoints < nextLevelSP;
    }

    #endregion

    #region Market Data Validation Helper Methods

    private async Task<bool> ValidateMarketDataBusinessRulesAsync(MarketData marketData, CancellationToken cancellationToken)
    {
        // Validate price is positive
        if (marketData.Price <= 0)
        {
            _logger.LogWarning("Invalid market price: {Price}", marketData.Price);
            return false;
        }

        // Validate volume is positive
        if (marketData.Volume <= 0)
        {
            _logger.LogWarning("Invalid market volume: {Volume}", marketData.Volume);
            return false;
        }

        // Validate duration is reasonable (0-365 days)
        if (marketData.Duration < 0 || marketData.Duration > 365)
        {
            _logger.LogWarning("Invalid order duration: {Duration}", marketData.Duration);
            return false;
        }

        // Validate location ID exists
        if (marketData.LocationId <= 0)
        {
            _logger.LogWarning("Invalid location ID: {LocationId}", marketData.LocationId);
            return false;
        }

        // Validate type exists in EVE data
        var eveType = await _unitOfWork.EveTypes.GetByIdAsync(marketData.TypeId, cancellationToken);
        if (eveType == null)
        {
            _logger.LogWarning("Unknown type ID in market data: {TypeId}", marketData.TypeId);
            return false;
        }

        // Validate region exists in EVE data
        var eveRegion = await _unitOfWork.EveRegions.GetByIdAsync(marketData.RegionId, cancellationToken);
        if (eveRegion == null)
        {
            _logger.LogWarning("Unknown region ID in market data: {RegionId}", marketData.RegionId);
            return false;
        }

        // Validate price is within reasonable bounds for the item
        if (!await IsReasonablePrice(marketData.TypeId, marketData.Price, cancellationToken))
        {
            _logger.LogWarning("Suspicious price for Type {TypeId}: {Price} ISK", 
                marketData.TypeId, marketData.Price);
            return false;
        }

        return true;
    }

    private async Task<bool> IsReasonablePrice(int typeId, decimal price, CancellationToken cancellationToken)
    {
        // Get recent price data for comparison
        var recentPrices = await _unitOfWork.MarketData.FindAsync(
            m => m.TypeId == typeId && m.RecordedDate >= DateTime.UtcNow.AddDays(-7),
            cancellationToken);

        if (!recentPrices.Any())
            return true; // No comparison data available

        var avgPrice = recentPrices.Average(p => p.Price);
        var priceRatio = (double)(price / avgPrice);

        // Flag if price is more than 10x or less than 0.1x the average
        return priceRatio >= 0.1 && priceRatio <= 10.0;
    }

    #endregion
}

/// <summary>
/// Ship fitting validation service for ensuring valid fittings
/// </summary>
public class ShipFittingValidationService : IShipFittingValidationService
{
    private readonly ILogger<ShipFittingValidationService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IShipFittingCalculationService _calculationService;

    public ShipFittingValidationService(
        ILogger<ShipFittingValidationService> logger,
        IUnitOfWork unitOfWork,
        IShipFittingCalculationService calculationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _calculationService = calculationService ?? throw new ArgumentNullException(nameof(calculationService));
    }

    /// <summary>
    /// Validate ship fitting constraints and compatibility
    /// </summary>
    public async Task<bool> ValidateFittingAsync(ShipFitting fitting, CancellationToken cancellationToken = default)
    {
        if (fitting == null)
        {
            _logger.LogWarning("Ship fitting validation failed: null fitting");
            return false;
        }

        var errors = await GetValidationErrorsAsync(fitting, cancellationToken);
        var isValid = !errors.Any();

        if (isValid)
        {
            _logger.LogDebug("Ship fitting validation successful for {FittingName}", fitting.Name);
        }
        else
        {
            _logger.LogWarning("Ship fitting validation failed for {FittingName}: {Errors}", 
                fitting.Name, string.Join(", ", errors));
        }

        return isValid;
    }

    /// <summary>
    /// Get detailed validation errors for a ship fitting
    /// </summary>
    public async Task<IEnumerable<string>> GetValidationErrorsAsync(ShipFitting fitting, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        if (fitting == null)
        {
            errors.Add("Fitting cannot be null");
            return errors;
        }

        // Validate basic fitting properties
        await ValidateBasicPropertiesAsync(fitting, errors, cancellationToken);

        // Validate ship exists and is valid
        await ValidateShipAsync(fitting, errors, cancellationToken);

        // Validate modules
        await ValidateModulesAsync(fitting, errors, cancellationToken);

        // Validate resource constraints
        await ValidateResourceConstraintsAsync(fitting, errors, cancellationToken);

        // Validate slot constraints
        await ValidateSlotConstraintsAsync(fitting, errors, cancellationToken);

        // Validate module compatibility
        await ValidateModuleCompatibilityAsync(fitting, errors, cancellationToken);

        return errors;
    }

    #region Validation Helper Methods

    private async Task ValidateBasicPropertiesAsync(ShipFitting fitting, List<string> errors, CancellationToken cancellationToken)
    {
        // Validate fitting name
        if (string.IsNullOrWhiteSpace(fitting.Name))
        {
            errors.Add("Fitting name cannot be empty");
        }
        else if (fitting.Name.Length > 100)
        {
            errors.Add("Fitting name cannot exceed 100 characters");
        }

        // Validate fitting has an ID
        if (fitting.Id == Guid.Empty)
        {
            errors.Add("Fitting must have a valid ID");
        }

        // Check for duplicate fitting names for the same character
        if (fitting.CharacterId.HasValue)
        {
            var existingFitting = await _unitOfWork.ShipFittings.FindFirstAsync(
                f => f.CharacterId == fitting.CharacterId && 
                     f.Name == fitting.Name && 
                     f.Id != fitting.Id,
                cancellationToken);

            if (existingFitting != null)
            {
                errors.Add($"A fitting named '{fitting.Name}' already exists for this character");
            }
        }
    }

    private async Task ValidateShipAsync(ShipFitting fitting, List<string> errors, CancellationToken cancellationToken)
    {
        if (fitting.ShipTypeId <= 0)
        {
            errors.Add("Ship type ID must be specified");
            return;
        }

        var shipType = await _unitOfWork.EveTypes.GetByIdAsync(fitting.ShipTypeId, cancellationToken);
        if (shipType == null)
        {
            errors.Add($"Unknown ship type ID: {fitting.ShipTypeId}");
            return;
        }

        // Validate ship is actually a ship (not a module or other item)
        if (!IsShipType(shipType))
        {
            errors.Add($"Type {shipType.TypeName} is not a ship");
        }
    }

    private async Task ValidateModulesAsync(ShipFitting fitting, List<string> errors, CancellationToken cancellationToken)
    {
        if (fitting.Modules == null || !fitting.Modules.Any())
        {
            return; // Empty fitting is valid
        }

        foreach (var module in fitting.Modules)
        {
            // Validate module type exists
            var moduleType = await _unitOfWork.EveTypes.GetByIdAsync(module.TypeId, cancellationToken);
            if (moduleType == null)
            {
                errors.Add($"Unknown module type ID: {module.TypeId}");
                continue;
            }

            // Validate module is actually a module
            if (!IsModuleType(moduleType))
            {
                errors.Add($"Type {moduleType.TypeName} is not a module");
            }

            // Validate slot type is valid
            if (!IsValidSlotType(module.Slot))
            {
                errors.Add($"Invalid slot type: {module.Slot}");
            }

            // Validate quantity is positive
            if (module.Quantity <= 0)
            {
                errors.Add($"Module quantity must be positive for {moduleType.TypeName}");
            }
        }
    }

    private async Task ValidateResourceConstraintsAsync(ShipFitting fitting, List<string> errors, CancellationToken cancellationToken)
    {
        try
        {
            var resourceUsage = await _calculationService.CalculateResourceUsageAsync(fitting, null, cancellationToken);

            // Check CPU usage
            if (resourceUsage.CpuUtilization > 1.0)
            {
                errors.Add($"CPU usage exceeds capacity: {resourceUsage.UsedCpu:F1}/{resourceUsage.TotalCpu:F1} tf");
            }

            // Check PowerGrid usage
            if (resourceUsage.PowerGridUtilization > 1.0)
            {
                errors.Add($"PowerGrid usage exceeds capacity: {resourceUsage.UsedPowerGrid:F1}/{resourceUsage.TotalPowerGrid:F1} MW");
            }

            // Check Calibration usage (for rigs)
            if (resourceUsage.CalibrationUtilization > 1.0)
            {
                errors.Add($"Calibration usage exceeds capacity: {resourceUsage.UsedCalibration:F0}/{resourceUsage.TotalCalibration:F0}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating resource constraints for fitting {FittingName}", fitting.Name);
            errors.Add("Unable to validate resource constraints due to calculation error");
        }
    }

    private async Task ValidateSlotConstraintsAsync(ShipFitting fitting, List<string> errors, CancellationToken cancellationToken)
    {
        if (fitting.Modules == null || !fitting.Modules.Any())
            return;

        var shipType = await _unitOfWork.EveTypes.GetByIdAsync(fitting.ShipTypeId, cancellationToken);
        if (shipType == null)
            return;

        // Get ship slot counts (would typically come from ship attributes)
        var slotCounts = GetShipSlotCounts(shipType);

        // Count modules by slot type
        var modulesBySlot = fitting.Modules
            .GroupBy(m => m.Slot)
            .ToDictionary(g => g.Key, g => g.Sum(m => m.Quantity));

        // Check each slot type
        foreach (var (slotType, usedSlots) in modulesBySlot)
        {
            var availableSlots = slotCounts.GetValueOrDefault(slotType, 0);
            if (usedSlots > availableSlots)
            {
                errors.Add($"Too many {slotType} modules: {usedSlots}/{availableSlots}");
            }
        }
    }

    private async Task ValidateModuleCompatibilityAsync(ShipFitting fitting, List<string> errors, CancellationToken cancellationToken)
    {
        if (fitting.Modules == null || !fitting.Modules.Any())
            return;

        var shipType = await _unitOfWork.EveTypes.GetByIdAsync(fitting.ShipTypeId, cancellationToken);
        if (shipType == null)
            return;

        foreach (var module in fitting.Modules)
        {
            var moduleType = await _unitOfWork.EveTypes.GetByIdAsync(module.TypeId, cancellationToken);
            if (moduleType == null)
                continue;

            // Check if module can be fitted to this ship type
            if (!CanModuleFitShip(moduleType, shipType))
            {
                errors.Add($"Module {moduleType.TypeName} cannot be fitted to {shipType.TypeName}");
            }

            // Check for conflicting modules
            var conflicts = GetModuleConflicts(module, fitting.Modules, moduleType);
            errors.AddRange(conflicts);
        }
    }

    private static bool IsShipType(EveType type)
    {
        // In EVE, ships typically have group IDs in specific ranges
        // This would be more sophisticated with actual EVE data
        return type.GroupId >= 25 && type.GroupId <= 31 || // Frigates, Cruisers, etc.
               type.GroupId >= 237 && type.GroupId <= 513 || // Various ship classes
               type.GroupId == 659 || type.GroupId == 883; // Supercarriers, Titans
    }

    private static bool IsModuleType(EveType type)
    {
        // Modules typically have specific group ID ranges
        return type.GroupId >= 1 && type.GroupId <= 24 || // Various module groups
               type.GroupId >= 32 && type.GroupId <= 89 || 
               type.GroupId >= 90 && type.GroupId <= 236;
    }

    private static bool IsValidSlotType(string slotType)
    {
        var validSlots = new[] { "High", "Medium", "Low", "Rig", "Subsystem", "Service" };
        return validSlots.Contains(slotType, StringComparer.OrdinalIgnoreCase);
    }

    private static Dictionary<string, int> GetShipSlotCounts(EveType shipType)
    {
        // This would typically come from ship attributes in the database
        // For now, return default slot counts based on ship group
        return shipType.GroupId switch
        {
            25 => new Dictionary<string, int> { ["High"] = 3, ["Medium"] = 3, ["Low"] = 2, ["Rig"] = 3 }, // Frigate
            26 => new Dictionary<string, int> { ["High"] = 5, ["Medium"] = 4, ["Low"] = 4, ["Rig"] = 3 }, // Cruiser
            27 => new Dictionary<string, int> { ["High"] = 7, ["Medium"] = 6, ["Low"] = 6, ["Rig"] = 3 }, // Battleship
            _ => new Dictionary<string, int> { ["High"] = 8, ["Medium"] = 8, ["Low"] = 8, ["Rig"] = 3 }   // Default
        };
    }

    private static bool CanModuleFitShip(EveType moduleType, EveType shipType)
    {
        // Size restrictions: Small modules on small ships, etc.
        // This would use actual EVE attributes for precise validation
        var moduleSize = GetModuleSize(moduleType);
        var shipSize = GetShipSize(shipType);

        return moduleSize <= shipSize;
    }

    private static int GetModuleSize(EveType moduleType)
    {
        // Determine module size from name or attributes
        if (moduleType.TypeName.Contains("Small") || moduleType.TypeName.Contains("I"))
            return 1;
        if (moduleType.TypeName.Contains("Medium") || moduleType.TypeName.Contains("II"))
            return 2;
        if (moduleType.TypeName.Contains("Large") || moduleType.TypeName.Contains("X-Large"))
            return 3;
        return 1; // Default to small
    }

    private static int GetShipSize(EveType shipType)
    {
        // Determine ship size from group
        return shipType.GroupId switch
        {
            25 => 1, // Frigate
            26 => 2, // Cruiser  
            27 => 3, // Battleship
            _ => 2   // Default to medium
        };
    }

    private static IEnumerable<string> GetModuleConflicts(FittingModule module, IEnumerable<FittingModule> allModules, EveType moduleType)
    {
        var conflicts = new List<string>();

        // Check for mutually exclusive modules
        if (IsMutuallyExclusive(moduleType))
        {
            var conflictingModules = allModules
                .Where(m => m.TypeId != module.TypeId && IsMutuallyExclusive(moduleType))
                .ToList();

            if (conflictingModules.Any())
            {
                conflicts.Add($"Module {moduleType.TypeName} conflicts with other fitted modules");
            }
        }

        return conflicts;
    }

    private static bool IsMutuallyExclusive(EveType moduleType)
    {
        // Certain modules can't be fitted together (e.g., multiple cloaking devices)
        var exclusiveModules = new[]
        {
            "Cloaking Device",
            "Cynosural Field Generator",
            "Jump Drive"
        };

        return exclusiveModules.Any(name => moduleType.TypeName.Contains(name));
    }

    #endregion
}

#endregion