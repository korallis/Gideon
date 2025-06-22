// ==========================================================================
// IMarketAnalysisService.cs - Market Analysis Service Interface
// ==========================================================================
// Advanced service interface for market analysis, predictions, and trading intelligence.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Gideon.WPF.Core.Domain.Entities;

namespace Gideon.WPF.Core.Application.Services;

/// <summary>
/// Service interface for advanced market analysis and prediction
/// </summary>
public interface IMarketAnalysisService
{
    /// <summary>
    /// Get comprehensive market analysis for an item
    /// </summary>
    Task<MarketAnalysisResult> GetMarketAnalysisAsync(int typeId, int regionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get trading opportunities with profit analysis
    /// </summary>
    Task<IEnumerable<TradingOpportunity>> GetTradingOpportunitiesAsync(
        double minProfitMargin = 0.1, 
        OpportunityRisk maxRisk = OpportunityRisk.Medium, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate market price predictions using AI analysis
    /// </summary>
    Task<MarketPrediction> GeneratePricePredictionAsync(
        int typeId, 
        int regionId, 
        PredictionTimeframe timeframe, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyze price trends and patterns
    /// </summary>
    Task<PriceTrendAnalysis> AnalyzePriceTrendsAsync(
        int typeId, 
        int regionId, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate profit margins for trading routes
    /// </summary>
    Task<ProfitAnalysisResult> CalculateProfitMarginsAsync(
        int typeId, 
        int sourceRegionId, 
        int destinationRegionId, 
        long quantity = 1, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get market volatility analysis
    /// </summary>
    Task<VolatilityAnalysis> GetVolatilityAnalysisAsync(
        int typeId, 
        int regionId, 
        int days = 30, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Identify arbitrage opportunities across regions
    /// </summary>
    Task<IEnumerable<ArbitrageOpportunity>> FindArbitrageOpportunitiesAsync(
        int typeId, 
        double minProfitThreshold = 1000000, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get market depth analysis (order book analysis)
    /// </summary>
    Task<MarketDepthAnalysis> GetMarketDepthAsync(int typeId, int regionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyze market manipulation indicators
    /// </summary>
    Task<ManipulationRisk> AnalyzeManipulationRiskAsync(int typeId, int regionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get competitive pricing recommendations
    /// </summary>
    Task<PricingRecommendation> GetPricingRecommendationAsync(
        int typeId, 
        int regionId, 
        OrderType orderType, 
        long quantity, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Track portfolio performance
    /// </summary>
    Task<PortfolioAnalysis> AnalyzePortfolioAsync(int characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update market data from ESI and process analysis
    /// </summary>
    Task UpdateMarketDataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get regional market comparison
    /// </summary>
    Task<RegionalComparison> CompareRegionalMarketsAsync(int typeId, IEnumerable<int> regionIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get historical price data with aggregation
    /// </summary>
    Task<IEnumerable<HistoricalPriceData>> GetHistoricalPricesAsync(
        int typeId, 
        int regionId, 
        DateTime fromDate, 
        DateTime toDate, 
        HistoricalDataInterval interval = HistoricalDataInterval.Daily,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get historical volume data with aggregation
    /// </summary>
    Task<IEnumerable<HistoricalVolumeData>> GetHistoricalVolumeAsync(
        int typeId, 
        int regionId, 
        DateTime fromDate, 
        DateTime toDate, 
        HistoricalDataInterval interval = HistoricalDataInterval.Daily,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get comprehensive market statistics for time period
    /// </summary>
    Task<MarketStatistics> GetMarketStatisticsAsync(
        int typeId, 
        int regionId, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Comprehensive market analysis result
/// </summary>
public class MarketAnalysisResult
{
    public int TypeId { get; set; }
    public int RegionId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string RegionName { get; set; } = string.Empty;
    
    public CurrentMarketState CurrentState { get; set; } = new();
    public PriceTrendAnalysis TrendAnalysis { get; set; } = new();
    public VolatilityAnalysis Volatility { get; set; } = new();
    public MarketDepthAnalysis Depth { get; set; } = new();
    public ManipulationRisk ManipulationRisk { get; set; } = new();
    public List<MarketPrediction> Predictions { get; set; } = new();
    
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan AnalysisTime { get; set; }
}

/// <summary>
/// Current market state snapshot
/// </summary>
public class CurrentMarketState
{
    public decimal BestBuyPrice { get; set; }
    public decimal BestSellPrice { get; set; }
    public decimal Spread { get; set; }
    public double SpreadPercentage { get; set; }
    public long TotalBuyVolume { get; set; }
    public long TotalSellVolume { get; set; }
    public int ActiveBuyOrders { get; set; }
    public int ActiveSellOrders { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal MedianPrice { get; set; }
    public long DailyVolume { get; set; }
    public decimal DailyIskVolume { get; set; }
}

/// <summary>
/// Price trend analysis with technical indicators
/// </summary>
public class PriceTrendAnalysis
{
    public TrendDirection Direction { get; set; }
    public double TrendStrength { get; set; }
    public decimal SupportLevel { get; set; }
    public decimal ResistanceLevel { get; set; }
    public decimal MovingAverage7d { get; set; }
    public decimal MovingAverage30d { get; set; }
    public decimal MovingAverage90d { get; set; }
    public double RSI { get; set; } // Relative Strength Index
    public double MACD { get; set; } // Moving Average Convergence Divergence
    public List<PricePoint> HistoricalPrices { get; set; } = new();
    public List<VolumePoint> HistoricalVolume { get; set; } = new();
}

/// <summary>
/// Market volatility analysis
/// </summary>
public class VolatilityAnalysis
{
    public double DailyVolatility { get; set; }
    public double WeeklyVolatility { get; set; }
    public double MonthlyVolatility { get; set; }
    public double StandardDeviation { get; set; }
    public double CoefficientOfVariation { get; set; }
    public VolatilityRating Rating { get; set; }
    public List<VolatilityPeriod> VolatilityHistory { get; set; } = new();
}

/// <summary>
/// Market depth analysis (order book)
/// </summary>
public class MarketDepthAnalysis
{
    public List<MarketDepthLevel> BuyLevels { get; set; } = new();
    public List<MarketDepthLevel> SellLevels { get; set; } = new();
    public decimal ImpactPrice { get; set; } // Price impact for large orders
    public long LiquidityScore { get; set; }
    public double Slippage { get; set; } // Expected slippage percentage
    public MarketLiquidity LiquidityRating { get; set; }
}

/// <summary>
/// Market manipulation risk assessment
/// </summary>
public class ManipulationRisk
{
    public RiskLevel RiskLevel { get; set; }
    public double RiskScore { get; set; }
    public List<string> RiskFactors { get; set; } = new();
    public List<string> RedFlags { get; set; } = new();
    public bool SuspiciousActivity { get; set; }
    public DateTime LastAssessment { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Profit analysis result for trading
/// </summary>
public class ProfitAnalysisResult
{
    public decimal BuyPrice { get; set; }
    public decimal SellPrice { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal NetProfit { get; set; }
    public double ProfitMargin { get; set; }
    public decimal TransactionTax { get; set; }
    public decimal BrokerFee { get; set; }
    public decimal TransportCost { get; set; }
    public TimeSpan EstimatedTurnoverTime { get; set; }
    public double ROI { get; set; } // Return on Investment
    public double DailyROI { get; set; }
    public OpportunityRisk Risk { get; set; }
}

/// <summary>
/// Arbitrage opportunity between regions
/// </summary>
public class ArbitrageOpportunity
{
    public int TypeId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int SourceRegionId { get; set; }
    public string SourceRegionName { get; set; } = string.Empty;
    public int DestinationRegionId { get; set; }
    public string DestinationRegionName { get; set; } = string.Empty;
    public decimal BuyPrice { get; set; }
    public decimal SellPrice { get; set; }
    public decimal ProfitPerUnit { get; set; }
    public double ProfitMargin { get; set; }
    public long MaxVolume { get; set; }
    public decimal TotalProfit { get; set; }
    public OpportunityRisk Risk { get; set; }
    public double JumpDistance { get; set; }
    public TimeSpan EstimatedTravelTime { get; set; }
}

/// <summary>
/// Pricing recommendation for orders
/// </summary>
public class PricingRecommendation
{
    public decimal RecommendedPrice { get; set; }
    public decimal ConservativePrice { get; set; }
    public decimal AggressivePrice { get; set; }
    public string Strategy { get; set; } = string.Empty;
    public string Reasoning { get; set; } = string.Empty;
    public TimeSpan EstimatedFillTime { get; set; }
    public double FillProbability { get; set; }
    public List<PriceAlternative> Alternatives { get; set; } = new();
}

/// <summary>
/// Portfolio performance analysis
/// </summary>
public class PortfolioAnalysis
{
    public int CharacterId { get; set; }
    public decimal TotalValue { get; set; }
    public decimal TotalProfit { get; set; }
    public double TotalReturn { get; set; }
    public decimal DailyProfit { get; set; }
    public decimal WeeklyProfit { get; set; }
    public decimal MonthlyProfit { get; set; }
    public List<PortfolioPosition> Positions { get; set; } = new();
    public List<TradePerformance> RecentTrades { get; set; } = new();
    public PortfolioMetrics Metrics { get; set; } = new();
}

/// <summary>
/// Regional market comparison
/// </summary>
public class RegionalComparison
{
    public int TypeId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public List<RegionalMarketData> RegionalData { get; set; } = new();
    public RegionalMarketData BestBuyRegion { get; set; } = new();
    public RegionalMarketData BestSellRegion { get; set; } = new();
    public decimal MaxPriceDifference { get; set; }
    public double MaxProfitMargin { get; set; }
}

/// <summary>
/// Supporting data structures
/// </summary>
public class PricePoint
{
    public DateTime Date { get; set; }
    public decimal Price { get; set; }
}

public class VolumePoint
{
    public DateTime Date { get; set; }
    public long Volume { get; set; }
    public decimal IskVolume { get; set; }
}

public class VolatilityPeriod
{
    public DateTime Date { get; set; }
    public double Volatility { get; set; }
}

public class MarketDepthLevel
{
    public decimal Price { get; set; }
    public long Volume { get; set; }
    public int OrderCount { get; set; }
}

public class PriceAlternative
{
    public decimal Price { get; set; }
    public TimeSpan EstimatedFillTime { get; set; }
    public double FillProbability { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class PortfolioPosition
{
    public int TypeId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public long Quantity { get; set; }
    public decimal AverageBuyPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal UnrealizedProfit { get; set; }
    public double UnrealizedReturn { get; set; }
}

public class TradePerformance
{
    public DateTime TradeDate { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public long Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Profit { get; set; }
    public double Return { get; set; }
}

public class PortfolioMetrics
{
    public double SharpeRatio { get; set; }
    public double MaxDrawdown { get; set; }
    public double WinRate { get; set; }
    public decimal AverageProfit { get; set; }
    public decimal AverageLoss { get; set; }
    public double ProfitFactor { get; set; }
}

public class RegionalMarketData
{
    public int RegionId { get; set; }
    public string RegionName { get; set; } = string.Empty;
    public decimal BestBuyPrice { get; set; }
    public decimal BestSellPrice { get; set; }
    public long Volume { get; set; }
    public int ActiveOrders { get; set; }
    public double SecurityRating { get; set; }
}

/// <summary>
/// Enumerations for market analysis
/// </summary>
public enum TrendDirection
{
    StronglyBearish,
    Bearish,
    Neutral,
    Bullish,
    StronglyBullish
}

public enum VolatilityRating
{
    VeryLow,
    Low,
    Moderate,
    High,
    VeryHigh
}

public enum MarketLiquidity
{
    VeryLow,
    Low,
    Moderate,
    High,
    VeryHigh
}

public enum RiskLevel
{
    VeryLow,
    Low,
    Moderate,
    High,
    VeryHigh
}