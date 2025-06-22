// ==========================================================================
// HoloMarketAlerts.cs - Holographic Market Alerts System
// ==========================================================================
// Advanced market alert system featuring real-time notifications,
// price monitoring, EVE-style alert management, and holographic alert animations.
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
/// Holographic market alerts with real-time monitoring and animated notification system
/// </summary>
public class HoloMarketAlerts : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloMarketAlerts),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloMarketAlerts),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty MarketAlertsProperty =
        DependencyProperty.Register(nameof(MarketAlerts), typeof(ObservableCollection<HoloMarketAlert>), typeof(HoloMarketAlerts),
            new PropertyMetadata(null, OnMarketAlertsChanged));

    public static readonly DependencyProperty ActiveNotificationsProperty =
        DependencyProperty.Register(nameof(ActiveNotifications), typeof(ObservableCollection<HoloAlertNotification>), typeof(HoloMarketAlerts),
            new PropertyMetadata(null, OnActiveNotificationsChanged));

    public static readonly DependencyProperty AlertModeProperty =
        DependencyProperty.Register(nameof(AlertMode), typeof(MarketAlertMode), typeof(HoloMarketAlerts),
            new PropertyMetadata(MarketAlertMode.All, OnAlertModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloMarketAlerts),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloMarketAlerts),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty EnableSoundAlertsProperty =
        DependencyProperty.Register(nameof(EnableSoundAlerts), typeof(bool), typeof(HoloMarketAlerts),
            new PropertyMetadata(true, OnEnableSoundAlertsChanged));

    public static readonly DependencyProperty ShowAlertHistoryProperty =
        DependencyProperty.Register(nameof(ShowAlertHistory), typeof(bool), typeof(HoloMarketAlerts),
            new PropertyMetadata(true, OnShowAlertHistoryChanged));

    public static readonly DependencyProperty AutoClearNotificationsProperty =
        DependencyProperty.Register(nameof(AutoClearNotifications), typeof(bool), typeof(HoloMarketAlerts),
            new PropertyMetadata(true, OnAutoClearNotificationsChanged));

    public static readonly DependencyProperty NotificationTimeoutProperty =
        DependencyProperty.Register(nameof(NotificationTimeout), typeof(TimeSpan), typeof(HoloMarketAlerts),
            new PropertyMetadata(TimeSpan.FromSeconds(10), OnNotificationTimeoutChanged));

    public static readonly DependencyProperty UpdateIntervalProperty =
        DependencyProperty.Register(nameof(UpdateInterval), typeof(TimeSpan), typeof(HoloMarketAlerts),
            new PropertyMetadata(TimeSpan.FromSeconds(1), OnUpdateIntervalChanged));

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

    public ObservableCollection<HoloMarketAlert> MarketAlerts
    {
        get => (ObservableCollection<HoloMarketAlert>)GetValue(MarketAlertsProperty);
        set => SetValue(MarketAlertsProperty, value);
    }

    public ObservableCollection<HoloAlertNotification> ActiveNotifications
    {
        get => (ObservableCollection<HoloAlertNotification>)GetValue(ActiveNotificationsProperty);
        set => SetValue(ActiveNotificationsProperty, value);
    }

    public MarketAlertMode AlertMode
    {
        get => (MarketAlertMode)GetValue(AlertModeProperty);
        set => SetValue(AlertModeProperty, value);
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

    public bool EnableSoundAlerts
    {
        get => (bool)GetValue(EnableSoundAlertsProperty);
        set => SetValue(EnableSoundAlertsProperty, value);
    }

    public bool ShowAlertHistory
    {
        get => (bool)GetValue(ShowAlertHistoryProperty);
        set => SetValue(ShowAlertHistoryProperty, value);
    }

    public bool AutoClearNotifications
    {
        get => (bool)GetValue(AutoClearNotificationsProperty);
        set => SetValue(AutoClearNotificationsProperty, value);
    }

    public TimeSpan NotificationTimeout
    {
        get => (TimeSpan)GetValue(NotificationTimeoutProperty);
        set => SetValue(NotificationTimeoutProperty, value);
    }

    public TimeSpan UpdateInterval
    {
        get => (TimeSpan)GetValue(UpdateIntervalProperty);
        set => SetValue(UpdateIntervalProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloMarketAlert> AlertTriggered;
    public event EventHandler<HoloAlertNotification> NotificationCreated;
    public event EventHandler<HoloAlertNotification> NotificationCleared;

    #endregion

    #region Fields

    private Grid _mainGrid;
    private Canvas _notificationCanvas;
    private Canvas _particleCanvas;
    private DataGrid _alertsGrid;
    private StackPanel _notificationPanel;
    private Border _statusPanel;
    private TextBlock _statusText;
    
    private DispatcherTimer _updateTimer;
    private DispatcherTimer _animationTimer;
    private DispatcherTimer _notificationTimer;
    
    private readonly Random _random = new();
    private List<UIElement> _alertIndicators;
    private List<UIElement> _particles;
    private List<UIElement> _notificationElements;
    
    // Alert monitoring
    private readonly Dictionary<string, double> _lastPrices = new();
    private readonly Dictionary<string, DateTime> _lastAlertTimes = new();
    private int _activeAlertCount = 0;
    private int _triggeredAlertsToday = 0;
    
    // Animation state
    private double _animationProgress = 0.0;
    private bool _isAnimating = false;
    private readonly Queue<HoloAlertNotification> _notificationQueue = new();
    
    // Performance optimization
    private bool _isInSimplifiedMode = false;
    private int _maxNotifications = 10;
    private int _maxParticles = 20;

    #endregion

    #region Constructor

    public HoloMarketAlerts()
    {
        InitializeComponent();
        InitializeAlertSystem();
        SetupEventHandlers();
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 800;
        Height = 600;
        Background = new SolidColorBrush(Color.FromArgb(30, 0, 50, 100));
        
        // Create main layout
        _mainGrid = new Grid();
        Content = _mainGrid;

        // Define layout structure
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header & Status
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Alerts grid
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(200) }); // Notifications

        CreateHeader();
        CreateStatusPanel();
        CreateAlertsGrid();
        CreateNotificationArea();
        CreateParticleLayer();
    }

    private void CreateHeader()
    {
        var headerPanel = new Border
        {
            Height = 50,
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
            Text = "MARKET ALERT SYSTEM",
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };

        var controlsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        var modeCombo = new ComboBox
        {
            Width = 120,
            SelectedIndex = 0,
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 50, 100)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 0, 150, 255)),
            Margin = new Thickness(0, 0, 10, 0)
        };

        modeCombo.Items.Add("All Alerts");
        modeCombo.Items.Add("Price Only");
        modeCombo.Items.Add("Volume Only");
        modeCombo.Items.Add("Critical");
        modeCombo.SelectionChanged += (s, e) => AlertMode = (MarketAlertMode)modeCombo.SelectedIndex;

        var addAlertButton = CreateHoloButton("ADD ALERT", ShowAddAlertDialog);
        var clearAllButton = CreateHoloButton("CLEAR ALL", ClearAllNotifications);

        controlsPanel.Children.Add(modeCombo);
        controlsPanel.Children.Add(addAlertButton);
        controlsPanel.Children.Add(clearAllButton);

        headerContent.Children.Add(titleText);
        headerContent.Children.Add(controlsPanel);
        headerPanel.Child = headerContent;

        Grid.SetRow(headerPanel, 0);
        _mainGrid.Children.Add(headerPanel);
    }

    private Button CreateHoloButton(string text, Action onClick)
    {
        var button = new Button
        {
            Content = text,
            Width = 90,
            Height = 25,
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 150, 255)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 0, 200, 255)),
            BorderThickness = new Thickness(1),
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            CornerRadius = new CornerRadius(3),
            Margin = new Thickness(5, 0, 0, 0)
        };

        button.Click += (s, e) => onClick();
        return button;
    }

    private void CreateStatusPanel()
    {
        _statusPanel = new Border
        {
            Height = 40,
            Background = new SolidColorBrush(Color.FromArgb(50, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 150, 255)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(5, 5, 5, 0),
            CornerRadius = new CornerRadius(3)
        };

        var statusContent = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.SpaceBetween,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(15, 5, 15, 5)
        };

        _statusText = new TextBlock
        {
            Text = "ALERT SYSTEM ACTIVE",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 100))
        };

        var statusIndicators = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        var activeIndicator = CreateStatusIndicator("ACTIVE", "0", Colors.LimeGreen);
        var triggeredIndicator = CreateStatusIndicator("TODAY", "0", Colors.Orange);
        var queuedIndicator = CreateStatusIndicator("QUEUED", "0", Colors.Cyan);

        statusIndicators.Children.Add(activeIndicator);
        statusIndicators.Children.Add(triggeredIndicator);
        statusIndicators.Children.Add(queuedIndicator);

        statusContent.Children.Add(_statusText);
        statusContent.Children.Add(statusIndicators);
        _statusPanel.Child = statusContent;

        Grid.SetRow(_statusPanel, 0);
        _mainGrid.Children.Add(_statusPanel);
    }

    private StackPanel CreateStatusIndicator(string label, string value, Color color)
    {
        var indicator = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10, 0, 10, 0)
        };

        var labelText = new TextBlock
        {
            Text = $"{label}: ",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 150, 200, 255)),
            Margin = new Thickness(0, 0, 5, 0)
        };

        var valueText = new TextBlock
        {
            Text = value,
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(color),
            Tag = label
        };

        indicator.Children.Add(labelText);
        indicator.Children.Add(valueText);

        return indicator;
    }

    private void CreateAlertsGrid()
    {
        var alertsBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(30, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 150, 255)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(5),
            CornerRadius = new CornerRadius(5)
        };

        _alertsGrid = new DataGrid
        {
            Background = Brushes.Transparent,
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            CanUserAddRows = false,
            CanUserDeleteRows = true,
            CanUserReorderColumns = false,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            IsReadOnly = false,
            SelectionMode = DataGridSelectionMode.Single,
            AutoGenerateColumns = false,
            Margin = new Thickness(5)
        };

        CreateAlertsGridColumns();
        alertsBorder.Child = _alertsGrid;

        Grid.SetRow(alertsBorder, 1);
        _mainGrid.Children.Add(alertsBorder);
    }

    private void CreateAlertsGridColumns()
    {
        var enabledColumn = new DataGridCheckBoxColumn
        {
            Header = "Active",
            Binding = new Binding("IsEnabled"),
            Width = new DataGridLength(60),
            CanUserSort = true
        };

        var itemColumn = new DataGridTextColumn
        {
            Header = "Item",
            Binding = new Binding("ItemName"),
            Width = new DataGridLength(150),
            ElementStyle = CreateCellStyle(Colors.White)
        };

        var typeColumn = new DataGridTextColumn
        {
            Header = "Type",
            Binding = new Binding("AlertType"),
            Width = new DataGridLength(80),
            ElementStyle = CreateCellStyle(Colors.Cyan)
        };

        var conditionColumn = new DataGridTextColumn
        {
            Header = "Condition",
            Binding = new Binding("FormattedCondition"),
            Width = new DataGridLength(120),
            ElementStyle = CreateCellStyle(Colors.Yellow)
        };

        var targetColumn = new DataGridTextColumn
        {
            Header = "Target",
            Binding = new Binding("FormattedTarget"),
            Width = new DataGridLength(100),
            ElementStyle = CreateCellStyle(Colors.Orange)
        };

        var statusColumn = new DataGridTemplateColumn
        {
            Header = "Status",
            Width = new DataGridLength(100),
            CellTemplate = CreateStatusCellTemplate()
        };

        var lastTriggeredColumn = new DataGridTextColumn
        {
            Header = "Last Triggered",
            Binding = new Binding("FormattedLastTriggered"),
            Width = new DataGridLength(120),
            ElementStyle = CreateCellStyle(Colors.LightGray)
        };

        _alertsGrid.Columns.Add(enabledColumn);
        _alertsGrid.Columns.Add(itemColumn);
        _alertsGrid.Columns.Add(typeColumn);
        _alertsGrid.Columns.Add(conditionColumn);
        _alertsGrid.Columns.Add(targetColumn);
        _alertsGrid.Columns.Add(statusColumn);
        _alertsGrid.Columns.Add(lastTriggeredColumn);
    }

    private Style CreateCellStyle(Color color)
    {
        var style = new Style(typeof(TextBlock));
        style.Setters.Add(new Setter(TextBlock.ForegroundProperty, new SolidColorBrush(color)));
        style.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.Bold));
        style.Setters.Add(new Setter(TextBlock.FontSizeProperty, 11.0));
        return style;
    }

    private DataTemplate CreateStatusCellTemplate()
    {
        var template = new DataTemplate();
        
        var factory = new FrameworkElementFactory(typeof(Border));
        factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
        factory.SetValue(Border.PaddingProperty, new Thickness(5, 2, 5, 2));
        factory.SetBinding(Border.BackgroundProperty, new Binding("StatusColor"));
        
        var textFactory = new FrameworkElementFactory(typeof(TextBlock));
        textFactory.SetBinding(TextBlock.TextProperty, new Binding("StatusText"));
        textFactory.SetValue(TextBlock.ForegroundProperty, Brushes.White);
        textFactory.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        textFactory.SetValue(TextBlock.FontSizeProperty, 10.0);
        textFactory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
        
        factory.AppendChild(textFactory);
        template.VisualTree = factory;
        
        return template;
    }

    private void CreateNotificationArea()
    {
        var notificationBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 0, 150, 255)),
            BorderThickness = new Thickness(2),
            Margin = new Thickness(5),
            CornerRadius = new CornerRadius(5)
        };

        var notificationHeader = new TextBlock
        {
            Text = "ACTIVE NOTIFICATIONS",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 10)
        };

        var notificationScrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
            Background = Brushes.Transparent
        };

        _notificationPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Background = Brushes.Transparent
        };

        _notificationCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            ClipToBounds = true
        };

        notificationScrollViewer.Content = _notificationPanel;

        var notificationContent = new Grid();
        notificationContent.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        notificationContent.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        Grid.SetRow(notificationHeader, 0);
        Grid.SetRow(notificationScrollViewer, 1);

        notificationContent.Children.Add(notificationHeader);
        notificationContent.Children.Add(notificationScrollViewer);
        notificationContent.Children.Add(_notificationCanvas);

        notificationBorder.Child = notificationContent;

        Grid.SetRow(notificationBorder, 2);
        _mainGrid.Children.Add(notificationBorder);
    }

    private void CreateParticleLayer()
    {
        _particleCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false
        };

        Grid.SetRowSpan(_particleCanvas, 3);
        _mainGrid.Children.Add(_particleCanvas);
    }

    private void InitializeAlertSystem()
    {
        _alertIndicators = new List<UIElement>();
        _particles = new List<UIElement>();
        _notificationElements = new List<UIElement>();
        
        // Initialize collections
        MarketAlerts = new ObservableCollection<HoloMarketAlert>();
        ActiveNotifications = new ObservableCollection<HoloAlertNotification>();
        
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
        
        _notificationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _notificationTimer.Tick += NotificationTimer_Tick;
    }

    private void SetupEventHandlers()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
        
        // Collection change handlers
        MarketAlerts.CollectionChanged += MarketAlerts_CollectionChanged;
        ActiveNotifications.CollectionChanged += ActiveNotifications_CollectionChanged;
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        StartAlertMonitoring();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopAlertMonitoring();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateNotificationLayout();
    }

    private void UpdateTimer_Tick(object sender, EventArgs e)
    {
        MonitorAlerts();
        UpdateStatusDisplay();
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

    private void NotificationTimer_Tick(object sender, EventArgs e)
    {
        ProcessNotificationQueue();
        CleanupExpiredNotifications();
    }

    private void MarketAlerts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdateAlertsGrid();
        UpdateStatusDisplay();
    }

    private void ActiveNotifications_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdateNotificationDisplay();
    }

    #endregion

    #region Alert System

    public void StartAlertMonitoring()
    {
        if (!_updateTimer.IsEnabled)
        {
            _updateTimer.Start();
            
            if (EnableAnimations)
            {
                _animationTimer.Start();
            }
            
            _notificationTimer.Start();
            
            // Generate sample alerts if none exist
            if (MarketAlerts == null || !MarketAlerts.Any())
            {
                GenerateSampleAlerts();
            }
            
            UpdateAlertsGrid();
        }
    }

    public void StopAlertMonitoring()
    {
        _updateTimer?.Stop();
        _animationTimer?.Stop();
        _notificationTimer?.Stop();
    }

    private void MonitorAlerts()
    {
        if (MarketAlerts == null || !MarketAlerts.Any()) return;

        foreach (var alert in MarketAlerts.Where(a => a.IsEnabled))
        {
            CheckAlertCondition(alert);
        }
    }

    private void CheckAlertCondition(HoloMarketAlert alert)
    {
        // Simulate current market price
        var currentPrice = GetCurrentPrice(alert.ItemName);
        var currentVolume = GetCurrentVolume(alert.ItemName);
        
        var previousPrice = _lastPrices.ContainsKey(alert.ItemName) 
            ? _lastPrices[alert.ItemName] 
            : currentPrice;
        
        _lastPrices[alert.ItemName] = currentPrice;
        
        bool conditionMet = false;
        string triggerReason = "";
        
        switch (alert.AlertType)
        {
            case AlertType.PriceAbove:
                conditionMet = currentPrice > alert.TargetValue;
                triggerReason = $"Price reached {currentPrice:N2} ISK (target: {alert.TargetValue:N2})";
                break;
                
            case AlertType.PriceBelow:
                conditionMet = currentPrice < alert.TargetValue;
                triggerReason = $"Price dropped to {currentPrice:N2} ISK (target: {alert.TargetValue:N2})";
                break;
                
            case AlertType.PriceChange:
                var changePercent = Math.Abs((currentPrice - previousPrice) / previousPrice) * 100;
                conditionMet = changePercent > alert.TargetValue;
                triggerReason = $"Price changed by {changePercent:F2}% (threshold: {alert.TargetValue:F2}%)";
                break;
                
            case AlertType.VolumeAbove:
                conditionMet = currentVolume > alert.TargetValue;
                triggerReason = $"Volume reached {currentVolume:N0} (target: {alert.TargetValue:N0})";
                break;
                
            case AlertType.VolumeBelow:
                conditionMet = currentVolume < alert.TargetValue;
                triggerReason = $"Volume dropped to {currentVolume:N0} (target: {alert.TargetValue:N0})";
                break;
        }
        
        if (conditionMet)
        {
            // Check cooldown period
            var lastTriggered = _lastAlertTimes.ContainsKey(alert.AlertId) 
                ? _lastAlertTimes[alert.AlertId] 
                : DateTime.MinValue;
                
            if (DateTime.Now - lastTriggered > TimeSpan.FromMinutes(1)) // 1 minute cooldown
            {
                TriggerAlert(alert, triggerReason, currentPrice, currentVolume);
                _lastAlertTimes[alert.AlertId] = DateTime.Now;
            }
        }
    }

    private double GetCurrentPrice(string itemName)
    {
        // Simulate market price with some variation
        var basePrice = GetBasePriceForItem(itemName);
        var variation = (_random.NextDouble() - 0.5) * 0.1; // ±10% variation
        return basePrice * (1 + variation);
    }

    private long GetCurrentVolume(string itemName)
    {
        // Simulate market volume
        var baseVolume = GetBaseVolumeForItem(itemName);
        var variation = _random.NextDouble() * 0.8 + 0.2; // 20-100% of base volume
        return (long)(baseVolume * variation);
    }

    private double GetBasePriceForItem(string itemName)
    {
        return itemName switch
        {
            "Tritanium" => 5.50,
            "Pyerite" => 12.80,
            "Mexallon" => 85.00,
            "PLEX" => 2800000.00,
            "Skill Injector" => 850000000.00,
            _ => 1000000.00
        };
    }

    private long GetBaseVolumeForItem(string itemName)
    {
        return itemName switch
        {
            "Tritanium" => 50000000,
            "Pyerite" => 25000000,
            "Mexallon" => 8000000,
            "PLEX" => 15000,
            "Skill Injector" => 2500,
            _ => 1000000
        };
    }

    private void TriggerAlert(HoloMarketAlert alert, string reason, double currentPrice, long currentVolume)
    {
        // Update alert status
        alert.LastTriggered = DateTime.Now;
        alert.TriggeredCount++;
        alert.Status = AlertStatus.Triggered;
        
        // Create notification
        var notification = new HoloAlertNotification
        {
            NotificationId = Guid.NewGuid().ToString(),
            AlertId = alert.AlertId,
            Title = $"{alert.ItemName} Alert",
            Message = reason,
            Timestamp = DateTime.Now,
            Priority = alert.Priority,
            CurrentPrice = currentPrice,
            CurrentVolume = currentVolume,
            IsRead = false
        };
        
        // Add to queue for animated display
        _notificationQueue.Enqueue(notification);
        
        // Add to active notifications
        ActiveNotifications.Insert(0, notification);
        
        // Limit active notifications
        while (ActiveNotifications.Count > _maxNotifications)
        {
            var oldestNotification = ActiveNotifications.Last();
            ActiveNotifications.Remove(oldestNotification);
        }
        
        // Create alert effects
        if (EnableParticleEffects && !_isInSimplifiedMode)
        {
            CreateAlertParticles(alert.Priority);
        }
        
        // Play sound alert
        if (EnableSoundAlerts)
        {
            PlayAlertSound(alert.Priority);
        }
        
        // Update counters
        _triggeredAlertsToday++;
        
        // Fire events
        AlertTriggered?.Invoke(this, alert);
        NotificationCreated?.Invoke(this, notification);
        
        StartAnimation();
    }

    private void PlayAlertSound(AlertPriority priority)
    {
        // Implementation would play actual sound based on priority
        // For now, just simulate the action
        System.Console.Beep(priority == AlertPriority.Critical ? 1000 : 800, 200);
    }

    #endregion

    #region Notification Management

    private void ProcessNotificationQueue()
    {
        if (_notificationQueue.Count > 0 && _notificationElements.Count < _maxNotifications)
        {
            var notification = _notificationQueue.Dequeue();
            CreateAnimatedNotification(notification);
        }
    }

    private void CreateAnimatedNotification(HoloAlertNotification notification)
    {
        var notificationCard = new Border
        {
            Background = GetNotificationBackground(notification.Priority),
            BorderBrush = GetNotificationBorder(notification.Priority),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(5),
            MinHeight = 60,
            Tag = notification
        };

        var cardContent = new Grid();
        cardContent.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        cardContent.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        cardContent.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Priority indicator
        var priorityIndicator = new Ellipse
        {
            Width = 12,
            Height = 12,
            Fill = GetPriorityColor(notification.Priority),
            Margin = new Thickness(10, 0, 10, 0)
        };
        
        Grid.SetColumn(priorityIndicator, 0);
        cardContent.Children.Add(priorityIndicator);

        // Content
        var contentPanel = new StackPanel
        {
            Margin = new Thickness(5, 10, 5, 10)
        };

        var titleText = new TextBlock
        {
            Text = notification.Title,
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White,
            Margin = new Thickness(0, 0, 0, 3)
        };

        var messageText = new TextBlock
        {
            Text = notification.Message,
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 200, 200)),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 3)
        };

        var timeText = new TextBlock
        {
            Text = notification.Timestamp.ToString("HH:mm:ss"),
            FontSize = 9,
            Foreground = new SolidColorBrush(Color.FromArgb(150, 150, 150, 150))
        };

        contentPanel.Children.Add(titleText);
        contentPanel.Children.Add(messageText);
        contentPanel.Children.Add(timeText);

        Grid.SetColumn(contentPanel, 1);
        cardContent.Children.Add(contentPanel);

        // Close button
        var closeButton = new Button
        {
            Content = "✕",
            Width = 20,
            Height = 20,
            Background = new SolidColorBrush(Color.FromArgb(100, 255, 100, 100)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(5)
        };

        closeButton.Click += (s, e) => RemoveNotification(notification);

        Grid.SetColumn(closeButton, 2);
        cardContent.Children.Add(closeButton);

        notificationCard.Child = cardContent;

        // Add glow effect for critical alerts
        if (notification.Priority == AlertPriority.Critical)
        {
            notificationCard.Effect = new DropShadowEffect
            {
                Color = Colors.Red,
                BlurRadius = 10,
                ShadowDepth = 0,
                Opacity = 0.7
            };
        }

        // Add to panel with animation
        _notificationPanel.Children.Insert(0, notificationCard);
        _notificationElements.Add(notificationCard);

        // Animate appearance
        if (EnableAnimations)
        {
            AnimateNotificationAppearance(notificationCard);
        }

        // Auto-remove after timeout
        if (AutoClearNotifications)
        {
            var removeTimer = new DispatcherTimer
            {
                Interval = NotificationTimeout
            };
            removeTimer.Tick += (s, e) =>
            {
                removeTimer.Stop();
                RemoveNotification(notification);
            };
            removeTimer.Start();
        }
    }

    private void AnimateNotificationAppearance(Border notificationCard)
    {
        notificationCard.Opacity = 0;
        notificationCard.RenderTransform = new TranslateTransform(300, 0);

        var opacityAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase()
        };

        var slideAnimation = new DoubleAnimation
        {
            From = 300,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase()
        };

        notificationCard.BeginAnimation(OpacityProperty, opacityAnimation);
        ((TranslateTransform)notificationCard.RenderTransform).BeginAnimation(TranslateTransform.XProperty, slideAnimation);
    }

    private void RemoveNotification(HoloAlertNotification notification)
    {
        var notificationCard = _notificationElements
            .OfType<Border>()
            .FirstOrDefault(b => b.Tag as HoloAlertNotification == notification);

        if (notificationCard != null)
        {
            _notificationPanel.Children.Remove(notificationCard);
            _notificationElements.Remove(notificationCard);
        }

        ActiveNotifications.Remove(notification);
        NotificationCleared?.Invoke(this, notification);
    }

    private void CleanupExpiredNotifications()
    {
        if (!AutoClearNotifications) return;

        var expiredNotifications = ActiveNotifications
            .Where(n => DateTime.Now - n.Timestamp > NotificationTimeout)
            .ToList();

        foreach (var notification in expiredNotifications)
        {
            RemoveNotification(notification);
        }
    }

    private Brush GetNotificationBackground(AlertPriority priority)
    {
        return priority switch
        {
            AlertPriority.Low => new SolidColorBrush(Color.FromArgb(150, 0, 100, 50)),
            AlertPriority.Medium => new SolidColorBrush(Color.FromArgb(150, 100, 100, 0)),
            AlertPriority.High => new SolidColorBrush(Color.FromArgb(150, 150, 100, 0)),
            AlertPriority.Critical => new SolidColorBrush(Color.FromArgb(150, 200, 50, 0)),
            _ => new SolidColorBrush(Color.FromArgb(150, 50, 50, 100))
        };
    }

    private Brush GetNotificationBorder(AlertPriority priority)
    {
        return priority switch
        {
            AlertPriority.Low => new SolidColorBrush(Color.FromArgb(200, 0, 200, 100)),
            AlertPriority.Medium => new SolidColorBrush(Color.FromArgb(200, 200, 200, 0)),
            AlertPriority.High => new SolidColorBrush(Color.FromArgb(200, 255, 200, 0)),
            AlertPriority.Critical => new SolidColorBrush(Color.FromArgb(200, 255, 100, 0)),
            _ => new SolidColorBrush(Color.FromArgb(200, 100, 100, 200))
        };
    }

    private Brush GetPriorityColor(AlertPriority priority)
    {
        return priority switch
        {
            AlertPriority.Low => new SolidColorBrush(Colors.LimeGreen),
            AlertPriority.Medium => new SolidColorBrush(Colors.Yellow),
            AlertPriority.High => new SolidColorBrush(Colors.Orange),
            AlertPriority.Critical => new SolidColorBrush(Colors.Red),
            _ => new SolidColorBrush(Colors.Gray)
        };
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
        // Animate alert status indicators
        foreach (var indicator in _alertIndicators)
        {
            if (indicator is Border border)
            {
                border.Opacity = 0.5 + (0.5 * Math.Abs(Math.Sin(_animationProgress * Math.PI * 2)));
            }
        }
    }

    private void CreateAlertParticles(AlertPriority priority)
    {
        var particleCount = priority switch
        {
            AlertPriority.Critical => 15,
            AlertPriority.High => 10,
            AlertPriority.Medium => 5,
            _ => 3
        };

        var color = priority switch
        {
            AlertPriority.Critical => Colors.Red,
            AlertPriority.High => Colors.Orange,
            AlertPriority.Medium => Colors.Yellow,
            _ => Colors.LimeGreen
        };

        for (int i = 0; i < particleCount; i++)
        {
            var particle = new Ellipse
            {
                Width = 4 + _random.NextDouble() * 6,
                Height = 4 + _random.NextDouble() * 6,
                Fill = new SolidColorBrush(Color.FromArgb(200, color.R, color.G, color.B))
            };

            var startX = ActualWidth / 2 + (_random.NextDouble() - 0.5) * 100;
            var startY = ActualHeight / 2 + (_random.NextDouble() - 0.5) * 100;

            Canvas.SetLeft(particle, startX);
            Canvas.SetTop(particle, startY);

            _particleCanvas.Children.Add(particle);
            _particles.Add(particle);

            AnimateAlertParticle(particle);
        }
    }

    private void AnimateAlertParticle(UIElement particle)
    {
        var duration = TimeSpan.FromSeconds(2 + _random.NextDouble() * 3);

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

    #region UI Updates

    private void UpdateAlertsGrid()
    {
        if (MarketAlerts != null)
        {
            var filteredAlerts = FilterAlertsByMode(MarketAlerts);
            _alertsGrid.ItemsSource = filteredAlerts;
        }

        _activeAlertCount = MarketAlerts?.Count(a => a.IsEnabled) ?? 0;
    }

    private List<HoloMarketAlert> FilterAlertsByMode(ObservableCollection<HoloMarketAlert> alerts)
    {
        return AlertMode switch
        {
            MarketAlertMode.PriceOnly => alerts.Where(a => a.AlertType.ToString().Contains("Price")).ToList(),
            MarketAlertMode.VolumeOnly => alerts.Where(a => a.AlertType.ToString().Contains("Volume")).ToList(),
            MarketAlertMode.Critical => alerts.Where(a => a.Priority == AlertPriority.Critical).ToList(),
            _ => alerts.ToList()
        };
    }

    private void UpdateStatusDisplay()
    {
        UpdateStatusIndicator("ACTIVE", _activeAlertCount.ToString());
        UpdateStatusIndicator("TODAY", _triggeredAlertsToday.ToString());
        UpdateStatusIndicator("QUEUED", _notificationQueue.Count.ToString());

        var statusColor = _activeAlertCount > 0 ? Colors.LimeGreen : Colors.Orange;
        var statusMessage = _activeAlertCount > 0 ? "ALERT SYSTEM ACTIVE" : "NO ACTIVE ALERTS";
        
        _statusText.Text = statusMessage;
        _statusText.Foreground = new SolidColorBrush(statusColor);
    }

    private void UpdateStatusIndicator(string label, string value)
    {
        var statusContent = _statusPanel.Child as StackPanel;
        var statusIndicators = statusContent?.Children[1] as StackPanel;
        
        if (statusIndicators != null)
        {
            foreach (StackPanel indicator in statusIndicators.Children)
            {
                var valueText = indicator.Children[1] as TextBlock;
                if (valueText?.Tag?.ToString() == label)
                {
                    valueText.Text = value;
                    break;
                }
            }
        }
    }

    private void UpdateNotificationDisplay()
    {
        // Notification display is managed by CreateAnimatedNotification
        // This method can be used for additional updates if needed
    }

    private void UpdateNotificationLayout()
    {
        // Update layout when window size changes
        if (_notificationCanvas != null)
        {
            _notificationCanvas.Width = ActualWidth;
            _notificationCanvas.Height = 200;
        }
    }

    #endregion

    #region Sample Data and Dialogs

    private void GenerateSampleAlerts()
    {
        var sampleItems = new[] { "Tritanium", "Pyerite", "Mexallon", "PLEX", "Skill Injector" };
        
        foreach (var item in sampleItems)
        {
            var alert = new HoloMarketAlert
            {
                AlertId = Guid.NewGuid().ToString(),
                ItemName = item,
                AlertType = (AlertType)_random.Next(0, 5),
                TargetValue = GetRandomTargetValue(item),
                Priority = (AlertPriority)_random.Next(0, 4),
                IsEnabled = _random.NextDouble() > 0.3,
                CreatedDate = DateTime.Now.AddDays(-_random.Next(30)),
                Status = AlertStatus.Active
            };
            
            MarketAlerts.Add(alert);
        }
    }

    private double GetRandomTargetValue(string itemName)
    {
        var basePrice = GetBasePriceForItem(itemName);
        return basePrice * (0.8 + _random.NextDouble() * 0.4); // ±20% of base price
    }

    private void ShowAddAlertDialog()
    {
        // Implementation would show a dialog to add new alert
        // For now, just add a random alert
        var sampleItems = new[] { "Tritanium", "Pyerite", "Mexallon", "PLEX", "Skill Injector" };
        var randomItem = sampleItems[_random.Next(sampleItems.Length)];
        
        var newAlert = new HoloMarketAlert
        {
            AlertId = Guid.NewGuid().ToString(),
            ItemName = randomItem,
            AlertType = AlertType.PriceAbove,
            TargetValue = GetRandomTargetValue(randomItem),
            Priority = AlertPriority.Medium,
            IsEnabled = true,
            CreatedDate = DateTime.Now,
            Status = AlertStatus.Active
        };
        
        MarketAlerts.Add(newAlert);
    }

    private void ClearAllNotifications()
    {
        var notificationsToRemove = ActiveNotifications.ToList();
        foreach (var notification in notificationsToRemove)
        {
            RemoveNotification(notification);
        }
        
        _notificationQueue.Clear();
    }

    #endregion

    #region Public Methods

    public MarketAlertAnalysis GetMarketAlertAnalysis()
    {
        return new MarketAlertAnalysis
        {
            TotalAlerts = MarketAlerts?.Count ?? 0,
            ActiveAlerts = _activeAlertCount,
            TriggeredToday = _triggeredAlertsToday,
            QueuedNotifications = _notificationQueue.Count,
            ActiveNotifications = ActiveNotifications?.Count ?? 0,
            AlertMode = AlertMode,
            IsMonitoring = _updateTimer?.IsEnabled ?? false,
            PerformanceMode = _isInSimplifiedMode ? "Simplified" : "Full"
        };
    }

    public void AddAlert(HoloMarketAlert alert)
    {
        MarketAlerts?.Add(alert);
    }

    public void RemoveAlert(string alertId)
    {
        var alert = MarketAlerts?.FirstOrDefault(a => a.AlertId == alertId);
        if (alert != null)
        {
            MarketAlerts.Remove(alert);
        }
    }

    public void SetAlertMode(MarketAlertMode mode)
    {
        AlertMode = mode;
        UpdateAlertsGrid();
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void SetSimplifiedMode(bool enabled)
    {
        _isInSimplifiedMode = enabled;
        EnableParticleEffects = !enabled;
        EnableAnimations = !enabled;
        _maxParticles = enabled ? 10 : 20;
        _maxNotifications = enabled ? 5 : 10;
        
        if (IsLoaded)
        {
            UpdateAlertsGrid();
        }
    }

    public bool IsInSimplifiedMode => _isInSimplifiedMode;

    #endregion

    #region Property Change Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketAlerts control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketAlerts control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnMarketAlertsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketAlerts control && control.IsLoaded)
        {
            control.UpdateAlertsGrid();
        }
    }

    private static void OnActiveNotificationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketAlerts control && control.IsLoaded)
        {
            control.UpdateNotificationDisplay();
        }
    }

    private static void OnAlertModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketAlerts control && control.IsLoaded)
        {
            control.UpdateAlertsGrid();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketAlerts control)
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
        // No immediate action needed
    }

    private static void OnEnableSoundAlertsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // No immediate action needed
    }

    private static void OnShowAlertHistoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Implementation would show/hide alert history panel
    }

    private static void OnAutoClearNotificationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // No immediate action needed
    }

    private static void OnNotificationTimeoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // No immediate action needed
    }

    private static void OnUpdateIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloMarketAlerts control)
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
            UpdateAlertsGrid();
        }
    }

    #endregion
}

#region Supporting Classes

public class HoloMarketAlert : INotifyPropertyChanged
{
    private bool _isEnabled;
    private AlertStatus _status;
    private DateTime? _lastTriggered;
    private int _triggeredCount;

    public string AlertId { get; set; }
    public string ItemName { get; set; }
    public AlertType AlertType { get; set; }
    public double TargetValue { get; set; }
    public AlertPriority Priority { get; set; }
    public DateTime CreatedDate { get; set; }

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            OnPropertyChanged(nameof(IsEnabled));
        }
    }

    public AlertStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusColor));
        }
    }

    public DateTime? LastTriggered
    {
        get => _lastTriggered;
        set
        {
            _lastTriggered = value;
            OnPropertyChanged(nameof(LastTriggered));
            OnPropertyChanged(nameof(FormattedLastTriggered));
        }
    }

    public int TriggeredCount
    {
        get => _triggeredCount;
        set
        {
            _triggeredCount = value;
            OnPropertyChanged(nameof(TriggeredCount));
        }
    }

    public string FormattedCondition => $"{AlertType}";
    public string FormattedTarget => TargetValue.ToString("N2");
    public string FormattedLastTriggered => LastTriggered?.ToString("MM/dd HH:mm") ?? "Never";

    public string StatusText => Status switch
    {
        AlertStatus.Active => "ACTIVE",
        AlertStatus.Triggered => "TRIGGERED",
        AlertStatus.Disabled => "DISABLED",
        AlertStatus.Error => "ERROR",
        _ => "UNKNOWN"
    };

    public Brush StatusColor => Status switch
    {
        AlertStatus.Active => new SolidColorBrush(Colors.LimeGreen),
        AlertStatus.Triggered => new SolidColorBrush(Colors.Orange),
        AlertStatus.Disabled => new SolidColorBrush(Colors.Gray),
        AlertStatus.Error => new SolidColorBrush(Colors.Red),
        _ => new SolidColorBrush(Colors.Gray)
    };

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class HoloAlertNotification
{
    public string NotificationId { get; set; }
    public string AlertId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
    public AlertPriority Priority { get; set; }
    public double CurrentPrice { get; set; }
    public long CurrentVolume { get; set; }
    public bool IsRead { get; set; }
}

public enum AlertType
{
    PriceAbove,
    PriceBelow,
    PriceChange,
    VolumeAbove,
    VolumeBelow
}

public enum AlertPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum AlertStatus
{
    Active,
    Triggered,
    Disabled,
    Error
}

public enum MarketAlertMode
{
    All,
    PriceOnly,
    VolumeOnly,
    Critical
}

public class MarketAlertAnalysis
{
    public int TotalAlerts { get; set; }
    public int ActiveAlerts { get; set; }
    public int TriggeredToday { get; set; }
    public int QueuedNotifications { get; set; }
    public int ActiveNotifications { get; set; }
    public MarketAlertMode AlertMode { get; set; }
    public bool IsMonitoring { get; set; }
    public string PerformanceMode { get; set; }
}

#endregion