using Gideon.WPF.Infrastructure.Services;

namespace Gideon.WPF.Infrastructure.Configuration;

/// <summary>
/// Configuration options for high DPI awareness and scaling
/// </summary>
public class HighDpiConfiguration
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "HighDpi";

    /// <summary>
    /// Whether high DPI support is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// DPI awareness mode
    /// </summary>
    public DpiAwarenessMode DpiAwarenessMode { get; set; } = DpiAwarenessMode.PerMonitorV2;

    /// <summary>
    /// Whether to enable automatic scaling
    /// </summary>
    public bool EnableAutoScaling { get; set; } = true;

    /// <summary>
    /// Whether to enable per-monitor DPI awareness
    /// </summary>
    public bool EnablePerMonitorDpi { get; set; } = true;

    /// <summary>
    /// Whether to enable font scaling
    /// </summary>
    public bool EnableFontScaling { get; set; } = true;

    /// <summary>
    /// Minimum font size (in WPF units)
    /// </summary>
    public double MinFontSize { get; set; } = 8.0;

    /// <summary>
    /// Maximum font size (in WPF units)
    /// </summary>
    public double MaxFontSize { get; set; } = 72.0;

    /// <summary>
    /// Whether to scale holographic effects based on DPI
    /// </summary>
    public bool ScaleHolographicEffects { get; set; } = true;

    /// <summary>
    /// Whether to optimize holographic rendering for DPI
    /// </summary>
    public bool OptimizeHolographicRendering { get; set; } = true;

    /// <summary>
    /// Whether to enable DPI change detection
    /// </summary>
    public bool EnableDpiChangeDetection { get; set; } = true;

    /// <summary>
    /// Custom scaling factors for different DPI ranges
    /// </summary>
    public DpiScalingConfiguration Scaling { get; set; } = new();

    /// <summary>
    /// Text rendering configuration
    /// </summary>
    public TextRenderingConfiguration TextRendering { get; set; } = new();

    /// <summary>
    /// Holographic effects scaling configuration
    /// </summary>
    public HolographicScalingConfiguration Holographic { get; set; } = new();
}

/// <summary>
/// DPI scaling configuration
/// </summary>
public class DpiScalingConfiguration
{
    /// <summary>
    /// Scale factor for 125% DPI (120 DPI)
    /// </summary>
    public double Scale125 { get; set; } = 1.25;

    /// <summary>
    /// Scale factor for 150% DPI (144 DPI)
    /// </summary>
    public double Scale150 { get; set; } = 1.5;

    /// <summary>
    /// Scale factor for 175% DPI (168 DPI)
    /// </summary>
    public double Scale175 { get; set; } = 1.75;

    /// <summary>
    /// Scale factor for 200% DPI (192 DPI)
    /// </summary>
    public double Scale200 { get; set; } = 2.0;

    /// <summary>
    /// Scale factor for 250% DPI (240 DPI)
    /// </summary>
    public double Scale250 { get; set; } = 2.5;

    /// <summary>
    /// Scale factor for 300% DPI (288 DPI)
    /// </summary>
    public double Scale300 { get; set; } = 3.0;

    /// <summary>
    /// Whether to use smooth scaling between defined points
    /// </summary>
    public bool UseSmoothScaling { get; set; } = true;

    /// <summary>
    /// Minimum scale factor allowed
    /// </summary>
    public double MinScaleFactor { get; set; } = 0.5;

    /// <summary>
    /// Maximum scale factor allowed
    /// </summary>
    public double MaxScaleFactor { get; set; } = 4.0;
}

/// <summary>
/// Text rendering configuration for different DPI levels
/// </summary>
public class TextRenderingConfiguration
{
    /// <summary>
    /// Whether to enable ClearType at all DPI levels
    /// </summary>
    public bool EnableClearType { get; set; } = true;

    /// <summary>
    /// Text formatting mode
    /// </summary>
    public string TextFormattingMode { get; set; } = "Ideal";

    /// <summary>
    /// Text rendering mode
    /// </summary>
    public string TextRenderingMode { get; set; } = "Auto";

    /// <summary>
    /// Whether to use layout rounding
    /// </summary>
    public bool UseLayoutRounding { get; set; } = true;

    /// <summary>
    /// Whether to snap text to pixel boundaries
    /// </summary>
    public bool SnapToDevicePixels { get; set; } = true;

    /// <summary>
    /// Font scaling behavior
    /// </summary>
    public FontScalingBehavior FontScalingBehavior { get; set; } = FontScalingBehavior.Proportional;

    /// <summary>
    /// Base font sizes for different UI elements
    /// </summary>
    public FontSizeConfiguration FontSizes { get; set; } = new();
}

/// <summary>
/// Font scaling behavior options
/// </summary>
public enum FontScalingBehavior
{
    /// <summary>
    /// Fonts scale proportionally with DPI
    /// </summary>
    Proportional,

    /// <summary>
    /// Fonts scale with diminishing returns at high DPI
    /// </summary>
    DiminishingReturns,

    /// <summary>
    /// Fonts have maximum size limits
    /// </summary>
    Clamped,

    /// <summary>
    /// Custom scaling curves
    /// </summary>
    Custom
}

/// <summary>
/// Font size configuration for different UI elements
/// </summary>
public class FontSizeConfiguration
{
    /// <summary>
    /// Base caption font size
    /// </summary>
    public double Caption { get; set; } = 12.0;

    /// <summary>
    /// Base body font size
    /// </summary>
    public double Body { get; set; } = 14.0;

    /// <summary>
    /// Base subtitle font size
    /// </summary>
    public double Subtitle { get; set; } = 16.0;

    /// <summary>
    /// Base title font size
    /// </summary>
    public double Title { get; set; } = 20.0;

    /// <summary>
    /// Base header font size
    /// </summary>
    public double Header { get; set; } = 24.0;

    /// <summary>
    /// Base display font size
    /// </summary>
    public double Display { get; set; } = 32.0;
}

/// <summary>
/// Holographic effects scaling configuration
/// </summary>
public class HolographicScalingConfiguration
{
    /// <summary>
    /// Whether to scale particle density with DPI
    /// </summary>
    public bool ScaleParticleDensity { get; set; } = true;

    /// <summary>
    /// Whether to scale blur effects with DPI
    /// </summary>
    public bool ScaleBlurEffects { get; set; } = true;

    /// <summary>
    /// Whether to scale glow effects with DPI
    /// </summary>
    public bool ScaleGlowEffects { get; set; } = true;

    /// <summary>
    /// Whether to scale shadow effects with DPI
    /// </summary>
    public bool ScaleShadowEffects { get; set; } = true;

    /// <summary>
    /// Particle density scaling factor
    /// </summary>
    public double ParticleDensityScale { get; set; } = 1.0;

    /// <summary>
    /// Blur radius scaling factor
    /// </summary>
    public double BlurRadiusScale { get; set; } = 1.0;

    /// <summary>
    /// Glow radius scaling factor
    /// </summary>
    public double GlowRadiusScale { get; set; } = 1.0;

    /// <summary>
    /// Shadow depth scaling factor
    /// </summary>
    public double ShadowDepthScale { get; set; } = 1.0;

    /// <summary>
    /// Whether to reduce effects quality at very high DPI for performance
    /// </summary>
    public bool ReduceQualityAtHighDpi { get; set; } = true;

    /// <summary>
    /// DPI threshold above which to reduce quality
    /// </summary>
    public double HighDpiThreshold { get; set; } = 2.5;

    /// <summary>
    /// Quality reduction factor at high DPI
    /// </summary>
    public double HighDpiQualityReduction { get; set; } = 0.75;
}