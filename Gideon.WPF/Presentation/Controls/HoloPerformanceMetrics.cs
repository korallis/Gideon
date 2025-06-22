// ==========================================================================
// HoloPerformanceMetrics.cs - Holographic Performance Metrics Display
// ==========================================================================
// Advanced performance metrics visualization featuring real-time DPS calculations,
// animated graphs, EVE-style combat metrics, and holographic data visualization.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Holographic performance metrics display with real-time calculations and animated visualizations
/// </summary>
public class HoloPerformanceMetrics : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloPerformanceMetrics),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloPerformanceMetrics),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty ShipFittingProperty =
        DependencyProperty.Register(nameof(ShipFitting), typeof(HoloShipFitting), typeof(HoloPerformanceMetrics),
            new PropertyMetadata(null, OnShipFittingChanged));

    public static readonly DependencyProperty MetricsDisplayModeProperty =
        DependencyProperty.Register(nameof(MetricsDisplayMode), typeof(MetricsDisplayMode), typeof(HoloPerformanceMetrics),
            new PropertyMetadata(MetricsDisplayMode.Combat, OnMetricsDisplayModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloPerformanceMetrics),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloPerformanceMetrics),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowGraphsProperty =
        DependencyProperty.Register(nameof(ShowGraphs), typeof(bool), typeof(HoloPerformanceMetrics),
            new PropertyMetadata(true, OnShowGraphsChanged));

    public static readonly DependencyProperty ShowComparisonsProperty =
        DependencyProperty.Register(nameof(ShowComparisons), typeof(bool), typeof(HoloPerformanceMetrics),
            new PropertyMetadata(false, OnShowComparisonsChanged));

    public static readonly DependencyProperty AutoUpdateProperty =
        DependencyProperty.Register(nameof(AutoUpdate), typeof(bool), typeof(HoloPerformanceMetrics),
            new PropertyMetadata(true, OnAutoUpdateChanged));

    public static readonly DependencyProperty UpdateIntervalProperty =
        DependencyProperty.Register(nameof(UpdateInterval), typeof(TimeSpan), typeof(HoloPerformanceMetrics),
            new PropertyMetadata(TimeSpan.FromMilliseconds(500), OnUpdateIntervalChanged));

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

    public HoloShipFitting ShipFitting
    {
        get => (HoloShipFitting)GetValue(ShipFittingProperty);
        set => SetValue(ShipFittingProperty, value);
    }

    public MetricsDisplayMode MetricsDisplayMode
    {
        get => (MetricsDisplayMode)GetValue(MetricsDisplayModeProperty);
        set => SetValue(MetricsDisplayModeProperty, value);
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

    public bool ShowGraphs
    {
        get => (bool)GetValue(ShowGraphsProperty);
        set => SetValue(ShowGraphsProperty, value);
    }

    public bool ShowComparisons
    {
        get => (bool)GetValue(ShowComparisonsProperty);
        set => SetValue(ShowComparisonsProperty, value);
    }

    public bool AutoUpdate
    {
        get => (bool)GetValue(AutoUpdateProperty);
        set => SetValue(AutoUpdateProperty, value);
    }

    public TimeSpan UpdateInterval
    {
        get => (TimeSpan)GetValue(UpdateIntervalProperty);
        set => SetValue(UpdateIntervalProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<MetricsUpdatedEventArgs> MetricsUpdated;
    public event EventHandler<MetricThresholdEventArgs> MetricThresholdExceeded;

    #endregion

    #region Private Fields

    private Grid _rootGrid;
    private TabControl _metricsTabControl;
    private Canvas _particleCanvas;
    private DispatcherTimer _updateTimer;
    private DispatcherTimer _animationTimer;
    private readonly List<UIElement> _particles = new();
    private readonly Dictionary<string, MetricCard> _metricCards = new();
    private readonly Dictionary<string, GraphControl> _metricGraphs = new();
    private PerformanceCalculator _calculator;
    private HoloPerformanceData _currentMetrics;
    private readonly Random _random = new();

    #endregion

    #region Constructor

    public HoloPerformanceMetrics()
    {
        InitializeComponent();
        InitializeCalculator();
        InitializeAnimationSystem();
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
        _rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Mode selector
        _rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content

        CreateModeSelector();
        CreateMetricsDisplay();
        CreateParticleSystem();

        Content = _rootGrid;
    }

    private void CreateModeSelector()
    {
        var selectorPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        CreateModeButton("Combat", MetricsDisplayMode.Combat, selectorPanel);
        CreateModeButton("Tank", MetricsDisplayMode.Tank, selectorPanel);
        CreateModeButton("Speed", MetricsDisplayMode.Speed, selectorPanel);
        CreateModeButton("Capacitor", MetricsDisplayMode.Capacitor, selectorPanel);
        CreateModeButton("Targeting", MetricsDisplayMode.Targeting, selectorPanel);
        CreateModeButton("Overview", MetricsDisplayMode.Overview, selectorPanel);

        Grid.SetRow(selectorPanel, 0);
        _rootGrid.Children.Add(selectorPanel);
    }

    private void CreateModeButton(string text, MetricsDisplayMode mode, Panel parent)
    {
        var button = new Button
        {
            Content = text,
            Margin = new Thickness(5),
            Padding = new Thickness(15, 8),
            Background = CreateHolographicBackground(mode == MetricsDisplayMode ? 0.5 : 0.2),
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            BorderBrush = GetBrushForColorScheme(EVEColorScheme),
            BorderThickness = new Thickness(1),
            Effect = CreateGlowEffect(),
            Style = CreateHolographicButtonStyle(),
            Tag = mode
        };

        button.Click += (s, e) => MetricsDisplayMode = (MetricsDisplayMode)button.Tag;
        parent.Children.Add(button);
    }

    private void CreateMetricsDisplay()
    {
        _metricsTabControl = new TabControl
        {
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Margin = new Thickness(10)
        };

        CreateCombatTab();
        CreateTankTab();
        CreateSpeedTab();
        CreateCapacitorTab();
        CreateTargetingTab();
        CreateOverviewTab();

        Grid.SetRow(_metricsTabControl, 1);
        _rootGrid.Children.Add(_metricsTabControl);
    }

    private void CreateCombatTab()
    {
        var tab = new TabItem
        {
            Header = "Combat",
            Visibility = Visibility.Collapsed // Controlled by mode selector
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Combat metrics cards
        var metricsPanel = new StackPanel { Margin = new Thickness(10) };
        CreateMetricCard("DPS", "0", "Damage Per Second", metricsPanel);
        CreateMetricCard("Alpha", "0", "Alpha Strike Damage", metricsPanel);
        CreateMetricCard("Volley", "0", "Volley Damage", metricsPanel);
        CreateMetricCard("Range", "0 km", "Optimal Range", metricsPanel);
        CreateMetricCard("Tracking", "0", "Tracking Speed", metricsPanel);

        // Combat graphs
        var graphPanel = new StackPanel { Margin = new Thickness(10) };
        if (ShowGraphs)
        {
            CreateMetricGraph("DPS Over Time", "dps_graph", graphPanel);
            CreateMetricGraph("Damage by Type", "damage_type_graph", graphPanel);
        }

        Grid.SetColumn(metricsPanel, 0);
        Grid.SetColumn(graphPanel, 1);
        grid.Children.Add(metricsPanel);
        grid.Children.Add(graphPanel);

        tab.Content = grid;
        _metricsTabControl.Items.Add(tab);
    }

    private void CreateTankTab()
    {
        var tab = new TabItem
        {
            Header = "Tank",
            Visibility = Visibility.Collapsed
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Tank metrics
        var metricsPanel = new StackPanel { Margin = new Thickness(10) };
        CreateMetricCard("Shield HP", "0", "Shield Hit Points", metricsPanel);
        CreateMetricCard("Armor HP", "0", "Armor Hit Points", metricsPanel);
        CreateMetricCard("Hull HP", "0", "Hull Hit Points", metricsPanel);
        CreateMetricCard("Shield Regen", "0 HP/s", "Shield Recharge Rate", metricsPanel);
        CreateMetricCard("Resistances", "0%", "Average Resistances", metricsPanel);

        // Tank graphs
        var graphPanel = new StackPanel { Margin = new Thickness(10) };
        if (ShowGraphs)
        {
            CreateMetricGraph("EHP by Damage Type", "ehp_graph", graphPanel);
            CreateMetricGraph("Resistance Profile", "resistance_graph", graphPanel);
        }

        Grid.SetColumn(metricsPanel, 0);
        Grid.SetColumn(graphPanel, 1);
        grid.Children.Add(metricsPanel);
        grid.Children.Add(graphPanel);

        tab.Content = grid;
        _metricsTabControl.Items.Add(tab);
    }

    private void CreateSpeedTab()
    {
        var tab = new TabItem
        {
            Header = "Speed",
            Visibility = Visibility.Collapsed
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Speed metrics
        var metricsPanel = new StackPanel { Margin = new Thickness(10) };
        CreateMetricCard("Max Velocity", "0 m/s", "Maximum Velocity", metricsPanel);
        CreateMetricCard("Acceleration", "0 m/s²", "Acceleration", metricsPanel);
        CreateMetricCard("Agility", "0 s", "Align Time", metricsPanel);
        CreateMetricCard("Warp Speed", "0 AU/s", "Warp Speed", metricsPanel);
        CreateMetricCard("Signature", "0 m", "Signature Radius", metricsPanel);

        // Speed graphs
        var graphPanel = new StackPanel { Margin = new Thickness(10) };
        if (ShowGraphs)
        {
            CreateMetricGraph("Velocity Profile", "velocity_graph", graphPanel);
            CreateMetricGraph("Agility Comparison", "agility_graph", graphPanel);
        }

        Grid.SetColumn(metricsPanel, 0);
        Grid.SetColumn(graphPanel, 1);
        grid.Children.Add(metricsPanel);
        grid.Children.Add(graphPanel);

        tab.Content = grid;
        _metricsTabControl.Items.Add(tab);
    }

    private void CreateCapacitorTab()
    {
        var tab = new TabItem
        {
            Header = "Capacitor",
            Visibility = Visibility.Collapsed
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Capacitor metrics
        var metricsPanel = new StackPanel { Margin = new Thickness(10) };
        CreateMetricCard("Capacitor", "0 GJ", "Total Capacitor", metricsPanel);
        CreateMetricCard("Cap Stable", "∞", "Stable Percentage", metricsPanel);
        CreateMetricCard("Recharge Time", "0 s", "Full Recharge Time", metricsPanel);
        CreateMetricCard("Usage", "0 GJ/s", "Capacitor Usage", metricsPanel);
        CreateMetricCard("Peak Regen", "0 GJ/s", "Peak Recharge Rate", metricsPanel);

        // Capacitor graphs
        var graphPanel = new StackPanel { Margin = new Thickness(10) };
        if (ShowGraphs)
        {
            CreateMetricGraph("Capacitor Simulation", "cap_sim_graph", graphPanel);
            CreateMetricGraph("Module Usage", "cap_usage_graph", graphPanel);
        }

        Grid.SetColumn(metricsPanel, 0);
        Grid.SetColumn(graphPanel, 1);
        grid.Children.Add(metricsPanel);
        grid.Children.Add(graphPanel);

        tab.Content = grid;
        _metricsTabControl.Items.Add(tab);
    }

    private void CreateTargetingTab()
    {
        var tab = new TabItem
        {
            Header = "Targeting",
            Visibility = Visibility.Collapsed
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Targeting metrics
        var metricsPanel = new StackPanel { Margin = new Thickness(10) };
        CreateMetricCard("Lock Range", "0 km", "Maximum Lock Range", metricsPanel);
        CreateMetricCard("Lock Time", "0 s", "Target Lock Time", metricsPanel);
        CreateMetricCard("Targets", "0", "Maximum Targets", metricsPanel);
        CreateMetricCard("Scan Res", "0 mm", "Scan Resolution", metricsPanel);
        CreateMetricCard("Sensor Str", "0", "Sensor Strength", metricsPanel);

        // Targeting graphs
        var graphPanel = new StackPanel { Margin = new Thickness(10) };
        if (ShowGraphs)
        {
            CreateMetricGraph("Lock Time vs Target", "lock_time_graph", graphPanel);
            CreateMetricGraph("Sensor Comparison", "sensor_graph", graphPanel);
        }

        Grid.SetColumn(metricsPanel, 0);
        Grid.SetColumn(graphPanel, 1);
        grid.Children.Add(metricsPanel);
        grid.Children.Add(graphPanel);

        tab.Content = grid;
        _metricsTabControl.Items.Add(tab);
    }

    private void CreateOverviewTab()
    {
        var tab = new TabItem
        {
            Header = "Overview",
            Visibility = Visibility.Collapsed
        };

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        // Key metrics summary
        var summaryPanel = new WrapPanel { Margin = new Thickness(10) };
        CreateMetricCard("Combat Index", "0", "Overall Combat Rating", summaryPanel);
        CreateMetricCard("Tank Index", "0", "Overall Tank Rating", summaryPanel);
        CreateMetricCard("Speed Index", "0", "Overall Speed Rating", summaryPanel);
        CreateMetricCard("Cap Stability", "0%", "Capacitor Stability", summaryPanel);
        CreateMetricCard("Fitting Usage", "0%", "CPU/PG Usage", summaryPanel);

        // Radar chart
        var radarPanel = new Border
        {
            Background = CreateHolographicBackground(0.1),
            BorderBrush = GetBrushForColorScheme(EVEColorScheme),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(10),
            Effect = CreateGlowEffect()
        };

        if (ShowGraphs)
        {
            var radarChart = CreateRadarChart();
            radarPanel.Child = radarChart;
        }

        Grid.SetRow(summaryPanel, 0);
        Grid.SetRow(radarPanel, 1);
        grid.Children.Add(summaryPanel);
        grid.Children.Add(radarPanel);

        tab.Content = grid;
        _metricsTabControl.Items.Add(tab);
    }

    private void CreateMetricCard(string title, string value, string description, Panel parent)
    {
        var card = new MetricCard
        {
            Title = title,
            Value = value,
            Description = description,
            Margin = new Thickness(5),
            Background = CreateHolographicBackground(0.2),
            BorderBrush = GetBrushForColorScheme(EVEColorScheme),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Effect = CreateGlowEffect()
        };

        _metricCards[title.ToLowerInvariant()] = card;
        parent.Children.Add(card);
    }

    private void CreateMetricGraph(string title, string id, Panel parent)
    {
        var graph = new GraphControl
        {
            Title = title,
            Width = 300,
            Height = 200,
            Margin = new Thickness(5),
            Background = CreateHolographicBackground(0.1),
            BorderBrush = GetBrushForColorScheme(EVEColorScheme),
            BorderThickness = new Thickness(1),
            Effect = CreateGlowEffect()
        };

        _metricGraphs[id] = graph;
        parent.Children.Add(graph);
    }

    private Canvas CreateRadarChart()
    {
        var canvas = new Canvas
        {
            Width = 300,
            Height = 300
        };

        // Draw radar chart background
        var centerX = 150;
        var centerY = 150;
        var maxRadius = 120;

        // Concentric circles
        for (int i = 1; i <= 5; i++)
        {
            var radius = (maxRadius / 5) * i;
            var circle = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Stroke = GetBrushForColorScheme(EVEColorScheme),
                StrokeThickness = 1,
                Opacity = 0.3
            };

            Canvas.SetLeft(circle, centerX - radius);
            Canvas.SetTop(circle, centerY - radius);
            canvas.Children.Add(circle);
        }

        // Radar axes
        var categories = new[] { "Combat", "Tank", "Speed", "Capacitor", "Targeting", "Fitting" };
        for (int i = 0; i < categories.Length; i++)
        {
            var angle = (2 * Math.PI * i) / categories.Length - Math.PI / 2;
            var x = centerX + maxRadius * Math.Cos(angle);
            var y = centerY + maxRadius * Math.Sin(angle);

            var line = new Line
            {
                X1 = centerX,
                Y1 = centerY,
                X2 = x,
                Y2 = y,
                Stroke = GetBrushForColorScheme(EVEColorScheme),
                StrokeThickness = 1,
                Opacity = 0.5
            };

            canvas.Children.Add(line);

            // Category labels
            var label = new TextBlock
            {
                Text = categories[i],
                Foreground = GetBrushForColorScheme(EVEColorScheme),
                FontSize = 10,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Canvas.SetLeft(label, x - 20);
            Canvas.SetTop(label, y - 10);
            canvas.Children.Add(label);
        }

        return canvas;
    }

    private void CreateParticleSystem()
    {
        _particleCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false
        };

        Grid.SetRowSpan(_particleCanvas, 2);
        _rootGrid.Children.Add(_particleCanvas);
    }

    private void InitializeCalculator()
    {
        _calculator = new PerformanceCalculator();
    }

    private void InitializeAnimationSystem()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        _animationTimer.Tick += AnimationTimer_Tick;

        _updateTimer = new DispatcherTimer
        {
            Interval = UpdateInterval
        };
        _updateTimer.Tick += UpdateTimer_Tick;
    }

    #endregion

    #region Performance Calculation

    private async Task UpdateMetrics()
    {
        if (ShipFitting == null) return;

        try
        {
            var metrics = await _calculator.CalculateAsync(ShipFitting);
            _currentMetrics = metrics;

            UpdateMetricDisplays(metrics);
            UpdateGraphs(metrics);

            if (EnableParticleEffects)
            {
                CreateUpdateParticles();
            }

            MetricsUpdated?.Invoke(this, new MetricsUpdatedEventArgs(metrics));

            // Check thresholds
            CheckMetricThresholds(metrics);
        }
        catch (Exception ex)
        {
            // Handle calculation errors
        }
    }

    private void UpdateMetricDisplays(HoloPerformanceData metrics)
    {
        // Update Combat metrics
        UpdateMetricCard("dps", metrics.Combat.DPS.ToString("N0"));
        UpdateMetricCard("alpha", metrics.Combat.AlphaStrike.ToString("N0"));
        UpdateMetricCard("volley", metrics.Combat.VolleyDamage.ToString("N0"));
        UpdateMetricCard("range", $"{metrics.Combat.OptimalRange:N1} km");
        UpdateMetricCard("tracking", metrics.Combat.TrackingSpeed.ToString("N2"));

        // Update Tank metrics
        UpdateMetricCard("shield hp", metrics.Tank.ShieldHP.ToString("N0"));
        UpdateMetricCard("armor hp", metrics.Tank.ArmorHP.ToString("N0"));
        UpdateMetricCard("hull hp", metrics.Tank.HullHP.ToString("N0"));
        UpdateMetricCard("shield regen", $"{metrics.Tank.ShieldRegenRate:N1} HP/s");
        UpdateMetricCard("resistances", $"{metrics.Tank.AverageResistance:P1}");

        // Update Speed metrics
        UpdateMetricCard("max velocity", $"{metrics.Speed.MaxVelocity:N0} m/s");
        UpdateMetricCard("acceleration", $"{metrics.Speed.Acceleration:N2} m/s²");
        UpdateMetricCard("agility", $"{metrics.Speed.AlignTime:N1} s");
        UpdateMetricCard("warp speed", $"{metrics.Speed.WarpSpeed:N1} AU/s");
        UpdateMetricCard("signature", $"{metrics.Speed.SignatureRadius:N0} m");

        // Update Capacitor metrics
        UpdateMetricCard("capacitor", $"{metrics.Capacitor.TotalCapacitor:N0} GJ");
        UpdateMetricCard("cap stable", metrics.Capacitor.IsStable ? $"{metrics.Capacitor.StablePercentage:P0}" : "Unstable");
        UpdateMetricCard("recharge time", $"{metrics.Capacitor.RechargeTime:N1} s");
        UpdateMetricCard("usage", $"{metrics.Capacitor.Usage:N2} GJ/s");
        UpdateMetricCard("peak regen", $"{metrics.Capacitor.PeakRegenRate:N2} GJ/s");

        // Update Targeting metrics
        UpdateMetricCard("lock range", $"{metrics.Targeting.MaxLockRange:N0} km");
        UpdateMetricCard("lock time", $"{metrics.Targeting.LockTime:N1} s");
        UpdateMetricCard("targets", metrics.Targeting.MaxTargets.ToString());
        UpdateMetricCard("scan res", $"{metrics.Targeting.ScanResolution:N0} mm");
        UpdateMetricCard("sensor str", metrics.Targeting.SensorStrength.ToString("N0"));

        // Update Overview metrics
        UpdateMetricCard("combat index", metrics.CombatIndex.ToString("N1"));
        UpdateMetricCard("tank index", metrics.TankIndex.ToString("N1"));
        UpdateMetricCard("speed index", metrics.SpeedIndex.ToString("N1"));
        UpdateMetricCard("cap stability", $"{metrics.Capacitor.StablePercentage:P0}");
        UpdateMetricCard("fitting usage", $"{metrics.FittingUsage:P0}");
    }

    private void UpdateMetricCard(string key, string value)
    {
        if (_metricCards.TryGetValue(key, out var card))
        {
            card.Value = value;

            if (EnableAnimations)
            {
                AnimateMetricUpdate(card);
            }
        }
    }

    private void UpdateGraphs(HoloPerformanceData metrics)
    {
        if (!ShowGraphs) return;

        // Update DPS graph
        if (_metricGraphs.TryGetValue("dps_graph", out var dpsGraph))
        {
            dpsGraph.AddDataPoint("DPS", metrics.Combat.DPS);
        }

        // Update other graphs...
    }

    private void CheckMetricThresholds(HoloPerformanceData metrics)
    {
        var thresholds = new List<MetricThreshold>();

        // Check various thresholds
        if (metrics.Capacitor.StablePercentage < 0.25)
        {
            thresholds.Add(new MetricThreshold
            {
                MetricName = "Capacitor Stability",
                ThresholdType = ThresholdType.Warning,
                CurrentValue = metrics.Capacitor.StablePercentage,
                ThresholdValue = 0.25,
                Message = "Low capacitor stability detected"
            });
        }

        if (metrics.FittingUsage > 0.95)
        {
            thresholds.Add(new MetricThreshold
            {
                MetricName = "Fitting Usage",
                ThresholdType = ThresholdType.Critical,
                CurrentValue = metrics.FittingUsage,
                ThresholdValue = 0.95,
                Message = "Fitting usage near maximum"
            });
        }

        if (thresholds.Any())
        {
            MetricThresholdExceeded?.Invoke(this, new MetricThresholdEventArgs(thresholds));
        }
    }

    #endregion

    #region Animation System

    private void AnimationTimer_Tick(object sender, EventArgs e)
    {
        UpdateParticles();
        UpdateAnimations();
    }

    private async void UpdateTimer_Tick(object sender, EventArgs e)
    {
        if (AutoUpdate)
        {
            await UpdateMetrics();
        }
    }

    private void UpdateParticles()
    {
        // Update and cleanup particles
        foreach (var particle in _particles.ToList())
        {
            if (particle.Opacity <= 0.1)
            {
                _particles.Remove(particle);
                _particleCanvas.Children.Remove(particle);
            }
        }
    }

    private void UpdateAnimations()
    {
        // Update metric card animations
        foreach (var card in _metricCards.Values)
        {
            if (card.IsAnimating)
            {
                card.UpdateAnimation();
            }
        }
    }

    private void AnimateMetricUpdate(MetricCard card)
    {
        // Flash effect for updated metrics
        var flashAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.6,
            Duration = TimeSpan.FromMilliseconds(200),
            AutoReverse = true
        };

        card.BeginAnimation(OpacityProperty, flashAnimation);

        // Scale effect
        var scaleAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 1.05,
            Duration = TimeSpan.FromMilliseconds(150),
            AutoReverse = true,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        if (card.RenderTransform is not ScaleTransform)
        {
            card.RenderTransform = new ScaleTransform(1, 1);
        }

        ((ScaleTransform)card.RenderTransform).BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
        ((ScaleTransform)card.RenderTransform).BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
    }

    private void CreateUpdateParticles()
    {
        for (int i = 0; i < 5; i++)
        {
            CreateDataParticle();
        }
    }

    private void CreateDataParticle()
    {
        var particle = new Ellipse
        {
            Width = _random.NextDouble() * 4 + 2,
            Height = _random.NextDouble() * 4 + 2,
            Fill = GetBrushForColorScheme(EVEColorScheme),
            Opacity = 0.8,
            Effect = CreateGlowEffect(0.6)
        };

        var startX = _random.NextDouble() * ActualWidth;
        var startY = ActualHeight;
        var endX = _random.NextDouble() * ActualWidth;
        var endY = 0;

        Canvas.SetLeft(particle, startX);
        Canvas.SetTop(particle, startY);
        Canvas.SetZIndex(particle, 1000);

        _particles.Add(particle);
        _particleCanvas.Children.Add(particle);

        // Animate upward movement
        var moveX = new DoubleAnimation
        {
            From = startX,
            To = endX,
            Duration = TimeSpan.FromMilliseconds(2000),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        var moveY = new DoubleAnimation
        {
            From = startY,
            To = endY,
            Duration = TimeSpan.FromMilliseconds(2000),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeOut = new DoubleAnimation
        {
            From = 0.8,
            To = 0,
            BeginTime = TimeSpan.FromMilliseconds(1500),
            Duration = TimeSpan.FromMilliseconds(500)
        };

        particle.BeginAnimation(Canvas.LeftProperty, moveX);
        particle.BeginAnimation(Canvas.TopProperty, moveY);
        particle.BeginAnimation(OpacityProperty, fadeOut);

        // Remove particle after animation
        Task.Delay(2000).ContinueWith(t =>
        {
            Dispatcher.BeginInvoke(() =>
            {
                _particles.Remove(particle);
                _particleCanvas.Children.Remove(particle);
            });
        });
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

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableAnimations)
        {
            _animationTimer.Start();
        }

        if (AutoUpdate)
        {
            _updateTimer.Start();
        }

        UpdateDisplayMode();

        if (ShipFitting != null)
        {
            Task.Run(UpdateMetrics);
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        _updateTimer?.Stop();
    }

    private void UpdateDisplayMode()
    {
        // Hide all tabs first
        foreach (TabItem tab in _metricsTabControl.Items)
        {
            tab.Visibility = Visibility.Collapsed;
        }

        // Show selected tab
        var selectedTab = MetricsDisplayMode switch
        {
            MetricsDisplayMode.Combat => _metricsTabControl.Items[0] as TabItem,
            MetricsDisplayMode.Tank => _metricsTabControl.Items[1] as TabItem,
            MetricsDisplayMode.Speed => _metricsTabControl.Items[2] as TabItem,
            MetricsDisplayMode.Capacitor => _metricsTabControl.Items[3] as TabItem,
            MetricsDisplayMode.Targeting => _metricsTabControl.Items[4] as TabItem,
            MetricsDisplayMode.Overview => _metricsTabControl.Items[5] as TabItem,
            _ => _metricsTabControl.Items[0] as TabItem
        };

        if (selectedTab != null)
        {
            selectedTab.Visibility = Visibility.Visible;
            _metricsTabControl.SelectedItem = selectedTab;
        }

        // Update mode buttons
        UpdateModeButtons();
    }

    private void UpdateModeButtons()
    {
        var selectorPanel = (StackPanel)_rootGrid.Children[0];
        foreach (Button button in selectorPanel.Children.OfType<Button>())
        {
            var isActive = (MetricsDisplayMode)button.Tag == MetricsDisplayMode;
            button.Background = CreateHolographicBackground(isActive ? 0.5 : 0.2);
            button.Effect = isActive ? CreateIntenseGlowEffect() : CreateGlowEffect();
        }
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

    #endregion

    #region Dependency Property Callbacks

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Update holographic effects intensity
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPerformanceMetrics metrics)
        {
            // Update all UI elements with new color scheme
        }
    }

    private static void OnShipFittingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPerformanceMetrics metrics && metrics.AutoUpdate)
        {
            Task.Run(metrics.UpdateMetrics);
        }
    }

    private static void OnMetricsDisplayModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPerformanceMetrics metrics)
        {
            metrics.UpdateDisplayMode();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPerformanceMetrics metrics)
        {
            if ((bool)e.NewValue)
                metrics._animationTimer.Start();
            else
                metrics._animationTimer.Stop();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPerformanceMetrics metrics && !(bool)e.NewValue)
        {
            foreach (var particle in metrics._particles.ToList())
            {
                metrics._particles.Remove(particle);
                metrics._particleCanvas.Children.Remove(particle);
            }
        }
    }

    private static void OnShowGraphsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Update graph visibility
    }

    private static void OnShowComparisonsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Update comparison display
    }

    private static void OnAutoUpdateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPerformanceMetrics metrics)
        {
            if ((bool)e.NewValue)
                metrics._updateTimer.Start();
            else
                metrics._updateTimer.Stop();
        }
    }

    private static void OnUpdateIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPerformanceMetrics metrics)
        {
            metrics._updateTimer.Interval = (TimeSpan)e.NewValue;
        }
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void SetSimplifiedMode(bool enabled)
    {
        EnableAnimations = !enabled;
        EnableParticleEffects = !enabled;
        ShowGraphs = !enabled;
        
        if (enabled)
        {
            _animationTimer?.Stop();
            foreach (var particle in _particles.ToList())
            {
                _particles.Remove(particle);
                _particleCanvas.Children.Remove(particle);
            }
        }
        else
        {
            _animationTimer?.Start();
        }
    }

    public bool IsSimplifiedModeActive => !EnableAnimations || !EnableParticleEffects || !ShowGraphs;

    #endregion

    #region Public Methods

    /// <summary>
    /// Forces immediate metrics update
    /// </summary>
    public async Task RefreshMetrics()
    {
        await UpdateMetrics();
    }

    /// <summary>
    /// Gets current performance metrics
    /// </summary>
    public HoloPerformanceData GetCurrentMetrics()
    {
        return _currentMetrics;
    }

    /// <summary>
    /// Exports metrics data to specified format
    /// </summary>
    public string ExportMetrics(MetricsExportFormat format)
    {
        if (_currentMetrics == null) return string.Empty;

        return format switch
        {
            MetricsExportFormat.Json => _currentMetrics.ToJson(),
            MetricsExportFormat.Csv => _currentMetrics.ToCsv(),
            MetricsExportFormat.Text => _currentMetrics.ToText(),
            _ => _currentMetrics.ToString()
        };
    }

    #endregion
}

#region Supporting Types

/// <summary>
/// Metrics display modes
/// </summary>
public enum MetricsDisplayMode
{
    Combat,
    Tank,
    Speed,
    Capacitor,
    Targeting,
    Overview
}

/// <summary>
/// Metrics export formats
/// </summary>
public enum MetricsExportFormat
{
    Json,
    Csv,
    Text
}

/// <summary>
/// Threshold types for metric monitoring
/// </summary>
public enum ThresholdType
{
    Info,
    Warning,
    Critical
}

/// <summary>
/// Metric threshold definition
/// </summary>
public class MetricThreshold
{
    public string MetricName { get; set; } = string.Empty;
    public ThresholdType ThresholdType { get; set; }
    public double CurrentValue { get; set; }
    public double ThresholdValue { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Performance metrics data container
/// </summary>
public class HoloPerformanceData
{
    public CombatMetrics Combat { get; set; } = new();
    public TankMetrics Tank { get; set; } = new();
    public SpeedMetrics Speed { get; set; } = new();
    public CapacitorMetrics Capacitor { get; set; } = new();
    public TargetingMetrics Targeting { get; set; } = new();
    public double CombatIndex { get; set; }
    public double TankIndex { get; set; }
    public double SpeedIndex { get; set; }
    public double FittingUsage { get; set; }
    public DateTime CalculatedAt { get; set; }

    public string ToJson() => System.Text.Json.JsonSerializer.Serialize(this);
    public string ToCsv() => $"{Combat.DPS},{Tank.ShieldHP},{Speed.MaxVelocity},{Capacitor.TotalCapacitor}";
    public string ToText() => $"DPS: {Combat.DPS:N0}, Shield: {Tank.ShieldHP:N0}, Speed: {Speed.MaxVelocity:N0}";
}

/// <summary>
/// Combat performance metrics
/// </summary>
public class CombatMetrics
{
    public double DPS { get; set; }
    public double AlphaStrike { get; set; }
    public double VolleyDamage { get; set; }
    public double OptimalRange { get; set; }
    public double TrackingSpeed { get; set; }
}

/// <summary>
/// Tank performance metrics
/// </summary>
public class TankMetrics
{
    public double ShieldHP { get; set; }
    public double ArmorHP { get; set; }
    public double HullHP { get; set; }
    public double ShieldRegenRate { get; set; }
    public double AverageResistance { get; set; }
}

/// <summary>
/// Speed performance metrics
/// </summary>
public class SpeedMetrics
{
    public double MaxVelocity { get; set; }
    public double Acceleration { get; set; }
    public double AlignTime { get; set; }
    public double WarpSpeed { get; set; }
    public double SignatureRadius { get; set; }
}

/// <summary>
/// Capacitor performance metrics
/// </summary>
public class CapacitorMetrics
{
    public double TotalCapacitor { get; set; }
    public bool IsStable { get; set; }
    public double StablePercentage { get; set; }
    public double RechargeTime { get; set; }
    public double Usage { get; set; }
    public double PeakRegenRate { get; set; }
}

/// <summary>
/// Targeting performance metrics
/// </summary>
public class TargetingMetrics
{
    public double MaxLockRange { get; set; }
    public double LockTime { get; set; }
    public int MaxTargets { get; set; }
    public double ScanResolution { get; set; }
    public double SensorStrength { get; set; }
}

/// <summary>
/// Ship fitting data for calculations
/// </summary>
public class HoloShipFitting
{
    public HoloShipData Ship { get; set; }
    public List<HoloModuleData> Modules { get; set; } = new();
    public List<HoloModuleData> Rigs { get; set; } = new();
    public Dictionary<string, object> Attributes { get; set; } = new();
}

/// <summary>
/// Performance calculator engine
/// </summary>
public class PerformanceCalculator
{
    public async Task<HoloPerformanceData> CalculateAsync(HoloShipFitting fitting)
    {
        // Simulate calculation time
        await Task.Delay(100);

        var data = new HoloPerformanceData
        {
            CalculatedAt = DateTime.UtcNow
        };

        // Calculate combat metrics
        data.Combat = CalculateCombatMetrics(fitting);
        
        // Calculate tank metrics
        data.Tank = CalculateTankMetrics(fitting);
        
        // Calculate speed metrics
        data.Speed = CalculateSpeedMetrics(fitting);
        
        // Calculate capacitor metrics
        data.Capacitor = CalculateCapacitorMetrics(fitting);
        
        // Calculate targeting metrics
        data.Targeting = CalculateTargetingMetrics(fitting);

        // Calculate indices
        data.CombatIndex = CalculateCombatIndex(data.Combat);
        data.TankIndex = CalculateTankIndex(data.Tank);
        data.SpeedIndex = CalculateSpeedIndex(data.Speed);
        data.FittingUsage = CalculateFittingUsage(fitting);

        return data;
    }

    private CombatMetrics CalculateCombatMetrics(HoloShipFitting fitting)
    {
        // Simplified combat calculations - real implementation would be more complex
        var random = new Random();
        return new CombatMetrics
        {
            DPS = random.Next(100, 1000),
            AlphaStrike = random.Next(500, 5000),
            VolleyDamage = random.Next(300, 3000),
            OptimalRange = random.Next(5, 50),
            TrackingSpeed = random.NextDouble() * 2
        };
    }

    private TankMetrics CalculateTankMetrics(HoloShipFitting fitting)
    {
        var random = new Random();
        return new TankMetrics
        {
            ShieldHP = random.Next(1000, 10000),
            ArmorHP = random.Next(1000, 10000),
            HullHP = random.Next(500, 5000),
            ShieldRegenRate = random.Next(10, 100),
            AverageResistance = random.NextDouble() * 0.8
        };
    }

    private SpeedMetrics CalculateSpeedMetrics(HoloShipFitting fitting)
    {
        var random = new Random();
        return new SpeedMetrics
        {
            MaxVelocity = random.Next(100, 500),
            Acceleration = random.NextDouble() * 5,
            AlignTime = random.NextDouble() * 20 + 5,
            WarpSpeed = random.NextDouble() * 5 + 1,
            SignatureRadius = random.Next(50, 400)
        };
    }

    private CapacitorMetrics CalculateCapacitorMetrics(HoloShipFitting fitting)
    {
        var random = new Random();
        var isStable = random.NextDouble() > 0.3;
        return new CapacitorMetrics
        {
            TotalCapacitor = random.Next(1000, 5000),
            IsStable = isStable,
            StablePercentage = isStable ? 1.0 : random.NextDouble(),
            RechargeTime = random.Next(200, 800),
            Usage = random.NextDouble() * 20,
            PeakRegenRate = random.NextDouble() * 30
        };
    }

    private TargetingMetrics CalculateTargetingMetrics(HoloShipFitting fitting)
    {
        var random = new Random();
        return new TargetingMetrics
        {
            MaxLockRange = random.Next(50, 200),
            LockTime = random.NextDouble() * 10 + 2,
            MaxTargets = random.Next(5, 12),
            ScanResolution = random.Next(100, 500),
            SensorStrength = random.Next(10, 30)
        };
    }

    private double CalculateCombatIndex(CombatMetrics combat)
    {
        return (combat.DPS / 100.0 + combat.AlphaStrike / 1000.0) / 2.0;
    }

    private double CalculateTankIndex(TankMetrics tank)
    {
        return ((tank.ShieldHP + tank.ArmorHP + tank.HullHP) / 10000.0 + tank.AverageResistance) / 2.0;
    }

    private double CalculateSpeedIndex(SpeedMetrics speed)
    {
        return (speed.MaxVelocity / 300.0 + (20.0 - speed.AlignTime) / 20.0) / 2.0;
    }

    private double CalculateFittingUsage(HoloShipFitting fitting)
    {
        return new Random().NextDouble() * 0.9 + 0.1; // 10-100%
    }
}

/// <summary>
/// Custom metric card control
/// </summary>
public class MetricCard : Border
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsAnimating { get; set; }

    public MetricCard()
    {
        Width = 150;
        Height = 80;
        Padding = new Thickness(10);
        
        var stackPanel = new StackPanel();
        
        var titleBlock = new TextBlock
        {
            Text = Title,
            FontWeight = FontWeights.Bold,
            FontSize = 12
        };
        
        var valueBlock = new TextBlock
        {
            Text = Value,
            FontSize = 18,
            FontWeight = FontWeights.Bold
        };
        
        var descBlock = new TextBlock
        {
            Text = Description,
            FontSize = 10,
            Opacity = 0.7
        };
        
        stackPanel.Children.Add(titleBlock);
        stackPanel.Children.Add(valueBlock);
        stackPanel.Children.Add(descBlock);
        
        Child = stackPanel;
    }

    public void UpdateAnimation()
    {
        // Animation update logic
    }
}

/// <summary>
/// Custom graph control
/// </summary>
public class GraphControl : Border
{
    public string Title { get; set; } = string.Empty;
    private readonly Dictionary<string, List<double>> _dataSeries = new();

    public void AddDataPoint(string series, double value)
    {
        if (!_dataSeries.ContainsKey(series))
        {
            _dataSeries[series] = new List<double>();
        }
        
        _dataSeries[series].Add(value);
        
        // Keep only last 50 points
        if (_dataSeries[series].Count > 50)
        {
            _dataSeries[series].RemoveAt(0);
        }
        
        InvalidateVisual();
    }
}

/// <summary>
/// Event args for metrics updates
/// </summary>
public class MetricsUpdatedEventArgs : EventArgs
{
    public HoloPerformanceData Metrics { get; }

    public MetricsUpdatedEventArgs(HoloPerformanceData metrics)
    {
        Metrics = metrics;
    }
}

/// <summary>
/// Event args for metric threshold events
/// </summary>
public class MetricThresholdEventArgs : EventArgs
{
    public List<MetricThreshold> Thresholds { get; }

    public MetricThresholdEventArgs(List<MetricThreshold> thresholds)
    {
        Thresholds = thresholds;
    }
}

#endregion