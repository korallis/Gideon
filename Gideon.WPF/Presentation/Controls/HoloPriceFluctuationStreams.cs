// ==========================================================================
// HoloPriceFluctuationStreams.cs - Holographic Price Fluctuation Streams
// ==========================================================================
// Advanced market visualization featuring real-time price particle streams,
// market trend analysis, EVE-style market data, and holographic price flow effects.
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
/// Holographic price fluctuation streams with real-time market analysis and particle flow visualization
/// </summary>
public class HoloPriceFluctuationStreams : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloPriceFluctuationStreams),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloPriceFluctuationStreams),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty MarketDataProperty =
        DependencyProperty.Register(nameof(MarketData), typeof(ObservableCollection<HoloMarketItem>), typeof(HoloPriceFluctuationStreams),
            new PropertyMetadata(null, OnMarketDataChanged));

    public static readonly DependencyProperty PriceHistoryProperty =
        DependencyProperty.Register(nameof(PriceHistory), typeof(ObservableCollection<HoloPricePoint>), typeof(HoloPriceFluctuationStreams),
            new PropertyMetadata(null, OnPriceHistoryChanged));

    public static readonly DependencyProperty VisualizationModeProperty =
        DependencyProperty.Register(nameof(VisualizationMode), typeof(PriceVisualizationMode), typeof(HoloPriceFluctuationStreams),
            new PropertyMetadata(PriceVisualizationMode.RealTime, OnVisualizationModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloPriceFluctuationStreams),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloPriceFluctuationStreams),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowTrendLinesProperty =
        DependencyProperty.Register(nameof(ShowTrendLines), typeof(bool), typeof(HoloPriceFluctuationStreams),
            new PropertyMetadata(true, OnShowTrendLinesChanged));

    public static readonly DependencyProperty ShowVolumeFlowProperty =
        DependencyProperty.Register(nameof(ShowVolumeFlow), typeof(bool), typeof(HoloPriceFluctuationStreams),
            new PropertyMetadata(true, OnShowVolumeFlowChanged));

    public static readonly DependencyProperty StreamDensityProperty =
        DependencyProperty.Register(nameof(StreamDensity), typeof(double), typeof(HoloPriceFluctuationStreams),
            new PropertyMetadata(1.0, OnStreamDensityChanged));

    public static readonly DependencyProperty UpdateIntervalProperty =
        DependencyProperty.Register(nameof(UpdateInterval), typeof(TimeSpan), typeof(HoloPriceFluctuationStreams),
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

    public ObservableCollection<HoloMarketItem> MarketData
    {
        get => (ObservableCollection<HoloMarketItem>)GetValue(MarketDataProperty);
        set => SetValue(MarketDataProperty, value);
    }

    public ObservableCollection<HoloPricePoint> PriceHistory
    {
        get => (ObservableCollection<HoloPricePoint>)GetValue(PriceHistoryProperty);
        set => SetValue(PriceHistoryProperty, value);
    }

    public PriceVisualizationMode VisualizationMode
    {
        get => (PriceVisualizationMode)GetValue(VisualizationModeProperty);
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

    public bool ShowTrendLines
    {
        get => (bool)GetValue(ShowTrendLinesProperty);
        set => SetValue(ShowTrendLinesProperty, value);
    }

    public bool ShowVolumeFlow
    {
        get => (bool)GetValue(ShowVolumeFlowProperty);
        set => SetValue(ShowVolumeFlowProperty, value);
    }

    public double StreamDensity
    {
        get => (double)GetValue(StreamDensityProperty);
        set => SetValue(StreamDensityProperty, value);
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
    private readonly Dictionary<string, Storyboard> _streamAnimations;
    private readonly List<HoloPriceParticle> _priceParticles;
    private readonly List<HoloVolumeStream> _volumeStreams;
    private readonly List<HoloTrendLine> _trendLines;
    private readonly HoloMarketCalculator _calculator;
    
    private Canvas _mainCanvas;
    private Canvas _particleCanvas;
    private Canvas _streamCanvas;
    private Canvas _trendCanvas;
    private Grid _marketItemsGrid;
    private Grid _metricsGrid;

    private readonly Dictionary<string, double> _priceChangeRates;
    private readonly Dictionary<string, MarketTrend> _marketTrends;
    private readonly Dictionary<string, Point> _itemPositions;
    private double _marketVolatility;
    private bool _isMarketActive;
    private DateTime _lastUpdateTime;
    private readonly Random _random = new();

    // Animation state
    private double _animationProgress;
    private readonly Dictionary<string, double> _streamIntensities;
    private readonly List<HoloPriceWave> _priceWaves;

    #endregion

    #region Constructor & Initialization

    public HoloPriceFluctuationStreams()
    {
        _animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) }; // 60 FPS
        _updateTimer = new DispatcherTimer();
        _streamAnimations = new Dictionary<string, Storyboard>();
        _priceParticles = new List<HoloPriceParticle>();
        _volumeStreams = new List<HoloVolumeStream>();
        _trendLines = new List<HoloTrendLine>();
        _calculator = new HoloMarketCalculator();
        _priceChangeRates = new Dictionary<string, double>();
        _marketTrends = new Dictionary<string, MarketTrend>();
        _itemPositions = new Dictionary<string, Point>();
        _streamIntensities = new Dictionary<string, double>();
        _priceWaves = new List<HoloPriceWave>();

        _animationTimer.Tick += OnAnimationTimerTick;
        _updateTimer.Tick += OnUpdateTimerTick;
        _lastUpdateTime = DateTime.Now;

        InitializeComponent();
        CreateHolographicInterface();
        StartAnimations();
    }

    private void InitializeComponent()
    {
        Width = 800;
        Height = 900;
        Background = new SolidColorBrush(Color.FromArgb(25, 100, 200, 100));

        // Apply holographic effects
        Effect = new DropShadowEffect
        {
            Color = Colors.Gold,
            BlurRadius = 28,
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
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(250) });

        // Create main canvas for market visualization
        _mainCanvas = new Canvas
        {
            Background = new SolidColorBrush(Color.FromArgb(15, 255, 200, 100)),
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

        // Create stream canvas for particle streams
        _streamCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false
        };
        _mainCanvas.Children.Add(_streamCanvas);

        // Create trend canvas for trend lines
        _trendCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false
        };
        _mainCanvas.Children.Add(_trendCanvas);

        // Create market items display
        CreateMarketItemsDisplay();

        // Create price chart area
        CreatePriceChartArea();

        // Create volume flow indicators
        if (ShowVolumeFlow)
        {
            CreateVolumeFlowIndicators();
        }

        // Create trend analysis panel
        CreateTrendAnalysisPanel();

        // Create metrics display
        CreateMetricsDisplay();
    }

    private void CreateMarketItemsDisplay()
    {
        var itemsContainer = new ScrollViewer
        {
            Width = 500,
            Height = 550,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)),
            BorderBrush = new SolidColorBrush(Colors.Gold),
            BorderThickness = new Thickness(1)
        };
        Canvas.SetLeft(itemsContainer, 20);
        Canvas.SetTop(itemsContainer, 20);
        _mainCanvas.Children.Add(itemsContainer);

        _marketItemsGrid = new Grid();
        itemsContainer.Content = _marketItemsGrid;

        // Add title
        var title = new TextBlock
        {
            Text = "Market Price Streams",
            Foreground = new SolidColorBrush(Colors.Gold),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 10)
        };
        _marketItemsGrid.Children.Add(title);
    }

    private void CreatePriceChartArea()
    {
        var chartContainer = new Border
        {
            Width = 250,
            Height = 200,
            Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5)
        };
        Canvas.SetRight(chartContainer, 20);
        Canvas.SetTop(chartContainer, 20);
        _mainCanvas.Children.Add(chartContainer);

        var chartCanvas = new Canvas
        {
            Name = "PriceChartCanvas",
            Background = Brushes.Transparent
        };
        chartContainer.Child = chartCanvas;

        // Add title
        var title = new TextBlock
        {
            Text = "Price Chart",
            Foreground = new SolidColorBrush(Colors.Gold),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Canvas.SetTop(title, 5);
        Canvas.SetLeft(title, 90);
        chartCanvas.Children.Add(title);

        // Create chart axes
        CreateChartAxes(chartCanvas);
    }

    private void CreateChartAxes(Canvas canvas)
    {
        // X-axis (time)
        var xAxis = new Line
        {
            X1 = 30, Y1 = 170, X2 = 220, Y2 = 170,
            Stroke = new SolidColorBrush(Colors.Gray),
            StrokeThickness = 1
        };
        canvas.Children.Add(xAxis);

        // Y-axis (price)
        var yAxis = new Line
        {
            X1 = 30, Y1 = 30, X2 = 30, Y2 = 170,
            Stroke = new SolidColorBrush(Colors.Gray),
            StrokeThickness = 1
        };
        canvas.Children.Add(yAxis);

        // Add grid lines
        for (int i = 1; i <= 4; i++)
        {
            var gridX = 30 + (i * 47.5);
            var gridLineX = new Line
            {
                X1 = gridX, Y1 = 30, X2 = gridX, Y2 = 170,
                Stroke = new SolidColorBrush(Color.FromArgb(50, 128, 128, 128)),
                StrokeThickness = 1
            };
            canvas.Children.Add(gridLineX);

            var gridY = 30 + (i * 35);
            var gridLineY = new Line
            {
                X1 = 30, Y1 = gridY, X2 = 220, Y2 = gridY,
                Stroke = new SolidColorBrush(Color.FromArgb(50, 128, 128, 128)),
                StrokeThickness = 1
            };
            canvas.Children.Add(gridLineY);
        }

        // Add axis labels
        var xLabel = new TextBlock
        {
            Text = "Time",
            Foreground = new SolidColorBrush(Colors.Gray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 9
        };
        Canvas.SetLeft(xLabel, 115);
        Canvas.SetTop(xLabel, 180);
        canvas.Children.Add(xLabel);

        var yLabel = new TextBlock
        {
            Text = "Price",
            Foreground = new SolidColorBrush(Colors.Gray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 9,
            RenderTransform = new RotateTransform(-90)
        };
        Canvas.SetLeft(yLabel, 5);
        Canvas.SetTop(yLabel, 100);
        canvas.Children.Add(yLabel);
    }

    private void CreateVolumeFlowIndicators()
    {
        var volumeContainer = new Border
        {
            Width = 250,
            Height = 150,
            Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5)
        };
        Canvas.SetRight(volumeContainer, 20);
        Canvas.SetTop(volumeContainer, 240);
        _mainCanvas.Children.Add(volumeContainer);

        var volumeCanvas = new Canvas
        {
            Name = "VolumeFlowCanvas",
            Background = Brushes.Transparent
        };
        volumeContainer.Child = volumeCanvas;

        // Add title
        var title = new TextBlock
        {
            Text = "Volume Flow",
            Foreground = new SolidColorBrush(Colors.Gold),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Canvas.SetTop(title, 5);
        Canvas.SetLeft(title, 90);
        volumeCanvas.Children.Add(title);

        // Create volume flow visualization
        CreateVolumeFlowVisualization(volumeCanvas);
    }

    private void CreateVolumeFlowVisualization(Canvas canvas)
    {
        // Buy orders flow (left to right, green)
        var buyFlow = new Rectangle
        {
            Name = "BuyFlowIndicator",
            Width = 100,
            Height = 8,
            Fill = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(0, 0, 255, 0), 0.0),
                    new GradientStop(Colors.LimeGreen, 1.0)
                }
            }
        };
        Canvas.SetLeft(buyFlow, 20);
        Canvas.SetTop(buyFlow, 50);
        canvas.Children.Add(buyFlow);

        var buyLabel = new TextBlock
        {
            Text = "Buy Orders",
            Foreground = new SolidColorBrush(Colors.LimeGreen),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 9
        };
        Canvas.SetLeft(buyLabel, 25);
        Canvas.SetTop(buyLabel, 65);
        canvas.Children.Add(buyLabel);

        // Sell orders flow (right to left, red)
        var sellFlow = new Rectangle
        {
            Name = "SellFlowIndicator",
            Width = 100,
            Height = 8,
            Fill = new LinearGradientBrush
            {
                StartPoint = new Point(1, 0),
                EndPoint = new Point(0, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(0, 255, 0, 0), 0.0),
                    new GradientStop(Colors.Red, 1.0)
                }
            }
        };
        Canvas.SetLeft(sellFlow, 130);
        Canvas.SetTop(sellFlow, 50);
        canvas.Children.Add(sellFlow);

        var sellLabel = new TextBlock
        {
            Text = "Sell Orders",
            Foreground = new SolidColorBrush(Colors.Red),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 9
        };
        Canvas.SetLeft(sellLabel, 135);
        Canvas.SetTop(sellLabel, 65);
        canvas.Children.Add(sellLabel);

        // Volume meter
        var volumeMeter = new Rectangle
        {
            Name = "VolumeMeter",
            Width = 200,
            Height = 20,
            Fill = new SolidColorBrush(Color.FromArgb(100, 255, 215, 0)),
            Stroke = new SolidColorBrush(Colors.Gold),
            StrokeThickness = 1
        };
        Canvas.SetLeft(volumeMeter, 25);
        Canvas.SetTop(volumeMeter, 100);
        canvas.Children.Add(volumeMeter);

        var volumeLabel = new TextBlock
        {
            Text = "Total Volume",
            Foreground = new SolidColorBrush(Colors.Gold),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 9
        };
        Canvas.SetLeft(volumeLabel, 25);
        Canvas.SetTop(volumeLabel, 125);
        canvas.Children.Add(volumeLabel);
    }

    private void CreateTrendAnalysisPanel()
    {
        var trendContainer = new Border
        {
            Width = 250,
            Height = 180,
            Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5)
        };
        Canvas.SetRight(trendContainer, 20);
        Canvas.SetTop(trendContainer, 410);
        _mainCanvas.Children.Add(trendContainer);

        var trendGrid = new Grid
        {
            Name = "TrendAnalysisGrid"
        };
        trendContainer.Child = trendGrid;

        trendGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
        trendGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        // Add title
        var title = new TextBlock
        {
            Text = "Trend Analysis",
            Foreground = new SolidColorBrush(Colors.Gold),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(title, 0);
        trendGrid.Children.Add(title);

        // Trend indicators
        var trendPanel = new StackPanel
        {
            Name = "TrendIndicatorsPanel",
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10)
        };
        Grid.SetRow(trendPanel, 1);
        trendGrid.Children.Add(trendPanel);
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
        _metricsGrid.ColumnDefinitions.Add(new ColumnDefinition());

        // Create metric displays
        CreateMetricDisplay("Items", "24", Colors.Gold, 0, 0);
        CreateMetricDisplay("Volume", "1.2M", Colors.Cyan, 1, 0);
        CreateMetricDisplay("Avg Price", "45.8k", Colors.Yellow, 2, 0);
        CreateMetricDisplay("Volatility", "High", Colors.Red, 3, 0);
        CreateMetricDisplay("Trend", "↗", Colors.LimeGreen, 4, 0);

        CreateMetricDisplay("Buy Orders", "847", Colors.LimeGreen, 0, 1);
        CreateMetricDisplay("Sell Orders", "523", Colors.Red, 1, 1);
        CreateMetricDisplay("Spread", "3.2%", Colors.Orange, 2, 1);
        CreateMetricDisplay("Velocity", "Fast", Colors.Cyan, 3, 1);
        CreateMetricDisplay("Liquidity", "High", Colors.Pink, 4, 1);

        CreateMetricDisplay("Daily +", "67", Colors.LimeGreen, 0, 2);
        CreateMetricDisplay("Daily -", "23", Colors.Red, 1, 2);
        CreateMetricDisplay("Weekly Δ", "+12.4%", Colors.Yellow, 2, 2);
        CreateMetricDisplay("Activity", "Active", Colors.Gold, 3, 2);
        CreateMetricDisplay("Status", "Online", Colors.LimeGreen, 4, 2);
    }

    private void CreateMetricDisplay(string label, string value, Color color, int column, int row)
    {
        var container = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(4)
        };

        var labelText = new TextBlock
        {
            Text = label,
            Foreground = new SolidColorBrush(Colors.Gray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 8,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var valueText = new TextBlock
        {
            Text = value,
            Foreground = new SolidColorBrush(color),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 11,
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
        UpdateVolumeStreams();
        UpdatePriceWaves();
        UpdateTrendLines();
        UpdateStreamFlows();
    }

    private void OnUpdateTimerTick(object sender, EventArgs e)
    {
        if (MarketData != null)
        {
            CalculateMarketMetrics();
            UpdateMarketItemsDisplay();
            UpdatePriceChart();
            UpdateVolumeFlowIndicators();
            UpdateTrendAnalysis();
            UpdateMetricsDisplay();
        }
    }

    private void UpdateParticleEffects()
    {
        if (!EnableParticleEffects)
            return;

        // Update existing price particles
        for (int i = _priceParticles.Count - 1; i >= 0; i--)
        {
            var particle = _priceParticles[i];
            particle.Update();

            if (particle.IsExpired)
            {
                _particleCanvas.Children.Remove(particle.Visual);
                _priceParticles.RemoveAt(i);
            }
        }

        // Create new price particles based on market activity
        if (_isMarketActive && _random.NextDouble() < (0.3 * StreamDensity))
        {
            CreatePriceParticle();
        }
    }

    private void UpdateVolumeStreams()
    {
        if (!ShowVolumeFlow)
            return;

        // Update existing volume streams
        for (int i = _volumeStreams.Count - 1; i >= 0; i--)
        {
            var stream = _volumeStreams[i];
            stream.Update();

            if (stream.IsExpired)
            {
                _streamCanvas.Children.Remove(stream.Visual);
                _volumeStreams.RemoveAt(i);
            }
        }

        // Create new volume streams based on trading activity
        if (_isMarketActive && _random.NextDouble() < (0.2 * StreamDensity))
        {
            CreateVolumeStream();
        }
    }

    private void UpdatePriceWaves()
    {
        _animationProgress += 0.02;
        if (_animationProgress > 1.0)
            _animationProgress = 0.0;

        // Update existing price waves
        for (int i = _priceWaves.Count - 1; i >= 0; i--)
        {
            var wave = _priceWaves[i];
            wave.Update();

            if (wave.IsExpired)
            {
                _streamCanvas.Children.Remove(wave.Visual);
                _priceWaves.RemoveAt(i);
            }
        }

        // Create new price waves based on significant price changes
        if (_marketVolatility > 0.5 && _random.NextDouble() < 0.1)
        {
            CreatePriceWave();
        }
    }

    private void UpdateTrendLines()
    {
        if (!ShowTrendLines)
            return;

        // Update trend line animations
        foreach (var trendLine in _trendLines)
        {
            trendLine.Update();
        }
    }

    private void UpdateStreamFlows()
    {
        // Update stream intensities based on market activity
        foreach (var kvp in _streamIntensities.ToList())
        {
            var itemId = kvp.Key;
            var baseIntensity = kvp.Value;
            var oscillation = Math.Sin(_animationProgress * 4 * Math.PI) * 0.3;
            _streamIntensities[itemId] = Math.Max(0.1, baseIntensity + oscillation);
        }
    }

    private void CreatePriceParticle()
    {
        var priceChange = (_random.NextDouble() - 0.5) * 2; // -1 to +1
        var startX = _random.NextDouble() * _mainCanvas.ActualWidth;
        var startY = _random.NextDouble() * (_mainCanvas.ActualHeight * 0.8) + 50;

        var particle = new HoloPriceParticle
        {
            Position = new Point(startX, startY),
            Velocity = new Vector(_random.NextDouble() * 4 - 2, _random.NextDouble() * 2 - 1),
            Color = GetPriceChangeColor(priceChange),
            Size = Math.Abs(priceChange) * 4 + 2,
            Life = TimeSpan.FromSeconds(_random.NextDouble() * 3 + 2),
            PriceChange = priceChange,
            ItemType = GetRandomItemType()
        };

        particle.CreateVisual();
        _particleCanvas.Children.Add(particle.Visual);
        _priceParticles.Add(particle);
    }

    private void CreateVolumeStream()
    {
        var isBuyOrder = _random.NextDouble() > 0.5;
        var startX = isBuyOrder ? 0 : _mainCanvas.ActualWidth;
        var endX = isBuyOrder ? _mainCanvas.ActualWidth : 0;
        var y = _random.NextDouble() * (_mainCanvas.ActualHeight * 0.6) + 100;

        var stream = new HoloVolumeStream
        {
            StartPosition = new Point(startX, y),
            EndPosition = new Point(endX, y),
            Color = isBuyOrder ? Colors.LimeGreen : Colors.Red,
            Thickness = _random.NextDouble() * 3 + 1,
            Speed = _random.NextDouble() * 100 + 50,
            Life = TimeSpan.FromSeconds(_random.NextDouble() * 4 + 2),
            IsBuyOrder = isBuyOrder,
            Volume = _random.Next(100, 10000)
        };

        stream.CreateVisual();
        _streamCanvas.Children.Add(stream.Visual);
        _volumeStreams.Add(stream);
    }

    private void CreatePriceWave()
    {
        var centerX = _random.NextDouble() * _mainCanvas.ActualWidth;
        var centerY = _random.NextDouble() * _mainCanvas.ActualHeight;

        var wave = new HoloPriceWave
        {
            Center = new Point(centerX, centerY),
            Color = GetMarketTrendColor(_marketVolatility),
            MaxRadius = 50 + (_marketVolatility * 100),
            Life = TimeSpan.FromSeconds(3),
            Intensity = _marketVolatility
        };

        wave.CreateVisual();
        _streamCanvas.Children.Add(wave.Visual);
        _priceWaves.Add(wave);
    }

    #endregion

    #region Calculation Methods

    private void CalculateMarketMetrics()
    {
        if (MarketData == null)
            return;

        var now = DateTime.Now;
        var deltaTime = (now - _lastUpdateTime).TotalSeconds;
        _lastUpdateTime = now;

        _priceChangeRates.Clear();
        _marketTrends.Clear();

        var totalVolatility = 0.0;
        var activeItems = 0;

        foreach (var item in MarketData)
        {
            var priceChangeRate = _calculator.CalculatePriceChangeRate(item);
            _priceChangeRates[item.Id] = priceChangeRate;

            var trend = _calculator.DetermineMarketTrend(item, PriceHistory);
            _marketTrends[item.Id] = trend;

            totalVolatility += Math.Abs(priceChangeRate);
            if (item.IsActive)
                activeItems++;

            // Update stream intensity based on activity
            var intensity = Math.Min(1.0, item.Volume / 100000.0);
            _streamIntensities[item.Id] = intensity;
        }

        _marketVolatility = MarketData.Count > 0 ? totalVolatility / MarketData.Count : 0;
        _isMarketActive = activeItems > 0;
    }

    private void UpdateMarketItemsDisplay()
    {
        if (MarketData == null)
            return;

        // Clear existing content (keep title)
        _marketItemsGrid.Children.Clear();
        _marketItemsGrid.RowDefinitions.Clear();

        // Add title
        _marketItemsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
        var title = new TextBlock
        {
            Text = "Market Price Streams",
            Foreground = new SolidColorBrush(Colors.Gold),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(title, 0);
        _marketItemsGrid.Children.Add(title);

        // Add market items sorted by activity
        var sortedItems = MarketData.OrderByDescending(i => _streamIntensities.GetValueOrDefault(i.Id, 0))
                                   .ThenByDescending(i => Math.Abs(_priceChangeRates.GetValueOrDefault(i.Id, 0)));

        int rowIndex = 1;
        foreach (var item in sortedItems.Take(12)) // Show top 12 most active items
        {
            _marketItemsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) });
            
            var itemDisplay = CreateMarketItemDisplay(item);
            Grid.SetRow(itemDisplay, rowIndex);
            _marketItemsGrid.Children.Add(itemDisplay);
            
            rowIndex++;
        }
    }

    private FrameworkElement CreateMarketItemDisplay(HoloMarketItem item)
    {
        var priceChangeRate = _priceChangeRates.GetValueOrDefault(item.Id, 0.0);
        var trend = _marketTrends.GetValueOrDefault(item.Id, MarketTrend.Stable);
        var streamIntensity = _streamIntensities.GetValueOrDefault(item.Id, 0.0);

        var container = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(60, 100, 100, 100)),
            BorderBrush = new SolidColorBrush(GetTrendColor(trend)),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(10, 3),
            Padding = new Thickness(10)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Item info
        var infoPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var nameText = new TextBlock
        {
            Text = item.Name,
            Foreground = new SolidColorBrush(Colors.White),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 12,
            FontWeight = FontWeights.Bold
        };
        infoPanel.Children.Add(nameText);

        var categoryText = new TextBlock
        {
            Text = $"{item.Category} | {item.Region}",
            Foreground = new SolidColorBrush(Colors.Gray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 9
        };
        infoPanel.Children.Add(categoryText);

        var priceText = new TextBlock
        {
            Text = $"{item.CurrentPrice:N2} ISK",
            Foreground = new SolidColorBrush(Colors.LightGray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 10
        };
        infoPanel.Children.Add(priceText);

        Grid.SetColumn(infoPanel, 0);
        grid.Children.Add(infoPanel);

        // Price change display
        var changePanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var changeText = new TextBlock
        {
            Text = $"{priceChangeRate:+0.00;-0.00}%",
            Foreground = new SolidColorBrush(GetPriceChangeColor(priceChangeRate)),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        changePanel.Children.Add(changeText);

        var changeLabel = new TextBlock
        {
            Text = "Change",
            Foreground = new SolidColorBrush(Colors.Gray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 8,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        changePanel.Children.Add(changeLabel);

        Grid.SetColumn(changePanel, 1);
        grid.Children.Add(changePanel);

        // Volume display
        var volumePanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var volumeText = new TextBlock
        {
            Text = FormatVolume(item.Volume),
            Foreground = new SolidColorBrush(Colors.Cyan),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        volumePanel.Children.Add(volumeText);

        var volumeLabel = new TextBlock
        {
            Text = "Volume",
            Foreground = new SolidColorBrush(Colors.Gray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 8,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        volumePanel.Children.Add(volumeLabel);

        Grid.SetColumn(volumePanel, 2);
        grid.Children.Add(volumePanel);

        // Stream intensity indicator
        var streamPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var streamBar = new Border
        {
            Width = 50,
            Height = 8,
            Background = new SolidColorBrush(Color.FromArgb(50, 128, 128, 128)),
            BorderBrush = new SolidColorBrush(Colors.Gold),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4)
        };

        var streamFill = new Rectangle
        {
            Fill = new SolidColorBrush(Colors.Gold),
            Width = 48 * streamIntensity,
            Height = 6,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        streamBar.Child = streamFill;

        streamPanel.Children.Add(streamBar);

        var streamLabel = new TextBlock
        {
            Text = "Activity",
            Foreground = new SolidColorBrush(Colors.Gray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 8,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 2, 0, 0)
        };
        streamPanel.Children.Add(streamLabel);

        Grid.SetColumn(streamPanel, 3);
        grid.Children.Add(streamPanel);

        // Add pulsing effect based on activity
        var pulseIntensity = 0.7 + (streamIntensity * 0.3);
        container.Opacity = pulseIntensity;

        container.Child = grid;
        return container;
    }

    private void UpdatePriceChart()
    {
        var chartCanvas = FindChildByName<Canvas>("PriceChartCanvas");
        if (chartCanvas == null || PriceHistory == null)
            return;

        // Clear existing price line
        var existingLine = chartCanvas.Children.OfType<Path>().FirstOrDefault();
        if (existingLine != null)
            chartCanvas.Children.Remove(existingLine);

        // Create price line from history
        if (PriceHistory.Count > 1)
        {
            var priceLine = CreatePriceLineGeometry();
            var priceLinePath = new Path
            {
                Data = priceLine,
                Stroke = new SolidColorBrush(Colors.Gold),
                StrokeThickness = 2,
                Effect = new DropShadowEffect
                {
                    Color = Colors.Gold,
                    BlurRadius = 4,
                    ShadowDepth = 0,
                    Opacity = 0.6
                }
            };
            chartCanvas.Children.Add(priceLinePath);
        }
    }

    private PathGeometry CreatePriceLineGeometry()
    {
        var geometry = new PathGeometry();
        var figure = new PathFigure();

        if (PriceHistory?.Count > 1)
        {
            var recentHistory = PriceHistory.TakeLast(10).ToList();
            var minPrice = recentHistory.Min(p => p.Price);
            var maxPrice = recentHistory.Max(p => p.Price);
            var priceRange = maxPrice - minPrice;

            if (priceRange == 0) priceRange = 1; // Avoid division by zero

            var startPoint = new Point(30, 170 - ((recentHistory[0].Price - minPrice) / priceRange) * 140);
            figure.StartPoint = startPoint;

            for (int i = 1; i < recentHistory.Count; i++)
            {
                var x = 30 + (i / (double)(recentHistory.Count - 1)) * 190;
                var y = 170 - ((recentHistory[i].Price - minPrice) / priceRange) * 140;
                figure.Segments.Add(new LineSegment(new Point(x, y), true));
            }
        }

        geometry.Figures.Add(figure);
        return geometry;
    }

    private void UpdateVolumeFlowIndicators()
    {
        if (!ShowVolumeFlow)
            return;

        var volumeCanvas = FindChildByName<Canvas>("VolumeFlowCanvas");
        if (volumeCanvas == null || MarketData == null)
            return;

        var totalBuyVolume = MarketData.Where(i => i.HasBuyOrders).Sum(i => i.Volume);
        var totalSellVolume = MarketData.Where(i => i.HasSellOrders).Sum(i => i.Volume);
        var totalVolume = totalBuyVolume + totalSellVolume;

        // Update buy flow indicator
        var buyFlow = volumeCanvas.Children.OfType<Rectangle>()
            .FirstOrDefault(r => r.Name == "BuyFlowIndicator");
        if (buyFlow != null)
        {
            var buyIntensity = totalVolume > 0 ? totalBuyVolume / totalVolume : 0;
            buyFlow.Opacity = 0.3 + (buyIntensity * 0.7);
        }

        // Update sell flow indicator
        var sellFlow = volumeCanvas.Children.OfType<Rectangle>()
            .FirstOrDefault(r => r.Name == "SellFlowIndicator");
        if (sellFlow != null)
        {
            var sellIntensity = totalVolume > 0 ? totalSellVolume / totalVolume : 0;
            sellFlow.Opacity = 0.3 + (sellIntensity * 0.7);
        }

        // Update volume meter
        var volumeMeter = volumeCanvas.Children.OfType<Rectangle>()
            .FirstOrDefault(r => r.Name == "VolumeMeter");
        if (volumeMeter != null)
        {
            var volumeIntensity = Math.Min(1.0, totalVolume / 1000000.0); // Scale to millions
            volumeMeter.Width = 25 + (volumeIntensity * 175);
        }
    }

    private void UpdateTrendAnalysis()
    {
        var trendPanel = FindChildByName<StackPanel>("TrendIndicatorsPanel");
        if (trendPanel == null)
            return;

        // Clear existing trend indicators
        trendPanel.Children.Clear();

        if (MarketData != null)
        {
            // Calculate overall trend distribution
            var trendCounts = new Dictionary<MarketTrend, int>
            {
                { MarketTrend.Bullish, 0 },
                { MarketTrend.Bearish, 0 },
                { MarketTrend.Stable, 0 },
                { MarketTrend.Volatile, 0 }
            };

            foreach (var trend in _marketTrends.Values)
            {
                trendCounts[trend]++;
            }

            foreach (var kvp in trendCounts)
            {
                var trendDisplay = CreateTrendIndicator(kvp.Key, kvp.Value);
                trendPanel.Children.Add(trendDisplay);
            }
        }
    }

    private FrameworkElement CreateTrendIndicator(MarketTrend trend, int count)
    {
        var container = new Grid
        {
            Margin = new Thickness(0, 3)
        };

        container.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        container.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        container.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });

        // Trend name
        var trendText = new TextBlock
        {
            Text = trend.ToString(),
            Foreground = new SolidColorBrush(GetTrendColor(trend)),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 10,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(trendText, 0);
        container.Children.Add(trendText);

        // Trend bar
        var barContainer = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(50, 128, 128, 128)),
            BorderBrush = new SolidColorBrush(GetTrendColor(trend)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(2),
            Height = 10,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 0)
        };
        Grid.SetColumn(barContainer, 1);
        container.Children.Add(barContainer);

        var maxCount = MarketData?.Count ?? 1;
        var trendFill = new Rectangle
        {
            Fill = new SolidColorBrush(GetTrendColor(trend)),
            Width = Math.Max(2, (count / (double)maxCount) * 120),
            Height = 8,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        barContainer.Child = trendFill;

        // Count
        var countText = new TextBlock
        {
            Text = count.ToString(),
            Foreground = new SolidColorBrush(GetTrendColor(trend)),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 9,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetColumn(countText, 2);
        container.Children.Add(countText);

        return container;
    }

    private void UpdateMetricsDisplay()
    {
        if (MarketData == null)
            return;

        var itemCount = MarketData.Count;
        var totalVolume = MarketData.Sum(i => i.Volume);
        var avgPrice = MarketData.Average(i => i.CurrentPrice);
        var buyOrders = MarketData.Count(i => i.HasBuyOrders);
        var sellOrders = MarketData.Count(i => i.HasSellOrders);

        UpdateMetricValue("Items", itemCount.ToString());
        UpdateMetricValue("Volume", FormatVolume(totalVolume));
        UpdateMetricValue("Avg Price", $"{avgPrice / 1000:F1}k");
        UpdateMetricValue("Buy Orders", buyOrders.ToString());
        UpdateMetricValue("Sell Orders", sellOrders.ToString());

        var volatilityText = _marketVolatility > 0.7 ? "High" : _marketVolatility > 0.3 ? "Medium" : "Low";
        UpdateMetricValue("Volatility", volatilityText);

        var overallTrend = DetermineOverallTrend();
        var trendSymbol = overallTrend switch
        {
            MarketTrend.Bullish => "↗",
            MarketTrend.Bearish => "↘",
            MarketTrend.Volatile => "↕",
            _ => "→"
        };
        UpdateMetricValue("Trend", trendSymbol);

        var spread = CalculateAverageSpread();
        UpdateMetricValue("Spread", $"{spread:F1}%");

        var velocityText = _isMarketActive ? "Fast" : "Slow";
        UpdateMetricValue("Velocity", velocityText);

        var liquidityText = totalVolume > 500000 ? "High" : totalVolume > 100000 ? "Medium" : "Low";
        UpdateMetricValue("Liquidity", liquidityText);

        // Calculate daily changes
        var dailyPositive = _priceChangeRates.Count(kvp => kvp.Value > 0);
        var dailyNegative = _priceChangeRates.Count(kvp => kvp.Value < 0);
        var weeklyChange = _random.NextDouble() * 30 - 15; // Placeholder

        UpdateMetricValue("Daily +", dailyPositive.ToString());
        UpdateMetricValue("Daily -", dailyNegative.ToString());
        UpdateMetricValue("Weekly Δ", $"{weeklyChange:+0.0;-0.0}%");

        var activityText = _isMarketActive ? "Active" : "Quiet";
        UpdateMetricValue("Activity", activityText);
        UpdateMetricValue("Status", "Online");

        // Update metric colors
        UpdateMetricColor("Volatility", GetVolatilityColor(_marketVolatility));
        UpdateMetricColor("Trend", GetTrendColor(overallTrend));
        UpdateMetricColor("Activity", _isMarketActive ? Colors.LimeGreen : Colors.Orange);
        UpdateMetricColor("Weekly Δ", weeklyChange > 0 ? Colors.LimeGreen : Colors.Red);
    }

    #endregion

    #region Helper Methods

    private Color GetPriceChangeColor(double change)
    {
        if (change > 0) return Colors.LimeGreen;
        if (change < 0) return Colors.Red;
        return Colors.Gray;
    }

    private Color GetTrendColor(MarketTrend trend)
    {
        return trend switch
        {
            MarketTrend.Bullish => Colors.LimeGreen,
            MarketTrend.Bearish => Colors.Red,
            MarketTrend.Volatile => Colors.Orange,
            MarketTrend.Stable => Colors.Gray,
            _ => Colors.White
        };
    }

    private Color GetMarketTrendColor(double volatility)
    {
        if (volatility > 0.7) return Colors.Red;
        if (volatility > 0.3) return Colors.Orange;
        return Colors.LimeGreen;
    }

    private Color GetVolatilityColor(double volatility)
    {
        if (volatility > 0.7) return Colors.Red;
        if (volatility > 0.3) return Colors.Orange;
        return Colors.LimeGreen;
    }

    private string GetRandomItemType()
    {
        var types = new[] { "Minerals", "Ships", "Modules", "Blueprints", "Ammunition" };
        return types[_random.Next(types.Length)];
    }

    private string FormatVolume(double volume)
    {
        if (volume >= 1000000) return $"{volume / 1000000:F1}M";
        if (volume >= 1000) return $"{volume / 1000:F1}k";
        return volume.ToString("F0");
    }

    private MarketTrend DetermineOverallTrend()
    {
        if (!_marketTrends.Any()) return MarketTrend.Stable;

        var trendCounts = _marketTrends.Values.GroupBy(t => t).ToDictionary(g => g.Key, g => g.Count());
        return trendCounts.OrderByDescending(kvp => kvp.Value).First().Key;
    }

    private double CalculateAverageSpread()
    {
        if (MarketData?.Any() != true) return 0;

        return MarketData.Where(i => i.BuyPrice > 0 && i.SellPrice > 0)
                        .Average(i => ((i.SellPrice - i.BuyPrice) / i.BuyPrice) * 100);
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
    /// Gets the current market analysis
    /// </summary>
    public MarketAnalysis GetMarketAnalysis()
    {
        return new MarketAnalysis
        {
            TotalItems = MarketData?.Count ?? 0,
            TotalVolume = MarketData?.Sum(i => i.Volume) ?? 0,
            AveragePrice = MarketData?.Average(i => i.CurrentPrice) ?? 0,
            MarketVolatility = _marketVolatility,
            IsMarketActive = _isMarketActive,
            PriceChangeRates = new Dictionary<string, double>(_priceChangeRates),
            MarketTrends = new Dictionary<string, MarketTrend>(_marketTrends),
            OverallTrend = DetermineOverallTrend(),
            AverageSpread = CalculateAverageSpread()
        };
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public void SetAnimationIntensity(double intensity)
    {
        HolographicIntensity = intensity;
        StreamDensity = intensity;
        
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
            ShowVolumeFlow = false;
            ShowTrendLines = false;
            StreamDensity = 0.3;
            _animationTimer.Interval = TimeSpan.FromMilliseconds(33); // 30 FPS
        }
        else
        {
            EnableParticleEffects = true;
            ShowVolumeFlow = true;
            ShowTrendLines = true;
            StreamDensity = 1.0;
            _animationTimer.Interval = TimeSpan.FromMilliseconds(16); // 60 FPS
        }
    }

    #endregion

    #region Event Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPriceFluctuationStreams control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPriceFluctuationStreams control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnMarketDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPriceFluctuationStreams control)
        {
            control.UpdateMarketData();
        }
    }

    private static void OnPriceHistoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPriceFluctuationStreams control)
        {
            control.UpdatePriceHistory();
        }
    }

    private static void OnVisualizationModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPriceFluctuationStreams control)
        {
            control.UpdateVisualizationMode();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPriceFluctuationStreams control)
        {
            if ((bool)e.NewValue)
                control.StartAnimations();
            else
                control.StopAnimations();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPriceFluctuationStreams control)
        {
            control.UpdateParticleSettings();
        }
    }

    private static void OnShowTrendLinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPriceFluctuationStreams control)
        {
            control.UpdateTrendLineSettings();
        }
    }

    private static void OnShowVolumeFlowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPriceFluctuationStreams control)
        {
            control.UpdateVolumeFlowSettings();
        }
    }

    private static void OnStreamDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPriceFluctuationStreams control)
        {
            control.UpdateStreamDensity();
        }
    }

    private static void OnUpdateIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPriceFluctuationStreams control)
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
            shadowEffect.BlurRadius = 24 + (HolographicIntensity * 8);
        }
    }

    private void UpdateColorScheme()
    {
        // Update colors based on EVE color scheme
    }

    private void UpdateMarketData()
    {
        CalculateMarketMetrics();
        UpdateMarketItemsDisplay();
    }

    private void UpdatePriceHistory()
    {
        UpdatePriceChart();
    }

    private void UpdateVisualizationMode()
    {
        // Switch between different visualization modes
    }

    private void UpdateParticleSettings()
    {
        if (!EnableParticleEffects)
        {
            foreach (var particle in _priceParticles)
            {
                _particleCanvas.Children.Remove(particle.Visual);
            }
            _priceParticles.Clear();
        }
    }

    private void UpdateTrendLineSettings()
    {
        if (!ShowTrendLines)
        {
            foreach (var trendLine in _trendLines)
            {
                _trendCanvas.Children.Remove(trendLine.Visual);
            }
            _trendLines.Clear();
        }
    }

    private void UpdateVolumeFlowSettings()
    {
        if (!ShowVolumeFlow)
        {
            foreach (var stream in _volumeStreams)
            {
                _streamCanvas.Children.Remove(stream.Visual);
            }
            _volumeStreams.Clear();
        }
    }

    private void UpdateStreamDensity()
    {
        // Adjust particle generation rate based on stream density
    }

    #endregion
}

#region Supporting Classes

/// <summary>
/// Represents a market item
/// </summary>
public class HoloMarketItem
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public string Region { get; set; }
    public double CurrentPrice { get; set; }
    public double BuyPrice { get; set; }
    public double SellPrice { get; set; }
    public long Volume { get; set; }
    public bool IsActive { get; set; }
    public bool HasBuyOrders { get; set; }
    public bool HasSellOrders { get; set; }
    public DateTime LastUpdate { get; set; }
}

/// <summary>
/// Represents a price point in history
/// </summary>
public class HoloPricePoint
{
    public DateTime Timestamp { get; set; }
    public double Price { get; set; }
    public long Volume { get; set; }
    public string ItemId { get; set; }
}

/// <summary>
/// Market trends
/// </summary>
public enum MarketTrend
{
    Bullish,
    Bearish,
    Stable,
    Volatile
}

/// <summary>
/// Price visualization modes
/// </summary>
public enum PriceVisualizationMode
{
    RealTime,
    Historical,
    Comparative,
    Predictive
}

/// <summary>
/// Market analysis result
/// </summary>
public class MarketAnalysis
{
    public int TotalItems { get; set; }
    public long TotalVolume { get; set; }
    public double AveragePrice { get; set; }
    public double MarketVolatility { get; set; }
    public bool IsMarketActive { get; set; }
    public Dictionary<string, double> PriceChangeRates { get; set; }
    public Dictionary<string, MarketTrend> MarketTrends { get; set; }
    public MarketTrend OverallTrend { get; set; }
    public double AverageSpread { get; set; }
}

/// <summary>
/// Price particle for visual effects
/// </summary>
public class HoloPriceParticle
{
    public Point Position { get; set; }
    public Vector Velocity { get; set; }
    public Color Color { get; set; }
    public double Size { get; set; }
    public TimeSpan Life { get; set; }
    public double PriceChange { get; set; }
    public string ItemType { get; set; }
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
/// Volume stream for flow visualization
/// </summary>
public class HoloVolumeStream
{
    public Point StartPosition { get; set; }
    public Point EndPosition { get; set; }
    public Color Color { get; set; }
    public double Thickness { get; set; }
    public double Speed { get; set; }
    public TimeSpan Life { get; set; }
    public bool IsBuyOrder { get; set; }
    public long Volume { get; set; }
    public DateTime CreatedTime { get; set; } = DateTime.Now;
    public FrameworkElement Visual { get; set; }

    public bool IsExpired => DateTime.Now - CreatedTime > Life;

    public void Update()
    {
        if (Visual != null)
        {
            // Animate stream movement
            var elapsed = DateTime.Now - CreatedTime;
            var progress = elapsed.TotalMilliseconds / Life.TotalMilliseconds;
            Visual.Opacity = Math.Max(0, 1.0 - progress);
        }
    }

    public void CreateVisual()
    {
        Visual = new Line
        {
            X1 = StartPosition.X,
            Y1 = StartPosition.Y,
            X2 = EndPosition.X,
            Y2 = EndPosition.Y,
            Stroke = new SolidColorBrush(Color),
            StrokeThickness = Thickness,
            StrokeDashArray = new DoubleCollection { 5, 5 },
            Effect = new DropShadowEffect
            {
                Color = Color,
                BlurRadius = Thickness * 2,
                ShadowDepth = 0,
                Opacity = 0.6
            }
        };
    }
}

/// <summary>
/// Price wave for market events
/// </summary>
public class HoloPriceWave
{
    public Point Center { get; set; }
    public Color Color { get; set; }
    public double MaxRadius { get; set; }
    public TimeSpan Life { get; set; }
    public double Intensity { get; set; }
    public DateTime CreatedTime { get; set; } = DateTime.Now;
    public FrameworkElement Visual { get; set; }

    public bool IsExpired => DateTime.Now - CreatedTime > Life;

    public void Update()
    {
        if (Visual != null)
        {
            var elapsed = DateTime.Now - CreatedTime;
            var progress = elapsed.TotalMilliseconds / Life.TotalMilliseconds;
            
            var currentRadius = progress * MaxRadius;
            var opacity = Math.Max(0, 1.0 - progress);
            
            Visual.RenderTransform = new ScaleTransform(currentRadius / 50, currentRadius / 50);
            Visual.Opacity = opacity * Intensity;
        }
    }

    public void CreateVisual()
    {
        Visual = new Ellipse
        {
            Width = 50,
            Height = 50,
            Stroke = new SolidColorBrush(Color),
            StrokeThickness = 2,
            Fill = new SolidColorBrush(Color.FromArgb(50, Color.R, Color.G, Color.B))
        };
        
        Canvas.SetLeft(Visual, Center.X - 25);
        Canvas.SetTop(Visual, Center.Y - 25);
    }
}

/// <summary>
/// Trend line visualization
/// </summary>
public class HoloTrendLine
{
    public Point StartPoint { get; set; }
    public Point EndPoint { get; set; }
    public Color Color { get; set; }
    public MarketTrend Trend { get; set; }
    public FrameworkElement Visual { get; set; }

    public void Update()
    {
        // Update trend line based on current market data
    }

    public void CreateVisual()
    {
        Visual = new Line
        {
            X1 = StartPoint.X,
            Y1 = StartPoint.Y,
            X2 = EndPoint.X,
            Y2 = EndPoint.Y,
            Stroke = new SolidColorBrush(Color),
            StrokeThickness = 1,
            StrokeDashArray = new DoubleCollection { 3, 3 }
        };
    }
}

/// <summary>
/// Market calculations helper
/// </summary>
public class HoloMarketCalculator
{
    public double CalculatePriceChangeRate(HoloMarketItem item)
    {
        // Simplified price change calculation
        // In reality, would compare current price to historical data
        var random = new Random(item.Id.GetHashCode());
        return (random.NextDouble() - 0.5) * 10; // -5% to +5%
    }

    public MarketTrend DetermineMarketTrend(HoloMarketItem item, ObservableCollection<HoloPricePoint> history)
    {
        var priceChangeRate = CalculatePriceChangeRate(item);
        
        if (Math.Abs(priceChangeRate) > 3) return MarketTrend.Volatile;
        if (priceChangeRate > 1) return MarketTrend.Bullish;
        if (priceChangeRate < -1) return MarketTrend.Bearish;
        return MarketTrend.Stable;
    }
}

#endregion