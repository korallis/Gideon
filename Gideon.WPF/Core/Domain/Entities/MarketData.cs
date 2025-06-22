// ==========================================================================
// MarketData.cs - Market Analysis Domain Entities
// ==========================================================================
// Domain entities for market data tracking and analysis.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gideon.WPF.Core.Domain.Entities;

/// <summary>
/// Market data tracking entity
/// </summary>
[Table("MarketData")]
public class MarketData
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TypeId { get; set; }

    [Required]
    public int RegionId { get; set; }

    public OrderType OrderType { get; set; }

    public decimal Price { get; set; }

    public long Volume { get; set; }

    public long VolumeRemain { get; set; }

    public int Duration { get; set; }

    public DateTime IssuedDate { get; set; }

    public DateTime RecordedDate { get; set; } = DateTime.UtcNow;

    public bool IsBuyOrder { get; set; }

    public long LocationId { get; set; }

    public double SecurityStatus { get; set; }

    // Navigation Properties
    [ForeignKey("TypeId")]
    public virtual EveType? ItemType { get; set; }

    [ForeignKey("RegionId")]
    public virtual EveRegion? Region { get; set; }
}

/// <summary>
/// Market prediction entity using AI analysis
/// </summary>
[Table("MarketPredictions")]
public class MarketPrediction
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TypeId { get; set; }

    [Required]
    public int RegionId { get; set; }

    public decimal PredictedPrice { get; set; }

    public decimal CurrentPrice { get; set; }

    public double PriceChangePercentage { get; set; }

    public PredictionTimeframe Timeframe { get; set; }

    public double Confidence { get; set; }

    public DateTime PredictionDate { get; set; } = DateTime.UtcNow;

    public DateTime TargetDate { get; set; }

    public bool IsAccurate { get; set; }

    [StringLength(500)]
    public string? Analysis { get; set; }

    [StringLength(100)]
    public string? TrendDirection { get; set; }

    // Navigation Properties
    [ForeignKey("TypeId")]
    public virtual EveType? ItemType { get; set; }

    [ForeignKey("RegionId")]
    public virtual EveRegion? Region { get; set; }
}

/// <summary>
/// Market region tracking entity
/// </summary>
[Table("MarketRegions")]
public class MarketRegion
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RegionId { get; set; }

    [Required]
    [StringLength(100)]
    public string RegionName { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public long TotalVolume { get; set; }

    public decimal AveragePrice { get; set; }

    public int ActiveOrders { get; set; }

    [StringLength(200)]
    public string? Description { get; set; }

    // Navigation Properties
    public virtual ICollection<MarketData> MarketData { get; set; } = new List<MarketData>();
    public virtual ICollection<TradingOpportunity> TradingOpportunities { get; set; } = new List<TradingOpportunity>();
}

/// <summary>
/// Trading opportunity tracking entity
/// </summary>
[Table("TradingOpportunities")]
public class TradingOpportunity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TypeId { get; set; }

    [Required]
    public int SourceRegionId { get; set; }

    [Required]
    public int DestinationRegionId { get; set; }

    public decimal BuyPrice { get; set; }

    public decimal SellPrice { get; set; }

    public decimal Profit { get; set; }

    public double ProfitMargin { get; set; }

    public long Volume { get; set; }

    public double TurnoverRate { get; set; }

    public OpportunityRisk Risk { get; set; }

    public DateTime IdentifiedDate { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiryDate { get; set; }

    public bool IsActive { get; set; } = true;

    [StringLength(500)]
    public string? Notes { get; set; }

    // Navigation Properties
    [ForeignKey("TypeId")]
    public virtual EveType? ItemType { get; set; }

    [ForeignKey("SourceRegionId")]
    public virtual MarketRegion? SourceRegion { get; set; }

    [ForeignKey("DestinationRegionId")]
    public virtual MarketRegion? DestinationRegion { get; set; }
}

/// <summary>
/// Market transaction tracking entity
/// </summary>
[Table("MarketTransactions")]
public class MarketTransaction
{
    [Key]
    public int Id { get; set; }

    [Required]
    public long TransactionId { get; set; } // ESI transaction ID

    public int? CharacterId { get; set; }

    [Required]
    public int TypeId { get; set; }

    [Required]
    public int RegionId { get; set; }

    public TransactionType TransactionType { get; set; }

    public decimal Price { get; set; }

    public long Quantity { get; set; }

    public decimal TotalValue { get; set; }

    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    public long LocationId { get; set; }

    [StringLength(100)]
    public string? LocationName { get; set; }

    public bool IsPersonal { get; set; }

    public bool IsBuy { get; set; }

    public long? RelatedOrderId { get; set; } // Link to PersonalMarketOrder if applicable

    public long? ClientId { get; set; } // Counterparty in transaction

    [StringLength(100)]
    public string? ClientName { get; set; }

    public decimal Tax { get; set; }

    public decimal Commission { get; set; }

    public JournalRefType JournalRefType { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    public string? Notes { get; set; }

    // Calculated properties
    public decimal NetValue => TotalValue - Tax - Commission;
    public decimal TotalCost => TotalValue + Tax + Commission;

    // Navigation Properties
    [ForeignKey("CharacterId")]
    public virtual Character? Character { get; set; }

    [ForeignKey("TypeId")]
    public virtual EveType? ItemType { get; set; }

    [ForeignKey("RegionId")]
    public virtual EveRegion? Region { get; set; }

    [ForeignKey("RelatedOrderId")]
    public virtual PersonalMarketOrder? RelatedOrder { get; set; }
}

/// <summary>
/// Order type enumeration
/// </summary>
public enum OrderType
{
    Buy,
    Sell
}

/// <summary>
/// Prediction timeframe enumeration
/// </summary>
public enum PredictionTimeframe
{
    OneHour,
    SixHours,
    OneDay,
    ThreeDays,
    OneWeek,
    OneMonth
}

/// <summary>
/// Opportunity risk level enumeration
/// </summary>
public enum OpportunityRisk
{
    VeryLow,
    Low,
    Medium,
    High,
    VeryHigh
}

/// <summary>
/// Transaction type enumeration
/// </summary>
public enum TransactionType
{
    Buy,
    Sell,
    Transfer,
    Contract
}

/// <summary>
/// Historical market data entity for aggregated price/volume tracking
/// </summary>
[Table("HistoricalMarketData")]
public class HistoricalMarketData
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TypeId { get; set; }

    [Required]
    public int RegionId { get; set; }

    public DateTime Date { get; set; }

    public HistoricalDataInterval Interval { get; set; }

    public decimal OpenPrice { get; set; }
    public decimal HighPrice { get; set; }
    public decimal LowPrice { get; set; }
    public decimal ClosePrice { get; set; }

    public long Volume { get; set; }
    public decimal IskVolume { get; set; }

    public int OrderCount { get; set; }
    public int BuyOrderCount { get; set; }
    public int SellOrderCount { get; set; }

    public decimal AveragePrice { get; set; }
    public decimal MedianPrice { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("TypeId")]
    public virtual EveType? ItemType { get; set; }

    [ForeignKey("RegionId")]
    public virtual EveRegion? Region { get; set; }
}

/// <summary>
/// Historical price data result structure
/// </summary>
public class HistoricalPriceData
{
    public DateTime Date { get; set; }
    public decimal OpenPrice { get; set; }
    public decimal HighPrice { get; set; }
    public decimal LowPrice { get; set; }
    public decimal ClosePrice { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal MedianPrice { get; set; }
    public long OrderCount { get; set; }
}

/// <summary>
/// Historical volume data result structure
/// </summary>
public class HistoricalVolumeData
{
    public DateTime Date { get; set; }
    public long Volume { get; set; }
    public decimal IskVolume { get; set; }
    public int BuyOrderCount { get; set; }
    public int SellOrderCount { get; set; }
    public int TotalOrderCount { get; set; }
}

/// <summary>
/// Market statistics result structure
/// </summary>
public class MarketStatistics
{
    public int TypeId { get; set; }
    public int RegionId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }

    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal MedianPrice { get; set; }
    public decimal StandardDeviation { get; set; }

    public long TotalVolume { get; set; }
    public decimal TotalIskVolume { get; set; }
    public long AverageDailyVolume { get; set; }
    public decimal AverageDailyIskVolume { get; set; }

    public int TotalOrders { get; set; }
    public int TotalBuyOrders { get; set; }
    public int TotalSellOrders { get; set; }

    public double Volatility { get; set; }
    public double TrendDirection { get; set; }
    public double LiquidityScore { get; set; }
}

/// <summary>
/// Historical data aggregation interval
/// </summary>
public enum HistoricalDataInterval
{
    Hourly,
    SixHourly,
    Daily,
    Weekly,
    Monthly
}

/// <summary>
/// Personal market order entity for tracking user's active orders
/// </summary>
[Table("PersonalMarketOrders")]
public class PersonalMarketOrder
{
    [Key]
    public int Id { get; set; }

    [Required]
    public long OrderId { get; set; } // ESI order ID

    [Required]
    public int CharacterId { get; set; }

    [Required]
    public int TypeId { get; set; }

    [Required]
    public int RegionId { get; set; }

    public long LocationId { get; set; }

    public bool IsBuyOrder { get; set; }

    public decimal Price { get; set; }

    public long VolumeTotal { get; set; }

    public long VolumeRemain { get; set; }

    public int Duration { get; set; }

    public DateTime IssuedDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public OrderState State { get; set; }

    public decimal EscrowValue { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Calculated properties
    public long VolumeFilled => VolumeTotal - VolumeRemain;
    public double FillPercentage => VolumeTotal > 0 ? (double)VolumeFilled / VolumeTotal * 100 : 0;
    public decimal TotalValue => Price * VolumeTotal;
    public decimal RemainingValue => Price * VolumeRemain;

    // Navigation Properties
    [ForeignKey("CharacterId")]
    public virtual Character? Character { get; set; }

    [ForeignKey("TypeId")]
    public virtual EveType? ItemType { get; set; }

    [ForeignKey("RegionId")]
    public virtual EveRegion? Region { get; set; }

    public virtual ICollection<OrderHistory> OrderHistory { get; set; } = new List<OrderHistory>();
}

/// <summary>
/// Order history tracking for market order changes
/// </summary>
[Table("OrderHistory")]
public class OrderHistory
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PersonalMarketOrderId { get; set; }

    public OrderHistoryType HistoryType { get; set; }

    public long PreviousVolumeRemain { get; set; }

    public long NewVolumeRemain { get; set; }

    public decimal? TransactionPrice { get; set; }

    public long? TransactionQuantity { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    public string? Notes { get; set; }

    // Navigation Properties
    [ForeignKey("PersonalMarketOrderId")]
    public virtual PersonalMarketOrder? PersonalMarketOrder { get; set; }
}

/// <summary>
/// Market order portfolio summary for character
/// </summary>
[Table("MarketOrderPortfolio")]
public class MarketOrderPortfolio
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CharacterId { get; set; }

    public int ActiveBuyOrders { get; set; }

    public int ActiveSellOrders { get; set; }

    public decimal TotalBuyOrderValue { get; set; }

    public decimal TotalSellOrderValue { get; set; }

    public decimal TotalEscrowValue { get; set; }

    public decimal UnrealizedProfit { get; set; }

    public decimal DailyTurnover { get; set; }

    public double AverageFillRate { get; set; }

    public DateTime LastCalculated { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("CharacterId")]
    public virtual Character? Character { get; set; }
}

/// <summary>
/// Order state enumeration
/// </summary>
public enum OrderState
{
    Open,
    Closed,
    Expired,
    Cancelled,
    Pending,
    CharacterDeleted
}

/// <summary>
/// Order history type enumeration
/// </summary>
public enum OrderHistoryType
{
    Created,
    PartiallyFilled,
    Completed,
    Modified,
    Cancelled,
    Expired
}

/// <summary>
/// Journal reference type enumeration for transaction classification
/// </summary>
public enum JournalRefType
{
    MarketTransaction,
    ContractTransaction,
    CharacterDonation,
    CorporationDividend,
    BountyPrize,
    InsurancePayment,
    MissionReward,
    AgentMissionReward,
    LoyaltyPointReward,
    SkillPurchase,
    Other
}

/// <summary>
/// Transaction synchronization status tracking
/// </summary>
[Table("TransactionSyncStatus")]
public class TransactionSyncStatus
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CharacterId { get; set; }

    public DateTime LastSyncDate { get; set; }

    public long LastTransactionId { get; set; }

    public int TransactionsProcessed { get; set; }

    public int TransactionsSkipped { get; set; }

    public int TransactionsErrored { get; set; }

    public SyncStatusType Status { get; set; }

    public DateTime? NextSyncScheduled { get; set; }

    [StringLength(500)]
    public string? LastError { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("CharacterId")]
    public virtual Character? Character { get; set; }
}

/// <summary>
/// Transaction analysis and statistics
/// </summary>
[Table("TransactionAnalytics")]
public class TransactionAnalytics
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CharacterId { get; set; }

    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;

    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd { get; set; }

    // Volume statistics
    public int TotalTransactions { get; set; }

    public int BuyTransactions { get; set; }

    public int SellTransactions { get; set; }

    // Value statistics
    public decimal TotalValue { get; set; }

    public decimal TotalBuyValue { get; set; }

    public decimal TotalSellValue { get; set; }

    public decimal NetProfit { get; set; }

    public decimal TotalTax { get; set; }

    public decimal TotalCommission { get; set; }

    // Performance metrics
    public decimal AverageTransactionValue { get; set; }

    public decimal LargestTransaction { get; set; }

    public decimal SmallestTransaction { get; set; }

    public double AverageMargin { get; set; }

    public int MostTradedTypeId { get; set; }

    public string MostTradedTypeName { get; set; } = string.Empty;

    public int TopProfitTypeId { get; set; }

    public string TopProfitTypeName { get; set; } = string.Empty;

    public decimal TopProfitAmount { get; set; }

    // Trading patterns
    public int UniqueItemsTraded { get; set; }

    public int UniqueLocationsUsed { get; set; }

    public TimeSpan AverageTimeBetweenTrades { get; set; }

    public DayOfWeek MostActiveDay { get; set; }

    public int MostActiveHour { get; set; }

    // Navigation Properties
    [ForeignKey("CharacterId")]
    public virtual Character? Character { get; set; }
}

/// <summary>
/// Synchronization status type enumeration
/// </summary>
public enum SyncStatusType
{
    Pending,
    InProgress,
    Completed,
    Failed,
    Paused
}