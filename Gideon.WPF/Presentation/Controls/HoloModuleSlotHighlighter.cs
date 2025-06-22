// ==========================================================================
// HoloModuleSlotHighlighter.cs - Holographic Module Slot Highlighting System
// ==========================================================================
// Advanced module slot highlighting featuring intelligent highlighting patterns,
// compatibility indicators, animated pulse effects, and EVE-style visual feedback.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Holographic module slot highlighting system with intelligent compatibility indicators and animated effects
/// </summary>
public class HoloModuleSlotHighlighter : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloModuleSlotHighlighter),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloModuleSlotHighlighter),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty ModuleSlotsProperty =
        DependencyProperty.Register(nameof(ModuleSlots), typeof(List<HoloModuleSlot>), typeof(HoloModuleSlotHighlighter),
            new PropertyMetadata(null, OnModuleSlotsChanged));

    public static readonly DependencyProperty HighlightModeProperty =
        DependencyProperty.Register(nameof(HighlightMode), typeof(HighlightMode), typeof(HoloModuleSlotHighlighter),
            new PropertyMetadata(HighlightMode.Pulse, OnHighlightModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloModuleSlotHighlighter),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloModuleSlotHighlighter),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowCompatibilityIndicatorsProperty =
        DependencyProperty.Register(nameof(ShowCompatibilityIndicators), typeof(bool), typeof(HoloModuleSlotHighlighter),
            new PropertyMetadata(true, OnShowCompatibilityIndicatorsChanged));

    public static readonly DependencyProperty ShowSlotNamesProperty =
        DependencyProperty.Register(nameof(ShowSlotNames), typeof(bool), typeof(HoloModuleSlotHighlighter),
            new PropertyMetadata(false, OnShowSlotNamesChanged));

    public static readonly DependencyProperty HighlightDurationProperty =
        DependencyProperty.Register(nameof(HighlightDuration), typeof(TimeSpan), typeof(HoloModuleSlotHighlighter),
            new PropertyMetadata(TimeSpan.FromSeconds(2), OnHighlightDurationChanged));

    public static readonly DependencyProperty AutoHighlightEmptySlotsProperty =
        DependencyProperty.Register(nameof(AutoHighlightEmptySlots), typeof(bool), typeof(HoloModuleSlotHighlighter),
            new PropertyMetadata(false, OnAutoHighlightEmptySlotsChanged));

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

    public List<HoloModuleSlot> ModuleSlots
    {
        get => (List<HoloModuleSlot>)GetValue(ModuleSlotsProperty);
        set => SetValue(ModuleSlotsProperty, value);
    }

    public HighlightMode HighlightMode
    {
        get => (HighlightMode)GetValue(HighlightModeProperty);
        set => SetValue(HighlightModeProperty, value);
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

    public bool ShowCompatibilityIndicators
    {
        get => (bool)GetValue(ShowCompatibilityIndicatorsProperty);
        set => SetValue(ShowCompatibilityIndicatorsProperty, value);
    }

    public bool ShowSlotNames
    {
        get => (bool)GetValue(ShowSlotNamesProperty);
        set => SetValue(ShowSlotNamesProperty, value);
    }

    public TimeSpan HighlightDuration
    {
        get => (TimeSpan)GetValue(HighlightDurationProperty);
        set => SetValue(HighlightDurationProperty, value);
    }

    public bool AutoHighlightEmptySlots
    {
        get => (bool)GetValue(AutoHighlightEmptySlotsProperty);
        set => SetValue(AutoHighlightEmptySlotsProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<SlotHighlightEventArgs> SlotHighlighted;
    public event EventHandler<SlotHighlightEventArgs> SlotUnhighlighted;
    public event EventHandler<SlotHighlightEventArgs> SlotClicked;
    public event EventHandler<SlotHighlightEventArgs> SlotHovered;
    public event EventHandler<SlotHighlightEventArgs> CompatibilityChecked;

    #endregion

    #region Private Fields

    private readonly DispatcherTimer _animationTimer;
    private readonly DispatcherTimer _highlightTimer;
    private readonly Dictionary<string, HighlightState> _slotStates = new();
    private readonly Dictionary<string, FrameworkElement> _slotElements = new();
    private readonly Dictionary<string, Canvas> _slotParticles = new();
    private readonly List<UIElement> _particlePool = new();
    private Canvas _mainCanvas;
    private bool _isSimplifiedMode;
    private double _animationPhase;
    private string _draggedModuleType;
    private readonly Random _random = new();

    #endregion

    #region Constructor

    public HoloModuleSlotHighlighter()
    {
        InitializeComponent();
        InitializeTimers();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 400;
        Height = 300;
        Background = new SolidColorBrush(Colors.Transparent);
        
        _mainCanvas = new Canvas
        {
            Width = Width,
            Height = Height,
            ClipToBounds = true
        };
        Content = _mainCanvas;

        ApplyEVEColorScheme();
    }

    private void InitializeTimers()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
        };
        _animationTimer.Tick += OnAnimationTimerTick;

        _highlightTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _highlightTimer.Tick += OnHighlightTimerTick;
    }

    #endregion

    #region Public Methods

    public void HighlightSlot(string slotId, HighlightType type = HighlightType.Standard, TimeSpan? duration = null)
    {
        if (string.IsNullOrEmpty(slotId)) return;

        var slot = ModuleSlots?.FirstOrDefault(s => s.Id == slotId);
        if (slot == null) return;

        var highlightDuration = duration ?? HighlightDuration;
        
        if (!_slotStates.ContainsKey(slotId))
        {
            _slotStates[slotId] = new HighlightState();
        }

        var state = _slotStates[slotId];
        state.IsHighlighted = true;
        state.HighlightType = type;
        state.StartTime = DateTime.Now;
        state.Duration = highlightDuration;
        state.Intensity = HolographicIntensity;

        CreateOrUpdateSlotElement(slot, state);
        
        if (EnableParticleEffects && !_isSimplifiedMode)
        {
            SpawnHighlightParticles(slotId, type);
        }

        SlotHighlighted?.Invoke(this, new SlotHighlightEventArgs
        {
            SlotId = slotId,
            HighlightType = type,
            Timestamp = DateTime.Now
        });
    }

    public void UnhighlightSlot(string slotId)
    {
        if (string.IsNullOrEmpty(slotId) || !_slotStates.ContainsKey(slotId)) return;

        var state = _slotStates[slotId];
        state.IsHighlighted = false;
        
        if (_slotElements.TryGetValue(slotId, out var element))
        {
            AnimateSlotUnhighlight(element, () =>
            {
                _mainCanvas.Children.Remove(element);
                _slotElements.Remove(slotId);
            });
        }

        CleanupSlotParticles(slotId);

        SlotUnhighlighted?.Invoke(this, new SlotHighlightEventArgs
        {
            SlotId = slotId,
            Timestamp = DateTime.Now
        });
    }

    public void HighlightSlotsByType(ModuleSlotType slotType, HighlightType highlightType = HighlightType.Standard)
    {
        if (ModuleSlots == null) return;

        var slotsOfType = ModuleSlots.Where(s => s.Type == slotType);
        foreach (var slot in slotsOfType)
        {
            HighlightSlot(slot.Id, highlightType);
        }
    }

    public void HighlightCompatibleSlots(string moduleType, HighlightType highlightType = HighlightType.Compatible)
    {
        if (ModuleSlots == null || string.IsNullOrEmpty(moduleType)) return;

        _draggedModuleType = moduleType;

        foreach (var slot in ModuleSlots)
        {
            if (IsSlotCompatible(slot, moduleType))
            {
                HighlightSlot(slot.Id, highlightType);
            }
            else if (ShowCompatibilityIndicators)
            {
                HighlightSlot(slot.Id, HighlightType.Incompatible);
            }
        }
    }

    public void ClearAllHighlights()
    {
        var slotIds = _slotStates.Keys.ToList();
        foreach (var slotId in slotIds)
        {
            UnhighlightSlot(slotId);
        }
        
        _draggedModuleType = null;
    }

    public void UpdateSlotPositions(List<HoloModuleSlot> updatedSlots)
    {
        ModuleSlots = updatedSlots;
        RefreshSlotElements();
    }

    public void PulseSlot(string slotId, int pulseCount = 3)
    {
        if (!_slotStates.ContainsKey(slotId)) return;

        var state = _slotStates[slotId];
        state.PulseCount = pulseCount;
        state.CurrentPulse = 0;
        state.IsPulsing = true;
    }

    #endregion

    #region Private Methods

    private void CreateOrUpdateSlotElement(HoloModuleSlot slot, HighlightState state)
    {
        if (_slotElements.TryGetValue(slot.Id, out var existingElement))
        {
            UpdateSlotElement(existingElement, slot, state);
            return;
        }

        var slotElement = CreateSlotElement(slot, state);
        _slotElements[slot.Id] = slotElement;
        _mainCanvas.Children.Add(slotElement);

        // Position the element
        Canvas.SetLeft(slotElement, slot.Position.X * ActualWidth);
        Canvas.SetTop(slotElement, slot.Position.Y * ActualHeight);

        // Add interaction handlers
        slotElement.MouseEnter += (s, e) => OnSlotMouseEnter(slot.Id);
        slotElement.MouseLeave += (s, e) => OnSlotMouseLeave(slot.Id);
        slotElement.MouseLeftButtonDown += (s, e) => OnSlotMouseClick(slot.Id);

        if (EnableAnimations && !_isSimplifiedMode)
        {
            AnimateSlotHighlight(slotElement, state);
        }
    }

    private FrameworkElement CreateSlotElement(HoloModuleSlot slot, HighlightState state)
    {
        var container = new Grid
        {
            Width = GetSlotSize(slot.Type),
            Height = GetSlotSize(slot.Type)
        };

        // Main slot indicator
        var slotIndicator = new Ellipse
        {
            Fill = GetSlotBrush(slot, state),
            Stroke = GetSlotStrokeBrush(slot, state),
            StrokeThickness = 2,
            Width = container.Width,
            Height = container.Height
        };

        container.Children.Add(slotIndicator);

        // Compatibility indicator ring
        if (ShowCompatibilityIndicators && state.HighlightType != HighlightType.Standard)
        {
            var compatibilityRing = new Ellipse
            {
                Fill = Brushes.Transparent,
                Stroke = GetCompatibilityBrush(state.HighlightType),
                StrokeThickness = 3,
                Width = container.Width + 10,
                Height = container.Height + 10,
                Margin = new Thickness(-5)
            };

            if (EnableAnimations && !_isSimplifiedMode)
            {
                var animation = new DoubleAnimation
                {
                    From = 0.3,
                    To = 1.0,
                    Duration = TimeSpan.FromSeconds(0.8),
                    RepeatBehavior = RepeatBehavior.Forever,
                    AutoReverse = true
                };
                compatibilityRing.BeginAnimation(OpacityProperty, animation);
            }

            container.Children.Add(compatibilityRing);
        }

        // Slot name label
        if (ShowSlotNames)
        {
            var nameLabel = new TextBlock
            {
                Text = GetSlotDisplayName(slot),
                Foreground = GetSlotTextBrush(),
                FontSize = 10,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 0, -15)
            };

            container.Children.Add(nameLabel);
        }

        // Add glow effect for enhanced highlighting
        if (state.HighlightType != HighlightType.Incompatible)
        {
            var glowEffect = new DropShadowEffect
            {
                Color = GetHighlightColor(state.HighlightType),
                BlurRadius = 10,
                ShadowDepth = 0,
                Opacity = state.Intensity
            };
            container.Effect = glowEffect;
        }

        return container;
    }

    private void UpdateSlotElement(FrameworkElement element, HoloModuleSlot slot, HighlightState state)
    {
        if (element is Grid container)
        {
            var slotIndicator = container.Children.OfType<Ellipse>().FirstOrDefault();
            if (slotIndicator != null)
            {
                slotIndicator.Fill = GetSlotBrush(slot, state);
                slotIndicator.Stroke = GetSlotStrokeBrush(slot, state);
            }

            // Update glow effect
            if (container.Effect is DropShadowEffect glowEffect)
            {
                glowEffect.Color = GetHighlightColor(state.HighlightType);
                glowEffect.Opacity = state.Intensity;
            }
        }
    }

    private void AnimateSlotHighlight(FrameworkElement element, HighlightState state)
    {
        switch (HighlightMode)
        {
            case HighlightMode.Pulse:
                AnimatePulse(element, state);
                break;
            case HighlightMode.Glow:
                AnimateGlow(element, state);
                break;
            case HighlightMode.Ripple:
                AnimateRipple(element, state);
                break;
            case HighlightMode.Sparkle:
                AnimateSparkle(element, state);
                break;
        }
    }

    private void AnimatePulse(FrameworkElement element, HighlightState state)
    {
        var scaleAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 1.3,
            Duration = TimeSpan.FromMilliseconds(600),
            RepeatBehavior = RepeatBehavior.Forever,
            AutoReverse = true,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        var scaleTransform = new ScaleTransform();
        element.RenderTransform = scaleTransform;
        element.RenderTransformOrigin = new Point(0.5, 0.5);

        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
    }

    private void AnimateGlow(FrameworkElement element, HighlightState state)
    {
        if (element.Effect is DropShadowEffect glowEffect)
        {
            var glowAnimation = new DoubleAnimation
            {
                From = 0.3,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(1),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };

            glowEffect.BeginAnimation(DropShadowEffect.OpacityProperty, glowAnimation);
        }
    }

    private void AnimateRipple(FrameworkElement element, HighlightState state)
    {
        // Create ripple effect with expanding circles
        var rippleCanvas = new Canvas
        {
            Width = element.Width * 2,
            Height = element.Height * 2,
            IsHitTestVisible = false
        };

        Canvas.SetLeft(rippleCanvas, -element.Width / 2);
        Canvas.SetTop(rippleCanvas, -element.Height / 2);

        if (element.Parent is Panel parent)
        {
            parent.Children.Add(rippleCanvas);
        }

        for (int i = 0; i < 3; i++)
        {
            var ripple = new Ellipse
            {
                Width = 0,
                Height = 0,
                Stroke = GetSlotStrokeBrush(null, state),
                StrokeThickness = 2,
                Fill = Brushes.Transparent
            };

            Canvas.SetLeft(ripple, rippleCanvas.Width / 2);
            Canvas.SetTop(ripple, rippleCanvas.Height / 2);
            rippleCanvas.Children.Add(ripple);

            var sizeAnimation = new DoubleAnimation
            {
                From = 0,
                To = rippleCanvas.Width,
                Duration = TimeSpan.FromSeconds(2),
                BeginTime = TimeSpan.FromMilliseconds(i * 400),
                RepeatBehavior = RepeatBehavior.Forever
            };

            var opacityAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(2),
                BeginTime = TimeSpan.FromMilliseconds(i * 400),
                RepeatBehavior = RepeatBehavior.Forever
            };

            ripple.BeginAnimation(WidthProperty, sizeAnimation);
            ripple.BeginAnimation(HeightProperty, sizeAnimation);
            ripple.BeginAnimation(OpacityProperty, opacityAnimation);
        }
    }

    private void AnimateSparkle(FrameworkElement element, HighlightState state)
    {
        // Add sparkle particles around the slot
        if (EnableParticleEffects && !_isSimplifiedMode)
        {
            SpawnSparkleParticles(element, state);
        }
    }

    private void AnimateSlotUnhighlight(FrameworkElement element, Action onComplete)
    {
        var fadeAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        fadeAnimation.Completed += (s, e) => onComplete?.Invoke();
        element.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void SpawnHighlightParticles(string slotId, HighlightType type)
    {
        if (!_slotElements.TryGetValue(slotId, out var element)) return;

        var particleCanvas = new Canvas
        {
            Width = ActualWidth,
            Height = ActualHeight,
            IsHitTestVisible = false
        };

        _slotParticles[slotId] = particleCanvas;
        _mainCanvas.Children.Add(particleCanvas);

        var particleCount = type switch
        {
            HighlightType.Compatible => 8,
            HighlightType.Incompatible => 4,
            HighlightType.Warning => 6,
            HighlightType.Error => 10,
            _ => 5
        };

        var elementPosition = element.TransformToAncestor(_mainCanvas).Transform(new Point(0, 0));

        for (int i = 0; i < particleCount; i++)
        {
            var particle = CreateHighlightParticle(type);
            Canvas.SetLeft(particle, elementPosition.X + element.Width / 2);
            Canvas.SetTop(particle, elementPosition.Y + element.Height / 2);
            particleCanvas.Children.Add(particle);

            AnimateHighlightParticle(particle, type);
        }
    }

    private UIElement CreateHighlightParticle(HighlightType type)
    {
        var color = GetHighlightColor(type);
        var size = _random.Next(2, 5);

        return new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromArgb(200, color.R, color.G, color.B))
        };
    }

    private void AnimateHighlightParticle(UIElement particle, HighlightType type)
    {
        var angle = _random.NextDouble() * Math.PI * 2;
        var distance = _random.Next(20, 60);
        var targetX = Canvas.GetLeft(particle) + Math.Cos(angle) * distance;
        var targetY = Canvas.GetTop(particle) + Math.Sin(angle) * distance;

        var moveXAnimation = new DoubleAnimation
        {
            From = Canvas.GetLeft(particle),
            To = targetX,
            Duration = TimeSpan.FromMilliseconds(_random.Next(800, 1500)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var moveYAnimation = new DoubleAnimation
        {
            From = Canvas.GetTop(particle),
            To = targetY,
            Duration = TimeSpan.FromMilliseconds(_random.Next(800, 1500)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(_random.Next(800, 1500))
        };

        moveXAnimation.Completed += (s, e) =>
        {
            if (particle.Parent is Panel parent)
            {
                parent.Children.Remove(particle);
            }
        };

        particle.BeginAnimation(Canvas.LeftProperty, moveXAnimation);
        particle.BeginAnimation(Canvas.TopProperty, moveYAnimation);
        particle.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void SpawnSparkleParticles(FrameworkElement element, HighlightState state)
    {
        // Implementation for sparkle particle effects
        var elementPosition = element.TransformToAncestor(_mainCanvas).Transform(new Point(0, 0));
        
        for (int i = 0; i < 5; i++)
        {
            var sparkle = new Polygon
            {
                Points = CreateStarPoints(4),
                Fill = new SolidColorBrush(GetHighlightColor(state.HighlightType)),
                Width = 6,
                Height = 6
            };

            var offsetX = _random.Next(-15, 15);
            var offsetY = _random.Next(-15, 15);
            
            Canvas.SetLeft(sparkle, elementPosition.X + element.Width / 2 + offsetX);
            Canvas.SetTop(sparkle, elementPosition.Y + element.Height / 2 + offsetY);
            _mainCanvas.Children.Add(sparkle);

            var scaleAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
            };

            var fadeAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(800),
                BeginTime = TimeSpan.FromMilliseconds(200)
            };

            var rotateAnimation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(1),
                RepeatBehavior = RepeatBehavior.Forever
            };

            var scaleTransform = new ScaleTransform();
            var rotateTransform = new RotateTransform();
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(scaleTransform);
            transformGroup.Children.Add(rotateTransform);
            
            sparkle.RenderTransform = transformGroup;
            sparkle.RenderTransformOrigin = new Point(0.5, 0.5);

            fadeAnimation.Completed += (s, e) => _mainCanvas.Children.Remove(sparkle);

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
            sparkle.BeginAnimation(OpacityProperty, fadeAnimation);
        }
    }

    private PointCollection CreateStarPoints(int numPoints)
    {
        var points = new PointCollection();
        var centerX = 3.0;
        var centerY = 3.0;
        var outerRadius = 3.0;
        var innerRadius = 1.5;

        for (int i = 0; i < numPoints * 2; i++)
        {
            var angle = i * Math.PI / numPoints;
            var radius = (i % 2 == 0) ? outerRadius : innerRadius;
            var x = centerX + radius * Math.Cos(angle);
            var y = centerY + radius * Math.Sin(angle);
            points.Add(new Point(x, y));
        }

        return points;
    }

    private void CleanupSlotParticles(string slotId)
    {
        if (_slotParticles.TryGetValue(slotId, out var particleCanvas))
        {
            _mainCanvas.Children.Remove(particleCanvas);
            _slotParticles.Remove(slotId);
        }
    }

    private void RefreshSlotElements()
    {
        ClearAllHighlights();
        
        if (AutoHighlightEmptySlots && ModuleSlots != null)
        {
            var emptySlots = ModuleSlots.Where(s => !s.IsOccupied);
            foreach (var slot in emptySlots)
            {
                HighlightSlot(slot.Id, HighlightType.Standard);
            }
        }
    }

    private bool IsSlotCompatible(HoloModuleSlot slot, string moduleType)
    {
        // Mock compatibility check - in real implementation this would check actual EVE module compatibility
        if (string.IsNullOrEmpty(moduleType)) return false;
        
        return moduleType.ToLower() switch
        {
            "weapon" => slot.Type == ModuleSlotType.High,
            "shield" => slot.Type == ModuleSlotType.Medium,
            "armor" => slot.Type == ModuleSlotType.Low,
            "rig" => slot.Type == ModuleSlotType.Rig,
            "subsystem" => slot.Type == ModuleSlotType.Subsystem,
            _ => true
        };
    }

    #endregion

    #region Helper Methods

    private double GetSlotSize(ModuleSlotType slotType)
    {
        return slotType switch
        {
            ModuleSlotType.High => 24,
            ModuleSlotType.Medium => 20,
            ModuleSlotType.Low => 18,
            ModuleSlotType.Rig => 16,
            ModuleSlotType.Subsystem => 22,
            _ => 20
        };
    }

    private Brush GetSlotBrush(HoloModuleSlot slot, HighlightState state)
    {
        var baseColor = GetSlotColor(slot?.Type ?? ModuleSlotType.High);
        var alpha = (byte)(120 * state.Intensity);
        
        if (state.HighlightType == HighlightType.Incompatible)
        {
            alpha = (byte)(60 * state.Intensity);
        }

        return new SolidColorBrush(Color.FromArgb(alpha, baseColor.R, baseColor.G, baseColor.B));
    }

    private Brush GetSlotStrokeBrush(HoloModuleSlot slot, HighlightState state)
    {
        var highlightColor = GetHighlightColor(state.HighlightType);
        var alpha = (byte)(200 * state.Intensity);
        
        return new SolidColorBrush(Color.FromArgb(alpha, highlightColor.R, highlightColor.G, highlightColor.B));
    }

    private Brush GetCompatibilityBrush(HighlightType type)
    {
        var color = GetHighlightColor(type);
        return new SolidColorBrush(Color.FromArgb(180, color.R, color.G, color.B));
    }

    private Brush GetSlotTextBrush()
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        return new SolidColorBrush(colors.Primary);
    }

    private Color GetSlotColor(ModuleSlotType slotType)
    {
        return slotType switch
        {
            ModuleSlotType.High => Color.FromRgb(255, 100, 100),
            ModuleSlotType.Medium => Color.FromRgb(255, 255, 100),
            ModuleSlotType.Low => Color.FromRgb(100, 255, 100),
            ModuleSlotType.Rig => Color.FromRgb(100, 100, 255),
            ModuleSlotType.Subsystem => Color.FromRgb(255, 100, 255),
            _ => Color.FromRgb(150, 150, 150)
        };
    }

    private Color GetHighlightColor(HighlightType type)
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        
        return type switch
        {
            HighlightType.Compatible => Color.FromRgb(100, 255, 100),
            HighlightType.Incompatible => Color.FromRgb(255, 100, 100),
            HighlightType.Warning => Color.FromRgb(255, 255, 100),
            HighlightType.Error => Color.FromRgb(255, 0, 0),
            HighlightType.Success => Color.FromRgb(0, 255, 100),
            _ => colors.Primary
        };
    }

    private string GetSlotDisplayName(HoloModuleSlot slot)
    {
        return $"{slot.Type} {slot.Id}";
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableAnimations && !_isSimplifiedMode)
        {
            _animationTimer.Start();
        }
        
        _highlightTimer.Start();
        RefreshSlotElements();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer.Stop();
        _highlightTimer.Stop();
        ClearAllHighlights();
    }

    private void OnAnimationTimerTick(object sender, EventArgs e)
    {
        if (!EnableAnimations || _isSimplifiedMode) return;

        _animationPhase += 0.1;
        if (_animationPhase > Math.PI * 2)
            _animationPhase = 0;

        UpdatePulsingSlots();
    }

    private void OnHighlightTimerTick(object sender, EventArgs e)
    {
        var currentTime = DateTime.Now;
        var expiredSlots = new List<string>();

        foreach (var kvp in _slotStates)
        {
            var state = kvp.Value;
            if (state.IsHighlighted && state.Duration != TimeSpan.Zero)
            {
                if (currentTime - state.StartTime >= state.Duration)
                {
                    expiredSlots.Add(kvp.Key);
                }
            }
        }

        foreach (var slotId in expiredSlots)
        {
            UnhighlightSlot(slotId);
        }
    }

    private void UpdatePulsingSlots()
    {
        foreach (var kvp in _slotStates)
        {
            var state = kvp.Value;
            if (state.IsPulsing)
            {
                // Update pulse animation logic here
                var pulseIntensity = Math.Sin(_animationPhase * 3) * 0.5 + 0.5;
                state.Intensity = HolographicIntensity * pulseIntensity;

                if (_slotElements.TryGetValue(kvp.Key, out var element))
                {
                    var slot = ModuleSlots?.FirstOrDefault(s => s.Id == kvp.Key);
                    if (slot != null)
                    {
                        UpdateSlotElement(element, slot, state);
                    }
                }
            }
        }
    }

    private void OnSlotMouseEnter(string slotId)
    {
        SlotHovered?.Invoke(this, new SlotHighlightEventArgs
        {
            SlotId = slotId,
            Timestamp = DateTime.Now
        });
    }

    private void OnSlotMouseLeave(string slotId)
    {
        // Handle mouse leave if needed
    }

    private void OnSlotMouseClick(string slotId)
    {
        SlotClicked?.Invoke(this, new SlotHighlightEventArgs
        {
            SlotId = slotId,
            Timestamp = DateTime.Now
        });
    }

    #endregion

    #region Property Change Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloModuleSlotHighlighter highlighter)
        {
            highlighter.RefreshSlotElements();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloModuleSlotHighlighter highlighter)
        {
            highlighter.ApplyEVEColorScheme();
        }
    }

    private static void OnModuleSlotsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloModuleSlotHighlighter highlighter)
        {
            highlighter.RefreshSlotElements();
        }
    }

    private static void OnHighlightModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloModuleSlotHighlighter highlighter)
        {
            highlighter.RefreshSlotElements();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloModuleSlotHighlighter highlighter)
        {
            if ((bool)e.NewValue && !highlighter._isSimplifiedMode)
            {
                highlighter._animationTimer.Start();
            }
            else
            {
                highlighter._animationTimer.Stop();
            }
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloModuleSlotHighlighter highlighter && !(bool)e.NewValue)
        {
            // Clean up all particle effects
            foreach (var slotId in highlighter._slotParticles.Keys.ToList())
            {
                highlighter.CleanupSlotParticles(slotId);
            }
        }
    }

    private static void OnShowCompatibilityIndicatorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloModuleSlotHighlighter highlighter)
        {
            highlighter.RefreshSlotElements();
        }
    }

    private static void OnShowSlotNamesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloModuleSlotHighlighter highlighter)
        {
            highlighter.RefreshSlotElements();
        }
    }

    private static void OnHighlightDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Duration change doesn't require immediate refresh
    }

    private static void OnAutoHighlightEmptySlotsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloModuleSlotHighlighter highlighter)
        {
            highlighter.RefreshSlotElements();
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
        _animationTimer.Stop();
        
        // Clean up existing particle effects
        foreach (var slotId in _slotParticles.Keys.ToList())
        {
            CleanupSlotParticles(slotId);
        }
    }

    public void ExitSimplifiedMode()
    {
        _isSimplifiedMode = false;
        EnableAnimations = true;
        EnableParticleEffects = true;
        _animationTimer.Start();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => true;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        
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
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        
        // Update any existing slot elements
        RefreshSlotElements();
    }

    #endregion
}

#region Supporting Classes and Enums

public class HighlightState
{
    public bool IsHighlighted { get; set; }
    public HighlightType HighlightType { get; set; }
    public DateTime StartTime { get; set; }
    public TimeSpan Duration { get; set; }
    public double Intensity { get; set; } = 1.0;
    public bool IsPulsing { get; set; }
    public int PulseCount { get; set; }
    public int CurrentPulse { get; set; }
}

public enum HighlightType
{
    Standard,
    Compatible,
    Incompatible,
    Warning,
    Error,
    Success
}

public enum HighlightMode
{
    Pulse,
    Glow,
    Ripple,
    Sparkle
}

public class SlotHighlightEventArgs : EventArgs
{
    public string SlotId { get; set; }
    public HighlightType HighlightType { get; set; }
    public DateTime Timestamp { get; set; }
}

#endregion