// ==========================================================================
// HoloLoginInterface.cs - Holographic Login Interface with Westworld Aesthetics
// ==========================================================================
// Comprehensive authentication interface featuring Westworld-inspired holographic
// login system with animated panels, particle effects, and EVE Online integration.
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
/// Holographic login interface with Westworld-inspired aesthetics and EVE Online authentication
/// </summary>
public class HoloLoginInterface : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloLoginInterface),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloLoginInterface),
            new PropertyMetadata(EVEColorScheme.VoidPurple, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty AuthenticationStatusProperty =
        DependencyProperty.Register(nameof(AuthenticationStatus), typeof(AuthenticationStatus), typeof(HoloLoginInterface),
            new PropertyMetadata(AuthenticationStatus.Disconnected, OnAuthenticationStatusChanged));

    public static readonly DependencyProperty LoginStateProperty =
        DependencyProperty.Register(nameof(LoginState), typeof(LoginState), typeof(HoloLoginInterface),
            new PropertyMetadata(LoginState.Welcome, OnLoginStateChanged));

    public static readonly DependencyProperty EnableLoginAnimationsProperty =
        DependencyProperty.Register(nameof(EnableLoginAnimations), typeof(bool), typeof(HoloLoginInterface),
            new PropertyMetadata(true, OnEnableLoginAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloLoginInterface),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty EnableAudioFeedbackProperty =
        DependencyProperty.Register(nameof(EnableAudioFeedback), typeof(bool), typeof(HoloLoginInterface),
            new PropertyMetadata(true));

    public static readonly DependencyProperty ShowWelcomeAnimationProperty =
        DependencyProperty.Register(nameof(ShowWelcomeAnimation), typeof(bool), typeof(HoloLoginInterface),
            new PropertyMetadata(true));

    public static readonly DependencyProperty ConnectionStatusProperty =
        DependencyProperty.Register(nameof(ConnectionStatus), typeof(string), typeof(HoloLoginInterface),
            new PropertyMetadata("Initializing..."));

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

    public AuthenticationStatus AuthenticationStatus
    {
        get => (AuthenticationStatus)GetValue(AuthenticationStatusProperty);
        set => SetValue(AuthenticationStatusProperty, value);
    }

    public LoginState LoginState
    {
        get => (LoginState)GetValue(LoginStateProperty);
        set => SetValue(LoginStateProperty, value);
    }

    public bool EnableLoginAnimations
    {
        get => (bool)GetValue(EnableLoginAnimationsProperty);
        set => SetValue(EnableLoginAnimationsProperty, value);
    }

    public bool EnableParticleEffects
    {
        get => (bool)GetValue(EnableParticleEffectsProperty);
        set => SetValue(EnableParticleEffectsProperty, value);
    }

    public bool EnableAudioFeedback
    {
        get => (bool)GetValue(EnableAudioFeedbackProperty);
        set => SetValue(EnableAudioFeedbackProperty, value);
    }

    public bool ShowWelcomeAnimation
    {
        get => (bool)GetValue(ShowWelcomeAnimationProperty);
        set => SetValue(ShowWelcomeAnimationProperty, value);
    }

    public string ConnectionStatus
    {
        get => (string)GetValue(ConnectionStatusProperty);
        set => SetValue(ConnectionStatusProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloLoginEventArgs> LoginRequested;
    public event EventHandler<HoloLoginEventArgs> LogoutRequested;
    public event EventHandler<HoloLoginEventArgs> AuthenticationCompleted;
    public event EventHandler<HoloLoginEventArgs> AuthenticationFailed;
    public event EventHandler<HoloLoginEventArgs> ConnectionStatusChanged;
    public event EventHandler<HoloLoginEventArgs> WelcomeAnimationCompleted;

    #endregion

    #region Private Fields

    private readonly DispatcherTimer _animationTimer;
    private readonly DispatcherTimer _connectionTimer;
    private readonly Random _random = new();
    private Canvas _mainCanvas;
    private Grid _loginPanel;
    private Grid _welcomePanel;
    private Grid _authenticationPanel;
    private TextBlock _titleText;
    private TextBlock _subtitleText;
    private TextBlock _statusText;
    private Button _loginButton;
    private Button _disconnectButton;
    private ProgressBar _connectionProgress;
    private Canvas _particleCanvas;
    private Canvas _backgroundCanvas;
    private readonly List<UIElement> _particles = new();
    private readonly List<UIElement> _backgroundElements = new();
    private bool _isSimplifiedMode;
    private bool _isAuthenticating;
    private Storyboard _welcomeStoryboard;
    private Storyboard _loginStoryboard;

    #endregion

    #region Constructor

    public HoloLoginInterface()
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
        Width = 800;
        Height = 600;
        Background = new SolidColorBrush(Color.FromArgb(30, 100, 50, 150));
        BorderBrush = new SolidColorBrush(Color.FromArgb(100, 100, 50, 150));
        BorderThickness = new Thickness(2);
        Effect = new DropShadowEffect
        {
            Color = Color.FromArgb(80, 100, 50, 150),
            BlurRadius = 15,
            ShadowDepth = 5,
            Direction = 315
        };

        CreateMainLayout();
        ApplyEVEColorScheme();
    }

    private void CreateMainLayout()
    {
        _mainCanvas = new Canvas();
        Content = _mainCanvas;

        CreateBackgroundCanvas();
        CreateMainPanels();
        CreateParticleCanvas();
    }

    private void CreateBackgroundCanvas()
    {
        _backgroundCanvas = new Canvas
        {
            Width = 800,
            Height = 600,
            IsHitTestVisible = false
        };
        _mainCanvas.Children.Add(_backgroundCanvas);

        // Animated background grid
        CreateBackgroundGrid();
        
        // Holographic overlays
        CreateHolographicOverlays();
    }

    private void CreateBackgroundGrid()
    {
        var gridSize = 50;
        var opacity = 0.1;

        // Vertical lines
        for (int x = 0; x < 800; x += gridSize)
        {
            var line = new Line
            {
                X1 = x,
                Y1 = 0,
                X2 = x,
                Y2 = 600,
                Stroke = new SolidColorBrush(Color.FromArgb((byte)(255 * opacity), 100, 200, 255)),
                StrokeThickness = 1
            };
            _backgroundCanvas.Children.Add(line);
            _backgroundElements.Add(line);
        }

        // Horizontal lines
        for (int y = 0; y < 600; y += gridSize)
        {
            var line = new Line
            {
                X1 = 0,
                Y1 = y,
                X2 = 800,
                Y2 = y,
                Stroke = new SolidColorBrush(Color.FromArgb((byte)(255 * opacity), 100, 200, 255)),
                StrokeThickness = 1
            };
            _backgroundCanvas.Children.Add(line);
            _backgroundElements.Add(line);
        }
    }

    private void CreateHolographicOverlays()
    {
        // Corner decorations
        var cornerDecoration = new Path
        {
            Data = Geometry.Parse("M 0,0 L 50,0 L 50,10 L 10,10 L 10,50 L 0,50 Z"),
            Fill = new SolidColorBrush(Color.FromArgb(60, 100, 200, 255)),
            Stroke = new SolidColorBrush(Color.FromArgb(120, 100, 200, 255)),
            StrokeThickness = 1
        };
        Canvas.SetLeft(cornerDecoration, 20);
        Canvas.SetTop(cornerDecoration, 20);
        _backgroundCanvas.Children.Add(cornerDecoration);
        _backgroundElements.Add(cornerDecoration);

        // Mirror corner decoration in other corners
        for (int i = 0; i < 3; i++)
        {
            var corner = new Path
            {
                Data = cornerDecoration.Data,
                Fill = cornerDecoration.Fill,
                Stroke = cornerDecoration.Stroke,
                StrokeThickness = cornerDecoration.StrokeThickness
            };

            switch (i)
            {
                case 0: // Top right
                    corner.RenderTransform = new ScaleTransform(-1, 1);
                    Canvas.SetLeft(corner, 780);
                    Canvas.SetTop(corner, 20);
                    break;
                case 1: // Bottom left
                    corner.RenderTransform = new ScaleTransform(1, -1);
                    Canvas.SetLeft(corner, 20);
                    Canvas.SetTop(corner, 580);
                    break;
                case 2: // Bottom right
                    corner.RenderTransform = new ScaleTransform(-1, -1);
                    Canvas.SetLeft(corner, 780);
                    Canvas.SetTop(corner, 580);
                    break;
            }

            _backgroundCanvas.Children.Add(corner);
            _backgroundElements.Add(corner);
        }
    }

    private void CreateMainPanels()
    {
        var mainContainer = new Grid
        {
            Width = 800,
            Height = 600
        };
        _mainCanvas.Children.Add(mainContainer);

        CreateWelcomePanel(mainContainer);
        CreateLoginPanel(mainContainer);
        CreateAuthenticationPanel(mainContainer);
    }

    private void CreateWelcomePanel(Grid container)
    {
        _welcomePanel = new Grid
        {
            Visibility = LoginState == LoginState.Welcome ? Visibility.Visible : Visibility.Collapsed
        };
        container.Children.Add(_welcomePanel);

        var welcomeContainer = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        _welcomePanel.Children.Add(welcomeContainer);

        // Main title with holographic effect
        _titleText = new TextBlock
        {
            Text = "GIDEON",
            FontSize = 72,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 100, 200, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(120, 100, 200, 255),
                BlurRadius = 10,
                ShadowDepth = 3
            }
        };
        welcomeContainer.Children.Add(_titleText);

        // Subtitle
        _subtitleText = new TextBlock
        {
            Text = "EVE ONLINE AI COPILOT",
            FontSize = 24,
            FontWeight = FontWeights.Normal,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 150, 150, 200)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 10, 0, 40)
        };
        welcomeContainer.Children.Add(_subtitleText);

        // Status text
        _statusText = new TextBlock
        {
            Text = ConnectionStatus,
            FontSize = 16,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 200, 200, 200)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 20)
        };
        welcomeContainer.Children.Add(_statusText);

        // Connection progress
        _connectionProgress = new ProgressBar
        {
            Width = 300,
            Height = 4,
            Margin = new Thickness(0, 0, 0, 30),
            Background = new SolidColorBrush(Color.FromArgb(40, 100, 200, 255)),
            Foreground = new SolidColorBrush(Color.FromArgb(180, 100, 200, 255)),
            BorderThickness = new Thickness(0),
            IsIndeterminate = true
        };
        welcomeContainer.Children.Add(_connectionProgress);
    }

    private void CreateLoginPanel(Grid container)
    {
        _loginPanel = new Grid
        {
            Visibility = LoginState == LoginState.Login ? Visibility.Visible : Visibility.Collapsed
        };
        container.Children.Add(_loginPanel);

        var loginContainer = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Width = 400
        };
        _loginPanel.Children.Add(loginContainer);

        // Login panel background
        var loginBackground = new Border
        {
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(60, 100, 200, 255), 0),
                    new GradientStop(Color.FromArgb(20, 50, 100, 200), 0.5),
                    new GradientStop(Color.FromArgb(40, 100, 200, 255), 1)
                }
            },
            BorderBrush = new SolidColorBrush(Color.FromArgb(120, 100, 200, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Padding = new Thickness(40),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(80, 100, 200, 255),
                BlurRadius = 15,
                ShadowDepth = 5
            }
        };
        loginContainer.Children.Add(loginBackground);

        var loginContent = new StackPanel();
        loginBackground.Child = loginContent;

        // Login title
        var loginTitle = new TextBlock
        {
            Text = "EVE ONLINE AUTHENTICATION",
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 100, 200, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 30)
        };
        loginContent.Children.Add(loginTitle);

        // Authentication description
        var authDescription = new TextBlock
        {
            Text = "Gideon will securely connect to EVE Online's ESI API using OAuth2 authentication. Your credentials are never stored by Gideon.",
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 200, 200)),
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 30)
        };
        loginContent.Children.Add(authDescription);

        // Login button
        _loginButton = new Button
        {
            Content = "CONNECT TO EVE ONLINE",
            Width = 250,
            Height = 50,
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 20),
            Style = CreateHolographicButtonStyle()
        };
        _loginButton.Click += OnLoginButtonClicked;
        loginContent.Children.Add(_loginButton);

        // Additional options
        var optionsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        loginContent.Children.Add(optionsPanel);

        var offlineButton = new Button
        {
            Content = "USE OFFLINE MODE",
            Width = 120,
            Height = 30,
            FontSize = 12,
            Margin = new Thickness(0, 0, 10, 0),
            Style = CreateSecondaryButtonStyle()
        };
        offlineButton.Click += OnOfflineModeClicked;
        optionsPanel.Children.Add(offlineButton);

        var helpButton = new Button
        {
            Content = "HELP",
            Width = 60,
            Height = 30,
            FontSize = 12,
            Style = CreateSecondaryButtonStyle()
        };
        helpButton.Click += OnHelpButtonClicked;
        optionsPanel.Children.Add(helpButton);
    }

    private void CreateAuthenticationPanel(Grid container)
    {
        _authenticationPanel = new Grid
        {
            Visibility = LoginState == LoginState.Authenticating ? Visibility.Visible : Visibility.Collapsed
        };
        container.Children.Add(_authenticationPanel);

        var authContainer = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        _authenticationPanel.Children.Add(authContainer);

        // Authentication status
        var authTitle = new TextBlock
        {
            Text = "AUTHENTICATING...",
            FontSize = 32,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 100, 200, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 20),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(120, 100, 200, 255),
                BlurRadius = 8,
                ShadowDepth = 2
            }
        };
        authContainer.Children.Add(authTitle);

        // Authentication progress
        var authProgress = new ProgressBar
        {
            Width = 400,
            Height = 6,
            Margin = new Thickness(0, 0, 0, 20),
            Background = new SolidColorBrush(Color.FromArgb(40, 100, 200, 255)),
            Foreground = new SolidColorBrush(Color.FromArgb(180, 100, 200, 255)),
            BorderThickness = new Thickness(0),
            IsIndeterminate = true
        };
        authContainer.Children.Add(authProgress);

        // Auth status text
        var authStatusText = new TextBlock
        {
            Text = "Please complete authentication in your web browser...",
            FontSize = 16,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 200, 200, 200)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 30)
        };
        authContainer.Children.Add(authStatusText);

        // Cancel button
        var cancelButton = new Button
        {
            Content = "CANCEL",
            Width = 100,
            Height = 35,
            FontSize = 14,
            Style = CreateSecondaryButtonStyle()
        };
        cancelButton.Click += OnCancelAuthenticationClicked;
        authContainer.Children.Add(cancelButton);
    }

    private void CreateParticleCanvas()
    {
        _particleCanvas = new Canvas
        {
            Width = 800,
            Height = 600,
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

        _connectionTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _connectionTimer.Tick += OnConnectionTimerTick;
    }

    #endregion

    #region Authentication Flow

    public async Task StartAuthenticationAsync()
    {
        if (_isAuthenticating) return;

        _isAuthenticating = true;
        LoginState = LoginState.Authenticating;

        if (EnableLoginAnimations && !_isSimplifiedMode)
        {
            await AnimateToAuthenticationState();
        }

        if (EnableParticleEffects && !_isSimplifiedMode)
        {
            SpawnAuthenticationParticles();
        }

        LoginRequested?.Invoke(this, new HoloLoginEventArgs
        {
            AuthenticationMethod = "ESI_OAuth2",
            Timestamp = DateTime.Now
        });
    }

    public async Task CompleteAuthenticationAsync(HoloAuthenticationResult result)
    {
        _isAuthenticating = false;

        if (result.IsSuccess)
        {
            AuthenticationStatus = AuthenticationStatus.Connected;
            
            if (EnableLoginAnimations && !_isSimplifiedMode)
            {
                await AnimateAuthenticationSuccess();
            }

            if (EnableParticleEffects && !_isSimplifiedMode)
            {
                SpawnSuccessParticles();
            }

            AuthenticationCompleted?.Invoke(this, new HoloLoginEventArgs
            {
                IsSuccess = true,
                CharacterName = result.CharacterName,
                CorporationName = result.CorporationName,
                Timestamp = DateTime.Now
            });
        }
        else
        {
            AuthenticationStatus = AuthenticationStatus.Error;
            LoginState = LoginState.Login;

            if (EnableLoginAnimations && !_isSimplifiedMode)
            {
                await AnimateAuthenticationError();
            }

            if (EnableParticleEffects && !_isSimplifiedMode)
            {
                SpawnErrorParticles();
            }

            AuthenticationFailed?.Invoke(this, new HoloLoginEventArgs
            {
                IsSuccess = false,
                ErrorMessage = result.ErrorMessage,
                Timestamp = DateTime.Now
            });
        }
    }

    public async Task DisconnectAsync()
    {
        AuthenticationStatus = AuthenticationStatus.Disconnected;
        LoginState = LoginState.Login;

        if (EnableLoginAnimations && !_isSimplifiedMode)
        {
            await AnimateToLoginState();
        }

        LogoutRequested?.Invoke(this, new HoloLoginEventArgs
        {
            Timestamp = DateTime.Now
        });
    }

    #endregion

    #region Animation Methods

    private async Task PlayWelcomeAnimation()
    {
        if (_isSimplifiedMode || !EnableLoginAnimations) return;

        _welcomeStoryboard = new Storyboard();

        // Title animation
        var titleFadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(1000),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(titleFadeIn, _titleText);
        Storyboard.SetTargetProperty(titleFadeIn, new PropertyPath(OpacityProperty));
        _welcomeStoryboard.Children.Add(titleFadeIn);

        var titleSlideIn = new DoubleAnimation
        {
            From = -100,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(1000),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        var titleTransform = new TranslateTransform();
        _titleText.RenderTransform = titleTransform;
        Storyboard.SetTarget(titleSlideIn, titleTransform);
        Storyboard.SetTargetProperty(titleSlideIn, new PropertyPath(TranslateTransform.YProperty));
        _welcomeStoryboard.Children.Add(titleSlideIn);

        // Subtitle animation (delayed)
        var subtitleFadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(800),
            BeginTime = TimeSpan.FromMilliseconds(500),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(subtitleFadeIn, _subtitleText);
        Storyboard.SetTargetProperty(subtitleFadeIn, new PropertyPath(OpacityProperty));
        _welcomeStoryboard.Children.Add(subtitleFadeIn);

        _titleText.Opacity = 0;
        _subtitleText.Opacity = 0;

        _welcomeStoryboard.Completed += (s, e) =>
        {
            WelcomeAnimationCompleted?.Invoke(this, new HoloLoginEventArgs
            {
                Timestamp = DateTime.Now
            });
        };

        _welcomeStoryboard.Begin();
        await Task.Delay(2000);
    }

    private async Task AnimateToLoginState()
    {
        if (_isSimplifiedMode || !EnableLoginAnimations) return;

        var storyboard = new Storyboard();

        // Fade out current panel
        var fadeOut = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(300)
        };
        Storyboard.SetTarget(fadeOut, _welcomePanel.Visibility == Visibility.Visible ? _welcomePanel : _authenticationPanel);
        Storyboard.SetTargetProperty(fadeOut, new PropertyPath(OpacityProperty));
        storyboard.Children.Add(fadeOut);

        storyboard.Completed += (s, e) =>
        {
            _welcomePanel.Visibility = Visibility.Collapsed;
            _authenticationPanel.Visibility = Visibility.Collapsed;
            _loginPanel.Visibility = Visibility.Visible;
            AnimateLoginPanelIn();
        };

        storyboard.Begin();
        await Task.Delay(300);
    }

    private void AnimateLoginPanelIn()
    {
        if (_isSimplifiedMode || !EnableLoginAnimations) return;

        var storyboard = new Storyboard();

        var fadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(500),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(fadeIn, _loginPanel);
        Storyboard.SetTargetProperty(fadeIn, new PropertyPath(OpacityProperty));
        storyboard.Children.Add(fadeIn);

        var slideIn = new DoubleAnimation
        {
            From = 50,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(500),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        var transform = new TranslateTransform();
        _loginPanel.RenderTransform = transform;
        Storyboard.SetTarget(slideIn, transform);
        Storyboard.SetTargetProperty(slideIn, new PropertyPath(TranslateTransform.YProperty));
        storyboard.Children.Add(slideIn);

        _loginPanel.Opacity = 0;
        storyboard.Begin();
    }

    private async Task AnimateToAuthenticationState()
    {
        if (_isSimplifiedMode || !EnableLoginAnimations) return;

        var storyboard = new Storyboard();

        var fadeOut = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(300)
        };
        Storyboard.SetTarget(fadeOut, _loginPanel);
        Storyboard.SetTargetProperty(fadeOut, new PropertyPath(OpacityProperty));
        storyboard.Children.Add(fadeOut);

        storyboard.Completed += (s, e) =>
        {
            _loginPanel.Visibility = Visibility.Collapsed;
            _authenticationPanel.Visibility = Visibility.Visible;
            AnimateAuthenticationPanelIn();
        };

        storyboard.Begin();
        await Task.Delay(300);
    }

    private void AnimateAuthenticationPanelIn()
    {
        if (_isSimplifiedMode || !EnableLoginAnimations) return;

        var storyboard = new Storyboard();

        var fadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(500),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(fadeIn, _authenticationPanel);
        Storyboard.SetTargetProperty(fadeIn, new PropertyPath(OpacityProperty));
        storyboard.Children.Add(fadeIn);

        _authenticationPanel.Opacity = 0;
        storyboard.Begin();
    }

    private async Task AnimateAuthenticationSuccess()
    {
        if (_isSimplifiedMode || !EnableLoginAnimations) return;

        // Flash green effect
        var flashEffect = new SolidColorBrush(Color.FromArgb(100, 0, 255, 0));
        var originalBackground = Background;

        Background = flashEffect;

        var fadeAnimation = new DoubleAnimation
        {
            From = 100,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(1000)
        };

        flashEffect.BeginAnimation(SolidColorBrush.ColorProperty, 
            new ColorAnimation
            {
                From = Color.FromArgb(100, 0, 255, 0),
                To = Color.FromArgb(0, 0, 255, 0),
                Duration = TimeSpan.FromMilliseconds(1000)
            });

        await Task.Delay(1000);
        Background = originalBackground;
    }

    private async Task AnimateAuthenticationError()
    {
        if (_isSimplifiedMode || !EnableLoginAnimations) return;

        // Shake animation
        var transform = new TranslateTransform();
        _authenticationPanel.RenderTransform = transform;

        var shakeAnimation = new DoubleAnimationUsingKeyFrames
        {
            Duration = TimeSpan.FromMilliseconds(500)
        };

        shakeAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(10, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(100))));
        shakeAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(-10, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200))));
        shakeAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(5, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300))));
        shakeAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(-5, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(400))));
        shakeAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500))));

        transform.BeginAnimation(TranslateTransform.XProperty, shakeAnimation);

        // Flash red effect
        var flashEffect = new SolidColorBrush(Color.FromArgb(80, 255, 0, 0));
        var originalBackground = Background;

        Background = flashEffect;

        flashEffect.BeginAnimation(SolidColorBrush.ColorProperty,
            new ColorAnimation
            {
                From = Color.FromArgb(80, 255, 0, 0),
                To = Color.FromArgb(0, 255, 0, 0),
                Duration = TimeSpan.FromMilliseconds(800)
            });

        await Task.Delay(800);
        Background = originalBackground;
    }

    #endregion

    #region Particle Effects

    private void SpawnWelcomeParticles()
    {
        if (_isSimplifiedMode || !EnableParticleEffects) return;

        for (int i = 0; i < 30; i++)
        {
            var particle = new Ellipse
            {
                Width = _random.Next(2, 6),
                Height = _random.Next(2, 6),
                Fill = new SolidColorBrush(Color.FromArgb(
                    (byte)_random.Next(100, 255),
                    (byte)_random.Next(80, 150),
                    (byte)_random.Next(150, 255),
                    (byte)_random.Next(200, 255)))
            };

            Canvas.SetLeft(particle, _random.Next(0, 800));
            Canvas.SetTop(particle, _random.Next(0, 600));
            _particleCanvas.Children.Add(particle);
            _particles.Add(particle);

            AnimateWelcomeParticle(particle);
        }

        CleanupParticles();
    }

    private void SpawnAuthenticationParticles()
    {
        if (_isSimplifiedMode || !EnableParticleEffects) return;

        for (int i = 0; i < 20; i++)
        {
            var particle = new Rectangle
            {
                Width = _random.Next(1, 4),
                Height = _random.Next(10, 30),
                Fill = new SolidColorBrush(Color.FromArgb(
                    (byte)_random.Next(120, 255),
                    100, 200, 255))
            };

            Canvas.SetLeft(particle, _random.Next(0, 800));
            Canvas.SetTop(particle, 600);
            _particleCanvas.Children.Add(particle);
            _particles.Add(particle);

            AnimateAuthenticationParticle(particle);
        }
    }

    private void SpawnSuccessParticles()
    {
        if (_isSimplifiedMode || !EnableParticleEffects) return;

        var centerX = 400;
        var centerY = 300;

        for (int i = 0; i < 50; i++)
        {
            var particle = new Ellipse
            {
                Width = _random.Next(3, 8),
                Height = _random.Next(3, 8),
                Fill = new SolidColorBrush(Color.FromArgb(
                    (byte)_random.Next(150, 255),
                    (byte)_random.Next(50, 150),
                    255,
                    (byte)_random.Next(50, 150)))
            };

            Canvas.SetLeft(particle, centerX);
            Canvas.SetTop(particle, centerY);
            _particleCanvas.Children.Add(particle);
            _particles.Add(particle);

            AnimateSuccessParticle(particle, centerX, centerY);
        }
    }

    private void SpawnErrorParticles()
    {
        if (_isSimplifiedMode || !EnableParticleEffects) return;

        for (int i = 0; i < 25; i++)
        {
            var particle = new Polygon
            {
                Points = new PointCollection { new Point(0, 0), new Point(5, 8), new Point(-5, 8) },
                Fill = new SolidColorBrush(Color.FromArgb(
                    (byte)_random.Next(150, 255),
                    255,
                    (byte)_random.Next(50, 100),
                    (byte)_random.Next(50, 100)))
            };

            Canvas.SetLeft(particle, _random.Next(300, 500));
            Canvas.SetTop(particle, _random.Next(250, 350));
            _particleCanvas.Children.Add(particle);
            _particles.Add(particle);

            AnimateErrorParticle(particle);
        }
    }

    private void AnimateWelcomeParticle(UIElement particle)
    {
        var floatAnimation = new DoubleAnimation
        {
            From = Canvas.GetTop(particle),
            To = Canvas.GetTop(particle) - _random.Next(100, 300),
            Duration = TimeSpan.FromMilliseconds(_random.Next(3000, 6000)),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(_random.Next(3000, 6000))
        };

        floatAnimation.Completed += (s, e) =>
        {
            _particleCanvas.Children.Remove(particle);
            _particles.Remove(particle);
        };

        particle.BeginAnimation(Canvas.TopProperty, floatAnimation);
        particle.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void AnimateAuthenticationParticle(UIElement particle)
    {
        var riseAnimation = new DoubleAnimation
        {
            From = 600,
            To = -50,
            Duration = TimeSpan.FromMilliseconds(_random.Next(2000, 4000)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(500)
        };

        var fadeOutAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(500),
            BeginTime = TimeSpan.FromMilliseconds(_random.Next(1500, 3500))
        };

        riseAnimation.Completed += (s, e) =>
        {
            _particleCanvas.Children.Remove(particle);
            _particles.Remove(particle);
        };

        particle.BeginAnimation(Canvas.TopProperty, riseAnimation);
        particle.BeginAnimation(OpacityProperty, fadeAnimation);
        particle.BeginAnimation(OpacityProperty, fadeOutAnimation);
    }

    private void AnimateSuccessParticle(UIElement particle, double centerX, double centerY)
    {
        var angle = _random.NextDouble() * Math.PI * 2;
        var distance = _random.Next(100, 200);
        var targetX = centerX + Math.Cos(angle) * distance;
        var targetY = centerY + Math.Sin(angle) * distance;

        var moveXAnimation = new DoubleAnimation
        {
            From = centerX,
            To = targetX,
            Duration = TimeSpan.FromMilliseconds(_random.Next(1000, 2000)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var moveYAnimation = new DoubleAnimation
        {
            From = centerY,
            To = targetY,
            Duration = TimeSpan.FromMilliseconds(_random.Next(1000, 2000)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(_random.Next(1000, 2000))
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

    private void AnimateErrorParticle(UIElement particle)
    {
        var fallAnimation = new DoubleAnimation
        {
            From = Canvas.GetTop(particle),
            To = Canvas.GetTop(particle) + _random.Next(100, 200),
            Duration = TimeSpan.FromMilliseconds(_random.Next(1000, 2000)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        var rotateAnimation = new DoubleAnimation
        {
            From = 0,
            To = _random.Next(-360, 360),
            Duration = TimeSpan.FromMilliseconds(_random.Next(1000, 2000))
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(_random.Next(1000, 2000))
        };

        var rotateTransform = new RotateTransform();
        particle.RenderTransform = rotateTransform;

        fallAnimation.Completed += (s, e) =>
        {
            _particleCanvas.Children.Remove(particle);
            _particles.Remove(particle);
        };

        particle.BeginAnimation(Canvas.TopProperty, fallAnimation);
        rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
        particle.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void CleanupParticles()
    {
        if (_particles.Count > 200)
        {
            var particlesToRemove = _particles.Take(100).ToList();
            foreach (var particle in particlesToRemove)
            {
                _particleCanvas.Children.Remove(particle);
                _particles.Remove(particle);
            }
        }
    }

    #endregion

    #region UI Style Helpers

    private Style CreateHolographicButtonStyle()
    {
        var style = new Style(typeof(Button));
        
        style.Setters.Add(new Setter(BackgroundProperty, new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(0, 1),
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Color.FromArgb(80, 100, 200, 255), 0),
                new GradientStop(Color.FromArgb(40, 50, 150, 255), 1)
            }
        }));
        
        style.Setters.Add(new Setter(BorderBrushProperty, new SolidColorBrush(Color.FromArgb(150, 100, 200, 255))));
        style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(ForegroundProperty, new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))));
        style.Setters.Add(new Setter(EffectProperty, new DropShadowEffect
        {
            Color = Color.FromArgb(80, 100, 200, 255),
            BlurRadius = 8,
            ShadowDepth = 3
        }));

        return style;
    }

    private Style CreateSecondaryButtonStyle()
    {
        var style = new Style(typeof(Button));
        
        style.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Color.FromArgb(40, 100, 200, 255))));
        style.Setters.Add(new Setter(BorderBrushProperty, new SolidColorBrush(Color.FromArgb(100, 100, 200, 255))));
        style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(ForegroundProperty, new SolidColorBrush(Color.FromArgb(200, 200, 200, 200))));

        return style;
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (ShowWelcomeAnimation && LoginState == LoginState.Welcome)
        {
            Task.Run(PlayWelcomeAnimation);
        }

        if (EnableParticleEffects && !_isSimplifiedMode)
        {
            SpawnWelcomeParticles();
        }

        _animationTimer.Start();
        _connectionTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        _connectionTimer?.Stop();
        CleanupAllParticles();
    }

    private void OnAnimationTimerTick(object sender, EventArgs e)
    {
        if (EnableParticleEffects && !_isSimplifiedMode && LoginState == LoginState.Welcome)
        {
            if (_random.NextDouble() < 0.1) // 10% chance per tick
            {
                SpawnWelcomeParticles();
            }
        }

        // Animate background grid
        AnimateBackgroundElements();
    }

    private void OnConnectionTimerTick(object sender, EventArgs e)
    {
        if (LoginState == LoginState.Welcome)
        {
            // Simulate connection status updates
            var statuses = new[]
            {
                "Initializing...",
                "Connecting to ESI...",
                "Verifying certificates...",
                "Ready for authentication"
            };

            var currentIndex = Array.IndexOf(statuses, ConnectionStatus);
            if (currentIndex < statuses.Length - 1)
            {
                ConnectionStatus = statuses[currentIndex + 1];
                _statusText.Text = ConnectionStatus;
            }
            else
            {
                _connectionTimer.Stop();
                _connectionProgress.IsIndeterminate = false;
                _connectionProgress.Value = 100;
                
                // Auto-transition to login after a delay
                Task.Delay(1000).ContinueWith(_ =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LoginState = LoginState.Login;
                    });
                });
            }
        }
    }

    private async void OnLoginButtonClicked(object sender, RoutedEventArgs e)
    {
        await StartAuthenticationAsync();
    }

    private void OnOfflineModeClicked(object sender, RoutedEventArgs e)
    {
        AuthenticationStatus = AuthenticationStatus.Offline;
        
        AuthenticationCompleted?.Invoke(this, new HoloLoginEventArgs
        {
            IsSuccess = true,
            IsOfflineMode = true,
            Timestamp = DateTime.Now
        });
    }

    private void OnHelpButtonClicked(object sender, RoutedEventArgs e)
    {
        // Open help documentation
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "https://docs.gideon-eve.com/authentication",
            UseShellExecute = true
        });
    }

    private async void OnCancelAuthenticationClicked(object sender, RoutedEventArgs e)
    {
        _isAuthenticating = false;
        LoginState = LoginState.Login;
        
        if (EnableLoginAnimations && !_isSimplifiedMode)
        {
            await AnimateToLoginState();
        }
    }

    #endregion

    #region Helper Methods

    private void AnimateBackgroundElements()
    {
        if (_isSimplifiedMode || !EnableLoginAnimations) return;

        foreach (var element in _backgroundElements)
        {
            if (element is Line line)
            {
                var currentOpacity = line.Opacity;
                var targetOpacity = 0.05 + (0.15 * Math.Sin(DateTime.Now.Millisecond / 1000.0));
                line.Opacity = currentOpacity + (targetOpacity - currentOpacity) * 0.1;
            }
        }
    }

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
        if (d is HoloLoginInterface control)
        {
            control.ApplyHolographicIntensity();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloLoginInterface control)
        {
            control.ApplyEVEColorScheme();
        }
    }

    private static void OnAuthenticationStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloLoginInterface control)
        {
            control.ConnectionStatusChanged?.Invoke(control, new HoloLoginEventArgs
            {
                Status = control.AuthenticationStatus,
                Timestamp = DateTime.Now
            });
        }
    }

    private static void OnLoginStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloLoginInterface control)
        {
            control.UpdateVisibilityForLoginState();
        }
    }

    private static void OnEnableLoginAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Animation state changed
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloLoginInterface control && !(bool)e.NewValue)
        {
            control.CleanupAllParticles();
        }
    }

    #endregion

    #region State Management

    private void UpdateVisibilityForLoginState()
    {
        _welcomePanel.Visibility = LoginState == LoginState.Welcome ? Visibility.Visible : Visibility.Collapsed;
        _loginPanel.Visibility = LoginState == LoginState.Login ? Visibility.Visible : Visibility.Collapsed;
        _authenticationPanel.Visibility = LoginState == LoginState.Authenticating ? Visibility.Visible : Visibility.Collapsed;
    }

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode => _isSimplifiedMode;

    public void EnterSimplifiedMode()
    {
        _isSimplifiedMode = true;
        CleanupAllParticles();
        EnableLoginAnimations = false;
        EnableParticleEffects = false;
    }

    public void ExitSimplifiedMode()
    {
        _isSimplifiedMode = false;
        EnableLoginAnimations = true;
        EnableParticleEffects = true;
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
            shadow.BlurRadius = 15 * intensity;
            shadow.ShadowDepth = 5 * intensity;
        }

        EnableLoginAnimations = intensity > 0.3;
        EnableParticleEffects = intensity > 0.5;
    }

    #endregion
}

#region Supporting Classes and Enums

public enum LoginState
{
    Welcome,
    Login,
    Authenticating,
    Connected,
    Error
}

public enum AuthenticationStatus
{
    Disconnected,
    Connecting,
    Connected,
    Error,
    Offline
}

public class HoloAuthenticationResult
{
    public bool IsSuccess { get; set; }
    public string CharacterName { get; set; }
    public string CorporationName { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorCode { get; set; }
}

public class HoloLoginEventArgs : EventArgs
{
    public bool IsSuccess { get; set; }
    public bool IsOfflineMode { get; set; }
    public string CharacterName { get; set; }
    public string CorporationName { get; set; }
    public string AuthenticationMethod { get; set; }
    public string ErrorMessage { get; set; }
    public AuthenticationStatus Status { get; set; }
    public DateTime Timestamp { get; set; }
}

#endregion