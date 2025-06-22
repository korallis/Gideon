// ==========================================================================
// ITransactionHistorySyncService.cs - Transaction History Synchronization Service Interface
// ==========================================================================
// Service interface for synchronizing transaction history with ESI API.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Gideon.WPF.Core.Domain.Entities;

namespace Gideon.WPF.Core.Application.Services;

/// <summary>
/// Service interface for transaction history synchronization
/// </summary>
public interface ITransactionHistorySyncService
{
    /// <summary>
    /// Synchronize transaction history for a character from ESI
    /// </summary>
    Task<TransactionSyncResult> SynchronizeTransactionHistoryAsync(int characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronize transactions for all authenticated characters
    /// </summary>
    Task<List<TransactionSyncResult>> SynchronizeAllCharactersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get synchronization status for a character
    /// </summary>
    Task<TransactionSyncStatus> GetSyncStatusAsync(int characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Force full synchronization (ignore last sync date)
    /// </summary>
    Task<TransactionSyncResult> ForceFullSyncAsync(int characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get transaction analytics for character
    /// </summary>
    Task<TransactionAnalytics> GetTransactionAnalyticsAsync(
        int characterId, 
        DateTime? fromDate = null, 
        DateTime? toDate = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate transaction report for period
    /// </summary>
    Task<TransactionReport> GenerateTransactionReportAsync(
        int characterId, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Link transactions to personal market orders
    /// </summary>
    Task LinkTransactionsToOrdersAsync(int characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate tax efficiency metrics
    /// </summary>
    Task<TaxEfficiencyAnalysis> AnalyzeTaxEfficiencyAsync(
        int characterId, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get trading velocity analysis
    /// </summary>
    Task<TradingVelocityAnalysis> GetTradingVelocityAsync(
        int characterId, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedule automatic synchronization
    /// </summary>
    Task ScheduleAutoSyncAsync(int characterId, TimeSpan interval, CancellationToken cancellationToken = default);

    /// <summary>
    /// Export transaction history to various formats
    /// </summary>
    Task<TransactionExportResult> ExportTransactionHistoryAsync(
        int characterId, 
        DateTime fromDate, 
        DateTime toDate, 
        TransactionExportFormat format, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get transaction anomaly detection results
    /// </summary>
    Task<List<TransactionAnomaly>> DetectTransactionAnomaliesAsync(
        int characterId, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Transaction synchronization result
/// </summary>
public class TransactionSyncResult
{
    public int CharacterId { get; set; }
    public bool Success { get; set; }
    public DateTime SyncStartTime { get; set; }
    public DateTime SyncEndTime { get; set; }
    public TimeSpan Duration => SyncEndTime - SyncStartTime;
    
    public int TransactionsRetrieved { get; set; }
    public int TransactionsProcessed { get; set; }
    public int TransactionsSkipped { get; set; }
    public int TransactionsErrored { get; set; }
    
    public long LastTransactionId { get; set; }
    public DateTime LastTransactionDate { get; set; }
    
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string? ErrorMessage { get; set; }
    
    public SyncType SyncType { get; set; }
    public int RateLimitRemaining { get; set; }
    public DateTime? RateLimitReset { get; set; }
}

/// <summary>
/// Transaction report summary
/// </summary>
public class TransactionReport
{
    public int CharacterId { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    // Summary statistics
    public TransactionStatistics Summary { get; set; } = new();
    public ProfitLossAnalysis ProfitLoss { get; set; } = new();
    public TransactionPerformance Performance { get; set; } = new();
    
    // Detailed breakdowns
    public List<ItemTradingStats> TopTradedItems { get; set; } = new();
    public List<ItemTradingStats> MostProfitableItems { get; set; } = new();
    public List<LocationTradingStats> TopLocations { get; set; } = new();
    public List<DailyTradingStats> DailyBreakdown { get; set; } = new();
    public List<MonthlyTradingStats> MonthlyBreakdown { get; set; } = new();
    
    // Charts data
    public List<ChartDataPoint> ProfitChart { get; set; } = new();
    public List<ChartDataPoint> VolumeChart { get; set; } = new();
    public List<ChartDataPoint> TransactionCountChart { get; set; } = new();
}

/// <summary>
/// Tax efficiency analysis
/// </summary>
public class TaxEfficiencyAnalysis
{
    public int CharacterId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    
    public decimal TotalTaxPaid { get; set; }
    public decimal TotalCommissionPaid { get; set; }
    public decimal TotalFeesAsPercentage { get; set; }
    
    public decimal AverageBrokerFeeRate { get; set; }
    public decimal AverageTransactionTaxRate { get; set; }
    
    public decimal PotentialSavingsWithSkills { get; set; }
    public decimal PotentialSavingsWithStandings { get; set; }
    
    public List<TaxSavingsRecommendation> Recommendations { get; set; } = new();
    public List<LocationTaxAnalysis> LocationAnalysis { get; set; } = new();
}

/// <summary>
/// Trading velocity analysis
/// </summary>
public class TradingVelocityAnalysis
{
    public int CharacterId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    
    public double TransactionsPerDay { get; set; }
    public double TransactionsPerHour { get; set; }
    public decimal ValuePerDay { get; set; }
    public decimal ProfitPerDay { get; set; }
    
    public TimeSpan AverageTimeBetweenTrades { get; set; }
    public TimeSpan FastestTradingStreak { get; set; }
    public TimeSpan LongestBreak { get; set; }
    
    public List<TradingSessionAnalysis> TradingSessions { get; set; } = new();
    public List<VelocityTrend> VelocityTrends { get; set; } = new();
    
    public double ActivityScore { get; set; }
    public TradingPattern TradingPattern { get; set; }
}

/// <summary>
/// Transaction export result
/// </summary>
public class TransactionExportResult
{
    public bool Success { get; set; }
    public string? FilePath { get; set; }
    public TransactionExportFormat Format { get; set; }
    public int RecordCount { get; set; }
    public long FileSizeBytes { get; set; }
    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Transaction anomaly detection
/// </summary>
public class TransactionAnomaly
{
    public long TransactionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public AnomalyType Type { get; set; }
    public AnomalySeverity Severity { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal ExpectedValue { get; set; }
    public decimal ActualValue { get; set; }
    public double DeviationScore { get; set; }
    public List<string> PossibleCauses { get; set; } = new();
}

/// <summary>
/// Supporting data structures
/// </summary>
public class LocationTradingStats
{
    public long LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public decimal TotalValue { get; set; }
    public decimal Profit { get; set; }
    public double ProfitMargin { get; set; }
}

public class DailyTradingStats
{
    public DateTime Date { get; set; }
    public int TransactionCount { get; set; }
    public decimal TotalValue { get; set; }
    public decimal Profit { get; set; }
    public int UniqueItems { get; set; }
}

public class MonthlyTradingStats
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public decimal TotalValue { get; set; }
    public decimal Profit { get; set; }
    public int TradingDays { get; set; }
}

public class ChartDataPoint
{
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
    public string Label { get; set; } = string.Empty;
}

public class TaxSavingsRecommendation
{
    public string Recommendation { get; set; } = string.Empty;
    public decimal PotentialSavings { get; set; }
    public string SkillOrStanding { get; set; } = string.Empty;
    public int RequiredLevel { get; set; }
    public string Benefit { get; set; } = string.Empty;
}

public class LocationTaxAnalysis
{
    public long LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public decimal AverageTaxRate { get; set; }
    public decimal TotalTaxPaid { get; set; }
    public decimal PotentialSavings { get; set; }
    public string OwningCorporation { get; set; } = string.Empty;
}

public class TradingSessionAnalysis
{
    public DateTime SessionStart { get; set; }
    public DateTime SessionEnd { get; set; }
    public TimeSpan Duration { get; set; }
    public int TransactionCount { get; set; }
    public decimal TotalValue { get; set; }
    public decimal Profit { get; set; }
    public double Intensity { get; set; } // Transactions per hour
}

public class VelocityTrend
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public double AverageVelocity { get; set; }
    public TrendDirection Direction { get; set; }
    public double ChangePercentage { get; set; }
}

/// <summary>
/// Enumerations
/// </summary>
public enum SyncType
{
    Incremental,
    Full,
    Scheduled,
    Manual
}

public enum TransactionExportFormat
{
    CSV,
    JSON,
    XML,
    Excel,
    PDF
}

public enum AnomalyType
{
    UnusualPrice,
    UnusualVolume,
    UnusualLocation,
    UnusualTiming,
    UnusualCounterparty,
    SuspiciousPattern
}

public enum AnomalySeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum TradingPattern
{
    Casual,
    Regular,
    Intensive,
    Sporadic,
    Professional
}