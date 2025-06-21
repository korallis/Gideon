using System.Windows;
using System.Windows.Controls;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// A card control that automatically adapts to Windows 11 theme changes
/// </summary>
public partial class ThemeAwareCard : System.Windows.Controls.UserControl
{
    public static readonly DependencyProperty CardContentProperty =
        DependencyProperty.Register(nameof(CardContent), typeof(object), typeof(ThemeAwareCard), 
            new PropertyMetadata(null));

    public static readonly DependencyProperty ElevationProperty =
        DependencyProperty.Register(nameof(Elevation), typeof(int), typeof(ThemeAwareCard), 
            new PropertyMetadata(2, OnElevationChanged));

    public static readonly DependencyProperty IsInteractiveProperty =
        DependencyProperty.Register(nameof(IsInteractive), typeof(bool), typeof(ThemeAwareCard), 
            new PropertyMetadata(false));

    public ThemeAwareCard()
    {
        InitializeComponent();
        this.Loaded += OnLoaded;
    }

    /// <summary>
    /// The content to display inside the card
    /// </summary>
    public object CardContent
    {
        get => GetValue(CardContentProperty);
        set => SetValue(CardContentProperty, value);
    }

    /// <summary>
    /// The elevation level for the card shadow (0-24)
    /// </summary>
    public int Elevation
    {
        get => (int)GetValue(ElevationProperty);
        set => SetValue(ElevationProperty, value);
    }

    /// <summary>
    /// Whether the card responds to mouse interactions
    /// </summary>
    public bool IsInteractive
    {
        get => (bool)GetValue(IsInteractiveProperty);
        set => SetValue(IsInteractiveProperty, value);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateCardAppearance();
    }

    private static void OnElevationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ThemeAwareCard card)
        {
            card.UpdateCardAppearance();
        }
    }

    private void UpdateCardAppearance()
    {
        // Update shadow based on elevation
        if (Effect is System.Windows.Media.Effects.DropShadowEffect shadow)
        {
            shadow.BlurRadius = Math.Max(0, Elevation * 2);
            shadow.ShadowDepth = Math.Max(0, Elevation / 2);
            shadow.Opacity = Math.Min(0.5, Elevation * 0.05);
        }

        // Update interaction states
        if (IsInteractive)
        {
            Cursor = System.Windows.Input.Cursors.Hand;
        }
        else
        {
            Cursor = System.Windows.Input.Cursors.Arrow;
        }
    }
}