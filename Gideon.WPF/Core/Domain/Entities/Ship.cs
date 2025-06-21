using System.ComponentModel.DataAnnotations;

namespace Gideon.WPF.Core.Domain.Entities;

/// <summary>
/// Represents an EVE Online ship type with all properties and slot information
/// </summary>
public class Ship
{
    [Key]
    public int ShipId { get; set; }
    
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
    
    // Slot configuration
    public int HighSlots { get; set; }
    
    public int MidSlots { get; set; }
    
    public int LowSlots { get; set; }
    
    public int RigSlots { get; set; }
    
    public int SubsystemSlots { get; set; }
    
    public int TurretHardpoints { get; set; }
    
    public int LauncherHardpoints { get; set; }
    
    // Ship attributes
    public double Hull { get; set; }
    
    public double Armor { get; set; }
    
    public double Shield { get; set; }
    
    public double Capacitor { get; set; }
    
    public double PowerGrid { get; set; }
    
    public double CpuOutput { get; set; }
    
    public double CalibrationPoints { get; set; }
    
    public double MaxVelocity { get; set; }
    
    public double Agility { get; set; }
    
    public double Mass { get; set; }
    
    public double Volume { get; set; }
    
    public double CargoCapacity { get; set; }
    
    public double FuelBayCapacity { get; set; }
    
    public double DroneBayCapacity { get; set; }
    
    public int MaxActiveDrones { get; set; }
    
    public double DronesBandwidth { get; set; }
    
    // Targeting
    public int MaxTargets { get; set; }
    
    public double MaxTargetingRange { get; set; }
    
    public double ScanResolution { get; set; }
    
    public double SignatureRadius { get; set; }
    
    // Resistances (base values)
    public double EmDamageResonance { get; set; }
    
    public double ThermalDamageResonance { get; set; }
    
    public double KineticDamageResonance { get; set; }
    
    public double ExplosiveDamageResonance { get; set; }
    
    // Shield resistances
    public double ShieldEmDamageResonance { get; set; }
    
    public double ShieldThermalDamageResonance { get; set; }
    
    public double ShieldKineticDamageResonance { get; set; }
    
    public double ShieldExplosiveDamageResonance { get; set; }
    
    // Armor resistances  
    public double ArmorEmDamageResonance { get; set; }
    
    public double ArmorThermalDamageResonance { get; set; }
    
    public double ArmorKineticDamageResonance { get; set; }
    
    public double ArmorExplosiveDamageResonance { get; set; }
    
    // Ship bonuses (JSON stored)
    [MaxLength(4000)]
    public string? ShipBonuses { get; set; }
    
    [MaxLength(4000)]
    public string? RoleSkillBonuses { get; set; }
    
    // Visual/UI properties
    [MaxLength(500)]
    public string? ImageUrl { get; set; }
    
    [MaxLength(500)]
    public string? ModelPath { get; set; }
    
    // Meta information
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
}