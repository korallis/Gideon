using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Gideon.WPF.Infrastructure.Configuration;
using System.IO;

namespace Gideon.WPF.Infrastructure.Services;

/// <summary>
/// Service for modern file dialogs with Windows App SDK integration
/// </summary>
public class ModernFileDialogService : IDisposable
{
    private readonly ILogger<ModernFileDialogService> _logger;
    private readonly WindowsAppSdkConfiguration _config;
    private readonly WindowsAppSdkService _appSdkService;
    private bool _isDisposed;

    public ModernFileDialogService(
        ILogger<ModernFileDialogService> logger,
        IOptions<WindowsAppSdkConfiguration> config,
        WindowsAppSdkService appSdkService)
    {
        _logger = logger;
        _config = config.Value;
        _appSdkService = appSdkService;
    }

    /// <summary>
    /// Gets whether modern file dialogs are available
    /// </summary>
    public bool IsAvailable => _appSdkService.IsInitialized && 
                              _config.EnableEnhancedFileDialogs &&
                              _appSdkService.IsFeatureAvailable("fileDialogs");

    /// <summary>
    /// Open file dialog for selecting a single EVE fitting file
    /// </summary>
    public async Task<string?> OpenFittingFileAsync()
    {
        try
        {
            _logger.LogDebug("Opening fitting file dialog...");

            if (IsAvailable)
            {
                var filePicker = _appSdkService.GetEnhancedFilePicker();
                return await filePicker?.PickSingleFileAsync("*.gideon;*.evefitting;*.xml;*.json") ?? await OpenFallbackFileDialogAsync("*.gideon;*.evefitting;*.xml;*.json");
            }

            return await OpenFallbackFileDialogAsync("*.gideon;*.evefitting;*.xml;*.json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open fitting file dialog");
            return null;
        }
    }

    /// <summary>
    /// Open file dialog for selecting multiple fitting files
    /// </summary>
    public async Task<IEnumerable<string>> OpenMultipleFittingFilesAsync()
    {
        try
        {
            _logger.LogDebug("Opening multiple fitting files dialog...");

            if (IsAvailable)
            {
                var filePicker = _appSdkService.GetEnhancedFilePicker();
                return await filePicker?.PickMultipleFilesAsync("*.gideon;*.evefitting;*.xml;*.json") ?? 
                       await OpenFallbackMultipleFileDialogAsync("*.gideon;*.evefitting;*.xml;*.json");
            }

            return await OpenFallbackMultipleFileDialogAsync("*.gideon;*.evefitting;*.xml;*.json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open multiple fitting files dialog");
            return Array.Empty<string>();
        }
    }

    /// <summary>
    /// Save file dialog for EVE fitting files
    /// </summary>
    public async Task<string?> SaveFittingFileAsync(string suggestedFileName = "")
    {
        try
        {
            _logger.LogDebug("Opening save fitting file dialog...");

            if (IsAvailable)
            {
                var filePicker = _appSdkService.GetEnhancedFilePicker();
                return await filePicker?.PickSaveFileAsync(suggestedFileName, "*.gideon;*.evefitting;*.xml;*.json") ?? 
                       await SaveFallbackFileDialogAsync(suggestedFileName, "*.gideon;*.evefitting;*.xml;*.json");
            }

            return await SaveFallbackFileDialogAsync(suggestedFileName, "*.gideon;*.evefitting;*.xml;*.json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open save fitting file dialog");
            return null;
        }
    }

    /// <summary>
    /// Open file dialog for selecting character export files
    /// </summary>
    public async Task<string?> OpenCharacterFileAsync()
    {
        try
        {
            _logger.LogDebug("Opening character file dialog...");

            if (IsAvailable)
            {
                var filePicker = _appSdkService.GetEnhancedFilePicker();
                return await filePicker?.PickSingleFileAsync("*.json;*.xml") ?? await OpenFallbackFileDialogAsync("*.json;*.xml");
            }

            return await OpenFallbackFileDialogAsync("*.json;*.xml");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open character file dialog");
            return null;
        }
    }

    /// <summary>
    /// Save file dialog for character export
    /// </summary>
    public async Task<string?> SaveCharacterFileAsync(string characterName = "")
    {
        try
        {
            _logger.LogDebug("Opening save character file dialog...");
            var suggestedFileName = string.IsNullOrEmpty(characterName) ? "character_export.json" : $"{characterName}_export.json";

            if (IsAvailable)
            {
                var filePicker = _appSdkService.GetEnhancedFilePicker();
                return await filePicker?.PickSaveFileAsync(suggestedFileName, "*.json;*.xml") ?? 
                       await SaveFallbackFileDialogAsync(suggestedFileName, "*.json;*.xml");
            }

            return await SaveFallbackFileDialogAsync(suggestedFileName, "*.json;*.xml");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open save character file dialog");
            return null;
        }
    }

    /// <summary>
    /// Open folder dialog for selecting EVE game directory or data directory
    /// </summary>
    public async Task<string?> SelectFolderAsync(string title = "Select Folder")
    {
        try
        {
            _logger.LogDebug("Opening folder selection dialog...");

            if (IsAvailable)
            {
                var filePicker = _appSdkService.GetEnhancedFilePicker();
                return await filePicker?.PickFolderAsync() ?? await OpenFallbackFolderDialogAsync();
            }

            return await OpenFallbackFolderDialogAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open folder selection dialog");
            return null;
        }
    }

    /// <summary>
    /// Open file dialog for EVE installation directory
    /// </summary>
    public async Task<string?> SelectEveInstallationAsync()
    {
        try
        {
            _logger.LogDebug("Opening EVE installation selection dialog...");

            if (IsAvailable)
            {
                var filePicker = _appSdkService.GetEnhancedFilePicker();
                return await filePicker?.PickSingleFileAsync("*.exe") ?? await OpenFallbackFileDialogAsync("*.exe");
            }

            return await OpenFallbackFileDialogAsync("*.exe");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open EVE installation selection dialog");
            return null;
        }
    }

    #region Fallback Methods

    /// <summary>
    /// Fallback file dialog using standard WPF OpenFileDialog
    /// </summary>
    private async Task<string?> OpenFallbackFileDialogAsync(string filter)
    {
        return await Task.Run(() =>
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = ConvertFilterFormat(filter),
                CheckFileExists = true,
                CheckPathExists = true
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        });
    }

    /// <summary>
    /// Fallback multiple file dialog using standard WPF OpenFileDialog
    /// </summary>
    private async Task<IEnumerable<string>> OpenFallbackMultipleFileDialogAsync(string filter)
    {
        return await Task.Run(() =>
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = ConvertFilterFormat(filter),
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = true
            };

            return dialog.ShowDialog() == true ? dialog.FileNames : Array.Empty<string>();
        });
    }

    /// <summary>
    /// Fallback save file dialog using standard WPF SaveFileDialog
    /// </summary>
    private async Task<string?> SaveFallbackFileDialogAsync(string suggestedFileName, string filter)
    {
        return await Task.Run(() =>
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = ConvertFilterFormat(filter),
                FileName = suggestedFileName,
                CheckPathExists = true
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        });
    }

    /// <summary>
    /// Fallback folder dialog using System.Windows.Forms.FolderBrowserDialog
    /// </summary>
    private async Task<string?> OpenFallbackFolderDialogAsync()
    {
        return await Task.Run(() =>
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Folder",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
            };

            return dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK ? dialog.SelectedPath : null;
        });
    }

    /// <summary>
    /// Convert Windows App SDK filter format to WPF format
    /// </summary>
    private static string ConvertFilterFormat(string filter)
    {
        if (string.IsNullOrEmpty(filter) || filter == "*")
        {
            return "All Files (*.*)|*.*";
        }

        // Convert from "*.gideon;*.evefitting;*.xml;*.json" to "Fitting Files (*.gideon;*.evefitting;*.xml;*.json)|*.gideon;*.evefitting;*.xml;*.json|All Files (*.*)|*.*"
        if (filter.Contains("gideon") || filter.Contains("evefitting"))
        {
            return $"Fitting Files ({filter})|{filter}|All Files (*.*)|*.*";
        }
        if (filter.Contains("json") || filter.Contains("xml"))
        {
            return $"Data Files ({filter})|{filter}|All Files (*.*)|*.*";
        }
        if (filter.Contains("exe"))
        {
            return $"Executable Files ({filter})|{filter}|All Files (*.*)|*.*";
        }

        return $"Files ({filter})|{filter}|All Files (*.*)|*.*";
    }

    #endregion

    /// <summary>
    /// Dispose resources
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _logger.LogDebug("ModernFileDialogService disposed");
        _isDisposed = true;
    }
}