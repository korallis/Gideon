// ==========================================================================
// HoloOAuthFlow.cs - OAuth Flow with Holographic Progress Indicators
// ==========================================================================
// Advanced OAuth authentication flow featuring holographic progress visualization,
// step-by-step guidance, and EVE-style authentication experience.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
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
/// OAuth authentication flow with holographic progress indicators and step-by-step guidance
/// </summary>
public class HoloOAuthFlow : Control, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloOAuthFlow),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloOAuthFlow),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty CurrentStepProperty =
        DependencyProperty.Register(nameof(CurrentStep), typeof(OAuthStep), typeof(HoloOAuthFlow),
            new PropertyMetadata(OAuthStep.Initialize, OnCurrentStepChanged));

    public static readonly DependencyProperty ProgressPercentageProperty =
        DependencyProperty.Register(nameof(ProgressPercentage), typeof(double), typeof(HoloOAuthFlow),
            new PropertyMetadata(0.0, OnProgressPercentageChanged));

    public static readonly DependencyProperty EnableFlowAnimationsProperty =
        DependencyProperty.Register(nameof(EnableFlowAnimations), typeof(bool), typeof(HoloOAuthFlow),
            new PropertyMetadata(true, OnEnableFlowAnimationsChanged));

    public static readonly DependencyProperty EnableParticleStreamProperty =
        DependencyProperty.Register(nameof(EnableParticleStream), typeof(bool), typeof(HoloOAuthFlow),
            new PropertyMetadata(true, OnEnableParticleStreamChanged));

    public static readonly DependencyProperty ShowDetailedStepsProperty =
        DependencyProperty.Register(nameof(ShowDetailedSteps), typeof(bool), typeof(HoloOAuthFlow),
            new PropertyMetadata(true));

    public static readonly DependencyProperty ClientIdProperty =
        DependencyProperty.Register(nameof(ClientId), typeof(string), typeof(HoloOAuthFlow),
            new PropertyMetadata(null));

    public static readonly DependencyProperty RedirectUriProperty =
        DependencyProperty.Register(nameof(RedirectUri), typeof(string), typeof(HoloOAuthFlow),
            new PropertyMetadata("https://localhost:8080/callback"));

    public static readonly DependencyProperty ScopesProperty =
        DependencyProperty.Register(nameof(Scopes), typeof(string), typeof(HoloOAuthFlow),
            new PropertyMetadata("esi-characters.read_character_online.v1 esi-universe.read_structures.v1"));

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

    public OAuthStep CurrentStep
    {
        get => (OAuthStep)GetValue(CurrentStepProperty);
        set => SetValue(CurrentStepProperty, value);
    }

    public double ProgressPercentage
    {
        get => (double)GetValue(ProgressPercentageProperty);
        set => SetValue(ProgressPercentageProperty, value);
    }

    public bool EnableFlowAnimations
    {
        get => (bool)GetValue(EnableFlowAnimationsProperty);
        set => SetValue(EnableFlowAnimationsProperty, value);
    }

    public bool EnableParticleStream
    {
        get => (bool)GetValue(EnableParticleStreamProperty);
        set => SetValue(EnableParticleStreamProperty, value);
    }

    public bool ShowDetailedSteps
    {
        get => (bool)GetValue(ShowDetailedStepsProperty);
        set => SetValue(ShowDetailedStepsProperty, value);
    }

    public string ClientId
    {
        get => (string)GetValue(ClientIdProperty);
        set => SetValue(ClientIdProperty, value);
    }

    public string RedirectUri
    {
        get => (string)GetValue(RedirectUriProperty);
        set => SetValue(RedirectUriProperty, value);
    }

    public string Scopes
    {
        get => (string)GetValue(ScopesProperty);
        set => SetValue(ScopesProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloOAuthEventArgs> StepChanged;
    public event EventHandler<HoloOAuthEventArgs> FlowCompleted;
    public event EventHandler<HoloOAuthEventArgs> FlowFailed;
    public event EventHandler<HoloOAuthEventArgs> FlowCancelled;
    public event EventHandler<HoloOAuthEventArgs> AuthorizationUrlGenerated;

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode { get; private set; }

    public void SwitchToFullMode()
    {
        IsInSimplifiedMode = false;
        EnableFlowAnimations = true;
        EnableParticleStream = true;
        ShowDetailedSteps = true;
        UpdateFlowAppearance();
    }

    public void SwitchToSimplifiedMode()
    {
        IsInSimplifiedMode = true;
        EnableFlowAnimations = false;
        EnableParticleStream = false;
        ShowDetailedSteps = false;
        UpdateFlowAppearance();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => !_disposed && IsLoaded;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        EnableFlowAnimations = settings.EnabledFeatures.HasFlag(AnimationFeatures.ComplexTransitions);
        EnableParticleStream = settings.EnabledFeatures.HasFlag(AnimationFeatures.ParticleEffects);
        
        UpdateFlowAppearance();
    }

    #endregion

    #region Fields

    private Grid _flowGrid;
    private Border _flowContainer;
    private StackPanel _stepsPanel;
    private HoloProgressBar _progressBar;
    private TextBlock _currentStepText;
    private TextBlock _stepDescriptionText;
    private Canvas _particleCanvas;
    private Canvas _connectionCanvas;
    
    private readonly Dictionary<OAuthStep, OAuthStepControl> _stepControls = new();
    private readonly List<FlowParticle> _particles = new();
    private readonly List<ConnectionLine> _connectionLines = new();
    private DispatcherTimer _particleTimer;
    private DispatcherTimer _animationTimer;
    private double _particlePhase = 0;
    private double _animationPhase = 0;
    private readonly Random _random = new();
    private bool _disposed = false;
    private bool _isFlowActive = false;

    #endregion

    #region Constructor

    public HoloOAuthFlow()
    {
        DefaultStyleKey = typeof(HoloOAuthFlow);
        Width = 500;
        Height = 400;
        InitializeFlow();
        SetupAnimations();
        
        // Register with animation intensity manager
        AnimationIntensityManager.Instance.RegisterTarget(this);
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Start the OAuth authentication flow
    /// </summary>
    public async Task StartFlowAsync()
    {
        if (_isFlowActive) return;

        _isFlowActive = true;
        CurrentStep = OAuthStep.Initialize;
        ProgressPercentage = 0;

        try
        {
            await ExecuteFlowStepsAsync();
        }
        catch (Exception ex)
        {
            FlowFailed?.Invoke(this, new HoloOAuthEventArgs 
            { 
                Step = CurrentStep,
                ErrorMessage = ex.Message,
                Timestamp = DateTime.Now
            });
        }
        finally
        {
            _isFlowActive = false;
        }
    }

    /// <summary>
    /// Cancel the current OAuth flow
    /// </summary>
    public void CancelFlow()
    {
        if (!_isFlowActive) return;

        _isFlowActive = false;
        FlowCancelled?.Invoke(this, new HoloOAuthEventArgs 
        { 
            Step = CurrentStep,
            Timestamp = DateTime.Now
        });
    }

    /// <summary>
    /// Set progress for current step
    /// </summary>
    public void SetStepProgress(double percentage)
    {
        ProgressPercentage = Math.Clamp(percentage, 0, 100);
        
        if (_progressBar != null)
        {
            _progressBar.Value = ProgressPercentage;
        }
    }

    /// <summary>
    /// Move to next step
    /// </summary>
    public void AdvanceToStep(OAuthStep step)
    {
        var previousStep = CurrentStep;
        CurrentStep = step;
        
        ProgressPercentage = GetStepProgress(step);
        
        StepChanged?.Invoke(this, new HoloOAuthEventArgs 
        { 
            PreviousStep = previousStep,
            Step = step,
            Progress = ProgressPercentage,
            Timestamp = DateTime.Now
        });
    }

    /// <summary>
    /// Complete the OAuth flow with success
    /// </summary>
    public void CompleteFlow(string authorizationCode, string accessToken = null)
    {
        _isFlowActive = false;
        CurrentStep = OAuthStep.Complete;
        ProgressPercentage = 100;
        
        FlowCompleted?.Invoke(this, new HoloOAuthEventArgs 
        { 
            Step = CurrentStep,
            AuthorizationCode = authorizationCode,
            AccessToken = accessToken,
            Timestamp = DateTime.Now
        });
    }

    #endregion

    #region Private Methods

    private void InitializeFlow()
    {
        Template = CreateFlowTemplate();
        UpdateFlowAppearance();
    }

    private ControlTemplate CreateFlowTemplate()
    {
        var template = new ControlTemplate(typeof(HoloOAuthFlow));

        // Main flow grid
        var flowGrid = new FrameworkElementFactory(typeof(Grid));
        flowGrid.Name = "PART_FlowGrid";

        // Row definitions
        var titleRow = new FrameworkElementFactory(typeof(RowDefinition));
        titleRow.SetValue(RowDefinition.HeightProperty, GridLength.Auto);
        var progressRow = new FrameworkElementFactory(typeof(RowDefinition));
        progressRow.SetValue(RowDefinition.HeightProperty, GridLength.Auto);
        var stepsRow = new FrameworkElementFactory(typeof(RowDefinition));
        stepsRow.SetValue(RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Star));
        var statusRow = new FrameworkElementFactory(typeof(RowDefinition));
        statusRow.SetValue(RowDefinition.HeightProperty, GridLength.Auto);

        flowGrid.AppendChild(titleRow);
        flowGrid.AppendChild(progressRow);
        flowGrid.AppendChild(stepsRow);
        flowGrid.AppendChild(statusRow);

        // Flow container
        var flowContainer = new FrameworkElementFactory(typeof(Border));
        flowContainer.Name = "PART_FlowContainer";
        flowContainer.SetValue(Border.CornerRadiusProperty, new CornerRadius(12));
        flowContainer.SetValue(Border.BorderThicknessProperty, new Thickness(2));
        flowContainer.SetValue(Border.PaddingProperty, new Thickness(20));
        flowContainer.SetValue(Grid.RowSpanProperty, 4);

        // Title section
        var titlePanel = new FrameworkElementFactory(typeof(StackPanel));
        titlePanel.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        titlePanel.SetValue(StackPanel.MarginProperty, new Thickness(0, 0, 0, 20));
        titlePanel.SetValue(Grid.RowProperty, 0);

        var titleText = new FrameworkElementFactory(typeof(TextBlock));
        titleText.SetValue(TextBlock.TextProperty, "EVE Online Authentication");
        titleText.SetValue(TextBlock.FontSizeProperty, 18.0);
        titleText.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        titleText.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);

        var subtitleText = new FrameworkElementFactory(typeof(TextBlock));
        subtitleText.SetValue(TextBlock.TextProperty, "Secure OAuth 2.0 PKCE Flow");
        subtitleText.SetValue(TextBlock.FontSizeProperty, 12.0);
        subtitleText.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        subtitleText.SetValue(TextBlock.OpacityProperty, 0.8);

        titlePanel.AppendChild(titleText);
        titlePanel.AppendChild(subtitleText);

        // Progress bar
        var progressBar = new FrameworkElementFactory(typeof(HoloProgressBar));
        progressBar.Name = "PART_ProgressBar";
        progressBar.SetValue(HoloProgressBar.HeightProperty, 8.0);
        progressBar.SetValue(HoloProgressBar.MarginProperty, new Thickness(0, 0, 0, 20));
        progressBar.SetValue(HoloProgressBar.MaximumProperty, 100.0);
        progressBar.SetValue(Grid.RowProperty, 1);

        // Steps panel container
        var stepsContainer = new FrameworkElementFactory(typeof(ScrollViewer));
        stepsContainer.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
        stepsContainer.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
        stepsContainer.SetValue(Grid.RowProperty, 2);

        // Steps panel
        var stepsPanel = new FrameworkElementFactory(typeof(StackPanel));
        stepsPanel.Name = "PART_StepsPanel";
        stepsPanel.SetValue(StackPanel.OrientationProperty, Orientation.Vertical);

        stepsContainer.AppendChild(stepsPanel);

        // Status section
        var statusPanel = new FrameworkElementFactory(typeof(StackPanel));
        statusPanel.SetValue(StackPanel.MarginProperty, new Thickness(0, 20, 0, 0));
        statusPanel.SetValue(Grid.RowProperty, 3);

        // Current step text
        var currentStepText = new FrameworkElementFactory(typeof(TextBlock));
        currentStepText.Name = "PART_CurrentStepText";
        currentStepText.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        currentStepText.SetValue(TextBlock.FontSizeProperty, 14.0);

        // Step description text
        var stepDescriptionText = new FrameworkElementFactory(typeof(TextBlock));
        stepDescriptionText.Name = "PART_StepDescriptionText";
        stepDescriptionText.SetValue(TextBlock.FontSizeProperty, 12.0);
        stepDescriptionText.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
        stepDescriptionText.SetValue(TextBlock.OpacityProperty, 0.8);
        stepDescriptionText.SetValue(TextBlock.MarginProperty, new Thickness(0, 4, 0, 0));

        statusPanel.AppendChild(currentStepText);
        statusPanel.AppendChild(stepDescriptionText);

        // Connection canvas for step connections
        var connectionCanvas = new FrameworkElementFactory(typeof(Canvas));
        connectionCanvas.Name = "PART_ConnectionCanvas";
        connectionCanvas.SetValue(Canvas.IsHitTestVisibleProperty, false);
        connectionCanvas.SetValue(Canvas.ClipToBoundsProperty, true);
        connectionCanvas.SetValue(Grid.RowSpanProperty, 4);

        // Particle canvas for effects
        var particleCanvas = new FrameworkElementFactory(typeof(Canvas));
        particleCanvas.Name = "PART_ParticleCanvas";
        particleCanvas.SetValue(Canvas.IsHitTestVisibleProperty, false);
        particleCanvas.SetValue(Canvas.ClipToBoundsProperty, true);
        particleCanvas.SetValue(Grid.RowSpanProperty, 4);

        // Assembly
        flowContainer.AppendChild(titlePanel);
        flowContainer.AppendChild(progressBar);
        flowContainer.AppendChild(stepsContainer);
        flowContainer.AppendChild(statusPanel);

        flowGrid.AppendChild(flowContainer);
        flowGrid.AppendChild(connectionCanvas);
        flowGrid.AppendChild(particleCanvas);

        template.VisualTree = flowGrid;
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

        // Flow animation timer
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
        };
        _animationTimer.Tick += OnAnimationTick;
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        if (!EnableParticleStream || IsInSimplifiedMode || _particleCanvas == null) return;

        _particlePhase += 0.1;
        if (_particlePhase > Math.PI * 2)
            _particlePhase = 0;

        UpdateFlowParticles();
        
        if (_isFlowActive)
        {
            SpawnFlowParticles();
        }
    }

    private void OnAnimationTick(object sender, EventArgs e)
    {
        if (!EnableFlowAnimations || IsInSimplifiedMode) return;

        _animationPhase += 0.05;
        if (_animationPhase > Math.PI * 2)
            _animationPhase = 0;

        UpdateConnectionAnimations();
        UpdateStepAnimations();
    }

    private void UpdateFlowParticles()
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var particle = _particles[i];
            
            // Update particle position
            particle.X += particle.VelocityX;
            particle.Y += particle.VelocityY;
            particle.Life -= 0.01;

            // Add flow effect
            particle.VelocityY += Math.Sin(particle.Phase) * 0.1;
            particle.Phase += 0.1;

            // Update visual position
            Canvas.SetLeft(particle.Visual, particle.X);
            Canvas.SetTop(particle.Visual, particle.Y);
            
            // Update opacity
            particle.Visual.Opacity = Math.Max(0, particle.Life) * HolographicIntensity;

            // Remove dead particles
            if (particle.Life <= 0 || particle.Y < -10 || particle.Y > ActualHeight + 10)
            {
                _particleCanvas.Children.Remove(particle.Visual);
                _particles.RemoveAt(i);
            }
        }
    }

    private void SpawnFlowParticles()
    {
        if (_particles.Count >= 15) return; // Limit particle count

        if (_random.NextDouble() < 0.2) // 20% chance to spawn
        {
            var particle = CreateFlowParticle();
            _particles.Add(particle);
            _particleCanvas.Children.Add(particle.Visual);
        }
    }

    private FlowParticle CreateFlowParticle()
    {
        var color = GetEVEColor(EVEColorScheme);
        var size = 1 + _random.NextDouble() * 2;
        
        var ellipse = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromArgb(120, color.R, color.G, color.B))
        };

        var particle = new FlowParticle
        {
            Visual = ellipse,
            X = _random.NextDouble() * ActualWidth,
            Y = ActualHeight + size,
            VelocityX = (_random.NextDouble() - 0.5) * 1,
            VelocityY = -1 - _random.NextDouble() * 2,
            Life = 1.0,
            Phase = _random.NextDouble() * Math.PI * 2
        };

        Canvas.SetLeft(ellipse, particle.X);
        Canvas.SetTop(ellipse, particle.Y);

        return particle;
    }

    private void UpdateConnectionAnimations()
    {
        foreach (var line in _connectionLines)
        {
            if (line.Visual.Effect is DropShadowEffect effect)
            {
                var glowIntensity = 0.5 + (Math.Sin(_animationPhase + line.Phase) * 0.3);
                effect.Opacity = HolographicIntensity * glowIntensity;
            }
        }
    }

    private void UpdateStepAnimations()
    {
        foreach (var stepControl in _stepControls.Values)
        {
            if (stepControl.Step == CurrentStep)
            {
                var pulseIntensity = 0.8 + (Math.Sin(_animationPhase * 2) * 0.2);
                stepControl.AnimationIntensity = pulseIntensity;
            }
            else if ((int)stepControl.Step < (int)CurrentStep)
            {
                stepControl.AnimationIntensity = 0.6;
            }
            else
            {
                stepControl.AnimationIntensity = 0.3;
            }
        }
    }

    private async Task ExecuteFlowStepsAsync()
    {
        var steps = Enum.GetValues<OAuthStep>();
        
        foreach (var step in steps)
        {
            if (!_isFlowActive) break;

            AdvanceToStep(step);
            await SimulateStepAsync(step);
        }

        if (_isFlowActive)
        {
            // Simulate successful completion
            CompleteFlow("dummy_auth_code", "dummy_access_token");
        }
    }

    private async Task SimulateStepAsync(OAuthStep step)
    {
        var duration = GetStepDuration(step);
        var stepProgress = 0.0;
        var increment = 100.0 / (duration / 50); // 50ms intervals

        while (stepProgress < 100 && _isFlowActive)
        {
            stepProgress = Math.Min(100, stepProgress + increment);
            SetStepProgress(GetStepProgress(step) + (stepProgress * GetStepWeight(step) / 100));
            
            await Task.Delay(50);
        }
    }

    private void UpdateFlowAppearance()
    {
        if (Template == null) return;

        Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            ApplyTemplate();
            
            _flowGrid = GetTemplateChild("PART_FlowGrid") as Grid;
            _flowContainer = GetTemplateChild("PART_FlowContainer") as Border;
            _stepsPanel = GetTemplateChild("PART_StepsPanel") as StackPanel;
            _progressBar = GetTemplateChild("PART_ProgressBar") as HoloProgressBar;
            _currentStepText = GetTemplateChild("PART_CurrentStepText") as TextBlock;
            _stepDescriptionText = GetTemplateChild("PART_StepDescriptionText") as TextBlock;
            _particleCanvas = GetTemplateChild("PART_ParticleCanvas") as Canvas;
            _connectionCanvas = GetTemplateChild("PART_ConnectionCanvas") as Canvas;

            UpdateColors();
            UpdateEffects();
            UpdateStepsDisplay();
            UpdateCurrentStepDisplay();
        }), DispatcherPriority.Render);
    }

    private void UpdateColors()
    {
        var color = GetEVEColor(EVEColorScheme);

        // Flow container colors
        if (_flowContainer != null)
        {
            var backgroundBrush = new LinearGradientBrush();
            backgroundBrush.StartPoint = new Point(0, 0);
            backgroundBrush.EndPoint = new Point(1, 1);
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(200, 0, 20, 40), 0.0));
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(180, 0, 15, 30), 1.0));

            _flowContainer.Background = backgroundBrush;
            _flowContainer.BorderBrush = new SolidColorBrush(Color.FromArgb(
                180, color.R, color.G, color.B));
        }

        // Progress bar colors
        if (_progressBar != null)
        {
            _progressBar.EVEColorScheme = EVEColorScheme;
            _progressBar.HolographicIntensity = HolographicIntensity;
        }

        // Text colors
        if (_currentStepText != null)
        {
            _currentStepText.Foreground = new SolidColorBrush(Colors.White);
        }

        if (_stepDescriptionText != null)
        {
            _stepDescriptionText.Foreground = new SolidColorBrush(Color.FromArgb(200, 180, 180, 180));
        }
    }

    private void UpdateEffects()
    {
        var color = GetEVEColor(EVEColorScheme);

        if (_flowContainer != null && !IsInSimplifiedMode)
        {
            _flowContainer.Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 15 * HolographicIntensity,
                ShadowDepth = 0,
                Opacity = 0.6 * HolographicIntensity
            };
        }
    }

    private void UpdateStepsDisplay()
    {
        if (_stepsPanel == null) return;

        _stepsPanel.Children.Clear();
        _stepControls.Clear();

        if (!ShowDetailedSteps) return;

        var steps = Enum.GetValues<OAuthStep>();
        
        for (int i = 0; i < steps.Length; i++)
        {
            var step = steps[i];
            var stepControl = CreateStepControl(step, i);
            _stepControls[step] = stepControl;
            _stepsPanel.Children.Add(stepControl);

            // Add connection line between steps
            if (i < steps.Length - 1)
            {
                CreateConnectionLine(i, i + 1);
            }
        }
    }

    private OAuthStepControl CreateStepControl(OAuthStep step, int index)
    {
        var control = new OAuthStepControl
        {
            Step = step,
            StepIndex = index,
            HolographicIntensity = HolographicIntensity,
            EVEColorScheme = EVEColorScheme,
            IsActive = step == CurrentStep,
            IsCompleted = (int)step < (int)CurrentStep,
            Margin = new Thickness(0, 4, 0, 4)
        };

        return control;
    }

    private void CreateConnectionLine(int fromIndex, int toIndex)
    {
        if (_connectionCanvas == null) return;

        var color = GetEVEColor(EVEColorScheme);
        var line = new Line
        {
            Stroke = new SolidColorBrush(Color.FromArgb(100, color.R, color.G, color.B)),
            StrokeThickness = 2,
            X1 = 20,
            Y1 = (fromIndex + 1) * 40 - 8,
            X2 = 20,
            Y2 = (toIndex + 1) * 40 + 8
        };

        if (!IsInSimplifiedMode)
        {
            line.Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 4,
                ShadowDepth = 0,
                Opacity = 0.5 * HolographicIntensity
            };
        }

        var connectionLine = new ConnectionLine
        {
            Visual = line,
            Phase = _random.NextDouble() * Math.PI * 2
        };

        _connectionLines.Add(connectionLine);
        _connectionCanvas.Children.Add(line);
    }

    private void UpdateCurrentStepDisplay()
    {
        if (_currentStepText != null)
        {
            _currentStepText.Text = GetStepTitle(CurrentStep);
        }

        if (_stepDescriptionText != null)
        {
            _stepDescriptionText.Text = GetStepDescription(CurrentStep);
        }
    }

    private double GetStepProgress(OAuthStep step)
    {
        return step switch
        {
            OAuthStep.Initialize => 0,
            OAuthStep.GenerateCodeChallenge => 10,
            OAuthStep.BuildAuthorizationUrl => 20,
            OAuthStep.OpenBrowser => 30,
            OAuthStep.WaitForCallback => 50,
            OAuthStep.ExchangeToken => 80,
            OAuthStep.ValidateToken => 90,
            OAuthStep.Complete => 100,
            _ => 0
        };
    }

    private double GetStepWeight(OAuthStep step)
    {
        return step switch
        {
            OAuthStep.Initialize => 10,
            OAuthStep.GenerateCodeChallenge => 10,
            OAuthStep.BuildAuthorizationUrl => 10,
            OAuthStep.OpenBrowser => 20,
            OAuthStep.WaitForCallback => 30,
            OAuthStep.ExchangeToken => 10,
            OAuthStep.ValidateToken => 10,
            OAuthStep.Complete => 0,
            _ => 10
        };
    }

    private int GetStepDuration(OAuthStep step)
    {
        return step switch
        {
            OAuthStep.Initialize => 500,
            OAuthStep.GenerateCodeChallenge => 300,
            OAuthStep.BuildAuthorizationUrl => 200,
            OAuthStep.OpenBrowser => 1000,
            OAuthStep.WaitForCallback => 3000,
            OAuthStep.ExchangeToken => 800,
            OAuthStep.ValidateToken => 400,
            OAuthStep.Complete => 200,
            _ => 500
        };
    }

    private string GetStepTitle(OAuthStep step)
    {
        return step switch
        {
            OAuthStep.Initialize => "Initializing Authentication",
            OAuthStep.GenerateCodeChallenge => "Generating Security Code",
            OAuthStep.BuildAuthorizationUrl => "Building Authorization URL",
            OAuthStep.OpenBrowser => "Opening EVE Online Login",
            OAuthStep.WaitForCallback => "Waiting for Authorization",
            OAuthStep.ExchangeToken => "Exchanging Authorization Code",
            OAuthStep.ValidateToken => "Validating Access Token",
            OAuthStep.Complete => "Authentication Complete",
            _ => "Unknown Step"
        };
    }

    private string GetStepDescription(OAuthStep step)
    {
        return step switch
        {
            OAuthStep.Initialize => "Setting up OAuth 2.0 PKCE flow parameters and preparing secure authentication.",
            OAuthStep.GenerateCodeChallenge => "Creating cryptographic challenge for enhanced security.",
            OAuthStep.BuildAuthorizationUrl => "Constructing secure authorization URL with required parameters.",
            OAuthStep.OpenBrowser => "Launching browser to EVE Online authentication page.",
            OAuthStep.WaitForCallback => "Please complete login in your browser. Waiting for authorization response.",
            OAuthStep.ExchangeToken => "Securely exchanging authorization code for access token.",
            OAuthStep.ValidateToken => "Verifying token validity and retrieving character information.",
            OAuthStep.Complete => "Authentication successful! Welcome to Gideon.",
            _ => "Processing authentication step..."
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
        if (EnableParticleStream && !IsInSimplifiedMode)
            _particleTimer.Start();
            
        if (EnableFlowAnimations && !IsInSimplifiedMode)
            _animationTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _particleTimer?.Stop();
        _animationTimer?.Stop();
        
        // Clean up particles and connections
        _particles.Clear();
        _connectionLines.Clear();
        _particleCanvas?.Children.Clear();
        _connectionCanvas?.Children.Clear();
        
        // Unregister from animation intensity manager
        AnimationIntensityManager.Instance.UnregisterTarget(this);
        
        _disposed = true;
    }

    #endregion

    #region Event Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloOAuthFlow flow)
            flow.UpdateFlowAppearance();
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloOAuthFlow flow)
            flow.UpdateFlowAppearance();
    }

    private static void OnCurrentStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloOAuthFlow flow)
            flow.UpdateCurrentStepDisplay();
    }

    private static void OnProgressPercentageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloOAuthFlow flow && flow._progressBar != null)
            flow._progressBar.Value = (double)e.NewValue;
    }

    private static void OnEnableFlowAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloOAuthFlow flow)
        {
            if ((bool)e.NewValue && !flow.IsInSimplifiedMode)
                flow._animationTimer.Start();
            else
                flow._animationTimer.Stop();
        }
    }

    private static void OnEnableParticleStreamChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloOAuthFlow flow)
        {
            if ((bool)e.NewValue && !flow.IsInSimplifiedMode)
                flow._particleTimer.Start();
            else
                flow._particleTimer.Stop();
        }
    }

    #endregion
}

/// <summary>
/// Flow particle for visual effects
/// </summary>
internal class FlowParticle
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
/// Connection line between steps
/// </summary>
internal class ConnectionLine
{
    public Line Visual { get; set; }
    public double Phase { get; set; }
}

/// <summary>
/// Individual OAuth step control
/// </summary>
internal class OAuthStepControl : Control
{
    public OAuthStep Step { get; set; }
    public int StepIndex { get; set; }
    public double HolographicIntensity { get; set; }
    public EVEColorScheme EVEColorScheme { get; set; }
    public bool IsActive { get; set; }
    public bool IsCompleted { get; set; }
    public double AnimationIntensity { get; set; } = 1.0;
}

/// <summary>
/// OAuth flow steps
/// </summary>
public enum OAuthStep
{
    Initialize,
    GenerateCodeChallenge,
    BuildAuthorizationUrl,
    OpenBrowser,
    WaitForCallback,
    ExchangeToken,
    ValidateToken,
    Complete
}

/// <summary>
/// Event args for OAuth flow events
/// </summary>
public class HoloOAuthEventArgs : EventArgs
{
    public OAuthStep PreviousStep { get; set; }
    public OAuthStep Step { get; set; }
    public double Progress { get; set; }
    public string AuthorizationCode { get; set; }
    public string AccessToken { get; set; }
    public string AuthorizationUrl { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; }
}