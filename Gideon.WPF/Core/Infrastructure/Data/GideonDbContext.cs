// ==========================================================================
// GideonDbContext.cs - Entity Framework Core Database Context
// ==========================================================================
// Comprehensive database context for Gideon EVE Online Toolkit featuring
// character data, ship fittings, market analysis, and holographic interface state.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Gideon.WPF.Core.Domain.Entities;

namespace Gideon.WPF.Core.Infrastructure.Data;

/// <summary>
/// Primary database context for Gideon application using Entity Framework Core with SQLite
/// </summary>
public class GideonDbContext : DbContext
{
    #region DbSets - Character Management

    public DbSet<Character> Characters { get; set; } = null!;
    public DbSet<CharacterSkill> CharacterSkills { get; set; } = null!;
    public DbSet<SkillPlan> SkillPlans { get; set; } = null!;
    public DbSet<SkillPlanEntry> SkillPlanEntries { get; set; } = null!;
    public DbSet<CharacterAsset> CharacterAssets { get; set; } = null!;
    public DbSet<CharacterLocation> CharacterLocations { get; set; } = null!;
    public DbSet<CharacterGoal> CharacterGoals { get; set; } = null!;
    public DbSet<CharacterStatistics> CharacterStatistics { get; set; } = null!;

    #endregion

    #region DbSets - Ship Fitting Management

    public DbSet<ShipFitting> ShipFittings { get; set; } = null!;
    public DbSet<FittingModule> FittingModules { get; set; } = null!;
    public DbSet<FittingPreset> FittingPresets { get; set; } = null!;
    public DbSet<FittingAnalysis> FittingAnalyses { get; set; } = null!;
    public DbSet<FittingComparison> FittingComparisons { get; set; } = null!;

    #endregion

    #region DbSets - Market Analysis

    public DbSet<MarketData> MarketData { get; set; } = null!;
    public DbSet<HistoricalMarketData> HistoricalMarketData { get; set; } = null!;
    public DbSet<PersonalMarketOrder> PersonalMarketOrders { get; set; } = null!;
    public DbSet<OrderHistory> OrderHistories { get; set; } = null!;
    public DbSet<MarketOrderPortfolio> MarketOrderPortfolios { get; set; } = null!;
    public DbSet<MarketPrediction> MarketPredictions { get; set; } = null!;
    public DbSet<MarketRegion> MarketRegions { get; set; } = null!;
    public DbSet<TradingOpportunity> TradingOpportunities { get; set; } = null!;
    public DbSet<MarketTransaction> MarketTransactions { get; set; } = null!;
    public DbSet<TransactionSyncStatus> TransactionSyncStatuses { get; set; } = null!;
    public DbSet<TransactionAnalytics> TransactionAnalytics { get; set; } = null!;
    public DbSet<TradeHub> TradeHubs { get; set; } = null!;
    public DbSet<RegionalMarketCoverage> RegionalMarketCoverage { get; set; } = null!;
    public DbSet<TradeRoute> TradeRoutes { get; set; } = null!;
    public DbSet<RouteItemProfitability> RouteItemProfitability { get; set; } = null!;
    public DbSet<MarketSyncJob> MarketSyncJobs { get; set; } = null!;
    public DbSet<RegionalMarketStatistics> RegionalMarketStatistics { get; set; } = null!;

    // Market Data Caching (TASK-177)
    public DbSet<MarketDataCacheEntry> MarketDataCache { get; set; } = null!;
    public DbSet<CacheStatistics> CacheStatistics { get; set; } = null!;
    public DbSet<CacheRefreshSchedule> CacheRefreshSchedule { get; set; } = null!;
    public DbSet<CachePerformanceMetrics> CachePerformanceMetrics { get; set; } = null!;
    public DbSet<CacheInvalidationLog> CacheInvalidationLog { get; set; } = null!;
    public DbSet<CacheConfiguration> CacheConfiguration { get; set; } = null!;

    // Market Trend Analysis (TASK-178)
    public DbSet<MarketTrendAnalysis> MarketTrendAnalyses { get; set; } = null!;
    public DbSet<MarketPrediction> MarketPredictions { get; set; } = null!;
    public DbSet<TrendSignal> TrendSignals { get; set; } = null!;
    public DbSet<SeasonalPattern> SeasonalPatterns { get; set; } = null!;
    public DbSet<AlgorithmPerformance> AlgorithmPerformance { get; set; } = null!;

    #endregion

    #region DbSets - EVE Online Static Data

    public DbSet<EveType> EveTypes { get; set; } = null!;
    public DbSet<EveGroup> EveGroups { get; set; } = null!;
    public DbSet<EveCategory> EveCategories { get; set; } = null!;
    public DbSet<EveMarketGroup> EveMarketGroups { get; set; } = null!;
    public DbSet<EveRegion> EveRegions { get; set; } = null!;
    public DbSet<EveSolarSystem> EveSolarSystems { get; set; } = null!;
    public DbSet<EveStation> EveStations { get; set; } = null!;

    // EVE Dogma Attribute System (TASK-179)
    public DbSet<EveDogmaAttribute> EveDogmaAttributes { get; set; } = null!;
    public DbSet<EveTypeAttribute> EveTypeAttributes { get; set; } = null!;
    public DbSet<EveDogmaEffect> EveDogmaEffects { get; set; } = null!;
    public DbSet<EveTypeEffect> EveTypeEffects { get; set; } = null!;
    public DbSet<EveWeaponType> EveWeaponTypes { get; set; } = null!;
    public DbSet<EveAmmunitionType> EveAmmunitionTypes { get; set; } = null!;

    #endregion

    #region DbSets - Application Data

    public DbSet<ApplicationSettings> ApplicationSettings { get; set; } = null!;
    public DbSet<UserPreference> UserPreferences { get; set; } = null!;
    public DbSet<HolographicTheme> HolographicThemes { get; set; } = null!;
    public DbSet<BackupEntry> BackupEntries { get; set; } = null!;
    public DbSet<SharedCharacterData> SharedCharacterData { get; set; } = null!;

    #endregion

    #region DbSets - Audit and Logging

    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<PerformanceMetric> PerformanceMetrics { get; set; } = null!;
    public DbSet<ErrorLog> ErrorLogs { get; set; } = null!;

    #endregion

    #region Constructor

    public GideonDbContext(DbContextOptions<GideonDbContext> options) : base(options)
    {
    }

    #endregion

    #region Configuration

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var databasePath = GetDatabasePath();
            optionsBuilder.UseSqlite($"Data Source={databasePath}");
        }

        // Enable sensitive data logging in development
        #if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
        #endif

        // Configure SQLite-specific options
        optionsBuilder.UseSqlite(options => 
        {
            options.CommandTimeout(30);
        });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Configure value conversions
        ConfigureValueConverters(modelBuilder);

        // Configure indexes for performance
        ConfigureIndexes(modelBuilder);

        // Seed initial data
        SeedInitialData(modelBuilder);
    }

    #endregion

    #region Entity Configuration

    private void ConfigureValueConverters(ModelBuilder modelBuilder)
    {
        // DateTime to UTC conversion
        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? v.Value.ToUniversalTime() : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        // Apply to all DateTime properties
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(nullableDateTimeConverter);
                }
            }
        }

        // Enum to string conversions for better readability
        modelBuilder.Entity<Character>()
            .Property(e => e.CharacterState)
            .HasConversion<string>();

        modelBuilder.Entity<SkillPlan>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<ShipFitting>()
            .Property(e => e.FittingType)
            .HasConversion<string>();

        modelBuilder.Entity<MarketData>()
            .Property(e => e.OrderType)
            .HasConversion<string>();

        modelBuilder.Entity<HistoricalMarketData>()
            .Property(e => e.Interval)
            .HasConversion<string>();

        modelBuilder.Entity<PersonalMarketOrder>()
            .Property(e => e.State)
            .HasConversion<string>();

        modelBuilder.Entity<OrderHistory>()
            .Property(e => e.HistoryType)
            .HasConversion<string>();

        modelBuilder.Entity<MarketTransaction>()
            .Property(e => e.TransactionType)
            .HasConversion<string>();

        modelBuilder.Entity<MarketTransaction>()
            .Property(e => e.JournalRefType)
            .HasConversion<string>();

        modelBuilder.Entity<TransactionSyncStatus>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<TransactionAnalytics>()
            .Property(e => e.MostActiveDay)
            .HasConversion<string>();

        // Regional Market Data enum conversions
        modelBuilder.Entity<TradeHub>()
            .Property(e => e.Tier)
            .HasConversion<string>();

        modelBuilder.Entity<RegionalMarketCoverage>()
            .Property(e => e.DataQuality)
            .HasConversion<string>();

        modelBuilder.Entity<TradeRoute>()
            .Property(e => e.RiskLevel)
            .HasConversion<string>();

        modelBuilder.Entity<RouteItemProfitability>()
            .Property(e => e.PriceTrend)
            .HasConversion<string>();

        modelBuilder.Entity<MarketSyncJob>()
            .Property(e => e.JobType)
            .HasConversion<string>();

        modelBuilder.Entity<MarketSyncJob>()
            .Property(e => e.Status)
            .HasConversion<string>();

        // Market Data Cache enum conversions (TASK-177)
        modelBuilder.Entity<MarketDataCacheEntry>()
            .Property(e => e.DataType)
            .HasConversion<string>();

        modelBuilder.Entity<MarketDataCacheEntry>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<MarketDataCacheEntry>()
            .Property(e => e.Priority)
            .HasConversion<string>();

        modelBuilder.Entity<CacheRefreshSchedule>()
            .Property(e => e.DataType)
            .HasConversion<string>();

        modelBuilder.Entity<CacheRefreshSchedule>()
            .Property(e => e.Strategy)
            .HasConversion<string>();

        modelBuilder.Entity<CacheRefreshSchedule>()
            .Property(e => e.Priority)
            .HasConversion<string>();

        modelBuilder.Entity<CachePerformanceMetrics>()
            .Property(e => e.DataType)
            .HasConversion<string>();

        modelBuilder.Entity<CachePerformanceMetrics>()
            .Property(e => e.MetricType)
            .HasConversion<string>();

        modelBuilder.Entity<CacheInvalidationLog>()
            .Property(e => e.DataType)
            .HasConversion<string>();

        modelBuilder.Entity<CacheInvalidationLog>()
            .Property(e => e.Reason)
            .HasConversion<string>();

        modelBuilder.Entity<CacheConfiguration>()
            .Property(e => e.AppliesTo)
            .HasConversion<string>();

        // Market Trend Analysis enum conversions (TASK-178)
        modelBuilder.Entity<MarketTrendAnalysis>()
            .Property(e => e.PriceTrend)
            .HasConversion<string>();

        modelBuilder.Entity<MarketTrendAnalysis>()
            .Property(e => e.VolumeTrend)
            .HasConversion<string>();

        modelBuilder.Entity<MarketTrendAnalysis>()
            .Property(e => e.MarketRegime)
            .HasConversion<string>();

        modelBuilder.Entity<MarketTrendAnalysis>()
            .Property(e => e.QualityScore)
            .HasConversion<string>();

        modelBuilder.Entity<MarketPrediction>()
            .Property(e => e.PredictedTrend)
            .HasConversion<string>();

        modelBuilder.Entity<MarketPrediction>()
            .Property(e => e.PredictedRegime)
            .HasConversion<string>();

        modelBuilder.Entity<MarketPrediction>()
            .Property(e => e.Algorithm)
            .HasConversion<string>();

        modelBuilder.Entity<MarketPrediction>()
            .Property(e => e.ActualTrend)
            .HasConversion<string>();

        modelBuilder.Entity<MarketPrediction>()
            .Property(e => e.RiskLevel)
            .HasConversion<string>();

        modelBuilder.Entity<MarketPrediction>()
            .Property(e => e.Signal)
            .HasConversion<string>();

        modelBuilder.Entity<TrendSignal>()
            .Property(e => e.SignalType)
            .HasConversion<string>();

        modelBuilder.Entity<TrendSignal>()
            .Property(e => e.Trigger)
            .HasConversion<string>();

        modelBuilder.Entity<TrendSignal>()
            .Property(e => e.Timeframe)
            .HasConversion<string>();

        modelBuilder.Entity<TrendSignal>()
            .Property(e => e.Outcome)
            .HasConversion<string>();

        modelBuilder.Entity<TrendSignal>()
            .Property(e => e.Priority)
            .HasConversion<string>();

        modelBuilder.Entity<SeasonalPattern>()
            .Property(e => e.Period)
            .HasConversion<string>();

        modelBuilder.Entity<SeasonalPattern>()
            .Property(e => e.DayOfWeek)
            .HasConversion<string>();

        modelBuilder.Entity<AlgorithmPerformance>()
            .Property(e => e.Algorithm)
            .HasConversion<string>();

        // JSON conversions for complex types
        modelBuilder.Entity<CharacterStatistics>()
            .Property(e => e.SkillCategoryBreakdown)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, long>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, long>());

        modelBuilder.Entity<FittingAnalysis>()
            .Property(e => e.PerformanceMetrics)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, double>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, double>());
    }

    private void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // Character indexes
        modelBuilder.Entity<Character>()
            .HasIndex(c => c.CharacterId)
            .IsUnique();

        modelBuilder.Entity<Character>()
            .HasIndex(c => c.CharacterName);

        modelBuilder.Entity<Character>()
            .HasIndex(c => c.CorporationId);

        // Character Skills indexes
        modelBuilder.Entity<CharacterSkill>()
            .HasIndex(cs => new { cs.CharacterId, cs.SkillTypeId })
            .IsUnique();

        modelBuilder.Entity<CharacterSkill>()
            .HasIndex(cs => cs.SkillTypeId);

        // Skill Plans indexes
        modelBuilder.Entity<SkillPlan>()
            .HasIndex(sp => sp.CharacterId);

        modelBuilder.Entity<SkillPlan>()
            .HasIndex(sp => sp.CreatedDate);

        // Ship Fittings indexes
        modelBuilder.Entity<ShipFitting>()
            .HasIndex(sf => sf.ShipTypeId);

        modelBuilder.Entity<ShipFitting>()
            .HasIndex(sf => sf.CharacterId);

        modelBuilder.Entity<ShipFitting>()
            .HasIndex(sf => sf.CreatedDate);

        // Market Data indexes
        modelBuilder.Entity<MarketData>()
            .HasIndex(md => new { md.TypeId, md.RegionId, md.RecordedDate });

        modelBuilder.Entity<MarketData>()
            .HasIndex(md => md.RecordedDate);

        // Historical Market Data indexes
        modelBuilder.Entity<HistoricalMarketData>()
            .HasIndex(hmd => new { hmd.TypeId, hmd.RegionId, hmd.Date, hmd.Interval })
            .IsUnique();

        modelBuilder.Entity<HistoricalMarketData>()
            .HasIndex(hmd => new { hmd.TypeId, hmd.RegionId, hmd.Date });

        modelBuilder.Entity<HistoricalMarketData>()
            .HasIndex(hmd => hmd.Date);

        // Personal Market Order indexes
        modelBuilder.Entity<PersonalMarketOrder>()
            .HasIndex(pmo => pmo.OrderId)
            .IsUnique();

        modelBuilder.Entity<PersonalMarketOrder>()
            .HasIndex(pmo => new { pmo.CharacterId, pmo.IsActive });

        modelBuilder.Entity<PersonalMarketOrder>()
            .HasIndex(pmo => new { pmo.TypeId, pmo.RegionId });

        modelBuilder.Entity<PersonalMarketOrder>()
            .HasIndex(pmo => pmo.State);

        modelBuilder.Entity<PersonalMarketOrder>()
            .HasIndex(pmo => pmo.ExpiryDate);

        // Order History indexes
        modelBuilder.Entity<OrderHistory>()
            .HasIndex(oh => oh.PersonalMarketOrderId);

        modelBuilder.Entity<OrderHistory>()
            .HasIndex(oh => oh.Timestamp);

        // Market Order Portfolio indexes
        modelBuilder.Entity<MarketOrderPortfolio>()
            .HasIndex(mop => mop.CharacterId)
            .IsUnique();

        // Market Transaction indexes
        modelBuilder.Entity<MarketTransaction>()
            .HasIndex(mt => mt.TransactionId)
            .IsUnique();

        modelBuilder.Entity<MarketTransaction>()
            .HasIndex(mt => new { mt.CharacterId, mt.TransactionDate });

        modelBuilder.Entity<MarketTransaction>()
            .HasIndex(mt => new { mt.TypeId, mt.RegionId });

        modelBuilder.Entity<MarketTransaction>()
            .HasIndex(mt => mt.TransactionDate);

        modelBuilder.Entity<MarketTransaction>()
            .HasIndex(mt => mt.RelatedOrderId);

        // Transaction Sync Status indexes
        modelBuilder.Entity<TransactionSyncStatus>()
            .HasIndex(tss => tss.CharacterId)
            .IsUnique();

        modelBuilder.Entity<TransactionSyncStatus>()
            .HasIndex(tss => tss.LastSyncDate);

        // Transaction Analytics indexes
        modelBuilder.Entity<TransactionAnalytics>()
            .HasIndex(ta => new { ta.CharacterId, ta.PeriodStart, ta.PeriodEnd });

        modelBuilder.Entity<TransactionAnalytics>()
            .HasIndex(ta => ta.AnalysisDate);

        // EVE Market Group indexes
        modelBuilder.Entity<EveMarketGroup>()
            .HasIndex(mg => mg.MarketGroupId)
            .IsUnique();

        modelBuilder.Entity<EveMarketGroup>()
            .HasIndex(mg => mg.ParentGroupId);

        modelBuilder.Entity<EveMarketGroup>()
            .HasIndex(mg => mg.MarketGroupName);

        // EVE Type indexes for market groups
        modelBuilder.Entity<EveType>()
            .HasIndex(t => t.MarketGroupId);

        // Regional Market Data indexes
        modelBuilder.Entity<TradeHub>()
            .HasIndex(th => th.RegionId);

        modelBuilder.Entity<TradeHub>()
            .HasIndex(th => th.SolarSystemId);

        modelBuilder.Entity<TradeHub>()
            .HasIndex(th => new { th.Tier, th.IsActive });

        modelBuilder.Entity<RegionalMarketCoverage>()
            .HasIndex(rmc => new { rmc.RegionId, rmc.MarketGroupId })
            .IsUnique();

        modelBuilder.Entity<RegionalMarketCoverage>()
            .HasIndex(rmc => rmc.NextSyncDue);

        modelBuilder.Entity<TradeRoute>()
            .HasIndex(tr => new { tr.OriginHubId, tr.DestinationHubId })
            .IsUnique();

        modelBuilder.Entity<TradeRoute>()
            .HasIndex(tr => tr.RiskLevel);

        modelBuilder.Entity<RouteItemProfitability>()
            .HasIndex(rip => new { rip.RouteId, rip.TypeId })
            .IsUnique();

        modelBuilder.Entity<RouteItemProfitability>()
            .HasIndex(rip => rip.ProfitMargin);

        modelBuilder.Entity<MarketSyncJob>()
            .HasIndex(msj => new { msj.Status, msj.ScheduledTime });

        modelBuilder.Entity<MarketSyncJob>()
            .HasIndex(msj => msj.JobType);

        modelBuilder.Entity<RegionalMarketStatistics>()
            .HasIndex(rms => new { rms.RegionId, rms.StatisticsDate })
            .IsUnique();

        // Market Data Cache indexes (TASK-177)
        modelBuilder.Entity<MarketDataCacheEntry>()
            .HasIndex(mdc => mdc.CacheKey)
            .IsUnique();

        modelBuilder.Entity<MarketDataCacheEntry>()
            .HasIndex(mdc => new { mdc.DataType, mdc.RegionId });

        modelBuilder.Entity<MarketDataCacheEntry>()
            .HasIndex(mdc => new { mdc.Status, mdc.ExpiryDate });

        modelBuilder.Entity<MarketDataCacheEntry>()
            .HasIndex(mdc => new { mdc.Priority, mdc.LastAccessDate });

        modelBuilder.Entity<MarketDataCacheEntry>()
            .HasIndex(mdc => mdc.TypeId);

        modelBuilder.Entity<MarketDataCacheEntry>()
            .HasIndex(mdc => mdc.LocationId);

        modelBuilder.Entity<CacheRefreshSchedule>()
            .HasIndex(crs => new { crs.DataType, crs.NextRefreshDue });

        modelBuilder.Entity<CacheRefreshSchedule>()
            .HasIndex(crs => new { crs.IsEnabled, crs.Priority });

        modelBuilder.Entity<CachePerformanceMetrics>()
            .HasIndex(cpm => new { cpm.MetricType, cpm.MetricDate });

        modelBuilder.Entity<CachePerformanceMetrics>()
            .HasIndex(cpm => cpm.DataType);

        modelBuilder.Entity<CacheInvalidationLog>()
            .HasIndex(cil => cil.InvalidationDate);

        modelBuilder.Entity<CacheInvalidationLog>()
            .HasIndex(cil => new { cil.DataType, cil.Reason });

        modelBuilder.Entity<CacheConfiguration>()
            .HasIndex(cc => cc.ConfigurationKey)
            .IsUnique();

        modelBuilder.Entity<CacheConfiguration>()
            .HasIndex(cc => cc.AppliesTo);

        // Market Trend Analysis indexes (TASK-178)
        modelBuilder.Entity<MarketTrendAnalysis>()
            .HasIndex(mta => new { mta.TypeId, mta.RegionId, mta.AnalysisDate })
            .IsUnique();

        modelBuilder.Entity<MarketTrendAnalysis>()
            .HasIndex(mta => new { mta.PriceTrend, mta.RegionId });

        modelBuilder.Entity<MarketTrendAnalysis>()
            .HasIndex(mta => new { mta.MarketRegime, mta.AnalysisDate });

        modelBuilder.Entity<MarketTrendAnalysis>()
            .HasIndex(mta => mta.QualityScore);

        modelBuilder.Entity<MarketTrendAnalysis>()
            .HasIndex(mta => new { mta.HasAnomalies, mta.AnomalyScore });

        modelBuilder.Entity<MarketPrediction>()
            .HasIndex(mp => new { mp.TypeId, mp.RegionId, mp.TargetDate });

        modelBuilder.Entity<MarketPrediction>()
            .HasIndex(mp => new { mp.Algorithm, mp.IsActive });

        modelBuilder.Entity<MarketPrediction>()
            .HasIndex(mp => new { mp.Signal, mp.SignalStrength });

        modelBuilder.Entity<MarketPrediction>()
            .HasIndex(mp => mp.ExpiryDate);

        modelBuilder.Entity<MarketPrediction>()
            .HasIndex(mp => new { mp.IsValidated, mp.ValidationDate });

        modelBuilder.Entity<TrendSignal>()
            .HasIndex(ts => new { ts.TypeId, ts.RegionId, ts.SignalDate });

        modelBuilder.Entity<TrendSignal>()
            .HasIndex(ts => new { ts.SignalType, ts.IsActive });

        modelBuilder.Entity<TrendSignal>()
            .HasIndex(ts => new { ts.Trigger, ts.Timeframe });

        modelBuilder.Entity<TrendSignal>()
            .HasIndex(ts => new { ts.Priority, ts.RequiresAction });

        modelBuilder.Entity<TrendSignal>()
            .HasIndex(ts => ts.ExpiryDate);

        modelBuilder.Entity<SeasonalPattern>()
            .HasIndex(sp => new { sp.TypeId, sp.RegionId, sp.Period });

        modelBuilder.Entity<SeasonalPattern>()
            .HasIndex(sp => new { sp.StartDay, sp.EndDay });

        modelBuilder.Entity<SeasonalPattern>()
            .HasIndex(sp => new { sp.IsActive, sp.Reliability });

        modelBuilder.Entity<AlgorithmPerformance>()
            .HasIndex(ap => new { ap.Algorithm, ap.AlgorithmVersion });

        modelBuilder.Entity<AlgorithmPerformance>()
            .HasIndex(ap => new { ap.TestPeriodStart, ap.TestPeriodEnd });

        modelBuilder.Entity<AlgorithmPerformance>()
            .HasIndex(ap => ap.OverallAccuracy);

        // Performance indexes
        modelBuilder.Entity<AuditLog>()
            .HasIndex(al => al.Timestamp);

        modelBuilder.Entity<PerformanceMetric>()
            .HasIndex(pm => new { pm.MetricName, pm.Timestamp });

        modelBuilder.Entity<ErrorLog>()
            .HasIndex(el => el.Timestamp);
    }

    private void SeedInitialData(ModelBuilder modelBuilder)
    {
        // Seed default application settings
        modelBuilder.Entity<ApplicationSettings>().HasData(
            new ApplicationSettings
            {
                Id = 1,
                SettingKey = "DatabaseVersion",
                SettingValue = "1.0.0",
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            },
            new ApplicationSettings
            {
                Id = 2,
                SettingKey = "DefaultHolographicTheme",
                SettingValue = "ElectricBlue",
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            },
            new ApplicationSettings
            {
                Id = 3,
                SettingKey = "AutoBackupEnabled",
                SettingValue = "true",
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            },
            new ApplicationSettings
            {
                Id = 4,
                SettingKey = "ParticleEffectsEnabled",
                SettingValue = "true",
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            },
            new ApplicationSettings
            {
                Id = 5,
                SettingKey = "AnimationsEnabled",
                SettingValue = "true",
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            }
        );

        // Seed default holographic themes
        modelBuilder.Entity<HolographicTheme>().HasData(
            new HolographicTheme
            {
                Id = 1,
                ThemeName = "Electric Blue",
                PrimaryColor = "#00C8FF",
                SecondaryColor = "#0096C8",
                AccentColor = "#00FFFF",
                ParticleColor = "#80C8FF",
                IsDefault = true,
                CreatedDate = DateTime.UtcNow
            },
            new HolographicTheme
            {
                Id = 2,
                ThemeName = "Corporation Gold",
                PrimaryColor = "#FFD700",
                SecondaryColor = "#FFA500",
                AccentColor = "#FFFF00",
                ParticleColor = "#FFE55C",
                IsDefault = false,
                CreatedDate = DateTime.UtcNow
            },
            new HolographicTheme
            {
                Id = 3,
                ThemeName = "Alliance Green",
                PrimaryColor = "#00FF64",
                SecondaryColor = "#00C850",
                AccentColor = "#64FF96",
                ParticleColor = "#80FF80",
                IsDefault = false,
                CreatedDate = DateTime.UtcNow
            },
            new HolographicTheme
            {
                Id = 4,
                ThemeName = "CONCORD Red",
                PrimaryColor = "#FF3232",
                SecondaryColor = "#C82828",
                AccentColor = "#FF6464",
                ParticleColor = "#FF8080",
                IsDefault = false,
                CreatedDate = DateTime.UtcNow
            }
        );

        // Seed default user preferences
        modelBuilder.Entity<UserPreference>().HasData(
            new UserPreference
            {
                Id = 1,
                UserId = "default",
                PreferenceKey = "HolographicIntensity",
                PreferenceValue = "1.0",
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            },
            new UserPreference
            {
                Id = 2,
                UserId = "default",
                PreferenceKey = "DefaultCharacter",
                PreferenceValue = "",
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            },
            new UserPreference
            {
                Id = 3,
                UserId = "default",
                PreferenceKey = "AutoLoadCharacterData",
                PreferenceValue = "true",
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            },
            new UserPreference
            {
                Id = 4,
                UserId = "default",
                PreferenceKey = "EnableNotifications",
                PreferenceValue = "true",
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            },
            new UserPreference
            {
                Id = 5,
                UserId = "default",
                PreferenceKey = "DefaultMarketRegion",
                PreferenceValue = "10000002", // The Forge
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            }
        );

        // Seed major trade hubs
        modelBuilder.Entity<TradeHub>().HasData(
            new TradeHub
            {
                TradeHubId = 1,
                HubName = "Jita IV - Moon 4 - Caldari Navy Assembly Plant",
                RegionId = 10000002, // The Forge
                SolarSystemId = 30000142, // Jita
                StationId = 60003760,
                HubType = "Station",
                Tier = TradeHubTier.Primary,
                AverageDailyVolume = 1000000000m,
                ActiveTraders = 50000,
                MarketDepth = 0.95,
                LiquidityScore = 1.0,
                TransactionFeeRate = 0.01m,
                BrokerFeeRate = 0.03m,
                IsActive = true,
                IsPlayerOwned = false,
                IsAccessible = true,
                LastMarketUpdate = DateTime.UtcNow,
                LastAccessibilityCheck = DateTime.UtcNow
            },
            new TradeHub
            {
                TradeHubId = 2,
                HubName = "Amarr VIII (Oris) - Emperor Family Academy",
                RegionId = 10000043, // Domain
                SolarSystemId = 30002187, // Amarr
                StationId = 60008494,
                HubType = "Station",
                Tier = TradeHubTier.Primary,
                AverageDailyVolume = 200000000m,
                ActiveTraders = 15000,
                MarketDepth = 0.85,
                LiquidityScore = 0.8,
                TransactionFeeRate = 0.01m,
                BrokerFeeRate = 0.03m,
                IsActive = true,
                IsPlayerOwned = false,
                IsAccessible = true,
                LastMarketUpdate = DateTime.UtcNow,
                LastAccessibilityCheck = DateTime.UtcNow
            },
            new TradeHub
            {
                TradeHubId = 3,
                HubName = "Dodixie IX - Moon 20 - Federation Navy Assembly Plant",
                RegionId = 10000032, // Sinq Laison
                SolarSystemId = 30002659, // Dodixie
                StationId = 60011866,
                HubType = "Station",
                Tier = TradeHubTier.Primary,
                AverageDailyVolume = 150000000m,
                ActiveTraders = 12000,
                MarketDepth = 0.75,
                LiquidityScore = 0.7,
                TransactionFeeRate = 0.01m,
                BrokerFeeRate = 0.03m,
                IsActive = true,
                IsPlayerOwned = false,
                IsAccessible = true,
                LastMarketUpdate = DateTime.UtcNow,
                LastAccessibilityCheck = DateTime.UtcNow
            },
            new TradeHub
            {
                TradeHubId = 4,
                HubName = "Rens VI - Moon 8 - Brutor Tribe Treasury",
                RegionId = 10000030, // Heimatar
                SolarSystemId = 30002510, // Rens
                StationId = 60004588,
                HubType = "Station",
                Tier = TradeHubTier.Primary,
                AverageDailyVolume = 100000000m,
                ActiveTraders = 8000,
                MarketDepth = 0.65,
                LiquidityScore = 0.6,
                TransactionFeeRate = 0.01m,
                BrokerFeeRate = 0.03m,
                IsActive = true,
                IsPlayerOwned = false,
                IsAccessible = true,
                LastMarketUpdate = DateTime.UtcNow,
                LastAccessibilityCheck = DateTime.UtcNow
            },
            new TradeHub
            {
                TradeHubId = 5,
                HubName = "Hek VIII - Moon 12 - Boundless Creation Factory",
                RegionId = 10000030, // Heimatar
                SolarSystemId = 30002053, // Hek
                StationId = 60005686,
                HubType = "Station",
                Tier = TradeHubTier.Secondary,
                AverageDailyVolume = 50000000m,
                ActiveTraders = 4000,
                MarketDepth = 0.55,
                LiquidityScore = 0.5,
                TransactionFeeRate = 0.01m,
                BrokerFeeRate = 0.03m,
                IsActive = true,
                IsPlayerOwned = false,
                IsAccessible = true,
                LastMarketUpdate = DateTime.UtcNow,
                LastAccessibilityCheck = DateTime.UtcNow
            }
        );
    }

    #endregion

    #region Database Management

    public async Task<bool> EnsureDatabaseCreatedAsync()
    {
        try
        {
            return await Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error creating database: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> MigrateDatabaseAsync()
    {
        try
        {
            await Database.MigrateAsync();
            return true;
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error migrating database: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteDatabaseAsync()
    {
        try
        {
            return await Database.EnsureDeletedAsync();
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error deleting database: {ex.Message}");
            return false;
        }
    }

    public async Task<long> GetDatabaseSizeAsync()
    {
        try
        {
            var databasePath = GetDatabasePath();
            if (File.Exists(databasePath))
            {
                var fileInfo = new FileInfo(databasePath);
                return fileInfo.Length;
            }
            return 0;
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error getting database size: {ex.Message}");
            return 0;
        }
    }

    public async Task<bool> VacuumDatabaseAsync()
    {
        try
        {
            await Database.ExecuteSqlRawAsync("VACUUM;");
            return true;
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error vacuuming database: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> BackupDatabaseAsync(string backupPath)
    {
        try
        {
            var sourcePath = GetDatabasePath();
            if (File.Exists(sourcePath))
            {
                await using var source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                await using var destination = new FileStream(backupPath, FileMode.Create, FileAccess.Write);
                await source.CopyToAsync(destination);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error backing up database: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> RestoreDatabaseAsync(string backupPath)
    {
        try
        {
            var targetPath = GetDatabasePath();
            if (File.Exists(backupPath))
            {
                // Close all connections
                await Database.CloseConnectionAsync();

                // Copy backup to target location
                await using var source = new FileStream(backupPath, FileMode.Open, FileAccess.Read);
                await using var destination = new FileStream(targetPath, FileMode.Create, FileAccess.Write);
                await source.CopyToAsync(destination);

                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error restoring database: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Performance Monitoring

    public async Task LogPerformanceMetricAsync(string metricName, double value, string? category = null)
    {
        try
        {
            var metric = new PerformanceMetric
            {
                MetricName = metricName,
                MetricValue = value,
                Category = category ?? "General",
                Timestamp = DateTime.UtcNow
            };

            PerformanceMetrics.Add(metric);
            await SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error logging performance metric: {ex.Message}");
        }
    }

    public async Task LogErrorAsync(string errorMessage, string? stackTrace = null, string? source = null)
    {
        try
        {
            var errorLog = new ErrorLog
            {
                ErrorMessage = errorMessage,
                StackTrace = stackTrace,
                Source = source,
                Timestamp = DateTime.UtcNow
            };

            ErrorLogs.Add(errorLog);
            await SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log error to console as fallback
            Console.WriteLine($"Error logging to database: {ex.Message}");
            Console.WriteLine($"Original error: {errorMessage}");
        }
    }

    public async Task LogAuditAsync(string action, string? entityType = null, string? entityId = null, string? userId = null)
    {
        try
        {
            var auditLog = new AuditLog
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                UserId = userId,
                Timestamp = DateTime.UtcNow
            };

            AuditLogs.Add(auditLog);
            await SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error logging audit entry: {ex.Message}");
        }
    }

    #endregion

    #region Helper Methods

    private static string GetDatabasePath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var gideonDataPath = Path.Combine(appDataPath, "Gideon");
        
        // Ensure directory exists
        Directory.CreateDirectory(gideonDataPath);
        
        return Path.Combine(gideonDataPath, "gideon.db");
    }

    public static string GetConnectionString()
    {
        var databasePath = GetDatabasePath();
        return $"Data Source={databasePath}";
    }

    #endregion

    #region Cleanup

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Perform any cleanup
        }
        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        // Perform any async cleanup
        await base.DisposeAsync();
    }

    #endregion
}