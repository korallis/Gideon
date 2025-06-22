// ==========================================================================
// GlassmorphismCard.cs - Glassmorphism Card Layout System
// ==========================================================================
// Advanced glassmorphism card layouts with transparency, blur effects,
// and holographic styling for the EVE Online interface.
//
// Author: Gideon Development Team
// Created: June 21, 2025
// ==========================================================================

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Glassmorphism visual styles
/// </summary>
public enum GlassStyle
{
    Standard,
    Frosted,
    Crystal,
    Ethereal,
    Combat,
    Market,
    Character
}

/// <summary>
/// Card interaction states
/// </summary>
public enum CardState
{
    Normal,
    Hover,
    Active,
    Selected,
    Disabled
}

/// <summary>
/// Glassmorphism card control with advanced visual effects
/// </summary>
public class GlassmorphismCard : ContentControl
{
    #region Dependency Properties

    public static readonly DependencyProperty GlassStyleProperty =
        DependencyProperty.Register(nameof(GlassStyle), typeof(GlassStyle), typeof(GlassmorphismCard),
            new PropertyMetadata(GlassStyle.Standard, OnGlassStyleChanged));

    public static readonly DependencyProperty CardStateProperty =
        DependencyProperty.Register(nameof(CardState), typeof(CardState), typeof(GlassmorphismCard),
            new PropertyMetadata(CardState.Normal, OnCardStateChanged));

    public static readonly DependencyProperty BlurRadiusProperty =
        DependencyProperty.Register(nameof(BlurRadius), typeof(double), typeof(GlassmorphismCard),
            new PropertyMetadata(10.0, OnBlurRadiusChanged));

    public static readonly DependencyProperty GlassOpacityProperty =
        DependencyProperty.Register(nameof(GlassOpacity), typeof(double), typeof(GlassmorphismCard),
            new PropertyMetadata(0.3, OnGlassOpacityChanged));

    public static readonly DependencyProperty BorderGlowProperty =
        DependencyProperty.Register(nameof(BorderGlow), typeof(bool), typeof(GlassmorphismCard),
            new PropertyMetadata(true, OnBorderGlowChanged));

    public static readonly DependencyProperty HeaderTextProperty =
        DependencyProperty.Register(nameof(HeaderText), typeof(string), typeof(GlassmorphismCard),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty HeaderIconProperty =
        DependencyProperty.Register(nameof(HeaderIcon), typeof(Geometry), typeof(GlassmorphismCard),
            new PropertyMetadata(null));

    #endregion

    #region Properties

    public GlassStyle GlassStyle
    {
        get => (GlassStyle)GetValue(GlassStyleProperty);
        set => SetValue(GlassStyleProperty, value);
    }

    public CardState CardState
    {
        get => (CardState)GetValue(CardStateProperty);
        set => SetValue(CardStateProperty, value);
    }

    public double BlurRadius
    {
        get => (double)GetValue(BlurRadiusProperty);
        set => SetValue(BlurRadiusProperty, value);
    }

    public double GlassOpacity
    {
        get => (double)GetValue(GlassOpacityProperty);
        set => SetValue(GlassOpacityProperty, value);
    }

    public bool BorderGlow
    {
        get => (bool)GetValue(BorderGlowProperty);
        set => SetValue(BorderGlowProperty, value);
    }

    public string HeaderText
    {
        get => (string)GetValue(HeaderTextProperty);
        set => SetValue(HeaderTextProperty, value);
    }

    public Geometry HeaderIcon
    {
        get => (Geometry)GetValue(HeaderIconProperty);
        set => SetValue(HeaderIconProperty, value);
    }

    #endregion

    #region Fields

    private Border? _glassBorder;
    private Border? _contentBorder;
    private Grid? _headerGrid;

    #endregion

    #region Constructor

    static GlassmorphismCard()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(GlassmorphismCard),
            new FrameworkPropertyMetadata(typeof(GlassmorphismCard)));
    }

    public GlassmorphismCard()
    {
        Loaded += OnLoaded;
        MouseEnter += OnMouseEnter;
        MouseLeave += OnMouseLeave;
    }

    #endregion

    #region Public Methods

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _glassBorder = GetTemplateChild("PART_GlassBorder") as Border;
        _contentBorder = GetTemplateChild("PART_ContentBorder") as Border;
        _headerGrid = GetTemplateChild("PART_HeaderGrid") as Grid;

        UpdateGlassStyle();
        UpdateCardState();
    }

    #endregion

    #region Private Methods

    private void UpdateGlassStyle()
    {
        if (_glassBorder == null) return;

        var (background, borderBrush, glowColor) = GetStyleColors();

        _glassBorder.Background = background;
        _glassBorder.BorderBrush = borderBrush;

        if (BorderGlow)
        {
            _glassBorder.Effect = new DropShadowEffect
            {
                Color = glowColor,
                Direction = 0,
                ShadowDepth = 0,
                BlurRadius = BlurRadius,
                Opacity = 0.6
            };
        }
    }

    private (Brush background, Brush borderBrush, Color glowColor) GetStyleColors()
    {
        return GlassStyle switch
        {
            GlassStyle.Standard => (
                new SolidColorBrush(Color.FromArgb((byte)(GlassOpacity * 255), 0, 255, 255)),
                new SolidColorBrush(Color.FromArgb(100, 0, 255, 255)),
                Color.FromRgb(0, 255, 255)
            ),
            GlassStyle.Frosted => (
                new SolidColorBrush(Color.FromArgb((byte)(GlassOpacity * 255), 200, 200, 255)),
                new SolidColorBrush(Color.FromArgb(120, 200, 200, 255)),
                Color.FromRgb(200, 200, 255)
            ),
            GlassStyle.Crystal => (
                new SolidColorBrush(Color.FromArgb((byte)(GlassOpacity * 255), 255, 255, 255)),
                new SolidColorBrush(Color.FromArgb(150, 255, 255, 255)),
                Color.FromRgb(255, 255, 255)
            ),
            GlassStyle.Ethereal => (
                new SolidColorBrush(Color.FromArgb((byte)(GlassOpacity * 255), 128, 0, 255)),
                new SolidColorBrush(Color.FromArgb(100, 128, 0, 255)),
                Color.FromRgb(128, 0, 255)
            ),
            GlassStyle.Combat => (
                new SolidColorBrush(Color.FromArgb((byte)(GlassOpacity * 255), 255, 0, 0)),
                new SolidColorBrush(Color.FromArgb(120, 255, 0, 0)),
                Color.FromRgb(255, 0, 0)
            ),
            GlassStyle.Market => (
                new SolidColorBrush(Color.FromArgb((byte)(GlassOpacity * 255), 0, 255, 0)),
                new SolidColorBrush(Color.FromArgb(100, 0, 255, 0)),
                Color.FromRgb(0, 255, 0)
            ),
            GlassStyle.Character => (
                new SolidColorBrush(Color.FromArgb((byte)(GlassOpacity * 255), 255, 255, 0)),
                new SolidColorBrush(Color.FromArgb(110, 255, 255, 0)),
                Color.FromRgb(255, 255, 0)
            ),
            _ => (
                new SolidColorBrush(Color.FromArgb((byte)(GlassOpacity * 255), 0, 255, 255)),
                new SolidColorBrush(Color.FromArgb(100, 0, 255, 255)),
                Color.FromRgb(0, 255, 255)
            )
        };
    }

    private void UpdateCardState()
    {
        if (_glassBorder == null) return;

        var stateMultiplier = CardState switch
        {
            CardState.Normal => 1.0,
            CardState.Hover => 1.3,
            CardState.Active => 1.5,
            CardState.Selected => 1.4,
            CardState.Disabled => 0.5,
            _ => 1.0
        };

        var currentOpacity = GlassOpacity * stateMultiplier;
        currentOpacity = Math.Min(currentOpacity, 0.8); // Cap at 80%

        UpdateGlassStyle();

        // Update transform for hover/active states
        var scaleTransform = CardState switch
        {
            CardState.Hover => new ScaleTransform(1.02, 1.02),
            CardState.Active => new ScaleTransform(0.98, 0.98),
            CardState.Selected => new ScaleTransform(1.01, 1.01),
            _ => new ScaleTransform(1.0, 1.0)
        };

        RenderTransform = scaleTransform;
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateGlassStyle();
        UpdateCardState();
    }

    private void OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (CardState == CardState.Normal)
        {
            CardState = CardState.Hover;
        }
    }

    private void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (CardState == CardState.Hover)
        {
            CardState = CardState.Normal;
        }
    }

    private static void OnGlassStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GlassmorphismCard card)
        {
            card.UpdateGlassStyle();
        }
    }

    private static void OnCardStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GlassmorphismCard card)
        {
            card.UpdateCardState();
        }
    }

    private static void OnBlurRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GlassmorphismCard card)
        {
            card.UpdateGlassStyle();
        }
    }

    private static void OnGlassOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GlassmorphismCard card)
        {
            card.UpdateGlassStyle();
        }
    }

    private static void OnBorderGlowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GlassmorphismCard card)
        {
            card.UpdateGlassStyle();
        }
    }

    #endregion
}