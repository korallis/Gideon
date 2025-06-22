// ==========================================================================
// EsiDegradationServices.cs - Graceful Degradation for ESI Outages
// ==========================================================================
// Services for handling ESI outages with graceful degradation and offline functionality.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.Extensions.Logging;
using Gideon.WPF.Core.Application.Services;
using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Core.Domain.Interfaces;
using System.Collections.Concurrent;

namespace Gideon.WPF.Core.Infrastructure.Services;

#region Graceful Degradation Services

/// <summary>
/// Graceful degradation service for ESI API outages
/// </summary>
public class EsiDegradationService : IEsiDegradationService
{
    private readonly IEsiServerStatusService _serverStatusService;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EsiDegradationService> _logger;
    private readonly IAuditLogService _auditService;
    private readonly IOfflineFunctionalityService _offlineService;
    
    private readonly ConcurrentDictionary<string, DateTime> _serviceOutages = new();
    private readonly Timer _outageCheckTimer;
    private EsiServiceStatus _currentStatus;

    public EsiDegradationService(
        IEsiServerStatusService serverStatusService,
        ICacheService cacheService,
        IUnitOfWork unitOfWork,
        ILogger<EsiDegradationService> logger,
        IAuditLogService auditService,
        IOfflineFunctionalityService offlineService)
    {
        _serverStatusService = serverStatusService ?? throw new ArgumentNullException(nameof(serverStatusService));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _offlineService = offlineService ?? throw new ArgumentNullException(nameof(offlineService));
        
        _currentStatus = new EsiServiceStatus { IsOnline = true, LastChecked = DateTime.UtcNow };
        
        // Monitor ESI status every 2 minutes
        _outageCheckTimer = new Timer(CheckServiceStatus, null, TimeSpan.Zero, TimeSpan.FromMinutes(2));
    }

    /// <summary>
    /// Get current ESI service status with degradation information
    /// </summary>
    public async Task<EsiServiceStatus> GetServiceStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var healthStatus = await _serverStatusService.GetHealthStatusAsync(cancellationToken);
            
            _currentStatus = new EsiServiceStatus
            {
                IsOnline = healthStatus.IsHealthy,
                LastChecked = DateTime.UtcNow,
                PlayerCount = healthStatus.ServerStatus.PlayerCount,
                ServerVersion = healthStatus.ServerStatus.ServerVersion,
                Issues = healthStatus.Issues,
                DegradationLevel = DetermineDegradationLevel(healthStatus),
                OfflineCapabilities = await _offlineService.GetOfflineCapabilitiesAsync(cancellationToken)
            };
            
            return _currentStatus;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ESI service status");
            
            return new EsiServiceStatus
            {
                IsOnline = false,
                LastChecked = DateTime.UtcNow,
                DegradationLevel = DegradationLevel.Complete,
                Issues = new List<string> { $"Status check failed: {ex.Message}" },
                OfflineCapabilities = await _offlineService.GetOfflineCapabilitiesAsync(cancellationToken)
            };
        }
    }

    /// <summary>
    /// Execute ESI request with automatic fallback to cached/offline data
    /// </summary>
    public async Task<T?> ExecuteWithFallbackAsync<T>(
        Func<CancellationToken, Task<T?>> esiCall,
        Func<CancellationToken, Task<T?>> fallbackCall,
        string operationName,
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            // Check if ESI is available
            var status = await GetServiceStatusAsync(cancellationToken);
            
            if (status.IsOnline || status.DegradationLevel == DegradationLevel.None)
            {
                try
                {
                    var result = await esiCall(cancellationToken);
                    if (result != null)
                    {
                        // Cache successful result for future fallback
                        await _cacheService.SetAsync($"fallback_{operationName}", result, TimeSpan.FromHours(24), cancellationToken);
                        return result;
                    }
                }
                catch (EsiApiException ex) when (IsTransientError(ex))
                {
                    _logger.LogWarning("ESI API call failed, attempting fallback for {Operation}: {Error}", 
                        operationName, ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ESI API call failed for {Operation}", operationName);
                }
            }
            
            // Fallback to cached/offline data
            _logger.LogInformation("Using fallback data for {Operation} due to ESI unavailability", operationName);
            
            var fallbackResult = await fallbackCall(cancellationToken);
            
            await _auditService.LogActionAsync("esi_fallback_used", operationName, null, cancellationToken);
            
            return fallbackResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Both ESI and fallback failed for {Operation}", operationName);
            return null;
        }
    }

    /// <summary>
    /// Get degraded functionality recommendations for current ESI status
    /// </summary>
    public async Task<DegradationRecommendations> GetDegradationRecommendationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var status = await GetServiceStatusAsync(cancellationToken);
            var recommendations = new DegradationRecommendations
            {
                CurrentLevel = status.DegradationLevel,
                LastUpdated = DateTime.UtcNow
            };
            
            switch (status.DegradationLevel)
            {
                case DegradationLevel.None:
                    recommendations.Message = "All ESI services are fully operational";
                    recommendations.AvailableFeatures = GideonFeature.All;
                    break;
                    
                case DegradationLevel.Minor:
                    recommendations.Message = "ESI experiencing minor issues - some delays may occur";
                    recommendations.AvailableFeatures = GideonFeature.All;
                    recommendations.Limitations.Add("Market data updates may be delayed");
                    recommendations.Limitations.Add("Character synchronization may be slower");
                    break;
                    
                case DegradationLevel.Partial:
                    recommendations.Message = "ESI partially unavailable - limited functionality active";
                    recommendations.AvailableFeatures = GideonFeature.Skills | GideonFeature.Assets;
                    recommendations.Limitations.Add("Market data unavailable - using cached data");
                    recommendations.Limitations.Add("Character updates disabled");
                    recommendations.Limitations.Add("Corporation data may be stale");
                    break;
                    
                case DegradationLevel.Severe:
                    recommendations.Message = "ESI severely degraded - offline mode recommended";
                    recommendations.AvailableFeatures = GideonFeature.Skills;
                    recommendations.Limitations.Add("Most real-time features unavailable");
                    recommendations.Limitations.Add("Using cached data only");
                    recommendations.Limitations.Add("Character authentication may fail");
                    break;
                    
                case DegradationLevel.Complete:
                    recommendations.Message = "ESI offline - running in full offline mode";
                    recommendations.AvailableFeatures = GideonFeature.None;
                    recommendations.Limitations.Add("All ESI-dependent features disabled");
                    recommendations.Limitations.Add("Cached data only");
                    recommendations.Limitations.Add("No character authentication");
                    break;
            }
            
            // Add offline alternatives
            recommendations.OfflineAlternatives = await _offlineService.GetAlternativeFeaturesAsync(
                status.DegradationLevel, cancellationToken);
            
            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating degradation recommendations");
            
            return new DegradationRecommendations
            {
                CurrentLevel = DegradationLevel.Complete,
                Message = "Unable to assess ESI status - assuming offline mode",
                AvailableFeatures = GideonFeature.None,
                LastUpdated = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Enable maintenance mode with user notification
    /// </summary>
    public async Task EnableMaintenanceModeAsync(string reason, TimeSpan? estimatedDuration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var maintenanceInfo = new MaintenanceInfo
            {
                IsEnabled = true,
                Reason = reason,
                StartTime = DateTime.UtcNow,
                EstimatedDuration = estimatedDuration,
                EstimatedEndTime = estimatedDuration.HasValue ? DateTime.UtcNow.Add(estimatedDuration.Value) : null
            };
            
            await _cacheService.SetAsync("maintenance_mode", maintenanceInfo, TimeSpan.FromDays(1), cancellationToken);
            
            _logger.LogWarning("Maintenance mode enabled: {Reason}", reason);
            await _auditService.LogActionAsync("maintenance_mode_enabled", "System", reason, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling maintenance mode");
        }
    }

    /// <summary>
    /// Disable maintenance mode
    /// </summary>
    public async Task DisableMaintenanceModeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _cacheService.RemoveAsync("maintenance_mode", cancellationToken);
            
            _logger.LogInformation("Maintenance mode disabled");
            await _auditService.LogActionAsync("maintenance_mode_disabled", "System", null, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling maintenance mode");
        }
    }

    /// <summary>
    /// Get current maintenance status
    /// </summary>
    public async Task<MaintenanceInfo?> GetMaintenanceStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _cacheService.GetAsync<MaintenanceInfo>("maintenance_mode", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving maintenance status");
            return null;
        }
    }

    #region Private Methods

    private async void CheckServiceStatus(object? state)
    {
        try
        {
            var previousStatus = _currentStatus.IsOnline;
            var currentStatus = await GetServiceStatusAsync(CancellationToken.None);
            
            if (currentStatus.IsOnline != previousStatus)
            {
                var statusText = currentStatus.IsOnline ? "ONLINE" : "OFFLINE";
                _logger.LogWarning("ESI service status changed: {Status}", statusText);
                
                if (!currentStatus.IsOnline)
                {
                    await EnableMaintenanceModeAsync(
                        "ESI service unavailable - running in offline mode",
                        TimeSpan.FromHours(1),
                        CancellationToken.None);
                }
                else
                {
                    await DisableMaintenanceModeAsync(CancellationToken.None);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during automatic service status check");
        }
    }

    private static DegradationLevel DetermineDegradationLevel(EsiHealthStatus healthStatus)
    {
        if (!healthStatus.IsHealthy)
        {
            return DegradationLevel.Complete;
        }
        
        var issueCount = healthStatus.Issues.Count;
        
        return issueCount switch
        {
            0 => DegradationLevel.None,
            1 => DegradationLevel.Minor,
            2 => DegradationLevel.Partial,
            >= 3 => DegradationLevel.Severe,
            _ => DegradationLevel.None
        };
    }

    private static bool IsTransientError(EsiApiException ex)
    {
        return ex is EsiServerException || 
               ex is EsiRateLimitException ||
               ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    public void Dispose()
    {
        _outageCheckTimer?.Dispose();
    }
}

/// <summary>
/// Offline functionality service for ESI-independent features
/// </summary>
public class OfflineFunctionalityService : IOfflineFunctionalityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<OfflineFunctionalityService> _logger;

    public OfflineFunctionalityService(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<OfflineFunctionalityService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get offline capabilities available during ESI outages
    /// </summary>
    public async Task<OfflineCapabilities> GetOfflineCapabilitiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var capabilities = new OfflineCapabilities
            {
                CanViewCachedCharacterData = await HasCachedCharacterDataAsync(cancellationToken),
                CanViewSavedFittings = await HasSavedFittingsAsync(cancellationToken),
                CanUseShipFittingCalculator = true, // Always available
                CanViewCachedMarketData = await HasCachedMarketDataAsync(cancellationToken),
                CanViewSkillPlans = await HasSkillPlansAsync(cancellationToken),
                CanExportData = true, // Always available
                CanImportData = true, // Always available
                CanUseFittingTemplates = await HasFittingTemplatesAsync(cancellationToken),
                LastCacheUpdate = await GetLastCacheUpdateAsync(cancellationToken)
            };
            
            return capabilities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error determining offline capabilities");
            
            return new OfflineCapabilities
            {
                CanUseShipFittingCalculator = true,
                CanExportData = true,
                CanImportData = true
            };
        }
    }

    /// <summary>
    /// Get alternative features available during different degradation levels
    /// </summary>
    public async Task<List<AlternativeFeature>> GetAlternativeFeaturesAsync(DegradationLevel degradationLevel, CancellationToken cancellationToken = default)
    {
        try
        {
            var alternatives = new List<AlternativeFeature>();
            
            // Ship fitting calculator (always available)
            alternatives.Add(new AlternativeFeature
            {
                Name = "Ship Fitting Calculator",
                Description = "Calculate ship performance using cached game data",
                IsAvailable = true,
                Category = "Ship Fitting"
            });
            
            // Saved fittings
            if (await HasSavedFittingsAsync(cancellationToken))
            {
                alternatives.Add(new AlternativeFeature
                {
                    Name = "Saved Ship Fittings",
                    Description = "View and modify your saved ship configurations",
                    IsAvailable = true,
                    Category = "Ship Fitting"
                });
            }
            
            // Skill planning (if we have character data)
            if (degradationLevel <= DegradationLevel.Partial && await HasCachedCharacterDataAsync(cancellationToken))
            {
                alternatives.Add(new AlternativeFeature
                {
                    Name = "Skill Planning",
                    Description = "Plan skill training using cached character data",
                    IsAvailable = true,
                    Category = "Character Planning"
                });
            }
            
            // Market analysis (if we have cached data)
            if (degradationLevel <= DegradationLevel.Severe && await HasCachedMarketDataAsync(cancellationToken))
            {
                alternatives.Add(new AlternativeFeature
                {
                    Name = "Market Analysis",
                    Description = "Analyze trends using cached market data",
                    IsAvailable = true,
                    Category = "Market Analysis",
                    Limitations = new List<string> { "Data may be outdated", "No real-time updates" }
                });
            }
            
            // Data export (always available)
            alternatives.Add(new AlternativeFeature
            {
                Name = "Data Export",
                Description = "Export fittings and configurations to various formats",
                IsAvailable = true,
                Category = "Data Management"
            });
            
            return alternatives;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alternative features");
            return new List<AlternativeFeature>();
        }
    }

    /// <summary>
    /// Prepare offline mode by caching essential data
    /// </summary>
    public async Task PrepareOfflineModeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Preparing offline mode by caching essential data");
            
            // Cache static game data
            await CacheStaticGameDataAsync(cancellationToken);
            
            // Cache user settings
            await CacheUserSettingsAsync(cancellationToken);
            
            // Cache fitting templates
            await CacheFittingTemplatesAsync(cancellationToken);
            
            _logger.LogInformation("Offline mode preparation completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error preparing offline mode");
        }
    }

    #region Private Methods

    private async Task<bool> HasCachedCharacterDataAsync(CancellationToken cancellationToken)
    {
        try
        {
            var characters = await _unitOfWork.Characters.GetAllAsync(cancellationToken);
            return characters.Any();
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> HasSavedFittingsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var fittings = await _unitOfWork.ShipFittings.GetAllAsync(cancellationToken);
            return fittings.Any();
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> HasCachedMarketDataAsync(CancellationToken cancellationToken)
    {
        try
        {
            var marketData = await _unitOfWork.MarketData.GetAllAsync(cancellationToken);
            return marketData.Any();
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> HasSkillPlansAsync(CancellationToken cancellationToken)
    {
        try
        {
            var skillPlans = await _unitOfWork.SkillPlans.GetAllAsync(cancellationToken);
            return skillPlans.Any();
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> HasFittingTemplatesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var templates = await _unitOfWork.FittingTemplates.GetAllAsync(cancellationToken);
            return templates.Any();
        }
        catch
        {
            return false;
        }
    }

    private async Task<DateTime?> GetLastCacheUpdateAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _cacheService.GetAsync<DateTime?>("last_cache_update", cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private async Task CacheStaticGameDataAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Cache essential EVE static data for offline calculations
            var eveTypes = await _unitOfWork.EveTypes.GetAllAsync(cancellationToken);
            await _cacheService.SetAsync("static_eve_types", eveTypes, TimeSpan.FromDays(30), cancellationToken);
            
            var eveGroups = await _unitOfWork.EveGroups.GetAllAsync(cancellationToken);
            await _cacheService.SetAsync("static_eve_groups", eveGroups, TimeSpan.FromDays(30), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching static game data");
        }
    }

    private async Task CacheUserSettingsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var settings = await _unitOfWork.ApplicationSettings.GetAllAsync(cancellationToken);
            await _cacheService.SetAsync("user_settings", settings, TimeSpan.FromDays(7), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching user settings");
        }
    }

    private async Task CacheFittingTemplatesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var templates = await _unitOfWork.FittingTemplates.GetAllAsync(cancellationToken);
            await _cacheService.SetAsync("fitting_templates", templates, TimeSpan.FromDays(7), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching fitting templates");
        }
    }

    #endregion
}

#endregion

#region Supporting Data Structures

/// <summary>
/// ESI service status with degradation information
/// </summary>
public class EsiServiceStatus
{
    public bool IsOnline { get; set; }
    public DateTime LastChecked { get; set; }
    public int PlayerCount { get; set; }
    public string ServerVersion { get; set; } = string.Empty;
    public List<string> Issues { get; set; } = new();
    public DegradationLevel DegradationLevel { get; set; }
    public OfflineCapabilities OfflineCapabilities { get; set; } = new();
}

/// <summary>
/// Service degradation levels
/// </summary>
public enum DegradationLevel
{
    None = 0,       // Full functionality
    Minor = 1,      // Minor delays/issues
    Partial = 2,    // Some features unavailable
    Severe = 3,     // Most features unavailable
    Complete = 4    // Full offline mode
}

/// <summary>
/// Degradation recommendations
/// </summary>
public class DegradationRecommendations
{
    public DegradationLevel CurrentLevel { get; set; }
    public string Message { get; set; } = string.Empty;
    public GideonFeature AvailableFeatures { get; set; }
    public List<string> Limitations { get; set; } = new();
    public List<AlternativeFeature> OfflineAlternatives { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Offline capabilities information
/// </summary>
public class OfflineCapabilities
{
    public bool CanViewCachedCharacterData { get; set; }
    public bool CanViewSavedFittings { get; set; }
    public bool CanUseShipFittingCalculator { get; set; }
    public bool CanViewCachedMarketData { get; set; }
    public bool CanViewSkillPlans { get; set; }
    public bool CanExportData { get; set; }
    public bool CanImportData { get; set; }
    public bool CanUseFittingTemplates { get; set; }
    public DateTime? LastCacheUpdate { get; set; }
}

/// <summary>
/// Alternative feature during degradation
/// </summary>
public class AlternativeFeature
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string Category { get; set; } = string.Empty;
    public List<string> Limitations { get; set; } = new();
}

/// <summary>
/// Maintenance mode information
/// </summary>
public class MaintenanceInfo
{
    public bool IsEnabled { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public TimeSpan? EstimatedDuration { get; set; }
    public DateTime? EstimatedEndTime { get; set; }
}

/// <summary>
/// ESI degradation service interface
/// </summary>
public interface IEsiDegradationService : IDisposable
{
    Task<EsiServiceStatus> GetServiceStatusAsync(CancellationToken cancellationToken = default);
    Task<T?> ExecuteWithFallbackAsync<T>(Func<CancellationToken, Task<T?>> esiCall, Func<CancellationToken, Task<T?>> fallbackCall, string operationName, CancellationToken cancellationToken = default) where T : class;
    Task<DegradationRecommendations> GetDegradationRecommendationsAsync(CancellationToken cancellationToken = default);
    Task EnableMaintenanceModeAsync(string reason, TimeSpan? estimatedDuration = null, CancellationToken cancellationToken = default);
    Task DisableMaintenanceModeAsync(CancellationToken cancellationToken = default);
    Task<MaintenanceInfo?> GetMaintenanceStatusAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Offline functionality service interface
/// </summary>
public interface IOfflineFunctionalityService
{
    Task<OfflineCapabilities> GetOfflineCapabilitiesAsync(CancellationToken cancellationToken = default);
    Task<List<AlternativeFeature>> GetAlternativeFeaturesAsync(DegradationLevel degradationLevel, CancellationToken cancellationToken = default);
    Task PrepareOfflineModeAsync(CancellationToken cancellationToken = default);
}

#endregion