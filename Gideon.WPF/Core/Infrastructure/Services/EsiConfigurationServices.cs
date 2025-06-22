// ==========================================================================
// EsiConfigurationServices.cs - ESI Application Configuration Services
// ==========================================================================
// Pre-configured ESI credentials and application settings for seamless user experience.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Gideon.WPF.Core.Application.Services;
using System.Text.Json;

namespace Gideon.WPF.Core.Infrastructure.Services;

#region ESI Configuration Services

/// <summary>
/// ESI application configuration service with pre-configured credentials
/// </summary>
public class EsiConfigurationService : IEsiConfigurationService
{
    private readonly ILogger<EsiConfigurationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAuditLogService _auditService;
    
    // Pre-configured ESI Application Settings for Gideon
    private readonly EsiApplicationConfig _defaultConfig;
    
    public EsiConfigurationService(
        ILogger<EsiConfigurationService> logger,
        IConfiguration configuration,
        IAuditLogService auditService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        
        _defaultConfig = LoadDefaultConfiguration();
        ValidateConfiguration();
    }

    /// <summary>
    /// Get ESI application configuration
    /// </summary>
    public async Task<EsiApplicationConfig> GetConfigurationAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Try to load user-specific configuration first
            var userConfig = await LoadUserConfigurationAsync(cancellationToken);
            if (userConfig != null && userConfig.IsValid)
            {
                _logger.LogDebug("Using user-specific ESI configuration");
                return userConfig;
            }
            
            // Fall back to default pre-configured settings
            _logger.LogInformation("Using default pre-configured ESI credentials");
            await _auditService.LogActionAsync("esi_config_default_used", "Configuration", null, cancellationToken);
            
            return _defaultConfig;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ESI configuration");
            return _defaultConfig;
        }
    }

    /// <summary>
    /// Save user-specific ESI configuration
    /// </summary>
    public async Task<bool> SaveUserConfigurationAsync(EsiApplicationConfig config, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!config.IsValid)
            {
                _logger.LogWarning("Attempted to save invalid ESI configuration");
                return false;
            }
            
            var configPath = GetUserConfigPath();
            var configJson = JsonSerializer.Serialize(config, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            await File.WriteAllTextAsync(configPath, configJson, cancellationToken);
            
            await _auditService.LogActionAsync("esi_config_user_saved", "Configuration", null, cancellationToken);
            _logger.LogInformation("Saved user-specific ESI configuration");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving user ESI configuration");
            return false;
        }
    }

    /// <summary>
    /// Get OAuth2 scopes required for Gideon functionality
    /// </summary>
    public async Task<string[]> GetRequiredScopesAsync(GideonFeature features = GideonFeature.All, CancellationToken cancellationToken = default)
    {
        try
        {
            var scopes = new List<string>();
            
            // Base scopes always required
            scopes.Add("publicData");
            
            if (features.HasFlag(GideonFeature.CharacterData))
            {
                scopes.AddRange(new[]
                {
                    "esi-characters.read_contacts.v1",
                    "esi-characters.read_standings.v1",
                    "esi-characters.read_corporation_roles.v1",
                    "esi-characters.read_titles.v1",
                    "esi-characters.read_notifications.v1"
                });
            }
            
            if (features.HasFlag(GideonFeature.Skills))
            {
                scopes.AddRange(new[]
                {
                    "esi-skills.read_skills.v1",
                    "esi-skills.read_skillqueue.v1",
                    "esi-characters.read_attributes.v1",
                    "esi-characters.read_implants.v1"
                });
            }
            
            if (features.HasFlag(GideonFeature.Assets))
            {
                scopes.AddRange(new[]
                {
                    "esi-assets.read_assets.v1",
                    "esi-characters.read_blueprints.v1",
                    "esi-characters.read_loyalty.v1"
                });
            }
            
            if (features.HasFlag(GideonFeature.Market))
            {
                scopes.AddRange(new[]
                {
                    "esi-markets.read_character_orders.v1",
                    "esi-wallet.read_character_wallet.v1",
                    "esi-wallet.read_character_journal.v1"
                });
            }
            
            if (features.HasFlag(GideonFeature.Location))
            {
                scopes.AddRange(new[]
                {
                    "esi-location.read_location.v1",
                    "esi-location.read_ship_type.v1",
                    "esi-location.read_online.v1",
                    "esi-clones.read_clones.v1"
                });
            }
            
            if (features.HasFlag(GideonFeature.Corporation))
            {
                scopes.AddRange(new[]
                {
                    "esi-corporations.read_corporation_membership.v1",
                    "esi-corporations.read_structures.v1"
                });
            }
            
            if (features.HasFlag(GideonFeature.Fleet))
            {
                scopes.AddRange(new[]
                {
                    "esi-fleets.read_fleet.v1",
                    "esi-fleets.write_fleet.v1"
                });
            }
            
            _logger.LogDebug("Generated {ScopeCount} OAuth2 scopes for features: {Features}", scopes.Count, features);
            return scopes.Distinct().ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating required scopes for features: {Features}", features);
            return Array.Empty<string>();
        }
    }

    /// <summary>
    /// Test ESI configuration validity
    /// </summary>
    public async Task<EsiConfigTestResult> TestConfigurationAsync(EsiApplicationConfig config, CancellationToken cancellationToken = default)
    {
        var result = new EsiConfigTestResult { Config = config };
        
        try
        {
            // Test 1: Basic validation
            result.IsValidConfig = config.IsValid;
            if (!result.IsValidConfig)
            {
                result.Errors.Add("Configuration validation failed");
                return result;
            }
            
            // Test 2: ESI server connectivity
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", config.UserAgent);
                
                var response = await httpClient.GetAsync($"{config.EsiBaseUrl}/latest/status/", cancellationToken);
                result.EsiConnectivity = response.IsSuccessStatusCode;
                
                if (!result.EsiConnectivity)
                {
                    result.Errors.Add($"ESI server unreachable: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                result.EsiConnectivity = false;
                result.Errors.Add($"ESI connectivity test failed: {ex.Message}");
            }
            
            // Test 3: OAuth endpoints
            try
            {
                using var httpClient = new HttpClient();
                var authUrl = $"{config.SsoBaseUrl}/v2/oauth/authorize";
                var response = await httpClient.GetAsync(authUrl, cancellationToken);
                
                // OAuth endpoint should return 400 for missing parameters, not 404
                result.OAuthEndpoints = response.StatusCode != System.Net.HttpStatusCode.NotFound;
                
                if (!result.OAuthEndpoints)
                {
                    result.Errors.Add("OAuth endpoints not accessible");
                }
            }
            catch (Exception ex)
            {
                result.OAuthEndpoints = false;
                result.Errors.Add($"OAuth endpoint test failed: {ex.Message}");
            }
            
            result.IsSuccessful = result.IsValidConfig && result.EsiConnectivity && result.OAuthEndpoints;
            
            _logger.LogInformation("ESI configuration test completed: {IsSuccessful}", result.IsSuccessful);
            await _auditService.LogActionAsync("esi_config_tested", "Configuration", 
                result.IsSuccessful.ToString(), cancellationToken);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing ESI configuration");
            result.Errors.Add($"Test execution failed: {ex.Message}");
            return result;
        }
    }

    /// <summary>
    /// Get ESI data source information
    /// </summary>
    public async Task<EsiDataSource> GetDataSourceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var config = await GetConfigurationAsync(cancellationToken);
            
            return new EsiDataSource
            {
                Name = config.DataSource,
                BaseUrl = config.EsiBaseUrl,
                SsoUrl = config.SsoBaseUrl,
                IsTransquility = config.DataSource.Equals("tranquility", StringComparison.OrdinalIgnoreCase),
                IsSingularity = config.DataSource.Equals("singularity", StringComparison.OrdinalIgnoreCase),
                Description = GetDataSourceDescription(config.DataSource)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ESI data source information");
            return new EsiDataSource
            {
                Name = "tranquility",
                BaseUrl = "https://esi.evetech.net",
                SsoUrl = "https://login.eveonline.com",
                IsTransquility = true,
                Description = "EVE Online live server (default)"
            };
        }
    }

    #region Private Methods

    private EsiApplicationConfig LoadDefaultConfiguration()
    {
        return new EsiApplicationConfig
        {
            ApplicationName = "Gideon - EVE Online AI Copilot",
            ClientId = GetConfiguredClientId(),
            ClientSecret = GetConfiguredClientSecret(),
            EsiBaseUrl = "https://esi.evetech.net",
            SsoBaseUrl = "https://login.eveonline.com",
            UserAgent = $"Gideon-EVE-Copilot/{GetApplicationVersion()} (contact@gideon-eve.com)",
            RedirectUri = "http://localhost:8080/callback",
            DataSource = "tranquility",
            Scopes = new[]
            {
                "publicData",
                "esi-skills.read_skills.v1",
                "esi-skills.read_skillqueue.v1",
                "esi-characters.read_contacts.v1",
                "esi-assets.read_assets.v1",
                "esi-markets.read_character_orders.v1",
                "esi-wallet.read_character_wallet.v1",
                "esi-location.read_location.v1",
                "esi-location.read_ship_type.v1",
                "esi-clones.read_clones.v1"
            },
            TimeoutSeconds = 30,
            MaxRetryAttempts = 3,
            RateLimitPerSecond = 20,
            IsProduction = true
        };
    }

    private async Task<EsiApplicationConfig?> LoadUserConfigurationAsync(CancellationToken cancellationToken)
    {
        try
        {
            var configPath = GetUserConfigPath();
            if (!File.Exists(configPath))
            {
                return null;
            }
            
            var configJson = await File.ReadAllTextAsync(configPath, cancellationToken);
            var config = JsonSerializer.Deserialize<EsiApplicationConfig>(configJson, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            });
            
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user ESI configuration");
            return null;
        }
    }

    private void ValidateConfiguration()
    {
        if (!_defaultConfig.IsValid)
        {
            _logger.LogError("Default ESI configuration is invalid");
            throw new InvalidOperationException("Default ESI configuration is invalid");
        }
        
        _logger.LogInformation("ESI configuration validated successfully");
    }

    private string GetConfiguredClientId()
    {
        // In production, this would be the actual registered ESI application client ID
        var clientId = _configuration.GetValue<string>("ESI:ClientId");
        if (!string.IsNullOrEmpty(clientId))
        {
            return clientId;
        }
        
        // Development/demo client ID (would be replaced with real registration)
        return "gideon-eve-copilot-dev";
    }

    private string GetConfiguredClientSecret()
    {
        // In production, this would be securely stored and retrieved
        var clientSecret = _configuration.GetValue<string>("ESI:ClientSecret");
        if (!string.IsNullOrEmpty(clientSecret))
        {
            return clientSecret;
        }
        
        // Development placeholder (would be replaced with real secret)
        return "";
    }

    private string GetUserConfigPath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var configDir = Path.Combine(appDataPath, "Gideon", "Configuration");
        Directory.CreateDirectory(configDir);
        return Path.Combine(configDir, "esi-config.json");
    }

    private static string GetApplicationVersion()
    {
        return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
    }

    private static string GetDataSourceDescription(string dataSource)
    {
        return dataSource.ToLowerInvariant() switch
        {
            "tranquility" => "EVE Online live server",
            "singularity" => "EVE Online test server (Sisi)",
            _ => $"Unknown data source: {dataSource}"
        };
    }

    #endregion
}

/// <summary>
/// ESI server status and connectivity service
/// </summary>
public class EsiServerStatusService : IEsiServerStatusService
{
    private readonly IEsiApiService _esiApiService;
    private readonly ILogger<EsiServerStatusService> _logger;
    private readonly Timer _statusCheckTimer;
    private EsiServerStatus _lastKnownStatus;

    public EsiServerStatusService(IEsiApiService esiApiService, ILogger<EsiServerStatusService> logger)
    {
        _esiApiService = esiApiService ?? throw new ArgumentNullException(nameof(esiApiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _lastKnownStatus = new EsiServerStatus { IsOnline = false };
        
        // Check server status every 5 minutes
        _statusCheckTimer = new Timer(CheckServerStatus, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
    }

    /// <summary>
    /// Get current ESI server status
    /// </summary>
    public async Task<EsiServerStatus> GetServerStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var status = await _esiApiService.GetServerStatusAsync(cancellationToken);
            _lastKnownStatus = status;
            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ESI server status");
            return _lastKnownStatus;
        }
    }

    /// <summary>
    /// Check if ESI is currently experiencing issues
    /// </summary>
    public async Task<EsiHealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var status = await GetServerStatusAsync(cancellationToken);
            var health = new EsiHealthStatus
            {
                IsHealthy = status.IsOnline,
                ServerStatus = status,
                LastCheck = DateTime.UtcNow,
                Issues = new List<string>()
            };

            if (!status.IsOnline)
            {
                health.Issues.Add("ESI server is offline or unreachable");
            }
            
            if (status.IsOnline && status.PlayerCount < 1000)
            {
                health.Issues.Add("Unusually low player count - possible server issues");
            }
            
            return health;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking ESI health status");
            return new EsiHealthStatus
            {
                IsHealthy = false,
                LastCheck = DateTime.UtcNow,
                Issues = new List<string> { $"Health check failed: {ex.Message}" }
            };
        }
    }

    private async void CheckServerStatus(object? state)
    {
        try
        {
            var status = await GetServerStatusAsync(CancellationToken.None);
            
            if (status.IsOnline != _lastKnownStatus.IsOnline)
            {
                var statusText = status.IsOnline ? "ONLINE" : "OFFLINE";
                _logger.LogInformation("ESI server status changed: {Status}", statusText);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during automatic server status check");
        }
    }

    public void Dispose()
    {
        _statusCheckTimer?.Dispose();
    }
}

#endregion

#region Supporting Data Structures

/// <summary>
/// ESI application configuration
/// </summary>
public class EsiApplicationConfig
{
    public string ApplicationName { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string EsiBaseUrl { get; set; } = string.Empty;
    public string SsoBaseUrl { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public string[] Scopes { get; set; } = Array.Empty<string>();
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetryAttempts { get; set; } = 3;
    public int RateLimitPerSecond { get; set; } = 20;
    public bool IsProduction { get; set; } = true;
    
    public bool IsValid => 
        !string.IsNullOrEmpty(ApplicationName) &&
        !string.IsNullOrEmpty(ClientId) &&
        !string.IsNullOrEmpty(EsiBaseUrl) &&
        !string.IsNullOrEmpty(SsoBaseUrl) &&
        !string.IsNullOrEmpty(UserAgent) &&
        !string.IsNullOrEmpty(RedirectUri) &&
        !string.IsNullOrEmpty(DataSource) &&
        Scopes.Length > 0;
}

/// <summary>
/// ESI configuration test result
/// </summary>
public class EsiConfigTestResult
{
    public EsiApplicationConfig Config { get; set; } = new();
    public bool IsValidConfig { get; set; }
    public bool EsiConnectivity { get; set; }
    public bool OAuthEndpoints { get; set; }
    public bool IsSuccessful { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime TestDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// ESI data source information
/// </summary>
public class EsiDataSource
{
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string SsoUrl { get; set; } = string.Empty;
    public bool IsTransquility { get; set; }
    public bool IsSingularity { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// ESI health status
/// </summary>
public class EsiHealthStatus
{
    public bool IsHealthy { get; set; }
    public EsiServerStatus ServerStatus { get; set; } = new();
    public DateTime LastCheck { get; set; }
    public List<string> Issues { get; set; } = new();
}

/// <summary>
/// Gideon feature flags for scope generation
/// </summary>
[Flags]
public enum GideonFeature
{
    None = 0,
    CharacterData = 1,
    Skills = 2,
    Assets = 4,
    Market = 8,
    Location = 16,
    Corporation = 32,
    Fleet = 64,
    All = CharacterData | Skills | Assets | Market | Location | Corporation | Fleet
}

/// <summary>
/// ESI configuration service interface
/// </summary>
public interface IEsiConfigurationService
{
    Task<EsiApplicationConfig> GetConfigurationAsync(CancellationToken cancellationToken = default);
    Task<bool> SaveUserConfigurationAsync(EsiApplicationConfig config, CancellationToken cancellationToken = default);
    Task<string[]> GetRequiredScopesAsync(GideonFeature features = GideonFeature.All, CancellationToken cancellationToken = default);
    Task<EsiConfigTestResult> TestConfigurationAsync(EsiApplicationConfig config, CancellationToken cancellationToken = default);
    Task<EsiDataSource> GetDataSourceAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// ESI server status service interface
/// </summary>
public interface IEsiServerStatusService : IDisposable
{
    Task<EsiServerStatus> GetServerStatusAsync(CancellationToken cancellationToken = default);
    Task<EsiHealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default);
}

#endregion