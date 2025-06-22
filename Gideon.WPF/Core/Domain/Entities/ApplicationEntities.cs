// ==========================================================================
// ApplicationEntities.cs - Application Management Entities
// ==========================================================================
// Domain entities for application settings, themes, and system management.
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
/// Application settings entity
/// </summary>
[Table("ApplicationSettings")]
public class ApplicationSettings
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string SettingKey { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string SettingValue { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? Category { get; set; }

    public bool IsUserConfigurable { get; set; } = true;

    public bool IsEncrypted { get; set; } = false;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// User preference entity
/// </summary>
[Table("UserPreferences")]
public class UserPreference
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string PreferenceKey { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string PreferenceValue { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? Category { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Holographic theme entity
/// </summary>
[Table("HolographicThemes")]
public class HolographicTheme
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string ThemeName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [StringLength(7)]
    public string PrimaryColor { get; set; } = string.Empty;

    [Required]
    [StringLength(7)]
    public string SecondaryColor { get; set; } = string.Empty;

    [Required]
    [StringLength(7)]
    public string AccentColor { get; set; } = string.Empty;

    [Required]
    [StringLength(7)]
    public string ParticleColor { get; set; } = string.Empty;

    public double HolographicIntensity { get; set; } = 1.0;

    public bool EnableParticleEffects { get; set; } = true;

    public bool EnableAnimations { get; set; } = true;

    public bool IsDefault { get; set; } = false;

    public bool IsCustom { get; set; } = false;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Backup entry tracking entity
/// </summary>
[Table("BackupEntries")]
public class BackupEntry
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string BackupName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;

    public BackupType BackupType { get; set; }

    public long FileSizeBytes { get; set; }

    public bool IsCompressed { get; set; } = true;

    public bool IsEncrypted { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? RestoredDate { get; set; }

    public int? CharacterId { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(500)]
    public string? ChecksumHash { get; set; }

    public bool IsValid { get; set; } = true;

    // Navigation Properties
    [ForeignKey("CharacterId")]
    public virtual Character? Character { get; set; }
}

/// <summary>
/// Shared character data entity
/// </summary>
[Table("SharedCharacterData")]
public class SharedCharacterData
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CharacterId { get; set; }

    [Required]
    [StringLength(200)]
    public string ShareName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    public SharingMode SharingMode { get; set; }

    public PrivacyLevel PrivacyLevel { get; set; }

    [Required]
    [Column(TypeName = "TEXT")]
    public string SharedData { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiryDate { get; set; }

    public int ViewCount { get; set; } = 0;

    public int LikeCount { get; set; } = 0;

    public int CommentCount { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    [StringLength(100)]
    public string? ShareCode { get; set; }

    // Navigation Properties
    [ForeignKey("CharacterId")]
    public virtual Character Character { get; set; } = null!;
}

/// <summary>
/// Audit log entity for tracking system actions
/// </summary>
[Table("AuditLogs")]
public class AuditLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Action { get; set; } = string.Empty;

    [StringLength(100)]
    public string? EntityType { get; set; }

    [StringLength(100)]
    public string? EntityId { get; set; }

    [StringLength(100)]
    public string? UserId { get; set; }

    [Column(TypeName = "TEXT")]
    public string? OldValues { get; set; }

    [Column(TypeName = "TEXT")]
    public string? NewValues { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string? IpAddress { get; set; }

    [StringLength(500)]
    public string? UserAgent { get; set; }
}

/// <summary>
/// Performance metric tracking entity
/// </summary>
[Table("PerformanceMetrics")]
public class PerformanceMetric
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string MetricName { get; set; } = string.Empty;

    public double MetricValue { get; set; }

    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string? Source { get; set; }
}

/// <summary>
/// Error log entity for tracking application errors
/// </summary>
[Table("ErrorLogs")]
public class ErrorLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(2000)]
    public string ErrorMessage { get; set; } = string.Empty;

    [Column(TypeName = "TEXT")]
    public string? StackTrace { get; set; }

    [StringLength(100)]
    public string? Source { get; set; }

    [StringLength(50)]
    public string? Severity { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string? UserId { get; set; }

    [StringLength(100)]
    public string? CharacterId { get; set; }

    public bool IsResolved { get; set; } = false;

    [StringLength(1000)]
    public string? Resolution { get; set; }
}

/// <summary>
/// Backup type enumeration
/// </summary>
public enum BackupType
{
    Full,
    Incremental,
    CharacterData,
    SkillPlans,
    ShipFittings,
    MarketData,
    Settings
}

/// <summary>
/// Sharing mode enumeration
/// </summary>
public enum SharingMode
{
    Portfolio,
    SkillPlan,
    FitExport,
    StatsOnly,
    Complete
}

/// <summary>
/// Privacy level enumeration
/// </summary>
public enum PrivacyLevel
{
    Public,
    Friends,
    Corporation,
    Alliance,
    Private
}