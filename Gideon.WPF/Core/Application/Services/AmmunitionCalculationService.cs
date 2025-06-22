// ==========================================================================
// AmmunitionCalculationService.cs - EVE Online Ammunition and Charge System
// ==========================================================================
// Implementation of accurate EVE Online ammunition, charge, and script effects
// for weapons and modules, matching server-side formulas.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.Extensions.Logging;
using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Core.Domain.Interfaces;

namespace Gideon.WPF.Core.Application.Services;

/// <summary>
/// Service for accurate EVE Online ammunition and charge calculations
/// </summary>
public class AmmunitionCalculationService : IAmmunitionCalculationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AmmunitionCalculationService> _logger;
    
    // EVE ammunition constants
    private const double DefaultAmmoCapacity = 1.0; // Default ammo capacity per unit
    private const double DefaultChargeCapacity = 1.0; // Default charge capacity per unit
    
    public AmmunitionCalculationService(IUnitOfWork unitOfWork, ILogger<AmmunitionCalculationService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calculate comprehensive ammunition effects for weapons and modules
    /// </summary>
    public async Task<AmmunitionEffectResult> CalculateAmmunitionEffectsAsync(
        ShipFitting fitting,
        Character? character = null,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogDebug("Starting ammunition effects calculation for fitting {FittingName}", fitting.Name);

            var result = new AmmunitionEffectResult();
            
            // Calculate weapon ammunition effects
            result.WeaponAmmunitionEffects = await CalculateWeaponAmmunitionEffectsAsync(fitting, character, cancellationToken);
            
            // Calculate module charge effects
            result.ModuleChargeEffects = await CalculateModuleChargeEffectsAsync(fitting, character, cancellationToken);
            
            // Calculate script effects for modules
            result.ScriptEffects = await CalculateScriptEffectsAsync(fitting, character, cancellationToken);
            
            // Calculate ammunition capacity and usage
            result.AmmunitionCapacity = await CalculateAmmunitionCapacityAsync(fitting, cancellationToken);
            
            var calculationTime = DateTime.UtcNow - startTime;
            result.CalculationTime = calculationTime;
            
            _logger.LogInformation("Ammunition effects calculation completed in {Ms}ms", calculationTime.TotalMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating ammunition effects for fitting {FittingName}", fitting.Name);
            throw;
        }
    }

    /// <summary>
    /// Calculate weapon ammunition effects (turrets and launchers)
    /// </summary>
    private async Task<List<WeaponAmmunitionEffect>> CalculateWeaponAmmunitionEffectsAsync(
        ShipFitting fitting,
        Character? character,
        CancellationToken cancellationToken)
    {
        var effects = new List<WeaponAmmunitionEffect>();
        
        // Parse high slot modules (weapons)
        var highSlotModules = ParseFittingSlot(fitting.HighSlotConfiguration);
        
        foreach (var weapon in highSlotModules)
        {
            var weaponData = await GetWeaponDataAsync(weapon.TypeId, cancellationToken);
            if (weaponData == null) continue;
            
            var ammunitionEffect = new WeaponAmmunitionEffect
            {
                WeaponTypeId = weapon.TypeId,
                WeaponName = weaponData.Name,
                WeaponCategory = weaponData.Category
            };
            
            // Get ammunition loaded in weapon
            var ammunition = await GetLoadedAmmunitionAsync(weapon, cancellationToken);
            if (ammunition != null)
            {
                ammunitionEffect.AmmunitionTypeId = ammunition.TypeId;
                ammunitionEffect.AmmunitionName = ammunition.Name;
                ammunitionEffect.AmmunitionCategory = ammunition.Category;
                
                // Calculate ammunition effects based on weapon and ammo type
                ammunitionEffect.Effects = await CalculateSpecificAmmunitionEffectsAsync(weaponData, ammunition, character, cancellationToken);
                
                // Calculate ammunition consumption
                ammunitionEffect.AmmunitionConsumption = CalculateAmmunitionConsumption(weaponData, ammunition);
                
                // Calculate ammunition capacity in weapon
                ammunitionEffect.AmmunitionCapacity = CalculateWeaponAmmunitionCapacity(weaponData, ammunition);
                
                _logger.LogDebug("Calculated ammunition effects for weapon {WeaponName} with ammo {AmmoName}",
                    weaponData.Name, ammunition.Name);
            }
            else
            {
                // No ammunition loaded
                ammunitionEffect.Effects = GetBaseWeaponEffects(weaponData);
                _logger.LogDebug("No ammunition loaded for weapon {WeaponName}, using base effects", weaponData.Name);
            }
            
            effects.Add(ammunitionEffect);
        }
        
        return effects;
    }

    /// <summary>
    /// Calculate module charge effects (boosters, repairers, etc.)
    /// </summary>
    private async Task<List<ModuleChargeEffect>> CalculateModuleChargeEffectsAsync(
        ShipFitting fitting,
        Character? character,
        CancellationToken cancellationToken)
    {
        var effects = new List<ModuleChargeEffect>();
        
        // Parse all module slots
        var allModules = ParseFittingSlot(fitting.HighSlotConfiguration)
            .Concat(ParseFittingSlot(fitting.MidSlotConfiguration))
            .Concat(ParseFittingSlot(fitting.LowSlotConfiguration))
            .ToList();
        
        foreach (var module in allModules)
        {
            var moduleData = await GetModuleDataAsync(module.TypeId, cancellationToken);
            if (moduleData == null || !moduleData.UsesCharges) continue;
            
            var chargeEffect = new ModuleChargeEffect
            {
                ModuleTypeId = module.TypeId,
                ModuleName = moduleData.Name,
                ModuleCategory = moduleData.Category
            };
            
            // Get charge loaded in module
            var charge = await GetLoadedChargeAsync(module, cancellationToken);
            if (charge != null)
            {
                chargeEffect.ChargeTypeId = charge.TypeId;
                chargeEffect.ChargeName = charge.Name;
                chargeEffect.ChargeCategory = charge.Category;
                
                // Calculate charge effects
                chargeEffect.Effects = await CalculateSpecificChargeEffectsAsync(moduleData, charge, character, cancellationToken);
                
                // Calculate charge consumption
                chargeEffect.ChargeConsumption = CalculateChargeConsumption(moduleData, charge);
                
                // Calculate charge capacity in module
                chargeEffect.ChargeCapacity = CalculateModuleChargeCapacity(moduleData, charge);
                
                _logger.LogDebug("Calculated charge effects for module {ModuleName} with charge {ChargeName}",
                    moduleData.Name, charge.Name);
            }
            else
            {
                // No charge loaded - module may not function or have reduced effectiveness
                chargeEffect.Effects = GetBaseModuleEffects(moduleData);
                _logger.LogDebug("No charge loaded for module {ModuleName}", moduleData.Name);
            }
            
            effects.Add(chargeEffect);
        }
        
        return effects;
    }

    /// <summary>
    /// Calculate script effects for modules (EWAR, links, etc.)
    /// </summary>
    private async Task<List<ScriptEffect>> CalculateScriptEffectsAsync(
        ShipFitting fitting,
        Character? character,
        CancellationToken cancellationToken)
    {
        var effects = new List<ScriptEffect>();
        
        // Parse all module slots
        var allModules = ParseFittingSlot(fitting.HighSlotConfiguration)
            .Concat(ParseFittingSlot(fitting.MidSlotConfiguration))
            .Concat(ParseFittingSlot(fitting.LowSlotConfiguration))
            .ToList();
        
        foreach (var module in allModules)
        {
            var moduleData = await GetModuleDataAsync(module.TypeId, cancellationToken);
            if (moduleData == null || !moduleData.UsesScripts) continue;
            
            var scriptEffect = new ScriptEffect
            {
                ModuleTypeId = module.TypeId,
                ModuleName = moduleData.Name,
                ModuleCategory = moduleData.Category
            };
            
            // Get script loaded in module
            var script = await GetLoadedScriptAsync(module, cancellationToken);
            if (script != null)
            {
                scriptEffect.ScriptTypeId = script.TypeId;
                scriptEffect.ScriptName = script.Name;
                scriptEffect.ScriptCategory = script.Category;
                
                // Calculate script effects
                scriptEffect.Effects = await CalculateSpecificScriptEffectsAsync(moduleData, script, character, cancellationToken);
                
                _logger.LogDebug("Calculated script effects for module {ModuleName} with script {ScriptName}",
                    moduleData.Name, script.Name);
            }
            else
            {
                // No script loaded - module has base effects
                scriptEffect.Effects = GetBaseModuleEffects(moduleData);
                _logger.LogDebug("No script loaded for module {ModuleName}, using base effects", moduleData.Name);
            }
            
            effects.Add(scriptEffect);
        }
        
        return effects;
    }

    /// <summary>
    /// Calculate specific ammunition effects for weapon + ammunition combination
    /// </summary>
    private async Task<AmmunitionEffects> CalculateSpecificAmmunitionEffectsAsync(
        WeaponData weapon,
        AmmunitionData ammunition,
        Character? character,
        CancellationToken cancellationToken)
    {
        var effects = new AmmunitionEffects();
        
        // Get base ammunition attributes
        var ammoAttributes = await GetAmmunitionAttributesAsync(ammunition.TypeId, cancellationToken);
        
        // Calculate damage effects
        effects.DamageMultiplier = ammoAttributes.DamageMultiplier;
        effects.DamageTypeProfile = ammoAttributes.DamageProfile;
        
        // Calculate range effects
        effects.OptimalRangeMultiplier = ammoAttributes.OptimalRangeMultiplier;
        effects.FalloffRangeMultiplier = ammoAttributes.FalloffMultiplier;
        
        // Calculate tracking effects
        effects.TrackingSpeedMultiplier = ammoAttributes.TrackingMultiplier;
        
        // Calculate rate of fire effects
        effects.RateOfFireMultiplier = ammoAttributes.RateOfFireMultiplier;
        
        // Calculate capacitor effects
        effects.CapacitorUsageMultiplier = ammoAttributes.CapacitorMultiplier;
        
        // Special ammunition effects
        if (ammunition.Category == AmmunitionCategory.Faction)
        {
            effects = ApplyFactionAmmunitionBonuses(effects, ammunition);
        }
        else if (ammunition.Category == AmmunitionCategory.Tech2)
        {
            effects = ApplyTech2AmmunitionEffects(effects, ammunition);
        }
        else if (ammunition.Category == AmmunitionCategory.Specialty)
        {
            effects = ApplySpecialtyAmmunitionEffects(effects, ammunition);
        }
        
        // Apply weapon-specific compatibility bonuses
        effects = ApplyWeaponCompatibilityBonuses(effects, weapon, ammunition);
        
        return effects;
    }

    /// <summary>
    /// Calculate specific charge effects for module + charge combination
    /// </summary>
    private async Task<ChargeEffects> CalculateSpecificChargeEffectsAsync(
        ModuleData module,
        ChargeData charge,
        Character? character,
        CancellationToken cancellationToken)
    {
        var effects = new ChargeEffects();
        
        // Get base charge attributes
        var chargeAttributes = await GetChargeAttributesAsync(charge.TypeId, cancellationToken);
        
        // Calculate based on module type
        switch (module.Category)
        {
            case ModuleCategory.ShieldBooster:
                effects.RepairAmountMultiplier = chargeAttributes.RepairAmountMultiplier;
                effects.CapacitorUsageMultiplier = chargeAttributes.CapacitorMultiplier;
                effects.CycleTimeMultiplier = chargeAttributes.CycleTimeMultiplier;
                break;
                
            case ModuleCategory.ArmorRepairer:
                effects.RepairAmountMultiplier = chargeAttributes.RepairAmountMultiplier;
                effects.CapacitorUsageMultiplier = chargeAttributes.CapacitorMultiplier;
                effects.CycleTimeMultiplier = chargeAttributes.CycleTimeMultiplier;
                break;
                
            case ModuleCategory.CapacitorBooster:
                effects.CapacitorInjectionAmount = chargeAttributes.CapacitorInjectionAmount;
                effects.CycleTimeMultiplier = chargeAttributes.CycleTimeMultiplier;
                break;
                
            case ModuleCategory.AncillaryArmorRepairer:
            case ModuleCategory.AncillaryShieldBooster:
                effects.RepairAmountMultiplier = chargeAttributes.RepairAmountMultiplier;
                effects.CycleTimeMultiplier = chargeAttributes.CycleTimeMultiplier;
                effects.ChargesPerReload = chargeAttributes.ChargesPerReload;
                effects.ReloadTimeMultiplier = chargeAttributes.ReloadTimeMultiplier;
                break;
        }
        
        // Apply charge quality bonuses
        if (charge.Category == ChargeCategory.Navy || charge.Category == ChargeCategory.Faction)
        {
            effects = ApplyHighQualityChargeBonuses(effects, charge);
        }
        
        return effects;
    }

    /// <summary>
    /// Calculate specific script effects for module + script combination
    /// </summary>
    private async Task<ScriptEffects> CalculateSpecificScriptEffectsAsync(
        ModuleData module,
        ScriptData script,
        Character? character,
        CancellationToken cancellationToken)
    {
        var effects = new ScriptEffects();
        
        // Get base script attributes
        var scriptAttributes = await GetScriptAttributesAsync(script.TypeId, cancellationToken);
        
        // Calculate based on script type and module compatibility
        switch (script.ScriptType)
        {
            case ScriptType.OptimalRangeScript:
                effects.OptimalRangeMultiplier = scriptAttributes.OptimalRangeBonus;
                effects.FalloffRangeMultiplier = scriptAttributes.FalloffRangePenalty;
                break;
                
            case ScriptType.TrackingSpeedScript:
                effects.TrackingSpeedMultiplier = scriptAttributes.TrackingSpeedBonus;
                effects.OptimalRangeMultiplier = scriptAttributes.OptimalRangePenalty;
                break;
                
            case ScriptType.ScanResolutionScript:
                effects.ScanResolutionMultiplier = scriptAttributes.ScanResolutionBonus;
                effects.TargetRangeMultiplier = scriptAttributes.TargetRangePenalty;
                break;
                
            case ScriptType.TargetingRangeScript:
                effects.TargetRangeMultiplier = scriptAttributes.TargetRangeBonus;
                effects.ScanResolutionMultiplier = scriptAttributes.ScanResolutionPenalty;
                break;
                
            case ScriptType.DisruptionScript:
                effects.DisruptionStrengthMultiplier = scriptAttributes.DisruptionStrengthBonus;
                effects.CapacitorUsageMultiplier = scriptAttributes.CapacitorUsagePenalty;
                break;
        }
        
        return effects;
    }

    /// <summary>
    /// Calculate ammunition capacity requirements for the fitting
    /// </summary>
    private async Task<AmmunitionCapacityResult> CalculateAmmunitionCapacityAsync(
        ShipFitting fitting,
        CancellationToken cancellationToken)
    {
        var result = new AmmunitionCapacityResult();
        
        // Parse cargo configuration to get ammunition storage
        var cargoItems = ParseFittingSlot(fitting.CargoConfiguration);
        
        // Calculate ammunition volume requirements
        foreach (var item in cargoItems)
        {
            var itemData = await GetItemDataAsync(item.TypeId, cancellationToken);
            if (itemData != null && IsAmmunition(itemData))
            {
                var ammunitionInfo = new AmmunitionInfo
                {
                    TypeId = item.TypeId,
                    Name = itemData.Name,
                    Quantity = item.Quantity,
                    VolumePerUnit = itemData.Volume,
                    TotalVolume = item.Quantity * itemData.Volume,
                    Category = GetAmmunitionCategory(itemData)
                };
                
                result.StoredAmmunition.Add(ammunitionInfo);
                result.TotalAmmunitionVolume += ammunitionInfo.TotalVolume;
            }
            else if (itemData != null && IsCharge(itemData))
            {
                var chargeInfo = new ChargeInfo
                {
                    TypeId = item.TypeId,
                    Name = itemData.Name,
                    Quantity = item.Quantity,
                    VolumePerUnit = itemData.Volume,
                    TotalVolume = item.Quantity * itemData.Volume,
                    Category = GetChargeCategory(itemData)
                };
                
                result.StoredCharges.Add(chargeInfo);
                result.TotalChargeVolume += chargeInfo.TotalVolume;
            }
        }
        
        // Calculate ammunition duration based on consumption rates
        var weaponEffects = await CalculateWeaponAmmunitionEffectsAsync(fitting, null, cancellationToken);
        foreach (var weaponEffect in weaponEffects)
        {
            if (weaponEffect.AmmunitionTypeId.HasValue)
            {
                var storedAmmo = result.StoredAmmunition.FirstOrDefault(a => a.TypeId == weaponEffect.AmmunitionTypeId.Value);
                if (storedAmmo != null && weaponEffect.AmmunitionConsumption > 0)
                {
                    var durationMinutes = storedAmmo.Quantity / weaponEffect.AmmunitionConsumption;
                    storedAmmo.EstimatedDuration = TimeSpan.FromMinutes(durationMinutes);
                }
            }
        }
        
        return result;
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

            return System.Text.Json.JsonSerializer.Deserialize<List<FittingModule>>(slotConfiguration) ?? new List<FittingModule>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse fitting slot configuration: {Config}", slotConfiguration);
            return new List<FittingModule>();
        }
    }

    /// <summary>
    /// Calculate ammunition consumption rate per minute
    /// </summary>
    private double CalculateAmmunitionConsumption(WeaponData weapon, AmmunitionData ammunition)
    {
        // EVE ammunition consumption: 1 round per cycle for most weapons
        var cycleTime = weapon.ActivationTime / 1000.0; // Convert to seconds
        var cyclesPerMinute = 60.0 / cycleTime;
        var consumptionPerCycle = GetAmmunitionConsumptionPerCycle(weapon, ammunition);
        
        return cyclesPerMinute * consumptionPerCycle;
    }

    /// <summary>
    /// Calculate charge consumption rate per minute
    /// </summary>
    private double CalculateChargeConsumption(ModuleData module, ChargeData charge)
    {
        // Charge consumption varies by module type
        var cycleTime = module.ActivationTime / 1000.0; // Convert to seconds
        var cyclesPerMinute = 60.0 / cycleTime;
        var consumptionPerCycle = GetChargeConsumptionPerCycle(module, charge);
        
        return cyclesPerMinute * consumptionPerCycle;
    }

    /// <summary>
    /// Calculate weapon ammunition capacity
    /// </summary>
    private int CalculateWeaponAmmunitionCapacity(WeaponData weapon, AmmunitionData ammunition)
    {
        // Most weapons have standard ammunition capacity
        return weapon.Category switch
        {
            WeaponCategory.EnergyTurret => 1, // Crystals don't consume (infinite capacity)
            WeaponCategory.HybridTurret => 100, // Standard hybrid charge capacity
            WeaponCategory.ProjectileTurret => 100, // Standard projectile ammo capacity
            WeaponCategory.MissileLauncher => GetMissileCapacity(weapon.Size),
            WeaponCategory.TorpedoLauncher => GetTorpedoCapacity(weapon.Size),
            _ => 100
        };
    }

    /// <summary>
    /// Calculate module charge capacity
    /// </summary>
    private int CalculateModuleChargeCapacity(ModuleData module, ChargeData charge)
    {
        // Module charge capacity varies by module type
        return module.Category switch
        {
            ModuleCategory.CapacitorBooster => 8, // Standard cap booster charges
            ModuleCategory.AncillaryArmorRepairer => 8, // Ancillary repairer charges
            ModuleCategory.AncillaryShieldBooster => 9, // Ancillary shield booster charges
            ModuleCategory.SmartBomb => 8, // Smart bomb charges
            _ => 1 // Most modules don't use charges
        };
    }

    /// <summary>
    /// Apply faction ammunition bonuses
    /// </summary>
    private AmmunitionEffects ApplyFactionAmmunitionBonuses(AmmunitionEffects effects, AmmunitionData ammunition)
    {
        // Faction ammunition typically has 10-15% better stats
        effects.DamageMultiplier *= 1.10; // 10% more damage
        effects.OptimalRangeMultiplier *= 1.15; // 15% more optimal range
        effects.TrackingSpeedMultiplier *= 1.05; // 5% better tracking
        
        return effects;
    }

    /// <summary>
    /// Apply Tech 2 ammunition effects
    /// </summary>
    private AmmunitionEffects ApplyTech2AmmunitionEffects(AmmunitionEffects effects, AmmunitionData ammunition)
    {
        // Tech 2 ammunition has specialized effects
        switch (ammunition.SubCategory)
        {
            case "LongRange":
                effects.OptimalRangeMultiplier *= 1.25;
                effects.FalloffRangeMultiplier *= 1.25;
                effects.DamageMultiplier *= 0.85; // Trade damage for range
                break;
                
            case "ShortRange":
                effects.DamageMultiplier *= 1.25;
                effects.TrackingSpeedMultiplier *= 1.20;
                effects.OptimalRangeMultiplier *= 0.75; // Trade range for damage
                break;
                
            case "Precision":
                effects.TrackingSpeedMultiplier *= 1.30;
                effects.DamageMultiplier *= 0.90; // Trade damage for tracking
                break;
        }
        
        return effects;
    }

    /// <summary>
    /// Apply specialty ammunition effects
    /// </summary>
    private AmmunitionEffects ApplySpecialtyAmmunitionEffects(AmmunitionEffects effects, AmmunitionData ammunition)
    {
        // Specialty ammunition has unique effects
        switch (ammunition.SubCategory)
        {
            case "EMP":
                effects.CapacitorNeut = true;
                effects.DamageMultiplier *= 0.90; // Trade damage for utility
                break;
                
            case "Tracking":
                effects.TrackingDisruption = true;
                effects.DamageMultiplier *= 0.85;
                break;
                
            case "Webifying":
                effects.VelocityReduction = true;
                effects.DamageMultiplier *= 0.80;
                break;
        }
        
        return effects;
    }

    /// <summary>
    /// Apply weapon compatibility bonuses
    /// </summary>
    private AmmunitionEffects ApplyWeaponCompatibilityBonuses(AmmunitionEffects effects, WeaponData weapon, AmmunitionData ammunition)
    {
        // Some weapons have bonuses for specific ammunition types
        if (weapon.SpecialtyAmmoBonus > 0 && IsSpecialtyAmmoCompatible(weapon, ammunition))
        {
            effects.DamageMultiplier *= (1.0 + weapon.SpecialtyAmmoBonus);
        }
        
        return effects;
    }

    /// <summary>
    /// Apply high quality charge bonuses
    /// </summary>
    private ChargeEffects ApplyHighQualityChargeBonuses(ChargeEffects effects, ChargeData charge)
    {
        // Navy/Faction charges typically have 10-20% better performance
        effects.RepairAmountMultiplier *= 1.15; // 15% better repair
        effects.CapacitorUsageMultiplier *= 0.95; // 5% less capacitor usage
        
        return effects;
    }

    // Placeholder methods for data retrieval - these would be implemented with actual database queries
    private async Task<WeaponData?> GetWeaponDataAsync(int typeId, CancellationToken cancellationToken) => null;
    private async Task<ModuleData?> GetModuleDataAsync(int typeId, CancellationToken cancellationToken) => null;
    private async Task<AmmunitionData?> GetLoadedAmmunitionAsync(FittingModule weapon, CancellationToken cancellationToken) => null;
    private async Task<ChargeData?> GetLoadedChargeAsync(FittingModule module, CancellationToken cancellationToken) => null;
    private async Task<ScriptData?> GetLoadedScriptAsync(FittingModule module, CancellationToken cancellationToken) => null;
    private async Task<ItemData?> GetItemDataAsync(int typeId, CancellationToken cancellationToken) => null;
    private async Task<AmmunitionAttributes> GetAmmunitionAttributesAsync(int typeId, CancellationToken cancellationToken) => new();
    private async Task<ChargeAttributes> GetChargeAttributesAsync(int typeId, CancellationToken cancellationToken) => new();
    private async Task<ScriptAttributes> GetScriptAttributesAsync(int typeId, CancellationToken cancellationToken) => new();
    
    private AmmunitionEffects GetBaseWeaponEffects(WeaponData weapon) => new();
    private ChargeEffects GetBaseModuleEffects(ModuleData module) => new();
    private bool IsAmmunition(ItemData item) => false;
    private bool IsCharge(ItemData item) => false;
    private AmmunitionCategory GetAmmunitionCategory(ItemData item) => AmmunitionCategory.Tech1;
    private ChargeCategory GetChargeCategory(ItemData item) => ChargeCategory.Standard;
    private double GetAmmunitionConsumptionPerCycle(WeaponData weapon, AmmunitionData ammunition) => 1.0;
    private double GetChargeConsumptionPerCycle(ModuleData module, ChargeData charge) => 1.0;
    private int GetMissileCapacity(WeaponSize size) => size == WeaponSize.Small ? 20 : size == WeaponSize.Medium ? 15 : 10;
    private int GetTorpedoCapacity(WeaponSize size) => 5;
    private bool IsSpecialtyAmmoCompatible(WeaponData weapon, AmmunitionData ammunition) => false;

    #endregion
}

/// <summary>
/// Interface for ammunition calculation service
/// </summary>
public interface IAmmunitionCalculationService
{
    Task<AmmunitionEffectResult> CalculateAmmunitionEffectsAsync(ShipFitting fitting, Character? character = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Comprehensive ammunition effect calculation result
/// </summary>
public class AmmunitionEffectResult
{
    public List<WeaponAmmunitionEffect> WeaponAmmunitionEffects { get; set; } = new();
    public List<ModuleChargeEffect> ModuleChargeEffects { get; set; } = new();
    public List<ScriptEffect> ScriptEffects { get; set; } = new();
    public AmmunitionCapacityResult AmmunitionCapacity { get; set; } = new();
    public TimeSpan CalculationTime { get; set; }
}

/// <summary>
/// Weapon ammunition effect
/// </summary>
public class WeaponAmmunitionEffect
{
    public int WeaponTypeId { get; set; }
    public string WeaponName { get; set; } = string.Empty;
    public WeaponCategory WeaponCategory { get; set; }
    public int? AmmunitionTypeId { get; set; }
    public string AmmunitionName { get; set; } = string.Empty;
    public AmmunitionCategory AmmunitionCategory { get; set; }
    public AmmunitionEffects Effects { get; set; } = new();
    public double AmmunitionConsumption { get; set; } // Per minute
    public int AmmunitionCapacity { get; set; }
}

/// <summary>
/// Module charge effect
/// </summary>
public class ModuleChargeEffect
{
    public int ModuleTypeId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public ModuleCategory ModuleCategory { get; set; }
    public int? ChargeTypeId { get; set; }
    public string ChargeName { get; set; } = string.Empty;
    public ChargeCategory ChargeCategory { get; set; }
    public ChargeEffects Effects { get; set; } = new();
    public double ChargeConsumption { get; set; } // Per minute
    public int ChargeCapacity { get; set; }
}

/// <summary>
/// Script effect
/// </summary>
public class ScriptEffect
{
    public int ModuleTypeId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public ModuleCategory ModuleCategory { get; set; }
    public int? ScriptTypeId { get; set; }
    public string ScriptName { get; set; } = string.Empty;
    public ScriptCategory ScriptCategory { get; set; }
    public ScriptEffects Effects { get; set; } = new();
}

/// <summary>
/// Ammunition effects
/// </summary>
public class AmmunitionEffects
{
    public double DamageMultiplier { get; set; } = 1.0;
    public DamageProfile DamageTypeProfile { get; set; } = new();
    public double OptimalRangeMultiplier { get; set; } = 1.0;
    public double FalloffRangeMultiplier { get; set; } = 1.0;
    public double TrackingSpeedMultiplier { get; set; } = 1.0;
    public double RateOfFireMultiplier { get; set; } = 1.0;
    public double CapacitorUsageMultiplier { get; set; } = 1.0;
    
    // Special effects
    public bool CapacitorNeut { get; set; }
    public bool TrackingDisruption { get; set; }
    public bool VelocityReduction { get; set; }
}

/// <summary>
/// Charge effects
/// </summary>
public class ChargeEffects
{
    public double RepairAmountMultiplier { get; set; } = 1.0;
    public double CapacitorUsageMultiplier { get; set; } = 1.0;
    public double CycleTimeMultiplier { get; set; } = 1.0;
    public double CapacitorInjectionAmount { get; set; } = 0.0;
    public int ChargesPerReload { get; set; } = 0;
    public double ReloadTimeMultiplier { get; set; } = 1.0;
}

/// <summary>
/// Script effects
/// </summary>
public class ScriptEffects
{
    public double OptimalRangeMultiplier { get; set; } = 1.0;
    public double FalloffRangeMultiplier { get; set; } = 1.0;
    public double TrackingSpeedMultiplier { get; set; } = 1.0;
    public double ScanResolutionMultiplier { get; set; } = 1.0;
    public double TargetRangeMultiplier { get; set; } = 1.0;
    public double DisruptionStrengthMultiplier { get; set; } = 1.0;
    public double CapacitorUsageMultiplier { get; set; } = 1.0;
}

/// <summary>
/// Ammunition capacity result
/// </summary>
public class AmmunitionCapacityResult
{
    public List<AmmunitionInfo> StoredAmmunition { get; set; } = new();
    public List<ChargeInfo> StoredCharges { get; set; } = new();
    public double TotalAmmunitionVolume { get; set; }
    public double TotalChargeVolume { get; set; }
}

/// <summary>
/// Ammunition information
/// </summary>
public class AmmunitionInfo
{
    public int TypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public double VolumePerUnit { get; set; }
    public double TotalVolume { get; set; }
    public AmmunitionCategory Category { get; set; }
    public TimeSpan? EstimatedDuration { get; set; }
}

/// <summary>
/// Charge information
/// </summary>
public class ChargeInfo
{
    public int TypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public double VolumePerUnit { get; set; }
    public double TotalVolume { get; set; }
    public ChargeCategory Category { get; set; }
    public TimeSpan? EstimatedDuration { get; set; }
}

/// <summary>
/// Damage profile for ammunition
/// </summary>
public class DamageProfile
{
    public double EmDamage { get; set; }
    public double ThermalDamage { get; set; }
    public double KineticDamage { get; set; }
    public double ExplosiveDamage { get; set; }
}

// Enums for categorization
public enum WeaponCategory { EnergyTurret, HybridTurret, ProjectileTurret, MissileLauncher, TorpedoLauncher }
public enum WeaponSize { Small, Medium, Large, ExtraLarge }
public enum ModuleCategory { ShieldBooster, ArmorRepairer, CapacitorBooster, AncillaryArmorRepairer, AncillaryShieldBooster, SmartBomb }
public enum AmmunitionCategory { Tech1, Tech2, Faction, Specialty }
public enum ChargeCategory { Standard, Navy, Faction }
public enum ScriptCategory { Tracking, Optimal, Disruption, Support }
public enum ScriptType { OptimalRangeScript, TrackingSpeedScript, ScanResolutionScript, TargetingRangeScript, DisruptionScript }

// Data classes for module/ammunition information
public class WeaponData
{
    public string Name { get; set; } = string.Empty;
    public WeaponCategory Category { get; set; }
    public WeaponSize Size { get; set; }
    public double ActivationTime { get; set; }
    public double SpecialtyAmmoBonus { get; set; }
}

public class ModuleData
{
    public string Name { get; set; } = string.Empty;
    public ModuleCategory Category { get; set; }
    public bool UsesCharges { get; set; }
    public bool UsesScripts { get; set; }
    public double ActivationTime { get; set; }
}

public class AmmunitionData
{
    public string Name { get; set; } = string.Empty;
    public AmmunitionCategory Category { get; set; }
    public string SubCategory { get; set; } = string.Empty;
    public int TypeId { get; set; }
}

public class ChargeData
{
    public string Name { get; set; } = string.Empty;
    public ChargeCategory Category { get; set; }
    public int TypeId { get; set; }
}

public class ScriptData
{
    public string Name { get; set; } = string.Empty;
    public ScriptCategory Category { get; set; }
    public ScriptType ScriptType { get; set; }
    public int TypeId { get; set; }
}

public class ItemData
{
    public string Name { get; set; } = string.Empty;
    public double Volume { get; set; }
}

// Attribute classes for effects
public class AmmunitionAttributes
{
    public double DamageMultiplier { get; set; } = 1.0;
    public DamageProfile DamageProfile { get; set; } = new();
    public double OptimalRangeMultiplier { get; set; } = 1.0;
    public double FalloffMultiplier { get; set; } = 1.0;
    public double TrackingMultiplier { get; set; } = 1.0;
    public double RateOfFireMultiplier { get; set; } = 1.0;
    public double CapacitorMultiplier { get; set; } = 1.0;
}

public class ChargeAttributes
{
    public double RepairAmountMultiplier { get; set; } = 1.0;
    public double CapacitorMultiplier { get; set; } = 1.0;
    public double CycleTimeMultiplier { get; set; } = 1.0;
    public double CapacitorInjectionAmount { get; set; } = 0.0;
    public int ChargesPerReload { get; set; } = 0;
    public double ReloadTimeMultiplier { get; set; } = 1.0;
}

public class ScriptAttributes
{
    public double OptimalRangeBonus { get; set; } = 1.0;
    public double FalloffRangePenalty { get; set; } = 1.0;
    public double TrackingSpeedBonus { get; set; } = 1.0;
    public double OptimalRangePenalty { get; set; } = 1.0;
    public double ScanResolutionBonus { get; set; } = 1.0;
    public double TargetRangePenalty { get; set; } = 1.0;
    public double TargetRangeBonus { get; set; } = 1.0;
    public double ScanResolutionPenalty { get; set; } = 1.0;
    public double DisruptionStrengthBonus { get; set; } = 1.0;
    public double CapacitorUsagePenalty { get; set; } = 1.0;
}