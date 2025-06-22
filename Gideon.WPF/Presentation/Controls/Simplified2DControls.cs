// ==========================================================================
// Simplified2DControls.cs - Simplified 2D Versions of Holographic Controls
// ==========================================================================
// Lightweight 2D versions of holographic controls that maintain visual 
// consistency while reducing performance requirements for low-end systems.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Simplified 2D panel that mimics holographic appearance
/// </summary>
public class Simple2DPanel : Border
{
    public static readonly DependencyProperty IntensityProperty =
        DependencyProperty.Register(nameof(Intensity), typeof(double), typeof(Simple2DPanel),
            new PropertyMetadata(1.0, OnIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(Simple2DPanel),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnColorSchemeChanged));

    public double Intensity
    {
        get => (double)GetValue(IntensityProperty);
        set => SetValue(IntensityProperty, value);
    }

    public EVEColorScheme EVEColorScheme
    {
        get => (EVEColorScheme)GetValue(EVEColorSchemeProperty);
        set => SetValue(EVEColorSchemeProperty, value);
    }

    public Simple2DPanel()
    {
        InitializePanel();
    }

    private void InitializePanel()
    {
        // Simple glassmorphism effect without blur
        Background = new SolidColorBrush(Color.FromArgb(40, 0, 50, 100));
        BorderBrush = new SolidColorBrush(Color.FromArgb(100, 64, 224, 255));
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(4);

        UpdateVisualEffects();
    }

    private void UpdateVisualEffects()
    {
        var color = GetEVEColor(EVEColorScheme);
        var intensity = (byte)(Intensity * 100);

        // Simple drop shadow instead of complex glow
        Effect = new DropShadowEffect
        {
            Color = color,
            BlurRadius = 3,
            ShadowDepth = 0,
            Opacity = 0.4 * Intensity
        };

        // Update border color
        BorderBrush = new SolidColorBrush(Color.FromArgb(intensity, color.R, color.G, color.B));
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

    private static void OnIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Simple2DPanel panel)
            panel.UpdateVisualEffects();
    }

    private static void OnColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Simple2DPanel panel)
            panel.UpdateVisualEffects();
    }
}

/// <summary>
/// Simplified 2D button with EVE styling
/// </summary>
public class Simple2DButton : Button
{
    public static readonly DependencyProperty GlowIntensityProperty =
        DependencyProperty.Register(nameof(GlowIntensity), typeof(double), typeof(Simple2DButton),
            new PropertyMetadata(0.5, OnGlowIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(Simple2DButton),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnColorSchemeChanged));

    public double GlowIntensity
    {
        get => (double)GetValue(GlowIntensityProperty);
        set => SetValue(GlowIntensityProperty, value);
    }

    public EVEColorScheme EVEColorScheme
    {
        get => (EVEColorScheme)GetValue(EVEColorSchemeProperty);
        set => SetValue(EVEColorSchemeProperty, value);
    }

    public Simple2DButton()
    {
        InitializeButton();
    }

    private void InitializeButton()
    {
        // Remove default button style
        Template = CreateSimpleButtonTemplate();
        
        // Mouse interaction effects
        MouseEnter += OnMouseEnter;
        MouseLeave += OnMouseLeave;
        
        UpdateButtonAppearance();
    }

    private ControlTemplate CreateSimpleButtonTemplate()
    {
        var template = new ControlTemplate(typeof(Button));
        
        var border = new FrameworkElementFactory(typeof(Border));
        border.Name = "border";
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
        border.SetValue(Border.BorderThicknessProperty, new Thickness(1));
        border.SetValue(Border.PaddingProperty, new Thickness(8, 4, 8, 4));

        var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
        contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

        border.AppendChild(contentPresenter);
        template.VisualTree = border;

        return template;
    }

    private void UpdateButtonAppearance()
    {
        var color = GetEVEColor(EVEColorScheme);
        
        Background = new SolidColorBrush(Color.FromArgb(60, color.R, color.G, color.B));
        BorderBrush = new SolidColorBrush(Color.FromArgb(150, color.R, color.G, color.B));
        Foreground = new SolidColorBrush(color);

        // Simple glow effect
        Effect = new DropShadowEffect
        {
            Color = color,
            BlurRadius = 4,
            ShadowDepth = 0,
            Opacity = GlowIntensity * 0.6
        };
    }

    private void OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        // Simple hover animation
        var scaleAnimation = new DoubleAnimation
        {
            To = 1.05,
            Duration = TimeSpan.FromMilliseconds(100)
        };
        
        var transform = new ScaleTransform(1, 1);
        RenderTransform = transform;
        RenderTransformOrigin = new Point(0.5, 0.5);
        
        transform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
        transform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);

        // Increase glow
        if (Effect is DropShadowEffect shadow)
        {
            var glowAnimation = new DoubleAnimation
            {
                To = GlowIntensity * 1.5,
                Duration = TimeSpan.FromMilliseconds(100)
            };
            shadow.BeginAnimation(DropShadowEffect.OpacityProperty, glowAnimation);
        }
    }

    private void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        // Return to normal
        var scaleAnimation = new DoubleAnimation
        {
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(100)
        };
        
        if (RenderTransform is ScaleTransform transform)
        {
            transform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }

        // Return glow to normal
        if (Effect is DropShadowEffect shadow)
        {
            var glowAnimation = new DoubleAnimation
            {
                To = GlowIntensity * 0.6,
                Duration = TimeSpan.FromMilliseconds(100)
            };
            shadow.BeginAnimation(DropShadowEffect.OpacityProperty, glowAnimation);
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

    private static void OnGlowIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Simple2DButton button)
            button.UpdateButtonAppearance();
    }

    private static void OnColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Simple2DButton button)
            button.UpdateButtonAppearance();
    }
}

/// <summary>
/// Simplified 2D progress bar with minimal effects
/// </summary>
public class Simple2DProgressBar : ProgressBar
{
    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(Simple2DProgressBar),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnColorSchemeChanged));

    public static readonly DependencyProperty AnimateProperty =
        DependencyProperty.Register(nameof(Animate), typeof(bool), typeof(Simple2DProgressBar),
            new PropertyMetadata(true));

    public EVEColorScheme EVEColorScheme
    {
        get => (EVEColorScheme)GetValue(EVEColorSchemeProperty);
        set => SetValue(EVEColorSchemeProperty, value);
    }

    public bool Animate
    {
        get => (bool)GetValue(AnimateProperty);
        set => SetValue(AnimateProperty, value);
    }

    public Simple2DProgressBar()
    {
        InitializeProgressBar();
    }

    private void InitializeProgressBar()
    {
        Height = 8;
        Template = CreateSimpleProgressBarTemplate();
        UpdateProgressBarAppearance();
    }

    private ControlTemplate CreateSimpleProgressBarTemplate()
    {
        var template = new ControlTemplate(typeof(ProgressBar));

        // Background track
        var track = new FrameworkElementFactory(typeof(Border));
        track.Name = "PART_Track";
        track.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromArgb(40, 128, 128, 128)));
        track.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));

        // Progress indicator
        var indicator = new FrameworkElementFactory(typeof(Rectangle));
        indicator.Name = "PART_Indicator";
        indicator.SetValue(Rectangle.RadiusXProperty, 4.0);
        indicator.SetValue(Rectangle.RadiusYProperty, 4.0);
        indicator.SetValue(Rectangle.HorizontalAlignmentProperty, HorizontalAlignment.Left);

        track.AppendChild(indicator);
        template.VisualTree = track;

        return template;
    }

    private void UpdateProgressBarAppearance()
    {
        var color = GetEVEColor(EVEColorScheme);
        
        // Simple gradient for progress fill
        var gradient = new LinearGradientBrush();
        gradient.GradientStops.Add(new GradientStop(Color.FromArgb(180, color.R, color.G, color.B), 0.0));
        gradient.GradientStops.Add(new GradientStop(Color.FromArgb(255, color.R, color.G, color.B), 0.5));
        gradient.GradientStops.Add(new GradientStop(Color.FromArgb(180, color.R, color.G, color.B), 1.0));
        
        Foreground = gradient;

        // Simple glow effect
        Effect = new DropShadowEffect
        {
            Color = color,
            BlurRadius = 2,
            ShadowDepth = 0,
            Opacity = 0.5
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

    private static void OnColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Simple2DProgressBar progressBar)
            progressBar.UpdateProgressBarAppearance();
    }
}

/// <summary>
/// Simplified 2D data visualization card
/// </summary>
public class Simple2DDataCard : ContentControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(Simple2DDataCard),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(Simple2DDataCard),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnColorSchemeChanged));

    public static readonly DependencyProperty ShowGlowProperty =
        DependencyProperty.Register(nameof(ShowGlow), typeof(bool), typeof(Simple2DDataCard),
            new PropertyMetadata(true, OnShowGlowChanged));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public EVEColorScheme EVEColorScheme
    {
        get => (EVEColorScheme)GetValue(EVEColorSchemeProperty);
        set => SetValue(EVEColorSchemeProperty, value);
    }

    public bool ShowGlow
    {
        get => (bool)GetValue(ShowGlowProperty);
        set => SetValue(ShowGlowProperty, value);
    }

    public Simple2DDataCard()
    {
        InitializeCard();
    }

    private void InitializeCard()
    {
        Template = CreateCardTemplate();
        UpdateCardAppearance();
    }

    private ControlTemplate CreateCardTemplate()
    {
        var template = new ControlTemplate(typeof(ContentControl));

        var mainBorder = new FrameworkElementFactory(typeof(Border));
        mainBorder.Name = "MainBorder";
        mainBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
        mainBorder.SetValue(Border.BorderThicknessProperty, new Thickness(1));
        mainBorder.SetValue(Border.PaddingProperty, new Thickness(12));

        var grid = new FrameworkElementFactory(typeof(Grid));
        grid.SetValue(Grid.RowDefinitionsProperty, new RowDefinitionCollection
        {
            new RowDefinition { Height = GridLength.Auto },
            new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
        });

        // Title
        var titleBlock = new FrameworkElementFactory(typeof(TextBlock));
        titleBlock.Name = "TitleBlock";
        titleBlock.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        titleBlock.SetValue(TextBlock.FontSizeProperty, 14.0);
        titleBlock.SetValue(TextBlock.MarginProperty, new Thickness(0, 0, 0, 8));
        titleBlock.SetBinding(TextBlock.TextProperty, new TemplateBindingExtension(TitleProperty));
        titleBlock.SetValue(Grid.RowProperty, 0);

        // Content
        var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
        contentPresenter.SetValue(Grid.RowProperty, 1);

        grid.AppendChild(titleBlock);
        grid.AppendChild(contentPresenter);
        mainBorder.AppendChild(grid);
        template.VisualTree = mainBorder;

        return template;
    }

    private void UpdateCardAppearance()
    {
        var color = GetEVEColor(EVEColorScheme);
        
        Background = new SolidColorBrush(Color.FromArgb(50, 0, 20, 40));
        BorderBrush = new SolidColorBrush(Color.FromArgb(120, color.R, color.G, color.B));
        Foreground = new SolidColorBrush(color);

        if (ShowGlow)
        {
            Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 5,
                ShadowDepth = 0,
                Opacity = 0.4
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

    private static void OnColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Simple2DDataCard card)
            card.UpdateCardAppearance();
    }

    private static void OnShowGlowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Simple2DDataCard card)
            card.UpdateCardAppearance();
    }
}

/// <summary>
/// Simplified 2D status indicator
/// </summary>
public class Simple2DStatusIndicator : UserControl
{
    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(nameof(Status), typeof(StatusType), typeof(Simple2DStatusIndicator),
            new PropertyMetadata(StatusType.Normal, OnStatusChanged));

    public static readonly DependencyProperty StatusTextProperty =
        DependencyProperty.Register(nameof(StatusText), typeof(string), typeof(Simple2DStatusIndicator),
            new PropertyMetadata("STATUS"));

    public StatusType Status
    {
        get => (StatusType)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public string StatusText
    {
        get => (string)GetValue(StatusTextProperty);
        set => SetValue(StatusTextProperty, value);
    }

    private Ellipse _indicator;
    private TextBlock _text;

    public Simple2DStatusIndicator()
    {
        InitializeIndicator();
    }

    private void InitializeIndicator()
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };

        _indicator = new Ellipse
        {
            Width = 8,
            Height = 8,
            Margin = new Thickness(0, 0, 6, 0)
        };

        _text = new TextBlock
        {
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center
        };

        _text.SetBinding(TextBlock.TextProperty, new Binding(nameof(StatusText)) { Source = this });

        stack.Children.Add(_indicator);
        stack.Children.Add(_text);
        Content = stack;

        UpdateStatusAppearance();
    }

    private void UpdateStatusAppearance()
    {
        var (color, shouldPulse) = Status switch
        {
            StatusType.Good => (Color.FromRgb(50, 205, 50), false),
            StatusType.Warning => (Color.FromRgb(255, 215, 0), true),
            StatusType.Error => (Color.FromRgb(220, 20, 60), true),
            StatusType.Normal => (Color.FromRgb(64, 224, 255), false),
            _ => (Color.FromRgb(128, 128, 128), false)
        };

        _indicator.Fill = new SolidColorBrush(color);
        _text.Foreground = new SolidColorBrush(color);

        // Simple glow effect
        _indicator.Effect = new DropShadowEffect
        {
            Color = color,
            BlurRadius = 3,
            ShadowDepth = 0,
            Opacity = 0.6
        };

        // Simple pulsing animation for warning/error states
        if (shouldPulse)
        {
            var pulseAnimation = new DoubleAnimation
            {
                From = 0.4,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(800),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            _indicator.BeginAnimation(UIElement.OpacityProperty, pulseAnimation);
        }
        else
        {
            _indicator.BeginAnimation(UIElement.OpacityProperty, null);
            _indicator.Opacity = 1.0;
        }
    }

    private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Simple2DStatusIndicator indicator)
            indicator.UpdateStatusAppearance();
    }
}

/// <summary>
/// Status types for the simplified indicator
/// </summary>
public enum StatusType
{
    Normal,
    Good,
    Warning,
    Error
}