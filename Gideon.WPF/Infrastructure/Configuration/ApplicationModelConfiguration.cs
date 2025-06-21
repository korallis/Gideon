using System.ComponentModel.DataAnnotations;

namespace Gideon.WPF.Infrastructure.Configuration;

/// <summary>
/// Configuration options for Windows ApplicationModel integration
/// </summary>
public class ApplicationModelConfiguration
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "ApplicationModel";

    /// <summary>
    /// Enable automatic application lifecycle management
    /// </summary>
    public bool EnableLifecycleManagement { get; set; } = true;

    /// <summary>
    /// Enable automatic data persistence on suspend
    /// </summary>
    public bool EnableAutomaticDataPersistence { get; set; } = true;

    /// <summary>
    /// Enable protocol activation handling
    /// </summary>
    public bool EnableProtocolActivation { get; set; } = true;

    /// <summary>
    /// Enable file activation handling
    /// </summary>
    public bool EnableFileActivation { get; set; } = true;

    /// <summary>
    /// Enable Windows notification integration
    /// </summary>
    public bool EnableNotifications { get; set; } = true;

    /// <summary>
    /// Enable app restart functionality
    /// </summary>
    public bool EnableAppRestart { get; set; } = true;

    /// <summary>
    /// Minimum time between data persistence operations (in seconds)
    /// </summary>
    [Range(1, 3600)]
    public int DataPersistenceIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum size of cached application data (in MB)
    /// </summary>
    [Range(1, 1024)]
    public int MaxCachedDataSizeMB { get; set; } = 100;

    /// <summary>
    /// Enable roaming data synchronization
    /// </summary>
    public bool EnableRoamingData { get; set; } = false;

    /// <summary>
    /// Enable background task registration
    /// </summary>
    public bool EnableBackgroundTasks { get; set; } = false;

    /// <summary>
    /// Custom protocol schemes to register
    /// </summary>
    public List<string> CustomProtocolSchemes { get; set; } = new()
    {
        "gideon",
        "eve-fitting"
    };

    /// <summary>
    /// File extensions to register for file activation
    /// </summary>
    public List<string> FileExtensions { get; set; } = new()
    {
        ".gideon",
        ".evefitting"
    };

    /// <summary>
    /// Application data keys that should be persisted automatically
    /// </summary>
    public List<string> AutoPersistDataKeys { get; set; } = new()
    {
        "UserSettings",
        "CharacterData",
        "WindowState",
        "UILayout"
    };

    /// <summary>
    /// Enable Windows 11 specific features
    /// </summary>
    public bool EnableWindows11Features { get; set; } = true;

    /// <summary>
    /// Enable Mica material support
    /// </summary>
    public bool EnableMicaMaterial { get; set; } = true;

    /// <summary>
    /// Enable Windows 11 rounded corners
    /// </summary>
    public bool EnableRoundedCorners { get; set; } = true;

    /// <summary>
    /// Enable Windows 11 theme synchronization
    /// </summary>
    public bool EnableThemeSynchronization { get; set; } = true;

    /// <summary>
    /// Application restart arguments
    /// </summary>
    public string RestartArguments { get; set; } = "--restarted";

    /// <summary>
    /// Enable debug logging for ApplicationModel
    /// </summary>
    public bool EnableDebugLogging { get; set; } = false;
}

/// <summary>
/// Configuration options for Windows ApplicationModel events
/// </summary>
public class ApplicationModelEventConfiguration
{
    /// <summary>
    /// Enable suspension event handling
    /// </summary>
    public bool HandleSuspensionEvents { get; set; } = true;

    /// <summary>
    /// Enable resume event handling
    /// </summary>
    public bool HandleResumeEvents { get; set; } = true;

    /// <summary>
    /// Enable activation event handling
    /// </summary>
    public bool HandleActivationEvents { get; set; } = true;

    /// <summary>
    /// Enable low memory event handling
    /// </summary>
    public bool HandleLowMemoryEvents { get; set; } = true;

    /// <summary>
    /// Enable app data backup on suspension
    /// </summary>
    public bool BackupDataOnSuspension { get; set; } = true;

    /// <summary>
    /// Enable app state restoration on resume
    /// </summary>
    public bool RestoreStateOnResume { get; set; } = true;

    /// <summary>
    /// Suspension timeout in seconds
    /// </summary>
    [Range(1, 60)]
    public int SuspensionTimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// Enable automatic memory cleanup on low memory
    /// </summary>
    public bool AutoCleanupOnLowMemory { get; set; } = true;
}

/// <summary>
/// Configuration options for application data management
/// </summary>
public class ApplicationDataConfiguration
{
    /// <summary>
    /// Enable automatic data cleanup
    /// </summary>
    public bool EnableAutomaticCleanup { get; set; } = true;

    /// <summary>
    /// Data retention period in days
    /// </summary>
    [Range(1, 365)]
    public int DataRetentionDays { get; set; } = 30;

    /// <summary>
    /// Enable data compression
    /// </summary>
    public bool EnableDataCompression { get; set; } = true;

    /// <summary>
    /// Enable data encryption
    /// </summary>
    public bool EnableDataEncryption { get; set; } = false;

    /// <summary>
    /// Maximum file size for individual data files (in MB)
    /// </summary>
    [Range(1, 100)]
    public int MaxFileSizeMB { get; set; } = 10;

    /// <summary>
    /// Data folder name in application data
    /// </summary>
    public string DataFolderName { get; set; } = "GideonData";

    /// <summary>
    /// Enable data versioning
    /// </summary>
    public bool EnableDataVersioning { get; set; } = true;

    /// <summary>
    /// Number of data versions to keep
    /// </summary>
    [Range(1, 10)]
    public int MaxDataVersions { get; set; } = 3;
}