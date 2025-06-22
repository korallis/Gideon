// ==========================================================================
// HoloNotificationCenter.cs - Holographic Notification Center
// ==========================================================================
// Comprehensive notification system featuring holographic notification panels,
// real-time alerts, priority management, and interactive notification actions.
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
/// Holographic notification center with real-time alerts and priority management
/// </summary>
public class HoloNotificationCenter : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloNotificationCenter),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloNotificationCenter),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty NotificationFilterProperty =
        DependencyProperty.Register(nameof(NotificationFilter), typeof(NotificationFilter), typeof(HoloNotificationCenter),
            new PropertyMetadata(NotificationFilter.All, OnNotificationFilterChanged));

    public static readonly DependencyProperty SortModeProperty =
        DependencyProperty.Register(nameof(SortMode), typeof(NotificationSortMode), typeof(HoloNotificationCenter),
            new PropertyMetadata(NotificationSortMode.Priority, OnSortModeChanged));

    public static readonly DependencyProperty EnableNotificationAnimationsProperty =
        DependencyProperty.Register(nameof(EnableNotificationAnimations), typeof(bool), typeof(HoloNotificationCenter),
            new PropertyMetadata(true, OnEnableNotificationAnimationsChanged));

    public static readonly DependencyProperty EnableSoundAlertsProperty =
        DependencyProperty.Register(nameof(EnableSoundAlerts), typeof(bool), typeof(HoloNotificationCenter),
            new PropertyMetadata(true, OnEnableSoundAlertsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloNotificationCenter),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowUnreadOnlyProperty =
        DependencyProperty.Register(nameof(ShowUnreadOnly), typeof(bool), typeof(HoloNotificationCenter),
            new PropertyMetadata(false));

    public static readonly DependencyProperty NotificationsProperty =
        DependencyProperty.Register(nameof(Notifications), typeof(ObservableCollection<HoloNotification>), typeof(HoloNotificationCenter),
            new PropertyMetadata(null));

    public static readonly DependencyProperty AlertSettingsProperty =
        DependencyProperty.Register(nameof(AlertSettings), typeof(ObservableCollection<HoloAlertSetting>), typeof(HoloNotificationCenter),
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

    public NotificationFilter NotificationFilter
    {
        get => (NotificationFilter)GetValue(NotificationFilterProperty);
        set => SetValue(NotificationFilterProperty, value);
    }

    public NotificationSortMode SortMode
    {
        get => (NotificationSortMode)GetValue(SortModeProperty);
        set => SetValue(SortModeProperty, value);
    }

    public bool EnableNotificationAnimations
    {
        get => (bool)GetValue(EnableNotificationAnimationsProperty);
        set => SetValue(EnableNotificationAnimationsProperty, value);
    }

    public bool EnableSoundAlerts
    {
        get => (bool)GetValue(EnableSoundAlertsProperty);
        set => SetValue(EnableSoundAlertsProperty, value);
    }

    public bool EnableParticleEffects
    {
        get => (bool)GetValue(EnableParticleEffectsProperty);
        set => SetValue(EnableParticleEffectsProperty, value);
    }

    public bool ShowUnreadOnly
    {
        get => (bool)GetValue(ShowUnreadOnlyProperty);
        set => SetValue(ShowUnreadOnlyProperty, value);
    }

    public ObservableCollection<HoloNotification> Notifications
    {
        get => (ObservableCollection<HoloNotification>)GetValue(NotificationsProperty);
        set => SetValue(NotificationsProperty, value);
    }

    public ObservableCollection<HoloAlertSetting> AlertSettings
    {
        get => (ObservableCollection<HoloAlertSetting>)GetValue(AlertSettingsProperty);
        set => SetValue(AlertSettingsProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloNotificationEventArgs> NotificationReceived;
    public event EventHandler<HoloNotificationEventArgs> NotificationRead;
    public event EventHandler<HoloNotificationEventArgs> NotificationDismissed;
    public event EventHandler<HoloNotificationEventArgs> NotificationActionTriggered;
    public event EventHandler<HoloNotificationCenterEventArgs> FilterChanged;
    public event EventHandler<HoloNotificationCenterEventArgs> AllNotificationsCleared;

    #endregion

    #region Private Fields

    private readonly DispatcherTimer _notificationTimer;
    private readonly Random _random = new();
    private Canvas _mainCanvas;
    private Grid _notificationGrid;
    private StackPanel _filterPanel;
    private ScrollViewer _notificationScroller;
    private TextBlock _notificationCounter;
    private Button _clearAllButton;
    private Button _markAllReadButton;
    private ComboBox _sortComboBox;
    private ToggleButton _filterToggle;
    private Canvas _particleCanvas;
    private readonly List<UIElement> _particles = new();
    private bool _isSimplifiedMode;

    #endregion

    #region Constructor

    public HoloNotificationCenter()
    {
        InitializeComponent();
        InitializeCollections();
        InitializeTimer();
        LoadSampleNotifications();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 400;
        Height = 600;
        Background = new SolidColorBrush(Color.FromArgb(20, 100, 200, 255));
        BorderBrush = new SolidColorBrush(Color.FromArgb(80, 100, 200, 255));
        BorderThickness = new Thickness(1);
        Effect = new DropShadowEffect
        {
            Color = Color.FromArgb(60, 100, 200, 255),
            BlurRadius = 10,
            ShadowDepth = 3,
            Direction = 315
        };

        CreateMainLayout();
        ApplyEVEColorScheme();
    }

    private void CreateMainLayout()
    {
        _mainCanvas = new Canvas();
        Content = _mainCanvas;

        // Background with holographic effect
        var backgroundRect = new Rectangle
        {
            Fill = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(30, 100, 200, 255), 0),
                    new GradientStop(Color.FromArgb(10, 50, 150, 255), 0.5),
                    new GradientStop(Color.FromArgb(20, 100, 200, 255), 1)
                }
            },
            Width = 400,
            Height = 600
        };
        _mainCanvas.Children.Add(backgroundRect);

        // Main grid for layout
        _notificationGrid = new Grid();
        _notificationGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _notificationGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _notificationGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        _notificationGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _mainCanvas.Children.Add(_notificationGrid);

        CreateHeaderSection();
        CreateFilterSection();
        CreateNotificationListSection();
        CreateActionSection();
        CreateParticleCanvas();
    }

    private void CreateHeaderSection()
    {
        var headerPanel = new DockPanel
        {
            Height = 50,
            Margin = new Thickness(10)
        };
        Grid.SetRow(headerPanel, 0);
        _notificationGrid.Children.Add(headerPanel);

        // Title
        var titleText = new TextBlock
        {
            Text = "NOTIFICATIONS",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 100, 200, 255)),
            VerticalAlignment = VerticalAlignment.Center,
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(80, 100, 200, 255),
                BlurRadius = 5,
                ShadowDepth = 1
            }
        };
        DockPanel.SetDock(titleText, Dock.Left);
        headerPanel.Children.Add(titleText);

        // Notification counter
        _notificationCounter = new TextBlock
        {
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 200, 200)),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        DockPanel.SetDock(_notificationCounter, Dock.Right);
        headerPanel.Children.Add(_notificationCounter);
    }

    private void CreateFilterSection()
    {
        _filterPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Height = 40,
            Margin = new Thickness(10, 0, 10, 10)
        };
        Grid.SetRow(_filterPanel, 1);
        _notificationGrid.Children.Add(_filterPanel);

        // Filter toggle
        _filterToggle = new ToggleButton
        {
            Content = "FILTERS",
            Width = 80,
            Height = 30,
            Margin = new Thickness(0, 0, 10, 0),
            Style = CreateHolographicButtonStyle()
        };
        _filterToggle.Checked += (s, e) => ShowAdvancedFilters();
        _filterToggle.Unchecked += (s, e) => HideAdvancedFilters();
        _filterPanel.Children.Add(_filterToggle);

        // Sort mode combo
        _sortComboBox = new ComboBox
        {
            Width = 120,
            Height = 30,
            Margin = new Thickness(0, 0, 10, 0)
        };
        _sortComboBox.Items.Add(new ComboBoxItem { Content = "Priority", Tag = NotificationSortMode.Priority });
        _sortComboBox.Items.Add(new ComboBoxItem { Content = "Time", Tag = NotificationSortMode.Timestamp });
        _sortComboBox.Items.Add(new ComboBoxItem { Content = "Type", Tag = NotificationSortMode.Type });
        _sortComboBox.SelectedIndex = 0;
        _sortComboBox.SelectionChanged += OnSortModeSelectionChanged;
        _filterPanel.Children.Add(_sortComboBox);

        // Unread only toggle
        var unreadToggle = new ToggleButton
        {
            Content = "UNREAD",
            Width = 70,
            Height = 30,
            Style = CreateHolographicButtonStyle()
        };
        unreadToggle.Checked += (s, e) => { ShowUnreadOnly = true; FilterNotifications(); };
        unreadToggle.Unchecked += (s, e) => { ShowUnreadOnly = false; FilterNotifications(); };
        _filterPanel.Children.Add(unreadToggle);
    }

    private void CreateNotificationListSection()
    {
        _notificationScroller = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Margin = new Thickness(10, 0, 10, 10)
        };
        Grid.SetRow(_notificationScroller, 2);
        _notificationGrid.Children.Add(_notificationScroller);

        var notificationList = new StackPanel
        {
            Orientation = Orientation.Vertical
        };
        _notificationScroller.Content = notificationList;
    }

    private void CreateActionSection()
    {
        var actionPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Height = 40,
            Margin = new Thickness(10),
            HorizontalAlignment = HorizontalAlignment.Right
        };
        Grid.SetRow(actionPanel, 3);
        _notificationGrid.Children.Add(actionPanel);

        // Mark all read button
        _markAllReadButton = new Button
        {
            Content = "MARK ALL READ",
            Width = 120,
            Height = 30,
            Margin = new Thickness(0, 0, 10, 0),
            Style = CreateHolographicButtonStyle()
        };
        _markAllReadButton.Click += OnMarkAllReadClicked;
        actionPanel.Children.Add(_markAllReadButton);

        // Clear all button
        _clearAllButton = new Button
        {
            Content = "CLEAR ALL",
            Width = 80,
            Height = 30,
            Style = CreateHolographicButtonStyle()
        };
        _clearAllButton.Click += OnClearAllClicked;
        actionPanel.Children.Add(_clearAllButton);
    }

    private void CreateParticleCanvas()
    {
        _particleCanvas = new Canvas
        {
            Width = 400,
            Height = 600,
            IsHitTestVisible = false
        };
        _mainCanvas.Children.Add(_particleCanvas);
    }

    private void InitializeCollections()
    {
        Notifications = new ObservableCollection<HoloNotification>();
        AlertSettings = new ObservableCollection<HoloAlertSetting>();
        Notifications.CollectionChanged += OnNotificationsCollectionChanged;
    }

    private void InitializeTimer()
    {
        _notificationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };
        _notificationTimer.Tick += OnNotificationTimerTick;
        _notificationTimer.Start();
    }

    #endregion

    #region Sample Data

    private void LoadSampleNotifications()
    {
        var sampleNotifications = new[]
        {
            new HoloNotification
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Ship Fitting Complete",
                Message = "Your Battleship fitting has been optimized for maximum DPS output.",
                Type = NotificationType.Success,
                Priority = NotificationPriority.High,
                Timestamp = DateTime.Now.AddMinutes(-2),
                IsRead = false,
                Source = "Ship Fitting Module",
                Actions = new List<HoloNotificationAction>
                {
                    new HoloNotificationAction { Text = "View Details", ActionType = "view_fitting" },
                    new HoloNotificationAction { Text = "Save Fit", ActionType = "save_fitting" }
                }
            },
            new HoloNotification
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Market Alert",
                Message = "Tritanium price has increased by 15% in Jita.",
                Type = NotificationType.Warning,
                Priority = NotificationPriority.High,
                Timestamp = DateTime.Now.AddMinutes(-5),
                IsRead = false,
                Source = "Market Analysis",
                Actions = new List<HoloNotificationAction>
                {
                    new HoloNotificationAction { Text = "View Market", ActionType = "open_market" },
                    new HoloNotificationAction { Text = "Set Alert", ActionType = "create_alert" }
                }
            },
            new HoloNotification
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Skill Training Complete",
                Message = "Caldari Battleship V has finished training.",
                Type = NotificationType.Info,
                Priority = NotificationPriority.Medium,
                Timestamp = DateTime.Now.AddHours(-1),
                IsRead = true,
                Source = "Character Planning",
                Actions = new List<HoloNotificationAction>
                {
                    new HoloNotificationAction { Text = "View Skills", ActionType = "open_skills" }
                }
            },
            new HoloNotification
            {
                Id = Guid.NewGuid().ToString(),
                Title = "API Connection Lost",
                Message = "Unable to connect to EVE ESI API. Retrying...",
                Type = NotificationType.Error,
                Priority = NotificationPriority.Critical,
                Timestamp = DateTime.Now.AddMinutes(-10),
                IsRead = false,
                Source = "System",
                Actions = new List<HoloNotificationAction>
                {
                    new HoloNotificationAction { Text = "Retry", ActionType = "retry_connection" },
                    new HoloNotificationAction { Text = "Settings", ActionType = "open_settings" }
                }
            }
        };

        foreach (var notification in sampleNotifications)
        {
            Notifications.Add(notification);
        }

        UpdateNotificationCounter();
    }

    #endregion

    #region Notification Management

    public async Task AddNotificationAsync(HoloNotification notification)
    {
        if (notification == null) return;

        notification.Id = Guid.NewGuid().ToString();
        notification.Timestamp = DateTime.Now;

        Application.Current.Dispatcher.Invoke(() =>
        {
            Notifications.Insert(0, notification);
            if (EnableNotificationAnimations && !_isSimplifiedMode)
            {
                AnimateNewNotification(notification);
            }
            if (EnableParticleEffects && !_isSimplifiedMode)
            {
                SpawnNotificationParticles(notification);
            }
            if (EnableSoundAlerts)
            {
                PlayNotificationSound(notification.Type);
            }
        });

        NotificationReceived?.Invoke(this, new HoloNotificationEventArgs
        {
            Notification = notification,
            Timestamp = DateTime.Now
        });

        UpdateNotificationCounter();
    }

    public async Task MarkAsReadAsync(string notificationId)
    {
        var notification = Notifications.FirstOrDefault(n => n.Id == notificationId);
        if (notification == null || notification.IsRead) return;

        notification.IsRead = true;
        UpdateNotificationCounter();

        if (EnableNotificationAnimations && !_isSimplifiedMode)
        {
            AnimateNotificationRead(notification);
        }

        NotificationRead?.Invoke(this, new HoloNotificationEventArgs
        {
            Notification = notification,
            Timestamp = DateTime.Now
        });
    }

    public async Task DismissNotificationAsync(string notificationId)
    {
        var notification = Notifications.FirstOrDefault(n => n.Id == notificationId);
        if (notification == null) return;

        if (EnableNotificationAnimations && !_isSimplifiedMode)
        {
            await AnimateNotificationDismissal(notification);
        }

        Notifications.Remove(notification);
        UpdateNotificationCounter();

        NotificationDismissed?.Invoke(this, new HoloNotificationEventArgs
        {
            Notification = notification,
            Timestamp = DateTime.Now
        });
    }

    public async Task ExecuteNotificationActionAsync(string notificationId, string actionType)
    {
        var notification = Notifications.FirstOrDefault(n => n.Id == notificationId);
        if (notification == null) return;

        var action = notification.Actions?.FirstOrDefault(a => a.ActionType == actionType);
        if (action == null) return;

        if (EnableParticleEffects && !_isSimplifiedMode)
        {
            SpawnActionParticles();
        }

        NotificationActionTriggered?.Invoke(this, new HoloNotificationEventArgs
        {
            Notification = notification,
            Action = action,
            Timestamp = DateTime.Now
        });

        // Mark as read when action is taken
        await MarkAsReadAsync(notificationId);
    }

    #endregion

    #region Filtering and Sorting

    private void FilterNotifications()
    {
        var filteredNotifications = Notifications.AsEnumerable();

        if (ShowUnreadOnly)
        {
            filteredNotifications = filteredNotifications.Where(n => !n.IsRead);
        }

        switch (NotificationFilter)
        {
            case NotificationFilter.Errors:
                filteredNotifications = filteredNotifications.Where(n => n.Type == NotificationType.Error);
                break;
            case NotificationFilter.Warnings:
                filteredNotifications = filteredNotifications.Where(n => n.Type == NotificationType.Warning);
                break;
            case NotificationFilter.Info:
                filteredNotifications = filteredNotifications.Where(n => n.Type == NotificationType.Info);
                break;
            case NotificationFilter.Success:
                filteredNotifications = filteredNotifications.Where(n => n.Type == NotificationType.Success);
                break;
        }

        SortNotifications(filteredNotifications.ToList());
        RefreshNotificationDisplay();
    }

    private void SortNotifications(List<HoloNotification> notifications)
    {
        switch (SortMode)
        {
            case NotificationSortMode.Priority:
                notifications.Sort((a, b) => b.Priority.CompareTo(a.Priority));
                break;
            case NotificationSortMode.Timestamp:
                notifications.Sort((a, b) => b.Timestamp.CompareTo(a.Timestamp));
                break;
            case NotificationSortMode.Type:
                notifications.Sort((a, b) => a.Type.CompareTo(b.Type));
                break;
        }
    }

    private void RefreshNotificationDisplay()
    {
        if (_notificationScroller?.Content is StackPanel panel)
        {
            panel.Children.Clear();
            var filteredNotifications = GetFilteredNotifications();
            
            foreach (var notification in filteredNotifications)
            {
                var notificationControl = CreateNotificationControl(notification);
                panel.Children.Add(notificationControl);
            }
        }
    }

    private List<HoloNotification> GetFilteredNotifications()
    {
        var filteredNotifications = Notifications.AsEnumerable();

        if (ShowUnreadOnly)
        {
            filteredNotifications = filteredNotifications.Where(n => !n.IsRead);
        }

        switch (NotificationFilter)
        {
            case NotificationFilter.Errors:
                filteredNotifications = filteredNotifications.Where(n => n.Type == NotificationType.Error);
                break;
            case NotificationFilter.Warnings:
                filteredNotifications = filteredNotifications.Where(n => n.Type == NotificationType.Warning);
                break;
            case NotificationFilter.Info:
                filteredNotifications = filteredNotifications.Where(n => n.Type == NotificationType.Info);
                break;
            case NotificationFilter.Success:
                filteredNotifications = filteredNotifications.Where(n => n.Type == NotificationType.Success);
                break;
        }

        var result = filteredNotifications.ToList();
        SortNotifications(result);
        return result;
    }

    #endregion

    #region UI Creation Helpers

    private UserControl CreateNotificationControl(HoloNotification notification)
    {
        var control = new UserControl
        {
            Height = 80,
            Margin = new Thickness(0, 5),
            Style = CreateNotificationStyle(notification)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Icon
        var icon = CreateNotificationIcon(notification.Type);
        Grid.SetColumn(icon, 0);
        grid.Children.Add(icon);

        // Content
        var contentPanel = new StackPanel { Margin = new Thickness(10, 5) };
        
        var titleText = new TextBlock
        {
            Text = notification.Title,
            FontWeight = FontWeights.Bold,
            FontSize = 14,
            Foreground = GetNotificationColor(notification.Type)
        };
        contentPanel.Children.Add(titleText);

        var messageText = new TextBlock
        {
            Text = notification.Message,
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 200, 200)),
            TextWrapping = TextWrapping.Wrap
        };
        contentPanel.Children.Add(messageText);

        var timeText = new TextBlock
        {
            Text = FormatRelativeTime(notification.Timestamp),
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(150, 150, 150, 150))
        };
        contentPanel.Children.Add(timeText);

        Grid.SetColumn(contentPanel, 1);
        grid.Children.Add(contentPanel);

        // Actions
        if (notification.Actions?.Any() == true)
        {
            var actionPanel = new StackPanel { Orientation = Orientation.Vertical };
            
            foreach (var action in notification.Actions.Take(2))
            {
                var actionButton = new Button
                {
                    Content = action.Text,
                    Width = 80,
                    Height = 25,
                    Margin = new Thickness(0, 2),
                    Style = CreateHolographicButtonStyle(),
                    Tag = new { NotificationId = notification.Id, ActionType = action.ActionType }
                };
                actionButton.Click += OnNotificationActionClicked;
                actionPanel.Children.Add(actionButton);
            }

            Grid.SetColumn(actionPanel, 2);
            grid.Children.Add(actionPanel);
        }

        control.Content = grid;
        return control;
    }

    private Style CreateNotificationStyle(HoloNotification notification)
    {
        var style = new Style(typeof(UserControl));
        
        var backgroundBrush = new SolidColorBrush(Color.FromArgb(
            notification.IsRead ? (byte)10 : (byte)20,
            100, 200, 255));
        
        style.Setters.Add(new Setter(BackgroundProperty, backgroundBrush));
        style.Setters.Add(new Setter(BorderBrushProperty, GetNotificationColor(notification.Type)));
        style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(1, 0, 0, 0)));
        
        return style;
    }

    private UIElement CreateNotificationIcon(NotificationType type)
    {
        var iconSize = 24;
        var icon = new Ellipse
        {
            Width = iconSize,
            Height = iconSize,
            Fill = GetNotificationColor(type),
            Margin = new Thickness(10)
        };

        switch (type)
        {
            case NotificationType.Error:
                icon.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 100, 100));
                break;
            case NotificationType.Warning:
                icon.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 200, 100));
                break;
            case NotificationType.Success:
                icon.Fill = new SolidColorBrush(Color.FromArgb(255, 100, 255, 100));
                break;
            case NotificationType.Info:
                icon.Fill = new SolidColorBrush(Color.FromArgb(255, 100, 200, 255));
                break;
        }

        return icon;
    }

    private SolidColorBrush GetNotificationColor(NotificationType type)
    {
        return type switch
        {
            NotificationType.Error => new SolidColorBrush(Color.FromArgb(255, 255, 100, 100)),
            NotificationType.Warning => new SolidColorBrush(Color.FromArgb(255, 255, 200, 100)),
            NotificationType.Success => new SolidColorBrush(Color.FromArgb(255, 100, 255, 100)),
            NotificationType.Info => new SolidColorBrush(Color.FromArgb(255, 100, 200, 255)),
            _ => new SolidColorBrush(Color.FromArgb(255, 200, 200, 200))
        };
    }

    private Style CreateHolographicButtonStyle()
    {
        var style = new Style(typeof(Button));
        style.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Color.FromArgb(40, 100, 200, 255))));
        style.Setters.Add(new Setter(BorderBrushProperty, new SolidColorBrush(Color.FromArgb(120, 100, 200, 255))));
        style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(ForegroundProperty, new SolidColorBrush(Color.FromArgb(255, 200, 200, 200))));
        style.Setters.Add(new Setter(FontSizeProperty, 11.0));
        style.Setters.Add(new Setter(FontWeightProperty, FontWeights.Bold));
        return style;
    }

    #endregion

    #region Animation Methods

    private void AnimateNewNotification(HoloNotification notification)
    {
        if (_isSimplifiedMode) return;

        var slideAnimation = new DoubleAnimation
        {
            From = -100,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(300)
        };

        // Apply animations to the notification control when it's created
        Dispatcher.BeginInvoke(() =>
        {
            RefreshNotificationDisplay();
        });
    }

    private void AnimateNotificationRead(HoloNotification notification)
    {
        if (_isSimplifiedMode) return;

        var fadeAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0.6,
            Duration = TimeSpan.FromMilliseconds(200)
        };

        // Apply to the specific notification control
        RefreshNotificationDisplay();
    }

    private async Task AnimateNotificationDismissal(HoloNotification notification)
    {
        if (_isSimplifiedMode) return;

        var slideAnimation = new DoubleAnimation
        {
            From = 0,
            To = 400,
            Duration = TimeSpan.FromMilliseconds(250),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        await Task.Delay(250);
    }

    private void SpawnNotificationParticles(HoloNotification notification)
    {
        if (_isSimplifiedMode || !EnableParticleEffects) return;

        var particleCount = notification.Priority == NotificationPriority.Critical ? 20 : 10;
        var color = GetNotificationColor(notification.Type).Color;

        for (int i = 0; i < particleCount; i++)
        {
            var particle = new Ellipse
            {
                Width = _random.Next(2, 6),
                Height = _random.Next(2, 6),
                Fill = new SolidColorBrush(Color.FromArgb(
                    (byte)_random.Next(100, 255), color.R, color.G, color.B))
            };

            Canvas.SetLeft(particle, _random.Next(0, 400));
            Canvas.SetTop(particle, _random.Next(0, 100));
            _particleCanvas.Children.Add(particle);
            _particles.Add(particle);

            AnimateParticle(particle);
        }

        CleanupParticles();
    }

    private void SpawnActionParticles()
    {
        if (_isSimplifiedMode || !EnableParticleEffects) return;

        for (int i = 0; i < 15; i++)
        {
            var particle = new Ellipse
            {
                Width = _random.Next(3, 8),
                Height = _random.Next(3, 8),
                Fill = new SolidColorBrush(Color.FromArgb(
                    (byte)_random.Next(150, 255), 100, 255, 100))
            };

            Canvas.SetLeft(particle, _random.Next(300, 400));
            Canvas.SetTop(particle, _random.Next(550, 600));
            _particleCanvas.Children.Add(particle);
            _particles.Add(particle);

            AnimateParticle(particle);
        }

        CleanupParticles();
    }

    private void AnimateParticle(UIElement particle)
    {
        var moveAnimation = new DoubleAnimation
        {
            From = Canvas.GetTop(particle),
            To = Canvas.GetTop(particle) - _random.Next(50, 150),
            Duration = TimeSpan.FromMilliseconds(_random.Next(1000, 2000)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(_random.Next(1000, 2000))
        };

        moveAnimation.Completed += (s, e) =>
        {
            _particleCanvas.Children.Remove(particle);
            _particles.Remove(particle);
        };

        particle.BeginAnimation(Canvas.TopProperty, moveAnimation);
        particle.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void CleanupParticles()
    {
        if (_particles.Count > 100)
        {
            var particlesToRemove = _particles.Take(50).ToList();
            foreach (var particle in particlesToRemove)
            {
                _particleCanvas.Children.Remove(particle);
                _particles.Remove(particle);
            }
        }
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        RefreshNotificationDisplay();
        if (EnableNotificationAnimations && !_isSimplifiedMode)
        {
            AnimateInitialLoad();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _notificationTimer?.Stop();
        CleanupAllParticles();
    }

    private void OnNotificationsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdateNotificationCounter();
        RefreshNotificationDisplay();
    }

    private void OnNotificationTimerTick(object sender, EventArgs e)
    {
        // Simulate receiving new notifications
        if (_random.NextDouble() < 0.3) // 30% chance every 5 seconds
        {
            GenerateRandomNotification();
        }
    }

    private async void OnNotificationActionClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is dynamic tag)
        {
            await ExecuteNotificationActionAsync(tag.NotificationId, tag.ActionType);
        }
    }

    private async void OnMarkAllReadClicked(object sender, RoutedEventArgs e)
    {
        var unreadNotifications = Notifications.Where(n => !n.IsRead).ToList();
        
        foreach (var notification in unreadNotifications)
        {
            await MarkAsReadAsync(notification.Id);
        }

        if (EnableParticleEffects && !_isSimplifiedMode)
        {
            SpawnActionParticles();
        }
    }

    private async void OnClearAllClicked(object sender, RoutedEventArgs e)
    {
        var notificationsToRemove = Notifications.ToList();
        
        foreach (var notification in notificationsToRemove)
        {
            await DismissNotificationAsync(notification.Id);
        }

        AllNotificationsCleared?.Invoke(this, new HoloNotificationCenterEventArgs
        {
            Action = NotificationCenterAction.ClearAll,
            Timestamp = DateTime.Now
        });
    }

    private void OnSortModeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_sortComboBox.SelectedItem is ComboBoxItem item && item.Tag is NotificationSortMode mode)
        {
            SortMode = mode;
            FilterNotifications();
        }
    }

    #endregion

    #region Helper Methods

    private void UpdateNotificationCounter()
    {
        if (_notificationCounter == null) return;

        var totalCount = Notifications.Count;
        var unreadCount = Notifications.Count(n => !n.IsRead);
        
        _notificationCounter.Text = unreadCount > 0 
            ? $"{unreadCount} unread / {totalCount} total"
            : $"{totalCount} total";
    }

    private string FormatRelativeTime(DateTime timestamp)
    {
        var elapsed = DateTime.Now - timestamp;
        
        if (elapsed.TotalMinutes < 1)
            return "Just now";
        if (elapsed.TotalMinutes < 60)
            return $"{(int)elapsed.TotalMinutes}m ago";
        if (elapsed.TotalHours < 24)
            return $"{(int)elapsed.TotalHours}h ago";
        
        return timestamp.ToString("MMM dd, HH:mm");
    }

    private void PlayNotificationSound(NotificationType type)
    {
        // Placeholder for sound playback
        // In a real implementation, this would play appropriate sounds
    }

    private void ShowAdvancedFilters()
    {
        // Show additional filter options
    }

    private void HideAdvancedFilters()
    {
        // Hide additional filter options
    }

    private void AnimateInitialLoad()
    {
        var fadeAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(500)
        };
        BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void CleanupAllParticles()
    {
        foreach (var particle in _particles.ToList())
        {
            _particleCanvas.Children.Remove(particle);
        }
        _particles.Clear();
    }

    private void GenerateRandomNotification()
    {
        var notificationTypes = new[]
        {
            ("Skill Training", "Advanced Weapon Upgrades V has finished training.", NotificationType.Success, NotificationPriority.Medium),
            ("Market Alert", "Helium Isotopes price changed by 12% in Amarr.", NotificationType.Warning, NotificationPriority.High),
            ("System Update", "Gideon has been updated to version 2.1.3.", NotificationType.Info, NotificationPriority.Low),
            ("API Warning", "ESI rate limit approaching. Reducing request frequency.", NotificationType.Warning, NotificationPriority.Medium)
        };

        var (title, message, type, priority) = notificationTypes[_random.Next(notificationTypes.Length)];
        
        var notification = new HoloNotification
        {
            Title = title,
            Message = message,
            Type = type,
            Priority = priority,
            Source = "System",
            Actions = new List<HoloNotificationAction>
            {
                new HoloNotificationAction { Text = "View", ActionType = "view_details" }
            }
        };

        Task.Run(() => AddNotificationAsync(notification));
    }

    #endregion

    #region Property Change Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloNotificationCenter control)
        {
            control.ApplyHolographicIntensity();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloNotificationCenter control)
        {
            control.ApplyEVEColorScheme();
        }
    }

    private static void OnNotificationFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloNotificationCenter control)
        {
            control.FilterNotifications();
            control.FilterChanged?.Invoke(control, new HoloNotificationCenterEventArgs
            {
                Filter = control.NotificationFilter,
                Timestamp = DateTime.Now
            });
        }
    }

    private static void OnSortModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloNotificationCenter control)
        {
            control.FilterNotifications();
        }
    }

    private static void OnEnableNotificationAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Animation state changed
    }

    private static void OnEnableSoundAlertsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Sound alert state changed
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloNotificationCenter control && !(bool)e.NewValue)
        {
            control.CleanupAllParticles();
        }
    }

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode => _isSimplifiedMode;

    public void EnterSimplifiedMode()
    {
        _isSimplifiedMode = true;
        CleanupAllParticles();
        EnableNotificationAnimations = false;
        EnableParticleEffects = false;
    }

    public void ExitSimplifiedMode()
    {
        _isSimplifiedMode = false;
        EnableNotificationAnimations = true;
        EnableParticleEffects = true;
    }

    #endregion

    #region Color Scheme Application

    private void ApplyEVEColorScheme()
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        
        if (BorderBrush is SolidColorBrush borderBrush)
        {
            borderBrush.Color = Color.FromArgb(80, colors.Primary.R, colors.Primary.G, colors.Primary.B);
        }

        if (Background is SolidColorBrush backgroundBrush)
        {
            backgroundBrush.Color = Color.FromArgb(20, colors.Primary.R, colors.Primary.G, colors.Primary.B);
        }

        if (Effect is DropShadowEffect shadow)
        {
            shadow.Color = Color.FromArgb(60, colors.Primary.R, colors.Primary.G, colors.Primary.B);
        }
    }

    private void ApplyHolographicIntensity()
    {
        var intensity = HolographicIntensity;
        
        if (Effect is DropShadowEffect shadow)
        {
            shadow.BlurRadius = 10 * intensity;
            shadow.ShadowDepth = 3 * intensity;
        }

        EnableNotificationAnimations = intensity > 0.3;
        EnableParticleEffects = intensity > 0.5;
    }

    #endregion
}

#region Supporting Classes and Enums

public class HoloNotification
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsRead { get; set; }
    public string Source { get; set; }
    public List<HoloNotificationAction> Actions { get; set; }
}

public class HoloNotificationAction
{
    public string Text { get; set; }
    public string ActionType { get; set; }
}

public class HoloAlertSetting
{
    public string Id { get; set; }
    public string Name { get; set; }
    public NotificationType Type { get; set; }
    public bool IsEnabled { get; set; }
    public Dictionary<string, object> Conditions { get; set; }
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}

public enum NotificationPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum NotificationFilter
{
    All,
    Errors,
    Warnings,
    Info,
    Success
}

public enum NotificationSortMode
{
    Priority,
    Timestamp,
    Type
}

public enum NotificationCenterAction
{
    FilterChanged,
    SortChanged,
    MarkAllRead,
    ClearAll
}

public class HoloNotificationEventArgs : EventArgs
{
    public HoloNotification Notification { get; set; }
    public HoloNotificationAction Action { get; set; }
    public DateTime Timestamp { get; set; }
}

public class HoloNotificationCenterEventArgs : EventArgs
{
    public NotificationFilter Filter { get; set; }
    public NotificationCenterAction Action { get; set; }
    public DateTime Timestamp { get; set; }
}

#endregion