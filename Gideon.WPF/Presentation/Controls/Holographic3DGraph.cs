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
        if (_model == null || DataPoints == null) return;

        // Clear existing graph but keep lighting
        ClearGraphGeometry();
        AddHolographicLighting();

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