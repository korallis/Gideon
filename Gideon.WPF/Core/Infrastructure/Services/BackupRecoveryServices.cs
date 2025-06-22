// ==========================================================================
// BackupRecoveryServices.cs - Data Backup and Recovery Service Implementations
// ==========================================================================
// Comprehensive backup and recovery services for character data, fittings, and settings.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.Extensions.Logging;
using Gideon.WPF.Core.Application.Services;
using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Core.Domain.Interfaces;
using System.IO.Compression;
using System.Text.Json;

namespace Gideon.WPF.Core.Infrastructure.Services;

#region Backup and Recovery Services

/// <summary>
/// Comprehensive backup and recovery service for application data
/// </summary>
public class BackupService : IBackupService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BackupService> _logger;
    private readonly IDataExportService _exportService;
    private readonly IDataImportService _importService;
    private readonly IAuditLogService _auditService;
    
    private readonly string _backupDirectory;

    public BackupService(
        IUnitOfWork unitOfWork,
        ILogger<BackupService> logger,
        IDataExportService exportService,
        IDataImportService importService,
        IAuditLogService auditService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        _importService = importService ?? throw new ArgumentNullException(nameof(importService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        
        _backupDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Gideon", "Backups");
        
        EnsureBackupDirectoryExists();
    }

    /// <summary>
    /// Create a comprehensive backup of application data
    /// </summary>
    public async Task<BackupEntry> CreateBackupAsync(BackupType backupType, int? characterId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var backupEntry = new BackupEntry
            {
                BackupName = GenerateBackupName(backupType, characterId),
                BackupType = backupType,
                CharacterId = characterId,
                CreatedDate = DateTime.UtcNow,
                BackupSize = 0,
                IsSuccessful = false
            };

            _logger.LogInformation("Starting {BackupType} backup for character {CharacterId}", backupType, characterId);

            var backupData = backupType switch
            {
                BackupType.FullDatabase => await CreateFullDatabaseBackupAsync(cancellationToken),
                BackupType.CharacterData => await CreateCharacterBackupAsync(characterId!.Value, cancellationToken),
                BackupType.Fittings => await CreateFittingsBackupAsync(characterId, cancellationToken),
                BackupType.SkillPlans => await CreateSkillPlansBackupAsync(characterId, cancellationToken),
                BackupType.Settings => await CreateSettingsBackupAsync(cancellationToken),
                BackupType.MarketData => await CreateMarketDataBackupAsync(cancellationToken),
                _ => throw new ArgumentException($"Unsupported backup type: {backupType}")
            };

            // Compress and save backup
            var backupPath = Path.Combine(_backupDirectory, $"{backupEntry.BackupName}.zip");
            await SaveCompressedBackupAsync(backupPath, backupData, cancellationToken);

            // Update backup entry with file information
            var fileInfo = new FileInfo(backupPath);
            backupEntry.FilePath = backupPath;
            backupEntry.BackupSize = fileInfo.Length;
            backupEntry.IsSuccessful = true;
            backupEntry.Checksum = await CalculateChecksumAsync(backupPath, cancellationToken);

            // Save backup entry to database
            await _unitOfWork.BackupEntries.AddAsync(backupEntry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Log audit trail
            await _auditService.LogActionAsync(
                "backup_created",
                "BackupEntry",
                backupEntry.Id.ToString(),
                cancellationToken);

            _logger.LogInformation("Successfully created {BackupType} backup: {BackupName} ({BackupSize} bytes)",
                backupType, backupEntry.BackupName, backupEntry.BackupSize);

            return backupEntry;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating {BackupType} backup for character {CharacterId}", backupType, characterId);
            throw;
        }
    }

    /// <summary>
    /// Restore data from a backup
    /// </summary>
    public async Task<bool> RestoreBackupAsync(int backupId, CancellationToken cancellationToken = default)
    {
        try
        {
            var backupEntry = await _unitOfWork.BackupEntries.GetByIdAsync(backupId, cancellationToken);
            if (backupEntry == null)
            {
                _logger.LogWarning("Backup entry {BackupId} not found", backupId);
                return false;
            }

            if (!File.Exists(backupEntry.FilePath))
            {
                _logger.LogWarning("Backup file not found: {FilePath}", backupEntry.FilePath);
                return false;
            }

            _logger.LogInformation("Starting restore from backup: {BackupName}", backupEntry.BackupName);

            // Verify backup integrity
            var currentChecksum = await CalculateChecksumAsync(backupEntry.FilePath, cancellationToken);
            if (currentChecksum != backupEntry.Checksum)
            {
                _logger.LogError("Backup file integrity check failed for {BackupName}", backupEntry.BackupName);
                return false;
            }

            // Extract and restore backup data
            var backupData = await ExtractBackupDataAsync(backupEntry.FilePath, cancellationToken);
            var restoreResult = await RestoreDataAsync(backupEntry.BackupType, backupData, backupEntry.CharacterId, cancellationToken);

            if (restoreResult)
            {
                // Update backup entry
                backupEntry.LastRestoreDate = DateTime.UtcNow;
                await _unitOfWork.BackupEntries.UpdateAsync(backupEntry, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Log audit trail
                await _auditService.LogActionAsync(
                    "backup_restored",
                    "BackupEntry",
                    backupEntry.Id.ToString(),
                    cancellationToken);

                _logger.LogInformation("Successfully restored backup: {BackupName}", backupEntry.BackupName);
            }

            return restoreResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring backup {BackupId}", backupId);
            return false;
        }
    }

    /// <summary>
    /// Get list of available backups
    /// </summary>
    public async Task<IEnumerable<BackupEntry>> GetBackupsAsync(BackupType? backupType = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _unitOfWork.BackupEntries.AsQueryable();
            
            if (backupType.HasValue)
            {
                query = query.Where(b => b.BackupType == backupType.Value);
            }

            var backups = await Task.FromResult(query.OrderByDescending(b => b.CreatedDate).ToList());
            
            // Verify file existence and update entries if needed
            var validBackups = new List<BackupEntry>();
            foreach (var backup in backups)
            {
                if (File.Exists(backup.FilePath))
                {
                    validBackups.Add(backup);
                }
                else
                {
                    _logger.LogWarning("Backup file missing, marking as invalid: {FilePath}", backup.FilePath);
                    backup.IsSuccessful = false;
                    await _unitOfWork.BackupEntries.UpdateAsync(backup, cancellationToken);
                }
            }

            if (validBackups.Count != backups.Count)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return validBackups;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving backup list");
            return Enumerable.Empty<BackupEntry>();
        }
    }

    /// <summary>
    /// Delete old backups based on retention policy
    /// </summary>
    public async Task CleanupOldBackupsAsync(TimeSpan retentionPeriod, CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow - retentionPeriod;
            var oldBackups = await _unitOfWork.BackupEntries.FindAsync(
                b => b.CreatedDate < cutoffDate, cancellationToken);

            var deletedCount = 0;
            foreach (var backup in oldBackups)
            {
                try
                {
                    if (File.Exists(backup.FilePath))
                    {
                        File.Delete(backup.FilePath);
                    }

                    await _unitOfWork.BackupEntries.RemoveAsync(backup, cancellationToken);
                    deletedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error deleting backup file: {FilePath}", backup.FilePath);
                }
            }

            if (deletedCount > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Cleaned up {DeletedCount} old backup files", deletedCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during backup cleanup");
        }
    }

    /// <summary>
    /// Get backup statistics
    /// </summary>
    public async Task<BackupStatistics> GetBackupStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var allBackups = await _unitOfWork.BackupEntries.GetAllAsync(cancellationToken);
            
            var statistics = new BackupStatistics
            {
                TotalBackups = allBackups.Count(),
                SuccessfulBackups = allBackups.Count(b => b.IsSuccessful),
                TotalBackupSize = allBackups.Sum(b => b.BackupSize),
                BackupsByType = allBackups.GroupBy(b => b.BackupType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                OldestBackup = allBackups.Any() ? allBackups.Min(b => b.CreatedDate) : (DateTime?)null,
                NewestBackup = allBackups.Any() ? allBackups.Max(b => b.CreatedDate) : (DateTime?)null,
                AverageBackupSize = allBackups.Any() ? allBackups.Average(b => b.BackupSize) : 0
            };

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating backup statistics");
            return new BackupStatistics();
        }
    }

    #region Backup Creation Methods

    private async Task<BackupData> CreateFullDatabaseBackupAsync(CancellationToken cancellationToken)
    {
        var data = new BackupData
        {
            BackupType = BackupType.FullDatabase,
            CreatedDate = DateTime.UtcNow,
            Data = new Dictionary<string, object>()
        };

        // Export all major data entities
        var characters = await _unitOfWork.Characters.GetAllAsync(cancellationToken);
        var fittings = await _unitOfWork.ShipFittings.GetAllAsync(cancellationToken);
        var skillPlans = await _unitOfWork.SkillPlans.GetAllAsync(cancellationToken);
        var settings = await _unitOfWork.ApplicationSettings.GetAllAsync(cancellationToken);

        data.Data["characters"] = characters;
        data.Data["fittings"] = fittings;
        data.Data["skillPlans"] = skillPlans;
        data.Data["settings"] = settings;
        data.Data["version"] = GetApplicationVersion();

        return data;
    }

    private async Task<BackupData> CreateCharacterBackupAsync(int characterId, CancellationToken cancellationToken)
    {
        var character = await _unitOfWork.Characters.GetByIdAsync(characterId, cancellationToken);
        if (character == null)
        {
            throw new ArgumentException($"Character {characterId} not found");
        }

        var fittings = await _unitOfWork.ShipFittings.FindAsync(f => f.CharacterId == characterId, cancellationToken);
        var skillPlans = await _unitOfWork.SkillPlans.FindAsync(sp => sp.CharacterId == characterId, cancellationToken);

        var data = new BackupData
        {
            BackupType = BackupType.CharacterData,
            CreatedDate = DateTime.UtcNow,
            Data = new Dictionary<string, object>
            {
                ["character"] = character,
                ["fittings"] = fittings,
                ["skillPlans"] = skillPlans,
                ["version"] = GetApplicationVersion()
            }
        };

        return data;
    }

    private async Task<BackupData> CreateFittingsBackupAsync(int? characterId, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.ShipFittings.AsQueryable();
        if (characterId.HasValue)
        {
            query = query.Where(f => f.CharacterId == characterId.Value);
        }

        var fittings = await Task.FromResult(query.ToList());

        var data = new BackupData
        {
            BackupType = BackupType.Fittings,
            CreatedDate = DateTime.UtcNow,
            Data = new Dictionary<string, object>
            {
                ["fittings"] = fittings,
                ["characterId"] = characterId,
                ["version"] = GetApplicationVersion()
            }
        };

        return data;
    }

    private async Task<BackupData> CreateSkillPlansBackupAsync(int? characterId, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.SkillPlans.AsQueryable();
        if (characterId.HasValue)
        {
            query = query.Where(sp => sp.CharacterId == characterId.Value);
        }

        var skillPlans = await Task.FromResult(query.ToList());

        var data = new BackupData
        {
            BackupType = BackupType.SkillPlans,
            CreatedDate = DateTime.UtcNow,
            Data = new Dictionary<string, object>
            {
                ["skillPlans"] = skillPlans,
                ["characterId"] = characterId,
                ["version"] = GetApplicationVersion()
            }
        };

        return data;
    }

    private async Task<BackupData> CreateSettingsBackupAsync(CancellationToken cancellationToken)
    {
        var settings = await _unitOfWork.ApplicationSettings.GetAllAsync(cancellationToken);
        var preferences = await _unitOfWork.UserPreferences.GetAllAsync(cancellationToken);
        var themes = await _unitOfWork.HolographicThemes.GetAllAsync(cancellationToken);

        var data = new BackupData
        {
            BackupType = BackupType.Settings,
            CreatedDate = DateTime.UtcNow,
            Data = new Dictionary<string, object>
            {
                ["settings"] = settings,
                ["preferences"] = preferences,
                ["themes"] = themes,
                ["version"] = GetApplicationVersion()
            }
        };

        return data;
    }

    private async Task<BackupData> CreateMarketDataBackupAsync(CancellationToken cancellationToken)
    {
        // Only backup recent market data (last 7 days)
        var cutoffDate = DateTime.UtcNow.AddDays(-7);
        var marketData = await _unitOfWork.MarketData.FindAsync(m => m.RecordedDate >= cutoffDate, cancellationToken);

        var data = new BackupData
        {
            BackupType = BackupType.MarketData,
            CreatedDate = DateTime.UtcNow,
            Data = new Dictionary<string, object>
            {
                ["marketData"] = marketData,
                ["cutoffDate"] = cutoffDate,
                ["version"] = GetApplicationVersion()
            }
        };

        return data;
    }

    #endregion

    #region Backup Restoration Methods

    private async Task<bool> RestoreDataAsync(BackupType backupType, BackupData backupData, int? characterId, CancellationToken cancellationToken)
    {
        return backupType switch
        {
            BackupType.FullDatabase => await RestoreFullDatabaseAsync(backupData, cancellationToken),
            BackupType.CharacterData => await RestoreCharacterDataAsync(backupData, cancellationToken),
            BackupType.Fittings => await RestoreFittingsAsync(backupData, cancellationToken),
            BackupType.SkillPlans => await RestoreSkillPlansAsync(backupData, cancellationToken),
            BackupType.Settings => await RestoreSettingsAsync(backupData, cancellationToken),
            BackupType.MarketData => await RestoreMarketDataAsync(backupData, cancellationToken),
            _ => false
        };
    }

    private async Task<bool> RestoreFullDatabaseAsync(BackupData backupData, CancellationToken cancellationToken)
    {
        try
        {
            // This would require careful handling to avoid data conflicts
            // For now, this is a placeholder implementation
            _logger.LogWarning("Full database restore is not yet implemented - too risky");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring full database");
            return false;
        }
    }

    private async Task<bool> RestoreCharacterDataAsync(BackupData backupData, CancellationToken cancellationToken)
    {
        try
        {
            if (backupData.Data.TryGetValue("character", out var characterObj) && characterObj is Character character)
            {
                // Check if character already exists
                var existing = await _unitOfWork.Characters.FindFirstAsync(
                    c => c.CharacterId == character.CharacterId, cancellationToken);

                if (existing == null)
                {
                    await _unitOfWork.Characters.AddAsync(character, cancellationToken);
                }
                else
                {
                    // Update existing character
                    existing.CharacterName = character.CharacterName;
                    existing.CorporationId = character.CorporationId;
                    existing.AllianceId = character.AllianceId;
                    existing.SecurityStatus = character.SecurityStatus;
                    existing.TotalSp = character.TotalSp;
                    await _unitOfWork.Characters.UpdateAsync(existing, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring character data");
            return false;
        }
    }

    private async Task<bool> RestoreFittingsAsync(BackupData backupData, CancellationToken cancellationToken)
    {
        try
        {
            if (backupData.Data.TryGetValue("fittings", out var fittingsObj) && 
                fittingsObj is IEnumerable<ShipFitting> fittings)
            {
                foreach (var fitting in fittings)
                {
                    // Generate new ID to avoid conflicts
                    fitting.Id = Guid.NewGuid();
                    fitting.ModifiedDate = DateTime.UtcNow;
                    await _unitOfWork.ShipFittings.AddAsync(fitting, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring fittings");
            return false;
        }
    }

    private async Task<bool> RestoreSkillPlansAsync(BackupData backupData, CancellationToken cancellationToken)
    {
        try
        {
            if (backupData.Data.TryGetValue("skillPlans", out var skillPlansObj) && 
                skillPlansObj is IEnumerable<SkillPlan> skillPlans)
            {
                foreach (var skillPlan in skillPlans)
                {
                    // Generate new ID to avoid conflicts
                    skillPlan.Id = 0; // Let EF assign new ID
                    skillPlan.ModifiedDate = DateTime.UtcNow;
                    await _unitOfWork.SkillPlans.AddAsync(skillPlan, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring skill plans");
            return false;
        }
    }

    private async Task<bool> RestoreSettingsAsync(BackupData backupData, CancellationToken cancellationToken)
    {
        try
        {
            if (backupData.Data.TryGetValue("settings", out var settingsObj) && 
                settingsObj is IEnumerable<ApplicationSettings> settings)
            {
                foreach (var setting in settings)
                {
                    var existing = await _unitOfWork.ApplicationSettings.FindFirstAsync(
                        s => s.SettingKey == setting.SettingKey, cancellationToken);

                    if (existing == null)
                    {
                        await _unitOfWork.ApplicationSettings.AddAsync(setting, cancellationToken);
                    }
                    else
                    {
                        existing.SettingValue = setting.SettingValue;
                        existing.ModifiedDate = DateTime.UtcNow;
                        await _unitOfWork.ApplicationSettings.UpdateAsync(existing, cancellationToken);
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring settings");
            return false;
        }
    }

    private async Task<bool> RestoreMarketDataAsync(BackupData backupData, CancellationToken cancellationToken)
    {
        try
        {
            if (backupData.Data.TryGetValue("marketData", out var marketDataObj) && 
                marketDataObj is IEnumerable<MarketData> marketData)
            {
                // Only restore if market data is not too old
                var cutoffDate = DateTime.UtcNow.AddDays(-1);
                var recentData = marketData.Where(m => m.RecordedDate >= cutoffDate);

                foreach (var data in recentData)
                {
                    await _unitOfWork.MarketData.AddAsync(data, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring market data");
            return false;
        }
    }

    #endregion

    #region Helper Methods

    private void EnsureBackupDirectoryExists()
    {
        if (!Directory.Exists(_backupDirectory))
        {
            Directory.CreateDirectory(_backupDirectory);
            _logger.LogInformation("Created backup directory: {BackupDirectory}", _backupDirectory);
        }
    }

    private static string GenerateBackupName(BackupType backupType, int? characterId)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var suffix = characterId.HasValue ? $"_char{characterId}" : "";
        return $"Gideon_{backupType}_{timestamp}{suffix}";
    }

    private async Task SaveCompressedBackupAsync(string filePath, BackupData backupData, CancellationToken cancellationToken)
    {
        var jsonData = JsonSerializer.Serialize(backupData, new JsonSerializerOptions 
        { 
            WriteIndented = false 
        });

        using var fileStream = new FileStream(filePath, FileMode.Create);
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Create);
        
        var entry = archive.CreateEntry("backup.json");
        using var entryStream = entry.Open();
        using var writer = new StreamWriter(entryStream);
        
        await writer.WriteAsync(jsonData);
    }

    private async Task<BackupData> ExtractBackupDataAsync(string filePath, CancellationToken cancellationToken)
    {
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
        
        var entry = archive.GetEntry("backup.json");
        if (entry == null)
        {
            throw new InvalidOperationException("Invalid backup file format");
        }

        using var entryStream = entry.Open();
        using var reader = new StreamReader(entryStream);
        
        var jsonData = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<BackupData>(jsonData) ?? new BackupData();
    }

    private async Task<string> CalculateChecksumAsync(string filePath, CancellationToken cancellationToken)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        using var stream = File.OpenRead(filePath);
        
        var hash = await md5.ComputeHashAsync(stream, cancellationToken);
        return Convert.ToHexString(hash);
    }

    private static string GetApplicationVersion()
    {
        return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
    }

    #endregion
}

#endregion

#region Supporting Data Structures

/// <summary>
/// Backup data container
/// </summary>
public class BackupData
{
    public BackupType BackupType { get; set; }
    public DateTime CreatedDate { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

/// <summary>
/// Backup statistics summary
/// </summary>
public class BackupStatistics
{
    public int TotalBackups { get; set; }
    public int SuccessfulBackups { get; set; }
    public long TotalBackupSize { get; set; }
    public Dictionary<BackupType, int> BackupsByType { get; set; } = new();
    public DateTime? OldestBackup { get; set; }
    public DateTime? NewestBackup { get; set; }
    public double AverageBackupSize { get; set; }
}

#endregion