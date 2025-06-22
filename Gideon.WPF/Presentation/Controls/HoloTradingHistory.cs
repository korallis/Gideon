// ==========================================================================
// HoloTradingHistory.cs - Holographic Trading History Visualization
// ==========================================================================
// Advanced trading history featuring comprehensive transaction tracking,
// performance analytics, EVE-style trading analysis, and holographic history visualization.
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
/// Holographic trading history with comprehensive transaction analysis and performance visualization
/// </summary>
public class HoloTradingHistory : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloTradingHistory),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloTradingHistory),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty TradingHistoryProperty =
        DependencyProperty.Register(nameof(TradingHistory), typeof(ObservableCollection<HoloTradeRecord>), typeof(HoloTradingHistory),
            new PropertyMetadata(null, OnTradingHistoryChanged));

    public static readonly DependencyProperty HistorySummaryProperty =
        DependencyProperty.Register(nameof(HistorySummary), typeof(HoloTradingHistorySummary), typeof(HoloTradingHistory),
            new PropertyMetadata(null, OnHistorySummaryChanged));

    public static readonly DependencyProperty SelectedCharacterProperty =
        DependencyProperty.Register(nameof(SelectedCharacter), typeof(string), typeof(HoloTradingHistory),
            new PropertyMetadata("All Characters", OnSelectedCharacterChanged));

    public static readonly DependencyProperty TimeFrameProperty =
        DependencyProperty.Register(nameof(TimeFrame), typeof(HistoryTimeFrame), typeof(HoloTradingHistory),
            new PropertyMetadata(HistoryTimeFrame.Month, OnTimeFrameChanged));

    public static readonly DependencyProperty ViewModeProperty =
        DependencyProperty.Register(nameof(ViewMode), typeof(HistoryViewMode), typeof(HoloTradingHistory),
            new PropertyMetadata(HistoryViewMode.Summary, OnViewModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloTradingHistory),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloTradingHistory),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowPerformanceChartsProperty =
        DependencyProperty.Register(nameof(ShowPerformanceCharts), typeof(bool), typeof(HoloTradingHistory),
            new PropertyMetadata(true, OnShowPerformanceChartsChanged));

    public static readonly DependencyProperty ShowTransactionDetailsProperty =
        DependencyProperty.Register(nameof(ShowTransactionDetails), typeof(bool), typeof(HoloTradingHistory),
            new PropertyMetadata(true, OnShowTransactionDetailsChanged));

    public static readonly DependencyProperty ShowProfitLossAnalysisProperty =
        DependencyProperty.Register(nameof(ShowProfitLossAnalysis), typeof(bool), typeof(HoloTradingHistory),
            new PropertyMetadata(true, OnShowProfitLossAnalysisChanged));

    public static readonly DependencyProperty AutoRefreshProperty =
        DependencyProperty.Register(nameof(AutoRefresh), typeof(bool), typeof(HoloTradingHistory),
            new PropertyMetadata(false, OnAutoRefreshChanged));

    public static readonly DependencyProperty UpdateIntervalProperty =
        DependencyProperty.Register(nameof(UpdateInterval), typeof(TimeSpan), typeof(HoloTradingHistory),
            new PropertyMetadata(TimeSpan.FromMinutes(5), OnUpdateIntervalChanged));

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

    public ObservableCollection<HoloTradeRecord> TradingHistory
    {
        get => (ObservableCollection<HoloTradeRecord>)GetValue(TradingHistoryProperty);
        set => SetValue(TradingHistoryProperty, value);
    }

    public HoloTradingHistorySummary HistorySummary
    {
        get => (HoloTradingHistorySummary)GetValue(HistorySummaryProperty);
        set => SetValue(HistorySummaryProperty, value);
    }

    public string SelectedCharacter
    {
        get => (string)GetValue(SelectedCharacterProperty);
        set => SetValue(SelectedCharacterProperty, value);
    }

    public HistoryTimeFrame TimeFrame
    {
        get => (HistoryTimeFrame)GetValue(TimeFrameProperty);
        set => SetValue(TimeFrameProperty, value);
    }

    public HistoryViewMode ViewMode
    {
        get => (HistoryViewMode)GetValue(ViewModeProperty);
        set => SetValue(ViewModeProperty, value);
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

    public bool ShowTransactionDetails
    {
        get => (bool)GetValue(ShowTransactionDetailsProperty);
        set => SetValue(ShowTransactionDetailsProperty, value);
    }

    public bool ShowProfitLossAnalysis
    {
        get => (bool)GetValue(ShowProfitLossAnalysisProperty);
        set => SetValue(ShowProfitLossAnalysisProperty, value);
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

    private Canvas _historyCanvas;
    private ScrollViewer _scrollViewer;
    private StackPanel _contentPanel;
    private DispatcherTimer _animationTimer;
    private DispatcherTimer _particleTimer;
    private DispatcherTimer _updateTimer;
    private Dictionary<string, Material> _materialCache;
    private Random _random;
    private List<UIElement> _particles;
    private List<HoloHistoryParticle> _historyParticles;
    private bool _isInitialized;

    #endregion

    #region Constructor

    public HoloTradingHistory()
    {
        InitializeComponent();
        InitializeFields();
        InitializeHistory();
        InitializeTimers();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 1000;
        Height = 700;
        Background = new SolidColorBrush(Color.FromArgb(20, 0, 100, 200));
        
        _historyCanvas = new Canvas
        {
            Width = Width,
            Height = Height,
            ClipToBounds = true
        };
        
        Content = _historyCanvas;
    }

    private void InitializeFields()
    {
        _materialCache = new Dictionary<string, Material>();
        _random = new Random();
        _particles = new List<UIElement>();
        _historyParticles = new List<HoloHistoryParticle>();
        _isInitialized = false;
        
        TradingHistory = new ObservableCollection<HoloTradeRecord>();
        InitializeSampleHistory();
    }

    private void InitializeHistory()
    {
        CreateHistoryInterface();
        UpdateHistoryVisuals();
        CalculateHistorySummary();
    }

    private void InitializeTimers()
    {
        _animationTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _animationTimer.Tick += OnAnimationTick;

        _particleTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(150)
        };
        _particleTimer.Tick += OnParticleTick;

        _updateTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = UpdateInterval
        };
        _updateTimer.Tick += OnUpdateTick;
    }

    private void InitializeSampleHistory()
    {
        var baseDate = DateTime.Now.AddDays(-30);
        var sampleTrades = new List<HoloTradeRecord>();
        
        for (int i = 0; i < 50; i++)
        {
            var tradeDate = baseDate.AddDays(_random.NextDouble() * 30);
            var items = new[] { "Tritanium", "Pyerite", "Mexallon", "Isogen", "Nocxium", "Zydrine", "Megacyte", "Morphite", "PLEX", "Skill Injector" };
            var regions = new[] { "The Forge", "Domain", "Sinq Laison", "Metropolis", "Heimatar" };
            var characters = new[] { "Main Character", "Alt Character", "Trading Alt" };
            
            var item = items[_random.Next(items.Length)];
            var basePrice = GetBasePrice(item);
            var quantity = GetRandomQuantity(item);
            var buyPrice = basePrice * (0.9 + _random.NextDouble() * 0.2);
            var sellPrice = basePrice * (0.95 + _random.NextDouble() * 0.3);
            
            var buyTrade = new HoloTradeRecord
            {
                TransactionId = Guid.NewGuid().ToString(),
                ItemName = item,
                TradeType = TradeType.Buy,
                Quantity = quantity,
                Price = buyPrice,
                TotalValue = buyPrice * quantity,
                Region = regions[_random.Next(regions.Length)],
                Station = "Trade Hub",
                Character = characters[_random.Next(characters.Length)],
                Timestamp = tradeDate,
                Fees = buyPrice * quantity * 0.03,
                Tax = 0
            };
            
            var sellTrade = new HoloTradeRecord
            {
                TransactionId = Guid.NewGuid().ToString(),
                ItemName = item,
                TradeType = TradeType.Sell,
                Quantity = quantity,
                Price = sellPrice,
                TotalValue = sellPrice * quantity,
                Region = buyTrade.Region,
                Station = buyTrade.Station,
                Character = buyTrade.Character,
                Timestamp = tradeDate.AddHours(_random.NextDouble() * 24),
                Fees = sellPrice * quantity * 0.03,
                Tax = sellPrice * quantity * 0.08
            };
            
            sampleTrades.Add(buyTrade);
            sampleTrades.Add(sellTrade);
        }
        
        foreach (var trade in sampleTrades.OrderByDescending(t => t.Timestamp))
        {
            TradingHistory.Add(trade);
        }
    }

    private double GetBasePrice(string item)
    {
        return item switch
        {
            "Tritanium" => 5.50,
            "Pyerite" => 12.30,
            "Mexallon" => 78.50,
            "Isogen" => 125.80,
            "Nocxium" => 845.60,
            "Zydrine" => 1256.70,
            "Megacyte" => 2845.30,
            "Morphite" => 8750.90,
            "PLEX" => 3256890.00,
            "Skill Injector" => 856750.00,
            _ => 100.00
        };
    }

    private long GetRandomQuantity(string item)
    {
        return item switch
        {
            "Tritanium" => _random.Next(100000, 1000000),
            "Pyerite" => _random.Next(50000, 500000),
            "Mexallon" => _random.Next(20000, 200000),
            "Isogen" => _random.Next(10000, 150000),
            "Nocxium" => _random.Next(5000, 75000),
            "Zydrine" => _random.Next(2000, 30000),
            "Megacyte" => _random.Next(1000, 15000),
            "Morphite" => _random.Next(500, 5000),
            "PLEX" => _random.Next(1, 10),
            "Skill Injector" => _random.Next(1, 5),
            _ => _random.Next(1000, 10000)
        };
    }

    #endregion

    #region History Interface Creation

    private void CreateHistoryInterface()
    {
        _historyCanvas.Children.Clear();
        
        CreateHeaderSection();
        CreateSummarySection();
        CreateHistoryDisplay();
        CreatePerformanceCharts();
        CreateParticleEffects();
    }

    private void CreateHeaderSection()
    {
        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(20, 10, 20, 10),
            Background = CreateHolographicBrush(0.4)
        };
        
        Canvas.SetLeft(headerPanel, 0);
        Canvas.SetTop(headerPanel, 0);
        _historyCanvas.Children.Add(headerPanel);
        
        var titleBlock = new TextBlock
        {
            Text = $"Trading History - {TimeFrame} View",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary"),
            Margin = new Thickness(10),
            VerticalAlignment = VerticalAlignment.Center
        };
        headerPanel.Children.Add(titleBlock);
        
        AddHeaderButton(headerPanel, "Summary", () => ViewMode = HistoryViewMode.Summary);
        AddHeaderButton(headerPanel, "Details", () => ViewMode = HistoryViewMode.Detailed);
        AddHeaderButton(headerPanel, "Charts", () => ViewMode = HistoryViewMode.Charts);
        AddHeaderButton(headerPanel, "Export", OnExportClick);
    }

    private void CreateSummarySection()
    {
        var summaryPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(20, 5, 20, 5),
            Background = CreateHolographicBrush(0.3)
        };
        
        Canvas.SetLeft(summaryPanel, 0);
        Canvas.SetTop(summaryPanel, 60);
        _historyCanvas.Children.Add(summaryPanel);
        
        var summary = HistorySummary ?? new HoloTradingHistorySummary();
        
        AddSummaryCard(summaryPanel, "Total Trades", summary.TotalTrades.ToString("N0"));
        AddSummaryCard(summaryPanel, "Total Volume", FormatCurrency(summary.TotalVolume));
        AddSummaryCard(summaryPanel, "Total Profit", FormatCurrency(summary.TotalProfit), summary.TotalProfit >= 0 ? "Success" : "Negative");
        AddSummaryCard(summaryPanel, "Win Rate", $"{summary.WinRate:F1}%", summary.WinRate >= 70 ? "Success" : summary.WinRate >= 50 ? "Warning" : "Negative");
        AddSummaryCard(summaryPanel, "Avg Profit/Trade", FormatCurrency(summary.AverageProfitPerTrade));
        AddSummaryCard(summaryPanel, "Best Trade", FormatCurrency(summary.BestTrade), "Success");
    }

    private void CreateHistoryDisplay()
    {
        _scrollViewer = new ScrollViewer
        {
            Width = Width - 40,
            Height = ViewMode == HistoryViewMode.Charts ? 300 : 400,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = CreateHolographicBrush(0.2)
        };
        
        Canvas.SetLeft(_scrollViewer, 20);
        Canvas.SetTop(_scrollViewer, 120);
        _historyCanvas.Children.Add(_scrollViewer);
        
        _contentPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(5)
        };
        
        _scrollViewer.Content = _contentPanel;
        
        PopulateHistoryContent();
    }

    private void CreatePerformanceCharts()
    {
        if (!ShowPerformanceCharts || ViewMode != HistoryViewMode.Charts) return;
        
        var chartsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(20),
            Background = CreateHolographicBrush(0.25)
        };
        
        Canvas.SetLeft(chartsPanel, 20);
        Canvas.SetTop(chartsPanel, 440);
        _historyCanvas.Children.Add(chartsPanel);
        
        CreateProfitChart(chartsPanel);
        CreateVolumeChart(chartsPanel);
        CreatePerformanceMetrics(chartsPanel);
    }

    private void PopulateHistoryContent()
    {
        if (_contentPanel == null || TradingHistory == null) return;
        
        _contentPanel.Children.Clear();
        
        var filteredHistory = GetFilteredHistory();
        
        if (ViewMode == HistoryViewMode.Summary)
        {
            PopulateSummaryView(filteredHistory);
        }
        else if (ViewMode == HistoryViewMode.Detailed)
        {
            PopulateDetailedView(filteredHistory);
        }
    }

    private void PopulateSummaryView(List<HoloTradeRecord> history)
    {
        var groupedTrades = history.GroupBy(t => t.ItemName)
                                  .Select(g => new HoloTradeGroupSummary
                                  {
                                      ItemName = g.Key,
                                      TotalTrades = g.Count(),
                                      TotalVolume = g.Sum(t => t.TotalValue),
                                      TotalProfit = CalculateGroupProfit(g.ToList()),
                                      WinRate = CalculateGroupWinRate(g.ToList()),
                                      LastTradeDate = g.Max(t => t.Timestamp)
                                  })
                                  .OrderByDescending(g => g.TotalVolume)
                                  .ToList();
        
        foreach (var group in groupedTrades)
        {
            var groupControl = CreateTradeGroupControl(group);
            _contentPanel.Children.Add(groupControl);
        }
    }

    private void PopulateDetailedView(List<HoloTradeRecord> history)
    {
        foreach (var trade in history.Take(100))
        {
            var tradeControl = CreateTradeRecordControl(trade);
            _contentPanel.Children.Add(tradeControl);
        }
    }

    private UserControl CreateTradeGroupControl(HoloTradeGroupSummary group)
    {
        var groupControl = new UserControl
        {
            Margin = new Thickness(2),
            Background = CreateHolographicBrush(0.3)
        };
        
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
        
        var nameBlock = new TextBlock
        {
            Text = group.ItemName,
            Foreground = GetEVEBrush("Primary"),
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5)
        };
        Grid.SetColumn(nameBlock, 0);
        grid.Children.Add(nameBlock);
        
        var tradesBlock = new TextBlock
        {
            Text = group.TotalTrades.ToString("N0"),
            Foreground = GetEVEBrush("Secondary"),
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(5)
        };
        Grid.SetColumn(tradesBlock, 1);
        grid.Children.Add(tradesBlock);
        
        var volumeBlock = new TextBlock
        {
            Text = FormatCurrency(group.TotalVolume),
            Foreground = GetEVEBrush("Info"),
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Right,
            Margin = new Thickness(5)
        };
        Grid.SetColumn(volumeBlock, 2);
        grid.Children.Add(volumeBlock);
        
        var profitColor = group.TotalProfit >= 0 ? GetEVEBrush("Success") : GetEVEBrush("Negative");
        var profitBlock = new TextBlock
        {
            Text = FormatCurrency(group.TotalProfit),
            Foreground = profitColor,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Right,
            Margin = new Thickness(5),
            FontWeight = FontWeights.Bold
        };
        Grid.SetColumn(profitBlock, 3);
        grid.Children.Add(profitBlock);
        
        var winRateColor = group.WinRate >= 70 ? GetEVEBrush("Success") : 
                          group.WinRate >= 50 ? GetEVEBrush("Warning") : GetEVEBrush("Negative");
        var winRateBlock = new TextBlock
        {
            Text = $"{group.WinRate:F1}%",
            Foreground = winRateColor,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(5)
        };
        Grid.SetColumn(winRateBlock, 4);
        grid.Children.Add(winRateBlock);
        
        var dateBlock = new TextBlock
        {
            Text = group.LastTradeDate.ToString("MMM dd"),
            Foreground = GetEVEBrush("Secondary"),
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(5)
        };
        Grid.SetColumn(dateBlock, 5);
        grid.Children.Add(dateBlock);
        
        groupControl.Content = grid;
        return groupControl;
    }

    private UserControl CreateTradeRecordControl(HoloTradeRecord trade)
    {
        var tradeControl = new UserControl
        {
            Margin = new Thickness(1),
            Background = CreateHolographicBrush(0.25)
        };
        
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
        
        var typeColor = trade.TradeType == TradeType.Buy ? GetEVEBrush("Info") : GetEVEBrush("Warning");
        var typeBlock = new TextBlock
        {
            Text = trade.TradeType.ToString().ToUpper(),
            Foreground = typeColor,
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(2)
        };
        Grid.SetColumn(typeBlock, 0);
        grid.Children.Add(typeBlock);
        
        var nameBlock = new TextBlock
        {
            Text = trade.ItemName,
            Foreground = GetEVEBrush("Primary"),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5)
        };
        Grid.SetColumn(nameBlock, 1);
        grid.Children.Add(nameBlock);
        
        var quantityBlock = new TextBlock
        {
            Text = trade.Quantity.ToString("N0"),
            Foreground = GetEVEBrush("Secondary"),
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Right,
            Margin = new Thickness(5)
        };
        Grid.SetColumn(quantityBlock, 2);
        grid.Children.Add(quantityBlock);
        
        var priceBlock = new TextBlock
        {
            Text = FormatPrice(trade.Price),
            Foreground = GetEVEBrush("Secondary"),
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Right,
            Margin = new Thickness(5)
        };
        Grid.SetColumn(priceBlock, 3);
        grid.Children.Add(priceBlock);
        
        var valueBlock = new TextBlock
        {
            Text = FormatCurrency(trade.TotalValue),
            Foreground = GetEVEBrush("Info"),
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Right,
            Margin = new Thickness(5)
        };
        Grid.SetColumn(valueBlock, 4);
        grid.Children.Add(valueBlock);
        
        var regionBlock = new TextBlock
        {
            Text = trade.Region,
            Foreground = GetEVEBrush("Secondary"),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5)
        };
        Grid.SetColumn(regionBlock, 5);
        grid.Children.Add(regionBlock);
        
        var dateBlock = new TextBlock
        {
            Text = trade.Timestamp.ToString("MM/dd HH:mm"),
            Foreground = GetEVEBrush("Secondary"),
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(5)
        };
        Grid.SetColumn(dateBlock, 6);
        grid.Children.Add(dateBlock);
        
        tradeControl.Content = grid;
        return tradeControl;
    }

    #endregion

    #region History Calculations

    private void CalculateHistorySummary()
    {
        var summary = new HoloTradingHistorySummary();
        
        if (TradingHistory != null && TradingHistory.Any())
        {
            var filteredHistory = GetFilteredHistory();
            
            summary.TotalTrades = filteredHistory.Count;
            summary.TotalVolume = filteredHistory.Sum(t => t.TotalValue);
            summary.TotalProfit = CalculateTotalProfit(filteredHistory);
            summary.WinRate = CalculateWinRate(filteredHistory);
            summary.AverageProfitPerTrade = summary.TotalTrades > 0 ? summary.TotalProfit / summary.TotalTrades : 0;
            summary.BestTrade = CalculateBestTrade(filteredHistory);
            summary.WorstTrade = CalculateWorstTrade(filteredHistory);
            summary.TotalFees = filteredHistory.Sum(t => t.Fees + t.Tax);
            summary.AverageTradeSize = summary.TotalTrades > 0 ? summary.TotalVolume / summary.TotalTrades : 0;
            summary.ProfitMargin = summary.TotalVolume > 0 ? (summary.TotalProfit / summary.TotalVolume) * 100 : 0;
        }
        
        HistorySummary = summary;
    }

    private List<HoloTradeRecord> GetFilteredHistory()
    {
        if (TradingHistory == null) return new List<HoloTradeRecord>();
        
        var cutoffDate = DateTime.Now.AddDays(-(int)TimeFrame);
        var filtered = TradingHistory.Where(t => t.Timestamp >= cutoffDate);
        
        if (SelectedCharacter != "All Characters")
        {
            filtered = filtered.Where(t => t.Character == SelectedCharacter);
        }
        
        return filtered.ToList();
    }

    private double CalculateTotalProfit(List<HoloTradeRecord> history)
    {
        var groupedByItem = history.GroupBy(t => new { t.ItemName, t.Character });
        double totalProfit = 0;
        
        foreach (var group in groupedByItem)
        {
            var buyTrades = group.Where(t => t.TradeType == TradeType.Buy).ToList();
            var sellTrades = group.Where(t => t.TradeType == TradeType.Sell).ToList();
            
            var totalBought = buyTrades.Sum(t => t.TotalValue + t.Fees);
            var totalSold = sellTrades.Sum(t => t.TotalValue - t.Fees - t.Tax);
            
            totalProfit += totalSold - totalBought;
        }
        
        return totalProfit;
    }

    private double CalculateWinRate(List<HoloTradeRecord> history)
    {
        var groupedByItem = history.GroupBy(t => new { t.ItemName, t.Character });
        int totalTrades = 0;
        int winningTrades = 0;
        
        foreach (var group in groupedByItem)
        {
            var profit = CalculateGroupProfit(group.ToList());
            totalTrades++;
            if (profit > 0) winningTrades++;
        }
        
        return totalTrades > 0 ? (double)winningTrades / totalTrades * 100 : 0;
    }

    private double CalculateGroupProfit(List<HoloTradeRecord> trades)
    {
        var buyTrades = trades.Where(t => t.TradeType == TradeType.Buy);
        var sellTrades = trades.Where(t => t.TradeType == TradeType.Sell);
        
        var totalBought = buyTrades.Sum(t => t.TotalValue + t.Fees);
        var totalSold = sellTrades.Sum(t => t.TotalValue - t.Fees - t.Tax);
        
        return totalSold - totalBought;
    }

    private double CalculateGroupWinRate(List<HoloTradeRecord> trades)
    {
        var profit = CalculateGroupProfit(trades);
        return profit > 0 ? 100.0 : 0.0;
    }

    private double CalculateBestTrade(List<HoloTradeRecord> history)
    {
        var groupedByItem = history.GroupBy(t => new { t.ItemName, t.Character });
        return groupedByItem.Select(g => CalculateGroupProfit(g.ToList())).DefaultIfEmpty(0).Max();
    }

    private double CalculateWorstTrade(List<HoloTradeRecord> history)
    {
        var groupedByItem = history.GroupBy(t => new { t.ItemName, t.Character });
        return groupedByItem.Select(g => CalculateGroupProfit(g.ToList())).DefaultIfEmpty(0).Min();
    }

    #endregion

    #region Chart Creation

    private void CreateProfitChart(Panel parent)
    {
        var chartPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Width = 300,
            Margin = new Thickness(10),
            Background = CreateHolographicBrush(0.2)
        };
        
        var titleBlock = new TextBlock
        {
            Text = "Profit Over Time",
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary"),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(5)
        };
        chartPanel.Children.Add(titleBlock);
        
        var chartCanvas = new Canvas
        {
            Width = 280,
            Height = 150,
            Background = CreateHolographicBrush(0.1),
            Margin = new Thickness(5)
        };
        
        DrawProfitChart(chartCanvas);
        chartPanel.Children.Add(chartCanvas);
        parent.Children.Add(chartPanel);
    }

    private void CreateVolumeChart(Panel parent)
    {
        var chartPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Width = 300,
            Margin = new Thickness(10),
            Background = CreateHolographicBrush(0.2)
        };
        
        var titleBlock = new TextBlock
        {
            Text = "Volume by Item",
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary"),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(5)
        };
        chartPanel.Children.Add(titleBlock);
        
        var chartCanvas = new Canvas
        {
            Width = 280,
            Height = 150,
            Background = CreateHolographicBrush(0.1),
            Margin = new Thickness(5)
        };
        
        DrawVolumeChart(chartCanvas);
        chartPanel.Children.Add(chartCanvas);
        parent.Children.Add(chartPanel);
    }

    private void CreatePerformanceMetrics(Panel parent)
    {
        var metricsPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Width = 300,
            Margin = new Thickness(10),
            Background = CreateHolographicBrush(0.2)
        };
        
        var titleBlock = new TextBlock
        {
            Text = "Performance Metrics",
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary"),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(5)
        };
        metricsPanel.Children.Add(titleBlock);
        
        var summary = HistorySummary ?? new HoloTradingHistorySummary();
        
        AddMetricDisplay(metricsPanel, "ROI:", $"{summary.ProfitMargin:F2}%");
        AddMetricDisplay(metricsPanel, "Sharpe Ratio:", CalculateSharpeRatio().ToString("F2"));
        AddMetricDisplay(metricsPanel, "Max Drawdown:", CalculateMaxDrawdown().ToString("F2") + "%");
        AddMetricDisplay(metricsPanel, "Consistency:", CalculateConsistency().ToString("F1") + "%");
        
        parent.Children.Add(metricsPanel);
    }

    private void DrawProfitChart(Canvas canvas)
    {
        var history = GetFilteredHistory().OrderBy(t => t.Timestamp).ToList();
        if (history.Count < 2) return;
        
        var cumulativeProfit = 0.0;
        var points = new List<Point>();
        
        foreach (var trade in history)
        {
            if (trade.TradeType == TradeType.Sell)
            {
                var buyTrades = history.Where(t => t.TradeType == TradeType.Buy && 
                                                  t.ItemName == trade.ItemName && 
                                                  t.Timestamp <= trade.Timestamp).ToList();
                if (buyTrades.Any())
                {
                    var avgBuyPrice = buyTrades.Average(t => t.Price);
                    var profit = (trade.Price - avgBuyPrice) * trade.Quantity - trade.Fees - trade.Tax;
                    cumulativeProfit += profit;
                }
            }
            
            var x = (trade.Timestamp - history.First().Timestamp).TotalDays / 
                   (history.Last().Timestamp - history.First().Timestamp).TotalDays * canvas.Width;
            var y = canvas.Height - Math.Abs(cumulativeProfit) / Math.Max(Math.Abs(cumulativeProfit), 1000000) * canvas.Height;
            
            points.Add(new Point(x, y));
        }
        
        for (int i = 1; i < points.Count; i++)
        {
            var line = new Line
            {
                X1 = points[i - 1].X,
                Y1 = points[i - 1].Y,
                X2 = points[i].X,
                Y2 = points[i].Y,
                Stroke = GetEVEBrush(cumulativeProfit >= 0 ? "Success" : "Negative"),
                StrokeThickness = 2,
                Opacity = 0.8
            };
            canvas.Children.Add(line);
        }
    }

    private void DrawVolumeChart(Canvas canvas)
    {
        var history = GetFilteredHistory();
        var itemVolumes = history.GroupBy(t => t.ItemName)
                                .Select(g => new { Item = g.Key, Volume = g.Sum(t => t.TotalValue) })
                                .OrderByDescending(x => x.Volume)
                                .Take(10)
                                .ToList();
        
        if (!itemVolumes.Any()) return;
        
        var maxVolume = itemVolumes.Max(x => x.Volume);
        var barWidth = canvas.Width / itemVolumes.Count;
        
        for (int i = 0; i < itemVolumes.Count; i++)
        {
            var barHeight = (itemVolumes[i].Volume / maxVolume) * canvas.Height * 0.8;
            var bar = new Rectangle
            {
                Width = barWidth - 2,
                Height = barHeight,
                Fill = GetEVEBrush("Info"),
                Opacity = 0.7
            };
            
            Canvas.SetLeft(bar, i * barWidth + 1);
            Canvas.SetBottom(bar, 0);
            canvas.Children.Add(bar);
        }
    }

    private double CalculateSharpeRatio()
    {
        var history = GetFilteredHistory();
        if (history.Count < 5) return 0;
        
        var dailyReturns = new List<double>();
        var groupedByDay = history.GroupBy(t => t.Timestamp.Date);
        
        foreach (var day in groupedByDay)
        {
            var dayProfit = CalculateGroupProfit(day.ToList());
            var dayVolume = day.Sum(t => t.TotalValue);
            if (dayVolume > 0)
            {
                dailyReturns.Add(dayProfit / dayVolume);
            }
        }
        
        if (dailyReturns.Count < 2) return 0;
        
        var avgReturn = dailyReturns.Average();
        var stdDev = Math.Sqrt(dailyReturns.Select(r => Math.Pow(r - avgReturn, 2)).Average());
        
        return stdDev > 0 ? avgReturn / stdDev : 0;
    }

    private double CalculateMaxDrawdown()
    {
        var history = GetFilteredHistory().OrderBy(t => t.Timestamp).ToList();
        if (history.Count < 2) return 0;
        
        var cumulativeProfit = 0.0;
        var peak = 0.0;
        var maxDrawdown = 0.0;
        
        foreach (var trade in history)
        {
            if (trade.TradeType == TradeType.Sell)
            {
                var profit = trade.TotalValue - trade.Fees - trade.Tax;
                cumulativeProfit += profit;
                
                if (cumulativeProfit > peak)
                {
                    peak = cumulativeProfit;
                }
                
                var drawdown = (peak - cumulativeProfit) / Math.Max(peak, 1) * 100;
                maxDrawdown = Math.Max(maxDrawdown, drawdown);
            }
        }
        
        return maxDrawdown;
    }

    private double CalculateConsistency()
    {
        var summary = HistorySummary;
        if (summary == null) return 0;
        
        return Math.Min(summary.WinRate, 100);
    }

    #endregion

    #region UI Helpers

    private void AddHeaderButton(Panel parent, string text, Action clickAction)
    {
        var button = new Button
        {
            Content = text,
            Margin = new Thickness(5),
            Padding = new Thickness(10, 5),
            Background = CreateHolographicBrush(0.3),
            Foreground = GetEVEBrush("Primary"),
            BorderBrush = GetEVEBrush("Accent")
        };
        button.Click += (s, e) => clickAction();
        parent.Children.Add(button);
    }

    private void AddSummaryCard(Panel parent, string label, string value, string colorType = "Secondary")
    {
        var card = new Border
        {
            Width = 150,
            Height = 60,
            Background = CreateHolographicBrush(0.3),
            BorderBrush = GetEVEBrush("Accent"),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(5),
            CornerRadius = new CornerRadius(3)
        };
        
        var panel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        
        var labelBlock = new TextBlock
        {
            Text = label,
            Foreground = GetEVEBrush("Primary"),
            FontSize = 10,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        
        var valueBlock = new TextBlock
        {
            Text = value,
            Foreground = GetEVEBrush(colorType),
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        
        panel.Children.Add(labelBlock);
        panel.Children.Add(valueBlock);
        card.Child = panel;
        parent.Children.Add(card);
    }

    private void AddMetricDisplay(Panel parent, string label, string value)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
        
        var labelElement = new TextBlock
        {
            Text = label,
            Width = 120,
            Foreground = GetEVEBrush("Primary"),
            VerticalAlignment = VerticalAlignment.Center
        };
        
        var valueElement = new TextBlock
        {
            Text = value,
            Width = 80,
            Foreground = GetEVEBrush("Secondary"),
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        
        panel.Children.Add(labelElement);
        panel.Children.Add(valueElement);
        parent.Children.Add(panel);
    }

    private string FormatPrice(double price)
    {
        if (price >= 1_000_000)
            return $"{price / 1_000_000:F2}M";
        if (price >= 1_000)
            return $"{price / 1_000:F2}K";
        return $"{price:F2}";
    }

    private string FormatCurrency(double value)
    {
        if (Math.Abs(value) >= 1_000_000_000)
            return $"{value / 1_000_000_000:F2}B ISK";
        if (Math.Abs(value) >= 1_000_000)
            return $"{value / 1_000_000:F2}M ISK";
        if (Math.Abs(value) >= 1_000)
            return $"{value / 1_000:F2}K ISK";
        return $"{value:F2} ISK";
    }

    private void OnExportClick()
    {
        // Placeholder for export functionality
    }

    #endregion

    #region Animation Methods

    private void AnimateHistoryUpdate()
    {
        if (!EnableAnimations) return;
        
        var storyboard = new Storyboard();
        
        var fadeAnimation = new DoubleAnimation
        {
            From = 0.5,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(400),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        
        Storyboard.SetTarget(fadeAnimation, _scrollViewer);
        Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath("Opacity"));
        storyboard.Children.Add(fadeAnimation);
        
        storyboard.Begin();
    }

    private HoloHistoryParticle CreateHistoryParticle()
    {
        var particle = new HoloHistoryParticle
        {
            Element = new Ellipse
            {
                Width = _random.Next(1, 4),
                Height = _random.Next(1, 4),
                Fill = GetEVEBrush("Accent"),
                Opacity = 0.4
            },
            X = _random.NextDouble() * Width,
            Y = _random.NextDouble() * Height,
            VelocityX = (_random.NextDouble() - 0.5) * 1.2,
            VelocityY = (_random.NextDouble() - 0.5) * 1.2,
            Life = 1.0,
            MaxLife = _random.NextDouble() * 10 + 5
        };
        
        Canvas.SetLeft(particle.Element, particle.X);
        Canvas.SetTop(particle.Element, particle.Y);
        
        return particle;
    }

    private void CreateParticleEffects()
    {
        if (!EnableParticleEffects) return;
        
        for (int i = 0; i < 75; i++)
        {
            var particle = CreateHistoryParticle();
            _historyParticles.Add(particle);
            _historyCanvas.Children.Add(particle.Element);
        }
    }

    #endregion

    #region Visual Helpers

    private Brush CreateHolographicBrush(double opacity)
    {
        var color = GetEVEColor("Background");
        color.A = (byte)(255 * opacity * HolographicIntensity);
        return new SolidColorBrush(color);
    }

    private Brush GetEVEBrush(string type)
    {
        return new SolidColorBrush(GetEVEColor(type));
    }

    private Color GetEVEColor(string type)
    {
        return type switch
        {
            "Primary" => EVEColorScheme switch
            {
                EVEColorScheme.ElectricBlue => Color.FromRgb(0, 150, 255),
                EVEColorScheme.Gold => Color.FromRgb(255, 215, 0),
                EVEColorScheme.Silver => Color.FromRgb(192, 192, 192),
                _ => Color.FromRgb(0, 150, 255)
            },
            "Secondary" => Color.FromRgb(150, 200, 255),
            "Accent" => Color.FromRgb(255, 200, 0),
            "Background" => Color.FromRgb(0, 30, 60),
            "Success" => Color.FromRgb(0, 255, 100),
            "Warning" => Color.FromRgb(255, 165, 0),
            "Negative" => Color.FromRgb(255, 50, 50),
            "Info" => Color.FromRgb(100, 200, 255),
            _ => Color.FromRgb(255, 255, 255)
        };
    }

    #endregion

    #region Timer Events

    private void OnAnimationTick(object sender, EventArgs e)
    {
        UpdateHistoryVisuals();
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        UpdateParticleEffects();
    }

    private void OnUpdateTick(object sender, EventArgs e)
    {
        if (AutoRefresh)
        {
            CalculateHistorySummary();
            CreateHistoryInterface();
        }
    }

    #endregion

    #region Update Methods

    private void UpdateHistoryVisuals()
    {
        if (!EnableAnimations) return;
        
        var time = DateTime.Now.TimeOfDay.TotalSeconds;
        var intensity = HolographicIntensity * (0.85 + 0.15 * Math.Sin(time * 2.5));
        
        foreach (var child in _historyCanvas.Children.OfType<FrameworkElement>())
        {
            if (child.Background is SolidColorBrush brush)
            {
                var color = brush.Color;
                color.A = (byte)(color.A * intensity);
                child.Background = new SolidColorBrush(color);
            }
        }
    }

    private void UpdateParticleEffects()
    {
        if (!EnableParticleEffects) return;
        
        foreach (var particle in _historyParticles.ToList())
        {
            particle.X += particle.VelocityX;
            particle.Y += particle.VelocityY;
            particle.Life -= 0.08;
            
            if (particle.X < 0 || particle.X > Width || particle.Y < 0 || particle.Y > Height || particle.Life <= 0)
            {
                particle.X = _random.NextDouble() * Width;
                particle.Y = _random.NextDouble() * Height;
                particle.Life = particle.MaxLife;
            }
            
            Canvas.SetLeft(particle.Element, particle.X);
            Canvas.SetTop(particle.Element, particle.Y);
            particle.Element.Opacity = particle.Life / particle.MaxLife * 0.4;
        }
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _isInitialized = true;
        
        if (EnableAnimations)
            _animationTimer.Start();
        if (EnableParticleEffects)
            _particleTimer.Start();
        if (AutoRefresh)
            _updateTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        _particleTimer?.Stop();
        _updateTimer?.Stop();
    }

    #endregion

    #region Dependency Property Callbacks

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingHistory history)
        {
            history.UpdateHistoryVisuals();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingHistory history)
        {
            history.CreateHistoryInterface();
        }
    }

    private static void OnTradingHistoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingHistory history)
        {
            history.CalculateHistorySummary();
            history.CreateHistoryInterface();
        }
    }

    private static void OnHistorySummaryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingHistory history)
        {
            history.AnimateHistoryUpdate();
        }
    }

    private static void OnSelectedCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingHistory history)
        {
            history.CalculateHistorySummary();
            history.CreateHistoryInterface();
        }
    }

    private static void OnTimeFrameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingHistory history)
        {
            history.CalculateHistorySummary();
            history.CreateHistoryInterface();
        }
    }

    private static void OnViewModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingHistory history)
        {
            history.CreateHistoryInterface();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingHistory history)
        {
            if ((bool)e.NewValue)
                history._animationTimer?.Start();
            else
                history._animationTimer?.Stop();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingHistory history)
        {
            if ((bool)e.NewValue)
            {
                history.CreateParticleEffects();
                history._particleTimer?.Start();
            }
            else
            {
                history._particleTimer?.Stop();
                history._historyParticles.Clear();
            }
        }
    }

    private static void OnShowPerformanceChartsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingHistory history)
        {
            history.CreateHistoryInterface();
        }
    }

    private static void OnShowTransactionDetailsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingHistory history)
        {
            history.CreateHistoryInterface();
        }
    }

    private static void OnShowProfitLossAnalysisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingHistory history)
        {
            history.CreateHistoryInterface();
        }
    }

    private static void OnAutoRefreshChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingHistory history)
        {
            if ((bool)e.NewValue)
                history._updateTimer?.Start();
            else
                history._updateTimer?.Stop();
        }
    }

    private static void OnUpdateIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTradingHistory history)
        {
            history._updateTimer.Interval = (TimeSpan)e.NewValue;
        }
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void EnableSimplifiedMode()
    {
        EnableAnimations = false;
        EnableParticleEffects = false;
        ShowPerformanceCharts = false;
        ViewMode = HistoryViewMode.Summary;
    }

    public void EnableFullMode()
    {
        EnableAnimations = true;
        EnableParticleEffects = true;
        ShowPerformanceCharts = true;
        ViewMode = HistoryViewMode.Charts;
    }

    #endregion
}

#region Supporting Classes

public class HoloTradeRecord
{
    public string TransactionId { get; set; }
    public string ItemName { get; set; }
    public TradeType TradeType { get; set; }
    public long Quantity { get; set; }
    public double Price { get; set; }
    public double TotalValue { get; set; }
    public string Region { get; set; }
    public string Station { get; set; }
    public string Character { get; set; }
    public DateTime Timestamp { get; set; }
    public double Fees { get; set; }
    public double Tax { get; set; }
}

public class HoloTradingHistorySummary
{
    public int TotalTrades { get; set; }
    public double TotalVolume { get; set; }
    public double TotalProfit { get; set; }
    public double WinRate { get; set; }
    public double AverageProfitPerTrade { get; set; }
    public double BestTrade { get; set; }
    public double WorstTrade { get; set; }
    public double TotalFees { get; set; }
    public double AverageTradeSize { get; set; }
    public double ProfitMargin { get; set; }
}

public class HoloTradeGroupSummary
{
    public string ItemName { get; set; }
    public int TotalTrades { get; set; }
    public double TotalVolume { get; set; }
    public double TotalProfit { get; set; }
    public double WinRate { get; set; }
    public DateTime LastTradeDate { get; set; }
}

public class HoloHistoryParticle
{
    public UIElement Element { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
    public double MaxLife { get; set; }
}

public enum TradeType
{
    Buy,
    Sell
}

public enum HistoryTimeFrame
{
    Day = 1,
    Week = 7,
    Month = 30,
    Quarter = 90,
    Year = 365
}

public enum HistoryViewMode
{
    Summary,
    Detailed,
    Charts
}

#endregion