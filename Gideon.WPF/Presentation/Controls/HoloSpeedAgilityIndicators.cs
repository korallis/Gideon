// ==========================================================================
// HoloSpeedAgilityIndicators.cs - Holographic Speed and Agility Indicators
// ==========================================================================
// Advanced speed and agility visualization featuring real-time velocity analysis,
// acceleration curves, EVE-style navigation metrics, and holographic movement effects.
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
/// Holographic speed and agility indicators with real-time navigation analysis and movement visualization
/// </summary>
public class HoloSpeedAgilityIndicators : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloSpeedAgilityIndicators),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloSpeedAgilityIndicators),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty NavigationDataProperty =
        DependencyProperty.Register(nameof(NavigationData), typeof(HoloNavigationData), typeof(HoloSpeedAgilityIndicators),
            new PropertyMetadata(null, OnNavigationDataChanged));

    public static readonly DependencyProperty PropulsionModulesProperty =
        DependencyProperty.Register(nameof(PropulsionModules), typeof(ObservableCollection<HoloPropulsionModule>), typeof(HoloSpeedAgilityIndicators),
            new PropertyMetadata(null, OnPropulsionModulesChanged));

    public static readonly DependencyProperty VisualizationModeProperty =
        DependencyProperty.Register(nameof(VisualizationMode), typeof(SpeedVisualizationMode), typeof(HoloSpeedAgilityIndicators),
            new PropertyMetadata(SpeedVisualizationMode.RealTime, OnVisualizationModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloSpeedAgilityIndicators),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloSpeedAgilityIndicators),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty Show3DVisualizationProperty =
        DependencyProperty.Register(nameof(Show3DVisualization), typeof(bool), typeof(HoloSpeedAgilityIndicators),
            new PropertyMetadata(true, OnShow3DVisualizationChanged));

    public static readonly DependencyProperty ShowAccelerationCurveProperty =
        DependencyProperty.Register(nameof(ShowAccelerationCurve), typeof(bool), typeof(HoloSpeedAgilityIndicators),
            new PropertyMetadata(true, OnShowAccelerationCurveChanged));

    public static readonly DependencyProperty ShowVelocityVectorProperty =
        DependencyProperty.Register(nameof(ShowVelocityVector), typeof(bool), typeof(HoloSpeedAgilityIndicators),
            new PropertyMetadata(true, OnShowVelocityVectorChanged));

    public static readonly DependencyProperty UpdateIntervalProperty =
        DependencyProperty.Register(nameof(UpdateInterval), typeof(TimeSpan), typeof(HoloSpeedAgilityIndicators),
            new PropertyMetadata(TimeSpan.FromMilliseconds(50), OnUpdateIntervalChanged));

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

    public HoloNavigationData NavigationData
    {
        get => (HoloNavigationData)GetValue(NavigationDataProperty);
        set => SetValue(NavigationDataProperty, value);
    }

    public ObservableCollection<HoloPropulsionModule> PropulsionModules
    {
        get => (ObservableCollection<HoloPropulsionModule>)GetValue(PropulsionModulesProperty);
        set => SetValue(PropulsionModulesProperty, value);
    }

    public SpeedVisualizationMode VisualizationMode
    {
        get => (SpeedVisualizationMode)GetValue(VisualizationModeProperty);
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

    public bool ShowAccelerationCurve
    {
        get => (bool)GetValue(ShowAccelerationCurveProperty);
        set => SetValue(ShowAccelerationCurveProperty, value);
    }

    public bool ShowVelocityVector
    {
        get => (bool)GetValue(ShowVelocityVectorProperty);
        set => SetValue(ShowVelocityVectorProperty, value);
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
    private readonly Dictionary<string, Storyboard> _speedAnimations;
    private readonly List<HoloThrustParticle> _thrustParticles;
    private readonly HoloSpeedCalculator _calculator;
    
    private Canvas _mainCanvas;
    private Canvas _particleCanvas;
    private Canvas _vectorCanvas;
    private Grid _metricsGrid;
    private Viewport3D _viewport3D;
    private ModelVisual3D _shipModel;
    private ModelVisual3D _thrustModel;

    private double _currentSpeed;
    private double _maxSpeed;
    private double _acceleration;
    private double _agility;
    private double _alignTime;
    private bool _isThrusting;
    private DateTime _lastUpdateTime;
    private readonly Random _random = new();

    // Animation state
    private double _animationProgress;
    private double _thrustIntensity;
    private readonly List<Point> _velocityHistory;
    private readonly Dictionary<string, double> _moduleEffects;

    #endregion

    #region Constructor & Initialization

    public HoloSpeedAgilityIndicators()
    {
        _animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) }; // 60 FPS
        _updateTimer = new DispatcherTimer();
        _speedAnimations = new Dictionary<string, Storyboard>();
        _thrustParticles = new List<HoloThrustParticle>();
        _calculator = new HoloSpeedCalculator();
        _velocityHistory = new List<Point>();
        _moduleEffects = new Dictionary<string, double>();

        _animationTimer.Tick += OnAnimationTimerTick;
        _updateTimer.Tick += OnUpdateTimerTick;
        _lastUpdateTime = DateTime.Now;

        InitializeComponent();
        CreateHolographicInterface();
        StartAnimations();
    }

    private void InitializeComponent()
    {
        Width = 500;
        Height = 700;
        Background = new SolidColorBrush(Color.FromArgb(25, 0, 200, 100));

        // Apply holographic effects
        Effect = new DropShadowEffect
        {
            Color = Colors.LimeGreen,
            BlurRadius = 18,
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
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(200) });

        // Create main canvas for speed visualization
        _mainCanvas = new Canvas
        {
            Background = new SolidColorBrush(Color.FromArgb(10, 0, 255, 100)),
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

        // Create vector canvas for velocity visualization
        _vectorCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false
        };
        _mainCanvas.Children.Add(_vectorCanvas);

        // Create 3D visualization
        if (Show3DVisualization)
        {
            Create3DVisualization();
        }

        // Create speed gauges
        CreateSpeedGauges();

        // Create acceleration curve
        if (ShowAccelerationCurve)
        {
            CreateAccelerationCurve();
        }

        // Create velocity vector display
        if (ShowVelocityVector)
        {
            CreateVelocityVector();
        }

        // Create metrics display
        CreateMetricsDisplay();
    }

    private void Create3DVisualization()
    {
        _viewport3D = new Viewport3D
        {
            Width = 280,
            Height = 280
        };
        Canvas.SetLeft(_viewport3D, 110);
        Canvas.SetTop(_viewport3D, 40);
        _mainCanvas.Children.Add(_viewport3D);

        // Setup camera
        var camera = new PerspectiveCamera
        {
            Position = new Point3D(0, 2, 6),
            LookDirection = new Vector3D(0, -0.3, -1),
            UpDirection = new Vector3D(0, 1, 0),
            FieldOfView = 50
        };
        _viewport3D.Camera = camera;

        // Create ship model
        CreateShip3DModel();

        // Create thrust effects
        CreateThrust3DEffects();

        // Add lighting
        CreateLighting3D();
    }

    private void CreateShip3DModel()
    {
        _shipModel = new ModelVisual3D();
        _viewport3D.Children.Add(_shipModel);

        // Create ship geometry (simplified ship shape)
        var shipGeometry = new MeshGeometry3D();
        CreateShipGeometry(shipGeometry);

        // Create ship material
        var shipMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Gray))
        {
            Brush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(150, 100, 100, 100), 0.0),
                    new GradientStop(Color.FromArgb(255, 200, 200, 200), 0.5),
                    new GradientStop(Color.FromArgb(100, 50, 50, 50), 1.0)
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

    private void CreateThrust3DEffects()
    {
        _thrustModel = new ModelVisual3D();
        _viewport3D.Children.Add(_thrustModel);

        // Create thrust geometry (cone for engine effects)
        var thrustGeometry = new MeshGeometry3D();
        CreateConeGeometry(thrustGeometry, 0.3, 1.5, 8);

        // Create thrust material with emission
        var thrustMaterial = new EmissiveMaterial(new SolidColorBrush(Colors.LimeGreen));

        var thrustModelGeometry = new GeometryModel3D
        {
            Geometry = thrustGeometry,
            Material = thrustMaterial
        };

        // Position thrust behind ship
        var thrustTransform = new TranslateTransform3D(0, 0, 2);
        thrustModelGeometry.Transform = thrustTransform;

        _thrustModel.Content = thrustModelGeometry;
    }

    private void CreateLighting3D()
    {
        var ambientLight = new AmbientLight(Colors.DarkBlue) { Color = Color.FromRgb(40, 40, 60) };
        var directionalLight = new DirectionalLight(Colors.White) { Direction = new Vector3D(-1, -1, -1) };
        var engineLight = new PointLight(Colors.LimeGreen) 
        { 
            Position = new Point3D(0, 0, 3),
            Range = 8,
            ConstantAttenuation = 0.2,
            LinearAttenuation = 0.1
        };
        
        _viewport3D.Children.Add(new ModelVisual3D { Content = ambientLight });
        _viewport3D.Children.Add(new ModelVisual3D { Content = directionalLight });
        _viewport3D.Children.Add(new ModelVisual3D { Content = engineLight });
    }

    private void CreateSpeedGauges()
    {
        // Speed gauge
        CreateCircularGauge("Speed", Colors.LimeGreen, new Point(100, 350), 80);
        
        // Agility gauge
        CreateCircularGauge("Agility", Colors.Yellow, new Point(250, 350), 80);
        
        // Acceleration gauge
        CreateCircularGauge("Acceleration", Colors.Orange, new Point(400, 350), 80);
    }

    private void CreateCircularGauge(string name, Color color, Point position, double radius)
    {
        var gaugeContainer = new Canvas
        {
            Width = radius * 2,
            Height = radius * 2
        };
        Canvas.SetLeft(gaugeContainer, position.X - radius);
        Canvas.SetTop(gaugeContainer, position.Y - radius);
        _mainCanvas.Children.Add(gaugeContainer);

        // Background circle
        var background = new Ellipse
        {
            Width = radius * 2,
            Height = radius * 2,
            Stroke = new SolidColorBrush(Color.FromArgb(100, color.R, color.G, color.B)),
            StrokeThickness = 3,
            Fill = new SolidColorBrush(Color.FromArgb(20, color.R, color.G, color.B))
        };
        gaugeContainer.Children.Add(background);

        // Progress arc
        var progressPath = new Path
        {
            Name = $"{name}Progress",
            Stroke = new SolidColorBrush(color),
            StrokeThickness = 6,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
            Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 8,
                ShadowDepth = 0,
                Opacity = 0.8
            }
        };
        gaugeContainer.Children.Add(progressPath);

        // Center value display
        var valueText = new TextBlock
        {
            Name = $"{name}Value",
            Text = "0",
            Foreground = new SolidColorBrush(color),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Width = radius * 2,
            Height = radius * 2,
            TextAlignment = TextAlignment.Center
        };
        Canvas.SetTop(valueText, radius - 10);
        gaugeContainer.Children.Add(valueText);

        // Label
        var label = new TextBlock
        {
            Text = name,
            Foreground = new SolidColorBrush(Colors.Gray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            Width = radius * 2,
            TextAlignment = TextAlignment.Center
        };
        Canvas.SetTop(label, radius + 20);
        gaugeContainer.Children.Add(label);
    }

    private void CreateAccelerationCurve()
    {
        var curveContainer = new Border
        {
            Width = 200,
            Height = 120,
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5)
        };
        Canvas.SetRight(curveContainer, 20);
        Canvas.SetTop(curveContainer, 50);
        _mainCanvas.Children.Add(curveContainer);

        var curveCanvas = new Canvas
        {
            Name = "AccelerationCurveCanvas",
            Background = Brushes.Transparent
        };
        curveContainer.Child = curveCanvas;

        // Add curve title
        var title = new TextBlock
        {
            Text = "Acceleration Curve",
            Foreground = new SolidColorBrush(Colors.Gray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 10,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Canvas.SetTop(title, 5);
        Canvas.SetLeft(title, 60);
        curveCanvas.Children.Add(title);
    }

    private void CreateVelocityVector()
    {
        var vectorContainer = new Border
        {
            Width = 180,
            Height = 180,
            Background = new SolidColorBrush(Color.FromArgb(30, 0, 0, 0)),
            BorderBrush = new SolidColorBrush(Colors.Cyan),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5)
        };
        Canvas.SetLeft(vectorContainer, 20);
        Canvas.SetTop(vectorContainer, 50);
        _mainCanvas.Children.Add(vectorContainer);

        var vectorCanvas = new Canvas
        {
            Name = "VelocityVectorCanvas",
            Background = Brushes.Transparent
        };
        vectorContainer.Child = vectorCanvas;

        // Center point
        var center = new Ellipse
        {
            Width = 6,
            Height = 6,
            Fill = new SolidColorBrush(Colors.Cyan)
        };
        Canvas.SetLeft(center, 87);
        Canvas.SetTop(center, 87);
        vectorCanvas.Children.Add(center);

        // Vector arrow (will be updated dynamically)
        var vectorArrow = new Line
        {
            Name = "VectorArrow",
            Stroke = new SolidColorBrush(Colors.LimeGreen),
            StrokeThickness = 3,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Triangle,
            X1 = 90,
            Y1 = 90,
            X2 = 90,
            Y2 = 90
        };
        vectorCanvas.Children.Add(vectorArrow);

        // Add title
        var title = new TextBlock
        {
            Text = "Velocity Vector",
            Foreground = new SolidColorBrush(Colors.Gray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 10,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Canvas.SetTop(title, 5);
        Canvas.SetLeft(title, 55);
        vectorCanvas.Children.Add(title);
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
        CreateMetricDisplay("Max Speed", "2,847 m/s", Colors.LimeGreen, 0, 0);
        CreateMetricDisplay("Current", "0 m/s", Colors.Cyan, 1, 0);
        CreateMetricDisplay("Acceleration", "1.89 m/s²", Colors.Orange, 2, 0);

        CreateMetricDisplay("Agility", "0.67 rad/s", Colors.Yellow, 0, 1);
        CreateMetricDisplay("Align Time", "7.2 s", Colors.Pink, 1, 1);
        CreateMetricDisplay("Mass", "12.4M kg", Colors.Gray, 2, 1);

        CreateMetricDisplay("MWD", "Active", Colors.LimeGreen, 0, 2);
        CreateMetricDisplay("AB", "Offline", Colors.Red, 1, 2);
        CreateMetricDisplay("Modules", "2/8", Colors.Gold, 2, 2);
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
                BlurRadius = 3,
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
        UpdateGaugeAnimations();
        Update3DEffects();
        UpdateVelocityVector();
        UpdateAccelerationCurve();
    }

    private void OnUpdateTimerTick(object sender, EventArgs e)
    {
        if (NavigationData != null)
        {
            CalculateSpeedMetrics();
            UpdateGaugeValues();
            UpdateMetricsDisplay();
        }
    }

    private void UpdateParticleEffects()
    {
        if (!EnableParticleEffects)
            return;

        // Update existing thrust particles
        for (int i = _thrustParticles.Count - 1; i >= 0; i--)
        {
            var particle = _thrustParticles[i];
            particle.Update();

            if (particle.IsExpired)
            {
                _particleCanvas.Children.Remove(particle.Visual);
                _thrustParticles.RemoveAt(i);
            }
        }

        // Create new thrust particles when engines are active
        if (_isThrusting && _random.NextDouble() < 0.6)
        {
            CreateThrustParticle();
        }
    }

    private void UpdateGaugeAnimations()
    {
        _animationProgress += 0.02;
        if (_animationProgress > 1.0)
            _animationProgress = 0.0;

        // Update gauge progress arcs
        UpdateGaugeProgress("Speed", _currentSpeed / _maxSpeed);
        UpdateGaugeProgress("Agility", _agility / 10.0); // Normalized agility
        UpdateGaugeProgress("Acceleration", _acceleration / 5.0); // Normalized acceleration
    }

    private void UpdateGaugeProgress(string gaugeName, double progress)
    {
        var progressPath = FindChildByName<Path>($"{gaugeName}Progress");
        if (progressPath != null)
        {
            var angle = progress * 270; // 270 degrees for 3/4 circle
            var radius = 37; // Adjusted for gauge size
            var startAngle = -135; // Start from bottom-left
            
            var geometry = CreateArcGeometry(radius, startAngle, angle);
            progressPath.Data = geometry;
        }
    }

    private void Update3DEffects()
    {
        if (!Show3DVisualization)
            return;

        // Update thrust model intensity
        if (_thrustModel?.Content is GeometryModel3D thrustGeometry)
        {
            var intensity = _thrustIntensity;
            var scale = 0.5 + (intensity * 1.5);
            var scaleTransform = new ScaleTransform3D(1, 1, scale);
            var translateTransform = new TranslateTransform3D(0, 0, 2);
            
            var group = new Transform3DGroup();
            group.Children.Add(scaleTransform);
            group.Children.Add(translateTransform);
            
            thrustGeometry.Transform = group;

            // Update thrust color based on intensity
            var color = Color.FromArgb(
                (byte)(intensity * 255),
                (byte)(100 + intensity * 155),
                (byte)(255),
                (byte)(100 + intensity * 155)
            );
            
            if (thrustGeometry.Material is EmissiveMaterial material)
            {
                material.Brush = new SolidColorBrush(color);
            }
        }

        // Rotate ship slightly based on velocity
        if (_shipModel != null && NavigationData != null)
        {
            var rotationAngle = Math.Sin(_animationProgress * 2 * Math.PI) * 5; // Slight oscillation
            var rotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), rotationAngle);
            var rotationTransform = new RotateTransform3D(rotation);
            _shipModel.Transform = rotationTransform;
        }
    }

    private void UpdateVelocityVector()
    {
        if (!ShowVelocityVector)
            return;

        var vectorArrow = FindChildByName<Line>("VectorArrow");
        if (vectorArrow != null && NavigationData != null)
        {
            var centerX = 90.0;
            var centerY = 90.0;
            var maxLength = 80.0;
            
            // Calculate vector endpoint based on current velocity
            var velocityMagnitude = Math.Min(1.0, _currentSpeed / _maxSpeed);
            var vectorLength = velocityMagnitude * maxLength;
            
            // For demo, use animation progress as direction
            var angle = _animationProgress * 2 * Math.PI;
            var endX = centerX + Math.Cos(angle) * vectorLength;
            var endY = centerY + Math.Sin(angle) * vectorLength;
            
            vectorArrow.X2 = endX;
            vectorArrow.Y2 = endY;
            
            // Update color based on speed
            var color = velocityMagnitude > 0.8 ? Colors.Red :
                       velocityMagnitude > 0.5 ? Colors.Orange :
                       velocityMagnitude > 0.2 ? Colors.Yellow : Colors.LimeGreen;
            vectorArrow.Stroke = new SolidColorBrush(color);
        }
    }

    private void UpdateAccelerationCurve()
    {
        if (!ShowAccelerationCurve)
            return;

        var curveCanvas = FindChildByName<Canvas>("AccelerationCurveCanvas");
        if (curveCanvas != null)
        {
            // Clear existing curve
            var existingCurve = curveCanvas.Children.OfType<Path>().FirstOrDefault();
            if (existingCurve != null)
                curveCanvas.Children.Remove(existingCurve);

            // Create new acceleration curve based on current data
            var curvePath = new Path
            {
                Stroke = new SolidColorBrush(Colors.Orange),
                StrokeThickness = 2,
                Data = CreateAccelerationCurveGeometry()
            };
            
            curveCanvas.Children.Add(curvePath);
        }
    }

    private void CreateThrustParticle()
    {
        var particle = new HoloThrustParticle
        {
            Position = new Point(250 + _random.NextDouble() * 40 - 20, 
                               350 + _random.NextDouble() * 40 - 20),
            Velocity = new Vector(_random.NextDouble() * 8 - 4, _random.NextDouble() * 8 - 4),
            Color = GetRandomThrustColor(),
            Size = _random.NextDouble() * 3 + 1,
            Life = TimeSpan.FromSeconds(_random.NextDouble() * 1.5 + 0.5)
        };

        particle.CreateVisual();
        _particleCanvas.Children.Add(particle.Visual);
        _thrustParticles.Add(particle);
    }

    #endregion

    #region Calculation Methods

    private void CalculateSpeedMetrics()
    {
        if (NavigationData == null)
            return;

        var now = DateTime.Now;
        var deltaTime = (now - _lastUpdateTime).TotalSeconds;
        _lastUpdateTime = now;

        // Calculate current speed and acceleration
        _currentSpeed = _calculator.CalculateCurrentSpeed(NavigationData, PropulsionModules);
        _maxSpeed = _calculator.CalculateMaxSpeed(NavigationData, PropulsionModules);
        _acceleration = _calculator.CalculateAcceleration(NavigationData, PropulsionModules);
        _agility = _calculator.CalculateAgility(NavigationData);
        _alignTime = _calculator.CalculateAlignTime(NavigationData);

        // Determine if thrusting
        _isThrusting = PropulsionModules?.Any(m => m.IsActive) ?? false;
        _thrustIntensity = _isThrusting ? Math.Min(1.0, _currentSpeed / _maxSpeed) : 0.0;

        // Update velocity history for visualization
        _velocityHistory.Add(new Point(_currentSpeed, _acceleration));
        if (_velocityHistory.Count > 100)
            _velocityHistory.RemoveAt(0);
    }

    private void UpdateGaugeValues()
    {
        UpdateGaugeValue("Speed", $"{_currentSpeed:F0} m/s");
        UpdateGaugeValue("Agility", $"{_agility:F2} rad/s");
        UpdateGaugeValue("Acceleration", $"{_acceleration:F1} m/s²");
    }

    private void UpdateGaugeValue(string gaugeName, string value)
    {
        var valueText = FindChildByName<TextBlock>($"{gaugeName}Value");
        if (valueText != null)
        {
            valueText.Text = value;
        }
    }

    private void UpdateMetricsDisplay()
    {
        if (NavigationData == null)
            return;

        UpdateMetricValue("Max Speed", $"{_maxSpeed:N0} m/s");
        UpdateMetricValue("Current", $"{_currentSpeed:N0} m/s");
        UpdateMetricValue("Acceleration", $"{_acceleration:F2} m/s²");
        UpdateMetricValue("Agility", $"{_agility:F2} rad/s");
        UpdateMetricValue("Align Time", $"{_alignTime:F1} s");
        UpdateMetricValue("Mass", $"{NavigationData.Mass / 1000000:F1}M kg");

        // Update module status
        var mwdActive = PropulsionModules?.Any(m => m.Type == "MWD" && m.IsActive) ?? false;
        var abActive = PropulsionModules?.Any(m => m.Type == "Afterburner" && m.IsActive) ?? false;
        var activeModules = PropulsionModules?.Count(m => m.IsActive) ?? 0;
        var totalModules = PropulsionModules?.Count ?? 0;

        UpdateMetricValue("MWD", mwdActive ? "Active" : "Offline");
        UpdateMetricValue("AB", abActive ? "Active" : "Offline");
        UpdateMetricValue("Modules", $"{activeModules}/{totalModules}");

        // Update metric colors based on status
        UpdateMetricColor("MWD", mwdActive ? Colors.LimeGreen : Colors.Red);
        UpdateMetricColor("AB", abActive ? Colors.LimeGreen : Colors.Red);
    }

    #endregion

    #region Helper Methods

    private Color GetRandomThrustColor()
    {
        var colors = new[] { Colors.LimeGreen, Colors.Cyan, Colors.Yellow, Colors.White };
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

    private PathGeometry CreateArcGeometry(double radius, double startAngle, double sweepAngle)
    {
        var geometry = new PathGeometry();
        var figure = new PathFigure();
        
        var startAngleRad = startAngle * Math.PI / 180;
        var sweepAngleRad = sweepAngle * Math.PI / 180;
        
        var startPoint = new Point(
            radius + radius * Math.Cos(startAngleRad),
            radius + radius * Math.Sin(startAngleRad)
        );
        
        var endPoint = new Point(
            radius + radius * Math.Cos(startAngleRad + sweepAngleRad),
            radius + radius * Math.Sin(startAngleRad + sweepAngleRad)
        );
        
        figure.StartPoint = startPoint;
        
        var arcSegment = new ArcSegment
        {
            Point = endPoint,
            Size = new Size(radius, radius),
            IsLargeArc = sweepAngle > 180,
            SweepDirection = SweepDirection.Clockwise
        };
        
        figure.Segments.Add(arcSegment);
        geometry.Figures.Add(figure);
        
        return geometry;
    }

    private PathGeometry CreateAccelerationCurveGeometry()
    {
        var geometry = new PathGeometry();
        
        if (_velocityHistory.Count < 2)
            return geometry;
        
        var figure = new PathFigure();
        var maxSpeed = _velocityHistory.Max(p => p.X);
        var maxAccel = _velocityHistory.Max(p => p.Y);
        
        if (maxSpeed == 0 || maxAccel == 0)
            return geometry;
        
        // Scale points to fit the canvas
        var scaledPoints = _velocityHistory.Select((p, i) => new Point(
            (i / (double)_velocityHistory.Count) * 190 + 5,
            110 - (p.Y / maxAccel) * 90
        )).ToList();
        
        figure.StartPoint = scaledPoints.First();
        
        for (int i = 1; i < scaledPoints.Count; i++)
        {
            figure.Segments.Add(new LineSegment(scaledPoints[i], true));
        }
        
        geometry.Figures.Add(figure);
        return geometry;
    }

    private void CreateShipGeometry(MeshGeometry3D mesh)
    {
        // Simplified ship geometry - triangle with thickness
        // Implementation would create a more detailed ship model
    }

    private void CreateConeGeometry(MeshGeometry3D mesh, double radius, double height, int sides)
    {
        // Implementation of cone geometry creation
        // This would create vertices and triangles for a 3D cone
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Activates a propulsion module
    /// </summary>
    public void ActivateModule(string moduleId, bool activate = true)
    {
        var module = PropulsionModules?.FirstOrDefault(m => m.Id == moduleId);
        if (module != null)
        {
            module.IsActive = activate;
            
            // Update thrust intensity immediately
            _isThrusting = PropulsionModules.Any(m => m.IsActive);
            
            // Create activation particle effect
            if (EnableParticleEffects && activate)
            {
                for (int i = 0; i < 3; i++)
                {
                    CreateThrustParticle();
                }
            }
        }
    }

    /// <summary>
    /// Gets the current speed and agility analysis
    /// </summary>
    public SpeedAgilityAnalysis GetSpeedAnalysis()
    {
        return new SpeedAgilityAnalysis
        {
            CurrentSpeed = _currentSpeed,
            MaxSpeed = _maxSpeed,
            Acceleration = _acceleration,
            Agility = _agility,
            AlignTime = _alignTime,
            IsThrusting = _isThrusting,
            ThrustIntensity = _thrustIntensity,
            ActiveModules = PropulsionModules?.Count(m => m.IsActive) ?? 0,
            TotalModules = PropulsionModules?.Count ?? 0
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
            ShowAccelerationCurve = false;
            _animationTimer.Interval = TimeSpan.FromMilliseconds(33); // 30 FPS
        }
        else
        {
            EnableParticleEffects = true;
            Show3DVisualization = true;
            ShowAccelerationCurve = true;
            _animationTimer.Interval = TimeSpan.FromMilliseconds(16); // 60 FPS
        }
    }

    #endregion

    #region Event Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSpeedAgilityIndicators control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSpeedAgilityIndicators control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnNavigationDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSpeedAgilityIndicators control)
        {
            control.UpdateNavigationData();
        }
    }

    private static void OnPropulsionModulesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSpeedAgilityIndicators control)
        {
            control.UpdatePropulsionModules();
        }
    }

    private static void OnVisualizationModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSpeedAgilityIndicators control)
        {
            control.UpdateVisualizationMode();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSpeedAgilityIndicators control)
        {
            if ((bool)e.NewValue)
                control.StartAnimations();
            else
                control.StopAnimations();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSpeedAgilityIndicators control)
        {
            control.UpdateParticleSettings();
        }
    }

    private static void OnShow3DVisualizationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSpeedAgilityIndicators control)
        {
            control.Update3DVisualization();
        }
    }

    private static void OnShowAccelerationCurveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSpeedAgilityIndicators control)
        {
            control.UpdateAccelerationCurveDisplay();
        }
    }

    private static void OnShowVelocityVectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSpeedAgilityIndicators control)
        {
            control.UpdateVelocityVectorDisplay();
        }
    }

    private static void OnUpdateIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSpeedAgilityIndicators control)
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
            shadowEffect.BlurRadius = 15 + (HolographicIntensity * 8);
        }
    }

    private void UpdateColorScheme()
    {
        // Update colors based on EVE color scheme
    }

    private void UpdateNavigationData()
    {
        if (NavigationData != null)
        {
            CalculateSpeedMetrics();
            UpdateGaugeValues();
        }
    }

    private void UpdatePropulsionModules()
    {
        CalculateSpeedMetrics();
        UpdateMetricsDisplay();
    }

    private void UpdateVisualizationMode()
    {
        // Switch between different visualization modes
    }

    private void UpdateParticleSettings()
    {
        if (!EnableParticleEffects)
        {
            foreach (var particle in _thrustParticles)
            {
                _particleCanvas.Children.Remove(particle.Visual);
            }
            _thrustParticles.Clear();
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

    private void UpdateAccelerationCurveDisplay()
    {
        // Update acceleration curve display
    }

    private void UpdateVelocityVectorDisplay()
    {
        // Update velocity vector display
    }

    #endregion
}

#region Supporting Classes

/// <summary>
/// Represents navigation data for speed calculations
/// </summary>
public class HoloNavigationData
{
    public double Mass { get; set; }
    public double Inertia { get; set; }
    public double MaxVelocity { get; set; }
    public double CurrentVelocity { get; set; }
    public double AgilityStat { get; set; }
    public Vector3D CurrentDirection { get; set; }
    public Vector3D TargetDirection { get; set; }
}

/// <summary>
/// Represents propulsion module information
/// </summary>
public class HoloPropulsionModule
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; } // MWD, Afterburner, etc.
    public double MaxSpeedBonus { get; set; }
    public double MassModifier { get; set; }
    public bool IsActive { get; set; }
    public double CapacitorUsage { get; set; }
}

/// <summary>
/// Speed visualization modes
/// </summary>
public enum SpeedVisualizationMode
{
    RealTime,
    Projected,
    Historical,
    Comparative
}

/// <summary>
/// Speed and agility analysis result
/// </summary>
public class SpeedAgilityAnalysis
{
    public double CurrentSpeed { get; set; }
    public double MaxSpeed { get; set; }
    public double Acceleration { get; set; }
    public double Agility { get; set; }
    public double AlignTime { get; set; }
    public bool IsThrusting { get; set; }
    public double ThrustIntensity { get; set; }
    public int ActiveModules { get; set; }
    public int TotalModules { get; set; }
}

/// <summary>
/// Thrust particle for visual effects
/// </summary>
public class HoloThrustParticle
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
                BlurRadius = Size * 1.5,
                ShadowDepth = 0,
                Opacity = 0.9
            }
        };
    }
}

/// <summary>
/// Speed calculations helper
/// </summary>
public class HoloSpeedCalculator
{
    public double CalculateCurrentSpeed(HoloNavigationData navData, ObservableCollection<HoloPropulsionModule> modules)
    {
        var baseSpeed = navData?.CurrentVelocity ?? 0;
        var speedBonus = modules?.Where(m => m.IsActive).Sum(m => m.MaxSpeedBonus) ?? 0;
        return baseSpeed * (1 + speedBonus);
    }

    public double CalculateMaxSpeed(HoloNavigationData navData, ObservableCollection<HoloPropulsionModule> modules)
    {
        var baseSpeed = navData?.MaxVelocity ?? 0;
        var speedBonus = modules?.Where(m => m.IsActive).Sum(m => m.MaxSpeedBonus) ?? 0;
        return baseSpeed * (1 + speedBonus);
    }

    public double CalculateAcceleration(HoloNavigationData navData, ObservableCollection<HoloPropulsionModule> modules)
    {
        if (navData == null)
            return 0;

        var effectiveMass = navData.Mass;
        var massModifier = modules?.Where(m => m.IsActive).Sum(m => m.MassModifier) ?? 0;
        effectiveMass *= (1 + massModifier);

        // Simplified acceleration calculation
        return navData.MaxVelocity / effectiveMass * 1000000; // Convert to m/s²
    }

    public double CalculateAgility(HoloNavigationData navData)
    {
        return navData?.AgilityStat ?? 0;
    }

    public double CalculateAlignTime(HoloNavigationData navData)
    {
        if (navData == null)
            return 0;

        // EVE Online align time formula approximation
        var inertiaModifier = navData.Inertia;
        var mass = navData.Mass;
        return Math.Log(2) * inertiaModifier * mass / 1000000;
    }
}

#endregion