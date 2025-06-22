// ==========================================================================
// HoloButton.cs - Holographic Button with Advanced Effects and Animations
// ==========================================================================
// Advanced holographic button control with multi-state animations, glowing
// effects, EVE-style interactions, and adaptive performance rendering.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Advanced holographic button with EVE-style effects and animations
/// </summary>
public class HoloButton : Button, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloButton),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloButton),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty ButtonStateProperty =
        DependencyProperty.Register(nameof(ButtonState), typeof(HoloButtonState), typeof(HoloButton),
            new PropertyMetadata(HoloButtonState.Normal, OnButtonStateChanged));

    public static readonly DependencyProperty EnablePulseAnimationProperty =
        DependencyProperty.Register(nameof(EnablePulseAnimation), typeof(bool), typeof(HoloButton),
            new PropertyMetadata(true, OnEnablePulseAnimationChanged));

    public static readonly DependencyProperty EnableScanlineEffectProperty =
        DependencyProperty.Register(nameof(EnableScanlineEffect), typeof(bool), typeof(HoloButton),
            new PropertyMetadata(true, OnEnableScanlineEffectChanged));

    public static readonly DependencyProperty EnableClickAnimationProperty =
        DependencyProperty.Register(nameof(EnableClickAnimation), typeof(bool), typeof(HoloButton),
            new PropertyMetadata(true));

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(HoloButton),
            new PropertyMetadata(new CornerRadius(6), OnCornerRadiusChanged));

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(FrameworkElement), typeof(HoloButton),
            new PropertyMetadata(null));

    public static readonly DependencyProperty IconPositionProperty =
        DependencyProperty.Register(nameof(IconPosition), typeof(IconPosition), typeof(HoloButton),
            new PropertyMetadata(IconPosition.Left));

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

    public HoloButtonState ButtonState
    {
        get => (HoloButtonState)GetValue(ButtonStateProperty);
        set => SetValue(ButtonStateProperty, value);
    }

    public bool EnablePulseAnimation
    {
        get => (bool)GetValue(EnablePulseAnimationProperty);
        set => SetValue(EnablePulseAnimationProperty, value);
    }

    public bool EnableScanlineEffect
    {
        get => (bool)GetValue(EnableScanlineEffectProperty);
        set => SetValue(EnableScanlineEffectProperty, value);
    }

    public bool EnableClickAnimation
    {
        get => (bool)GetValue(EnableClickAnimationProperty);
        set => SetValue(EnableClickAnimationProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public FrameworkElement Icon
    {
        get => (FrameworkElement)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public IconPosition IconPosition
    {
        get => (IconPosition)GetValue(IconPositionProperty);
        set => SetValue(IconPositionProperty, value);
    }

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode { get; private set; }

    public void SwitchToFullMode()
    {
        IsInSimplifiedMode = false;
        EnablePulseAnimation = true;
        EnableScanlineEffect = true;
        EnableClickAnimation = true;
        UpdateButtonAppearance();
    }

    public void SwitchToSimplifiedMode()
    {
        IsInSimplifiedMode = true;
        EnablePulseAnimation = false;
        EnableScanlineEffect = false;
        EnableClickAnimation = false;
        UpdateButtonAppearance();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => !_disposed && IsLoaded;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        EnablePulseAnimation = settings.EnabledFeatures.HasFlag(AnimationFeatures.BasicGlow);
        EnableScanlineEffect = settings.EnabledFeatures.HasFlag(AnimationFeatures.AdvancedShaders);
        EnableClickAnimation = settings.EnabledFeatures.HasFlag(AnimationFeatures.MotionEffects);
        
        UpdateButtonAppearance();
    }

    #endregion

    #region Fields

    private Border _mainBorder;
    private Border _glowBorder;
    private Rectangle _scanlineLayer;
    private Grid _contentGrid;
    private ContentPresenter _contentPresenter;
    private ContentPresenter _iconPresenter;
    private DispatcherTimer _pulseTimer;
    private Storyboard _hoverAnimation;
    private Storyboard _pressAnimation;
    private Storyboard _pulseAnimation;
    private double _pulsePhase = 0;
    private bool _isPressed = false;
    private bool _disposed = false;

    #endregion

    #region Constructor

    public HoloButton()
    {
        DefaultStyleKey = typeof(HoloButton);
        InitializeButton();
        SetupAnimations();
        
        // Register with animation intensity manager
        AnimationIntensityManager.Instance.RegisterTarget(this);
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Private Methods

    private void InitializeButton()
    {
        Template = CreateButtonTemplate();
        UpdateButtonAppearance();
        
        // Wire up interaction events
        MouseEnter += OnMouseEnter;
        MouseLeave += OnMouseLeave;
        PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
        PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
    }

    private ControlTemplate CreateButtonTemplate()
    {
        var template = new ControlTemplate(typeof(HoloButton));

        // Main grid container
        var mainGrid = new FrameworkElementFactory(typeof(Grid));

        // Glow border (background layer)
        var glowBorder = new FrameworkElementFactory(typeof(Border));
        glowBorder.Name = "PART_GlowBorder";
        glowBorder.SetValue(Border.CornerRadiusProperty, new TemplateBindingExtension(CornerRadiusProperty));
        glowBorder.SetValue(Border.OpacityProperty, 0.0);

        // Main border
        var mainBorder = new FrameworkElementFactory(typeof(Border));
        mainBorder.Name = "PART_MainBorder";
        mainBorder.SetValue(Border.CornerRadiusProperty, new TemplateBindingExtension(CornerRadiusProperty));
        mainBorder.SetValue(Border.BorderThicknessProperty, new Thickness(1));
        mainBorder.SetValue(Border.PaddingProperty, new TemplateBindingExtension(PaddingProperty));

        // Content grid for layout
        var contentGrid = new FrameworkElementFactory(typeof(Grid));
        contentGrid.Name = "PART_ContentGrid";

        // Column definitions for icon and content
        var column1 = new FrameworkElementFactory(typeof(ColumnDefinition));
        column1.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);
        var column2 = new FrameworkElementFactory(typeof(ColumnDefinition));
        column2.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
        var column3 = new FrameworkElementFactory(typeof(ColumnDefinition));
        column3.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);
        
        contentGrid.AppendChild(column1);
        contentGrid.AppendChild(column2);
        contentGrid.AppendChild(column3);

        // Icon presenter
        var iconPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
        iconPresenter.Name = "PART_IconPresenter";
        iconPresenter.SetValue(ContentPresenter.ContentProperty, new TemplateBindingExtension(IconProperty));
        iconPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
        iconPresenter.SetValue(ContentPresenter.MarginProperty, new Thickness(0, 0, 8, 0));
        iconPresenter.SetValue(Grid.ColumnProperty, 0);

        // Content presenter
        var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
        contentPresenter.Name = "PART_ContentPresenter";
        contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
        contentPresenter.SetValue(Grid.ColumnProperty, 1);

        // Scanline layer
        var scanlineLayer = new FrameworkElementFactory(typeof(Rectangle));
        scanlineLayer.Name = "PART_ScanlineLayer";
        scanlineLayer.SetValue(Rectangle.RadiusXProperty, new TemplateBindingExtension(CornerRadiusProperty));
        scanlineLayer.SetValue(Rectangle.RadiusYProperty, new TemplateBindingExtension(CornerRadiusProperty));
        scanlineLayer.SetValue(FrameworkElement.IsHitTestVisibleProperty, false);
        scanlineLayer.SetValue(FrameworkElement.OpacityProperty, 0.0);

        // Assembly
        contentGrid.AppendChild(iconPresenter);
        contentGrid.AppendChild(contentPresenter);
        mainBorder.AppendChild(contentGrid);
        
        mainGrid.AppendChild(glowBorder);
        mainGrid.AppendChild(mainBorder);
        mainGrid.AppendChild(scanlineLayer);

        template.VisualTree = mainGrid;
        return template;
    }

    private void SetupAnimations()
    {
        // Pulse timer for idle animation
        _pulseTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50) // 20 FPS for smooth pulse
        };
        _pulseTimer.Tick += OnPulseTick;

        CreateHoverAnimation();
        CreatePressAnimation();
        CreatePulseAnimation();
    }

    private void CreateHoverAnimation()
    {
        _hoverAnimation = new Storyboard();

        // Scale animation
        var scaleXAnimation = new DoubleAnimation
        {
            To = 1.05,
            Duration = TimeSpan.FromMilliseconds(150),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("RenderTransform.ScaleX"));

        var scaleYAnimation = new DoubleAnimation
        {
            To = 1.05,
            Duration = TimeSpan.FromMilliseconds(150),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("RenderTransform.ScaleY"));

        // Glow animation
        var glowAnimation = new DoubleAnimation
        {
            To = 0.8,
            Duration = TimeSpan.FromMilliseconds(150),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTargetProperty(glowAnimation, new PropertyPath("Opacity"));

        _hoverAnimation.Children.Add(scaleXAnimation);
        _hoverAnimation.Children.Add(scaleYAnimation);
        _hoverAnimation.Children.Add(glowAnimation);
    }

    private void CreatePressAnimation()
    {
        _pressAnimation = new Storyboard();

        // Quick scale down
        var scaleXAnimation = new DoubleAnimation
        {
            To = 0.95,
            Duration = TimeSpan.FromMilliseconds(80),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("RenderTransform.ScaleX"));

        var scaleYAnimation = new DoubleAnimation
        {
            To = 0.95,
            Duration = TimeSpan.FromMilliseconds(80),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("RenderTransform.ScaleY"));

        _pressAnimation.Children.Add(scaleXAnimation);
        _pressAnimation.Children.Add(scaleYAnimation);
    }

    private void CreatePulseAnimation()
    {
        _pulseAnimation = new Storyboard();

        var pulseOpacityAnimation = new DoubleAnimation
        {
            From = 0.3,
            To = 0.7,
            Duration = TimeSpan.FromSeconds(2),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        Storyboard.SetTargetProperty(pulseOpacityAnimation, new PropertyPath("Opacity"));

        _pulseAnimation.Children.Add(pulseOpacityAnimation);
    }

    private void OnPulseTick(object sender, EventArgs e)
    {
        if (!EnablePulseAnimation || IsInSimplifiedMode) return;

        _pulsePhase += 0.1;
        if (_pulsePhase > Math.PI * 2)
            _pulsePhase = 0;

        UpdatePulseEffects();
    }

    private void UpdatePulseEffects()
    {
        if (_glowBorder?.Effect is DropShadowEffect effect)
        {
            var pulseIntensity = 0.6 + (Math.Sin(_pulsePhase) * 0.2);
            effect.Opacity = HolographicIntensity * pulseIntensity * 0.5;
        }
    }

    private void UpdateButtonAppearance()
    {
        if (Template == null) return;

        Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            ApplyTemplate();
            
            _mainBorder = GetTemplateChild("PART_MainBorder") as Border;
            _glowBorder = GetTemplateChild("PART_GlowBorder") as Border;
            _scanlineLayer = GetTemplateChild("PART_ScanlineLayer") as Rectangle;
            _contentGrid = GetTemplateChild("PART_ContentGrid") as Grid;
            _contentPresenter = GetTemplateChild("PART_ContentPresenter") as ContentPresenter;
            _iconPresenter = GetTemplateChild("PART_IconPresenter") as ContentPresenter;

            UpdateColors();
            UpdateEffects();
            UpdateLayout();
            SetupRenderTransform();
        }), DispatcherPriority.Render);
    }

    private void UpdateColors()
    {
        var color = GetEVEColor(EVEColorScheme);
        var stateMultiplier = GetStateMultiplier(ButtonState);

        if (_mainBorder != null)
        {
            // Background gradient
            var backgroundBrush = new LinearGradientBrush();
            backgroundBrush.StartPoint = new Point(0, 0);
            backgroundBrush.EndPoint = new Point(0, 1);
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb((byte)(60 * stateMultiplier), color.R, color.G, color.B), 0.0));
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb((byte)(40 * stateMultiplier), color.R, color.G, color.B), 0.5));
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb((byte)(80 * stateMultiplier), color.R, color.G, color.B), 1.0));

            _mainBorder.Background = backgroundBrush;

            // Border brush
            var borderBrush = new SolidColorBrush(Color.FromArgb(
                (byte)(180 * stateMultiplier), color.R, color.G, color.B));
            _mainBorder.BorderBrush = borderBrush;
        }

        if (_glowBorder != null)
        {
            var glowBrush = new SolidColorBrush(Color.FromArgb(
                (byte)(100 * stateMultiplier), color.R, color.G, color.B));
            _glowBorder.Background = glowBrush;
        }

        // Update text color
        Foreground = new SolidColorBrush(Color.FromArgb(
            (byte)(255 * stateMultiplier), color.R, color.G, color.B));
    }

    private void UpdateEffects()
    {
        var color = GetEVEColor(EVEColorScheme);
        var stateMultiplier = GetStateMultiplier(ButtonState);

        if (_mainBorder != null && !IsInSimplifiedMode)
        {
            _mainBorder.Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 8 * HolographicIntensity,
                ShadowDepth = 0,
                Opacity = 0.4 * HolographicIntensity * stateMultiplier
            };
        }
        else if (_mainBorder != null)
        {
            _mainBorder.Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 2,
                ShadowDepth = 0,
                Opacity = 0.2 * HolographicIntensity
            };
        }

        if (_glowBorder != null && !IsInSimplifiedMode)
        {
            _glowBorder.Effect = new BlurEffect
            {
                Radius = 15,
                RenderingBias = RenderingBias.Performance
            };
        }

        UpdateScanlineEffect();
    }

    private void UpdateScanlineEffect()
    {
        if (_scanlineLayer == null || !EnableScanlineEffect || IsInSimplifiedMode) return;

        var scanlineBrush = new LinearGradientBrush();
        scanlineBrush.StartPoint = new Point(0, 0);
        scanlineBrush.EndPoint = new Point(0, 1);

        for (int i = 0; i < 10; i++)
        {
            var position = i / 10.0;
            var alpha = (i % 2 == 0) ? 30 : 10;
            scanlineBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(alpha, 255, 255, 255), position));
        }

        _scanlineLayer.Fill = scanlineBrush;
    }

    private void UpdateLayout()
    {
        if (_contentGrid == null || _iconPresenter == null) return;

        // Adjust layout based on icon position
        switch (IconPosition)
        {
            case IconPosition.Left:
                Grid.SetColumn(_iconPresenter, 0);
                Grid.SetColumn(_contentPresenter, 1);
                _iconPresenter.Margin = new Thickness(0, 0, Icon != null ? 8 : 0, 0);
                break;
            case IconPosition.Right:
                Grid.SetColumn(_iconPresenter, 2);
                Grid.SetColumn(_contentPresenter, 1);
                _iconPresenter.Margin = new Thickness(Icon != null ? 8 : 0, 0, 0, 0);
                break;
            case IconPosition.Top:
                // Would need row-based layout for this
                break;
            case IconPosition.Bottom:
                // Would need row-based layout for this
                break;
        }

        _iconPresenter.Visibility = Icon != null ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SetupRenderTransform()
    {
        if (RenderTransform == null || !(RenderTransform is ScaleTransform))
        {
            RenderTransform = new ScaleTransform(1, 1);
            RenderTransformOrigin = new Point(0.5, 0.5);
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

    private double GetStateMultiplier(HoloButtonState state)
    {
        return state switch
        {
            HoloButtonState.Normal => 1.0,
            HoloButtonState.Alert => 1.3,
            HoloButtonState.Warning => 1.1,
            HoloButtonState.Success => 0.9,
            HoloButtonState.Disabled => 0.4,
            _ => 1.0
        };
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnablePulseAnimation && !IsInSimplifiedMode)
            _pulseTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _pulseTimer?.Stop();
        _hoverAnimation?.Stop();
        _pressAnimation?.Stop();
        _pulseAnimation?.Stop();
        
        // Unregister from animation intensity manager
        AnimationIntensityManager.Instance.UnregisterTarget(this);
        
        _disposed = true;
    }

    #endregion

    #region Event Handlers

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (!IsEnabled || IsInSimplifiedMode) return;

        _hoverAnimation?.Stop();
        
        if (_glowBorder != null)
        {
            Storyboard.SetTarget(_hoverAnimation.Children[2], _glowBorder);
            _hoverAnimation.Begin();
        }

        if (RenderTransform is ScaleTransform)
        {
            Storyboard.SetTarget(_hoverAnimation.Children[0], this);
            Storyboard.SetTarget(_hoverAnimation.Children[1], this);
        }
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (!IsEnabled || IsInSimplifiedMode) return;

        _hoverAnimation?.Stop();
        
        // Return to normal state
        if (RenderTransform is ScaleTransform transform)
        {
            var returnScale = new DoubleAnimation
            {
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(150)
            };
            transform.BeginAnimation(ScaleTransform.ScaleXProperty, returnScale);
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, returnScale);
        }

        if (_glowBorder != null)
        {
            var returnGlow = new DoubleAnimation
            {
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(150)
            };
            _glowBorder.BeginAnimation(OpacityProperty, returnGlow);
        }
    }

    private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!IsEnabled || !EnableClickAnimation || IsInSimplifiedMode) return;

        _isPressed = true;
        _pressAnimation?.Stop();
        
        if (RenderTransform is ScaleTransform)
        {
            Storyboard.SetTarget(_pressAnimation.Children[0], this);
            Storyboard.SetTarget(_pressAnimation.Children[1], this);
            _pressAnimation.Begin();
        }
    }

    private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!IsEnabled || !EnableClickAnimation || IsInSimplifiedMode) return;

        _isPressed = false;
        
        // Spring back to hover state
        if (RenderTransform is ScaleTransform transform)
        {
            var returnScale = new DoubleAnimation
            {
                To = IsMouseOver ? 1.05 : 1.0,
                Duration = TimeSpan.FromMilliseconds(100),
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
            };
            transform.BeginAnimation(ScaleTransform.ScaleXProperty, returnScale);
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, returnScale);
        }
    }

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloButton button)
            button.UpdateButtonAppearance();
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloButton button)
            button.UpdateButtonAppearance();
    }

    private static void OnButtonStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloButton button)
            button.UpdateButtonAppearance();
    }

    private static void OnEnablePulseAnimationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloButton button)
        {
            if ((bool)e.NewValue && !button.IsInSimplifiedMode)
                button._pulseTimer.Start();
            else
                button._pulseTimer.Stop();
        }
    }

    private static void OnEnableScanlineEffectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloButton button)
            button.UpdateButtonAppearance();
    }

    private static void OnCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloButton button)
            button.UpdateButtonAppearance();
    }

    #endregion
}

/// <summary>
/// Holographic button states with different visual intensities
/// </summary>
public enum HoloButtonState
{
    Normal,
    Alert,      // High intensity, typically red
    Warning,    // Medium intensity, typically yellow
    Success,    // Calm intensity, typically green
    Disabled    // Low intensity, muted colors
}

/// <summary>
/// Icon positions for holographic buttons
/// </summary>
public enum IconPosition
{
    Left,
    Right,
    Top,
    Bottom
}