using System.ComponentModel.DataAnnotations;

namespace Gideon.WPF.Infrastructure.Configuration;

/// <summary>
/// Configuration options for Windows system integration features
/// </summary>
public class SystemIntegrationConfiguration
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "SystemIntegration";

    /// <summary>
    /// System tray configuration
    /// </summary>
    public SystemTrayConfiguration SystemTray { get; set; } = new();

    /// <summary>
    /// Jump list configuration
    /// </summary>
    public JumpListConfiguration JumpList { get; set; } = new();

    /// <summary>
    /// Taskbar configuration
    /// </summary>
    public TaskbarConfiguration Taskbar { get; set; } = new();
}

/// <summary>
/// Configuration options for system tray functionality
/// </summary>
public class SystemTrayConfiguration
{
    /// <summary>
    /// Enable system tray icon
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Show tray icon on application startup
    /// </summary>
    public bool ShowOnStartup { get; set; } = true;

    /// <summary>
    /// Minimize to tray instead of taskbar
    /// </summary>
    public bool MinimizeToTray { get; set; } = true;

    /// <summary>
    /// Close to tray instead of exiting
    /// </summary>
    public bool CloseToTray { get; set; } = false;

    /// <summary>
    /// Show balloon tips
    /// </summary>
    public bool ShowBalloonTips { get; set; } = true;

    /// <summary>
    /// Single click action for tray icon
    /// </summary>
    public TrayClickAction SingleClickAction { get; set; } = TrayClickAction.ToggleWindow;

    /// <summary>
    /// Double click action for tray icon
    /// </summary>
    public TrayClickAction DoubleClickAction { get; set; } = TrayClickAction.ShowWindow;

    /// <summary>
    /// Show context menu on right click
    /// </summary>
    public bool ShowContextMenuOnRightClick { get; set; } = true;

    /// <summary>
    /// Update tray icon based on application status
    /// </summary>
    public bool UpdateIconBasedOnStatus { get; set; } = true;

    /// <summary>
    /// Maximum number of characters to show in context menu
    /// </summary>
    [Range(1, 20)]
    public int MaxCharactersInMenu { get; set; } = 10;

    /// <summary>
    /// Show quick action buttons in context menu
    /// </summary>
    public bool ShowQuickActions { get; set; } = true;

    /// <summary>
    /// Balloon tip display duration in milliseconds
    /// </summary>
    [Range(1000, 30000)]
    public int BalloonTipDuration { get; set; } = 5000;
}

/// <summary>
/// Configuration options for Windows jump lists
/// </summary>
public class JumpListConfiguration
{
    /// <summary>
    /// Enable jump list functionality
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Show recent characters in jump list
    /// </summary>
    public bool ShowRecentCharacters { get; set; } = true;

    /// <summary>
    /// Show recent fittings in jump list
    /// </summary>
    public bool ShowRecentFittings { get; set; } = true;

    /// <summary>
    /// Show quick action tasks
    /// </summary>
    public bool ShowQuickActions { get; set; } = true;

    /// <summary>
    /// Maximum number of recent characters to show
    /// </summary>
    [Range(1, 10)]
    public int MaxRecentCharacters { get; set; } = 5;

    /// <summary>
    /// Maximum number of recent fittings to show
    /// </summary>
    [Range(1, 10)]
    public int MaxRecentFittings { get; set; } = 5;

    /// <summary>
    /// Update jump list automatically when data changes
    /// </summary>
    public bool AutoUpdate { get; set; } = true;

    /// <summary>
    /// Show custom categories in jump list
    /// </summary>
    public bool ShowCustomCategories { get; set; } = true;

    /// <summary>
    /// Custom jump list categories to include
    /// </summary>
    public List<string> CustomCategories { get; set; } = new()
    {
        "Quick Actions",
        "Characters",
        "Recent Fittings",
        "Tools"
    };
}

/// <summary>
/// Configuration options for taskbar integration
/// </summary>
public class TaskbarConfiguration
{
    /// <summary>
    /// Enable taskbar progress indicator
    /// </summary>
    public bool EnableProgressIndicator { get; set; } = true;

    /// <summary>
    /// Enable taskbar thumbnail buttons
    /// </summary>
    public bool EnableThumbnailButtons { get; set; } = true;

    /// <summary>
    /// Enable taskbar overlay icons
    /// </summary>
    public bool EnableOverlayIcons { get; set; } = true;

    /// <summary>
    /// Show progress for long-running operations
    /// </summary>
    public bool ShowOperationProgress { get; set; } = true;

    /// <summary>
    /// Show progress for data synchronization
    /// </summary>
    public bool ShowSyncProgress { get; set; } = true;

    /// <summary>
    /// Show progress for calculations
    /// </summary>
    public bool ShowCalculationProgress { get; set; } = false;

    /// <summary>
    /// Thumbnail button actions to include
    /// </summary>
    public List<string> ThumbnailButtonActions { get; set; } = new()
    {
        "ShipFitting",
        "MarketAnalysis",
        "CharacterPlanning"
    };

    /// <summary>
    /// Update taskbar icon based on application state
    /// </summary>
    public bool UpdateIconBasedOnState { get; set; } = true;

    /// <summary>
    /// Show connection status in taskbar
    /// </summary>
    public bool ShowConnectionStatus { get; set; } = true;
}

/// <summary>
/// Actions that can be performed when clicking the tray icon
/// </summary>
public enum TrayClickAction
{
    /// <summary>
    /// Do nothing
    /// </summary>
    None,

    /// <summary>
    /// Show the main window
    /// </summary>
    ShowWindow,

    /// <summary>
    /// Hide the main window
    /// </summary>
    HideWindow,

    /// <summary>
    /// Toggle window visibility
    /// </summary>
    ToggleWindow,

    /// <summary>
    /// Show context menu
    /// </summary>
    ShowContextMenu,

    /// <summary>
    /// Open ship fitting module
    /// </summary>
    OpenShipFitting,

    /// <summary>
    /// Open market analysis module
    /// </summary>
    OpenMarketAnalysis,

    /// <summary>
    /// Open character planning module
    /// </summary>
    OpenCharacterPlanning
}