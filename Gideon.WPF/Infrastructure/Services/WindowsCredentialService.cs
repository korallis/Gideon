using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Gideon.WPF.Infrastructure.Services;

/// <summary>
/// Service for secure storage and retrieval of credentials using Windows Credential Manager
/// </summary>
public class WindowsCredentialService
{
    private readonly ILogger<WindowsCredentialService> _logger;
    private const string TARGET_PREFIX = "Gideon_EVE_";

    public WindowsCredentialService(ILogger<WindowsCredentialService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Stores credentials securely in Windows Credential Manager
    /// </summary>
    /// <param name="characterId">EVE character ID</param>
    /// <param name="accessToken">Access token to store</param>
    /// <param name="refreshToken">Refresh token to store</param>
    /// <param name="expiresAt">Token expiration time</param>
    public async Task<bool> StoreCredentialsAsync(int characterId, string accessToken, string refreshToken, DateTime expiresAt)
    {
        try
        {
            var target = $"{TARGET_PREFIX}{characterId}";
            var credentialData = new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt.ToBinary()
            };

            var jsonData = System.Text.Json.JsonSerializer.Serialize(credentialData);
            var credentialBlob = Encoding.UTF8.GetBytes(jsonData);

            var credential = new CREDENTIAL
            {
                Type = CRED_TYPE.GENERIC,
                TargetName = target,
                CredentialBlobSize = credentialBlob.Length,
                CredentialBlob = Marshal.AllocHGlobal(credentialBlob.Length),
                Persist = CRED_PERSIST.LOCAL_MACHINE,
                UserName = $"EVE_Character_{characterId}",
                Comment = $"Gideon EVE Online credentials for character {characterId}"
            };

            try
            {
                Marshal.Copy(credentialBlob, 0, credential.CredentialBlob, credentialBlob.Length);

                var result = CredWrite(ref credential, 0);
                if (result)
                {
                    _logger.LogDebug("Successfully stored credentials for character {CharacterId}", characterId);
                    return true;
                }
                else
                {
                    var error = Marshal.GetLastWin32Error();
                    _logger.LogError("Failed to store credentials for character {CharacterId}. Win32 Error: {Error}", characterId, error);
                    return false;
                }
            }
            finally
            {
                if (credential.CredentialBlob != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(credential.CredentialBlob);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while storing credentials for character {CharacterId}", characterId);
            return false;
        }
    }

    /// <summary>
    /// Retrieves stored credentials from Windows Credential Manager
    /// </summary>
    /// <param name="characterId">EVE character ID</param>
    /// <returns>Stored credential data or null if not found</returns>
    public async Task<StoredCredentials?> RetrieveCredentialsAsync(int characterId)
    {
        try
        {
            var target = $"{TARGET_PREFIX}{characterId}";
            
            if (CredRead(target, CRED_TYPE.GENERIC, 0, out var credentialPtr))
            {
                try
                {
                    var credential = Marshal.PtrToStructure<CREDENTIAL>(credentialPtr);
                    var credentialBlob = new byte[credential.CredentialBlobSize];
                    Marshal.Copy(credential.CredentialBlob, credentialBlob, 0, credential.CredentialBlobSize);
                    
                    var jsonData = Encoding.UTF8.GetString(credentialBlob);
                    var credentialData = System.Text.Json.JsonSerializer.Deserialize<dynamic>(jsonData);

                    if (credentialData != null)
                    {
                        var accessToken = credentialData.GetProperty("AccessToken").GetString();
                        var refreshToken = credentialData.GetProperty("RefreshToken").GetString();
                        var expiresAtBinary = credentialData.GetProperty("ExpiresAt").GetInt64();
                        var expiresAt = DateTime.FromBinary(expiresAtBinary);

                        _logger.LogDebug("Successfully retrieved credentials for character {CharacterId}", characterId);
                        
                        return new StoredCredentials
                        {
                            AccessToken = accessToken ?? string.Empty,
                            RefreshToken = refreshToken ?? string.Empty,
                            ExpiresAt = expiresAt
                        };
                    }
                }
                finally
                {
                    CredFree(credentialPtr);
                }
            }
            else
            {
                var error = Marshal.GetLastWin32Error();
                if (error != 1168) // ERROR_NOT_FOUND
                {
                    _logger.LogWarning("Failed to retrieve credentials for character {CharacterId}. Win32 Error: {Error}", characterId, error);
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while retrieving credentials for character {CharacterId}", characterId);
            return null;
        }
    }

    /// <summary>
    /// Deletes stored credentials from Windows Credential Manager
    /// </summary>
    /// <param name="characterId">EVE character ID</param>
    public async Task<bool> DeleteCredentialsAsync(int characterId)
    {
        try
        {
            var target = $"{TARGET_PREFIX}{characterId}";
            
            var result = CredDelete(target, CRED_TYPE.GENERIC, 0);
            if (result)
            {
                _logger.LogDebug("Successfully deleted credentials for character {CharacterId}", characterId);
                return true;
            }
            else
            {
                var error = Marshal.GetLastWin32Error();
                if (error == 1168) // ERROR_NOT_FOUND
                {
                    _logger.LogDebug("Credentials for character {CharacterId} were already deleted or never existed", characterId);
                    return true; // Consider it successful if already deleted
                }
                
                _logger.LogError("Failed to delete credentials for character {CharacterId}. Win32 Error: {Error}", characterId, error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while deleting credentials for character {CharacterId}", characterId);
            return false;
        }
    }

    /// <summary>
    /// Lists all stored EVE character credentials
    /// </summary>
    /// <returns>List of character IDs with stored credentials</returns>
    public async Task<List<int>> ListStoredCharactersAsync()
    {
        var characterIds = new List<int>();

        try
        {
            if (CredEnumerate($"{TARGET_PREFIX}*", 0, out var count, out var credentialArrayPtr))
            {
                try
                {
                    var credentialPtrs = new IntPtr[count];
                    Marshal.Copy(credentialArrayPtr, credentialPtrs, 0, count);

                    foreach (var credentialPtr in credentialPtrs)
                    {
                        var credential = Marshal.PtrToStructure<CREDENTIAL>(credentialPtr);
                        var targetName = credential.TargetName;
                        
                        if (targetName.StartsWith(TARGET_PREFIX))
                        {
                            var characterIdStr = targetName.Substring(TARGET_PREFIX.Length);
                            if (int.TryParse(characterIdStr, out var characterId))
                            {
                                characterIds.Add(characterId);
                            }
                        }
                    }
                }
                finally
                {
                    CredFree(credentialArrayPtr);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while listing stored characters");
        }

        return characterIds;
    }

    /// <summary>
    /// Checks if credentials exist for a character
    /// </summary>
    /// <param name="characterId">EVE character ID</param>
    /// <returns>True if credentials exist</returns>
    public async Task<bool> HasCredentialsAsync(int characterId)
    {
        var target = $"{TARGET_PREFIX}{characterId}";
        return CredRead(target, CRED_TYPE.GENERIC, 0, out var credentialPtr) && credentialPtr != IntPtr.Zero;
    }

    #region Windows API Imports

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredWrite([In] ref CREDENTIAL userCredential, [In] uint flags);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredRead(string target, CRED_TYPE type, int reservedFlag, out IntPtr credentialPtr);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredDelete(string target, CRED_TYPE type, int reservedFlag);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredEnumerate(string? filter, int flag, out int count, out IntPtr pCredentials);

    [DllImport("advapi32.dll")]
    private static extern void CredFree([In] IntPtr buffer);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct CREDENTIAL
    {
        public uint Flags;
        public CRED_TYPE Type;
        public string TargetName;
        public string Comment;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
        public int CredentialBlobSize;
        public IntPtr CredentialBlob;
        public CRED_PERSIST Persist;
        public int AttributeCount;
        public IntPtr Attributes;
        public string TargetAlias;
        public string UserName;
    }

    private enum CRED_TYPE : uint
    {
        GENERIC = 1,
        DOMAIN_PASSWORD = 2,
        DOMAIN_CERTIFICATE = 3,
        DOMAIN_VISIBLE_PASSWORD = 4,
        GENERIC_CERTIFICATE = 5,
        DOMAIN_EXTENDED = 6,
        MAXIMUM = 7,
        MAXIMUM_EX = (MAXIMUM + 1000)
    }

    private enum CRED_PERSIST : uint
    {
        SESSION = 1,
        LOCAL_MACHINE = 2,
        ENTERPRISE = 3
    }

    #endregion
}

/// <summary>
/// Represents stored credential data
/// </summary>
public class StoredCredentials
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool WillExpireSoon => DateTime.UtcNow.AddMinutes(15) >= ExpiresAt;
}