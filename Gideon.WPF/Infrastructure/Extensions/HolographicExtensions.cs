// ==========================================================================
// HolographicExtensions.cs - Extension Methods for Holographic Effects
// ==========================================================================
// Provides convenient extension methods to apply holographic effects to
// any WPF FrameworkElement. Simplifies the application of Westworld-EVE
// fusion UI effects throughout the application.
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
using Gideon.WPF.Infrastructure.Graphics;
using Microsoft.Extensions.DependencyInjection;

namespace Gideon.WPF.Infrastructure.Extensions;

/// <summary>
/// Extension methods for applying holographic effects to UI elements
/// </summary>
public static class HolographicExtensions
{
    #region Holographic Glow Extensions

    /// <summary>
    /// Apply holographic glow effect with default EVE cyan color
    /// </summary>
    public static T WithHolographicGlow<T>(this T element, double intensity = 1.0) where T : FrameworkElement
    {
        return element.WithHolographicGlow(new HolographicGlowOptions
        {
            Intensity = intensity,
            Color = Colors.EVECyan
        });
    }

    /// <summary>
    /// Apply holographic glow effect with custom options
    /// </summary>
    public static T WithHolographicGlow<T>(this T element, HolographicGlowOptions options) where T : FrameworkElement
    {
        var service = GetHolographicEffectsService(element);
        service?.ApplyHolographicGlow(element, options);
        return element;
    }

    /// <summary>
    /// Apply EVE-themed holographic glow (cyan)
    /// </summary>
    public static T WithEVEGlow<T>(this T element, double intensity = 1.0) where T : FrameworkElement
    {
        return element.WithHolographicGlow(new HolographicGlowOptions
        {
            Intensity = intensity,
            Radius = 12.0,
            PulseFrequency = 1.5,
            Color = Colors.EVECyan
        });
    }

    /// <summary>
    /// Apply Westworld-themed holographic glow (blue-white)
    /// </summary>
    public static T WithWestworldGlow<T>(this T element, double intensity = 1.0) where T : FrameworkElement
    {
        return element.WithHolographicGlow(new HolographicGlowOptions
        {
            Intensity = intensity,
            Radius = 15.0,
            PulseFrequency = 2.5,
            Color = Colors.WestworldBlue
        });
    }

    #endregion

    #region Glassmorphism Extensions

    /// <summary>
    /// Apply glassmorphism effect with default settings
    /// </summary>
    public static T WithGlassmorphism<T>(this T element, double opacity = 0.8) where T : FrameworkElement
    {
        return element.WithGlassmorphism(new GlassmorphismOptions
        {
            Opacity = opacity
        });
    }

    /// <summary>
    /// Apply glassmorphism effect with custom options
    /// </summary>
    public static T WithGlassmorphism<T>(this T element, GlassmorphismOptions options) where T : FrameworkElement
    {
        var service = GetHolographicEffectsService(element);
        service?.ApplyGlassmorphism(element, options);
        return element;
    }

    /// <summary>
    /// Apply light glassmorphism for subtle effects
    /// </summary>
    public static T WithLightGlass<T>(this T element) where T : FrameworkElement
    {
        return element.WithGlassmorphism(new GlassmorphismOptions
        {
            BlurRadius = 8.0,
            Thickness = 0.05,
            Opacity = 0.6,
            TintColor = Color.FromArgb(15, 255, 255, 255)
        });
    }

    /// <summary>
    /// Apply heavy glassmorphism for prominent effects
    /// </summary>
    public static T WithHeavyGlass<T>(this T element) where T : FrameworkElement
    {
        return element.WithGlassmorphism(new GlassmorphismOptions
        {
            BlurRadius = 25.0,
            Thickness = 0.2,
            Opacity = 0.9,
            TintColor = Color.FromArgb(40, 0, 212, 255)
        });
    }

    #endregion

    #region Animation Extensions

    /// <summary>
    /// Start holographic pulsing animation
    /// </summary>
    public static T WithPulsing<T>(this T element, double frequency = 2.0, double intensity = 0.3) where T : FrameworkElement
    {
        var service = GetHolographicEffectsService(element);
        service?.StartPulsing(element, frequency, intensity);
        return element;
    }

    /// <summary>
    /// Apply slow, subtle pulsing for ambient effects
    /// </summary>
    public static T WithAmbientPulse<T>(this T element) where T : FrameworkElement
    {
        return element.WithPulsing(1.0, 0.15);
    }

    /// <summary>
    /// Apply fast, intense pulsing for alerts
    /// </summary>
    public static T WithAlertPulse<T>(this T element) where T : FrameworkElement
    {
        return element.WithPulsing(4.0, 0.5);
    }

    /// <summary>
    /// Apply holographic scanlines effect
    /// </summary>
    public static T WithScanlines<T>(this T element, double intensity = 0.3) where T : FrameworkElement
    {
        var service = GetHolographicEffectsService(element);
        service?.ApplyHolographicScanlines(element, new ScanlinesOptions
        {
            Intensity = intensity
        });
        return element;
    }

    #endregion

    #region Composite Effects

    /// <summary>
    /// Apply complete holographic panel effect (glow + glass + scanlines)
    /// </summary>
    public static T WithHolographicPanel<T>(this T element) where T : FrameworkElement
    {
        return element
            .WithGlassmorphism(0.8)
            .WithHolographicGlow(0.7)
            .WithScanlines(0.2)
            .WithAmbientPulse();
    }

    /// <summary>
    /// Apply EVE military panel styling
    /// </summary>
    public static T WithEVEMilitaryStyle<T>(this T element) where T : FrameworkElement
    {
        return element
            .WithGlassmorphism(new GlassmorphismOptions
            {
                BlurRadius = 12.0,
                Opacity = 0.85,
                TintColor = Color.FromArgb(20, 255, 215, 0) // Gold tint
            })
            .WithHolographicGlow(new HolographicGlowOptions
            {
                Intensity = 0.8,
                Radius = 8.0,
                Color = Colors.EVEGold
            });
    }

    /// <summary>
    /// Apply Westworld diagnostic panel styling
    /// </summary>
    public static T WithWestworldDiagnostic<T>(this T element) where T : FrameworkElement
    {
        return element
            .WithHeavyGlass()
            .WithWestworldGlow(0.9)
            .WithScanlines(0.4)
            .WithPulsing(1.5, 0.2);
    }

    /// <summary>
    /// Apply data visualization styling for dynamic content
    /// </summary>
    public static T WithDataVisualization<T>(this T element) where T : FrameworkElement
    {
        return element
            .WithLightGlass()
            .WithEVEGlow(1.2)
            .WithPulsing(3.0, 0.4);
    }

    #endregion

    #region Removal Extensions

    /// <summary>
    /// Remove all holographic effects from element
    /// </summary>
    public static T WithoutHolographicEffects<T>(this T element) where T : FrameworkElement
    {
        var service = GetHolographicEffectsService(element);
        service?.RemoveEffects(element);
        return element;
    }

    #endregion

    #region Container Extensions

    /// <summary>
    /// Create animated data stream within a container
    /// </summary>
    public static Panel WithDataStream(this Panel container, DataStreamOptions? options = null)
    {
        var service = GetHolographicEffectsService(container);
        service?.CreateDataStream(container, options);
        return container;
    }

    /// <summary>
    /// Create EVE-themed data stream (cyan particles)
    /// </summary>
    public static Panel WithEVEDataStream(this Panel container, int particleCount = 15)
    {
        return container.WithDataStream(new DataStreamOptions
        {
            ParticleCount = particleCount,
            ParticleColor = Colors.EVECyan,
            ParticleSize = 2.5,
            StreamDuration = 4.0,
            EnableGlow = true
        });
    }

    /// <summary>
    /// Create market data stream with gold particles
    /// </summary>
    public static Panel WithMarketDataStream(this Panel container, int particleCount = 20)
    {
        return container.WithDataStream(new DataStreamOptions
        {
            ParticleCount = particleCount,
            ParticleColor = Colors.EVEGold,
            ParticleSize = 3.0,
            StreamDuration = 3.5,
            EnableGlow = true
        });
    }

    #endregion

    #region Helper Methods

    private static HolographicEffectsService? GetHolographicEffectsService(FrameworkElement element)
    {
        try
        {
            // Try to get service from application services
            if (Application.Current is App app)
            {
                // In a real implementation, this would get the service from DI container
                // For now, we'll return null to avoid errors
                return null;
            }
        }
        catch (Exception)
        {
            // Service not available - effects will be skipped
        }

        return null;
    }

    #endregion
}

/// <summary>
/// Predefined color constants for holographic effects
/// </summary>
public static class Colors
{
    public static readonly Color EVECyan = Color.FromArgb(255, 0, 212, 255);
    public static readonly Color EVEGold = Color.FromArgb(255, 255, 215, 0);
    public static readonly Color EVEOrange = Color.FromArgb(255, 255, 159, 10);
    public static readonly Color EVERed = Color.FromArgb(255, 255, 69, 58);
    public static readonly Color EVEGreen = Color.FromArgb(255, 48, 209, 88);
    
    public static readonly Color WestworldBlue = Color.FromArgb(255, 100, 200, 255);
    public static readonly Color WestworldWhite = Color.FromArgb(255, 240, 248, 255);
    public static readonly Color WestworldGold = Color.FromArgb(255, 255, 223, 128);
    
    public static readonly Color HolographicCyan = Color.FromArgb(255, 0, 255, 255);
    public static readonly Color HolographicPurple = Color.FromArgb(255, 138, 43, 226);
    public static readonly Color HolographicPink = Color.FromArgb(255, 255, 20, 147);
}

/// <summary>
/// Extension methods for creating holographic brushes
/// </summary>
public static class HolographicBrushes
{
    /// <summary>
    /// Create animated holographic gradient brush
    /// </summary>
    public static LinearGradientBrush CreateHolographicGradient(Color primaryColor, Color secondaryColor)
    {
        var brush = new LinearGradientBrush();
        brush.StartPoint = new Point(0, 0);
        brush.EndPoint = new Point(1, 1);
        
        brush.GradientStops.Add(new GradientStop(primaryColor, 0.0));
        brush.GradientStops.Add(new GradientStop(secondaryColor, 0.3));
        brush.GradientStops.Add(new GradientStop(primaryColor, 0.7));
        brush.GradientStops.Add(new GradientStop(secondaryColor, 1.0));
        
        return brush;
    }

    /// <summary>
    /// Create EVE-themed gradient brush
    /// </summary>
    public static LinearGradientBrush CreateEVEGradient()
    {
        return CreateHolographicGradient(Colors.EVECyan, Colors.EVEGold);
    }

    /// <summary>
    /// Create Westworld-themed gradient brush
    /// </summary>
    public static LinearGradientBrush CreateWestworldGradient()
    {
        return CreateHolographicGradient(Colors.WestworldBlue, Colors.WestworldWhite);
    }
}