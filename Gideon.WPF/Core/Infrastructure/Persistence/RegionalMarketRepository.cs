// ==========================================================================
// RegionalMarketRepository.cs - Regional Market Repository Implementation
// ==========================================================================
// Concrete implementation of regional market repository with specialized operations.
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
/// Regional market repository implementation with specialized operations
/// </summary>
public class RegionalMarketRepository : Repository<RegionalMarketCoverage>, IRegionalMarketRepository
{
    public RegionalMarketRepository(GideonDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get all trade hubs by tier
    /// </summary>
    public async Task<IEnumerable<TradeHub>> GetTradeHubsByTierAsync(TradeHubTier tier, CancellationToken cancellationToken = default)
    {
        return await _context.TradeHubs
            .Include(th => th.Region)
            .Include(th => th.SolarSystem)
            .Include(th => th.Station)
            .Where(th => th.Tier == tier && th.IsActive)
            .OrderByDescending(th => th.LiquidityScore)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get trade hubs in a specific region
    /// </summary>
    public async Task<IEnumerable<TradeHub>> GetTradeHubsByRegionAsync(int regionId, CancellationToken cancellationToken = default)
    {
        return await _context.TradeHubs
            .Include(th => th.Region)
            .Include(th => th.SolarSystem)
            .Include(th => th.Station)
            .Where(th => th.RegionId == regionId && th.IsActive)
            .OrderByDescending(th => th.LiquidityScore)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get market coverage for a region
    /// </summary>
    public async Task<IEnumerable<RegionalMarketCoverage>> GetMarketCoverageByRegionAsync(int regionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(rmc => rmc.Region)
            .Include(rmc => rmc.TradeHub)
            .Include(rmc => rmc.MarketGroup)
            .Where(rmc => rmc.RegionId == regionId && rmc.IsEnabled)
            .OrderBy(rmc => rmc.Priority)
            .ThenByDescending(rmc => rmc.CoveragePercentage)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get market coverage that needs synchronization
    /// </summary>
    public async Task<IEnumerable<RegionalMarketCoverage>> GetCoverageDueForSyncAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Include(rmc => rmc.Region)
            .Include(rmc => rmc.TradeHub)
            .Include(rmc => rmc.MarketGroup)
            .Where(rmc => rmc.IsEnabled && rmc.NextSyncDue <= now)
            .OrderBy(rmc => rmc.Priority)
            .ThenBy(rmc => rmc.NextSyncDue)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get trade routes from a specific hub
    /// </summary>
    public async Task<IEnumerable<TradeRoute>> GetTradeRoutesFromHubAsync(int originHubId, CancellationToken cancellationToken = default)
    {
        return await _context.TradeRoutes
            .Include(tr => tr.OriginHub)
            .Include(tr => tr.DestinationHub)
            .Where(tr => tr.OriginHubId == originHubId && tr.IsActive)
            .OrderByDescending(tr => tr.AverageProfitMargin)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get profitable trade routes by risk level
    /// </summary>
    public async Task<IEnumerable<TradeRoute>> GetProfitableRoutesByRiskAsync(TradeRouteRisk maxRisk, CancellationToken cancellationToken = default)
    {
        return await _context.TradeRoutes
            .Include(tr => tr.OriginHub)
            .Include(tr => tr.DestinationHub)
            .Where(tr => tr.IsActive && 
                        tr.RiskLevel <= maxRisk && 
                        tr.AverageProfitMargin > 0.05m)
            .OrderByDescending(tr => tr.AverageProfitMargin)
            .ThenBy(tr => tr.RiskLevel)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get route profitability for specific items
    /// </summary>
    public async Task<IEnumerable<RouteItemProfitability>> GetRouteProfitabilityAsync(int routeId, CancellationToken cancellationToken = default)
    {
        return await _context.RouteItemProfitability
            .Include(rip => rip.Route)
            .Include(rip => rip.ItemType)
            .Where(rip => rip.RouteId == routeId)
            .OrderByDescending(rip => rip.ProfitMargin)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get top profitable items on a route
    /// </summary>
    public async Task<IEnumerable<RouteItemProfitability>> GetTopProfitableItemsAsync(int routeId, int count = 10, CancellationToken cancellationToken = default)
    {
        return await _context.RouteItemProfitability
            .Include(rip => rip.Route)
            .Include(rip => rip.ItemType)
            .Where(rip => rip.RouteId == routeId && rip.IsRecommended)
            .OrderByDescending(rip => rip.DailyPotentialProfit)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Create or update market sync job
    /// </summary>
    public async Task<MarketSyncJob> CreateSyncJobAsync(MarketSyncJob job, CancellationToken cancellationToken = default)
    {
        var existingJob = await _context.MarketSyncJobs
            .FirstOrDefaultAsync(msj => msj.JobName == job.JobName && 
                                       msj.Status == MarketSyncJobStatus.Pending, 
                               cancellationToken);

        if (existingJob != null)
        {
            // Update existing pending job
            existingJob.ScheduledTime = job.ScheduledTime;
            existingJob.Priority = job.Priority;
            existingJob.Configuration = job.Configuration;
            existingJob.LastUpdated = DateTime.UtcNow;
            return existingJob;
        }
        else
        {
            // Create new job
            job.CreatedDate = DateTime.UtcNow;
            job.LastUpdated = DateTime.UtcNow;
            await _context.MarketSyncJobs.AddAsync(job, cancellationToken);
            return job;
        }
    }

    /// <summary>
    /// Get pending sync jobs
    /// </summary>
    public async Task<IEnumerable<MarketSyncJob>> GetPendingSyncJobsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.MarketSyncJobs
            .Include(msj => msj.Region)
            .Include(msj => msj.TradeHub)
            .Include(msj => msj.MarketGroup)
            .Where(msj => msj.Status == MarketSyncJobStatus.Pending && 
                         msj.ScheduledTime <= DateTime.UtcNow)
            .OrderBy(msj => msj.Priority)
            .ThenBy(msj => msj.ScheduledTime)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Update sync job status
    /// </summary>
    public async Task UpdateSyncJobStatusAsync(int jobId, MarketSyncJobStatus status, string? errorMessage = null, CancellationToken cancellationToken = default)
    {
        var job = await _context.MarketSyncJobs.FindAsync(new object[] { jobId }, cancellationToken);
        if (job != null)
        {
            job.Status = status;
            job.ErrorMessage = errorMessage;
            job.LastUpdated = DateTime.UtcNow;

            if (status == MarketSyncJobStatus.Running && job.StartTime == null)
            {
                job.StartTime = DateTime.UtcNow;
            }
            else if (status == MarketSyncJobStatus.Completed || status == MarketSyncJobStatus.Failed)
            {
                job.CompletionTime = DateTime.UtcNow;
            }
        }
    }

    /// <summary>
    /// Get regional market statistics
    /// </summary>
    public async Task<RegionalMarketStatistics?> GetRegionalStatisticsAsync(int regionId, DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.RegionalMarketStatistics
            .Include(rms => rms.Region)
            .Include(rms => rms.TopItem)
            .FirstOrDefaultAsync(rms => rms.RegionId == regionId && 
                                       rms.StatisticsDate.Date == date.Date, 
                               cancellationToken);
    }

    /// <summary>
    /// Update regional market statistics
    /// </summary>
    public async Task UpsertRegionalStatisticsAsync(RegionalMarketStatistics statistics, CancellationToken cancellationToken = default)
    {
        var existing = await GetRegionalStatisticsAsync(statistics.RegionId, statistics.StatisticsDate, cancellationToken);
        
        if (existing == null)
        {
            statistics.CalculationDate = DateTime.UtcNow;
            await _context.RegionalMarketStatistics.AddAsync(statistics, cancellationToken);
        }
        else
        {
            // Update existing statistics
            existing.TotalDailyVolume = statistics.TotalDailyVolume;
            existing.TotalDailyValue = statistics.TotalDailyValue;
            existing.ActiveOrders = statistics.ActiveOrders;
            existing.UniqueTraders = statistics.UniqueTraders;
            existing.TrackedItems = statistics.TrackedItems;
            existing.AverageSpread = statistics.AverageSpread;
            existing.MarketEfficiency = statistics.MarketEfficiency;
            existing.LiquidityIndex = statistics.LiquidityIndex;
            existing.VolatilityIndex = statistics.VolatilityIndex;
            existing.TopItemVolume = statistics.TopItemVolume;
            existing.TopItemTypeId = statistics.TopItemTypeId;
            existing.TopItemName = statistics.TopItemName;
            existing.AverageOrderSize = statistics.AverageOrderSize;
            existing.AverageOrderLifetime = statistics.AverageOrderLifetime;
            existing.RegionActivityScore = statistics.RegionActivityScore;
            existing.CalculationDate = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Get trade hub performance metrics
    /// </summary>
    public async Task<TradeHubMetrics> GetTradeHubMetricsAsync(int tradeHubId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var tradeHub = await _context.TradeHubs
            .FirstOrDefaultAsync(th => th.TradeHubId == tradeHubId, cancellationToken);

        if (tradeHub == null)
        {
            throw new ArgumentException($"Trade hub {tradeHubId} not found");
        }

        // Get market data for the trade hub in the date range
        // This would typically query market transactions and orders
        // For now, we'll return a basic metrics object
        var metrics = new TradeHubMetrics
        {
            TradeHubId = tradeHubId,
            HubName = tradeHub.HubName,
            FromDate = fromDate,
            ToDate = toDate,
            TotalVolume = tradeHub.AverageDailyVolume * (decimal)(toDate - fromDate).TotalDays,
            LiquidityIndex = tradeHub.LiquidityScore,
            ActivityLevel = tradeHub.ActiveTraders / 10000.0 // Normalize to 0-1 scale
        };

        // TODO: Calculate actual metrics from market data
        // - Query MarketTransaction table for this hub's location
        // - Calculate real volume, value, transaction count
        // - Generate hourly breakdown
        // - Identify top trading items

        return metrics;
    }

    /// <summary>
    /// Update trade hub accessibility status
    /// </summary>
    public async Task UpdateTradeHubAccessibilityAsync(int tradeHubId, bool isAccessible, string? notes = null, CancellationToken cancellationToken = default)
    {
        var tradeHub = await _context.TradeHubs.FindAsync(new object[] { tradeHubId }, cancellationToken);
        if (tradeHub != null)
        {
            tradeHub.IsAccessible = isAccessible;
            tradeHub.AccessibilityNotes = notes;
            tradeHub.LastAccessibilityCheck = DateTime.UtcNow;
            tradeHub.LastUpdated = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Get arbitrage opportunities between hubs
    /// </summary>
    public async Task<IEnumerable<ArbitrageOpportunity>> GetArbitrageOpportunitiesAsync(decimal minProfitMargin = 0.05m, CancellationToken cancellationToken = default)
    {
        var opportunities = new List<ArbitrageOpportunity>();

        // Get all profitable routes
        var routes = await _context.TradeRoutes
            .Include(tr => tr.OriginHub)
            .Include(tr => tr.DestinationHub)
            .Include(tr => tr.ItemProfitability)
                .ThenInclude(ip => ip.ItemType)
            .Where(tr => tr.IsActive && tr.AverageProfitMargin >= minProfitMargin)
            .ToListAsync(cancellationToken);

        foreach (var route in routes)
        {
            foreach (var itemProfit in route.ItemProfitability.Where(ip => ip.ProfitMargin >= (double)minProfitMargin))
            {
                var opportunity = new ArbitrageOpportunity
                {
                    ItemTypeId = itemProfit.TypeId,
                    ItemTypeName = itemProfit.ItemType?.TypeName ?? "Unknown",
                    
                    BuyHubId = route.OriginHubId,
                    BuyHubName = route.OriginHub?.HubName ?? "Unknown",
                    BuyPrice = itemProfit.BuyPrice,
                    BuyVolume = itemProfit.DailyVolume,
                    
                    SellHubId = route.DestinationHubId,
                    SellHubName = route.DestinationHub?.HubName ?? "Unknown",
                    SellPrice = itemProfit.SellPrice,
                    SellVolume = itemProfit.DailyVolume,
                    
                    ProfitPerUnit = itemProfit.ProfitPerUnit,
                    ProfitMargin = itemProfit.ProfitMargin,
                    MaxVolume = itemProfit.DailyVolume,
                    MaxProfit = itemProfit.DailyPotentialProfit,
                    
                    Distance = route.DistanceLightYears,
                    JumpCount = route.JumpCount,
                    RiskLevel = route.RiskLevel,
                    
                    OpportunityDate = itemProfit.CalculationDate,
                    EstimatedDuration = route.EstimatedTravelTime
                };

                opportunities.Add(opportunity);
            }
        }

        return opportunities.OrderByDescending(o => o.ProfitMargin).ToList();
    }

    /// <summary>
    /// Bulk update route item profitability
    /// </summary>
    public async Task BulkUpdateRouteProfitabilityAsync(IEnumerable<RouteItemProfitability> profitabilities, CancellationToken cancellationToken = default)
    {
        var profitabilityList = profitabilities.ToList();
        var routeItemPairs = profitabilityList.Select(p => new { RouteId = p.RouteId, TypeId = p.TypeId }).ToList();

        // Get existing profitability records
        var existingProfitabilities = await _context.RouteItemProfitability
            .Where(rip => routeItemPairs.Any(pair => pair.RouteId == rip.RouteId && pair.TypeId == rip.TypeId))
            .ToListAsync(cancellationToken);

        foreach (var newProfitability in profitabilityList)
        {
            var existing = existingProfitabilities
                .FirstOrDefault(ep => ep.RouteId == newProfitability.RouteId && ep.TypeId == newProfitability.TypeId);

            if (existing == null)
            {
                newProfitability.LastUpdated = DateTime.UtcNow;
                await _context.RouteItemProfitability.AddAsync(newProfitability, cancellationToken);
            }
            else
            {
                // Update existing record
                existing.BuyPrice = newProfitability.BuyPrice;
                existing.SellPrice = newProfitability.SellPrice;
                existing.ProfitPerUnit = newProfitability.ProfitPerUnit;
                existing.ProfitMargin = newProfitability.ProfitMargin;
                existing.DailyVolume = newProfitability.DailyVolume;
                existing.DailyPotentialProfit = newProfitability.DailyPotentialProfit;
                existing.BuyOrderCount = newProfitability.BuyOrderCount;
                existing.SellOrderCount = newProfitability.SellOrderCount;
                existing.CompetitionIndex = newProfitability.CompetitionIndex;
                existing.VelocityScore = newProfitability.VelocityScore;
                existing.PriceTrend = newProfitability.PriceTrend;
                existing.IsRecommended = newProfitability.IsRecommended;
                existing.Warnings = newProfitability.Warnings;
                existing.CalculationDate = newProfitability.CalculationDate;
                existing.LastUpdated = DateTime.UtcNow;
            }
        }
    }
}