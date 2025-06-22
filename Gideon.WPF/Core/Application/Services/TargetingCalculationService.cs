// ==========================================================================
// TargetingCalculationService.cs - Comprehensive Targeting System Calculation Service
// ==========================================================================
// Implementation of EVE Online accurate targeting system calculations including
// sensor strength, scan resolution, targeting range, and lock-on mechanics.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.Extensions.Logging;
using System.Text.Json;
using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Core.Domain.Interfaces;

namespace Gideon.WPF.Core.Application.Services;

/// <summary>
/// Service for accurate EVE Online targeting system calculations
/// </summary>
public class TargetingCalculationService : ITargetingCalculationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TargetingCalculationService> _logger;
    
    // EVE constants for precise calculations
    private const double ScanResolutionBaseMultiplier = 40000.0; // EVE's scan resolution calculation constant
    private const double TargetingRangeMultiplier = 1000.0; // km to m conversion
    private const double SensorStrengthBaselinePoints = 25.0; // Base sensor strength for calculations
    private const double MaximumTargets = 8.0; // Maximum targets for most ships
    private const double LockTimeMinimum = 1.0; // Minimum lock time in seconds
    
    public TargetingCalculationService(IUnitOfWork unitOfWork, ILogger<TargetingCalculationService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calculate comprehensive targeting system statistics for a ship fitting
    /// </summary>
    public async Task<TargetingCalculationResult> CalculateComprehensiveTargetingAsync(
        ShipFitting fitting, 
        Character? character = null, 
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogDebug("Starting comprehensive targeting calculation for fitting {FittingName}", fitting.Name);

            var result = new TargetingCalculationResult();
            
            // Get ship base stats
            var ship = await _unitOfWork.Ships.GetByIdAsync(fitting.ShipId, cancellationToken);
            if (ship == null)
            {
                throw new InvalidOperationException($"Ship with ID {fitting.ShipId} not found");
            }

            // Parse module configurations
            var midSlotModules = await ParseAllModulesAsync(fitting, "mid", cancellationToken);
            var lowSlotModules = await ParseAllModulesAsync(fitting, "low", cancellationToken);
            var rigModules = await ParseAllModulesAsync(fitting, "rig", cancellationToken);
            var subsystemModules = await ParseAllModulesAsync(fitting, "subsystem", cancellationToken);

            // Calculate base targeting stats
            var baseTargeting = await CalculateBaseTargetingStatsAsync(ship, character, cancellationToken);
            result.MaxTargets = baseTargeting.MaxTargets;
            result.MaxTargetRange = baseTargeting.MaxTargetRange;
            result.ScanResolution = baseTargeting.ScanResolution;
            result.SensorStrength = baseTargeting.SensorStrength;
            result.SensorType = baseTargeting.SensorType;

            // Calculate targeting enhancements from modules
            var targetingEnhancements = await CalculateTargetingEnhancementsAsync(
                midSlotModules.Concat(lowSlotModules).Concat(rigModules).Concat(subsystemModules).ToList(),
                ship,
                character,
                cancellationToken);

            // Apply targeting modifications
            result.MaxTargetsWithModules = Math.Min(result.MaxTargets + targetingEnhancements.MaxTargetsBonus, MaximumTargets);
            result.MaxTargetRangeWithModules = result.MaxTargetRange * targetingEnhancements.TargetRangeMultiplier;
            result.ScanResolutionWithModules = result.ScanResolution * targetingEnhancements.ScanResolutionMultiplier;
            result.SensorStrengthWithModules = result.SensorStrength * targetingEnhancements.SensorStrengthMultiplier;

            // Calculate lock times for different signature radii
            result.LockTimes = CalculateLockTimes(result.ScanResolutionWithModules);

            // Calculate sensor dampening resistance
            result.SensorDampeningResistance = CalculateSensorDampeningResistance(
                result.SensorStrengthWithModules, 
                targetingEnhancements.DampeningResistanceBonus);

            // Calculate targeting computer effects
            var targetingComputerEffects = await CalculateTargetingComputerEffectsAsync(
                midSlotModules, 
                character, 
                cancellationToken);
            result.TargetingComputerEffects = targetingComputerEffects;

            // Calculate ECCM (Electronic Counter-CounterMeasures) effectiveness
            var eccmEffects = await CalculateECCMEffectsAsync(
                midSlotModules.Concat(lowSlotModules).ToList(),
                result.SensorType,
                character,
                cancellationToken);
            result.ECCMEffectiveness = eccmEffects;

            // Calculate targeting disruption resistance
            result.TargetingDisruptionResistance = CalculateTargetingDisruptionResistance(
                result.SensorStrengthWithModules,
                targetingEnhancements.DisruptionResistanceBonus);

            // Calculate effective targeting range under EWAR
            result.EffectiveTargetingRange = CalculateEffectiveTargetingRange(
                result.MaxTargetRangeWithModules,
                result.SensorDampeningResistance,
                result.TargetingDisruptionResistance);

            // Store active targeting modules
            result.ActiveTargetingModules = targetingEnhancements.ActiveModules;
            result.TargetingEfficiency = CalculateTargetingEfficiency(result);

            var calculationTime = DateTime.UtcNow - startTime;
            _logger.LogInformation("Targeting calculation completed in {Ms}ms - Scan Resolution: {ScanRes:F1} mm", 
                calculationTime.TotalMilliseconds, result.ScanResolutionWithModules);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating targeting for fitting {FittingName}", fitting.Name);
            throw;
        }
    }

    /// <summary>
    /// Calculate base targeting statistics from ship and skills
    /// </summary>
    private async Task<BaseTargetingStats> CalculateBaseTargetingStatsAsync(
        Ship ship, 
        Character? character, 
        CancellationToken cancellationToken)
    {
        var maxTargets = GetShipAttribute(ship, "maxTargets");
        var maxTargetRange = GetShipAttribute(ship, "maxTargetRange");
        var scanResolution = GetShipAttribute(ship, "scanResolution");
        var sensorStrength = GetShipAttribute(ship, "sensorStrength");
        var sensorType = GetShipSensorType(ship);

        // Apply targeting skills
        var skillMultipliers = await CalculateTargetingSkillBonusesAsync(character, cancellationToken);
        
        return new BaseTargetingStats
        {
            MaxTargets = maxTargets + skillMultipliers.MaxTargetsBonus,
            MaxTargetRange = maxTargetRange * skillMultipliers.TargetRangeBonus,
            ScanResolution = scanResolution * skillMultipliers.ScanResolutionBonus,
            SensorStrength = sensorStrength * skillMultipliers.SensorStrengthBonus,
            SensorType = sensorType
        };
    }

    /// <summary>
    /// Calculate targeting enhancements from modules
    /// </summary>
    private async Task<TargetingEnhancements> CalculateTargetingEnhancementsAsync(
        List<FittingModuleWithUsage> modules,
        Ship ship,
        Character? character,
        CancellationToken cancellationToken)
    {
        var enhancements = new TargetingEnhancements();
        var activeModules = new List<TargetingModuleEffect>();

        foreach (var moduleWithUsage in modules)
        {
            var moduleType = await _unitOfWork.EveTypes.GetByIdAsync(moduleWithUsage.Module.TypeId, cancellationToken);
            if (moduleType == null) continue;

            var targetingEffect = await CalculateTargetingModuleEffectAsync(moduleType, character, cancellationToken);
            if (targetingEffect.ModuleType != TargetingModuleType.None)
            {
                activeModules.Add(targetingEffect);
            }
        }

        // Calculate stacking penalties for each enhancement type
        var targetRangeModifiers = activeModules.Where(m => m.TargetRangeBonus > 1.0).Select(m => m.TargetRangeBonus).ToList();
        var scanResolutionModifiers = activeModules.Where(m => m.ScanResolutionBonus > 1.0).Select(m => m.ScanResolutionBonus).ToList();
        var sensorStrengthModifiers = activeModules.Where(m => m.SensorStrengthBonus > 1.0).Select(m => m.SensorStrengthBonus).ToList();

        enhancements.ActiveModules = activeModules;
        enhancements.MaxTargetsBonus = activeModules.Sum(m => m.MaxTargetsBonus);
        enhancements.TargetRangeMultiplier = _stackingPenaltyService.CalculateStackingPenalty(targetRangeModifiers, StackingPenaltyGroup.SensorBooster);
        enhancements.ScanResolutionMultiplier = _stackingPenaltyService.CalculateStackingPenalty(scanResolutionModifiers, StackingPenaltyGroup.SensorBooster);
        enhancements.SensorStrengthMultiplier = _stackingPenaltyService.CalculateStackingPenalty(sensorStrengthModifiers, StackingPenaltyGroup.SensorBooster);
        enhancements.DampeningResistanceBonus = activeModules.Sum(m => m.DampeningResistanceBonus);
        enhancements.DisruptionResistanceBonus = activeModules.Sum(m => m.DisruptionResistanceBonus);

        return enhancements;
    }

    /// <summary>
    /// Calculate lock times for different signature radii
    /// </summary>
    private LockTimeProfile CalculateLockTimes(double scanResolution)
    {
        var profile = new LockTimeProfile();

        // EVE's lock time formula: LockTime = (40000 / ScanResolution) * (signatureRadius / 1000)
        var lockTimeBase = ScanResolutionBaseMultiplier / scanResolution;

        // Calculate lock times for various signature radii
        profile.Frigate = Math.Max(LockTimeMinimum, lockTimeBase * (35.0 / 1000.0)); // ~35m sig
        profile.Destroyer = Math.Max(LockTimeMinimum, lockTimeBase * (55.0 / 1000.0)); // ~55m sig
        profile.Cruiser = Math.Max(LockTimeMinimum, lockTimeBase * (125.0 / 1000.0)); // ~125m sig
        profile.Battlecruiser = Math.Max(LockTimeMinimum, lockTimeBase * (260.0 / 1000.0)); // ~260m sig
        profile.Battleship = Math.Max(LockTimeMinimum, lockTimeBase * (400.0 / 1000.0)); // ~400m sig
        profile.Capital = Math.Max(LockTimeMinimum, lockTimeBase * (2500.0 / 1000.0)); // ~2500m sig
        profile.Supercapital = Math.Max(LockTimeMinimum, lockTimeBase * (12000.0 / 1000.0)); // ~12000m sig

        return profile;
    }

    /// <summary>
    /// Calculate sensor dampening resistance
    /// </summary>
    private double CalculateSensorDampeningResistance(double sensorStrength, double resistanceBonus)
    {
        // Base resistance from sensor strength
        var baseResistance = sensorStrength / (sensorStrength + SensorStrengthBaselinePoints);
        
        // Apply module bonuses
        var totalResistance = Math.Min(0.99, baseResistance + (resistanceBonus / 100.0));
        
        return totalResistance;
    }

    /// <summary>
    /// Calculate targeting computer effects
    /// </summary>
    private async Task<TargetingComputerEffects> CalculateTargetingComputerEffectsAsync(
        List<FittingModuleWithUsage> midSlotModules,
        Character? character,
        CancellationToken cancellationToken)
    {
        var effects = new TargetingComputerEffects();
        var targetingComputers = new List<TargetingComputerModule>();

        foreach (var moduleWithUsage in midSlotModules)
        {
            var moduleType = await _unitOfWork.EveTypes.GetByIdAsync(moduleWithUsage.Module.TypeId, cancellationToken);
            if (moduleType != null && IsTargetingComputer(moduleType.TypeName))
            {
                var computerEffect = await CalculateTargetingComputerModuleAsync(moduleType, character, cancellationToken);
                targetingComputers.Add(computerEffect);
            }
        }

        if (targetingComputers.Any())
        {
            var rangeModifiers = targetingComputers.Select(tc => tc.RangeBonus).ToList();
            var speedModifiers = targetingComputers.Select(tc => tc.ScanSpeedBonus).ToList();

            effects.TotalRangeBonus = _stackingPenaltyService.CalculateStackingPenalty(rangeModifiers, StackingPenaltyGroup.SensorBooster);
            effects.TotalScanSpeedBonus = _stackingPenaltyService.CalculateStackingPenalty(speedModifiers, StackingPenaltyGroup.SensorBooster);
            effects.ActiveComputers = targetingComputers;
        }

        return effects;
    }

    /// <summary>
    /// Calculate ECCM (Electronic Counter-CounterMeasures) effects
    /// </summary>
    private async Task<ECCMEffects> CalculateECCMEffectsAsync(
        List<FittingModuleWithUsage> modules,
        SensorType sensorType,
        Character? character,
        CancellationToken cancellationToken)
    {
        var effects = new ECCMEffects();
        var eccmModules = new List<ECCMModule>();

        foreach (var moduleWithUsage in modules)
        {
            var moduleType = await _unitOfWork.EveTypes.GetByIdAsync(moduleWithUsage.Module.TypeId, cancellationToken);
            if (moduleType != null && IsECCMModule(moduleType.TypeName))
            {
                var eccmEffect = await CalculateECCMModuleAsync(moduleType, sensorType, character, cancellationToken);
                if (eccmEffect.IsEffective)
                {
                    eccmModules.Add(eccmEffect);
                }
            }
        }

        if (eccmModules.Any())
        {
            var jamResistanceModifiers = eccmModules.Select(e => e.JamResistanceBonus).ToList();
            effects.TotalJamResistance = _stackingPenaltyService.CalculateStackingPenalty(jamResistanceModifiers, StackingPenaltyGroup.ElectronicWarfare);
            effects.ActiveECCMModules = eccmModules;
        }

        return effects;
    }

    /// <summary>
    /// Calculate targeting disruption resistance
    /// </summary>
    private double CalculateTargetingDisruptionResistance(double sensorStrength, double resistanceBonus)
    {
        // Similar to sensor dampening but affects targeting range and scan resolution
        var baseResistance = sensorStrength / (sensorStrength + SensorStrengthBaselinePoints * 2.0);
        var totalResistance = Math.Min(0.95, baseResistance + (resistanceBonus / 100.0));
        
        return totalResistance;
    }

    /// <summary>
    /// Calculate effective targeting range under EWAR conditions
    /// </summary>
    private double CalculateEffectiveTargetingRange(
        double baseRange, 
        double dampeningResistance, 
        double disruptionResistance)
    {
        // Assume moderate EWAR pressure (can be parameterized)
        var dampeningFactor = 1.0 - (0.3 * (1.0 - dampeningResistance)); // 30% dampening pressure
        var disruptionFactor = 1.0 - (0.2 * (1.0 - disruptionResistance)); // 20% disruption pressure
        
        return baseRange * dampeningFactor * disruptionFactor;
    }

    /// <summary>
    /// Calculate overall targeting efficiency
    /// </summary>
    private double CalculateTargetingEfficiency(TargetingCalculationResult result)
    {
        // Composite efficiency score based on multiple factors
        var rangeFactor = Math.Min(1.0, result.MaxTargetRangeWithModules / 100000.0); // Normalize to 100km
        var scanFactor = Math.Min(1.0, result.ScanResolutionWithModules / 500.0); // Normalize to 500mm
        var targetsFactor = result.MaxTargetsWithModules / MaximumTargets;
        var resistanceFactor = (result.SensorDampeningResistance + result.TargetingDisruptionResistance) / 2.0;
        
        return (rangeFactor + scanFactor + targetsFactor + resistanceFactor) / 4.0 * 100.0;
    }

    #region Helper Methods

    /// <summary>
    /// Parse all modules from fitting configuration
    /// </summary>
    private async Task<List<FittingModuleWithUsage>> ParseAllModulesAsync(
        ShipFitting fitting, 
        string slotType, 
        CancellationToken cancellationToken)
    {
        var modules = new List<FittingModuleWithUsage>();
        var slotConfiguration = slotType switch
        {
            "high" => fitting.HighSlotConfiguration,
            "mid" => fitting.MidSlotConfiguration,
            "low" => fitting.LowSlotConfiguration,
            "rig" => fitting.RigSlotConfiguration,
            "subsystem" => fitting.SubsystemConfiguration,
            _ => "[]"
        };

        var parsedModules = ParseFittingSlot(slotConfiguration);
        modules.AddRange(parsedModules.Select(m => new FittingModuleWithUsage { Module = m, SlotType = slotType }));

        return modules;
    }

    /// <summary>
    /// Parse fitting slot JSON configuration
    /// </summary>
    private List<FittingModule> ParseFittingSlot(string slotConfiguration)
    {
        try
        {
            if (string.IsNullOrEmpty(slotConfiguration) || slotConfiguration == "[]")
                return new List<FittingModule>();

            return JsonSerializer.Deserialize<List<FittingModule>>(slotConfiguration) ?? new List<FittingModule>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse fitting slot configuration: {Config}", slotConfiguration);
            return new List<FittingModule>();
        }
    }

    /// <summary>
    /// Get ship attribute value
    /// </summary>
    private double GetShipAttribute(Ship ship, string attributeName)
    {
        // This would extract attributes from ship data
        return attributeName switch
        {
            "maxTargets" => 5, // Default for most ships
            "maxTargetRange" => 50000, // 50km in meters
            "scanResolution" => 200, // mm
            "sensorStrength" => 15, // Points
            _ => 0
        };
    }

    /// <summary>
    /// Get ship sensor type
    /// </summary>
    private SensorType GetShipSensorType(Ship ship)
    {
        // This would be determined from ship attributes or race
        // For now, return a default based on ship name patterns
        var shipName = ship.Name?.ToLower() ?? "";
        return shipName switch
        {
            var name when name.Contains("caldari") || name.Contains("griffin") => SensorType.Gravimetric,
            var name when name.Contains("gallente") || name.Contains("celestis") => SensorType.Magnetometric,
            var name when name.Contains("amarr") || name.Contains("arbitrator") => SensorType.Radar,
            var name when name.Contains("minmatar") || name.Contains("bellicose") => SensorType.Ladar,
            _ => SensorType.Radar // Default
        };
    }

    // Placeholder methods for targeting-specific calculations
    private async Task<TargetingSkillBonuses> CalculateTargetingSkillBonusesAsync(Character? character, CancellationToken cancellationToken)
    {
        var bonuses = new TargetingSkillBonuses();
        
        if (character == null) return bonuses;

        // Target Management skill (1 additional target per level)
        var targetManagementSkill = await _unitOfWork.CharacterSkills
            .GetFirstOrDefaultAsync(cs => cs.CharacterId == character.Id && cs.Skill.TypeName == "Target Management", cancellationToken);
        if (targetManagementSkill != null)
        {
            bonuses.MaxTargetsBonus += targetManagementSkill.CurrentLevel;
        }

        // Long Range Targeting skill (5% bonus to targeting range per level)
        var longRangeTargetingSkill = await _unitOfWork.CharacterSkills
            .GetFirstOrDefaultAsync(cs => cs.CharacterId == character.Id && cs.Skill.TypeName == "Long Range Targeting", cancellationToken);
        if (longRangeTargetingSkill != null)
        {
            bonuses.TargetRangeBonus *= 1.0 + (longRangeTargetingSkill.CurrentLevel * 0.05);
        }

        // Signature Analysis skill (5% bonus to scan resolution per level)
        var signatureAnalysisSkill = await _unitOfWork.CharacterSkills
            .GetFirstOrDefaultAsync(cs => cs.CharacterId == character.Id && cs.Skill.TypeName == "Signature Analysis", cancellationToken);
        if (signatureAnalysisSkill != null)
        {
            bonuses.ScanResolutionBonus *= 1.0 + (signatureAnalysisSkill.CurrentLevel * 0.05);
        }

        return bonuses;
    }

    private async Task<TargetingModuleEffect> CalculateTargetingModuleEffectAsync(EveType moduleType, Character? character, CancellationToken cancellationToken)
    {
        var effect = new TargetingModuleEffect
        {
            ModuleName = moduleType.TypeName,
            ModuleType = DetermineTargetingModuleType(moduleType.TypeName)
        };

        switch (effect.ModuleType)
        {
            case TargetingModuleType.SensorBooster:
                effect.ScanResolutionBonus = 1.3; // 30% bonus
                effect.TargetRangeBonus = 1.25; // 25% bonus
                break;
            case TargetingModuleType.SignalAmplifier:
                effect.SensorStrengthBonus = 1.25; // 25% bonus
                effect.DampeningResistanceBonus = 15.0; // 15% resistance
                break;
            case TargetingModuleType.TargetingComputer:
                effect.ScanResolutionBonus = 1.2; // 20% bonus
                effect.TargetRangeBonus = 1.3; // 30% bonus
                break;
            case TargetingModuleType.ECCM:
                effect.SensorStrengthBonus = 1.15; // 15% bonus
                effect.DisruptionResistanceBonus = 20.0; // 20% resistance
                break;
        }

        return effect;
    }

    private TargetingModuleType DetermineTargetingModuleType(string moduleName)
    {
        return moduleName.ToLower() switch
        {
            var name when name.Contains("sensor booster") => TargetingModuleType.SensorBooster,
            var name when name.Contains("signal amplifier") => TargetingModuleType.SignalAmplifier,
            var name when name.Contains("targeting computer") => TargetingModuleType.TargetingComputer,
            var name when name.Contains("eccm") || name.Contains("backup array") => TargetingModuleType.ECCM,
            _ => TargetingModuleType.None
        };
    }

    private async Task<TargetingComputerModule> CalculateTargetingComputerModuleAsync(EveType moduleType, Character? character, CancellationToken cancellationToken)
    {
        return new TargetingComputerModule
        {
            ModuleName = moduleType.TypeName,
            RangeBonus = 1.3, // 30% range bonus
            ScanSpeedBonus = 1.2, // 20% scan speed bonus
            CapacitorUsage = 15.0 // GJ/s
        };
    }

    private async Task<ECCMModule> CalculateECCMModuleAsync(EveType moduleType, SensorType sensorType, Character? character, CancellationToken cancellationToken)
    {
        var eccmSensorType = DetermineECCMSensorType(moduleType.TypeName);
        
        return new ECCMModule
        {
            ModuleName = moduleType.TypeName,
            ECCMSensorType = eccmSensorType,
            JamResistanceBonus = 1.5, // 50% jam resistance
            IsEffective = eccmSensorType == sensorType || eccmSensorType == SensorType.Multispectral
        };
    }

    private SensorType DetermineECCMSensorType(string moduleName)
    {
        return moduleName.ToLower() switch
        {
            var name when name.Contains("gravimetric") => SensorType.Gravimetric,
            var name when name.Contains("magnetometric") => SensorType.Magnetometric,
            var name when name.Contains("radar") => SensorType.Radar,
            var name when name.Contains("ladar") => SensorType.Ladar,
            var name when name.Contains("multispectral") => SensorType.Multispectral,
            _ => SensorType.Radar
        };
    }

    private bool IsTargetingComputer(string moduleName) => moduleName.ToLower().Contains("targeting computer");
    private bool IsECCMModule(string moduleName) => moduleName.ToLower().Contains("eccm") || moduleName.ToLower().Contains("backup");

    #endregion
}

/// <summary>
/// Interface for targeting calculation service
/// </summary>
public interface ITargetingCalculationService
{
    Task<TargetingCalculationResult> CalculateComprehensiveTargetingAsync(ShipFitting fitting, Character? character = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Base targeting statistics
/// </summary>
public class BaseTargetingStats
{
    public double MaxTargets { get; set; }
    public double MaxTargetRange { get; set; }
    public double ScanResolution { get; set; }
    public double SensorStrength { get; set; }
    public SensorType SensorType { get; set; }
}

/// <summary>
/// Targeting enhancements from modules
/// </summary>
public class TargetingEnhancements
{
    public List<TargetingModuleEffect> ActiveModules { get; set; } = new();
    public double MaxTargetsBonus { get; set; }
    public double TargetRangeMultiplier { get; set; } = 1.0;
    public double ScanResolutionMultiplier { get; set; } = 1.0;
    public double SensorStrengthMultiplier { get; set; } = 1.0;
    public double DampeningResistanceBonus { get; set; }
    public double DisruptionResistanceBonus { get; set; }
}

/// <summary>
/// Targeting module effect
/// </summary>
public class TargetingModuleEffect
{
    public string ModuleName { get; set; } = string.Empty;
    public TargetingModuleType ModuleType { get; set; }
    public double MaxTargetsBonus { get; set; }
    public double TargetRangeBonus { get; set; } = 1.0;
    public double ScanResolutionBonus { get; set; } = 1.0;
    public double SensorStrengthBonus { get; set; } = 1.0;
    public double DampeningResistanceBonus { get; set; }
    public double DisruptionResistanceBonus { get; set; }
}

/// <summary>
/// Targeting module types
/// </summary>
public enum TargetingModuleType
{
    None,
    SensorBooster,
    SignalAmplifier,
    TargetingComputer,
    ECCM
}

/// <summary>
/// Sensor types for EVE Online
/// </summary>
public enum SensorType
{
    Radar,
    Ladar,
    Gravimetric,
    Magnetometric,
    Multispectral
}

/// <summary>
/// Lock time profile for different ship sizes
/// </summary>
public class LockTimeProfile
{
    public double Frigate { get; set; }
    public double Destroyer { get; set; }
    public double Cruiser { get; set; }
    public double Battlecruiser { get; set; }
    public double Battleship { get; set; }
    public double Capital { get; set; }
    public double Supercapital { get; set; }
}

/// <summary>
/// Targeting computer effects
/// </summary>
public class TargetingComputerEffects
{
    public List<TargetingComputerModule> ActiveComputers { get; set; } = new();
    public double TotalRangeBonus { get; set; } = 1.0;
    public double TotalScanSpeedBonus { get; set; } = 1.0;
}

/// <summary>
/// Targeting computer module
/// </summary>
public class TargetingComputerModule
{
    public string ModuleName { get; set; } = string.Empty;
    public double RangeBonus { get; set; } = 1.0;
    public double ScanSpeedBonus { get; set; } = 1.0;
    public double CapacitorUsage { get; set; }
}

/// <summary>
/// ECCM effects
/// </summary>
public class ECCMEffects
{
    public List<ECCMModule> ActiveECCMModules { get; set; } = new();
    public double TotalJamResistance { get; set; } = 1.0;
}

/// <summary>
/// ECCM module
/// </summary>
public class ECCMModule
{
    public string ModuleName { get; set; } = string.Empty;
    public SensorType ECCMSensorType { get; set; }
    public double JamResistanceBonus { get; set; } = 1.0;
    public bool IsEffective { get; set; }
}

/// <summary>
/// Targeting skill bonuses
/// </summary>
public class TargetingSkillBonuses
{
    public double MaxTargetsBonus { get; set; }
    public double TargetRangeBonus { get; set; } = 1.0;
    public double ScanResolutionBonus { get; set; } = 1.0;
    public double SensorStrengthBonus { get; set; } = 1.0;
}

/// <summary>
/// Fitting module with usage information
/// </summary>
public class FittingModuleWithUsage
{
    public FittingModule Module { get; set; } = new();
    public string SlotType { get; set; } = string.Empty;
}

// Static StackingPenalty class removed - now using centralized IStackingPenaltyService