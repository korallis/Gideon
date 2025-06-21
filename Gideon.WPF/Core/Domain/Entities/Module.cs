using System.ComponentModel.DataAnnotations;

namespace Gideon.WPF.Core.Domain.Entities;

/// <summary>
/// Represents an EVE Online module/item that can be fitted to ships
/// </summary>
public class Module
{
    [Key]
    public int ModuleId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public int GroupId { get; set; }
    
    [MaxLength(100)]
    public string GroupName { get; set; } = string.Empty;
    
    public int CategoryId { get; set; }
    
    [MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    // Fitting requirements
    public double PowerGridRequirement { get; set; }
    
    public double CpuRequirement { get; set; }
    
    public double CalibrationRequirement { get; set; }
    
    // Module properties
    public double Volume { get; set; }
    
    public double Mass { get; set; }
    
    public double Capacity { get; set; }
    
    // Slot type
    [MaxLength(20)]
    public string SlotType { get; set; } = string.Empty; // High, Mid, Low, Rig, Subsystem
    
    // Activation properties
    public double? ActivationTime { get; set; }
    
    public double? CapacitorNeed { get; set; }
    
    public double? ReloadTime { get; set; }
    
    public int? Charges { get; set; }
    
    // Damage properties (for weapons)
    public double? EmDamage { get; set; }
    
    public double? ThermalDamage { get; set; }
    
    public double? KineticDamage { get; set; }
    
    public double? ExplosiveDamage { get; set; }
    
    public double? DamageMultiplier { get; set; }
    
    public double? RateOfFire { get; set; }
    
    // Range properties
    public double? OptimalRange { get; set; }
    
    public double? AccuracyFalloff { get; set; }
    
    public double? MaximumRange { get; set; }
    
    // Shield/Armor repair
    public double? ShieldBonus { get; set; }
    
    public double? ArmorRepairBonus { get; set; }
    
    public double? HullRepairBonus { get; set; }
    
    // Speed/Agility modifications
    public double? VelocityModifier { get; set; }
    
    public double? InertiaModifier { get; set; }
    
    public double? SignatureRadiusModifier { get; set; }
    
    // Targeting modifications
    public double? ScanResolutionModifier { get; set; }
    
    public double? MaxTargetingRangeModifier { get; set; }
    
    // Resistance modifications
    public double? EmResistanceBonus { get; set; }
    
    public double? ThermalResistanceBonus { get; set; }
    
    public double? KineticResistanceBonus { get; set; }
    
    public double? ExplosiveResistanceBonus { get; set; }
    
    // Capacitor modifications
    public double? CapacitorBonus { get; set; }
    
    public double? CapacitorRechargeTimeModifier { get; set; }
    
    // Stacking penalties
    public bool IsStackingPenalized { get; set; } = true;
    
    public double? StackingPenaltyMultiplier { get; set; }
    
    // Module effects (JSON stored)
    [MaxLength(4000)]
    public string? ModuleEffects { get; set; }
    
    [MaxLength(4000)]
    public string? ModuleBonuses { get; set; }
    
    // Market information
    public decimal? BasePrice { get; set; }
    
    public bool IsPublished { get; set; } = true;
    
    // Visual properties
    [MaxLength(500)]
    public string? ImageUrl { get; set; }
    
    // Meta information
    public int MetaLevel { get; set; }
    
    public int TechLevel { get; set; } = 1;
    
    [MaxLength(50)]
    public string? MetaGroup { get; set; }
    
    // Fitting restrictions
    [MaxLength(500)]
    public string? FittingRestrictions { get; set; }
    
    // Compatible ammunition types (for weapons)
    [MaxLength(1000)]
    public string? CompatibleAmmoTypes { get; set; }
    
    // Tracking
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
}