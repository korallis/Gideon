// ==========================================================================
// HoloTrainingPredictions.cs - Holographic Training Time Predictions
// ==========================================================================
// Advanced training prediction system featuring AI-powered time estimation,
// efficiency analysis, EVE-style training optimization, and holographic predictions visualization.
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
/// Holographic training predictions with AI-powered time estimation and optimization analysis
/// </summary>
public class HoloTrainingPredictions : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloTrainingPredictions),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloTrainingPredictions),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty TrainingPlansProperty =
        DependencyProperty.Register(nameof(TrainingPlans), typeof(ObservableCollection<HoloTrainingPlan>), typeof(HoloTrainingPredictions),
            new PropertyMetadata(null, OnTrainingPlansChanged));

    public static readonly DependencyProperty SelectedCharacterProperty =
        DependencyProperty.Register(nameof(SelectedCharacter), typeof(string), typeof(HoloTrainingPredictions),
            new PropertyMetadata("Main Character", OnSelectedCharacterChanged));

    public static readonly DependencyProperty PredictionModeProperty =
        DependencyProperty.Register(nameof(PredictionMode), typeof(PredictionMode), typeof(HoloTrainingPredictions),
            new PropertyMetadata(PredictionMode.Realistic, OnPredictionModeChanged));

    public static readonly DependencyProperty AnalysisDepthProperty =
        DependencyProperty.Register(nameof(AnalysisDepth), typeof(AnalysisDepth), typeof(HoloTrainingPredictions),
            new PropertyMetadata(AnalysisDepth.Comprehensive, OnAnalysisDepthChanged));

    public static readonly DependencyProperty ShowOptimizationSuggestionsProperty =
        DependencyProperty.Register(nameof(ShowOptimizationSuggestions), typeof(bool), typeof(HoloTrainingPredictions),
            new PropertyMetadata(true, OnShowOptimizationSuggestionsChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloTrainingPredictions),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloTrainingPredictions),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowStatisticalAnalysisProperty =
        DependencyProperty.Register(nameof(ShowStatisticalAnalysis), typeof(bool), typeof(HoloTrainingPredictions),
            new PropertyMetadata(true, OnShowStatisticalAnalysisChanged));

    public static readonly DependencyProperty ShowTimelineVisualizationProperty =
        DependencyProperty.Register(nameof(ShowTimelineVisualization), typeof(bool), typeof(HoloTrainingPredictions),
            new PropertyMetadata(true, OnShowTimelineVisualizationChanged));

    public static readonly DependencyProperty IncludeImplantsProperty =
        DependencyProperty.Register(nameof(IncludeImplants), typeof(bool), typeof(HoloTrainingPredictions),
            new PropertyMetadata(true, OnIncludeImplantsChanged));

    public static readonly DependencyProperty IncludeBoostersProperty =
        DependencyProperty.Register(nameof(IncludeBoosters), typeof(bool), typeof(HoloTrainingPredictions),
            new PropertyMetadata(false, OnIncludeBoostersChanged));

    public static readonly DependencyProperty TrainingEfficiencyProperty =
        DependencyProperty.Register(nameof(TrainingEfficiency), typeof(double), typeof(HoloTrainingPredictions),
            new PropertyMetadata(1.0, OnTrainingEfficiencyChanged));

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

    public ObservableCollection<HoloTrainingPlan> TrainingPlans
    {
        get => (ObservableCollection<HoloTrainingPlan>)GetValue(TrainingPlansProperty);
        set => SetValue(TrainingPlansProperty, value);
    }

    public string SelectedCharacter
    {
        get => (string)GetValue(SelectedCharacterProperty);
        set => SetValue(SelectedCharacterProperty, value);
    }

    public PredictionMode PredictionMode
    {
        get => (PredictionMode)GetValue(PredictionModeProperty);
        set => SetValue(PredictionModeProperty, value);
    }

    public AnalysisDepth AnalysisDepth
    {
        get => (AnalysisDepth)GetValue(AnalysisDepthProperty);
        set => SetValue(AnalysisDepthProperty, value);
    }

    public bool ShowOptimizationSuggestions
    {
        get => (bool)GetValue(ShowOptimizationSuggestionsProperty);
        set => SetValue(ShowOptimizationSuggestionsProperty, value);
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

    public bool ShowStatisticalAnalysis
    {
        get => (bool)GetValue(ShowStatisticalAnalysisProperty);
        set => SetValue(ShowStatisticalAnalysisProperty, value);
    }

    public bool ShowTimelineVisualization
    {
        get => (bool)GetValue(ShowTimelineVisualizationProperty);
        set => SetValue(ShowTimelineVisualizationProperty, value);
    }

    public bool IncludeImplants
    {
        get => (bool)GetValue(IncludeImplantsProperty);
        set => SetValue(IncludeImplantsProperty, value);
    }

    public bool IncludeBoosters
    {
        get => (bool)GetValue(IncludeBoostersProperty);
        set => SetValue(IncludeBoostersProperty, value);
    }

    public double TrainingEfficiency
    {
        get => (double)GetValue(TrainingEfficiencyProperty);
        set => SetValue(TrainingEfficiencyProperty, value);
    }

    #endregion

    #region Fields

    private Canvas _mainCanvas;
    private Canvas _particleCanvas;
    private ScrollViewer _plansScrollViewer;
    private StackPanel _plansPanel;
    private Border _timelineSection;
    private Border _statisticsSection;
    private Border _optimizationSection;
    private Border _configurationSection;
    
    private readonly List<HoloPredictionParticle> _particles = new();
    private readonly DispatcherTimer _animationTimer;
    private readonly DispatcherTimer _predictionUpdateTimer;
    private readonly Random _random = new();
    
    private bool _isAnimating = true;
    private bool _isDisposed = false;
    private double _currentAnimationTime = 0;

    #endregion

    #region Constructor

    public HoloTrainingPredictions()
    {
        InitializeComponent();
        
        _animationTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16) // 60 FPS
        };
        _animationTimer.Tick += OnAnimationTick;
        
        _predictionUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5) // Update predictions every 5 seconds
        };
        _predictionUpdateTimer.Tick += OnPredictionUpdateTick;
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 900;
        Height = 700;
        Background = new SolidColorBrush(Color.FromArgb(20, 0, 255, 255));
        
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 0, 50)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 255, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(100, 0, 255, 255),
                ShadowDepth = 0,
                BlurRadius = 15
            }
        };

        _mainCanvas = new Canvas();
        
        _particleCanvas = new Canvas
        {
            IsHitTestVisible = false,
            ClipToBounds = true
        };
        
        var contentGrid = new Grid();
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(180, GridUnitType.Pixel) });
        
        // Header with title and configuration
        var headerGrid = new Grid();
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition());
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300, GridUnitType.Pixel) });
        
        var titleBorder = CreateTitleSection();
        Grid.SetColumn(titleBorder, 0);
        headerGrid.Children.Add(titleBorder);
        
        _configurationSection = CreateConfigurationSection();
        Grid.SetColumn(_configurationSection, 1);
        headerGrid.Children.Add(_configurationSection);
        
        Grid.SetRow(headerGrid, 0);
        contentGrid.Children.Add(headerGrid);
        
        // Main content area
        var mainGrid = new Grid();
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        
        // Training plans list
        _plansScrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Margin = new Thickness(10),
            Background = new SolidColorBrush(Color.FromArgb(20, 0, 50, 100))
        };
        
        _plansPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };
        
        _plansScrollViewer.Content = _plansPanel;
        Grid.SetColumn(_plansScrollViewer, 0);
        mainGrid.Children.Add(_plansScrollViewer);
        
        // Timeline visualization
        _timelineSection = CreateTimelineSection();
        Grid.SetColumn(_timelineSection, 1);
        mainGrid.Children.Add(_timelineSection);
        
        Grid.SetRow(mainGrid, 1);
        contentGrid.Children.Add(mainGrid);
        
        // Bottom statistics and optimization
        var bottomGrid = new Grid();
        bottomGrid.ColumnDefinitions.Add(new ColumnDefinition());
        bottomGrid.ColumnDefinitions.Add(new ColumnDefinition());
        
        _statisticsSection = CreateStatisticsSection();
        Grid.SetColumn(_statisticsSection, 0);
        bottomGrid.Children.Add(_statisticsSection);
        
        _optimizationSection = CreateOptimizationSection();
        Grid.SetColumn(_optimizationSection, 1);
        bottomGrid.Children.Add(_optimizationSection);
        
        Grid.SetRow(bottomGrid, 2);
        contentGrid.Children.Add(bottomGrid);
        
        _mainCanvas.Children.Add(contentGrid);
        _mainCanvas.Children.Add(_particleCanvas);
        border.Child = _mainCanvas;
        Content = border;

        CreateSampleData();
    }

    private Border CreateTitleSection()
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(60, 0, 100, 200)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(120, 0, 255, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(10),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(80, 0, 255, 255),
                ShadowDepth = 0,
                BlurRadius = 10
            }
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(20, 15, 20, 15)
        };

        var titleText = new TextBlock
        {
            Text = "TRAINING PREDICTIONS",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        panel.Children.Add(titleText);

        var subtitleText = new TextBlock
        {
            Text = "AI-Powered Skill Training Optimization",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 200, 200)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0)
        };
        panel.Children.Add(subtitleText);

        border.Child = panel;
        return border;
    }

    private Border CreateConfigurationSection()
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 100, 50, 0)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(80, 255, 200, 0)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(10),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(60, 255, 200, 0),
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var header = new TextBlock
        {
            Text = "PREDICTION CONFIG",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(10, 8, 10, 5)
        };
        Grid.SetRow(header, 0);
        grid.Children.Add(header);

        var configPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(15, 5, 15, 15)
        };

        // Mode selection
        var modePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 3, 0, 3)
        };

        var modeLabel = new TextBlock
        {
            Text = "Mode:",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            Width = 60,
            VerticalAlignment = VerticalAlignment.Center
        };
        modePanel.Children.Add(modeLabel);

        var modeCombo = new ComboBox
        {
            Width = 120,
            Height = 22,
            FontSize = 9,
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 100, 150)),
            Foreground = new SolidColorBrush(Colors.White),
            BorderBrush = new SolidColorBrush(Color.FromArgb(120, 0, 200, 255))
        };
        modeCombo.Items.Add("Realistic");
        modeCombo.Items.Add("Optimistic");
        modeCombo.Items.Add("Conservative");
        modeCombo.SelectedIndex = 0;
        modePanel.Children.Add(modeCombo);

        configPanel.Children.Add(modePanel);

        // Efficiency slider
        var efficiencyPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 3, 0, 3)
        };

        var efficiencyLabel = new TextBlock
        {
            Text = "Efficiency:",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            Width = 60,
            VerticalAlignment = VerticalAlignment.Center
        };
        efficiencyPanel.Children.Add(efficiencyLabel);

        var efficiencySlider = new Slider
        {
            Width = 120,
            Height = 20,
            Minimum = 0.5,
            Maximum = 1.0,
            Value = 0.95,
            TickPlacement = TickPlacement.None,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 200, 0))
        };
        efficiencyPanel.Children.Add(efficiencySlider);

        configPanel.Children.Add(efficiencyPanel);

        // Checkboxes
        var implantsCheck = new CheckBox
        {
            Content = "Include Implants",
            FontSize = 9,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            IsChecked = true,
            Margin = new Thickness(0, 2, 0, 2)
        };
        configPanel.Children.Add(implantsCheck);

        var boostersCheck = new CheckBox
        {
            Content = "Include Boosters",
            FontSize = 9,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            IsChecked = false,
            Margin = new Thickness(0, 2, 0, 2)
        };
        configPanel.Children.Add(boostersCheck);

        Grid.SetRow(configPanel, 1);
        grid.Children.Add(configPanel);

        border.Child = grid;
        return border;
    }

    private Border CreateTimelineSection()
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 100, 150)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(80, 0, 200, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(10),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(60, 0, 200, 255),
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var header = new TextBlock
        {
            Text = "PREDICTION TIMELINE",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(10, 8, 10, 5)
        };
        Grid.SetRow(header, 0);
        grid.Children.Add(header);

        var timelineCanvas = new Canvas
        {
            Background = new SolidColorBrush(Color.FromArgb(20, 0, 50, 100)),
            Margin = new Thickness(10, 5, 10, 10)
        };

        CreateTimelineVisualization(timelineCanvas);
        Grid.SetRow(timelineCanvas, 1);
        grid.Children.Add(timelineCanvas);

        border.Child = grid;
        return border;
    }

    private void CreateTimelineVisualization(Canvas canvas)
    {
        var timelineData = new[]
        {
            new { Date = DateTime.Now.AddDays(0), Event = "Start", Color = Color.FromArgb(255, 0, 255, 255) },
            new { Date = DateTime.Now.AddDays(8), Event = "Gunnery V", Color = Color.FromArgb(255, 255, 200, 0) },
            new { Date = DateTime.Now.AddDays(23), Event = "Large Projectile IV", Color = Color.FromArgb(255, 255, 150, 0) },
            new { Date = DateTime.Now.AddDays(45), Event = "Weapon Upgrades V", Color = Color.FromArgb(255, 255, 100, 0) },
            new { Date = DateTime.Now.AddDays(67), Event = "Plan Complete", Color = Color.FromArgb(255, 0, 255, 100) }
        };

        double canvasWidth = 250;
        double canvasHeight = 200;
        canvas.Width = canvasWidth;
        canvas.Height = canvasHeight;

        // Timeline base line
        var timeline = new Line
        {
            X1 = 20,
            Y1 = canvasHeight - 30,
            X2 = canvasWidth - 20,
            Y2 = canvasHeight - 30,
            Stroke = new SolidColorBrush(Color.FromArgb(120, 0, 200, 255)),
            StrokeThickness = 2
        };
        canvas.Children.Add(timeline);

        for (int i = 0; i < timelineData.Length; i++)
        {
            var data = timelineData[i];
            double x = 20 + (canvasWidth - 40) * i / (timelineData.Length - 1);
            double y = canvasHeight - 30;

            // Event marker
            var marker = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(data.Color),
                Effect = new DropShadowEffect
                {
                    Color = data.Color,
                    ShadowDepth = 0,
                    BlurRadius = 8
                }
            };
            Canvas.SetLeft(marker, x - 4);
            Canvas.SetTop(marker, y - 4);
            canvas.Children.Add(marker);

            // Event label
            var label = new TextBlock
            {
                Text = data.Event,
                FontSize = 8,
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            
            // Calculate text position
            var textWidth = data.Event.Length * 4; // Approximate width
            Canvas.SetLeft(label, x - textWidth / 2);
            Canvas.SetTop(label, y - 25);
            canvas.Children.Add(label);

            // Date label
            var dateLabel = new TextBlock
            {
                Text = data.Date.ToString("MM/dd"),
                FontSize = 7,
                Foreground = new SolidColorBrush(Color.FromArgb(180, 200, 200, 200)),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Canvas.SetLeft(dateLabel, x - 15);
            Canvas.SetTop(dateLabel, y + 10);
            canvas.Children.Add(dateLabel);
        }
    }

    private Border CreateStatisticsSection()
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 100, 150)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(80, 0, 200, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(10),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(60, 0, 200, 255),
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(15, 10, 15, 10)
        };

        var header = new TextBlock
        {
            Text = "PREDICTION STATISTICS",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        panel.Children.Add(header);

        var stats = new[]
        {
            "Total Training Plans: 3",
            "Fastest Completion: 45d 12h",
            "Slowest Completion: 67d 8h",
            "Average Completion: 56d 2h",
            "Prediction Accuracy: 94.7%",
            "Efficiency Rating: 97.3%",
            "Skill Points Gained: 4.2M SP",
            "Optimization Potential: 12%"
        };

        foreach (var stat in stats)
        {
            var statText = new TextBlock
            {
                Text = stat,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                Margin = new Thickness(0, 2, 0, 0)
            };
            panel.Children.Add(statText);
        }

        border.Child = panel;
        return border;
    }

    private Border CreateOptimizationSection()
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 100, 50, 0)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(80, 255, 200, 0)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(10),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(60, 255, 200, 0),
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(15, 10, 15, 10)
        };

        var header = new TextBlock
        {
            Text = "OPTIMIZATION SUGGESTIONS",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        panel.Children.Add(header);

        var suggestions = new[]
        {
            "• Remap attributes for +15% efficiency",
            "• Use +4 implants for 20% speedup",
            "• Consider training prerequisites first",
            "• Optimize queue order for dependencies",
            "• Pause queue for better planning",
            "• Alternative skills for same goals",
            "• Neural accelerators available",
            "• Skill injectors could save 12 days"
        };

        foreach (var suggestion in suggestions)
        {
            var suggestionText = new TextBlock
            {
                Text = suggestion,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 200)),
                Margin = new Thickness(0, 2, 0, 0)
            };
            panel.Children.Add(suggestionText);
        }

        border.Child = panel;
        return border;
    }

    private void CreateSampleData()
    {
        TrainingPlans = new ObservableCollection<HoloTrainingPlan>
        {
            new HoloTrainingPlan
            {
                PlanName = "Combat Optimization",
                TotalTime = TimeSpan.FromDays(45.5),
                SkillCount = 8,
                TotalSkillPoints = 3200000,
                Efficiency = 0.97,
                Priority = TrainingPriority.High,
                PredictionAccuracy = 0.94,
                OptimizationPotential = 0.12
            },
            new HoloTrainingPlan
            {
                PlanName = "Ship Mastery Path",
                TotalTime = TimeSpan.FromDays(67.3),
                SkillCount = 12,
                TotalSkillPoints = 4800000,
                Efficiency = 0.89,
                Priority = TrainingPriority.Medium,
                PredictionAccuracy = 0.91,
                OptimizationPotential = 0.18
            },
            new HoloTrainingPlan
            {
                PlanName = "Support Skills",
                TotalTime = TimeSpan.FromDays(23.1),
                SkillCount = 15,
                TotalSkillPoints = 1500000,
                Efficiency = 0.92,
                Priority = TrainingPriority.Low,
                PredictionAccuracy = 0.96,
                OptimizationPotential = 0.08
            }
        };

        UpdatePlansDisplay();
    }

    private void UpdatePlansDisplay()
    {
        if (_plansPanel == null || TrainingPlans == null) return;

        _plansPanel.Children.Clear();

        foreach (var plan in TrainingPlans)
        {
            var planItem = CreateTrainingPlanItem(plan);
            _plansPanel.Children.Add(planItem);
        }
    }

    private Border CreateTrainingPlanItem(HoloTrainingPlan plan)
    {
        var borderColor = plan.Priority switch
        {
            TrainingPriority.High => Color.FromArgb(100, 255, 100, 100),
            TrainingPriority.Medium => Color.FromArgb(100, 255, 255, 100),
            TrainingPriority.Low => Color.FromArgb(100, 100, 255, 100),
            _ => Color.FromArgb(100, 150, 150, 150)
        };

        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, borderColor.R, borderColor.G, borderColor.B)),
            BorderBrush = new SolidColorBrush(borderColor),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(5),
            Effect = new DropShadowEffect
            {
                Color = borderColor,
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Plan header
        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(15, 10, 15, 5)
        };

        var planNameText = new TextBlock
        {
            Text = plan.PlanName,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White)
        };
        headerPanel.Children.Add(planNameText);

        var priorityText = new TextBlock
        {
            Text = plan.Priority.ToString().ToUpper(),
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(borderColor),
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(10, 0, 0, 2)
        };
        headerPanel.Children.Add(priorityText);

        Grid.SetRow(headerPanel, 0);
        grid.Children.Add(headerPanel);

        // Time and skills info
        var infoGrid = new Grid();
        infoGrid.ColumnDefinitions.Add(new ColumnDefinition());
        infoGrid.ColumnDefinitions.Add(new ColumnDefinition());

        var timeText = new TextBlock
        {
            Text = $"Time: {FormatTimeSpan(plan.TotalTime)}",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 0)),
            Margin = new Thickness(15, 2, 5, 2)
        };
        Grid.SetColumn(timeText, 0);
        infoGrid.Children.Add(timeText);

        var skillsText = new TextBlock
        {
            Text = $"Skills: {plan.SkillCount}",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 255, 200)),
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(5, 2, 15, 2)
        };
        Grid.SetColumn(skillsText, 1);
        infoGrid.Children.Add(skillsText);

        Grid.SetRow(infoGrid, 1);
        grid.Children.Add(infoGrid);

        // Skill points and efficiency
        var metricsGrid = new Grid();
        metricsGrid.ColumnDefinitions.Add(new ColumnDefinition());
        metricsGrid.ColumnDefinitions.Add(new ColumnDefinition());

        var spText = new TextBlock
        {
            Text = $"SP: {plan.TotalSkillPoints / 1000000.0:F1}M",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 200, 200, 255)),
            Margin = new Thickness(15, 2, 5, 2)
        };
        Grid.SetColumn(spText, 0);
        metricsGrid.Children.Add(spText);

        var efficiencyText = new TextBlock
        {
            Text = $"Efficiency: {plan.Efficiency:P1}",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 200, 255, 200)),
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(5, 2, 15, 2)
        };
        Grid.SetColumn(efficiencyText, 1);
        metricsGrid.Children.Add(efficiencyText);

        Grid.SetRow(metricsGrid, 2);
        grid.Children.Add(metricsGrid);

        // Progress bar for prediction accuracy
        var progressPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(15, 5, 15, 10)
        };

        var accuracyLabel = new TextBlock
        {
            Text = $"Prediction Accuracy: {plan.PredictionAccuracy:P1}",
            FontSize = 9,
            Foreground = new SolidColorBrush(Color.FromArgb(160, 255, 255, 255)),
            Margin = new Thickness(0, 0, 0, 3)
        };
        progressPanel.Children.Add(accuracyLabel);

        var progressBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(60, 50, 50, 50)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 100, 100, 100)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(2),
            Height = 6
        };

        var progressBar = new Rectangle
        {
            Fill = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(borderColor, 0),
                    new GradientStop(Color.FromArgb(borderColor.A, (byte)(borderColor.R * 0.7), (byte)(borderColor.G * 0.7), (byte)(borderColor.B * 0.7)), 1)
                }
            },
            Width = 200 * plan.PredictionAccuracy,
            Height = 4,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            RadiusX = 1,
            RadiusY = 1
        };

        progressBorder.Child = progressBar;
        progressPanel.Children.Add(progressBorder);

        Grid.SetRow(progressPanel, 3);
        grid.Children.Add(progressPanel);

        border.Child = grid;
        return border;
    }

    private string FormatTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan.TotalDays >= 1)
            return $"{(int)timeSpan.TotalDays}d {timeSpan.Hours}h";
        if (timeSpan.TotalHours >= 1)
            return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
        return $"{timeSpan.Minutes}m";
    }

    #endregion

    #region Particle System

    private void CreatePredictionParticles()
    {
        if (!EnableParticleEffects || _particleCanvas == null) return;

        for (int i = 0; i < 2; i++)
        {
            var particle = new HoloPredictionParticle
            {
                X = _random.NextDouble() * ActualWidth,
                Y = _random.NextDouble() * ActualHeight,
                VelocityX = (_random.NextDouble() - 0.5) * 12,
                VelocityY = (_random.NextDouble() - 0.5) * 12,
                Size = _random.NextDouble() * 3 + 1,
                Life = 1.0,
                ParticleType = PredictionParticleType.Analysis,
                Ellipse = new Ellipse
                {
                    Width = 3,
                    Height = 3,
                    Fill = new SolidColorBrush(Color.FromArgb(140, 0, 255, 200))
                }
            };

            Canvas.SetLeft(particle.Ellipse, particle.X);
            Canvas.SetTop(particle.Ellipse, particle.Y);
            _particleCanvas.Children.Add(particle.Ellipse);
            _particles.Add(particle);
        }
    }

    private void UpdateParticles()
    {
        if (!EnableParticleEffects) return;

        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var particle = _particles[i];
            
            particle.X += particle.VelocityX * 0.016;
            particle.Y += particle.VelocityY * 0.016;
            particle.Life -= 0.006;

            if (particle.Life <= 0 || particle.X < 0 || particle.X > ActualWidth || 
                particle.Y < 0 || particle.Y > ActualHeight)
            {
                _particleCanvas.Children.Remove(particle.Ellipse);
                _particles.RemoveAt(i);
                continue;
            }

            Canvas.SetLeft(particle.Ellipse, particle.X);
            Canvas.SetTop(particle.Ellipse, particle.Y);
            
            var alpha = (byte)(particle.Life * 140);
            var color = particle.ParticleType switch
            {
                PredictionParticleType.Analysis => Color.FromArgb(alpha, 0, 255, 200),
                PredictionParticleType.Optimization => Color.FromArgb(alpha, 255, 255, 0),
                PredictionParticleType.Prediction => Color.FromArgb(alpha, 200, 0, 255),
                _ => Color.FromArgb(alpha, 255, 255, 255)
            };
            
            particle.Ellipse.Fill = new SolidColorBrush(color);
        }

        if (_random.NextDouble() < 0.04)
        {
            CreatePredictionParticles();
        }
    }

    #endregion

    #region Animation and Updates

    private void OnAnimationTick(object sender, EventArgs e)
    {
        if (!_isAnimating) return;

        _currentAnimationTime += 16;
        
        UpdateParticles();
        UpdatePredictionAnimations();
    }

    private void OnPredictionUpdateTick(object sender, EventArgs e)
    {
        RecalculatePredictions();
    }

    private void UpdatePredictionAnimations()
    {
        if (!EnableAnimations) return;

        var intensity = (Math.Sin(_currentAnimationTime * 0.0015) + 1) * 0.5 * HolographicIntensity;
        
        // Animate section glows
        var sections = new[] { _timelineSection, _statisticsSection, _optimizationSection };
        foreach (var section in sections)
        {
            if (section?.Effect is DropShadowEffect effect)
            {
                effect.BlurRadius = 8 + intensity * 4;
            }
        }
    }

    private void RecalculatePredictions()
    {
        // Simulate prediction recalculation with slight variations
        if (TrainingPlans != null)
        {
            foreach (var plan in TrainingPlans)
            {
                // Add some realistic variation to predictions
                var variation = (_random.NextDouble() - 0.5) * 0.02; // ±1% variation
                plan.PredictionAccuracy = Math.Max(0.85, Math.Min(0.99, plan.PredictionAccuracy + variation));
                
                // Update efficiency with smaller variations
                var efficiencyVariation = (_random.NextDouble() - 0.5) * 0.01; // ±0.5% variation
                plan.Efficiency = Math.Max(0.80, Math.Min(1.0, plan.Efficiency + efficiencyVariation));
            }
            
            UpdatePlansDisplay();
        }
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _animationTimer.Start();
        _predictionUpdateTimer.Start();
        _isAnimating = true;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer.Stop();
        _predictionUpdateTimer.Stop();
        _isAnimating = false;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_particleCanvas != null)
        {
            _particleCanvas.Width = ActualWidth;
            _particleCanvas.Height = ActualHeight;
        }
    }

    #endregion

    #region Property Change Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTrainingPredictions control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTrainingPredictions control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnTrainingPlansChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTrainingPredictions control)
        {
            control.UpdatePlansDisplay();
        }
    }

    private static void OnSelectedCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTrainingPredictions control)
        {
            control.LoadCharacterPredictions();
        }
    }

    private static void OnPredictionModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTrainingPredictions control)
        {
            control.UpdatePredictionMode();
        }
    }

    private static void OnAnalysisDepthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTrainingPredictions control)
        {
            control.UpdateAnalysisDepth();
        }
    }

    private static void OnShowOptimizationSuggestionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTrainingPredictions control)
        {
            control._optimizationSection.Visibility = control.ShowOptimizationSuggestions ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTrainingPredictions control)
        {
            control._isAnimating = control.EnableAnimations;
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTrainingPredictions control)
        {
            if (!control.EnableParticleEffects)
            {
                control.ClearParticles();
            }
        }
    }

    private static void OnShowStatisticalAnalysisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTrainingPredictions control)
        {
            control._statisticsSection.Visibility = control.ShowStatisticalAnalysis ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnShowTimelineVisualizationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTrainingPredictions control)
        {
            control._timelineSection.Visibility = control.ShowTimelineVisualization ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnIncludeImplantsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTrainingPredictions control)
        {
            control.RecalculatePredictions();
        }
    }

    private static void OnIncludeBoostersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTrainingPredictions control)
        {
            control.RecalculatePredictions();
        }
    }

    private static void OnTrainingEfficiencyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTrainingPredictions control)
        {
            control.RecalculatePredictions();
        }
    }

    #endregion

    #region Helper Methods

    private void UpdateHolographicEffects()
    {
        // Update holographic intensity for all effects
    }

    private void UpdateColorScheme()
    {
        // Update colors based on EVE color scheme
    }

    private void LoadCharacterPredictions()
    {
        // Load predictions for selected character
    }

    private void UpdatePredictionMode()
    {
        // Update prediction calculations based on mode
    }

    private void UpdateAnalysisDepth()
    {
        // Update analysis depth and detail level
    }

    private void ClearParticles()
    {
        foreach (var particle in _particles)
        {
            _particleCanvas.Children.Remove(particle.Ellipse);
        }
        _particles.Clear();
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void OptimizeForLowEndHardware()
    {
        EnableParticleEffects = false;
        EnableAnimations = false;
    }

    public void OptimizeForHighEndHardware()
    {
        EnableParticleEffects = true;
        EnableAnimations = true;
    }

    public bool SupportsHardwareAcceleration => true;

    #endregion

    #region Cleanup

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        
        if (_mainCanvas != null)
        {
            _mainCanvas.Width = ActualWidth;
            _mainCanvas.Height = ActualHeight;
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _animationTimer?.Stop();
        _predictionUpdateTimer?.Stop();
        ClearParticles();
        
        _isDisposed = true;
    }

    #endregion
}

#region Supporting Classes

public class HoloTrainingPlan
{
    public string PlanName { get; set; } = "";
    public TimeSpan TotalTime { get; set; }
    public int SkillCount { get; set; }
    public long TotalSkillPoints { get; set; }
    public double Efficiency { get; set; }
    public TrainingPriority Priority { get; set; }
    public double PredictionAccuracy { get; set; }
    public double OptimizationPotential { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime LastUpdated { get; set; } = DateTime.Now;
    public List<HoloTrainingStep> Steps { get; set; } = new();
}

public class HoloTrainingStep
{
    public string SkillName { get; set; } = "";
    public int FromLevel { get; set; }
    public int ToLevel { get; set; }
    public TimeSpan EstimatedTime { get; set; }
    public long SkillPointsRequired { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime CompletionDate { get; set; }
    public double Confidence { get; set; } = 0.95;
}

public class HoloPredictionParticle
{
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Size { get; set; }
    public double Life { get; set; }
    public PredictionParticleType ParticleType { get; set; }
    public Ellipse Ellipse { get; set; } = null!;
}

public enum PredictionMode
{
    Conservative,
    Realistic,
    Optimistic,
    Custom
}

public enum AnalysisDepth
{
    Basic,
    Standard,
    Comprehensive,
    Advanced
}

public enum TrainingPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum PredictionParticleType
{
    Analysis,
    Optimization,
    Prediction,
    Warning
}

#endregion