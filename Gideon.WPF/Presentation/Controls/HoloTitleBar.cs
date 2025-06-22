// ==========================================================================
// HoloTitleBar.cs - Holographic Title Bar with Character Info Display
// ==========================================================================
// Advanced holographic title bar featuring character information display,
// EVE-style visual effects, and adaptive performance rendering.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Advanced holographic title bar with character information display and EVE styling
/// </summary>
public class HoloTitleBar : Control, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloTitleBar),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloTitleBar),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty ApplicationTitleProperty =
        DependencyProperty.Register(nameof(ApplicationTitle), typeof(string), typeof(HoloTitleBar),
            new PropertyMetadata("GIDEON"));

    public static readonly DependencyProperty ApplicationSubtitleProperty =
        DependencyProperty.Register(nameof(ApplicationSubtitle), typeof(string), typeof(HoloTitleBar),
            new PropertyMetadata("EVE AI COPILOT"));

    public static readonly DependencyProperty CharacterNameProperty =
        DependencyProperty.Register(nameof(CharacterName), typeof(string), typeof(HoloTitleBar),
            new PropertyMetadata(null, OnCharacterNameChanged));

    public static readonly DependencyProperty CharacterCorporationProperty =
        DependencyProperty.Register(nameof(CharacterCorporation), typeof(string), typeof(HoloTitleBar),
            new PropertyMetadata(null));

    public static readonly DependencyProperty CharacterAllianceProperty =
        DependencyProperty.Register(nameof(CharacterAlliance), typeof(string), typeof(HoloTitleBar),
            new PropertyMetadata(null));

    public static readonly DependencyProperty CharacterPortraitProperty =
        DependencyProperty.Register(nameof(CharacterPortrait), typeof(ImageSource), typeof(HoloTitleBar),
            new PropertyMetadata(null, OnCharacterPortraitChanged));

    public static readonly DependencyProperty ConnectionStatusProperty =
        DependencyProperty.Register(nameof(ConnectionStatus), typeof(ConnectionStatus), typeof(HoloTitleBar),
            new PropertyMetadata(ConnectionStatus.Disconnected, OnConnectionStatusChanged));

    public static readonly DependencyProperty ShowCharacterInfoProperty =
        DependencyProperty.Register(nameof(ShowCharacterInfo), typeof(bool), typeof(HoloTitleBar),
            new PropertyMetadata(true, OnShowCharacterInfoChanged));

    public static readonly DependencyProperty EnableStatusAnimationsProperty =
        DependencyProperty.Register(nameof(EnableStatusAnimations), typeof(bool), typeof(HoloTitleBar),
            new PropertyMetadata(true, OnEnableStatusAnimationsChanged));

    public static readonly DependencyProperty ShowSkillQueueProperty =
        DependencyProperty.Register(nameof(ShowSkillQueue), typeof(bool), typeof(HoloTitleBar),
            new PropertyMetadata(false));

    public static readonly DependencyProperty CurrentSkillProperty =
        DependencyProperty.Register(nameof(CurrentSkill), typeof(string), typeof(HoloTitleBar),
            new PropertyMetadata(null));

    public static readonly DependencyProperty SkillProgressProperty =
        DependencyProperty.Register(nameof(SkillProgress), typeof(double), typeof(HoloTitleBar),
            new PropertyMetadata(0.0, OnSkillProgressChanged));

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

    public string ApplicationTitle
    {
        get => (string)GetValue(ApplicationTitleProperty);
        set => SetValue(ApplicationTitleProperty, value);
    }

    public string ApplicationSubtitle
    {
        get => (string)GetValue(ApplicationSubtitleProperty);
        set => SetValue(ApplicationSubtitleProperty, value);
    }

    public string CharacterName
    {
        get => (string)GetValue(CharacterNameProperty);
        set => SetValue(CharacterNameProperty, value);
    }

    public string CharacterCorporation
    {
        get => (string)GetValue(CharacterCorporationProperty);
        set => SetValue(CharacterCorporationProperty, value);
    }

    public string CharacterAlliance
    {
        get => (string)GetValue(CharacterAllianceProperty);
        set => SetValue(CharacterAllianceProperty, value);
    }

    public ImageSource CharacterPortrait
    {
        get => (ImageSource)GetValue(CharacterPortraitProperty);
        set => SetValue(CharacterPortraitProperty, value);
    }

    public ConnectionStatus ConnectionStatus
    {
        get => (ConnectionStatus)GetValue(ConnectionStatusProperty);
        set => SetValue(ConnectionStatusProperty, value);
    }

    public bool ShowCharacterInfo
    {
        get => (bool)GetValue(ShowCharacterInfoProperty);
        set => SetValue(ShowCharacterInfoProperty, value);
    }

    public bool EnableStatusAnimations
    {
        get => (bool)GetValue(EnableStatusAnimationsProperty);
        set => SetValue(EnableStatusAnimationsProperty, value);
    }

    public bool ShowSkillQueue
    {
        get => (bool)GetValue(ShowSkillQueueProperty);
        set => SetValue(ShowSkillQueueProperty, value);
    }

    public string CurrentSkill
    {
        get => (string)GetValue(CurrentSkillProperty);
        set => SetValue(CurrentSkillProperty, value);
    }

    public double SkillProgress
    {
        get => (double)GetValue(SkillProgressProperty);
        set => SetValue(SkillProgressProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloTitleBarEventArgs> CharacterPortraitClicked;
    public event EventHandler<HoloTitleBarEventArgs> ConnectionStatusClicked;
    public event EventHandler<HoloTitleBarEventArgs> SkillQueueClicked;

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode { get; private set; }

    public void SwitchToFullMode()
    {
        IsInSimplifiedMode = false;
        EnableStatusAnimations = true;
        ShowCharacterInfo = true;
        UpdateTitleBarAppearance();
    }

    public void SwitchToSimplifiedMode()
    {
        IsInSimplifiedMode = true;
        EnableStatusAnimations = false;
        ShowCharacterInfo = false;
        UpdateTitleBarAppearance();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => !_disposed && IsLoaded;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        EnableStatusAnimations = settings.EnabledFeatures.HasFlag(AnimationFeatures.BasicGlow);
        
        UpdateTitleBarAppearance();
    }

    #endregion

    #region Fields

    private Grid _titleBarGrid;
    private StackPanel _appInfoPanel;
    private StackPanel _characterInfoPanel;
    private StackPanel _statusPanel;
    private TextBlock _appTitleText;
    private TextBlock _appSubtitleText;
    private Border _characterPortraitBorder;
    private Image _characterPortraitImage;
    private TextBlock _characterNameText;
    private TextBlock _characterCorpText;
    private Border _connectionIndicator;
    private TextBlock _connectionText;
    private HoloProgressBar _skillProgressBar;
    private TextBlock _skillNameText;
    private Canvas _effectCanvas;
    
    private readonly List<TitleBarParticle> _particles = new();
    private DispatcherTimer _particleTimer;
    private DispatcherTimer _pulseTimer;
    private double _particlePhase = 0;
    private double _pulsePhase = 0;
    private readonly Random _random = new();
    private bool _disposed = false;

    #endregion

    #region Constructor

    public HoloTitleBar()
    {
        DefaultStyleKey = typeof(HoloTitleBar);
        Height = 40;
        InitializeTitleBar();
        SetupAnimations();
        
        // Register with animation intensity manager
        AnimationIntensityManager.Instance.RegisterTarget(this);
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Update character information
    /// </summary>
    public void UpdateCharacterInfo(string name, string corporation, string alliance, ImageSource portrait)
    {
        CharacterName = name;
        CharacterCorporation = corporation;
        CharacterAlliance = alliance;
        CharacterPortrait = portrait;
    }

    /// <summary>
    /// Update skill queue information
    /// </summary>
    public void UpdateSkillQueue(string skillName, double progress)
    {
        CurrentSkill = skillName;
        SkillProgress = Math.Clamp(progress, 0.0, 1.0);
    }

    /// <summary>
    /// Flash title bar for notifications
    /// </summary>
    public void FlashTitleBar(Color color, TimeSpan duration)
    {
        if (IsInSimplifiedMode) return;

        var flashAnimation = new ColorAnimation
        {
            To = color,
            Duration = TimeSpan.FromMilliseconds(100),
            AutoReverse = true,
            RepeatBehavior = new RepeatBehavior(duration.TotalMilliseconds / 200)
        };

        if (Background is SolidColorBrush brush)
        {
            brush.BeginAnimation(SolidColorBrush.ColorProperty, flashAnimation);
        }
    }

    #endregion

    #region Private Methods

    private void InitializeTitleBar()
    {
        Template = CreateTitleBarTemplate();
        UpdateTitleBarAppearance();
    }

    private ControlTemplate CreateTitleBarTemplate()
    {
        var template = new ControlTemplate(typeof(HoloTitleBar));

        // Main grid container
        var titleBarGrid = new FrameworkElementFactory(typeof(Grid));
        titleBarGrid.Name = "PART_TitleBarGrid";

        // Column definitions
        var appColumn = new FrameworkElementFactory(typeof(ColumnDefinition));
        appColumn.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);
        var spacerColumn = new FrameworkElementFactory(typeof(ColumnDefinition));
        spacerColumn.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
        var characterColumn = new FrameworkElementFactory(typeof(ColumnDefinition));
        characterColumn.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);
        var statusColumn = new FrameworkElementFactory(typeof(ColumnDefinition));
        statusColumn.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);

        titleBarGrid.AppendChild(appColumn);
        titleBarGrid.AppendChild(spacerColumn);
        titleBarGrid.AppendChild(characterColumn);
        titleBarGrid.AppendChild(statusColumn);

        // App info panel
        var appInfoPanel = new FrameworkElementFactory(typeof(StackPanel));
        appInfoPanel.Name = "PART_AppInfoPanel";
        appInfoPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        appInfoPanel.SetValue(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Center);
        appInfoPanel.SetValue(StackPanel.MarginProperty, new Thickness(12, 0, 0, 0));
        appInfoPanel.SetValue(Grid.ColumnProperty, 0);

        // Character info panel
        var characterInfoPanel = new FrameworkElementFactory(typeof(StackPanel));
        characterInfoPanel.Name = "PART_CharacterInfoPanel";
        characterInfoPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        characterInfoPanel.SetValue(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Center);
        characterInfoPanel.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        characterInfoPanel.SetValue(Grid.ColumnProperty, 2);

        // Status panel
        var statusPanel = new FrameworkElementFactory(typeof(StackPanel));
        statusPanel.Name = "PART_StatusPanel";
        statusPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        statusPanel.SetValue(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Center);
        statusPanel.SetValue(StackPanel.MarginProperty, new Thickness(0, 0, 12, 0));
        statusPanel.SetValue(Grid.ColumnProperty, 3);

        // Effect canvas
        var effectCanvas = new FrameworkElementFactory(typeof(Canvas));
        effectCanvas.Name = "PART_EffectCanvas";
        effectCanvas.SetValue(Canvas.IsHitTestVisibleProperty, false);
        effectCanvas.SetValue(Canvas.ClipToBoundsProperty, true);
        effectCanvas.SetValue(Grid.ColumnSpanProperty, 4);

        // Assembly
        titleBarGrid.AppendChild(appInfoPanel);
        titleBarGrid.AppendChild(characterInfoPanel);
        titleBarGrid.AppendChild(statusPanel);
        titleBarGrid.AppendChild(effectCanvas);

        template.VisualTree = titleBarGrid;
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

        // Pulse animation timer
        _pulseTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
        };
        _pulseTimer.Tick += OnPulseTick;
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        if (!EnableStatusAnimations || IsInSimplifiedMode || _effectCanvas == null) return;

        _particlePhase += 0.1;
        if (_particlePhase > Math.PI * 2)
            _particlePhase = 0;

        UpdateParticles();
        SpawnTitleBarParticles();
    }

    private void OnPulseTick(object sender, EventArgs e)
    {
        if (!EnableStatusAnimations || IsInSimplifiedMode) return;

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
            particle.Life -= 0.03;

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

    private void SpawnTitleBarParticles()
    {
        if (_particles.Count >= 15) return; // Limit particle count

        if (_random.NextDouble() < 0.1) // 10% chance to spawn
        {
            var particle = CreateTitleBarParticle();
            _particles.Add(particle);
            _effectCanvas.Children.Add(particle.Visual);
        }
    }

    private TitleBarParticle CreateTitleBarParticle()
    {
        var color = GetEVEColor(EVEColorScheme);
        var size = 1 + _random.NextDouble() * 2;
        
        var ellipse = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromArgb(100, color.R, color.G, color.B))
        };

        var particle = new TitleBarParticle
        {
            Visual = ellipse,
            X = -size,
            Y = _random.NextDouble() * ActualHeight,
            VelocityX = 20 + _random.NextDouble() * 30,
            VelocityY = (_random.NextDouble() - 0.5) * 2,
            Life = 1.0
        };

        Canvas.SetLeft(ellipse, particle.X);
        Canvas.SetTop(ellipse, particle.Y);

        return particle;
    }

    private void UpdatePulseEffects()
    {
        // Connection indicator pulse
        if (_connectionIndicator != null && ConnectionStatus == ConnectionStatus.Connected)
        {
            var pulseIntensity = 0.7 + (Math.Sin(_pulsePhase) * 0.3);
            _connectionIndicator.Opacity = pulseIntensity;
        }

        // Character portrait glow
        if (_characterPortraitBorder?.Effect is DropShadowEffect effect)
        {
            var glowIntensity = 0.6 + (Math.Sin(_pulsePhase * 2) * 0.4);
            effect.Opacity = HolographicIntensity * glowIntensity;
        }
    }

    private void UpdateTitleBarAppearance()
    {
        if (Template == null) return;

        Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            ApplyTemplate();
            
            _titleBarGrid = GetTemplateChild("PART_TitleBarGrid") as Grid;
            _appInfoPanel = GetTemplateChild("PART_AppInfoPanel") as StackPanel;
            _characterInfoPanel = GetTemplateChild("PART_CharacterInfoPanel") as StackPanel;
            _statusPanel = GetTemplateChild("PART_StatusPanel") as StackPanel;
            _effectCanvas = GetTemplateChild("PART_EffectCanvas") as Canvas;

            UpdateAppInfo();
            UpdateCharacterDisplay();
            UpdateStatusDisplay();
            UpdateColors();
            UpdateEffects();
        }), DispatcherPriority.Render);
    }

    private void UpdateAppInfo()
    {
        if (_appInfoPanel == null) return;

        _appInfoPanel.Children.Clear();

        // App icon
        var appIcon = new Path
        {
            Width = 24,
            Height = 24,
            Fill = new SolidColorBrush(GetEVEColor(EVEColorScheme)),
            Data = Geometry.Parse("M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M11,16.5L6.5,12L7.91,10.59L11,13.67L16.59,8.09L18,9.5L11,16.5Z"),
            Margin = new Thickness(0, 0, 8, 0)
        };

        if (!IsInSimplifiedMode)
        {
            appIcon.Effect = new DropShadowEffect
            {
                Color = GetEVEColor(EVEColorScheme),
                BlurRadius = 4,
                ShadowDepth = 0,
                Opacity = 0.8 * HolographicIntensity
            };
        }

        _appInfoPanel.Children.Add(appIcon);

        // App title
        _appTitleText = new TextBlock
        {
            Text = ApplicationTitle,
            FontFamily = new FontFamily("Segoe UI"),
            FontWeight = FontWeights.Bold,
            FontSize = 16,
            Foreground = new SolidColorBrush(Colors.White),
            VerticalAlignment = VerticalAlignment.Center
        };
        _appInfoPanel.Children.Add(_appTitleText);

        // App subtitle
        _appSubtitleText = new TextBlock
        {
            Text = ApplicationSubtitle,
            FontFamily = new FontFamily("Segoe UI"),
            FontWeight = FontWeights.Normal,
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 128, 128, 128)),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0)
        };
        _appInfoPanel.Children.Add(_appSubtitleText);
    }

    private void UpdateCharacterDisplay()
    {
        if (_characterInfoPanel == null || !ShowCharacterInfo) return;

        _characterInfoPanel.Children.Clear();

        if (string.IsNullOrEmpty(CharacterName))
        {
            // Not authenticated state
            var authText = new TextBlock
            {
                Text = "Pilot: [Not Authenticated]",
                Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 215, 0)),
                FontWeight = FontWeights.Bold,
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center
            };
            _characterInfoPanel.Children.Add(authText);
            return;
        }

        // Character portrait
        _characterPortraitBorder = new Border
        {
            Width = 28,
            Height = 28,
            CornerRadius = new CornerRadius(14),
            BorderBrush = new SolidColorBrush(GetEVEColor(EVEColorScheme)),
            BorderThickness = new Thickness(2),
            Margin = new Thickness(0, 0, 8, 0),
            Cursor = Cursors.Hand
        };

        if (!IsInSimplifiedMode)
        {
            _characterPortraitBorder.Effect = new DropShadowEffect
            {
                Color = GetEVEColor(EVEColorScheme),
                BlurRadius = 6,
                ShadowDepth = 0,
                Opacity = 0.6 * HolographicIntensity
            };
        }

        _characterPortraitImage = new Image
        {
            Source = CharacterPortrait,
            Stretch = Stretch.UniformToFill
        };

        _characterPortraitBorder.Child = _characterPortraitImage;
        _characterPortraitBorder.MouseLeftButtonUp += OnCharacterPortraitClick;
        _characterInfoPanel.Children.Add(_characterPortraitBorder);

        // Character info stack
        var charInfoStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center
        };

        _characterNameText = new TextBlock
        {
            Text = CharacterName,
            Foreground = new SolidColorBrush(Colors.White),
            FontWeight = FontWeights.Bold,
            FontSize = 12
        };
        charInfoStack.Children.Add(_characterNameText);

        _characterCorpText = new TextBlock
        {
            Text = CharacterCorporation ?? "Unknown Corporation",
            Foreground = new SolidColorBrush(Color.FromArgb(180, 128, 128, 128)),
            FontSize = 9
        };
        charInfoStack.Children.Add(_characterCorpText);

        _characterInfoPanel.Children.Add(charInfoStack);

        // Skill queue (if enabled)
        if (ShowSkillQueue && !string.IsNullOrEmpty(CurrentSkill))
        {
            var skillStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 0, 0),
                Cursor = Cursors.Hand
            };

            _skillNameText = new TextBlock
            {
                Text = CurrentSkill,
                Foreground = new SolidColorBrush(GetEVEColor(EVEColorScheme)),
                FontSize = 10,
                FontWeight = FontWeights.Bold
            };
            skillStack.Children.Add(_skillNameText);

            _skillProgressBar = new HoloProgressBar
            {
                Width = 80,
                Height = 4,
                Value = SkillProgress * 100,
                Maximum = 100,
                HolographicIntensity = HolographicIntensity,
                EVEColorScheme = EVEColorScheme,
                EnableParticleStream = !IsInSimplifiedMode,
                ShowPercentage = false
            };
            skillStack.Children.Add(_skillProgressBar);

            skillStack.MouseLeftButtonUp += OnSkillQueueClick;
            _characterInfoPanel.Children.Add(skillStack);
        }
    }

    private void UpdateStatusDisplay()
    {
        if (_statusPanel == null) return;

        _statusPanel.Children.Clear();

        // Connection status indicator
        _connectionIndicator = new Border
        {
            Width = 12,
            Height = 12,
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(GetConnectionColor(ConnectionStatus)),
            Margin = new Thickness(0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Cursor = Cursors.Hand
        };

        _connectionIndicator.MouseLeftButtonUp += OnConnectionStatusClick;
        _statusPanel.Children.Add(_connectionIndicator);

        // Connection text
        _connectionText = new TextBlock
        {
            Text = GetConnectionText(ConnectionStatus),
            Foreground = new SolidColorBrush(GetConnectionColor(ConnectionStatus)),
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center
        };
        _statusPanel.Children.Add(_connectionText);
    }

    private void UpdateColors()
    {
        var color = GetEVEColor(EVEColorScheme);

        // Update background
        Background = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(0, 1),
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Color.FromArgb(120, 0, 20, 40), 0.0),
                new GradientStop(Color.FromArgb(100, 0, 15, 30), 1.0)
            }
        };
    }

    private void UpdateEffects()
    {
        if (!IsInSimplifiedMode)
        {
            Effect = new DropShadowEffect
            {
                Color = GetEVEColor(EVEColorScheme),
                BlurRadius = 6 * HolographicIntensity,
                ShadowDepth = 0,
                Opacity = 0.4 * HolographicIntensity
            };
        }
        else
        {
            Effect = null;
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

    private Color GetConnectionColor(ConnectionStatus status)
    {
        return status switch
        {
            ConnectionStatus.Connected => Color.FromRgb(50, 205, 50),
            ConnectionStatus.Connecting => Color.FromRgb(255, 215, 0),
            ConnectionStatus.Disconnected => Color.FromRgb(220, 20, 60),
            ConnectionStatus.Error => Color.FromRgb(255, 69, 0),
            _ => Color.FromRgb(128, 128, 128)
        };
    }

    private string GetConnectionText(ConnectionStatus status)
    {
        return status switch
        {
            ConnectionStatus.Connected => "Connected",
            ConnectionStatus.Connecting => "Connecting...",
            ConnectionStatus.Disconnected => "Disconnected",
            ConnectionStatus.Error => "Connection Error",
            _ => "Unknown"
        };
    }

    private void OnCharacterPortraitClick(object sender, MouseButtonEventArgs e)
    {
        CharacterPortraitClicked?.Invoke(this, new HoloTitleBarEventArgs { CharacterName = CharacterName });
    }

    private void OnConnectionStatusClick(object sender, MouseButtonEventArgs e)
    {
        ConnectionStatusClicked?.Invoke(this, new HoloTitleBarEventArgs { ConnectionStatus = ConnectionStatus });
    }

    private void OnSkillQueueClick(object sender, MouseButtonEventArgs e)
    {
        SkillQueueClicked?.Invoke(this, new HoloTitleBarEventArgs { CurrentSkill = CurrentSkill });
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableStatusAnimations && !IsInSimplifiedMode)
        {
            _particleTimer.Start();
            _pulseTimer.Start();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _particleTimer?.Stop();
        _pulseTimer?.Stop();
        
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
        if (d is HoloTitleBar titleBar)
            titleBar.UpdateTitleBarAppearance();
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTitleBar titleBar)
            titleBar.UpdateTitleBarAppearance();
    }

    private static void OnCharacterNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTitleBar titleBar)
            titleBar.UpdateCharacterDisplay();
    }

    private static void OnCharacterPortraitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTitleBar titleBar)
            titleBar.UpdateCharacterDisplay();
    }

    private static void OnConnectionStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTitleBar titleBar)
            titleBar.UpdateStatusDisplay();
    }

    private static void OnShowCharacterInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTitleBar titleBar)
            titleBar.UpdateTitleBarAppearance();
    }

    private static void OnEnableStatusAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTitleBar titleBar)
        {
            if ((bool)e.NewValue && !titleBar.IsInSimplifiedMode)
            {
                titleBar._particleTimer.Start();
                titleBar._pulseTimer.Start();
            }
            else
            {
                titleBar._particleTimer.Stop();
                titleBar._pulseTimer.Stop();
            }
        }
    }

    private static void OnSkillProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloTitleBar titleBar && titleBar._skillProgressBar != null)
        {
            titleBar._skillProgressBar.Value = (double)e.NewValue * 100;
        }
    }

    #endregion
}

/// <summary>
/// Title bar particle for visual effects
/// </summary>
internal class TitleBarParticle
{
    public FrameworkElement Visual { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
}

/// <summary>
/// Event args for holographic title bar events
/// </summary>
public class HoloTitleBarEventArgs : EventArgs
{
    public string CharacterName { get; set; }
    public ConnectionStatus ConnectionStatus { get; set; }
    public string CurrentSkill { get; set; }
}