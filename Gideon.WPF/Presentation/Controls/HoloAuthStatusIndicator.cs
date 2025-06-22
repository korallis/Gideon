// ==========================================================================
// HoloAuthStatusIndicator.cs - Holographic Authentication Status Indicators
// ==========================================================================
// Advanced authentication status display featuring holographic indicators,
// animated connection states, real-time status updates, and visual feedback.
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
/// Holographic authentication status indicator with animated states and real-time feedback
/// </summary>
public class HoloAuthStatusIndicator : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloAuthStatusIndicator),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloAuthStatusIndicator),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty AuthenticationStatusProperty =
        DependencyProperty.Register(nameof(AuthenticationStatus), typeof(AuthStatus), typeof(HoloAuthStatusIndicator),
            new PropertyMetadata(AuthStatus.Disconnected, OnAuthenticationStatusChanged));

    public static readonly DependencyProperty ConnectionQualityProperty =
        DependencyProperty.Register(nameof(ConnectionQuality), typeof(ConnectionQuality), typeof(HoloAuthStatusIndicator),
            new PropertyMetadata(ConnectionQuality.Unknown, OnConnectionQualityChanged));

    public static readonly DependencyProperty StatusMessageProperty =
        DependencyProperty.Register(nameof(StatusMessage), typeof(string), typeof(HoloAuthStatusIndicator),
            new PropertyMetadata("Initializing...", OnStatusMessageChanged));

    public static readonly DependencyProperty ShowDetailedStatusProperty =
        DependencyProperty.Register(nameof(ShowDetailedStatus), typeof(bool), typeof(HoloAuthStatusIndicator),
            new PropertyMetadata(true));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloAuthStatusIndicator),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloAuthStatusIndicator),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty CompactModeProperty =
        DependencyProperty.Register(nameof(CompactMode), typeof(bool), typeof(HoloAuthStatusIndicator),
            new PropertyMetadata(false, OnCompactModeChanged));

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

    public AuthStatus AuthenticationStatus
    {
        get => (AuthStatus)GetValue(AuthenticationStatusProperty);
        set => SetValue(AuthenticationStatusProperty, value);
    }

    public ConnectionQuality ConnectionQuality
    {
        get => (ConnectionQuality)GetValue(ConnectionQualityProperty);
        set => SetValue(ConnectionQualityProperty, value);
    }

    public string StatusMessage
    {
        get => (string)GetValue(StatusMessageProperty);
        set => SetValue(StatusMessageProperty, value);
    }

    public bool ShowDetailedStatus
    {
        get => (bool)GetValue(ShowDetailedStatusProperty);
        set => SetValue(ShowDetailedStatusProperty, value);
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

    public bool CompactMode
    {
        get => (bool)GetValue(CompactModeProperty);
        set => SetValue(CompactModeProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloAuthStatusEventArgs> StatusChanged;
    public event EventHandler<HoloAuthStatusEventArgs> ConnectionQualityChanged;
    public event EventHandler<HoloAuthStatusEventArgs> StatusClicked;
    public event EventHandler<HoloAuthStatusEventArgs> DetailToggled;

    #endregion

    #region Private Fields

    private readonly DispatcherTimer _animationTimer;
    private readonly DispatcherTimer _pulseTimer;
    private readonly Random _random = new();
    private Canvas _mainCanvas;
    private Grid _statusContainer;
    private Ellipse _statusIndicator;
    private Rectangle _connectionBar;
    private TextBlock _statusText;
    private TextBlock _detailText;
    private StackPanel _connectionBars;
    private Grid _detailPanel;
    private Canvas _particleCanvas;
    private readonly List<UIElement> _particles = new();
    private readonly List<Rectangle> _signalBars = new();
    private bool _isSimplifiedMode;
    private DateTime _lastStatusChange;
    private Storyboard _pulseStoryboard;
    private double _animationPhase;

    #endregion

    #region Constructor

    public HoloAuthStatusIndicator()
    {
        InitializeComponent();
        InitializeTimers();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = CompactMode ? 120 : 200;
        Height = CompactMode ? 30 : 80;
        Background = new SolidColorBrush(Color.FromArgb(30, 100, 200, 255));
        BorderBrush = new SolidColorBrush(Color.FromArgb(100, 100, 200, 255));
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(4);
        Effect = new DropShadowEffect
        {
            Color = Color.FromArgb(80, 100, 200, 255),
            BlurRadius = 8,
            ShadowDepth = 2,
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
                    new GradientStop(Color.FromArgb(40, 100, 200, 255), 0),
                    new GradientStop(Color.FromArgb(15, 50, 100, 200), 0.5),
                    new GradientStop(Color.FromArgb(30, 100, 200, 255), 1)
                }
            },
            Width = Width,
            Height = Height
        };
        _mainCanvas.Children.Add(backgroundRect);

        // Main container
        _statusContainer = new Grid
        {
            Width = Width,
            Height = Height,
            Margin = new Thickness(5)
        };
        _mainCanvas.Children.Add(_statusContainer);

        if (CompactMode)
        {
            CreateCompactLayout();
        }
        else
        {
            CreateFullLayout();
        }

        CreateParticleCanvas();
    }

    private void CreateCompactLayout()
    {
        _statusContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        _statusContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        _statusContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Status indicator
        _statusIndicator = new Ellipse
        {
            Width = 12,
            Height = 12,
            Margin = new Thickness(5, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(_statusIndicator, 0);
        _statusContainer.Children.Add(_statusIndicator);

        // Status text
        _statusText = new TextBlock
        {
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 0),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
        };
        Grid.SetColumn(_statusText, 1);
        _statusContainer.Children.Add(_statusText);

        // Connection quality bars
        CreateConnectionBars();
        Grid.SetColumn(_connectionBars, 2);
        _statusContainer.Children.Add(_connectionBars);
    }

    private void CreateFullLayout()
    {
        _statusContainer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _statusContainer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _statusContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        // Header with status indicator and text
        var headerPanel = new DockPanel
        {
            Height = 25,
            Margin = new Thickness(5)
        };
        Grid.SetRow(headerPanel, 0);
        _statusContainer.Children.Add(headerPanel);

        _statusIndicator = new Ellipse
        {
            Width = 16,
            Height = 16,
            Margin = new Thickness(0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        DockPanel.SetDock(_statusIndicator, Dock.Left);
        headerPanel.Children.Add(_statusIndicator);

        _statusText = new TextBlock
        {
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
        };
        headerPanel.Children.Add(_statusText);

        CreateConnectionBars();
        DockPanel.SetDock(_connectionBars, Dock.Right);
        headerPanel.Children.Add(_connectionBars);

        // Detail text
        _detailText = new TextBlock
        {
            FontSize = 10,
            Margin = new Thickness(5, 0),
            Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 200, 200)),
            TextWrapping = TextWrapping.Wrap,
            Visibility = ShowDetailedStatus ? Visibility.Visible : Visibility.Collapsed
        };
        Grid.SetRow(_detailText, 1);
        _statusContainer.Children.Add(_detailText);

        // Detail panel (for expanded info)
        _detailPanel = new Grid
        {
            Margin = new Thickness(5),
            Visibility = ShowDetailedStatus ? Visibility.Visible : Visibility.Collapsed
        };
        Grid.SetRow(_detailPanel, 2);
        _statusContainer.Children.Add(_detailPanel);

        CreateDetailPanel();
    }

    private void CreateConnectionBars()
    {
        _connectionBars = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };

        for (int i = 0; i < 4; i++)
        {
            var bar = new Rectangle
            {
                Width = 3,
                Height = 6 + (i * 2),
                Margin = new Thickness(1),
                Fill = new SolidColorBrush(Color.FromArgb(100, 100, 200, 255)),
                VerticalAlignment = VerticalAlignment.Bottom
            };
            _signalBars.Add(bar);
            _connectionBars.Children.Add(bar);
        }
    }

    private void CreateDetailPanel()
    {
        var detailGrid = new Grid();
        detailGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        detailGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Connection progress bar
        _connectionBar = new Rectangle
        {
            Height = 4,
            Margin = new Thickness(0, 2),
            Fill = new SolidColorBrush(Color.FromArgb(150, 100, 200, 255)),
            HorizontalAlignment = HorizontalAlignment.Left
        };
        Grid.SetRow(_connectionBar, 0);
        detailGrid.Children.Add(_connectionBar);

        // Additional status info
        var statusInfo = new TextBlock
        {
            FontSize = 9,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 180, 180, 180)),
            Text = "ESI API • OAuth2 • Secure",
            Margin = new Thickness(0, 2)
        };
        Grid.SetRow(statusInfo, 1);
        detailGrid.Children.Add(statusInfo);

        _detailPanel.Children.Add(detailGrid);
    }

    private void CreateParticleCanvas()
    {
        _particleCanvas = new Canvas
        {
            Width = Width,
            Height = Height,
            IsHitTestVisible = false
        };
        _mainCanvas.Children.Add(_particleCanvas);
    }

    private void InitializeTimers()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };
        _animationTimer.Tick += OnAnimationTimerTick;
        _animationTimer.Start();

        _pulseTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _pulseTimer.Tick += OnPulseTimerTick;
    }

    #endregion

    #region Status Management

    public void UpdateAuthenticationStatus(AuthStatus status, string message = null, ConnectionQuality quality = ConnectionQuality.Unknown)
    {
        AuthenticationStatus = status;
        if (!string.IsNullOrEmpty(message))
            StatusMessage = message;
        if (quality != ConnectionQuality.Unknown)
            ConnectionQuality = quality;

        _lastStatusChange = DateTime.Now;

        if (EnableAnimations && !_isSimplifiedMode)
        {
            AnimateStatusChange();
        }

        if (EnableParticleEffects && !_isSimplifiedMode)
        {
            SpawnStatusParticles(status);
        }

        StatusChanged?.Invoke(this, new HoloAuthStatusEventArgs
        {
            Status = status,
            Message = message,
            Quality = quality,
            Timestamp = DateTime.Now
        });
    }

    public void SimulateConnectionProgress(double progress)
    {
        if (_connectionBar != null)
        {
            var targetWidth = (_detailPanel?.ActualWidth ?? 100) * Math.Max(0, Math.Min(1, progress));
            _connectionBar.Width = targetWidth;
        }
    }

    #endregion

    #region Visual Updates

    private void UpdateStatusIndicator()
    {
        if (_statusIndicator == null) return;

        var (color, effect) = GetStatusVisualization(AuthenticationStatus);
        
        _statusIndicator.Fill = new SolidColorBrush(color);
        
        if (EnableAnimations && !_isSimplifiedMode && effect != IndicatorEffect.None)
        {
            ApplyStatusEffect(effect);
        }

        // Update status text
        if (_statusText != null)
        {
            _statusText.Text = GetStatusDisplayText();
        }

        // Update detail text
        if (_detailText != null)
        {
            _detailText.Text = StatusMessage;
        }
    }

    private void UpdateConnectionQualityBars()
    {
        var qualityLevel = GetConnectionQualityLevel();
        
        for (int i = 0; i < _signalBars.Count; i++)
        {
            var bar = _signalBars[i];
            var isActive = i < qualityLevel;
            
            bar.Fill = new SolidColorBrush(isActive 
                ? GetConnectionQualityColor()
                : Color.FromArgb(50, 100, 100, 100));

            if (EnableAnimations && !_isSimplifiedMode && isActive)
            {
                AnimateSignalBar(bar, i);
            }
        }
    }

    private (Color color, IndicatorEffect effect) GetStatusVisualization(AuthStatus status)
    {
        return status switch
        {
            AuthStatus.Connected => (Color.FromArgb(255, 100, 255, 100), IndicatorEffect.Steady),
            AuthStatus.Connecting => (Color.FromArgb(255, 255, 200, 100), IndicatorEffect.Pulse),
            AuthStatus.Authenticating => (Color.FromArgb(255, 100, 200, 255), IndicatorEffect.FastPulse),
            AuthStatus.Disconnected => (Color.FromArgb(255, 150, 150, 150), IndicatorEffect.None),
            AuthStatus.Error => (Color.FromArgb(255, 255, 100, 100), IndicatorEffect.Blink),
            AuthStatus.Expired => (Color.FromArgb(255, 255, 150, 100), IndicatorEffect.SlowPulse),
            _ => (Color.FromArgb(255, 200, 200, 200), IndicatorEffect.None)
        };
    }

    private string GetStatusDisplayText()
    {
        return AuthenticationStatus switch
        {
            AuthStatus.Connected => "Connected",
            AuthStatus.Connecting => "Connecting...",
            AuthStatus.Authenticating => "Authenticating...",
            AuthStatus.Disconnected => "Disconnected",
            AuthStatus.Error => "Error",
            AuthStatus.Expired => "Token Expired",
            _ => "Unknown"
        };
    }

    private int GetConnectionQualityLevel()
    {
        return ConnectionQuality switch
        {
            ConnectionQuality.Excellent => 4,
            ConnectionQuality.Good => 3,
            ConnectionQuality.Fair => 2,
            ConnectionQuality.Poor => 1,
            _ => 0
        };
    }

    private Color GetConnectionQualityColor()
    {
        return ConnectionQuality switch
        {
            ConnectionQuality.Excellent => Color.FromArgb(255, 100, 255, 100),
            ConnectionQuality.Good => Color.FromArgb(255, 150, 255, 100),
            ConnectionQuality.Fair => Color.FromArgb(255, 255, 200, 100),
            ConnectionQuality.Poor => Color.FromArgb(255, 255, 150, 100),
            _ => Color.FromArgb(255, 150, 150, 150)
        };
    }

    #endregion

    #region Animation Methods

    private void AnimateStatusChange()
    {
        if (_isSimplifiedMode) return;

        // Scale animation
        var scaleTransform = new ScaleTransform(1, 1);
        _statusIndicator.RenderTransform = scaleTransform;
        _statusIndicator.RenderTransformOrigin = new Point(0.5, 0.5);

        var scaleAnimation = new DoubleAnimation
        {
            From = 1,
            To = 1.3,
            Duration = TimeSpan.FromMilliseconds(200),
            AutoReverse = true,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
    }

    private void ApplyStatusEffect(IndicatorEffect effect)
    {
        if (_pulseStoryboard != null)
        {
            _pulseStoryboard.Stop();
            _pulseStoryboard = null;
        }

        switch (effect)
        {
            case IndicatorEffect.Pulse:
                _pulseTimer.Interval = TimeSpan.FromMilliseconds(800);
                _pulseTimer.Start();
                break;
            case IndicatorEffect.FastPulse:
                _pulseTimer.Interval = TimeSpan.FromMilliseconds(400);
                _pulseTimer.Start();
                break;
            case IndicatorEffect.SlowPulse:
                _pulseTimer.Interval = TimeSpan.FromMilliseconds(1200);
                _pulseTimer.Start();
                break;
            case IndicatorEffect.Blink:
                CreateBlinkAnimation();
                break;
            case IndicatorEffect.Steady:
                _pulseTimer.Stop();
                _statusIndicator.Opacity = 1.0;
                break;
        }
    }

    private void CreateBlinkAnimation()
    {
        _pulseStoryboard = new Storyboard
        {
            RepeatBehavior = RepeatBehavior.Forever
        };

        var blinkAnimation = new DoubleAnimationUsingKeyFrames
        {
            Duration = TimeSpan.FromMilliseconds(600)
        };

        blinkAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        blinkAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0.2, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300))));
        blinkAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(600))));

        Storyboard.SetTarget(blinkAnimation, _statusIndicator);
        Storyboard.SetTargetProperty(blinkAnimation, new PropertyPath(OpacityProperty));

        _pulseStoryboard.Children.Add(blinkAnimation);
        _pulseStoryboard.Begin();
    }

    private void AnimateSignalBar(Rectangle bar, int index)
    {
        var delay = index * 100;
        
        var heightAnimation = new DoubleAnimation
        {
            From = bar.Height,
            To = bar.Height * 1.2,
            Duration = TimeSpan.FromMilliseconds(300),
            AutoReverse = true,
            BeginTime = TimeSpan.FromMilliseconds(delay),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        bar.BeginAnimation(HeightProperty, heightAnimation);
    }

    #endregion

    #region Particle Effects

    private void SpawnStatusParticles(AuthStatus status)
    {
        if (_isSimplifiedMode || !EnableParticleEffects) return;

        var particleCount = GetParticleCountForStatus(status);
        var color = GetStatusVisualization(status).color;

        for (int i = 0; i < particleCount; i++)
        {
            var particle = new Ellipse
            {
                Width = _random.Next(2, 5),
                Height = _random.Next(2, 5),
                Fill = new SolidColorBrush(Color.FromArgb(
                    (byte)_random.Next(100, 200),
                    color.R, color.G, color.B))
            };

            var startX = _statusIndicator.Margin.Left + 8;
            var startY = _statusIndicator.Margin.Top + 8;

            Canvas.SetLeft(particle, startX);
            Canvas.SetTop(particle, startY);
            _particleCanvas.Children.Add(particle);
            _particles.Add(particle);

            AnimateStatusParticle(particle);
        }

        CleanupParticles();
    }

    private int GetParticleCountForStatus(AuthStatus status)
    {
        return status switch
        {
            AuthStatus.Connected => 8,
            AuthStatus.Connecting => 12,
            AuthStatus.Authenticating => 15,
            AuthStatus.Error => 20,
            _ => 5
        };
    }

    private void AnimateStatusParticle(UIElement particle)
    {
        var angle = _random.NextDouble() * Math.PI * 2;
        var distance = _random.Next(20, 40);
        var targetX = Canvas.GetLeft(particle) + Math.Cos(angle) * distance;
        var targetY = Canvas.GetTop(particle) + Math.Sin(angle) * distance;

        var moveXAnimation = new DoubleAnimation
        {
            From = Canvas.GetLeft(particle),
            To = targetX,
            Duration = TimeSpan.FromMilliseconds(_random.Next(800, 1500)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var moveYAnimation = new DoubleAnimation
        {
            From = Canvas.GetTop(particle),
            To = targetY,
            Duration = TimeSpan.FromMilliseconds(_random.Next(800, 1500)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(_random.Next(800, 1500))
        };

        moveXAnimation.Completed += (s, e) =>
        {
            _particleCanvas.Children.Remove(particle);
            _particles.Remove(particle);
        };

        particle.BeginAnimation(Canvas.LeftProperty, moveXAnimation);
        particle.BeginAnimation(Canvas.TopProperty, moveYAnimation);
        particle.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void CleanupParticles()
    {
        if (_particles.Count > 50)
        {
            var particlesToRemove = _particles.Take(25).ToList();
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
        UpdateStatusIndicator();
        UpdateConnectionQualityBars();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        _pulseTimer?.Stop();
        _pulseStoryboard?.Stop();
        CleanupAllParticles();
    }

    private void OnAnimationTimerTick(object sender, EventArgs e)
    {
        if (EnableAnimations && !_isSimplifiedMode)
        {
            _animationPhase += 0.1;
            if (_animationPhase > Math.PI * 2)
                _animationPhase = 0;

            UpdateAnimatedElements();
        }
    }

    private void OnPulseTimerTick(object sender, EventArgs e)
    {
        if (EnableAnimations && !_isSimplifiedMode)
        {
            var pulseOpacity = 0.6 + (Math.Sin(_animationPhase * 3) * 0.4);
            _statusIndicator.Opacity = pulseOpacity * HolographicIntensity;
        }
    }

    private void UpdateAnimatedElements()
    {
        // Subtle animation for connection bars when connected
        if (AuthenticationStatus == AuthStatus.Connected)
        {
            for (int i = 0; i < _signalBars.Count; i++)
            {
                var bar = _signalBars[i];
                var baseOpacity = i < GetConnectionQualityLevel() ? 1.0 : 0.3;
                var animatedOpacity = baseOpacity + (Math.Sin(_animationPhase + i * 0.5) * 0.2);
                bar.Opacity = Math.Max(0.1, animatedOpacity) * HolographicIntensity;
            }
        }
    }

    #endregion

    #region Helper Methods

    private void CleanupAllParticles()
    {
        foreach (var particle in _particles.ToList())
        {
            _particleCanvas.Children.Remove(particle);
        }
        _particles.Clear();
    }

    #endregion

    #region Property Change Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAuthStatusIndicator control)
        {
            control.ApplyHolographicIntensity();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAuthStatusIndicator control)
        {
            control.ApplyEVEColorScheme();
        }
    }

    private static void OnAuthenticationStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAuthStatusIndicator control)
        {
            control.UpdateStatusIndicator();
        }
    }

    private static void OnConnectionQualityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAuthStatusIndicator control)
        {
            control.UpdateConnectionQualityBars();
            control.ConnectionQualityChanged?.Invoke(control, new HoloAuthStatusEventArgs
            {
                Quality = control.ConnectionQuality,
                Timestamp = DateTime.Now
            });
        }
    }

    private static void OnStatusMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAuthStatusIndicator control && control._detailText != null)
        {
            control._detailText.Text = control.StatusMessage;
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAuthStatusIndicator control)
        {
            if (!(bool)e.NewValue)
            {
                control._pulseTimer?.Stop();
                control._pulseStoryboard?.Stop();
            }
            else
            {
                control.UpdateStatusIndicator();
            }
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAuthStatusIndicator control && !(bool)e.NewValue)
        {
            control.CleanupAllParticles();
        }
    }

    private static void OnCompactModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAuthStatusIndicator control)
        {
            control.Width = control.CompactMode ? 120 : 200;
            control.Height = control.CompactMode ? 30 : 80;
            control.InitializeComponent(); // Recreate layout
        }
    }

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode => _isSimplifiedMode;

    public void EnterSimplifiedMode()
    {
        _isSimplifiedMode = true;
        CleanupAllParticles();
        EnableAnimations = false;
        EnableParticleEffects = false;
        _pulseTimer?.Stop();
        _pulseStoryboard?.Stop();
    }

    public void ExitSimplifiedMode()
    {
        _isSimplifiedMode = false;
        EnableAnimations = true;
        EnableParticleEffects = true;
        UpdateStatusIndicator();
    }

    #endregion

    #region Color Scheme Application

    private void ApplyEVEColorScheme()
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        
        if (BorderBrush is SolidColorBrush borderBrush)
        {
            borderBrush.Color = Color.FromArgb(100, colors.Primary.R, colors.Primary.G, colors.Primary.B);
        }

        if (Background is SolidColorBrush backgroundBrush)
        {
            backgroundBrush.Color = Color.FromArgb(30, colors.Primary.R, colors.Primary.G, colors.Primary.B);
        }

        if (Effect is DropShadowEffect shadow)
        {
            shadow.Color = Color.FromArgb(80, colors.Primary.R, colors.Primary.G, colors.Primary.B);
        }
    }

    private void ApplyHolographicIntensity()
    {
        var intensity = HolographicIntensity;
        
        if (Effect is DropShadowEffect shadow)
        {
            shadow.BlurRadius = 8 * intensity;
            shadow.ShadowDepth = 2 * intensity;
        }

        EnableAnimations = intensity > 0.3;
        EnableParticleEffects = intensity > 0.5;
    }

    #endregion
}

#region Supporting Classes and Enums

public enum AuthStatus
{
    Disconnected,
    Connecting,
    Authenticating,
    Connected,
    Error,
    Expired
}

public enum ConnectionQuality
{
    Unknown,
    Poor,
    Fair,
    Good,
    Excellent
}

public enum IndicatorEffect
{
    None,
    Steady,
    Pulse,
    FastPulse,
    SlowPulse,
    Blink
}

public class HoloAuthStatusEventArgs : EventArgs
{
    public AuthStatus Status { get; set; }
    public ConnectionQuality Quality { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
}

#endregion