// ==========================================================================
// HoloPortrait.cs - Holographic Character Portrait Display System
// ==========================================================================
// Advanced character portrait display featuring holographic effects,
// dynamic overlays, EVE character integration, and interactive elements.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Holographic character portrait display with EVE integration and interactive elements
/// </summary>
public class HoloPortrait : Control, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloPortrait),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloPortrait),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty CharacterPortraitProperty =
        DependencyProperty.Register(nameof(CharacterPortrait), typeof(ImageSource), typeof(HoloPortrait),
            new PropertyMetadata(null, OnCharacterPortraitChanged));

    public static readonly DependencyProperty CharacterNameProperty =
        DependencyProperty.Register(nameof(CharacterName), typeof(string), typeof(HoloPortrait),
            new PropertyMetadata(null, OnCharacterNameChanged));

    public static readonly DependencyProperty SecurityStatusProperty =
        DependencyProperty.Register(nameof(SecurityStatus), typeof(double), typeof(HoloPortrait),
            new PropertyMetadata(0.0, OnSecurityStatusChanged));

    public static readonly DependencyProperty OnlineStatusProperty =
        DependencyProperty.Register(nameof(OnlineStatus), typeof(CharacterOnlineStatus), typeof(HoloPortrait),
            new PropertyMetadata(CharacterOnlineStatus.Unknown, OnOnlineStatusChanged));

    public static readonly DependencyProperty PortraitSizeProperty =
        DependencyProperty.Register(nameof(PortraitSize), typeof(PortraitSize), typeof(HoloPortrait),
            new PropertyMetadata(PortraitSize.Medium, OnPortraitSizeChanged));

    public static readonly DependencyProperty ShowOverlaysProperty =
        DependencyProperty.Register(nameof(ShowOverlays), typeof(bool), typeof(HoloPortrait),
            new PropertyMetadata(true));

    public static readonly DependencyProperty EnableInteractionProperty =
        DependencyProperty.Register(nameof(EnableInteraction), typeof(bool), typeof(HoloPortrait),
            new PropertyMetadata(true));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloPortrait),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty ShowSecurityBorderProperty =
        DependencyProperty.Register(nameof(ShowSecurityBorder), typeof(bool), typeof(HoloPortrait),
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

    public ImageSource CharacterPortrait
    {
        get => (ImageSource)GetValue(CharacterPortraitProperty);
        set => SetValue(CharacterPortraitProperty, value);
    }

    public string CharacterName
    {
        get => (string)GetValue(CharacterNameProperty);
        set => SetValue(CharacterNameProperty, value);
    }

    public double SecurityStatus
    {
        get => (double)GetValue(SecurityStatusProperty);
        set => SetValue(SecurityStatusProperty, value);
    }

    public CharacterOnlineStatus OnlineStatus
    {
        get => (CharacterOnlineStatus)GetValue(OnlineStatusProperty);
        set => SetValue(OnlineStatusProperty, value);
    }

    public PortraitSize PortraitSize
    {
        get => (PortraitSize)GetValue(PortraitSizeProperty);
        set => SetValue(PortraitSizeProperty, value);
    }

    public bool ShowOverlays
    {
        get => (bool)GetValue(ShowOverlaysProperty);
        set => SetValue(ShowOverlaysProperty, value);
    }

    public bool EnableInteraction
    {
        get => (bool)GetValue(EnableInteractionProperty);
        set => SetValue(EnableInteractionProperty, value);
    }

    public bool EnableAnimations
    {
        get => (bool)GetValue(EnableAnimationsProperty);
        set => SetValue(EnableAnimationsProperty, value);
    }

    public bool ShowSecurityBorder
    {
        get => (bool)GetValue(ShowSecurityBorderProperty);
        set => SetValue(ShowSecurityBorderProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloPortraitEventArgs> PortraitClicked;
    public event EventHandler<HoloPortraitEventArgs> PortraitHovered;
    public event EventHandler<HoloPortraitEventArgs> PortraitDoubleClicked;
    public event EventHandler<HoloPortraitEventArgs> ContextMenuRequested;

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode { get; private set; }

    public void SwitchToFullMode()
    {
        IsInSimplifiedMode = false;
        EnableAnimations = true;
        ShowOverlays = true;
        UpdatePortraitAppearance();
    }

    public void SwitchToSimplifiedMode()
    {
        IsInSimplifiedMode = true;
        EnableAnimations = false;
        ShowOverlays = false;
        UpdatePortraitAppearance();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => !_disposed && IsLoaded;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        EnableAnimations = settings.EnabledFeatures.HasFlag(AnimationFeatures.BasicGlow);
        
        UpdatePortraitAppearance();
    }

    #endregion

    #region Fields

    private Grid _portraitGrid;
    private Border _portraitBorder;
    private Border _imageBorder;
    private Image _portraitImage;
    private Border _overlayBorder;
    private Ellipse _onlineIndicator;
    private TextBlock _securityText;
    private Canvas _effectCanvas;
    private Rectangle _scanlineOverlay;
    
    private readonly List<PortraitParticle> _particles = new();
    private DispatcherTimer _animationTimer;
    private DispatcherTimer _scanlineTimer;
    private double _animationPhase = 0;
    private double _scanlinePhase = 0;
    private readonly Random _random = new();
    private bool _disposed = false;
    private bool _isHovered = false;

    #endregion

    #region Constructor

    public HoloPortrait()
    {
        DefaultStyleKey = typeof(HoloPortrait);
        InitializePortrait();
        SetupAnimations();
        
        // Register with animation intensity manager
        AnimationIntensityManager.Instance.RegisterTarget(this);
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        MouseEnter += OnMouseEnter;
        MouseLeave += OnMouseLeave;
        MouseLeftButtonUp += OnMouseLeftButtonUp;
        MouseDoubleClick += OnMouseDoubleClick;
        MouseRightButtonUp += OnMouseRightButtonUp;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Update character information
    /// </summary>
    public void UpdateCharacter(ImageSource portrait, string name, double securityStatus, CharacterOnlineStatus onlineStatus)
    {
        CharacterPortrait = portrait;
        CharacterName = name;
        SecurityStatus = securityStatus;
        OnlineStatus = onlineStatus;
    }

    /// <summary>
    /// Flash portrait for attention
    /// </summary>
    public void FlashPortrait(Color color, TimeSpan duration)
    {
        if (!EnableAnimations || IsInSimplifiedMode) return;

        var flashAnimation = new ColorAnimation
        {
            To = color,
            Duration = TimeSpan.FromMilliseconds(100),
            AutoReverse = true,
            RepeatBehavior = new RepeatBehavior(duration.TotalMilliseconds / 200)
        };

        if (_portraitBorder?.BorderBrush is SolidColorBrush brush)
        {
            brush.BeginAnimation(SolidColorBrush.ColorProperty, flashAnimation);
        }
    }

    /// <summary>
    /// Set portrait loading state
    /// </summary>
    public void SetLoadingState(bool isLoading)
    {
        if (_portraitImage != null)
        {
            _portraitImage.Opacity = isLoading ? 0.5 : 1.0;
        }

        if (isLoading && EnableAnimations && !IsInSimplifiedMode)
        {
            StartLoadingAnimation();
        }
        else
        {
            StopLoadingAnimation();
        }
    }

    #endregion

    #region Private Methods

    private void InitializePortrait()
    {
        Template = CreatePortraitTemplate();
        UpdatePortraitAppearance();
    }

    private ControlTemplate CreatePortraitTemplate()
    {
        var template = new ControlTemplate(typeof(HoloPortrait));

        // Main portrait grid
        var portraitGrid = new FrameworkElementFactory(typeof(Grid));
        portraitGrid.Name = "PART_PortraitGrid";

        // Portrait border (outer glow)
        var portraitBorder = new FrameworkElementFactory(typeof(Border));
        portraitBorder.Name = "PART_PortraitBorder";
        portraitBorder.SetValue(Border.CornerRadiusProperty, new TemplateBindingExtension(CornerRadiusProperty));
        portraitBorder.SetValue(Border.BorderThicknessProperty, new Thickness(3));
        portraitBorder.SetValue(Border.PaddingProperty, new Thickness(2));

        // Image border (inner frame)
        var imageBorder = new FrameworkElementFactory(typeof(Border));
        imageBorder.Name = "PART_ImageBorder";
        imageBorder.SetValue(Border.CornerRadiusProperty, new TemplateBindingExtension(CornerRadiusProperty));
        imageBorder.SetValue(Border.ClipToBoundsProperty, true);

        // Portrait image
        var portraitImage = new FrameworkElementFactory(typeof(Image));
        portraitImage.Name = "PART_PortraitImage";
        portraitImage.SetValue(Image.StretchProperty, Stretch.UniformToFill);

        imageBorder.AppendChild(portraitImage);

        // Overlay border for additional effects
        var overlayBorder = new FrameworkElementFactory(typeof(Border));
        overlayBorder.Name = "PART_OverlayBorder";
        overlayBorder.SetValue(Border.CornerRadiusProperty, new TemplateBindingExtension(CornerRadiusProperty));
        overlayBorder.SetValue(Border.IsHitTestVisibleProperty, false);

        // Online status indicator
        var onlineIndicator = new FrameworkElementFactory(typeof(Ellipse));
        onlineIndicator.Name = "PART_OnlineIndicator";
        onlineIndicator.SetValue(Ellipse.WidthProperty, 12.0);
        onlineIndicator.SetValue(Ellipse.HeightProperty, 12.0);
        onlineIndicator.SetValue(Ellipse.HorizontalAlignmentProperty, HorizontalAlignment.Right);
        onlineIndicator.SetValue(Ellipse.VerticalAlignmentProperty, VerticalAlignment.Bottom);
        onlineIndicator.SetValue(Ellipse.MarginProperty, new Thickness(0, 0, 2, 2));

        // Security status text
        var securityText = new FrameworkElementFactory(typeof(TextBlock));
        securityText.Name = "PART_SecurityText";
        securityText.SetValue(TextBlock.FontSizeProperty, 9.0);
        securityText.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        securityText.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Left);
        securityText.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Top);
        securityText.SetValue(TextBlock.MarginProperty, new Thickness(2, 2, 0, 0));

        // Scanline overlay
        var scanlineOverlay = new FrameworkElementFactory(typeof(Rectangle));
        scanlineOverlay.Name = "PART_ScanlineOverlay";
        scanlineOverlay.SetValue(Rectangle.HeightProperty, 2.0);
        scanlineOverlay.SetValue(Rectangle.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
        scanlineOverlay.SetValue(Rectangle.VerticalAlignmentProperty, VerticalAlignment.Top);
        scanlineOverlay.SetValue(Rectangle.IsHitTestVisibleProperty, false);
        scanlineOverlay.SetValue(Rectangle.OpacityProperty, 0.6);

        // Effect canvas for particles
        var effectCanvas = new FrameworkElementFactory(typeof(Canvas));
        effectCanvas.Name = "PART_EffectCanvas";
        effectCanvas.SetValue(Canvas.IsHitTestVisibleProperty, false);
        effectCanvas.SetValue(Canvas.ClipToBoundsProperty, true);

        // Assembly
        portraitBorder.AppendChild(imageBorder);
        portraitGrid.AppendChild(portraitBorder);
        portraitGrid.AppendChild(overlayBorder);
        portraitGrid.AppendChild(onlineIndicator);
        portraitGrid.AppendChild(securityText);
        portraitGrid.AppendChild(scanlineOverlay);
        portraitGrid.AppendChild(effectCanvas);

        template.VisualTree = portraitGrid;
        return template;
    }

    private void SetupAnimations()
    {
        // Main animation timer
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
        };
        _animationTimer.Tick += OnAnimationTick;

        // Scanline effect timer
        _scanlineTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100) // 10 FPS
        };
        _scanlineTimer.Tick += OnScanlineTick;
    }

    private void OnAnimationTick(object sender, EventArgs e)
    {
        if (!EnableAnimations || IsInSimplifiedMode) return;

        _animationPhase += 0.05;
        if (_animationPhase > Math.PI * 2)
            _animationPhase = 0;

        UpdateGlowEffects();
        UpdateParticleEffects();
    }

    private void OnScanlineTick(object sender, EventArgs e)
    {
        if (!EnableAnimations || IsInSimplifiedMode || _scanlineOverlay == null) return;

        _scanlinePhase += 0.1;
        if (_scanlinePhase > Math.PI * 2)
            _scanlinePhase = 0;

        UpdateScanlineEffect();
    }

    private void UpdateGlowEffects()
    {
        if (_portraitBorder?.Effect is DropShadowEffect effect)
        {
            var baseIntensity = _isHovered ? 0.8 : 0.5;
            var pulseIntensity = baseIntensity + (Math.Sin(_animationPhase * 2) * 0.2);
            effect.Opacity = HolographicIntensity * pulseIntensity;
        }

        if (_onlineIndicator?.Effect is DropShadowEffect onlineEffect && OnlineStatus == CharacterOnlineStatus.Online)
        {
            var onlinePulse = 0.6 + (Math.Sin(_animationPhase * 3) * 0.4);
            onlineEffect.Opacity = HolographicIntensity * onlinePulse;
        }
    }

    private void UpdateParticleEffects()
    {
        if (!_isHovered || _effectCanvas == null) return;

        // Update existing particles
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var particle = _particles[i];
            
            particle.Life -= 0.02;
            particle.Y -= particle.Speed;
            particle.Opacity = Math.Max(0, particle.Life) * HolographicIntensity;

            Canvas.SetTop(particle.Visual, particle.Y);
            particle.Visual.Opacity = particle.Opacity;

            if (particle.Life <= 0 || particle.Y < -10)
            {
                _effectCanvas.Children.Remove(particle.Visual);
                _particles.RemoveAt(i);
            }
        }

        // Spawn new particles
        if (_particles.Count < 5 && _random.NextDouble() < 0.3)
        {
            SpawnParticle();
        }
    }

    private void SpawnParticle()
    {
        var color = GetEVEColor(EVEColorScheme);
        var size = 1 + _random.NextDouble() * 2;
        
        var ellipse = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromArgb(120, color.R, color.G, color.B))
        };

        var particle = new PortraitParticle
        {
            Visual = ellipse,
            X = _random.NextDouble() * ActualWidth,
            Y = ActualHeight,
            Speed = 1 + _random.NextDouble() * 2,
            Life = 1.0,
            Opacity = 1.0
        };

        Canvas.SetLeft(ellipse, particle.X);
        Canvas.SetTop(ellipse, particle.Y);

        _particles.Add(particle);
        _effectCanvas.Children.Add(ellipse);
    }

    private void UpdateScanlineEffect()
    {
        if (_scanlineOverlay == null) return;

        var scanlinePosition = (Math.Sin(_scanlinePhase) * 0.5 + 0.5) * ActualHeight;
        Canvas.SetTop(_scanlineOverlay, scanlinePosition);
    }

    private void StartLoadingAnimation()
    {
        if (_portraitImage == null) return;

        var rotateTransform = new RotateTransform();
        _portraitImage.RenderTransform = rotateTransform;
        _portraitImage.RenderTransformOrigin = new Point(0.5, 0.5);

        var animation = new DoubleAnimation
        {
            From = 0,
            To = 360,
            Duration = TimeSpan.FromSeconds(2),
            RepeatBehavior = RepeatBehavior.Forever
        };

        rotateTransform.BeginAnimation(RotateTransform.AngleProperty, animation);
    }

    private void StopLoadingAnimation()
    {
        if (_portraitImage == null) return;

        _portraitImage.RenderTransform = null;
    }

    private void UpdatePortraitAppearance()
    {
        if (Template == null) return;

        Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            ApplyTemplate();
            
            _portraitGrid = GetTemplateChild("PART_PortraitGrid") as Grid;
            _portraitBorder = GetTemplateChild("PART_PortraitBorder") as Border;
            _imageBorder = GetTemplateChild("PART_ImageBorder") as Border;
            _portraitImage = GetTemplateChild("PART_PortraitImage") as Image;
            _overlayBorder = GetTemplateChild("PART_OverlayBorder") as Border;
            _onlineIndicator = GetTemplateChild("PART_OnlineIndicator") as Ellipse;
            _securityText = GetTemplateChild("PART_SecurityText") as TextBlock;
            _effectCanvas = GetTemplateChild("PART_EffectCanvas") as Canvas;
            _scanlineOverlay = GetTemplateChild("PART_ScanlineOverlay") as Rectangle;

            UpdatePortraitSize();
            UpdateColors();
            UpdateEffects();
            UpdateOverlays();
            UpdateInteraction();
        }), DispatcherPriority.Render);
    }

    private void UpdatePortraitSize()
    {
        var size = GetPortraitPixelSize(PortraitSize);
        Width = size;
        Height = size;

        var cornerRadius = size / 8; // 1/8 of size for rounded corners
        CornerRadius = new CornerRadius(cornerRadius);
    }

    private void UpdateColors()
    {
        var schemeColor = GetEVEColor(EVEColorScheme);
        var securityColor = GetSecurityStatusColor(SecurityStatus);

        // Portrait border
        if (_portraitBorder != null)
        {
            var borderColor = ShowSecurityBorder ? securityColor : schemeColor;
            _portraitBorder.BorderBrush = new SolidColorBrush(borderColor);
        }

        // Online indicator
        if (_onlineIndicator != null)
        {
            var onlineColor = GetOnlineStatusColor(OnlineStatus);
            _onlineIndicator.Fill = new SolidColorBrush(onlineColor);
            _onlineIndicator.Stroke = new SolidColorBrush(Colors.White);
            _onlineIndicator.StrokeThickness = 1;
        }

        // Security status text
        if (_securityText != null)
        {
            _securityText.Text = SecurityStatus.ToString("F1");
            _securityText.Foreground = new SolidColorBrush(securityColor);
        }

        // Scanline overlay
        if (_scanlineOverlay != null)
        {
            _scanlineOverlay.Fill = new SolidColorBrush(Color.FromArgb(100, schemeColor.R, schemeColor.G, schemeColor.B));
        }
    }

    private void UpdateEffects()
    {
        var schemeColor = GetEVEColor(EVEColorScheme);
        var securityColor = GetSecurityStatusColor(SecurityStatus);

        if (!IsInSimplifiedMode)
        {
            // Portrait border glow
            if (_portraitBorder != null)
            {
                var glowColor = ShowSecurityBorder ? securityColor : schemeColor;
                _portraitBorder.Effect = new DropShadowEffect
                {
                    Color = glowColor,
                    BlurRadius = 8 * HolographicIntensity,
                    ShadowDepth = 0,
                    Opacity = 0.6 * HolographicIntensity
                };
            }

            // Online indicator glow
            if (_onlineIndicator != null && OnlineStatus == CharacterOnlineStatus.Online)
            {
                _onlineIndicator.Effect = new DropShadowEffect
                {
                    Color = Colors.LimeGreen,
                    BlurRadius = 4 * HolographicIntensity,
                    ShadowDepth = 0,
                    Opacity = 0.8 * HolographicIntensity
                };
            }
        }
        else
        {
            // Remove effects in simplified mode
            if (_portraitBorder != null)
                _portraitBorder.Effect = null;
            if (_onlineIndicator != null)
                _onlineIndicator.Effect = null;
        }
    }

    private void UpdateOverlays()
    {
        if (_onlineIndicator != null)
        {
            _onlineIndicator.Visibility = ShowOverlays && OnlineStatus != CharacterOnlineStatus.Unknown ? 
                Visibility.Visible : Visibility.Collapsed;
        }

        if (_securityText != null)
        {
            _securityText.Visibility = ShowOverlays && ShowSecurityBorder ? 
                Visibility.Visible : Visibility.Collapsed;
        }

        if (_scanlineOverlay != null)
        {
            _scanlineOverlay.Visibility = ShowOverlays && EnableAnimations && !IsInSimplifiedMode ? 
                Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void UpdateInteraction()
    {
        Cursor = EnableInteraction ? Cursors.Hand : Cursors.Arrow;
        IsHitTestVisible = EnableInteraction;
    }

    private void UpdatePortraitImage()
    {
        if (_portraitImage != null)
        {
            _portraitImage.Source = CharacterPortrait;
        }
    }

    private double GetPortraitPixelSize(PortraitSize size)
    {
        return size switch
        {
            PortraitSize.Small => 32,
            PortraitSize.Medium => 64,
            PortraitSize.Large => 96,
            PortraitSize.XLarge => 128,
            _ => 64
        };
    }

    private Color GetSecurityStatusColor(double securityStatus)
    {
        if (securityStatus >= 5.0)
            return Color.FromRgb(50, 205, 50);      // Green (high sec)
        else if (securityStatus >= 0.5)
            return Color.FromRgb(255, 215, 0);      // Gold (low sec)
        else if (securityStatus >= 0.0)
            return Color.FromRgb(255, 140, 0);      // Orange (very low sec)
        else if (securityStatus >= -2.0)
            return Color.FromRgb(220, 20, 60);      // Red (negative sec)
        else
            return Color.FromRgb(128, 0, 128);      // Purple (very negative)
    }

    private Color GetOnlineStatusColor(CharacterOnlineStatus status)
    {
        return status switch
        {
            CharacterOnlineStatus.Online => Color.FromRgb(50, 205, 50),
            CharacterOnlineStatus.Away => Color.FromRgb(255, 215, 0),
            CharacterOnlineStatus.Offline => Color.FromRgb(128, 128, 128),
            _ => Color.FromRgb(128, 128, 128)
        };
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

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        _isHovered = true;
        PortraitHovered?.Invoke(this, new HoloPortraitEventArgs 
        { 
            CharacterName = CharacterName,
            IsHovered = true 
        });
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        _isHovered = false;
        
        // Clear particles when not hovered
        _particles.Clear();
        _effectCanvas?.Children.Clear();
        
        PortraitHovered?.Invoke(this, new HoloPortraitEventArgs 
        { 
            CharacterName = CharacterName,
            IsHovered = false 
        });
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!EnableInteraction) return;
        
        PortraitClicked?.Invoke(this, new HoloPortraitEventArgs 
        { 
            CharacterName = CharacterName,
            SecurityStatus = SecurityStatus,
            OnlineStatus = OnlineStatus
        });
    }

    private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (!EnableInteraction) return;
        
        PortraitDoubleClicked?.Invoke(this, new HoloPortraitEventArgs 
        { 
            CharacterName = CharacterName,
            SecurityStatus = SecurityStatus,
            OnlineStatus = OnlineStatus
        });
    }

    private void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!EnableInteraction) return;
        
        ContextMenuRequested?.Invoke(this, new HoloPortraitEventArgs 
        { 
            CharacterName = CharacterName,
            SecurityStatus = SecurityStatus,
            OnlineStatus = OnlineStatus
        });
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableAnimations && !IsInSimplifiedMode)
        {
            _animationTimer.Start();
            _scanlineTimer.Start();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        _scanlineTimer?.Stop();
        
        // Clean up particles
        _particles.Clear();
        _effectCanvas?.Children.Clear();
        
        // Unregister from animation intensity manager
        AnimationIntensityManager.Instance.UnregisterTarget(this);
        
        _disposed = true;
    }

    #endregion

    #region Event Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPortrait portrait)
            portrait.UpdatePortraitAppearance();
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPortrait portrait)
            portrait.UpdatePortraitAppearance();
    }

    private static void OnCharacterPortraitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPortrait portrait)
            portrait.UpdatePortraitImage();
    }

    private static void OnCharacterNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Name changes don't require visual updates currently
    }

    private static void OnSecurityStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPortrait portrait)
            portrait.UpdateColors();
    }

    private static void OnOnlineStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPortrait portrait)
            portrait.UpdateColors();
    }

    private static void OnPortraitSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPortrait portrait)
            portrait.UpdatePortraitSize();
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPortrait portrait)
        {
            if ((bool)e.NewValue && !portrait.IsInSimplifiedMode)
            {
                portrait._animationTimer.Start();
                portrait._scanlineTimer.Start();
            }
            else
            {
                portrait._animationTimer.Stop();
                portrait._scanlineTimer.Stop();
            }
            
            portrait.UpdateOverlays();
        }
    }

    #endregion
}

/// <summary>
/// Portrait particle for hover effects
/// </summary>
internal class PortraitParticle
{
    public FrameworkElement Visual { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Speed { get; set; }
    public double Life { get; set; }
    public double Opacity { get; set; }
}

/// <summary>
/// Character online status
/// </summary>
public enum CharacterOnlineStatus
{
    Unknown,
    Offline,
    Online,
    Away
}

/// <summary>
/// Portrait sizes
/// </summary>
public enum PortraitSize
{
    Small,
    Medium,
    Large,
    XLarge
}

/// <summary>
/// Event args for portrait events
/// </summary>
public class HoloPortraitEventArgs : EventArgs
{
    public string CharacterName { get; set; }
    public double SecurityStatus { get; set; }
    public CharacterOnlineStatus OnlineStatus { get; set; }
    public bool IsHovered { get; set; }
}