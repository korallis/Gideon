using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Gideon.WPF.Infrastructure.Services;
using Gideon.WPF.Infrastructure.Extensions;
using WpfUserControl = System.Windows.Controls.UserControl;
using WpfBrush = System.Windows.Media.Brush;
using WpfColor = System.Windows.Media.Color;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// A holographic glassmorphism panel with depth effects and EVE styling
/// </summary>
public partial class HoloPanel : WpfUserControl
{
    #region Dependency Properties

    /// <summary>
    /// The title displayed in the header
    /// </summary>
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(HoloPanel),
            new PropertyMetadata("Holographic Panel", OnTitleChanged));

    /// <summary>
    /// The icon displayed in the header
    /// </summary>
    public static readonly DependencyProperty HeaderIconProperty =
        DependencyProperty.Register(nameof(HeaderIcon), typeof(Brush), typeof(HoloPanel),
            new PropertyMetadata(null, OnHeaderIconChanged));

    /// <summary>
    /// Whether the header is visible
    /// </summary>
    public static readonly DependencyProperty ShowHeaderProperty =
        DependencyProperty.Register(nameof(ShowHeader), typeof(bool), typeof(HoloPanel),
            new PropertyMetadata(true, OnShowHeaderChanged));

    /// <summary>
    /// Whether the footer is visible
    /// </summary>
    public static readonly DependencyProperty ShowFooterProperty =
        DependencyProperty.Register(nameof(ShowFooter), typeof(bool), typeof(HoloPanel),
            new PropertyMetadata(false, OnShowFooterChanged));

    /// <summary>
    /// The footer text
    /// </summary>
    public static readonly DependencyProperty FooterTextProperty =
        DependencyProperty.Register(nameof(FooterText), typeof(string), typeof(HoloPanel),
            new PropertyMetadata("Status: Operational", OnFooterTextChanged));

    /// <summary>
    /// The panel style variant
    /// </summary>
    public static readonly DependencyProperty PanelStyleProperty =
        DependencyProperty.Register(nameof(PanelStyle), typeof(HoloPanelStyle), typeof(HoloPanel),
            new PropertyMetadata(HoloPanelStyle.Holographic, OnPanelStyleChanged));

    /// <summary>
    /// The elevation level (affects shadow depth)
    /// </summary>
    public static readonly DependencyProperty ElevationProperty =
        DependencyProperty.Register(nameof(Elevation), typeof(int), typeof(HoloPanel),
            new PropertyMetadata(2, OnElevationChanged));

    /// <summary>
    /// Whether corner accents are visible
    /// </summary>
    public static readonly DependencyProperty ShowCornerAccentsProperty =
        DependencyProperty.Register(nameof(ShowCornerAccents), typeof(bool), typeof(HoloPanel),
            new PropertyMetadata(true, OnShowCornerAccentsChanged));

    /// <summary>
    /// The glow intensity (0.0 to 1.0)
    /// </summary>
    public static readonly DependencyProperty GlowIntensityProperty =
        DependencyProperty.Register(nameof(GlowIntensity), typeof(double), typeof(HoloPanel),
            new PropertyMetadata(0.8, OnGlowIntensityChanged));

    #endregion

    #region Properties

    /// <summary>
    /// The title displayed in the header
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// The icon displayed in the header
    /// </summary>
    public WpfBrush HeaderIcon
    {
        get => (WpfBrush)GetValue(HeaderIconProperty);
        set => SetValue(HeaderIconProperty, value);
    }

    /// <summary>
    /// Whether the header is visible
    /// </summary>
    public bool ShowHeader
    {
        get => (bool)GetValue(ShowHeaderProperty);
        set => SetValue(ShowHeaderProperty, value);
    }

    /// <summary>
    /// Whether the footer is visible
    /// </summary>
    public bool ShowFooter
    {
        get => (bool)GetValue(ShowFooterProperty);
        set => SetValue(ShowFooterProperty, value);
    }

    /// <summary>
    /// The footer text
    /// </summary>
    public string FooterText
    {
        get => (string)GetValue(FooterTextProperty);
        set => SetValue(FooterTextProperty, value);
    }

    /// <summary>
    /// The panel style variant
    /// </summary>
    public HoloPanelStyle PanelStyle
    {
        get => (HoloPanelStyle)GetValue(PanelStyleProperty);
        set => SetValue(PanelStyleProperty, value);
    }

    /// <summary>
    /// The elevation level (affects shadow depth)
    /// </summary>
    public int Elevation
    {
        get => (int)GetValue(ElevationProperty);
        set => SetValue(ElevationProperty, value);
    }

    /// <summary>
    /// Whether corner accents are visible
    /// </summary>
    public bool ShowCornerAccents
    {
        get => (bool)GetValue(ShowCornerAccentsProperty);
        set => SetValue(ShowCornerAccentsProperty, value);
    }

    /// <summary>
    /// The glow intensity (0.0 to 1.0)
    /// </summary>
    public double GlowIntensity
    {
        get => (double)GetValue(GlowIntensityProperty);
        set => SetValue(GlowIntensityProperty, value);
    }

    #endregion

    #region Fields

    private HighDpiService? _dpiService;

    #endregion

    #region Constructor

    public HoloPanel()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Get DPI service from application services if available
        if (Application.Current is App app)
        {
            // Note: In a real implementation, we'd get this from DI container
            // For now, we'll handle DPI scaling manually
        }

        ConfigureForDpi();
        ApplyPanelStyle();
        UpdateElevation();
    }

    #endregion

    #region Property Change Handlers

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPanel panel && panel.HeaderTitle != null)
        {
            panel.HeaderTitle.Text = e.NewValue?.ToString() ?? string.Empty;
        }
    }

    private static void OnHeaderIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPanel panel && panel.HeaderIconElement != null)
        {
            panel.HeaderIconElement.Fill = e.NewValue as WpfBrush;
        }
    }

    private static void OnShowHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPanel panel && panel.HeaderBorder != null)
        {
            panel.HeaderBorder.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnShowFooterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPanel panel && panel.FooterBorder != null)
        {
            panel.FooterBorder.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnFooterTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPanel panel && panel.FooterTextElement != null)
        {
            panel.FooterTextElement.Text = e.NewValue?.ToString() ?? string.Empty;
        }
    }

    private static void OnPanelStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPanel panel)
        {
            panel.ApplyPanelStyle();
        }
    }

    private static void OnElevationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPanel panel)
        {
            panel.UpdateElevation();
        }
    }

    private static void OnShowCornerAccentsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPanel panel)
        {
            // Update corner accents visibility
            // Note: In a full implementation, we'd find and update the Canvas with corner accents
        }
    }

    private static void OnGlowIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPanel panel)
        {
            panel.UpdateGlowEffects();
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Configure the panel for optimal DPI rendering
    /// </summary>
    private void ConfigureForDpi()
    {
        this.ConfigureForDpi(_dpiService);

        if (_dpiService != null)
        {
            // Scale blur effects for glassmorphism
            if (Resources["GlassmorphismBlur"] is BlurEffect blur)
            {
                blur.Radius = 20.0.GetOptimalBlurRadius(_dpiService);
            }

            // Scale shadow effects
            UpdateElevationWithDpi();
        }
    }

    /// <summary>
    /// Apply the selected panel style
    /// </summary>
    private void ApplyPanelStyle()
    {
        if (MainBorder == null) return;

        switch (PanelStyle)
        {
            case HoloPanelStyle.Holographic:
                MainBorder.Style = (Style)FindResource("HoloPanelBaseStyle");
                break;

            case HoloPanelStyle.EVE:
                MainBorder.Style = (Style)FindResource("EVEPanelStyle");
                break;

            case HoloPanelStyle.Minimal:
                ApplyMinimalStyle();
                break;

            case HoloPanelStyle.Military:
                ApplyMilitaryStyle();
                break;
        }
    }

    /// <summary>
    /// Apply minimal style for subtle panels
    /// </summary>
    private void ApplyMinimalStyle()
    {
        if (MainBorder == null) return;

        MainBorder.Background = new SolidColorBrush(WpfColor.FromArgb(16, 255, 255, 255));
        MainBorder.BorderBrush = new SolidColorBrush(WpfColor.FromArgb(32, 0, 212, 255));
        MainBorder.BorderThickness = new Thickness(1);
    }

    /// <summary>
    /// Apply military style for tactical interfaces
    /// </summary>
    private void ApplyMilitaryStyle()
    {
        if (MainBorder == null) return;

        MainBorder.Background = new SolidColorBrush(WpfColor.FromArgb(32, 255, 215, 0));
        MainBorder.BorderBrush = new SolidColorBrush(WpfColor.FromArgb(128, 255, 215, 0));
        MainBorder.BorderThickness = new Thickness(2);
        MainBorder.CornerRadius = new CornerRadius(4); // More angular
    }

    /// <summary>
    /// Update elevation shadows
    /// </summary>
    private void UpdateElevation()
    {
        if (MainBorder == null) return;

        var shadowKey = Elevation switch
        {
            1 => "DepthShadow1",
            2 => "DepthShadow2",
            3 => "DepthShadow3",
            _ => "DepthShadow2"
        };

        if (FindResource(shadowKey) is DropShadowEffect shadow)
        {
            MainBorder.Effect = shadow;
        }
    }

    /// <summary>
    /// Update elevation with DPI scaling
    /// </summary>
    private void UpdateElevationWithDpi()
    {
        if (MainBorder?.Effect is DropShadowEffect shadow && _dpiService != null)
        {
            shadow.BlurRadius = shadow.BlurRadius.GetOptimalBlurRadius(_dpiService);
            shadow.ShadowDepth = shadow.ShadowDepth.GetDpiAwareShadowDepth(_dpiService);
        }
    }

    /// <summary>
    /// Update glow effects based on intensity
    /// </summary>
    private void UpdateGlowEffects()
    {
        // Update glow effects throughout the panel based on GlowIntensity
        // This would be used for dynamic glow adjustments
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Set the DPI service for proper scaling
    /// </summary>
    public void SetDpiService(HighDpiService dpiService)
    {
        _dpiService = dpiService;
        ConfigureForDpi();
    }

    /// <summary>
    /// Animate the panel's appearance
    /// </summary>
    public void AnimateIn()
    {
        if (FindResource("FadeInStoryboard") is System.Windows.Media.Animation.Storyboard storyboard)
        {
            storyboard.Begin(this);
        }
    }

    /// <summary>
    /// Update the footer progress value
    /// </summary>
    public void SetFooterProgress(double value, bool visible = true)
    {
        if (FooterProgress != null)
        {
            FooterProgress.Value = Math.Max(0, Math.Min(100, value));
            FooterProgress.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Flash the panel with a glow effect for notifications
    /// </summary>
    public void FlashNotification(WpfColor color, double duration = 0.5)
    {
        // Create a temporary glow effect for notifications
        var glowEffect = new DropShadowEffect
        {
            Color = color,
            BlurRadius = 16,
            ShadowDepth = 0,
            Opacity = 1.0
        };

        var originalEffect = MainBorder?.Effect;
        if (MainBorder != null)
        {
            MainBorder.Effect = glowEffect;

            // Restore original effect after duration
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(duration)
            };
            timer.Tick += (s, e) =>
            {
                MainBorder.Effect = originalEffect;
                timer.Stop();
            };
            timer.Start();
        }
    }

    #endregion
}

/// <summary>
/// Available panel style variants
/// </summary>
public enum HoloPanelStyle
{
    /// <summary>
    /// Standard holographic style with cyan accents
    /// </summary>
    Holographic,

    /// <summary>
    /// EVE-themed style with gold accents
    /// </summary>
    EVE,

    /// <summary>
    /// Minimal style for subtle backgrounds
    /// </summary>
    Minimal,

    /// <summary>
    /// Military style for tactical interfaces
    /// </summary>
    Military
}