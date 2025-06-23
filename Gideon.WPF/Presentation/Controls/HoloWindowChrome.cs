// ==========================================================================
// HoloWindowChrome.cs - Holographic Window Chrome with EVE Styling
// ==========================================================================
// Advanced window chrome system that provides holographic window decorations,
// custom title bars, and EVE-style window management with glassmorphism effects.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Holographic window chrome with EVE styling and glassmorphism effects
/// </summary>
public class HoloWindowChrome : Control, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloWindowChrome),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloWindowChrome),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty WindowTitleProperty =
        DependencyProperty.Register(nameof(WindowTitle), typeof(string), typeof(HoloWindowChrome),
            new PropertyMetadata("Gideon - EVE Toolkit"));

    public static readonly DependencyProperty WindowIconProperty =
        DependencyProperty.Register(nameof(WindowIcon), typeof(ImageSource), typeof(HoloWindowChrome),
            new PropertyMetadata(null));

    public static readonly DependencyProperty EnableGlassMorphismProperty =
        DependencyProperty.Register(nameof(EnableGlassMorphism), typeof(bool), typeof(HoloWindowChrome),
            new PropertyMetadata(true, OnEnableGlassMorphismChanged));

    public static readonly DependencyProperty EnableBorderGlowProperty =
        DependencyProperty.Register(nameof(EnableBorderGlow), typeof(bool), typeof(HoloWindowChrome),
            new PropertyMetadata(true, OnEnableBorderGlowChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloWindowChrome),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ChromeThicknessProperty =
        DependencyProperty.Register(nameof(ChromeThickness), typeof(Thickness), typeof(HoloWindowChrome),
            new PropertyMetadata(new Thickness(8)));

    public static readonly DependencyProperty TitleBarHeightProperty =
        DependencyProperty.Register(nameof(TitleBarHeight), typeof(double), typeof(HoloWindowChrome),
            new PropertyMetadata(40.0));

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(HoloWindowChrome),
            new PropertyMetadata(new CornerRadius(12)));

    public static readonly DependencyProperty ShowControlButtonsProperty =
        DependencyProperty.Register(nameof(ShowControlButtons), typeof(bool), typeof(HoloWindowChrome),
            new PropertyMetadata(true));

    public double HolographicIntensity
    {
        get => (double)GetValue(HolographicIntensityProperty);
        set => SetValue(HolographicIntensityProperty, value);
    }

    public EVEColorScheme EVEColorScheme
    {
        get => (EVEColorScheme)GetValue(EVEColorSchemeProperty);
        set => SetValue(EVEColorSchemeProperty, value);
    }

    public string WindowTitle
    {
        get => (string)GetValue(WindowTitleProperty);
        set => SetValue(WindowTitleProperty, value);
    }

    public ImageSource WindowIcon
    {
        get => (ImageSource)GetValue(WindowIconProperty);
        set => SetValue(WindowIconProperty, value);
    }

    public bool EnableGlassMorphism
    {
        get => (bool)GetValue(EnableGlassMorphismProperty);
        set => SetValue(EnableGlassMorphismProperty, value);
    }

    public bool EnableBorderGlow
    {
        get => (bool)GetValue(EnableBorderGlowProperty);
        set => SetValue(EnableBorderGlowProperty, value);
    }

    public bool EnableParticleEffects
    {
        get => (bool)GetValue(EnableParticleEffectsProperty);
        set => SetValue(EnableParticleEffectsProperty, value);
    }

    public Thickness ChromeThickness
    {
        get => (Thickness)GetValue(ChromeThicknessProperty);
        set => SetValue(ChromeThicknessProperty, value);
    }

    public double TitleBarHeight
    {
        get => (double)GetValue(TitleBarHeightProperty);
        set => SetValue(TitleBarHeightProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public bool ShowControlButtons
    {
        get => (bool)GetValue(ShowControlButtonsProperty);
        set => SetValue(ShowControlButtonsProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloChromeEventArgs> MinimizeRequested;
    public event EventHandler<HoloChromeEventArgs> MaximizeRequested;
    public event EventHandler<HoloChromeEventArgs> CloseRequested;
    public event EventHandler<HoloChromeEventArgs> TitleBarDoubleClick;

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode { get; private set; }

    public void SwitchToFullMode()
    {
        IsInSimplifiedMode = false;
        EnableGlassMorphism = true;
        EnableBorderGlow = true;
        EnableParticleEffects = true;
        UpdateChromeAppearance();
    }

    public void SwitchToSimplifiedMode()
    {
        IsInSimplifiedMode = true;
        EnableGlassMorphism = false;
        EnableBorderGlow = false;
        EnableParticleEffects = false;
        UpdateChromeAppearance();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => !_disposed && IsLoaded;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        EnableBorderGlow = settings.EnabledFeatures.HasFlag(AnimationFeatures.BasicGlow);
        EnableParticleEffects = settings.EnabledFeatures.HasFlag(AnimationFeatures.ParticleEffects);
        EnableGlassMorphism = settings.EnabledFeatures.HasFlag(AnimationFeatures.AdvancedShaders);
        
        UpdateChromeAppearance();
    }

    #endregion

    #region Fields

    private Grid _chromeGrid;
    private Border _outerBorder;
    private Border _innerBorder;
    private Grid _titleBarGrid;
    private TextBlock _titleText;
    private Image _titleIcon;
    private StackPanel _controlButtons;
    private Canvas _particleCanvas;
    private Rectangle _glassLayer;
    
    private HoloButton _minimizeButton;
    private HoloButton _maximizeButton;
    private HoloButton _closeButton;
    
    private readonly List<ChromeParticle> _particles = new();
    private DispatcherTimer _particleTimer;
    private DispatcherTimer _glowTimer;
    private double _particlePhase = 0;
    private double _glowPhase = 0;
    private readonly Random _random = new();
    private bool _disposed = false;

    #endregion

    #region Constructor

    public HoloWindowChrome()
    {
        DefaultStyleKey = typeof(HoloWindowChrome);
        InitializeChrome();
        SetupAnimations();
        
        // Register with animation intensity manager
        AnimationIntensityManager.Instance.RegisterTarget(this);
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Attach chrome to a window
    /// </summary>
    public void AttachToWindow(Window window)
    {
        if (window == null) return;

        // Set window properties for custom chrome
        window.WindowStyle = WindowStyle.None;
        window.AllowsTransparency = true;
        window.ResizeMode = ResizeMode.CanResize;

        // Set window content
        if (window.Content is FrameworkElement content)
        {
            window.Content = null;
            SetWindowContent(content);
        }

        window.Content = this;

        // Handle window events
        window.MouseLeftButtonDown += OnWindowMouseLeftButtonDown;
        window.StateChanged += OnWindowStateChanged;
        window.SizeChanged += OnWindowSizeChanged;
    }

    /// <summary>
    /// Set the main window content
    /// </summary>
    public void SetWindowContent(FrameworkElement content)
    {
        if (_chromeGrid?.Children.Count > 1)
        {
            _chromeGrid.Children.RemoveAt(1);
        }

        if (content != null)
        {
            Grid.SetRow(content, 1);
            _chromeGrid?.Children.Add(content);
        }
    }

    /// <summary>
    /// Flash chrome for notifications
    /// </summary>
    public void FlashChrome(Color color, TimeSpan duration)
    {
        if (IsInSimplifiedMode) return;

        var flashAnimation = new ColorAnimation
        {
            To = color,
            Duration = TimeSpan.FromMilliseconds(100),
            AutoReverse = true,
            RepeatBehavior = new RepeatBehavior(duration.TotalMilliseconds / 200)
        };

        if (_outerBorder?.BorderBrush is SolidColorBrush brush)
        {
            brush.BeginAnimation(SolidColorBrush.ColorProperty, flashAnimation);
        }
    }

    #endregion

    #region Private Methods

    private void InitializeChrome()
    {
        Template = CreateChromeTemplate();
        UpdateChromeAppearance();
    }

    private ControlTemplate CreateChromeTemplate()
    {
        var template = new ControlTemplate(typeof(HoloWindowChrome));

        // Main chrome grid
        var chromeGrid = new FrameworkElementFactory(typeof(Grid));
        chromeGrid.Name = "PART_ChromeGrid";

        // Row definitions
        var titleRow = new FrameworkElementFactory(typeof(RowDefinition));
        titleRow.SetValue(RowDefinition.HeightProperty, new TemplateBindingExtension(TitleBarHeightProperty));
        var contentRow = new FrameworkElementFactory(typeof(RowDefinition));
        contentRow.SetValue(RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Star));

        chromeGrid.AppendChild(titleRow);
        chromeGrid.AppendChild(contentRow);

        // Outer border with glow
        var outerBorder = new FrameworkElementFactory(typeof(Border));
        outerBorder.Name = "PART_OuterBorder";
        outerBorder.SetValue(Border.CornerRadiusProperty, new TemplateBindingExtension(CornerRadiusProperty));
        outerBorder.SetValue(Border.BorderThicknessProperty, new Thickness(2));
        outerBorder.SetValue(Grid.RowSpanProperty, 2);

        // Inner border for content
        var innerBorder = new FrameworkElementFactory(typeof(Border));
        innerBorder.Name = "PART_InnerBorder";
        innerBorder.SetValue(Border.CornerRadiusProperty, new TemplateBindingExtension(CornerRadiusProperty));
        innerBorder.SetValue(Border.MarginProperty, new Thickness(4));
        innerBorder.SetValue(Grid.RowSpanProperty, 2);

        // Glass layer for glassmorphism
        var glassLayer = new FrameworkElementFactory(typeof(Rectangle));
        glassLayer.Name = "PART_GlassLayer";
        glassLayer.SetValue(Rectangle.RadiusXProperty, new TemplateBindingExtension(CornerRadiusProperty));
        glassLayer.SetValue(Rectangle.RadiusYProperty, new TemplateBindingExtension(CornerRadiusProperty));
        glassLayer.SetValue(Rectangle.OpacityProperty, 0.3);
        glassLayer.SetValue(Grid.RowSpanProperty, 2);

        // Title bar grid
        var titleBarGrid = new FrameworkElementFactory(typeof(Grid));
        titleBarGrid.Name = "PART_TitleBarGrid";
        titleBarGrid.SetValue(Grid.RowProperty, 0);
        titleBarGrid.SetValue(Grid.MarginProperty, new Thickness(8, 4, 8, 4));

        // Title bar columns
        var iconColumn = new FrameworkElementFactory(typeof(ColumnDefinition));
        iconColumn.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);
        var titleColumn = new FrameworkElementFactory(typeof(ColumnDefinition));
        titleColumn.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
        var buttonsColumn = new FrameworkElementFactory(typeof(ColumnDefinition));
        buttonsColumn.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);

        titleBarGrid.AppendChild(iconColumn);
        titleBarGrid.AppendChild(titleColumn);
        titleBarGrid.AppendChild(buttonsColumn);

        // Window icon
        var titleIcon = new FrameworkElementFactory(typeof(Image));
        titleIcon.Name = "PART_TitleIcon";
        titleIcon.SetValue(Image.WidthProperty, 24.0);
        titleIcon.SetValue(Image.HeightProperty, 24.0);
        titleIcon.SetValue(Image.MarginProperty, new Thickness(0, 0, 8, 0));
        titleIcon.SetValue(Image.VerticalAlignmentProperty, VerticalAlignment.Center);
        titleIcon.SetValue(Grid.ColumnProperty, 0);

        // Window title
        var titleText = new FrameworkElementFactory(typeof(TextBlock));
        titleText.Name = "PART_TitleText";
        titleText.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        titleText.SetValue(TextBlock.FontSizeProperty, 14.0);
        titleText.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
        titleText.SetValue(Grid.ColumnProperty, 1);

        // Control buttons panel
        var controlButtons = new FrameworkElementFactory(typeof(StackPanel));
        controlButtons.Name = "PART_ControlButtons";
        controlButtons.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        controlButtons.SetValue(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Center);
        controlButtons.SetValue(Grid.ColumnProperty, 2);

        titleBarGrid.AppendChild(titleIcon);
        titleBarGrid.AppendChild(titleText);
        titleBarGrid.AppendChild(controlButtons);

        // Particle canvas
        var particleCanvas = new FrameworkElementFactory(typeof(Canvas));
        particleCanvas.Name = "PART_ParticleCanvas";
        particleCanvas.SetValue(Canvas.IsHitTestVisibleProperty, false);
        particleCanvas.SetValue(Canvas.ClipToBoundsProperty, true);
        particleCanvas.SetValue(Grid.RowSpanProperty, 2);

        // Assembly
        chromeGrid.AppendChild(outerBorder);
        chromeGrid.AppendChild(innerBorder);
        chromeGrid.AppendChild(glassLayer);
        chromeGrid.AppendChild(titleBarGrid);
        chromeGrid.AppendChild(particleCanvas);

        template.VisualTree = chromeGrid;
        return template;
    }

    private void SetupAnimations()
    {
        // Particle animation timer
        _particleTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50) // 20 FPS
        };
        _particleTimer.Tick += OnParticleTick;

        // Glow animation timer
        _glowTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
        };
        _glowTimer.Tick += OnGlowTick;
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        if (!EnableParticleEffects || IsInSimplifiedMode || _particleCanvas == null) return;

        _particlePhase += 0.1;
        if (_particlePhase > Math.PI * 2)
            _particlePhase = 0;

        UpdateParticles();
        SpawnChromeParticles();
    }

    private void OnGlowTick(object sender, EventArgs e)
    {
        if (!EnableBorderGlow || IsInSimplifiedMode) return;

        _glowPhase += 0.05;
        if (_glowPhase > Math.PI * 2)
            _glowPhase = 0;

        UpdateGlowEffects();
    }

    private void UpdateParticles()
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var particle = _particles[i];
            
            // Update particle position
            particle.X += particle.VelocityX;
            particle.Y += particle.VelocityY;
            particle.Life -= 0.02;

            // Update visual position
            Canvas.SetLeft(particle.Visual, particle.X);
            Canvas.SetTop(particle.Visual, particle.Y);
            
            // Update opacity
            particle.Visual.Opacity = Math.Max(0, particle.Life) * HolographicIntensity;

            // Remove dead particles
            if (particle.Life <= 0 || particle.X > ActualWidth + 10 || particle.Y > ActualHeight + 10)
            {
                _particleCanvas.Children.Remove(particle.Visual);
                _particles.RemoveAt(i);
            }
        }
    }

    private void SpawnChromeParticles()
    {
        if (_particles.Count >= 20) return; // Limit particle count

        if (_random.NextDouble() < 0.2) // 20% chance to spawn
        {
            var particle = CreateChromeParticle();
            _particles.Add(particle);
            _particleCanvas.Children.Add(particle.Visual);
        }
    }

    private ChromeParticle CreateChromeParticle()
    {
        var color = GetEVEColor(EVEColorScheme);
        var size = 1 + _random.NextDouble() * 2;
        
        var ellipse = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromArgb(120, color.R, color.G, color.B))
        };

        var particle = new ChromeParticle
        {
            Visual = ellipse,
            X = _random.NextDouble() * ActualWidth,
            Y = ActualHeight,
            VelocityX = (_random.NextDouble() - 0.5) * 1,
            VelocityY = -1 - _random.NextDouble() * 2,
            Life = 1.0
        };

        Canvas.SetLeft(ellipse, particle.X);
        Canvas.SetTop(ellipse, particle.Y);

        return particle;
    }

    private void UpdateGlowEffects()
    {
        if (_outerBorder?.Effect is DropShadowEffect effect)
        {
            var glowIntensity = 0.5 + (Math.Sin(_glowPhase) * 0.3);
            effect.Opacity = HolographicIntensity * glowIntensity;
        }
    }

    private void UpdateChromeAppearance()
    {
        if (Template == null) return;

        Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            ApplyTemplate();
            
            _chromeGrid = GetTemplateChild("PART_ChromeGrid") as Grid;
            _outerBorder = GetTemplateChild("PART_OuterBorder") as Border;
            _innerBorder = GetTemplateChild("PART_InnerBorder") as Border;
            _titleBarGrid = GetTemplateChild("PART_TitleBarGrid") as Grid;
            _titleText = GetTemplateChild("PART_TitleText") as TextBlock;
            _titleIcon = GetTemplateChild("PART_TitleIcon") as Image;
            _controlButtons = GetTemplateChild("PART_ControlButtons") as StackPanel;
            _particleCanvas = GetTemplateChild("PART_ParticleCanvas") as Canvas;
            _glassLayer = GetTemplateChild("PART_GlassLayer") as Rectangle;

            UpdateColors();
            UpdateEffects();
            UpdateControls();
            UpdateContent();
        }), DispatcherPriority.Render);
    }

    private void UpdateColors()
    {
        var color = GetEVEColor(EVEColorScheme);

        // Outer border colors
        if (_outerBorder != null)
        {
            var backgroundBrush = new LinearGradientBrush();
            backgroundBrush.StartPoint = new Point(0, 0);
            backgroundBrush.EndPoint = new Point(0, 1);
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(240, 0, 20, 40), 0.0));
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(220, 0, 15, 30), 1.0));

            _outerBorder.Background = backgroundBrush;
            _outerBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(
                180, color.R, color.G, color.B));
        }

        // Inner border colors
        if (_innerBorder != null)
        {
            _innerBorder.Background = new SolidColorBrush(Color.FromArgb(40, 0, 20, 40));
            _innerBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(
                100, color.R, color.G, color.B));
            _innerBorder.BorderThickness = new Thickness(1);
        }

        // Glass layer
        if (_glassLayer != null && EnableGlassMorphism)
        {
            _glassLayer.Fill = new SolidColorBrush(Color.FromArgb(
                30, color.R, color.G, color.B));
        }

        // Title text
        if (_titleText != null)
        {
            _titleText.Foreground = new SolidColorBrush(Colors.White);
        }
    }

    private void UpdateEffects()
    {
        var color = GetEVEColor(EVEColorScheme);

        // Outer border glow
        if (_outerBorder != null && EnableBorderGlow && !IsInSimplifiedMode)
        {
            _outerBorder.Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 15 * HolographicIntensity,
                ShadowDepth = 0,
                Opacity = 0.6 * HolographicIntensity
            };
        }
        else if (_outerBorder != null && EnableBorderGlow)
        {
            _outerBorder.Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 4,
                ShadowDepth = 0,
                Opacity = 0.3 * HolographicIntensity
            };
        }
        else if (_outerBorder != null)
        {
            _outerBorder.Effect = null;
        }

        // Glass layer blur
        if (_glassLayer != null && EnableGlassMorphism && !IsInSimplifiedMode)
        {
            _glassLayer.Effect = new BlurEffect
            {
                Radius = 8,
                RenderingBias = RenderingBias.Performance
            };
        }
        else if (_glassLayer != null)
        {
            _glassLayer.Effect = null;
        }
    }

    private void UpdateControls()
    {
        if (_controlButtons == null || !ShowControlButtons) return;

        _controlButtons.Children.Clear();

        // Create control buttons
        _minimizeButton = new HoloButton
        {
            Width = 30,
            Height = 30,
            HolographicIntensity = 0.6,
            EVEColorScheme = EVEColorScheme,
            ButtonState = HoloButtonState.Normal,
            EnableHoverGlow = true,
            Margin = new Thickness(2),
            Content = new Path
            {
                Width = 12,
                Height = 12,
                Fill = new SolidColorBrush(Colors.White),
                Data = Geometry.Parse("M5,12H19V14H5V12Z")
            }
        };
        _minimizeButton.Click += OnMinimizeClick;

        _maximizeButton = new HoloButton
        {
            Width = 30,
            Height = 30,
            HolographicIntensity = 0.6,
            EVEColorScheme = EVEColorScheme,
            ButtonState = HoloButtonState.Normal,
            EnableHoverGlow = true,
            Margin = new Thickness(2),
            Content = new Path
            {
                Width = 12,
                Height = 12,
                Fill = new SolidColorBrush(Colors.White),
                Data = Geometry.Parse("M4,4H20V20H4V4M6,8V18H18V8H6Z")
            }
        };
        _maximizeButton.Click += OnMaximizeClick;

        _closeButton = new HoloButton
        {
            Width = 30,
            Height = 30,
            HolographicIntensity = 0.8,
            EVEColorScheme = EVEColorScheme.CrimsonRed,
            ButtonState = HoloButtonState.Normal,
            EnableHoverGlow = true,
            Margin = new Thickness(2),
            Content = new Path
            {
                Width = 12,
                Height = 12,
                Fill = new SolidColorBrush(Colors.White),
                Data = Geometry.Parse("M19,6.41L17.59,5L12,10.59L6.41,5L5,6.41L10.59,12L5,17.59L6.41,19L12,13.41L17.59,19L19,17.59L13.41,12L19,6.41Z")
            }
        };
        _closeButton.Click += OnCloseClick;

        _controlButtons.Children.Add(_minimizeButton);
        _controlButtons.Children.Add(_maximizeButton);
        _controlButtons.Children.Add(_closeButton);
    }

    private void UpdateContent()
    {
        if (_titleText != null)
        {
            _titleText.Text = WindowTitle;
        }

        if (_titleIcon != null)
        {
            _titleIcon.Source = WindowIcon;
            _titleIcon.Visibility = WindowIcon != null ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private Color GetEVEColor(EVEColorScheme scheme)
    {
        return scheme switch
        {
            EVEColorScheme.ElectricBlue => Color.FromRgb(64, 224, 255),
            EVEColorScheme.GoldAccent => Color.FromRgb(255, 215, 0),
            EVEColorScheme.CrimsonRed => Color.FromRgb(220, 20, 60),
            EVEColorScheme.EmeraldGreen => Color.FromRgb(50, 205, 50),
            EVEColorScheme.VoidPurple => Color.FromRgb(138, 43, 226),
            _ => Color.FromRgb(64, 224, 255)
        };
    }

    private void OnWindowMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            var window = Window.GetWindow(this);
            var position = e.GetPosition(this);
            
            // Check if click is in title bar area
            if (position.Y <= TitleBarHeight)
            {
                if (e.ClickCount == 2)
                {
                    TitleBarDoubleClick?.Invoke(this, new HoloChromeEventArgs());
                }
                else
                {
                    window?.DragMove();
                }
            }
        }
    }

    private void OnWindowStateChanged(object sender, EventArgs e)
    {
        var window = sender as Window;
        if (window == null || _maximizeButton?.Content is not Path maxIcon) return;

        // Update maximize button icon
        if (window.WindowState == WindowState.Maximized)
        {
            maxIcon.Data = Geometry.Parse("M4,8H8V4H20V16H16V20H4V8M16,8V14H18V6H10V8H16Z");
        }
        else
        {
            maxIcon.Data = Geometry.Parse("M4,4H20V20H4V4M6,8V18H18V8H6Z");
        }
    }

    private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Update chrome layout for new window size
        InvalidateArrange();
    }

    private void OnMinimizeClick(object sender, RoutedEventArgs e)
    {
        MinimizeRequested?.Invoke(this, new HoloChromeEventArgs());
    }

    private void OnMaximizeClick(object sender, RoutedEventArgs e)
    {
        MaximizeRequested?.Invoke(this, new HoloChromeEventArgs());
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        CloseRequested?.Invoke(this, new HoloChromeEventArgs());
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableParticleEffects && !IsInSimplifiedMode)
            _particleTimer.Start();
            
        if (EnableBorderGlow && !IsInSimplifiedMode)
            _glowTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _particleTimer?.Stop();
        _glowTimer?.Stop();
        
        // Clean up particles
        _particles.Clear();
        _particleCanvas?.Children.Clear();
        
        // Unregister from animation intensity manager
        AnimationIntensityManager.Instance.UnregisterTarget(this);
        
        _disposed = true;
    }

    #endregion

    #region Event Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloWindowChrome chrome)
            chrome.UpdateChromeAppearance();
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloWindowChrome chrome)
            chrome.UpdateChromeAppearance();
    }

    private static void OnEnableGlassMorphismChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloWindowChrome chrome)
            chrome.UpdateChromeAppearance();
    }

    private static void OnEnableBorderGlowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloWindowChrome chrome)
        {
            chrome.UpdateChromeAppearance();
            
            if ((bool)e.NewValue && !chrome.IsInSimplifiedMode)
                chrome._glowTimer.Start();
            else
                chrome._glowTimer.Stop();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloWindowChrome chrome)
        {
            if ((bool)e.NewValue && !chrome.IsInSimplifiedMode)
                chrome._particleTimer.Start();
            else
                chrome._particleTimer.Stop();
        }
    }

    #endregion
}

/// <summary>
/// Chrome particle for visual effects
/// </summary>
internal class ChromeParticle
{
    public FrameworkElement Visual { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
}

/// <summary>
/// Event args for holographic chrome events
/// </summary>
public class HoloChromeEventArgs : EventArgs
{
    // Event-specific properties can be added here
}