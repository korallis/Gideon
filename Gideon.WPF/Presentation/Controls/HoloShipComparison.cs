// ==========================================================================
// HoloShipComparison.cs - Holographic Ship Comparison Interface
// ==========================================================================
// Advanced ship comparison system featuring side-by-side holographic displays,
// animated difference visualization, performance metrics, and EVE-style analytics.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
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
/// Holographic ship comparison interface with side-by-side analysis and animated metrics
/// </summary>
public class HoloShipComparison : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloShipComparison),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloShipComparison),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty LeftShipProperty =
        DependencyProperty.Register(nameof(LeftShip), typeof(HoloShipData), typeof(HoloShipComparison),
            new PropertyMetadata(null, OnLeftShipChanged));

    public static readonly DependencyProperty RightShipProperty =
        DependencyProperty.Register(nameof(RightShip), typeof(HoloShipData), typeof(HoloShipComparison),
            new PropertyMetadata(null, OnRightShipChanged));

    public static readonly DependencyProperty ComparisonModeProperty =
        DependencyProperty.Register(nameof(ComparisonMode), typeof(ComparisonMode), typeof(HoloShipComparison),
            new PropertyMetadata(ComparisonMode.SideBySide, OnComparisonModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloShipComparison),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloShipComparison),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowDifferenceHighlightsProperty =
        DependencyProperty.Register(nameof(ShowDifferenceHighlights), typeof(bool), typeof(HoloShipComparison),
            new PropertyMetadata(true, OnShowDifferenceHighlightsChanged));

    public static readonly DependencyProperty ShowPerformanceMetricsProperty =
        DependencyProperty.Register(nameof(ShowPerformanceMetrics), typeof(bool), typeof(HoloShipComparison),
            new PropertyMetadata(true, OnShowPerformanceMetricsChanged));

    public static readonly DependencyProperty ComparisonCriteriaProperty =
        DependencyProperty.Register(nameof(ComparisonCriteria), typeof(List<string>), typeof(HoloShipComparison),
            new PropertyMetadata(null, OnComparisonCriteriaChanged));

    public static readonly DependencyProperty AutoSyncRotationProperty =
        DependencyProperty.Register(nameof(AutoSyncRotation), typeof(bool), typeof(HoloShipComparison),
            new PropertyMetadata(true, OnAutoSyncRotationChanged));

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

    public HoloShipData LeftShip
    {
        get => (HoloShipData)GetValue(LeftShipProperty);
        set => SetValue(LeftShipProperty, value);
    }

    public HoloShipData RightShip
    {
        get => (HoloShipData)GetValue(RightShipProperty);
        set => SetValue(RightShipProperty, value);
    }

    public ComparisonMode ComparisonMode
    {
        get => (ComparisonMode)GetValue(ComparisonModeProperty);
        set => SetValue(ComparisonModeProperty, value);
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

    public bool ShowDifferenceHighlights
    {
        get => (bool)GetValue(ShowDifferenceHighlightsProperty);
        set => SetValue(ShowDifferenceHighlightsProperty, value);
    }

    public bool ShowPerformanceMetrics
    {
        get => (bool)GetValue(ShowPerformanceMetricsProperty);
        set => SetValue(ShowPerformanceMetricsProperty, value);
    }

    public List<string> ComparisonCriteria
    {
        get => (List<string>)GetValue(ComparisonCriteriaProperty);
        set => SetValue(ComparisonCriteriaProperty, value);
    }

    public bool AutoSyncRotation
    {
        get => (bool)GetValue(AutoSyncRotationProperty);
        set => SetValue(AutoSyncRotationProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<ComparisonEventArgs> ComparisonUpdated;
    public event EventHandler<ComparisonEventArgs> ShipSelected;
    public event EventHandler<ComparisonEventArgs> MetricHighlighted;
    public event EventHandler<ComparisonEventArgs> ComparisonExported;

    #endregion

    #region Private Fields

    private readonly DispatcherTimer _animationTimer;
    private readonly DispatcherTimer _syncTimer;
    private Grid _mainGrid;
    private HoloShipProjection _leftProjection;
    private HoloShipProjection _rightProjection;
    private Canvas _comparisonOverlay;
    private Canvas _metricsPanel;
    private Canvas _particleCanvas;
    private Border _dividerLine;
    private readonly Dictionary<string, FrameworkElement> _metricElements = new();
    private readonly List<UIElement> _differenceIndicators = new();
    private readonly List<UIElement> _particleEffects = new();
    private bool _isSimplifiedMode;
    private double _animationPhase;
    private ComparisonData _currentComparison;
    private readonly Random _random = new();

    #endregion

    #region Constructor

    public HoloShipComparison()
    {
        InitializeComponent();
        InitializeTimers();
        InitializeDefaultCriteria();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 1200;
        Height = 800;
        Background = new SolidColorBrush(Colors.Transparent);
        
        CreateMainLayout();
        ApplyEVEColorScheme();
    }

    private void CreateMainLayout()
    {
        _mainGrid = new Grid();
        Content = _mainGrid;

        // Define columns for side-by-side layout
        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) }); // Divider
        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Define rows
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) }); // Ship projections
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Metrics panel

        CreateShipProjections();
        CreateDividerLine();
        CreateMetricsPanel();
        CreateComparisonOverlay();
        CreateParticleCanvas();
    }

    private void CreateShipProjections()
    {
        // Left ship projection
        _leftProjection = new HoloShipProjection
        {
            EVEColorScheme = EVEColorScheme,
            HolographicIntensity = HolographicIntensity,
            EnableInteraction = true,
            ProjectionMode = ShipProjectionMode.Holographic
        };
        Grid.SetColumn(_leftProjection, 0);
        Grid.SetRow(_leftProjection, 0);
        _mainGrid.Children.Add(_leftProjection);

        // Right ship projection
        _rightProjection = new HoloShipProjection
        {
            EVEColorScheme = EVEColorScheme,
            HolographicIntensity = HolographicIntensity,
            EnableInteraction = true,
            ProjectionMode = ShipProjectionMode.Holographic
        };
        Grid.SetColumn(_rightProjection, 2);
        Grid.SetRow(_rightProjection, 0);
        _mainGrid.Children.Add(_rightProjection);

        // Sync rotation events if enabled
        _leftProjection.ShipRotated += OnLeftShipRotated;
        _rightProjection.ShipRotated += OnRightShipRotated;
    }

    private void CreateDividerLine()
    {
        _dividerLine = new Border
        {
            Width = 2,
            Background = CreateDividerBrush(),
            VerticalAlignment = VerticalAlignment.Stretch,
            Effect = new DropShadowEffect
            {
                Color = GetPrimaryColor(),
                BlurRadius = 10,
                ShadowDepth = 0,
                Opacity = 0.8
            }
        };
        Grid.SetColumn(_dividerLine, 1);
        Grid.SetRowSpan(_dividerLine, 2);
        _mainGrid.Children.Add(_dividerLine);

        if (EnableAnimations && !_isSimplifiedMode)
        {
            AnimateDividerLine();
        }
    }

    private void CreateMetricsPanel()
    {
        _metricsPanel = new Canvas
        {
            Background = CreateMetricsPanelBrush()
        };
        Grid.SetColumnSpan(_metricsPanel, 3);
        Grid.SetRow(_metricsPanel, 1);
        _mainGrid.Children.Add(_metricsPanel);
    }

    private void CreateComparisonOverlay()
    {
        _comparisonOverlay = new Canvas
        {
            IsHitTestVisible = false
        };
        Grid.SetColumnSpan(_comparisonOverlay, 3);
        Grid.SetRowSpan(_comparisonOverlay, 2);
        _mainGrid.Children.Add(_comparisonOverlay);
    }

    private void CreateParticleCanvas()
    {
        _particleCanvas = new Canvas
        {
            IsHitTestVisible = false
        };
        Grid.SetColumnSpan(_particleCanvas, 3);
        Grid.SetRowSpan(_particleCanvas, 2);
        _mainGrid.Children.Add(_particleCanvas);
    }

    private void InitializeTimers()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
        };
        _animationTimer.Tick += OnAnimationTimerTick;

        _syncTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100) // 10 Hz for sync updates
        };
        _syncTimer.Tick += OnSyncTimerTick;
    }

    private void InitializeDefaultCriteria()
    {
        ComparisonCriteria = new List<string>
        {
            "DPS", "Tank", "Speed", "Range", "Capacitor", "Cost", "Fittings", "Alpha"
        };
    }

    #endregion

    #region Public Methods

    public void SetShips(HoloShipData leftShip, HoloShipData rightShip)
    {
        LeftShip = leftShip;
        RightShip = rightShip;
        
        UpdateShipProjections();
        UpdateComparison();
    }

    public void SwapShips()
    {
        var temp = LeftShip;
        LeftShip = RightShip;
        RightShip = temp;
        
        UpdateShipProjections();
        UpdateComparison();

        if (EnableParticleEffects && !_isSimplifiedMode)
        {
            CreateSwapEffect();
        }
    }

    public void HighlightDifferences(string metric)
    {
        ClearDifferenceHighlights();
        
        if (!ShowDifferenceHighlights || _currentComparison == null) return;

        var difference = _currentComparison.GetMetricDifference(metric);
        if (Math.Abs(difference) > 0.01)
        {
            CreateDifferenceVisualization(metric, difference);
        }

        MetricHighlighted?.Invoke(this, new ComparisonEventArgs
        {
            LeftShip = LeftShip,
            RightShip = RightShip,
            Metric = metric,
            Timestamp = DateTime.Now
        });
    }

    public void ResetView()
    {
        _leftProjection?.ResetShipOrientation();
        _rightProjection?.ResetShipOrientation();
        
        ClearDifferenceHighlights();
        _comparisonOverlay.Children.Clear();
    }

    public ComparisonData ExportComparison()
    {
        var exportData = _currentComparison?.Clone() ?? new ComparisonData();
        
        ComparisonExported?.Invoke(this, new ComparisonEventArgs
        {
            LeftShip = LeftShip,
            RightShip = RightShip,
            ComparisonData = exportData,
            Timestamp = DateTime.Now
        });

        return exportData;
    }

    public void SetComparisonMode(ComparisonMode mode)
    {
        ComparisonMode = mode;
        UpdateLayoutForMode();
    }

    public void FocusOnMetric(string metric)
    {
        if (_metricElements.TryGetValue(metric, out var element))
        {
            CreateFocusAnimation(element);
            HighlightDifferences(metric);
        }
    }

    #endregion

    #region Private Methods

    private void UpdateShipProjections()
    {
        if (_leftProjection != null && LeftShip != null)
        {
            _ = _leftProjection.LoadShipAsync(LeftShip);
        }

        if (_rightProjection != null && RightShip != null)
        {
            _ = _rightProjection.LoadShipAsync(RightShip);
        }
    }

    private void UpdateComparison()
    {
        if (LeftShip == null || RightShip == null) return;

        _currentComparison = new ComparisonData(LeftShip, RightShip);
        UpdateMetricsDisplay();
        UpdateDifferenceIndicators();

        ComparisonUpdated?.Invoke(this, new ComparisonEventArgs
        {
            LeftShip = LeftShip,
            RightShip = RightShip,
            ComparisonData = _currentComparison,
            Timestamp = DateTime.Now
        });
    }

    private void UpdateMetricsDisplay()
    {
        if (!ShowPerformanceMetrics || _currentComparison == null) return;

        _metricsPanel.Children.Clear();
        _metricElements.Clear();

        var y = 20.0;
        var columnWidth = ActualWidth / 3;

        // Header
        CreateMetricHeader("SHIP COMPARISON", ref y);

        // Ship names
        CreateShipNamesDisplay(ref y);

        // Metrics comparison
        foreach (var criterion in ComparisonCriteria ?? new List<string>())
        {
            CreateMetricComparison(criterion, ref y, columnWidth);
        }

        // Overall assessment
        CreateOverallAssessment(ref y);
    }

    private void CreateMetricHeader(string title, ref double y)
    {
        var header = new TextBlock
        {
            Text = title,
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = GetPrimaryBrush(),
            HorizontalAlignment = HorizontalAlignment.Center,
            Effect = CreateTextGlow()
        };

        Canvas.SetLeft(header, ActualWidth / 2 - 80);
        Canvas.SetTop(header, y);
        _metricsPanel.Children.Add(header);

        y += 30;
    }

    private void CreateShipNamesDisplay(ref double y)
    {
        var leftNameContainer = new Grid
        {
            Width = 200,
            Height = 40
        };

        var leftName = new TextBlock
        {
            Text = LeftShip?.ShipName ?? "Ship A",
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(100, 150, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        leftNameContainer.Children.Add(leftName);

        var leftBorder = new Border
        {
            Child = leftNameContainer,
            BorderBrush = new SolidColorBrush(Color.FromRgb(100, 150, 255)),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(5)
        };

        Canvas.SetLeft(leftBorder, 50);
        Canvas.SetTop(leftBorder, y);
        _metricsPanel.Children.Add(leftBorder);

        // VS indicator
        var vsLabel = new TextBlock
        {
            Text = "VS",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = GetSecondaryBrush(),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        Canvas.SetLeft(vsLabel, ActualWidth / 2 - 10);
        Canvas.SetTop(vsLabel, y + 15);
        _metricsPanel.Children.Add(vsLabel);

        var rightNameContainer = new Grid
        {
            Width = 200,
            Height = 40
        };

        var rightName = new TextBlock
        {
            Text = RightShip?.ShipName ?? "Ship B",
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(255, 150, 100)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        rightNameContainer.Children.Add(rightName);

        var rightBorder = new Border
        {
            Child = rightNameContainer,
            BorderBrush = new SolidColorBrush(Color.FromRgb(255, 150, 100)),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(5)
        };

        Canvas.SetLeft(rightBorder, ActualWidth - 250);
        Canvas.SetTop(rightBorder, y);
        _metricsPanel.Children.Add(rightBorder);

        y += 60;
    }

    private void CreateMetricComparison(string metric, ref double y, double columnWidth)
    {
        var leftValue = _currentComparison.GetLeftValue(metric);
        var rightValue = _currentComparison.GetRightValue(metric);
        var difference = _currentComparison.GetMetricDifference(metric);

        // Metric name
        var nameLabel = new TextBlock
        {
            Text = metric.ToUpper(),
            FontSize = 12,
            FontWeight = FontWeights.Medium,
            Foreground = GetTextBrush(),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        Canvas.SetLeft(nameLabel, ActualWidth / 2 - 30);
        Canvas.SetTop(nameLabel, y);
        _metricsPanel.Children.Add(nameLabel);

        // Left value
        var leftValueLabel = new TextBlock
        {
            Text = FormatMetricValue(leftValue, metric),
            FontSize = 11,
            Foreground = difference < 0 ? GetWinnerBrush() : GetLoserBrush(),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        Canvas.SetLeft(leftValueLabel, 100);
        Canvas.SetTop(leftValueLabel, y);
        _metricsPanel.Children.Add(leftValueLabel);

        // Right value
        var rightValueLabel = new TextBlock
        {
            Text = FormatMetricValue(rightValue, metric),
            FontSize = 11,
            Foreground = difference > 0 ? GetWinnerBrush() : GetLoserBrush(),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        Canvas.SetLeft(rightValueLabel, ActualWidth - 150);
        Canvas.SetTop(rightValueLabel, y);
        _metricsPanel.Children.Add(rightValueLabel);

        // Difference indicator
        if (Math.Abs(difference) > 0.01)
        {
            CreateDifferenceIndicator(metric, difference, y);
        }

        // Store for later reference
        var container = new StackPanel { Orientation = Orientation.Horizontal };
        container.Children.Add(leftValueLabel);
        container.Children.Add(nameLabel);
        container.Children.Add(rightValueLabel);
        _metricElements[metric] = container;

        y += 25;
    }

    private void CreateDifferenceIndicator(string metric, double difference, double y)
    {
        var isLeftBetter = difference < 0;
        var improvementPercentage = Math.Abs(difference);

        // Arrow indicator
        var arrow = new Polygon
        {
            Points = isLeftBetter ? CreateLeftArrowPoints() : CreateRightArrowPoints(),
            Fill = GetDifferenceBrush(improvementPercentage),
            Width = 15,
            Height = 10
        };

        Canvas.SetLeft(arrow, ActualWidth / 2 + (isLeftBetter ? -80 : 65));
        Canvas.SetTop(arrow, y + 2);
        _metricsPanel.Children.Add(arrow);

        // Percentage label
        var percentLabel = new TextBlock
        {
            Text = $"{improvementPercentage:P0}",
            FontSize = 9,
            Foreground = GetDifferenceBrush(improvementPercentage),
            FontWeight = FontWeights.Bold
        };

        Canvas.SetLeft(percentLabel, Canvas.GetLeft(arrow) + (isLeftBetter ? -25 : 20));
        Canvas.SetTop(percentLabel, y);
        _metricsPanel.Children.Add(percentLabel);

        if (EnableAnimations && !_isSimplifiedMode)
        {
            var pulseAnimation = new DoubleAnimation
            {
                From = 0.7,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(1),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };
            arrow.BeginAnimation(OpacityProperty, pulseAnimation);
        }
    }

    private void CreateOverallAssessment(ref double y)
    {
        y += 20;

        var assessment = CalculateOverallAssessment();
        var assessmentText = new TextBlock
        {
            Text = $"OVERALL: {assessment.Winner} ({assessment.Advantage:P0} advantage)",
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = assessment.Winner == "LEFT" ? 
                new SolidColorBrush(Color.FromRgb(100, 150, 255)) : 
                new SolidColorBrush(Color.FromRgb(255, 150, 100)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Effect = CreateTextGlow()
        };

        Canvas.SetLeft(assessmentText, ActualWidth / 2 - 100);
        Canvas.SetTop(assessmentText, y);
        _metricsPanel.Children.Add(assessmentText);

        y += 30;
    }

    private void UpdateDifferenceIndicators()
    {
        ClearDifferenceHighlights();

        if (!ShowDifferenceHighlights || _currentComparison == null) return;

        foreach (var criterion in ComparisonCriteria ?? new List<string>())
        {
            var difference = _currentComparison.GetMetricDifference(criterion);
            if (Math.Abs(difference) > 0.1) // Only show significant differences
            {
                CreateVisualDifferenceIndicator(criterion, difference);
            }
        }
    }

    private void CreateVisualDifferenceIndicator(string metric, double difference)
    {
        var isLeftBetter = difference < 0;
        var targetProjection = isLeftBetter ? _leftProjection : _rightProjection;
        
        if (targetProjection == null) return;

        // Create glowing outline effect
        var glowEffect = new Border
        {
            BorderBrush = GetDifferenceBrush(Math.Abs(difference)),
            BorderThickness = new Thickness(3),
            CornerRadius = new CornerRadius(5),
            Background = Brushes.Transparent,
            Effect = new DropShadowEffect
            {
                Color = isLeftBetter ? Color.FromRgb(100, 255, 100) : Color.FromRgb(100, 255, 100),
                BlurRadius = 15,
                ShadowDepth = 0,
                Opacity = 0.8
            }
        };

        // Position the effect around the better performing ship
        Grid.SetColumn(glowEffect, isLeftBetter ? 0 : 2);
        Grid.SetRow(glowEffect, 0);
        _mainGrid.Children.Add(glowEffect);
        _differenceIndicators.Add(glowEffect);

        if (EnableAnimations && !_isSimplifiedMode)
        {
            var pulseAnimation = new DoubleAnimation
            {
                From = 0.3,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(2),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };
            glowEffect.BeginAnimation(OpacityProperty, pulseAnimation);
        }
    }

    private void CreateDifferenceVisualization(string metric, double difference)
    {
        var isLeftBetter = difference < 0;
        var connectionLine = new Line
        {
            X1 = isLeftBetter ? ActualWidth * 0.45 : ActualWidth * 0.55,
            Y1 = ActualHeight * 0.3,
            X2 = isLeftBetter ? ActualWidth * 0.55 : ActualWidth * 0.45,
            Y2 = ActualHeight * 0.3,
            Stroke = GetDifferenceBrush(Math.Abs(difference)),
            StrokeThickness = 4,
            StrokeDashArray = new DoubleCollection { 10, 5 },
            Opacity = HolographicIntensity
        };

        _comparisonOverlay.Children.Add(connectionLine);
        _differenceIndicators.Add(connectionLine);

        // Animate the connection line
        if (EnableAnimations && !_isSimplifiedMode)
        {
            var dashAnimation = new DoubleAnimation
            {
                From = 0,
                To = 15,
                Duration = TimeSpan.FromSeconds(2),
                RepeatBehavior = RepeatBehavior.Forever
            };
            connectionLine.BeginAnimation(Line.StrokeDashOffsetProperty, dashAnimation);
        }

        // Add metric label
        var label = new TextBlock
        {
            Text = $"{metric}\n{Math.Abs(difference):P0} {(isLeftBetter ? "←" : "→")}",
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = GetDifferenceBrush(Math.Abs(difference)),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            Effect = CreateTextGlow()
        };

        Canvas.SetLeft(label, ActualWidth / 2 - 30);
        Canvas.SetTop(label, ActualHeight * 0.25);
        _comparisonOverlay.Children.Add(label);
        _differenceIndicators.Add(label);
    }

    private void UpdateLayoutForMode()
    {
        switch (ComparisonMode)
        {
            case ComparisonMode.SideBySide:
                UpdateSideBySideLayout();
                break;
            case ComparisonMode.Overlay:
                UpdateOverlayLayout();
                break;
            case ComparisonMode.Split:
                UpdateSplitLayout();
                break;
        }
    }

    private void UpdateSideBySideLayout()
    {
        // Already implemented as default layout
        _leftProjection.Opacity = HolographicIntensity;
        _rightProjection.Opacity = HolographicIntensity;
        _dividerLine.Visibility = Visibility.Visible;
    }

    private void UpdateOverlayLayout()
    {
        // Overlay ships with transparency
        _leftProjection.Opacity = HolographicIntensity * 0.7;
        _rightProjection.Opacity = HolographicIntensity * 0.7;
        _dividerLine.Visibility = Visibility.Collapsed;

        // Center both projections
        Grid.SetColumn(_leftProjection, 0);
        Grid.SetColumnSpan(_leftProjection, 3);
        Grid.SetColumn(_rightProjection, 0);
        Grid.SetColumnSpan(_rightProjection, 3);
    }

    private void UpdateSplitLayout()
    {
        // Implement vertical split layout
        _mainGrid.RowDefinitions.Clear();
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) }); // Horizontal divider
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(200) }); // Metrics

        Grid.SetRow(_leftProjection, 0);
        Grid.SetColumnSpan(_leftProjection, 3);
        Grid.SetRow(_rightProjection, 2);
        Grid.SetColumnSpan(_rightProjection, 3);
        Grid.SetRow(_metricsPanel, 3);
    }

    #endregion

    #region Animation Methods

    private void AnimateDividerLine()
    {
        var gradientBrush = _dividerLine.Background as LinearGradientBrush;
        if (gradientBrush == null) return;

        var animation = new PointAnimation
        {
            From = new Point(0, 0),
            To = new Point(0, 1),
            Duration = TimeSpan.FromSeconds(3),
            RepeatBehavior = RepeatBehavior.Forever,
            AutoReverse = true
        };

        gradientBrush.BeginAnimation(LinearGradientBrush.StartPointProperty, animation);
    }

    private void CreateSwapEffect()
    {
        // Create particle burst effect
        var burstCenter = new Point(ActualWidth / 2, ActualHeight / 2);
        
        for (int i = 0; i < 20; i++)
        {
            var particle = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = GetPrimaryBrush(),
                Opacity = HolographicIntensity
            };

            Canvas.SetLeft(particle, burstCenter.X);
            Canvas.SetTop(particle, burstCenter.Y);
            _particleCanvas.Children.Add(particle);

            AnimateSwapParticle(particle, burstCenter, i);
        }
    }

    private void AnimateSwapParticle(UIElement particle, Point center, int index)
    {
        var angle = (Math.PI * 2 / 20) * index;
        var distance = 100;
        var targetX = center.X + Math.Cos(angle) * distance;
        var targetY = center.Y + Math.Sin(angle) * distance;

        var moveXAnimation = new DoubleAnimation
        {
            From = center.X,
            To = targetX,
            Duration = TimeSpan.FromMilliseconds(800),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var moveYAnimation = new DoubleAnimation
        {
            From = center.Y,
            To = targetY,
            Duration = TimeSpan.FromMilliseconds(800),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = HolographicIntensity,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(800)
        };

        moveXAnimation.Completed += (s, e) => _particleCanvas.Children.Remove(particle);

        particle.BeginAnimation(Canvas.LeftProperty, moveXAnimation);
        particle.BeginAnimation(Canvas.TopProperty, moveYAnimation);
        particle.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void CreateFocusAnimation(FrameworkElement element)
    {
        var scaleTransform = new ScaleTransform(1, 1);
        element.RenderTransform = scaleTransform;
        element.RenderTransformOrigin = new Point(0.5, 0.5);

        var scaleAnimation = new DoubleAnimation
        {
            From = 1,
            To = 1.2,
            Duration = TimeSpan.FromMilliseconds(300),
            AutoReverse = true,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
    }

    #endregion

    #region Helper Methods

    private OverallAssessment CalculateOverallAssessment()
    {
        if (_currentComparison == null) 
            return new OverallAssessment { Winner = "TIED", Advantage = 0 };

        var leftWins = 0;
        var rightWins = 0;
        var totalAdvantage = 0.0;

        foreach (var criterion in ComparisonCriteria ?? new List<string>())
        {
            var difference = _currentComparison.GetMetricDifference(criterion);
            if (Math.Abs(difference) > 0.05) // 5% threshold
            {
                if (difference < 0)
                {
                    leftWins++;
                    totalAdvantage += Math.Abs(difference);
                }
                else
                {
                    rightWins++;
                    totalAdvantage += Math.Abs(difference);
                }
            }
        }

        if (leftWins == rightWins)
            return new OverallAssessment { Winner = "TIED", Advantage = 0 };

        var winner = leftWins > rightWins ? "LEFT" : "RIGHT";
        var advantage = totalAdvantage / ComparisonCriteria.Count;

        return new OverallAssessment { Winner = winner, Advantage = advantage };
    }

    private PointCollection CreateLeftArrowPoints()
    {
        return new PointCollection
        {
            new Point(15, 5),
            new Point(5, 5),
            new Point(0, 0),
            new Point(5, -5),
            new Point(15, -5)
        };
    }

    private PointCollection CreateRightArrowPoints()
    {
        return new PointCollection
        {
            new Point(0, 5),
            new Point(10, 5),
            new Point(15, 0),
            new Point(10, -5),
            new Point(0, -5)
        };
    }

    private string FormatMetricValue(double value, string metric)
    {
        return metric.ToLower() switch
        {
            "dps" => $"{value:F0}",
            "range" => $"{value / 1000:F1}km",
            "speed" => $"{value:F0}m/s",
            "capacitor" => $"{value:F0}%",
            "cost" => $"{value / 1000000:F1}M ISK",
            _ => $"{value:F1}"
        };
    }

    private Brush CreateDividerBrush()
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        return new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(0, 1),
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Color.FromArgb(0, colors.Primary.R, colors.Primary.G, colors.Primary.B), 0),
                new GradientStop(colors.Primary, 0.5),
                new GradientStop(Color.FromArgb(0, colors.Primary.R, colors.Primary.G, colors.Primary.B), 1)
            }
        };
    }

    private Brush CreateMetricsPanelBrush()
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        return new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 1),
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Color.FromArgb(20, colors.Primary.R, colors.Primary.G, colors.Primary.B), 0),
                new GradientStop(Color.FromArgb(40, colors.Secondary.R, colors.Secondary.G, colors.Secondary.B), 1)
            }
        };
    }

    private Brush GetPrimaryBrush()
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        return new SolidColorBrush(colors.Primary);
    }

    private Brush GetSecondaryBrush()
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        return new SolidColorBrush(colors.Secondary);
    }

    private Brush GetTextBrush()
    {
        return new SolidColorBrush(Color.FromArgb(200, 255, 255, 255));
    }

    private Brush GetWinnerBrush()
    {
        return new SolidColorBrush(Color.FromRgb(100, 255, 100));
    }

    private Brush GetLoserBrush()
    {
        return new SolidColorBrush(Color.FromArgb(150, 255, 255, 255));
    }

    private Brush GetDifferenceBrush(double magnitude)
    {
        if (magnitude > 0.5)
            return new SolidColorBrush(Color.FromRgb(255, 100, 100));
        else if (magnitude > 0.25)
            return new SolidColorBrush(Color.FromRgb(255, 200, 100));
        else if (magnitude > 0.1)
            return new SolidColorBrush(Color.FromRgb(255, 255, 100));
        else
            return new SolidColorBrush(Color.FromRgb(100, 255, 100));
    }

    private Color GetPrimaryColor()
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        return colors.Primary;
    }

    private Effect CreateTextGlow()
    {
        return new DropShadowEffect
        {
            Color = GetPrimaryColor(),
            BlurRadius = 8,
            ShadowDepth = 0,
            Opacity = 0.8
        };
    }

    private void ClearDifferenceHighlights()
    {
        foreach (var indicator in _differenceIndicators)
        {
            if (_mainGrid.Children.Contains(indicator))
                _mainGrid.Children.Remove(indicator);
            else if (_comparisonOverlay.Children.Contains(indicator))
                _comparisonOverlay.Children.Remove(indicator);
        }
        _differenceIndicators.Clear();
    }

    private void ClearParticleEffects()
    {
        _particleCanvas.Children.Clear();
        _particleEffects.Clear();
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableAnimations && !_isSimplifiedMode)
        {
            _animationTimer.Start();
        }

        if (AutoSyncRotation)
        {
            _syncTimer.Start();
        }

        UpdateShipProjections();
        UpdateComparison();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer.Stop();
        _syncTimer.Stop();
        ClearDifferenceHighlights();
        ClearParticleEffects();
    }

    private void OnAnimationTimerTick(object sender, EventArgs e)
    {
        if (!EnableAnimations || _isSimplifiedMode) return;

        _animationPhase += 0.1;
        if (_animationPhase > Math.PI * 2)
            _animationPhase = 0;

        // Update any phase-based animations
    }

    private void OnSyncTimerTick(object sender, EventArgs e)
    {
        if (!AutoSyncRotation || _leftProjection == null || _rightProjection == null) return;

        // Sync rotation and zoom between projections
        // Implementation would sync the camera positions
    }

    private void OnLeftShipRotated(object sender, HoloShipEventArgs e)
    {
        if (AutoSyncRotation && _rightProjection != null)
        {
            // Sync rotation to right projection
            // Implementation would copy rotation parameters
        }
    }

    private void OnRightShipRotated(object sender, HoloShipEventArgs e)
    {
        if (AutoSyncRotation && _leftProjection != null)
        {
            // Sync rotation to left projection
            // Implementation would copy rotation parameters
        }
    }

    #endregion

    #region Property Change Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipComparison comparison)
        {
            comparison._leftProjection.HolographicIntensity = (double)e.NewValue;
            comparison._rightProjection.HolographicIntensity = (double)e.NewValue;
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipComparison comparison)
        {
            comparison.ApplyEVEColorScheme();
        }
    }

    private static void OnLeftShipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipComparison comparison)
        {
            comparison.UpdateShipProjections();
            comparison.UpdateComparison();
        }
    }

    private static void OnRightShipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipComparison comparison)
        {
            comparison.UpdateShipProjections();
            comparison.UpdateComparison();
        }
    }

    private static void OnComparisonModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipComparison comparison)
        {
            comparison.UpdateLayoutForMode();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipComparison comparison)
        {
            if ((bool)e.NewValue && !comparison._isSimplifiedMode)
            {
                comparison._animationTimer.Start();
            }
            else
            {
                comparison._animationTimer.Stop();
            }
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipComparison comparison && !(bool)e.NewValue)
        {
            comparison.ClearParticleEffects();
        }
    }

    private static void OnShowDifferenceHighlightsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipComparison comparison)
        {
            comparison.UpdateDifferenceIndicators();
        }
    }

    private static void OnShowPerformanceMetricsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipComparison comparison)
        {
            comparison.UpdateMetricsDisplay();
        }
    }

    private static void OnComparisonCriteriaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipComparison comparison)
        {
            comparison.UpdateComparison();
        }
    }

    private static void OnAutoSyncRotationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipComparison comparison)
        {
            if ((bool)e.NewValue)
            {
                comparison._syncTimer.Start();
            }
            else
            {
                comparison._syncTimer.Stop();
            }
        }
    }

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode => _isSimplifiedMode;

    public void EnterSimplifiedMode()
    {
        _isSimplifiedMode = true;
        EnableAnimations = false;
        EnableParticleEffects = false;
        _animationTimer.Stop();
        ClearParticleEffects();
        
        _leftProjection?.EnterSimplifiedMode();
        _rightProjection?.EnterSimplifiedMode();
    }

    public void ExitSimplifiedMode()
    {
        _isSimplifiedMode = false;
        EnableAnimations = true;
        EnableParticleEffects = true;
        _animationTimer.Start();
        
        _leftProjection?.ExitSimplifiedMode();
        _rightProjection?.ExitSimplifiedMode();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => true;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity;
        
        if (!settings.EnabledFeatures.HasFlag(AnimationFeatures.ParticleEffects))
        {
            EnableParticleEffects = false;
        }
        
        if (!settings.EnabledFeatures.HasFlag(AnimationFeatures.UIAnimations))
        {
            EnableAnimations = false;
        }

        if (settings.PerformanceMode == PerformanceMode.PowerSaver)
        {
            EnterSimplifiedMode();
        }
        else
        {
            ExitSimplifiedMode();
        }

        _leftProjection?.ApplyIntensitySettings(settings);
        _rightProjection?.ApplyIntensitySettings(settings);
    }

    #endregion

    #region Color Scheme Application

    private void ApplyEVEColorScheme()
    {
        if (_leftProjection != null)
            _leftProjection.EVEColorScheme = EVEColorScheme;
        
        if (_rightProjection != null)
            _rightProjection.EVEColorScheme = EVEColorScheme;

        if (_dividerLine != null)
            _dividerLine.Background = CreateDividerBrush();

        if (_metricsPanel != null)
            _metricsPanel.Background = CreateMetricsPanelBrush();

        UpdateComparison();
    }

    #endregion
}

#region Supporting Classes and Enums

public enum ComparisonMode
{
    SideBySide,
    Overlay,
    Split
}

public class ComparisonData
{
    public HoloShipData LeftShip { get; set; }
    public HoloShipData RightShip { get; set; }
    public Dictionary<string, double> LeftMetrics { get; set; } = new();
    public Dictionary<string, double> RightMetrics { get; set; } = new();
    public DateTime ComparisonTimestamp { get; set; } = DateTime.Now;

    public ComparisonData() { }

    public ComparisonData(HoloShipData leftShip, HoloShipData rightShip)
    {
        LeftShip = leftShip;
        RightShip = rightShip;
        CalculateMetrics();
    }

    private void CalculateMetrics()
    {
        // Mock calculations - in real implementation this would use actual ship stats
        LeftMetrics["DPS"] = CalculateDPS(LeftShip);
        LeftMetrics["Tank"] = CalculateTank(LeftShip);
        LeftMetrics["Speed"] = CalculateSpeed(LeftShip);
        LeftMetrics["Range"] = CalculateRange(LeftShip);
        LeftMetrics["Capacitor"] = CalculateCapacitor(LeftShip);
        LeftMetrics["Cost"] = CalculateCost(LeftShip);

        RightMetrics["DPS"] = CalculateDPS(RightShip);
        RightMetrics["Tank"] = CalculateTank(RightShip);
        RightMetrics["Speed"] = CalculateSpeed(RightShip);
        RightMetrics["Range"] = CalculateRange(RightShip);
        RightMetrics["Capacitor"] = CalculateCapacitor(RightShip);
        RightMetrics["Cost"] = CalculateCost(RightShip);
    }

    public double GetLeftValue(string metric) => LeftMetrics.TryGetValue(metric, out var value) ? value : 0;
    public double GetRightValue(string metric) => RightMetrics.TryGetValue(metric, out var value) ? value : 0;
    
    public double GetMetricDifference(string metric)
    {
        var leftValue = GetLeftValue(metric);
        var rightValue = GetRightValue(metric);
        if (rightValue == 0) return 0;
        return (leftValue - rightValue) / rightValue;
    }

    private double CalculateDPS(HoloShipData ship) => ship?.ShipStats.TryGetValue("DPS", out var dps) == true ? (double)dps : 500 + new Random().Next(200);
    private double CalculateTank(HoloShipData ship) => ship?.ShipStats.TryGetValue("Tank", out var tank) == true ? (double)tank : 15000 + new Random().Next(5000);
    private double CalculateSpeed(HoloShipData ship) => ship?.ShipStats.TryGetValue("Speed", out var speed) == true ? (double)speed : 200 + new Random().Next(100);
    private double CalculateRange(HoloShipData ship) => ship?.ShipStats.TryGetValue("Range", out var range) == true ? (double)range : 25000 + new Random().Next(10000);
    private double CalculateCapacitor(HoloShipData ship) => ship?.ShipStats.TryGetValue("Capacitor", out var cap) == true ? (double)cap : 80 + new Random().Next(20);
    private double CalculateCost(HoloShipData ship) => ship?.ShipStats.TryGetValue("Cost", out var cost) == true ? (double)cost : 50000000 + new Random().Next(20000000);

    public ComparisonData Clone()
    {
        return new ComparisonData
        {
            LeftShip = LeftShip,
            RightShip = RightShip,
            LeftMetrics = new Dictionary<string, double>(LeftMetrics),
            RightMetrics = new Dictionary<string, double>(RightMetrics),
            ComparisonTimestamp = ComparisonTimestamp
        };
    }
}

public class OverallAssessment
{
    public string Winner { get; set; }
    public double Advantage { get; set; }
}

public class ComparisonEventArgs : EventArgs
{
    public HoloShipData LeftShip { get; set; }
    public HoloShipData RightShip { get; set; }
    public string Metric { get; set; }
    public ComparisonData ComparisonData { get; set; }
    public DateTime Timestamp { get; set; }
}

#endregion