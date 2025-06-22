// ==========================================================================
// PersonalMarketOrderRepository.cs - Personal Market Order Repository Implementation
// ==========================================================================
// Concrete implementation of personal market order repository with specialized operations.
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
/// Personal market order repository implementation with specialized operations
/// </summary>
public class PersonalMarketOrderRepository : Repository<PersonalMarketOrder>, IPersonalMarketOrderRepository
{
    public PersonalMarketOrderRepository(GideonDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get all active orders for a character
    /// </summary>
    public async Task<IEnumerable<PersonalMarketOrder>> GetActiveOrdersByCharacterAsync(int characterId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.ItemType)
            .Include(o => o.Region)
            .Include(o => o.Character)
            .Where(o => o.CharacterId == characterId && o.IsActive && o.State == OrderState.Open)
            .OrderByDescending(o => o.IssuedDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get orders by character and state
    /// </summary>
    public async Task<IEnumerable<PersonalMarketOrder>> GetOrdersByStateAsync(int characterId, OrderState state, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.ItemType)
            .Include(o => o.Region)
            .Include(o => o.Character)
            .Where(o => o.CharacterId == characterId && o.State == state)
            .OrderByDescending(o => o.LastUpdated)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get order by ESI order ID
    /// </summary>
    public async Task<PersonalMarketOrder?> GetByOrderIdAsync(long orderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.ItemType)
            .Include(o => o.Region)
            .Include(o => o.Character)
            .Include(o => o.OrderHistory)
            .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
    }

    /// <summary>
    /// Get orders expiring within specified timespan
    /// </summary>
    public async Task<IEnumerable<PersonalMarketOrder>> GetExpiringOrdersAsync(int characterId, TimeSpan within, CancellationToken cancellationToken = default)
    {
        var expirationCutoff = DateTime.UtcNow.Add(within);
        
        return await _dbSet
            .Include(o => o.ItemType)
            .Include(o => o.Region)
            .Where(o => o.CharacterId == characterId && 
                       o.IsActive && 
                       o.State == OrderState.Open &&
                       o.ExpiryDate.HasValue && 
                       o.ExpiryDate.Value <= expirationCutoff)
            .OrderBy(o => o.ExpiryDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get order history for a specific order
    /// </summary>
    public async Task<IEnumerable<OrderHistory>> GetOrderHistoryAsync(int personalMarketOrderId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderHistories
            .Where(h => h.PersonalMarketOrderId == personalMarketOrderId)
            .OrderByDescending(h => h.Timestamp)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Update order status and volume
    /// </summary>
    public async Task UpdateOrderStatusAsync(long orderId, long newVolumeRemain, OrderState? newState = null, CancellationToken cancellationToken = default)
    {
        var order = await GetByOrderIdAsync(orderId, cancellationToken);
        if (order == null) return;

        var previousVolumeRemain = order.VolumeRemain;
        order.VolumeRemain = newVolumeRemain;
        order.LastUpdated = DateTime.UtcNow;

        if (newState.HasValue && newState.Value != order.State)
        {
            order.State = newState.Value;
            if (newState.Value != OrderState.Open)
            {
                order.IsActive = false;
            }
        }

        // Create history entry
        var historyType = DetermineHistoryType(previousVolumeRemain, newVolumeRemain, order.State);
        var history = new OrderHistory
        {
            PersonalMarketOrderId = order.Id,
            HistoryType = historyType,
            PreviousVolumeRemain = previousVolumeRemain,
            NewVolumeRemain = newVolumeRemain,
            TransactionQuantity = previousVolumeRemain - newVolumeRemain,
            Timestamp = DateTime.UtcNow
        };

        await AddOrderHistoryAsync(history, cancellationToken);
    }

    /// <summary>
    /// Add order history entry
    /// </summary>
    public async Task AddOrderHistoryAsync(OrderHistory orderHistory, CancellationToken cancellationToken = default)
    {
        await _context.OrderHistories.AddAsync(orderHistory, cancellationToken);
    }

    /// <summary>
    /// Get market order portfolio for character
    /// </summary>
    public async Task<MarketOrderPortfolio?> GetPortfolioAsync(int characterId, CancellationToken cancellationToken = default)
    {
        return await _context.MarketOrderPortfolios
            .Include(p => p.Character)
            .FirstOrDefaultAsync(p => p.CharacterId == characterId, cancellationToken);
    }

    /// <summary>
    /// Update or create market order portfolio
    /// </summary>
    public async Task UpsertPortfolioAsync(MarketOrderPortfolio portfolio, CancellationToken cancellationToken = default)
    {
        var existing = await GetPortfolioAsync(portfolio.CharacterId, cancellationToken);
        
        if (existing == null)
        {
            await _context.MarketOrderPortfolios.AddAsync(portfolio, cancellationToken);
        }
        else
        {
            existing.ActiveBuyOrders = portfolio.ActiveBuyOrders;
            existing.ActiveSellOrders = portfolio.ActiveSellOrders;
            existing.TotalBuyOrderValue = portfolio.TotalBuyOrderValue;
            existing.TotalSellOrderValue = portfolio.TotalSellOrderValue;
            existing.TotalEscrowValue = portfolio.TotalEscrowValue;
            existing.UnrealizedProfit = portfolio.UnrealizedProfit;
            existing.DailyTurnover = portfolio.DailyTurnover;
            existing.AverageFillRate = portfolio.AverageFillRate;
            existing.LastCalculated = portfolio.LastCalculated;
        }
    }

    /// <summary>
    /// Get order performance statistics
    /// </summary>
    public async Task<OrderPerformanceStats> GetOrderPerformanceAsync(int characterId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var orders = await _dbSet
            .Include(o => o.ItemType)
            .Where(o => o.CharacterId == characterId && 
                       o.IssuedDate >= fromDate && 
                       o.IssuedDate <= toDate)
            .ToListAsync(cancellationToken);

        var stats = new OrderPerformanceStats
        {
            CharacterId = characterId,
            FromDate = fromDate,
            ToDate = toDate,
            TotalOrders = orders.Count
        };

        if (!orders.Any()) return stats;

        stats.CompletedOrders = orders.Count(o => o.State == OrderState.Closed);
        stats.CancelledOrders = orders.Count(o => o.State == OrderState.Cancelled);
        stats.ExpiredOrders = orders.Count(o => o.State == OrderState.Expired);

        stats.CompletionRate = (double)stats.CompletedOrders / stats.TotalOrders * 100;
        stats.CancellationRate = (double)stats.CancelledOrders / stats.TotalOrders * 100;
        stats.ExpirationRate = (double)stats.ExpiredOrders / stats.TotalOrders * 100;

        var completedOrders = orders.Where(o => o.State == OrderState.Closed).ToList();
        if (completedOrders.Any())
        {
            stats.TotalValueTraded = completedOrders.Sum(o => o.TotalValue);
            
            var fillTimes = completedOrders
                .Where(o => o.ExpiryDate.HasValue)
                .Select(o => o.ExpiryDate!.Value - o.IssuedDate)
                .ToList();
            
            if (fillTimes.Any())
            {
                stats.AverageFillTime = TimeSpan.FromTicks((long)fillTimes.Average(t => t.Ticks));
            }
        }

        // Find most traded item
        var typeGroups = orders.GroupBy(o => o.TypeId).ToList();
        if (typeGroups.Any())
        {
            var mostTraded = typeGroups.OrderByDescending(g => g.Sum(o => o.VolumeFilled)).First();
            stats.MostTradedTypeId = mostTraded.Key;
            stats.MostTradedTypeName = mostTraded.First().ItemType?.TypeName ?? "Unknown";
        }

        return stats;
    }

    /// <summary>
    /// Get orders by item type and region
    /// </summary>
    public async Task<IEnumerable<PersonalMarketOrder>> GetOrdersByItemAndRegionAsync(int characterId, int typeId, int regionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.ItemType)
            .Include(o => o.Region)
            .Where(o => o.CharacterId == characterId && 
                       o.TypeId == typeId && 
                       o.RegionId == regionId)
            .OrderByDescending(o => o.IssuedDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Bulk update orders from ESI data
    /// </summary>
    public async Task BulkUpdateOrdersAsync(IEnumerable<PersonalMarketOrder> orders, CancellationToken cancellationToken = default)
    {
        var orderList = orders.ToList();
        var orderIds = orderList.Select(o => o.OrderId).ToList();

        // Get existing orders
        var existingOrders = await _dbSet
            .Where(o => orderIds.Contains(o.OrderId))
            .ToListAsync(cancellationToken);

        var existingOrderDict = existingOrders.ToDictionary(o => o.OrderId);

        foreach (var newOrder in orderList)
        {
            if (existingOrderDict.TryGetValue(newOrder.OrderId, out var existingOrder))
            {
                // Update existing order
                var volumeChanged = existingOrder.VolumeRemain != newOrder.VolumeRemain;
                var stateChanged = existingOrder.State != newOrder.State;

                if (volumeChanged || stateChanged)
                {
                    await UpdateOrderStatusAsync(newOrder.OrderId, newOrder.VolumeRemain, newOrder.State, cancellationToken);
                }

                existingOrder.Price = newOrder.Price;
                existingOrder.LastUpdated = DateTime.UtcNow;
            }
            else
            {
                // Add new order
                newOrder.CreatedAt = DateTime.UtcNow;
                newOrder.LastUpdated = DateTime.UtcNow;
                await _dbSet.AddAsync(newOrder, cancellationToken);

                // Create history entry for new order
                var history = new OrderHistory
                {
                    PersonalMarketOrder = newOrder,
                    HistoryType = OrderHistoryType.Created,
                    NewVolumeRemain = newOrder.VolumeRemain,
                    Timestamp = DateTime.UtcNow
                };
                await AddOrderHistoryAsync(history, cancellationToken);
            }
        }

        // Mark orders not in the update as inactive (they may have been cancelled/expired)
        var inactiveOrders = existingOrders
            .Where(o => !orderIds.Contains(o.OrderId) && o.IsActive)
            .ToList();

        foreach (var inactiveOrder in inactiveOrders)
        {
            inactiveOrder.IsActive = false;
            inactiveOrder.State = OrderState.Expired; // Assume expired unless we know otherwise
            inactiveOrder.LastUpdated = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Mark orders as inactive/expired
    /// </summary>
    public async Task MarkOrdersInactiveAsync(IEnumerable<long> orderIds, CancellationToken cancellationToken = default)
    {
        var orders = await _dbSet
            .Where(o => orderIds.Contains(o.OrderId))
            .ToListAsync(cancellationToken);

        foreach (var order in orders)
        {
            order.IsActive = false;
            order.State = OrderState.Expired;
            order.LastUpdated = DateTime.UtcNow;

            var history = new OrderHistory
            {
                PersonalMarketOrderId = order.Id,
                HistoryType = OrderHistoryType.Expired,
                PreviousVolumeRemain = order.VolumeRemain,
                NewVolumeRemain = order.VolumeRemain,
                Timestamp = DateTime.UtcNow
            };
            await AddOrderHistoryAsync(history, cancellationToken);
        }
    }

    /// <summary>
    /// Get order fill rate statistics
    /// </summary>
    public async Task<OrderFillRateStats> GetFillRateStatsAsync(int characterId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var completedOrders = await _dbSet
            .Include(o => o.ItemType)
            .Include(o => o.Region)
            .Where(o => o.CharacterId == characterId && 
                       o.State == OrderState.Closed &&
                       o.IssuedDate >= fromDate && 
                       o.IssuedDate <= toDate)
            .ToListAsync(cancellationToken);

        var stats = new OrderFillRateStats
        {
            CharacterId = characterId,
            FromDate = fromDate,
            ToDate = toDate
        };

        if (!completedOrders.Any()) return stats;

        // Calculate fill rates
        var allOrders = completedOrders.ToList();
        var buyOrders = allOrders.Where(o => o.IsBuyOrder).ToList();
        var sellOrders = allOrders.Where(o => !o.IsBuyOrder).ToList();

        stats.OverallFillRate = allOrders.Average(o => o.FillPercentage);
        stats.BuyOrderFillRate = buyOrders.Any() ? buyOrders.Average(o => o.FillPercentage) : 0;
        stats.SellOrderFillRate = sellOrders.Any() ? sellOrders.Average(o => o.FillPercentage) : 0;

        // Fill rate by type
        stats.FillRateByType = allOrders
            .GroupBy(o => o.TypeId)
            .ToDictionary(g => g.Key, g => g.Average(o => o.FillPercentage));

        // Fill rate by region
        stats.FillRateByRegion = allOrders
            .GroupBy(o => o.RegionId)
            .ToDictionary(g => g.Key, g => g.Average(o => o.FillPercentage));

        // Calculate fill times (approximate)
        var fillTimes = allOrders
            .Where(o => o.ExpiryDate.HasValue)
            .Select(o => o.ExpiryDate!.Value - o.IssuedDate)
            .OrderBy(t => t)
            .ToList();

        if (fillTimes.Any())
        {
            stats.AverageFillTime = TimeSpan.FromTicks((long)fillTimes.Average(t => t.Ticks));
            stats.MedianFillTime = fillTimes[fillTimes.Count / 2];
            stats.FastestFillTime = fillTimes.First();
            stats.SlowestFillTime = fillTimes.Last();
        }

        return stats;
    }

    /// <summary>
    /// Get orders by location
    /// </summary>
    public async Task<IEnumerable<PersonalMarketOrder>> GetOrdersByLocationAsync(int characterId, long locationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.ItemType)
            .Include(o => o.Region)
            .Where(o => o.CharacterId == characterId && o.LocationId == locationId)
            .OrderByDescending(o => o.IssuedDate)
            .ToListAsync(cancellationToken);
    }

    #region Private Helper Methods

    /// <summary>
    /// Determine the appropriate history type based on volume and state changes
    /// </summary>
    private static OrderHistoryType DetermineHistoryType(long previousVolume, long newVolume, OrderState state)
    {
        if (state == OrderState.Cancelled)
            return OrderHistoryType.Cancelled;
        
        if (state == OrderState.Expired)
            return OrderHistoryType.Expired;
        
        if (newVolume == 0)
            return OrderHistoryType.Completed;
        
        if (newVolume < previousVolume)
            return OrderHistoryType.PartiallyFilled;
        
        return OrderHistoryType.Modified;
    }

    #endregion
}