// ==========================================================================
// HoloConstraintValidator.cs - Holographic Constraint Validation System
// ==========================================================================
// Advanced constraint validation featuring real-time fitting validation,
// animated constraint feedback, EVE-style warnings, and holographic error visualization.
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
/// Holographic constraint validation system with real-time fitting analysis and animated feedback
/// </summary>
public class HoloConstraintValidator : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloConstraintValidator),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloConstraintValidator),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty ShipFittingProperty =
        DependencyProperty.Register(nameof(ShipFitting), typeof(HoloShipFitting), typeof(HoloConstraintValidator),
            new PropertyMetadata(null, OnShipFittingChanged));

    public static readonly DependencyProperty ValidatorModeProperty =
        DependencyProperty.Register(nameof(ValidatorMode), typeof(ValidatorMode), typeof(HoloConstraintValidator),
            new PropertyMetadata(ValidatorMode.RealTime, OnValidatorModeChanged));

    public static readonly DependencyProperty ConstraintViolationsProperty =
        DependencyProperty.Register(nameof(ConstraintViolations), typeof(ObservableCollection<ConstraintViolation>), typeof(HoloConstraintValidator),
            new PropertyMetadata(null, OnConstraintViolationsChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloConstraintValidator),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloConstraintValidator),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowDetailedErrorsProperty =
        DependencyProperty.Register(nameof(ShowDetailedErrors), typeof(bool), typeof(HoloConstraintValidator),
            new PropertyMetadata(true, OnShowDetailedErrorsChanged));

    public static readonly DependencyProperty AutoFixSuggestionsProperty =
        DependencyProperty.Register(nameof(AutoFixSuggestions), typeof(bool), typeof(HoloConstraintValidator),
            new PropertyMetadata(true, OnAutoFixSuggestionsChanged));

    public static readonly DependencyProperty ValidationSeverityProperty =
        DependencyProperty.Register(nameof(ValidationSeverity), typeof(ValidationSeverity), typeof(HoloConstraintValidator),
            new PropertyMetadata(ValidationSeverity.All, OnValidationSeverityChanged));

    public static readonly DependencyProperty EnableSoundAlertsProperty =
        DependencyProperty.Register(nameof(EnableSoundAlerts), typeof(bool), typeof(HoloConstraintValidator),
            new PropertyMetadata(false, OnEnableSoundAlertsChanged));

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

    public HoloShipFitting ShipFitting
    {
        get => (HoloShipFitting)GetValue(ShipFittingProperty);
        set => SetValue(ShipFittingProperty, value);
    }

    public ValidatorMode ValidatorMode
    {
        get => (ValidatorMode)GetValue(ValidatorModeProperty);
        set => SetValue(ValidatorModeProperty, value);
    }

    public ObservableCollection<ConstraintViolation> ConstraintViolations
    {
        get => (ObservableCollection<ConstraintViolation>)GetValue(ConstraintViolationsProperty);
        set => SetValue(ConstraintViolationsProperty, value);
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

    public bool ShowDetailedErrors
    {
        get => (bool)GetValue(ShowDetailedErrorsProperty);
        set => SetValue(ShowDetailedErrorsProperty, value);
    }

    public bool AutoFixSuggestions
    {
        get => (bool)GetValue(AutoFixSuggestionsProperty);
        set => SetValue(AutoFixSuggestionsProperty, value);
    }

    public ValidationSeverity ValidationSeverity
    {
        get => (ValidationSeverity)GetValue(ValidationSeverityProperty);
        set => SetValue(ValidationSeverityProperty, value);
    }

    public bool EnableSoundAlerts
    {
        get => (bool)GetValue(EnableSoundAlertsProperty);
        set => SetValue(EnableSoundAlertsProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<ValidationCompleteEventArgs> ValidationComplete;
    public event EventHandler<ViolationDetectedEventArgs> ViolationDetected;
    public event EventHandler<ViolationResolvedEventArgs> ViolationResolved;
    public event EventHandler<AutoFixSuggestionEventArgs> AutoFixSuggestion;

    #endregion

    #region Private Fields

    private Grid _rootGrid;
    private Border _statusPanel;
    private ScrollViewer _violationsScrollViewer;
    private StackPanel _violationsPanel;
    private Canvas _particleCanvas;
    private ProgressBar _validationProgress;
    private TextBlock _statusText;
    private Path _statusIcon;
    private Button _validateButton;
    private Button _autoFixButton;
    private DispatcherTimer _validationTimer;
    private DispatcherTimer _animationTimer;
    private readonly List<UIElement> _particles = new();
    private readonly Dictionary<string, FrameworkElement> _violationVisuals = new();
    private ConstraintEngine _constraintEngine;
    private ValidationState _currentState;
    private readonly Random _random = new();

    #endregion

    #region Constructor

    public HoloConstraintValidator()
    {
        InitializeComponent();
        InitializeConstraintEngine();
        InitializeAnimationSystem();
        InitializeValidationSystem();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 600;
        Height = 400;
        Background = Brushes.Transparent;

        _rootGrid = new Grid();
        _rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Status
        _rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Violations

        CreateStatusPanel();
        CreateViolationsPanel();
        CreateParticleSystem();

        Content = _rootGrid;
    }

    private void CreateStatusPanel()
    {
        _statusPanel = new Border
        {
            Height = 80,
            Margin = new Thickness(10),
            Background = CreateHolographicBackground(0.3),
            BorderBrush = GetBrushForColorScheme(EVEColorScheme),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(8),
            Effect = CreateGlowEffect()
        };

        var statusGrid = new Grid();
        statusGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Icon
        statusGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Text/Progress
        statusGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Actions

        // Status icon
        _statusIcon = new Path
        {
            Width = 32,
            Height = 32,
            Margin = new Thickness(15),
            Fill = GetBrushForValidationState(ValidationState.Unknown),
            Data = GetGeometryForValidationState(ValidationState.Unknown),
            Effect = CreateGlowEffect(0.8)
        };

        // Status content
        var statusContent = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0)
        };

        _statusText = new TextBlock
        {
            Text = "Ready for validation",
            FontSize = 16,
            FontWeight = FontWeights.Medium,
            Foreground = GetBrushForValidationState(ValidationState.Unknown),
            Margin = new Thickness(0, 0, 0, 5)
        };

        _validationProgress = new ProgressBar
        {
            Height = 6,
            Background = CreateHolographicBackground(0.2),
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            Visibility = Visibility.Collapsed
        };

        statusContent.Children.Add(_statusText);
        statusContent.Children.Add(_validationProgress);

        // Action buttons
        var actionPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 15, 0)
        };

        _validateButton = new Button
        {
            Content = "Validate",
            Margin = new Thickness(5),
            Padding = new Thickness(15, 8),
            Background = CreateHolographicBackground(0.4),
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            BorderBrush = GetBrushForColorScheme(EVEColorScheme),
            BorderThickness = new Thickness(1),
            Effect = CreateGlowEffect(),
            Style = CreateHolographicButtonStyle()
        };

        _autoFixButton = new Button
        {
            Content = "Auto Fix",
            Margin = new Thickness(5),
            Padding = new Thickness(15, 8),
            Background = CreateHolographicBackground(0.4),
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            BorderBrush = GetBrushForColorScheme(EVEColorScheme),
            BorderThickness = new Thickness(1),
            Effect = CreateGlowEffect(),
            Style = CreateHolographicButtonStyle(),
            IsEnabled = false
        };

        _validateButton.Click += ValidateButton_Click;
        _autoFixButton.Click += AutoFixButton_Click;

        actionPanel.Children.Add(_validateButton);
        actionPanel.Children.Add(_autoFixButton);

        Grid.SetColumn(_statusIcon, 0);
        Grid.SetColumn(statusContent, 1);
        Grid.SetColumn(actionPanel, 2);

        statusGrid.Children.Add(_statusIcon);
        statusGrid.Children.Add(statusContent);
        statusGrid.Children.Add(actionPanel);

        _statusPanel.Child = statusGrid;
        Grid.SetRow(_statusPanel, 0);
        _rootGrid.Children.Add(_statusPanel);
    }

    private void CreateViolationsPanel()
    {
        _violationsScrollViewer = new ScrollViewer
        {
            Margin = new Thickness(10, 0, 10, 10),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = CreateHolographicBackground(0.1),
            BorderBrush = GetBrushForColorScheme(EVEColorScheme),
            BorderThickness = new Thickness(1),
            Effect = CreateGlowEffect()
        };

        _violationsPanel = new StackPanel
        {
            Margin = new Thickness(10)
        };

        _violationsScrollViewer.Content = _violationsPanel;
        Grid.SetRow(_violationsScrollViewer, 1);
        _rootGrid.Children.Add(_violationsScrollViewer);
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

    private void InitializeConstraintEngine()
    {
        _constraintEngine = new ConstraintEngine();
        
        if (ConstraintViolations == null)
        {
            ConstraintViolations = new ObservableCollection<ConstraintViolation>();
        }

        ConstraintViolations.CollectionChanged += ConstraintViolations_CollectionChanged;
    }

    private void InitializeAnimationSystem()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        _animationTimer.Tick += AnimationTimer_Tick;
    }

    private void InitializeValidationSystem()
    {
        if (ValidatorMode == ValidatorMode.RealTime)
        {
            _validationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _validationTimer.Tick += ValidationTimer_Tick;
        }

        _currentState = ValidationState.Unknown;
    }

    #endregion

    #region Validation System

    private void ConstraintViolations_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (ConstraintViolation violation in e.NewItems)
            {
                CreateViolationVisual(violation);
                ViolationDetected?.Invoke(this, new ViolationDetectedEventArgs(violation));
            }
        }

        if (e.OldItems != null)
        {
            foreach (ConstraintViolation violation in e.OldItems)
            {
                RemoveViolationVisual(violation);
                ViolationResolved?.Invoke(this, new ViolationResolvedEventArgs(violation));
            }
        }

        UpdateValidationState();
    }

    private void CreateViolationVisual(ConstraintViolation violation)
    {
        var violationCard = new Border
        {
            Margin = new Thickness(0, 5),
            Background = CreateViolationBackground(violation.Severity),
            BorderBrush = GetBrushForSeverity(violation.Severity),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(5),
            Effect = CreateGlowEffect(0.8),
            Tag = violation
        };

        var cardGrid = new Grid();
        cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Icon
        cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Content
        cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Actions

        // Severity icon
        var severityIcon = new Path
        {
            Width = 24,
            Height = 24,
            Margin = new Thickness(12),
            Fill = GetBrushForSeverity(violation.Severity),
            Data = GetGeometryForSeverity(violation.Severity),
            Effect = CreateGlowEffect(0.6)
        };

        // Violation content
        var contentPanel = new StackPanel
        {
            Margin = new Thickness(10),
            VerticalAlignment = VerticalAlignment.Center
        };

        var titleBlock = new TextBlock
        {
            Text = violation.Title,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = GetBrushForSeverity(violation.Severity),
            Margin = new Thickness(0, 0, 0, 3)
        };

        var descriptionBlock = new TextBlock
        {
            Text = violation.Description,
            FontSize = 12,
            Foreground = GetBrushForSeverity(violation.Severity),
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.9
        };

        contentPanel.Children.Add(titleBlock);
        contentPanel.Children.Add(descriptionBlock);

        if (ShowDetailedErrors && !string.IsNullOrEmpty(violation.Details))
        {
            var detailsBlock = new TextBlock
            {
                Text = violation.Details,
                FontSize = 10,
                Foreground = GetBrushForSeverity(violation.Severity),
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.7,
                Margin = new Thickness(0, 5, 0, 0)
            };
            contentPanel.Children.Add(detailsBlock);
        }

        // Action buttons
        var actionPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0)
        };

        if (AutoFixSuggestions && violation.CanAutoFix)
        {
            var fixButton = new Button
            {
                Content = "Fix",
                Margin = new Thickness(3),
                Padding = new Thickness(8, 4),
                Background = CreateHolographicBackground(0.3),
                Foreground = GetBrushForSeverity(violation.Severity),
                BorderBrush = GetBrushForSeverity(violation.Severity),
                BorderThickness = new Thickness(1),
                Tag = violation
            };

            fixButton.Click += (s, e) => ApplyAutoFix(violation);
            actionPanel.Children.Add(fixButton);
        }

        var dismissButton = new Button
        {
            Content = "×",
            Width = 24,
            Height = 24,
            Margin = new Thickness(3),
            Background = CreateHolographicBackground(0.2),
            Foreground = GetBrushForSeverity(violation.Severity),
            BorderBrush = GetBrushForSeverity(violation.Severity),
            BorderThickness = new Thickness(1),
            Tag = violation,
            ToolTip = "Dismiss"
        };

        dismissButton.Click += (s, e) => DismissViolation(violation);
        actionPanel.Children.Add(dismissButton);

        Grid.SetColumn(severityIcon, 0);
        Grid.SetColumn(contentPanel, 1);
        Grid.SetColumn(actionPanel, 2);

        cardGrid.Children.Add(severityIcon);
        cardGrid.Children.Add(contentPanel);
        cardGrid.Children.Add(actionPanel);

        violationCard.Child = cardGrid;

        _violationVisuals[violation.Id] = violationCard;
        _violationsPanel.Children.Add(violationCard);

        if (EnableAnimations)
        {
            AnimateViolationAppearance(violationCard);
        }

        if (EnableParticleEffects)
        {
            CreateViolationParticles(violation);
        }
    }

    private void RemoveViolationVisual(ConstraintViolation violation)
    {
        if (_violationVisuals.TryGetValue(violation.Id, out var visual))
        {
            if (EnableAnimations)
            {
                AnimateViolationDisappearance(visual, () =>
                {
                    _violationsPanel.Children.Remove(visual);
                    _violationVisuals.Remove(violation.Id);
                });
            }
            else
            {
                _violationsPanel.Children.Remove(visual);
                _violationVisuals.Remove(violation.Id);
            }
        }
    }

    private async Task ValidateAsync()
    {
        if (ShipFitting == null) return;

        StartValidationAnimation();
        _currentState = ValidationState.Validating;
        UpdateStatusDisplay();

        try
        {
            var violations = await _constraintEngine.ValidateAsync(ShipFitting, ValidationSeverity);
            
            // Clear existing violations
            ConstraintViolations.Clear();
            
            // Add new violations
            foreach (var violation in violations)
            {
                ConstraintViolations.Add(violation);
            }

            // Determine final state
            _currentState = violations.Any() ? 
                (violations.Any(v => v.Severity == ConstraintSeverity.Error || v.Severity == ConstraintSeverity.Critical) ? 
                    ValidationState.Invalid : ValidationState.Warning) : 
                ValidationState.Valid;

            ValidationComplete?.Invoke(this, new ValidationCompleteEventArgs(_currentState, violations));

            // Auto-fix suggestions
            if (AutoFixSuggestions && violations.Any(v => v.CanAutoFix))
            {
                var autoFixable = violations.Where(v => v.CanAutoFix).ToList();
                AutoFixSuggestion?.Invoke(this, new AutoFixSuggestionEventArgs(autoFixable));
                _autoFixButton.IsEnabled = true;
            }
            else
            {
                _autoFixButton.IsEnabled = false;
            }
        }
        catch (Exception ex)
        {
            _currentState = ValidationState.Error;
            // Handle validation error
        }
        finally
        {
            StopValidationAnimation();
            UpdateStatusDisplay();
        }
    }

    private async void ValidationTimer_Tick(object sender, EventArgs e)
    {
        if (ValidatorMode == ValidatorMode.RealTime && ShipFitting != null)
        {
            await ValidateAsync();
        }
    }

    private void UpdateValidationState()
    {
        if (!ConstraintViolations.Any())
        {
            _currentState = ValidationState.Valid;
        }
        else if (ConstraintViolations.Any(v => v.Severity == ConstraintSeverity.Error || v.Severity == ConstraintSeverity.Critical))
        {
            _currentState = ValidationState.Invalid;
        }
        else
        {
            _currentState = ValidationState.Warning;
        }

        UpdateStatusDisplay();
    }

    private void UpdateStatusDisplay()
    {
        _statusIcon.Fill = GetBrushForValidationState(_currentState);
        _statusIcon.Data = GetGeometryForValidationState(_currentState);
        _statusText.Foreground = GetBrushForValidationState(_currentState);

        _statusText.Text = _currentState switch
        {
            ValidationState.Valid => "Fitting is valid",
            ValidationState.Warning => $"Fitting has {ConstraintViolations.Count} warning(s)",
            ValidationState.Invalid => $"Fitting has {ConstraintViolations.Count} error(s)",
            ValidationState.Validating => "Validating fitting...",
            ValidationState.Error => "Validation error occurred",
            _ => "Ready for validation"
        };

        if (EnableAnimations)
        {
            AnimateStatusChange();
        }
    }

    #endregion

    #region Auto-Fix System

    private async void ApplyAutoFix(ConstraintViolation violation)
    {
        if (!violation.CanAutoFix || violation.AutoFixAction == null) return;

        try
        {
            var result = await violation.AutoFixAction.ExecuteAsync(ShipFitting);
            
            if (result.Success)
            {
                ConstraintViolations.Remove(violation);
                
                if (EnableParticleEffects)
                {
                    CreateFixParticles();
                }

                // Re-validate after fix
                if (ValidatorMode == ValidatorMode.RealTime)
                {
                    await Task.Delay(100);
                    await ValidateAsync();
                }
            }
        }
        catch (Exception ex)
        {
            // Handle auto-fix error
        }
    }

    private async void AutoFixButton_Click(object sender, RoutedEventArgs e)
    {
        var autoFixableViolations = ConstraintViolations.Where(v => v.CanAutoFix).ToList();
        
        foreach (var violation in autoFixableViolations)
        {
            await ApplyAutoFix(violation);
            await Task.Delay(50); // Small delay between fixes
        }
    }

    private void DismissViolation(ConstraintViolation violation)
    {
        ConstraintViolations.Remove(violation);
    }

    #endregion

    #region Animation System

    private void AnimationTimer_Tick(object sender, EventArgs e)
    {
        UpdateParticles();
    }

    private void UpdateParticles()
    {
        foreach (var particle in _particles.ToList())
        {
            if (particle.Opacity <= 0.1)
            {
                _particles.Remove(particle);
                _particleCanvas.Children.Remove(particle);
            }
        }
    }

    private void StartValidationAnimation()
    {
        _validationProgress.Visibility = Visibility.Visible;
        
        if (EnableAnimations)
        {
            var progressAnimation = new DoubleAnimation
            {
                From = 0,
                To = 100,
                Duration = TimeSpan.FromMilliseconds(3000),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            _validationProgress.BeginAnimation(ProgressBar.ValueProperty, progressAnimation);

            // Pulsing status panel
            var pulseAnimation = new DoubleAnimation
            {
                From = 0.8,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(600),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            _statusPanel.BeginAnimation(OpacityProperty, pulseAnimation);
        }

        if (EnableParticleEffects)
        {
            CreateValidationParticles();
        }
    }

    private void StopValidationAnimation()
    {
        _validationProgress.Visibility = Visibility.Collapsed;
        _validationProgress.BeginAnimation(ProgressBar.ValueProperty, null);
        _statusPanel.BeginAnimation(OpacityProperty, null);
        _statusPanel.Opacity = 1.0;
    }

    private void AnimateStatusChange()
    {
        // Flash effect for status change
        var flashAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.5,
            Duration = TimeSpan.FromMilliseconds(200),
            AutoReverse = true,
            RepeatBehavior = new RepeatBehavior(2)
        };

        _statusIcon.BeginAnimation(OpacityProperty, flashAnimation);
    }

    private void AnimateViolationAppearance(FrameworkElement visual)
    {
        visual.Opacity = 0;
        visual.RenderTransform = new TranslateTransform(0, -20);

        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400));
        var slideIn = new DoubleAnimation(-20, 0, TimeSpan.FromMilliseconds(400))
        {
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
        };

        visual.BeginAnimation(OpacityProperty, fadeIn);
        ((TranslateTransform)visual.RenderTransform).BeginAnimation(TranslateTransform.YProperty, slideIn);
    }

    private void AnimateViolationDisappearance(FrameworkElement visual, Action onComplete)
    {
        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
        var slideOut = new DoubleAnimation(0, 20, TimeSpan.FromMilliseconds(300));

        fadeOut.Completed += (s, e) => onComplete?.Invoke();

        visual.BeginAnimation(OpacityProperty, fadeOut);
        if (visual.RenderTransform is TranslateTransform translate)
        {
            translate.BeginAnimation(TranslateTransform.YProperty, slideOut);
        }
    }

    #endregion

    #region Particle Effects

    private void CreateValidationParticles()
    {
        for (int i = 0; i < 12; i++)
        {
            Task.Delay(i * 200).ContinueWith(t =>
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

    private void CreateViolationParticles(ConstraintViolation violation)
    {
        if (!EnableParticleEffects) return;

        var particleCount = violation.Severity switch
        {
            ConstraintSeverity.Critical => 15,
            ConstraintSeverity.Error => 10,
            ConstraintSeverity.Warning => 6,
            _ => 3
        };

        for (int i = 0; i < particleCount; i++)
        {
            CreateViolationParticle(violation.Severity);
        }
    }

    private void CreateFixParticles()
    {
        if (!EnableParticleEffects) return;

        for (int i = 0; i < 20; i++)
        {
            CreateSuccessParticle();
        }
    }

    private void CreateScanParticle()
    {
        var particle = new Ellipse
        {
            Width = 3,
            Height = 3,
            Fill = GetBrushForColorScheme(EVEColorScheme),
            Opacity = 0.8,
            Effect = CreateGlowEffect(0.5)
        };

        var startX = _random.NextDouble() * ActualWidth;
        var startY = ActualHeight;
        var endX = _statusPanel.ActualWidth / 2;
        var endY = _statusPanel.ActualHeight / 2;

        Canvas.SetLeft(particle, startX);
        Canvas.SetTop(particle, startY);
        Canvas.SetZIndex(particle, 1000);

        _particles.Add(particle);
        _particleCanvas.Children.Add(particle);

        // Animate movement toward status panel
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

        Task.Delay(1500).ContinueWith(t =>
        {
            Dispatcher.BeginInvoke(() =>
            {
                _particles.Remove(particle);
                _particleCanvas.Children.Remove(particle);
            });
        });
    }

    private void CreateViolationParticle(ConstraintSeverity severity)
    {
        var particle = new Rectangle
        {
            Width = _random.NextDouble() * 8 + 3,
            Height = _random.NextDouble() * 8 + 3,
            Fill = GetBrushForSeverity(severity),
            Opacity = 0.7,
            Effect = CreateGlowEffect(0.4)
        };

        var startX = _random.NextDouble() * ActualWidth;
        var startY = _random.NextDouble() * ActualHeight;
        var angle = _random.NextDouble() * 2 * Math.PI;
        var distance = _random.NextDouble() * 100 + 50;

        var endX = startX + distance * Math.Cos(angle);
        var endY = startY + distance * Math.Sin(angle);

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
            Duration = TimeSpan.FromMilliseconds(1000),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var moveY = new DoubleAnimation
        {
            From = startY,
            To = endY,
            Duration = TimeSpan.FromMilliseconds(1000),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeOut = new DoubleAnimation
        {
            From = 0.7,
            To = 0,
            BeginTime = TimeSpan.FromMilliseconds(600),
            Duration = TimeSpan.FromMilliseconds(400)
        };

        particle.BeginAnimation(Canvas.LeftProperty, moveX);
        particle.BeginAnimation(Canvas.TopProperty, moveY);
        particle.BeginAnimation(OpacityProperty, fadeOut);

        Task.Delay(1000).ContinueWith(t =>
        {
            Dispatcher.BeginInvoke(() =>
            {
                _particles.Remove(particle);
                _particleCanvas.Children.Remove(particle);
            });
        });
    }

    private void CreateSuccessParticle()
    {
        var particle = new Ellipse
        {
            Width = _random.NextDouble() * 6 + 4,
            Height = _random.NextDouble() * 6 + 4,
            Fill = new SolidColorBrush(Color.FromRgb(0, 255, 127)), // Green
            Opacity = 0.9,
            Effect = CreateGlowEffect(0.6)
        };

        var centerX = ActualWidth / 2;
        var centerY = ActualHeight / 2;
        var angle = _random.NextDouble() * 2 * Math.PI;
        var startDistance = 20;
        var endDistance = 150;

        var startX = centerX + startDistance * Math.Cos(angle);
        var startY = centerY + startDistance * Math.Sin(angle);
        var endX = centerX + endDistance * Math.Cos(angle);
        var endY = centerY + endDistance * Math.Sin(angle);

        Canvas.SetLeft(particle, startX);
        Canvas.SetTop(particle, startY);
        Canvas.SetZIndex(particle, 1001);

        _particles.Add(particle);
        _particleCanvas.Children.Add(particle);

        // Animate radial expansion
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
            From = 0.9,
            To = 0,
            BeginTime = TimeSpan.FromMilliseconds(400),
            Duration = TimeSpan.FromMilliseconds(400)
        };

        particle.BeginAnimation(Canvas.LeftProperty, moveX);
        particle.BeginAnimation(Canvas.TopProperty, moveY);
        particle.BeginAnimation(OpacityProperty, fadeOut);

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

    private Brush CreateViolationBackground(ConstraintSeverity severity)
    {
        var color = GetColorForSeverity(severity);
        var brush = new LinearGradientBrush();
        brush.StartPoint = new Point(0, 0);
        brush.EndPoint = new Point(1, 1);
        
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(20, color.R, color.G, color.B), 0));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(10, color.R, color.G, color.B), 0.5));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(20, color.R, color.G, color.B), 1));

        return brush;
    }

    private Brush GetBrushForValidationState(ValidationState state)
    {
        return state switch
        {
            ValidationState.Valid => new SolidColorBrush(Color.FromRgb(0, 255, 127)),      // Green
            ValidationState.Warning => new SolidColorBrush(Color.FromRgb(255, 191, 0)),   // Amber
            ValidationState.Invalid => new SolidColorBrush(Color.FromRgb(220, 20, 60)),    // Red
            ValidationState.Validating => GetBrushForColorScheme(EVEColorScheme),         // Theme
            ValidationState.Error => new SolidColorBrush(Color.FromRgb(128, 0, 0)),       // Dark Red
            _ => new SolidColorBrush(Color.FromRgb(128, 128, 128))                        // Gray
        };
    }

    private Brush GetBrushForSeverity(ConstraintSeverity severity)
    {
        return severity switch
        {
            ConstraintSeverity.Critical => new SolidColorBrush(Color.FromRgb(128, 0, 0)),     // Dark Red
            ConstraintSeverity.Error => new SolidColorBrush(Color.FromRgb(220, 20, 60)),      // Red
            ConstraintSeverity.Warning => new SolidColorBrush(Color.FromRgb(255, 191, 0)),    // Amber
            ConstraintSeverity.Info => new SolidColorBrush(Color.FromRgb(0, 191, 255)),       // Blue
            _ => new SolidColorBrush(Color.FromRgb(128, 128, 128))                           // Gray
        };
    }

    private Color GetColorForSeverity(ConstraintSeverity severity)
    {
        return severity switch
        {
            ConstraintSeverity.Critical => Color.FromRgb(128, 0, 0),
            ConstraintSeverity.Error => Color.FromRgb(220, 20, 60),
            ConstraintSeverity.Warning => Color.FromRgb(255, 191, 0),
            ConstraintSeverity.Info => Color.FromRgb(0, 191, 255),
            _ => Color.FromRgb(128, 128, 128)
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

    private Geometry GetGeometryForValidationState(ValidationState state)
    {
        return state switch
        {
            ValidationState.Valid => Geometry.Parse("M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M11,16.5L6.5,12L7.91,10.59L11,13.67L16.59,8.09L18,9.5L11,16.5Z"),
            ValidationState.Warning => Geometry.Parse("M13,14H11V10H13M13,18H11V16H13M1,21H23L12,2L1,21Z"),
            ValidationState.Invalid => Geometry.Parse("M12,2C17.53,2 22,6.47 22,12C22,17.53 17.53,22 12,22C6.47,22 2,17.53 2,12C2,6.47 6.47,2 12,2M15.59,7L12,10.59L8.41,7L7,8.41L10.59,12L7,15.59L8.41,17L12,13.41L15.59,17L17,15.59L13.41,12L17,8.41L15.59,7Z"),
            ValidationState.Validating => Geometry.Parse("M12,4V2A10,10 0 0,1 22,12H20C20,7.58 16.42,4 12,4Z"),
            ValidationState.Error => Geometry.Parse("M11,15H13V17H11V15M11,7H13V13H11V7M12,2C6.47,2 2,6.5 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4A8,8 0 0,1 20,12A8,8 0 0,1 12,20Z"),
            _ => Geometry.Parse("M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2Z")
        };
    }

    private Geometry GetGeometryForSeverity(ConstraintSeverity severity)
    {
        return severity switch
        {
            ConstraintSeverity.Critical => Geometry.Parse("M12,2L1,21H23M12,6L19.53,19H4.47M11,10V14H13V10M11,16V18H13V16"),
            ConstraintSeverity.Error => Geometry.Parse("M12,2C17.53,2 22,6.47 22,12C22,17.53 17.53,22 12,22C6.47,22 2,17.53 2,12C2,6.47 6.47,2 12,2M15.59,7L12,10.59L8.41,7L7,8.41L10.59,12L7,15.59L8.41,17L12,13.41L15.59,17L17,15.59L13.41,12L17,8.41L15.59,7Z"),
            ConstraintSeverity.Warning => Geometry.Parse("M13,14H11V10H13M13,18H11V16H13M1,21H23L12,2L1,21Z"),
            ConstraintSeverity.Info => Geometry.Parse("M11,9H13V7H11M12,20C7.59,20 4,16.41 4,12C4,7.59 7.59,4 12,4C16.41,4 20,7.59 20,12C20,16.41 16.41,20 12,20M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M11,17H13V11H11V17Z"),
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

    private Style CreateHolographicButtonStyle()
    {
        var style = new Style(typeof(Button));
        style.Setters.Add(new Setter(ForegroundProperty, GetBrushForColorScheme(EVEColorScheme)));
        style.Setters.Add(new Setter(FontFamilyProperty, new FontFamily("Segoe UI")));
        style.Setters.Add(new Setter(FontWeightProperty, FontWeights.Medium));
        style.Setters.Add(new Setter(CursorProperty, Cursors.Hand));
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

        if (ValidatorMode == ValidatorMode.RealTime)
        {
            _validationTimer?.Start();
        }

        if (ShipFitting != null)
        {
            Task.Run(ValidateAsync);
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        _validationTimer?.Stop();
    }

    private async void ValidateButton_Click(object sender, RoutedEventArgs e)
    {
        await ValidateAsync();
    }

    #endregion

    #region Dependency Property Callbacks

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Update holographic effects intensity
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Update color scheme
    }

    private static void OnShipFittingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloConstraintValidator validator && validator.ValidatorMode == ValidatorMode.RealTime)
        {
            Task.Run(validator.ValidateAsync);
        }
    }

    private static void OnValidatorModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloConstraintValidator validator)
        {
            if ((ValidatorMode)e.NewValue == ValidatorMode.RealTime)
            {
                validator._validationTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(500)
                };
                validator._validationTimer.Tick += validator.ValidationTimer_Tick;
                validator._validationTimer.Start();
            }
            else
            {
                validator._validationTimer?.Stop();
                validator._validationTimer = null;
            }
        }
    }

    private static void OnConstraintViolationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloConstraintValidator validator)
        {
            if (e.OldValue is ObservableCollection<ConstraintViolation> oldViolations)
            {
                oldViolations.CollectionChanged -= validator.ConstraintViolations_CollectionChanged;
            }

            if (e.NewValue is ObservableCollection<ConstraintViolation> newViolations)
            {
                newViolations.CollectionChanged += validator.ConstraintViolations_CollectionChanged;
            }
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloConstraintValidator validator)
        {
            if ((bool)e.NewValue)
                validator._animationTimer.Start();
            else
                validator._animationTimer.Stop();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloConstraintValidator validator && !(bool)e.NewValue)
        {
            foreach (var particle in validator._particles.ToList())
            {
                validator._particles.Remove(particle);
                validator._particleCanvas.Children.Remove(particle);
            }
        }
    }

    private static void OnShowDetailedErrorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Update error detail display
    }

    private static void OnAutoFixSuggestionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Update auto-fix button visibility
    }

    private static void OnValidationSeverityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloConstraintValidator validator && validator.ValidatorMode == ValidatorMode.RealTime)
        {
            Task.Run(validator.ValidateAsync);
        }
    }

    private static void OnEnableSoundAlertsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Update sound alert settings
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
    /// Forces immediate validation
    /// </summary>
    public async Task ValidateNowAsync()
    {
        await ValidateAsync();
    }

    /// <summary>
    /// Clears all current violations
    /// </summary>
    public void ClearViolations()
    {
        ConstraintViolations.Clear();
        _currentState = ValidationState.Unknown;
        UpdateStatusDisplay();
    }

    /// <summary>
    /// Gets violations by severity
    /// </summary>
    public IEnumerable<ConstraintViolation> GetViolationsBySeverity(ConstraintSeverity severity)
    {
        return ConstraintViolations.Where(v => v.Severity == severity);
    }

    /// <summary>
    /// Applies all available auto-fixes
    /// </summary>
    public async Task ApplyAllAutoFixesAsync()
    {
        var autoFixableViolations = ConstraintViolations.Where(v => v.CanAutoFix).ToList();
        
        foreach (var violation in autoFixableViolations)
        {
            await ApplyAutoFix(violation);
        }
    }

    #endregion
}

#region Supporting Types

/// <summary>
/// Validation modes
/// </summary>
public enum ValidatorMode
{
    Manual,
    RealTime,
    OnChange
}

/// <summary>
/// Validation states
/// </summary>
public enum ValidationState
{
    Unknown,
    Valid,
    Warning,
    Invalid,
    Validating,
    Error
}

/// <summary>
/// Validation severity levels
/// </summary>
[Flags]
public enum ValidationSeverity
{
    Info = 1,
    Warning = 2,
    Error = 4,
    Critical = 8,
    All = Info | Warning | Error | Critical
}

/// <summary>
/// Constraint violation severity
/// </summary>
public enum ConstraintSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

/// <summary>
/// Constraint violation representation
/// </summary>
public class ConstraintViolation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public ConstraintSeverity Severity { get; set; }
    public string ConstraintType { get; set; } = string.Empty;
    public string AffectedComponent { get; set; } = string.Empty;
    public bool CanAutoFix { get; set; }
    public IAutoFixAction AutoFixAction { get; set; }
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// Auto-fix action interface
/// </summary>
public interface IAutoFixAction
{
    Task<AutoFixResult> ExecuteAsync(HoloShipFitting fitting);
    string GetDescription();
}

/// <summary>
/// Auto-fix result
/// </summary>
public class AutoFixResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Changes { get; set; } = new();
}

/// <summary>
/// Constraint validation engine
/// </summary>
public class ConstraintEngine
{
    public async Task<List<ConstraintViolation>> ValidateAsync(HoloShipFitting fitting, ValidationSeverity severity)
    {
        await Task.Delay(200); // Simulate validation time

        var violations = new List<ConstraintViolation>();

        // Power grid validation
        if (severity.HasFlag(ValidationSeverity.Error))
        {
            var powerUsage = CalculatePowerUsage(fitting);
            var powerAvailable = GetAvailablePower(fitting);
            
            if (powerUsage > powerAvailable)
            {
                violations.Add(new ConstraintViolation
                {
                    Title = "Insufficient Power Grid",
                    Description = $"Fitting requires {powerUsage:N0} MW but only {powerAvailable:N0} MW available",
                    Details = $"Excess usage: {powerUsage - powerAvailable:N0} MW",
                    Severity = ConstraintSeverity.Error,
                    ConstraintType = "PowerGrid",
                    CanAutoFix = true,
                    AutoFixAction = new PowerGridAutoFix()
                });
            }
        }

        // CPU validation
        if (severity.HasFlag(ValidationSeverity.Error))
        {
            var cpuUsage = CalculateCpuUsage(fitting);
            var cpuAvailable = GetAvailableCpu(fitting);
            
            if (cpuUsage > cpuAvailable)
            {
                violations.Add(new ConstraintViolation
                {
                    Title = "Insufficient CPU",
                    Description = $"Fitting requires {cpuUsage:N0} tf but only {cpuAvailable:N0} tf available",
                    Details = $"Excess usage: {cpuUsage - cpuAvailable:N0} tf",
                    Severity = ConstraintSeverity.Error,
                    ConstraintType = "CPU",
                    CanAutoFix = true,
                    AutoFixAction = new CpuAutoFix()
                });
            }
        }

        // Calibration validation (for rigs)
        if (severity.HasFlag(ValidationSeverity.Warning))
        {
            var calibrationUsage = CalculateCalibrationUsage(fitting);
            var calibrationAvailable = GetAvailableCalibration(fitting);
            
            if (calibrationUsage > calibrationAvailable)
            {
                violations.Add(new ConstraintViolation
                {
                    Title = "Insufficient Calibration",
                    Description = $"Rigs require {calibrationUsage:N0} but only {calibrationAvailable:N0} available",
                    Severity = ConstraintSeverity.Error,
                    ConstraintType = "Calibration",
                    CanAutoFix = false
                });
            }
        }

        // Skill requirements validation
        if (severity.HasFlag(ValidationSeverity.Warning))
        {
            var missingSkills = GetMissingSkills(fitting);
            if (missingSkills.Any())
            {
                violations.Add(new ConstraintViolation
                {
                    Title = "Missing Required Skills",
                    Description = $"Fitting requires {missingSkills.Count} additional skill(s)",
                    Details = string.Join(", ", missingSkills),
                    Severity = ConstraintSeverity.Warning,
                    ConstraintType = "Skills",
                    CanAutoFix = false
                });
            }
        }

        return violations;
    }

    private double CalculatePowerUsage(HoloShipFitting fitting)
    {
        return new Random().NextDouble() * 1000 + 500; // Simplified
    }

    private double GetAvailablePower(HoloShipFitting fitting)
    {
        return 800; // Simplified
    }

    private double CalculateCpuUsage(HoloShipFitting fitting)
    {
        return new Random().NextDouble() * 400 + 200; // Simplified
    }

    private double GetAvailableCpu(HoloShipFitting fitting)
    {
        return 350; // Simplified
    }

    private double CalculateCalibrationUsage(HoloShipFitting fitting)
    {
        return fitting.Rigs.Sum(r => 100); // Simplified
    }

    private double GetAvailableCalibration(HoloShipFitting fitting)
    {
        return 400; // Simplified
    }

    private List<string> GetMissingSkills(HoloShipFitting fitting)
    {
        return new List<string>(); // Simplified
    }
}

/// <summary>
/// Power grid auto-fix implementation
/// </summary>
public class PowerGridAutoFix : IAutoFixAction
{
    public async Task<AutoFixResult> ExecuteAsync(HoloShipFitting fitting)
    {
        await Task.Delay(100);
        
        return new AutoFixResult
        {
            Success = true,
            Message = "Installed power diagnostic system",
            Changes = new List<string> { "Added Power Diagnostic System I" }
        };
    }

    public string GetDescription()
    {
        return "Install power diagnostic system to increase power grid";
    }
}

/// <summary>
/// CPU auto-fix implementation
/// </summary>
public class CpuAutoFix : IAutoFixAction
{
    public async Task<AutoFixResult> ExecuteAsync(HoloShipFitting fitting)
    {
        await Task.Delay(100);
        
        return new AutoFixResult
        {
            Success = true,
            Message = "Installed co-processor",
            Changes = new List<string> { "Added Co-Processor I" }
        };
    }

    public string GetDescription()
    {
        return "Install co-processor to increase CPU";
    }
}

/// <summary>
/// Event args for validation completion
/// </summary>
public class ValidationCompleteEventArgs : EventArgs
{
    public ValidationState State { get; }
    public List<ConstraintViolation> Violations { get; }

    public ValidationCompleteEventArgs(ValidationState state, List<ConstraintViolation> violations)
    {
        State = state;
        Violations = violations;
    }
}

/// <summary>
/// Event args for violation detection
/// </summary>
public class ViolationDetectedEventArgs : EventArgs
{
    public ConstraintViolation Violation { get; }

    public ViolationDetectedEventArgs(ConstraintViolation violation)
    {
        Violation = violation;
    }
}

/// <summary>
/// Event args for violation resolution
/// </summary>
public class ViolationResolvedEventArgs : EventArgs
{
    public ConstraintViolation Violation { get; }

    public ViolationResolvedEventArgs(ConstraintViolation violation)
    {
        Violation = violation;
    }
}

/// <summary>
/// Event args for auto-fix suggestions
/// </summary>
public class AutoFixSuggestionEventArgs : EventArgs
{
    public List<ConstraintViolation> AutoFixableViolations { get; }

    public AutoFixSuggestionEventArgs(List<ConstraintViolation> violations)
    {
        AutoFixableViolations = violations;
    }
}

#endregion