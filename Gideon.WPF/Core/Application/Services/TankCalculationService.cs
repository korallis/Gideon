// ==========================================================================
// TankCalculationService.cs - Comprehensive Tank Calculation Service
// ==========================================================================
// Implementation of EVE Online accurate tank calculations for shield, armor,
// and hull systems with resistance profiles, repair rates, and effective HP.
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
/// Service for accurate EVE Online tank calculations
/// </summary>
public class TankCalculationService : ITankCalculationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TankCalculationService> _logger;
    private readonly IStackingPenaltyService _stackingPenaltyService;
    private readonly ISkillCalculationService _skillCalculationService;
    
    // EVE constants for precise calculations
    private const double PassiveShieldRegenPeakRate = 0.2; // 20% of shield capacity
    private const double ArmorRepairCycleTime = 5000.0; // 5 seconds in milliseconds
    private const double ShieldBoosterCycleTime = 4000.0; // 4 seconds in milliseconds
    
    public TankCalculationService(IUnitOfWork unitOfWork, ILogger<TankCalculationService> logger, IStackingPenaltyService stackingPenaltyService, ISkillCalculationService skillCalculationService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _stackingPenaltyService = stackingPenaltyService ?? throw new ArgumentNullException(nameof(stackingPenaltyService));
        _skillCalculationService = skillCalculationService ?? throw new ArgumentNullException(nameof(skillCalculationService));
    }

    /// <summary>
    /// Calculate comprehensive tank statistics for a ship fitting
    /// </summary>
    public async Task<TankCalculationResult> CalculateComprehensiveTankAsync(
        ShipFitting fitting, 
        Character? character = null, 
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogDebug("Starting comprehensive tank calculation for fitting {FittingName}", fitting.Name);

            var result = new TankCalculationResult();
            
            // Get ship base stats
            var ship = await _unitOfWork.Ships.GetByIdAsync(fitting.ShipId, cancellationToken);
            if (ship == null)
            {
                throw new InvalidOperationException($"Ship with ID {fitting.ShipId} not found");
            }

            // Parse module configurations
            var midSlotModules = ParseFittingSlot(fitting.MidSlotConfiguration);
            var lowSlotModules = ParseFittingSlot(fitting.LowSlotConfiguration);
            var rigModules = ParseFittingSlot(fitting.RigSlotConfiguration);
            var subsystemModules = ParseFittingSlot(fitting.SubsystemConfiguration);

            // Calculate shield tank
            var shieldTank = await CalculateShieldTankAsync(ship, midSlotModules, rigModules, character, cancellationToken);
            result.ShieldHp = shieldTank.TotalHitPoints;
            result.ShieldResistances = shieldTank.Resistances;
            result.PassiveShieldRegenRate = shieldTank.PassiveRegenRate;
            result.ShieldBoostAmount = shieldTank.ActiveRepairAmount;

            // Calculate armor tank
            var armorTank = await CalculateArmorTankAsync(ship, lowSlotModules, rigModules, character, cancellationToken);
            result.ArmorHp = armorTank.TotalHitPoints;
            result.ArmorResistances = armorTank.Resistances;
            result.ArmorRepairAmount = armorTank.ActiveRepairAmount;

            // Calculate hull tank
            var hullTank = await CalculateHullTankAsync(ship, lowSlotModules, rigModules, subsystemModules, character, cancellationToken);
            result.HullHp = hullTank.TotalHitPoints;
            result.HullResistances = hullTank.Resistances;

            // Calculate effective hit points (EHP) using the combined method
            result.TotalEhp = CalculateEffectiveHitPoints(result.ShieldHp, result.ShieldResistances) +
                             CalculateEffectiveHitPoints(result.ArmorHp, result.ArmorResistances) +
                             CalculateEffectiveHitPoints(result.HullHp, result.HullResistances);

            // Calculate shield and armor recharge rates
            result.ShieldRecharge = CalculateShieldRepairRate(result.PassiveShieldRegenRate, result.ShieldBoostAmount, shieldTank.RepairCycleTime);
            result.ArmorRepair = CalculateArmorRepairRate(result.ArmorRepairAmount, armorTank.RepairCycleTime);
            result.HullRepair = 0; // Hull typically doesn't have repair

            var calculationTime = DateTime.UtcNow - startTime;
            _logger.LogInformation("Tank calculation completed in {Ms}ms - Total EHP: {Ehp:F0}", 
                calculationTime.TotalMilliseconds, result.TotalEhp);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating tank for fitting {FittingName}", fitting.Name);
            throw;
        }
    }

    /// <summary>
    /// Calculate shield tank statistics
    /// </summary>
    private async Task<TankLayerResult> CalculateShieldTankAsync(
        Ship ship,
        List<FittingModule> midSlotModules,
        List<FittingModule> rigModules,
        Character? character,
        CancellationToken cancellationToken)
    {
        var result = new TankLayerResult();

        // Base shield HP
        var baseShieldHp = GetShipAttribute(ship, "shieldCapacity");
        
        // Apply shield extender modules
        var shieldExtenderBonuses = await GetShieldExtenderBonusesAsync(midSlotModules, cancellationToken);
        var shieldHpMultiplier = _stackingPenaltyService.CalculateStackingPenalty(shieldExtenderBonuses, StackingPenaltyGroup.ShieldBooster);
        
        // Apply skill bonuses using centralized skill calculation service
        var skillBonuses = await _skillCalculationService.CalculateComprehensiveSkillBonusesAsync(character, SkillCategory.Shields, cancellationToken);
        var skillMultiplier = skillBonuses.ShieldBonuses.ShieldCapacityMultiplier;
        
        result.TotalHitPoints = baseShieldHp * shieldHpMultiplier * skillMultiplier;

        // Calculate resistances
        result.Resistances = await CalculateShieldResistancesAsync(ship, midSlotModules, rigModules, character, cancellationToken);

        // Calculate passive regeneration
        var baseRegenTime = GetShipAttribute(ship, "shieldRechargeRate"); // milliseconds
        var regenMultipliers = await GetShieldRegenModifiersAsync(midSlotModules, rigModules, cancellationToken);
        var finalRegenTime = baseRegenTime / _stackingPenaltyService.CalculateStackingPenalty(regenMultipliers, StackingPenaltyGroup.ShieldBooster);
        result.PassiveRegenRate = CalculatePassiveShieldRegen(result.TotalHitPoints, finalRegenTime);

        // Calculate active shield boosting
        var shieldBoosters = await GetShieldBoosterModulesAsync(midSlotModules, cancellationToken);
        if (shieldBoosters.Any())
        {
            var (boostAmount, cycleTime) = await CalculateActiveShieldBoostAsync(shieldBoosters, character, cancellationToken);
            result.ActiveRepairAmount = boostAmount;
            result.RepairCycleTime = TimeSpan.FromMilliseconds(cycleTime);
        }

        return result;
    }

    /// <summary>
    /// Calculate armor tank statistics
    /// </summary>
    private async Task<TankLayerResult> CalculateArmorTankAsync(
        Ship ship,
        List<FittingModule> lowSlotModules,
        List<FittingModule> rigModules,
        Character? character,
        CancellationToken cancellationToken)
    {
        var result = new TankLayerResult();

        // Base armor HP
        var baseArmorHp = GetShipAttribute(ship, "armorHP");
        
        // Apply armor plate modules
        var armorPlateBonuses = await GetArmorPlateBonusesAsync(lowSlotModules, cancellationToken);
        var armorHpMultiplier = _stackingPenaltyService.CalculateStackingPenalty(armorPlateBonuses, StackingPenaltyGroup.ArmorRepairer);
        
        // Apply skill bonuses using centralized skill calculation service
        var skillBonuses = await _skillCalculationService.CalculateComprehensiveSkillBonusesAsync(character, SkillCategory.Armor, cancellationToken);
        var skillMultiplier = skillBonuses.ArmorBonuses.ArmorHitPointsMultiplier;
        
        result.TotalHitPoints = baseArmorHp * armorHpMultiplier * skillMultiplier;

        // Calculate resistances
        result.Resistances = await CalculateArmorResistancesAsync(ship, lowSlotModules, rigModules, character, cancellationToken);

        // Calculate active armor repair
        var armorRepairers = await GetArmorRepairerModulesAsync(lowSlotModules, cancellationToken);
        if (armorRepairers.Any())
        {
            var (repairAmount, cycleTime) = await CalculateActiveArmorRepairAsync(armorRepairers, character, cancellationToken);
            result.ActiveRepairAmount = repairAmount;
            result.RepairCycleTime = TimeSpan.FromMilliseconds(cycleTime);
        }

        return result;
    }

    /// <summary>
    /// Calculate hull tank statistics
    /// </summary>
    private async Task<TankLayerResult> CalculateHullTankAsync(
        Ship ship,
        List<FittingModule> lowSlotModules,
        List<FittingModule> rigModules,
        List<FittingModule> subsystemModules,
        Character? character,
        CancellationToken cancellationToken)
    {
        var result = new TankLayerResult();

        // Base hull HP
        var baseHullHp = GetShipAttribute(ship, "hp");
        
        // Apply hull upgrade modules (rare but possible)
        var hullUpgradeBonuses = await GetHullUpgradeBonusesAsync(lowSlotModules, rigModules, cancellationToken);
        var hullHpMultiplier = _stackingPenaltyService.CalculateStackingPenalty(hullUpgradeBonuses, StackingPenaltyGroup.HullResistance);
        
        // Apply subsystem bonuses for T3 ships
        var subsystemBonuses = await GetSubsystemHullBonusesAsync(subsystemModules, cancellationToken);
        var subsystemMultiplier = _stackingPenaltyService.CalculateStackingPenalty(subsystemBonuses, StackingPenaltyGroup.HullResistance);
        
        result.TotalHitPoints = baseHullHp * hullHpMultiplier * subsystemMultiplier;

        // Calculate resistances (hull has base 0% resistances, modified by modules)
        result.Resistances = await CalculateHullResistancesAsync(ship, lowSlotModules, rigModules, character, cancellationToken);

        // Hull typically has no active repair (except hull repairers which are rare)
        result.ActiveRepairAmount = 0;
        result.RepairCycleTime = TimeSpan.Zero;

        return result;
    }

    /// <summary>
    /// Calculate shield resistances with modules and skills
    /// </summary>
    private async Task<ResistanceProfile> CalculateShieldResistancesAsync(
        Ship ship,
        List<FittingModule> midSlotModules,
        List<FittingModule> rigModules,
        Character? character,
        CancellationToken cancellationToken)
    {
        var profile = new ResistanceProfile();

        // Get base ship resistances (most ships have 0% base shield resistances)
        var baseEmRes = GetShipAttribute(ship, "shieldEmDamageResonance");
        var baseThermalRes = GetShipAttribute(ship, "shieldThermalDamageResonance");
        var baseKineticRes = GetShipAttribute(ship, "shieldKineticDamageResonance");
        var baseExplosiveRes = GetShipAttribute(ship, "shieldExplosiveDamageResonance");

        // Apply shield hardener modules with stacking penalties
        var emHardeners = await GetResistanceModifiersAsync(midSlotModules.Concat(rigModules), "shieldEm", cancellationToken);
        var thermalHardeners = await GetResistanceModifiersAsync(midSlotModules.Concat(rigModules), "shieldThermal", cancellationToken);
        var kineticHardeners = await GetResistanceModifiersAsync(midSlotModules.Concat(rigModules), "shieldKinetic", cancellationToken);
        var explosiveHardeners = await GetResistanceModifiersAsync(midSlotModules.Concat(rigModules), "shieldExplosive", cancellationToken);

        // Apply skill bonuses using centralized skill calculation service
        var skillBonuses = await _skillCalculationService.CalculateComprehensiveSkillBonusesAsync(character, SkillCategory.Shields, cancellationToken);
        var skillBonus = 1.0; // Shield resistance skills don't typically provide base resistance bonuses

        // Calculate final resistances (EVE uses resonance values, where 1.0 = 0% resistance, 0.5 = 50% resistance)
        profile.EmResistance = (1.0 - (baseEmRes * _stackingPenaltyService.CalculateStackingPenalty(emHardeners, StackingPenaltyGroup.ShieldResistance) * skillBonus)) * 100;
        profile.ThermalResistance = (1.0 - (baseThermalRes * _stackingPenaltyService.CalculateStackingPenalty(thermalHardeners, StackingPenaltyGroup.ShieldResistance) * skillBonus)) * 100;
        profile.KineticResistance = (1.0 - (baseKineticRes * _stackingPenaltyService.CalculateStackingPenalty(kineticHardeners, StackingPenaltyGroup.ShieldResistance) * skillBonus)) * 100;
        profile.ExplosiveResistance = (1.0 - (baseExplosiveRes * _stackingPenaltyService.CalculateStackingPenalty(explosiveHardeners, StackingPenaltyGroup.ShieldResistance) * skillBonus)) * 100;

        // Ensure resistances don't exceed 100%
        profile.EmResistance = Math.Min(profile.EmResistance, 100.0);
        profile.ThermalResistance = Math.Min(profile.ThermalResistance, 100.0);
        profile.KineticResistance = Math.Min(profile.KineticResistance, 100.0);
        profile.ExplosiveResistance = Math.Min(profile.ExplosiveResistance, 100.0);

        return profile;
    }

    /// <summary>
    /// Calculate armor resistances with modules and skills
    /// </summary>
    private async Task<ResistanceProfile> CalculateArmorResistancesAsync(
        Ship ship,
        List<FittingModule> lowSlotModules,
        List<FittingModule> rigModules,
        Character? character,
        CancellationToken cancellationToken)
    {
        var profile = new ResistanceProfile();

        // Get base ship armor resistances (varies by ship type)
        var baseEmRes = GetShipAttribute(ship, "armorEmDamageResonance");
        var baseThermalRes = GetShipAttribute(ship, "armorThermalDamageResonance");
        var baseKineticRes = GetShipAttribute(ship, "armorKineticDamageResonance");
        var baseExplosiveRes = GetShipAttribute(ship, "armorExplosiveDamageResonance");

        // Apply armor hardener modules with stacking penalties
        var emHardeners = await GetResistanceModifiersAsync(lowSlotModules.Concat(rigModules), "armorEm", cancellationToken);
        var thermalHardeners = await GetResistanceModifiersAsync(lowSlotModules.Concat(rigModules), "armorThermal", cancellationToken);
        var kineticHardeners = await GetResistanceModifiersAsync(lowSlotModules.Concat(rigModules), "armorKinetic", cancellationToken);
        var explosiveHardeners = await GetResistanceModifiersAsync(lowSlotModules.Concat(rigModules), "armorExplosive", cancellationToken);

        // Apply skill bonuses using centralized skill calculation service
        var skillBonuses = await _skillCalculationService.CalculateComprehensiveSkillBonusesAsync(character, SkillCategory.Armor, cancellationToken);
        var skillBonus = 1.0; // Armor resistance skills don't typically provide base resistance bonuses

        // Calculate final resistances
        profile.EmResistance = (1.0 - (baseEmRes * _stackingPenaltyService.CalculateStackingPenalty(emHardeners, StackingPenaltyGroup.ArmorResistance) * skillBonus)) * 100;
        profile.ThermalResistance = (1.0 - (baseThermalRes * _stackingPenaltyService.CalculateStackingPenalty(thermalHardeners, StackingPenaltyGroup.ArmorResistance) * skillBonus)) * 100;
        profile.KineticResistance = (1.0 - (baseKineticRes * _stackingPenaltyService.CalculateStackingPenalty(kineticHardeners, StackingPenaltyGroup.ArmorResistance) * skillBonus)) * 100;
        profile.ExplosiveResistance = (1.0 - (baseExplosiveRes * _stackingPenaltyService.CalculateStackingPenalty(explosiveHardeners, StackingPenaltyGroup.ArmorResistance) * skillBonus)) * 100;

        // Ensure resistances don't exceed 100%
        profile.EmResistance = Math.Min(profile.EmResistance, 100.0);
        profile.ThermalResistance = Math.Min(profile.ThermalResistance, 100.0);
        profile.KineticResistance = Math.Min(profile.KineticResistance, 100.0);
        profile.ExplosiveResistance = Math.Min(profile.ExplosiveResistance, 100.0);

        return profile;
    }

    /// <summary>
    /// Calculate hull resistances with modules and skills
    /// </summary>
    private async Task<ResistanceProfile> CalculateHullResistancesAsync(
        Ship ship,
        List<FittingModule> lowSlotModules,
        List<FittingModule> rigModules,
        Character? character,
        CancellationToken cancellationToken)
    {
        var profile = new ResistanceProfile();

        // Hull has base 0% resistances (resonance = 1.0)
        const double baseResonance = 1.0;

        // Apply damage control modules (affects hull resistances)
        var damageControlModifiers = await GetDamageControlModifiersAsync(lowSlotModules, cancellationToken);
        var hullResistanceBonus = _stackingPenaltyService.CalculateStackingPenalty(damageControlModifiers, StackingPenaltyGroup.HullResistance);

        // Calculate final resistances (damage control provides hull resistance)
        profile.EmResistance = (1.0 - (baseResonance * hullResistanceBonus)) * 100;
        profile.ThermalResistance = (1.0 - (baseResonance * hullResistanceBonus)) * 100;
        profile.KineticResistance = (1.0 - (baseResonance * hullResistanceBonus)) * 100;
        profile.ExplosiveResistance = (1.0 - (baseResonance * hullResistanceBonus)) * 100;

        return profile;
    }

    /// <summary>
    /// Calculate effective hit points based on resistances
    /// </summary>
    private double CalculateEffectiveHitPoints(double hitPoints, ResistanceProfile resistances)
    {
        // Use average resistance for simplicity, though in reality it depends on damage type
        var averageResistance = (resistances.EmResistance + resistances.ThermalResistance + 
                                resistances.KineticResistance + resistances.ExplosiveResistance) / 4.0;
        
        return hitPoints / (1.0 - (averageResistance / 100.0));
    }

    /// <summary>
    /// Calculate passive shield regeneration rate
    /// </summary>
    private double CalculatePassiveShieldRegen(double shieldCapacity, double rechargeTime)
    {
        // EVE's passive shield regen formula: peak regen at 25% shield, formula based on capacitor recharge
        var tau = rechargeTime / 5000.0; // Convert to seconds
        var peakRegenRate = (2.5 * shieldCapacity) / tau; // Peak regen rate
        return peakRegenRate * PassiveShieldRegenPeakRate; // Average sustainable regen
    }

    /// <summary>
    /// Calculate shield repair rate (passive + active)
    /// </summary>
    private double CalculateShieldRepairRate(double passiveRegen, double activeBoost, TimeSpan cycleTime)
    {
        var activeRepairRate = cycleTime.TotalSeconds > 0 ? activeBoost / cycleTime.TotalSeconds : 0;
        return passiveRegen + activeRepairRate;
    }

    /// <summary>
    /// Calculate armor repair rate
    /// </summary>
    private double CalculateArmorRepairRate(double repairAmount, TimeSpan cycleTime)
    {
        return cycleTime.TotalSeconds > 0 ? repairAmount / cycleTime.TotalSeconds : 0;
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
            "shieldCapacity" => 5000,
            "shieldRechargeRate" => 600000, // 10 minutes in milliseconds
            "armorHP" => 3000,
            "hp" => 2000,
            "shieldEmDamageResonance" => 1.0,
            "shieldThermalDamageResonance" => 1.0,
            "shieldKineticDamageResonance" => 1.0,
            "shieldExplosiveDamageResonance" => 1.0,
            "armorEmDamageResonance" => 0.9,
            "armorThermalDamageResonance" => 0.9,
            "armorKineticDamageResonance" => 0.75,
            "armorExplosiveDamageResonance" => 0.8,
            _ => 0
        };
    }

    // Placeholder methods for module-specific calculations
    private async Task<IEnumerable<double>> GetShieldExtenderBonusesAsync(List<FittingModule> modules, CancellationToken cancellationToken) => new[] { 1.3, 1.3 };
    private async Task<IEnumerable<double>> GetArmorPlateBonusesAsync(List<FittingModule> modules, CancellationToken cancellationToken) => new[] { 1.6 };
    private async Task<IEnumerable<double>> GetHullUpgradeBonusesAsync(List<FittingModule> lowSlotModules, List<FittingModule> rigModules, CancellationToken cancellationToken) => new[] { 1.1 };
    private async Task<IEnumerable<double>> GetSubsystemHullBonusesAsync(List<FittingModule> subsystemModules, CancellationToken cancellationToken) => new[] { 1.0 };
    private async Task<IEnumerable<double>> GetShieldRegenModifiersAsync(List<FittingModule> midSlotModules, List<FittingModule> rigModules, CancellationToken cancellationToken) => new[] { 1.1 };
    private async Task<IEnumerable<double>> GetResistanceModifiersAsync(IEnumerable<FittingModule> modules, string resistanceType, CancellationToken cancellationToken) => new[] { 0.8 };
    private async Task<IEnumerable<double>> GetDamageControlModifiersAsync(List<FittingModule> modules, CancellationToken cancellationToken) => new[] { 0.6 };
    private async Task<List<FittingModule>> GetShieldBoosterModulesAsync(List<FittingModule> modules, CancellationToken cancellationToken) => new();
    private async Task<List<FittingModule>> GetArmorRepairerModulesAsync(List<FittingModule> modules, CancellationToken cancellationToken) => new();
    private async Task<(double amount, double cycleTime)> CalculateActiveShieldBoostAsync(List<FittingModule> boosters, Character? character, CancellationToken cancellationToken) => (500, ShieldBoosterCycleTime);
    private async Task<(double amount, double cycleTime)> CalculateActiveArmorRepairAsync(List<FittingModule> repairers, Character? character, CancellationToken cancellationToken) => (400, ArmorRepairCycleTime);
    private async Task<double> CalculateShieldSkillBonusesAsync(Character? character, CancellationToken cancellationToken) => 1.05;
    private async Task<double> CalculateArmorSkillBonusesAsync(Character? character, CancellationToken cancellationToken) => 1.05;
    private async Task<double> CalculateShieldResistanceSkillBonusesAsync(Character? character, CancellationToken cancellationToken) => 0.95;
    private async Task<double> CalculateArmorResistanceSkillBonusesAsync(Character? character, CancellationToken cancellationToken) => 0.95;

    #endregion
}

/// <summary>
/// Interface for tank calculation service
/// </summary>
public interface ITankCalculationService
{
    Task<TankCalculationResult> CalculateComprehensiveTankAsync(ShipFitting fitting, Character? character = null, CancellationToken cancellationToken = default);
}

// Use the existing TankCalculationResult from IShipFittingCalculationService.cs

/// <summary>
/// Tank layer result (shield, armor, or hull)
/// </summary>
public class TankLayerResult
{
    public double TotalHitPoints { get; set; }
    public ResistanceProfile Resistances { get; set; } = new();
    public double PassiveRegenRate { get; set; }
    public double ActiveRepairAmount { get; set; }
    public TimeSpan RepairCycleTime { get; set; }
}


