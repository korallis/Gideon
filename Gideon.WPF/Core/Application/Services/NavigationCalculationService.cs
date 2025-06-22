// ==========================================================================
// NavigationCalculationService.cs - Comprehensive Navigation Calculation Service
// ==========================================================================
// Implementation of EVE Online accurate navigation calculations including
// speed, agility, warp, and propulsion system performance.
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
/// Service for accurate EVE Online navigation and propulsion calculations
/// </summary>
public class NavigationCalculationService : INavigationCalculationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NavigationCalculationService> _logger;
    
    // EVE constants for precise calculations
    private const double AlignTimeConstant = 1000000.0; // EVE's align time calculation constant
    private const double WarpSpeedMultiplier = 3.0; // AU/s conversion factor
    private const double PropulsionEfficiencyBase = 1.0; // Base propulsion efficiency
    private const double AgilityConstant = 500000.0; // Agility calculation constant
    private const double MaxSubWarpVelocity = 7500.0; // Maximum sub-warp velocity in m/s
    
    public NavigationCalculationService(IUnitOfWork unitOfWork, ILogger<NavigationCalculationService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calculate comprehensive navigation statistics for a ship fitting
    /// </summary>
    public async Task<NavigationCalculationResult> CalculateComprehensiveNavigationAsync(
        ShipFitting fitting, 
        Character? character = null, 
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogDebug("Starting comprehensive navigation calculation for fitting {FittingName}", fitting.Name);

            var result = new NavigationCalculationResult();
            
            // Get ship base stats
            var ship = await _unitOfWork.Ships.GetByIdAsync(fitting.ShipId, cancellationToken);
            if (ship == null)
            {
                throw new InvalidOperationException($"Ship with ID {fitting.ShipId} not found");
            }

            // Parse module configurations
            var lowSlotModules = await ParseAllModulesAsync(fitting, "low", cancellationToken);
            var midSlotModules = await ParseAllModulesAsync(fitting, "mid", cancellationToken);
            var rigModules = await ParseAllModulesAsync(fitting, "rig", cancellationToken);
            var subsystemModules = await ParseAllModulesAsync(fitting, "subsystem", cancellationToken);

            // Calculate base speed and agility
            var baseNavigation = await CalculateBaseNavigationStatsAsync(ship, character, cancellationToken);
            result.MaxVelocity = baseNavigation.MaxVelocity;
            result.Agility = baseNavigation.Agility;
            result.Mass = baseNavigation.Mass;
            result.AlignTime = baseNavigation.AlignTime;

            // Calculate propulsion system effects
            var propulsionEffects = await CalculatePropulsionSystemEffectsAsync(
                lowSlotModules.Concat(midSlotModules).ToList(), 
                ship, 
                character, 
                cancellationToken);
            
            // Apply propulsion modifications
            result.MaxVelocityWithPropulsion = ApplyPropulsionVelocityModifications(result.MaxVelocity, propulsionEffects);
            result.AgilityWithPropulsion = ApplyPropulsionAgilityModifications(result.Agility, propulsionEffects);
            result.AlignTimeWithPropulsion = CalculateAlignTime(result.Mass, result.AgilityWithPropulsion);

            // Calculate warp statistics
            var warpStats = await CalculateWarpStatsAsync(ship, rigModules, subsystemModules, character, cancellationToken);
            result.WarpSpeed = warpStats.WarpSpeed;
            result.WarpAcceleration = warpStats.WarpAcceleration;
            result.WarpDeceleration = warpStats.WarpDeceleration;
            result.WarpCapacitorNeed = warpStats.WarpCapacitorNeed;

            // Calculate signature radius and targeting implications
            var signatureStats = await CalculateSignatureAndTargetingAsync(
                ship, 
                lowSlotModules.Concat(midSlotModules).Concat(rigModules).ToList(), 
                propulsionEffects,
                character, 
                cancellationToken);
            result.SignatureRadius = signatureStats.SignatureRadius;
            result.EffectiveSignatureRadius = signatureStats.EffectiveSignatureRadius;

            // Calculate advanced navigation metrics
            result.TurnRadius = CalculateTurnRadius(result.MaxVelocityWithPropulsion, result.AgilityWithPropulsion);
            result.AccelerationTime = CalculateAccelerationTime(result.MaxVelocityWithPropulsion, result.AgilityWithPropulsion);
            result.BrakingDistance = CalculateBrakingDistance(result.MaxVelocityWithPropulsion, result.AgilityWithPropulsion);

            // Store propulsion module details
            result.PropulsionModules = propulsionEffects.ActiveModules;
            result.PropulsionEfficiency = propulsionEffects.OverallEfficiency;

            var calculationTime = DateTime.UtcNow - startTime;
            _logger.LogInformation("Navigation calculation completed in {Ms}ms - Max Velocity: {Velocity:F1} m/s", 
                calculationTime.TotalMilliseconds, result.MaxVelocityWithPropulsion);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating navigation for fitting {FittingName}", fitting.Name);
            throw;
        }
    }

    /// <summary>
    /// Calculate base navigation statistics from ship and skills
    /// </summary>
    private async Task<BaseNavigationStats> CalculateBaseNavigationStatsAsync(
        Ship ship, 
        Character? character, 
        CancellationToken cancellationToken)
    {
        var baseMaxVelocity = GetShipAttribute(ship, "maxVelocity");
        var baseAgility = GetShipAttribute(ship, "agility");
        var baseMass = GetShipAttribute(ship, "mass");

        // Apply navigation skills
        var skillMultipliers = await CalculateNavigationSkillBonusesAsync(character, cancellationToken);
        
        var finalMaxVelocity = baseMaxVelocity * skillMultipliers.VelocityBonus;
        var finalAgility = baseAgility * skillMultipliers.AgilityBonus;
        var finalMass = baseMass; // Mass typically not affected by skills directly

        return new BaseNavigationStats
        {
            MaxVelocity = finalMaxVelocity,
            Agility = finalAgility,
            Mass = finalMass,
            AlignTime = CalculateAlignTime(finalMass, finalAgility)
        };
    }

    /// <summary>
    /// Calculate propulsion system effects from modules
    /// </summary>
    private async Task<PropulsionSystemEffects> CalculatePropulsionSystemEffectsAsync(
        List<FittingModuleWithUsage> modules,
        Ship ship,
        Character? character,
        CancellationToken cancellationToken)
    {
        var effects = new PropulsionSystemEffects();
        var activeModules = new List<PropulsionModuleEffect>();

        foreach (var moduleWithUsage in modules)
        {
            var moduleType = await _unitOfWork.EveTypes.GetByIdAsync(moduleWithUsage.Module.TypeId, cancellationToken);
            if (moduleType == null) continue;

            var propulsionEffect = await CalculatePropulsionModuleEffectAsync(moduleType, character, cancellationToken);
            if (propulsionEffect.ModuleType != PropulsionType.None)
            {
                activeModules.Add(propulsionEffect);
            }
        }

        effects.ActiveModules = activeModules;
        effects.VelocityMultiplier = CalculatePropulsionVelocityMultiplier(activeModules);
        effects.AgilityModifier = CalculatePropulsionAgilityModifier(activeModules);
        effects.SignatureMultiplier = CalculatePropulsionSignatureMultiplier(activeModules);
        effects.OverallEfficiency = CalculatePropulsionEfficiency(activeModules);

        return effects;
    }

    /// <summary>
    /// Calculate warp drive statistics
    /// </summary>
    private async Task<WarpDriveStats> CalculateWarpStatsAsync(
        Ship ship,
        List<FittingModuleWithUsage> rigModules,
        List<FittingModuleWithUsage> subsystemModules,
        Character? character,
        CancellationToken cancellationToken)
    {
        var baseWarpSpeed = GetShipAttribute(ship, "warpSpeedMultiplier");
        var baseWarpCapacitor = GetShipAttribute(ship, "warpCapacitorNeed");

        // Apply warp drive modifications from rigs and subsystems
        var warpModifiers = await GetWarpDriveModifiersAsync(rigModules.Concat(subsystemModules).ToList(), cancellationToken);
        var warpSkillBonuses = await CalculateWarpSkillBonusesAsync(character, cancellationToken);

        var finalWarpSpeed = baseWarpSpeed * _stackingPenaltyService.CalculateStackingPenalty(warpModifiers.SpeedModifiers, StackingPenaltyGroup.NavigationUpgrade) * warpSkillBonuses.WarpSpeedBonus;
        var finalWarpCapacitor = baseWarpCapacitor * _stackingPenaltyService.CalculateStackingPenalty(warpModifiers.CapacitorModifiers, StackingPenaltyGroup.NavigationUpgrade) * warpSkillBonuses.WarpCapacitorBonus;

        return new WarpDriveStats
        {
            WarpSpeed = finalWarpSpeed,
            WarpAcceleration = CalculateWarpAcceleration(finalWarpSpeed),
            WarpDeceleration = CalculateWarpDeceleration(finalWarpSpeed),
            WarpCapacitorNeed = finalWarpCapacitor
        };
    }

    /// <summary>
    /// Calculate signature radius and targeting effects
    /// </summary>
    private async Task<SignatureAndTargetingStats> CalculateSignatureAndTargetingAsync(
        Ship ship,
        List<FittingModuleWithUsage> modules,
        PropulsionSystemEffects propulsionEffects,
        Character? character,
        CancellationToken cancellationToken)
    {
        var baseSignatureRadius = GetShipAttribute(ship, "signatureRadius");

        // Apply signature modifications from modules
        var signatureModifiers = await GetSignatureRadiusModifiersAsync(modules, cancellationToken);
        var skillBonuses = await CalculateSignatureSkillBonusesAsync(character, cancellationToken);

        var finalSignatureRadius = baseSignatureRadius * 
                                   _stackingPenaltyService.CalculateStackingPenalty(signatureModifiers, StackingPenaltyGroup.NavigationUpgrade) * 
                                   skillBonuses * 
                                   propulsionEffects.SignatureMultiplier;

        // Calculate effective signature (includes velocity effects)
        var effectiveSignature = CalculateEffectiveSignatureRadius(
            finalSignatureRadius, 
            propulsionEffects.VelocityMultiplier);

        return new SignatureAndTargetingStats
        {
            SignatureRadius = finalSignatureRadius,
            EffectiveSignatureRadius = effectiveSignature
        };
    }

    /// <summary>
    /// Calculate EVE's align time formula
    /// </summary>
    private double CalculateAlignTime(double mass, double agility)
    {
        // EVE's align time formula: AlignTime = -ln(0.25) * inertia * mass / 1000000
        var inertia = agility; // In EVE, agility is the inertia modifier
        return -Math.Log(0.25) * inertia * mass / AlignTimeConstant;
    }

    /// <summary>
    /// Apply propulsion velocity modifications
    /// </summary>
    private double ApplyPropulsionVelocityModifications(double baseVelocity, PropulsionSystemEffects propulsionEffects)
    {
        var modifiedVelocity = baseVelocity * propulsionEffects.VelocityMultiplier;
        return Math.Min(modifiedVelocity, MaxSubWarpVelocity); // Cap at EVE's max sub-warp velocity
    }

    /// <summary>
    /// Apply propulsion agility modifications
    /// </summary>
    private double ApplyPropulsionAgilityModifications(double baseAgility, PropulsionSystemEffects propulsionEffects)
    {
        return baseAgility * propulsionEffects.AgilityModifier;
    }

    /// <summary>
    /// Calculate turn radius based on velocity and agility
    /// </summary>
    private double CalculateTurnRadius(double velocity, double agility)
    {
        // Simplified turn radius calculation
        return velocity * agility / 100.0;
    }

    /// <summary>
    /// Calculate acceleration time to maximum velocity
    /// </summary>
    private double CalculateAccelerationTime(double maxVelocity, double agility)
    {
        // Time to reach 99% of max velocity
        return agility * 3.0; // Simplified calculation
    }

    /// <summary>
    /// Calculate braking distance from maximum velocity
    /// </summary>
    private double CalculateBrakingDistance(double maxVelocity, double agility)
    {
        // Distance covered while braking from max velocity
        return maxVelocity * agility * 0.5;
    }

    /// <summary>
    /// Calculate warp acceleration
    /// </summary>
    private double CalculateWarpAcceleration(double warpSpeed)
    {
        // Warp acceleration scales with warp speed
        return warpSpeed * 2.0; // AU/s²
    }

    /// <summary>
    /// Calculate warp deceleration
    /// </summary>
    private double CalculateWarpDeceleration(double warpSpeed)
    {
        // Warp deceleration is typically faster than acceleration
        return warpSpeed * 3.0; // AU/s²
    }

    /// <summary>
    /// Calculate effective signature radius including velocity effects
    /// </summary>
    private double CalculateEffectiveSignatureRadius(double baseSignature, double velocityMultiplier)
    {
        // Signature blooms with velocity for targeting calculations
        var velocityFactor = Math.Max(1.0, velocityMultiplier / 2.0);
        return baseSignature * velocityFactor;
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
            "maxVelocity" => 200, // m/s
            "agility" => 0.5, // inertia modifier
            "mass" => 1000000, // kg
            "signatureRadius" => 50, // meters
            "warpSpeedMultiplier" => 3.0, // AU/s
            "warpCapacitorNeed" => 0.1, // GJ/AU
            _ => 0
        };
    }

    // Placeholder methods for module-specific calculations
    private async Task<NavigationSkillBonuses> CalculateNavigationSkillBonusesAsync(Character? character, CancellationToken cancellationToken)
    {
        var bonuses = new NavigationSkillBonuses();
        
        if (character == null) return bonuses;

        // Navigation skill (5% bonus to sub-warp speed per level)
        var navigationSkill = await _unitOfWork.CharacterSkills
            .GetFirstOrDefaultAsync(cs => cs.CharacterId == character.Id && cs.Skill.TypeName == "Navigation", cancellationToken);
        if (navigationSkill != null)
        {
            bonuses.VelocityBonus *= 1.0 + (navigationSkill.CurrentLevel * 0.05);
        }

        // Evasive Maneuvering skill (5% bonus to agility per level)
        var evasiveManeuveringSkill = await _unitOfWork.CharacterSkills
            .GetFirstOrDefaultAsync(cs => cs.CharacterId == character.Id && cs.Skill.TypeName == "Evasive Maneuvering", cancellationToken);
        if (evasiveManeuveringSkill != null)
        {
            bonuses.AgilityBonus *= 1.0 - (evasiveManeuveringSkill.CurrentLevel * 0.05); // Lower agility value = better
        }

        return bonuses;
    }

    private async Task<PropulsionModuleEffect> CalculatePropulsionModuleEffectAsync(EveType moduleType, Character? character, CancellationToken cancellationToken)
    {
        var effect = new PropulsionModuleEffect
        {
            ModuleName = moduleType.TypeName,
            ModuleType = DeterminePropulsionType(moduleType.TypeName)
        };

        // Get module attributes and calculate effects based on type
        var attributes = await _unitOfWork.EveTypeAttributes
            .GetAllAsync(a => a.TypeId == moduleType.Id, cancellationToken);

        switch (effect.ModuleType)
        {
            case PropulsionType.Afterburner:
                effect.VelocityBonus = 1.25; // 25% velocity bonus
                effect.SignaturePenalty = 1.0; // No signature penalty
                effect.CapacitorUsage = 2.0; // GJ/s
                break;
            case PropulsionType.Microwarpdrive:
                effect.VelocityBonus = 5.0; // 500% velocity bonus
                effect.SignaturePenalty = 6.0; // 600% signature penalty
                effect.CapacitorUsage = 25.0; // GJ/s
                break;
            case PropulsionType.MicrojumpDrive:
                effect.VelocityBonus = 1.0; // No continuous velocity bonus
                effect.SignaturePenalty = 1.0;
                effect.CapacitorUsage = 95.0; // Per activation
                break;
        }

        return effect;
    }

    private PropulsionType DeterminePropulsionType(string moduleName)
    {
        return moduleName.ToLower() switch
        {
            var name when name.Contains("afterburner") => PropulsionType.Afterburner,
            var name when name.Contains("microwarpdrive") || name.Contains("mwd") => PropulsionType.Microwarpdrive,
            var name when name.Contains("microjump") => PropulsionType.MicrojumpDrive,
            _ => PropulsionType.None
        };
    }

    private double CalculatePropulsionVelocityMultiplier(List<PropulsionModuleEffect> modules)
    {
        // Only the highest velocity bonus applies (modules don't stack)
        return modules.Any() ? modules.Max(m => m.VelocityBonus) : 1.0;
    }

    private double CalculatePropulsionAgilityModifier(List<PropulsionModuleEffect> modules)
    {
        // Propulsion modules typically don't affect agility directly
        return 1.0;
    }

    private double CalculatePropulsionSignatureMultiplier(List<PropulsionModuleEffect> modules)
    {
        // Signature penalty from active propulsion modules
        return modules.Any() ? modules.Max(m => m.SignaturePenalty) : 1.0;
    }

    private double CalculatePropulsionEfficiency(List<PropulsionModuleEffect> modules)
    {
        if (!modules.Any()) return 0;
        
        var velocityGain = modules.Max(m => m.VelocityBonus) - 1.0;
        var capacitorCost = modules.Where(m => m.VelocityBonus > 1.0).Sum(m => m.CapacitorUsage);
        
        return capacitorCost > 0 ? velocityGain / capacitorCost * 100 : 0;
    }

    private async Task<WarpDriveModifiers> GetWarpDriveModifiersAsync(List<FittingModuleWithUsage> modules, CancellationToken cancellationToken)
    {
        var modifiers = new WarpDriveModifiers();
        
        foreach (var moduleWithUsage in modules)
        {
            var moduleType = await _unitOfWork.EveTypes.GetByIdAsync(moduleWithUsage.Module.TypeId, cancellationToken);
            if (moduleType != null && IsWarpDriveModule(moduleType.TypeName))
            {
                // Low-grade Nomad implant set or warp speed rigs
                modifiers.SpeedModifiers.Add(1.1); // 10% bonus example
            }
        }

        return modifiers;
    }

    private async Task<WarpSkillBonuses> CalculateWarpSkillBonusesAsync(Character? character, CancellationToken cancellationToken)
    {
        var bonuses = new WarpSkillBonuses();
        
        if (character == null) return bonuses;

        // Warp Drive Operation skill (10% reduction in capacitor need per level)
        var warpDriveSkill = await _unitOfWork.CharacterSkills
            .GetFirstOrDefaultAsync(cs => cs.CharacterId == character.Id && cs.Skill.TypeName == "Warp Drive Operation", cancellationToken);
        if (warpDriveSkill != null)
        {
            bonuses.WarpCapacitorBonus *= 1.0 - (warpDriveSkill.CurrentLevel * 0.1);
        }

        return bonuses;
    }

    private async Task<IEnumerable<double>> GetSignatureRadiusModifiersAsync(List<FittingModuleWithUsage> modules, CancellationToken cancellationToken)
    {
        var modifiers = new List<double>();
        
        foreach (var moduleWithUsage in modules)
        {
            var moduleType = await _unitOfWork.EveTypes.GetByIdAsync(moduleWithUsage.Module.TypeId, cancellationToken);
            if (moduleType != null && IsSignatureModule(moduleType.TypeName))
            {
                modifiers.Add(0.9); // 10% signature reduction example
            }
        }

        return modifiers;
    }

    private async Task<double> CalculateSignatureSkillBonusesAsync(Character? character, CancellationToken cancellationToken)
    {
        if (character == null) return 1.0;

        // Signature Analysis skill affects signature resolution, not radius directly
        return 1.0;
    }

    private bool IsWarpDriveModule(string moduleName) => moduleName.ToLower().Contains("warp");
    private bool IsSignatureModule(string moduleName) => moduleName.ToLower().Contains("signature");

    #endregion
}

/// <summary>
/// Interface for navigation calculation service
/// </summary>
public interface INavigationCalculationService
{
    Task<NavigationCalculationResult> CalculateComprehensiveNavigationAsync(ShipFitting fitting, Character? character = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Base navigation statistics
/// </summary>
public class BaseNavigationStats
{
    public double MaxVelocity { get; set; }
    public double Agility { get; set; }
    public double Mass { get; set; }
    public double AlignTime { get; set; }
}

/// <summary>
/// Propulsion system effects
/// </summary>
public class PropulsionSystemEffects
{
    public List<PropulsionModuleEffect> ActiveModules { get; set; } = new();
    public double VelocityMultiplier { get; set; } = 1.0;
    public double AgilityModifier { get; set; } = 1.0;
    public double SignatureMultiplier { get; set; } = 1.0;
    public double OverallEfficiency { get; set; }
}

/// <summary>
/// Propulsion module effect
/// </summary>
public class PropulsionModuleEffect
{
    public string ModuleName { get; set; } = string.Empty;
    public PropulsionType ModuleType { get; set; }
    public double VelocityBonus { get; set; } = 1.0;
    public double SignaturePenalty { get; set; } = 1.0;
    public double CapacitorUsage { get; set; }
}

/// <summary>
/// Propulsion module types
/// </summary>
public enum PropulsionType
{
    None,
    Afterburner,
    Microwarpdrive,
    MicrojumpDrive
}

/// <summary>
/// Warp drive statistics
/// </summary>
public class WarpDriveStats
{
    public double WarpSpeed { get; set; }
    public double WarpAcceleration { get; set; }
    public double WarpDeceleration { get; set; }
    public double WarpCapacitorNeed { get; set; }
}

/// <summary>
/// Warp drive modifiers from modules
/// </summary>
public class WarpDriveModifiers
{
    public List<double> SpeedModifiers { get; set; } = new();
    public List<double> CapacitorModifiers { get; set; } = new();
}

/// <summary>
/// Navigation skill bonuses
/// </summary>
public class NavigationSkillBonuses
{
    public double VelocityBonus { get; set; } = 1.0;
    public double AgilityBonus { get; set; } = 1.0;
}

/// <summary>
/// Warp skill bonuses
/// </summary>
public class WarpSkillBonuses
{
    public double WarpSpeedBonus { get; set; } = 1.0;
    public double WarpCapacitorBonus { get; set; } = 1.0;
}

/// <summary>
/// Signature and targeting statistics
/// </summary>
public class SignatureAndTargetingStats
{
    public double SignatureRadius { get; set; }
    public double EffectiveSignatureRadius { get; set; }
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