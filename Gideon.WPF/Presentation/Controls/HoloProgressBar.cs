// ==========================================================================
// HoloProgressBar.cs - Holographic Progress Bar with Particle Stream Effects
// ==========================================================================
// Advanced holographic progress bar featuring particle stream animations,
// EVE-style visual effects, and adaptive performance rendering.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Advanced holographic progress bar with particle stream effects and EVE styling
/// </summary>
public class HoloProgressBar : RangeBase, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloProgressBar),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloProgressBar),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty ProgressStateProperty =
        DependencyProperty.Register(nameof(ProgressState), typeof(HoloProgressState), typeof(HoloProgressBar),
            new PropertyMetadata(HoloProgressState.Normal, OnProgressStateChanged));

    public static readonly DependencyProperty EnableParticleStreamProperty =
        DependencyProperty.Register(nameof(EnableParticleStream), typeof(bool), typeof(HoloProgressBar),
            new PropertyMetadata(true, OnEnableParticleStreamChanged));

    public static readonly DependencyProperty EnablePulseAnimationProperty =
        DependencyProperty.Register(nameof(EnablePulseAnimation), typeof(bool), typeof(HoloProgressBar),
            new PropertyMetadata(true, OnEnablePulseAnimationChanged));

    public static readonly DependencyProperty ShowPercentageProperty =
        DependencyProperty.Register(nameof(ShowPercentage), typeof(bool), typeof(HoloProgressBar),
            new PropertyMetadata(true));

    public static readonly DependencyProperty ProgressTextProperty =
        DependencyProperty.Register(nameof(ProgressText), typeof(string), typeof(HoloProgressBar),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(HoloProgressBar),
            new PropertyMetadata(new CornerRadius(12)));

    public static readonly DependencyProperty ParticleIntensityProperty =
        DependencyProperty.Register(nameof(ParticleIntensity), typeof(double), typeof(HoloProgressBar),
            new PropertyMetadata(1.0, OnParticleIntensityChanged));

    public static readonly DependencyProperty AnimationSpeedProperty =
        DependencyProperty.Register(nameof(AnimationSpeed), typeof(double), typeof(HoloProgressBar),
            new PropertyMetadata(1.0, OnAnimationSpeedChanged));

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

    public HoloProgressState ProgressState
    {
        get => (HoloProgressState)GetValue(ProgressStateProperty);
        set => SetValue(ProgressStateProperty, value);
    }

    public bool EnableParticleStream
    {
        get => (bool)GetValue(EnableParticleStreamProperty);
        set => SetValue(EnableParticleStreamProperty, value);
    }

    public bool EnablePulseAnimation
    {
        get => (bool)GetValue(EnablePulseAnimationProperty);
        set => SetValue(EnablePulseAnimationProperty, value);
    }

    public bool ShowPercentage
    {
        get => (bool)GetValue(ShowPercentageProperty);
        set => SetValue(ShowPercentageProperty, value);
    }

    public string ProgressText
    {
        get => (string)GetValue(ProgressTextProperty);
        set => SetValue(ProgressTextProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public double ParticleIntensity
    {
        get => (double)GetValue(ParticleIntensityProperty);
        set => SetValue(ParticleIntensityProperty, value);
    }

    public double AnimationSpeed
    {
        get => (double)GetValue(AnimationSpeedProperty);
        set => SetValue(AnimationSpeedProperty, value);
    }

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode { get; private set; }

    public void SwitchToFullMode()
    {
        IsInSimplifiedMode = false;
        EnableParticleStream = true;
        EnablePulseAnimation = true;
        ParticleIntensity = 1.0;
        UpdateProgressBarAppearance();
    }

    public void SwitchToSimplifiedMode()
    {
        IsInSimplifiedMode = true;
        EnableParticleStream = false;
        EnablePulseAnimation = false;
        ParticleIntensity = 0.3;
        UpdateProgressBarAppearance();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => !_disposed && IsLoaded;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        ParticleIntensity = settings.ParticleIntensity;
        AnimationSpeed = settings.AnimationSpeed;
        EnableParticleStream = settings.EnabledFeatures.HasFlag(AnimationFeatures.ParticleEffects);
        EnablePulseAnimation = settings.EnabledFeatures.HasFlag(AnimationFeatures.BasicGlow);
        
        UpdateProgressBarAppearance();
    }

    #endregion

    #region Fields

    private Grid _mainGrid;
    private Border _trackBorder;
    private Border _progressBorder;
    private Canvas _particleCanvas;
    private TextBlock _percentageText;
    private TextBlock _progressTextBlock;
    private Rectangle _glowLayer;
    
    private readonly List<ProgressParticle> _particles = new();
    private DispatcherTimer _particleTimer;
    private DispatcherTimer _pulseTimer;
    private double _particlePhase = 0;
    private double _pulsePhase = 0;
    private readonly Random _random = new();
    private bool _disposed = false;

    #endregion

    #region Constructor

    public HoloProgressBar()
    {
        DefaultStyleKey = typeof(HoloProgressBar);
        Height = 24;
        InitializeProgressBar();
        SetupAnimations();
        
        // Register with animation intensity manager
        AnimationIntensityManager.Instance.RegisterTarget(this);
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        ValueChanged += OnValueChanged;
    }

    #endregion

    #region Private Methods

    private void InitializeProgressBar()
    {
        Template = CreateProgressBarTemplate();
        UpdateProgressBarAppearance();
    }

    private ControlTemplate CreateProgressBarTemplate()
    {
        var template = new ControlTemplate(typeof(HoloProgressBar));

        // Main grid container
        var mainGrid = new FrameworkElementFactory(typeof(Grid));
        mainGrid.Name = "PART_MainGrid";

        // Track border (background)
        var trackBorder = new FrameworkElementFactory(typeof(Border));
        trackBorder.Name = "PART_TrackBorder";
        trackBorder.SetValue(Border.CornerRadiusProperty, new TemplateBindingExtension(CornerRadiusProperty));
        trackBorder.SetValue(Border.BorderThicknessProperty, new Thickness(1));

        // Progress container with clipping
        var progressContainer = new FrameworkElementFactory(typeof(Border));
        progressContainer.Name = "PART_ProgressContainer";
        progressContainer.SetValue(Border.CornerRadiusProperty, new TemplateBindingExtension(CornerRadiusProperty));
        progressContainer.SetValue(Border.ClipToBoundsProperty, true);

        // Progress border (foreground)
        var progressBorder = new FrameworkElementFactory(typeof(Border));
        progressBorder.Name = "PART_ProgressBorder";
        progressBorder.SetValue(Border.CornerRadiusProperty, new TemplateBindingExtension(CornerRadiusProperty));
        progressBorder.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Left);

        // Glow layer for enhanced effects
        var glowLayer = new FrameworkElementFactory(typeof(Rectangle));
        glowLayer.Name = "PART_GlowLayer";
        glowLayer.SetValue(Rectangle.RadiusXProperty, new TemplateBindingExtension(CornerRadiusProperty));
        glowLayer.SetValue(Rectangle.RadiusYProperty, new TemplateBindingExtension(CornerRadiusProperty));
        glowLayer.SetValue(Rectangle.HorizontalAlignmentProperty, HorizontalAlignment.Left);
        glowLayer.SetValue(Rectangle.OpacityProperty, 0.6);

        // Particle canvas for stream effects
        var particleCanvas = new FrameworkElementFactory(typeof(Canvas));
        particleCanvas.Name = "PART_ParticleCanvas";
        particleCanvas.SetValue(Canvas.IsHitTestVisibleProperty, false);
        particleCanvas.SetValue(Canvas.ClipToBoundsProperty, true);

        // Text overlay grid
        var textGrid = new FrameworkElementFactory(typeof(Grid));

        // Percentage text
        var percentageText = new FrameworkElementFactory(typeof(TextBlock));
        percentageText.Name = "PART_PercentageText";
        percentageText.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        percentageText.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
        percentageText.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        percentageText.SetValue(TextBlock.FontSizeProperty, 10.0);

        // Progress text
        var progressTextBlock = new FrameworkElementFactory(typeof(TextBlock));
        progressTextBlock.Name = "PART_ProgressTextBlock";
        progressTextBlock.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Left);
        progressTextBlock.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
        progressTextBlock.SetValue(TextBlock.FontSizeProperty, 9.0);
        progressTextBlock.SetValue(TextBlock.MarginProperty, new Thickness(8, 0, 0, 0));
        progressTextBlock.SetBinding(TextBlock.TextProperty, new TemplateBindingExtension(ProgressTextProperty));

        // Assembly
        progressContainer.AppendChild(progressBorder);
        progressContainer.AppendChild(glowLayer);
        
        textGrid.AppendChild(percentageText);
        textGrid.AppendChild(progressTextBlock);

        mainGrid.AppendChild(trackBorder);
        mainGrid.AppendChild(progressContainer);
        mainGrid.AppendChild(particleCanvas);
        mainGrid.AppendChild(textGrid);

        template.VisualTree = mainGrid;
        return template;
    }

    private void SetupAnimations()
    {
        // Particle animation timer
        _particleTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
        };
        _particleTimer.Tick += OnParticleTick;

        // Pulse animation timer
        _pulseTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50) // 20 FPS for pulse
        };
        _pulseTimer.Tick += OnPulseTick;
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        if (!EnableParticleStream || IsInSimplifiedMode || _particleCanvas == null) return;

        _particlePhase += 0.1 * AnimationSpeed;
        if (_particlePhase > Math.PI * 2)
            _particlePhase = 0;

        UpdateParticles();
        SpawnNewParticles();
    }

    private void OnPulseTick(object sender, EventArgs e)
    {
        if (!EnablePulseAnimation || IsInSimplifiedMode) return;

        _pulsePhase += 0.08 * AnimationSpeed;
        if (_pulsePhase > Math.PI * 2)
            _pulsePhase = 0;

        UpdatePulseEffects();
    }

    private void UpdateParticles()
    {
        var progressWidth = GetProgressWidth();
        
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var particle = _particles[i];
            
            // Update particle position
            particle.X += particle.VelocityX * AnimationSpeed;
            particle.Y += particle.VelocityY * AnimationSpeed;
            particle.Life -= 0.02 * AnimationSpeed;

            // Update visual position
            Canvas.SetLeft(particle.Visual, particle.X);
            Canvas.SetTop(particle.Visual, particle.Y);
            
            // Update opacity based on life and progress boundaries
            var withinProgress = particle.X <= progressWidth;
            var baseOpacity = Math.Max(0, particle.Life) * ParticleIntensity;
            particle.Visual.Opacity = withinProgress ? baseOpacity : baseOpacity * 0.3;

            // Remove dead or out-of-bounds particles
            if (particle.Life <= 0 || particle.X > ActualWidth + 20)
            {
                _particleCanvas.Children.Remove(particle.Visual);
                _particles.RemoveAt(i);
            }
        }
    }

    private void SpawnNewParticles()
    {
        var progressWidth = GetProgressWidth();
        if (progressWidth <= 0) return;

        var particleCount = (int)(ParticleIntensity * 3);
        
        for (int i = 0; i < particleCount; i++)
        {
            if (_particles.Count >= 50) break; // Limit particle count

            var particle = CreateProgressParticle();
            _particles.Add(particle);
            _particleCanvas.Children.Add(particle.Visual);
        }
    }

    private ProgressParticle CreateProgressParticle()
    {
        var color = GetEVEColor(EVEColorScheme);
        var stateColor = GetStateColor(ProgressState);
        
        var particleSize = 2 + _random.NextDouble() * 3;
        var shape = _random.Next(3) switch
        {
            0 => CreateCircleParticle(particleSize, stateColor),
            1 => CreateSquareParticle(particleSize, stateColor),
            _ => CreateDiamondParticle(particleSize, stateColor)
        };

        var particle = new ProgressParticle
        {
            Visual = shape,
            X = -particleSize,
            Y = _random.NextDouble() * (ActualHeight - particleSize),
            VelocityX = 30 + _random.NextDouble() * 40,
            VelocityY = (_random.NextDouble() - 0.5) * 10,
            Life = 1.0,
            MaxLife = 2.0 + _random.NextDouble()
        };

        Canvas.SetLeft(shape, particle.X);
        Canvas.SetTop(shape, particle.Y);

        return particle;
    }

    private Shape CreateCircleParticle(double size, Color color)
    {
        return new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromArgb(200, color.R, color.G, color.B)),
            Effect = new BlurEffect { Radius = 1, RenderingBias = RenderingBias.Performance }
        };
    }

    private Shape CreateSquareParticle(double size, Color color)
    {
        return new Rectangle
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromArgb(180, color.R, color.G, color.B)),
            RadiusX = size * 0.2,
            RadiusY = size * 0.2
        };
    }

    private Shape CreateDiamondParticle(double size, Color color)
    {
        var diamond = new Polygon();
        diamond.Points = new PointCollection
        {
            new Point(size/2, 0),
            new Point(size, size/2),
            new Point(size/2, size),
            new Point(0, size/2)
        };
        diamond.Fill = new SolidColorBrush(Color.FromArgb(220, color.R, color.G, color.B));
        return diamond;
    }

    private void UpdatePulseEffects()
    {
        if (_glowLayer?.Effect is DropShadowEffect effect)
        {
            var pulseIntensity = 0.6 + (Math.Sin(_pulsePhase) * 0.4);
            effect.Opacity = HolographicIntensity * pulseIntensity * 0.8;
        }

        if (_progressBorder != null)
        {
            var scaleIntensity = 1.0 + (Math.Sin(_pulsePhase * 2) * 0.02);
            if (_progressBorder.RenderTransform is ScaleTransform transform)
            {
                transform.ScaleY = scaleIntensity;
            }
            else
            {
                _progressBorder.RenderTransform = new ScaleTransform(1, scaleIntensity);
                _progressBorder.RenderTransformOrigin = new Point(0.5, 0.5);
            }
        }
    }

    private double GetProgressWidth()
    {
        if (Maximum <= Minimum) return 0;
        var progressRatio = (Value - Minimum) / (Maximum - Minimum);
        return ActualWidth * progressRatio;
    }

    private void UpdateProgressBarAppearance()
    {
        if (Template == null) return;

        Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            ApplyTemplate();
            
            _mainGrid = GetTemplateChild("PART_MainGrid") as Grid;
            _trackBorder = GetTemplateChild("PART_TrackBorder") as Border;
            _progressBorder = GetTemplateChild("PART_ProgressBorder") as Border;
            _particleCanvas = GetTemplateChild("PART_ParticleCanvas") as Canvas;
            _percentageText = GetTemplateChild("PART_PercentageText") as TextBlock;
            _progressTextBlock = GetTemplateChild("PART_ProgressTextBlock") as TextBlock;
            _glowLayer = GetTemplateChild("PART_GlowLayer") as Rectangle;

            UpdateColors();
            UpdateEffects();
            UpdateProgress();
            UpdateText();
        }), DispatcherPriority.Render);
    }

    private void UpdateColors()
    {
        var color = GetEVEColor(EVEColorScheme);
        var stateColor = GetStateColor(ProgressState);

        // Track colors
        if (_trackBorder != null)
        {
            _trackBorder.Background = new SolidColorBrush(Color.FromArgb(60, 0, 20, 40));
            _trackBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(100, color.R, color.G, color.B));
        }

        // Progress colors
        if (_progressBorder != null)
        {
            var progressBrush = new LinearGradientBrush();
            progressBrush.StartPoint = new Point(0, 0);
            progressBrush.EndPoint = new Point(1, 0);
            progressBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(200, stateColor.R, stateColor.G, stateColor.B), 0.0));
            progressBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(255, stateColor.R, stateColor.G, stateColor.B), 0.5));
            progressBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(180, stateColor.R, stateColor.G, stateColor.B), 1.0));

            _progressBorder.Background = progressBrush;
        }

        // Glow layer
        if (_glowLayer != null)
        {
            _glowLayer.Fill = new SolidColorBrush(Color.FromArgb(100, stateColor.R, stateColor.G, stateColor.B));
        }

        // Text colors
        if (_percentageText != null)
        {
            _percentageText.Foreground = new SolidColorBrush(Colors.White);
        }

        if (_progressTextBlock != null)
        {
            _progressTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(200, color.R, color.G, color.B));
        }
    }

    private void UpdateEffects()
    {
        var stateColor = GetStateColor(ProgressState);

        if (_progressBorder != null && !IsInSimplifiedMode)
        {
            _progressBorder.Effect = new DropShadowEffect
            {
                Color = stateColor,
                BlurRadius = 8 * HolographicIntensity,
                ShadowDepth = 0,
                Opacity = 0.6 * HolographicIntensity
            };
        }
        else if (_progressBorder != null)
        {
            _progressBorder.Effect = new DropShadowEffect
            {
                Color = stateColor,
                BlurRadius = 2,
                ShadowDepth = 0,
                Opacity = 0.3 * HolographicIntensity
            };
        }

        if (_glowLayer != null && !IsInSimplifiedMode)
        {
            _glowLayer.Effect = new BlurEffect
            {
                Radius = 4,
                RenderingBias = RenderingBias.Performance
            };
        }
    }

    private void UpdateProgress()
    {
        if (_progressBorder == null || _glowLayer == null) return;

        var progressWidth = GetProgressWidth();
        
        _progressBorder.Width = progressWidth;
        _glowLayer.Width = progressWidth;
    }

    private void UpdateText()
    {
        if (_percentageText != null)
        {
            if (ShowPercentage && Maximum > Minimum)
            {
                var percentage = ((Value - Minimum) / (Maximum - Minimum)) * 100;
                _percentageText.Text = $"{percentage:F0}%";
                _percentageText.Visibility = Visibility.Visible;
            }
            else
            {
                _percentageText.Visibility = Visibility.Collapsed;
            }
        }

        if (_progressTextBlock != null)
        {
            _progressTextBlock.Visibility = !string.IsNullOrEmpty(ProgressText) ? 
                Visibility.Visible : Visibility.Collapsed;
        }
    }

    private Color GetEVEColor(EVEColorScheme scheme)
    {
        return scheme switch
        {
            EVEColorScheme.ElectricBlue => Color.FromRgb(64, 224, 255),
            EVEColorScheme.GoldAccent => Color.FromRgb(255, 215, 0),
            EVEColorScheme.CrimsonRed => Color.FromRgb(220, 20, 60),
            EVEColorScheme.EmeraldGreen => Color.FromRgb(50, 205, 50),
            EVEColorScheme.VoidPurple => Color.FromRgb(138, 43, 226),
            _ => Color.FromRgb(64, 224, 255)
        };
    }

    private Color GetStateColor(HoloProgressState state)
    {
        return state switch
        {
            HoloProgressState.Normal => GetEVEColor(EVEColorScheme),
            HoloProgressState.Success => Color.FromRgb(50, 205, 50),
            HoloProgressState.Warning => Color.FromRgb(255, 215, 0),
            HoloProgressState.Error => Color.FromRgb(220, 20, 60),
            HoloProgressState.Processing => Color.FromRgb(64, 224, 255),
            _ => GetEVEColor(EVEColorScheme)
        };
    }

    private void OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        UpdateProgress();
        UpdateText();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableParticleStream && !IsInSimplifiedMode)
            _particleTimer.Start();
            
        if (EnablePulseAnimation && !IsInSimplifiedMode)
            _pulseTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _particleTimer?.Stop();
        _pulseTimer?.Stop();
        
        // Clean up particles
        _particles.Clear();
        _particleCanvas?.Children.Clear();
        
        // Unregister from animation intensity manager
        AnimationIntensityManager.Instance.UnregisterTarget(this);
        
        _disposed = true;
    }

    #endregion

    #region Event Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloProgressBar progressBar)
            progressBar.UpdateProgressBarAppearance();
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloProgressBar progressBar)
            progressBar.UpdateProgressBarAppearance();
    }

    private static void OnProgressStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloProgressBar progressBar)
            progressBar.UpdateProgressBarAppearance();
    }

    private static void OnEnableParticleStreamChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloProgressBar progressBar)
        {
            if ((bool)e.NewValue && !progressBar.IsInSimplifiedMode)
                progressBar._particleTimer.Start();
            else
                progressBar._particleTimer.Stop();
        }
    }

    private static void OnEnablePulseAnimationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloProgressBar progressBar)
        {
            if ((bool)e.NewValue && !progressBar.IsInSimplifiedMode)
                progressBar._pulseTimer.Start();
            else
                progressBar._pulseTimer.Stop();
        }
    }

    private static void OnParticleIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Intensity changes are handled in real-time during particle updates
    }

    private static void OnAnimationSpeedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Speed changes are handled in real-time during animation updates
    }

    #endregion
}

/// <summary>
/// Progress particle for stream effects
/// </summary>
internal class ProgressParticle
{
    public FrameworkElement Visual { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
    public double MaxLife { get; set; }
}

/// <summary>
/// Holographic progress bar states for different visual modes
/// </summary>
public enum HoloProgressState
{
    Normal,
    Success,
    Warning,
    Error,
    Processing
}