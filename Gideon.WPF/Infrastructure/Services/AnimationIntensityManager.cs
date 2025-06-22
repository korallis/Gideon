using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Gideon.WPF.Infrastructure.Services
{
    /// <summary>
    /// Manages animation intensity settings across the application
    /// </summary>
    public class AnimationIntensityManager : INotifyPropertyChanged
    {
        private static AnimationIntensityManager _instance;
        private AnimationIntensityLevel _currentLevel = AnimationIntensityLevel.High;
        private double _globalIntensityMultiplier = 1.0;
        private bool _enableParticleEffects = true;
        private bool _enableGlowEffects = true;
        private bool _enableTransitions = true;
        private bool _enableHolographicEffects = true;
        private double _frameRateTarget = 60.0;

        public static AnimationIntensityManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new AnimationIntensityManager();
                return _instance;
            }
        }

        #region Properties

        /// <summary>
        /// Current animation intensity level
        /// </summary>
        public AnimationIntensityLevel CurrentLevel
        {
            get => _currentLevel;
            set
            {
                if (_currentLevel != value)
                {
                    _currentLevel = value;
                    ApplyIntensityLevel();
                    OnPropertyChanged();
                    LevelChanged?.Invoke(value);
                }
            }
        }

        /// <summary>
        /// Global intensity multiplier (0.0 to 2.0)
        /// </summary>
        public double GlobalIntensityMultiplier
        {
            get => _globalIntensityMultiplier;
            set
            {
                var clampedValue = Math.Max(0.0, Math.Min(2.0, value));
                if (Math.Abs(_globalIntensityMultiplier - clampedValue) > 0.01)
                {
                    _globalIntensityMultiplier = clampedValue;
                    OnPropertyChanged();
                    IntensityChanged?.Invoke(clampedValue);
                }
            }
        }

        /// <summary>
        /// Whether particle effects are enabled
        /// </summary>
        public bool EnableParticleEffects
        {
            get => _enableParticleEffects;
            set
            {
                if (_enableParticleEffects != value)
                {
                    _enableParticleEffects = value;
                    OnPropertyChanged();
                    EffectToggled?.Invoke(nameof(EnableParticleEffects), value);
                }
            }
        }

        /// <summary>
        /// Whether glow effects are enabled
        /// </summary>
        public bool EnableGlowEffects
        {
            get => _enableGlowEffects;
            set
            {
                if (_enableGlowEffects != value)
                {
                    _enableGlowEffects = value;
                    OnPropertyChanged();
                    EffectToggled?.Invoke(nameof(EnableGlowEffects), value);
                }
            }
        }

        /// <summary>
        /// Whether transitions and micro-animations are enabled
        /// </summary>
        public bool EnableTransitions
        {
            get => _enableTransitions;
            set
            {
                if (_enableTransitions != value)
                {
                    _enableTransitions = value;
                    OnPropertyChanged();
                    EffectToggled?.Invoke(nameof(EnableTransitions), value);
                }
            }
        }

        /// <summary>
        /// Whether holographic effects are enabled
        /// </summary>
        public bool EnableHolographicEffects
        {
            get => _enableHolographicEffects;
            set
            {
                if (_enableHolographicEffects != value)
                {
                    _enableHolographicEffects = value;
                    OnPropertyChanged();
                    EffectToggled?.Invoke(nameof(EnableHolographicEffects), value);
                }
            }
        }

        /// <summary>
        /// Target frame rate for animations
        /// </summary>
        public double FrameRateTarget
        {
            get => _frameRateTarget;
            set
            {
                var clampedValue = Math.Max(15.0, Math.Min(120.0, value));
                if (Math.Abs(_frameRateTarget - clampedValue) > 0.1)
                {
                    _frameRateTarget = clampedValue;
                    OnPropertyChanged();
                    FrameRateChanged?.Invoke(clampedValue);
                }
            }
        }

        /// <summary>
        /// Gets the current animation configuration
        /// </summary>
        public AnimationConfig CurrentConfig => new()
        {
            Level = CurrentLevel,
            GlobalMultiplier = GlobalIntensityMultiplier,
            EnableParticles = EnableParticleEffects,
            EnableGlow = EnableGlowEffects,
            EnableTransitions = EnableTransitions,
            EnableHolographic = EnableHolographicEffects,
            TargetFrameRate = FrameRateTarget,
            ParticleCount = GetParticleCountForLevel(),
            BlurIntensity = GetBlurIntensityForLevel(),
            GlowIntensity = GetGlowIntensityForLevel(),
            TransitionDuration = GetTransitionDurationForLevel()
        };

        #endregion

        #region Events

        public event Action<AnimationIntensityLevel> LevelChanged;
        public event Action<double> IntensityChanged;
        public event Action<string, bool> EffectToggled;
        public event Action<double> FrameRateChanged;

        #endregion

        #region Public Methods

        /// <summary>
        /// Set a predefined intensity level
        /// </summary>
        public void SetIntensityLevel(AnimationIntensityLevel level)
        {
            CurrentLevel = level;
        }

        /// <summary>
        /// Apply custom intensity settings
        /// </summary>
        public void ApplyCustomSettings(AnimationConfig config)
        {
            _currentLevel = AnimationIntensityLevel.Custom;
            _globalIntensityMultiplier = config.GlobalMultiplier;
            _enableParticleEffects = config.EnableParticles;
            _enableGlowEffects = config.EnableGlow;
            _enableTransitions = config.EnableTransitions;
            _enableHolographicEffects = config.EnableHolographic;
            _frameRateTarget = config.TargetFrameRate;

            OnPropertyChanged(nameof(CurrentLevel));
            OnPropertyChanged(nameof(GlobalIntensityMultiplier));
            OnPropertyChanged(nameof(EnableParticleEffects));
            OnPropertyChanged(nameof(EnableGlowEffects));
            OnPropertyChanged(nameof(EnableTransitions));
            OnPropertyChanged(nameof(EnableHolographicEffects));
            OnPropertyChanged(nameof(FrameRateTarget));

            LevelChanged?.Invoke(AnimationIntensityLevel.Custom);
        }

        /// <summary>
        /// Reset to default settings
        /// </summary>
        public void ResetToDefaults()
        {
            CurrentLevel = AnimationIntensityLevel.High;
        }

        /// <summary>
        /// Get optimized settings for performance level
        /// </summary>
        public AnimationConfig GetOptimizedConfig(PerformanceLevel performanceLevel)
        {
            return performanceLevel switch
            {
                PerformanceLevel.Maximum => new AnimationConfig
                {
                    Level = AnimationIntensityLevel.Maximum,
                    GlobalMultiplier = 1.2,
                    EnableParticles = true,
                    EnableGlow = true,
                    EnableTransitions = true,
                    EnableHolographic = true,
                    TargetFrameRate = 60,
                    ParticleCount = 500,
                    BlurIntensity = 1.0,
                    GlowIntensity = 1.0,
                    TransitionDuration = 300
                },
                PerformanceLevel.High => new AnimationConfig
                {
                    Level = AnimationIntensityLevel.High,
                    GlobalMultiplier = 1.0,
                    EnableParticles = true,
                    EnableGlow = true,
                    EnableTransitions = true,
                    EnableHolographic = true,
                    TargetFrameRate = 60,
                    ParticleCount = 200,
                    BlurIntensity = 0.8,
                    GlowIntensity = 0.8,
                    TransitionDuration = 250
                },
                PerformanceLevel.Medium => new AnimationConfig
                {
                    Level = AnimationIntensityLevel.Medium,
                    GlobalMultiplier = 0.7,
                    EnableParticles = true,
                    EnableGlow = false,
                    EnableTransitions = true,
                    EnableHolographic = false,
                    TargetFrameRate = 30,
                    ParticleCount = 100,
                    BlurIntensity = 0.5,
                    GlowIntensity = 0.5,
                    TransitionDuration = 200
                },
                PerformanceLevel.Low => new AnimationConfig
                {
                    Level = AnimationIntensityLevel.Low,
                    GlobalMultiplier = 0.3,
                    EnableParticles = false,
                    EnableGlow = false,
                    EnableTransitions = false,
                    EnableHolographic = false,
                    TargetFrameRate = 30,
                    ParticleCount = 50,
                    BlurIntensity = 0.2,
                    GlowIntensity = 0.2,
                    TransitionDuration = 150
                },
                _ => CurrentConfig
            };
        }

        /// <summary>
        /// Temporarily reduce intensity for performance
        /// </summary>
        public void ReduceIntensityTemporarily(TimeSpan duration)
        {
            var originalLevel = CurrentLevel;
            var reducedLevel = CurrentLevel switch
            {
                AnimationIntensityLevel.Maximum => AnimationIntensityLevel.High,
                AnimationIntensityLevel.High => AnimationIntensityLevel.Medium,
                AnimationIntensityLevel.Medium => AnimationIntensityLevel.Low,
                _ => AnimationIntensityLevel.Low
            };

            SetIntensityLevel(reducedLevel);

            // Restore after duration
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = duration
            };
            timer.Tick += (s, e) =>
            {
                SetIntensityLevel(originalLevel);
                timer.Stop();
            };
            timer.Start();
        }

        /// <summary>
        /// Check if a specific effect should be enabled based on current settings
        /// </summary>
        public bool ShouldEnableEffect(EffectType effectType)
        {
            return effectType switch
            {
                EffectType.Particles => EnableParticleEffects,
                EffectType.Glow => EnableGlowEffects,
                EffectType.Transitions => EnableTransitions,
                EffectType.Holographic => EnableHolographicEffects,
                _ => true
            };
        }

        /// <summary>
        /// Get scaled value based on current intensity
        /// </summary>
        public double GetScaledValue(double baseValue)
        {
            return baseValue * GlobalIntensityMultiplier;
        }

        /// <summary>
        /// Get scaled duration based on current intensity
        /// </summary>
        public TimeSpan GetScaledDuration(TimeSpan baseDuration)
        {
            var scaleFactor = EnableTransitions ? GlobalIntensityMultiplier : 0.1;
            return TimeSpan.FromMilliseconds(baseDuration.TotalMilliseconds * scaleFactor);
        }

        #endregion

        #region Private Methods

        private void ApplyIntensityLevel()
        {
            switch (CurrentLevel)
            {
                case AnimationIntensityLevel.Off:
                    _globalIntensityMultiplier = 0.0;
                    _enableParticleEffects = false;
                    _enableGlowEffects = false;
                    _enableTransitions = false;
                    _enableHolographicEffects = false;
                    _frameRateTarget = 30;
                    break;

                case AnimationIntensityLevel.Low:
                    _globalIntensityMultiplier = 0.3;
                    _enableParticleEffects = false;
                    _enableGlowEffects = false;
                    _enableTransitions = true;
                    _enableHolographicEffects = false;
                    _frameRateTarget = 30;
                    break;

                case AnimationIntensityLevel.Medium:
                    _globalIntensityMultiplier = 0.7;
                    _enableParticleEffects = true;
                    _enableGlowEffects = false;
                    _enableTransitions = true;
                    _enableHolographicEffects = false;
                    _frameRateTarget = 30;
                    break;

                case AnimationIntensityLevel.High:
                    _globalIntensityMultiplier = 1.0;
                    _enableParticleEffects = true;
                    _enableGlowEffects = true;
                    _enableTransitions = true;
                    _enableHolographicEffects = true;
                    _frameRateTarget = 60;
                    break;

                case AnimationIntensityLevel.Maximum:
                    _globalIntensityMultiplier = 1.2;
                    _enableParticleEffects = true;
                    _enableGlowEffects = true;
                    _enableTransitions = true;
                    _enableHolographicEffects = true;
                    _frameRateTarget = 60;
                    break;
            }

            // Notify all properties changed
            OnPropertyChanged(nameof(GlobalIntensityMultiplier));
            OnPropertyChanged(nameof(EnableParticleEffects));
            OnPropertyChanged(nameof(EnableGlowEffects));
            OnPropertyChanged(nameof(EnableTransitions));
            OnPropertyChanged(nameof(EnableHolographicEffects));
            OnPropertyChanged(nameof(FrameRateTarget));
        }

        private int GetParticleCountForLevel()
        {
            return CurrentLevel switch
            {
                AnimationIntensityLevel.Off => 0,
                AnimationIntensityLevel.Low => 50,
                AnimationIntensityLevel.Medium => 100,
                AnimationIntensityLevel.High => 200,
                AnimationIntensityLevel.Maximum => 500,
                _ => (int)(200 * GlobalIntensityMultiplier)
            };
        }

        private double GetBlurIntensityForLevel()
        {
            return CurrentLevel switch
            {
                AnimationIntensityLevel.Off => 0.0,
                AnimationIntensityLevel.Low => 0.2,
                AnimationIntensityLevel.Medium => 0.5,
                AnimationIntensityLevel.High => 0.8,
                AnimationIntensityLevel.Maximum => 1.0,
                _ => GlobalIntensityMultiplier
            };
        }

        private double GetGlowIntensityForLevel()
        {
            return EnableGlowEffects ? GetBlurIntensityForLevel() : 0.0;
        }

        private double GetTransitionDurationForLevel()
        {
            if (!EnableTransitions) return 50;

            return CurrentLevel switch
            {
                AnimationIntensityLevel.Off => 50,
                AnimationIntensityLevel.Low => 150,
                AnimationIntensityLevel.Medium => 200,
                AnimationIntensityLevel.High => 250,
                AnimationIntensityLevel.Maximum => 300,
                _ => 250 * GlobalIntensityMultiplier
            };
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    #region Supporting Types

    /// <summary>
    /// Animation intensity levels
    /// </summary>
    public enum AnimationIntensityLevel
    {
        Off,
        Low,
        Medium,
        High,
        Maximum,
        Custom
    }

    /// <summary>
    /// Types of effects that can be controlled
    /// </summary>
    public enum EffectType
    {
        Particles,
        Glow,
        Transitions,
        Holographic
    }

    /// <summary>
    /// Complete animation configuration
    /// </summary>
    public class AnimationConfig
    {
        public AnimationIntensityLevel Level { get; set; }
        public double GlobalMultiplier { get; set; } = 1.0;
        public bool EnableParticles { get; set; } = true;
        public bool EnableGlow { get; set; } = true;
        public bool EnableTransitions { get; set; } = true;
        public bool EnableHolographic { get; set; } = true;
        public double TargetFrameRate { get; set; } = 60;
        public int ParticleCount { get; set; } = 200;
        public double BlurIntensity { get; set; } = 1.0;
        public double GlowIntensity { get; set; } = 1.0;
        public double TransitionDuration { get; set; } = 250;
    }

    #endregion
}