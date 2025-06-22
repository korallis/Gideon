// ==========================================================================
// HoloCharacterOverview.cs - Holographic Character Overview Dashboard
// ==========================================================================
// Comprehensive character overview system featuring holographic data visualization,
// real-time statistics, EVE-style character analytics, and interactive dashboard display.
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
/// Holographic character overview dashboard with comprehensive character analytics and data visualization
/// </summary>
public class HoloCharacterOverview : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloCharacterOverview),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloCharacterOverview),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty CharacterDataProperty =
        DependencyProperty.Register(nameof(CharacterData), typeof(HoloCharacterData), typeof(HoloCharacterOverview),
            new PropertyMetadata(null, OnCharacterDataChanged));

    public static readonly DependencyProperty SelectedCharacterProperty =
        DependencyProperty.Register(nameof(SelectedCharacter), typeof(string), typeof(HoloCharacterOverview),
            new PropertyMetadata("Main Character", OnSelectedCharacterChanged));

    public static readonly DependencyProperty DashboardModeProperty =
        DependencyProperty.Register(nameof(DashboardMode), typeof(CharacterDashboardMode), typeof(HoloCharacterOverview),
            new PropertyMetadata(CharacterDashboardMode.Overview, OnDashboardModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloCharacterOverview),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloCharacterOverview),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowRealTimeDataProperty =
        DependencyProperty.Register(nameof(ShowRealTimeData), typeof(bool), typeof(HoloCharacterOverview),
            new PropertyMetadata(true, OnShowRealTimeDataChanged));

    public static readonly DependencyProperty ShowAdvancedMetricsProperty =
        DependencyProperty.Register(nameof(ShowAdvancedMetrics), typeof(bool), typeof(HoloCharacterOverview),
            new PropertyMetadata(true, OnShowAdvancedMetricsChanged));

    public static readonly DependencyProperty ShowCharacterPortraitProperty =
        DependencyProperty.Register(nameof(ShowCharacterPortrait), typeof(bool), typeof(HoloCharacterOverview),
            new PropertyMetadata(true, OnShowCharacterPortraitChanged));

    public static readonly DependencyProperty AutoRefreshProperty =
        DependencyProperty.Register(nameof(AutoRefresh), typeof(bool), typeof(HoloCharacterOverview),
            new PropertyMetadata(true, OnAutoRefreshChanged));

    public static readonly DependencyProperty RefreshIntervalProperty =
        DependencyProperty.Register(nameof(RefreshInterval), typeof(TimeSpan), typeof(HoloCharacterOverview),
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

    public HoloCharacterData CharacterData
    {
        get => (HoloCharacterData)GetValue(CharacterDataProperty);
        set => SetValue(CharacterDataProperty, value);
    }

    public string SelectedCharacter
    {
        get => (string)GetValue(SelectedCharacterProperty);
        set => SetValue(SelectedCharacterProperty, value);
    }

    public CharacterDashboardMode DashboardMode
    {
        get => (CharacterDashboardMode)GetValue(DashboardModeProperty);
        set => SetValue(DashboardModeProperty, value);
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

    public bool ShowRealTimeData
    {
        get => (bool)GetValue(ShowRealTimeDataProperty);
        set => SetValue(ShowRealTimeDataProperty, value);
    }

    public bool ShowAdvancedMetrics
    {
        get => (bool)GetValue(ShowAdvancedMetricsProperty);
        set => SetValue(ShowAdvancedMetricsProperty, value);
    }

    public bool ShowCharacterPortrait
    {
        get => (bool)GetValue(ShowCharacterPortraitProperty);
        set => SetValue(ShowCharacterPortraitProperty, value);
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

    private Canvas _mainCanvas;
    private Canvas _particleCanvas;
    private Border _characterHeaderSection;
    private Border _skillsOverviewSection;
    private Border _assetsOverviewSection;
    private Border _statusSection;
    private Border _metricsSection;
    private Border _activitySection;
    private Grid _dashboardGrid;
    private TabControl _overviewTabs;
    
    private readonly List<HoloCharacterParticle> _particles = new();
    private readonly DispatcherTimer _animationTimer;
    private readonly DispatcherTimer _dataRefreshTimer;
    private readonly Random _random = new();
    
    private bool _isAnimating = true;
    private bool _isDisposed = false;
    private double _currentAnimationTime = 0;

    #endregion

    #region Constructor

    public HoloCharacterOverview()
    {
        InitializeComponent();
        
        _animationTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16) // 60 FPS
        };
        _animationTimer.Tick += OnAnimationTick;
        
        _dataRefreshTimer = new DispatcherTimer
        {
            Interval = RefreshInterval
        };
        _dataRefreshTimer.Tick += OnDataRefreshTick;
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 1200;
        Height = 800;
        Background = new SolidColorBrush(Color.FromArgb(20, 0, 255, 255));
        
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 0, 50)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 255, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(100, 0, 255, 255),
                ShadowDepth = 0,
                BlurRadius = 15
            }
        };

        _mainCanvas = new Canvas();
        
        _particleCanvas = new Canvas
        {
            IsHitTestVisible = false,
            ClipToBounds = true
        };
        
        var contentGrid = new Grid();
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(120, GridUnitType.Pixel) });
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        
        // Header section with character info
        _characterHeaderSection = CreateCharacterHeaderSection();
        Grid.SetRow(_characterHeaderSection, 0);
        contentGrid.Children.Add(_characterHeaderSection);
        
        // Main dashboard area
        _overviewTabs = CreateOverviewTabs();
        Grid.SetRow(_overviewTabs, 1);
        contentGrid.Children.Add(_overviewTabs);
        
        _mainCanvas.Children.Add(contentGrid);
        _mainCanvas.Children.Add(_particleCanvas);
        border.Child = _mainCanvas;
        Content = border;

        CreateSampleData();
    }

    private Border CreateCharacterHeaderSection()
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(60, 0, 100, 200)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(120, 0, 255, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(10),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(80, 0, 255, 255),
                ShadowDepth = 0,
                BlurRadius = 10
            }
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100, GridUnitType.Pixel) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200, GridUnitType.Pixel) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200, GridUnitType.Pixel) });

        // Character portrait
        var portraitBorder = new Border
        {
            Width = 80,
            Height = 80,
            CornerRadius = new CornerRadius(40),
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 0, 255, 255)),
            BorderThickness = new Thickness(2),
            Margin = new Thickness(10),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(150, 0, 255, 255),
                ShadowDepth = 0,
                BlurRadius = 12
            }
        };

        var portraitEllipse = new Ellipse
        {
            Width = 76,
            Height = 76,
            Fill = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(200, 0, 150, 255), 0),
                    new GradientStop(Color.FromArgb(200, 0, 255, 150), 1)
                }
            }
        };
        portraitBorder.Child = portraitEllipse;
        Grid.SetColumn(portraitBorder, 0);
        grid.Children.Add(portraitBorder);

        // Character info
        var infoPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(20, 10, 10, 10)
        };

        var characterName = new TextBlock
        {
            Text = "Commander Aria Voss",
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };
        infoPanel.Children.Add(characterName);

        var corporationName = new TextBlock
        {
            Text = "Stellar Dynamics Corporation",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 200, 200)),
            Margin = new Thickness(0, 2, 0, 0)
        };
        infoPanel.Children.Add(corporationName);

        var allianceName = new TextBlock
        {
            Text = "Northern Coalition Alliance",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 150, 150, 150)),
            Margin = new Thickness(0, 2, 0, 0)
        };
        infoPanel.Children.Add(allianceName);

        var securityStatus = new TextBlock
        {
            Text = "Security Status: 5.0",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 0, 255, 100)),
            Margin = new Thickness(0, 5, 0, 0)
        };
        infoPanel.Children.Add(securityStatus);

        Grid.SetColumn(infoPanel, 1);
        grid.Children.Add(infoPanel);

        // Skill points summary
        var skillsPanel = CreateSkillsSummaryPanel();
        Grid.SetColumn(skillsPanel, 2);
        grid.Children.Add(skillsPanel);

        // Wealth summary
        var wealthPanel = CreateWealthSummaryPanel();
        Grid.SetColumn(wealthPanel, 3);
        grid.Children.Add(wealthPanel);

        border.Child = grid;
        return border;
    }

    private StackPanel CreateSkillsSummaryPanel()
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10)
        };

        var header = new TextBlock
        {
            Text = "SKILL POINTS",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 5)
        };
        panel.Children.Add(header);

        var totalSP = new TextBlock
        {
            Text = "125.7M SP",
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        panel.Children.Add(totalSP);

        var unallocatedSP = new TextBlock
        {
            Text = "2.1M Unallocated",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 200, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 2, 0, 0)
        };
        panel.Children.Add(unallocatedSP);

        var currentTraining = new TextBlock
        {
            Text = "Training: Gunnery V",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 0, 255, 200)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 2, 0, 0)
        };
        panel.Children.Add(currentTraining);

        var spPerHour = new TextBlock
        {
            Text = "2,547 SP/hour",
            FontSize = 9,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 200, 200, 200)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 2, 0, 0)
        };
        panel.Children.Add(spPerHour);

        return panel;
    }

    private StackPanel CreateWealthSummaryPanel()
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10)
        };

        var header = new TextBlock
        {
            Text = "NET WORTH",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 200, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 5)
        };
        panel.Children.Add(header);

        var totalWealth = new TextBlock
        {
            Text = "47.2B ISK",
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 200, 0)),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        panel.Children.Add(totalWealth);

        var liquidISK = new TextBlock
        {
            Text = "Liquid: 8.4B ISK",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 255, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 2, 0, 0)
        };
        panel.Children.Add(liquidISK);

        var assets = new TextBlock
        {
            Text = "Assets: 38.8B ISK",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 150, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 2, 0, 0)
        };
        panel.Children.Add(assets);

        var marketOrders = new TextBlock
        {
            Text = "Orders: 47 active",
            FontSize = 9,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 200, 200, 200)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 2, 0, 0)
        };
        panel.Children.Add(marketOrders);

        return panel;
    }

    private TabControl CreateOverviewTabs()
    {
        var tabControl = new TabControl
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(80, 0, 200, 255)),
            Margin = new Thickness(10)
        };

        // Overview Tab
        var overviewTab = new TabItem
        {
            Header = "Overview",
            Foreground = new SolidColorBrush(Colors.White),
            Background = new SolidColorBrush(Color.FromArgb(60, 0, 100, 150))
        };
        overviewTab.Content = CreateOverviewContent();
        tabControl.Items.Add(overviewTab);

        // Skills Tab
        var skillsTab = new TabItem
        {
            Header = "Skills",
            Foreground = new SolidColorBrush(Colors.White),
            Background = new SolidColorBrush(Color.FromArgb(60, 0, 100, 150))
        };
        skillsTab.Content = CreateSkillsContent();
        tabControl.Items.Add(skillsTab);

        // Assets Tab
        var assetsTab = new TabItem
        {
            Header = "Assets",
            Foreground = new SolidColorBrush(Colors.White),
            Background = new SolidColorBrush(Color.FromArgb(60, 0, 100, 150))
        };
        assetsTab.Content = CreateAssetsContent();
        tabControl.Items.Add(assetsTab);

        // Activity Tab
        var activityTab = new TabItem
        {
            Header = "Activity",
            Foreground = new SolidColorBrush(Colors.White),
            Background = new SolidColorBrush(Color.FromArgb(60, 0, 100, 150))
        };
        activityTab.Content = CreateActivityContent();
        tabControl.Items.Add(activityTab);

        return tabControl;
    }

    private Grid CreateOverviewContent()
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.RowDefinitions.Add(new RowDefinition());
        grid.RowDefinitions.Add(new RowDefinition());

        // Character status
        _statusSection = CreateStatusSection();
        Grid.SetColumn(_statusSection, 0);
        Grid.SetRow(_statusSection, 0);
        grid.Children.Add(_statusSection);

        // Metrics and analytics
        _metricsSection = CreateMetricsSection();
        Grid.SetColumn(_metricsSection, 1);
        Grid.SetRow(_metricsSection, 0);
        grid.Children.Add(_metricsSection);

        // Location and ship info
        var locationSection = CreateLocationSection();
        Grid.SetColumn(locationSection, 0);
        Grid.SetRow(locationSection, 1);
        grid.Children.Add(locationSection);

        // Recent activity
        var recentActivitySection = CreateRecentActivitySection();
        Grid.SetColumn(recentActivitySection, 1);
        Grid.SetRow(recentActivitySection, 1);
        grid.Children.Add(recentActivitySection);

        return grid;
    }

    private Border CreateStatusSection()
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 100, 150)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(80, 0, 200, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(10),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(60, 0, 200, 255),
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(15, 10, 15, 10)
        };

        var header = new TextBlock
        {
            Text = "CHARACTER STATUS",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        panel.Children.Add(header);

        var status = new[]
        {
            new { Label = "Online Status:", Value = "Online", Color = Color.FromArgb(255, 0, 255, 100) },
            new { Label = "Clone Location:", Value = "Jita IV - Moon 4", Color = Color.FromArgb(200, 255, 255, 255) },
            new { Label = "Clone Grade:", Value = "Gamma", Color = Color.FromArgb(200, 255, 255, 0) },
            new { Label = "Neural Remap:", Value = "Available", Color = Color.FromArgb(200, 0, 255, 200) },
            new { Label = "Character Age:", Value = "8 years, 142 days", Color = Color.FromArgb(200, 255, 255, 255) },
            new { Label = "Corporation Role:", Value = "Director", Color = Color.FromArgb(200, 255, 200, 0) },
            new { Label = "Standings:", Value = "Excellent", Color = Color.FromArgb(200, 0, 255, 100) },
            new { Label = "Jump Clones:", Value = "5/10 Active", Color = Color.FromArgb(200, 255, 255, 255) }
        };

        foreach (var stat in status)
        {
            var statPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 2, 0, 2)
            };

            var labelText = new TextBlock
            {
                Text = stat.Label,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 200, 200)),
                Width = 120
            };
            statPanel.Children.Add(labelText);

            var valueText = new TextBlock
            {
                Text = stat.Value,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(stat.Color)
            };
            statPanel.Children.Add(valueText);

            panel.Children.Add(statPanel);
        }

        border.Child = panel;
        return border;
    }

    private Border CreateMetricsSection()
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 100, 50, 0)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(80, 255, 200, 0)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(10),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(60, 255, 200, 0),
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(15, 10, 15, 10)
        };

        var header = new TextBlock
        {
            Text = "PERFORMANCE METRICS",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        panel.Children.Add(header);

        var metrics = new[]
        {
            "ISK Efficiency: 97.3%",
            "PvP Rating: 2,847",
            "Mission Success: 94.1%",
            "Market Efficiency: 89.6%",
            "Exploration Score: 15,420",
            "Industry Index: 3.2",
            "Fleet Command: Expert",
            "Risk Assessment: Moderate"
        };

        foreach (var metric in metrics)
        {
            var metricText = new TextBlock
            {
                Text = metric,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 200)),
                Margin = new Thickness(0, 2, 0, 0)
            };
            panel.Children.Add(metricText);
        }

        border.Child = panel;
        return border;
    }

    private Border CreateLocationSection()
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 150, 0, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(80, 255, 0, 200)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(10),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(60, 255, 0, 200),
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(15, 10, 15, 10)
        };

        var header = new TextBlock
        {
            Text = "LOCATION & SHIP",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 200)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        panel.Children.Add(header);

        var location = new[]
        {
            new { Label = "Current System:", Value = "Jita", Color = Color.FromArgb(255, 255, 255, 0) },
            new { Label = "Region:", Value = "The Forge", Color = Color.FromArgb(200, 255, 255, 255) },
            new { Label = "Security Level:", Value = "1.0 (High-Sec)", Color = Color.FromArgb(200, 0, 255, 100) },
            new { Label = "Station:", Value = "Jita IV - Caldari Navy Assembly Plant", Color = Color.FromArgb(200, 255, 255, 255) },
            new { Label = "Current Ship:", Value = "Raven Navy Issue", Color = Color.FromArgb(255, 255, 200, 0) },
            new { Label = "Ship Value:", Value = "2.1B ISK", Color = Color.FromArgb(200, 255, 200, 0) },
            new { Label = "Fitting Status:", Value = "T2 Combat Fit", Color = Color.FromArgb(200, 0, 255, 200) },
            new { Label = "Jump Fatigue:", Value = "None", Color = Color.FromArgb(200, 0, 255, 100) }
        };

        foreach (var loc in location)
        {
            var locPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 2, 0, 2)
            };

            var labelText = new TextBlock
            {
                Text = loc.Label,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 200, 200)),
                Width = 120
            };
            locPanel.Children.Add(labelText);

            var valueText = new TextBlock
            {
                Text = loc.Value,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(loc.Color)
            };
            locPanel.Children.Add(valueText);

            panel.Children.Add(locPanel);
        }

        border.Child = panel;
        return border;
    }

    private Border CreateRecentActivitySection()
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 100, 0, 150)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(80, 200, 0, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(10),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(60, 200, 0, 255),
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(15, 10, 15, 10)
        };

        var header = new TextBlock
        {
            Text = "RECENT ACTIVITY",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 0, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        panel.Children.Add(header);

        var activities = new[]
        {
            new { Time = "2 min ago", Activity = "Completed Gunnery V training", Type = "Skill" },
            new { Time = "15 min ago", Activity = "Sold 1,000 units of Tritanium", Type = "Market" },
            new { Time = "1 hour ago", Activity = "Docked at Jita IV - Moon 4", Type = "Movement" },
            new { Time = "2 hours ago", Activity = "Fleet operation in null-sec", Type = "Combat" },
            new { Time = "4 hours ago", Activity = "Updated market orders", Type = "Market" },
            new { Time = "6 hours ago", Activity = "Joined corporation fleet", Type = "Social" },
            new { Time = "8 hours ago", Activity = "Purchased Raven Navy Issue", Type = "Purchase" },
            new { Time = "12 hours ago", Activity = "Logged in from Amarr", Type = "Login" }
        };

        foreach (var activity in activities)
        {
            var activityPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 3, 0, 3)
            };

            var timeText = new TextBlock
            {
                Text = activity.Time,
                FontSize = 9,
                Foreground = new SolidColorBrush(Color.FromArgb(150, 200, 200, 200))
            };
            activityPanel.Children.Add(timeText);

            var activityText = new TextBlock
            {
                Text = activity.Activity,
                FontSize = 10,
                Foreground = new SolidColorBrush(Colors.White),
                TextWrapping = TextWrapping.Wrap
            };
            activityPanel.Children.Add(activityText);

            var typeColor = activity.Type switch
            {
                "Skill" => Color.FromArgb(200, 255, 255, 0),
                "Market" => Color.FromArgb(200, 0, 255, 100),
                "Combat" => Color.FromArgb(200, 255, 100, 100),
                "Movement" => Color.FromArgb(200, 100, 200, 255),
                _ => Color.FromArgb(200, 200, 200, 200)
            };

            var typeText = new TextBlock
            {
                Text = $"[{activity.Type}]",
                FontSize = 8,
                Foreground = new SolidColorBrush(typeColor),
                FontWeight = FontWeights.Bold
            };
            activityPanel.Children.Add(typeText);

            panel.Children.Add(activityPanel);
        }

        border.Child = panel;
        return border;
    }

    private ScrollViewer CreateSkillsContent()
    {
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = new SolidColorBrush(Color.FromArgb(20, 0, 50, 100))
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10)
        };

        // Skill categories with progress
        var skillCategories = new[]
        {
            new { Category = "Spaceship Command", Progress = 0.87, SP = "34.2M", Color = Color.FromArgb(255, 255, 200, 0) },
            new { Category = "Gunnery", Progress = 0.94, SP = "28.7M", Color = Color.FromArgb(255, 255, 100, 100) },
            new { Category = "Engineering", Progress = 0.76, SP = "15.3M", Color = Color.FromArgb(255, 0, 255, 200) },
            new { Category = "Navigation", Progress = 0.82, SP = "12.1M", Color = Color.FromArgb(255, 100, 255, 100) },
            new { Category = "Trade", Progress = 0.65, SP = "8.4M", Color = Color.FromArgb(255, 255, 255, 0) },
            new { Category = "Industry", Progress = 0.43, SP = "5.2M", Color = Color.FromArgb(255, 200, 150, 100) }
        };

        foreach (var category in skillCategories)
        {
            var categoryItem = CreateSkillCategoryItem(category.Category, category.Progress, category.SP, category.Color);
            panel.Children.Add(categoryItem);
        }

        scrollViewer.Content = panel;
        return scrollViewer;
    }

    private Border CreateSkillCategoryItem(string category, double progress, string sp, Color color)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, color.R, color.G, color.B)),
            BorderBrush = new SolidColorBrush(color),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(5),
            Effect = new DropShadowEffect
            {
                Color = color,
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80, GridUnitType.Pixel) });
        grid.RowDefinitions.Add(new RowDefinition());
        grid.RowDefinitions.Add(new RowDefinition());

        var categoryText = new TextBlock
        {
            Text = category,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            Margin = new Thickness(15, 10, 10, 5)
        };
        Grid.SetColumn(categoryText, 0);
        Grid.SetRow(categoryText, 0);
        grid.Children.Add(categoryText);

        var spText = new TextBlock
        {
            Text = sp,
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(color),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 10, 15, 5)
        };
        Grid.SetColumn(spText, 1);
        Grid.SetRow(spText, 0);
        grid.Children.Add(spText);

        // Progress bar
        var progressBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(60, 50, 50, 50)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 100, 100, 100)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(2),
            Height = 8,
            Margin = new Thickness(15, 0, 15, 10)
        };

        var progressBar = new Rectangle
        {
            Fill = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(color, 0),
                    new GradientStop(Color.FromArgb(color.A, (byte)(color.R * 0.7), (byte)(color.G * 0.7), (byte)(color.B * 0.7)), 1)
                }
            },
            Width = 250 * progress,
            Height = 6,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            RadiusX = 1,
            RadiusY = 1
        };

        progressBorder.Child = progressBar;
        Grid.SetColumn(progressBorder, 0);
        Grid.SetRow(progressBorder, 1);
        Grid.SetColumnSpan(progressBorder, 2);
        grid.Children.Add(progressBorder);

        border.Child = grid;
        return border;
    }

    private ScrollViewer CreateAssetsContent()
    {
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = new SolidColorBrush(Color.FromArgb(20, 0, 50, 100))
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10)
        };

        // Asset categories
        var assetCategories = new[]
        {
            new { Category = "Ships", Count = 47, Value = "23.5B ISK", Color = Color.FromArgb(255, 255, 200, 0) },
            new { Category = "Modules", Count = 1523, Value = "8.7B ISK", Color = Color.FromArgb(255, 0, 255, 200) },
            new { Category = "Ammunition", Count = 45678, Value = "2.1B ISK", Color = Color.FromArgb(255, 255, 100, 100) },
            new { Category = "Materials", Count = 234567, Value = "3.8B ISK", Color = Color.FromArgb(255, 100, 255, 100) },
            new { Category = "Blueprints", Count = 89, Value = "1.2B ISK", Color = Color.FromArgb(255, 200, 0, 255) },
            new { Category = "Implants", Count = 23, Value = "890M ISK", Color = Color.FromArgb(255, 255, 0, 200) }
        };

        foreach (var asset in assetCategories)
        {
            var assetItem = CreateAssetCategoryItem(asset.Category, asset.Count, asset.Value, asset.Color);
            panel.Children.Add(assetItem);
        }

        scrollViewer.Content = panel;
        return scrollViewer;
    }

    private Border CreateAssetCategoryItem(string category, int count, string value, Color color)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, color.R, color.G, color.B)),
            BorderBrush = new SolidColorBrush(color),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(5),
            Effect = new DropShadowEffect
            {
                Color = color,
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100, GridUnitType.Pixel) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100, GridUnitType.Pixel) });

        var categoryText = new TextBlock
        {
            Text = category,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(15, 15, 10, 15)
        };
        Grid.SetColumn(categoryText, 0);
        grid.Children.Add(categoryText);

        var countText = new TextBlock
        {
            Text = $"{count:N0} items",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 15, 5, 15)
        };
        Grid.SetColumn(countText, 1);
        grid.Children.Add(countText);

        var valueText = new TextBlock
        {
            Text = value,
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(color),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 15, 15, 15)
        };
        Grid.SetColumn(valueText, 2);
        grid.Children.Add(valueText);

        border.Child = grid;
        return border;
    }

    private ScrollViewer CreateActivityContent()
    {
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = new SolidColorBrush(Color.FromArgb(20, 0, 50, 100))
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10)
        };

        // Activity timeline with more detailed entries
        var activities = new[]
        {
            new { Date = "Today", Time = "14:23", Activity = "Completed training: Gunnery V", Details = "Final level completed after 8 days", Type = "Skill", Icon = "🎯" },
            new { Date = "Today", Time = "13:47", Activity = "Market transaction completed", Details = "Sold 1,000 units of Tritanium for 4.2M ISK", Type = "Market", Icon = "💰" },
            new { Date = "Today", Time = "12:15", Activity = "Docked at Jita IV - Moon 4", Details = "Caldari Navy Assembly Plant", Type = "Movement", Icon = "🚀" },
            new { Date = "Today", Time = "10:30", Activity = "Fleet operation completed", Details = "Null-sec roam with 24 pilots, 3 kills", Type = "Combat", Icon = "⚔️" },
            new { Date = "Yesterday", Time = "22:45", Activity = "Ship purchase", Details = "Acquired Raven Navy Issue for 2.1B ISK", Type = "Purchase", Icon = "🛸" },
            new { Date = "Yesterday", Time = "20:12", Activity = "Corporation meeting", Details = "Strategic planning session", Type = "Social", Icon = "👥" },
            new { Date = "Yesterday", Time = "18:30", Activity = "Mission completed", Details = "Level 4 security mission - The Blockade", Type = "PvE", Icon = "🎯" },
            new { Date = "2 days ago", Time = "16:45", Activity = "Manufacturing job completed", Details = "Produced 50 Large Shield Extender II", Type = "Industry", Icon = "🏭" }
        };

        string currentDate = "";
        foreach (var activity in activities)
        {
            if (activity.Date != currentDate)
            {
                currentDate = activity.Date;
                var dateHeader = new TextBlock
                {
                    Text = activity.Date.ToUpper(),
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
                    Margin = new Thickness(5, 15, 5, 5)
                };
                panel.Children.Add(dateHeader);
            }

            var activityItem = CreateActivityItem(activity.Time, activity.Activity, activity.Details, activity.Type, activity.Icon);
            panel.Children.Add(activityItem);
        }

        scrollViewer.Content = panel;
        return scrollViewer;
    }

    private Border CreateActivityItem(string time, string activity, string details, string type, string icon)
    {
        var typeColor = type switch
        {
            "Skill" => Color.FromArgb(100, 255, 255, 0),
            "Market" => Color.FromArgb(100, 0, 255, 100),
            "Combat" => Color.FromArgb(100, 255, 100, 100),
            "Movement" => Color.FromArgb(100, 100, 200, 255),
            "Purchase" => Color.FromArgb(100, 255, 200, 0),
            "Social" => Color.FromArgb(100, 200, 0, 255),
            "PvE" => Color.FromArgb(100, 255, 150, 0),
            "Industry" => Color.FromArgb(100, 150, 255, 0),
            _ => Color.FromArgb(100, 150, 150, 150)
        };

        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(30, typeColor.R, typeColor.G, typeColor.B)),
            BorderBrush = new SolidColorBrush(typeColor),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3),
            Margin = new Thickness(5, 2, 5, 2)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40, GridUnitType.Pixel) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60, GridUnitType.Pixel) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var iconText = new TextBlock
        {
            Text = icon,
            FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5)
        };
        Grid.SetColumn(iconText, 0);
        grid.Children.Add(iconText);

        var timeText = new TextBlock
        {
            Text = time,
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 200, 200)),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5)
        };
        Grid.SetColumn(timeText, 1);
        grid.Children.Add(timeText);

        var contentPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10, 8, 15, 8)
        };

        var activityText = new TextBlock
        {
            Text = activity,
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            TextWrapping = TextWrapping.Wrap
        };
        contentPanel.Children.Add(activityText);

        var detailsText = new TextBlock
        {
            Text = details,
            FontSize = 9,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 200, 200, 200)),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 2, 0, 0)
        };
        contentPanel.Children.Add(detailsText);

        Grid.SetColumn(contentPanel, 2);
        grid.Children.Add(contentPanel);

        border.Child = grid;
        return border;
    }

    private void CreateSampleData()
    {
        CharacterData = new HoloCharacterData
        {
            CharacterName = "Commander Aria Voss",
            CorporationName = "Stellar Dynamics Corporation",
            AllianceName = "Northern Coalition Alliance",
            SecurityStatus = 5.0,
            TotalSkillPoints = 125700000,
            UnallocatedSkillPoints = 2100000,
            CurrentTrainingSkill = "Gunnery V",
            TrainingTimeRemaining = TimeSpan.FromHours(8.4),
            NetWorth = 47200000000,
            LiquidISK = 8400000000,
            CharacterAge = TimeSpan.FromDays(2975), // ~8 years
            OnlineStatus = "Online",
            CurrentLocation = "Jita IV - Moon 4 - Caldari Navy Assembly Plant",
            CurrentShip = "Raven Navy Issue",
            LastLogin = DateTime.Now.AddMinutes(-15)
        };
    }

    #endregion

    #region Particle System

    private void CreateCharacterParticles()
    {
        if (!EnableParticleEffects || _particleCanvas == null) return;

        for (int i = 0; i < 2; i++)
        {
            var particle = new HoloCharacterParticle
            {
                X = _random.NextDouble() * ActualWidth,
                Y = _random.NextDouble() * ActualHeight,
                VelocityX = (_random.NextDouble() - 0.5) * 4,
                VelocityY = (_random.NextDouble() - 0.5) * 4,
                Size = _random.NextDouble() * 3 + 1,
                Life = 1.0,
                ParticleType = CharacterParticleType.Data,
                Ellipse = new Ellipse
                {
                    Width = 3,
                    Height = 3,
                    Fill = new SolidColorBrush(Color.FromArgb(60, 0, 255, 200))
                }
            };

            Canvas.SetLeft(particle.Ellipse, particle.X);
            Canvas.SetTop(particle.Ellipse, particle.Y);
            _particleCanvas.Children.Add(particle.Ellipse);
            _particles.Add(particle);
        }
    }

    private void UpdateParticles()
    {
        if (!EnableParticleEffects) return;

        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var particle = _particles[i];
            
            particle.X += particle.VelocityX * 0.016;
            particle.Y += particle.VelocityY * 0.016;
            particle.Life -= 0.002;

            if (particle.Life <= 0 || particle.X < 0 || particle.X > ActualWidth || 
                particle.Y < 0 || particle.Y > ActualHeight)
            {
                _particleCanvas.Children.Remove(particle.Ellipse);
                _particles.RemoveAt(i);
                continue;
            }

            Canvas.SetLeft(particle.Ellipse, particle.X);
            Canvas.SetTop(particle.Ellipse, particle.Y);
            
            var alpha = (byte)(particle.Life * 60);
            var color = particle.ParticleType switch
            {
                CharacterParticleType.Data => Color.FromArgb(alpha, 0, 255, 200),
                CharacterParticleType.Activity => Color.FromArgb(alpha, 255, 200, 0),
                CharacterParticleType.Status => Color.FromArgb(alpha, 200, 0, 255),
                _ => Color.FromArgb(alpha, 255, 255, 255)
            };
            
            particle.Ellipse.Fill = new SolidColorBrush(color);
        }

        if (_random.NextDouble() < 0.02)
        {
            CreateCharacterParticles();
        }
    }

    #endregion

    #region Animation and Updates

    private void OnAnimationTick(object sender, EventArgs e)
    {
        if (!_isAnimating) return;

        _currentAnimationTime += 16;
        
        UpdateParticles();
        UpdateCharacterAnimations();
    }

    private void OnDataRefreshTick(object sender, EventArgs e)
    {
        if (ShowRealTimeData)
        {
            RefreshCharacterData();
        }
    }

    private void UpdateCharacterAnimations()
    {
        if (!EnableAnimations) return;

        var intensity = (Math.Sin(_currentAnimationTime * 0.0003) + 1) * 0.5 * HolographicIntensity;
        
        // Animate section glows
        var sections = new[] { _characterHeaderSection, _statusSection, _metricsSection, _activitySection };
        foreach (var section in sections)
        {
            if (section?.Effect is DropShadowEffect effect)
            {
                effect.BlurRadius = 8 + intensity * 2;
            }
        }
    }

    private void RefreshCharacterData()
    {
        // Simulate real-time data updates with small variations
        if (CharacterData != null)
        {
            // Simulate skill point changes
            CharacterData.TotalSkillPoints += _random.Next(0, 100);
            
            // Simulate ISK changes
            var iskChange = (_random.NextDouble() - 0.5) * 100000;
            CharacterData.LiquidISK += (long)iskChange;
            CharacterData.NetWorth += (long)iskChange;
            
            // Update training time
            if (CharacterData.TrainingTimeRemaining > TimeSpan.Zero)
            {
                CharacterData.TrainingTimeRemaining = CharacterData.TrainingTimeRemaining.Subtract(TimeSpan.FromMinutes(5));
            }
        }
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _animationTimer.Start();
        if (AutoRefresh)
        {
            _dataRefreshTimer.Start();
        }
        _isAnimating = true;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer.Stop();
        _dataRefreshTimer.Stop();
        _isAnimating = false;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_particleCanvas != null)
        {
            _particleCanvas.Width = ActualWidth;
            _particleCanvas.Height = ActualHeight;
        }
    }

    #endregion

    #region Property Change Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterOverview control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterOverview control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnCharacterDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterOverview control)
        {
            control.RefreshCharacterDisplay();
        }
    }

    private static void OnSelectedCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterOverview control)
        {
            control.LoadCharacterData();
        }
    }

    private static void OnDashboardModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterOverview control)
        {
            control.UpdateDashboardMode();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterOverview control)
        {
            control._isAnimating = control.EnableAnimations;
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterOverview control)
        {
            if (!control.EnableParticleEffects)
            {
                control.ClearParticles();
            }
        }
    }

    private static void OnShowRealTimeDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterOverview control)
        {
            if (control.ShowRealTimeData && control.AutoRefresh)
            {
                control._dataRefreshTimer.Start();
            }
            else
            {
                control._dataRefreshTimer.Stop();
            }
        }
    }

    private static void OnShowAdvancedMetricsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterOverview control)
        {
            control._metricsSection.Visibility = control.ShowAdvancedMetrics ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnShowCharacterPortraitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterOverview control)
        {
            control.UpdatePortraitVisibility();
        }
    }

    private static void OnAutoRefreshChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterOverview control)
        {
            if (control.AutoRefresh && control.ShowRealTimeData)
            {
                control._dataRefreshTimer.Start();
            }
            else
            {
                control._dataRefreshTimer.Stop();
            }
        }
    }

    private static void OnRefreshIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterOverview control)
        {
            control._dataRefreshTimer.Interval = control.RefreshInterval;
        }
    }

    #endregion

    #region Helper Methods

    private void UpdateHolographicEffects()
    {
        // Update holographic intensity for all effects
    }

    private void UpdateColorScheme()
    {
        // Update colors based on EVE color scheme
    }

    private void RefreshCharacterDisplay()
    {
        // Refresh character data display
    }

    private void LoadCharacterData()
    {
        // Load data for selected character
    }

    private void UpdateDashboardMode()
    {
        // Update dashboard display based on mode
    }

    private void UpdatePortraitVisibility()
    {
        // Show/hide character portrait
    }

    private void ClearParticles()
    {
        foreach (var particle in _particles)
        {
            _particleCanvas.Children.Remove(particle.Ellipse);
        }
        _particles.Clear();
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void OptimizeForLowEndHardware()
    {
        EnableParticleEffects = false;
        EnableAnimations = false;
        ShowRealTimeData = false;
    }

    public void OptimizeForHighEndHardware()
    {
        EnableParticleEffects = true;
        EnableAnimations = true;
        ShowRealTimeData = true;
    }

    public bool SupportsHardwareAcceleration => true;

    #endregion

    #region Cleanup

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        
        if (_mainCanvas != null)
        {
            _mainCanvas.Width = ActualWidth;
            _mainCanvas.Height = ActualHeight;
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _animationTimer?.Stop();
        _dataRefreshTimer?.Stop();
        ClearParticles();
        
        _isDisposed = true;
    }

    #endregion
}

#region Supporting Classes

public class HoloCharacterData
{
    public string CharacterName { get; set; } = "";
    public string CorporationName { get; set; } = "";
    public string AllianceName { get; set; } = "";
    public double SecurityStatus { get; set; }
    public long TotalSkillPoints { get; set; }
    public long UnallocatedSkillPoints { get; set; }
    public string CurrentTrainingSkill { get; set; } = "";
    public TimeSpan TrainingTimeRemaining { get; set; }
    public long NetWorth { get; set; }
    public long LiquidISK { get; set; }
    public TimeSpan CharacterAge { get; set; }
    public string OnlineStatus { get; set; } = "";
    public string CurrentLocation { get; set; } = "";
    public string CurrentShip { get; set; } = "";
    public DateTime LastLogin { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

public class HoloCharacterParticle
{
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Size { get; set; }
    public double Life { get; set; }
    public CharacterParticleType ParticleType { get; set; }
    public Ellipse Ellipse { get; set; } = null!;
}

public enum CharacterDashboardMode
{
    Overview,
    Skills,
    Assets,
    Activity,
    Detailed
}

public enum CharacterParticleType
{
    Data,
    Activity,
    Status,
    Metric
}

#endregion