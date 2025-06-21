using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using WpfColor = System.Windows.Media.Color;

namespace Gideon.WPF.Infrastructure.Services;

/// <summary>
/// Service for integrating with Windows 11 theming and Fluent Design system
/// </summary>
public class WindowsThemeService
{
    private const string REGISTRY_KEY_PATH = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private const string ACCENT_COLOR_KEY = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\DWM";

    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;
    public event EventHandler<AccentColorChangedEventArgs>? AccentColorChanged;

    /// <summary>
    /// Gets whether Windows is in dark mode
    /// </summary>
    public bool IsDarkMode
    {
        get
        {
            try
            {
                var value = Registry.GetValue(REGISTRY_KEY_PATH, "AppsUseLightTheme", 1);
                return value is int intValue && intValue == 0;
            }
            catch
            {
                return false; // Default to light mode if we can't determine
            }
        }
    }

    /// <summary>
    /// Gets whether Windows uses dark mode for system elements
    /// </summary>
    public bool IsSystemDarkMode
    {
        get
        {
            try
            {
                var value = Registry.GetValue(REGISTRY_KEY_PATH, "SystemUsesLightTheme", 1);
                return value is int intValue && intValue == 0;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Gets the Windows accent color
    /// </summary>
    public WpfColor AccentColor
    {
        get
        {
            try
            {
                var value = Registry.GetValue(ACCENT_COLOR_KEY, "AccentColor", 0);
                if (value is int colorValue)
                {
                    var bytes = BitConverter.GetBytes(colorValue);
                    return WpfColor.FromRgb(bytes[0], bytes[1], bytes[2]);
                }
            }
            catch
            {
                // Ignore errors and fall back to default
            }

            // Default EVE Online cyan color
            return WpfColor.FromRgb(0x00, 0xD4, 0xFF);
        }
    }

    /// <summary>
    /// Gets whether transparency effects are enabled
    /// </summary>
    public bool IsTransparencyEnabled
    {
        get
        {
            try
            {
                var value = Registry.GetValue(REGISTRY_KEY_PATH, "EnableTransparency", 1);
                return value is int intValue && intValue == 1;
            }
            catch
            {
                return true; // Default to enabled
            }
        }
    }

    /// <summary>
    /// Initializes the theme service and starts monitoring for changes
    /// </summary>
    public void Initialize()
    {
        try
        {
            // Monitor registry changes for theme updates
            StartRegistryMonitoring();
        }
        catch (Exception ex)
        {
            // Log error but don't fail initialization
            System.Diagnostics.Debug.WriteLine($"Failed to initialize theme monitoring: {ex.Message}");
        }
    }

    /// <summary>
    /// Applies the current Windows theme to the application
    /// </summary>
    public void ApplyCurrentTheme()
    {
        try
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var resources = Application.Current.Resources;
                
                // Update theme-aware brushes
                if (IsDarkMode)
                {
                    ApplyDarkTheme(resources);
                }
                else
                {
                    ApplyLightTheme(resources);
                }

                // Update accent color
                ApplyAccentColor(resources, AccentColor);

                // Apply transparency effects if supported
                ApplyTransparencyEffects(resources, IsTransparencyEnabled);
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to apply theme: {ex.Message}");
        }
    }

    private void ApplyDarkTheme(ResourceDictionary resources)
    {
        // Update EVE theme colors for dark mode
        resources["EVE_BackgroundBrush"] = new SolidColorBrush(WpfColor.FromRgb(0x1C, 0x1C, 0x1E));
        resources["EVE_SurfaceBrush"] = new SolidColorBrush(WpfColor.FromRgb(0x2C, 0x2C, 0x2E));
        resources["EVE_TextPrimaryBrush"] = new SolidColorBrush(Colors.White);
        resources["EVE_TextSecondaryBrush"] = new SolidColorBrush(WpfColor.FromRgb(0xC4, 0xC4, 0xC6));
        resources["EVE_BorderBrush"] = new SolidColorBrush(WpfColor.FromRgb(0x48, 0x48, 0x4A));
    }

    private void ApplyLightTheme(ResourceDictionary resources)
    {
        // Update EVE theme colors for light mode (if user prefers light)
        resources["EVE_BackgroundBrush"] = new SolidColorBrush(WpfColor.FromRgb(0xF2, 0xF2, 0xF7));
        resources["EVE_SurfaceBrush"] = new SolidColorBrush(Colors.White);
        resources["EVE_TextPrimaryBrush"] = new SolidColorBrush(Colors.Black);
        resources["EVE_TextSecondaryBrush"] = new SolidColorBrush(WpfColor.FromRgb(0x3C, 0x3C, 0x43));
        resources["EVE_BorderBrush"] = new SolidColorBrush(WpfColor.FromRgb(0xC6, 0xC6, 0xC8));
    }

    private void ApplyAccentColor(ResourceDictionary resources, WpfColor accentColor)
    {
        // Use Windows accent color as secondary, keep EVE cyan as primary
        resources["EVE_AccentBrush"] = new SolidColorBrush(accentColor);
        
        // Create derived colors
        var darkerAccent = WpfColor.FromRgb(
            (byte)(accentColor.R * 0.8),
            (byte)(accentColor.G * 0.8),
            (byte)(accentColor.B * 0.8));
        resources["EVE_AccentDarkBrush"] = new SolidColorBrush(darkerAccent);

        var lighterAccent = WpfColor.FromRgb(
            (byte)Math.Min(255, accentColor.R * 1.2),
            (byte)Math.Min(255, accentColor.G * 1.2),
            (byte)Math.Min(255, accentColor.B * 1.2));
        resources["EVE_AccentLightBrush"] = new SolidColorBrush(lighterAccent);
    }

    private void ApplyTransparencyEffects(ResourceDictionary resources, bool isEnabled)
    {
        if (isEnabled)
        {
            // Enable acrylic/mica-like effects
            resources["EVE_GlassOpacity"] = 0.7;
            resources["EVE_BlurRadius"] = 20.0;
        }
        else
        {
            // Disable transparency effects
            resources["EVE_GlassOpacity"] = 1.0;
            resources["EVE_BlurRadius"] = 0.0;
        }
    }

    private void StartRegistryMonitoring()
    {
        // Note: In a production app, you would use ManagementEventWatcher
        // to monitor registry changes. For now, we'll implement a simple timer-based check
        var timer = new System.Timers.Timer(5000); // Check every 5 seconds
        timer.Elapsed += (sender, e) => CheckForThemeChanges();
        timer.Start();
    }

    private bool _lastDarkMode;
    private WpfColor _lastAccentColor;

    private void CheckForThemeChanges()
    {
        try
        {
            var currentDarkMode = IsDarkMode;
            var currentAccentColor = AccentColor;

            if (currentDarkMode != _lastDarkMode)
            {
                _lastDarkMode = currentDarkMode;
                ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(currentDarkMode));
                ApplyCurrentTheme();
            }

            if (currentAccentColor != _lastAccentColor)
            {
                _lastAccentColor = currentAccentColor;
                AccentColorChanged?.Invoke(this, new AccentColorChangedEventArgs(currentAccentColor));
                ApplyCurrentTheme();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error checking theme changes: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets Windows 11 Mica material information
    /// </summary>
    public bool IsMicaSupported
    {
        get
        {
            // Check if running on Windows 11 and Mica is supported
            try
            {
                var version = Environment.OSVersion.Version;
                return version.Major >= 10 && version.Build >= 22000; // Windows 11
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Attempts to enable Mica material on the main window
    /// </summary>
    public bool TryEnableMica(Window window)
    {
        if (!IsMicaSupported) return false;

        try
        {
            // This would require Windows App SDK integration
            // For now, we'll prepare the structure
            return false; // TODO: Implement when Windows App SDK is fully integrated
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        // Clean up resources and stop monitoring
    }
}

public class ThemeChangedEventArgs : EventArgs
{
    public bool IsDarkMode { get; }

    public ThemeChangedEventArgs(bool isDarkMode)
    {
        IsDarkMode = isDarkMode;
    }
}

public class AccentColorChangedEventArgs : EventArgs
{
    public WpfColor AccentColor { get; }

    public AccentColorChangedEventArgs(WpfColor accentColor)
    {
        AccentColor = accentColor;
    }
}