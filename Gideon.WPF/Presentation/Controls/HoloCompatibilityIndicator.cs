// ==========================================================================
// HoloCompatibilityIndicator.cs - Holographic Module Compatibility System
// ==========================================================================
// Advanced compatibility visualization featuring real-time compatibility analysis,
// animated indicators, constraint validation, and EVE-style compatibility feedback.
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
/// Holographic module compatibility indicator with real-time analysis and animated feedback
/// </summary>
public class HoloCompatibilityIndicator : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloCompatibilityIndicator),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloCompatibilityIndicator),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty TargetModuleProperty =
        DependencyProperty.Register(nameof(TargetModule), typeof(HoloModuleData), typeof(HoloCompatibilityIndicator),
            new PropertyMetadata(null, OnTargetModuleChanged));

    public static readonly DependencyProperty TargetSlotProperty =
        DependencyProperty.Register(nameof(TargetSlot), typeof(HoloFittingSlot), typeof(HoloCompatibilityIndicator),
            new PropertyMetadata(null, OnTargetSlotChanged));

    public static readonly DependencyProperty ShipDataProperty =
        DependencyProperty.Register(nameof(ShipData), typeof(HoloShipData), typeof(HoloCompatibilityIndicator),
            new PropertyMetadata(null, OnShipDataChanged));

    public static readonly DependencyProperty CompatibilityStatusProperty =
        DependencyProperty.Register(nameof(CompatibilityStatus), typeof(CompatibilityStatus), typeof(HoloCompatibilityIndicator),
            new PropertyMetadata(CompatibilityStatus.Unknown, OnCompatibilityStatusChanged));

    public static readonly DependencyProperty ShowDetailedAnalysisProperty =
        DependencyProperty.Register(nameof(ShowDetailedAnalysis), typeof(bool), typeof(HoloCompatibilityIndicator),
            new PropertyMetadata(true, OnShowDetailedAnalysisChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloCompatibilityIndicator),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloCompatibilityIndicator),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowWarningsProperty =
        DependencyProperty.Register(nameof(ShowWarnings), typeof(bool), typeof(HoloCompatibilityIndicator),
            new PropertyMetadata(true, OnShowWarningsChanged));

    public static readonly DependencyProperty AutoUpdateProperty =
        DependencyProperty.Register(nameof(AutoUpdate), typeof(bool), typeof(HoloCompatibilityIndicator),
            new PropertyMetadata(true, OnAutoUpdateChanged));

    public static readonly DependencyProperty AnalysisDepthProperty =
        DependencyProperty.Register(nameof(AnalysisDepth), typeof(AnalysisDepth), typeof(HoloCompatibilityIndicator),
            new PropertyMetadata(AnalysisDepth.Complete, OnAnalysisDepthChanged));

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

    public HoloModuleData TargetModule
    {
        get => (HoloModuleData)GetValue(TargetModuleProperty);
        set => SetValue(TargetModuleProperty, value);
    }

    public HoloFittingSlot TargetSlot
    {
        get => (HoloFittingSlot)GetValue(TargetSlotProperty);
        set => SetValue(TargetSlotProperty, value);
    }

    public HoloShipData ShipData
    {
        get => (HoloShipData)GetValue(ShipDataProperty);
        set => SetValue(ShipDataProperty, value);
    }

    public CompatibilityStatus CompatibilityStatus
    {
        get => (CompatibilityStatus)GetValue(CompatibilityStatusProperty);
        set => SetValue(CompatibilityStatusProperty, value);
    }

    public bool ShowDetailedAnalysis
    {
        get => (bool)GetValue(ShowDetailedAnalysisProperty);
        set => SetValue(ShowDetailedAnalysisProperty, value);
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

    public bool ShowWarnings
    {
        get => (bool)GetValue(ShowWarningsProperty);
        set => SetValue(ShowWarningsProperty, value);
    }

    public bool AutoUpdate
    {
        get => (bool)GetValue(AutoUpdateProperty);
        set => SetValue(AutoUpdateProperty, value);
    }

    public AnalysisDepth AnalysisDepth
    {
        get => (AnalysisDepth)GetValue(AnalysisDepthProperty);
        set => SetValue(AnalysisDepthProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<CompatibilityAnalysisEventArgs> CompatibilityAnalyzed;
    public event EventHandler<CompatibilityWarningEventArgs> CompatibilityWarning;

    #endregion

    #region Private Fields

    private Grid _rootGrid;
    private Border _indicatorBorder;
    private Canvas _particleCanvas;
    private StackPanel _detailsPanel;
    private TextBlock _statusText;
    private Path _statusIcon;
    private ProgressBar _analysisProgress;
    private ListBox _issuesList;
    private DispatcherTimer _animationTimer;
    private DispatcherTimer _updateTimer;
    private readonly List<UIElement> _particles = new();
    private CompatibilityAnalyzer _analyzer;
    private CompatibilityResult _lastResult;
    private readonly Random _random = new();

    #endregion

    #region Constructor

    public HoloCompatibilityIndicator()
    {
        InitializeComponent();
        InitializeAnalyzer();
        InitializeAnimationSystem();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 300;
        Height = 200;
        Background = Brushes.Transparent;

        _rootGrid = new Grid();
        _rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Status indicator
        _rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Details

        CreateStatusIndicator();
        CreateDetailsPanel();
        CreateParticleSystem();

        Content = _rootGrid;
    }

    private void CreateStatusIndicator()
    {
        _indicatorBorder = new Border
        {
            Height = 60,
            Margin = new Thickness(10),
            CornerRadius = new CornerRadius(5),
            Background = CreateHolographicBackground(0.3),
            BorderBrush = GetBrushForStatus(CompatibilityStatus.Unknown),
            BorderThickness = new Thickness(2),
            Effect = CreateGlowEffect()
        };

        var indicatorGrid = new Grid();
        indicatorGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        indicatorGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        indicatorGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Status icon
        _statusIcon = new Path
        {
            Width = 24,
            Height = 24,
            Margin = new Thickness(10),
            Fill = GetBrushForStatus(CompatibilityStatus.Unknown),
            Data = GetGeometryForStatus(CompatibilityStatus.Unknown),
            Effect = CreateGlowEffect(0.8)
        };

        // Status text
        _statusText = new TextBlock
        {
            Text = "Analyzing Compatibility...",
            FontSize = 14,
            FontWeight = FontWeights.Medium,
            Foreground = GetBrushForStatus(CompatibilityStatus.Unknown),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 0)
        };

        // Analysis progress
        _analysisProgress = new ProgressBar
        {
            Width = 60,
            Height = 4,
            Margin = new Thickness(10),
            Background = CreateHolographicBackground(0.2),
            Foreground = GetBrushForStatus(CompatibilityStatus.Unknown),
            Visibility = Visibility.Collapsed
        };

        Grid.SetColumn(_statusIcon, 0);
        Grid.SetColumn(_statusText, 1);
        Grid.SetColumn(_analysisProgress, 2);

        indicatorGrid.Children.Add(_statusIcon);
        indicatorGrid.Children.Add(_statusText);
        indicatorGrid.Children.Add(_analysisProgress);

        _indicatorBorder.Child = indicatorGrid;
        Grid.SetRow(_indicatorBorder, 0);
        _rootGrid.Children.Add(_indicatorBorder);
    }

    private void CreateDetailsPanel()
    {
        _detailsPanel = new StackPanel
        {
            Margin = new Thickness(10, 0, 10, 10),
            Visibility = ShowDetailedAnalysis ? Visibility.Visible : Visibility.Collapsed
        };

        // Issues list
        _issuesList = new ListBox
        {
            Background = CreateHolographicBackground(0.1),
            BorderBrush = GetBrushForColorScheme(EVEColorScheme),
            BorderThickness = new Thickness(1),
            MaxHeight = 120,
            ItemContainerStyle = CreateHolographicListItemStyle()
        };

        _detailsPanel.Children.Add(_issuesList);

        Grid.SetRow(_detailsPanel, 1);
        _rootGrid.Children.Add(_detailsPanel);
    }

    private void CreateParticleSystem()
    {
        _particleCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false
        };

        Grid.SetRowSpan(_particleCanvas, 2);
        _rootGrid.Children.Add(_particleCanvas);
    }

    private void InitializeAnalyzer()
    {
        _analyzer = new CompatibilityAnalyzer();
    }

    private void InitializeAnimationSystem()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        _animationTimer.Tick += AnimationTimer_Tick;

        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _updateTimer.Tick += UpdateTimer_Tick;
    }

    #endregion

    #region Compatibility Analysis

    public async Task<CompatibilityResult> AnalyzeCompatibility()
    {
        if (TargetModule == null || TargetSlot == null || ShipData == null)
        {
            return new CompatibilityResult
            {
                Status = CompatibilityStatus.Unknown,
                Issues = new List<CompatibilityIssue>(),
                Message = "Insufficient data for analysis"
            };
        }

        StartAnalysisAnimation();

        try
        {
            var result = await _analyzer.AnalyzeAsync(TargetModule, TargetSlot, ShipData, AnalysisDepth);
            _lastResult = result;

            UpdateDisplay(result);
            CreateStatusParticles(result.Status);

            CompatibilityAnalyzed?.Invoke(this, new CompatibilityAnalysisEventArgs(result));

            if (result.Status == CompatibilityStatus.Warning || result.Status == CompatibilityStatus.Incompatible)
            {
                CompatibilityWarning?.Invoke(this, new CompatibilityWarningEventArgs(result.Issues));
            }

            return result;
        }
        finally
        {
            StopAnalysisAnimation();
        }
    }

    private void UpdateDisplay(CompatibilityResult result)
    {
        CompatibilityStatus = result.Status;
        _statusText.Text = result.Message;

        // Update colors and effects
        var statusBrush = GetBrushForStatus(result.Status);
        _indicatorBorder.BorderBrush = statusBrush;
        _statusIcon.Fill = statusBrush;
        _statusText.Foreground = statusBrush;
        _statusIcon.Data = GetGeometryForStatus(result.Status);

        // Update issues list
        if (ShowDetailedAnalysis && result.Issues.Any())
        {
            _issuesList.ItemsSource = result.Issues.Select(issue => new CompatibilityIssueViewModel(issue));
            _detailsPanel.Visibility = Visibility.Visible;
        }
        else
        {
            _detailsPanel.Visibility = Visibility.Collapsed;
        }

        // Animate status change
        if (EnableAnimations)
        {
            AnimateStatusChange(result.Status);
        }
    }

    private async void UpdateTimer_Tick(object sender, EventArgs e)
    {
        if (AutoUpdate && TargetModule != null && TargetSlot != null && ShipData != null)
        {
            await AnalyzeCompatibility();
        }
    }

    #endregion

    #region Animation System

    private void AnimationTimer_Tick(object sender, EventArgs e)
    {
        UpdateParticles();
    }

    private void UpdateParticles()
    {
        // Update and cleanup particles
        foreach (var particle in _particles.ToList())
        {
            if (particle.Opacity <= 0.1)
            {
                _particles.Remove(particle);
                _particleCanvas.Children.Remove(particle);
            }
        }
    }

    private void StartAnalysisAnimation()
    {
        _analysisProgress.Visibility = Visibility.Visible;
        
        if (EnableAnimations)
        {
            var progressAnimation = new DoubleAnimation
            {
                From = 0,
                To = 100,
                Duration = TimeSpan.FromMilliseconds(2000),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            _analysisProgress.BeginAnimation(ProgressBar.ValueProperty, progressAnimation);

            // Pulsing effect on indicator
            var pulseAnimation = new DoubleAnimation
            {
                From = 0.8,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(500),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            _indicatorBorder.BeginAnimation(OpacityProperty, pulseAnimation);
        }

        if (EnableParticleEffects)
        {
            CreateAnalysisParticles();
        }
    }

    private void StopAnalysisAnimation()
    {
        _analysisProgress.Visibility = Visibility.Collapsed;
        _analysisProgress.BeginAnimation(ProgressBar.ValueProperty, null);
        _indicatorBorder.BeginAnimation(OpacityProperty, null);
        _indicatorBorder.Opacity = 1.0;
    }

    private void AnimateStatusChange(CompatibilityStatus status)
    {
        // Flash effect for status change
        var flashAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.3,
            Duration = TimeSpan.FromMilliseconds(150),
            AutoReverse = true,
            RepeatBehavior = new RepeatBehavior(2)
        };

        _indicatorBorder.BeginAnimation(OpacityProperty, flashAnimation);

        // Scale effect
        var scaleAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 1.05,
            Duration = TimeSpan.FromMilliseconds(200),
            AutoReverse = true,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        if (_indicatorBorder.RenderTransform is not ScaleTransform)
        {
            _indicatorBorder.RenderTransform = new ScaleTransform(1, 1);
        }

        ((ScaleTransform)_indicatorBorder.RenderTransform).BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
        ((ScaleTransform)_indicatorBorder.RenderTransform).BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
    }

    #endregion

    #region Particle Effects

    private void CreateAnalysisParticles()
    {
        for (int i = 0; i < 8; i++)
        {
            Task.Delay(i * 250).ContinueWith(t =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    if (EnableParticleEffects)
                    {
                        CreateScanParticle();
                    }
                });
            });
        }
    }

    private void CreateScanParticle()
    {
        var particle = new Ellipse
        {
            Width = 4,
            Height = 4,
            Fill = GetBrushForColorScheme(EVEColorScheme),
            Opacity = 0.8,
            Effect = CreateGlowEffect(0.6)
        };

        var startX = _random.NextDouble() * ActualWidth;
        var startY = ActualHeight;
        var endX = _indicatorBorder.ActualWidth / 2;
        var endY = _indicatorBorder.ActualHeight / 2;

        Canvas.SetLeft(particle, startX);
        Canvas.SetTop(particle, startY);
        Canvas.SetZIndex(particle, 1000);

        _particles.Add(particle);
        _particleCanvas.Children.Add(particle);

        // Animate movement toward center
        var moveX = new DoubleAnimation
        {
            From = startX,
            To = endX,
            Duration = TimeSpan.FromMilliseconds(1500),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };

        var moveY = new DoubleAnimation
        {
            From = startY,
            To = endY,
            Duration = TimeSpan.FromMilliseconds(1500),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };

        var fadeOut = new DoubleAnimation
        {
            From = 0.8,
            To = 0,
            BeginTime = TimeSpan.FromMilliseconds(1200),
            Duration = TimeSpan.FromMilliseconds(300)
        };

        particle.BeginAnimation(Canvas.LeftProperty, moveX);
        particle.BeginAnimation(Canvas.TopProperty, moveY);
        particle.BeginAnimation(OpacityProperty, fadeOut);

        // Remove particle after animation
        Task.Delay(1500).ContinueWith(t =>
        {
            Dispatcher.BeginInvoke(() =>
            {
                _particles.Remove(particle);
                _particleCanvas.Children.Remove(particle);
            });
        });
    }

    private void CreateStatusParticles(CompatibilityStatus status)
    {
        if (!EnableParticleEffects) return;

        var particleCount = status switch
        {
            CompatibilityStatus.Compatible => 15,
            CompatibilityStatus.Warning => 10,
            CompatibilityStatus.Incompatible => 8,
            _ => 5
        };

        var particleColor = GetBrushForStatus(status);

        for (int i = 0; i < particleCount; i++)
        {
            CreateStatusParticle(particleColor);
        }
    }

    private void CreateStatusParticle(Brush color)
    {
        var particle = new Ellipse
        {
            Width = _random.NextDouble() * 6 + 2,
            Height = _random.NextDouble() * 6 + 2,
            Fill = color,
            Opacity = 0.7,
            Effect = CreateGlowEffect(0.5)
        };

        var centerX = _indicatorBorder.ActualWidth / 2;
        var centerY = _indicatorBorder.ActualHeight / 2;
        var angle = _random.NextDouble() * 2 * Math.PI;
        var distance = _random.NextDouble() * 80 + 20;

        var startX = centerX;
        var startY = centerY;
        var endX = centerX + distance * Math.Cos(angle);
        var endY = centerY + distance * Math.Sin(angle);

        Canvas.SetLeft(particle, startX);
        Canvas.SetTop(particle, startY);
        Canvas.SetZIndex(particle, 999);

        _particles.Add(particle);
        _particleCanvas.Children.Add(particle);

        // Animate explosion effect
        var moveX = new DoubleAnimation
        {
            From = startX,
            To = endX,
            Duration = TimeSpan.FromMilliseconds(800),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var moveY = new DoubleAnimation
        {
            From = startY,
            To = endY,
            Duration = TimeSpan.FromMilliseconds(800),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeOut = new DoubleAnimation
        {
            From = 0.7,
            To = 0,
            BeginTime = TimeSpan.FromMilliseconds(400),
            Duration = TimeSpan.FromMilliseconds(400)
        };

        particle.BeginAnimation(Canvas.LeftProperty, moveX);
        particle.BeginAnimation(Canvas.TopProperty, moveY);
        particle.BeginAnimation(OpacityProperty, fadeOut);

        // Remove particle after animation
        Task.Delay(800).ContinueWith(t =>
        {
            Dispatcher.BeginInvoke(() =>
            {
                _particles.Remove(particle);
                _particleCanvas.Children.Remove(particle);
            });
        });
    }

    #endregion

    #region Style Helpers

    private Brush CreateHolographicBackground(double opacity)
    {
        var brush = new LinearGradientBrush();
        brush.StartPoint = new Point(0, 0);
        brush.EndPoint = new Point(1, 1);

        var color = GetColorForScheme(EVEColorScheme);
        brush.GradientStops.Add(new GradientStop(Color.FromArgb((byte)(opacity * 40), color.R, color.G, color.B), 0));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb((byte)(opacity * 20), color.R, color.G, color.B), 0.5));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb((byte)(opacity * 40), color.R, color.G, color.B), 1));

        return brush;
    }

    private Brush GetBrushForStatus(CompatibilityStatus status)
    {
        return status switch
        {
            CompatibilityStatus.Compatible => new SolidColorBrush(Color.FromRgb(0, 255, 127)),  // Green
            CompatibilityStatus.Warning => new SolidColorBrush(Color.FromRgb(255, 191, 0)),    // Amber
            CompatibilityStatus.Incompatible => new SolidColorBrush(Color.FromRgb(220, 20, 60)), // Red
            CompatibilityStatus.Unknown => new SolidColorBrush(Color.FromRgb(128, 128, 128)),   // Gray
            _ => new SolidColorBrush(Color.FromRgb(0, 191, 255))                               // Blue
        };
    }

    private Brush GetBrushForColorScheme(EVEColorScheme scheme)
    {
        return scheme switch
        {
            EVEColorScheme.ElectricBlue => new SolidColorBrush(Color.FromRgb(0, 191, 255)),
            EVEColorScheme.AmberGold => new SolidColorBrush(Color.FromRgb(255, 191, 0)),
            EVEColorScheme.CrimsonRed => new SolidColorBrush(Color.FromRgb(220, 20, 60)),
            EVEColorScheme.EmeraldGreen => new SolidColorBrush(Color.FromRgb(0, 255, 127)),
            _ => new SolidColorBrush(Color.FromRgb(0, 191, 255))
        };
    }

    private Color GetColorForScheme(EVEColorScheme scheme)
    {
        return scheme switch
        {
            EVEColorScheme.ElectricBlue => Color.FromRgb(0, 191, 255),
            EVEColorScheme.AmberGold => Color.FromRgb(255, 191, 0),
            EVEColorScheme.CrimsonRed => Color.FromRgb(220, 20, 60),
            EVEColorScheme.EmeraldGreen => Color.FromRgb(0, 255, 127),
            _ => Color.FromRgb(0, 191, 255)
        };
    }

    private Geometry GetGeometryForStatus(CompatibilityStatus status)
    {
        return status switch
        {
            CompatibilityStatus.Compatible => Geometry.Parse("M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M11,16.5L6.5,12L7.91,10.59L11,13.67L16.59,8.09L18,9.5L11,16.5Z"),
            CompatibilityStatus.Warning => Geometry.Parse("M13,14H11V10H13M13,18H11V16H13M1,21H23L12,2L1,21Z"),
            CompatibilityStatus.Incompatible => Geometry.Parse("M12,2C17.53,2 22,6.47 22,12C22,17.53 17.53,22 12,22C6.47,22 2,17.53 2,12C2,6.47 6.47,2 12,2M15.59,7L12,10.59L8.41,7L7,8.41L10.59,12L7,15.59L8.41,17L12,13.41L15.59,17L17,15.59L13.41,12L17,8.41L15.59,7Z"),
            CompatibilityStatus.Unknown => Geometry.Parse("M11,18H13V16H11V18M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,6A4,4 0 0,1 16,10C16,10.39 15.96,10.78 15.88,11.14C15.79,11.61 15.64,12.08 15.47,12.54C15.12,13.47 14.78,14.21 14.78,15A1,1 0 0,1 13.78,16H10.22A1,1 0 0,1 9.22,15C9.22,14.21 8.88,13.47 8.53,12.54C8.36,12.08 8.21,11.61 8.12,11.14C8.04,10.78 8,10.39 8,10A4,4 0 0,1 12,6Z"),
            _ => Geometry.Parse("M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2Z")
        };
    }

    private Effect CreateGlowEffect(double intensity = 1.0)
    {
        return new DropShadowEffect
        {
            Color = GetColorForScheme(EVEColorScheme),
            BlurRadius = 8 * intensity,
            Opacity = 0.6 * intensity,
            ShadowDepth = 0
        };
    }

    private Style CreateHolographicListItemStyle()
    {
        var style = new Style(typeof(ListBoxItem));
        style.Setters.Add(new Setter(BackgroundProperty, CreateHolographicBackground(0.1)));
        style.Setters.Add(new Setter(ForegroundProperty, GetBrushForColorScheme(EVEColorScheme)));
        style.Setters.Add(new Setter(BorderBrushProperty, GetBrushForColorScheme(EVEColorScheme)));
        style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(MarginProperty, new Thickness(2)));
        style.Setters.Add(new Setter(PaddingProperty, new Thickness(8)));
        return style;
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableAnimations)
        {
            _animationTimer.Start();
        }

        if (AutoUpdate)
        {
            _updateTimer.Start();
        }

        if (TargetModule != null && TargetSlot != null && ShipData != null)
        {
            Task.Run(AnalyzeCompatibility);
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        _updateTimer?.Stop();
    }

    #endregion

    #region Dependency Property Callbacks

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Update holographic effects intensity
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCompatibilityIndicator indicator)
        {
            // Update all UI elements with new color scheme
            indicator._indicatorBorder.BorderBrush = indicator.GetBrushForColorScheme((EVEColorScheme)e.NewValue);
        }
    }

    private static void OnTargetModuleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCompatibilityIndicator indicator && indicator.AutoUpdate)
        {
            Task.Run(indicator.AnalyzeCompatibility);
        }
    }

    private static void OnTargetSlotChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCompatibilityIndicator indicator && indicator.AutoUpdate)
        {
            Task.Run(indicator.AnalyzeCompatibility);
        }
    }

    private static void OnShipDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCompatibilityIndicator indicator && indicator.AutoUpdate)
        {
            Task.Run(indicator.AnalyzeCompatibility);
        }
    }

    private static void OnCompatibilityStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Status changed - update display
    }

    private static void OnShowDetailedAnalysisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCompatibilityIndicator indicator)
        {
            indicator._detailsPanel.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCompatibilityIndicator indicator)
        {
            if ((bool)e.NewValue)
                indicator._animationTimer.Start();
            else
                indicator._animationTimer.Stop();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCompatibilityIndicator indicator && !(bool)e.NewValue)
        {
            // Clear all particles
            foreach (var particle in indicator._particles.ToList())
            {
                indicator._particles.Remove(particle);
                indicator._particleCanvas.Children.Remove(particle);
            }
        }
    }

    private static void OnShowWarningsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Warning display setting changed
    }

    private static void OnAutoUpdateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCompatibilityIndicator indicator)
        {
            if ((bool)e.NewValue)
                indicator._updateTimer.Start();
            else
                indicator._updateTimer.Stop();
        }
    }

    private static void OnAnalysisDepthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCompatibilityIndicator indicator && indicator.AutoUpdate)
        {
            Task.Run(indicator.AnalyzeCompatibility);
        }
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void SetSimplifiedMode(bool enabled)
    {
        EnableAnimations = !enabled;
        EnableParticleEffects = !enabled;
        
        if (enabled)
        {
            _animationTimer?.Stop();
            foreach (var particle in _particles.ToList())
            {
                _particles.Remove(particle);
                _particleCanvas.Children.Remove(particle);
            }
        }
        else
        {
            _animationTimer?.Start();
        }
    }

    public bool IsSimplifiedModeActive => !EnableAnimations || !EnableParticleEffects;

    #endregion

    #region Public Methods

    /// <summary>
    /// Forces immediate compatibility analysis
    /// </summary>
    public async Task RefreshAnalysis()
    {
        await AnalyzeCompatibility();
    }

    /// <summary>
    /// Clears current analysis results
    /// </summary>
    public void ClearAnalysis()
    {
        CompatibilityStatus = CompatibilityStatus.Unknown;
        _statusText.Text = "No analysis available";
        _issuesList.ItemsSource = null;
        _lastResult = null;
    }

    /// <summary>
    /// Gets the last compatibility analysis result
    /// </summary>
    public CompatibilityResult GetLastResult()
    {
        return _lastResult;
    }

    #endregion
}

#region Supporting Types

/// <summary>
/// Compatibility analysis status
/// </summary>
public enum CompatibilityStatus
{
    Unknown,
    Compatible,
    Warning,
    Incompatible
}

/// <summary>
/// Depth of compatibility analysis
/// </summary>
public enum AnalysisDepth
{
    Basic,      // Slot type and basic requirements only
    Standard,   // Include power, CPU, skills
    Complete    // Full analysis including stacking, conflicts
}

/// <summary>
/// Compatibility analysis result
/// </summary>
public class CompatibilityResult
{
    public CompatibilityStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<CompatibilityIssue> Issues { get; set; } = new();
    public double CompatibilityScore { get; set; } // 0.0 - 1.0
    public TimeSpan AnalysisTime { get; set; }
}

/// <summary>
/// Individual compatibility issue
/// </summary>
public class CompatibilityIssue
{
    public CompatibilityIssueType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public CompatibilityIssueSeverity Severity { get; set; }
    public string Suggestion { get; set; } = string.Empty;
}

/// <summary>
/// Types of compatibility issues
/// </summary>
public enum CompatibilityIssueType
{
    SlotType,
    PowerGrid,
    CPU,
    Calibration,
    Skills,
    Stacking,
    Conflict,
    ShipRestriction
}

/// <summary>
/// Severity levels for compatibility issues
/// </summary>
public enum CompatibilityIssueSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

/// <summary>
/// View model for compatibility issues in the list
/// </summary>
public class CompatibilityIssueViewModel
{
    public CompatibilityIssue Issue { get; }
    public string DisplayText { get; }
    public Brush SeverityBrush { get; }

    public CompatibilityIssueViewModel(CompatibilityIssue issue)
    {
        Issue = issue;
        DisplayText = $"{issue.Type}: {issue.Description}";
        SeverityBrush = GetBrushForSeverity(issue.Severity);
    }

    private Brush GetBrushForSeverity(CompatibilityIssueSeverity severity)
    {
        return severity switch
        {
            CompatibilityIssueSeverity.Info => new SolidColorBrush(Color.FromRgb(0, 191, 255)),
            CompatibilityIssueSeverity.Warning => new SolidColorBrush(Color.FromRgb(255, 191, 0)),
            CompatibilityIssueSeverity.Error => new SolidColorBrush(Color.FromRgb(220, 20, 60)),
            CompatibilityIssueSeverity.Critical => new SolidColorBrush(Color.FromRgb(128, 0, 0)),
            _ => new SolidColorBrush(Color.FromRgb(128, 128, 128))
        };
    }
}

/// <summary>
/// Event args for compatibility analysis completion
/// </summary>
public class CompatibilityAnalysisEventArgs : EventArgs
{
    public CompatibilityResult Result { get; }

    public CompatibilityAnalysisEventArgs(CompatibilityResult result)
    {
        Result = result;
    }
}

/// <summary>
/// Event args for compatibility warnings
/// </summary>
public class CompatibilityWarningEventArgs : EventArgs
{
    public List<CompatibilityIssue> Issues { get; }

    public CompatibilityWarningEventArgs(List<CompatibilityIssue> issues)
    {
        Issues = issues;
    }
}

/// <summary>
/// Compatibility analyzer engine
/// </summary>
public class CompatibilityAnalyzer
{
    public async Task<CompatibilityResult> AnalyzeAsync(HoloModuleData module, HoloFittingSlot slot, HoloShipData ship, AnalysisDepth depth)
    {
        var result = new CompatibilityResult();
        var startTime = DateTime.UtcNow;

        try
        {
            // Simulate analysis time
            await Task.Delay(500);

            var issues = new List<CompatibilityIssue>();

            // Basic slot type compatibility
            if (!IsSlotTypeCompatible(module, slot))
            {
                issues.Add(new CompatibilityIssue
                {
                    Type = CompatibilityIssueType.SlotType,
                    Description = $"Module cannot be fitted to {slot.SlotType} slot",
                    Severity = CompatibilityIssueSeverity.Error
                });
            }

            if (depth >= AnalysisDepth.Standard)
            {
                // Power grid check
                if (!HasSufficientPowerGrid(module, ship))
                {
                    issues.Add(new CompatibilityIssue
                    {
                        Type = CompatibilityIssueType.PowerGrid,
                        Description = "Insufficient power grid",
                        Severity = CompatibilityIssueSeverity.Error,
                        Suggestion = "Consider power grid upgrades or lower power modules"
                    });
                }

                // CPU check
                if (!HasSufficientCPU(module, ship))
                {
                    issues.Add(new CompatibilityIssue
                    {
                        Type = CompatibilityIssueType.CPU,
                        Description = "Insufficient CPU",
                        Severity = CompatibilityIssueSeverity.Error,
                        Suggestion = "Consider CPU upgrades or lower CPU modules"
                    });
                }

                // Skills check
                var missingSkills = CheckRequiredSkills(module);
                if (missingSkills.Any())
                {
                    issues.Add(new CompatibilityIssue
                    {
                        Type = CompatibilityIssueType.Skills,
                        Description = $"Missing required skills: {string.Join(", ", missingSkills)}",
                        Severity = CompatibilityIssueSeverity.Warning,
                        Suggestion = "Train required skills before fitting"
                    });
                }
            }

            if (depth >= AnalysisDepth.Complete)
            {
                // Stacking penalties
                var stackingIssues = CheckStackingPenalties(module, ship);
                issues.AddRange(stackingIssues);

                // Module conflicts
                var conflictIssues = CheckModuleConflicts(module, ship);
                issues.AddRange(conflictIssues);
            }

            // Determine overall status
            result.Status = DetermineOverallStatus(issues);
            result.Message = GenerateStatusMessage(result.Status, issues);
            result.Issues = issues;
            result.CompatibilityScore = CalculateCompatibilityScore(issues);
        }
        finally
        {
            result.AnalysisTime = DateTime.UtcNow - startTime;
        }

        return result;
    }

    private bool IsSlotTypeCompatible(HoloModuleData module, HoloFittingSlot slot)
    {
        // Simplified compatibility check
        return module.Category switch
        {
            ModuleCategory.Weapons => slot.SlotType == SlotType.High,
            ModuleCategory.Defense => slot.SlotType == SlotType.Mid || slot.SlotType == SlotType.Low,
            ModuleCategory.Electronics => slot.SlotType == SlotType.Mid,
            ModuleCategory.Engineering => slot.SlotType == SlotType.Low,
            ModuleCategory.Rigs => slot.SlotType == SlotType.Rig,
            _ => false
        };
    }

    private bool HasSufficientPowerGrid(HoloModuleData module, HoloShipData ship)
    {
        // Simplified power grid check
        return true; // In real implementation, check actual values
    }

    private bool HasSufficientCPU(HoloModuleData module, HoloShipData ship)
    {
        // Simplified CPU check
        return true; // In real implementation, check actual values
    }

    private List<string> CheckRequiredSkills(HoloModuleData module)
    {
        // Simplified skills check
        return new List<string>(); // In real implementation, check against character skills
    }

    private List<CompatibilityIssue> CheckStackingPenalties(HoloModuleData module, HoloShipData ship)
    {
        // Simplified stacking check
        return new List<CompatibilityIssue>();
    }

    private List<CompatibilityIssue> CheckModuleConflicts(HoloModuleData module, HoloShipData ship)
    {
        // Simplified conflict check
        return new List<CompatibilityIssue>();
    }

    private CompatibilityStatus DetermineOverallStatus(List<CompatibilityIssue> issues)
    {
        if (!issues.Any())
            return CompatibilityStatus.Compatible;

        if (issues.Any(i => i.Severity == CompatibilityIssueSeverity.Error || i.Severity == CompatibilityIssueSeverity.Critical))
            return CompatibilityStatus.Incompatible;

        if (issues.Any(i => i.Severity == CompatibilityIssueSeverity.Warning))
            return CompatibilityStatus.Warning;

        return CompatibilityStatus.Compatible;
    }

    private string GenerateStatusMessage(CompatibilityStatus status, List<CompatibilityIssue> issues)
    {
        return status switch
        {
            CompatibilityStatus.Compatible => "Module is compatible",
            CompatibilityStatus.Warning => $"Compatible with {issues.Count} warning(s)",
            CompatibilityStatus.Incompatible => $"Incompatible - {issues.Count} issue(s)",
            _ => "Analysis required"
        };
    }

    private double CalculateCompatibilityScore(List<CompatibilityIssue> issues)
    {
        if (!issues.Any())
            return 1.0;

        var penalty = issues.Sum(issue => issue.Severity switch
        {
            CompatibilityIssueSeverity.Info => 0.05,
            CompatibilityIssueSeverity.Warning => 0.15,
            CompatibilityIssueSeverity.Error => 0.5,
            CompatibilityIssueSeverity.Critical => 1.0,
            _ => 0.1
        });

        return Math.Max(0.0, 1.0 - penalty);
    }
}

#endregion