// ==========================================================================
// HoloStackingPenaltyIndicators.cs - Holographic Stacking Penalty Indicators
// ==========================================================================
// Advanced stacking penalty visualization featuring real-time module analysis,
// penalty curves, EVE-style stacking mechanics, and holographic effect reduction display.
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
/// Holographic stacking penalty indicators with real-time module analysis and penalty visualization
/// </summary>
public class HoloStackingPenaltyIndicators : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloStackingPenaltyIndicators),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloStackingPenaltyIndicators),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty ModuleGroupsProperty =
        DependencyProperty.Register(nameof(ModuleGroups), typeof(ObservableCollection<HoloModuleGroup>), typeof(HoloStackingPenaltyIndicators),
            new PropertyMetadata(null, OnModuleGroupsChanged));

    public static readonly DependencyProperty VisualizationModeProperty =
        DependencyProperty.Register(nameof(VisualizationMode), typeof(StackingVisualizationMode), typeof(HoloStackingPenaltyIndicators),
            new PropertyMetadata(StackingVisualizationMode.Overview, OnVisualizationModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloStackingPenaltyIndicators),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloStackingPenaltyIndicators),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowPenaltyCurvesProperty =
        DependencyProperty.Register(nameof(ShowPenaltyCurves), typeof(bool), typeof(HoloStackingPenaltyIndicators),
            new PropertyMetadata(true, OnShowPenaltyCurvesChanged));

    public static readonly DependencyProperty ShowEfficiencyIndicatorsProperty =
        DependencyProperty.Register(nameof(ShowEfficiencyIndicators), typeof(bool), typeof(HoloStackingPenaltyIndicators),
            new PropertyMetadata(true, OnShowEfficiencyIndicatorsChanged));

    public static readonly DependencyProperty ShowOptimalSuggestionsProperty =
        DependencyProperty.Register(nameof(ShowOptimalSuggestions), typeof(bool), typeof(HoloStackingPenaltyIndicators),
            new PropertyMetadata(true, OnShowOptimalSuggestionsChanged));

    public static readonly DependencyProperty UpdateIntervalProperty =
        DependencyProperty.Register(nameof(UpdateInterval), typeof(TimeSpan), typeof(HoloStackingPenaltyIndicators),
            new PropertyMetadata(TimeSpan.FromMilliseconds(200), OnUpdateIntervalChanged));

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

    public ObservableCollection<HoloModuleGroup> ModuleGroups
    {
        get => (ObservableCollection<HoloModuleGroup>)GetValue(ModuleGroupsProperty);
        set => SetValue(ModuleGroupsProperty, value);
    }

    public StackingVisualizationMode VisualizationMode
    {
        get => (StackingVisualizationMode)GetValue(VisualizationModeProperty);
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

    public bool ShowPenaltyCurves
    {
        get => (bool)GetValue(ShowPenaltyCurvesProperty);
        set => SetValue(ShowPenaltyCurvesProperty, value);
    }

    public bool ShowEfficiencyIndicators
    {
        get => (bool)GetValue(ShowEfficiencyIndicatorsProperty);
        set => SetValue(ShowEfficiencyIndicatorsProperty, value);
    }

    public bool ShowOptimalSuggestions
    {
        get => (bool)GetValue(ShowOptimalSuggestionsProperty);
        set => SetValue(ShowOptimalSuggestionsProperty, value);
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
    private readonly Dictionary<string, Storyboard> _penaltyAnimations;
    private readonly List<HoloPenaltyParticle> _penaltyParticles;
    private readonly HoloStackingCalculator _calculator;
    
    private Canvas _mainCanvas;
    private Canvas _particleCanvas;
    private Canvas _curveCanvas;
    private Grid _moduleGroupsGrid;
    private Grid _metricsGrid;
    private ScrollViewer _suggestionsList;

    private readonly Dictionary<string, double> _groupEfficiencies;
    private readonly Dictionary<string, double> _penaltyFactors;
    private double _overallEfficiency;
    private bool _hasStackingPenalties;
    private DateTime _lastUpdateTime;
    private readonly Random _random = new();

    // Animation state
    private double _animationProgress;
    private readonly Dictionary<string, double> _moduleAlphas;

    #endregion

    #region Constructor & Initialization

    public HoloStackingPenaltyIndicators()
    {
        _animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) }; // 60 FPS
        _updateTimer = new DispatcherTimer();
        _penaltyAnimations = new Dictionary<string, Storyboard>();
        _penaltyParticles = new List<HoloPenaltyParticle>();
        _calculator = new HoloStackingCalculator();
        _groupEfficiencies = new Dictionary<string, double>();
        _penaltyFactors = new Dictionary<string, double>();
        _moduleAlphas = new Dictionary<string, double>();

        _animationTimer.Tick += OnAnimationTimerTick;
        _updateTimer.Tick += OnUpdateTimerTick;
        _lastUpdateTime = DateTime.Now;

        InitializeComponent();
        CreateHolographicInterface();
        StartAnimations();
    }

    private void InitializeComponent()
    {
        Width = 700;
        Height = 800;
        Background = new SolidColorBrush(Color.FromArgb(25, 200, 100, 0));

        // Apply holographic effects
        Effect = new DropShadowEffect
        {
            Color = Colors.Orange,
            BlurRadius = 22,
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
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(200) });

        // Create main canvas for stacking visualization
        _mainCanvas = new Canvas
        {
            Background = new SolidColorBrush(Color.FromArgb(15, 255, 150, 0)),
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

        // Create curve canvas for penalty curves
        _curveCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false
        };
        _mainCanvas.Children.Add(_curveCanvas);

        // Create module groups display
        CreateModuleGroupsDisplay();

        // Create penalty curves
        if (ShowPenaltyCurves)
        {
            CreatePenaltyCurves();
        }

        // Create efficiency indicators
        if (ShowEfficiencyIndicators)
        {
            CreateEfficiencyIndicators();
        }

        // Create optimal suggestions
        if (ShowOptimalSuggestions)
        {
            CreateOptimalSuggestions();
        }

        // Create metrics display
        CreateMetricsDisplay();
    }

    private void CreateModuleGroupsDisplay()
    {
        var groupsContainer = new ScrollViewer
        {
            Width = 400,
            Height = 450,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)),
            BorderBrush = new SolidColorBrush(Colors.Orange),
            BorderThickness = new Thickness(1)
        };
        Canvas.SetLeft(groupsContainer, 20);
        Canvas.SetTop(groupsContainer, 20);
        _mainCanvas.Children.Add(groupsContainer);

        _moduleGroupsGrid = new Grid();
        groupsContainer.Content = _moduleGroupsGrid;

        // Add title
        var title = new TextBlock
        {
            Text = "Module Stacking Analysis",
            Foreground = new SolidColorBrush(Colors.Orange),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 10)
        };
        _moduleGroupsGrid.Children.Add(title);
    }

    private void CreatePenaltyCurves()
    {
        var curveContainer = new Border
        {
            Width = 250,
            Height = 200,
            Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5)
        };
        Canvas.SetRight(curveContainer, 20);
        Canvas.SetTop(curveContainer, 20);
        _mainCanvas.Children.Add(curveContainer);

        var curveCanvas = new Canvas
        {
            Name = "PenaltyCurveCanvas",
            Background = Brushes.Transparent
        };
        curveContainer.Child = curveCanvas;

        // Add axes
        CreateCurveAxes(curveCanvas);

        // Add title
        var title = new TextBlock
        {
            Text = "Stacking Penalty Curve",
            Foreground = new SolidColorBrush(Colors.Orange),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Canvas.SetTop(title, 5);
        Canvas.SetLeft(title, 60);
        curveCanvas.Children.Add(title);
    }

    private void CreateCurveAxes(Canvas canvas)
    {
        // X-axis
        var xAxis = new Line
        {
            X1 = 30, Y1 = 170, X2 = 220, Y2 = 170,
            Stroke = new SolidColorBrush(Colors.Gray),
            StrokeThickness = 1
        };
        canvas.Children.Add(xAxis);

        // Y-axis
        var yAxis = new Line
        {
            X1 = 30, Y1 = 30, X2 = 30, Y2 = 170,
            Stroke = new SolidColorBrush(Colors.Gray),
            StrokeThickness = 1
        };
        canvas.Children.Add(yAxis);

        // Add axis labels
        var xLabel = new TextBlock
        {
            Text = "Module Count",
            Foreground = new SolidColorBrush(Colors.Gray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 9
        };
        Canvas.SetLeft(xLabel, 110);
        Canvas.SetTop(xLabel, 180);
        canvas.Children.Add(xLabel);

        var yLabel = new TextBlock
        {
            Text = "Efficiency",
            Foreground = new SolidColorBrush(Colors.Gray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 9,
            RenderTransform = new RotateTransform(-90)
        };
        Canvas.SetLeft(yLabel, 5);
        Canvas.SetTop(yLabel, 100);
        canvas.Children.Add(yLabel);

        // Add grid lines
        for (int i = 1; i <= 7; i++)
        {
            var gridX = 30 + (i * 25);
            var gridLine = new Line
            {
                X1 = gridX, Y1 = 30, X2 = gridX, Y2 = 170,
                Stroke = new SolidColorBrush(Color.FromArgb(50, 128, 128, 128)),
                StrokeThickness = 1
            };
            canvas.Children.Add(gridLine);

            // Add number labels
            var label = new TextBlock
            {
                Text = i.ToString(),
                Foreground = new SolidColorBrush(Colors.Gray),
                FontFamily = new FontFamily("Orbitron"),
                FontSize = 8
            };
            Canvas.SetLeft(label, gridX - 3);
            Canvas.SetTop(label, 175);
            canvas.Children.Add(label);
        }

        // Y-axis grid lines
        for (int i = 1; i <= 4; i++)
        {
            var gridY = 30 + (i * 35);
            var gridLine = new Line
            {
                X1 = 30, Y1 = gridY, X2 = 220, Y2 = gridY,
                Stroke = new SolidColorBrush(Color.FromArgb(50, 128, 128, 128)),
                StrokeThickness = 1
            };
            canvas.Children.Add(gridLine);

            // Add percentage labels
            var percentage = 100 - (i * 25);
            var label = new TextBlock
            {
                Text = $"{percentage}%",
                Foreground = new SolidColorBrush(Colors.Gray),
                FontFamily = new FontFamily("Orbitron"),
                FontSize = 8
            };
            Canvas.SetLeft(label, 5);
            Canvas.SetTop(label, gridY - 5);
            canvas.Children.Add(label);
        }
    }

    private void CreateEfficiencyIndicators()
    {
        var indicatorContainer = new Border
        {
            Width = 250,
            Height = 150,
            Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5)
        };
        Canvas.SetRight(indicatorContainer, 20);
        Canvas.SetTop(indicatorContainer, 240);
        _mainCanvas.Children.Add(indicatorContainer);

        var indicatorGrid = new Grid
        {
            Name = "EfficiencyIndicatorGrid"
        };
        indicatorContainer.Child = indicatorGrid;

        // Add title
        var title = new TextBlock
        {
            Text = "Efficiency Indicators",
            Foreground = new SolidColorBrush(Colors.Orange),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 10)
        };
        indicatorGrid.Children.Add(title);
    }

    private void CreateOptimalSuggestions()
    {
        var suggestionsContainer = new Border
        {
            Width = 250,
            Height = 180,
            Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5)
        };
        Canvas.SetRight(suggestionsContainer, 20);
        Canvas.SetTop(suggestionsContainer, 410);
        _mainCanvas.Children.Add(suggestionsContainer);

        var suggestionsGrid = new Grid();
        suggestionsContainer.Child = suggestionsGrid;

        var title = new TextBlock
        {
            Text = "Optimization Suggestions",
            Foreground = new SolidColorBrush(Colors.Orange),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5)
        };
        Grid.SetRow(title, 0);
        suggestionsGrid.Children.Add(title);

        _suggestionsList = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Margin = new Thickness(10, 30, 10, 10)
        };
        suggestionsGrid.Children.Add(_suggestionsList);

        var suggestionsPanel = new StackPanel
        {
            Name = "SuggestionsPanel",
            Orientation = Orientation.Vertical
        };
        _suggestionsList.Content = suggestionsPanel;
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

        // Create metric displays
        CreateMetricDisplay("Groups", "5", Colors.Orange, 0, 0);
        CreateMetricDisplay("Penalties", "3", Colors.Red, 1, 0);
        CreateMetricDisplay("Efficiency", "87.3%", Colors.Yellow, 2, 0);
        CreateMetricDisplay("Optimal", "4/6", Colors.LimeGreen, 3, 0);

        CreateMetricDisplay("Damage", "92.1%", Colors.Red, 0, 1);
        CreateMetricDisplay("Tank", "84.5%", Colors.Cyan, 1, 1);
        CreateMetricDisplay("ECM", "100%", Colors.Purple, 2, 1);
        CreateMetricDisplay("Mining", "76.8%", Colors.Gold, 3, 1);

        CreateMetricDisplay("Worst", "Shield Ext", Colors.Red, 0, 2);
        CreateMetricDisplay("Best", "Gyros", Colors.LimeGreen, 1, 2);
        CreateMetricDisplay("Savings", "12.7%", Colors.Cyan, 2, 2);
        CreateMetricDisplay("Status", "Suboptimal", Colors.Orange, 3, 2);
    }

    private void CreateMetricDisplay(string label, string value, Color color, int column, int row)
    {
        var container = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5)
        };

        var labelText = new TextBlock
        {
            Text = label,
            Foreground = new SolidColorBrush(Colors.Gray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 9,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var valueText = new TextBlock
        {
            Text = value,
            Foreground = new SolidColorBrush(color),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 12,
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
        UpdateModuleAnimations();
        UpdatePenaltyCurveAnimation();
        UpdateEfficiencyAnimations();
    }

    private void OnUpdateTimerTick(object sender, EventArgs e)
    {
        if (ModuleGroups != null)
        {
            CalculateStackingPenalties();
            UpdateModuleGroupsDisplay();
            UpdatePenaltyCurve();
            UpdateEfficiencyIndicators();
            UpdateOptimalSuggestions();
            UpdateMetricsDisplay();
        }
    }

    private void UpdateParticleEffects()
    {
        if (!EnableParticleEffects)
            return;

        // Update existing penalty particles
        for (int i = _penaltyParticles.Count - 1; i >= 0; i--)
        {
            var particle = _penaltyParticles[i];
            particle.Update();

            if (particle.IsExpired)
            {
                _particleCanvas.Children.Remove(particle.Visual);
                _penaltyParticles.RemoveAt(i);
            }
        }

        // Create new penalty particles when there are stacking penalties
        if (_hasStackingPenalties && _random.NextDouble() < 0.2)
        {
            CreatePenaltyParticle();
        }
    }

    private void UpdateModuleAnimations()
    {
        _animationProgress += 0.01;
        if (_animationProgress > 1.0)
            _animationProgress = 0.0;

        // Update module group alpha oscillations based on penalties
        foreach (var group in _groupEfficiencies.Keys.ToList())
        {
            var efficiency = _groupEfficiencies[group];
            var baseAlpha = Math.Max(0.3, efficiency);
            var oscillation = Math.Sin(_animationProgress * 4 * Math.PI) * 0.15;
            _moduleAlphas[group] = Math.Max(0.2, baseAlpha + oscillation);
        }
    }

    private void UpdatePenaltyCurveAnimation()
    {
        if (!ShowPenaltyCurves)
            return;

        // Animate penalty curve updates
        var curveCanvas = FindChildByName<Canvas>("PenaltyCurveCanvas");
        if (curveCanvas != null)
        {
            // Update curve opacity based on penalty severity
            var maxPenalty = _penaltyFactors.Values.DefaultIfEmpty(0).Max();
            var curveOpacity = 0.5 + (maxPenalty * 0.5);
            curveCanvas.Opacity = curveOpacity;
        }
    }

    private void UpdateEfficiencyAnimations()
    {
        // Animate efficiency indicator colors based on values
        foreach (var kvp in _groupEfficiencies)
        {
            var efficiency = kvp.Value;
            var color = GetEfficiencyColor(efficiency);
            
            // Find and update efficiency indicators
            // Implementation would update visual elements
        }
    }

    private void CreatePenaltyParticle()
    {
        var particle = new HoloPenaltyParticle
        {
            Position = new Point(_random.NextDouble() * _mainCanvas.ActualWidth, 
                               _random.NextDouble() * _mainCanvas.ActualHeight),
            Velocity = new Vector(_random.NextDouble() * 3 - 1.5, _random.NextDouble() * 3 - 1.5),
            Color = GetRandomPenaltyColor(),
            Size = _random.NextDouble() * 2.5 + 1,
            Life = TimeSpan.FromSeconds(_random.NextDouble() * 3 + 1)
        };

        particle.CreateVisual();
        _particleCanvas.Children.Add(particle.Visual);
        _penaltyParticles.Add(particle);
    }

    #endregion

    #region Calculation Methods

    private void CalculateStackingPenalties()
    {
        if (ModuleGroups == null)
            return;

        var now = DateTime.Now;
        var deltaTime = (now - _lastUpdateTime).TotalSeconds;
        _lastUpdateTime = now;

        _groupEfficiencies.Clear();
        _penaltyFactors.Clear();

        double totalEfficiency = 0;
        int groupCount = 0;

        foreach (var group in ModuleGroups)
        {
            var stackingData = _calculator.CalculateStackingPenalty(group);
            
            _groupEfficiencies[group.GroupName] = stackingData.TotalEfficiency;
            _penaltyFactors[group.GroupName] = stackingData.PenaltyFactor;
            
            totalEfficiency += stackingData.TotalEfficiency;
            groupCount++;
        }

        _overallEfficiency = groupCount > 0 ? totalEfficiency / groupCount : 1.0;
        _hasStackingPenalties = _penaltyFactors.Values.Any(p => p > 0.01);
    }

    private void UpdateModuleGroupsDisplay()
    {
        if (ModuleGroups == null)
            return;

        // Clear existing content (keep title)
        _moduleGroupsGrid.Children.Clear();
        _moduleGroupsGrid.RowDefinitions.Clear();

        // Add title
        _moduleGroupsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
        var title = new TextBlock
        {
            Text = "Module Stacking Analysis",
            Foreground = new SolidColorBrush(Colors.Orange),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(title, 0);
        _moduleGroupsGrid.Children.Add(title);

        // Add module groups
        int rowIndex = 1;
        foreach (var group in ModuleGroups)
        {
            _moduleGroupsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80) });
            
            var groupDisplay = CreateModuleGroupDisplay(group);
            Grid.SetRow(groupDisplay, rowIndex);
            _moduleGroupsGrid.Children.Add(groupDisplay);
            
            rowIndex++;
        }
    }

    private FrameworkElement CreateModuleGroupDisplay(HoloModuleGroup group)
    {
        var container = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(60, 100, 100, 100)),
            BorderBrush = new SolidColorBrush(GetEfficiencyColor(_groupEfficiencies.GetValueOrDefault(group.GroupName, 1.0))),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(10, 5),
            Padding = new Thickness(10)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Group info
        var infoPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var nameText = new TextBlock
        {
            Text = group.GroupName,
            Foreground = new SolidColorBrush(Colors.White),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 14,
            FontWeight = FontWeights.Bold
        };
        infoPanel.Children.Add(nameText);

        var moduleCountText = new TextBlock
        {
            Text = $"{group.Modules.Count} modules, {group.StackingAttribute}",
            Foreground = new SolidColorBrush(Colors.Gray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 10
        };
        infoPanel.Children.Add(moduleCountText);

        var moduleListText = new TextBlock
        {
            Text = string.Join(", ", group.Modules.Select(m => m.Name)),
            Foreground = new SolidColorBrush(Colors.LightGray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 9,
            TextWrapping = TextWrapping.Wrap,
            MaxHeight = 30
        };
        infoPanel.Children.Add(moduleListText);

        Grid.SetColumn(infoPanel, 0);
        grid.Children.Add(infoPanel);

        // Efficiency display
        var efficiency = _groupEfficiencies.GetValueOrDefault(group.GroupName, 1.0);
        var efficiencyPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var efficiencyText = new TextBlock
        {
            Text = $"{efficiency * 100:F1}%",
            Foreground = new SolidColorBrush(GetEfficiencyColor(efficiency)),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        efficiencyPanel.Children.Add(efficiencyText);

        var efficiencyLabel = new TextBlock
        {
            Text = "Efficiency",
            Foreground = new SolidColorBrush(Colors.Gray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 9,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        efficiencyPanel.Children.Add(efficiencyLabel);

        Grid.SetColumn(efficiencyPanel, 1);
        grid.Children.Add(efficiencyPanel);

        // Penalty factor display
        var penaltyFactor = _penaltyFactors.GetValueOrDefault(group.GroupName, 0.0);
        var penaltyPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var penaltyText = new TextBlock
        {
            Text = $"-{penaltyFactor * 100:F1}%",
            Foreground = new SolidColorBrush(GetPenaltyColor(penaltyFactor)),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        penaltyPanel.Children.Add(penaltyText);

        var penaltyLabel = new TextBlock
        {
            Text = "Penalty",
            Foreground = new SolidColorBrush(Colors.Gray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 9,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        penaltyPanel.Children.Add(penaltyLabel);

        Grid.SetColumn(penaltyPanel, 2);
        grid.Children.Add(penaltyPanel);

        // Add alpha animation based on efficiency
        if (_moduleAlphas.TryGetValue(group.GroupName, out var alpha))
        {
            container.Opacity = alpha;
        }

        container.Child = grid;
        return container;
    }

    private void UpdatePenaltyCurve()
    {
        if (!ShowPenaltyCurves)
            return;

        var curveCanvas = FindChildByName<Canvas>("PenaltyCurveCanvas");
        if (curveCanvas == null)
            return;

        // Clear existing curve
        var existingCurve = curveCanvas.Children.OfType<Path>().FirstOrDefault();
        if (existingCurve != null)
            curveCanvas.Children.Remove(existingCurve);

        // Create stacking penalty curve
        var curvePath = new Path
        {
            Stroke = new SolidColorBrush(Colors.Orange),
            StrokeThickness = 2,
            Data = CreateStackingCurveGeometry(),
            Effect = new DropShadowEffect
            {
                Color = Colors.Orange,
                BlurRadius = 3,
                ShadowDepth = 0,
                Opacity = 0.6
            }
        };
        
        curveCanvas.Children.Add(curvePath);

        // Add current module count indicators
        if (ModuleGroups != null)
        {
            foreach (var group in ModuleGroups)
            {
                if (group.Modules.Count > 1)
                {
                    var moduleCount = group.Modules.Count;
                    var efficiency = _groupEfficiencies.GetValueOrDefault(group.GroupName, 1.0);
                    
                    var indicator = new Ellipse
                    {
                        Width = 6,
                        Height = 6,
                        Fill = new SolidColorBrush(GetEfficiencyColor(efficiency))
                    };
                    
                    var x = 30 + (moduleCount * 25);
                    var y = 170 - (efficiency * 140);
                    
                    Canvas.SetLeft(indicator, x - 3);
                    Canvas.SetTop(indicator, y - 3);
                    curveCanvas.Children.Add(indicator);
                }
            }
        }
    }

    private void UpdateEfficiencyIndicators()
    {
        if (!ShowEfficiencyIndicators)
            return;

        var indicatorGrid = FindChildByName<Grid>("EfficiencyIndicatorGrid");
        if (indicatorGrid == null)
            return;

        // Clear existing indicators (keep title)
        indicatorGrid.Children.Clear();
        indicatorGrid.RowDefinitions.Clear();

        // Add title
        indicatorGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
        var title = new TextBlock
        {
            Text = "Efficiency Indicators",
            Foreground = new SolidColorBrush(Colors.Orange),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(title, 0);
        indicatorGrid.Children.Add(title);

        // Add efficiency bars
        if (ModuleGroups != null)
        {
            int rowIndex = 1;
            foreach (var group in ModuleGroups.Take(4)) // Show top 4 groups
            {
                indicatorGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(25) });
                
                var efficiency = _groupEfficiencies.GetValueOrDefault(group.GroupName, 1.0);
                var efficiencyBar = CreateEfficiencyBar(group.GroupName, efficiency);
                
                Grid.SetRow(efficiencyBar, rowIndex);
                indicatorGrid.Children.Add(efficiencyBar);
                
                rowIndex++;
            }
        }
    }

    private FrameworkElement CreateEfficiencyBar(string groupName, double efficiency)
    {
        var container = new Grid
        {
            Margin = new Thickness(10, 2)
        };

        container.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        container.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        container.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });

        // Group name
        var nameText = new TextBlock
        {
            Text = groupName.Length > 10 ? groupName.Substring(0, 10) + "..." : groupName,
            Foreground = new SolidColorBrush(Colors.White),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 9,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(nameText, 0);
        container.Children.Add(nameText);

        // Efficiency bar
        var barContainer = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(50, 128, 128, 128)),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(2),
            Height = 12,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 0)
        };
        Grid.SetColumn(barContainer, 1);
        container.Children.Add(barContainer);

        var efficiencyFill = new Rectangle
        {
            Fill = new SolidColorBrush(GetEfficiencyColor(efficiency)),
            Width = (barContainer.Width - 2) * efficiency,
            Height = 10,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        barContainer.Child = efficiencyFill;

        // Percentage
        var percentageText = new TextBlock
        {
            Text = $"{efficiency * 100:F0}%",
            Foreground = new SolidColorBrush(GetEfficiencyColor(efficiency)),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 9,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetColumn(percentageText, 2);
        container.Children.Add(percentageText);

        return container;
    }

    private void UpdateOptimalSuggestions()
    {
        if (!ShowOptimalSuggestions)
            return;

        var suggestionsPanel = FindChildByName<StackPanel>("SuggestionsPanel");
        if (suggestionsPanel == null)
            return;

        // Clear existing suggestions
        suggestionsPanel.Children.Clear();

        // Generate optimization suggestions
        var suggestions = _calculator.GenerateOptimizationSuggestions(ModuleGroups, _groupEfficiencies, _penaltyFactors);

        foreach (var suggestion in suggestions.Take(5)) // Show top 5 suggestions
        {
            var suggestionDisplay = CreateSuggestionDisplay(suggestion);
            suggestionsPanel.Children.Add(suggestionDisplay);
        }
    }

    private FrameworkElement CreateSuggestionDisplay(HoloOptimizationSuggestion suggestion)
    {
        var container = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 100, 100, 100)),
            BorderBrush = new SolidColorBrush(GetSuggestionColor(suggestion.ImpactLevel)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3),
            Margin = new Thickness(2),
            Padding = new Thickness(5)
        };

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var titleText = new TextBlock
        {
            Text = suggestion.Title,
            Foreground = new SolidColorBrush(GetSuggestionColor(suggestion.ImpactLevel)),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 10,
            FontWeight = FontWeights.Bold
        };
        stackPanel.Children.Add(titleText);

        var descriptionText = new TextBlock
        {
            Text = suggestion.Description,
            Foreground = new SolidColorBrush(Colors.LightGray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 8,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 2)
        };
        stackPanel.Children.Add(descriptionText);

        var impactText = new TextBlock
        {
            Text = $"Impact: +{suggestion.EfficiencyGain * 100:F1}%",
            Foreground = new SolidColorBrush(Colors.LimeGreen),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 8,
            FontWeight = FontWeights.Bold
        };
        stackPanel.Children.Add(impactText);

        container.Child = stackPanel;
        return container;
    }

    private void UpdateMetricsDisplay()
    {
        if (ModuleGroups == null)
            return;

        var groupCount = ModuleGroups.Count;
        var penaltyCount = _penaltyFactors.Count(kvp => kvp.Value > 0.01);
        var worstGroup = _groupEfficiencies.OrderBy(kvp => kvp.Value).FirstOrDefault();
        var bestGroup = _groupEfficiencies.OrderByDescending(kvp => kvp.Value).FirstOrDefault();

        UpdateMetricValue("Groups", groupCount.ToString());
        UpdateMetricValue("Penalties", penaltyCount.ToString());
        UpdateMetricValue("Efficiency", $"{_overallEfficiency * 100:F1}%");

        var optimalCount = _groupEfficiencies.Count(kvp => kvp.Value > 0.9);
        UpdateMetricValue("Optimal", $"{optimalCount}/{groupCount}");

        // Update category efficiencies
        var damageEff = GetCategoryEfficiency("Damage");
        var tankEff = GetCategoryEfficiency("Tank");
        var ecmEff = GetCategoryEfficiency("ECM");
        var miningEff = GetCategoryEfficiency("Mining");

        UpdateMetricValue("Damage", $"{damageEff * 100:F1}%");
        UpdateMetricValue("Tank", $"{tankEff * 100:F1}%");
        UpdateMetricValue("ECM", $"{ecmEff * 100:F1}%");
        UpdateMetricValue("Mining", $"{miningEff * 100:F1}%");

        UpdateMetricValue("Worst", worstGroup.Key?.Length > 8 ? worstGroup.Key.Substring(0, 8) : worstGroup.Key ?? "None");
        UpdateMetricValue("Best", bestGroup.Key?.Length > 8 ? bestGroup.Key.Substring(0, 8) : bestGroup.Key ?? "None");

        var potentialSavings = (1.0 - _overallEfficiency) * 100;
        UpdateMetricValue("Savings", $"{potentialSavings:F1}%");
        UpdateMetricValue("Status", _hasStackingPenalties ? "Suboptimal" : "Optimal");

        // Update metric colors
        UpdateMetricColor("Efficiency", GetEfficiencyColor(_overallEfficiency));
        UpdateMetricColor("Status", _hasStackingPenalties ? Colors.Orange : Colors.LimeGreen);
        UpdateMetricColor("Penalties", penaltyCount > 0 ? Colors.Red : Colors.LimeGreen);
    }

    #endregion

    #region Helper Methods

    private Color GetEfficiencyColor(double efficiency)
    {
        if (efficiency >= 0.95) return Colors.LimeGreen;
        if (efficiency >= 0.85) return Colors.Yellow;
        if (efficiency >= 0.70) return Colors.Orange;
        return Colors.Red;
    }

    private Color GetPenaltyColor(double penalty)
    {
        if (penalty >= 0.30) return Colors.Red;
        if (penalty >= 0.15) return Colors.Orange;
        if (penalty >= 0.05) return Colors.Yellow;
        return Colors.LimeGreen;
    }

    private Color GetSuggestionColor(SuggestionImpactLevel impact)
    {
        return impact switch
        {
            SuggestionImpactLevel.High => Colors.Red,
            SuggestionImpactLevel.Medium => Colors.Orange,
            SuggestionImpactLevel.Low => Colors.Yellow,
            _ => Colors.Gray
        };
    }

    private Color GetRandomPenaltyColor()
    {
        var colors = new[] { Colors.Orange, Colors.Red, Colors.Yellow };
        return colors[_random.Next(colors.Length)];
    }

    private double GetCategoryEfficiency(string category)
    {
        if (ModuleGroups == null)
            return 1.0;

        var categoryGroups = ModuleGroups.Where(g => 
            g.Modules.Any(m => m.Category?.Equals(category, StringComparison.OrdinalIgnoreCase) == true));

        if (!categoryGroups.Any())
            return 1.0;

        return categoryGroups.Average(g => _groupEfficiencies.GetValueOrDefault(g.GroupName, 1.0));
    }

    private PathGeometry CreateStackingCurveGeometry()
    {
        var geometry = new PathGeometry();
        var figure = new PathFigure();

        // EVE Online stacking penalty curve: effectiveness = 1.0, 0.869, 0.571, 0.283, 0.106, 0.034, 0.009
        var stackingFactors = new[] { 1.0, 0.869, 0.571, 0.283, 0.106, 0.034, 0.009 };
        
        var startPoint = new Point(30, 170 - (stackingFactors[0] * 140)); // Start at 100%
        figure.StartPoint = startPoint;

        for (int i = 1; i < stackingFactors.Length; i++)
        {
            var x = 30 + (i * 25);
            var y = 170 - (stackingFactors[i] * 140);
            figure.Segments.Add(new LineSegment(new Point(x, y), true));
        }

        geometry.Figures.Add(figure);
        return geometry;
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
    /// Gets the current stacking penalty analysis
    /// </summary>
    public StackingPenaltyAnalysis GetStackingAnalysis()
    {
        return new StackingPenaltyAnalysis
        {
            OverallEfficiency = _overallEfficiency,
            HasStackingPenalties = _hasStackingPenalties,
            GroupEfficiencies = new Dictionary<string, double>(_groupEfficiencies),
            PenaltyFactors = new Dictionary<string, double>(_penaltyFactors),
            TotalGroups = ModuleGroups?.Count ?? 0,
            GroupsWithPenalties = _penaltyFactors.Count(kvp => kvp.Value > 0.01),
            PotentialSavings = (1.0 - _overallEfficiency) * 100
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
            ShowPenaltyCurves = false;
            _animationTimer.Interval = TimeSpan.FromMilliseconds(33); // 30 FPS
        }
        else
        {
            EnableParticleEffects = true;
            ShowPenaltyCurves = true;
            _animationTimer.Interval = TimeSpan.FromMilliseconds(16); // 60 FPS
        }
    }

    #endregion

    #region Event Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloStackingPenaltyIndicators control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloStackingPenaltyIndicators control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnModuleGroupsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloStackingPenaltyIndicators control)
        {
            control.UpdateModuleGroups();
        }
    }

    private static void OnVisualizationModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloStackingPenaltyIndicators control)
        {
            control.UpdateVisualizationMode();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloStackingPenaltyIndicators control)
        {
            if ((bool)e.NewValue)
                control.StartAnimations();
            else
                control.StopAnimations();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloStackingPenaltyIndicators control)
        {
            control.UpdateParticleSettings();
        }
    }

    private static void OnShowPenaltyCurvesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloStackingPenaltyIndicators control)
        {
            control.UpdatePenaltyCurveDisplay();
        }
    }

    private static void OnShowEfficiencyIndicatorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloStackingPenaltyIndicators control)
        {
            control.UpdateEfficiencyIndicatorDisplay();
        }
    }

    private static void OnShowOptimalSuggestionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloStackingPenaltyIndicators control)
        {
            control.UpdateOptimalSuggestionsDisplay();
        }
    }

    private static void OnUpdateIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloStackingPenaltyIndicators control)
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
            shadowEffect.BlurRadius = 18 + (HolographicIntensity * 8);
        }
    }

    private void UpdateColorScheme()
    {
        // Update colors based on EVE color scheme
    }

    private void UpdateModuleGroups()
    {
        CalculateStackingPenalties();
        UpdateModuleGroupsDisplay();
    }

    private void UpdateVisualizationMode()
    {
        // Switch between different visualization modes
    }

    private void UpdateParticleSettings()
    {
        if (!EnableParticleEffects)
        {
            foreach (var particle in _penaltyParticles)
            {
                _particleCanvas.Children.Remove(particle.Visual);
            }
            _penaltyParticles.Clear();
        }
    }

    private void UpdatePenaltyCurveDisplay()
    {
        // Update penalty curve display
    }

    private void UpdateEfficiencyIndicatorDisplay()
    {
        // Update efficiency indicator display
    }

    private void UpdateOptimalSuggestionsDisplay()
    {
        // Update optimal suggestions display
    }

    #endregion
}

#region Supporting Classes

/// <summary>
/// Represents a group of modules with the same stacking attribute
/// </summary>
public class HoloModuleGroup
{
    public string GroupName { get; set; }
    public string StackingAttribute { get; set; }
    public List<HoloStackingModule> Modules { get; set; } = new();
}

/// <summary>
/// Represents a module that can have stacking penalties
/// </summary>
public class HoloStackingModule
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public double EffectValue { get; set; }
    public int StackingOrder { get; set; }
}

/// <summary>
/// Stacking visualization modes
/// </summary>
public enum StackingVisualizationMode
{
    Overview,
    Detailed,
    Curves,
    Optimization
}

/// <summary>
/// Stacking penalty calculation result
/// </summary>
public class StackingPenaltyData
{
    public double TotalEfficiency { get; set; }
    public double PenaltyFactor { get; set; }
    public List<double> IndividualEfficiencies { get; set; } = new();
}

/// <summary>
/// Optimization suggestion
/// </summary>
public class HoloOptimizationSuggestion
{
    public string Title { get; set; }
    public string Description { get; set; }
    public double EfficiencyGain { get; set; }
    public SuggestionImpactLevel ImpactLevel { get; set; }
    public string GroupName { get; set; }
}

/// <summary>
/// Suggestion impact levels
/// </summary>
public enum SuggestionImpactLevel
{
    Low,
    Medium,
    High
}

/// <summary>
/// Stacking penalty analysis result
/// </summary>
public class StackingPenaltyAnalysis
{
    public double OverallEfficiency { get; set; }
    public bool HasStackingPenalties { get; set; }
    public Dictionary<string, double> GroupEfficiencies { get; set; }
    public Dictionary<string, double> PenaltyFactors { get; set; }
    public int TotalGroups { get; set; }
    public int GroupsWithPenalties { get; set; }
    public double PotentialSavings { get; set; }
}

/// <summary>
/// Penalty particle for visual effects
/// </summary>
public class HoloPenaltyParticle
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
                Opacity = 0.7
            }
        };
    }
}

/// <summary>
/// Stacking calculations helper
/// </summary>
public class HoloStackingCalculator
{
    // EVE Online stacking penalty formula
    private readonly double[] _stackingFactors = { 1.0, 0.869, 0.571, 0.283, 0.106, 0.034, 0.009 };

    public StackingPenaltyData CalculateStackingPenalty(HoloModuleGroup group)
    {
        if (group?.Modules == null || group.Modules.Count == 0)
            return new StackingPenaltyData { TotalEfficiency = 1.0, PenaltyFactor = 0.0 };

        var sortedModules = group.Modules.OrderByDescending(m => m.EffectValue).ToList();
        var individualEfficiencies = new List<double>();
        double totalEffect = 0;
        double baseEffect = 0;

        for (int i = 0; i < sortedModules.Count; i++)
        {
            var module = sortedModules[i];
            var stackingFactor = i < _stackingFactors.Length ? _stackingFactors[i] : _stackingFactors.Last();
            var effectiveValue = module.EffectValue * stackingFactor;
            
            totalEffect += effectiveValue;
            baseEffect += module.EffectValue;
            individualEfficiencies.Add(stackingFactor);
        }

        var totalEfficiency = baseEffect > 0 ? totalEffect / baseEffect : 1.0;
        var penaltyFactor = 1.0 - totalEfficiency;

        return new StackingPenaltyData
        {
            TotalEfficiency = totalEfficiency,
            PenaltyFactor = penaltyFactor,
            IndividualEfficiencies = individualEfficiencies
        };
    }

    public List<HoloOptimizationSuggestion> GenerateOptimizationSuggestions(
        ObservableCollection<HoloModuleGroup> moduleGroups,
        Dictionary<string, double> groupEfficiencies,
        Dictionary<string, double> penaltyFactors)
    {
        var suggestions = new List<HoloOptimizationSuggestion>();

        if (moduleGroups == null)
            return suggestions;

        foreach (var group in moduleGroups)
        {
            var efficiency = groupEfficiencies.GetValueOrDefault(group.GroupName, 1.0);
            var penalty = penaltyFactors.GetValueOrDefault(group.GroupName, 0.0);

            if (penalty > 0.05 && group.Modules.Count > 2)
            {
                var impact = penalty > 0.2 ? SuggestionImpactLevel.High :
                            penalty > 0.1 ? SuggestionImpactLevel.Medium : SuggestionImpactLevel.Low;

                suggestions.Add(new HoloOptimizationSuggestion
                {
                    Title = $"Reduce {group.GroupName} Modules",
                    Description = $"Consider removing {group.Modules.Count - 2} modules to minimize stacking penalties.",
                    EfficiencyGain = penalty * 0.6, // Estimated gain
                    ImpactLevel = impact,
                    GroupName = group.GroupName
                });
            }

            if (group.Modules.Count > 4)
            {
                suggestions.Add(new HoloOptimizationSuggestion
                {
                    Title = $"Optimize {group.GroupName} Configuration",
                    Description = $"Replace weaker modules with stronger variants to improve stacking efficiency.",
                    EfficiencyGain = penalty * 0.3,
                    ImpactLevel = SuggestionImpactLevel.Medium,
                    GroupName = group.GroupName
                });
            }
        }

        return suggestions.OrderByDescending(s => s.EfficiencyGain).ToList();
    }
}

#endregion