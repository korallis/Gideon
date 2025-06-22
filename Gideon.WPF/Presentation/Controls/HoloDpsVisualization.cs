// ==========================================================================
// HoloDpsVisualization.cs - Real-Time Holographic DPS Visualization
// ==========================================================================
// Advanced DPS visualization featuring real-time damage calculations, animated
// damage streams, EVE-style combat metrics, and holographic damage projections.
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
/// Real-time holographic DPS visualization with animated damage streams and combat metrics
/// </summary>
public class HoloDpsVisualization : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloDpsVisualization),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloDpsVisualization),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty WeaponDataProperty =
        DependencyProperty.Register(nameof(WeaponData), typeof(ObservableCollection<HoloWeaponData>), typeof(HoloDpsVisualization),
            new PropertyMetadata(null, OnWeaponDataChanged));

    public static readonly DependencyProperty TargetDataProperty =
        DependencyProperty.Register(nameof(TargetData), typeof(HoloTargetData), typeof(HoloDpsVisualization),
            new PropertyMetadata(null, OnTargetDataChanged));

    public static readonly DependencyProperty VisualizationModeProperty =
        DependencyProperty.Register(nameof(VisualizationMode), typeof(DpsVisualizationMode), typeof(HoloDpsVisualization),
            new PropertyMetadata(DpsVisualizationMode.RealTime, OnVisualizationModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloDpsVisualization),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloDpsVisualization),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty Show3DProjectionProperty =
        DependencyProperty.Register(nameof(Show3DProjection), typeof(bool), typeof(HoloDpsVisualization),
            new PropertyMetadata(true, OnShow3DProjectionChanged));

    public static readonly DependencyProperty ShowDamageTypesProperty =
        DependencyProperty.Register(nameof(ShowDamageTypes), typeof(bool), typeof(HoloDpsVisualization),
            new PropertyMetadata(true, OnShowDamageTypesChanged));

    public static readonly DependencyProperty UpdateIntervalProperty =
        DependencyProperty.Register(nameof(UpdateInterval), typeof(TimeSpan), typeof(HoloDpsVisualization),
            new PropertyMetadata(TimeSpan.FromMilliseconds(100), OnUpdateIntervalChanged));

    public static readonly DependencyProperty SimulationSpeedProperty =
        DependencyProperty.Register(nameof(SimulationSpeed), typeof(double), typeof(HoloDpsVisualization),
            new PropertyMetadata(1.0, OnSimulationSpeedChanged));

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

    public ObservableCollection<HoloWeaponData> WeaponData
    {
        get => (ObservableCollection<HoloWeaponData>)GetValue(WeaponDataProperty);
        set => SetValue(WeaponDataProperty, value);
    }

    public HoloTargetData TargetData
    {
        get => (HoloTargetData)GetValue(TargetDataProperty);
        set => SetValue(TargetDataProperty, value);
    }

    public DpsVisualizationMode VisualizationMode
    {
        get => (DpsVisualizationMode)GetValue(VisualizationModeProperty);
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

    public bool Show3DProjection
    {
        get => (bool)GetValue(Show3DProjectionProperty);
        set => SetValue(Show3DProjectionProperty, value);
    }

    public bool ShowDamageTypes
    {
        get => (bool)GetValue(ShowDamageTypesProperty);
        set => SetValue(ShowDamageTypesProperty, value);
    }

    public TimeSpan UpdateInterval
    {
        get => (TimeSpan)GetValue(UpdateIntervalProperty);
        set => SetValue(UpdateIntervalProperty, value);
    }

    public double SimulationSpeed
    {
        get => (double)GetValue(SimulationSpeedProperty);
        set => SetValue(SimulationSpeedProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<DpsCalculatedEventArgs> DpsCalculated;
    public event EventHandler<WeaponFiredEventArgs> WeaponFired;
    public event EventHandler<DamageAppliedEventArgs> DamageApplied;

    #endregion

    #region Private Fields

    private Grid _rootGrid;
    private Viewport3D _viewport3D;
    private Canvas _overlayCanvas;
    private Canvas _particleCanvas;
    private StackPanel _metricsPanel;
    private Border _dpsDisplay;
    private TextBlock _totalDpsText;
    private TextBlock _alphaDamageText;
    private TextBlock _volleyDamageText;
    private DispatcherTimer _updateTimer;
    private DispatcherTimer _animationTimer;
    private DispatcherTimer _weaponTimer;
    private readonly List<UIElement> _particles = new();
    private readonly List<UIElement> _damageStreams = new();
    private readonly Dictionary<string, WeaponVisualization> _weaponVisuals = new();
    private DpsCalculationEngine _dpsEngine;
    private DpsMetrics _currentMetrics;
    private Model3DGroup _sceneModel;
    private readonly Random _random = new();

    #endregion

    #region Constructor

    public HoloDpsVisualization()
    {
        InitializeComponent();
        InitializeDpsEngine();
        InitializeAnimationSystem();
        Initialize3DScene();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 800;
        Height = 600;
        Background = Brushes.Transparent;

        _rootGrid = new Grid();
        _rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }); // 3D View
        _rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Metrics

        Create3DViewport();
        CreateMetricsPanel();
        CreateOverlayCanvas();
        CreateParticleSystem();

        Content = _rootGrid;
    }

    private void Create3DViewport()
    {
        _viewport3D = new Viewport3D
        {
            Background = CreateHolographicBackground(0.1),
            Visibility = Show3DProjection ? Visibility.Visible : Visibility.Collapsed
        };

        // Add camera
        var camera = new PerspectiveCamera
        {
            Position = new Point3D(0, 0, 10),
            LookDirection = new Vector3D(0, 0, -1),
            UpDirection = new Vector3D(0, 1, 0),
            FieldOfView = 45
        };
        _viewport3D.Camera = camera;

        // Add lighting
        var ambientLight = new AmbientLight(Colors.Gray) { Color = Color.FromRgb(64, 64, 64) };
        var directionalLight = new DirectionalLight(Colors.White) 
        { 
            Direction = new Vector3D(-1, -1, -1),
            Color = GetColorForScheme(EVEColorScheme)
        };

        _sceneModel = new Model3DGroup();
        _sceneModel.Children.Add(ambientLight);
        _sceneModel.Children.Add(directionalLight);

        var modelVisual = new ModelVisual3D { Content = _sceneModel };
        _viewport3D.Children.Add(modelVisual);

        Grid.SetColumn(_viewport3D, 0);
        _rootGrid.Children.Add(_viewport3D);
    }

    private void CreateMetricsPanel()
    {
        _metricsPanel = new StackPanel
        {
            Margin = new Thickness(10),
            Background = CreateHolographicBackground(0.2),
            Effect = CreateGlowEffect()
        };

        CreateDpsDisplay();
        CreateWeaponsList();
        CreateDamageBreakdown();
        CreateSimulationControls();

        Grid.SetColumn(_metricsPanel, 1);
        _rootGrid.Children.Add(_metricsPanel);
    }

    private void CreateDpsDisplay()
    {
        _dpsDisplay = new Border
        {
            Margin = new Thickness(10),
            Background = CreateHolographicBackground(0.4),
            BorderBrush = GetBrushForColorScheme(EVEColorScheme),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(8),
            Effect = CreateIntenseGlowEffect()
        };

        var dpsGrid = new Grid();
        dpsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        dpsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        dpsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        dpsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var titleBlock = new TextBlock
        {
            Text = "DPS METRICS",
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(10, 10, 10, 5)
        };

        _totalDpsText = new TextBlock
        {
            Text = "Total DPS: 0",
            FontSize = 24,
            FontWeight = FontWeights.Bold,
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(10, 5)
        };

        _alphaDamageText = new TextBlock
        {
            Text = "Alpha: 0",
            FontSize = 14,
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(10, 2)
        };

        _volleyDamageText = new TextBlock
        {
            Text = "Volley: 0",
            FontSize = 14,
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(10, 2, 10, 10)
        };

        Grid.SetRow(titleBlock, 0);
        Grid.SetRow(_totalDpsText, 1);
        Grid.SetRow(_alphaDamageText, 2);
        Grid.SetRow(_volleyDamageText, 3);

        dpsGrid.Children.Add(titleBlock);
        dpsGrid.Children.Add(_totalDpsText);
        dpsGrid.Children.Add(_alphaDamageText);
        dpsGrid.Children.Add(_volleyDamageText);

        _dpsDisplay.Child = dpsGrid;
        _metricsPanel.Children.Add(_dpsDisplay);
    }

    private void CreateWeaponsList()
    {
        var weaponsHeader = new TextBlock
        {
            Text = "WEAPONS",
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            Margin = new Thickness(10, 15, 10, 5)
        };

        var weaponsScroll = new ScrollViewer
        {
            Height = 200,
            Margin = new Thickness(10, 0),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Background = CreateHolographicBackground(0.1),
            BorderBrush = GetBrushForColorScheme(EVEColorScheme),
            BorderThickness = new Thickness(1),
            Effect = CreateGlowEffect()
        };

        var weaponsPanel = new StackPanel();
        weaponsScroll.Content = weaponsPanel;

        _metricsPanel.Children.Add(weaponsHeader);
        _metricsPanel.Children.Add(weaponsScroll);
    }

    private void CreateDamageBreakdown()
    {
        if (!ShowDamageTypes) return;

        var damageHeader = new TextBlock
        {
            Text = "DAMAGE BREAKDOWN",
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            Margin = new Thickness(10, 15, 10, 5)
        };

        var damagePanel = new StackPanel
        {
            Margin = new Thickness(10, 0)
        };

        CreateDamageTypeBar("EM", Colors.Purple, 0);
        CreateDamageTypeBar("Thermal", Colors.Red, 0);
        CreateDamageTypeBar("Kinetic", Colors.Blue, 0);
        CreateDamageTypeBar("Explosive", Colors.Orange, 0);

        _metricsPanel.Children.Add(damageHeader);
        _metricsPanel.Children.Add(damagePanel);
    }

    private void CreateDamageTypeBar(string damageType, Color color, double percentage)
    {
        var barContainer = new Grid
        {
            Height = 25,
            Margin = new Thickness(0, 2)
        };

        barContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60, GridUnitType.Pixel) });
        barContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        barContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40, GridUnitType.Pixel) });

        var label = new TextBlock
        {
            Text = damageType,
            FontSize = 10,
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            VerticalAlignment = VerticalAlignment.Center
        };

        var barBackground = new Border
        {
            Background = CreateHolographicBackground(0.2),
            BorderBrush = new SolidColorBrush(color),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(2)
        };

        var barFill = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(128, color.R, color.G, color.B)),
            CornerRadius = new CornerRadius(2),
            Width = 0,
            HorizontalAlignment = HorizontalAlignment.Left,
            Tag = damageType
        };

        var percentText = new TextBlock
        {
            Text = $"{percentage:F0}%",
            FontSize = 9,
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        Grid.SetColumn(label, 0);
        Grid.SetColumn(barBackground, 1);
        Grid.SetColumn(percentText, 2);

        barBackground.Child = barFill;

        barContainer.Children.Add(label);
        barContainer.Children.Add(barBackground);
        barContainer.Children.Add(percentText);

        var damagePanel = _metricsPanel.Children.OfType<StackPanel>().LastOrDefault();
        damagePanel?.Children.Add(barContainer);
    }

    private void CreateSimulationControls()
    {
        var controlsHeader = new TextBlock
        {
            Text = "SIMULATION",
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            Margin = new Thickness(10, 15, 10, 5)
        };

        var controlsPanel = new StackPanel
        {
            Margin = new Thickness(10, 0)
        };

        var speedLabel = new TextBlock
        {
            Text = "Speed:",
            FontSize = 12,
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            Margin = new Thickness(0, 5, 0, 2)
        };

        var speedSlider = new Slider
        {
            Minimum = 0.1,
            Maximum = 5.0,
            Value = SimulationSpeed,
            TickPlacement = System.Windows.Controls.Primitives.TickPlacement.None,
            Margin = new Thickness(0, 0, 0, 5)
        };

        speedSlider.ValueChanged += (s, e) => SimulationSpeed = e.NewValue;

        var pauseButton = new Button
        {
            Content = "Pause/Resume",
            Margin = new Thickness(0, 5),
            Padding = new Thickness(10, 5),
            Background = CreateHolographicBackground(0.3),
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            BorderBrush = GetBrushForColorScheme(EVEColorScheme),
            BorderThickness = new Thickness(1),
            Effect = CreateGlowEffect(),
            Style = CreateHolographicButtonStyle()
        };

        pauseButton.Click += PauseButton_Click;

        controlsPanel.Children.Add(speedLabel);
        controlsPanel.Children.Add(speedSlider);
        controlsPanel.Children.Add(pauseButton);

        _metricsPanel.Children.Add(controlsHeader);
        _metricsPanel.Children.Add(controlsPanel);
    }

    private void CreateOverlayCanvas()
    {
        _overlayCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false
        };

        Grid.SetColumnSpan(_overlayCanvas, 2);
        _rootGrid.Children.Add(_overlayCanvas);
    }

    private void CreateParticleSystem()
    {
        _particleCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false
        };

        Grid.SetColumnSpan(_particleCanvas, 2);
        _rootGrid.Children.Add(_particleCanvas);
    }

    private void InitializeDpsEngine()
    {
        _dpsEngine = new DpsCalculationEngine();
        
        if (WeaponData == null)
        {
            WeaponData = new ObservableCollection<HoloWeaponData>();
        }

        WeaponData.CollectionChanged += WeaponData_CollectionChanged;
    }

    private void InitializeAnimationSystem()
    {
        _updateTimer = new DispatcherTimer
        {
            Interval = UpdateInterval
        };
        _updateTimer.Tick += UpdateTimer_Tick;

        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        _animationTimer.Tick += AnimationTimer_Tick;

        _weaponTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _weaponTimer.Tick += WeaponTimer_Tick;
    }

    private void Initialize3DScene()
    {
        if (!Show3DProjection || _sceneModel == null) return;

        // Create 3D weapon positions
        Create3DWeaponPositions();
        
        // Create target representation
        Create3DTarget();
    }

    #endregion

    #region DPS Calculation and Updates

    private void WeaponData_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (HoloWeaponData weapon in e.NewItems)
            {
                CreateWeaponVisualization(weapon);
            }
        }

        if (e.OldItems != null)
        {
            foreach (HoloWeaponData weapon in e.OldItems)
            {
                RemoveWeaponVisualization(weapon);
            }
        }

        UpdateDpsCalculations();
    }

    private void CreateWeaponVisualization(HoloWeaponData weapon)
    {
        var visualization = new WeaponVisualization
        {
            Weapon = weapon,
            LastFiredTime = DateTime.MinValue,
            CycleProgress = 0
        };

        // Create 3D representation
        if (Show3DProjection)
        {
            Create3DWeapon(weapon, visualization);
        }

        // Create 2D overlay
        CreateWeaponOverlay(weapon, visualization);

        _weaponVisuals[weapon.Id] = visualization;
    }

    private void RemoveWeaponVisualization(HoloWeaponData weapon)
    {
        if (_weaponVisuals.TryGetValue(weapon.Id, out var visualization))
        {
            // Remove 3D model
            if (visualization.Model3D != null)
            {
                _sceneModel.Children.Remove(visualization.Model3D);
            }

            // Remove overlay elements
            if (visualization.OverlayElements != null)
            {
                foreach (var element in visualization.OverlayElements)
                {
                    _overlayCanvas.Children.Remove(element);
                }
            }

            _weaponVisuals.Remove(weapon.Id);
        }
    }

    private async void UpdateTimer_Tick(object sender, EventArgs e)
    {
        await UpdateDpsCalculations();
        UpdateMetricsDisplay();
    }

    private void AnimationTimer_Tick(object sender, EventArgs e)
    {
        UpdateParticles();
        UpdateDamageStreams();
        UpdateWeaponAnimations();
    }

    private void WeaponTimer_Tick(object sender, EventArgs e)
    {
        if (VisualizationMode == DpsVisualizationMode.RealTime)
        {
            SimulateWeaponFiring();
        }
    }

    private async Task UpdateDpsCalculations()
    {
        if (WeaponData?.Any() != true || TargetData == null) return;

        _currentMetrics = await _dpsEngine.CalculateAsync(WeaponData, TargetData);
        DpsCalculated?.Invoke(this, new DpsCalculatedEventArgs(_currentMetrics));
    }

    private void UpdateMetricsDisplay()
    {
        if (_currentMetrics == null) return;

        // Update main DPS display
        _totalDpsText.Text = $"Total DPS: {_currentMetrics.TotalDps:N0}";
        _alphaDamageText.Text = $"Alpha: {_currentMetrics.AlphaDamage:N0}";
        _volleyDamageText.Text = $"Volley: {_currentMetrics.VolleyDamage:N0}";

        // Update damage type breakdown
        UpdateDamageTypeBreakdown();

        // Animate DPS changes
        if (EnableAnimations)
        {
            AnimateDpsChange();
        }
    }

    private void UpdateDamageTypeBreakdown()
    {
        if (!ShowDamageTypes || _currentMetrics?.DamageBreakdown == null) return;

        var damagePanel = _metricsPanel.Children.OfType<StackPanel>().LastOrDefault();
        if (damagePanel == null) return;

        var totalDamage = _currentMetrics.DamageBreakdown.Values.Sum();
        if (totalDamage <= 0) return;

        foreach (var kvp in _currentMetrics.DamageBreakdown)
        {
            var percentage = (kvp.Value / totalDamage) * 100;
            UpdateDamageTypeBar(kvp.Key, percentage);
        }
    }

    private void UpdateDamageTypeBar(string damageType, double percentage)
    {
        var damagePanel = _metricsPanel.Children.OfType<StackPanel>().LastOrDefault();
        if (damagePanel == null) return;

        foreach (Grid barContainer in damagePanel.Children.OfType<Grid>())
        {
            var barBackground = barContainer.Children.OfType<Border>().FirstOrDefault();
            var barFill = barBackground?.Child as Border;
            var percentText = barContainer.Children.OfType<TextBlock>().LastOrDefault();

            if (barFill?.Tag?.ToString() == damageType)
            {
                var targetWidth = barBackground.ActualWidth * (percentage / 100);
                
                if (EnableAnimations)
                {
                    var widthAnimation = new DoubleAnimation
                    {
                        To = targetWidth,
                        Duration = TimeSpan.FromMilliseconds(500),
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    };
                    barFill.BeginAnimation(WidthProperty, widthAnimation);
                }
                else
                {
                    barFill.Width = targetWidth;
                }

                if (percentText != null)
                {
                    percentText.Text = $"{percentage:F0}%";
                }
                break;
            }
        }
    }

    #endregion

    #region Weapon Simulation

    private void SimulateWeaponFiring()
    {
        if (WeaponData?.Any() != true) return;

        var currentTime = DateTime.UtcNow;

        foreach (var weapon in WeaponData)
        {
            if (_weaponVisuals.TryGetValue(weapon.Id, out var visualization))
            {
                var timeSinceLastFire = currentTime - visualization.LastFiredTime;
                var cycleTime = TimeSpan.FromSeconds(weapon.RateOfFire * SimulationSpeed);

                if (timeSinceLastFire >= cycleTime)
                {
                    FireWeapon(weapon, visualization);
                    visualization.LastFiredTime = currentTime;
                }

                // Update cycle progress
                visualization.CycleProgress = Math.Min(timeSinceLastFire.TotalMilliseconds / cycleTime.TotalMilliseconds, 1.0);
            }
        }
    }

    private void FireWeapon(HoloWeaponData weapon, WeaponVisualization visualization)
    {
        WeaponFired?.Invoke(this, new WeaponFiredEventArgs(weapon));

        if (EnableAnimations)
        {
            AnimateWeaponFire(visualization);
        }

        if (EnableParticleEffects)
        {
            CreateMuzzleFlash(visualization);
        }

        // Create damage stream
        CreateDamageStream(weapon, visualization);

        // Apply damage to target
        var damage = _dpsEngine.CalculateWeaponDamage(weapon, TargetData);
        ApplyDamageToTarget(weapon, damage);
    }

    private void CreateDamageStream(HoloWeaponData weapon, WeaponVisualization visualization)
    {
        if (visualization.Position3D == null || TargetData?.Position3D == null) return;

        var stream = new Line
        {
            X1 = visualization.Position3D.Value.X * 100 + 400, // Convert 3D to 2D screen space
            Y1 = visualization.Position3D.Value.Y * 100 + 300,
            X2 = TargetData.Position3D.Value.X * 100 + 400,
            Y2 = TargetData.Position3D.Value.Y * 100 + 300,
            Stroke = GetBrushForDamageType(weapon.PrimaryDamageType),
            StrokeThickness = 3,
            Opacity = 0.8,
            Effect = CreateGlowEffect(1.0)
        };

        _damageStreams.Add(stream);
        _overlayCanvas.Children.Add(stream);

        // Animate stream
        if (EnableAnimations)
        {
            AnimateDamageStream(stream);
        }
    }

    private void ApplyDamageToTarget(HoloWeaponData weapon, double damage)
    {
        DamageApplied?.Invoke(this, new DamageAppliedEventArgs(weapon, TargetData, damage));

        if (EnableParticleEffects)
        {
            CreateDamageImpactParticles(damage);
        }
    }

    #endregion

    #region 3D Scene Management

    private void Create3DWeaponPositions()
    {
        if (_sceneModel == null) return;

        // Clear existing weapon models
        var weaponModels = _sceneModel.Children.OfType<GeometryModel3D>()
            .Where(m => m.GetValue(FrameworkElement.TagProperty)?.ToString() == "Weapon")
            .ToList();

        foreach (var model in weaponModels)
        {
            _sceneModel.Children.Remove(model);
        }
    }

    private void Create3DWeapon(HoloWeaponData weapon, WeaponVisualization visualization)
    {
        // Create weapon geometry
        var weaponGeometry = new BoxVisual3D
        {
            Center = GetWeaponPosition(weapon),
            Length = 0.5,
            Width = 0.2,
            Height = 0.2
        };

        var weaponMaterial = new DiffuseMaterial
        {
            Brush = GetBrushForDamageType(weapon.PrimaryDamageType)
        };

        var weaponModel = new GeometryModel3D
        {
            Geometry = weaponGeometry.Geometry,
            Material = weaponMaterial,
            Tag = "Weapon"
        };

        visualization.Model3D = weaponModel;
        visualization.Position3D = GetWeaponPosition(weapon);

        _sceneModel.Children.Add(weaponModel);
    }

    private void Create3DTarget()
    {
        if (TargetData?.Position3D == null) return;

        var targetGeometry = new SphereVisual3D
        {
            Center = TargetData.Position3D.Value,
            Radius = 1.0
        };

        var targetMaterial = new DiffuseMaterial
        {
            Brush = new SolidColorBrush(Colors.Red)
        };

        var targetModel = new GeometryModel3D
        {
            Geometry = targetGeometry.Geometry,
            Material = targetMaterial,
            Tag = "Target"
        };

        _sceneModel.Children.Add(targetModel);
    }

    private Point3D GetWeaponPosition(HoloWeaponData weapon)
    {
        // Calculate weapon position based on slot type and index
        var angle = (2 * Math.PI * weapon.SlotIndex) / 8; // Assume 8 slots max
        var radius = weapon.SlotType switch
        {
            SlotType.High => 2.5,
            SlotType.Mid => 2.0,
            SlotType.Low => 1.5,
            _ => 2.0
        };

        return new Point3D(
            radius * Math.Cos(angle),
            radius * Math.Sin(angle),
            0
        );
    }

    #endregion

    #region Animation and Visual Effects

    private void UpdateParticles()
    {
        foreach (var particle in _particles.ToList())
        {
            if (particle.Opacity <= 0.1)
            {
                _particles.Remove(particle);
                _particleCanvas.Children.Remove(particle);
            }
        }
    }

    private void UpdateDamageStreams()
    {
        foreach (var stream in _damageStreams.ToList())
        {
            if (stream.Opacity <= 0.1)
            {
                _damageStreams.Remove(stream);
                _overlayCanvas.Children.Remove(stream);
            }
        }
    }

    private void UpdateWeaponAnimations()
    {
        foreach (var visualization in _weaponVisuals.Values)
        {
            if (visualization.CycleProgress > 0 && visualization.CycleProgress < 1)
            {
                // Update weapon charging animation
                UpdateWeaponChargeAnimation(visualization);
            }
        }
    }

    private void AnimateDpsChange()
    {
        // Flash effect for DPS display
        var flashAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.7,
            Duration = TimeSpan.FromMilliseconds(150),
            AutoReverse = true
        };

        _totalDpsText.BeginAnimation(OpacityProperty, flashAnimation);
    }

    private void AnimateWeaponFire(WeaponVisualization visualization)
    {
        if (visualization.Model3D != null)
        {
            // Scale animation for weapon fire
            var scaleTransform = new ScaleTransform3D(1.2, 1.2, 1.2);
            var scaleAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 1.2,
                Duration = TimeSpan.FromMilliseconds(100),
                AutoReverse = true
            };

            visualization.Model3D.Transform = scaleTransform;
            scaleTransform.BeginAnimation(ScaleTransform3D.ScaleXProperty, scaleAnimation);
            scaleTransform.BeginAnimation(ScaleTransform3D.ScaleYProperty, scaleAnimation);
            scaleTransform.BeginAnimation(ScaleTransform3D.ScaleZProperty, scaleAnimation);
        }
    }

    private void AnimateDamageStream(Line stream)
    {
        // Animate stream opacity and thickness
        var fadeAnimation = new DoubleAnimation
        {
            From = 0.8,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(500)
        };

        var thicknessAnimation = new DoubleAnimation
        {
            From = 3,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(500)
        };

        stream.BeginAnimation(OpacityProperty, fadeAnimation);
        stream.BeginAnimation(Line.StrokeThicknessProperty, thicknessAnimation);

        // Remove after animation
        Task.Delay(500).ContinueWith(t =>
        {
            Dispatcher.BeginInvoke(() =>
            {
                _damageStreams.Remove(stream);
                _overlayCanvas.Children.Remove(stream);
            });
        });
    }

    private void UpdateWeaponChargeAnimation(WeaponVisualization visualization)
    {
        // Update charging effect based on cycle progress
        var intensity = Math.Sin(visualization.CycleProgress * Math.PI);
        
        if (visualization.Model3D?.Material is DiffuseMaterial material)
        {
            var brush = material.Brush as SolidColorBrush;
            if (brush != null)
            {
                var color = brush.Color;
                material.Brush = new SolidColorBrush(Color.FromArgb(
                    (byte)(color.A * intensity),
                    color.R, color.G, color.B));
            }
        }
    }

    #endregion

    #region Particle Effects

    private void CreateMuzzleFlash(WeaponVisualization visualization)
    {
        if (!EnableParticleEffects || visualization.Position3D == null) return;

        var screenPos = Convert3DToScreen(visualization.Position3D.Value);

        for (int i = 0; i < 8; i++)
        {
            var particle = new Ellipse
            {
                Width = _random.NextDouble() * 6 + 3,
                Height = _random.NextDouble() * 6 + 3,
                Fill = GetBrushForDamageType(visualization.Weapon.PrimaryDamageType),
                Opacity = 0.9,
                Effect = CreateGlowEffect(0.8)
            };

            var angle = (2 * Math.PI * i) / 8;
            var distance = _random.NextDouble() * 30 + 10;

            var startX = screenPos.X;
            var startY = screenPos.Y;
            var endX = startX + distance * Math.Cos(angle);
            var endY = startY + distance * Math.Sin(angle);

            Canvas.SetLeft(particle, startX);
            Canvas.SetTop(particle, startY);
            Canvas.SetZIndex(particle, 1000);

            _particles.Add(particle);
            _particleCanvas.Children.Add(particle);

            // Animate particle explosion
            var moveX = new DoubleAnimation
            {
                From = startX,
                To = endX,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var moveY = new DoubleAnimation
            {
                From = startY,
                To = endY,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var fadeOut = new DoubleAnimation
            {
                From = 0.9,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200)
            };

            particle.BeginAnimation(Canvas.LeftProperty, moveX);
            particle.BeginAnimation(Canvas.TopProperty, moveY);
            particle.BeginAnimation(OpacityProperty, fadeOut);

            Task.Delay(200).ContinueWith(t =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    _particles.Remove(particle);
                    _particleCanvas.Children.Remove(particle);
                });
            });
        }
    }

    private void CreateDamageImpactParticles(double damage)
    {
        if (!EnableParticleEffects || TargetData?.Position3D == null) return;

        var screenPos = Convert3DToScreen(TargetData.Position3D.Value);
        var particleCount = Math.Min((int)(damage / 100), 20);

        for (int i = 0; i < particleCount; i++)
        {
            var particle = new Rectangle
            {
                Width = _random.NextDouble() * 4 + 2,
                Height = _random.NextDouble() * 8 + 4,
                Fill = new SolidColorBrush(Colors.Orange),
                Opacity = 0.8,
                Effect = CreateGlowEffect(0.6)
            };

            var angle = _random.NextDouble() * 2 * Math.PI;
            var distance = _random.NextDouble() * 50 + 20;

            var startX = screenPos.X;
            var startY = screenPos.Y;
            var endX = startX + distance * Math.Cos(angle);
            var endY = startY + distance * Math.Sin(angle);

            Canvas.SetLeft(particle, startX);
            Canvas.SetTop(particle, startY);
            Canvas.SetZIndex(particle, 999);

            _particles.Add(particle);
            _particleCanvas.Children.Add(particle);

            // Animate impact explosion
            var moveX = new DoubleAnimation
            {
                From = startX,
                To = endX,
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var moveY = new DoubleAnimation
            {
                From = startY,
                To = endY,
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var fadeOut = new DoubleAnimation
            {
                From = 0.8,
                To = 0,
                BeginTime = TimeSpan.FromMilliseconds(200),
                Duration = TimeSpan.FromMilliseconds(200)
            };

            particle.BeginAnimation(Canvas.LeftProperty, moveX);
            particle.BeginAnimation(Canvas.TopProperty, moveY);
            particle.BeginAnimation(OpacityProperty, fadeOut);

            Task.Delay(400).ContinueWith(t =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    _particles.Remove(particle);
                    _particleCanvas.Children.Remove(particle);
                });
            });
        }
    }

    #endregion

    #region Helper Methods

    private void CreateWeaponOverlay(HoloWeaponData weapon, WeaponVisualization visualization)
    {
        // Create overlay elements for 2D display
        visualization.OverlayElements = new List<UIElement>();

        var weaponCard = new Border
        {
            Width = 200,
            Height = 40,
            Background = CreateHolographicBackground(0.3),
            BorderBrush = GetBrushForDamageType(weapon.PrimaryDamageType),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3),
            Effect = CreateGlowEffect()
        };

        var cardContent = new Grid();
        cardContent.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        cardContent.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var weaponName = new TextBlock
        {
            Text = weapon.Name,
            FontSize = 10,
            Foreground = GetBrushForDamageType(weapon.PrimaryDamageType),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 0)
        };

        var dpsText = new TextBlock
        {
            Text = $"{weapon.Damage / weapon.RateOfFire:N0} DPS",
            FontSize = 9,
            Foreground = GetBrushForDamageType(weapon.PrimaryDamageType),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 0)
        };

        Grid.SetColumn(weaponName, 0);
        Grid.SetColumn(dpsText, 1);
        cardContent.Children.Add(weaponName);
        cardContent.Children.Add(dpsText);

        weaponCard.Child = cardContent;

        // Position overlay element
        var yOffset = weapon.SlotIndex * 45;
        Canvas.SetLeft(weaponCard, 10);
        Canvas.SetTop(weaponCard, 10 + yOffset);

        visualization.OverlayElements.Add(weaponCard);
        _overlayCanvas.Children.Add(weaponCard);
    }

    private Point Convert3DToScreen(Point3D point3D)
    {
        // Simple conversion from 3D to 2D screen coordinates
        return new Point(
            point3D.X * 100 + 400,
            point3D.Y * 100 + 300
        );
    }

    private Brush GetBrushForDamageType(string damageType)
    {
        return damageType.ToLowerInvariant() switch
        {
            "em" => new SolidColorBrush(Colors.Purple),
            "thermal" => new SolidColorBrush(Colors.Red),
            "kinetic" => new SolidColorBrush(Colors.Blue),
            "explosive" => new SolidColorBrush(Colors.Orange),
            _ => GetBrushForColorScheme(EVEColorScheme)
        };
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableAnimations)
        {
            _animationTimer.Start();
        }

        _updateTimer.Start();
        _weaponTimer.Start();

        Initialize3DScene();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _updateTimer?.Stop();
        _animationTimer?.Stop();
        _weaponTimer?.Stop();
    }

    private void PauseButton_Click(object sender, RoutedEventArgs e)
    {
        if (_weaponTimer.IsEnabled)
        {
            _weaponTimer.Stop();
        }
        else
        {
            _weaponTimer.Start();
        }
    }

    #endregion

    #region Style Helpers

    private Brush CreateHolographicBackground(double opacity)
    {
        var brush = new LinearGradientBrush();
        brush.StartPoint = new Point(0, 0);
        brush.EndPoint = new Point(1, 1);

        var color = GetColorForScheme(EVEColorScheme);
        brush.GradientStops.Add(new GradientStop(Color.FromArgb((byte)(opacity * 40), color.R, color.G, color.B), 0));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb((byte)(opacity * 20), color.R, color.G, color.B), 0.5));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb((byte)(opacity * 40), color.R, color.G, color.B), 1));

        return brush;
    }

    private Brush GetBrushForColorScheme(EVEColorScheme scheme)
    {
        return scheme switch
        {
            EVEColorScheme.ElectricBlue => new SolidColorBrush(Color.FromRgb(0, 191, 255)),
            EVEColorScheme.AmberGold => new SolidColorBrush(Color.FromRgb(255, 191, 0)),
            EVEColorScheme.CrimsonRed => new SolidColorBrush(Color.FromRgb(220, 20, 60)),
            EVEColorScheme.EmeraldGreen => new SolidColorBrush(Color.FromRgb(0, 255, 127)),
            _ => new SolidColorBrush(Color.FromRgb(0, 191, 255))
        };
    }

    private Color GetColorForScheme(EVEColorScheme scheme)
    {
        return scheme switch
        {
            EVEColorScheme.ElectricBlue => Color.FromRgb(0, 191, 255),
            EVEColorScheme.AmberGold => Color.FromRgb(255, 191, 0),
            EVEColorScheme.CrimsonRed => Color.FromRgb(220, 20, 60),
            EVEColorScheme.EmeraldGreen => Color.FromRgb(0, 255, 127),
            _ => Color.FromRgb(0, 191, 255)
        };
    }

    private Effect CreateGlowEffect(double intensity = 1.0)
    {
        return new DropShadowEffect
        {
            Color = GetColorForScheme(EVEColorScheme),
            BlurRadius = 8 * intensity,
            Opacity = 0.6 * intensity,
            ShadowDepth = 0
        };
    }

    private Effect CreateIntenseGlowEffect()
    {
        return new DropShadowEffect
        {
            Color = GetColorForScheme(EVEColorScheme),
            BlurRadius = 15,
            Opacity = 0.9,
            ShadowDepth = 0
        };
    }

    private Style CreateHolographicButtonStyle()
    {
        var style = new Style(typeof(Button));
        style.Setters.Add(new Setter(ForegroundProperty, GetBrushForColorScheme(EVEColorScheme)));
        style.Setters.Add(new Setter(FontFamilyProperty, new FontFamily("Segoe UI")));
        style.Setters.Add(new Setter(FontWeightProperty, FontWeights.Medium));
        style.Setters.Add(new Setter(CursorProperty, Cursors.Hand));
        return style;
    }

    #endregion

    #region Dependency Property Callbacks

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Update holographic effects intensity
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Update color scheme
    }

    private static void OnWeaponDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDpsVisualization control)
        {
            if (e.OldValue is ObservableCollection<HoloWeaponData> oldWeapons)
            {
                oldWeapons.CollectionChanged -= control.WeaponData_CollectionChanged;
            }

            if (e.NewValue is ObservableCollection<HoloWeaponData> newWeapons)
            {
                newWeapons.CollectionChanged += control.WeaponData_CollectionChanged;
            }
        }
    }

    private static void OnTargetDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDpsVisualization control)
        {
            control.Initialize3DScene();
        }
    }

    private static void OnVisualizationModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Update visualization mode
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDpsVisualization control)
        {
            if ((bool)e.NewValue)
                control._animationTimer.Start();
            else
                control._animationTimer.Stop();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDpsVisualization control && !(bool)e.NewValue)
        {
            foreach (var particle in control._particles.ToList())
            {
                control._particles.Remove(particle);
                control._particleCanvas.Children.Remove(particle);
            }
        }
    }

    private static void OnShow3DProjectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDpsVisualization control)
        {
            control._viewport3D.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnShowDamageTypesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Update damage type visibility
    }

    private static void OnUpdateIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDpsVisualization control)
        {
            control._updateTimer.Interval = (TimeSpan)e.NewValue;
        }
    }

    private static void OnSimulationSpeedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Simulation speed changed
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void SetSimplifiedMode(bool enabled)
    {
        EnableAnimations = !enabled;
        EnableParticleEffects = !enabled;
        Show3DProjection = !enabled;
        
        if (enabled)
        {
            _animationTimer?.Stop();
            foreach (var particle in _particles.ToList())
            {
                _particles.Remove(particle);
                _particleCanvas.Children.Remove(particle);
            }
            foreach (var stream in _damageStreams.ToList())
            {
                _damageStreams.Remove(stream);
                _overlayCanvas.Children.Remove(stream);
            }
        }
        else
        {
            _animationTimer?.Start();
        }
    }

    public bool IsSimplifiedModeActive => !EnableAnimations || !EnableParticleEffects || !Show3DProjection;

    #endregion

    #region Public Methods

    /// <summary>
    /// Starts DPS simulation
    /// </summary>
    public void StartSimulation()
    {
        _updateTimer.Start();
        _weaponTimer.Start();
        if (EnableAnimations)
            _animationTimer.Start();
    }

    /// <summary>
    /// Stops DPS simulation
    /// </summary>
    public void StopSimulation()
    {
        _updateTimer.Stop();
        _weaponTimer.Stop();
        _animationTimer.Stop();
    }

    /// <summary>
    /// Gets current DPS metrics
    /// </summary>
    public DpsMetrics GetCurrentMetrics()
    {
        return _currentMetrics;
    }

    /// <summary>
    /// Manually triggers weapon fire
    /// </summary>
    public void FireWeaponManually(string weaponId)
    {
        var weapon = WeaponData?.FirstOrDefault(w => w.Id == weaponId);
        if (weapon != null && _weaponVisuals.TryGetValue(weaponId, out var visualization))
        {
            FireWeapon(weapon, visualization);
        }
    }

    #endregion
}

#region Supporting Types

/// <summary>
/// DPS visualization modes
/// </summary>
public enum DpsVisualizationMode
{
    RealTime,
    Burst,
    Sustained,
    Alpha
}

/// <summary>
/// Weapon visualization container
/// </summary>
public class WeaponVisualization
{
    public HoloWeaponData Weapon { get; set; }
    public GeometryModel3D Model3D { get; set; }
    public Point3D? Position3D { get; set; }
    public List<UIElement> OverlayElements { get; set; }
    public DateTime LastFiredTime { get; set; }
    public double CycleProgress { get; set; }
}

/// <summary>
/// Weapon data for DPS calculation
/// </summary>
public class HoloWeaponData
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public double Damage { get; set; }
    public double RateOfFire { get; set; } // seconds per shot
    public string PrimaryDamageType { get; set; } = "Kinetic";
    public Dictionary<string, double> DamageBreakdown { get; set; } = new();
    public SlotType SlotType { get; set; }
    public int SlotIndex { get; set; }
    public double OptimalRange { get; set; }
    public double FalloffRange { get; set; }
    public double TrackingSpeed { get; set; }
}

/// <summary>
/// Target data for DPS calculation
/// </summary>
public class HoloTargetData
{
    public string Name { get; set; } = string.Empty;
    public Point3D? Position3D { get; set; }
    public double Distance { get; set; }
    public double Velocity { get; set; }
    public double SignatureRadius { get; set; }
    public Dictionary<string, double> Resistances { get; set; } = new();
    public double ShieldHP { get; set; }
    public double ArmorHP { get; set; }
    public double HullHP { get; set; }
}

/// <summary>
/// DPS calculation metrics
/// </summary>
public class DpsMetrics
{
    public double TotalDps { get; set; }
    public double AlphaDamage { get; set; }
    public double VolleyDamage { get; set; }
    public Dictionary<string, double> DamageBreakdown { get; set; } = new();
    public Dictionary<string, double> WeaponDps { get; set; } = new();
    public double EffectiveDps { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DPS calculation engine
/// </summary>
public class DpsCalculationEngine
{
    public async Task<DpsMetrics> CalculateAsync(IEnumerable<HoloWeaponData> weapons, HoloTargetData target)
    {
        await Task.Delay(10); // Simulate calculation time

        var metrics = new DpsMetrics();
        var damageBreakdown = new Dictionary<string, double>();
        var weaponDps = new Dictionary<string, double>();

        foreach (var weapon in weapons)
        {
            var weaponDamage = CalculateWeaponDamage(weapon, target);
            var weaponDpsValue = weaponDamage / weapon.RateOfFire;
            
            weaponDps[weapon.Name] = weaponDpsValue;
            metrics.TotalDps += weaponDpsValue;
            metrics.AlphaDamage += weaponDamage;

            // Damage type breakdown
            foreach (var kvp in weapon.DamageBreakdown)
            {
                if (!damageBreakdown.ContainsKey(kvp.Key))
                    damageBreakdown[kvp.Key] = 0;
                damageBreakdown[kvp.Key] += kvp.Value * weaponDpsValue / weaponDamage;
            }
        }

        metrics.VolleyDamage = metrics.AlphaDamage;
        metrics.DamageBreakdown = damageBreakdown;
        metrics.WeaponDps = weaponDps;
        metrics.EffectiveDps = ApplyTargetResistances(metrics.TotalDps, target);

        return metrics;
    }

    public double CalculateWeaponDamage(HoloWeaponData weapon, HoloTargetData target)
    {
        var baseDamage = weapon.Damage;
        
        // Apply range modifier
        var rangeModifier = CalculateRangeModifier(weapon, target);
        
        // Apply tracking modifier
        var trackingModifier = CalculateTrackingModifier(weapon, target);
        
        return baseDamage * rangeModifier * trackingModifier;
    }

    private double CalculateRangeModifier(HoloWeaponData weapon, HoloTargetData target)
    {
        if (target.Distance <= weapon.OptimalRange)
            return 1.0;

        var falloffDistance = target.Distance - weapon.OptimalRange;
        var falloffRatio = falloffDistance / weapon.FalloffRange;
        
        return Math.Pow(0.5, falloffRatio * falloffRatio);
    }

    private double CalculateTrackingModifier(HoloWeaponData weapon, HoloTargetData target)
    {
        if (target.Velocity <= 0 || target.Distance <= 0)
            return 1.0;

        var angularVelocity = target.Velocity / target.Distance;
        var trackingRatio = angularVelocity / weapon.TrackingSpeed;
        
        return Math.Pow(0.5, trackingRatio * trackingRatio);
    }

    private double ApplyTargetResistances(double damage, HoloTargetData target)
    {
        // Simplified resistance calculation
        var averageResistance = target.Resistances.Values.Any() ? target.Resistances.Values.Average() : 0;
        return damage * (1 - averageResistance);
    }
}

/// <summary>
/// Simple 3D visual helper classes
/// </summary>
public class BoxVisual3D
{
    public Point3D Center { get; set; }
    public double Length { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

    public MeshGeometry3D Geometry
    {
        get
        {
            var mesh = new MeshGeometry3D();
            // Simplified box geometry
            return mesh;
        }
    }
}

public class SphereVisual3D
{
    public Point3D Center { get; set; }
    public double Radius { get; set; }

    public MeshGeometry3D Geometry
    {
        get
        {
            var mesh = new MeshGeometry3D();
            // Simplified sphere geometry
            return mesh;
        }
    }
}

/// <summary>
/// Event args for DPS calculations
/// </summary>
public class DpsCalculatedEventArgs : EventArgs
{
    public DpsMetrics Metrics { get; }

    public DpsCalculatedEventArgs(DpsMetrics metrics)
    {
        Metrics = metrics;
    }
}

/// <summary>
/// Event args for weapon firing
/// </summary>
public class WeaponFiredEventArgs : EventArgs
{
    public HoloWeaponData Weapon { get; }

    public WeaponFiredEventArgs(HoloWeaponData weapon)
    {
        Weapon = weapon;
    }
}

/// <summary>
/// Event args for damage application
/// </summary>
public class DamageAppliedEventArgs : EventArgs
{
    public HoloWeaponData Weapon { get; }
    public HoloTargetData Target { get; }
    public double Damage { get; }

    public DamageAppliedEventArgs(HoloWeaponData weapon, HoloTargetData target, double damage)
    {
        Weapon = weapon;
        Target = target;
        Damage = damage;
    }
}

#endregion