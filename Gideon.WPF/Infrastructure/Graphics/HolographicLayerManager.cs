// ==========================================================================
// HolographicLayerManager.cs - Layered Composition System
// ==========================================================================
// Manages the layered composition system for holographic depth effects.
// Provides background, mid-layer, and foreground composition with proper
// z-ordering and depth perception for the Westworld-EVE fusion interface.
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
using Microsoft.Extensions.Logging;

namespace Gideon.WPF.Infrastructure.Graphics;

/// <summary>
/// Manages holographic layer composition for depth effects
/// </summary>
public class HolographicLayerManager : IDisposable
{
    private readonly ILogger<HolographicLayerManager> _logger;
    private readonly Dictionary<string, HolographicLayer> _layers = new();
    private readonly Dictionary<FrameworkElement, LayerRegistration> _elementRegistrations = new();
    private bool _disposed;

    // Z-Index ranges for different layers
    private const int BackgroundLayerZIndex = 0;
    private const int MidLayerZIndex = 1000;
    private const int ForegroundLayerZIndex = 2000;
    private const int OverlayLayerZIndex = 3000;
    private const int TopmostLayerZIndex = 4000;

    public HolographicLayerManager(ILogger<HolographicLayerManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        InitializeDefaultLayers();
    }

    /// <summary>
    /// Initialize the default layer structure
    /// </summary>
    private void InitializeDefaultLayers()
    {
        // Background Layer - Deep holographic elements
        RegisterLayer("Background", new HolographicLayerOptions
        {
            ZIndexBase = BackgroundLayerZIndex,
            DefaultDepth = 0.9,
            BlurIntensity = 0.8,
            OpacityMultiplier = 0.7,
            Description = "Deep background holographic elements"
        });

        // Mid Layer - Primary content and interface elements
        RegisterLayer("MidLayer", new HolographicLayerOptions
        {
            ZIndexBase = MidLayerZIndex,
            DefaultDepth = 0.5,
            BlurIntensity = 0.3,
            OpacityMultiplier = 1.0,
            Description = "Primary content and interface elements"
        });

        // Foreground Layer - Interactive overlays and focused content
        RegisterLayer("Foreground", new HolographicLayerOptions
        {
            ZIndexBase = ForegroundLayerZIndex,
            DefaultDepth = 0.1,
            BlurIntensity = 0.0,
            OpacityMultiplier = 1.0,
            Description = "Interactive overlays and focused content"
        });

        // Overlay Layer - Tooltips, menus, and temporary elements
        RegisterLayer("Overlay", new HolographicLayerOptions
        {
            ZIndexBase = OverlayLayerZIndex,
            DefaultDepth = 0.0,
            BlurIntensity = 0.0,
            OpacityMultiplier = 1.0,
            Description = "Tooltips, menus, and temporary elements"
        });

        // Topmost Layer - Critical alerts and modal dialogs
        RegisterLayer("Topmost", new HolographicLayerOptions
        {
            ZIndexBase = TopmostLayerZIndex,
            DefaultDepth = -0.1,
            BlurIntensity = 0.0,
            OpacityMultiplier = 1.0,
            Description = "Critical alerts and modal dialogs"
        });

        _logger.LogInformation("Initialized {Count} default holographic layers", _layers.Count);
    }

    /// <summary>
    /// Register a new holographic layer
    /// </summary>
    public void RegisterLayer(string layerName, HolographicLayerOptions options)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(HolographicLayerManager));

        if (string.IsNullOrEmpty(layerName))
            throw new ArgumentException("Layer name cannot be null or empty", nameof(layerName));

        if (options == null)
            throw new ArgumentNullException(nameof(options));

        var layer = new HolographicLayer(layerName, options);
        _layers[layerName] = layer;

        _logger.LogDebug("Registered holographic layer: {LayerName} (Z-Index: {ZIndex})", 
                        layerName, options.ZIndexBase);
    }

    /// <summary>
    /// Add an element to a specific layer
    /// </summary>
    public void AddToLayer(FrameworkElement element, string layerName, double? customDepth = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(HolographicLayerManager));

        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (!_layers.TryGetValue(layerName, out var layer))
        {
            _logger.LogWarning("Layer not found: {LayerName}. Available layers: {Layers}", 
                              layerName, string.Join(", ", _layers.Keys));
            return;
        }

        try
        {
            // Remove from previous layer if registered
            RemoveFromLayer(element);

            // Calculate Z-Index and depth
            var zIndex = layer.Options.ZIndexBase + layer.Elements.Count;
            var depth = customDepth ?? layer.Options.DefaultDepth;

            // Apply layer properties to element
            Panel.SetZIndex(element, zIndex);
            ApplyLayerEffects(element, layer, depth);

            // Register element
            var registration = new LayerRegistration
            {
                LayerName = layerName,
                Depth = depth,
                ZIndex = zIndex,
                OriginalOpacity = element.Opacity,
                OriginalEffect = element.Effect
            };

            layer.Elements.Add(element);
            _elementRegistrations[element] = registration;

            _logger.LogDebug("Added element {ElementType} to layer {LayerName} at depth {Depth}", 
                            element.GetType().Name, layerName, depth);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add element to layer {LayerName}", layerName);
        }
    }

    /// <summary>
    /// Remove an element from its current layer
    /// </summary>
    public void RemoveFromLayer(FrameworkElement element)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(HolographicLayerManager));

        if (element == null || !_elementRegistrations.TryGetValue(element, out var registration))
            return;

        try
        {
            // Remove from layer
            if (_layers.TryGetValue(registration.LayerName, out var layer))
            {
                layer.Elements.Remove(element);
            }

            // Restore original properties
            element.Opacity = registration.OriginalOpacity;
            element.Effect = registration.OriginalEffect;
            Panel.SetZIndex(element, 0);

            // Unregister
            _elementRegistrations.Remove(element);

            _logger.LogDebug("Removed element {ElementType} from layer {LayerName}", 
                            element.GetType().Name, registration.LayerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove element from layer");
        }
    }

    /// <summary>
    /// Change an element's depth within its current layer
    /// </summary>
    public void SetElementDepth(FrameworkElement element, double depth)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(HolographicLayerManager));

        if (element == null || !_elementRegistrations.TryGetValue(element, out var registration))
            return;

        if (!_layers.TryGetValue(registration.LayerName, out var layer))
            return;

        try
        {
            registration.Depth = depth;
            ApplyLayerEffects(element, layer, depth);

            _logger.LogDebug("Updated element {ElementType} depth to {Depth}", 
                            element.GetType().Name, depth);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set element depth");
        }
    }

    /// <summary>
    /// Animate element transition between layers
    /// </summary>
    public void TransitionToLayer(FrameworkElement element, string targetLayerName, 
                                  double duration = 0.5, double? targetDepth = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(HolographicLayerManager));

        if (element == null || !_layers.ContainsKey(targetLayerName))
            return;

        try
        {
            var currentRegistration = _elementRegistrations.GetValueOrDefault(element);
            var targetLayer = _layers[targetLayerName];
            var finalDepth = targetDepth ?? targetLayer.Options.DefaultDepth;

            // Create transition animation
            var storyboard = new Storyboard();

            // Opacity transition
            var opacityAnimation = new DoubleAnimation
            {
                To = targetLayer.Options.OpacityMultiplier * (currentRegistration?.OriginalOpacity ?? 1.0),
                Duration = TimeSpan.FromSeconds(duration),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(opacityAnimation, element);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));
            storyboard.Children.Add(opacityAnimation);

            // Z-Index transition (discrete)
            var zIndexAnimation = new Int32Animation
            {
                To = targetLayer.Options.ZIndexBase + targetLayer.Elements.Count,
                Duration = TimeSpan.FromSeconds(duration * 0.1) // Quick Z change
            };
            Storyboard.SetTarget(zIndexAnimation, element);
            Storyboard.SetTargetProperty(zIndexAnimation, new PropertyPath(Panel.ZIndexProperty));
            storyboard.Children.Add(zIndexAnimation);

            // Complete transition
            storyboard.Completed += (s, e) =>
            {
                AddToLayer(element, targetLayerName, finalDepth);
            };

            storyboard.Begin();

            _logger.LogDebug("Started transition of {ElementType} to layer {LayerName}", 
                            element.GetType().Name, targetLayerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to transition element to layer {LayerName}", targetLayerName);
        }
    }

    /// <summary>
    /// Get layer statistics
    /// </summary>
    public LayerStatistics GetLayerStatistics()
    {
        return new LayerStatistics
        {
            TotalLayers = _layers.Count,
            TotalElements = _elementRegistrations.Count,
            LayerDetails = _layers.ToDictionary(
                kvp => kvp.Key,
                kvp => new LayerDetail
                {
                    ElementCount = kvp.Value.Elements.Count,
                    ZIndexBase = kvp.Value.Options.ZIndexBase,
                    DefaultDepth = kvp.Value.Options.DefaultDepth,
                    Description = kvp.Value.Options.Description
                })
        };
    }

    /// <summary>
    /// Apply depth-based effects to an element
    /// </summary>
    private void ApplyLayerEffects(FrameworkElement element, HolographicLayer layer, double depth)
    {
        try
        {
            // Apply opacity based on depth
            var depthOpacity = CalculateDepthOpacity(depth);
            element.Opacity = layer.Options.OpacityMultiplier * depthOpacity;

            // Apply blur effects based on depth and layer settings
            if (layer.Options.BlurIntensity > 0 && depth > 0.3)
            {
                var blurRadius = layer.Options.BlurIntensity * depth * 10.0;
                element.Effect = new BlurEffect
                {
                    Radius = blurRadius
                };
            }

            // Apply render transform for subtle depth perception
            var depthScale = 1.0 - (depth * 0.1); // Subtle scaling based on depth
            if (element.RenderTransform == null || element.RenderTransform == Transform.Identity)
            {
                element.RenderTransform = new ScaleTransform(depthScale, depthScale);
                element.RenderTransformOrigin = new Point(0.5, 0.5);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply layer effects to element");
        }
    }

    /// <summary>
    /// Calculate opacity based on depth (further = more transparent)
    /// </summary>
    private static double CalculateDepthOpacity(double depth)
    {
        // Linear falloff from full opacity at depth 0 to 30% opacity at depth 1
        return Math.Max(0.3, 1.0 - (depth * 0.7));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            // Remove all elements from layers
            foreach (var element in _elementRegistrations.Keys.ToList())
            {
                RemoveFromLayer(element);
            }

            _layers.Clear();
            _elementRegistrations.Clear();
            _disposed = true;

            _logger.LogInformation("HolographicLayerManager disposed");
        }
    }
}

#region Supporting Classes

/// <summary>
/// Configuration options for a holographic layer
/// </summary>
public class HolographicLayerOptions
{
    /// <summary>
    /// Base Z-Index for elements in this layer
    /// </summary>
    public int ZIndexBase { get; set; }

    /// <summary>
    /// Default depth value for elements (0.0 = front, 1.0 = back)
    /// </summary>
    public double DefaultDepth { get; set; }

    /// <summary>
    /// Blur intensity multiplier for depth effects
    /// </summary>
    public double BlurIntensity { get; set; }

    /// <summary>
    /// Opacity multiplier for layer elements
    /// </summary>
    public double OpacityMultiplier { get; set; } = 1.0;

    /// <summary>
    /// Description of the layer's purpose
    /// </summary>
    public string Description { get; set; } = "";
}

/// <summary>
/// Represents a holographic layer containing elements
/// </summary>
internal class HolographicLayer
{
    public string Name { get; }
    public HolographicLayerOptions Options { get; }
    public List<FrameworkElement> Elements { get; } = new();

    public HolographicLayer(string name, HolographicLayerOptions options)
    {
        Name = name;
        Options = options;
    }
}

/// <summary>
/// Registration information for an element in a layer
/// </summary>
internal class LayerRegistration
{
    public string LayerName { get; set; } = "";
    public double Depth { get; set; }
    public int ZIndex { get; set; }
    public double OriginalOpacity { get; set; } = 1.0;
    public Effect? OriginalEffect { get; set; }
}

/// <summary>
/// Statistics about layer usage
/// </summary>
public class LayerStatistics
{
    public int TotalLayers { get; set; }
    public int TotalElements { get; set; }
    public Dictionary<string, LayerDetail> LayerDetails { get; set; } = new();
}

/// <summary>
/// Details about a specific layer
/// </summary>
public class LayerDetail
{
    public int ElementCount { get; set; }
    public int ZIndexBase { get; set; }
    public double DefaultDepth { get; set; }
    public string Description { get; set; } = "";
}

#endregion