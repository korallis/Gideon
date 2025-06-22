// ==========================================================================
// UnitOfWork.cs - Unit of Work Implementation
// ==========================================================================
// Implementation of Unit of Work pattern managing all repositories and transactions.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Core.Domain.Interfaces;
using Gideon.WPF.Core.Infrastructure.Data;

namespace Gideon.WPF.Core.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation managing all repositories and transactions
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly GideonDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed = false;

    // Specialized repositories
    private ICharacterRepository? _characters;
    private IShipFittingRepository? _shipFittings;
    private IMarketDataRepository? _marketData;
    private IPersonalMarketOrderRepository? _personalMarketOrders;
    private ITransactionRepository? _transactions;
    private IMarketGroupRepository? _marketGroups;
    private IRegionalMarketRepository? _regionalMarket;
    private IMarketDataCacheRepository? _marketDataCacheRepository;

    // Generic repositories
    private IRepository<Ship>? _ships;
    private IRepository<Module>? _modules;
    private IRepository<SkillPlan>? _skillPlans;
    private IRepository<SkillPlanEntry>? _skillPlanEntries;
    private IRepository<CharacterSkill>? _characterSkills;
    private IRepository<CharacterAsset>? _characterAssets;
    private IRepository<CharacterLocation>? _characterLocations;
    private IRepository<CharacterGoal>? _characterGoals;
    private IRepository<CharacterStatistic>? _characterStatistics;
    private IRepository<FittingModule>? _fittingModules;

    // Market and Trading repositories
    private IRepository<MarketPrediction>? _marketPredictions;
    private IRepository<MarketRegion>? _marketRegions;
    private IRepository<TradingOpportunity>? _tradingOpportunities;
    private IRepository<MarketTransaction>? _marketTransactions;
    private IRepository<HistoricalMarketData>? _historicalMarketData;
    private IRepository<OrderHistory>? _orderHistories;
    private IRepository<MarketOrderPortfolio>? _marketOrderPortfolios;
    private IRepository<TransactionSyncStatus>? _transactionSyncStatuses;
    private IRepository<TransactionAnalytics>? _transactionAnalytics;
    private IRepository<TradeHub>? _tradeHubs;
    private IRepository<TradeRoute>? _tradeRoutes;
    private IRepository<RouteItemProfitability>? _routeItemProfitability;
    private IRepository<MarketSyncJob>? _marketSyncJobs;
    private IRepository<RegionalMarketStatistics>? _regionalMarketStatistics;

    // Market Data Cache repositories (TASK-177)
    private IRepository<MarketDataCacheEntry>? _marketDataCache;
    private IRepository<CacheStatistics>? _cacheStatistics;
    private IRepository<CacheRefreshSchedule>? _cacheRefreshSchedule;
    private IRepository<CachePerformanceMetrics>? _cachePerformanceMetrics;
    private IRepository<CacheInvalidationLog>? _cacheInvalidationLog;
    private IRepository<CacheConfiguration>? _cacheConfiguration;

    // Market Trend Analysis repositories (TASK-178)
    private IMarketTrendRepository? _marketTrendAnalyses;
    private IMarketPredictionRepository? _marketPredictions;
    private ITrendSignalRepository? _trendSignals;
    private ISeasonalPatternRepository? _seasonalPatterns;
    private IRepository<AlgorithmPerformance>? _algorithmPerformance;

    // EVE Static Data repositories
    private IRepository<EveType>? _eveTypes;
    private IRepository<EveGroup>? _eveGroups;
    private IRepository<EveCategory>? _eveCategories;
    private IRepository<EveMarketGroup>? _eveMarketGroups;
    private IRepository<EveRegion>? _eveRegions;
    private IRepository<EveSolarSystem>? _eveSolarSystems;
    private IRepository<EveStation>? _eveStations;

    // Application Management repositories
    private IRepository<ApplicationSettings>? _applicationSettings;
    private IRepository<UserPreference>? _userPreferences;
    private IRepository<HolographicTheme>? _holographicThemes;
    private IRepository<BackupEntry>? _backupEntries;
    private IRepository<SharedCharacterData>? _sharedCharacterData;
    private IRepository<AuditLog>? _auditLogs;
    private IRepository<PerformanceMetric>? _performanceMetrics;
    private IRepository<ErrorLog>? _errorLogs;

    public UnitOfWork(GideonDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    #region Specialized Repository Properties

    public ICharacterRepository Characters => 
        _characters ??= new CharacterRepository(_context);

    public IShipFittingRepository ShipFittings => 
        _shipFittings ??= new ShipFittingRepository(_context);

    public IMarketDataRepository MarketData => 
        _marketData ??= new MarketDataRepository(_context);

    public IPersonalMarketOrderRepository PersonalMarketOrders => 
        _personalMarketOrders ??= new PersonalMarketOrderRepository(_context);

    public ITransactionRepository Transactions => 
        _transactions ??= new TransactionRepository(_context);

    public IMarketGroupRepository MarketGroups => 
        _marketGroups ??= new MarketGroupRepository(_context);

    public IRegionalMarketRepository RegionalMarket => 
        _regionalMarket ??= new RegionalMarketRepository(_context);

    public IMarketDataCacheRepository MarketDataCacheRepository => 
        _marketDataCacheRepository ??= new MarketDataCacheRepository(_context);

    #endregion

    #region Generic Repository Properties

    public IRepository<Ship> Ships => 
        _ships ??= new Repository<Ship>(_context);

    public IRepository<Module> Modules => 
        _modules ??= new Repository<Module>(_context);

    public IRepository<SkillPlan> SkillPlans => 
        _skillPlans ??= new Repository<SkillPlan>(_context);

    public IRepository<SkillPlanEntry> SkillPlanEntries => 
        _skillPlanEntries ??= new Repository<SkillPlanEntry>(_context);

    public IRepository<CharacterSkill> CharacterSkills => 
        _characterSkills ??= new Repository<CharacterSkill>(_context);

    public IRepository<CharacterAsset> CharacterAssets => 
        _characterAssets ??= new Repository<CharacterAsset>(_context);

    public IRepository<CharacterLocation> CharacterLocations => 
        _characterLocations ??= new Repository<CharacterLocation>(_context);

    public IRepository<CharacterGoal> CharacterGoals => 
        _characterGoals ??= new Repository<CharacterGoal>(_context);

    public IRepository<CharacterStatistic> CharacterStatistics => 
        _characterStatistics ??= new Repository<CharacterStatistic>(_context);

    public IRepository<FittingModule> FittingModules => 
        _fittingModules ??= new Repository<FittingModule>(_context);

    #endregion

    #region Market and Trading Repository Properties

    public IRepository<MarketPrediction> MarketPredictions => 
        _marketPredictions ??= new Repository<MarketPrediction>(_context);

    public IRepository<MarketRegion> MarketRegions => 
        _marketRegions ??= new Repository<MarketRegion>(_context);

    public IRepository<TradingOpportunity> TradingOpportunities => 
        _tradingOpportunities ??= new Repository<TradingOpportunity>(_context);

    public IRepository<MarketTransaction> MarketTransactions => 
        _marketTransactions ??= new Repository<MarketTransaction>(_context);

    public IRepository<HistoricalMarketData> HistoricalMarketData => 
        _historicalMarketData ??= new Repository<HistoricalMarketData>(_context);

    public IRepository<OrderHistory> OrderHistories => 
        _orderHistories ??= new Repository<OrderHistory>(_context);

    public IRepository<MarketOrderPortfolio> MarketOrderPortfolios => 
        _marketOrderPortfolios ??= new Repository<MarketOrderPortfolio>(_context);

    public IRepository<TransactionSyncStatus> TransactionSyncStatuses => 
        _transactionSyncStatuses ??= new Repository<TransactionSyncStatus>(_context);

    public IRepository<TransactionAnalytics> TransactionAnalytics => 
        _transactionAnalytics ??= new Repository<TransactionAnalytics>(_context);

    public IRepository<TradeHub> TradeHubs => 
        _tradeHubs ??= new Repository<TradeHub>(_context);

    public IRepository<TradeRoute> TradeRoutes => 
        _tradeRoutes ??= new Repository<TradeRoute>(_context);

    public IRepository<RouteItemProfitability> RouteItemProfitability => 
        _routeItemProfitability ??= new Repository<RouteItemProfitability>(_context);

    public IRepository<MarketSyncJob> MarketSyncJobs => 
        _marketSyncJobs ??= new Repository<MarketSyncJob>(_context);

    public IRepository<RegionalMarketStatistics> RegionalMarketStatistics => 
        _regionalMarketStatistics ??= new Repository<RegionalMarketStatistics>(_context);

    #endregion

    #region Market Data Cache Repository Properties (TASK-177)

    public IRepository<MarketDataCacheEntry> MarketDataCache => 
        _marketDataCache ??= new Repository<MarketDataCacheEntry>(_context);

    public IRepository<CacheStatistics> CacheStatistics => 
        _cacheStatistics ??= new Repository<CacheStatistics>(_context);

    public IRepository<CacheRefreshSchedule> CacheRefreshSchedule => 
        _cacheRefreshSchedule ??= new Repository<CacheRefreshSchedule>(_context);

    public IRepository<CachePerformanceMetrics> CachePerformanceMetrics => 
        _cachePerformanceMetrics ??= new Repository<CachePerformanceMetrics>(_context);

    public IRepository<CacheInvalidationLog> CacheInvalidationLog => 
        _cacheInvalidationLog ??= new Repository<CacheInvalidationLog>(_context);

    public IRepository<CacheConfiguration> CacheConfiguration => 
        _cacheConfiguration ??= new Repository<CacheConfiguration>(_context);

    #endregion

    #region Market Trend Analysis Repository Properties (TASK-178)

    public IMarketTrendRepository MarketTrendAnalyses => 
        _marketTrendAnalyses ??= new MarketTrendRepository(_context);

    public IMarketPredictionRepository MarketPredictions => 
        _marketPredictions ??= new MarketPredictionRepository(_context);

    public ITrendSignalRepository TrendSignals => 
        _trendSignals ??= new TrendSignalRepository(_context);

    public ISeasonalPatternRepository SeasonalPatterns => 
        _seasonalPatterns ??= new SeasonalPatternRepository(_context);

    public IRepository<AlgorithmPerformance> AlgorithmPerformance => 
        _algorithmPerformance ??= new Repository<AlgorithmPerformance>(_context);

    #endregion

    #region EVE Static Data Repository Properties

    public IRepository<EveType> EveTypes => 
        _eveTypes ??= new Repository<EveType>(_context);

    public IRepository<EveGroup> EveGroups => 
        _eveGroups ??= new Repository<EveGroup>(_context);

    public IRepository<EveCategory> EveCategories => 
        _eveCategories ??= new Repository<EveCategory>(_context);

    public IRepository<EveMarketGroup> EveMarketGroups => 
        _eveMarketGroups ??= new Repository<EveMarketGroup>(_context);

    public IRepository<EveRegion> EveRegions => 
        _eveRegions ??= new Repository<EveRegion>(_context);

    public IRepository<EveSolarSystem> EveSolarSystems => 
        _eveSolarSystems ??= new Repository<EveSolarSystem>(_context);

    public IRepository<EveStation> EveStations => 
        _eveStations ??= new Repository<EveStation>(_context);

    #endregion

    #region Application Management Repository Properties

    public IRepository<ApplicationSettings> ApplicationSettings => 
        _applicationSettings ??= new Repository<ApplicationSettings>(_context);

    public IRepository<UserPreference> UserPreferences => 
        _userPreferences ??= new Repository<UserPreference>(_context);

    public IRepository<HolographicTheme> HolographicThemes => 
        _holographicThemes ??= new Repository<HolographicTheme>(_context);

    public IRepository<BackupEntry> BackupEntries => 
        _backupEntries ??= new Repository<BackupEntry>(_context);

    public IRepository<SharedCharacterData> SharedCharacterData => 
        _sharedCharacterData ??= new Repository<SharedCharacterData>(_context);

    public IRepository<AuditLog> AuditLogs => 
        _auditLogs ??= new Repository<AuditLog>(_context);

    public IRepository<PerformanceMetric> PerformanceMetrics => 
        _performanceMetrics ??= new Repository<PerformanceMetric>(_context);

    public IRepository<ErrorLog> ErrorLogs => 
        _errorLogs ??= new Repository<ErrorLog>(_context);

    #endregion

    #region Transaction Management

    /// <summary>
    /// Save all changes to the database
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Log concurrency conflict
            await LogErrorAsync("Concurrency conflict during save", ex);
            throw;
        }
        catch (DbUpdateException ex)
        {
            // Log database update error
            await LogErrorAsync("Database update error", ex);
            throw;
        }
    }

    /// <summary>
    /// Begin a database transaction
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
            throw new InvalidOperationException("Transaction already started");

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Commit the current transaction
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("No transaction started");

        try
        {
            await SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>
    /// Rollback the current transaction
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("No transaction started");

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    #endregion

    #region Bulk Operations

    /// <summary>
    /// Execute raw SQL command
    /// </summary>
    public async Task<int> ExecuteSqlAsync(string sql, params object[] parameters)
    {
        return await _context.Database.ExecuteSqlRawAsync(sql, parameters);
    }

    /// <summary>
    /// Execute raw SQL command with cancellation token
    /// </summary>
    public async Task<int> ExecuteSqlAsync(string sql, CancellationToken cancellationToken, params object[] parameters)
    {
        return await _context.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Log error to the error log repository
    /// </summary>
    private async Task LogErrorAsync(string message, Exception exception)
    {
        try
        {
            var errorLog = new ErrorLog
            {
                ErrorMessage = message,
                StackTrace = exception.StackTrace,
                Source = exception.Source,
                Severity = "Error",
                Timestamp = DateTime.UtcNow,
                IsResolved = false
            };

            await ErrorLogs.AddAsync(errorLog);
            await SaveChangesAsync();
        }
        catch
        {
            // Ignore logging errors to prevent infinite loops
        }
    }

    #endregion

    #region IDisposable Implementation

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}