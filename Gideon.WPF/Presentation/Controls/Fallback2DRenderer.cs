// ==========================================================================
// Fallback2DRenderer.cs - 2D Fallback Renderer for Low-End Systems
// ==========================================================================
// Lightweight 2D rendering system that provides similar visual effects
// to the holographic UI but with minimal performance requirements.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Fallback 2D renderer for low-performance systems
/// </summary>
public class Fallback2DRenderer : IDisposable
{
    #region Properties

    public bool IsEnabled { get; private set; }
    public FallbackRenderingMode RenderingMode { get; set; } = FallbackRenderingMode.Automatic;
    public double AnimationIntensity { get; set; } = 0.5; // Reduced default intensity
    public bool EnableParticleEffects { get; set; } = false;
    public bool EnableBlurEffects { get; set; } = false;
    public bool EnableGlowEffects { get; set; } = true; // Keep minimal glow

    #endregion

    #region Fields

    private readonly PerformanceOptimizedRenderer _performanceRenderer;
    private readonly Dictionary<string, Brush> _cachedBrushes = new();
    private readonly Dictionary<string, Effect> _cachedEffects = new();
    private readonly DispatcherTimer _simplifiedAnimationTimer;
    private bool _disposed;

    #endregion

    #region Constructor

    public Fallback2DRenderer(PerformanceOptimizedRenderer performanceRenderer)
    {
        _performanceRenderer = performanceRenderer ?? throw new ArgumentNullException(nameof(performanceRenderer));
        
        // Set up simplified animation timer
        _simplifiedAnimationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // 30 FPS for fallback
        };
        _simplifiedAnimationTimer.Tick += OnSimplifiedAnimationTick;

        // Subscribe to performance level changes
        _performanceRenderer.OnPerformanceLevelChanged += OnPerformanceLevelChanged;

        DetermineRenderingCapability();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Enable fallback mode for low-end systems
    /// </summary>
    public void EnableFallbackMode()
    {
        IsEnabled = true;
        _simplifiedAnimationTimer.Start();
    }

    /// <summary>
    /// Disable fallback mode and return to full rendering
    /// </summary>
    public void DisableFallbackMode()
    {
        IsEnabled = false;
        _simplifiedAnimationTimer.Stop();
    }

    /// <summary>
    /// Create simplified particle system for market data
    /// </summary>
    public Canvas CreateSimplifiedMarketDataVisual(IEnumerable<MarketDataPoint> data)
    {
        var canvas = new Canvas
        {
            Background = GetCachedBrush("MarketBackground", () => 
                new SolidColorBrush(Color.FromArgb(20, 0, 50, 100)))
        };

        if (data?.Any() != true) return canvas;

        var dataPoints = data.Take(20).ToList(); // Limit data points
        
        // Create simplified bars instead of complex particles
        for (int i = 0; i < dataPoints.Count; i++)
        {
            var point = dataPoints[i];
            var bar = CreateSimplifiedDataBar(point, i, dataPoints.Count);
            canvas.Children.Add(bar);
        }

        // Add minimal glow if enabled
        if (EnableGlowEffects)
        {
            canvas.Effect = GetCachedEffect("MarketGlow", () => new DropShadowEffect
            {
                Color = Color.FromRgb(64, 224, 255),
                BlurRadius = 5,
                ShadowDepth = 0,
                Opacity = 0.3
            });
        }

        return canvas;
    }

    /// <summary>
    /// Create simplified price fluctuation visual
    /// </summary>
    public FrameworkElement CreateSimplifiedPriceFluctuation(decimal currentPrice, decimal previousPrice)
    {
        var isPositive = currentPrice > previousPrice;
        var magnitude = Math.Abs((double)((currentPrice - previousPrice) / previousPrice)) * 100;

        var indicator = new Border
        {
            Width = 60,
            Height = 20,
            CornerRadius = new CornerRadius(10),
            Background = GetFluctuationBrush(isPositive, magnitude),
            BorderBrush = GetFluctuationBorderBrush(isPositive),
            BorderThickness = new Thickness(1)
        };

        var content = new TextBlock
        {
            Text = $"{(isPositive ? "↑" : "↓")} {magnitude:F1}%",
            Foreground = Brushes.White,
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        indicator.Child = content;

        // Simple pulsing animation if enabled
        if (AnimationIntensity > 0 && magnitude > 1)
        {
            var pulseAnimation = new DoubleAnimation
            {
                From = 0.7,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(500),
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(3)
            };
            indicator.BeginAnimation(UIElement.OpacityProperty, pulseAnimation);
        }

        return indicator;
    }

    /// <summary>
    /// Create simplified 3D graph representation
    /// </summary>
    public Canvas CreateSimplified3DGraph(IEnumerable<Point3D> dataPoints, GraphType graphType)
    {
        var canvas = new Canvas
        {
            Background = GetCachedBrush("GraphBackground", () => 
                new SolidColorBrush(Color.FromArgb(15, 0, 0, 0)))
        };

        var points = dataPoints?.Take(50).ToList(); // Limit complexity
        if (points?.Any() != true) return canvas;

        switch (graphType)
        {
            case GraphType.Line:
                CreateSimplifiedLineGraph(canvas, points);
                break;
            case GraphType.Bar:
                CreateSimplifiedBarGraph(canvas, points);
                break;
            case GraphType.Scatter:
                CreateSimplifiedScatterGraph(canvas, points);
                break;
            default:
                CreateSimplifiedLineGraph(canvas, points);
                break;
        }

        return canvas;
    }

    /// <summary>
    /// Create simplified tactical HUD
    /// </summary>
    public Grid CreateSimplifiedTacticalHUD()
    {
        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Simplified header
        var header = new Border
        {
            Background = GetCachedBrush("HudHeader", () => 
                new SolidColorBrush(Color.FromArgb(80, 0, 100, 200))),
            Height = 30,
            BorderBrush = new SolidColorBrush(Color.FromRgb(64, 224, 255)),
            BorderThickness = new Thickness(0, 0, 0, 1)
        };

        var headerText = new TextBlock
        {
            Text = "TACTICAL OVERVIEW",
            Foreground = new SolidColorBrush(Color.FromRgb(64, 224, 255)),
            FontWeight = FontWeights.Bold,
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        header.Child = headerText;
        Grid.SetRow(header, 0);
        grid.Children.Add(header);

        // Simplified radar
        var radar = CreateSimplifiedRadar();
        Grid.SetRow(radar, 1);
        grid.Children.Add(radar);

        // Simplified status bar
        var statusBar = CreateSimplifiedStatusBar();
        Grid.SetRow(statusBar, 2);
        grid.Children.Add(statusBar);

        return grid;
    }

    /// <summary>
    /// Create simplified data stream animation
    /// </summary>
    public Canvas CreateSimplifiedDataStream(DataStreamType streamType)
    {
        var canvas = new Canvas
        {
            Background = Brushes.Transparent,
            ClipToBounds = true
        };

        // Create simplified moving dots instead of complex particles
        for (int i = 0; i < 5; i++)
        {
            var dot = new Ellipse
            {
                Width = 4,
                Height = 4,
                Fill = GetDataStreamBrush(streamType),
                Opacity = 0.7
            };

            Canvas.SetLeft(dot, i * 20);
            Canvas.SetTop(dot, 10);
            canvas.Children.Add(dot);

            // Simple movement animation
            if (AnimationIntensity > 0)
            {
                var moveAnimation = new DoubleAnimation
                {
                    From = -10,
                    To = 150,
                    Duration = TimeSpan.FromSeconds(2 + i * 0.5),
                    RepeatBehavior = RepeatBehavior.Forever
                };
                dot.BeginAnimation(Canvas.LeftProperty, moveAnimation);
            }
        }

        return canvas;
    }

    #endregion

    #region Private Methods

    private void DetermineRenderingCapability()
    {
        var metrics = _performanceRenderer.GetCurrentMetrics();
        
        if (RenderingMode == FallbackRenderingMode.Automatic)
        {
            // Auto-enable fallback for low performance levels
            if (metrics.PerformanceLevel >= PerformanceLevel.Medium)
            {
                EnableFallbackMode();
                EnableParticleEffects = false;
                EnableBlurEffects = false;
            }
            else if (metrics.PerformanceLevel == PerformanceLevel.High)
            {
                EnableParticleEffects = true;
                EnableBlurEffects = false;
            }
        }
    }

    private void OnPerformanceLevelChanged(PerformanceLevel newLevel)
    {
        if (RenderingMode != FallbackRenderingMode.Automatic) return;

        switch (newLevel)
        {
            case PerformanceLevel.Low:
                EnableFallbackMode();
                EnableParticleEffects = false;
                EnableBlurEffects = false;
                AnimationIntensity = 0.2;
                break;
            case PerformanceLevel.Medium:
                EnableFallbackMode();
                EnableParticleEffects = false;
                EnableBlurEffects = false;
                AnimationIntensity = 0.5;
                break;
            case PerformanceLevel.High:
                DisableFallbackMode();
                EnableParticleEffects = true;
                EnableBlurEffects = false;
                AnimationIntensity = 0.8;
                break;
            case PerformanceLevel.Maximum:
                DisableFallbackMode();
                EnableParticleEffects = true;
                EnableBlurEffects = true;
                AnimationIntensity = 1.0;
                break;
        }
    }

    private void OnSimplifiedAnimationTick(object sender, EventArgs e)
    {
        // Simplified animation updates if needed
        // Keep minimal to preserve performance
    }

    private Rectangle CreateSimplifiedDataBar(MarketDataPoint data, int index, int total)
    {
        var maxHeight = 80;
        var barWidth = 6;
        var height = Math.Max(5, (double)(data.Volume / 1000000) * maxHeight);

        var bar = new Rectangle
        {
            Width = barWidth,
            Height = height,
            Fill = GetMarketDataBrush(data),
            Stroke = new SolidColorBrush(Color.FromRgb(64, 224, 255)),
            StrokeThickness = 0.5
        };

        Canvas.SetLeft(bar, index * (barWidth + 2));
        Canvas.SetBottom(bar, 0);

        return bar;
    }

    private void CreateSimplifiedLineGraph(Canvas canvas, List<Point3D> points)
    {
        var polyline = new Polyline
        {
            Stroke = new SolidColorBrush(Color.FromRgb(64, 224, 255)),
            StrokeThickness = 2,
            Points = new PointCollection()
        };

        var width = 200;
        var height = 100;

        for (int i = 0; i < points.Count; i++)
        {
            var x = (i / (double)(points.Count - 1)) * width;
            var y = height - (points[i].Z / 10.0 * height); // Simplified Z mapping
            polyline.Points.Add(new Point(x, y));
        }

        canvas.Children.Add(polyline);
    }

    private void CreateSimplifiedBarGraph(Canvas canvas, List<Point3D> points)
    {
        var barWidth = 200.0 / points.Count;

        for (int i = 0; i < points.Count; i++)
        {
            var height = Math.Max(2, points[i].Z * 5);
            var bar = new Rectangle
            {
                Width = barWidth * 0.8,
                Height = height,
                Fill = new SolidColorBrush(Color.FromRgb(50, 205, 50))
            };

            Canvas.SetLeft(bar, i * barWidth);
            Canvas.SetBottom(bar, 0);
            canvas.Children.Add(bar);
        }
    }

    private void CreateSimplifiedScatterGraph(Canvas canvas, List<Point3D> points)
    {
        foreach (var point in points)
        {
            var dot = new Ellipse
            {
                Width = 4,
                Height = 4,
                Fill = new SolidColorBrush(Color.FromRgb(255, 215, 0))
            };

            Canvas.SetLeft(dot, point.X * 10);
            Canvas.SetTop(dot, 100 - point.Z * 10);
            canvas.Children.Add(dot);
        }
    }

    private Canvas CreateSimplifiedRadar()
    {
        var radar = new Canvas
        {
            Width = 150,
            Height = 150,
            Background = GetCachedBrush("RadarBackground", () => 
                new SolidColorBrush(Color.FromArgb(20, 0, 50, 100)))
        };

        // Simple concentric circles
        for (int i = 1; i <= 3; i++)
        {
            var circle = new Ellipse
            {
                Width = i * 50,
                Height = i * 50,
                Stroke = new SolidColorBrush(Color.FromArgb(100, 64, 224, 255)),
                StrokeThickness = 1,
                Fill = Brushes.Transparent
            };

            Canvas.SetLeft(circle, 75 - (i * 25));
            Canvas.SetTop(circle, 75 - (i * 25));
            radar.Children.Add(circle);
        }

        // Simple crosshairs
        var hLine = new Line
        {
            X1 = 0, Y1 = 75, X2 = 150, Y2 = 75,
            Stroke = new SolidColorBrush(Color.FromArgb(80, 64, 224, 255)),
            StrokeThickness = 1
        };
        radar.Children.Add(hLine);

        var vLine = new Line
        {
            X1 = 75, Y1 = 0, X2 = 75, Y2 = 150,
            Stroke = new SolidColorBrush(Color.FromArgb(80, 64, 224, 255)),
            StrokeThickness = 1
        };
        radar.Children.Add(vLine);

        return radar;
    }

    private Border CreateSimplifiedStatusBar()
    {
        var statusBar = new Border
        {
            Background = GetCachedBrush("StatusBackground", () => 
                new SolidColorBrush(Color.FromArgb(60, 0, 0, 0))),
            Height = 25,
            BorderBrush = new SolidColorBrush(Color.FromRgb(64, 224, 255)),
            BorderThickness = new Thickness(0, 1, 0, 0)
        };

        var status = new TextBlock
        {
            Text = "SYSTEM OPERATIONAL",
            Foreground = new SolidColorBrush(Color.FromRgb(50, 205, 50)),
            FontSize = 10,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0)
        };

        statusBar.Child = status;
        return statusBar;
    }

    private Brush GetCachedBrush(string key, Func<Brush> factory)
    {
        if (!_cachedBrushes.TryGetValue(key, out var brush))
        {
            brush = factory();
            _cachedBrushes[key] = brush;
        }
        return brush;
    }

    private Effect GetCachedEffect(string key, Func<Effect> factory)
    {
        if (!_cachedEffects.TryGetValue(key, out var effect))
        {
            effect = factory();
            _cachedEffects[key] = effect;
        }
        return effect;
    }

    private Brush GetFluctuationBrush(bool isPositive, double magnitude)
    {
        var intensity = Math.Min(255, 100 + magnitude * 5);
        var color = isPositive 
            ? Color.FromArgb(150, 0, (byte)intensity, 100)
            : Color.FromArgb(150, (byte)intensity, 50, 50);
        
        return new SolidColorBrush(color);
    }

    private Brush GetFluctuationBorderBrush(bool isPositive)
    {
        return isPositive 
            ? new SolidColorBrush(Color.FromRgb(50, 205, 50))
            : new SolidColorBrush(Color.FromRgb(220, 20, 60));
    }

    private Brush GetMarketDataBrush(MarketDataPoint data)
    {
        // Simple green for positive volume
        return new SolidColorBrush(Color.FromRgb(50, 205, 50));
    }

    private Brush GetDataStreamBrush(DataStreamType streamType)
    {
        return streamType switch
        {
            DataStreamType.MarketData => new SolidColorBrush(Color.FromRgb(50, 205, 50)),
            DataStreamType.CharacterData => new SolidColorBrush(Color.FromRgb(64, 224, 255)),
            DataStreamType.ShipData => new SolidColorBrush(Color.FromRgb(255, 215, 0)),
            DataStreamType.Combat => new SolidColorBrush(Color.FromRgb(255, 64, 64)),
            _ => new SolidColorBrush(Color.FromRgb(128, 128, 128))
        };
    }

    #endregion

    #region Dispose

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _simplifiedAnimationTimer?.Stop();
            
            if (_performanceRenderer != null)
                _performanceRenderer.OnPerformanceLevelChanged -= OnPerformanceLevelChanged;

            _cachedBrushes.Clear();
            _cachedEffects.Clear();
            
            _disposed = true;
        }
    }

    #endregion
}

/// <summary>
/// Fallback rendering modes
/// </summary>
public enum FallbackRenderingMode
{
    Automatic,      // Automatically switch based on performance
    ForceEnabled,   // Always use fallback rendering
    ForceDisabled   // Never use fallback rendering
}