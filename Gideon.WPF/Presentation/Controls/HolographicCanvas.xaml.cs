// ==========================================================================
// HolographicCanvas.xaml.cs - Layered Composition Container Control
// ==========================================================================
// Code-behind for the holographic canvas that provides automatic layer
// management and depth effects for the Westworld-EVE fusion interface.
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
using System.Windows.Threading;
using Gideon.WPF.Infrastructure.Graphics;
using Microsoft.Extensions.Logging;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Holographic canvas with layered composition system
/// </summary>
public partial class HolographicCanvas : UserControl
{
    #region Dependency Properties

    public static readonly DependencyProperty ShowDebugOverlayProperty =
        DependencyProperty.Register(nameof(ShowDebugOverlay), typeof(bool), typeof(HolographicCanvas),
            new PropertyMetadata(false, OnShowDebugOverlayChanged));

    public static readonly DependencyProperty ShowPerformanceOverlayProperty =
        DependencyProperty.Register(nameof(ShowPerformanceOverlay), typeof(bool), typeof(HolographicCanvas),
            new PropertyMetadata(false, OnShowPerformanceOverlayChanged));

    public static readonly DependencyProperty EnableScanLinesProperty =
        DependencyProperty.Register(nameof(EnableScanLines), typeof(bool), typeof(HolographicCanvas),
            new PropertyMetadata(true, OnEnableScanLinesChanged));

    public static readonly DependencyProperty LayerTransitionDurationProperty =
        DependencyProperty.Register(nameof(LayerTransitionDuration), typeof(TimeSpan), typeof(HolographicCanvas),
            new PropertyMetadata(TimeSpan.FromSeconds(0.5)));

    #endregion

    #region Properties

    public bool ShowDebugOverlay
    {
        get => (bool)GetValue(ShowDebugOverlayProperty);
        set => SetValue(ShowDebugOverlayProperty, value);
    }

    public bool ShowPerformanceOverlay
    {
        get => (bool)GetValue(ShowPerformanceOverlayProperty);
        set => SetValue(ShowPerformanceOverlayProperty, value);
    }

    public bool EnableScanLines
    {
        get => (bool)GetValue(EnableScanLinesProperty);
        set => SetValue(EnableScanLinesProperty, value);
    }

    public TimeSpan LayerTransitionDuration
    {
        get => (TimeSpan)GetValue(LayerTransitionDurationProperty);
        set => SetValue(LayerTransitionDurationProperty, value);
    }

    #endregion

    #region Fields

    private readonly ILogger<HolographicCanvas>? _logger;
    private readonly HolographicLayerManager? _layerManager;
    private readonly DispatcherTimer _performanceTimer;
    private readonly Dictionary<string, Canvas> _layerCanvases = new();
    private int _totalElements;
    private DateTime _lastFrameTime = DateTime.Now;
    private double _frameRate = 60.0;

    #endregion

    #region Constructor

    public HolographicCanvas()
    {
        InitializeComponent();
        
        // Initialize layer canvases
        InitializeLayerCanvases();

        // Setup performance monitoring
        _performanceTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _performanceTimer.Tick += OnPerformanceTimerTick;

        // Initialize layer manager (would be injected in real implementation)
        // _layerManager = GetLayerManager();

        Loaded += OnCanvasLoaded;
        Unloaded += OnCanvasUnloaded;
        SizeChanged += OnCanvasSizeChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Add an element to a specific layer
    /// </summary>
    public void AddToLayer(FrameworkElement element, HolographicLayer layer, double depth = 0.0)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        try
        {
            var targetCanvas = GetLayerCanvas(layer);
            if (targetCanvas == null)
            {
                _logger?.LogWarning("Cannot find canvas for layer: {Layer}", layer);
                return;
            }

            // Apply layer-specific styling
            ApplyLayerStyling(element, layer, depth);

            // Add to canvas
            targetCanvas.Children.Add(element);
            _totalElements++;

            // Register with layer manager
            _layerManager?.AddToLayer(element, layer.ToString(), depth);

            _logger?.LogDebug("Added element {ElementType} to layer {Layer} at depth {Depth}", 
                             element.GetType().Name, layer, depth);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to add element to layer {Layer}", layer);
        }
    }

    /// <summary>
    /// Remove an element from all layers
    /// </summary>
    public void RemoveElement(FrameworkElement element)
    {
        if (element == null) return;

        try
        {
            // Remove from all layer canvases
            foreach (var canvas in _layerCanvases.Values)
            {
                if (canvas.Children.Contains(element))
                {
                    canvas.Children.Remove(element);
                    _totalElements--;
                    break;
                }
            }

            // Unregister from layer manager
            _layerManager?.RemoveFromLayer(element);

            _logger?.LogDebug("Removed element {ElementType} from canvas", element.GetType().Name);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to remove element from canvas");
        }
    }

    /// <summary>
    /// Move an element to a different layer with animation
    /// </summary>
    public void TransitionToLayer(FrameworkElement element, HolographicLayer targetLayer, 
                                  double targetDepth = 0.0)
    {
        if (element == null) return;

        try
        {
            // Create transition animation
            var storyboard = new Storyboard();

            // Fade out
            var fadeOut = new DoubleAnimation
            {
                To = 0.3,
                Duration = TimeSpan.FromSeconds(LayerTransitionDuration.TotalSeconds * 0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTarget(fadeOut, element);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath(UIElement.OpacityProperty));
            storyboard.Children.Add(fadeOut);

            // Complete transition when fade out completes
            fadeOut.Completed += (s, e) =>
            {
                // Remove from current layer
                RemoveElement(element);

                // Add to new layer
                AddToLayer(element, targetLayer, targetDepth);

                // Fade in
                var fadeIn = new DoubleAnimation
                {
                    From = 0.3,
                    To = 1.0,
                    Duration = TimeSpan.FromSeconds(LayerTransitionDuration.TotalSeconds * 0.7),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                element.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };

            storyboard.Begin();

            _logger?.LogDebug("Started transition of {ElementType} to layer {Layer}", 
                             element.GetType().Name, targetLayer);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to transition element to layer {Layer}", targetLayer);
        }
    }

    /// <summary>
    /// Clear all elements from all layers
    /// </summary>
    public void ClearAllLayers()
    {
        try
        {
            foreach (var canvas in _layerCanvases.Values)
            {
                canvas.Children.Clear();
            }
            _totalElements = 0;

            _logger?.LogDebug("Cleared all layers");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to clear all layers");
        }
    }

    /// <summary>
    /// Get statistics about layer usage
    /// </summary>
    public HolographicCanvasStatistics GetStatistics()
    {
        return new HolographicCanvasStatistics
        {
            TotalElements = _totalElements,
            LayerElementCounts = _layerCanvases.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Children.Count),
            FrameRate = _frameRate,
            IsDebugMode = ShowDebugOverlay
        };
    }

    #endregion

    #region Private Methods

    private void InitializeLayerCanvases()
    {
        _layerCanvases["Background"] = BackgroundLayer;
        _layerCanvases["MidLayer"] = MidLayer;
        _layerCanvases["Foreground"] = ForegroundLayer;
        _layerCanvases["Overlay"] = OverlayLayer;
        _layerCanvases["Topmost"] = TopmostLayer;
    }

    private Canvas? GetLayerCanvas(HolographicLayer layer)
    {
        var layerName = layer.ToString();
        return _layerCanvases.GetValueOrDefault(layerName);
    }

    private void ApplyLayerStyling(FrameworkElement element, HolographicLayer layer, double depth)
    {
        try
        {
            // Apply base layer styling
            switch (layer)
            {
                case HolographicLayer.Background:
                    if (TryFindResource("BackgroundLayerStyle") is Style backgroundStyle)
                        element.Style = backgroundStyle;
                    break;

                case HolographicLayer.MidLayer:
                    if (TryFindResource("MidLayerStyle") is Style midStyle)
                        element.Style = midStyle;
                    break;

                case HolographicLayer.Foreground:
                    if (TryFindResource("ForegroundLayerStyle") is Style foregroundStyle)
                        element.Style = foregroundStyle;
                    break;

                case HolographicLayer.Overlay:
                    if (TryFindResource("OverlayLayerStyle") is Style overlayStyle)
                        element.Style = overlayStyle;
                    break;
            }

            // Apply depth-specific effects
            ApplyDepthEffects(element, depth);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to apply layer styling");
        }
    }

    private void ApplyDepthEffects(FrameworkElement element, double depth)
    {
        // Apply depth-based opacity
        var depthOpacity = Math.Max(0.3, 1.0 - (depth * 0.7));
        if (element.Opacity == 1.0) // Only modify if not already customized
        {
            element.Opacity = depthOpacity;
        }

        // Apply depth-based scaling
        var depthScale = 1.0 - (depth * 0.1);
        if (element.RenderTransform == null || element.RenderTransform == Transform.Identity)
        {
            element.RenderTransform = new ScaleTransform(depthScale, depthScale);
            element.RenderTransformOrigin = new Point(0.5, 0.5);
        }
    }

    private void UpdateDebugOverlay()
    {
        if (!ShowDebugOverlay || DebugOverlay == null) return;

        try
        {
            // Update layer boundary indicators
            var canvasSize = new Size(ActualWidth, ActualHeight);
            
            if (BackgroundIndicator != null)
            {
                BackgroundIndicator.Width = canvasSize.Width;
                BackgroundIndicator.Height = canvasSize.Height;
            }

            if (MidLayerIndicator != null)
            {
                MidLayerIndicator.Width = canvasSize.Width * 0.9;
                MidLayerIndicator.Height = canvasSize.Height * 0.9;
                Canvas.SetLeft(MidLayerIndicator, canvasSize.Width * 0.05);
                Canvas.SetTop(MidLayerIndicator, canvasSize.Height * 0.05);
            }

            if (ForegroundIndicator != null)
            {
                ForegroundIndicator.Width = canvasSize.Width * 0.8;
                ForegroundIndicator.Height = canvasSize.Height * 0.8;
                Canvas.SetLeft(ForegroundIndicator, canvasSize.Width * 0.1);
                Canvas.SetTop(ForegroundIndicator, canvasSize.Height * 0.1);
            }

            // Update debug info
            if (LayerDebugInfo != null)
            {
                var stats = GetStatistics();
                LayerDebugInfo.Text = $"Layers: {stats.LayerElementCounts.Count} | " +
                                     $"Elements: {stats.TotalElements} | " +
                                     $"FPS: {stats.FrameRate:F1}";
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to update debug overlay");
        }
    }

    private void UpdatePerformanceOverlay()
    {
        if (!ShowPerformanceOverlay) return;

        try
        {
            if (ElementCountText != null)
                ElementCountText.Text = $"Elements: {_totalElements}";

            if (LayerCountText != null)
                LayerCountText.Text = $"Layers: {_layerCanvases.Count}";

            if (FrameRateText != null)
                FrameRateText.Text = $"FPS: {_frameRate:F0}";
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to update performance overlay");
        }
    }

    #endregion

    #region Event Handlers

    private void OnCanvasLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _performanceTimer.Start();
            UpdateDebugOverlay();

            _logger?.LogInformation("HolographicCanvas loaded");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during canvas load");
        }
    }

    private void OnCanvasUnloaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _performanceTimer.Stop();
            ClearAllLayers();

            _logger?.LogInformation("HolographicCanvas unloaded");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during canvas unload");
        }
    }

    private void OnCanvasSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateDebugOverlay();
    }

    private void OnPerformanceTimerTick(object? sender, EventArgs e)
    {
        // Calculate frame rate
        var currentTime = DateTime.Now;
        var deltaTime = currentTime - _lastFrameTime;
        if (deltaTime.TotalSeconds > 0)
        {
            _frameRate = 1.0 / deltaTime.TotalSeconds;
        }
        _lastFrameTime = currentTime;

        UpdatePerformanceOverlay();
    }

    private static void OnShowDebugOverlayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HolographicCanvas canvas && canvas.DebugOverlay != null)
        {
            canvas.DebugOverlay.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            canvas.UpdateDebugOverlay();
        }
    }

    private static void OnShowPerformanceOverlayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HolographicCanvas canvas && canvas.PerformanceOverlay != null)
        {
            canvas.PerformanceOverlay.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnEnableScanLinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HolographicCanvas canvas && canvas.ScanLineOverlay != null)
        {
            canvas.ScanLineOverlay.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    #endregion
}

#region Supporting Enums and Classes

/// <summary>
/// Available holographic layers
/// </summary>
public enum HolographicLayer
{
    Background,
    MidLayer,
    Foreground,
    Overlay,
    Topmost
}

/// <summary>
/// Statistics about the holographic canvas
/// </summary>
public class HolographicCanvasStatistics
{
    public int TotalElements { get; set; }
    public Dictionary<string, int> LayerElementCounts { get; set; } = new();
    public double FrameRate { get; set; }
    public bool IsDebugMode { get; set; }
}

#endregion