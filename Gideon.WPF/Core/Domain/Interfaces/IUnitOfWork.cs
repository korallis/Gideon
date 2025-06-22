using Gideon.WPF.Core.Domain.Entities;

namespace Gideon.WPF.Core.Domain.Interfaces;

/// <summary>
/// Unit of Work pattern interface for managing repository transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Specialized Repository properties
    ICharacterRepository Characters { get; }
    IShipFittingRepository ShipFittings { get; }
    IMarketDataRepository MarketData { get; }
    IPersonalMarketOrderRepository PersonalMarketOrders { get; }
    ITransactionRepository Transactions { get; }
    IMarketGroupRepository MarketGroups { get; }
    IRegionalMarketRepository RegionalMarket { get; }
    IMarketDataCacheRepository MarketDataCacheRepository { get; }
    
    // Generic Repository properties
    IRepository<Ship> Ships { get; }
    IRepository<Module> Modules { get; }
    IRepository<SkillPlan> SkillPlans { get; }
    IRepository<SkillPlanEntry> SkillPlanEntries { get; }
    IRepository<CharacterSkill> CharacterSkills { get; }
    IRepository<CharacterAsset> CharacterAssets { get; }
    IRepository<CharacterLocation> CharacterLocations { get; }
    IRepository<CharacterGoal> CharacterGoals { get; }
    IRepository<CharacterStatistic> CharacterStatistics { get; }
    IRepository<FittingModule> FittingModules { get; }
    
    // Market and Trading
    IRepository<MarketPrediction> MarketPredictions { get; }
    IRepository<MarketRegion> MarketRegions { get; }
    IRepository<TradingOpportunity> TradingOpportunities { get; }
    IRepository<MarketTransaction> MarketTransactions { get; }
    IRepository<HistoricalMarketData> HistoricalMarketData { get; }
    IRepository<OrderHistory> OrderHistories { get; }
    IRepository<MarketOrderPortfolio> MarketOrderPortfolios { get; }
    IRepository<TransactionSyncStatus> TransactionSyncStatuses { get; }
    IRepository<TransactionAnalytics> TransactionAnalytics { get; }
    IRepository<TradeHub> TradeHubs { get; }
    IRepository<TradeRoute> TradeRoutes { get; }
    IRepository<RouteItemProfitability> RouteItemProfitability { get; }
    IRepository<MarketSyncJob> MarketSyncJobs { get; }
    IRepository<RegionalMarketStatistics> RegionalMarketStatistics { get; }
    
    // Market Data Caching (TASK-177)
    IRepository<MarketDataCacheEntry> MarketDataCache { get; }
    IRepository<CacheStatistics> CacheStatistics { get; }
    IRepository<CacheRefreshSchedule> CacheRefreshSchedule { get; }
    IRepository<CachePerformanceMetrics> CachePerformanceMetrics { get; }
    IRepository<CacheInvalidationLog> CacheInvalidationLog { get; }
    IRepository<CacheConfiguration> CacheConfiguration { get; }
    
    // Market Trend Analysis (TASK-178)
    IMarketTrendRepository MarketTrendAnalyses { get; }
    IMarketPredictionRepository MarketPredictions { get; }
    ITrendSignalRepository TrendSignals { get; }
    ISeasonalPatternRepository SeasonalPatterns { get; }
    IRepository<AlgorithmPerformance> AlgorithmPerformance { get; }
    
    // EVE Static Data
    IRepository<EveType> EveTypes { get; }
    IRepository<EveGroup> EveGroups { get; }
    IRepository<EveCategory> EveCategories { get; }
    IRepository<EveMarketGroup> EveMarketGroups { get; }
    IRepository<EveRegion> EveRegions { get; }
    IRepository<EveSolarSystem> EveSolarSystems { get; }
    IRepository<EveStation> EveStations { get; }
    
    // EVE Dogma Attribute System (TASK-179)
    IRepository<EveDogmaAttribute> EveDogmaAttributes { get; }
    IRepository<EveTypeAttribute> EveTypeAttributes { get; }
    IRepository<EveDogmaEffect> EveDogmaEffects { get; }
    IRepository<EveTypeEffect> EveTypeEffects { get; }
    IRepository<EveWeaponType> EveWeaponTypes { get; }
    IRepository<EveAmmunitionType> EveAmmunitionTypes { get; }
    
    // Application Management
    IRepository<ApplicationSettings> ApplicationSettings { get; }
    IRepository<UserPreference> UserPreferences { get; }
    IRepository<HolographicTheme> HolographicThemes { get; }
    IRepository<BackupEntry> BackupEntries { get; }
    IRepository<SharedCharacterData> SharedCharacterData { get; }
    IRepository<AuditLog> AuditLogs { get; }
    IRepository<PerformanceMetric> PerformanceMetrics { get; }
    IRepository<ErrorLog> ErrorLogs { get; }
    
    // Transaction management
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    
    // Bulk operations
    Task<int> ExecuteSqlAsync(string sql, params object[] parameters);
    
    Task<int> ExecuteSqlAsync(string sql, CancellationToken cancellationToken, params object[] parameters);
}