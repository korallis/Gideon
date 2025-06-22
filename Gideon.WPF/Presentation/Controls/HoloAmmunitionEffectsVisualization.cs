// ==========================================================================
// HoloAmmunitionEffectsVisualization.cs - Holographic Ammunition Effects Visualization
// ==========================================================================
// Advanced ammunition visualization featuring real-time damage calculations,
// ammunition effects analysis, EVE-style ballistics, and holographic projectile tracking.
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
/// Holographic ammunition effects visualization with real-time ballistics analysis and damage tracking
/// </summary>
public class HoloAmmunitionEffectsVisualization : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloAmmunitionEffectsVisualization),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloAmmunitionEffectsVisualization),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty AmmunitionTypesProperty =
        DependencyProperty.Register(nameof(AmmunitionTypes), typeof(ObservableCollection<HoloAmmunitionType>), typeof(HoloAmmunitionEffectsVisualization),
            new PropertyMetadata(null, OnAmmunitionTypesChanged));

    public static readonly DependencyProperty TargetDataProperty =
        DependencyProperty.Register(nameof(TargetData), typeof(HoloTargetData), typeof(HoloAmmunitionEffectsVisualization),
            new PropertyMetadata(null, OnTargetDataChanged));

    public static readonly DependencyProperty VisualizationModeProperty =
        DependencyProperty.Register(nameof(VisualizationMode), typeof(AmmunitionVisualizationMode), typeof(HoloAmmunitionEffectsVisualization),
            new PropertyMetadata(AmmunitionVisualizationMode.Overview, OnVisualizationModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloAmmunitionEffectsVisualization),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloAmmunitionEffectsVisualization),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowProjectileTrailsProperty =
        DependencyProperty.Register(nameof(ShowProjectileTrails), typeof(bool), typeof(HoloAmmunitionEffectsVisualization),
            new PropertyMetadata(true, OnShowProjectileTrailsChanged));

    public static readonly DependencyProperty ShowDamageAnalysisProperty =
        DependencyProperty.Register(nameof(ShowDamageAnalysis), typeof(bool), typeof(HoloAmmunitionEffectsVisualization),
            new PropertyMetadata(true, OnShowDamageAnalysisChanged));

    public static readonly DependencyProperty ShowBallisticsProperty =
        DependencyProperty.Register(nameof(ShowBallistics), typeof(bool), typeof(HoloAmmunitionEffectsVisualization),
            new PropertyMetadata(true, OnShowBallisticsChanged));

    public static readonly DependencyProperty UpdateIntervalProperty =
        DependencyProperty.Register(nameof(UpdateInterval), typeof(TimeSpan), typeof(HoloAmmunitionEffectsVisualization),
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

    public ObservableCollection<HoloAmmunitionType> AmmunitionTypes
    {
        get => (ObservableCollection<HoloAmmunitionType>)GetValue(AmmunitionTypesProperty);
        set => SetValue(AmmunitionTypesProperty, value);
    }

    public HoloTargetData TargetData
    {
        get => (HoloTargetData)GetValue(TargetDataProperty);
        set => SetValue(TargetDataProperty, value);
    }

    public AmmunitionVisualizationMode VisualizationMode
    {
        get => (AmmunitionVisualizationMode)GetValue(VisualizationModeProperty);
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

    public bool ShowProjectileTrails
    {
        get => (bool)GetValue(ShowProjectileTrailsProperty);
        set => SetValue(ShowProjectileTrailsProperty, value);
    }

    public bool ShowDamageAnalysis
    {
        get => (bool)GetValue(ShowDamageAnalysisProperty);
        set => SetValue(ShowDamageAnalysisProperty, value);
    }

    public bool ShowBallistics
    {
        get => (bool)GetValue(ShowBallisticsProperty);
        set => SetValue(ShowBallisticsProperty, value);
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
    private readonly Dictionary<string, Storyboard> _projectileAnimations;
    private readonly List<HoloProjectileParticle> _projectileParticles;
    private readonly List<HoloProjectileTrail> _projectileTrails;
    private readonly HoloAmmunitionCalculator _calculator;
    
    private Canvas _mainCanvas;
    private Canvas _particleCanvas;
    private Canvas _trailCanvas;
    private Canvas _ballisticsCanvas;
    private Grid _ammunitionGrid;
    private Grid _metricsGrid;

    private readonly Dictionary<string, double> _damageEffectiveness;
    private readonly Dictionary<string, ProjectileFlightData> _flightData;
    private double _optimalRange;
    private double _falloffRange;
    private bool _isFiring;
    private DateTime _lastFireTime;
    private readonly Random _random = new();

    // Animation state
    private double _animationProgress;
    private readonly Dictionary<string, Point> _projectilePositions;
    private readonly List<HoloImpactEffect> _impactEffects;

    #endregion

    #region Constructor & Initialization

    public HoloAmmunitionEffectsVisualization()
    {
        _animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) }; // 60 FPS
        _updateTimer = new DispatcherTimer();
        _projectileAnimations = new Dictionary<string, Storyboard>();
        _projectileParticles = new List<HoloProjectileParticle>();
        _projectileTrails = new List<HoloProjectileTrail>();
        _calculator = new HoloAmmunitionCalculator();
        _damageEffectiveness = new Dictionary<string, double>();
        _flightData = new Dictionary<string, ProjectileFlightData>();
        _projectilePositions = new Dictionary<string, Point>();
        _impactEffects = new List<HoloImpactEffect>();

        _animationTimer.Tick += OnAnimationTimerTick;
        _updateTimer.Tick += OnUpdateTimerTick;
        _lastFireTime = DateTime.Now;

        InitializeComponent();
        CreateHolographicInterface();
        StartAnimations();
    }

    private void InitializeComponent()
    {
        Width = 750;
        Height = 850;
        Background = new SolidColorBrush(Color.FromArgb(25, 150, 100, 200));

        // Apply holographic effects
        Effect = new DropShadowEffect
        {
            Color = Colors.Purple,
            BlurRadius = 26,
            ShadowDepth = 0,
            Opacity = 0.7
        };
    }

    private void CreateHolographicInterface()
    {
        var mainGrid = new Grid();
        Content = mainGrid;

        // Define rows for layout
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(250) });

        // Create main canvas for ammunition visualization
        _mainCanvas = new Canvas
        {
            Background = new SolidColorBrush(Color.FromArgb(15, 200, 100, 255)),
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

        // Create trail canvas for projectile trails
        _trailCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false
        };
        _mainCanvas.Children.Add(_trailCanvas);

        // Create ballistics canvas
        _ballisticsCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false
        };
        _mainCanvas.Children.Add(_ballisticsCanvas);

        // Create ammunition list display
        CreateAmmunitionListDisplay();

        // Create ballistics analysis
        if (ShowBallistics)
        {
            CreateBallisticsAnalysis();
        }

        // Create damage analysis
        if (ShowDamageAnalysis)
        {
            CreateDamageAnalysis();
        }

        // Create projectile simulation
        CreateProjectileSimulation();

        // Create metrics display
        CreateMetricsDisplay();
    }

    private void CreateAmmunitionListDisplay()
    {
        var ammoListContainer = new ScrollViewer
        {
            Width = 450,
            Height = 500,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)),
            BorderBrush = new SolidColorBrush(Colors.Purple),
            BorderThickness = new Thickness(1)
        };
        Canvas.SetLeft(ammoListContainer, 20);
        Canvas.SetTop(ammoListContainer, 20);
        _mainCanvas.Children.Add(ammoListContainer);

        _ammunitionGrid = new Grid();
        ammoListContainer.Content = _ammunitionGrid;

        // Add title
        var title = new TextBlock
        {
            Text = "Ammunition Effects Analysis",
            Foreground = new SolidColorBrush(Colors.Purple),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 10)
        };
        _ammunitionGrid.Children.Add(title);
    }

    private void CreateBallisticsAnalysis()
    {
        var ballisticsContainer = new Border
        {
            Width = 250,
            Height = 200,
            Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5)
        };
        Canvas.SetRight(ballisticsContainer, 20);
        Canvas.SetTop(ballisticsContainer, 20);
        _mainCanvas.Children.Add(ballisticsContainer);

        var ballisticsCanvas = new Canvas
        {
            Name = "BallisticsAnalysisCanvas",
            Background = Brushes.Transparent
        };
        ballisticsContainer.Child = ballisticsCanvas;

        // Add title
        var title = new TextBlock
        {
            Text = "Ballistics Analysis",
            Foreground = new SolidColorBrush(Colors.Purple),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Canvas.SetTop(title, 5);
        Canvas.SetLeft(title, 70);
        ballisticsCanvas.Children.Add(title);

        // Create range indicators
        CreateRangeIndicators(ballisticsCanvas);
    }

    private void CreateRangeIndicators(Canvas canvas)
    {
        // Optimal range indicator
        var optimalRange = new Rectangle
        {
            Name = "OptimalRangeIndicator",
            Width = 200,
            Height = 20,
            Fill = new SolidColorBrush(Colors.LimeGreen),
            Opacity = 0.3
        };
        Canvas.SetLeft(optimalRange, 25);
        Canvas.SetTop(optimalRange, 40);
        canvas.Children.Add(optimalRange);

        var optimalLabel = new TextBlock
        {
            Text = "Optimal",
            Foreground = new SolidColorBrush(Colors.LimeGreen),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 10
        };
        Canvas.SetLeft(optimalLabel, 30);
        Canvas.SetTop(optimalLabel, 65);
        canvas.Children.Add(optimalLabel);

        // Falloff range indicator
        var falloffRange = new Rectangle
        {
            Name = "FalloffRangeIndicator",
            Width = 200,
            Height = 20,
            Fill = new SolidColorBrush(Colors.Orange),
            Opacity = 0.3
        };
        Canvas.SetLeft(falloffRange, 25);
        Canvas.SetTop(falloffRange, 85);
        canvas.Children.Add(falloffRange);

        var falloffLabel = new TextBlock
        {
            Text = "Falloff",
            Foreground = new SolidColorBrush(Colors.Orange),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 10
        };
        Canvas.SetLeft(falloffLabel, 30);
        Canvas.SetTop(falloffLabel, 110);
        canvas.Children.Add(falloffLabel);

        // Target range indicator
        var targetRange = new Line
        {
            Name = "TargetRangeIndicator",
            X1 = 25, Y1 = 130, X2 = 225, Y2 = 130,
            Stroke = new SolidColorBrush(Colors.Red),
            StrokeThickness = 2
        };
        canvas.Children.Add(targetRange);

        var targetLabel = new TextBlock
        {
            Text = "Target Range",
            Foreground = new SolidColorBrush(Colors.Red),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 10
        };
        Canvas.SetLeft(targetLabel, 30);
        Canvas.SetTop(targetLabel, 135);
        canvas.Children.Add(targetLabel);
    }

    private void CreateDamageAnalysis()
    {
        var damageContainer = new Border
        {
            Width = 250,
            Height = 180,
            Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5)
        };
        Canvas.SetRight(damageContainer, 20);
        Canvas.SetTop(damageContainer, 240);
        _mainCanvas.Children.Add(damageContainer);

        var damageGrid = new Grid
        {
            Name = "DamageAnalysisGrid"
        };
        damageContainer.Child = damageGrid;

        damageGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
        damageGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        // Add title
        var title = new TextBlock
        {
            Text = "Damage Analysis",
            Foreground = new SolidColorBrush(Colors.Purple),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(title, 0);
        damageGrid.Children.Add(title);

        // Damage type breakdown
        var damagePanel = new StackPanel
        {
            Name = "DamageBreakdownPanel",
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10)
        };
        Grid.SetRow(damagePanel, 1);
        damageGrid.Children.Add(damagePanel);
    }

    private void CreateProjectileSimulation()
    {
        var simulationContainer = new Border
        {
            Width = 250,
            Height = 150,
            Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5)
        };
        Canvas.SetRight(simulationContainer, 20);
        Canvas.SetTop(simulationContainer, 440);
        _mainCanvas.Children.Add(simulationContainer);

        var simulationCanvas = new Canvas
        {
            Name = "ProjectileSimulationCanvas",
            Background = Brushes.Transparent
        };
        simulationContainer.Child = simulationCanvas;

        // Add title
        var title = new TextBlock
        {
            Text = "Projectile Simulation",
            Foreground = new SolidColorBrush(Colors.Purple),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Canvas.SetTop(title, 5);
        Canvas.SetLeft(title, 60);
        simulationCanvas.Children.Add(title);

        // Ship position (left side)
        var ship = new Ellipse
        {
            Name = "ShipIndicator",
            Width = 12,
            Height = 8,
            Fill = new SolidColorBrush(Colors.Cyan)
        };
        Canvas.SetLeft(ship, 20);
        Canvas.SetTop(ship, 70);
        simulationCanvas.Children.Add(ship);

        // Target position (right side)
        var target = new Rectangle
        {
            Name = "TargetIndicator",
            Width = 10,
            Height = 10,
            Fill = new SolidColorBrush(Colors.Red)
        };
        Canvas.SetLeft(target, 210);
        Canvas.SetTop(target, 70);
        simulationCanvas.Children.Add(target);

        // Fire button
        var fireButton = new Button
        {
            Content = "FIRE",
            Width = 60,
            Height = 25,
            Background = new SolidColorBrush(Color.FromArgb(100, 255, 0, 0)),
            Foreground = new SolidColorBrush(Colors.White),
            BorderBrush = new SolidColorBrush(Colors.Red),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 10,
            FontWeight = FontWeights.Bold
        };
        Canvas.SetLeft(fireButton, 95);
        Canvas.SetTop(fireButton, 110);
        fireButton.Click += OnFireButtonClick;
        simulationCanvas.Children.Add(fireButton);
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
        _metricsGrid.ColumnDefinitions.Add(new ColumnDefinition());
        _metricsGrid.ColumnDefinitions.Add(new ColumnDefinition());

        // Create metric displays
        CreateMetricDisplay("Types", "6", Colors.Purple, 0, 0);
        CreateMetricDisplay("DPS", "547", Colors.Red, 1, 0);
        CreateMetricDisplay("Alpha", "2,847", Colors.Orange, 2, 0);
        CreateMetricDisplay("Range", "18 km", Colors.Yellow, 3, 0);
        CreateMetricDisplay("Tracking", "0.65", Colors.Cyan, 4, 0);

        CreateMetricDisplay("EM", "25%", Colors.Purple, 0, 1);
        CreateMetricDisplay("Thermal", "35%", Colors.Red, 1, 1);
        CreateMetricDisplay("Kinetic", "30%", Colors.Blue, 2, 1);
        CreateMetricDisplay("Explosive", "10%", Colors.Orange, 3, 1);
        CreateMetricDisplay("Velocity", "4,500 m/s", Colors.LimeGreen, 4, 1);

        CreateMetricDisplay("Falloff", "12 km", Colors.Orange, 0, 2);
        CreateMetricDisplay("Flight Time", "4.2 s", Colors.Yellow, 1, 2);
        CreateMetricDisplay("Accuracy", "87%", Colors.LimeGreen, 2, 2);
        CreateMetricDisplay("Volley", "8", Colors.Cyan, 3, 2);
        CreateMetricDisplay("ROF", "6.5 s", Colors.Pink, 4, 2);
    }

    private void CreateMetricDisplay(string label, string value, Color color, int column, int row)
    {
        var container = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(4)
        };

        var labelText = new TextBlock
        {
            Text = label,
            Foreground = new SolidColorBrush(Colors.Gray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 8,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var valueText = new TextBlock
        {
            Text = value,
            Foreground = new SolidColorBrush(color),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 11,
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
        UpdateProjectileTrails();
        UpdateProjectileSimulation();
        UpdateImpactEffects();
        UpdateBallisticsAnimations();
    }

    private void OnUpdateTimerTick(object sender, EventArgs e)
    {
        if (AmmunitionTypes != null)
        {
            CalculateAmmunitionEffects();
            UpdateAmmunitionListDisplay();
            UpdateDamageAnalysis();
            UpdateBallisticsAnalysis();
            UpdateMetricsDisplay();
        }
    }

    private void UpdateParticleEffects()
    {
        if (!EnableParticleEffects)
            return;

        // Update existing projectile particles
        for (int i = _projectileParticles.Count - 1; i >= 0; i--)
        {
            var particle = _projectileParticles[i];
            particle.Update();

            if (particle.IsExpired)
            {
                _particleCanvas.Children.Remove(particle.Visual);
                _projectileParticles.RemoveAt(i);
            }
        }

        // Create new particles when firing
        if (_isFiring && _random.NextDouble() < 0.4)
        {
            CreateProjectileParticle();
        }
    }

    private void UpdateProjectileTrails()
    {
        if (!ShowProjectileTrails)
            return;

        // Update existing trails
        for (int i = _projectileTrails.Count - 1; i >= 0; i--)
        {
            var trail = _projectileTrails[i];
            trail.Update();

            if (trail.IsExpired)
            {
                _trailCanvas.Children.Remove(trail.Visual);
                _projectileTrails.RemoveAt(i);
            }
        }
    }

    private void UpdateProjectileSimulation()
    {
        _animationProgress += 0.02;
        if (_animationProgress > 1.0)
            _animationProgress = 0.0;

        // Update projectile positions in simulation
        foreach (var kvp in _projectilePositions.ToList())
        {
            var ammunitionId = kvp.Key;
            var position = kvp.Value;
            
            // Simple ballistic trajectory
            var progress = _animationProgress;
            var x = 20 + (progress * 190); // Move from ship to target
            var y = 70 + Math.Sin(progress * Math.PI) * 20; // Arc trajectory
            
            _projectilePositions[ammunitionId] = new Point(x, y);
            
            // Update visual projectile if exists
            var simulationCanvas = FindChildByName<Canvas>("ProjectileSimulationCanvas");
            var projectile = simulationCanvas?.Children.OfType<Ellipse>()
                .FirstOrDefault(e => e.Name == $"Projectile_{ammunitionId}");
            
            if (projectile != null)
            {
                Canvas.SetLeft(projectile, x - 2);
                Canvas.SetTop(projectile, y - 2);
            }
        }
    }

    private void UpdateImpactEffects()
    {
        // Update existing impact effects
        for (int i = _impactEffects.Count - 1; i >= 0; i--)
        {
            var effect = _impactEffects[i];
            effect.Update();

            if (effect.IsExpired)
            {
                _particleCanvas.Children.Remove(effect.Visual);
                _impactEffects.RemoveAt(i);
            }
        }
    }

    private void UpdateBallisticsAnimations()
    {
        if (!ShowBallistics)
            return;

        // Animate range indicators based on current data
        var ballisticsCanvas = FindChildByName<Canvas>("BallisticsAnalysisCanvas");
        if (ballisticsCanvas != null)
        {
            var pulseIntensity = Math.Sin(_animationProgress * 4 * Math.PI) * 0.2 + 0.8;
            
            var optimalIndicator = ballisticsCanvas.Children.OfType<Rectangle>()
                .FirstOrDefault(r => r.Name == "OptimalRangeIndicator");
            if (optimalIndicator != null)
            {
                optimalIndicator.Opacity = pulseIntensity * 0.3;
            }
        }
    }

    private void CreateProjectileParticle()
    {
        var particle = new HoloProjectileParticle
        {
            Position = new Point(_random.NextDouble() * _mainCanvas.ActualWidth, 
                               _random.NextDouble() * _mainCanvas.ActualHeight),
            Velocity = new Vector(_random.NextDouble() * 6 - 3, _random.NextDouble() * 6 - 3),
            Color = GetRandomProjectileColor(),
            Size = _random.NextDouble() * 3 + 1.5,
            Life = TimeSpan.FromSeconds(_random.NextDouble() * 2 + 1),
            AmmunitionType = GetRandomAmmunitionType()
        };

        particle.CreateVisual();
        _particleCanvas.Children.Add(particle.Visual);
        _projectileParticles.Add(particle);
    }

    private void CreateProjectileTrail(Point start, Point end, Color color)
    {
        var trail = new HoloProjectileTrail
        {
            StartPosition = start,
            EndPosition = end,
            Color = color,
            Width = 2,
            Life = TimeSpan.FromSeconds(0.8)
        };

        trail.CreateVisual();
        _trailCanvas.Children.Add(trail.Visual);
        _projectileTrails.Add(trail);
    }

    private void CreateImpactEffect(Point position, Color color, double intensity)
    {
        var effect = new HoloImpactEffect
        {
            Position = position,
            Color = color,
            Intensity = intensity,
            Size = 8 + intensity * 12,
            Life = TimeSpan.FromSeconds(1.5)
        };

        effect.CreateVisual();
        _particleCanvas.Children.Add(effect.Visual);
        _impactEffects.Add(effect);
    }

    #endregion

    #region Calculation Methods

    private void CalculateAmmunitionEffects()
    {
        if (AmmunitionTypes == null || TargetData == null)
            return;

        _damageEffectiveness.Clear();
        _flightData.Clear();

        foreach (var ammo in AmmunitionTypes)
        {
            var effectiveness = _calculator.CalculateDamageEffectiveness(ammo, TargetData);
            _damageEffectiveness[ammo.Id] = effectiveness;

            var flightData = _calculator.CalculateProjectileFlightData(ammo, TargetData);
            _flightData[ammo.Id] = flightData;
        }

        // Update firing state based on recent activity
        _isFiring = DateTime.Now - _lastFireTime < TimeSpan.FromSeconds(2);
    }

    private void UpdateAmmunitionListDisplay()
    {
        if (AmmunitionTypes == null)
            return;

        // Clear existing content (keep title)
        _ammunitionGrid.Children.Clear();
        _ammunitionGrid.RowDefinitions.Clear();

        // Add title
        _ammunitionGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
        var title = new TextBlock
        {
            Text = "Ammunition Effects Analysis",
            Foreground = new SolidColorBrush(Colors.Purple),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(title, 0);
        _ammunitionGrid.Children.Add(title);

        // Add ammunition types
        int rowIndex = 1;
        foreach (var ammo in AmmunitionTypes.OrderByDescending(a => _damageEffectiveness.GetValueOrDefault(a.Id, 0)))
        {
            _ammunitionGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80) });
            
            var ammoDisplay = CreateAmmunitionDisplay(ammo);
            Grid.SetRow(ammoDisplay, rowIndex);
            _ammunitionGrid.Children.Add(ammoDisplay);
            
            rowIndex++;
        }
    }

    private FrameworkElement CreateAmmunitionDisplay(HoloAmmunitionType ammo)
    {
        var effectiveness = _damageEffectiveness.GetValueOrDefault(ammo.Id, 0.0);
        var flightData = _flightData.GetValueOrDefault(ammo.Id, new ProjectileFlightData());

        var container = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(60, 100, 100, 100)),
            BorderBrush = new SolidColorBrush(GetEffectivenessColor(effectiveness)),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(10, 3),
            Padding = new Thickness(10)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Ammunition info
        var infoPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var nameText = new TextBlock
        {
            Text = ammo.Name,
            Foreground = new SolidColorBrush(Colors.White),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 13,
            FontWeight = FontWeights.Bold
        };
        infoPanel.Children.Add(nameText);

        var typeText = new TextBlock
        {
            Text = $"{ammo.DamageType} | {ammo.ProjectileType}",
            Foreground = new SolidColorBrush(Colors.Gray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 10
        };
        infoPanel.Children.Add(typeText);

        var damageText = new TextBlock
        {
            Text = $"Damage: {ammo.BaseDamage:N0} | Velocity: {ammo.Velocity:N0} m/s",
            Foreground = new SolidColorBrush(Colors.LightGray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 9
        };
        infoPanel.Children.Add(damageText);

        Grid.SetColumn(infoPanel, 0);
        grid.Children.Add(infoPanel);

        // Effectiveness display
        var effectivenessPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var effectivenessText = new TextBlock
        {
            Text = $"{effectiveness * 100:F1}%",
            Foreground = new SolidColorBrush(GetEffectivenessColor(effectiveness)),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        effectivenessPanel.Children.Add(effectivenessText);

        var effectivenessLabel = new TextBlock
        {
            Text = "Effectiveness",
            Foreground = new SolidColorBrush(Colors.Gray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 8,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        effectivenessPanel.Children.Add(effectivenessLabel);

        Grid.SetColumn(effectivenessPanel, 1);
        grid.Children.Add(effectivenessPanel);

        // Flight time display
        var flightPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var flightTimeText = new TextBlock
        {
            Text = $"{flightData.FlightTime:F1}s",
            Foreground = new SolidColorBrush(Colors.Yellow),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        flightPanel.Children.Add(flightTimeText);

        var flightLabel = new TextBlock
        {
            Text = "Flight Time",
            Foreground = new SolidColorBrush(Colors.Gray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 8,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        flightPanel.Children.Add(flightLabel);

        Grid.SetColumn(flightPanel, 2);
        grid.Children.Add(flightPanel);

        // Accuracy display
        var accuracyPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var accuracyText = new TextBlock
        {
            Text = $"{flightData.Accuracy * 100:F0}%",
            Foreground = new SolidColorBrush(GetAccuracyColor(flightData.Accuracy)),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        accuracyPanel.Children.Add(accuracyText);

        var accuracyLabel = new TextBlock
        {
            Text = "Accuracy",
            Foreground = new SolidColorBrush(Colors.Gray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 8,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        accuracyPanel.Children.Add(accuracyLabel);

        Grid.SetColumn(accuracyPanel, 3);
        grid.Children.Add(accuracyPanel);

        container.Child = grid;
        return container;
    }

    private void UpdateDamageAnalysis()
    {
        if (!ShowDamageAnalysis)
            return;

        var damagePanel = FindChildByName<StackPanel>("DamageBreakdownPanel");
        if (damagePanel == null)
            return;

        // Clear existing damage breakdown
        damagePanel.Children.Clear();

        if (AmmunitionTypes != null)
        {
            // Calculate damage type distribution
            var damageDistribution = CalculateDamageDistribution();
            
            foreach (var kvp in damageDistribution)
            {
                var damageDisplay = CreateDamageTypeDisplay(kvp.Key, kvp.Value);
                damagePanel.Children.Add(damageDisplay);
            }
        }
    }

    private Dictionary<string, double> CalculateDamageDistribution()
    {
        var distribution = new Dictionary<string, double>
        {
            { "EM", 0 },
            { "Thermal", 0 },
            { "Kinetic", 0 },
            { "Explosive", 0 }
        };

        if (AmmunitionTypes == null)
            return distribution;

        var totalDamage = AmmunitionTypes.Sum(a => a.BaseDamage);
        
        foreach (var ammo in AmmunitionTypes)
        {
            var percentage = totalDamage > 0 ? ammo.BaseDamage / totalDamage : 0;
            
            if (distribution.ContainsKey(ammo.DamageType))
            {
                distribution[ammo.DamageType] += percentage;
            }
        }

        return distribution;
    }

    private FrameworkElement CreateDamageTypeDisplay(string damageType, double percentage)
    {
        var container = new Grid
        {
            Margin = new Thickness(0, 2)
        };

        container.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
        container.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        container.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });

        // Damage type label
        var typeLabel = new TextBlock
        {
            Text = damageType,
            Foreground = new SolidColorBrush(GetDamageTypeColor(damageType)),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 9,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(typeLabel, 0);
        container.Children.Add(typeLabel);

        // Percentage bar
        var barContainer = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(50, 128, 128, 128)),
            BorderBrush = new SolidColorBrush(GetDamageTypeColor(damageType)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(2),
            Height = 12,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 0)
        };
        Grid.SetColumn(barContainer, 1);
        container.Children.Add(barContainer);

        var percentageFill = new Rectangle
        {
            Fill = new SolidColorBrush(GetDamageTypeColor(damageType)),
            Width = (barContainer.Width - 2) * percentage,
            Height = 10,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        barContainer.Child = percentageFill;

        // Percentage text
        var percentageText = new TextBlock
        {
            Text = $"{percentage * 100:F0}%",
            Foreground = new SolidColorBrush(GetDamageTypeColor(damageType)),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 9,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetColumn(percentageText, 2);
        container.Children.Add(percentageText);

        return container;
    }

    private void UpdateBallisticsAnalysis()
    {
        if (!ShowBallistics)
            return;

        // Update range calculations based on current ammunition
        if (AmmunitionTypes?.Any() == true)
        {
            var optimalAmmo = AmmunitionTypes.OrderByDescending(a => _damageEffectiveness.GetValueOrDefault(a.Id, 0)).First();
            _optimalRange = optimalAmmo.OptimalRange;
            _falloffRange = optimalAmmo.FalloffRange;
        }
    }

    private void UpdateMetricsDisplay()
    {
        if (AmmunitionTypes == null)
            return;

        var typeCount = AmmunitionTypes.Count;
        var totalDPS = AmmunitionTypes.Sum(a => a.BaseDamage / Math.Max(1, a.RateOfFire));
        var totalAlpha = AmmunitionTypes.Sum(a => a.BaseDamage);
        var avgRange = AmmunitionTypes.Average(a => a.OptimalRange);
        var avgTracking = AmmunitionTypes.Average(a => a.TrackingSpeed);

        UpdateMetricValue("Types", typeCount.ToString());
        UpdateMetricValue("DPS", $"{totalDPS:N0}");
        UpdateMetricValue("Alpha", $"{totalAlpha:N0}");
        UpdateMetricValue("Range", $"{avgRange / 1000:F0} km");
        UpdateMetricValue("Tracking", $"{avgTracking:F2}");

        // Update damage type percentages
        var damageDistribution = CalculateDamageDistribution();
        UpdateMetricValue("EM", $"{damageDistribution.GetValueOrDefault("EM", 0) * 100:F0}%");
        UpdateMetricValue("Thermal", $"{damageDistribution.GetValueOrDefault("Thermal", 0) * 100:F0}%");
        UpdateMetricValue("Kinetic", $"{damageDistribution.GetValueOrDefault("Kinetic", 0) * 100:F0}%");
        UpdateMetricValue("Explosive", $"{damageDistribution.GetValueOrDefault("Explosive", 0) * 100:F0}%");

        var avgVelocity = AmmunitionTypes.Average(a => a.Velocity);
        UpdateMetricValue("Velocity", $"{avgVelocity:N0} m/s");

        var avgFalloff = AmmunitionTypes.Average(a => a.FalloffRange);
        var avgFlightTime = _flightData.Values.DefaultIfEmpty(new ProjectileFlightData()).Average(f => f.FlightTime);
        var avgAccuracy = _damageEffectiveness.Values.DefaultIfEmpty(0).Average();
        var totalVolley = AmmunitionTypes.Count;
        var avgROF = AmmunitionTypes.Average(a => a.RateOfFire);

        UpdateMetricValue("Falloff", $"{avgFalloff / 1000:F0} km");
        UpdateMetricValue("Flight Time", $"{avgFlightTime:F1} s");
        UpdateMetricValue("Accuracy", $"{avgAccuracy * 100:F0}%");
        UpdateMetricValue("Volley", totalVolley.ToString());
        UpdateMetricValue("ROF", $"{avgROF:F1} s");

        // Update metric colors based on effectiveness
        UpdateMetricColor("Accuracy", GetEffectivenessColor(avgAccuracy));
        UpdateMetricColor("DPS", totalDPS > 1000 ? Colors.LimeGreen : Colors.Orange);
    }

    #endregion

    #region Event Handlers

    private void OnFireButtonClick(object sender, RoutedEventArgs e)
    {
        _lastFireTime = DateTime.Now;
        _isFiring = true;

        // Create projectile animation
        if (AmmunitionTypes?.Any() == true)
        {
            var ammo = AmmunitionTypes.First();
            CreateProjectileAnimation(ammo);
        }

        // Stop firing after a short duration
        var fireTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        fireTimer.Tick += (s, args) =>
        {
            _isFiring = false;
            fireTimer.Stop();
        };
        fireTimer.Start();
    }

    private void CreateProjectileAnimation(HoloAmmunitionType ammo)
    {
        var simulationCanvas = FindChildByName<Canvas>("ProjectileSimulationCanvas");
        if (simulationCanvas == null)
            return;

        // Create projectile visual
        var projectile = new Ellipse
        {
            Name = $"Projectile_{ammo.Id}",
            Width = 4,
            Height = 4,
            Fill = new SolidColorBrush(GetDamageTypeColor(ammo.DamageType))
        };
        Canvas.SetLeft(projectile, 28);
        Canvas.SetTop(projectile, 72);
        simulationCanvas.Children.Add(projectile);

        // Animate projectile to target
        var animation = new DoubleAnimation
        {
            From = 28,
            To = 210,
            Duration = TimeSpan.FromSeconds(2),
            EasingFunction = new QuadraticEase()
        };

        animation.Completed += (s, args) =>
        {
            // Create impact effect
            CreateImpactEffect(new Point(210, 70), GetDamageTypeColor(ammo.DamageType), 1.0);
            
            // Remove projectile
            simulationCanvas.Children.Remove(projectile);
        };

        projectile.BeginAnimation(Canvas.LeftProperty, animation);

        // Create trail effect
        if (ShowProjectileTrails)
        {
            CreateProjectileTrail(new Point(28, 72), new Point(210, 70), GetDamageTypeColor(ammo.DamageType));
        }
    }

    #endregion

    #region Helper Methods

    private Color GetEffectivenessColor(double effectiveness)
    {
        if (effectiveness >= 0.9) return Colors.LimeGreen;
        if (effectiveness >= 0.7) return Colors.Yellow;
        if (effectiveness >= 0.5) return Colors.Orange;
        return Colors.Red;
    }

    private Color GetAccuracyColor(double accuracy)
    {
        if (accuracy >= 0.9) return Colors.LimeGreen;
        if (accuracy >= 0.7) return Colors.Yellow;
        if (accuracy >= 0.5) return Colors.Orange;
        return Colors.Red;
    }

    private Color GetDamageTypeColor(string damageType)
    {
        return damageType switch
        {
            "EM" => Colors.Purple,
            "Thermal" => Colors.Red,
            "Kinetic" => Colors.Blue,
            "Explosive" => Colors.Orange,
            _ => Colors.White
        };
    }

    private Color GetRandomProjectileColor()
    {
        var colors = new[] { Colors.Red, Colors.Orange, Colors.Yellow, Colors.Purple, Colors.Blue };
        return colors[_random.Next(colors.Length)];
    }

    private string GetRandomAmmunitionType()
    {
        var types = new[] { "Antimatter", "Void", "Null", "Faction", "T2" };
        return types[_random.Next(types.Length)];
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

    #endregion

    #region Public Methods

    /// <summary>
    /// Simulates firing ammunition
    /// </summary>
    public void FireAmmunition(string ammunitionId)
    {
        var ammo = AmmunitionTypes?.FirstOrDefault(a => a.Id == ammunitionId);
        if (ammo != null)
        {
            _lastFireTime = DateTime.Now;
            _isFiring = true;
            CreateProjectileAnimation(ammo);
        }
    }

    /// <summary>
    /// Gets the current ammunition analysis
    /// </summary>
    public AmmunitionAnalysis GetAmmunitionAnalysis()
    {
        return new AmmunitionAnalysis
        {
            TotalTypes = AmmunitionTypes?.Count ?? 0,
            DamageEffectiveness = new Dictionary<string, double>(_damageEffectiveness),
            FlightData = new Dictionary<string, ProjectileFlightData>(_flightData),
            OptimalRange = _optimalRange,
            FalloffRange = _falloffRange,
            IsFiring = _isFiring,
            DamageDistribution = CalculateDamageDistribution()
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
            ShowProjectileTrails = false;
            ShowBallistics = false;
            _animationTimer.Interval = TimeSpan.FromMilliseconds(33); // 30 FPS
        }
        else
        {
            EnableParticleEffects = true;
            ShowProjectileTrails = true;
            ShowBallistics = true;
            _animationTimer.Interval = TimeSpan.FromMilliseconds(16); // 60 FPS
        }
    }

    #endregion

    #region Event Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAmmunitionEffectsVisualization control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAmmunitionEffectsVisualization control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnAmmunitionTypesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAmmunitionEffectsVisualization control)
        {
            control.UpdateAmmunitionTypes();
        }
    }

    private static void OnTargetDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAmmunitionEffectsVisualization control)
        {
            control.UpdateTargetData();
        }
    }

    private static void OnVisualizationModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAmmunitionEffectsVisualization control)
        {
            control.UpdateVisualizationMode();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAmmunitionEffectsVisualization control)
        {
            if ((bool)e.NewValue)
                control.StartAnimations();
            else
                control.StopAnimations();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAmmunitionEffectsVisualization control)
        {
            control.UpdateParticleSettings();
        }
    }

    private static void OnShowProjectileTrailsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAmmunitionEffectsVisualization control)
        {
            control.UpdateProjectileTrailSettings();
        }
    }

    private static void OnShowDamageAnalysisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAmmunitionEffectsVisualization control)
        {
            control.UpdateDamageAnalysisSettings();
        }
    }

    private static void OnShowBallisticsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAmmunitionEffectsVisualization control)
        {
            control.UpdateBallisticsSettings();
        }
    }

    private static void OnUpdateIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAmmunitionEffectsVisualization control)
        {
            control._updateTimer.Interval = (TimeSpan)e.NewValue;
        }
    }

    private void UpdateHolographicEffects()
    {
        Opacity = 0.8 + (HolographicIntensity * 0.2);
        
        if (Effect is DropShadowEffect shadowEffect)
        {
            shadowEffect.Opacity = 0.5 + (HolographicIntensity * 0.4);
            shadowEffect.BlurRadius = 22 + (HolographicIntensity * 8);
        }
    }

    private void UpdateColorScheme()
    {
        // Update colors based on EVE color scheme
    }

    private void UpdateAmmunitionTypes()
    {
        CalculateAmmunitionEffects();
        UpdateAmmunitionListDisplay();
    }

    private void UpdateTargetData()
    {
        CalculateAmmunitionEffects();
        UpdateBallisticsAnalysis();
    }

    private void UpdateVisualizationMode()
    {
        // Switch between different visualization modes
    }

    private void UpdateParticleSettings()
    {
        if (!EnableParticleEffects)
        {
            foreach (var particle in _projectileParticles)
            {
                _particleCanvas.Children.Remove(particle.Visual);
            }
            _projectileParticles.Clear();
        }
    }

    private void UpdateProjectileTrailSettings()
    {
        if (!ShowProjectileTrails)
        {
            foreach (var trail in _projectileTrails)
            {
                _trailCanvas.Children.Remove(trail.Visual);
            }
            _projectileTrails.Clear();
        }
    }

    private void UpdateDamageAnalysisSettings()
    {
        // Update damage analysis display settings
    }

    private void UpdateBallisticsSettings()
    {
        // Update ballistics display settings
    }

    #endregion
}

#region Supporting Classes

/// <summary>
/// Represents an ammunition type
/// </summary>
public class HoloAmmunitionType
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string DamageType { get; set; }
    public string ProjectileType { get; set; }
    public double BaseDamage { get; set; }
    public double Velocity { get; set; }
    public double OptimalRange { get; set; }
    public double FalloffRange { get; set; }
    public double TrackingSpeed { get; set; }
    public double RateOfFire { get; set; }
    public Dictionary<string, double> DamageModifiers { get; set; } = new();
}

/// <summary>
/// Represents target data for ammunition calculations
/// </summary>
public class HoloTargetData
{
    public double Distance { get; set; }
    public double SignatureRadius { get; set; }
    public double Velocity { get; set; }
    public Dictionary<string, double> Resistances { get; set; } = new();
}

/// <summary>
/// Ammunition visualization modes
/// </summary>
public enum AmmunitionVisualizationMode
{
    Overview,
    Damage,
    Ballistics,
    Effects
}

/// <summary>
/// Projectile flight data
/// </summary>
public class ProjectileFlightData
{
    public double FlightTime { get; set; }
    public double Accuracy { get; set; }
    public double TrajectoryArc { get; set; }
    public double EnergyLoss { get; set; }
}

/// <summary>
/// Ammunition analysis result
/// </summary>
public class AmmunitionAnalysis
{
    public int TotalTypes { get; set; }
    public Dictionary<string, double> DamageEffectiveness { get; set; }
    public Dictionary<string, ProjectileFlightData> FlightData { get; set; }
    public double OptimalRange { get; set; }
    public double FalloffRange { get; set; }
    public bool IsFiring { get; set; }
    public Dictionary<string, double> DamageDistribution { get; set; }
}

/// <summary>
/// Projectile particle for visual effects
/// </summary>
public class HoloProjectileParticle
{
    public Point Position { get; set; }
    public Vector Velocity { get; set; }
    public Color Color { get; set; }
    public double Size { get; set; }
    public TimeSpan Life { get; set; }
    public string AmmunitionType { get; set; }
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
                Opacity = 0.9
            }
        };
    }
}

/// <summary>
/// Projectile trail for visual effects
/// </summary>
public class HoloProjectileTrail
{
    public Point StartPosition { get; set; }
    public Point EndPosition { get; set; }
    public Color Color { get; set; }
    public double Width { get; set; }
    public TimeSpan Life { get; set; }
    public DateTime CreatedTime { get; set; } = DateTime.Now;
    public FrameworkElement Visual { get; set; }

    public bool IsExpired => DateTime.Now - CreatedTime > Life;

    public void Update()
    {
        if (Visual != null)
        {
            // Fade out over time
            var elapsed = DateTime.Now - CreatedTime;
            var fadeProgress = elapsed.TotalMilliseconds / Life.TotalMilliseconds;
            Visual.Opacity = Math.Max(0, 1.0 - fadeProgress);
        }
    }

    public void CreateVisual()
    {
        Visual = new Line
        {
            X1 = StartPosition.X,
            Y1 = StartPosition.Y,
            X2 = EndPosition.X,
            Y2 = EndPosition.Y,
            Stroke = new SolidColorBrush(Color),
            StrokeThickness = Width,
            Effect = new DropShadowEffect
            {
                Color = Color,
                BlurRadius = Width * 2,
                ShadowDepth = 0,
                Opacity = 0.7
            }
        };
    }
}

/// <summary>
/// Impact effect for visual feedback
/// </summary>
public class HoloImpactEffect
{
    public Point Position { get; set; }
    public Color Color { get; set; }
    public double Intensity { get; set; }
    public double Size { get; set; }
    public TimeSpan Life { get; set; }
    public DateTime CreatedTime { get; set; } = DateTime.Now;
    public FrameworkElement Visual { get; set; }

    public bool IsExpired => DateTime.Now - CreatedTime > Life;

    public void Update()
    {
        if (Visual != null)
        {
            // Expand and fade over time
            var elapsed = DateTime.Now - CreatedTime;
            var progress = elapsed.TotalMilliseconds / Life.TotalMilliseconds;
            
            var scale = 0.5 + (progress * 1.5);
            var opacity = Math.Max(0, 1.0 - progress);
            
            Visual.RenderTransform = new ScaleTransform(scale, scale);
            Visual.Opacity = opacity;
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
                    new GradientStop(Color.FromArgb(100, Color.R, Color.G, Color.B), 0.5),
                    new GradientStop(Color.FromArgb(0, Color.R, Color.G, Color.B), 1.0)
                }
            },
            Effect = new DropShadowEffect
            {
                Color = Color,
                BlurRadius = Size,
                ShadowDepth = 0,
                Opacity = 0.8
            }
        };
        
        Canvas.SetLeft(Visual, Position.X - Size / 2);
        Canvas.SetTop(Visual, Position.Y - Size / 2);
    }
}

/// <summary>
/// Ammunition calculations helper
/// </summary>
public class HoloAmmunitionCalculator
{
    public double CalculateDamageEffectiveness(HoloAmmunitionType ammo, HoloTargetData target)
    {
        if (ammo == null || target == null)
            return 0;

        // Calculate range effectiveness
        var rangeEffectiveness = CalculateRangeEffectiveness(ammo, target.Distance);
        
        // Calculate tracking effectiveness
        var trackingEffectiveness = CalculateTrackingEffectiveness(ammo, target);
        
        // Calculate resistance effectiveness
        var resistanceEffectiveness = CalculateResistanceEffectiveness(ammo, target);
        
        return rangeEffectiveness * trackingEffectiveness * resistanceEffectiveness;
    }

    private double CalculateRangeEffectiveness(HoloAmmunitionType ammo, double distance)
    {
        if (distance <= ammo.OptimalRange)
            return 1.0;
        
        if (distance > ammo.OptimalRange + ammo.FalloffRange)
            return 0.1;
        
        var falloffDistance = distance - ammo.OptimalRange;
        var falloffPercentage = falloffDistance / ammo.FalloffRange;
        return Math.Max(0.1, 1.0 - (falloffPercentage * 0.9));
    }

    private double CalculateTrackingEffectiveness(HoloAmmunitionType ammo, HoloTargetData target)
    {
        if (target.Velocity == 0)
            return 1.0;
        
        var angularVelocity = target.Velocity / target.Distance;
        var trackingRatio = ammo.TrackingSpeed / angularVelocity;
        
        return Math.Min(1.0, trackingRatio);
    }

    private double CalculateResistanceEffectiveness(HoloAmmunitionType ammo, HoloTargetData target)
    {
        var resistance = target.Resistances.GetValueOrDefault(ammo.DamageType, 0);
        return Math.Max(0.1, 1.0 - resistance);
    }

    public ProjectileFlightData CalculateProjectileFlightData(HoloAmmunitionType ammo, HoloTargetData target)
    {
        var flightTime = target.Distance / ammo.Velocity;
        var accuracy = CalculateDamageEffectiveness(ammo, target);
        var trajectoryArc = CalculateTrajectoryArc(ammo, target.Distance);
        var energyLoss = CalculateEnergyLoss(ammo, target.Distance);

        return new ProjectileFlightData
        {
            FlightTime = flightTime,
            Accuracy = accuracy,
            TrajectoryArc = trajectoryArc,
            EnergyLoss = energyLoss
        };
    }

    private double CalculateTrajectoryArc(HoloAmmunitionType ammo, double distance)
    {
        // Simplified ballistic arc calculation
        return distance / (ammo.Velocity * ammo.Velocity) * 9.81;
    }

    private double CalculateEnergyLoss(HoloAmmunitionType ammo, double distance)
    {
        // Energy loss over distance
        return Math.Min(0.9, distance / (ammo.OptimalRange + ammo.FalloffRange));
    }
}

#endregion