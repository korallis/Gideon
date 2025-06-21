using Microsoft.EntityFrameworkCore;
using Gideon.WPF.Core.Domain.Entities;

namespace Gideon.WPF.Infrastructure.Data.Context;

/// <summary>
/// Entity Framework DbContext for Gideon application
/// </summary>
public class GideonDbContext : DbContext
{
    public GideonDbContext(DbContextOptions<GideonDbContext> options) : base(options)
    {
    }

    // DbSets for entities
    public DbSet<Character> Characters { get; set; } = null!;
    
    public DbSet<Ship> Ships { get; set; } = null!;
    
    public DbSet<Module> Modules { get; set; } = null!;
    
    public DbSet<ShipFitting> ShipFittings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Character entity
        modelBuilder.Entity<Character>(entity =>
        {
            entity.HasKey(e => e.CharacterId);
            
            entity.Property(e => e.CharacterId)
                .ValueGeneratedNever(); // EVE character IDs are assigned by CCP
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.CorporationName)
                .HasMaxLength(200);
            
            entity.Property(e => e.AllianceName)
                .HasMaxLength(200);
            
            entity.Property(e => e.Description)
                .HasMaxLength(500);
            
            entity.Property(e => e.Portrait64x64)
                .HasMaxLength(500);
            
            entity.Property(e => e.Portrait128x128)
                .HasMaxLength(500);
            
            entity.Property(e => e.Portrait256x256)
                .HasMaxLength(500);
            
            entity.Property(e => e.Portrait512x512)
                .HasMaxLength(500);
            
            entity.Property(e => e.AuthenticatedScopes)
                .HasMaxLength(1000);

            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsAuthenticated);
        });

        // Configure Ship entity
        modelBuilder.Entity<Ship>(entity =>
        {
            entity.HasKey(e => e.ShipId);
            
            entity.Property(e => e.ShipId)
                .ValueGeneratedNever(); // EVE ship IDs are assigned by CCP
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.GroupName)
                .HasMaxLength(100);
            
            entity.Property(e => e.CategoryName)
                .HasMaxLength(100);
            
            entity.Property(e => e.Description)
                .HasMaxLength(1000);
            
            entity.Property(e => e.ShipBonuses)
                .HasMaxLength(4000);
            
            entity.Property(e => e.RoleSkillBonuses)
                .HasMaxLength(4000);
            
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500);
            
            entity.Property(e => e.ModelPath)
                .HasMaxLength(500);

            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.GroupId);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.IsActive);
        });

        // Configure Module entity
        modelBuilder.Entity<Module>(entity =>
        {
            entity.HasKey(e => e.ModuleId);
            
            entity.Property(e => e.ModuleId)
                .ValueGeneratedNever(); // EVE module IDs are assigned by CCP
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.GroupName)
                .HasMaxLength(100);
            
            entity.Property(e => e.CategoryName)
                .HasMaxLength(100);
            
            entity.Property(e => e.Description)
                .HasMaxLength(1000);
            
            entity.Property(e => e.SlotType)
                .IsRequired()
                .HasMaxLength(20);
            
            entity.Property(e => e.ModuleEffects)
                .HasMaxLength(4000);
            
            entity.Property(e => e.ModuleBonuses)
                .HasMaxLength(4000);
            
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500);
            
            entity.Property(e => e.MetaGroup)
                .HasMaxLength(50);
            
            entity.Property(e => e.FittingRestrictions)
                .HasMaxLength(500);
            
            entity.Property(e => e.CompatibleAmmoTypes)
                .HasMaxLength(1000);

            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.GroupId);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.SlotType);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsPublished);
        });

        // Configure ShipFitting entity
        modelBuilder.Entity<ShipFitting>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Description)
                .HasMaxLength(500);
            
            entity.Property(e => e.HighSlotConfiguration)
                .IsRequired()
                .HasMaxLength(4000)
                .HasDefaultValue("[]");
            
            entity.Property(e => e.MidSlotConfiguration)
                .IsRequired()
                .HasMaxLength(4000)
                .HasDefaultValue("[]");
            
            entity.Property(e => e.LowSlotConfiguration)
                .IsRequired()
                .HasMaxLength(4000)
                .HasDefaultValue("[]");
            
            entity.Property(e => e.RigSlotConfiguration)
                .IsRequired()
                .HasMaxLength(4000)
                .HasDefaultValue("[]");
            
            entity.Property(e => e.SubsystemConfiguration)
                .IsRequired()
                .HasMaxLength(4000)
                .HasDefaultValue("[]");
            
            entity.Property(e => e.DroneConfiguration)
                .IsRequired()
                .HasMaxLength(2000)
                .HasDefaultValue("[]");
            
            entity.Property(e => e.CargoConfiguration)
                .IsRequired()
                .HasMaxLength(2000)
                .HasDefaultValue("[]");
            
            entity.Property(e => e.ValidationErrors)
                .HasMaxLength(1000);
            
            entity.Property(e => e.EftFormat)
                .HasMaxLength(4000);
            
            entity.Property(e => e.DnaFormat)
                .HasMaxLength(4000);
            
            entity.Property(e => e.Tags)
                .HasMaxLength(500);
            
            entity.Property(e => e.FittingCategory)
                .HasMaxLength(100);
            
            entity.Property(e => e.ActivityType)
                .HasMaxLength(100);
            
            entity.Property(e => e.SharedWith)
                .HasMaxLength(100);

            // Relationships
            entity.HasOne(e => e.Ship)
                .WithMany()
                .HasForeignKey(e => e.ShipId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Character)
                .WithMany()
                .HasForeignKey(e => e.CharacterId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.ParentFitting)
                .WithMany(e => e.ChildFittings)
                .HasForeignKey(e => e.ParentFittingId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.ShipId);
            entity.HasIndex(e => e.CharacterId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsFavorite);
            entity.HasIndex(e => e.IsPublic);
            entity.HasIndex(e => e.FittingCategory);
            entity.HasIndex(e => e.ActivityType);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.UpdatedAt);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps for entities that implement tracking
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Character or Ship or Module or ShipFitting)
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var now = DateTime.UtcNow;
            
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.GetType().GetProperty("CreatedAt") != null)
                    entry.Property("CreatedAt").CurrentValue = now;
            }
            
            if (entry.Entity.GetType().GetProperty("UpdatedAt") != null)
                entry.Property("UpdatedAt").CurrentValue = now;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}