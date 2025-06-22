// ==========================================================================
// HoloCapacitorFlow.cs - Holographic Capacitor Flow Animation
// ==========================================================================
// Advanced capacitor visualization featuring animated energy flow, 
// capacitor stability analysis, EVE-style power management, and holographic flow effects.
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
/// Holographic capacitor flow animation with real-time energy analysis and flow visualization
/// </summary>
public class HoloCapacitorFlow : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloCapacitorFlow),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloCapacitorFlow),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty CapacitorDataProperty =
        DependencyProperty.Register(nameof(CapacitorData), typeof(HoloCapacitorData), typeof(HoloCapacitorFlow),
            new PropertyMetadata(null, OnCapacitorDataChanged));

    public static readonly DependencyProperty ModuleConsumptionProperty =
        DependencyProperty.Register(nameof(ModuleConsumption), typeof(ObservableCollection<HoloModuleConsumption>), typeof(HoloCapacitorFlow),
            new PropertyMetadata(null, OnModuleConsumptionChanged));

    public static readonly DependencyProperty FlowVisualizationModeProperty =
        DependencyProperty.Register(nameof(FlowVisualizationMode), typeof(FlowVisualizationMode), typeof(HoloCapacitorFlow),
            new PropertyMetadata(FlowVisualizationMode.RealTime, OnFlowVisualizationModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloCapacitorFlow),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloCapacitorFlow),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty Show3DVisualizationProperty =
        DependencyProperty.Register(nameof(Show3DVisualization), typeof(bool), typeof(HoloCapacitorFlow),
            new PropertyMetadata(true, OnShow3DVisualizationChanged));

    public static readonly DependencyProperty ShowStabilityAnalysisProperty =
        DependencyProperty.Register(nameof(ShowStabilityAnalysis), typeof(bool), typeof(HoloCapacitorFlow),
            new PropertyMetadata(true, OnShowStabilityAnalysisChanged));

    public static readonly DependencyProperty FlowIntensityProperty =
        DependencyProperty.Register(nameof(FlowIntensity), typeof(double), typeof(HoloCapacitorFlow),
            new PropertyMetadata(1.0, OnFlowIntensityChanged));

    public static readonly DependencyProperty UpdateIntervalProperty =
        DependencyProperty.Register(nameof(UpdateInterval), typeof(TimeSpan), typeof(HoloCapacitorFlow),
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

    public HoloCapacitorData CapacitorData
    {
        get => (HoloCapacitorData)GetValue(CapacitorDataProperty);
        set => SetValue(CapacitorDataProperty, value);
    }

    public ObservableCollection<HoloModuleConsumption> ModuleConsumption
    {
        get => (ObservableCollection<HoloModuleConsumption>)GetValue(ModuleConsumptionProperty);
        set => SetValue(ModuleConsumptionProperty, value);
    }

    public FlowVisualizationMode FlowVisualizationMode
    {
        get => (FlowVisualizationMode)GetValue(FlowVisualizationModeProperty);
        set => SetValue(FlowVisualizationModeProperty, value);
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

    public bool ShowStabilityAnalysis
    {
        get => (bool)GetValue(ShowStabilityAnalysisProperty);
        set => SetValue(ShowStabilityAnalysisProperty, value);
    }

    public double FlowIntensity
    {
        get => (double)GetValue(FlowIntensityProperty);
        set => SetValue(FlowIntensityProperty, value);
    }

    public TimeSpan UpdateInterval
    {
        get => (TimeSpan)GetValue(UpdateIntervalProperty);
        set => SetValue(UpdateIntervalProperty, value);
    }

    #endregion

    #region Fields & Properties

    private readonly DispatcherTimer _animationTimer;
    private readonly DispatcherTimer _flowTimer;
    private readonly Dictionary<string, Storyboard> _flowAnimations;
    private readonly List<HoloEnergyParticle> _energyParticles;
    private readonly Dictionary<string, PathGeometry> _flowPaths;
    private readonly HoloCapacitorCalculator _calculator;
    
    private Canvas _mainCanvas;
    private Canvas _particleCanvas;
    private Canvas _flowCanvas;
    private Grid _metricsGrid;
    private Viewport3D _viewport3D;
    private ModelVisual3D _capacitorModel;
    private ModelVisual3D _flowModel;

    private double _currentCapacitor;
    private double _capacitorStability;
    private bool _isStable;
    private DateTime _lastUpdateTime;
    private readonly Random _random = new();

    // Animation state
    private bool _isFlowActive;
    private double _flowAnimationProgress;
    private readonly Dictionary<string, double> _moduleFlowRates;

    #endregion

    #region Constructor & Initialization

    public HoloCapacitorFlow()
    {
        _animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) }; // 60 FPS
        _flowTimer = new DispatcherTimer();
        _flowAnimations = new Dictionary<string, Storyboard>();
        _energyParticles = new List<HoloEnergyParticle>();
        _flowPaths = new Dictionary<string, PathGeometry>();
        _calculator = new HoloCapacitorCalculator();
        _moduleFlowRates = new Dictionary<string, double>();

        _animationTimer.Tick += OnAnimationTimerTick;
        _flowTimer.Tick += OnFlowTimerTick;
        _lastUpdateTime = DateTime.Now;

        InitializeComponent();
        CreateHolographicInterface();
        StartAnimations();
    }

    private void InitializeComponent()
    {
        Width = 400;
        Height = 600;
        Background = new SolidColorBrush(Color.FromArgb(20, 0, 150, 255));

        // Apply holographic effects
        Effect = new DropShadowEffect
        {
            Color = Colors.Cyan,
            BlurRadius = 15,
            ShadowDepth = 0,
            Opacity = 0.5
        };
    }

    private void CreateHolographicInterface()
    {
        var mainGrid = new Grid();
        Content = mainGrid;

        // Define rows for layout
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(200) });

        // Create main canvas for capacitor visualization
        _mainCanvas = new Canvas
        {
            Background = new SolidColorBrush(Color.FromArgb(10, 0, 255, 255)),
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

        // Create flow canvas for energy streams
        _flowCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false
        };
        _mainCanvas.Children.Add(_flowCanvas);

        // Create 3D viewport for capacitor model
        if (Show3DVisualization)
        {
            Create3DVisualization();
        }

        // Create metrics display
        CreateMetricsDisplay();

        // Create capacitor visualization
        CreateCapacitorVisualization();

        // Create module flow connections
        CreateModuleFlowConnections();
    }

    private void Create3DVisualization()
    {
        _viewport3D = new Viewport3D
        {
            Width = 300,
            Height = 300
        };
        Canvas.SetLeft(_viewport3D, 50);
        Canvas.SetTop(_viewport3D, 50);
        _mainCanvas.Children.Add(_viewport3D);

        // Setup camera
        var camera = new PerspectiveCamera
        {
            Position = new Point3D(0, 0, 5),
            LookDirection = new Vector3D(0, 0, -1),
            UpDirection = new Vector3D(0, 1, 0),
            FieldOfView = 45
        };
        _viewport3D.Camera = camera;

        // Create capacitor 3D model
        CreateCapacitor3DModel();

        // Create flow 3D effects
        CreateFlow3DEffects();
    }

    private void CreateCapacitor3DModel()
    {
        _capacitorModel = new ModelVisual3D();
        _viewport3D.Children.Add(_capacitorModel);

        // Create capacitor geometry (cylinder)
        var capacitorGeometry = new MeshGeometry3D();
        CreateCylinderGeometry(capacitorGeometry, 1.0, 2.0, 16);

        // Create holographic material
        var capacitorMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Cyan))
        {
            Brush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(100, 0, 255, 255), 0.0),
                    new GradientStop(Color.FromArgb(200, 0, 150, 255), 0.5),
                    new GradientStop(Color.FromArgb(50, 0, 100, 255), 1.0)
                }
            }
        };

        var capacitorModel = new GeometryModel3D
        {
            Geometry = capacitorGeometry,
            Material = capacitorMaterial
        };

        _capacitorModel.Content = capacitorModel;

        // Add lighting
        var ambientLight = new AmbientLight(Colors.DarkBlue) { Color = Color.FromRgb(20, 20, 40) };
        var directionalLight = new DirectionalLight(Colors.Cyan) { Direction = new Vector3D(-1, -1, -1) };
        
        _viewport3D.Children.Add(new ModelVisual3D { Content = ambientLight });
        _viewport3D.Children.Add(new ModelVisual3D { Content = directionalLight });
    }

    private void CreateFlow3DEffects()
    {
        _flowModel = new ModelVisual3D();
        _viewport3D.Children.Add(_flowModel);

        // Create energy flow particles around capacitor
        var flowGroup = new Model3DGroup();

        for (int i = 0; i < 20; i++)
        {
            var angle = (i / 20.0) * 2 * Math.PI;
            var radius = 1.5;
            var x = Math.Cos(angle) * radius;
            var z = Math.Sin(angle) * radius;

            var particleGeometry = new MeshGeometry3D();
            CreateSphereGeometry(particleGeometry, 0.05, 6);

            var particleMaterial = new EmissiveMaterial(new SolidColorBrush(Colors.Cyan));

            var particleModel = new GeometryModel3D
            {
                Geometry = particleGeometry,
                Material = particleMaterial
            };

            var particleTransform = new TranslateTransform3D(x, 0, z);
            particleModel.Transform = particleTransform;

            flowGroup.Children.Add(particleModel);
        }

        _flowModel.Content = flowGroup;
    }

    private void CreateCapacitorVisualization()
    {
        var capacitorContainer = new Border
        {
            Width = 80,
            Height = 200,
            CornerRadius = new CornerRadius(10),
            BorderBrush = new SolidColorBrush(Colors.Cyan),
            BorderThickness = new Thickness(2),
            Background = new SolidColorBrush(Color.FromArgb(30, 0, 255, 255))
        };
        Canvas.SetLeft(capacitorContainer, 160);
        Canvas.SetTop(capacitorContainer, 100);
        _mainCanvas.Children.Add(capacitorContainer);

        // Create capacitor level indicator
        var capacitorLevel = new Rectangle
        {
            Name = "CapacitorLevel",
            Width = 76,
            Height = 0,
            Fill = new LinearGradientBrush
            {
                StartPoint = new Point(0, 1),
                EndPoint = new Point(0, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Colors.Red, 0.0),
                    new GradientStop(Colors.Orange, 0.3),
                    new GradientStop(Colors.Yellow, 0.6),
                    new GradientStop(Colors.Cyan, 1.0)
                }
            }
        };
        capacitorContainer.Child = capacitorLevel;
        Canvas.SetBottom(capacitorLevel, 0);

        // Add capacitor percentage text
        var capacitorText = new TextBlock
        {
            Name = "CapacitorText",
            Text = "100%",
            Foreground = new SolidColorBrush(Colors.Cyan),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Canvas.SetLeft(capacitorText, 190);
        Canvas.SetTop(capacitorText, 50);
        _mainCanvas.Children.Add(capacitorText);
    }

    private void CreateModuleFlowConnections()
    {
        // Create flow paths for different module types
        var modulePositions = new Dictionary<string, Point>
        {
            { "Weapons", new Point(50, 150) },
            { "Shield", new Point(350, 100) },
            { "Armor", new Point(350, 200) },
            { "Engineering", new Point(50, 250) },
            { "Propulsion", new Point(200, 350) }
        };

        var capacitorCenter = new Point(200, 200);

        foreach (var module in modulePositions)
        {
            CreateFlowPath(module.Key, module.Value, capacitorCenter);
        }
    }

    private void CreateFlowPath(string moduleType, Point start, Point end)
    {
        var path = new Path
        {
            Name = $"FlowPath_{moduleType}",
            Stroke = new SolidColorBrush(GetModuleColor(moduleType)),
            StrokeThickness = 3,
            Opacity = 0.7
        };

        // Create curved path geometry
        var geometry = new PathGeometry();
        var figure = new PathFigure { StartPoint = start };
        
        var controlPoint1 = new Point(start.X + (end.X - start.X) * 0.3, start.Y - 50);
        var controlPoint2 = new Point(start.X + (end.X - start.X) * 0.7, end.Y - 50);
        
        var bezierSegment = new BezierSegment
        {
            Point1 = controlPoint1,
            Point2 = controlPoint2,
            Point3 = end
        };
        
        figure.Segments.Add(bezierSegment);
        geometry.Figures.Add(figure);
        
        path.Data = geometry;
        _flowPaths[moduleType] = geometry;
        
        _flowCanvas.Children.Add(path);

        // Create animated dashed stroke
        path.StrokeDashArray = new DoubleCollection { 5, 5 };
        
        var animation = new DoubleAnimation
        {
            From = 0,
            To = 10,
            Duration = TimeSpan.FromSeconds(1),
            RepeatBehavior = RepeatBehavior.Forever
        };
        
        path.BeginAnimation(Shape.StrokeDashOffsetProperty, animation);
    }

    private void CreateMetricsDisplay()
    {
        _metricsGrid = new Grid
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0))
        };
        Grid.SetRow(_metricsGrid, 1);
        ((Grid)Content).Children.Add(_metricsGrid);

        // Define columns for metrics
        _metricsGrid.ColumnDefinitions.Add(new ColumnDefinition());
        _metricsGrid.ColumnDefinitions.Add(new ColumnDefinition());
        _metricsGrid.ColumnDefinitions.Add(new ColumnDefinition());

        // Create metric displays
        CreateMetricDisplay("Current", "25.4 GJ", Colors.Cyan, 0);
        CreateMetricDisplay("Peak", "28.7 GJ", Colors.Gold, 1);
        CreateMetricDisplay("Stability", "98.2%", Colors.LimeGreen, 2);

        // Add additional metrics row
        _metricsGrid.RowDefinitions.Add(new RowDefinition());
        _metricsGrid.RowDefinitions.Add(new RowDefinition());
        
        CreateMetricDisplay("Recharge", "4.2 GJ/s", Colors.LightBlue, 0, 1);
        CreateMetricDisplay("Usage", "3.8 GJ/s", Colors.Orange, 1, 1);
        CreateMetricDisplay("Delta", "+0.4 GJ/s", Colors.LimeGreen, 2, 1);
    }

    private void CreateMetricDisplay(string label, string value, Color color, int column, int row = 0)
    {
        var container = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10)
        };

        var labelText = new TextBlock
        {
            Text = label,
            Foreground = new SolidColorBrush(Colors.Gray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var valueText = new TextBlock
        {
            Text = value,
            Foreground = new SolidColorBrush(color),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 5,
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
            _flowTimer.Interval = UpdateInterval;
            _flowTimer.Start();
            _isFlowActive = true;
        }
    }

    private void StopAnimations()
    {
        _animationTimer.Stop();
        _flowTimer.Stop();
        _isFlowActive = false;
    }

    private void OnAnimationTimerTick(object sender, EventArgs e)
    {
        if (!EnableAnimations || !_isFlowActive)
            return;

        UpdateParticleEffects();
        UpdateFlowAnimations();
        Update3DEffects();
        UpdateMetricsDisplay();
    }

    private void OnFlowTimerTick(object sender, EventArgs e)
    {
        if (CapacitorData != null)
        {
            CalculateCapacitorMetrics();
            UpdateCapacitorLevel();
            UpdateFlowRates();
        }
    }

    private void UpdateParticleEffects()
    {
        if (!EnableParticleEffects)
            return;

        // Update existing particles
        for (int i = _energyParticles.Count - 1; i >= 0; i--)
        {
            var particle = _energyParticles[i];
            particle.Update();

            if (particle.IsExpired)
            {
                _particleCanvas.Children.Remove(particle.Visual);
                _energyParticles.RemoveAt(i);
            }
        }

        // Create new particles based on capacitor flow
        if (_random.NextDouble() < 0.3 * FlowIntensity)
        {
            CreateEnergyParticle();
        }
    }

    private void UpdateFlowAnimations()
    {
        _flowAnimationProgress += 0.02;
        if (_flowAnimationProgress > 1.0)
            _flowAnimationProgress = 0.0;

        // Update flow path animations based on module consumption
        foreach (var kvp in _moduleFlowRates)
        {
            var moduleType = kvp.Key;
            var flowRate = kvp.Value;
            
            if (_flowPaths.TryGetValue(moduleType, out var path))
            {
                UpdateFlowPathAnimation(moduleType, flowRate);
            }
        }
    }

    private void UpdateFlowPathAnimation(string moduleType, double flowRate)
    {
        var pathElement = _flowCanvas.Children.OfType<Path>()
            .FirstOrDefault(p => p.Name == $"FlowPath_{moduleType}");
        
        if (pathElement != null)
        {
            // Adjust animation speed based on flow rate
            var animationSpeed = Math.Max(0.1, flowRate / 100.0);
            var opacity = Math.Min(1.0, 0.3 + (flowRate / 50.0));
            
            pathElement.Opacity = opacity * HolographicIntensity;
            
            // Update stroke thickness based on flow
            pathElement.StrokeThickness = 2 + (flowRate / 20.0);
        }
    }

    private void Update3DEffects()
    {
        if (!Show3DVisualization || _flowModel == null)
            return;

        // Rotate the flow particles around the capacitor
        var rotationAngle = _flowAnimationProgress * 360;
        var rotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), rotationAngle);
        var rotationTransform = new RotateTransform3D(rotation);
        
        _flowModel.Transform = rotationTransform;

        // Update capacitor model intensity based on charge level
        if (_capacitorModel?.Content is GeometryModel3D model)
        {
            var intensity = _currentCapacitor / (CapacitorData?.MaxCapacitor ?? 100);
            var color = Color.FromArgb(
                (byte)(50 + intensity * 205),
                0,
                (byte)(100 + intensity * 155),
                255
            );
            
            if (model.Material is DiffuseMaterial material)
            {
                material.Brush = new SolidColorBrush(color);
            }
        }
    }

    private void UpdateMetricsDisplay()
    {
        // Update metric values in the display grid
        UpdateMetricValue("Current", $"{_currentCapacitor:F1} GJ");
        UpdateMetricValue("Stability", $"{_capacitorStability:F1}%");
        
        var rechargeRate = CapacitorData?.RechargeRate ?? 0;
        var usageRate = _moduleFlowRates.Values.Sum();
        var deltaRate = rechargeRate - usageRate;
        
        UpdateMetricValue("Recharge", $"{rechargeRate:F1} GJ/s");
        UpdateMetricValue("Usage", $"{usageRate:F1} GJ/s");
        UpdateMetricValue("Delta", $"{deltaRate:+0.0;-0.0} GJ/s");
        
        // Update metric colors based on stability
        var deltaColor = deltaRate >= 0 ? Colors.LimeGreen : Colors.Red;
        UpdateMetricColor("Delta", deltaColor);
        
        var stabilityColor = _isStable ? Colors.LimeGreen : Colors.Red;
        UpdateMetricColor("Stability", stabilityColor);
    }

    private void CreateEnergyParticle()
    {
        var particle = new HoloEnergyParticle
        {
            Position = new Point(_random.NextDouble() * _mainCanvas.ActualWidth, 
                               _random.NextDouble() * _mainCanvas.ActualHeight),
            Velocity = new Vector(_random.NextDouble() * 4 - 2, _random.NextDouble() * 4 - 2),
            Color = GetRandomEnergyColor(),
            Size = _random.NextDouble() * 3 + 1,
            Life = TimeSpan.FromSeconds(_random.NextDouble() * 3 + 1)
        };

        particle.CreateVisual();
        _particleCanvas.Children.Add(particle.Visual);
        _energyParticles.Add(particle);
    }

    #endregion

    #region Calculation Methods

    private void CalculateCapacitorMetrics()
    {
        if (CapacitorData == null)
            return;

        var now = DateTime.Now;
        var deltaTime = (now - _lastUpdateTime).TotalSeconds;
        _lastUpdateTime = now;

        // Calculate current capacitor level
        var rechargeRate = CapacitorData.RechargeRate;
        var totalUsage = ModuleConsumption?.Sum(m => m.CapacitorUsage) ?? 0;
        var netRecharge = rechargeRate - totalUsage;

        _currentCapacitor = Math.Max(0, Math.Min(CapacitorData.MaxCapacitor, 
            _currentCapacitor + netRecharge * deltaTime));

        // Calculate stability
        _capacitorStability = _calculator.CalculateStability(_currentCapacitor, 
            CapacitorData.MaxCapacitor, netRecharge);
        _isStable = _capacitorStability > 25.0; // EVE Online stability threshold

        // Update flow rates for modules
        UpdateModuleFlowRates();
    }

    private void UpdateModuleFlowRates()
    {
        _moduleFlowRates.Clear();
        
        if (ModuleConsumption == null)
            return;

        var moduleGroups = ModuleConsumption.GroupBy(m => m.ModuleType);
        
        foreach (var group in moduleGroups)
        {
            var totalUsage = group.Sum(m => m.CapacitorUsage);
            _moduleFlowRates[group.Key] = totalUsage;
        }
    }

    private void UpdateCapacitorLevel()
    {
        var capacitorLevel = _mainCanvas.Children.OfType<Border>()
            .FirstOrDefault()?.Child as Rectangle;
        
        if (capacitorLevel != null && CapacitorData != null)
        {
            var percentage = _currentCapacitor / CapacitorData.MaxCapacitor;
            var targetHeight = 196 * percentage; // Container height minus border
            
            var heightAnimation = new DoubleAnimation
            {
                To = targetHeight,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuadraticEase()
            };
            
            capacitorLevel.BeginAnimation(FrameworkElement.HeightProperty, heightAnimation);
        }

        // Update percentage text
        var capacitorText = _mainCanvas.Children.OfType<TextBlock>()
            .FirstOrDefault(t => t.Name == "CapacitorText");
        
        if (capacitorText != null && CapacitorData != null)
        {
            var percentage = (_currentCapacitor / CapacitorData.MaxCapacitor) * 100;
            capacitorText.Text = $"{percentage:F0}%";
            
            // Update color based on level
            var color = percentage > 25 ? Colors.Cyan : 
                       percentage > 10 ? Colors.Orange : Colors.Red;
            capacitorText.Foreground = new SolidColorBrush(color);
        }
    }

    #endregion

    #region Helper Methods

    private Color GetModuleColor(string moduleType)
    {
        return moduleType switch
        {
            "Weapons" => Colors.Red,
            "Shield" => Colors.Cyan,
            "Armor" => Colors.Orange,
            "Engineering" => Colors.Yellow,
            "Propulsion" => Colors.LimeGreen,
            _ => Colors.Gray
        };
    }

    private Color GetRandomEnergyColor()
    {
        var colors = new[] { Colors.Cyan, Colors.LightBlue, Colors.White, Colors.Yellow };
        return colors[_random.Next(colors.Length)];
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

    private void CreateCylinderGeometry(MeshGeometry3D mesh, double radius, double height, int sides)
    {
        // Implementation of cylinder geometry creation
        // This would create vertices and triangles for a 3D cylinder
        // Simplified for brevity
    }

    private void CreateSphereGeometry(MeshGeometry3D mesh, double radius, int detail)
    {
        // Implementation of sphere geometry creation
        // This would create vertices and triangles for a 3D sphere
        // Simplified for brevity
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Starts the capacitor flow animation
    /// </summary>
    public void StartFlow()
    {
        _isFlowActive = true;
        StartAnimations();
    }

    /// <summary>
    /// Stops the capacitor flow animation
    /// </summary>
    public void StopFlow()
    {
        _isFlowActive = false;
        StopAnimations();
    }

    /// <summary>
    /// Simulates activating a module with capacitor consumption
    /// </summary>
    public void ActivateModule(string moduleId, double capacitorCost)
    {
        if (CapacitorData != null && _currentCapacitor >= capacitorCost)
        {
            _currentCapacitor -= capacitorCost;
            
            // Create activation particle effect
            if (EnableParticleEffects)
            {
                for (int i = 0; i < 5; i++)
                {
                    CreateEnergyParticle();
                }
            }
        }
    }

    /// <summary>
    /// Gets the current capacitor stability analysis
    /// </summary>
    public CapacitorStabilityAnalysis GetStabilityAnalysis()
    {
        return new CapacitorStabilityAnalysis
        {
            CurrentCapacitor = _currentCapacitor,
            MaxCapacitor = CapacitorData?.MaxCapacitor ?? 0,
            StabilityPercentage = _capacitorStability,
            IsStable = _isStable,
            TimeToEmpty = _calculator.CalculateTimeToEmpty(_currentCapacitor, 
                _moduleFlowRates.Values.Sum()),
            NetRechargeRate = (CapacitorData?.RechargeRate ?? 0) - _moduleFlowRates.Values.Sum()
        };
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public void SetAnimationIntensity(double intensity)
    {
        HolographicIntensity = intensity;
        FlowIntensity = intensity;
        
        // Adjust animation speeds and effects based on intensity
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
            _animationTimer.Interval = TimeSpan.FromMilliseconds(33); // 30 FPS
        }
        else
        {
            EnableParticleEffects = true;
            Show3DVisualization = true;
            _animationTimer.Interval = TimeSpan.FromMilliseconds(16); // 60 FPS
        }
    }

    #endregion

    #region Event Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCapacitorFlow control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCapacitorFlow control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnCapacitorDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCapacitorFlow control)
        {
            control.UpdateCapacitorData();
        }
    }

    private static void OnModuleConsumptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCapacitorFlow control)
        {
            control.UpdateModuleConsumption();
        }
    }

    private static void OnFlowVisualizationModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCapacitorFlow control)
        {
            control.UpdateVisualizationMode();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCapacitorFlow control)
        {
            if ((bool)e.NewValue)
                control.StartAnimations();
            else
                control.StopAnimations();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCapacitorFlow control)
        {
            control.UpdateParticleSettings();
        }
    }

    private static void OnShow3DVisualizationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCapacitorFlow control)
        {
            control.Update3DVisualization();
        }
    }

    private static void OnShowStabilityAnalysisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCapacitorFlow control)
        {
            control.UpdateStabilityDisplay();
        }
    }

    private static void OnFlowIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCapacitorFlow control)
        {
            control.UpdateFlowIntensity();
        }
    }

    private static void OnUpdateIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCapacitorFlow control)
        {
            control._flowTimer.Interval = (TimeSpan)e.NewValue;
        }
    }

    private void UpdateHolographicEffects()
    {
        // Update holographic intensity effects
        Opacity = 0.7 + (HolographicIntensity * 0.3);
        
        if (Effect is DropShadowEffect shadowEffect)
        {
            shadowEffect.Opacity = 0.3 + (HolographicIntensity * 0.5);
            shadowEffect.BlurRadius = 10 + (HolographicIntensity * 10);
        }
    }

    private void UpdateColorScheme()
    {
        // Update colors based on EVE color scheme
        // Implementation would update all visual elements
    }

    private void UpdateCapacitorData()
    {
        if (CapacitorData != null)
        {
            _currentCapacitor = CapacitorData.CurrentCapacitor;
            UpdateCapacitorLevel();
        }
    }

    private void UpdateModuleConsumption()
    {
        UpdateModuleFlowRates();
        CreateModuleFlowConnections();
    }

    private void UpdateVisualizationMode()
    {
        // Update visualization based on selected mode
        // Implementation would switch between different display modes
    }

    private void UpdateParticleSettings()
    {
        if (!EnableParticleEffects)
        {
            // Clear existing particles
            foreach (var particle in _energyParticles)
            {
                _particleCanvas.Children.Remove(particle.Visual);
            }
            _energyParticles.Clear();
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

    private void UpdateStabilityDisplay()
    {
        // Update stability analysis display
        // Implementation would show/hide stability metrics
    }

    private void UpdateFlowIntensity()
    {
        // Update flow animation intensity
        // Implementation would adjust particle generation and flow speeds
    }

    #endregion
}

#region Supporting Classes

/// <summary>
/// Represents capacitor data for visualization
/// </summary>
public class HoloCapacitorData
{
    public double MaxCapacitor { get; set; }
    public double CurrentCapacitor { get; set; }
    public double RechargeRate { get; set; }
    public double RechargeTime { get; set; }
}

/// <summary>
/// Represents module capacitor consumption
/// </summary>
public class HoloModuleConsumption
{
    public string ModuleId { get; set; }
    public string ModuleName { get; set; }
    public string ModuleType { get; set; }
    public double CapacitorUsage { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Flow visualization modes
/// </summary>
public enum FlowVisualizationMode
{
    RealTime,
    Average,
    Peak,
    Simulation
}

/// <summary>
/// Capacitor stability analysis result
/// </summary>
public class CapacitorStabilityAnalysis
{
    public double CurrentCapacitor { get; set; }
    public double MaxCapacitor { get; set; }
    public double StabilityPercentage { get; set; }
    public bool IsStable { get; set; }
    public TimeSpan TimeToEmpty { get; set; }
    public double NetRechargeRate { get; set; }
}

/// <summary>
/// Energy particle for visual effects
/// </summary>
public class HoloEnergyParticle
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
            Fill = new SolidColorBrush(Color),
            Effect = new DropShadowEffect
            {
                Color = Color,
                BlurRadius = Size,
                ShadowDepth = 0,
                Opacity = 0.8
            }
        };
    }
}

/// <summary>
/// Capacitor calculations helper
/// </summary>
public class HoloCapacitorCalculator
{
    public double CalculateStability(double current, double max, double netRecharge)
    {
        if (netRecharge >= 0)
            return 100.0;
        
        // EVE Online capacitor stability formula
        var stableLevel = max * 0.25; // 25% is minimum stable level
        return (current / stableLevel) * 100.0;
    }

    public TimeSpan CalculateTimeToEmpty(double current, double usage)
    {
        if (usage <= 0)
            return TimeSpan.MaxValue;
        
        var timeSeconds = current / usage;
        return TimeSpan.FromSeconds(timeSeconds);
    }
}

#endregion