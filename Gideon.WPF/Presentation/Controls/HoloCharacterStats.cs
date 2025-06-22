// ==========================================================================
// HoloCharacterStats.cs - Holographic Character Statistics Display
// ==========================================================================
// Advanced statistics visualization system featuring real-time analytics,
// performance metrics, EVE-style character analysis, and holographic data representation.
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
/// Holographic character statistics display with comprehensive analytics and performance metrics
/// </summary>
public class HoloCharacterStats : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloCharacterStats),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloCharacterStats),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty CharacterStatisticsProperty =
        DependencyProperty.Register(nameof(CharacterStatistics), typeof(HoloCharacterStatistics), typeof(HoloCharacterStats),
            new PropertyMetadata(null, OnCharacterStatisticsChanged));

    public static readonly DependencyProperty SelectedCharacterProperty =
        DependencyProperty.Register(nameof(SelectedCharacter), typeof(string), typeof(HoloCharacterStats),
            new PropertyMetadata("Main Character", OnSelectedCharacterChanged));

    public static readonly DependencyProperty StatsCategoryProperty =
        DependencyProperty.Register(nameof(StatsCategory), typeof(StatsCategory), typeof(HoloCharacterStats),
            new PropertyMetadata(StatsCategory.Overview, OnStatsCategoryChanged));

    public static readonly DependencyProperty TimeRangeProperty =
        DependencyProperty.Register(nameof(TimeRange), typeof(StatsTimeRange), typeof(HoloCharacterStats),
            new PropertyMetadata(StatsTimeRange.Last30Days, OnTimeRangeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloCharacterStats),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloCharacterStats),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowRealTimeUpdatesProperty =
        DependencyProperty.Register(nameof(ShowRealTimeUpdates), typeof(bool), typeof(HoloCharacterStats),
            new PropertyMetadata(true, OnShowRealTimeUpdatesChanged));

    public static readonly DependencyProperty ShowPerformanceMetricsProperty =
        DependencyProperty.Register(nameof(ShowPerformanceMetrics), typeof(bool), typeof(HoloCharacterStats),
            new PropertyMetadata(true, OnShowPerformanceMetricsChanged));

    public static readonly DependencyProperty ShowTrendAnalysisProperty =
        DependencyProperty.Register(nameof(ShowTrendAnalysis), typeof(bool), typeof(HoloCharacterStats),
            new PropertyMetadata(true, OnShowTrendAnalysisChanged));

    public static readonly DependencyProperty ShowComparisonsProperty =
        DependencyProperty.Register(nameof(ShowComparisons), typeof(bool), typeof(HoloCharacterStats),
            new PropertyMetadata(true, OnShowComparisonsChanged));

    public static readonly DependencyProperty AutoRefreshProperty =
        DependencyProperty.Register(nameof(AutoRefresh), typeof(bool), typeof(HoloCharacterStats),
            new PropertyMetadata(true, OnAutoRefreshChanged));

    public static readonly DependencyProperty RefreshIntervalProperty =
        DependencyProperty.Register(nameof(RefreshInterval), typeof(TimeSpan), typeof(HoloCharacterStats),
            new PropertyMetadata(TimeSpan.FromMinutes(5), OnRefreshIntervalChanged));

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

    public HoloCharacterStatistics CharacterStatistics
    {
        get => (HoloCharacterStatistics)GetValue(CharacterStatisticsProperty);
        set => SetValue(CharacterStatisticsProperty, value);
    }

    public string SelectedCharacter
    {
        get => (string)GetValue(SelectedCharacterProperty);
        set => SetValue(SelectedCharacterProperty, value);
    }

    public StatsCategory StatsCategory
    {
        get => (StatsCategory)GetValue(StatsCategoryProperty);
        set => SetValue(StatsCategoryProperty, value);
    }

    public StatsTimeRange TimeRange
    {
        get => (StatsTimeRange)GetValue(TimeRangeProperty);
        set => SetValue(TimeRangeProperty, value);
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

    public bool ShowRealTimeUpdates
    {
        get => (bool)GetValue(ShowRealTimeUpdatesProperty);
        set => SetValue(ShowRealTimeUpdatesProperty, value);
    }

    public bool ShowPerformanceMetrics
    {
        get => (bool)GetValue(ShowPerformanceMetricsProperty);
        set => SetValue(ShowPerformanceMetricsProperty, value);
    }

    public bool ShowTrendAnalysis
    {
        get => (bool)GetValue(ShowTrendAnalysisProperty);
        set => SetValue(ShowTrendAnalysisProperty, value);
    }

    public bool ShowComparisons
    {
        get => (bool)GetValue(ShowComparisonsProperty);
        set => SetValue(ShowComparisonsProperty, value);
    }

    public bool AutoRefresh
    {
        get => (bool)GetValue(AutoRefreshProperty);
        set => SetValue(AutoRefreshProperty, value);
    }

    public TimeSpan RefreshInterval
    {
        get => (TimeSpan)GetValue(RefreshIntervalProperty);
        set => SetValue(RefreshIntervalProperty, value);
    }

    #endregion

    #region Fields

    private readonly DispatcherTimer _animationTimer;
    private readonly DispatcherTimer _refreshTimer;
    private readonly List<ParticleSystem> _particleSystems = new();
    private Canvas _particleCanvas;
    private Grid _mainGrid;
    private TabControl _tabControl;
    private Canvas _chartCanvas;
    private readonly Random _random = new();

    #endregion

    #region Constructor

    public HoloCharacterStats()
    {
        InitializeComponent();
        InitializeTimers();
        InitializeDefaults();
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        _mainGrid = new Grid
        {
            Background = new SolidColorBrush(Color.FromArgb(20, 0, 150, 255))
        };

        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var header = CreateHeaderSection();
        Grid.SetRow(header, 0);
        _mainGrid.Children.Add(header);

        _tabControl = CreateTabControl();
        Grid.SetRow(_tabControl, 1);
        _mainGrid.Children.Add(_tabControl);

        _particleCanvas = new Canvas
        {
            IsHitTestVisible = false,
            Background = Brushes.Transparent
        };
        Grid.SetRowSpan(_particleCanvas, 2);
        _mainGrid.Children.Add(_particleCanvas);

        Content = _mainGrid;
    }

    private Border CreateHeaderSection()
    {
        var header = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromArgb(100, 0, 150, 255),
                Color.FromArgb(50, 0, 100, 200),
                90),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 0, 200, 255)),
            BorderThickness = new Thickness(0, 0, 0, 2),
            Padding = new Thickness(20, 15),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(100, 0, 150, 255),
                ShadowDepth = 0,
                BlurRadius = 20
            }
        };

        var stackPanel = new StackPanel();

        var titleBlock = new TextBlock
        {
            Text = "HOLOGRAPHIC CHARACTER STATISTICS",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Left,
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(150, 0, 255, 255),
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };

        var controlsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 10, 0, 0)
        };

        var characterSelector = CreateCharacterSelector();
        var categorySelector = CreateCategorySelector();
        var timeRangeSelector = CreateTimeRangeSelector();
        var refreshButton = CreateRefreshButton();

        controlsPanel.Children.Add(characterSelector);
        controlsPanel.Children.Add(categorySelector);
        controlsPanel.Children.Add(timeRangeSelector);
        controlsPanel.Children.Add(refreshButton);

        stackPanel.Children.Add(titleBlock);
        stackPanel.Children.Add(controlsPanel);

        header.Child = stackPanel;
        return header;
    }

    private ComboBox CreateCharacterSelector()
    {
        var comboBox = new ComboBox
        {
            Width = 150,
            Margin = new Thickness(0, 0, 15, 0),
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 50, 100)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 0, 200, 255))
        };

        comboBox.Items.Add("Main Character");
        comboBox.Items.Add("Alt Character");
        comboBox.Items.Add("Trading Alt");
        comboBox.SelectedIndex = 0;

        return comboBox;
    }

    private ComboBox CreateCategorySelector()
    {
        var comboBox = new ComboBox
        {
            Width = 120,
            Margin = new Thickness(0, 0, 15, 0),
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 50, 100)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 0, 200, 255))
        };

        comboBox.Items.Add("Overview");
        comboBox.Items.Add("Combat");
        comboBox.Items.Add("Industry");
        comboBox.Items.Add("Exploration");
        comboBox.Items.Add("Trading");
        comboBox.SelectedIndex = 0;

        return comboBox;
    }

    private ComboBox CreateTimeRangeSelector()
    {
        var comboBox = new ComboBox
        {
            Width = 100,
            Margin = new Thickness(0, 0, 15, 0),
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 50, 100)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 0, 200, 255))
        };

        comboBox.Items.Add("24 Hours");
        comboBox.Items.Add("7 Days");
        comboBox.Items.Add("30 Days");
        comboBox.Items.Add("All Time");
        comboBox.SelectedIndex = 2;

        return comboBox;
    }

    private Button CreateRefreshButton()
    {
        var button = new Button
        {
            Content = "REFRESH",
            Width = 80,
            Height = 30,
            Background = new LinearGradientBrush(
                Color.FromArgb(150, 0, 200, 100),
                Color.FromArgb(100, 0, 150, 50),
                90),
            Foreground = new SolidColorBrush(Colors.White),
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 0, 255, 100)),
            BorderThickness = new Thickness(1),
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(100, 0, 200, 100),
                ShadowDepth = 0,
                BlurRadius = 10
            }
        };

        button.Click += OnRefreshClicked;

        return button;
    }

    private TabControl CreateTabControl()
    {
        var tabControl = new TabControl
        {
            Background = Brushes.Transparent,
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 200, 255)),
            BorderThickness = new Thickness(1)
        };

        tabControl.Items.Add(CreateOverviewTab());
        tabControl.Items.Add(CreatePerformanceTab());
        tabControl.Items.Add(CreateTrendsTab());
        tabControl.Items.Add(CreateComparisonsTab());

        return tabControl;
    }

    private TabItem CreateOverviewTab()
    {
        var tab = new TabItem
        {
            Header = "OVERVIEW",
            Background = new SolidColorBrush(Color.FromArgb(80, 0, 100, 200)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };

        var grid = new Grid
        {
            Margin = new Thickness(20)
        };

        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var summaryCards = CreateSummaryCards();
        Grid.SetRow(summaryCards, 0);
        grid.Children.Add(summaryCards);

        var detailsGrid = CreateDetailsGrid();
        Grid.SetRow(detailsGrid, 1);
        grid.Children.Add(detailsGrid);

        tab.Content = grid;
        return tab;
    }

    private TabItem CreatePerformanceTab()
    {
        var tab = new TabItem
        {
            Header = "PERFORMANCE",
            Background = new SolidColorBrush(Color.FromArgb(80, 0, 100, 200)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };

        var scrollViewer = new ScrollViewer
        {
            Background = Brushes.Transparent,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        var stackPanel = new StackPanel
        {
            Margin = new Thickness(20)
        };

        var performanceMetrics = CreatePerformanceMetrics();
        foreach (var metric in performanceMetrics)
        {
            stackPanel.Children.Add(metric);
        }

        scrollViewer.Content = stackPanel;
        tab.Content = scrollViewer;
        return tab;
    }

    private TabItem CreateTrendsTab()
    {
        var tab = new TabItem
        {
            Header = "TRENDS",
            Background = new SolidColorBrush(Color.FromArgb(80, 0, 100, 200)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };

        var grid = new Grid
        {
            Margin = new Thickness(20)
        };

        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var trendControls = CreateTrendControls();
        Grid.SetRow(trendControls, 0);
        grid.Children.Add(trendControls);

        _chartCanvas = CreateTrendChart();
        Grid.SetRow(_chartCanvas, 1);
        grid.Children.Add(_chartCanvas);

        tab.Content = grid;
        return tab;
    }

    private TabItem CreateComparisonsTab()
    {
        var tab = new TabItem
        {
            Header = "COMPARISONS",
            Background = new SolidColorBrush(Color.FromArgb(80, 0, 100, 200)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };

        var grid = new Grid
        {
            Margin = new Thickness(20)
        };

        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var comparisonControls = CreateComparisonControls();
        Grid.SetRow(comparisonControls, 0);
        grid.Children.Add(comparisonControls);

        var comparisonDisplay = CreateComparisonDisplay();
        Grid.SetRow(comparisonDisplay, 1);
        grid.Children.Add(comparisonDisplay);

        tab.Content = grid;
        return tab;
    }

    private WrapPanel CreateSummaryCards()
    {
        var panel = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 20)
        };

        var cards = new[]
        {
            new { Title = "TOTAL SP", Value = "127,450,000", Change = "+2.3%", Color = Colors.Blue },
            new { Title = "ISK BALANCE", Value = "12.5B", Change = "+15.7%", Color = Colors.Gold },
            new { Title = "TOTAL ASSETS", Value = "45.2B", Change = "+8.4%", Color = Colors.Green },
            new { Title = "ACTIVE TIME", Value = "1,245h", Change = "+12.1%", Color = Colors.Orange },
            new { Title = "PVP KILLS", Value = "127", Change = "+5", Color = Colors.Red },
            new { Title = "SECURITY STATUS", Value = "+2.4", Change = "+0.1", Color = Colors.Cyan }
        };

        foreach (var card in cards)
        {
            var cardElement = CreateSummaryCard(card.Title, card.Value, card.Change, card.Color);
            panel.Children.Add(cardElement);
        }

        return panel;
    }

    private Border CreateSummaryCard(string title, string value, string change, Color color)
    {
        var border = new Border
        {
            Width = 180,
            Height = 120,
            Background = new LinearGradientBrush(
                Color.FromArgb(80, color.R, color.G, color.B),
                Color.FromArgb(40, color.R, color.G, color.B),
                90),
            BorderBrush = new SolidColorBrush(color),
            BorderThickness = new Thickness(2),
            Margin = new Thickness(10),
            Padding = new Thickness(15),
            CornerRadius = new CornerRadius(10),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(100, color.R, color.G, color.B),
                ShadowDepth = 0,
                BlurRadius = 15
            }
        };

        var stackPanel = new StackPanel();

        var titleText = new TextBlock
        {
            Text = title,
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(color),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 5)
        };

        var valueText = new TextBlock
        {
            Text = value,
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 5)
        };

        var changeText = new TextBlock
        {
            Text = change,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(change.StartsWith("+") ? Colors.LightGreen : Colors.LightCoral),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        stackPanel.Children.Add(titleText);
        stackPanel.Children.Add(valueText);
        stackPanel.Children.Add(changeText);

        border.Child = stackPanel;

        border.MouseEnter += (s, e) =>
        {
            var scaleTransform = new ScaleTransform(1.05, 1.05);
            border.RenderTransform = scaleTransform;
            border.RenderTransformOrigin = new Point(0.5, 0.5);
        };

        border.MouseLeave += (s, e) =>
        {
            border.RenderTransform = null;
        };

        return border;
    }

    private Grid CreateDetailsGrid()
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());

        var skillsPanel = CreateSkillsPanel();
        Grid.SetColumn(skillsPanel, 0);
        grid.Children.Add(skillsPanel);

        var activitiesPanel = CreateActivitiesPanel();
        Grid.SetColumn(activitiesPanel, 1);
        grid.Children.Add(activitiesPanel);

        return grid;
    }

    private Border CreateSkillsPanel()
    {
        var border = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromArgb(60, 0, 100, 200),
                Color.FromArgb(30, 0, 50, 100),
                90),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 0, 200, 255)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 10, 0),
            Padding = new Thickness(15),
            CornerRadius = new CornerRadius(8)
        };

        var stackPanel = new StackPanel();

        var title = new TextBlock
        {
            Text = "SKILL BREAKDOWN",
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            Margin = new Thickness(0, 0, 0, 15)
        };

        stackPanel.Children.Add(title);

        var skills = new[]
        {
            new { Category = "Spaceship Command", SP = "25,400,000", Percentage = 85 },
            new { Category = "Gunnery", SP = "18,200,000", Percentage = 72 },
            new { Category = "Engineering", SP = "15,800,000", Percentage = 68 },
            new { Category = "Navigation", SP = "12,500,000", Percentage = 55 },
            new { Category = "Electronic Systems", SP = "11,200,000", Percentage = 48 }
        };

        foreach (var skill in skills)
        {
            var skillItem = CreateSkillItem(skill.Category, skill.SP, skill.Percentage);
            stackPanel.Children.Add(skillItem);
        }

        border.Child = stackPanel;
        return border;
    }

    private Border CreateSkillItem(string category, string sp, int percentage)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 150, 255)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 0, 8),
            Padding = new Thickness(10, 8),
            CornerRadius = new CornerRadius(5)
        };

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition());
        grid.RowDefinitions.Add(new RowDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var categoryText = new TextBlock
        {
            Text = category,
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White)
        };
        Grid.SetRow(categoryText, 0);
        Grid.SetColumn(categoryText, 0);

        var spText = new TextBlock
        {
            Text = sp,
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Right
        };
        Grid.SetRow(spText, 0);
        Grid.SetColumn(spText, 1);

        var progressBar = CreateMiniProgressBar(percentage);
        Grid.SetRow(progressBar, 1);
        Grid.SetColumnSpan(progressBar, 2);

        grid.Children.Add(categoryText);
        grid.Children.Add(spText);
        grid.Children.Add(progressBar);

        border.Child = grid;
        return border;
    }

    private Border CreateMiniProgressBar(int percentage)
    {
        var container = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 30, 60)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 0, 100, 200)),
            BorderThickness = new Thickness(1),
            Height = 12,
            Margin = new Thickness(0, 5, 0, 0),
            CornerRadius = new CornerRadius(6)
        };

        var progressGrid = new Grid();

        var progressFill = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromArgb(200, 0, 200, 255),
                Color.FromArgb(150, 0, 150, 200),
                0),
            CornerRadius = new CornerRadius(6),
            HorizontalAlignment = HorizontalAlignment.Left,
            Width = Math.Max(12, 200 * percentage / 100)
        };

        progressGrid.Children.Add(progressFill);
        container.Child = progressGrid;

        return container;
    }

    private Border CreateActivitiesPanel()
    {
        var border = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromArgb(60, 0, 100, 200),
                Color.FromArgb(30, 0, 50, 100),
                90),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 0, 200, 255)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(10, 0, 0, 0),
            Padding = new Thickness(15),
            CornerRadius = new CornerRadius(8)
        };

        var stackPanel = new StackPanel();

        var title = new TextBlock
        {
            Text = "RECENT ACTIVITIES",
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            Margin = new Thickness(0, 0, 0, 15)
        };

        stackPanel.Children.Add(title);

        var activities = new[]
        {
            new { Activity = "Completed mission 'Worlds Collide'", Time = "2 hours ago", Type = "Mission" },
            new { Activity = "Destroyed Rifter in Tama", Time = "4 hours ago", Type = "PvP" },
            new { Activity = "Manufactured 50x Rifter", Time = "6 hours ago", Type = "Industry" },
            new { Activity = "Completed skill 'Caldari Frigate V'", Time = "8 hours ago", Type = "Training" },
            new { Activity = "Sold items worth 150M ISK", Time = "12 hours ago", Type = "Trading" }
        };

        foreach (var activity in activities)
        {
            var activityItem = CreateActivityItem(activity.Activity, activity.Time, activity.Type);
            stackPanel.Children.Add(activityItem);
        }

        border.Child = stackPanel;
        return border;
    }

    private Border CreateActivityItem(string activity, string time, string type)
    {
        var typeColor = GetActivityColor(type);

        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(typeColor),
            BorderThickness = new Thickness(1, 1, 3, 1),
            Margin = new Thickness(0, 0, 0, 8),
            Padding = new Thickness(10, 8),
            CornerRadius = new CornerRadius(5)
        };

        var stackPanel = new StackPanel();

        var activityText = new TextBlock
        {
            Text = activity,
            FontSize = 11,
            Foreground = new SolidColorBrush(Colors.White),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 3)
        };

        var timeText = new TextBlock
        {
            Text = time,
            FontSize = 9,
            Foreground = new SolidColorBrush(Color.FromArgb(150, 255, 255, 255))
        };

        stackPanel.Children.Add(activityText);
        stackPanel.Children.Add(timeText);

        border.Child = stackPanel;
        return border;
    }

    private List<Border> CreatePerformanceMetrics()
    {
        var metrics = new List<Border>();

        var performanceData = new[]
        {
            new { Title = "ISK Efficiency", Current = 85, Target = 90, Trend = "↗", Description = "ISK earned per hour played" },
            new { Title = "Skill Training Efficiency", Current = 92, Target = 95, Trend = "↗", Description = "Optimal skill queue management" },
            new { Title = "PvP Success Rate", Current = 68, Target = 75, Trend = "↘", Description = "Percentage of successful engagements" },
            new { Title = "Market Profit Margin", Current = 15, Target = 20, Trend = "↗", Description = "Average profit percentage on trades" },
            new { Title = "Mission Completion Rate", Current = 95, Target = 98, Trend = "→", Description = "Percentage of missions completed successfully" }
        };

        foreach (var data in performanceData)
        {
            var metric = CreatePerformanceMetric(data.Title, data.Current, data.Target, data.Trend, data.Description);
            metrics.Add(metric);
        }

        return metrics;
    }

    private Border CreatePerformanceMetric(string title, int current, int target, string trend, string description)
    {
        var trendColor = trend switch
        {
            "↗" => Colors.Green,
            "↘" => Colors.Red,
            "→" => Colors.Orange,
            _ => Colors.Gray
        };

        var border = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromArgb(60, 0, 100, 200),
                Color.FromArgb(30, 0, 50, 100),
                90),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 0, 200, 255)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 0, 15),
            Padding = new Thickness(20, 15),
            CornerRadius = new CornerRadius(8)
        };

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var titleText = new TextBlock
        {
            Text = title,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };
        Grid.SetRow(titleText, 0);
        Grid.SetColumn(titleText, 0);

        var trendText = new TextBlock
        {
            Text = trend,
            FontSize = 24,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(trendColor),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(trendText, 0);
        Grid.SetColumn(trendText, 1);
        Grid.SetRowSpan(trendText, 2);

        var valuesPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 8, 0, 0)
        };

        var currentText = new TextBlock
        {
            Text = $"Current: {current}%",
            FontSize = 12,
            Foreground = new SolidColorBrush(Colors.White),
            Margin = new Thickness(0, 0, 20, 0)
        };

        var targetText = new TextBlock
        {
            Text = $"Target: {target}%",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 100))
        };

        valuesPanel.Children.Add(currentText);
        valuesPanel.Children.Add(targetText);

        Grid.SetRow(valuesPanel, 1);
        Grid.SetColumn(valuesPanel, 0);

        var descriptionText = new TextBlock
        {
            Text = description,
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 8, 0, 0)
        };
        Grid.SetRow(descriptionText, 2);
        Grid.SetColumn(descriptionText, 0);

        grid.Children.Add(titleText);
        grid.Children.Add(trendText);
        grid.Children.Add(valuesPanel);
        grid.Children.Add(descriptionText);

        border.Child = grid;
        return border;
    }

    private StackPanel CreateTrendControls()
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 20)
        };

        var metricLabel = new TextBlock
        {
            Text = "METRIC:",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };

        var metricCombo = new ComboBox
        {
            Width = 150,
            Margin = new Thickness(0, 0, 20, 0),
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 50, 100)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };
        metricCombo.Items.Add("ISK Balance");
        metricCombo.Items.Add("Skill Points");
        metricCombo.Items.Add("PvP Kills");
        metricCombo.Items.Add("Trading Volume");
        metricCombo.SelectedIndex = 0;

        var periodLabel = new TextBlock
        {
            Text = "PERIOD:",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };

        var periodCombo = new ComboBox
        {
            Width = 120,
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 50, 100)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };
        periodCombo.Items.Add("Last 24 Hours");
        periodCombo.Items.Add("Last 7 Days");
        periodCombo.Items.Add("Last 30 Days");
        periodCombo.Items.Add("Last 3 Months");
        periodCombo.SelectedIndex = 2;

        panel.Children.Add(metricLabel);
        panel.Children.Add(metricCombo);
        panel.Children.Add(periodLabel);
        panel.Children.Add(periodCombo);

        return panel;
    }

    private Canvas CreateTrendChart()
    {
        var canvas = new Canvas
        {
            Background = new RadialGradientBrush(
                Color.FromArgb(40, 0, 100, 200),
                Color.FromArgb(20, 0, 50, 100))
        };

        CreateChartAxes(canvas);
        CreateTrendLine(canvas);
        CreateDataPoints(canvas);

        return canvas;
    }

    private void CreateChartAxes(Canvas canvas)
    {
        // X-axis
        var xAxis = new Line
        {
            X1 = 50,
            Y1 = 350,
            X2 = 550,
            Y2 = 350,
            Stroke = new SolidColorBrush(Color.FromArgb(150, 0, 200, 255)),
            StrokeThickness = 2
        };

        // Y-axis
        var yAxis = new Line
        {
            X1 = 50,
            Y1 = 50,
            X2 = 50,
            Y2 = 350,
            Stroke = new SolidColorBrush(Color.FromArgb(150, 0, 200, 255)),
            StrokeThickness = 2
        };

        canvas.Children.Add(xAxis);
        canvas.Children.Add(yAxis);

        // Add grid lines
        for (int i = 1; i <= 10; i++)
        {
            var gridLine = new Line
            {
                X1 = 50,
                Y1 = 50 + (i * 30),
                X2 = 550,
                Y2 = 50 + (i * 30),
                Stroke = new SolidColorBrush(Color.FromArgb(50, 0, 150, 255)),
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 5, 5 }
            };
            canvas.Children.Add(gridLine);
        }
    }

    private void CreateTrendLine(Canvas canvas)
    {
        var dataPoints = new[]
        {
            new Point(80, 300),
            new Point(150, 280),
            new Point(220, 250),
            new Point(290, 200),
            new Point(360, 180),
            new Point(430, 150),
            new Point(500, 120)
        };

        for (int i = 0; i < dataPoints.Length - 1; i++)
        {
            var line = new Line
            {
                X1 = dataPoints[i].X,
                Y1 = dataPoints[i].Y,
                X2 = dataPoints[i + 1].X,
                Y2 = dataPoints[i + 1].Y,
                Stroke = new SolidColorBrush(Color.FromArgb(200, 0, 255, 100)),
                StrokeThickness = 3,
                Effect = new DropShadowEffect
                {
                    Color = Color.FromArgb(150, 0, 255, 100),
                    ShadowDepth = 0,
                    BlurRadius = 8
                }
            };
            canvas.Children.Add(line);
        }
    }

    private void CreateDataPoints(Canvas canvas)
    {
        var dataPoints = new[]
        {
            new Point(80, 300),
            new Point(150, 280),
            new Point(220, 250),
            new Point(290, 200),
            new Point(360, 180),
            new Point(430, 150),
            new Point(500, 120)
        };

        foreach (var point in dataPoints)
        {
            var ellipse = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(Color.FromArgb(255, 0, 255, 150)),
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 2,
                Effect = new DropShadowEffect
                {
                    Color = Color.FromArgb(150, 0, 255, 150),
                    ShadowDepth = 0,
                    BlurRadius = 10
                }
            };

            Canvas.SetLeft(ellipse, point.X - 4);
            Canvas.SetTop(ellipse, point.Y - 4);
            canvas.Children.Add(ellipse);
        }
    }

    private StackPanel CreateComparisonControls()
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 20)
        };

        var character1Label = new TextBlock
        {
            Text = "CHARACTER 1:",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };

        var character1Combo = new ComboBox
        {
            Width = 120,
            Margin = new Thickness(0, 0, 20, 0),
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 50, 100)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };
        character1Combo.Items.Add("Main Character");
        character1Combo.Items.Add("Alt Character");
        character1Combo.Items.Add("Trading Alt");
        character1Combo.SelectedIndex = 0;

        var vsText = new TextBlock
        {
            Text = "VS",
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 20, 0)
        };

        var character2Label = new TextBlock
        {
            Text = "CHARACTER 2:",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };

        var character2Combo = new ComboBox
        {
            Width = 120,
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 50, 100)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };
        character2Combo.Items.Add("Main Character");
        character2Combo.Items.Add("Alt Character");
        character2Combo.Items.Add("Trading Alt");
        character2Combo.SelectedIndex = 1;

        panel.Children.Add(character1Label);
        panel.Children.Add(character1Combo);
        panel.Children.Add(vsText);
        panel.Children.Add(character2Label);
        panel.Children.Add(character2Combo);

        return panel;
    }

    private Grid CreateComparisonDisplay()
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
        grid.ColumnDefinitions.Add(new ColumnDefinition());

        var character1Panel = CreateCharacterComparisonPanel("MAIN CHARACTER", true);
        Grid.SetColumn(character1Panel, 0);
        grid.Children.Add(character1Panel);

        var character2Panel = CreateCharacterComparisonPanel("ALT CHARACTER", false);
        Grid.SetColumn(character2Panel, 2);
        grid.Children.Add(character2Panel);

        return grid;
    }

    private Border CreateCharacterComparisonPanel(string characterName, bool isMain)
    {
        var color = isMain ? Colors.Blue : Colors.Green;

        var border = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromArgb(60, color.R, color.G, color.B),
                Color.FromArgb(30, color.R, color.G, color.B),
                90),
            BorderBrush = new SolidColorBrush(color),
            BorderThickness = new Thickness(2),
            Margin = new Thickness(5),
            Padding = new Thickness(15),
            CornerRadius = new CornerRadius(8)
        };

        var stackPanel = new StackPanel();

        var title = new TextBlock
        {
            Text = characterName,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(color),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 15)
        };

        stackPanel.Children.Add(title);

        var stats = isMain ? new[]
        {
            new { Label = "Total SP", Value = "127,450,000" },
            new { Label = "ISK Balance", Value = "12.5B" },
            new { Label = "Total Assets", Value = "45.2B" },
            new { Label = "PvP Kills", Value = "127" },
            new { Label = "Security Status", Value = "+2.4" }
        } : new[]
        {
            new { Label = "Total SP", Value = "45,200,000" },
            new { Label = "ISK Balance", Value = "2.1B" },
            new { Label = "Total Assets", Value = "8.7B" },
            new { Label = "PvP Kills", Value = "23" },
            new { Label = "Security Status", Value = "+5.0" }
        };

        foreach (var stat in stats)
        {
            var statItem = CreateComparisonStatItem(stat.Label, stat.Value);
            stackPanel.Children.Add(statItem);
        }

        border.Child = stackPanel;
        return border;
    }

    private Border CreateComparisonStatItem(string label, string value)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 150, 255)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 0, 8),
            Padding = new Thickness(10, 8),
            CornerRadius = new CornerRadius(5)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());

        var labelText = new TextBlock
        {
            Text = label,
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255))
        };
        Grid.SetColumn(labelText, 0);

        var valueText = new TextBlock
        {
            Text = value,
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            HorizontalAlignment = HorizontalAlignment.Right
        };
        Grid.SetColumn(valueText, 1);

        grid.Children.Add(labelText);
        grid.Children.Add(valueText);

        border.Child = grid;
        return border;
    }

    private void InitializeTimers()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };
        _animationTimer.Tick += OnAnimationTick;

        _refreshTimer = new DispatcherTimer
        {
            Interval = RefreshInterval
        };
        _refreshTimer.Tick += OnRefreshTick;
    }

    private void InitializeDefaults()
    {
        CharacterStatistics = new HoloCharacterStatistics();
    }

    #endregion

    #region Animation Methods

    private void StartStatsPulseAnimations()
    {
        if (!EnableAnimations) return;

        // Add pulse animations to stat cards
    }

    #endregion

    #region Particle System

    private void CreateParticleSystem()
    {
        if (!EnableParticleEffects) return;

        var dataParticles = new ParticleSystem(_particleCanvas)
        {
            ParticleColor = Color.FromArgb(150, 0, 200, 255),
            ParticleSize = 2,
            ParticleSpeed = 1.0,
            EmissionRate = 3,
            ParticleLifespan = TimeSpan.FromSeconds(4)
        };

        var updateParticles = new ParticleSystem(_particleCanvas)
        {
            ParticleColor = Color.FromArgb(150, 0, 255, 100),
            ParticleSize = 3,
            ParticleSpeed = 1.5,
            EmissionRate = 2,
            ParticleLifespan = TimeSpan.FromSeconds(5)
        };

        _particleSystems.Add(dataParticles);
        _particleSystems.Add(updateParticles);
    }

    private void UpdateParticles()
    {
        if (!EnableParticleEffects) return;

        foreach (var system in _particleSystems)
        {
            system.Update();

            if (_random.NextDouble() < 0.15)
            {
                var x = _random.NextDouble() * ActualWidth;
                var y = _random.NextDouble() * ActualHeight;
                system.EmitParticle(new Point(x, y));
            }
        }
    }

    #endregion

    #region Helper Methods

    private Color GetActivityColor(string type)
    {
        return type switch
        {
            "Mission" => Colors.Blue,
            "PvP" => Colors.Red,
            "Industry" => Colors.Green,
            "Training" => Colors.Purple,
            "Trading" => Colors.Gold,
            _ => Colors.Gray
        };
    }

    private Color GetEVEColor(EVEColorScheme scheme)
    {
        return scheme switch
        {
            EVEColorScheme.ElectricBlue => Color.FromRgb(0, 200, 255),
            EVEColorScheme.CorporationGold => Color.FromRgb(255, 215, 0),
            EVEColorScheme.AllianceGreen => Color.FromRgb(0, 255, 100),
            EVEColorScheme.ConcordRed => Color.FromRgb(255, 50, 50),
            _ => Color.FromRgb(0, 200, 255)
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

        if (AutoRefresh)
        {
            _refreshTimer.Start();
        }

        CreateParticleSystem();
        StartStatsPulseAnimations();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        _refreshTimer?.Stop();

        foreach (var system in _particleSystems)
        {
            system.Dispose();
        }
        _particleSystems.Clear();
    }

    private void OnAnimationTick(object sender, EventArgs e)
    {
        UpdateParticles();
    }

    private void OnRefreshTick(object sender, EventArgs e)
    {
        RefreshStatistics();
    }

    private void OnRefreshClicked(object sender, RoutedEventArgs e)
    {
        RefreshStatistics();
    }

    private void RefreshStatistics()
    {
        // Refresh statistics from data sources
    }

    #endregion

    #region Property Changed Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterStats control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterStats control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnCharacterStatisticsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterStats control)
        {
            control.RefreshDisplay();
        }
    }

    private static void OnSelectedCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterStats control)
        {
            control.LoadCharacterStatistics();
        }
    }

    private static void OnStatsCategoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterStats control)
        {
            control.FilterStatsByCategory();
        }
    }

    private static void OnTimeRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterStats control)
        {
            control.UpdateTimeRange();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterStats control)
        {
            if ((bool)e.NewValue)
            {
                control._animationTimer?.Start();
                control.StartStatsPulseAnimations();
            }
            else
            {
                control._animationTimer?.Stop();
            }
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterStats control)
        {
            if (!(bool)e.NewValue)
            {
                foreach (var system in control._particleSystems)
                {
                    system.ClearParticles();
                }
            }
        }
    }

    private static void OnShowRealTimeUpdatesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterStats control)
        {
            control.UpdateRealTimeDisplay();
        }
    }

    private static void OnShowPerformanceMetricsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterStats control)
        {
            control.UpdatePerformanceDisplay();
        }
    }

    private static void OnShowTrendAnalysisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterStats control)
        {
            control.UpdateTrendDisplay();
        }
    }

    private static void OnShowComparisonsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterStats control)
        {
            control.UpdateComparisonDisplay();
        }
    }

    private static void OnAutoRefreshChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterStats control)
        {
            if ((bool)e.NewValue)
            {
                control._refreshTimer?.Start();
            }
            else
            {
                control._refreshTimer?.Stop();
            }
        }
    }

    private static void OnRefreshIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterStats control)
        {
            control._refreshTimer.Interval = (TimeSpan)e.NewValue;
        }
    }

    private void UpdateHolographicEffects()
    {
        // Update holographic intensity across all visual elements
    }

    private void UpdateColorScheme()
    {
        var color = GetEVEColor(EVEColorScheme);
        // Apply color scheme to all relevant elements
    }

    private void RefreshDisplay()
    {
        // Refresh the statistics display
    }

    private void LoadCharacterStatistics()
    {
        // Load statistics for the selected character
    }

    private void FilterStatsByCategory()
    {
        // Filter statistics based on selected category
    }

    private void UpdateTimeRange()
    {
        // Update the time range for statistics
    }

    private void UpdateRealTimeDisplay()
    {
        // Update real-time display settings
    }

    private void UpdatePerformanceDisplay()
    {
        // Update performance metrics display
    }

    private void UpdateTrendDisplay()
    {
        // Update trend analysis display
    }

    private void UpdateComparisonDisplay()
    {
        // Update character comparison display
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void AdaptToHardware(HardwareCapability capability)
    {
        switch (capability)
        {
            case HardwareCapability.Low:
                EnableAnimations = false;
                EnableParticleEffects = false;
                break;
            case HardwareCapability.Medium:
                EnableAnimations = true;
                EnableParticleEffects = false;
                break;
            case HardwareCapability.High:
                EnableAnimations = true;
                EnableParticleEffects = true;
                break;
        }
    }

    #endregion
}

#region Supporting Types

public class HoloCharacterStatistics
{
    public long TotalSkillPoints { get; set; }
    public decimal ISKBalance { get; set; }
    public decimal TotalAssets { get; set; }
    public int PvPKills { get; set; }
    public double SecurityStatus { get; set; }
    public TimeSpan TotalPlayTime { get; set; }
    public List<HoloSkillCategory> SkillBreakdown { get; set; } = new();
    public List<HoloActivity> RecentActivities { get; set; } = new();
    public List<HoloPerformanceMetric> PerformanceMetrics { get; set; } = new();
}

public class HoloSkillCategory
{
    public string Name { get; set; } = string.Empty;
    public long SkillPoints { get; set; }
    public int CompletionPercentage { get; set; }
}

public class HoloActivity
{
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}

public class HoloPerformanceMetric
{
    public string Name { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public double TargetValue { get; set; }
    public string Trend { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public enum StatsCategory
{
    Overview,
    Combat,
    Industry,
    Exploration,
    Trading,
    Social
}

public enum StatsTimeRange
{
    Last24Hours,
    Last7Days,
    Last30Days,
    Last3Months,
    AllTime
}

public enum EVEColorScheme
{
    ElectricBlue,
    CorporationGold,
    AllianceGreen,
    ConcordRed
}

public enum HardwareCapability
{
    Low,
    Medium,
    High
}

public interface IAnimationIntensityTarget
{
    double HolographicIntensity { get; set; }
}

public interface IAdaptiveControl
{
    void AdaptToHardware(HardwareCapability capability);
}

#endregion