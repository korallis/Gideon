// ==========================================================================
// IPersonalMarketOrderService.cs - Personal Market Order Service Interface
// ==========================================================================
// Service interface for managing user's personal market orders with ESI integration.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Gideon.WPF.Core.Domain.Entities;

namespace Gideon.WPF.Core.Application.Services;

/// <summary>
/// Service interface for personal market order management
/// </summary>
public interface IPersonalMarketOrderService
{
    /// <summary>
    /// Synchronize market orders from ESI for a character
    /// </summary>
    Task SynchronizeOrdersAsync(int characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active orders for a character
    /// </summary>
    Task<IEnumerable<PersonalMarketOrder>> GetActiveOrdersAsync(int characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get order history for a character
    /// </summary>
    Task<IEnumerable<PersonalMarketOrder>> GetOrderHistoryAsync(int characterId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get orders expiring soon
    /// </summary>
    Task<IEnumerable<PersonalMarketOrder>> GetExpiringOrdersAsync(int characterId, TimeSpan? within = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get market order portfolio summary
    /// </summary>
    Task<MarketOrderPortfolio> GetPortfolioSummaryAsync(int characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get order performance analytics
    /// </summary>
    Task<OrderPerformanceStats> GetPerformanceAnalyticsAsync(int characterId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get fill rate analytics
    /// </summary>
    Task<OrderFillRateStats> GetFillRateAnalyticsAsync(int characterId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate unrealized profit/loss for active orders
    /// </summary>
    Task<UnrealizedPnL> CalculateUnrealizedPnLAsync(int characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get order recommendations based on market analysis
    /// </summary>
    Task<IEnumerable<OrderRecommendation>> GetOrderRecommendationsAsync(int characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Monitor order status changes and send notifications
    /// </summary>
    Task MonitorOrderChangesAsync(int characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get order alerts (price changes, competition, etc.)
    /// </summary>
    Task<IEnumerable<OrderAlert>> GetOrderAlertsAsync(int characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate optimal order pricing
    /// </summary>
    Task<OptimalPricing> CalculateOptimalPricingAsync(int characterId, int typeId, int regionId, bool isBuyOrder, long volume, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get order competition analysis
    /// </summary>
    Task<OrderCompetitionAnalysis> GetCompetitionAnalysisAsync(int characterId, long orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update order notifications preferences
    /// </summary>
    Task UpdateOrderNotificationPreferencesAsync(int characterId, OrderNotificationSettings settings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get order tax and fee calculations
    /// </summary>
    Task<OrderTaxCalculation> CalculateOrderTaxesAsync(int characterId, int typeId, decimal orderValue, long locationId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Unrealized profit/loss calculation result
/// </summary>
public class UnrealizedPnL
{
    public int CharacterId { get; set; }
    public decimal TotalUnrealizedProfit { get; set; }
    public decimal TotalUnrealizedLoss { get; set; }
    public decimal NetUnrealizedPnL { get; set; }
    
    public List<OrderPnL> OrderBreakdown { get; set; } = new();
    public Dictionary<int, decimal> PnLByType { get; set; } = new();
    public Dictionary<int, decimal> PnLByRegion { get; set; } = new();
    
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Individual order profit/loss
/// </summary>
public class OrderPnL
{
    public long OrderId { get; set; }
    public int TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public bool IsBuyOrder { get; set; }
    public decimal OrderPrice { get; set; }
    public decimal CurrentMarketPrice { get; set; }
    public long VolumeRemain { get; set; }
    public decimal UnrealizedPnL { get; set; }
    public double PnLPercentage { get; set; }
}

/// <summary>
/// Order recommendation based on market analysis
/// </summary>
public class OrderRecommendation
{
    public int TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public int RegionId { get; set; }
    public string RegionName { get; set; } = string.Empty;
    
    public RecommendationType Type { get; set; }
    public decimal RecommendedPrice { get; set; }
    public long RecommendedVolume { get; set; }
    public decimal EstimatedProfit { get; set; }
    public double Confidence { get; set; }
    
    public string Reasoning { get; set; } = string.Empty;
    public TimeSpan EstimatedFillTime { get; set; }
    public decimal RiskLevel { get; set; }
}

/// <summary>
/// Order alert for price changes, competition, etc.
/// </summary>
public class OrderAlert
{
    public long OrderId { get; set; }
    public AlertType Type { get; set; }
    public AlertSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal? PriceChange { get; set; }
    public int? CompetitorCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
}

/// <summary>
/// Optimal pricing calculation result
/// </summary>
public class OptimalPricing
{
    public int TypeId { get; set; }
    public int RegionId { get; set; }
    public bool IsBuyOrder { get; set; }
    public long Volume { get; set; }
    
    public decimal OptimalPrice { get; set; }
    public decimal ConservativePrice { get; set; }
    public decimal AggressivePrice { get; set; }
    
    public decimal CurrentBestPrice { get; set; }
    public decimal RecommendedAdjustment { get; set; }
    
    public TimeSpan EstimatedFillTime { get; set; }
    public double FillProbability { get; set; }
    public decimal EstimatedProfit { get; set; }
    
    public List<PriceStrategy> Strategies { get; set; } = new();
}

/// <summary>
/// Price strategy option
/// </summary>
public class PriceStrategy
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public TimeSpan EstimatedFillTime { get; set; }
    public double FillProbability { get; set; }
    public decimal EstimatedProfit { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Order competition analysis
/// </summary>
public class OrderCompetitionAnalysis
{
    public long OrderId { get; set; }
    public int CompetitorCount { get; set; }
    public decimal YourPrice { get; set; }
    public decimal BestCompetitorPrice { get; set; }
    public decimal PriceDifference { get; set; }
    public int YourRankPosition { get; set; }
    
    public List<CompetitorOrder> Competitors { get; set; } = new();
    public decimal RecommendedPriceAdjustment { get; set; }
    public string CompetitiveAdvice { get; set; } = string.Empty;
}

/// <summary>
/// Competitor order information
/// </summary>
public class CompetitorOrder
{
    public decimal Price { get; set; }
    public long Volume { get; set; }
    public long LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public int MinJumps { get; set; }
    public DateTime IssuedDate { get; set; }
}

/// <summary>
/// Order notification settings
/// </summary>
public class OrderNotificationSettings
{
    public bool EnableOrderFilled { get; set; } = true;
    public bool EnableOrderExpiring { get; set; } = true;
    public bool EnablePriceChanges { get; set; } = true;
    public bool EnableCompetition { get; set; } = true;
    public bool EnableUnusualActivity { get; set; } = true;
    
    public TimeSpan ExpirationWarningTime { get; set; } = TimeSpan.FromHours(24);
    public decimal PriceChangeThreshold { get; set; } = 0.05m; // 5%
    public int CompetitionWarningCount { get; set; } = 5;
    
    public bool SendDesktopNotifications { get; set; } = true;
    public bool SendEmailNotifications { get; set; } = false;
    public bool PlaySounds { get; set; } = true;
}

/// <summary>
/// Order tax calculation result
/// </summary>
public class OrderTaxCalculation
{
    public decimal OrderValue { get; set; }
    public decimal BrokerFee { get; set; }
    public decimal TransactionTax { get; set; }
    public decimal TotalFees { get; set; }
    public decimal NetValue { get; set; }
    
    public double BrokerFeeRate { get; set; }
    public double TransactionTaxRate { get; set; }
    public double TotalFeeRate { get; set; }
    
    public bool HasCorpOffice { get; set; }
    public int StandingsLevel { get; set; }
    public decimal StandingsDiscount { get; set; }
}

/// <summary>
/// Recommendation type enumeration
/// </summary>
public enum RecommendationType
{
    Buy,
    Sell,
    Modify,
    Cancel,
    Hold
}

/// <summary>
/// Alert type enumeration
/// </summary>
public enum AlertType
{
    OrderFilled,
    OrderExpiring,
    PriceUndercut,
    PriceImproved,
    HighCompetition,
    LowCompetition,
    UnusualVolume,
    MarketCrash,
    MarketSpike
}

/// <summary>
/// Alert severity enumeration
/// </summary>
public enum AlertSeverity
{
    Info,
    Warning,
    High,
    Critical
}