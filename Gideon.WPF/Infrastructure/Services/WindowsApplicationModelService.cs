using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Text.Json;

namespace Gideon.WPF.Infrastructure.Services;

/// <summary>
/// Service for integrating with Windows ApplicationModel for modern app features
/// </summary>
public class WindowsApplicationModelService
{
    private readonly ILogger<WindowsApplicationModelService> _logger;
    private bool _isInitialized;

    public WindowsApplicationModelService(ILogger<WindowsApplicationModelService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets the application package information
    /// </summary>
    public PackageInfo? PackageInfo { get; private set; }

    /// <summary>
    /// Gets whether the app is running in a packaged context
    /// </summary>
    public bool IsPackaged { get; private set; }

    /// <summary>
    /// Gets the application data folder path
    /// </summary>
    public string? ApplicationDataFolderPath { get; private set; }

    /// <summary>
    /// Gets the local application data folder path
    /// </summary>
    public string? LocalDataFolderPath { get; private set; }

    /// <summary>
    /// Gets the roaming application data folder path
    /// </summary>
    public string? RoamingDataFolderPath { get; private set; }

    /// <summary>
    /// Gets the temporary application data folder path
    /// </summary>
    public string? TemporaryDataFolderPath { get; private set; }

    /// <summary>
    /// Event fired when the app is activated
    /// </summary>
    public event EventHandler<AppActivatedEventArgs>? AppActivated;

    /// <summary>
    /// Event fired when the app is suspended
    /// </summary>
    public event EventHandler<AppSuspendedEventArgs>? AppSuspended;

    /// <summary>
    /// Event fired when the app is resumed
    /// </summary>
    public event EventHandler<AppResumedEventArgs>? AppResumed;

    /// <summary>
    /// Initializes the Windows ApplicationModel service
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            // Check if we're running in a packaged context
            IsPackaged = await CheckIfPackagedAsync();
            
            if (IsPackaged)
            {
                await InitializePackagedAppAsync();
            }
            else
            {
                await InitializeUnpackagedAppAsync();
            }

            _isInitialized = true;
            _logger.LogInformation("Windows ApplicationModel service initialized. Packaged: {IsPackaged}", IsPackaged);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Windows ApplicationModel service");
        }
    }

    /// <summary>
    /// Gets the application version
    /// </summary>
    public string GetApplicationVersion()
    {
        try
        {
            if (IsPackaged && PackageInfo != null)
            {
                var version = PackageInfo.Version;
                return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            }
            else
            {
                var assembly = Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                return version?.ToString() ?? "1.0.0.0";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get application version");
            return "1.0.0.0";
        }
    }

    /// <summary>
    /// Gets the application display name
    /// </summary>
    public string GetApplicationDisplayName()
    {
        try
        {
            if (IsPackaged && PackageInfo != null)
            {
                return PackageInfo.DisplayName;
            }
            else
            {
                var assembly = Assembly.GetExecutingAssembly();
                var title = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
                return title ?? "Gideon";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get application display name");
            return "Gideon";
        }
    }

    /// <summary>
    /// Gets the application publisher
    /// </summary>
    public string GetApplicationPublisher()
    {
        try
        {
            if (IsPackaged && PackageInfo != null)
            {
                return PackageInfo.PublisherDisplayName;
            }
            else
            {
                var assembly = Assembly.GetExecutingAssembly();
                var company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
                return company ?? "Gideon Development Team";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get application publisher");
            return "Gideon Development Team";
        }
    }

    /// <summary>
    /// Requests to restart the application
    /// </summary>
    public async Task<bool> RequestRestartAsync(string? arguments = null)
    {
        try
        {
            // For WPF apps, we handle restart manually
            var currentProcess = Process.GetCurrentProcess();
            var startInfo = new ProcessStartInfo
            {
                FileName = currentProcess.MainModule?.FileName ?? string.Empty,
                Arguments = arguments ?? string.Empty,
                UseShellExecute = true
            };

            Process.Start(startInfo);
            System.Windows.Application.Current.Shutdown();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to request application restart");
            return false;
        }
    }

    /// <summary>
    /// Launches the default app for the specified URI
    /// </summary>
    public async Task<bool> LaunchUriAsync(Uri uri)
    {
        try
        {
            Process.Start(new ProcessStartInfo(uri.ToString()) { UseShellExecute = true });
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to launch URI: {Uri}", uri);
            return false;
        }
    }

    /// <summary>
    /// Saves application data to local storage
    /// </summary>
    public async Task<bool> SaveApplicationDataAsync(string key, string data)
    {
        try
        {
            var appDataPath = LocalDataFolderPath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gideon");
            Directory.CreateDirectory(appDataPath);
            var filePath = Path.Combine(appDataPath, $"{key}.json");
            await File.WriteAllTextAsync(filePath, data);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save application data for key: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// Loads application data from local storage
    /// </summary>
    public async Task<string?> LoadApplicationDataAsync(string key)
    {
        try
        {
            var appDataPath = LocalDataFolderPath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gideon");
            var filePath = Path.Combine(appDataPath, $"{key}.json");
            if (File.Exists(filePath))
            {
                return await File.ReadAllTextAsync(filePath);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load application data for key: {Key}", key);
            return null;
        }
    }

    /// <summary>
    /// Deletes application data from local storage
    /// </summary>
    public async Task<bool> DeleteApplicationDataAsync(string key)
    {
        try
        {
            var appDataPath = LocalDataFolderPath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gideon");
            var filePath = Path.Combine(appDataPath, $"{key}.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete application data for key: {Key}", key);
            return false;
        }
    }

    private async Task<bool> CheckIfPackagedAsync()
    {
        try
        {
            // For WPF apps, we assume they are not packaged by default
            // This could be enhanced to check for MSIX packaging
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Exception while checking if app is packaged");
            return false;
        }
    }

    private async Task InitializePackagedAppAsync()
    {
        try
        {
            // This method is kept for future MSIX packaging support
            await InitializeUnpackagedAppAsync();
            _logger.LogDebug("Packaged app initialized: {DisplayName} v{Version}", 
                PackageInfo?.DisplayName ?? "Unknown", GetApplicationVersion());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize packaged app");
        }
    }

    private async Task InitializeUnpackagedAppAsync()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version ?? new Version(1, 0, 0, 0);

            PackageInfo = new PackageInfo
            {
                Id = assembly.GetName().Name ?? "Gideon.WPF",
                DisplayName = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "Gideon",
                PublisherDisplayName = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "Gideon Development Team",
                Version = new ApplicationPackageVersion
                {
                    Major = (ushort)version.Major,
                    Minor = (ushort)version.Minor,
                    Build = (ushort)version.Build,
                    Revision = (ushort)version.Revision
                },
                InstallDate = File.GetCreationTime(assembly.Location),
                InstalledLocation = Path.GetDirectoryName(assembly.Location) ?? string.Empty
            };

            // Set up application data folders
            var appDataRoot = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appDataPath = Path.Combine(appDataRoot, "Gideon");
            
            ApplicationDataFolderPath = appDataPath;
            LocalDataFolderPath = appDataPath;
            RoamingDataFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gideon");
            TemporaryDataFolderPath = Path.Combine(Path.GetTempPath(), "Gideon");

            // Create directories if they don't exist
            Directory.CreateDirectory(LocalDataFolderPath);
            Directory.CreateDirectory(RoamingDataFolderPath);
            Directory.CreateDirectory(TemporaryDataFolderPath);

            _logger.LogDebug("Unpackaged app initialized: {DisplayName} v{Version}", 
                PackageInfo.DisplayName, GetApplicationVersion());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize unpackaged app");
        }
    }

    private void OnSuspending(object? sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Application suspending");
            AppSuspended?.Invoke(this, new AppSuspendedEventArgs(DateTime.UtcNow.AddMinutes(5)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during application suspension");
        }
    }

    private void OnResuming(object? sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Application resuming");
            AppResumed?.Invoke(this, new AppResumedEventArgs());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during application resume");
        }
    }

    public void Dispose()
    {
        // Clean up any resources
        _logger.LogDebug("ApplicationModel service disposed");
    }
}

/// <summary>
/// Contains package information for the application
/// </summary>
public class PackageInfo
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PublisherDisplayName { get; set; } = string.Empty;
    public ApplicationPackageVersion Version { get; set; } = new();
    public DateTime InstallDate { get; set; }
    public string InstalledLocation { get; set; } = string.Empty;
}

/// <summary>
/// Represents application package version information
/// </summary>
public class ApplicationPackageVersion
{
    public ushort Major { get; set; }
    public ushort Minor { get; set; }
    public ushort Build { get; set; }
    public ushort Revision { get; set; }

    public override string ToString()
    {
        return $"{Major}.{Minor}.{Build}.{Revision}";
    }
}

/// <summary>
/// Event arguments for app activated events
/// </summary>
public class AppActivatedEventArgs : EventArgs
{
    public ActivationKind Kind { get; }
    public string Arguments { get; }

    public AppActivatedEventArgs(ActivationKind kind, string arguments)
    {
        Kind = kind;
        Arguments = arguments;
    }
}

/// <summary>
/// Event arguments for app suspended events
/// </summary>
public class AppSuspendedEventArgs : EventArgs
{
    public DateTime Deadline { get; }

    public AppSuspendedEventArgs(DateTime deadline)
    {
        Deadline = deadline;
    }
}

/// <summary>
/// Event arguments for app resumed events
/// </summary>
public class AppResumedEventArgs : EventArgs
{
}