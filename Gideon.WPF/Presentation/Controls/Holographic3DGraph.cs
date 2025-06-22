// ==========================================================================
// Holographic3DGraph.cs - 3D Holographic Market Graphs
// ==========================================================================
// Advanced 3D holographic graphs for market data visualization using
// HelixToolkit.Wpf with EVE color schemes and holographic effects.
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
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using HelixToolkit.Wpf;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// 3D holographic graph control for market data visualization with EVE styling
/// </summary>
public class Holographic3DGraph : UserControl
{
    #region Dependency Properties

    public static readonly DependencyProperty DataPointsProperty =
        DependencyProperty.Register(nameof(DataPoints), typeof(IEnumerable<Point3D>), typeof(Holographic3DGraph),
            new PropertyMetadata(null, OnDataPointsChanged));

    public static readonly DependencyProperty GraphTypeProperty =
        DependencyProperty.Register(nameof(GraphType), typeof(GraphType), typeof(Holographic3DGraph),
            new PropertyMetadata(GraphType.Line, OnGraphTypeChanged));

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(Holographic3DGraph),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty IsAnimatedProperty =
        DependencyProperty.Register(nameof(IsAnimated), typeof(bool), typeof(Holographic3DGraph),
            new PropertyMetadata(true, OnIsAnimatedChanged));

    public static readonly DependencyProperty ColorSchemeProperty =
        DependencyProperty.Register(nameof(ColorScheme), typeof(EVEColorScheme), typeof(Holographic3DGraph),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnColorSchemeChanged));

    public static readonly DependencyProperty MarketDataProperty =
        DependencyProperty.Register(nameof(MarketData), typeof(IEnumerable<MarketDataPoint>), typeof(Holographic3DGraph),
            new PropertyMetadata(null, OnMarketDataChanged));

    public static readonly DependencyProperty TimeRangeProperty =
        DependencyProperty.Register(nameof(TimeRange), typeof(TimeSpan), typeof(Holographic3DGraph),
            new PropertyMetadata(TimeSpan.FromHours(24), OnTimeRangeChanged));

    public static readonly DependencyProperty ShowGridProperty =
        DependencyProperty.Register(nameof(ShowGrid), typeof(bool), typeof(Holographic3DGraph),
            new PropertyMetadata(true, OnShowGridChanged));

    public static readonly DependencyProperty ShowVolatilityProperty =
        DependencyProperty.Register(nameof(ShowVolatility), typeof(bool), typeof(Holographic3DGraph),
            new PropertyMetadata(false, OnShowVolatilityChanged));

    #endregion

    #region Properties

    public IEnumerable<Point3D> DataPoints
    {
        get => (IEnumerable<Point3D>)GetValue(DataPointsProperty);
        set => SetValue(DataPointsProperty, value);
    }

    public GraphType GraphType
    {
        get => (GraphType)GetValue(GraphTypeProperty);
        set => SetValue(GraphTypeProperty, value);
    }

    public double HolographicIntensity
    {
        get => (double)GetValue(HolographicIntensityProperty);
        set => SetValue(HolographicIntensityProperty, value);
    }

    public bool IsAnimated
    {
        get => (bool)GetValue(IsAnimatedProperty);
        set => SetValue(IsAnimatedProperty, value);
    }

    public EVEColorScheme ColorScheme
    {
        get => (EVEColorScheme)GetValue(ColorSchemeProperty);
        set => SetValue(ColorSchemeProperty, value);
    }

    public IEnumerable<MarketDataPoint> MarketData
    {
        get => (IEnumerable<MarketDataPoint>)GetValue(MarketDataProperty);
        set => SetValue(MarketDataProperty, value);
    }

    public TimeSpan TimeRange
    {
        get => (TimeSpan)GetValue(TimeRangeProperty);
        set => SetValue(TimeRangeProperty, value);
    }

    public bool ShowGrid
    {
        get => (bool)GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }

    public bool ShowVolatility
    {
        get => (bool)GetValue(ShowVolatilityProperty);
        set => SetValue(ShowVolatilityProperty, value);
    }

    #endregion

    #region Fields

    private HelixViewport3D? _viewport;
    private ModelVisual3D? _model;
    private DispatcherTimer? _animationTimer;
    private double _animationPhase = 0;

    #endregion

    #region Constructor

    public Holographic3DGraph()
    {
        InitializeComponent();
        SetupAnimation();
    }

    #endregion

    #region Private Methods

    private void InitializeComponent()
    {
        _viewport = new HelixViewport3D
        {
            Background = new SolidColorBrush(Color.FromArgb(20, 0, 0, 0)),
            ShowCoordinateSystem = false,
            ShowViewCube = false
        };

        // Set up camera with smooth positioning
        _viewport.Camera = new PerspectiveCamera
        {
            Position = new Point3D(15, 15, 15),
            LookDirection = new Vector3D(-15, -15, -15),
            UpDirection = new Vector3D(0, 0, 1),
            FieldOfView = 45
        };

        // Add holographic glow effect
        _viewport.Effect = new DropShadowEffect
        {
            Color = GetEVEColor(ColorScheme),
            BlurRadius = 20,
            ShadowDepth = 0,
            Opacity = HolographicIntensity * 0.6
        };

        Content = _viewport;
        CreateGraph();
    }

    private void SetupAnimation()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // 60 FPS
        };
        _animationTimer.Tick += OnAnimationTick;
        
        if (IsAnimated)
            _animationTimer.Start();
    }

    private void OnAnimationTick(object sender, EventArgs e)
    {
        _animationPhase += 0.02;
        if (_animationPhase > Math.PI * 2)
            _animationPhase = 0;

        UpdateHolographicEffects();
    }

    private void UpdateHolographicEffects()
    {
        if (_viewport?.Effect is DropShadowEffect effect)
        {
            effect.Opacity = (HolographicIntensity * 0.6) + (Math.Sin(_animationPhase) * 0.2);
        }
    }

    private void CreateGraph()
    {
        if (_viewport == null) return;

        _model = new ModelVisual3D();
        _viewport.Children.Add(_model);

        // Add custom EVE-style lighting
        AddHolographicLighting();

        UpdateGraph();
    }

    private void AddHolographicLighting()
    {
        if (_model == null) return;

        // Primary holographic light
        var primaryLight = new DirectionalLight
        {
            Color = GetEVELightColor(ColorScheme),
            Direction = new Vector3D(-1, -1, -1)
        };
        _model.Children.Add(new ModelVisual3D { Content = primaryLight });

        // Ambient holographic glow
        var ambientLight = new AmbientLight
        {
            Color = Color.FromRgb(20, 40, 60)
        };
        _model.Children.Add(new ModelVisual3D { Content = ambientLight });

        // Accent lighting
        var accentLight = new DirectionalLight
        {
            Color = Color.FromRgb(255, 215, 0), // Gold accent
            Direction = new Vector3D(1, 1, -1)
        };
        _model.Children.Add(new ModelVisual3D { Content = accentLight });
    }

    private void UpdateGraph()
    {
        if (_model == null) return;

        // Clear existing graph but keep lighting
        ClearGraphGeometry();
        AddHolographicLighting();

        // Add holographic grid if enabled
        if (ShowGrid)
        {
            CreateHolographicGrid();
        }

        // Create market data visualization if available
        if (MarketData != null && MarketData.Any())
        {
            CreateMarketDataVisualization();
        }
        // Fallback to generic data points
        else if (DataPoints != null)
        {
            CreateGenericGraph();
        }
    }

    private void CreateGenericGraph()
    {
        // Create graph based on type
        switch (GraphType)
        {
            case GraphType.Line:
                CreateHolographicLineGraph();
                break;
            case GraphType.Surface:
                CreateHolographicSurfaceGraph();
                break;
            case GraphType.Bar:
                CreateHolographicBarGraph();
                break;
            case GraphType.Scatter:
                CreateHolographicScatterGraph();
                break;
        }
    }

    private void CreateMarketDataVisualization()
    {
        var marketData = MarketData.Where(d => d.Timestamp >= DateTime.Now - TimeRange).ToList();
        if (!marketData.Any()) return;

        switch (GraphType)
        {
            case GraphType.Line:
                CreateMarketLineGraph(marketData);
                break;
            case GraphType.Surface:
                CreateMarketSurfaceGraph(marketData);
                break;
            case GraphType.Bar:
                CreateMarketVolumeGraph(marketData);
                break;
            case GraphType.Scatter:
                CreateMarketScatterGraph(marketData);
                break;
        }

        if (ShowVolatility)
        {
            CreateVolatilityVisualization(marketData);
        }
    }

    private void CreateHolographicGrid()
    {
        var gridBuilder = new MeshBuilder();
        var gridSize = 10;
        var gridSpacing = 1.0;
        
        // Create grid lines
        for (int i = -gridSize; i <= gridSize; i++)
        {
            var x = i * gridSpacing;
            // X-direction lines
            gridBuilder.AddTube(
                new Point3D(x, -gridSize * gridSpacing, 0), 
                new Point3D(x, gridSize * gridSpacing, 0), 
                0.02, 0.02, 4);
            
            // Y-direction lines
            gridBuilder.AddTube(
                new Point3D(-gridSize * gridSpacing, x, 0), 
                new Point3D(gridSize * gridSpacing, x, 0), 
                0.02, 0.02, 4);
        }

        var gridMesh = gridBuilder.ToMesh();
        var gridMaterial = CreateHolographicMaterial(ColorScheme, 0.2);
        var gridGeometry = new GeometryModel3D(gridMesh, gridMaterial);
        
        _model.Children.Add(new ModelVisual3D { Content = gridGeometry });
    }

    private void ClearGraphGeometry()
    {
        for (int i = _model.Children.Count - 1; i >= 0; i--)
        {
            if (_model.Children[i] is ModelVisual3D visual && 
                visual.Content is GeometryModel3D)
            {
                _model.Children.RemoveAt(i);
            }
        }
    }

    private void CreateHolographicLineGraph()
    {
        var dataPoints = DataPoints.ToList();
        if (dataPoints.Count < 2) return;

        for (int i = 0; i < dataPoints.Count - 1; i++)
        {
            var start = dataPoints[i];
            var end = dataPoints[i + 1];

            // Create holographic line segment
            var lineBuilder = new MeshBuilder();
            var direction = end - start;
            var length = direction.Length;
            direction.Normalize();

            // Create tube for line
            lineBuilder.AddTube(start, end, 0.1, 0.1, 8);
            var lineMesh = lineBuilder.ToMesh();

            var lineMaterial = CreateHolographicMaterial(ColorScheme, 0.8);
            var lineGeometry = new GeometryModel3D(lineMesh, lineMaterial);
            
            // Add glow effect
            var lineVisual = new ModelVisual3D { Content = lineGeometry };
            _model.Children.Add(lineVisual);
        }

        // Add data point markers
        foreach (var point in dataPoints)
        {
            CreateDataPointMarker(point);
        }
    }

    private void CreateHolographicSurfaceGraph()
    {
        var dataPoints = DataPoints.ToList();
        if (dataPoints.Count < 3) return;

        var surfaceBuilder = new MeshBuilder();
        
        // Create triangulated surface from points
        var gridSize = (int)Math.Sqrt(dataPoints.Count);
        if (gridSize * gridSize == dataPoints.Count)
        {
            // Create grid surface
            for (int i = 0; i < gridSize - 1; i++)
            {
                for (int j = 0; j < gridSize - 1; j++)
                {
                    var p1 = dataPoints[i * gridSize + j];
                    var p2 = dataPoints[i * gridSize + (j + 1)];
                    var p3 = dataPoints[(i + 1) * gridSize + j];
                    var p4 = dataPoints[(i + 1) * gridSize + (j + 1)];

                    surfaceBuilder.AddTriangle(p1, p2, p3);
                    surfaceBuilder.AddTriangle(p2, p4, p3);
                }
            }
        }

        var surfaceMesh = surfaceBuilder.ToMesh();
        var surfaceMaterial = CreateHolographicMaterial(ColorScheme, 0.6);
        var surfaceGeometry = new GeometryModel3D(surfaceMesh, surfaceMaterial);
        
        _model.Children.Add(new ModelVisual3D { Content = surfaceGeometry });
    }

    private void CreateHolographicBarGraph()
    {
        foreach (var point in DataPoints)
        {
            var barBuilder = new MeshBuilder();
            var height = Math.Max(0.1, point.Z);
            var center = new Point3D(point.X, point.Y, height / 2);
            
            barBuilder.AddBox(center, 0.8, 0.8, height);
            var barMesh = barBuilder.ToMesh();

            var barMaterial = CreateHolographicMaterial(ColorScheme, 0.7);
            var barGeometry = new GeometryModel3D(barMesh, barMaterial);
            
            _model.Children.Add(new ModelVisual3D { Content = barGeometry });
        }
    }

    private void CreateHolographicScatterGraph()
    {
        foreach (var point in DataPoints)
        {
            CreateDataPointMarker(point, 0.3);
        }
    }

    private void CreateMarketLineGraph(List<MarketDataPoint> marketData)
    {
        if (marketData.Count < 2) return;

        // Normalize data for 3D space
        var minPrice = marketData.Min(d => d.Price);
        var maxPrice = marketData.Max(d => d.Price);
        var priceRange = maxPrice - minPrice;
        
        var normalizedPoints = marketData.Select((d, i) => new Point3D(
            i * 10.0 / marketData.Count - 5, // X: Time axis
            0, // Y: Keep at center
            (double)((d.Price - minPrice) / priceRange) * 5 // Z: Price axis
        )).ToList();

        // Create price line with varying thickness based on volume
        for (int i = 0; i < normalizedPoints.Count - 1; i++)
        {
            var start = normalizedPoints[i];
            var end = normalizedPoints[i + 1];
            var volume = marketData[i].Volume;
            var thickness = Math.Max(0.05, Math.Log10((double)volume + 1) * 0.03);

            var lineBuilder = new MeshBuilder();
            lineBuilder.AddTube(start, end, thickness, thickness, 8);
            var lineMesh = lineBuilder.ToMesh();

            // Color based on price movement
            var priceChange = marketData[i + 1].Price - marketData[i].Price;
            var lineColorScheme = priceChange >= 0 ? EVEColorScheme.EmeraldGreen : EVEColorScheme.CrimsonRed;
            var lineMaterial = CreateHolographicMaterial(lineColorScheme, 0.8);
            var lineGeometry = new GeometryModel3D(lineMesh, lineMaterial);
            
            _model.Children.Add(new ModelVisual3D { Content = lineGeometry });
        }

        // Add price markers at significant points
        foreach (var (point, data) in normalizedPoints.Zip(marketData))
        {
            if (IsSignificantDataPoint(data, marketData))
            {
                CreateMarketDataMarker(point, data);
            }
        }
    }

    private void CreateMarketSurfaceGraph(List<MarketDataPoint> marketData)
    {
        // Create a 3D surface showing price/volume/time relationships
        var surfaceBuilder = new MeshBuilder();
        var timeSteps = Math.Min(50, marketData.Count);
        var volumeSteps = 20;
        
        // Create surface grid
        for (int t = 0; t < timeSteps - 1; t++)
        {
            for (int v = 0; v < volumeSteps - 1; v++)
            {
                var timeProgress = (double)t / timeSteps;
                var volumeProgress = (double)v / volumeSteps;
                
                var dataIndex = (int)(timeProgress * (marketData.Count - 1));
                var data = marketData[dataIndex];
                
                var x = timeProgress * 10 - 5; // Time axis
                var y = volumeProgress * 5 - 2.5; // Volume axis
                var z = (double)(data.Price - marketData.Min(d => d.Price)) / 
                       (double)(marketData.Max(d => d.Price) - marketData.Min(d => d.Price)) * 5; // Price axis

                var p1 = new Point3D(x, y, z);
                var p2 = new Point3D(x + 10.0/timeSteps, y, z);
                var p3 = new Point3D(x, y + 5.0/volumeSteps, z);
                var p4 = new Point3D(x + 10.0/timeSteps, y + 5.0/volumeSteps, z);

                surfaceBuilder.AddTriangle(p1, p2, p3);
                surfaceBuilder.AddTriangle(p2, p4, p3);
            }
        }

        var surfaceMesh = surfaceBuilder.ToMesh();
        var surfaceMaterial = CreateHolographicMaterial(ColorScheme, 0.5);
        var surfaceGeometry = new GeometryModel3D(surfaceMesh, surfaceMaterial);
        
        _model.Children.Add(new ModelVisual3D { Content = surfaceGeometry });
    }

    private void CreateMarketVolumeGraph(List<MarketDataPoint> marketData)
    {
        var maxVolume = marketData.Max(d => d.Volume);
        
        for (int i = 0; i < marketData.Count; i++)
        {
            var data = marketData[i];
            var barBuilder = new MeshBuilder();
            
            var x = i * 10.0 / marketData.Count - 5;
            var height = (double)(data.Volume / maxVolume) * 5;
            var center = new Point3D(x, 0, height / 2);
            
            barBuilder.AddBox(center, 0.1, 0.1, height);
            var barMesh = barBuilder.ToMesh();

            // Color based on price trend
            var colorScheme = i > 0 && data.Price > marketData[i-1].Price 
                ? EVEColorScheme.EmeraldGreen 
                : EVEColorScheme.CrimsonRed;
            var barMaterial = CreateHolographicMaterial(colorScheme, 0.7);
            var barGeometry = new GeometryModel3D(barMesh, barMaterial);
            
            _model.Children.Add(new ModelVisual3D { Content = barGeometry });
        }
    }

    private void CreateMarketScatterGraph(List<MarketDataPoint> marketData)
    {
        var minPrice = marketData.Min(d => d.Price);
        var maxPrice = marketData.Max(d => d.Price);
        var maxVolume = marketData.Max(d => d.Volume);
        
        foreach (var (data, i) in marketData.Select((d, i) => (d, i)))
        {
            var x = i * 10.0 / marketData.Count - 5; // Time
            var y = (double)(data.Volume / maxVolume) * 5 - 2.5; // Volume
            var z = (double)((data.Price - minPrice) / (maxPrice - minPrice)) * 5; // Price
            
            var point = new Point3D(x, y, z);
            var size = Math.Max(0.1, (double)(data.Volume / maxVolume) * 0.5);
            
            CreateMarketDataMarker(point, data, size);
        }
    }

    private void CreateVolatilityVisualization(List<MarketDataPoint> marketData)
    {
        // Calculate volatility bands
        for (int i = 1; i < marketData.Count; i++)
        {
            var priceChange = Math.Abs((double)(marketData[i].Price - marketData[i-1].Price));
            var avgPrice = (double)(marketData[i].Price + marketData[i-1].Price) / 2;
            var volatility = priceChange / avgPrice;
            
            if (volatility > 0.05) // 5% volatility threshold
            {
                var x = i * 10.0 / marketData.Count - 5;
                var z = (double)((marketData[i].Price - marketData.Min(d => d.Price)) / 
                                (marketData.Max(d => d.Price) - marketData.Min(d => d.Price))) * 5;
                
                // Create volatility indicator
                var volBuilder = new MeshBuilder();
                volBuilder.AddSphere(new Point3D(x, 0, z), 0.2 + volatility * 2, 6, 6);
                var volMesh = volBuilder.ToMesh();
                
                var volMaterial = CreateHolographicMaterial(EVEColorScheme.VoidPurple, 0.6);
                var volGeometry = new GeometryModel3D(volMesh, volMaterial);
                
                _model.Children.Add(new ModelVisual3D { Content = volGeometry });
            }
        }
    }

    private bool IsSignificantDataPoint(MarketDataPoint data, List<MarketDataPoint> allData)
    {
        // Identify significant price movements, high volume, etc.
        var avgVolume = allData.Average(d => (double)d.Volume);
        return (double)data.Volume > avgVolume * 2; // High volume threshold
    }

    private void CreateMarketDataMarker(Point3D point, MarketDataPoint data, double size = 0.15)
    {
        var sphereBuilder = new MeshBuilder();
        sphereBuilder.AddSphere(point, size, 8, 8);
        var sphereMesh = sphereBuilder.ToMesh();

        var sphereMaterial = CreateHolographicMaterial(EVEColorScheme.GoldAccent, 0.9);
        var sphereGeometry = new GeometryModel3D(sphereMesh, sphereMaterial);
        
        _model.Children.Add(new ModelVisual3D { Content = sphereGeometry });
    }

    private void CreateDataPointMarker(Point3D point, double size = 0.2)
    {
        var sphereBuilder = new MeshBuilder();
        sphereBuilder.AddSphere(point, size, 8, 8);
        var sphereMesh = sphereBuilder.ToMesh();

        var sphereMaterial = CreateHolographicMaterial(ColorScheme, 0.9);
        var sphereGeometry = new GeometryModel3D(sphereMesh, sphereMaterial);
        
        _model.Children.Add(new ModelVisual3D { Content = sphereGeometry });
    }

    private Material CreateHolographicMaterial(EVEColorScheme scheme, double opacity)
    {
        var color = GetEVEColor(scheme);
        var brush = new SolidColorBrush(Color.FromArgb(
            (byte)(opacity * 255),
            color.R,
            color.G,
            color.B
        ));

        var material = new MaterialGroup();
        material.Children.Add(new DiffuseMaterial(brush));
        material.Children.Add(new EmissiveMaterial(new SolidColorBrush(Color.FromArgb(
            (byte)(HolographicIntensity * 100),
            color.R,
            color.G,
            color.B
        ))));

        return material;
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

    private Color GetEVELightColor(EVEColorScheme scheme)
    {
        var baseColor = GetEVEColor(scheme);
        return Color.FromRgb(
            (byte)Math.Min(255, baseColor.R + 50),
            (byte)Math.Min(255, baseColor.G + 50),
            (byte)Math.Min(255, baseColor.B + 50)
        );
    }

    #endregion

    #region Event Handlers

    private static void OnDataPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Holographic3DGraph graph)
        {
            graph.UpdateGraph();
        }
    }

    private static void OnGraphTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Holographic3DGraph graph)
        {
            graph.UpdateGraph();
        }
    }

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Holographic3DGraph graph)
        {
            if (graph._viewport?.Effect is DropShadowEffect effect)
            {
                effect.Opacity = graph.HolographicIntensity * 0.6;
            }
            graph.UpdateGraph();
        }
    }

    private static void OnIsAnimatedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Holographic3DGraph graph)
        {
            if ((bool)e.NewValue)
                graph._animationTimer?.Start();
            else
                graph._animationTimer?.Stop();
        }
    }

    private static void OnColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Holographic3DGraph graph)
        {
            if (graph._viewport?.Effect is DropShadowEffect effect)
            {
                effect.Color = graph.GetEVEColor(graph.ColorScheme);
            }
            graph.UpdateGraph();
        }
    }

    private static void OnMarketDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Holographic3DGraph graph)
        {
            graph.UpdateGraph();
        }
    }

    private static void OnTimeRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Holographic3DGraph graph)
        {
            graph.UpdateGraph();
        }
    }

    private static void OnShowGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Holographic3DGraph graph)
        {
            graph.UpdateGraph();
        }
    }

    private static void OnShowVolatilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Holographic3DGraph graph)
        {
            graph.UpdateGraph();
        }
    }

    #endregion

    #region Dispose

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        base.OnUnloaded(e);
    }

    #endregion
}

/// <summary>
/// 3D graph types for holographic visualization
/// </summary>
public enum GraphType
{
    Line,
    Surface,
    Bar,
    Scatter
}

/// <summary>
/// EVE Online color schemes for holographic effects
/// </summary>
public enum EVEColorScheme
{
    ElectricBlue,
    GoldAccent,
    CrimsonRed,
    EmeraldGreen,
    VoidPurple
}

/// <summary>
/// Market data point for 3D visualization
/// </summary>
public class MarketDataPoint
{
    public DateTime Timestamp { get; set; }
    public decimal Price { get; set; }
    public decimal Volume { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Open { get; set; }
    public decimal Close { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int ItemId { get; set; }
    public string Region { get; set; } = string.Empty;
}