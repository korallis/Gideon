// ==========================================================================
// PerformanceOptimizedRenderer.cs - Performance-Optimized Particle Renderer
// ==========================================================================
// High-performance particle rendering system with hardware acceleration,
// level-of-detail scaling, and adaptive quality management.
//
// Author: Gideon Development Team
// Created: June 21, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Performance levels for adaptive rendering
/// </summary>
public enum PerformanceLevel
{
    Maximum = 0,    // High-end hardware
    High = 1,       // Mid-range hardware
    Medium = 2,     // Lower-end hardware
    Low = 3         // Minimal hardware
}

/// <summary>
/// High-performance particle renderer with adaptive quality
/// </summary>
public class PerformanceOptimizedRenderer
{
    #region Properties

    public PerformanceLevel CurrentPerformanceLevel { get; private set; } = PerformanceLevel.High;
    public int MaxParticleCount => GetMaxParticleCount();
    public double TargetFrameRate => GetTargetFrameRate();
    public bool UseHardwareAcceleration { get; private set; } = true;

    #endregion

    #region Fields

    private readonly DispatcherTimer _performanceMonitor;
    private readonly Queue<double> _frameTimeHistory = new();
    private double _averageFrameTime = 16.67; // 60 FPS baseline
    private const int FrameHistorySize = 30;

    #endregion

    #region Constructor

    public PerformanceOptimizedRenderer()
    {
        _performanceMonitor = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _performanceMonitor.Tick += MonitorPerformance;
        _performanceMonitor.Start();

        DetectHardwareCapabilities();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Update frame time for performance monitoring
    /// </summary>
    public void UpdateFrameTime(double frameTime)
    {
        _frameTimeHistory.Enqueue(frameTime);
        
        if (_frameTimeHistory.Count > FrameHistorySize)
        {
            _frameTimeHistory.Dequeue();
        }

        // Calculate moving average
        double sum = 0;
        foreach (var time in _frameTimeHistory)
        {
            sum += time;
        }
        _averageFrameTime = sum / _frameTimeHistory.Count;
    }

    /// <summary>
    /// Get optimized particle configuration for current performance level
    /// </summary>
    public ParticleConfig GetOptimizedConfig()
    {
        return CurrentPerformanceLevel switch
        {
            PerformanceLevel.Maximum => new ParticleConfig
            {
                MaxParticles = 500,
                EnableBlur = true,
                EnableGlow = true,
                EnableAnimations = true,
                ParticleComplexity = 1.0,
                UpdateFrequency = 60
            },
            PerformanceLevel.High => new ParticleConfig
            {
                MaxParticles = 200,
                EnableBlur = true,
                EnableGlow = true,
                EnableAnimations = true,
                ParticleComplexity = 0.8,
                UpdateFrequency = 60
            },
            PerformanceLevel.Medium => new ParticleConfig
            {
                MaxParticles = 100,
                EnableBlur = false,
                EnableGlow = true,
                EnableAnimations = true,
                ParticleComplexity = 0.6,
                UpdateFrequency = 30
            },
            PerformanceLevel.Low => new ParticleConfig
            {
                MaxParticles = 50,
                EnableBlur = false,
                EnableGlow = false,
                EnableAnimations = false,
                ParticleComplexity = 0.3,
                UpdateFrequency = 30
            },
            _ => new ParticleConfig()
        };
    }

    /// <summary>
    /// Manually set performance level
    /// </summary>
    public void SetPerformanceLevel(PerformanceLevel level)
    {
        CurrentPerformanceLevel = level;
        OnPerformanceLevelChanged?.Invoke(level);
    }

    #endregion

    #region Events

    public event Action<PerformanceLevel>? OnPerformanceLevelChanged;

    #endregion

    #region Private Methods

    private void DetectHardwareCapabilities()
    {
        try
        {
            // Check for hardware acceleration support
            var renderCapability = RenderCapability.Tier;
            UseHardwareAcceleration = renderCapability > 0;
            
            // Initial performance level based on hardware
            CurrentPerformanceLevel = renderCapability switch
            {
                2 => PerformanceLevel.Maximum,
                1 => PerformanceLevel.High,
                _ => PerformanceLevel.Medium
            };
        }
        catch
        {
            CurrentPerformanceLevel = PerformanceLevel.Low;
            UseHardwareAcceleration = false;
        }
    }

    private void MonitorPerformance(object? sender, EventArgs e)
    {
        if (_frameTimeHistory.Count < FrameHistorySize) return;

        // Adjust performance level based on frame rate
        var currentFPS = 1000.0 / _averageFrameTime;
        var targetFPS = GetTargetFrameRate();

        if (currentFPS < targetFPS * 0.8) // 20% below target
        {
            // Decrease performance level
            if (CurrentPerformanceLevel < PerformanceLevel.Low)
            {
                CurrentPerformanceLevel++;
                OnPerformanceLevelChanged?.Invoke(CurrentPerformanceLevel);
            }
        }
        else if (currentFPS > targetFPS * 1.2) // 20% above target
        {
            // Increase performance level
            if (CurrentPerformanceLevel > PerformanceLevel.Maximum)
            {
                CurrentPerformanceLevel--;
                OnPerformanceLevelChanged?.Invoke(CurrentPerformanceLevel);
            }
        }
    }

    private int GetMaxParticleCount()
    {
        return CurrentPerformanceLevel switch
        {
            PerformanceLevel.Maximum => 500,
            PerformanceLevel.High => 200,
            PerformanceLevel.Medium => 100,
            PerformanceLevel.Low => 50,
            _ => 100
        };
    }

    private double GetTargetFrameRate()
    {
        return CurrentPerformanceLevel switch
        {
            PerformanceLevel.Maximum => 60.0,
            PerformanceLevel.High => 60.0,
            PerformanceLevel.Medium => 30.0,
            PerformanceLevel.Low => 30.0,
            _ => 30.0
        };
    }

    /// <summary>
    /// Get level-of-detail configuration for distance-based optimization
    /// </summary>
    public LODConfig GetLODConfig(double distance)
    {
        var normalizedDistance = Math.Clamp(distance / 1000.0, 0.0, 1.0); // Normalize to 0-1 range
        
        return new LODConfig
        {
            ParticleScale = Math.Max(0.1, 1.0 - normalizedDistance * 0.8),
            UpdateRate = normalizedDistance < 0.3 ? 1.0 : (normalizedDistance < 0.6 ? 0.5 : 0.25),
            EnableEffects = normalizedDistance < 0.5,
            MaxComplexity = Math.Max(0.1, 1.0 - normalizedDistance * 0.7)
        };
    }

    /// <summary>
    /// Enable or disable performance monitoring
    /// </summary>
    public void SetPerformanceMonitoring(bool enabled)
    {
        if (enabled)
            _performanceMonitor.Start();
        else
            _performanceMonitor.Stop();
    }

    /// <summary>
    /// Get memory usage optimization settings
    /// </summary>
    public MemoryOptimizationConfig GetMemoryConfig()
    {
        return CurrentPerformanceLevel switch
        {
            PerformanceLevel.Maximum => new MemoryOptimizationConfig
            {
                ParticlePoolSize = 1000,
                EnableObjectPooling = true,
                TextureCacheSize = 50,
                GeometryCacheSize = 100,
                GCCollectionMode = System.Runtime.GCCollectionMode.Optimized
            },
            PerformanceLevel.High => new MemoryOptimizationConfig
            {
                ParticlePoolSize = 500,
                EnableObjectPooling = true,
                TextureCacheSize = 30,
                GeometryCacheSize = 50,
                GCCollectionMode = System.Runtime.GCCollectionMode.Default
            },
            PerformanceLevel.Medium => new MemoryOptimizationConfig
            {
                ParticlePoolSize = 250,
                EnableObjectPooling = true,
                TextureCacheSize = 20,
                GeometryCacheSize = 25,
                GCCollectionMode = System.Runtime.GCCollectionMode.Default
            },
            PerformanceLevel.Low => new MemoryOptimizationConfig
            {
                ParticlePoolSize = 100,
                EnableObjectPooling = false,
                TextureCacheSize = 10,
                GeometryCacheSize = 10,
                GCCollectionMode = System.Runtime.GCCollectionMode.Forced
            },
            _ => new MemoryOptimizationConfig()
        };
    }

    /// <summary>
    /// Get current performance metrics
    /// </summary>
    public PerformanceMetrics GetCurrentMetrics()
    {
        return new PerformanceMetrics
        {
            AverageFrameTime = _averageFrameTime,
            CurrentFPS = 1000.0 / _averageFrameTime,
            PerformanceLevel = CurrentPerformanceLevel,
            FrameTimeHistory = new Queue<double>(_frameTimeHistory),
            HardwareAcceleration = UseHardwareAcceleration
        };
    }

    /// <summary>
    /// Force garbage collection for memory management
    /// </summary>
    public void OptimizeMemory()
    {
        var config = GetMemoryConfig();
        if (config.GCCollectionMode == System.Runtime.GCCollectionMode.Forced)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        else if (config.GCCollectionMode == System.Runtime.GCCollectionMode.Default)
        {
            GC.Collect(0, GCCollectionMode.Optimized);
        }
    }

    #endregion
}

/// <summary>
/// Particle configuration for performance optimization
/// </summary>
public class ParticleConfig
{
    public int MaxParticles { get; set; } = 100;
    public bool EnableBlur { get; set; } = true;
    public bool EnableGlow { get; set; } = true;
    public bool EnableAnimations { get; set; } = true;
    public double ParticleComplexity { get; set; } = 1.0;
    public int UpdateFrequency { get; set; } = 60;
    public bool EnableBatching { get; set; } = true;
    public bool EnableCulling { get; set; } = true;
    public double CullingDistance { get; set; } = 1000.0;
}

/// <summary>
/// Level-of-detail configuration for distance-based optimization
/// </summary>
public class LODConfig
{
    public double ParticleScale { get; set; } = 1.0;
    public double UpdateRate { get; set; } = 1.0;
    public bool EnableEffects { get; set; } = true;
    public double MaxComplexity { get; set; } = 1.0;
}

/// <summary>
/// Memory optimization configuration
/// </summary>
public class MemoryOptimizationConfig
{
    public int ParticlePoolSize { get; set; } = 500;
    public bool EnableObjectPooling { get; set; } = true;
    public int TextureCacheSize { get; set; } = 30;
    public int GeometryCacheSize { get; set; } = 50;
    public System.Runtime.GCCollectionMode GCCollectionMode { get; set; } = System.Runtime.GCCollectionMode.Default;
}

/// <summary>
/// Performance metrics for monitoring and debugging
/// </summary>
public class PerformanceMetrics
{
    public double AverageFrameTime { get; set; }
    public double CurrentFPS { get; set; }
    public PerformanceLevel PerformanceLevel { get; set; }
    public Queue<double> FrameTimeHistory { get; set; } = new();
    public bool HardwareAcceleration { get; set; }
    public long MemoryUsage { get; set; }
    public int ActiveParticles { get; set; }
}