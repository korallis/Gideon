// ==========================================================================
// RealtimeInfoDisplay.cs - Real-time Information Display System
// ==========================================================================
// Advanced real-time information display with live data updates,
// streaming information, and holographic UI integration.
//
// Author: Gideon Development Team
// Created: June 21, 2025
// ==========================================================================

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Information update types for different data streams
/// </summary>
public enum InfoUpdateType
{
    MarketPrice,
    CharacterUpdate,
    ShipStatus,
    SystemAlert,
    NewsUpdate,
    CorporationUpdate
}

/// <summary>
/// Priority levels for information display
/// </summary>
public enum InfoPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3,
    Emergency = 4
}

/// <summary>
/// Real-time information item
/// </summary>
public class RealtimeInfoItem : INotifyPropertyChanged
{
    private string _title = string.Empty;
    private string _content = string.Empty;
    private DateTime _timestamp = DateTime.Now;
    private InfoUpdateType _updateType = InfoUpdateType.SystemAlert;
    private InfoPriority _priority = InfoPriority.Normal;
    private bool _isRead = false;
    private Brush _accentColor = Brushes.Cyan;

    public string Title
    {
        get => _title;
        set { _title = value; OnPropertyChanged(); }
    }

    public string Content
    {
        get => _content;
        set { _content = value; OnPropertyChanged(); }
    }

    public DateTime Timestamp
    {
        get => _timestamp;
        set { _timestamp = value; OnPropertyChanged(); }
    }

    public InfoUpdateType UpdateType
    {
        get => _updateType;
        set { _updateType = value; OnPropertyChanged(); UpdateAccentColor(); }
    }

    public InfoPriority Priority
    {
        get => _priority;
        set { _priority = value; OnPropertyChanged(); }
    }

    public bool IsRead
    {
        get => _isRead;
        set { _isRead = value; OnPropertyChanged(); }
    }

    public Brush AccentColor
    {
        get => _accentColor;
        private set { _accentColor = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void UpdateAccentColor()
    {
        AccentColor = UpdateType switch
        {
            InfoUpdateType.MarketPrice => new SolidColorBrush(Color.FromRgb(0, 255, 255)), // Cyan
            InfoUpdateType.CharacterUpdate => new SolidColorBrush(Color.FromRgb(0, 255, 0)), // Green
            InfoUpdateType.ShipStatus => new SolidColorBrush(Color.FromRgb(255, 255, 0)), // Yellow
            InfoUpdateType.SystemAlert => new SolidColorBrush(Color.FromRgb(255, 165, 0)), // Orange
            InfoUpdateType.NewsUpdate => new SolidColorBrush(Color.FromRgb(255, 255, 255)), // White
            InfoUpdateType.CorporationUpdate => new SolidColorBrush(Color.FromRgb(128, 0, 255)), // Purple
            _ => new SolidColorBrush(Color.FromRgb(0, 255, 255))
        };
    }
}

/// <summary>
/// Real-time information display control
/// </summary>
public class RealtimeInfoDisplay : UserControl
{
    #region Dependency Properties

    public static readonly DependencyProperty InfoItemsProperty =
        DependencyProperty.Register(nameof(InfoItems), typeof(ObservableCollection<RealtimeInfoItem>), typeof(RealtimeInfoDisplay),
            new PropertyMetadata(null));

    public static readonly DependencyProperty MaxDisplayItemsProperty =
        DependencyProperty.Register(nameof(MaxDisplayItems), typeof(int), typeof(RealtimeInfoDisplay),
            new PropertyMetadata(50));

    public static readonly DependencyProperty AutoScrollProperty =
        DependencyProperty.Register(nameof(AutoScroll), typeof(bool), typeof(RealtimeInfoDisplay),
            new PropertyMetadata(true));

    public static readonly DependencyProperty ShowTimestampsProperty =
        DependencyProperty.Register(nameof(ShowTimestamps), typeof(bool), typeof(RealtimeInfoDisplay),
            new PropertyMetadata(true));

    #endregion

    #region Properties

    public ObservableCollection<RealtimeInfoItem> InfoItems
    {
        get => (ObservableCollection<RealtimeInfoItem>)GetValue(InfoItemsProperty);
        set => SetValue(InfoItemsProperty, value);
    }

    public int MaxDisplayItems
    {
        get => (int)GetValue(MaxDisplayItemsProperty);
        set => SetValue(MaxDisplayItemsProperty, value);
    }

    public bool AutoScroll
    {
        get => (bool)GetValue(AutoScrollProperty);
        set => SetValue(AutoScrollProperty, value);
    }

    public bool ShowTimestamps
    {
        get => (bool)GetValue(ShowTimestampsProperty);
        set => SetValue(ShowTimestampsProperty, value);
    }

    #endregion

    #region Fields

    private ScrollViewer? _scrollViewer;
    private ItemsControl? _itemsControl;
    private DispatcherTimer _updateTimer;

    #endregion

    #region Constructor

    public RealtimeInfoDisplay()
    {
        InfoItems = new ObservableCollection<RealtimeInfoItem>();
        
        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _updateTimer.Tick += UpdateTimer_Tick;
        _updateTimer.Start();

        InitializeComponent();
        
        InfoItems.CollectionChanged += InfoItems_CollectionChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Add new information item with automatic styling
    /// </summary>
    public void AddInfo(string title, string content, InfoUpdateType updateType, InfoPriority priority = InfoPriority.Normal)
    {
        var item = new RealtimeInfoItem
        {
            Title = title,
            Content = content,
            UpdateType = updateType,
            Priority = priority,
            Timestamp = DateTime.Now
        };

        Dispatcher.BeginInvoke(() =>
        {
            // Insert based on priority
            int insertIndex = 0;
            for (int i = 0; i < InfoItems.Count; i++)
            {
                if (InfoItems[i].Priority < priority)
                {
                    insertIndex = i;
                    break;
                }
                insertIndex = i + 1;
            }

            InfoItems.Insert(insertIndex, item);

            // Manage max items
            while (InfoItems.Count > MaxDisplayItems)
            {
                InfoItems.RemoveAt(InfoItems.Count - 1);
            }

            // Auto-scroll to new item if enabled
            if (AutoScroll && _scrollViewer != null)
            {
                _scrollViewer.ScrollToTop();
            }
        });
    }

    /// <summary>
    /// Clear all information items
    /// </summary>
    public void ClearInfo()
    {
        InfoItems.Clear();
    }

    /// <summary>
    /// Mark all items as read
    /// </summary>
    public void MarkAllAsRead()
    {
        foreach (var item in InfoItems)
        {
            item.IsRead = true;
        }
    }

    #endregion

    #region Private Methods

    private void InitializeComponent()
    {
        // Create main grid
        var mainGrid = new Grid();
        
        // Create scroll viewer
        _scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = Brushes.Transparent
        };

        // Create items control
        _itemsControl = new ItemsControl
        {
            Background = Brushes.Transparent
        };

        // Set up data template
        _itemsControl.ItemTemplate = CreateInfoItemTemplate();
        _itemsControl.SetBinding(ItemsControl.ItemsSourceProperty, 
            new System.Windows.Data.Binding(nameof(InfoItems)) { Source = this });

        _scrollViewer.Content = _itemsControl;
        mainGrid.Children.Add(_scrollViewer);
        
        Content = mainGrid;
    }

    private DataTemplate CreateInfoItemTemplate()
    {
        var template = new DataTemplate();
        
        // Create template content
        var factory = new FrameworkElementFactory(typeof(Border));
        factory.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromArgb(30, 0, 255, 255)));
        factory.SetValue(Border.BorderBrushProperty, new SolidColorBrush(Color.FromArgb(100, 0, 255, 255)));
        factory.SetValue(Border.BorderThicknessProperty, new Thickness(1));
        factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
        factory.SetValue(Border.MarginProperty, new Thickness(2));
        factory.SetValue(Border.PaddingProperty, new Thickness(8));

        // Create stack panel
        var stackPanel = new FrameworkElementFactory(typeof(StackPanel));
        
        // Title text
        var titleText = new FrameworkElementFactory(typeof(TextBlock));
        titleText.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding(nameof(RealtimeInfoItem.Title)));
        titleText.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        titleText.SetValue(TextBlock.FontSizeProperty, 12.0);
        titleText.SetBinding(TextBlock.ForegroundProperty, new System.Windows.Data.Binding(nameof(RealtimeInfoItem.AccentColor)));
        
        // Content text
        var contentText = new FrameworkElementFactory(typeof(TextBlock));
        contentText.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding(nameof(RealtimeInfoItem.Content)));
        contentText.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
        contentText.SetValue(TextBlock.FontSizeProperty, 10.0);
        contentText.SetValue(TextBlock.ForegroundProperty, Brushes.LightGray);
        contentText.SetValue(TextBlock.MarginProperty, new Thickness(0, 2, 0, 0));

        // Timestamp text
        var timestampText = new FrameworkElementFactory(typeof(TextBlock));
        timestampText.SetBinding(TextBlock.TextProperty, 
            new System.Windows.Data.Binding(nameof(RealtimeInfoItem.Timestamp)) 
            { 
                StringFormat = "HH:mm:ss" 
            });
        timestampText.SetValue(TextBlock.FontSizeProperty, 9.0);
        timestampText.SetValue(TextBlock.ForegroundProperty, Brushes.Gray);
        timestampText.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Right);
        timestampText.SetValue(TextBlock.MarginProperty, new Thickness(0, 2, 0, 0));

        stackPanel.AppendChild(titleText);
        stackPanel.AppendChild(contentText);
        stackPanel.AppendChild(timestampText);
        factory.AppendChild(stackPanel);

        template.VisualTree = factory;
        return template;
    }

    #endregion

    #region Event Handlers

    private void InfoItems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // Handle collection changes if needed
    }

    private void UpdateTimer_Tick(object? sender, EventArgs e)
    {
        // Update relative timestamps or other time-based operations
        // Could be used for auto-expiring items or refreshing display
    }

    #endregion
}