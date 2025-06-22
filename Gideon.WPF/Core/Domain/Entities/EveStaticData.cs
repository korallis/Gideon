// ==========================================================================
// EveStaticData.cs - EVE Online Static Data Entities
// ==========================================================================
// Domain entities for EVE Online static data including types, regions, systems.
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
/// EVE Online item/type entity
/// </summary>
[Table("EveTypes")]
public class EveType
{
    [Key]
    public int TypeId { get; set; }

    [Required]
    [StringLength(200)]
    public string TypeName { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    public int GroupId { get; set; }

    public double Mass { get; set; }

    public double Volume { get; set; }

    public double Capacity { get; set; }

    public int PortionSize { get; set; }

    public decimal BasePrice { get; set; }

    public bool Published { get; set; }

    public int? MarketGroupId { get; set; }

    public double? Radius { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("GroupId")]
    public virtual EveGroup? Group { get; set; }

    [ForeignKey("MarketGroupId")]
    public virtual EveMarketGroup? MarketGroup { get; set; }

    public virtual ICollection<CharacterSkill> CharacterSkills { get; set; } = new List<CharacterSkill>();
    public virtual ICollection<CharacterAsset> CharacterAssets { get; set; } = new List<CharacterAsset>();
    public virtual ICollection<FittingModule> FittingModules { get; set; } = new List<FittingModule>();
    public virtual ICollection<MarketData> MarketData { get; set; } = new List<MarketData>();
}

/// <summary>
/// EVE Online group entity
/// </summary>
[Table("EveGroups")]
public class EveGroup
{
    [Key]
    public int GroupId { get; set; }

    [Required]
    [StringLength(200)]
    public string GroupName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    public int CategoryId { get; set; }

    public bool Published { get; set; }

    public bool UseBasePrice { get; set; }

    public bool Anchored { get; set; }

    public bool AnchoredExplosively { get; set; }

    public bool FittableNonSingleton { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("CategoryId")]
    public virtual EveCategory? Category { get; set; }

    public virtual ICollection<EveType> Types { get; set; } = new List<EveType>();
}

/// <summary>
/// EVE Online category entity
/// </summary>
[Table("EveCategories")]
public class EveCategory
{
    [Key]
    public int CategoryId { get; set; }

    [Required]
    [StringLength(200)]
    public string CategoryName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    public bool Published { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual ICollection<EveGroup> Groups { get; set; } = new List<EveGroup>();
}

/// <summary>
/// EVE Online market group entity
/// </summary>
[Table("EveMarketGroups")]
public class EveMarketGroup
{
    [Key]
    public int MarketGroupId { get; set; }

    [Required]
    [StringLength(200)]
    public string MarketGroupName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    public int? ParentGroupId { get; set; }

    public bool HasTypes { get; set; }

    [StringLength(100)]
    public string? IconId { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("ParentGroupId")]
    public virtual EveMarketGroup? ParentGroup { get; set; }

    public virtual ICollection<EveMarketGroup> ChildGroups { get; set; } = new List<EveMarketGroup>();
    public virtual ICollection<EveType> Types { get; set; } = new List<EveType>();
}

/// <summary>
/// EVE Online region entity
/// </summary>
[Table("EveRegions")]
public class EveRegion
{
    [Key]
    public int RegionId { get; set; }

    [Required]
    [StringLength(200)]
    public string RegionName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    public double CenterX { get; set; }

    public double CenterY { get; set; }

    public double CenterZ { get; set; }

    public double MaxX { get; set; }

    public double MaxY { get; set; }

    public double MaxZ { get; set; }

    public double MinX { get; set; }

    public double MinY { get; set; }

    public double MinZ { get; set; }

    public int? FactionId { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual ICollection<EveSolarSystem> SolarSystems { get; set; } = new List<EveSolarSystem>();
    public virtual ICollection<CharacterLocation> CharacterLocations { get; set; } = new List<CharacterLocation>();
    public virtual ICollection<MarketData> MarketData { get; set; } = new List<MarketData>();
}

/// <summary>
/// EVE Online solar system entity
/// </summary>
[Table("EveSolarSystems")]
public class EveSolarSystem
{
    [Key]
    public int SolarSystemId { get; set; }

    [Required]
    [StringLength(200)]
    public string SolarSystemName { get; set; } = string.Empty;

    public int RegionId { get; set; }

    public int ConstellationId { get; set; }

    public double SecurityStatus { get; set; }

    public string SecurityClass { get; set; } = string.Empty;

    public double CenterX { get; set; }

    public double CenterY { get; set; }

    public double CenterZ { get; set; }

    public int? FactionId { get; set; }

    public double Luminosity { get; set; }

    public double Radius { get; set; }

    public int? SunTypeId { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("RegionId")]
    public virtual EveRegion? Region { get; set; }

    public virtual ICollection<EveStation> Stations { get; set; } = new List<EveStation>();
    public virtual ICollection<CharacterLocation> CharacterLocations { get; set; } = new List<CharacterLocation>();
}

/// <summary>
/// EVE Online station entity
/// </summary>
[Table("EveStations")]
public class EveStation
{
    [Key]
    public long StationId { get; set; }

    [Required]
    [StringLength(200)]
    public string StationName { get; set; } = string.Empty;

    public int SolarSystemId { get; set; }

    public int TypeId { get; set; }

    public double PositionX { get; set; }

    public double PositionY { get; set; }

    public double PositionZ { get; set; }

    public int? CorporationId { get; set; }

    public double DockingCostPerVolume { get; set; }

    public double MaxShipVolumeDockable { get; set; }

    public double OfficeRentalCost { get; set; }

    public double ReprocessingEfficiency { get; set; }

    public double ReprocessingStationsTake { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("SolarSystemId")]
    public virtual EveSolarSystem? SolarSystem { get; set; }

    [ForeignKey("TypeId")]
    public virtual EveType? StationType { get; set; }
}