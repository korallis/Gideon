// ==========================================================================
// HoloPanel.cs - Holographic Panel with Advanced Glassmorphism Effects
// ==========================================================================
// Core holographic panel control featuring advanced glassmorphism effects,
// depth layering, EVE-style borders, and adaptive visual quality.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Advanced holographic panel with glassmorphism effects and EVE styling
/// </summary>
public class HoloPanel : ContentControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloPanel),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloPanel),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty DepthLayerProperty =
        DependencyProperty.Register(nameof(DepthLayer), typeof(HolographicDepth), typeof(HoloPanel),
            new PropertyMetadata(HolographicDepth.Mid, OnDepthLayerChanged));

    public static readonly DependencyProperty GlassmorphismIntensityProperty =
        DependencyProperty.Register(nameof(GlassmorphismIntensity), typeof(double), typeof(HoloPanel),
            new PropertyMetadata(0.8, OnGlassmorphismIntensityChanged));

    public static readonly DependencyProperty EnableHoloscanlineProperty =
        DependencyProperty.Register(nameof(EnableHoloscanline), typeof(bool), typeof(HoloPanel),
            new PropertyMetadata(true, OnEnableHoloscanlineChanged));

    public static readonly DependencyProperty EnableDepthEffectsProperty =
        DependencyProperty.Register(nameof(EnableDepthEffects), typeof(bool), typeof(HoloPanel),
            new PropertyMetadata(true, OnEnableDepthEffectsChanged));

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(HoloPanel),
            new PropertyMetadata(new CornerRadius(8), OnCornerRadiusChanged));

    public static readonly DependencyProperty BorderThicknessProperty =
        DependencyProperty.Register(nameof(BorderThickness), typeof(Thickness), typeof(HoloPanel),
            new PropertyMetadata(new Thickness(1), OnBorderThicknessChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloPanel),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

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

    public HolographicDepth DepthLayer
    {
        get => (HolographicDepth)GetValue(DepthLayerProperty);
        set => SetValue(DepthLayerProperty, value);
    }

    public double GlassmorphismIntensity
    {
        get => (double)GetValue(GlassmorphismIntensityProperty);
        set => SetValue(GlassmorphismIntensityProperty, value);
    }

    public bool EnableHoloscanline
    {
        get => (bool)GetValue(EnableHoloscanlineProperty);
        set => SetValue(EnableHoloscanlineProperty, value);
    }

    public bool EnableDepthEffects
    {
        get => (bool)GetValue(EnableDepthEffectsProperty);
        set => SetValue(EnableDepthEffectsProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public Thickness BorderThickness
    {
        get => (Thickness)GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    public bool EnableAnimations
    {
        get => (bool)GetValue(EnableAnimationsProperty);
        set => SetValue(EnableAnimationsProperty, value);
    }

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode { get; private set; }

    public void SwitchToFullMode()
    {
        IsInSimplifiedMode = false;
        EnableDepthEffects = true;
        EnableHoloscanline = true;
        EnableAnimations = true;
        UpdateHolographicEffects();
    }

    public void SwitchToSimplifiedMode()
    {
        IsInSimplifiedMode = true;
        EnableDepthEffects = false;
        EnableHoloscanline = false;
        EnableAnimations = false;
        UpdateHolographicEffects();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => !_disposed && IsLoaded;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        EnableAnimations = settings.EnabledFeatures.HasFlag(AnimationFeatures.BasicGlow);
        EnableDepthEffects = settings.EnabledFeatures.HasFlag(AnimationFeatures.AdvancedShaders);
        
        UpdateHolographicEffects();
    }

    #endregion

    #region Fields

    private Border _mainBorder;
    private Canvas _effectsCanvas;
    private Rectangle _glassmorphismLayer;
    private Rectangle _scanlineLayer;
    private Grid _depthGrid;
    private DispatcherTimer _animationTimer;
    private double _animationPhase = 0;
    private bool _disposed = false;

    #endregion

    #region Constructor

    public HoloPanel()
    {
        InitializeControl();
        SetupAnimations();
        
        // Register with animation intensity manager
        AnimationIntensityManager.Instance.RegisterTarget(this);
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Private Methods

    private void InitializeControl()
    {
        Template = CreateControlTemplate();
        UpdateHolographicEffects();
    }

    private ControlTemplate CreateControlTemplate()
    {
        var template = new ControlTemplate(typeof(HoloPanel));

        // Main depth grid for layering
        var depthGrid = new FrameworkElementFactory(typeof(Grid));
        depthGrid.Name = "PART_DepthGrid";

        // Background glassmorphism layer
        var backgroundLayer = new FrameworkElementFactory(typeof(Rectangle));
        backgroundLayer.Name = "PART_GlassmorphismLayer";
        backgroundLayer.SetValue(Rectangle.RadiusXProperty, new TemplateBindingExtension(CornerRadiusProperty));
        backgroundLayer.SetValue(Rectangle.RadiusYProperty, new TemplateBindingExtension(CornerRadiusProperty));

        // Main border for content
        var mainBorder = new FrameworkElementFactory(typeof(Border));
        mainBorder.Name = "PART_MainBorder";
        mainBorder.SetValue(Border.CornerRadiusProperty, new TemplateBindingExtension(CornerRadiusProperty));
        mainBorder.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(BorderThicknessProperty));
        mainBorder.SetValue(Border.PaddingProperty, new TemplateBindingExtension(PaddingProperty));

        // Content presenter
        var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
        contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
        contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Stretch);

        // Scanline effects canvas
        var effectsCanvas = new FrameworkElementFactory(typeof(Canvas));
        effectsCanvas.Name = "PART_EffectsCanvas";
        effectsCanvas.SetValue(Canvas.IsHitTestVisibleProperty, false);

        // Scanline layer
        var scanlineLayer = new FrameworkElementFactory(typeof(Rectangle));
        scanlineLayer.Name = "PART_ScanlineLayer";
        scanlineLayer.SetValue(Rectangle.RadiusXProperty, new TemplateBindingExtension(CornerRadiusProperty));
        scanlineLayer.SetValue(Rectangle.RadiusYProperty, new TemplateBindingExtension(CornerRadiusProperty));
        scanlineLayer.SetValue(FrameworkElement.IsHitTestVisibleProperty, false);

        // Assembly
        mainBorder.AppendChild(contentPresenter);
        effectsCanvas.AppendChild(scanlineLayer);
        
        depthGrid.AppendChild(backgroundLayer);
        depthGrid.AppendChild(mainBorder);
        depthGrid.AppendChild(effectsCanvas);

        template.VisualTree = depthGrid;
        return template;
    }

    private void SetupAnimations()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS for smooth holographic effects
        };
        _animationTimer.Tick += OnAnimationTick;
    }

    private void OnAnimationTick(object sender, EventArgs e)
    {
        if (!EnableAnimations || IsInSimplifiedMode) return;

        _animationPhase += 0.05;
        if (_animationPhase > Math.PI * 2)
            _animationPhase = 0;

        UpdateAnimatedEffects();
    }

    private void UpdateAnimatedEffects()
    {
        // Update holographic glow pulsing
        if (_mainBorder?.Effect is DropShadowEffect glowEffect)
        {
            var pulseIntensity = 0.8 + (Math.Sin(_animationPhase) * 0.2);
            glowEffect.Opacity = HolographicIntensity * 0.6 * pulseIntensity;
        }

        // Update scanline movement
        if (EnableHoloscanline && _scanlineLayer != null)
        {
            var scanlineTransform = _scanlineLayer.RenderTransform as TranslateTransform ?? new TranslateTransform();
            scanlineTransform.Y = Math.Sin(_animationPhase * 0.5) * 2;
            _scanlineLayer.RenderTransform = scanlineTransform;
        }

        // Update depth perspective
        if (EnableDepthEffects && _depthGrid != null)
        {
            var depthIntensity = Math.Sin(_animationPhase * 0.3) * 0.02;
            var perspectiveTransform = _depthGrid.RenderTransform as TransformGroup ?? new TransformGroup();
            
            var scaleTransform = GetOrCreateTransform<ScaleTransform>(perspectiveTransform);
            scaleTransform.ScaleX = 1.0 + depthIntensity;
            scaleTransform.ScaleY = 1.0 + depthIntensity;
            
            _depthGrid.RenderTransform = perspectiveTransform;
            _depthGrid.RenderTransformOrigin = new Point(0.5, 0.5);
        }
    }

    private T GetOrCreateTransform<T>(TransformGroup group) where T : Transform, new()
    {
        foreach (var transform in group.Children)
        {
            if (transform is T existing)
                return existing;
        }
        
        var newTransform = new T();
        group.Children.Add(newTransform);
        return newTransform;
    }

    private void UpdateHolographicEffects()
    {
        if (Template == null) return;

        Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            ApplyTemplate();
            
            _depthGrid = GetTemplateChild("PART_DepthGrid") as Grid;
            _mainBorder = GetTemplateChild("PART_MainBorder") as Border;
            _effectsCanvas = GetTemplateChild("PART_EffectsCanvas") as Canvas;
            _glassmorphismLayer = GetTemplateChild("PART_GlassmorphismLayer") as Rectangle;
            _scanlineLayer = GetTemplateChild("PART_ScanlineLayer") as Rectangle;

            UpdateGlassmorphismEffects();
            UpdateBorderEffects();
            UpdateDepthEffects();
            UpdateScanlineEffects();
        }), DispatcherPriority.Render);
    }

    private void UpdateGlassmorphismEffects()
    {
        if (_glassmorphismLayer == null) return;

        var color = GetEVEColor(EVEColorScheme);
        var depthAlpha = GetDepthAlpha(DepthLayer);
        var glassmorphismAlpha = (byte)(depthAlpha * GlassmorphismIntensity * 255);

        // Create glassmorphism gradient
        var glassBrush = new RadialGradientBrush();
        glassBrush.GradientStops.Add(new GradientStop(
            Color.FromArgb((byte)(glassmorphismAlpha * 0.3), color.R, color.G, color.B), 0.0));
        glassBrush.GradientStops.Add(new GradientStop(
            Color.FromArgb((byte)(glassmorphismAlpha * 0.6), color.R, color.G, color.B), 0.4));
        glassBrush.GradientStops.Add(new GradientStop(
            Color.FromArgb((byte)(glassmorphismAlpha * 0.2), color.R, color.G, color.B), 1.0));

        _glassmorphismLayer.Fill = glassBrush;

        // Add blur effect for glassmorphism (simplified for performance)
        if (!IsInSimplifiedMode && EnableDepthEffects)
        {
            _glassmorphismLayer.Effect = new BlurEffect
            {
                Radius = 8 * GlassmorphismIntensity,
                RenderingBias = RenderingBias.Performance
            };
        }
        else
        {
            _glassmorphismLayer.Effect = null;
        }
    }

    private void UpdateBorderEffects()
    {
        if (_mainBorder == null) return;

        var color = GetEVEColor(EVEColorScheme);
        var depthAlpha = GetDepthAlpha(DepthLayer);

        // Create border gradient
        var borderBrush = new LinearGradientBrush();
        borderBrush.StartPoint = new Point(0, 0);
        borderBrush.EndPoint = new Point(1, 1);
        borderBrush.GradientStops.Add(new GradientStop(
            Color.FromArgb((byte)(200 * depthAlpha), color.R, color.G, color.B), 0.0));
        borderBrush.GradientStops.Add(new GradientStop(
            Color.FromArgb((byte)(100 * depthAlpha), color.R, color.G, color.B), 0.5));
        borderBrush.GradientStops.Add(new GradientStop(
            Color.FromArgb((byte)(200 * depthAlpha), color.R, color.G, color.B), 1.0));

        _mainBorder.BorderBrush = borderBrush;

        // Add holographic glow
        if (!IsInSimplifiedMode)
        {
            _mainBorder.Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 12 * HolographicIntensity,
                ShadowDepth = 0,
                Opacity = 0.6 * HolographicIntensity * depthAlpha
            };
        }
        else
        {
            _mainBorder.Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 3,
                ShadowDepth = 0,
                Opacity = 0.3 * HolographicIntensity
            };
        }
    }

    private void UpdateDepthEffects()
    {
        if (_depthGrid == null || !EnableDepthEffects || IsInSimplifiedMode) return;

        var depthOffset = GetDepthOffset(DepthLayer);
        
        // Apply depth perspective
        var transformGroup = new TransformGroup();
        transformGroup.Children.Add(new TranslateTransform(depthOffset.X, depthOffset.Y));
        transformGroup.Children.Add(new ScaleTransform(1.0, 1.0));

        _depthGrid.RenderTransform = transformGroup;
        _depthGrid.RenderTransformOrigin = new Point(0.5, 0.5);

        // Apply depth opacity
        Opacity = GetDepthOpacity(DepthLayer);
    }

    private void UpdateScanlineEffects()
    {
        if (_scanlineLayer == null || !EnableHoloscanline || IsInSimplifiedMode) return;

        // Create scanline pattern
        var scanlineBrush = new LinearGradientBrush();
        scanlineBrush.StartPoint = new Point(0, 0);
        scanlineBrush.EndPoint = new Point(0, 1);

        for (int i = 0; i < 20; i++)
        {
            var position = i / 20.0;
            var alpha = (i % 2 == 0) ? 20 : 5;
            scanlineBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(alpha, 255, 255, 255), position));
        }

        _scanlineLayer.Fill = scanlineBrush;
        _scanlineLayer.Opacity = 0.3 * HolographicIntensity;
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

    private double GetDepthAlpha(HolographicDepth depth)
    {
        return depth switch
        {
            HolographicDepth.Background => 0.3,
            HolographicDepth.Mid => 0.6,
            HolographicDepth.Foreground => 0.9,
            HolographicDepth.Overlay => 1.0,
            _ => 0.6
        };
    }

    private Point GetDepthOffset(HolographicDepth depth)
    {
        return depth switch
        {
            HolographicDepth.Background => new Point(-2, -2),
            HolographicDepth.Mid => new Point(0, 0),
            HolographicDepth.Foreground => new Point(1, 1),
            HolographicDepth.Overlay => new Point(2, 2),
            _ => new Point(0, 0)
        };
    }

    private double GetDepthOpacity(HolographicDepth depth)
    {
        return depth switch
        {
            HolographicDepth.Background => 0.7,
            HolographicDepth.Mid => 0.9,
            HolographicDepth.Foreground => 0.95,
            HolographicDepth.Overlay => 1.0,
            _ => 0.9
        };
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableAnimations && !IsInSimplifiedMode)
            _animationTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        
        // Unregister from animation intensity manager
        AnimationIntensityManager.Instance.UnregisterTarget(this);
        
        _disposed = true;
    }

    #endregion

    #region Event Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPanel panel)
            panel.UpdateHolographicEffects();
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPanel panel)
            panel.UpdateHolographicEffects();
    }

    private static void OnDepthLayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPanel panel)
            panel.UpdateHolographicEffects();
    }

    private static void OnGlassmorphismIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPanel panel)
            panel.UpdateHolographicEffects();
    }

    private static void OnEnableHoloscanlineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPanel panel)
            panel.UpdateHolographicEffects();
    }

    private static void OnEnableDepthEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPanel panel)
            panel.UpdateHolographicEffects();
    }

    private static void OnCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPanel panel)
            panel.UpdateHolographicEffects();
    }

    private static void OnBorderThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPanel panel)
            panel.UpdateHolographicEffects();
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPanel panel)
        {
            if ((bool)e.NewValue && !panel.IsInSimplifiedMode)
                panel._animationTimer.Start();
            else
                panel._animationTimer.Stop();
        }
    }

    #endregion
}

/// <summary>
/// Holographic depth layers for 3D effect simulation
/// </summary>
public enum HolographicDepth
{
    Background,  // Furthest back, most transparent
    Mid,         // Standard depth layer
    Foreground,  // Closer to viewer, more opaque
    Overlay      // Frontmost layer, full opacity
}