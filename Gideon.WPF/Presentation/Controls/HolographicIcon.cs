// ==========================================================================
// HolographicIcon.cs - Holographic Glyph Icon Control
// ==========================================================================
// Advanced holographic icon control for the Westworld-EVE fusion interface.
// Provides dynamic glyph rendering with holographic effects, animations,
// and state management for interactive UI elements.
//
// Features:
// - Dynamic glyph selection from icon library
// - Holographic glow effects with intensity control
// - Animation states (idle, hover, active, pulse)
// - Size scaling and responsive design
// - Performance-optimized rendering
// - Accessibility support
//
// Author: Gideon Development Team
// Created: June 21, 2025
// ==========================================================================

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Icon glyph types for the holographic system
/// </summary>
public enum HolographicIconType
{
    // Navigation & Interface
    Menu,
    Close,
    Settings,
    Search,
    
    // Ship Modules
    WeaponTurret,
    WeaponMissile,
    Shield,
    Armor,
    Capacitor,
    
    // Ship Types
    ShipFrigate,
    ShipCruiser,
    ShipBattleship,
    
    // Stations & Infrastructure
    Station,
    Market,
    Industry,
    
    // Characters & Organizations
    Character,
    Corporation,
    Alliance
}

/// <summary>
/// Icon state for visual feedback
/// </summary>
public enum HolographicIconState
{
    Normal,
    Hover,
    Active,
    Disabled,
    Warning,
    Success,
    Error
}

/// <summary>
/// Icon size presets
/// </summary>
public enum HolographicIconSize
{
    Small = 16,
    Medium = 24,
    Large = 32,
    XLarge = 48,
    Custom = 0
}

/// <summary>
/// Holographic icon control with dynamic glyph rendering and effects
/// </summary>
public class HolographicIcon : Control
{
    #region Dependency Properties

    /// <summary>
    /// The type of icon glyph to display
    /// </summary>
    public static readonly DependencyProperty IconTypeProperty =
        DependencyProperty.Register(nameof(IconType), typeof(HolographicIconType), typeof(HolographicIcon),
            new PropertyMetadata(HolographicIconType.Menu, OnIconTypeChanged));

    /// <summary>
    /// The current state of the icon
    /// </summary>
    public static readonly DependencyProperty IconStateProperty =
        DependencyProperty.Register(nameof(IconState), typeof(HolographicIconState), typeof(HolographicIcon),
            new PropertyMetadata(HolographicIconState.Normal, OnIconStateChanged));

    /// <summary>
    /// The size preset for the icon
    /// </summary>
    public static readonly DependencyProperty IconSizeProperty =
        DependencyProperty.Register(nameof(IconSize), typeof(HolographicIconSize), typeof(HolographicIcon),
            new PropertyMetadata(HolographicIconSize.Medium, OnIconSizeChanged));

    /// <summary>
    /// Custom icon size (when IconSize is Custom)
    /// </summary>
    public static readonly DependencyProperty CustomSizeProperty =
        DependencyProperty.Register(nameof(CustomSize), typeof(double), typeof(HolographicIcon),
            new PropertyMetadata(24.0, OnCustomSizeChanged));

    /// <summary>
    /// Glow intensity multiplier
    /// </summary>
    public static readonly DependencyProperty GlowIntensityProperty =
        DependencyProperty.Register(nameof(GlowIntensity), typeof(double), typeof(HolographicIcon),
            new PropertyMetadata(1.0, OnGlowIntensityChanged));

    /// <summary>
    /// Enable pulse animation
    /// </summary>
    public static readonly DependencyProperty EnablePulseProperty =
        DependencyProperty.Register(nameof(EnablePulse), typeof(bool), typeof(HolographicIcon),
            new PropertyMetadata(false, OnEnablePulseChanged));

    /// <summary>
    /// Interactive mode (responds to mouse events)
    /// </summary>
    public static readonly DependencyProperty IsInteractiveProperty =
        DependencyProperty.Register(nameof(IsInteractive), typeof(bool), typeof(HolographicIcon),
            new PropertyMetadata(false, OnIsInteractiveChanged));

    /// <summary>
    /// Primary glow color
    /// </summary>
    public static readonly DependencyProperty GlowColorProperty =
        DependencyProperty.Register(nameof(GlowColor), typeof(Color), typeof(HolographicIcon),
            new PropertyMetadata(Colors.Cyan, OnGlowColorChanged));

    /// <summary>
    /// Icon stroke color
    /// </summary>
    public static readonly DependencyProperty StrokeColorProperty =
        DependencyProperty.Register(nameof(StrokeColor), typeof(Color), typeof(HolographicIcon),
            new PropertyMetadata(Colors.Cyan, OnStrokeColorChanged));

    /// <summary>
    /// Stroke thickness
    /// </summary>
    public static readonly DependencyProperty StrokeThicknessProperty =
        DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(HolographicIcon),
            new PropertyMetadata(2.0, OnStrokeThicknessChanged));

    #endregion

    #region Properties

    public HolographicIconType IconType
    {
        get => (HolographicIconType)GetValue(IconTypeProperty);
        set => SetValue(IconTypeProperty, value);
    }

    public HolographicIconState IconState
    {
        get => (HolographicIconState)GetValue(IconStateProperty);
        set => SetValue(IconStateProperty, value);
    }

    public HolographicIconSize IconSize
    {
        get => (HolographicIconSize)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public double CustomSize
    {
        get => (double)GetValue(CustomSizeProperty);
        set => SetValue(CustomSizeProperty, value);
    }

    public double GlowIntensity
    {
        get => (double)GetValue(GlowIntensityProperty);
        set => SetValue(GlowIntensityProperty, value);
    }

    public bool EnablePulse
    {
        get => (bool)GetValue(EnablePulseProperty);
        set => SetValue(EnablePulseProperty, value);
    }

    public bool IsInteractive
    {
        get => (bool)GetValue(IsInteractiveProperty);
        set => SetValue(IsInteractiveProperty, value);
    }

    public Color GlowColor
    {
        get => (Color)GetValue(GlowColorProperty);
        set => SetValue(GlowColorProperty, value);
    }

    public Color StrokeColor
    {
        get => (Color)GetValue(StrokeColorProperty);
        set => SetValue(StrokeColorProperty, value);
    }

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    #endregion

    #region Fields

    private Path? _iconPath;
    private Viewbox? _container;
    private DropShadowEffect? _glowEffect;
    private Storyboard? _pulseStoryboard;
    private Storyboard? _hoverStoryboard;
    private bool _isLoaded = false;

    #endregion

    #region Constructor

    static HolographicIcon()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(HolographicIcon),
            new FrameworkPropertyMetadata(typeof(HolographicIcon)));
    }

    public HolographicIcon()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        
        // Set up default properties
        Focusable = false;
        Background = Brushes.Transparent;
        
        // Initialize render transform
        RenderTransform = new ScaleTransform(1.0, 1.0);
        RenderTransformOrigin = new Point(0.5, 0.5);
    }

    #endregion

    #region Overrides

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        
        CreateIconElements();
        if (_isLoaded)
        {
            UpdateIconAppearance();
        }
    }

    protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        
        if (IsInteractive && IconState != HolographicIconState.Disabled)
        {
            IconState = HolographicIconState.Hover;
            StartHoverAnimation(true);
        }
    }

    protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        
        if (IsInteractive && IconState == HolographicIconState.Hover)
        {
            IconState = HolographicIconState.Normal;
            StartHoverAnimation(false);
        }
    }

    protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        
        if (IsInteractive && IconState != HolographicIconState.Disabled)
        {
            IconState = HolographicIconState.Active;
            TriggerActivationEffect();
        }
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _isLoaded = true;
        UpdateIconAppearance();
        
        if (EnablePulse)
        {
            StartPulseAnimation();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _isLoaded = false;
        StopAllAnimations();
    }

    #endregion

    #region Property Change Handlers

    private static void OnIconTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HolographicIcon icon)
        {
            icon.UpdateIconGeometry();
        }
    }

    private static void OnIconStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HolographicIcon icon)
        {
            icon.UpdateIconState();
        }
    }

    private static void OnIconSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HolographicIcon icon)
        {
            icon.UpdateIconSize();
        }
    }

    private static void OnCustomSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HolographicIcon icon && icon.IconSize == HolographicIconSize.Custom)
        {
            icon.UpdateIconSize();
        }
    }

    private static void OnGlowIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HolographicIcon icon)
        {
            icon.UpdateGlowEffect();
        }
    }

    private static void OnEnablePulseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HolographicIcon icon)
        {
            if ((bool)e.NewValue && icon._isLoaded)
            {
                icon.StartPulseAnimation();
            }
            else
            {
                icon.StopPulseAnimation();
            }
        }
    }

    private static void OnIsInteractiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HolographicIcon icon)
        {
            icon.Cursor = (bool)e.NewValue ? System.Windows.Input.Cursors.Hand : null;
        }
    }

    private static void OnGlowColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HolographicIcon icon)
        {
            icon.UpdateGlowEffect();
        }
    }

    private static void OnStrokeColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HolographicIcon icon)
        {
            icon.UpdateIconColors();
        }
    }

    private static void OnStrokeThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HolographicIcon icon)
        {
            icon.UpdateIconStroke();
        }
    }

    #endregion

    #region Private Methods

    private void CreateIconElements()
    {
        // Clear existing content
        Content = null;
        
        // Create container
        _container = new Viewbox
        {
            Stretch = Stretch.Uniform
        };
        
        // Create icon path
        _iconPath = new Path
        {
            Fill = Brushes.Transparent,
            RenderTransform = new ScaleTransform(1.0, 1.0),
            RenderTransformOrigin = new Point(0.5, 0.5)
        };
        
        // Create glow effect
        _glowEffect = new DropShadowEffect
        {
            ShadowDepth = 0,
            BlurRadius = 8,
            Opacity = 0.6
        };
        
        _iconPath.Effect = _glowEffect;
        _container.Child = _iconPath;
        Content = _container;
    }

    private void UpdateIconAppearance()
    {
        UpdateIconGeometry();
        UpdateIconSize();
        UpdateIconState();
        UpdateIconColors();
        UpdateIconStroke();
        UpdateGlowEffect();
    }

    private void UpdateIconGeometry()
    {
        if (_iconPath == null) return;
        
        var geometryKey = GetGeometryKeyForIconType(IconType);
        if (TryFindResource(geometryKey) is Geometry geometry)
        {
            _iconPath.Data = geometry;
        }
    }

    private string GetGeometryKeyForIconType(HolographicIconType iconType)
    {
        return iconType switch
        {
            HolographicIconType.Menu => "Glyph_Menu",
            HolographicIconType.Close => "Glyph_Close",
            HolographicIconType.Settings => "Glyph_Settings",
            HolographicIconType.Search => "Glyph_Search",
            HolographicIconType.WeaponTurret => "Glyph_Weapon_Turret",
            HolographicIconType.WeaponMissile => "Glyph_Weapon_Missile",
            HolographicIconType.Shield => "Glyph_Shield",
            HolographicIconType.Armor => "Glyph_Armor",
            HolographicIconType.Capacitor => "Glyph_Capacitor",
            HolographicIconType.ShipFrigate => "Glyph_Ship_Frigate",
            HolographicIconType.ShipCruiser => "Glyph_Ship_Cruiser",
            HolographicIconType.ShipBattleship => "Glyph_Ship_Battleship",
            HolographicIconType.Station => "Glyph_Station",
            HolographicIconType.Market => "Glyph_Market",
            HolographicIconType.Industry => "Glyph_Industry",
            HolographicIconType.Character => "Glyph_Character",
            HolographicIconType.Corporation => "Glyph_Corporation",
            HolographicIconType.Alliance => "Glyph_Alliance",
            _ => "Glyph_Menu"
        };
    }

    private void UpdateIconSize()
    {
        if (_container == null) return;
        
        var size = IconSize == HolographicIconSize.Custom ? CustomSize : (double)IconSize;
        
        _container.Width = size;
        _container.Height = size;
        Width = size;
        Height = size;
    }

    private void UpdateIconState()
    {
        if (_iconPath == null) return;
        
        // Update colors and effects based on state
        var (strokeColor, glowColor) = GetColorsForState(IconState);
        
        StrokeColor = strokeColor;
        GlowColor = glowColor;
        
        // Update opacity based on state
        var opacity = IconState == HolographicIconState.Disabled ? 0.4 : 1.0;
        _iconPath.Opacity = opacity;
    }

    private (Color stroke, Color glow) GetColorsForState(HolographicIconState state)
    {
        return state switch
        {
            HolographicIconState.Normal => (Colors.Cyan, Colors.Cyan),
            HolographicIconState.Hover => (Color.FromRgb(0, 255, 255), Color.FromRgb(0, 255, 255)),
            HolographicIconState.Active => (Color.FromRgb(100, 255, 255), Color.FromRgb(100, 255, 255)),
            HolographicIconState.Disabled => (Color.FromRgb(100, 100, 100), Color.FromRgb(100, 100, 100)),
            HolographicIconState.Warning => (Color.FromRgb(255, 165, 0), Color.FromRgb(255, 165, 0)),
            HolographicIconState.Success => (Color.FromRgb(0, 255, 0), Color.FromRgb(0, 255, 0)),
            HolographicIconState.Error => (Color.FromRgb(255, 0, 0), Color.FromRgb(255, 0, 0)),
            _ => (Colors.Cyan, Colors.Cyan)
        };
    }

    private void UpdateIconColors()
    {
        if (_iconPath == null) return;
        
        _iconPath.Stroke = new SolidColorBrush(StrokeColor);
    }

    private void UpdateIconStroke()
    {
        if (_iconPath == null) return;
        
        _iconPath.StrokeThickness = StrokeThickness;
    }

    private void UpdateGlowEffect()
    {
        if (_glowEffect == null) return;
        
        _glowEffect.Color = GlowColor;
        _glowEffect.Opacity = 0.6 * GlowIntensity;
        _glowEffect.BlurRadius = 8 * GlowIntensity;
    }

    private void StartPulseAnimation()
    {
        if (_glowEffect == null || _pulseStoryboard != null) return;
        
        _pulseStoryboard = new Storyboard
        {
            RepeatBehavior = RepeatBehavior.Forever,
            AutoReverse = true
        };
        
        var animation = new DoubleAnimation
        {
            From = 0.4 * GlowIntensity,
            To = 1.0 * GlowIntensity,
            Duration = TimeSpan.FromSeconds(1.5),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        
        Storyboard.SetTarget(animation, _glowEffect);
        Storyboard.SetTargetProperty(animation, new PropertyPath(DropShadowEffect.OpacityProperty));
        _pulseStoryboard.Children.Add(animation);
        
        _pulseStoryboard.Begin();
    }

    private void StopPulseAnimation()
    {
        _pulseStoryboard?.Stop();
        _pulseStoryboard = null;
    }

    private void StartHoverAnimation(bool isEntering)
    {
        if (_glowEffect == null) return;
        
        _hoverStoryboard?.Stop();
        _hoverStoryboard = new Storyboard();
        
        var blurAnimation = new DoubleAnimation
        {
            To = isEntering ? 12 * GlowIntensity : 8 * GlowIntensity,
            Duration = TimeSpan.FromSeconds(0.2),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        
        var opacityAnimation = new DoubleAnimation
        {
            To = isEntering ? 0.8 * GlowIntensity : 0.6 * GlowIntensity,
            Duration = TimeSpan.FromSeconds(0.2),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        
        Storyboard.SetTarget(blurAnimation, _glowEffect);
        Storyboard.SetTargetProperty(blurAnimation, new PropertyPath(DropShadowEffect.BlurRadiusProperty));
        
        Storyboard.SetTarget(opacityAnimation, _glowEffect);
        Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(DropShadowEffect.OpacityProperty));
        
        _hoverStoryboard.Children.Add(blurAnimation);
        _hoverStoryboard.Children.Add(opacityAnimation);
        
        _hoverStoryboard.Begin();
    }

    private void TriggerActivationEffect()
    {
        if (_iconPath?.RenderTransform is not ScaleTransform transform) return;
        
        var storyboard = new Storyboard();
        
        var scaleXAnimation = new DoubleAnimation
        {
            From = 0.9,
            To = 1.0,
            Duration = TimeSpan.FromSeconds(0.15),
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
        };
        
        var scaleYAnimation = new DoubleAnimation
        {
            From = 0.9,
            To = 1.0,
            Duration = TimeSpan.FromSeconds(0.15),
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
        };
        
        Storyboard.SetTarget(scaleXAnimation, transform);
        Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));
        
        Storyboard.SetTarget(scaleYAnimation, transform);
        Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));
        
        storyboard.Children.Add(scaleXAnimation);
        storyboard.Children.Add(scaleYAnimation);
        
        storyboard.Begin();
    }

    private void StopAllAnimations()
    {
        _pulseStoryboard?.Stop();
        _pulseStoryboard = null;
        
        _hoverStoryboard?.Stop();
        _hoverStoryboard = null;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Manually trigger the activation effect
    /// </summary>
    public void TriggerActivation()
    {
        TriggerActivationEffect();
    }

    /// <summary>
    /// Sets the icon state with optional animation
    /// </summary>
    public void SetState(HolographicIconState state, bool animate = true)
    {
        if (animate && state != IconState)
        {
            // TODO: Add state transition animations
        }
        
        IconState = state;
    }

    #endregion
}