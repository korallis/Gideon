// ==========================================================================
// HoloStatusBar.cs - Holographic Status Bar with Real-time Data Streams
// ==========================================================================
// Advanced holographic status bar featuring real-time data streams,
// system monitoring, EVE-style visual effects, and adaptive performance.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Advanced holographic status bar with real-time data streams and EVE styling
/// </summary>
public class HoloStatusBar : Control, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty StatusItemsProperty =
        DependencyProperty.Register(nameof(StatusItems), typeof(ObservableCollection<HoloStatusItem>), typeof(HoloStatusBar),
            new PropertyMetadata(null, OnStatusItemsChanged));

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloStatusBar),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloStatusBar),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty EnableDataStreamProperty =
        DependencyProperty.Register(nameof(EnableDataStream), typeof(bool), typeof(HoloStatusBar),
            new PropertyMetadata(true, OnEnableDataStreamChanged));

    public static readonly DependencyProperty EnableSystemMonitoringProperty =
        DependencyProperty.Register(nameof(EnableSystemMonitoring), typeof(bool), typeof(HoloStatusBar),
            new PropertyMetadata(true, OnEnableSystemMonitoringChanged));

    public static readonly DependencyProperty ShowTimestampProperty =
        DependencyProperty.Register(nameof(ShowTimestamp), typeof(bool), typeof(HoloStatusBar),
            new PropertyMetadata(true));

    public static readonly DependencyProperty ShowConnectionStatusProperty =
        DependencyProperty.Register(nameof(ShowConnectionStatus), typeof(bool), typeof(HoloStatusBar),
            new PropertyMetadata(true));

    public static readonly DependencyProperty ConnectionStatusProperty =
        DependencyProperty.Register(nameof(ConnectionStatus), typeof(ConnectionStatus), typeof(HoloStatusBar),
            new PropertyMetadata(ConnectionStatus.Connected, OnConnectionStatusChanged));

    public static readonly DependencyProperty SystemLoadProperty =
        DependencyProperty.Register(nameof(SystemLoad), typeof(double), typeof(HoloStatusBar),
            new PropertyMetadata(0.0, OnSystemLoadChanged));

    public static readonly DependencyProperty MemoryUsageProperty =
        DependencyProperty.Register(nameof(MemoryUsage), typeof(double), typeof(HoloStatusBar),
            new PropertyMetadata(0.0, OnMemoryUsageChanged));

    public ObservableCollection<HoloStatusItem> StatusItems
    {
        get => (ObservableCollection<HoloStatusItem>)GetValue(StatusItemsProperty);
        set => SetValue(StatusItemsProperty, value);
    }

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

    public bool EnableDataStream
    {
        get => (bool)GetValue(EnableDataStreamProperty);
        set => SetValue(EnableDataStreamProperty, value);
    }

    public bool EnableSystemMonitoring
    {
        get => (bool)GetValue(EnableSystemMonitoringProperty);
        set => SetValue(EnableSystemMonitoringProperty, value);
    }

    public bool ShowTimestamp
    {
        get => (bool)GetValue(ShowTimestampProperty);
        set => SetValue(ShowTimestampProperty, value);
    }

    public bool ShowConnectionStatus
    {
        get => (bool)GetValue(ShowConnectionStatusProperty);
        set => SetValue(ShowConnectionStatusProperty, value);
    }

    public ConnectionStatus ConnectionStatus
    {
        get => (ConnectionStatus)GetValue(ConnectionStatusProperty);
        set => SetValue(ConnectionStatusProperty, value);
    }

    public double SystemLoad
    {
        get => (double)GetValue(SystemLoadProperty);
        set => SetValue(SystemLoadProperty, value);
    }

    public double MemoryUsage
    {
        get => (double)GetValue(MemoryUsageProperty);
        set => SetValue(MemoryUsageProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloStatusEventArgs> StatusItemClicked;
    public event EventHandler<HoloStatusEventArgs> StatusItemUpdated;

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode { get; private set; }

    public void SwitchToFullMode()
    {
        IsInSimplifiedMode = false;
        EnableDataStream = true;
        EnableSystemMonitoring = true;
        UpdateStatusBarAppearance();
    }

    public void SwitchToSimplifiedMode()
    {
        IsInSimplifiedMode = true;
        EnableDataStream = false;
        EnableSystemMonitoring = false;
        UpdateStatusBarAppearance();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => !_disposed && IsLoaded;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        EnableDataStream = settings.EnabledFeatures.HasFlag(AnimationFeatures.ParticleEffects);
        
        UpdateStatusBarAppearance();
    }

    #endregion

    #region Fields

    private Grid _mainGrid;
    private StackPanel _leftPanel;
    private StackPanel _centerPanel;
    private StackPanel _rightPanel;
    private Canvas _dataStreamCanvas;
    private Border _statusBorder;
    private TextBlock _timestampText;
    private Border _connectionIndicator;
    private ProgressBar _systemLoadBar;
    private ProgressBar _memoryUsageBar;
    
    private readonly List<StreamDataParticle> _streamParticles = new();
    private DispatcherTimer _dataStreamTimer;
    private DispatcherTimer _timestampTimer;
    private DispatcherTimer _systemMonitorTimer;
    private double _streamPhase = 0;
    private readonly Random _random = new();
    private bool _disposed = false;

    #endregion

    #region Constructor

    public HoloStatusBar()
    {
        DefaultStyleKey = typeof(HoloStatusBar);
        StatusItems = new ObservableCollection<HoloStatusItem>();
        Height = 32;
        InitializeStatusBar();
        SetupTimers();
        
        // Register with animation intensity manager
        AnimationIntensityManager.Instance.RegisterTarget(this);
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Add a new status item
    /// </summary>
    public void AddStatusItem(HoloStatusItem item)
    {
        if (item != null && !StatusItems.Contains(item))
        {
            StatusItems.Add(item);
            StatusItemUpdated?.Invoke(this, new HoloStatusEventArgs { Item = item });
        }
    }

    /// <summary>
    /// Remove a status item
    /// </summary>
    public void RemoveStatusItem(HoloStatusItem item)
    {
        if (item != null)
        {
            StatusItems.Remove(item);
        }
    }

    /// <summary>
    /// Update system metrics
    /// </summary>
    public void UpdateSystemMetrics(double cpuUsage, double memoryUsage)
    {
        SystemLoad = Math.Clamp(cpuUsage, 0, 100);
        MemoryUsage = Math.Clamp(memoryUsage, 0, 100);
    }

    /// <summary>
    /// Flash status bar for notifications
    /// </summary>
    public void FlashNotification(Color color, TimeSpan duration)
    {
        if (IsInSimplifiedMode) return;

        var flashAnimation = new ColorAnimation
        {
            To = color,
            Duration = TimeSpan.FromMilliseconds(100),
            AutoReverse = true,
            RepeatBehavior = new RepeatBehavior(duration.TotalMilliseconds / 200)
        };

        if (_statusBorder?.Background is SolidColorBrush brush)
        {
            brush.BeginAnimation(SolidColorBrush.ColorProperty, flashAnimation);
        }
    }

    #endregion

    #region Private Methods

    private void InitializeStatusBar()
    {
        Template = CreateStatusBarTemplate();
        UpdateStatusBarAppearance();
    }

    private ControlTemplate CreateStatusBarTemplate()
    {
        var template = new ControlTemplate(typeof(HoloStatusBar));

        // Main border
        var statusBorder = new FrameworkElementFactory(typeof(Border));
        statusBorder.Name = "PART_StatusBorder";
        statusBorder.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 0));

        // Main grid container
        var mainGrid = new FrameworkElementFactory(typeof(Grid));
        mainGrid.Name = "PART_MainGrid";

        // Column definitions
        var leftColumn = new FrameworkElementFactory(typeof(ColumnDefinition));
        leftColumn.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
        var centerColumn = new FrameworkElementFactory(typeof(ColumnDefinition));
        centerColumn.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);
        var rightColumn = new FrameworkElementFactory(typeof(ColumnDefinition));
        rightColumn.SetValue(ColumnDefinition.WidthProperty, new GridLength(200));

        mainGrid.AppendChild(leftColumn);
        mainGrid.AppendChild(centerColumn);
        mainGrid.AppendChild(rightColumn);

        // Left panel for status items
        var leftPanel = new FrameworkElementFactory(typeof(StackPanel));
        leftPanel.Name = "PART_LeftPanel";
        leftPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        leftPanel.SetValue(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Center);
        leftPanel.SetValue(StackPanel.MarginProperty, new Thickness(8, 0, 0, 0));
        leftPanel.SetValue(Grid.ColumnProperty, 0);

        // Center panel for primary status
        var centerPanel = new FrameworkElementFactory(typeof(StackPanel));
        centerPanel.Name = "PART_CenterPanel";
        centerPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        centerPanel.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        centerPanel.SetValue(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Center);
        centerPanel.SetValue(Grid.ColumnProperty, 1);

        // Right panel for system info
        var rightPanel = new FrameworkElementFactory(typeof(StackPanel));
        rightPanel.Name = "PART_RightPanel";
        rightPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        rightPanel.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Right);
        rightPanel.SetValue(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Center);
        rightPanel.SetValue(StackPanel.MarginProperty, new Thickness(0, 0, 8, 0));
        rightPanel.SetValue(Grid.ColumnProperty, 2);

        // Data stream canvas
        var dataStreamCanvas = new FrameworkElementFactory(typeof(Canvas));
        dataStreamCanvas.Name = "PART_DataStreamCanvas";
        dataStreamCanvas.SetValue(Canvas.IsHitTestVisibleProperty, false);
        dataStreamCanvas.SetValue(Canvas.ClipToBoundsProperty, true);
        dataStreamCanvas.SetValue(Grid.ColumnSpanProperty, 3);

        // Assembly
        mainGrid.AppendChild(leftPanel);
        mainGrid.AppendChild(centerPanel);
        mainGrid.AppendChild(rightPanel);
        mainGrid.AppendChild(dataStreamCanvas);
        statusBorder.AppendChild(mainGrid);

        template.VisualTree = statusBorder;
        return template;
    }

    private void SetupTimers()
    {
        // Data stream animation timer
        _dataStreamTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50) // 20 FPS
        };
        _dataStreamTimer.Tick += OnDataStreamTick;

        // Timestamp update timer
        _timestampTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timestampTimer.Tick += OnTimestampTick;

        // System monitoring timer
        _systemMonitorTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _systemMonitorTimer.Tick += OnSystemMonitorTick;
    }

    private void OnDataStreamTick(object sender, EventArgs e)
    {
        if (!EnableDataStream || IsInSimplifiedMode || _dataStreamCanvas == null) return;

        _streamPhase += 0.1;
        if (_streamPhase > Math.PI * 2)
            _streamPhase = 0;

        UpdateStreamParticles();
        SpawnStreamParticles();
    }

    private void OnTimestampTick(object sender, EventArgs e)
    {
        if (_timestampText != null && ShowTimestamp)
        {
            _timestampText.Text = DateTime.Now.ToString("HH:mm:ss");
        }
    }

    private void OnSystemMonitorTick(object sender, EventArgs e)
    {
        if (!EnableSystemMonitoring) return;

        // Simulate system monitoring (in real implementation, get actual system metrics)
        var cpuUsage = _random.NextDouble() * 100;
        var memoryUsage = _random.NextDouble() * 100;
        
        UpdateSystemMetrics(cpuUsage, memoryUsage);
    }

    private void UpdateStreamParticles()
    {
        for (int i = _streamParticles.Count - 1; i >= 0; i--)
        {
            var particle = _streamParticles[i];
            
            // Update particle position
            particle.X += particle.VelocityX;
            particle.Y += particle.VelocityY * Math.Sin(_streamPhase + particle.Phase);
            particle.Life -= 0.02;

            // Update visual position
            Canvas.SetLeft(particle.Visual, particle.X);
            Canvas.SetTop(particle.Visual, particle.Y);
            
            // Update opacity
            particle.Visual.Opacity = Math.Max(0, particle.Life) * HolographicIntensity;

            // Remove dead particles
            if (particle.Life <= 0 || particle.X > ActualWidth + 10)
            {
                _dataStreamCanvas.Children.Remove(particle.Visual);
                _streamParticles.RemoveAt(i);
            }
        }
    }

    private void SpawnStreamParticles()
    {
        if (_streamParticles.Count >= 30) return; // Limit particle count

        if (_random.NextDouble() < 0.3) // 30% chance to spawn
        {
            var particle = CreateStreamParticle();
            _streamParticles.Add(particle);
            _dataStreamCanvas.Children.Add(particle.Visual);
        }
    }

    private StreamDataParticle CreateStreamParticle()
    {
        var color = GetEVEColor(EVEColorScheme);
        var size = 1 + _random.NextDouble() * 2;
        
        var shape = _random.Next(3) switch
        {
            0 => new Ellipse
            {
                Width = size,
                Height = size,
                Fill = new SolidColorBrush(Color.FromArgb(180, color.R, color.G, color.B))
            },
            1 => new Rectangle
            {
                Width = size,
                Height = size,
                Fill = new SolidColorBrush(Color.FromArgb(160, color.R, color.G, color.B))
            },
            _ => new Line
            {
                X1 = 0, Y1 = 0, X2 = size * 2, Y2 = 0,
                Stroke = new SolidColorBrush(Color.FromArgb(200, color.R, color.G, color.B)),
                StrokeThickness = 1
            }
        };

        var particle = new StreamDataParticle
        {
            Visual = shape,
            X = -size,
            Y = _random.NextDouble() * ActualHeight,
            VelocityX = 20 + _random.NextDouble() * 30,
            VelocityY = (_random.NextDouble() - 0.5) * 5,
            Life = 1.0,
            Phase = _random.NextDouble() * Math.PI * 2
        };

        Canvas.SetLeft(shape, particle.X);
        Canvas.SetTop(shape, particle.Y);

        return particle;
    }

    private void UpdateStatusBarAppearance()
    {
        if (Template == null) return;

        Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            ApplyTemplate();
            
            _mainGrid = GetTemplateChild("PART_MainGrid") as Grid;
            _leftPanel = GetTemplateChild("PART_LeftPanel") as StackPanel;
            _centerPanel = GetTemplateChild("PART_CenterPanel") as StackPanel;
            _rightPanel = GetTemplateChild("PART_RightPanel") as StackPanel;
            _dataStreamCanvas = GetTemplateChild("PART_DataStreamCanvas") as Canvas;
            _statusBorder = GetTemplateChild("PART_StatusBorder") as Border;

            UpdateColors();
            UpdateEffects();
            RebuildStatusItems();
            SetupSystemMonitoring();
            SetupTimestamp();
        }), DispatcherPriority.Render);
    }

    private void UpdateColors()
    {
        var color = GetEVEColor(EVEColorScheme);

        if (_statusBorder != null)
        {
            var backgroundBrush = new LinearGradientBrush();
            backgroundBrush.StartPoint = new Point(0, 0);
            backgroundBrush.EndPoint = new Point(0, 1);
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(80, 0, 20, 40), 0.0));
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(60, 0, 20, 40), 0.5));
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(80, 0, 20, 40), 1.0));

            _statusBorder.Background = backgroundBrush;
            _statusBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(
                120, color.R, color.G, color.B));
        }
    }

    private void UpdateEffects()
    {
        var color = GetEVEColor(EVEColorScheme);

        if (_statusBorder != null && !IsInSimplifiedMode)
        {
            _statusBorder.Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 4 * HolographicIntensity,
                ShadowDepth = 0,
                Opacity = 0.3 * HolographicIntensity,
                Direction = 90
            };
        }
        else if (_statusBorder != null)
        {
            _statusBorder.Effect = null;
        }
    }

    private void RebuildStatusItems()
    {
        if (_leftPanel == null) return;

        // Clear existing items except separators
        var itemsToRemove = _leftPanel.Children.OfType<FrameworkElement>()
            .Where(e => e.Tag?.ToString() == "StatusItem").ToList();
        
        foreach (var item in itemsToRemove)
        {
            _leftPanel.Children.Remove(item);
        }

        // Add status items
        foreach (var statusItem in StatusItems)
        {
            var itemControl = CreateStatusItemControl(statusItem);
            _leftPanel.Children.Add(itemControl);
        }
    }

    private FrameworkElement CreateStatusItemControl(HoloStatusItem item)
    {
        var container = new Border
        {
            Tag = "StatusItem",
            Margin = new Thickness(4, 2, 4, 2),
            Padding = new Thickness(6, 2, 6, 2),
            CornerRadius = new CornerRadius(3),
            Background = new SolidColorBrush(GetStatusColor(item.Status)),
            Cursor = System.Windows.Input.Cursors.Hand
        };

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        // Status indicator
        var indicator = new Ellipse
        {
            Width = 6,
            Height = 6,
            Fill = new SolidColorBrush(GetStatusIndicatorColor(item.Status)),
            Margin = new Thickness(0, 0, 4, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        // Add pulsing animation for active/warning states
        if (item.Status == StatusType.Warning || item.Status == StatusType.Error)
        {
            var pulseAnimation = new DoubleAnimation
            {
                From = 0.4,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(1),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            indicator.BeginAnimation(OpacityProperty, pulseAnimation);
        }

        stackPanel.Children.Add(indicator);

        // Text
        var textBlock = new TextBlock
        {
            Text = item.Text,
            Foreground = Brushes.White,
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center
        };
        stackPanel.Children.Add(textBlock);

        container.Child = stackPanel;

        // Add click handler
        container.MouseLeftButtonUp += (s, e) =>
        {
            StatusItemClicked?.Invoke(this, new HoloStatusEventArgs { Item = item });
        };

        return container;
    }

    private void SetupSystemMonitoring()
    {
        if (_rightPanel == null || !EnableSystemMonitoring) return;

        _rightPanel.Children.Clear();

        // Connection status
        if (ShowConnectionStatus)
        {
            _connectionIndicator = new Border
            {
                Width = 12,
                Height = 12,
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(4, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            UpdateConnectionIndicator();
            _rightPanel.Children.Add(_connectionIndicator);
        }

        // System load
        var loadLabel = new TextBlock
        {
            Text = "CPU:",
            Foreground = new SolidColorBrush(GetEVEColor(EVEColorScheme)),
            FontSize = 9,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 2, 0)
        };
        _rightPanel.Children.Add(loadLabel);

        _systemLoadBar = new ProgressBar
        {
            Width = 40,
            Height = 8,
            Margin = new Thickness(0, 0, 8, 0),
            Minimum = 0,
            Maximum = 100,
            Value = SystemLoad
        };
        _rightPanel.Children.Add(_systemLoadBar);

        // Memory usage
        var memoryLabel = new TextBlock
        {
            Text = "MEM:",
            Foreground = new SolidColorBrush(GetEVEColor(EVEColorScheme)),
            FontSize = 9,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 2, 0)
        };
        _rightPanel.Children.Add(memoryLabel);

        _memoryUsageBar = new ProgressBar
        {
            Width = 40,
            Height = 8,
            Margin = new Thickness(0, 0, 8, 0),
            Minimum = 0,
            Maximum = 100,
            Value = MemoryUsage
        };
        _rightPanel.Children.Add(_memoryUsageBar);

        // Timestamp
        if (ShowTimestamp)
        {
            _timestampText = new TextBlock
            {
                Foreground = new SolidColorBrush(GetEVEColor(EVEColorScheme)),
                FontSize = 10,
                FontFamily = new FontFamily("Consolas"),
                VerticalAlignment = VerticalAlignment.Center
            };
            _rightPanel.Children.Add(_timestampText);
        }
    }

    private void SetupTimestamp()
    {
        if (_timestampText != null && ShowTimestamp)
        {
            _timestampText.Text = DateTime.Now.ToString("HH:mm:ss");
        }
    }

    private void UpdateConnectionIndicator()
    {
        if (_connectionIndicator == null) return;

        var (color, shouldPulse) = ConnectionStatus switch
        {
            ConnectionStatus.Connected => (Colors.LimeGreen, false),
            ConnectionStatus.Connecting => (Colors.Yellow, true),
            ConnectionStatus.Disconnected => (Colors.Red, false),
            ConnectionStatus.Error => (Colors.OrangeRed, true),
            _ => (Colors.Gray, false)
        };

        _connectionIndicator.Background = new SolidColorBrush(color);

        if (shouldPulse && !IsInSimplifiedMode)
        {
            var pulseAnimation = new DoubleAnimation
            {
                From = 0.3,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(1),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            _connectionIndicator.BeginAnimation(OpacityProperty, pulseAnimation);
        }
        else
        {
            _connectionIndicator.BeginAnimation(OpacityProperty, null);
            _connectionIndicator.Opacity = 1.0;
        }
    }

    private Color GetStatusColor(StatusType status)
    {
        return status switch
        {
            StatusType.Normal => Color.FromArgb(60, 50, 205, 50),
            StatusType.Warning => Color.FromArgb(60, 255, 215, 0),
            StatusType.Error => Color.FromArgb(60, 220, 20, 60),
            StatusType.Info => Color.FromArgb(60, 64, 224, 255),
            _ => Color.FromArgb(60, 128, 128, 128)
        };
    }

    private Color GetStatusIndicatorColor(StatusType status)
    {
        return status switch
        {
            StatusType.Normal => Color.FromRgb(50, 205, 50),
            StatusType.Warning => Color.FromRgb(255, 215, 0),
            StatusType.Error => Color.FromRgb(220, 20, 60),
            StatusType.Info => Color.FromRgb(64, 224, 255),
            _ => Color.FromRgb(128, 128, 128)
        };
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
            _ => Color.FromRgb(64, 224, 255)
        };
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableDataStream && !IsInSimplifiedMode)
            _dataStreamTimer.Start();
            
        if (ShowTimestamp)
            _timestampTimer.Start();
            
        if (EnableSystemMonitoring)
            _systemMonitorTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _dataStreamTimer?.Stop();
        _timestampTimer?.Stop();
        _systemMonitorTimer?.Stop();
        
        // Clean up particles
        _streamParticles.Clear();
        _dataStreamCanvas?.Children.Clear();
        
        // Unregister from animation intensity manager
        AnimationIntensityManager.Instance.UnregisterTarget(this);
        
        _disposed = true;
    }

    #endregion

    #region Event Handlers

    private static void OnStatusItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloStatusBar statusBar)
        {
            statusBar.RebuildStatusItems();
        }
    }

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloStatusBar statusBar)
            statusBar.UpdateStatusBarAppearance();
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloStatusBar statusBar)
            statusBar.UpdateStatusBarAppearance();
    }

    private static void OnEnableDataStreamChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloStatusBar statusBar)
        {
            if ((bool)e.NewValue && !statusBar.IsInSimplifiedMode)
                statusBar._dataStreamTimer.Start();
            else
                statusBar._dataStreamTimer.Stop();
        }
    }

    private static void OnEnableSystemMonitoringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloStatusBar statusBar)
        {
            statusBar.UpdateStatusBarAppearance();
            
            if ((bool)e.NewValue)
                statusBar._systemMonitorTimer.Start();
            else
                statusBar._systemMonitorTimer.Stop();
        }
    }

    private static void OnConnectionStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloStatusBar statusBar)
            statusBar.UpdateConnectionIndicator();
    }

    private static void OnSystemLoadChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloStatusBar statusBar && statusBar._systemLoadBar != null)
        {
            statusBar._systemLoadBar.Value = (double)e.NewValue;
        }
    }

    private static void OnMemoryUsageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloStatusBar statusBar && statusBar._memoryUsageBar != null)
        {
            statusBar._memoryUsageBar.Value = (double)e.NewValue;
        }
    }

    #endregion
}

/// <summary>
/// Stream data particle for status bar effects
/// </summary>
internal class StreamDataParticle
{
    public FrameworkElement Visual { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
    public double Phase { get; set; }
}

/// <summary>
/// Status item for holographic status bar
/// </summary>
public class HoloStatusItem : INotifyPropertyChanged
{
    private string _text;
    private StatusType _status;
    private object _data;

    public string Text
    {
        get => _text;
        set { _text = value; OnPropertyChanged(); }
    }

    public StatusType Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }

    public object Data
    {
        get => _data;
        set { _data = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Status types for status items
/// </summary>
public enum StatusType
{
    Normal,
    Warning,
    Error,
    Info
}

/// <summary>
/// Connection status types
/// </summary>
public enum ConnectionStatus
{
    Connected,
    Connecting,
    Disconnected,
    Error
}

/// <summary>
/// Event args for holographic status bar events
/// </summary>
public class HoloStatusEventArgs : EventArgs
{
    public HoloStatusItem Item { get; set; }
}