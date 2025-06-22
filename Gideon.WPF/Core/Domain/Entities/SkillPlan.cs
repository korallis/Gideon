// ==========================================================================
// SkillPlan.cs - Skill Plan Domain Entities
// ==========================================================================
// Domain entities for skill planning and training queue management.
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
/// Skill plan entity for character skill training management
/// </summary>
[Table("SkillPlans")]
public class SkillPlan
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CharacterId { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    public SkillPlanStatus Status { get; set; } = SkillPlanStatus.Active;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

    public DateTime? StartDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public TimeSpan EstimatedTrainingTime { get; set; }

    public bool IsTemplate { get; set; }

    public bool IsActive { get; set; } = true;

    [StringLength(100)]
    public string? Category { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    // Navigation Properties
    [ForeignKey("CharacterId")]
    public virtual Character Character { get; set; } = null!;

    public virtual ICollection<SkillPlanEntry> Entries { get; set; } = new List<SkillPlanEntry>();
}

/// <summary>
/// Individual skill entries within a skill plan
/// </summary>
[Table("SkillPlanEntries")]
public class SkillPlanEntry
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int SkillPlanId { get; set; }

    [Required]
    public int SkillTypeId { get; set; }

    [StringLength(100)]
    public string SkillName { get; set; } = string.Empty;

    public int TargetLevel { get; set; }

    public int CurrentLevel { get; set; }

    public long RequiredSkillPoints { get; set; }

    public TimeSpan EstimatedTrainingTime { get; set; }

    public int Priority { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime? CompletedDate { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    public string? Notes { get; set; }

    // Navigation Properties
    [ForeignKey("SkillPlanId")]
    public virtual SkillPlan SkillPlan { get; set; } = null!;

    [ForeignKey("SkillTypeId")]
    public virtual EveType? SkillType { get; set; }
}

/// <summary>
/// Skill plan status enumeration
/// </summary>
public enum SkillPlanStatus
{
    Active,
    Paused,
    Completed,
    Archived
}