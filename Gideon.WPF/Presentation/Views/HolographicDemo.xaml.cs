// ==========================================================================
// HolographicDemo.xaml.cs - Holographic Effects Demonstration Page
// ==========================================================================
// Code-behind for the holographic effects demonstration page. Implements
// interactive controls and real-time effects updates for showcasing the
// Westworld-EVE fusion interface capabilities.
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
using Gideon.WPF.Infrastructure.Extensions;
using Gideon.WPF.Infrastructure.Graphics;
using Microsoft.Extensions.Logging;

namespace Gideon.WPF.Presentation.Views;

/// <summary>
/// Interaction logic for HolographicDemo.xaml
/// </summary>
public partial class HolographicDemo : Page
{
    private readonly ILogger<HolographicDemo>? _logger;
    private readonly DispatcherTimer _particleAnimationTimer;
    private readonly List<UIElement> _dataParticles = new();
    private readonly Random _random = new();
    private double _animationTime;

    public HolographicDemo()
    {
        InitializeComponent();
        
        // Initialize particle animation timer
        _particleAnimationTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        _particleAnimationTimer.Tick += OnParticleAnimationTick;
        
        Loaded += OnPageLoaded;
        Unloaded += OnPageUnloaded;
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            InitializeHolographicEffects();
            StartAnimations();
            SetupInteractiveControls();
            CreateDataParticles();
            
            _particleAnimationTimer.Start();
            
            _logger?.LogInformation("Holographic demo page loaded successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to initialize holographic demo page");
        }
    }

    private void OnPageUnloaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _particleAnimationTimer?.Stop();
            CleanupAnimations();
            
            _logger?.LogInformation("Holographic demo page unloaded");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during holographic demo page cleanup");
        }
    }

    /// <summary>
    /// Initialize holographic effects on UI elements
    /// </summary>
    private void InitializeHolographicEffects()
    {
        try
        {
            // Apply holographic effects to demonstration elements
            // Note: In a real implementation, these would use the HolographicEffectsService
            
            // Apply enhanced glow to status indicators
            var statusIndicators = FindVisualChildren<Ellipse>(this);
            foreach (var indicator in statusIndicators)
            {
                if (indicator.Fill is SolidColorBrush brush)
                {
                    // Enhance with pulsing animation
                    var pulseAnimation = new DoubleAnimation
                    {
                        From = 0.6,
                        To = 1.0,
                        Duration = TimeSpan.FromSeconds(2),
                        RepeatBehavior = RepeatBehavior.Forever,
                        AutoReverse = true,
                        EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                    };
                    
                    indicator.BeginAnimation(UIElement.OpacityProperty, pulseAnimation);
                }
            }
            
            _logger?.LogDebug("Holographic effects initialized");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to initialize holographic effects");
        }
    }

    /// <summary>
    /// Start demonstration animations
    /// </summary>
    private void StartAnimations()
    {
        try
        {
            // Start particle animations for existing elements
            StartParticleMovement(Particle1Transform, 0, -200, 4.0);
            StartParticleMovement(Particle2Transform, 0, -200, 3.5, 0.5);
            StartParticleMovement(Particle3Transform, 0, -200, 4.2, 1.0);
            
            _logger?.LogDebug("Demonstration animations started");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to start animations");
        }
    }

    /// <summary>
    /// Setup interactive controls for real-time effect adjustment
    /// </summary>
    private void SetupInteractiveControls()
    {
        try
        {
            // Wire up slider events for real-time effect updates
            if (GlowIntensitySlider != null)
            {
                GlowIntensitySlider.ValueChanged += OnGlowIntensityChanged;
            }
            
            if (BlurRadiusSlider != null)
            {
                BlurRadiusSlider.ValueChanged += OnBlurRadiusChanged;
            }
            
            if (ScanlinesCheckbox != null)
            {
                ScanlinesCheckbox.Checked += OnScanlinesToggled;
                ScanlinesCheckbox.Unchecked += OnScanlinesToggled;
            }
            
            _logger?.LogDebug("Interactive controls initialized");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to setup interactive controls");
        }
    }

    /// <summary>
    /// Create additional data particles for animation
    /// </summary>
    private void CreateDataParticles()
    {
        try
        {
            var canvas = DataStreamCanvas;
            if (canvas == null) return;

            // Create additional animated particles
            for (int i = 0; i < 8; i++)
            {
                var particle = new Rectangle
                {
                    Width = 2,
                    Height = _random.Next(6, 12),
                    Fill = i % 2 == 0 ? 
                        new SolidColorBrush(Color.FromArgb(255, 0, 212, 255)) : // EVE Cyan
                        new SolidColorBrush(Color.FromArgb(255, 255, 215, 0)),   // EVE Gold
                    RenderTransform = new TranslateTransform(),
                    Opacity = 0.8
                };

                // Add glow effect
                particle.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = ((SolidColorBrush)particle.Fill).Color,
                    BlurRadius = 8,
                    ShadowDepth = 0,
                    Opacity = 0.8
                };

                // Position randomly
                Canvas.SetLeft(particle, _random.Next(20, (int)canvas.Width - 20));
                Canvas.SetTop(particle, canvas.Height + particle.Height);

                canvas.Children.Add(particle);
                _dataParticles.Add(particle);

                // Start animation with random delay
                var delay = _random.NextDouble() * 2.0;
                StartParticleMovement(particle.RenderTransform as TranslateTransform, 
                                    0, -(canvas.Height + particle.Height * 2), 
                                    3.0 + _random.NextDouble() * 2.0, delay);
            }
            
            _logger?.LogDebug("Created {Count} data particles", _dataParticles.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to create data particles");
        }
    }

    /// <summary>
    /// Start particle movement animation
    /// </summary>
    private void StartParticleMovement(TranslateTransform? transform, double fromY, double toY, double duration, double delay = 0)
    {
        if (transform == null) return;

        var animation = new DoubleAnimation
        {
            From = fromY,
            To = toY,
            Duration = TimeSpan.FromSeconds(duration),
            RepeatBehavior = RepeatBehavior.Forever,
            BeginTime = TimeSpan.FromSeconds(delay),
            EasingFunction = new LinearEase()
        };

        transform.BeginAnimation(TranslateTransform.YProperty, animation);
    }

    /// <summary>
    /// Handle particle animation updates
    /// </summary>
    private void OnParticleAnimationTick(object? sender, EventArgs e)
    {
        _animationTime += 0.016; // ~16ms

        try
        {
            // Update any custom particle behaviors here
            UpdateCustomParticleEffects();
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error in particle animation update");
        }
    }

    /// <summary>
    /// Update custom particle effects
    /// </summary>
    private void UpdateCustomParticleEffects()
    {
        // Add any custom particle behaviors, such as:
        // - Physics simulation
        // - Color transitions
        // - Dynamic spawning/despawning
        // - Interaction with other elements
    }

    #region Event Handlers

    private void OnGlowIntensityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        try
        {
            var intensity = e.NewValue;
            
            // Update glow effects throughout the page
            // In a real implementation, this would update shader parameters
            
            _logger?.LogDebug("Glow intensity changed to {Intensity}", intensity);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to update glow intensity");
        }
    }

    private void OnBlurRadiusChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        try
        {
            var radius = e.NewValue;
            
            // Update glassmorphism blur radius
            // In a real implementation, this would update shader parameters
            
            _logger?.LogDebug("Blur radius changed to {Radius}", radius);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to update blur radius");
        }
    }

    private void OnScanlinesToggled(object sender, RoutedEventArgs e)
    {
        try
        {
            var isEnabled = ScanlinesCheckbox?.IsChecked == true;
            
            // Toggle scanlines effect
            // In a real implementation, this would toggle shader effects
            
            _logger?.LogDebug("Scanlines toggled: {Enabled}", isEnabled);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to toggle scanlines");
        }
    }

    #endregion

    #region Cleanup

    private void CleanupAnimations()
    {
        try
        {
            // Stop all animations
            var transforms = FindVisualChildren<TranslateTransform>(this);
            foreach (var transform in transforms)
            {
                transform.BeginAnimation(TranslateTransform.YProperty, null);
            }
            
            var indicators = FindVisualChildren<Ellipse>(this);
            foreach (var indicator in indicators)
            {
                indicator.BeginAnimation(UIElement.OpacityProperty, null);
            }
            
            _dataParticles.Clear();
            
            _logger?.LogDebug("Animations cleaned up");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during animation cleanup");
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Find visual children of a specific type
    /// </summary>
    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent == null) yield break;

        var childCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            
            if (child is T typedChild)
                yield return typedChild;

            foreach (var descendant in FindVisualChildren<T>(child))
                yield return descendant;
        }
    }

    #endregion
}

/// <summary>
/// Linear easing function for smooth animations
/// </summary>
public class LinearEase : IEasingFunction
{
    public double Ease(double normalizedTime) => normalizedTime;
}