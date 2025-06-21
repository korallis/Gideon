using System.Windows;
using System.Windows.Controls;
using WpfButton = System.Windows.Controls.Button;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// A button control that automatically adapts to Windows 11 theme changes
/// </summary>
public partial class ThemeAwareButton : WpfButton
{
    public static readonly DependencyProperty ButtonVariantProperty =
        DependencyProperty.Register(nameof(ButtonVariant), typeof(ButtonVariant), typeof(ThemeAwareButton), 
            new PropertyMetadata(ButtonVariant.Primary, OnButtonVariantChanged));

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(ThemeAwareButton), 
            new PropertyMetadata(null));

    public static readonly DependencyProperty IconPositionProperty =
        DependencyProperty.Register(nameof(IconPosition), typeof(IconPosition), typeof(ThemeAwareButton), 
            new PropertyMetadata(IconPosition.Left));

    public ThemeAwareButton()
    {
        InitializeComponent();
        UpdateButtonStyle();
    }

    /// <summary>
    /// The visual variant of the button
    /// </summary>
    public ButtonVariant ButtonVariant
    {
        get => (ButtonVariant)GetValue(ButtonVariantProperty);
        set => SetValue(ButtonVariantProperty, value);
    }

    /// <summary>
    /// The icon to display in the button
    /// </summary>
    public object Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// The position of the icon relative to the text
    /// </summary>
    public IconPosition IconPosition
    {
        get => (IconPosition)GetValue(IconPositionProperty);
        set => SetValue(IconPositionProperty, value);
    }

    private static void OnButtonVariantChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ThemeAwareButton button)
        {
            button.UpdateButtonStyle();
        }
    }

    private void UpdateButtonStyle()
    {
        var styleName = ButtonVariant switch
        {
            ButtonVariant.Primary => "PrimaryButtonStyle",
            ButtonVariant.Secondary => "SecondaryButtonStyle",
            ButtonVariant.Text => "TextButtonStyle",
            _ => "PrimaryButtonStyle"
        };

        if (Resources[styleName] is Style style)
        {
            Style = style;
        }

        UpdateContentLayout();
    }

    private void UpdateContentLayout()
    {
        if (Icon == null)
        {
            // No icon, just show content
            return;
        }

        // Create content with icon
        var panel = new StackPanel
        {
            Orientation = IconPosition == IconPosition.Top || IconPosition == IconPosition.Bottom 
                ? Orientation.Vertical 
                : Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var iconElement = new ContentPresenter
        {
            Content = Icon,
            Width = 16,
            Height = 16,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var textElement = new ContentPresenter
        {
            Content = Content,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        // Add elements in correct order based on position
        switch (IconPosition)
        {
            case IconPosition.Left:
            case IconPosition.Top:
                panel.Children.Add(iconElement);
                if (Content != null)
                {
                    if (IconPosition == IconPosition.Left)
                        textElement.Margin = new Thickness(8, 0, 0, 0);
                    else
                        textElement.Margin = new Thickness(0, 4, 0, 0);
                    panel.Children.Add(textElement);
                }
                break;

            case IconPosition.Right:
            case IconPosition.Bottom:
                if (Content != null)
                {
                    panel.Children.Add(textElement);
                    if (IconPosition == IconPosition.Right)
                        iconElement.Margin = new Thickness(8, 0, 0, 0);
                    else
                        iconElement.Margin = new Thickness(0, 4, 0, 0);
                }
                panel.Children.Add(iconElement);
                break;
        }

        Content = panel;
    }
}

/// <summary>
/// Button visual variants
/// </summary>
public enum ButtonVariant
{
    Primary,
    Secondary,
    Text
}

/// <summary>
/// Icon positions relative to button text
/// </summary>
public enum IconPosition
{
    Left,
    Right,
    Top,
    Bottom
}