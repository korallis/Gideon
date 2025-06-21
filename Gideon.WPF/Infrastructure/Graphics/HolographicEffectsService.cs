// ==========================================================================
// HolographicEffectsService.cs - Holographic Effects Management Service
// ==========================================================================
// Provides high-level holographic effects for the Westworld-EVE fusion UI.
// Manages shader effects, animations, and visual states for holographic
// elements throughout the application.
//
// Author: Gideon Development Team
// Created: June 21, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;

namespace Gideon.WPF.Infrastructure.Graphics;

/// <summary>
/// Service for managing holographic effects throughout the application
/// </summary>
public class HolographicEffectsService : IDisposable
{
    private readonly ILogger<HolographicEffectsService> _logger;
    private readonly ShaderManager _shaderManager;
    private readonly DispatcherTimer _animationTimer;
    private readonly Dictionary<FrameworkElement, HolographicState> _holographicElements = new();
    private readonly Random _random = new();
    private bool _disposed;

    // Animation properties
    private double _globalTime;
    private const double AnimationFps = 60.0;
    private const double TimeStep = 1.0 / AnimationFps;

    public HolographicEffectsService(ILogger<HolographicEffectsService> logger, ShaderManager shaderManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _shaderManager = shaderManager ?? throw new ArgumentNullException(nameof(shaderManager));

        // Initialize animation timer
        _animationTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromSeconds(TimeStep)
        };
        _animationTimer.Tick += OnAnimationTick;
        _animationTimer.Start();

        _logger.LogInformation("HolographicEffectsService initialized with {Fps} FPS animation", AnimationFps);
    }

    /// <summary>
    /// Apply holographic glow effect to an element
    /// </summary>
    public void ApplyHolographicGlow(FrameworkElement element, HolographicGlowOptions? options = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(HolographicEffectsService));

        if (element == null)
            throw new ArgumentNullException(nameof(element));

        options ??= new HolographicGlowOptions();

        try
        {
            var glowEffect = _shaderManager.CreateHolographicGlow();
            if (glowEffect != null)
            {
                glowEffect.GlowIntensity = options.Intensity;
                glowEffect.GlowRadius = options.Radius;
                glowEffect.PulseFrequency = options.PulseFrequency;
                glowEffect.GlowColor = options.Color;

                element.Effect = glowEffect;

                // Track element for animation
                var state = GetOrCreateHolographicState(element);
                state.GlowOptions = options;
                state.HasGlow = true;

                _logger.LogDebug("Applied holographic glow to element: {ElementType}", element.GetType().Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply holographic glow to element: {ElementType}", element.GetType().Name);
        }
    }

    /// <summary>
    /// Apply glassmorphism effect to an element
    /// </summary>
    public void ApplyGlassmorphism(FrameworkElement element, GlassmorphismOptions? options = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(HolographicEffectsService));

        if (element == null)
            throw new ArgumentNullException(nameof(element));

        options ??= new GlassmorphismOptions();

        try
        {
            var glassmorphismEffect = _shaderManager.CreateGlassmorphism();
            if (glassmorphismEffect != null)
            {
                glassmorphismEffect.BlurRadius = options.BlurRadius;
                glassmorphismEffect.GlassThickness = options.Thickness;
                glassmorphismEffect.Opacity = options.Opacity;

                element.Effect = glassmorphismEffect;

                // Track element for animation
                var state = GetOrCreateHolographicState(element);
                state.GlassmorphismOptions = options;
                state.HasGlassmorphism = true;

                _logger.LogDebug("Applied glassmorphism to element: {ElementType}", element.GetType().Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply glassmorphism to element: {ElementType}", element.GetType().Name);
        }
    }

    /// <summary>
    /// Apply holographic scanlines effect
    /// </summary>
    public void ApplyHolographicScanlines(FrameworkElement element, ScanlinesOptions? options = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(HolographicEffectsService));

        if (element == null)
            throw new ArgumentNullException(nameof(element));

        options ??= new ScanlinesOptions();

        try
        {
            // Create scanlines overlay
            var scanlinesOverlay = new Rectangle
            {
                Fill = CreateScanlinesBrush(options),
                IsHitTestVisible = false,
                Opacity = options.Opacity
            };

            // Add to parent container
            if (element.Parent is Panel parent)
            {
                parent.Children.Add(scanlinesOverlay);

                // Track for cleanup
                var state = GetOrCreateHolographicState(element);
                state.ScanlinesOverlay = scanlinesOverlay;
                state.ScanlinesOptions = options;
                state.HasScanlines = true;

                _logger.LogDebug("Applied scanlines to element: {ElementType}", element.GetType().Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply scanlines to element: {ElementType}", element.GetType().Name);
        }
    }

    /// <summary>
    /// Create animated data stream effect
    /// </summary>
    public void CreateDataStream(Panel container, DataStreamOptions? options = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(HolographicEffectsService));

        if (container == null)
            throw new ArgumentNullException(nameof(container));

        options ??= new DataStreamOptions();

        try
        {
            for (int i = 0; i < options.ParticleCount; i++)
            {
                var particle = CreateDataParticle(options);
                container.Children.Add(particle);

                // Animate particle
                AnimateDataParticle(particle, container, options);
            }

            _logger.LogDebug("Created data stream with {Count} particles", options.ParticleCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create data stream: {Error}", ex.Message);
        }
    }

    /// <summary>
    /// Apply pulsing animation to holographic elements
    /// </summary>
    public void StartPulsing(FrameworkElement element, double frequency = 2.0, double intensity = 0.3)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(HolographicEffectsService));

        if (element == null)
            throw new ArgumentNullException(nameof(element));

        try
        {
            var animation = new DoubleAnimation
            {
                From = 1.0 - intensity,
                To = 1.0 + intensity,
                Duration = TimeSpan.FromSeconds(1.0 / frequency),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            element.BeginAnimation(UIElement.OpacityProperty, animation);

            var state = GetOrCreateHolographicState(element);
            state.IsPulsing = true;
            state.PulseFrequency = frequency;

            _logger.LogDebug("Started pulsing animation on element: {ElementType}", element.GetType().Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start pulsing animation: {Error}", ex.Message);
        }
    }

    /// <summary>
    /// Remove all holographic effects from an element
    /// </summary>
    public void RemoveEffects(FrameworkElement element)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(HolographicEffectsService));

        if (element == null)
            return;

        try
        {
            // Remove shader effects
            element.Effect = null;

            // Stop animations
            element.BeginAnimation(UIElement.OpacityProperty, null);

            // Remove scanlines overlay
            if (_holographicElements.TryGetValue(element, out var state))
            {
                if (state.ScanlinesOverlay?.Parent is Panel parent)
                {
                    parent.Children.Remove(state.ScanlinesOverlay);
                }

                _holographicElements.Remove(element);
            }

            _logger.LogDebug("Removed holographic effects from element: {ElementType}", element.GetType().Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove effects from element: {Error}", ex.Message);
        }
    }

    #region Private Methods

    private HolographicState GetOrCreateHolographicState(FrameworkElement element)
    {
        if (!_holographicElements.TryGetValue(element, out var state))
        {
            state = new HolographicState();
            _holographicElements[element] = state;
        }
        return state;
    }

    private void OnAnimationTick(object? sender, EventArgs e)
    {
        _globalTime += TimeStep;

        // Update holographic elements
        UpdateHolographicElements();

        // Update data particles
        UpdateDataParticles();
    }

    private void UpdateHolographicElements()
    {
        foreach (var kvp in _holographicElements.ToList())
        {
            var element = kvp.Key;
            var state = kvp.Value;

            try
            {
                // Update glow effects
                if (state.HasGlow && element.Effect is HolographicGlowEffect glow)
                {
                    // Update time-based properties
                    // glow.Time = _globalTime; // Would be set if shader supported it
                }

                // Update glassmorphism effects
                if (state.HasGlassmorphism && element.Effect is GlassmorphismEffect glassmorphism)
                {
                    // Update animated properties
                    // glassmorphism.Time = _globalTime; // Would be set if shader supported it
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error updating holographic element: {ElementType}", element.GetType().Name);
            }
        }
    }

    private void UpdateDataParticles()
    {
        // Update any active data particle animations
        // This would track and update particle positions, colors, etc.
    }

    private Brush CreateScanlinesBrush(ScanlinesOptions options)
    {
        var gradient = new LinearGradientBrush();
        gradient.StartPoint = new Point(0, 0);
        gradient.EndPoint = new Point(0, 1);

        // Create scanline pattern
        for (int i = 0; i < 20; i++)
        {
            var position = i / 19.0;
            var alpha = (i % 2 == 0) ? options.Intensity : 0.0;
            
            gradient.GradientStops.Add(new GradientStop(
                Color.FromArgb((byte)(alpha * 255), options.Color.R, options.Color.G, options.Color.B),
                position));
        }

        return gradient;
    }

    private Rectangle CreateDataParticle(DataStreamOptions options)
    {
        var particle = new Rectangle
        {
            Width = options.ParticleSize,
            Height = options.ParticleSize,
            Fill = new SolidColorBrush(options.ParticleColor),
            Opacity = options.ParticleOpacity
        };

        // Apply glow effect
        if (options.EnableGlow)
        {
            particle.Effect = new DropShadowEffect
            {
                Color = options.ParticleColor,
                BlurRadius = options.ParticleSize * 2,
                ShadowDepth = 0,
                Opacity = 0.8
            };
        }

        return particle;
    }

    private void AnimateDataParticle(Rectangle particle, Panel container, DataStreamOptions options)
    {
        var startX = _random.NextDouble() * container.ActualWidth;
        var startY = container.ActualHeight + options.ParticleSize;
        var endY = -options.ParticleSize;

        Canvas.SetLeft(particle, startX);
        Canvas.SetTop(particle, startY);

        var animation = new DoubleAnimation
        {
            From = startY,
            To = endY,
            Duration = TimeSpan.FromSeconds(options.StreamDuration),
            EasingFunction = new LinearEasing()
        };

        animation.Completed += (s, e) =>
        {
            container.Children.Remove(particle);
            // Create new particle to maintain count
            var newParticle = CreateDataParticle(options);
            container.Children.Add(newParticle);
            AnimateDataParticle(newParticle, container, options);
        };

        particle.BeginAnimation(Canvas.TopProperty, animation);
    }

    #endregion

    public void Dispose()
    {
        if (!_disposed)
        {
            _animationTimer?.Stop();
            
            foreach (var element in _holographicElements.Keys.ToList())
            {
                RemoveEffects(element);
            }
            
            _holographicElements.Clear();
            _disposed = true;
            
            _logger.LogInformation("HolographicEffectsService disposed");
        }
    }
}

#region Supporting Classes

/// <summary>
/// State tracking for holographic elements
/// </summary>
internal class HolographicState
{
    public bool HasGlow { get; set; }
    public bool HasGlassmorphism { get; set; }
    public bool HasScanlines { get; set; }
    public bool IsPulsing { get; set; }
    public double PulseFrequency { get; set; }
    public Rectangle? ScanlinesOverlay { get; set; }
    public HolographicGlowOptions? GlowOptions { get; set; }
    public GlassmorphismOptions? GlassmorphismOptions { get; set; }
    public ScanlinesOptions? ScanlinesOptions { get; set; }
}

/// <summary>
/// Options for holographic glow effects
/// </summary>
public class HolographicGlowOptions
{
    public double Intensity { get; set; } = 1.0;
    public double Radius { get; set; } = 10.0;
    public double PulseFrequency { get; set; } = 2.0;
    public Color Color { get; set; } = Color.FromArgb(255, 0, 212, 255); // EVE Cyan
}

/// <summary>
/// Options for glassmorphism effects
/// </summary>
public class GlassmorphismOptions
{
    public double BlurRadius { get; set; } = 15.0;
    public double Thickness { get; set; } = 0.1;
    public double Opacity { get; set; } = 0.8;
    public Color TintColor { get; set; } = Color.FromArgb(25, 0, 212, 255);
}

/// <summary>
/// Options for scanlines effects
/// </summary>
public class ScanlinesOptions
{
    public double Intensity { get; set; } = 0.3;
    public double Opacity { get; set; } = 0.1;
    public Color Color { get; set; } = Color.FromArgb(255, 0, 212, 255);
}

/// <summary>
/// Options for data stream effects
/// </summary>
public class DataStreamOptions
{
    public int ParticleCount { get; set; } = 20;
    public double ParticleSize { get; set; } = 3.0;
    public double ParticleOpacity { get; set; } = 0.8;
    public Color ParticleColor { get; set; } = Color.FromArgb(255, 0, 212, 255);
    public double StreamDuration { get; set; } = 3.0;
    public bool EnableGlow { get; set; } = true;
}

/// <summary>
/// Linear easing function for animations
/// </summary>
public class LinearEasing : IEasingFunction
{
    public double Ease(double normalizedTime) => normalizedTime;
}

#endregion