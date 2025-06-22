// ==========================================================================
// HoloSidebar.cs - Holographic Sidebar Navigation with Module Icons
// ==========================================================================
// Advanced holographic sidebar navigation featuring module icons,
// EVE-style visual effects, and adaptive layout management.
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
/// Advanced holographic sidebar navigation with module icons and EVE styling
/// </summary>
public class HoloSidebar : Control, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloSidebar),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloSidebar),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty SidebarItemsProperty =
        DependencyProperty.Register(nameof(SidebarItems), typeof(ObservableCollection<HoloSidebarItem>), typeof(HoloSidebar),
            new PropertyMetadata(null, OnSidebarItemsChanged));

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(HoloSidebarItem), typeof(HoloSidebar),
            new PropertyMetadata(null, OnSelectedItemChanged));

    public static readonly DependencyProperty SidebarModeProperty =
        DependencyProperty.Register(nameof(SidebarMode), typeof(HoloSidebarMode), typeof(HoloSidebar),
            new PropertyMetadata(HoloSidebarMode.Icons, OnSidebarModeChanged));

    public static readonly DependencyProperty EnableHoverEffectsProperty =
        DependencyProperty.Register(nameof(EnableHoverEffects), typeof(bool), typeof(HoloSidebar),
            new PropertyMetadata(true, OnEnableHoverEffectsChanged));

    public static readonly DependencyProperty EnableSelectionAnimationProperty =
        DependencyProperty.Register(nameof(EnableSelectionAnimation), typeof(bool), typeof(HoloSidebar),
            new PropertyMetadata(true, OnEnableSelectionAnimationChanged));

    public static readonly DependencyProperty ShowTooltipsProperty =
        DependencyProperty.Register(nameof(ShowTooltips), typeof(bool), typeof(HoloSidebar),
            new PropertyMetadata(true));

    public static readonly DependencyProperty CollapsedWidthProperty =
        DependencyProperty.Register(nameof(CollapsedWidth), typeof(double), typeof(HoloSidebar),
            new PropertyMetadata(60.0));

    public static readonly DependencyProperty ExpandedWidthProperty =
        DependencyProperty.Register(nameof(ExpandedWidth), typeof(double), typeof(HoloSidebar),
            new PropertyMetadata(200.0));

    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(HoloSidebar),
            new PropertyMetadata(false, OnIsExpandedChanged));

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

    public ObservableCollection<HoloSidebarItem> SidebarItems
    {
        get => (ObservableCollection<HoloSidebarItem>)GetValue(SidebarItemsProperty);
        set => SetValue(SidebarItemsProperty, value);
    }

    public HoloSidebarItem SelectedItem
    {
        get => (HoloSidebarItem)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public HoloSidebarMode SidebarMode
    {
        get => (HoloSidebarMode)GetValue(SidebarModeProperty);
        set => SetValue(SidebarModeProperty, value);
    }

    public bool EnableHoverEffects
    {
        get => (bool)GetValue(EnableHoverEffectsProperty);
        set => SetValue(EnableHoverEffectsProperty, value);
    }

    public bool EnableSelectionAnimation
    {
        get => (bool)GetValue(EnableSelectionAnimationProperty);
        set => SetValue(EnableSelectionAnimationProperty, value);
    }

    public bool ShowTooltips
    {
        get => (bool)GetValue(ShowTooltipsProperty);
        set => SetValue(ShowTooltipsProperty, value);
    }

    public double CollapsedWidth
    {
        get => (double)GetValue(CollapsedWidthProperty);
        set => SetValue(CollapsedWidthProperty, value);
    }

    public double ExpandedWidth
    {
        get => (double)GetValue(ExpandedWidthProperty);
        set => SetValue(ExpandedWidthProperty, value);
    }

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloSidebarEventArgs> ItemSelected;
    public event EventHandler<HoloSidebarEventArgs> ItemHovered;
    public event EventHandler<HoloSidebarEventArgs> SidebarExpanded;
    public event EventHandler<HoloSidebarEventArgs> SidebarCollapsed;

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode { get; private set; }

    public void SwitchToFullMode()
    {
        IsInSimplifiedMode = false;
        EnableHoverEffects = true;
        EnableSelectionAnimation = true;
        ShowTooltips = true;
        UpdateSidebarAppearance();
    }

    public void SwitchToSimplifiedMode()
    {
        IsInSimplifiedMode = true;
        EnableHoverEffects = false;
        EnableSelectionAnimation = false;
        ShowTooltips = false;
        UpdateSidebarAppearance();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => !_disposed && IsLoaded;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        EnableHoverEffects = settings.EnabledFeatures.HasFlag(AnimationFeatures.BasicGlow);
        EnableSelectionAnimation = settings.EnabledFeatures.HasFlag(AnimationFeatures.ComplexTransitions);
        
        UpdateSidebarAppearance();
    }

    #endregion

    #region Fields

    private StackPanel _itemsPanel;
    private Border _sidebarBorder;
    private Button _expandToggleButton;
    private Canvas _effectCanvas;
    private Rectangle _selectionIndicator;
    
    private readonly Dictionary<HoloSidebarItem, SidebarItemControl> _itemControls = new();
    private readonly List<SidebarParticle> _particles = new();
    private DispatcherTimer _particleTimer;
    private DispatcherTimer _glowTimer;
    private double _particlePhase = 0;
    private double _glowPhase = 0;
    private readonly Random _random = new();
    private bool _disposed = false;
    private bool _isAnimating = false;

    #endregion

    #region Constructor

    public HoloSidebar()
    {
        DefaultStyleKey = typeof(HoloSidebar);
        SidebarItems = new ObservableCollection<HoloSidebarItem>();
        Width = CollapsedWidth;
        InitializeSidebar();
        SetupAnimations();
        
        // Register with animation intensity manager
        AnimationIntensityManager.Instance.RegisterTarget(this);
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Add a sidebar item
    /// </summary>
    public void AddSidebarItem(HoloSidebarItem item)
    {
        if (item != null && !SidebarItems.Contains(item))
        {
            SidebarItems.Add(item);
        }
    }

    /// <summary>
    /// Remove a sidebar item
    /// </summary>
    public void RemoveSidebarItem(HoloSidebarItem item)
    {
        if (item != null)
        {
            SidebarItems.Remove(item);
        }
    }

    /// <summary>
    /// Select a specific item
    /// </summary>
    public void SelectItem(HoloSidebarItem item)
    {
        if (item != null && SidebarItems.Contains(item))
        {
            SelectedItem = item;
        }
    }

    /// <summary>
    /// Toggle sidebar expansion
    /// </summary>
    public void ToggleExpansion()
    {
        IsExpanded = !IsExpanded;
    }

    /// <summary>
    /// Show badge on specific item
    /// </summary>
    public void ShowBadge(HoloSidebarItem item, string badgeText, Color badgeColor)
    {
        if (item != null && _itemControls.ContainsKey(item))
        {
            _itemControls[item].ShowBadge(badgeText, badgeColor);
        }
    }

    /// <summary>
    /// Hide badge on specific item
    /// </summary>
    public void HideBadge(HoloSidebarItem item)
    {
        if (item != null && _itemControls.ContainsKey(item))
        {
            _itemControls[item].HideBadge();
        }
    }

    #endregion

    #region Private Methods

    private void InitializeSidebar()
    {
        Template = CreateSidebarTemplate();
        UpdateSidebarAppearance();
    }

    private ControlTemplate CreateSidebarTemplate()
    {
        var template = new ControlTemplate(typeof(HoloSidebar));

        // Main border
        var sidebarBorder = new FrameworkElementFactory(typeof(Border));
        sidebarBorder.Name = "PART_SidebarBorder";
        sidebarBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(8, 0, 0, 8));
        sidebarBorder.SetValue(Border.BorderThicknessProperty, new Thickness(1, 1, 0, 1));

        // Main grid
        var mainGrid = new FrameworkElementFactory(typeof(Grid));

        // Row definitions
        var itemsRow = new FrameworkElementFactory(typeof(RowDefinition));
        itemsRow.SetValue(RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Star));
        var toggleRow = new FrameworkElementFactory(typeof(RowDefinition));
        toggleRow.SetValue(RowDefinition.HeightProperty, GridLength.Auto);

        mainGrid.AppendChild(itemsRow);
        mainGrid.AppendChild(toggleRow);

        // Items panel container
        var itemsContainer = new FrameworkElementFactory(typeof(ScrollViewer));
        itemsContainer.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
        itemsContainer.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
        itemsContainer.SetValue(Grid.RowProperty, 0);

        // Items panel
        var itemsPanel = new FrameworkElementFactory(typeof(StackPanel));
        itemsPanel.Name = "PART_ItemsPanel";
        itemsPanel.SetValue(StackPanel.OrientationProperty, Orientation.Vertical);
        itemsPanel.SetValue(StackPanel.MarginProperty, new Thickness(4));

        itemsContainer.AppendChild(itemsPanel);

        // Expand toggle button
        var expandToggleButton = new FrameworkElementFactory(typeof(Button));
        expandToggleButton.Name = "PART_ExpandToggleButton";
        expandToggleButton.SetValue(Button.HeightProperty, 30.0);
        expandToggleButton.SetValue(Button.MarginProperty, new Thickness(4));
        expandToggleButton.SetValue(Grid.RowProperty, 1);

        // Selection indicator
        var selectionIndicator = new FrameworkElementFactory(typeof(Rectangle));
        selectionIndicator.Name = "PART_SelectionIndicator";
        selectionIndicator.SetValue(Rectangle.WidthProperty, 4.0);
        selectionIndicator.SetValue(Rectangle.HorizontalAlignmentProperty, HorizontalAlignment.Left);
        selectionIndicator.SetValue(Rectangle.RadiusXProperty, 2.0);
        selectionIndicator.SetValue(Rectangle.RadiusYProperty, 2.0);
        selectionIndicator.SetValue(Grid.RowSpanProperty, 2);

        // Effect canvas
        var effectCanvas = new FrameworkElementFactory(typeof(Canvas));
        effectCanvas.Name = "PART_EffectCanvas";
        effectCanvas.SetValue(Canvas.IsHitTestVisibleProperty, false);
        effectCanvas.SetValue(Canvas.ClipToBoundsProperty, true);
        effectCanvas.SetValue(Grid.RowSpanProperty, 2);

        // Assembly
        mainGrid.AppendChild(itemsContainer);
        mainGrid.AppendChild(expandToggleButton);
        mainGrid.AppendChild(selectionIndicator);
        mainGrid.AppendChild(effectCanvas);
        sidebarBorder.AppendChild(mainGrid);

        template.VisualTree = sidebarBorder;
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

        // Glow animation timer
        _glowTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
        };
        _glowTimer.Tick += OnGlowTick;
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        if (!EnableHoverEffects || IsInSimplifiedMode || _effectCanvas == null) return;

        _particlePhase += 0.1;
        if (_particlePhase > Math.PI * 2)
            _particlePhase = 0;

        UpdateParticles();
        SpawnSidebarParticles();
    }

    private void OnGlowTick(object sender, EventArgs e)
    {
        if (!EnableHoverEffects || IsInSimplifiedMode) return;

        _glowPhase += 0.05;
        if (_glowPhase > Math.PI * 2)
            _glowPhase = 0;

        UpdateGlowEffects();
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
            if (particle.Life <= 0 || particle.Y < -10)
            {
                _effectCanvas.Children.Remove(particle.Visual);
                _particles.RemoveAt(i);
            }
        }
    }

    private void SpawnSidebarParticles()
    {
        if (_particles.Count >= 10) return; // Limit particle count

        if (_random.NextDouble() < 0.1) // 10% chance to spawn
        {
            var particle = CreateSidebarParticle();
            _particles.Add(particle);
            _effectCanvas.Children.Add(particle.Visual);
        }
    }

    private SidebarParticle CreateSidebarParticle()
    {
        var color = GetEVEColor(EVEColorScheme);
        var size = 1 + _random.NextDouble() * 2;
        
        var ellipse = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromArgb(120, color.R, color.G, color.B))
        };

        var particle = new SidebarParticle
        {
            Visual = ellipse,
            X = _random.NextDouble() * ActualWidth,
            Y = ActualHeight + size,
            VelocityX = (_random.NextDouble() - 0.5) * 1,
            VelocityY = -2 - _random.NextDouble() * 3,
            Life = 1.0
        };

        Canvas.SetLeft(ellipse, particle.X);
        Canvas.SetTop(ellipse, particle.Y);

        return particle;
    }

    private void UpdateGlowEffects()
    {
        if (_selectionIndicator?.Effect is DropShadowEffect effect)
        {
            var glowIntensity = 0.6 + (Math.Sin(_glowPhase) * 0.4);
            effect.Opacity = HolographicIntensity * glowIntensity;
        }
    }

    private void UpdateSidebarAppearance()
    {
        if (Template == null) return;

        Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            ApplyTemplate();
            
            _itemsPanel = GetTemplateChild("PART_ItemsPanel") as StackPanel;
            _sidebarBorder = GetTemplateChild("PART_SidebarBorder") as Border;
            _expandToggleButton = GetTemplateChild("PART_ExpandToggleButton") as Button;
            _effectCanvas = GetTemplateChild("PART_EffectCanvas") as Canvas;
            _selectionIndicator = GetTemplateChild("PART_SelectionIndicator") as Rectangle;

            UpdateColors();
            UpdateEffects();
            UpdateToggleButton();
            RebuildSidebarItems();
            UpdateSelectionIndicator();
        }), DispatcherPriority.Render);
    }

    private void UpdateColors()
    {
        var color = GetEVEColor(EVEColorScheme);

        if (_sidebarBorder != null)
        {
            var backgroundBrush = new LinearGradientBrush();
            backgroundBrush.StartPoint = new Point(0, 0);
            backgroundBrush.EndPoint = new Point(1, 1);
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(180, 0, 20, 40), 0.0));
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(120, 0, 15, 30), 1.0));

            _sidebarBorder.Background = backgroundBrush;
            _sidebarBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(
                150, color.R, color.G, color.B));
        }

        if (_selectionIndicator != null)
        {
            _selectionIndicator.Fill = new SolidColorBrush(color);
        }
    }

    private void UpdateEffects()
    {
        var color = GetEVEColor(EVEColorScheme);

        if (_sidebarBorder != null && !IsInSimplifiedMode)
        {
            _sidebarBorder.Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 8 * HolographicIntensity,
                ShadowDepth = 0,
                Opacity = 0.4 * HolographicIntensity
            };
        }
        else if (_sidebarBorder != null)
        {
            _sidebarBorder.Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 2,
                ShadowDepth = 0,
                Opacity = 0.2 * HolographicIntensity
            };
        }

        if (_selectionIndicator != null && !IsInSimplifiedMode)
        {
            _selectionIndicator.Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 6,
                ShadowDepth = 0,
                Opacity = 0.8 * HolographicIntensity
            };
        }
    }

    private void UpdateToggleButton()
    {
        if (_expandToggleButton == null) return;

        var color = GetEVEColor(EVEColorScheme);
        var toggleIcon = new Path
        {
            Width = 16,
            Height = 16,
            Fill = new SolidColorBrush(color),
            Data = IsExpanded ? 
                Geometry.Parse("M15.41,16.58L10.83,12L15.41,7.41L14,6L8,12L14,18L15.41,16.58Z") :
                Geometry.Parse("M8.59,16.58L13.17,12L8.59,7.41L10,6L16,12L10,18L8.59,16.58Z"),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        _expandToggleButton.Content = toggleIcon;
        _expandToggleButton.Background = new SolidColorBrush(Color.FromArgb(60, color.R, color.G, color.B));
        _expandToggleButton.BorderBrush = new SolidColorBrush(Color.FromArgb(100, color.R, color.G, color.B));
        _expandToggleButton.BorderThickness = new Thickness(1);
        _expandToggleButton.Click -= OnExpandToggleClick;
        _expandToggleButton.Click += OnExpandToggleClick;
    }

    private void RebuildSidebarItems()
    {
        if (_itemsPanel == null) return;

        _itemsPanel.Children.Clear();
        _itemControls.Clear();

        foreach (var item in SidebarItems)
        {
            var itemControl = CreateSidebarItemControl(item);
            _itemControls[item] = itemControl;
            _itemsPanel.Children.Add(itemControl);
        }
    }

    private SidebarItemControl CreateSidebarItemControl(HoloSidebarItem item)
    {
        var control = new SidebarItemControl
        {
            Item = item,
            HolographicIntensity = HolographicIntensity,
            EVEColorScheme = EVEColorScheme,
            IsExpanded = IsExpanded,
            EnableHoverEffects = EnableHoverEffects && !IsInSimplifiedMode,
            ShowTooltips = ShowTooltips && !IsInSimplifiedMode,
            Margin = new Thickness(0, 2, 0, 2)
        };

        control.ItemClicked += OnSidebarItemClicked;
        control.ItemHovered += OnSidebarItemHovered;

        return control;
    }

    private void UpdateSelectionIndicator()
    {
        if (_selectionIndicator == null || SelectedItem == null) return;

        if (_itemControls.ContainsKey(SelectedItem))
        {
            var selectedControl = _itemControls[SelectedItem];
            var position = selectedControl.TransformToAncestor(this).Transform(new Point(0, 0));
            
            if (EnableSelectionAnimation && !IsInSimplifiedMode && !_isAnimating)
            {
                AnimateSelectionIndicator(position.Y, selectedControl.ActualHeight);
            }
            else
            {
                Canvas.SetTop(_selectionIndicator, position.Y);
                _selectionIndicator.Height = selectedControl.ActualHeight;
                _selectionIndicator.Visibility = Visibility.Visible;
            }
        }
        else
        {
            _selectionIndicator.Visibility = Visibility.Collapsed;
        }
    }

    private void AnimateSelectionIndicator(double targetY, double targetHeight)
    {
        if (_isAnimating) return;

        _isAnimating = true;
        _selectionIndicator.Visibility = Visibility.Visible;

        var positionAnimation = new DoubleAnimation
        {
            To = targetY,
            Duration = TimeSpan.FromMilliseconds(200),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var heightAnimation = new DoubleAnimation
        {
            To = targetHeight,
            Duration = TimeSpan.FromMilliseconds(200),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        positionAnimation.Completed += (s, e) => _isAnimating = false;

        _selectionIndicator.BeginAnimation(Canvas.TopProperty, positionAnimation);
        _selectionIndicator.BeginAnimation(HeightProperty, heightAnimation);
    }

    private void AnimateSidebarExpansion()
    {
        if (_isAnimating) return;

        _isAnimating = true;
        var targetWidth = IsExpanded ? ExpandedWidth : CollapsedWidth;

        var widthAnimation = new DoubleAnimation
        {
            To = targetWidth,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        widthAnimation.Completed += (s, e) =>
        {
            _isAnimating = false;
            UpdateItemControlsExpansion();
            
            if (IsExpanded)
                SidebarExpanded?.Invoke(this, new HoloSidebarEventArgs { SelectedItem = SelectedItem });
            else
                SidebarCollapsed?.Invoke(this, new HoloSidebarEventArgs { SelectedItem = SelectedItem });
        };

        BeginAnimation(WidthProperty, widthAnimation);
    }

    private void UpdateItemControlsExpansion()
    {
        foreach (var control in _itemControls.Values)
        {
            control.IsExpanded = IsExpanded;
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

    private void OnSidebarItemClicked(object sender, SidebarItemEventArgs e)
    {
        SelectedItem = e.Item;
        ItemSelected?.Invoke(this, new HoloSidebarEventArgs { SelectedItem = e.Item });
    }

    private void OnSidebarItemHovered(object sender, SidebarItemEventArgs e)
    {
        ItemHovered?.Invoke(this, new HoloSidebarEventArgs { SelectedItem = e.Item });
    }

    private void OnExpandToggleClick(object sender, RoutedEventArgs e)
    {
        ToggleExpansion();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableHoverEffects && !IsInSimplifiedMode)
        {
            _particleTimer.Start();
            _glowTimer.Start();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _particleTimer?.Stop();
        _glowTimer?.Stop();
        
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
        if (d is HoloSidebar sidebar)
            sidebar.UpdateSidebarAppearance();
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSidebar sidebar)
            sidebar.UpdateSidebarAppearance();
    }

    private static void OnSidebarItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSidebar sidebar)
            sidebar.RebuildSidebarItems();
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSidebar sidebar)
            sidebar.UpdateSelectionIndicator();
    }

    private static void OnSidebarModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSidebar sidebar)
            sidebar.UpdateSidebarAppearance();
    }

    private static void OnEnableHoverEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSidebar sidebar)
        {
            if ((bool)e.NewValue && !sidebar.IsInSimplifiedMode)
            {
                sidebar._particleTimer.Start();
                sidebar._glowTimer.Start();
            }
            else
            {
                sidebar._particleTimer.Stop();
                sidebar._glowTimer.Stop();
            }
        }
    }

    private static void OnEnableSelectionAnimationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Animation enabling/disabling is handled in real-time
    }

    private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSidebar sidebar)
        {
            sidebar.UpdateToggleButton();
            sidebar.AnimateSidebarExpansion();
        }
    }

    #endregion
}

/// <summary>
/// Sidebar particle for visual effects
/// </summary>
internal class SidebarParticle
{
    public FrameworkElement Visual { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
}

/// <summary>
/// Individual sidebar item control
/// </summary>
internal class SidebarItemControl : Control
{
    public HoloSidebarItem Item { get; set; }
    public double HolographicIntensity { get; set; }
    public EVEColorScheme EVEColorScheme { get; set; }
    public bool IsExpanded { get; set; }
    public bool EnableHoverEffects { get; set; }
    public bool ShowTooltips { get; set; }

    public event EventHandler<SidebarItemEventArgs> ItemClicked;
    public event EventHandler<SidebarItemEventArgs> ItemHovered;

    private Border _itemBorder;
    private Path _iconPath;
    private TextBlock _labelText;
    private Border _badgeBorder;

    public SidebarItemControl()
    {
        Template = CreateItemTemplate();
        MouseLeftButtonUp += OnItemClick;
        MouseEnter += OnItemHover;
    }

    private ControlTemplate CreateItemTemplate()
    {
        var template = new ControlTemplate(typeof(SidebarItemControl));

        var itemBorder = new FrameworkElementFactory(typeof(Border));
        itemBorder.Name = "PART_ItemBorder";
        itemBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
        itemBorder.SetValue(Border.PaddingProperty, new Thickness(8));
        itemBorder.SetValue(Button.CursorProperty, Cursors.Hand);

        var itemGrid = new FrameworkElementFactory(typeof(Grid));

        var iconColumn = new FrameworkElementFactory(typeof(ColumnDefinition));
        iconColumn.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);
        var labelColumn = new FrameworkElementFactory(typeof(ColumnDefinition));
        labelColumn.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));

        itemGrid.AppendChild(iconColumn);
        itemGrid.AppendChild(labelColumn);

        var iconPath = new FrameworkElementFactory(typeof(Path));
        iconPath.Name = "PART_IconPath";
        iconPath.SetValue(Path.WidthProperty, 24.0);
        iconPath.SetValue(Path.HeightProperty, 24.0);
        iconPath.SetValue(Path.StretchProperty, Stretch.Uniform);
        iconPath.SetValue(Path.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        iconPath.SetValue(Path.VerticalAlignmentProperty, VerticalAlignment.Center);
        iconPath.SetValue(Grid.ColumnProperty, 0);

        var labelText = new FrameworkElementFactory(typeof(TextBlock));
        labelText.Name = "PART_LabelText";
        labelText.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
        labelText.SetValue(TextBlock.MarginProperty, new Thickness(12, 0, 0, 0));
        labelText.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        labelText.SetValue(TextBlock.FontSizeProperty, 12.0);
        labelText.SetValue(Grid.ColumnProperty, 1);

        var badgeBorder = new FrameworkElementFactory(typeof(Border));
        badgeBorder.Name = "PART_BadgeBorder";
        badgeBorder.SetValue(Border.VisibilityProperty, Visibility.Collapsed);

        itemGrid.AppendChild(iconPath);
        itemGrid.AppendChild(labelText);
        itemGrid.AppendChild(badgeBorder);
        itemBorder.AppendChild(itemGrid);

        template.VisualTree = itemBorder;
        return template;
    }

    public void ShowBadge(string text, Color color)
    {
        // Implementation for showing badges
    }

    public void HideBadge()
    {
        // Implementation for hiding badges
    }

    private void OnItemClick(object sender, MouseButtonEventArgs e)
    {
        ItemClicked?.Invoke(this, new SidebarItemEventArgs { Item = Item });
    }

    private void OnItemHover(object sender, MouseEventArgs e)
    {
        ItemHovered?.Invoke(this, new SidebarItemEventArgs { Item = Item });
    }
}

/// <summary>
/// Sidebar item data model
/// </summary>
public class HoloSidebarItem : INotifyPropertyChanged
{
    private string _title;
    private string _icon;
    private EVEColorScheme _colorScheme;
    private object _content;
    private bool _isEnabled = true;
    private int _priority = 0;

    public string Title
    {
        get => _title;
        set { _title = value; OnPropertyChanged(); }
    }

    public string Icon
    {
        get => _icon;
        set { _icon = value; OnPropertyChanged(); }
    }

    public EVEColorScheme ColorScheme
    {
        get => _colorScheme;
        set { _colorScheme = value; OnPropertyChanged(); }
    }

    public object Content
    {
        get => _content;
        set { _content = value; OnPropertyChanged(); }
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set { _isEnabled = value; OnPropertyChanged(); }
    }

    public int Priority
    {
        get => _priority;
        set { _priority = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Sidebar modes for different layouts
/// </summary>
public enum HoloSidebarMode
{
    Icons,
    IconsWithLabels,
    Compact,
    Extended
}

/// <summary>
/// Event args for sidebar events
/// </summary>
public class HoloSidebarEventArgs : EventArgs
{
    public HoloSidebarItem SelectedItem { get; set; }
}

/// <summary>
/// Event args for sidebar item events
/// </summary>
public class SidebarItemEventArgs : EventArgs
{
    public HoloSidebarItem Item { get; set; }
}