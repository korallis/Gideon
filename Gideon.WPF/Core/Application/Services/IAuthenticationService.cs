using Gideon.WPF.Core.Domain.Entities;

namespace Gideon.WPF.Core.Application.Services;

/// <summary>
/// Authentication service interface for EVE Online ESI OAuth2 authentication
/// </summary>
public interface IAuthenticationService
{
    // Authentication state
    bool IsAuthenticated { get; }
    
    Character? CurrentCharacter { get; }
    
    IReadOnlyList<Character> AuthenticatedCharacters { get; }
    
    // Events
    event EventHandler<Character>? CharacterAuthenticated;
    
    event EventHandler<Character>? CharacterLoggedOut;
    
    event EventHandler<Character>? ActiveCharacterChanged;
    
    event EventHandler<string>? AuthenticationError;
    
    // Authentication flow
    Task<bool> StartAuthenticationAsync(CancellationToken cancellationToken = default);
    
    Task<bool> CompleteAuthenticationAsync(string authorizationCode, string state, CancellationToken cancellationToken = default);
    
    Task<bool> RefreshTokenAsync(int? characterId = null, CancellationToken cancellationToken = default);
    
    Task LogoutAsync(int? characterId = null, CancellationToken cancellationToken = default);
    
    Task LogoutAllAsync(CancellationToken cancellationToken = default);
    
    // Character management
    Task<bool> SwitchActiveCharacterAsync(int characterId, CancellationToken cancellationToken = default);
    
    Task<Character?> GetCharacterAsync(int characterId, CancellationToken cancellationToken = default);
    
    Task<bool> UpdateCharacterAsync(Character character, CancellationToken cancellationToken = default);
    
    // Token management
    Task<string?> GetValidAccessTokenAsync(int? characterId = null, CancellationToken cancellationToken = default);
    
    Task<bool> IsTokenValidAsync(int? characterId = null, CancellationToken cancellationToken = default);
    
    Task<TimeSpan> GetTokenExpiryTimeAsync(int? characterId = null, CancellationToken cancellationToken = default);
    
    // Scope management
    Task<IReadOnlyList<string>> GetAuthorizedScopesAsync(int? characterId = null, CancellationToken cancellationToken = default);
    
    Task<bool> HasScopeAsync(string scope, int? characterId = null, CancellationToken cancellationToken = default);
    
    // Configuration
    Task<bool> UpdateConfigurationAsync(string clientId, string redirectUri, IEnumerable<string> scopes, CancellationToken cancellationToken = default);
    
    // Initialization and cleanup
    Task InitializeAsync(CancellationToken cancellationToken = default);
    
    Task ShutdownAsync(CancellationToken cancellationToken = default);
}