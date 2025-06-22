using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
            var particleCount = Math.Min(20, (int)(magnitude * 10 * Intensity));

            for (int i = 0; i < particleCount; i++)
            {
                var particle = CreateFluctuationParticle(isPositive, magnitude);
                _particles.Add(particle);
                Children.Add(particle.Visual);
            }
        }

        private PriceParticle CreateFluctuationParticle(bool isPositive, double magnitude)
        {
            var size = Math.Min(8, 2 + magnitude / 2);
            var shape = _random.Next(3) switch
            {
                0 => CreateArrowShape(size, isPositive),
                1 => CreateDiamondShape(size),
                _ => CreateCircleShape(size)
            };

            shape.Fill = GetFluctuationColor(isPositive, magnitude);
            shape.Opacity = 0.9;

            var startX = _random.NextDouble() * ActualWidth;
            var startY = ActualHeight / 2;
            
            var particle = new PriceParticle
            {
                Visual = shape,
                X = startX,
                Y = startY,
                VelocityX = (_random.NextDouble() - 0.5) * 50,
                VelocityY = isPositive ? -50 - magnitude * 2 : 50 + magnitude * 2,
                Life = 1.0,
                MaxLife = 2.0 + magnitude / 10,
                RotationSpeed = (_random.NextDouble() - 0.5) * 180,
                ScaleSpeed = -0.5
            };

            Canvas.SetLeft(shape, particle.X);
            Canvas.SetTop(shape, particle.Y);

            // Add glow effect for significant changes
            if (magnitude > 5)
            {
                shape.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = isPositive ? Colors.LimeGreen : Colors.Red,
                    BlurRadius = 10,
                    ShadowDepth = 0,
                    Opacity = 0.7
                };
            }

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
                
                // Update position
                particle.X += particle.VelocityX * deltaTime;
                particle.Y += particle.VelocityY * deltaTime;

                // Apply gravity/drift
                particle.VelocityY += 20 * deltaTime; // Slight gravity
                particle.VelocityX *= 0.98; // Air resistance

                // Update life
                particle.Life -= deltaTime / particle.MaxLife;

                // Update rotation
                var currentRotation = particle.Visual.RenderTransform as RotateTransform 
                    ?? new RotateTransform();
                currentRotation.Angle += particle.RotationSpeed * deltaTime;
                particle.Visual.RenderTransform = currentRotation;
                particle.Visual.RenderTransformOrigin = new Point(0.5, 0.5);

                // Update scale
                var currentScale = particle.Visual.LayoutTransform as ScaleTransform 
                    ?? new ScaleTransform(1, 1);
                var newScale = Math.Max(0.1, currentScale.ScaleX + particle.ScaleSpeed * deltaTime);
                particle.Visual.LayoutTransform = new ScaleTransform(newScale, newScale);

                // Update visual position and opacity
                Canvas.SetLeft(particle.Visual, particle.X);
                Canvas.SetTop(particle.Visual, particle.Y);
                particle.Visual.Opacity = Math.Max(0, particle.Life * 0.9);

                // Remove dead particles
                if (particle.Life <= 0 || particle.Y > ActualHeight + 50 || particle.Y < -50)
                {
                    Children.Remove(particle.Visual);
                    _particles.RemoveAt(i);
                }
            }
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
    }
}