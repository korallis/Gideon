// ==========================================================================
// EsiAuthenticationServices.cs - ESI API Authentication Service Implementations
// ==========================================================================
// OAuth2 PKCE authentication services for EVE Online ESI API integration.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.Extensions.Logging;
using Gideon.WPF.Core.Application.Services;
using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Core.Domain.Interfaces;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using System.Net.Http;
using System.Web;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace Gideon.WPF.Core.Infrastructure.Services;

#region OAuth2 PKCE Authentication Services

/// <summary>
/// OAuth2 PKCE authentication service for EVE Online ESI API
/// </summary>
public class OAuth2Service : IOAuth2Service
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OAuth2Service> _logger;
    private readonly IAuditLogService _auditService;
    
    // EVE Online SSO Configuration
    private const string EveLoginUrl = "https://login.eveonline.com";
    private const string AuthorizeEndpoint = "/v2/oauth/authorize";
    private const string TokenEndpoint = "/v2/oauth/token";
    private const string ClientId = "gideon-eve-copilot"; // Would be configured
    private const string RedirectUri = "http://localhost:8080/callback";
    
    private readonly Dictionary<string, PkceChallenge> _activeChallenges = new();

    public OAuth2Service(HttpClient httpClient, ILogger<OAuth2Service> logger, IAuditLogService auditService)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        
        _httpClient.BaseAddress = new Uri(EveLoginUrl);
    }

    /// <summary>
    /// Generate OAuth2 authorization URL with PKCE challenge
    /// </summary>
    public async Task<string> GetAuthorizationUrlAsync(string[] scopes, CancellationToken cancellationToken = default)
    {
        try
        {
            var pkceChallenge = GeneratePkceChallenge();
            var state = GenerateSecureRandomString(32);
            
            // Store challenge for later verification
            _activeChallenges[state] = pkceChallenge;
            
            var scopeString = string.Join(" ", scopes);
            
            var queryParams = new Dictionary<string, string>
            {
                ["response_type"] = "code",
                ["redirect_uri"] = RedirectUri,
                ["client_id"] = ClientId,
                ["scope"] = scopeString,
                ["code_challenge"] = pkceChallenge.CodeChallenge,
                ["code_challenge_method"] = "S256",
                ["state"] = state
            };

            var queryString = string.Join("&", 
                queryParams.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            
            var authUrl = $"{EveLoginUrl}{AuthorizeEndpoint}?{queryString}";
            
            _logger.LogInformation("Generated OAuth2 authorization URL for scopes: {Scopes}", scopeString);
            await _auditService.LogActionAsync("oauth2_auth_url_generated", "Authentication", null, cancellationToken);
            
            return authUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating OAuth2 authorization URL");
            throw;
        }
    }

    /// <summary>
    /// Exchange authorization code for access token
    /// </summary>
    public async Task<string> ExchangeCodeForTokenAsync(string authorizationCode, CancellationToken cancellationToken = default)
    {
        try
        {
            // Extract state from callback (would be passed from callback handler)
            var state = ExtractStateFromContext(); // Implementation depends on callback handling
            
            if (!_activeChallenges.TryGetValue(state, out var pkceChallenge))
            {
                throw new InvalidOperationException("Invalid or expired PKCE challenge");
            }

            var tokenRequest = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["client_id"] = ClientId,
                ["code"] = authorizationCode,
                ["redirect_uri"] = RedirectUri,
                ["code_verifier"] = pkceChallenge.CodeVerifier
            };

            var requestContent = new FormUrlEncodedContent(tokenRequest);
            var response = await _httpClient.PostAsync(TokenEndpoint, requestContent, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Token exchange failed: {StatusCode} - {Error}", 
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"Token exchange failed: {response.StatusCode}");
            }

            var tokenResponseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(tokenResponseJson);
            
            if (tokenResponse?.AccessToken == null)
            {
                throw new InvalidOperationException("Invalid token response");
            }

            // Clean up used challenge
            _activeChallenges.Remove(state);
            
            _logger.LogInformation("Successfully exchanged authorization code for access token");
            await _auditService.LogActionAsync("oauth2_token_exchange", "Authentication", null, cancellationToken);
            
            return tokenResponse.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging authorization code for token");
            throw;
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    public async Task<string> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var refreshRequest = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = ClientId,
                ["refresh_token"] = refreshToken
            };

            var requestContent = new FormUrlEncodedContent(refreshRequest);
            var response = await _httpClient.PostAsync(TokenEndpoint, requestContent, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Token refresh failed: {StatusCode} - {Error}", 
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"Token refresh failed: {response.StatusCode}");
            }

            var tokenResponseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(tokenResponseJson);
            
            if (tokenResponse?.AccessToken == null)
            {
                throw new InvalidOperationException("Invalid refresh token response");
            }

            _logger.LogDebug("Successfully refreshed access token");
            await _auditService.LogActionAsync("oauth2_token_refresh", "Authentication", null, cancellationToken);
            
            return tokenResponse.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing access token");
            throw;
        }
    }

    /// <summary>
    /// Validate access token by making a test API call
    /// </summary>
    public async Task<bool> ValidateTokenAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://esi.evetech.net/latest/verify/");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating access token");
            return false;
        }
    }

    #region Helper Methods

    private static PkceChallenge GeneratePkceChallenge()
    {
        var codeVerifier = GenerateSecureRandomString(128);
        var codeChallenge = GenerateCodeChallenge(codeVerifier);
        
        return new PkceChallenge
        {
            CodeVerifier = codeVerifier,
            CodeChallenge = codeChallenge
        };
    }

    private static string GenerateSecureRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~";
        var result = new StringBuilder(length);
        
        using var rng = RandomNumberGenerator.Create();
        var buffer = new byte[4];
        
        for (int i = 0; i < length; i++)
        {
            rng.GetBytes(buffer);
            var randomIndex = BitConverter.ToUInt32(buffer, 0) % chars.Length;
            result.Append(chars[(int)randomIndex]);
        }
        
        return result.ToString();
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
        return Convert.ToBase64String(hash)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string ExtractStateFromContext()
    {
        // This would be implemented to extract state from the OAuth callback
        // For now, return a placeholder
        return "placeholder_state";
    }

    #endregion
}

/// <summary>
/// Windows Credential Manager token storage service
/// </summary>
public class TokenStorageService : ITokenStorageService
{
    private readonly ILogger<TokenStorageService> _logger;
    private readonly IAuditLogService _auditService;
    private const string TargetPrefix = "Gideon_EVE_";

    public TokenStorageService(ILogger<TokenStorageService> logger, IAuditLogService auditService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    /// <summary>
    /// Store tokens securely in Windows Credential Manager
    /// </summary>
    public async Task StoreTokenAsync(long characterId, string accessToken, string refreshToken, DateTime expiry, CancellationToken cancellationToken = default)
    {
        try
        {
            var credentialData = new CredentialData
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiryDate = expiry,
                StoredDate = DateTime.UtcNow
            };

            var credentialJson = JsonSerializer.Serialize(credentialData);
            var targetName = $"{TargetPrefix}{characterId}";

            // Store in Windows Credential Manager
            await StoreWindowsCredentialAsync(targetName, credentialJson, cancellationToken);

            _logger.LogInformation("Stored tokens for character {CharacterId}", characterId);
            await _auditService.LogActionAsync("token_stored", "Authentication", characterId.ToString(), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing tokens for character {CharacterId}", characterId);
            throw;
        }
    }

    /// <summary>
    /// Retrieve tokens from Windows Credential Manager
    /// </summary>
    public async Task<(string accessToken, string refreshToken, DateTime expiry)?> GetTokenAsync(long characterId, CancellationToken cancellationToken = default)
    {
        try
        {
            var targetName = $"{TargetPrefix}{characterId}";
            var credentialJson = await GetWindowsCredentialAsync(targetName, cancellationToken);

            if (string.IsNullOrEmpty(credentialJson))
            {
                _logger.LogDebug("No stored tokens found for character {CharacterId}", characterId);
                return null;
            }

            var credentialData = JsonSerializer.Deserialize<CredentialData>(credentialJson);
            if (credentialData == null)
            {
                _logger.LogWarning("Invalid credential data for character {CharacterId}", characterId);
                return null;
            }

            _logger.LogDebug("Retrieved tokens for character {CharacterId}", characterId);
            return (credentialData.AccessToken, credentialData.RefreshToken, credentialData.ExpiryDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tokens for character {CharacterId}", characterId);
            return null;
        }
    }

    /// <summary>
    /// Remove tokens from Windows Credential Manager
    /// </summary>
    public async Task RemoveTokenAsync(long characterId, CancellationToken cancellationToken = default)
    {
        try
        {
            var targetName = $"{TargetPrefix}{characterId}";
            await RemoveWindowsCredentialAsync(targetName, cancellationToken);

            _logger.LogInformation("Removed tokens for character {CharacterId}", characterId);
            await _auditService.LogActionAsync("token_removed", "Authentication", characterId.ToString(), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing tokens for character {CharacterId}", characterId);
            throw;
        }
    }

    #region Windows Credential Manager Integration

    private async Task StoreWindowsCredentialAsync(string targetName, string credentialData, CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            try
            {
                // This would use Windows Credential Manager APIs
                // For now, using a file-based approach as fallback
                var credentialPath = GetCredentialFilePath(targetName);
                var encryptedData = ProtectedData.Protect(
                    Encoding.UTF8.GetBytes(credentialData),
                    null,
                    DataProtectionScope.CurrentUser);
                
                Directory.CreateDirectory(Path.GetDirectoryName(credentialPath)!);
                File.WriteAllBytes(credentialPath, encryptedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing Windows credential for {TargetName}", targetName);
                throw;
            }
        }, cancellationToken);
    }

    private async Task<string?> GetWindowsCredentialAsync(string targetName, CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            try
            {
                var credentialPath = GetCredentialFilePath(targetName);
                if (!File.Exists(credentialPath))
                    return null;

                var encryptedData = File.ReadAllBytes(credentialPath);
                var decryptedData = ProtectedData.Unprotect(
                    encryptedData,
                    null,
                    DataProtectionScope.CurrentUser);
                
                return Encoding.UTF8.GetString(decryptedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Windows credential for {TargetName}", targetName);
                return null;
            }
        }, cancellationToken);
    }

    private async Task RemoveWindowsCredentialAsync(string targetName, CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            try
            {
                var credentialPath = GetCredentialFilePath(targetName);
                if (File.Exists(credentialPath))
                {
                    File.Delete(credentialPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing Windows credential for {TargetName}", targetName);
                throw;
            }
        }, cancellationToken);
    }

    private static string GetCredentialFilePath(string targetName)
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var credentialDir = Path.Combine(appDataPath, "Gideon", "Credentials");
        return Path.Combine(credentialDir, $"{targetName}.dat");
    }

    #endregion
}

/// <summary>
/// Authentication service for managing EVE character authentication
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IOAuth2Service _oauth2Service;
    private readonly ITokenStorageService _tokenStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IAuditLogService _auditService;

    private readonly Dictionary<long, AuthenticatedCharacter> _authenticatedCharacters = new();
    private readonly Timer _tokenRefreshTimer;

    public AuthenticationService(
        IOAuth2Service oauth2Service,
        ITokenStorageService tokenStorage,
        IUnitOfWork unitOfWork,
        ILogger<AuthenticationService> logger,
        IAuditLogService auditService)
    {
        _oauth2Service = oauth2Service ?? throw new ArgumentNullException(nameof(oauth2Service));
        _tokenStorage = tokenStorage ?? throw new ArgumentNullException(nameof(tokenStorage));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));

        // Set up automatic token refresh timer (every 30 minutes)
        _tokenRefreshTimer = new Timer(RefreshExpiredTokens, null, TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));
    }

    /// <summary>
    /// Initiate character authentication flow
    /// </summary>
    public async Task<string> InitiateAuthenticationAsync(string[] scopes, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Initiating authentication for scopes: {Scopes}", string.Join(", ", scopes));
            
            var authUrl = await _oauth2Service.GetAuthorizationUrlAsync(scopes, cancellationToken);
            
            // Launch browser to authentication URL
            await LaunchBrowserAsync(authUrl, cancellationToken);
            
            return authUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating authentication");
            throw;
        }
    }

    /// <summary>
    /// Complete authentication after OAuth callback
    /// </summary>
    public async Task<bool> CompleteAuthenticationAsync(string authorizationCode, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Completing authentication with authorization code");
            
            // Exchange code for tokens
            var accessToken = await _oauth2Service.ExchangeCodeForTokenAsync(authorizationCode, cancellationToken);
            
            // Get character information from token
            var characterInfo = await GetCharacterFromTokenAsync(accessToken, cancellationToken);
            if (characterInfo == null)
            {
                _logger.LogError("Failed to retrieve character information from token");
                return false;
            }

            // Store character and tokens
            await StoreAuthenticatedCharacterAsync(characterInfo, accessToken, cancellationToken);
            
            _logger.LogInformation("Successfully authenticated character {CharacterName} ({CharacterId})", 
                characterInfo.CharacterName, characterInfo.CharacterId);
            
            await _auditService.LogActionAsync("character_authenticated", "Character", 
                characterInfo.CharacterId.ToString(), cancellationToken);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing authentication");
            return false;
        }
    }

    /// <summary>
    /// Get authenticated character by ID
    /// </summary>
    public async Task<AuthenticatedCharacter?> GetAuthenticatedCharacterAsync(long characterId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check in-memory cache first
            if (_authenticatedCharacters.TryGetValue(characterId, out var cachedCharacter))
            {
                // Verify token is still valid
                if (cachedCharacter.TokenExpiry > DateTime.UtcNow.AddMinutes(5))
                {
                    return cachedCharacter;
                }
            }

            // Try to load from storage
            var tokenData = await _tokenStorage.GetTokenAsync(characterId, cancellationToken);
            if (tokenData == null)
            {
                return null;
            }

            var character = await _unitOfWork.Characters.FindFirstAsync(
                c => c.CharacterId == characterId, cancellationToken);
            
            if (character == null)
            {
                return null;
            }

            var authenticatedCharacter = new AuthenticatedCharacter
            {
                CharacterId = characterId,
                CharacterName = character.CharacterName,
                AccessToken = tokenData.Value.accessToken,
                RefreshToken = tokenData.Value.refreshToken,
                TokenExpiry = tokenData.Value.expiry,
                CorporationId = character.CorporationId,
                AllianceId = character.AllianceId
            };

            // Cache the character
            _authenticatedCharacters[characterId] = authenticatedCharacter;
            
            return authenticatedCharacter;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving authenticated character {CharacterId}", characterId);
            return null;
        }
    }

    /// <summary>
    /// Get all authenticated characters
    /// </summary>
    public async Task<IEnumerable<AuthenticatedCharacter>> GetAllAuthenticatedCharactersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var characters = await _unitOfWork.Characters.GetAllAsync(cancellationToken);
            var authenticatedCharacters = new List<AuthenticatedCharacter>();

            foreach (var character in characters)
            {
                var authChar = await GetAuthenticatedCharacterAsync(character.CharacterId, cancellationToken);
                if (authChar != null)
                {
                    authenticatedCharacters.Add(authChar);
                }
            }

            return authenticatedCharacters;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all authenticated characters");
            return Enumerable.Empty<AuthenticatedCharacter>();
        }
    }

    /// <summary>
    /// Refresh access token for a character
    /// </summary>
    public async Task<bool> RefreshCharacterTokenAsync(long characterId, CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenData = await _tokenStorage.GetTokenAsync(characterId, cancellationToken);
            if (tokenData == null)
            {
                _logger.LogWarning("No stored tokens found for character {CharacterId}", characterId);
                return false;
            }

            var newAccessToken = await _oauth2Service.RefreshTokenAsync(tokenData.Value.refreshToken, cancellationToken);
            
            // Store updated tokens (refresh token may also be updated)
            await _tokenStorage.StoreTokenAsync(characterId, newAccessToken, tokenData.Value.refreshToken, 
                DateTime.UtcNow.AddHours(1), cancellationToken);

            // Update cached character
            if (_authenticatedCharacters.TryGetValue(characterId, out var cachedCharacter))
            {
                cachedCharacter.AccessToken = newAccessToken;
                cachedCharacter.TokenExpiry = DateTime.UtcNow.AddHours(1);
            }

            _logger.LogDebug("Successfully refreshed token for character {CharacterId}", characterId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token for character {CharacterId}", characterId);
            return false;
        }
    }

    /// <summary>
    /// Remove character authentication
    /// </summary>
    public async Task RemoveCharacterAsync(long characterId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _tokenStorage.RemoveTokenAsync(characterId, cancellationToken);
            _authenticatedCharacters.Remove(characterId);
            
            _logger.LogInformation("Removed authentication for character {CharacterId}", characterId);
            await _auditService.LogActionAsync("character_deauthenticated", "Character", 
                characterId.ToString(), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing character authentication {CharacterId}", characterId);
            throw;
        }
    }

    #region Helper Methods

    private async Task LaunchBrowserAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            
            Process.Start(processStartInfo);
            _logger.LogDebug("Launched browser for authentication URL");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error launching browser for authentication");
            throw;
        }
    }

    private async Task<Character?> GetCharacterFromTokenAsync(string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            // This would make an API call to get character info from the token
            // For now, return a placeholder
            return new Character
            {
                CharacterId = 123456789, // Would come from API
                CharacterName = "Test Character", // Would come from API
                CorporationId = 1000001, // Would come from API
                AllianceId = null,
                SecurityStatus = 0.0,
                TotalSp = 5000000,
                LastLoginDate = DateTime.UtcNow,
                IsActive = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting character from token");
            return null;
        }
    }

    private async Task StoreAuthenticatedCharacterAsync(Character character, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            // Store character in database
            var existingCharacter = await _unitOfWork.Characters.FindFirstAsync(
                c => c.CharacterId == character.CharacterId, cancellationToken);

            if (existingCharacter == null)
            {
                await _unitOfWork.Characters.AddAsync(character, cancellationToken);
            }
            else
            {
                existingCharacter.CharacterName = character.CharacterName;
                existingCharacter.CorporationId = character.CorporationId;
                existingCharacter.AllianceId = character.AllianceId;
                existingCharacter.LastLoginDate = DateTime.UtcNow;
                await _unitOfWork.Characters.UpdateAsync(existingCharacter, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Store tokens
            var tokenExpiry = DateTime.UtcNow.AddHours(1); // EVE tokens typically expire in 1 hour
            await _tokenStorage.StoreTokenAsync(character.CharacterId, accessToken, "refresh_token_placeholder", 
                tokenExpiry, cancellationToken);

            // Cache authenticated character
            _authenticatedCharacters[character.CharacterId] = new AuthenticatedCharacter
            {
                CharacterId = character.CharacterId,
                CharacterName = character.CharacterName,
                AccessToken = accessToken,
                RefreshToken = "refresh_token_placeholder",
                TokenExpiry = tokenExpiry,
                CorporationId = character.CorporationId,
                AllianceId = character.AllianceId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing authenticated character");
            throw;
        }
    }

    private async void RefreshExpiredTokens(object? state)
    {
        try
        {
            var charactersToRefresh = _authenticatedCharacters.Values
                .Where(c => c.TokenExpiry <= DateTime.UtcNow.AddMinutes(10))
                .ToList();

            foreach (var character in charactersToRefresh)
            {
                try
                {
                    await RefreshCharacterTokenAsync(character.CharacterId, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error refreshing token for character {CharacterId} in background", 
                        character.CharacterId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in background token refresh");
        }
    }

    public void Dispose()
    {
        _tokenRefreshTimer?.Dispose();
    }

    #endregion
}

#endregion

#region Supporting Data Structures

/// <summary>
/// PKCE challenge data
/// </summary>
public class PkceChallenge
{
    public string CodeVerifier { get; set; } = string.Empty;
    public string CodeChallenge { get; set; } = string.Empty;
}

/// <summary>
/// OAuth2 token response
/// </summary>
public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}

/// <summary>
/// Credential data for secure storage
/// </summary>
public class CredentialData
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public DateTime StoredDate { get; set; }
}

/// <summary>
/// Authenticated character information
/// </summary>
public class AuthenticatedCharacter
{
    public long CharacterId { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime TokenExpiry { get; set; }
    public long? CorporationId { get; set; }
    public long? AllianceId { get; set; }
    public bool IsTokenValid => TokenExpiry > DateTime.UtcNow.AddMinutes(5);
}

#endregion