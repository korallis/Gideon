// ==========================================================================
// HoloPriceHistoryTimeline.cs - Holographic Price History Timeline
// ==========================================================================
// Advanced price history visualization featuring real-time timeline analysis,
// interactive price tracking, EVE-style temporal data, and holographic scrolling effects.
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
/// Holographic price history timeline with real-time temporal analysis and interactive navigation
/// </summary>
public class HoloPriceHistoryTimeline : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloPriceHistoryTimeline),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloPriceHistoryTimeline),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty PriceHistoryProperty =
        DependencyProperty.Register(nameof(PriceHistory), typeof(ObservableCollection<HoloPriceHistoryPoint>), typeof(HoloPriceHistoryTimeline),
            new PropertyMetadata(null, OnPriceHistoryChanged));

    public static readonly DependencyProperty TimeRangeProperty =
        DependencyProperty.Register(nameof(TimeRange), typeof(TimelineRange), typeof(HoloPriceHistoryTimeline),
            new PropertyMetadata(TimelineRange.Day, OnTimeRangeChanged));

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(HoloTimelineItem), typeof(HoloPriceHistoryTimeline),
            new PropertyMetadata(null, OnSelectedItemChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloPriceHistoryTimeline),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloPriceHistoryTimeline),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowTrendLinesProperty =
        DependencyProperty.Register(nameof(ShowTrendLines), typeof(bool), typeof(HoloPriceHistoryTimeline),
            new PropertyMetadata(true, OnShowTrendLinesChanged));

    public static readonly DependencyProperty ShowVolumeIndicatorsProperty =
        DependencyProperty.Register(nameof(ShowVolumeIndicators), typeof(bool), typeof(HoloPriceHistoryTimeline),
            new PropertyMetadata(true, OnShowVolumeIndicatorsChanged));

    public static readonly DependencyProperty ShowEventMarkersProperty =
        DependencyProperty.Register(nameof(ShowEventMarkers), typeof(bool), typeof(HoloPriceHistoryTimeline),
            new PropertyMetadata(true, OnShowEventMarkersChanged));

    public static readonly DependencyProperty AutoScrollProperty =
        DependencyProperty.Register(nameof(AutoScroll), typeof(bool), typeof(HoloPriceHistoryTimeline),
            new PropertyMetadata(true, OnAutoScrollChanged));

    public static readonly DependencyProperty ZoomLevelProperty =
        DependencyProperty.Register(nameof(ZoomLevel), typeof(double), typeof(HoloPriceHistoryTimeline),
            new PropertyMetadata(1.0, OnZoomLevelChanged));

    public static readonly DependencyProperty UpdateIntervalProperty =
        DependencyProperty.Register(nameof(UpdateInterval), typeof(TimeSpan), typeof(HoloPriceHistoryTimeline),
            new PropertyMetadata(TimeSpan.FromMilliseconds(100), OnUpdateIntervalChanged));

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

    public ObservableCollection<HoloPriceHistoryPoint> PriceHistory
    {
        get => (ObservableCollection<HoloPriceHistoryPoint>)GetValue(PriceHistoryProperty);
        set => SetValue(PriceHistoryProperty, value);
    }

    public TimelineRange TimeRange
    {
        get => (TimelineRange)GetValue(TimeRangeProperty);
        set => SetValue(TimeRangeProperty, value);
    }

    public HoloTimelineItem SelectedItem
    {
        get => (HoloTimelineItem)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
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

    public bool ShowTrendLines
    {
        get => (bool)GetValue(ShowTrendLinesProperty);
        set => SetValue(ShowTrendLinesProperty, value);
    }

    public bool ShowVolumeIndicators
    {
        get => (bool)GetValue(ShowVolumeIndicatorsProperty);
        set => SetValue(ShowVolumeIndicatorsProperty, value);
    }

    public bool ShowEventMarkers
    {
        get => (bool)GetValue(ShowEventMarkersProperty);
        set => SetValue(ShowEventMarkersProperty, value);
    }

    public bool AutoScroll
    {
        get => (bool)GetValue(AutoScrollProperty);
        set => SetValue(AutoScrollProperty, value);
    }

    public double ZoomLevel
    {
        get => (double)GetValue(ZoomLevelProperty);
        set => SetValue(ZoomLevelProperty, value);
    }

    public TimeSpan UpdateInterval
    {
        get => (TimeSpan)GetValue(UpdateIntervalProperty);
        set => SetValue(UpdateIntervalProperty, value);
    }

    #endregion

    #region Fields

    private Canvas _timelineCanvas;
    private Canvas _priceCanvas;
    private Canvas _volumeCanvas;
    private Canvas _particleCanvas;
    private Canvas _eventCanvas;
    private ScrollViewer _timelineScroller;
    
    private DispatcherTimer _updateTimer;
    private DispatcherTimer _animationTimer;
    private DispatcherTimer _scrollTimer;
    
    private readonly Random _random = new();
    private List<UIElement> _pricePoints;
    private List<UIElement> _trendLines;
    private List<UIElement> _volumeIndicators;
    private List<UIElement> _eventMarkers;
    private List<UIElement> _particles;
    
    // Timeline navigation
    private double _timelineOffset = 0;
    private double _maxPrice = 0;
    private double _minPrice = double.MaxValue;
    private DateTime _startTime;
    private DateTime _endTime;
    private double _pixelsPerHour = 100;
    
    // Animation state
    private double _animationProgress = 0.0;
    private bool _isAnimating = false;
    private double _scrollPosition = 0;
    
    // Interactive state
    private bool _isDragging = false;
    private Point _lastMousePosition;
    private HoloPriceHistoryPoint _hoveredPoint;
    
    // Performance optimization
    private bool _isInSimplifiedMode = false;
    private int _maxVisiblePoints = 1000;

    #endregion

    #region Constructor

    public HoloPriceHistoryTimeline()
    {
        InitializeComponent();
        InitializeTimelineSystem();
        SetupEventHandlers();
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 1000;
        Height = 400;
        Background = new SolidColorBrush(Color.FromArgb(20, 0, 50, 100));
        
        // Create main layout
        var mainGrid = new Grid();
        Content = mainGrid;

        // Create scroll viewer for timeline navigation
        _timelineScroller = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
            Background = Brushes.Transparent
        };

        // Create main timeline canvas
        _timelineCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            ClipToBounds = true
        };

        // Create layered canvases
        _priceCanvas = new Canvas { Background = Brushes.Transparent };
        _volumeCanvas = new Canvas { Background = Brushes.Transparent };
        _particleCanvas = new Canvas { Background = Brushes.Transparent };
        _eventCanvas = new Canvas { Background = Brushes.Transparent };

        _timelineCanvas.Children.Add(_volumeCanvas);
        _timelineCanvas.Children.Add(_priceCanvas);
        _timelineCanvas.Children.Add(_eventCanvas);
        _timelineCanvas.Children.Add(_particleCanvas);

        _timelineScroller.Content = _timelineCanvas;
        mainGrid.Children.Add(_timelineScroller);

        // Create overlay elements
        CreateTimelineOverlay(mainGrid);
    }

    private void CreateTimelineOverlay(Grid mainGrid)
    {
        // Create timeline controls overlay
        var controlsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(10),
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 50, 100))
        };

        // Zoom controls
        var zoomInButton = CreateHoloButton("ZOOM+", () => ZoomLevel *= 1.2);
        var zoomOutButton = CreateHoloButton("ZOOM-", () => ZoomLevel *= 0.8);
        var resetZoomButton = CreateHoloButton("RESET", () => ZoomLevel = 1.0);

        controlsPanel.Children.Add(zoomInButton);
        controlsPanel.Children.Add(zoomOutButton);
        controlsPanel.Children.Add(resetZoomButton);

        mainGrid.Children.Add(controlsPanel);

        // Create price scale on the left
        var priceScale = new Canvas
        {
            Width = 80,
            HorizontalAlignment = HorizontalAlignment.Left,
            Background = new SolidColorBrush(Color.FromArgb(50, 0, 100, 200))
        };

        mainGrid.Children.Add(priceScale);

        // Create time scale on the bottom
        var timeScale = new Canvas
        {
            Height = 40,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(80, 0, 0, 0),
            Background = new SolidColorBrush(Color.FromArgb(50, 0, 100, 200))
        };

        mainGrid.Children.Add(timeScale);
    }

    private Button CreateHoloButton(string text, Action onClick)
    {
        var button = new Button
        {
            Content = text,
            Width = 60,
            Height = 30,
            Margin = new Thickness(2),
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 150, 255)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 0, 200, 255)),
            BorderThickness = new Thickness(1),
            FontSize = 10,
            FontWeight = FontWeights.Bold
        };

        button.Click += (s, e) => onClick();
        return button;
    }

    private void InitializeTimelineSystem()
    {
        _pricePoints = new List<UIElement>();
        _trendLines = new List<UIElement>();
        _volumeIndicators = new List<UIElement>();
        _eventMarkers = new List<UIElement>();
        _particles = new List<UIElement>();
        
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
        
        _scrollTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _scrollTimer.Tick += ScrollTimer_Tick;
    }

    private void SetupEventHandlers()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
        MouseMove += OnMouseMove;
        MouseLeftButtonDown += OnMouseLeftButtonDown;
        MouseLeftButtonUp += OnMouseLeftButtonUp;
        MouseWheel += OnMouseWheel;
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        StartTimelineVisualization();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopTimelineVisualization();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateTimelineLayout();
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        var position = e.GetPosition(_timelineCanvas);
        UpdateHoverEffects(position);
        
        if (_isDragging)
        {
            var deltaX = position.X - _lastMousePosition.X;
            _timelineOffset -= deltaX / ZoomLevel;
            UpdateTimelinePosition();
            _lastMousePosition = position;
        }
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isDragging = true;
        _lastMousePosition = e.GetPosition(_timelineCanvas);
        _timelineCanvas.CaptureMouse();
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isDragging = false;
        _timelineCanvas.ReleaseMouseCapture();
        
        // Check for point selection
        var position = e.GetPosition(_timelineCanvas);
        SelectPointAtPosition(position);
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
        ZoomLevel *= zoomFactor;
        
        // Zoom towards mouse position
        var mousePosition = e.GetPosition(_timelineCanvas);
        var timeAtMouse = PositionToTime(mousePosition.X);
        UpdateTimelineLayout();
        var newPosition = TimeToPosition(timeAtMouse);
        _timelineOffset += (mousePosition.X - newPosition) / ZoomLevel;
        UpdateTimelinePosition();
    }

    private void UpdateTimer_Tick(object sender, EventArgs e)
    {
        UpdateTimelineData();
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

    private void ScrollTimer_Tick(object sender, EventArgs e)
    {
        if (AutoScroll && PriceHistory?.Any() == true)
        {
            // Auto-scroll to latest data
            var latestTime = PriceHistory.Max(p => p.Timestamp);
            ScrollToTime(latestTime);
        }
    }

    #endregion

    #region Timeline Visualization

    public void StartTimelineVisualization()
    {
        if (!_updateTimer.IsEnabled)
        {
            _updateTimer.Start();
            
            if (EnableAnimations)
            {
                _animationTimer.Start();
            }
            
            if (AutoScroll)
            {
                _scrollTimer.Start();
            }
            
            UpdateTimelineData();
        }
    }

    public void StopTimelineVisualization()
    {
        _updateTimer?.Stop();
        _animationTimer?.Stop();
        _scrollTimer?.Stop();
    }

    private void UpdateTimelineData()
    {
        if (PriceHistory == null || !PriceHistory.Any())
        {
            GenerateSampleData();
        }

        CalculateTimeBounds();
        UpdateTimelineLayout();
        RenderTimeline();
    }

    private void CalculateTimeBounds()
    {
        if (PriceHistory == null || !PriceHistory.Any()) return;

        _startTime = PriceHistory.Min(p => p.Timestamp);
        _endTime = PriceHistory.Max(p => p.Timestamp);
        _maxPrice = PriceHistory.Max(p => p.Price);
        _minPrice = PriceHistory.Min(p => p.Price);
        
        // Add padding
        var priceRange = _maxPrice - _minPrice;
        _maxPrice += priceRange * 0.1;
        _minPrice -= priceRange * 0.1;
    }

    private void UpdateTimelineLayout()
    {
        if (PriceHistory == null || !PriceHistory.Any()) return;

        var timeSpan = _endTime - _startTime;
        _pixelsPerHour = (ActualWidth - 100) * ZoomLevel / Math.Max(1, timeSpan.TotalHours);
        
        _timelineCanvas.Width = timeSpan.TotalHours * _pixelsPerHour + 200;
        _timelineCanvas.Height = ActualHeight;
        
        UpdateTimelinePosition();
    }

    private void UpdateTimelinePosition()
    {
        var transform = new TranslateTransform(_timelineOffset, 0);
        _timelineCanvas.RenderTransform = transform;
    }

    private void RenderTimeline()
    {
        ClearTimelineElements();
        
        if (PriceHistory == null || !PriceHistory.Any()) return;

        // Filter visible points for performance
        var visiblePoints = GetVisiblePoints();
        
        if (ShowVolumeIndicators)
        {
            RenderVolumeIndicators(visiblePoints);
        }
        
        RenderPriceChart(visiblePoints);
        
        if (ShowTrendLines)
        {
            RenderTrendLines(visiblePoints);
        }
        
        if (ShowEventMarkers)
        {
            RenderEventMarkers(visiblePoints);
        }
        
        if (EnableParticleEffects && !_isInSimplifiedMode)
        {
            CreateParticleEffects();
        }
        
        RenderTimeScale();
        RenderPriceScale();
        
        StartAnimation();
    }

    private List<HoloPriceHistoryPoint> GetVisiblePoints()
    {
        if (PriceHistory == null) return new List<HoloPriceHistoryPoint>();

        var visibleStart = PositionToTime(-_timelineOffset);
        var visibleEnd = PositionToTime(-_timelineOffset + ActualWidth);
        
        return PriceHistory
            .Where(p => p.Timestamp >= visibleStart && p.Timestamp <= visibleEnd)
            .Take(_maxVisiblePoints)
            .ToList();
    }

    private void RenderPriceChart(List<HoloPriceHistoryPoint> points)
    {
        if (points.Count < 2) return;

        var polyline = new Polyline
        {
            Stroke = new SolidColorBrush(Color.FromArgb(200, 0, 150, 255)),
            StrokeThickness = 2,
            Points = new PointCollection()
        };

        var area = new Polygon
        {
            Fill = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(100, 0, 150, 255), 0),
                    new GradientStop(Color.FromArgb(20, 0, 150, 255), 1)
                }
            },
            Points = new PointCollection()
        };

        foreach (var point in points)
        {
            var x = TimeToPosition(point.Timestamp);
            var y = PriceToPosition(point.Price);
            
            polyline.Points.Add(new Point(x, y));
            area.Points.Add(new Point(x, y));
            
            // Add price point marker
            var marker = CreatePricePointMarker(x, y, point);
            _priceCanvas.Children.Add(marker);
            _pricePoints.Add(marker);
        }

        // Close the area polygon
        if (area.Points.Count > 0)
        {
            area.Points.Add(new Point(area.Points.Last().X, ActualHeight - 40));
            area.Points.Add(new Point(area.Points.First().X, ActualHeight - 40));
        }

        _priceCanvas.Children.Add(area);
        _priceCanvas.Children.Add(polyline);
    }

    private UIElement CreatePricePointMarker(double x, double y, HoloPriceHistoryPoint point)
    {
        var marker = new Ellipse
        {
            Width = 6,
            Height = 6,
            Fill = new SolidColorBrush(Color.FromArgb(255, 0, 200, 255)),
            Stroke = new SolidColorBrush(Color.FromArgb(150, 255, 255, 255)),
            StrokeThickness = 1,
            Tag = point
        };

        Canvas.SetLeft(marker, x - 3);
        Canvas.SetTop(marker, y - 3);
        
        // Add glow effect
        marker.Effect = new DropShadowEffect
        {
            Color = Color.FromArgb(100, 0, 200, 255),
            BlurRadius = 8,
            ShadowDepth = 0
        };

        return marker;
    }

    private void RenderVolumeIndicators(List<HoloPriceHistoryPoint> points)
    {
        if (points.Count == 0) return;

        var maxVolume = points.Max(p => p.Volume);
        if (maxVolume == 0) return;

        foreach (var point in points)
        {
            var x = TimeToPosition(point.Timestamp);
            var volumeHeight = (point.Volume / maxVolume) * 100;
            
            var volumeBar = new Rectangle
            {
                Width = Math.Max(1, _pixelsPerHour / 24), // Hour width divided by 24 for minute resolution
                Height = volumeHeight,
                Fill = new SolidColorBrush(Color.FromArgb(100, 100, 255, 100)),
                Stroke = new SolidColorBrush(Color.FromArgb(150, 150, 255, 150)),
                StrokeThickness = 0.5
            };

            Canvas.SetLeft(volumeBar, x - volumeBar.Width / 2);
            Canvas.SetTop(volumeBar, ActualHeight - 40 - volumeHeight);
            
            _volumeCanvas.Children.Add(volumeBar);
            _volumeIndicators.Add(volumeBar);
        }
    }

    private void RenderTrendLines(List<HoloPriceHistoryPoint> points)
    {
        if (points.Count < 10) return; // Need sufficient data for trend analysis

        // Calculate short-term and long-term trends
        var shortTermPoints = points.TakeLast(20).ToList();
        var longTermPoints = points.TakeLast(50).ToList();
        
        RenderTrendLine(shortTermPoints, Color.FromArgb(150, 255, 200, 0), 1.5); // Short-term (yellow)
        RenderTrendLine(longTermPoints, Color.FromArgb(150, 255, 100, 100), 2); // Long-term (orange)
    }

    private void RenderTrendLine(List<HoloPriceHistoryPoint> points, Color color, double thickness)
    {
        if (points.Count < 2) return;

        // Simple linear regression
        var n = points.Count;
        var sumX = 0.0;
        var sumY = 0.0;
        var sumXY = 0.0;
        var sumX2 = 0.0;

        for (int i = 0; i < n; i++)
        {
            var x = i;
            var y = points[i].Price;
            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumX2 += x * x;
        }

        var slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        var intercept = (sumY - slope * sumX) / n;

        // Draw trend line
        var startPoint = new Point(
            TimeToPosition(points.First().Timestamp),
            PriceToPosition(intercept)
        );
        
        var endPoint = new Point(
            TimeToPosition(points.Last().Timestamp),
            PriceToPosition(intercept + slope * (n - 1))
        );

        var trendLine = new Line
        {
            X1 = startPoint.X,
            Y1 = startPoint.Y,
            X2 = endPoint.X,
            Y2 = endPoint.Y,
            Stroke = new SolidColorBrush(color),
            StrokeThickness = thickness,
            StrokeDashArray = new DoubleCollection { 5, 3 }
        };

        _priceCanvas.Children.Add(trendLine);
        _trendLines.Add(trendLine);
    }

    private void RenderEventMarkers(List<HoloPriceHistoryPoint> points)
    {
        // Detect significant price events
        var events = DetectPriceEvents(points);
        
        foreach (var eventPoint in events)
        {
            var x = TimeToPosition(eventPoint.Timestamp);
            var y = PriceToPosition(eventPoint.Price);
            
            var marker = CreateEventMarker(x, y, eventPoint);
            _eventCanvas.Children.Add(marker);
            _eventMarkers.Add(marker);
        }
    }

    private List<HoloPriceHistoryPoint> DetectPriceEvents(List<HoloPriceHistoryPoint> points)
    {
        var events = new List<HoloPriceHistoryPoint>();
        if (points.Count < 10) return events;

        for (int i = 5; i < points.Count - 5; i++)
        {
            var current = points[i];
            var before = points.Take(i).TakeLast(5).Average(p => p.Price);
            var after = points.Skip(i + 1).Take(5).Average(p => p.Price);
            
            // Detect significant price movements (>5%)
            var changeFromBefore = Math.Abs(current.Price - before) / before;
            var changeToAfter = Math.Abs(after - current.Price) / current.Price;
            
            if (changeFromBefore > 0.05 || changeToAfter > 0.05)
            {
                events.Add(current);
            }
        }
        
        return events;
    }

    private UIElement CreateEventMarker(double x, double y, HoloPriceHistoryPoint point)
    {
        var marker = new Path
        {
            Data = Geometry.Parse("M 0,0 L 8,8 L 0,16 L -8,8 Z"), // Diamond shape
            Fill = new SolidColorBrush(Color.FromArgb(200, 255, 100, 0)),
            Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 200, 0)),
            StrokeThickness = 1,
            Tag = point
        };

        Canvas.SetLeft(marker, x);
        Canvas.SetTop(marker, y - 8);
        
        // Add pulsing animation
        if (EnableAnimations)
        {
            var animation = new DoubleAnimation
            {
                From = 0.5,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(1),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };
            
            marker.BeginAnimation(OpacityProperty, animation);
        }

        return marker;
    }

    private void CreateParticleEffects()
    {
        // Create flowing particles along the price line
        for (int i = 0; i < 10; i++)
        {
            var particle = new Ellipse
            {
                Width = 3,
                Height = 3,
                Fill = new SolidColorBrush(Color.FromArgb(150, 0, 255, 200)),
                Tag = "particle"
            };

            var x = _random.NextDouble() * _timelineCanvas.Width;
            var y = _random.NextDouble() * (ActualHeight - 100) + 50;
            
            Canvas.SetLeft(particle, x);
            Canvas.SetTop(particle, y);
            
            _particleCanvas.Children.Add(particle);
            _particles.Add(particle);
        }
    }

    private void RenderTimeScale()
    {
        // Implementation would add time labels along the bottom
        // Showing hours, days, weeks depending on zoom level
    }

    private void RenderPriceScale()
    {
        // Implementation would add price labels along the left side
        // Showing price ticks based on min/max range
    }

    #endregion

    #region Animation

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
        // Animate price points appearing
        for (int i = 0; i < _pricePoints.Count; i++)
        {
            var progress = Math.Min(1.0, _animationProgress * 2 - (i / (double)_pricePoints.Count));
            if (progress > 0)
            {
                _pricePoints[i].Opacity = progress;
                var transform = new ScaleTransform(progress, progress);
                _pricePoints[i].RenderTransform = transform;
            }
        }
    }

    private void UpdateParticleEffects()
    {
        foreach (var particle in _particles.ToList())
        {
            var currentX = Canvas.GetLeft(particle);
            var currentY = Canvas.GetTop(particle);
            
            // Move particle along price curve with some randomness
            currentX += 2;
            currentY += (_random.NextDouble() - 0.5) * 2;
            
            // Reset particle if it goes off-screen
            if (currentX > _timelineCanvas.Width + 50)
            {
                currentX = -50;
                currentY = _random.NextDouble() * (ActualHeight - 100) + 50;
            }
            
            Canvas.SetLeft(particle, currentX);
            Canvas.SetTop(particle, currentY);
        }
    }

    #endregion

    #region Utility Methods

    private double TimeToPosition(DateTime time)
    {
        var timeSpan = time - _startTime;
        return timeSpan.TotalHours * _pixelsPerHour + 80; // 80px offset for price scale
    }

    private DateTime PositionToTime(double position)
    {
        var hours = (position - 80) / _pixelsPerHour;
        return _startTime.AddHours(hours);
    }

    private double PriceToPosition(double price)
    {
        var priceRange = _maxPrice - _minPrice;
        var normalized = (price - _minPrice) / priceRange;
        return (1 - normalized) * (ActualHeight - 80) + 20; // 20px top margin, 60px bottom margin
    }

    private double PositionToPrice(double position)
    {
        var normalized = 1 - ((position - 20) / (ActualHeight - 80));
        var priceRange = _maxPrice - _minPrice;
        return _minPrice + (normalized * priceRange);
    }

    private void ClearTimelineElements()
    {
        _priceCanvas.Children.Clear();
        _volumeCanvas.Children.Clear();
        _eventCanvas.Children.Clear();
        _particleCanvas.Children.Clear();
        
        _pricePoints.Clear();
        _trendLines.Clear();
        _volumeIndicators.Clear();
        _eventMarkers.Clear();
        _particles.Clear();
    }

    private void UpdateHoverEffects(Point position)
    {
        // Find nearest price point
        var nearestPoint = FindNearestPricePoint(position);
        
        if (nearestPoint != _hoveredPoint)
        {
            _hoveredPoint = nearestPoint;
            ShowPriceTooltip(position, nearestPoint);
        }
    }

    private HoloPriceHistoryPoint FindNearestPricePoint(Point position)
    {
        var time = PositionToTime(position.X);
        var price = PositionToPrice(position.Y);
        
        return PriceHistory?.OrderBy(p => 
            Math.Sqrt(Math.Pow((p.Timestamp - time).TotalMinutes, 2) + 
                     Math.Pow(p.Price - price, 2)))
            .FirstOrDefault();
    }

    private void ShowPriceTooltip(Point position, HoloPriceHistoryPoint point)
    {
        if (point == null) return;
        
        // Implementation would show a tooltip with price, time, volume information
    }

    private void SelectPointAtPosition(Point position)
    {
        var selectedPoint = FindNearestPricePoint(position);
        if (selectedPoint != null)
        {
            SelectedItem = new HoloTimelineItem
            {
                Timestamp = selectedPoint.Timestamp,
                Price = selectedPoint.Price,
                Volume = selectedPoint.Volume,
                Change = CalculatePriceChange(selectedPoint),
                ChangePercent = CalculatePriceChangePercent(selectedPoint)
            };
        }
    }

    private double CalculatePriceChange(HoloPriceHistoryPoint point)
    {
        var previousPoint = PriceHistory?
            .Where(p => p.Timestamp < point.Timestamp)
            .OrderByDescending(p => p.Timestamp)
            .FirstOrDefault();
            
        return previousPoint != null ? point.Price - previousPoint.Price : 0;
    }

    private double CalculatePriceChangePercent(HoloPriceHistoryPoint point)
    {
        var change = CalculatePriceChange(point);
        var previousPoint = PriceHistory?
            .Where(p => p.Timestamp < point.Timestamp)
            .OrderByDescending(p => p.Timestamp)
            .FirstOrDefault();
            
        return previousPoint != null && previousPoint.Price > 0 
            ? (change / previousPoint.Price) * 100 
            : 0;
    }

    public void ScrollToTime(DateTime time)
    {
        var position = TimeToPosition(time);
        _timelineOffset = ActualWidth / 2 - position;
        UpdateTimelinePosition();
    }

    #endregion

    #region Sample Data

    private void GenerateSampleData()
    {
        PriceHistory = new ObservableCollection<HoloPriceHistoryPoint>();
        var baseTime = DateTime.Now.AddDays(-1);
        var basePrice = 1000000.0; // 1M ISK
        
        for (int i = 0; i < 288; i++) // 24 hours at 5-minute intervals
        {
            var priceVariation = (_random.NextDouble() - 0.5) * 0.1; // ±10% variation
            var volumeVariation = _random.NextDouble() * 0.8 + 0.2; // 20-100% of base volume
            
            var point = new HoloPriceHistoryPoint
            {
                Timestamp = baseTime.AddMinutes(i * 5),
                Price = basePrice * (1 + priceVariation),
                Volume = (long)(100 * volumeVariation),
                ItemId = 34, // Tritanium
                ItemName = "Tritanium",
                Region = "The Forge"
            };
            
            PriceHistory.Add(point);
            
            // Add some trend
            basePrice *= 1 + (_random.NextDouble() - 0.5) * 0.002;
        }
    }

    #endregion

    #region Public Methods

    public TimelineAnalysis GetTimelineAnalysis()
    {
        return new TimelineAnalysis
        {
            TimeRange = TimeRange,
            VisibleDataPoints = GetVisiblePoints().Count,
            TotalDataPoints = PriceHistory?.Count ?? 0,
            PriceRange = new { Min = _minPrice, Max = _maxPrice },
            TimeSpan = _endTime - _startTime,
            ZoomLevel = ZoomLevel,
            ScrollPosition = _timelineOffset,
            IsAnimating = _isAnimating,
            PerformanceMode = _isInSimplifiedMode ? "Simplified" : "Full"
        };
    }

    public void SetTimeRange(TimelineRange range)
    {
        TimeRange = range;
        GenerateSampleData();
        UpdateTimelineData();
    }

    public void ZoomToFit()
    {
        if (PriceHistory?.Any() == true)
        {
            ZoomLevel = 1.0;
            _timelineOffset = 0;
            UpdateTimelineLayout();
        }
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void SetSimplifiedMode(bool enabled)
    {
        _isInSimplifiedMode = enabled;
        EnableParticleEffects = !enabled;
        EnableAnimations = !enabled;
        _maxVisiblePoints = enabled ? 200 : 1000;
        
        if (IsLoaded)
        {
            UpdateTimelineData();
        }
    }

    public bool IsInSimplifiedMode => _isInSimplifiedMode;

    #endregion

    #region Property Change Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPriceHistoryTimeline control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPriceHistoryTimeline control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnPriceHistoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPriceHistoryTimeline control && control.IsLoaded)
        {
            control.UpdateTimelineData();
        }
    }

    private static void OnTimeRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPriceHistoryTimeline control && control.IsLoaded)
        {
            control.GenerateSampleData();
            control.UpdateTimelineData();
        }
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPriceHistoryTimeline control)
        {
            control.UpdateSelectionHighlight();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPriceHistoryTimeline control)
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
        if (d is HoloPriceHistoryTimeline control && control.IsLoaded)
        {
            control.UpdateTimelineData();
        }
    }

    private static void OnShowTrendLinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPriceHistoryTimeline control && control.IsLoaded)
        {
            control.UpdateTimelineData();
        }
    }

    private static void OnShowVolumeIndicatorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPriceHistoryTimeline control && control.IsLoaded)
        {
            control.UpdateTimelineData();
        }
    }

    private static void OnShowEventMarkersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPriceHistoryTimeline control && control.IsLoaded)
        {
            control.UpdateTimelineData();
        }
    }

    private static void OnAutoScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPriceHistoryTimeline control)
        {
            if ((bool)e.NewValue)
            {
                control._scrollTimer?.Start();
            }
            else
            {
                control._scrollTimer?.Stop();
            }
        }
    }

    private static void OnZoomLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPriceHistoryTimeline control && control.IsLoaded)
        {
            control.UpdateTimelineLayout();
        }
    }

    private static void OnUpdateIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloPriceHistoryTimeline control)
        {
            control._updateTimer.Interval = (TimeSpan)e.NewValue;
        }
    }

    private void UpdateHolographicEffects()
    {
        // Update holographic intensity effects on all elements
        Opacity = 0.8 + (0.2 * HolographicIntensity);
    }

    private void UpdateColorScheme()
    {
        // Update color scheme based on EVE colors
        if (IsLoaded)
        {
            UpdateTimelineData();
        }
    }

    private void UpdateSelectionHighlight()
    {
        // Highlight selected timeline item
        if (SelectedItem != null)
        {
            var position = TimeToPosition(SelectedItem.Timestamp);
            // Add visual highlight at position
        }
    }

    #endregion
}

#region Supporting Classes

public class HoloPriceHistoryPoint
{
    public DateTime Timestamp { get; set; }
    public double Price { get; set; }
    public long Volume { get; set; }
    public int ItemId { get; set; }
    public string ItemName { get; set; }
    public string Region { get; set; }
}

public class HoloTimelineItem
{
    public DateTime Timestamp { get; set; }
    public double Price { get; set; }
    public long Volume { get; set; }
    public double Change { get; set; }
    public double ChangePercent { get; set; }
}

public enum TimelineRange
{
    Hour,
    Day,
    Week,
    Month
}

public class TimelineAnalysis
{
    public TimelineRange TimeRange { get; set; }
    public int VisibleDataPoints { get; set; }
    public int TotalDataPoints { get; set; }
    public object PriceRange { get; set; }
    public TimeSpan TimeSpan { get; set; }
    public double ZoomLevel { get; set; }
    public double ScrollPosition { get; set; }
    public bool IsAnimating { get; set; }
    public string PerformanceMode { get; set; }
}

#endregion