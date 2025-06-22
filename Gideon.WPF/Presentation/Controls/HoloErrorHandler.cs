// ==========================================================================
// HoloErrorHandler.cs - Holographic Error Handling with Animated Feedback
// ==========================================================================
// Advanced error handling system featuring holographic error displays,
// animated feedback, contextual actions, and EVE-style error visualization.
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Holographic error handling system with animated feedback and contextual actions
/// </summary>
public class HoloErrorHandler : Control, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloErrorHandler),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloErrorHandler),
            new PropertyMetadata(EVEColorScheme.CrimsonRed, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty CurrentErrorProperty =
        DependencyProperty.Register(nameof(CurrentError), typeof(HoloError), typeof(HoloErrorHandler),
            new PropertyMetadata(null, OnCurrentErrorChanged));

    public static readonly DependencyProperty ErrorDisplayModeProperty =
        DependencyProperty.Register(nameof(ErrorDisplayMode), typeof(ErrorDisplayMode), typeof(HoloErrorHandler),
            new PropertyMetadata(ErrorDisplayMode.Overlay));

    public static readonly DependencyProperty AutoDismissTimeoutProperty =
        DependencyProperty.Register(nameof(AutoDismissTimeout), typeof(TimeSpan), typeof(HoloErrorHandler),
            new PropertyMetadata(TimeSpan.FromSeconds(5)));

    public static readonly DependencyProperty EnableErrorAnimationsProperty =
        DependencyProperty.Register(nameof(EnableErrorAnimations), typeof(bool), typeof(HoloErrorHandler),
            new PropertyMetadata(true, OnEnableErrorAnimationsChanged));

    public static readonly DependencyProperty EnableErrorSoundsProperty =
        DependencyProperty.Register(nameof(EnableErrorSounds), typeof(bool), typeof(HoloErrorHandler),
            new PropertyMetadata(true));

    public static readonly DependencyProperty ShowErrorHistoryProperty =
        DependencyProperty.Register(nameof(ShowErrorHistory), typeof(bool), typeof(HoloErrorHandler),
            new PropertyMetadata(false));

    public static readonly DependencyProperty MaxErrorHistoryProperty =
        DependencyProperty.Register(nameof(MaxErrorHistory), typeof(int), typeof(HoloErrorHandler),
            new PropertyMetadata(10));

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

    public HoloError CurrentError
    {
        get => (HoloError)GetValue(CurrentErrorProperty);
        set => SetValue(CurrentErrorProperty, value);
    }

    public ErrorDisplayMode ErrorDisplayMode
    {
        get => (ErrorDisplayMode)GetValue(ErrorDisplayModeProperty);
        set => SetValue(ErrorDisplayModeProperty, value);
    }

    public TimeSpan AutoDismissTimeout
    {
        get => (TimeSpan)GetValue(AutoDismissTimeoutProperty);
        set => SetValue(AutoDismissTimeoutProperty, value);
    }

    public bool EnableErrorAnimations
    {
        get => (bool)GetValue(EnableErrorAnimationsProperty);
        set => SetValue(EnableErrorAnimationsProperty, value);
    }

    public bool EnableErrorSounds
    {
        get => (bool)GetValue(EnableErrorSoundsProperty);
        set => SetValue(EnableErrorSoundsProperty, value);
    }

    public bool ShowErrorHistory
    {
        get => (bool)GetValue(ShowErrorHistoryProperty);
        set => SetValue(ShowErrorHistoryProperty, value);
    }

    public int MaxErrorHistory
    {
        get => (int)GetValue(MaxErrorHistoryProperty);
        set => SetValue(MaxErrorHistoryProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloErrorEventArgs> ErrorShown;
    public event EventHandler<HoloErrorEventArgs> ErrorDismissed;
    public event EventHandler<HoloErrorEventArgs> ErrorActionTriggered;
    public event EventHandler<HoloErrorEventArgs> ErrorRetryRequested;

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode { get; private set; }

    public void SwitchToFullMode()
    {
        IsInSimplifiedMode = false;
        EnableErrorAnimations = true;
        UpdateErrorAppearance();
    }

    public void SwitchToSimplifiedMode()
    {
        IsInSimplifiedMode = true;
        EnableErrorAnimations = false;
        UpdateErrorAppearance();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => !_disposed && IsLoaded;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        EnableErrorAnimations = settings.EnabledFeatures.HasFlag(AnimationFeatures.ComplexTransitions);
        
        UpdateErrorAppearance();
    }

    #endregion

    #region Fields

    private Grid _errorGrid;
    private Border _errorContainer;
    private Border _errorIcon;
    private Path _iconPath;
    private TextBlock _errorTitle;
    private TextBlock _errorMessage;
    private TextBlock _errorCode;
    private StackPanel _actionsPanel;
    private Button _retryButton;
    private Button _dismissButton;
    private Button _detailsButton;
    private Canvas _particleCanvas;
    private Canvas _glitchCanvas;
    
    private readonly List<HoloError> _errorHistory = new();
    private readonly List<ErrorParticle> _particles = new();
    private readonly List<GlitchElement> _glitchElements = new();
    private DispatcherTimer _particleTimer;
    private DispatcherTimer _glitchTimer;
    private DispatcherTimer _dismissTimer;
    private double _particlePhase = 0;
    private double _glitchPhase = 0;
    private readonly Random _random = new();
    private bool _disposed = false;
    private bool _isVisible = false;

    #endregion

    #region Constructor

    public HoloErrorHandler()
    {
        DefaultStyleKey = typeof(HoloErrorHandler);
        Visibility = Visibility.Collapsed;
        InitializeErrorHandler();
        SetupAnimations();
        
        // Register with animation intensity manager
        AnimationIntensityManager.Instance.RegisterTarget(this);
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Show an error with specified severity
    /// </summary>
    public async Task ShowErrorAsync(string title, string message, ErrorSeverity severity = ErrorSeverity.Error, 
        string errorCode = null, List<HoloErrorAction> actions = null)
    {
        var error = new HoloError
        {
            Title = title,
            Message = message,
            Severity = severity,
            ErrorCode = errorCode,
            Actions = actions ?? new List<HoloErrorAction>(),
            Timestamp = DateTime.Now
        };

        await ShowErrorAsync(error);
    }

    /// <summary>
    /// Show a HoloError object
    /// </summary>
    public async Task ShowErrorAsync(HoloError error)
    {
        if (error == null) return;

        CurrentError = error;
        AddToErrorHistory(error);

        if (EnableErrorAnimations && !IsInSimplifiedMode)
        {
            await AnimateErrorInAsync();
        }
        else
        {
            Visibility = Visibility.Visible;
            _isVisible = true;
        }

        ErrorShown?.Invoke(this, new HoloErrorEventArgs { Error = error });

        // Start auto-dismiss timer if applicable
        if (error.AutoDismiss && AutoDismissTimeout > TimeSpan.Zero)
        {
            StartDismissTimer();
        }
    }

    /// <summary>
    /// Dismiss the current error
    /// </summary>
    public async Task DismissErrorAsync()
    {
        if (CurrentError == null || !_isVisible) return;

        StopDismissTimer();

        if (EnableErrorAnimations && !IsInSimplifiedMode)
        {
            await AnimateErrorOutAsync();
        }
        else
        {
            Visibility = Visibility.Collapsed;
            _isVisible = false;
        }

        var error = CurrentError;
        CurrentError = null;

        ErrorDismissed?.Invoke(this, new HoloErrorEventArgs { Error = error });
    }

    /// <summary>
    /// Show a quick error notification
    /// </summary>
    public async Task ShowQuickErrorAsync(string message, ErrorSeverity severity = ErrorSeverity.Warning)
    {
        await ShowErrorAsync("Error", message, severity, actions: new List<HoloErrorAction>
        {
            new HoloErrorAction { Text = "OK", ActionType = ErrorActionType.Dismiss }
        });
    }

    /// <summary>
    /// Show an error with retry option
    /// </summary>
    public async Task ShowRetryErrorAsync(string title, string message, Action retryAction)
    {
        var actions = new List<HoloErrorAction>
        {
            new HoloErrorAction 
            { 
                Text = "Retry", 
                ActionType = ErrorActionType.Retry,
                Action = retryAction
            },
            new HoloErrorAction { Text = "Cancel", ActionType = ErrorActionType.Dismiss }
        };

        await ShowErrorAsync(title, message, ErrorSeverity.Error, actions: actions);
    }

    /// <summary>
    /// Get error history
    /// </summary>
    public IReadOnlyList<HoloError> GetErrorHistory()
    {
        return _errorHistory.AsReadOnly();
    }

    /// <summary>
    /// Clear error history
    /// </summary>
    public void ClearErrorHistory()
    {
        _errorHistory.Clear();
    }

    #endregion

    #region Private Methods

    private void InitializeErrorHandler()
    {
        Template = CreateErrorTemplate();
        UpdateErrorAppearance();
    }

    private ControlTemplate CreateErrorTemplate()
    {
        var template = new ControlTemplate(typeof(HoloErrorHandler));

        // Main error grid
        var errorGrid = new FrameworkElementFactory(typeof(Grid));
        errorGrid.Name = "PART_ErrorGrid";

        // Error container
        var errorContainer = new FrameworkElementFactory(typeof(Border));
        errorContainer.Name = "PART_ErrorContainer";
        errorContainer.SetValue(Border.CornerRadiusProperty, new CornerRadius(12));
        errorContainer.SetValue(Border.BorderThicknessProperty, new Thickness(2));
        errorContainer.SetValue(Border.PaddingProperty, new Thickness(20));
        errorContainer.SetValue(Border.MinWidthProperty, 300.0);
        errorContainer.SetValue(Border.MaxWidthProperty, 500.0);
        errorContainer.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        errorContainer.SetValue(Border.VerticalAlignmentProperty, VerticalAlignment.Center);

        // Content grid
        var contentGrid = new FrameworkElementFactory(typeof(Grid));

        // Row definitions
        var headerRow = new FrameworkElementFactory(typeof(RowDefinition));
        headerRow.SetValue(RowDefinition.HeightProperty, GridLength.Auto);
        var messageRow = new FrameworkElementFactory(typeof(RowDefinition));
        messageRow.SetValue(RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Star));
        var actionsRow = new FrameworkElementFactory(typeof(RowDefinition));
        actionsRow.SetValue(RowDefinition.HeightProperty, GridLength.Auto);

        contentGrid.AppendChild(headerRow);
        contentGrid.AppendChild(messageRow);
        contentGrid.AppendChild(actionsRow);

        // Header panel
        var headerPanel = new FrameworkElementFactory(typeof(StackPanel));
        headerPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        headerPanel.SetValue(StackPanel.MarginProperty, new Thickness(0, 0, 0, 16));
        headerPanel.SetValue(Grid.RowProperty, 0);

        // Error icon
        var errorIcon = new FrameworkElementFactory(typeof(Border));
        errorIcon.Name = "PART_ErrorIcon";
        errorIcon.SetValue(Border.WidthProperty, 32.0);
        errorIcon.SetValue(Border.HeightProperty, 32.0);
        errorIcon.SetValue(Border.CornerRadiusProperty, new CornerRadius(16));
        errorIcon.SetValue(Border.MarginProperty, new Thickness(0, 0, 12, 0));

        var iconPath = new FrameworkElementFactory(typeof(Path));
        iconPath.Name = "PART_IconPath";
        iconPath.SetValue(Path.WidthProperty, 20.0);
        iconPath.SetValue(Path.HeightProperty, 20.0);
        iconPath.SetValue(Path.StretchProperty, Stretch.Uniform);
        iconPath.SetValue(Path.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        iconPath.SetValue(Path.VerticalAlignmentProperty, VerticalAlignment.Center);

        errorIcon.AppendChild(iconPath);

        // Title and code panel
        var titlePanel = new FrameworkElementFactory(typeof(StackPanel));
        titlePanel.SetValue(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Center);

        // Error title
        var errorTitle = new FrameworkElementFactory(typeof(TextBlock));
        errorTitle.Name = "PART_ErrorTitle";
        errorTitle.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        errorTitle.SetValue(TextBlock.FontSizeProperty, 16.0);

        // Error code
        var errorCode = new FrameworkElementFactory(typeof(TextBlock));
        errorCode.Name = "PART_ErrorCode";
        errorCode.SetValue(TextBlock.FontSizeProperty, 11.0);
        errorCode.SetValue(TextBlock.OpacityProperty, 0.7);

        titlePanel.AppendChild(errorTitle);
        titlePanel.AppendChild(errorCode);

        headerPanel.AppendChild(errorIcon);
        headerPanel.AppendChild(titlePanel);

        // Error message
        var errorMessage = new FrameworkElementFactory(typeof(TextBlock));
        errorMessage.Name = "PART_ErrorMessage";
        errorMessage.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
        errorMessage.SetValue(TextBlock.FontSizeProperty, 13.0);
        errorMessage.SetValue(TextBlock.MarginProperty, new Thickness(0, 0, 0, 16));
        errorMessage.SetValue(Grid.RowProperty, 1);

        // Actions panel
        var actionsPanel = new FrameworkElementFactory(typeof(StackPanel));
        actionsPanel.Name = "PART_ActionsPanel";
        actionsPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        actionsPanel.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Right);
        actionsPanel.SetValue(Grid.RowProperty, 2);

        // Assembly content
        contentGrid.AppendChild(headerPanel);
        contentGrid.AppendChild(errorMessage);
        contentGrid.AppendChild(actionsPanel);
        errorContainer.AppendChild(contentGrid);

        // Particle canvas for effects
        var particleCanvas = new FrameworkElementFactory(typeof(Canvas));
        particleCanvas.Name = "PART_ParticleCanvas";
        particleCanvas.SetValue(Canvas.IsHitTestVisibleProperty, false);
        particleCanvas.SetValue(Canvas.ClipToBoundsProperty, true);

        // Glitch canvas for error effects
        var glitchCanvas = new FrameworkElementFactory(typeof(Canvas));
        glitchCanvas.Name = "PART_GlitchCanvas";
        glitchCanvas.SetValue(Canvas.IsHitTestVisibleProperty, false);
        glitchCanvas.SetValue(Canvas.ClipToBoundsProperty, true);

        // Assembly
        errorGrid.AppendChild(errorContainer);
        errorGrid.AppendChild(particleCanvas);
        errorGrid.AppendChild(glitchCanvas);

        template.VisualTree = errorGrid;
        return template;
    }

    private void SetupAnimations()
    {
        // Particle animation timer
        _particleTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50) // 20 FPS
        };
        _particleTimer.Tick += OnParticleTick;

        // Glitch effect timer
        _glitchTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100) // 10 FPS
        };
        _glitchTimer.Tick += OnGlitchTick;

        // Auto-dismiss timer
        _dismissTimer = new DispatcherTimer();
        _dismissTimer.Tick += OnDismissTimerTick;
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        if (!EnableErrorAnimations || IsInSimplifiedMode || _particleCanvas == null) return;

        _particlePhase += 0.1;
        if (_particlePhase > Math.PI * 2)
            _particlePhase = 0;

        UpdateErrorParticles();
        
        if (_isVisible && CurrentError != null)
        {
            SpawnErrorParticles();
        }
    }

    private void OnGlitchTick(object sender, EventArgs e)
    {
        if (!EnableErrorAnimations || IsInSimplifiedMode || _glitchCanvas == null) return;

        _glitchPhase += 0.2;
        if (_glitchPhase > Math.PI * 2)
            _glitchPhase = 0;

        UpdateGlitchEffects();
        
        if (_isVisible && CurrentError?.Severity == ErrorSeverity.Critical)
        {
            SpawnGlitchElements();
        }
    }

    private void OnDismissTimerTick(object sender, EventArgs e)
    {
        _ = DismissErrorAsync();
    }

    private void UpdateErrorParticles()
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var particle = _particles[i];
            
            // Update particle position
            particle.X += particle.VelocityX;
            particle.Y += particle.VelocityY;
            particle.Life -= 0.02;

            // Add turbulence for error effect
            particle.VelocityX += (Math.Sin(particle.Phase) * 0.5);
            particle.VelocityY += (Math.Cos(particle.Phase) * 0.5);
            particle.Phase += 0.1;

            // Update visual position
            Canvas.SetLeft(particle.Visual, particle.X);
            Canvas.SetTop(particle.Visual, particle.Y);
            
            // Update opacity
            particle.Visual.Opacity = Math.Max(0, particle.Life) * HolographicIntensity;

            // Remove dead particles
            if (particle.Life <= 0 || particle.X < -50 || particle.X > ActualWidth + 50 ||
                particle.Y < -50 || particle.Y > ActualHeight + 50)
            {
                _particleCanvas.Children.Remove(particle.Visual);
                _particles.RemoveAt(i);
            }
        }
    }

    private void SpawnErrorParticles()
    {
        if (_particles.Count >= 20) return; // Limit particle count

        if (_random.NextDouble() < 0.3) // 30% chance to spawn
        {
            var particle = CreateErrorParticle();
            _particles.Add(particle);
            _particleCanvas.Children.Add(particle.Visual);
        }
    }

    private ErrorParticle CreateErrorParticle()
    {
        var color = GetSeverityColor(CurrentError?.Severity ?? ErrorSeverity.Error);
        var size = 2 + _random.NextDouble() * 3;
        
        var shape = _random.NextDouble() < 0.7 ? 
            CreateParticleEllipse(size, color) : 
            CreateParticleTriangle(size, color);

        var centerX = ActualWidth / 2;
        var centerY = ActualHeight / 2;
        var angle = _random.NextDouble() * Math.PI * 2;
        var radius = 50 + _random.NextDouble() * 100;

        var particle = new ErrorParticle
        {
            Visual = shape,
            X = centerX + Math.Cos(angle) * radius,
            Y = centerY + Math.Sin(angle) * radius,
            VelocityX = (Math.Cos(angle) * 2) + (_random.NextDouble() - 0.5) * 2,
            VelocityY = (Math.Sin(angle) * 2) + (_random.NextDouble() - 0.5) * 2,
            Life = 1.0,
            Phase = _random.NextDouble() * Math.PI * 2
        };

        Canvas.SetLeft(shape, particle.X);
        Canvas.SetTop(shape, particle.Y);

        return particle;
    }

    private FrameworkElement CreateParticleEllipse(double size, Color color)
    {
        return new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromArgb(150, color.R, color.G, color.B)),
            Stroke = new SolidColorBrush(Color.FromArgb(200, color.R, color.G, color.B)),
            StrokeThickness = 0.5
        };
    }

    private FrameworkElement CreateParticleTriangle(double size, Color color)
    {
        var polygon = new Polygon();
        polygon.Points.Add(new Point(size / 2, 0));
        polygon.Points.Add(new Point(0, size));
        polygon.Points.Add(new Point(size, size));
        
        polygon.Fill = new SolidColorBrush(Color.FromArgb(120, color.R, color.G, color.B));
        polygon.Stroke = new SolidColorBrush(Color.FromArgb(180, color.R, color.G, color.B));
        polygon.StrokeThickness = 0.5;
        
        return polygon;
    }

    private void UpdateGlitchEffects()
    {
        // Clear old glitch elements
        _glitchCanvas.Children.Clear();
        _glitchElements.Clear();

        if (CurrentError?.Severity != ErrorSeverity.Critical) return;

        // Create glitch lines
        for (int i = 0; i < 3; i++)
        {
            var glitchLine = new Rectangle
            {
                Width = ActualWidth,
                Height = 1 + _random.NextDouble() * 2,
                Fill = new SolidColorBrush(Color.FromArgb(100, 255, 0, 0)),
                Opacity = 0.3 + (_random.NextDouble() * 0.4)
            };

            var y = _random.NextDouble() * ActualHeight;
            Canvas.SetLeft(glitchLine, 0);
            Canvas.SetTop(glitchLine, y);

            _glitchCanvas.Children.Add(glitchLine);
        }
    }

    private void SpawnGlitchElements()
    {
        // Glitch elements are created in UpdateGlitchEffects
    }

    private async Task AnimateErrorInAsync()
    {
        _isVisible = true;
        Visibility = Visibility.Visible;
        
        // Initial state
        _errorContainer.Opacity = 0;
        _errorContainer.RenderTransform = new CompositeTransform
        {
            ScaleX = 0.8,
            ScaleY = 0.8,
            TranslateY = 50
        };

        // Animate in
        var fadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var scaleXAnimation = new DoubleAnimation
        {
            From = 0.8,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(400),
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
        };

        var scaleYAnimation = new DoubleAnimation
        {
            From = 0.8,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(400),
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
        };

        var translateAnimation = new DoubleAnimation
        {
            From = 50,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(400),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        _errorContainer.BeginAnimation(OpacityProperty, fadeIn);
        
        if (_errorContainer.RenderTransform is CompositeTransform transform)
        {
            transform.BeginAnimation(CompositeTransform.ScaleXProperty, scaleXAnimation);
            transform.BeginAnimation(CompositeTransform.ScaleYProperty, scaleYAnimation);
            transform.BeginAnimation(CompositeTransform.TranslateYProperty, translateAnimation);
        }

        await Task.Delay(400);
    }

    private async Task AnimateErrorOutAsync()
    {
        // Animate out
        var fadeOut = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(250),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        var scaleAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.9,
            Duration = TimeSpan.FromMilliseconds(250),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        var translateAnimation = new DoubleAnimation
        {
            From = 0,
            To = -30,
            Duration = TimeSpan.FromMilliseconds(250),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        _errorContainer.BeginAnimation(OpacityProperty, fadeOut);
        
        if (_errorContainer.RenderTransform is CompositeTransform transform)
        {
            transform.BeginAnimation(CompositeTransform.ScaleXProperty, scaleAnimation);
            transform.BeginAnimation(CompositeTransform.ScaleYProperty, scaleAnimation);
            transform.BeginAnimation(CompositeTransform.TranslateYProperty, translateAnimation);
        }

        await Task.Delay(250);
        
        Visibility = Visibility.Collapsed;
        _isVisible = false;
    }

    private void UpdateErrorAppearance()
    {
        if (Template == null) return;

        Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            ApplyTemplate();
            
            _errorGrid = GetTemplateChild("PART_ErrorGrid") as Grid;
            _errorContainer = GetTemplateChild("PART_ErrorContainer") as Border;
            _errorIcon = GetTemplateChild("PART_ErrorIcon") as Border;
            _iconPath = GetTemplateChild("PART_IconPath") as Path;
            _errorTitle = GetTemplateChild("PART_ErrorTitle") as TextBlock;
            _errorMessage = GetTemplateChild("PART_ErrorMessage") as TextBlock;
            _errorCode = GetTemplateChild("PART_ErrorCode") as TextBlock;
            _actionsPanel = GetTemplateChild("PART_ActionsPanel") as StackPanel;
            _particleCanvas = GetTemplateChild("PART_ParticleCanvas") as Canvas;
            _glitchCanvas = GetTemplateChild("PART_GlitchCanvas") as Canvas;

            UpdateColors();
            UpdateEffects();
            UpdateErrorDisplay();
        }), DispatcherPriority.Render);
    }

    private void UpdateColors()
    {
        if (CurrentError == null) return;

        var severityColor = GetSeverityColor(CurrentError.Severity);
        var schemeColor = GetEVEColor(EVEColorScheme);

        // Error container colors
        if (_errorContainer != null)
        {
            var backgroundBrush = new LinearGradientBrush();
            backgroundBrush.StartPoint = new Point(0, 0);
            backgroundBrush.EndPoint = new Point(1, 1);
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(220, 0, 0, 0), 0.0));
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(200, 20, 0, 0), 1.0));

            _errorContainer.Background = backgroundBrush;
            _errorContainer.BorderBrush = new SolidColorBrush(Color.FromArgb(
                200, severityColor.R, severityColor.G, severityColor.B));
        }

        // Error icon colors
        if (_errorIcon != null)
        {
            _errorIcon.Background = new SolidColorBrush(Color.FromArgb(
                100, severityColor.R, severityColor.G, severityColor.B));
            _errorIcon.BorderBrush = new SolidColorBrush(severityColor);
            _errorIcon.BorderThickness = new Thickness(2);
        }

        if (_iconPath != null)
        {
            _iconPath.Fill = new SolidColorBrush(severityColor);
        }

        // Text colors
        if (_errorTitle != null)
        {
            _errorTitle.Foreground = new SolidColorBrush(Colors.White);
        }

        if (_errorMessage != null)
        {
            _errorMessage.Foreground = new SolidColorBrush(Color.FromArgb(220, 220, 220, 220));
        }

        if (_errorCode != null)
        {
            _errorCode.Foreground = new SolidColorBrush(Color.FromArgb(160, 180, 180, 180));
        }
    }

    private void UpdateEffects()
    {
        if (CurrentError == null) return;

        var severityColor = GetSeverityColor(CurrentError.Severity);

        if (!IsInSimplifiedMode)
        {
            // Error container glow
            if (_errorContainer != null)
            {
                _errorContainer.Effect = new DropShadowEffect
                {
                    Color = severityColor,
                    BlurRadius = 20 * HolographicIntensity,
                    ShadowDepth = 0,
                    Opacity = 0.8 * HolographicIntensity
                };
            }

            // Error icon glow
            if (_errorIcon != null)
            {
                _errorIcon.Effect = new DropShadowEffect
                {
                    Color = severityColor,
                    BlurRadius = 8 * HolographicIntensity,
                    ShadowDepth = 0,
                    Opacity = 0.9 * HolographicIntensity
                };
            }
        }
    }

    private void UpdateErrorDisplay()
    {
        if (CurrentError == null) return;

        // Update icon
        if (_iconPath != null)
        {
            _iconPath.Data = GetSeverityIcon(CurrentError.Severity);
        }

        // Update texts
        if (_errorTitle != null)
        {
            _errorTitle.Text = CurrentError.Title ?? "Error";
        }

        if (_errorMessage != null)
        {
            _errorMessage.Text = CurrentError.Message ?? "An error occurred.";
        }

        if (_errorCode != null)
        {
            _errorCode.Text = string.IsNullOrEmpty(CurrentError.ErrorCode) ? 
                string.Empty : $"Error Code: {CurrentError.ErrorCode}";
            _errorCode.Visibility = string.IsNullOrEmpty(CurrentError.ErrorCode) ? 
                Visibility.Collapsed : Visibility.Visible;
        }

        // Update actions
        UpdateErrorActions();
    }

    private void UpdateErrorActions()
    {
        if (_actionsPanel == null) return;

        _actionsPanel.Children.Clear();

        if (CurrentError?.Actions == null || CurrentError.Actions.Count == 0)
        {
            // Default dismiss button
            var dismissButton = new HoloButton
            {
                Content = "OK",
                Width = 80,
                Height = 32,
                EVEColorScheme = EVEColorScheme,
                HolographicIntensity = HolographicIntensity,
                Margin = new Thickness(4)
            };
            dismissButton.Click += (s, e) => _ = DismissErrorAsync();
            _actionsPanel.Children.Add(dismissButton);
            return;
        }

        foreach (var action in CurrentError.Actions)
        {
            var button = new HoloButton
            {
                Content = action.Text,
                Width = 80,
                Height = 32,
                EVEColorScheme = GetActionColorScheme(action.ActionType),
                HolographicIntensity = HolographicIntensity,
                Margin = new Thickness(4),
                Tag = action
            };

            button.Click += OnActionButtonClick;
            _actionsPanel.Children.Add(button);
        }
    }

    private void AddToErrorHistory(HoloError error)
    {
        _errorHistory.Add(error);

        // Keep only the most recent errors
        while (_errorHistory.Count > MaxErrorHistory)
        {
            _errorHistory.RemoveAt(0);
        }
    }

    private void StartDismissTimer()
    {
        StopDismissTimer();
        _dismissTimer.Interval = AutoDismissTimeout;
        _dismissTimer.Start();
    }

    private void StopDismissTimer()
    {
        _dismissTimer.Stop();
    }

    private Color GetSeverityColor(ErrorSeverity severity)
    {
        return severity switch
        {
            ErrorSeverity.Info => Color.FromRgb(64, 224, 255),
            ErrorSeverity.Warning => Color.FromRgb(255, 215, 0),
            ErrorSeverity.Error => Color.FromRgb(220, 20, 60),
            ErrorSeverity.Critical => Color.FromRgb(255, 0, 0),
            _ => Color.FromRgb(220, 20, 60)
        };
    }

    private Geometry GetSeverityIcon(ErrorSeverity severity)
    {
        return severity switch
        {
            ErrorSeverity.Info => Geometry.Parse("M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M11,16.5L6.5,12L7.91,10.59L11,13.67L16.59,8.09L18,9.5L11,16.5Z"),
            ErrorSeverity.Warning => Geometry.Parse("M13,13H11V7H13M13,17H11V15H13M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z"),
            ErrorSeverity.Error => Geometry.Parse("M12,2C17.53,2 22,6.47 22,12C22,17.53 17.53,22 12,22C6.47,22 2,17.53 2,12C2,6.47 6.47,2 12,2M15.59,7L12,10.59L8.41,7L7,8.41L10.59,12L7,15.59L8.41,17L12,13.41L15.59,17L17,15.59L13.41,12L17,8.41L15.59,7Z"),
            ErrorSeverity.Critical => Geometry.Parse("M12,2L13.09,8.26L22,9L17,14L18.18,23L12,19.77L5.82,23L7,14L2,9L10.91,8.26L12,2Z"),
            _ => Geometry.Parse("M12,2C17.53,2 22,6.47 22,12C22,17.53 17.53,22 12,22C6.47,22 2,17.53 2,12C2,6.47 6.47,2 12,2M15.59,7L12,10.59L8.41,7L7,8.41L10.59,12L7,15.59L8.41,17L12,13.41L15.59,17L17,15.59L13.41,12L17,8.41L15.59,7Z")
        };
    }

    private EVEColorScheme GetActionColorScheme(ErrorActionType actionType)
    {
        return actionType switch
        {
            ErrorActionType.Retry => EVEColorScheme.ElectricBlue,
            ErrorActionType.Dismiss => EVEColorScheme.VoidPurple,
            ErrorActionType.Custom => EVEColorScheme.GoldAccent,
            _ => EVEColorScheme.ElectricBlue
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

    private void OnActionButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is HoloErrorAction action)
        {
            ErrorActionTriggered?.Invoke(this, new HoloErrorEventArgs 
            { 
                Error = CurrentError,
                Action = action
            });

            switch (action.ActionType)
            {
                case ErrorActionType.Dismiss:
                    _ = DismissErrorAsync();
                    break;
                case ErrorActionType.Retry:
                    ErrorRetryRequested?.Invoke(this, new HoloErrorEventArgs 
                    { 
                        Error = CurrentError,
                        Action = action
                    });
                    action.Action?.Invoke();
                    _ = DismissErrorAsync();
                    break;
                case ErrorActionType.Custom:
                    action.Action?.Invoke();
                    break;
            }
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableErrorAnimations && !IsInSimplifiedMode)
        {
            _particleTimer.Start();
            _glitchTimer.Start();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _particleTimer?.Stop();
        _glitchTimer?.Stop();
        _dismissTimer?.Stop();
        
        // Clean up particles and effects
        _particles.Clear();
        _glitchElements.Clear();
        _particleCanvas?.Children.Clear();
        _glitchCanvas?.Children.Clear();
        
        // Unregister from animation intensity manager
        AnimationIntensityManager.Instance.UnregisterTarget(this);
        
        _disposed = true;
    }

    #endregion

    #region Event Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloErrorHandler handler)
            handler.UpdateErrorAppearance();
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloErrorHandler handler)
            handler.UpdateErrorAppearance();
    }

    private static void OnCurrentErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloErrorHandler handler)
            handler.UpdateErrorAppearance();
    }

    private static void OnEnableErrorAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloErrorHandler handler)
        {
            if ((bool)e.NewValue && !handler.IsInSimplifiedMode)
            {
                handler._particleTimer.Start();
                handler._glitchTimer.Start();
            }
            else
            {
                handler._particleTimer.Stop();
                handler._glitchTimer.Stop();
            }
        }
    }

    #endregion
}

/// <summary>
/// Error particle for visual effects
/// </summary>
internal class ErrorParticle
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
/// Glitch element for critical errors
/// </summary>
internal class GlitchElement
{
    public FrameworkElement Visual { get; set; }
    public double Duration { get; set; }
    public double Intensity { get; set; }
}

/// <summary>
/// Holographic error object
/// </summary>
public class HoloError
{
    public string Title { get; set; }
    public string Message { get; set; }
    public string ErrorCode { get; set; }
    public ErrorSeverity Severity { get; set; } = ErrorSeverity.Error;
    public List<HoloErrorAction> Actions { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public bool AutoDismiss { get; set; } = false;
    public object Data { get; set; }
}

/// <summary>
/// Error action definition
/// </summary>
public class HoloErrorAction
{
    public string Text { get; set; }
    public ErrorActionType ActionType { get; set; } = ErrorActionType.Custom;
    public Action Action { get; set; }
    public object Tag { get; set; }
}

/// <summary>
/// Error severity levels
/// </summary>
public enum ErrorSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

/// <summary>
/// Error action types
/// </summary>
public enum ErrorActionType
{
    Dismiss,
    Retry,
    Custom
}

/// <summary>
/// Error display modes
/// </summary>
public enum ErrorDisplayMode
{
    Overlay,
    Inline,
    Toast,
    Modal
}

/// <summary>
/// Event args for error events
/// </summary>
public class HoloErrorEventArgs : EventArgs
{
    public HoloError Error { get; set; }
    public HoloErrorAction Action { get; set; }
}