// ==========================================================================
// HoloCharacterPortrait.cs - Holographic Character Portrait Display System
// ==========================================================================
// Advanced character portrait interface featuring holographic character displays,
// animated avatars, skill visualization, and EVE-style character presentation.
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
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using HelixToolkit.Wpf;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Holographic character portrait display system with animated avatars and skill visualization
/// </summary>
public class HoloCharacterPortrait : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloCharacterPortrait),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloCharacterPortrait),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty CharacterDataProperty =
        DependencyProperty.Register(nameof(CharacterData), typeof(HoloCharacterData), typeof(HoloCharacterPortrait),
            new PropertyMetadata(null, OnCharacterDataChanged));

    public static readonly DependencyProperty DisplayModeProperty =
        DependencyProperty.Register(nameof(DisplayMode), typeof(PortraitDisplayMode), typeof(HoloCharacterPortrait),
            new PropertyMetadata(PortraitDisplayMode.Full, OnDisplayModeChanged));

    public static readonly DependencyProperty Enable3DAvatarProperty =
        DependencyProperty.Register(nameof(Enable3DAvatar), typeof(bool), typeof(HoloCharacterPortrait),
            new PropertyMetadata(true, OnEnable3DAvatarChanged));

    public static readonly DependencyProperty EnablePortraitAnimationsProperty =
        DependencyProperty.Register(nameof(EnablePortraitAnimations), typeof(bool), typeof(HoloCharacterPortrait),
            new PropertyMetadata(true, OnEnablePortraitAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloCharacterPortrait),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowSkillPointsProperty =
        DependencyProperty.Register(nameof(ShowSkillPoints), typeof(bool), typeof(HoloCharacterPortrait),
            new PropertyMetadata(true, OnShowSkillPointsChanged));

    public static readonly DependencyProperty ShowCorporationInfoProperty =
        DependencyProperty.Register(nameof(ShowCorporationInfo), typeof(bool), typeof(HoloCharacterPortrait),
            new PropertyMetadata(true, OnShowCorporationInfoChanged));

    public static readonly DependencyProperty PortraitSizeProperty =
        DependencyProperty.Register(nameof(PortraitSize), typeof(PortraitSize), typeof(HoloCharacterPortrait),
            new PropertyMetadata(PortraitSize.Medium, OnPortraitSizeChanged));

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

    public HoloCharacterData CharacterData
    {
        get => (HoloCharacterData)GetValue(CharacterDataProperty);
        set => SetValue(CharacterDataProperty, value);
    }

    public PortraitDisplayMode DisplayMode
    {
        get => (PortraitDisplayMode)GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    public bool Enable3DAvatar
    {
        get => (bool)GetValue(Enable3DAvatarProperty);
        set => SetValue(Enable3DAvatarProperty, value);
    }

    public bool EnablePortraitAnimations
    {
        get => (bool)GetValue(EnablePortraitAnimationsProperty);
        set => SetValue(EnablePortraitAnimationsProperty, value);
    }

    public bool EnableParticleEffects
    {
        get => (bool)GetValue(EnableParticleEffectsProperty);
        set => SetValue(EnableParticleEffectsProperty, value);
    }

    public bool ShowSkillPoints
    {
        get => (bool)GetValue(ShowSkillPointsProperty);
        set => SetValue(ShowSkillPointsProperty, value);
    }

    public bool ShowCorporationInfo
    {
        get => (bool)GetValue(ShowCorporationInfoProperty);
        set => SetValue(ShowCorporationInfoProperty, value);
    }

    public PortraitSize PortraitSize
    {
        get => (PortraitSize)GetValue(PortraitSizeProperty);
        set => SetValue(PortraitSizeProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloCharacterEventArgs> CharacterSelected;
    public event EventHandler<HoloCharacterEventArgs> CorporationClicked;
    public event EventHandler<HoloCharacterEventArgs> SkillsClicked;
    public event EventHandler<HoloCharacterEventArgs> PortraitClicked;
    public event EventHandler<HoloCharacterEventArgs> CharacterDetailsRequested;

    #endregion

    #region Private Fields

    private readonly DispatcherTimer _animationTimer;
    private readonly Random _random = new();
    private Canvas _mainCanvas;
    private Grid _portraitContainer;
    private Border _portraitFrame;
    private Grid _avatarContainer;
    private Image _characterImage;
    private Viewport3D _avatar3D;
    private ModelVisual3D _characterModel;
    private TextBlock _characterName;
    private TextBlock _corporationName;
    private StackPanel _skillsPanel;
    private TextBlock _skillPointsText;
    private StackPanel _statusIndicators;
    private Border _corporationLogo;
    private Image _corporationImage;
    private Canvas _particleCanvas;
    private Canvas _glowCanvas;
    private readonly List<UIElement> _skillParticles = new();
    private readonly List<UIElement> _backgroundParticles = new();
    private readonly List<UIElement> _glowElements = new();
    private bool _isSimplifiedMode;
    private double _animationPhase;
    private bool _isHovered;
    private DateTime _lastUpdate;

    #endregion

    #region Constructor

    public HoloCharacterPortrait()
    {
        InitializeComponent();
        InitializeTimer();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        MouseEnter += OnMouseEnter;
        MouseLeave += OnMouseLeave;
        MouseLeftButtonDown += OnMouseLeftButtonDown;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        var dimensions = GetPortraitDimensions();
        Width = dimensions.Width;
        Height = dimensions.Height;
        Background = new SolidColorBrush(Colors.Transparent);

        CreateMainLayout();
        ApplyEVEColorScheme();
    }

    private void CreateMainLayout()
    {
        _mainCanvas = new Canvas();
        Content = _mainCanvas;

        // Portrait container
        _portraitContainer = new Grid
        {
            Width = Width,
            Height = Height
        };
        _mainCanvas.Children.Add(_portraitContainer);

        CreatePortraitFrame();
        CreateAvatarDisplay();
        CreateCharacterInfo();
        CreateSkillsDisplay();
        CreateCorporationInfo();
        CreateParticleCanvas();
        CreateGlowCanvas();
    }

    private void CreatePortraitFrame()
    {
        _portraitFrame = new Border
        {
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(2),
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(40, 0, 50, 100), 0),
                    new GradientStop(Color.FromArgb(20, 0, 30, 60), 0.5),
                    new GradientStop(Color.FromArgb(30, 0, 40, 80), 1)
                }
            },
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 100, 200, 255)),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(80, 100, 200, 255),
                BlurRadius = 10,
                ShadowDepth = 2,
                Direction = 315
            }
        };
        _portraitContainer.Children.Add(_portraitFrame);
    }

    private void CreateAvatarDisplay()
    {
        _avatarContainer = new Grid
        {
            Margin = new Thickness(10)
        };

        var avatarDimensions = GetAvatarDimensions();
        _avatarContainer.Width = avatarDimensions.Width;
        _avatarContainer.Height = avatarDimensions.Height;

        if (DisplayMode == PortraitDisplayMode.Full || DisplayMode == PortraitDisplayMode.Compact)
        {
            _avatarContainer.VerticalAlignment = VerticalAlignment.Top;
            _avatarContainer.HorizontalAlignment = HorizontalAlignment.Left;
        }
        else
        {
            _avatarContainer.VerticalAlignment = VerticalAlignment.Center;
            _avatarContainer.HorizontalAlignment = HorizontalAlignment.Center;
        }

        _portraitContainer.Children.Add(_avatarContainer);

        // Character image (2D fallback)
        _characterImage = new Image
        {
            Stretch = Stretch.UniformToFill,
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(60, 255, 255, 255),
                BlurRadius = 5,
                ShadowDepth = 0
            }
        };

        var imageMask = new Border
        {
            CornerRadius = new CornerRadius(avatarDimensions.Width / 2),
            Width = avatarDimensions.Width,
            Height = avatarDimensions.Height,
            Child = _characterImage,
            ClipToBounds = true
        };

        _avatarContainer.Children.Add(imageMask);

        // 3D Avatar viewport (if enabled)
        if (Enable3DAvatar)
        {
            Create3DAvatar();
        }
    }

    private void Create3DAvatar()
    {
        _avatar3D = new Viewport3D
        {
            Width = GetAvatarDimensions().Width,
            Height = GetAvatarDimensions().Height,
            Visibility = Enable3DAvatar ? Visibility.Visible : Visibility.Collapsed
        };

        // Camera setup
        var camera = new PerspectiveCamera
        {
            Position = new Point3D(0, 0, 5),
            LookDirection = new Vector3D(0, 0, -1),
            UpDirection = new Vector3D(0, 1, 0),
            FieldOfView = 45
        };
        _avatar3D.Camera = camera;

        // Lighting
        var ambientLight = new AmbientLight(Colors.White)
        {
            Color = Color.FromRgb(100, 100, 100)
        };

        var directionalLight = new DirectionalLight
        {
            Color = Colors.White,
            Direction = new Vector3D(-1, -1, -1)
        };

        var lightGroup = new Model3DGroup();
        lightGroup.Children.Add(ambientLight);
        lightGroup.Children.Add(directionalLight);

        var lightModel = new ModelVisual3D
        {
            Content = lightGroup
        };
        _avatar3D.Children.Add(lightModel);

        // Character model placeholder
        _characterModel = new ModelVisual3D();
        _avatar3D.Children.Add(_characterModel);

        _avatarContainer.Children.Add(_avatar3D);
    }

    private void CreateCharacterInfo()
    {
        if (DisplayMode == PortraitDisplayMode.AvatarOnly) return;

        var infoPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10)
        };

        var nameDimensions = GetPortraitDimensions();
        if (DisplayMode == PortraitDisplayMode.Full)
        {
            infoPanel.Margin = new Thickness(GetAvatarDimensions().Width + 20, 10, 10, 10);
        }
        else if (DisplayMode == PortraitDisplayMode.Compact)
        {
            infoPanel.Margin = new Thickness(GetAvatarDimensions().Width + 15, 10, 10, 10);
        }
        else // Minimal
        {
            infoPanel.Margin = new Thickness(10, GetAvatarDimensions().Height + 15, 10, 10);
        }

        // Character name
        _characterName = new TextBlock
        {
            FontSize = DisplayMode == PortraitDisplayMode.Full ? 16 : 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
            Text = "Unknown Pilot",
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(100, 100, 200, 255),
                BlurRadius = 3,
                ShadowDepth = 1
            }
        };
        infoPanel.Children.Add(_characterName);

        // Corporation name
        if (ShowCorporationInfo)
        {
            _corporationName = new TextBlock
            {
                FontSize = DisplayMode == PortraitDisplayMode.Full ? 12 : 10,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 200, 200)),
                Text = "Unknown Corporation",
                Margin = new Thickness(0, 2, 0, 0),
                Cursor = Cursors.Hand
            };
            _corporationName.MouseLeftButtonDown += OnCorporationClicked;
            infoPanel.Children.Add(_corporationName);
        }

        _portraitContainer.Children.Add(infoPanel);
    }

    private void CreateSkillsDisplay()
    {
        if (DisplayMode == PortraitDisplayMode.AvatarOnly || DisplayMode == PortraitDisplayMode.Minimal) return;
        if (!ShowSkillPoints) return;

        _skillsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(10)
        };

        // Skill points text
        _skillPointsText = new TextBlock
        {
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 100, 200, 255)),
            Text = "0 SP",
            Cursor = Cursors.Hand
        };
        _skillPointsText.MouseLeftButtonDown += OnSkillsClicked;
        _skillsPanel.Children.Add(_skillPointsText);

        _portraitContainer.Children.Add(_skillsPanel);
    }

    private void CreateCorporationInfo()
    {
        if (!ShowCorporationInfo || DisplayMode == PortraitDisplayMode.AvatarOnly) return;

        _corporationLogo = new Border
        {
            Width = 20,
            Height = 20,
            CornerRadius = new CornerRadius(2),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 10, 10, 0),
            Cursor = Cursors.Hand
        };

        _corporationImage = new Image
        {
            Stretch = Stretch.UniformToFill
        };
        _corporationLogo.Child = _corporationImage;
        _corporationLogo.MouseLeftButtonDown += OnCorporationClicked;

        _portraitContainer.Children.Add(_corporationLogo);
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

    private void CreateGlowCanvas()
    {
        _glowCanvas = new Canvas
        {
            Width = Width,
            Height = Height,
            IsHitTestVisible = false
        };
        _mainCanvas.Children.Add(_glowCanvas);
    }

    private void InitializeTimer()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };
        _animationTimer.Tick += OnAnimationTimerTick;
    }

    #endregion

    #region Character Management

    public void UpdateCharacterData(HoloCharacterData characterData)
    {
        CharacterData = characterData;
        UpdateCharacterDisplay();
    }

    public void RefreshPortrait()
    {
        UpdateCharacterDisplay();
        if (EnablePortraitAnimations && !_isSimplifiedMode)
        {
            AnimatePortraitRefresh();
        }
    }

    public async Task LoadCharacterImageAsync(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl)) return;

        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(imageUrl);
            bitmap.EndInit();

            _characterImage.Source = bitmap;

            if (EnablePortraitAnimations && !_isSimplifiedMode)
            {
                await AnimateImageLoad();
            }
        }
        catch
        {
            // Use placeholder image
            _characterImage.Source = CreatePlaceholderImage();
        }
    }

    public async Task Load3DModelAsync(string modelPath)
    {
        if (!Enable3DAvatar || string.IsNullOrEmpty(modelPath)) return;

        try
        {
            // Load 3D model implementation would go here
            // For now, create a simple geometric representation
            Create3DPlaceholder();

            if (EnablePortraitAnimations && !_isSimplifiedMode)
            {
                await Animate3DModelLoad();
            }
        }
        catch
        {
            // Fallback to 2D image
            Enable3DAvatar = false;
            _avatar3D.Visibility = Visibility.Collapsed;
        }
    }

    #endregion

    #region Visual Updates

    private void UpdateCharacterDisplay()
    {
        if (CharacterData == null) return;

        // Update character name
        if (_characterName != null)
        {
            _characterName.Text = CharacterData.Name ?? "Unknown Pilot";
        }

        // Update corporation info
        if (_corporationName != null && ShowCorporationInfo)
        {
            _corporationName.Text = CharacterData.CorporationName ?? "Unknown Corporation";
        }

        // Update skill points
        if (_skillPointsText != null && ShowSkillPoints)
        {
            _skillPointsText.Text = FormatSkillPoints(CharacterData.TotalSkillPoints);
        }

        // Update status indicators
        UpdateStatusIndicators();

        // Update portraits
        if (!string.IsNullOrEmpty(CharacterData.PortraitUrl))
        {
            _ = LoadCharacterImageAsync(CharacterData.PortraitUrl);
        }

        if (!string.IsNullOrEmpty(CharacterData.CorporationLogoUrl) && _corporationImage != null)
        {
            _ = LoadCorporationLogo(CharacterData.CorporationLogoUrl);
        }

        // Update 3D model if available
        if (Enable3DAvatar && !string.IsNullOrEmpty(CharacterData.ModelPath))
        {
            _ = Load3DModelAsync(CharacterData.ModelPath);
        }

        _lastUpdate = DateTime.Now;
    }

    private void UpdateStatusIndicators()
    {
        // Clear existing indicators
        if (_statusIndicators != null)
        {
            _portraitContainer.Children.Remove(_statusIndicators);
        }

        if (CharacterData?.StatusIndicators == null || CharacterData.StatusIndicators.Count == 0) return;

        _statusIndicators = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(10)
        };

        foreach (var status in CharacterData.StatusIndicators)
        {
            var indicator = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(GetStatusColor(status)),
                Margin = new Thickness(2),
                ToolTip = GetStatusTooltip(status)
            };

            if (EnablePortraitAnimations && !_isSimplifiedMode)
            {
                AnimateStatusIndicator(indicator);
            }

            _statusIndicators.Children.Add(indicator);
        }

        _portraitContainer.Children.Add(_statusIndicators);
    }

    private async Task LoadCorporationLogo(string logoUrl)
    {
        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(logoUrl);
            bitmap.EndInit();

            _corporationImage.Source = bitmap;
        }
        catch
        {
            // Use default corporation logo
            _corporationImage.Source = CreateDefaultCorpLogo();
        }
    }

    #endregion

    #region Animation Methods

    private async Task AnimatePortraitRefresh()
    {
        if (_isSimplifiedMode) return;

        var scaleTransform = new ScaleTransform(1, 1);
        _portraitContainer.RenderTransform = scaleTransform;
        _portraitContainer.RenderTransformOrigin = new Point(0.5, 0.5);

        var scaleAnimation = new DoubleAnimation
        {
            From = 1,
            To = 1.05,
            Duration = TimeSpan.FromMilliseconds(200),
            AutoReverse = true,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);

        await Task.Delay(400);
    }

    private async Task AnimateImageLoad()
    {
        if (_isSimplifiedMode) return;

        _characterImage.Opacity = 0;

        var fadeAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(500),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        _characterImage.BeginAnimation(OpacityProperty, fadeAnimation);
        await Task.Delay(500);
    }

    private async Task Animate3DModelLoad()
    {
        if (_isSimplifiedMode || _avatar3D == null) return;

        _avatar3D.Opacity = 0;

        var fadeAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(800),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        // Rotate the model during load
        var rotateTransform = new RotateTransform3D();
        var axisRotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0);
        rotateTransform.Rotation = axisRotation;

        var rotateAnimation = new DoubleAnimation
        {
            From = 0,
            To = 360,
            Duration = TimeSpan.FromMilliseconds(800),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        _avatar3D.BeginAnimation(OpacityProperty, fadeAnimation);
        axisRotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, rotateAnimation);

        await Task.Delay(800);
    }

    private void AnimateStatusIndicator(UIElement indicator)
    {
        var scaleTransform = new ScaleTransform(0, 0);
        indicator.RenderTransform = scaleTransform;
        indicator.RenderTransformOrigin = new Point(0.5, 0.5);

        var scaleAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
        };

        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
    }

    private void AnimateHoverEffect(bool isEntering)
    {
        if (_isSimplifiedMode) return;

        var targetScale = isEntering ? 1.05 : 1.0;
        var targetIntensity = isEntering ? 1.3 : 1.0;

        var scaleTransform = _portraitContainer.RenderTransform as ScaleTransform ?? new ScaleTransform();
        _portraitContainer.RenderTransform = scaleTransform;
        _portraitContainer.RenderTransformOrigin = new Point(0.5, 0.5);

        var scaleAnimation = new DoubleAnimation
        {
            To = targetScale,
            Duration = TimeSpan.FromMilliseconds(200),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);

        // Update glow intensity
        if (_portraitFrame?.Effect is DropShadowEffect effect)
        {
            var glowAnimation = new DoubleAnimation
            {
                To = 10 * targetIntensity,
                Duration = TimeSpan.FromMilliseconds(200)
            };
            effect.BeginAnimation(DropShadowEffect.BlurRadiusProperty, glowAnimation);
        }
    }

    #endregion

    #region Particle Effects

    private void SpawnSkillParticles()
    {
        if (_isSimplifiedMode || !EnableParticleEffects) return;
        if (CharacterData?.TotalSkillPoints == null || CharacterData.TotalSkillPoints == 0) return;

        var particleCount = Math.Min(5, (int)(CharacterData.TotalSkillPoints / 1000000)); // 1 particle per million SP
        
        for (int i = 0; i < particleCount; i++)
        {
            var particle = new Ellipse
            {
                Width = _random.Next(2, 4),
                Height = _random.Next(2, 4),
                Fill = new SolidColorBrush(Color.FromArgb(
                    (byte)_random.Next(100, 200),
                    100, 200, 255))
            };

            var startX = _skillsPanel?.ActualWidth ?? Width - 50;
            var startY = _skillsPanel?.ActualHeight ?? Height - 20;

            Canvas.SetLeft(particle, startX + _random.Next(-10, 10));
            Canvas.SetTop(particle, startY + _random.Next(-10, 10));
            _particleCanvas.Children.Add(particle);
            _skillParticles.Add(particle);

            AnimateSkillParticle(particle);
        }

        CleanupSkillParticles();
    }

    private void SpawnBackgroundParticles()
    {
        if (_isSimplifiedMode || !EnableParticleEffects) return;

        if (_random.NextDouble() < 0.1) // 10% chance
        {
            var particle = new Ellipse
            {
                Width = _random.Next(1, 3),
                Height = _random.Next(1, 3),
                Fill = new SolidColorBrush(Color.FromArgb(
                    (byte)_random.Next(50, 100),
                    100, 150, 200))
            };

            var startX = _random.NextDouble() * Width;
            var startY = _random.NextDouble() * Height;

            Canvas.SetLeft(particle, startX);
            Canvas.SetTop(particle, startY);
            _particleCanvas.Children.Add(particle);
            _backgroundParticles.Add(particle);

            AnimateBackgroundParticle(particle);
        }

        CleanupBackgroundParticles();
    }

    private void AnimateSkillParticle(UIElement particle)
    {
        var angle = _random.NextDouble() * Math.PI * 2;
        var distance = _random.Next(15, 30);
        var targetX = Canvas.GetLeft(particle) + Math.Cos(angle) * distance;
        var targetY = Canvas.GetTop(particle) + Math.Sin(angle) * distance;

        var moveXAnimation = new DoubleAnimation
        {
            From = Canvas.GetLeft(particle),
            To = targetX,
            Duration = TimeSpan.FromMilliseconds(_random.Next(800, 1200)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var moveYAnimation = new DoubleAnimation
        {
            From = Canvas.GetTop(particle),
            To = targetY,
            Duration = TimeSpan.FromMilliseconds(_random.Next(800, 1200)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(_random.Next(800, 1200))
        };

        moveXAnimation.Completed += (s, e) =>
        {
            _particleCanvas.Children.Remove(particle);
            _skillParticles.Remove(particle);
        };

        particle.BeginAnimation(Canvas.LeftProperty, moveXAnimation);
        particle.BeginAnimation(Canvas.TopProperty, moveYAnimation);
        particle.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void AnimateBackgroundParticle(UIElement particle)
    {
        var moveAnimation = new DoubleAnimation
        {
            From = Canvas.GetTop(particle),
            To = Canvas.GetTop(particle) - 20,
            Duration = TimeSpan.FromMilliseconds(_random.Next(2000, 3000)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(_random.Next(2000, 3000))
        };

        moveAnimation.Completed += (s, e) =>
        {
            _particleCanvas.Children.Remove(particle);
            _backgroundParticles.Remove(particle);
        };

        particle.BeginAnimation(Canvas.TopProperty, moveAnimation);
        particle.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void CleanupSkillParticles()
    {
        if (_skillParticles.Count > 20)
        {
            var particlesToRemove = _skillParticles.Take(10).ToList();
            foreach (var particle in particlesToRemove)
            {
                _particleCanvas.Children.Remove(particle);
                _skillParticles.Remove(particle);
            }
        }
    }

    private void CleanupBackgroundParticles()
    {
        if (_backgroundParticles.Count > 15)
        {
            var particlesToRemove = _backgroundParticles.Take(8).ToList();
            foreach (var particle in particlesToRemove)
            {
                _particleCanvas.Children.Remove(particle);
                _backgroundParticles.Remove(particle);
            }
        }
    }

    #endregion

    #region Helper Methods

    private (double Width, double Height) GetPortraitDimensions()
    {
        return PortraitSize switch
        {
            PortraitSize.Small => (120, 80),
            PortraitSize.Medium => (200, 120),
            PortraitSize.Large => (280, 160),
            PortraitSize.ExtraLarge => (360, 200),
            _ => (200, 120)
        };
    }

    private (double Width, double Height) GetAvatarDimensions()
    {
        return PortraitSize switch
        {
            PortraitSize.Small => (50, 50),
            PortraitSize.Medium => (80, 80),
            PortraitSize.Large => (100, 100),
            PortraitSize.ExtraLarge => (120, 120),
            _ => (80, 80)
        };
    }

    private string FormatSkillPoints(long skillPoints)
    {
        if (skillPoints >= 1000000)
        {
            return $"{skillPoints / 1000000.0:F1}M SP";
        }
        if (skillPoints >= 1000)
        {
            return $"{skillPoints / 1000.0:F0}K SP";
        }
        return $"{skillPoints} SP";
    }

    private Color GetStatusColor(CharacterStatus status)
    {
        return status switch
        {
            CharacterStatus.Online => Color.FromArgb(255, 100, 255, 100),
            CharacterStatus.Offline => Color.FromArgb(255, 150, 150, 150),
            CharacterStatus.InSpace => Color.FromArgb(255, 100, 200, 255),
            CharacterStatus.Docked => Color.FromArgb(255, 255, 200, 100),
            CharacterStatus.Training => Color.FromArgb(255, 200, 100, 255),
            _ => Color.FromArgb(255, 150, 150, 150)
        };
    }

    private string GetStatusTooltip(CharacterStatus status)
    {
        return status switch
        {
            CharacterStatus.Online => "Character is online",
            CharacterStatus.Offline => "Character is offline",
            CharacterStatus.InSpace => "Character is in space",
            CharacterStatus.Docked => "Character is docked",
            CharacterStatus.Training => "Character is training skills",
            _ => "Unknown status"
        };
    }

    private ImageSource CreatePlaceholderImage()
    {
        // Create a simple placeholder image
        var visual = new Grid
        {
            Width = 128,
            Height = 128,
            Background = new SolidColorBrush(Color.FromArgb(100, 100, 100, 100))
        };

        var text = new TextBlock
        {
            Text = "?",
            FontSize = 48,
            Foreground = new SolidColorBrush(Colors.White),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        visual.Children.Add(text);

        var renderBitmap = new RenderTargetBitmap(128, 128, 96, 96, PixelFormats.Pbgra32);
        renderBitmap.Render(visual);
        return renderBitmap;
    }

    private ImageSource CreateDefaultCorpLogo()
    {
        // Create a simple default corporation logo
        var visual = new Ellipse
        {
            Width = 20,
            Height = 20,
            Fill = new SolidColorBrush(Color.FromArgb(150, 100, 100, 100)),
            Stroke = new SolidColorBrush(Colors.White),
            StrokeThickness = 1
        };

        var renderBitmap = new RenderTargetBitmap(20, 20, 96, 96, PixelFormats.Pbgra32);
        renderBitmap.Render(visual);
        return renderBitmap;
    }

    private void Create3DPlaceholder()
    {
        if (_characterModel == null) return;

        // Create a simple 3D geometric representation
        var geometry = new MeshGeometry3D();
        
        // Simple box geometry
        geometry.Positions.Add(new Point3D(-0.5, -0.5, -0.5));
        geometry.Positions.Add(new Point3D(0.5, -0.5, -0.5));
        geometry.Positions.Add(new Point3D(0.5, 0.5, -0.5));
        geometry.Positions.Add(new Point3D(-0.5, 0.5, -0.5));

        geometry.TriangleIndices.Add(0);
        geometry.TriangleIndices.Add(1);
        geometry.TriangleIndices.Add(2);
        geometry.TriangleIndices.Add(0);
        geometry.TriangleIndices.Add(2);
        geometry.TriangleIndices.Add(3);

        var material = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(200, 100, 200, 255)));
        var model = new GeometryModel3D(geometry, material);
        _characterModel.Content = model;
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateCharacterDisplay();
        if (EnablePortraitAnimations && !_isSimplifiedMode)
        {
            _animationTimer.Start();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        CleanupAllParticles();
    }

    private void OnAnimationTimerTick(object sender, EventArgs e)
    {
        if (EnablePortraitAnimations && !_isSimplifiedMode)
        {
            _animationPhase += 0.1;
            if (_animationPhase > Math.PI * 2)
                _animationPhase = 0;

            UpdateAnimatedElements();

            if (_isHovered)
            {
                SpawnSkillParticles();
            }

            SpawnBackgroundParticles();
        }
    }

    private void UpdateAnimatedElements()
    {
        // Subtle glow animation
        if (_portraitFrame?.Effect is DropShadowEffect effect)
        {
            var glowIntensity = 0.8 + (Math.Sin(_animationPhase * 2) * 0.2);
            effect.Opacity = glowIntensity * HolographicIntensity;
        }

        // Rotate 3D model slightly
        if (_characterModel?.Content is GeometryModel3D model)
        {
            var rotateTransform = new RotateTransform3D();
            var axisRotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), Math.Sin(_animationPhase) * 5);
            rotateTransform.Rotation = axisRotation;
            model.Transform = rotateTransform;
        }
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        _isHovered = true;
        if (EnablePortraitAnimations && !_isSimplifiedMode)
        {
            AnimateHoverEffect(true);
        }
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        _isHovered = false;
        if (EnablePortraitAnimations && !_isSimplifiedMode)
        {
            AnimateHoverEffect(false);
        }
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        PortraitClicked?.Invoke(this, new HoloCharacterEventArgs
        {
            Character = CharacterData,
            Timestamp = DateTime.Now
        });

        if (e.ClickCount == 2)
        {
            CharacterDetailsRequested?.Invoke(this, new HoloCharacterEventArgs
            {
                Character = CharacterData,
                Timestamp = DateTime.Now
            });
        }
    }

    private void OnCorporationClicked(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        CorporationClicked?.Invoke(this, new HoloCharacterEventArgs
        {
            Character = CharacterData,
            Timestamp = DateTime.Now
        });
    }

    private void OnSkillsClicked(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        SkillsClicked?.Invoke(this, new HoloCharacterEventArgs
        {
            Character = CharacterData,
            Timestamp = DateTime.Now
        });
    }

    private void CleanupAllParticles()
    {
        foreach (var particle in _skillParticles.ToList())
        {
            _particleCanvas.Children.Remove(particle);
        }
        _skillParticles.Clear();

        foreach (var particle in _backgroundParticles.ToList())
        {
            _particleCanvas.Children.Remove(particle);
        }
        _backgroundParticles.Clear();
    }

    #endregion

    #region Property Change Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterPortrait portrait)
        {
            portrait.ApplyHolographicIntensity();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterPortrait portrait)
        {
            portrait.ApplyEVEColorScheme();
        }
    }

    private static void OnCharacterDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterPortrait portrait)
        {
            portrait.UpdateCharacterDisplay();
        }
    }

    private static void OnDisplayModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterPortrait portrait)
        {
            portrait.InitializeComponent();
        }
    }

    private static void OnEnable3DAvatarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterPortrait portrait)
        {
            if (portrait._avatar3D != null)
            {
                portrait._avatar3D.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }

    private static void OnEnablePortraitAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterPortrait portrait)
        {
            if ((bool)e.NewValue && !portrait._isSimplifiedMode)
            {
                portrait._animationTimer?.Start();
            }
            else
            {
                portrait._animationTimer?.Stop();
            }
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterPortrait portrait && !(bool)e.NewValue)
        {
            portrait.CleanupAllParticles();
        }
    }

    private static void OnShowSkillPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterPortrait portrait)
        {
            if (portrait._skillsPanel != null)
            {
                portrait._skillsPanel.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }

    private static void OnShowCorporationInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterPortrait portrait)
        {
            if (portrait._corporationName != null)
            {
                portrait._corporationName.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            }
            if (portrait._corporationLogo != null)
            {
                portrait._corporationLogo.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }

    private static void OnPortraitSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterPortrait portrait)
        {
            portrait.InitializeComponent();
        }
    }

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode => _isSimplifiedMode;

    public void EnterSimplifiedMode()
    {
        _isSimplifiedMode = true;
        CleanupAllParticles();
        EnablePortraitAnimations = false;
        EnableParticleEffects = false;
        Enable3DAvatar = false;
        _animationTimer?.Stop();
    }

    public void ExitSimplifiedMode()
    {
        _isSimplifiedMode = false;
        EnablePortraitAnimations = true;
        EnableParticleEffects = true;
        Enable3DAvatar = true;
        _animationTimer?.Start();
    }

    #endregion

    #region Color Scheme Application

    private void ApplyEVEColorScheme()
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        
        if (_portraitFrame != null)
        {
            _portraitFrame.BorderBrush = new SolidColorBrush(Color.FromArgb(100, colors.Primary.R, colors.Primary.G, colors.Primary.B));
            
            if (_portraitFrame.Effect is DropShadowEffect shadow)
            {
                shadow.Color = Color.FromArgb(80, colors.Primary.R, colors.Primary.G, colors.Primary.B);
            }
        }

        if (_characterName?.Effect is DropShadowEffect nameEffect)
        {
            nameEffect.Color = Color.FromArgb(100, colors.Primary.R, colors.Primary.G, colors.Primary.B);
        }
    }

    private void ApplyHolographicIntensity()
    {
        var intensity = HolographicIntensity;
        
        if (_portraitFrame?.Effect is DropShadowEffect shadow)
        {
            shadow.BlurRadius = 10 * intensity;
            shadow.ShadowDepth = 2 * intensity;
        }

        EnablePortraitAnimations = intensity > 0.3;
        EnableParticleEffects = intensity > 0.5;
    }

    #endregion
}

#region Supporting Classes and Enums

public enum PortraitDisplayMode
{
    Full,
    Compact,
    Minimal,
    AvatarOnly
}

public enum PortraitSize
{
    Small,
    Medium,
    Large,
    ExtraLarge
}

public enum CharacterStatus
{
    Unknown,
    Offline,
    Online,
    InSpace,
    Docked,
    Training
}

public class HoloCharacterData
{
    public string Name { get; set; }
    public string CorporationName { get; set; }
    public string AllianceName { get; set; }
    public long TotalSkillPoints { get; set; }
    public string PortraitUrl { get; set; }
    public string CorporationLogoUrl { get; set; }
    public string AllianceLogoUrl { get; set; }
    public string ModelPath { get; set; }
    public List<CharacterStatus> StatusIndicators { get; set; } = new();
    public DateTime LastLogin { get; set; }
    public string Location { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

public class HoloCharacterEventArgs : EventArgs
{
    public HoloCharacterData Character { get; set; }
    public DateTime Timestamp { get; set; }
}

#endregion