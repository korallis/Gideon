// ==========================================================================
// AdaptiveRenderingManager.cs - Adaptive Rendering Management System
// ==========================================================================
// Manages automatic switching between full holographic rendering and 
// simplified 2D fallback based on system performance and user preferences.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Central manager for adaptive rendering between holographic and simplified modes
/// </summary>
public class AdaptiveRenderingManager : IDisposable
{
    #region Events

    public event EventHandler<RenderingModeChangedEventArgs> RenderingModeChanged;
    public event EventHandler<PerformanceThresholdEventArgs> PerformanceThresholdReached;

    #endregion

    #region Properties

    public AdaptiveRenderingMode CurrentRenderingMode { get; private set; } = AdaptiveRenderingMode.Auto;
    public bool IsInFallbackMode { get; private set; }
    public PerformanceLevel CurrentPerformanceLevel { get; private set; }
    public double CurrentFrameRate { get; private set; }
    public bool AutoSwitchEnabled { get; set; } = true;

    #endregion

    #region Fields

    private readonly PerformanceOptimizedRenderer _performanceRenderer;
    private readonly Fallback2DRenderer _fallbackRenderer;
    private readonly DispatcherTimer _performanceMonitor;
    private readonly Dictionary<FrameworkElement, IAdaptiveControl> _registeredControls = new();
    private readonly PerformanceThresholds _thresholds;
    private readonly Queue<double> _performanceHistory = new();
    private const int PerformanceHistorySize = 10;
    private bool _disposed;

    #endregion

    #region Constructor

    public AdaptiveRenderingManager(PerformanceOptimizedRenderer performanceRenderer)
    {
        _performanceRenderer = performanceRenderer ?? throw new ArgumentNullException(nameof(performanceRenderer));
        _fallbackRenderer = new Fallback2DRenderer(_performanceRenderer);
        
        _thresholds = new PerformanceThresholds
        {
            FallbackActivationFPS = 25.0,
            FallbackDeactivationFPS = 45.0,
            LowPerformanceMemoryMB = 512,
            CriticalPerformanceMemoryMB = 256
        };

        InitializePerformanceMonitoring();
        RegisterPerformanceCallbacks();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Register a control for adaptive rendering
    /// </summary>
    public void RegisterControl(FrameworkElement control, IAdaptiveControl adaptiveControl)
    {
        if (control == null || adaptiveControl == null) return;

        _registeredControls[control] = adaptiveControl;
        
        // Apply current rendering mode to new control
        ApplyRenderingModeToControl(control, adaptiveControl);
    }

    /// <summary>
    /// Unregister a control from adaptive rendering
    /// </summary>
    public void UnregisterControl(FrameworkElement control)
    {
        _registeredControls.Remove(control);
    }

    /// <summary>
    /// Force switch to a specific rendering mode
    /// </summary>
    public void SetRenderingMode(AdaptiveRenderingMode mode)
    {
        var previousMode = CurrentRenderingMode;
        CurrentRenderingMode = mode;

        switch (mode)
        {
            case AdaptiveRenderingMode.FullHolographic:
                DisableFallbackMode();
                break;
            case AdaptiveRenderingMode.Simplified2D:
                EnableFallbackMode();
                break;
            case AdaptiveRenderingMode.Auto:
                EvaluatePerformanceAndSwitch();
                break;
        }

        if (previousMode != CurrentRenderingMode)
        {
            RenderingModeChanged?.Invoke(this, new RenderingModeChangedEventArgs
            {
                PreviousMode = previousMode,
                NewMode = CurrentRenderingMode,
                Reason = "Manual switch"
            });
        }
    }

    /// <summary>
    /// Get current performance metrics
    /// </summary>
    public AdaptiveRenderingMetrics GetCurrentMetrics()
    {
        var performanceMetrics = _performanceRenderer.GetCurrentMetrics();
        
        return new AdaptiveRenderingMetrics
        {
            CurrentFPS = performanceMetrics.CurrentFPS,
            AverageFrameTime = performanceMetrics.AverageFrameTime,
            PerformanceLevel = performanceMetrics.PerformanceLevel,
            HardwareAcceleration = performanceMetrics.HardwareAcceleration,
            MemoryUsageMB = GC.GetTotalMemory(false) / (1024 * 1024),
            RegisteredControlsCount = _registeredControls.Count,
            IsInFallbackMode = IsInFallbackMode,
            CurrentRenderingMode = CurrentRenderingMode,
            PerformanceStability = CalculatePerformanceStability()
        };
    }

    /// <summary>
    /// Configure performance thresholds
    /// </summary>
    public void ConfigureThresholds(PerformanceThresholds thresholds)
    {
        _thresholds.FallbackActivationFPS = thresholds.FallbackActivationFPS;
        _thresholds.FallbackDeactivationFPS = thresholds.FallbackDeactivationFPS;
        _thresholds.LowPerformanceMemoryMB = thresholds.LowPerformanceMemoryMB;
        _thresholds.CriticalPerformanceMemoryMB = thresholds.CriticalPerformanceMemoryMB;
    }

    /// <summary>
    /// Manually trigger performance evaluation
    /// </summary>
    public void EvaluatePerformance()
    {
        EvaluatePerformanceAndSwitch();
    }

    #endregion

    #region Private Methods

    private void InitializePerformanceMonitoring()
    {
        _performanceMonitor = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2) // Check every 2 seconds
        };
        _performanceMonitor.Tick += OnPerformanceMonitorTick;
        _performanceMonitor.Start();
    }

    private void RegisterPerformanceCallbacks()
    {
        _performanceRenderer.OnPerformanceLevelChanged += OnPerformanceLevelChanged;
    }

    private void OnPerformanceMonitorTick(object sender, EventArgs e)
    {
        if (!AutoSwitchEnabled || CurrentRenderingMode != AdaptiveRenderingMode.Auto)
            return;

        var metrics = _performanceRenderer.GetCurrentMetrics();
        CurrentFrameRate = metrics.CurrentFPS;
        CurrentPerformanceLevel = metrics.PerformanceLevel;

        // Track performance history
        _performanceHistory.Enqueue(CurrentFrameRate);
        if (_performanceHistory.Count > PerformanceHistorySize)
        {
            _performanceHistory.Dequeue();
        }

        EvaluatePerformanceAndSwitch();
    }

    private void OnPerformanceLevelChanged(PerformanceLevel newLevel)
    {
        CurrentPerformanceLevel = newLevel;
        
        if (AutoSwitchEnabled && CurrentRenderingMode == AdaptiveRenderingMode.Auto)
        {
            EvaluatePerformanceAndSwitch();
        }
    }

    private void EvaluatePerformanceAndSwitch()
    {
        var shouldUseFallback = ShouldUseFallbackMode();
        
        if (shouldUseFallback && !IsInFallbackMode)
        {
            EnableFallbackMode();
            PerformanceThresholdReached?.Invoke(this, new PerformanceThresholdEventArgs
            {
                ThresholdType = PerformanceThresholdType.FallbackActivated,
                CurrentFPS = CurrentFrameRate,
                PerformanceLevel = CurrentPerformanceLevel
            });
        }
        else if (!shouldUseFallback && IsInFallbackMode)
        {
            DisableFallbackMode();
            PerformanceThresholdReached?.Invoke(this, new PerformanceThresholdEventArgs
            {
                ThresholdType = PerformanceThresholdType.FallbackDeactivated,
                CurrentFPS = CurrentFrameRate,
                PerformanceLevel = CurrentPerformanceLevel
            });
        }
    }

    private bool ShouldUseFallbackMode()
    {
        // Check frame rate threshold
        if (CurrentFrameRate < _thresholds.FallbackActivationFPS)
            return true;

        // Check if we should stay in fallback mode (hysteresis)
        if (IsInFallbackMode && CurrentFrameRate < _thresholds.FallbackDeactivationFPS)
            return true;

        // Check performance level
        if (CurrentPerformanceLevel >= PerformanceLevel.Medium)
            return true;

        // Check memory usage
        var memoryUsageMB = GC.GetTotalMemory(false) / (1024 * 1024);
        if (memoryUsageMB > _thresholds.LowPerformanceMemoryMB)
            return true;

        // Check performance stability
        var stability = CalculatePerformanceStability();
        if (stability < 0.8) // Unstable performance
            return true;

        return false;
    }

    private double CalculatePerformanceStability()
    {
        if (_performanceHistory.Count < 3)
            return 1.0;

        var values = _performanceHistory.ToArray();
        var average = values.Average();
        var variance = values.Select(v => Math.Pow(v - average, 2)).Average();
        var standardDeviation = Math.Sqrt(variance);
        
        // Stability is inversely related to standard deviation
        // Normalize to 0-1 range where 1 is most stable
        return Math.Max(0, 1 - (standardDeviation / average));
    }

    private void EnableFallbackMode()
    {
        if (IsInFallbackMode) return;

        IsInFallbackMode = true;
        _fallbackRenderer.EnableFallbackMode();

        // Switch all registered controls to fallback mode
        foreach (var kvp in _registeredControls)
        {
            kvp.Value.SwitchToSimplifiedMode();
        }

        Debug.WriteLine($"[AdaptiveRenderingManager] Switched to fallback mode - FPS: {CurrentFrameRate:F1}, Level: {CurrentPerformanceLevel}");
    }

    private void DisableFallbackMode()
    {
        if (!IsInFallbackMode) return;

        IsInFallbackMode = false;
        _fallbackRenderer.DisableFallbackMode();

        // Switch all registered controls to full mode
        foreach (var kvp in _registeredControls)
        {
            kvp.Value.SwitchToFullMode();
        }

        Debug.WriteLine($"[AdaptiveRenderingManager] Switched to full mode - FPS: {CurrentFrameRate:F1}, Level: {CurrentPerformanceLevel}");
    }

    private void ApplyRenderingModeToControl(FrameworkElement control, IAdaptiveControl adaptiveControl)
    {
        if (IsInFallbackMode)
        {
            adaptiveControl.SwitchToSimplifiedMode();
        }
        else
        {
            adaptiveControl.SwitchToFullMode();
        }
    }

    #endregion

    #region Dispose

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _performanceMonitor?.Stop();
            _fallbackRenderer?.Dispose();
            
            if (_performanceRenderer != null)
                _performanceRenderer.OnPerformanceLevelChanged -= OnPerformanceLevelChanged;

            _registeredControls.Clear();
            _performanceHistory.Clear();
            
            _disposed = true;
        }
    }

    #endregion
}

/// <summary>
/// Interface for controls that support adaptive rendering
/// </summary>
public interface IAdaptiveControl
{
    void SwitchToFullMode();
    void SwitchToSimplifiedMode();
    bool IsInSimplifiedMode { get; }
}

/// <summary>
/// Adaptive rendering modes
/// </summary>
public enum AdaptiveRenderingMode
{
    Auto,               // Automatically switch based on performance
    FullHolographic,    // Always use full holographic rendering
    Simplified2D        // Always use simplified 2D rendering
}

/// <summary>
/// Performance thresholds for adaptive rendering
/// </summary>
public class PerformanceThresholds
{
    public double FallbackActivationFPS { get; set; } = 25.0;
    public double FallbackDeactivationFPS { get; set; } = 45.0;
    public long LowPerformanceMemoryMB { get; set; } = 512;
    public long CriticalPerformanceMemoryMB { get; set; } = 256;
}

/// <summary>
/// Adaptive rendering metrics
/// </summary>
public class AdaptiveRenderingMetrics
{
    public double CurrentFPS { get; set; }
    public double AverageFrameTime { get; set; }
    public PerformanceLevel PerformanceLevel { get; set; }
    public bool HardwareAcceleration { get; set; }
    public long MemoryUsageMB { get; set; }
    public int RegisteredControlsCount { get; set; }
    public bool IsInFallbackMode { get; set; }
    public AdaptiveRenderingMode CurrentRenderingMode { get; set; }
    public double PerformanceStability { get; set; }
}

/// <summary>
/// Event args for rendering mode changes
/// </summary>
public class RenderingModeChangedEventArgs : EventArgs
{
    public AdaptiveRenderingMode PreviousMode { get; set; }
    public AdaptiveRenderingMode NewMode { get; set; }
    public string Reason { get; set; }
}

/// <summary>
/// Event args for performance threshold events
/// </summary>
public class PerformanceThresholdEventArgs : EventArgs
{
    public PerformanceThresholdType ThresholdType { get; set; }
    public double CurrentFPS { get; set; }
    public PerformanceLevel PerformanceLevel { get; set; }
}

/// <summary>
/// Performance threshold types
/// </summary>
public enum PerformanceThresholdType
{
    FallbackActivated,
    FallbackDeactivated,
    LowMemory,
    CriticalMemory
}