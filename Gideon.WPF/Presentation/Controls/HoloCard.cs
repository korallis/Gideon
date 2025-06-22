// ==========================================================================
// HoloCard.cs - Holographic Data Display Card with Advanced Depth Effects
// ==========================================================================
// Advanced holographic card control for data display featuring depth effects,
// animated headers, data visualization integration, and EVE-style aesthetics.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
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
/// Advanced holographic card for data display with depth effects and EVE styling
/// </summary>
public class HoloCard : ContentControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(HoloCard),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(HoloCard),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty HeaderContentProperty =
        DependencyProperty.Register(nameof(HeaderContent), typeof(object), typeof(HoloCard),
            new PropertyMetadata(null));

    public static readonly DependencyProperty FooterContentProperty =
        DependencyProperty.Register(nameof(FooterContent), typeof(object), typeof(HoloCard),
            new PropertyMetadata(null));

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloCard),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloCard),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty DepthLayerProperty =
        DependencyProperty.Register(nameof(DepthLayer), typeof(HolographicDepth), typeof(HoloCard),
            new PropertyMetadata(HolographicDepth.Mid, OnDepthLayerChanged));

    public static readonly DependencyProperty CardStateProperty =
        DependencyProperty.Register(nameof(CardState), typeof(HoloCardState), typeof(HoloCard),
            new PropertyMetadata(HoloCardState.Normal, OnCardStateChanged));

    public static readonly DependencyProperty EnableHoverEffectsProperty =
        DependencyProperty.Register(nameof(EnableHoverEffects), typeof(bool), typeof(HoloCard),
            new PropertyMetadata(true));

    public static readonly DependencyProperty EnableDepthAnimationProperty =
        DependencyProperty.Register(nameof(EnableDepthAnimation), typeof(bool), typeof(HoloCard),
            new PropertyMetadata(true, OnEnableDepthAnimationChanged));

    public static readonly DependencyProperty EnableDataVisualizationProperty =
        DependencyProperty.Register(nameof(EnableDataVisualization), typeof(bool), typeof(HoloCard),
            new PropertyMetadata(false, OnEnableDataVisualizationChanged));

    public static readonly DependencyProperty IsExpandableProperty =
        DependencyProperty.Register(nameof(IsExpandable), typeof(bool), typeof(HoloCard),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(HoloCard),
            new PropertyMetadata(false, OnIsExpandedChanged));

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(HoloCard),
            new PropertyMetadata(new CornerRadius(8)));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public object HeaderContent
    {
        get => GetValue(HeaderContentProperty);
        set => SetValue(HeaderContentProperty, value);
    }

    public object FooterContent
    {
        get => GetValue(FooterContentProperty);
        set => SetValue(FooterContentProperty, value);
    }

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

    public HolographicDepth DepthLayer
    {
        get => (HolographicDepth)GetValue(DepthLayerProperty);
        set => SetValue(DepthLayerProperty, value);
    }

    public HoloCardState CardState
    {
        get => (HoloCardState)GetValue(CardStateProperty);
        set => SetValue(CardStateProperty, value);
    }

    public bool EnableHoverEffects
    {
        get => (bool)GetValue(EnableHoverEffectsProperty);
        set => SetValue(EnableHoverEffectsProperty, value);
    }

    public bool EnableDepthAnimation
    {
        get => (bool)GetValue(EnableDepthAnimationProperty);
        set => SetValue(EnableDepthAnimationProperty, value);
    }

    public bool EnableDataVisualization
    {
        get => (bool)GetValue(EnableDataVisualizationProperty);
        set => SetValue(EnableDataVisualizationProperty, value);
    }

    public bool IsExpandable
    {
        get => (bool)GetValue(IsExpandableProperty);
        set => SetValue(IsExpandableProperty, value);
    }

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloCardEventArgs> CardClicked;
    public event EventHandler<HoloCardEventArgs> CardExpanded;
    public event EventHandler<HoloCardEventArgs> CardCollapsed;

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode { get; private set; }

    public void SwitchToFullMode()
    {
        IsInSimplifiedMode = false;
        EnableDepthAnimation = true;
        EnableHoverEffects = true;
        UpdateCardAppearance();
    }

    public void SwitchToSimplifiedMode()
    {
        IsInSimplifiedMode = true;
        EnableDepthAnimation = false;
        EnableHoverEffects = false;
        UpdateCardAppearance();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => !_disposed && IsLoaded;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        EnableDepthAnimation = settings.EnabledFeatures.HasFlag(AnimationFeatures.MotionEffects);
        EnableHoverEffects = settings.EnabledFeatures.HasFlag(AnimationFeatures.BasicGlow);
        
        UpdateCardAppearance();
    }

    #endregion

    #region Fields

    private Grid _mainGrid;
    private Border _cardBorder;
    private Border _headerBorder;
    private Border _footerBorder;
    private ContentPresenter _contentPresenter;
    private ContentPresenter _headerContentPresenter;
    private ContentPresenter _footerContentPresenter;
    private TextBlock _titleBlock;
    private TextBlock _subtitleBlock;
    private Canvas _dataVisualizationCanvas;
    private Rectangle _depthLayer;
    private Path _expandIcon;
    
    private DispatcherTimer _depthAnimationTimer;
    private Storyboard _hoverAnimation;
    private Storyboard _expandAnimation;
    private double _depthPhase = 0;
    private bool _isHovered = false;
    private bool _disposed = false;

    #endregion

    #region Constructor

    public HoloCard()
    {
        DefaultStyleKey = typeof(HoloCard);
        InitializeCard();
        SetupAnimations();
        
        // Register with animation intensity manager
        AnimationIntensityManager.Instance.RegisterTarget(this);
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        MouseEnter += OnMouseEnter;
        MouseLeave += OnMouseLeave;
        PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
    }

    #endregion

    #region Private Methods

    private void InitializeCard()
    {
        Template = CreateCardTemplate();
        UpdateCardAppearance();
    }

    private ControlTemplate CreateCardTemplate()
    {
        var template = new ControlTemplate(typeof(HoloCard));

        // Main grid container
        var mainGrid = new FrameworkElementFactory(typeof(Grid));
        mainGrid.Name = "PART_MainGrid";

        // Row definitions
        var headerRow = new FrameworkElementFactory(typeof(RowDefinition));
        headerRow.SetValue(RowDefinition.HeightProperty, GridLength.Auto);
        var contentRow = new FrameworkElementFactory(typeof(RowDefinition));
        contentRow.SetValue(RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Star));
        var footerRow = new FrameworkElementFactory(typeof(RowDefinition));
        footerRow.SetValue(RowDefinition.HeightProperty, GridLength.Auto);

        mainGrid.AppendChild(headerRow);
        mainGrid.AppendChild(contentRow);
        mainGrid.AppendChild(footerRow);

        // Depth background layer
        var depthLayer = new FrameworkElementFactory(typeof(Rectangle));
        depthLayer.Name = "PART_DepthLayer";
        depthLayer.SetValue(Rectangle.RadiusXProperty, new TemplateBindingExtension(CornerRadiusProperty));
        depthLayer.SetValue(Rectangle.RadiusYProperty, new TemplateBindingExtension(CornerRadiusProperty));
        depthLayer.SetValue(Grid.RowSpanProperty, 3);

        // Main card border
        var cardBorder = new FrameworkElementFactory(typeof(Border));
        cardBorder.Name = "PART_CardBorder";
        cardBorder.SetValue(Border.CornerRadiusProperty, new TemplateBindingExtension(CornerRadiusProperty));
        cardBorder.SetValue(Border.BorderThicknessProperty, new Thickness(1));
        cardBorder.SetValue(Grid.RowSpanProperty, 3);

        // Header section
        var headerBorder = new FrameworkElementFactory(typeof(Border));
        headerBorder.Name = "PART_HeaderBorder";
        headerBorder.SetValue(Grid.RowProperty, 0);
        headerBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(8, 8, 0, 0));
        headerBorder.SetValue(Border.PaddingProperty, new Thickness(16, 12, 16, 12));

        var headerStack = new FrameworkElementFactory(typeof(StackPanel));
        headerStack.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

        // Title and subtitle stack
        var titleStack = new FrameworkElementFactory(typeof(StackPanel));
        titleStack.SetValue(StackPanel.OrientationProperty, Orientation.Vertical);

        var titleBlock = new FrameworkElementFactory(typeof(TextBlock));
        titleBlock.Name = "PART_TitleBlock";
        titleBlock.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        titleBlock.SetValue(TextBlock.FontSizeProperty, 16.0);
        titleBlock.SetBinding(TextBlock.TextProperty, new TemplateBindingExtension(TitleProperty));

        var subtitleBlock = new FrameworkElementFactory(typeof(TextBlock));
        subtitleBlock.Name = "PART_SubtitleBlock";
        subtitleBlock.SetValue(TextBlock.FontSizeProperty, 12.0);
        subtitleBlock.SetValue(TextBlock.OpacityProperty, 0.7);
        subtitleBlock.SetBinding(TextBlock.TextProperty, new TemplateBindingExtension(SubtitleProperty));

        titleStack.AppendChild(titleBlock);
        titleStack.AppendChild(subtitleBlock);

        // Header content presenter
        var headerContentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
        headerContentPresenter.Name = "PART_HeaderContentPresenter";
        headerContentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Right);
        headerContentPresenter.SetBinding(ContentPresenter.ContentProperty, new TemplateBindingExtension(HeaderContentProperty));

        // Expand icon
        var expandIcon = new FrameworkElementFactory(typeof(Path));
        expandIcon.Name = "PART_ExpandIcon";
        expandIcon.SetValue(Path.WidthProperty, 12.0);
        expandIcon.SetValue(Path.HeightProperty, 12.0);
        expandIcon.SetValue(Path.StretchProperty, Stretch.Uniform);
        expandIcon.SetValue(Path.HorizontalAlignmentProperty, HorizontalAlignment.Right);
        expandIcon.SetValue(Path.VerticalAlignmentProperty, VerticalAlignment.Center);
        expandIcon.SetValue(Path.MarginProperty, new Thickness(8, 0, 0, 0));

        headerStack.AppendChild(titleStack);
        headerStack.AppendChild(headerContentPresenter);
        headerStack.AppendChild(expandIcon);
        headerBorder.AppendChild(headerStack);

        // Content section
        var contentGrid = new FrameworkElementFactory(typeof(Grid));
        contentGrid.SetValue(Grid.RowProperty, 1);

        var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
        contentPresenter.Name = "PART_ContentPresenter";
        contentPresenter.SetValue(ContentPresenter.MarginProperty, new Thickness(16, 12, 16, 12));

        // Data visualization canvas
        var dataVisualizationCanvas = new FrameworkElementFactory(typeof(Canvas));
        dataVisualizationCanvas.Name = "PART_DataVisualizationCanvas";
        dataVisualizationCanvas.SetValue(Canvas.IsHitTestVisibleProperty, false);
        dataVisualizationCanvas.SetValue(Canvas.OpacityProperty, 0.3);

        contentGrid.AppendChild(contentPresenter);
        contentGrid.AppendChild(dataVisualizationCanvas);

        // Footer section
        var footerBorder = new FrameworkElementFactory(typeof(Border));
        footerBorder.Name = "PART_FooterBorder";
        footerBorder.SetValue(Grid.RowProperty, 2);
        footerBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(0, 0, 8, 8));
        footerBorder.SetValue(Border.PaddingProperty, new Thickness(16, 8, 16, 8));

        var footerContentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
        footerContentPresenter.Name = "PART_FooterContentPresenter";
        footerContentPresenter.SetBinding(ContentPresenter.ContentProperty, new TemplateBindingExtension(FooterContentProperty));

        footerBorder.AppendChild(footerContentPresenter);

        // Assembly
        mainGrid.AppendChild(depthLayer);
        mainGrid.AppendChild(cardBorder);
        mainGrid.AppendChild(headerBorder);
        mainGrid.AppendChild(contentGrid);
        mainGrid.AppendChild(footerBorder);

        template.VisualTree = mainGrid;
        return template;
    }

    private void SetupAnimations()
    {
        // Depth animation timer
        _depthAnimationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50) // 20 FPS
        };
        _depthAnimationTimer.Tick += OnDepthAnimationTick;

        CreateHoverAnimation();
        CreateExpandAnimation();
    }

    private void CreateHoverAnimation()
    {
        _hoverAnimation = new Storyboard();

        // Elevation animation
        var elevationAnimation = new DoubleAnimation
        {
            To = -3,
            Duration = TimeSpan.FromMilliseconds(200),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTargetProperty(elevationAnimation, new PropertyPath("RenderTransform.Y"));

        // Glow intensity animation
        var glowAnimation = new DoubleAnimation
        {
            To = 1.5,
            Duration = TimeSpan.FromMilliseconds(200),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTargetProperty(glowAnimation, new PropertyPath("Effect.Opacity"));

        _hoverAnimation.Children.Add(elevationAnimation);
        _hoverAnimation.Children.Add(glowAnimation);
    }

    private void CreateExpandAnimation()
    {
        _expandAnimation = new Storyboard();

        // Height animation for expansion
        var heightAnimation = new DoubleAnimation
        {
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };
        Storyboard.SetTargetProperty(heightAnimation, new PropertyPath("Height"));

        // Opacity animation for content
        var opacityAnimation = new DoubleAnimation
        {
            Duration = TimeSpan.FromMilliseconds(200),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };
        Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));

        _expandAnimation.Children.Add(heightAnimation);
        _expandAnimation.Children.Add(opacityAnimation);
    }

    private void OnDepthAnimationTick(object sender, EventArgs e)
    {
        if (!EnableDepthAnimation || IsInSimplifiedMode) return;

        _depthPhase += 0.08;
        if (_depthPhase > Math.PI * 2)
            _depthPhase = 0;

        UpdateDepthEffects();
    }

    private void UpdateDepthEffects()
    {
        if (_depthLayer == null) return;

        // Subtle depth pulsing
        var depthIntensity = 1.0 + (Math.Sin(_depthPhase) * 0.1);
        
        if (_depthLayer.Effect is DropShadowEffect depthEffect)
        {
            depthEffect.Opacity = 0.3 * depthIntensity * HolographicIntensity;
        }

        // Subtle transform for parallax effect
        if (_isHovered && RenderTransform is TranslateTransform transform)
        {
            transform.Y = Math.Sin(_depthPhase * 0.5) * 0.5;
        }
    }

    private void UpdateCardAppearance()
    {
        if (Template == null) return;

        Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            ApplyTemplate();
            
            _mainGrid = GetTemplateChild("PART_MainGrid") as Grid;
            _cardBorder = GetTemplateChild("PART_CardBorder") as Border;
            _headerBorder = GetTemplateChild("PART_HeaderBorder") as Border;
            _footerBorder = GetTemplateChild("PART_FooterBorder") as Border;
            _contentPresenter = GetTemplateChild("PART_ContentPresenter") as ContentPresenter;
            _headerContentPresenter = GetTemplateChild("PART_HeaderContentPresenter") as ContentPresenter;
            _footerContentPresenter = GetTemplateChild("PART_FooterContentPresenter") as ContentPresenter;
            _titleBlock = GetTemplateChild("PART_TitleBlock") as TextBlock;
            _subtitleBlock = GetTemplateChild("PART_SubtitleBlock") as TextBlock;
            _dataVisualizationCanvas = GetTemplateChild("PART_DataVisualizationCanvas") as Canvas;
            _depthLayer = GetTemplateChild("PART_DepthLayer") as Rectangle;
            _expandIcon = GetTemplateChild("PART_ExpandIcon") as Path;

            UpdateColors();
            UpdateEffects();
            UpdateDataVisualization();
            UpdateExpandIcon();
            UpdateVisibility();
            SetupRenderTransform();
        }), DispatcherPriority.Render);
    }

    private void UpdateColors()
    {
        var color = GetEVEColor(EVEColorScheme);
        var stateColor = GetStateColor(CardState);
        var depthAlpha = GetDepthAlpha(DepthLayer);

        // Card background
        if (_cardBorder != null)
        {
            var backgroundBrush = new RadialGradientBrush();
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb((byte)(40 * depthAlpha), stateColor.R, stateColor.G, stateColor.B), 0.0));
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb((byte)(20 * depthAlpha), stateColor.R, stateColor.G, stateColor.B), 0.7));
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb((byte)(60 * depthAlpha), stateColor.R, stateColor.G, stateColor.B), 1.0));

            _cardBorder.Background = backgroundBrush;
            _cardBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(
                (byte)(150 * depthAlpha), stateColor.R, stateColor.G, stateColor.B));
        }

        // Header background
        if (_headerBorder != null)
        {
            var headerBrush = new LinearGradientBrush();
            headerBrush.StartPoint = new Point(0, 0);
            headerBrush.EndPoint = new Point(1, 0);
            headerBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb((byte)(80 * depthAlpha), color.R, color.G, color.B), 0.0));
            headerBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb((byte)(40 * depthAlpha), color.R, color.G, color.B), 1.0));

            _headerBorder.Background = headerBrush;
        }

        // Footer background
        if (_footerBorder != null && FooterContent != null)
        {
            _footerBorder.Background = new SolidColorBrush(Color.FromArgb(
                (byte)(30 * depthAlpha), color.R, color.G, color.B));
        }

        // Text colors
        if (_titleBlock != null)
        {
            _titleBlock.Foreground = new SolidColorBrush(Color.FromArgb(
                (byte)(255 * depthAlpha), color.R, color.G, color.B));
        }

        if (_subtitleBlock != null)
        {
            _subtitleBlock.Foreground = new SolidColorBrush(Color.FromArgb(
                (byte)(180 * depthAlpha), color.R, color.G, color.B));
        }

        // Expand icon color
        if (_expandIcon != null)
        {
            _expandIcon.Fill = new SolidColorBrush(Color.FromArgb(
                (byte)(200 * depthAlpha), color.R, color.G, color.B));
        }
    }

    private void UpdateEffects()
    {
        var color = GetEVEColor(EVEColorScheme);
        var stateColor = GetStateColor(CardState);
        var depthAlpha = GetDepthAlpha(DepthLayer);

        // Card glow effect
        if (_cardBorder != null && !IsInSimplifiedMode)
        {
            _cardBorder.Effect = new DropShadowEffect
            {
                Color = stateColor,
                BlurRadius = 12 * HolographicIntensity,
                ShadowDepth = 0,
                Opacity = 0.4 * HolographicIntensity * depthAlpha
            };
        }
        else if (_cardBorder != null)
        {
            _cardBorder.Effect = new DropShadowEffect
            {
                Color = stateColor,
                BlurRadius = 3,
                ShadowDepth = 0,
                Opacity = 0.2 * HolographicIntensity
            };
        }

        // Depth layer effect
        if (_depthLayer != null && !IsInSimplifiedMode)
        {
            _depthLayer.Fill = new SolidColorBrush(Color.FromArgb(20, 0, 0, 0));
            _depthLayer.Effect = new BlurEffect
            {
                Radius = 5,
                RenderingBias = RenderingBias.Performance
            };
        }
    }

    private void UpdateDataVisualization()
    {
        if (_dataVisualizationCanvas == null || !EnableDataVisualization) return;

        _dataVisualizationCanvas.Children.Clear();

        // Create simple data visualization elements
        CreateDataPoints();
        CreateDataLines();
    }

    private void CreateDataPoints()
    {
        var color = GetEVEColor(EVEColorScheme);
        var random = new Random();

        for (int i = 0; i < 8; i++)
        {
            var point = new Ellipse
            {
                Width = 3,
                Height = 3,
                Fill = new SolidColorBrush(Color.FromArgb(120, color.R, color.G, color.B))
            };

            Canvas.SetLeft(point, random.NextDouble() * 200);
            Canvas.SetTop(point, random.NextDouble() * 60);
            _dataVisualizationCanvas.Children.Add(point);
        }
    }

    private void CreateDataLines()
    {
        var color = GetEVEColor(EVEColorScheme);
        var random = new Random();

        for (int i = 0; i < 3; i++)
        {
            var line = new Line
            {
                X1 = random.NextDouble() * 200,
                Y1 = random.NextDouble() * 60,
                X2 = random.NextDouble() * 200,
                Y2 = random.NextDouble() * 60,
                Stroke = new SolidColorBrush(Color.FromArgb(60, color.R, color.G, color.B)),
                StrokeThickness = 1
            };

            _dataVisualizationCanvas.Children.Add(line);
        }
    }

    private void UpdateExpandIcon()
    {
        if (_expandIcon == null) return;

        // Chevron down geometry
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            if (IsExpanded)
            {
                // Chevron up
                ctx.BeginFigure(new Point(2, 8), false, false);
                ctx.LineTo(new Point(6, 4), true, false);
                ctx.LineTo(new Point(10, 8), true, false);
            }
            else
            {
                // Chevron down
                ctx.BeginFigure(new Point(2, 4), false, false);
                ctx.LineTo(new Point(6, 8), true, false);
                ctx.LineTo(new Point(10, 4), true, false);
            }
        }
        geometry.Freeze();
        _expandIcon.Data = geometry;

        _expandIcon.Visibility = IsExpandable ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateVisibility()
    {
        if (_footerBorder != null)
        {
            _footerBorder.Visibility = FooterContent != null ? Visibility.Visible : Visibility.Collapsed;
        }

        if (_headerContentPresenter != null)
        {
            _headerContentPresenter.Visibility = HeaderContent != null ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void SetupRenderTransform()
    {
        if (RenderTransform == null || !(RenderTransform is TranslateTransform))
        {
            RenderTransform = new TranslateTransform(0, 0);
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

    private Color GetStateColor(HoloCardState state)
    {
        return state switch
        {
            HoloCardState.Normal => GetEVEColor(EVEColorScheme),
            HoloCardState.Success => Color.FromRgb(50, 205, 50),
            HoloCardState.Warning => Color.FromRgb(255, 215, 0),
            HoloCardState.Error => Color.FromRgb(220, 20, 60),
            HoloCardState.Info => Color.FromRgb(64, 224, 255),
            _ => GetEVEColor(EVEColorScheme)
        };
    }

    private double GetDepthAlpha(HolographicDepth depth)
    {
        return depth switch
        {
            HolographicDepth.Background => 0.4,
            HolographicDepth.Mid => 0.7,
            HolographicDepth.Foreground => 0.9,
            HolographicDepth.Overlay => 1.0,
            _ => 0.7
        };
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableDepthAnimation && !IsInSimplifiedMode)
            _depthAnimationTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _depthAnimationTimer?.Stop();
        _hoverAnimation?.Stop();
        _expandAnimation?.Stop();
        
        // Unregister from animation intensity manager
        AnimationIntensityManager.Instance.UnregisterTarget(this);
        
        _disposed = true;
    }

    #endregion

    #region Event Handlers

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (!EnableHoverEffects || IsInSimplifiedMode) return;

        _isHovered = true;
        
        _hoverAnimation?.Stop();
        if (RenderTransform is TranslateTransform && _cardBorder?.Effect != null)
        {
            Storyboard.SetTarget(_hoverAnimation.Children[0], this);
            Storyboard.SetTarget(_hoverAnimation.Children[1], _cardBorder);
            _hoverAnimation.Begin();
        }
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (!EnableHoverEffects || IsInSimplifiedMode) return;

        _isHovered = false;
        
        // Return to normal state
        if (RenderTransform is TranslateTransform transform)
        {
            var returnAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200)
            };
            transform.BeginAnimation(TranslateTransform.YProperty, returnAnimation);
        }

        if (_cardBorder?.Effect is DropShadowEffect effect)
        {
            var glowReturn = new DoubleAnimation
            {
                To = 0.4 * HolographicIntensity,
                Duration = TimeSpan.FromMilliseconds(200)
            };
            effect.BeginAnimation(DropShadowEffect.OpacityProperty, glowReturn);
        }
    }

    private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (IsExpandable)
        {
            IsExpanded = !IsExpanded;
        }

        CardClicked?.Invoke(this, new HoloCardEventArgs { Card = this });
    }

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCard card)
            card.UpdateCardAppearance();
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCard card)
            card.UpdateCardAppearance();
    }

    private static void OnDepthLayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCard card)
            card.UpdateCardAppearance();
    }

    private static void OnCardStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCard card)
            card.UpdateCardAppearance();
    }

    private static void OnEnableDepthAnimationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCard card)
        {
            if ((bool)e.NewValue && !card.IsInSimplifiedMode)
                card._depthAnimationTimer.Start();
            else
                card._depthAnimationTimer.Stop();
        }
    }

    private static void OnEnableDataVisualizationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCard card)
            card.UpdateCardAppearance();
    }

    private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCard card)
        {
            card.UpdateExpandIcon();
            
            if ((bool)e.NewValue)
                card.CardExpanded?.Invoke(card, new HoloCardEventArgs { Card = card });
            else
                card.CardCollapsed?.Invoke(card, new HoloCardEventArgs { Card = card });
        }
    }

    #endregion
}

/// <summary>
/// Holographic card states for different visual modes
/// </summary>
public enum HoloCardState
{
    Normal,
    Success,
    Warning,
    Error,
    Info
}

/// <summary>
/// Event args for holographic card events
/// </summary>
public class HoloCardEventArgs : EventArgs
{
    public HoloCard Card { get; set; }
}