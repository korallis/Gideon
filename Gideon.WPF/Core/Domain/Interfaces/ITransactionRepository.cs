// ==========================================================================
// ITransactionRepository.cs - Transaction Repository Interface
// ==========================================================================
// Specialized repository interface for managing market transactions and history.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Gideon.WPF.Core.Domain.Entities;

namespace Gideon.WPF.Core.Domain.Interfaces;

/// <summary>
/// Repository interface for transaction operations
/// </summary>
public interface ITransactionRepository : IRepository<MarketTransaction>
{
    /// <summary>
    /// Get transactions for a character within date range
    /// </summary>
    Task<IEnumerable<MarketTransaction>> GetTransactionsByCharacterAsync(
        int characterId, 
        DateTime? fromDate = null, 
        DateTime? toDate = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get transaction by ESI transaction ID
    /// </summary>
    Task<MarketTransaction?> GetByTransactionIdAsync(long transactionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get transactions for specific item type
    /// </summary>
    Task<IEnumerable<MarketTransaction>> GetTransactionsByTypeAsync(
        int characterId, 
        int typeId, 
        DateTime? fromDate = null, 
        DateTime? toDate = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get transactions by location
    /// </summary>
    Task<IEnumerable<MarketTransaction>> GetTransactionsByLocationAsync(
        int characterId, 
        long locationId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recent transactions (last N transactions)
    /// </summary>
    Task<IEnumerable<MarketTransaction>> GetRecentTransactionsAsync(
        int characterId, 
        int count = 50, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk insert transactions from ESI sync
    /// </summary>
    Task BulkInsertTransactionsAsync(IEnumerable<MarketTransaction> transactions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get transaction statistics for period
    /// </summary>
    Task<TransactionStatistics> GetTransactionStatisticsAsync(
        int characterId, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get top traded items by volume
    /// </summary>
    Task<IEnumerable<ItemTradingStats>> GetTopTradedItemsAsync(
        int characterId, 
        DateTime fromDate, 
        DateTime toDate, 
        int count = 10, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get top traded items by profit
    /// </summary>
    Task<IEnumerable<ItemTradingStats>> GetMostProfitableItemsAsync(
        int characterId, 
        DateTime fromDate, 
        DateTime toDate, 
        int count = 10, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get transactions related to specific personal order
    /// </summary>
    Task<IEnumerable<MarketTransaction>> GetTransactionsByOrderIdAsync(
        long relatedOrderId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get transaction sync status for character
    /// </summary>
    Task<TransactionSyncStatus?> GetSyncStatusAsync(int characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update transaction sync status
    /// </summary>
    Task UpsertSyncStatusAsync(TransactionSyncStatus syncStatus, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get or create transaction analytics for period
    /// </summary>
    Task<TransactionAnalytics> GetOrCreateAnalyticsAsync(
        int characterId, 
        DateTime periodStart, 
        DateTime periodEnd, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update transaction analytics
    /// </summary>
    Task UpsertAnalyticsAsync(TransactionAnalytics analytics, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get latest analytics for character
    /// </summary>
    Task<TransactionAnalytics?> GetLatestAnalyticsAsync(int characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get profit/loss analysis for time period
    /// </summary>
    Task<ProfitLossAnalysis> GetProfitLossAnalysisAsync(
        int characterId, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get transaction performance metrics
    /// </summary>
    Task<TransactionPerformance> GetPerformanceMetricsAsync(
        int characterId, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clean up old transactions based on retention policy
    /// </summary>
    Task CleanupOldTransactionsAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);
}

/// <summary>
/// Transaction statistics summary
/// </summary>
public class TransactionStatistics
{
    public int CharacterId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    
    public int TotalTransactions { get; set; }
    public int BuyTransactions { get; set; }
    public int SellTransactions { get; set; }
    
    public decimal TotalValue { get; set; }
    public decimal TotalBuyValue { get; set; }
    public decimal TotalSellValue { get; set; }
    public decimal NetValue { get; set; }
    
    public decimal TotalTax { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal TotalFees => TotalTax + TotalCommission;
    
    public decimal AverageTransactionValue { get; set; }
    public decimal LargestTransaction { get; set; }
    public decimal SmallestTransaction { get; set; }
    
    public int UniqueItems { get; set; }
    public int UniqueLocations { get; set; }
    
    public TimeSpan TradingPeriod => ToDate - FromDate;
    public double TransactionsPerDay => TradingPeriod.TotalDays > 0 ? TotalTransactions / TradingPeriod.TotalDays : 0;
}

/// <summary>
/// Item trading statistics
/// </summary>
public class ItemTradingStats
{
    public int TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public long TotalQuantity { get; set; }
    public decimal TotalValue { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal Profit { get; set; }
    public double ProfitMargin { get; set; }
    public int TransactionCount { get; set; }
}

/// <summary>
/// Profit/Loss analysis result
/// </summary>
public class ProfitLossAnalysis
{
    public int CharacterId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    
    public decimal TotalProfit { get; set; }
    public decimal TotalLoss { get; set; }
    public decimal NetProfitLoss { get; set; }
    
    public decimal GrossRevenue { get; set; }
    public decimal GrossCosts { get; set; }
    public decimal TotalFees { get; set; }
    
    public double ProfitMargin { get; set; }
    public double ROI { get; set; }
    
    public List<ItemProfitLoss> ItemBreakdown { get; set; } = new();
    public List<LocationProfitLoss> LocationBreakdown { get; set; } = new();
    public List<DailyProfitLoss> DailyBreakdown { get; set; } = new();
}

/// <summary>
/// Item-specific profit/loss
/// </summary>
public class ItemProfitLoss
{
    public int TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public decimal Profit { get; set; }
    public decimal Revenue { get; set; }
    public decimal Costs { get; set; }
    public double Margin { get; set; }
    public long Volume { get; set; }
}

/// <summary>
/// Location-specific profit/loss
/// </summary>
public class LocationProfitLoss
{
    public long LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public decimal Profit { get; set; }
    public decimal Revenue { get; set; }
    public decimal Costs { get; set; }
    public int TransactionCount { get; set; }
}

/// <summary>
/// Daily profit/loss breakdown
/// </summary>
public class DailyProfitLoss
{
    public DateTime Date { get; set; }
    public decimal Profit { get; set; }
    public decimal Revenue { get; set; }
    public decimal Costs { get; set; }
    public int TransactionCount { get; set; }
}

/// <summary>
/// Transaction performance metrics
/// </summary>
public class TransactionPerformance
{
    public int CharacterId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    
    public double VelocityScore { get; set; } // Transactions per day
    public double EfficiencyScore { get; set; } // Profit per transaction
    public double ConsistencyScore { get; set; } // Volatility of profits
    public double DiversificationScore { get; set; } // Number of different items/markets
    
    public TimeSpan AverageTimeBetweenTrades { get; set; }
    public DayOfWeek MostActiveDay { get; set; }
    public int MostActiveHour { get; set; }
    
    public decimal BestDayProfit { get; set; }
    public decimal WorstDayLoss { get; set; }
    public int ConsecutiveProfitableDays { get; set; }
    public int LongestProfitStreak { get; set; }
}