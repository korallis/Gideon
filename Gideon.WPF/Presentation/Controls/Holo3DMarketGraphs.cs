// ==========================================================================
// Holo3DMarketGraphs.cs - Holographic 3D Market Graphs
// ==========================================================================
// Advanced 3D market visualization featuring real-time price analysis,
// volume visualization, EVE-style market data, and holographic depth effects.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using HelixToolkit.Wpf;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Holographic 3D market graphs with real-time price visualization and interactive market analysis
/// </summary>
public class Holo3DMarketGraphs : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(Holo3DMarketGraphs),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(Holo3DMarketGraphs),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty MarketDataProperty =
        DependencyProperty.Register(nameof(MarketData), typeof(ObservableCollection<HoloMarketDataPoint>), typeof(Holo3DMarketGraphs),
            new PropertyMetadata(null, OnMarketDataChanged));

    public static readonly DependencyProperty GraphTypeProperty =
        DependencyProperty.Register(nameof(GraphType), typeof(Market3DGraphType), typeof(Holo3DMarketGraphs),
            new PropertyMetadata(Market3DGraphType.PriceVolume, OnGraphTypeChanged));

    public static readonly DependencyProperty TimeRangeProperty =
        DependencyProperty.Register(nameof(TimeRange), typeof(MarketTimeRange), typeof(Holo3DMarketGraphs),
            new PropertyMetadata(MarketTimeRange.Day, OnTimeRangeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(Holo3DMarketGraphs),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(Holo3DMarketGraphs),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowPriceGridProperty =
        DependencyProperty.Register(nameof(ShowPriceGrid), typeof(bool), typeof(Holo3DMarketGraphs),
            new PropertyMetadata(true, OnShowPriceGridChanged));

    public static readonly DependencyProperty ShowVolumeColumnsProperty =
        DependencyProperty.Register(nameof(ShowVolumeColumns), typeof(bool), typeof(Holo3DMarketGraphs),
            new PropertyMetadata(true, OnShowVolumeColumnsChanged));

    public static readonly DependencyProperty InteractiveModeProperty =
        DependencyProperty.Register(nameof(InteractiveMode), typeof(bool), typeof(Holo3DMarketGraphs),
            new PropertyMetadata(true, OnInteractiveModeChanged));

    public static readonly DependencyProperty CameraAutoRotateProperty =
        DependencyProperty.Register(nameof(CameraAutoRotate), typeof(bool), typeof(Holo3DMarketGraphs),
            new PropertyMetadata(false, OnCameraAutoRotateChanged));

    public static readonly DependencyProperty UpdateIntervalProperty =
        DependencyProperty.Register(nameof(UpdateInterval), typeof(TimeSpan), typeof(Holo3DMarketGraphs),
            new PropertyMetadata(TimeSpan.FromMilliseconds(100), OnUpdateIntervalChanged));

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

    public ObservableCollection<HoloMarketDataPoint> MarketData
    {
        get => (ObservableCollection<HoloMarketDataPoint>)GetValue(MarketDataProperty);
        set => SetValue(MarketDataProperty, value);
    }

    public Market3DGraphType GraphType
    {
        get => (Market3DGraphType)GetValue(GraphTypeProperty);
        set => SetValue(GraphTypeProperty, value);
    }

    public MarketTimeRange TimeRange
    {
        get => (MarketTimeRange)GetValue(TimeRangeProperty);
        set => SetValue(TimeRangeProperty, value);
    }

    public bool EnableAnimations
    {
        get => (bool)GetValue(EnableAnimationsProperty);
        set => SetValue(EnableAnimationsProperty, value);
    }

    public bool EnableParticleEffects
    {
        get => (bool)GetValue(EnableParticleEffectsProperty);
        set => SetValue(EnableParticleEffectsProperty, value);
    }

    public bool ShowPriceGrid
    {
        get => (bool)GetValue(ShowPriceGridProperty);
        set => SetValue(ShowPriceGridProperty, value);
    }

    public bool ShowVolumeColumns
    {
        get => (bool)GetValue(ShowVolumeColumnsProperty);
        set => SetValue(ShowVolumeColumnsProperty, value);
    }

    public bool InteractiveMode
    {
        get => (bool)GetValue(InteractiveModeProperty);
        set => SetValue(InteractiveModeProperty, value);
    }

    public bool CameraAutoRotate
    {
        get => (bool)GetValue(CameraAutoRotateProperty);
        set => SetValue(CameraAutoRotateProperty, value);
    }

    public TimeSpan UpdateInterval
    {
        get => (TimeSpan)GetValue(UpdateIntervalProperty);
        set => SetValue(UpdateIntervalProperty, value);
    }

    #endregion

    #region Fields

    private HelixViewport3D _viewport3D;
    private ModelVisual3D _graphModel;
    private ModelVisual3D _priceGridModel;
    private ModelVisual3D _volumeColumnsModel;
    private ModelVisual3D _particleEffectsModel;
    private ModelVisual3D _labelsModel;
    
    private DispatcherTimer _updateTimer;
    private DispatcherTimer _animationTimer;
    private DispatcherTimer _cameraRotationTimer;
    
    private readonly Random _random = new();
    private Dictionary<string, Material> _materialCache;
    private List<Point3D> _dataPoints;
    private double _maxPrice;
    private double _maxVolume;
    private DateTime _startTime;
    private DateTime _endTime;

    // Animation state
    private double _animationProgress = 0.0;
    private bool _isAnimating = false;
    private double _cameraRotationAngle = 0.0;

    // Performance optimization
    private bool _isInSimplifiedMode = false;
    private int _maxDataPoints = 1000;

    #endregion

    #region Constructor

    public Holo3DMarketGraphs()
    {
        InitializeComponent();
        InitializeGraphSystem();
        SetupEventHandlers();
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 800;
        Height = 600;
        
        // Create main layout
        var mainGrid = new Grid();
        Content = mainGrid;

        // Create viewport
        _viewport3D = new HelixViewport3D
        {
            Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
            ShowFrameRate = false,
            ShowTriangleCountInfo = false,
            IsRotationEnabled = true,
            IsZoomEnabled = true,
            IsPanEnabled = true
        };

        // Setup camera
        _viewport3D.Camera = new PerspectiveCamera
        {
            Position = new Point3D(10, 8, 15),
            LookDirection = new Vector3D(-10, -8, -15),
            UpDirection = new Vector3D(0, 1, 0),
            FieldOfView = 45
        };

        mainGrid.Children.Add(_viewport3D);

        // Create overlay for 2D elements
        var overlayCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false
        };
        
        mainGrid.Children.Add(overlayCanvas);
    }

    private void InitializeGraphSystem()
    {
        _materialCache = new Dictionary<string, Material>();
        _dataPoints = new List<Point3D>();
        
        // Create 3D models
        _graphModel = new ModelVisual3D();
        _priceGridModel = new ModelVisual3D();
        _volumeColumnsModel = new ModelVisual3D();
        _particleEffectsModel = new ModelVisual3D();
        _labelsModel = new ModelVisual3D();
        
        _viewport3D.Children.Add(_graphModel);
        _viewport3D.Children.Add(_priceGridModel);
        _viewport3D.Children.Add(_volumeColumnsModel);
        _viewport3D.Children.Add(_particleEffectsModel);
        _viewport3D.Children.Add(_labelsModel);

        // Setup lighting
        SetupLighting();
        
        // Initialize materials
        InitializeMaterials();
        
        // Setup timers
        _updateTimer = new DispatcherTimer
        {
            Interval = UpdateInterval
        };
        _updateTimer.Tick += UpdateTimer_Tick;
        
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // 60 FPS
        };
        _animationTimer.Tick += AnimationTimer_Tick;
        
        _cameraRotationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };
        _cameraRotationTimer.Tick += CameraRotationTimer_Tick;
    }

    private void SetupLighting()
    {
        // Ambient light
        var ambientLight = new AmbientLight(Colors.White)
        {
            Color = Color.FromRgb(40, 40, 60)
        };
        _viewport3D.Children.Add(new ModelVisual3D { Content = ambientLight });

        // Directional lights
        var directionalLight1 = new DirectionalLight
        {
            Color = Color.FromRgb(100, 150, 255),
            Direction = new Vector3D(-1, -1, -1)
        };
        _viewport3D.Children.Add(new ModelVisual3D { Content = directionalLight1 });

        var directionalLight2 = new DirectionalLight
        {
            Color = Color.FromRgb(80, 120, 200),
            Direction = new Vector3D(1, -1, 1)
        };
        _viewport3D.Children.Add(new ModelVisual3D { Content = directionalLight2 });
    }

    private void InitializeMaterials()
    {
        // Price line material
        var priceLineMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(0, 150, 255)));
        priceLineMaterial.Brush.Opacity = 0.8;
        _materialCache["PriceLine"] = priceLineMaterial;

        // Volume column materials
        var buyVolumeMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(0, 255, 100)));
        buyVolumeMaterial.Brush.Opacity = 0.7;
        _materialCache["BuyVolume"] = buyVolumeMaterial;

        var sellVolumeMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255, 100, 0)));
        sellVolumeMaterial.Brush.Opacity = 0.7;
        _materialCache["SellVolume"] = sellVolumeMaterial;

        // Grid material
        var gridMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(100, 150, 200)));
        gridMaterial.Brush.Opacity = 0.3;
        _materialCache["Grid"] = gridMaterial;

        // Holographic glow material
        var glowMaterial = new EmissiveMaterial(new SolidColorBrush(Color.FromRgb(0, 200, 255)));
        _materialCache["Glow"] = glowMaterial;
    }

    private void SetupEventHandlers()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Start3DVisualization();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        Stop3DVisualization();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateLayout();
    }

    private void UpdateTimer_Tick(object sender, EventArgs e)
    {
        UpdateMarketVisualization();
    }

    private void AnimationTimer_Tick(object sender, EventArgs e)
    {
        if (_isAnimating && EnableAnimations)
        {
            _animationProgress += 0.02;
            if (_animationProgress >= 1.0)
            {
                _animationProgress = 1.0;
                _isAnimating = false;
            }
            
            UpdateAnimatedElements();
        }
    }

    private void CameraRotationTimer_Tick(object sender, EventArgs e)
    {
        if (CameraAutoRotate && _viewport3D.Camera is PerspectiveCamera camera)
        {
            _cameraRotationAngle += 0.5;
            if (_cameraRotationAngle >= 360) _cameraRotationAngle = 0;
            
            var radians = _cameraRotationAngle * Math.PI / 180.0;
            var radius = 20.0;
            
            camera.Position = new Point3D(
                radius * Math.Cos(radians),
                8,
                radius * Math.Sin(radians)
            );
            
            camera.LookDirection = new Vector3D(-camera.Position.X, -camera.Position.Y, -camera.Position.Z);
        }
    }

    #endregion

    #region 3D Visualization

    public void Start3DVisualization()
    {
        if (!_updateTimer.IsEnabled)
        {
            _updateTimer.Start();
            
            if (EnableAnimations)
            {
                _animationTimer.Start();
            }
            
            if (CameraAutoRotate)
            {
                _cameraRotationTimer.Start();
            }
            
            UpdateMarketVisualization();
        }
    }

    public void Stop3DVisualization()
    {
        _updateTimer?.Stop();
        _animationTimer?.Stop();
        _cameraRotationTimer?.Stop();
    }

    private void UpdateMarketVisualization()
    {
        if (MarketData == null || !MarketData.Any())
        {
            GenerateSampleData();
        }

        // Clear existing models
        _graphModel.Children.Clear();
        _priceGridModel.Children.Clear();
        _volumeColumnsModel.Children.Clear();
        _particleEffectsModel.Children.Clear();
        _labelsModel.Children.Clear();

        // Calculate data bounds
        CalculateDataBounds();

        // Create visualization based on graph type
        switch (GraphType)
        {
            case Market3DGraphType.PriceVolume:
                CreatePriceVolumeGraph();
                break;
            case Market3DGraphType.OrderBook:
                CreateOrderBookGraph();
                break;
            case Market3DGraphType.Timeline:
                CreateTimelineGraph();
                break;
            case Market3DGraphType.Heatmap:
                CreateHeatmapGraph();
                break;
        }

        // Add grid if enabled
        if (ShowPriceGrid)
        {
            CreatePriceGrid();
        }

        // Add particle effects if enabled
        if (EnableParticleEffects && !_isInSimplifiedMode)
        {
            CreateParticleEffects();
        }

        // Start animation
        StartAnimation();
    }

    private void CalculateDataBounds()
    {
        if (MarketData == null || !MarketData.Any()) return;

        _maxPrice = MarketData.Max(d => Math.Max(d.BuyPrice, d.SellPrice));
        _maxVolume = MarketData.Max(d => Math.Max(d.BuyVolume, d.SellVolume));
        _startTime = MarketData.Min(d => d.Timestamp);
        _endTime = MarketData.Max(d => d.Timestamp);
    }

    private void CreatePriceVolumeGraph()
    {
        if (MarketData == null) return;

        var points = new Point3DCollection();
        var indices = new Int32Collection();
        
        for (int i = 0; i < MarketData.Count; i++)
        {
            var data = MarketData[i];
            var timeNormalized = (data.Timestamp - _startTime).TotalMinutes / (_endTime - _startTime).TotalMinutes;
            var priceNormalized = data.AveragePrice / _maxPrice;
            var volumeNormalized = data.TotalVolume / _maxVolume;
            
            // Create price surface point
            var point = new Point3D(
                timeNormalized * 10,
                priceNormalized * 10,
                volumeNormalized * 5
            );
            
            points.Add(point);
            
            // Create volume columns if enabled
            if (ShowVolumeColumns)
            {
                CreateVolumeColumn(point, data);
            }
        }

        // Create price surface mesh
        if (points.Count > 2)
        {
            CreateSurfaceMesh(points);
        }
    }

    private void CreateOrderBookGraph()
    {
        if (MarketData == null) return;

        foreach (var data in MarketData.Take(50)) // Limit for performance
        {
            var timeNormalized = (data.Timestamp - _startTime).TotalMinutes / (_endTime - _startTime).TotalMinutes;
            
            // Create buy order column
            if (data.BuyVolume > 0)
            {
                var buyHeight = (data.BuyVolume / _maxVolume) * 8;
                var buyPosition = new Point3D(timeNormalized * 10, 0, -2);
                CreateOrderColumn(buyPosition, buyHeight, _materialCache["BuyVolume"]);
            }
            
            // Create sell order column
            if (data.SellVolume > 0)
            {
                var sellHeight = (data.SellVolume / _maxVolume) * 8;
                var sellPosition = new Point3D(timeNormalized * 10, 0, 2);
                CreateOrderColumn(sellPosition, sellHeight, _materialCache["SellVolume"]);
            }
        }
    }

    private void CreateTimelineGraph()
    {
        if (MarketData == null) return;

        var pricePoints = new Point3DCollection();
        
        for (int i = 0; i < MarketData.Count; i++)
        {
            var data = MarketData[i];
            var timeNormalized = (data.Timestamp - _startTime).TotalMinutes / (_endTime - _startTime).TotalMinutes;
            var priceNormalized = data.AveragePrice / _maxPrice;
            
            pricePoints.Add(new Point3D(timeNormalized * 12, priceNormalized * 8, 0));
        }
        
        if (pricePoints.Count > 1)
        {
            CreatePriceLine(pricePoints);
        }
    }

    private void CreateHeatmapGraph()
    {
        if (MarketData == null) return;

        const int gridSize = 20;
        var heatmapData = new double[gridSize, gridSize];
        
        // Calculate heatmap data
        foreach (var data in MarketData)
        {
            var timeIndex = (int)((data.Timestamp - _startTime).TotalMinutes / (_endTime - _startTime).TotalMinutes * (gridSize - 1));
            var priceIndex = (int)(data.AveragePrice / _maxPrice * (gridSize - 1));
            
            timeIndex = Math.Max(0, Math.Min(gridSize - 1, timeIndex));
            priceIndex = Math.Max(0, Math.Min(gridSize - 1, priceIndex));
            
            heatmapData[timeIndex, priceIndex] += data.TotalVolume;
        }
        
        // Create heatmap visualization
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                var intensity = heatmapData[x, y] / _maxVolume;
                if (intensity > 0.01)
                {
                    CreateHeatmapCell(x, y, intensity, gridSize);
                }
            }
        }
    }

    private void CreateVolumeColumn(Point3D basePoint, HoloMarketDataPoint data)
    {
        var buyHeight = (data.BuyVolume / _maxVolume) * 3;
        var sellHeight = (data.SellVolume / _maxVolume) * 3;
        
        if (buyHeight > 0.1)
        {
            var buyColumn = CreateColumn(new Point3D(basePoint.X - 0.2, 0, basePoint.Z), buyHeight, 0.3);
            buyColumn.Material = _materialCache["BuyVolume"];
            _volumeColumnsModel.Children.Add(new ModelVisual3D { Content = buyColumn });
        }
        
        if (sellHeight > 0.1)
        {
            var sellColumn = CreateColumn(new Point3D(basePoint.X + 0.2, 0, basePoint.Z), sellHeight, 0.3);
            sellColumn.Material = _materialCache["SellVolume"];
            _volumeColumnsModel.Children.Add(new ModelVisual3D { Content = sellColumn });
        }
    }

    private void CreateOrderColumn(Point3D position, double height, Material material)
    {
        var column = CreateColumn(position, height, 0.4);
        column.Material = material;
        _volumeColumnsModel.Children.Add(new ModelVisual3D { Content = column });
    }

    private GeometryModel3D CreateColumn(Point3D position, double height, double radius)
    {
        var meshBuilder = new MeshBuilder();
        meshBuilder.AddCylinder(position, new Point3D(position.X, position.Y + height, position.Z), radius, 8);
        
        return new GeometryModel3D
        {
            Geometry = meshBuilder.ToMesh()
        };
    }

    private void CreateSurfaceMesh(Point3DCollection points)
    {
        var meshBuilder = new MeshBuilder();
        
        // Create triangulated surface
        for (int i = 0; i < points.Count - 1; i++)
        {
            if (i % 10 == 9) continue; // Skip end of row
            
            var p1 = points[i];
            var p2 = points[i + 1];
            var p3 = new Point3D(p1.X, 0, p1.Z);
            var p4 = new Point3D(p2.X, 0, p2.Z);
            
            meshBuilder.AddTriangle(p1, p2, p3);
            meshBuilder.AddTriangle(p2, p4, p3);
        }
        
        var surfaceModel = new GeometryModel3D
        {
            Geometry = meshBuilder.ToMesh(),
            Material = _materialCache["PriceLine"]
        };
        
        _graphModel.Children.Add(new ModelVisual3D { Content = surfaceModel });
    }

    private void CreatePriceLine(Point3DCollection points)
    {
        for (int i = 0; i < points.Count - 1; i++)
        {
            var meshBuilder = new MeshBuilder();
            meshBuilder.AddCylinder(points[i], points[i + 1], 0.05, 6);
            
            var lineModel = new GeometryModel3D
            {
                Geometry = meshBuilder.ToMesh(),
                Material = _materialCache["PriceLine"]
            };
            
            _graphModel.Children.Add(new ModelVisual3D { Content = lineModel });
        }
    }

    private void CreateHeatmapCell(int x, int y, double intensity, int gridSize)
    {
        var position = new Point3D(
            (x / (double)gridSize) * 10,
            intensity * 5,
            (y / (double)gridSize) * 10
        );
        
        var meshBuilder = new MeshBuilder();
        meshBuilder.AddBox(position, 10.0 / gridSize * 0.8, intensity * 5, 10.0 / gridSize * 0.8);
        
        var color = GetHeatmapColor(intensity);
        var material = new DiffuseMaterial(new SolidColorBrush(color));
        
        var cellModel = new GeometryModel3D
        {
            Geometry = meshBuilder.ToMesh(),
            Material = material
        };
        
        _graphModel.Children.Add(new ModelVisual3D { Content = cellModel });
    }

    private Color GetHeatmapColor(double intensity)
    {
        // Blue to red heatmap
        var red = (byte)(255 * intensity);
        var blue = (byte)(255 * (1 - intensity));
        return Color.FromRgb(red, 0, blue);
    }

    private void CreatePriceGrid()
    {
        var meshBuilder = new MeshBuilder();
        
        // Horizontal grid lines
        for (int i = 0; i <= 10; i++)
        {
            var y = i;
            meshBuilder.AddCylinder(new Point3D(0, y, 0), new Point3D(10, y, 0), 0.02, 4);
            meshBuilder.AddCylinder(new Point3D(0, y, 0), new Point3D(0, y, 5), 0.02, 4);
        }
        
        // Vertical grid lines
        for (int i = 0; i <= 10; i++)
        {
            var x = i;
            meshBuilder.AddCylinder(new Point3D(x, 0, 0), new Point3D(x, 10, 0), 0.02, 4);
            meshBuilder.AddCylinder(new Point3D(x, 0, 0), new Point3D(x, 0, 5), 0.02, 4);
        }
        
        // Depth grid lines
        for (int i = 0; i <= 5; i++)
        {
            var z = i;
            meshBuilder.AddCylinder(new Point3D(0, 0, z), new Point3D(10, 0, z), 0.02, 4);
            meshBuilder.AddCylinder(new Point3D(0, 0, z), new Point3D(0, 10, z), 0.02, 4);
        }
        
        var gridModel = new GeometryModel3D
        {
            Geometry = meshBuilder.ToMesh(),
            Material = _materialCache["Grid"]
        };
        
        _priceGridModel.Children.Add(new ModelVisual3D { Content = gridModel });
    }

    private void CreateParticleEffects()
    {
        // Create flowing particles representing market activity
        for (int i = 0; i < 20; i++)
        {
            var position = new Point3D(
                _random.NextDouble() * 10,
                _random.NextDouble() * 8,
                _random.NextDouble() * 5
            );
            
            CreateParticle(position);
        }
    }

    private void CreateParticle(Point3D position)
    {
        var meshBuilder = new MeshBuilder();
        meshBuilder.AddSphere(position, 0.1, 6, 6);
        
        var particle = new GeometryModel3D
        {
            Geometry = meshBuilder.ToMesh(),
            Material = _materialCache["Glow"]
        };
        
        _particleEffectsModel.Children.Add(new ModelVisual3D { Content = particle });
    }

    #endregion

    #region Animation

    private void StartAnimation()
    {
        if (EnableAnimations && !_isAnimating)
        {
            _animationProgress = 0.0;
            _isAnimating = true;
        }
    }

    private void UpdateAnimatedElements()
    {
        // Update particle positions
        foreach (ModelVisual3D visual in _particleEffectsModel.Children)
        {
            if (visual.Content is GeometryModel3D model && model.Geometry is MeshGeometry3D mesh)
            {
                // Animate particle movement
                var transform = visual.Transform as TranslateTransform3D ?? new TranslateTransform3D();
                transform.OffsetY += 0.1;
                
                if (transform.OffsetY > 12)
                {
                    transform.OffsetY = -2;
                    transform.OffsetX = _random.NextDouble() * 10;
                    transform.OffsetZ = _random.NextDouble() * 5;
                }
                
                visual.Transform = transform;
            }
        }
        
        // Update material opacity based on animation progress
        foreach (var material in _materialCache.Values)
        {
            if (material is DiffuseMaterial diffuse && diffuse.Brush is SolidColorBrush brush)
            {
                var opacity = 0.3 + (0.5 * _animationProgress);
                brush.Opacity = Math.Min(1.0, opacity);
            }
        }
    }

    #endregion

    #region Sample Data

    private void GenerateSampleData()
    {
        MarketData = new ObservableCollection<HoloMarketDataPoint>();
        var baseTime = DateTime.Now.AddHours(-24);
        var basePrice = 1000000.0; // 1M ISK
        
        for (int i = 0; i < 100; i++)
        {
            var priceVariation = (_random.NextDouble() - 0.5) * 0.2; // ±20% variation
            var volumeVariation = _random.NextDouble() * 0.8 + 0.2; // 20-100% of base volume
            
            var dataPoint = new HoloMarketDataPoint
            {
                Timestamp = baseTime.AddMinutes(i * 14.4), // 24 hours / 100 points
                BuyPrice = basePrice * (1 + priceVariation - 0.01),
                SellPrice = basePrice * (1 + priceVariation + 0.01),
                BuyVolume = (long)(1000 * volumeVariation),
                SellVolume = (long)(1200 * volumeVariation),
                ItemId = 34, // Tritanium
                ItemName = "Tritanium",
                Region = "The Forge"
            };
            
            MarketData.Add(dataPoint);
            
            // Slight price trend
            basePrice *= 1 + (_random.NextDouble() - 0.5) * 0.002;
        }
    }

    #endregion

    #region Public Methods

    public Market3DAnalysis GetMarket3DAnalysis()
    {
        return new Market3DAnalysis
        {
            GraphType = GraphType,
            TimeRange = TimeRange,
            DataPointCount = MarketData?.Count ?? 0,
            MaxPrice = _maxPrice,
            MaxVolume = _maxVolume,
            TimeSpan = _endTime - _startTime,
            IsAnimating = _isAnimating,
            CameraPosition = _viewport3D.Camera?.Position ?? new Point3D(),
            PerformanceMode = _isInSimplifiedMode ? "Simplified" : "Full"
        };
    }

    public void SetGraphType(Market3DGraphType graphType)
    {
        GraphType = graphType;
        UpdateMarketVisualization();
    }

    public void SetTimeRange(MarketTimeRange timeRange)
    {
        TimeRange = timeRange;
        // Regenerate data for new time range
        GenerateSampleData();
        UpdateMarketVisualization();
    }

    public void ResetCamera()
    {
        if (_viewport3D.Camera is PerspectiveCamera camera)
        {
            camera.Position = new Point3D(10, 8, 15);
            camera.LookDirection = new Vector3D(-10, -8, -15);
            camera.UpDirection = new Vector3D(0, 1, 0);
        }
    }

    public void ToggleSimplifiedMode()
    {
        _isInSimplifiedMode = !_isInSimplifiedMode;
        _maxDataPoints = _isInSimplifiedMode ? 200 : 1000;
        UpdateMarketVisualization();
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void SetSimplifiedMode(bool enabled)
    {
        _isInSimplifiedMode = enabled;
        EnableParticleEffects = !enabled;
        EnableAnimations = !enabled;
        _maxDataPoints = enabled ? 200 : 1000;
        
        if (IsLoaded)
        {
            UpdateMarketVisualization();
        }
    }

    public bool IsInSimplifiedMode => _isInSimplifiedMode;

    #endregion

    #region Property Change Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Holo3DMarketGraphs control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Holo3DMarketGraphs control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnMarketDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Holo3DMarketGraphs control && control.IsLoaded)
        {
            control.UpdateMarketVisualization();
        }
    }

    private static void OnGraphTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Holo3DMarketGraphs control && control.IsLoaded)
        {
            control.UpdateMarketVisualization();
        }
    }

    private static void OnTimeRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Holo3DMarketGraphs control && control.IsLoaded)
        {
            control.GenerateSampleData();
            control.UpdateMarketVisualization();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Holo3DMarketGraphs control)
        {
            if ((bool)e.NewValue)
            {
                control._animationTimer?.Start();
            }
            else
            {
                control._animationTimer?.Stop();
            }
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Holo3DMarketGraphs control && control.IsLoaded)
        {
            control.UpdateMarketVisualization();
        }
    }

    private static void OnShowPriceGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Holo3DMarketGraphs control && control.IsLoaded)
        {
            control.UpdateMarketVisualization();
        }
    }

    private static void OnShowVolumeColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Holo3DMarketGraphs control && control.IsLoaded)
        {
            control.UpdateMarketVisualization();
        }
    }

    private static void OnInteractiveModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Holo3DMarketGraphs control)
        {
            var enabled = (bool)e.NewValue;
            control._viewport3D.IsRotationEnabled = enabled;
            control._viewport3D.IsZoomEnabled = enabled;
            control._viewport3D.IsPanEnabled = enabled;
        }
    }

    private static void OnCameraAutoRotateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Holo3DMarketGraphs control)
        {
            if ((bool)e.NewValue)
            {
                control._cameraRotationTimer?.Start();
            }
            else
            {
                control._cameraRotationTimer?.Stop();
            }
        }
    }

    private static void OnUpdateIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Holo3DMarketGraphs control)
        {
            control._updateTimer.Interval = (TimeSpan)e.NewValue;
        }
    }

    private void UpdateHolographicEffects()
    {
        // Update holographic intensity effects
        foreach (var material in _materialCache.Values)
        {
            if (material is DiffuseMaterial diffuse && diffuse.Brush is SolidColorBrush brush)
            {
                brush.Opacity = 0.3 + (0.5 * HolographicIntensity);
            }
        }
    }

    private void UpdateColorScheme()
    {
        // Update materials based on EVE color scheme
        InitializeMaterials();
        if (IsLoaded)
        {
            UpdateMarketVisualization();
        }
    }

    #endregion
}

#region Supporting Classes

public class HoloMarketDataPoint
{
    public DateTime Timestamp { get; set; }
    public double BuyPrice { get; set; }
    public double SellPrice { get; set; }
    public long BuyVolume { get; set; }
    public long SellVolume { get; set; }
    public int ItemId { get; set; }
    public string ItemName { get; set; }
    public string Region { get; set; }
    
    public double AveragePrice => (BuyPrice + SellPrice) / 2.0;
    public long TotalVolume => BuyVolume + SellVolume;
    public double PriceSpread => SellPrice - BuyPrice;
    public double PriceSpreadPercent => (PriceSpread / AveragePrice) * 100.0;
}

public enum Market3DGraphType
{
    PriceVolume,
    OrderBook,
    Timeline,
    Heatmap
}

public enum MarketTimeRange
{
    Hour,
    Day,
    Week,
    Month
}

public class Market3DAnalysis
{
    public Market3DGraphType GraphType { get; set; }
    public MarketTimeRange TimeRange { get; set; }
    public int DataPointCount { get; set; }
    public double MaxPrice { get; set; }
    public double MaxVolume { get; set; }
    public TimeSpan TimeSpan { get; set; }
    public bool IsAnimating { get; set; }
    public Point3D CameraPosition { get; set; }
    public string PerformanceMode { get; set; }
}

#endregion