// ==========================================================================
// IMarketTrendAnalysisService.cs - Market Trend Analysis Service Interface
// ==========================================================================
// Service interface for market trend analysis and prediction algorithms.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Core.Domain.Interfaces;

namespace Gideon.WPF.Core.Application.Services;

/// <summary>
/// Service interface for market trend analysis and prediction
/// </summary>
public interface IMarketTrendAnalysisService
{
    /// <summary>
    /// Analyze market trends for specific item and region
    /// </summary>
    Task<MarketTrendAnalysisResult> AnalyzeMarketTrendAsync(int typeId, int regionId, TimeSpan analysisPeriod, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate market predictions using multiple algorithms
    /// </summary>
    Task<MarketPredictionResult> GeneratePredictionsAsync(int typeId, int regionId, List<TimeSpan> predictionHorizons, CancellationToken cancellationToken = default);

    /// <summary>
    /// Detect market anomalies
    /// </summary>
    Task<MarketAnomalyDetectionResult> DetectMarketAnomaliesAsync(int typeId, int regionId, TimeSpan lookbackPeriod, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate trading signals based on technical analysis
    /// </summary>
    Task<TradingSignalResult> GenerateTradingSignalsAsync(int typeId, int regionId, List<SignalTrigger> enabledTriggers, CancellationToken cancellationToken = default);

    /// <summary>
    /// Identify seasonal patterns
    /// </summary>
    Task<SeasonalPatternResult> IdentifySeasonalPatternsAsync(int typeId, int regionId, TimeSpan historicalPeriod, CancellationToken cancellationToken = default);

    /// <summary>
    /// Perform comprehensive market analysis
    /// </summary>
    Task<ComprehensiveMarketAnalysis> PerformComprehensiveAnalysisAsync(int typeId, int regionId, AnalysisConfiguration config, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk analyze multiple items
    /// </summary>
    Task<BulkAnalysisResult> BulkAnalyzeItemsAsync(List<int> typeIds, int regionId, AnalysisConfiguration config, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing trend analysis
    /// </summary>
    Task<bool> UpdateTrendAnalysisAsync(int analysisId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate prediction accuracy
    /// </summary>
    Task<PredictionValidationResult> ValidatePredictionsAsync(DateTime validationDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get market intelligence summary
    /// </summary>
    Task<MarketIntelligenceSummary> GetMarketIntelligenceAsync(int regionId, MarketIntelligenceRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get algorithm performance metrics
    /// </summary>
    Task<AlgorithmPerformanceReport> GetAlgorithmPerformanceAsync(PredictionAlgorithm? algorithm = null, TimeSpan? period = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Optimize algorithm parameters
    /// </summary>
    Task<AlgorithmOptimizationResult> OptimizeAlgorithmParametersAsync(PredictionAlgorithm algorithm, OptimizationConfiguration config, CancellationToken cancellationToken = default);

    /// <summary>
    /// Export analysis results
    /// </summary>
    Task<AnalysisExportResult> ExportAnalysisAsync(AnalysisExportRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get trending items in region
    /// </summary>
    Task<List<TrendingItem>> GetTrendingItemsAsync(int regionId, TrendingCriteria criteria, CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedule automatic analysis
    /// </summary>
    Task<bool> ScheduleAnalysisAsync(AnalysisScheduleRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Market trend analysis result
/// </summary>
public class MarketTrendAnalysisResult
{
    public bool Success { get; set; }
    public MarketTrendAnalysis? Analysis { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
    public AnalysisQualityMetrics Quality { get; set; } = new();
}

/// <summary>
/// Market prediction result
/// </summary>
public class MarketPredictionResult
{
    public bool Success { get; set; }
    public List<MarketPrediction> Predictions { get; set; } = new();
    public PredictionEnsemble? EnsemblePrediction { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
    public DateTime PredictionDate { get; set; } = DateTime.UtcNow;
    public Dictionary<PredictionAlgorithm, double> AlgorithmConfidence { get; set; } = new();
}

/// <summary>
/// Market anomaly detection result
/// </summary>
public class MarketAnomalyDetectionResult
{
    public bool Success { get; set; }
    public bool HasAnomalies { get; set; }
    public List<MarketAnomaly> Anomalies { get; set; } = new();
    public double OverallAnomalyScore { get; set; }
    public AnomalyStatistics Statistics { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public DateTime DetectionDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Trading signal result
/// </summary>
public class TradingSignalResult
{
    public bool Success { get; set; }
    public List<TrendSignal> Signals { get; set; } = new();
    public TradingSignal OverallSignal { get; set; }
    public double SignalStrength { get; set; }
    public double Confidence { get; set; }
    public string SignalReasoning { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
    public DateTime SignalDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Seasonal pattern result
/// </summary>
public class SeasonalPatternResult
{
    public bool Success { get; set; }
    public List<SeasonalPattern> Patterns { get; set; } = new();
    public double OverallSeasonality { get; set; }
    public List<PatternInsight> Insights { get; set; } = new();
    public PatternStatistics Statistics { get; set; } = new();
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Comprehensive market analysis
/// </summary>
public class ComprehensiveMarketAnalysis
{
    public bool Success { get; set; }
    public MarketTrendAnalysis TrendAnalysis { get; set; } = new();
    public List<MarketPrediction> Predictions { get; set; } = new();
    public List<TrendSignal> Signals { get; set; } = new();
    public List<SeasonalPattern> SeasonalPatterns { get; set; } = new();
    public List<MarketAnomaly> Anomalies { get; set; } = new();
    public MarketHealthScore HealthScore { get; set; } = new();
    public List<MarketInsight> Insights { get; set; } = new();
    public List<TradingRecommendation> Recommendations { get; set; } = new();
    public TimeSpan TotalProcessingTime { get; set; }
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Bulk analysis result
/// </summary>
public class BulkAnalysisResult
{
    public bool Success { get; set; }
    public int TotalItems { get; set; }
    public int SuccessfulAnalyses { get; set; }
    public int FailedAnalyses { get; set; }
    public List<MarketTrendAnalysis> Results { get; set; } = new();
    public List<AnalysisError> Errors { get; set; } = new();
    public TimeSpan TotalProcessingTime { get; set; }
    public BulkAnalysisStatistics Statistics { get; set; } = new();
    public DateTime CompletionDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Market intelligence summary
/// </summary>
public class MarketIntelligenceSummary
{
    public int RegionId { get; set; }
    public string RegionName { get; set; } = string.Empty;
    public DateTime SummaryDate { get; set; } = DateTime.UtcNow;
    
    public MarketOverview Overview { get; set; } = new();
    public List<TrendingItem> TopTrendingItems { get; set; } = new();
    public List<TradingOpportunity> TopOpportunities { get; set; } = new();
    public List<MarketAlert> Alerts { get; set; } = new();
    public MarketSentiment Sentiment { get; set; } = new();
    public List<MarketInsight> KeyInsights { get; set; } = new();
    public RiskAssessment RiskAssessment { get; set; } = new();
}

/// <summary>
/// Algorithm performance report
/// </summary>
public class AlgorithmPerformanceReport
{
    public PredictionAlgorithm? Algorithm { get; set; }
    public TimeSpan ReportPeriod { get; set; }
    public AlgorithmPerformance Performance { get; set; } = new();
    public List<PerformanceMetric> Metrics { get; set; } = new();
    public List<PerformanceComparison> Comparisons { get; set; } = new();
    public List<PerformanceInsight> Insights { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public DateTime ReportDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Configuration classes
/// </summary>
public class AnalysisConfiguration
{
    public TimeSpan AnalysisPeriod { get; set; } = TimeSpan.FromDays(30);
    public List<PredictionAlgorithm> EnabledAlgorithms { get; set; } = new();
    public List<SignalTrigger> EnabledSignals { get; set; } = new();
    public bool DetectAnomalies { get; set; } = true;
    public bool IdentifySeasonalPatterns { get; set; } = true;
    public double MinConfidenceThreshold { get; set; } = 0.6;
    public int MaxPredictionHorizonDays { get; set; } = 30;
    public bool GenerateRecommendations { get; set; } = true;
    public Dictionary<string, object> CustomParameters { get; set; } = new();
}

public class MarketIntelligenceRequest
{
    public List<int>? TypeIds { get; set; }
    public List<MarketRegime>? IncludeRegimes { get; set; }
    public TrendDirection? TrendFilter { get; set; }
    public double? MinLiquidity { get; set; }
    public double? MaxVolatility { get; set; }
    public int MaxResults { get; set; } = 50;
    public bool IncludeSignals { get; set; } = true;
    public bool IncludePredictions { get; set; } = true;
    public bool IncludeAnomalies { get; set; } = true;
    public MarketIntelligenceScope Scope { get; set; } = MarketIntelligenceScope.Standard;
}

public class OptimizationConfiguration
{
    public List<OptimizationParameter> Parameters { get; set; } = new();
    public OptimizationMetric TargetMetric { get; set; } = OptimizationMetric.Accuracy;
    public TimeSpan TrainingPeriod { get; set; } = TimeSpan.FromDays(90);
    public TimeSpan ValidationPeriod { get; set; } = TimeSpan.FromDays(30);
    public int MaxIterations { get; set; } = 100;
    public double ConvergenceThreshold { get; set; } = 0.001;
    public bool UseParallelProcessing { get; set; } = true;
}

public class AnalysisExportRequest
{
    public List<int>? AnalysisIds { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<int>? TypeIds { get; set; }
    public List<int>? RegionIds { get; set; }
    public ExportFormat Format { get; set; } = ExportFormat.Json;
    public string ExportPath { get; set; } = string.Empty;
    public bool IncludePredictions { get; set; } = true;
    public bool IncludeSignals { get; set; } = true;
    public bool IncludeAnomalies { get; set; } = true;
    public bool CompressOutput { get; set; } = true;
}

public class TrendingCriteria
{
    public TrendDirection Direction { get; set; } = TrendDirection.Bullish;
    public double MinTrendStrength { get; set; } = 0.5;
    public double MinLiquidity { get; set; } = 0.3;
    public TimeSpan TrendDuration { get; set; } = TimeSpan.FromDays(7);
    public int MaxResults { get; set; } = 20;
    public bool RequireHighVolume { get; set; } = false;
    public TrendingSortOrder SortBy { get; set; } = TrendingSortOrder.TrendStrength;
}

public class AnalysisScheduleRequest
{
    public List<int> TypeIds { get; set; } = new();
    public List<int> RegionIds { get; set; } = new();
    public TimeSpan AnalysisInterval { get; set; } = TimeSpan.FromHours(6);
    public AnalysisConfiguration Configuration { get; set; } = new();
    public bool IsEnabled { get; set; } = true;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public SchedulePriority Priority { get; set; } = SchedulePriority.Normal;
}

/// <summary>
/// Supporting data classes
/// </summary>
public class PredictionEnsemble
{
    public decimal ConsensusPrice { get; set; }
    public long ConsensusVolume { get; set; }
    public TrendDirection ConsensusTrend { get; set; }
    public double EnsembleConfidence { get; set; }
    public Dictionary<PredictionAlgorithm, double> AlgorithmWeights { get; set; } = new();
    public double PredictionVariance { get; set; }
    public RiskAssessment RiskAssessment { get; set; } = new();
}

public class MarketAnomaly
{
    public int TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public AnomalyType Type { get; set; }
    public double Severity { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime DetectedDate { get; set; }
    public decimal PriceAtDetection { get; set; }
    public long VolumeAtDetection { get; set; }
    public double DeviationFromNormal { get; set; }
    public List<string> PossibleCauses { get; set; } = new();
}

public class TrendingItem
{
    public int TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public TrendDirection TrendDirection { get; set; }
    public double TrendStrength { get; set; }
    public double PriceChange { get; set; }
    public double VolumeChange { get; set; }
    public double LiquidityScore { get; set; }
    public MarketRegime Regime { get; set; }
    public List<TradingSignal> ActiveSignals { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class TradingOpportunity
{
    public int TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public OpportunityType Type { get; set; }
    public double ExpectedReturn { get; set; }
    public double Confidence { get; set; }
    public PredictionRisk RiskLevel { get; set; }
    public TimeSpan TimeHorizon { get; set; }
    public decimal EntryPrice { get; set; }
    public decimal? TargetPrice { get; set; }
    public decimal? StopLoss { get; set; }
    public string Reasoning { get; set; } = string.Empty;
}

public class MarketAlert
{
    public AlertType Type { get; set; }
    public AlertSeverity Severity { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime AlertDate { get; set; }
    public List<int> AffectedItems { get; set; } = new();
    public bool RequiresAction { get; set; }
}

/// <summary>
/// Enumerations for market intelligence
/// </summary>
public enum MarketIntelligenceScope
{
    Basic,
    Standard,
    Comprehensive,
    Professional
}

public enum TrendingSortOrder
{
    TrendStrength,
    PriceChange,
    VolumeChange,
    LiquidityScore,
    MarketCapitalization
}

public enum SchedulePriority
{
    Low,
    Normal,
    High,
    Critical
}

public enum AnomalyType
{
    PriceSpike,
    VolumeSpike,
    LiquidityDrop,
    SpreadWidening,
    UnusualPattern,
    DataGap,
    MarketManipulation
}

public enum OpportunityType
{
    Arbitrage,
    Momentum,
    Reversal,
    Breakout,
    SeasonalPattern,
    Undervalued,
    Overvalued
}

public enum AlertType
{
    TrendReversal,
    VolumeAnomaly,
    PriceBreakout,
    LiquidityAlert,
    TechnicalSignal,
    SeasonalPattern,
    MarketRegimeChange
}

public enum AlertSeverity
{
    Info,
    Warning,
    Critical
}

public enum OptimizationMetric
{
    Accuracy,
    Precision,
    Recall,
    F1Score,
    SharpeRatio,
    MaxDrawdown,
    ProfitFactor
}

public enum OptimizationParameter
{
    LookbackPeriod,
    SmoothingFactor,
    ConfidenceThreshold,
    VolatilityWindow,
    TrendStrengthThreshold,
    RSIPeriod,
    MACDFastPeriod,
    MACDSlowPeriod
}

/// <summary>
/// Supporting statistics and metrics classes
/// </summary>
public class AnalysisQualityMetrics
{
    public double DataCompleteness { get; set; }
    public double DataQuality { get; set; }
    public double StatisticalSignificance { get; set; }
    public TrendAnalysisQuality OverallQuality { get; set; }
}

public class AnomalyStatistics
{
    public int TotalAnomalies { get; set; }
    public Dictionary<AnomalyType, int> AnomaliesByType { get; set; } = new();
    public double AverageAnomalyScore { get; set; }
    public double MaxAnomalyScore { get; set; }
}

public class PatternInsight
{
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public PatternType Type { get; set; }
    public DateTime? NextExpectedOccurrence { get; set; }
}

public class PatternStatistics
{
    public int TotalPatternsFound { get; set; }
    public Dictionary<SeasonalPeriod, int> PatternsByPeriod { get; set; } = new();
    public double AverageReliability { get; set; }
    public double AverageStrength { get; set; }
}

public class MarketHealthScore
{
    public double OverallScore { get; set; }
    public double LiquidityHealth { get; set; }
    public double VolatilityHealth { get; set; }
    public double TrendHealth { get; set; }
    public double EfficiencyScore { get; set; }
    public List<string> HealthFactors { get; set; } = new();
}

public class MarketInsight
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public InsightType Type { get; set; }
    public double Confidence { get; set; }
    public InsightPriority Priority { get; set; }
    public DateTime GeneratedDate { get; set; }
}

public class TradingRecommendation
{
    public TradingSignal Action { get; set; }
    public double Confidence { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public decimal? TargetPrice { get; set; }
    public decimal? StopLoss { get; set; }
    public TimeSpan? TimeHorizon { get; set; }
    public PredictionRisk RiskLevel { get; set; }
}

public class AnalysisError
{
    public int TypeId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
    public DateTime ErrorDate { get; set; }
}

public class BulkAnalysisStatistics
{
    public double SuccessRate { get; set; }
    public TimeSpan AverageProcessingTime { get; set; }
    public Dictionary<TrendDirection, int> TrendDistribution { get; set; } = new();
    public Dictionary<MarketRegime, int> RegimeDistribution { get; set; } = new();
}

public enum PatternType
{
    Cyclic,
    Seasonal,
    Trending,
    Reversal,
    Consolidation
}

public enum InsightType
{
    TrendAnalysis,
    PriceAction,
    VolumeAnalysis,
    TechnicalIndicator,
    SeasonalPattern,
    MarketStructure,
    RiskAssessment
}

public enum InsightPriority
{
    Low,
    Medium,
    High,
    Critical
}

public class MarketOverview
{
    public int TotalItems { get; set; }
    public int ActivelyTraded { get; set; }
    public double AverageVolatility { get; set; }
    public double AverageLiquidity { get; set; }
    public Dictionary<TrendDirection, int> TrendDistribution { get; set; } = new();
    public Dictionary<MarketRegime, int> RegimeDistribution { get; set; } = new();
}

public class MarketSentiment
{
    public double OverallSentiment { get; set; } // -1.0 to 1.0
    public SentimentTrend Trend { get; set; }
    public double BullishPercentage { get; set; }
    public double BearishPercentage { get; set; }
    public double NeutralPercentage { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class RiskAssessment
{
    public double OverallRisk { get; set; }
    public double VolatilityRisk { get; set; }
    public double LiquidityRisk { get; set; }
    public double ConcentrationRisk { get; set; }
    public List<RiskFactor> RiskFactors { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

public class RiskFactor
{
    public string Name { get; set; } = string.Empty;
    public double Impact { get; set; }
    public double Probability { get; set; }
    public string Description { get; set; } = string.Empty;
}

public enum SentimentTrend
{
    Improving,
    Stable,
    Deteriorating
}

public class PerformanceMetric
{
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public MetricTrend Trend { get; set; }
}

public class PerformanceComparison
{
    public PredictionAlgorithm Algorithm1 { get; set; }
    public PredictionAlgorithm Algorithm2 { get; set; }
    public double Algorithm1Score { get; set; }
    public double Algorithm2Score { get; set; }
    public string ComparisonMetric { get; set; } = string.Empty;
    public string Winner { get; set; } = string.Empty;
}

public class PerformanceInsight
{
    public string Description { get; set; } = string.Empty;
    public InsightType Type { get; set; }
    public double Impact { get; set; }
    public List<string> Recommendations { get; set; } = new();
}

public enum MetricTrend
{
    Improving,
    Stable,
    Declining
}

public class AnalysisExportResult
{
    public bool Success { get; set; }
    public string ExportPath { get; set; } = string.Empty;
    public int ExportedRecords { get; set; }
    public long FileSizeBytes { get; set; }
    public TimeSpan ExportDuration { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class AlgorithmOptimizationResult
{
    public bool Success { get; set; }
    public PredictionAlgorithm Algorithm { get; set; }
    public Dictionary<string, object> OptimalParameters { get; set; } = new();
    public double OptimalScore { get; set; }
    public double ImprovementPercentage { get; set; }
    public int Iterations { get; set; }
    public TimeSpan OptimizationTime { get; set; }
    public List<string> Recommendations { get; set; } = new();
}