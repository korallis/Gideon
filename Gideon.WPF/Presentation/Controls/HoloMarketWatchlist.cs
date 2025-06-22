// ==========================================================================
// HoloMarketWatchlist.cs - Holographic Market Watchlist Interface
// ==========================================================================
// Advanced market watchlist featuring real-time price monitoring,
// customizable alerts, EVE-style watchlist management, and holographic market visualization.
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
/// Holographic market watchlist with real-time monitoring and customizable alerts
/// </summary>
public class HoloMarketWatchlist : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloMarketWatchlist),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloMarketWatchlist),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty WatchlistItemsProperty =
        DependencyProperty.Register(nameof(WatchlistItems), typeof(ObservableCollection<HoloWatchlistItem>), typeof(HoloMarketWatchlist),
            new PropertyMetadata(null, OnWatchlistItemsChanged));

    public static readonly DependencyProperty SelectedWatchlistProperty =
        DependencyProperty.Register(nameof(SelectedWatchlist), typeof(string), typeof(HoloMarketWatchlist),
            new PropertyMetadata("Default", OnSelectedWatchlistChanged));

    public static readonly DependencyProperty WatchlistModeProperty =
        DependencyProperty.Register(nameof(WatchlistMode), typeof(WatchlistViewMode), typeof(HoloMarketWatchlist),
            new PropertyMetadata(WatchlistViewMode.Compact, OnWatchlistModeChanged));

    public static readonly DependencyProperty SortModeProperty =
        DependencyProperty.Register(nameof(SortMode), typeof(WatchlistSortMode), typeof(HoloMarketWatchlist),
            new PropertyMetadata(WatchlistSortMode.Alphabetical, OnSortModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloMarketWatchlist),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloMarketWatchlist),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowPriceAlertsProperty =
        DependencyProperty.Register(nameof(ShowPriceAlerts), typeof(bool), typeof(HoloMarketWatchlist),
            new PropertyMetadata(true, OnShowPriceAlertsChanged));

    public static readonly DependencyProperty ShowVolumeAlertsProperty =
        DependencyProperty.Register(nameof(ShowVolumeAlerts), typeof(bool), typeof(HoloMarketWatchlist),
            new PropertyMetadata(true, OnShowVolumeAlertsChanged));

    public static readonly DependencyProperty ShowMarketTrendsProperty =
        DependencyProperty.Register(nameof(ShowMarketTrends), typeof(bool), typeof(HoloMarketWatchlist),
            new PropertyMetadata(true, OnShowMarketTrendsChanged));

    public static readonly DependencyProperty AutoRefreshProperty =
        DependencyProperty.Register(nameof(AutoRefresh), typeof(bool), typeof(HoloMarketWatchlist),
            new PropertyMetadata(true, OnAutoRefreshChanged));

    public static readonly DependencyProperty UpdateIntervalProperty =
        DependencyProperty.Register(nameof(UpdateInterval), typeof(TimeSpan), typeof(HoloMarketWatchlist),
            new PropertyMetadata(TimeSpan.FromSeconds(5), OnUpdateIntervalChanged));

    public static readonly DependencyProperty HighlightChangesProperty =
        DependencyProperty.Register(nameof(HighlightChanges), typeof(bool), typeof(HoloMarketWatchlist),
            new PropertyMetadata(true, OnHighlightChangesChanged));

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

    public ObservableCollection<HoloWatchlistItem> WatchlistItems
    {
        get => (ObservableCollection<HoloWatchlistItem>)GetValue(WatchlistItemsProperty);
        set => SetValue(WatchlistItemsProperty, value);
    }

    public string SelectedWatchlist
    {
        get => (string)GetValue(SelectedWatchlistProperty);
        set => SetValue(SelectedWatchlistProperty, value);
    }

    public WatchlistViewMode WatchlistMode
    {
        get => (WatchlistViewMode)GetValue(WatchlistModeProperty);
        set => SetValue(WatchlistModeProperty, value);
    }

    public WatchlistSortMode SortMode
    {
        get => (WatchlistSortMode)GetValue(SortModeProperty);
        set => SetValue(SortModeProperty, value);
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

    public bool ShowPriceAlerts
    {
        get => (bool)GetValue(ShowPriceAlertsProperty);
        set => SetValue(ShowPriceAlertsProperty, value);
    }

    public bool ShowVolumeAlerts
    {
        get => (bool)GetValue(ShowVolumeAlertsProperty);
        set => SetValue(ShowVolumeAlertsProperty, value);
    }

    public bool ShowMarketTrends
    {
        get => (bool)GetValue(ShowMarketTrendsProperty);
        set => SetValue(ShowMarketTrendsProperty, value);
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

    public bool HighlightChanges
    {
        get => (bool)GetValue(HighlightChangesProperty);
        set => SetValue(HighlightChangesProperty, value);
    }

    #endregion

    #region Fields

    private Canvas _watchlistCanvas;
    private ScrollViewer _scrollViewer;
    private StackPanel _itemsPanel;
    private DispatcherTimer _animationTimer;
    private DispatcherTimer _particleTimer;
    private DispatcherTimer _updateTimer;
    private Dictionary<string, Material> _materialCache;
    private Random _random;
    private List<UIElement> _particles;
    private List<HoloWatchlistParticle> _watchlistParticles;
    private Dictionary<string, DateTime> _changeHighlights;
    private bool _isInitialized;

    #endregion

    #region Constructor

    public HoloMarketWatchlist()
    {
        InitializeComponent();
        InitializeFields();
        InitializeWatchlist();
        InitializeTimers();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 800;
        Height = 600;
        Background = new SolidColorBrush(Color.FromArgb(20, 0, 100, 200));
        
        _watchlistCanvas = new Canvas
        {
            Width = Width,
            Height = Height,
            ClipToBounds = true
        };
        
        Content = _watchlistCanvas;
    }

    private void InitializeFields()
    {
        _materialCache = new Dictionary<string, Material>();
        _random = new Random();
        _particles = new List<UIElement>();
        _watchlistParticles = new List<HoloWatchlistParticle>();
        _changeHighlights = new Dictionary<string, DateTime>();
        _isInitialized = false;
        
        WatchlistItems = new ObservableCollection<HoloWatchlistItem>();
        InitializeSampleWatchlist();
    }

    private void InitializeWatchlist()
    {
        CreateWatchlistInterface();
        UpdateWatchlistVisuals();
        SortWatchlistItems();
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
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _particleTimer.Tick += OnParticleTick;

        _updateTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = UpdateInterval
        };
        _updateTimer.Tick += OnUpdateTick;
    }

    private void InitializeSampleWatchlist()
    {
        var sampleItems = new List<HoloWatchlistItem>
        {
            new() { ItemName = "Tritanium", CurrentPrice = 5.50, PreviousPrice = 5.25, Volume = 45_000_000, Change24h = 4.76, Region = "The Forge", PriceAlert = 6.00, VolumeAlert = 50_000_000, IsWatched = true },
            new() { ItemName = "Pyerite", CurrentPrice = 12.30, PreviousPrice = 12.85, Volume = 25_000_000, Change24h = -4.28, Region = "The Forge", PriceAlert = 15.00, VolumeAlert = 30_000_000, IsWatched = true },
            new() { ItemName = "Mexallon", CurrentPrice = 78.50, PreviousPrice = 75.20, Volume = 8_500_000, Change24h = 4.39, Region = "Domain", PriceAlert = 80.00, VolumeAlert = 10_000_000, IsWatched = true },
            new() { ItemName = "Isogen", CurrentPrice = 125.80, PreviousPrice = 130.45, Volume = 3_200_000, Change24h = -3.56, Region = "The Forge", PriceAlert = 140.00, VolumeAlert = 5_000_000, IsWatched = true },
            new() { ItemName = "Nocxium", CurrentPrice = 845.60, PreviousPrice = 820.35, Volume = 1_800_000, Change24h = 3.08, Region = "Sinq Laison", PriceAlert = 900.00, VolumeAlert = 2_000_000, IsWatched = true },
            new() { ItemName = "Zydrine", CurrentPrice = 1_256.70, PreviousPrice = 1_180.25, Volume = 950_000, Change24h = 6.48, Region = "Metropolis", PriceAlert = 1_300.00, VolumeAlert = 1_000_000, IsWatched = true },
            new() { ItemName = "Megacyte", CurrentPrice = 2_845.30, PreviousPrice = 2_950.75, Volume = 450_000, Change24h = -3.58, Region = "Heimatar", PriceAlert = 3_000.00, VolumeAlert = 500_000, IsWatched = true },
            new() { ItemName = "Morphite", CurrentPrice = 8_750.90, PreviousPrice = 8_500.15, Volume = 125_000, Change24h = 2.95, Region = "The Forge", PriceAlert = 9_000.00, VolumeAlert = 150_000, IsWatched = true },
            new() { ItemName = "PLEX", CurrentPrice = 3_256_890.00, PreviousPrice = 3_180_450.00, Volume = 8_500, Change24h = 2.40, Region = "The Forge", PriceAlert = 3_500_000.00, VolumeAlert = 10_000, IsWatched = true },
            new() { ItemName = "Skill Injector", CurrentPrice = 856_750.00, PreviousPrice = 845_200.00, Volume = 2_850, Change24h = 1.37, Region = "The Forge", PriceAlert = 900_000.00, VolumeAlert = 3_000, IsWatched = true }
        };
        
        foreach (var item in sampleItems)
        {
            WatchlistItems.Add(item);
        }
    }

    #endregion

    #region Watchlist Interface Creation

    private void CreateWatchlistInterface()
    {
        _watchlistCanvas.Children.Clear();
        
        CreateHeaderSection();
        CreateWatchlistDisplay();
        CreateControlsSection();
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
        _watchlistCanvas.Children.Add(headerPanel);
        
        var titleBlock = new TextBlock
        {
            Text = $"Market Watchlist - {SelectedWatchlist}",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary"),
            Margin = new Thickness(10),
            VerticalAlignment = VerticalAlignment.Center
        };
        headerPanel.Children.Add(titleBlock);
        
        AddHeaderButton(headerPanel, "Add Item", OnAddItemClick);
        AddHeaderButton(headerPanel, "Sort", OnSortClick);
        AddHeaderButton(headerPanel, "Alerts", OnAlertsClick);
        AddHeaderButton(headerPanel, "Export", OnExportClick);
    }

    private void CreateWatchlistDisplay()
    {
        _scrollViewer = new ScrollViewer
        {
            Width = Width - 40,
            Height = Height - 120,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = CreateHolographicBrush(0.2)
        };
        
        Canvas.SetLeft(_scrollViewer, 20);
        Canvas.SetTop(_scrollViewer, 60);
        _watchlistCanvas.Children.Add(_scrollViewer);
        
        _itemsPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(5)
        };
        
        _scrollViewer.Content = _itemsPanel;
        
        PopulateWatchlistItems();
    }

    private void CreateControlsSection()
    {
        var controlsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(20, 5, 20, 5),
            Background = CreateHolographicBrush(0.3)
        };
        
        Canvas.SetLeft(controlsPanel, 0);
        Canvas.SetTop(controlsPanel, Height - 50);
        _watchlistCanvas.Children.Add(controlsPanel);
        
        var statusBlock = new TextBlock
        {
            Text = $"{WatchlistItems?.Count ?? 0} items tracked | Last update: {DateTime.Now:HH:mm:ss}",
            Foreground = GetEVEBrush("Secondary"),
            Margin = new Thickness(10),
            VerticalAlignment = VerticalAlignment.Center
        };
        controlsPanel.Children.Add(statusBlock);
        
        var refreshButton = new Button
        {
            Content = "Refresh",
            Margin = new Thickness(5),
            Background = CreateHolographicBrush(0.3),
            Foreground = GetEVEBrush("Primary"),
            BorderBrush = GetEVEBrush("Accent")
        };
        refreshButton.Click += OnRefreshClick;
        controlsPanel.Children.Add(refreshButton);
    }

    private void PopulateWatchlistItems()
    {
        if (_itemsPanel == null || WatchlistItems == null) return;
        
        _itemsPanel.Children.Clear();
        
        var sortedItems = GetSortedWatchlistItems();
        
        foreach (var item in sortedItems)
        {
            var itemControl = CreateWatchlistItemControl(item);
            _itemsPanel.Children.Add(itemControl);
        }
    }

    private UserControl CreateWatchlistItemControl(HoloWatchlistItem item)
    {
        var itemControl = new UserControl
        {
            Margin = new Thickness(2),
            Background = CreateHolographicBrush(0.25)
        };
        
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        
        var nameBlock = new TextBlock
        {
            Text = item.ItemName,
            Foreground = GetEVEBrush("Primary"),
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5)
        };
        Grid.SetColumn(nameBlock, 0);
        grid.Children.Add(nameBlock);
        
        var priceBlock = new TextBlock
        {
            Text = FormatPrice(item.CurrentPrice),
            Foreground = GetEVEBrush("Secondary"),
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Right,
            Margin = new Thickness(5)
        };
        Grid.SetColumn(priceBlock, 1);
        grid.Children.Add(priceBlock);
        
        var changeColor = item.Change24h >= 0 ? GetEVEBrush("Success") : GetEVEBrush("Negative");
        var changeBlock = new TextBlock
        {
            Text = $"{item.Change24h:+0.00;-0.00;0.00}%",
            Foreground = changeColor,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Right,
            Margin = new Thickness(5),
            FontWeight = FontWeights.Bold
        };
        Grid.SetColumn(changeBlock, 2);
        grid.Children.Add(changeBlock);
        
        var volumeBlock = new TextBlock
        {
            Text = FormatVolume(item.Volume),
            Foreground = GetEVEBrush("Info"),
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Right,
            Margin = new Thickness(5)
        };
        Grid.SetColumn(volumeBlock, 3);
        grid.Children.Add(volumeBlock);
        
        var regionBlock = new TextBlock
        {
            Text = item.Region,
            Foreground = GetEVEBrush("Secondary"),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5)
        };
        Grid.SetColumn(regionBlock, 4);
        grid.Children.Add(regionBlock);
        
        var alertPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5)
        };
        
        if (ShowPriceAlerts && ShouldShowPriceAlert(item))
        {
            var priceAlertIcon = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = GetEVEBrush("Warning"),
                Margin = new Thickness(2)
            };
            alertPanel.Children.Add(priceAlertIcon);
        }
        
        if (ShowVolumeAlerts && ShouldShowVolumeAlert(item))
        {
            var volumeAlertIcon = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = GetEVEBrush("Info"),
                Margin = new Thickness(2)
            };
            alertPanel.Children.Add(volumeAlertIcon);
        }
        
        Grid.SetColumn(alertPanel, 5);
        grid.Children.Add(alertPanel);
        
        var trendIndicator = CreateTrendIndicator(item);
        Grid.SetColumn(trendIndicator, 6);
        grid.Children.Add(trendIndicator);
        
        itemControl.Content = grid;
        
        if (HighlightChanges && HasRecentChange(item))
        {
            ApplyChangeHighlight(itemControl, item);
        }
        
        return itemControl;
    }

    private UIElement CreateTrendIndicator(HoloWatchlistItem item)
    {
        if (!ShowMarketTrends) return new UIElement();
        
        var canvas = new Canvas
        {
            Width = 60,
            Height = 20,
            Margin = new Thickness(5)
        };
        
        var trendColor = item.Change24h >= 0 ? GetEVEColor("Success") : GetEVEColor("Negative");
        
        for (int i = 0; i < 10; i++)
        {
            var height = Math.Abs(item.Change24h) * 2 + _random.NextDouble() * 5;
            height = Math.Min(height, 15);
            
            var bar = new Rectangle
            {
                Width = 4,
                Height = height,
                Fill = new SolidColorBrush(trendColor),
                Opacity = 0.7
            };
            
            Canvas.SetLeft(bar, i * 6);
            Canvas.SetBottom(bar, 0);
            canvas.Children.Add(bar);
        }
        
        return canvas;
    }

    private void CreateParticleEffects()
    {
        if (!EnableParticleEffects) return;
        
        for (int i = 0; i < 50; i++)
        {
            var particle = CreateWatchlistParticle();
            _watchlistParticles.Add(particle);
            _watchlistCanvas.Children.Add(particle.Element);
        }
    }

    #endregion

    #region Watchlist Management

    private List<HoloWatchlistItem> GetSortedWatchlistItems()
    {
        if (WatchlistItems == null) return new List<HoloWatchlistItem>();
        
        return SortMode switch
        {
            WatchlistSortMode.Alphabetical => WatchlistItems.OrderBy(i => i.ItemName).ToList(),
            WatchlistSortMode.Price => WatchlistItems.OrderByDescending(i => i.CurrentPrice).ToList(),
            WatchlistSortMode.Change => WatchlistItems.OrderByDescending(i => i.Change24h).ToList(),
            WatchlistSortMode.Volume => WatchlistItems.OrderByDescending(i => i.Volume).ToList(),
            WatchlistSortMode.Region => WatchlistItems.OrderBy(i => i.Region).ThenBy(i => i.ItemName).ToList(),
            _ => WatchlistItems.ToList()
        };
    }

    private void SortWatchlistItems()
    {
        PopulateWatchlistItems();
    }

    private bool ShouldShowPriceAlert(HoloWatchlistItem item)
    {
        return item.PriceAlert > 0 && 
               (item.CurrentPrice >= item.PriceAlert || 
                item.CurrentPrice <= item.PriceAlert * 0.9);
    }

    private bool ShouldShowVolumeAlert(HoloWatchlistItem item)
    {
        return item.VolumeAlert > 0 && item.Volume >= item.VolumeAlert;
    }

    private bool HasRecentChange(HoloWatchlistItem item)
    {
        return _changeHighlights.ContainsKey(item.ItemName) &&
               (DateTime.Now - _changeHighlights[item.ItemName]).TotalSeconds < 10;
    }

    private void ApplyChangeHighlight(UserControl control, HoloWatchlistItem item)
    {
        var highlightBrush = item.Change24h >= 0 ? 
            new SolidColorBrush(Color.FromArgb(100, 0, 255, 100)) :
            new SolidColorBrush(Color.FromArgb(100, 255, 50, 50));
        
        control.Background = highlightBrush;
        
        if (EnableAnimations)
        {
            var storyboard = new Storyboard();
            var animation = new ColorAnimation
            {
                To = GetEVEColor("Background"),
                Duration = TimeSpan.FromSeconds(3),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            
            Storyboard.SetTarget(animation, control.Background);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Color"));
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }
    }

    private void UpdateMarketData()
    {
        if (WatchlistItems == null) return;
        
        foreach (var item in WatchlistItems)
        {
            var oldPrice = item.CurrentPrice;
            var priceChange = (_random.NextDouble() - 0.5) * 0.1;
            item.PreviousPrice = oldPrice;
            item.CurrentPrice = Math.Max(0.01, oldPrice * (1 + priceChange));
            
            var volumeChange = (_random.NextDouble() - 0.5) * 0.2;
            item.Volume = Math.Max(1, (long)(item.Volume * (1 + volumeChange)));
            
            item.Change24h = ((item.CurrentPrice - item.PreviousPrice) / item.PreviousPrice) * 100;
            
            if (Math.Abs(oldPrice - item.CurrentPrice) > oldPrice * 0.01)
            {
                _changeHighlights[item.ItemName] = DateTime.Now;
            }
        }
        
        PopulateWatchlistItems();
    }

    #endregion

    #region UI Helpers

    private void AddHeaderButton(Panel parent, string text, RoutedEventHandler clickHandler)
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
        button.Click += clickHandler;
        parent.Children.Add(button);
    }

    private string FormatPrice(double price)
    {
        if (price >= 1_000_000)
            return $"{price / 1_000_000:F2}M";
        if (price >= 1_000)
            return $"{price / 1_000:F2}K";
        return $"{price:F2}";
    }

    private string FormatVolume(long volume)
    {
        if (volume >= 1_000_000_000)
            return $"{volume / 1_000_000_000.0:F1}B";
        if (volume >= 1_000_000)
            return $"{volume / 1_000_000.0:F1}M";
        if (volume >= 1_000)
            return $"{volume / 1_000.0:F1}K";
        return volume.ToString();
    }

    #endregion

    #region Animation Methods

    private void AnimateWatchlistUpdate()
    {
        if (!EnableAnimations) return;
        
        var storyboard = new Storyboard();
        
        var opacityAnimation = new DoubleAnimation
        {
            From = 0.7,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        
        Storyboard.SetTarget(opacityAnimation, _scrollViewer);
        Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));
        storyboard.Children.Add(opacityAnimation);
        
        storyboard.Begin();
    }

    private HoloWatchlistParticle CreateWatchlistParticle()
    {
        var particle = new HoloWatchlistParticle
        {
            Element = new Ellipse
            {
                Width = _random.Next(1, 3),
                Height = _random.Next(1, 3),
                Fill = GetEVEBrush("Accent"),
                Opacity = 0.5
            },
            X = _random.NextDouble() * Width,
            Y = _random.NextDouble() * Height,
            VelocityX = (_random.NextDouble() - 0.5) * 1,
            VelocityY = (_random.NextDouble() - 0.5) * 1,
            Life = 1.0,
            MaxLife = _random.NextDouble() * 6 + 3
        };
        
        Canvas.SetLeft(particle.Element, particle.X);
        Canvas.SetTop(particle.Element, particle.Y);
        
        return particle;
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

    #region Event Handlers

    private void OnAddItemClick(object sender, RoutedEventArgs e)
    {
        // Placeholder for add item functionality
    }

    private void OnSortClick(object sender, RoutedEventArgs e)
    {
        var nextMode = (WatchlistSortMode)(((int)SortMode + 1) % Enum.GetValues<WatchlistSortMode>().Length);
        SortMode = nextMode;
    }

    private void OnAlertsClick(object sender, RoutedEventArgs e)
    {
        // Placeholder for alerts configuration
    }

    private void OnExportClick(object sender, RoutedEventArgs e)
    {
        // Placeholder for export functionality
    }

    private void OnRefreshClick(object sender, RoutedEventArgs e)
    {
        UpdateMarketData();
        AnimateWatchlistUpdate();
    }

    #endregion

    #region Timer Events

    private void OnAnimationTick(object sender, EventArgs e)
    {
        UpdateWatchlistVisuals();
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        UpdateParticleEffects();
    }

    private void OnUpdateTick(object sender, EventArgs e)
    {
        if (AutoRefresh)
        {
            UpdateMarketData();
        }
    }

    #endregion

    #region Update Methods

    private void UpdateWatchlistVisuals()
    {
        if (!EnableAnimations) return;
        
        var time = DateTime.Now.TimeOfDay.TotalSeconds;
        var intensity = HolographicIntensity * (0.9 + 0.1 * Math.Sin(time * 3));
        
        foreach (var child in _watchlistCanvas.Children.OfType<FrameworkElement>())
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
        
        foreach (var particle in _watchlistParticles.ToList())
        {
            particle.X += particle.VelocityX;
            particle.Y += particle.VelocityY;
            particle.Life -= 0.1;
            
            if (particle.X < 0 || particle.X > Width || particle.Y < 0 || particle.Y > Height || particle.Life <= 0)
            {
                particle.X = _random.NextDouble() * Width;
                particle.Y = _random.NextDouble() * Height;
                particle.Life = particle.MaxLife;
            }
            
            Canvas.SetLeft(particle.Element, particle.X);
            Canvas.SetTop(particle.Element, particle.Y);
            particle.Element.Opacity = particle.Life / particle.MaxLife * 0.5;
        }
    }

    #endregion

    #region Loaded/Unloaded Events

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
        if (d is HoloMarketWatchlist watchlist)
        {
            watchlist.UpdateWatchlistVisuals();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketWatchlist watchlist)
        {
            watchlist.CreateWatchlistInterface();
        }
    }

    private static void OnWatchlistItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketWatchlist watchlist)
        {
            watchlist.PopulateWatchlistItems();
        }
    }

    private static void OnSelectedWatchlistChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketWatchlist watchlist)
        {
            watchlist.CreateWatchlistInterface();
        }
    }

    private static void OnWatchlistModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketWatchlist watchlist)
        {
            watchlist.CreateWatchlistInterface();
        }
    }

    private static void OnSortModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketWatchlist watchlist)
        {
            watchlist.SortWatchlistItems();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketWatchlist watchlist)
        {
            if ((bool)e.NewValue)
                watchlist._animationTimer?.Start();
            else
                watchlist._animationTimer?.Stop();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketWatchlist watchlist)
        {
            if ((bool)e.NewValue)
            {
                watchlist.CreateParticleEffects();
                watchlist._particleTimer?.Start();
            }
            else
            {
                watchlist._particleTimer?.Stop();
                watchlist._watchlistParticles.Clear();
            }
        }
    }

    private static void OnShowPriceAlertsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketWatchlist watchlist)
        {
            watchlist.PopulateWatchlistItems();
        }
    }

    private static void OnShowVolumeAlertsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketWatchlist watchlist)
        {
            watchlist.PopulateWatchlistItems();
        }
    }

    private static void OnShowMarketTrendsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketWatchlist watchlist)
        {
            watchlist.PopulateWatchlistItems();
        }
    }

    private static void OnAutoRefreshChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketWatchlist watchlist)
        {
            if ((bool)e.NewValue)
                watchlist._updateTimer?.Start();
            else
                watchlist._updateTimer?.Stop();
        }
    }

    private static void OnUpdateIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketWatchlist watchlist)
        {
            watchlist._updateTimer.Interval = (TimeSpan)e.NewValue;
        }
    }

    private static void OnHighlightChangesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketWatchlist watchlist)
        {
            watchlist.PopulateWatchlistItems();
        }
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void EnableSimplifiedMode()
    {
        EnableAnimations = false;
        EnableParticleEffects = false;
        ShowMarketTrends = false;
        HighlightChanges = false;
    }

    public void EnableFullMode()
    {
        EnableAnimations = true;
        EnableParticleEffects = true;
        ShowMarketTrends = true;
        HighlightChanges = true;
    }

    #endregion
}

#region Supporting Classes

public class HoloWatchlistItem
{
    public string ItemName { get; set; }
    public double CurrentPrice { get; set; }
    public double PreviousPrice { get; set; }
    public long Volume { get; set; }
    public double Change24h { get; set; }
    public string Region { get; set; }
    public double PriceAlert { get; set; }
    public long VolumeAlert { get; set; }
    public bool IsWatched { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.Now;
}

public class HoloWatchlistParticle
{
    public UIElement Element { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
    public double MaxLife { get; set; }
}

public enum WatchlistViewMode
{
    Compact,
    Detailed,
    Chart
}

public enum WatchlistSortMode
{
    Alphabetical,
    Price,
    Change,
    Volume,
    Region
}

#endregion