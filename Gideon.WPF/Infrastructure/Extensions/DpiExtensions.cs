using System.Windows;
using System.Windows.Media;
using Gideon.WPF.Infrastructure.Services;
using WpfSize = System.Windows.Size;

namespace Gideon.WPF.Infrastructure.Extensions;

/// <summary>
/// Extension methods for DPI-aware scaling
/// </summary>
public static class DpiExtensions
{
    /// <summary>
    /// Scale a double value for the current DPI
    /// </summary>
    public static double ScaleForDpi(this double value, HighDpiService dpiService)
    {
        return dpiService?.ScaleValue(value) ?? value;
    }

    /// <summary>
    /// Scale a Size for the current DPI
    /// </summary>
    public static WpfSize ScaleForDpi(this WpfSize size, HighDpiService dpiService)
    {
        return dpiService?.ScaleSize(size) ?? size;
    }

    /// <summary>
    /// Scale a Thickness for the current DPI
    /// </summary>
    public static Thickness ScaleForDpi(this Thickness thickness, HighDpiService dpiService)
    {
        return dpiService?.ScaleThickness(thickness) ?? thickness;
    }

    /// <summary>
    /// Get optimal font size for the current DPI
    /// </summary>
    public static double GetOptimalFontSize(this double baseFontSize, HighDpiService dpiService)
    {
        return dpiService?.GetOptimalFontSize(baseFontSize) ?? baseFontSize;
    }

    /// <summary>
    /// Get optimal particle density for holographic effects
    /// </summary>
    public static double GetOptimalParticleDensity(this double baseParticleDensity, HighDpiService dpiService)
    {
        return dpiService?.GetOptimalParticleDensity(baseParticleDensity) ?? baseParticleDensity;
    }

    /// <summary>
    /// Get optimal blur radius for glassmorphism effects
    /// </summary>
    public static double GetOptimalBlurRadius(this double baseBlurRadius, HighDpiService dpiService)
    {
        return dpiService?.GetOptimalBlurRadius(baseBlurRadius) ?? baseBlurRadius;
    }

    /// <summary>
    /// Configure a FrameworkElement for optimal DPI rendering
    /// </summary>
    public static void ConfigureForDpi(this FrameworkElement element, HighDpiService dpiService)
    {
        if (dpiService == null || !dpiService.IsInitialized) return;

        // Set optimal text rendering properties
        TextOptions.SetTextFormattingMode(element, TextFormattingMode.Ideal);
        TextOptions.SetTextRenderingMode(element, TextRenderingMode.Auto);
        RenderOptions.SetClearTypeHint(element, ClearTypeHint.Enabled);

        // Enable layout rounding for crisp rendering
        element.UseLayoutRounding = true;
        element.SnapsToDevicePixels = true;
    }

    /// <summary>
    /// Configure a Window for optimal DPI handling
    /// </summary>
    public static void ConfigureForDpi(this Window window, HighDpiService dpiService)
    {
        dpiService?.ConfigureWindow(window);
        window.ConfigureForDpi(dpiService);
    }

    /// <summary>
    /// Get DPI-aware corner radius for holographic panels
    /// </summary>
    public static CornerRadius GetDpiAwareCornerRadius(this CornerRadius baseRadius, HighDpiService dpiService)
    {
        if (dpiService == null || !dpiService.IsInitialized) return baseRadius;

        var scaleFactor = dpiService.CurrentScaleFactor;
        return new CornerRadius(
            baseRadius.TopLeft * scaleFactor,
            baseRadius.TopRight * scaleFactor,
            baseRadius.BottomRight * scaleFactor,
            baseRadius.BottomLeft * scaleFactor);
    }

    /// <summary>
    /// Get DPI-aware shadow depth for holographic effects
    /// </summary>
    public static double GetDpiAwareShadowDepth(this double baseShadowDepth, HighDpiService dpiService)
    {
        if (dpiService == null || !dpiService.IsInitialized) return baseShadowDepth;

        // Shadow depth scales with square root for natural appearance
        return baseShadowDepth * Math.Sqrt(dpiService.CurrentScaleFactor);
    }

    /// <summary>
    /// Get DPI-aware glow radius for holographic effects
    /// </summary>
    public static double GetDpiAwareGlowRadius(this double baseGlowRadius, HighDpiService dpiService)
    {
        if (dpiService == null || !dpiService.IsInitialized) return baseGlowRadius;

        return baseGlowRadius * dpiService.CurrentScaleFactor;
    }

    /// <summary>
    /// Create a DPI-aware transform for scaling elements
    /// </summary>
    public static ScaleTransform CreateDpiAwareScaleTransform(this HighDpiService dpiService)
    {
        if (dpiService == null || !dpiService.IsInitialized)
        {
            return new ScaleTransform(1.0, 1.0);
        }

        var scale = dpiService.CurrentScaleFactor;
        return new ScaleTransform(scale, scale);
    }

    /// <summary>
    /// Get DPI-aware line thickness for holographic borders
    /// </summary>
    public static double GetDpiAwareLineThickness(this double baseThickness, HighDpiService dpiService)
    {
        if (dpiService == null || !dpiService.IsInitialized) return baseThickness;

        var scale = dpiService.CurrentScaleFactor;
        
        // Ensure minimum thickness of 1 device pixel
        var scaledThickness = baseThickness * scale;
        return Math.Max(scaledThickness, 1.0 / scale);
    }

    /// <summary>
    /// Get optimal animation duration based on DPI (for performance)
    /// </summary>
    public static TimeSpan GetOptimalAnimationDuration(this TimeSpan baseDuration, HighDpiService dpiService)
    {
        if (dpiService == null || !dpiService.IsInitialized) return baseDuration;

        // Slightly increase animation duration at very high DPI for smoother appearance
        var scale = dpiService.CurrentScaleFactor;
        if (scale > 2.0)
        {
            var factor = 1.0 + (scale - 2.0) * 0.1; // 10% longer per scale factor above 2.0
            return TimeSpan.FromMilliseconds(baseDuration.TotalMilliseconds * factor);
        }

        return baseDuration;
    }
}