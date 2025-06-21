using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Gideon.WPF.Core.Application.Services;
using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Core.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Gideon.WPF.Infrastructure.Services;

/// <summary>
/// EVE Online OAuth2 PKCE authentication service implementation
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly HttpClient _httpClient;

    // EVE Online ESI OAuth2 endpoints
    private const string ESI_BASE_URL = "https://esi.evetech.net/";
    private const string LOGIN_BASE_URL = "https://login.eveonline.com/";
    private const string AUTHORIZE_URL = LOGIN_BASE_URL + "v2/oauth/authorize";
    private const string TOKEN_URL = LOGIN_BASE_URL + "v2/oauth/token";
    private const string VERIFY_URL = LOGIN_BASE_URL + "oauth/verify";
    private const string REVOKE_URL = LOGIN_BASE_URL + "v2/oauth/revoke";

    private Character? _currentCharacter;
    private readonly Dictionary<int, Character> _authenticatedCharacters = new();
    private string? _clientId;
    private string? _codeVerifier;
    private string? _state;

    public AuthenticationService(
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<AuthenticationService> logger,
        HttpClient httpClient)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
    }

    // Properties
    public Character? CurrentCharacter => _currentCharacter;
    
    public bool IsAuthenticated => _currentCharacter != null && 
                                   !string.IsNullOrEmpty(_currentCharacter.AccessToken) && 
                                   _currentCharacter.TokenExpiresAt > DateTime.UtcNow;

    public IEnumerable<Character> AuthenticatedCharacters => _authenticatedCharacters.Values;

    // Events
    public event EventHandler<Character>? ActiveCharacterChanged;
    public event EventHandler<Character>? CharacterAuthenticated;
    public event EventHandler<Character>? CharacterLoggedOut;
    public event EventHandler<string>? AuthenticationError;

    // Initialization
    public async Task InitializeAsync()
    {
        try
        {
            _clientId = _configuration["EVE:ClientId"];
            if (string.IsNullOrEmpty(_clientId))
            {
                _logger.LogWarning("EVE Client ID not configured");
            }

            // Load stored authenticated characters
            var characters = await _unitOfWork.Characters.FindAsync(c => c.IsAuthenticated);
            foreach (var character in characters)
            {
                // Validate stored tokens
                if (await ValidateTokenAsync(character))
                {
                    _authenticatedCharacters[character.CharacterId] = character;
                    
                    // Set the first valid character as current
                    if (_currentCharacter == null)
                    {
                        _currentCharacter = character;
                    }
                }
                else
                {
                    // Token expired or invalid, mark as not authenticated
                    character.IsAuthenticated = false;
                    character.AccessToken = null;
                    character.RefreshToken = null;
                    await _unitOfWork.Characters.UpdateAsync(character);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation($"Initialized authentication service with {_authenticatedCharacters.Count} authenticated characters");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize authentication service");
            AuthenticationError?.Invoke(this, "Failed to initialize authentication service");
        }
    }

    // Authentication flow
    public async Task<bool> StartAuthenticationAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(_clientId))
            {
                AuthenticationError?.Invoke(this, "EVE Client ID not configured");
                return false;
            }

            // Generate PKCE parameters
            _codeVerifier = GenerateCodeVerifier();
            var codeChallenge = GenerateCodeChallenge(_codeVerifier);
            _state = GenerateState();

            // Build authorization URL
            var scopes = "esi-characters.read_blueprints.v1 esi-characters.read_contacts.v1 esi-characters.read_fittings.v1 esi-characters.read_skills.v1 esi-characters.read_standings.v1 esi-fleets.read_fleet.v1 esi-fleets.write_fleet.v1 esi-skills.read_skills.v1 esi-skills.read_skillqueue.v1 esi-wallet.read_character_wallet.v1";
            
            var authUrl = $"{AUTHORIZE_URL}?" +
                         $"response_type=code&" +
                         $"redirect_uri=http://localhost:8080/callback&" +
                         $"client_id={_clientId}&" +
                         $"scope={Uri.EscapeDataString(scopes)}&" +
                         $"code_challenge={codeChallenge}&" +
                         $"code_challenge_method=S256&" +
                         $"state={_state}";

            // Open browser for authentication
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = authUrl,
                UseShellExecute = true
            });

            _logger.LogInformation("Started EVE Online authentication flow");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start authentication");
            AuthenticationError?.Invoke(this, "Failed to start authentication");
            return false;
        }
    }

    public async Task<bool> CompleteAuthenticationAsync(string authorizationCode, string state)
    {
        try
        {
            if (state != _state)
            {
                AuthenticationError?.Invoke(this, "Invalid state parameter");
                return false;
            }

            if (string.IsNullOrEmpty(_codeVerifier))
            {
                AuthenticationError?.Invoke(this, "Code verifier not found");
                return false;
            }

            // Exchange authorization code for tokens
            var tokenResponse = await ExchangeCodeForTokensAsync(authorizationCode, _codeVerifier);
            if (tokenResponse == null)
            {
                return false;
            }

            // Get character information
            var characterInfo = await GetCharacterInfoAsync(tokenResponse.AccessToken);
            if (characterInfo == null)
            {
                return false;
            }

            // Create or update character
            var character = await GetOrCreateCharacterAsync(characterInfo);
            
            // Update authentication info
            character.AccessToken = tokenResponse.AccessToken;
            character.RefreshToken = tokenResponse.RefreshToken;
            character.TokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
            character.AuthenticatedScopes = tokenResponse.Scope;
            character.IsAuthenticated = true;
            character.LastLoginAt = DateTime.UtcNow;

            await _unitOfWork.Characters.UpdateAsync(character);
            await _unitOfWork.SaveChangesAsync();

            // Add to authenticated characters and set as current
            _authenticatedCharacters[character.CharacterId] = character;
            _currentCharacter = character;

            _logger.LogInformation($"Successfully authenticated character: {character.Name}");
            CharacterAuthenticated?.Invoke(this, character);
            ActiveCharacterChanged?.Invoke(this, character);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete authentication");
            AuthenticationError?.Invoke(this, "Failed to complete authentication");
            return false;
        }
    }

    // Character management
    public async Task<bool> SwitchActiveCharacterAsync(int characterId)
    {
        try
        {
            if (!_authenticatedCharacters.TryGetValue(characterId, out var character))
            {
                AuthenticationError?.Invoke(this, "Character not authenticated");
                return false;
            }

            // Validate token before switching
            if (!await ValidateTokenAsync(character))
            {
                AuthenticationError?.Invoke(this, "Character token expired");
                return false;
            }

            _currentCharacter = character;
            ActiveCharacterChanged?.Invoke(this, character);
            
            _logger.LogInformation($"Switched to character: {character.Name}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to switch character");
            AuthenticationError?.Invoke(this, "Failed to switch character");
            return false;
        }
    }

    // Token management
    public async Task<bool> RefreshTokenAsync(int characterId)
    {
        try
        {
            if (!_authenticatedCharacters.TryGetValue(characterId, out var character))
            {
                return false;
            }

            if (string.IsNullOrEmpty(character.RefreshToken))
            {
                return false;
            }

            var tokenResponse = await RefreshAccessTokenAsync(character.RefreshToken);
            if (tokenResponse == null)
            {
                return false;
            }

            // Update token information
            character.AccessToken = tokenResponse.AccessToken;
            character.RefreshToken = tokenResponse.RefreshToken ?? character.RefreshToken;
            character.TokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

            await _unitOfWork.Characters.UpdateAsync(character);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"Refreshed token for character: {character.Name}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh token for character {CharacterId}", characterId);
            return false;
        }
    }

    // Logout
    public async Task<bool> LogoutAsync(int characterId)
    {
        try
        {
            if (!_authenticatedCharacters.TryGetValue(characterId, out var character))
            {
                return false;
            }

            // Revoke token
            if (!string.IsNullOrEmpty(character.RefreshToken))
            {
                await RevokeTokenAsync(character.RefreshToken);
            }

            // Update character
            character.IsAuthenticated = false;
            character.AccessToken = null;
            character.RefreshToken = null;
            character.TokenExpiresAt = null;
            character.AuthenticatedScopes = null;

            await _unitOfWork.Characters.UpdateAsync(character);
            await _unitOfWork.SaveChangesAsync();

            // Remove from authenticated characters
            _authenticatedCharacters.Remove(characterId);

            // Update current character
            if (_currentCharacter?.CharacterId == characterId)
            {
                _currentCharacter = _authenticatedCharacters.Values.FirstOrDefault();
                if (_currentCharacter != null)
                {
                    ActiveCharacterChanged?.Invoke(this, _currentCharacter);
                }
            }

            _logger.LogInformation($"Logged out character: {character.Name}");
            CharacterLoggedOut?.Invoke(this, character);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to logout character {CharacterId}", characterId);
            return false;
        }
    }

    public async Task<bool> LogoutAllAsync()
    {
        try
        {
            var charactersToLogout = _authenticatedCharacters.Values.ToList();
            foreach (var character in charactersToLogout)
            {
                await LogoutAsync(character.CharacterId);
            }

            _currentCharacter = null;
            _authenticatedCharacters.Clear();

            _logger.LogInformation("Logged out all characters");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to logout all characters");
            return false;
        }
    }

    // Helper methods
    private async Task<bool> ValidateTokenAsync(Character character)
    {
        if (string.IsNullOrEmpty(character.AccessToken))
            return false;

        if (character.TokenExpiresAt <= DateTime.UtcNow)
        {
            // Try to refresh the token
            return await RefreshTokenAsync(character.CharacterId);
        }

        return true;
    }

    private async Task<Character> GetOrCreateCharacterAsync(dynamic characterInfo)
    {
        var character = await _unitOfWork.Characters.GetByIdAsync(characterInfo.CharacterID);
        
        if (character == null)
        {
            character = new Character
            {
                CharacterId = characterInfo.CharacterID,
                Name = characterInfo.CharacterName,
                CorporationId = characterInfo.CorporationID,
                AllianceId = characterInfo.AllianceID,
                CreatedAt = DateTime.UtcNow
            };
            
            await _unitOfWork.Characters.AddAsync(character);
        }
        else
        {
            // Update character info
            character.Name = characterInfo.CharacterName;
            character.CorporationId = characterInfo.CorporationID;
            character.AllianceId = characterInfo.AllianceID;
            character.UpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork.Characters.UpdateAsync(character);
        }

        return character;
    }

    // PKCE helpers
    private static string GenerateCodeVerifier()
    {
        var bytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(codeVerifier);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }

    private static string GenerateState()
    {
        var bytes = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    // HTTP API methods (placeholder implementations)
    private async Task<dynamic?> ExchangeCodeForTokensAsync(string code, string codeVerifier)
    {
        // TODO: Implement token exchange
        await Task.Delay(100);
        return null;
    }

    private async Task<dynamic?> GetCharacterInfoAsync(string accessToken)
    {
        // TODO: Implement character info retrieval
        await Task.Delay(100);
        return null;
    }

    private async Task<dynamic?> RefreshAccessTokenAsync(string refreshToken)
    {
        // TODO: Implement token refresh
        await Task.Delay(100);
        return null;
    }

    private async Task<bool> RevokeTokenAsync(string token)
    {
        // TODO: Implement token revocation
        await Task.Delay(100);
        return true;
    }
}