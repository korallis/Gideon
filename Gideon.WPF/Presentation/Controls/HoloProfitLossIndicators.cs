// ==========================================================================
// HoloProfitLossIndicators.cs - Holographic Profit/Loss Indicators
// ==========================================================================
// Advanced profit/loss visualization featuring real-time P&L analysis,
// portfolio tracking, EVE-style financial data, and holographic performance indicators.
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
/// Holographic profit/loss indicators with real-time financial analysis and performance visualization
/// </summary>
public class HoloProfitLossIndicators : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloProfitLossIndicators),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloProfitLossIndicators),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty TradingPositionsProperty =
        DependencyProperty.Register(nameof(TradingPositions), typeof(ObservableCollection<HoloTradingPosition>), typeof(HoloProfitLossIndicators),
            new PropertyMetadata(null, OnTradingPositionsChanged));

    public static readonly DependencyProperty PortfolioSummaryProperty =
        DependencyProperty.Register(nameof(PortfolioSummary), typeof(HoloPortfolioSummary), typeof(HoloProfitLossIndicators),
            new PropertyMetadata(null, OnPortfolioSummaryChanged));

    public static readonly DependencyProperty TimeFrameProperty =
        DependencyProperty.Register(nameof(TimeFrame), typeof(ProfitLossTimeFrame), typeof(HoloProfitLossIndicators),
            new PropertyMetadata(ProfitLossTimeFrame.Daily, OnTimeFrameChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloProfitLossIndicators),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloProfitLossIndicators),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowPerformanceChartsProperty =
        DependencyProperty.Register(nameof(ShowPerformanceCharts), typeof(bool), typeof(HoloProfitLossIndicators),
            new PropertyMetadata(true, OnShowPerformanceChartsChanged));

    public static readonly DependencyProperty ShowRiskMetricsProperty =
        DependencyProperty.Register(nameof(ShowRiskMetrics), typeof(bool), typeof(HoloProfitLossIndicators),
            new PropertyMetadata(true, OnShowRiskMetricsChanged));

    public static readonly DependencyProperty ShowPortfolioBreakdownProperty =
        DependencyProperty.Register(nameof(ShowPortfolioBreakdown), typeof(bool), typeof(HoloProfitLossIndicators),
            new PropertyMetadata(true, OnShowPortfolioBreakdownChanged));

    public static readonly DependencyProperty AutoRefreshProperty =
        DependencyProperty.Register(nameof(AutoRefresh), typeof(bool), typeof(HoloProfitLossIndicators),
            new PropertyMetadata(true, OnAutoRefreshChanged));

    public static readonly DependencyProperty UpdateIntervalProperty =
        DependencyProperty.Register(nameof(UpdateInterval), typeof(TimeSpan), typeof(HoloProfitLossIndicators),
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

    public ObservableCollection<HoloTradingPosition> TradingPositions
    {
        get => (ObservableCollection<HoloTradingPosition>)GetValue(TradingPositionsProperty);
        set => SetValue(TradingPositionsProperty, value);
    }

    public HoloPortfolioSummary PortfolioSummary
    {
        get => (HoloPortfolioSummary)GetValue(PortfolioSummaryProperty);
        set => SetValue(PortfolioSummaryProperty, value);
    }

    public ProfitLossTimeFrame TimeFrame
    {
        get => (ProfitLossTimeFrame)GetValue(TimeFrameProperty);
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

    public bool ShowPerformanceCharts
    {
        get => (bool)GetValue(ShowPerformanceChartsProperty);
        set => SetValue(ShowPerformanceChartsProperty, value);
    }

    public bool ShowRiskMetrics
    {
        get => (bool)GetValue(ShowRiskMetricsProperty);
        set => SetValue(ShowRiskMetricsProperty, value);
    }

    public bool ShowPortfolioBreakdown
    {
        get => (bool)GetValue(ShowPortfolioBreakdownProperty);
        set => SetValue(ShowPortfolioBreakdownProperty, value);
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
    private Canvas _performanceCanvas;
    private Canvas _particleCanvas;
    private Border _summaryPanel;
    private StackPanel _metricsPanel;
    private DataGrid _positionsGrid;
    private Canvas _chartCanvas;
    
    private DispatcherTimer _updateTimer;
    private DispatcherTimer _animationTimer;
    
    private readonly Random _random = new();
    private List<UIElement> _performanceIndicators;
    private List<UIElement> _particles;
    private List<UIElement> _chartElements;
    
    // Financial calculations
    private double _totalProfitLoss = 0;
    private double _totalInvested = 0;
    private double _totalValue = 0;
    private double _dayProfitLoss = 0;
    private double _weekProfitLoss = 0;
    private double _monthProfitLoss = 0;
    private double _returnOnInvestment = 0;
    private double _sharpeRatio = 0;
    private double _maxDrawdown = 0;
    private double _volatility = 0;
    
    // Animation state
    private double _animationProgress = 0.0;
    private bool _isAnimating = false;
    private readonly Dictionary<string, double> _previousValues = new();
    
    // Performance optimization
    private bool _isInSimplifiedMode = false;
    private int _maxParticles = 30;

    #endregion

    #region Constructor

    public HoloProfitLossIndicators()
    {
        InitializeComponent();
        InitializeProfitLossSystem();
        SetupEventHandlers();
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 1000;
        Height = 700;
        Background = new SolidColorBrush(Color.FromArgb(30, 0, 50, 100));
        
        // Create main layout
        _mainGrid = new Grid();
        Content = _mainGrid;

        // Define layout structure
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(200) }); // Summary panel
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Charts/positions
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(150) }); // Metrics panel

        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }); // Charts
        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Positions

        CreateHeader();
        CreateSummaryPanel();
        CreatePerformanceCharts();
        CreatePositionsGrid();
        CreateMetricsPanel();
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
            Text = "PROFIT/LOSS ANALYSIS",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };

        var controlsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        var timeFrameCombo = new ComboBox
        {
            Width = 100,
            SelectedIndex = 1, // Daily
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 50, 100)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 0, 150, 255)),
            Margin = new Thickness(0, 0, 10, 0)
        };

        timeFrameCombo.Items.Add("Hourly");
        timeFrameCombo.Items.Add("Daily");
        timeFrameCombo.Items.Add("Weekly");
        timeFrameCombo.Items.Add("Monthly");
        timeFrameCombo.SelectionChanged += (s, e) => TimeFrame = (ProfitLossTimeFrame)timeFrameCombo.SelectedIndex;

        var refreshButton = CreateHoloButton("REFRESH", RefreshData);

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

        // Total P&L
        var totalPLPanel = CreateSummaryCard("TOTAL P&L", "0.00 ISK", Colors.White);
        Grid.SetColumn(totalPLPanel, 0);
        summaryGrid.Children.Add(totalPLPanel);

        // Daily P&L
        var dailyPLPanel = CreateSummaryCard("TODAY", "0.00 ISK", Colors.LimeGreen);
        Grid.SetColumn(dailyPLPanel, 1);
        summaryGrid.Children.Add(dailyPLPanel);

        // ROI
        var roiPanel = CreateSummaryCard("ROI", "0.00%", Colors.Cyan);
        Grid.SetColumn(roiPanel, 2);
        summaryGrid.Children.Add(roiPanel);

        // Portfolio Value
        var valuePanel = CreateSummaryCard("PORTFOLIO", "0.00 ISK", Colors.Gold);
        Grid.SetColumn(valuePanel, 3);
        summaryGrid.Children.Add(valuePanel);

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
            Margin = new Thickness(10),
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
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 150, 200, 255)),
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 5)
        };

        var valueText = new TextBlock
        {
            Text = value,
            FontSize = 16,
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

    private void CreatePerformanceCharts()
    {
        var chartBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(30, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 150, 255)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(5),
            CornerRadius = new CornerRadius(5)
        };

        _chartCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            ClipToBounds = true
        };

        _performanceCanvas = new Canvas
        {
            Background = Brushes.Transparent
        };

        _chartCanvas.Children.Add(_performanceCanvas);
        chartBorder.Child = _chartCanvas;

        Grid.SetRow(chartBorder, 2);
        Grid.SetColumn(chartBorder, 0);
        _mainGrid.Children.Add(chartBorder);
    }

    private void CreatePositionsGrid()
    {
        var positionsBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(30, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 150, 255)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(5),
            CornerRadius = new CornerRadius(5)
        };

        _positionsGrid = new DataGrid
        {
            Background = Brushes.Transparent,
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            CanUserAddRows = false,
            CanUserDeleteRows = false,
            CanUserReorderColumns = false,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            IsReadOnly = true,
            SelectionMode = DataGridSelectionMode.Single,
            AutoGenerateColumns = false,
            Margin = new Thickness(5)
        };

        CreatePositionsGridColumns();
        positionsBorder.Child = _positionsGrid;

        Grid.SetRow(positionsBorder, 2);
        Grid.SetColumn(positionsBorder, 1);
        _mainGrid.Children.Add(positionsBorder);
    }

    private void CreatePositionsGridColumns()
    {
        var itemColumn = new DataGridTextColumn
        {
            Header = "Item",
            Binding = new Binding("ItemName"),
            Width = new DataGridLength(120),
            ElementStyle = CreateCellStyle(Colors.White)
        };

        var quantityColumn = new DataGridTextColumn
        {
            Header = "Qty",
            Binding = new Binding("FormattedQuantity"),
            Width = new DataGridLength(60),
            ElementStyle = CreateCellStyle(Colors.Cyan)
        };

        var avgPriceColumn = new DataGridTextColumn
        {
            Header = "Avg Price",
            Binding = new Binding("FormattedAvgPrice"),
            Width = new DataGridLength(80),
            ElementStyle = CreateCellStyle(Colors.White)
        };

        var currentPriceColumn = new DataGridTextColumn
        {
            Header = "Current",
            Binding = new Binding("FormattedCurrentPrice"),
            Width = new DataGridLength(80),
            ElementStyle = CreateCellStyle(Colors.Gold)
        };

        var plColumn = new DataGridTemplateColumn
        {
            Header = "P&L",
            Width = new DataGridLength(100),
            CellTemplate = CreatePLCellTemplate()
        };

        _positionsGrid.Columns.Add(itemColumn);
        _positionsGrid.Columns.Add(quantityColumn);
        _positionsGrid.Columns.Add(avgPriceColumn);
        _positionsGrid.Columns.Add(currentPriceColumn);
        _positionsGrid.Columns.Add(plColumn);
    }

    private Style CreateCellStyle(Color color)
    {
        var style = new Style(typeof(TextBlock));
        style.Setters.Add(new Setter(TextBlock.ForegroundProperty, new SolidColorBrush(color)));
        style.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.Bold));
        style.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right));
        style.Setters.Add(new Setter(TextBlock.FontSizeProperty, 11.0));
        return style;
    }

    private DataTemplate CreatePLCellTemplate()
    {
        var template = new DataTemplate();
        
        var factory = new FrameworkElementFactory(typeof(StackPanel));
        factory.SetValue(StackPanel.OrientationProperty, Orientation.Vertical);
        
        var plTextFactory = new FrameworkElementFactory(typeof(TextBlock));
        plTextFactory.SetBinding(TextBlock.TextProperty, new Binding("FormattedProfitLoss"));
        plTextFactory.SetBinding(TextBlock.ForegroundProperty, new Binding("ProfitLossColor"));
        plTextFactory.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        plTextFactory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Right);
        plTextFactory.SetValue(TextBlock.FontSizeProperty, 11.0);
        
        var percentTextFactory = new FrameworkElementFactory(typeof(TextBlock));
        percentTextFactory.SetBinding(TextBlock.TextProperty, new Binding("FormattedProfitLossPercent"));
        percentTextFactory.SetBinding(TextBlock.ForegroundProperty, new Binding("ProfitLossColor"));
        percentTextFactory.SetValue(TextBlock.FontSizeProperty, 9.0);
        percentTextFactory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Right);
        
        factory.AppendChild(plTextFactory);
        factory.AppendChild(percentTextFactory);
        template.VisualTree = factory;
        
        return template;
    }

    private void CreateMetricsPanel()
    {
        var metricsBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(50, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 150, 255)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(5),
            CornerRadius = new CornerRadius(5)
        };

        _metricsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.SpaceEvenly,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10)
        };

        CreateMetricIndicator("SHARPE RATIO", "0.00", Colors.Cyan);
        CreateMetricIndicator("MAX DRAWDOWN", "0.00%", Colors.OrangeRed);
        CreateMetricIndicator("VOLATILITY", "0.00%", Colors.Yellow);
        CreateMetricIndicator("WIN RATE", "0.00%", Colors.LimeGreen);

        metricsBorder.Child = _metricsPanel;

        Grid.SetRow(metricsBorder, 3);
        Grid.SetColumnSpan(metricsBorder, 2);
        _mainGrid.Children.Add(metricsBorder);
    }

    private void CreateMetricIndicator(string label, string value, Color color)
    {
        var indicator = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(20, 10, 20, 10)
        };

        var labelText = new TextBlock
        {
            Text = label,
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 150, 200, 255)),
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 5)
        };

        var valueText = new TextBlock
        {
            Text = value,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(color),
            TextAlignment = TextAlignment.Center,
            Tag = label
        };

        indicator.Children.Add(labelText);
        indicator.Children.Add(valueText);
        _metricsPanel.Children.Add(indicator);
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

    private void InitializeProfitLossSystem()
    {
        _performanceIndicators = new List<UIElement>();
        _particles = new List<UIElement>();
        _chartElements = new List<UIElement>();
        
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
        StartProfitLossVisualization();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopProfitLossVisualization();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdatePerformanceCharts();
    }

    private void UpdateTimer_Tick(object sender, EventArgs e)
    {
        UpdateProfitLossData();
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

    #region Profit/Loss Visualization

    public void StartProfitLossVisualization()
    {
        if (!_updateTimer.IsEnabled)
        {
            _updateTimer.Start();
            
            if (EnableAnimations)
            {
                _animationTimer.Start();
            }
            
            UpdateProfitLossData();
        }
    }

    public void StopProfitLossVisualization()
    {
        _updateTimer?.Stop();
        _animationTimer?.Stop();
    }

    private void UpdateProfitLossData()
    {
        if (TradingPositions == null || PortfolioSummary == null)
        {
            GenerateSampleData();
        }

        CalculateFinancialMetrics();
        UpdateSummaryCards();
        UpdatePositionsGrid();
        UpdatePerformanceCharts();
        
        if (EnableParticleEffects && !_isInSimplifiedMode)
        {
            CreateProfitLossParticles();
        }
        
        StartAnimation();
    }

    private void CalculateFinancialMetrics()
    {
        if (TradingPositions == null || !TradingPositions.Any()) return;

        _totalInvested = TradingPositions.Sum(p => p.Quantity * p.AveragePrice);
        _totalValue = TradingPositions.Sum(p => p.Quantity * p.CurrentPrice);
        _totalProfitLoss = _totalValue - _totalInvested;
        _returnOnInvestment = _totalInvested > 0 ? (_totalProfitLoss / _totalInvested) * 100 : 0;

        // Calculate time-based P&L
        CalculateTimeBasedProfitLoss();
        
        // Calculate risk metrics
        CalculateRiskMetrics();
        
        // Update portfolio summary
        if (PortfolioSummary != null)
        {
            PortfolioSummary.TotalValue = _totalValue;
            PortfolioSummary.TotalProfitLoss = _totalProfitLoss;
            PortfolioSummary.ReturnOnInvestment = _returnOnInvestment;
            PortfolioSummary.LastUpdated = DateTime.Now;
        }
    }

    private void CalculateTimeBasedProfitLoss()
    {
        var now = DateTime.Now;
        
        switch (TimeFrame)
        {
            case ProfitLossTimeFrame.Hourly:
                _dayProfitLoss = TradingPositions?
                    .Where(p => p.LastUpdate >= now.AddHours(-1))
                    .Sum(p => (p.CurrentPrice - p.PreviousPrice) * p.Quantity) ?? 0;
                break;
            case ProfitLossTimeFrame.Daily:
                _dayProfitLoss = TradingPositions?
                    .Where(p => p.LastUpdate >= now.AddDays(-1))
                    .Sum(p => (p.CurrentPrice - p.PreviousPrice) * p.Quantity) ?? 0;
                break;
            case ProfitLossTimeFrame.Weekly:
                _weekProfitLoss = TradingPositions?
                    .Where(p => p.LastUpdate >= now.AddDays(-7))
                    .Sum(p => (p.CurrentPrice - p.PreviousPrice) * p.Quantity) ?? 0;
                break;
            case ProfitLossTimeFrame.Monthly:
                _monthProfitLoss = TradingPositions?
                    .Where(p => p.LastUpdate >= now.AddDays(-30))
                    .Sum(p => (p.CurrentPrice - p.PreviousPrice) * p.Quantity) ?? 0;
                break;
        }
    }

    private void CalculateRiskMetrics()
    {
        if (TradingPositions == null || !TradingPositions.Any()) return;

        // Calculate volatility (standard deviation of returns)
        var returns = TradingPositions.Select(p => (p.CurrentPrice - p.AveragePrice) / p.AveragePrice).ToList();
        if (returns.Any())
        {
            var meanReturn = returns.Average();
            var variance = returns.Select(r => Math.Pow(r - meanReturn, 2)).Average();
            _volatility = Math.Sqrt(variance) * 100;
        }

        // Simplified Sharpe ratio calculation
        var riskFreeRate = 0.02; // 2% annual risk-free rate
        _sharpeRatio = _volatility > 0 ? (_returnOnInvestment / 100 - riskFreeRate) / (_volatility / 100) : 0;

        // Calculate max drawdown
        var portfolioValues = new List<double>();
        var runningValue = _totalInvested;
        foreach (var position in TradingPositions)
        {
            runningValue += (position.CurrentPrice - position.AveragePrice) * position.Quantity;
            portfolioValues.Add(runningValue);
        }

        if (portfolioValues.Any())
        {
            var peak = portfolioValues.First();
            _maxDrawdown = 0;
            foreach (var value in portfolioValues)
            {
                if (value > peak) peak = value;
                var drawdown = (peak - value) / peak * 100;
                if (drawdown > _maxDrawdown) _maxDrawdown = drawdown;
            }
        }
    }

    private void UpdateSummaryCards()
    {
        // Find and update summary card values
        UpdateSummaryCard("TOTAL P&L", FormatCurrency(_totalProfitLoss), GetProfitLossColor(_totalProfitLoss));
        UpdateSummaryCard("TODAY", FormatCurrency(_dayProfitLoss), GetProfitLossColor(_dayProfitLoss));
        UpdateSummaryCard("ROI", $"{_returnOnInvestment:F2}%", GetProfitLossColor(_totalProfitLoss));
        UpdateSummaryCard("PORTFOLIO", FormatCurrency(_totalValue), Colors.Gold);
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
                        
                        // Add animation for value changes
                        if (EnableAnimations && _previousValues.ContainsKey(title))
                        {
                            AnimateValueChange(valueText);
                        }
                        
                        _previousValues[title] = value.GetHashCode();
                    }
                    break;
                }
            }
        }
    }

    private void AnimateValueChange(TextBlock textBlock)
    {
        var scaleTransform = new ScaleTransform(1, 1);
        textBlock.RenderTransform = scaleTransform;
        
        var animation = new DoubleAnimation
        {
            From = 1.0,
            To = 1.2,
            Duration = TimeSpan.FromMilliseconds(200),
            AutoReverse = true
        };
        
        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
    }

    private Color GetProfitLossColor(double value)
    {
        if (value > 0) return Colors.LimeGreen;
        if (value < 0) return Colors.OrangeRed;
        return Colors.White;
    }

    private string FormatCurrency(double value)
    {
        var absValue = Math.Abs(value);
        if (absValue >= 1e12) return $"{(value / 1e12):F2}T ISK";
        if (absValue >= 1e9) return $"{(value / 1e9):F2}B ISK";
        if (absValue >= 1e6) return $"{(value / 1e6):F2}M ISK";
        if (absValue >= 1e3) return $"{(value / 1e3):F2}K ISK";
        return $"{value:F2} ISK";
    }

    private void UpdatePositionsGrid()
    {
        if (TradingPositions != null)
        {
            var sortedPositions = TradingPositions
                .OrderByDescending(p => Math.Abs(p.ProfitLoss))
                .ToList();
                
            _positionsGrid.ItemsSource = sortedPositions;
        }
    }

    private void UpdatePerformanceCharts()
    {
        if (!ShowPerformanceCharts) return;
        
        _performanceCanvas.Children.Clear();
        _chartElements.Clear();
        
        var canvasWidth = _chartCanvas.ActualWidth > 0 ? _chartCanvas.ActualWidth : 400;
        var canvasHeight = _chartCanvas.ActualHeight > 0 ? _chartCanvas.ActualHeight : 300;
        
        DrawPerformanceChart(canvasWidth, canvasHeight);
        DrawProfitLossDistribution(canvasWidth, canvasHeight);
    }

    private void DrawPerformanceChart(double width, double height)
    {
        if (TradingPositions == null || !TradingPositions.Any()) return;

        var chartHeight = height * 0.6;
        var startY = 20;
        
        // Draw background grid
        DrawChartGrid(width, chartHeight, startY);
        
        // Calculate performance over time
        var performancePoints = CalculatePerformancePoints();
        if (performancePoints.Count < 2) return;
        
        // Draw performance line
        var polyline = new Polyline
        {
            Stroke = new SolidColorBrush(Color.FromArgb(200, 0, 255, 150)),
            StrokeThickness = 3,
            Points = new PointCollection()
        };
        
        var area = new Polygon
        {
            Fill = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(100, 0, 255, 150), 0),
                    new GradientStop(Color.FromArgb(20, 0, 255, 150), 1)
                }
            },
            Points = new PointCollection()
        };
        
        var maxValue = performancePoints.Max(p => p.Value);
        var minValue = performancePoints.Min(p => p.Value);
        var valueRange = maxValue - minValue;
        if (valueRange == 0) valueRange = 1;
        
        var timeSpan = performancePoints.Max(p => p.Time) - performancePoints.Min(p => p.Time);
        var totalMinutes = timeSpan.TotalMinutes;
        if (totalMinutes == 0) totalMinutes = 1;
        
        foreach (var point in performancePoints)
        {
            var x = ((point.Time - performancePoints.First().Time).TotalMinutes / totalMinutes) * (width - 40) + 20;
            var y = startY + chartHeight - ((point.Value - minValue) / valueRange) * chartHeight;
            
            polyline.Points.Add(new Point(x, y));
            area.Points.Add(new Point(x, y));
        }
        
        // Close area polygon
        if (area.Points.Count > 0)
        {
            area.Points.Add(new Point(area.Points.Last().X, startY + chartHeight));
            area.Points.Add(new Point(area.Points.First().X, startY + chartHeight));
        }
        
        _performanceCanvas.Children.Add(area);
        _performanceCanvas.Children.Add(polyline);
        _chartElements.Add(area);
        _chartElements.Add(polyline);
        
        // Add chart title
        var titleText = new TextBlock
        {
            Text = "PORTFOLIO PERFORMANCE",
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            Background = new SolidColorBrush(Color.FromArgb(150, 0, 0, 0))
        };
        
        Canvas.SetLeft(titleText, 20);
        Canvas.SetTop(titleText, 0);
        _performanceCanvas.Children.Add(titleText);
    }

    private void DrawChartGrid(double width, double height, double startY)
    {
        // Horizontal grid lines
        for (int i = 0; i <= 5; i++)
        {
            var y = startY + (height / 5) * i;
            var gridLine = new Line
            {
                X1 = 20, Y1 = y, X2 = width - 20, Y2 = y,
                Stroke = new SolidColorBrush(Color.FromArgb(50, 100, 150, 200)),
                StrokeThickness = 1
            };
            _performanceCanvas.Children.Add(gridLine);
        }
        
        // Vertical grid lines
        for (int i = 0; i <= 10; i++)
        {
            var x = 20 + ((width - 40) / 10) * i;
            var gridLine = new Line
            {
                X1 = x, Y1 = startY, X2 = x, Y2 = startY + height,
                Stroke = new SolidColorBrush(Color.FromArgb(30, 100, 150, 200)),
                StrokeThickness = 1
            };
            _performanceCanvas.Children.Add(gridLine);
        }
    }

    private List<PerformancePoint> CalculatePerformancePoints()
    {
        var points = new List<PerformancePoint>();
        var baseTime = DateTime.Now.AddHours(-24);
        var runningValue = _totalInvested;
        
        // Simulate performance over time
        for (int i = 0; i < 100; i++)
        {
            var time = baseTime.AddMinutes(i * 14.4); // 24 hours / 100 points
            var change = (_random.NextDouble() - 0.5) * (_totalInvested * 0.001); // Small random changes
            runningValue += change;
            
            points.Add(new PerformancePoint
            {
                Time = time,
                Value = runningValue - _totalInvested // P&L value
            });
        }
        
        return points;
    }

    private void DrawProfitLossDistribution(double width, double height)
    {
        var chartHeight = height * 0.3;
        var startY = height * 0.7;
        
        if (TradingPositions == null || !TradingPositions.Any()) return;
        
        var profitable = TradingPositions.Count(p => p.ProfitLoss > 0);
        var losing = TradingPositions.Count(p => p.ProfitLoss < 0);
        var total = profitable + losing;
        
        if (total == 0) return;
        
        var profitableWidth = (profitable / (double)total) * (width - 40);
        var losingWidth = (losing / (double)total) * (width - 40);
        
        // Profitable positions bar
        var profitBar = new Rectangle
        {
            Width = profitableWidth,
            Height = chartHeight * 0.5,
            Fill = new SolidColorBrush(Color.FromArgb(150, 0, 255, 100)),
            Stroke = new SolidColorBrush(Color.FromArgb(200, 0, 255, 150)),
            StrokeThickness = 1
        };
        
        Canvas.SetLeft(profitBar, 20);
        Canvas.SetTop(profitBar, startY);
        _performanceCanvas.Children.Add(profitBar);
        
        // Losing positions bar
        var lossBar = new Rectangle
        {
            Width = losingWidth,
            Height = chartHeight * 0.5,
            Fill = new SolidColorBrush(Color.FromArgb(150, 255, 100, 0)),
            Stroke = new SolidColorBrush(Color.FromArgb(200, 255, 150, 0)),
            StrokeThickness = 1
        };
        
        Canvas.SetLeft(lossBar, 20 + profitableWidth);
        Canvas.SetTop(lossBar, startY);
        _performanceCanvas.Children.Add(lossBar);
        
        // Labels
        var distributionLabel = new TextBlock
        {
            Text = $"WIN/LOSS DISTRIBUTION ({profitable}/{losing})",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 200, 255))
        };
        
        Canvas.SetLeft(distributionLabel, 20);
        Canvas.SetTop(distributionLabel, startY - 20);
        _performanceCanvas.Children.Add(distributionLabel);
    }

    private void CreateProfitLossParticles()
    {
        // Remove old particles
        var oldParticles = _particles.ToList();
        foreach (var particle in oldParticles)
        {
            _particleCanvas.Children.Remove(particle);
        }
        _particles.Clear();
        
        // Create particles based on P&L performance
        var particleCount = Math.Min(_maxParticles, Math.Abs((int)(_totalProfitLoss / 1000000))); // 1 particle per 1M ISK
        
        for (int i = 0; i < particleCount; i++)
        {
            var isProfit = _totalProfitLoss > 0;
            var particle = new Ellipse
            {
                Width = 3 + _random.NextDouble() * 4,
                Height = 3 + _random.NextDouble() * 4,
                Fill = new SolidColorBrush(isProfit 
                    ? Color.FromArgb(200, 0, 255, 100) 
                    : Color.FromArgb(200, 255, 100, 0))
            };
            
            var startX = _random.NextDouble() * ActualWidth;
            var startY = _random.NextDouble() * ActualHeight;
            
            Canvas.SetLeft(particle, startX);
            Canvas.SetTop(particle, startY);
            
            _particleCanvas.Children.Add(particle);
            _particles.Add(particle);
            
            AnimateProfitLossParticle(particle, isProfit);
        }
    }

    private void AnimateProfitLossParticle(UIElement particle, bool isProfit)
    {
        var duration = TimeSpan.FromSeconds(3 + _random.NextDouble() * 4);
        
        // Movement animation
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
            To = isProfit ? Canvas.GetTop(particle) - 100 : Canvas.GetTop(particle) + 100,
            Duration = duration,
            EasingFunction = new SineEase()
        };
        
        // Fade animation
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

    #endregion

    #region Animation

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
        // Animate chart elements
        foreach (var element in _chartElements)
        {
            element.Opacity = 0.3 + (0.7 * _animationProgress);
        }
        
        // Update metrics with animated appearance
        foreach (StackPanel metric in _metricsPanel.Children)
        {
            if (metric.Children.Count > 1 && metric.Children[1] is TextBlock valueText)
            {
                var transform = new ScaleTransform(_animationProgress, _animationProgress);
                valueText.RenderTransform = transform;
            }
        }
    }

    private void UpdateParticleEffects()
    {
        // Clean up completed particles
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

    private void GenerateSampleData()
    {
        TradingPositions = new ObservableCollection<HoloTradingPosition>();
        
        var sampleItems = new[]
        {
            "Tritanium", "Pyerite", "Mexallon", "Isogen", "Nocxium",
            "Zydrine", "Megacyte", "Morphite", "PLEX", "Skill Injector"
        };
        
        foreach (var item in sampleItems)
        {
            var avgPrice = 100000 + _random.NextDouble() * 2000000;
            var currentPrice = avgPrice * (0.8 + _random.NextDouble() * 0.4); // ±20% variation
            var quantity = (long)(100 + _random.NextDouble() * 10000);
            
            var position = new HoloTradingPosition
            {
                ItemName = item,
                Quantity = quantity,
                AveragePrice = avgPrice,
                CurrentPrice = currentPrice,
                PreviousPrice = avgPrice * (0.95 + _random.NextDouble() * 0.1),
                LastUpdate = DateTime.Now.AddMinutes(-_random.Next(1440)) // Within last day
            };
            
            TradingPositions.Add(position);
        }
        
        PortfolioSummary = new HoloPortfolioSummary
        {
            TotalValue = 0,
            TotalProfitLoss = 0,
            ReturnOnInvestment = 0,
            LastUpdated = DateTime.Now
        };
    }

    private void RefreshData()
    {
        GenerateSampleData();
        UpdateProfitLossData();
    }

    #endregion

    #region Public Methods

    public ProfitLossAnalysis GetProfitLossAnalysis()
    {
        return new ProfitLossAnalysis
        {
            TotalProfitLoss = _totalProfitLoss,
            TotalValue = _totalValue,
            TotalInvested = _totalInvested,
            ReturnOnInvestment = _returnOnInvestment,
            DayProfitLoss = _dayProfitLoss,
            WeekProfitLoss = _weekProfitLoss,
            MonthProfitLoss = _monthProfitLoss,
            SharpeRatio = _sharpeRatio,
            MaxDrawdown = _maxDrawdown,
            Volatility = _volatility,
            PositionCount = TradingPositions?.Count ?? 0,
            TimeFrame = TimeFrame,
            IsAnimating = _isAnimating,
            PerformanceMode = _isInSimplifiedMode ? "Simplified" : "Full"
        };
    }

    public void SetTimeFrame(ProfitLossTimeFrame timeFrame)
    {
        TimeFrame = timeFrame;
        UpdateProfitLossData();
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void SetSimplifiedMode(bool enabled)
    {
        _isInSimplifiedMode = enabled;
        EnableParticleEffects = !enabled;
        EnableAnimations = !enabled;
        _maxParticles = enabled ? 10 : 30;
        
        if (IsLoaded)
        {
            UpdateProfitLossData();
        }
    }

    public bool IsInSimplifiedMode => _isInSimplifiedMode;

    #endregion

    #region Property Change Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloProfitLossIndicators control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloProfitLossIndicators control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnTradingPositionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloProfitLossIndicators control && control.IsLoaded)
        {
            control.UpdateProfitLossData();
        }
    }

    private static void OnPortfolioSummaryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloProfitLossIndicators control && control.IsLoaded)
        {
            control.UpdateSummaryCards();
        }
    }

    private static void OnTimeFrameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloProfitLossIndicators control && control.IsLoaded)
        {
            control.UpdateProfitLossData();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloProfitLossIndicators control)
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
        if (d is HoloProfitLossIndicators control && control.IsLoaded)
        {
            control.UpdateProfitLossData();
        }
    }

    private static void OnShowPerformanceChartsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloProfitLossIndicators control && control.IsLoaded)
        {
            control.UpdatePerformanceCharts();
        }
    }

    private static void OnShowRiskMetricsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloProfitLossIndicators control)
        {
            control._metricsPanel.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnShowPortfolioBreakdownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloProfitLossIndicators control)
        {
            control._positionsGrid.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnAutoRefreshChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloProfitLossIndicators control)
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
        if (d is HoloProfitLossIndicators control)
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
            UpdateProfitLossData();
        }
    }

    #endregion
}

#region Supporting Classes

public class HoloTradingPosition : INotifyPropertyChanged
{
    private double _currentPrice;
    private double _profitLoss;
    
    public string ItemName { get; set; }
    public long Quantity { get; set; }
    public double AveragePrice { get; set; }
    public double PreviousPrice { get; set; }
    public DateTime LastUpdate { get; set; }
    
    public double CurrentPrice
    {
        get => _currentPrice;
        set
        {
            _currentPrice = value;
            CalculateProfitLoss();
            OnPropertyChanged(nameof(CurrentPrice));
            OnPropertyChanged(nameof(FormattedCurrentPrice));
        }
    }
    
    public double ProfitLoss
    {
        get => _profitLoss;
        private set
        {
            _profitLoss = value;
            OnPropertyChanged(nameof(ProfitLoss));
            OnPropertyChanged(nameof(FormattedProfitLoss));
            OnPropertyChanged(nameof(FormattedProfitLossPercent));
            OnPropertyChanged(nameof(ProfitLossColor));
        }
    }
    
    private void CalculateProfitLoss()
    {
        ProfitLoss = (CurrentPrice - AveragePrice) * Quantity;
    }
    
    public string FormattedQuantity => Quantity.ToString("N0");
    public string FormattedAvgPrice => AveragePrice.ToString("N2");
    public string FormattedCurrentPrice => CurrentPrice.ToString("N2");
    public string FormattedProfitLoss => FormatCurrency(ProfitLoss);
    public string FormattedProfitLossPercent => $"{(ProfitLossPercent):F2}%";
    
    public double ProfitLossPercent => AveragePrice > 0 ? ((CurrentPrice - AveragePrice) / AveragePrice) * 100 : 0;
    
    public Brush ProfitLossColor
    {
        get
        {
            if (ProfitLoss > 0) return new SolidColorBrush(Colors.LimeGreen);
            if (ProfitLoss < 0) return new SolidColorBrush(Colors.OrangeRed);
            return new SolidColorBrush(Colors.White);
        }
    }
    
    private string FormatCurrency(double value)
    {
        var absValue = Math.Abs(value);
        if (absValue >= 1e9) return $"{(value / 1e9):F2}B";
        if (absValue >= 1e6) return $"{(value / 1e6):F2}M";
        if (absValue >= 1e3) return $"{(value / 1e3):F2}K";
        return $"{value:F2}";
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
    
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class HoloPortfolioSummary
{
    public double TotalValue { get; set; }
    public double TotalProfitLoss { get; set; }
    public double ReturnOnInvestment { get; set; }
    public DateTime LastUpdated { get; set; }
}

public enum ProfitLossTimeFrame
{
    Hourly,
    Daily,
    Weekly,
    Monthly
}

public class PerformancePoint
{
    public DateTime Time { get; set; }
    public double Value { get; set; }
}

public class ProfitLossAnalysis
{
    public double TotalProfitLoss { get; set; }
    public double TotalValue { get; set; }
    public double TotalInvested { get; set; }
    public double ReturnOnInvestment { get; set; }
    public double DayProfitLoss { get; set; }
    public double WeekProfitLoss { get; set; }
    public double MonthProfitLoss { get; set; }
    public double SharpeRatio { get; set; }
    public double MaxDrawdown { get; set; }
    public double Volatility { get; set; }
    public int PositionCount { get; set; }
    public ProfitLossTimeFrame TimeFrame { get; set; }
    public bool IsAnimating { get; set; }
    public string PerformanceMode { get; set; }
}

#endregion