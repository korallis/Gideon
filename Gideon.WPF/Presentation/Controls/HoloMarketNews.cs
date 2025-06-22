// ==========================================================================
// HoloMarketNews.cs - Holographic Market News and Events Display
// ==========================================================================
// Advanced market news system featuring real-time event monitoring,
// impact analysis, EVE-style news aggregation, and holographic news visualization.
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
/// Holographic market news with real-time event monitoring and market impact analysis
/// </summary>
public class HoloMarketNews : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloMarketNews),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloMarketNews),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty NewsItemsProperty =
        DependencyProperty.Register(nameof(NewsItems), typeof(ObservableCollection<HoloMarketNewsItem>), typeof(HoloMarketNews),
            new PropertyMetadata(null, OnNewsItemsChanged));

    public static readonly DependencyProperty MarketEventsProperty =
        DependencyProperty.Register(nameof(MarketEvents), typeof(ObservableCollection<HoloMarketEvent>), typeof(HoloMarketNews),
            new PropertyMetadata(null, OnMarketEventsChanged));

    public static readonly DependencyProperty NewsCategoryProperty =
        DependencyProperty.Register(nameof(NewsCategory), typeof(NewsCategory), typeof(HoloMarketNews),
            new PropertyMetadata(NewsCategory.All, OnNewsCategoryChanged));

    public static readonly DependencyProperty EventSeverityProperty =
        DependencyProperty.Register(nameof(EventSeverity), typeof(EventSeverity), typeof(HoloMarketNews),
            new PropertyMetadata(EventSeverity.All, OnEventSeverityChanged));

    public static readonly DependencyProperty ShowMarketImpactProperty =
        DependencyProperty.Register(nameof(ShowMarketImpact), typeof(bool), typeof(HoloMarketNews),
            new PropertyMetadata(true, OnShowMarketImpactChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloMarketNews),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloMarketNews),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowTrendingTopicsProperty =
        DependencyProperty.Register(nameof(ShowTrendingTopics), typeof(bool), typeof(HoloMarketNews),
            new PropertyMetadata(true, OnShowTrendingTopicsChanged));

    public static readonly DependencyProperty ShowEventTimelineProperty =
        DependencyProperty.Register(nameof(ShowEventTimeline), typeof(bool), typeof(HoloMarketNews),
            new PropertyMetadata(true, OnShowEventTimelineChanged));

    public static readonly DependencyProperty AutoRefreshProperty =
        DependencyProperty.Register(nameof(AutoRefresh), typeof(bool), typeof(HoloMarketNews),
            new PropertyMetadata(true, OnAutoRefreshChanged));

    public static readonly DependencyProperty RefreshIntervalProperty =
        DependencyProperty.Register(nameof(RefreshInterval), typeof(TimeSpan), typeof(HoloMarketNews),
            new PropertyMetadata(TimeSpan.FromMinutes(2), OnRefreshIntervalChanged));

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

    public ObservableCollection<HoloMarketNewsItem> NewsItems
    {
        get => (ObservableCollection<HoloMarketNewsItem>)GetValue(NewsItemsProperty);
        set => SetValue(NewsItemsProperty, value);
    }

    public ObservableCollection<HoloMarketEvent> MarketEvents
    {
        get => (ObservableCollection<HoloMarketEvent>)GetValue(MarketEventsProperty);
        set => SetValue(MarketEventsProperty, value);
    }

    public NewsCategory NewsCategory
    {
        get => (NewsCategory)GetValue(NewsCategoryProperty);
        set => SetValue(NewsCategoryProperty, value);
    }

    public EventSeverity EventSeverity
    {
        get => (EventSeverity)GetValue(EventSeverityProperty);
        set => SetValue(EventSeverityProperty, value);
    }

    public bool ShowMarketImpact
    {
        get => (bool)GetValue(ShowMarketImpactProperty);
        set => SetValue(ShowMarketImpactProperty, value);
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

    public bool ShowTrendingTopics
    {
        get => (bool)GetValue(ShowTrendingTopicsProperty);
        set => SetValue(ShowTrendingTopicsProperty, value);
    }

    public bool ShowEventTimeline
    {
        get => (bool)GetValue(ShowEventTimelineProperty);
        set => SetValue(ShowEventTimelineProperty, value);
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

    private Canvas _newsCanvas;
    private ScrollViewer _newsScrollViewer;
    private ScrollViewer _eventsScrollViewer;
    private StackPanel _newsPanel;
    private StackPanel _eventsPanel;
    private DispatcherTimer _animationTimer;
    private DispatcherTimer _particleTimer;
    private DispatcherTimer _refreshTimer;
    private Dictionary<string, Material> _materialCache;
    private Random _random;
    private List<UIElement> _particles;
    private List<HoloNewsParticle> _newsParticles;
    private bool _isInitialized;

    #endregion

    #region Constructor

    public HoloMarketNews()
    {
        InitializeComponent();
        InitializeFields();
        InitializeNews();
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
        
        _newsCanvas = new Canvas
        {
            Width = Width,
            Height = Height,
            ClipToBounds = true
        };
        
        Content = _newsCanvas;
    }

    private void InitializeFields()
    {
        _materialCache = new Dictionary<string, Material>();
        _random = new Random();
        _particles = new List<UIElement>();
        _newsParticles = new List<HoloNewsParticle>();
        _isInitialized = false;
        
        NewsItems = new ObservableCollection<HoloMarketNewsItem>();
        MarketEvents = new ObservableCollection<HoloMarketEvent>();
        GenerateSampleNews();
    }

    private void InitializeNews()
    {
        CreateNewsInterface();
        UpdateNewsVisuals();
        RefreshNewsData();
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

        _refreshTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = RefreshInterval
        };
        _refreshTimer.Tick += OnRefreshTick;
    }

    private void GenerateSampleNews()
    {
        var newsTemplates = new[]
        {
            ("CCP announces balance changes to mining ships", NewsCategory.GameUpdates, EventSeverity.Medium, 0.05),
            ("Major alliance declares war in nullsec", NewsCategory.Politics, EventSeverity.High, 0.12),
            ("Tritanium prices surge 15% following supply disruption", NewsCategory.MarketNews, EventSeverity.High, 0.15),
            ("New player corporation recruitment drive successful", NewsCategory.Community, EventSeverity.Low, 0.02),
            ("Faction warfare mechanics update deployed", NewsCategory.GameUpdates, EventSeverity.Medium, 0.08),
            ("PLEX market manipulation investigation ongoing", NewsCategory.MarketNews, EventSeverity.High, 0.20),
            ("Exploration site discovery rate increased", NewsCategory.GameUpdates, EventSeverity.Low, 0.03),
            ("Major trading corporation reports record profits", NewsCategory.Economics, EventSeverity.Medium, 0.07),
            ("Manufacturing costs affected by resource scarcity", NewsCategory.Economics, EventSeverity.Medium, 0.09),
            ("Security status changes in contested systems", NewsCategory.Politics, EventSeverity.Medium, 0.06)
        };
        
        for (int i = 0; i < 15; i++)
        {
            var template = newsTemplates[_random.Next(newsTemplates.Length)];
            var newsItem = new HoloMarketNewsItem
            {
                Id = Guid.NewGuid().ToString(),
                Title = template.Item1,
                Summary = GenerateNewsSummary(template.Item1),
                Category = template.Item2,
                Severity = template.Item3,
                MarketImpact = template.Item4,
                Timestamp = DateTime.Now.AddMinutes(-_random.Next(1, 120)),
                Source = "EVE Market Intelligence",
                Tags = GenerateNewsTags(template.Item2),
                IsBreaking = template.Item3 == EventSeverity.High && _random.NextDouble() < 0.3
            };
            
            NewsItems.Add(newsItem);
        }
        
        var eventTemplates = new[]
        {
            ("Mineral market volatility detected", EventSeverity.Medium, "Large fluctuations in mineral prices"),
            ("Major corporation liquidation event", EventSeverity.High, "Massive asset sell-off in progress"),
            ("Regional trade route disruption", EventSeverity.Medium, "Security issues affecting transport"),
            ("New industrial capacity online", EventSeverity.Low, "Manufacturing output increased"),
            ("Market manipulation alert", EventSeverity.High, "Suspicious trading patterns detected")
        };
        
        for (int i = 0; i < 10; i++)
        {
            var template = eventTemplates[_random.Next(eventTemplates.Length)];
            var marketEvent = new HoloMarketEvent
            {
                Id = Guid.NewGuid().ToString(),
                Title = template.Item1,
                Description = template.Item3,
                Severity = template.Item2,
                Timestamp = DateTime.Now.AddMinutes(-_random.Next(1, 180)),
                AffectedItems = GenerateAffectedItems(),
                EstimatedImpact = _random.NextDouble() * 0.25,
                Duration = TimeSpan.FromHours(_random.Next(1, 48)),
                IsActive = _random.NextDouble() < 0.7
            };
            
            MarketEvents.Add(marketEvent);
        }
    }

    private string GenerateNewsSummary(string title)
    {
        var summaries = new[]
        {
            "Market analysts report significant shifts in trading patterns across New Eden...",
            "EVE Online's dynamic economy continues to evolve with new developments...",
            "Corporate leaders express concerns about recent market volatility...",
            "Independent traders capitalize on emerging opportunities...",
            "Regional markets show divergent trends following recent events..."
        };
        
        return summaries[_random.Next(summaries.Length)];
    }

    private List<string> GenerateNewsTags(NewsCategory category)
    {
        var tagSets = new Dictionary<NewsCategory, string[]>
        {
            [NewsCategory.MarketNews] = new[] { "Trading", "Prices", "Volume", "Economy" },
            [NewsCategory.GameUpdates] = new[] { "CCP", "Patch", "Balance", "Features" },
            [NewsCategory.Politics] = new[] { "Alliance", "War", "Diplomacy", "Territory" },
            [NewsCategory.Economics] = new[] { "Industry", "Production", "Resources", "Investment" },
            [NewsCategory.Community] = new[] { "Players", "Events", "Recruitment", "Social" }
        };
        
        if (tagSets.TryGetValue(category, out var tags))
        {
            return tags.Take(_random.Next(2, 4)).ToList();
        }
        
        return new List<string> { "General" };
    }

    private List<string> GenerateAffectedItems()
    {
        var items = new[] { "Tritanium", "Pyerite", "Mexallon", "Isogen", "Nocxium", "PLEX", "Ships", "Modules" };
        return items.Take(_random.Next(1, 4)).ToList();
    }

    #endregion

    #region News Interface Creation

    private void CreateNewsInterface()
    {
        _newsCanvas.Children.Clear();
        
        CreateHeaderSection();
        CreateFiltersSection();
        CreateNewsSection();
        CreateEventsSection();
        CreateTrendingSection();
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
        _newsCanvas.Children.Add(headerPanel);
        
        var titleBlock = new TextBlock
        {
            Text = "Market Intelligence Feed",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary"),
            Margin = new Thickness(10),
            VerticalAlignment = VerticalAlignment.Center
        };
        headerPanel.Children.Add(titleBlock);
        
        var liveIndicator = new Ellipse
        {
            Width = 12,
            Height = 12,
            Fill = GetEVEBrush("Success"),
            Margin = new Thickness(10, 0, 0, 0)
        };
        headerPanel.Children.Add(liveIndicator);
        
        var liveText = new TextBlock
        {
            Text = "LIVE",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Success"),
            Margin = new Thickness(5, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        headerPanel.Children.Add(liveText);
        
        AddHeaderButton(headerPanel, "All News", () => NewsCategory = NewsCategory.All);
        AddHeaderButton(headerPanel, "Market", () => NewsCategory = NewsCategory.MarketNews);
        AddHeaderButton(headerPanel, "Updates", () => NewsCategory = NewsCategory.GameUpdates);
        AddHeaderButton(headerPanel, "Refresh", RefreshNewsData);
    }

    private void CreateFiltersSection()
    {
        var filtersPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(20, 5, 20, 5),
            Background = CreateHolographicBrush(0.3)
        };
        
        Canvas.SetLeft(filtersPanel, 0);
        Canvas.SetTop(filtersPanel, 60);
        _newsCanvas.Children.Add(filtersPanel);
        
        AddFilterCard(filtersPanel, "Category", NewsCategory.ToString());
        AddFilterCard(filtersPanel, "Severity", EventSeverity.ToString());
        AddFilterCard(filtersPanel, "News Items", GetFilteredNews().Count().ToString());
        AddFilterCard(filtersPanel, "Active Events", MarketEvents?.Count(e => e.IsActive).ToString() ?? "0");
        AddFilterCard(filtersPanel, "Breaking News", NewsItems?.Count(n => n.IsBreaking).ToString() ?? "0");
        AddFilterCard(filtersPanel, "Last Update", DateTime.Now.ToString("HH:mm:ss"));
    }

    private void CreateNewsSection()
    {
        var newsContainer = new Border
        {
            Width = 480,
            Height = 350,
            Background = CreateHolographicBrush(0.2),
            BorderBrush = GetEVEBrush("Accent"),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(20, 10, 10, 10)
        };
        
        Canvas.SetLeft(newsContainer, 0);
        Canvas.SetTop(newsContainer, 120);
        _newsCanvas.Children.Add(newsContainer);
        
        var newsHeader = new TextBlock
        {
            Text = "Market News",
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary"),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(10, 5, 10, 5)
        };
        
        var newsStack = new StackPanel();
        newsStack.Children.Add(newsHeader);
        
        _newsScrollViewer = new ScrollViewer
        {
            Height = 315,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };
        
        _newsPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(5)
        };
        
        _newsScrollViewer.Content = _newsPanel;
        newsStack.Children.Add(_newsScrollViewer);
        newsContainer.Child = newsStack;
        
        PopulateNewsContent();
    }

    private void CreateEventsSection()
    {
        var eventsContainer = new Border
        {
            Width = 480,
            Height = 350,
            Background = CreateHolographicBrush(0.2),
            BorderBrush = GetEVEBrush("Warning"),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(10, 10, 20, 10)
        };
        
        Canvas.SetLeft(eventsContainer, 500);
        Canvas.SetTop(eventsContainer, 120);
        _newsCanvas.Children.Add(eventsContainer);
        
        var eventsHeader = new TextBlock
        {
            Text = "Market Events",
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary"),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(10, 5, 10, 5)
        };
        
        var eventsStack = new StackPanel();
        eventsStack.Children.Add(eventsHeader);
        
        _eventsScrollViewer = new ScrollViewer
        {
            Height = 315,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };
        
        _eventsPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(5)
        };
        
        _eventsScrollViewer.Content = _eventsPanel;
        eventsStack.Children.Add(_eventsScrollViewer);
        eventsContainer.Child = eventsStack;
        
        PopulateEventsContent();
    }

    private void CreateTrendingSection()
    {
        if (!ShowTrendingTopics) return;
        
        var trendingPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(20),
            Background = CreateHolographicBrush(0.25)
        };
        
        Canvas.SetLeft(trendingPanel, 20);
        Canvas.SetTop(trendingPanel, 490);
        _newsCanvas.Children.Add(trendingPanel);
        
        CreateTrendingTopics(trendingPanel);
        CreateMarketSentiment(trendingPanel);
        CreateImpactAnalysis(trendingPanel);
    }

    private void PopulateNewsContent()
    {
        if (_newsPanel == null) return;
        
        _newsPanel.Children.Clear();
        
        var filteredNews = GetFilteredNews().OrderByDescending(n => n.Timestamp).Take(20);
        
        foreach (var newsItem in filteredNews)
        {
            var newsControl = CreateNewsItemControl(newsItem);
            _newsPanel.Children.Add(newsControl);
        }
    }

    private void PopulateEventsContent()
    {
        if (_eventsPanel == null) return;
        
        _eventsPanel.Children.Clear();
        
        var filteredEvents = GetFilteredEvents().OrderByDescending(e => e.Timestamp).Take(15);
        
        foreach (var marketEvent in filteredEvents)
        {
            var eventControl = CreateEventControl(marketEvent);
            _eventsPanel.Children.Add(eventControl);
        }
    }

    private UserControl CreateNewsItemControl(HoloMarketNewsItem newsItem)
    {
        var newsControl = new UserControl
        {
            Margin = new Thickness(2),
            Background = CreateHolographicBrush(newsItem.IsBreaking ? 0.4 : 0.25)
        };
        
        if (newsItem.IsBreaking)
        {
            newsControl.BorderBrush = GetEVEBrush("Negative");
            newsControl.BorderThickness = new Thickness(1);
        }
        
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(8)
        };
        
        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 3)
        };
        
        if (newsItem.IsBreaking)
        {
            var breakingTag = new Border
            {
                Background = GetEVEBrush("Negative"),
                CornerRadius = new CornerRadius(2),
                Padding = new Thickness(4, 1),
                Margin = new Thickness(0, 0, 5, 0)
            };
            
            var breakingText = new TextBlock
            {
                Text = "BREAKING",
                Foreground = Brushes.White,
                FontSize = 8,
                FontWeight = FontWeights.Bold
            };
            
            breakingTag.Child = breakingText;
            headerPanel.Children.Add(breakingTag);
        }
        
        var categoryColor = newsItem.Category switch
        {
            NewsCategory.MarketNews => GetEVEBrush("Success"),
            NewsCategory.GameUpdates => GetEVEBrush("Info"),
            NewsCategory.Politics => GetEVEBrush("Warning"),
            NewsCategory.Economics => GetEVEBrush("Secondary"),
            NewsCategory.Community => GetEVEBrush("Primary"),
            _ => GetEVEBrush("Secondary")
        };
        
        var categoryBlock = new TextBlock
        {
            Text = newsItem.Category.ToString().ToUpper(),
            Foreground = categoryColor,
            FontSize = 8,
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center
        };
        headerPanel.Children.Add(categoryBlock);
        
        var timeBlock = new TextBlock
        {
            Text = FormatTimeAgo(newsItem.Timestamp),
            Foreground = GetEVEBrush("Secondary"),
            FontSize = 8,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(10, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        headerPanel.Children.Add(timeBlock);
        
        var titleBlock = new TextBlock
        {
            Text = newsItem.Title,
            Foreground = GetEVEBrush("Primary"),
            FontWeight = FontWeights.Bold,
            FontSize = 11,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 2, 0, 2)
        };
        
        var summaryBlock = new TextBlock
        {
            Text = newsItem.Summary,
            Foreground = GetEVEBrush("Secondary"),
            FontSize = 9,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 3)
        };
        
        if (ShowMarketImpact && newsItem.MarketImpact > 0.05)
        {
            var impactPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 2, 0, 0)
            };
            
            var impactLabel = new TextBlock
            {
                Text = "Market Impact:",
                Foreground = GetEVEBrush("Warning"),
                FontSize = 8,
                FontWeight = FontWeights.Bold
            };
            
            var impactValue = new TextBlock
            {
                Text = $"{newsItem.MarketImpact:P1}",
                Foreground = GetEVEBrush("Warning"),
                FontSize = 8,
                Margin = new Thickness(5, 0, 0, 0)
            };
            
            impactPanel.Children.Add(impactLabel);
            impactPanel.Children.Add(impactValue);
            panel.Children.Add(impactPanel);
        }
        
        panel.Children.Add(headerPanel);
        panel.Children.Add(titleBlock);
        panel.Children.Add(summaryBlock);
        newsControl.Content = panel;
        
        if (EnableAnimations && newsItem.IsBreaking)
        {
            ApplyBreakingNewsAnimation(newsControl);
        }
        
        return newsControl;
    }

    private UserControl CreateEventControl(HoloMarketEvent marketEvent)
    {
        var eventControl = new UserControl
        {
            Margin = new Thickness(2),
            Background = CreateHolographicBrush(0.3)
        };
        
        var severityColor = marketEvent.Severity switch
        {
            EventSeverity.High => GetEVEBrush("Negative"),
            EventSeverity.Medium => GetEVEBrush("Warning"),
            EventSeverity.Low => GetEVEBrush("Success"),
            _ => GetEVEBrush("Secondary")
        };
        
        eventControl.BorderBrush = severityColor;
        eventControl.BorderThickness = new Thickness(0, 0, 3, 0);
        
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(8)
        };
        
        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 3)
        };
        
        var statusIndicator = new Ellipse
        {
            Width = 8,
            Height = 8,
            Fill = marketEvent.IsActive ? GetEVEBrush("Success") : GetEVEBrush("Secondary"),
            Margin = new Thickness(0, 0, 5, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        headerPanel.Children.Add(statusIndicator);
        
        var severityBlock = new TextBlock
        {
            Text = marketEvent.Severity.ToString().ToUpper(),
            Foreground = severityColor,
            FontSize = 8,
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center
        };
        headerPanel.Children.Add(severityBlock);
        
        var timeBlock = new TextBlock
        {
            Text = FormatTimeAgo(marketEvent.Timestamp),
            Foreground = GetEVEBrush("Secondary"),
            FontSize = 8,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(10, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        headerPanel.Children.Add(timeBlock);
        
        var titleBlock = new TextBlock
        {
            Text = marketEvent.Title,
            Foreground = GetEVEBrush("Primary"),
            FontWeight = FontWeights.Bold,
            FontSize = 11,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 2, 0, 2)
        };
        
        var descriptionBlock = new TextBlock
        {
            Text = marketEvent.Description,
            Foreground = GetEVEBrush("Secondary"),
            FontSize = 9,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 3)
        };
        
        if (marketEvent.AffectedItems.Any())
        {
            var itemsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 2, 0, 0)
            };
            
            var itemsLabel = new TextBlock
            {
                Text = "Affected:",
                Foreground = GetEVEBrush("Info"),
                FontSize = 8,
                FontWeight = FontWeights.Bold
            };
            
            var itemsText = new TextBlock
            {
                Text = string.Join(", ", marketEvent.AffectedItems),
                Foreground = GetEVEBrush("Info"),
                FontSize = 8,
                Margin = new Thickness(5, 0, 0, 0)
            };
            
            itemsPanel.Children.Add(itemsLabel);
            itemsPanel.Children.Add(itemsText);
            panel.Children.Add(itemsPanel);
        }
        
        panel.Children.Add(headerPanel);
        panel.Children.Add(titleBlock);
        panel.Children.Add(descriptionBlock);
        eventControl.Content = panel;
        
        return eventControl;
    }

    #endregion

    #region Filtering and Data

    private IEnumerable<HoloMarketNewsItem> GetFilteredNews()
    {
        if (NewsItems == null) return Enumerable.Empty<HoloMarketNewsItem>();
        
        var filtered = NewsItems.AsEnumerable();
        
        if (NewsCategory != NewsCategory.All)
        {
            filtered = filtered.Where(n => n.Category == NewsCategory);
        }
        
        if (EventSeverity != EventSeverity.All)
        {
            filtered = filtered.Where(n => n.Severity == EventSeverity);
        }
        
        return filtered;
    }

    private IEnumerable<HoloMarketEvent> GetFilteredEvents()
    {
        if (MarketEvents == null) return Enumerable.Empty<HoloMarketEvent>();
        
        var filtered = MarketEvents.AsEnumerable();
        
        if (EventSeverity != EventSeverity.All)
        {
            filtered = filtered.Where(e => e.Severity == EventSeverity);
        }
        
        return filtered;
    }

    private void RefreshNewsData()
    {
        // Simulate new news items
        var newItems = _random.Next(1, 4);
        for (int i = 0; i < newItems; i++)
        {
            var templates = new[]
            {
                ("Market volatility detected in mineral sector", NewsCategory.MarketNews, EventSeverity.Medium),
                ("Corporation merger announcement shakes markets", NewsCategory.Economics, EventSeverity.High),
                ("New trade routes established in lowsec", NewsCategory.MarketNews, EventSeverity.Low),
                ("Faction warfare escalation affects logistics", NewsCategory.Politics, EventSeverity.Medium)
            };
            
            var template = templates[_random.Next(templates.Length)];
            var newsItem = new HoloMarketNewsItem
            {
                Id = Guid.NewGuid().ToString(),
                Title = template.Item1,
                Summary = GenerateNewsSummary(template.Item1),
                Category = template.Item2,
                Severity = template.Item3,
                MarketImpact = _random.NextDouble() * 0.2,
                Timestamp = DateTime.Now,
                Source = "EVE Market Intelligence",
                Tags = GenerateNewsTags(template.Item2),
                IsBreaking = template.Item3 == EventSeverity.High && _random.NextDouble() < 0.5
            };
            
            NewsItems.Insert(0, newsItem);
        }
        
        // Remove old items
        while (NewsItems.Count > 50)
        {
            NewsItems.RemoveAt(NewsItems.Count - 1);
        }
        
        PopulateNewsContent();
        PopulateEventsContent();
        AnimateNewsUpdate();
    }

    #endregion

    #region Trending and Analysis

    private void CreateTrendingTopics(Panel parent)
    {
        var trendingPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Width = 300,
            Margin = new Thickness(10),
            Background = CreateHolographicBrush(0.2)
        };
        
        var titleBlock = new TextBlock
        {
            Text = "Trending Topics",
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary"),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(5)
        };
        trendingPanel.Children.Add(titleBlock);
        
        var trendingTopics = new[] { "Mineral Prices", "Alliance Wars", "Manufacturing", "PLEX Market", "Trade Routes" };
        
        foreach (var topic in trendingTopics)
        {
            var trendPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
            
            var trendIcon = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = GetEVEBrush("Success"),
                Margin = new Thickness(0, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            
            var trendText = new TextBlock
            {
                Text = topic,
                Foreground = GetEVEBrush("Secondary"),
                FontSize = 10,
                VerticalAlignment = VerticalAlignment.Center
            };
            
            var trendCount = new TextBlock
            {
                Text = _random.Next(5, 25).ToString(),
                Foreground = GetEVEBrush("Success"),
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(10, 0, 0, 0)
            };
            
            trendPanel.Children.Add(trendIcon);
            trendPanel.Children.Add(trendText);
            trendPanel.Children.Add(trendCount);
            trendingPanel.Children.Add(trendPanel);
        }
        
        parent.Children.Add(trendingPanel);
    }

    private void CreateMarketSentiment(Panel parent)
    {
        var sentimentPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Width = 300,
            Margin = new Thickness(10),
            Background = CreateHolographicBrush(0.2)
        };
        
        var titleBlock = new TextBlock
        {
            Text = "Market Sentiment",
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary"),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(5)
        };
        sentimentPanel.Children.Add(titleBlock);
        
        var sentimentValue = _random.NextDouble() * 2 - 1; // -1 to 1
        var sentimentText = sentimentValue switch
        {
            > 0.3 => "Bullish",
            < -0.3 => "Bearish",
            _ => "Neutral"
        };
        var sentimentColor = sentimentValue switch
        {
            > 0.3 => "Success",
            < -0.3 => "Negative",
            _ => "Warning"
        };
        
        AddSentimentMetric(sentimentPanel, "Overall:", sentimentText, sentimentColor);
        AddSentimentMetric(sentimentPanel, "Minerals:", "Bullish", "Success");
        AddSentimentMetric(sentimentPanel, "Manufacturing:", "Neutral", "Warning");
        AddSentimentMetric(sentimentPanel, "Trading:", "Bearish", "Negative");
        AddSentimentMetric(sentimentPanel, "PLEX:", "Bullish", "Success");
        
        parent.Children.Add(sentimentPanel);
    }

    private void CreateImpactAnalysis(Panel parent)
    {
        var impactPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Width = 300,
            Margin = new Thickness(10),
            Background = CreateHolographicBrush(0.2)
        };
        
        var titleBlock = new TextBlock
        {
            Text = "Impact Analysis",
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary"),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(5)
        };
        impactPanel.Children.Add(titleBlock);
        
        var highImpactNews = NewsItems?.Count(n => n.MarketImpact > 0.1) ?? 0;
        var activeEvents = MarketEvents?.Count(e => e.IsActive) ?? 0;
        var avgImpact = NewsItems?.Average(n => n.MarketImpact) ?? 0;
        
        AddImpactMetric(impactPanel, "High Impact News:", highImpactNews.ToString());
        AddImpactMetric(impactPanel, "Active Events:", activeEvents.ToString());
        AddImpactMetric(impactPanel, "Average Impact:", $"{avgImpact:P1}");
        AddImpactMetric(impactPanel, "Risk Level:", "Medium");
        AddImpactMetric(impactPanel, "Volatility:", "Elevated");
        
        parent.Children.Add(impactPanel);
    }

    #endregion

    #region UI Helpers

    private void AddHeaderButton(Panel parent, string text, Action clickAction)
    {
        var button = new Button
        {
            Content = text,
            Margin = new Thickness(5),
            Padding = new Thickness(8, 4),
            Background = CreateHolographicBrush(0.3),
            Foreground = GetEVEBrush("Primary"),
            BorderBrush = GetEVEBrush("Accent"),
            FontSize = 10
        };
        button.Click += (s, e) => clickAction();
        parent.Children.Add(button);
    }

    private void AddFilterCard(Panel parent, string label, string value)
    {
        var card = new Border
        {
            Width = 120,
            Height = 45,
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
            FontSize = 9,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        
        var valueBlock = new TextBlock
        {
            Text = value,
            Foreground = GetEVEBrush("Secondary"),
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        
        panel.Children.Add(labelBlock);
        panel.Children.Add(valueBlock);
        card.Child = panel;
        parent.Children.Add(card);
    }

    private void AddSentimentMetric(Panel parent, string label, string sentiment, string colorType)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
        
        var labelElement = new TextBlock
        {
            Text = label,
            Width = 120,
            Foreground = GetEVEBrush("Primary"),
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 10
        };
        
        var sentimentElement = new TextBlock
        {
            Text = sentiment,
            Width = 80,
            Foreground = GetEVEBrush(colorType),
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeights.Bold,
            FontSize = 10
        };
        
        panel.Children.Add(labelElement);
        panel.Children.Add(sentimentElement);
        parent.Children.Add(panel);
    }

    private void AddImpactMetric(Panel parent, string label, string value)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
        
        var labelElement = new TextBlock
        {
            Text = label,
            Width = 140,
            Foreground = GetEVEBrush("Primary"),
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 10
        };
        
        var valueElement = new TextBlock
        {
            Text = value,
            Width = 80,
            Foreground = GetEVEBrush("Secondary"),
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 10
        };
        
        panel.Children.Add(labelElement);
        panel.Children.Add(valueElement);
        parent.Children.Add(panel);
    }

    private string FormatTimeAgo(DateTime timestamp)
    {
        var timeSpan = DateTime.Now - timestamp;
        
        return timeSpan switch
        {
            { TotalMinutes: < 1 } => "Just now",
            { TotalMinutes: < 60 } => $"{(int)timeSpan.TotalMinutes}m ago",
            { TotalHours: < 24 } => $"{(int)timeSpan.TotalHours}h ago",
            _ => $"{(int)timeSpan.TotalDays}d ago"
        };
    }

    #endregion

    #region Animation Methods

    private void AnimateNewsUpdate()
    {
        if (!EnableAnimations) return;
        
        var storyboard = new Storyboard();
        
        var fadeAnimation = new DoubleAnimation
        {
            From = 0.7,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        
        Storyboard.SetTarget(fadeAnimation, _newsScrollViewer);
        Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath("Opacity"));
        storyboard.Children.Add(fadeAnimation);
        
        var eventsAnimation = fadeAnimation.Clone();
        Storyboard.SetTarget(eventsAnimation, _eventsScrollViewer);
        storyboard.Children.Add(eventsAnimation);
        
        storyboard.Begin();
    }

    private void ApplyBreakingNewsAnimation(UserControl control)
    {
        var glowBrush = new SolidColorBrush(Color.FromArgb(80, 255, 50, 50));
        var originalBrush = control.Background;
        
        var storyboard = new Storyboard { RepeatBehavior = new RepeatBehavior(3) };
        
        var colorAnimation = new ColorAnimationUsingKeyFrames
        {
            Duration = TimeSpan.FromSeconds(1)
        };
        
        colorAnimation.KeyFrames.Add(new LinearColorKeyFrame(((SolidColorBrush)originalBrush).Color, TimeSpan.Zero));
        colorAnimation.KeyFrames.Add(new LinearColorKeyFrame(glowBrush.Color, TimeSpan.FromSeconds(0.5)));
        colorAnimation.KeyFrames.Add(new LinearColorKeyFrame(((SolidColorBrush)originalBrush).Color, TimeSpan.FromSeconds(1)));
        
        Storyboard.SetTarget(colorAnimation, control.Background);
        Storyboard.SetTargetProperty(colorAnimation, new PropertyPath("Color"));
        storyboard.Children.Add(colorAnimation);
        
        storyboard.Begin();
    }

    private HoloNewsParticle CreateNewsParticle()
    {
        var particle = new HoloNewsParticle
        {
            Element = new Ellipse
            {
                Width = _random.Next(1, 4),
                Height = _random.Next(1, 4),
                Fill = GetEVEBrush("Accent"),
                Opacity = 0.5
            },
            X = _random.NextDouble() * Width,
            Y = _random.NextDouble() * Height,
            VelocityX = (_random.NextDouble() - 0.5) * 1.2,
            VelocityY = (_random.NextDouble() - 0.5) * 1.2,
            Life = 1.0,
            MaxLife = _random.NextDouble() * 12 + 6
        };
        
        Canvas.SetLeft(particle.Element, particle.X);
        Canvas.SetTop(particle.Element, particle.Y);
        
        return particle;
    }

    private void CreateParticleEffects()
    {
        if (!EnableParticleEffects) return;
        
        for (int i = 0; i < 40; i++)
        {
            var particle = CreateNewsParticle();
            _newsParticles.Add(particle);
            _newsCanvas.Children.Add(particle.Element);
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
        UpdateNewsVisuals();
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        UpdateParticleEffects();
    }

    private void OnRefreshTick(object sender, EventArgs e)
    {
        if (AutoRefresh)
        {
            RefreshNewsData();
        }
    }

    #endregion

    #region Update Methods

    private void UpdateNewsVisuals()
    {
        if (!EnableAnimations) return;
        
        var time = DateTime.Now.TimeOfDay.TotalSeconds;
        var intensity = HolographicIntensity * (0.9 + 0.1 * Math.Sin(time * 2));
        
        foreach (var child in _newsCanvas.Children.OfType<FrameworkElement>())
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
        
        foreach (var particle in _newsParticles.ToList())
        {
            particle.X += particle.VelocityX;
            particle.Y += particle.VelocityY;
            particle.Life -= 0.05;
            
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

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _isInitialized = true;
        
        if (EnableAnimations)
            _animationTimer.Start();
        if (EnableParticleEffects)
            _particleTimer.Start();
        if (AutoRefresh)
            _refreshTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        _particleTimer?.Stop();
        _refreshTimer?.Stop();
    }

    #endregion

    #region Dependency Property Callbacks

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketNews news)
        {
            news.UpdateNewsVisuals();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketNews news)
        {
            news.CreateNewsInterface();
        }
    }

    private static void OnNewsItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketNews news)
        {
            news.PopulateNewsContent();
        }
    }

    private static void OnMarketEventsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketNews news)
        {
            news.PopulateEventsContent();
        }
    }

    private static void OnNewsCategoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketNews news)
        {
            news.PopulateNewsContent();
            news.CreateNewsInterface();
        }
    }

    private static void OnEventSeverityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketNews news)
        {
            news.PopulateNewsContent();
            news.PopulateEventsContent();
            news.CreateNewsInterface();
        }
    }

    private static void OnShowMarketImpactChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketNews news)
        {
            news.PopulateNewsContent();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketNews news)
        {
            if ((bool)e.NewValue)
                news._animationTimer?.Start();
            else
                news._animationTimer?.Stop();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketNews news)
        {
            if ((bool)e.NewValue)
            {
                news.CreateParticleEffects();
                news._particleTimer?.Start();
            }
            else
            {
                news._particleTimer?.Stop();
                news._newsParticles.Clear();
            }
        }
    }

    private static void OnShowTrendingTopicsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketNews news)
        {
            news.CreateNewsInterface();
        }
    }

    private static void OnShowEventTimelineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketNews news)
        {
            news.CreateNewsInterface();
        }
    }

    private static void OnAutoRefreshChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketNews news)
        {
            if ((bool)e.NewValue)
                news._refreshTimer?.Start();
            else
                news._refreshTimer?.Stop();
        }
    }

    private static void OnRefreshIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketNews news)
        {
            news._refreshTimer.Interval = (TimeSpan)e.NewValue;
        }
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void EnableSimplifiedMode()
    {
        EnableAnimations = false;
        EnableParticleEffects = false;
        ShowTrendingTopics = false;
        ShowEventTimeline = false;
    }

    public void EnableFullMode()
    {
        EnableAnimations = true;
        EnableParticleEffects = true;
        ShowTrendingTopics = true;
        ShowEventTimeline = true;
    }

    #endregion
}

#region Supporting Classes

public class HoloMarketNewsItem
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Summary { get; set; }
    public NewsCategory Category { get; set; }
    public EventSeverity Severity { get; set; }
    public double MarketImpact { get; set; }
    public DateTime Timestamp { get; set; }
    public string Source { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool IsBreaking { get; set; }
}

public class HoloMarketEvent
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public EventSeverity Severity { get; set; }
    public DateTime Timestamp { get; set; }
    public List<string> AffectedItems { get; set; } = new();
    public double EstimatedImpact { get; set; }
    public TimeSpan Duration { get; set; }
    public bool IsActive { get; set; }
}

public class HoloNewsParticle
{
    public UIElement Element { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
    public double MaxLife { get; set; }
}

public enum NewsCategory
{
    All,
    MarketNews,
    GameUpdates,
    Politics,
    Economics,
    Community
}

public enum EventSeverity
{
    All,
    Low,
    Medium,
    High
}

#endregion