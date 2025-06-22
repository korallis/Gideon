// ==========================================================================
// EsiApiServices.cs - EVE Swagger Interface (ESI) API Service Implementations
// ==========================================================================
// Comprehensive ESI API client with rate limiting, error handling, and resilience patterns.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Gideon.WPF.Core.Application.Services;
using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Core.Domain.Interfaces;
using System.Text.Json;
using System.Net.Http;
using System.Text;
using Polly;
using Polly.Extensions.Http;

namespace Gideon.WPF.Core.Infrastructure.Services;

#region ESI API Core Services

/// <summary>
/// Core ESI API client with rate limiting and error handling
/// </summary>
public class EsiApiService : IEsiApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EsiApiService> _logger;
    private readonly IAuditLogService _auditService;
    private readonly IConfiguration _configuration;
    private readonly IRateLimitService _rateLimitService;
    
    // ESI API Configuration
    private readonly string _baseUrl;
    private readonly string _userAgent;
    private readonly TimeSpan _timeout;
    private readonly int _maxRetryAttempts;
    
    // Polly resilience pipeline
    private readonly ResiliencePipeline _resiliencePipeline;

    public EsiApiService(
        HttpClient httpClient,
        ILogger<EsiApiService> logger,
        IAuditLogService auditService,
        IConfiguration configuration,
        IRateLimitService rateLimitService)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _rateLimitService = rateLimitService ?? throw new ArgumentNullException(nameof(rateLimitService));
        
        // Load configuration
        _baseUrl = _configuration.GetValue<string>("ESI:BaseUrl") ?? "https://esi.evetech.net";
        _userAgent = _configuration.GetValue<string>("ESI:UserAgent") ?? "Gideon-EVE-Copilot/1.0 (contact@gideon-eve.com)";
        _timeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("ESI:TimeoutSeconds", 30));
        _maxRetryAttempts = _configuration.GetValue<int>("ESI:MaxRetryAttempts", 3);
        
        // Configure HTTP client
        ConfigureHttpClient();
        
        // Build resilience pipeline
        _resiliencePipeline = BuildResiliencePipeline();
    }

    /// <summary>
    /// Execute GET request to ESI API
    /// </summary>
    public async Task<T?> GetAsync<T>(string endpoint, string? accessToken = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("ESI GET request to: {Endpoint}", endpoint);
            
            // Check rate limiting
            await _rateLimitService.WaitForAvailabilityAsync("ESI", cancellationToken);
            
            var request = CreateHttpRequest(HttpMethod.Get, endpoint, accessToken);
            
            var response = await _resiliencePipeline.ExecuteAsync(async (ct) =>
            {
                var httpResponse = await _httpClient.SendAsync(request, ct);
                await HandleRateLimitHeaders(httpResponse);
                return httpResponse;
            }, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<T>(content, GetJsonSerializerOptions());
                
                _logger.LogDebug("ESI GET successful: {Endpoint}", endpoint);
                await _auditService.LogActionAsync("esi_api_request", "ESI", endpoint, cancellationToken);
                
                return result;
            }
            
            await HandleErrorResponse(response, endpoint, cancellationToken);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing ESI GET request to {Endpoint}", endpoint);
            throw new EsiApiException($"ESI GET request failed: {endpoint}", ex);
        }
    }

    /// <summary>
    /// Execute POST request to ESI API
    /// </summary>
    public async Task<T?> PostAsync<T>(string endpoint, object? data = null, string? accessToken = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("ESI POST request to: {Endpoint}", endpoint);
            
            // Check rate limiting
            await _rateLimitService.WaitForAvailabilityAsync("ESI", cancellationToken);
            
            var request = CreateHttpRequest(HttpMethod.Post, endpoint, accessToken, data);
            
            var response = await _resiliencePipeline.ExecuteAsync(async (ct) =>
            {
                var httpResponse = await _httpClient.SendAsync(request, ct);
                await HandleRateLimitHeaders(httpResponse);
                return httpResponse;
            }, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<T>(content, GetJsonSerializerOptions());
                
                _logger.LogDebug("ESI POST successful: {Endpoint}", endpoint);
                await _auditService.LogActionAsync("esi_api_request", "ESI", endpoint, cancellationToken);
                
                return result;
            }
            
            await HandleErrorResponse(response, endpoint, cancellationToken);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing ESI POST request to {Endpoint}", endpoint);
            throw new EsiApiException($"ESI POST request failed: {endpoint}", ex);
        }
    }

    /// <summary>
    /// Get current ESI server status
    /// </summary>
    public async Task<EsiServerStatus> GetServerStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var status = await GetAsync<EsiServerStatusResponse>("/latest/status/", cancellationToken: cancellationToken);
            
            return new EsiServerStatus
            {
                IsOnline = status?.players != null,
                PlayerCount = status?.players ?? 0,
                ServerVersion = status?.server_version ?? "Unknown",
                StartTime = status?.start_time ?? DateTime.UtcNow,
                Vip = status?.vip ?? false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ESI server status");
            return new EsiServerStatus { IsOnline = false };
        }
    }

    #region HTTP Configuration and Utilities

    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.Timeout = _timeout;
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", _userAgent);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    private HttpRequestMessage CreateHttpRequest(HttpMethod method, string endpoint, string? accessToken, object? data = null)
    {
        var request = new HttpRequestMessage(method, endpoint);
        
        // Add authorization header if token provided
        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        }
        
        // Add request body for POST requests
        if (method == HttpMethod.Post && data != null)
        {
            var jsonContent = JsonSerializer.Serialize(data, GetJsonSerializerOptions());
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        }
        
        return request;
    }

    private ResiliencePipeline BuildResiliencePipeline()
    {
        return new ResiliencePipelineBuilder()
            .AddRetry(new Polly.Retry.RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
                MaxRetryAttempts = _maxRetryAttempts,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = Polly.Retry.DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    _logger.LogWarning("ESI request retry {AttemptNumber}/{MaxRetryAttempts}: {Exception}", 
                        args.AttemptNumber, _maxRetryAttempts, args.Outcome.Exception?.Message);
                    return ValueTask.CompletedTask;
                }
            })
            .AddCircuitBreaker(new Polly.CircuitBreaker.CircuitBreakerStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 5,
                BreakDuration = TimeSpan.FromMinutes(1),
                OnOpened = args =>
                {
                    _logger.LogWarning("ESI circuit breaker opened due to high failure rate");
                    return ValueTask.CompletedTask;
                },
                OnClosed = args =>
                {
                    _logger.LogInformation("ESI circuit breaker closed - service recovered");
                    return ValueTask.CompletedTask;
                }
            })
            .AddTimeout(TimeSpan.FromSeconds(45))
            .Build();
    }

    private async Task HandleRateLimitHeaders(HttpResponseMessage response)
    {
        // Handle ESI rate limiting headers
        if (response.Headers.TryGetValues("X-ESI-Error-Limit-Remain", out var remainingValues))
        {
            if (int.TryParse(remainingValues.First(), out var remaining))
            {
                await _rateLimitService.UpdateRateLimitAsync("ESI", remaining, DateTime.UtcNow.AddMinutes(1));
                
                if (remaining < 10)
                {
                    _logger.LogWarning("ESI rate limit approaching: {Remaining} requests remaining", remaining);
                }
            }
        }
        
        // Handle 420 rate limit exceeded
        if ((int)response.StatusCode == 420)
        {
            _logger.LogWarning("ESI rate limit exceeded (HTTP 420)");
            await _rateLimitService.HandleRateLimitExceededAsync("ESI", TimeSpan.FromMinutes(1));
            throw new EsiRateLimitException("ESI rate limit exceeded");
        }
    }

    private async Task HandleErrorResponse(HttpResponseMessage response, string endpoint, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var errorMessage = $"ESI API error: {response.StatusCode} - {content}";
        
        _logger.LogError("ESI API error for {Endpoint}: {StatusCode} - {Content}", 
            endpoint, response.StatusCode, content);
        
        // Handle specific HTTP status codes
        switch ((int)response.StatusCode)
        {
            case 400:
                throw new EsiBadRequestException($"Bad request to {endpoint}: {content}");
            case 401:
                throw new EsiUnauthorizedException($"Unauthorized request to {endpoint}");
            case 403:
                throw new EsiForbiddenException($"Forbidden request to {endpoint}");
            case 404:
                throw new EsiNotFoundException($"Resource not found: {endpoint}");
            case 420:
                throw new EsiRateLimitException("Rate limit exceeded");
            case 500:
            case 502:
            case 503:
            case 504:
                throw new EsiServerException($"ESI server error ({response.StatusCode}): {content}");
            default:
                throw new EsiApiException(errorMessage);
        }
    }

    private static JsonSerializerOptions GetJsonSerializerOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    #endregion
}

/// <summary>
/// Rate limiting service for ESI API calls
/// </summary>
public class RateLimitService : IRateLimitService
{
    private readonly ILogger<RateLimitService> _logger;
    private readonly Dictionary<string, RateLimitInfo> _rateLimits = new();
    private readonly object _lock = new();

    public RateLimitService(ILogger<RateLimitService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Wait for API availability respecting rate limits
    /// </summary>
    public async Task WaitForAvailabilityAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_rateLimits.TryGetValue(apiKey, out var rateLimit))
            {
                if (rateLimit.IsRateLimited)
                {
                    var waitTime = rateLimit.ResetTime - DateTime.UtcNow;
                    if (waitTime > TimeSpan.Zero)
                    {
                        _logger.LogInformation("Rate limited for {ApiKey}, waiting {WaitTime}ms", 
                            apiKey, waitTime.TotalMilliseconds);
                        Task.Delay(waitTime, cancellationToken).Wait(cancellationToken);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Update rate limit information
    /// </summary>
    public async Task UpdateRateLimitAsync(string apiKey, int remainingRequests, DateTime resetTime)
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                _rateLimits[apiKey] = new RateLimitInfo
                {
                    RemainingRequests = remainingRequests,
                    ResetTime = resetTime,
                    LastUpdated = DateTime.UtcNow
                };
            }
        });
    }

    /// <summary>
    /// Handle rate limit exceeded scenario
    /// </summary>
    public async Task HandleRateLimitExceededAsync(string apiKey, TimeSpan cooldownPeriod)
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                _rateLimits[apiKey] = new RateLimitInfo
                {
                    RemainingRequests = 0,
                    ResetTime = DateTime.UtcNow.Add(cooldownPeriod),
                    LastUpdated = DateTime.UtcNow
                };
                
                _logger.LogWarning("Rate limit exceeded for {ApiKey}, cooldown until {ResetTime}", 
                    apiKey, _rateLimits[apiKey].ResetTime);
            }
        });
    }

    /// <summary>
    /// Get current rate limit status
    /// </summary>
    public async Task<RateLimitStatus> GetRateLimitStatusAsync(string apiKey)
    {
        return await Task.Run(() =>
        {
            lock (_lock)
            {
                if (_rateLimits.TryGetValue(apiKey, out var rateLimit))
                {
                    return new RateLimitStatus
                    {
                        ApiKey = apiKey,
                        RemainingRequests = rateLimit.RemainingRequests,
                        ResetTime = rateLimit.ResetTime,
                        IsRateLimited = rateLimit.IsRateLimited
                    };
                }
                
                return new RateLimitStatus
                {
                    ApiKey = apiKey,
                    RemainingRequests = 100, // Default for ESI
                    ResetTime = DateTime.UtcNow.AddMinutes(1),
                    IsRateLimited = false
                };
            }
        });
    }
}

#endregion

#region ESI Specialized Services

/// <summary>
/// ESI Character service implementation
/// </summary>
public class EsiCharacterService : IEsiCharacterService
{
    private readonly IEsiApiService _esiApi;
    private readonly ILogger<EsiCharacterService> _logger;
    private readonly IAuditLogService _auditService;

    public EsiCharacterService(
        IEsiApiService esiApi,
        ILogger<EsiCharacterService> logger,
        IAuditLogService auditService)
    {
        _esiApi = esiApi ?? throw new ArgumentNullException(nameof(esiApi));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    /// <summary>
    /// Get character information
    /// </summary>
    public async Task<Character?> GetCharacterAsync(long characterId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var characterInfo = await _esiApi.GetAsync<EsiCharacterResponse>(
                $"/latest/characters/{characterId}/", accessToken, cancellationToken);
            
            if (characterInfo == null)
                return null;

            var character = new Character
            {
                CharacterId = characterId,
                CharacterName = characterInfo.name,
                CorporationId = characterInfo.corporation_id,
                AllianceId = characterInfo.alliance_id,
                SecurityStatus = characterInfo.security_status,
                TotalSp = 0, // Would need separate API call
                LastLoginDate = DateTime.UtcNow,
                IsActive = true
            };

            _logger.LogDebug("Retrieved character information for {CharacterId}: {CharacterName}", 
                characterId, character.CharacterName);
            
            return character;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving character {CharacterId}", characterId);
            return null;
        }
    }

    /// <summary>
    /// Get character skills
    /// </summary>
    public async Task<IEnumerable<CharacterSkill>> GetCharacterSkillsAsync(long characterId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var skillsResponse = await _esiApi.GetAsync<EsiSkillsResponse>(
                $"/latest/characters/{characterId}/skills/", accessToken, cancellationToken);
            
            if (skillsResponse?.skills == null)
                return Enumerable.Empty<CharacterSkill>();

            var characterSkills = skillsResponse.skills.Select(skill => new CharacterSkill
            {
                CharacterId = (int)characterId,
                SkillId = skill.skill_id,
                ActiveSkillLevel = skill.active_skill_level,
                TrainedSkillLevel = skill.trained_skill_level,
                SkillPointsInSkill = skill.skillpoints_in_skill
            }).ToList();

            _logger.LogDebug("Retrieved {SkillCount} skills for character {CharacterId}", 
                characterSkills.Count, characterId);
            
            return characterSkills;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving skills for character {CharacterId}", characterId);
            return Enumerable.Empty<CharacterSkill>();
        }
    }
}

/// <summary>
/// ESI Market service implementation
/// </summary>
public class EsiMarketService : IEsiMarketService
{
    private readonly IEsiApiService _esiApi;
    private readonly ILogger<EsiMarketService> _logger;

    public EsiMarketService(IEsiApiService esiApi, ILogger<EsiMarketService> logger)
    {
        _esiApi = esiApi ?? throw new ArgumentNullException(nameof(esiApi));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get market orders for a region
    /// </summary>
    public async Task<IEnumerable<MarketData>> GetMarketOrdersAsync(int regionId, int? typeId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = typeId.HasValue 
                ? $"/latest/markets/{regionId}/orders/?type_id={typeId}"
                : $"/latest/markets/{regionId}/orders/";
                
            var orders = await _esiApi.GetAsync<List<EsiMarketOrder>>(endpoint, cancellationToken: cancellationToken);
            
            if (orders == null)
                return Enumerable.Empty<MarketData>();

            var marketData = orders.Select(order => new MarketData
            {
                RegionId = regionId,
                TypeId = order.type_id,
                OrderId = order.order_id,
                IsBuyOrder = order.is_buy_order,
                Price = order.price,
                VolumeRemain = order.volume_remain,
                VolumeTotal = order.volume_total,
                LocationId = order.location_id,
                Duration = order.duration,
                Issued = order.issued,
                RecordedDate = DateTime.UtcNow
            }).ToList();

            _logger.LogDebug("Retrieved {OrderCount} market orders for region {RegionId}", 
                marketData.Count, regionId);
            
            return marketData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving market orders for region {RegionId}", regionId);
            return Enumerable.Empty<MarketData>();
        }
    }

    /// <summary>
    /// Get market history for a type in a region
    /// </summary>
    public async Task<IEnumerable<MarketData>> GetMarketHistoryAsync(int regionId, int typeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = $"/latest/markets/{regionId}/history/?type_id={typeId}";
            var history = await _esiApi.GetAsync<List<EsiMarketHistory>>(endpoint, cancellationToken: cancellationToken);
            
            if (history == null)
                return Enumerable.Empty<MarketData>();

            var marketData = history.Select(entry => new MarketData
            {
                RegionId = regionId,
                TypeId = typeId,
                Date = entry.date,
                Volume = entry.volume,
                OrderCount = entry.order_count,
                LowestPrice = entry.lowest,
                HighestPrice = entry.highest,
                AveragePrice = entry.average,
                RecordedDate = DateTime.UtcNow
            }).ToList();

            _logger.LogDebug("Retrieved {HistoryCount} market history entries for type {TypeId} in region {RegionId}", 
                marketData.Count, typeId, regionId);
            
            return marketData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving market history for type {TypeId} in region {RegionId}", typeId, regionId);
            return Enumerable.Empty<MarketData>();
        }
    }
}

/// <summary>
/// ESI Universe service implementation
/// </summary>
public class EsiUniverseService : IEsiUniverseService
{
    private readonly IEsiApiService _esiApi;
    private readonly ILogger<EsiUniverseService> _logger;

    public EsiUniverseService(IEsiApiService esiApi, ILogger<EsiUniverseService> logger)
    {
        _esiApi = esiApi ?? throw new ArgumentNullException(nameof(esiApi));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get type information
    /// </summary>
    public async Task<EveType?> GetTypeAsync(int typeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var typeInfo = await _esiApi.GetAsync<EsiTypeResponse>(
                $"/latest/universe/types/{typeId}/", cancellationToken: cancellationToken);
            
            if (typeInfo == null)
                return null;

            var eveType = new EveType
            {
                TypeId = typeId,
                TypeName = typeInfo.name,
                Description = typeInfo.description,
                GroupId = typeInfo.group_id,
                CategoryId = typeInfo.category_id,
                Mass = typeInfo.mass,
                Volume = typeInfo.volume,
                Capacity = typeInfo.capacity,
                PortionSize = typeInfo.portion_size,
                BasePrice = typeInfo.base_price,
                IsPublished = typeInfo.published,
                MarketGroupId = typeInfo.market_group_id
            };

            _logger.LogDebug("Retrieved type information for {TypeId}: {TypeName}", typeId, eveType.TypeName);
            
            return eveType;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving type {TypeId}", typeId);
            return null;
        }
    }

    /// <summary>
    /// Get region information
    /// </summary>
    public async Task<EveRegion?> GetRegionAsync(int regionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var regionInfo = await _esiApi.GetAsync<EsiRegionResponse>(
                $"/latest/universe/regions/{regionId}/", cancellationToken: cancellationToken);
            
            if (regionInfo == null)
                return null;

            var eveRegion = new EveRegion
            {
                RegionId = regionId,
                RegionName = regionInfo.name,
                Description = regionInfo.description
            };

            _logger.LogDebug("Retrieved region information for {RegionId}: {RegionName}", regionId, eveRegion.RegionName);
            
            return eveRegion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving region {RegionId}", regionId);
            return null;
        }
    }
}

/// <summary>
/// ESI Skills service implementation
/// </summary>
public class EsiSkillsService : IEsiSkillsService
{
    private readonly IEsiApiService _esiApi;
    private readonly ILogger<EsiSkillsService> _logger;

    public EsiSkillsService(IEsiApiService esiApi, ILogger<EsiSkillsService> logger)
    {
        _esiApi = esiApi ?? throw new ArgumentNullException(nameof(esiApi));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get character skills (same as character service but specialized)
    /// </summary>
    public async Task<IEnumerable<CharacterSkill>> GetCharacterSkillsAsync(long characterId, string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var skillsResponse = await _esiApi.GetAsync<EsiSkillsResponse>(
                $"/latest/characters/{characterId}/skills/", accessToken, cancellationToken);
            
            if (skillsResponse?.skills == null)
                return Enumerable.Empty<CharacterSkill>();

            var characterSkills = skillsResponse.skills.Select(skill => new CharacterSkill
            {
                CharacterId = (int)characterId,
                SkillId = skill.skill_id,
                ActiveSkillLevel = skill.active_skill_level,
                TrainedSkillLevel = skill.trained_skill_level,
                SkillPointsInSkill = skill.skillpoints_in_skill
            }).ToList();

            return characterSkills;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving skills for character {CharacterId}", characterId);
            return Enumerable.Empty<CharacterSkill>();
        }
    }

    /// <summary>
    /// Get skill details (from universe endpoint)
    /// </summary>
    public async Task<CharacterSkill?> GetSkillDetailsAsync(int skillId, CancellationToken cancellationToken = default)
    {
        try
        {
            var skillInfo = await _esiApi.GetAsync<EsiTypeResponse>(
                $"/latest/universe/types/{skillId}/", cancellationToken: cancellationToken);
            
            if (skillInfo == null)
                return null;

            // This would return skill type information, not character-specific skill data
            // For actual character skill details, use GetCharacterSkillsAsync
            return new CharacterSkill
            {
                SkillId = skillId,
                // Other properties would need character context
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving skill details for {SkillId}", skillId);
            return null;
        }
    }
}

#endregion

#region ESI Response DTOs

/// <summary>
/// ESI server status response
/// </summary>
public class EsiServerStatusResponse
{
    public int players { get; set; }
    public string server_version { get; set; } = string.Empty;
    public DateTime start_time { get; set; }
    public bool vip { get; set; }
}

/// <summary>
/// ESI character response
/// </summary>
public class EsiCharacterResponse
{
    public string name { get; set; } = string.Empty;
    public long corporation_id { get; set; }
    public long? alliance_id { get; set; }
    public double security_status { get; set; }
    public DateTime birthday { get; set; }
    public string description { get; set; } = string.Empty;
    public int? faction_id { get; set; }
    public string gender { get; set; } = string.Empty;
    public int race_id { get; set; }
    public string title { get; set; } = string.Empty;
}

/// <summary>
/// ESI skills response
/// </summary>
public class EsiSkillsResponse
{
    public List<EsiSkill> skills { get; set; } = new();
    public long total_sp { get; set; }
    public long unallocated_sp { get; set; }
}

/// <summary>
/// ESI skill entry
/// </summary>
public class EsiSkill
{
    public int skill_id { get; set; }
    public int active_skill_level { get; set; }
    public int trained_skill_level { get; set; }
    public long skillpoints_in_skill { get; set; }
}

/// <summary>
/// ESI market order response
/// </summary>
public class EsiMarketOrder
{
    public long order_id { get; set; }
    public int type_id { get; set; }
    public bool is_buy_order { get; set; }
    public double price { get; set; }
    public int volume_remain { get; set; }
    public int volume_total { get; set; }
    public long location_id { get; set; }
    public int duration { get; set; }
    public DateTime issued { get; set; }
    public double? min_volume { get; set; }
    public string range { get; set; } = string.Empty;
}

/// <summary>
/// ESI market history response
/// </summary>
public class EsiMarketHistory
{
    public DateTime date { get; set; }
    public long volume { get; set; }
    public int order_count { get; set; }
    public double lowest { get; set; }
    public double highest { get; set; }
    public double average { get; set; }
}

/// <summary>
/// ESI type response
/// </summary>
public class EsiTypeResponse
{
    public string name { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public int group_id { get; set; }
    public int category_id { get; set; }
    public double? mass { get; set; }
    public double? volume { get; set; }
    public double? capacity { get; set; }
    public int? portion_size { get; set; }
    public double? base_price { get; set; }
    public bool published { get; set; }
    public int? market_group_id { get; set; }
    public List<EsiTypeAttribute> dogma_attributes { get; set; } = new();
    public List<EsiTypeEffect> dogma_effects { get; set; } = new();
}

/// <summary>
/// ESI type attribute
/// </summary>
public class EsiTypeAttribute
{
    public int attribute_id { get; set; }
    public double value { get; set; }
}

/// <summary>
/// ESI type effect
/// </summary>
public class EsiTypeEffect
{
    public int effect_id { get; set; }
    public bool is_default { get; set; }
}

/// <summary>
/// ESI region response
/// </summary>
public class EsiRegionResponse
{
    public string name { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public List<int> constellations { get; set; } = new();
}

#endregion

#region Supporting Data Structures

/// <summary>
/// ESI server status information
/// </summary>
public class EsiServerStatus
{
    public bool IsOnline { get; set; }
    public int PlayerCount { get; set; }
    public string ServerVersion { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public bool Vip { get; set; }
}

/// <summary>
/// Rate limit information
/// </summary>
public class RateLimitInfo
{
    public int RemainingRequests { get; set; }
    public DateTime ResetTime { get; set; }
    public DateTime LastUpdated { get; set; }
    public bool IsRateLimited => RemainingRequests <= 0 && ResetTime > DateTime.UtcNow;
}

/// <summary>
/// Rate limit status
/// </summary>
public class RateLimitStatus
{
    public string ApiKey { get; set; } = string.Empty;
    public int RemainingRequests { get; set; }
    public DateTime ResetTime { get; set; }
    public bool IsRateLimited { get; set; }
}

/// <summary>
/// Rate limit service interface
/// </summary>
public interface IRateLimitService
{
    Task WaitForAvailabilityAsync(string apiKey, CancellationToken cancellationToken = default);
    Task UpdateRateLimitAsync(string apiKey, int remainingRequests, DateTime resetTime);
    Task HandleRateLimitExceededAsync(string apiKey, TimeSpan cooldownPeriod);
    Task<RateLimitStatus> GetRateLimitStatusAsync(string apiKey);
}

#endregion

#region ESI Exception Types

/// <summary>
/// Base ESI API exception
/// </summary>
public class EsiApiException : Exception
{
    public EsiApiException(string message) : base(message) { }
    public EsiApiException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// ESI bad request exception (400)
/// </summary>
public class EsiBadRequestException : EsiApiException
{
    public EsiBadRequestException(string message) : base(message) { }
}

/// <summary>
/// ESI unauthorized exception (401)
/// </summary>
public class EsiUnauthorizedException : EsiApiException
{
    public EsiUnauthorizedException(string message) : base(message) { }
}

/// <summary>
/// ESI forbidden exception (403)
/// </summary>
public class EsiForbiddenException : EsiApiException
{
    public EsiForbiddenException(string message) : base(message) { }
}

/// <summary>
/// ESI not found exception (404)
/// </summary>
public class EsiNotFoundException : EsiApiException
{
    public EsiNotFoundException(string message) : base(message) { }
}

/// <summary>
/// ESI rate limit exception (420)
/// </summary>
public class EsiRateLimitException : EsiApiException
{
    public EsiRateLimitException(string message) : base(message) { }
}

/// <summary>
/// ESI server exception (5xx)
/// </summary>
public class EsiServerException : EsiApiException
{
    public EsiServerException(string message) : base(message) { }
}

#endregion