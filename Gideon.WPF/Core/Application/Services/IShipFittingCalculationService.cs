// ==========================================================================
// IShipFittingCalculationService.cs - Ship Fitting Calculation Service Interface
// ==========================================================================
// Service interface for accurate ship fitting calculations matching EVE Online.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Gideon.WPF.Core.Domain.Entities;

namespace Gideon.WPF.Core.Application.Services;

/// <summary>
/// Service interface for ship fitting calculations with EVE Online accuracy
/// </summary>
public interface IShipFittingCalculationService
{
    /// <summary>
    /// Calculate comprehensive ship fitting statistics
    /// </summary>
    Task<ShipFittingCalculationResult> CalculateFittingAsync(ShipFitting fitting, Character? character = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate DPS (Damage Per Second) for the fitting
    /// </summary>
    Task<DpsCalculationResult> CalculateDpsAsync(ShipFitting fitting, Character? character = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate tank statistics (shield, armor, hull)
    /// </summary>
    Task<TankCalculationResult> CalculateTankAsync(ShipFitting fitting, Character? character = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate capacitor stability and duration
    /// </summary>
    Task<CapacitorCalculationResult> CalculateCapacitorAsync(ShipFitting fitting, Character? character = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate speed, agility, and navigation stats
    /// </summary>
    Task<NavigationCalculationResult> CalculateNavigationAsync(ShipFitting fitting, Character? character = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate targeting and sensor stats
    /// </summary>
    Task<TargetingCalculationResult> CalculateTargetingAsync(ShipFitting fitting, Character? character = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate fitting constraints (CPU, PowerGrid, Calibration)
    /// </summary>
    Task<FittingValidationResult> ValidateFittingConstraintsAsync(ShipFitting fitting, Character? character = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate resource usage for the fitting
    /// </summary>
    Task<ResourceUsageResult> CalculateResourceUsageAsync(ShipFitting fitting, Character? character = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Apply skill bonuses to calculations
    /// </summary>
    Task<double> ApplySkillBonusAsync(double baseValue, string skillName, int skillLevel, string bonusType, double bonusAmount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Apply stacking penalties for multiple modules
    /// </summary>
    double ApplyStackingPenalty(IEnumerable<double> bonuses);

    /// <summary>
    /// Calculate module activation cost and effects
    /// </summary>
    Task<ModuleEffectResult> CalculateModuleEffectsAsync(FittingModule module, Character? character = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Comprehensive ship fitting calculation result
/// </summary>
public class ShipFittingCalculationResult
{
    public DpsCalculationResult Dps { get; set; } = new();
    public TankCalculationResult Tank { get; set; } = new();
    public CapacitorCalculationResult Capacitor { get; set; } = new();
    public NavigationCalculationResult Navigation { get; set; } = new();
    public TargetingCalculationResult Targeting { get; set; } = new();
    public ResourceUsageResult ResourceUsage { get; set; } = new();
    public FittingValidationResult Validation { get; set; } = new();
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan CalculationTime { get; set; }
}

/// <summary>
/// DPS calculation result with detailed breakdown
/// </summary>
public class DpsCalculationResult
{
    public double TotalDps { get; set; }
    public double VolleyDamage { get; set; }
    public double EmDps { get; set; }
    public double ThermalDps { get; set; }
    public double KineticDps { get; set; }
    public double ExplosiveDps { get; set; }
    public double OptimalRange { get; set; }
    public double FalloffRange { get; set; }
    public double TrackingSpeed { get; set; }
    public double ProjectileVelocity { get; set; }
    public List<WeaponGroupDps> WeaponGroups { get; set; } = new();
}

/// <summary>
/// Tank calculation result with all defensive stats
/// </summary>
public class TankCalculationResult
{
    public double ShieldHp { get; set; }
    public double ArmorHp { get; set; }
    public double HullHp { get; set; }
    public double TotalEhp { get; set; }
    public double ShieldRecharge { get; set; }
    public double ArmorRepair { get; set; }
    public double HullRepair { get; set; }
    public ResistanceProfile ShieldResistances { get; set; } = new();
    public ResistanceProfile ArmorResistances { get; set; } = new();
    public ResistanceProfile HullResistances { get; set; } = new();
    public double PassiveShieldRegenRate { get; set; }
    public double ShieldBoostAmount { get; set; }
    public double ArmorRepairAmount { get; set; }
}

/// <summary>
/// Capacitor calculation result
/// </summary>
public class CapacitorCalculationResult
{
    public double CapacitorCapacity { get; set; }
    public double CapacitorRecharge { get; set; }
    public double CapacitorStability { get; set; }
    public TimeSpan CapacitorDuration { get; set; }
    public double CapacitorUsage { get; set; }
    public double CapacitorDelta { get; set; }
    public bool IsStable { get; set; }
    public List<ModuleCapUsage> ModuleUsage { get; set; } = new();
}

/// <summary>
/// Navigation calculation result
/// </summary>
public class NavigationCalculationResult
{
    public double MaxVelocity { get; set; }
    public double Acceleration { get; set; }
    public double AlignTime { get; set; }
    public double WarpSpeed { get; set; }
    public double Agility { get; set; }
    public double Mass { get; set; }
    public double Inertia { get; set; }
    public double SignatureRadius { get; set; }
}

/// <summary>
/// Targeting calculation result
/// </summary>
public class TargetingCalculationResult
{
    public double MaxTargetingRange { get; set; }
    public double ScanResolution { get; set; }
    public int MaxLockedTargets { get; set; }
    public double TargetLockTime { get; set; }
    public double SensorStrength { get; set; }
    public string SensorType { get; set; } = string.Empty;
}

/// <summary>
/// Resource usage calculation result
/// </summary>
public class ResourceUsageResult
{
    public double UsedCpu { get; set; }
    public double TotalCpu { get; set; }
    public double CpuUtilization { get; set; }
    public double UsedPowerGrid { get; set; }
    public double TotalPowerGrid { get; set; }
    public double PowerGridUtilization { get; set; }
    public double UsedCalibration { get; set; }
    public double TotalCalibration { get; set; }
    public double CalibrationUtilization { get; set; }
    public double UsedCargoSpace { get; set; }
    public double TotalCargoSpace { get; set; }
    public double UsedDroneBay { get; set; }
    public double TotalDroneBay { get; set; }
    public double UsedDroneBandwidth { get; set; }
    public double TotalDroneBandwidth { get; set; }
}

/// <summary>
/// Fitting validation result
/// </summary>
public class FittingValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<ConstraintViolation> Violations { get; set; } = new();
}

/// <summary>
/// Module effect calculation result
/// </summary>
public class ModuleEffectResult
{
    public double ActivationCost { get; set; }
    public TimeSpan ActivationTime { get; set; }
    public double EffectStrength { get; set; }
    public double Range { get; set; }
    public Dictionary<string, double> AttributeModifiers { get; set; } = new();
}

/// <summary>
/// Weapon group DPS breakdown
/// </summary>
public class WeaponGroupDps
{
    public string WeaponType { get; set; } = string.Empty;
    public int WeaponCount { get; set; }
    public double GroupDps { get; set; }
    public double VolleyDamage { get; set; }
    public double RateOfFire { get; set; }
    public double OptimalRange { get; set; }
    public double FalloffRange { get; set; }
}

/// <summary>
/// Resistance profile for shield/armor/hull
/// </summary>
public class ResistanceProfile
{
    public double EmResistance { get; set; }
    public double ThermalResistance { get; set; }
    public double KineticResistance { get; set; }
    public double ExplosiveResistance { get; set; }
    
    public double AverageResistance => (EmResistance + ThermalResistance + KineticResistance + ExplosiveResistance) / 4.0;
}

/// <summary>
/// Module capacitor usage information
/// </summary>
public class ModuleCapUsage
{
    public string ModuleName { get; set; } = string.Empty;
    public double CapacitorUsage { get; set; }
    public TimeSpan CycleTime { get; set; }
    public bool IsAlwaysOn { get; set; }
}

/// <summary>
/// Constraint violation information
/// </summary>
public class ConstraintViolation
{
    public string ConstraintType { get; set; } = string.Empty;
    public double Required { get; set; }
    public double Available { get; set; }
    public double Violation { get; set; }
    public string Message { get; set; } = string.Empty;
}