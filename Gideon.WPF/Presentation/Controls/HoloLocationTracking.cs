// ==========================================================================
// HoloLocationTracking.cs - Holographic Character Location Tracking
// ==========================================================================
// Advanced location tracking system featuring 3D map visualization,
// security status analysis, EVE-style jump tracking, and holographic route display.
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
/// Holographic character location tracking with 3D map visualization and real-time position monitoring
/// </summary>
public class HoloLocationTracking : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloLocationTracking),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloLocationTracking),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty LocationHistoryProperty =
        DependencyProperty.Register(nameof(LocationHistory), typeof(ObservableCollection<HoloLocationEntry>), typeof(HoloLocationTracking),
            new PropertyMetadata(null, OnLocationHistoryChanged));

    public static readonly DependencyProperty CurrentLocationProperty =
        DependencyProperty.Register(nameof(CurrentLocation), typeof(HoloLocationEntry), typeof(HoloLocationTracking),
            new PropertyMetadata(null, OnCurrentLocationChanged));

    public static readonly DependencyProperty SelectedCharacterProperty =
        DependencyProperty.Register(nameof(SelectedCharacter), typeof(string), typeof(HoloLocationTracking),
            new PropertyMetadata("Main Character", OnSelectedCharacterChanged));

    public static readonly DependencyProperty TrackingModeProperty =
        DependencyProperty.Register(nameof(TrackingMode), typeof(LocationTrackingMode), typeof(HoloLocationTracking),
            new PropertyMetadata(LocationTrackingMode.RealTime, OnTrackingModeChanged));

    public static readonly DependencyProperty MapViewModeProperty =
        DependencyProperty.Register(nameof(MapViewMode), typeof(MapViewMode), typeof(HoloLocationTracking),
            new PropertyMetadata(MapViewMode.Galaxy, OnMapViewModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloLocationTracking),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloLocationTracking),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowSecurityStatusProperty =
        DependencyProperty.Register(nameof(ShowSecurityStatus), typeof(bool), typeof(HoloLocationTracking),
            new PropertyMetadata(true, OnShowSecurityStatusChanged));

    public static readonly DependencyProperty ShowJumpHistoryProperty =
        DependencyProperty.Register(nameof(ShowJumpHistory), typeof(bool), typeof(HoloLocationTracking),
            new PropertyMetadata(true, OnShowJumpHistoryChanged));

    public static readonly DependencyProperty ShowRouteOptimizationProperty =
        DependencyProperty.Register(nameof(ShowRouteOptimization), typeof(bool), typeof(HoloLocationTracking),
            new PropertyMetadata(true, OnShowRouteOptimizationChanged));

    public static readonly DependencyProperty AutoCenterOnCharacterProperty =
        DependencyProperty.Register(nameof(AutoCenterOnCharacter), typeof(bool), typeof(HoloLocationTracking),
            new PropertyMetadata(true, OnAutoCenterOnCharacterChanged));

    public static readonly DependencyProperty TrackingRangeProperty =
        DependencyProperty.Register(nameof(TrackingRange), typeof(double), typeof(HoloLocationTracking),
            new PropertyMetadata(10.0, OnTrackingRangeChanged));

    public static readonly DependencyProperty ShowDangerZonesProperty =
        DependencyProperty.Register(nameof(ShowDangerZones), typeof(bool), typeof(HoloLocationTracking),
            new PropertyMetadata(true, OnShowDangerZonesChanged));

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

    public ObservableCollection<HoloLocationEntry> LocationHistory
    {
        get => (ObservableCollection<HoloLocationEntry>)GetValue(LocationHistoryProperty);
        set => SetValue(LocationHistoryProperty, value);
    }

    public HoloLocationEntry CurrentLocation
    {
        get => (HoloLocationEntry)GetValue(CurrentLocationProperty);
        set => SetValue(CurrentLocationProperty, value);
    }

    public string SelectedCharacter
    {
        get => (string)GetValue(SelectedCharacterProperty);
        set => SetValue(SelectedCharacterProperty, value);
    }

    public LocationTrackingMode TrackingMode
    {
        get => (LocationTrackingMode)GetValue(TrackingModeProperty);
        set => SetValue(TrackingModeProperty, value);
    }

    public MapViewMode MapViewMode
    {
        get => (MapViewMode)GetValue(MapViewModeProperty);
        set => SetValue(MapViewModeProperty, value);
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

    public bool ShowSecurityStatus
    {
        get => (bool)GetValue(ShowSecurityStatusProperty);
        set => SetValue(ShowSecurityStatusProperty, value);
    }

    public bool ShowJumpHistory
    {
        get => (bool)GetValue(ShowJumpHistoryProperty);
        set => SetValue(ShowJumpHistoryProperty, value);
    }

    public bool ShowRouteOptimization
    {
        get => (bool)GetValue(ShowRouteOptimizationProperty);
        set => SetValue(ShowRouteOptimizationProperty, value);
    }

    public bool AutoCenterOnCharacter
    {
        get => (bool)GetValue(AutoCenterOnCharacterProperty);
        set => SetValue(AutoCenterOnCharacterProperty, value);
    }

    public double TrackingRange
    {
        get => (double)GetValue(TrackingRangeProperty);
        set => SetValue(TrackingRangeProperty, value);
    }

    public bool ShowDangerZones
    {
        get => (bool)GetValue(ShowDangerZonesProperty);
        set => SetValue(ShowDangerZonesProperty, value);
    }

    #endregion

    #region Fields

    private readonly DispatcherTimer _animationTimer;
    private readonly DispatcherTimer _trackingUpdateTimer;
    private readonly List<ParticleSystem> _particleSystems = new();
    private Canvas _particleCanvas;
    private Grid _mainGrid;
    private TabControl _tabControl;
    private Canvas _mapCanvas;
    private Border _currentLocationIndicator;
    private readonly Random _random = new();

    #endregion

    #region Constructor

    public HoloLocationTracking()
    {
        InitializeComponent();
        InitializeTimers();
        InitializeDefaults();
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        _mainGrid = new Grid
        {
            Background = new SolidColorBrush(Color.FromArgb(20, 0, 150, 255))
        };

        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var header = CreateHeaderSection();
        Grid.SetRow(header, 0);
        _mainGrid.Children.Add(header);

        _tabControl = CreateTabControl();
        Grid.SetRow(_tabControl, 1);
        _mainGrid.Children.Add(_tabControl);

        _particleCanvas = new Canvas
        {
            IsHitTestVisible = false,
            Background = Brushes.Transparent
        };
        Grid.SetRowSpan(_particleCanvas, 2);
        _mainGrid.Children.Add(_particleCanvas);

        Content = _mainGrid;
    }

    private Border CreateHeaderSection()
    {
        var header = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromArgb(100, 0, 150, 255),
                Color.FromArgb(50, 0, 100, 200),
                90),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 0, 200, 255)),
            BorderThickness = new Thickness(0, 0, 0, 2),
            Padding = new Thickness(20, 15),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(100, 0, 150, 255),
                ShadowDepth = 0,
                BlurRadius = 20
            }
        };

        var stackPanel = new StackPanel();

        var titleBlock = new TextBlock
        {
            Text = "HOLOGRAPHIC LOCATION TRACKING",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Left,
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(150, 0, 255, 255),
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };

        var controlsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 10, 0, 0)
        };

        var characterSelector = CreateCharacterSelector();
        var trackingModeSelector = CreateTrackingModeSelector();
        var mapViewSelector = CreateMapViewSelector();

        controlsPanel.Children.Add(characterSelector);
        controlsPanel.Children.Add(trackingModeSelector);
        controlsPanel.Children.Add(mapViewSelector);

        stackPanel.Children.Add(titleBlock);
        stackPanel.Children.Add(controlsPanel);

        header.Child = stackPanel;
        return header;
    }

    private ComboBox CreateCharacterSelector()
    {
        var comboBox = new ComboBox
        {
            Width = 150,
            Margin = new Thickness(0, 0, 15, 0),
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 50, 100)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 0, 200, 255))
        };

        comboBox.Items.Add("Main Character");
        comboBox.Items.Add("Alt Character");
        comboBox.Items.Add("Trading Alt");
        comboBox.SelectedIndex = 0;

        return comboBox;
    }

    private ComboBox CreateTrackingModeSelector()
    {
        var comboBox = new ComboBox
        {
            Width = 120,
            Margin = new Thickness(0, 0, 15, 0),
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 50, 100)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 0, 200, 255))
        };

        comboBox.Items.Add("Real Time");
        comboBox.Items.Add("Historical");
        comboBox.Items.Add("Predictive");
        comboBox.SelectedIndex = 0;

        return comboBox;
    }

    private ComboBox CreateMapViewSelector()
    {
        var comboBox = new ComboBox
        {
            Width = 100,
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 50, 100)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 0, 200, 255))
        };

        comboBox.Items.Add("Galaxy");
        comboBox.Items.Add("Region");
        comboBox.Items.Add("System");
        comboBox.SelectedIndex = 0;

        return comboBox;
    }

    private TabControl CreateTabControl()
    {
        var tabControl = new TabControl
        {
            Background = Brushes.Transparent,
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 200, 255)),
            BorderThickness = new Thickness(1)
        };

        tabControl.Items.Add(CreateMapTab());
        tabControl.Items.Add(CreateHistoryTab());
        tabControl.Items.Add(CreateSecurityTab());
        tabControl.Items.Add(CreateRoutesTab());

        return tabControl;
    }

    private TabItem CreateMapTab()
    {
        var tab = new TabItem
        {
            Header = "3D MAP",
            Background = new SolidColorBrush(Color.FromArgb(80, 0, 100, 200)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };

        _mapCanvas = new Canvas
        {
            Background = new RadialGradientBrush(
                Color.FromArgb(50, 0, 100, 200),
                Color.FromArgb(20, 0, 50, 100))
        };

        InitializeMapVisualization();

        tab.Content = _mapCanvas;
        return tab;
    }

    private TabItem CreateHistoryTab()
    {
        var tab = new TabItem
        {
            Header = "JUMP HISTORY",
            Background = new SolidColorBrush(Color.FromArgb(80, 0, 100, 200)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };

        var scrollViewer = new ScrollViewer
        {
            Background = Brushes.Transparent,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        var stackPanel = new StackPanel
        {
            Margin = new Thickness(20)
        };

        var historyItems = CreateLocationHistoryItems();
        foreach (var item in historyItems)
        {
            stackPanel.Children.Add(item);
        }

        scrollViewer.Content = stackPanel;
        tab.Content = scrollViewer;
        return tab;
    }

    private TabItem CreateSecurityTab()
    {
        var tab = new TabItem
        {
            Header = "SECURITY STATUS",
            Background = new SolidColorBrush(Color.FromArgb(80, 0, 100, 200)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };

        var grid = new Grid
        {
            Margin = new Thickness(20)
        };

        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var securityOverview = CreateSecurityOverview();
        Grid.SetRow(securityOverview, 0);
        grid.Children.Add(securityOverview);

        var dangerZones = CreateDangerZonesDisplay();
        Grid.SetRow(dangerZones, 1);
        grid.Children.Add(dangerZones);

        tab.Content = grid;
        return tab;
    }

    private TabItem CreateRoutesTab()
    {
        var tab = new TabItem
        {
            Header = "ROUTE OPTIMIZATION",
            Background = new SolidColorBrush(Color.FromArgb(80, 0, 100, 200)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };

        var grid = new Grid
        {
            Margin = new Thickness(20)
        };

        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var routeControls = CreateRouteControls();
        Grid.SetRow(routeControls, 0);
        grid.Children.Add(routeControls);

        var routeVisualization = CreateRouteVisualization();
        Grid.SetRow(routeVisualization, 1);
        grid.Children.Add(routeVisualization);

        tab.Content = grid;
        return tab;
    }

    private void InitializeMapVisualization()
    {
        CreateGalaxyNodes();
        CreateStarGates();
        CreateCurrentLocationIndicator();
        CreateLocationTrail();
    }

    private void CreateGalaxyNodes()
    {
        var regions = new[]
        {
            new { Name = "The Forge", X = 200, Y = 150, Security = 1.0 },
            new { Name = "Domain", X = 350, Y = 200, Security = 0.8 },
            new { Name = "Delve", X = 150, Y = 350, Security = 0.0 },
            new { Name = "Pure Blind", X = 450, Y = 100, Security = 0.2 },
            new { Name = "Curse", X = 500, Y = 300, Security = -0.3 }
        };

        foreach (var region in regions)
        {
            var node = CreateRegionNode(region.Name, region.X, region.Y, region.Security);
            _mapCanvas.Children.Add(node);
        }
    }

    private Border CreateRegionNode(string name, int x, int y, double security)
    {
        var color = GetSecurityColor(security);
        var border = new Border
        {
            Width = 40,
            Height = 40,
            Background = new RadialGradientBrush(
                Color.FromArgb(150, color.R, color.G, color.B),
                Color.FromArgb(50, color.R, color.G, color.B)),
            BorderBrush = new SolidColorBrush(color),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(20),
            Effect = new DropShadowEffect
            {
                Color = color,
                ShadowDepth = 0,
                BlurRadius = 15
            }
        };

        Canvas.SetLeft(border, x);
        Canvas.SetTop(border, y);

        var textBlock = new TextBlock
        {
            Text = name,
            FontSize = 8,
            Foreground = new SolidColorBrush(Colors.White),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        border.Child = textBlock;

        border.MouseEnter += (s, e) =>
        {
            var scaleTransform = new ScaleTransform(1.2, 1.2);
            border.RenderTransform = scaleTransform;
            border.RenderTransformOrigin = new Point(0.5, 0.5);
        };

        border.MouseLeave += (s, e) =>
        {
            border.RenderTransform = null;
        };

        return border;
    }

    private void CreateStarGates()
    {
        var connections = new[]
        {
            new { From = new Point(200, 150), To = new Point(350, 200) },
            new { From = new Point(350, 200), To = new Point(150, 350) },
            new { From = new Point(450, 100), To = new Point(500, 300) },
            new { From = new Point(200, 150), To = new Point(450, 100) }
        };

        foreach (var connection in connections)
        {
            var line = CreateStarGateLine(connection.From, connection.To);
            _mapCanvas.Children.Add(line);
        }
    }

    private Line CreateStarGateLine(Point from, Point to)
    {
        var line = new Line
        {
            X1 = from.X + 20,
            Y1 = from.Y + 20,
            X2 = to.X + 20,
            Y2 = to.Y + 20,
            Stroke = new SolidColorBrush(Color.FromArgb(100, 0, 200, 255)),
            StrokeThickness = 2,
            StrokeDashArray = new DoubleCollection { 5, 5 },
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(80, 0, 200, 255),
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };

        return line;
    }

    private void CreateCurrentLocationIndicator()
    {
        _currentLocationIndicator = new Border
        {
            Width = 60,
            Height = 60,
            Background = new RadialGradientBrush(
                Color.FromArgb(200, 255, 255, 0),
                Color.FromArgb(50, 255, 255, 0)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
            BorderThickness = new Thickness(3),
            CornerRadius = new CornerRadius(30),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(200, 255, 255, 0),
                ShadowDepth = 0,
                BlurRadius = 20
            }
        };

        var textBlock = new TextBlock
        {
            Text = "YOU",
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.Black),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        _currentLocationIndicator.Child = textBlock;

        Canvas.SetLeft(_currentLocationIndicator, 190);
        Canvas.SetTop(_currentLocationIndicator, 140);
        _mapCanvas.Children.Add(_currentLocationIndicator);

        StartLocationPulseAnimation();
    }

    private void CreateLocationTrail()
    {
        var trailPoints = new[]
        {
            new Point(350, 200),
            new Point(280, 175),
            new Point(220, 160),
            new Point(200, 150)
        };

        for (int i = 0; i < trailPoints.Length - 1; i++)
        {
            var opacity = (double)(i + 1) / trailPoints.Length * 0.8;
            var trail = CreateTrailSegment(trailPoints[i], trailPoints[i + 1], opacity);
            _mapCanvas.Children.Add(trail);
        }
    }

    private Line CreateTrailSegment(Point from, Point to, double opacity)
    {
        var line = new Line
        {
            X1 = from.X + 20,
            Y1 = from.Y + 20,
            X2 = to.X + 20,
            Y2 = to.Y + 20,
            Stroke = new SolidColorBrush(Color.FromArgb((byte)(255 * opacity), 0, 255, 100)),
            StrokeThickness = 4,
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb((byte)(150 * opacity), 0, 255, 100),
                ShadowDepth = 0,
                BlurRadius = 6
            }
        };

        return line;
    }

    private List<Border> CreateLocationHistoryItems()
    {
        var items = new List<Border>();
        var locations = new[]
        {
            new { System = "Jita", Region = "The Forge", Time = "2 minutes ago", Security = 1.0, Activity = "Trading" },
            new { System = "Amarr", Region = "Domain", Time = "1 hour ago", Security = 0.9, Activity = "Mission Running" },
            new { System = "1DH-SX", Region = "Delve", Time = "3 hours ago", Security = 0.0, Activity = "PvP Fleet" },
            new { System = "VFK-IV", Region = "Pure Blind", Time = "1 day ago", Security = 0.2, Activity = "Ratting" },
            new { System = "Doril", Region = "Curse", Time = "2 days ago", Security = -0.3, Activity = "Exploration" }
        };

        foreach (var location in locations)
        {
            var item = CreateHistoryItem(location.System, location.Region, location.Time, location.Security, location.Activity);
            items.Add(item);
        }

        return items;
    }

    private Border CreateHistoryItem(string system, string region, string time, double security, string activity)
    {
        var border = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromArgb(60, 0, 100, 200),
                Color.FromArgb(30, 0, 50, 100),
                90),
            BorderBrush = new SolidColorBrush(GetSecurityColor(security)),
            BorderThickness = new Thickness(1, 1, 4, 1),
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(15, 10),
            CornerRadius = new CornerRadius(5)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var locationPanel = new StackPanel();
        var systemText = new TextBlock
        {
            Text = system,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };
        var regionText = new TextBlock
        {
            Text = region,
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255))
        };

        locationPanel.Children.Add(systemText);
        locationPanel.Children.Add(regionText);
        Grid.SetColumn(locationPanel, 0);
        grid.Children.Add(locationPanel);

        var timeText = new TextBlock
        {
            Text = time,
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(timeText, 1);
        grid.Children.Add(timeText);

        var activityText = new TextBlock
        {
            Text = activity,
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 0, 255, 150)),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(activityText, 2);
        grid.Children.Add(activityText);

        border.Child = grid;
        return border;
    }

    private StackPanel CreateSecurityOverview()
    {
        var panel = new StackPanel
        {
            Margin = new Thickness(0, 0, 0, 20)
        };

        var title = new TextBlock
        {
            Text = "SECURITY STATUS OVERVIEW",
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            Margin = new Thickness(0, 0, 0, 15)
        };

        var statusGrid = new Grid();
        statusGrid.ColumnDefinitions.Add(new ColumnDefinition());
        statusGrid.ColumnDefinitions.Add(new ColumnDefinition());
        statusGrid.ColumnDefinitions.Add(new ColumnDefinition());

        var highSecPanel = CreateSecurityPanel("HIGH SEC", "0.5 - 1.0", Colors.Green, "95%");
        var lowSecPanel = CreateSecurityPanel("LOW SEC", "0.1 - 0.4", Colors.Orange, "3%");
        var nullSecPanel = CreateSecurityPanel("NULL SEC", "-1.0 - 0.0", Colors.Red, "2%");

        Grid.SetColumn(highSecPanel, 0);
        Grid.SetColumn(lowSecPanel, 1);
        Grid.SetColumn(nullSecPanel, 2);

        statusGrid.Children.Add(highSecPanel);
        statusGrid.Children.Add(lowSecPanel);
        statusGrid.Children.Add(nullSecPanel);

        panel.Children.Add(title);
        panel.Children.Add(statusGrid);

        return panel;
    }

    private Border CreateSecurityPanel(string title, string range, Color color, string percentage)
    {
        var border = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromArgb(80, color.R, color.G, color.B),
                Color.FromArgb(40, color.R, color.G, color.B),
                90),
            BorderBrush = new SolidColorBrush(color),
            BorderThickness = new Thickness(2),
            Margin = new Thickness(5),
            Padding = new Thickness(15, 10),
            CornerRadius = new CornerRadius(8)
        };

        var stackPanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var titleText = new TextBlock
        {
            Text = title,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(color),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var rangeText = new TextBlock
        {
            Text = range,
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var percentageText = new TextBlock
        {
            Text = percentage,
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0)
        };

        stackPanel.Children.Add(titleText);
        stackPanel.Children.Add(rangeText);
        stackPanel.Children.Add(percentageText);

        border.Child = stackPanel;
        return border;
    }

    private ScrollViewer CreateDangerZonesDisplay()
    {
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        var stackPanel = new StackPanel();

        var title = new TextBlock
        {
            Text = "DANGER ZONES",
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 100, 100)),
            Margin = new Thickness(0, 0, 0, 10)
        };

        stackPanel.Children.Add(title);

        var dangerZones = new[]
        {
            new { System = "Rancer", Threat = "Gate Camp", Level = "EXTREME" },
            new { System = "Tama", Threat = "Solo PvP", Level = "HIGH" },
            new { System = "Amamake", Threat = "Faction Warfare", Level = "HIGH" },
            new { System = "EC-P8R", Threat = "Bubble Camp", Level = "MEDIUM" }
        };

        foreach (var zone in dangerZones)
        {
            var item = CreateDangerZoneItem(zone.System, zone.Threat, zone.Level);
            stackPanel.Children.Add(item);
        }

        scrollViewer.Content = stackPanel;
        return scrollViewer;
    }

    private Border CreateDangerZoneItem(string system, string threat, string level)
    {
        var levelColor = level switch
        {
            "EXTREME" => Colors.Red,
            "HIGH" => Colors.Orange,
            "MEDIUM" => Colors.Yellow,
            _ => Colors.Gray
        };

        var border = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromArgb(60, levelColor.R, levelColor.G, levelColor.B),
                Color.FromArgb(30, levelColor.R, levelColor.G, levelColor.B),
                90),
            BorderBrush = new SolidColorBrush(levelColor),
            BorderThickness = new Thickness(1, 1, 4, 1),
            Margin = new Thickness(0, 0, 0, 8),
            Padding = new Thickness(12, 8),
            CornerRadius = new CornerRadius(5)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var systemText = new TextBlock
        {
            Text = system,
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(systemText, 0);

        var threatText = new TextBlock
        {
            Text = threat,
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(threatText, 1);

        var levelText = new TextBlock
        {
            Text = level,
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(levelColor),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(levelText, 2);

        grid.Children.Add(systemText);
        grid.Children.Add(threatText);
        grid.Children.Add(levelText);

        border.Child = grid;
        return border;
    }

    private StackPanel CreateRouteControls()
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 20)
        };

        var fromLabel = new TextBlock
        {
            Text = "FROM:",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };

        var fromCombo = new ComboBox
        {
            Width = 120,
            Margin = new Thickness(0, 0, 20, 0),
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 50, 100)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };
        fromCombo.Items.Add("Jita");
        fromCombo.Items.Add("Amarr");
        fromCombo.Items.Add("Dodixie");
        fromCombo.SelectedIndex = 0;

        var toLabel = new TextBlock
        {
            Text = "TO:",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };

        var toCombo = new ComboBox
        {
            Width = 120,
            Margin = new Thickness(0, 0, 20, 0),
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 50, 100)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };
        toCombo.Items.Add("Jita");
        toCombo.Items.Add("Amarr");
        toCombo.Items.Add("Dodixie");
        toCombo.SelectedIndex = 1;

        var optimizeButton = new Button
        {
            Content = "OPTIMIZE ROUTE",
            Width = 120,
            Height = 30,
            Background = new LinearGradientBrush(
                Color.FromArgb(150, 0, 150, 255),
                Color.FromArgb(100, 0, 100, 200),
                90),
            Foreground = new SolidColorBrush(Colors.White),
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 0, 200, 255)),
            BorderThickness = new Thickness(1),
            FontSize = 10,
            FontWeight = FontWeights.Bold
        };

        panel.Children.Add(fromLabel);
        panel.Children.Add(fromCombo);
        panel.Children.Add(toLabel);
        panel.Children.Add(toCombo);
        panel.Children.Add(optimizeButton);

        return panel;
    }

    private Canvas CreateRouteVisualization()
    {
        var canvas = new Canvas
        {
            Background = new RadialGradientBrush(
                Color.FromArgb(40, 0, 100, 200),
                Color.FromArgb(20, 0, 50, 100))
        };

        CreateRouteNodes(canvas);
        CreateOptimalRoute(canvas);

        return canvas;
    }

    private void CreateRouteNodes(Canvas canvas)
    {
        var nodes = new[]
        {
            new { Name = "Jita", X = 100, Y = 100 },
            new { Name = "Perimeter", X = 150, Y = 120 },
            new { Name = "Sobaseki", X = 200, Y = 140 },
            new { Name = "Amarr", X = 400, Y = 250 }
        };

        foreach (var node in nodes)
        {
            var border = new Border
            {
                Width = 30,
                Height = 30,
                Background = new RadialGradientBrush(
                    Color.FromArgb(150, 0, 200, 255),
                    Color.FromArgb(50, 0, 100, 200)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 200, 255)),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(15)
            };

            var text = new TextBlock
            {
                Text = node.Name,
                FontSize = 8,
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            border.Child = text;

            Canvas.SetLeft(border, node.X);
            Canvas.SetTop(border, node.Y);
            canvas.Children.Add(border);
        }
    }

    private void CreateOptimalRoute(Canvas canvas)
    {
        var routePoints = new[]
        {
            new Point(115, 115),
            new Point(165, 135),
            new Point(215, 155),
            new Point(415, 265)
        };

        for (int i = 0; i < routePoints.Length - 1; i++)
        {
            var line = new Line
            {
                X1 = routePoints[i].X,
                Y1 = routePoints[i].Y,
                X2 = routePoints[i + 1].X,
                Y2 = routePoints[i + 1].Y,
                Stroke = new SolidColorBrush(Color.FromArgb(200, 0, 255, 100)),
                StrokeThickness = 3,
                Effect = new DropShadowEffect
                {
                    Color = Color.FromArgb(150, 0, 255, 100),
                    ShadowDepth = 0,
                    BlurRadius = 8
                }
            };

            canvas.Children.Add(line);
        }
    }

    private void InitializeTimers()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };
        _animationTimer.Tick += OnAnimationTick;

        _trackingUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _trackingUpdateTimer.Tick += OnTrackingUpdate;
    }

    private void InitializeDefaults()
    {
        LocationHistory = new ObservableCollection<HoloLocationEntry>();
        CurrentLocation = new HoloLocationEntry
        {
            SystemName = "Jita",
            RegionName = "The Forge",
            SecurityStatus = 0.9,
            Timestamp = DateTime.Now,
            Activity = "Trading"
        };
    }

    #endregion

    #region Animation Methods

    private void StartLocationPulseAnimation()
    {
        if (_currentLocationIndicator == null || !EnableAnimations) return;

        var scaleAnimation = new DoubleAnimationUsingKeyFrames();
        scaleAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        scaleAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1.2, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1))));
        scaleAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2))));

        scaleAnimation.RepeatBehavior = RepeatBehavior.Forever;

        var scaleTransform = new ScaleTransform();
        _currentLocationIndicator.RenderTransform = scaleTransform;
        _currentLocationIndicator.RenderTransformOrigin = new Point(0.5, 0.5);

        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
    }

    #endregion

    #region Particle System

    private void CreateParticleSystem()
    {
        if (!EnableParticleEffects) return;

        var dataParticles = new ParticleSystem(_particleCanvas)
        {
            ParticleColor = Color.FromArgb(150, 0, 200, 255),
            ParticleSize = 2,
            ParticleSpeed = 1.5,
            EmissionRate = 5,
            ParticleLifespan = TimeSpan.FromSeconds(3)
        };

        var jumpParticles = new ParticleSystem(_particleCanvas)
        {
            ParticleColor = Color.FromArgb(150, 0, 255, 100),
            ParticleSize = 3,
            ParticleSpeed = 2.0,
            EmissionRate = 3,
            ParticleLifespan = TimeSpan.FromSeconds(4)
        };

        _particleSystems.Add(dataParticles);
        _particleSystems.Add(jumpParticles);
    }

    private void UpdateParticles()
    {
        if (!EnableParticleEffects) return;

        foreach (var system in _particleSystems)
        {
            system.Update();

            if (_random.NextDouble() < 0.3)
            {
                var x = _random.NextDouble() * ActualWidth;
                var y = _random.NextDouble() * ActualHeight;
                system.EmitParticle(new Point(x, y));
            }
        }
    }

    #endregion

    #region Helper Methods

    private Color GetSecurityColor(double security)
    {
        return security switch
        {
            >= 0.5 => Colors.Green,
            >= 0.1 => Colors.Orange,
            >= 0.0 => Colors.Red,
            _ => Colors.DarkRed
        };
    }

    private Color GetEVEColor(EVEColorScheme scheme)
    {
        return scheme switch
        {
            EVEColorScheme.ElectricBlue => Color.FromRgb(0, 200, 255),
            EVEColorScheme.CorporationGold => Color.FromRgb(255, 215, 0),
            EVEColorScheme.AllianceGreen => Color.FromRgb(0, 255, 100),
            EVEColorScheme.ConcordRed => Color.FromRgb(255, 50, 50),
            _ => Color.FromRgb(0, 200, 255)
        };
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableAnimations)
        {
            _animationTimer.Start();
        }

        if (TrackingMode == LocationTrackingMode.RealTime)
        {
            _trackingUpdateTimer.Start();
        }

        CreateParticleSystem();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        _trackingUpdateTimer?.Stop();

        foreach (var system in _particleSystems)
        {
            system.Dispose();
        }
        _particleSystems.Clear();
    }

    private void OnAnimationTick(object sender, EventArgs e)
    {
        UpdateParticles();
    }

    private void OnTrackingUpdate(object sender, EventArgs e)
    {
        // Simulate location updates in real-time mode
        if (TrackingMode == LocationTrackingMode.RealTime)
        {
            UpdateCurrentLocation();
        }
    }

    private void UpdateCurrentLocation()
    {
        // This would typically connect to ESI API for real location data
        // For now, simulate movement
        if (CurrentLocation != null)
        {
            CurrentLocation.Timestamp = DateTime.Now;
        }
    }

    #endregion

    #region Property Changed Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloLocationTracking control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloLocationTracking control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnLocationHistoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloLocationTracking control)
        {
            control.UpdateLocationHistory();
        }
    }

    private static void OnCurrentLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloLocationTracking control)
        {
            control.UpdateCurrentLocationDisplay();
        }
    }

    private static void OnSelectedCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloLocationTracking control)
        {
            control.LoadCharacterLocationData();
        }
    }

    private static void OnTrackingModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloLocationTracking control)
        {
            control.UpdateTrackingMode();
        }
    }

    private static void OnMapViewModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloLocationTracking control)
        {
            control.UpdateMapView();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloLocationTracking control)
        {
            if ((bool)e.NewValue)
            {
                control._animationTimer?.Start();
                control.StartLocationPulseAnimation();
            }
            else
            {
                control._animationTimer?.Stop();
            }
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloLocationTracking control)
        {
            if (!(bool)e.NewValue)
            {
                foreach (var system in control._particleSystems)
                {
                    system.ClearParticles();
                }
            }
        }
    }

    private static void OnShowSecurityStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloLocationTracking control)
        {
            control.UpdateSecurityDisplay();
        }
    }

    private static void OnShowJumpHistoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloLocationTracking control)
        {
            control.UpdateJumpHistoryDisplay();
        }
    }

    private static void OnShowRouteOptimizationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloLocationTracking control)
        {
            control.UpdateRouteDisplay();
        }
    }

    private static void OnAutoCenterOnCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloLocationTracking control)
        {
            control.UpdateMapCentering();
        }
    }

    private static void OnTrackingRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloLocationTracking control)
        {
            control.UpdateTrackingRange();
        }
    }

    private static void OnShowDangerZonesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloLocationTracking control)
        {
            control.UpdateDangerZoneDisplay();
        }
    }

    private void UpdateHolographicEffects()
    {
        // Update holographic intensity across all visual elements
    }

    private void UpdateColorScheme()
    {
        var color = GetEVEColor(EVEColorScheme);
        // Apply color scheme to all relevant elements
    }

    private void UpdateLocationHistory()
    {
        // Refresh location history display
    }

    private void UpdateCurrentLocationDisplay()
    {
        // Update current location indicator
    }

    private void LoadCharacterLocationData()
    {
        // Load location data for selected character
    }

    private void UpdateTrackingMode()
    {
        if (TrackingMode == LocationTrackingMode.RealTime)
        {
            _trackingUpdateTimer?.Start();
        }
        else
        {
            _trackingUpdateTimer?.Stop();
        }
    }

    private void UpdateMapView()
    {
        // Update map view based on selected mode
    }

    private void UpdateSecurityDisplay()
    {
        // Update security status visualization
    }

    private void UpdateJumpHistoryDisplay()
    {
        // Update jump history visualization
    }

    private void UpdateRouteDisplay()
    {
        // Update route optimization display
    }

    private void UpdateMapCentering()
    {
        // Update map centering behavior
    }

    private void UpdateTrackingRange()
    {
        // Update tracking range visualization
    }

    private void UpdateDangerZoneDisplay()
    {
        // Update danger zone visualization
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void AdaptToHardware(HardwareCapability capability)
    {
        switch (capability)
        {
            case HardwareCapability.Low:
                EnableAnimations = false;
                EnableParticleEffects = false;
                break;
            case HardwareCapability.Medium:
                EnableAnimations = true;
                EnableParticleEffects = false;
                break;
            case HardwareCapability.High:
                EnableAnimations = true;
                EnableParticleEffects = true;
                break;
        }
    }

    #endregion
}

#region Supporting Types

public class HoloLocationEntry
{
    public string SystemName { get; set; } = string.Empty;
    public string RegionName { get; set; } = string.Empty;
    public double SecurityStatus { get; set; }
    public DateTime Timestamp { get; set; }
    public string Activity { get; set; } = string.Empty;
    public Point3D Coordinates { get; set; }
    public string StationName { get; set; } = string.Empty;
}

public enum LocationTrackingMode
{
    RealTime,
    Historical,
    Predictive
}

public enum MapViewMode
{
    Galaxy,
    Region,
    Constellation,
    System
}

public class ParticleSystem : IDisposable
{
    private readonly Canvas _canvas;
    private readonly List<Particle> _particles = new();
    private readonly Random _random = new();

    public Color ParticleColor { get; set; } = Colors.Blue;
    public double ParticleSize { get; set; } = 2;
    public double ParticleSpeed { get; set; } = 1;
    public int EmissionRate { get; set; } = 5;
    public TimeSpan ParticleLifespan { get; set; } = TimeSpan.FromSeconds(3);

    public ParticleSystem(Canvas canvas)
    {
        _canvas = canvas;
    }

    public void EmitParticle(Point position)
    {
        var particle = new Particle
        {
            Position = position,
            Velocity = new Vector(_random.NextDouble() * 4 - 2, _random.NextDouble() * 4 - 2),
            CreationTime = DateTime.Now,
            Element = new Ellipse
            {
                Width = ParticleSize,
                Height = ParticleSize,
                Fill = new SolidColorBrush(ParticleColor)
            }
        };

        Canvas.SetLeft(particle.Element, position.X);
        Canvas.SetTop(particle.Element, position.Y);
        _canvas.Children.Add(particle.Element);
        _particles.Add(particle);
    }

    public void Update()
    {
        var particlesToRemove = new List<Particle>();

        foreach (var particle in _particles)
        {
            if (DateTime.Now - particle.CreationTime > ParticleLifespan)
            {
                particlesToRemove.Add(particle);
                continue;
            }

            particle.Position = Point.Add(particle.Position, particle.Velocity);
            Canvas.SetLeft(particle.Element, particle.Position.X);
            Canvas.SetTop(particle.Element, particle.Position.Y);

            var age = (DateTime.Now - particle.CreationTime).TotalMilliseconds / ParticleLifespan.TotalMilliseconds;
            var opacity = 1.0 - age;
            particle.Element.Opacity = Math.Max(0, opacity);
        }

        foreach (var particle in particlesToRemove)
        {
            _canvas.Children.Remove(particle.Element);
            _particles.Remove(particle);
        }
    }

    public void ClearParticles()
    {
        foreach (var particle in _particles)
        {
            _canvas.Children.Remove(particle.Element);
        }
        _particles.Clear();
    }

    public void Dispose()
    {
        ClearParticles();
    }
}

public class Particle
{
    public Point Position { get; set; }
    public Vector Velocity { get; set; }
    public DateTime CreationTime { get; set; }
    public UIElement Element { get; set; } = null!;
}

public enum EVEColorScheme
{
    ElectricBlue,
    CorporationGold,
    AllianceGreen,
    ConcordRed
}

public enum HardwareCapability
{
    Low,
    Medium,
    High
}

public interface IAnimationIntensityTarget
{
    double HolographicIntensity { get; set; }
}

public interface IAdaptiveControl
{
    void AdaptToHardware(HardwareCapability capability);
}

#endregion