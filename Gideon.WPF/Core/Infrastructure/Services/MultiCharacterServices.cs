// ==========================================================================
// MultiCharacterServices.cs - Multi-Character Authentication and Management Services
// ==========================================================================
// Enhanced services for managing 20+ EVE Online characters per account with session persistence.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.Extensions.Logging;
using Gideon.WPF.Core.Application.Services;
using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Core.Domain.Interfaces;
using System.Text.Json;
using System.Collections.Concurrent;

namespace Gideon.WPF.Core.Infrastructure.Services;

#region Multi-Character Management Services

/// <summary>
/// Enhanced multi-character authentication and session management service
/// </summary>
public class MultiCharacterService : IMultiCharacterService
{
    private readonly IAuthenticationService _authService;
    private readonly ITokenStorageService _tokenStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MultiCharacterService> _logger;
    private readonly IAuditLogService _auditService;
    private readonly ISessionPersistenceService _sessionService;
    
    // Thread-safe collections for high-performance character management
    private readonly ConcurrentDictionary<long, AuthenticatedCharacter> _authenticatedCharacters = new();
    private readonly ConcurrentDictionary<string, CharacterGroup> _characterGroups = new();
    private readonly Timer _healthCheckTimer;
    private readonly Timer _bulkRefreshTimer;
    
    // Configuration
    private readonly int _maxCharactersPerAccount = 25; // EVE Online character limit + buffer
    private readonly TimeSpan _sessionTimeout = TimeSpan.FromHours(12);
    
    public MultiCharacterService(
        IAuthenticationService authService,
        ITokenStorageService tokenStorage,
        IUnitOfWork unitOfWork,
        ILogger<MultiCharacterService> logger,
        IAuditLogService auditService,
        ISessionPersistenceService sessionService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _tokenStorage = tokenStorage ?? throw new ArgumentNullException(nameof(tokenStorage));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        
        // Set up background health check and bulk refresh timers
        _healthCheckTimer = new Timer(PerformHealthCheck, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        _bulkRefreshTimer = new Timer(PerformBulkTokenRefresh, null, TimeSpan.FromMinutes(45), TimeSpan.FromMinutes(45));
        
        // Load existing sessions on startup
        _ = Task.Run(LoadPersistedSessionsAsync);
    }

    /// <summary>
    /// Get all authenticated characters
    /// </summary>
    public async Task<IEnumerable<AuthenticatedCharacter>> GetAllCharactersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Return cached characters if available
            if (_authenticatedCharacters.Count > 0)
            {
                return _authenticatedCharacters.Values.Where(c => c.IsTokenValid).ToList();
            }
            
            // Load from authentication service
            var characters = await _authService.GetAllAuthenticatedCharactersAsync(cancellationToken);
            
            // Cache the characters
            foreach (var character in characters)
            {
                _authenticatedCharacters.TryAdd(character.CharacterId, character);
            }
            
            _logger.LogInformation("Loaded {CharacterCount} authenticated characters", characters.Count());
            return characters;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all authenticated characters");
            return Enumerable.Empty<AuthenticatedCharacter>();
        }
    }

    /// <summary>
    /// Add a new character to the multi-character session
    /// </summary>
    public async Task<bool> AddCharacterAsync(long characterId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check character limit
            if (_authenticatedCharacters.Count >= _maxCharactersPerAccount)
            {
                _logger.LogWarning("Character limit reached ({MaxCharacters}). Cannot add character {CharacterId}", 
                    _maxCharactersPerAccount, characterId);
                return false;
            }
            
            // Check if character is already authenticated
            if (_authenticatedCharacters.ContainsKey(characterId))
            {
                _logger.LogInformation("Character {CharacterId} is already authenticated", characterId);
                return true;
            }
            
            // Get character from authentication service
            var character = await _authService.GetAuthenticatedCharacterAsync(characterId, cancellationToken);
            if (character == null)
            {
                _logger.LogWarning("Character {CharacterId} not found or not authenticated", characterId);
                return false;
            }
            
            // Add to cache
            _authenticatedCharacters.TryAdd(characterId, character);
            
            // Persist session
            await _sessionService.SaveCharacterSessionAsync(character, cancellationToken);
            
            // Log audit trail
            await _auditService.LogActionAsync("character_added_to_session", "Character", 
                characterId.ToString(), cancellationToken);
            
            _logger.LogInformation("Added character {CharacterName} ({CharacterId}) to multi-character session", 
                character.CharacterName, characterId);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding character {CharacterId} to session", characterId);
            return false;
        }
    }

    /// <summary>
    /// Remove a character from the multi-character session
    /// </summary>
    public async Task<bool> RemoveCharacterAsync(long characterId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_authenticatedCharacters.TryRemove(characterId, out var character))
            {
                _logger.LogWarning("Character {CharacterId} not found in session", characterId);
                return false;
            }
            
            // Remove from authentication service
            await _authService.RemoveCharacterAsync(characterId, cancellationToken);
            
            // Remove from session persistence
            await _sessionService.RemoveCharacterSessionAsync(characterId, cancellationToken);
            
            // Log audit trail
            await _auditService.LogActionAsync("character_removed_from_session", "Character", 
                characterId.ToString(), cancellationToken);
            
            _logger.LogInformation("Removed character {CharacterName} ({CharacterId}) from multi-character session", 
                character.CharacterName, characterId);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing character {CharacterId} from session", characterId);
            return false;
        }
    }

    /// <summary>
    /// Switch active character context
    /// </summary>
    public async Task<bool> SwitchActiveCharacterAsync(long characterId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_authenticatedCharacters.TryGetValue(characterId, out var character))
            {
                _logger.LogWarning("Character {CharacterId} not found in session", characterId);
                return false;
            }
            
            // Verify token is still valid
            if (!character.IsTokenValid)
            {
                _logger.LogWarning("Character {CharacterId} token is expired, attempting refresh", characterId);
                var refreshed = await _authService.RefreshCharacterTokenAsync(characterId, cancellationToken);
                if (!refreshed)
                {
                    _logger.LogError("Failed to refresh token for character {CharacterId}", characterId);
                    return false;
                }
                
                // Update cached character
                var updatedCharacter = await _authService.GetAuthenticatedCharacterAsync(characterId, cancellationToken);
                if (updatedCharacter != null)
                {
                    _authenticatedCharacters.TryUpdate(characterId, updatedCharacter, character);
                    character = updatedCharacter;
                }
            }
            
            // Update session with active character
            await _sessionService.SetActiveCharacterAsync(characterId, cancellationToken);
            
            // Log audit trail
            await _auditService.LogActionAsync("character_switched", "Character", 
                characterId.ToString(), cancellationToken);
            
            _logger.LogInformation("Switched to character {CharacterName} ({CharacterId})", 
                character.CharacterName, characterId);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error switching to character {CharacterId}", characterId);
            return false;
        }
    }

    /// <summary>
    /// Create a character group for organizational purposes
    /// </summary>
    public async Task<string> CreateCharacterGroupAsync(string groupName, IEnumerable<long> characterIds, CancellationToken cancellationToken = default)
    {
        try
        {
            var groupId = Guid.NewGuid().ToString();
            var characterGroup = new CharacterGroup
            {
                GroupId = groupId,
                GroupName = groupName,
                CharacterIds = characterIds.ToList(),
                CreatedDate = DateTime.UtcNow
            };
            
            _characterGroups.TryAdd(groupId, characterGroup);
            
            // Persist group
            await _sessionService.SaveCharacterGroupAsync(characterGroup, cancellationToken);
            
            _logger.LogInformation("Created character group '{GroupName}' with {CharacterCount} characters", 
                groupName, characterIds.Count());
            
            return groupId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating character group '{GroupName}'", groupName);
            throw;
        }
    }

    /// <summary>
    /// Get character groups
    /// </summary>
    public async Task<IEnumerable<CharacterGroup>> GetCharacterGroupsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_characterGroups.Count == 0)
            {
                // Load from persistence
                var groups = await _sessionService.LoadCharacterGroupsAsync(cancellationToken);
                foreach (var group in groups)
                {
                    _characterGroups.TryAdd(group.GroupId, group);
                }
            }
            
            return _characterGroups.Values.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving character groups");
            return Enumerable.Empty<CharacterGroup>();
        }
    }

    /// <summary>
    /// Perform bulk token refresh for all characters
    /// </summary>
    public async Task<BulkOperationResult> RefreshAllTokensAsync(CancellationToken cancellationToken = default)
    {
        var result = new BulkOperationResult();
        var tasks = new List<Task>();
        
        try
        {
            _logger.LogInformation("Starting bulk token refresh for {CharacterCount} characters", 
                _authenticatedCharacters.Count);
            
            foreach (var character in _authenticatedCharacters.Values)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var refreshed = await _authService.RefreshCharacterTokenAsync(character.CharacterId, cancellationToken);
                        if (refreshed)
                        {
                            Interlocked.Increment(ref result.SuccessCount);
                        }
                        else
                        {
                            Interlocked.Increment(ref result.FailureCount);
                            result.Failures.Add($"Failed to refresh token for character {character.CharacterId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref result.FailureCount);
                        result.Failures.Add($"Error refreshing character {character.CharacterId}: {ex.Message}");
                    }
                }, cancellationToken));
            }
            
            await Task.WhenAll(tasks);
            
            _logger.LogInformation("Bulk token refresh completed: {SuccessCount} successful, {FailureCount} failed", 
                result.SuccessCount, result.FailureCount);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk token refresh");
            result.Failures.Add($"Bulk operation error: {ex.Message}");
            return result;
        }
    }

    /// <summary>
    /// Get session statistics
    /// </summary>
    public async Task<MultiCharacterSessionStats> GetSessionStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var characters = await GetAllCharactersAsync(cancellationToken);
            var validTokens = characters.Count(c => c.IsTokenValid);
            var expiredTokens = characters.Count() - validTokens;
            
            var stats = new MultiCharacterSessionStats
            {
                TotalCharacters = characters.Count(),
                ValidTokens = validTokens,
                ExpiredTokens = expiredTokens,
                CharacterGroups = _characterGroups.Count,
                SessionUptime = DateTime.UtcNow - await _sessionService.GetSessionStartTimeAsync(),
                LastHealthCheck = DateTime.UtcNow
            };
            
            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating session statistics");
            return new MultiCharacterSessionStats();
        }
    }

    #region Background Services

    private async void PerformHealthCheck(object? state)
    {
        try
        {
            _logger.LogDebug("Performing multi-character session health check");
            
            var expiredCharacters = _authenticatedCharacters.Values
                .Where(c => !c.IsTokenValid)
                .ToList();
            
            if (expiredCharacters.Any())
            {
                _logger.LogInformation("Found {ExpiredCount} characters with expired tokens", expiredCharacters.Count);
                
                // Remove expired characters that can't be refreshed
                foreach (var character in expiredCharacters)
                {
                    var refreshed = await _authService.RefreshCharacterTokenAsync(character.CharacterId, CancellationToken.None);
                    if (!refreshed)
                    {
                        _logger.LogWarning("Removing character {CharacterId} due to unrefreshable token", character.CharacterId);
                        _authenticatedCharacters.TryRemove(character.CharacterId, out _);
                    }
                }
            }
            
            // Clean up old session data
            await _sessionService.CleanupExpiredSessionsAsync(_sessionTimeout, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during health check");
        }
    }

    private async void PerformBulkTokenRefresh(object? state)
    {
        try
        {
            _logger.LogDebug("Performing automatic bulk token refresh");
            
            var result = await RefreshAllTokensAsync(CancellationToken.None);
            
            if (result.FailureCount > 0)
            {
                _logger.LogWarning("Bulk token refresh completed with {FailureCount} failures", result.FailureCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during automatic bulk token refresh");
        }
    }

    private async Task LoadPersistedSessionsAsync()
    {
        try
        {
            _logger.LogInformation("Loading persisted character sessions");
            
            var sessions = await _sessionService.LoadAllSessionsAsync();
            foreach (var session in sessions)
            {
                // Validate session is not expired
                if (session.LastAccessed > DateTime.UtcNow - _sessionTimeout)
                {
                    _authenticatedCharacters.TryAdd(session.CharacterId, session);
                }
            }
            
            _logger.LogInformation("Loaded {SessionCount} persisted character sessions", _authenticatedCharacters.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading persisted sessions");
        }
    }

    #endregion

    public void Dispose()
    {
        _healthCheckTimer?.Dispose();
        _bulkRefreshTimer?.Dispose();
    }
}

/// <summary>
/// Session persistence service for multi-character management
/// </summary>
public class SessionPersistenceService : ISessionPersistenceService
{
    private readonly ILogger<SessionPersistenceService> _logger;
    private readonly string _sessionDirectory;

    public SessionPersistenceService(ILogger<SessionPersistenceService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _sessionDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Gideon", "Sessions");
        
        EnsureSessionDirectoryExists();
    }

    /// <summary>
    /// Save character session data
    /// </summary>
    public async Task SaveCharacterSessionAsync(AuthenticatedCharacter character, CancellationToken cancellationToken = default)
    {
        try
        {
            var sessionData = new CharacterSession
            {
                CharacterId = character.CharacterId,
                CharacterName = character.CharacterName,
                CorporationId = character.CorporationId,
                AllianceId = character.AllianceId,
                TokenExpiry = character.TokenExpiry,
                LastAccessed = DateTime.UtcNow,
                SessionId = Guid.NewGuid().ToString()
            };

            var sessionPath = GetSessionFilePath(character.CharacterId);
            var jsonData = JsonSerializer.Serialize(sessionData, new JsonSerializerOptions { WriteIndented = true });
            
            await File.WriteAllTextAsync(sessionPath, jsonData, cancellationToken);
            
            _logger.LogDebug("Saved session for character {CharacterId}", character.CharacterId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving session for character {CharacterId}", character.CharacterId);
        }
    }

    /// <summary>
    /// Remove character session data
    /// </summary>
    public async Task RemoveCharacterSessionAsync(long characterId, CancellationToken cancellationToken = default)
    {
        try
        {
            var sessionPath = GetSessionFilePath(characterId);
            if (File.Exists(sessionPath))
            {
                await Task.Run(() => File.Delete(sessionPath), cancellationToken);
                _logger.LogDebug("Removed session for character {CharacterId}", characterId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing session for character {CharacterId}", characterId);
        }
    }

    /// <summary>
    /// Load all persisted sessions
    /// </summary>
    public async Task<IEnumerable<AuthenticatedCharacter>> LoadAllSessionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var sessions = new List<AuthenticatedCharacter>();
            var sessionFiles = Directory.GetFiles(_sessionDirectory, "session_*.json");
            
            foreach (var sessionFile in sessionFiles)
            {
                try
                {
                    var jsonData = await File.ReadAllTextAsync(sessionFile, cancellationToken);
                    var sessionData = JsonSerializer.Deserialize<CharacterSession>(jsonData);
                    
                    if (sessionData != null)
                    {
                        var character = new AuthenticatedCharacter
                        {
                            CharacterId = sessionData.CharacterId,
                            CharacterName = sessionData.CharacterName,
                            CorporationId = sessionData.CorporationId,
                            AllianceId = sessionData.AllianceId,
                            TokenExpiry = sessionData.TokenExpiry,
                            AccessToken = "", // Will be loaded from secure storage
                            RefreshToken = "" // Will be loaded from secure storage
                        };
                        
                        sessions.Add(character);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading session file {SessionFile}", sessionFile);
                }
            }
            
            _logger.LogDebug("Loaded {SessionCount} persisted sessions", sessions.Count);
            return sessions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading all sessions");
            return Enumerable.Empty<AuthenticatedCharacter>();
        }
    }

    /// <summary>
    /// Set active character
    /// </summary>
    public async Task SetActiveCharacterAsync(long characterId, CancellationToken cancellationToken = default)
    {
        try
        {
            var activeCharacterPath = Path.Combine(_sessionDirectory, "active_character.json");
            var activeData = new { CharacterId = characterId, SetTime = DateTime.UtcNow };
            var jsonData = JsonSerializer.Serialize(activeData);
            
            await File.WriteAllTextAsync(activeCharacterPath, jsonData, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting active character {CharacterId}", characterId);
        }
    }

    /// <summary>
    /// Save character group
    /// </summary>
    public async Task SaveCharacterGroupAsync(CharacterGroup group, CancellationToken cancellationToken = default)
    {
        try
        {
            var groupPath = Path.Combine(_sessionDirectory, $"group_{group.GroupId}.json");
            var jsonData = JsonSerializer.Serialize(group, new JsonSerializerOptions { WriteIndented = true });
            
            await File.WriteAllTextAsync(groupPath, jsonData, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving character group {GroupId}", group.GroupId);
        }
    }

    /// <summary>
    /// Load character groups
    /// </summary>
    public async Task<IEnumerable<CharacterGroup>> LoadCharacterGroupsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var groups = new List<CharacterGroup>();
            var groupFiles = Directory.GetFiles(_sessionDirectory, "group_*.json");
            
            foreach (var groupFile in groupFiles)
            {
                try
                {
                    var jsonData = await File.ReadAllTextAsync(groupFile, cancellationToken);
                    var group = JsonSerializer.Deserialize<CharacterGroup>(jsonData);
                    
                    if (group != null)
                    {
                        groups.Add(group);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading group file {GroupFile}", groupFile);
                }
            }
            
            return groups;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading character groups");
            return Enumerable.Empty<CharacterGroup>();
        }
    }

    /// <summary>
    /// Get session start time
    /// </summary>
    public async Task<DateTime> GetSessionStartTimeAsync()
    {
        try
        {
            var startTimePath = Path.Combine(_sessionDirectory, "session_start.json");
            if (File.Exists(startTimePath))
            {
                var jsonData = await File.ReadAllTextAsync(startTimePath);
                var data = JsonSerializer.Deserialize<Dictionary<string, DateTime>>(jsonData);
                return data?["StartTime"] ?? DateTime.UtcNow;
            }
            
            // Create new session start time
            var startTime = DateTime.UtcNow;
            var startData = new Dictionary<string, DateTime> { ["StartTime"] = startTime };
            var startJson = JsonSerializer.Serialize(startData);
            await File.WriteAllTextAsync(startTimePath, startJson);
            
            return startTime;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session start time");
            return DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Clean up expired sessions
    /// </summary>
    public async Task CleanupExpiredSessionsAsync(TimeSpan maxAge, CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffTime = DateTime.UtcNow - maxAge;
            var sessionFiles = Directory.GetFiles(_sessionDirectory, "session_*.json");
            var deletedCount = 0;
            
            foreach (var sessionFile in sessionFiles)
            {
                try
                {
                    var lastWrite = File.GetLastWriteTime(sessionFile);
                    if (lastWrite < cutoffTime)
                    {
                        File.Delete(sessionFile);
                        deletedCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error cleaning up session file {SessionFile}", sessionFile);
                }
            }
            
            if (deletedCount > 0)
            {
                _logger.LogInformation("Cleaned up {DeletedCount} expired session files", deletedCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during session cleanup");
        }
    }

    #region Helper Methods

    private void EnsureSessionDirectoryExists()
    {
        if (!Directory.Exists(_sessionDirectory))
        {
            Directory.CreateDirectory(_sessionDirectory);
            _logger.LogInformation("Created session directory: {SessionDirectory}", _sessionDirectory);
        }
    }

    private string GetSessionFilePath(long characterId)
    {
        return Path.Combine(_sessionDirectory, $"session_{characterId}.json");
    }

    #endregion
}

#endregion

#region Supporting Data Structures

/// <summary>
/// Character session data for persistence
/// </summary>
public class CharacterSession
{
    public long CharacterId { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public long? CorporationId { get; set; }
    public long? AllianceId { get; set; }
    public DateTime TokenExpiry { get; set; }
    public DateTime LastAccessed { get; set; }
    public string SessionId { get; set; } = string.Empty;
}

/// <summary>
/// Character group for organization
/// </summary>
public class CharacterGroup
{
    public string GroupId { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public List<long> CharacterIds { get; set; } = new();
    public DateTime CreatedDate { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Bulk operation result
/// </summary>
public class BulkOperationResult
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Failures { get; set; } = new();
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// Multi-character session statistics
/// </summary>
public class MultiCharacterSessionStats
{
    public int TotalCharacters { get; set; }
    public int ValidTokens { get; set; }
    public int ExpiredTokens { get; set; }
    public int CharacterGroups { get; set; }
    public TimeSpan SessionUptime { get; set; }
    public DateTime LastHealthCheck { get; set; }
    public double TokenValidityPercentage => TotalCharacters > 0 ? (double)ValidTokens / TotalCharacters * 100 : 0;
}

/// <summary>
/// Multi-character service interface
/// </summary>
public interface IMultiCharacterService : IDisposable
{
    Task<IEnumerable<AuthenticatedCharacter>> GetAllCharactersAsync(CancellationToken cancellationToken = default);
    Task<bool> AddCharacterAsync(long characterId, CancellationToken cancellationToken = default);
    Task<bool> RemoveCharacterAsync(long characterId, CancellationToken cancellationToken = default);
    Task<bool> SwitchActiveCharacterAsync(long characterId, CancellationToken cancellationToken = default);
    Task<string> CreateCharacterGroupAsync(string groupName, IEnumerable<long> characterIds, CancellationToken cancellationToken = default);
    Task<IEnumerable<CharacterGroup>> GetCharacterGroupsAsync(CancellationToken cancellationToken = default);
    Task<BulkOperationResult> RefreshAllTokensAsync(CancellationToken cancellationToken = default);
    Task<MultiCharacterSessionStats> GetSessionStatsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Session persistence service interface
/// </summary>
public interface ISessionPersistenceService
{
    Task SaveCharacterSessionAsync(AuthenticatedCharacter character, CancellationToken cancellationToken = default);
    Task RemoveCharacterSessionAsync(long characterId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuthenticatedCharacter>> LoadAllSessionsAsync(CancellationToken cancellationToken = default);
    Task SetActiveCharacterAsync(long characterId, CancellationToken cancellationToken = default);
    Task SaveCharacterGroupAsync(CharacterGroup group, CancellationToken cancellationToken = default);
    Task<IEnumerable<CharacterGroup>> LoadCharacterGroupsAsync(CancellationToken cancellationToken = default);
    Task<DateTime> GetSessionStartTimeAsync();
    Task CleanupExpiredSessionsAsync(TimeSpan maxAge, CancellationToken cancellationToken = default);
}

#endregion