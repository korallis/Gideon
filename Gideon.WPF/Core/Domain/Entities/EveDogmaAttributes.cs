// ==========================================================================
// EveDogmaAttributes.cs - EVE Online Dogma Attribute System Entities
// ==========================================================================
// Domain entities for EVE Online's Dogma attribute system used for accurate
// ship fitting calculations including DPS, tank, capacitor, and navigation.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gideon.WPF.Core.Domain.Entities;

/// <summary>
/// EVE Dogma attribute definition
/// </summary>
[Table("EveDogmaAttributes")]
public class EveDogmaAttribute
{
    [Key]
    public int AttributeId { get; set; }

    [Required]
    [StringLength(100)]
    public string AttributeName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? Unit { get; set; }

    [StringLength(100)]
    public string? DisplayName { get; set; }

    public double? DefaultValue { get; set; }

    public bool Published { get; set; }

    public bool HighIsGood { get; set; }

    public bool Stackable { get; set; }

    public int CategoryId { get; set; }

    [StringLength(50)]
    public string? IconId { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual ICollection<EveTypeAttribute> TypeAttributes { get; set; } = new List<EveTypeAttribute>();
    public virtual ICollection<EveDogmaEffect> Effects { get; set; } = new List<EveDogmaEffect>();
}

/// <summary>
/// Links EVE types to their attribute values
/// </summary>
[Table("EveTypeAttributes")]
public class EveTypeAttribute
{
    [Key]
    public int Id { get; set; }

    public int TypeId { get; set; }

    public int AttributeId { get; set; }

    public double? ValueInt { get; set; }

    public double? ValueFloat { get; set; }

    // Navigation Properties
    [ForeignKey("TypeId")]
    public virtual EveType Type { get; set; } = null!;

    [ForeignKey("AttributeId")]
    public virtual EveDogmaAttribute Attribute { get; set; } = null!;

    /// <summary>
    /// Get the actual value, preferring float over int
    /// </summary>
    public double Value => ValueFloat ?? ValueInt ?? 0.0;
}

/// <summary>
/// EVE Dogma effect definition for module effects
/// </summary>
[Table("EveDogmaEffects")]
public class EveDogmaEffect
{
    [Key]
    public int EffectId { get; set; }

    [Required]
    [StringLength(100)]
    public string EffectName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public int EffectCategory { get; set; }

    public bool IsOffensive { get; set; }

    public bool IsAssistance { get; set; }

    public bool IsWarpSafe { get; set; }

    public bool RangeChance { get; set; }

    public bool ElectronicChance { get; set; }

    public bool PropulsionChance { get; set; }

    public int? DurationAttributeId { get; set; }

    public int? TrackingSpeedAttributeId { get; set; }

    public int? DischargeAttributeId { get; set; }

    public int? RangeAttributeId { get; set; }

    public int? FalloffAttributeId { get; set; }

    public bool NpcUsageChanceAttributeId { get; set; }

    public bool NpcActivationChanceAttributeId { get; set; }

    public bool FittingUsageChanceAttributeId { get; set; }

    public bool Published { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("DurationAttributeId")]
    public virtual EveDogmaAttribute? DurationAttribute { get; set; }

    [ForeignKey("TrackingSpeedAttributeId")]
    public virtual EveDogmaAttribute? TrackingSpeedAttribute { get; set; }

    [ForeignKey("DischargeAttributeId")]
    public virtual EveDogmaAttribute? DischargeAttribute { get; set; }

    [ForeignKey("RangeAttributeId")]
    public virtual EveDogmaAttribute? RangeAttribute { get; set; }

    [ForeignKey("FalloffAttributeId")]
    public virtual EveDogmaAttribute? FalloffAttribute { get; set; }

    public virtual ICollection<EveTypeEffect> TypeEffects { get; set; } = new List<EveTypeEffect>();
}

/// <summary>
/// Links EVE types to their effects
/// </summary>
[Table("EveTypeEffects")]
public class EveTypeEffect
{
    [Key]
    public int Id { get; set; }

    public int TypeId { get; set; }

    public int EffectId { get; set; }

    public bool IsDefault { get; set; }

    // Navigation Properties
    [ForeignKey("TypeId")]
    public virtual EveType Type { get; set; } = null!;

    [ForeignKey("EffectId")]
    public virtual EveDogmaEffect Effect { get; set; } = null!;
}

/// <summary>
/// EVE weapon type damage profile
/// </summary>
[Table("EveWeaponTypes")]
public class EveWeaponType
{
    [Key]
    public int TypeId { get; set; }

    [Required]
    [StringLength(100)]
    public string WeaponClass { get; set; } = string.Empty; // "Energy", "Projectile", "Missile", "Hybrid"

    [StringLength(100)]
    public string WeaponSize { get; set; } = string.Empty; // "Small", "Medium", "Large", "XL"

    public double EmDamage { get; set; }

    public double ThermalDamage { get; set; }

    public double KineticDamage { get; set; }

    public double ExplosiveDamage { get; set; }

    public double RateOfFire { get; set; } // Seconds between shots

    public double OptimalRange { get; set; } // Meters

    public double AccuracyFalloff { get; set; } // Meters

    public double TrackingSpeed { get; set; } // Rad/sec

    public double ProjectileVelocity { get; set; } // m/s for missiles/projectiles

    public double CapacitorNeed { get; set; } // GJ per activation

    public double ActivationTime { get; set; } // Milliseconds

    public bool RequiresAmmo { get; set; }

    public int? AmmoTypeId { get; set; }

    // Navigation Properties
    [ForeignKey("TypeId")]
    public virtual EveType Type { get; set; } = null!;

    [ForeignKey("AmmoTypeId")]
    public virtual EveType? AmmoType { get; set; }

    /// <summary>
    /// Calculate total damage per shot
    /// </summary>
    public double TotalDamage => EmDamage + ThermalDamage + KineticDamage + ExplosiveDamage;

    /// <summary>
    /// Calculate damage per second
    /// </summary>
    public double BaseDps => RateOfFire > 0 ? TotalDamage / RateOfFire : 0;

    /// <summary>
    /// Get damage type percentages
    /// </summary>
    public (double Em, double Thermal, double Kinetic, double Explosive) GetDamagePercentages()
    {
        var total = TotalDamage;
        if (total == 0) return (0, 0, 0, 0);

        return (
            EmDamage / total,
            ThermalDamage / total,
            KineticDamage / total,
            ExplosiveDamage / total
        );
    }
}

/// <summary>
/// EVE ammunition types and their damage modifications
/// </summary>
[Table("EveAmmunitionTypes")]
public class EveAmmunitionType
{
    [Key]
    public int TypeId { get; set; }

    [Required]
    [StringLength(100)]
    public string AmmoClass { get; set; } = string.Empty; // "Short Range", "Long Range", "Tech2"

    public double DamageMultiplier { get; set; } = 1.0;

    public double EmDamageMultiplier { get; set; } = 1.0;

    public double ThermalDamageMultiplier { get; set; } = 1.0;

    public double KineticDamageMultiplier { get; set; } = 1.0;

    public double ExplosiveDamageMultiplier { get; set; } = 1.0;

    public double RangeMultiplier { get; set; } = 1.0;

    public double TrackingMultiplier { get; set; } = 1.0;

    public double VelocityMultiplier { get; set; } = 1.0;

    public double CapacitorNeedMultiplier { get; set; } = 1.0;

    public double RateOfFireMultiplier { get; set; } = 1.0;

    public int Volume { get; set; } // m³ per unit

    // Navigation Properties
    [ForeignKey("TypeId")]
    public virtual EveType Type { get; set; } = null!;
}

/// <summary>
/// EVE attribute constants for ship fitting calculations
/// </summary>
public static class EveAttributeIds
{
    // Weapon Attributes
    public const int EmDamage = 114;
    public const int ThermalDamage = 118;
    public const int KineticDamage = 117;
    public const int ExplosiveDamage = 116;
    public const int DamageMultiplier = 64;
    public const int RateOfFire = 51;
    public const int OptimalRange = 54;
    public const int AccuracyFalloff = 158;
    public const int TrackingSpeed = 160;
    public const int ProjectileVelocity = 69;

    // Capacitor Attributes
    public const int CapacitorNeed = 6;
    public const int ActivationTime = 73;
    public const int CapacitorCapacity = 482;
    public const int RechargeRate = 55;

    // Tank Attributes
    public const int ShieldCapacity = 263;
    public const int ArmorHP = 265;
    public const int StructureHP = 9;
    public const int ShieldEmResistance = 271;
    public const int ShieldThermalResistance = 274;
    public const int ShieldKineticResistance = 273;
    public const int ShieldExplosiveResistance = 272;
    public const int ArmorEmResistance = 267;
    public const int ArmorThermalResistance = 270;
    public const int ArmorKineticResistance = 269;
    public const int ArmorExplosiveResistance = 268;

    // Navigation Attributes
    public const int MaxVelocity = 37;
    public const int Mass = 4;
    public const int Agility = 70;
    public const int SignatureRadius = 552;
    public const int WarpSpeed = 600;

    // Targeting Attributes
    public const int ScanResolution = 564;
    public const int MaxTargetRange = 76;
    public const int MaxLockedTargets = 192;
    public const int RadarSensorStrength = 208;
    public const int LadarSensorStrength = 209;
    public const int MagnetometricSensorStrength = 210;
    public const int GravimetricSensorStrength = 211;

    // Resource Attributes
    public const int CpuOutput = 48;
    public const int PowerOutput = 11;
    public const int CpuNeed = 50;
    public const int PowerNeed = 30;
    public const int CalibrationNeed = 1153;
    public const int RigSlots = 1137;
    public const int CargoCapacity = 38;
    public const int DroneBay = 283;
    public const int DroneBandwidth = 1271;

    // Skill Bonuses
    public const int SkillTimeConstant = 275;
    public const int RequiredSkill1 = 182;
    public const int RequiredSkill1Level = 277;
    public const int RequiredSkill2 = 183;
    public const int RequiredSkill2Level = 278;
    public const int RequiredSkill3 = 184;
    public const int RequiredSkill3Level = 279;
}

/// <summary>
/// EVE weapon group constants
/// </summary>
public static class EveWeaponGroups
{
    // Energy Weapons
    public const int EnergyWeapon = 53;
    public const int PulseLaser = 74;
    public const int BeamLaser = 76;

    // Projectile Weapons
    public const int ProjectileWeapon = 55;
    public const int AutoCannon = 56;
    public const int Artillery = 57;

    // Hybrid Weapons
    public const int HybridWeapon = 74;
    public const int Blaster = 74;
    public const int Railgun = 55;

    // Missile Launchers
    public const int MissileLauncher = 507;
    public const int RocketLauncher = 771;
    public const int LightMissileLauncher = 771;
    public const int HeavyMissileLauncher = 510;
    public const int CruiseMissileLauncher = 506;
    public const int TorpedoLauncher = 509;

    // Drone Groups
    public const int LightDrone = 100;
    public const int MediumDrone = 101;
    public const int HeavyDrone = 102;
    public const int SentryDrone = 106;

    // Support Modules
    public const int DamageAmplifier = 1206; // Damage enhancer modules
    public const int TrackingEnhancer = 1202;
    public const int TrackingComputer = 213;
}

// Static StackingPenalty class removed - now using centralized IStackingPenaltyService

/// <summary>
/// EVE damage calculation result with breakdown
/// </summary>
public class WeaponDamageProfile
{
    public double EmPercentage { get; set; }
    public double ThermalPercentage { get; set; }
    public double KineticPercentage { get; set; }
    public double ExplosivePercentage { get; set; }
    public double TotalDamage { get; set; }
    public double BaseDps { get; set; }
    public double RateOfFire { get; set; }
    public double OptimalRange { get; set; }
    public double FalloffRange { get; set; }
    public double TrackingSpeed { get; set; }
}