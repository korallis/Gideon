// ==========================================================================
// HolographicGlassPanel.cs - Advanced Holographic Glass Panel Control
// ==========================================================================
// Custom control that provides dynamic blur effects, transparency management,
// and depth-based rendering for the Westworld-EVE fusion interface.
//
// Features:
// - Dynamic blur radius based on depth
// - Automatic transparency adjustment
// - Performance-aware effect switching
// - Interactive blur transitions
// - Holographic depth perception
//
// Author: Gideon Development Team
// Created: June 21, 2025
// ==========================================================================

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Animation;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Holographic glass panel types for different use cases
/// </summary>
public enum HolographicGlassType
{
    UltraLight,
    Light,
    Medium,
    Strong,
    Background,
    ElectricBlue,
    Gold,
    LowPerformance,
    HighPerformance
}

/// <summary>
/// Performance quality levels for blur effects
/// </summary>
public enum BlurQuality
{
    Low,
    Medium,
    High,
    Ultra
}

/// <summary>
/// Advanced holographic glass panel with dynamic blur and transparency effects
/// </summary>
public class HolographicGlassPanel : Border
{
    #region Dependency Properties

    /// <summary>
    /// Depth level for automatic blur calculation (0.0 = foreground, 1.0 = deep background)
    /// </summary>
    public static readonly DependencyProperty DepthProperty =
        DependencyProperty.Register(nameof(Depth), typeof(double), typeof(HolographicGlassPanel),
            new PropertyMetadata(0.0, OnDepthChanged));

    /// <summary>
    /// Glass panel type for different visual styles
    /// </summary>
    public static readonly DependencyProperty GlassTypeProperty =
        DependencyProperty.Register(nameof(GlassType), typeof(HolographicGlassType), typeof(HolographicGlassPanel),
            new PropertyMetadata(HolographicGlassType.Medium, OnGlassTypeChanged));

    /// <summary>
    /// Blur quality setting for performance optimization
    /// </summary>
    public static readonly DependencyProperty BlurQualityProperty =
        DependencyProperty.Register(nameof(BlurQuality), typeof(BlurQuality), typeof(HolographicGlassPanel),
            new PropertyMetadata(BlurQuality.High, OnBlurQualityChanged));

    /// <summary>
    /// Enable dynamic blur animations
    /// </summary>
    public static readonly DependencyProperty EnableBlurAnimationsProperty =
        DependencyProperty.Register(nameof(EnableBlurAnimations), typeof(bool), typeof(HolographicGlassPanel),
            new PropertyMetadata(true));

    /// <summary>
    /// Enable transparency pulse effect
    /// </summary>
    public static readonly DependencyProperty EnableTransparencyPulseProperty =
        DependencyProperty.Register(nameof(EnableTransparencyPulse), typeof(bool), typeof(HolographicGlassPanel),
            new PropertyMetadata(false, OnTransparencyPulseChanged));

    /// <summary>
    /// Maximum blur radius for depth calculations
    /// </summary>
    public static readonly DependencyProperty MaxBlurRadiusProperty =
        DependencyProperty.Register(nameof(MaxBlurRadius), typeof(double), typeof(HolographicGlassPanel),
            new PropertyMetadata(25.0, OnMaxBlurRadiusChanged));

    /// <summary>
    /// Transparency multiplier for glass effect intensity
    /// </summary>
    public static readonly DependencyProperty TransparencyMultiplierProperty =
        DependencyProperty.Register(nameof(TransparencyMultiplier), typeof(double), typeof(HolographicGlassPanel),
            new PropertyMetadata(1.0, OnTransparencyMultiplierChanged));

    #endregion

    #region Properties

    public double Depth
    {
        get => (double)GetValue(DepthProperty);
        set => SetValue(DepthProperty, value);
    }

    public HolographicGlassType GlassType
    {
        get => (HolographicGlassType)GetValue(GlassTypeProperty);
        set => SetValue(GlassTypeProperty, value);
    }

    public BlurQuality BlurQuality
    {
        get => (BlurQuality)GetValue(BlurQualityProperty);
        set => SetValue(BlurQualityProperty, value);
    }

    public bool EnableBlurAnimations
    {
        get => (bool)GetValue(EnableBlurAnimationsProperty);
        set => SetValue(EnableBlurAnimationsProperty, value);
    }

    public bool EnableTransparencyPulse
    {
        get => (bool)GetValue(EnableTransparencyPulseProperty);
        set => SetValue(EnableTransparencyPulseProperty, value);
    }

    public double MaxBlurRadius
    {
        get => (double)GetValue(MaxBlurRadiusProperty);
        set => SetValue(MaxBlurRadiusProperty, value);
    }

    public double TransparencyMultiplier
    {
        get => (double)GetValue(TransparencyMultiplierProperty);
        set => SetValue(TransparencyMultiplierProperty, value);
    }

    #endregion

    #region Fields

    private BlurEffect? _blurEffect;
    private DropShadowEffect? _shadowEffect;
    private EffectGroup? _effectGroup;
    private Storyboard? _transparencyPulseStoryboard;
    private bool _isLoaded = false;

    #endregion

    #region Constructor

    static HolographicGlassPanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(HolographicGlassPanel),
            new FrameworkPropertyMetadata(typeof(HolographicGlassPanel)));
    }

    public HolographicGlassPanel()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        
        // Set up initial render transform for animations
        RenderTransform = new ScaleTransform(1.0, 1.0);
        RenderTransformOrigin = new Point(0.5, 0.5);
        
        // Enable layout rounding for crisp edges
        UseLayoutRounding = true;
        SnapsToDevicePixels = true;
        
        InitializeEffects();
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _isLoaded = true;
        UpdateGlassAppearance();
        
        if (EnableTransparencyPulse)
        {
            StartTransparencyPulse();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _isLoaded = false;
        StopTransparencyPulse();
    }

    protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        
        if (EnableBlurAnimations && _isLoaded)
        {
            AnimateToFocus();
        }
    }

    protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        
        if (EnableBlurAnimations && _isLoaded)
        {
            AnimateToUnfocus();
        }
    }

    #endregion

    #region Property Change Handlers

    private static void OnDepthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HolographicGlassPanel panel)
        {
            panel.UpdateBlurEffect();
            panel.UpdateTransparency();
        }
    }

    private static void OnGlassTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HolographicGlassPanel panel)
        {
            panel.UpdateGlassAppearance();
        }
    }

    private static void OnBlurQualityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HolographicGlassPanel panel)
        {
            panel.UpdateBlurEffect();
        }
    }

    private static void OnTransparencyPulseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HolographicGlassPanel panel)
        {
            if ((bool)e.NewValue && panel._isLoaded)
            {
                panel.StartTransparencyPulse();
            }
            else
            {
                panel.StopTransparencyPulse();
            }
        }
    }

    private static void OnMaxBlurRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HolographicGlassPanel panel)
        {
            panel.UpdateBlurEffect();
        }
    }

    private static void OnTransparencyMultiplierChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HolographicGlassPanel panel)
        {
            panel.UpdateTransparency();
        }
    }

    #endregion

    #region Private Methods

    private void InitializeEffects()
    {
        _blurEffect = new BlurEffect { Radius = 0 };
        _shadowEffect = new DropShadowEffect
        {
            Color = Colors.Transparent,
            BlurRadius = 0,
            ShadowDepth = 0,
            Opacity = 0
        };
        
        _effectGroup = new EffectGroup();
        _effectGroup.Children.Add(_blurEffect);
        _effectGroup.Children.Add(_shadowEffect);
        
        Effect = _effectGroup;
    }

    private void UpdateGlassAppearance()
    {
        if (!_isLoaded) return;

        // Update background based on glass type
        Background = GlassType switch
        {
            HolographicGlassType.UltraLight => FindResource("HoloGlass_UltraLight") as Brush,
            HolographicGlassType.Light => FindResource("HoloGlass_Light") as Brush,
            HolographicGlassType.Medium => FindResource("HoloGlass_Medium") as Brush,
            HolographicGlassType.Strong => FindResource("HoloGlass_Strong") as Brush,
            HolographicGlassType.Background => FindResource("HoloGlass_VeryStrong") as Brush,
            HolographicGlassType.ElectricBlue => FindResource("HoloGlass_ElectricBlue_Medium") as Brush,
            HolographicGlassType.Gold => FindResource("HoloGlass_Gold_Medium") as Brush,
            HolographicGlassType.LowPerformance => FindResource("HoloGlass_Light") as Brush,
            HolographicGlassType.HighPerformance => FindResource("HoloGlass_Medium") as Brush,
            _ => FindResource("HoloGlass_Medium") as Brush
        };

        // Update border based on glass type
        BorderBrush = GlassType switch
        {
            HolographicGlassType.ElectricBlue => FindResource("ElectricBlue_CoreBrush") as Brush,
            HolographicGlassType.Gold => FindResource("EVEGold_CoreBrush") as Brush,
            HolographicGlassType.HighPerformance => FindResource("ElectricBlue_HorizontalGradient") as Brush,
            _ => FindResource("ElectricBlue_Alpha50") as Brush
        };

        UpdateBlurEffect();
        UpdateTransparency();
    }

    private void UpdateBlurEffect()
    {
        if (_blurEffect == null) return;

        double blurRadius = CalculateBlurRadius();
        
        if (EnableBlurAnimations && _isLoaded)
        {
            AnimateBlurRadius(blurRadius);
        }
        else
        {
            _blurEffect.Radius = blurRadius;
        }

        // Update shadow effect for holographic appearance
        if (_shadowEffect != null && GlassType != HolographicGlassType.LowPerformance)
        {
            _shadowEffect.Color = GlassType switch
            {
                HolographicGlassType.ElectricBlue => Color.FromRgb(0, 212, 255),
                HolographicGlassType.Gold => Color.FromRgb(255, 215, 0),
                _ => Color.FromRgb(0, 212, 255)
            };
            
            _shadowEffect.BlurRadius = Math.Min(blurRadius * 0.5, 12);
            _shadowEffect.Opacity = Math.Max(0, 0.6 - (Depth * 0.4));
        }
    }

    private double CalculateBlurRadius()
    {
        double baseBlur = GlassType switch
        {
            HolographicGlassType.UltraLight => 0,
            HolographicGlassType.Light => 2,
            HolographicGlassType.Medium => 4,
            HolographicGlassType.Strong => 8,
            HolographicGlassType.Background => 15,
            HolographicGlassType.ElectricBlue => 3,
            HolographicGlassType.Gold => 3,
            HolographicGlassType.LowPerformance => 0,
            HolographicGlassType.HighPerformance => 6,
            _ => 4
        };

        // Apply depth-based blur scaling
        double depthBlur = Depth * MaxBlurRadius;
        
        // Apply quality multiplier
        double qualityMultiplier = BlurQuality switch
        {
            BlurQuality.Low => 0.5,
            BlurQuality.Medium => 0.75,
            BlurQuality.High => 1.0,
            BlurQuality.Ultra => 1.25,
            _ => 1.0
        };

        return Math.Max(0, (baseBlur + depthBlur) * qualityMultiplier);
    }

    private void UpdateTransparency()
    {
        if (!_isLoaded) return;

        // Calculate opacity based on depth and glass type
        double baseOpacity = GlassType switch
        {
            HolographicGlassType.UltraLight => 1.0,
            HolographicGlassType.Light => 0.95,
            HolographicGlassType.Medium => 0.9,
            HolographicGlassType.Strong => 0.85,
            HolographicGlassType.Background => 0.7,
            HolographicGlassType.ElectricBlue => 0.9,
            HolographicGlassType.Gold => 0.9,
            HolographicGlassType.LowPerformance => 0.95,
            HolographicGlassType.HighPerformance => 0.85,
            _ => 0.9
        };

        // Apply depth-based transparency (further = more transparent)
        double depthOpacity = Math.Max(0.3, 1.0 - (Depth * 0.5));
        
        // Combine and apply multiplier
        double finalOpacity = Math.Min(1.0, baseOpacity * depthOpacity * TransparencyMultiplier);
        
        if (EnableBlurAnimations && _isLoaded)
        {
            AnimateOpacity(finalOpacity);
        }
        else
        {
            Opacity = finalOpacity;
        }
    }

    private void AnimateBlurRadius(double targetRadius)
    {
        if (_blurEffect == null) return;

        var animation = new DoubleAnimation
        {
            To = targetRadius,
            Duration = TimeSpan.FromSeconds(0.3),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        _blurEffect.BeginAnimation(BlurEffect.RadiusProperty, animation);
    }

    private void AnimateOpacity(double targetOpacity)
    {
        var animation = new DoubleAnimation
        {
            To = targetOpacity,
            Duration = TimeSpan.FromSeconds(0.2),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        BeginAnimation(OpacityProperty, animation);
    }

    private void AnimateToFocus()
    {
        // Reduce blur when focused
        double focusBlurRadius = Math.Max(0, CalculateBlurRadius() - 3);
        AnimateBlurRadius(focusBlurRadius);
        
        // Increase opacity slightly
        AnimateOpacity(Math.Min(1.0, Opacity + 0.1));
        
        // Scale up slightly for interaction feedback
        var scaleAnimation = new DoubleAnimation
        {
            To = 1.02,
            Duration = TimeSpan.FromSeconds(0.15),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        if (RenderTransform is ScaleTransform transform)
        {
            transform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }
    }

    private void AnimateToUnfocus()
    {
        // Restore original blur
        AnimateBlurRadius(CalculateBlurRadius());
        
        // Restore original opacity
        UpdateTransparency();
        
        // Scale back to normal
        var scaleAnimation = new DoubleAnimation
        {
            To = 1.0,
            Duration = TimeSpan.FromSeconds(0.2),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        if (RenderTransform is ScaleTransform transform)
        {
            transform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }
    }

    private void StartTransparencyPulse()
    {
        if (_transparencyPulseStoryboard != null) return;

        _transparencyPulseStoryboard = new Storyboard
        {
            RepeatBehavior = RepeatBehavior.Forever,
            AutoReverse = true
        };

        var pulseAnimation = new DoubleAnimation
        {
            From = Opacity * 0.8,
            To = Math.Min(1.0, Opacity * 1.1),
            Duration = TimeSpan.FromSeconds(2.5),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        Storyboard.SetTarget(pulseAnimation, this);
        Storyboard.SetTargetProperty(pulseAnimation, new PropertyPath(OpacityProperty));
        
        _transparencyPulseStoryboard.Children.Add(pulseAnimation);
        _transparencyPulseStoryboard.Begin();
    }

    private void StopTransparencyPulse()
    {
        if (_transparencyPulseStoryboard != null)
        {
            _transparencyPulseStoryboard.Stop();
            _transparencyPulseStoryboard = null;
        }
    }

    private object? FindResource(string resourceKey)
    {
        try
        {
            return TryFindResource(resourceKey) ?? Application.Current?.TryFindResource(resourceKey);
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Updates all glass effects based on current settings
    /// </summary>
    public void RefreshGlassEffects()
    {
        UpdateGlassAppearance();
    }

    /// <summary>
    /// Sets the depth with smooth animation
    /// </summary>
    public void AnimateToDepth(double targetDepth, double durationSeconds = 0.5)
    {
        var animation = new DoubleAnimation
        {
            To = targetDepth,
            Duration = TimeSpan.FromSeconds(durationSeconds),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };

        BeginAnimation(DepthProperty, animation);
    }

    /// <summary>
    /// Applies a temporary glow effect
    /// </summary>
    public void TriggerGlowEffect(Color glowColor, double duration = 1.0)
    {
        if (_shadowEffect == null) return;

        var colorAnimation = new ColorAnimation
        {
            To = glowColor,
            Duration = TimeSpan.FromSeconds(duration * 0.3),
            AutoReverse = true,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var opacityAnimation = new DoubleAnimation
        {
            To = 0.8,
            Duration = TimeSpan.FromSeconds(duration * 0.3),
            AutoReverse = true,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        _shadowEffect.BeginAnimation(DropShadowEffect.ColorProperty, colorAnimation);
        _shadowEffect.BeginAnimation(DropShadowEffect.OpacityProperty, opacityAnimation);
    }

    #endregion
}