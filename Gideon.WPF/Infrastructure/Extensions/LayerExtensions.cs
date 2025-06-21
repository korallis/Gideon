// ==========================================================================
// LayerExtensions.cs - Extension Methods for Layer Management
// ==========================================================================
// Provides fluent extension methods for managing holographic layers.
// Simplifies adding elements to layers and applying depth effects for
// the Westworld-EVE fusion interface.
//
// Author: Gideon Development Team
// Created: June 21, 2025
// ==========================================================================

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Gideon.WPF.Presentation.Controls;

namespace Gideon.WPF.Infrastructure.Extensions;

/// <summary>
/// Extension methods for holographic layer management
/// </summary>
public static class LayerExtensions
{
    #region Layer Assignment Extensions

    /// <summary>
    /// Place element in the background layer with deep holographic effects
    /// </summary>
    public static T InBackgroundLayer<T>(this T element, double depth = 0.8) where T : FrameworkElement
    {
        if (element == null) return element;

        try
        {
            var canvas = FindHolographicCanvas(element);
            canvas?.AddToLayer(element, HolographicLayer.Background, depth);

            // Apply background-specific effects
            element.Opacity = Math.Max(0.3, 1.0 - depth);
            
            if (depth > 0.5)
            {
                element.Effect = new BlurEffect { Radius = depth * 10 };
            }

            ApplyDepthTransform(element, depth);
        }
        catch (Exception)
        {
            // Fallback to basic styling if canvas not available
            ApplyBasicBackgroundStyling(element, depth);
        }

        return element;
    }

    /// <summary>
    /// Place element in the mid-layer for primary content
    /// </summary>
    public static T InMidLayer<T>(this T element, double depth = 0.5) where T : FrameworkElement
    {
        if (element == null) return element;

        try
        {
            var canvas = FindHolographicCanvas(element);
            canvas?.AddToLayer(element, HolographicLayer.MidLayer, depth);

            // Apply mid-layer specific effects
            element.Opacity = Math.Max(0.7, 1.0 - (depth * 0.3));
            
            if (depth > 0.3)
            {
                element.Effect = new BlurEffect { Radius = depth * 3 };
            }

            ApplyDepthTransform(element, depth * 0.5);
        }
        catch (Exception)
        {
            // Fallback to basic styling
            ApplyBasicMidLayerStyling(element, depth);
        }

        return element;
    }

    /// <summary>
    /// Place element in the foreground layer for interactive content
    /// </summary>
    public static T InForegroundLayer<T>(this T element, double depth = 0.1) where T : FrameworkElement
    {
        if (element == null) return element;

        try
        {
            var canvas = FindHolographicCanvas(element);
            canvas?.AddToLayer(element, HolographicLayer.Foreground, depth);

            // Apply foreground-specific effects (minimal depth effects)
            element.Opacity = Math.Max(0.9, 1.0 - (depth * 0.1));
            element.Effect = null; // Clear any blur effects

            if (depth > 0)
            {
                ApplyDepthTransform(element, depth * 0.2);
            }
        }
        catch (Exception)
        {
            // Foreground elements should always be fully visible
            element.Opacity = 1.0;
            element.Effect = null;
        }

        return element;
    }

    /// <summary>
    /// Place element in the overlay layer for tooltips and menus
    /// </summary>
    public static T InOverlayLayer<T>(this T element) where T : FrameworkElement
    {
        if (element == null) return element;

        try
        {
            var canvas = FindHolographicCanvas(element);
            canvas?.AddToLayer(element, HolographicLayer.Overlay, 0.0);

            // Apply overlay-specific effects
            element.Opacity = 1.0;
            element.Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 12,
                ShadowDepth = 4,
                Opacity = 0.5
            };
        }
        catch (Exception)
        {
            // Ensure overlay is always visible
            element.Opacity = 1.0;
        }

        return element;
    }

    /// <summary>
    /// Place element in the topmost layer for critical content
    /// </summary>
    public static T InTopmostLayer<T>(this T element) where T : FrameworkElement
    {
        if (element == null) return element;

        try
        {
            var canvas = FindHolographicCanvas(element);
            canvas?.AddToLayer(element, HolographicLayer.Topmost, -0.1);

            // Apply topmost-specific effects
            element.Opacity = 1.0;
            element.Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(128, 0, 212, 255), // EVE Cyan
                BlurRadius = 16,
                ShadowDepth = 0,
                Opacity = 0.8
            };

            // Slight forward transform
            element.RenderTransform = new ScaleTransform(1.02, 1.02);
            element.RenderTransformOrigin = new Point(0.5, 0.5);
        }
        catch (Exception)
        {
            // Ensure topmost is always fully visible
            element.Opacity = 1.0;
        }

        return element;
    }

    #endregion

    #region Depth Transition Extensions

    /// <summary>
    /// Animate element to background layer
    /// </summary>
    public static T TransitionToBackground<T>(this T element, double duration = 0.5, double targetDepth = 0.8) 
        where T : FrameworkElement
    {
        if (element == null) return element;

        try
        {
            var canvas = FindHolographicCanvas(element);
            if (canvas != null)
            {
                canvas.TransitionToLayer(element, HolographicLayer.Background, targetDepth);
            }
            else
            {
                // Fallback animation
                AnimateToBackground(element, duration, targetDepth);
            }
        }
        catch (Exception)
        {
            // Direct application if animation fails
            element.InBackgroundLayer(targetDepth);
        }

        return element;
    }

    /// <summary>
    /// Animate element to foreground layer
    /// </summary>
    public static T TransitionToForeground<T>(this T element, double duration = 0.3, double targetDepth = 0.1) 
        where T : FrameworkElement
    {
        if (element == null) return element;

        try
        {
            var canvas = FindHolographicCanvas(element);
            if (canvas != null)
            {
                canvas.TransitionToLayer(element, HolographicLayer.Foreground, targetDepth);
            }
            else
            {
                // Fallback animation
                AnimateToForeground(element, duration, targetDepth);
            }
        }
        catch (Exception)
        {
            // Direct application if animation fails
            element.InForegroundLayer(targetDepth);
        }

        return element;
    }

    /// <summary>
    /// Animate element between layers based on focus state
    /// </summary>
    public static T WithFocusLayerTransition<T>(this T element) where T : FrameworkElement
    {
        if (element == null) return element;

        try
        {
            element.GotFocus += (s, e) => element.TransitionToForeground(0.2);
            element.LostFocus += (s, e) => element.TransitionToBackground(0.3);

            if (element is Control control)
            {
                control.MouseEnter += (s, e) => element.TransitionToForeground(0.15);
                control.MouseLeave += (s, e) => element.InMidLayer();
            }
        }
        catch (Exception)
        {
            // Focus transitions are optional
        }

        return element;
    }

    #endregion

    #region Depth Effect Extensions

    /// <summary>
    /// Apply depth-based blur effect
    /// </summary>
    public static T WithDepthBlur<T>(this T element, double depth = 0.5) where T : FrameworkElement
    {
        if (element == null || depth <= 0) return element;

        var blurRadius = depth * 15.0; // Scale blur with depth
        element.Effect = new BlurEffect { Radius = blurRadius };

        return element;
    }

    /// <summary>
    /// Apply depth-based opacity
    /// </summary>
    public static T WithDepthOpacity<T>(this T element, double depth = 0.5) where T : FrameworkElement
    {
        if (element == null) return element;

        // Opacity decreases with depth (further = more transparent)
        var opacity = Math.Max(0.2, 1.0 - (depth * 0.8));
        element.Opacity = opacity;

        return element;
    }

    /// <summary>
    /// Apply depth-based scale transform
    /// </summary>
    public static T WithDepthScale<T>(this T element, double depth = 0.5) where T : FrameworkElement
    {
        if (element == null) return element;

        // Scale decreases slightly with depth for perspective effect
        var scale = 1.0 - (depth * 0.15);
        ApplyDepthTransform(element, depth);

        return element;
    }

    /// <summary>
    /// Apply complete depth effect package
    /// </summary>
    public static T WithDepthEffects<T>(this T element, double depth = 0.5) where T : FrameworkElement
    {
        return element
            .WithDepthOpacity(depth)
            .WithDepthBlur(depth)
            .WithDepthScale(depth);
    }

    #endregion

    #region Layer Removal Extensions

    /// <summary>
    /// Remove element from all layers
    /// </summary>
    public static T RemoveFromLayers<T>(this T element) where T : FrameworkElement
    {
        if (element == null) return element;

        try
        {
            var canvas = FindHolographicCanvas(element);
            canvas?.RemoveElement(element);

            // Reset effects
            element.Opacity = 1.0;
            element.Effect = null;
            element.RenderTransform = Transform.Identity;
            Panel.SetZIndex(element, 0);
        }
        catch (Exception)
        {
            // Best effort cleanup
        }

        return element;
    }

    #endregion

    #region Private Helper Methods

    private static HolographicCanvas? FindHolographicCanvas(FrameworkElement element)
    {
        var parent = element.Parent as DependencyObject;
        
        while (parent != null)
        {
            if (parent is HolographicCanvas canvas)
                return canvas;

            parent = LogicalTreeHelper.GetParent(parent) ?? VisualTreeHelper.GetParent(parent);
        }

        return null;
    }

    private static void ApplyDepthTransform(FrameworkElement element, double depth)
    {
        var scale = 1.0 - (depth * 0.15);
        element.RenderTransform = new ScaleTransform(scale, scale);
        element.RenderTransformOrigin = new Point(0.5, 0.5);
    }

    private static void ApplyBasicBackgroundStyling(FrameworkElement element, double depth)
    {
        element.Opacity = Math.Max(0.3, 1.0 - depth);
        if (depth > 0.5)
        {
            element.Effect = new BlurEffect { Radius = depth * 8 };
        }
        Panel.SetZIndex(element, 0);
    }

    private static void ApplyBasicMidLayerStyling(FrameworkElement element, double depth)
    {
        element.Opacity = Math.Max(0.7, 1.0 - (depth * 0.3));
        Panel.SetZIndex(element, 1000);
    }

    private static void AnimateToBackground(FrameworkElement element, double duration, double targetDepth)
    {
        var storyboard = new Storyboard();

        // Animate opacity
        var opacityAnimation = new DoubleAnimation
        {
            To = Math.Max(0.3, 1.0 - targetDepth),
            Duration = TimeSpan.FromSeconds(duration),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };
        Storyboard.SetTarget(opacityAnimation, element);
        Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));
        storyboard.Children.Add(opacityAnimation);

        // Animate scale
        var scaleTransform = new ScaleTransform();
        element.RenderTransform = scaleTransform;
        element.RenderTransformOrigin = new Point(0.5, 0.5);

        var scaleAnimation = new DoubleAnimation
        {
            To = 1.0 - (targetDepth * 0.15),
            Duration = TimeSpan.FromSeconds(duration),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };
        Storyboard.SetTarget(scaleAnimation, scaleTransform);
        Storyboard.SetTargetProperty(scaleAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));
        storyboard.Children.Add(scaleAnimation);

        var scaleYAnimation = new DoubleAnimation
        {
            To = 1.0 - (targetDepth * 0.15),
            Duration = TimeSpan.FromSeconds(duration),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };
        Storyboard.SetTarget(scaleYAnimation, scaleTransform);
        Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));
        storyboard.Children.Add(scaleYAnimation);

        storyboard.Begin();
    }

    private static void AnimateToForeground(FrameworkElement element, double duration, double targetDepth)
    {
        var storyboard = new Storyboard();

        // Animate opacity
        var opacityAnimation = new DoubleAnimation
        {
            To = Math.Max(0.9, 1.0 - (targetDepth * 0.1)),
            Duration = TimeSpan.FromSeconds(duration),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(opacityAnimation, element);
        Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));
        storyboard.Children.Add(opacityAnimation);

        // Animate scale
        var scaleTransform = element.RenderTransform as ScaleTransform ?? new ScaleTransform();
        if (element.RenderTransform != scaleTransform)
        {
            element.RenderTransform = scaleTransform;
            element.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        var scaleAnimation = new DoubleAnimation
        {
            To = 1.0,
            Duration = TimeSpan.FromSeconds(duration),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(scaleAnimation, scaleTransform);
        Storyboard.SetTargetProperty(scaleAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));
        storyboard.Children.Add(scaleAnimation);

        var scaleYAnimation = new DoubleAnimation
        {
            To = 1.0,
            Duration = TimeSpan.FromSeconds(duration),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(scaleYAnimation, scaleTransform);
        Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));
        storyboard.Children.Add(scaleYAnimation);

        storyboard.Begin();
    }

    #endregion
}

/// <summary>
/// Extension methods for working with holographic canvas directly
/// </summary>
public static class HolographicCanvasExtensions
{
    /// <summary>
    /// Enable debug mode for the canvas
    /// </summary>
    public static HolographicCanvas WithDebugMode(this HolographicCanvas canvas, bool enabled = true)
    {
        if (canvas != null)
        {
            canvas.ShowDebugOverlay = enabled;
            canvas.ShowPerformanceOverlay = enabled;
        }
        return canvas;
    }

    /// <summary>
    /// Configure scan line effects
    /// </summary>
    public static HolographicCanvas WithScanLines(this HolographicCanvas canvas, bool enabled = true)
    {
        if (canvas != null)
        {
            canvas.EnableScanLines = enabled;
        }
        return canvas;
    }

    /// <summary>
    /// Set layer transition speed
    /// </summary>
    public static HolographicCanvas WithTransitionSpeed(this HolographicCanvas canvas, TimeSpan duration)
    {
        if (canvas != null)
        {
            canvas.LayerTransitionDuration = duration;
        }
        return canvas;
    }
}