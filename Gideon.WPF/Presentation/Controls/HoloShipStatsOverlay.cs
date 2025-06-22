// ==========================================================================
// HoloShipStatsOverlay.cs - Holographic Ship Statistics Overlay System
// ==========================================================================
// Advanced ship statistics visualization featuring real-time holographic HUD,
// animated performance metrics, EVE-style tactical displays, and dynamic graphs.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
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
/// Holographic ship statistics overlay with real-time performance metrics and EVE-style HUD
/// </summary>
public class HoloShipStatsOverlay : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloShipStatsOverlay),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloShipStatsOverlay),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty ShipStatsProperty =
        DependencyProperty.Register(nameof(ShipStats), typeof(HoloShipStats), typeof(HoloShipStatsOverlay),
            new PropertyMetadata(null, OnShipStatsChanged));

    public static readonly DependencyProperty OverlayStyleProperty =
        DependencyProperty.Register(nameof(OverlayStyle), typeof(OverlayStyle), typeof(HoloShipStatsOverlay),
            new PropertyMetadata(OverlayStyle.Tactical, OnOverlayStyleChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloShipStatsOverlay),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloShipStatsOverlay),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowRealTimeUpdatesProperty =
        DependencyProperty.Register(nameof(ShowRealTimeUpdates), typeof(bool), typeof(HoloShipStatsOverlay),
            new PropertyMetadata(true, OnShowRealTimeUpdatesChanged));

    public static readonly DependencyProperty ShowGraphicalDisplaysProperty =
        DependencyProperty.Register(nameof(ShowGraphicalDisplays), typeof(bool), typeof(HoloShipStatsOverlay),
            new PropertyMetadata(true, OnShowGraphicalDisplaysChanged));

    public static readonly DependencyProperty AutoHideDelayProperty =
        DependencyProperty.Register(nameof(AutoHideDelay), typeof(TimeSpan), typeof(HoloShipStatsOverlay),
            new PropertyMetadata(TimeSpan.Zero, OnAutoHideDelayChanged));

    public static readonly DependencyProperty CompactModeProperty =
        DependencyProperty.Register(nameof(CompactMode), typeof(bool), typeof(HoloShipStatsOverlay),
            new PropertyMetadata(false, OnCompactModeChanged));

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

    public HoloShipStats ShipStats
    {
        get => (HoloShipStats)GetValue(ShipStatsProperty);
        set => SetValue(ShipStatsProperty, value);
    }

    public OverlayStyle OverlayStyle
    {
        get => (OverlayStyle)GetValue(OverlayStyleProperty);
        set => SetValue(OverlayStyleProperty, value);
    }

    public bool EnableAnimations
    {
        get => (bool)GetValue(EnableAnimationsProperty);
        set => SetValue(EnableAnimationsProperty, value);
    }

    public bool EnableParticleEffects
    {
        get => (bool)GetValue(EnableParticleEffectsProperty);
        set => SetValue(EnableParticleEffectsProperty, value);
    }

    public bool ShowRealTimeUpdates
    {
        get => (bool)GetValue(ShowRealTimeUpdatesProperty);
        set => SetValue(ShowRealTimeUpdatesProperty, value);
    }

    public bool ShowGraphicalDisplays
    {
        get => (bool)GetValue(ShowGraphicalDisplaysProperty);
        set => SetValue(ShowGraphicalDisplaysProperty, value);
    }

    public TimeSpan AutoHideDelay
    {
        get => (TimeSpan)GetValue(AutoHideDelayProperty);
        set => SetValue(AutoHideDelayProperty, value);
    }

    public bool CompactMode
    {
        get => (bool)GetValue(CompactModeProperty);
        set => SetValue(CompactModeProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<StatsEventArgs> StatsUpdated;
    public event EventHandler<StatsEventArgs> StatAlertTriggered;
    public event EventHandler<StatsEventArgs> OverlayVisibilityChanged;

    #endregion

    #region Private Fields

    private readonly DispatcherTimer _animationTimer;
    private readonly DispatcherTimer _updateTimer;
    private readonly DispatcherTimer _autoHideTimer;
    private readonly Dictionary<string, FrameworkElement> _statElements = new();
    private readonly Dictionary<string, ProgressBar> _statBars = new();
    private readonly Dictionary<string, Canvas> _statGraphs = new();
    private readonly List<UIElement> _particleEffects = new();
    private Grid _mainGrid;
    private Canvas _overlayCanvas;
    private Canvas _particleCanvas;
    private Border _backgroundPanel;
    private bool _isSimplifiedMode;
    private double _animationPhase;
    private bool _isVisible = true;
    private readonly Random _random = new();

    #endregion

    #region Constructor

    public HoloShipStatsOverlay()
    {
        InitializeComponent();
        InitializeTimers();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        MouseEnter += OnMouseEnter;
        MouseLeave += OnMouseLeave;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 350;
        Height = 400;
        Background = new SolidColorBrush(Colors.Transparent);
        
        CreateMainLayout();
        ApplyEVEColorScheme();
    }

    private void CreateMainLayout()
    {
        _mainGrid = new Grid();
        Content = _mainGrid;

        // Background panel with glassmorphism effect
        _backgroundPanel = new Border
        {
            Background = CreateGlassmorphismBrush(),
            CornerRadius = new CornerRadius(8),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)),
            BorderThickness = new Thickness(1),
            Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 20,
                ShadowDepth = 5,
                Opacity = 0.3
            }
        };
        _mainGrid.Children.Add(_backgroundPanel);

        // Main overlay canvas
        _overlayCanvas = new Canvas
        {
            Margin = new Thickness(15)
        };
        _mainGrid.Children.Add(_overlayCanvas);

        // Particle effects canvas
        _particleCanvas = new Canvas
        {
            IsHitTestVisible = false
        };
        _mainGrid.Children.Add(_particleCanvas);
    }

    private void InitializeTimers()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
        };
        _animationTimer.Tick += OnAnimationTimerTick;

        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500) // 2 Hz for stats updates
        };
        _updateTimer.Tick += OnUpdateTimerTick;

        _autoHideTimer = new DispatcherTimer();
        _autoHideTimer.Tick += OnAutoHideTimerTick;
    }

    #endregion

    #region Public Methods

    public void UpdateStats(HoloShipStats newStats)
    {
        if (newStats == null) return;

        var oldStats = ShipStats;
        ShipStats = newStats;

        if (oldStats != null)
        {
            AnimateStatChanges(oldStats, newStats);
        }

        RefreshOverlay();

        StatsUpdated?.Invoke(this, new StatsEventArgs
        {
            Stats = newStats,
            Timestamp = DateTime.Now
        });
    }

    public void ShowOverlay()
    {
        if (_isVisible) return;

        _isVisible = true;
        Visibility = Visibility.Visible;

        if (EnableAnimations && !_isSimplifiedMode)
        {
            AnimateOverlayShow();
        }

        ResetAutoHideTimer();

        OverlayVisibilityChanged?.Invoke(this, new StatsEventArgs
        {
            Stats = ShipStats,
            Timestamp = DateTime.Now
        });
    }

    public void HideOverlay()
    {
        if (!_isVisible) return;

        _isVisible = false;

        if (EnableAnimations && !_isSimplifiedMode)
        {
            AnimateOverlayHide(() => Visibility = Visibility.Collapsed);
        }
        else
        {
            Visibility = Visibility.Collapsed;
        }

        OverlayVisibilityChanged?.Invoke(this, new StatsEventArgs
        {
            Stats = ShipStats,
            Timestamp = DateTime.Now
        });
    }

    public void TriggerStatAlert(string statName, AlertLevel level)
    {
        if (string.IsNullOrEmpty(statName)) return;

        if (_statElements.TryGetValue(statName, out var element))
        {
            AnimateStatAlert(element, level);
        }

        StatAlertTriggered?.Invoke(this, new StatsEventArgs
        {
            Stats = ShipStats,
            Timestamp = DateTime.Now
        });
    }

    public void SetOverlayPosition(OverlayPosition position)
    {
        switch (position)
        {
            case OverlayPosition.TopLeft:
                HorizontalAlignment = HorizontalAlignment.Left;
                VerticalAlignment = VerticalAlignment.Top;
                break;
            case OverlayPosition.TopRight:
                HorizontalAlignment = HorizontalAlignment.Right;
                VerticalAlignment = VerticalAlignment.Top;
                break;
            case OverlayPosition.BottomLeft:
                HorizontalAlignment = HorizontalAlignment.Left;
                VerticalAlignment = VerticalAlignment.Bottom;
                break;
            case OverlayPosition.BottomRight:
                HorizontalAlignment = HorizontalAlignment.Right;
                VerticalAlignment = VerticalAlignment.Bottom;
                break;
            case OverlayPosition.Center:
                HorizontalAlignment = HorizontalAlignment.Center;
                VerticalAlignment = VerticalAlignment.Center;
                break;
        }
    }

    #endregion

    #region Private Methods

    private void RefreshOverlay()
    {
        if (ShipStats == null) return;

        _overlayCanvas.Children.Clear();
        _statElements.Clear();
        _statBars.Clear();
        _statGraphs.Clear();

        switch (OverlayStyle)
        {
            case OverlayStyle.Tactical:
                CreateTacticalOverlay();
                break;
            case OverlayStyle.Engineering:
                CreateEngineeringOverlay();
                break;
            case OverlayStyle.Pilot:
                CreatePilotOverlay();
                break;
            case OverlayStyle.Minimal:
                CreateMinimalOverlay();
                break;
        }

        if (EnableParticleEffects && !_isSimplifiedMode)
        {
            SpawnAmbientParticles();
        }
    }

    private void CreateTacticalOverlay()
    {
        var y = 0.0;

        // Ship Name Header
        CreateHeader("TACTICAL OVERVIEW", ref y);

        // Combat Statistics
        CreateStatSection("COMBAT", ref y);
        CreateStatDisplay("DPS", ShipStats.DPS, "damage/s", ref y, StatType.DPS);
        CreateStatDisplay("Alpha", ShipStats.AlphaDamage, "hp", ref y, StatType.Alpha);
        CreateStatDisplay("Range", ShipStats.OptimalRange, "km", ref y, StatType.Range);

        // Defense Statistics
        CreateStatSection("DEFENSE", ref y);
        CreateDefenseDisplay("Shield", ShipStats.ShieldHP, ShipStats.MaxShieldHP, ref y);
        CreateDefenseDisplay("Armor", ShipStats.ArmorHP, ShipStats.MaxArmorHP, ref y);
        CreateDefenseDisplay("Hull", ShipStats.HullHP, ShipStats.MaxHullHP, ref y);

        // Capacitor
        CreateStatSection("POWER", ref y);
        CreateCapacitorDisplay(ref y);

        // Speed and Agility
        CreateStatSection("MOBILITY", ref y);
        CreateStatDisplay("Speed", ShipStats.MaxVelocity, "m/s", ref y, StatType.Speed);
        CreateStatDisplay("Agility", ShipStats.Agility, "s", ref y, StatType.Agility);
    }

    private void CreateEngineeringOverlay()
    {
        var y = 0.0;

        CreateHeader("ENGINEERING", ref y);

        // Power Management
        CreateStatSection("POWER GRID", ref y);
        CreateProgressBarDisplay("Power", ShipStats.PowerUsed, ShipStats.MaxPower, "MW", ref y, StatType.Power);
        CreateProgressBarDisplay("CPU", ShipStats.CPUUsed, ShipStats.MaxCPU, "tf", ref y, StatType.CPU);

        // Thermal Management
        CreateStatSection("THERMAL", ref y);
        CreateStatDisplay("Heat", ShipStats.HeatLevel, "%", ref y, StatType.Heat);
        CreateStatDisplay("Cooling", ShipStats.CoolingRate, "/s", ref y, StatType.Cooling);

        // Efficiency Metrics
        CreateStatSection("EFFICIENCY", ref y);
        CreateStatDisplay("Capacitor", ShipStats.CapacitorStability, "%", ref y, StatType.CapStability);
        CreateStatDisplay("Targeting", ShipStats.ScanResolution, "mm", ref y, StatType.Targeting);
    }

    private void CreatePilotOverlay()
    {
        var y = 0.0;

        CreateHeader("PILOT HUD", ref y);

        // Essential Flight Data
        CreateStatSection("FLIGHT", ref y);
        CreateStatDisplay("Velocity", ShipStats.CurrentVelocity, "m/s", ref y, StatType.Speed);
        CreateStatDisplay("Direction", ShipStats.Heading, "°", ref y, StatType.Heading);
        CreateStatDisplay("Align Time", ShipStats.AlignTime, "s", ref y, StatType.Agility);

        // Combat Readiness
        CreateStatSection("WEAPONS", ref y);
        CreateStatDisplay("DPS", ShipStats.DPS, "dmg/s", ref y, StatType.DPS);
        CreateStatDisplay("Range", ShipStats.OptimalRange, "km", ref y, StatType.Range);

        // Ship Health (Simplified)
        CreateStatSection("STATUS", ref y);
        CreateHealthSummary(ref y);
    }

    private void CreateMinimalOverlay()
    {
        var y = 0.0;

        // Compact display with only critical stats
        CreateStatDisplay("HP", GetTotalHP(), "hp", ref y, StatType.Health, compact: true);
        CreateStatDisplay("CAP", ShipStats.CapacitorCharge, "%", ref y, StatType.Capacitor, compact: true);
        CreateStatDisplay("DPS", ShipStats.DPS, "", ref y, StatType.DPS, compact: true);
        CreateStatDisplay("SPD", ShipStats.CurrentVelocity, "m/s", ref y, StatType.Speed, compact: true);
    }

    private void CreateHeader(string title, ref double y)
    {
        var header = new TextBlock
        {
            Text = title,
            FontSize = CompactMode ? 12 : 14,
            FontWeight = FontWeights.Bold,
            Foreground = GetPrimaryBrush(),
            Effect = CreateTextGlow()
        };

        Canvas.SetLeft(header, 0);
        Canvas.SetTop(header, y);
        _overlayCanvas.Children.Add(header);

        y += CompactMode ? 20 : 25;

        // Add underline
        var underline = new Rectangle
        {
            Width = Width - 30,
            Height = 1,
            Fill = GetPrimaryBrush(),
            Opacity = 0.6
        };

        Canvas.SetLeft(underline, 0);
        Canvas.SetTop(underline, y);
        _overlayCanvas.Children.Add(underline);

        y += 10;
    }

    private void CreateStatSection(string sectionName, ref double y)
    {
        var section = new TextBlock
        {
            Text = sectionName,
            FontSize = CompactMode ? 10 : 11,
            FontWeight = FontWeights.SemiBold,
            Foreground = GetSecondaryBrush(),
            Opacity = 0.8
        };

        Canvas.SetLeft(section, 5);
        Canvas.SetTop(section, y);
        _overlayCanvas.Children.Add(section);

        y += CompactMode ? 15 : 18;
    }

    private void CreateStatDisplay(string name, double value, string unit, ref double y, StatType type, bool compact = false)
    {
        var container = new Grid
        {
            Width = Width - 30,
            Height = compact ? 20 : 25
        };

        // Stat name
        var nameText = new TextBlock
        {
            Text = name,
            FontSize = compact ? 9 : 10,
            Foreground = GetTextBrush(),
            VerticalAlignment = VerticalAlignment.Center
        };
        container.Children.Add(nameText);

        // Stat value
        var valueText = new TextBlock
        {
            Text = FormatStatValue(value, type) + (string.IsNullOrEmpty(unit) ? "" : " " + unit),
            FontSize = compact ? 10 : 11,
            FontWeight = FontWeights.Medium,
            Foreground = GetStatValueBrush(value, type),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Effect = CreateValueGlow(type)
        };
        container.Children.Add(valueText);

        Canvas.SetLeft(container, 10);
        Canvas.SetTop(container, y);
        _overlayCanvas.Children.Add(container);

        _statElements[name] = valueText;

        if (EnableAnimations && !_isSimplifiedMode)
        {
            AnimateStatValue(valueText, type);
        }

        y += compact ? 22 : 28;
    }

    private void CreateProgressBarDisplay(string name, double current, double max, string unit, ref double y, StatType type)
    {
        var container = new Grid
        {
            Width = Width - 30,
            Height = 35
        };

        // Name and values
        var nameText = new TextBlock
        {
            Text = name,
            FontSize = 10,
            Foreground = GetTextBrush(),
            VerticalAlignment = VerticalAlignment.Top
        };
        container.Children.Add(nameText);

        var valueText = new TextBlock
        {
            Text = $"{current:F1}/{max:F1} {unit}",
            FontSize = 9,
            Foreground = GetSecondaryBrush(),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top
        };
        container.Children.Add(valueText);

        // Progress bar
        var progressBar = new ProgressBar
        {
            Width = Width - 30,
            Height = 8,
            Minimum = 0,
            Maximum = max,
            Value = current,
            VerticalAlignment = VerticalAlignment.Bottom,
            Background = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255)),
            Foreground = GetProgressBarBrush(current / max, type),
            BorderThickness = new Thickness(0)
        };

        container.Children.Add(progressBar);

        Canvas.SetLeft(container, 10);
        Canvas.SetTop(container, y);
        _overlayCanvas.Children.Add(container);

        _statBars[name] = progressBar;

        if (EnableAnimations && !_isSimplifiedMode)
        {
            AnimateProgressBar(progressBar);
        }

        y += 40;
    }

    private void CreateDefenseDisplay(string name, double current, double max, ref double y)
    {
        CreateProgressBarDisplay(name, current, max, "HP", ref y, 
            name.ToLower() switch
            {
                "shield" => StatType.Shield,
                "armor" => StatType.Armor,
                "hull" => StatType.Hull,
                _ => StatType.Health
            });
    }

    private void CreateCapacitorDisplay(ref double y)
    {
        var container = new Grid
        {
            Width = Width - 30,
            Height = 50
        };

        // Capacitor ring visualization
        var capacitorRing = CreateCapacitorRing();
        container.Children.Add(capacitorRing);

        // Capacitor stats
        var capText = new TextBlock
        {
            Text = $"CAP: {ShipStats.CapacitorCharge:F0}%",
            FontSize = 11,
            FontWeight = FontWeights.Medium,
            Foreground = GetCapacitorBrush(),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Effect = CreateTextGlow()
        };
        container.Children.Add(capText);

        var stabilityText = new TextBlock
        {
            Text = ShipStats.CapacitorStability > 0 ? $"Stable at {ShipStats.CapacitorStability:F0}%" : "Unstable",
            FontSize = 8,
            Foreground = GetSecondaryBrush(),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom
        };
        container.Children.Add(stabilityText);

        Canvas.SetLeft(container, 10);
        Canvas.SetTop(container, y);
        _overlayCanvas.Children.Add(container);

        y += 55;
    }

    private UIElement CreateCapacitorRing()
    {
        var ringCanvas = new Canvas
        {
            Width = 60,
            Height = 60,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top
        };

        // Background ring
        var backgroundRing = new Ellipse
        {
            Width = 50,
            Height = 50,
            Stroke = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255)),
            StrokeThickness = 4,
            Fill = Brushes.Transparent
        };
        Canvas.SetLeft(backgroundRing, 5);
        Canvas.SetTop(backgroundRing, 5);
        ringCanvas.Children.Add(backgroundRing);

        // Capacitor charge ring
        var chargePercentage = ShipStats.CapacitorCharge / 100.0;
        var chargeRing = CreateArcShape(25, 25, 23, 0, chargePercentage * 360, GetCapacitorBrush(), 4);
        ringCanvas.Children.Add(chargeRing);

        if (EnableAnimations && !_isSimplifiedMode)
        {
            AnimateCapacitorRing(chargeRing);
        }

        return ringCanvas;
    }

    private Path CreateArcShape(double centerX, double centerY, double radius, double startAngle, double endAngle, Brush stroke, double strokeThickness)
    {
        var path = new Path
        {
            Stroke = stroke,
            StrokeThickness = strokeThickness,
            Fill = Brushes.Transparent
        };

        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();

        var startPoint = new Point(
            centerX + radius * Math.Cos(startAngle * Math.PI / 180),
            centerY + radius * Math.Sin(startAngle * Math.PI / 180));

        pathFigure.StartPoint = startPoint;

        var arcSegment = new ArcSegment
        {
            Point = new Point(
                centerX + radius * Math.Cos(endAngle * Math.PI / 180),
                centerY + radius * Math.Sin(endAngle * Math.PI / 180)),
            Size = new Size(radius, radius),
            IsLargeArc = (endAngle - startAngle) > 180,
            SweepDirection = SweepDirection.Clockwise
        };

        pathFigure.Segments.Add(arcSegment);
        pathGeometry.Figures.Add(pathFigure);
        path.Data = pathGeometry;

        return path;
    }

    private void CreateHealthSummary(ref double y)
    {
        var totalHP = GetTotalHP();
        var maxHP = GetMaxTotalHP();
        var healthPercentage = totalHP / maxHP;

        var healthText = new TextBlock
        {
            Text = $"HEALTH: {healthPercentage:P0}",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = GetHealthBrush(healthPercentage),
            Effect = CreateTextGlow()
        };

        Canvas.SetLeft(healthText, 10);
        Canvas.SetTop(healthText, y);
        _overlayCanvas.Children.Add(healthText);

        y += 25;
    }

    #endregion

    #region Animation Methods

    private void AnimateOverlayShow()
    {
        var fadeAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(400),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var scaleTransform = new ScaleTransform(0.8, 0.8);
        RenderTransform = scaleTransform;
        RenderTransformOrigin = new Point(0.5, 0.5);

        var scaleAnimation = new DoubleAnimation
        {
            From = 0.8,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(400),
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
        };

        BeginAnimation(OpacityProperty, fadeAnimation);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
    }

    private void AnimateOverlayHide(Action onComplete)
    {
        var fadeAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        fadeAnimation.Completed += (s, e) => onComplete?.Invoke();
        BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void AnimateStatValue(UIElement element, StatType type)
    {
        if (type == StatType.DPS || type == StatType.Speed || type == StatType.Capacitor)
        {
            var pulseAnimation = new DoubleAnimation
            {
                From = 0.7,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(2),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };
            element.BeginAnimation(OpacityProperty, pulseAnimation);
        }
    }

    private void AnimateProgressBar(ProgressBar progressBar)
    {
        var glowAnimation = new ColorAnimation
        {
            From = ((SolidColorBrush)progressBar.Foreground).Color,
            To = Color.FromArgb(255, 
                Math.Min(255, ((SolidColorBrush)progressBar.Foreground).Color.R + 50),
                Math.Min(255, ((SolidColorBrush)progressBar.Foreground).Color.G + 50),
                Math.Min(255, ((SolidColorBrush)progressBar.Foreground).Color.B + 50)),
            Duration = TimeSpan.FromSeconds(1.5),
            RepeatBehavior = RepeatBehavior.Forever,
            AutoReverse = true
        };

        ((SolidColorBrush)progressBar.Foreground).BeginAnimation(SolidColorBrush.ColorProperty, glowAnimation);
    }

    private void AnimateCapacitorRing(UIElement ring)
    {
        var rotateTransform = new RotateTransform();
        ring.RenderTransform = rotateTransform;
        ring.RenderTransformOrigin = new Point(0.5, 0.5);

        var rotateAnimation = new DoubleAnimation
        {
            From = 0,
            To = 360,
            Duration = TimeSpan.FromSeconds(30),
            RepeatBehavior = RepeatBehavior.Forever
        };

        rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
    }

    private void AnimateStatChanges(HoloShipStats oldStats, HoloShipStats newStats)
    {
        // Animate value changes with color flashes
        AnimateStatChange("DPS", oldStats.DPS, newStats.DPS);
        AnimateStatChange("Shield", oldStats.ShieldHP, newStats.ShieldHP);
        AnimateStatChange("Armor", oldStats.ArmorHP, newStats.ArmorHP);
        AnimateStatChange("Hull", oldStats.HullHP, newStats.HullHP);
    }

    private void AnimateStatChange(string statName, double oldValue, double newValue)
    {
        if (!_statElements.TryGetValue(statName, out var element)) return;

        if (Math.Abs(oldValue - newValue) > 0.01)
        {
            var flashColor = newValue > oldValue ? Colors.Green : Colors.Red;
            var originalBrush = element.Foreground;

            var colorAnimation = new ColorAnimation
            {
                From = flashColor,
                To = ((SolidColorBrush)originalBrush).Color,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            var flashBrush = new SolidColorBrush(flashColor);
            element.Foreground = flashBrush;
            flashBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }
    }

    private void AnimateStatAlert(FrameworkElement element, AlertLevel level)
    {
        var alertColor = level switch
        {
            AlertLevel.Warning => Colors.Yellow,
            AlertLevel.Critical => Colors.Red,
            AlertLevel.Emergency => Colors.Magenta,
            _ => Colors.Orange
        };

        var pulseCount = level switch
        {
            AlertLevel.Critical => 5,
            AlertLevel.Emergency => 8,
            _ => 3
        };

        for (int i = 0; i < pulseCount; i++)
        {
            var pulseAnimation = new ColorAnimation
            {
                From = ((SolidColorBrush)element.Foreground).Color,
                To = alertColor,
                Duration = TimeSpan.FromMilliseconds(150),
                BeginTime = TimeSpan.FromMilliseconds(i * 300),
                AutoReverse = true
            };

            if (element.Foreground is SolidColorBrush brush)
            {
                brush.BeginAnimation(SolidColorBrush.ColorProperty, pulseAnimation);
            }
        }

        if (EnableParticleEffects && !_isSimplifiedMode)
        {
            SpawnAlertParticles(element, level);
        }
    }

    #endregion

    #region Particle Effects

    private void SpawnAmbientParticles()
    {
        if (!EnableParticleEffects || _isSimplifiedMode) return;

        for (int i = 0; i < 5; i++)
        {
            var particle = new Ellipse
            {
                Width = 2,
                Height = 2,
                Fill = new SolidColorBrush(GetPrimaryColor()),
                Opacity = 0.6
            };

            Canvas.SetLeft(particle, _random.NextDouble() * ActualWidth);
            Canvas.SetTop(particle, _random.NextDouble() * ActualHeight);
            _particleCanvas.Children.Add(particle);
            _particleEffects.Add(particle);

            AnimateAmbientParticle(particle);
        }
    }

    private void AnimateAmbientParticle(UIElement particle)
    {
        var moveAnimation = new DoubleAnimation
        {
            From = Canvas.GetTop(particle),
            To = Canvas.GetTop(particle) - 50,
            Duration = TimeSpan.FromSeconds(3 + _random.NextDouble() * 2),
            RepeatBehavior = RepeatBehavior.Forever
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = 0.6,
            To = 0.1,
            Duration = TimeSpan.FromSeconds(2),
            RepeatBehavior = RepeatBehavior.Forever,
            AutoReverse = true
        };

        particle.BeginAnimation(Canvas.TopProperty, moveAnimation);
        particle.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void SpawnAlertParticles(FrameworkElement element, AlertLevel level)
    {
        var particleCount = level switch
        {
            AlertLevel.Critical => 8,
            AlertLevel.Emergency => 12,
            _ => 5
        };

        var alertColor = level switch
        {
            AlertLevel.Warning => Colors.Yellow,
            AlertLevel.Critical => Colors.Red,
            AlertLevel.Emergency => Colors.Magenta,
            _ => Colors.Orange
        };

        var elementPosition = element.TransformToAncestor(_overlayCanvas).Transform(new Point(0, 0));

        for (int i = 0; i < particleCount; i++)
        {
            var particle = new Ellipse
            {
                Width = 3,
                Height = 3,
                Fill = new SolidColorBrush(alertColor)
            };

            Canvas.SetLeft(particle, elementPosition.X + element.ActualWidth / 2);
            Canvas.SetTop(particle, elementPosition.Y + element.ActualHeight / 2);
            _particleCanvas.Children.Add(particle);

            AnimateAlertParticle(particle);
        }
    }

    private void AnimateAlertParticle(UIElement particle)
    {
        var angle = _random.NextDouble() * Math.PI * 2;
        var distance = _random.Next(20, 40);
        var targetX = Canvas.GetLeft(particle) + Math.Cos(angle) * distance;
        var targetY = Canvas.GetTop(particle) + Math.Sin(angle) * distance;

        var moveXAnimation = new DoubleAnimation
        {
            From = Canvas.GetLeft(particle),
            To = targetX,
            Duration = TimeSpan.FromMilliseconds(800),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var moveYAnimation = new DoubleAnimation
        {
            From = Canvas.GetTop(particle),
            To = targetY,
            Duration = TimeSpan.FromMilliseconds(800),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(800)
        };

        moveXAnimation.Completed += (s, e) => _particleCanvas.Children.Remove(particle);

        particle.BeginAnimation(Canvas.LeftProperty, moveXAnimation);
        particle.BeginAnimation(Canvas.TopProperty, moveYAnimation);
        particle.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void CleanupParticleEffects()
    {
        foreach (var particle in _particleEffects)
        {
            _particleCanvas.Children.Remove(particle);
        }
        _particleEffects.Clear();
    }

    #endregion

    #region Helper Methods

    private Brush CreateGlassmorphismBrush()
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        
        return new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 1),
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Color.FromArgb(40, colors.Primary.R, colors.Primary.G, colors.Primary.B), 0),
                new GradientStop(Color.FromArgb(20, colors.Secondary.R, colors.Secondary.G, colors.Secondary.B), 0.5),
                new GradientStop(Color.FromArgb(30, colors.Accent.R, colors.Accent.G, colors.Accent.B), 1)
            }
        };
    }

    private Brush GetPrimaryBrush()
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        return new SolidColorBrush(colors.Primary);
    }

    private Brush GetSecondaryBrush()
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        return new SolidColorBrush(colors.Secondary);
    }

    private Brush GetTextBrush()
    {
        return new SolidColorBrush(Color.FromArgb(200, 255, 255, 255));
    }

    private Brush GetStatValueBrush(double value, StatType type)
    {
        return type switch
        {
            StatType.Health => GetHealthBrush(value / 100),
            StatType.Shield => new SolidColorBrush(Color.FromRgb(100, 150, 255)),
            StatType.Armor => new SolidColorBrush(Color.FromRgb(255, 200, 100)),
            StatType.Hull => new SolidColorBrush(Color.FromRgb(255, 100, 100)),
            StatType.Capacitor => GetCapacitorBrush(),
            StatType.DPS => new SolidColorBrush(Color.FromRgb(255, 100, 100)),
            _ => GetPrimaryBrush()
        };
    }

    private Brush GetProgressBarBrush(double percentage, StatType type)
    {
        if (percentage < 0.25)
            return new SolidColorBrush(Colors.Red);
        else if (percentage < 0.5)
            return new SolidColorBrush(Colors.Orange);
        else if (percentage < 0.75)
            return new SolidColorBrush(Colors.Yellow);
        else
            return new SolidColorBrush(Colors.Green);
    }

    private Brush GetHealthBrush(double healthPercentage)
    {
        if (healthPercentage > 0.75)
            return new SolidColorBrush(Colors.Green);
        else if (healthPercentage > 0.5)
            return new SolidColorBrush(Colors.Yellow);
        else if (healthPercentage > 0.25)
            return new SolidColorBrush(Colors.Orange);
        else
            return new SolidColorBrush(Colors.Red);
    }

    private Brush GetCapacitorBrush()
    {
        var capPercentage = ShipStats?.CapacitorCharge ?? 100;
        if (capPercentage > 75)
            return new SolidColorBrush(Color.FromRgb(100, 200, 255));
        else if (capPercentage > 50)
            return new SolidColorBrush(Color.FromRgb(150, 150, 255));
        else if (capPercentage > 25)
            return new SolidColorBrush(Color.FromRgb(255, 200, 100));
        else
            return new SolidColorBrush(Color.FromRgb(255, 100, 100));
    }

    private Color GetPrimaryColor()
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        return colors.Primary;
    }

    private Effect CreateTextGlow()
    {
        return new DropShadowEffect
        {
            Color = GetPrimaryColor(),
            BlurRadius = 8,
            ShadowDepth = 0,
            Opacity = 0.8
        };
    }

    private Effect CreateValueGlow(StatType type)
    {
        var color = type switch
        {
            StatType.DPS => Colors.Red,
            StatType.Shield => Color.FromRgb(100, 150, 255),
            StatType.Capacitor => Color.FromRgb(100, 200, 255),
            _ => GetPrimaryColor()
        };

        return new DropShadowEffect
        {
            Color = color,
            BlurRadius = 6,
            ShadowDepth = 0,
            Opacity = 0.6
        };
    }

    private string FormatStatValue(double value, StatType type)
    {
        return type switch
        {
            StatType.DPS => value.ToString("F0"),
            StatType.Speed => value.ToString("F0"),
            StatType.Health => value.ToString("F0"),
            StatType.Capacitor => value.ToString("F0"),
            StatType.Agility => value.ToString("F1"),
            StatType.Range => (value / 1000).ToString("F1"), // Convert to km
            _ => value.ToString("F1")
        };
    }

    private double GetTotalHP()
    {
        if (ShipStats == null) return 0;
        return ShipStats.ShieldHP + ShipStats.ArmorHP + ShipStats.HullHP;
    }

    private double GetMaxTotalHP()
    {
        if (ShipStats == null) return 1;
        return ShipStats.MaxShieldHP + ShipStats.MaxArmorHP + ShipStats.MaxHullHP;
    }

    private void ResetAutoHideTimer()
    {
        if (AutoHideDelay == TimeSpan.Zero) return;

        _autoHideTimer.Stop();
        _autoHideTimer.Interval = AutoHideDelay;
        _autoHideTimer.Start();
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableAnimations && !_isSimplifiedMode)
        {
            _animationTimer.Start();
        }

        if (ShowRealTimeUpdates)
        {
            _updateTimer.Start();
        }

        RefreshOverlay();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer.Stop();
        _updateTimer.Stop();
        _autoHideTimer.Stop();
        CleanupParticleEffects();
    }

    private void OnAnimationTimerTick(object sender, EventArgs e)
    {
        if (!EnableAnimations || _isSimplifiedMode) return;

        _animationPhase += 0.1;
        if (_animationPhase > Math.PI * 2)
            _animationPhase = 0;

        // Update any phase-based animations here
    }

    private void OnUpdateTimerTick(object sender, EventArgs e)
    {
        if (!ShowRealTimeUpdates || ShipStats == null) return;

        // Simulate minor stat fluctuations for demonstration
        // In real implementation, this would receive actual ship state updates
        RefreshOverlay();
    }

    private void OnAutoHideTimerTick(object sender, EventArgs e)
    {
        _autoHideTimer.Stop();
        HideOverlay();
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        _autoHideTimer.Stop();
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        ResetAutoHideTimer();
    }

    #endregion

    #region Property Change Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipStatsOverlay overlay)
        {
            overlay.Opacity = (double)e.NewValue;
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipStatsOverlay overlay)
        {
            overlay.ApplyEVEColorScheme();
        }
    }

    private static void OnShipStatsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipStatsOverlay overlay)
        {
            overlay.RefreshOverlay();
        }
    }

    private static void OnOverlayStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipStatsOverlay overlay)
        {
            overlay.RefreshOverlay();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipStatsOverlay overlay)
        {
            if ((bool)e.NewValue && !overlay._isSimplifiedMode)
            {
                overlay._animationTimer.Start();
            }
            else
            {
                overlay._animationTimer.Stop();
            }
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipStatsOverlay overlay && !(bool)e.NewValue)
        {
            overlay.CleanupParticleEffects();
        }
    }

    private static void OnShowRealTimeUpdatesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipStatsOverlay overlay)
        {
            if ((bool)e.NewValue)
            {
                overlay._updateTimer.Start();
            }
            else
            {
                overlay._updateTimer.Stop();
            }
        }
    }

    private static void OnShowGraphicalDisplaysChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipStatsOverlay overlay)
        {
            overlay.RefreshOverlay();
        }
    }

    private static void OnAutoHideDelayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipStatsOverlay overlay)
        {
            overlay.ResetAutoHideTimer();
        }
    }

    private static void OnCompactModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipStatsOverlay overlay)
        {
            overlay.RefreshOverlay();
        }
    }

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode => _isSimplifiedMode;

    public void EnterSimplifiedMode()
    {
        _isSimplifiedMode = true;
        EnableAnimations = false;
        EnableParticleEffects = false;
        CompactMode = true;
        _animationTimer.Stop();
        CleanupParticleEffects();
    }

    public void ExitSimplifiedMode()
    {
        _isSimplifiedMode = false;
        EnableAnimations = true;
        EnableParticleEffects = true;
        CompactMode = false;
        _animationTimer.Start();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => true;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity;
        
        if (!settings.EnabledFeatures.HasFlag(AnimationFeatures.ParticleEffects))
        {
            EnableParticleEffects = false;
        }
        
        if (!settings.EnabledFeatures.HasFlag(AnimationFeatures.UIAnimations))
        {
            EnableAnimations = false;
        }

        if (settings.PerformanceMode == PerformanceMode.PowerSaver)
        {
            EnterSimplifiedMode();
        }
        else
        {
            ExitSimplifiedMode();
        }
    }

    #endregion

    #region Color Scheme Application

    private void ApplyEVEColorScheme()
    {
        if (_backgroundPanel != null)
        {
            _backgroundPanel.Background = CreateGlassmorphismBrush();
        }

        RefreshOverlay();
    }

    #endregion
}

#region Supporting Classes and Enums

public class HoloShipStats
{
    // Combat Stats
    public double DPS { get; set; }
    public double AlphaDamage { get; set; }
    public double OptimalRange { get; set; }
    public double FalloffRange { get; set; }

    // Defense Stats
    public double ShieldHP { get; set; }
    public double MaxShieldHP { get; set; }
    public double ArmorHP { get; set; }
    public double MaxArmorHP { get; set; }
    public double HullHP { get; set; }
    public double MaxHullHP { get; set; }

    // Capacitor
    public double CapacitorCharge { get; set; }
    public double MaxCapacitor { get; set; }
    public double CapacitorStability { get; set; }

    // Mobility
    public double MaxVelocity { get; set; }
    public double CurrentVelocity { get; set; }
    public double Agility { get; set; }
    public double AlignTime { get; set; }
    public double Heading { get; set; }

    // Power and CPU
    public double PowerUsed { get; set; }
    public double MaxPower { get; set; }
    public double CPUUsed { get; set; }
    public double MaxCPU { get; set; }

    // Targeting
    public double ScanResolution { get; set; }
    public double TargetingRange { get; set; }
    public int MaxTargets { get; set; }

    // Engineering
    public double HeatLevel { get; set; }
    public double CoolingRate { get; set; }
    public double SignatureRadius { get; set; }

    // Additional metadata
    public DateTime LastUpdated { get; set; } = DateTime.Now;
    public Dictionary<string, object> CustomStats { get; set; } = new();
}

public enum OverlayStyle
{
    Tactical,
    Engineering,
    Pilot,
    Minimal
}

public enum OverlayPosition
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    Center
}

public enum StatType
{
    Health,
    Shield,
    Armor,
    Hull,
    Capacitor,
    DPS,
    Alpha,
    Range,
    Speed,
    Agility,
    Power,
    CPU,
    Heat,
    Cooling,
    CapStability,
    Targeting,
    Heading
}

public enum AlertLevel
{
    Info,
    Warning,
    Critical,
    Emergency
}

public class StatsEventArgs : EventArgs
{
    public HoloShipStats Stats { get; set; }
    public DateTime Timestamp { get; set; }
}

#endregion