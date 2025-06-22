// ==========================================================================
// HoloRegionalMarketComparison.cs - Holographic Regional Market Comparison
// ==========================================================================
// Advanced regional market comparison featuring real-time cross-region analysis,
// price arbitrage detection, EVE-style regional data, and holographic comparison visualization.
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
/// Holographic regional market comparison with real-time arbitrage analysis and cross-region visualization
/// </summary>
public class HoloRegionalMarketComparison : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloRegionalMarketComparison),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloRegionalMarketComparison),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty RegionalDataProperty =
        DependencyProperty.Register(nameof(RegionalData), typeof(ObservableCollection<HoloRegionalMarketData>), typeof(HoloRegionalMarketComparison),
            new PropertyMetadata(null, OnRegionalDataChanged));

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(string), typeof(HoloRegionalMarketComparison),
            new PropertyMetadata("Tritanium", OnSelectedItemChanged));

    public static readonly DependencyProperty ComparisonModeProperty =
        DependencyProperty.Register(nameof(ComparisonMode), typeof(RegionalComparisonMode), typeof(HoloRegionalMarketComparison),
            new PropertyMetadata(RegionalComparisonMode.PriceComparison, OnComparisonModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloRegionalMarketComparison),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloRegionalMarketComparison),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowArbitrageOpportunitiesProperty =
        DependencyProperty.Register(nameof(ShowArbitrageOpportunities), typeof(bool), typeof(HoloRegionalMarketComparison),
            new PropertyMetadata(true, OnShowArbitrageOpportunitiesChanged));

    public static readonly DependencyProperty ShowRegionalTrendsProperty =
        DependencyProperty.Register(nameof(ShowRegionalTrends), typeof(bool), typeof(HoloRegionalMarketComparison),
            new PropertyMetadata(true, OnShowRegionalTrendsChanged));

    public static readonly DependencyProperty ShowVolumeAnalysisProperty =
        DependencyProperty.Register(nameof(ShowVolumeAnalysis), typeof(bool), typeof(HoloRegionalMarketComparison),
            new PropertyMetadata(true, OnShowVolumeAnalysisChanged));

    public static readonly DependencyProperty AutoRefreshProperty =
        DependencyProperty.Register(nameof(AutoRefresh), typeof(bool), typeof(HoloRegionalMarketComparison),
            new PropertyMetadata(true, OnAutoRefreshChanged));

    public static readonly DependencyProperty UpdateIntervalProperty =
        DependencyProperty.Register(nameof(UpdateInterval), typeof(TimeSpan), typeof(HoloRegionalMarketComparison),
            new PropertyMetadata(TimeSpan.FromSeconds(10), OnUpdateIntervalChanged));

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

    public ObservableCollection<HoloRegionalMarketData> RegionalData
    {
        get => (ObservableCollection<HoloRegionalMarketData>)GetValue(RegionalDataProperty);
        set => SetValue(RegionalDataProperty, value);
    }

    public string SelectedItem
    {
        get => (string)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public RegionalComparisonMode ComparisonMode
    {
        get => (RegionalComparisonMode)GetValue(ComparisonModeProperty);
        set => SetValue(ComparisonModeProperty, value);
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

    public bool ShowArbitrageOpportunities
    {
        get => (bool)GetValue(ShowArbitrageOpportunitiesProperty);
        set => SetValue(ShowArbitrageOpportunitiesProperty, value);
    }

    public bool ShowRegionalTrends
    {
        get => (bool)GetValue(ShowRegionalTrendsProperty);
        set => SetValue(ShowRegionalTrendsProperty, value);
    }

    public bool ShowVolumeAnalysis
    {
        get => (bool)GetValue(ShowVolumeAnalysisProperty);
        set => SetValue(ShowVolumeAnalysisProperty, value);
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
    private Canvas _comparisonCanvas;
    private Canvas _particleCanvas;
    private Border _summaryPanel;
    private DataGrid _regionalGrid;
    private StackPanel _arbitragePanel;
    private Canvas _visualizationCanvas;
    
    private DispatcherTimer _updateTimer;
    private DispatcherTimer _animationTimer;
    
    private readonly Random _random = new();
    private List<UIElement> _comparisonElements;
    private List<UIElement> _particles;
    private List<UIElement> _arbitrageIndicators;
    
    // Regional analysis data
    private List<ArbitrageOpportunity> _arbitrageOpportunities;
    private Dictionary<string, RegionalTrend> _regionalTrends;
    private Dictionary<string, double> _priceVariances;
    private double _bestArbitrageMargin = 0.0;
    private string _bestArbitrageRoute = "";
    private int _totalOpportunities = 0;
    
    // EVE Online regions
    private readonly List<string> _eveRegions = new()
    {
        "The Forge", "Domain", "Sinq Laison", "Heimatar", "Metropolis",
        "The Citadel", "Lonetrek", "Essence", "Verge Vendor", "Everyshore",
        "Placid", "Solitude", "Aridia", "Genesis", "Kador"
    };
    
    // Animation state
    private double _animationProgress = 0.0;
    private bool _isAnimating = false;
    
    // Performance optimization
    private bool _isInSimplifiedMode = false;
    private int _maxRegions = 15;
    private int _maxParticles = 30;

    #endregion

    #region Constructor

    public HoloRegionalMarketComparison()
    {
        InitializeComponent();
        InitializeRegionalComparisonSystem();
        SetupEventHandlers();
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 1400;
        Height = 900;
        Background = new SolidColorBrush(Color.FromArgb(30, 0, 50, 100));
        
        // Create main layout
        _mainGrid = new Grid();
        Content = _mainGrid;

        // Define layout structure
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(120) }); // Summary
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) }); // Main comparison
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Details

        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }); // Visualization
        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Data grid

        CreateHeader();
        CreateSummaryPanel();
        CreateComparisonVisualization();
        CreateRegionalDataGrid();
        CreateArbitragePanel();
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
            Text = "REGIONAL MARKET COMPARISON",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };

        var controlsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        var itemCombo = new ComboBox
        {
            Width = 120,
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 50, 100)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 0, 150, 255)),
            Margin = new Thickness(0, 0, 10, 0)
        };

        var items = new[] { "Tritanium", "Pyerite", "Mexallon", "Isogen", "Nocxium", "Zydrine", "Megacyte", "PLEX", "Skill Injector" };
        foreach (var item in items)
        {
            itemCombo.Items.Add(item);
        }
        itemCombo.SelectedItem = SelectedItem;
        itemCombo.SelectionChanged += (s, e) => SelectedItem = itemCombo.SelectedItem?.ToString() ?? "Tritanium";

        var modeCombo = new ComboBox
        {
            Width = 140,
            SelectedIndex = 0,
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 50, 100)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 0, 150, 255)),
            Margin = new Thickness(0, 0, 10, 0)
        };

        modeCombo.Items.Add("Price Comparison");
        modeCombo.Items.Add("Volume Analysis");
        modeCombo.Items.Add("Arbitrage Finder");
        modeCombo.Items.Add("Trend Analysis");
        modeCombo.SelectionChanged += (s, e) => ComparisonMode = (RegionalComparisonMode)modeCombo.SelectedIndex;

        var refreshButton = CreateHoloButton("REFRESH", RefreshRegionalData);

        controlsPanel.Children.Add(itemCombo);
        controlsPanel.Children.Add(modeCombo);
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

        // Best Arbitrage
        var arbitrageCard = CreateSummaryCard("BEST ARBITRAGE", "0%", Colors.LimeGreen);
        Grid.SetColumn(arbitrageCard, 0);
        summaryGrid.Children.Add(arbitrageCard);

        // Opportunities
        var opportunitiesCard = CreateSummaryCard("OPPORTUNITIES", "0", Colors.Cyan);
        Grid.SetColumn(opportunitiesCard, 1);
        summaryGrid.Children.Add(opportunitiesCard);

        // Price Variance
        var varianceCard = CreateSummaryCard("VARIANCE", "0%", Colors.Orange);
        Grid.SetColumn(varianceCard, 2);
        summaryGrid.Children.Add(varianceCard);

        // Active Regions
        var regionsCard = CreateSummaryCard("REGIONS", "0", Colors.Yellow);
        Grid.SetColumn(regionsCard, 3);
        summaryGrid.Children.Add(regionsCard);

        // Best Route
        var routeCard = CreateSummaryCard("BEST ROUTE", "NONE", Colors.Gold);
        Grid.SetColumn(routeCard, 4);
        summaryGrid.Children.Add(routeCard);

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
            Margin = new Thickness(8)
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
            FontSize = 12,
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

    private void CreateComparisonVisualization()
    {
        var visualizationBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(30, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 150, 255)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(5),
            CornerRadius = new CornerRadius(5)
        };

        _visualizationCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            ClipToBounds = true
        };

        _comparisonCanvas = new Canvas
        {
            Background = Brushes.Transparent
        };

        _visualizationCanvas.Children.Add(_comparisonCanvas);
        visualizationBorder.Child = _visualizationCanvas;

        Grid.SetRow(visualizationBorder, 2);
        Grid.SetColumn(visualizationBorder, 0);
        _mainGrid.Children.Add(visualizationBorder);
    }

    private void CreateRegionalDataGrid()
    {
        var gridBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(30, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 150, 255)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(5),
            CornerRadius = new CornerRadius(5)
        };

        var gridContent = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var gridTitle = new TextBlock
        {
            Text = "REGIONAL DATA",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 10)
        };

        _regionalGrid = new DataGrid
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
            MaxHeight = 400,
            Margin = new Thickness(5)
        };

        CreateRegionalGridColumns();

        gridContent.Children.Add(gridTitle);
        gridContent.Children.Add(_regionalGrid);
        gridBorder.Child = gridContent;

        Grid.SetRow(gridBorder, 2);
        Grid.SetColumn(gridBorder, 1);
        _mainGrid.Children.Add(gridBorder);
    }

    private void CreateRegionalGridColumns()
    {
        var regionColumn = new DataGridTextColumn
        {
            Header = "Region",
            Binding = new Binding("Region"),
            Width = new DataGridLength(100),
            ElementStyle = CreateCellStyle(Colors.Cyan)
        };

        var priceColumn = new DataGridTextColumn
        {
            Header = "Price",
            Binding = new Binding("FormattedPrice"),
            Width = new DataGridLength(80),
            ElementStyle = CreateCellStyle(Colors.White)
        };

        var volumeColumn = new DataGridTextColumn
        {
            Header = "Volume",
            Binding = new Binding("FormattedVolume"),
            Width = new DataGridLength(70),
            ElementStyle = CreateCellStyle(Colors.Yellow)
        };

        var trendColumn = new DataGridTemplateColumn
        {
            Header = "Trend",
            Width = new DataGridLength(50),
            CellTemplate = CreateTrendCellTemplate()
        };

        var arbitrageColumn = new DataGridTextColumn
        {
            Header = "Arbitrage",
            Binding = new Binding("FormattedArbitragePercent"),
            Width = new DataGridLength(70),
            ElementStyle = CreateCellStyle(Colors.LimeGreen)
        };

        _regionalGrid.Columns.Add(regionColumn);
        _regionalGrid.Columns.Add(priceColumn);
        _regionalGrid.Columns.Add(volumeColumn);
        _regionalGrid.Columns.Add(trendColumn);
        _regionalGrid.Columns.Add(arbitrageColumn);
    }

    private Style CreateCellStyle(Color color)
    {
        var style = new Style(typeof(TextBlock));
        style.Setters.Add(new Setter(TextBlock.ForegroundProperty, new SolidColorBrush(color)));
        style.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.Bold));
        style.Setters.Add(new Setter(TextBlock.FontSizeProperty, 10.0));
        style.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right));
        return style;
    }

    private DataTemplate CreateTrendCellTemplate()
    {
        var template = new DataTemplate();
        
        var factory = new FrameworkElementFactory(typeof(Polygon));
        factory.SetValue(Polygon.WidthProperty, 12.0);
        factory.SetValue(Polygon.HeightProperty, 12.0);
        factory.SetBinding(Polygon.FillProperty, new Binding("TrendColor"));
        factory.SetBinding(Polygon.PointsProperty, new Binding("TrendPoints"));
        factory.SetValue(Polygon.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        
        template.VisualTree = factory;
        return template;
    }

    private void CreateArbitragePanel()
    {
        var arbitrageBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 0, 150, 255)),
            BorderThickness = new Thickness(2),
            Margin = new Thickness(5),
            CornerRadius = new CornerRadius(5)
        };

        var arbitrageContent = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10)
        };

        var arbitrageTitle = new TextBlock
        {
            Text = "ARBITRAGE OPPORTUNITIES",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };

        _arbitragePanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        arbitrageContent.Children.Add(arbitrageTitle);
        arbitrageContent.Children.Add(_arbitragePanel);
        arbitrageBorder.Child = arbitrageContent;

        Grid.SetRow(arbitrageBorder, 3);
        Grid.SetColumnSpan(arbitrageBorder, 2);
        _mainGrid.Children.Add(arbitrageBorder);
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

    private void InitializeRegionalComparisonSystem()
    {
        _comparisonElements = new List<UIElement>();
        _particles = new List<UIElement>();
        _arbitrageIndicators = new List<UIElement>();
        
        _arbitrageOpportunities = new List<ArbitrageOpportunity>();
        _regionalTrends = new Dictionary<string, RegionalTrend>();
        _priceVariances = new Dictionary<string, double>();
        
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
        StartRegionalComparison();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopRegionalComparison();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateComparisonVisualization();
    }

    private void UpdateTimer_Tick(object sender, EventArgs e)
    {
        UpdateRegionalComparisonData();
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

    #region Regional Comparison

    public void StartRegionalComparison()
    {
        if (!_updateTimer.IsEnabled)
        {
            _updateTimer.Start();
            
            if (EnableAnimations)
            {
                _animationTimer.Start();
            }
            
            // Generate sample data if none exists
            if (RegionalData == null || !RegionalData.Any())
            {
                GenerateSampleRegionalData();
            }
            
            UpdateRegionalComparisonData();
        }
    }

    public void StopRegionalComparison()
    {
        _updateTimer?.Stop();
        _animationTimer?.Stop();
    }

    private void UpdateRegionalComparisonData()
    {
        if (RegionalData == null || !RegionalData.Any()) return;

        AnalyzeRegionalData();
        UpdateArbitrageOpportunities();
        UpdateRegionalTrends();
        UpdateSummaryCards();
        UpdateRegionalGrid();
        UpdateComparisonVisualization();
        UpdateArbitrageDisplay();
        
        if (EnableParticleEffects && !_isInSimplifiedMode)
        {
            CreateRegionalParticles();
        }
        
        StartAnimation();
    }

    private void AnalyzeRegionalData()
    {
        var currentItemData = RegionalData
            .Where(d => d.ItemName == SelectedItem)
            .ToList();

        if (!currentItemData.Any()) return;

        // Calculate price variance
        var prices = currentItemData.Select(d => d.Price).ToList();
        var meanPrice = prices.Average();
        var variance = prices.Select(p => Math.Pow(p - meanPrice, 2)).Average();
        var standardDeviation = Math.Sqrt(variance);
        var coefficientOfVariation = meanPrice > 0 ? (standardDeviation / meanPrice) * 100 : 0;

        _priceVariances[SelectedItem] = coefficientOfVariation;

        // Find best arbitrage opportunity
        var minPriceData = currentItemData.OrderBy(d => d.Price).First();
        var maxPriceData = currentItemData.OrderByDescending(d => d.Price).First();

        if (minPriceData.Price > 0)
        {
            _bestArbitrageMargin = ((maxPriceData.Price - minPriceData.Price) / minPriceData.Price) * 100;
            _bestArbitrageRoute = $"{minPriceData.Region} → {maxPriceData.Region}";
        }

        // Update regional arbitrage percentages
        foreach (var data in currentItemData)
        {
            if (minPriceData.Price > 0)
            {
                data.ArbitragePercent = ((data.Price - minPriceData.Price) / minPriceData.Price) * 100;
            }
        }
    }

    private void UpdateArbitrageOpportunities()
    {
        _arbitrageOpportunities.Clear();

        if (!ShowArbitrageOpportunities) return;

        var currentItemData = RegionalData
            .Where(d => d.ItemName == SelectedItem)
            .OrderBy(d => d.Price)
            .ToList();

        if (currentItemData.Count < 2) return;

        var basePriceData = currentItemData.First();

        foreach (var targetData in currentItemData.Skip(1))
        {
            if (basePriceData.Price > 0 && targetData.Volume > 100) // Minimum volume threshold
            {
                var marginPercent = ((targetData.Price - basePriceData.Price) / basePriceData.Price) * 100;
                
                if (marginPercent > 5) // Minimum 5% margin
                {
                    var opportunity = new ArbitrageOpportunity
                    {
                        ItemName = SelectedItem,
                        SourceRegion = basePriceData.Region,
                        TargetRegion = targetData.Region,
                        SourcePrice = basePriceData.Price,
                        TargetPrice = targetData.Price,
                        MarginPercent = marginPercent,
                        PotentialProfit = (targetData.Price - basePriceData.Price) * Math.Min(basePriceData.Volume, targetData.Volume),
                        Risk = CalculateArbitrageRisk(basePriceData, targetData),
                        RouteDistance = CalculateRouteDistance(basePriceData.Region, targetData.Region)
                    };

                    _arbitrageOpportunities.Add(opportunity);
                }
            }
        }

        _totalOpportunities = _arbitrageOpportunities.Count;
    }

    private double CalculateArbitrageRisk(HoloRegionalMarketData source, HoloRegionalMarketData target)
    {
        // Simplified risk calculation based on volume and price volatility
        var volumeRisk = Math.Max(0, 50 - Math.Min(source.Volume, target.Volume) / 100.0);
        var priceRisk = Math.Abs(target.Price - source.Price) / Math.Max(source.Price, target.Price) * 20;
        return Math.Min(100, volumeRisk + priceRisk);
    }

    private double CalculateRouteDistance(string sourceRegion, string targetRegion)
    {
        // Simplified distance calculation (in reality would use EVE jump data)
        return _random.NextDouble() * 20 + 5; // 5-25 jumps
    }

    private void UpdateRegionalTrends()
    {
        _regionalTrends.Clear();

        if (!ShowRegionalTrends) return;

        foreach (var region in _eveRegions.Take(_maxRegions))
        {
            var regionData = RegionalData
                .Where(d => d.Region == region && d.ItemName == SelectedItem)
                .FirstOrDefault();

            if (regionData != null)
            {
                var trend = new RegionalTrend
                {
                    Region = region,
                    Direction = (TrendDirection)_random.Next(0, 3),
                    Strength = _random.NextDouble() * 100,
                    PriceChange24h = (_random.NextDouble() - 0.5) * 20, // ±10%
                    VolumeChange24h = (_random.NextDouble() - 0.5) * 40 // ±20%
                };

                _regionalTrends[region] = trend;
            }
        }
    }

    private void UpdateComparisonVisualization()
    {
        if (RegionalData == null || !RegionalData.Any()) return;

        _comparisonCanvas.Children.Clear();
        _comparisonElements.Clear();

        var canvasWidth = _visualizationCanvas.ActualWidth > 0 ? _visualizationCanvas.ActualWidth : 900;
        var canvasHeight = _visualizationCanvas.ActualHeight > 0 ? _visualizationCanvas.ActualHeight : 400;

        switch (ComparisonMode)
        {
            case RegionalComparisonMode.PriceComparison:
                DrawPriceComparison(canvasWidth, canvasHeight);
                break;
            case RegionalComparisonMode.VolumeAnalysis:
                DrawVolumeAnalysis(canvasWidth, canvasHeight);
                break;
            case RegionalComparisonMode.ArbitrageFinder:
                DrawArbitrageFinder(canvasWidth, canvasHeight);
                break;
            case RegionalComparisonMode.TrendAnalysis:
                DrawTrendAnalysis(canvasWidth, canvasHeight);
                break;
        }
    }

    private void DrawPriceComparison(double width, double height)
    {
        var currentItemData = RegionalData
            .Where(d => d.ItemName == SelectedItem)
            .OrderBy(d => d.Price)
            .Take(_maxRegions)
            .ToList();

        if (!currentItemData.Any()) return;

        var margin = 40;
        var chartWidth = width - (2 * margin);
        var chartHeight = height - (2 * margin);

        var maxPrice = currentItemData.Max(d => d.Price);
        var minPrice = currentItemData.Min(d => d.Price);
        var priceRange = maxPrice - minPrice;
        if (priceRange == 0) priceRange = 1;

        // Draw background grid
        DrawGrid(width, height, margin);

        // Draw price bars
        var barWidth = chartWidth / currentItemData.Count * 0.8;
        for (int i = 0; i < currentItemData.Count; i++)
        {
            var data = currentItemData[i];
            var x = margin + (i + 0.1) * (chartWidth / currentItemData.Count);
            var barHeight = ((data.Price - minPrice) / priceRange) * chartHeight;
            var y = margin + chartHeight - barHeight;

            var priceBar = new Rectangle
            {
                Width = barWidth,
                Height = barHeight,
                Fill = GetPriceComparisonColor(data.Price, minPrice, maxPrice),
                Stroke = new SolidColorBrush(Color.FromArgb(200, 0, 200, 255)),
                StrokeThickness = 1
            };

            Canvas.SetLeft(priceBar, x);
            Canvas.SetTop(priceBar, y);

            _comparisonCanvas.Children.Add(priceBar);
            _comparisonElements.Add(priceBar);

            // Add region label
            var label = new TextBlock
            {
                Text = data.Region.Length > 10 ? data.Region.Substring(0, 8) + ".." : data.Region,
                FontSize = 9,
                Foreground = Brushes.White,
                TextAlignment = TextAlignment.Center,
                Width = barWidth
            };

            Canvas.SetLeft(label, x);
            Canvas.SetTop(label, margin + chartHeight + 5);

            _comparisonCanvas.Children.Add(label);

            // Add price label
            var priceLabel = new TextBlock
            {
                Text = FormatPrice(data.Price),
                FontSize = 8,
                Foreground = Brushes.Yellow,
                TextAlignment = TextAlignment.Center,
                Width = barWidth
            };

            Canvas.SetLeft(priceLabel, x);
            Canvas.SetTop(priceLabel, y - 15);

            _comparisonCanvas.Children.Add(priceLabel);
        }

        // Add title
        var title = new TextBlock
        {
            Text = $"{SelectedItem} - Price Comparison Across Regions",
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        Canvas.SetLeft(title, width / 2 - 150);
        Canvas.SetTop(title, 10);

        _comparisonCanvas.Children.Add(title);
    }

    private void DrawVolumeAnalysis(double width, double height)
    {
        var currentItemData = RegionalData
            .Where(d => d.ItemName == SelectedItem)
            .OrderByDescending(d => d.Volume)
            .Take(_maxRegions)
            .ToList();

        if (!currentItemData.Any()) return;

        var centerX = width / 2;
        var centerY = height / 2;
        var radius = Math.Min(width, height) / 3;

        var totalVolume = currentItemData.Sum(d => d.Volume);
        if (totalVolume == 0) return;

        double currentAngle = 0;

        foreach (var data in currentItemData)
        {
            var sliceAngle = (data.Volume / (double)totalVolume) * 360;
            var slice = CreateVolumeSlice(centerX, centerY, radius, currentAngle, sliceAngle, data);

            _comparisonCanvas.Children.Add(slice);
            _comparisonElements.Add(slice);

            // Add labels
            var labelAngle = currentAngle + (sliceAngle / 2);
            var labelRadius = radius * 1.2;
            var labelX = centerX + Math.Cos(labelAngle * Math.PI / 180) * labelRadius;
            var labelY = centerY + Math.Sin(labelAngle * Math.PI / 180) * labelRadius;

            var label = new TextBlock
            {
                Text = $"{data.Region}\n{FormatVolume(data.Volume)}",
                FontSize = 9,
                Foreground = Brushes.White,
                TextAlignment = TextAlignment.Center
            };

            Canvas.SetLeft(label, labelX - 30);
            Canvas.SetTop(label, labelY - 10);

            _comparisonCanvas.Children.Add(label);

            currentAngle += sliceAngle;
        }

        // Add center title
        var title = new TextBlock
        {
            Text = $"{SelectedItem}\nVolume Distribution",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            TextAlignment = TextAlignment.Center
        };

        Canvas.SetLeft(title, centerX - 60);
        Canvas.SetTop(title, centerY - 15);

        _comparisonCanvas.Children.Add(title);
    }

    private Path CreateVolumeSlice(double centerX, double centerY, double radius, double startAngle, double sliceAngle, HoloRegionalMarketData data)
    {
        var startAngleRad = startAngle * Math.PI / 180;
        var endAngleRad = (startAngle + sliceAngle) * Math.PI / 180;

        var startPoint = new Point(
            centerX + Math.Cos(startAngleRad) * radius,
            centerY + Math.Sin(startAngleRad) * radius
        );

        var endPoint = new Point(
            centerX + Math.Cos(endAngleRad) * radius,
            centerY + Math.Sin(endAngleRad) * radius
        );

        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure
        {
            StartPoint = new Point(centerX, centerY)
        };

        pathFigure.Segments.Add(new LineSegment(startPoint, true));
        pathFigure.Segments.Add(new ArcSegment(endPoint, new Size(radius, radius), 0, sliceAngle > 180, SweepDirection.Clockwise, true));
        pathFigure.Segments.Add(new LineSegment(new Point(centerX, centerY), true));

        pathGeometry.Figures.Add(pathFigure);

        return new Path
        {
            Data = pathGeometry,
            Fill = GetVolumeSliceColor(data.Volume),
            Stroke = new SolidColorBrush(Color.FromArgb(150, 0, 200, 255)),
            StrokeThickness = 1
        };
    }

    private void DrawArbitrageFinder(double width, double height)
    {
        if (_arbitrageOpportunities.Count == 0) return;

        var topOpportunities = _arbitrageOpportunities
            .OrderByDescending(o => o.MarginPercent)
            .Take(10)
            .ToList();

        var margin = 40;
        var chartHeight = height - (2 * margin);
        var barHeight = chartHeight / topOpportunities.Count * 0.8;

        for (int i = 0; i < topOpportunities.Count; i++)
        {
            var opportunity = topOpportunities[i];
            var y = margin + i * (chartHeight / topOpportunities.Count);
            var barWidth = (opportunity.MarginPercent / 100) * (width - 200);

            var arbitrageBar = new Rectangle
            {
                Width = barWidth,
                Height = barHeight,
                Fill = GetArbitrageColor(opportunity.MarginPercent),
                Stroke = new SolidColorBrush(Color.FromArgb(200, 0, 255, 200)),
                StrokeThickness = 1
            };

            Canvas.SetLeft(arbitrageBar, margin);
            Canvas.SetTop(arbitrageBar, y);

            _comparisonCanvas.Children.Add(arbitrageBar);
            _comparisonElements.Add(arbitrageBar);

            // Add route label
            var routeLabel = new TextBlock
            {
                Text = $"{opportunity.SourceRegion} → {opportunity.TargetRegion}",
                FontSize = 10,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            };

            Canvas.SetLeft(routeLabel, margin + barWidth + 10);
            Canvas.SetTop(routeLabel, y + barHeight / 2 - 6);

            _comparisonCanvas.Children.Add(routeLabel);

            // Add margin label
            var marginLabel = new TextBlock
            {
                Text = $"{opportunity.MarginPercent:F1}%",
                FontSize = 9,
                Foreground = Brushes.Yellow,
                FontWeight = FontWeights.Bold
            };

            Canvas.SetLeft(marginLabel, margin + 5);
            Canvas.SetTop(marginLabel, y + barHeight / 2 - 6);

            _comparisonCanvas.Children.Add(marginLabel);
        }

        // Add title
        var title = new TextBlock
        {
            Text = $"Top Arbitrage Opportunities - {SelectedItem}",
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };

        Canvas.SetLeft(title, margin);
        Canvas.SetTop(title, 10);

        _comparisonCanvas.Children.Add(title);
    }

    private void DrawTrendAnalysis(double width, double height)
    {
        if (_regionalTrends.Count == 0) return;

        var trends = _regionalTrends.Values.ToList();
        var margin = 50;
        var chartWidth = width - (2 * margin);
        var chartHeight = height - (2 * margin);

        // Draw coordinate system
        var xAxis = new Line
        {
            X1 = margin, Y1 = height / 2, X2 = width - margin, Y2 = height / 2,
            Stroke = new SolidColorBrush(Color.FromArgb(100, 200, 200, 200)),
            StrokeThickness = 2
        };

        var yAxis = new Line
        {
            X1 = width / 2, Y1 = margin, X2 = width / 2, Y2 = height - margin,
            Stroke = new SolidColorBrush(Color.FromArgb(100, 200, 200, 200)),
            StrokeThickness = 2
        };

        _comparisonCanvas.Children.Add(xAxis);
        _comparisonCanvas.Children.Add(yAxis);

        // Plot regions based on price change vs volume change
        foreach (var trend in trends)
        {
            var x = width / 2 + (trend.PriceChange24h / 20) * (chartWidth / 2); // ±20% range
            var y = height / 2 - (trend.VolumeChange24h / 40) * (chartHeight / 2); // ±40% range

            var bubble = new Ellipse
            {
                Width = 8 + (trend.Strength / 100) * 12, // Size based on strength
                Height = 8 + (trend.Strength / 100) * 12,
                Fill = GetTrendColor(trend.Direction),
                Stroke = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                StrokeThickness = 1
            };

            Canvas.SetLeft(bubble, x - bubble.Width / 2);
            Canvas.SetTop(bubble, y - bubble.Height / 2);

            _comparisonCanvas.Children.Add(bubble);
            _comparisonElements.Add(bubble);

            // Add region label
            var label = new TextBlock
            {
                Text = trend.Region.Length > 8 ? trend.Region.Substring(0, 6) + ".." : trend.Region,
                FontSize = 8,
                Foreground = Brushes.White
            };

            Canvas.SetLeft(label, x + 10);
            Canvas.SetTop(label, y - 5);

            _comparisonCanvas.Children.Add(label);
        }

        // Add axis labels
        var xLabel = new TextBlock
        {
            Text = "Price Change 24h (%)",
            FontSize = 10,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        Canvas.SetLeft(xLabel, width / 2 - 50);
        Canvas.SetTop(xLabel, height - 20);

        _comparisonCanvas.Children.Add(xLabel);

        var yLabel = new TextBlock
        {
            Text = "Volume Change 24h (%)",
            FontSize = 10,
            Foreground = Brushes.White,
            RenderTransform = new RotateTransform(-90)
        };

        Canvas.SetLeft(yLabel, 15);
        Canvas.SetTop(yLabel, height / 2);

        _comparisonCanvas.Children.Add(yLabel);

        // Add title
        var title = new TextBlock
        {
            Text = $"Regional Trend Analysis - {SelectedItem}",
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };

        Canvas.SetLeft(title, width / 2 - 120);
        Canvas.SetTop(title, 10);

        _comparisonCanvas.Children.Add(title);
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
            _comparisonCanvas.Children.Add(gridLine);
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
            _comparisonCanvas.Children.Add(gridLine);
        }
    }

    #endregion

    #region UI Updates

    private void UpdateSummaryCards()
    {
        UpdateSummaryCard("BEST ARBITRAGE", $"{_bestArbitrageMargin:F1}%", GetArbitrageColor(_bestArbitrageMargin));
        UpdateSummaryCard("OPPORTUNITIES", _totalOpportunities.ToString(), Colors.Cyan);
        
        var variance = _priceVariances.ContainsKey(SelectedItem) ? _priceVariances[SelectedItem] : 0;
        UpdateSummaryCard("VARIANCE", $"{variance:F1}%", GetVarianceColor(variance));
        
        var activeRegions = RegionalData?.Where(d => d.ItemName == SelectedItem).Count() ?? 0;
        UpdateSummaryCard("REGIONS", activeRegions.ToString(), Colors.Yellow);
        
        var routeText = _bestArbitrageRoute.Length > 15 ? _bestArbitrageRoute.Substring(0, 12) + "..." : _bestArbitrageRoute;
        UpdateSummaryCard("BEST ROUTE", routeText.Length == 0 ? "NONE" : routeText, Colors.Gold);
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

    private void UpdateRegionalGrid()
    {
        var currentItemData = RegionalData
            .Where(d => d.ItemName == SelectedItem)
            .OrderByDescending(d => d.ArbitragePercent)
            .ToList();

        foreach (var data in currentItemData)
        {
            // Update trend visualization data
            if (_regionalTrends.ContainsKey(data.Region))
            {
                var trend = _regionalTrends[data.Region];
                data.TrendDirection = trend.Direction;
            }
        }

        _regionalGrid.ItemsSource = currentItemData;
    }

    private void UpdateArbitrageDisplay()
    {
        _arbitragePanel.Children.Clear();

        var topOpportunities = _arbitrageOpportunities
            .OrderByDescending(o => o.MarginPercent)
            .Take(5)
            .ToList();

        foreach (var opportunity in topOpportunities)
        {
            var opportunityCard = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(50, 0, 150, 100)),
                BorderBrush = GetArbitrageColor(opportunity.MarginPercent),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Margin = new Thickness(0, 2, 0, 2),
                Padding = new Thickness(8, 4, 8, 4)
            };

            var cardContent = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            var routeText = new TextBlock
            {
                Text = $"{opportunity.SourceRegion} → {opportunity.TargetRegion}",
                Width = 200,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };

            var marginText = new TextBlock
            {
                Text = $"{opportunity.MarginPercent:F1}%",
                Width = 60,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = GetArbitrageColor(opportunity.MarginPercent),
                TextAlignment = TextAlignment.Right
            };

            var profitText = new TextBlock
            {
                Text = FormatPrice(opportunity.PotentialProfit),
                Width = 80,
                FontSize = 9,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 255, 200)),
                TextAlignment = TextAlignment.Right
            };

            var riskText = new TextBlock
            {
                Text = $"Risk: {opportunity.Risk:F0}%",
                Width = 70,
                FontSize = 9,
                Foreground = GetRiskColor(opportunity.Risk),
                TextAlignment = TextAlignment.Right
            };

            cardContent.Children.Add(routeText);
            cardContent.Children.Add(marginText);
            cardContent.Children.Add(profitText);
            cardContent.Children.Add(riskText);

            opportunityCard.Child = cardContent;
            _arbitragePanel.Children.Add(opportunityCard);
        }
    }

    #endregion

    #region Color Helpers

    private Brush GetPriceComparisonColor(double price, double minPrice, double maxPrice)
    {
        var normalizedPrice = (price - minPrice) / (maxPrice - minPrice);
        var red = (byte)(255 * normalizedPrice);
        var green = (byte)(255 * (1 - normalizedPrice));
        return new SolidColorBrush(Color.FromArgb(200, red, green, 100));
    }

    private Brush GetVolumeSliceColor(long volume)
    {
        var hue = (_random.NextDouble() * 360);
        return new SolidColorBrush(ColorFromHSV(hue, 0.7, 0.9));
    }

    private Color ColorFromHSV(double hue, double saturation, double value)
    {
        int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        double f = hue / 60 - Math.Floor(hue / 60);

        value = value * 255;
        byte v = Convert.ToByte(value);
        byte p = Convert.ToByte(value * (1 - saturation));
        byte q = Convert.ToByte(value * (1 - f * saturation));
        byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));

        return hi switch
        {
            0 => Color.FromArgb(255, v, t, p),
            1 => Color.FromArgb(255, q, v, p),
            2 => Color.FromArgb(255, p, v, t),
            3 => Color.FromArgb(255, p, q, v),
            4 => Color.FromArgb(255, t, p, v),
            _ => Color.FromArgb(255, v, p, q)
        };
    }

    private Brush GetArbitrageColor(double marginPercent)
    {
        if (marginPercent > 50) return new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)); // Red - Very high
        if (marginPercent > 25) return new SolidColorBrush(Color.FromArgb(255, 255, 165, 0)); // Orange - High
        if (marginPercent > 10) return new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)); // Yellow - Medium
        if (marginPercent > 5) return new SolidColorBrush(Color.FromArgb(255, 173, 255, 47)); // Green-Yellow - Low
        return new SolidColorBrush(Color.FromArgb(255, 0, 255, 0)); // Green - Very low
    }

    private Color GetVarianceColor(double variance)
    {
        if (variance > 30) return Colors.Red;
        if (variance > 15) return Colors.Orange;
        if (variance > 5) return Colors.Yellow;
        return Colors.LimeGreen;
    }

    private Brush GetTrendColor(TrendDirection direction)
    {
        return direction switch
        {
            TrendDirection.Bullish => new SolidColorBrush(Colors.LimeGreen),
            TrendDirection.Bearish => new SolidColorBrush(Colors.OrangeRed),
            _ => new SolidColorBrush(Colors.Yellow)
        };
    }

    private Brush GetRiskColor(double risk)
    {
        if (risk > 70) return new SolidColorBrush(Colors.Red);
        if (risk > 40) return new SolidColorBrush(Colors.Orange);
        if (risk > 20) return new SolidColorBrush(Colors.Yellow);
        return new SolidColorBrush(Colors.LimeGreen);
    }

    #endregion

    #region Formatting Helpers

    private string FormatPrice(double price)
    {
        if (price >= 1e9) return $"{price / 1e9:F1}B";
        if (price >= 1e6) return $"{price / 1e6:F1}M";
        if (price >= 1e3) return $"{price / 1e3:F1}K";
        return $"{price:F2}";
    }

    private string FormatVolume(long volume)
    {
        if (volume >= 1e9) return $"{volume / 1e9:F1}B";
        if (volume >= 1e6) return $"{volume / 1e6:F1}M";
        if (volume >= 1e3) return $"{volume / 1e3:F1}K";
        return volume.ToString();
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
        // Animate comparison elements
        foreach (var element in _comparisonElements)
        {
            element.Opacity = 0.3 + (0.7 * _animationProgress);
        }
    }

    private void CreateRegionalParticles()
    {
        // Remove old particles
        var oldParticles = _particles.ToList();
        foreach (var particle in oldParticles)
        {
            _particleCanvas.Children.Remove(particle);
        }
        _particles.Clear();

        // Create particles based on arbitrage opportunities
        var particleCount = Math.Min(_maxParticles, _totalOpportunities * 2);

        for (int i = 0; i < particleCount; i++)
        {
            var particle = new Ellipse
            {
                Width = 3 + _random.NextDouble() * 4,
                Height = 3 + _random.NextDouble() * 4,
                Fill = GetArbitrageColor(_bestArbitrageMargin)
            };

            var startX = _random.NextDouble() * ActualWidth;
            var startY = _random.NextDouble() * ActualHeight;

            Canvas.SetLeft(particle, startX);
            Canvas.SetTop(particle, startY);

            _particleCanvas.Children.Add(particle);
            _particles.Add(particle);

            AnimateRegionalParticle(particle);
        }
    }

    private void AnimateRegionalParticle(UIElement particle)
    {
        var duration = TimeSpan.FromSeconds(4 + _random.NextDouble() * 6);

        var xAnimation = new DoubleAnimation
        {
            From = Canvas.GetLeft(particle),
            To = Canvas.GetLeft(particle) + (_random.NextDouble() - 0.5) * 300,
            Duration = duration,
            EasingFunction = new SineEase()
        };

        var yAnimation = new DoubleAnimation
        {
            From = Canvas.GetTop(particle),
            To = Canvas.GetTop(particle) + (_random.NextDouble() - 0.5) * 200,
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

    private void GenerateSampleRegionalData()
    {
        RegionalData = new ObservableCollection<HoloRegionalMarketData>();

        var items = new[] { "Tritanium", "Pyerite", "Mexallon", "Isogen", "Nocxium", "Zydrine", "Megacyte", "PLEX", "Skill Injector" };
        var basePrice = new Dictionary<string, double>
        {
            ["Tritanium"] = 5.50,
            ["Pyerite"] = 12.80,
            ["Mexallon"] = 85.00,
            ["Isogen"] = 125.00,
            ["Nocxium"] = 950.00,
            ["Zydrine"] = 2100.00,
            ["Megacyte"] = 4500.00,
            ["PLEX"] = 2800000.00,
            ["Skill Injector"] = 850000000.00
        };

        foreach (var item in items)
        {
            var itemBasePrice = basePrice[item];

            foreach (var region in _eveRegions.Take(_maxRegions))
            {
                var priceVariation = (_random.NextDouble() - 0.5) * 0.4; // ±20% variation
                var volumeBase = GetBaseVolumeForItem(item);
                var volumeVariation = _random.NextDouble() * 0.8 + 0.2; // 20-100% variation

                var regionalData = new HoloRegionalMarketData
                {
                    ItemName = item,
                    Region = region,
                    Price = itemBasePrice * (1 + priceVariation),
                    Volume = (long)(volumeBase * volumeVariation),
                    TrendDirection = (TrendDirection)_random.Next(0, 3),
                    LastUpdated = DateTime.Now.AddMinutes(-_random.Next(60))
                };

                RegionalData.Add(regionalData);
            }
        }
    }

    private long GetBaseVolumeForItem(string itemName)
    {
        return itemName switch
        {
            "Tritanium" => 50000000,
            "Pyerite" => 25000000,
            "Mexallon" => 8000000,
            "Isogen" => 5000000,
            "Nocxium" => 2000000,
            "Zydrine" => 800000,
            "Megacyte" => 400000,
            "PLEX" => 15000,
            "Skill Injector" => 2500,
            _ => 1000000
        };
    }

    private void RefreshRegionalData()
    {
        GenerateSampleRegionalData();
        UpdateRegionalComparisonData();
    }

    #endregion

    #region Public Methods

    public RegionalComparisonAnalysis GetRegionalComparisonAnalysis()
    {
        return new RegionalComparisonAnalysis
        {
            SelectedItem = SelectedItem,
            ComparisonMode = ComparisonMode,
            BestArbitrageMargin = _bestArbitrageMargin,
            BestArbitrageRoute = _bestArbitrageRoute,
            TotalOpportunities = _totalOpportunities,
            PriceVariance = _priceVariances.ContainsKey(SelectedItem) ? _priceVariances[SelectedItem] : 0,
            ActiveRegions = RegionalData?.Where(d => d.ItemName == SelectedItem).Count() ?? 0,
            ArbitrageOpportunities = _arbitrageOpportunities,
            RegionalTrends = _regionalTrends,
            IsAnalyzing = _updateTimer?.IsEnabled ?? false,
            PerformanceMode = _isInSimplifiedMode ? "Simplified" : "Full"
        };
    }

    public void SetSelectedItem(string itemName)
    {
        SelectedItem = itemName;
        UpdateRegionalComparisonData();
    }

    public void SetComparisonMode(RegionalComparisonMode mode)
    {
        ComparisonMode = mode;
        UpdateComparisonVisualization();
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void SetSimplifiedMode(bool enabled)
    {
        _isInSimplifiedMode = enabled;
        EnableParticleEffects = !enabled;
        EnableAnimations = !enabled;
        _maxParticles = enabled ? 10 : 30;
        _maxRegions = enabled ? 8 : 15;

        if (IsLoaded)
        {
            UpdateRegionalComparisonData();
        }
    }

    public bool IsInSimplifiedMode => _isInSimplifiedMode;

    #endregion

    #region Property Change Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloRegionalMarketComparison control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloRegionalMarketComparison control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnRegionalDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloRegionalMarketComparison control && control.IsLoaded)
        {
            control.UpdateRegionalComparisonData();
        }
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloRegionalMarketComparison control && control.IsLoaded)
        {
            control.UpdateRegionalComparisonData();
        }
    }

    private static void OnComparisonModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloRegionalMarketComparison control && control.IsLoaded)
        {
            control.UpdateComparisonVisualization();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloRegionalMarketComparison control)
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
        if (d is HoloRegionalMarketComparison control && control.IsLoaded)
        {
            control.UpdateRegionalComparisonData();
        }
    }

    private static void OnShowArbitrageOpportunitiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloRegionalMarketComparison control && control.IsLoaded)
        {
            control.UpdateArbitrageOpportunities();
            control.UpdateArbitrageDisplay();
        }
    }

    private static void OnShowRegionalTrendsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloRegionalMarketComparison control && control.IsLoaded)
        {
            control.UpdateRegionalTrends();
        }
    }

    private static void OnShowVolumeAnalysisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloRegionalMarketComparison control && control.IsLoaded)
        {
            control.UpdateComparisonVisualization();
        }
    }

    private static void OnAutoRefreshChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloRegionalMarketComparison control)
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
        if (d is HoloRegionalMarketComparison control)
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
            UpdateRegionalComparisonData();
        }
    }

    #endregion
}

#region Supporting Classes

public class HoloRegionalMarketData : INotifyPropertyChanged
{
    private TrendDirection _trendDirection;
    private double _arbitragePercent;

    public string ItemName { get; set; }
    public string Region { get; set; }
    public double Price { get; set; }
    public long Volume { get; set; }
    public DateTime LastUpdated { get; set; }

    public TrendDirection TrendDirection
    {
        get => _trendDirection;
        set
        {
            _trendDirection = value;
            OnPropertyChanged(nameof(TrendDirection));
            OnPropertyChanged(nameof(TrendColor));
            OnPropertyChanged(nameof(TrendPoints));
        }
    }

    public double ArbitragePercent
    {
        get => _arbitragePercent;
        set
        {
            _arbitragePercent = value;
            OnPropertyChanged(nameof(ArbitragePercent));
            OnPropertyChanged(nameof(FormattedArbitragePercent));
        }
    }

    public string FormattedPrice => Price.ToString("N2");
    public string FormattedVolume => Volume.ToString("N0");
    public string FormattedArbitragePercent => $"{ArbitragePercent:F1}%";

    public Brush TrendColor => TrendDirection switch
    {
        TrendDirection.Bullish => new SolidColorBrush(Colors.LimeGreen),
        TrendDirection.Bearish => new SolidColorBrush(Colors.OrangeRed),
        _ => new SolidColorBrush(Colors.Yellow)
    };

    public PointCollection TrendPoints => TrendDirection switch
    {
        TrendDirection.Bullish => new PointCollection { new Point(6, 10), new Point(0, 2), new Point(12, 2) },
        TrendDirection.Bearish => new PointCollection { new Point(6, 2), new Point(0, 10), new Point(12, 10) },
        _ => new PointCollection { new Point(0, 6), new Point(12, 6), new Point(8, 2), new Point(12, 6), new Point(8, 10) }
    };

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class ArbitrageOpportunity
{
    public string ItemName { get; set; }
    public string SourceRegion { get; set; }
    public string TargetRegion { get; set; }
    public double SourcePrice { get; set; }
    public double TargetPrice { get; set; }
    public double MarginPercent { get; set; }
    public double PotentialProfit { get; set; }
    public double Risk { get; set; }
    public double RouteDistance { get; set; }
}

public class RegionalTrend
{
    public string Region { get; set; }
    public TrendDirection Direction { get; set; }
    public double Strength { get; set; }
    public double PriceChange24h { get; set; }
    public double VolumeChange24h { get; set; }
}

public enum RegionalComparisonMode
{
    PriceComparison,
    VolumeAnalysis,
    ArbitrageFinder,
    TrendAnalysis
}

public enum TrendDirection
{
    Bullish,
    Bearish,
    Neutral
}

public class RegionalComparisonAnalysis
{
    public string SelectedItem { get; set; }
    public RegionalComparisonMode ComparisonMode { get; set; }
    public double BestArbitrageMargin { get; set; }
    public string BestArbitrageRoute { get; set; }
    public int TotalOpportunities { get; set; }
    public double PriceVariance { get; set; }
    public int ActiveRegions { get; set; }
    public List<ArbitrageOpportunity> ArbitrageOpportunities { get; set; }
    public Dictionary<string, RegionalTrend> RegionalTrends { get; set; }
    public bool IsAnalyzing { get; set; }
    public string PerformanceMode { get; set; }
}

#endregion