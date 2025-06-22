// ==========================================================================
// HoloAuthStatus.cs - Holographic Authentication Status Indicators
// ==========================================================================
// Advanced authentication status display featuring holographic indicators,
// real-time status updates, and EVE-style visual feedback systems.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Holographic authentication status indicators with real-time updates and EVE styling
/// </summary>
public class HoloAuthStatus : Control, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloAuthStatus),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloAuthStatus),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty AuthenticationStatusProperty =
        DependencyProperty.Register(nameof(AuthenticationStatus), typeof(AuthenticationState), typeof(HoloAuthStatus),
            new PropertyMetadata(AuthenticationState.Disconnected, OnAuthenticationStatusChanged));

    public static readonly DependencyProperty ConnectionQualityProperty =
        DependencyProperty.Register(nameof(ConnectionQuality), typeof(ConnectionQuality), typeof(HoloAuthStatus),
            new PropertyMetadata(ConnectionQuality.Unknown, OnConnectionQualityChanged));

    public static readonly DependencyProperty LastUpdateTimeProperty =
        DependencyProperty.Register(nameof(LastUpdateTime), typeof(DateTime), typeof(HoloAuthStatus),
            new PropertyMetadata(DateTime.Now, OnLastUpdateTimeChanged));

    public static readonly DependencyProperty ShowDetailedInfoProperty =
        DependencyProperty.Register(nameof(ShowDetailedInfo), typeof(bool), typeof(HoloAuthStatus),
            new PropertyMetadata(true));

    public static readonly DependencyProperty EnablePulseAnimationProperty =
        DependencyProperty.Register(nameof(EnablePulseAnimation), typeof(bool), typeof(HoloAuthStatus),
            new PropertyMetadata(true, OnEnablePulseAnimationChanged));

    public static readonly DependencyProperty EnableStatusHistoryProperty =
        DependencyProperty.Register(nameof(EnableStatusHistory), typeof(bool), typeof(HoloAuthStatus),
            new PropertyMetadata(true));

    public static readonly DependencyProperty StatusMessageProperty =
        DependencyProperty.Register(nameof(StatusMessage), typeof(string), typeof(HoloAuthStatus),
            new PropertyMetadata("Ready"));

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

    public AuthenticationState AuthenticationStatus
    {
        get => (AuthenticationState)GetValue(AuthenticationStatusProperty);
        set => SetValue(AuthenticationStatusProperty, value);
    }

    public ConnectionQuality ConnectionQuality
    {
        get => (ConnectionQuality)GetValue(ConnectionQualityProperty);
        set => SetValue(ConnectionQualityProperty, value);
    }

    public DateTime LastUpdateTime
    {
        get => (DateTime)GetValue(LastUpdateTimeProperty);
        set => SetValue(LastUpdateTimeProperty, value);
    }

    public bool ShowDetailedInfo
    {
        get => (bool)GetValue(ShowDetailedInfoProperty);
        set => SetValue(ShowDetailedInfoProperty, value);
    }

    public bool EnablePulseAnimation
    {
        get => (bool)GetValue(EnablePulseAnimationProperty);
        set => SetValue(EnablePulseAnimationProperty, value);
    }

    public bool EnableStatusHistory
    {
        get => (bool)GetValue(EnableStatusHistoryProperty);
        set => SetValue(EnableStatusHistoryProperty, value);
    }

    public string StatusMessage
    {
        get => (string)GetValue(StatusMessageProperty);
        set => SetValue(StatusMessageProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloAuthStatusEventArgs> StatusClicked;
    public event EventHandler<HoloAuthStatusEventArgs> StatusChanged;
    public event EventHandler<HoloAuthStatusEventArgs> ConnectionQualityChanged;

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode { get; private set; }

    public void SwitchToFullMode()
    {
        IsInSimplifiedMode = false;
        EnablePulseAnimation = true;
        ShowDetailedInfo = true;
        UpdateStatusAppearance();
    }

    public void SwitchToSimplifiedMode()
    {
        IsInSimplifiedMode = true;
        EnablePulseAnimation = false;
        ShowDetailedInfo = false;
        UpdateStatusAppearance();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => !_disposed && IsLoaded;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        EnablePulseAnimation = settings.EnabledFeatures.HasFlag(AnimationFeatures.BasicGlow);
        
        UpdateStatusAppearance();
    }

    #endregion

    #region Fields

    private Grid _statusGrid;
    private Border _mainIndicator;
    private Ellipse _statusIndicator;
    private TextBlock _statusText;
    private TextBlock _detailText;
    private StackPanel _qualityIndicators;
    private Canvas _effectCanvas;
    private ProgressBar _activityIndicator;
    
    private readonly List<StatusHistoryItem> _statusHistory = new();
    private readonly List<StatusParticle> _particles = new();
    private DispatcherTimer _pulseTimer;
    private DispatcherTimer _particleTimer;
    private DispatcherTimer _updateTimer;
    private double _pulsePhase = 0;
    private double _particlePhase = 0;
    private readonly Random _random = new();
    private bool _disposed = false;
    private AuthenticationState _previousStatus;

    #endregion

    #region Constructor

    public HoloAuthStatus()
    {
        DefaultStyleKey = typeof(HoloAuthStatus);
        Width = 200;
        Height = 60;
        _previousStatus = AuthenticationStatus;
        InitializeStatus();
        SetupAnimations();
        
        // Register with animation intensity manager
        AnimationIntensityManager.Instance.RegisterTarget(this);
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Update authentication status with message
    /// </summary>
    public void UpdateStatus(AuthenticationState status, string message = null)
    {
        var previousStatus = AuthenticationStatus;
        AuthenticationStatus = status;
        StatusMessage = message ?? GetDefaultStatusMessage(status);
        LastUpdateTime = DateTime.Now;

        if (EnableStatusHistory)
        {
            AddToStatusHistory(status, StatusMessage);
        }

        StatusChanged?.Invoke(this, new HoloAuthStatusEventArgs 
        { 
            PreviousStatus = previousStatus,
            CurrentStatus = status,
            Message = StatusMessage,
            Timestamp = LastUpdateTime
        });
    }

    /// <summary>
    /// Update connection quality
    /// </summary>
    public void UpdateConnectionQuality(ConnectionQuality quality)
    {
        var previousQuality = ConnectionQuality;
        ConnectionQuality = quality;

        ConnectionQualityChanged?.Invoke(this, new HoloAuthStatusEventArgs 
        { 
            PreviousQuality = previousQuality,
            CurrentQuality = quality,
            Timestamp = DateTime.Now
        });
    }

    /// <summary>
    /// Flash status indicator for attention
    /// </summary>
    public void FlashStatus(Color color, TimeSpan duration)
    {
        if (IsInSimplifiedMode || _statusIndicator == null) return;

        var flashAnimation = new ColorAnimation
        {
            To = color,
            Duration = TimeSpan.FromMilliseconds(100),
            AutoReverse = true,
            RepeatBehavior = new RepeatBehavior(duration.TotalMilliseconds / 200)
        };

        if (_statusIndicator.Fill is SolidColorBrush brush)
        {
            brush.BeginAnimation(SolidColorBrush.ColorProperty, flashAnimation);
        }
    }

    /// <summary>
    /// Get status history
    /// </summary>
    public IReadOnlyList<StatusHistoryItem> GetStatusHistory()
    {
        return _statusHistory.AsReadOnly();
    }

    /// <summary>
    /// Clear status history
    /// </summary>
    public void ClearStatusHistory()
    {
        _statusHistory.Clear();
    }

    #endregion

    #region Private Methods

    private void InitializeStatus()
    {
        Template = CreateStatusTemplate();
        UpdateStatusAppearance();
    }

    private ControlTemplate CreateStatusTemplate()
    {
        var template = new ControlTemplate(typeof(HoloAuthStatus));

        // Main status grid
        var statusGrid = new FrameworkElementFactory(typeof(Grid));
        statusGrid.Name = "PART_StatusGrid";

        // Column definitions
        var indicatorColumn = new FrameworkElementFactory(typeof(ColumnDefinition));
        indicatorColumn.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);
        var textColumn = new FrameworkElementFactory(typeof(ColumnDefinition));
        textColumn.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
        var qualityColumn = new FrameworkElementFactory(typeof(ColumnDefinition));
        qualityColumn.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);

        statusGrid.AppendChild(indicatorColumn);
        statusGrid.AppendChild(textColumn);
        statusGrid.AppendChild(qualityColumn);

        // Main indicator border
        var mainIndicator = new FrameworkElementFactory(typeof(Border));
        mainIndicator.Name = "PART_MainIndicator";
        mainIndicator.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
        mainIndicator.SetValue(Border.BorderThicknessProperty, new Thickness(1));
        mainIndicator.SetValue(Border.PaddingProperty, new Thickness(8, 4, 8, 4));
        mainIndicator.SetValue(Grid.ColumnSpanProperty, 3);
        mainIndicator.SetValue(Button.CursorProperty, Cursors.Hand);

        // Content grid inside main indicator
        var contentGrid = new FrameworkElementFactory(typeof(Grid));
        contentGrid.AppendChild(indicatorColumn);
        contentGrid.AppendChild(textColumn);
        contentGrid.AppendChild(qualityColumn);

        // Status indicator circle
        var statusIndicator = new FrameworkElementFactory(typeof(Ellipse));
        statusIndicator.Name = "PART_StatusIndicator";
        statusIndicator.SetValue(Ellipse.WidthProperty, 12.0);
        statusIndicator.SetValue(Ellipse.HeightProperty, 12.0);
        statusIndicator.SetValue(Ellipse.VerticalAlignmentProperty, VerticalAlignment.Center);
        statusIndicator.SetValue(Ellipse.MarginProperty, new Thickness(0, 0, 8, 0));
        statusIndicator.SetValue(Grid.ColumnProperty, 0);

        // Text stack panel
        var textStack = new FrameworkElementFactory(typeof(StackPanel));
        textStack.SetValue(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Center);
        textStack.SetValue(Grid.ColumnProperty, 1);

        // Status text
        var statusText = new FrameworkElementFactory(typeof(TextBlock));
        statusText.Name = "PART_StatusText";
        statusText.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        statusText.SetValue(TextBlock.FontSizeProperty, 12.0);

        // Detail text
        var detailText = new FrameworkElementFactory(typeof(TextBlock));
        detailText.Name = "PART_DetailText";
        detailText.SetValue(TextBlock.FontSizeProperty, 10.0);
        detailText.SetValue(TextBlock.OpacityProperty, 0.8);

        textStack.AppendChild(statusText);
        textStack.AppendChild(detailText);

        // Quality indicators
        var qualityIndicators = new FrameworkElementFactory(typeof(StackPanel));
        qualityIndicators.Name = "PART_QualityIndicators";
        qualityIndicators.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        qualityIndicators.SetValue(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Center);
        qualityIndicators.SetValue(Grid.ColumnProperty, 2);

        // Activity indicator (hidden by default)
        var activityIndicator = new FrameworkElementFactory(typeof(ProgressBar));
        activityIndicator.Name = "PART_ActivityIndicator";
        activityIndicator.SetValue(ProgressBar.WidthProperty, 60.0);
        activityIndicator.SetValue(ProgressBar.HeightProperty, 4.0);
        activityIndicator.SetValue(ProgressBar.IsIndeterminateProperty, true);
        activityIndicator.SetValue(ProgressBar.VisibilityProperty, Visibility.Collapsed);
        activityIndicator.SetValue(ProgressBar.MarginProperty, new Thickness(8, 0, 0, 0));
        activityIndicator.SetValue(Grid.ColumnProperty, 2);

        // Effect canvas
        var effectCanvas = new FrameworkElementFactory(typeof(Canvas));
        effectCanvas.Name = "PART_EffectCanvas";
        effectCanvas.SetValue(Canvas.IsHitTestVisibleProperty, false);
        effectCanvas.SetValue(Canvas.ClipToBoundsProperty, true);
        effectCanvas.SetValue(Grid.ColumnSpanProperty, 3);

        // Assembly
        contentGrid.AppendChild(statusIndicator);
        contentGrid.AppendChild(textStack);
        contentGrid.AppendChild(qualityIndicators);
        contentGrid.AppendChild(activityIndicator);
        mainIndicator.AppendChild(contentGrid);

        statusGrid.AppendChild(mainIndicator);
        statusGrid.AppendChild(effectCanvas);

        template.VisualTree = statusGrid;
        return template;
    }

    private void SetupAnimations()
    {
        // Pulse animation timer
        _pulseTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
        };
        _pulseTimer.Tick += OnPulseTick;

        // Particle animation timer
        _particleTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50) // 20 FPS
        };
        _particleTimer.Tick += OnParticleTick;

        // Update timer for status refreshing
        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _updateTimer.Tick += OnUpdateTick;
    }

    private void OnPulseTick(object sender, EventArgs e)
    {
        if (!EnablePulseAnimation || IsInSimplifiedMode) return;

        _pulsePhase += 0.05;
        if (_pulsePhase > Math.PI * 2)
            _pulsePhase = 0;

        UpdatePulseEffects();
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        if (IsInSimplifiedMode || _effectCanvas == null) return;

        _particlePhase += 0.1;
        if (_particlePhase > Math.PI * 2)
            _particlePhase = 0;

        UpdateStatusParticles();
        
        if (AuthenticationStatus == AuthenticationState.Authenticating)
        {
            SpawnStatusParticles();
        }
    }

    private void OnUpdateTick(object sender, EventArgs e)
    {
        // Update detail text with relative time
        if (_detailText != null && ShowDetailedInfo)
        {
            var timeSinceUpdate = DateTime.Now - LastUpdateTime;
            var timeText = timeSinceUpdate.TotalMinutes < 1 ? 
                "Just now" : 
                $"{(int)timeSinceUpdate.TotalMinutes}m ago";
                
            _detailText.Text = $"{GetQualityText(ConnectionQuality)} • {timeText}";
        }
    }

    private void UpdatePulseEffects()
    {
        if (_statusIndicator?.Effect is DropShadowEffect effect)
        {
            var intensity = GetPulseIntensityForStatus(AuthenticationStatus);
            var pulseIntensity = intensity + (Math.Sin(_pulsePhase * GetPulseSpeedForStatus(AuthenticationStatus)) * 0.3);
            effect.Opacity = HolographicIntensity * pulseIntensity;
        }

        if (_mainIndicator?.Effect is DropShadowEffect borderEffect)
        {
            var glowIntensity = 0.4 + (Math.Sin(_pulsePhase) * 0.2);
            borderEffect.Opacity = HolographicIntensity * glowIntensity;
        }
    }

    private void UpdateStatusParticles()
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var particle = _particles[i];
            
            // Update particle position
            particle.X += particle.VelocityX;
            particle.Y += particle.VelocityY;
            particle.Life -= 0.02;

            // Update visual position
            Canvas.SetLeft(particle.Visual, particle.X);
            Canvas.SetTop(particle.Visual, particle.Y);
            
            // Update opacity
            particle.Visual.Opacity = Math.Max(0, particle.Life) * HolographicIntensity;

            // Remove dead particles
            if (particle.Life <= 0 || particle.X > ActualWidth + 10)
            {
                _effectCanvas.Children.Remove(particle.Visual);
                _particles.RemoveAt(i);
            }
        }
    }

    private void SpawnStatusParticles()
    {
        if (_particles.Count >= 8) return; // Limit particle count

        if (_random.NextDouble() < 0.2) // 20% chance to spawn
        {
            var particle = CreateStatusParticle();
            _particles.Add(particle);
            _effectCanvas.Children.Add(particle.Visual);
        }
    }

    private StatusParticle CreateStatusParticle()
    {
        var color = GetStatusColor(AuthenticationStatus);
        var size = 1 + _random.NextDouble() * 2;
        
        var ellipse = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromArgb(120, color.R, color.G, color.B))
        };

        var particle = new StatusParticle
        {
            Visual = ellipse,
            X = 0,
            Y = _random.NextDouble() * ActualHeight,
            VelocityX = 1 + _random.NextDouble() * 2,
            VelocityY = (_random.NextDouble() - 0.5) * 1,
            Life = 1.0
        };

        Canvas.SetLeft(ellipse, particle.X);
        Canvas.SetTop(ellipse, particle.Y);

        return particle;
    }

    private void UpdateStatusAppearance()
    {
        if (Template == null) return;

        Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            ApplyTemplate();
            
            _statusGrid = GetTemplateChild("PART_StatusGrid") as Grid;
            _mainIndicator = GetTemplateChild("PART_MainIndicator") as Border;
            _statusIndicator = GetTemplateChild("PART_StatusIndicator") as Ellipse;
            _statusText = GetTemplateChild("PART_StatusText") as TextBlock;
            _detailText = GetTemplateChild("PART_DetailText") as TextBlock;
            _qualityIndicators = GetTemplateChild("PART_QualityIndicators") as StackPanel;
            _effectCanvas = GetTemplateChild("PART_EffectCanvas") as Canvas;
            _activityIndicator = GetTemplateChild("PART_ActivityIndicator") as ProgressBar;

            UpdateColors();
            UpdateEffects();
            UpdateStatusDisplay();
            UpdateQualityIndicators();
            SetupEventHandlers();
        }), DispatcherPriority.Render);
    }

    private void UpdateColors()
    {
        var statusColor = GetStatusColor(AuthenticationStatus);
        var schemeColor = GetEVEColor(EVEColorScheme);

        // Main indicator colors
        if (_mainIndicator != null)
        {
            var backgroundBrush = new LinearGradientBrush();
            backgroundBrush.StartPoint = new Point(0, 0);
            backgroundBrush.EndPoint = new Point(1, 0);
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(100, 0, 20, 40), 0.0));
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(80, 0, 15, 30), 1.0));

            _mainIndicator.Background = backgroundBrush;
            _mainIndicator.BorderBrush = new SolidColorBrush(Color.FromArgb(
                120, schemeColor.R, schemeColor.G, schemeColor.B));
        }

        // Status indicator color
        if (_statusIndicator != null)
        {
            _statusIndicator.Fill = new SolidColorBrush(statusColor);
            _statusIndicator.Stroke = new SolidColorBrush(Color.FromArgb(
                180, statusColor.R, statusColor.G, statusColor.B));
            _statusIndicator.StrokeThickness = 1;
        }

        // Text colors
        if (_statusText != null)
        {
            _statusText.Foreground = new SolidColorBrush(Colors.White);
        }

        if (_detailText != null)
        {
            _detailText.Foreground = new SolidColorBrush(Color.FromArgb(180, 180, 180, 180));
        }
    }

    private void UpdateEffects()
    {
        var statusColor = GetStatusColor(AuthenticationStatus);
        var schemeColor = GetEVEColor(EVEColorScheme);

        if (!IsInSimplifiedMode)
        {
            // Status indicator glow
            if (_statusIndicator != null)
            {
                _statusIndicator.Effect = new DropShadowEffect
                {
                    Color = statusColor,
                    BlurRadius = 6 * HolographicIntensity,
                    ShadowDepth = 0,
                    Opacity = 0.8 * HolographicIntensity
                };
            }

            // Main indicator glow
            if (_mainIndicator != null)
            {
                _mainIndicator.Effect = new DropShadowEffect
                {
                    Color = schemeColor,
                    BlurRadius = 4 * HolographicIntensity,
                    ShadowDepth = 0,
                    Opacity = 0.4 * HolographicIntensity
                };
            }
        }
    }

    private void UpdateStatusDisplay()
    {
        if (_statusText != null)
        {
            _statusText.Text = GetStatusText(AuthenticationStatus);
        }

        if (_detailText != null && ShowDetailedInfo)
        {
            var timeSinceUpdate = DateTime.Now - LastUpdateTime;
            var timeText = timeSinceUpdate.TotalMinutes < 1 ? 
                "Just now" : 
                $"{(int)timeSinceUpdate.TotalMinutes}m ago";
                
            _detailText.Text = $"{GetQualityText(ConnectionQuality)} • {timeText}";
            _detailText.Visibility = Visibility.Visible;
        }
        else if (_detailText != null)
        {
            _detailText.Visibility = Visibility.Collapsed;
        }

        // Show activity indicator for authenticating state
        if (_activityIndicator != null)
        {
            _activityIndicator.Visibility = AuthenticationStatus == AuthenticationState.Authenticating ? 
                Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void UpdateQualityIndicators()
    {
        if (_qualityIndicators == null) return;

        _qualityIndicators.Children.Clear();

        if (!ShowDetailedInfo) return;

        var qualityLevel = (int)ConnectionQuality;
        var maxBars = 4;

        for (int i = 0; i < maxBars; i++)
        {
            var bar = new Rectangle
            {
                Width = 3,
                Height = 4 + (i * 2),
                Margin = new Thickness(1, 0, 1, 0),
                VerticalAlignment = VerticalAlignment.Bottom
            };

            if (i < qualityLevel)
            {
                var color = GetQualityColor(ConnectionQuality);
                bar.Fill = new SolidColorBrush(color);
            }
            else
            {
                bar.Fill = new SolidColorBrush(Color.FromArgb(60, 128, 128, 128));
            }

            _qualityIndicators.Children.Add(bar);
        }
    }

    private void SetupEventHandlers()
    {
        if (_mainIndicator != null)
        {
            _mainIndicator.MouseLeftButtonUp -= OnStatusClicked;
            _mainIndicator.MouseLeftButtonUp += OnStatusClicked;
        }
    }

    private void AddToStatusHistory(AuthenticationState status, string message)
    {
        _statusHistory.Add(new StatusHistoryItem
        {
            Status = status,
            Message = message,
            Timestamp = DateTime.Now
        });

        // Keep only last 50 entries
        while (_statusHistory.Count > 50)
        {
            _statusHistory.RemoveAt(0);
        }
    }

    private Color GetStatusColor(AuthenticationState status)
    {
        return status switch
        {
            AuthenticationState.Connected => Color.FromRgb(50, 205, 50),
            AuthenticationState.Authenticating => Color.FromRgb(255, 215, 0),
            AuthenticationState.Disconnected => Color.FromRgb(128, 128, 128),
            AuthenticationState.Error => Color.FromRgb(220, 20, 60),
            AuthenticationState.Expired => Color.FromRgb(255, 140, 0),
            _ => Color.FromRgb(128, 128, 128)
        };
    }

    private Color GetQualityColor(ConnectionQuality quality)
    {
        return quality switch
        {
            ConnectionQuality.Excellent => Color.FromRgb(50, 205, 50),
            ConnectionQuality.Good => Color.FromRgb(173, 255, 47),
            ConnectionQuality.Fair => Color.FromRgb(255, 215, 0),
            ConnectionQuality.Poor => Color.FromRgb(255, 140, 0),
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

    private string GetStatusText(AuthenticationState status)
    {
        return status switch
        {
            AuthenticationState.Connected => "Connected",
            AuthenticationState.Authenticating => "Authenticating",
            AuthenticationState.Disconnected => "Disconnected",
            AuthenticationState.Error => "Error",
            AuthenticationState.Expired => "Expired",
            _ => "Unknown"
        };
    }

    private string GetQualityText(ConnectionQuality quality)
    {
        return quality switch
        {
            ConnectionQuality.Excellent => "Excellent",
            ConnectionQuality.Good => "Good",
            ConnectionQuality.Fair => "Fair",
            ConnectionQuality.Poor => "Poor",
            _ => "Unknown"
        };
    }

    private string GetDefaultStatusMessage(AuthenticationState status)
    {
        return status switch
        {
            AuthenticationState.Connected => "EVE Online connection established",
            AuthenticationState.Authenticating => "Authenticating with EVE Online...",
            AuthenticationState.Disconnected => "Not connected to EVE Online",
            AuthenticationState.Error => "Authentication error occurred",
            AuthenticationState.Expired => "Authentication token expired",
            _ => "Status unknown"
        };
    }

    private double GetPulseIntensityForStatus(AuthenticationState status)
    {
        return status switch
        {
            AuthenticationState.Connected => 0.6,
            AuthenticationState.Authenticating => 0.8,
            AuthenticationState.Error => 0.9,
            AuthenticationState.Expired => 0.7,
            _ => 0.3
        };
    }

    private double GetPulseSpeedForStatus(AuthenticationState status)
    {
        return status switch
        {
            AuthenticationState.Authenticating => 3.0,
            AuthenticationState.Error => 4.0,
            AuthenticationState.Expired => 2.0,
            _ => 1.0
        };
    }

    private void OnStatusClicked(object sender, MouseButtonEventArgs e)
    {
        StatusClicked?.Invoke(this, new HoloAuthStatusEventArgs 
        { 
            CurrentStatus = AuthenticationStatus,
            CurrentQuality = ConnectionQuality,
            Message = StatusMessage,
            Timestamp = DateTime.Now
        });
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnablePulseAnimation && !IsInSimplifiedMode)
            _pulseTimer.Start();
            
        if (!IsInSimplifiedMode)
            _particleTimer.Start();
            
        _updateTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _pulseTimer?.Stop();
        _particleTimer?.Stop();
        _updateTimer?.Stop();
        
        // Clean up particles
        _particles.Clear();
        _effectCanvas?.Children.Clear();
        
        // Unregister from animation intensity manager
        AnimationIntensityManager.Instance.UnregisterTarget(this);
        
        _disposed = true;
    }

    #endregion

    #region Event Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAuthStatus status)
            status.UpdateStatusAppearance();
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAuthStatus status)
            status.UpdateStatusAppearance();
    }

    private static void OnAuthenticationStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAuthStatus status)
        {
            status._previousStatus = (AuthenticationState)e.OldValue;
            status.UpdateStatusAppearance();
        }
    }

    private static void OnConnectionQualityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAuthStatus status)
            status.UpdateQualityIndicators();
    }

    private static void OnLastUpdateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAuthStatus status)
            status.UpdateStatusDisplay();
    }

    private static void OnEnablePulseAnimationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAuthStatus status)
        {
            if ((bool)e.NewValue && !status.IsInSimplifiedMode)
                status._pulseTimer.Start();
            else
                status._pulseTimer.Stop();
        }
    }

    #endregion
}

/// <summary>
/// Status particle for visual effects
/// </summary>
internal class StatusParticle
{
    public FrameworkElement Visual { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
}

/// <summary>
/// Authentication states
/// </summary>
public enum AuthenticationState
{
    Disconnected,
    Authenticating,
    Connected,
    Error,
    Expired
}

/// <summary>
/// Connection quality levels
/// </summary>
public enum ConnectionQuality
{
    Unknown = 0,
    Poor = 1,
    Fair = 2,
    Good = 3,
    Excellent = 4
}

/// <summary>
/// Status history item
/// </summary>
public class StatusHistoryItem
{
    public AuthenticationState Status { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Event args for authentication status events
/// </summary>
public class HoloAuthStatusEventArgs : EventArgs
{
    public AuthenticationState PreviousStatus { get; set; }
    public AuthenticationState CurrentStatus { get; set; }
    public ConnectionQuality PreviousQuality { get; set; }
    public ConnectionQuality CurrentQuality { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
}