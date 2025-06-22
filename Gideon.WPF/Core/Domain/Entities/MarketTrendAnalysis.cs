// ==========================================================================
// MarketTrendAnalysis.cs - Market Trend Analysis Entities
// ==========================================================================
// Domain entities for market trend analysis, prediction algorithms, and 
// advanced market intelligence for EVE Online market data.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gideon.WPF.Core.Domain.Entities;

/// <summary>
/// Market trend analysis data
/// </summary>
[Table("MarketTrendAnalysis")]
public class MarketTrendAnalysis
{
    [Key]
    public int AnalysisId { get; set; }

    public int TypeId { get; set; }

    public int RegionId { get; set; }

    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;

    public TimeSpan AnalysisPeriod { get; set; } = TimeSpan.FromDays(30);

    // Price Trend Analysis
    public TrendDirection PriceTrend { get; set; }

    public double PriceTrendStrength { get; set; } // 0.0 to 1.0

    public decimal AveragePrice { get; set; }

    public decimal MedianPrice { get; set; }

    public decimal PriceVolatility { get; set; } // Standard deviation

    public decimal PriceChangePercent { get; set; }

    public decimal PriceChangeAbsolute { get; set; }

    // Volume Trend Analysis
    public TrendDirection VolumeTrend { get; set; }

    public double VolumeTrendStrength { get; set; }

    public long AverageVolume { get; set; }

    public long MedianVolume { get; set; }

    public double VolumeVolatility { get; set; }

    public double VolumeChangePercent { get; set; }

    // Market Depth Analysis
    public decimal SpreadAverage { get; set; }

    public decimal SpreadMedian { get; set; }

    public double SpreadVolatility { get; set; }

    public double MarketDepthScore { get; set; } // 0.0 to 1.0

    // Liquidity Analysis
    public double LiquidityScore { get; set; } // 0.0 to 1.0

    public TimeSpan AverageOrderLifetime { get; set; }

    public int ActiveOrderCount { get; set; }

    public int UniqueTraders { get; set; }

    // Statistical Indicators
    public decimal RSI { get; set; } // Relative Strength Index (0-100)

    public decimal MACD { get; set; } // Moving Average Convergence Divergence

    public decimal MACDSignal { get; set; }

    public decimal MACDHistogram { get; set; }

    public decimal BollingerUpper { get; set; }

    public decimal BollingerMiddle { get; set; }

    public decimal BollingerLower { get; set; }

    public decimal SMA20 { get; set; } // 20-day Simple Moving Average

    public decimal SMA50 { get; set; } // 50-day Simple Moving Average

    public decimal EMA12 { get; set; } // 12-day Exponential Moving Average

    public decimal EMA26 { get; set; } // 26-day Exponential Moving Average

    // Market Behavior Analysis
    public MarketRegime MarketRegime { get; set; }

    public double TrendConsistency { get; set; } // How consistent the trend is

    public int TrendDurationDays { get; set; }

    public DateTime? TrendStartDate { get; set; }

    public double SeasonalityScore { get; set; } // Seasonal pattern strength

    // Support and Resistance Levels
    public decimal? SupportLevel1 { get; set; }

    public decimal? SupportLevel2 { get; set; }

    public decimal? ResistanceLevel1 { get; set; }

    public decimal? ResistanceLevel2 { get; set; }

    public double SupportStrength { get; set; }

    public double ResistanceStrength { get; set; }

    // Anomaly Detection
    public bool HasAnomalies { get; set; }

    public int AnomalyCount { get; set; }

    public string? AnomalyDescription { get; set; }

    public double AnomalyScore { get; set; }

    // Analysis Metadata
    public int DataPointsAnalyzed { get; set; }

    public double AnalysisConfidence { get; set; } // 0.0 to 1.0

    public string? AnalysisNotes { get; set; }

    public TrendAnalysisQuality QualityScore { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("TypeId")]
    public virtual EveType? ItemType { get; set; }

    [ForeignKey("RegionId")]
    public virtual EveRegion? Region { get; set; }

    public virtual ICollection<MarketPrediction> Predictions { get; set; } = new List<MarketPrediction>();

    public virtual ICollection<TrendSignal> TrendSignals { get; set; } = new List<TrendSignal>();
}

/// <summary>
/// Market prediction data
/// </summary>
[Table("MarketPredictions")]
public class MarketPrediction
{
    [Key]
    public int PredictionId { get; set; }

    public int TrendAnalysisId { get; set; }

    public int TypeId { get; set; }

    public int RegionId { get; set; }

    public DateTime PredictionDate { get; set; } = DateTime.UtcNow;

    public DateTime TargetDate { get; set; }

    public TimeSpan PredictionHorizon { get; set; }

    // Price Predictions
    public decimal PredictedPrice { get; set; }

    public decimal PredictedPriceMin { get; set; }

    public decimal PredictedPriceMax { get; set; }

    public double PricePredictionConfidence { get; set; } // 0.0 to 1.0

    // Volume Predictions
    public long PredictedVolume { get; set; }

    public long PredictedVolumeMin { get; set; }

    public long PredictedVolumeMax { get; set; }

    public double VolumePredictionConfidence { get; set; }

    // Trend Predictions
    public TrendDirection PredictedTrend { get; set; }

    public double TrendConfidence { get; set; }

    public int ExpectedTrendDurationDays { get; set; }

    // Market Condition Predictions
    public MarketRegime PredictedRegime { get; set; }

    public double VolatilityForecast { get; set; }

    public double LiquidityForecast { get; set; }

    // Algorithm Information
    public PredictionAlgorithm Algorithm { get; set; }

    public string AlgorithmVersion { get; set; } = "1.0";

    public string? AlgorithmParameters { get; set; } // JSON serialized

    // Accuracy Tracking
    public double? ActualPrice { get; set; }

    public long? ActualVolume { get; set; }

    public TrendDirection? ActualTrend { get; set; }

    public double? PredictionAccuracy { get; set; }

    public bool IsValidated { get; set; }

    public DateTime? ValidationDate { get; set; }

    // Risk Assessment
    public PredictionRisk RiskLevel { get; set; }

    public double DownsideRisk { get; set; }

    public double UpsideRisk { get; set; }

    public decimal MaxExpectedLoss { get; set; }

    public decimal MaxExpectedGain { get; set; }

    // Trading Signals
    public TradingSignal Signal { get; set; }

    public double SignalStrength { get; set; } // 0.0 to 1.0

    public string? SignalReasoning { get; set; }

    // Metadata
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime ExpiryDate { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation Properties
    [ForeignKey("TrendAnalysisId")]
    public virtual MarketTrendAnalysis? TrendAnalysis { get; set; }

    [ForeignKey("TypeId")]
    public virtual EveType? ItemType { get; set; }

    [ForeignKey("RegionId")]
    public virtual EveRegion? Region { get; set; }
}

/// <summary>
/// Market trend signals
/// </summary>
[Table("TrendSignals")]
public class TrendSignal
{
    [Key]
    public int SignalId { get; set; }

    public int TrendAnalysisId { get; set; }

    public int TypeId { get; set; }

    public int RegionId { get; set; }

    public DateTime SignalDate { get; set; } = DateTime.UtcNow;

    // Signal Information
    public TradingSignal SignalType { get; set; }

    public SignalTrigger Trigger { get; set; }

    public double SignalStrength { get; set; } // 0.0 to 1.0

    public double Confidence { get; set; } // 0.0 to 1.0

    // Price Context
    public decimal PriceAtSignal { get; set; }

    public long VolumeAtSignal { get; set; }

    // Technical Indicators at Signal
    public decimal RSIAtSignal { get; set; }

    public decimal MACDAtSignal { get; set; }

    public decimal BollingerPositionAtSignal { get; set; } // 0.0 to 1.0 (position between bands)

    // Signal Details
    public string SignalDescription { get; set; } = string.Empty;

    public string? TechnicalReasoning { get; set; }

    public SignalTimeframe Timeframe { get; set; }

    // Target and Stop Levels
    public decimal? TargetPrice { get; set; }

    public decimal? StopLossPrice { get; set; }

    public decimal? RiskRewardRatio { get; set; }

    // Signal Validation
    public bool IsValidated { get; set; }

    public DateTime? ValidationDate { get; set; }

    public SignalOutcome? Outcome { get; set; }

    public double? ActualReturn { get; set; }

    public TimeSpan? SignalDuration { get; set; }

    // Priority and Action
    public SignalPriority Priority { get; set; }

    public bool RequiresAction { get; set; }

    public DateTime? ActionTakenDate { get; set; }

    public string? ActionTaken { get; set; }

    // Metadata
    public bool IsActive { get; set; } = true;

    public DateTime ExpiryDate { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("TrendAnalysisId")]
    public virtual MarketTrendAnalysis? TrendAnalysis { get; set; }

    [ForeignKey("TypeId")]
    public virtual EveType? ItemType { get; set; }

    [ForeignKey("RegionId")]
    public virtual EveRegion? Region { get; set; }
}

/// <summary>
/// Seasonal market patterns
/// </summary>
[Table("SeasonalPatterns")]
public class SeasonalPattern
{
    [Key]
    public int PatternId { get; set; }

    public int TypeId { get; set; }

    public int RegionId { get; set; }

    // Pattern Information
    public SeasonalPeriod Period { get; set; }

    public string PatternName { get; set; } = string.Empty;

    public string PatternDescription { get; set; } = string.Empty;

    // Timing Information
    public int StartDay { get; set; } // Day of year (1-365)

    public int EndDay { get; set; } // Day of year (1-365)

    public int StartHour { get; set; } // Hour of day (0-23) for intraday patterns

    public int EndHour { get; set; } // Hour of day (0-23) for intraday patterns

    public DayOfWeek? DayOfWeek { get; set; } // For weekly patterns

    public int? MonthOfYear { get; set; } // For monthly/yearly patterns

    // Pattern Characteristics
    public double PatternStrength { get; set; } // 0.0 to 1.0

    public double Reliability { get; set; } // Historical reliability (0.0 to 1.0)

    public int OccurrenceCount { get; set; } // Number of times observed

    public double AveragePriceChange { get; set; }

    public double AverageVolumeChange { get; set; }

    public double PriceVolatility { get; set; }

    public double VolumeVolatility { get; set; }

    // Statistical Validation
    public double StatisticalSignificance { get; set; } // p-value

    public double CorrelationCoefficient { get; set; }

    public int SampleSize { get; set; }

    public DateTime FirstObserved { get; set; }

    public DateTime LastObserved { get; set; }

    // Pattern Metadata
    public bool IsActive { get; set; } = true;

    public string? Notes { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("TypeId")]
    public virtual EveType? ItemType { get; set; }

    [ForeignKey("RegionId")]
    public virtual EveRegion? Region { get; set; }
}

/// <summary>
/// Market algorithm performance tracking
/// </summary>
[Table("AlgorithmPerformance")]
public class AlgorithmPerformance
{
    [Key]
    public int PerformanceId { get; set; }

    public PredictionAlgorithm Algorithm { get; set; }

    public string AlgorithmVersion { get; set; } = string.Empty;

    public DateTime TestPeriodStart { get; set; }

    public DateTime TestPeriodEnd { get; set; }

    // Accuracy Metrics
    public double OverallAccuracy { get; set; } // 0.0 to 1.0

    public double PricePredictionAccuracy { get; set; }

    public double VolumePredictionAccuracy { get; set; }

    public double TrendPredictionAccuracy { get; set; }

    public double DirectionAccuracy { get; set; } // % of correct direction predictions

    // Error Metrics
    public double MeanAbsoluteError { get; set; }

    public double MeanSquaredError { get; set; }

    public double RootMeanSquaredError { get; set; }

    public double MeanAbsolutePercentageError { get; set; }

    // Performance Statistics
    public int TotalPredictions { get; set; }

    public int CorrectPredictions { get; set; }

    public int IncorrectPredictions { get; set; }

    public int PendingValidation { get; set; }

    // Risk-Adjusted Performance
    public double SharpeRatio { get; set; }

    public double MaxDrawdown { get; set; }

    public double WinRate { get; set; }

    public double AverageWin { get; set; }

    public double AverageLoss { get; set; }

    public double ProfitFactor { get; set; }

    // Timeframe Performance
    public double ShortTermAccuracy { get; set; } // < 1 day

    public double MediumTermAccuracy { get; set; } // 1-7 days

    public double LongTermAccuracy { get; set; } // > 7 days

    // Market Condition Performance
    public double TrendingMarketAccuracy { get; set; }

    public double SidewaysMarketAccuracy { get; set; }

    public double VolatileMarketAccuracy { get; set; }

    // Algorithm Characteristics
    public TimeSpan AverageProcessingTime { get; set; }

    public int ComputationalComplexity { get; set; } // 1-10 scale

    public double ResourceUtilization { get; set; } // CPU/Memory usage

    // Metadata
    public string? Notes { get; set; }

    public DateTime LastEvaluated { get; set; } = DateTime.UtcNow;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Enumerations for market trend analysis
/// </summary>
public enum TrendDirection
{
    StronglyBearish = -2,
    Bearish = -1,
    Sideways = 0,
    Bullish = 1,
    StronglyBullish = 2
}

public enum MarketRegime
{
    Trending,
    Ranging,
    Volatile,
    LowVolatility,
    Breakout,
    Consolidation,
    Reversal
}

public enum TrendAnalysisQuality
{
    Excellent,
    Good,
    Fair,
    Poor,
    Insufficient
}

public enum PredictionAlgorithm
{
    SimpleMovingAverage,
    ExponentialMovingAverage,
    LinearRegression,
    PolynomialRegression,
    ARIMA,
    LSTM,
    RandomForest,
    SupportVectorMachine,
    EnsembleMethod,
    TechnicalAnalysis,
    FundamentalAnalysis,
    HybridModel
}

public enum PredictionRisk
{
    VeryLow,
    Low,
    Medium,
    High,
    VeryHigh
}

public enum TradingSignal
{
    StrongBuy,
    Buy,
    Hold,
    Sell,
    StrongSell,
    NoSignal
}

public enum SignalTrigger
{
    RSIOverbought,
    RSIOversold,
    MACDCrossover,
    BollingerBandBreakout,
    SupportBreakdown,
    ResistanceBreakout,
    VolumeSpike,
    PriceGap,
    TrendReversal,
    PatternCompletion,
    MovingAverageCrossover,
    SeasonalPattern,
    AnomalyDetection
}

public enum SignalTimeframe
{
    Immediate,      // < 1 hour
    Intraday,       // 1-24 hours
    ShortTerm,      // 1-7 days
    MediumTerm,     // 1-4 weeks
    LongTerm        // > 1 month
}

public enum SignalOutcome
{
    Success,
    Failure,
    Partial,
    Cancelled,
    Pending
}

public enum SignalPriority
{
    Critical,
    High,
    Medium,
    Low
}

public enum SeasonalPeriod
{
    Intraday,       // Within a day
    Daily,          // Day of week patterns
    Weekly,         // Week of month patterns
    Monthly,        // Month of year patterns
    Quarterly,      // Quarterly patterns
    Yearly,         // Annual patterns
    Custom          // Custom defined periods
}