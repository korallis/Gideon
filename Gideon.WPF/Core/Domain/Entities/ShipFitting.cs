// ==========================================================================
// ShipFitting.cs - Ship Fitting Domain Entities
// ==========================================================================
// Domain entities for ship fitting management, analysis, and optimization.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gideon.WPF.Core.Domain.Entities;

/// <summary>
/// Represents a complete ship fitting configuration
/// </summary>
public class ShipFitting
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    // Associated ship and character
    public int ShipId { get; set; }
    
    public virtual Ship Ship { get; set; } = null!;
    
    public int? CharacterId { get; set; }
    
    public virtual Character? Character { get; set; }
    
    // Fitting configuration (JSON stored for flexibility)
    [MaxLength(4000)]
    public string HighSlotConfiguration { get; set; } = "[]";
    
    [MaxLength(4000)]
    public string MidSlotConfiguration { get; set; } = "[]";
    
    [MaxLength(4000)]
    public string LowSlotConfiguration { get; set; } = "[]";
    
    [MaxLength(4000)]
    public string RigSlotConfiguration { get; set; } = "[]";
    
    [MaxLength(4000)]
    public string SubsystemConfiguration { get; set; } = "[]";
    
    [MaxLength(2000)]
    public string DroneConfiguration { get; set; } = "[]";
    
    [MaxLength(2000)]
    public string CargoConfiguration { get; set; } = "[]";
    
    // Calculated statistics
    public double CalculatedDps { get; set; }
    
    public double CalculatedVolleyDamage { get; set; }
    
    public double CalculatedShieldHp { get; set; }
    
    public double CalculatedArmorHp { get; set; }
    
    public double CalculatedHullHp { get; set; }
    
    public double CalculatedCapacitorStability { get; set; }
    
    public double CalculatedCapacitorTime { get; set; }
    
    public double CalculatedMaxVelocity { get; set; }
    
    public double CalculatedAlignTime { get; set; }
    
    public double CalculatedWarpSpeed { get; set; }
    
    public double CalculatedMaxTargetingRange { get; set; }
    
    public double CalculatedScanResolution { get; set; }
    
    public double CalculatedSignatureRadius { get; set; }
    
    // Resource usage
    public double UsedPowerGrid { get; set; }
    
    public double UsedCpu { get; set; }
    
    public double UsedCalibration { get; set; }
    
    public double UsedCargoSpace { get; set; }
    
    public double UsedDroneBay { get; set; }
    
    public double UsedDroneBandwidth { get; set; }
    
    // Fitting validation
    public bool IsValid { get; set; } = true;
    
    [MaxLength(1000)]
    public string? ValidationErrors { get; set; }
    
    // Import/Export formats
    [MaxLength(4000)]
    public string? EftFormat { get; set; }
    
    [MaxLength(4000)]
    public string? DnaFormat { get; set; }
    
    // Cost analysis
    public decimal? EstimatedCost { get; set; }
    
    public DateTime? LastCostUpdate { get; set; }
    
    // Usage tracking
    public int UseCount { get; set; }
    
    public DateTime? LastUsed { get; set; }
    
    // Tags and categorization
    [MaxLength(500)]
    public string? Tags { get; set; }
    
    [MaxLength(100)]
    public string? FittingCategory { get; set; } // PvP, PvE, Mining, etc.
    
    [MaxLength(100)]
    public string? ActivityType { get; set; } // Mission, Ratting, Mining, etc.
    
    // Sharing and visibility
    public bool IsPublic { get; set; } = false;
    
    public bool IsShared { get; set; } = false;
    
    [MaxLength(100)]
    public string? SharedWith { get; set; }
    
    // Version control
    public int Version { get; set; } = 1;
    
    public Guid? ParentFittingId { get; set; }
    
    public virtual ShipFitting? ParentFitting { get; set; }
    
    public virtual ICollection<ShipFitting> ChildFittings { get; set; } = new List<ShipFitting>();
    
    // Tracking
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
    
    public bool IsFavorite { get; set; } = false;
}