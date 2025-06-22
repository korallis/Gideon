// ==========================================================================
// HoloMarketAnalysis.cs - Holographic Market Analysis Module Shell
// ==========================================================================
// Market analysis interface featuring holographic price charts, trade data,
// real-time market updates, and EVE market integration.
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Holographic market analysis module with real-time data visualization and trade analytics
/// </summary>
public class HoloMarketAnalysis : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloMarketAnalysis),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloMarketAnalysis),
            new PropertyMetadata(EVEColorScheme.GoldAccent, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(HoloMarketItem), typeof(HoloMarketAnalysis),
            new PropertyMetadata(null, OnSelectedItemChanged));

    public static readonly DependencyProperty MarketRegionProperty =
        DependencyProperty.Register(nameof(MarketRegion), typeof(HoloMarketRegion), typeof(HoloMarketAnalysis),
            new PropertyMetadata(null, OnMarketRegionChanged));

    public static readonly DependencyProperty ChartTimeframeProperty =
        DependencyProperty.Register(nameof(ChartTimeframe), typeof(MarketTimeframe), typeof(HoloMarketAnalysis),
            new PropertyMetadata(MarketTimeframe.Day, OnChartTimeframeChanged));

    public static readonly DependencyProperty EnableRealTimeUpdatesProperty =
        DependencyProperty.Register(nameof(EnableRealTimeUpdates), typeof(bool), typeof(HoloMarketAnalysis),
            new PropertyMetadata(true, OnEnableRealTimeUpdatesChanged));

    public static readonly DependencyProperty EnableChartAnimationsProperty =
        DependencyProperty.Register(nameof(EnableChartAnimations), typeof(bool), typeof(HoloMarketAnalysis),
            new PropertyMetadata(true, OnEnableChartAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloMarketAnalysis),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty MarketDataProperty =
        DependencyProperty.Register(nameof(MarketData), typeof(ObservableCollection<HoloMarketItem>), typeof(HoloMarketAnalysis),
            new PropertyMetadata(null));

    public static readonly DependencyProperty WatchListProperty =
        DependencyProperty.Register(nameof(WatchList), typeof(ObservableCollection<HoloMarketItem>), typeof(HoloMarketAnalysis),
            new PropertyMetadata(null));

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

    public HoloMarketItem SelectedItem
    {
        get => (HoloMarketItem)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public HoloMarketRegion MarketRegion
    {
        get => (HoloMarketRegion)GetValue(MarketRegionProperty);
        set => SetValue(MarketRegionProperty, value);
    }

    public MarketTimeframe ChartTimeframe
    {
        get => (MarketTimeframe)GetValue(ChartTimeframeProperty);
        set => SetValue(ChartTimeframeProperty, value);
    }

    public bool EnableRealTimeUpdates
    {
        get => (bool)GetValue(EnableRealTimeUpdatesProperty);
        set => SetValue(EnableRealTimeUpdatesProperty, value);
    }

    public bool EnableChartAnimations
    {
        get => (bool)GetValue(EnableChartAnimationsProperty);
        set => SetValue(EnableChartAnimationsProperty, value);
    }

    public bool EnableParticleEffects
    {
        get => (bool)GetValue(EnableParticleEffectsProperty);
        set => SetValue(EnableParticleEffectsProperty, value);
    }

    public ObservableCollection<HoloMarketItem> MarketData
    {
        get => (ObservableCollection<HoloMarketItem>)GetValue(MarketDataProperty);
        set => SetValue(MarketDataProperty, value);
    }

    public ObservableCollection<HoloMarketItem> WatchList
    {
        get => (ObservableCollection<HoloMarketItem>)GetValue(WatchListProperty);
        set => SetValue(WatchListProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloMarketAnalysisEventArgs> ItemSelected;
    public event EventHandler<HoloMarketAnalysisEventArgs> RegionChanged;
    public event EventHandler<HoloMarketAnalysisEventArgs> TimeframeChanged;
    public event EventHandler<HoloMarketAnalysisEventArgs> WatchListUpdated;
    public event EventHandler<HoloMarketAnalysisEventArgs> TradeOpportunityFound;

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode { get; private set; }

    public void SwitchToFullMode()
    {
        IsInSimplifiedMode = false;
        EnableChartAnimations = true;
        EnableParticleEffects = true;
        EnableRealTimeUpdates = true;
        UpdateAnalysisAppearance();
    }

    public void SwitchToSimplifiedMode()
    {
        IsInSimplifiedMode = true;
        EnableChartAnimations = false;
        EnableParticleEffects = false;
        EnableRealTimeUpdates = false;
        UpdateAnalysisAppearance();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => !_disposed && IsLoaded;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        EnableChartAnimations = settings.EnabledFeatures.HasFlag(AnimationFeatures.DataVisualization);
        EnableParticleEffects = settings.EnabledFeatures.HasFlag(AnimationFeatures.ParticleEffects);
        
        UpdateAnalysisAppearance();
    }

    #endregion

    #region Fields

    private Grid _mainGrid;
    private Grid _chartGrid;
    private Grid _marketListGrid;
    private Grid _watchListGrid;
    private Grid _detailsGrid;
    private Canvas _chartCanvas;
    private Canvas _effectCanvas;
    private ItemsControl _marketList;
    private ItemsControl _watchListControl;
    private StackPanel _detailsPanel;
    private ComboBox _regionSelector;
    private ComboBox _timeframeSelector;
    private TextBox _searchBox;
    private ProgressBar _loadingIndicator;
    
    private readonly List<ChartParticle> _particles = new();
    private readonly List<PricePoint> _priceHistory = new();
    private readonly List<Line> _chartLines = new();
    private DispatcherTimer _animationTimer;
    private DispatcherTimer _particleTimer;
    private DispatcherTimer _updateTimer;
    private double _animationPhase = 0;
    private double _particlePhase = 0;
    private readonly Random _random = new();
    private bool _disposed = false;
    private bool _isUpdating = false;

    #endregion

    #region Constructor

    public HoloMarketAnalysis()
    {
        InitializeAnalysis();
        SetupAnimations();
        
        // Register with animation intensity manager
        AnimationIntensityManager.Instance.RegisterTarget(this);
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        
        // Initialize collections
        MarketData = new ObservableCollection<HoloMarketItem>();
        WatchList = new ObservableCollection<HoloMarketItem>();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Search for market items
    /// </summary>
    public async Task<List<HoloMarketItem>> SearchMarketAsync(string query)
    {
        _isUpdating = true;
        _loadingIndicator.Visibility = Visibility.Visible;
        
        try
        {
            // Simulate market search
            await Task.Delay(500);
            
            var results = new List<HoloMarketItem>();
            
            // Add mock search results
            for (int i = 0; i < 10; i++)
            {
                results.Add(new HoloMarketItem
                {
                    Name = $"{query} Result {i + 1}",
                    TypeId = 1000 + i,
                    CurrentPrice = 1000000 + _random.NextDouble() * 50000000,
                    DailyVolume = 100 + _random.Next(10000),
                    PriceChange = (_random.NextDouble() - 0.5) * 20,
                    LastUpdated = DateTime.Now
                });
            }
            
            return results;
        }
        finally
        {
            _isUpdating = false;
            _loadingIndicator.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Add item to watch list
    /// </summary>
    public void AddToWatchList(HoloMarketItem item)
    {
        if (item == null || WatchList.Contains(item)) return;
        
        WatchList.Add(item);
        
        if (EnableParticleEffects && !IsInSimplifiedMode)
        {
            SpawnWatchListParticles();
        }
        
        WatchListUpdated?.Invoke(this, new HoloMarketAnalysisEventArgs 
        { 
            Item = item,
            Action = MarketAction.AddedToWatchList,
            Timestamp = DateTime.Now
        });
    }

    /// <summary>
    /// Remove item from watch list
    /// </summary>
    public void RemoveFromWatchList(HoloMarketItem item)
    {
        if (item == null) return;
        
        WatchList.Remove(item);
        
        WatchListUpdated?.Invoke(this, new HoloMarketAnalysisEventArgs 
        { 
            Item = item,
            Action = MarketAction.RemovedFromWatchList,
            Timestamp = DateTime.Now
        });
    }

    /// <summary>
    /// Update market data for selected region
    /// </summary>
    public async Task RefreshMarketDataAsync()
    {
        if (_isUpdating) return;
        
        _isUpdating = true;
        _loadingIndicator.Visibility = Visibility.Visible;
        
        try
        {
            // Simulate market data refresh
            await Task.Delay(1000);
            
            MarketData.Clear();
            
            // Generate mock market data
            for (int i = 0; i < 50; i++)
            {
                MarketData.Add(new HoloMarketItem
                {
                    Name = $"Market Item {i + 1}",
                    TypeId = 2000 + i,
                    CurrentPrice = 500000 + _random.NextDouble() * 100000000,
                    DailyVolume = 50 + _random.Next(5000),
                    PriceChange = (_random.NextDouble() - 0.5) * 25,
                    LastUpdated = DateTime.Now
                });
            }
            
            UpdatePriceChart();
        }
        finally
        {
            _isUpdating = false;
            _loadingIndicator.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Analyze trade opportunities
    /// </summary>
    public async Task<List<HoloTradeOpportunity>> AnalyzeTradeOpportunitiesAsync()
    {
        var opportunities = new List<HoloTradeOpportunity>();
        
        // Simulate trade analysis
        await Task.Delay(800);
        
        foreach (var item in MarketData.Take(10))
        {
            if (Math.Abs(item.PriceChange) > 10) // Significant price movement
            {
                opportunities.Add(new HoloTradeOpportunity
                {
                    Item = item,
                    OpportunityType = item.PriceChange > 0 ? OpportunityType.Sell : OpportunityType.Buy,
                    ProfitPotential = Math.Abs(item.PriceChange) * item.DailyVolume,
                    Confidence = 0.6 + (_random.NextDouble() * 0.3),
                    Region = MarketRegion
                });
            }
        }
        
        return opportunities;
    }

    #endregion

    #region Private Methods

    private void InitializeAnalysis()
    {
        CreateAnalysisInterface();
        UpdateAnalysisAppearance();
    }

    private void CreateAnalysisInterface()
    {
        // Main grid with 2x2 layout
        _mainGrid = new Grid();
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Header section
        CreateHeaderSection();
        
        // Chart section
        CreateChartSection();
        
        // Market list section
        CreateMarketListSection();
        
        // Watch list section
        CreateWatchListSection();
        
        // Details section
        CreateDetailsSection();

        Content = _mainGrid;
    }

    private void CreateHeaderSection()
    {
        var headerBorder = new Border
        {
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(2),
            Margin = new Thickness(10, 10, 10, 5),
            Padding = new Thickness(15, 10),
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(80, 255, 215, 0), 0.0),
                    new GradientStop(Color.FromArgb(40, 255, 215, 0), 1.0)
                }
            }
        };

        var headerGrid = new Grid();
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Title
        var titleText = new TextBlock
        {
            Text = "Market Analysis",
            Foreground = Brushes.White,
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(titleText, 0);

        // Search box
        _searchBox = new TextBox
        {
            Width = 200,
            Height = 30,
            Margin = new Thickness(10, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        _searchBox.KeyDown += OnSearchKeyDown;
        Grid.SetColumn(_searchBox, 1);

        // Region selector
        _regionSelector = new ComboBox
        {
            Width = 150,
            Height = 30,
            Margin = new Thickness(5, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(_regionSelector, 2);

        // Timeframe selector
        _timeframeSelector = new ComboBox
        {
            Width = 100,
            Height = 30,
            Margin = new Thickness(5, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        PopulateTimeframeSelector();
        Grid.SetColumn(_timeframeSelector, 3);

        headerGrid.Children.Add(titleText);
        headerGrid.Children.Add(_searchBox);
        headerGrid.Children.Add(_regionSelector);
        headerGrid.Children.Add(_timeframeSelector);

        headerBorder.Child = headerGrid;
        Grid.SetColumnSpan(headerBorder, 2);
        Grid.SetRow(headerBorder, 0);
        _mainGrid.Children.Add(headerBorder);
    }

    private void CreateChartSection()
    {
        var chartBorder = new Border
        {
            CornerRadius = new CornerRadius(12),
            BorderThickness = new Thickness(2),
            Margin = new Thickness(10, 5, 5, 10),
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(40, 0, 30, 60), 0.0),
                    new GradientStop(Color.FromArgb(20, 0, 20, 40), 1.0)
                }
            }
        };

        _chartGrid = new Grid();
        _chartGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _chartGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var chartHeader = new TextBlock
        {
            Text = "Price Chart",
            Foreground = Brushes.White,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(15, 10, 15, 5)
        };
        Grid.SetRow(chartHeader, 0);

        // Chart canvas container
        var chartContainer = new Border
        {
            Margin = new Thickness(15, 5, 15, 15),
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)),
            CornerRadius = new CornerRadius(6)
        };

        _chartCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            ClipToBounds = true
        };
        chartContainer.Child = _chartCanvas;
        Grid.SetRow(chartContainer, 1);

        // Loading indicator
        _loadingIndicator = new ProgressBar
        {
            IsIndeterminate = true,
            Height = 4,
            VerticalAlignment = VerticalAlignment.Top,
            Visibility = Visibility.Collapsed
        };
        Grid.SetRow(_loadingIndicator, 1);

        _chartGrid.Children.Add(chartHeader);
        _chartGrid.Children.Add(chartContainer);
        _chartGrid.Children.Add(_loadingIndicator);

        chartBorder.Child = _chartGrid;
        Grid.SetColumn(chartBorder, 0);
        Grid.SetRow(chartBorder, 1);
        _mainGrid.Children.Add(chartBorder);
    }

    private void CreateMarketListSection()
    {
        var listBorder = new Border
        {
            CornerRadius = new CornerRadius(12),
            BorderThickness = new Thickness(2),
            Margin = new Thickness(5, 5, 10, 5),
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(40, 20, 0, 40), 0.0),
                    new GradientStop(Color.FromArgb(20, 15, 0, 30), 1.0)
                }
            }
        };

        _marketListGrid = new Grid();
        _marketListGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _marketListGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var listHeader = new TextBlock
        {
            Text = "Market Data",
            Foreground = Brushes.White,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(15, 10, 15, 5)
        };
        Grid.SetRow(listHeader, 0);

        _marketList = new ItemsControl
        {
            Margin = new Thickness(15, 5, 15, 15),
            ItemTemplate = CreateMarketItemTemplate()
        };

        var listScrollViewer = new ScrollViewer
        {
            Content = _marketList,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
        };
        Grid.SetRow(listScrollViewer, 1);

        _marketListGrid.Children.Add(listHeader);
        _marketListGrid.Children.Add(listScrollViewer);

        listBorder.Child = _marketListGrid;
        Grid.SetColumn(listBorder, 1);
        Grid.SetRow(listBorder, 1);
        Grid.SetRowSpan(listBorder, 1);
        _mainGrid.Children.Add(listBorder);
    }

    private void CreateWatchListSection()
    {
        // Watch list will be part of the market list section for now
        // Can be expanded to separate section if needed
    }

    private void CreateDetailsSection()
    {
        // Details will be shown in overlay or separate panel
        _detailsPanel = new StackPanel();
    }

    private void PopulateTimeframeSelector()
    {
        _timeframeSelector.Items.Add(new ComboBoxItem { Content = "1 Hour", Tag = MarketTimeframe.Hour });
        _timeframeSelector.Items.Add(new ComboBoxItem { Content = "1 Day", Tag = MarketTimeframe.Day });
        _timeframeSelector.Items.Add(new ComboBoxItem { Content = "1 Week", Tag = MarketTimeframe.Week });
        _timeframeSelector.Items.Add(new ComboBoxItem { Content = "1 Month", Tag = MarketTimeframe.Month });
        _timeframeSelector.SelectedIndex = 1; // Default to Day
    }

    private DataTemplate CreateMarketItemTemplate()
    {
        var template = new DataTemplate();
        
        var border = new FrameworkElementFactory(typeof(Border));
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
        border.SetValue(Border.MarginProperty, new Thickness(2));
        border.SetValue(Border.PaddingProperty, new Thickness(8, 4));
        border.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)));
        
        var grid = new FrameworkElementFactory(typeof(Grid));
        grid.SetValue(Grid.ColumnDefinitionsProperty, new ColumnDefinitionCollection
        {
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            new ColumnDefinition { Width = GridLength.Auto },
            new ColumnDefinition { Width = GridLength.Auto }
        });
        
        var nameText = new FrameworkElementFactory(typeof(TextBlock));
        nameText.SetBinding(TextBlock.TextProperty, new Binding("Name"));
        nameText.SetValue(TextBlock.ForegroundProperty, Brushes.White);
        nameText.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        nameText.SetValue(Grid.ColumnProperty, 0);
        
        var priceText = new FrameworkElementFactory(typeof(TextBlock));
        priceText.SetBinding(TextBlock.TextProperty, new Binding("CurrentPrice") { StringFormat = "{0:N0} ISK" });
        priceText.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Color.FromRgb(255, 215, 0)));
        priceText.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        priceText.SetValue(Grid.ColumnProperty, 1);
        priceText.SetValue(TextBlock.MarginProperty, new Thickness(10, 0, 0, 0));
        
        var changeText = new FrameworkElementFactory(typeof(TextBlock));
        changeText.SetBinding(TextBlock.TextProperty, new Binding("PriceChange") { StringFormat = "{0:+0.0;-0.0}%" });
        changeText.SetValue(Grid.ColumnProperty, 2);
        changeText.SetValue(TextBlock.MarginProperty, new Thickness(10, 0, 0, 0));
        changeText.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        
        grid.AppendChild(nameText);
        grid.AppendChild(priceText);
        grid.AppendChild(changeText);
        border.AppendChild(grid);
        
        template.VisualTree = border;
        return template;
    }

    private void SetupAnimations()
    {
        // Main animation timer
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
        };
        _animationTimer.Tick += OnAnimationTick;

        // Particle animation timer
        _particleTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50) // 20 FPS
        };
        _particleTimer.Tick += OnParticleTick;

        // Data update timer
        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30) // Update every 30 seconds
        };
        _updateTimer.Tick += OnUpdateTick;
    }

    private void OnAnimationTick(object sender, EventArgs e)
    {
        if (!EnableChartAnimations || IsInSimplifiedMode) return;

        _animationPhase += 0.05;
        if (_animationPhase > Math.PI * 2)
            _animationPhase = 0;

        UpdateChartAnimations();
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        if (!EnableParticleEffects || IsInSimplifiedMode || _effectCanvas == null) return;

        _particlePhase += 0.1;
        if (_particlePhase > Math.PI * 2)
            _particlePhase = 0;

        UpdateParticleEffects();
    }

    private void OnUpdateTick(object sender, EventArgs e)
    {
        if (EnableRealTimeUpdates && !_isUpdating)
        {
            _ = RefreshMarketDataAsync();
        }
    }

    private void UpdateChartAnimations()
    {
        foreach (var line in _chartLines)
        {
            if (line.Effect is DropShadowEffect effect)
            {
                var intensity = 0.6 + (Math.Sin(_animationPhase) * 0.3);
                effect.Opacity = HolographicIntensity * intensity;
            }
        }
    }

    private void UpdateParticleEffects()
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var particle = _particles[i];
            
            particle.X += particle.VelocityX;
            particle.Y += particle.VelocityY;
            particle.Life -= 0.02;
            particle.VelocityY += 0.1; // Gravity effect

            Canvas.SetLeft(particle.Visual, particle.X);
            Canvas.SetTop(particle.Visual, particle.Y);
            particle.Visual.Opacity = Math.Max(0, particle.Life) * HolographicIntensity;

            if (particle.Life <= 0 || particle.Y > _chartCanvas.ActualHeight + 10)
            {
                _chartCanvas.Children.Remove(particle.Visual);
                _particles.RemoveAt(i);
            }
        }
    }

    private void UpdatePriceChart()
    {
        if (_chartCanvas == null || SelectedItem == null) return;

        _chartCanvas.Children.Clear();
        _chartLines.Clear();
        _priceHistory.Clear();

        // Generate sample price history
        GenerateSamplePriceData();
        
        // Draw price chart
        DrawPriceChart();
        
        // Add chart effects
        if (_effectCanvas == null)
        {
            _effectCanvas = new Canvas
            {
                IsHitTestVisible = false,
                ClipToBounds = true
            };
            _chartCanvas.Children.Add(_effectCanvas);
        }
    }

    private void GenerateSamplePriceData()
    {
        if (SelectedItem == null) return;

        var basePrice = SelectedItem.CurrentPrice;
        var timePoints = GetTimePointsForTimeframe(ChartTimeframe);
        
        for (int i = 0; i < timePoints; i++)
        {
            var variation = (_random.NextDouble() - 0.5) * 0.1; // ±10% variation
            var price = basePrice * (1 + variation);
            
            _priceHistory.Add(new PricePoint
            {
                Timestamp = DateTime.Now.AddMinutes(-timePoints + i),
                Price = price,
                Volume = 100 + _random.Next(1000)
            });
        }
    }

    private void DrawPriceChart()
    {
        if (_priceHistory.Count < 2) return;

        var width = _chartCanvas.ActualWidth;
        var height = _chartCanvas.ActualHeight;
        
        if (width <= 0 || height <= 0) return;

        var minPrice = _priceHistory.Min(p => p.Price);
        var maxPrice = _priceHistory.Max(p => p.Price);
        var priceRange = maxPrice - minPrice;
        
        if (priceRange <= 0) return;

        var color = GetEVEColor(EVEColorScheme);
        
        // Draw price line
        for (int i = 1; i < _priceHistory.Count; i++)
        {
            var x1 = (double)(i - 1) / (_priceHistory.Count - 1) * width;
            var y1 = height - ((_priceHistory[i - 1].Price - minPrice) / priceRange * height);
            var x2 = (double)i / (_priceHistory.Count - 1) * width;
            var y2 = height - ((_priceHistory[i].Price - minPrice) / priceRange * height);

            var line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 2
            };

            if (!IsInSimplifiedMode)
            {
                line.Effect = new DropShadowEffect
                {
                    Color = color,
                    BlurRadius = 4 * HolographicIntensity,
                    ShadowDepth = 0,
                    Opacity = 0.6 * HolographicIntensity
                };
            }

            _chartLines.Add(line);
            _chartCanvas.Children.Add(line);
        }
    }

    private void SpawnWatchListParticles()
    {
        if (_effectCanvas == null) return;

        for (int i = 0; i < 8; i++)
        {
            var particle = CreateChartParticle();
            _particles.Add(particle);
            _chartCanvas.Children.Add(particle.Visual);
        }
    }

    private ChartParticle CreateChartParticle()
    {
        var color = GetEVEColor(EVEColorScheme);
        var size = 2 + _random.NextDouble() * 3;
        
        var ellipse = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromArgb(150, color.R, color.G, color.B))
        };

        var particle = new ChartParticle
        {
            Visual = ellipse,
            X = _random.NextDouble() * _chartCanvas.ActualWidth,
            Y = _chartCanvas.ActualHeight,
            VelocityX = (_random.NextDouble() - 0.5) * 2,
            VelocityY = -2 - _random.NextDouble() * 3,
            Life = 1.0
        };

        Canvas.SetLeft(ellipse, particle.X);
        Canvas.SetTop(ellipse, particle.Y);

        return particle;
    }

    private int GetTimePointsForTimeframe(MarketTimeframe timeframe)
    {
        return timeframe switch
        {
            MarketTimeframe.Hour => 60,
            MarketTimeframe.Day => 144, // 10-minute intervals
            MarketTimeframe.Week => 168, // 1-hour intervals
            MarketTimeframe.Month => 120, // 6-hour intervals
            _ => 144
        };
    }

    private void UpdateAnalysisAppearance()
    {
        UpdateColors();
        UpdateEffects();
        
        if (_marketList != null && MarketData != null)
        {
            _marketList.ItemsSource = MarketData;
        }
    }

    private void UpdateColors()
    {
        var color = GetEVEColor(EVEColorScheme);

        // Update border colors
        foreach (Border border in _mainGrid.Children.OfType<Border>())
        {
            border.BorderBrush = new SolidColorBrush(Color.FromArgb(180, color.R, color.G, color.B));
        }
    }

    private void UpdateEffects()
    {
        if (IsInSimplifiedMode) return;

        var color = GetEVEColor(EVEColorScheme);

        foreach (Border border in _mainGrid.Children.OfType<Border>())
        {
            border.Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 8 * HolographicIntensity,
                ShadowDepth = 0,
                Opacity = 0.4 * HolographicIntensity
            };
        }
    }

    private Color GetEVEColor(EVEColorScheme scheme)
    {
        return scheme switch
        {
            EVEColorScheme.ElectricBlue => Color.FromRgb(64, 224, 255),
            EVEColorScheme.GoldAccent => Color.FromRgb(255, 215, 0),
            EVEColorScheme.CrimsonRed => Color.FromRgb(220, 20, 60),
            EVEColorScheme.EmeraldGreen => Color.FromRgb(50, 205, 50),
            EVEColorScheme.VoidPurple => Color.FromRgb(138, 43, 226),
            _ => Color.FromRgb(255, 215, 0)
        };
    }

    private async void OnSearchKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(_searchBox.Text))
        {
            var results = await SearchMarketAsync(_searchBox.Text);
            MarketData.Clear();
            foreach (var item in results)
            {
                MarketData.Add(item);
            }
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableChartAnimations && !IsInSimplifiedMode)
            _animationTimer.Start();
            
        if (EnableParticleEffects && !IsInSimplifiedMode)
            _particleTimer.Start();
            
        if (EnableRealTimeUpdates)
            _updateTimer.Start();

        // Initial data load
        _ = RefreshMarketDataAsync();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        _particleTimer?.Stop();
        _updateTimer?.Stop();
        
        // Clean up particles and chart elements
        _particles.Clear();
        _chartLines.Clear();
        _chartCanvas?.Children.Clear();
        
        // Unregister from animation intensity manager
        AnimationIntensityManager.Instance.UnregisterTarget(this);
        
        _disposed = true;
    }

    #endregion

    #region Event Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketAnalysis analysis)
            analysis.UpdateAnalysisAppearance();
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketAnalysis analysis)
            analysis.UpdateAnalysisAppearance();
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketAnalysis analysis)
        {
            analysis.UpdatePriceChart();
            analysis.ItemSelected?.Invoke(analysis, new HoloMarketAnalysisEventArgs 
            { 
                Item = (HoloMarketItem)e.NewValue,
                Timestamp = DateTime.Now
            });
        }
    }

    private static void OnMarketRegionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketAnalysis analysis)
        {
            analysis.RegionChanged?.Invoke(analysis, new HoloMarketAnalysisEventArgs 
            { 
                Region = (HoloMarketRegion)e.NewValue,
                Timestamp = DateTime.Now
            });
            _ = analysis.RefreshMarketDataAsync();
        }
    }

    private static void OnChartTimeframeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketAnalysis analysis)
        {
            analysis.UpdatePriceChart();
            analysis.TimeframeChanged?.Invoke(analysis, new HoloMarketAnalysisEventArgs 
            { 
                Timeframe = (MarketTimeframe)e.NewValue,
                Timestamp = DateTime.Now
            });
        }
    }

    private static void OnEnableRealTimeUpdatesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketAnalysis analysis)
        {
            if ((bool)e.NewValue)
                analysis._updateTimer.Start();
            else
                analysis._updateTimer.Stop();
        }
    }

    private static void OnEnableChartAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketAnalysis analysis)
        {
            if ((bool)e.NewValue && !analysis.IsInSimplifiedMode)
                analysis._animationTimer.Start();
            else
                analysis._animationTimer.Stop();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketAnalysis analysis)
        {
            if ((bool)e.NewValue && !analysis.IsInSimplifiedMode)
                analysis._particleTimer.Start();
            else
                analysis._particleTimer.Stop();
        }
    }

    #endregion
}

/// <summary>
/// Chart particle for visual effects
/// </summary>
internal class ChartParticle
{
    public FrameworkElement Visual { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
}

/// <summary>
/// Price point for chart data
/// </summary>
public class PricePoint
{
    public DateTime Timestamp { get; set; }
    public double Price { get; set; }
    public long Volume { get; set; }
}

/// <summary>
/// Market item data
/// </summary>
public class HoloMarketItem
{
    public string Name { get; set; }
    public int TypeId { get; set; }
    public double CurrentPrice { get; set; }
    public long DailyVolume { get; set; }
    public double PriceChange { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Market region information
/// </summary>
public class HoloMarketRegion
{
    public int RegionId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

/// <summary>
/// Trade opportunity analysis
/// </summary>
public class HoloTradeOpportunity
{
    public HoloMarketItem Item { get; set; }
    public OpportunityType OpportunityType { get; set; }
    public double ProfitPotential { get; set; }
    public double Confidence { get; set; }
    public HoloMarketRegion Region { get; set; }
}

/// <summary>
/// Market chart timeframes
/// </summary>
public enum MarketTimeframe
{
    Hour,
    Day,
    Week,
    Month
}

/// <summary>
/// Trade opportunity types
/// </summary>
public enum OpportunityType
{
    Buy,
    Sell,
    Arbitrage
}

/// <summary>
/// Market actions
/// </summary>
public enum MarketAction
{
    AddedToWatchList,
    RemovedFromWatchList,
    PriceAlert,
    TradeOpportunity
}

/// <summary>
/// Event args for market analysis events
/// </summary>
public class HoloMarketAnalysisEventArgs : EventArgs
{
    public HoloMarketItem Item { get; set; }
    public HoloMarketRegion Region { get; set; }
    public MarketTimeframe Timeframe { get; set; }
    public MarketAction Action { get; set; }
    public DateTime Timestamp { get; set; }
}