// ==========================================================================
// HoloTargetingSystem.cs - Holographic Targeting System Visualization
// ==========================================================================
// Advanced targeting visualization featuring real-time lock analysis,
// targeting range visualization, EVE-style targeting mechanics, and holographic target tracking.
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

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Holographic targeting system visualization with real-time lock analysis and target tracking
/// </summary>
public class HoloTargetingSystem : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloTargetingSystem),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloTargetingSystem),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty TargetingDataProperty =
        DependencyProperty.Register(nameof(TargetingData), typeof(HoloTargetingData), typeof(HoloTargetingSystem),
            new PropertyMetadata(null, OnTargetingDataChanged));

    public static readonly DependencyProperty TargetsProperty =
        DependencyProperty.Register(nameof(Targets), typeof(ObservableCollection<HoloTarget>), typeof(HoloTargetingSystem),
            new PropertyMetadata(null, OnTargetsChanged));

    public static readonly DependencyProperty VisualizationModeProperty =
        DependencyProperty.Register(nameof(VisualizationMode), typeof(TargetingVisualizationMode), typeof(HoloTargetingSystem),
            new PropertyMetadata(TargetingVisualizationMode.Overview, OnVisualizationModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloTargetingSystem),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloTargetingSystem),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty Show3DVisualizationProperty =
        DependencyProperty.Register(nameof(Show3DVisualization), typeof(bool), typeof(HoloTargetingSystem),
            new PropertyMetadata(true, OnShow3DVisualizationChanged));

    public static readonly DependencyProperty ShowRangeIndicatorsProperty =
        DependencyProperty.Register(nameof(ShowRangeIndicators), typeof(bool), typeof(HoloTargetingSystem),
            new PropertyMetadata(true, OnShowRangeIndicatorsChanged));

    public static readonly DependencyProperty ShowLockingBeamsProperty =
        DependencyProperty.Register(nameof(ShowLockingBeams), typeof(bool), typeof(HoloTargetingSystem),
            new PropertyMetadata(true, OnShowLockingBeamsChanged));

    public static readonly DependencyProperty UpdateIntervalProperty =
        DependencyProperty.Register(nameof(UpdateInterval), typeof(TimeSpan), typeof(HoloTargetingSystem),
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

    public HoloTargetingData TargetingData
    {
        get => (HoloTargetingData)GetValue(TargetingDataProperty);
        set => SetValue(TargetingDataProperty, value);
    }

    public ObservableCollection<HoloTarget> Targets
    {
        get => (ObservableCollection<HoloTarget>)GetValue(TargetsProperty);
        set => SetValue(TargetsProperty, value);
    }

    public TargetingVisualizationMode VisualizationMode
    {
        get => (TargetingVisualizationMode)GetValue(VisualizationModeProperty);
        set => SetValue(VisualizationModeProperty, value);
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

    public bool Show3DVisualization
    {
        get => (bool)GetValue(Show3DVisualizationProperty);
        set => SetValue(Show3DVisualizationProperty, value);
    }

    public bool ShowRangeIndicators
    {
        get => (bool)GetValue(ShowRangeIndicatorsProperty);
        set => SetValue(ShowRangeIndicatorsProperty, value);
    }

    public bool ShowLockingBeams
    {
        get => (bool)GetValue(ShowLockingBeamsProperty);
        set => SetValue(ShowLockingBeamsProperty, value);
    }

    public TimeSpan UpdateInterval
    {
        get => (TimeSpan)GetValue(UpdateIntervalProperty);
        set => SetValue(UpdateIntervalProperty, value);
    }

    #endregion

    #region Fields & Properties

    private readonly DispatcherTimer _animationTimer;
    private readonly DispatcherTimer _updateTimer;
    private readonly Dictionary<string, Storyboard> _lockingAnimations;
    private readonly List<HoloLockingParticle> _lockingParticles;
    private readonly HoloTargetingCalculator _calculator;
    
    private Canvas _mainCanvas;
    private Canvas _particleCanvas;
    private Canvas _beamCanvas;
    private Canvas _radarCanvas;
    private Grid _metricsGrid;
    private Viewport3D _viewport3D;
    private ModelVisual3D _shipModel;
    private ModelVisual3D _targetsModel;

    private double _scanResolution;
    private double _maxTargets;
    private double _lockingRange;
    private double _currentRange;
    private bool _isScanning;
    private DateTime _lastScanTime;
    private readonly Random _random = new();

    // Animation state
    private double _animationProgress;
    private double _radarSweepAngle;
    private readonly Dictionary<string, double> _targetLockProgress;
    private readonly Dictionary<string, Point> _targetPositions;

    #endregion

    #region Constructor & Initialization

    public HoloTargetingSystem()
    {
        _animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) }; // 60 FPS
        _updateTimer = new DispatcherTimer();
        _lockingAnimations = new Dictionary<string, Storyboard>();
        _lockingParticles = new List<HoloLockingParticle>();
        _calculator = new HoloTargetingCalculator();
        _targetLockProgress = new Dictionary<string, double>();
        _targetPositions = new Dictionary<string, Point>();

        _animationTimer.Tick += OnAnimationTimerTick;
        _updateTimer.Tick += OnUpdateTimerTick;
        _lastScanTime = DateTime.Now;

        InitializeComponent();
        CreateHolographicInterface();
        StartAnimations();
    }

    private void InitializeComponent()
    {
        Width = 600;
        Height = 750;
        Background = new SolidColorBrush(Color.FromArgb(25, 150, 0, 150));

        // Apply holographic effects
        Effect = new DropShadowEffect
        {
            Color = Colors.Red,
            BlurRadius = 20,
            ShadowDepth = 0,
            Opacity = 0.6
        };
    }

    private void CreateHolographicInterface()
    {
        var mainGrid = new Grid();
        Content = mainGrid;

        // Define rows for layout
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(250) });

        // Create main canvas for targeting visualization
        _mainCanvas = new Canvas
        {
            Background = new SolidColorBrush(Color.FromArgb(15, 255, 0, 100)),
            ClipToBounds = true
        };
        Grid.SetRow(_mainCanvas, 0);
        mainGrid.Children.Add(_mainCanvas);

        // Create particle canvas overlay
        _particleCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false
        };
        _mainCanvas.Children.Add(_particleCanvas);

        // Create beam canvas for locking beams
        _beamCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false
        };
        _mainCanvas.Children.Add(_beamCanvas);

        // Create radar canvas
        _radarCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false
        };
        _mainCanvas.Children.Add(_radarCanvas);

        // Create 3D visualization
        if (Show3DVisualization)
        {
            Create3DVisualization();
        }

        // Create radar display
        CreateRadarDisplay();

        // Create range indicators
        if (ShowRangeIndicators)
        {
            CreateRangeIndicators();
        }

        // Create target list
        CreateTargetList();

        // Create metrics display
        CreateMetricsDisplay();
    }

    private void Create3DVisualization()
    {
        _viewport3D = new Viewport3D
        {
            Width = 300,
            Height = 300
        };
        Canvas.SetLeft(_viewport3D, 150);
        Canvas.SetTop(_viewport3D, 50);
        _mainCanvas.Children.Add(_viewport3D);

        // Setup camera
        var camera = new PerspectiveCamera
        {
            Position = new Point3D(0, 5, 10),
            LookDirection = new Vector3D(0, -0.5, -1),
            UpDirection = new Vector3D(0, 1, 0),
            FieldOfView = 60
        };
        _viewport3D.Camera = camera;

        // Create ship model (center)
        CreateShip3DModel();

        // Create targets model group
        CreateTargets3DModel();

        // Add lighting
        CreateLighting3D();
    }

    private void CreateShip3DModel()
    {
        _shipModel = new ModelVisual3D();
        _viewport3D.Children.Add(_shipModel);

        // Create ship geometry
        var shipGeometry = new MeshGeometry3D();
        CreateShipGeometry(shipGeometry);

        // Create ship material
        var shipMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Cyan))
        {
            Brush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(200, 0, 255, 255), 0.0),
                    new GradientStop(Color.FromArgb(255, 0, 200, 255), 0.5),
                    new GradientStop(Color.FromArgb(150, 0, 150, 255), 1.0)
                }
            }
        };

        var shipModel = new GeometryModel3D
        {
            Geometry = shipGeometry,
            Material = shipMaterial
        };

        _shipModel.Content = shipModel;
    }

    private void CreateTargets3DModel()
    {
        _targetsModel = new ModelVisual3D();
        _viewport3D.Children.Add(_targetsModel);

        var targetsGroup = new Model3DGroup();
        _targetsModel.Content = targetsGroup;
    }

    private void CreateLighting3D()
    {
        var ambientLight = new AmbientLight(Colors.DarkRed) { Color = Color.FromRgb(40, 20, 20) };
        var directionalLight = new DirectionalLight(Colors.White) { Direction = new Vector3D(-1, -1, -1) };
        var targetingLight = new PointLight(Colors.Red) 
        { 
            Position = new Point3D(0, 3, 5),
            Range = 15,
            ConstantAttenuation = 0.1,
            LinearAttenuation = 0.05
        };
        
        _viewport3D.Children.Add(new ModelVisual3D { Content = ambientLight });
        _viewport3D.Children.Add(new ModelVisual3D { Content = directionalLight });
        _viewport3D.Children.Add(new ModelVisual3D { Content = targetingLight });
    }

    private void CreateRadarDisplay()
    {
        var radarContainer = new Border
        {
            Width = 250,
            Height = 250,
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)),
            BorderBrush = new SolidColorBrush(Colors.Red),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(125)
        };
        Canvas.SetLeft(radarContainer, 50);
        Canvas.SetTop(radarContainer, 200);
        _mainCanvas.Children.Add(radarContainer);

        var radarGrid = new Canvas
        {
            Name = "RadarGrid",
            Background = Brushes.Transparent
        };
        radarContainer.Child = radarGrid;

        // Create radar rings
        for (int i = 1; i <= 4; i++)
        {
            var ring = new Ellipse
            {
                Width = i * 60,
                Height = i * 60,
                Stroke = new SolidColorBrush(Color.FromArgb(100, 255, 0, 0)),
                StrokeThickness = 1
            };
            Canvas.SetLeft(ring, 125 - (i * 30));
            Canvas.SetTop(ring, 125 - (i * 30));
            radarGrid.Children.Add(ring);
        }

        // Create radar crosshairs
        var hLine = new Line
        {
            X1 = 0, Y1 = 125, X2 = 250, Y2 = 125,
            Stroke = new SolidColorBrush(Color.FromArgb(80, 255, 0, 0)),
            StrokeThickness = 1
        };
        radarGrid.Children.Add(hLine);

        var vLine = new Line
        {
            X1 = 125, Y1 = 0, X2 = 125, Y2 = 250,
            Stroke = new SolidColorBrush(Color.FromArgb(80, 255, 0, 0)),
            StrokeThickness = 1
        };
        radarGrid.Children.Add(vLine);

        // Create radar sweep
        var sweepLine = new Line
        {
            Name = "RadarSweep",
            X1 = 125, Y1 = 125, X2 = 125, Y2 = 5,
            Stroke = new SolidColorBrush(Colors.Red),
            StrokeThickness = 2,
            Effect = new DropShadowEffect
            {
                Color = Colors.Red,
                BlurRadius = 10,
                ShadowDepth = 0,
                Opacity = 0.8
            }
        };
        radarGrid.Children.Add(sweepLine);

        // Center dot
        var center = new Ellipse
        {
            Width = 6,
            Height = 6,
            Fill = new SolidColorBrush(Colors.Cyan)
        };
        Canvas.SetLeft(center, 122);
        Canvas.SetTop(center, 122);
        radarGrid.Children.Add(center);
    }

    private void CreateRangeIndicators()
    {
        var rangePanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(20)
        };
        Canvas.SetRight(rangePanel, 20);
        Canvas.SetTop(rangePanel, 50);
        _mainCanvas.Children.Add(rangePanel);

        // Create range displays
        CreateRangeDisplay(rangePanel, "Lock Range", "150 km", Colors.Red);
        CreateRangeDisplay(rangePanel, "Scan Range", "200 km", Colors.Orange);
        CreateRangeDisplay(rangePanel, "Max Range", "250 km", Colors.Yellow);
    }

    private void CreateRangeDisplay(Panel container, string label, string value, Color color)
    {
        var rangeGrid = new Grid
        {
            Width = 140,
            Height = 35,
            Margin = new Thickness(0, 5)
        };

        rangeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        rangeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Range label
        var labelText = new TextBlock
        {
            Text = label,
            Foreground = new SolidColorBrush(color),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(labelText, 0);
        rangeGrid.Children.Add(labelText);

        // Range value
        var valueText = new TextBlock
        {
            Name = $"{label.Replace(" ", "")}Value",
            Text = value,
            Foreground = new SolidColorBrush(color),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 3,
                ShadowDepth = 0,
                Opacity = 0.8
            }
        };
        Grid.SetColumn(valueText, 1);
        rangeGrid.Children.Add(valueText);

        container.Children.Add(rangeGrid);
    }

    private void CreateTargetList()
    {
        var targetListContainer = new Border
        {
            Width = 200,
            Height = 300,
            Background = new SolidColorBrush(Color.FromArgb(60, 0, 0, 0)),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5)
        };
        Canvas.SetLeft(targetListContainer, 20);
        Canvas.SetTop(targetListContainer, 50);
        _mainCanvas.Children.Add(targetListContainer);

        var targetList = new ScrollViewer
        {
            Name = "TargetList",
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };
        targetListContainer.Child = targetList;

        var targetStackPanel = new StackPanel
        {
            Name = "TargetStackPanel",
            Orientation = Orientation.Vertical
        };
        targetList.Content = targetStackPanel;

        // Add title
        var title = new TextBlock
        {
            Text = "Target List",
            Foreground = new SolidColorBrush(Colors.Red),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 10)
        };
        targetStackPanel.Children.Add(title);
    }

    private void CreateMetricsDisplay()
    {
        _metricsGrid = new Grid
        {
            Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0))
        };
        Grid.SetRow(_metricsGrid, 1);
        ((Grid)Content).Children.Add(_metricsGrid);

        // Define rows and columns for metrics
        _metricsGrid.RowDefinitions.Add(new RowDefinition());
        _metricsGrid.RowDefinitions.Add(new RowDefinition());
        _metricsGrid.RowDefinitions.Add(new RowDefinition());
        
        _metricsGrid.ColumnDefinitions.Add(new ColumnDefinition());
        _metricsGrid.ColumnDefinitions.Add(new ColumnDefinition());
        _metricsGrid.ColumnDefinitions.Add(new ColumnDefinition());

        // Create metric displays
        CreateMetricDisplay("Targets", "3/8", Colors.Red, 0, 0);
        CreateMetricDisplay("Locked", "2", Colors.Orange, 1, 0);
        CreateMetricDisplay("Locking", "1", Colors.Yellow, 2, 0);

        CreateMetricDisplay("Scan Res", "215 mm", Colors.Cyan, 0, 1);
        CreateMetricDisplay("Lock Time", "4.2 s", Colors.LimeGreen, 1, 1);
        CreateMetricDisplay("Range", "187 km", Colors.Pink, 2, 1);

        CreateMetricDisplay("Signature", "125 m", Colors.Gray, 0, 2);
        CreateMetricDisplay("Velocity", "2,847 m/s", Colors.White, 1, 2);
        CreateMetricDisplay("Status", "Active", Colors.LimeGreen, 2, 2);
    }

    private void CreateMetricDisplay(string label, string value, Color color, int column, int row)
    {
        var container = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8)
        };

        var labelText = new TextBlock
        {
            Text = label,
            Foreground = new SolidColorBrush(Colors.Gray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 10,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var valueText = new TextBlock
        {
            Text = value,
            Foreground = new SolidColorBrush(color),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 13,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 4,
                ShadowDepth = 0,
                Opacity = 0.8
            }
        };

        container.Children.Add(labelText);
        container.Children.Add(valueText);

        Grid.SetColumn(container, column);
        Grid.SetRow(container, row);
        _metricsGrid.Children.Add(container);
    }

    #endregion

    #region Animation Methods

    private void StartAnimations()
    {
        if (EnableAnimations)
        {
            _animationTimer.Start();
            _updateTimer.Interval = UpdateInterval;
            _updateTimer.Start();
        }
    }

    private void StopAnimations()
    {
        _animationTimer.Stop();
        _updateTimer.Stop();
    }

    private void OnAnimationTimerTick(object sender, EventArgs e)
    {
        if (!EnableAnimations)
            return;

        UpdateParticleEffects();
        UpdateRadarSweep();
        UpdateLockingBeams();
        Update3DEffects();
        UpdateTargetAnimations();
    }

    private void OnUpdateTimerTick(object sender, EventArgs e)
    {
        if (TargetingData != null)
        {
            CalculateTargetingMetrics();
            UpdateTargetList();
            UpdateMetricsDisplay();
            UpdateTargetPositions();
        }
    }

    private void UpdateParticleEffects()
    {
        if (!EnableParticleEffects)
            return;

        // Update existing locking particles
        for (int i = _lockingParticles.Count - 1; i >= 0; i--)
        {
            var particle = _lockingParticles[i];
            particle.Update();

            if (particle.IsExpired)
            {
                _particleCanvas.Children.Remove(particle.Visual);
                _lockingParticles.RemoveAt(i);
            }
        }

        // Create new locking particles when targets are being locked
        if (_isScanning && _random.NextDouble() < 0.3)
        {
            CreateLockingParticle();
        }
    }

    private void UpdateRadarSweep()
    {
        _radarSweepAngle += 2.0; // 2 degrees per frame
        if (_radarSweepAngle >= 360)
            _radarSweepAngle = 0;

        var sweepLine = FindChildByName<Line>("RadarSweep");
        if (sweepLine != null)
        {
            var angleRad = _radarSweepAngle * Math.PI / 180;
            var endX = 125 + Math.Sin(angleRad) * 120;
            var endY = 125 - Math.Cos(angleRad) * 120;
            
            sweepLine.X2 = endX;
            sweepLine.Y2 = endY;
        }
    }

    private void UpdateLockingBeams()
    {
        if (!ShowLockingBeams)
            return;

        // Clear existing beams
        _beamCanvas.Children.Clear();

        // Create beams to locked/locking targets
        if (Targets != null)
        {
            var shipCenter = new Point(300, 300); // Ship position in canvas

            foreach (var target in Targets.Where(t => t.LockState != TargetLockState.Unlocked))
            {
                if (_targetPositions.TryGetValue(target.Id, out var targetPos))
                {
                    CreateLockingBeam(shipCenter, targetPos, target);
                }
            }
        }
    }

    private void CreateLockingBeam(Point start, Point end, HoloTarget target)
    {
        var beam = new Line
        {
            X1 = start.X,
            Y1 = start.Y,
            X2 = end.X,
            Y2 = end.Y,
            StrokeThickness = target.LockState == TargetLockState.Locked ? 3 : 2,
            Stroke = new SolidColorBrush(GetTargetStateColor(target.LockState))
        };

        // Add beam effect based on lock state
        if (target.LockState == TargetLockState.Locking)
        {
            beam.StrokeDashArray = new DoubleCollection { 5, 5 };
            
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 10,
                Duration = TimeSpan.FromSeconds(0.5),
                RepeatBehavior = RepeatBehavior.Forever
            };
            
            beam.BeginAnimation(Shape.StrokeDashOffsetProperty, animation);
        }

        beam.Effect = new DropShadowEffect
        {
            Color = GetTargetStateColor(target.LockState),
            BlurRadius = 8,
            ShadowDepth = 0,
            Opacity = 0.6
        };

        _beamCanvas.Children.Add(beam);
    }

    private void Update3DEffects()
    {
        if (!Show3DVisualization)
            return;

        // Update targets in 3D space
        UpdateTargets3D();

        // Rotate ship slightly based on scanning
        if (_shipModel != null && _isScanning)
        {
            var rotationAngle = _animationProgress * 360;
            var rotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), rotationAngle * 0.1);
            var rotationTransform = new RotateTransform3D(rotation);
            _shipModel.Transform = rotationTransform;
        }
    }

    private void UpdateTargets3D()
    {
        if (_targetsModel?.Content is Model3DGroup targetsGroup)
        {
            targetsGroup.Children.Clear();

            if (Targets != null)
            {
                foreach (var target in Targets)
                {
                    var targetModel = CreateTarget3DModel(target);
                    if (targetModel != null)
                        targetsGroup.Children.Add(targetModel);
                }
            }
        }
    }

    private GeometryModel3D CreateTarget3DModel(HoloTarget target)
    {
        var geometry = new MeshGeometry3D();
        CreateSphereGeometry(geometry, 0.3, 8);

        var color = GetTargetStateColor(target.LockState);
        var material = new EmissiveMaterial(new SolidColorBrush(color));

        var model = new GeometryModel3D
        {
            Geometry = geometry,
            Material = material
        };

        // Position target in 3D space based on distance and angle
        var angle = target.Bearing * Math.PI / 180;
        var distance = Math.Min(target.Distance / 50000, 8); // Scale distance for 3D view
        
        var x = Math.Sin(angle) * distance;
        var z = Math.Cos(angle) * distance;
        var y = (_random.NextDouble() - 0.5) * 2; // Random height

        var transform = new TranslateTransform3D(x, y, z);
        model.Transform = transform;

        return model;
    }

    private void UpdateTargetAnimations()
    {
        _animationProgress += 0.01;
        if (_animationProgress > 1.0)
            _animationProgress = 0.0;

        // Update target lock progress animations
        if (Targets != null)
        {
            foreach (var target in Targets.Where(t => t.LockState == TargetLockState.Locking))
            {
                if (!_targetLockProgress.ContainsKey(target.Id))
                    _targetLockProgress[target.Id] = 0.0;

                _targetLockProgress[target.Id] += 0.02; // 2% per frame
                if (_targetLockProgress[target.Id] >= 1.0)
                {
                    target.LockState = TargetLockState.Locked;
                    _targetLockProgress[target.Id] = 1.0;
                }
            }
        }
    }

    private void CreateLockingParticle()
    {
        var particle = new HoloLockingParticle
        {
            Position = new Point(_random.NextDouble() * _mainCanvas.ActualWidth, 
                               _random.NextDouble() * _mainCanvas.ActualHeight),
            Velocity = new Vector(_random.NextDouble() * 4 - 2, _random.NextDouble() * 4 - 2),
            Color = GetRandomLockingColor(),
            Size = _random.NextDouble() * 2 + 1,
            Life = TimeSpan.FromSeconds(_random.NextDouble() * 2 + 1)
        };

        particle.CreateVisual();
        _particleCanvas.Children.Add(particle.Visual);
        _lockingParticles.Add(particle);
    }

    #endregion

    #region Calculation Methods

    private void CalculateTargetingMetrics()
    {
        if (TargetingData == null)
            return;

        var now = DateTime.Now;
        var deltaTime = (now - _lastScanTime).TotalSeconds;
        _lastScanTime = now;

        // Calculate targeting metrics
        _scanResolution = TargetingData.ScanResolution;
        _maxTargets = TargetingData.MaxTargets;
        _lockingRange = TargetingData.LockingRange;

        // Determine if actively scanning/locking
        _isScanning = Targets?.Any(t => t.LockState == TargetLockState.Locking) ?? false;

        // Update current range based on closest target
        _currentRange = Targets?.Where(t => t.LockState != TargetLockState.Unlocked)
                               .Min(t => t.Distance) ?? 0;
    }

    private void UpdateTargetList()
    {
        var stackPanel = FindChildByName<StackPanel>("TargetStackPanel");
        if (stackPanel == null)
            return;

        // Clear existing target entries (keep title)
        while (stackPanel.Children.Count > 1)
        {
            stackPanel.Children.RemoveAt(1);
        }

        // Add current targets
        if (Targets != null)
        {
            foreach (var target in Targets.OrderBy(t => t.Distance))
            {
                var targetEntry = CreateTargetEntry(target);
                stackPanel.Children.Add(targetEntry);
            }
        }
    }

    private FrameworkElement CreateTargetEntry(HoloTarget target)
    {
        var container = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 100, 100, 100)),
            BorderBrush = new SolidColorBrush(GetTargetStateColor(target.LockState)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3),
            Margin = new Thickness(5, 2),
            Padding = new Thickness(5)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });

        var infoPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var nameText = new TextBlock
        {
            Text = target.Name,
            Foreground = new SolidColorBrush(GetTargetStateColor(target.LockState)),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 10,
            FontWeight = FontWeights.Bold
        };
        infoPanel.Children.Add(nameText);

        var infoText = new TextBlock
        {
            Text = $"{target.Type} | {target.Distance:N0} km",
            Foreground = new SolidColorBrush(Colors.Gray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 8
        };
        infoPanel.Children.Add(infoText);

        Grid.SetColumn(infoPanel, 0);
        grid.Children.Add(infoPanel);

        // Lock progress indicator
        if (target.LockState == TargetLockState.Locking)
        {
            var progressBar = new Border
            {
                Width = 50,
                Height = 6,
                Background = new SolidColorBrush(Color.FromArgb(50, 255, 255, 0)),
                BorderBrush = new SolidColorBrush(Colors.Yellow),
                BorderThickness = new Thickness(1),
                VerticalAlignment = VerticalAlignment.Center
            };

            var progress = _targetLockProgress.TryGetValue(target.Id, out var p) ? p : 0.0;
            var progressFill = new Rectangle
            {
                Fill = new SolidColorBrush(Colors.Yellow),
                Width = 48 * progress,
                Height = 4,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            progressBar.Child = progressFill;

            Grid.SetColumn(progressBar, 1);
            grid.Children.Add(progressBar);
        }

        container.Child = grid;
        return container;
    }

    private void UpdateTargetPositions()
    {
        if (Targets == null)
            return;

        _targetPositions.Clear();

        foreach (var target in Targets)
        {
            // Calculate position on radar based on bearing and distance
            var angle = target.Bearing * Math.PI / 180;
            var radarRadius = Math.Min(target.Distance / _lockingRange, 1.0) * 100; // Scale to radar

            var x = 175 + Math.Sin(angle) * radarRadius; // Radar center at 175,325
            var y = 325 - Math.Cos(angle) * radarRadius;

            _targetPositions[target.Id] = new Point(x, y);

            // Update radar blip
            UpdateRadarBlip(target, new Point(x - 125, y - 200)); // Adjust for radar container
        }
    }

    private void UpdateRadarBlip(HoloTarget target, Point position)
    {
        var radarGrid = FindChildByName<Canvas>("RadarGrid");
        if (radarGrid == null)
            return;

        // Remove existing blip
        var existingBlip = radarGrid.Children.OfType<Ellipse>()
            .FirstOrDefault(e => e.Name == $"Blip_{target.Id}");
        if (existingBlip != null)
            radarGrid.Children.Remove(existingBlip);

        // Create new blip
        var blip = new Ellipse
        {
            Name = $"Blip_{target.Id}",
            Width = 6,
            Height = 6,
            Fill = new SolidColorBrush(GetTargetStateColor(target.LockState)),
            Effect = new DropShadowEffect
            {
                Color = GetTargetStateColor(target.LockState),
                BlurRadius = 4,
                ShadowDepth = 0,
                Opacity = 0.8
            }
        };

        Canvas.SetLeft(blip, position.X - 3);
        Canvas.SetTop(blip, position.Y - 3);
        radarGrid.Children.Add(blip);
    }

    private void UpdateMetricsDisplay()
    {
        if (TargetingData == null || Targets == null)
            return;

        var totalTargets = Targets.Count;
        var lockedTargets = Targets.Count(t => t.LockState == TargetLockState.Locked);
        var lockingTargets = Targets.Count(t => t.LockState == TargetLockState.Locking);

        UpdateMetricValue("Targets", $"{totalTargets}/{_maxTargets:F0}");
        UpdateMetricValue("Locked", lockedTargets.ToString());
        UpdateMetricValue("Locking", lockingTargets.ToString());

        UpdateMetricValue("Scan Res", $"{_scanResolution:F0} mm");
        
        var avgLockTime = _calculator.CalculateAverageLockTime(TargetingData, Targets);
        UpdateMetricValue("Lock Time", $"{avgLockTime:F1} s");
        
        UpdateMetricValue("Range", $"{_currentRange:F0} km");

        var avgSignature = Targets.Average(t => t.SignatureRadius);
        var avgVelocity = Targets.Average(t => t.Velocity);
        
        UpdateMetricValue("Signature", $"{avgSignature:F0} m");
        UpdateMetricValue("Velocity", $"{avgVelocity:F0} m/s");
        UpdateMetricValue("Status", _isScanning ? "Scanning" : "Idle");

        // Update metric colors based on status
        UpdateMetricColor("Status", _isScanning ? Colors.LimeGreen : Colors.Gray);
        UpdateMetricColor("Targets", totalTargets >= _maxTargets ? Colors.Red : Colors.LimeGreen);
    }

    #endregion

    #region Helper Methods

    private Color GetTargetStateColor(TargetLockState state)
    {
        return state switch
        {
            TargetLockState.Locked => Colors.Red,
            TargetLockState.Locking => Colors.Yellow,
            TargetLockState.Unlocked => Colors.Gray,
            _ => Colors.White
        };
    }

    private Color GetRandomLockingColor()
    {
        var colors = new[] { Colors.Red, Colors.Orange, Colors.Yellow, Colors.White };
        return colors[_random.Next(colors.Length)];
    }

    private T FindChildByName<T>(string name) where T : FrameworkElement
    {
        return _mainCanvas.Children.OfType<T>()
            .FirstOrDefault(e => e.Name == name) ??
            _metricsGrid.Children.OfType<T>()
            .FirstOrDefault(e => e.Name == name);
    }

    private void UpdateMetricValue(string label, string value)
    {
        var container = _metricsGrid.Children.OfType<StackPanel>()
            .FirstOrDefault(sp => ((TextBlock)sp.Children[0]).Text == label);
        
        if (container?.Children[1] is TextBlock valueText)
        {
            valueText.Text = value;
        }
    }

    private void UpdateMetricColor(string label, Color color)
    {
        var container = _metricsGrid.Children.OfType<StackPanel>()
            .FirstOrDefault(sp => ((TextBlock)sp.Children[0]).Text == label);
        
        if (container?.Children[1] is TextBlock valueText)
        {
            valueText.Foreground = new SolidColorBrush(color);
            if (valueText.Effect is DropShadowEffect effect)
            {
                effect.Color = color;
            }
        }
    }

    private void CreateShipGeometry(MeshGeometry3D mesh)
    {
        // Simplified ship geometry - triangle with thickness
        // Implementation would create a more detailed ship model
    }

    private void CreateSphereGeometry(MeshGeometry3D mesh, double radius, int detail)
    {
        // Implementation of sphere geometry creation
        // This would create vertices and triangles for a 3D sphere
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Attempts to lock a target
    /// </summary>
    public void LockTarget(string targetId)
    {
        var target = Targets?.FirstOrDefault(t => t.Id == targetId);
        if (target != null && target.LockState == TargetLockState.Unlocked)
        {
            target.LockState = TargetLockState.Locking;
            _targetLockProgress[targetId] = 0.0;

            // Create locking particle effect
            if (EnableParticleEffects)
            {
                for (int i = 0; i < 5; i++)
                {
                    CreateLockingParticle();
                }
            }
        }
    }

    /// <summary>
    /// Unlocks a target
    /// </summary>
    public void UnlockTarget(string targetId)
    {
        var target = Targets?.FirstOrDefault(t => t.Id == targetId);
        if (target != null)
        {
            target.LockState = TargetLockState.Unlocked;
            _targetLockProgress.Remove(targetId);
        }
    }

    /// <summary>
    /// Gets the current targeting analysis
    /// </summary>
    public TargetingAnalysis GetTargetingAnalysis()
    {
        return new TargetingAnalysis
        {
            TotalTargets = Targets?.Count ?? 0,
            LockedTargets = Targets?.Count(t => t.LockState == TargetLockState.Locked) ?? 0,
            LockingTargets = Targets?.Count(t => t.LockState == TargetLockState.Locking) ?? 0,
            MaxTargets = _maxTargets,
            ScanResolution = _scanResolution,
            LockingRange = _lockingRange,
            CurrentRange = _currentRange,
            IsScanning = _isScanning,
            AverageLockTime = _calculator.CalculateAverageLockTime(TargetingData, Targets)
        };
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public void SetAnimationIntensity(double intensity)
    {
        HolographicIntensity = intensity;
        
        // Adjust animation speeds based on intensity
        _animationTimer.Interval = TimeSpan.FromMilliseconds(16 / Math.Max(0.1, intensity));
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void EnableSimplifiedMode(bool enable)
    {
        if (enable)
        {
            EnableParticleEffects = false;
            Show3DVisualization = false;
            ShowLockingBeams = false;
            _animationTimer.Interval = TimeSpan.FromMilliseconds(33); // 30 FPS
        }
        else
        {
            EnableParticleEffects = true;
            Show3DVisualization = true;
            ShowLockingBeams = true;
            _animationTimer.Interval = TimeSpan.FromMilliseconds(16); // 60 FPS
        }
    }

    #endregion

    #region Event Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTargetingSystem control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTargetingSystem control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnTargetingDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTargetingSystem control)
        {
            control.UpdateTargetingData();
        }
    }

    private static void OnTargetsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTargetingSystem control)
        {
            control.UpdateTargets();
        }
    }

    private static void OnVisualizationModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTargetingSystem control)
        {
            control.UpdateVisualizationMode();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTargetingSystem control)
        {
            if ((bool)e.NewValue)
                control.StartAnimations();
            else
                control.StopAnimations();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTargetingSystem control)
        {
            control.UpdateParticleSettings();
        }
    }

    private static void OnShow3DVisualizationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTargetingSystem control)
        {
            control.Update3DVisualization();
        }
    }

    private static void OnShowRangeIndicatorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTargetingSystem control)
        {
            control.UpdateRangeIndicators();
        }
    }

    private static void OnShowLockingBeamsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTargetingSystem control)
        {
            control.UpdateLockingBeamSettings();
        }
    }

    private static void OnUpdateIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTargetingSystem control)
        {
            control._updateTimer.Interval = (TimeSpan)e.NewValue;
        }
    }

    private void UpdateHolographicEffects()
    {
        Opacity = 0.8 + (HolographicIntensity * 0.2);
        
        if (Effect is DropShadowEffect shadowEffect)
        {
            shadowEffect.Opacity = 0.4 + (HolographicIntensity * 0.4);
            shadowEffect.BlurRadius = 15 + (HolographicIntensity * 10);
        }
    }

    private void UpdateColorScheme()
    {
        // Update colors based on EVE color scheme
    }

    private void UpdateTargetingData()
    {
        if (TargetingData != null)
        {
            CalculateTargetingMetrics();
            UpdateMetricsDisplay();
        }
    }

    private void UpdateTargets()
    {
        UpdateTargetList();
        UpdateTargetPositions();
    }

    private void UpdateVisualizationMode()
    {
        // Switch between different visualization modes
    }

    private void UpdateParticleSettings()
    {
        if (!EnableParticleEffects)
        {
            foreach (var particle in _lockingParticles)
            {
                _particleCanvas.Children.Remove(particle.Visual);
            }
            _lockingParticles.Clear();
        }
    }

    private void Update3DVisualization()
    {
        if (Show3DVisualization && _viewport3D == null)
        {
            Create3DVisualization();
        }
        else if (!Show3DVisualization && _viewport3D != null)
        {
            _mainCanvas.Children.Remove(_viewport3D);
            _viewport3D = null;
        }
    }

    private void UpdateRangeIndicators()
    {
        // Update range indicator display
    }

    private void UpdateLockingBeamSettings()
    {
        if (!ShowLockingBeams)
        {
            _beamCanvas.Children.Clear();
        }
    }

    #endregion
}

#region Supporting Classes

/// <summary>
/// Represents targeting system data
/// </summary>
public class HoloTargetingData
{
    public double ScanResolution { get; set; }
    public double MaxTargets { get; set; }
    public double LockingRange { get; set; }
    public double LockSpeed { get; set; }
    public double SensorStrength { get; set; }
}

/// <summary>
/// Represents a target in the targeting system
/// </summary>
public class HoloTarget
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public double Distance { get; set; }
    public double Bearing { get; set; }
    public double SignatureRadius { get; set; }
    public double Velocity { get; set; }
    public TargetLockState LockState { get; set; }
    public DateTime FirstDetected { get; set; }
}

/// <summary>
/// Target lock states
/// </summary>
public enum TargetLockState
{
    Unlocked,
    Locking,
    Locked
}

/// <summary>
/// Targeting visualization modes
/// </summary>
public enum TargetingVisualizationMode
{
    Overview,
    Tactical,
    Range,
    Signature
}

/// <summary>
/// Targeting analysis result
/// </summary>
public class TargetingAnalysis
{
    public int TotalTargets { get; set; }
    public int LockedTargets { get; set; }
    public int LockingTargets { get; set; }
    public double MaxTargets { get; set; }
    public double ScanResolution { get; set; }
    public double LockingRange { get; set; }
    public double CurrentRange { get; set; }
    public bool IsScanning { get; set; }
    public double AverageLockTime { get; set; }
}

/// <summary>
/// Locking particle for visual effects
/// </summary>
public class HoloLockingParticle
{
    public Point Position { get; set; }
    public Vector Velocity { get; set; }
    public Color Color { get; set; }
    public double Size { get; set; }
    public TimeSpan Life { get; set; }
    public DateTime CreatedTime { get; set; } = DateTime.Now;
    public FrameworkElement Visual { get; set; }

    public bool IsExpired => DateTime.Now - CreatedTime > Life;

    public void Update()
    {
        Position = new Point(Position.X + Velocity.X, Position.Y + Velocity.Y);
        
        if (Visual != null)
        {
            Canvas.SetLeft(Visual, Position.X);
            Canvas.SetTop(Visual, Position.Y);
            
            // Fade out over time
            var elapsed = DateTime.Now - CreatedTime;
            var fadeProgress = elapsed.TotalMilliseconds / Life.TotalMilliseconds;
            Visual.Opacity = Math.Max(0, 1.0 - fadeProgress);
        }
    }

    public void CreateVisual()
    {
        Visual = new Ellipse
        {
            Width = Size,
            Height = Size,
            Fill = new RadialGradientBrush
            {
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color, 0.0),
                    new GradientStop(Color.FromArgb(0, Color.R, Color.G, Color.B), 1.0)
                }
            },
            Effect = new DropShadowEffect
            {
                Color = Color,
                BlurRadius = Size * 2,
                ShadowDepth = 0,
                Opacity = 0.8
            }
        };
    }
}

/// <summary>
/// Targeting calculations helper
/// </summary>
public class HoloTargetingCalculator
{
    public double CalculateAverageLockTime(HoloTargetingData targetingData, ObservableCollection<HoloTarget> targets)
    {
        if (targetingData == null || targets == null || !targets.Any())
            return 0;

        var avgSignature = targets.Average(t => t.SignatureRadius);
        var scanRes = targetingData.ScanResolution;
        
        // EVE Online lock time formula approximation
        return Math.Log(avgSignature / scanRes) / Math.Log(2) + 1;
    }

    public double CalculateLockTime(HoloTargetingData targetingData, HoloTarget target)
    {
        if (targetingData == null || target == null)
            return 0;

        var scanRes = targetingData.ScanResolution;
        var signature = target.SignatureRadius;
        
        // EVE Online lock time formula
        return Math.Log(signature / scanRes) / Math.Log(2) + 1;
    }
}

#endregion