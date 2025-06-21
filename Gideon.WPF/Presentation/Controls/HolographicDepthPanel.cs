// ==========================================================================
// HolographicDepthPanel.cs - Holographic Depth Management Panel
// ==========================================================================
// Advanced depth management control for the Westworld-EVE fusion interface.
// Provides programmatic control over holographic depth layers with automatic
// z-index management, depth-based effects, and performance optimization.
//
// Features:
// - 10-level depth system (0.0 to 1.0)
// - Automatic z-index and effect calculation
// - Dynamic depth transitions with animations
// - Parallax motion support
// - Performance-optimized rendering
// - Accessibility-aware depth cues
//
// Author: Gideon Development Team
// Created: June 21, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Holographic depth levels for UI layering
/// </summary>
public enum HolographicDepthLevel
{
    DeepBackground = 0,
    Background = 1,
    BackgroundSecondary = 2,
    MidLayer = 3,
    Foreground = 4,
    Surface = 5,
    Interactive = 6,
    Overlay = 7,
    Modal = 8,
    System = 9
}

/// <summary>
/// Depth transition types for animations
/// </summary>
public enum DepthTransitionType
{
    None,
    Emergence,
    Recession,
    Parallax,
    Focus,
    Blur
}

/// <summary>
/// Holographic depth management panel with automatic layering
/// </summary>
public class HolographicDepthPanel : Panel
{
    #region Dependency Properties

    /// <summary>
    /// Enable automatic depth-based effects
    /// </summary>
    public static readonly DependencyProperty EnableDepthEffectsProperty =
        DependencyProperty.Register(nameof(EnableDepthEffects), typeof(bool), typeof(HolographicDepthPanel),
            new PropertyMetadata(true, OnEnableDepthEffectsChanged));

    /// <summary>
    /// Enable parallax motion for background layers
    /// </summary>
    public static readonly DependencyProperty EnableParallaxProperty =
        DependencyProperty.Register(nameof(EnableParallax), typeof(bool), typeof(HolographicDepthPanel),
            new PropertyMetadata(false, OnEnableParallaxChanged));

    /// <summary>
    /// Depth perception intensity multiplier
    /// </summary>
    public static readonly DependencyProperty DepthIntensityProperty =
        DependencyProperty.Register(nameof(DepthIntensity), typeof(double), typeof(HolographicDepthPanel),
            new PropertyMetadata(1.0, OnDepthIntensityChanged));

    /// <summary>
    /// Performance optimization level
    /// </summary>
    public static readonly DependencyProperty OptimizationLevelProperty =
        DependencyProperty.Register(nameof(OptimizationLevel), typeof(int), typeof(HolographicDepthPanel),
            new PropertyMetadata(1, OnOptimizationLevelChanged));

    #endregion

    #region Attached Properties

    /// <summary>
    /// Attached property for element depth (0.0 to 1.0)
    /// </summary>
    public static readonly DependencyProperty DepthProperty =
        DependencyProperty.RegisterAttached("Depth", typeof(double), typeof(HolographicDepthPanel),
            new PropertyMetadata(0.5, OnDepthChanged));

    /// <summary>
    /// Attached property for depth level enumeration
    /// </summary>
    public static readonly DependencyProperty DepthLevelProperty =
        DependencyProperty.RegisterAttached("DepthLevel", typeof(HolographicDepthLevel), typeof(HolographicDepthPanel),
            new PropertyMetadata(HolographicDepthLevel.MidLayer, OnDepthLevelChanged));

    /// <summary>
    /// Attached property for depth transition type
    /// </summary>
    public static readonly DependencyProperty DepthTransitionProperty =
        DependencyProperty.RegisterAttached("DepthTransition", typeof(DepthTransitionType), typeof(HolographicDepthPanel),
            new PropertyMetadata(DepthTransitionType.None));

    /// <summary>
    /// Attached property for enabling parallax on specific elements
    /// </summary>
    public static readonly DependencyProperty EnableElementParallaxProperty =
        DependencyProperty.RegisterAttached("EnableElementParallax", typeof(bool), typeof(HolographicDepthPanel),
            new PropertyMetadata(false));

    #endregion

    #region Properties

    public bool EnableDepthEffects
    {
        get => (bool)GetValue(EnableDepthEffectsProperty);
        set => SetValue(EnableDepthEffectsProperty, value);
    }

    public bool EnableParallax
    {
        get => (bool)GetValue(EnableParallaxProperty);
        set => SetValue(EnableParallaxProperty, value);
    }

    public double DepthIntensity
    {
        get => (double)GetValue(DepthIntensityProperty);
        set => SetValue(DepthIntensityProperty, value);
    }

    public int OptimizationLevel
    {
        get => (int)GetValue(OptimizationLevelProperty);
        set => SetValue(OptimizationLevelProperty, value);
    }

    #endregion

    #region Attached Property Accessors

    public static double GetDepth(UIElement element)
        => (double)element.GetValue(DepthProperty);

    public static void SetDepth(UIElement element, double value)
        => element.SetValue(DepthProperty, value);

    public static HolographicDepthLevel GetDepthLevel(UIElement element)
        => (HolographicDepthLevel)element.GetValue(DepthLevelProperty);

    public static void SetDepthLevel(UIElement element, HolographicDepthLevel value)
        => element.SetValue(DepthLevelProperty, value);

    public static DepthTransitionType GetDepthTransition(UIElement element)
        => (DepthTransitionType)element.GetValue(DepthTransitionProperty);

    public static void SetDepthTransition(UIElement element, DepthTransitionType value)
        => element.SetValue(DepthTransitionProperty, value);

    public static bool GetEnableElementParallax(UIElement element)
        => (bool)element.GetValue(EnableElementParallaxProperty);

    public static void SetEnableElementParallax(UIElement element, bool value)
        => element.SetValue(EnableElementParallaxProperty, value);

    #endregion

    #region Fields

    private readonly Dictionary<UIElement, Storyboard> _activeAnimations = new();
    private readonly Dictionary<UIElement, double> _originalDepths = new();
    private bool _isLoaded = false;

    #endregion

    #region Constructor

    public HolographicDepthPanel()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        ClipToBounds = false;
    }

    #endregion

    #region Overrides

    protected override Size MeasureOverride(Size availableSize)
    {
        var maxSize = new Size();
        
        foreach (UIElement child in Children)
        {
            child.Measure(availableSize);
            maxSize.Width = Math.Max(maxSize.Width, child.DesiredSize.Width);
            maxSize.Height = Math.Max(maxSize.Height, child.DesiredSize.Height);
        }
        
        return maxSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (UIElement child in Children)
        {
            var depth = GetDepth(child);
            var parallaxOffset = CalculateParallaxOffset(depth);
            
            var rect = new Rect(
                parallaxOffset.X, 
                parallaxOffset.Y, 
                finalSize.Width, 
                finalSize.Height);
            
            child.Arrange(rect);
        }
        
        if (_isLoaded)
        {
            UpdateDepthEffects();
        }
        
        return finalSize;
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _isLoaded = true;
        UpdateDepthEffects();
        
        if (EnableParallax)
        {
            StartParallaxAnimations();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _isLoaded = false;
        StopAllAnimations();
    }

    #endregion

    #region Property Change Handlers

    private static void OnEnableDepthEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HolographicDepthPanel panel)
        {
            panel.UpdateDepthEffects();
        }
    }

    private static void OnEnableParallaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HolographicDepthPanel panel)
        {
            if ((bool)e.NewValue)
            {
                panel.StartParallaxAnimations();
            }
            else
            {
                panel.StopParallaxAnimations();
            }
        }
    }

    private static void OnDepthIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HolographicDepthPanel panel)
        {
            panel.UpdateDepthEffects();
        }
    }

    private static void OnOptimizationLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HolographicDepthPanel panel)
        {
            panel.UpdateDepthEffects();
        }
    }

    private static void OnDepthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element && GetParent(element) is HolographicDepthPanel panel)
        {
            panel.UpdateElementDepth(element);
        }
    }

    private static void OnDepthLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element && GetParent(element) is HolographicDepthPanel panel)
        {
            // Convert depth level to numeric depth
            var depthLevel = (HolographicDepthLevel)e.NewValue;
            var numericDepth = ConvertDepthLevelToNumeric(depthLevel);
            SetDepth(element, numericDepth);
        }
    }

    private static HolographicDepthPanel? GetParent(DependencyObject element)
    {
        var parent = VisualTreeHelper.GetParent(element);
        while (parent != null && parent is not HolographicDepthPanel)
        {
            parent = VisualTreeHelper.GetParent(parent);
        }
        return parent as HolographicDepthPanel;
    }

    #endregion

    #region Private Methods

    private void UpdateDepthEffects()
    {
        if (!EnableDepthEffects || !_isLoaded) return;

        foreach (UIElement child in Children)
        {
            UpdateElementDepth(child);
        }
        
        // Update z-index ordering
        UpdateZIndexOrdering();
    }

    private void UpdateElementDepth(UIElement element)
    {
        if (!EnableDepthEffects) return;

        var depth = GetDepth(element);
        var clampedDepth = Math.Clamp(depth, 0.0, 1.0);
        
        // Calculate z-index (higher depth = higher z-index)
        var zIndex = (int)(clampedDepth * 100);
        Panel.SetZIndex(element, zIndex);
        
        // Calculate opacity based on depth
        var opacity = CalculateOpacityForDepth(clampedDepth);
        element.Opacity = opacity;
        
        // Apply depth-based effects
        ApplyDepthEffect(element, clampedDepth);
    }

    private double CalculateOpacityForDepth(double depth)
    {
        // Opacity ranges from 0.2 (deep background) to 1.0 (surface)
        var baseOpacity = 0.2 + (depth * 0.8);
        return Math.Clamp(baseOpacity * DepthIntensity, 0.1, 1.0);
    }

    private void ApplyDepthEffect(UIElement element, double depth)
    {
        if (OptimizationLevel == 0) return; // No effects for maximum performance
        
        Effect? effect = null;
        
        if (depth < 0.3)
        {
            // Deep background - heavy blur
            var blurRadius = (1.0 - depth) * 25 * DepthIntensity;
            if (OptimizationLevel >= 2)
            {
                effect = new BlurEffect { Radius = Math.Max(0, blurRadius) };
            }
        }
        else if (depth < 0.5)
        {
            // Background - medium blur
            var blurRadius = (0.5 - depth) * 20 * DepthIntensity;
            if (OptimizationLevel >= 2)
            {
                effect = new BlurEffect { Radius = Math.Max(0, blurRadius) };
            }
        }
        else if (depth < 0.7)
        {
            // Mid-layer - light blur
            var blurRadius = (0.7 - depth) * 10 * DepthIntensity;
            if (OptimizationLevel >= 3)
            {
                effect = new BlurEffect { Radius = Math.Max(0, blurRadius) };
            }
        }
        else
        {
            // Surface/foreground - glow effect
            var glowIntensity = (depth - 0.7) * 3.33; // Scale 0.7-1.0 to 0-1
            var glowOpacity = 0.4 + (glowIntensity * 0.4) * DepthIntensity;
            var glowRadius = 8 + (glowIntensity * 12) * DepthIntensity;
            
            if (OptimizationLevel >= 1)
            {
                effect = new DropShadowEffect
                {
                    Color = Colors.Cyan,
                    BlurRadius = glowRadius,
                    ShadowDepth = 0,
                    Opacity = Math.Clamp(glowOpacity, 0, 1)
                };
            }
        }
        
        element.Effect = effect;
    }

    private void UpdateZIndexOrdering()
    {
        // Sort children by depth and update z-index accordingly
        var sortedChildren = Children.Cast<UIElement>()
            .OrderBy(GetDepth)
            .ToList();
        
        for (int i = 0; i < sortedChildren.Count; i++)
        {
            Panel.SetZIndex(sortedChildren[i], i * 10);
        }
    }

    private Point CalculateParallaxOffset(double depth)
    {
        if (!EnableParallax) return new Point(0, 0);
        
        // Parallax effect - background elements move slower
        var parallaxFactor = (1.0 - depth) * 0.5; // Max 50% offset for deep background
        return new Point(0, 0); // Base offset - will be animated
    }

    private void StartParallaxAnimations()
    {
        if (!EnableParallax) return;
        
        foreach (UIElement child in Children)
        {
            var depth = GetDepth(child);
            if (depth < 0.5 || GetEnableElementParallax(child))
            {
                StartElementParallax(child, depth);
            }
        }
    }

    private void StartElementParallax(UIElement element, double depth)
    {
        if (_activeAnimations.ContainsKey(element)) return;
        
        var parallaxFactor = (1.0 - depth) * 5; // Stronger effect for background elements
        var duration = TimeSpan.FromSeconds(8 + (depth * 4)); // Faster for background
        
        var transform = new TranslateTransform();
        element.RenderTransform = transform;
        
        var storyboard = new Storyboard
        {
            RepeatBehavior = RepeatBehavior.Forever,
            AutoReverse = true
        };
        
        var xAnimation = new DoubleAnimation
        {
            From = -parallaxFactor,
            To = parallaxFactor,
            Duration = duration,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        
        var yAnimation = new DoubleAnimation
        {
            From = -parallaxFactor * 0.5,
            To = parallaxFactor * 0.5,
            Duration = TimeSpan.FromSeconds(duration.TotalSeconds * 1.5),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        
        Storyboard.SetTarget(xAnimation, transform);
        Storyboard.SetTargetProperty(xAnimation, new PropertyPath(TranslateTransform.XProperty));
        
        Storyboard.SetTarget(yAnimation, transform);
        Storyboard.SetTargetProperty(yAnimation, new PropertyPath(TranslateTransform.YProperty));
        
        storyboard.Children.Add(xAnimation);
        storyboard.Children.Add(yAnimation);
        
        _activeAnimations[element] = storyboard;
        storyboard.Begin();
    }

    private void StopParallaxAnimations()
    {
        foreach (var kvp in _activeAnimations.ToList())
        {
            kvp.Value.Stop();
            kvp.Key.RenderTransform = null;
        }
        _activeAnimations.Clear();
    }

    private void StopAllAnimations()
    {
        StopParallaxAnimations();
    }

    private static double ConvertDepthLevelToNumeric(HolographicDepthLevel level)
    {
        return level switch
        {
            HolographicDepthLevel.DeepBackground => 0.05,
            HolographicDepthLevel.Background => 0.2,
            HolographicDepthLevel.BackgroundSecondary => 0.35,
            HolographicDepthLevel.MidLayer => 0.5,
            HolographicDepthLevel.Foreground => 0.65,
            HolographicDepthLevel.Surface => 0.8,
            HolographicDepthLevel.Interactive => 0.9,
            HolographicDepthLevel.Overlay => 0.95,
            HolographicDepthLevel.Modal => 0.98,
            HolographicDepthLevel.System => 1.0,
            _ => 0.5
        };
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Animate element to new depth with transition effect
    /// </summary>
    public void AnimateElementToDepth(UIElement element, double newDepth, DepthTransitionType transition = DepthTransitionType.None)
    {
        if (!Children.Contains(element)) return;
        
        var currentDepth = GetDepth(element);
        _originalDepths[element] = currentDepth;
        
        var storyboard = CreateDepthTransitionAnimation(element, currentDepth, newDepth, transition);
        if (storyboard != null)
        {
            storyboard.Completed += (s, e) => SetDepth(element, newDepth);
            storyboard.Begin();
        }
        else
        {
            SetDepth(element, newDepth);
        }
    }

    /// <summary>
    /// Focus on specific element by bringing it to surface
    /// </summary>
    public void FocusElement(UIElement element)
    {
        AnimateElementToDepth(element, 0.95, DepthTransitionType.Focus);
    }

    /// <summary>
    /// Restore element to original depth
    /// </summary>
    public void RestoreElement(UIElement element)
    {
        if (_originalDepths.TryGetValue(element, out var originalDepth))
        {
            AnimateElementToDepth(element, originalDepth, DepthTransitionType.Blur);
            _originalDepths.Remove(element);
        }
    }

    /// <summary>
    /// Get all elements at specific depth level
    /// </summary>
    public IEnumerable<UIElement> GetElementsAtDepthLevel(HolographicDepthLevel level)
    {
        var targetDepth = ConvertDepthLevelToNumeric(level);
        var tolerance = 0.1;
        
        return Children.Cast<UIElement>()
            .Where(e => Math.Abs(GetDepth(e) - targetDepth) < tolerance);
    }

    /// <summary>
    /// Optimize depth effects based on performance requirements
    /// </summary>
    public void OptimizeForPerformance(bool enableOptimization)
    {
        OptimizationLevel = enableOptimization ? 1 : 3;
        UpdateDepthEffects();
    }

    #endregion

    #region Private Animation Methods

    private Storyboard? CreateDepthTransitionAnimation(UIElement element, double fromDepth, double toDepth, DepthTransitionType transition)
    {
        return transition switch
        {
            DepthTransitionType.Emergence => CreateEmergenceAnimation(element, fromDepth, toDepth),
            DepthTransitionType.Recession => CreateRecessionAnimation(element, fromDepth, toDepth),
            DepthTransitionType.Focus => CreateFocusAnimation(element, fromDepth, toDepth),
            DepthTransitionType.Blur => CreateBlurAnimation(element, fromDepth, toDepth),
            _ => null
        };
    }

    private Storyboard CreateEmergenceAnimation(UIElement element, double fromDepth, double toDepth)
    {
        var storyboard = new Storyboard();
        var duration = TimeSpan.FromSeconds(0.8);
        
        // Opacity animation
        var opacityAnimation = new DoubleAnimation
        {
            From = CalculateOpacityForDepth(fromDepth),
            To = CalculateOpacityForDepth(toDepth),
            Duration = duration,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        
        Storyboard.SetTarget(opacityAnimation, element);
        Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));
        storyboard.Children.Add(opacityAnimation);
        
        return storyboard;
    }

    private Storyboard CreateRecessionAnimation(UIElement element, double fromDepth, double toDepth)
    {
        var storyboard = new Storyboard();
        var duration = TimeSpan.FromSeconds(0.6);
        
        // Opacity animation
        var opacityAnimation = new DoubleAnimation
        {
            From = CalculateOpacityForDepth(fromDepth),
            To = CalculateOpacityForDepth(toDepth),
            Duration = duration,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        
        Storyboard.SetTarget(opacityAnimation, element);
        Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));
        storyboard.Children.Add(opacityAnimation);
        
        return storyboard;
    }

    private Storyboard CreateFocusAnimation(UIElement element, double fromDepth, double toDepth)
    {
        var storyboard = new Storyboard();
        var duration = TimeSpan.FromSeconds(0.4);
        
        // Scale animation for focus effect
        var transform = new ScaleTransform(1.0, 1.0);
        element.RenderTransform = transform;
        element.RenderTransformOrigin = new Point(0.5, 0.5);
        
        var scaleAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 1.05,
            Duration = duration,
            AutoReverse = true,
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
        };
        
        Storyboard.SetTarget(scaleAnimation, transform);
        Storyboard.SetTargetProperty(scaleAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));
        storyboard.Children.Add(scaleAnimation);
        
        var scaleYAnimation = scaleAnimation.Clone();
        Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));
        storyboard.Children.Add(scaleYAnimation);
        
        return storyboard;
    }

    private Storyboard CreateBlurAnimation(UIElement element, double fromDepth, double toDepth)
    {
        var storyboard = new Storyboard();
        var duration = TimeSpan.FromSeconds(0.5);
        
        // Simple opacity transition for blur effect
        var opacityAnimation = new DoubleAnimation
        {
            From = CalculateOpacityForDepth(fromDepth),
            To = CalculateOpacityForDepth(toDepth),
            Duration = duration,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };
        
        Storyboard.SetTarget(opacityAnimation, element);
        Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));
        storyboard.Children.Add(opacityAnimation);
        
        return storyboard;
    }

    #endregion
}