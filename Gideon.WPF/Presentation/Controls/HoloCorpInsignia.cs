// ==========================================================================
// HoloCorpInsignia.cs - Holographic Corporation Insignia Animations
// ==========================================================================
// Advanced corporation insignia display featuring holographic animations,
// dynamic overlays, alliance support, and EVE corporation integration.
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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Holographic corporation insignia display with animated effects and alliance integration
/// </summary>
public class HoloCorpInsignia : Control, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloCorpInsignia),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloCorpInsignia),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty CorporationLogoProperty =
        DependencyProperty.Register(nameof(CorporationLogo), typeof(ImageSource), typeof(HoloCorpInsignia),
            new PropertyMetadata(null, OnCorporationLogoChanged));

    public static readonly DependencyProperty AllianceLogoProperty =
        DependencyProperty.Register(nameof(AllianceLogo), typeof(ImageSource), typeof(HoloCorpInsignia),
            new PropertyMetadata(null, OnAllianceLogoChanged));

    public static readonly DependencyProperty CorporationNameProperty =
        DependencyProperty.Register(nameof(CorporationName), typeof(string), typeof(HoloCorpInsignia),
            new PropertyMetadata(null, OnCorporationNameChanged));

    public static readonly DependencyProperty AllianceNameProperty =
        DependencyProperty.Register(nameof(AllianceName), typeof(string), typeof(HoloCorpInsignia),
            new PropertyMetadata(null, OnAllianceNameChanged));

    public static readonly DependencyProperty InsigniaDisplayModeProperty =
        DependencyProperty.Register(nameof(InsigniaDisplayMode), typeof(InsigniaDisplayMode), typeof(HoloCorpInsignia),
            new PropertyMetadata(InsigniaDisplayMode.CorporationOnly, OnInsigniaDisplayModeChanged));

    public static readonly DependencyProperty InsigniaSizeProperty =
        DependencyProperty.Register(nameof(InsigniaSize), typeof(InsigniaSize), typeof(HoloCorpInsignia),
            new PropertyMetadata(InsigniaSize.Medium, OnInsigniaSizeChanged));

    public static readonly DependencyProperty EnableRotationProperty =
        DependencyProperty.Register(nameof(EnableRotation), typeof(bool), typeof(HoloCorpInsignia),
            new PropertyMetadata(true, OnEnableRotationChanged));

    public static readonly DependencyProperty EnablePulseEffectProperty =
        DependencyProperty.Register(nameof(EnablePulseEffect), typeof(bool), typeof(HoloCorpInsignia),
            new PropertyMetadata(true, OnEnablePulseEffectChanged));

    public static readonly DependencyProperty EnableParticleHaloProperty =
        DependencyProperty.Register(nameof(EnableParticleHalo), typeof(bool), typeof(HoloCorpInsignia),
            new PropertyMetadata(true, OnEnableParticleHaloChanged));

    public static readonly DependencyProperty ShowInsigniaTextProperty =
        DependencyProperty.Register(nameof(ShowInsigniaText), typeof(bool), typeof(HoloCorpInsignia),
            new PropertyMetadata(true));

    public static readonly DependencyProperty AnimationSpeedProperty =
        DependencyProperty.Register(nameof(AnimationSpeed), typeof(double), typeof(HoloCorpInsignia),
            new PropertyMetadata(1.0, OnAnimationSpeedChanged));

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

    public ImageSource CorporationLogo
    {
        get => (ImageSource)GetValue(CorporationLogoProperty);
        set => SetValue(CorporationLogoProperty, value);
    }

    public ImageSource AllianceLogo
    {
        get => (ImageSource)GetValue(AllianceLogoProperty);
        set => SetValue(AllianceLogoProperty, value);
    }

    public string CorporationName
    {
        get => (string)GetValue(CorporationNameProperty);
        set => SetValue(CorporationNameProperty, value);
    }

    public string AllianceName
    {
        get => (string)GetValue(AllianceNameProperty);
        set => SetValue(AllianceNameProperty, value);
    }

    public InsigniaDisplayMode InsigniaDisplayMode
    {
        get => (InsigniaDisplayMode)GetValue(InsigniaDisplayModeProperty);
        set => SetValue(InsigniaDisplayModeProperty, value);
    }

    public InsigniaSize InsigniaSize
    {
        get => (InsigniaSize)GetValue(InsigniaSizeProperty);
        set => SetValue(InsigniaSizeProperty, value);
    }

    public bool EnableRotation
    {
        get => (bool)GetValue(EnableRotationProperty);
        set => SetValue(EnableRotationProperty, value);
    }

    public bool EnablePulseEffect
    {
        get => (bool)GetValue(EnablePulseEffectProperty);
        set => SetValue(EnablePulseEffectProperty, value);
    }

    public bool EnableParticleHalo
    {
        get => (bool)GetValue(EnableParticleHaloProperty);
        set => SetValue(EnableParticleHaloProperty, value);
    }

    public bool ShowInsigniaText
    {
        get => (bool)GetValue(ShowInsigniaTextProperty);
        set => SetValue(ShowInsigniaTextProperty, value);
    }

    public double AnimationSpeed
    {
        get => (double)GetValue(AnimationSpeedProperty);
        set => SetValue(AnimationSpeedProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloCorpInsigniaEventArgs> InsigniaClicked;
    public event EventHandler<HoloCorpInsigniaEventArgs> InsigniaHovered;
    public event EventHandler<HoloCorpInsigniaEventArgs> CorporationChanged;
    public event EventHandler<HoloCorpInsigniaEventArgs> AllianceChanged;

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode { get; private set; }

    public void SwitchToFullMode()
    {
        IsInSimplifiedMode = false;
        EnableRotation = true;
        EnablePulseEffect = true;
        EnableParticleHalo = true;
        ShowInsigniaText = true;
        UpdateInsigniaAppearance();
    }

    public void SwitchToSimplifiedMode()
    {
        IsInSimplifiedMode = true;
        EnableRotation = false;
        EnablePulseEffect = false;
        EnableParticleHalo = false;
        ShowInsigniaText = false;
        UpdateInsigniaAppearance();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => !_disposed && IsLoaded;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        EnableRotation = settings.EnabledFeatures.HasFlag(AnimationFeatures.Rotation);
        EnablePulseEffect = settings.EnabledFeatures.HasFlag(AnimationFeatures.BasicGlow);
        EnableParticleHalo = settings.EnabledFeatures.HasFlag(AnimationFeatures.ParticleEffects);
        AnimationSpeed = settings.AnimationSpeed;
        
        UpdateInsigniaAppearance();
    }

    #endregion

    #region Fields

    private Grid _insigniaGrid;
    private Border _corpContainer;
    private Border _allianceContainer;
    private Image _corpLogo;
    private Image _allianceLogo;
    private TextBlock _corpName;
    private TextBlock _allianceName;
    private Canvas _effectCanvas;
    private Ellipse _pulseRing;
    private Path _decorativeBorder;
    
    private readonly List<InsigniaParticle> _particles = new();
    private DispatcherTimer _rotationTimer;
    private DispatcherTimer _pulseTimer;
    private DispatcherTimer _particleTimer;
    private double _rotationAngle = 0;
    private double _pulsePhase = 0;
    private double _particlePhase = 0;
    private readonly Random _random = new();
    private bool _disposed = false;
    private bool _isHovered = false;

    #endregion

    #region Constructor

    public HoloCorpInsignia()
    {
        DefaultStyleKey = typeof(HoloCorpInsignia);
        Width = 80;
        Height = 80;
        InitializeInsignia();
        SetupAnimations();
        
        // Register with animation intensity manager
        AnimationIntensityManager.Instance.RegisterTarget(this);
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        MouseEnter += OnMouseEnter;
        MouseLeave += OnMouseLeave;
        MouseLeftButtonUp += OnMouseLeftButtonUp;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Update corporation information
    /// </summary>
    public void UpdateCorporation(ImageSource logo, string name)
    {
        CorporationLogo = logo;
        CorporationName = name;
        
        CorporationChanged?.Invoke(this, new HoloCorpInsigniaEventArgs 
        { 
            CorporationName = name,
            Timestamp = DateTime.Now
        });
    }

    /// <summary>
    /// Update alliance information
    /// </summary>
    public void UpdateAlliance(ImageSource logo, string name)
    {
        AllianceLogo = logo;
        AllianceName = name;
        
        AllianceChanged?.Invoke(this, new HoloCorpInsigniaEventArgs 
        { 
            AllianceName = name,
            Timestamp = DateTime.Now
        });
    }

    /// <summary>
    /// Trigger insignia flash animation
    /// </summary>
    public void FlashInsignia(Color color, TimeSpan duration)
    {
        if (IsInSimplifiedMode) return;

        var flashAnimation = new ColorAnimation
        {
            To = color,
            Duration = TimeSpan.FromMilliseconds(100),
            AutoReverse = true,
            RepeatBehavior = new RepeatBehavior(duration.TotalMilliseconds / 200)
        };

        if (_corpContainer?.BorderBrush is SolidColorBrush brush)
        {
            brush.BeginAnimation(SolidColorBrush.ColorProperty, flashAnimation);
        }
    }

    /// <summary>
    /// Start insignia celebration animation
    /// </summary>
    public async Task PlayCelebrationAnimationAsync()
    {
        if (IsInSimplifiedMode) return;

        // Trigger extra particle burst
        for (int i = 0; i < 10; i++)
        {
            SpawnCelebrationParticle();
            await Task.Delay(50);
        }
    }

    #endregion

    #region Private Methods

    private void InitializeInsignia()
    {
        Template = CreateInsigniaTemplate();
        UpdateInsigniaAppearance();
    }

    private ControlTemplate CreateInsigniaTemplate()
    {
        var template = new ControlTemplate(typeof(HoloCorpInsignia));

        // Main insignia grid
        var insigniaGrid = new FrameworkElementFactory(typeof(Grid));
        insigniaGrid.Name = "PART_InsigniaGrid";

        // Corporation container
        var corpContainer = new FrameworkElementFactory(typeof(Border));
        corpContainer.Name = "PART_CorpContainer";
        corpContainer.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
        corpContainer.SetValue(Border.BorderThicknessProperty, new Thickness(2));
        corpContainer.SetValue(Border.ClipToBoundsProperty, true);

        // Corporation logo
        var corpLogo = new FrameworkElementFactory(typeof(Image));
        corpLogo.Name = "PART_CorpLogo";
        corpLogo.SetValue(Image.StretchProperty, Stretch.Uniform);
        corpLogo.SetValue(Image.MarginProperty, new Thickness(4));

        corpContainer.AppendChild(corpLogo);

        // Alliance container (overlay)
        var allianceContainer = new FrameworkElementFactory(typeof(Border));
        allianceContainer.Name = "PART_AllianceContainer";
        allianceContainer.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
        allianceContainer.SetValue(Border.BorderThicknessProperty, new Thickness(1));
        allianceContainer.SetValue(Border.WidthProperty, 32.0);
        allianceContainer.SetValue(Border.HeightProperty, 32.0);
        allianceContainer.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Right);
        allianceContainer.SetValue(Border.VerticalAlignmentProperty, VerticalAlignment.Bottom);
        allianceContainer.SetValue(Border.MarginProperty, new Thickness(0, 0, -8, -8));

        // Alliance logo
        var allianceLogo = new FrameworkElementFactory(typeof(Image));
        allianceLogo.Name = "PART_AllianceLogo";
        allianceLogo.SetValue(Image.StretchProperty, Stretch.Uniform);
        allianceLogo.SetValue(Image.MarginProperty, new Thickness(2));

        allianceContainer.AppendChild(allianceLogo);

        // Text stack (below insignia)
        var textStack = new FrameworkElementFactory(typeof(StackPanel));
        textStack.SetValue(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Bottom);
        textStack.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        textStack.SetValue(StackPanel.MarginProperty, new Thickness(0, 4, 0, 0));

        // Corporation name
        var corpName = new FrameworkElementFactory(typeof(TextBlock));
        corpName.Name = "PART_CorpName";
        corpName.SetValue(TextBlock.FontSizeProperty, 10.0);
        corpName.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        corpName.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        corpName.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);

        // Alliance name
        var allianceName = new FrameworkElementFactory(typeof(TextBlock));
        allianceName.Name = "PART_AllianceName";
        allianceName.SetValue(TextBlock.FontSizeProperty, 8.0);
        allianceName.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        allianceName.SetValue(TextBlock.OpacityProperty, 0.8);
        allianceName.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);

        textStack.AppendChild(corpName);
        textStack.AppendChild(allianceName);

        // Pulse ring
        var pulseRing = new FrameworkElementFactory(typeof(Ellipse));
        pulseRing.Name = "PART_PulseRing";
        pulseRing.SetValue(Ellipse.StrokeThicknessProperty, 2.0);
        pulseRing.SetValue(Ellipse.FillProperty, Brushes.Transparent);
        pulseRing.SetValue(Ellipse.IsHitTestVisibleProperty, false);
        pulseRing.SetValue(Ellipse.OpacityProperty, 0.0);

        // Decorative border
        var decorativeBorder = new FrameworkElementFactory(typeof(Path));
        decorativeBorder.Name = "PART_DecorativeBorder";
        decorativeBorder.SetValue(Path.StrokeThicknessProperty, 1.0);
        decorativeBorder.SetValue(Path.FillProperty, Brushes.Transparent);
        decorativeBorder.SetValue(Path.IsHitTestVisibleProperty, false);

        // Effect canvas for particles
        var effectCanvas = new FrameworkElementFactory(typeof(Canvas));
        effectCanvas.Name = "PART_EffectCanvas";
        effectCanvas.SetValue(Canvas.IsHitTestVisibleProperty, false);
        effectCanvas.SetValue(Canvas.ClipToBoundsProperty, false);

        // Assembly
        insigniaGrid.AppendChild(corpContainer);
        insigniaGrid.AppendChild(allianceContainer);
        insigniaGrid.AppendChild(textStack);
        insigniaGrid.AppendChild(pulseRing);
        insigniaGrid.AppendChild(decorativeBorder);
        insigniaGrid.AppendChild(effectCanvas);

        template.VisualTree = insigniaGrid;
        return template;
    }

    private void SetupAnimations()
    {
        // Rotation animation timer
        _rotationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
        };
        _rotationTimer.Tick += OnRotationTick;

        // Pulse animation timer
        _pulseTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50) // 20 FPS
        };
        _pulseTimer.Tick += OnPulseTick;

        // Particle animation timer
        _particleTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(66) // 15 FPS
        };
        _particleTimer.Tick += OnParticleTick;
    }

    private void OnRotationTick(object sender, EventArgs e)
    {
        if (!EnableRotation || IsInSimplifiedMode) return;

        _rotationAngle += 0.5 * AnimationSpeed;
        if (_rotationAngle >= 360)
            _rotationAngle -= 360;

        UpdateRotationEffects();
    }

    private void OnPulseTick(object sender, EventArgs e)
    {
        if (!EnablePulseEffect || IsInSimplifiedMode) return;

        _pulsePhase += 0.1 * AnimationSpeed;
        if (_pulsePhase > Math.PI * 2)
            _pulsePhase = 0;

        UpdatePulseEffects();
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        if (!EnableParticleHalo || IsInSimplifiedMode || _effectCanvas == null) return;

        _particlePhase += 0.1 * AnimationSpeed;
        if (_particlePhase > Math.PI * 2)
            _particlePhase = 0;

        UpdateParticleHalo();
        
        if (_isHovered && _random.NextDouble() < 0.3)
        {
            SpawnHaloParticle();
        }
    }

    private void UpdateRotationEffects()
    {
        if (_decorativeBorder != null)
        {
            var rotateTransform = new RotateTransform(_rotationAngle);
            rotateTransform.CenterX = ActualWidth / 2;
            rotateTransform.CenterY = ActualHeight / 2;
            _decorativeBorder.RenderTransform = rotateTransform;
        }
    }

    private void UpdatePulseEffects()
    {
        if (_pulseRing != null)
        {
            var pulseIntensity = 0.3 + (Math.Sin(_pulsePhase) * 0.4);
            var pulseScale = 1.0 + (Math.Sin(_pulsePhase * 0.7) * 0.1);
            
            _pulseRing.Opacity = HolographicIntensity * pulseIntensity;
            
            var scaleTransform = new ScaleTransform(pulseScale, pulseScale);
            scaleTransform.CenterX = ActualWidth / 2;
            scaleTransform.CenterY = ActualHeight / 2;
            _pulseRing.RenderTransform = scaleTransform;
        }

        // Pulse main containers
        if (_corpContainer?.Effect is DropShadowEffect corpEffect)
        {
            var glowIntensity = 0.6 + (Math.Sin(_pulsePhase * 1.5) * 0.3);
            corpEffect.Opacity = HolographicIntensity * glowIntensity;
        }
    }

    private void UpdateParticleHalo()
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var particle = _particles[i];
            
            // Update particle position in orbital motion
            particle.Angle += particle.Speed * AnimationSpeed;
            particle.Life -= 0.01;
            
            var radius = particle.Distance + (Math.Sin(particle.Angle * 2) * 5);
            particle.X = (ActualWidth / 2) + (Math.Cos(particle.Angle) * radius);
            particle.Y = (ActualHeight / 2) + (Math.Sin(particle.Angle) * radius);
            
            Canvas.SetLeft(particle.Visual, particle.X);
            Canvas.SetTop(particle.Visual, particle.Y);
            
            particle.Visual.Opacity = Math.Max(0, particle.Life) * HolographicIntensity;

            // Remove dead particles
            if (particle.Life <= 0)
            {
                _effectCanvas.Children.Remove(particle.Visual);
                _particles.RemoveAt(i);
            }
        }
    }

    private void SpawnHaloParticle()
    {
        if (_particles.Count >= 12) return; // Limit particle count

        var color = GetEVEColor(EVEColorScheme);
        var size = 1 + _random.NextDouble() * 1.5;
        
        var ellipse = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromArgb(150, color.R, color.G, color.B))
        };

        var particle = new InsigniaParticle
        {
            Visual = ellipse,
            Angle = _random.NextDouble() * Math.PI * 2,
            Distance = 30 + _random.NextDouble() * 20,
            Speed = 0.02 + _random.NextDouble() * 0.03,
            Life = 1.0,
            X = 0,
            Y = 0
        };

        _particles.Add(particle);
        _effectCanvas.Children.Add(ellipse);
    }

    private void SpawnCelebrationParticle()
    {
        if (_effectCanvas == null) return;

        var color = GetEVEColor(EVEColorScheme);
        var size = 2 + _random.NextDouble() * 3;
        
        var ellipse = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromArgb(200, color.R, color.G, color.B))
        };

        var startX = ActualWidth / 2;
        var startY = ActualHeight / 2;
        var endX = startX + (_random.NextDouble() - 0.5) * 100;
        var endY = startY + (_random.NextDouble() - 0.5) * 100;

        Canvas.SetLeft(ellipse, startX);
        Canvas.SetTop(ellipse, startY);
        
        _effectCanvas.Children.Add(ellipse);

        // Animate particle burst
        var moveAnimation = new DoubleAnimationUsingKeyFrames();
        moveAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(startX, TimeSpan.Zero));
        moveAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(endX, TimeSpan.FromMilliseconds(1000)));
        
        var moveYAnimation = new DoubleAnimationUsingKeyFrames();
        moveYAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(startY, TimeSpan.Zero));
        moveYAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(endY, TimeSpan.FromMilliseconds(1000)));

        var fadeAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.0,
            Duration = TimeSpan.FromMilliseconds(1000)
        };

        fadeAnimation.Completed += (s, e) => _effectCanvas.Children.Remove(ellipse);

        ellipse.BeginAnimation(Canvas.LeftProperty, moveAnimation);
        ellipse.BeginAnimation(Canvas.TopProperty, moveYAnimation);
        ellipse.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void UpdateInsigniaAppearance()
    {
        if (Template == null) return;

        Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            ApplyTemplate();
            
            _insigniaGrid = GetTemplateChild("PART_InsigniaGrid") as Grid;
            _corpContainer = GetTemplateChild("PART_CorpContainer") as Border;
            _allianceContainer = GetTemplateChild("PART_AllianceContainer") as Border;
            _corpLogo = GetTemplateChild("PART_CorpLogo") as Image;
            _allianceLogo = GetTemplateChild("PART_AllianceLogo") as Image;
            _corpName = GetTemplateChild("PART_CorpName") as TextBlock;
            _allianceName = GetTemplateChild("PART_AllianceName") as TextBlock;
            _effectCanvas = GetTemplateChild("PART_EffectCanvas") as Canvas;
            _pulseRing = GetTemplateChild("PART_PulseRing") as Ellipse;
            _decorativeBorder = GetTemplateChild("PART_DecorativeBorder") as Path;

            UpdateInsigniaSize();
            UpdateColors();
            UpdateEffects();
            UpdateDisplayMode();
            UpdateImages();
            UpdateText();
        }), DispatcherPriority.Render);
    }

    private void UpdateInsigniaSize()
    {
        var size = GetInsigniaPixelSize(InsigniaSize);
        Width = size;
        Height = size;

        if (_corpContainer != null)
        {
            _corpContainer.Width = size * 0.8;
            _corpContainer.Height = size * 0.8;
        }

        if (_pulseRing != null)
        {
            _pulseRing.Width = size;
            _pulseRing.Height = size;
        }
    }

    private void UpdateColors()
    {
        var color = GetEVEColor(EVEColorScheme);

        // Corporation container
        if (_corpContainer != null)
        {
            _corpContainer.BorderBrush = new SolidColorBrush(Color.FromArgb(180, color.R, color.G, color.B));
            
            var backgroundBrush = new RadialGradientBrush();
            backgroundBrush.GradientStops.Add(new GradientStop(Color.FromArgb(60, color.R, color.G, color.B), 0.0));
            backgroundBrush.GradientStops.Add(new GradientStop(Color.FromArgb(30, color.R, color.G, color.B), 1.0));
            _corpContainer.Background = backgroundBrush;
        }

        // Alliance container
        if (_allianceContainer != null)
        {
            _allianceContainer.BorderBrush = new SolidColorBrush(Color.FromArgb(120, color.R, color.G, color.B));
            _allianceContainer.Background = new SolidColorBrush(Color.FromArgb(80, 0, 20, 40));
        }

        // Pulse ring
        if (_pulseRing != null)
        {
            _pulseRing.Stroke = new SolidColorBrush(Color.FromArgb(150, color.R, color.G, color.B));
        }

        // Decorative border
        if (_decorativeBorder != null)
        {
            _decorativeBorder.Stroke = new SolidColorBrush(Color.FromArgb(100, color.R, color.G, color.B));
            _decorativeBorder.Data = CreateDecorativeGeometry();
        }

        // Text colors
        if (_corpName != null)
        {
            _corpName.Foreground = new SolidColorBrush(Colors.White);
        }

        if (_allianceName != null)
        {
            _allianceName.Foreground = new SolidColorBrush(Color.FromArgb(200, 180, 180, 180));
        }
    }

    private void UpdateEffects()
    {
        var color = GetEVEColor(EVEColorScheme);

        if (!IsInSimplifiedMode)
        {
            // Corporation container glow
            if (_corpContainer != null)
            {
                _corpContainer.Effect = new DropShadowEffect
                {
                    Color = color,
                    BlurRadius = 8 * HolographicIntensity,
                    ShadowDepth = 0,
                    Opacity = 0.6 * HolographicIntensity
                };
            }

            // Alliance container glow
            if (_allianceContainer != null && AllianceLogo != null)
            {
                _allianceContainer.Effect = new DropShadowEffect
                {
                    Color = color,
                    BlurRadius = 4 * HolographicIntensity,
                    ShadowDepth = 0,
                    Opacity = 0.4 * HolographicIntensity
                };
            }
        }
    }

    private void UpdateDisplayMode()
    {
        if (_allianceContainer != null)
        {
            _allianceContainer.Visibility = (InsigniaDisplayMode == InsigniaDisplayMode.CorporationAndAlliance && AllianceLogo != null) ? 
                Visibility.Visible : Visibility.Collapsed;
        }

        if (_allianceName != null)
        {
            _allianceName.Visibility = (InsigniaDisplayMode == InsigniaDisplayMode.CorporationAndAlliance && !string.IsNullOrEmpty(AllianceName) && ShowInsigniaText) ? 
                Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void UpdateImages()
    {
        if (_corpLogo != null)
        {
            _corpLogo.Source = CorporationLogo;
        }

        if (_allianceLogo != null)
        {
            _allianceLogo.Source = AllianceLogo;
        }
    }

    private void UpdateText()
    {
        if (_corpName != null)
        {
            _corpName.Text = CorporationName;
            _corpName.Visibility = ShowInsigniaText && !string.IsNullOrEmpty(CorporationName) ? 
                Visibility.Visible : Visibility.Collapsed;
        }

        if (_allianceName != null)
        {
            _allianceName.Text = AllianceName;
            _allianceName.Visibility = ShowInsigniaText && !string.IsNullOrEmpty(AllianceName) && InsigniaDisplayMode == InsigniaDisplayMode.CorporationAndAlliance ? 
                Visibility.Visible : Visibility.Collapsed;
        }
    }

    private Geometry CreateDecorativeGeometry()
    {
        var geometry = new GeometryGroup();
        var centerX = ActualWidth / 2;
        var centerY = ActualHeight / 2;
        var radius = Math.Min(ActualWidth, ActualHeight) / 2 - 5;

        // Create decorative corner marks
        for (int i = 0; i < 4; i++)
        {
            var angle = i * Math.PI / 2;
            var x1 = centerX + Math.Cos(angle) * (radius - 8);
            var y1 = centerY + Math.Sin(angle) * (radius - 8);
            var x2 = centerX + Math.Cos(angle) * radius;
            var y2 = centerY + Math.Sin(angle) * radius;

            var line = new LineGeometry(new Point(x1, y1), new Point(x2, y2));
            geometry.Children.Add(line);
        }

        return geometry;
    }

    private double GetInsigniaPixelSize(InsigniaSize size)
    {
        return size switch
        {
            InsigniaSize.Small => 48,
            InsigniaSize.Medium => 80,
            InsigniaSize.Large => 120,
            InsigniaSize.XLarge => 160,
            _ => 80
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

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        _isHovered = true;
        InsigniaHovered?.Invoke(this, new HoloCorpInsigniaEventArgs 
        { 
            CorporationName = CorporationName,
            AllianceName = AllianceName,
            IsHovered = true,
            Timestamp = DateTime.Now
        });
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        _isHovered = false;
        InsigniaHovered?.Invoke(this, new HoloCorpInsigniaEventArgs 
        { 
            CorporationName = CorporationName,
            AllianceName = AllianceName,
            IsHovered = false,
            Timestamp = DateTime.Now
        });
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        InsigniaClicked?.Invoke(this, new HoloCorpInsigniaEventArgs 
        { 
            CorporationName = CorporationName,
            AllianceName = AllianceName,
            Timestamp = DateTime.Now
        });
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableRotation && !IsInSimplifiedMode)
            _rotationTimer.Start();
            
        if (EnablePulseEffect && !IsInSimplifiedMode)
            _pulseTimer.Start();
            
        if (EnableParticleHalo && !IsInSimplifiedMode)
            _particleTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _rotationTimer?.Stop();
        _pulseTimer?.Stop();
        _particleTimer?.Stop();
        
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
        if (d is HoloCorpInsignia insignia)
            insignia.UpdateInsigniaAppearance();
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCorpInsignia insignia)
            insignia.UpdateInsigniaAppearance();
    }

    private static void OnCorporationLogoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCorpInsignia insignia)
            insignia.UpdateImages();
    }

    private static void OnAllianceLogoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCorpInsignia insignia)
        {
            insignia.UpdateImages();
            insignia.UpdateDisplayMode();
        }
    }

    private static void OnCorporationNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCorpInsignia insignia)
            insignia.UpdateText();
    }

    private static void OnAllianceNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCorpInsignia insignia)
            insignia.UpdateText();
    }

    private static void OnInsigniaDisplayModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCorpInsignia insignia)
            insignia.UpdateDisplayMode();
    }

    private static void OnInsigniaSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCorpInsignia insignia)
            insignia.UpdateInsigniaSize();
    }

    private static void OnEnableRotationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCorpInsignia insignia)
        {
            if ((bool)e.NewValue && !insignia.IsInSimplifiedMode)
                insignia._rotationTimer.Start();
            else
                insignia._rotationTimer.Stop();
        }
    }

    private static void OnEnablePulseEffectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCorpInsignia insignia)
        {
            if ((bool)e.NewValue && !insignia.IsInSimplifiedMode)
                insignia._pulseTimer.Start();
            else
                insignia._pulseTimer.Stop();
        }
    }

    private static void OnEnableParticleHaloChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCorpInsignia insignia)
        {
            if ((bool)e.NewValue && !insignia.IsInSimplifiedMode)
                insignia._particleTimer.Start();
            else
                insignia._particleTimer.Stop();
        }
    }

    private static void OnAnimationSpeedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Animation speed affects timers dynamically, no restart needed
    }

    #endregion
}

/// <summary>
/// Insignia particle for halo effects
/// </summary>
internal class InsigniaParticle
{
    public FrameworkElement Visual { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Angle { get; set; }
    public double Distance { get; set; }
    public double Speed { get; set; }
    public double Life { get; set; }
}

/// <summary>
/// Insignia display modes
/// </summary>
public enum InsigniaDisplayMode
{
    CorporationOnly,
    CorporationAndAlliance
}

/// <summary>
/// Insignia sizes
/// </summary>
public enum InsigniaSize
{
    Small,
    Medium,
    Large,
    XLarge
}

/// <summary>
/// Event args for corporation insignia events
/// </summary>
public class HoloCorpInsigniaEventArgs : EventArgs
{
    public string CorporationName { get; set; }
    public string AllianceName { get; set; }
    public bool IsHovered { get; set; }
    public DateTime Timestamp { get; set; }
}