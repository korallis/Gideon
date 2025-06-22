// ==========================================================================
// HoloNavigation.cs - Holographic Navigation with Advanced Transitions
// ==========================================================================
// Advanced holographic navigation control featuring smooth transitions,
// EVE-style visual effects, breadcrumb navigation, and adaptive performance.
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
/// Advanced holographic navigation control with smooth transitions and EVE styling
/// </summary>
public class HoloNavigation : Control, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty NavigationItemsProperty =
        DependencyProperty.Register(nameof(NavigationItems), typeof(ObservableCollection<HoloNavigationItem>), typeof(HoloNavigation),
            new PropertyMetadata(null, OnNavigationItemsChanged));

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(HoloNavigationItem), typeof(HoloNavigation),
            new PropertyMetadata(null, OnSelectedItemChanged));

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloNavigation),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloNavigation),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty NavigationModeProperty =
        DependencyProperty.Register(nameof(NavigationMode), typeof(HoloNavigationMode), typeof(HoloNavigation),
            new PropertyMetadata(HoloNavigationMode.Horizontal, OnNavigationModeChanged));

    public static readonly DependencyProperty EnableTransitionAnimationsProperty =
        DependencyProperty.Register(nameof(EnableTransitionAnimations), typeof(bool), typeof(HoloNavigation),
            new PropertyMetadata(true, OnEnableTransitionAnimationsChanged));

    public static readonly DependencyProperty EnableHoverEffectsProperty =
        DependencyProperty.Register(nameof(EnableHoverEffects), typeof(bool), typeof(HoloNavigation),
            new PropertyMetadata(true));

    public static readonly DependencyProperty ShowBreadcrumbProperty =
        DependencyProperty.Register(nameof(ShowBreadcrumb), typeof(bool), typeof(HoloNavigation),
            new PropertyMetadata(true, OnShowBreadcrumbChanged));

    public static readonly DependencyProperty TransitionDurationProperty =
        DependencyProperty.Register(nameof(TransitionDuration), typeof(TimeSpan), typeof(HoloNavigation),
            new PropertyMetadata(TimeSpan.FromMilliseconds(300)));

    public static readonly DependencyProperty EnableParticleTrailProperty =
        DependencyProperty.Register(nameof(EnableParticleTrail), typeof(bool), typeof(HoloNavigation),
            new PropertyMetadata(true, OnEnableParticleTrailChanged));

    public ObservableCollection<HoloNavigationItem> NavigationItems
    {
        get => (ObservableCollection<HoloNavigationItem>)GetValue(NavigationItemsProperty);
        set => SetValue(NavigationItemsProperty, value);
    }

    public HoloNavigationItem SelectedItem
    {
        get => (HoloNavigationItem)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
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

    public HoloNavigationMode NavigationMode
    {
        get => (HoloNavigationMode)GetValue(NavigationModeProperty);
        set => SetValue(NavigationModeProperty, value);
    }

    public bool EnableTransitionAnimations
    {
        get => (bool)GetValue(EnableTransitionAnimationsProperty);
        set => SetValue(EnableTransitionAnimationsProperty, value);
    }

    public bool EnableHoverEffects
    {
        get => (bool)GetValue(EnableHoverEffectsProperty);
        set => SetValue(EnableHoverEffectsProperty, value);
    }

    public bool ShowBreadcrumb
    {
        get => (bool)GetValue(ShowBreadcrumbProperty);
        set => SetValue(ShowBreadcrumbProperty, value);
    }

    public TimeSpan TransitionDuration
    {
        get => (TimeSpan)GetValue(TransitionDurationProperty);
        set => SetValue(TransitionDurationProperty, value);
    }

    public bool EnableParticleTrail
    {
        get => (bool)GetValue(EnableParticleTrailProperty);
        set => SetValue(EnableParticleTrailProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloNavigationEventArgs> NavigationRequested;
    public event EventHandler<HoloNavigationEventArgs> NavigationCompleted;
    public event EventHandler<HoloNavigationEventArgs> ItemHovered;

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode { get; private set; }

    public void SwitchToFullMode()
    {
        IsInSimplifiedMode = false;
        EnableTransitionAnimations = true;
        EnableHoverEffects = true;
        EnableParticleTrail = true;
        UpdateNavigationAppearance();
    }

    public void SwitchToSimplifiedMode()
    {
        IsInSimplifiedMode = true;
        EnableTransitionAnimations = false;
        EnableHoverEffects = false;
        EnableParticleTrail = false;
        UpdateNavigationAppearance();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => !_disposed && IsLoaded;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        EnableTransitionAnimations = settings.EnabledFeatures.HasFlag(AnimationFeatures.ComplexTransitions);
        EnableHoverEffects = settings.EnabledFeatures.HasFlag(AnimationFeatures.BasicGlow);
        EnableParticleTrail = settings.EnabledFeatures.HasFlag(AnimationFeatures.ParticleEffects);
        
        UpdateNavigationAppearance();
    }

    #endregion

    #region Fields

    private StackPanel _navigationPanel;
    private StackPanel _breadcrumbPanel;
    private Canvas _particleCanvas;
    private Rectangle _selectionIndicator;
    private Border _navigationBorder;
    
    private readonly List<NavigationParticle> _particles = new();
    private DispatcherTimer _particleTimer;
    private DispatcherTimer _pulseTimer;
    private double _particlePhase = 0;
    private double _pulsePhase = 0;
    private readonly Random _random = new();
    private bool _isTransitioning = false;
    private bool _disposed = false;

    #endregion

    #region Constructor

    public HoloNavigation()
    {
        DefaultStyleKey = typeof(HoloNavigation);
        NavigationItems = new ObservableCollection<HoloNavigationItem>();
        InitializeNavigation();
        SetupAnimations();
        
        // Register with animation intensity manager
        AnimationIntensityManager.Instance.RegisterTarget(this);
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Navigate to a specific item with animation
    /// </summary>
    public void NavigateToItem(HoloNavigationItem item, bool animate = true)
    {
        if (item == null || _isTransitioning) return;

        var args = new HoloNavigationEventArgs { Item = item, Cancel = false };
        NavigationRequested?.Invoke(this, args);
        
        if (args.Cancel) return;

        if (animate && EnableTransitionAnimations && !IsInSimplifiedMode)
        {
            AnimateToItem(item);
        }
        else
        {
            SelectedItem = item;
        }
    }

    /// <summary>
    /// Add a new navigation item
    /// </summary>
    public void AddNavigationItem(HoloNavigationItem item)
    {
        if (item != null && !NavigationItems.Contains(item))
        {
            NavigationItems.Add(item);
        }
    }

    /// <summary>
    /// Remove a navigation item
    /// </summary>
    public void RemoveNavigationItem(HoloNavigationItem item)
    {
        if (item != null)
        {
            NavigationItems.Remove(item);
        }
    }

    /// <summary>
    /// Clear all navigation items
    /// </summary>
    public void ClearNavigationItems()
    {
        NavigationItems.Clear();
        SelectedItem = null;
    }

    #endregion

    #region Private Methods

    private void InitializeNavigation()
    {
        Template = CreateNavigationTemplate();
        UpdateNavigationAppearance();
    }

    private ControlTemplate CreateNavigationTemplate()
    {
        var template = new ControlTemplate(typeof(HoloNavigation));

        // Main grid container
        var mainGrid = new FrameworkElementFactory(typeof(Grid));

        // Row definitions
        var navRow = new FrameworkElementFactory(typeof(RowDefinition));
        navRow.SetValue(RowDefinition.HeightProperty, GridLength.Auto);
        var breadcrumbRow = new FrameworkElementFactory(typeof(RowDefinition));
        breadcrumbRow.SetValue(RowDefinition.HeightProperty, GridLength.Auto);

        mainGrid.AppendChild(navRow);
        mainGrid.AppendChild(breadcrumbRow);

        // Navigation border
        var navigationBorder = new FrameworkElementFactory(typeof(Border));
        navigationBorder.Name = "PART_NavigationBorder";
        navigationBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
        navigationBorder.SetValue(Border.BorderThicknessProperty, new Thickness(1));
        navigationBorder.SetValue(Border.PaddingProperty, new Thickness(12, 8, 12, 8));
        navigationBorder.SetValue(Grid.RowProperty, 0);

        // Navigation content grid
        var navContentGrid = new FrameworkElementFactory(typeof(Grid));

        // Navigation panel
        var navigationPanel = new FrameworkElementFactory(typeof(StackPanel));
        navigationPanel.Name = "PART_NavigationPanel";
        navigationPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        navigationPanel.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);

        // Selection indicator
        var selectionIndicator = new FrameworkElementFactory(typeof(Rectangle));
        selectionIndicator.Name = "PART_SelectionIndicator";
        selectionIndicator.SetValue(Rectangle.HeightProperty, 3.0);
        selectionIndicator.SetValue(Rectangle.VerticalAlignmentProperty, VerticalAlignment.Bottom);
        selectionIndicator.SetValue(Rectangle.RadiusXProperty, 1.5);
        selectionIndicator.SetValue(Rectangle.RadiusYProperty, 1.5);

        // Particle canvas
        var particleCanvas = new FrameworkElementFactory(typeof(Canvas));
        particleCanvas.Name = "PART_ParticleCanvas";
        particleCanvas.SetValue(Canvas.IsHitTestVisibleProperty, false);
        particleCanvas.SetValue(Canvas.ClipToBoundsProperty, true);

        navContentGrid.AppendChild(navigationPanel);
        navContentGrid.AppendChild(selectionIndicator);
        navContentGrid.AppendChild(particleCanvas);
        navigationBorder.AppendChild(navContentGrid);

        // Breadcrumb panel
        var breadcrumbPanel = new FrameworkElementFactory(typeof(StackPanel));
        breadcrumbPanel.Name = "PART_BreadcrumbPanel";
        breadcrumbPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        breadcrumbPanel.SetValue(StackPanel.MarginProperty, new Thickness(0, 8, 0, 0));
        breadcrumbPanel.SetValue(Grid.RowProperty, 1);

        // Assembly
        mainGrid.AppendChild(navigationBorder);
        mainGrid.AppendChild(breadcrumbPanel);

        template.VisualTree = mainGrid;
        return template;
    }

    private void SetupAnimations()
    {
        // Particle animation timer
        _particleTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
        };
        _particleTimer.Tick += OnParticleTick;

        // Pulse animation timer
        _pulseTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50) // 20 FPS
        };
        _pulseTimer.Tick += OnPulseTick;
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        if (!EnableParticleTrail || IsInSimplifiedMode || _particleCanvas == null) return;

        _particlePhase += 0.1;
        if (_particlePhase > Math.PI * 2)
            _particlePhase = 0;

        UpdateParticles();
        SpawnTrailParticles();
    }

    private void OnPulseTick(object sender, EventArgs e)
    {
        if (IsInSimplifiedMode) return;

        _pulsePhase += 0.05;
        if (_pulsePhase > Math.PI * 2)
            _pulsePhase = 0;

        UpdatePulseEffects();
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
            particle.Visual.Opacity = Math.Max(0, particle.Life);

            // Remove dead particles
            if (particle.Life <= 0)
            {
                _particleCanvas.Children.Remove(particle.Visual);
                _particles.RemoveAt(i);
            }
        }
    }

    private void SpawnTrailParticles()
    {
        if (_particles.Count >= 20) return; // Limit particle count

        if (SelectedItem != null && _selectionIndicator != null)
        {
            var particle = CreateTrailParticle();
            _particles.Add(particle);
            _particleCanvas.Children.Add(particle.Visual);
        }
    }

    private NavigationParticle CreateTrailParticle()
    {
        var color = GetEVEColor(EVEColorScheme);
        var size = 1 + _random.NextDouble() * 2;
        
        var ellipse = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromArgb(150, color.R, color.G, color.B))
        };

        var particle = new NavigationParticle
        {
            Visual = ellipse,
            X = _selectionIndicator.ActualWidth / 2 + _random.NextDouble() * 20 - 10,
            Y = _selectionIndicator.ActualHeight,
            VelocityX = (_random.NextDouble() - 0.5) * 2,
            VelocityY = -1 - _random.NextDouble() * 2,
            Life = 1.0
        };

        Canvas.SetLeft(ellipse, particle.X);
        Canvas.SetTop(ellipse, particle.Y);

        return particle;
    }

    private void UpdatePulseEffects()
    {
        if (_selectionIndicator?.Effect is DropShadowEffect effect)
        {
            var pulseIntensity = 0.7 + (Math.Sin(_pulsePhase) * 0.3);
            effect.Opacity = HolographicIntensity * pulseIntensity;
        }
    }

    private void AnimateToItem(HoloNavigationItem item)
    {
        if (_isTransitioning) return;

        _isTransitioning = true;
        var previousItem = SelectedItem;

        // Create transition animation
        var storyboard = new Storyboard();

        // Fade out current selection
        if (_selectionIndicator != null)
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(150)
            };
            Storyboard.SetTarget(fadeOut, _selectionIndicator);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath(OpacityProperty));
            storyboard.Children.Add(fadeOut);
        }

        storyboard.Completed += (s, e) =>
        {
            // Update selection
            SelectedItem = item;
            
            // Animate selection indicator to new position
            AnimateSelectionIndicator();
            
            _isTransitioning = false;
            NavigationCompleted?.Invoke(this, new HoloNavigationEventArgs { Item = item });
        };

        storyboard.Begin();
    }

    private void AnimateSelectionIndicator()
    {
        if (_selectionIndicator == null || SelectedItem == null) return;

        var targetPosition = GetItemPosition(SelectedItem);
        
        // Slide animation
        var slideAnimation = new DoubleAnimation
        {
            To = targetPosition.X,
            Duration = TransitionDuration,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        // Fade in animation
        var fadeIn = new DoubleAnimation
        {
            From = 0.0,
            To = 1.0,
            Duration = TransitionDuration,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        _selectionIndicator.BeginAnimation(Canvas.LeftProperty, slideAnimation);
        _selectionIndicator.BeginAnimation(OpacityProperty, fadeIn);
    }

    private Point GetItemPosition(HoloNavigationItem item)
    {
        // Calculate position based on item index
        var index = NavigationItems.IndexOf(item);
        var itemWidth = 100; // Estimated item width
        var x = index * (itemWidth + 8);
        return new Point(x, 0);
    }

    private void UpdateNavigationAppearance()
    {
        if (Template == null) return;

        Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            ApplyTemplate();
            
            _navigationPanel = GetTemplateChild("PART_NavigationPanel") as StackPanel;
            _breadcrumbPanel = GetTemplateChild("PART_BreadcrumbPanel") as StackPanel;
            _particleCanvas = GetTemplateChild("PART_ParticleCanvas") as Canvas;
            _selectionIndicator = GetTemplateChild("PART_SelectionIndicator") as Rectangle;
            _navigationBorder = GetTemplateChild("PART_NavigationBorder") as Border;

            UpdateColors();
            UpdateEffects();
            UpdateLayout();
            RebuildNavigation();
        }), DispatcherPriority.Render);
    }

    private void UpdateColors()
    {
        var color = GetEVEColor(EVEColorScheme);

        if (_navigationBorder != null)
        {
            var backgroundBrush = new LinearGradientBrush();
            backgroundBrush.StartPoint = new Point(0, 0);
            backgroundBrush.EndPoint = new Point(0, 1);
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(60, color.R, color.G, color.B), 0.0));
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(40, color.R, color.G, color.B), 0.5));
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(60, color.R, color.G, color.B), 1.0));

            _navigationBorder.Background = backgroundBrush;
            _navigationBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(
                150, color.R, color.G, color.B));
        }

        if (_selectionIndicator != null)
        {
            var indicatorBrush = new LinearGradientBrush();
            indicatorBrush.StartPoint = new Point(0, 0);
            indicatorBrush.EndPoint = new Point(1, 0);
            indicatorBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 0.0));
            indicatorBrush.GradientStops.Add(new GradientStop(color, 0.5));
            indicatorBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));

            _selectionIndicator.Fill = indicatorBrush;
        }
    }

    private void UpdateEffects()
    {
        var color = GetEVEColor(EVEColorScheme);

        if (_navigationBorder != null && !IsInSimplifiedMode)
        {
            _navigationBorder.Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 8 * HolographicIntensity,
                ShadowDepth = 0,
                Opacity = 0.4 * HolographicIntensity
            };
        }
        else if (_navigationBorder != null)
        {
            _navigationBorder.Effect = new DropShadowEffect
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

    private void UpdateLayout()
    {
        if (_navigationPanel != null)
        {
            _navigationPanel.Orientation = NavigationMode == HoloNavigationMode.Horizontal 
                ? Orientation.Horizontal 
                : Orientation.Vertical;
        }

        if (_breadcrumbPanel != null)
        {
            _breadcrumbPanel.Visibility = ShowBreadcrumb ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void RebuildNavigation()
    {
        if (_navigationPanel == null) return;

        _navigationPanel.Children.Clear();

        foreach (var item in NavigationItems)
        {
            var button = CreateNavigationButton(item);
            _navigationPanel.Children.Add(button);
        }

        UpdateBreadcrumb();
    }

    private Button CreateNavigationButton(HoloNavigationItem item)
    {
        var button = new Button
        {
            Content = item.Title,
            Tag = item,
            Style = CreateNavigationButtonStyle(),
            Margin = new Thickness(4, 0, 4, 0),
            Padding = new Thickness(16, 8, 16, 8)
        };

        button.Click += OnNavigationButtonClick;
        button.MouseEnter += OnNavigationButtonMouseEnter;
        button.MouseLeave += OnNavigationButtonMouseLeave;

        return button;
    }

    private Style CreateNavigationButtonStyle()
    {
        var color = GetEVEColor(EVEColorScheme);
        var style = new Style(typeof(Button));
        
        style.Setters.Add(new Setter(BackgroundProperty, Brushes.Transparent));
        style.Setters.Add(new Setter(ForegroundProperty, 
            new SolidColorBrush(Color.FromArgb(200, color.R, color.G, color.B))));
        style.Setters.Add(new Setter(BorderBrushProperty, Brushes.Transparent));
        style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(0)));
        style.Setters.Add(new Setter(FontWeightProperty, FontWeights.Bold));
        style.Setters.Add(new Setter(CursorProperty, Cursors.Hand));

        // Hover trigger
        var hoverTrigger = new Trigger { Property = IsMouseOverProperty, Value = true };
        hoverTrigger.Setters.Add(new Setter(ForegroundProperty, new SolidColorBrush(color)));
        
        style.Triggers.Add(hoverTrigger);

        return style;
    }

    private void UpdateBreadcrumb()
    {
        if (_breadcrumbPanel == null || !ShowBreadcrumb) return;

        _breadcrumbPanel.Children.Clear();

        if (SelectedItem != null)
        {
            var breadcrumbs = GetBreadcrumbPath(SelectedItem);
            
            for (int i = 0; i < breadcrumbs.Count; i++)
            {
                if (i > 0)
                {
                    // Add separator
                    var separator = new TextBlock
                    {
                        Text = " > ",
                        Foreground = new SolidColorBrush(Color.FromArgb(120, 128, 128, 128)),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    _breadcrumbPanel.Children.Add(separator);
                }

                var breadcrumbButton = new Button
                {
                    Content = breadcrumbs[i].Title,
                    Tag = breadcrumbs[i],
                    Style = CreateBreadcrumbButtonStyle(),
                    Margin = new Thickness(2)
                };
                
                breadcrumbButton.Click += OnBreadcrumbButtonClick;
                _breadcrumbPanel.Children.Add(breadcrumbButton);
            }
        }
    }

    private List<HoloNavigationItem> GetBreadcrumbPath(HoloNavigationItem item)
    {
        var path = new List<HoloNavigationItem>();
        var current = item;
        
        while (current != null)
        {
            path.Insert(0, current);
            current = current.Parent;
        }
        
        return path;
    }

    private Style CreateBreadcrumbButtonStyle()
    {
        var color = GetEVEColor(EVEColorScheme);
        var style = new Style(typeof(Button));
        
        style.Setters.Add(new Setter(BackgroundProperty, Brushes.Transparent));
        style.Setters.Add(new Setter(ForegroundProperty, 
            new SolidColorBrush(Color.FromArgb(150, color.R, color.G, color.B))));
        style.Setters.Add(new Setter(BorderBrushProperty, Brushes.Transparent));
        style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(0)));
        style.Setters.Add(new Setter(FontSizeProperty, 10.0));
        style.Setters.Add(new Setter(CursorProperty, Cursors.Hand));

        return style;
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

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableParticleTrail && !IsInSimplifiedMode)
            _particleTimer.Start();
            
        _pulseTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _particleTimer?.Stop();
        _pulseTimer?.Stop();
        
        // Clean up particles
        _particles.Clear();
        _particleCanvas?.Children.Clear();
        
        // Unregister from animation intensity manager
        AnimationIntensityManager.Instance.UnregisterTarget(this);
        
        _disposed = true;
    }

    #endregion

    #region Event Handlers

    private void OnNavigationButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is HoloNavigationItem item)
        {
            NavigateToItem(item);
        }
    }

    private void OnNavigationButtonMouseEnter(object sender, MouseEventArgs e)
    {
        if (!EnableHoverEffects || IsInSimplifiedMode) return;

        if (sender is Button button && button.Tag is HoloNavigationItem item)
        {
            ItemHovered?.Invoke(this, new HoloNavigationEventArgs { Item = item });
            
            // Add hover glow effect
            var color = GetEVEColor(EVEColorScheme);
            button.Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 8,
                ShadowDepth = 0,
                Opacity = 0.6 * HolographicIntensity
            };
        }
    }

    private void OnNavigationButtonMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is Button button)
        {
            button.Effect = null;
        }
    }

    private void OnBreadcrumbButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is HoloNavigationItem item)
        {
            NavigateToItem(item);
        }
    }

    private static void OnNavigationItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloNavigation nav)
        {
            nav.RebuildNavigation();
        }
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloNavigation nav)
        {
            nav.UpdateBreadcrumb();
            if (!nav._isTransitioning)
            {
                nav.AnimateSelectionIndicator();
            }
        }
    }

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloNavigation nav)
            nav.UpdateNavigationAppearance();
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloNavigation nav)
            nav.UpdateNavigationAppearance();
    }

    private static void OnNavigationModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloNavigation nav)
            nav.UpdateNavigationAppearance();
    }

    private static void OnEnableTransitionAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Animation enabling/disabling is handled in real-time
    }

    private static void OnShowBreadcrumbChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloNavigation nav)
            nav.UpdateNavigationAppearance();
    }

    private static void OnEnableParticleTrailChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloNavigation nav)
        {
            if ((bool)e.NewValue && !nav.IsInSimplifiedMode)
                nav._particleTimer.Start();
            else
                nav._particleTimer.Stop();
        }
    }

    #endregion
}

/// <summary>
/// Navigation particle for trail effects
/// </summary>
internal class NavigationParticle
{
    public FrameworkElement Visual { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
}

/// <summary>
/// Navigation item for holographic navigation
/// </summary>
public class HoloNavigationItem : INotifyPropertyChanged
{
    private string _title;
    private string _icon;
    private object _content;
    private HoloNavigationItem _parent;
    private ObservableCollection<HoloNavigationItem> _children;
    private bool _isEnabled = true;

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

    public object Content
    {
        get => _content;
        set { _content = value; OnPropertyChanged(); }
    }

    public HoloNavigationItem Parent
    {
        get => _parent;
        set { _parent = value; OnPropertyChanged(); }
    }

    public ObservableCollection<HoloNavigationItem> Children
    {
        get => _children ??= new ObservableCollection<HoloNavigationItem>();
        set { _children = value; OnPropertyChanged(); }
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set { _isEnabled = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Navigation modes for holographic navigation
/// </summary>
public enum HoloNavigationMode
{
    Horizontal,
    Vertical,
    Sidebar
}

/// <summary>
/// Event args for holographic navigation events
/// </summary>
public class HoloNavigationEventArgs : EventArgs
{
    public HoloNavigationItem Item { get; set; }
    public bool Cancel { get; set; }
}