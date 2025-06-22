// ==========================================================================
// HolographicParticleEngine.cs - Market Data Particle Engine
// ==========================================================================
// Advanced particle engine for market data visualization in the 
// Westworld-EVE fusion interface. Manages particle lifecycle, animations,
// and performance optimization for real-time data streams.
//
// Features:
// - High-performance particle management
// - Real-time market data binding
// - Configurable particle behaviors
// - Performance scaling and optimization
// - Memory-efficient particle pooling
//
// Author: Gideon Development Team
// Created: June 21, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Particle types for market data visualization
/// </summary>
public enum ParticleType
{
    DataStream,
    PricePulse,
    VolumeBurst,
    TradeFlash,
    TrendFlow
}

/// <summary>
/// Market data directions
/// </summary>
public enum MarketDirection
{
    Up,
    Down,
    Neutral
}

/// <summary>
/// Volume levels for particle sizing
/// </summary>
public enum VolumeLevel
{
    Low,
    Medium,
    High
}

/// <summary>
/// Particle data structure
/// </summary>
public class MarketParticle
{
    public ParticleType Type { get; set; }
    public MarketDirection Direction { get; set; }
    public VolumeLevel Volume { get; set; }
    public Point Position { get; set; }
    public Point Velocity { get; set; }
    public double LifeTime { get; set; }
    public double MaxLifeTime { get; set; }
    public Path? Visual { get; set; }
    public Storyboard? Animation { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// High-performance particle engine for market data visualization
/// </summary>
public class HolographicParticleEngine : Canvas
{
    #region Dependency Properties

    public static readonly DependencyProperty MaxParticlesProperty =
        DependencyProperty.Register(nameof(MaxParticles), typeof(int), typeof(HolographicParticleEngine),
            new PropertyMetadata(100, OnMaxParticlesChanged));

    public static readonly DependencyProperty ParticleRateProperty =
        DependencyProperty.Register(nameof(ParticleRate), typeof(double), typeof(HolographicParticleEngine),
            new PropertyMetadata(5.0, OnParticleRateChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HolographicParticleEngine),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty PerformanceModeProperty =
        DependencyProperty.Register(nameof(PerformanceMode), typeof(bool), typeof(HolographicParticleEngine),
            new PropertyMetadata(false, OnPerformanceModeChanged));

    #endregion

    #region Properties

    public int MaxParticles
    {
        get => (int)GetValue(MaxParticlesProperty);
        set => SetValue(MaxParticlesProperty, value);
    }

    public double ParticleRate
    {
        get => (double)GetValue(ParticleRateProperty);
        set => SetValue(ParticleRateProperty, value);
    }

    public bool EnableAnimations
    {
        get => (bool)GetValue(EnableAnimationsProperty);
        set => SetValue(EnableAnimationsProperty, value);
    }

    public bool PerformanceMode
    {
        get => (bool)GetValue(PerformanceModeProperty);
        set => SetValue(PerformanceModeProperty, value);
    }

    #endregion

    #region Fields

    private readonly List<MarketParticle> _activeParticles = new();
    private readonly Queue<MarketParticle> _particlePool = new();
    private readonly Random _random = new();
    private readonly DispatcherTimer _updateTimer;
    private readonly DispatcherTimer _spawnTimer;
    private bool _isRunning = false;

    #endregion

    #region Constructor

    public HolographicParticleEngine()
    {
        ClipToBounds = true;
        IsHitTestVisible = false;
        
        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        _updateTimer.Tick += UpdateParticles;
        
        _spawnTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200) // 5 particles per second default
        };
        _spawnTimer.Tick += SpawnParticle;
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        
        InitializeParticlePool();
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        StartEngine();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopEngine();
    }

    private void UpdateParticles(object? sender, EventArgs e)
    {
        var deltaTime = 0.016; // 16ms frame time
        
        for (int i = _activeParticles.Count - 1; i >= 0; i--)
        {
            var particle = _activeParticles[i];
            
            // Update lifetime
            particle.LifeTime += deltaTime;
            
            // Check if particle should be removed
            if (particle.LifeTime >= particle.MaxLifeTime || !particle.IsActive)
            {
                RemoveParticle(particle);
                continue;
            }
            
            // Update position
            particle.Position = new Point(
                particle.Position.X + particle.Velocity.X * deltaTime,
                particle.Position.Y + particle.Velocity.Y * deltaTime
            );
            
            // Update visual position
            if (particle.Visual != null)
            {
                Canvas.SetLeft(particle.Visual, particle.Position.X);
                Canvas.SetTop(particle.Visual, particle.Position.Y);
                
                // Update opacity based on lifetime
                var alpha = 1.0 - (particle.LifeTime / particle.MaxLifeTime);
                particle.Visual.Opacity = Math.Max(0, alpha);
            }
        }
    }

    private void SpawnParticle(object? sender, EventArgs e)
    {
        if (_activeParticles.Count < MaxParticles)
        {
            CreateDataStreamParticle();
        }
    }

    #endregion

    #region Property Change Handlers

    private static void OnMaxParticlesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HolographicParticleEngine engine)
        {
            engine.AdjustParticleCount();
        }
    }

    private static void OnParticleRateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HolographicParticleEngine engine)
        {
            var rate = (double)e.NewValue;
            engine._spawnTimer.Interval = TimeSpan.FromMilliseconds(Math.Max(50, 1000.0 / rate));
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HolographicParticleEngine engine)
        {
            engine.UpdateAnimationState();
        }
    }

    private static void OnPerformanceModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HolographicParticleEngine engine)
        {
            engine.UpdatePerformanceSettings();
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Start the particle engine
    /// </summary>
    public void StartEngine()
    {
        if (!_isRunning)
        {
            _isRunning = true;
            _updateTimer.Start();
            _spawnTimer.Start();
        }
    }

    /// <summary>
    /// Stop the particle engine
    /// </summary>
    public void StopEngine()
    {
        if (_isRunning)
        {
            _isRunning = false;
            _updateTimer.Stop();
            _spawnTimer.Stop();
            ClearAllParticles();
        }
    }

    /// <summary>
    /// Create a price pulse particle at specific location
    /// </summary>
    public void CreatePricePulse(Point position, MarketDirection direction)
    {
        var particle = GetPooledParticle();
        if (particle == null) return;
        
        particle.Type = ParticleType.PricePulse;
        particle.Direction = direction;
        particle.Position = position;
        particle.Velocity = new Point(0, 0);
        particle.LifeTime = 0;
        particle.MaxLifeTime = 1.2;
        particle.IsActive = true;
        
        SetupParticleVisual(particle);
        StartParticleAnimation(particle, "PricePulse");
        
        _activeParticles.Add(particle);
        Children.Add(particle.Visual!);
    }

    /// <summary>
    /// Create a volume burst particle
    /// </summary>
    public void CreateVolumeBurst(Point position, VolumeLevel volume)
    {
        var particle = GetPooledParticle();
        if (particle == null) return;
        
        particle.Type = ParticleType.VolumeBurst;
        particle.Volume = volume;
        particle.Position = position;
        particle.Velocity = new Point(0, 0);
        particle.LifeTime = 0;
        particle.MaxLifeTime = 0.8;
        particle.IsActive = true;
        
        SetupParticleVisual(particle);
        StartParticleAnimation(particle, "VolumeBurst");
        
        _activeParticles.Add(particle);
        Children.Add(particle.Visual!);
    }

    /// <summary>
    /// Create a trade flash particle
    /// </summary>
    public void CreateTradeFlash(Point position, MarketDirection direction)
    {
        var particle = GetPooledParticle();
        if (particle == null) return;
        
        particle.Type = ParticleType.TradeFlash;
        particle.Direction = direction;
        particle.Position = position;
        particle.Velocity = new Point(0, 0);
        particle.LifeTime = 0;
        particle.MaxLifeTime = 0.3;
        particle.IsActive = true;
        
        SetupParticleVisual(particle);
        StartParticleAnimation(particle, "TradeFlash");
        
        _activeParticles.Add(particle);
        Children.Add(particle.Visual!);
    }

    /// <summary>
    /// Create multiple trend flow particles
    /// </summary>
    public void CreateTrendFlow(Point startPosition, MarketDirection direction, int count = 5)
    {
        for (int i = 0; i < count; i++)
        {
            var particle = GetPooledParticle();
            if (particle == null) break;
            
            particle.Type = ParticleType.TrendFlow;
            particle.Direction = direction;
            particle.Position = new Point(
                startPosition.X + _random.NextDouble() * 20 - 10,
                startPosition.Y + _random.NextDouble() * 20 - 10
            );
            particle.Velocity = new Point(
                _random.NextDouble() * 10 - 5,
                direction == MarketDirection.Up ? -20 : 20
            );
            particle.LifeTime = 0;
            particle.MaxLifeTime = 2.0 + _random.NextDouble();
            particle.IsActive = true;
            
            SetupParticleVisual(particle);
            StartParticleAnimation(particle, direction == MarketDirection.Up ? "TrendFlowUp" : "TrendFlowDown");
            
            _activeParticles.Add(particle);
            Children.Add(particle.Visual!);
        }
    }

    /// <summary>
    /// Clear all particles
    /// </summary>
    public void ClearAllParticles()
    {
        foreach (var particle in _activeParticles)
        {
            if (particle.Visual != null)
            {
                Children.Remove(particle.Visual);
                particle.Animation?.Stop();
            }
            ReturnParticleToPool(particle);
        }
        _activeParticles.Clear();
    }

    #endregion

    #region Private Methods

    private void InitializeParticlePool()
    {
        // Pre-create particle objects for pooling
        for (int i = 0; i < MaxParticles * 2; i++)
        {
            _particlePool.Enqueue(new MarketParticle());
        }
    }

    private MarketParticle? GetPooledParticle()
    {
        if (_particlePool.Count > 0)
        {
            var particle = _particlePool.Dequeue();
            ResetParticle(particle);
            return particle;
        }
        return new MarketParticle();
    }

    private void ReturnParticleToPool(MarketParticle particle)
    {
        ResetParticle(particle);
        _particlePool.Enqueue(particle);
    }

    private void ResetParticle(MarketParticle particle)
    {
        particle.IsActive = false;
        particle.LifeTime = 0;
        particle.Visual = null;
        particle.Animation?.Stop();
        particle.Animation = null;
    }

    private void CreateDataStreamParticle()
    {
        var particle = GetPooledParticle();
        if (particle == null) return;
        
        particle.Type = ParticleType.DataStream;
        particle.Direction = (MarketDirection)_random.Next(0, 3);
        particle.Position = new Point(-10, _random.NextDouble() * ActualHeight);
        particle.Velocity = new Point(100 + _random.NextDouble() * 50, _random.NextDouble() * 10 - 5);
        particle.LifeTime = 0;
        particle.MaxLifeTime = 3.0;
        particle.IsActive = true;
        
        SetupParticleVisual(particle);
        if (EnableAnimations)
        {
            StartParticleAnimation(particle, "DataStreamFlow");
        }
        
        _activeParticles.Add(particle);
        Children.Add(particle.Visual!);
    }

    private void SetupParticleVisual(MarketParticle particle)
    {
        var path = new Path
        {
            IsHitTestVisible = false,
            RenderTransform = new TransformGroup
            {
                Children =
                {
                    new ScaleTransform(1, 1),
                    new TranslateTransform(0, 0)
                }
            },
            RenderTransformOrigin = new Point(0.5, 0.5)
        };
        
        // Set geometry and style based on particle type
        switch (particle.Type)
        {
            case ParticleType.DataStream:
                path.Data = CreateEllipseGeometry(2, 2);
                path.Fill = GetDataStreamBrush(particle.Direction);
                path.Effect = CreateGlowEffect(Colors.Cyan, 6);
                break;
                
            case ParticleType.PricePulse:
                path.Data = CreateEllipseGeometry(3, 3);
                path.Fill = GetDirectionBrush(particle.Direction);
                path.Effect = CreateGlowEffect(GetDirectionColor(particle.Direction), 10);
                break;
                
            case ParticleType.VolumeBurst:
                path.Data = CreateEllipseGeometry(4, 4);
                path.Fill = GetVolumeBrush(particle.Volume);
                path.Effect = CreateGlowEffect(Colors.Gold, 12);
                break;
                
            case ParticleType.TradeFlash:
                path.Data = CreateEllipseGeometry(1.5, 1.5);
                path.Fill = GetDirectionBrush(particle.Direction);
                path.Effect = CreateGlowEffect(GetDirectionColor(particle.Direction), 15);
                break;
                
            case ParticleType.TrendFlow:
                path.Data = CreateArrowGeometry();
                path.Fill = GetDirectionBrush(particle.Direction);
                path.Effect = CreateGlowEffect(GetDirectionColor(particle.Direction), 8);
                break;
        }
        
        Canvas.SetLeft(path, particle.Position.X);
        Canvas.SetTop(path, particle.Position.Y);
        
        particle.Visual = path;
    }

    private void StartParticleAnimation(MarketParticle particle, string animationType)
    {
        if (!EnableAnimations || particle.Visual == null) return;
        
        var storyboard = new Storyboard();
        
        switch (animationType)
        {
            case "PricePulse":
                CreatePricePulseAnimation(storyboard, particle.Visual);
                break;
            case "VolumeBurst":
                CreateVolumeBurstAnimation(storyboard, particle.Visual);
                break;
            case "TradeFlash":
                CreateTradeFlashAnimation(storyboard, particle.Visual);
                break;
            case "TrendFlowUp":
            case "TrendFlowDown":
                CreateTrendFlowAnimation(storyboard, particle.Visual, animationType == "TrendFlowUp");
                break;
            case "DataStreamFlow":
                CreateDataStreamAnimation(storyboard, particle.Visual);
                break;
        }
        
        particle.Animation = storyboard;
        storyboard.Begin();
    }

    private void CreatePricePulseAnimation(Storyboard storyboard, Path visual)
    {
        var transform = visual.RenderTransform as TransformGroup;
        var scaleTransform = transform?.Children[0] as ScaleTransform;
        
        if (scaleTransform != null)
        {
            var scaleX = new DoubleAnimation(0.5, 2.0, TimeSpan.FromSeconds(0.6))
            {
                AutoReverse = true,
                EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 2 }
            };
            
            var scaleY = new DoubleAnimation(0.5, 2.0, TimeSpan.FromSeconds(0.6))
            {
                AutoReverse = true,
                EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 2 }
            };
            
            Storyboard.SetTarget(scaleX, scaleTransform);
            Storyboard.SetTargetProperty(scaleX, new PropertyPath(ScaleTransform.ScaleXProperty));
            
            Storyboard.SetTarget(scaleY, scaleTransform);
            Storyboard.SetTargetProperty(scaleY, new PropertyPath(ScaleTransform.ScaleYProperty));
            
            storyboard.Children.Add(scaleX);
            storyboard.Children.Add(scaleY);
        }
        
        var opacity = new DoubleAnimation(1.0, 0.0, TimeSpan.FromSeconds(1.2));
        Storyboard.SetTarget(opacity, visual);
        Storyboard.SetTargetProperty(opacity, new PropertyPath(UIElement.OpacityProperty));
        storyboard.Children.Add(opacity);
    }

    private void CreateVolumeBurstAnimation(Storyboard storyboard, Path visual)
    {
        var transform = visual.RenderTransform as TransformGroup;
        var scaleTransform = transform?.Children[0] as ScaleTransform;
        
        if (scaleTransform != null)
        {
            var scale = new DoubleAnimation(0, 3.0, TimeSpan.FromSeconds(0.8))
            {
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut }
            };
            
            Storyboard.SetTarget(scale, scaleTransform);
            Storyboard.SetTargetProperty(scale, new PropertyPath(ScaleTransform.ScaleXProperty));
            storyboard.Children.Add(scale);
            
            var scaleY = scale.Clone();
            Storyboard.SetTargetProperty(scaleY, new PropertyPath(ScaleTransform.ScaleYProperty));
            storyboard.Children.Add(scaleY);
        }
        
        var opacity = new DoubleAnimation(1.0, 0.0, TimeSpan.FromSeconds(0.8));
        Storyboard.SetTarget(opacity, visual);
        Storyboard.SetTargetProperty(opacity, new PropertyPath(UIElement.OpacityProperty));
        storyboard.Children.Add(opacity);
    }

    private void CreateTradeFlashAnimation(Storyboard storyboard, Path visual)
    {
        var opacity1 = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.1));
        var opacity2 = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.2)) { BeginTime = TimeSpan.FromSeconds(0.1) };
        
        Storyboard.SetTarget(opacity1, visual);
        Storyboard.SetTargetProperty(opacity1, new PropertyPath(UIElement.OpacityProperty));
        
        Storyboard.SetTarget(opacity2, visual);
        Storyboard.SetTargetProperty(opacity2, new PropertyPath(UIElement.OpacityProperty));
        
        storyboard.Children.Add(opacity1);
        storyboard.Children.Add(opacity2);
    }

    private void CreateTrendFlowAnimation(Storyboard storyboard, Path visual, bool isUp)
    {
        var transform = visual.RenderTransform as TransformGroup;
        var translateTransform = transform?.Children[1] as TranslateTransform;
        
        if (translateTransform != null)
        {
            var move = new DoubleAnimation(
                isUp ? 50 : -50,
                isUp ? -50 : 50,
                TimeSpan.FromSeconds(2))
            {
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            
            Storyboard.SetTarget(move, translateTransform);
            Storyboard.SetTargetProperty(move, new PropertyPath(TranslateTransform.YProperty));
            storyboard.Children.Add(move);
        }
        
        var opacity = new DoubleAnimationUsingKeyFrames();
        opacity.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(0)));
        opacity.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromPercent(0.2)));
        opacity.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromPercent(0.8)));
        opacity.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(1)));
        
        Storyboard.SetTarget(opacity, visual);
        Storyboard.SetTargetProperty(opacity, new PropertyPath(UIElement.OpacityProperty));
        storyboard.Children.Add(opacity);
    }

    private void CreateDataStreamAnimation(Storyboard storyboard, Path visual)
    {
        var transform = visual.RenderTransform as TransformGroup;
        var translateTransform = transform?.Children[1] as TranslateTransform;
        
        if (translateTransform != null)
        {
            var move = new DoubleAnimation(0, 300, TimeSpan.FromSeconds(3))
            {
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            
            Storyboard.SetTarget(move, translateTransform);
            Storyboard.SetTargetProperty(move, new PropertyPath(TranslateTransform.XProperty));
            storyboard.Children.Add(move);
        }
    }

    private void RemoveParticle(MarketParticle particle)
    {
        if (particle.Visual != null)
        {
            Children.Remove(particle.Visual);
        }
        particle.Animation?.Stop();
        _activeParticles.Remove(particle);
        ReturnParticleToPool(particle);
    }

    private void AdjustParticleCount()
    {
        while (_activeParticles.Count > MaxParticles)
        {
            var oldest = _activeParticles[0];
            RemoveParticle(oldest);
        }
    }

    private void UpdateAnimationState()
    {
        if (!EnableAnimations)
        {
            foreach (var particle in _activeParticles)
            {
                particle.Animation?.Stop();
            }
        }
    }

    private void UpdatePerformanceSettings()
    {
        if (PerformanceMode)
        {
            MaxParticles = Math.Min(MaxParticles, 50);
            _updateTimer.Interval = TimeSpan.FromMilliseconds(33); // 30 FPS
        }
        else
        {
            _updateTimer.Interval = TimeSpan.FromMilliseconds(16); // 60 FPS
        }
    }

    #endregion

    #region Helper Methods

    private EllipseGeometry CreateEllipseGeometry(double radiusX, double radiusY)
    {
        return new EllipseGeometry(new Point(0, 0), radiusX, radiusY);
    }

    private PathGeometry CreateArrowGeometry()
    {
        var geometry = new PathGeometry();
        var figure = new PathFigure { StartPoint = new Point(0, 0), IsClosed = true };
        figure.Segments.Add(new LineSegment(new Point(8, 0), true));
        figure.Segments.Add(new LineSegment(new Point(6, 2), true));
        figure.Segments.Add(new LineSegment(new Point(8, 0), true));
        figure.Segments.Add(new LineSegment(new Point(6, -2), true));
        geometry.Figures.Add(figure);
        return geometry;
    }

    private Brush GetDataStreamBrush(MarketDirection direction)
    {
        return direction switch
        {
            MarketDirection.Up => new SolidColorBrush(Colors.LimeGreen),
            MarketDirection.Down => new SolidColorBrush(Colors.Red),
            _ => new SolidColorBrush(Colors.Cyan)
        };
    }

    private Brush GetDirectionBrush(MarketDirection direction)
    {
        return direction switch
        {
            MarketDirection.Up => new SolidColorBrush(Colors.LimeGreen),
            MarketDirection.Down => new SolidColorBrush(Colors.Red),
            _ => new SolidColorBrush(Colors.Cyan)
        };
    }

    private Brush GetVolumeBrush(VolumeLevel volume)
    {
        return volume switch
        {
            VolumeLevel.High => new SolidColorBrush(Colors.Gold),
            VolumeLevel.Medium => new SolidColorBrush(Colors.Cyan),
            _ => new SolidColorBrush(Colors.LightBlue)
        };
    }

    private Color GetDirectionColor(MarketDirection direction)
    {
        return direction switch
        {
            MarketDirection.Up => Colors.LimeGreen,
            MarketDirection.Down => Colors.Red,
            _ => Colors.Cyan
        };
    }

    private System.Windows.Media.Effects.DropShadowEffect CreateGlowEffect(Color color, double radius)
    {
        return new System.Windows.Media.Effects.DropShadowEffect
        {
            Color = color,
            BlurRadius = radius,
            ShadowDepth = 0,
            Opacity = 0.8
        };
    }

    #endregion
}