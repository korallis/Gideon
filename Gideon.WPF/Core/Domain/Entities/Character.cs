// ==========================================================================
// Character.cs - Character Domain Entity
// ==========================================================================
// Core character entity representing an EVE Online character with comprehensive
// skill tracking, statistics, and holographic interface integration.
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
/// Represents an EVE Online character with comprehensive data tracking
/// </summary>
[Table("Characters")]
public class Character
{
    [Key]
    public int Id { get; set; }

    [Required]
    public long CharacterId { get; set; }

    [Required]
    [StringLength(100)]
    public string CharacterName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public long? CorporationId { get; set; }

    [StringLength(100)]
    public string? CorporationName { get; set; }

    public long? AllianceId { get; set; }

    [StringLength(100)]
    public string? AllianceName { get; set; }

    public long TotalSkillPoints { get; set; }

    public decimal WalletBalance { get; set; }

    public double SecurityStatus { get; set; }

    public DateTime? Birthday { get; set; }

    public CharacterState CharacterState { get; set; } = CharacterState.Active;

    [StringLength(500)]
    public string? RefreshToken { get; set; }

    [StringLength(2000)]
    public string? AccessToken { get; set; }

    public DateTime? TokenExpiry { get; set; }

    // Portrait URLs
    [StringLength(500)]
    public string? Portrait64x64 { get; set; }
    
    [StringLength(500)]
    public string? Portrait128x128 { get; set; }
    
    [StringLength(500)]
    public string? Portrait256x256 { get; set; }
    
    [StringLength(500)]
    public string? Portrait512x512 { get; set; }

    // Authentication related
    public bool IsAuthenticated { get; set; }
    
    public DateTime? LastAuthenticationTime { get; set; }
    
    [StringLength(1000)]
    public string? AuthenticatedScopes { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginDate { get; set; }

    public DateTime? LastDataUpdate { get; set; }

    public bool IsMainCharacter { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual ICollection<CharacterSkill> Skills { get; set; } = new List<CharacterSkill>();
    public virtual ICollection<SkillPlan> SkillPlans { get; set; } = new List<SkillPlan>();
    public virtual ICollection<CharacterAsset> Assets { get; set; } = new List<CharacterAsset>();
    public virtual ICollection<CharacterLocation> Locations { get; set; } = new List<CharacterLocation>();
    public virtual ICollection<CharacterGoal> Goals { get; set; } = new List<CharacterGoal>();
    public virtual ICollection<ShipFitting> ShipFittings { get; set; } = new List<ShipFitting>();
    public virtual ICollection<CharacterStatistics> Statistics { get; set; } = new List<CharacterStatistics>();
    public virtual ICollection<SharedCharacterData> SharedData { get; set; } = new List<SharedCharacterData>();
}

/// <summary>
/// Character skill tracking entity
/// </summary>
[Table("CharacterSkills")]
public class CharacterSkill
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CharacterId { get; set; }

    [Required]
    public int SkillTypeId { get; set; }

    [StringLength(100)]
    public string SkillName { get; set; } = string.Empty;

    public int CurrentLevel { get; set; }

    public long CurrentSkillPoints { get; set; }

    public long? SkillPointsInLevel { get; set; }

    public long? SkillPointsToNextLevel { get; set; }

    public DateTime? TrainingStartDate { get; set; }

    public DateTime? TrainingEndDate { get; set; }

    public bool IsTraining { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("CharacterId")]
    public virtual Character Character { get; set; } = null!;

    [ForeignKey("SkillTypeId")]
    public virtual EveType? SkillType { get; set; }
}

/// <summary>
/// Character asset tracking entity
/// </summary>
[Table("CharacterAssets")]
public class CharacterAsset
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CharacterId { get; set; }

    [Required]
    public long ItemId { get; set; }

    [Required]
    public int TypeId { get; set; }

    [StringLength(100)]
    public string TypeName { get; set; } = string.Empty;

    public long Quantity { get; set; }

    public long LocationId { get; set; }

    [StringLength(100)]
    public string LocationName { get; set; } = string.Empty;

    [StringLength(50)]
    public string LocationFlag { get; set; } = string.Empty;

    public decimal? EstimatedValue { get; set; }

    public bool IsSingleton { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("CharacterId")]
    public virtual Character Character { get; set; } = null!;

    [ForeignKey("TypeId")]
    public virtual EveType? ItemType { get; set; }
}

/// <summary>
/// Character location tracking entity
/// </summary>
[Table("CharacterLocations")]
public class CharacterLocation
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CharacterId { get; set; }

    public int? SolarSystemId { get; set; }

    [StringLength(100)]
    public string? SolarSystemName { get; set; }

    public int? RegionId { get; set; }

    [StringLength(100)]
    public string? RegionName { get; set; }

    public long? StationId { get; set; }

    [StringLength(100)]
    public string? StationName { get; set; }

    public double? SecurityStatus { get; set; }

    [StringLength(50)]
    public string? Activity { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public bool IsCurrent { get; set; }

    // Navigation Properties
    [ForeignKey("CharacterId")]
    public virtual Character Character { get; set; } = null!;

    [ForeignKey("SolarSystemId")]
    public virtual EveSolarSystem? SolarSystem { get; set; }

    [ForeignKey("RegionId")]
    public virtual EveRegion? Region { get; set; }
}

/// <summary>
/// Character goal tracking entity
/// </summary>
[Table("CharacterGoals")]
public class CharacterGoal
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CharacterId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    public GoalCategory Category { get; set; }

    public GoalPriority Priority { get; set; }

    public int Progress { get; set; }

    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    public DateTime? TargetDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public bool IsCompleted { get; set; }

    public bool IsActive { get; set; } = true;

    [StringLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("CharacterId")]
    public virtual Character Character { get; set; } = null!;
}

/// <summary>
/// Character statistics tracking entity
/// </summary>
[Table("CharacterStatistics")]
public class CharacterStatistics
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CharacterId { get; set; }

    public long TotalSkillPoints { get; set; }

    public decimal ISKBalance { get; set; }

    public decimal TotalAssets { get; set; }

    public int PvPKills { get; set; }

    public int PvPLosses { get; set; }

    public double SecurityStatus { get; set; }

    public TimeSpan TotalPlayTime { get; set; }

    [Column(TypeName = "TEXT")]
    public Dictionary<string, long> SkillCategoryBreakdown { get; set; } = new();

    public DateTime StatisticsDate { get; set; } = DateTime.UtcNow;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("CharacterId")]
    public virtual Character Character { get; set; } = null!;
}

/// <summary>
/// Character state enumeration
/// </summary>
public enum CharacterState
{
    Active,
    Inactive,
    Biomassed,
    Unknown
}

/// <summary>
/// Goal category enumeration
/// </summary>
public enum GoalCategory
{
    Skills,
    ISK,
    Ships,
    PvP,
    Industry,
    Exploration,
    Social,
    Achievements,
    Other
}

/// <summary>
/// Goal priority enumeration
/// </summary>
public enum GoalPriority
{
    Low,
    Medium,
    High,
    Critical
}