using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Gideon.WPF.Infrastructure.Configuration;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using WpfSize = System.Windows.Size;

namespace Gideon.WPF.Infrastructure.Services;

/// <summary>
/// Service for managing high DPI awareness and scaling
/// </summary>
public class HighDpiService : IDisposable
{
    private readonly ILogger<HighDpiService> _logger;
    private readonly HighDpiConfiguration _config;
    private bool _isInitialized;
    private bool _isDisposed;
    private double _systemDpiX = 96.0;
    private double _systemDpiY = 96.0;
    private double _currentScaleFactor = 1.0;

    // DPI awareness constants
    private const int DPI_AWARENESS_CONTEXT_UNAWARE = -1;
    private const int DPI_AWARENESS_CONTEXT_SYSTEM_AWARE = -2;
    private const int DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE = -3;
    private const int DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4;

    public HighDpiService(
        ILogger<HighDpiService> logger,
        IOptions<HighDpiConfiguration> config)
    {
        _logger = logger;
        _config = config.Value;
    }

    /// <summary>
    /// Gets whether high DPI support is available
    /// </summary>
    public bool IsAvailable => Environment.OSVersion.Version >= new Version(6, 3); // Windows 8.1+

    /// <summary>
    /// Gets whether the service has been initialized
    /// </summary>
    public bool IsInitialized => _isInitialized;

    /// <summary>
    /// Gets the current system DPI scale factor
    /// </summary>
    public double CurrentScaleFactor => _currentScaleFactor;

    /// <summary>
    /// Gets the system DPI values
    /// </summary>
    public (double X, double Y) SystemDpi => (_systemDpiX, _systemDpiY);

    /// <summary>
    /// Event raised when DPI changes
    /// </summary>
    public event EventHandler<DpiChangedEventArgs>? DpiChanged;

    /// <summary>
    /// Initialize high DPI support
    /// </summary>
    public async Task<bool> InitializeAsync()
    {
        if (_isInitialized || !IsAvailable || !_config.Enabled)
        {
            return _isInitialized;
        }

        try
        {
            _logger.LogInformation("Initializing high DPI awareness...");

            // Set DPI awareness
            await SetDpiAwarenessAsync();

            // Get system DPI
            await GetSystemDpiAsync();

            // Calculate initial scale factor
            _currentScaleFactor = _systemDpiX / 96.0;

            // Set up DPI change monitoring
            await SetupDpiMonitoringAsync();

            // Configure holographic rendering for DPI
            if (_config.OptimizeHolographicRendering)
            {
                await OptimizeHolographicRenderingAsync();
            }

            _isInitialized = true;
            _logger.LogInformation("High DPI awareness initialized successfully. Scale factor: {ScaleFactor:F2}", _currentScaleFactor);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize high DPI awareness");
            return false;
        }
    }

    /// <summary>
    /// Set DPI awareness context
    /// </summary>
    private async Task SetDpiAwarenessAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                // Set per-monitor DPI awareness V2 for best results
                if (_config.DpiAwarenessMode == DpiAwarenessMode.PerMonitorV2)
                {
                    SetProcessDpiAwarenessContext(new IntPtr(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2));
                    _logger.LogDebug("Set DPI awareness to PerMonitorV2");
                }
                else if (_config.DpiAwarenessMode == DpiAwarenessMode.PerMonitor)
                {
                    SetProcessDpiAwarenessContext(new IntPtr(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE));
                    _logger.LogDebug("Set DPI awareness to PerMonitor");
                }
                else if (_config.DpiAwarenessMode == DpiAwarenessMode.System)
                {
                    SetProcessDpiAwarenessContext(new IntPtr(DPI_AWARENESS_CONTEXT_SYSTEM_AWARE));
                    _logger.LogDebug("Set DPI awareness to System");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set DPI awareness context");
            }
        });
    }

    /// <summary>
    /// Get system DPI values
    /// </summary>
    private async Task GetSystemDpiAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                var source = PresentationSource.FromVisual(Application.Current.MainWindow);
                if (source?.CompositionTarget != null)
                {
                    var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                    var dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

                    if (dpiXProperty != null && dpiYProperty != null)
                    {
                        _systemDpiX = (int)(dpiXProperty.GetValue(null) ?? 96);
                        _systemDpiY = (int)(dpiYProperty.GetValue(null) ?? 96);
                    }
                    else
                    {
                        // Fallback method
                        _systemDpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
                        _systemDpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;
                    }
                }
                else
                {
                    // Fallback to system metrics
                    using var graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
                    _systemDpiX = graphics.DpiX;
                    _systemDpiY = graphics.DpiY;
                }

                _logger.LogDebug("System DPI: {DpiX} x {DpiY}", _systemDpiX, _systemDpiY);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get system DPI, using defaults");
                _systemDpiX = _systemDpiY = 96.0;
            }
        });
    }

    /// <summary>
    /// Set up DPI change monitoring
    /// </summary>
    private async Task SetupDpiMonitoringAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                // Monitor for DPI changes at the application level
                SystemParameters.StaticPropertyChanged += OnSystemParametersChanged;
                _logger.LogDebug("DPI change monitoring enabled");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set up DPI monitoring");
            }
        });
    }

    /// <summary>
    /// Optimize holographic rendering for current DPI
    /// </summary>
    private async Task OptimizeHolographicRenderingAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                // Set optimal rendering settings based on DPI
                if (_currentScaleFactor >= 2.0)
                {
                    // High DPI optimizations
                    RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
                    _logger.LogDebug("Optimized rendering for high DPI (scale: {Scale:F2})", _currentScaleFactor);
                }
                else if (_currentScaleFactor >= 1.5)
                {
                    // Medium DPI optimizations
                    _logger.LogDebug("Optimized rendering for medium DPI (scale: {Scale:F2})", _currentScaleFactor);
                }
                else
                {
                    // Standard DPI
                    _logger.LogDebug("Standard DPI rendering (scale: {Scale:F2})", _currentScaleFactor);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to optimize holographic rendering");
            }
        });
    }

    /// <summary>
    /// Handle system parameter changes (including DPI changes)
    /// </summary>
    private void OnSystemParametersChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "DpiX" || e.PropertyName == "Dpi")
        {
            Task.Run(async () =>
            {
                var oldScaleFactor = _currentScaleFactor;
                await GetSystemDpiAsync();
                _currentScaleFactor = _systemDpiX / 96.0;

                if (Math.Abs(_currentScaleFactor - oldScaleFactor) > 0.01)
                {
                    _logger.LogInformation("DPI changed from {OldScale:F2} to {NewScale:F2}", oldScaleFactor, _currentScaleFactor);
                    
                    if (_config.OptimizeHolographicRendering)
                    {
                        await OptimizeHolographicRenderingAsync();
                    }

                    DpiChanged?.Invoke(this, new DpiChangedEventArgs(oldScaleFactor, _currentScaleFactor, _systemDpiX, _systemDpiY));
                }
            });
        }
    }

    /// <summary>
    /// Scale a value for current DPI
    /// </summary>
    public double ScaleValue(double value)
    {
        return _config.EnableAutoScaling ? value * _currentScaleFactor : value;
    }

    /// <summary>
    /// Scale a size for current DPI
    /// </summary>
    public WpfSize ScaleSize(WpfSize size)
    {
        if (!_config.EnableAutoScaling) return size;
        return new WpfSize(size.Width * _currentScaleFactor, size.Height * _currentScaleFactor);
    }

    /// <summary>
    /// Scale a thickness for current DPI
    /// </summary>
    public Thickness ScaleThickness(Thickness thickness)
    {
        if (!_config.EnableAutoScaling) return thickness;
        return new Thickness(
            thickness.Left * _currentScaleFactor,
            thickness.Top * _currentScaleFactor,
            thickness.Right * _currentScaleFactor,
            thickness.Bottom * _currentScaleFactor);
    }

    /// <summary>
    /// Get optimal font size for current DPI
    /// </summary>
    public double GetOptimalFontSize(double baseFontSize)
    {
        if (!_config.EnableFontScaling) return baseFontSize;

        var scaledSize = baseFontSize * _currentScaleFactor;
        
        // Apply font scaling limits
        if (scaledSize < _config.MinFontSize) return _config.MinFontSize;
        if (scaledSize > _config.MaxFontSize) return _config.MaxFontSize;
        
        return scaledSize;
    }

    /// <summary>
    /// Get optimal particle density for current DPI (for holographic effects)
    /// </summary>
    public double GetOptimalParticleDensity(double baseParticleDensity)
    {
        if (!_config.ScaleHolographicEffects) return baseParticleDensity;

        // Scale particle density based on DPI, but with diminishing returns at very high DPI
        var scaleFactor = Math.Min(_currentScaleFactor, 2.0);
        return baseParticleDensity * scaleFactor;
    }

    /// <summary>
    /// Get optimal blur radius for glassmorphism effects
    /// </summary>
    public double GetOptimalBlurRadius(double baseBlurRadius)
    {
        if (!_config.ScaleHolographicEffects) return baseBlurRadius;
        return baseBlurRadius * Math.Sqrt(_currentScaleFactor);
    }

    /// <summary>
    /// Configure window for optimal DPI handling
    /// </summary>
    public void ConfigureWindow(Window window)
    {
        if (!_isInitialized) return;

        try
        {
            // Enable per-monitor DPI awareness for the window
            if (_config.EnablePerMonitorDpi)
            {
                var hwnd = new WindowInteropHelper(window).Handle;
                if (hwnd != IntPtr.Zero)
                {
                    SetWindowDpiAwarenessContext(hwnd, new IntPtr(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2));
                }
            }

            // Set optimal text rendering
            TextOptions.SetTextFormattingMode(window, TextFormattingMode.Ideal);
            TextOptions.SetTextRenderingMode(window, TextRenderingMode.Auto);
            RenderOptions.SetClearTypeHint(window, ClearTypeHint.Enabled);

            _logger.LogDebug("Configured window for optimal DPI handling");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to configure window for DPI");
        }
    }

    #region Windows API

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetProcessDpiAwarenessContext(IntPtr dpiAwarenessContext);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowDpiAwarenessContext(IntPtr hwnd, IntPtr dpiAwarenessContext);

    #endregion

    /// <summary>
    /// Dispose resources
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;

        try
        {
            SystemParameters.StaticPropertyChanged -= OnSystemParametersChanged;
            _logger.LogDebug("High DPI service disposed");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error disposing high DPI service");
        }

        _isDisposed = true;
    }
}

/// <summary>
/// DPI awareness modes
/// </summary>
public enum DpiAwarenessMode
{
    Unaware,
    System,
    PerMonitor,
    PerMonitorV2
}

/// <summary>
/// Event arguments for DPI change events
/// </summary>
public class DpiChangedEventArgs : EventArgs
{
    public double OldScaleFactor { get; }
    public double NewScaleFactor { get; }
    public double DpiX { get; }
    public double DpiY { get; }

    public DpiChangedEventArgs(double oldScaleFactor, double newScaleFactor, double dpiX, double dpiY)
    {
        OldScaleFactor = oldScaleFactor;
        NewScaleFactor = newScaleFactor;
        DpiX = dpiX;
        DpiY = dpiY;
    }
}