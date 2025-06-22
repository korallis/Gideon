// ==========================================================================
// HoloDamageVisualization.cs - Holographic Damage Visualization System
// ==========================================================================
// Advanced damage visualization featuring real-time damage mapping, hull breach
// effects, shield/armor damage patterns, and EVE-style combat feedback.
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
/// Holographic damage visualization system with real-time damage mapping and combat effects
/// </summary>
public class HoloDamageVisualization : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloDamageVisualization),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloDamageVisualization),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty DamageDataProperty =
        DependencyProperty.Register(nameof(DamageData), typeof(HoloDamageData), typeof(HoloDamageVisualization),
            new PropertyMetadata(null, OnDamageDataChanged));

    public static readonly DependencyProperty VisualizationModeProperty =
        DependencyProperty.Register(nameof(VisualizationMode), typeof(DamageVisualizationMode), typeof(HoloDamageVisualization),
            new PropertyMetadata(DamageVisualizationMode.Realistic, OnVisualizationModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloDamageVisualization),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloDamageVisualization),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowDamageHeatmapProperty =
        DependencyProperty.Register(nameof(ShowDamageHeatmap), typeof(bool), typeof(HoloDamageVisualization),
            new PropertyMetadata(true, OnShowDamageHeatmapChanged));

    public static readonly DependencyProperty ShowStructuralIntegrityProperty =
        DependencyProperty.Register(nameof(ShowStructuralIntegrity), typeof(bool), typeof(HoloDamageVisualization),
            new PropertyMetadata(true, OnShowStructuralIntegrityChanged));

    public static readonly DependencyProperty AnimationSpeedProperty =
        DependencyProperty.Register(nameof(AnimationSpeed), typeof(double), typeof(HoloDamageVisualization),
            new PropertyMetadata(1.0, OnAnimationSpeedChanged));

    public static readonly DependencyProperty EnableRealTimeDamageProperty =
        DependencyProperty.Register(nameof(EnableRealTimeDamage), typeof(bool), typeof(HoloDamageVisualization),
            new PropertyMetadata(false, OnEnableRealTimeDamageChanged));

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

    public HoloDamageData DamageData
    {
        get => (HoloDamageData)GetValue(DamageDataProperty);
        set => SetValue(DamageDataProperty, value);
    }

    public DamageVisualizationMode VisualizationMode
    {
        get => (DamageVisualizationMode)GetValue(VisualizationModeProperty);
        set => SetValue(VisualizationModeProperty, value);
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

    public bool ShowDamageHeatmap
    {
        get => (bool)GetValue(ShowDamageHeatmapProperty);
        set => SetValue(ShowDamageHeatmapProperty, value);
    }

    public bool ShowStructuralIntegrity
    {
        get => (bool)GetValue(ShowStructuralIntegrityProperty);
        set => SetValue(ShowStructuralIntegrityProperty, value);
    }

    public double AnimationSpeed
    {
        get => (double)GetValue(AnimationSpeedProperty);
        set => SetValue(AnimationSpeedProperty, value);
    }

    public bool EnableRealTimeDamage
    {
        get => (bool)GetValue(EnableRealTimeDamageProperty);
        set => SetValue(EnableRealTimeDamageProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<DamageEventArgs> DamageReceived;
    public event EventHandler<DamageEventArgs> CriticalDamage;
    public event EventHandler<DamageEventArgs> StructuralFailure;
    public event EventHandler<DamageEventArgs> RepairCompleted;
    public event EventHandler<DamageEventArgs> DamagePatternChanged;

    #endregion

    #region Private Fields

    private readonly DispatcherTimer _animationTimer;
    private readonly DispatcherTimer _damageTimer;
    private readonly Dictionary<string, DamageZone> _damageZones = new();
    private readonly Dictionary<string, List<UIElement>> _damageEffects = new();
    private readonly List<UIElement> _particleEffects = new();
    private readonly Queue<DamageEvent> _damageQueue = new();
    private Canvas _mainCanvas;
    private Canvas _heatmapCanvas;
    private Canvas _structuralCanvas;
    private Canvas _effectsCanvas;
    private bool _isSimplifiedMode;
    private double _animationPhase;
    private readonly Random _random = new();

    #endregion

    #region Constructor

    public HoloDamageVisualization()
    {
        InitializeComponent();
        InitializeTimers();
        InitializeDamageZones();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 600;
        Height = 400;
        Background = new SolidColorBrush(Colors.Transparent);
        
        CreateCanvasLayers();
        ApplyEVEColorScheme();
    }

    private void CreateCanvasLayers()
    {
        _mainCanvas = new Canvas
        {
            Width = Width,
            Height = Height,
            ClipToBounds = true
        };
        Content = _mainCanvas;

        // Structural integrity layer (background)
        _structuralCanvas = new Canvas
        {
            Width = Width,
            Height = Height,
            IsHitTestVisible = false
        };
        _mainCanvas.Children.Add(_structuralCanvas);

        // Damage heatmap layer (middle)
        _heatmapCanvas = new Canvas
        {
            Width = Width,
            Height = Height,
            IsHitTestVisible = false
        };
        _mainCanvas.Children.Add(_heatmapCanvas);

        // Effects layer (foreground)
        _effectsCanvas = new Canvas
        {
            Width = Width,
            Height = Height,
            IsHitTestVisible = false
        };
        _mainCanvas.Children.Add(_effectsCanvas);
    }

    private void InitializeTimers()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
        };
        _animationTimer.Tick += OnAnimationTimerTick;

        _damageTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100) // 10 Hz for damage processing
        };
        _damageTimer.Tick += OnDamageTimerTick;
    }

    private void InitializeDamageZones()
    {
        // Create damage zones for different ship sections
        var zones = new[]
        {
            new DamageZone { Id = "bow", Name = "Bow", Region = new Rect(200, 50, 200, 100), MaxIntegrity = 100 },
            new DamageZone { Id = "port", Name = "Port", Region = new Rect(50, 150, 150, 100), MaxIntegrity = 100 },
            new DamageZone { Id = "starboard", Name = "Starboard", Region = new Rect(400, 150, 150, 100), MaxIntegrity = 100 },
            new DamageZone { Id = "stern", Name = "Stern", Region = new Rect(200, 300, 200, 100), MaxIntegrity = 100 },
            new DamageZone { Id = "core", Name = "Core", Region = new Rect(250, 175, 100, 50), MaxIntegrity = 150 },
            new DamageZone { Id = "engines", Name = "Engines", Region = new Rect(225, 320, 150, 60), MaxIntegrity = 80 }
        };

        foreach (var zone in zones)
        {
            zone.CurrentIntegrity = zone.MaxIntegrity;
            _damageZones[zone.Id] = zone;
        }
    }

    #endregion

    #region Public Methods

    public void ApplyDamage(string zoneId, double amount, DamageType damageType)
    {
        if (!_damageZones.TryGetValue(zoneId, out var zone)) return;

        var damageEvent = new DamageEvent
        {
            ZoneId = zoneId,
            Amount = amount,
            Type = damageType,
            Timestamp = DateTime.Now,
            IsCritical = amount > zone.MaxIntegrity * 0.2
        };

        _damageQueue.Enqueue(damageEvent);
        ProcessDamageEvent(damageEvent);

        DamageReceived?.Invoke(this, new DamageEventArgs
        {
            DamageEvent = damageEvent,
            Zone = zone,
            Timestamp = DateTime.Now
        });
    }

    public void RepairDamage(string zoneId, double amount)
    {
        if (!_damageZones.TryGetValue(zoneId, out var zone)) return;

        var oldIntegrity = zone.CurrentIntegrity;
        zone.CurrentIntegrity = Math.Min(zone.MaxIntegrity, zone.CurrentIntegrity + amount);

        if (zone.CurrentIntegrity > oldIntegrity)
        {
            CreateRepairEffect(zone);
            
            RepairCompleted?.Invoke(this, new DamageEventArgs
            {
                Zone = zone,
                Timestamp = DateTime.Now
            });
        }

        UpdateDamageVisualization();
    }

    public void ClearAllDamage()
    {
        foreach (var zone in _damageZones.Values)
        {
            zone.CurrentIntegrity = zone.MaxIntegrity;
            zone.DamagePattern.Clear();
        }

        ClearAllEffects();
        UpdateDamageVisualization();
    }

    public void SetDamagePattern(string zoneId, List<DamagePoint> pattern)
    {
        if (!_damageZones.TryGetValue(zoneId, out var zone)) return;

        zone.DamagePattern = pattern;
        UpdateDamageVisualization();

        DamagePatternChanged?.Invoke(this, new DamageEventArgs
        {
            Zone = zone,
            Timestamp = DateTime.Now
        });
    }

    public void SimulateCombatDamage(int duration = 5000)
    {
        if (!EnableRealTimeDamage) return;

        var combatTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(_random.Next(200, 800))
        };

        var combatEndTime = DateTime.Now.AddMilliseconds(duration);

        combatTimer.Tick += (s, e) =>
        {
            if (DateTime.Now > combatEndTime)
            {
                combatTimer.Stop();
                return;
            }

            // Simulate random damage
            var zoneIds = _damageZones.Keys.ToArray();
            var randomZone = zoneIds[_random.Next(zoneIds.Length)];
            var damageAmount = _random.Next(5, 25);
            var damageTypes = Enum.GetValues<DamageType>();
            var randomType = (DamageType)damageTypes.GetValue(_random.Next(damageTypes.Length));

            ApplyDamage(randomZone, damageAmount, randomType);

            // Vary the interval for realistic combat feel
            combatTimer.Interval = TimeSpan.FromMilliseconds(_random.Next(200, 800));
        };

        combatTimer.Start();
    }

    public double GetOverallIntegrity()
    {
        if (_damageZones.Count == 0) return 100;

        var totalCurrent = _damageZones.Values.Sum(z => z.CurrentIntegrity);
        var totalMax = _damageZones.Values.Sum(z => z.MaxIntegrity);
        
        return (totalCurrent / totalMax) * 100;
    }

    public DamageZone GetMostDamagedZone()
    {
        return _damageZones.Values
            .OrderBy(z => z.CurrentIntegrity / z.MaxIntegrity)
            .FirstOrDefault();
    }

    #endregion

    #region Private Methods

    private void ProcessDamageEvent(DamageEvent damageEvent)
    {
        var zone = _damageZones[damageEvent.ZoneId];
        
        // Apply damage to zone
        var oldIntegrity = zone.CurrentIntegrity;
        zone.CurrentIntegrity = Math.Max(0, zone.CurrentIntegrity - damageEvent.Amount);

        // Add damage point to pattern
        var damagePoint = new DamagePoint
        {
            Position = GetRandomPointInZone(zone),
            Severity = damageEvent.Amount / zone.MaxIntegrity,
            Type = damageEvent.Type,
            Timestamp = damageEvent.Timestamp
        };
        zone.DamagePattern.Add(damagePoint);

        // Create visual effects
        CreateDamageEffect(zone, damagePoint, damageEvent);

        // Check for critical damage
        if (damageEvent.IsCritical || zone.CurrentIntegrity < zone.MaxIntegrity * 0.2)
        {
            CreateCriticalDamageEffect(zone, damagePoint);
            
            CriticalDamage?.Invoke(this, new DamageEventArgs
            {
                DamageEvent = damageEvent,
                Zone = zone,
                Timestamp = DateTime.Now
            });
        }

        // Check for structural failure
        if (zone.CurrentIntegrity <= 0)
        {
            CreateStructuralFailureEffect(zone);
            
            StructuralFailure?.Invoke(this, new DamageEventArgs
            {
                DamageEvent = damageEvent,
                Zone = zone,
                Timestamp = DateTime.Now
            });
        }

        UpdateDamageVisualization();
    }

    private void UpdateDamageVisualization()
    {
        switch (VisualizationMode)
        {
            case DamageVisualizationMode.Realistic:
                UpdateRealisticVisualization();
                break;
            case DamageVisualizationMode.Schematic:
                UpdateSchematicVisualization();
                break;
            case DamageVisualizationMode.Heatmap:
                UpdateHeatmapVisualization();
                break;
            case DamageVisualizationMode.Wireframe:
                UpdateWireframeVisualization();
                break;
        }
    }

    private void UpdateRealisticVisualization()
    {
        _heatmapCanvas.Children.Clear();

        foreach (var zone in _damageZones.Values)
        {
            var integrityPercentage = zone.CurrentIntegrity / zone.MaxIntegrity;
            
            // Create zone representation
            var zoneRect = new Rectangle
            {
                Width = zone.Region.Width,
                Height = zone.Region.Height,
                Fill = GetZoneStatusBrush(integrityPercentage),
                Stroke = new SolidColorBrush(GetZoneStatusColor(integrityPercentage)),
                StrokeThickness = 2,
                Opacity = HolographicIntensity * 0.7
            };

            Canvas.SetLeft(zoneRect, zone.Region.X);
            Canvas.SetTop(zoneRect, zone.Region.Y);
            _heatmapCanvas.Children.Add(zoneRect);

            // Add damage points
            foreach (var damagePoint in zone.DamagePattern)
            {
                CreateDamagePointVisualization(damagePoint);
            }

            // Add zone label
            if (ShowStructuralIntegrity)
            {
                var label = new TextBlock
                {
                    Text = $"{zone.Name}\n{integrityPercentage:P0}",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Colors.White),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Effect = new DropShadowEffect
                    {
                        Color = Colors.Black,
                        BlurRadius = 3,
                        ShadowDepth = 1
                    }
                };

                Canvas.SetLeft(label, zone.Region.X + zone.Region.Width / 2 - 25);
                Canvas.SetTop(label, zone.Region.Y + zone.Region.Height / 2 - 10);
                _heatmapCanvas.Children.Add(label);
            }
        }
    }

    private void UpdateSchematicVisualization()
    {
        _heatmapCanvas.Children.Clear();

        foreach (var zone in _damageZones.Values)
        {
            var integrityPercentage = zone.CurrentIntegrity / zone.MaxIntegrity;
            
            // Create schematic zone outline
            var zoneOutline = new Rectangle
            {
                Width = zone.Region.Width,
                Height = zone.Region.Height,
                Fill = Brushes.Transparent,
                Stroke = new SolidColorBrush(GetSchematicColor(integrityPercentage)),
                StrokeThickness = 3,
                StrokeDashArray = new DoubleCollection { 5, 3 },
                Opacity = HolographicIntensity
            };

            Canvas.SetLeft(zoneOutline, zone.Region.X);
            Canvas.SetTop(zoneOutline, zone.Region.Y);
            _heatmapCanvas.Children.Add(zoneOutline);

            // Add integrity bar
            var integrityBar = new Rectangle
            {
                Width = zone.Region.Width * 0.8,
                Height = 8,
                Fill = new SolidColorBrush(GetZoneStatusColor(integrityPercentage)),
                Opacity = HolographicIntensity
            };

            Canvas.SetLeft(integrityBar, zone.Region.X + zone.Region.Width * 0.1);
            Canvas.SetTop(integrityBar, zone.Region.Y + zone.Region.Height - 15);
            _heatmapCanvas.Children.Add(integrityBar);
        }
    }

    private void UpdateHeatmapVisualization()
    {
        if (!ShowDamageHeatmap) return;

        _heatmapCanvas.Children.Clear();

        // Create heatmap grid
        var gridSize = 20;
        var cellWidth = Width / gridSize;
        var cellHeight = Height / gridSize;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                var cellRect = new Rect(x * cellWidth, y * cellHeight, cellWidth, cellHeight);
                var damage = CalculateAreaDamage(cellRect);
                
                if (damage > 0)
                {
                    var heatCell = new Rectangle
                    {
                        Width = cellWidth,
                        Height = cellHeight,
                        Fill = GetHeatmapBrush(damage),
                        Opacity = HolographicIntensity * damage
                    };

                    Canvas.SetLeft(heatCell, cellRect.X);
                    Canvas.SetTop(heatCell, cellRect.Y);
                    _heatmapCanvas.Children.Add(heatCell);
                }
            }
        }
    }

    private void UpdateWireframeVisualization()
    {
        _heatmapCanvas.Children.Clear();

        foreach (var zone in _damageZones.Values)
        {
            var integrityPercentage = zone.CurrentIntegrity / zone.MaxIntegrity;
            
            // Create wireframe lines
            var lines = CreateWireframeLines(zone.Region, integrityPercentage);
            foreach (var line in lines)
            {
                _heatmapCanvas.Children.Add(line);
            }
        }
    }

    private void CreateDamageEffect(DamageZone zone, DamagePoint point, DamageEvent damageEvent)
    {
        if (!EnableParticleEffects || _isSimplifiedMode) return;

        var effects = new List<UIElement>();

        // Impact flash
        var flash = new Ellipse
        {
            Width = 20 * point.Severity,
            Height = 20 * point.Severity,
            Fill = GetDamageTypeFlashBrush(point.Type),
            Opacity = HolographicIntensity
        };

        Canvas.SetLeft(flash, point.Position.X - flash.Width / 2);
        Canvas.SetTop(flash, point.Position.Y - flash.Height / 2);
        _effectsCanvas.Children.Add(flash);
        effects.Add(flash);

        // Animate flash
        var scaleTransform = new ScaleTransform(0, 0);
        flash.RenderTransform = scaleTransform;
        flash.RenderTransformOrigin = new Point(0.5, 0.5);

        var scaleAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1.5,
            Duration = TimeSpan.FromMilliseconds(200),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = HolographicIntensity,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(400)
        };

        fadeAnimation.Completed += (s, e) =>
        {
            _effectsCanvas.Children.Remove(flash);
            effects.Remove(flash);
        };

        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        flash.BeginAnimation(OpacityProperty, fadeAnimation);

        // Debris particles
        CreateDebrisParticles(point.Position, point.Type, point.Severity);

        if (!_damageEffects.ContainsKey(zone.Id))
        {
            _damageEffects[zone.Id] = new List<UIElement>();
        }
        _damageEffects[zone.Id].AddRange(effects);
    }

    private void CreateCriticalDamageEffect(DamageZone zone, DamagePoint point)
    {
        // Critical damage warning flash
        var criticalFlash = new Rectangle
        {
            Width = ActualWidth,
            Height = ActualHeight,
            Fill = new SolidColorBrush(Color.FromArgb(100, 255, 0, 0)),
            IsHitTestVisible = false
        };

        _effectsCanvas.Children.Add(criticalFlash);

        var flashAnimation = new DoubleAnimation
        {
            From = 0.5,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(300),
            RepeatBehavior = new RepeatBehavior(3),
            AutoReverse = true
        };

        flashAnimation.Completed += (s, e) => _effectsCanvas.Children.Remove(criticalFlash);
        criticalFlash.BeginAnimation(OpacityProperty, flashAnimation);

        // Electrical discharge effect
        CreateElectricalDischarge(point.Position);
    }

    private void CreateStructuralFailureEffect(DamageZone zone)
    {
        // Structural failure explosion effect
        var explosionCenter = new Point(
            zone.Region.X + zone.Region.Width / 2,
            zone.Region.Y + zone.Region.Height / 2);

        CreateExplosionEffect(explosionCenter, zone.Region.Width / 2);

        // Zone destruction visual
        var destructionOverlay = new Rectangle
        {
            Width = zone.Region.Width,
            Height = zone.Region.Height,
            Fill = new SolidColorBrush(Color.FromArgb(150, 255, 100, 0)),
            Effect = new BlurEffect { Radius = 5 }
        };

        Canvas.SetLeft(destructionOverlay, zone.Region.X);
        Canvas.SetTop(destructionOverlay, zone.Region.Y);
        _effectsCanvas.Children.Add(destructionOverlay);

        if (EnableAnimations && !_isSimplifiedMode)
        {
            var flickerAnimation = new DoubleAnimation
            {
                From = 0.7,
                To = 0.3,
                Duration = TimeSpan.FromMilliseconds(200),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };
            destructionOverlay.BeginAnimation(OpacityProperty, flickerAnimation);
        }
    }

    private void CreateRepairEffect(DamageZone zone)
    {
        if (!EnableParticleEffects || _isSimplifiedMode) return;

        var repairCenter = new Point(
            zone.Region.X + zone.Region.Width / 2,
            zone.Region.Y + zone.Region.Height / 2);

        // Repair nanobots visualization
        for (int i = 0; i < 10; i++)
        {
            var nanobot = new Ellipse
            {
                Width = 3,
                Height = 3,
                Fill = new SolidColorBrush(Color.FromRgb(0, 255, 150)),
                Opacity = HolographicIntensity
            };

            var angle = (Math.PI * 2 / 10) * i;
            var startX = repairCenter.X + Math.Cos(angle) * 30;
            var startY = repairCenter.Y + Math.Sin(angle) * 30;

            Canvas.SetLeft(nanobot, startX);
            Canvas.SetTop(nanobot, startY);
            _effectsCanvas.Children.Add(nanobot);

            // Animate nanobots toward center
            var moveXAnimation = new DoubleAnimation
            {
                From = startX,
                To = repairCenter.X,
                Duration = TimeSpan.FromSeconds(1),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            var moveYAnimation = new DoubleAnimation
            {
                From = startY,
                To = repairCenter.Y,
                Duration = TimeSpan.FromSeconds(1),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            var fadeAnimation = new DoubleAnimation
            {
                From = HolographicIntensity,
                To = 0,
                Duration = TimeSpan.FromSeconds(1)
            };

            moveXAnimation.Completed += (s, e) => _effectsCanvas.Children.Remove(nanobot);

            nanobot.BeginAnimation(Canvas.LeftProperty, moveXAnimation);
            nanobot.BeginAnimation(Canvas.TopProperty, moveYAnimation);
            nanobot.BeginAnimation(OpacityProperty, fadeAnimation);
        }
    }

    private void CreateDebrisParticles(Point impact, DamageType damageType, double severity)
    {
        var particleCount = Math.Min(20, (int)(severity * 30));
        var particleColor = GetDamageTypeColor(damageType);

        for (int i = 0; i < particleCount; i++)
        {
            var particle = new Rectangle
            {
                Width = _random.Next(2, 6),
                Height = _random.Next(2, 6),
                Fill = new SolidColorBrush(particleColor),
                Opacity = HolographicIntensity
            };

            Canvas.SetLeft(particle, impact.X);
            Canvas.SetTop(particle, impact.Y);
            _effectsCanvas.Children.Add(particle);

            AnimateDebrisParticle(particle, impact);
        }
    }

    private void AnimateDebrisParticle(UIElement particle, Point origin)
    {
        var angle = _random.NextDouble() * Math.PI * 2;
        var velocity = _random.Next(20, 60);
        var targetX = origin.X + Math.Cos(angle) * velocity;
        var targetY = origin.Y + Math.Sin(angle) * velocity;

        var moveXAnimation = new DoubleAnimation
        {
            From = origin.X,
            To = targetX,
            Duration = TimeSpan.FromMilliseconds(_random.Next(800, 1500)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var moveYAnimation = new DoubleAnimation
        {
            From = origin.Y,
            To = targetY,
            Duration = TimeSpan.FromMilliseconds(_random.Next(800, 1500)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = HolographicIntensity,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(_random.Next(800, 1500))
        };

        moveXAnimation.Completed += (s, e) => _effectsCanvas.Children.Remove(particle);

        particle.BeginAnimation(Canvas.LeftProperty, moveXAnimation);
        particle.BeginAnimation(Canvas.TopProperty, moveYAnimation);
        particle.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void CreateElectricalDischarge(Point center)
    {
        for (int i = 0; i < 5; i++)
        {
            var discharge = CreateLightningBolt(center, _random.Next(30, 80));
            _effectsCanvas.Children.Add(discharge);

            var fadeAnimation = new DoubleAnimation
            {
                From = HolographicIntensity,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                BeginTime = TimeSpan.FromMilliseconds(i * 50)
            };

            fadeAnimation.Completed += (s, e) => _effectsCanvas.Children.Remove(discharge);
            discharge.BeginAnimation(OpacityProperty, fadeAnimation);
        }
    }

    private Path CreateLightningBolt(Point start, double length)
    {
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure { StartPoint = start };
        
        var currentPoint = start;
        var segments = (int)(length / 10);
        
        for (int i = 0; i < segments; i++)
        {
            var nextPoint = new Point(
                currentPoint.X + _random.Next(-5, 15),
                currentPoint.Y + _random.Next(-10, 10));
            
            pathFigure.Segments.Add(new LineSegment(nextPoint, true));
            currentPoint = nextPoint;
        }
        
        pathGeometry.Figures.Add(pathFigure);
        
        return new Path
        {
            Data = pathGeometry,
            Stroke = new SolidColorBrush(Color.FromRgb(100, 200, 255)),
            StrokeThickness = 2,
            Effect = new DropShadowEffect
            {
                Color = Color.FromRgb(100, 200, 255),
                BlurRadius = 8,
                ShadowDepth = 0
            }
        };
    }

    private void CreateExplosionEffect(Point center, double radius)
    {
        var explosion = new Ellipse
        {
            Width = 0,
            Height = 0,
            Fill = new RadialGradientBrush
            {
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(255, 255, 150, 0), 0),
                    new GradientStop(Color.FromArgb(200, 255, 100, 0), 0.7),
                    new GradientStop(Color.FromArgb(0, 255, 0, 0), 1)
                }
            }
        };

        Canvas.SetLeft(explosion, center.X);
        Canvas.SetTop(explosion, center.Y);
        _effectsCanvas.Children.Add(explosion);

        var sizeAnimation = new DoubleAnimation
        {
            From = 0,
            To = radius * 2,
            Duration = TimeSpan.FromMilliseconds(800),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = HolographicIntensity,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(800)
        };

        sizeAnimation.Completed += (s, e) => _effectsCanvas.Children.Remove(explosion);

        explosion.BeginAnimation(WidthProperty, sizeAnimation);
        explosion.BeginAnimation(HeightProperty, sizeAnimation);
        explosion.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void CreateDamagePointVisualization(DamagePoint point)
    {
        var severity = Math.Min(1.0, point.Severity);
        var size = 4 + (severity * 8);

        var damageMarker = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = GetDamageTypeBrush(point.Type, severity),
            Stroke = new SolidColorBrush(Colors.White),
            StrokeThickness = 1,
            Opacity = HolographicIntensity * severity
        };

        Canvas.SetLeft(damageMarker, point.Position.X - size / 2);
        Canvas.SetTop(damageMarker, point.Position.Y - size / 2);
        _heatmapCanvas.Children.Add(damageMarker);

        if (EnableAnimations && !_isSimplifiedMode && point.Severity > 0.5)
        {
            var pulseAnimation = new DoubleAnimation
            {
                From = 0.5,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(2),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };
            damageMarker.BeginAnimation(OpacityProperty, pulseAnimation);
        }
    }

    #endregion

    #region Helper Methods

    private Point GetRandomPointInZone(DamageZone zone)
    {
        return new Point(
            zone.Region.X + _random.NextDouble() * zone.Region.Width,
            zone.Region.Y + _random.NextDouble() * zone.Region.Height);
    }

    private double CalculateAreaDamage(Rect area)
    {
        double totalDamage = 0;
        int pointCount = 0;

        foreach (var zone in _damageZones.Values)
        {
            if (zone.Region.IntersectsWith(area))
            {
                foreach (var point in zone.DamagePattern)
                {
                    if (area.Contains(point.Position))
                    {
                        totalDamage += point.Severity;
                        pointCount++;
                    }
                }
            }
        }

        return pointCount > 0 ? totalDamage / pointCount : 0;
    }

    private List<Line> CreateWireframeLines(Rect region, double integrity)
    {
        var lines = new List<Line>();
        var color = GetZoneStatusColor(integrity);
        var brush = new SolidColorBrush(color);

        // Create grid pattern
        var gridSpacing = 20;
        var dashArray = integrity < 0.5 ? new DoubleCollection { 3, 3 } : null;

        for (double x = region.X; x <= region.X + region.Width; x += gridSpacing)
        {
            var line = new Line
            {
                X1 = x,
                Y1 = region.Y,
                X2 = x,
                Y2 = region.Y + region.Height,
                Stroke = brush,
                StrokeThickness = 1,
                StrokeDashArray = dashArray,
                Opacity = HolographicIntensity * integrity
            };
            lines.Add(line);
        }

        for (double y = region.Y; y <= region.Y + region.Height; y += gridSpacing)
        {
            var line = new Line
            {
                X1 = region.X,
                Y1 = y,
                X2 = region.X + region.Width,
                Y2 = y,
                Stroke = brush,
                StrokeThickness = 1,
                StrokeDashArray = dashArray,
                Opacity = HolographicIntensity * integrity
            };
            lines.Add(line);
        }

        return lines;
    }

    private Brush GetZoneStatusBrush(double integrityPercentage)
    {
        if (integrityPercentage > 0.75)
            return new SolidColorBrush(Color.FromArgb(100, 0, 255, 0));
        else if (integrityPercentage > 0.5)
            return new SolidColorBrush(Color.FromArgb(100, 255, 255, 0));
        else if (integrityPercentage > 0.25)
            return new SolidColorBrush(Color.FromArgb(100, 255, 150, 0));
        else
            return new SolidColorBrush(Color.FromArgb(100, 255, 0, 0));
    }

    private Color GetZoneStatusColor(double integrityPercentage)
    {
        if (integrityPercentage > 0.75)
            return Color.FromRgb(0, 255, 0);
        else if (integrityPercentage > 0.5)
            return Color.FromRgb(255, 255, 0);
        else if (integrityPercentage > 0.25)
            return Color.FromRgb(255, 150, 0);
        else
            return Color.FromRgb(255, 0, 0);
    }

    private Color GetSchematicColor(double integrityPercentage)
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        if (integrityPercentage < 0.5)
            return Color.FromRgb(255, 100, 100);
        else
            return colors.Primary;
    }

    private Brush GetHeatmapBrush(double damage)
    {
        var alpha = (byte)(255 * Math.Min(1.0, damage));
        return new SolidColorBrush(Color.FromArgb(alpha, 255, (byte)(255 * (1 - damage)), 0));
    }

    private Brush GetDamageTypeFlashBrush(DamageType type)
    {
        var color = GetDamageTypeColor(type);
        return new SolidColorBrush(Color.FromArgb(200, color.R, color.G, color.B));
    }

    private Brush GetDamageTypeBrush(DamageType type, double severity)
    {
        var color = GetDamageTypeColor(type);
        var alpha = (byte)(255 * severity);
        return new SolidColorBrush(Color.FromArgb(alpha, color.R, color.G, color.B));
    }

    private Color GetDamageTypeColor(DamageType type)
    {
        return type switch
        {
            DamageType.Kinetic => Color.FromRgb(255, 255, 100),
            DamageType.Thermal => Color.FromRgb(255, 100, 0),
            DamageType.Electromagnetic => Color.FromRgb(100, 150, 255),
            DamageType.Explosive => Color.FromRgb(255, 150, 0),
            DamageType.Energy => Color.FromRgb(255, 0, 255),
            _ => Color.FromRgb(255, 255, 255)
        };
    }

    private void ClearAllEffects()
    {
        _effectsCanvas.Children.Clear();
        _particleEffects.Clear();
        
        foreach (var effects in _damageEffects.Values)
        {
            effects.Clear();
        }
        _damageEffects.Clear();
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableAnimations && !_isSimplifiedMode)
        {
            _animationTimer.Start();
        }

        if (EnableRealTimeDamage)
        {
            _damageTimer.Start();
        }

        UpdateDamageVisualization();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer.Stop();
        _damageTimer.Stop();
        ClearAllEffects();
    }

    private void OnAnimationTimerTick(object sender, EventArgs e)
    {
        if (!EnableAnimations || _isSimplifiedMode) return;

        _animationPhase += 0.1 * AnimationSpeed;
        if (_animationPhase > Math.PI * 2)
            _animationPhase = 0;

        // Update any phase-based animations
        UpdateAnimatedEffects();
    }

    private void OnDamageTimerTick(object sender, EventArgs e)
    {
        if (_damageQueue.Count > 0)
        {
            var damageEvent = _damageQueue.Dequeue();
            ProcessDamageEvent(damageEvent);
        }
    }

    private void UpdateAnimatedEffects()
    {
        // Update any ongoing animated effects based on animation phase
        foreach (var effects in _damageEffects.Values)
        {
            foreach (var effect in effects)
            {
                if (effect is Shape shape && shape.Fill is SolidColorBrush brush)
                {
                    var intensity = (Math.Sin(_animationPhase) + 1) / 2;
                    brush.Opacity = HolographicIntensity * intensity;
                }
            }
        }
    }

    #endregion

    #region Property Change Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDamageVisualization visualization)
        {
            visualization.UpdateDamageVisualization();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDamageVisualization visualization)
        {
            visualization.ApplyEVEColorScheme();
        }
    }

    private static void OnDamageDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDamageVisualization visualization)
        {
            visualization.UpdateDamageVisualization();
        }
    }

    private static void OnVisualizationModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDamageVisualization visualization)
        {
            visualization.UpdateDamageVisualization();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDamageVisualization visualization)
        {
            if ((bool)e.NewValue && !visualization._isSimplifiedMode)
            {
                visualization._animationTimer.Start();
            }
            else
            {
                visualization._animationTimer.Stop();
            }
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDamageVisualization visualization && !(bool)e.NewValue)
        {
            visualization.ClearAllEffects();
        }
    }

    private static void OnShowDamageHeatmapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDamageVisualization visualization)
        {
            visualization.UpdateDamageVisualization();
        }
    }

    private static void OnShowStructuralIntegrityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDamageVisualization visualization)
        {
            visualization.UpdateDamageVisualization();
        }
    }

    private static void OnAnimationSpeedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDamageVisualization visualization)
        {
            var speed = (double)e.NewValue;
            visualization._animationTimer.Interval = TimeSpan.FromMilliseconds(33 / speed);
        }
    }

    private static void OnEnableRealTimeDamageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDamageVisualization visualization)
        {
            if ((bool)e.NewValue)
            {
                visualization._damageTimer.Start();
            }
            else
            {
                visualization._damageTimer.Stop();
            }
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
        ClearAllEffects();
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

        HolographicIntensity = settings.MasterIntensity;
        AnimationSpeed = settings.AnimationSpeed;
        
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
        UpdateDamageVisualization();
    }

    #endregion
}

#region Supporting Classes and Enums

public class HoloDamageData
{
    public Dictionary<string, DamageZone> DamageZones { get; set; } = new();
    public List<DamageEvent> DamageHistory { get; set; } = new();
    public double OverallIntegrity { get; set; } = 100;
    public DateTime LastUpdated { get; set; } = DateTime.Now;
}

public class DamageZone
{
    public string Id { get; set; }
    public string Name { get; set; }
    public Rect Region { get; set; }
    public double CurrentIntegrity { get; set; }
    public double MaxIntegrity { get; set; }
    public List<DamagePoint> DamagePattern { get; set; } = new();
    public bool IsCritical => CurrentIntegrity < MaxIntegrity * 0.25;
    public bool IsDestroyed => CurrentIntegrity <= 0;
}

public class DamagePoint
{
    public Point Position { get; set; }
    public double Severity { get; set; }
    public DamageType Type { get; set; }
    public DateTime Timestamp { get; set; }
}

public class DamageEvent
{
    public string ZoneId { get; set; }
    public double Amount { get; set; }
    public DamageType Type { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsCritical { get; set; }
    public Point ImpactPoint { get; set; }
}

public enum DamageType
{
    Kinetic,
    Thermal,
    Electromagnetic,
    Explosive,
    Energy
}

public enum DamageVisualizationMode
{
    Realistic,
    Schematic,
    Heatmap,
    Wireframe
}

public class DamageEventArgs : EventArgs
{
    public DamageEvent DamageEvent { get; set; }
    public DamageZone Zone { get; set; }
    public DateTime Timestamp { get; set; }
}

#endregion