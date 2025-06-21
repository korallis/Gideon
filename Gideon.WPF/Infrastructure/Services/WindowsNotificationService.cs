using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.ComponentModel;
using Gideon.WPF.Infrastructure.Configuration;

namespace Gideon.WPF.Infrastructure.Services;

/// <summary>
/// Service for Windows notification system integration using WPF-compatible methods
/// </summary>
public class WindowsNotificationService
{
    private readonly ILogger<WindowsNotificationService> _logger;
    private readonly ApplicationModelConfiguration _config;
    private readonly List<NotificationInfo> _activeNotifications = new();
    private bool _isInitialized;

    public WindowsNotificationService(
        ILogger<WindowsNotificationService> logger,
        IOptions<ApplicationModelConfiguration> config)
    {
        _logger = logger;
        _config = config.Value;
    }

    /// <summary>
    /// Gets whether notifications are supported on this system
    /// </summary>
    public bool IsSupported
    {
        get
        {
            try
            {
                // Check if we're on Windows 10/11 with basic notification support
                var version = Environment.OSVersion.Version;
                return version.Major >= 6; // Windows Vista and later
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Gets whether notifications are enabled
    /// </summary>
    public bool IsEnabled => _config.EnableNotifications && IsSupported;

    /// <summary>
    /// Event fired when a notification is activated
    /// </summary>
    public event EventHandler<NotificationActivatedEventArgs>? NotificationActivated;

    /// <summary>
    /// Event fired when a notification is dismissed
    /// </summary>
    public event EventHandler<NotificationDismissedEventArgs>? NotificationDismissed;

    /// <summary>
    /// Initializes the notification service
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized || !IsEnabled) return;

        try
        {
            // Initialize with WPF-compatible notification system
            // This uses the Windows system tray and balloon tips
            _isInitialized = true;
            _logger.LogInformation("Windows notification service initialized (WPF mode)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Windows notification service");
        }
    }

    /// <summary>
    /// Shows a simple notification using Windows system tray balloon
    /// </summary>
    public async Task ShowNotificationAsync(string title, string message, NotificationType type = NotificationType.Info)
    {
        if (!IsEnabled) return;

        try
        {
            // Use Windows Forms NotifyIcon for system tray notifications
            await ShowSystemTrayNotificationAsync(title, message, type);
            _logger.LogDebug("Showed notification: {Title}", title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show notification: {Title}", title);
        }
    }

    /// <summary>
    /// Shows a character-related notification
    /// </summary>
    public async Task ShowCharacterNotificationAsync(string characterName, string message, string? imageUrl = null)
    {
        if (!IsEnabled) return;

        var title = $"Character: {characterName}";
        await ShowNotificationAsync(title, message, NotificationType.Character);
    }

    /// <summary>
    /// Shows a market-related notification
    /// </summary>
    public async Task ShowMarketNotificationAsync(string itemName, decimal price, string region, string? imageUrl = null)
    {
        if (!IsEnabled) return;

        var title = "Market Alert";
        var message = $"{itemName} is now {price:N2} ISK in {region}";
        await ShowNotificationAsync(title, message, NotificationType.Market);
    }

    /// <summary>
    /// Shows a skill training notification
    /// </summary>
    public async Task ShowSkillNotificationAsync(string skillName, string characterName, string? imageUrl = null)
    {
        if (!IsEnabled) return;

        var title = "Skill Training Complete";
        var message = $"{skillName} training completed for {characterName}";
        await ShowNotificationAsync(title, message, NotificationType.Skill);
    }

    /// <summary>
    /// Shows a fitting-related notification
    /// </summary>
    public async Task ShowFittingNotificationAsync(string fittingName, string message, string? imageUrl = null)
    {
        if (!IsEnabled) return;

        var title = $"Fitting: {fittingName}";
        await ShowNotificationAsync(title, message, NotificationType.Fitting);
    }

    /// <summary>
    /// Shows an advanced notification with custom actions (simplified for WPF)
    /// </summary>
    public async Task ShowAdvancedNotificationAsync(
        string title, 
        string message, 
        string? imageUrl = null,
        NotificationAction[]? actions = null,
        NotificationType type = NotificationType.Info,
        DateTime? expirationTime = null)
    {
        if (!IsEnabled) return;

        // For WPF, we'll show a standard notification and log the advanced features
        await ShowNotificationAsync(title, message, type);
        
        if (actions != null && actions.Length > 0)
        {
            _logger.LogDebug("Advanced notification actions available: {Actions}", 
                string.Join(", ", actions.Select(a => a.Content)));
        }
    }

    /// <summary>
    /// Clears all notifications for the application
    /// </summary>
    public void ClearAllNotifications()
    {
        if (!IsEnabled) return;

        try
        {
            _activeNotifications.Clear();
            _logger.LogDebug("Cleared all notifications");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear notifications");
        }
    }

    /// <summary>
    /// Removes a specific notification by tag
    /// </summary>
    public void RemoveNotification(string tag, string? group = null)
    {
        if (!IsEnabled) return;

        try
        {
            _activeNotifications.RemoveAll(n => n.Tag == tag && (group == null || n.Group == group));
            _logger.LogDebug("Removed notification with tag: {Tag}", tag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove notification: {Tag}", tag);
        }
    }

    private async Task ShowSystemTrayNotificationAsync(string title, string message, NotificationType type)
    {
        try
        {
            // Create a notification record
            var notification = new NotificationInfo
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Message = message,
                Type = type,
                Timestamp = DateTime.UtcNow
            };

            _activeNotifications.Add(notification);

            // Use Windows API to show balloon notification
            ShowBalloonTip(title, message, GetBalloonIcon(type));

            // Play notification sound if enabled
            if (_config.EnableNotifications)
            {
                PlayNotificationSound(type);
            }

            // Simulate notification activation after a delay (for testing)
            await Task.Delay(100);
            
            _logger.LogDebug("Showed system tray notification: {Title}", title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show system tray notification");
        }
    }

    private void ShowBalloonTip(string title, string message, int icon)
    {
        try
        {
            // Use Windows Shell API to show balloon tip
            var nid = new NOTIFYICONDATA
            {
                cbSize = Marshal.SizeOf<NOTIFYICONDATA>(),
                hWnd = GetConsoleWindow(),
                uID = 1,
                uFlags = NIF_INFO,
                dwInfoFlags = icon,
                szInfoTitle = title,
                szInfo = message,
                uTimeout = 5000
            };

            Shell_NotifyIcon(NIM_MODIFY, ref nid);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to show balloon tip, falling back to console output");
            
            // Fallback: Log to console for development
            Console.WriteLine($"NOTIFICATION: {title} - {message}");
        }
    }

    private int GetBalloonIcon(NotificationType type) => type switch
    {
        NotificationType.Success => NIIF_INFO,
        NotificationType.Warning => NIIF_WARNING,
        NotificationType.Error => NIIF_ERROR,
        _ => NIIF_INFO
    };

    private void PlayNotificationSound(NotificationType type)
    {
        try
        {
            var soundType = type switch
            {
                NotificationType.Success => 0x00000040, // MB_ICONASTERISK
                NotificationType.Warning => 0x00000030, // MB_ICONEXCLAMATION  
                NotificationType.Error => 0x00000010, // MB_ICONHAND
                _ => 0x00000040 // MB_ICONASTERISK
            };

            MessageBeep(soundType);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to play notification sound");
        }
    }

    public void Dispose()
    {
        ClearAllNotifications();
    }

    #region Windows API

    [DllImport("shell32.dll")]
    private static extern bool Shell_NotifyIcon(uint dwMessage, ref NOTIFYICONDATA pnid);

    [DllImport("user32.dll")]
    private static extern bool MessageBeep(uint uType);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NOTIFYICONDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public uint uID;
        public uint uFlags;
        public uint uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
        public uint dwState;
        public uint dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szInfo;
        public uint uTimeout;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;
        public uint dwInfoFlags;
        public Guid guidItem;
        public IntPtr hBalloonIcon;
    }

    private const uint NIM_ADD = 0x00000000;
    private const uint NIM_MODIFY = 0x00000001;
    private const uint NIM_DELETE = 0x00000002;
    private const uint NIF_MESSAGE = 0x00000001;
    private const uint NIF_ICON = 0x00000002;
    private const uint NIF_TIP = 0x00000004;
    private const uint NIF_STATE = 0x00000008;
    private const uint NIF_INFO = 0x00000010;
    private const uint NIF_GUID = 0x00000020;

    private const uint NIIF_NONE = 0x00000000;
    private const uint NIIF_INFO = 0x00000001;
    private const uint NIIF_WARNING = 0x00000002;
    private const uint NIIF_ERROR = 0x00000003;

    #endregion
}

/// <summary>
/// Information about a notification
/// </summary>
public class NotificationInfo
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Tag { get; set; }
    public string? Group { get; set; }
}

/// <summary>
/// Represents a notification action
/// </summary>
public class NotificationAction
{
    public string Id { get; }
    public string Content { get; }
    public string Arguments { get; }
    public string? ImageUri { get; }

    public NotificationAction(string id, string content, string arguments, string? imageUri = null)
    {
        Id = id;
        Content = content;
        Arguments = arguments;
        ImageUri = imageUri;
    }
}

/// <summary>
/// Types of notifications
/// </summary>
public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error,
    Character,
    Market,
    Skill,
    Fitting
}

/// <summary>
/// Event arguments for notification activation
/// </summary>
public class NotificationActivatedEventArgs : EventArgs
{
    public string Arguments { get; }

    public NotificationActivatedEventArgs(string arguments)
    {
        Arguments = arguments;
    }
}

/// <summary>
/// Event arguments for notification dismissal
/// </summary>
public class NotificationDismissedEventArgs : EventArgs
{
    public string Reason { get; }

    public NotificationDismissedEventArgs(string reason)
    {
        Reason = reason;
    }
}