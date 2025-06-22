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

namespace Gideon.WPF.Presentation.Controls
{
    public class PriceFluctuationParticles : Canvas
    {
        private readonly List<PriceParticle> _particles = new();
        private readonly Random _random = new();
        private readonly DispatcherTimer _animationTimer;
        
        public static readonly DependencyProperty CurrentPriceProperty =
            DependencyProperty.Register(nameof(CurrentPrice), typeof(decimal), typeof(PriceFluctuationParticles),
                new PropertyMetadata(0m, OnPriceChanged));

        public static readonly DependencyProperty PreviousPriceProperty =
            DependencyProperty.Register(nameof(PreviousPrice), typeof(decimal), typeof(PriceFluctuationParticles),
                new PropertyMetadata(0m, OnPriceChanged));

        public static readonly DependencyProperty IntensityProperty =
            DependencyProperty.Register(nameof(Intensity), typeof(double), typeof(PriceFluctuationParticles),
                new PropertyMetadata(1.0));

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(PriceFluctuationParticles),
                new PropertyMetadata(true, OnIsActiveChanged));

        public static readonly DependencyProperty FluctuationTypeProperty =
            DependencyProperty.Register(nameof(FluctuationType), typeof(FluctuationType), typeof(PriceFluctuationParticles),
                new PropertyMetadata(FluctuationType.Mixed));

        public static readonly DependencyProperty ParticleLifetimeProperty =
            DependencyProperty.Register(nameof(ParticleLifetime), typeof(double), typeof(PriceFluctuationParticles),
                new PropertyMetadata(3.0));

        public static readonly DependencyProperty EmissionRateProperty =
            DependencyProperty.Register(nameof(EmissionRate), typeof(double), typeof(PriceFluctuationParticles),
                new PropertyMetadata(1.0));

        public decimal CurrentPrice
        {
            get => (decimal)GetValue(CurrentPriceProperty);
            set => SetValue(CurrentPriceProperty, value);
        }

        public decimal PreviousPrice
        {
            get => (decimal)GetValue(PreviousPriceProperty);
            set => SetValue(PreviousPriceProperty, value);
        }

        public double Intensity
        {
            get => (double)GetValue(IntensityProperty);
            set => SetValue(IntensityProperty, value);
        }

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public FluctuationType FluctuationType
        {
            get => (FluctuationType)GetValue(FluctuationTypeProperty);
            set => SetValue(FluctuationTypeProperty, value);
        }

        public double ParticleLifetime
        {
            get => (double)GetValue(ParticleLifetimeProperty);
            set => SetValue(ParticleLifetimeProperty, value);
        }

        public double EmissionRate
        {
            get => (double)GetValue(EmissionRateProperty);
            set => SetValue(EmissionRateProperty, value);
        }

        public PriceFluctuationParticles()
        {
            Background = Brushes.Transparent;
            ClipToBounds = true;

            _animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
            };
            _animationTimer.Tick += OnAnimationTick;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (IsActive)
                _animationTimer.Start();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _animationTimer.Stop();
            ClearParticles();
        }

        private static void OnPriceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PriceFluctuationParticles system)
                system.OnPriceUpdated();
        }

        private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PriceFluctuationParticles system)
            {
                if ((bool)e.NewValue)
                    system._animationTimer.Start();
                else
                    system._animationTimer.Stop();
            }
        }

        private void OnPriceUpdated()
        {
            if (CurrentPrice == 0 || PreviousPrice == 0)
                return;

            var priceChange = CurrentPrice - PreviousPrice;
            var percentChange = (double)(priceChange / PreviousPrice) * 100;
            
            // Emit particles based on price movement
            EmitFluctuationParticles(percentChange);
        }

        private void EmitFluctuationParticles(double percentChange)
        {
            var isPositive = percentChange > 0;
            var magnitude = Math.Abs(percentChange);
            var baseParticleCount = Math.Min(30, (int)(magnitude * 15 * Intensity * EmissionRate));
            
            // Emit different types based on fluctuation type and magnitude
            switch (FluctuationType)
            {
                case FluctuationType.Burst:
                    EmitBurstParticles(isPositive, magnitude, baseParticleCount);
                    break;
                case FluctuationType.Stream:
                    EmitStreamParticles(isPositive, magnitude, baseParticleCount / 3);
                    break;
                case FluctuationType.Explosion:
                    EmitExplosionParticles(isPositive, magnitude, baseParticleCount * 2);
                    break;
                default:
                    EmitMixedParticles(isPositive, magnitude, baseParticleCount);
                    break;
            }

            // Add market volatility indicators for significant changes
            if (magnitude > 10)
            {
                EmitVolatilityIndicators(isPositive, magnitude);
            }
        }

        private void EmitBurstParticles(bool isPositive, double magnitude, int count)
        {
            var centerX = ActualWidth / 2;
            var centerY = ActualHeight / 2;

            for (int i = 0; i < count; i++)
            {
                var angle = (i / (double)count) * 2 * Math.PI;
                var speed = 80 + magnitude * 5;
                
                var particle = CreateEnhancedFluctuationParticle(isPositive, magnitude, ParticleEffect.Burst);
                particle.X = centerX;
                particle.Y = centerY;
                particle.VelocityX = Math.Cos(angle) * speed;
                particle.VelocityY = Math.Sin(angle) * speed;
                
                _particles.Add(particle);
                Children.Add(particle.Visual);
            }
        }

        private void EmitStreamParticles(bool isPositive, double magnitude, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var particle = CreateEnhancedFluctuationParticle(isPositive, magnitude, ParticleEffect.Stream);
                particle.X = _random.NextDouble() * ActualWidth;
                particle.Y = isPositive ? ActualHeight : 0;
                particle.VelocityX = (_random.NextDouble() - 0.5) * 30;
                particle.VelocityY = isPositive ? -120 - magnitude * 3 : 120 + magnitude * 3;
                
                _particles.Add(particle);
                Children.Add(particle.Visual);
            }
        }

        private void EmitExplosionParticles(bool isPositive, double magnitude, int count)
        {
            var centerX = ActualWidth / 2;
            var centerY = ActualHeight / 2;

            for (int i = 0; i < count; i++)
            {
                var angle = _random.NextDouble() * 2 * Math.PI;
                var speed = 150 + _random.NextDouble() * magnitude * 10;
                
                var particle = CreateEnhancedFluctuationParticle(isPositive, magnitude, ParticleEffect.Explosion);
                particle.X = centerX + (_random.NextDouble() - 0.5) * 40;
                particle.Y = centerY + (_random.NextDouble() - 0.5) * 40;
                particle.VelocityX = Math.Cos(angle) * speed;
                particle.VelocityY = Math.Sin(angle) * speed;
                
                _particles.Add(particle);
                Children.Add(particle.Visual);
            }
        }

        private void EmitMixedParticles(bool isPositive, double magnitude, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var effectType = (ParticleEffect)_random.Next(3);
                var particle = CreateEnhancedFluctuationParticle(isPositive, magnitude, effectType);
                
                particle.X = _random.NextDouble() * ActualWidth;
                particle.Y = _random.NextDouble() * ActualHeight;
                particle.VelocityX = (_random.NextDouble() - 0.5) * 100;
                particle.VelocityY = (_random.NextDouble() - 0.5) * 100;
                
                _particles.Add(particle);
                Children.Add(particle.Visual);
            }
        }

        private void EmitVolatilityIndicators(bool isPositive, double magnitude)
        {
            // Create warning indicators for high volatility
            for (int i = 0; i < 3; i++)
            {
                var indicator = CreateVolatilityIndicator(isPositive, magnitude);
                _particles.Add(indicator);
                Children.Add(indicator.Visual);
            }
        }

        private PriceParticle CreateEnhancedFluctuationParticle(bool isPositive, double magnitude, ParticleEffect effect)
        {
            var size = Math.Min(12, 2 + magnitude / 2);
            Shape shape;

            switch (effect)
            {
                case ParticleEffect.Burst:
                    shape = CreateStarBurstShape(size);
                    break;
                case ParticleEffect.Stream:
                    shape = CreateStreamlineShape(size, isPositive);
                    break;
                case ParticleEffect.Explosion:
                    shape = CreateFragmentShape(size);
                    break;
                default:
                    shape = _random.Next(4) switch
                    {
                        0 => CreateArrowShape(size, isPositive),
                        1 => CreateDiamondShape(size),
                        2 => CreateHexagonShape(size),
                        _ => CreateCircleShape(size)
                    };
                    break;
            }

            shape.Fill = GetEnhancedFluctuationColor(isPositive, magnitude, effect);
            shape.Opacity = 0.85 + _random.NextDouble() * 0.15;

            var particle = new PriceParticle
            {
                Visual = shape,
                X = 0, // Will be set by caller
                Y = 0, // Will be set by caller
                VelocityX = 0, // Will be set by caller
                VelocityY = 0, // Will be set by caller
                Life = 1.0,
                MaxLife = ParticleLifetime + magnitude / 10,
                RotationSpeed = (_random.NextDouble() - 0.5) * 360,
                ScaleSpeed = -0.3 - magnitude * 0.05,
                Effect = effect,
                Magnitude = magnitude
            };

            Canvas.SetLeft(shape, particle.X);
            Canvas.SetTop(shape, particle.Y);

            // Add enhanced visual effects
            ApplyVisualEffects(shape, isPositive, magnitude, effect);

            return particle;
        }

        private PriceParticle CreateVolatilityIndicator(bool isPositive, double magnitude)
        {
            var size = 15 + magnitude;
            var shape = CreateWarningShape(size);
            
            shape.Fill = new SolidColorBrush(Color.FromArgb(200, 255, 215, 0)); // Gold warning
            shape.Stroke = new SolidColorBrush(isPositive ? Colors.LimeGreen : Colors.Red);
            shape.StrokeThickness = 2;

            var particle = new PriceParticle
            {
                Visual = shape,
                X = _random.NextDouble() * ActualWidth,
                Y = _random.NextDouble() * ActualHeight,
                VelocityX = (_random.NextDouble() - 0.5) * 20,
                VelocityY = (_random.NextDouble() - 0.5) * 20,
                Life = 1.0,
                MaxLife = ParticleLifetime * 2,
                RotationSpeed = (_random.NextDouble() - 0.5) * 90,
                ScaleSpeed = 0.1, // Grow instead of shrink
                Effect = ParticleEffect.Warning,
                Magnitude = magnitude
            };

            // Pulsing glow effect for volatility warnings
            shape.Effect = new DropShadowEffect
            {
                Color = Colors.Gold,
                BlurRadius = 15,
                ShadowDepth = 0,
                Opacity = 0.8
            };

            return particle;
        }

        private Shape CreateArrowShape(double size, bool pointsUp)
        {
            var polygon = new Polygon();
            var points = new PointCollection();
            
            if (pointsUp)
            {
                points.Add(new Point(size / 2, 0));
                points.Add(new Point(size, size));
                points.Add(new Point(size * 0.7, size));
                points.Add(new Point(size * 0.7, size * 1.5));
                points.Add(new Point(size * 0.3, size * 1.5));
                points.Add(new Point(size * 0.3, size));
                points.Add(new Point(0, size));
            }
            else
            {
                points.Add(new Point(size / 2, size * 1.5));
                points.Add(new Point(0, size * 0.5));
                points.Add(new Point(size * 0.3, size * 0.5));
                points.Add(new Point(size * 0.3, 0));
                points.Add(new Point(size * 0.7, 0));
                points.Add(new Point(size * 0.7, size * 0.5));
                points.Add(new Point(size, size * 0.5));
            }
            
            polygon.Points = points;
            return polygon;
        }

        private Shape CreateDiamondShape(double size)
        {
            var polygon = new Polygon();
            polygon.Points = new PointCollection
            {
                new Point(size / 2, 0),
                new Point(size, size / 2),
                new Point(size / 2, size),
                new Point(0, size / 2)
            };
            return polygon;
        }

        private Shape CreateCircleShape(double size)
        {
            return new Ellipse
            {
                Width = size,
                Height = size
            };
        }

        private Shape CreateStarBurstShape(double size)
        {
            var polygon = new Polygon();
            var points = new PointCollection();
            var outerRadius = size;
            var innerRadius = size * 0.4;
            
            for (int i = 0; i < 8; i++)
            {
                var outerAngle = i * Math.PI / 4;
                var innerAngle = (i + 0.5) * Math.PI / 4;
                
                points.Add(new Point(
                    outerRadius + outerRadius * Math.Cos(outerAngle),
                    outerRadius + outerRadius * Math.Sin(outerAngle)
                ));
                points.Add(new Point(
                    outerRadius + innerRadius * Math.Cos(innerAngle),
                    outerRadius + innerRadius * Math.Sin(innerAngle)
                ));
            }
            
            polygon.Points = points;
            return polygon;
        }

        private Shape CreateStreamlineShape(double size, bool isPositive)
        {
            var polygon = new Polygon();
            var points = new PointCollection();
            
            if (isPositive)
            {
                // Upward teardrop
                points.Add(new Point(size / 2, 0));
                points.Add(new Point(size * 0.8, size * 0.3));
                points.Add(new Point(size, size));
                points.Add(new Point(0, size));
                points.Add(new Point(size * 0.2, size * 0.3));
            }
            else
            {
                // Downward teardrop
                points.Add(new Point(0, 0));
                points.Add(new Point(size, 0));
                points.Add(new Point(size * 0.8, size * 0.7));
                points.Add(new Point(size / 2, size));
                points.Add(new Point(size * 0.2, size * 0.7));
            }
            
            polygon.Points = points;
            return polygon;
        }

        private Shape CreateFragmentShape(double size)
        {
            var polygon = new Polygon();
            var points = new PointCollection();
            var vertices = 5 + _random.Next(3);
            
            for (int i = 0; i < vertices; i++)
            {
                var angle = (i / (double)vertices) * 2 * Math.PI;
                var radius = size * (0.7 + _random.NextDouble() * 0.6);
                points.Add(new Point(
                    size + radius * Math.Cos(angle),
                    size + radius * Math.Sin(angle)
                ));
            }
            
            polygon.Points = points;
            return polygon;
        }

        private Shape CreateHexagonShape(double size)
        {
            var polygon = new Polygon();
            var points = new PointCollection();
            
            for (int i = 0; i < 6; i++)
            {
                var angle = i * Math.PI / 3;
                points.Add(new Point(
                    size / 2 + (size / 2) * Math.Cos(angle),
                    size / 2 + (size / 2) * Math.Sin(angle)
                ));
            }
            
            polygon.Points = points;
            return polygon;
        }

        private Shape CreateWarningShape(double size)
        {
            var polygon = new Polygon();
            polygon.Points = new PointCollection
            {
                new Point(size / 2, 0),
                new Point(size, size * 0.866),
                new Point(0, size * 0.866)
            };
            return polygon;
        }

        private Brush GetEnhancedFluctuationColor(bool isPositive, double magnitude, ParticleEffect effect)
        {
            Color baseColor;
            
            if (isPositive)
            {
                // EVE-style positive colors: Electric green to bright cyan
                var intensity = Math.Min(255, 150 + magnitude * 8);
                baseColor = effect switch
                {
                    ParticleEffect.Burst => Color.FromRgb(0, (byte)intensity, 255), // Electric blue-cyan
                    ParticleEffect.Stream => Color.FromRgb(0, (byte)intensity, 200), // Stream green-cyan
                    ParticleEffect.Explosion => Color.FromRgb(64, (byte)intensity, 255), // Bright electric
                    _ => Color.FromRgb(0, (byte)intensity, 128) // Standard positive green
                };
            }
            else
            {
                // EVE-style negative colors: Deep red to orange-red
                var intensity = Math.Min(255, 150 + magnitude * 8);
                baseColor = effect switch
                {
                    ParticleEffect.Burst => Color.FromRgb((byte)intensity, 64, 0), // Orange-red burst
                    ParticleEffect.Stream => Color.FromRgb((byte)intensity, 32, 32), // Deep red stream
                    ParticleEffect.Explosion => Color.FromRgb(255, (byte)(intensity * 0.7), 0), // Explosive orange
                    _ => Color.FromRgb((byte)intensity, 64, 64) // Standard negative red
                };
            }

            // Add gradient for more dynamic appearance
            var gradientBrush = new RadialGradientBrush();
            gradientBrush.GradientStops.Add(new GradientStop(baseColor, 0.0));
            gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(100, baseColor.R, baseColor.G, baseColor.B), 0.7));
            gradientBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));
            
            return gradientBrush;
        }

        private void ApplyVisualEffects(Shape shape, bool isPositive, double magnitude, ParticleEffect effect)
        {
            // Apply glow effects based on magnitude and type
            if (magnitude > 3)
            {
                var glowColor = isPositive ? Colors.LimeGreen : Colors.OrangeRed;
                var blurRadius = Math.Min(20, 5 + magnitude * 1.5);
                
                shape.Effect = new DropShadowEffect
                {
                    Color = glowColor,
                    BlurRadius = blurRadius,
                    ShadowDepth = 0,
                    Opacity = 0.6 + Math.Min(0.4, magnitude * 0.05)
                };
            }

            // Add stroke for certain effects
            if (effect == ParticleEffect.Burst || effect == ParticleEffect.Warning)
            {
                shape.Stroke = new SolidColorBrush(isPositive ? Colors.Cyan : Colors.Yellow);
                shape.StrokeThickness = 1;
            }
        }

        private Brush GetFluctuationColor(bool isPositive, double magnitude)
        {
            if (isPositive)
            {
                // Green for positive changes
                var intensity = Math.Min(255, 128 + magnitude * 10);
                return new SolidColorBrush(Color.FromArgb(200, 0, (byte)intensity, 64));
            }
            else
            {
                // Red for negative changes
                var intensity = Math.Min(255, 128 + magnitude * 10);
                return new SolidColorBrush(Color.FromArgb(200, (byte)intensity, 64, 64));
            }
        }

        private void OnAnimationTick(object sender, EventArgs e)
        {
            var deltaTime = 0.016; // 16ms

            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var particle = _particles[i];
                
                // Update position based on particle effect
                UpdateParticlePosition(particle, deltaTime);

                // Update life
                particle.Life -= deltaTime / particle.MaxLife;

                // Update rotation with effect-specific behavior
                UpdateParticleRotation(particle, deltaTime);

                // Update scale with enhanced effects
                UpdateParticleScale(particle, deltaTime);

                // Update visual effects
                UpdateParticleVisualEffects(particle, deltaTime);

                // Update visual position and opacity
                Canvas.SetLeft(particle.Visual, particle.X);
                Canvas.SetTop(particle.Visual, particle.Y);
                
                var baseOpacity = particle.Life * 0.9;
                if (particle.Effect == ParticleEffect.Warning)
                {
                    // Pulsing opacity for warnings
                    baseOpacity *= 0.7 + 0.3 * Math.Sin(DateTime.Now.Millisecond * 0.02);
                }
                particle.Visual.Opacity = Math.Max(0, baseOpacity);

                // Remove dead particles
                if (particle.Life <= 0 || IsParticleOutOfBounds(particle))
                {
                    Children.Remove(particle.Visual);
                    _particles.RemoveAt(i);
                }
            }
        }

        private void UpdateParticlePosition(PriceParticle particle, double deltaTime)
        {
            particle.X += particle.VelocityX * deltaTime;
            particle.Y += particle.VelocityY * deltaTime;

            switch (particle.Effect)
            {
                case ParticleEffect.Stream:
                    // Minimal air resistance for streams
                    particle.VelocityX *= 0.995;
                    particle.VelocityY += 10 * deltaTime; // Light gravity
                    break;
                case ParticleEffect.Explosion:
                    // Strong air resistance and gravity for explosion fragments
                    particle.VelocityX *= 0.92;
                    particle.VelocityY += 50 * deltaTime;
                    break;
                case ParticleEffect.Warning:
                    // Floating behavior for warnings
                    particle.VelocityY += Math.Sin(DateTime.Now.Millisecond * 0.01) * 5 * deltaTime;
                    break;
                default:
                    // Standard physics
                    particle.VelocityY += 20 * deltaTime;
                    particle.VelocityX *= 0.98;
                    break;
            }
        }

        private void UpdateParticleRotation(PriceParticle particle, double deltaTime)
        {
            var currentRotation = particle.Visual.RenderTransform as RotateTransform 
                ?? new RotateTransform();
            
            var rotationDelta = particle.RotationSpeed * deltaTime;
            if (particle.Effect == ParticleEffect.Explosion)
            {
                // Accelerating rotation for explosion fragments
                particle.RotationSpeed *= 1.02;
            }
            
            currentRotation.Angle += rotationDelta;
            particle.Visual.RenderTransform = currentRotation;
            particle.Visual.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        private void UpdateParticleScale(PriceParticle particle, double deltaTime)
        {
            var currentScale = particle.Visual.LayoutTransform as ScaleTransform 
                ?? new ScaleTransform(1, 1);
            
            var scaleDelta = particle.ScaleSpeed * deltaTime;
            if (particle.Effect == ParticleEffect.Warning && particle.ScaleSpeed > 0)
            {
                // Oscillating scale for warnings
                scaleDelta *= 1 + 0.5 * Math.Sin(DateTime.Now.Millisecond * 0.015);
            }
            
            var newScale = Math.Max(0.1, currentScale.ScaleX + scaleDelta);
            particle.Visual.LayoutTransform = new ScaleTransform(newScale, newScale);
        }

        private void UpdateParticleVisualEffects(PriceParticle particle, double deltaTime)
        {
            // Update glow intensity based on life and magnitude
            if (particle.Visual.Effect is DropShadowEffect shadow)
            {
                var intensityMultiplier = particle.Life * (0.5 + particle.Magnitude * 0.05);
                shadow.Opacity = Math.Max(0.1, intensityMultiplier);
            }
        }

        private bool IsParticleOutOfBounds(PriceParticle particle)
        {
            return particle.X < -50 || particle.X > ActualWidth + 50 || 
                   particle.Y < -50 || particle.Y > ActualHeight + 50;
        }

        private void ClearParticles()
        {
            foreach (var particle in _particles)
                Children.Remove(particle.Visual);
            _particles.Clear();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            return constraint;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            return arrangeSize;
        }
    }

    public class PriceParticle
    {
        public FrameworkElement Visual { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public double Life { get; set; }
        public double MaxLife { get; set; }
        public double RotationSpeed { get; set; }
        public double ScaleSpeed { get; set; }
        public ParticleEffect Effect { get; set; } = ParticleEffect.Standard;
        public double Magnitude { get; set; }
    }

    public enum FluctuationType
    {
        Mixed,
        Burst,
        Stream,
        Explosion
    }

    public enum ParticleEffect
    {
        Standard,
        Burst,
        Stream,
        Explosion,
        Warning
    }
}