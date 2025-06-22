// ==========================================================================
// HoloResponsiveLayout.cs - Responsive Holographic Layout with Adaptive Panels
// ==========================================================================
// Advanced responsive layout system that adapts to screen size and performance,
// providing optimal holographic UI arrangement for different scenarios.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Responsive holographic layout that adapts to screen size and performance
/// </summary>
public class HoloResponsiveLayout : Panel, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty LayoutModeProperty =
        DependencyProperty.Register(nameof(LayoutMode), typeof(HoloLayoutMode), typeof(HoloResponsiveLayout),
            new PropertyMetadata(HoloLayoutMode.Adaptive, OnLayoutModeChanged));

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloResponsiveLayout),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloResponsiveLayout),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty EnableLayoutAnimationsProperty =
        DependencyProperty.Register(nameof(EnableLayoutAnimations), typeof(bool), typeof(HoloResponsiveLayout),
            new PropertyMetadata(true, OnEnableLayoutAnimationsChanged));

    public static readonly DependencyProperty AdaptiveBreakpointProperty =
        DependencyProperty.Register(nameof(AdaptiveBreakpoint), typeof(double), typeof(HoloResponsiveLayout),
            new PropertyMetadata(1200.0));

    public static readonly DependencyProperty CompactBreakpointProperty =
        DependencyProperty.Register(nameof(CompactBreakpoint), typeof(double), typeof(HoloResponsiveLayout),
            new PropertyMetadata(800.0));

    public static readonly DependencyProperty MinimumPanelWidthProperty =
        DependencyProperty.Register(nameof(MinimumPanelWidth), typeof(double), typeof(HoloResponsiveLayout),
            new PropertyMetadata(250.0));

    public static readonly DependencyProperty PanelSpacingProperty =
        DependencyProperty.Register(nameof(PanelSpacing), typeof(double), typeof(HoloResponsiveLayout),
            new PropertyMetadata(8.0));

    public static readonly DependencyProperty EnableParallaxEffectProperty =
        DependencyProperty.Register(nameof(EnableParallaxEffect), typeof(bool), typeof(HoloResponsiveLayout),
            new PropertyMetadata(true, OnEnableParallaxEffectChanged));

    public HoloLayoutMode LayoutMode
    {
        get => (HoloLayoutMode)GetValue(LayoutModeProperty);
        set => SetValue(LayoutModeProperty, value);
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

    public bool EnableLayoutAnimations
    {
        get => (bool)GetValue(EnableLayoutAnimationsProperty);
        set => SetValue(EnableLayoutAnimationsProperty, value);
    }

    public double AdaptiveBreakpoint
    {
        get => (double)GetValue(AdaptiveBreakpointProperty);
        set => SetValue(AdaptiveBreakpointProperty, value);
    }

    public double CompactBreakpoint
    {
        get => (double)GetValue(CompactBreakpointProperty);
        set => SetValue(CompactBreakpointProperty, value);
    }

    public double MinimumPanelWidth
    {
        get => (double)GetValue(MinimumPanelWidthProperty);
        set => SetValue(MinimumPanelWidthProperty, value);
    }

    public double PanelSpacing
    {
        get => (double)GetValue(PanelSpacingProperty);
        set => SetValue(PanelSpacingProperty, value);
    }

    public bool EnableParallaxEffect
    {
        get => (bool)GetValue(EnableParallaxEffectProperty);
        set => SetValue(EnableParallaxEffectProperty, value);
    }

    #endregion

    #region Attached Properties

    public static readonly DependencyProperty PanelPriorityProperty =
        DependencyProperty.RegisterAttached("PanelPriority", typeof(int), typeof(HoloResponsiveLayout),
            new PropertyMetadata(0, OnPanelPriorityChanged));

    public static readonly DependencyProperty PanelFlexProperty =
        DependencyProperty.RegisterAttached("PanelFlex", typeof(double), typeof(HoloResponsiveLayout),
            new PropertyMetadata(1.0, OnPanelFlexChanged));

    public static readonly DependencyProperty PanelDepthProperty =
        DependencyProperty.RegisterAttached("PanelDepth", typeof(double), typeof(HoloResponsiveLayout),
            new PropertyMetadata(0.0, OnPanelDepthChanged));

    public static readonly DependencyProperty CollapsibleProperty =
        DependencyProperty.RegisterAttached("Collapsible", typeof(bool), typeof(HoloResponsiveLayout),
            new PropertyMetadata(false));

    public static int GetPanelPriority(DependencyObject obj) => (int)obj.GetValue(PanelPriorityProperty);
    public static void SetPanelPriority(DependencyObject obj, int value) => obj.SetValue(PanelPriorityProperty, value);

    public static double GetPanelFlex(DependencyObject obj) => (double)obj.GetValue(PanelFlexProperty);
    public static void SetPanelFlex(DependencyObject obj, double value) => obj.SetValue(PanelFlexProperty, value);

    public static double GetPanelDepth(DependencyObject obj) => (double)obj.GetValue(PanelDepthProperty);
    public static void SetPanelDepth(DependencyObject obj, double value) => obj.SetValue(PanelDepthProperty, value);

    public static bool GetCollapsible(DependencyObject obj) => (bool)obj.GetValue(CollapsibleProperty);
    public static void SetCollapsible(DependencyObject obj, bool value) => obj.SetValue(CollapsibleProperty, value);

    #endregion

    #region Events

    public event EventHandler<HoloLayoutEventArgs> LayoutModeChanged;
    public event EventHandler<HoloLayoutEventArgs> PanelCollapsed;
    public event EventHandler<HoloLayoutEventArgs> PanelExpanded;

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode { get; private set; }

    public void SwitchToFullMode()
    {
        IsInSimplifiedMode = false;
        EnableLayoutAnimations = true;
        EnableParallaxEffect = true;
        UpdateLayoutMode();
    }

    public void SwitchToSimplifiedMode()
    {
        IsInSimplifiedMode = true;
        EnableLayoutAnimations = false;
        EnableParallaxEffect = false;
        LayoutMode = HoloLayoutMode.Compact;
        UpdateLayoutMode();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => !_disposed && IsLoaded;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity;
        EnableLayoutAnimations = settings.EnabledFeatures.HasFlag(AnimationFeatures.ComplexTransitions);
        EnableParallaxEffect = settings.EnabledFeatures.HasFlag(AnimationFeatures.AdvancedShaders);
        
        UpdateLayoutEffects();
    }

    #endregion

    #region Fields

    private readonly Dictionary<UIElement, PanelLayoutInfo> _panelInfo = new();
    private DispatcherTimer _layoutTimer;
    private DispatcherTimer _parallaxTimer;
    private HoloLayoutMode _currentLayoutMode;
    private double _parallaxOffset = 0;
    private bool _disposed = false;

    #endregion

    #region Constructor

    public HoloResponsiveLayout()
    {
        _currentLayoutMode = LayoutMode;
        SetupLayoutSystem();
        
        // Register with animation intensity manager
        AnimationIntensityManager.Instance.RegisterTarget(this);
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Force layout recalculation
    /// </summary>
    public void RefreshLayout()
    {
        InvalidateArrange();
        InvalidateMeasure();
    }

    /// <summary>
    /// Collapse a specific panel
    /// </summary>
    public void CollapsePanel(UIElement panel)
    {
        if (panel == null || !Children.Contains(panel)) return;

        if (GetCollapsible(panel))
        {
            if (EnableLayoutAnimations && !IsInSimplifiedMode)
            {
                AnimatePanelCollapse(panel);
            }
            else
            {
                panel.Visibility = Visibility.Collapsed;
            }
            
            PanelCollapsed?.Invoke(this, new HoloLayoutEventArgs { Panel = panel });
        }
    }

    /// <summary>
    /// Expand a specific panel
    /// </summary>
    public void ExpandPanel(UIElement panel)
    {
        if (panel == null || !Children.Contains(panel)) return;

        if (EnableLayoutAnimations && !IsInSimplifiedMode)
        {
            AnimatePanelExpand(panel);
        }
        else
        {
            panel.Visibility = Visibility.Visible;
        }
        
        PanelExpanded?.Invoke(this, new HoloLayoutEventArgs { Panel = panel });
    }

    #endregion

    #region Layout Overrides

    protected override Size MeasureOverride(Size availableSize)
    {
        var desiredSize = new Size();
        var currentMode = DetermineLayoutMode(availableSize.Width);
        
        foreach (UIElement child in Children)
        {
            if (child.Visibility == Visibility.Collapsed) continue;
            
            var availableChildSize = CalculateChildMeasureSize(child, availableSize, currentMode);
            child.Measure(availableChildSize);
            
            UpdatePanelInfo(child, child.DesiredSize);
        }

        desiredSize = CalculateDesiredSize(availableSize, currentMode);
        return desiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var currentMode = DetermineLayoutMode(finalSize.Width);
        
        if (currentMode != _currentLayoutMode)
        {
            _currentLayoutMode = currentMode;
            LayoutModeChanged?.Invoke(this, new HoloLayoutEventArgs { LayoutMode = currentMode });
        }

        switch (currentMode)
        {
            case HoloLayoutMode.Wide:
                ArrangeWideLayout(finalSize);
                break;
            case HoloLayoutMode.Standard:
                ArrangeStandardLayout(finalSize);
                break;
            case HoloLayoutMode.Compact:
                ArrangeCompactLayout(finalSize);
                break;
            case HoloLayoutMode.Minimal:
                ArrangeMinimalLayout(finalSize);
                break;
            default:
                ArrangeAdaptiveLayout(finalSize);
                break;
        }

        UpdateParallaxEffects();
        return finalSize;
    }

    #endregion

    #region Private Methods

    private void SetupLayoutSystem()
    {
        // Layout update timer for smooth transitions
        _layoutTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        _layoutTimer.Tick += OnLayoutTick;

        // Parallax effect timer
        _parallaxTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
        };
        _parallaxTimer.Tick += OnParallaxTick;
    }

    private HoloLayoutMode DetermineLayoutMode(double availableWidth)
    {
        if (LayoutMode != HoloLayoutMode.Adaptive)
            return LayoutMode;

        if (availableWidth >= AdaptiveBreakpoint)
            return HoloLayoutMode.Wide;
        else if (availableWidth >= CompactBreakpoint)
            return HoloLayoutMode.Standard;
        else if (availableWidth >= MinimumPanelWidth * 2)
            return HoloLayoutMode.Compact;
        else
            return HoloLayoutMode.Minimal;
    }

    private Size CalculateChildMeasureSize(UIElement child, Size availableSize, HoloLayoutMode mode)
    {
        var flex = GetPanelFlex(child);
        var priority = GetPanelPriority(child);
        
        return mode switch
        {
            HoloLayoutMode.Wide => new Size(availableSize.Width * flex / 3, availableSize.Height),
            HoloLayoutMode.Standard => new Size(availableSize.Width * flex / 2, availableSize.Height),
            HoloLayoutMode.Compact => new Size(availableSize.Width * flex, availableSize.Height),
            HoloLayoutMode.Minimal => new Size(availableSize.Width, availableSize.Height / Children.Count),
            _ => new Size(availableSize.Width * flex, availableSize.Height)
        };
    }

    private void UpdatePanelInfo(UIElement child, Size desiredSize)
    {
        if (!_panelInfo.ContainsKey(child))
        {
            _panelInfo[child] = new PanelLayoutInfo();
        }
        
        var info = _panelInfo[child];
        info.DesiredSize = desiredSize;
        info.Priority = GetPanelPriority(child);
        info.Flex = GetPanelFlex(child);
        info.Depth = GetPanelDepth(child);
        info.IsCollapsible = GetCollapsible(child);
    }

    private Size CalculateDesiredSize(Size availableSize, HoloLayoutMode mode)
    {
        var visibleChildren = Children.Cast<UIElement>()
            .Where(c => c.Visibility != Visibility.Collapsed).ToList();
        
        if (!visibleChildren.Any())
            return new Size();

        return mode switch
        {
            HoloLayoutMode.Wide or HoloLayoutMode.Standard => 
                new Size(availableSize.Width, visibleChildren.Max(c => c.DesiredSize.Height)),
            HoloLayoutMode.Compact => 
                new Size(availableSize.Width, visibleChildren.Max(c => c.DesiredSize.Height)),
            HoloLayoutMode.Minimal => 
                new Size(availableSize.Width, visibleChildren.Sum(c => c.DesiredSize.Height)),
            _ => availableSize
        };
    }

    private void ArrangeWideLayout(Size finalSize)
    {
        var visibleChildren = GetVisibleChildrenByPriority();
        var totalFlex = visibleChildren.Sum(c => GetPanelFlex(c));
        var usedWidth = 0.0;

        foreach (var child in visibleChildren)
        {
            var flex = GetPanelFlex(child);
            var width = (finalSize.Width - (visibleChildren.Count - 1) * PanelSpacing) * flex / totalFlex;
            var depth = GetPanelDepth(child);
            
            var rect = new Rect(usedWidth, depth, width, finalSize.Height);
            ArrangeChildWithEffects(child, rect);
            
            usedWidth += width + PanelSpacing;
        }
    }

    private void ArrangeStandardLayout(Size finalSize)
    {
        var visibleChildren = GetVisibleChildrenByPriority();
        var primaryChildren = visibleChildren.Take(2).ToList();
        var totalFlex = primaryChildren.Sum(c => GetPanelFlex(c));
        var usedWidth = 0.0;

        foreach (var child in primaryChildren)
        {
            var flex = GetPanelFlex(child);
            var width = (finalSize.Width - PanelSpacing) * flex / totalFlex;
            var depth = GetPanelDepth(child);
            
            var rect = new Rect(usedWidth, depth, width, finalSize.Height);
            ArrangeChildWithEffects(child, rect);
            
            usedWidth += width + PanelSpacing;
        }

        // Collapse remaining panels
        foreach (var child in visibleChildren.Skip(2))
        {
            if (GetCollapsible(child))
            {
                child.Arrange(new Rect(0, 0, 0, 0));
            }
        }
    }

    private void ArrangeCompactLayout(Size finalSize)
    {
        var visibleChildren = GetVisibleChildrenByPriority();
        var primaryChild = visibleChildren.FirstOrDefault();
        
        if (primaryChild != null)
        {
            var depth = GetPanelDepth(primaryChild);
            var rect = new Rect(0, depth, finalSize.Width, finalSize.Height);
            ArrangeChildWithEffects(primaryChild, rect);
        }

        // Collapse all other panels
        foreach (var child in visibleChildren.Skip(1))
        {
            if (GetCollapsible(child))
            {
                child.Arrange(new Rect(0, 0, 0, 0));
            }
        }
    }

    private void ArrangeMinimalLayout(Size finalSize)
    {
        var visibleChildren = GetVisibleChildrenByPriority();
        var usedHeight = 0.0;
        var itemHeight = finalSize.Height / visibleChildren.Count;

        foreach (var child in visibleChildren)
        {
            var depth = GetPanelDepth(child);
            var rect = new Rect(0, usedHeight + depth, finalSize.Width, itemHeight);
            ArrangeChildWithEffects(child, rect);
            
            usedHeight += itemHeight;
        }
    }

    private void ArrangeAdaptiveLayout(Size finalSize)
    {
        // Default to standard layout for adaptive mode
        ArrangeStandardLayout(finalSize);
    }

    private void ArrangeChildWithEffects(UIElement child, Rect finalRect)
    {
        child.Arrange(finalRect);
        
        if (EnableParallaxEffect && !IsInSimplifiedMode)
        {
            ApplyParallaxToChild(child, finalRect);
        }
    }

    private void ApplyParallaxToChild(UIElement child, Rect finalRect)
    {
        var depth = GetPanelDepth(child);
        var parallaxOffset = _parallaxOffset * depth * 0.1; // Subtle parallax effect
        
        var transform = child.RenderTransform as TranslateTransform ?? new TranslateTransform();
        transform.X = parallaxOffset;
        child.RenderTransform = transform;
    }

    private List<UIElement> GetVisibleChildrenByPriority()
    {
        return Children.Cast<UIElement>()
            .Where(c => c.Visibility != Visibility.Collapsed)
            .OrderByDescending(c => GetPanelPriority(c))
            .ToList();
    }

    private void AnimatePanelCollapse(UIElement panel)
    {
        var collapseAnimation = new DoubleAnimation
        {
            From = panel.ActualWidth,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        collapseAnimation.Completed += (s, e) =>
        {
            panel.Visibility = Visibility.Collapsed;
            RefreshLayout();
        };

        panel.BeginAnimation(WidthProperty, collapseAnimation);
    }

    private void AnimatePanelExpand(UIElement panel)
    {
        panel.Visibility = Visibility.Visible;
        panel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var targetWidth = panel.DesiredSize.Width;

        var expandAnimation = new DoubleAnimation
        {
            From = 0,
            To = targetWidth,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        expandAnimation.Completed += (s, e) =>
        {
            panel.BeginAnimation(WidthProperty, null);
            RefreshLayout();
        };

        panel.BeginAnimation(WidthProperty, expandAnimation);
    }

    private void UpdateLayoutMode()
    {
        InvalidateArrange();
        InvalidateMeasure();
    }

    private void UpdateLayoutEffects()
    {
        foreach (UIElement child in Children)
        {
            if (child is IAnimationIntensityTarget target)
            {
                var settings = new AnimationIntensitySettings
                {
                    MasterIntensity = HolographicIntensity,
                    EnabledFeatures = IsInSimplifiedMode ? 
                        AnimationFeatures.None : 
                        AnimationFeatures.BasicGlow | AnimationFeatures.ComplexTransitions
                };
                target.ApplyIntensitySettings(settings);
            }
        }
    }

    private void UpdateParallaxEffects()
    {
        if (!EnableParallaxEffect || IsInSimplifiedMode) return;

        foreach (UIElement child in Children)
        {
            if (child.Visibility != Visibility.Collapsed)
            {
                var rect = new Rect(child.RenderSize);
                ApplyParallaxToChild(child, rect);
            }
        }
    }

    private void OnLayoutTick(object sender, EventArgs e)
    {
        // Smooth layout transitions
        if (EnableLayoutAnimations && !IsInSimplifiedMode)
        {
            // Update any ongoing layout animations
        }
    }

    private void OnParallaxTick(object sender, EventArgs e)
    {
        if (!EnableParallaxEffect || IsInSimplifiedMode) return;

        _parallaxOffset += 0.02;
        if (_parallaxOffset > Math.PI * 2)
            _parallaxOffset = 0;

        UpdateParallaxEffects();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        RefreshLayout();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableLayoutAnimations && !IsInSimplifiedMode)
            _layoutTimer.Start();
            
        if (EnableParallaxEffect && !IsInSimplifiedMode)
            _parallaxTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _layoutTimer?.Stop();
        _parallaxTimer?.Stop();
        
        // Unregister from animation intensity manager
        AnimationIntensityManager.Instance.UnregisterTarget(this);
        
        _disposed = true;
    }

    #endregion

    #region Event Handlers

    private static void OnLayoutModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloResponsiveLayout layout)
            layout.UpdateLayoutMode();
    }

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloResponsiveLayout layout)
            layout.UpdateLayoutEffects();
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloResponsiveLayout layout)
            layout.UpdateLayoutEffects();
    }

    private static void OnEnableLayoutAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloResponsiveLayout layout)
        {
            if ((bool)e.NewValue && !layout.IsInSimplifiedMode)
                layout._layoutTimer.Start();
            else
                layout._layoutTimer.Stop();
        }
    }

    private static void OnEnableParallaxEffectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloResponsiveLayout layout)
        {
            if ((bool)e.NewValue && !layout.IsInSimplifiedMode)
                layout._parallaxTimer.Start();
            else
                layout._parallaxTimer.Stop();
        }
    }

    private static void OnPanelPriorityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element && VisualTreeHelper.GetParent(element) is HoloResponsiveLayout layout)
            layout.RefreshLayout();
    }

    private static void OnPanelFlexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element && VisualTreeHelper.GetParent(element) is HoloResponsiveLayout layout)
            layout.RefreshLayout();
    }

    private static void OnPanelDepthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element && VisualTreeHelper.GetParent(element) is HoloResponsiveLayout layout)
            layout.RefreshLayout();
    }

    #endregion
}

/// <summary>
/// Panel layout information for responsive system
/// </summary>
internal class PanelLayoutInfo
{
    public Size DesiredSize { get; set; }
    public int Priority { get; set; }
    public double Flex { get; set; }
    public double Depth { get; set; }
    public bool IsCollapsible { get; set; }
}

/// <summary>
/// Layout modes for holographic responsive layout
/// </summary>
public enum HoloLayoutMode
{
    Adaptive,
    Wide,
    Standard,
    Compact,
    Minimal
}

/// <summary>
/// Event args for holographic layout events
/// </summary>
public class HoloLayoutEventArgs : EventArgs
{
    public UIElement Panel { get; set; }
    public HoloLayoutMode LayoutMode { get; set; }
}