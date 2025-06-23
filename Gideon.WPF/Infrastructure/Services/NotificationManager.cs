using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Gideon.WPF.Infrastructure.Configuration;

namespace Gideon.WPF.Infrastructure.Services;

/// <summary>
/// High-level notification manager that coordinates different types of notifications
/// </summary>
public class NotificationManager
{
    private readonly WindowsNotificationService _notificationService;
    private readonly ILogger<NotificationManager> _logger;
    private readonly NotificationConfiguration _config;
    private readonly Dictionary<string, DateTime> _lastNotificationTimes = new();

    public NotificationManager(
        WindowsNotificationService notificationService,
        ILogger<NotificationManager> logger,
        IOptions<NotificationConfiguration> config)
    {
        _notificationService = notificationService;
        _logger = logger;
        _config = config.Value;
    }

    /// <summary>
    /// Initializes the notification manager
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            await _notificationService.InitializeAsync();

            // Set up event handlers
            _notificationService.NotificationActivated += OnNotificationActivated;
            _notificationService.NotificationDismissed += OnNotificationDismissed;

            _logger.LogInformation("Notification manager initialized");

            // Show startup notification if enabled
            if (_config.System.EnableStartup)
            {
                await ShowSystemNotificationAsync("Gideon Started", "EVE Online Toolkit is ready to assist you");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize notification manager");
        }
    }

    /// <summary>
    /// Shows a character-related notification with throttling
    /// </summary>
    public async Task ShowCharacterNotificationAsync(string characterName, string message, string? imageUrl = null)
    {
        if (!_config.EnableCharacterNotifications || !_config.Character.EnableStatusChanges) return;

        var key = $"character_{characterName}";
        if (ShouldThrottleNotification(key, TimeSpan.FromMinutes(5))) return;

        try
        {
            await _notificationService.ShowCharacterNotificationAsync(characterName, message, imageUrl);
            _lastNotificationTimes[key] = DateTime.UtcNow;
            _logger.LogDebug("Showed character notification for {Character}", characterName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show character notification for {Character}", characterName);
        }
    }

    /// <summary>
    /// Shows a skill training completion notification
    /// </summary>
    public async Task ShowSkillCompletedNotificationAsync(string skillName, string characterName, int skillLevel = 0)
    {
        if (!_config.EnableSkillNotifications || !_config.Skill.EnableSkillCompletion) return;

        try
        {
            var message = skillLevel > 0 
                ? $"{skillName} Level {skillLevel} training completed for {characterName}"
                : $"{skillName} training completed for {characterName}";

            await _notificationService.ShowSkillNotificationAsync(skillName, characterName);
            _logger.LogDebug("Showed skill completion notification: {Skill} for {Character}", skillName, characterName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show skill completion notification: {Skill}", skillName);
        }
    }

    /// <summary>
    /// Shows a skill queue warning notification
    /// </summary>
    public async Task ShowSkillQueueWarningAsync(string characterName, int hoursRemaining)
    {
        if (!_config.EnableSkillNotifications || !_config.Skill.EnableSkillQueue) return;
        if (hoursRemaining > _config.Skill.NotifyQueueHoursRemaining) return;

        var key = $"skillqueue_{characterName}";
        if (ShouldThrottleNotification(key, TimeSpan.FromHours(2))) return;

        try
        {
            var message = hoursRemaining <= 0 
                ? $"Skill queue is empty for {characterName}"
                : $"Skill queue for {characterName} has only {hoursRemaining} hours remaining";

            await _notificationService.ShowSkillNotificationAsync("Skill Queue Warning", characterName);
            _lastNotificationTimes[key] = DateTime.UtcNow;
            _logger.LogDebug("Showed skill queue warning for {Character}: {Hours} hours remaining", characterName, hoursRemaining);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show skill queue warning for {Character}", characterName);
        }
    }

    /// <summary>
    /// Shows a market price alert notification
    /// </summary>
    public async Task ShowMarketAlertAsync(string itemName, decimal currentPrice, decimal targetPrice, string region)
    {
        if (!_config.EnableMarketNotifications || !_config.Market.EnablePriceAlerts) return;

        var priceChange = Math.Abs((currentPrice - targetPrice) / targetPrice * 100);
        if (priceChange < _config.Market.MinPriceChangePercent) return;

        var key = $"market_{itemName}_{region}";
        if (ShouldThrottleNotification(key, TimeSpan.FromMinutes(30))) return;

        try
        {
            await _notificationService.ShowMarketNotificationAsync(itemName, currentPrice, region);
            _lastNotificationTimes[key] = DateTime.UtcNow;
            _logger.LogDebug("Showed market alert for {Item} in {Region}: {Price:N2} ISK", itemName, region, currentPrice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show market alert for {Item}", itemName);
        }
    }

    /// <summary>
    /// Shows a market order completion notification
    /// </summary>
    public async Task ShowMarketOrderCompletionAsync(string orderType, string itemName, int quantity, decimal totalValue)
    {
        if (!_config.EnableMarketNotifications || !_config.Market.EnableOrderCompletion) return;

        try
        {
            var message = $"{orderType} order completed: {quantity:N0}x {itemName} for {totalValue:N2} ISK";
            await _notificationService.ShowMarketNotificationAsync("Order Completed", itemName, "Market");
            _logger.LogDebug("Showed market order completion: {OrderType} {Item} x{Quantity}", orderType, itemName, quantity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show market order completion notification");
        }
    }

    /// <summary>
    /// Shows a fitting-related notification
    /// </summary>
    public async Task ShowFittingNotificationAsync(string fittingName, string message, FittingNotificationType type = FittingNotificationType.General)
    {
        if (!_config.EnableFittingNotifications) return;

        var shouldShow = type switch
        {
            FittingNotificationType.SimulationComplete => _config.Fitting.EnableSimulationCompletion,
            FittingNotificationType.OptimizationSuggestion => _config.Fitting.EnableOptimizationSuggestions,
            FittingNotificationType.ValidationWarning => _config.Fitting.EnableValidationWarnings,
            FittingNotificationType.ImportExport => _config.Fitting.EnableImportExport,
            _ => true
        };

        if (!shouldShow) return;

        try
        {
            await _notificationService.ShowFittingNotificationAsync(fittingName, message);
            _logger.LogDebug("Showed fitting notification: {Fitting} - {Type}", fittingName, type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show fitting notification for {Fitting}", fittingName);
        }
    }

    /// <summary>
    /// Shows a system notification
    /// </summary>
    public async Task ShowSystemNotificationAsync(string title, string message, SystemNotificationType type = SystemNotificationType.Info)
    {
        if (!_config.EnableSystemNotifications) return;

        var shouldShow = type switch
        {
            SystemNotificationType.Startup => _config.System.EnableStartup,
            SystemNotificationType.Update => _config.System.EnableUpdates,
            SystemNotificationType.Error => _config.System.EnableErrors,
            SystemNotificationType.ConnectionStatus => _config.System.EnableConnectionStatus,
            SystemNotificationType.PerformanceWarning => _config.System.EnablePerformanceWarnings,
            _ => true
        };

        if (!shouldShow) return;

        // Check quiet hours for non-critical notifications
        if (IsQuietHours() && _config.System.Priority != NotificationPriority.Critical)
        {
            if (!_config.QuietHours.AllowHighPriority || _config.System.Priority != NotificationPriority.High)
            {
                return;
            }
        }

        try
        {
            var notificationType = type switch
            {
                SystemNotificationType.Error => NotificationType.Error,
                SystemNotificationType.Update => NotificationType.Info,
                SystemNotificationType.PerformanceWarning => NotificationType.Warning,
                _ => NotificationType.Info
            };

            await _notificationService.ShowNotificationAsync(title, message, notificationType);
            _logger.LogDebug("Showed system notification: {Title} - {Type}", title, type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show system notification: {Title}", title);
        }
    }

    /// <summary>
    /// Shows an error notification
    /// </summary>
    public async Task ShowErrorNotificationAsync(string title, string message, Exception? exception = null)
    {
        if (!_config.System.EnableErrors) return;

        try
        {
            var errorMessage = exception != null 
                ? $"{message}\n\nError: {exception.Message}"
                : message;

            await _notificationService.ShowNotificationAsync(title, errorMessage, NotificationType.Error);
            _logger.LogDebug("Showed error notification: {Title}", title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show error notification: {Title}", title);
        }
    }

    /// <summary>
    /// Shows a connection status notification
    /// </summary>
    public async Task ShowConnectionStatusNotificationAsync(bool isConnected, string? details = null)
    {
        if (!_config.System.EnableConnectionStatus) return;

        var key = "connection_status";
        if (ShouldThrottleNotification(key, TimeSpan.FromMinutes(1))) return;

        try
        {
            var title = isConnected ? "Connected" : "Connection Lost";
            var message = details ?? (isConnected ? "Successfully connected to EVE Online services" : "Lost connection to EVE Online services");
            var type = isConnected ? NotificationType.Success : NotificationType.Warning;

            await _notificationService.ShowNotificationAsync(title, message, type);
            _lastNotificationTimes[key] = DateTime.UtcNow;
            _logger.LogDebug("Showed connection status notification: {Connected}", isConnected);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show connection status notification");
        }
    }

    /// <summary>
    /// Clears all notifications
    /// </summary>
    public void ClearAllNotifications()
    {
        try
        {
            _notificationService.ClearAllNotifications();
            _logger.LogDebug("Cleared all notifications");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear all notifications");
        }
    }

    private bool ShouldThrottleNotification(string key, TimeSpan throttleInterval)
    {
        if (!_lastNotificationTimes.TryGetValue(key, out var lastTime))
            return false;

        return DateTime.UtcNow - lastTime < throttleInterval;
    }

    private bool IsQuietHours()
    {
        if (!_config.QuietHours.Enabled) return false;

        var now = DateTime.Now.TimeOfDay;
        var dayOfWeek = DateTime.Now.DayOfWeek;

        if (!_config.QuietHours.ApplicableDays.Contains(dayOfWeek))
            return false;

        var startTime = _config.QuietHours.StartTime;
        var endTime = _config.QuietHours.EndTime;

        // Handle quiet hours that span midnight
        if (startTime > endTime)
        {
            return now >= startTime || now <= endTime;
        }
        else
        {
            return now >= startTime && now <= endTime;
        }
    }

    private void OnNotificationActivated(object? sender, NotificationActivatedEventArgs e)
    {
        try
        {
            _logger.LogDebug("Notification activated: {Arguments}", e.Arguments);

            // Handle notification activation based on arguments
            if (e.Arguments.StartsWith("character://"))
            {
                // Handle character-related actions
                HandleCharacterAction(e.Arguments);
            }
            else if (e.Arguments.StartsWith("market://"))
            {
                // Handle market-related actions
                HandleMarketAction(e.Arguments);
            }
            else if (e.Arguments.StartsWith("fitting://"))
            {
                // Handle fitting-related actions
                HandleFittingAction(e.Arguments);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling notification activation");
        }
    }

    private void OnNotificationDismissed(object? sender, NotificationDismissedEventArgs e)
    {
        _logger.LogDebug("Notification dismissed: {Reason}", e.Reason);
    }

    private void HandleCharacterAction(string arguments)
    {
        // TODO: Implement character action handling
        _logger.LogDebug("Handling character action: {Arguments}", arguments);
    }

    private void HandleMarketAction(string arguments)
    {
        // TODO: Implement market action handling
        _logger.LogDebug("Handling market action: {Arguments}", arguments);
    }

    private void HandleFittingAction(string arguments)
    {
        // TODO: Implement fitting action handling
        _logger.LogDebug("Handling fitting action: {Arguments}", arguments);
    }

    public void Dispose()
    {
        if (_notificationService != null)
        {
            _notificationService.NotificationActivated -= OnNotificationActivated;
            _notificationService.NotificationDismissed -= OnNotificationDismissed;
        }
    }
}

/// <summary>
/// Types of fitting notifications
/// </summary>
public enum FittingNotificationType
{
    General,
    SimulationComplete,
    OptimizationSuggestion,
    ValidationWarning,
    ImportExport
}

/// <summary>
/// Types of system notifications
/// </summary>
public enum SystemNotificationType
{
    Info,
    Startup,
    Update,
    Error,
    ConnectionStatus,
    PerformanceWarning
}