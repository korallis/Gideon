// ==========================================================================
// IPersonalMarketOrderRepository.cs - Personal Market Order Repository Interface
// ==========================================================================
// Specialized repository interface for managing user's personal market orders.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Gideon.WPF.Core.Domain.Entities;

namespace Gideon.WPF.Core.Domain.Interfaces;

/// <summary>
/// Repository interface for personal market order operations
/// </summary>
public interface IPersonalMarketOrderRepository : IRepository<PersonalMarketOrder>
{
    /// <summary>
    /// Get all active orders for a character
    /// </summary>
    Task<IEnumerable<PersonalMarketOrder>> GetActiveOrdersByCharacterAsync(int characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get orders by character and state
    /// </summary>
    Task<IEnumerable<PersonalMarketOrder>> GetOrdersByStateAsync(int characterId, OrderState state, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get order by ESI order ID
    /// </summary>
    Task<PersonalMarketOrder?> GetByOrderIdAsync(long orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get orders expiring within specified timespan
    /// </summary>
    Task<IEnumerable<PersonalMarketOrder>> GetExpiringOrdersAsync(int characterId, TimeSpan within, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get order history for a specific order
    /// </summary>
    Task<IEnumerable<OrderHistory>> GetOrderHistoryAsync(int personalMarketOrderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update order status and volume
    /// </summary>
    Task UpdateOrderStatusAsync(long orderId, long newVolumeRemain, OrderState? newState = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add order history entry
    /// </summary>
    Task AddOrderHistoryAsync(OrderHistory orderHistory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get market order portfolio for character
    /// </summary>
    Task<MarketOrderPortfolio?> GetPortfolioAsync(int characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update or create market order portfolio
    /// </summary>
    Task UpsertPortfolioAsync(MarketOrderPortfolio portfolio, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get order performance statistics
    /// </summary>
    Task<OrderPerformanceStats> GetOrderPerformanceAsync(int characterId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get orders by item type and region
    /// </summary>
    Task<IEnumerable<PersonalMarketOrder>> GetOrdersByItemAndRegionAsync(int characterId, int typeId, int regionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk update orders from ESI data
    /// </summary>
    Task BulkUpdateOrdersAsync(IEnumerable<PersonalMarketOrder> orders, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark orders as inactive/expired
    /// </summary>
    Task MarkOrdersInactiveAsync(IEnumerable<long> orderIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get order fill rate statistics
    /// </summary>
    Task<OrderFillRateStats> GetFillRateStatsAsync(int characterId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get orders by location
    /// </summary>
    Task<IEnumerable<PersonalMarketOrder>> GetOrdersByLocationAsync(int characterId, long locationId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Order performance statistics
/// </summary>
public class OrderPerformanceStats
{
    public int CharacterId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int ExpiredOrders { get; set; }
    
    public decimal TotalValueTraded { get; set; }
    public decimal TotalProfit { get; set; }
    public decimal AverageProfit { get; set; }
    
    public double CompletionRate { get; set; }
    public double CancellationRate { get; set; }
    public double ExpirationRate { get; set; }
    
    public TimeSpan AverageFillTime { get; set; }
    public TimeSpan AverageOrderDuration { get; set; }
    
    public int MostTradedTypeId { get; set; }
    public string MostTradedTypeName { get; set; } = string.Empty;
    public int MostProfitableTypeId { get; set; }
    public string MostProfitableTypeName { get; set; } = string.Empty;
}

/// <summary>
/// Order fill rate statistics
/// </summary>
public class OrderFillRateStats
{
    public int CharacterId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    
    public double OverallFillRate { get; set; }
    public double BuyOrderFillRate { get; set; }
    public double SellOrderFillRate { get; set; }
    
    public Dictionary<int, double> FillRateByType { get; set; } = new();
    public Dictionary<int, double> FillRateByRegion { get; set; } = new();
    public Dictionary<decimal, double> FillRateByPriceRange { get; set; } = new();
    
    public TimeSpan AverageFillTime { get; set; }
    public TimeSpan MedianFillTime { get; set; }
    public TimeSpan FastestFillTime { get; set; }
    public TimeSpan SlowestFillTime { get; set; }
}