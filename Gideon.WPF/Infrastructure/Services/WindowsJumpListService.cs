using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Windows.Shell;
using System.Windows;
using System.Diagnostics;
using System.Reflection;
using Gideon.WPF.Infrastructure.Configuration;

namespace Gideon.WPF.Infrastructure.Services;

/// <summary>
/// Service for managing Windows taskbar jump lists
/// </summary>
public class WindowsJumpListService
{
    private readonly ILogger<WindowsJumpListService> _logger;
    private readonly SystemIntegrationConfiguration _config;
    private JumpList? _jumpList;
    private bool _isInitialized;

    public WindowsJumpListService(
        ILogger<WindowsJumpListService> logger,
        IOptions<SystemIntegrationConfiguration> config)
    {
        _logger = logger;
        _config = config.Value;
    }

    /// <summary>
    /// Gets whether jump lists are supported on this system
    /// </summary>
    public bool IsSupported
    {
        get
        {
            try
            {
                // Jump lists require Windows 7 or later
                var version = Environment.OSVersion.Version;
                return version.Major >= 6 && (version.Major > 6 || version.Minor >= 1);
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Initializes the jump list service
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized || !IsSupported || !_config.JumpList.Enabled) return;

        try
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _jumpList = new JumpList();
                    _jumpList.JumpItemsRejected += OnJumpItemsRejected;
                    _jumpList.JumpItemsRemovedByUser += OnJumpItemsRemovedByUser;

                    // Create initial jump list items
                    CreateDefaultJumpListItems();

                    // Apply the jump list to the current application
                    JumpList.SetJumpList(Application.Current, _jumpList);

                    _isInitialized = true;
                    _logger.LogInformation("Windows jump list service initialized");
                });
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Windows jump list service");
        }
    }

    /// <summary>
    /// Updates the jump list with recent character information
    /// </summary>
    public void UpdateWithCharacters(IEnumerable<CharacterInfo> characters)
    {
        if (!_isInitialized || _jumpList == null) return;

        try
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Remove existing character items
                var itemsToRemove = _jumpList.JumpItems
                    .Where(item => item.CustomCategory == "Characters")
                    .ToList();

                foreach (var item in itemsToRemove)
                {
                    _jumpList.JumpItems.Remove(item);
                }

                // Add character jump items
                foreach (var character in characters.Take(5)) // Limit to 5 recent characters
                {
                    var jumpTask = new JumpTask
                    {
                        Title = character.Name,
                        Description = $"Switch to {character.Name} ({character.Corporation})",
                        ApplicationPath = GetApplicationPath(),
                        Arguments = $"--character {character.Id}",
                        CustomCategory = "Characters",
                        IconResourcePath = GetApplicationPath(),
                        IconResourceIndex = 0
                    };

                    _jumpList.JumpItems.Add(jumpTask);
                }

                // Apply the updated jump list
                _jumpList.Apply();

                _logger.LogDebug("Jump list updated with {Count} characters", characters.Count());
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update jump list with characters");
        }
    }

    /// <summary>
    /// Updates the jump list with recent fittings
    /// </summary>
    public void UpdateWithRecentFittings(IEnumerable<FittingInfo> fittings)
    {
        if (!_isInitialized || _jumpList == null) return;

        try
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Remove existing fitting items
                var itemsToRemove = _jumpList.JumpItems
                    .Where(item => item.CustomCategory == "Recent Fittings")
                    .ToList();

                foreach (var item in itemsToRemove)
                {
                    _jumpList.JumpItems.Remove(item);
                }

                // Add recent fitting jump items
                foreach (var fitting in fittings.Take(5)) // Limit to 5 recent fittings
                {
                    var jumpTask = new JumpTask
                    {
                        Title = fitting.Name,
                        Description = $"Open {fitting.ShipName} fitting: {fitting.Name}",
                        ApplicationPath = GetApplicationPath(),
                        Arguments = $"--fitting {fitting.Id}",
                        CustomCategory = "Recent Fittings",
                        IconResourcePath = GetApplicationPath(),
                        IconResourceIndex = 1
                    };

                    _jumpList.JumpItems.Add(jumpTask);
                }

                // Apply the updated jump list
                _jumpList.Apply();

                _logger.LogDebug("Jump list updated with {Count} recent fittings", fittings.Count());
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update jump list with recent fittings");
        }
    }

    /// <summary>
    /// Adds a custom jump list task
    /// </summary>
    public void AddCustomTask(string title, string description, string arguments, string category = "Tasks")
    {
        if (!_isInitialized || _jumpList == null) return;

        try
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var jumpTask = new JumpTask
                {
                    Title = title,
                    Description = description,
                    ApplicationPath = GetApplicationPath(),
                    Arguments = arguments,
                    CustomCategory = category,
                    IconResourcePath = GetApplicationPath(),
                    IconResourceIndex = 0
                };

                _jumpList.JumpItems.Add(jumpTask);
                _jumpList.Apply();

                _logger.LogDebug("Added custom jump list task: {Title}", title);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add custom jump list task: {Title}", title);
        }
    }

    /// <summary>
    /// Removes all jump list items from a specific category
    /// </summary>
    public void ClearCategory(string category)
    {
        if (!_isInitialized || _jumpList == null) return;

        try
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var itemsToRemove = _jumpList.JumpItems
                    .Where(item => item.CustomCategory == category)
                    .ToList();

                foreach (var item in itemsToRemove)
                {
                    _jumpList.JumpItems.Remove(item);
                }

                _jumpList.Apply();

                _logger.LogDebug("Cleared jump list category: {Category}", category);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear jump list category: {Category}", category);
        }
    }

    /// <summary>
    /// Updates taskbar progress state
    /// </summary>
    public void UpdateTaskbarProgress(TaskbarProgressState state, double value = 0.0)
    {
        if (!_isInitialized) return;

        try
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow != null)
                {
                    var taskbarItemInfo = mainWindow.TaskbarItemInfo ?? new TaskbarItemInfo();
                    
                    taskbarItemInfo.ProgressState = state switch
                    {
                        TaskbarProgressState.Normal => System.Windows.Shell.TaskbarItemProgressState.Normal,
                        TaskbarProgressState.Indeterminate => System.Windows.Shell.TaskbarItemProgressState.Indeterminate,
                        TaskbarProgressState.Error => System.Windows.Shell.TaskbarItemProgressState.Error,
                        TaskbarProgressState.Paused => System.Windows.Shell.TaskbarItemProgressState.Paused,
                        _ => System.Windows.Shell.TaskbarItemProgressState.None
                    };

                    if (state == TaskbarProgressState.Normal)
                    {
                        taskbarItemInfo.ProgressValue = Math.Max(0.0, Math.Min(1.0, value));
                    }

                    mainWindow.TaskbarItemInfo = taskbarItemInfo;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update taskbar progress");
        }
    }

    /// <summary>
    /// Adds taskbar thumbnail buttons
    /// </summary>
    public void SetupThumbnailButtons()
    {
        if (!_isInitialized) return;

        try
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow != null)
                {
                    var taskbarItemInfo = mainWindow.TaskbarItemInfo ?? new TaskbarItemInfo();

                    // Create thumbnail buttons
                    var thumbButtonCollection = new ThumbButtonInfoCollection();

                    // Ship Fitting button
                    var fittingButton = new ThumbButtonInfo
                    {
                        Description = "Ship Fitting",
                        ImageSource = CreateButtonIcon("🚀"),
                        Command = new RelayCommand(() => OnThumbnailButtonClicked("ShipFitting"))
                    };
                    thumbButtonCollection.Add(fittingButton);

                    // Market Analysis button
                    var marketButton = new ThumbButtonInfo
                    {
                        Description = "Market Analysis",
                        ImageSource = CreateButtonIcon("📈"),
                        Command = new RelayCommand(() => OnThumbnailButtonClicked("MarketAnalysis"))
                    };
                    thumbButtonCollection.Add(marketButton);

                    // Character Planning button
                    var characterButton = new ThumbButtonInfo
                    {
                        Description = "Character Planning",
                        ImageSource = CreateButtonIcon("👤"),
                        Command = new RelayCommand(() => OnThumbnailButtonClicked("CharacterPlanning"))
                    };
                    thumbButtonCollection.Add(characterButton);

                    taskbarItemInfo.ThumbButtonInfos = thumbButtonCollection;
                    mainWindow.TaskbarItemInfo = taskbarItemInfo;

                    _logger.LogDebug("Taskbar thumbnail buttons configured");
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to setup taskbar thumbnail buttons");
        }
    }

    /// <summary>
    /// Event fired when a thumbnail button is clicked
    /// </summary>
    public event EventHandler<ThumbnailButtonClickedEventArgs>? ThumbnailButtonClicked;

    private void CreateDefaultJumpListItems()
    {
        // Quick actions category
        var shipFittingTask = new JumpTask
        {
            Title = "Ship Fitting",
            Description = "Open Ship Fitting module",
            ApplicationPath = GetApplicationPath(),
            Arguments = "--module shipfitting",
            CustomCategory = "Quick Actions",
            IconResourcePath = GetApplicationPath(),
            IconResourceIndex = 0
        };
        _jumpList!.JumpItems.Add(shipFittingTask);

        var marketAnalysisTask = new JumpTask
        {
            Title = "Market Analysis",
            Description = "Open Market Analysis module",
            ApplicationPath = GetApplicationPath(),
            Arguments = "--module market",
            CustomCategory = "Quick Actions",
            IconResourcePath = GetApplicationPath(),
            IconResourceIndex = 0
        };
        _jumpList.JumpItems.Add(marketAnalysisTask);

        var characterPlanningTask = new JumpTask
        {
            Title = "Character Planning",
            Description = "Open Character Planning module",
            ApplicationPath = GetApplicationPath(),
            Arguments = "--module character",
            CustomCategory = "Quick Actions",
            IconResourcePath = GetApplicationPath(),
            IconResourceIndex = 0
        };
        _jumpList.JumpItems.Add(characterPlanningTask);

        // Settings task
        var settingsTask = new JumpTask
        {
            Title = "Settings",
            Description = "Open Gideon Settings",
            ApplicationPath = GetApplicationPath(),
            Arguments = "--settings",
            CustomCategory = "Tools",
            IconResourcePath = GetApplicationPath(),
            IconResourceIndex = 0
        };
        _jumpList.JumpItems.Add(settingsTask);
    }

    private string GetApplicationPath()
    {
        try
        {
            return Process.GetCurrentProcess().MainModule?.FileName ?? Assembly.GetExecutingAssembly().Location;
        }
        catch
        {
            return Assembly.GetExecutingAssembly().Location;
        }
    }

    private System.Windows.Media.ImageSource? CreateButtonIcon(string emoji)
    {
        try
        {
            // For now, return null - in a real implementation, this would create
            // an icon from the emoji or load from resources
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create button icon for emoji: {Emoji}", emoji);
            return null;
        }
    }

    private void OnThumbnailButtonClicked(string action)
    {
        ThumbnailButtonClicked?.Invoke(this, new ThumbnailButtonClickedEventArgs(action));
    }

    private void OnJumpItemsRejected(object? sender, JumpItemsRejectedEventArgs e)
    {
        _logger.LogWarning("Jump list items rejected: {Count} items", e.RejectedItems.Count);
        foreach (var item in e.RejectedItems)
        {
            _logger.LogDebug("Rejected jump item: {Item}", item);
        }
    }

    private void OnJumpItemsRemovedByUser(object? sender, JumpItemsRemovedEventArgs e)
    {
        _logger.LogDebug("Jump list items removed by user: {Count} items", e.RemovedItems.Count);
        foreach (var item in e.RemovedItems)
        {
            _logger.LogDebug("Removed jump item: {Item}", item);
        }
    }

    public void Dispose()
    {
        try
        {
            if (_jumpList != null)
            {
                _jumpList.JumpItemsRejected -= OnJumpItemsRejected;
                _jumpList.JumpItemsRemovedByUser -= OnJumpItemsRemovedByUser;
                _jumpList = null;
            }

            _logger.LogDebug("Windows jump list service disposed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing jump list service");
        }
    }
}

/// <summary>
/// Information about a ship fitting for jump list
/// </summary>
public class FittingInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShipName { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
}

/// <summary>
/// Taskbar progress states
/// </summary>
public enum TaskbarProgressState
{
    None,
    Normal,
    Indeterminate,
    Error,
    Paused
}

/// <summary>
/// Event arguments for thumbnail button clicks
/// </summary>
public class ThumbnailButtonClickedEventArgs : EventArgs
{
    public string Action { get; }

    public ThumbnailButtonClickedEventArgs(string action)
    {
        Action = action;
    }
}

/// <summary>
/// Simple relay command implementation for thumbnail buttons
/// </summary>
public class RelayCommand : System.Windows.Input.ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add { System.Windows.Input.CommandManager.RequerySuggested += value; }
        remove { System.Windows.Input.CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => _execute();
}