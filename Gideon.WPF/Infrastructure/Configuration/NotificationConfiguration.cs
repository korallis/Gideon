using System.ComponentModel.DataAnnotations;

namespace Gideon.WPF.Infrastructure.Configuration;

/// <summary>
/// Configuration options for Windows notification system
/// </summary>
public class NotificationConfiguration
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "Notifications";

    /// <summary>
    /// Enable notification system
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Enable character-related notifications
    /// </summary>
    public bool EnableCharacterNotifications { get; set; } = true;

    /// <summary>
    /// Enable market-related notifications
    /// </summary>
    public bool EnableMarketNotifications { get; set; } = true;

    /// <summary>
    /// Enable skill training notifications
    /// </summary>
    public bool EnableSkillNotifications { get; set; } = true;

    /// <summary>
    /// Enable fitting-related notifications
    /// </summary>
    public bool EnableFittingNotifications { get; set; } = true;

    /// <summary>
    /// Enable system notifications (updates, errors, etc.)
    /// </summary>
    public bool EnableSystemNotifications { get; set; } = true;

    /// <summary>
    /// Enable sound for notifications
    /// </summary>
    public bool EnableSound { get; set; } = true;

    /// <summary>
    /// Default notification expiration time in minutes
    /// </summary>
    [Range(1, 1440)]
    public int DefaultExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Maximum number of notifications to show simultaneously
    /// </summary>
    [Range(1, 10)]
    public int MaxSimultaneousNotifications { get; set; } = 3;

    /// <summary>
    /// Notification display duration for different types
    /// </summary>
    public NotificationDurationSettings Duration { get; set; } = new();

    /// <summary>
    /// Character notification settings
    /// </summary>
    public CharacterNotificationSettings Character { get; set; } = new();

    /// <summary>
    /// Market notification settings
    /// </summary>
    public MarketNotificationSettings Market { get; set; } = new();

    /// <summary>
    /// Skill notification settings
    /// </summary>
    public SkillNotificationSettings Skill { get; set; } = new();

    /// <summary>
    /// Fitting notification settings
    /// </summary>
    public FittingNotificationSettings Fitting { get; set; } = new();

    /// <summary>
    /// System notification settings
    /// </summary>
    public SystemNotificationSettings System { get; set; } = new();

    /// <summary>
    /// Quiet hours settings
    /// </summary>
    public QuietHoursSettings QuietHours { get; set; } = new();
}

/// <summary>
/// Notification duration settings for different types
/// </summary>
public class NotificationDurationSettings
{
    /// <summary>
    /// Duration for info notifications (seconds)
    /// </summary>
    [Range(1, 300)]
    public int InfoSeconds { get; set; } = 10;

    /// <summary>
    /// Duration for success notifications (seconds)
    /// </summary>
    [Range(1, 300)]
    public int SuccessSeconds { get; set; } = 5;

    /// <summary>
    /// Duration for warning notifications (seconds)
    /// </summary>
    [Range(1, 300)]
    public int WarningSeconds { get; set; } = 15;

    /// <summary>
    /// Duration for error notifications (seconds)
    /// </summary>
    [Range(1, 300)]
    public int ErrorSeconds { get; set; } = 30;

    /// <summary>
    /// Duration for character notifications (seconds)
    /// </summary>
    [Range(1, 300)]
    public int CharacterSeconds { get; set; } = 20;

    /// <summary>
    /// Duration for market notifications (seconds)
    /// </summary>
    [Range(1, 300)]
    public int MarketSeconds { get; set; } = 25;

    /// <summary>
    /// Duration for skill notifications (seconds)
    /// </summary>
    [Range(1, 300)]
    public int SkillSeconds { get; set; } = 30;

    /// <summary>
    /// Duration for fitting notifications (seconds)
    /// </summary>
    [Range(1, 300)]
    public int FittingSeconds { get; set; } = 15;
}

/// <summary>
/// Character notification settings
/// </summary>
public class CharacterNotificationSettings
{
    /// <summary>
    /// Enable character login/logout notifications
    /// </summary>
    public bool EnableLoginLogout { get; set; } = false;

    /// <summary>
    /// Enable character status change notifications
    /// </summary>
    public bool EnableStatusChanges { get; set; } = true;

    /// <summary>
    /// Enable character attribute changes
    /// </summary>
    public bool EnableAttributeChanges { get; set; } = true;

    /// <summary>
    /// Enable character location changes
    /// </summary>
    public bool EnableLocationChanges { get; set; } = false;

    /// <summary>
    /// Show character portrait in notifications
    /// </summary>
    public bool ShowPortrait { get; set; } = true;

    /// <summary>
    /// Character notification priority
    /// </summary>
    public NotificationPriority Priority { get; set; } = NotificationPriority.Medium;
}

/// <summary>
/// Market notification settings
/// </summary>
public class MarketNotificationSettings
{
    /// <summary>
    /// Enable price alert notifications
    /// </summary>
    public bool EnablePriceAlerts { get; set; } = true;

    /// <summary>
    /// Enable order completion notifications
    /// </summary>
    public bool EnableOrderCompletion { get; set; } = true;

    /// <summary>
    /// Enable order expiration notifications
    /// </summary>
    public bool EnableOrderExpiration { get; set; } = true;

    /// <summary>
    /// Minimum price change percentage to trigger notification
    /// </summary>
    [Range(0.1, 100.0)]
    public double MinPriceChangePercent { get; set; } = 5.0;

    /// <summary>
    /// Show item images in market notifications
    /// </summary>
    public bool ShowItemImages { get; set; } = true;

    /// <summary>
    /// Market notification priority
    /// </summary>
    public NotificationPriority Priority { get; set; } = NotificationPriority.Medium;
}

/// <summary>
/// Skill notification settings
/// </summary>
public class SkillNotificationSettings
{
    /// <summary>
    /// Enable skill completion notifications
    /// </summary>
    public bool EnableSkillCompletion { get; set; } = true;

    /// <summary>
    /// Enable skill queue notifications
    /// </summary>
    public bool EnableSkillQueue { get; set; } = true;

    /// <summary>
    /// Enable skill prerequisite notifications
    /// </summary>
    public bool EnablePrerequisites { get; set; } = true;

    /// <summary>
    /// Notify when skill queue is empty
    /// </summary>
    public bool NotifyEmptyQueue { get; set; } = true;

    /// <summary>
    /// Notify when skill queue has less than X hours remaining
    /// </summary>
    [Range(1, 168)]
    public int NotifyQueueHoursRemaining { get; set; } = 24;

    /// <summary>
    /// Show skill icons in notifications
    /// </summary>
    public bool ShowSkillIcons { get; set; } = true;

    /// <summary>
    /// Skill notification priority
    /// </summary>
    public NotificationPriority Priority { get; set; } = NotificationPriority.High;
}

/// <summary>
/// Fitting notification settings
/// </summary>
public class FittingNotificationSettings
{
    /// <summary>
    /// Enable fitting simulation completion
    /// </summary>
    public bool EnableSimulationCompletion { get; set; } = true;

    /// <summary>
    /// Enable fitting optimization suggestions
    /// </summary>
    public bool EnableOptimizationSuggestions { get; set; } = true;

    /// <summary>
    /// Enable fitting validation warnings
    /// </summary>
    public bool EnableValidationWarnings { get; set; } = true;

    /// <summary>
    /// Enable fitting import/export notifications
    /// </summary>
    public bool EnableImportExport { get; set; } = false;

    /// <summary>
    /// Show ship images in fitting notifications
    /// </summary>
    public bool ShowShipImages { get; set; } = true;

    /// <summary>
    /// Fitting notification priority
    /// </summary>
    public NotificationPriority Priority { get; set; } = NotificationPriority.Medium;
}

/// <summary>
/// System notification settings
/// </summary>
public class SystemNotificationSettings
{
    /// <summary>
    /// Enable application startup notifications
    /// </summary>
    public bool EnableStartup { get; set; } = false;

    /// <summary>
    /// Enable application update notifications
    /// </summary>
    public bool EnableUpdates { get; set; } = true;

    /// <summary>
    /// Enable error notifications
    /// </summary>
    public bool EnableErrors { get; set; } = true;

    /// <summary>
    /// Enable connection status notifications
    /// </summary>
    public bool EnableConnectionStatus { get; set; } = true;

    /// <summary>
    /// Enable performance warnings
    /// </summary>
    public bool EnablePerformanceWarnings { get; set; } = true;

    /// <summary>
    /// System notification priority
    /// </summary>
    public NotificationPriority Priority { get; set; } = NotificationPriority.High;
}

/// <summary>
/// Quiet hours settings to suppress notifications during certain times
/// </summary>
public class QuietHoursSettings
{
    /// <summary>
    /// Enable quiet hours
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Start time for quiet hours (24-hour format)
    /// </summary>
    public TimeSpan StartTime { get; set; } = new(22, 0, 0); // 10:00 PM

    /// <summary>
    /// End time for quiet hours (24-hour format)
    /// </summary>
    public TimeSpan EndTime { get; set; } = new(7, 0, 0); // 7:00 AM

    /// <summary>
    /// Allow high priority notifications during quiet hours
    /// </summary>
    public bool AllowHighPriority { get; set; } = true;

    /// <summary>
    /// Allow critical notifications during quiet hours
    /// </summary>
    public bool AllowCritical { get; set; } = true;

    /// <summary>
    /// Days of the week when quiet hours apply
    /// </summary>
    public List<DayOfWeek> ApplicableDays { get; set; } = new()
    {
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday,
        DayOfWeek.Saturday,
        DayOfWeek.Sunday
    };
}

/// <summary>
/// Notification priority levels
/// </summary>
public enum NotificationPriority
{
    Low,
    Medium,
    High,
    Critical
}