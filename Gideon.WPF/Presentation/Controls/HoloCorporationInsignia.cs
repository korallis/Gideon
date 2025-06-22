// ==========================================================================
// HoloCorporationInsignia.cs - Holographic Corporation Insignia Animations
// ==========================================================================
// Advanced corporation insignia system featuring holographic corporation logos,
// animated faction emblems, alliance banners, and EVE-style visual hierarchy.
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Holographic corporation insignia system with animated logos and faction emblems
/// </summary>
public class HoloCorporationInsignia : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloCorporationInsignia),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloCorporationInsignia),
            new PropertyMetadata(EVEColorScheme.GoldAccent, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty CorporationDataProperty =
        DependencyProperty.Register(nameof(CorporationData), typeof(HoloCorporationData), typeof(HoloCorporationInsignia),
            new PropertyMetadata(null, OnCorporationDataChanged));

    public static readonly DependencyProperty InsigniaStyleProperty =
        DependencyProperty.Register(nameof(InsigniaStyle), typeof(InsigniaStyle), typeof(HoloCorporationInsignia),
            new PropertyMetadata(InsigniaStyle.Standard, OnInsigniaStyleChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloCorporationInsignia),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloCorporationInsignia),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowFactionEmblemProperty =
        DependencyProperty.Register(nameof(ShowFactionEmblem), typeof(bool), typeof(HoloCorporationInsignia),
            new PropertyMetadata(true, OnShowFactionEmblemChanged));

    public static readonly DependencyProperty ShowAllianceBannerProperty =
        DependencyProperty.Register(nameof(ShowAllianceBanner), typeof(bool), typeof(HoloCorporationInsignia),
            new PropertyMetadata(true, OnShowAllianceBannerChanged));

    public static readonly DependencyProperty InsigniaSizeProperty =
        DependencyProperty.Register(nameof(InsigniaSize), typeof(InsigniaSize), typeof(HoloCorporationInsignia),
            new PropertyMetadata(InsigniaSize.Medium, OnInsigniaSizeChanged));

    public static readonly DependencyProperty EnableHologramEffectProperty =
        DependencyProperty.Register(nameof(EnableHologramEffect), typeof(bool), typeof(HoloCorporationInsignia),
            new PropertyMetadata(true, OnEnableHologramEffectChanged));

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

    public HoloCorporationData CorporationData
    {
        get => (HoloCorporationData)GetValue(CorporationDataProperty);
        set => SetValue(CorporationDataProperty, value);
    }

    public InsigniaStyle InsigniaStyle
    {
        get => (InsigniaStyle)GetValue(InsigniaStyleProperty);
        set => SetValue(InsigniaStyleProperty, value);
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

    public bool ShowFactionEmblem
    {
        get => (bool)GetValue(ShowFactionEmblemProperty);
        set => SetValue(ShowFactionEmblemProperty, value);
    }

    public bool ShowAllianceBanner
    {
        get => (bool)GetValue(ShowAllianceBannerProperty);
        set => SetValue(ShowAllianceBannerProperty, value);
    }

    public InsigniaSize InsigniaSize
    {
        get => (InsigniaSize)GetValue(InsigniaSizeProperty);
        set => SetValue(InsigniaSizeProperty, value);
    }

    public bool EnableHologramEffect
    {
        get => (bool)GetValue(EnableHologramEffectProperty);
        set => SetValue(EnableHologramEffectProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloCorporationEventArgs> CorporationClicked;
    public event EventHandler<HoloCorporationEventArgs> AllianceClicked;
    public event EventHandler<HoloCorporationEventArgs> FactionClicked;
    public event EventHandler<HoloCorporationEventArgs> InsigniaHovered;
    public event EventHandler<HoloCorporationEventArgs> InsigniaDetailsRequested;

    #endregion

    #region Private Fields

    private readonly DispatcherTimer _animationTimer;
    private readonly DispatcherTimer _hologramTimer;
    private readonly Random _random = new();
    private Canvas _mainCanvas;
    private Grid _insigniaContainer;
    private Border _corporationFrame;
    private Image _corporationLogo;
    private Border _allianceFrame;
    private Image _allianceLogo;
    private Border _factionFrame;
    private Image _factionEmblem;
    private Grid _textOverlay;
    private TextBlock _corporationName;
    private TextBlock _allianceName;
    private Canvas _particleCanvas;
    private Canvas _hologramCanvas;
    private readonly List<UIElement> _insigniaParticles = new();
    private readonly List<UIElement> _hologramLines = new();
    private bool _isSimplifiedMode;
    private double _animationPhase;
    private double _hologramPhase;
    private bool _isHovered;
    private DateTime _lastUpdate;

    #endregion

    #region Constructor

    public HoloCorporationInsignia()
    {
        InitializeComponent();
        InitializeTimers();
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
        var dimensions = GetInsigniaDimensions();
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

        // Main insignia container
        _insigniaContainer = new Grid
        {
            Width = Width,
            Height = Height
        };
        _mainCanvas.Children.Add(_insigniaContainer);

        CreateInsigniaElements();
        CreateTextOverlay();
        CreateParticleCanvas();
        CreateHologramCanvas();
    }

    private void CreateInsigniaElements()
    {
        switch (InsigniaStyle)
        {
            case InsigniaStyle.Minimal:
                CreateMinimalLayout();
                break;
            case InsigniaStyle.Compact:
                CreateCompactLayout();
                break;
            case InsigniaStyle.Standard:
                CreateStandardLayout();
                break;
            case InsigniaStyle.Detailed:
                CreateDetailedLayout();
                break;
            case InsigniaStyle.Banner:
                CreateBannerLayout();
                break;
        }
    }

    private void CreateStandardLayout()
    {
        // Corporation logo (primary)
        _corporationFrame = new Border
        {
            Width = GetLogoSize(),
            Height = GetLogoSize(),
            CornerRadius = new CornerRadius(4),
            BorderThickness = new Thickness(2),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 100, 200, 255)),
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(100, 100, 200, 255),
                BlurRadius = 8,
                ShadowDepth = 2,
                Direction = 315
            },
            Cursor = Cursors.Hand
        };

        _corporationLogo = new Image
        {
            Stretch = Stretch.UniformToFill,
            Margin = new Thickness(4)
        };
        _corporationFrame.Child = _corporationLogo;
        _corporationFrame.MouseLeftButtonDown += OnCorporationClicked;
        _insigniaContainer.Children.Add(_corporationFrame);

        // Alliance logo (secondary, if enabled)
        if (ShowAllianceBanner)
        {
            var allianceSize = GetLogoSize() * 0.6;
            _allianceFrame = new Border
            {
                Width = allianceSize,
                Height = allianceSize,
                CornerRadius = new CornerRadius(3),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromArgb(120, 255, 215, 0)),
                Background = new SolidColorBrush(Color.FromArgb(30, 0, 0, 0)),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, -8, -8),
                Effect = new DropShadowEffect
                {
                    Color = Color.FromArgb(80, 255, 215, 0),
                    BlurRadius = 6,
                    ShadowDepth = 1
                },
                Cursor = Cursors.Hand
            };

            _allianceLogo = new Image
            {
                Stretch = Stretch.UniformToFill,
                Margin = new Thickness(2)
            };
            _allianceFrame.Child = _allianceLogo;
            _allianceFrame.MouseLeftButtonDown += OnAllianceClicked;
            _insigniaContainer.Children.Add(_allianceFrame);
        }

        // Faction emblem (tertiary, if enabled)
        if (ShowFactionEmblem)
        {
            var factionSize = GetLogoSize() * 0.4;
            _factionFrame = new Border
            {
                Width = factionSize,
                Height = factionSize,
                CornerRadius = new CornerRadius(2),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromArgb(100, 220, 20, 60)),
                Background = new SolidColorBrush(Color.FromArgb(20, 0, 0, 0)),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(-6, -6, 0, 0),
                Effect = new DropShadowEffect
                {
                    Color = Color.FromArgb(60, 220, 20, 60),
                    BlurRadius = 4,
                    ShadowDepth = 1
                },
                Cursor = Cursors.Hand
            };

            _factionEmblem = new Image
            {
                Stretch = Stretch.UniformToFill,
                Margin = new Thickness(1)
            };
            _factionFrame.Child = _factionEmblem;
            _factionFrame.MouseLeftButtonDown += OnFactionClicked;
            _insigniaContainer.Children.Add(_factionFrame);
        }
    }

    private void CreateMinimalLayout()
    {
        var logoSize = GetLogoSize() * 0.8;
        
        _corporationFrame = new Border
        {
            Width = logoSize,
            Height = logoSize,
            CornerRadius = new CornerRadius(logoSize / 2),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 100, 200, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Cursor = Cursors.Hand
        };

        _corporationLogo = new Image
        {
            Stretch = Stretch.UniformToFill
        };
        _corporationFrame.Child = _corporationLogo;
        _corporationFrame.MouseLeftButtonDown += OnCorporationClicked;
        _insigniaContainer.Children.Add(_corporationFrame);
    }

    private void CreateCompactLayout()
    {
        _insigniaContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        _insigniaContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var logoSize = GetLogoSize() * 0.7;

        // Corporation logo
        _corporationFrame = new Border
        {
            Width = logoSize,
            Height = logoSize,
            CornerRadius = new CornerRadius(3),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromArgb(120, 100, 200, 255)),
            Margin = new Thickness(2),
            Cursor = Cursors.Hand
        };

        _corporationLogo = new Image
        {
            Stretch = Stretch.UniformToFill
        };
        _corporationFrame.Child = _corporationLogo;
        _corporationFrame.MouseLeftButtonDown += OnCorporationClicked;
        Grid.SetColumn(_corporationFrame, 0);
        _insigniaContainer.Children.Add(_corporationFrame);

        // Alliance logo
        if (ShowAllianceBanner)
        {
            _allianceFrame = new Border
            {
                Width = logoSize,
                Height = logoSize,
                CornerRadius = new CornerRadius(3),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromArgb(120, 255, 215, 0)),
                Margin = new Thickness(2),
                Cursor = Cursors.Hand
            };

            _allianceLogo = new Image
            {
                Stretch = Stretch.UniformToFill
            };
            _allianceFrame.Child = _allianceLogo;
            _allianceFrame.MouseLeftButtonDown += OnAllianceClicked;
            Grid.SetColumn(_allianceFrame, 1);
            _insigniaContainer.Children.Add(_allianceFrame);
        }
    }

    private void CreateDetailedLayout()
    {
        CreateStandardLayout();
        
        // Add decorative elements
        CreateInsigniaDecorations();
    }

    private void CreateBannerLayout()
    {
        _insigniaContainer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _insigniaContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var logoSize = GetLogoSize();

        // Corporation logo at top
        _corporationFrame = new Border
        {
            Width = logoSize,
            Height = logoSize,
            CornerRadius = new CornerRadius(4),
            BorderThickness = new Thickness(2),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 100, 200, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5),
            Cursor = Cursors.Hand
        };

        _corporationLogo = new Image
        {
            Stretch = Stretch.UniformToFill,
            Margin = new Thickness(2)
        };
        _corporationFrame.Child = _corporationLogo;
        _corporationFrame.MouseLeftButtonDown += OnCorporationClicked;
        Grid.SetRow(_corporationFrame, 0);
        _insigniaContainer.Children.Add(_corporationFrame);

        // Banner area
        var bannerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5)
        };
        Grid.SetRow(bannerPanel, 1);
        _insigniaContainer.Children.Add(bannerPanel);

        // Alliance and faction in banner
        if (ShowAllianceBanner)
        {
            var allianceSize = logoSize * 0.5;
            _allianceFrame = new Border
            {
                Width = allianceSize,
                Height = allianceSize,
                CornerRadius = new CornerRadius(2),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromArgb(120, 255, 215, 0)),
                Margin = new Thickness(2),
                Cursor = Cursors.Hand
            };

            _allianceLogo = new Image
            {
                Stretch = Stretch.UniformToFill
            };
            _allianceFrame.Child = _allianceLogo;
            _allianceFrame.MouseLeftButtonDown += OnAllianceClicked;
            bannerPanel.Children.Add(_allianceFrame);
        }

        if (ShowFactionEmblem)
        {
            var factionSize = logoSize * 0.5;
            _factionFrame = new Border
            {
                Width = factionSize,
                Height = factionSize,
                CornerRadius = new CornerRadius(2),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromArgb(100, 220, 20, 60)),
                Margin = new Thickness(2),
                Cursor = Cursors.Hand
            };

            _factionEmblem = new Image
            {
                Stretch = Stretch.UniformToFill
            };
            _factionFrame.Child = _factionEmblem;
            _factionFrame.MouseLeftButtonDown += OnFactionClicked;
            bannerPanel.Children.Add(_factionFrame);
        }
    }

    private void CreateInsigniaDecorations()
    {
        // Add decorative corner elements
        var decorations = new List<Rectangle>();

        for (int i = 0; i < 4; i++)
        {
            var decoration = new Rectangle
            {
                Width = 8,
                Height = 2,
                Fill = new SolidColorBrush(Color.FromArgb(100, 100, 200, 255)),
                HorizontalAlignment = i < 2 ? HorizontalAlignment.Left : HorizontalAlignment.Right,
                VerticalAlignment = i % 2 == 0 ? VerticalAlignment.Top : VerticalAlignment.Bottom,
                Margin = new Thickness(i < 2 ? 5 : -5, i % 2 == 0 ? 5 : -5, i >= 2 ? 5 : -5, i % 2 == 1 ? 5 : -5)
            };

            if (EnableHologramEffect)
            {
                decoration.Effect = new DropShadowEffect
                {
                    Color = Color.FromArgb(80, 100, 200, 255),
                    BlurRadius = 4,
                    ShadowDepth = 0
                };
            }

            decorations.Add(decoration);
            _insigniaContainer.Children.Add(decoration);
        }
    }

    private void CreateTextOverlay()
    {
        if (InsigniaStyle == InsigniaStyle.Minimal) return;

        _textOverlay = new Grid
        {
            IsHitTestVisible = false,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(5)
        };

        var backgroundPanel = new Border
        {
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(0, 0, 0, 0), 0),
                    new GradientStop(Color.FromArgb(120, 0, 0, 0), 1)
                }
            },
            CornerRadius = new CornerRadius(0, 0, 4, 4),
            Padding = new Thickness(5, 8, 5, 5)
        };

        var textPanel = new StackPanel();

        _corporationName = new TextBlock
        {
            FontSize = GetTextSize(),
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
            Text = "Unknown Corporation",
            HorizontalAlignment = HorizontalAlignment.Center,
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(100, 0, 0, 0),
                BlurRadius = 2,
                ShadowDepth = 1
            }
        };
        textPanel.Children.Add(_corporationName);

        if (ShowAllianceBanner)
        {
            _allianceName = new TextBlock
            {
                FontSize = GetTextSize() * 0.8,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 215, 0)),
                Text = "Unknown Alliance",
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 1, 0, 0)
            };
            textPanel.Children.Add(_allianceName);
        }

        backgroundPanel.Child = textPanel;
        _textOverlay.Children.Add(backgroundPanel);
        _insigniaContainer.Children.Add(_textOverlay);
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

    private void CreateHologramCanvas()
    {
        _hologramCanvas = new Canvas
        {
            Width = Width,
            Height = Height,
            IsHitTestVisible = false
        };
        _mainCanvas.Children.Add(_hologramCanvas);
    }

    private void InitializeTimers()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };
        _animationTimer.Tick += OnAnimationTimerTick;

        _hologramTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _hologramTimer.Tick += OnHologramTimerTick;
    }

    #endregion

    #region Corporation Management

    public async Task LoadCorporationDataAsync(HoloCorporationData data)
    {
        CorporationData = data;
        await UpdateInsigniaDisplayAsync();
    }

    public async Task UpdateInsigniaDisplayAsync()
    {
        if (CorporationData == null) return;

        // Update text
        if (_corporationName != null)
        {
            _corporationName.Text = CorporationData.CorporationName ?? "Unknown Corporation";
        }

        if (_allianceName != null)
        {
            _allianceName.Text = CorporationData.AllianceName ?? "Unknown Alliance";
        }

        // Load images
        if (!string.IsNullOrEmpty(CorporationData.CorporationLogoUrl))
        {
            await LoadImageAsync(_corporationLogo, CorporationData.CorporationLogoUrl);
        }

        if (!string.IsNullOrEmpty(CorporationData.AllianceLogoUrl) && _allianceLogo != null)
        {
            await LoadImageAsync(_allianceLogo, CorporationData.AllianceLogoUrl);
        }

        if (!string.IsNullOrEmpty(CorporationData.FactionEmblemUrl) && _factionEmblem != null)
        {
            await LoadImageAsync(_factionEmblem, CorporationData.FactionEmblemUrl);
        }

        // Apply faction colors
        ApplyFactionStyling();

        _lastUpdate = DateTime.Now;
    }

    public void RefreshInsignia()
    {
        if (EnableAnimations && !_isSimplifiedMode)
        {
            AnimateInsigniaRefresh();
        }
        _ = UpdateInsigniaDisplayAsync();
    }

    #endregion

    #region Visual Updates

    private async Task LoadImageAsync(Image imageControl, string imageUrl)
    {
        if (imageControl == null || string.IsNullOrEmpty(imageUrl)) return;

        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(imageUrl);
            bitmap.EndInit();

            imageControl.Source = bitmap;

            if (EnableAnimations && !_isSimplifiedMode)
            {
                await AnimateImageLoad(imageControl);
            }
        }
        catch
        {
            // Use placeholder
            imageControl.Source = CreatePlaceholderLogo();
        }
    }

    private void ApplyFactionStyling()
    {
        if (CorporationData?.FactionId == null) return;

        var factionColors = GetFactionColors(CorporationData.FactionId.Value);
        
        // Apply faction colors to appropriate elements
        if (_factionFrame != null)
        {
            _factionFrame.BorderBrush = new SolidColorBrush(factionColors.Primary);
            if (_factionFrame.Effect is DropShadowEffect factionEffect)
            {
                factionEffect.Color = factionColors.Primary;
            }
        }

        // Update corporation frame with faction influence
        if (_corporationFrame != null && CorporationData.FactionStanding > 0)
        {
            var influenceOpacity = (byte)(50 + (CorporationData.FactionStanding * 100));
            var influenceBrush = new SolidColorBrush(Color.FromArgb(influenceOpacity, 
                factionColors.Primary.R, factionColors.Primary.G, factionColors.Primary.B));
            
            _corporationFrame.Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(40, 0, 0, 0), 0),
                    new GradientStop(influenceBrush.Color, 1)
                }
            };
        }
    }

    #endregion

    #region Animation Methods

    private void AnimateInsigniaRefresh()
    {
        if (_isSimplifiedMode) return;

        var rotateTransform = new RotateTransform();
        _insigniaContainer.RenderTransform = rotateTransform;
        _insigniaContainer.RenderTransformOrigin = new Point(0.5, 0.5);

        var rotateAnimation = new DoubleAnimation
        {
            From = 0,
            To = 360,
            Duration = TimeSpan.FromMilliseconds(800),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
    }

    private async Task AnimateImageLoad(UIElement element)
    {
        if (_isSimplifiedMode) return;

        element.Opacity = 0;

        var fadeAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(600),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        element.BeginAnimation(OpacityProperty, fadeAnimation);
        await Task.Delay(600);
    }

    private void AnimateHoverEffect(bool isEntering)
    {
        if (_isSimplifiedMode) return;

        var targetScale = isEntering ? 1.1 : 1.0;
        var targetIntensity = isEntering ? 1.5 : 1.0;

        var scaleTransform = _insigniaContainer.RenderTransform as ScaleTransform ?? new ScaleTransform();
        _insigniaContainer.RenderTransform = scaleTransform;
        _insigniaContainer.RenderTransformOrigin = new Point(0.5, 0.5);

        var scaleAnimation = new DoubleAnimation
        {
            To = targetScale,
            Duration = TimeSpan.FromMilliseconds(200),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);

        // Enhance glow effects
        AnimateGlowEffects(targetIntensity);
    }

    private void AnimateGlowEffects(double intensity)
    {
        var elements = new[] { _corporationFrame, _allianceFrame, _factionFrame }.Where(e => e != null);

        foreach (var element in elements)
        {
            if (element.Effect is DropShadowEffect effect)
            {
                var glowAnimation = new DoubleAnimation
                {
                    To = effect.BlurRadius * intensity,
                    Duration = TimeSpan.FromMilliseconds(200)
                };
                effect.BeginAnimation(DropShadowEffect.BlurRadiusProperty, glowAnimation);
            }
        }
    }

    #endregion

    #region Particle Effects

    private void SpawnInsigniaParticles()
    {
        if (_isSimplifiedMode || !EnableParticleEffects) return;

        if (_random.NextDouble() < 0.2) // 20% chance
        {
            var particle = new Ellipse
            {
                Width = _random.Next(2, 5),
                Height = _random.Next(2, 5),
                Fill = new SolidColorBrush(GetParticleColor())
            };

            var startX = _random.NextDouble() * Width;
            var startY = _random.NextDouble() * Height;

            Canvas.SetLeft(particle, startX);
            Canvas.SetTop(particle, startY);
            _particleCanvas.Children.Add(particle);
            _insigniaParticles.Add(particle);

            AnimateInsigniaParticle(particle);
        }

        CleanupInsigniaParticles();
    }

    private void SpawnHologramLines()
    {
        if (_isSimplifiedMode || !EnableHologramEffect) return;

        // Clear old lines
        foreach (var line in _hologramLines.ToList())
        {
            _hologramCanvas.Children.Remove(line);
        }
        _hologramLines.Clear();

        // Create new hologram lines
        var lineCount = _random.Next(2, 5);
        for (int i = 0; i < lineCount; i++)
        {
            var line = new Rectangle
            {
                Width = Width,
                Height = 1,
                Fill = new SolidColorBrush(Color.FromArgb(80, 100, 200, 255)),
                Opacity = 0.3 + (_random.NextDouble() * 0.4)
            };

            var y = _random.NextDouble() * Height;
            Canvas.SetLeft(line, 0);
            Canvas.SetTop(line, y);

            _hologramCanvas.Children.Add(line);
            _hologramLines.Add(line);

            // Animate line
            var fadeAnimation = new DoubleAnimation
            {
                From = line.Opacity,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(_random.Next(500, 1000))
            };

            fadeAnimation.Completed += (s, e) =>
            {
                _hologramCanvas.Children.Remove(line);
                _hologramLines.Remove(line);
            };

            line.BeginAnimation(OpacityProperty, fadeAnimation);
        }
    }

    private void AnimateInsigniaParticle(UIElement particle)
    {
        var angle = _random.NextDouble() * Math.PI * 2;
        var distance = _random.Next(20, 40);
        var targetX = Canvas.GetLeft(particle) + Math.Cos(angle) * distance;
        var targetY = Canvas.GetTop(particle) + Math.Sin(angle) * distance;

        var moveXAnimation = new DoubleAnimation
        {
            From = Canvas.GetLeft(particle),
            To = targetX,
            Duration = TimeSpan.FromMilliseconds(_random.Next(1000, 1500)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var moveYAnimation = new DoubleAnimation
        {
            From = Canvas.GetTop(particle),
            To = targetY,
            Duration = TimeSpan.FromMilliseconds(_random.Next(1000, 1500)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(_random.Next(1000, 1500))
        };

        moveXAnimation.Completed += (s, e) =>
        {
            _particleCanvas.Children.Remove(particle);
            _insigniaParticles.Remove(particle);
        };

        particle.BeginAnimation(Canvas.LeftProperty, moveXAnimation);
        particle.BeginAnimation(Canvas.TopProperty, moveYAnimation);
        particle.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void CleanupInsigniaParticles()
    {
        if (_insigniaParticles.Count > 20)
        {
            var particlesToRemove = _insigniaParticles.Take(10).ToList();
            foreach (var particle in particlesToRemove)
            {
                _particleCanvas.Children.Remove(particle);
                _insigniaParticles.Remove(particle);
            }
        }
    }

    #endregion

    #region Helper Methods

    private (double Width, double Height) GetInsigniaDimensions()
    {
        return InsigniaSize switch
        {
            InsigniaSize.Small => (80, 80),
            InsigniaSize.Medium => (120, 120),
            InsigniaSize.Large => (160, 160),
            InsigniaSize.ExtraLarge => (200, 200),
            _ => (120, 120)
        };
    }

    private double GetLogoSize()
    {
        return InsigniaSize switch
        {
            InsigniaSize.Small => 40,
            InsigniaSize.Medium => 60,
            InsigniaSize.Large => 80,
            InsigniaSize.ExtraLarge => 100,
            _ => 60
        };
    }

    private double GetTextSize()
    {
        return InsigniaSize switch
        {
            InsigniaSize.Small => 10,
            InsigniaSize.Medium => 12,
            InsigniaSize.Large => 14,
            InsigniaSize.ExtraLarge => 16,
            _ => 12
        };
    }

    private Color GetParticleColor()
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        return Color.FromArgb(
            (byte)_random.Next(100, 200),
            colors.Primary.R, colors.Primary.G, colors.Primary.B);
    }

    private (Color Primary, Color Secondary) GetFactionColors(int factionId)
    {
        return factionId switch
        {
            // Amarr Empire
            500003 => (Color.FromRgb(255, 215, 0), Color.FromRgb(139, 69, 19)),
            // Caldari State
            500001 => (Color.FromRgb(64, 224, 255), Color.FromRgb(25, 25, 112)),
            // Gallente Federation
            500004 => (Color.FromRgb(50, 205, 50), Color.FromRgb(0, 100, 0)),
            // Minmatar Republic
            500002 => (Color.FromRgb(220, 20, 60), Color.FromRgb(139, 0, 0)),
            // Default
            _ => (Color.FromRgb(150, 150, 150), Color.FromRgb(100, 100, 100))
        };
    }

    private ImageSource CreatePlaceholderLogo()
    {
        var visual = new Grid
        {
            Width = 64,
            Height = 64,
            Background = new SolidColorBrush(Color.FromArgb(100, 100, 100, 100))
        };

        var text = new TextBlock
        {
            Text = "CORP",
            FontSize = 12,
            Foreground = new SolidColorBrush(Colors.White),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        visual.Children.Add(text);

        var renderBitmap = new RenderTargetBitmap(64, 64, 96, 96, PixelFormats.Pbgra32);
        renderBitmap.Render(visual);
        return renderBitmap;
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _ = UpdateInsigniaDisplayAsync();
        if (EnableAnimations && !_isSimplifiedMode)
        {
            _animationTimer.Start();
            if (EnableHologramEffect)
            {
                _hologramTimer.Start();
            }
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        _hologramTimer?.Stop();
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
            SpawnInsigniaParticles();
        }
    }

    private void OnHologramTimerTick(object sender, EventArgs e)
    {
        if (EnableHologramEffect && !_isSimplifiedMode)
        {
            _hologramPhase += 0.2;
            if (_hologramPhase > Math.PI * 2)
                _hologramPhase = 0;

            if (_random.NextDouble() < 0.3) // 30% chance
            {
                SpawnHologramLines();
            }
        }
    }

    private void UpdateAnimatedElements()
    {
        // Pulse glow effects
        var pulseIntensity = 0.8 + (Math.Sin(_animationPhase * 2) * 0.2);
        
        var elements = new[] { _corporationFrame, _allianceFrame, _factionFrame }.Where(e => e != null);
        foreach (var element in elements)
        {
            if (element.Effect is DropShadowEffect effect)
            {
                effect.Opacity = pulseIntensity * HolographicIntensity;
            }
        }
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        _isHovered = true;
        if (EnableAnimations && !_isSimplifiedMode)
        {
            AnimateHoverEffect(true);
        }

        InsigniaHovered?.Invoke(this, new HoloCorporationEventArgs
        {
            Corporation = CorporationData,
            Timestamp = DateTime.Now
        });
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        _isHovered = false;
        if (EnableAnimations && !_isSimplifiedMode)
        {
            AnimateHoverEffect(false);
        }
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            InsigniaDetailsRequested?.Invoke(this, new HoloCorporationEventArgs
            {
                Corporation = CorporationData,
                Timestamp = DateTime.Now
            });
        }
    }

    private void OnCorporationClicked(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        CorporationClicked?.Invoke(this, new HoloCorporationEventArgs
        {
            Corporation = CorporationData,
            Timestamp = DateTime.Now
        });
    }

    private void OnAllianceClicked(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        AllianceClicked?.Invoke(this, new HoloCorporationEventArgs
        {
            Corporation = CorporationData,
            Timestamp = DateTime.Now
        });
    }

    private void OnFactionClicked(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        FactionClicked?.Invoke(this, new HoloCorporationEventArgs
        {
            Corporation = CorporationData,
            Timestamp = DateTime.Now
        });
    }

    private void CleanupAllParticles()
    {
        foreach (var particle in _insigniaParticles.ToList())
        {
            _particleCanvas.Children.Remove(particle);
        }
        _insigniaParticles.Clear();

        foreach (var line in _hologramLines.ToList())
        {
            _hologramCanvas.Children.Remove(line);
        }
        _hologramLines.Clear();
    }

    #endregion

    #region Property Change Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCorporationInsignia insignia)
        {
            insignia.ApplyHolographicIntensity();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCorporationInsignia insignia)
        {
            insignia.ApplyEVEColorScheme();
        }
    }

    private static void OnCorporationDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCorporationInsignia insignia)
        {
            _ = insignia.UpdateInsigniaDisplayAsync();
        }
    }

    private static void OnInsigniaStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCorporationInsignia insignia)
        {
            insignia.InitializeComponent();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCorporationInsignia insignia)
        {
            if ((bool)e.NewValue && !insignia._isSimplifiedMode)
            {
                insignia._animationTimer?.Start();
            }
            else
            {
                insignia._animationTimer?.Stop();
            }
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCorporationInsignia insignia && !(bool)e.NewValue)
        {
            insignia.CleanupAllParticles();
        }
    }

    private static void OnShowFactionEmblemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCorporationInsignia insignia)
        {
            if (insignia._factionFrame != null)
            {
                insignia._factionFrame.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }

    private static void OnShowAllianceBannerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCorporationInsignia insignia)
        {
            if (insignia._allianceFrame != null)
            {
                insignia._allianceFrame.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            }
            if (insignia._allianceName != null)
            {
                insignia._allianceName.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }

    private static void OnInsigniaSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCorporationInsignia insignia)
        {
            insignia.InitializeComponent();
        }
    }

    private static void OnEnableHologramEffectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCorporationInsignia insignia)
        {
            if ((bool)e.NewValue && !insignia._isSimplifiedMode)
            {
                insignia._hologramTimer?.Start();
            }
            else
            {
                insignia._hologramTimer?.Stop();
            }
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
        EnableHologramEffect = false;
        _animationTimer?.Stop();
        _hologramTimer?.Stop();
    }

    public void ExitSimplifiedMode()
    {
        _isSimplifiedMode = false;
        EnableAnimations = true;
        EnableParticleEffects = true;
        EnableHologramEffect = true;
        _animationTimer?.Start();
        _hologramTimer?.Start();
    }

    #endregion

    #region Color Scheme Application

    private void ApplyEVEColorScheme()
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        
        if (_corporationFrame != null)
        {
            _corporationFrame.BorderBrush = new SolidColorBrush(Color.FromArgb(150, colors.Primary.R, colors.Primary.G, colors.Primary.B));
            if (_corporationFrame.Effect is DropShadowEffect corpEffect)
            {
                corpEffect.Color = Color.FromArgb(100, colors.Primary.R, colors.Primary.G, colors.Primary.B);
            }
        }
    }

    private void ApplyHolographicIntensity()
    {
        var intensity = HolographicIntensity;
        
        var elements = new[] { _corporationFrame, _allianceFrame, _factionFrame }.Where(e => e != null);
        foreach (var element in elements)
        {
            if (element.Effect is DropShadowEffect shadow)
            {
                shadow.BlurRadius = shadow.BlurRadius * intensity;
                shadow.ShadowDepth = shadow.ShadowDepth * intensity;
            }
        }

        EnableAnimations = intensity > 0.3;
        EnableParticleEffects = intensity > 0.5;
    }

    #endregion
}

#region Supporting Classes and Enums

public enum InsigniaStyle
{
    Minimal,
    Compact,
    Standard,
    Detailed,
    Banner
}

public enum InsigniaSize
{
    Small,
    Medium,
    Large,
    ExtraLarge
}

public class HoloCorporationData
{
    public string CorporationName { get; set; }
    public string AllianceName { get; set; }
    public string CorporationLogoUrl { get; set; }
    public string AllianceLogoUrl { get; set; }
    public string FactionEmblemUrl { get; set; }
    public int? FactionId { get; set; }
    public double FactionStanding { get; set; }
    public DateTime Founded { get; set; }
    public int MemberCount { get; set; }
    public string Ticker { get; set; }
    public string Description { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

public class HoloCorporationEventArgs : EventArgs
{
    public HoloCorporationData Corporation { get; set; }
    public DateTime Timestamp { get; set; }
}

#endregion