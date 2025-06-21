using System.ComponentModel.DataAnnotations;

namespace Gideon.WPF.Core.Domain.Entities;

/// <summary>
/// Represents an EVE Online character with authentication and profile information
/// </summary>
public class Character
{
    [Key]
    public int CharacterId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public int CorporationId { get; set; }
    
    [MaxLength(200)]
    public string CorporationName { get; set; } = string.Empty;
    
    public int? AllianceId { get; set; }
    
    [MaxLength(200)]
    public string? AllianceName { get; set; }
    
    public DateTime DateOfBirth { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public float SecurityStatus { get; set; }
    
    // Portrait URLs
    [MaxLength(500)]
    public string? Portrait64x64 { get; set; }
    
    [MaxLength(500)]
    public string? Portrait128x128 { get; set; }
    
    [MaxLength(500)]
    public string? Portrait256x256 { get; set; }
    
    [MaxLength(500)]
    public string? Portrait512x512 { get; set; }
    
    // Authentication related
    public bool IsAuthenticated { get; set; }
    
    public DateTime? LastAuthenticationTime { get; set; }
    
    public DateTime? TokenExpiresAt { get; set; }
    
    [MaxLength(1000)]
    public string? AuthenticatedScopes { get; set; }
    
    // Tracking
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
}