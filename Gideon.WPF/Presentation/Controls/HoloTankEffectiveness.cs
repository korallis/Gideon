// ==========================================================================
// HoloTankEffectiveness.cs - Holographic Tank Effectiveness Display
// ==========================================================================
// Advanced tank visualization featuring real-time damage resistance analysis,
// shield/armor/hull visualization, EVE-style defense metrics, and holographic damage flow effects.
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
/// Holographic tank effectiveness display with real-time damage analysis and layer visualization
/// </summary>
public class HoloTankEffectiveness : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloTankEffectiveness),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloTankEffectiveness),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty TankDataProperty =
        DependencyProperty.Register(nameof(TankData), typeof(HoloTankData), typeof(HoloTankEffectiveness),
            new PropertyMetadata(null, OnTankDataChanged));

    public static readonly DependencyProperty DamageTypesProperty =
        DependencyProperty.Register(nameof(DamageTypes), typeof(ObservableCollection<HoloDamageType>), typeof(HoloTankEffectiveness),
            new PropertyMetadata(null, OnDamageTypesChanged));

    public static readonly DependencyProperty DisplayModeProperty =
        DependencyProperty.Register(nameof(DisplayMode), typeof(TankDisplayMode), typeof(HoloTankEffectiveness),
            new PropertyMetadata(TankDisplayMode.Combined, OnDisplayModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloTankEffectiveness),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloTankEffectiveness),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty Show3DVisualizationProperty =
        DependencyProperty.Register(nameof(Show3DVisualization), typeof(bool), typeof(HoloTankEffectiveness),
            new PropertyMetadata(true, OnShow3DVisualizationChanged));

    public static readonly DependencyProperty ShowResistanceBreakdownProperty =
        DependencyProperty.Register(nameof(ShowResistanceBreakdown), typeof(bool), typeof(HoloTankEffectiveness),
            new PropertyMetadata(true, OnShowResistanceBreakdownChanged));

    public static readonly DependencyProperty ShowDamageFlowProperty =
        DependencyProperty.Register(nameof(ShowDamageFlow), typeof(bool), typeof(HoloTankEffectiveness),
            new PropertyMetadata(true, OnShowDamageFlowChanged));

    public static readonly DependencyProperty UpdateIntervalProperty =
        DependencyProperty.Register(nameof(UpdateInterval), typeof(TimeSpan), typeof(HoloTankEffectiveness),
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

    public HoloTankData TankData
    {
        get => (HoloTankData)GetValue(TankDataProperty);
        set => SetValue(TankDataProperty, value);
    }

    public ObservableCollection<HoloDamageType> DamageTypes
    {
        get => (ObservableCollection<HoloDamageType>)GetValue(DamageTypesProperty);
        set => SetValue(DamageTypesProperty, value);
    }

    public TankDisplayMode DisplayMode
    {
        get => (TankDisplayMode)GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
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

    public bool ShowResistanceBreakdown
    {
        get => (bool)GetValue(ShowResistanceBreakdownProperty);
        set => SetValue(ShowResistanceBreakdownProperty, value);
    }

    public bool ShowDamageFlow
    {
        get => (bool)GetValue(ShowDamageFlowProperty);
        set => SetValue(ShowDamageFlowProperty, value);
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
    private readonly Dictionary<string, Storyboard> _layerAnimations;
    private readonly List<HoloDamageParticle> _damageParticles;
    private readonly HoloTankCalculator _calculator;
    
    private Canvas _mainCanvas;
    private Canvas _particleCanvas;
    private Canvas _damageFlowCanvas;
    private Grid _metricsGrid;
    private Viewport3D _viewport3D;
    private ModelVisual3D _shieldModel;
    private ModelVisual3D _armorModel;
    private ModelVisual3D _hullModel;

    private double _shieldLevel;
    private double _armorLevel;
    private double _hullLevel;
    private double _currentEHP;
    private bool _isUnderAttack;
    private DateTime _lastDamageTime;
    private readonly Random _random = new();

    // Animation state
    private double _animationProgress;
    private readonly Dictionary<string, double> _layerAlphas;
    private readonly Dictionary<string, double> _resistanceValues;

    #endregion

    #region Constructor & Initialization

    public HoloTankEffectiveness()
    {
        _animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) }; // 60 FPS
        _updateTimer = new DispatcherTimer();
        _layerAnimations = new Dictionary<string, Storyboard>();
        _damageParticles = new List<HoloDamageParticle>();
        _calculator = new HoloTankCalculator();
        _layerAlphas = new Dictionary<string, double>
        {
            { "Shield", 1.0 },
            { "Armor", 1.0 },
            { "Hull", 1.0 }
        };
        _resistanceValues = new Dictionary<string, double>();

        _animationTimer.Tick += OnAnimationTimerTick;
        _updateTimer.Tick += OnUpdateTimerTick;

        InitializeComponent();
        CreateHolographicInterface();
        StartAnimations();
    }

    private void InitializeComponent()
    {
        Width = 450;
        Height = 650;
        Background = new SolidColorBrush(Color.FromArgb(20, 0, 100, 150));

        // Apply holographic effects
        Effect = new DropShadowEffect
        {
            Color = Colors.Cyan,
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

        // Create main canvas for tank visualization
        _mainCanvas = new Canvas
        {
            Background = new SolidColorBrush(Color.FromArgb(15, 0, 255, 255)),
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

        // Create damage flow canvas
        _damageFlowCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false
        };
        _mainCanvas.Children.Add(_damageFlowCanvas);

        // Create 3D visualization
        if (Show3DVisualization)
        {
            Create3DVisualization();
        }

        // Create tank layer visualization
        CreateTankLayerVisualization();

        // Create resistance breakdown
        if (ShowResistanceBreakdown)
        {
            CreateResistanceBreakdown();
        }

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
        Canvas.SetLeft(_viewport3D, 75);
        Canvas.SetTop(_viewport3D, 30);
        _mainCanvas.Children.Add(_viewport3D);

        // Setup camera
        var camera = new PerspectiveCamera
        {
            Position = new Point3D(0, 0, 8),
            LookDirection = new Vector3D(0, 0, -1),
            UpDirection = new Vector3D(0, 1, 0),
            FieldOfView = 45
        };
        _viewport3D.Camera = camera;

        // Create tank layers in 3D
        CreateTankLayers3D();

        // Add lighting
        CreateLighting3D();
    }

    private void CreateTankLayers3D()
    {
        // Hull layer (innermost)
        _hullModel = CreateLayer3D("Hull", 1.0, Colors.Gray, 0.8);
        _viewport3D.Children.Add(_hullModel);

        // Armor layer (middle)
        _armorModel = CreateLayer3D("Armor", 1.3, Colors.Orange, 0.6);
        _viewport3D.Children.Add(_armorModel);

        // Shield layer (outermost)
        _shieldModel = CreateLayer3D("Shield", 1.6, Colors.Cyan, 0.4);
        _viewport3D.Children.Add(_shieldModel);
    }

    private ModelVisual3D CreateLayer3D(string layerName, double radius, Color color, double opacity)
    {
        var model = new ModelVisual3D();

        var geometry = new MeshGeometry3D();
        CreateSphereGeometry(geometry, radius, 20);

        var material = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(
            (byte)(opacity * 255), color.R, color.G, color.B)));

        var geometryModel = new GeometryModel3D
        {
            Geometry = geometry,
            Material = material,
            BackMaterial = material
        };

        model.Content = geometryModel;
        return model;
    }

    private void CreateLighting3D()
    {
        var ambientLight = new AmbientLight(Colors.DarkBlue) { Color = Color.FromRgb(30, 30, 50) };
        var directionalLight = new DirectionalLight(Colors.White) { Direction = new Vector3D(-1, -1, -1) };
        var pointLight = new PointLight(Colors.Cyan) 
        { 
            Position = new Point3D(2, 2, 2),
            Range = 10,
            ConstantAttenuation = 0.1,
            LinearAttenuation = 0.1
        };
        
        _viewport3D.Children.Add(new ModelVisual3D { Content = ambientLight });
        _viewport3D.Children.Add(new ModelVisual3D { Content = directionalLight });
        _viewport3D.Children.Add(new ModelVisual3D { Content = pointLight });
    }

    private void CreateTankLayerVisualization()
    {
        var layerContainer = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(20)
        };
        Canvas.SetRight(layerContainer, 20);
        Canvas.SetTop(layerContainer, 50);
        _mainCanvas.Children.Add(layerContainer);

        // Create layer displays
        CreateLayerDisplay(layerContainer, "Shield", Colors.Cyan, _shieldLevel);
        CreateLayerDisplay(layerContainer, "Armor", Colors.Orange, _armorLevel);
        CreateLayerDisplay(layerContainer, "Hull", Colors.Gray, _hullLevel);
    }

    private void CreateLayerDisplay(Panel container, string layerName, Color color, double level)
    {
        var layerPanel = new Grid
        {
            Width = 120,
            Height = 40,
            Margin = new Thickness(0, 5)
        };

        // Background
        var background = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(30, color.R, color.G, color.B)),
            BorderBrush = new SolidColorBrush(color),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5)
        };
        layerPanel.Children.Add(background);

        // Level bar
        var levelBar = new Rectangle
        {
            Name = $"{layerName}LevelBar",
            Fill = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(100, color.R, color.G, color.B), 0.0),
                    new GradientStop(color, 1.0)
                }
            },
            Width = 116,
            Height = 36,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        layerPanel.Children.Add(levelBar);

        // Label
        var label = new TextBlock
        {
            Text = layerName,
            Foreground = new SolidColorBrush(color),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 3,
                ShadowDepth = 0,
                Opacity = 0.8
            }
        };
        layerPanel.Children.Add(label);

        container.Children.Add(layerPanel);
    }

    private void CreateResistanceBreakdown()
    {
        var resistancePanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(20)
        };
        Canvas.SetLeft(resistancePanel, 20);
        Canvas.SetTop(resistancePanel, 50);
        _mainCanvas.Children.Add(resistancePanel);

        // Create resistance displays for each damage type
        var damageTypes = new[] { "EM", "Thermal", "Kinetic", "Explosive" };
        var colors = new[] { Colors.Purple, Colors.Red, Colors.Blue, Colors.Orange };

        for (int i = 0; i < damageTypes.Length; i++)
        {
            CreateResistanceDisplay(resistancePanel, damageTypes[i], colors[i]);
        }
    }

    private void CreateResistanceDisplay(Panel container, string damageType, Color color)
    {
        var resistanceGrid = new Grid
        {
            Width = 150,
            Height = 30,
            Margin = new Thickness(0, 3)
        };

        resistanceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
        resistanceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        resistanceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });

        // Damage type label
        var typeLabel = new TextBlock
        {
            Text = damageType,
            Foreground = new SolidColorBrush(color),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(typeLabel, 0);
        resistanceGrid.Children.Add(typeLabel);

        // Resistance bar
        var resistanceBar = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(30, color.R, color.G, color.B)),
            BorderBrush = new SolidColorBrush(color),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3),
            Margin = new Thickness(5, 0)
        };
        Grid.SetColumn(resistanceBar, 1);
        resistanceGrid.Children.Add(resistanceBar);

        var resistanceLevel = new Rectangle
        {
            Name = $"{damageType}ResistanceLevel",
            Fill = new SolidColorBrush(color),
            Height = 26,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        resistanceBar.Child = resistanceLevel;

        // Resistance percentage
        var resistanceText = new TextBlock
        {
            Name = $"{damageType}ResistanceText",
            Text = "0%",
            Foreground = new SolidColorBrush(color),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 10,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetColumn(resistanceText, 2);
        resistanceGrid.Children.Add(resistanceText);

        container.Children.Add(resistanceGrid);
    }

    private void CreateMetricsDisplay()
    {
        _metricsGrid = new Grid
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0))
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
        CreateMetricDisplay("EHP", "45,678 HP", Colors.Cyan, 0, 0);
        CreateMetricDisplay("Raw HP", "38,234 HP", Colors.Gray, 1, 0);
        CreateMetricDisplay("Multiplier", "1.19x", Colors.Gold, 2, 0);

        CreateMetricDisplay("Shield Rep", "287 HP/s", Colors.Cyan, 0, 1);
        CreateMetricDisplay("Armor Rep", "145 HP/s", Colors.Orange, 1, 1);
        CreateMetricDisplay("Total Rep", "432 HP/s", Colors.LimeGreen, 2, 1);

        CreateMetricDisplay("Shield Res", "67%", Colors.Cyan, 0, 2);
        CreateMetricDisplay("Armor Res", "73%", Colors.Orange, 1, 2);
        CreateMetricDisplay("Avg Res", "70%", Colors.Gold, 2, 2);
    }

    private void CreateMetricDisplay(string label, string value, Color color, int column, int row)
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
            FontSize = 11,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var valueText = new TextBlock
        {
            Text = value,
            Foreground = new SolidColorBrush(color),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 14,
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
        UpdateLayerAnimations();
        Update3DEffects();
        UpdateDamageFlow();
    }

    private void OnUpdateTimerTick(object sender, EventArgs e)
    {
        if (TankData != null)
        {
            CalculateTankMetrics();
            UpdateLayerLevels();
            UpdateResistanceDisplays();
            UpdateMetricsDisplay();
        }
    }

    private void UpdateParticleEffects()
    {
        if (!EnableParticleEffects)
            return;

        // Update existing particles
        for (int i = _damageParticles.Count - 1; i >= 0; i--)
        {
            var particle = _damageParticles[i];
            particle.Update();

            if (particle.IsExpired)
            {
                _particleCanvas.Children.Remove(particle.Visual);
                _damageParticles.RemoveAt(i);
            }
        }

        // Create damage particles when under attack
        if (_isUnderAttack && _random.NextDouble() < 0.4)
        {
            CreateDamageParticle();
        }
    }

    private void UpdateLayerAnimations()
    {
        _animationProgress += 0.01;
        if (_animationProgress > 1.0)
            _animationProgress = 0.0;

        // Update layer alpha oscillations for recharge effects
        foreach (var layer in _layerAlphas.Keys.ToList())
        {
            var baseAlpha = GetLayerLevel(layer) / 100.0;
            var oscillation = Math.Sin(_animationProgress * 2 * Math.PI) * 0.2;
            _layerAlphas[layer] = Math.Max(0.2, baseAlpha + oscillation);
        }
    }

    private void Update3DEffects()
    {
        if (!Show3DVisualization)
            return;

        // Update 3D model opacity based on layer levels
        UpdateLayerModel(_shieldModel, _shieldLevel / 100.0, Colors.Cyan);
        UpdateLayerModel(_armorModel, _armorLevel / 100.0, Colors.Orange);
        UpdateLayerModel(_hullModel, _hullLevel / 100.0, Colors.Gray);

        // Add pulse effect for damaged layers
        if (_isUnderAttack)
        {
            var pulseIntensity = Math.Sin(_animationProgress * 8 * Math.PI) * 0.3 + 0.7;
            ApplyPulseEffect(_shieldModel, pulseIntensity);
        }
    }

    private void UpdateLayerModel(ModelVisual3D model, double level, Color baseColor)
    {
        if (model?.Content is GeometryModel3D geometry)
        {
            var alpha = (byte)(level * 200 + 55);
            var color = Color.FromArgb(alpha, baseColor.R, baseColor.G, baseColor.B);
            
            if (geometry.Material is DiffuseMaterial material)
            {
                material.Brush = new SolidColorBrush(color);
            }
        }
    }

    private void ApplyPulseEffect(ModelVisual3D model, double intensity)
    {
        if (model != null)
        {
            var scaleTransform = new ScaleTransform3D(intensity, intensity, intensity);
            model.Transform = scaleTransform;
        }
    }

    private void UpdateDamageFlow()
    {
        if (!ShowDamageFlow)
            return;

        // Create damage flow effects from external sources to tank layers
        // Implementation would show damage streams and absorption effects
    }

    private void CreateDamageParticle()
    {
        var particle = new HoloDamageParticle
        {
            Position = new Point(_random.NextDouble() * _mainCanvas.ActualWidth, 
                               _random.NextDouble() * _mainCanvas.ActualHeight),
            Velocity = new Vector(_random.NextDouble() * 6 - 3, _random.NextDouble() * 6 - 3),
            Color = GetRandomDamageColor(),
            Size = _random.NextDouble() * 4 + 2,
            Life = TimeSpan.FromSeconds(_random.NextDouble() * 2 + 0.5),
            DamageType = GetRandomDamageType()
        };

        particle.CreateVisual();
        _particleCanvas.Children.Add(particle.Visual);
        _damageParticles.Add(particle);
    }

    #endregion

    #region Calculation Methods

    private void CalculateTankMetrics()
    {
        if (TankData == null)
            return;

        // Calculate effective HP
        _currentEHP = _calculator.CalculateEffectiveHP(TankData);

        // Update layer levels based on damage and repair
        UpdateLayerHealth();

        // Check if under attack (recent damage)
        _isUnderAttack = DateTime.Now - _lastDamageTime < TimeSpan.FromSeconds(5);
    }

    private void UpdateLayerHealth()
    {
        if (TankData == null)
            return;

        _shieldLevel = (TankData.CurrentShield / TankData.MaxShield) * 100;
        _armorLevel = (TankData.CurrentArmor / TankData.MaxArmor) * 100;
        _hullLevel = (TankData.CurrentHull / TankData.MaxHull) * 100;
    }

    private void UpdateLayerLevels()
    {
        UpdateLayerBar("Shield", _shieldLevel, Colors.Cyan);
        UpdateLayerBar("Armor", _armorLevel, Colors.Orange);
        UpdateLayerBar("Hull", _hullLevel, Colors.Gray);
    }

    private void UpdateLayerBar(string layerName, double level, Color color)
    {
        var levelBar = FindChildByName<Rectangle>($"{layerName}LevelBar");
        if (levelBar != null)
        {
            var targetWidth = (level / 100.0) * 116;
            
            var animation = new DoubleAnimation
            {
                To = targetWidth,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase()
            };
            
            levelBar.BeginAnimation(FrameworkElement.WidthProperty, animation);

            // Update color intensity based on level
            var intensity = Math.Max(0.3, level / 100.0);
            var adjustedColor = Color.FromArgb(
                (byte)(intensity * 255),
                color.R, color.G, color.B
            );
            
            levelBar.Fill = new SolidColorBrush(adjustedColor);
        }
    }

    private void UpdateResistanceDisplays()
    {
        if (TankData?.Resistances == null)
            return;

        foreach (var resistance in TankData.Resistances)
        {
            UpdateResistanceBar(resistance.DamageType, resistance.Value);
        }
    }

    private void UpdateResistanceBar(string damageType, double resistance)
    {
        var resistanceLevel = FindChildByName<Rectangle>($"{damageType}ResistanceLevel");
        var resistanceText = FindChildByName<TextBlock>($"{damageType}ResistanceText");
        
        if (resistanceLevel != null && resistanceText != null)
        {
            var percentage = resistance * 100;
            var targetWidth = (resistance * 140); // Adjusted for bar container width
            
            var animation = new DoubleAnimation
            {
                To = targetWidth,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuadraticEase()
            };
            
            resistanceLevel.BeginAnimation(FrameworkElement.WidthProperty, animation);
            resistanceText.Text = $"{percentage:F0}%";
            
            // Update color based on resistance value
            var color = GetResistanceColor(resistance);
            resistanceLevel.Fill = new SolidColorBrush(color);
            resistanceText.Foreground = new SolidColorBrush(color);
        }
    }

    private void UpdateMetricsDisplay()
    {
        if (TankData == null)
            return;

        var ehp = _calculator.CalculateEffectiveHP(TankData);
        var rawHp = TankData.CurrentShield + TankData.CurrentArmor + TankData.CurrentHull;
        var multiplier = ehp / rawHp;

        UpdateMetricValue("EHP", $"{ehp:N0} HP");
        UpdateMetricValue("Raw HP", $"{rawHp:N0} HP");
        UpdateMetricValue("Multiplier", $"{multiplier:F2}x");

        var shieldRep = TankData.ShieldRepairRate;
        var armorRep = TankData.ArmorRepairRate;
        var totalRep = shieldRep + armorRep;

        UpdateMetricValue("Shield Rep", $"{shieldRep:F0} HP/s");
        UpdateMetricValue("Armor Rep", $"{armorRep:F0} HP/s");
        UpdateMetricValue("Total Rep", $"{totalRep:F0} HP/s");

        var avgShieldRes = TankData.Resistances?.Where(r => r.Layer == "Shield").Average(r => r.Value) ?? 0;
        var avgArmorRes = TankData.Resistances?.Where(r => r.Layer == "Armor").Average(r => r.Value) ?? 0;
        var avgRes = (avgShieldRes + avgArmorRes) / 2;

        UpdateMetricValue("Shield Res", $"{avgShieldRes * 100:F0}%");
        UpdateMetricValue("Armor Res", $"{avgArmorRes * 100:F0}%");
        UpdateMetricValue("Avg Res", $"{avgRes * 100:F0}%");
    }

    #endregion

    #region Helper Methods

    private double GetLayerLevel(string layer)
    {
        return layer switch
        {
            "Shield" => _shieldLevel,
            "Armor" => _armorLevel,
            "Hull" => _hullLevel,
            _ => 0
        };
    }

    private Color GetRandomDamageColor()
    {
        var colors = new[] { Colors.Purple, Colors.Red, Colors.Blue, Colors.Orange };
        return colors[_random.Next(colors.Length)];
    }

    private string GetRandomDamageType()
    {
        var types = new[] { "EM", "Thermal", "Kinetic", "Explosive" };
        return types[_random.Next(types.Length)];
    }

    private Color GetResistanceColor(double resistance)
    {
        // Color based on resistance percentage
        if (resistance >= 0.8) return Colors.LimeGreen;
        if (resistance >= 0.6) return Colors.Yellow;
        if (resistance >= 0.4) return Colors.Orange;
        return Colors.Red;
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

    private void CreateSphereGeometry(MeshGeometry3D mesh, double radius, int detail)
    {
        // Implementation of sphere geometry creation
        // This would create vertices and triangles for a 3D sphere
        // Simplified for brevity
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Simulates taking damage to the tank
    /// </summary>
    public void TakeDamage(double amount, string damageType, string targetLayer = "Shield")
    {
        _lastDamageTime = DateTime.Now;
        _isUnderAttack = true;

        // Apply damage to appropriate layer
        if (TankData != null)
        {
            switch (targetLayer)
            {
                case "Shield":
                    TankData.CurrentShield = Math.Max(0, TankData.CurrentShield - amount);
                    break;
                case "Armor":
                    TankData.CurrentArmor = Math.Max(0, TankData.CurrentArmor - amount);
                    break;
                case "Hull":
                    TankData.CurrentHull = Math.Max(0, TankData.CurrentHull - amount);
                    break;
            }
        }

        // Create damage particles
        if (EnableParticleEffects)
        {
            for (int i = 0; i < 3; i++)
            {
                CreateDamageParticle();
            }
        }
    }

    /// <summary>
    /// Gets the current tank effectiveness analysis
    /// </summary>
    public TankEffectivenessAnalysis GetEffectivenessAnalysis()
    {
        return new TankEffectivenessAnalysis
        {
            EffectiveHP = _currentEHP,
            RawHP = TankData?.CurrentShield + TankData?.CurrentArmor + TankData?.CurrentHull ?? 0,
            EHPMultiplier = _currentEHP / (TankData?.CurrentShield + TankData?.CurrentArmor + TankData?.CurrentHull ?? 1),
            ShieldLevel = _shieldLevel,
            ArmorLevel = _armorLevel,
            HullLevel = _hullLevel,
            IsUnderAttack = _isUnderAttack,
            TotalRepairRate = (TankData?.ShieldRepairRate ?? 0) + (TankData?.ArmorRepairRate ?? 0)
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
            ShowDamageFlow = false;
            _animationTimer.Interval = TimeSpan.FromMilliseconds(33); // 30 FPS
        }
        else
        {
            EnableParticleEffects = true;
            Show3DVisualization = true;
            ShowDamageFlow = true;
            _animationTimer.Interval = TimeSpan.FromMilliseconds(16); // 60 FPS
        }
    }

    #endregion

    #region Event Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTankEffectiveness control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTankEffectiveness control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnTankDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTankEffectiveness control)
        {
            control.UpdateTankData();
        }
    }

    private static void OnDamageTypesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTankEffectiveness control)
        {
            control.UpdateDamageTypes();
        }
    }

    private static void OnDisplayModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTankEffectiveness control)
        {
            control.UpdateDisplayMode();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTankEffectiveness control)
        {
            if ((bool)e.NewValue)
                control.StartAnimations();
            else
                control.StopAnimations();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTankEffectiveness control)
        {
            control.UpdateParticleSettings();
        }
    }

    private static void OnShow3DVisualizationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTankEffectiveness control)
        {
            control.Update3DVisualization();
        }
    }

    private static void OnShowResistanceBreakdownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTankEffectiveness control)
        {
            control.UpdateResistanceBreakdown();
        }
    }

    private static void OnShowDamageFlowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTankEffectiveness control)
        {
            control.UpdateDamageFlowSettings();
        }
    }

    private static void OnUpdateIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTankEffectiveness control)
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
        // Implementation would update all visual elements
    }

    private void UpdateTankData()
    {
        if (TankData != null)
        {
            UpdateLayerHealth();
            UpdateLayerLevels();
        }
    }

    private void UpdateDamageTypes()
    {
        // Update damage type displays
        // Implementation would refresh resistance breakdown
    }

    private void UpdateDisplayMode()
    {
        // Switch between different display modes
        // Implementation would show/hide different visualizations
    }

    private void UpdateParticleSettings()
    {
        if (!EnableParticleEffects)
        {
            foreach (var particle in _damageParticles)
            {
                _particleCanvas.Children.Remove(particle.Visual);
            }
            _damageParticles.Clear();
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

    private void UpdateResistanceBreakdown()
    {
        // Update resistance breakdown display
        // Implementation would show/hide resistance details
    }

    private void UpdateDamageFlowSettings()
    {
        // Update damage flow visualization
        // Implementation would show/hide damage flow effects
    }

    #endregion
}

#region Supporting Classes

/// <summary>
/// Represents tank data for visualization
/// </summary>
public class HoloTankData
{
    public double MaxShield { get; set; }
    public double CurrentShield { get; set; }
    public double MaxArmor { get; set; }
    public double CurrentArmor { get; set; }
    public double MaxHull { get; set; }
    public double CurrentHull { get; set; }
    public double ShieldRepairRate { get; set; }
    public double ArmorRepairRate { get; set; }
    public List<HoloResistance> Resistances { get; set; } = new();
}

/// <summary>
/// Represents damage type information
/// </summary>
public class HoloDamageType
{
    public string Name { get; set; }
    public Color Color { get; set; }
    public double DamageAmount { get; set; }
}

/// <summary>
/// Represents resistance values
/// </summary>
public class HoloResistance
{
    public string DamageType { get; set; }
    public string Layer { get; set; } // Shield, Armor, Hull
    public double Value { get; set; } // 0.0 to 1.0
}

/// <summary>
/// Tank display modes
/// </summary>
public enum TankDisplayMode
{
    Combined,
    Separated,
    Resistance,
    Repair
}

/// <summary>
/// Tank effectiveness analysis result
/// </summary>
public class TankEffectivenessAnalysis
{
    public double EffectiveHP { get; set; }
    public double RawHP { get; set; }
    public double EHPMultiplier { get; set; }
    public double ShieldLevel { get; set; }
    public double ArmorLevel { get; set; }
    public double HullLevel { get; set; }
    public bool IsUnderAttack { get; set; }
    public double TotalRepairRate { get; set; }
}

/// <summary>
/// Damage particle for visual effects
/// </summary>
public class HoloDamageParticle
{
    public Point Position { get; set; }
    public Vector Velocity { get; set; }
    public Color Color { get; set; }
    public double Size { get; set; }
    public TimeSpan Life { get; set; }
    public string DamageType { get; set; }
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
/// Tank calculations helper
/// </summary>
public class HoloTankCalculator
{
    public double CalculateEffectiveHP(HoloTankData tankData)
    {
        if (tankData?.Resistances == null)
            return tankData?.CurrentShield + tankData?.CurrentArmor + tankData?.CurrentHull ?? 0;

        var shieldEHP = CalculateLayerEHP(tankData.CurrentShield, tankData.Resistances, "Shield");
        var armorEHP = CalculateLayerEHP(tankData.CurrentArmor, tankData.Resistances, "Armor");
        var hullEHP = tankData.CurrentHull; // Hull typically has no resistances

        return shieldEHP + armorEHP + hullEHP;
    }

    private double CalculateLayerEHP(double rawHP, List<HoloResistance> resistances, string layer)
    {
        var layerResistances = resistances.Where(r => r.Layer == layer).ToList();
        if (!layerResistances.Any())
            return rawHP;

        // Calculate average resistance for simplified EHP
        var avgResistance = layerResistances.Average(r => r.Value);
        return rawHP / (1.0 - avgResistance);
    }
}

#endregion