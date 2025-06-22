// ==========================================================================
// ServiceInterfaces.cs - Core Service Interfaces
// ==========================================================================
// Collection of essential service interfaces for dependency injection.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Gideon.WPF.Core.Domain.Entities;

namespace Gideon.WPF.Core.Application.Services;

#region Market and Analysis Services

/// <summary>
/// Market analysis service interface
/// </summary>
public interface IMarketAnalysisService
{
    Task<IEnumerable<TradingOpportunity>> GetTradingOpportunitiesAsync(CancellationToken cancellationToken = default);
    Task<MarketPrediction?> GetMarketPredictionAsync(int typeId, int regionId, CancellationToken cancellationToken = default);
    Task UpdateMarketDataAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Skill planning service interface
/// </summary>
public interface ISkillPlanningService
{
    Task<SkillPlan> CreateSkillPlanAsync(int characterId, string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<SkillPlan>> GetSkillPlansAsync(int characterId, CancellationToken cancellationToken = default);
    Task<TimeSpan> CalculateTrainingTimeAsync(int skillPlanId, CancellationToken cancellationToken = default);
}

#endregion

#region Data Synchronization Services

/// <summary>
/// Data synchronization service interface
/// </summary>
public interface IDataSynchronizationService
{
    Task SynchronizeCharacterDataAsync(int characterId, CancellationToken cancellationToken = default);
    Task SynchronizeMarketDataAsync(CancellationToken cancellationToken = default);
    Task SynchronizeStaticDataAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Market data update service interface
/// </summary>
public interface IMarketDataUpdateService
{
    Task UpdateMarketDataAsync(int regionId, CancellationToken cancellationToken = default);
    Task CleanOldMarketDataAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Character data update service interface
/// </summary>
public interface ICharacterDataUpdateService
{
    Task UpdateCharacterSkillsAsync(int characterId, CancellationToken cancellationToken = default);
    Task UpdateCharacterAssetsAsync(int characterId, CancellationToken cancellationToken = default);
}

#endregion

#region Caching Services

/// <summary>
/// Cache service interface
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Market data cache service interface
/// </summary>
public interface IMarketDataCacheService
{
    Task<MarketData?> GetCachedMarketDataAsync(int typeId, int regionId, CancellationToken cancellationToken = default);
    Task SetMarketDataCacheAsync(MarketData marketData, CancellationToken cancellationToken = default);
}

/// <summary>
/// Character data cache service interface
/// </summary>
public interface ICharacterDataCacheService
{
    Task<Character?> GetCachedCharacterAsync(int characterId, CancellationToken cancellationToken = default);
    Task SetCharacterCacheAsync(Character character, CancellationToken cancellationToken = default);
}

#endregion

#region ESI API Services

/// <summary>
/// ESI API service interface
/// </summary>
public interface IEsiApiService
{
    Task<T?> GetAsync<T>(string endpoint, string? accessToken = null, CancellationToken cancellationToken = default);
    Task<T?> PostAsync<T>(string endpoint, object? data = null, string? accessToken = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// ESI character service interface
/// </summary>
public interface IEsiCharacterService
{
    Task<Character?> GetCharacterAsync(long characterId, string accessToken, CancellationToken cancellationToken = default);
    Task<IEnumerable<CharacterSkill>> GetCharacterSkillsAsync(long characterId, string accessToken, CancellationToken cancellationToken = default);
}

/// <summary>
/// ESI market service interface
/// </summary>
public interface IEsiMarketService
{
    Task<IEnumerable<MarketData>> GetMarketOrdersAsync(int regionId, int? typeId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<MarketData>> GetMarketHistoryAsync(int regionId, int typeId, CancellationToken cancellationToken = default);
}

/// <summary>
/// ESI universe service interface
/// </summary>
public interface IEsiUniverseService
{
    Task<EveType?> GetTypeAsync(int typeId, CancellationToken cancellationToken = default);
    Task<EveRegion?> GetRegionAsync(int regionId, CancellationToken cancellationToken = default);
}

/// <summary>
/// ESI skills service interface
/// </summary>
public interface IEsiSkillsService
{
    Task<IEnumerable<CharacterSkill>> GetCharacterSkillsAsync(long characterId, string accessToken, CancellationToken cancellationToken = default);
    Task<CharacterSkill?> GetSkillDetailsAsync(int skillId, CancellationToken cancellationToken = default);
}

#endregion

#region Authentication Services

/// <summary>
/// OAuth2 service interface
/// </summary>
public interface IOAuth2Service
{
    Task<string> GetAuthorizationUrlAsync(string[] scopes, CancellationToken cancellationToken = default);
    Task<string> ExchangeCodeForTokenAsync(string authorizationCode, CancellationToken cancellationToken = default);
    Task<string> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}

/// <summary>
/// Token storage service interface
/// </summary>
public interface ITokenStorageService
{
    Task StoreTokenAsync(long characterId, string accessToken, string refreshToken, DateTime expiry, CancellationToken cancellationToken = default);
    Task<(string accessToken, string refreshToken, DateTime expiry)?> GetTokenAsync(long characterId, CancellationToken cancellationToken = default);
    Task RemoveTokenAsync(long characterId, CancellationToken cancellationToken = default);
}

#endregion

#region Configuration Services

/// <summary>
/// Settings service interface
/// </summary>
public interface ISettingsService
{
    Task<string?> GetSettingAsync(string key, CancellationToken cancellationToken = default);
    Task SetSettingAsync(string key, string value, CancellationToken cancellationToken = default);
    Task<T?> GetSettingAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetSettingAsync<T>(string key, T value, CancellationToken cancellationToken = default);
}

/// <summary>
/// Theme service interface
/// </summary>
public interface IThemeService
{
    Task<IEnumerable<HolographicTheme>> GetThemesAsync(CancellationToken cancellationToken = default);
    Task<HolographicTheme?> GetDefaultThemeAsync(CancellationToken cancellationToken = default);
    Task SetThemeAsync(int themeId, CancellationToken cancellationToken = default);
}

/// <summary>
/// User preferences service interface
/// </summary>
public interface IUserPreferencesService
{
    Task<string?> GetPreferenceAsync(string userId, string key, CancellationToken cancellationToken = default);
    Task SetPreferenceAsync(string userId, string key, string value, CancellationToken cancellationToken = default);
}

#endregion

#region Validation Services

/// <summary>
/// Data validation service interface
/// </summary>
public interface IDataValidationService
{
    Task<bool> ValidateCharacterDataAsync(Character character, CancellationToken cancellationToken = default);
    Task<bool> ValidateMarketDataAsync(MarketData marketData, CancellationToken cancellationToken = default);
}

/// <summary>
/// Ship fitting validation service interface
/// </summary>
public interface IShipFittingValidationService
{
    Task<bool> ValidateFittingAsync(ShipFitting fitting, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetValidationErrorsAsync(ShipFitting fitting, CancellationToken cancellationToken = default);
}

#endregion

#region Performance and Monitoring Services

/// <summary>
/// Performance metrics service interface
/// </summary>
public interface IPerformanceMetricsService
{
    Task RecordMetricAsync(string name, double value, string? category = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<PerformanceMetric>> GetMetricsAsync(string category, DateTime? from = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Audit log service interface
/// </summary>
public interface IAuditLogService
{
    Task LogActionAsync(string action, string? entityType = null, string? entityId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetAuditLogsAsync(DateTime? from = null, string? entityType = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Error log service interface
/// </summary>
public interface IErrorLogService
{
    Task LogErrorAsync(string message, Exception? exception = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<ErrorLog>> GetErrorLogsAsync(DateTime? from = null, bool? resolved = null, CancellationToken cancellationToken = default);
    Task MarkErrorResolvedAsync(int errorId, string? resolution = null, CancellationToken cancellationToken = default);
}

#endregion

#region Ship Fitting Calculation Services

/// <summary>
/// Ammunition calculation service interface
/// </summary>
public interface IAmmunitionCalculationService
{
    Task<AmmunitionEffectResult> CalculateAmmunitionEffectsAsync(ShipFitting fitting, Character? character = null, CancellationToken cancellationToken = default);
}

#endregion

#region Backup and Export Services

/// <summary>
/// Backup service interface
/// </summary>
public interface IBackupService
{
    Task<BackupEntry> CreateBackupAsync(BackupType backupType, int? characterId = null, CancellationToken cancellationToken = default);
    Task<bool> RestoreBackupAsync(int backupId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BackupEntry>> GetBackupsAsync(BackupType? backupType = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Data export service interface
/// </summary>
public interface IDataExportService
{
    Task<string> ExportCharacterDataAsync(int characterId, string format, CancellationToken cancellationToken = default);
    Task<string> ExportShipFittingAsync(Guid fittingId, string format, CancellationToken cancellationToken = default);
    Task<string> ExportSkillPlanAsync(int skillPlanId, string format, CancellationToken cancellationToken = default);
}

/// <summary>
/// Data import service interface
/// </summary>
public interface IDataImportService
{
    Task<bool> ImportShipFittingAsync(string data, string format, int? characterId = null, CancellationToken cancellationToken = default);
    Task<bool> ImportSkillPlanAsync(string data, string format, int characterId, CancellationToken cancellationToken = default);
}