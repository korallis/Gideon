// ==========================================================================
// TransactionHistorySyncService.cs - Transaction History Synchronization Service Implementation
// ==========================================================================
// Service implementation for synchronizing transaction history with ESI API.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.Extensions.Logging;
using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Core.Domain.Interfaces;

namespace Gideon.WPF.Core.Application.Services;

/// <summary>
/// Transaction history synchronization service implementation
/// </summary>
public class TransactionHistorySyncService : ITransactionHistorySyncService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPersonalMarketOrderRepository _orderRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionHistorySyncService> _logger;

    public TransactionHistorySyncService(
        ITransactionRepository transactionRepository,
        IPersonalMarketOrderRepository orderRepository,
        ICharacterRepository characterRepository,
        IUnitOfWork unitOfWork,
        ILogger<TransactionHistorySyncService> logger)
    {
        _transactionRepository = transactionRepository;
        _orderRepository = orderRepository;
        _characterRepository = characterRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Synchronize transaction history for a character from ESI
    /// </summary>
    public async Task<TransactionSyncResult> SynchronizeTransactionHistoryAsync(int characterId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting transaction history synchronization for character {CharacterId}", characterId);

        var result = new TransactionSyncResult
        {
            CharacterId = characterId,
            SyncStartTime = DateTime.UtcNow,
            SyncType = SyncType.Incremental
        };

        try
        {
            var character = await _characterRepository.GetByIdAsync(characterId, cancellationToken);
            if (character == null)
            {
                result.Success = false;
                result.ErrorMessage = $"Character {characterId} not found";
                return result;
            }

            // Get sync status to determine where to start
            var syncStatus = await _transactionRepository.GetSyncStatusAsync(characterId, cancellationToken);
            var fromTransactionId = syncStatus?.LastTransactionId ?? 0;

            // TODO: In a real implementation, this would call ESI API
            // var esiTransactions = await _esiClient.GetCharacterTransactionsAsync(characterId, fromTransactionId);
            
            // For now, simulate ESI call with empty list
            var esiTransactions = new List<MarketTransaction>();

            // Process transactions
            await ProcessESITransactionsAsync(esiTransactions, characterId, result, cancellationToken);

            // Update sync status
            await UpdateSyncStatusAsync(characterId, result, cancellationToken);

            // Link transactions to orders
            await LinkTransactionsToOrdersAsync(characterId, cancellationToken);

            // Update analytics
            await UpdateTransactionAnalyticsAsync(characterId, cancellationToken);

            result.Success = true;
            result.SyncEndTime = DateTime.UtcNow;

            _logger.LogInformation("Transaction history synchronization completed for character {CharacterId}. " +
                                 "Retrieved: {Retrieved}, Processed: {Processed}, Skipped: {Skipped}, Errors: {Errors}",
                characterId, result.TransactionsRetrieved, result.TransactionsProcessed,
                result.TransactionsSkipped, result.TransactionsErrored);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.Errors.Add(ex.ToString());
            result.SyncEndTime = DateTime.UtcNow;

            _logger.LogError(ex, "Error during transaction history synchronization for character {CharacterId}", characterId);
        }

        return result;
    }

    /// <summary>
    /// Synchronize transactions for all authenticated characters
    /// </summary>
    public async Task<List<TransactionSyncResult>> SynchronizeAllCharactersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting transaction synchronization for all characters");

        var results = new List<TransactionSyncResult>();
        var characters = await _characterRepository.GetAllAsync(cancellationToken);

        foreach (var character in characters)
        {
            try
            {
                var result = await SynchronizeTransactionHistoryAsync(character.Id, cancellationToken);
                results.Add(result);

                // Small delay between characters to respect rate limits
                await Task.Delay(1000, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synchronizing character {CharacterId}", character.Id);
                results.Add(new TransactionSyncResult
                {
                    CharacterId = character.Id,
                    Success = false,
                    ErrorMessage = ex.Message,
                    SyncStartTime = DateTime.UtcNow,
                    SyncEndTime = DateTime.UtcNow
                });
            }
        }

        return results;
    }

    /// <summary>
    /// Get synchronization status for a character
    /// </summary>
    public async Task<TransactionSyncStatus> GetSyncStatusAsync(int characterId, CancellationToken cancellationToken = default)
    {
        var status = await _transactionRepository.GetSyncStatusAsync(characterId, cancellationToken);
        
        if (status == null)
        {
            status = new TransactionSyncStatus
            {
                CharacterId = characterId,
                Status = SyncStatusType.Pending,
                LastSyncDate = DateTime.MinValue,
                LastTransactionId = 0
            };
        }

        return status;
    }

    /// <summary>
    /// Force full synchronization (ignore last sync date)
    /// </summary>
    public async Task<TransactionSyncResult> ForceFullSyncAsync(int characterId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting full transaction synchronization for character {CharacterId}", characterId);

        // Reset sync status to force full sync
        var syncStatus = new TransactionSyncStatus
        {
            CharacterId = characterId,
            LastTransactionId = 0,
            LastSyncDate = DateTime.MinValue,
            Status = SyncStatusType.Pending
        };

        await _transactionRepository.UpsertSyncStatusAsync(syncStatus, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = await SynchronizeTransactionHistoryAsync(characterId, cancellationToken);
        result.SyncType = SyncType.Full;

        return result;
    }

    /// <summary>
    /// Get transaction analytics for character
    /// </summary>
    public async Task<TransactionAnalytics> GetTransactionAnalyticsAsync(
        int characterId, 
        DateTime? fromDate = null, 
        DateTime? toDate = null, 
        CancellationToken cancellationToken = default)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-30); // Default to last 30 days
        var to = toDate ?? DateTime.UtcNow;

        return await _transactionRepository.GetOrCreateAnalyticsAsync(characterId, from, to, cancellationToken);
    }

    /// <summary>
    /// Generate transaction report for period
    /// </summary>
    public async Task<TransactionReport> GenerateTransactionReportAsync(
        int characterId, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating transaction report for character {CharacterId} from {FromDate} to {ToDate}",
            characterId, fromDate, toDate);

        var character = await _characterRepository.GetByIdAsync(characterId, cancellationToken);
        var report = new TransactionReport
        {
            CharacterId = characterId,
            CharacterName = character?.CharacterName ?? "Unknown",
            FromDate = fromDate,
            ToDate = toDate
        };

        // Get summary statistics
        report.Summary = await _transactionRepository.GetTransactionStatisticsAsync(characterId, fromDate, toDate, cancellationToken);

        // Get profit/loss analysis
        report.ProfitLoss = await _transactionRepository.GetProfitLossAnalysisAsync(characterId, fromDate, toDate, cancellationToken);

        // Get performance metrics
        report.Performance = await _transactionRepository.GetPerformanceMetricsAsync(characterId, fromDate, toDate, cancellationToken);

        // Get top traded items
        report.TopTradedItems = (await _transactionRepository.GetTopTradedItemsAsync(characterId, fromDate, toDate, 10, cancellationToken)).ToList();

        // Get most profitable items
        report.MostProfitableItems = (await _transactionRepository.GetMostProfitableItemsAsync(characterId, fromDate, toDate, 10, cancellationToken)).ToList();

        // Generate daily breakdown
        report.DailyBreakdown = await GenerateDailyBreakdownAsync(characterId, fromDate, toDate, cancellationToken);

        // Generate chart data
        report.ProfitChart = await GenerateProfitChartDataAsync(characterId, fromDate, toDate, cancellationToken);

        return report;
    }

    /// <summary>
    /// Link transactions to personal market orders
    /// </summary>
    public async Task LinkTransactionsToOrdersAsync(int characterId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Linking transactions to orders for character {CharacterId}", characterId);

        var transactions = await _transactionRepository.GetTransactionsByCharacterAsync(characterId, 
            DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, cancellationToken);

        var orders = await _orderRepository.GetActiveOrdersByCharacterAsync(characterId, cancellationToken);

        foreach (var transaction in transactions.Where(t => t.RelatedOrderId == null))
        {
            // Try to find matching order based on type, price, and timing
            var matchingOrder = orders.FirstOrDefault(o =>
                o.TypeId == transaction.TypeId &&
                o.IsBuyOrder == transaction.IsBuy &&
                Math.Abs(o.Price - transaction.Price) < 0.01m &&
                transaction.TransactionDate >= o.IssuedDate);

            if (matchingOrder != null)
            {
                transaction.RelatedOrderId = matchingOrder.OrderId;
                transaction.LastUpdated = DateTime.UtcNow;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Calculate tax efficiency metrics
    /// </summary>
    public async Task<TaxEfficiencyAnalysis> AnalyzeTaxEfficiencyAsync(
        int characterId, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default)
    {
        var transactions = await _transactionRepository.GetTransactionsByCharacterAsync(characterId, fromDate, toDate, cancellationToken);
        var transactionList = transactions.ToList();

        var analysis = new TaxEfficiencyAnalysis
        {
            CharacterId = characterId,
            FromDate = fromDate,
            ToDate = toDate
        };

        if (!transactionList.Any())
            return analysis;

        analysis.TotalTaxPaid = transactionList.Sum(t => t.Tax);
        analysis.TotalCommissionPaid = transactionList.Sum(t => t.Commission);
        var totalValue = transactionList.Sum(t => t.TotalValue);
        
        if (totalValue > 0)
            analysis.TotalFeesAsPercentage = (analysis.TotalTaxPaid + analysis.TotalCommissionPaid) / totalValue * 100;

        // Calculate potential savings
        analysis.PotentialSavingsWithSkills = analysis.TotalCommissionPaid * 0.3m; // Assume 30% reduction with skills
        analysis.PotentialSavingsWithStandings = analysis.TotalCommissionPaid * 0.1m; // Assume 10% reduction with standings

        // Add recommendations
        analysis.Recommendations.Add(new TaxSavingsRecommendation
        {
            Recommendation = "Train Broker Relations skill to V",
            PotentialSavings = analysis.TotalCommissionPaid * 0.25m,
            SkillOrStanding = "Broker Relations",
            RequiredLevel = 5,
            Benefit = "Reduces broker fees by 25%"
        });

        return analysis;
    }

    /// <summary>
    /// Get trading velocity analysis
    /// </summary>
    public async Task<TradingVelocityAnalysis> GetTradingVelocityAsync(
        int characterId, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default)
    {
        var transactions = await _transactionRepository.GetTransactionsByCharacterAsync(characterId, fromDate, toDate, cancellationToken);
        var transactionList = transactions.OrderBy(t => t.TransactionDate).ToList();

        var analysis = new TradingVelocityAnalysis
        {
            CharacterId = characterId,
            FromDate = fromDate,
            ToDate = toDate
        };

        if (!transactionList.Any())
            return analysis;

        var totalDays = (toDate - fromDate).TotalDays;
        var totalHours = (toDate - fromDate).TotalHours;

        analysis.TransactionsPerDay = totalDays > 0 ? transactionList.Count / totalDays : 0;
        analysis.TransactionsPerHour = totalHours > 0 ? transactionList.Count / totalHours : 0;
        analysis.ValuePerDay = totalDays > 0 ? transactionList.Sum(t => t.TotalValue) / (decimal)totalDays : 0;

        // Calculate activity score based on various factors
        analysis.ActivityScore = CalculateActivityScore(transactionList, totalDays);
        analysis.TradingPattern = DetermineTradingPattern(analysis.ActivityScore, analysis.TransactionsPerDay);

        return analysis;
    }

    /// <summary>
    /// Schedule automatic synchronization
    /// </summary>
    public async Task ScheduleAutoSyncAsync(int characterId, TimeSpan interval, CancellationToken cancellationToken = default)
    {
        var syncStatus = await GetSyncStatusAsync(characterId, cancellationToken);
        syncStatus.NextSyncScheduled = DateTime.UtcNow.Add(interval);
        
        await _transactionRepository.UpsertSyncStatusAsync(syncStatus, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Scheduled automatic sync for character {CharacterId} in {Interval}", 
            characterId, interval);
    }

    /// <summary>
    /// Export transaction history to various formats
    /// </summary>
    public async Task<TransactionExportResult> ExportTransactionHistoryAsync(
        int characterId, 
        DateTime fromDate, 
        DateTime toDate, 
        TransactionExportFormat format, 
        CancellationToken cancellationToken = default)
    {
        var result = new TransactionExportResult
        {
            Format = format,
            Success = false
        };

        try
        {
            var transactions = await _transactionRepository.GetTransactionsByCharacterAsync(characterId, fromDate, toDate, cancellationToken);
            var transactionList = transactions.ToList();

            result.RecordCount = transactionList.Count;

            // TODO: Implement actual export logic for different formats
            switch (format)
            {
                case TransactionExportFormat.CSV:
                    result.FilePath = await ExportToCSVAsync(transactionList, characterId, fromDate, toDate);
                    break;
                case TransactionExportFormat.JSON:
                    result.FilePath = await ExportToJSONAsync(transactionList, characterId, fromDate, toDate);
                    break;
                default:
                    throw new NotSupportedException($"Export format {format} is not yet supported");
            }

            result.Success = true;
            _logger.LogInformation("Exported {Count} transactions to {Format} for character {CharacterId}",
                result.RecordCount, format, characterId);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Error exporting transactions for character {CharacterId}", characterId);
        }

        return result;
    }

    /// <summary>
    /// Get transaction anomaly detection results
    /// </summary>
    public async Task<List<TransactionAnomaly>> DetectTransactionAnomaliesAsync(
        int characterId, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default)
    {
        var anomalies = new List<TransactionAnomaly>();
        var transactions = await _transactionRepository.GetTransactionsByCharacterAsync(characterId, fromDate, toDate, cancellationToken);
        var transactionList = transactions.ToList();

        if (!transactionList.Any())
            return anomalies;

        // Detect price anomalies
        anomalies.AddRange(DetectPriceAnomalies(transactionList));

        // Detect volume anomalies
        anomalies.AddRange(DetectVolumeAnomalies(transactionList));

        // Detect timing anomalies
        anomalies.AddRange(DetectTimingAnomalies(transactionList));

        return anomalies.OrderByDescending(a => a.Severity).ThenByDescending(a => a.DeviationScore).ToList();
    }

    #region Private Helper Methods

    /// <summary>
    /// Process ESI transactions and save to database
    /// </summary>
    private async Task ProcessESITransactionsAsync(
        IEnumerable<MarketTransaction> esiTransactions, 
        int characterId, 
        TransactionSyncResult result, 
        CancellationToken cancellationToken)
    {
        var transactions = esiTransactions.ToList();
        result.TransactionsRetrieved = transactions.Count;

        if (!transactions.Any())
            return;

        try
        {
            await _transactionRepository.BulkInsertTransactionsAsync(transactions, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            result.TransactionsProcessed = transactions.Count;
            result.LastTransactionId = transactions.Max(t => t.TransactionId);
            result.LastTransactionDate = transactions.Max(t => t.TransactionDate);
        }
        catch (Exception ex)
        {
            result.TransactionsErrored = transactions.Count;
            result.Errors.Add($"Error processing transactions: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Update sync status after synchronization
    /// </summary>
    private async Task UpdateSyncStatusAsync(int characterId, TransactionSyncResult result, CancellationToken cancellationToken)
    {
        var syncStatus = new TransactionSyncStatus
        {
            CharacterId = characterId,
            LastSyncDate = result.SyncEndTime,
            LastTransactionId = result.LastTransactionId,
            TransactionsProcessed = result.TransactionsProcessed,
            TransactionsSkipped = result.TransactionsSkipped,
            TransactionsErrored = result.TransactionsErrored,
            Status = result.Success ? SyncStatusType.Completed : SyncStatusType.Failed,
            LastError = result.ErrorMessage
        };

        await _transactionRepository.UpsertSyncStatusAsync(syncStatus, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Update transaction analytics
    /// </summary>
    private async Task UpdateTransactionAnalyticsAsync(int characterId, CancellationToken cancellationToken)
    {
        var endDate = DateTime.UtcNow.Date;
        var startDate = endDate.AddDays(-30); // Last 30 days

        var analytics = await _transactionRepository.GetOrCreateAnalyticsAsync(characterId, startDate, endDate, cancellationToken);
        await _transactionRepository.UpsertAnalyticsAsync(analytics, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Generate daily breakdown for report
    /// </summary>
    private async Task<List<DailyTradingStats>> GenerateDailyBreakdownAsync(
        int characterId, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken)
    {
        var transactions = await _transactionRepository.GetTransactionsByCharacterAsync(characterId, fromDate, toDate, cancellationToken);
        
        return transactions
            .GroupBy(t => t.TransactionDate.Date)
            .Select(g => new DailyTradingStats
            {
                Date = g.Key,
                TransactionCount = g.Count(),
                TotalValue = g.Sum(t => t.TotalValue),
                Profit = g.Where(t => !t.IsBuy).Sum(t => t.NetValue) - g.Where(t => t.IsBuy).Sum(t => t.TotalCost),
                UniqueItems = g.Select(t => t.TypeId).Distinct().Count()
            })
            .OrderBy(d => d.Date)
            .ToList();
    }

    /// <summary>
    /// Generate profit chart data
    /// </summary>
    private async Task<List<ChartDataPoint>> GenerateProfitChartDataAsync(
        int characterId, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken)
    {
        var dailyStats = await GenerateDailyBreakdownAsync(characterId, fromDate, toDate, cancellationToken);
        
        return dailyStats.Select(d => new ChartDataPoint
        {
            Date = d.Date,
            Value = d.Profit,
            Label = d.Date.ToString("MMM dd")
        }).ToList();
    }

    /// <summary>
    /// Calculate activity score
    /// </summary>
    private static double CalculateActivityScore(List<MarketTransaction> transactions, double totalDays)
    {
        if (!transactions.Any() || totalDays <= 0)
            return 0;

        var transactionsPerDay = transactions.Count / totalDays;
        var uniqueDays = transactions.Select(t => t.TransactionDate.Date).Distinct().Count();
        var consistency = uniqueDays / totalDays;
        
        return Math.Min(100, transactionsPerDay * 10 + consistency * 50);
    }

    /// <summary>
    /// Determine trading pattern
    /// </summary>
    private static TradingPattern DetermineTradingPattern(double activityScore, double transactionsPerDay)
    {
        if (transactionsPerDay < 1)
            return TradingPattern.Casual;
        else if (transactionsPerDay < 5)
            return TradingPattern.Regular;
        else if (transactionsPerDay < 20)
            return TradingPattern.Intensive;
        else if (activityScore < 30)
            return TradingPattern.Sporadic;
        else
            return TradingPattern.Professional;
    }

    /// <summary>
    /// Detect price anomalies
    /// </summary>
    private static List<TransactionAnomaly> DetectPriceAnomalies(List<MarketTransaction> transactions)
    {
        var anomalies = new List<TransactionAnomaly>();
        
        // Group by item type and calculate average prices
        var priceStats = transactions
            .GroupBy(t => t.TypeId)
            .Where(g => g.Count() > 5) // Need enough data points
            .Select(g => new
            {
                TypeId = g.Key,
                AveragePrice = g.Average(t => t.Price),
                StandardDeviation = CalculateStandardDeviation(g.Select(t => t.Price).ToList())
            })
            .ToList();

        foreach (var stat in priceStats)
        {
            var relevantTransactions = transactions.Where(t => t.TypeId == stat.TypeId).ToList();
            
            foreach (var transaction in relevantTransactions)
            {
                var deviation = Math.Abs(transaction.Price - stat.AveragePrice);
                var deviationScore = stat.StandardDeviation > 0 ? deviation / stat.StandardDeviation : 0;
                
                if (deviationScore > 2.5) // More than 2.5 standard deviations
                {
                    anomalies.Add(new TransactionAnomaly
                    {
                        TransactionId = transaction.TransactionId,
                        TransactionDate = transaction.TransactionDate,
                        Type = AnomalyType.UnusualPrice,
                        Severity = deviationScore > 4 ? AnomalySeverity.High : AnomalySeverity.Medium,
                        Description = $"Price {transaction.Price:N2} is significantly different from average {stat.AveragePrice:N2}",
                        ExpectedValue = stat.AveragePrice,
                        ActualValue = transaction.Price,
                        DeviationScore = deviationScore
                    });
                }
            }
        }

        return anomalies;
    }

    /// <summary>
    /// Detect volume anomalies
    /// </summary>
    private static List<TransactionAnomaly> DetectVolumeAnomalies(List<MarketTransaction> transactions)
    {
        var anomalies = new List<TransactionAnomaly>();
        
        var volumes = transactions.Select(t => (decimal)t.Quantity).ToList();
        if (volumes.Count < 10) return anomalies;

        var averageVolume = volumes.Average();
        var stdDev = CalculateStandardDeviation(volumes);

        foreach (var transaction in transactions)
        {
            var deviation = Math.Abs(transaction.Quantity - (long)averageVolume);
            var deviationScore = stdDev > 0 ? deviation / (double)stdDev : 0;

            if (deviationScore > 3)
            {
                anomalies.Add(new TransactionAnomaly
                {
                    TransactionId = transaction.TransactionId,
                    TransactionDate = transaction.TransactionDate,
                    Type = AnomalyType.UnusualVolume,
                    Severity = deviationScore > 5 ? AnomalySeverity.High : AnomalySeverity.Medium,
                    Description = $"Volume {transaction.Quantity:N0} is unusually large compared to average {averageVolume:N0}",
                    ExpectedValue = averageVolume,
                    ActualValue = transaction.Quantity,
                    DeviationScore = deviationScore
                });
            }
        }

        return anomalies;
    }

    /// <summary>
    /// Detect timing anomalies
    /// </summary>
    private static List<TransactionAnomaly> DetectTimingAnomalies(List<MarketTransaction> transactions)
    {
        var anomalies = new List<TransactionAnomaly>();
        
        var sortedTransactions = transactions.OrderBy(t => t.TransactionDate).ToList();
        if (sortedTransactions.Count < 5) return anomalies;

        // Look for unusual trading hours
        var hourCounts = sortedTransactions.GroupBy(t => t.TransactionDate.Hour).ToDictionary(g => g.Key, g => g.Count());
        var averagePerHour = hourCounts.Values.Average();
        
        foreach (var transaction in sortedTransactions)
        {
            var hour = transaction.TransactionDate.Hour;
            
            // Check for trades during unusual hours (very early morning)
            if (hour >= 2 && hour <= 6 && hourCounts[hour] < averagePerHour * 0.1)
            {
                anomalies.Add(new TransactionAnomaly
                {
                    TransactionId = transaction.TransactionId,
                    TransactionDate = transaction.TransactionDate,
                    Type = AnomalyType.UnusualTiming,
                    Severity = AnomalySeverity.Low,
                    Description = $"Transaction occurred during unusual hours ({hour:D2}:00)",
                    DeviationScore = 1.0
                });
            }
        }

        return anomalies;
    }

    /// <summary>
    /// Calculate standard deviation
    /// </summary>
    private static decimal CalculateStandardDeviation(List<decimal> values)
    {
        if (values.Count < 2) return 0;

        var average = values.Average();
        var sumSquaredDifferences = values.Sum(v => Math.Pow((double)(v - average), 2));
        var variance = sumSquaredDifferences / (values.Count - 1);
        return (decimal)Math.Sqrt(variance);
    }

    /// <summary>
    /// Export transactions to CSV
    /// </summary>
    private static async Task<string> ExportToCSVAsync(List<MarketTransaction> transactions, int characterId, DateTime fromDate, DateTime toDate)
    {
        // TODO: Implement CSV export
        var fileName = $"transactions_{characterId}_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.csv";
        var filePath = Path.Combine(Path.GetTempPath(), fileName);
        
        // Placeholder implementation
        await File.WriteAllTextAsync(filePath, "CSV export not yet implemented");
        
        return filePath;
    }

    /// <summary>
    /// Export transactions to JSON
    /// </summary>
    private static async Task<string> ExportToJSONAsync(List<MarketTransaction> transactions, int characterId, DateTime fromDate, DateTime toDate)
    {
        // TODO: Implement JSON export
        var fileName = $"transactions_{characterId}_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.json";
        var filePath = Path.Combine(Path.GetTempPath(), fileName);
        
        // Placeholder implementation
        await File.WriteAllTextAsync(filePath, "JSON export not yet implemented");
        
        return filePath;
    }

    #endregion
}