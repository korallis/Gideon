namespace Gideon.WPF.Infrastructure.Configuration;

/// <summary>
/// Configuration options for Windows App SDK integration
/// </summary>
public class WindowsAppSdkConfiguration
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "WindowsAppSdk";

    /// <summary>
    /// Whether Windows App SDK integration is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Whether to enable enhanced file dialogs
    /// </summary>
    public bool EnableEnhancedFileDialogs { get; set; } = true;

    /// <summary>
    /// Whether to enable modern controls
    /// </summary>
    public bool EnableModernControls { get; set; } = true;

    /// <summary>
    /// Whether to enable packaging features
    /// </summary>
    public bool EnablePackagingFeatures { get; set; } = true;

    /// <summary>
    /// Whether to enable push notifications
    /// </summary>
    public bool EnablePushNotifications { get; set; } = false;

    /// <summary>
    /// Whether to enable widgets support
    /// </summary>
    public bool EnableWidgets { get; set; } = false;

    /// <summary>
    /// Whether to enable power management features
    /// </summary>
    public bool EnablePowerManagement { get; set; } = true;

    /// <summary>
    /// Whether to enable environment manager features
    /// </summary>
    public bool EnableEnvironmentManager { get; set; } = false;

    /// <summary>
    /// Whether to enable deployment API features
    /// </summary>
    public bool EnableDeploymentApi { get; set; } = false;

    /// <summary>
    /// Runtime initialization options
    /// </summary>
    public RuntimeConfiguration Runtime { get; set; } = new();

    /// <summary>
    /// File picker configuration
    /// </summary>
    public FilePickerConfiguration FilePicker { get; set; } = new();

    /// <summary>
    /// Notification configuration
    /// </summary>
    public AppSdkNotificationConfiguration Notifications { get; set; } = new();
}

/// <summary>
/// Windows App SDK runtime configuration
/// </summary>
public class RuntimeConfiguration
{
    /// <summary>
    /// Whether to initialize runtime on startup
    /// </summary>
    public bool InitializeOnStartup { get; set; } = true;

    /// <summary>
    /// Whether to use bootstrap for runtime initialization
    /// </summary>
    public bool UseBootstrap { get; set; } = true;

    /// <summary>
    /// Runtime version to target (if specific version needed)
    /// </summary>
    public string? TargetVersion { get; set; }

    /// <summary>
    /// Architecture to target
    /// </summary>
    public string Architecture { get; set; } = "x64";

    /// <summary>
    /// Whether to enable runtime debugging
    /// </summary>
    public bool EnableDebugging { get; set; } = false;
}

/// <summary>
/// File picker configuration
/// </summary>
public class FilePickerConfiguration
{
    /// <summary>
    /// Default starting location for file pickers
    /// </summary>
    public string? DefaultStartLocation { get; set; }

    /// <summary>
    /// Whether to remember last used locations
    /// </summary>
    public bool RememberLastLocation { get; set; } = true;

    /// <summary>
    /// Whether to show hidden files by default
    /// </summary>
    public bool ShowHiddenFiles { get; set; } = false;

    /// <summary>
    /// Default view mode for file pickers
    /// </summary>
    public FilePickerViewMode DefaultViewMode { get; set; } = FilePickerViewMode.Details;

    /// <summary>
    /// Maximum number of files that can be selected in multi-select mode
    /// </summary>
    public int MaxMultiSelectFiles { get; set; } = 100;
}

/// <summary>
/// File picker view modes
/// </summary>
public enum FilePickerViewMode
{
    List,
    Details,
    Tiles,
    Content,
    Thumbnails
}

/// <summary>
/// App SDK notification configuration
/// </summary>
public class AppSdkNotificationConfiguration
{
    /// <summary>
    /// Whether to enable modern notifications
    /// </summary>
    public bool EnableModernNotifications { get; set; } = true;

    /// <summary>
    /// Whether to enable progress notifications
    /// </summary>
    public bool EnableProgressNotifications { get; set; } = true;

    /// <summary>
    /// Whether to enable interactive notifications
    /// </summary>
    public bool EnableInteractiveNotifications { get; set; } = false;

    /// <summary>
    /// Default notification duration in seconds
    /// </summary>
    public int DefaultDurationSeconds { get; set; } = 5;

    /// <summary>
    /// Whether to show notifications in action center
    /// </summary>
    public bool ShowInActionCenter { get; set; } = true;

    /// <summary>
    /// Whether to play sound with notifications
    /// </summary>
    public bool PlaySound { get; set; } = true;

    /// <summary>
    /// Application display name for notifications
    /// </summary>
    public string AppDisplayName { get; set; } = "Gideon - EVE Online AI Copilot";

    /// <summary>
    /// Application icon path for notifications
    /// </summary>
    public string? AppIconPath { get; set; }
}