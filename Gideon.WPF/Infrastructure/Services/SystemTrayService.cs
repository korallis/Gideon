using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;
using Gideon.WPF.Infrastructure.Configuration;

namespace Gideon.WPF.Infrastructure.Services;

/// <summary>
/// Service for managing system tray icon and functionality
/// </summary>
public class SystemTrayService : IDisposable
{
    private readonly ILogger<SystemTrayService> _logger;
    private readonly SystemIntegrationConfiguration _config;
    private readonly NotificationManager _notificationManager;
    private NotifyIcon? _notifyIcon;
    private ContextMenuStrip? _contextMenu;
    private bool _isInitialized;

    public SystemTrayService(
        ILogger<SystemTrayService> logger,
        IOptions<SystemIntegrationConfiguration> config,
        NotificationManager notificationManager)
    {
        _logger = logger;
        _config = config.Value;
        _notificationManager = notificationManager;
    }

    /// <summary>
    /// Event fired when the system tray icon is clicked
    /// </summary>
    public event EventHandler? TrayIconClicked;

    /// <summary>
    /// Event fired when the system tray icon is double-clicked
    /// </summary>
    public event EventHandler? TrayIconDoubleClicked;

    /// <summary>
    /// Event fired when a tray menu item is clicked
    /// </summary>
    public event EventHandler<TrayMenuItemClickedEventArgs>? TrayMenuItemClicked;

    /// <summary>
    /// Gets whether the system tray is available
    /// </summary>
    public bool IsAvailable => SystemInformation.Platform == PlatformID.Win32NT;

    /// <summary>
    /// Gets whether the tray icon is visible
    /// </summary>
    public bool IsVisible => _notifyIcon?.Visible ?? false;

    /// <summary>
    /// Initializes the system tray service
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized || !IsAvailable || !_config.SystemTray.Enabled) return;

        try
        {
            await Task.Run(() =>
            {
                // Create the NotifyIcon
                _notifyIcon = new NotifyIcon
                {
                    Icon = GetApplicationIcon(),
                    Text = "Gideon - EVE Online AI Copilot",
                    Visible = _config.SystemTray.ShowOnStartup
                };

                // Set up event handlers
                _notifyIcon.Click += OnTrayIconClick;
                _notifyIcon.DoubleClick += OnTrayIconDoubleClick;
                _notifyIcon.MouseUp += OnTrayIconMouseUp;

                // Create context menu
                CreateContextMenu();

                _isInitialized = true;
                _logger.LogInformation("System tray service initialized");
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize system tray service");
        }
    }

    /// <summary>
    /// Shows the system tray icon
    /// </summary>
    public void ShowTrayIcon()
    {
        if (_notifyIcon != null && _isInitialized && _config.SystemTray.Enabled)
        {
            _notifyIcon.Visible = true;
            _logger.LogDebug("System tray icon shown");
        }
    }

    /// <summary>
    /// Hides the system tray icon
    /// </summary>
    public void HideTrayIcon()
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _logger.LogDebug("System tray icon hidden");
        }
    }

    /// <summary>
    /// Updates the tray icon tooltip text
    /// </summary>
    public void UpdateTooltip(string text)
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.Text = text.Length > 63 ? text.Substring(0, 60) + "..." : text;
            _logger.LogDebug("Tray icon tooltip updated: {Text}", text);
        }
    }

    /// <summary>
    /// Updates the tray icon image
    /// </summary>
    public void UpdateIcon(Icon icon)
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.Icon = icon;
            _logger.LogDebug("Tray icon image updated");
        }
    }

    /// <summary>
    /// Shows a balloon tip notification
    /// </summary>
    public void ShowBalloonTip(string title, string text, ToolTipIcon icon = ToolTipIcon.Info, int timeout = 5000)
    {
        if (_notifyIcon != null && IsVisible)
        {
            _notifyIcon.ShowBalloonTip(timeout, title, text, icon);
            _logger.LogDebug("Balloon tip shown: {Title} - {Text}", title, text);
        }
    }

    /// <summary>
    /// Updates the context menu with character information
    /// </summary>
    public void UpdateContextMenuWithCharacters(IEnumerable<CharacterInfo> characters)
    {
        if (_contextMenu == null) return;

        try
        {
            // Find and update the characters submenu
            var charactersMenuItem = _contextMenu.Items.Cast<ToolStripItem>()
                .FirstOrDefault(item => item.Name == "CharactersMenuItem") as ToolStripMenuItem;

            if (charactersMenuItem != null)
            {
                // Clear existing character items (keep the separator and "No characters" item)
                var itemsToRemove = charactersMenuItem.DropDownItems.Cast<ToolStripItem>()
                    .Where(item => item.Tag is CharacterInfo)
                    .ToList();

                foreach (var item in itemsToRemove)
                {
                    charactersMenuItem.DropDownItems.Remove(item);
                }

                // Add character items
                if (characters.Any())
                {
                    foreach (var character in characters)
                    {
                        var characterItem = new ToolStripMenuItem
                        {
                            Name = $"Character_{character.Id}",
                            Text = character.Name,
                            Tag = character,
                            ToolTipText = $"Switch to {character.Name}"
                        };

                        characterItem.Click += (sender, e) =>
                        {
                            TrayMenuItemClicked?.Invoke(this, new TrayMenuItemClickedEventArgs("SwitchCharacter", character));
                        };

                        charactersMenuItem.DropDownItems.Add(characterItem);
                    }

                    // Update "No characters" item visibility
                    var noCharactersItem = charactersMenuItem.DropDownItems.Cast<ToolStripItem>()
                        .FirstOrDefault(item => item.Name == "NoCharactersItem");
                    
                    if (noCharactersItem != null)
                    {
                        noCharactersItem.Visible = false;
                    }
                }
                else
                {
                    // Show "No characters" item
                    var noCharactersItem = charactersMenuItem.DropDownItems.Cast<ToolStripItem>()
                        .FirstOrDefault(item => item.Name == "NoCharactersItem");
                    
                    if (noCharactersItem != null)
                    {
                        noCharactersItem.Visible = true;
                    }
                }
            }

            _logger.LogDebug("Context menu updated with {Count} characters", characters.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update context menu with characters");
        }
    }

    /// <summary>
    /// Updates the context menu with application status
    /// </summary>
    public void UpdateContextMenuStatus(string status, bool isConnected)
    {
        if (_contextMenu == null) return;

        try
        {
            var statusMenuItem = _contextMenu.Items.Cast<ToolStripItem>()
                .FirstOrDefault(item => item.Name == "StatusMenuItem");

            if (statusMenuItem != null)
            {
                statusMenuItem.Text = $"Status: {status}";
                
                // Update icon based on connection status
                if (isConnected)
                {
                    statusMenuItem.Image = SystemIcons.Information.ToBitmap();
                }
                else
                {
                    statusMenuItem.Image = SystemIcons.Warning.ToBitmap();
                }
            }

            _logger.LogDebug("Context menu status updated: {Status} (Connected: {Connected})", status, isConnected);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update context menu status");
        }
    }

    private void CreateContextMenu()
    {
        _contextMenu = new ContextMenuStrip();

        // Application status
        var statusItem = new ToolStripMenuItem
        {
            Name = "StatusMenuItem",
            Text = "Status: Starting...",
            Enabled = false
        };
        _contextMenu.Items.Add(statusItem);

        _contextMenu.Items.Add(new ToolStripSeparator());

        // Show/Hide main window
        var showHideItem = new ToolStripMenuItem
        {
            Name = "ShowHideMenuItem",
            Text = "Show Gideon"
        };
        showHideItem.Click += (sender, e) => TrayMenuItemClicked?.Invoke(this, new TrayMenuItemClickedEventArgs("ShowHide", null));
        _contextMenu.Items.Add(showHideItem);

        _contextMenu.Items.Add(new ToolStripSeparator());

        // Characters submenu
        var charactersItem = new ToolStripMenuItem
        {
            Name = "CharactersMenuItem",
            Text = "Characters"
        };

        var noCharactersItem = new ToolStripMenuItem
        {
            Name = "NoCharactersItem",
            Text = "No characters authenticated",
            Enabled = false
        };
        charactersItem.DropDownItems.Add(noCharactersItem);

        _contextMenu.Items.Add(charactersItem);

        _contextMenu.Items.Add(new ToolStripSeparator());

        // Quick actions
        var shipFittingItem = new ToolStripMenuItem
        {
            Name = "ShipFittingMenuItem",
            Text = "Ship Fitting"
        };
        shipFittingItem.Click += (sender, e) => TrayMenuItemClicked?.Invoke(this, new TrayMenuItemClickedEventArgs("ShipFitting", null));
        _contextMenu.Items.Add(shipFittingItem);

        var marketAnalysisItem = new ToolStripMenuItem
        {
            Name = "MarketAnalysisMenuItem",
            Text = "Market Analysis"
        };
        marketAnalysisItem.Click += (sender, e) => TrayMenuItemClicked?.Invoke(this, new TrayMenuItemClickedEventArgs("MarketAnalysis", null));
        _contextMenu.Items.Add(marketAnalysisItem);

        var characterPlanningItem = new ToolStripMenuItem
        {
            Name = "CharacterPlanningMenuItem",
            Text = "Character Planning"
        };
        characterPlanningItem.Click += (sender, e) => TrayMenuItemClicked?.Invoke(this, new TrayMenuItemClickedEventArgs("CharacterPlanning", null));
        _contextMenu.Items.Add(characterPlanningItem);

        _contextMenu.Items.Add(new ToolStripSeparator());

        // Settings
        var settingsItem = new ToolStripMenuItem
        {
            Name = "SettingsMenuItem",
            Text = "Settings"
        };
        settingsItem.Click += (sender, e) => TrayMenuItemClicked?.Invoke(this, new TrayMenuItemClickedEventArgs("Settings", null));
        _contextMenu.Items.Add(settingsItem);

        _contextMenu.Items.Add(new ToolStripSeparator());

        // Exit
        var exitItem = new ToolStripMenuItem
        {
            Name = "ExitMenuItem",
            Text = "Exit Gideon"
        };
        exitItem.Click += (sender, e) => TrayMenuItemClicked?.Invoke(this, new TrayMenuItemClickedEventArgs("Exit", null));
        _contextMenu.Items.Add(exitItem);

        _notifyIcon!.ContextMenuStrip = _contextMenu;
    }

    private Icon GetApplicationIcon()
    {
        try
        {
            // Try to get the application icon from resources
            var assembly = Assembly.GetExecutingAssembly();
            var iconStream = assembly.GetManifestResourceStream("Gideon.WPF.Resources.gideon.ico");
            
            if (iconStream != null)
            {
                return new Icon(iconStream);
            }

            // Fallback to system default icon
            return SystemIcons.Application;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load application icon, using default");
            return SystemIcons.Application;
        }
    }

    private void OnTrayIconClick(object? sender, EventArgs e)
    {
        try
        {
            if (e is MouseEventArgs mouseArgs && mouseArgs.Button == MouseButtons.Left)
            {
                TrayIconClicked?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling tray icon click");
        }
    }

    private void OnTrayIconDoubleClick(object? sender, EventArgs e)
    {
        try
        {
            TrayIconDoubleClicked?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling tray icon double-click");
        }
    }

    private void OnTrayIconMouseUp(object? sender, MouseEventArgs e)
    {
        try
        {
            if (e.Button == MouseButtons.Right)
            {
                // Context menu will show automatically
                _logger.LogDebug("Tray icon right-clicked, showing context menu");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling tray icon mouse up");
        }
    }

    public void Dispose()
    {
        try
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }

            _contextMenu?.Dispose();
            _contextMenu = null;

            _logger.LogDebug("System tray service disposed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing system tray service");
        }
    }
}

/// <summary>
/// Character information for tray menu
/// </summary>
public class CharacterInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Corporation { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public DateTime LastLogin { get; set; }
}

/// <summary>
/// Event arguments for tray menu item clicks
/// </summary>
public class TrayMenuItemClickedEventArgs : EventArgs
{
    public string Action { get; }
    public object? Data { get; }

    public TrayMenuItemClickedEventArgs(string action, object? data)
    {
        Action = action;
        Data = data;
    }
}