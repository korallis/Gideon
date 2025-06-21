using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Interop;
using Gideon.WPF.Infrastructure.Configuration;
using WpfColor = System.Windows.Media.Color;

namespace Gideon.WPF.Infrastructure.Services;

/// <summary>
/// Advanced Windows 11 theme manager with enhanced theme awareness
/// </summary>
public class Windows11ThemeManager
{
    private readonly ILogger<Windows11ThemeManager> _logger;
    private readonly ApplicationModelConfiguration _config;
    private readonly Dictionary<string, WpfColor> _accentColors = new();
    private readonly Dictionary<string, SolidColorBrush> _themeBrushes = new();
    private bool _isInitialized;

    public Windows11ThemeManager(
        ILogger<Windows11ThemeManager> logger,
        IOptions<ApplicationModelConfiguration> config)
    {
        _logger = logger;
        _config = config.Value;
    }

    /// <summary>
    /// Event fired when Windows theme changes
    /// </summary>
    public event EventHandler<Windows11ThemeChangedEventArgs>? ThemeChanged;

    /// <summary>
    /// Event fired when accent color changes
    /// </summary>
    public event EventHandler<AccentColorChangedEventArgs>? AccentColorChanged;

    /// <summary>
    /// Gets the current Windows 11 theme information
    /// </summary>
    public Windows11ThemeInfo CurrentTheme { get; private set; } = new();

    /// <summary>
    /// Gets whether Windows 11 advanced theming is supported
    /// </summary>
    public bool IsWindows11ThemingSupported
    {
        get
        {
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
    /// Initializes the Windows 11 theme manager
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            // Load current theme information
            await LoadCurrentThemeAsync();

            // Initialize theme brushes
            InitializeThemeBrushes();

            // Set up theme change monitoring
            SetupThemeMonitoring();

            _isInitialized = true;
            _logger.LogInformation("Windows 11 theme manager initialized. Theme: {Theme}, AccentColor: {AccentColor}", 
                CurrentTheme.IsDarkMode ? "Dark" : "Light", CurrentTheme.AccentColor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Windows 11 theme manager");
        }
    }

    /// <summary>
    /// Applies Windows 11 theme to the application
    /// </summary>
    public async Task ApplyThemeToApplicationAsync()
    {
        if (!_isInitialized) return;

        try
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var resources = Application.Current.Resources;

                // Apply base theme colors
                ApplyBaseThemeColors(resources);

                // Apply accent colors
                ApplyAccentColors(resources);

                // Apply Windows 11 specific effects
                ApplyWindows11Effects(resources);

                // Update Material Design theme if available
                UpdateMaterialDesignTheme(resources);

                _logger.LogDebug("Applied Windows 11 theme to application");
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply Windows 11 theme");
        }
    }

    /// <summary>
    /// Gets a theme-aware brush for the specified color type
    /// </summary>
    public SolidColorBrush GetThemeBrush(string colorName)
    {
        if (_themeBrushes.TryGetValue(colorName, out var brush))
        {
            return brush;
        }

        // Return default brush if not found
        return new SolidColorBrush(CurrentTheme.IsDarkMode ? Colors.White : Colors.Black);
    }

    /// <summary>
    /// Gets the current accent color with the specified opacity
    /// </summary>
    public WpfColor GetAccentColor(double opacity = 1.0)
    {
        var color = CurrentTheme.AccentColor;
        return WpfColor.FromArgb((byte)(255 * opacity), color.R, color.G, color.B);
    }

    /// <summary>
    /// Gets a derived accent color (lighter or darker)
    /// </summary>
    public WpfColor GetDerivedAccentColor(double factor = 0.8)
    {
        var color = CurrentTheme.AccentColor;
        
        if (factor > 1.0) // Lighter
        {
            return WpfColor.FromRgb(
                (byte)Math.Min(255, color.R + (255 - color.R) * (factor - 1.0)),
                (byte)Math.Min(255, color.G + (255 - color.G) * (factor - 1.0)),
                (byte)Math.Min(255, color.B + (255 - color.B) * (factor - 1.0))
            );
        }
        else // Darker
        {
            return WpfColor.FromRgb(
                (byte)(color.R * factor),
                (byte)(color.G * factor),
                (byte)(color.B * factor)
            );
        }
    }

    /// <summary>
    /// Applies Windows 11 Mica effect to a window
    /// </summary>
    public bool ApplyMicaEffect(Window window)
    {
        if (!IsWindows11ThemingSupported) return false;

        try
        {
            var helper = new WindowInteropHelper(window);
            var hwnd = helper.Handle;

            if (hwnd == IntPtr.Zero) return false;

            // Enable Mica effect using Windows API
            var backdropType = CurrentTheme.IsDarkMode ? 2 : 1; // DWMSBT_MAINWINDOW
            var result = DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdropType, sizeof(int));

            if (result == 0)
            {
                _logger.LogDebug("Applied Mica effect to window: {WindowTitle}", window.Title);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to apply Mica effect to window");
            return false;
        }
    }

    /// <summary>
    /// Updates window title bar colors
    /// </summary>
    public void UpdateWindowTitleBar(Window window)
    {
        if (!IsWindows11ThemingSupported) return;

        try
        {
            var helper = new WindowInteropHelper(window);
            var hwnd = helper.Handle;

            if (hwnd == IntPtr.Zero) return;

            // Set title bar color based on theme
            var titleBarColor = CurrentTheme.IsDarkMode ? 0x00202020 : 0x00F0F0F0;
            DwmSetWindowAttribute(hwnd, DWMWA_CAPTION_COLOR, ref titleBarColor, sizeof(int));

            // Set title bar text color
            var textColor = CurrentTheme.IsDarkMode ? 0x00FFFFFF : 0x00000000;
            DwmSetWindowAttribute(hwnd, DWMWA_TEXT_COLOR, ref textColor, sizeof(int));

            _logger.LogDebug("Updated window title bar colors for: {WindowTitle}", window.Title);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update window title bar colors");
        }
    }

    private async Task LoadCurrentThemeAsync()
    {
        await Task.Run(() =>
        {
            CurrentTheme = new Windows11ThemeInfo
            {
                IsDarkMode = GetSystemDarkMode(),
                AccentColor = GetSystemAccentColor(),
                IsHighContrast = GetHighContrastMode(),
                IsTransparencyEnabled = GetTransparencyEnabled(),
                AnimationsEnabled = GetAnimationsEnabled(),
                AccentColorOnTitleBars = GetAccentColorOnTitleBars(),
                ShowAccentColorOnStartTaskbarJumpLists = GetAccentColorOnTaskbar()
            };

            // Load additional Windows 11 specific settings
            if (IsWindows11ThemingSupported)
            {
                CurrentTheme.MicaEnabled = GetMicaEnabled();
                CurrentTheme.RoundedCorners = GetRoundedCornersEnabled();
            }
        });
    }

    private void InitializeThemeBrushes()
    {
        // Clear existing brushes
        _themeBrushes.Clear();

        if (CurrentTheme.IsDarkMode)
        {
            // Dark theme brushes
            _themeBrushes["Background"] = new SolidColorBrush(WpfColor.FromRgb(0x20, 0x20, 0x20));
            _themeBrushes["Surface"] = new SolidColorBrush(WpfColor.FromRgb(0x2C, 0x2C, 0x2C));
            _themeBrushes["Card"] = new SolidColorBrush(WpfColor.FromRgb(0x36, 0x36, 0x36));
            _themeBrushes["TextPrimary"] = new SolidColorBrush(Colors.White);
            _themeBrushes["TextSecondary"] = new SolidColorBrush(WpfColor.FromRgb(0xB0, 0xB0, 0xB0));
            _themeBrushes["Border"] = new SolidColorBrush(WpfColor.FromRgb(0x48, 0x48, 0x48));
            _themeBrushes["Hover"] = new SolidColorBrush(WpfColor.FromRgb(0x40, 0x40, 0x40));
        }
        else
        {
            // Light theme brushes
            _themeBrushes["Background"] = new SolidColorBrush(WpfColor.FromRgb(0xF8, 0xF8, 0xF8));
            _themeBrushes["Surface"] = new SolidColorBrush(Colors.White);
            _themeBrushes["Card"] = new SolidColorBrush(WpfColor.FromRgb(0xFC, 0xFC, 0xFC));
            _themeBrushes["TextPrimary"] = new SolidColorBrush(Colors.Black);
            _themeBrushes["TextSecondary"] = new SolidColorBrush(WpfColor.FromRgb(0x60, 0x60, 0x60));
            _themeBrushes["Border"] = new SolidColorBrush(WpfColor.FromRgb(0xE0, 0xE0, 0xE0));
            _themeBrushes["Hover"] = new SolidColorBrush(WpfColor.FromRgb(0xF0, 0xF0, 0xF0));
        }

        // Accent color brushes
        _themeBrushes["Accent"] = new SolidColorBrush(CurrentTheme.AccentColor);
        _themeBrushes["AccentLight"] = new SolidColorBrush(GetDerivedAccentColor(1.2));
        _themeBrushes["AccentDark"] = new SolidColorBrush(GetDerivedAccentColor(0.8));
        _themeBrushes["AccentText"] = new SolidColorBrush(GetContrastColor(CurrentTheme.AccentColor));
    }

    private void ApplyBaseThemeColors(ResourceDictionary resources)
    {
        foreach (var brush in _themeBrushes)
        {
            resources[$"Gideon{brush.Key}Brush"] = brush.Value;
        }

        // Set additional theme-specific resources
        resources["GideonThemeIsDark"] = CurrentTheme.IsDarkMode;
        resources["GideonAccentColor"] = CurrentTheme.AccentColor;
        resources["GideonIsHighContrast"] = CurrentTheme.IsHighContrast;
    }

    private void ApplyAccentColors(ResourceDictionary resources)
    {
        // Apply accent color variations
        resources["GideonAccentBrush"] = new SolidColorBrush(CurrentTheme.AccentColor);
        resources["GideonAccentLightBrush"] = new SolidColorBrush(GetDerivedAccentColor(1.2));
        resources["GideonAccentDarkBrush"] = new SolidColorBrush(GetDerivedAccentColor(0.8));
        resources["GideonAccentTransparentBrush"] = new SolidColorBrush(GetAccentColor(0.3));

        // Set accent color for various UI states
        resources["GideonAccentHoverBrush"] = new SolidColorBrush(GetDerivedAccentColor(1.1));
        resources["GideonAccentPressedBrush"] = new SolidColorBrush(GetDerivedAccentColor(0.9));
        resources["GideonAccentDisabledBrush"] = new SolidColorBrush(GetAccentColor(0.5));
    }

    private void ApplyWindows11Effects(ResourceDictionary resources)
    {
        if (!IsWindows11ThemingSupported) return;

        // Windows 11 corner radius
        resources["GideonCornerRadius"] = new CornerRadius(CurrentTheme.RoundedCorners ? 8 : 0);
        resources["GideonCardCornerRadius"] = new CornerRadius(CurrentTheme.RoundedCorners ? 12 : 0);
        resources["GideonButtonCornerRadius"] = new CornerRadius(CurrentTheme.RoundedCorners ? 6 : 0);

        // Mica and transparency effects
        if (CurrentTheme.MicaEnabled && CurrentTheme.IsTransparencyEnabled)
        {
            resources["GideonBackgroundOpacity"] = 0.7;
            resources["GideonBlurRadius"] = 20.0;
        }
        else
        {
            resources["GideonBackgroundOpacity"] = 1.0;
            resources["GideonBlurRadius"] = 0.0;
        }

        // Animation settings
        var animationDuration = CurrentTheme.AnimationsEnabled ? TimeSpan.FromMilliseconds(200) : TimeSpan.Zero;
        resources["GideonAnimationDuration"] = animationDuration;
    }

    private void UpdateMaterialDesignTheme(ResourceDictionary resources)
    {
        try
        {
            // Update Material Design theme to match Windows theme
            // This would integrate with MaterialDesignInXamlToolkit if available
            _logger.LogDebug("Updated Material Design theme integration");
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to update Material Design theme integration");
        }
    }

    private void SetupThemeMonitoring()
    {
        // Register for theme change notifications
        SystemEvents.UserPreferenceChanged += OnSystemThemeChanged;
        _logger.LogDebug("Theme change monitoring setup complete");
    }

    private void OnSystemThemeChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category == UserPreferenceCategory.General || e.Category == UserPreferenceCategory.Color)
        {
            Task.Run(async () =>
            {
                try
                {
                    var oldTheme = CurrentTheme;
                    await LoadCurrentThemeAsync();
                    InitializeThemeBrushes();

                    var hasThemeChanged = oldTheme.IsDarkMode != CurrentTheme.IsDarkMode;
                    var hasAccentChanged = oldTheme.AccentColor != CurrentTheme.AccentColor;

                    if (hasThemeChanged || hasAccentChanged)
                    {
                        await ApplyThemeToApplicationAsync();

                        ThemeChanged?.Invoke(this, new Windows11ThemeChangedEventArgs(CurrentTheme));
                        
                        if (hasAccentChanged)
                        {
                            AccentColorChanged?.Invoke(this, new AccentColorChangedEventArgs(CurrentTheme.AccentColor));
                        }

                        _logger.LogInformation("Theme changed - Dark: {IsDark}, Accent: {AccentColor}", 
                            CurrentTheme.IsDarkMode, CurrentTheme.AccentColor);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling theme change");
                }
            });
        }
    }

    #region Windows Registry Helpers

    private bool GetSystemDarkMode()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            return value is int intValue && intValue == 0;
        }
        catch
        {
            return false;
        }
    }

    private WpfColor GetSystemAccentColor()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM");
            var value = key?.GetValue("AccentColor");
            if (value is int colorValue)
            {
                var bytes = BitConverter.GetBytes(colorValue);
                return WpfColor.FromRgb(bytes[0], bytes[1], bytes[2]);
            }
        }
        catch
        {
            // Ignore errors
        }

        return WpfColor.FromRgb(0x00, 0x78, 0xD4); // Windows default blue
    }

    private bool GetHighContrastMode()
    {
        try
        {
            return SystemParameters.HighContrast;
        }
        catch
        {
            return false;
        }
    }

    private bool GetTransparencyEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("EnableTransparency");
            return value is int intValue && intValue == 1;
        }
        catch
        {
            return true;
        }
    }

    private bool GetAnimationsEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop\WindowMetrics");
            var value = key?.GetValue("MinAnimate");
            return value?.ToString() != "0";
        }
        catch
        {
            return true;
        }
    }

    private bool GetAccentColorOnTitleBars()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM");
            var value = key?.GetValue("ColorPrevalence");
            return value is int intValue && intValue == 1;
        }
        catch
        {
            return false;
        }
    }

    private bool GetAccentColorOnTaskbar()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("ColorPrevalence");
            return value is int intValue && intValue == 1;
        }
        catch
        {
            return false;
        }
    }

    private bool GetMicaEnabled()
    {
        // Mica is enabled by default on Windows 11
        return IsWindows11ThemingSupported;
    }

    private bool GetRoundedCornersEnabled()
    {
        // Rounded corners are default on Windows 11
        return IsWindows11ThemingSupported;
    }

    #endregion

    #region Helper Methods

    private WpfColor GetContrastColor(WpfColor color)
    {
        // Calculate luminance
        var luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
        return luminance > 0.5 ? Colors.Black : Colors.White;
    }

    #endregion

    #region Windows API

    [DllImport("dwmapi.dll", SetLastError = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
    private const int DWMWA_CAPTION_COLOR = 35;
    private const int DWMWA_TEXT_COLOR = 36;

    #endregion

    public void Dispose()
    {
        SystemEvents.UserPreferenceChanged -= OnSystemThemeChanged;
        _logger.LogDebug("Windows 11 theme manager disposed");
    }
}

/// <summary>
/// Contains comprehensive Windows 11 theme information
/// </summary>
public class Windows11ThemeInfo
{
    public bool IsDarkMode { get; set; }
    public WpfColor AccentColor { get; set; }
    public bool IsHighContrast { get; set; }
    public bool IsTransparencyEnabled { get; set; }
    public bool AnimationsEnabled { get; set; }
    public bool AccentColorOnTitleBars { get; set; }
    public bool ShowAccentColorOnStartTaskbarJumpLists { get; set; }
    public bool MicaEnabled { get; set; }
    public bool RoundedCorners { get; set; }
}

/// <summary>
/// Event arguments for Windows 11 theme changes
/// </summary>
public class Windows11ThemeChangedEventArgs : EventArgs
{
    public Windows11ThemeInfo ThemeInfo { get; }

    public Windows11ThemeChangedEventArgs(Windows11ThemeInfo themeInfo)
    {
        ThemeInfo = themeInfo;
    }
}