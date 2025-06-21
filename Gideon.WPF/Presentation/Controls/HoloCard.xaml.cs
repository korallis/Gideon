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
/// A holographic glassmorphism card with depth effects and interactive states
/// </summary>
public partial class HoloCard : WpfUserControl
{
    #region Dependency Properties

    /// <summary>
    /// The header text displayed at the top of the card
    /// </summary>
    public static readonly DependencyProperty HeaderTextProperty =
        DependencyProperty.Register(nameof(HeaderText), typeof(string), typeof(HoloCard),
            new PropertyMetadata(string.Empty, OnHeaderTextChanged));

    /// <summary>
    /// Whether the header section is visible
    /// </summary>
    public static readonly DependencyProperty ShowHeaderProperty =
        DependencyProperty.Register(nameof(ShowHeader), typeof(bool), typeof(HoloCard),
            new PropertyMetadata(false, OnShowHeaderChanged));

    /// <summary>
    /// Whether the header icon is visible
    /// </summary>
    public static readonly DependencyProperty ShowHeaderIconProperty =
        DependencyProperty.Register(nameof(ShowHeaderIcon), typeof(bool), typeof(HoloCard),
            new PropertyMetadata(false, OnShowHeaderIconChanged));

    /// <summary>
    /// The footer text displayed at the bottom of the card
    /// </summary>
    public static readonly DependencyProperty FooterTextProperty =
        DependencyProperty.Register(nameof(FooterText), typeof(string), typeof(HoloCard),
            new PropertyMetadata(string.Empty, OnFooterTextChanged));

    /// <summary>
    /// Whether the footer section is visible
    /// </summary>
    public static readonly DependencyProperty ShowFooterProperty =
        DependencyProperty.Register(nameof(ShowFooter), typeof(bool), typeof(HoloCard),
            new PropertyMetadata(false, OnShowFooterChanged));

    /// <summary>
    /// The badge text displayed in the header
    /// </summary>
    public static readonly DependencyProperty BadgeTextProperty =
        DependencyProperty.Register(nameof(BadgeText), typeof(string), typeof(HoloCard),
            new PropertyMetadata(string.Empty, OnBadgeTextChanged));

    /// <summary>
    /// Whether the badge is visible
    /// </summary>
    public static readonly DependencyProperty ShowBadgeProperty =
        DependencyProperty.Register(nameof(ShowBadge), typeof(bool), typeof(HoloCard),
            new PropertyMetadata(false, OnShowBadgeChanged));

    /// <summary>
    /// The depth level of the card (affects shadow and elevation)
    /// </summary>
    public static readonly DependencyProperty DepthLevelProperty =
        DependencyProperty.Register(nameof(DepthLevel), typeof(HoloCardDepth), typeof(HoloCard),
            new PropertyMetadata(HoloCardDepth.Raised, OnDepthLevelChanged));

    /// <summary>
    /// The visual style variant of the card
    /// </summary>
    public static readonly DependencyProperty CardStyleProperty =
        DependencyProperty.Register(nameof(CardStyle), typeof(HoloCardStyle), typeof(HoloCard),
            new PropertyMetadata(HoloCardStyle.Holographic, OnCardStyleChanged));

    /// <summary>
    /// Whether the card is currently selected
    /// </summary>
    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(HoloCard),
            new PropertyMetadata(false, OnIsSelectedChanged));

    /// <summary>
    /// Whether corner indicators are visible
    /// </summary>
    public static readonly DependencyProperty ShowCornerIndicatorsProperty =
        DependencyProperty.Register(nameof(ShowCornerIndicators), typeof(bool), typeof(HoloCard),
            new PropertyMetadata(true, OnShowCornerIndicatorsChanged));

    /// <summary>
    /// Whether the card is clickable
    /// </summary>
    public static readonly DependencyProperty IsClickableProperty =
        DependencyProperty.Register(nameof(IsClickable), typeof(bool), typeof(HoloCard),
            new PropertyMetadata(true));

    #endregion

    #region Properties

    /// <summary>
    /// The header text displayed at the top of the card
    /// </summary>
    public string HeaderText
    {
        get => (string)GetValue(HeaderTextProperty);
        set => SetValue(HeaderTextProperty, value);
    }

    /// <summary>
    /// Whether the header section is visible
    /// </summary>
    public bool ShowHeader
    {
        get => (bool)GetValue(ShowHeaderProperty);
        set => SetValue(ShowHeaderProperty, value);
    }

    /// <summary>
    /// Whether the header icon is visible
    /// </summary>
    public bool ShowHeaderIcon
    {
        get => (bool)GetValue(ShowHeaderIconProperty);
        set => SetValue(ShowHeaderIconProperty, value);
    }

    /// <summary>
    /// The footer text displayed at the bottom of the card
    /// </summary>
    public string FooterText
    {
        get => (string)GetValue(FooterTextProperty);
        set => SetValue(FooterTextProperty, value);
    }

    /// <summary>
    /// Whether the footer section is visible
    /// </summary>
    public bool ShowFooter
    {
        get => (bool)GetValue(ShowFooterProperty);
        set => SetValue(ShowFooterProperty, value);
    }

    /// <summary>
    /// The badge text displayed in the header
    /// </summary>
    public string BadgeText
    {
        get => (string)GetValue(BadgeTextProperty);
        set => SetValue(BadgeTextProperty, value);
    }

    /// <summary>
    /// Whether the badge is visible
    /// </summary>
    public bool ShowBadge
    {
        get => (bool)GetValue(ShowBadgeProperty);
        set => SetValue(ShowBadgeProperty, value);
    }

    /// <summary>
    /// The depth level of the card (affects shadow and elevation)
    /// </summary>
    public HoloCardDepth DepthLevel
    {
        get => (HoloCardDepth)GetValue(DepthLevelProperty);
        set => SetValue(DepthLevelProperty, value);
    }

    /// <summary>
    /// The visual style variant of the card
    /// </summary>
    public HoloCardStyle CardStyle
    {
        get => (HoloCardStyle)GetValue(CardStyleProperty);
        set => SetValue(CardStyleProperty, value);
    }

    /// <summary>
    /// Whether the card is currently selected
    /// </summary>
    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>
    /// Whether corner indicators are visible
    /// </summary>
    public bool ShowCornerIndicators
    {
        get => (bool)GetValue(ShowCornerIndicatorsProperty);
        set => SetValue(ShowCornerIndicatorsProperty, value);
    }

    /// <summary>
    /// Whether the card is clickable
    /// </summary>
    public bool IsClickable
    {
        get => (bool)GetValue(IsClickableProperty);
        set => SetValue(IsClickableProperty, value);
    }

    #endregion

    #region Events

    /// <summary>
    /// Raised when the card is clicked
    /// </summary>
    public event RoutedEventHandler? CardClicked;

    /// <summary>
    /// Raised when the footer action button is clicked
    /// </summary>
    public event RoutedEventHandler? FooterActionClicked;

    #endregion

    #region Fields

    private HighDpiService? _dpiService;

    #endregion

    #region Constructor

    public HoloCard()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        MouseLeftButtonUp += OnMouseLeftButtonUp;
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ConfigureForDpi();
        ApplyDepthLevel();
        ApplyCardStyle();
    }

    private void OnMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (IsClickable)
        {
            CardClicked?.Invoke(this, new RoutedEventArgs());
        }
    }

    #endregion

    #region Property Change Handlers

    private static void OnHeaderTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCard card && card.HeaderTextElement != null)
        {
            card.HeaderTextElement.Text = e.NewValue?.ToString() ?? string.Empty;
        }
    }

    private static void OnShowHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCard card && card.HeaderSection != null)
        {
            card.HeaderSection.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnShowHeaderIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCard card && card.HeaderIconContainer != null)
        {
            card.HeaderIconContainer.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnFooterTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCard card && card.FooterTextElement != null)
        {
            card.FooterTextElement.Text = e.NewValue?.ToString() ?? string.Empty;
        }
    }

    private static void OnShowFooterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCard card && card.FooterSection != null)
        {
            card.FooterSection.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnBadgeTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCard card && card.BadgeTextElement != null)
        {
            card.BadgeTextElement.Text = e.NewValue?.ToString() ?? string.Empty;
        }
    }

    private static void OnShowBadgeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCard card && card.HeaderBadge != null)
        {
            card.HeaderBadge.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnDepthLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCard card)
        {
            card.ApplyDepthLevel();
        }
    }

    private static void OnCardStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCard card)
        {
            card.ApplyCardStyle();
        }
    }

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCard card && card.SelectionIndicator != null)
        {
            card.SelectionIndicator.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnShowCornerIndicatorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCard card && card.CornerIndicators != null)
        {
            card.CornerIndicators.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Configure the card for optimal DPI rendering
    /// </summary>
    private void ConfigureForDpi()
    {
        this.ConfigureForDpi(_dpiService);

        if (_dpiService != null)
        {
            // Scale blur effects
            if (BlurBackground?.Effect is BlurEffect blur)
            {
                blur.Radius = 15.0.GetOptimalBlurRadius(_dpiService);
            }

            // Scale corner radius
            if (MainCardBorder != null)
            {
                MainCardBorder.CornerRadius = new CornerRadius(8).GetDpiAwareCornerRadius(_dpiService);
            }
        }
    }

    /// <summary>
    /// Apply the selected depth level
    /// </summary>
    private void ApplyDepthLevel()
    {
        if (MainCardBorder == null) return;

        var shadowKey = DepthLevel switch
        {
            HoloCardDepth.Flat => "CardShadowFlat",
            HoloCardDepth.Raised => "CardShadowRaised",
            HoloCardDepth.Elevated => "CardShadowElevated",
            HoloCardDepth.Floating => "CardShadowFloating",
            _ => "CardShadowRaised"
        };

        if (FindResource(shadowKey) is DropShadowEffect shadow)
        {
            MainCardBorder.Effect = shadow;

            // Apply DPI scaling if available
            if (_dpiService != null)
            {
                shadow.BlurRadius = shadow.BlurRadius.GetOptimalBlurRadius(_dpiService);
                shadow.ShadowDepth = shadow.ShadowDepth.GetDpiAwareShadowDepth(_dpiService);
            }
        }
    }

    /// <summary>
    /// Apply the selected card style
    /// </summary>
    private void ApplyCardStyle()
    {
        if (MainCardBorder == null) return;

        switch (CardStyle)
        {
            case HoloCardStyle.Holographic:
                MainCardBorder.BorderBrush = (WpfBrush)FindResource("HoloCardBorder");
                break;

            case HoloCardStyle.EVE:
                MainCardBorder.BorderBrush = (WpfBrush)FindResource("EVECardBorder");
                break;

            case HoloCardStyle.Minimal:
                MainCardBorder.BorderBrush = new SolidColorBrush(WpfColor.FromArgb(64, 255, 255, 255));
                break;

            case HoloCardStyle.Military:
                MainCardBorder.BorderBrush = new SolidColorBrush(WpfColor.FromArgb(128, 255, 215, 0));
                MainCardBorder.CornerRadius = new CornerRadius(4); // More angular
                break;
        }
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
    /// Flash the card with a notification color
    /// </summary>
    public void FlashNotification(WpfColor color, double duration = 1.0)
    {
        var glowEffect = new DropShadowEffect
        {
            Color = color,
            BlurRadius = 16,
            ShadowDepth = 0,
            Opacity = 1.0
        };

        var originalEffect = MainCardBorder?.Effect;
        if (MainCardBorder != null)
        {
            MainCardBorder.Effect = glowEffect;

            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(duration)
            };
            timer.Tick += (s, e) =>
            {
                MainCardBorder.Effect = originalEffect;
                timer.Stop();
            };
            timer.Start();
        }
    }

    /// <summary>
    /// Animate the card entrance
    /// </summary>
    public void AnimateIn(double delay = 0.0)
    {
        Opacity = 0;
        RenderTransform = new ScaleTransform(0.8, 0.8);

        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(delay)
        };
        timer.Tick += (s, e) =>
        {
            // Animate in
            var fadeIn = new System.Windows.Media.Animation.DoubleAnimation(1.0, TimeSpan.FromSeconds(0.3));
            var scaleInX = new System.Windows.Media.Animation.DoubleAnimation(1.0, TimeSpan.FromSeconds(0.3));
            var scaleInY = new System.Windows.Media.Animation.DoubleAnimation(1.0, TimeSpan.FromSeconds(0.3));

            BeginAnimation(OpacityProperty, fadeIn);
            RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleInX);
            RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleInY);

            timer.Stop();
        };
        timer.Start();
    }

    /// <summary>
    /// Set the footer action button properties
    /// </summary>
    public void SetFooterAction(string text, RoutedEventHandler clickHandler)
    {
        if (FooterAction != null)
        {
            FooterAction.Content = text;
            FooterAction.Visibility = Visibility.Visible;
            FooterAction.Click -= OnFooterActionClick; // Remove existing handler
            FooterAction.Click += OnFooterActionClick;
            FooterActionClicked = clickHandler;
        }
    }

    private void OnFooterActionClick(object sender, RoutedEventArgs e)
    {
        FooterActionClicked?.Invoke(this, e);
    }

    #endregion
}

/// <summary>
/// Depth levels for holographic cards
/// </summary>
public enum HoloCardDepth
{
    /// <summary>
    /// Minimal shadow for flat appearance
    /// </summary>
    Flat,

    /// <summary>
    /// Subtle elevation for standard cards
    /// </summary>
    Raised,

    /// <summary>
    /// Moderate elevation for important cards
    /// </summary>
    Elevated,

    /// <summary>
    /// High elevation for floating cards
    /// </summary>
    Floating
}

/// <summary>
/// Visual style variants for holographic cards
/// </summary>
public enum HoloCardStyle
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
    /// Minimal style for subtle cards
    /// </summary>
    Minimal,

    /// <summary>
    /// Military style for tactical interfaces
    /// </summary>
    Military
}