// ==========================================================================
// PersonalMarketOrderService.cs - Personal Market Order Service Implementation
// ==========================================================================
// Service implementation for managing user's personal market orders with ESI integration.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.Extensions.Logging;
using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Core.Domain.Interfaces;

namespace Gideon.WPF.Core.Application.Services;

/// <summary>
/// Personal market order service implementation
/// </summary>
public class PersonalMarketOrderService : IPersonalMarketOrderService
{
    private readonly IPersonalMarketOrderRepository _orderRepository;
    private readonly IMarketDataRepository _marketDataRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly IMarketAnalysisService _marketAnalysisService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PersonalMarketOrderService> _logger;

    public PersonalMarketOrderService(
        IPersonalMarketOrderRepository orderRepository,
        IMarketDataRepository marketDataRepository,
        ICharacterRepository characterRepository,
        IMarketAnalysisService marketAnalysisService,
        IUnitOfWork unitOfWork,
        ILogger<PersonalMarketOrderService> logger)
    {
        _orderRepository = orderRepository;
        _marketDataRepository = marketDataRepository;
        _characterRepository = characterRepository;
        _marketAnalysisService = marketAnalysisService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Synchronize market orders from ESI for a character
    /// </summary>
    public async Task SynchronizeOrdersAsync(int characterId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Synchronizing market orders for character {CharacterId}", characterId);

        try
        {
            // In a real implementation, this would call ESI API
            // For now, we'll simulate the synchronization process
            
            var character = await _characterRepository.GetByIdAsync(characterId, cancellationToken);
            if (character == null)
            {
                _logger.LogWarning("Character {CharacterId} not found", characterId);
                return;
            }

            // TODO: Implement ESI API call to get character orders
            // var esiOrders = await _esiClient.GetCharacterOrdersAsync(characterId);
            
            // For now, simulate with empty list
            var esiOrders = new List<PersonalMarketOrder>();

            await _orderRepository.BulkUpdateOrdersAsync(esiOrders, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Update portfolio summary
            await UpdatePortfolioSummaryAsync(characterId, cancellationToken);

            _logger.LogInformation("Market order synchronization completed for character {CharacterId}", characterId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing market orders for character {CharacterId}", characterId);
            throw;
        }
    }

    /// <summary>
    /// Get active orders for a character
    /// </summary>
    public async Task<IEnumerable<PersonalMarketOrder>> GetActiveOrdersAsync(int characterId, CancellationToken cancellationToken = default)
    {
        return await _orderRepository.GetActiveOrdersByCharacterAsync(characterId, cancellationToken);
    }

    /// <summary>
    /// Get order history for a character
    /// </summary>
    public async Task<IEnumerable<PersonalMarketOrder>> GetOrderHistoryAsync(int characterId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetAllAsync(cancellationToken);
        
        var filtered = orders.Where(o => o.CharacterId == characterId);
        
        if (fromDate.HasValue)
            filtered = filtered.Where(o => o.IssuedDate >= fromDate.Value);
            
        if (toDate.HasValue)
            filtered = filtered.Where(o => o.IssuedDate <= toDate.Value);

        return filtered.OrderByDescending(o => o.IssuedDate);
    }

    /// <summary>
    /// Get orders expiring soon
    /// </summary>
    public async Task<IEnumerable<PersonalMarketOrder>> GetExpiringOrdersAsync(int characterId, TimeSpan? within = null, CancellationToken cancellationToken = default)
    {
        var timespan = within ?? TimeSpan.FromDays(1); // Default to 1 day
        return await _orderRepository.GetExpiringOrdersAsync(characterId, timespan, cancellationToken);
    }

    /// <summary>
    /// Get market order portfolio summary
    /// </summary>
    public async Task<MarketOrderPortfolio> GetPortfolioSummaryAsync(int characterId, CancellationToken cancellationToken = default)
    {
        var portfolio = await _orderRepository.GetPortfolioAsync(characterId, cancellationToken);
        
        if (portfolio == null)
        {
            await UpdatePortfolioSummaryAsync(characterId, cancellationToken);
            portfolio = await _orderRepository.GetPortfolioAsync(characterId, cancellationToken);
        }

        return portfolio ?? new MarketOrderPortfolio { CharacterId = characterId };
    }

    /// <summary>
    /// Get order performance analytics
    /// </summary>
    public async Task<OrderPerformanceStats> GetPerformanceAnalyticsAsync(int characterId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _orderRepository.GetOrderPerformanceAsync(characterId, fromDate, toDate, cancellationToken);
    }

    /// <summary>
    /// Get fill rate analytics
    /// </summary>
    public async Task<OrderFillRateStats> GetFillRateAnalyticsAsync(int characterId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _orderRepository.GetFillRateStatsAsync(characterId, fromDate, toDate, cancellationToken);
    }

    /// <summary>
    /// Calculate unrealized profit/loss for active orders
    /// </summary>
    public async Task<UnrealizedPnL> CalculateUnrealizedPnLAsync(int characterId, CancellationToken cancellationToken = default)
    {
        var activeOrders = await GetActiveOrdersAsync(characterId, cancellationToken);
        var result = new UnrealizedPnL { CharacterId = characterId };

        foreach (var order in activeOrders)
        {
            try
            {
                // Get current market price
                var (bestBuy, bestSell) = await _marketDataRepository.GetBestPricesAsync(order.TypeId, order.RegionId, cancellationToken);
                var currentMarketPrice = order.IsBuyOrder ? bestBuy ?? order.Price : bestSell ?? order.Price;

                if (currentMarketPrice == null) continue;

                var priceDifference = order.IsBuyOrder 
                    ? order.Price - currentMarketPrice.Value  // For buy orders, lower market price is better
                    : currentMarketPrice.Value - order.Price; // For sell orders, higher market price is better

                var unrealizedPnL = priceDifference * order.VolumeRemain;
                var pnlPercentage = order.Price != 0 ? (double)(priceDifference / order.Price) * 100 : 0;

                var orderPnL = new OrderPnL
                {
                    OrderId = order.OrderId,
                    TypeId = order.TypeId,
                    TypeName = order.ItemType?.TypeName ?? "Unknown",
                    IsBuyOrder = order.IsBuyOrder,
                    OrderPrice = order.Price,
                    CurrentMarketPrice = currentMarketPrice.Value,
                    VolumeRemain = order.VolumeRemain,
                    UnrealizedPnL = unrealizedPnL,
                    PnLPercentage = pnlPercentage
                };

                result.OrderBreakdown.Add(orderPnL);

                if (unrealizedPnL > 0)
                    result.TotalUnrealizedProfit += unrealizedPnL;
                else
                    result.TotalUnrealizedLoss += Math.Abs(unrealizedPnL);

                // Aggregate by type and region
                if (!result.PnLByType.ContainsKey(order.TypeId))
                    result.PnLByType[order.TypeId] = 0;
                result.PnLByType[order.TypeId] += unrealizedPnL;

                if (!result.PnLByRegion.ContainsKey(order.RegionId))
                    result.PnLByRegion[order.RegionId] = 0;
                result.PnLByRegion[order.RegionId] += unrealizedPnL;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error calculating PnL for order {OrderId}", order.OrderId);
            }
        }

        result.NetUnrealizedPnL = result.TotalUnrealizedProfit - result.TotalUnrealizedLoss;
        return result;
    }

    /// <summary>
    /// Get order recommendations based on market analysis
    /// </summary>
    public async Task<IEnumerable<OrderRecommendation>> GetOrderRecommendationsAsync(int characterId, CancellationToken cancellationToken = default)
    {
        var recommendations = new List<OrderRecommendation>();
        var activeOrders = await GetActiveOrdersAsync(characterId, cancellationToken);

        foreach (var order in activeOrders)
        {
            try
            {
                var analysis = await _marketAnalysisService.GetMarketAnalysisAsync(order.TypeId, order.RegionId, cancellationToken);
                
                // Check if price adjustment is needed
                var currentBestPrice = order.IsBuyOrder ? analysis.CurrentState.BestBuyPrice : analysis.CurrentState.BestSellPrice;
                var priceDifference = Math.Abs(order.Price - currentBestPrice);
                var priceChangePercentage = order.Price != 0 ? (double)(priceDifference / order.Price) : 0;

                if (priceChangePercentage > 0.05) // 5% threshold
                {
                    var recommendation = new OrderRecommendation
                    {
                        TypeId = order.TypeId,
                        TypeName = order.ItemType?.TypeName ?? "Unknown",
                        RegionId = order.RegionId,
                        RegionName = order.Region?.RegionName ?? "Unknown",
                        Type = RecommendationType.Modify,
                        RecommendedPrice = currentBestPrice,
                        RecommendedVolume = order.VolumeRemain,
                        Confidence = 0.8,
                        Reasoning = $"Current price ({order.Price:N2}) is {priceChangePercentage:P1} away from market best ({currentBestPrice:N2})",
                        EstimatedFillTime = TimeSpan.FromHours(analysis.TrendAnalysis.RSI > 70 ? 2 : 6)
                    };

                    recommendations.Add(recommendation);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error generating recommendation for order {OrderId}", order.OrderId);
            }
        }

        return recommendations;
    }

    /// <summary>
    /// Monitor order status changes and send notifications
    /// </summary>
    public async Task MonitorOrderChangesAsync(int characterId, CancellationToken cancellationToken = default)
    {
        // This would typically be implemented as a background service
        // For now, just sync orders and check for changes
        await SynchronizeOrdersAsync(characterId, cancellationToken);
    }

    /// <summary>
    /// Get order alerts (price changes, competition, etc.)
    /// </summary>
    public async Task<IEnumerable<OrderAlert>> GetOrderAlertsAsync(int characterId, CancellationToken cancellationToken = default)
    {
        var alerts = new List<OrderAlert>();
        var activeOrders = await GetActiveOrdersAsync(characterId, cancellationToken);

        foreach (var order in activeOrders)
        {
            // Check for expiring orders
            if (order.ExpiryDate.HasValue && order.ExpiryDate.Value <= DateTime.UtcNow.AddHours(24))
            {
                alerts.Add(new OrderAlert
                {
                    OrderId = order.OrderId,
                    Type = AlertType.OrderExpiring,
                    Severity = order.ExpiryDate.Value <= DateTime.UtcNow.AddHours(6) ? AlertSeverity.High : AlertSeverity.Warning,
                    Message = $"Order for {order.ItemType?.TypeName} expires in {(order.ExpiryDate.Value - DateTime.UtcNow).TotalHours:F1} hours"
                });
            }

            // Check for price competition
            try
            {
                var (bestBuy, bestSell) = await _marketDataRepository.GetBestPricesAsync(order.TypeId, order.RegionId, cancellationToken);
                var bestPrice = order.IsBuyOrder ? bestBuy : bestSell;
                
                if (bestPrice.HasValue)
                {
                    var isUndercut = order.IsBuyOrder ? bestPrice.Value > order.Price : bestPrice.Value < order.Price;
                    if (isUndercut)
                    {
                        alerts.Add(new OrderAlert
                        {
                            OrderId = order.OrderId,
                            Type = AlertType.PriceUndercut,
                            Severity = AlertSeverity.Warning,
                            Message = $"Your {(order.IsBuyOrder ? "buy" : "sell")} order for {order.ItemType?.TypeName} has been undercut. Market: {bestPrice:N2}, Your: {order.Price:N2}",
                            PriceChange = bestPrice.Value - order.Price
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking price competition for order {OrderId}", order.OrderId);
            }
        }

        return alerts;
    }

    /// <summary>
    /// Calculate optimal order pricing
    /// </summary>
    public async Task<OptimalPricing> CalculateOptimalPricingAsync(int characterId, int typeId, int regionId, bool isBuyOrder, long volume, CancellationToken cancellationToken = default)
    {
        var analysis = await _marketAnalysisService.GetMarketAnalysisAsync(typeId, regionId, cancellationToken);
        var (bestBuy, bestSell) = await _marketDataRepository.GetBestPricesAsync(typeId, regionId, cancellationToken);

        var currentBestPrice = isBuyOrder ? bestBuy ?? 0 : bestSell ?? 0;
        var spreadPrice = isBuyOrder ? bestSell ?? 0 : bestBuy ?? 0;
        var spread = Math.Abs(spreadPrice - currentBestPrice);

        var result = new OptimalPricing
        {
            TypeId = typeId,
            RegionId = regionId,
            IsBuyOrder = isBuyOrder,
            Volume = volume,
            CurrentBestPrice = currentBestPrice
        };

        // Calculate pricing strategies
        var tickSize = CalculateTickSize(currentBestPrice);
        
        result.AggressivePrice = isBuyOrder ? currentBestPrice + tickSize : currentBestPrice - tickSize;
        result.OptimalPrice = isBuyOrder ? currentBestPrice + (spread * 0.3m) : currentBestPrice - (spread * 0.3m);
        result.ConservativePrice = isBuyOrder ? spreadPrice - tickSize : spreadPrice + tickSize;

        result.RecommendedAdjustment = result.OptimalPrice - currentBestPrice;
        result.EstimatedFillTime = EstimateFillTime(analysis, volume);
        result.FillProbability = CalculateFillProbability(analysis, result.OptimalPrice, isBuyOrder);

        // Add strategy options
        result.Strategies.Add(new PriceStrategy
        {
            Name = "Aggressive",
            Price = result.AggressivePrice,
            EstimatedFillTime = TimeSpan.FromMinutes(30),
            FillProbability = 0.9,
            Description = "Highest priority in order book, fastest fill"
        });

        result.Strategies.Add(new PriceStrategy
        {
            Name = "Optimal",
            Price = result.OptimalPrice,
            EstimatedFillTime = TimeSpan.FromHours(4),
            FillProbability = 0.7,
            Description = "Balanced approach between speed and profit"
        });

        result.Strategies.Add(new PriceStrategy
        {
            Name = "Conservative",
            Price = result.ConservativePrice,
            EstimatedFillTime = TimeSpan.FromHours(12),
            FillProbability = 0.4,
            Description = "Best profit margin, slower fill"
        });

        return result;
    }

    /// <summary>
    /// Get order competition analysis
    /// </summary>
    public async Task<OrderCompetitionAnalysis> GetCompetitionAnalysisAsync(int characterId, long orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByOrderIdAsync(orderId, cancellationToken);
        if (order == null || order.CharacterId != characterId)
        {
            throw new ArgumentException($"Order {orderId} not found or does not belong to character {characterId}");
        }

        // Get market orders for the same item/region
        var marketOrders = await _marketDataRepository.GetByTypeAndRegionAsync(order.TypeId, order.RegionId, cancellationToken);
        var competitors = marketOrders
            .Where(m => m.IsBuyOrder == order.IsBuyOrder && m.Price != order.Price)
            .OrderBy(m => order.IsBuyOrder ? -m.Price : m.Price) // Sort by best price first
            .Take(20)
            .Select(m => new CompetitorOrder
            {
                Price = m.Price,
                Volume = m.Volume,
                LocationId = m.LocationId,
                IssuedDate = m.IssuedDate
            })
            .ToList();

        var analysis = new OrderCompetitionAnalysis
        {
            OrderId = orderId,
            CompetitorCount = competitors.Count,
            YourPrice = order.Price,
            Competitors = competitors
        };

        if (competitors.Any())
        {
            analysis.BestCompetitorPrice = competitors.First().Price;
            analysis.PriceDifference = Math.Abs(order.Price - analysis.BestCompetitorPrice);
            
            // Calculate rank position
            var allPrices = competitors.Select(c => c.Price).Concat(new[] { order.Price }).ToList();
            allPrices.Sort((a, b) => order.IsBuyOrder ? b.CompareTo(a) : a.CompareTo(b));
            analysis.YourRankPosition = allPrices.IndexOf(order.Price) + 1;

            // Provide competitive advice
            if (analysis.YourRankPosition == 1)
            {
                analysis.CompetitiveAdvice = "You have the best price! Monitor for competition.";
            }
            else if (analysis.YourRankPosition <= 3)
            {
                analysis.CompetitiveAdvice = "You're in the top 3. Consider a small price adjustment to gain first position.";
                analysis.RecommendedPriceAdjustment = order.IsBuyOrder ? 
                    analysis.BestCompetitorPrice - order.Price + 0.01m :
                    order.Price - analysis.BestCompetitorPrice - 0.01m;
            }
            else
            {
                analysis.CompetitiveAdvice = "Your price is not competitive. Consider a significant adjustment.";
                analysis.RecommendedPriceAdjustment = order.IsBuyOrder ?
                    analysis.BestCompetitorPrice - order.Price + 0.01m :
                    order.Price - analysis.BestCompetitorPrice - 0.01m;
            }
        }
        else
        {
            analysis.CompetitiveAdvice = "No direct competition found. Your order should fill quickly.";
        }

        return analysis;
    }

    /// <summary>
    /// Update order notifications preferences
    /// </summary>
    public async Task UpdateOrderNotificationPreferencesAsync(int characterId, OrderNotificationSettings settings, CancellationToken cancellationToken = default)
    {
        // This would typically save to user preferences table
        // For now, just log the update
        _logger.LogInformation("Updated order notification preferences for character {CharacterId}", characterId);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Get order tax and fee calculations
    /// </summary>
    public async Task<OrderTaxCalculation> CalculateOrderTaxesAsync(int characterId, int typeId, decimal orderValue, long locationId, CancellationToken cancellationToken = default)
    {
        // Default EVE Online tax rates (these would normally come from character skills/standings)
        var brokerFeeRate = 0.03; // 3% base rate
        var transactionTaxRate = 0.036; // 3.6% base rate

        // TODO: Factor in character skills and standings
        // - Broker Relations skill reduces broker fees
        // - Accounting skill reduces transaction tax
        // - Corporation standings with station owner
        
        var brokerFee = orderValue * (decimal)brokerFeeRate;
        var transactionTax = orderValue * (decimal)transactionTaxRate;

        return new OrderTaxCalculation
        {
            OrderValue = orderValue,
            BrokerFee = brokerFee,
            TransactionTax = transactionTax,
            TotalFees = brokerFee + transactionTax,
            NetValue = orderValue - brokerFee - transactionTax,
            BrokerFeeRate = brokerFeeRate,
            TransactionTaxRate = transactionTaxRate,
            TotalFeeRate = brokerFeeRate + transactionTaxRate
        };
    }

    #region Private Helper Methods

    /// <summary>
    /// Update portfolio summary for character
    /// </summary>
    private async Task UpdatePortfolioSummaryAsync(int characterId, CancellationToken cancellationToken)
    {
        var activeOrders = await GetActiveOrdersAsync(characterId, cancellationToken);
        var orderList = activeOrders.ToList();

        var portfolio = new MarketOrderPortfolio
        {
            CharacterId = characterId,
            ActiveBuyOrders = orderList.Count(o => o.IsBuyOrder),
            ActiveSellOrders = orderList.Count(o => !o.IsBuyOrder),
            TotalBuyOrderValue = orderList.Where(o => o.IsBuyOrder).Sum(o => o.RemainingValue),
            TotalSellOrderValue = orderList.Where(o => !o.IsBuyOrder).Sum(o => o.RemainingValue),
            TotalEscrowValue = orderList.Sum(o => o.EscrowValue),
            AverageFillRate = orderList.Any() ? orderList.Average(o => o.FillPercentage) : 0,
            LastCalculated = DateTime.UtcNow
        };

        await _orderRepository.UpsertPortfolioAsync(portfolio, cancellationToken);
    }

    /// <summary>
    /// Calculate tick size for price adjustments
    /// </summary>
    private static decimal CalculateTickSize(decimal price)
    {
        if (price < 1m) return 0.01m;
        if (price < 10m) return 0.01m;
        if (price < 100m) return 0.01m;
        if (price < 1000m) return 0.01m;
        if (price < 10000m) return 0.01m;
        return 0.01m; // Standard ISK tick size
    }

    /// <summary>
    /// Estimate fill time based on market analysis
    /// </summary>
    private static TimeSpan EstimateFillTime(MarketAnalysisResult analysis, long volume)
    {
        var dailyVolume = analysis.CurrentState.DailyVolume;
        if (dailyVolume == 0) return TimeSpan.FromDays(1);

        var volumeRatio = (double)volume / dailyVolume;
        var baseFillTime = volumeRatio * 24; // Hours

        // Adjust based on market activity
        if (analysis.TrendAnalysis.RSI > 70) baseFillTime *= 0.5; // High activity
        else if (analysis.TrendAnalysis.RSI < 30) baseFillTime *= 2.0; // Low activity

        return TimeSpan.FromHours(Math.Max(0.5, Math.Min(168, baseFillTime))); // 30 min to 1 week
    }

    /// <summary>
    /// Calculate fill probability based on market conditions
    /// </summary>
    private static double CalculateFillProbability(MarketAnalysisResult analysis, decimal price, bool isBuyOrder)
    {
        var currentBestPrice = isBuyOrder ? analysis.CurrentState.BestBuyPrice : analysis.CurrentState.BestSellPrice;
        if (currentBestPrice == 0) return 0.5;

        var priceCompetitiveness = (double)Math.Abs(price - currentBestPrice) / (double)currentBestPrice;
        
        var baseProbability = 0.7;
        if (priceCompetitiveness < 0.01) baseProbability = 0.95; // Very competitive
        else if (priceCompetitiveness < 0.05) baseProbability = 0.8; // Competitive
        else if (priceCompetitiveness > 0.2) baseProbability = 0.3; // Not competitive

        // Adjust for market volatility
        if (analysis.Volatility.Rating == VolatilityRating.High)
            baseProbability *= 1.2;
        else if (analysis.Volatility.Rating == VolatilityRating.Low)
            baseProbability *= 0.8;

        return Math.Max(0.1, Math.Min(0.99, baseProbability));
    }

    #endregion
}