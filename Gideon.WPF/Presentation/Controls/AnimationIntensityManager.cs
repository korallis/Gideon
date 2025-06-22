// ==========================================================================
// AnimationIntensityManager.cs - Configurable Animation Intensity System
// ==========================================================================
// Centralized system for managing animation intensity across all holographic
// controls, allowing users to customize visual effects based on preference
// and system performance.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Central manager for animation intensity settings across the application
/// </summary>
public class AnimationIntensityManager : INotifyPropertyChanged, IDisposable
{
    #region Singleton Pattern

    private static readonly Lazy<AnimationIntensityManager> _instance = 
        new(() => new AnimationIntensityManager());

    public static AnimationIntensityManager Instance => _instance.Value;

    #endregion

    #region Events

    public event PropertyChangedEventHandler PropertyChanged;
    public event EventHandler<IntensityChangedEventArgs> IntensityChanged;
    public event EventHandler<AnimationPresetChangedEventArgs> PresetChanged;

    #endregion

    #region Properties

    private AnimationIntensitySettings _currentSettings;
    private AnimationPreset _currentPreset = AnimationPreset.Balanced;
    private bool _isAdaptiveEnabled = true;
    private readonly List<IAnimationIntensityTarget> _registeredTargets = new();
    private readonly DispatcherTimer _adaptiveTimer;

    public AnimationIntensitySettings CurrentSettings
    {
        get => _currentSettings;
        private set
        {
            if (_currentSettings != value)
            {
                var oldSettings = _currentSettings;
                _currentSettings = value;
                OnPropertyChanged();
                NotifyIntensityChanged(oldSettings, value);
            }
        }
    }

    public AnimationPreset CurrentPreset
    {
        get => _currentPreset;
        set
        {
            if (_currentPreset != value)
            {
                var oldPreset = _currentPreset;
                _currentPreset = value;
                ApplyPreset(value);
                OnPropertyChanged();
                PresetChanged?.Invoke(this, new AnimationPresetChangedEventArgs
                {
                    OldPreset = oldPreset,
                    NewPreset = value
                });
            }
        }
    }

    public bool IsAdaptiveEnabled
    {
        get => _isAdaptiveEnabled;
        set
        {
            if (_isAdaptiveEnabled != value)
            {
                _isAdaptiveEnabled = value;
                OnPropertyChanged();
                
                if (value)
                    _adaptiveTimer.Start();
                else
                    _adaptiveTimer.Stop();
            }
        }
    }

    public double MasterIntensity => CurrentSettings?.MasterIntensity ?? 1.0;
    public double ParticleIntensity => CurrentSettings?.ParticleIntensity ?? 1.0;
    public double GlowIntensity => CurrentSettings?.GlowIntensity ?? 1.0;
    public double TransitionIntensity => CurrentSettings?.TransitionIntensity ?? 1.0;
    public double MotionIntensity => CurrentSettings?.MotionIntensity ?? 1.0;

    #endregion

    #region Constructor

    private AnimationIntensityManager()
    {
        // Initialize with balanced preset
        _currentSettings = GetPresetSettings(AnimationPreset.Balanced);
        
        // Set up adaptive timer
        _adaptiveTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };
        _adaptiveTimer.Tick += OnAdaptiveTimerTick;
        
        if (_isAdaptiveEnabled)
            _adaptiveTimer.Start();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Register a control for animation intensity updates
    /// </summary>
    public void RegisterTarget(IAnimationIntensityTarget target)
    {
        if (target != null && !_registeredTargets.Contains(target))
        {
            _registeredTargets.Add(target);
            target.ApplyIntensitySettings(CurrentSettings);
        }
    }

    /// <summary>
    /// Unregister a control from animation intensity updates
    /// </summary>
    public void UnregisterTarget(IAnimationIntensityTarget target)
    {
        _registeredTargets.Remove(target);
    }

    /// <summary>
    /// Apply a predefined animation preset
    /// </summary>
    public void ApplyPreset(AnimationPreset preset)
    {
        CurrentSettings = GetPresetSettings(preset);
    }

    /// <summary>
    /// Set custom animation intensity settings
    /// </summary>
    public void SetCustomSettings(AnimationIntensitySettings settings)
    {
        if (settings != null)
        {
            _currentPreset = AnimationPreset.Custom;
            CurrentSettings = settings.Clone();
        }
    }

    /// <summary>
    /// Adjust master intensity (affects all other intensities proportionally)
    /// </summary>
    public void SetMasterIntensity(double intensity)
    {
        intensity = Math.Clamp(intensity, 0.0, 2.0);
        
        var newSettings = CurrentSettings.Clone();
        newSettings.MasterIntensity = intensity;
        
        CurrentSettings = newSettings;
        
        if (_currentPreset != AnimationPreset.Custom)
            _currentPreset = AnimationPreset.Custom;
    }

    /// <summary>
    /// Adjust specific animation component intensity
    /// </summary>
    public void SetComponentIntensity(AnimationComponent component, double intensity)
    {
        intensity = Math.Clamp(intensity, 0.0, 2.0);
        
        var newSettings = CurrentSettings.Clone();
        
        switch (component)
        {
            case AnimationComponent.Particles:
                newSettings.ParticleIntensity = intensity;
                break;
            case AnimationComponent.Glow:
                newSettings.GlowIntensity = intensity;
                break;
            case AnimationComponent.Transitions:
                newSettings.TransitionIntensity = intensity;
                break;
            case AnimationComponent.Motion:
                newSettings.MotionIntensity = intensity;
                break;
        }
        
        CurrentSettings = newSettings;
        
        if (_currentPreset != AnimationPreset.Custom)
            _currentPreset = AnimationPreset.Custom;
    }

    /// <summary>
    /// Enable or disable specific animation features
    /// </summary>
    public void SetAnimationFeatures(AnimationFeatures features)
    {
        var newSettings = CurrentSettings.Clone();
        newSettings.EnabledFeatures = features;
        
        CurrentSettings = newSettings;
        
        if (_currentPreset != AnimationPreset.Custom)
            _currentPreset = AnimationPreset.Custom;
    }

    /// <summary>
    /// Get effective intensity for a specific component (master * component)
    /// </summary>
    public double GetEffectiveIntensity(AnimationComponent component)
    {
        var componentIntensity = component switch
        {
            AnimationComponent.Particles => CurrentSettings.ParticleIntensity,
            AnimationComponent.Glow => CurrentSettings.GlowIntensity,
            AnimationComponent.Transitions => CurrentSettings.TransitionIntensity,
            AnimationComponent.Motion => CurrentSettings.MotionIntensity,
            _ => 1.0
        };
        
        return Math.Clamp(CurrentSettings.MasterIntensity * componentIntensity, 0.0, 2.0);
    }

    /// <summary>
    /// Check if a specific animation feature is enabled
    /// </summary>
    public bool IsFeatureEnabled(AnimationFeatures feature)
    {
        return CurrentSettings.EnabledFeatures.HasFlag(feature);
    }

    /// <summary>
    /// Temporarily reduce all animations (e.g., during high CPU usage)
    /// </summary>
    public void TemporaryPerformanceReduction(double reductionFactor = 0.5)
    {
        var tempSettings = CurrentSettings.Clone();
        tempSettings.MasterIntensity *= reductionFactor;
        tempSettings.ParticleIntensity *= reductionFactor;
        tempSettings.GlowIntensity *= reductionFactor;
        tempSettings.TransitionIntensity *= reductionFactor;
        tempSettings.MotionIntensity *= reductionFactor;
        
        // Apply temporarily without changing current preset
        NotifyTargetsOfIntensityChange(tempSettings);
    }

    /// <summary>
    /// Restore normal animation intensity after temporary reduction
    /// </summary>
    public void RestoreNormalIntensity()
    {
        NotifyTargetsOfIntensityChange(CurrentSettings);
    }

    /// <summary>
    /// Get all available animation presets
    /// </summary>
    public IEnumerable<AnimationPreset> GetAvailablePresets()
    {
        return Enum.GetValues<AnimationPreset>().Where(p => p != AnimationPreset.Custom);
    }

    /// <summary>
    /// Export current settings for saving to user preferences
    /// </summary>
    public string ExportSettings()
    {
        return System.Text.Json.JsonSerializer.Serialize(CurrentSettings);
    }

    /// <summary>
    /// Import settings from saved user preferences
    /// </summary>
    public bool ImportSettings(string json)
    {
        try
        {
            var settings = System.Text.Json.JsonSerializer.Deserialize<AnimationIntensitySettings>(json);
            if (settings != null)
            {
                SetCustomSettings(settings);
                return true;
            }
        }
        catch
        {
            // Invalid JSON or settings
        }
        return false;
    }

    #endregion

    #region Private Methods

    private AnimationIntensitySettings GetPresetSettings(AnimationPreset preset)
    {
        return preset switch
        {
            AnimationPreset.Minimal => new AnimationIntensitySettings
            {
                MasterIntensity = 0.3,
                ParticleIntensity = 0.2,
                GlowIntensity = 0.4,
                TransitionIntensity = 0.5,
                MotionIntensity = 0.3,
                EnabledFeatures = AnimationFeatures.BasicGlow | AnimationFeatures.SimpleTransitions,
                AnimationSpeed = 0.7,
                QualityLevel = AnimationQuality.Low
            },
            
            AnimationPreset.Balanced => new AnimationIntensitySettings
            {
                MasterIntensity = 0.7,
                ParticleIntensity = 0.6,
                GlowIntensity = 0.8,
                TransitionIntensity = 0.7,
                MotionIntensity = 0.6,
                EnabledFeatures = AnimationFeatures.BasicGlow | AnimationFeatures.SimpleTransitions | 
                                 AnimationFeatures.ParticleEffects | AnimationFeatures.MotionEffects,
                AnimationSpeed = 1.0,
                QualityLevel = AnimationQuality.Medium
            },
            
            AnimationPreset.Enhanced => new AnimationIntensitySettings
            {
                MasterIntensity = 1.0,
                ParticleIntensity = 0.9,
                GlowIntensity = 1.0,
                TransitionIntensity = 0.9,
                MotionIntensity = 0.8,
                EnabledFeatures = AnimationFeatures.All,
                AnimationSpeed = 1.2,
                QualityLevel = AnimationQuality.High
            },
            
            AnimationPreset.Maximum => new AnimationIntensitySettings
            {
                MasterIntensity = 1.5,
                ParticleIntensity = 1.3,
                GlowIntensity = 1.4,
                TransitionIntensity = 1.2,
                MotionIntensity = 1.1,
                EnabledFeatures = AnimationFeatures.All,
                AnimationSpeed = 1.5,
                QualityLevel = AnimationQuality.Ultra
            },
            
            AnimationPreset.Performance => new AnimationIntensitySettings
            {
                MasterIntensity = 0.4,
                ParticleIntensity = 0.1,
                GlowIntensity = 0.3,
                TransitionIntensity = 0.6,
                MotionIntensity = 0.2,
                EnabledFeatures = AnimationFeatures.BasicGlow | AnimationFeatures.SimpleTransitions,
                AnimationSpeed = 0.8,
                QualityLevel = AnimationQuality.Low
            },
            
            _ => GetPresetSettings(AnimationPreset.Balanced)
        };
    }

    private void OnAdaptiveTimerTick(object sender, EventArgs e)
    {
        if (!IsAdaptiveEnabled) return;

        // Get current system performance (would need performance manager instance)
        // For now, simulate basic adaptive behavior
        var currentFPS = GetCurrentFrameRate();
        
        if (currentFPS < 30 && CurrentPreset != AnimationPreset.Performance)
        {
            // Automatically reduce to performance preset
            CurrentPreset = AnimationPreset.Performance;
        }
        else if (currentFPS > 55 && CurrentPreset == AnimationPreset.Performance)
        {
            // Restore to balanced preset when performance improves
            CurrentPreset = AnimationPreset.Balanced;
        }
    }

    private double GetCurrentFrameRate()
    {
        // This would integrate with the PerformanceOptimizedRenderer
        // For now, return a placeholder value
        return 60.0;
    }

    private void NotifyIntensityChanged(AnimationIntensitySettings oldSettings, AnimationIntensitySettings newSettings)
    {
        IntensityChanged?.Invoke(this, new IntensityChangedEventArgs
        {
            OldSettings = oldSettings,
            NewSettings = newSettings
        });
        
        NotifyTargetsOfIntensityChange(newSettings);
    }

    private void NotifyTargetsOfIntensityChange(AnimationIntensitySettings settings)
    {
        // Remove any targets that are no longer valid
        for (int i = _registeredTargets.Count - 1; i >= 0; i--)
        {
            var target = _registeredTargets[i];
            if (!target.IsValid)
            {
                _registeredTargets.RemoveAt(i);
                continue;
            }
            
            try
            {
                target.ApplyIntensitySettings(settings);
            }
            catch
            {
                // Target may have been disposed, remove it
                _registeredTargets.RemoveAt(i);
            }
        }
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region Dispose

    public void Dispose()
    {
        _adaptiveTimer?.Stop();
        _registeredTargets.Clear();
        IntensityChanged = null;
        PresetChanged = null;
        PropertyChanged = null;
    }

    #endregion
}

/// <summary>
/// Interface for controls that can respond to animation intensity changes
/// </summary>
public interface IAnimationIntensityTarget
{
    void ApplyIntensitySettings(AnimationIntensitySettings settings);
    bool IsValid { get; }
}

/// <summary>
/// Animation intensity settings configuration
/// </summary>
public class AnimationIntensitySettings
{
    public double MasterIntensity { get; set; } = 1.0;
    public double ParticleIntensity { get; set; } = 1.0;
    public double GlowIntensity { get; set; } = 1.0;
    public double TransitionIntensity { get; set; } = 1.0;
    public double MotionIntensity { get; set; } = 1.0;
    public AnimationFeatures EnabledFeatures { get; set; } = AnimationFeatures.All;
    public double AnimationSpeed { get; set; } = 1.0;
    public AnimationQuality QualityLevel { get; set; } = AnimationQuality.Medium;

    public AnimationIntensitySettings Clone()
    {
        return new AnimationIntensitySettings
        {
            MasterIntensity = MasterIntensity,
            ParticleIntensity = ParticleIntensity,
            GlowIntensity = GlowIntensity,
            TransitionIntensity = TransitionIntensity,
            MotionIntensity = MotionIntensity,
            EnabledFeatures = EnabledFeatures,
            AnimationSpeed = AnimationSpeed,
            QualityLevel = QualityLevel
        };
    }
}

/// <summary>
/// Animation presets for quick configuration
/// </summary>
public enum AnimationPreset
{
    Performance,    // Minimal animations for best performance
    Minimal,        // Reduced animations, basic effects
    Balanced,       // Good balance of effects and performance  
    Enhanced,       // Rich animations with full effects
    Maximum,        // Maximum visual impact
    Custom          // User-defined settings
}

/// <summary>
/// Animation components that can be individually controlled
/// </summary>
public enum AnimationComponent
{
    Particles,
    Glow,
    Transitions,
    Motion
}

/// <summary>
/// Animation features that can be enabled/disabled
/// </summary>
[Flags]
public enum AnimationFeatures
{
    None = 0,
    BasicGlow = 1,
    ParticleEffects = 2,
    SimpleTransitions = 4,
    ComplexTransitions = 8,
    MotionEffects = 16,
    BlurEffects = 32,
    AdvancedShaders = 64,
    All = BasicGlow | ParticleEffects | SimpleTransitions | ComplexTransitions | 
          MotionEffects | BlurEffects | AdvancedShaders
}

/// <summary>
/// Animation quality levels
/// </summary>
public enum AnimationQuality
{
    Low,
    Medium,
    High,
    Ultra
}

/// <summary>
/// Event args for intensity changes
/// </summary>
public class IntensityChangedEventArgs : EventArgs
{
    public AnimationIntensitySettings OldSettings { get; set; }
    public AnimationIntensitySettings NewSettings { get; set; }
}

/// <summary>
/// Event args for preset changes
/// </summary>
public class AnimationPresetChangedEventArgs : EventArgs
{
    public AnimationPreset OldPreset { get; set; }
    public AnimationPreset NewPreset { get; set; }
}