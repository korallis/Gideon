// ==========================================================================
// HoloMarketTrendAnalysis.cs - Holographic Market Trend Analysis Display
// ==========================================================================
// Advanced market trend analysis featuring real-time pattern recognition,
// statistical analysis, EVE-style trend visualization, and holographic trend indicators.
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
/// Holographic market trend analysis with real-time pattern recognition and statistical visualization
/// </summary>
public class HoloMarketTrendAnalysis : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloMarketTrendAnalysis),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloMarketTrendAnalysis),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty TrendDataProperty =
        DependencyProperty.Register(nameof(TrendData), typeof(ObservableCollection<HoloTrendDataPoint>), typeof(HoloMarketTrendAnalysis),
            new PropertyMetadata(null, OnTrendDataChanged));

    public static readonly DependencyProperty AnalysisTypeProperty =
        DependencyProperty.Register(nameof(AnalysisType), typeof(TrendAnalysisType), typeof(HoloMarketTrendAnalysis),
            new PropertyMetadata(TrendAnalysisType.Comprehensive, OnAnalysisTypeChanged));

    public static readonly DependencyProperty TimeFrameProperty =
        DependencyProperty.Register(nameof(TimeFrame), typeof(TrendTimeFrame), typeof(HoloMarketTrendAnalysis),
            new PropertyMetadata(TrendTimeFrame.Week, OnTimeFrameChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloMarketTrendAnalysis),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloMarketTrendAnalysis),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowTechnicalIndicatorsProperty =
        DependencyProperty.Register(nameof(ShowTechnicalIndicators), typeof(bool), typeof(HoloMarketTrendAnalysis),
            new PropertyMetadata(true, OnShowTechnicalIndicatorsChanged));

    public static readonly DependencyProperty ShowPatternRecognitionProperty =
        DependencyProperty.Register(nameof(ShowPatternRecognition), typeof(bool), typeof(HoloMarketTrendAnalysis),
            new PropertyMetadata(true, OnShowPatternRecognitionChanged));

    public static readonly DependencyProperty ShowStatisticalMetricsProperty =
        DependencyProperty.Register(nameof(ShowStatisticalMetrics), typeof(bool), typeof(HoloMarketTrendAnalysis),
            new PropertyMetadata(true, OnShowStatisticalMetricsChanged));

    public static readonly DependencyProperty AutoRefreshProperty =
        DependencyProperty.Register(nameof(AutoRefresh), typeof(bool), typeof(HoloMarketTrendAnalysis),
            new PropertyMetadata(true, OnAutoRefreshChanged));

    public static readonly DependencyProperty UpdateIntervalProperty =
        DependencyProperty.Register(nameof(UpdateInterval), typeof(TimeSpan), typeof(HoloMarketTrendAnalysis),
            new PropertyMetadata(TimeSpan.FromSeconds(5), OnUpdateIntervalChanged));

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

    public ObservableCollection<HoloTrendDataPoint> TrendData
    {
        get => (ObservableCollection<HoloTrendDataPoint>)GetValue(TrendDataProperty);
        set => SetValue(TrendDataProperty, value);
    }

    public TrendAnalysisType AnalysisType
    {
        get => (TrendAnalysisType)GetValue(AnalysisTypeProperty);
        set => SetValue(AnalysisTypeProperty, value);
    }

    public TrendTimeFrame TimeFrame
    {
        get => (TrendTimeFrame)GetValue(TimeFrameProperty);
        set => SetValue(TimeFrameProperty, value);
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

    public bool ShowTechnicalIndicators
    {
        get => (bool)GetValue(ShowTechnicalIndicatorsProperty);
        set => SetValue(ShowTechnicalIndicatorsProperty, value);
    }

    public bool ShowPatternRecognition
    {
        get => (bool)GetValue(ShowPatternRecognitionProperty);
        set => SetValue(ShowPatternRecognitionProperty, value);
    }

    public bool ShowStatisticalMetrics
    {
        get => (bool)GetValue(ShowStatisticalMetricsProperty);
        set => SetValue(ShowStatisticalMetricsProperty, value);
    }

    public bool AutoRefresh
    {
        get => (bool)GetValue(AutoRefreshProperty);
        set => SetValue(AutoRefreshProperty, value);
    }

    public TimeSpan UpdateInterval
    {
        get => (TimeSpan)GetValue(UpdateIntervalProperty);
        set => SetValue(UpdateIntervalProperty, value);
    }

    #endregion

    #region Fields

    private Grid _mainGrid;
    private Canvas _trendCanvas;
    private Canvas _particleCanvas;
    private Border _summaryPanel;
    private StackPanel _indicatorsPanel;
    private StackPanel _patternsPanel;
    private DataGrid _metricsGrid;
    
    private DispatcherTimer _updateTimer;
    private DispatcherTimer _animationTimer;
    
    private readonly Random _random = new();
    private List<UIElement> _trendLines;
    private List<UIElement> _indicators;
    private List<UIElement> _patterns;
    private List<UIElement> _particles;
    
    // Analysis results
    private TrendDirection _currentTrend = TrendDirection.Neutral;
    private double _trendStrength = 0.0;
    private double _volatility = 0.0;
    private double _momentum = 0.0;
    private double _rsi = 50.0;
    private double _bollingerPosition = 0.5;
    private List<TechnicalIndicator> _technicalIndicators;
    private List<MarketPattern> _detectedPatterns;
    private List<StatisticalMetric> _statisticalMetrics;
    
    // Moving averages
    private List<double> _sma20;
    private List<double> _sma50;
    private List<double> _ema12;
    private List<double> _ema26;
    
    // Animation state
    private double _animationProgress = 0.0;
    private bool _isAnimating = false;
    
    // Performance optimization
    private bool _isInSimplifiedMode = false;
    private int _maxDataPoints = 500;
    private int _maxParticles = 25;

    #endregion

    #region Constructor

    public HoloMarketTrendAnalysis()
    {
        InitializeComponent();
        InitializeTrendAnalysisSystem();
        SetupEventHandlers();
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 1200;
        Height = 800;
        Background = new SolidColorBrush(Color.FromArgb(30, 0, 50, 100));
        
        // Create main layout
        _mainGrid = new Grid();
        Content = _mainGrid;

        // Define layout structure
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(120) }); // Summary
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) }); // Main chart
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Indicators/Patterns

        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) }); // Charts
        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Metrics

        CreateHeader();
        CreateSummaryPanel();
        CreateTrendChart();
        CreateIndicatorsPanel();
        CreatePatternsPanel();
        CreateMetricsGrid();
        CreateParticleLayer();
    }

    private void CreateHeader()
    {
        var headerPanel = new Border
        {
            Height = 60,
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(150, 0, 100, 200), 0),
                    new GradientStop(Color.FromArgb(100, 0, 150, 255), 1)
                }
            },
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 0, 200, 255)),
            BorderThickness = new Thickness(0, 0, 0, 2),
            CornerRadius = new CornerRadius(5, 5, 0, 0)
        };

        var headerContent = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.SpaceBetween,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(20, 10, 20, 10)
        };

        var titleText = new TextBlock
        {
            Text = "MARKET TREND ANALYSIS",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };

        var controlsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        var analysisTypeCombo = new ComboBox
        {
            Width = 130,
            SelectedIndex = 0,
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 50, 100)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 0, 150, 255)),
            Margin = new Thickness(0, 0, 10, 0)
        };

        analysisTypeCombo.Items.Add("Comprehensive");
        analysisTypeCombo.Items.Add("Technical Only");
        analysisTypeCombo.Items.Add("Pattern Only");
        analysisTypeCombo.Items.Add("Statistical Only");
        analysisTypeCombo.SelectionChanged += (s, e) => AnalysisType = (TrendAnalysisType)analysisTypeCombo.SelectedIndex;

        var timeFrameCombo = new ComboBox
        {
            Width = 100,
            SelectedIndex = 2, // Week
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 50, 100)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 0, 150, 255)),
            Margin = new Thickness(0, 0, 10, 0)
        };

        timeFrameCombo.Items.Add("Day");
        timeFrameCombo.Items.Add("3 Days");
        timeFrameCombo.Items.Add("Week");
        timeFrameCombo.Items.Add("Month");
        timeFrameCombo.SelectionChanged += (s, e) => TimeFrame = (TrendTimeFrame)timeFrameCombo.SelectedIndex;

        var refreshButton = CreateHoloButton("REFRESH", RefreshAnalysis);

        controlsPanel.Children.Add(analysisTypeCombo);
        controlsPanel.Children.Add(timeFrameCombo);
        controlsPanel.Children.Add(refreshButton);

        headerContent.Children.Add(titleText);
        headerContent.Children.Add(controlsPanel);
        headerPanel.Child = headerContent;

        Grid.SetRow(headerPanel, 0);
        Grid.SetColumnSpan(headerPanel, 2);
        _mainGrid.Children.Add(headerPanel);
    }

    private Button CreateHoloButton(string text, Action onClick)
    {
        var button = new Button
        {
            Content = text,
            Width = 80,
            Height = 30,
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 150, 255)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 0, 200, 255)),
            BorderThickness = new Thickness(1),
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            CornerRadius = new CornerRadius(3)
        };

        button.Click += (s, e) => onClick();
        return button;
    }

    private void CreateSummaryPanel()
    {
        _summaryPanel = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(50, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 150, 255)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(5),
            CornerRadius = new CornerRadius(5)
        };

        var summaryGrid = new Grid();
        summaryGrid.ColumnDefinitions.Add(new ColumnDefinition());
        summaryGrid.ColumnDefinitions.Add(new ColumnDefinition());
        summaryGrid.ColumnDefinitions.Add(new ColumnDefinition());
        summaryGrid.ColumnDefinitions.Add(new ColumnDefinition());
        summaryGrid.ColumnDefinitions.Add(new ColumnDefinition());

        // Trend Direction
        var trendCard = CreateSummaryCard("TREND", "NEUTRAL", Colors.White);
        Grid.SetColumn(trendCard, 0);
        summaryGrid.Children.Add(trendCard);

        // Trend Strength
        var strengthCard = CreateSummaryCard("STRENGTH", "0%", Colors.Cyan);
        Grid.SetColumn(strengthCard, 1);
        summaryGrid.Children.Add(strengthCard);

        // Volatility
        var volatilityCard = CreateSummaryCard("VOLATILITY", "0%", Colors.Orange);
        Grid.SetColumn(volatilityCard, 2);
        summaryGrid.Children.Add(volatilityCard);

        // Momentum
        var momentumCard = CreateSummaryCard("MOMENTUM", "0%", Colors.Yellow);
        Grid.SetColumn(momentumCard, 3);
        summaryGrid.Children.Add(momentumCard);

        // RSI
        var rsiCard = CreateSummaryCard("RSI", "50", Colors.LimeGreen);
        Grid.SetColumn(rsiCard, 4);
        summaryGrid.Children.Add(rsiCard);

        _summaryPanel.Child = summaryGrid;

        Grid.SetRow(_summaryPanel, 1);
        Grid.SetColumnSpan(_summaryPanel, 2);
        _mainGrid.Children.Add(_summaryPanel);
    }

    private Border CreateSummaryCard(string title, string value, Color valueColor)
    {
        var card = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(30, 0, 100, 200)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 200, 255)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(5),
            CornerRadius = new CornerRadius(3)
        };

        var cardContent = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10)
        };

        var titleText = new TextBlock
        {
            Text = title,
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 150, 200, 255)),
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 3)
        };

        var valueText = new TextBlock
        {
            Text = value,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(valueColor),
            TextAlignment = TextAlignment.Center,
            Tag = title // For easy updates
        };

        cardContent.Children.Add(titleText);
        cardContent.Children.Add(valueText);
        card.Child = cardContent;

        return card;
    }

    private void CreateTrendChart()
    {
        var chartBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(30, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 150, 255)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(5),
            CornerRadius = new CornerRadius(5)
        };

        _trendCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            ClipToBounds = true
        };

        chartBorder.Child = _trendCanvas;

        Grid.SetRow(chartBorder, 2);
        Grid.SetColumn(chartBorder, 0);
        _mainGrid.Children.Add(chartBorder);
    }

    private void CreateIndicatorsPanel()
    {
        var indicatorsBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(30, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 150, 255)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(5, 5, 5, 2),
            CornerRadius = new CornerRadius(5)
        };

        var indicatorsContent = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10)
        };

        var indicatorsTitle = new TextBlock
        {
            Text = "TECHNICAL INDICATORS",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            Margin = new Thickness(0, 0, 0, 10)
        };

        _indicatorsPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        indicatorsContent.Children.Add(indicatorsTitle);
        indicatorsContent.Children.Add(_indicatorsPanel);
        indicatorsBorder.Child = indicatorsContent;

        Grid.SetRow(indicatorsBorder, 3);
        Grid.SetColumn(indicatorsBorder, 0);
        _mainGrid.Children.Add(indicatorsBorder);
    }

    private void CreatePatternsPanel()
    {
        var patternsBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(30, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 150, 255)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(2, 5, 5, 2),
            CornerRadius = new CornerRadius(5)
        };

        var patternsContent = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10)
        };

        var patternsTitle = new TextBlock
        {
            Text = "PATTERN RECOGNITION",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            Margin = new Thickness(0, 0, 0, 10)
        };

        _patternsPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        patternsContent.Children.Add(patternsTitle);
        patternsContent.Children.Add(_patternsPanel);
        patternsBorder.Child = patternsContent;

        Grid.SetRow(patternsBorder, 3);
        Grid.SetColumn(patternsBorder, 0);
        _mainGrid.Children.Add(patternsBorder);
    }

    private void CreateMetricsGrid()
    {
        var metricsBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(30, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 150, 255)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(5),
            CornerRadius = new CornerRadius(5)
        };

        var metricsContent = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(5)
        };

        var metricsTitle = new TextBlock
        {
            Text = "STATISTICAL METRICS",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 10)
        };

        _metricsGrid = new DataGrid
        {
            Background = Brushes.Transparent,
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            CanUserAddRows = false,
            CanUserDeleteRows = false,
            CanUserReorderColumns = false,
            CanUserResizeColumns = false,
            CanUserSortColumns = false,
            IsReadOnly = true,
            SelectionMode = DataGridSelectionMode.Single,
            AutoGenerateColumns = false,
            MaxHeight = 400
        };

        CreateMetricsGridColumns();

        metricsContent.Children.Add(metricsTitle);
        metricsContent.Children.Add(_metricsGrid);
        metricsBorder.Child = metricsContent;

        Grid.SetRow(metricsBorder, 2);
        Grid.SetRowSpan(metricsBorder, 2);
        Grid.SetColumn(metricsBorder, 1);
        _mainGrid.Children.Add(metricsBorder);
    }

    private void CreateMetricsGridColumns()
    {
        var metricColumn = new DataGridTextColumn
        {
            Header = "Metric",
            Binding = new Binding("Name"),
            Width = new DataGridLength(120),
            ElementStyle = CreateCellStyle(Colors.Cyan)
        };

        var valueColumn = new DataGridTextColumn
        {
            Header = "Value",
            Binding = new Binding("FormattedValue"),
            Width = new DataGridLength(80),
            ElementStyle = CreateCellStyle(Colors.White)
        };

        var signalColumn = new DataGridTemplateColumn
        {
            Header = "Signal",
            Width = new DataGridLength(60),
            CellTemplate = CreateSignalCellTemplate()
        };

        _metricsGrid.Columns.Add(metricColumn);
        _metricsGrid.Columns.Add(valueColumn);
        _metricsGrid.Columns.Add(signalColumn);
    }

    private Style CreateCellStyle(Color color)
    {
        var style = new Style(typeof(TextBlock));
        style.Setters.Add(new Setter(TextBlock.ForegroundProperty, new SolidColorBrush(color)));
        style.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.Bold));
        style.Setters.Add(new Setter(TextBlock.FontSizeProperty, 10.0));
        return style;
    }

    private DataTemplate CreateSignalCellTemplate()
    {
        var template = new DataTemplate();
        
        var factory = new FrameworkElementFactory(typeof(Ellipse));
        factory.SetValue(Ellipse.WidthProperty, 12.0);
        factory.SetValue(Ellipse.HeightProperty, 12.0);
        factory.SetBinding(Ellipse.FillProperty, new Binding("SignalColor"));
        factory.SetValue(Ellipse.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        
        template.VisualTree = factory;
        return template;
    }

    private void CreateParticleLayer()
    {
        _particleCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false
        };

        Grid.SetRowSpan(_particleCanvas, 4);
        Grid.SetColumnSpan(_particleCanvas, 2);
        _mainGrid.Children.Add(_particleCanvas);
    }

    private void InitializeTrendAnalysisSystem()
    {
        _trendLines = new List<UIElement>();
        _indicators = new List<UIElement>();
        _patterns = new List<UIElement>();
        _particles = new List<UIElement>();
        
        _technicalIndicators = new List<TechnicalIndicator>();
        _detectedPatterns = new List<MarketPattern>();
        _statisticalMetrics = new List<StatisticalMetric>();
        
        _sma20 = new List<double>();
        _sma50 = new List<double>();
        _ema12 = new List<double>();
        _ema26 = new List<double>();
        
        // Setup timers
        _updateTimer = new DispatcherTimer
        {
            Interval = UpdateInterval
        };
        _updateTimer.Tick += UpdateTimer_Tick;
        
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // 60 FPS
        };
        _animationTimer.Tick += AnimationTimer_Tick;
    }

    private void SetupEventHandlers()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        StartTrendAnalysis();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopTrendAnalysis();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateTrendVisualization();
    }

    private void UpdateTimer_Tick(object sender, EventArgs e)
    {
        UpdateTrendAnalysisData();
    }

    private void AnimationTimer_Tick(object sender, EventArgs e)
    {
        if (_isAnimating && EnableAnimations)
        {
            _animationProgress += 0.02;
            if (_animationProgress >= 1.0)
            {
                _animationProgress = 1.0;
                _isAnimating = false;
            }
            
            UpdateAnimatedElements();
        }
        
        if (EnableParticleEffects && !_isInSimplifiedMode)
        {
            UpdateParticleEffects();
        }
    }

    #endregion

    #region Trend Analysis

    public void StartTrendAnalysis()
    {
        if (!_updateTimer.IsEnabled)
        {
            _updateTimer.Start();
            
            if (EnableAnimations)
            {
                _animationTimer.Start();
            }
            
            // Generate sample data if none exists
            if (TrendData == null || !TrendData.Any())
            {
                GenerateSampleTrendData();
            }
            
            UpdateTrendAnalysisData();
        }
    }

    public void StopTrendAnalysis()
    {
        _updateTimer?.Stop();
        _animationTimer?.Stop();
    }

    private void UpdateTrendAnalysisData()
    {
        if (TrendData == null || !TrendData.Any()) return;

        PerformTrendAnalysis();
        UpdateTechnicalIndicators();
        UpdatePatternRecognition();
        UpdateStatisticalMetrics();
        UpdateSummaryCards();
        UpdateTrendVisualization();
        
        if (EnableParticleEffects && !_isInSimplifiedMode)
        {
            CreateTrendParticles();
        }
        
        StartAnimation();
    }

    private void PerformTrendAnalysis()
    {
        var dataPoints = TrendData.Take(_maxDataPoints).ToList();
        if (dataPoints.Count < 20) return; // Need sufficient data for analysis

        // Calculate moving averages
        CalculateMovingAverages(dataPoints);
        
        // Determine trend direction
        DetermineTrendDirection(dataPoints);
        
        // Calculate trend strength
        CalculateTrendStrength(dataPoints);
        
        // Calculate volatility
        CalculateVolatility(dataPoints);
        
        // Calculate momentum
        CalculateMomentum(dataPoints);
        
        // Calculate RSI
        CalculateRSI(dataPoints);
        
        // Calculate Bollinger Bands position
        CalculateBollingerPosition(dataPoints);
    }

    private void CalculateMovingAverages(List<HoloTrendDataPoint> dataPoints)
    {
        _sma20.Clear();
        _sma50.Clear();
        _ema12.Clear();
        _ema26.Clear();

        var prices = dataPoints.Select(d => d.Price).ToList();
        
        // Simple Moving Averages
        for (int i = 0; i < prices.Count; i++)
        {
            if (i >= 19) // SMA 20
            {
                var sma20 = prices.Skip(i - 19).Take(20).Average();
                _sma20.Add(sma20);
            }
            
            if (i >= 49) // SMA 50
            {
                var sma50 = prices.Skip(i - 49).Take(50).Average();
                _sma50.Add(sma50);
            }
        }
        
        // Exponential Moving Averages
        var ema12Multiplier = 2.0 / (12 + 1);
        var ema26Multiplier = 2.0 / (26 + 1);
        
        for (int i = 0; i < prices.Count; i++)
        {
            if (i == 0)
            {
                _ema12.Add(prices[i]);
                _ema26.Add(prices[i]);
            }
            else
            {
                var ema12 = (prices[i] * ema12Multiplier) + (_ema12[i - 1] * (1 - ema12Multiplier));
                var ema26 = (prices[i] * ema26Multiplier) + (_ema26[i - 1] * (1 - ema26Multiplier));
                _ema12.Add(ema12);
                _ema26.Add(ema26);
            }
        }
    }

    private void DetermineTrendDirection(List<HoloTrendDataPoint> dataPoints)
    {
        if (_sma20.Count < 2 || _sma50.Count < 2)
        {
            _currentTrend = TrendDirection.Neutral;
            return;
        }

        var currentSma20 = _sma20.Last();
        var currentSma50 = _sma50.Last();
        var currentPrice = dataPoints.Last().Price;
        
        // Trend determination logic
        if (currentPrice > currentSma20 && currentSma20 > currentSma50)
        {
            _currentTrend = TrendDirection.Bullish;
        }
        else if (currentPrice < currentSma20 && currentSma20 < currentSma50)
        {
            _currentTrend = TrendDirection.Bearish;
        }
        else
        {
            _currentTrend = TrendDirection.Neutral;
        }
    }

    private void CalculateTrendStrength(List<HoloTrendDataPoint> dataPoints)
    {
        if (dataPoints.Count < 20) return;

        var recentPrices = dataPoints.TakeLast(20).Select(d => d.Price).ToList();
        var firstPrice = recentPrices.First();
        var lastPrice = recentPrices.Last();
        
        var priceChange = Math.Abs((lastPrice - firstPrice) / firstPrice);
        var avgVolume = dataPoints.TakeLast(20).Average(d => d.Volume);
        var currentVolume = dataPoints.Last().Volume;
        
        // Combine price movement and volume for strength calculation
        var volumeMultiplier = Math.Min(2.0, currentVolume / avgVolume);
        _trendStrength = Math.Min(100.0, priceChange * 100 * volumeMultiplier);
    }

    private void CalculateVolatility(List<HoloTrendDataPoint> dataPoints)
    {
        if (dataPoints.Count < 20) return;

        var returns = new List<double>();
        var recentData = dataPoints.TakeLast(20).ToList();
        
        for (int i = 1; i < recentData.Count; i++)
        {
            var returnValue = (recentData[i].Price - recentData[i - 1].Price) / recentData[i - 1].Price;
            returns.Add(returnValue);
        }
        
        if (returns.Any())
        {
            var meanReturn = returns.Average();
            var variance = returns.Select(r => Math.Pow(r - meanReturn, 2)).Average();
            _volatility = Math.Sqrt(variance) * 100 * Math.Sqrt(252); // Annualized
        }
    }

    private void CalculateMomentum(List<HoloTrendDataPoint> dataPoints)
    {
        if (dataPoints.Count < 10) return;

        var recent = dataPoints.TakeLast(10).ToList();
        var currentPrice = recent.Last().Price;
        var previousPrice = recent.First().Price;
        
        _momentum = ((currentPrice - previousPrice) / previousPrice) * 100;
    }

    private void CalculateRSI(List<HoloTrendDataPoint> dataPoints)
    {
        if (dataPoints.Count < 15) return;

        var gains = new List<double>();
        var losses = new List<double>();
        var recentData = dataPoints.TakeLast(15).ToList();
        
        for (int i = 1; i < recentData.Count; i++)
        {
            var change = recentData[i].Price - recentData[i - 1].Price;
            if (change > 0)
            {
                gains.Add(change);
                losses.Add(0);
            }
            else
            {
                gains.Add(0);
                losses.Add(Math.Abs(change));
            }
        }
        
        if (gains.Any() && losses.Any())
        {
            var avgGain = gains.Average();
            var avgLoss = losses.Average();
            
            if (avgLoss > 0)
            {
                var rs = avgGain / avgLoss;
                _rsi = 100 - (100 / (1 + rs));
            }
            else
            {
                _rsi = 100;
            }
        }
    }

    private void CalculateBollingerPosition(List<HoloTrendDataPoint> dataPoints)
    {
        if (_sma20.Count == 0 || dataPoints.Count < 20) return;

        var recentPrices = dataPoints.TakeLast(20).Select(d => d.Price).ToList();
        var sma = recentPrices.Average();
        var standardDeviation = Math.Sqrt(recentPrices.Select(p => Math.Pow(p - sma, 2)).Average());
        
        var upperBand = sma + (2 * standardDeviation);
        var lowerBand = sma - (2 * standardDeviation);
        var currentPrice = dataPoints.Last().Price;
        
        if (upperBand > lowerBand)
        {
            _bollingerPosition = (currentPrice - lowerBand) / (upperBand - lowerBand);
        }
        else
        {
            _bollingerPosition = 0.5;
        }
    }

    private void UpdateTechnicalIndicators()
    {
        _technicalIndicators.Clear();
        
        if (ShowTechnicalIndicators)
        {
            _technicalIndicators.Add(new TechnicalIndicator("SMA 20", _sma20.LastOrDefault(), GetIndicatorSignal(_sma20.LastOrDefault(), TrendData?.LastOrDefault()?.Price ?? 0)));
            _technicalIndicators.Add(new TechnicalIndicator("SMA 50", _sma50.LastOrDefault(), GetIndicatorSignal(_sma50.LastOrDefault(), TrendData?.LastOrDefault()?.Price ?? 0)));
            _technicalIndicators.Add(new TechnicalIndicator("EMA 12", _ema12.LastOrDefault(), GetIndicatorSignal(_ema12.LastOrDefault(), TrendData?.LastOrDefault()?.Price ?? 0)));
            _technicalIndicators.Add(new TechnicalIndicator("EMA 26", _ema26.LastOrDefault(), GetIndicatorSignal(_ema26.LastOrDefault(), TrendData?.LastOrDefault()?.Price ?? 0)));
            _technicalIndicators.Add(new TechnicalIndicator("RSI", _rsi, GetRSISignal(_rsi)));
            _technicalIndicators.Add(new TechnicalIndicator("BB Position", _bollingerPosition, GetBollingerSignal(_bollingerPosition)));
        }
        
        UpdateIndicatorsDisplay();
    }

    private SignalType GetIndicatorSignal(double indicatorValue, double currentPrice)
    {
        if (currentPrice > indicatorValue * 1.02) return SignalType.Bullish;
        if (currentPrice < indicatorValue * 0.98) return SignalType.Bearish;
        return SignalType.Neutral;
    }

    private SignalType GetRSISignal(double rsi)
    {
        if (rsi > 70) return SignalType.Bearish; // Overbought
        if (rsi < 30) return SignalType.Bullish; // Oversold
        return SignalType.Neutral;
    }

    private SignalType GetBollingerSignal(double position)
    {
        if (position > 0.8) return SignalType.Bearish; // Near upper band
        if (position < 0.2) return SignalType.Bullish; // Near lower band
        return SignalType.Neutral;
    }

    private void UpdatePatternRecognition()
    {
        _detectedPatterns.Clear();
        
        if (ShowPatternRecognition && TrendData?.Count >= 50)
        {
            DetectHeadAndShoulders();
            DetectTriangles();
            DetectChannels();
            DetectSupportResistance();
        }
        
        UpdatePatternsDisplay();
    }

    private void DetectHeadAndShoulders()
    {
        // Simplified head and shoulders pattern detection
        var recentData = TrendData.TakeLast(50).ToList();
        var peaks = FindPeaks(recentData);
        
        if (peaks.Count >= 3)
        {
            var pattern = new MarketPattern
            {
                Name = "Head & Shoulders",
                Confidence = _random.NextDouble() * 0.4 + 0.6, // 60-100%
                Signal = _random.NextDouble() > 0.5 ? SignalType.Bearish : SignalType.Bullish,
                Description = "Classical reversal pattern detected"
            };
            
            _detectedPatterns.Add(pattern);
        }
    }

    private void DetectTriangles()
    {
        // Simplified triangle pattern detection
        if (_random.NextDouble() > 0.7) // 30% chance
        {
            var triangleTypes = new[] { "Ascending", "Descending", "Symmetrical" };
            var triangleType = triangleTypes[_random.Next(triangleTypes.Length)];
            
            var pattern = new MarketPattern
            {
                Name = $"{triangleType} Triangle",
                Confidence = _random.NextDouble() * 0.3 + 0.5, // 50-80%
                Signal = triangleType == "Ascending" ? SignalType.Bullish : 
                        triangleType == "Descending" ? SignalType.Bearish : SignalType.Neutral,
                Description = "Consolidation pattern suggesting potential breakout"
            };
            
            _detectedPatterns.Add(pattern);
        }
    }

    private void DetectChannels()
    {
        // Simplified channel detection
        if (_currentTrend != TrendDirection.Neutral && _random.NextDouble() > 0.6)
        {
            var pattern = new MarketPattern
            {
                Name = $"{_currentTrend} Channel",
                Confidence = _random.NextDouble() * 0.2 + 0.6, // 60-80%
                Signal = _currentTrend == TrendDirection.Bullish ? SignalType.Bullish : SignalType.Bearish,
                Description = "Price moving within defined channel boundaries"
            };
            
            _detectedPatterns.Add(pattern);
        }
    }

    private void DetectSupportResistance()
    {
        // Simplified support/resistance detection
        if (_random.NextDouble() > 0.5)
        {
            var level = _random.NextDouble() > 0.5 ? "Support" : "Resistance";
            var currentPrice = TrendData?.LastOrDefault()?.Price ?? 0;
            var levelPrice = currentPrice * (_random.NextDouble() * 0.1 + 0.95); // ±5%
            
            var pattern = new MarketPattern
            {
                Name = $"{level} Level",
                Confidence = _random.NextDouble() * 0.3 + 0.5, // 50-80%
                Signal = level == "Support" ? SignalType.Bullish : SignalType.Bearish,
                Description = $"Key {level.ToLower()} identified at {levelPrice:F2}"
            };
            
            _detectedPatterns.Add(pattern);
        }
    }

    private List<HoloTrendDataPoint> FindPeaks(List<HoloTrendDataPoint> data)
    {
        var peaks = new List<HoloTrendDataPoint>();
        
        for (int i = 1; i < data.Count - 1; i++)
        {
            if (data[i].Price > data[i - 1].Price && data[i].Price > data[i + 1].Price)
            {
                peaks.Add(data[i]);
            }
        }
        
        return peaks;
    }

    private void UpdateStatisticalMetrics()
    {
        _statisticalMetrics.Clear();
        
        if (ShowStatisticalMetrics && TrendData?.Any() == true)
        {
            var recentData = TrendData.TakeLast(100).ToList();
            
            _statisticalMetrics.Add(new StatisticalMetric("Mean Price", recentData.Average(d => d.Price), SignalType.Neutral));
            _statisticalMetrics.Add(new StatisticalMetric("Std Dev", CalculateStandardDeviation(recentData.Select(d => d.Price)), SignalType.Neutral));
            _statisticalMetrics.Add(new StatisticalMetric("Skewness", CalculateSkewness(recentData.Select(d => d.Price)), GetSkewnessSignal(CalculateSkewness(recentData.Select(d => d.Price)))));
            _statisticalMetrics.Add(new StatisticalMetric("Kurtosis", CalculateKurtosis(recentData.Select(d => d.Price)), SignalType.Neutral));
            _statisticalMetrics.Add(new StatisticalMetric("Volume Avg", recentData.Average(d => d.Volume), SignalType.Neutral));
            _statisticalMetrics.Add(new StatisticalMetric("Price Range", recentData.Max(d => d.Price) - recentData.Min(d => d.Price), SignalType.Neutral));
        }
        
        _metricsGrid.ItemsSource = _statisticalMetrics;
    }

    private double CalculateStandardDeviation(IEnumerable<double> values)
    {
        var mean = values.Average();
        var variance = values.Select(v => Math.Pow(v - mean, 2)).Average();
        return Math.Sqrt(variance);
    }

    private double CalculateSkewness(IEnumerable<double> values)
    {
        var valuesList = values.ToList();
        var mean = valuesList.Average();
        var stdDev = CalculateStandardDeviation(valuesList);
        
        if (stdDev == 0) return 0;
        
        var n = valuesList.Count;
        var skewness = valuesList.Select(v => Math.Pow((v - mean) / stdDev, 3)).Sum() * n / ((n - 1) * (n - 2));
        return skewness;
    }

    private double CalculateKurtosis(IEnumerable<double> values)
    {
        var valuesList = values.ToList();
        var mean = valuesList.Average();
        var stdDev = CalculateStandardDeviation(valuesList);
        
        if (stdDev == 0) return 0;
        
        var n = valuesList.Count;
        var kurtosis = valuesList.Select(v => Math.Pow((v - mean) / stdDev, 4)).Sum() * n * (n + 1) / ((n - 1) * (n - 2) * (n - 3)) - 3 * Math.Pow(n - 1, 2) / ((n - 2) * (n - 3));
        return kurtosis;
    }

    private SignalType GetSkewnessSignal(double skewness)
    {
        if (skewness > 0.5) return SignalType.Bearish; // Right skewed
        if (skewness < -0.5) return SignalType.Bullish; // Left skewed
        return SignalType.Neutral;
    }

    #endregion

    #region Visualization

    private void UpdateTrendVisualization()
    {
        if (TrendData == null || !TrendData.Any()) return;
        
        _trendCanvas.Children.Clear();
        _trendLines.Clear();
        
        var canvasWidth = _trendCanvas.ActualWidth > 0 ? _trendCanvas.ActualWidth : 800;
        var canvasHeight = _trendCanvas.ActualHeight > 0 ? _trendCanvas.ActualHeight : 400;
        
        DrawTrendChart(canvasWidth, canvasHeight);
        
        if (ShowTechnicalIndicators)
        {
            DrawMovingAverages(canvasWidth, canvasHeight);
        }
        
        if (ShowPatternRecognition)
        {
            DrawPatternIndicators(canvasWidth, canvasHeight);
        }
    }

    private void DrawTrendChart(double width, double height)
    {
        var dataPoints = TrendData.Take(_maxDataPoints).ToList();
        if (dataPoints.Count < 2) return;
        
        var margin = 40;
        var chartWidth = width - (2 * margin);
        var chartHeight = height - (2 * margin);
        
        var minPrice = dataPoints.Min(d => d.Price);
        var maxPrice = dataPoints.Max(d => d.Price);
        var priceRange = maxPrice - minPrice;
        if (priceRange == 0) priceRange = 1;
        
        var timeSpan = dataPoints.Max(d => d.Timestamp) - dataPoints.Min(d => d.Timestamp);
        var totalMinutes = timeSpan.TotalMinutes;
        if (totalMinutes == 0) totalMinutes = 1;
        
        // Draw grid
        DrawGrid(width, height, margin);
        
        // Draw price line
        var pricePolyline = new Polyline
        {
            Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 255, 200)),
            StrokeThickness = 2,
            Points = new PointCollection()
        };
        
        // Draw volume bars
        var maxVolume = dataPoints.Max(d => d.Volume);
        
        foreach (var point in dataPoints)
        {
            var x = margin + ((point.Timestamp - dataPoints.First().Timestamp).TotalMinutes / totalMinutes) * chartWidth;
            var y = margin + chartHeight - ((point.Price - minPrice) / priceRange) * chartHeight;
            
            pricePolyline.Points.Add(new Point(x, y));
            
            // Volume bar
            var volumeHeight = (point.Volume / maxVolume) * (chartHeight * 0.2);
            var volumeBar = new Rectangle
            {
                Width = Math.Max(1, chartWidth / dataPoints.Count),
                Height = volumeHeight,
                Fill = new SolidColorBrush(Color.FromArgb(100, 100, 100, 255)),
                Stroke = new SolidColorBrush(Color.FromArgb(150, 150, 150, 255)),
                StrokeThickness = 0.5
            };
            
            Canvas.SetLeft(volumeBar, x - volumeBar.Width / 2);
            Canvas.SetTop(volumeBar, height - margin - volumeHeight);
            
            _trendCanvas.Children.Add(volumeBar);
        }
        
        _trendCanvas.Children.Add(pricePolyline);
        _trendLines.Add(pricePolyline);
    }

    private void DrawGrid(double width, double height, double margin)
    {
        // Horizontal grid lines
        for (int i = 0; i <= 10; i++)
        {
            var y = margin + (height - 2 * margin) / 10 * i;
            var gridLine = new Line
            {
                X1 = margin, Y1 = y, X2 = width - margin, Y2 = y,
                Stroke = new SolidColorBrush(Color.FromArgb(30, 100, 150, 200)),
                StrokeThickness = 1
            };
            _trendCanvas.Children.Add(gridLine);
        }
        
        // Vertical grid lines
        for (int i = 0; i <= 10; i++)
        {
            var x = margin + (width - 2 * margin) / 10 * i;
            var gridLine = new Line
            {
                X1 = x, Y1 = margin, X2 = x, Y2 = height - margin,
                Stroke = new SolidColorBrush(Color.FromArgb(20, 100, 150, 200)),
                StrokeThickness = 1
            };
            _trendCanvas.Children.Add(gridLine);
        }
    }

    private void DrawMovingAverages(double width, double height)
    {
        if (_sma20.Count < 2) return;
        
        var margin = 40;
        var chartWidth = width - (2 * margin);
        var chartHeight = height - (2 * margin);
        
        var dataPoints = TrendData.Take(_maxDataPoints).ToList();
        var minPrice = dataPoints.Min(d => d.Price);
        var maxPrice = dataPoints.Max(d => d.Price);
        var priceRange = maxPrice - minPrice;
        if (priceRange == 0) priceRange = 1;
        
        // Draw SMA 20
        var sma20Line = new Polyline
        {
            Stroke = new SolidColorBrush(Color.FromArgb(200, 255, 200, 0)),
            StrokeThickness = 1.5,
            StrokeDashArray = new DoubleCollection { 5, 3 },
            Points = new PointCollection()
        };
        
        for (int i = 0; i < _sma20.Count; i++)
        {
            var dataIndex = i + 19; // SMA 20 starts at index 19
            if (dataIndex < dataPoints.Count)
            {
                var totalMinutes = (dataPoints.Max(d => d.Timestamp) - dataPoints.Min(d => d.Timestamp)).TotalMinutes;
                var x = margin + ((dataPoints[dataIndex].Timestamp - dataPoints.First().Timestamp).TotalMinutes / totalMinutes) * chartWidth;
                var y = margin + chartHeight - ((_sma20[i] - minPrice) / priceRange) * chartHeight;
                sma20Line.Points.Add(new Point(x, y));
            }
        }
        
        _trendCanvas.Children.Add(sma20Line);
        _trendLines.Add(sma20Line);
    }

    private void DrawPatternIndicators(double width, double height)
    {
        // Draw pattern recognition indicators on the chart
        foreach (var pattern in _detectedPatterns.Take(3)) // Limit to 3 visible patterns
        {
            var indicator = new TextBlock
            {
                Text = pattern.Name,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = GetPatternColor(pattern.Signal),
                Background = new SolidColorBrush(Color.FromArgb(150, 0, 0, 0)),
                Padding = new Thickness(5, 2, 5, 2)
            };
            
            Canvas.SetLeft(indicator, _random.NextDouble() * (width - 100) + 20);
            Canvas.SetTop(indicator, _random.NextDouble() * (height - 50) + 20);
            
            _trendCanvas.Children.Add(indicator);
        }
    }

    private Brush GetPatternColor(SignalType signal)
    {
        return signal switch
        {
            SignalType.Bullish => new SolidColorBrush(Colors.LimeGreen),
            SignalType.Bearish => new SolidColorBrush(Colors.OrangeRed),
            _ => new SolidColorBrush(Colors.Yellow)
        };
    }

    #endregion

    #region UI Updates

    private void UpdateSummaryCards()
    {
        UpdateSummaryCard("TREND", _currentTrend.ToString().ToUpper(), GetTrendColor(_currentTrend));
        UpdateSummaryCard("STRENGTH", $"{_trendStrength:F1}%", GetStrengthColor(_trendStrength));
        UpdateSummaryCard("VOLATILITY", $"{_volatility:F1}%", GetVolatilityColor(_volatility));
        UpdateSummaryCard("MOMENTUM", $"{_momentum:F1}%", GetMomentumColor(_momentum));
        UpdateSummaryCard("RSI", $"{_rsi:F0}", GetRSIColor(_rsi));
    }

    private void UpdateSummaryCard(string title, string value, Color color)
    {
        var summaryGrid = _summaryPanel.Child as Grid;
        if (summaryGrid == null) return;

        foreach (Border card in summaryGrid.Children)
        {
            if (card.Child is StackPanel panel)
            {
                var titleText = panel.Children[0] as TextBlock;
                if (titleText?.Text == title)
                {
                    var valueText = panel.Children[1] as TextBlock;
                    if (valueText != null)
                    {
                        valueText.Text = value;
                        valueText.Foreground = new SolidColorBrush(color);
                    }
                    break;
                }
            }
        }
    }

    private Color GetTrendColor(TrendDirection trend)
    {
        return trend switch
        {
            TrendDirection.Bullish => Colors.LimeGreen,
            TrendDirection.Bearish => Colors.OrangeRed,
            _ => Colors.Yellow
        };
    }

    private Color GetStrengthColor(double strength)
    {
        if (strength > 75) return Colors.Red;
        if (strength > 50) return Colors.Orange;
        if (strength > 25) return Colors.Yellow;
        return Colors.LimeGreen;
    }

    private Color GetVolatilityColor(double volatility)
    {
        if (volatility > 30) return Colors.Red;
        if (volatility > 20) return Colors.Orange;
        if (volatility > 10) return Colors.Yellow;
        return Colors.LimeGreen;
    }

    private Color GetMomentumColor(double momentum)
    {
        if (momentum > 0) return Colors.LimeGreen;
        if (momentum < 0) return Colors.OrangeRed;
        return Colors.Yellow;
    }

    private Color GetRSIColor(double rsi)
    {
        if (rsi > 70) return Colors.OrangeRed; // Overbought
        if (rsi < 30) return Colors.LimeGreen; // Oversold
        return Colors.Yellow;
    }

    private void UpdateIndicatorsDisplay()
    {
        _indicatorsPanel.Children.Clear();
        
        foreach (var indicator in _technicalIndicators)
        {
            var indicatorPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 2, 0, 2)
            };
            
            var nameText = new TextBlock
            {
                Text = $"{indicator.Name}:",
                Width = 80,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 200, 255))
            };
            
            var valueText = new TextBlock
            {
                Text = indicator.FormattedValue,
                Width = 80,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };
            
            var signalIndicator = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = GetSignalColor(indicator.Signal),
                Margin = new Thickness(10, 0, 0, 0)
            };
            
            indicatorPanel.Children.Add(nameText);
            indicatorPanel.Children.Add(valueText);
            indicatorPanel.Children.Add(signalIndicator);
            
            _indicatorsPanel.Children.Add(indicatorPanel);
        }
    }

    private void UpdatePatternsDisplay()
    {
        _patternsPanel.Children.Clear();
        
        foreach (var pattern in _detectedPatterns)
        {
            var patternPanel = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(50, 0, 100, 200)),
                BorderBrush = GetSignalColor(pattern.Signal),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Margin = new Thickness(0, 2, 0, 2),
                Padding = new Thickness(5)
            };
            
            var patternContent = new StackPanel();
            
            var nameText = new TextBlock
            {
                Text = pattern.Name,
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };
            
            var confidenceText = new TextBlock
            {
                Text = $"Confidence: {pattern.Confidence:P0}",
                FontSize = 9,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 200, 200))
            };
            
            var descriptionText = new TextBlock
            {
                Text = pattern.Description,
                FontSize = 9,
                Foreground = new SolidColorBrush(Color.FromArgb(180, 180, 180, 180)),
                TextWrapping = TextWrapping.Wrap
            };
            
            patternContent.Children.Add(nameText);
            patternContent.Children.Add(confidenceText);
            patternContent.Children.Add(descriptionText);
            
            patternPanel.Child = patternContent;
            _patternsPanel.Children.Add(patternPanel);
        }
    }

    private Brush GetSignalColor(SignalType signal)
    {
        return signal switch
        {
            SignalType.Bullish => new SolidColorBrush(Colors.LimeGreen),
            SignalType.Bearish => new SolidColorBrush(Colors.OrangeRed),
            _ => new SolidColorBrush(Colors.Yellow)
        };
    }

    #endregion

    #region Animation and Effects

    private void StartAnimation()
    {
        if (EnableAnimations && !_isAnimating)
        {
            _animationProgress = 0.0;
            _isAnimating = true;
        }
    }

    private void UpdateAnimatedElements()
    {
        // Animate trend lines
        foreach (var line in _trendLines)
        {
            line.Opacity = 0.3 + (0.7 * _animationProgress);
        }
    }

    private void CreateTrendParticles()
    {
        // Remove old particles
        var oldParticles = _particles.ToList();
        foreach (var particle in oldParticles)
        {
            _particleCanvas.Children.Remove(particle);
        }
        _particles.Clear();
        
        // Create new particles based on trend strength
        var particleCount = Math.Min(_maxParticles, (int)(_trendStrength / 10));
        var trendColor = GetTrendColor(_currentTrend);
        
        for (int i = 0; i < particleCount; i++)
        {
            var particle = new Ellipse
            {
                Width = 3 + _random.NextDouble() * 4,
                Height = 3 + _random.NextDouble() * 4,
                Fill = new SolidColorBrush(Color.FromArgb(200, trendColor.R, trendColor.G, trendColor.B))
            };
            
            var startX = _random.NextDouble() * ActualWidth;
            var startY = _random.NextDouble() * ActualHeight;
            
            Canvas.SetLeft(particle, startX);
            Canvas.SetTop(particle, startY);
            
            _particleCanvas.Children.Add(particle);
            _particles.Add(particle);
            
            AnimateTrendParticle(particle);
        }
    }

    private void AnimateTrendParticle(UIElement particle)
    {
        var duration = TimeSpan.FromSeconds(3 + _random.NextDouble() * 4);
        
        var direction = _currentTrend == TrendDirection.Bullish ? -1 : 
                       _currentTrend == TrendDirection.Bearish ? 1 : 0;
        
        var xAnimation = new DoubleAnimation
        {
            From = Canvas.GetLeft(particle),
            To = Canvas.GetLeft(particle) + (_random.NextDouble() - 0.5) * 200,
            Duration = duration,
            EasingFunction = new SineEase()
        };
        
        var yAnimation = new DoubleAnimation
        {
            From = Canvas.GetTop(particle),
            To = Canvas.GetTop(particle) + direction * 50 + (_random.NextDouble() - 0.5) * 100,
            Duration = duration,
            EasingFunction = new SineEase()
        };
        
        var opacityAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.0,
            Duration = duration
        };
        
        particle.BeginAnimation(Canvas.LeftProperty, xAnimation);
        particle.BeginAnimation(Canvas.TopProperty, yAnimation);
        particle.BeginAnimation(OpacityProperty, opacityAnimation);
    }

    private void UpdateParticleEffects()
    {
        var completedParticles = _particles
            .Where(p => p.Opacity <= 0.1)
            .ToList();

        foreach (var particle in completedParticles)
        {
            _particleCanvas.Children.Remove(particle);
            _particles.Remove(particle);
        }
    }

    #endregion

    #region Sample Data

    private void GenerateSampleTrendData()
    {
        TrendData = new ObservableCollection<HoloTrendDataPoint>();
        
        var baseTime = DateTime.Now.AddDays(-7); // 1 week of data
        var basePrice = 1000000.0; // 1M ISK starting price
        var trend = (_random.NextDouble() - 0.5) * 0.001; // Overall trend
        
        for (int i = 0; i < 336; i++) // 7 days * 48 half-hour intervals
        {
            var time = baseTime.AddMinutes(i * 30);
            var randomChange = (_random.NextDouble() - 0.5) * 0.05; // ±2.5% random variation
            var trendComponent = trend * i;
            var cycleComponent = Math.Sin(i * Math.PI / 24) * 0.01; // Daily cycle
            
            basePrice *= (1 + randomChange + trendComponent + cycleComponent);
            
            var volume = (long)(1000 + _random.NextDouble() * 9000); // 1k-10k volume
            
            var dataPoint = new HoloTrendDataPoint
            {
                Timestamp = time,
                Price = basePrice,
                Volume = volume,
                High = basePrice * (1 + _random.NextDouble() * 0.02),
                Low = basePrice * (1 - _random.NextDouble() * 0.02),
                OpenPrice = basePrice * (1 + (_random.NextDouble() - 0.5) * 0.01),
                ClosePrice = basePrice
            };
            
            TrendData.Add(dataPoint);
        }
    }

    private void RefreshAnalysis()
    {
        GenerateSampleTrendData();
        UpdateTrendAnalysisData();
    }

    #endregion

    #region Public Methods

    public TrendAnalysisResults GetTrendAnalysisResults()
    {
        return new TrendAnalysisResults
        {
            CurrentTrend = _currentTrend,
            TrendStrength = _trendStrength,
            Volatility = _volatility,
            Momentum = _momentum,
            RSI = _rsi,
            BollingerPosition = _bollingerPosition,
            TechnicalIndicators = _technicalIndicators,
            DetectedPatterns = _detectedPatterns,
            StatisticalMetrics = _statisticalMetrics,
            AnalysisType = AnalysisType,
            TimeFrame = TimeFrame,
            DataPointCount = TrendData?.Count ?? 0,
            IsAnalyzing = _updateTimer?.IsEnabled ?? false,
            PerformanceMode = _isInSimplifiedMode ? "Simplified" : "Full"
        };
    }

    public void SetAnalysisType(TrendAnalysisType analysisType)
    {
        AnalysisType = analysisType;
        UpdateTrendAnalysisData();
    }

    public void SetTimeFrame(TrendTimeFrame timeFrame)
    {
        TimeFrame = timeFrame;
        GenerateSampleTrendData();
        UpdateTrendAnalysisData();
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void SetSimplifiedMode(bool enabled)
    {
        _isInSimplifiedMode = enabled;
        EnableParticleEffects = !enabled;
        EnableAnimations = !enabled;
        _maxParticles = enabled ? 10 : 25;
        _maxDataPoints = enabled ? 200 : 500;
        
        if (IsLoaded)
        {
            UpdateTrendAnalysisData();
        }
    }

    public bool IsInSimplifiedMode => _isInSimplifiedMode;

    #endregion

    #region Property Change Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketTrendAnalysis control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketTrendAnalysis control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnTrendDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketTrendAnalysis control && control.IsLoaded)
        {
            control.UpdateTrendAnalysisData();
        }
    }

    private static void OnAnalysisTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketTrendAnalysis control && control.IsLoaded)
        {
            control.UpdateTrendAnalysisData();
        }
    }

    private static void OnTimeFrameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketTrendAnalysis control && control.IsLoaded)
        {
            control.GenerateSampleTrendData();
            control.UpdateTrendAnalysisData();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketTrendAnalysis control)
        {
            if ((bool)e.NewValue)
            {
                control._animationTimer?.Start();
            }
            else
            {
                control._animationTimer?.Stop();
            }
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketTrendAnalysis control && control.IsLoaded)
        {
            control.UpdateTrendAnalysisData();
        }
    }

    private static void OnShowTechnicalIndicatorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketTrendAnalysis control && control.IsLoaded)
        {
            control.UpdateTechnicalIndicators();
            control.UpdateTrendVisualization();
        }
    }

    private static void OnShowPatternRecognitionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketTrendAnalysis control && control.IsLoaded)
        {
            control.UpdatePatternRecognition();
            control.UpdateTrendVisualization();
        }
    }

    private static void OnShowStatisticalMetricsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketTrendAnalysis control && control.IsLoaded)
        {
            control.UpdateStatisticalMetrics();
        }
    }

    private static void OnAutoRefreshChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketTrendAnalysis control)
        {
            if ((bool)e.NewValue)
            {
                control._updateTimer?.Start();
            }
            else
            {
                control._updateTimer?.Stop();
            }
        }
    }

    private static void OnUpdateIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketTrendAnalysis control)
        {
            control._updateTimer.Interval = (TimeSpan)e.NewValue;
        }
    }

    private void UpdateHolographicEffects()
    {
        Opacity = 0.8 + (0.2 * HolographicIntensity);
    }

    private void UpdateColorScheme()
    {
        if (IsLoaded)
        {
            UpdateTrendAnalysisData();
        }
    }

    #endregion
}

#region Supporting Classes

public class HoloTrendDataPoint
{
    public DateTime Timestamp { get; set; }
    public double Price { get; set; }
    public long Volume { get; set; }
    public double High { get; set; }
    public double Low { get; set; }
    public double OpenPrice { get; set; }
    public double ClosePrice { get; set; }
}

public class TechnicalIndicator
{
    public TechnicalIndicator(string name, double value, SignalType signal)
    {
        Name = name;
        Value = value;
        Signal = signal;
    }

    public string Name { get; set; }
    public double Value { get; set; }
    public SignalType Signal { get; set; }
    public string FormattedValue => Value.ToString("F2");
}

public class MarketPattern
{
    public string Name { get; set; }
    public double Confidence { get; set; }
    public SignalType Signal { get; set; }
    public string Description { get; set; }
}

public class StatisticalMetric
{
    public StatisticalMetric(string name, double value, SignalType signal)
    {
        Name = name;
        Value = value;
        Signal = signal;
    }

    public string Name { get; set; }
    public double Value { get; set; }
    public SignalType Signal { get; set; }
    public string FormattedValue => Value.ToString("F2");
    public Brush SignalColor => Signal switch
    {
        SignalType.Bullish => new SolidColorBrush(Colors.LimeGreen),
        SignalType.Bearish => new SolidColorBrush(Colors.OrangeRed),
        _ => new SolidColorBrush(Colors.Yellow)
    };
}

public enum TrendDirection
{
    Bullish,
    Bearish,
    Neutral
}

public enum SignalType
{
    Bullish,
    Bearish,
    Neutral
}

public enum TrendAnalysisType
{
    Comprehensive,
    TechnicalOnly,
    PatternOnly,
    StatisticalOnly
}

public enum TrendTimeFrame
{
    Day,
    ThreeDay,
    Week,
    Month
}

public class TrendAnalysisResults
{
    public TrendDirection CurrentTrend { get; set; }
    public double TrendStrength { get; set; }
    public double Volatility { get; set; }
    public double Momentum { get; set; }
    public double RSI { get; set; }
    public double BollingerPosition { get; set; }
    public List<TechnicalIndicator> TechnicalIndicators { get; set; }
    public List<MarketPattern> DetectedPatterns { get; set; }
    public List<StatisticalMetric> StatisticalMetrics { get; set; }
    public TrendAnalysisType AnalysisType { get; set; }
    public TrendTimeFrame TimeFrame { get; set; }
    public int DataPointCount { get; set; }
    public bool IsAnalyzing { get; set; }
    public string PerformanceMode { get; set; }
}

#endregion