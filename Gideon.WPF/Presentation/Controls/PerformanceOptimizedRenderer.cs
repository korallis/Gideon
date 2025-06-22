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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

    // GPU and Parallel Processing
    private readonly ConcurrentQueue<ParticleUpdateBatch> _updateQueue = new();
    private readonly SemaphoreSlim _renderSemaphore = new(1, 1);
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Task _parallelUpdateTask;
    private readonly int _maxConcurrency;

    // Caching System
    private readonly ConcurrentDictionary<string, CachedRenderData> _renderCache = new();
    private readonly Timer _cacheCleanupTimer;
    private const int CacheExpirationMinutes = 5;
    private const int MaxCacheEntries = 1000;

    // Memory Management
    private readonly ObjectPool<ParticleUpdateBatch> _batchPool;
    private long _lastMemoryCleanup = Environment.TickCount64;
    private const long MemoryCleanupInterval = 30000; // 30 seconds

    // Performance Tracking
    private readonly PerformanceCounter _gpuUsageCounter;
    private readonly PerformanceCounter _memoryUsageCounter;
    private volatile bool _isGpuAccelerationAvailable;
    private volatile int _activeParticleCount;

    #endregion

    #region Constructor

    public PerformanceOptimizedRenderer()
    {
        _maxConcurrency = Math.Max(2, Environment.ProcessorCount / 2);
        _batchPool = new ObjectPool<ParticleUpdateBatch>(() => new ParticleUpdateBatch(), maxSize: 100);

        _performanceMonitor = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _performanceMonitor.Tick += MonitorPerformance;
        _performanceMonitor.Start();

        // Initialize performance counters
        _gpuUsageCounter = InitializePerformanceCounter("GPU Engine", "Utilization Percentage", "_Total");
        _memoryUsageCounter = InitializePerformanceCounter("Memory", "Available MBytes", null);

        // Start cache cleanup timer
        _cacheCleanupTimer = new Timer(CleanupCache, null, 
            TimeSpan.FromMinutes(CacheExpirationMinutes), 
            TimeSpan.FromMinutes(CacheExpirationMinutes));

        // Start parallel processing task
        _parallelUpdateTask = Task.Run(ParallelParticleUpdateLoop, _cancellationTokenSource.Token);

        DetectHardwareCapabilities();
        InitializeGpuAcceleration();
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

    /// <summary>
    /// Queue particle batch for parallel processing
    /// </summary>
    public void QueueParticleUpdate(ParticleUpdateBatch batch)
    {
        if (batch != null)
        {
            _updateQueue.Enqueue(batch);
        }
    }

    /// <summary>
    /// Get current GPU utilization percentage
    /// </summary>
    public float GetGpuUtilization()
    {
        try
        {
            return _gpuUsageCounter?.NextValue() ?? 0f;
        }
        catch
        {
            return 0f;
        }
    }

    /// <summary>
    /// Get cached render data or create new entry
    /// </summary>
    public CachedRenderData GetCachedRenderData(string key, Func<CachedRenderData> factory)
    {
        return _renderCache.GetOrAdd(key, _ => factory());
    }

    /// <summary>
    /// Force immediate memory optimization
    /// </summary>
    public void ForceMemoryOptimization()
    {
        OptimizeMemory();
        CleanupCache(null);
        CleanupUnusedResources();
    }

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

    private PerformanceCounter InitializePerformanceCounter(string category, string counter, string instance)
    {
        try
        {
            return new PerformanceCounter(category, counter, instance);
        }
        catch
        {
            return null; // Performance counter not available
        }
    }

    private void InitializeGpuAcceleration()
    {
        try
        {
            // Check for hardware acceleration capability
            var hwndSource = PresentationSource.FromVisual(Application.Current.MainWindow) as HwndSource;
            if (hwndSource?.CompositionTarget != null)
            {
                _isGpuAccelerationAvailable = RenderCapability.IsDisplayed(Application.Current.MainWindow) &&
                                              RenderCapability.Tier > 0;
            }
        }
        catch
        {
            _isGpuAccelerationAvailable = false;
        }
    }

    private async Task ParallelParticleUpdateLoop()
    {
        var concurrencyOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = _maxConcurrency,
            CancellationToken = _cancellationTokenSource.Token
        };

        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                var batches = new List<ParticleUpdateBatch>();
                
                // Collect batches for processing
                while (_updateQueue.TryDequeue(out var batch) && batches.Count < _maxConcurrency * 2)
                {
                    batches.Add(batch);
                }

                if (batches.Count > 0)
                {
                    await _renderSemaphore.WaitAsync(_cancellationTokenSource.Token);
                    try
                    {
                        // Process batches in parallel
                        await Parallel.ForEachAsync(batches, concurrencyOptions, async (batch, ct) =>
                        {
                            await ProcessParticleBatch(batch, ct);
                            _batchPool.Return(batch);
                        });
                    }
                    finally
                    {
                        _renderSemaphore.Release();
                    }
                }
                else
                {
                    // No work available, yield control
                    await Task.Delay(1, _cancellationTokenSource.Token);
                }

                // Periodic memory cleanup
                if (Environment.TickCount64 - _lastMemoryCleanup > MemoryCleanupInterval)
                {
                    OptimizeMemory();
                    _lastMemoryCleanup = Environment.TickCount64;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in parallel particle update: {ex.Message}");
                await Task.Delay(100, _cancellationTokenSource.Token);
            }
        }
    }

    private async Task ProcessParticleBatch(ParticleUpdateBatch batch, CancellationToken cancellationToken)
    {
        if (batch?.Particles == null) return;

        await Task.Run(() =>
        {
            foreach (var particle in batch.Particles)
            {
                if (cancellationToken.IsCancellationRequested) break;

                // Update particle physics
                particle.X += particle.VelocityX * batch.DeltaTime;
                particle.Y += particle.VelocityY * batch.DeltaTime;
                particle.Life -= batch.DeltaTime / particle.MaxLife;

                // Apply performance-based optimizations
                var config = GetOptimizedConfig();
                if (config.EnableAnimations)
                {
                    particle.Rotation += particle.RotationSpeed * batch.DeltaTime;
                    particle.Scale = Math.Max(0.1, particle.Scale + particle.ScaleSpeed * batch.DeltaTime);
                }

                // Update active particle count
                Interlocked.Increment(ref _activeParticleCount);
            }
        }, cancellationToken);
    }

    private void CleanupCache(object state)
    {
        var expiredKeys = new List<string>();
        var now = DateTime.UtcNow;

        foreach (var kvp in _renderCache)
        {
            if (now - kvp.Value.CreationTime > TimeSpan.FromMinutes(CacheExpirationMinutes))
            {
                expiredKeys.Add(kvp.Key);
            }
        }

        foreach (var key in expiredKeys)
        {
            _renderCache.TryRemove(key, out _);
        }

        // Enforce max cache size
        if (_renderCache.Count > MaxCacheEntries)
        {
            var oldestEntries = _renderCache
                .OrderBy(kvp => kvp.Value.CreationTime)
                .Take(_renderCache.Count - MaxCacheEntries)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in oldestEntries)
            {
                _renderCache.TryRemove(key, out _);
            }
        }
    }

    private void CleanupUnusedResources()
    {
        // Return unused batches to pool
        while (_updateQueue.TryDequeue(out var batch))
        {
            _batchPool.Return(batch);
        }

        // Reset active particle count
        Interlocked.Exchange(ref _activeParticleCount, 0);
    }

    /// <summary>
    /// Get advanced performance metrics including GPU and memory usage
    /// </summary>
    public AdvancedPerformanceMetrics GetAdvancedMetrics()
    {
        return new AdvancedPerformanceMetrics
        {
            AverageFrameTime = _averageFrameTime,
            CurrentFPS = 1000.0 / _averageFrameTime,
            PerformanceLevel = CurrentPerformanceLevel,
            FrameTimeHistory = new Queue<double>(_frameTimeHistory),
            HardwareAcceleration = UseHardwareAcceleration,
            GpuUtilization = GetGpuUtilization(),
            MemoryUsageMB = GC.GetTotalMemory(false) / (1024 * 1024),
            ActiveParticleCount = _activeParticleCount,
            CacheHitRatio = CalculateCacheHitRatio(),
            ParallelProcessingEnabled = _maxConcurrency > 1,
            QueuedBatches = _updateQueue.Count
        };
    }

    private double CalculateCacheHitRatio()
    {
        // Simple approximation based on cache size and activity
        return _renderCache.Count > 0 ? Math.Min(1.0, _renderCache.Count / 100.0) : 0.0;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cancellationTokenSource.Cancel();
            _parallelUpdateTask?.Wait(TimeSpan.FromSeconds(5));
            _cancellationTokenSource.Dispose();
            
            _performanceMonitor?.Stop();
            _cacheCleanupTimer?.Dispose();
            _renderSemaphore?.Dispose();
            
            _gpuUsageCounter?.Dispose();
            _memoryUsageCounter?.Dispose();
            
            CleanupUnusedResources();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}

/// <summary>
/// Particle update batch for parallel processing
/// </summary>
public class ParticleUpdateBatch
{
    public List<ParticleData> Particles { get; set; } = new();
    public double DeltaTime { get; set; }
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
    
    public void Reset()
    {
        Particles.Clear();
        DeltaTime = 0;
        CreationTime = DateTime.UtcNow;
    }
}

/// <summary>
/// Lightweight particle data for parallel processing
/// </summary>
public class ParticleData
{
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
    public double MaxLife { get; set; }
    public double Rotation { get; set; }
    public double RotationSpeed { get; set; }
    public double Scale { get; set; } = 1.0;
    public double ScaleSpeed { get; set; }
}

/// <summary>
/// Cached render data for performance optimization
/// </summary>
public class CachedRenderData
{
    public object Data { get; set; }
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
    public DateTime LastAccessed { get; set; } = DateTime.UtcNow;
    public int AccessCount { get; set; }
    public long SizeBytes { get; set; }
}

/// <summary>
/// Advanced performance metrics with GPU and parallel processing stats
/// </summary>
public class AdvancedPerformanceMetrics : PerformanceMetrics
{
    public float GpuUtilization { get; set; }
    public long MemoryUsageMB { get; set; }
    public double CacheHitRatio { get; set; }
    public bool ParallelProcessingEnabled { get; set; }
    public int QueuedBatches { get; set; }
}

/// <summary>
/// Simple object pool for memory optimization
/// </summary>
public class ObjectPool<T> where T : class, new()
{
    private readonly ConcurrentQueue<T> _objects = new();
    private readonly Func<T> _objectGenerator;
    private readonly Action<T> _resetAction;
    private readonly int _maxSize;
    private int _currentCount;

    public ObjectPool(Func<T> objectGenerator, Action<T> resetAction = null, int maxSize = 100)
    {
        _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
        _resetAction = resetAction;
        _maxSize = maxSize;
    }

    public T Get()
    {
        if (_objects.TryDequeue(out T item))
        {
            Interlocked.Decrement(ref _currentCount);
            return item;
        }
        return _objectGenerator();
    }

    public void Return(T item)
    {
        if (item == null) return;
        
        if (_currentCount < _maxSize)
        {
            _resetAction?.Invoke(item);
            _objects.Enqueue(item);
            Interlocked.Increment(ref _currentCount);
        }
    }
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