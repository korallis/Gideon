// ==========================================================================
// IMarketTrendRepository.cs - Market Trend Analysis Repository Interface
// ==========================================================================
// Repository interface for market trend analysis with specialized methods
// for trend analysis, prediction management, and signal processing.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Gideon.WPF.Core.Domain.Entities;

namespace Gideon.WPF.Core.Domain.Interfaces;

/// <summary>
/// Repository interface for market trend analysis
/// </summary>
public interface IMarketTrendRepository : IRepository<MarketTrendAnalysis>
{
    /// <summary>
    /// Get trend analysis for a specific item and region
    /// </summary>
    Task<MarketTrendAnalysis?> GetLatestTrendAnalysisAsync(int typeId, int regionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get trend analyses for multiple items in a region
    /// </summary>
    Task<List<MarketTrendAnalysis>> GetTrendAnalysesAsync(List<int> typeIds, int regionId, DateTime? since = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get trend analyses by market regime
    /// </summary>
    Task<List<MarketTrendAnalysis>> GetTrendAnalysesByRegimeAsync(MarketRegime regime, int? regionId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get items with specific trend direction
    /// </summary>
    Task<List<MarketTrendAnalysis>> GetItemsByTrendDirectionAsync(TrendDirection direction, int? regionId = null, double minStrength = 0.5, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get volatile items (high price volatility)
    /// </summary>
    Task<List<MarketTrendAnalysis>> GetVolatileItemsAsync(int regionId, double minVolatility = 0.1, int maxResults = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get liquid items (high liquidity score)
    /// </summary>
    Task<List<MarketTrendAnalysis>> GetLiquidItemsAsync(int regionId, double minLiquidity = 0.7, int maxResults = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get items with anomalies detected
    /// </summary>
    Task<List<MarketTrendAnalysis>> GetItemsWithAnomaliesAsync(int? regionId = null, double minAnomalyScore = 0.5, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get trend analysis statistics for a region
    /// </summary>
    Task<TrendStatistics> GetTrendStatisticsAsync(int regionId, DateTime? fromDate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update trend analysis data
    /// </summary>
    Task UpdateTrendAnalysisAsync(MarketTrendAnalysis analysis, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clean up old trend analyses
    /// </summary>
    Task<int> CleanupOldAnalysesAsync(TimeSpan maxAge, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for market predictions
/// </summary>
public interface IMarketPredictionRepository : IRepository<MarketPrediction>
{
    /// <summary>
    /// Get active predictions for an item
    /// </summary>
    Task<List<MarketPrediction>> GetActivePredictionsAsync(int typeId, int regionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get predictions by algorithm
    /// </summary>
    Task<List<MarketPrediction>> GetPredictionsByAlgorithmAsync(PredictionAlgorithm algorithm, DateTime? since = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get predictions due for validation
    /// </summary>
    Task<List<MarketPrediction>> GetPredictionsDueForValidationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get predictions by confidence level
    /// </summary>
    Task<List<MarketPrediction>> GetHighConfidencePredictionsAsync(double minConfidence = 0.8, int? regionId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get predictions with trading signals
    /// </summary>
    Task<List<MarketPrediction>> GetPredictionsWithSignalsAsync(List<TradingSignal> signals, int? regionId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate prediction accuracy
    /// </summary>
    Task<PredictionValidationResult> ValidatePredictionAsync(int predictionId, decimal actualPrice, long actualVolume, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get prediction accuracy statistics
    /// </summary>
    Task<PredictionAccuracyStats> GetAccuracyStatisticsAsync(PredictionAlgorithm? algorithm = null, TimeSpan? period = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Expire old predictions
    /// </summary>
    Task<int> ExpireOldPredictionsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for trend signals
/// </summary>
public interface ITrendSignalRepository : IRepository<TrendSignal>
{
    /// <summary>
    /// Get active signals for monitoring
    /// </summary>
    Task<List<TrendSignal>> GetActiveSignalsAsync(int? regionId = null, SignalPriority? minPriority = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get signals by type and timeframe
    /// </summary>
    Task<List<TrendSignal>> GetSignalsByTypeAsync(TradingSignal signalType, SignalTimeframe? timeframe = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get signals requiring action
    /// </summary>
    Task<List<TrendSignal>> GetSignalsRequiringActionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recent signals for an item
    /// </summary>
    Task<List<TrendSignal>> GetRecentSignalsAsync(int typeId, int regionId, TimeSpan period, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate signal outcome
    /// </summary>
    Task ValidateSignalOutcomeAsync(int signalId, SignalOutcome outcome, double? actualReturn = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get signal performance statistics
    /// </summary>
    Task<SignalPerformanceStats> GetSignalPerformanceAsync(SignalTrigger? trigger = null, TimeSpan? period = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark signal as actioned
    /// </summary>
    Task MarkSignalActionedAsync(int signalId, string actionTaken, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for seasonal patterns
/// </summary>
public interface ISeasonalPatternRepository : IRepository<SeasonalPattern>
{
    /// <summary>
    /// Get active seasonal patterns for current period
    /// </summary>
    Task<List<SeasonalPattern>> GetActiveSeasonalPatternsAsync(int? typeId = null, int? regionId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get patterns by period type
    /// </summary>
    Task<List<SeasonalPattern>> GetPatternsByPeriodAsync(SeasonalPeriod period, double minReliability = 0.6, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get patterns for specific timeframe
    /// </summary>
    Task<List<SeasonalPattern>> GetPatternsForTimeframeAsync(DateTime startDate, DateTime endDate, int? regionId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update pattern reliability
    /// </summary>
    Task UpdatePatternReliabilityAsync(int patternId, double newReliability, CancellationToken cancellationToken = default);

    /// <summary>
    /// Find similar patterns
    /// </summary>
    Task<List<SeasonalPattern>> FindSimilarPatternsAsync(SeasonalPattern pattern, double similarityThreshold = 0.8, CancellationToken cancellationToken = default);
}

/// <summary>
/// Data transfer objects for repository operations
/// </summary>
public class TrendStatistics
{
    public int RegionId { get; set; }
    public string RegionName { get; set; } = string.Empty;
    public int TotalAnalyses { get; set; }
    public int BullishTrends { get; set; }
    public int BearishTrends { get; set; }
    public int SidewaysTrends { get; set; }
    public double AverageVolatility { get; set; }
    public double AverageLiquidity { get; set; }
    public int AnomalyCount { get; set; }
    public Dictionary<MarketRegime, int> RegimeDistribution { get; set; } = new();
    public DateTime CalculatedDate { get; set; } = DateTime.UtcNow;
}

public class PredictionValidationResult
{
    public int PredictionId { get; set; }
    public bool IsValidated { get; set; }
    public double PredictionAccuracy { get; set; }
    public double PriceError { get; set; }
    public double VolumeError { get; set; }
    public bool TrendDirectionCorrect { get; set; }
    public DateTime ValidationDate { get; set; } = DateTime.UtcNow;
}

public class PredictionAccuracyStats
{
    public PredictionAlgorithm? Algorithm { get; set; }
    public TimeSpan Period { get; set; }
    public int TotalPredictions { get; set; }
    public int ValidatedPredictions { get; set; }
    public double OverallAccuracy { get; set; }
    public double PriceAccuracy { get; set; }
    public double VolumeAccuracy { get; set; }
    public double DirectionAccuracy { get; set; }
    public double MeanAbsoluteError { get; set; }
    public double RootMeanSquaredError { get; set; }
    public Dictionary<PredictionRisk, double> AccuracyByRisk { get; set; } = new();
    public Dictionary<SignalTimeframe, double> AccuracyByTimeframe { get; set; } = new();
}

public class SignalPerformanceStats
{
    public SignalTrigger? Trigger { get; set; }
    public TimeSpan Period { get; set; }
    public int TotalSignals { get; set; }
    public int ValidatedSignals { get; set; }
    public double SuccessRate { get; set; }
    public double AverageReturn { get; set; }
    public double WinRate { get; set; }
    public double AverageWin { get; set; }
    public double AverageLoss { get; set; }
    public double MaxDrawdown { get; set; }
    public double SharpeRatio { get; set; }
    public Dictionary<SignalTimeframe, double> PerformanceByTimeframe { get; set; } = new();
    public Dictionary<SignalPriority, double> PerformanceByPriority { get; set; } = new();
}