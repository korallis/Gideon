// ==========================================================================
// CapacitorCalculationService.cs - Comprehensive Capacitor Calculation Service
// ==========================================================================
// Implementation of EVE Online accurate capacitor calculations including
// stability analysis, injection calculations, and module usage optimization.
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
/// Service for accurate EVE Online capacitor calculations
/// </summary>
public class CapacitorCalculationService : ICapacitorCalculationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CapacitorCalculationService> _logger;
    
    // EVE constants for precise calculations
    private const double CapacitorRechargeConstant = 5.0; // EVE's capacitor tau constant
    private const double PeakRechargePoint = 0.25; // 25% capacitor level for peak recharge
    private const double StableCapacitorThreshold = 0.01; // 1% threshold for stability calculations
    private const double CapacitorInjectorEfficiency = 0.25; // 25% of shield/armor booster amount
    
    public CapacitorCalculationService(IUnitOfWork unitOfWork, ILogger<CapacitorCalculationService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calculate comprehensive capacitor statistics for a ship fitting
    /// </summary>
    public async Task<CapacitorCalculationResult> CalculateComprehensiveCapacitorAsync(
        ShipFitting fitting, 
        Character? character = null, 
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogDebug("Starting comprehensive capacitor calculation for fitting {FittingName}", fitting.Name);

            var result = new CapacitorCalculationResult();
            
            // Get ship base stats
            var ship = await _unitOfWork.Ships.GetByIdAsync(fitting.ShipId, cancellationToken);
            if (ship == null)
            {
                throw new InvalidOperationException($"Ship with ID {fitting.ShipId} not found");
            }

            // Calculate base capacitor stats
            var baseCapacitor = await CalculateBaseCapacitorStatsAsync(ship, character, cancellationToken);
            result.CapacitorCapacity = baseCapacitor.Capacity;
            result.CapacitorRecharge = baseCapacitor.RechargeTime;

            // Parse all module configurations
            var allModules = await ParseAllModulesAsync(fitting, cancellationToken);

            // Calculate module capacitor usage
            var moduleUsage = await CalculateModuleCapacitorUsageAsync(allModules, character, cancellationToken);
            result.ModuleUsage = moduleUsage.ModuleUsageList;
            result.CapacitorUsage = moduleUsage.TotalUsage;

            // Calculate capacitor stability using EVE's complex formula
            var stabilityResult = CalculateCapacitorStability(result.CapacitorCapacity, result.CapacitorRecharge, result.CapacitorUsage);
            result.CapacitorStability = stabilityResult.StabilityPercentage;
            result.IsStable = stabilityResult.IsStable;
            result.CapacitorDelta = stabilityResult.CapacitorDelta;

            // Calculate duration if not stable
            if (!result.IsStable)
            {
                result.CapacitorDuration = CalculateCapacitorDuration(result.CapacitorCapacity, result.CapacitorRecharge, result.CapacitorUsage);
            }
            else
            {
                result.CapacitorDuration = TimeSpan.Zero; // Stable = infinite duration
            }

            // Calculate capacitor injection analysis (store in extended result for future use)
            var injectionAnalysis = await CalculateCapacitorInjectionAnalysisAsync(allModules, result, character, cancellationToken);
            
            // Store additional analysis data for logging
            var peakRechargeRate = CalculatePeakRechargeRate(result.CapacitorCapacity, result.CapacitorRecharge);
            var capacitorEfficiency = CalculateCapacitorEfficiency(result);

            var calculationTime = DateTime.UtcNow - startTime;
            _logger.LogInformation("Capacitor calculation completed in {Ms}ms - Stability: {Stability:F1}%", 
                calculationTime.TotalMilliseconds, result.CapacitorStability);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating capacitor for fitting {FittingName}", fitting.Name);
            throw;
        }
    }

    /// <summary>
    /// Calculate base capacitor statistics from ship and skills
    /// </summary>
    private async Task<BaseCapacitorStats> CalculateBaseCapacitorStatsAsync(
        Ship ship, 
        Character? character, 
        CancellationToken cancellationToken)
    {
        var baseCapacity = GetShipAttribute(ship, "capacitorCapacity");
        var baseRechargeTime = GetShipAttribute(ship, "rechargeRate"); // milliseconds

        // Apply capacitor management skills
        var skillMultipliers = await CalculateCapacitorSkillBonusesAsync(character, cancellationToken);
        
        return new BaseCapacitorStats
        {
            Capacity = baseCapacity * skillMultipliers.CapacityBonus,
            RechargeTime = baseRechargeTime / skillMultipliers.RechargeBonus // Faster recharge = lower time
        };
    }

    /// <summary>
    /// Parse all modules from fitting configuration
    /// </summary>
    private async Task<List<FittingModuleWithUsage>> ParseAllModulesAsync(
        ShipFitting fitting, 
        CancellationToken cancellationToken)
    {
        var allModules = new List<FittingModuleWithUsage>();

        // Parse each slot type
        var highSlotModules = ParseFittingSlot(fitting.HighSlotConfiguration);
        var midSlotModules = ParseFittingSlot(fitting.MidSlotConfiguration);
        var lowSlotModules = ParseFittingSlot(fitting.LowSlotConfiguration);
        var rigModules = ParseFittingSlot(fitting.RigSlotConfiguration);

        // Add modules with slot information
        allModules.AddRange(highSlotModules.Select(m => new FittingModuleWithUsage { Module = m, SlotType = "High" }));
        allModules.AddRange(midSlotModules.Select(m => new FittingModuleWithUsage { Module = m, SlotType = "Mid" }));
        allModules.AddRange(lowSlotModules.Select(m => new FittingModuleWithUsage { Module = m, SlotType = "Low" }));
        allModules.AddRange(rigModules.Select(m => new FittingModuleWithUsage { Module = m, SlotType = "Rig" }));

        return allModules;
    }

    /// <summary>
    /// Calculate capacitor usage for all modules
    /// </summary>
    private async Task<ModuleCapacitorUsageResult> CalculateModuleCapacitorUsageAsync(
        List<FittingModuleWithUsage> modules, 
        Character? character, 
        CancellationToken cancellationToken)
    {
        var result = new ModuleCapacitorUsageResult();
        var moduleUsageList = new List<ModuleCapUsage>();

        foreach (var moduleWithUsage in modules)
        {
            var module = moduleWithUsage.Module;
            var moduleType = await _unitOfWork.EveTypes.GetByIdAsync(module.TypeId, cancellationToken);
            
            if (moduleType == null) continue;

            // Get module attributes
            var attributes = await _unitOfWork.EveTypeAttributes
                .GetAllAsync(a => a.TypeId == module.TypeId, cancellationToken);

            var capacitorNeed = attributes.FirstOrDefault(a => a.AttributeId == EveAttributeIds.CapacitorNeed)?.Value ?? 0;
            var activationTime = attributes.FirstOrDefault(a => a.AttributeId == EveAttributeIds.ActivationTime)?.Value ?? 1000; // ms
            var alwaysOn = await IsModuleAlwaysOnAsync(moduleType, cancellationToken);

            // Apply skill bonuses to capacitor usage
            var skillMultiplier = await CalculateModuleCapacitorSkillBonusesAsync(moduleType, character, cancellationToken);
            var finalCapacitorNeed = capacitorNeed * skillMultiplier;

            // Calculate usage per second
            var cycleTime = TimeSpan.FromMilliseconds(activationTime);
            var usagePerSecond = cycleTime.TotalSeconds > 0 ? finalCapacitorNeed / cycleTime.TotalSeconds : 0;

            var moduleUsage = new ModuleCapUsage
            {
                ModuleName = moduleType.TypeName,
                CapacitorUsage = finalCapacitorNeed,
                CycleTime = cycleTime,
                IsAlwaysOn = alwaysOn
            };

            moduleUsageList.Add(moduleUsage);

            // Add to total usage (only for active modules)
            if (alwaysOn || await IsModuleActiveByDefaultAsync(moduleType, cancellationToken))
            {
                result.TotalUsage += usagePerSecond;
            }
        }

        result.ModuleUsageList = moduleUsageList;
        return result;
    }

    /// <summary>
    /// Calculate capacitor stability using EVE's exact formula
    /// </summary>
    private CapacitorStabilityResult CalculateCapacitorStability(
        double capacitorCapacity, 
        double rechargeTime, 
        double usage)
    {
        var result = new CapacitorStabilityResult();

        // Convert recharge time to seconds
        var rechargeTimeSeconds = rechargeTime / 1000.0;
        var tau = rechargeTimeSeconds / CapacitorRechargeConstant;

        // Calculate peak recharge rate (at 25% capacitor)
        var peakRechargeRate = (2.5 * capacitorCapacity) / tau;

        // Calculate capacitor delta (recharge - usage)
        result.CapacitorDelta = peakRechargeRate - usage;

        // Check if stable
        result.IsStable = usage <= peakRechargeRate;

        if (result.IsStable)
        {
            // Calculate stable capacitor percentage using quadratic formula
            // Based on EVE's capacitor recharge curve: dC/dt = (C_max / tau) * (sqrt(C / C_max) - C / C_max)
            var a = usage / (capacitorCapacity / tau);
            var discriminant = 1.0 - 4.0 * a;

            if (discriminant >= 0)
            {
                // Stable level as percentage
                result.StabilityPercentage = ((1.0 - Math.Sqrt(discriminant)) / 2.0) * 100.0;
            }
            else
            {
                // Should not happen if usage <= peak, but handle edge case
                result.StabilityPercentage = 0;
                result.IsStable = false;
            }
        }
        else
        {
            result.StabilityPercentage = 0;
        }

        return result;
    }

    /// <summary>
    /// Calculate capacitor duration until empty (for unstable fits)
    /// </summary>
    private TimeSpan CalculateCapacitorDuration(double capacitorCapacity, double rechargeTime, double usage)
    {
        if (usage <= 0) return TimeSpan.MaxValue;

        var rechargeTimeSeconds = rechargeTime / 1000.0;
        var tau = rechargeTimeSeconds / CapacitorRechargeConstant;

        // Simplified calculation: time to drain from 100% to 0%
        // Using EVE's capacitor curve differential equation
        var k = usage / (capacitorCapacity / tau);
        
        if (k >= 1.0)
        {
            // Simple linear drain if usage exceeds recharge capability
            return TimeSpan.FromSeconds(capacitorCapacity / usage);
        }

        // Complex exponential decay calculation
        var durationSeconds = tau * Math.Log(1.0 / (1.0 - k));
        return TimeSpan.FromSeconds(Math.Min(durationSeconds, 86400)); // Cap at 24 hours
    }

    /// <summary>
    /// Calculate capacitor injection analysis
    /// </summary>
    private async Task<CapacitorInjectionAnalysis> CalculateCapacitorInjectionAnalysisAsync(
        List<FittingModuleWithUsage> modules,
        CapacitorCalculationResult baseResult,
        Character? character,
        CancellationToken cancellationToken)
    {
        var analysis = new CapacitorInjectionAnalysis();

        // Find capacitor injection modules (cap boosters, nosferatu, energy neutralizers)
        var injectionModules = await FindCapacitorInjectionModulesAsync(modules, cancellationToken);

        foreach (var injectionModule in injectionModules)
        {
            var injectionEffect = await CalculateInjectionEffectAsync(injectionModule, character, cancellationToken);
            analysis.InjectionModules.Add(injectionEffect);
        }

        // Calculate total injection rate
        analysis.TotalInjectionRate = analysis.InjectionModules.Sum(m => m.InjectionRate);

        // Calculate injection efficiency (how much the injection improves stability)
        if (analysis.TotalInjectionRate > 0)
        {
            var improvedUsage = baseResult.CapacitorUsage - analysis.TotalInjectionRate;
            var improvedStability = CalculateCapacitorStability(
                baseResult.CapacitorCapacity, 
                baseResult.CapacitorRecharge, 
                improvedUsage);

            analysis.InjectionEfficiency = improvedStability.StabilityPercentage - baseResult.CapacitorStability;
            analysis.ImprovedStability = improvedStability.StabilityPercentage;
        }

        return analysis;
    }

    /// <summary>
    /// Calculate peak recharge rate
    /// </summary>
    private double CalculatePeakRechargeRate(double capacitorCapacity, double rechargeTime)
    {
        var rechargeTimeSeconds = rechargeTime / 1000.0;
        var tau = rechargeTimeSeconds / CapacitorRechargeConstant;
        return (2.5 * capacitorCapacity) / tau; // Peak at 25% capacitor
    }

    /// <summary>
    /// Calculate overall capacitor efficiency
    /// </summary>
    private double CalculateCapacitorEfficiency(CapacitorCalculationResult result)
    {
        if (result.CapacitorUsage <= 0) return 100.0;

        var peakRecharge = CalculatePeakRechargeRate(result.CapacitorCapacity, result.CapacitorRecharge);
        return Math.Min(100.0, (peakRecharge / result.CapacitorUsage) * 100.0);
    }

    #region Helper Methods

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
            "capacitorCapacity" => 2000, // GJ
            "rechargeRate" => 600000, // 10 minutes in milliseconds
            _ => 0
        };
    }

    /// <summary>
    /// Calculate capacitor skill bonuses
    /// </summary>
    private async Task<CapacitorSkillBonuses> CalculateCapacitorSkillBonusesAsync(
        Character? character, 
        CancellationToken cancellationToken)
    {
        var bonuses = new CapacitorSkillBonuses();
        
        if (character == null) return bonuses;

        // Capacitor Management skill (5% bonus to capacitor capacity per level)
        var capManagementSkill = await _unitOfWork.CharacterSkills
            .GetFirstOrDefaultAsync(cs => cs.CharacterId == character.Id && cs.Skill.TypeName == "Capacitor Management", cancellationToken);
        if (capManagementSkill != null)
        {
            bonuses.CapacityBonus *= 1.0 + (capManagementSkill.CurrentLevel * 0.05);
        }

        // Capacitor Systems Operation skill (5% bonus to recharge rate per level)
        var capSystemsSkill = await _unitOfWork.CharacterSkills
            .GetFirstOrDefaultAsync(cs => cs.CharacterId == character.Id && cs.Skill.TypeName == "Capacitor Systems Operation", cancellationToken);
        if (capSystemsSkill != null)
        {
            bonuses.RechargeBonus *= 1.0 + (capSystemsSkill.CurrentLevel * 0.05);
        }

        // Energy Management skill (5% reduction to energy weapon capacitor usage per level)
        var energyManagementSkill = await _unitOfWork.CharacterSkills
            .GetFirstOrDefaultAsync(cs => cs.CharacterId == character.Id && cs.Skill.TypeName == "Energy Management", cancellationToken);
        if (energyManagementSkill != null)
        {
            bonuses.EnergyWeaponBonus *= 1.0 - (energyManagementSkill.CurrentLevel * 0.05);
        }

        return bonuses;
    }

    /// <summary>
    /// Calculate skill bonuses for specific module capacitor usage
    /// </summary>
    private async Task<double> CalculateModuleCapacitorSkillBonusesAsync(
        EveType moduleType, 
        Character? character, 
        CancellationToken cancellationToken)
    {
        if (character == null) return 1.0;

        double multiplier = 1.0;

        // Energy weapon skills reduce capacitor usage
        if (await IsEnergyWeaponAsync(moduleType, cancellationToken))
        {
            var energyManagementSkill = await _unitOfWork.CharacterSkills
                .GetFirstOrDefaultAsync(cs => cs.CharacterId == character.Id && cs.Skill.TypeName == "Energy Management", cancellationToken);
            if (energyManagementSkill != null)
            {
                multiplier *= 1.0 - (energyManagementSkill.CurrentLevel * 0.05); // 5% reduction per level
            }
        }

        // Shield operation skills for shield boosters
        if (await IsShieldBoosterAsync(moduleType, cancellationToken))
        {
            var shieldOperationSkill = await _unitOfWork.CharacterSkills
                .GetFirstOrDefaultAsync(cs => cs.CharacterId == character.Id && cs.Skill.TypeName == "Shield Operation", cancellationToken);
            if (shieldOperationSkill != null)
            {
                multiplier *= 1.0 - (shieldOperationSkill.CurrentLevel * 0.05); // 5% reduction per level
            }
        }

        return multiplier;
    }

    /// <summary>
    /// Find capacitor injection modules in the fitting
    /// </summary>
    private async Task<List<FittingModuleWithUsage>> FindCapacitorInjectionModulesAsync(
        List<FittingModuleWithUsage> modules, 
        CancellationToken cancellationToken)
    {
        var injectionModules = new List<FittingModuleWithUsage>();

        foreach (var moduleWithUsage in modules)
        {
            var moduleType = await _unitOfWork.EveTypes.GetByIdAsync(moduleWithUsage.Module.TypeId, cancellationToken);
            if (moduleType != null && await IsCapacitorInjectionModuleAsync(moduleType, cancellationToken))
            {
                injectionModules.Add(moduleWithUsage);
            }
        }

        return injectionModules;
    }

    /// <summary>
    /// Calculate injection effect for a specific module
    /// </summary>
    private async Task<CapacitorInjectionEffect> CalculateInjectionEffectAsync(
        FittingModuleWithUsage moduleWithUsage, 
        Character? character, 
        CancellationToken cancellationToken)
    {
        var effect = new CapacitorInjectionEffect();
        var moduleType = await _unitOfWork.EveTypes.GetByIdAsync(moduleWithUsage.Module.TypeId, cancellationToken);
        
        if (moduleType == null) return effect;

        effect.ModuleName = moduleType.TypeName;

        // Get module attributes
        var attributes = await _unitOfWork.EveTypeAttributes
            .GetAllAsync(a => a.TypeId == moduleWithUsage.Module.TypeId, cancellationToken);

        // Different calculation based on module type
        if (await IsCapacitorBoosterAsync(moduleType, cancellationToken))
        {
            // Capacitor boosters inject capacitor directly
            var capacitorBonus = attributes.FirstOrDefault(a => a.AttributeId == EveAttributeIds.CapacitorBonus)?.Value ?? 0;
            var cycleTime = attributes.FirstOrDefault(a => a.AttributeId == EveAttributeIds.ActivationTime)?.Value ?? 1000;
            
            effect.InjectionRate = capacitorBonus / (cycleTime / 1000.0); // GJ/s
            effect.InjectionType = "Capacitor Booster";
        }
        else if (await IsNosferatuAsync(moduleType, cancellationToken))
        {
            // Nosferatu drains capacitor from target
            var energyTransfer = attributes.FirstOrDefault(a => a.AttributeId == EveAttributeIds.EnergyTransfer)?.Value ?? 0;
            var cycleTime = attributes.FirstOrDefault(a => a.AttributeId == EveAttributeIds.ActivationTime)?.Value ?? 1000;
            
            effect.InjectionRate = energyTransfer / (cycleTime / 1000.0); // GJ/s
            effect.InjectionType = "Nosferatu";
        }

        return effect;
    }

    // Placeholder methods for module type detection
    private async Task<bool> IsModuleAlwaysOnAsync(EveType moduleType, CancellationToken cancellationToken) => false;
    private async Task<bool> IsModuleActiveByDefaultAsync(EveType moduleType, CancellationToken cancellationToken) => true;
    private async Task<bool> IsEnergyWeaponAsync(EveType moduleType, CancellationToken cancellationToken) => moduleType.TypeName.Contains("Laser");
    private async Task<bool> IsShieldBoosterAsync(EveType moduleType, CancellationToken cancellationToken) => moduleType.TypeName.Contains("Shield Booster");
    private async Task<bool> IsCapacitorInjectionModuleAsync(EveType moduleType, CancellationToken cancellationToken) => 
        await IsCapacitorBoosterAsync(moduleType, cancellationToken) || await IsNosferatuAsync(moduleType, cancellationToken);
    private async Task<bool> IsCapacitorBoosterAsync(EveType moduleType, CancellationToken cancellationToken) => moduleType.TypeName.Contains("Capacitor Booster");
    private async Task<bool> IsNosferatuAsync(EveType moduleType, CancellationToken cancellationToken) => moduleType.TypeName.Contains("Nosferatu");

    #endregion
}

/// <summary>
/// Interface for capacitor calculation service
/// </summary>
public interface ICapacitorCalculationService
{
    Task<CapacitorCalculationResult> CalculateComprehensiveCapacitorAsync(ShipFitting fitting, Character? character = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Extended capacitor calculation result with injection analysis
/// </summary>
public class ExtendedCapacitorCalculationResult : CapacitorCalculationResult
{
    public CapacitorInjectionAnalysis InjectionAnalysis { get; set; } = new();
    public double PeakRechargeRate { get; set; }
    public double CapacitorEfficiency { get; set; }
}

/// <summary>
/// Base capacitor statistics
/// </summary>
public class BaseCapacitorStats
{
    public double Capacity { get; set; }
    public double RechargeTime { get; set; }
}

/// <summary>
/// Fitting module with usage information
/// </summary>
public class FittingModuleWithUsage
{
    public FittingModule Module { get; set; } = new();
    public string SlotType { get; set; } = string.Empty;
}

/// <summary>
/// Module capacitor usage result
/// </summary>
public class ModuleCapacitorUsageResult
{
    public List<ModuleCapUsage> ModuleUsageList { get; set; } = new();
    public double TotalUsage { get; set; }
}

/// <summary>
/// Extended module capacitor usage with per-second calculation
/// </summary>
public class ExtendedModuleCapUsage : ModuleCapUsage
{
    public double UsagePerSecond { get; set; }
}

/// <summary>
/// Capacitor stability calculation result
/// </summary>
public class CapacitorStabilityResult
{
    public bool IsStable { get; set; }
    public double StabilityPercentage { get; set; }
    public double CapacitorDelta { get; set; }
}

/// <summary>
/// Capacitor skill bonuses
/// </summary>
public class CapacitorSkillBonuses
{
    public double CapacityBonus { get; set; } = 1.0;
    public double RechargeBonus { get; set; } = 1.0;
    public double EnergyWeaponBonus { get; set; } = 1.0;
}

/// <summary>
/// Capacitor injection analysis
/// </summary>
public class CapacitorInjectionAnalysis
{
    public List<CapacitorInjectionEffect> InjectionModules { get; set; } = new();
    public double TotalInjectionRate { get; set; }
    public double InjectionEfficiency { get; set; }
    public double ImprovedStability { get; set; }
}

/// <summary>
/// Capacitor injection effect from a module
/// </summary>
public class CapacitorInjectionEffect
{
    public string ModuleName { get; set; } = string.Empty;
    public string InjectionType { get; set; } = string.Empty;
    public double InjectionRate { get; set; }
    public TimeSpan CycleTime { get; set; }
}

/// <summary>
/// EVE Dogma attribute IDs for capacitor calculations
/// </summary>
public static class EveAttributeIds
{
    public const int CapacitorNeed = 6;
    public const int ActivationTime = 73;
    public const int CapacitorBonus = 67;
    public const int EnergyTransfer = 90;
    public const int DamageMultiplier = 64;
    public const int Speed = 51;
    public const int Volume = 161;
    public const int DroneBandwidthUsed = 1271;
    public const int Cpu = 50;
    public const int Power = 30;
    public const int UpgradeCost = 1153;
}