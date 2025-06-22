// ==========================================================================
// HoloMarketOrderBook.cs - Holographic Market Order Book Display
// ==========================================================================
// Advanced order book visualization featuring real-time buy/sell order tracking,
// depth analysis, EVE-style market data, and holographic order flow effects.
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
/// Holographic market order book with real-time depth analysis and interactive order visualization
/// </summary>
public class HoloMarketOrderBook : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloMarketOrderBook),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloMarketOrderBook),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty BuyOrdersProperty =
        DependencyProperty.Register(nameof(BuyOrders), typeof(ObservableCollection<HoloMarketOrder>), typeof(HoloMarketOrderBook),
            new PropertyMetadata(null, OnBuyOrdersChanged));

    public static readonly DependencyProperty SellOrdersProperty =
        DependencyProperty.Register(nameof(SellOrders), typeof(ObservableCollection<HoloMarketOrder>), typeof(HoloMarketOrderBook),
            new PropertyMetadata(null, OnSellOrdersChanged));

    public static readonly DependencyProperty SelectedOrderProperty =
        DependencyProperty.Register(nameof(SelectedOrder), typeof(HoloMarketOrder), typeof(HoloMarketOrderBook),
            new PropertyMetadata(null, OnSelectedOrderChanged));

    public static readonly DependencyProperty DisplayModeProperty =
        DependencyProperty.Register(nameof(DisplayMode), typeof(OrderBookDisplayMode), typeof(HoloMarketOrderBook),
            new PropertyMetadata(OrderBookDisplayMode.Full, OnDisplayModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloMarketOrderBook),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloMarketOrderBook),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowDepthVisualizationProperty =
        DependencyProperty.Register(nameof(ShowDepthVisualization), typeof(bool), typeof(HoloMarketOrderBook),
            new PropertyMetadata(true, OnShowDepthVisualizationChanged));

    public static readonly DependencyProperty ShowCumulativeVolumeProperty =
        DependencyProperty.Register(nameof(ShowCumulativeVolume), typeof(bool), typeof(HoloMarketOrderBook),
            new PropertyMetadata(true, OnShowCumulativeVolumeChanged));

    public static readonly DependencyProperty ShowSpreadIndicatorProperty =
        DependencyProperty.Register(nameof(ShowSpreadIndicator), typeof(bool), typeof(HoloMarketOrderBook),
            new PropertyMetadata(true, OnShowSpreadIndicatorChanged));

    public static readonly DependencyProperty MaxOrdersDisplayedProperty =
        DependencyProperty.Register(nameof(MaxOrdersDisplayed), typeof(int), typeof(HoloMarketOrderBook),
            new PropertyMetadata(20, OnMaxOrdersDisplayedChanged));

    public static readonly DependencyProperty UpdateIntervalProperty =
        DependencyProperty.Register(nameof(UpdateInterval), typeof(TimeSpan), typeof(HoloMarketOrderBook),
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

    public ObservableCollection<HoloMarketOrder> BuyOrders
    {
        get => (ObservableCollection<HoloMarketOrder>)GetValue(BuyOrdersProperty);
        set => SetValue(BuyOrdersProperty, value);
    }

    public ObservableCollection<HoloMarketOrder> SellOrders
    {
        get => (ObservableCollection<HoloMarketOrder>)GetValue(SellOrdersProperty);
        set => SetValue(SellOrdersProperty, value);
    }

    public HoloMarketOrder SelectedOrder
    {
        get => (HoloMarketOrder)GetValue(SelectedOrderProperty);
        set => SetValue(SelectedOrderProperty, value);
    }

    public OrderBookDisplayMode DisplayMode
    {
        get => (OrderBookDisplayMode)GetValue(DisplayModeProperty);
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

    public bool ShowDepthVisualization
    {
        get => (bool)GetValue(ShowDepthVisualizationProperty);
        set => SetValue(ShowDepthVisualizationProperty, value);
    }

    public bool ShowCumulativeVolume
    {
        get => (bool)GetValue(ShowCumulativeVolumeProperty);
        set => SetValue(ShowCumulativeVolumeProperty, value);
    }

    public bool ShowSpreadIndicator
    {
        get => (bool)GetValue(ShowSpreadIndicatorProperty);
        set => SetValue(ShowSpreadIndicatorProperty, value);
    }

    public int MaxOrdersDisplayed
    {
        get => (int)GetValue(MaxOrdersDisplayedProperty);
        set => SetValue(MaxOrdersDisplayedProperty, value);
    }

    public TimeSpan UpdateInterval
    {
        get => (TimeSpan)GetValue(UpdateIntervalProperty);
        set => SetValue(UpdateIntervalProperty, value);
    }

    #endregion

    #region Fields

    private Grid _mainGrid;
    private Canvas _depthCanvas;
    private Canvas _particleCanvas;
    private DataGrid _buyOrdersGrid;
    private DataGrid _sellOrdersGrid;
    private Border _spreadIndicator;
    private TextBlock _spreadText;
    
    private DispatcherTimer _updateTimer;
    private DispatcherTimer _animationTimer;
    
    private readonly Random _random = new();
    private List<UIElement> _depthBars;
    private List<UIElement> _particles;
    private List<UIElement> _orderFlowIndicators;
    
    // Market data analysis
    private double _bestBuyPrice = 0;
    private double _bestSellPrice = 0;
    private double _spread = 0;
    private double _spreadPercent = 0;
    private long _totalBuyVolume = 0;
    private long _totalSellVolume = 0;
    private double _maxCumulativeVolume = 0;
    
    // Animation state
    private double _animationProgress = 0.0;
    private bool _isAnimating = false;
    private readonly Dictionary<string, DateTime> _orderAnimationTimes = new();
    
    // Performance optimization
    private bool _isInSimplifiedMode = false;
    private int _maxParticles = 50;

    #endregion

    #region Constructor

    public HoloMarketOrderBook()
    {
        InitializeComponent();
        InitializeOrderBookSystem();
        SetupEventHandlers();
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 600;
        Height = 800;
        Background = new SolidColorBrush(Color.FromArgb(30, 0, 50, 100));
        
        // Create main layout
        _mainGrid = new Grid();
        Content = _mainGrid;

        // Define rows
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) }); // Header
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Sell orders
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Spread indicator
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Buy orders
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(200) }); // Depth visualization

        CreateHeader();
        CreateOrderGrids();
        CreateSpreadIndicator();
        CreateDepthVisualization();
        CreateParticleLayer();
    }

    private void CreateHeader()
    {
        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 100, 200))
        };

        var titleText = new TextBlock
        {
            Text = "MARKET ORDER BOOK",
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 200, 255)),
            Margin = new Thickness(10, 0, 20, 0)
        };

        var modeCombo = new ComboBox
        {
            Width = 120,
            SelectedIndex = 0,
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 50, 100)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 0, 150, 255))
        };

        modeCombo.Items.Add("Full Display");
        modeCombo.Items.Add("Compact");
        modeCombo.Items.Add("Depth Only");
        modeCombo.SelectionChanged += (s, e) => DisplayMode = (OrderBookDisplayMode)modeCombo.SelectedIndex;

        headerPanel.Children.Add(titleText);
        headerPanel.Children.Add(modeCombo);

        Grid.SetRow(headerPanel, 0);
        _mainGrid.Children.Add(headerPanel);
    }

    private void CreateOrderGrids()
    {
        // Create sell orders grid (top)
        _sellOrdersGrid = CreateOrderGrid(true);
        Grid.SetRow(_sellOrdersGrid, 1);
        _mainGrid.Children.Add(_sellOrdersGrid);

        // Create buy orders grid (bottom)
        _buyOrdersGrid = CreateOrderGrid(false);
        Grid.SetRow(_buyOrdersGrid, 3);
        _mainGrid.Children.Add(_buyOrdersGrid);
    }

    private DataGrid CreateOrderGrid(bool isSellOrders)
    {
        var grid = new DataGrid
        {
            Background = new SolidColorBrush(Color.FromArgb(50, 0, 50, 100)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 150, 255)),
            BorderThickness = new Thickness(1),
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            CanUserAddRows = false,
            CanUserDeleteRows = false,
            CanUserReorderColumns = false,
            CanUserResizeColumns = true,
            CanUserSortColumns = false,
            IsReadOnly = true,
            SelectionMode = DataGridSelectionMode.Single,
            AutoGenerateColumns = false,
            Margin = new Thickness(5)
        };

        // Create columns
        var priceColumn = new DataGridTextColumn
        {
            Header = "Price (ISK)",
            Binding = new Binding("FormattedPrice"),
            Width = new DataGridLength(120),
            ElementStyle = CreateCellStyle(isSellOrders ? Colors.OrangeRed : Colors.LimeGreen)
        };

        var volumeColumn = new DataGridTextColumn
        {
            Header = "Volume",
            Binding = new Binding("FormattedVolume"),
            Width = new DataGridLength(100),
            ElementStyle = CreateCellStyle(Colors.White)
        };

        var cumulativeColumn = new DataGridTextColumn
        {
            Header = "Cumulative",
            Binding = new Binding("FormattedCumulative"),
            Width = new DataGridLength(100),
            ElementStyle = CreateCellStyle(Colors.Cyan)
        };

        var depthColumn = new DataGridTemplateColumn
        {
            Header = "Depth",
            Width = new DataGridLength(150),
            CellTemplate = CreateDepthBarTemplate(isSellOrders)
        };

        grid.Columns.Add(priceColumn);
        grid.Columns.Add(volumeColumn);
        grid.Columns.Add(cumulativeColumn);
        
        if (ShowDepthVisualization)
        {
            grid.Columns.Add(depthColumn);
        }

        // Set row style
        grid.RowStyle = CreateRowStyle(isSellOrders);

        return grid;
    }

    private Style CreateCellStyle(Color color)
    {
        var style = new Style(typeof(TextBlock));
        style.Setters.Add(new Setter(TextBlock.ForegroundProperty, new SolidColorBrush(color)));
        style.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.Bold));
        style.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right));
        return style;
    }

    private DataTemplate CreateDepthBarTemplate(bool isSellOrders)
    {
        var template = new DataTemplate();
        
        var factory = new FrameworkElementFactory(typeof(Canvas));
        factory.SetValue(Canvas.HeightProperty, 20.0);
        
        var barFactory = new FrameworkElementFactory(typeof(Rectangle));
        barFactory.SetValue(Rectangle.HeightProperty, 18.0);
        barFactory.SetValue(Rectangle.FillProperty, new SolidColorBrush(
            isSellOrders ? Color.FromArgb(100, 255, 100, 0) : Color.FromArgb(100, 0, 255, 100)));
        barFactory.SetBinding(Rectangle.WidthProperty, new Binding("DepthBarWidth"));
        
        factory.AppendChild(barFactory);
        template.VisualTree = factory;
        
        return template;
    }

    private Style CreateRowStyle(bool isSellOrders)
    {
        var style = new Style(typeof(DataGridRow));
        
        var trigger = new Trigger
        {
            Property = DataGridRow.IsMouseOverProperty,
            Value = true
        };
        
        trigger.Setters.Add(new Setter(DataGridRow.BackgroundProperty, 
            new SolidColorBrush(Color.FromArgb(100, 0, 200, 255))));
        
        style.Triggers.Add(trigger);
        
        return style;
    }

    private void CreateSpreadIndicator()
    {
        _spreadIndicator = new Border
        {
            Height = 60,
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(100, 255, 100, 0), 0),
                    new GradientStop(Color.FromArgb(50, 255, 200, 0), 0.5),
                    new GradientStop(Color.FromArgb(100, 0, 255, 100), 1)
                }
            },
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 0, 200, 255)),
            BorderThickness = new Thickness(2),
            Margin = new Thickness(5),
            CornerRadius = new CornerRadius(5)
        };

        var spreadPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        _spreadText = new TextBlock
        {
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White,
            TextAlignment = TextAlignment.Center
        };

        spreadPanel.Children.Add(_spreadText);
        _spreadIndicator.Child = spreadPanel;

        Grid.SetRow(_spreadIndicator, 2);
        _mainGrid.Children.Add(_spreadIndicator);
    }

    private void CreateDepthVisualization()
    {
        var depthBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(30, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 150, 255)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(5),
            CornerRadius = new CornerRadius(5)
        };

        _depthCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            ClipToBounds = true
        };

        depthBorder.Child = _depthCanvas;
        Grid.SetRow(depthBorder, 4);
        _mainGrid.Children.Add(depthBorder);
    }

    private void CreateParticleLayer()
    {
        _particleCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false
        };

        Grid.SetRowSpan(_particleCanvas, 5);
        _mainGrid.Children.Add(_particleCanvas);
    }

    private void InitializeOrderBookSystem()
    {
        _depthBars = new List<UIElement>();
        _particles = new List<UIElement>();
        _orderFlowIndicators = new List<UIElement>();
        
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
        StartOrderBookVisualization();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopOrderBookVisualization();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateDepthVisualization();
    }

    private void UpdateTimer_Tick(object sender, EventArgs e)
    {
        UpdateOrderBookData();
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

    #region Order Book Visualization

    public void StartOrderBookVisualization()
    {
        if (!_updateTimer.IsEnabled)
        {
            _updateTimer.Start();
            
            if (EnableAnimations)
            {
                _animationTimer.Start();
            }
            
            UpdateOrderBookData();
        }
    }

    public void StopOrderBookVisualization()
    {
        _updateTimer?.Stop();
        _animationTimer?.Stop();
    }

    private void UpdateOrderBookData()
    {
        if (BuyOrders == null || SellOrders == null)
        {
            GenerateSampleOrders();
        }

        CalculateMarketMetrics();
        UpdateOrderGrids();
        UpdateSpreadDisplay();
        UpdateDepthVisualization();
        
        if (EnableParticleEffects && !_isInSimplifiedMode)
        {
            CreateOrderFlowParticles();
        }
        
        StartAnimation();
    }

    private void CalculateMarketMetrics()
    {
        // Calculate best bid/ask
        _bestBuyPrice = BuyOrders?.FirstOrDefault()?.Price ?? 0;
        _bestSellPrice = SellOrders?.FirstOrDefault()?.Price ?? 0;
        
        // Calculate spread
        if (_bestBuyPrice > 0 && _bestSellPrice > 0)
        {
            _spread = _bestSellPrice - _bestBuyPrice;
            _spreadPercent = (_spread / _bestBuyPrice) * 100;
        }
        
        // Calculate total volumes
        _totalBuyVolume = BuyOrders?.Sum(o => o.Volume) ?? 0;
        _totalSellVolume = SellOrders?.Sum(o => o.Volume) ?? 0;
        
        // Calculate cumulative volumes and depth bars
        CalculateCumulativeVolumes();
    }

    private void CalculateCumulativeVolumes()
    {
        long cumulativeBuy = 0;
        long cumulativeSell = 0;
        
        // Process buy orders (highest to lowest price)
        if (BuyOrders != null)
        {
            foreach (var order in BuyOrders.Take(MaxOrdersDisplayed))
            {
                cumulativeBuy += order.Volume;
                order.CumulativeVolume = cumulativeBuy;
                order.DepthBarWidth = (order.Volume / (double)_totalBuyVolume) * 140; // Max width 140px
            }
        }
        
        // Process sell orders (lowest to highest price)
        if (SellOrders != null)
        {
            foreach (var order in SellOrders.Take(MaxOrdersDisplayed))
            {
                cumulativeSell += order.Volume;
                order.CumulativeVolume = cumulativeSell;
                order.DepthBarWidth = (order.Volume / (double)_totalSellVolume) * 140; // Max width 140px
            }
        }
        
        _maxCumulativeVolume = Math.Max(cumulativeBuy, cumulativeSell);
    }

    private void UpdateOrderGrids()
    {
        // Update buy orders (show highest prices first)
        var buyOrdersToShow = BuyOrders?
            .OrderByDescending(o => o.Price)
            .Take(MaxOrdersDisplayed)
            .ToList() ?? new List<HoloMarketOrder>();
            
        _buyOrdersGrid.ItemsSource = buyOrdersToShow;

        // Update sell orders (show lowest prices first)
        var sellOrdersToShow = SellOrders?
            .OrderBy(o => o.Price)
            .Take(MaxOrdersDisplayed)
            .ToList() ?? new List<HoloMarketOrder>();
            
        _sellOrdersGrid.ItemsSource = sellOrdersToShow;
    }

    private void UpdateSpreadDisplay()
    {
        if (_spread > 0)
        {
            _spreadText.Text = $"SPREAD: {_spread:N2} ISK ({_spreadPercent:F2}%)\n" +
                              $"BID: {_bestBuyPrice:N2} | ASK: {_bestSellPrice:N2}";
        }
        else
        {
            _spreadText.Text = "CALCULATING SPREAD...";
        }
        
        // Animate spread indicator color based on spread size
        var spreadIntensity = Math.Min(1.0, _spreadPercent / 5.0); // 5% = full intensity
        var color = Color.FromArgb(
            (byte)(100 + (155 * spreadIntensity)),
            (byte)(255 * (1 - spreadIntensity)),
            (byte)(100 + (155 * spreadIntensity)),
            0
        );
        
        _spreadIndicator.Background = new SolidColorBrush(color);
    }

    private void UpdateDepthVisualization()
    {
        if (!ShowDepthVisualization) return;
        
        _depthCanvas.Children.Clear();
        _depthBars.Clear();
        
        var canvasWidth = _depthCanvas.ActualWidth > 0 ? _depthCanvas.ActualWidth : 590;
        var canvasHeight = _depthCanvas.ActualHeight > 0 ? _depthCanvas.ActualHeight : 190;
        var centerY = canvasHeight / 2;
        
        // Draw depth chart background
        DrawDepthBackground(canvasWidth, canvasHeight, centerY);
        
        // Draw buy side depth (bottom half, green)
        DrawDepthSide(BuyOrders?.OrderByDescending(o => o.Price).Take(MaxOrdersDisplayed), 
                     canvasWidth, centerY, canvasHeight, true);
        
        // Draw sell side depth (top half, red)
        DrawDepthSide(SellOrders?.OrderBy(o => o.Price).Take(MaxOrdersDisplayed), 
                     canvasWidth, centerY, canvasHeight, false);
        
        // Draw center line (spread indicator)
        DrawSpreadLine(canvasWidth, centerY);
    }

    private void DrawDepthBackground(double width, double height, double centerY)
    {
        // Draw grid lines
        for (int i = 1; i < 10; i++)
        {
            var y = (height / 10) * i;
            var gridLine = new Line
            {
                X1 = 0, Y1 = y, X2 = width, Y2 = y,
                Stroke = new SolidColorBrush(Color.FromArgb(30, 100, 150, 200)),
                StrokeThickness = 1
            };
            _depthCanvas.Children.Add(gridLine);
        }
        
        // Draw vertical lines for price levels
        for (int i = 1; i < 20; i++)
        {
            var x = (width / 20) * i;
            var gridLine = new Line
            {
                X1 = x, Y1 = 0, X2 = x, Y2 = height,
                Stroke = new SolidColorBrush(Color.FromArgb(20, 100, 150, 200)),
                StrokeThickness = 1
            };
            _depthCanvas.Children.Add(gridLine);
        }
    }

    private void DrawDepthSide(IEnumerable<HoloMarketOrder> orders, double width, double centerY, double height, bool isBuyOrders)
    {
        if (orders == null) return;
        
        var orderList = orders.ToList();
        if (!orderList.Any()) return;
        
        var maxVolume = orderList.Max(o => o.CumulativeVolume);
        if (maxVolume == 0) return;
        
        var points = new PointCollection();
        var sideHeight = isBuyOrders ? (height - centerY) : centerY;
        var baseY = isBuyOrders ? height : 0;
        
        // Start from center
        points.Add(new Point(0, centerY));
        
        var pixelsPerOrder = width / Math.Max(1, orderList.Count);
        
        for (int i = 0; i < orderList.Count; i++)
        {
            var order = orderList[i];
            var x = i * pixelsPerOrder;
            var volumeRatio = order.CumulativeVolume / (double)maxVolume;
            var y = isBuyOrders 
                ? centerY + (volumeRatio * sideHeight)
                : centerY - (volumeRatio * sideHeight);
            
            points.Add(new Point(x, y));
        }
        
        // Close the shape
        points.Add(new Point(width, centerY));
        
        var depthPolygon = new Polygon
        {
            Points = points,
            Fill = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(isBuyOrders ? (byte)150 : (byte)0, 0, (byte)(isBuyOrders ? 255 : 0), isBuyOrders ? (byte)0 : (byte)255), 0),
                    new GradientStop(Color.FromArgb(50, 0, isBuyOrders ? (byte)255 : (byte)0, isBuyOrders ? (byte)0 : (byte)255), 1)
                }
            },
            Stroke = new SolidColorBrush(Color.FromArgb(200, 0, isBuyOrders ? (byte)255 : (byte)0, isBuyOrders ? (byte)0 : (byte)255)),
            StrokeThickness = 2
        };
        
        _depthCanvas.Children.Add(depthPolygon);
        _depthBars.Add(depthPolygon);
    }

    private void DrawSpreadLine(double width, double centerY)
    {
        var spreadLine = new Line
        {
            X1 = 0, Y1 = centerY, X2 = width, Y2 = centerY,
            Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 200, 0)),
            StrokeThickness = 3,
            StrokeDashArray = new DoubleCollection { 10, 5 }
        };
        
        _depthCanvas.Children.Add(spreadLine);
        
        // Add spread value text
        var spreadLabel = new TextBlock
        {
            Text = $"SPREAD: {_spread:N2}",
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Background = new SolidColorBrush(Color.FromArgb(150, 0, 0, 0))
        };
        
        Canvas.SetLeft(spreadLabel, width / 2 - 40);
        Canvas.SetTop(spreadLabel, centerY - 20);
        _depthCanvas.Children.Add(spreadLabel);
    }

    private void CreateOrderFlowParticles()
    {
        // Remove old particles
        var oldParticles = _particles.ToList();
        foreach (var particle in oldParticles)
        {
            _particleCanvas.Children.Remove(particle);
        }
        _particles.Clear();
        
        // Create new particles for recent order activity
        CreateBuyOrderParticles();
        CreateSellOrderParticles();
    }

    private void CreateBuyOrderParticles()
    {
        for (int i = 0; i < Math.Min(5, _totalBuyVolume / 1000); i++)
        {
            var particle = new Ellipse
            {
                Width = 4,
                Height = 4,
                Fill = new SolidColorBrush(Color.FromArgb(200, 0, 255, 100)),
                Tag = "buy_particle"
            };
            
            var startX = _random.NextDouble() * ActualWidth;
            var startY = ActualHeight * 0.75; // Buy orders area
            
            Canvas.SetLeft(particle, startX);
            Canvas.SetTop(particle, startY);
            
            _particleCanvas.Children.Add(particle);
            _particles.Add(particle);
            
            // Animate particle movement
            AnimateParticle(particle, true);
        }
    }

    private void CreateSellOrderParticles()
    {
        for (int i = 0; i < Math.Min(5, _totalSellVolume / 1000); i++)
        {
            var particle = new Ellipse
            {
                Width = 4,
                Height = 4,
                Fill = new SolidColorBrush(Color.FromArgb(200, 255, 100, 0)),
                Tag = "sell_particle"
            };
            
            var startX = _random.NextDouble() * ActualWidth;
            var startY = ActualHeight * 0.25; // Sell orders area
            
            Canvas.SetLeft(particle, startX);
            Canvas.SetTop(particle, startY);
            
            _particleCanvas.Children.Add(particle);
            _particles.Add(particle);
            
            // Animate particle movement
            AnimateParticle(particle, false);
        }
    }

    private void AnimateParticle(UIElement particle, bool isBuyOrder)
    {
        var duration = TimeSpan.FromSeconds(2 + _random.NextDouble() * 3);
        
        // X animation (drift)
        var xAnimation = new DoubleAnimation
        {
            From = Canvas.GetLeft(particle),
            To = Canvas.GetLeft(particle) + (_random.NextDouble() - 0.5) * 100,
            Duration = duration,
            EasingFunction = new SineEase()
        };
        
        // Y animation (float towards center)
        var yAnimation = new DoubleAnimation
        {
            From = Canvas.GetTop(particle),
            To = ActualHeight * 0.5 + (_random.NextDouble() - 0.5) * 50,
            Duration = duration,
            EasingFunction = new SineEase()
        };
        
        // Opacity animation (fade out)
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
        // Animate order rows appearing
        AnimateGridRows(_buyOrdersGrid);
        AnimateGridRows(_sellOrdersGrid);
        
        // Animate depth bars
        foreach (var bar in _depthBars)
        {
            if (bar is Polygon polygon)
            {
                polygon.Opacity = 0.3 + (0.7 * _animationProgress);
            }
        }
    }

    private void AnimateGridRows(DataGrid grid)
    {
        // Implementation would animate each row's appearance
        // This requires accessing the DataGrid's visual tree
    }

    private void UpdateParticleEffects()
    {
        // Clean up completed particle animations
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

    private void GenerateSampleOrders()
    {
        BuyOrders = new ObservableCollection<HoloMarketOrder>();
        SellOrders = new ObservableCollection<HoloMarketOrder>();
        
        var basePrice = 1000000.0; // 1M ISK
        var spreadBase = basePrice * 0.01; // 1% spread
        
        // Generate buy orders (below market price)
        for (int i = 0; i < 25; i++)
        {
            var priceOffset = (i + 1) * (spreadBase / 25) + (_random.NextDouble() - 0.5) * spreadBase * 0.1;
            var volume = (long)(100 + _random.NextDouble() * 1000);
            
            BuyOrders.Add(new HoloMarketOrder
            {
                OrderId = $"BUY_{i}",
                Price = basePrice - priceOffset,
                Volume = volume,
                IsBuyOrder = true,
                Timestamp = DateTime.Now.AddMinutes(-_random.Next(60)),
                Location = "Jita IV - Moon 4",
                MinVolume = Math.Min(1, volume / 10),
                Duration = 90,
                Range = 32767
            });
        }
        
        // Generate sell orders (above market price)
        for (int i = 0; i < 25; i++)
        {
            var priceOffset = (i + 1) * (spreadBase / 25) + (_random.NextDouble() - 0.5) * spreadBase * 0.1;
            var volume = (long)(100 + _random.NextDouble() * 1000);
            
            SellOrders.Add(new HoloMarketOrder
            {
                OrderId = $"SELL_{i}",
                Price = basePrice + spreadBase + priceOffset,
                Volume = volume,
                IsBuyOrder = false,
                Timestamp = DateTime.Now.AddMinutes(-_random.Next(60)),
                Location = "Jita IV - Moon 4",
                MinVolume = Math.Min(1, volume / 10),
                Duration = 90,
                Range = 0
            });
        }
    }

    #endregion

    #region Public Methods

    public OrderBookAnalysis GetOrderBookAnalysis()
    {
        return new OrderBookAnalysis
        {
            BestBidPrice = _bestBuyPrice,
            BestAskPrice = _bestSellPrice,
            Spread = _spread,
            SpreadPercent = _spreadPercent,
            TotalBuyVolume = _totalBuyVolume,
            TotalSellVolume = _totalSellVolume,
            BuyOrderCount = BuyOrders?.Count ?? 0,
            SellOrderCount = SellOrders?.Count ?? 0,
            MaxCumulativeVolume = _maxCumulativeVolume,
            DisplayMode = DisplayMode,
            IsAnimating = _isAnimating,
            PerformanceMode = _isInSimplifiedMode ? "Simplified" : "Full"
        };
    }

    public void SetDisplayMode(OrderBookDisplayMode mode)
    {
        DisplayMode = mode;
        UpdateOrderBookVisualization();
    }

    public void RefreshOrders()
    {
        GenerateSampleOrders();
        UpdateOrderBookData();
    }

    private void UpdateOrderBookVisualization()
    {
        switch (DisplayMode)
        {
            case OrderBookDisplayMode.Full:
                _buyOrdersGrid.Visibility = Visibility.Visible;
                _sellOrdersGrid.Visibility = Visibility.Visible;
                _depthCanvas.Parent.SetValue(UIElement.VisibilityProperty, Visibility.Visible);
                break;
            case OrderBookDisplayMode.Compact:
                _buyOrdersGrid.Visibility = Visibility.Visible;
                _sellOrdersGrid.Visibility = Visibility.Visible;
                _depthCanvas.Parent.SetValue(UIElement.VisibilityProperty, Visibility.Collapsed);
                MaxOrdersDisplayed = 10;
                break;
            case OrderBookDisplayMode.DepthOnly:
                _buyOrdersGrid.Visibility = Visibility.Collapsed;
                _sellOrdersGrid.Visibility = Visibility.Collapsed;
                _depthCanvas.Parent.SetValue(UIElement.VisibilityProperty, Visibility.Visible);
                break;
        }
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void SetSimplifiedMode(bool enabled)
    {
        _isInSimplifiedMode = enabled;
        EnableParticleEffects = !enabled;
        EnableAnimations = !enabled;
        _maxParticles = enabled ? 10 : 50;
        MaxOrdersDisplayed = enabled ? 10 : 20;
        
        if (IsLoaded)
        {
            UpdateOrderBookData();
        }
    }

    public bool IsInSimplifiedMode => _isInSimplifiedMode;

    #endregion

    #region Property Change Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketOrderBook control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketOrderBook control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnBuyOrdersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketOrderBook control && control.IsLoaded)
        {
            control.UpdateOrderBookData();
        }
    }

    private static void OnSellOrdersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketOrderBook control && control.IsLoaded)
        {
            control.UpdateOrderBookData();
        }
    }

    private static void OnSelectedOrderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketOrderBook control)
        {
            control.UpdateOrderSelection();
        }
    }

    private static void OnDisplayModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketOrderBook control)
        {
            control.UpdateOrderBookVisualization();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketOrderBook control)
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
        if (d is HoloMarketOrderBook control && control.IsLoaded)
        {
            control.UpdateOrderBookData();
        }
    }

    private static void OnShowDepthVisualizationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketOrderBook control && control.IsLoaded)
        {
            control.UpdateDepthVisualization();
        }
    }

    private static void OnShowCumulativeVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketOrderBook control && control.IsLoaded)
        {
            control.UpdateOrderGrids();
        }
    }

    private static void OnShowSpreadIndicatorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketOrderBook control)
        {
            control._spreadIndicator.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnMaxOrdersDisplayedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketOrderBook control && control.IsLoaded)
        {
            control.UpdateOrderBookData();
        }
    }

    private static void OnUpdateIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketOrderBook control)
        {
            control._updateTimer.Interval = (TimeSpan)e.NewValue;
        }
    }

    private void UpdateHolographicEffects()
    {
        // Update holographic intensity effects
        Opacity = 0.8 + (0.2 * HolographicIntensity);
    }

    private void UpdateColorScheme()
    {
        // Update color scheme based on EVE colors
        if (IsLoaded)
        {
            UpdateOrderBookData();
        }
    }

    private void UpdateOrderSelection()
    {
        // Highlight selected order in grids
        if (SelectedOrder != null)
        {
            // Implementation would highlight the selected order
        }
    }

    #endregion
}

#region Supporting Classes

public class HoloMarketOrder : INotifyPropertyChanged
{
    private long _volume;
    private double _price;
    private long _cumulativeVolume;
    private double _depthBarWidth;

    public string OrderId { get; set; }
    
    public double Price
    {
        get => _price;
        set
        {
            _price = value;
            OnPropertyChanged(nameof(Price));
            OnPropertyChanged(nameof(FormattedPrice));
        }
    }
    
    public long Volume
    {
        get => _volume;
        set
        {
            _volume = value;
            OnPropertyChanged(nameof(Volume));
            OnPropertyChanged(nameof(FormattedVolume));
        }
    }
    
    public long CumulativeVolume
    {
        get => _cumulativeVolume;
        set
        {
            _cumulativeVolume = value;
            OnPropertyChanged(nameof(CumulativeVolume));
            OnPropertyChanged(nameof(FormattedCumulative));
        }
    }
    
    public double DepthBarWidth
    {
        get => _depthBarWidth;
        set
        {
            _depthBarWidth = value;
            OnPropertyChanged(nameof(DepthBarWidth));
        }
    }
    
    public bool IsBuyOrder { get; set; }
    public DateTime Timestamp { get; set; }
    public string Location { get; set; }
    public long MinVolume { get; set; }
    public int Duration { get; set; }
    public int Range { get; set; }
    
    public string FormattedPrice => Price.ToString("N2");
    public string FormattedVolume => Volume.ToString("N0");
    public string FormattedCumulative => CumulativeVolume.ToString("N0");
    
    public event PropertyChangedEventHandler PropertyChanged;
    
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public enum OrderBookDisplayMode
{
    Full,
    Compact,
    DepthOnly
}

public class OrderBookAnalysis
{
    public double BestBidPrice { get; set; }
    public double BestAskPrice { get; set; }
    public double Spread { get; set; }
    public double SpreadPercent { get; set; }
    public long TotalBuyVolume { get; set; }
    public long TotalSellVolume { get; set; }
    public int BuyOrderCount { get; set; }
    public int SellOrderCount { get; set; }
    public double MaxCumulativeVolume { get; set; }
    public OrderBookDisplayMode DisplayMode { get; set; }
    public bool IsAnimating { get; set; }
    public string PerformanceMode { get; set; }
}

#endregion