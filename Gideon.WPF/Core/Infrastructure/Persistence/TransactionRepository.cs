// ==========================================================================
// TransactionRepository.cs - Transaction Repository Implementation
// ==========================================================================
// Concrete implementation of transaction repository with specialized operations.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Core.Domain.Interfaces;
using Gideon.WPF.Core.Infrastructure.Data;

namespace Gideon.WPF.Core.Infrastructure.Persistence;

/// <summary>
/// Transaction repository implementation with specialized operations
/// </summary>
public class TransactionRepository : Repository<MarketTransaction>, ITransactionRepository
{
    public TransactionRepository(GideonDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get transactions for a character within date range
    /// </summary>
    public async Task<IEnumerable<MarketTransaction>> GetTransactionsByCharacterAsync(
        int characterId, 
        DateTime? fromDate = null, 
        DateTime? toDate = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(t => t.Character)
            .Include(t => t.ItemType)
            .Include(t => t.Region)
            .Where(t => t.CharacterId == characterId);

        if (fromDate.HasValue)
            query = query.Where(t => t.TransactionDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(t => t.TransactionDate <= toDate.Value);

        return await query
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get transaction by ESI transaction ID
    /// </summary>
    public async Task<MarketTransaction?> GetByTransactionIdAsync(long transactionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Character)
            .Include(t => t.ItemType)
            .Include(t => t.Region)
            .Include(t => t.RelatedOrder)
            .FirstOrDefaultAsync(t => t.TransactionId == transactionId, cancellationToken);
    }

    /// <summary>
    /// Get transactions for specific item type
    /// </summary>
    public async Task<IEnumerable<MarketTransaction>> GetTransactionsByTypeAsync(
        int characterId, 
        int typeId, 
        DateTime? fromDate = null, 
        DateTime? toDate = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(t => t.Character)
            .Include(t => t.ItemType)
            .Include(t => t.Region)
            .Where(t => t.CharacterId == characterId && t.TypeId == typeId);

        if (fromDate.HasValue)
            query = query.Where(t => t.TransactionDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(t => t.TransactionDate <= toDate.Value);

        return await query
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get transactions by location
    /// </summary>
    public async Task<IEnumerable<MarketTransaction>> GetTransactionsByLocationAsync(
        int characterId, 
        long locationId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Character)
            .Include(t => t.ItemType)
            .Include(t => t.Region)
            .Where(t => t.CharacterId == characterId && t.LocationId == locationId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get recent transactions (last N transactions)
    /// </summary>
    public async Task<IEnumerable<MarketTransaction>> GetRecentTransactionsAsync(
        int characterId, 
        int count = 50, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Character)
            .Include(t => t.ItemType)
            .Include(t => t.Region)
            .Where(t => t.CharacterId == characterId)
            .OrderByDescending(t => t.TransactionDate)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Bulk insert transactions from ESI sync
    /// </summary>
    public async Task BulkInsertTransactionsAsync(IEnumerable<MarketTransaction> transactions, CancellationToken cancellationToken = default)
    {
        var transactionList = transactions.ToList();
        var transactionIds = transactionList.Select(t => t.TransactionId).ToList();

        // Check for existing transactions to avoid duplicates
        var existingIds = await _dbSet
            .Where(t => transactionIds.Contains(t.TransactionId))
            .Select(t => t.TransactionId)
            .ToListAsync(cancellationToken);

        var newTransactions = transactionList
            .Where(t => !existingIds.Contains(t.TransactionId))
            .ToList();

        if (newTransactions.Any())
        {
            await _dbSet.AddRangeAsync(newTransactions, cancellationToken);
        }
    }

    /// <summary>
    /// Get transaction statistics for period
    /// </summary>
    public async Task<TransactionStatistics> GetTransactionStatisticsAsync(
        int characterId, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default)
    {
        var transactions = await _dbSet
            .Where(t => t.CharacterId == characterId && 
                       t.TransactionDate >= fromDate && 
                       t.TransactionDate <= toDate)
            .ToListAsync(cancellationToken);

        if (!transactions.Any())
        {
            return new TransactionStatistics
            {
                CharacterId = characterId,
                FromDate = fromDate,
                ToDate = toDate
            };
        }

        var buyTransactions = transactions.Where(t => t.IsBuy).ToList();
        var sellTransactions = transactions.Where(t => !t.IsBuy).ToList();

        return new TransactionStatistics
        {
            CharacterId = characterId,
            FromDate = fromDate,
            ToDate = toDate,
            
            TotalTransactions = transactions.Count,
            BuyTransactions = buyTransactions.Count,
            SellTransactions = sellTransactions.Count,
            
            TotalValue = transactions.Sum(t => t.TotalValue),
            TotalBuyValue = buyTransactions.Sum(t => t.TotalValue),
            TotalSellValue = sellTransactions.Sum(t => t.TotalValue),
            NetValue = sellTransactions.Sum(t => t.NetValue) - buyTransactions.Sum(t => t.TotalCost),
            
            TotalTax = transactions.Sum(t => t.Tax),
            TotalCommission = transactions.Sum(t => t.Commission),
            
            AverageTransactionValue = transactions.Average(t => t.TotalValue),
            LargestTransaction = transactions.Max(t => t.TotalValue),
            SmallestTransaction = transactions.Min(t => t.TotalValue),
            
            UniqueItems = transactions.Select(t => t.TypeId).Distinct().Count(),
            UniqueLocations = transactions.Select(t => t.LocationId).Distinct().Count()
        };
    }

    /// <summary>
    /// Get top traded items by volume
    /// </summary>
    public async Task<IEnumerable<ItemTradingStats>> GetTopTradedItemsAsync(
        int characterId, 
        DateTime fromDate, 
        DateTime toDate, 
        int count = 10, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.ItemType)
            .Where(t => t.CharacterId == characterId && 
                       t.TransactionDate >= fromDate && 
                       t.TransactionDate <= toDate)
            .GroupBy(t => new { t.TypeId, t.ItemType!.TypeName })
            .Select(g => new ItemTradingStats
            {
                TypeId = g.Key.TypeId,
                TypeName = g.Key.TypeName,
                TotalQuantity = g.Sum(t => t.Quantity),
                TotalValue = g.Sum(t => t.TotalValue),
                AveragePrice = g.Average(t => t.Price),
                TransactionCount = g.Count(),
                Profit = g.Where(t => !t.IsBuy).Sum(t => t.NetValue) - g.Where(t => t.IsBuy).Sum(t => t.TotalCost)
            })
            .OrderByDescending(s => s.TotalQuantity)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get top traded items by profit
    /// </summary>
    public async Task<IEnumerable<ItemTradingStats>> GetMostProfitableItemsAsync(
        int characterId, 
        DateTime fromDate, 
        DateTime toDate, 
        int count = 10, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.ItemType)
            .Where(t => t.CharacterId == characterId && 
                       t.TransactionDate >= fromDate && 
                       t.TransactionDate <= toDate)
            .GroupBy(t => new { t.TypeId, t.ItemType!.TypeName })
            .Select(g => new ItemTradingStats
            {
                TypeId = g.Key.TypeId,
                TypeName = g.Key.TypeName,
                TotalQuantity = g.Sum(t => t.Quantity),
                TotalValue = g.Sum(t => t.TotalValue),
                AveragePrice = g.Average(t => t.Price),
                TransactionCount = g.Count(),
                Profit = g.Where(t => !t.IsBuy).Sum(t => t.NetValue) - g.Where(t => t.IsBuy).Sum(t => t.TotalCost)
            })
            .Where(s => s.Profit > 0)
            .OrderByDescending(s => s.Profit)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get transactions related to specific personal order
    /// </summary>
    public async Task<IEnumerable<MarketTransaction>> GetTransactionsByOrderIdAsync(
        long relatedOrderId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Character)
            .Include(t => t.ItemType)
            .Include(t => t.Region)
            .Include(t => t.RelatedOrder)
            .Where(t => t.RelatedOrderId == relatedOrderId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get transaction sync status for character
    /// </summary>
    public async Task<TransactionSyncStatus?> GetSyncStatusAsync(int characterId, CancellationToken cancellationToken = default)
    {
        return await _context.TransactionSyncStatuses
            .Include(s => s.Character)
            .FirstOrDefaultAsync(s => s.CharacterId == characterId, cancellationToken);
    }

    /// <summary>
    /// Update transaction sync status
    /// </summary>
    public async Task UpsertSyncStatusAsync(TransactionSyncStatus syncStatus, CancellationToken cancellationToken = default)
    {
        var existing = await GetSyncStatusAsync(syncStatus.CharacterId, cancellationToken);
        
        if (existing == null)
        {
            await _context.TransactionSyncStatuses.AddAsync(syncStatus, cancellationToken);
        }
        else
        {
            existing.LastSyncDate = syncStatus.LastSyncDate;
            existing.LastTransactionId = syncStatus.LastTransactionId;
            existing.TransactionsProcessed = syncStatus.TransactionsProcessed;
            existing.TransactionsSkipped = syncStatus.TransactionsSkipped;
            existing.TransactionsErrored = syncStatus.TransactionsErrored;
            existing.Status = syncStatus.Status;
            existing.NextSyncScheduled = syncStatus.NextSyncScheduled;
            existing.LastError = syncStatus.LastError;
            existing.LastUpdated = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Get or create transaction analytics for period
    /// </summary>
    public async Task<TransactionAnalytics> GetOrCreateAnalyticsAsync(
        int characterId, 
        DateTime periodStart, 
        DateTime periodEnd, 
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.TransactionAnalytics
            .FirstOrDefaultAsync(a => a.CharacterId == characterId && 
                                     a.PeriodStart == periodStart.Date && 
                                     a.PeriodEnd == periodEnd.Date, cancellationToken);

        if (existing != null)
            return existing;

        // Create new analytics by calculating from transactions
        var analytics = await CalculateAnalyticsAsync(characterId, periodStart, periodEnd, cancellationToken);
        await _context.TransactionAnalytics.AddAsync(analytics, cancellationToken);
        return analytics;
    }

    /// <summary>
    /// Update transaction analytics
    /// </summary>
    public async Task UpsertAnalyticsAsync(TransactionAnalytics analytics, CancellationToken cancellationToken = default)
    {
        var existing = await _context.TransactionAnalytics
            .FirstOrDefaultAsync(a => a.CharacterId == analytics.CharacterId && 
                                     a.PeriodStart == analytics.PeriodStart && 
                                     a.PeriodEnd == analytics.PeriodEnd, cancellationToken);

        if (existing == null)
        {
            await _context.TransactionAnalytics.AddAsync(analytics, cancellationToken);
        }
        else
        {
            // Update existing analytics
            existing.TotalTransactions = analytics.TotalTransactions;
            existing.BuyTransactions = analytics.BuyTransactions;
            existing.SellTransactions = analytics.SellTransactions;
            existing.TotalValue = analytics.TotalValue;
            existing.TotalBuyValue = analytics.TotalBuyValue;
            existing.TotalSellValue = analytics.TotalSellValue;
            existing.NetProfit = analytics.NetProfit;
            existing.TotalTax = analytics.TotalTax;
            existing.TotalCommission = analytics.TotalCommission;
            existing.AverageTransactionValue = analytics.AverageTransactionValue;
            existing.LargestTransaction = analytics.LargestTransaction;
            existing.SmallestTransaction = analytics.SmallestTransaction;
            existing.AverageMargin = analytics.AverageMargin;
            existing.MostTradedTypeId = analytics.MostTradedTypeId;
            existing.MostTradedTypeName = analytics.MostTradedTypeName;
            existing.TopProfitTypeId = analytics.TopProfitTypeId;
            existing.TopProfitTypeName = analytics.TopProfitTypeName;
            existing.TopProfitAmount = analytics.TopProfitAmount;
            existing.UniqueItemsTraded = analytics.UniqueItemsTraded;
            existing.UniqueLocationsUsed = analytics.UniqueLocationsUsed;
            existing.AverageTimeBetweenTrades = analytics.AverageTimeBetweenTrades;
            existing.MostActiveDay = analytics.MostActiveDay;
            existing.MostActiveHour = analytics.MostActiveHour;
            existing.AnalysisDate = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Get latest analytics for character
    /// </summary>
    public async Task<TransactionAnalytics?> GetLatestAnalyticsAsync(int characterId, CancellationToken cancellationToken = default)
    {
        return await _context.TransactionAnalytics
            .Include(a => a.Character)
            .Where(a => a.CharacterId == characterId)
            .OrderByDescending(a => a.PeriodEnd)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Get profit/loss analysis for time period
    /// </summary>
    public async Task<ProfitLossAnalysis> GetProfitLossAnalysisAsync(
        int characterId, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default)
    {
        var transactions = await GetTransactionsByCharacterAsync(characterId, fromDate, toDate, cancellationToken);
        var transactionList = transactions.ToList();

        var analysis = new ProfitLossAnalysis
        {
            CharacterId = characterId,
            FromDate = fromDate,
            ToDate = toDate
        };

        if (!transactionList.Any())
            return analysis;

        var sells = transactionList.Where(t => !t.IsBuy).ToList();
        var buys = transactionList.Where(t => t.IsBuy).ToList();

        analysis.GrossRevenue = sells.Sum(t => t.NetValue);
        analysis.GrossCosts = buys.Sum(t => t.TotalCost);
        analysis.TotalFees = transactionList.Sum(t => t.Tax + t.Commission);
        analysis.NetProfitLoss = analysis.GrossRevenue - analysis.GrossCosts;

        if (analysis.GrossRevenue > 0)
            analysis.ProfitMargin = (double)(analysis.NetProfitLoss / analysis.GrossRevenue);

        if (analysis.GrossCosts > 0)
            analysis.ROI = (double)(analysis.NetProfitLoss / analysis.GrossCosts);

        // Item breakdown
        analysis.ItemBreakdown = transactionList
            .GroupBy(t => new { t.TypeId, TypeName = t.ItemType?.TypeName ?? "Unknown" })
            .Select(g => new ItemProfitLoss
            {
                TypeId = g.Key.TypeId,
                TypeName = g.Key.TypeName,
                Revenue = g.Where(t => !t.IsBuy).Sum(t => t.NetValue),
                Costs = g.Where(t => t.IsBuy).Sum(t => t.TotalCost),
                Volume = g.Sum(t => t.Quantity)
            })
            .Select(i => new ItemProfitLoss
            {
                TypeId = i.TypeId,
                TypeName = i.TypeName,
                Revenue = i.Revenue,
                Costs = i.Costs,
                Profit = i.Revenue - i.Costs,
                Margin = i.Revenue > 0 ? (double)((i.Revenue - i.Costs) / i.Revenue) : 0,
                Volume = i.Volume
            })
            .OrderByDescending(i => i.Profit)
            .ToList();

        return analysis;
    }

    /// <summary>
    /// Get transaction performance metrics
    /// </summary>
    public async Task<TransactionPerformance> GetPerformanceMetricsAsync(
        int characterId, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default)
    {
        var transactions = await GetTransactionsByCharacterAsync(characterId, fromDate, toDate, cancellationToken);
        var transactionList = transactions.OrderBy(t => t.TransactionDate).ToList();

        var performance = new TransactionPerformance
        {
            CharacterId = characterId,
            FromDate = fromDate,
            ToDate = toDate
        };

        if (!transactionList.Any())
            return performance;

        var totalDays = (toDate - fromDate).TotalDays;
        performance.VelocityScore = totalDays > 0 ? transactionList.Count / totalDays : 0;

        // Calculate time between trades
        if (transactionList.Count > 1)
        {
            var timeDifferences = new List<TimeSpan>();
            for (int i = 1; i < transactionList.Count; i++)
            {
                timeDifferences.Add(transactionList[i].TransactionDate - transactionList[i - 1].TransactionDate);
            }
            performance.AverageTimeBetweenTrades = TimeSpan.FromTicks((long)timeDifferences.Average(t => t.Ticks));
        }

        // Most active day and hour
        performance.MostActiveDay = transactionList
            .GroupBy(t => t.TransactionDate.DayOfWeek)
            .OrderByDescending(g => g.Count())
            .First().Key;

        performance.MostActiveHour = transactionList
            .GroupBy(t => t.TransactionDate.Hour)
            .OrderByDescending(g => g.Count())
            .First().Key;

        // Diversification score
        var uniqueItems = transactionList.Select(t => t.TypeId).Distinct().Count();
        var uniqueLocations = transactionList.Select(t => t.LocationId).Distinct().Count();
        performance.DiversificationScore = Math.Sqrt(uniqueItems * uniqueLocations);

        return performance;
    }

    /// <summary>
    /// Clean up old transactions based on retention policy
    /// </summary>
    public async Task CleanupOldTransactionsAsync(DateTime cutoffDate, CancellationToken cancellationToken = default)
    {
        var oldTransactions = await _dbSet
            .Where(t => t.TransactionDate < cutoffDate)
            .ToListAsync(cancellationToken);

        if (oldTransactions.Any())
        {
            _dbSet.RemoveRange(oldTransactions);
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Calculate analytics for a period
    /// </summary>
    private async Task<TransactionAnalytics> CalculateAnalyticsAsync(
        int characterId, 
        DateTime periodStart, 
        DateTime periodEnd, 
        CancellationToken cancellationToken)
    {
        var transactions = await GetTransactionsByCharacterAsync(characterId, periodStart, periodEnd, cancellationToken);
        var transactionList = transactions.ToList();

        var analytics = new TransactionAnalytics
        {
            CharacterId = characterId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            AnalysisDate = DateTime.UtcNow
        };

        if (!transactionList.Any())
            return analytics;

        var buys = transactionList.Where(t => t.IsBuy).ToList();
        var sells = transactionList.Where(t => !t.IsBuy).ToList();

        analytics.TotalTransactions = transactionList.Count;
        analytics.BuyTransactions = buys.Count;
        analytics.SellTransactions = sells.Count;

        analytics.TotalValue = transactionList.Sum(t => t.TotalValue);
        analytics.TotalBuyValue = buys.Sum(t => t.TotalValue);
        analytics.TotalSellValue = sells.Sum(t => t.TotalValue);
        analytics.NetProfit = sells.Sum(t => t.NetValue) - buys.Sum(t => t.TotalCost);

        analytics.TotalTax = transactionList.Sum(t => t.Tax);
        analytics.TotalCommission = transactionList.Sum(t => t.Commission);

        analytics.AverageTransactionValue = transactionList.Average(t => t.TotalValue);
        analytics.LargestTransaction = transactionList.Max(t => t.TotalValue);
        analytics.SmallestTransaction = transactionList.Min(t => t.TotalValue);

        if (analytics.TotalBuyValue > 0)
            analytics.AverageMargin = (double)(analytics.NetProfit / analytics.TotalBuyValue);

        // Most traded item
        var topVolumeItem = transactionList
            .GroupBy(t => new { t.TypeId, TypeName = t.ItemType?.TypeName ?? "Unknown" })
            .OrderByDescending(g => g.Sum(t => t.Quantity))
            .FirstOrDefault();

        if (topVolumeItem != null)
        {
            analytics.MostTradedTypeId = topVolumeItem.Key.TypeId;
            analytics.MostTradedTypeName = topVolumeItem.Key.TypeName;
        }

        // Most profitable item
        var topProfitItem = transactionList
            .GroupBy(t => new { t.TypeId, TypeName = t.ItemType?.TypeName ?? "Unknown" })
            .Select(g => new
            {
                g.Key.TypeId,
                g.Key.TypeName,
                Profit = g.Where(t => !t.IsBuy).Sum(t => t.NetValue) - g.Where(t => t.IsBuy).Sum(t => t.TotalCost)
            })
            .OrderByDescending(x => x.Profit)
            .FirstOrDefault();

        if (topProfitItem != null)
        {
            analytics.TopProfitTypeId = topProfitItem.TypeId;
            analytics.TopProfitTypeName = topProfitItem.TypeName;
            analytics.TopProfitAmount = topProfitItem.Profit;
        }

        analytics.UniqueItemsTraded = transactionList.Select(t => t.TypeId).Distinct().Count();
        analytics.UniqueLocationsUsed = transactionList.Select(t => t.LocationId).Distinct().Count();

        // Time analysis
        if (transactionList.Count > 1)
        {
            var sortedTxns = transactionList.OrderBy(t => t.TransactionDate).ToList();
            var timeDiffs = new List<TimeSpan>();
            for (int i = 1; i < sortedTxns.Count; i++)
            {
                timeDiffs.Add(sortedTxns[i].TransactionDate - sortedTxns[i - 1].TransactionDate);
            }
            analytics.AverageTimeBetweenTrades = TimeSpan.FromTicks((long)timeDiffs.Average(t => t.Ticks));
        }

        analytics.MostActiveDay = transactionList
            .GroupBy(t => t.TransactionDate.DayOfWeek)
            .OrderByDescending(g => g.Count())
            .First().Key;

        analytics.MostActiveHour = transactionList
            .GroupBy(t => t.TransactionDate.Hour)
            .OrderByDescending(g => g.Count())
            .First().Key;

        return analytics;
    }

    #endregion
}