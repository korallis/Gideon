// ==========================================================================
// HoloBreadcrumb.cs - Holographic Breadcrumb Navigation
// ==========================================================================
// Advanced holographic breadcrumb navigation system that provides visual
// navigation trail with EVE-style effects and adaptive rendering.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Advanced holographic breadcrumb navigation with EVE styling
/// </summary>
public class HoloBreadcrumb : Control, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloBreadcrumb),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloBreadcrumb),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty BreadcrumbItemsProperty =
        DependencyProperty.Register(nameof(BreadcrumbItems), typeof(ObservableCollection<HoloBreadcrumbItem>), typeof(HoloBreadcrumb),
            new PropertyMetadata(null, OnBreadcrumbItemsChanged));

    public static readonly DependencyProperty MaxVisibleItemsProperty =
        DependencyProperty.Register(nameof(MaxVisibleItems), typeof(int), typeof(HoloBreadcrumb),
            new PropertyMetadata(6, OnMaxVisibleItemsChanged));

    public static readonly DependencyProperty EnableNavigationProperty =
        DependencyProperty.Register(nameof(EnableNavigation), typeof(bool), typeof(HoloBreadcrumb),
            new PropertyMetadata(true));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloBreadcrumb),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty ShowSeparatorsProperty =
        DependencyProperty.Register(nameof(ShowSeparators), typeof(bool), typeof(HoloBreadcrumb),
            new PropertyMetadata(true));

    public static readonly DependencyProperty SeparatorStyleProperty =
        DependencyProperty.Register(nameof(SeparatorStyle), typeof(BreadcrumbSeparatorStyle), typeof(HoloBreadcrumb),
            new PropertyMetadata(BreadcrumbSeparatorStyle.ChevronRight));

    public static readonly DependencyProperty EnableTooltipsProperty =
        DependencyProperty.Register(nameof(EnableTooltips), typeof(bool), typeof(HoloBreadcrumb),
            new PropertyMetadata(true));

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

    public ObservableCollection<HoloBreadcrumbItem> BreadcrumbItems
    {
        get => (ObservableCollection<HoloBreadcrumbItem>)GetValue(BreadcrumbItemsProperty);
        set => SetValue(BreadcrumbItemsProperty, value);
    }

    public int MaxVisibleItems
    {
        get => (int)GetValue(MaxVisibleItemsProperty);
        set => SetValue(MaxVisibleItemsProperty, value);
    }

    public bool EnableNavigation
    {
        get => (bool)GetValue(EnableNavigationProperty);
        set => SetValue(EnableNavigationProperty, value);
    }

    public bool EnableAnimations
    {
        get => (bool)GetValue(EnableAnimationsProperty);
        set => SetValue(EnableAnimationsProperty, value);
    }

    public bool ShowSeparators
    {
        get => (bool)GetValue(ShowSeparatorsProperty);
        set => SetValue(ShowSeparatorsProperty, value);
    }

    public BreadcrumbSeparatorStyle SeparatorStyle
    {
        get => (BreadcrumbSeparatorStyle)GetValue(SeparatorStyleProperty);
        set => SetValue(SeparatorStyleProperty, value);
    }

    public bool EnableTooltips
    {
        get => (bool)GetValue(EnableTooltipsProperty);
        set => SetValue(EnableTooltipsProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloBreadcrumbEventArgs> ItemClicked;
    public event EventHandler<HoloBreadcrumbEventArgs> ItemHovered;
    public event EventHandler<HoloBreadcrumbEventArgs> NavigationRequested;

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode { get; private set; }

    public void SwitchToFullMode()
    {
        IsInSimplifiedMode = false;
        EnableAnimations = true;
        EnableTooltips = true;
        UpdateBreadcrumbAppearance();
    }

    public void SwitchToSimplifiedMode()
    {
        IsInSimplifiedMode = true;
        EnableAnimations = false;
        EnableTooltips = false;
        UpdateBreadcrumbAppearance();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => !_disposed && IsLoaded;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        EnableAnimations = settings.EnabledFeatures.HasFlag(AnimationFeatures.ComplexTransitions);
        
        UpdateBreadcrumbAppearance();
    }

    #endregion

    #region Fields

    private StackPanel _breadcrumbPanel;
    private Button _overflowButton;
    private Canvas _effectCanvas;
    
    private readonly Dictionary<HoloBreadcrumbItem, BreadcrumbItemControl> _itemControls = new();
    private readonly List<BreadcrumbParticle> _particles = new();
    private DispatcherTimer _particleTimer;
    private DispatcherTimer _hoverTimer;
    private double _particlePhase = 0;
    private double _hoverPhase = 0;
    private readonly Random _random = new();
    private bool _disposed = false;
    private bool _isAnimating = false;

    #endregion

    #region Constructor

    public HoloBreadcrumb()
    {
        DefaultStyleKey = typeof(HoloBreadcrumb);
        BreadcrumbItems = new ObservableCollection<HoloBreadcrumbItem>();
        Height = 32;
        InitializeBreadcrumb();
        SetupAnimations();
        
        // Register with animation intensity manager
        AnimationIntensityManager.Instance.RegisterTarget(this);
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Add a breadcrumb item
    /// </summary>
    public void AddBreadcrumbItem(HoloBreadcrumbItem item)
    {
        if (item != null && !BreadcrumbItems.Contains(item))
        {
            BreadcrumbItems.Add(item);
        }
    }

    /// <summary>
    /// Navigate to a specific breadcrumb item
    /// </summary>
    public void NavigateToItem(HoloBreadcrumbItem item)
    {
        if (item == null || !BreadcrumbItems.Contains(item)) return;

        var itemIndex = BreadcrumbItems.IndexOf(item);
        
        // Remove items after the selected one
        for (int i = BreadcrumbItems.Count - 1; i > itemIndex; i--)
        {
            BreadcrumbItems.RemoveAt(i);
        }

        NavigationRequested?.Invoke(this, new HoloBreadcrumbEventArgs { Item = item });
    }

    /// <summary>
    /// Clear all breadcrumb items
    /// </summary>
    public void ClearBreadcrumbs()
    {
        BreadcrumbItems.Clear();
    }

    /// <summary>
    /// Get navigation path as string
    /// </summary>
    public string GetNavigationPath(string separator = " > ")
    {
        return string.Join(separator, BreadcrumbItems.Select(item => item.Title));
    }

    #endregion

    #region Private Methods

    private void InitializeBreadcrumb()
    {
        Template = CreateBreadcrumbTemplate();
        UpdateBreadcrumbAppearance();
    }

    private ControlTemplate CreateBreadcrumbTemplate()
    {
        var template = new ControlTemplate(typeof(HoloBreadcrumb));

        // Main border
        var breadcrumbBorder = new FrameworkElementFactory(typeof(Border));
        breadcrumbBorder.Name = "PART_BreadcrumbBorder";
        breadcrumbBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
        breadcrumbBorder.SetValue(Border.BorderThicknessProperty, new Thickness(1));
        breadcrumbBorder.SetValue(Border.PaddingProperty, new Thickness(8, 4, 8, 4));

        // Main grid
        var mainGrid = new FrameworkElementFactory(typeof(Grid));

        // Column definitions
        var breadcrumbColumn = new FrameworkElementFactory(typeof(ColumnDefinition));
        breadcrumbColumn.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
        var overflowColumn = new FrameworkElementFactory(typeof(ColumnDefinition));
        overflowColumn.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);

        mainGrid.AppendChild(breadcrumbColumn);
        mainGrid.AppendChild(overflowColumn);

        // Breadcrumb items container
        var scrollViewer = new FrameworkElementFactory(typeof(ScrollViewer));
        scrollViewer.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
        scrollViewer.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
        scrollViewer.SetValue(Grid.ColumnProperty, 0);

        // Breadcrumb panel
        var breadcrumbPanel = new FrameworkElementFactory(typeof(StackPanel));
        breadcrumbPanel.Name = "PART_BreadcrumbPanel";
        breadcrumbPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        breadcrumbPanel.SetValue(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Center);

        scrollViewer.AppendChild(breadcrumbPanel);

        // Overflow button
        var overflowButton = new FrameworkElementFactory(typeof(Button));
        overflowButton.Name = "PART_OverflowButton";
        overflowButton.SetValue(Button.WidthProperty, 24.0);
        overflowButton.SetValue(Button.HeightProperty, 24.0);
        overflowButton.SetValue(Button.VisibilityProperty, Visibility.Collapsed);
        overflowButton.SetValue(Grid.ColumnProperty, 1);

        // Effect canvas
        var effectCanvas = new FrameworkElementFactory(typeof(Canvas));
        effectCanvas.Name = "PART_EffectCanvas";
        effectCanvas.SetValue(Canvas.IsHitTestVisibleProperty, false);
        effectCanvas.SetValue(Canvas.ClipToBoundsProperty, true);
        effectCanvas.SetValue(Grid.ColumnSpanProperty, 2);

        // Assembly
        mainGrid.AppendChild(scrollViewer);
        mainGrid.AppendChild(overflowButton);
        mainGrid.AppendChild(effectCanvas);
        breadcrumbBorder.AppendChild(mainGrid);

        template.VisualTree = breadcrumbBorder;
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

        // Hover effect timer
        _hoverTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
        };
        _hoverTimer.Tick += OnHoverTick;
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        if (!EnableAnimations || IsInSimplifiedMode || _effectCanvas == null) return;

        _particlePhase += 0.1;
        if (_particlePhase > Math.PI * 2)
            _particlePhase = 0;

        UpdateParticles();
        SpawnBreadcrumbParticles();
    }

    private void OnHoverTick(object sender, EventArgs e)
    {
        if (!EnableAnimations || IsInSimplifiedMode) return;

        _hoverPhase += 0.05;
        if (_hoverPhase > Math.PI * 2)
            _hoverPhase = 0;

        UpdateHoverEffects();
    }

    private void UpdateParticles()
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var particle = _particles[i];
            
            // Update particle position
            particle.X += particle.VelocityX;
            particle.Y += particle.VelocityY;
            particle.Life -= 0.02;

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

    private void SpawnBreadcrumbParticles()
    {
        if (_particles.Count >= 8) return; // Limit particle count

        if (_random.NextDouble() < 0.1) // 10% chance to spawn
        {
            var particle = CreateBreadcrumbParticle();
            _particles.Add(particle);
            _effectCanvas.Children.Add(particle.Visual);
        }
    }

    private BreadcrumbParticle CreateBreadcrumbParticle()
    {
        var color = GetEVEColor(EVEColorScheme);
        var size = 1 + _random.NextDouble() * 1.5;
        
        var ellipse = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromArgb(80, color.R, color.G, color.B))
        };

        var particle = new BreadcrumbParticle
        {
            Visual = ellipse,
            X = -size,
            Y = _random.NextDouble() * ActualHeight,
            VelocityX = 15 + _random.NextDouble() * 20,
            VelocityY = (_random.NextDouble() - 0.5) * 1,
            Life = 1.0
        };

        Canvas.SetLeft(ellipse, particle.X);
        Canvas.SetTop(ellipse, particle.Y);

        return particle;
    }

    private void UpdateHoverEffects()
    {
        foreach (var control in _itemControls.Values)
        {
            if (control.IsMouseOver && !IsInSimplifiedMode)
            {
                var hoverIntensity = 0.8 + (Math.Sin(_hoverPhase * 2) * 0.2);
                control.HoverIntensity = hoverIntensity;
            }
        }
    }

    private void UpdateBreadcrumbAppearance()
    {
        if (Template == null) return;

        Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            ApplyTemplate();
            
            _breadcrumbPanel = GetTemplateChild("PART_BreadcrumbPanel") as StackPanel;
            _overflowButton = GetTemplateChild("PART_OverflowButton") as Button;
            _effectCanvas = GetTemplateChild("PART_EffectCanvas") as Canvas;

            UpdateColors();
            UpdateEffects();
            RebuildBreadcrumbItems();
        }), DispatcherPriority.Render);
    }

    private void UpdateColors()
    {
        var color = GetEVEColor(EVEColorScheme);

        if (GetTemplateChild("PART_BreadcrumbBorder") is Border border)
        {
            var backgroundBrush = new LinearGradientBrush();
            backgroundBrush.StartPoint = new Point(0, 0);
            backgroundBrush.EndPoint = new Point(1, 0);
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(60, 0, 20, 40), 0.0));
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(40, 0, 15, 30), 1.0));

            border.Background = backgroundBrush;
            border.BorderBrush = new SolidColorBrush(Color.FromArgb(
                120, color.R, color.G, color.B));
        }
    }

    private void UpdateEffects()
    {
        var color = GetEVEColor(EVEColorScheme);

        if (GetTemplateChild("PART_BreadcrumbBorder") is Border border && !IsInSimplifiedMode)
        {
            border.Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 4 * HolographicIntensity,
                ShadowDepth = 0,
                Opacity = 0.3 * HolographicIntensity
            };
        }
    }

    private void RebuildBreadcrumbItems()
    {
        if (_breadcrumbPanel == null) return;

        _breadcrumbPanel.Children.Clear();
        _itemControls.Clear();

        var visibleItems = GetVisibleItems();
        
        for (int i = 0; i < visibleItems.Count; i++)
        {
            var item = visibleItems[i];
            var itemControl = CreateBreadcrumbItemControl(item, i == visibleItems.Count - 1);
            _itemControls[item] = itemControl;
            _breadcrumbPanel.Children.Add(itemControl);

            // Add separator
            if (ShowSeparators && i < visibleItems.Count - 1)
            {
                var separator = CreateSeparator();
                _breadcrumbPanel.Children.Add(separator);
            }
        }

        UpdateOverflowButton();
    }

    private List<HoloBreadcrumbItem> GetVisibleItems()
    {
        if (BreadcrumbItems.Count <= MaxVisibleItems)
            return BreadcrumbItems.ToList();

        var result = new List<HoloBreadcrumbItem>();
        
        // Always show first item
        if (BreadcrumbItems.Count > 0)
            result.Add(BreadcrumbItems[0]);

        // Show last few items
        var startIndex = Math.Max(1, BreadcrumbItems.Count - (MaxVisibleItems - 1));
        for (int i = startIndex; i < BreadcrumbItems.Count; i++)
        {
            result.Add(BreadcrumbItems[i]);
        }

        return result;
    }

    private BreadcrumbItemControl CreateBreadcrumbItemControl(HoloBreadcrumbItem item, bool isLast)
    {
        var control = new BreadcrumbItemControl
        {
            Item = item,
            HolographicIntensity = HolographicIntensity,
            EVEColorScheme = EVEColorScheme,
            IsNavigable = EnableNavigation && !isLast,
            EnableAnimations = EnableAnimations && !IsInSimplifiedMode,
            EnableTooltips = EnableTooltips && !IsInSimplifiedMode,
            Margin = new Thickness(2, 0, 2, 0)
        };

        control.ItemClicked += OnBreadcrumbItemClicked;
        control.ItemHovered += OnBreadcrumbItemHovered;

        return control;
    }

    private FrameworkElement CreateSeparator()
    {
        var color = GetEVEColor(EVEColorScheme);
        var separatorPath = new Path
        {
            Width = 12,
            Height = 12,
            Fill = new SolidColorBrush(Color.FromArgb(120, color.R, color.G, color.B)),
            Data = GetSeparatorGeometry(),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(4, 0, 4, 0)
        };

        if (!IsInSimplifiedMode)
        {
            separatorPath.Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 2,
                ShadowDepth = 0,
                Opacity = 0.5 * HolographicIntensity
            };
        }

        return separatorPath;
    }

    private Geometry GetSeparatorGeometry()
    {
        return SeparatorStyle switch
        {
            BreadcrumbSeparatorStyle.ChevronRight => Geometry.Parse("M8.59,16.58L13.17,12L8.59,7.41L10,6L16,12L10,18L8.59,16.58Z"),
            BreadcrumbSeparatorStyle.Arrow => Geometry.Parse("M8,5.14V19.14L19,12.14L8,5.14Z"),
            BreadcrumbSeparatorStyle.Slash => Geometry.Parse("M7,21L9,21L17,3L15,3L7,21Z"),
            BreadcrumbSeparatorStyle.Dot => Geometry.Parse("M12,10A2,2 0 0,0 10,12C10,13.11 10.9,14 12,14C13.11,14 14,13.11 14,12A2,2 0 0,0 12,10Z"),
            _ => Geometry.Parse("M8.59,16.58L13.17,12L8.59,7.41L10,6L16,12L10,18L8.59,16.58Z")
        };
    }

    private void UpdateOverflowButton()
    {
        if (_overflowButton == null) return;

        var hasOverflow = BreadcrumbItems.Count > MaxVisibleItems;
        _overflowButton.Visibility = hasOverflow ? Visibility.Visible : Visibility.Collapsed;

        if (hasOverflow)
        {
            var color = GetEVEColor(EVEColorScheme);
            var overflowIcon = new Path
            {
                Width = 12,
                Height = 12,
                Fill = new SolidColorBrush(color),
                Data = Geometry.Parse("M6,10C4.9,10 4,10.9 4,12A2,2 0 0,0 6,14C7.1,14 8,13.1 8,12C8,10.9 7.1,10 6,10M12,10C10.9,10 10,10.9 10,12A2,2 0 0,0 12,14C13.1,14 14,13.1 14,12C14,10.9 13.1,10 12,10M18,10C16.9,10 16,10.9 16,12A2,2 0 0,0 18,14C19.1,14 20,13.1 20,12C20,10.9 19.1,10 18,10Z"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            _overflowButton.Content = overflowIcon;
            _overflowButton.Background = new SolidColorBrush(Color.FromArgb(60, color.R, color.G, color.B));
            _overflowButton.BorderBrush = new SolidColorBrush(Color.FromArgb(100, color.R, color.G, color.B));
            _overflowButton.BorderThickness = new Thickness(1);
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

    private void OnBreadcrumbItemClicked(object sender, BreadcrumbItemEventArgs e)
    {
        NavigateToItem(e.Item);
        ItemClicked?.Invoke(this, new HoloBreadcrumbEventArgs { Item = e.Item });
    }

    private void OnBreadcrumbItemHovered(object sender, BreadcrumbItemEventArgs e)
    {
        ItemHovered?.Invoke(this, new HoloBreadcrumbEventArgs { Item = e.Item });
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableAnimations && !IsInSimplifiedMode)
        {
            _particleTimer.Start();
            _hoverTimer.Start();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _particleTimer?.Stop();
        _hoverTimer?.Stop();
        
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
        if (d is HoloBreadcrumb breadcrumb)
            breadcrumb.UpdateBreadcrumbAppearance();
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloBreadcrumb breadcrumb)
            breadcrumb.UpdateBreadcrumbAppearance();
    }

    private static void OnBreadcrumbItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloBreadcrumb breadcrumb)
        {
            if (e.OldValue is ObservableCollection<HoloBreadcrumbItem> oldCollection)
                oldCollection.CollectionChanged -= breadcrumb.OnBreadcrumbItemsCollectionChanged;

            if (e.NewValue is ObservableCollection<HoloBreadcrumbItem> newCollection)
                newCollection.CollectionChanged += breadcrumb.OnBreadcrumbItemsCollectionChanged;

            breadcrumb.RebuildBreadcrumbItems();
        }
    }

    private void OnBreadcrumbItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        RebuildBreadcrumbItems();
    }

    private static void OnMaxVisibleItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloBreadcrumb breadcrumb)
            breadcrumb.RebuildBreadcrumbItems();
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloBreadcrumb breadcrumb)
        {
            if ((bool)e.NewValue && !breadcrumb.IsInSimplifiedMode)
            {
                breadcrumb._particleTimer.Start();
                breadcrumb._hoverTimer.Start();
            }
            else
            {
                breadcrumb._particleTimer.Stop();
                breadcrumb._hoverTimer.Stop();
            }
        }
    }

    #endregion
}

/// <summary>
/// Breadcrumb particle for visual effects
/// </summary>
internal class BreadcrumbParticle
{
    public FrameworkElement Visual { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
}

/// <summary>
/// Individual breadcrumb item control
/// </summary>
internal class BreadcrumbItemControl : Control
{
    public HoloBreadcrumbItem Item { get; set; }
    public double HolographicIntensity { get; set; }
    public EVEColorScheme EVEColorScheme { get; set; }
    public bool IsNavigable { get; set; }
    public bool EnableAnimations { get; set; }
    public bool EnableTooltips { get; set; }
    public double HoverIntensity { get; set; } = 1.0;

    public event EventHandler<BreadcrumbItemEventArgs> ItemClicked;
    public event EventHandler<BreadcrumbItemEventArgs> ItemHovered;

    private Border _itemBorder;
    private TextBlock _itemText;

    public BreadcrumbItemControl()
    {
        Template = CreateItemTemplate();
        MouseLeftButtonUp += OnItemClick;
        MouseEnter += OnItemHover;
        MouseLeave += OnItemLeave;
    }

    private ControlTemplate CreateItemTemplate()
    {
        var template = new ControlTemplate(typeof(BreadcrumbItemControl));

        var itemBorder = new FrameworkElementFactory(typeof(Border));
        itemBorder.Name = "PART_ItemBorder";
        itemBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
        itemBorder.SetValue(Border.PaddingProperty, new Thickness(8, 4, 8, 4));
        itemBorder.SetValue(Button.CursorProperty, Cursors.Hand);

        var itemText = new FrameworkElementFactory(typeof(TextBlock));
        itemText.Name = "PART_ItemText";
        itemText.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
        itemText.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        itemText.SetValue(TextBlock.FontSizeProperty, 11.0);

        itemBorder.AppendChild(itemText);
        template.VisualTree = itemBorder;
        return template;
    }

    private void OnItemClick(object sender, MouseButtonEventArgs e)
    {
        if (IsNavigable)
            ItemClicked?.Invoke(this, new BreadcrumbItemEventArgs { Item = Item });
    }

    private void OnItemHover(object sender, MouseEventArgs e)
    {
        ItemHovered?.Invoke(this, new BreadcrumbItemEventArgs { Item = Item });
    }

    private void OnItemLeave(object sender, MouseEventArgs e)
    {
        HoverIntensity = 1.0;
    }
}

/// <summary>
/// Breadcrumb item data model
/// </summary>
public class HoloBreadcrumbItem : INotifyPropertyChanged
{
    private string _title;
    private string _tooltip;
    private object _data;

    public string Title
    {
        get => _title;
        set { _title = value; OnPropertyChanged(); }
    }

    public string Tooltip
    {
        get => _tooltip;
        set { _tooltip = value; OnPropertyChanged(); }
    }

    public object Data
    {
        get => _data;
        set { _data = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Breadcrumb separator styles
/// </summary>
public enum BreadcrumbSeparatorStyle
{
    ChevronRight,
    Arrow,
    Slash,
    Dot
}

/// <summary>
/// Event args for breadcrumb events
/// </summary>
public class HoloBreadcrumbEventArgs : EventArgs
{
    public HoloBreadcrumbItem Item { get; set; }
}

/// <summary>
/// Event args for breadcrumb item events
/// </summary>
public class BreadcrumbItemEventArgs : EventArgs
{
    public HoloBreadcrumbItem Item { get; set; }
}