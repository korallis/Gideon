// ==========================================================================
// AngularBorderPanel.cs - EVE Angular Border Panel with Corporation Support
// ==========================================================================
// Advanced angular border control with corporation insignia support, 
// animated corner effects, and modular border styling for the 
// Westworld-EVE fusion interface.
//
// Features:
// - Dynamic angular border geometry
// - Corporation insignia placement
// - Border animation control
// - Military rank indicator support
// - Interactive border effects
// - Scalable border thickness
//
// Author: Gideon Development Team
// Created: June 21, 2025
// ==========================================================================

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Angular border types for different use cases
/// </summary>
public enum AngularBorderType
{
    Standard,
    Military,
    Corporation,
    ScanLine,
    Minimal,
    Heavy
}

/// <summary>
/// Corporation insignia positions
/// </summary>
public enum CorporationInsigniaPosition
{
    None,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    Both
}

/// <summary>
/// Military rank levels for rank indicators
/// </summary>
public enum MilitaryRank
{
    None,
    Ensign,
    Lieutenant,
    Commander,
    Captain,
    Admiral
}

/// <summary>
/// Advanced angular border panel with corporation insignia support
/// </summary>
public class AngularBorderPanel : ContentControl
{
    #region Dependency Properties

    /// <summary>
    /// Type of angular border to display
    /// </summary>
    public static readonly DependencyProperty BorderTypeProperty =
        DependencyProperty.Register(nameof(BorderType), typeof(AngularBorderType), typeof(AngularBorderPanel),
            new PropertyMetadata(AngularBorderType.Standard, OnBorderTypeChanged));

    /// <summary>
    /// Corporation insignia position
    /// </summary>
    public static readonly DependencyProperty InsigniaPositionProperty =
        DependencyProperty.Register(nameof(InsigniaPosition), typeof(CorporationInsigniaPosition), typeof(AngularBorderPanel),
            new PropertyMetadata(CorporationInsigniaPosition.None, OnInsigniaPositionChanged));

    /// <summary>
    /// Corporation logo image source
    /// </summary>
    public static readonly DependencyProperty CorporationLogoProperty =
        DependencyProperty.Register(nameof(CorporationLogo), typeof(ImageSource), typeof(AngularBorderPanel),
            new PropertyMetadata(null, OnCorporationLogoChanged));

    /// <summary>
    /// Alliance logo image source
    /// </summary>
    public static readonly DependencyProperty AllianceLogoProperty =
        DependencyProperty.Register(nameof(AllianceLogo), typeof(ImageSource), typeof(AngularBorderPanel),
            new PropertyMetadata(null, OnAllianceLogoChanged));

    /// <summary>
    /// Military rank for rank indicators
    /// </summary>
    public static readonly DependencyProperty MilitaryRankProperty =
        DependencyProperty.Register(nameof(MilitaryRank), typeof(MilitaryRank), typeof(AngularBorderPanel),
            new PropertyMetadata(MilitaryRank.None, OnMilitaryRankChanged));

    /// <summary>
    /// Border thickness multiplier
    /// </summary>
    public static readonly DependencyProperty BorderThicknessMultiplierProperty =
        DependencyProperty.Register(nameof(BorderThicknessMultiplier), typeof(double), typeof(AngularBorderPanel),
            new PropertyMetadata(1.0, OnBorderThicknessMultiplierChanged));

    /// <summary>
    /// Enable corner glow animations
    /// </summary>
    public static readonly DependencyProperty EnableCornerGlowProperty =
        DependencyProperty.Register(nameof(EnableCornerGlow), typeof(bool), typeof(AngularBorderPanel),
            new PropertyMetadata(true, OnEnableCornerGlowChanged));

    /// <summary>
    /// Enable scan line animation
    /// </summary>
    public static readonly DependencyProperty EnableScanLineProperty =
        DependencyProperty.Register(nameof(EnableScanLine), typeof(bool), typeof(AngularBorderPanel),
            new PropertyMetadata(false, OnEnableScanLineChanged));

    /// <summary>
    /// Primary border color
    /// </summary>
    public static readonly DependencyProperty BorderColorProperty =
        DependencyProperty.Register(nameof(BorderColor), typeof(Color), typeof(AngularBorderPanel),
            new PropertyMetadata(Colors.Cyan, OnBorderColorChanged));

    /// <summary>
    /// Secondary border color (for gradients)
    /// </summary>
    public static readonly DependencyProperty SecondaryBorderColorProperty =
        DependencyProperty.Register(nameof(SecondaryBorderColor), typeof(Color), typeof(AngularBorderPanel),
            new PropertyMetadata(Colors.Gold, OnSecondaryBorderColorChanged));

    /// <summary>
    /// Corporation name text
    /// </summary>
    public static readonly DependencyProperty CorporationNameProperty =
        DependencyProperty.Register(nameof(CorporationName), typeof(string), typeof(AngularBorderPanel),
            new PropertyMetadata(string.Empty, OnCorporationNameChanged));

    /// <summary>
    /// Alliance name text
    /// </summary>
    public static readonly DependencyProperty AllianceNameProperty =
        DependencyProperty.Register(nameof(AllianceName), typeof(string), typeof(AngularBorderPanel),
            new PropertyMetadata(string.Empty, OnAllianceNameChanged));

    #endregion

    #region Properties

    public AngularBorderType BorderType
    {
        get => (AngularBorderType)GetValue(BorderTypeProperty);
        set => SetValue(BorderTypeProperty, value);
    }

    public CorporationInsigniaPosition InsigniaPosition
    {
        get => (CorporationInsigniaPosition)GetValue(InsigniaPositionProperty);
        set => SetValue(InsigniaPositionProperty, value);
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

    public MilitaryRank MilitaryRank
    {
        get => (MilitaryRank)GetValue(MilitaryRankProperty);
        set => SetValue(MilitaryRankProperty, value);
    }

    public double BorderThicknessMultiplier
    {
        get => (double)GetValue(BorderThicknessMultiplierProperty);
        set => SetValue(BorderThicknessMultiplierProperty, value);
    }

    public bool EnableCornerGlow
    {
        get => (bool)GetValue(EnableCornerGlowProperty);
        set => SetValue(EnableCornerGlowProperty, value);
    }

    public bool EnableScanLine
    {
        get => (bool)GetValue(EnableScanLineProperty);
        set => SetValue(EnableScanLineProperty, value);
    }

    public Color BorderColor
    {
        get => (Color)GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }

    public Color SecondaryBorderColor
    {
        get => (Color)GetValue(SecondaryBorderColorProperty);
        set => SetValue(SecondaryBorderColorProperty, value);
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

    #endregion

    #region Fields

    private Canvas? _borderCanvas;
    private readonly Path[] _corners = new Path[4];
    private Border? _corporationInsignia;
    private Border? _allianceInsignia;
    private StackPanel? _rankIndicators;
    private Rectangle? _scanLineElement;
    private Storyboard? _cornerGlowStoryboard;
    private Storyboard? _scanLineStoryboard;
    private bool _isLoaded = false;

    #endregion

    #region Constructor

    static AngularBorderPanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AngularBorderPanel),
            new FrameworkPropertyMetadata(typeof(AngularBorderPanel)));
    }

    public AngularBorderPanel()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
        
        // Set up initial render transform
        RenderTransform = new ScaleTransform(1.0, 1.0);
        RenderTransformOrigin = new Point(0.5, 0.5);
    }

    #endregion

    #region Overrides

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        
        _borderCanvas = GetTemplateChild("PART_BorderCanvas") as Canvas;
        _corporationInsignia = GetTemplateChild("PART_CorporationInsignia") as Border;
        _allianceInsignia = GetTemplateChild("PART_AllianceInsignia") as Border;
        _rankIndicators = GetTemplateChild("PART_RankIndicators") as StackPanel;
        _scanLineElement = GetTemplateChild("PART_ScanLine") as Rectangle;
        
        if (_isLoaded)
        {
            UpdateBorderAppearance();
        }
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _isLoaded = true;
        UpdateBorderAppearance();
        
        if (EnableCornerGlow)
        {
            StartCornerGlowAnimation();
        }
        
        if (EnableScanLine)
        {
            StartScanLineAnimation();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _isLoaded = false;
        StopAllAnimations();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_isLoaded)
        {
            UpdateCornerPositions();
        }
    }

    #endregion

    #region Property Change Handlers

    private static void OnBorderTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AngularBorderPanel panel)
        {
            panel.UpdateBorderAppearance();
        }
    }

    private static void OnInsigniaPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AngularBorderPanel panel)
        {
            panel.UpdateInsigniaVisibility();
        }
    }

    private static void OnCorporationLogoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AngularBorderPanel panel)
        {
            panel.UpdateCorporationInsignia();
        }
    }

    private static void OnAllianceLogoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AngularBorderPanel panel)
        {
            panel.UpdateAllianceInsignia();
        }
    }

    private static void OnMilitaryRankChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AngularBorderPanel panel)
        {
            panel.UpdateRankIndicators();
        }
    }

    private static void OnBorderThicknessMultiplierChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AngularBorderPanel panel)
        {
            panel.UpdateBorderThickness();
        }
    }

    private static void OnEnableCornerGlowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AngularBorderPanel panel)
        {
            if ((bool)e.NewValue && panel._isLoaded)
            {
                panel.StartCornerGlowAnimation();
            }
            else
            {
                panel.StopCornerGlowAnimation();
            }
        }
    }

    private static void OnEnableScanLineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AngularBorderPanel panel)
        {
            if ((bool)e.NewValue && panel._isLoaded)
            {
                panel.StartScanLineAnimation();
            }
            else
            {
                panel.StopScanLineAnimation();
            }
        }
    }

    private static void OnBorderColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AngularBorderPanel panel)
        {
            panel.UpdateBorderColors();
        }
    }

    private static void OnSecondaryBorderColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AngularBorderPanel panel)
        {
            panel.UpdateBorderColors();
        }
    }

    private static void OnCorporationNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AngularBorderPanel panel)
        {
            panel.UpdateCorporationInsignia();
        }
    }

    private static void OnAllianceNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AngularBorderPanel panel)
        {
            panel.UpdateAllianceInsignia();
        }
    }

    #endregion

    #region Private Methods

    private void UpdateBorderAppearance()
    {
        if (_borderCanvas == null) return;

        // Clear existing corners
        _borderCanvas.Children.Clear();
        
        // Create new corners based on border type
        CreateCorners();
        UpdateCornerPositions();
        UpdateBorderThickness();
        UpdateBorderColors();
        UpdateInsigniaVisibility();
        UpdateRankIndicators();
    }

    private void CreateCorners()
    {
        if (_borderCanvas == null) return;

        var cornerSize = BorderType switch
        {
            AngularBorderType.Standard => 20,
            AngularBorderType.Military => 35,
            AngularBorderType.Corporation => 50,
            AngularBorderType.Heavy => 40,
            AngularBorderType.Minimal => 15,
            _ => 25
        };

        // Create corner paths
        for (int i = 0; i < 4; i++)
        {
            _corners[i] = new Path
            {
                Data = CreateCornerGeometry(i, cornerSize),
                StrokeThickness = GetBorderThickness(),
                Effect = CreateCornerEffect()
            };
            
            _borderCanvas.Children.Add(_corners[i]);
        }

        // Add corner enhancement elements for military/corporation borders
        if (BorderType == AngularBorderType.Military || BorderType == AngularBorderType.Corporation)
        {
            AddCornerEnhancements();
        }
    }

    private Geometry CreateCornerGeometry(int cornerIndex, double size)
    {
        var pathGeometry = new PathGeometry();
        var figure = new PathFigure();

        switch (cornerIndex)
        {
            case 0: // Top-Left
                figure.StartPoint = new Point(size, 0);
                figure.Segments.Add(new LineSegment(new Point(0, 0), true));
                figure.Segments.Add(new LineSegment(new Point(0, size), true));
                break;
            case 1: // Top-Right
                figure.StartPoint = new Point(-size, 0);
                figure.Segments.Add(new LineSegment(new Point(0, 0), true));
                figure.Segments.Add(new LineSegment(new Point(0, size), true));
                break;
            case 2: // Bottom-Left
                figure.StartPoint = new Point(0, -size);
                figure.Segments.Add(new LineSegment(new Point(0, 0), true));
                figure.Segments.Add(new LineSegment(new Point(size, 0), true));
                break;
            case 3: // Bottom-Right
                figure.StartPoint = new Point(0, -size);
                figure.Segments.Add(new LineSegment(new Point(0, 0), true));
                figure.Segments.Add(new LineSegment(new Point(-size, 0), true));
                break;
        }

        figure.IsClosed = false;
        pathGeometry.Figures.Add(figure);
        return pathGeometry;
    }

    private double GetBorderThickness()
    {
        var baseThickness = BorderType switch
        {
            AngularBorderType.Standard => 2.0,
            AngularBorderType.Military => 3.0,
            AngularBorderType.Corporation => 4.0,
            AngularBorderType.Heavy => 5.0,
            AngularBorderType.Minimal => 1.5,
            _ => 2.0
        };

        return baseThickness * BorderThicknessMultiplier;
    }

    private Effect CreateCornerEffect()
    {
        var intensity = BorderType switch
        {
            AngularBorderType.Military => 0.8,
            AngularBorderType.Corporation => 0.6,
            AngularBorderType.Heavy => 0.9,
            _ => 0.5
        };

        return new DropShadowEffect
        {
            Color = BorderColor,
            BlurRadius = 8,
            ShadowDepth = 0,
            Opacity = intensity
        };
    }

    private void UpdateCornerPositions()
    {
        if (_borderCanvas == null || _corners[0] == null) return;

        var margin = GetBorderMargin();

        // Position corners
        Canvas.SetLeft(_corners[0], margin);      // Top-Left
        Canvas.SetTop(_corners[0], margin);

        Canvas.SetRight(_corners[1], margin);     // Top-Right
        Canvas.SetTop(_corners[1], margin);

        Canvas.SetLeft(_corners[2], margin);      // Bottom-Left
        Canvas.SetBottom(_corners[2], margin);

        Canvas.SetRight(_corners[3], margin);     // Bottom-Right
        Canvas.SetBottom(_corners[3], margin);
    }

    private double GetBorderMargin()
    {
        return BorderType switch
        {
            AngularBorderType.Standard => 8,
            AngularBorderType.Military => 5,
            AngularBorderType.Corporation => 3,
            AngularBorderType.Heavy => 4,
            AngularBorderType.Minimal => 10,
            _ => 6
        };
    }

    private void UpdateBorderThickness()
    {
        var thickness = GetBorderThickness();
        
        foreach (var corner in _corners)
        {
            if (corner != null)
            {
                corner.StrokeThickness = thickness;
            }
        }
    }

    private void UpdateBorderColors()
    {
        var brush = CreateBorderBrush();
        
        foreach (var corner in _corners)
        {
            if (corner != null)
            {
                corner.Stroke = brush;
                if (corner.Effect is DropShadowEffect effect)
                {
                    effect.Color = BorderColor;
                }
            }
        }
    }

    private Brush CreateBorderBrush()
    {
        if (BorderType == AngularBorderType.Corporation)
        {
            return new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(SecondaryBorderColor, 0),
                    new GradientStop(BorderColor, 0.5),
                    new GradientStop(SecondaryBorderColor, 1)
                }
            };
        }

        return new SolidColorBrush(BorderColor);
    }

    private void AddCornerEnhancements()
    {
        if (_borderCanvas == null) return;

        var dotSize = 4;
        var dotDistance = BorderType == AngularBorderType.Corporation ? 30 : 25;

        // Add corner dots
        for (int i = 0; i < 4; i++)
        {
            var dot = new Ellipse
            {
                Width = dotSize,
                Height = dotSize,
                Fill = new SolidColorBrush(SecondaryBorderColor),
                Effect = new DropShadowEffect
                {
                    Color = SecondaryBorderColor,
                    BlurRadius = 6,
                    ShadowDepth = 0,
                    Opacity = 0.6
                }
            };

            switch (i)
            {
                case 0: // Top-Left
                    Canvas.SetLeft(dot, dotDistance);
                    Canvas.SetTop(dot, dotDistance);
                    break;
                case 1: // Top-Right
                    Canvas.SetRight(dot, dotDistance);
                    Canvas.SetTop(dot, dotDistance);
                    break;
                case 2: // Bottom-Left
                    Canvas.SetLeft(dot, dotDistance);
                    Canvas.SetBottom(dot, dotDistance);
                    break;
                case 3: // Bottom-Right
                    Canvas.SetRight(dot, dotDistance);
                    Canvas.SetBottom(dot, dotDistance);
                    break;
            }

            _borderCanvas.Children.Add(dot);
        }
    }

    private void UpdateInsigniaVisibility()
    {
        // This will be implemented when the template is applied
        // For now, just update corporation and alliance insignia
        UpdateCorporationInsignia();
        UpdateAllianceInsignia();
    }

    private void UpdateCorporationInsignia()
    {
        if (_corporationInsignia == null) return;

        _corporationInsignia.Visibility = (InsigniaPosition != CorporationInsigniaPosition.None && 
                                          (CorporationLogo != null || !string.IsNullOrEmpty(CorporationName))) 
                                          ? Visibility.Visible : Visibility.Collapsed;

        // Update corporation logo or text
        if (CorporationLogo != null)
        {
            var image = new Image
            {
                Source = CorporationLogo,
                Stretch = Stretch.Uniform
            };
            _corporationInsignia.Child = image;
        }
        else if (!string.IsNullOrEmpty(CorporationName))
        {
            var textBlock = new TextBlock
            {
                Text = CorporationName.Length > 4 ? CorporationName.Substring(0, 4) : CorporationName,
                FontSize = 8,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(SecondaryBorderColor),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            _corporationInsignia.Child = textBlock;
        }
    }

    private void UpdateAllianceInsignia()
    {
        if (_allianceInsignia == null) return;

        _allianceInsignia.Visibility = (InsigniaPosition == CorporationInsigniaPosition.Both && 
                                       (AllianceLogo != null || !string.IsNullOrEmpty(AllianceName))) 
                                       ? Visibility.Visible : Visibility.Collapsed;

        // Update alliance logo or text
        if (AllianceLogo != null)
        {
            var image = new Image
            {
                Source = AllianceLogo,
                Stretch = Stretch.Uniform
            };
            _allianceInsignia.Child = image;
        }
        else if (!string.IsNullOrEmpty(AllianceName))
        {
            var textBlock = new TextBlock
            {
                Text = AllianceName.Length > 4 ? AllianceName.Substring(0, 4) : AllianceName,
                FontSize = 8,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(BorderColor),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            _allianceInsignia.Child = textBlock;
        }
    }

    private void UpdateRankIndicators()
    {
        if (_rankIndicators == null || MilitaryRank == MilitaryRank.None) return;

        _rankIndicators.Children.Clear();
        _rankIndicators.Visibility = Visibility.Visible;

        var rankCount = (int)MilitaryRank;
        var maxHeight = 15;

        for (int i = 0; i < rankCount; i++)
        {
            var height = maxHeight - (i * 2);
            var indicator = new Rectangle
            {
                Width = 2,
                Height = height,
                Fill = new SolidColorBrush(SecondaryBorderColor),
                Margin = new Thickness(1, 0, 1, 0)
            };
            
            _rankIndicators.Children.Add(indicator);
        }
    }

    private void StartCornerGlowAnimation()
    {
        if (!EnableCornerGlow || _corners[0] == null) return;

        _cornerGlowStoryboard = new Storyboard
        {
            RepeatBehavior = RepeatBehavior.Forever,
            AutoReverse = true
        };

        foreach (var corner in _corners)
        {
            if (corner?.Effect is DropShadowEffect effect)
            {
                var animation = new DoubleAnimation
                {
                    From = 0.4,
                    To = 1.0,
                    Duration = TimeSpan.FromSeconds(2),
                    EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                };

                Storyboard.SetTarget(animation, effect);
                Storyboard.SetTargetProperty(animation, new PropertyPath(DropShadowEffect.OpacityProperty));
                _cornerGlowStoryboard.Children.Add(animation);
            }
        }

        _cornerGlowStoryboard.Begin();
    }

    private void StopCornerGlowAnimation()
    {
        _cornerGlowStoryboard?.Stop();
        _cornerGlowStoryboard = null;
    }

    private void StartScanLineAnimation()
    {
        if (!EnableScanLine || _scanLineElement == null) return;

        _scanLineStoryboard = new Storyboard
        {
            RepeatBehavior = RepeatBehavior.Forever
        };

        var transform = new TranslateTransform();
        _scanLineElement.RenderTransform = transform;

        var animation = new DoubleAnimation
        {
            From = -ActualWidth,
            To = ActualWidth,
            Duration = TimeSpan.FromSeconds(3),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        Storyboard.SetTarget(animation, transform);
        Storyboard.SetTargetProperty(animation, new PropertyPath(TranslateTransform.XProperty));
        _scanLineStoryboard.Children.Add(animation);

        _scanLineStoryboard.Begin();
    }

    private void StopScanLineAnimation()
    {
        _scanLineStoryboard?.Stop();
        _scanLineStoryboard = null;
    }

    private void StopAllAnimations()
    {
        StopCornerGlowAnimation();
        StopScanLineAnimation();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Triggers a border activation effect
    /// </summary>
    public void TriggerActivationEffect()
    {
        var storyboard = new Storyboard();

        var scaleAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 1.05,
            Duration = TimeSpan.FromSeconds(0.15),
            AutoReverse = true,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        if (RenderTransform is ScaleTransform transform)
        {
            Storyboard.SetTarget(scaleAnimation, transform);
            Storyboard.SetTargetProperty(scaleAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));
            storyboard.Children.Add(scaleAnimation);

            var scaleYAnimation = scaleAnimation.Clone();
            Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));
            storyboard.Children.Add(scaleYAnimation);
        }

        storyboard.Begin();
    }

    /// <summary>
    /// Sets corporation information
    /// </summary>
    public void SetCorporationInfo(string corporationName, ImageSource? logo = null)
    {
        CorporationName = corporationName;
        if (logo != null)
        {
            CorporationLogo = logo;
        }
    }

    /// <summary>
    /// Sets alliance information
    /// </summary>
    public void SetAllianceInfo(string allianceName, ImageSource? logo = null)
    {
        AllianceName = allianceName;
        if (logo != null)
        {
            AllianceLogo = logo;
        }
    }

    #endregion
}