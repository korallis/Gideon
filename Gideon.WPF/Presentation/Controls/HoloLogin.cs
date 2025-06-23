// ==========================================================================
// HoloLogin.cs - Holographic Login Interface with Westworld Aesthetics
// ==========================================================================
// Advanced holographic login interface featuring EVE OAuth integration,
// Westworld-style visual effects, and comprehensive authentication flow.
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
/// Holographic login interface with Westworld aesthetics and EVE OAuth integration
/// </summary>
public class HoloLogin : Control, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloLogin),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloLogin),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty AuthenticationStateProperty =
        DependencyProperty.Register(nameof(AuthenticationState), typeof(AuthState), typeof(HoloLogin),
            new PropertyMetadata(AuthState.Disconnected, OnAuthenticationStateChanged));

    public static readonly DependencyProperty ShowWelcomeMessageProperty =
        DependencyProperty.Register(nameof(ShowWelcomeMessage), typeof(bool), typeof(HoloLogin),
            new PropertyMetadata(true));

    public static readonly DependencyProperty EnableBackgroundAnimationProperty =
        DependencyProperty.Register(nameof(EnableBackgroundAnimation), typeof(bool), typeof(HoloLogin),
            new PropertyMetadata(true, OnEnableBackgroundAnimationChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloLogin),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ClientIdProperty =
        DependencyProperty.Register(nameof(ClientId), typeof(string), typeof(HoloLogin),
            new PropertyMetadata(null));

    public static readonly DependencyProperty RedirectUriProperty =
        DependencyProperty.Register(nameof(RedirectUri), typeof(string), typeof(HoloLogin),
            new PropertyMetadata("https://localhost:8080/callback"));

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

    public AuthState AuthenticationState
    {
        get => (AuthState)GetValue(AuthenticationStateProperty);
        set => SetValue(AuthenticationStateProperty, value);
    }

    public bool ShowWelcomeMessage
    {
        get => (bool)GetValue(ShowWelcomeMessageProperty);
        set => SetValue(ShowWelcomeMessageProperty, value);
    }

    public bool EnableBackgroundAnimation
    {
        get => (bool)GetValue(EnableBackgroundAnimationProperty);
        set => SetValue(EnableBackgroundAnimationProperty, value);
    }

    public bool EnableParticleEffects
    {
        get => (bool)GetValue(EnableParticleEffectsProperty);
        set => SetValue(EnableParticleEffectsProperty, value);
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

    #endregion

    #region Events

    public event EventHandler<HoloLoginEventArgs> LoginRequested;
    public event EventHandler<HoloLoginEventArgs> LogoutRequested;
    public event EventHandler<HoloLoginEventArgs> AuthenticationCompleted;
    public event EventHandler<HoloLoginEventArgs> AuthenticationFailed;

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode { get; private set; }

    public void SwitchToFullMode()
    {
        IsInSimplifiedMode = false;
        EnableBackgroundAnimation = true;
        EnableParticleEffects = true;
        UpdateLoginAppearance();
    }

    public void SwitchToSimplifiedMode()
    {
        IsInSimplifiedMode = true;
        EnableBackgroundAnimation = false;
        EnableParticleEffects = false;
        UpdateLoginAppearance();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => !_disposed && IsLoaded;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        EnableBackgroundAnimation = settings.EnabledFeatures.HasFlag(AnimationFeatures.ComplexTransitions);
        EnableParticleEffects = settings.EnabledFeatures.HasFlag(AnimationFeatures.ParticleEffects);
        
        UpdateLoginAppearance();
    }

    #endregion

    #region Fields

    private Grid _loginGrid;
    private Border _loginPanel;
    private Border _logoContainer;
    private Path _gideonLogo;
    private TextBlock _welcomeText;
    private TextBlock _subtitleText;
    private HoloButton _loginButton;
    private HoloButton _logoutButton;
    private HoloProgressBar _authProgress;
    private TextBlock _statusText;
    private Canvas _particleCanvas;
    private Canvas _backgroundCanvas;
    
    private readonly List<LoginParticle> _particles = new();
    private readonly List<BackgroundElement> _backgroundElements = new();
    private DispatcherTimer _particleTimer;
    private DispatcherTimer _backgroundTimer;
    private DispatcherTimer _pulseTimer;
    private double _particlePhase = 0;
    private double _backgroundPhase = 0;
    private double _pulsePhase = 0;
    private readonly Random _random = new();
    private bool _disposed = false;
    private bool _isAuthenticating = false;

    #endregion

    #region Constructor

    public HoloLogin()
    {
        DefaultStyleKey = typeof(HoloLogin);
        Width = 400;
        Height = 500;
        InitializeLogin();
        SetupAnimations();
        
        // Register with animation intensity manager
        AnimationIntensityManager.Instance.RegisterTarget(this);
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Start authentication process
    /// </summary>
    public async Task StartAuthenticationAsync()
    {
        if (_isAuthenticating) return;

        _isAuthenticating = true;
        AuthenticationState = AuthState.Authenticating;
        
        LoginRequested?.Invoke(this, new HoloLoginEventArgs { State = AuthState.Authenticating });

        // TODO: Implement actual OAuth flow
        await SimulateAuthenticationAsync();
    }

    /// <summary>
    /// Complete authentication with result
    /// </summary>
    public void CompleteAuthentication(bool success, string message = null)
    {
        _isAuthenticating = false;
        
        if (success)
        {
            AuthenticationState = AuthState.Authenticated;
            AuthenticationCompleted?.Invoke(this, new HoloLoginEventArgs 
            { 
                State = AuthState.Authenticated,
                Message = message ?? "Authentication successful"
            });
        }
        else
        {
            AuthenticationState = AuthState.Failed;
            AuthenticationFailed?.Invoke(this, new HoloLoginEventArgs 
            { 
                State = AuthState.Failed,
                Message = message ?? "Authentication failed"
            });
        }
    }

    /// <summary>
    /// Logout current session
    /// </summary>
    public void Logout()
    {
        AuthenticationState = AuthState.Disconnected;
        LogoutRequested?.Invoke(this, new HoloLoginEventArgs { State = AuthState.Disconnected });
    }

    /// <summary>
    /// Update authentication status text
    /// </summary>
    public void UpdateStatus(string message)
    {
        if (_statusText != null)
        {
            _statusText.Text = message;
        }
    }

    #endregion

    #region Private Methods

    private void InitializeLogin()
    {
        Template = CreateLoginTemplate();
        UpdateLoginAppearance();
    }

    private ControlTemplate CreateLoginTemplate()
    {
        var template = new ControlTemplate(typeof(HoloLogin));

        // Main login grid
        var loginGrid = new FrameworkElementFactory(typeof(Grid));
        loginGrid.Name = "PART_LoginGrid";

        // Background canvas for effects
        var backgroundCanvas = new FrameworkElementFactory(typeof(Canvas));
        backgroundCanvas.Name = "PART_BackgroundCanvas";
        backgroundCanvas.SetValue(Canvas.IsHitTestVisibleProperty, false);
        backgroundCanvas.SetValue(Canvas.ClipToBoundsProperty, true);

        // Main login panel
        var loginPanel = new FrameworkElementFactory(typeof(Border));
        loginPanel.Name = "PART_LoginPanel";
        loginPanel.SetValue(Border.CornerRadiusProperty, new CornerRadius(12));
        loginPanel.SetValue(Border.BorderThicknessProperty, new Thickness(2));
        loginPanel.SetValue(Border.PaddingProperty, new Thickness(40, 30, 40, 30));
        loginPanel.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        loginPanel.SetValue(Border.VerticalAlignmentProperty, VerticalAlignment.Center);

        // Content stack panel
        var contentStack = new FrameworkElementFactory(typeof(StackPanel));
        contentStack.SetValue(StackPanel.OrientationProperty, Orientation.Vertical);
        contentStack.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);

        // Logo container
        var logoContainer = new FrameworkElementFactory(typeof(Border));
        logoContainer.Name = "PART_LogoContainer";
        logoContainer.SetValue(Border.WidthProperty, 80.0);
        logoContainer.SetValue(Border.HeightProperty, 80.0);
        logoContainer.SetValue(Border.CornerRadiusProperty, new CornerRadius(40));
        logoContainer.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        logoContainer.SetValue(Border.MarginProperty, new Thickness(0, 0, 0, 20));

        // Gideon logo
        var gideonLogo = new FrameworkElementFactory(typeof(Path));
        gideonLogo.Name = "PART_GideonLogo";
        gideonLogo.SetValue(Path.WidthProperty, 40.0);
        gideonLogo.SetValue(Path.HeightProperty, 40.0);
        gideonLogo.SetValue(Path.StretchProperty, Stretch.Uniform);
        gideonLogo.SetValue(Path.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        gideonLogo.SetValue(Path.VerticalAlignmentProperty, VerticalAlignment.Center);
        gideonLogo.SetValue(Path.DataProperty, Geometry.Parse("M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M11,16.5L6.5,12L7.91,10.59L11,13.67L16.59,8.09L18,9.5L11,16.5Z"));

        logoContainer.AppendChild(gideonLogo);

        // Welcome text
        var welcomeText = new FrameworkElementFactory(typeof(TextBlock));
        welcomeText.Name = "PART_WelcomeText";
        welcomeText.SetValue(TextBlock.TextProperty, "GIDEON");
        welcomeText.SetValue(TextBlock.FontFamilyProperty, new FontFamily("Segoe UI"));
        welcomeText.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        welcomeText.SetValue(TextBlock.FontSizeProperty, 32.0);
        welcomeText.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        welcomeText.SetValue(TextBlock.MarginProperty, new Thickness(0, 0, 0, 8));

        // Subtitle text
        var subtitleText = new FrameworkElementFactory(typeof(TextBlock));
        subtitleText.Name = "PART_SubtitleText";
        subtitleText.SetValue(TextBlock.TextProperty, "EVE TOOLKIT");
        subtitleText.SetValue(TextBlock.FontFamilyProperty, new FontFamily("Segoe UI"));
        subtitleText.SetValue(TextBlock.FontWeightProperty, FontWeights.Normal);
        subtitleText.SetValue(TextBlock.FontSizeProperty, 14.0);
        subtitleText.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        subtitleText.SetValue(TextBlock.MarginProperty, new Thickness(0, 0, 0, 30));

        // Login button
        var loginButton = new FrameworkElementFactory(typeof(HoloButton));
        loginButton.Name = "PART_LoginButton";
        loginButton.SetValue(HoloButton.WidthProperty, 250.0);
        loginButton.SetValue(HoloButton.HeightProperty, 45.0);
        loginButton.SetValue(HoloButton.ContentProperty, "LOGIN WITH EVE ONLINE");
        loginButton.SetValue(HoloButton.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        loginButton.SetValue(HoloButton.MarginProperty, new Thickness(0, 0, 0, 15));

        // Logout button
        var logoutButton = new FrameworkElementFactory(typeof(HoloButton));
        logoutButton.Name = "PART_LogoutButton";
        logoutButton.SetValue(HoloButton.WidthProperty, 250.0);
        logoutButton.SetValue(HoloButton.HeightProperty, 35.0);
        logoutButton.SetValue(HoloButton.ContentProperty, "LOGOUT");
        logoutButton.SetValue(HoloButton.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        logoutButton.SetValue(HoloButton.MarginProperty, new Thickness(0, 0, 0, 15));
        logoutButton.SetValue(HoloButton.VisibilityProperty, Visibility.Collapsed);

        // Authentication progress
        var authProgress = new FrameworkElementFactory(typeof(HoloProgressBar));
        authProgress.Name = "PART_AuthProgress";
        authProgress.SetValue(HoloProgressBar.WidthProperty, 250.0);
        authProgress.SetValue(HoloProgressBar.HeightProperty, 8.0);
        authProgress.SetValue(HoloProgressBar.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        authProgress.SetValue(HoloProgressBar.MarginProperty, new Thickness(0, 0, 0, 15));
        authProgress.SetValue(HoloProgressBar.VisibilityProperty, Visibility.Collapsed);

        // Status text
        var statusText = new FrameworkElementFactory(typeof(TextBlock));
        statusText.Name = "PART_StatusText";
        statusText.SetValue(TextBlock.TextProperty, "Ready to authenticate");
        statusText.SetValue(TextBlock.FontSizeProperty, 12.0);
        statusText.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        statusText.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);

        // Particle canvas
        var particleCanvas = new FrameworkElementFactory(typeof(Canvas));
        particleCanvas.Name = "PART_ParticleCanvas";
        particleCanvas.SetValue(Canvas.IsHitTestVisibleProperty, false);
        particleCanvas.SetValue(Canvas.ClipToBoundsProperty, true);

        // Assembly
        contentStack.AppendChild(logoContainer);
        contentStack.AppendChild(welcomeText);
        contentStack.AppendChild(subtitleText);
        contentStack.AppendChild(loginButton);
        contentStack.AppendChild(logoutButton);
        contentStack.AppendChild(authProgress);
        contentStack.AppendChild(statusText);

        loginPanel.AppendChild(contentStack);

        loginGrid.AppendChild(backgroundCanvas);
        loginGrid.AppendChild(loginPanel);
        loginGrid.AppendChild(particleCanvas);

        template.VisualTree = loginGrid;
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

        // Background animation timer
        _backgroundTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
        };
        _backgroundTimer.Tick += OnBackgroundTick;

        // Pulse animation timer
        _pulseTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
        };
        _pulseTimer.Tick += OnPulseTick;
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        if (!EnableParticleEffects || IsInSimplifiedMode || _particleCanvas == null) return;

        _particlePhase += 0.1;
        if (_particlePhase > Math.PI * 2)
            _particlePhase = 0;

        UpdateParticles();
        SpawnLoginParticles();
    }

    private void OnBackgroundTick(object sender, EventArgs e)
    {
        if (!EnableBackgroundAnimation || IsInSimplifiedMode || _backgroundCanvas == null) return;

        _backgroundPhase += 0.02;
        if (_backgroundPhase > Math.PI * 2)
            _backgroundPhase = 0;

        UpdateBackgroundElements();
        SpawnBackgroundElements();
    }

    private void OnPulseTick(object sender, EventArgs e)
    {
        if (IsInSimplifiedMode) return;

        _pulsePhase += 0.05;
        if (_pulsePhase > Math.PI * 2)
            _pulsePhase = 0;

        UpdatePulseEffects();
    }

    private void UpdateParticles()
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var particle = _particles[i];
            
            // Update particle position
            particle.X += particle.VelocityX;
            particle.Y += particle.VelocityY;
            particle.Life -= 0.01;

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

    private void SpawnLoginParticles()
    {
        if (_particles.Count >= 15) return; // Limit particle count

        if (_random.NextDouble() < 0.15) // 15% chance to spawn
        {
            var particle = CreateLoginParticle();
            _particles.Add(particle);
            _particleCanvas.Children.Add(particle.Visual);
        }
    }

    private LoginParticle CreateLoginParticle()
    {
        var color = GetEVEColor(EVEColorScheme);
        var size = 1 + _random.NextDouble() * 2;
        
        var ellipse = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromArgb(100, color.R, color.G, color.B))
        };

        var particle = new LoginParticle
        {
            Visual = ellipse,
            X = _random.NextDouble() * ActualWidth,
            Y = ActualHeight + size,
            VelocityX = (_random.NextDouble() - 0.5) * 2,
            VelocityY = -1 - _random.NextDouble() * 3,
            Life = 1.0
        };

        Canvas.SetLeft(ellipse, particle.X);
        Canvas.SetTop(ellipse, particle.Y);

        return particle;
    }

    private void UpdateBackgroundElements()
    {
        for (int i = _backgroundElements.Count - 1; i >= 0; i--)
        {
            var element = _backgroundElements[i];
            
            // Update element animation
            element.Phase += element.Speed;
            if (element.Phase > Math.PI * 2)
                element.Phase = 0;

            // Update visual effects
            var transform = element.Visual.RenderTransform as TranslateTransform ?? new TranslateTransform();
            transform.X = element.BaseX + Math.Sin(element.Phase) * element.Amplitude;
            transform.Y = element.BaseY + Math.Cos(element.Phase * 0.7) * element.Amplitude * 0.5;
            element.Visual.RenderTransform = transform;

            element.Visual.Opacity = (0.1 + Math.Sin(element.Phase * 2) * 0.05) * HolographicIntensity;
        }
    }

    private void SpawnBackgroundElements()
    {
        if (_backgroundElements.Count >= 8) return;

        if (_random.NextDouble() < 0.05) // 5% chance to spawn
        {
            var element = CreateBackgroundElement();
            _backgroundElements.Add(element);
            _backgroundCanvas.Children.Add(element.Visual);
        }
    }

    private BackgroundElement CreateBackgroundElement()
    {
        var color = GetEVEColor(EVEColorScheme);
        var size = 20 + _random.NextDouble() * 40;
        
        var shape = new Rectangle
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromArgb(30, color.R, color.G, color.B)),
            Stroke = new SolidColorBrush(Color.FromArgb(60, color.R, color.G, color.B)),
            StrokeThickness = 1,
            RadiusX = 4,
            RadiusY = 4
        };

        var element = new BackgroundElement
        {
            Visual = shape,
            BaseX = _random.NextDouble() * ActualWidth,
            BaseY = _random.NextDouble() * ActualHeight,
            Amplitude = 10 + _random.NextDouble() * 20,
            Speed = 0.01 + _random.NextDouble() * 0.02,
            Phase = _random.NextDouble() * Math.PI * 2
        };

        Canvas.SetLeft(shape, element.BaseX);
        Canvas.SetTop(shape, element.BaseY);

        return element;
    }

    private void UpdatePulseEffects()
    {
        // Logo pulsing effect
        if (_gideonLogo != null && !IsInSimplifiedMode)
        {
            var pulseIntensity = 0.8 + (Math.Sin(_pulsePhase * 2) * 0.2);
            _gideonLogo.Opacity = pulseIntensity;
        }

        // Panel glow effect
        if (_loginPanel?.Effect is DropShadowEffect effect)
        {
            var glowIntensity = 0.6 + (Math.Sin(_pulsePhase) * 0.4);
            effect.Opacity = HolographicIntensity * glowIntensity;
        }
    }

    private void UpdateLoginAppearance()
    {
        if (Template == null) return;

        Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            ApplyTemplate();
            
            _loginGrid = GetTemplateChild("PART_LoginGrid") as Grid;
            _loginPanel = GetTemplateChild("PART_LoginPanel") as Border;
            _logoContainer = GetTemplateChild("PART_LogoContainer") as Border;
            _gideonLogo = GetTemplateChild("PART_GideonLogo") as Path;
            _welcomeText = GetTemplateChild("PART_WelcomeText") as TextBlock;
            _subtitleText = GetTemplateChild("PART_SubtitleText") as TextBlock;
            _loginButton = GetTemplateChild("PART_LoginButton") as HoloButton;
            _logoutButton = GetTemplateChild("PART_LogoutButton") as HoloButton;
            _authProgress = GetTemplateChild("PART_AuthProgress") as HoloProgressBar;
            _statusText = GetTemplateChild("PART_StatusText") as TextBlock;
            _particleCanvas = GetTemplateChild("PART_ParticleCanvas") as Canvas;
            _backgroundCanvas = GetTemplateChild("PART_BackgroundCanvas") as Canvas;

            UpdateColors();
            UpdateEffects();
            UpdateButtonEvents();
            UpdateAuthenticationUI();
        }), DispatcherPriority.Render);
    }

    private void UpdateColors()
    {
        var color = GetEVEColor(EVEColorScheme);

        // Login panel colors
        if (_loginPanel != null)
        {
            var backgroundBrush = new LinearGradientBrush();
            backgroundBrush.StartPoint = new Point(0, 0);
            backgroundBrush.EndPoint = new Point(1, 1);
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(200, 0, 20, 40), 0.0));
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(180, 0, 15, 30), 1.0));

            _loginPanel.Background = backgroundBrush;
            _loginPanel.BorderBrush = new SolidColorBrush(Color.FromArgb(
                180, color.R, color.G, color.B));
        }

        // Logo colors
        if (_gideonLogo != null)
        {
            _gideonLogo.Fill = new SolidColorBrush(color);
        }

        if (_logoContainer != null)
        {
            var logoBrush = new RadialGradientBrush();
            logoBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(80, color.R, color.G, color.B), 0.0));
            logoBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(40, color.R, color.G, color.B), 1.0));

            _logoContainer.Background = logoBrush;
            _logoContainer.BorderBrush = new SolidColorBrush(Color.FromArgb(
                120, color.R, color.G, color.B));
            _logoContainer.BorderThickness = new Thickness(2);
        }

        // Text colors
        if (_welcomeText != null)
        {
            _welcomeText.Foreground = new SolidColorBrush(Colors.White);
        }

        if (_subtitleText != null)
        {
            _subtitleText.Foreground = new SolidColorBrush(color);
        }

        if (_statusText != null)
        {
            _statusText.Foreground = new SolidColorBrush(Color.FromArgb(200, 180, 180, 180));
        }
    }

    private void UpdateEffects()
    {
        var color = GetEVEColor(EVEColorScheme);

        // Login panel effects
        if (_loginPanel != null && !IsInSimplifiedMode)
        {
            _loginPanel.Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 20 * HolographicIntensity,
                ShadowDepth = 0,
                Opacity = 0.6 * HolographicIntensity
            };
        }

        // Logo effects
        if (_logoContainer != null && !IsInSimplifiedMode)
        {
            _logoContainer.Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 15 * HolographicIntensity,
                ShadowDepth = 0,
                Opacity = 0.8 * HolographicIntensity
            };
        }
    }

    private void UpdateButtonEvents()
    {
        if (_loginButton != null)
        {
            _loginButton.Click -= OnLoginButtonClick;
            _loginButton.Click += OnLoginButtonClick;
            _loginButton.EVEColorScheme = EVEColorScheme;
            _loginButton.HolographicIntensity = HolographicIntensity;
        }

        if (_logoutButton != null)
        {
            _logoutButton.Click -= OnLogoutButtonClick;
            _logoutButton.Click += OnLogoutButtonClick;
            _logoutButton.EVEColorScheme = EVEColorScheme.CrimsonRed;
            _logoutButton.HolographicIntensity = HolographicIntensity;
        }

        if (_authProgress != null)
        {
            _authProgress.EVEColorScheme = EVEColorScheme;
            _authProgress.HolographicIntensity = HolographicIntensity;
        }
    }

    private void UpdateAuthenticationUI()
    {
        switch (AuthenticationState)
        {
            case AuthState.Disconnected:
                _loginButton.Visibility = Visibility.Visible;
                _logoutButton.Visibility = Visibility.Collapsed;
                _authProgress.Visibility = Visibility.Collapsed;
                UpdateStatus("Ready to authenticate with EVE Online");
                break;

            case AuthState.Authenticating:
                _loginButton.Visibility = Visibility.Collapsed;
                _logoutButton.Visibility = Visibility.Collapsed;
                _authProgress.Visibility = Visibility.Visible;
                _authProgress.IsIndeterminate = true;
                UpdateStatus("Authenticating with EVE Online...");
                break;

            case AuthState.Authenticated:
                _loginButton.Visibility = Visibility.Collapsed;
                _logoutButton.Visibility = Visibility.Visible;
                _authProgress.Visibility = Visibility.Collapsed;
                UpdateStatus("Successfully authenticated");
                break;

            case AuthState.Failed:
                _loginButton.Visibility = Visibility.Visible;
                _logoutButton.Visibility = Visibility.Collapsed;
                _authProgress.Visibility = Visibility.Collapsed;
                UpdateStatus("Authentication failed. Please try again.");
                break;
        }
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

    private async Task SimulateAuthenticationAsync()
    {
        // Simulate authentication delay
        await Task.Delay(2000);
        
        // Simulate success (in real implementation, this would be the OAuth callback)
        CompleteAuthentication(true, "Authentication completed successfully");
    }

    private void OnLoginButtonClick(object sender, RoutedEventArgs e)
    {
        _ = StartAuthenticationAsync();
    }

    private void OnLogoutButtonClick(object sender, RoutedEventArgs e)
    {
        Logout();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableParticleEffects && !IsInSimplifiedMode)
            _particleTimer.Start();
            
        if (EnableBackgroundAnimation && !IsInSimplifiedMode)
            _backgroundTimer.Start();
            
        _pulseTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _particleTimer?.Stop();
        _backgroundTimer?.Stop();
        _pulseTimer?.Stop();
        
        // Clean up particles and elements
        _particles.Clear();
        _backgroundElements.Clear();
        _particleCanvas?.Children.Clear();
        _backgroundCanvas?.Children.Clear();
        
        // Unregister from animation intensity manager
        AnimationIntensityManager.Instance.UnregisterTarget(this);
        
        _disposed = true;
    }

    #endregion

    #region Event Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloLogin login)
            login.UpdateLoginAppearance();
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloLogin login)
            login.UpdateLoginAppearance();
    }

    private static void OnAuthenticationStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloLogin login)
            login.UpdateAuthenticationUI();
    }

    private static void OnEnableBackgroundAnimationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloLogin login)
        {
            if ((bool)e.NewValue && !login.IsInSimplifiedMode)
                login._backgroundTimer.Start();
            else
                login._backgroundTimer.Stop();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloLogin login)
        {
            if ((bool)e.NewValue && !login.IsInSimplifiedMode)
                login._particleTimer.Start();
            else
                login._particleTimer.Stop();
        }
    }

    #endregion
}

/// <summary>
/// Login particle for visual effects
/// </summary>
internal class LoginParticle
{
    public FrameworkElement Visual { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
}

/// <summary>
/// Background element for atmospheric effects
/// </summary>
internal class BackgroundElement
{
    public FrameworkElement Visual { get; set; }
    public double BaseX { get; set; }
    public double BaseY { get; set; }
    public double Amplitude { get; set; }
    public double Speed { get; set; }
    public double Phase { get; set; }
}

/// <summary>
/// Authentication states
/// </summary>
public enum AuthState
{
    Disconnected,
    Authenticating,
    Authenticated,
    Failed
}

/// <summary>
/// Event args for login events
/// </summary>
public class HoloLoginEventArgs : EventArgs
{
    public AuthState State { get; set; }
    public string Message { get; set; }
}