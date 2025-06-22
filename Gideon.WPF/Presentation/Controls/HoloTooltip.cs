// ==========================================================================
// HoloTooltip.cs - Holographic Tooltip with Contextual Information Display
// ==========================================================================
// Advanced holographic tooltip featuring contextual information display,
// EVE-style visual effects, smart positioning, and adaptive performance.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Advanced holographic tooltip with contextual information display and EVE styling
/// </summary>
public class HoloTooltip : Popup, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloTooltip),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloTooltip),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty TooltipTypeProperty =
        DependencyProperty.Register(nameof(TooltipType), typeof(HoloTooltipType), typeof(HoloTooltip),
            new PropertyMetadata(HoloTooltipType.Information, OnTooltipTypeChanged));

    public static readonly DependencyProperty TooltipTitleProperty =
        DependencyProperty.Register(nameof(TooltipTitle), typeof(string), typeof(HoloTooltip),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty TooltipContentProperty =
        DependencyProperty.Register(nameof(TooltipContent), typeof(object), typeof(HoloTooltip),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ShowPointerProperty =
        DependencyProperty.Register(nameof(ShowPointer), typeof(bool), typeof(HoloTooltip),
            new PropertyMetadata(true, OnShowPointerChanged));

    public static readonly DependencyProperty EnableAnimationProperty =
        DependencyProperty.Register(nameof(EnableAnimation), typeof(bool), typeof(HoloTooltip),
            new PropertyMetadata(true, OnEnableAnimationChanged));

    public static readonly DependencyProperty ShowIconProperty =
        DependencyProperty.Register(nameof(ShowIcon), typeof(bool), typeof(HoloTooltip),
            new PropertyMetadata(true));

    public static readonly DependencyProperty MaxWidthLimitProperty =
        DependencyProperty.Register(nameof(MaxWidthLimit), typeof(double), typeof(HoloTooltip),
            new PropertyMetadata(300.0));

    public static readonly DependencyProperty AutoPositionProperty =
        DependencyProperty.Register(nameof(AutoPosition), typeof(bool), typeof(HoloTooltip),
            new PropertyMetadata(true));

    public static readonly DependencyProperty EnableGlowEffectProperty =
        DependencyProperty.Register(nameof(EnableGlowEffect), typeof(bool), typeof(HoloTooltip),
            new PropertyMetadata(true, OnEnableGlowEffectChanged));

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

    public HoloTooltipType TooltipType
    {
        get => (HoloTooltipType)GetValue(TooltipTypeProperty);
        set => SetValue(TooltipTypeProperty, value);
    }

    public string TooltipTitle
    {
        get => (string)GetValue(TooltipTitleProperty);
        set => SetValue(TooltipTitleProperty, value);
    }

    public object TooltipContent
    {
        get => GetValue(TooltipContentProperty);
        set => SetValue(TooltipContentProperty, value);
    }

    public bool ShowPointer
    {
        get => (bool)GetValue(ShowPointerProperty);
        set => SetValue(ShowPointerProperty, value);
    }

    public bool EnableAnimation
    {
        get => (bool)GetValue(EnableAnimationProperty);
        set => SetValue(EnableAnimationProperty, value);
    }

    public bool ShowIcon
    {
        get => (bool)GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    public double MaxWidthLimit
    {
        get => (double)GetValue(MaxWidthLimitProperty);
        set => SetValue(MaxWidthLimitProperty, value);
    }

    public bool AutoPosition
    {
        get => (bool)GetValue(AutoPositionProperty);
        set => SetValue(AutoPositionProperty, value);
    }

    public bool EnableGlowEffect
    {
        get => (bool)GetValue(EnableGlowEffectProperty);
        set => SetValue(EnableGlowEffectProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloTooltipEventArgs> TooltipShown;
    public event EventHandler<HoloTooltipEventArgs> TooltipHidden;

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode { get; private set; }

    public void SwitchToFullMode()
    {
        IsInSimplifiedMode = false;
        EnableAnimation = true;
        EnableGlowEffect = true;
        ShowPointer = true;
        UpdateTooltipAppearance();
    }

    public void SwitchToSimplifiedMode()
    {
        IsInSimplifiedMode = true;
        EnableAnimation = false;
        EnableGlowEffect = false;
        ShowPointer = false;
        UpdateTooltipAppearance();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => !_disposed && IsLoaded;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        EnableAnimation = settings.EnabledFeatures.HasFlag(AnimationFeatures.ComplexTransitions);
        EnableGlowEffect = settings.EnabledFeatures.HasFlag(AnimationFeatures.BasicGlow);
        
        UpdateTooltipAppearance();
    }

    #endregion

    #region Fields

    private Border _tooltipBorder;
    private Grid _contentGrid;
    private TextBlock _titleText;
    private ContentPresenter _contentPresenter;
    private Path _iconPath;
    private Polygon _pointer;
    private Canvas _effectCanvas;
    private Border _backgroundBorder;
    
    private readonly List<TooltipParticle> _particles = new();
    private DispatcherTimer _particleTimer;
    private DispatcherTimer _glowTimer;
    private double _particlePhase = 0;
    private double _glowPhase = 0;
    private readonly Random _random = new();
    private bool _disposed = false;
    private bool _isShowing = false;

    #endregion

    #region Constructor

    public HoloTooltip()
    {
        InitializeTooltip();
        SetupAnimations();
        
        // Register with animation intensity manager
        AnimationIntensityManager.Instance.RegisterTarget(this);
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        Opened += OnOpened;
        Closed += OnClosed;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Show tooltip at specified position
    /// </summary>
    public void ShowTooltip(Point position, string title, object content, HoloTooltipType type = HoloTooltipType.Information)
    {
        TooltipTitle = title;
        TooltipContent = content;
        TooltipType = type;
        
        if (AutoPosition)
        {
            SetOptimalPosition(position);
        }
        else
        {
            HorizontalOffset = position.X;
            VerticalOffset = position.Y;
        }
        
        if (EnableAnimation && !IsInSimplifiedMode)
        {
            AnimateShow();
        }
        else
        {
            IsOpen = true;
        }
    }

    /// <summary>
    /// Hide tooltip with animation
    /// </summary>
    public void HideTooltip()
    {
        if (EnableAnimation && !IsInSimplifiedMode)
        {
            AnimateHide();
        }
        else
        {
            IsOpen = false;
        }
    }

    /// <summary>
    /// Update tooltip content without hiding
    /// </summary>
    public void UpdateContent(string title, object content)
    {
        TooltipTitle = title;
        TooltipContent = content;
        UpdateTooltipContent();
    }

    #endregion

    #region Private Methods

    private void InitializeTooltip()
    {
        AllowsTransparency = true;
        PopupAnimation = PopupAnimation.None; // We handle animation ourselves
        StaysOpen = false;
        
        Child = CreateTooltipContent();
        UpdateTooltipAppearance();
    }

    private FrameworkElement CreateTooltipContent()
    {
        // Main border container
        _tooltipBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(240, 0, 20, 40)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(180, 64, 224, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12, 8, 12, 8),
            MaxWidth = MaxWidthLimit
        };

        // Content grid
        _contentGrid = new Grid();
        
        // Row definitions
        _contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Header grid for title and icon
        var headerGrid = new Grid();
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Icon
        _iconPath = new Path
        {
            Width = 16,
            Height = 16,
            Stretch = Stretch.Uniform,
            Fill = new SolidColorBrush(GetTypeColor(TooltipType)),
            Margin = new Thickness(0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Top
        };
        Grid.SetColumn(_iconPath, 0);
        headerGrid.Children.Add(_iconPath);

        // Title
        _titleText = new TextBlock
        {
            FontWeight = FontWeights.Bold,
            FontSize = 12,
            Foreground = new SolidColorBrush(Colors.White),
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(_titleText, 1);
        headerGrid.Children.Add(_titleText);

        Grid.SetRow(headerGrid, 0);
        _contentGrid.Children.Add(headerGrid);

        // Content presenter
        _contentPresenter = new ContentPresenter
        {
            Margin = new Thickness(0, 4, 0, 0)
        };
        Grid.SetRow(_contentPresenter, 1);
        _contentGrid.Children.Add(_contentPresenter);

        // Effect canvas for particles
        _effectCanvas = new Canvas
        {
            IsHitTestVisible = false,
            ClipToBounds = true
        };

        // Background border for glow effects
        _backgroundBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 64, 224, 255)),
            CornerRadius = new CornerRadius(8),
            Margin = new Thickness(-2)
        };

        // Main container with all elements
        var mainGrid = new Grid();
        mainGrid.Children.Add(_backgroundBorder);
        mainGrid.Children.Add(_tooltipBorder);
        mainGrid.Children.Add(_effectCanvas);

        _tooltipBorder.Child = _contentGrid;

        return mainGrid;
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
        if (!EnableAnimation || IsInSimplifiedMode || _effectCanvas == null) return;

        _particlePhase += 0.1;
        if (_particlePhase > Math.PI * 2)
            _particlePhase = 0;

        UpdateParticles();
        SpawnTooltipParticles();
    }

    private void OnGlowTick(object sender, EventArgs e)
    {
        if (!EnableGlowEffect || IsInSimplifiedMode) return;

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
            particle.Life -= 0.03;

            // Update visual position
            Canvas.SetLeft(particle.Visual, particle.X);
            Canvas.SetTop(particle.Visual, particle.Y);
            
            // Update opacity
            particle.Visual.Opacity = Math.Max(0, particle.Life) * HolographicIntensity;

            // Remove dead particles
            if (particle.Life <= 0)
            {
                _effectCanvas.Children.Remove(particle.Visual);
                _particles.RemoveAt(i);
            }
        }
    }

    private void SpawnTooltipParticles()
    {
        if (_particles.Count >= 15) return; // Limit particle count

        if (_random.NextDouble() < 0.3) // 30% chance to spawn
        {
            var particle = CreateTooltipParticle();
            _particles.Add(particle);
            _effectCanvas.Children.Add(particle.Visual);
        }
    }

    private TooltipParticle CreateTooltipParticle()
    {
        var color = GetTypeColor(TooltipType);
        var size = 1 + _random.NextDouble() * 2;
        
        var ellipse = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromArgb(150, color.R, color.G, color.B))
        };

        var particle = new TooltipParticle
        {
            Visual = ellipse,
            X = _random.NextDouble() * _tooltipBorder.ActualWidth,
            Y = _tooltipBorder.ActualHeight,
            VelocityX = (_random.NextDouble() - 0.5) * 2,
            VelocityY = -1 - _random.NextDouble() * 3,
            Life = 1.0
        };

        Canvas.SetLeft(ellipse, particle.X);
        Canvas.SetTop(ellipse, particle.Y);

        return particle;
    }

    private void UpdateGlowEffects()
    {
        if (_tooltipBorder?.Effect is DropShadowEffect effect)
        {
            var glowIntensity = 0.5 + (Math.Sin(_glowPhase) * 0.3);
            effect.Opacity = HolographicIntensity * glowIntensity;
        }

        if (_backgroundBorder != null)
        {
            var pulseIntensity = 0.3 + (Math.Sin(_glowPhase * 2) * 0.2);
            _backgroundBorder.Opacity = pulseIntensity * HolographicIntensity;
        }
    }

    private void AnimateShow()
    {
        if (_isShowing) return;
        
        _isShowing = true;
        IsOpen = true;

        // Scale and fade in animation
        var scaleTransform = new ScaleTransform(0.1, 0.1);
        _tooltipBorder.RenderTransform = scaleTransform;
        _tooltipBorder.RenderTransformOrigin = new Point(0.5, 1.0);
        _tooltipBorder.Opacity = 0;

        var scaleAnimation = new DoubleAnimation
        {
            From = 0.1,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(200),
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(150)
        };

        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        _tooltipBorder.BeginAnimation(OpacityProperty, fadeAnimation);

        fadeAnimation.Completed += (s, e) => 
        {
            _isShowing = false;
            TooltipShown?.Invoke(this, new HoloTooltipEventArgs { Tooltip = this });
        };
    }

    private void AnimateHide()
    {
        var scaleAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.1,
            Duration = TimeSpan.FromMilliseconds(150),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(100)
        };

        if (_tooltipBorder.RenderTransform is ScaleTransform transform)
        {
            transform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }

        _tooltipBorder.BeginAnimation(OpacityProperty, fadeAnimation);

        fadeAnimation.Completed += (s, e) => 
        {
            IsOpen = false;
            TooltipHidden?.Invoke(this, new HoloTooltipEventArgs { Tooltip = this });
        };
    }

    private void SetOptimalPosition(Point targetPosition)
    {
        var screenBounds = SystemParameters.WorkArea;
        var tooltipSize = new Size(MaxWidthLimit, 100); // Estimated size

        var preferredX = targetPosition.X + 15;
        var preferredY = targetPosition.Y - tooltipSize.Height - 10;

        // Adjust horizontal position if would go off screen
        if (preferredX + tooltipSize.Width > screenBounds.Right)
        {
            preferredX = targetPosition.X - tooltipSize.Width - 15;
        }

        // Adjust vertical position if would go off screen
        if (preferredY < screenBounds.Top)
        {
            preferredY = targetPosition.Y + 15;
        }

        HorizontalOffset = Math.Max(screenBounds.Left, preferredX);
        VerticalOffset = Math.Max(screenBounds.Top, preferredY);
    }

    private void UpdateTooltipAppearance()
    {
        if (_tooltipBorder == null) return;

        Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            UpdateColors();
            UpdateEffects();
            UpdateIcon();
            UpdateTooltipContent();
        }), DispatcherPriority.Render);
    }

    private void UpdateColors()
    {
        var typeColor = GetTypeColor(TooltipType);
        var eveColor = GetEVEColor(EVEColorScheme);

        if (_tooltipBorder != null)
        {
            var backgroundBrush = new LinearGradientBrush();
            backgroundBrush.StartPoint = new Point(0, 0);
            backgroundBrush.EndPoint = new Point(0, 1);
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(240, 0, 20, 40), 0.0));
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(220, 0, 15, 30), 1.0));

            _tooltipBorder.Background = backgroundBrush;
            _tooltipBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(
                180, typeColor.R, typeColor.G, typeColor.B));
        }

        if (_iconPath != null)
        {
            _iconPath.Fill = new SolidColorBrush(typeColor);
        }

        if (_backgroundBorder != null)
        {
            _backgroundBorder.Background = new SolidColorBrush(Color.FromArgb(
                40, typeColor.R, typeColor.G, typeColor.B));
        }
    }

    private void UpdateEffects()
    {
        var typeColor = GetTypeColor(TooltipType);

        if (_tooltipBorder != null && EnableGlowEffect && !IsInSimplifiedMode)
        {
            _tooltipBorder.Effect = new DropShadowEffect
            {
                Color = typeColor,
                BlurRadius = 12 * HolographicIntensity,
                ShadowDepth = 0,
                Opacity = 0.8 * HolographicIntensity
            };
        }
        else if (_tooltipBorder != null && EnableGlowEffect)
        {
            _tooltipBorder.Effect = new DropShadowEffect
            {
                Color = typeColor,
                BlurRadius = 4,
                ShadowDepth = 0,
                Opacity = 0.4 * HolographicIntensity
            };
        }
        else if (_tooltipBorder != null)
        {
            _tooltipBorder.Effect = null;
        }
    }

    private void UpdateIcon()
    {
        if (_iconPath == null) return;

        _iconPath.Visibility = ShowIcon ? Visibility.Visible : Visibility.Collapsed;
        
        if (ShowIcon)
        {
            _iconPath.Data = GetIconGeometry(TooltipType);
        }
    }

    private void UpdateTooltipContent()
    {
        if (_titleText != null)
        {
            _titleText.Text = TooltipTitle ?? string.Empty;
            _titleText.Visibility = !string.IsNullOrEmpty(TooltipTitle) ? 
                Visibility.Visible : Visibility.Collapsed;
        }

        if (_contentPresenter != null)
        {
            _contentPresenter.Content = TooltipContent;
            _contentPresenter.Visibility = TooltipContent != null ? 
                Visibility.Visible : Visibility.Collapsed;
        }
    }

    private Color GetTypeColor(HoloTooltipType type)
    {
        return type switch
        {
            HoloTooltipType.Information => Color.FromRgb(64, 224, 255),
            HoloTooltipType.Warning => Color.FromRgb(255, 215, 0),
            HoloTooltipType.Error => Color.FromRgb(220, 20, 60),
            HoloTooltipType.Success => Color.FromRgb(50, 205, 50),
            HoloTooltipType.Help => Color.FromRgb(138, 43, 226),
            _ => Color.FromRgb(64, 224, 255)
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

    private Geometry GetIconGeometry(HoloTooltipType type)
    {
        return type switch
        {
            HoloTooltipType.Information => Geometry.Parse("M8,12A4,4 0 0,1 12,8A4,4 0 0,1 16,12A4,4 0 0,1 12,16A4,4 0 0,1 8,12M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M11,10H13V14H11V10M11,6H13V8H11V6Z"),
            HoloTooltipType.Warning => Geometry.Parse("M13,14H11V10H13M13,18H11V16H13M1,21H23L12,2L1,21Z"),
            HoloTooltipType.Error => Geometry.Parse("M19,6.41L17.59,5L12,10.59L6.41,5L5,6.41L10.59,12L5,17.59L6.41,19L12,13.41L17.59,19L19,17.59L13.41,12L19,6.41Z"),
            HoloTooltipType.Success => Geometry.Parse("M9,20.42L2.79,14.21L5.62,11.38L9,14.77L18.88,4.88L21.71,7.71L9,20.42Z"),
            HoloTooltipType.Help => Geometry.Parse("M15.07,11.25L14.17,12.17C13.45,12.89 13,13.5 13,15H11V14.5C11,13.39 11.45,12.39 12.17,11.67L13.41,10.41C13.78,10.05 14,9.55 14,9C14,7.89 13.1,7 12,7A2,2 0 0,0 10,9H8A4,4 0 0,1 12,5A4,4 0 0,1 16,9C16,9.88 15.64,10.67 15.07,11.25M13,19H11V17H13M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12C22,6.47 17.5,2 12,2Z"),
            _ => Geometry.Parse("M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M11,10H13V14H11V10M11,6H13V8H11V6Z")
        };
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableAnimation && !IsInSimplifiedMode)
            _particleTimer.Start();
            
        if (EnableGlowEffect && !IsInSimplifiedMode)
            _glowTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _particleTimer?.Stop();
        _glowTimer?.Stop();
        
        // Clean up particles
        _particles.Clear();
        _effectCanvas?.Children.Clear();
        
        // Unregister from animation intensity manager
        AnimationIntensityManager.Instance.UnregisterTarget(this);
        
        _disposed = true;
    }

    #endregion

    #region Event Handlers

    private void OnOpened(object sender, EventArgs e)
    {
        if (EnableAnimation && !IsInSimplifiedMode && !_isShowing)
        {
            _particleTimer.Start();
        }
        
        if (EnableGlowEffect && !IsInSimplifiedMode)
        {
            _glowTimer.Start();
        }
    }

    private void OnClosed(object sender, EventArgs e)
    {
        _particleTimer?.Stop();
        _glowTimer?.Stop();
        
        // Clean up particles
        _particles.Clear();
        _effectCanvas?.Children.Clear();
    }

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTooltip tooltip)
            tooltip.UpdateTooltipAppearance();
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTooltip tooltip)
            tooltip.UpdateTooltipAppearance();
    }

    private static void OnTooltipTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTooltip tooltip)
            tooltip.UpdateTooltipAppearance();
    }

    private static void OnShowPointerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Pointer visibility logic would be implemented here
    }

    private static void OnEnableAnimationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTooltip tooltip)
        {
            if ((bool)e.NewValue && !tooltip.IsInSimplifiedMode)
                tooltip._particleTimer.Start();
            else
                tooltip._particleTimer.Stop();
        }
    }

    private static void OnEnableGlowEffectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTooltip tooltip)
        {
            tooltip.UpdateTooltipAppearance();
            
            if ((bool)e.NewValue && !tooltip.IsInSimplifiedMode)
                tooltip._glowTimer.Start();
            else
                tooltip._glowTimer.Stop();
        }
    }

    #endregion
}

/// <summary>
/// Tooltip particle for visual effects
/// </summary>
internal class TooltipParticle
{
    public FrameworkElement Visual { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
}

/// <summary>
/// Holographic tooltip types for different visual styles
/// </summary>
public enum HoloTooltipType
{
    Information,
    Warning,
    Error,
    Success,
    Help
}

/// <summary>
/// Event args for holographic tooltip events
/// </summary>
public class HoloTooltipEventArgs : EventArgs
{
    public HoloTooltip Tooltip { get; set; }
}