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
    public class MarketDataParticleSystem : Canvas
    {
        private readonly List<MarketParticle> _particles = new();
        private readonly Random _random = new();
        private readonly DispatcherTimer _animationTimer;
        private readonly CompositeTransform _transform = new();

        public static readonly DependencyProperty ParticleCountProperty =
            DependencyProperty.Register(nameof(ParticleCount), typeof(int), typeof(MarketDataParticleSystem),
                new PropertyMetadata(50, OnParticleCountChanged));

        public static readonly DependencyProperty AnimationSpeedProperty =
            DependencyProperty.Register(nameof(AnimationSpeed), typeof(double), typeof(MarketDataParticleSystem),
                new PropertyMetadata(1.0));

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(MarketDataParticleSystem),
                new PropertyMetadata(true, OnIsActiveChanged));

        public static readonly DependencyProperty DataDirectionProperty =
            DependencyProperty.Register(nameof(DataDirection), typeof(MarketDataDirection), typeof(MarketDataParticleSystem),
                new PropertyMetadata(MarketDataDirection.Mixed, OnDataDirectionChanged));

        public int ParticleCount
        {
            get => (int)GetValue(ParticleCountProperty);
            set => SetValue(ParticleCountProperty, value);
        }

        public double AnimationSpeed
        {
            get => (double)GetValue(AnimationSpeedProperty);
            set => SetValue(AnimationSpeedProperty, value);
        }

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public MarketDataDirection DataDirection
        {
            get => (MarketDataDirection)GetValue(DataDirectionProperty);
            set => SetValue(DataDirectionProperty, value);
        }

        public MarketDataParticleSystem()
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
            InitializeParticles();
            if (IsActive)
                _animationTimer.Start();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _animationTimer.Stop();
            ClearParticles();
        }

        private static void OnParticleCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MarketDataParticleSystem system)
                system.InitializeParticles();
        }

        private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MarketDataParticleSystem system)
            {
                if ((bool)e.NewValue)
                    system._animationTimer.Start();
                else
                    system._animationTimer.Stop();
            }
        }

        private static void OnDataDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MarketDataParticleSystem system)
                system.UpdateParticleColors();
        }

        private void InitializeParticles()
        {
            ClearParticles();

            if (ActualWidth <= 0 || ActualHeight <= 0)
                return;

            for (int i = 0; i < ParticleCount; i++)
            {
                var particle = CreateParticle();
                _particles.Add(particle);
                Children.Add(particle.Visual);
            }
        }

        private MarketParticle CreateParticle()
        {
            var ellipse = new Ellipse
            {
                Width = _random.NextDouble() * 3 + 1,
                Height = _random.NextDouble() * 3 + 1,
                Fill = GetParticleColor(),
                Opacity = 0.7
            };

            var particle = new MarketParticle
            {
                Visual = ellipse,
                X = _random.NextDouble() * ActualWidth,
                Y = _random.NextDouble() * ActualHeight,
                VelocityX = (_random.NextDouble() - 0.5) * 100 * AnimationSpeed,
                VelocityY = (_random.NextDouble() - 0.5) * 100 * AnimationSpeed,
                Life = 1.0,
                MaxLife = _random.NextDouble() * 5 + 2
            };

            Canvas.SetLeft(ellipse, particle.X);
            Canvas.SetTop(ellipse, particle.Y);

            return particle;
        }

        private Brush GetParticleColor()
        {
            return DataDirection switch
            {
                MarketDataDirection.Up => new SolidColorBrush(Color.FromArgb(180, 0, 255, 128)), // Green
                MarketDataDirection.Down => new SolidColorBrush(Color.FromArgb(180, 255, 64, 64)), // Red
                MarketDataDirection.Mixed => _random.NextDouble() > 0.5 
                    ? new SolidColorBrush(Color.FromArgb(180, 0, 255, 128))
                    : new SolidColorBrush(Color.FromArgb(180, 255, 64, 64)),
                _ => new SolidColorBrush(Color.FromArgb(180, 64, 224, 255)) // EVE Blue
            };
        }

        private void UpdateParticleColors()
        {
            foreach (var particle in _particles)
            {
                if (particle.Visual is Ellipse ellipse)
                    ellipse.Fill = GetParticleColor();
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

                // Update life
                particle.Life -= deltaTime / particle.MaxLife;

                // Wrap around edges
                if (particle.X < 0) particle.X = ActualWidth;
                if (particle.X > ActualWidth) particle.X = 0;
                if (particle.Y < 0) particle.Y = ActualHeight;
                if (particle.Y > ActualHeight) particle.Y = 0;

                // Update visual position and opacity
                Canvas.SetLeft(particle.Visual, particle.X);
                Canvas.SetTop(particle.Visual, particle.Y);
                particle.Visual.Opacity = Math.Max(0, particle.Life * 0.7);

                // Respawn particle if dead
                if (particle.Life <= 0)
                {
                    RespawnParticle(particle);
                }
            }
        }

        private void RespawnParticle(MarketParticle particle)
        {
            particle.X = _random.NextDouble() * ActualWidth;
            particle.Y = _random.NextDouble() * ActualHeight;
            particle.VelocityX = (_random.NextDouble() - 0.5) * 100 * AnimationSpeed;
            particle.VelocityY = (_random.NextDouble() - 0.5) * 100 * AnimationSpeed;
            particle.Life = 1.0;
            particle.MaxLife = _random.NextDouble() * 5 + 2;

            if (particle.Visual is Ellipse ellipse)
            {
                ellipse.Width = _random.NextDouble() * 3 + 1;
                ellipse.Height = _random.NextDouble() * 3 + 1;
                ellipse.Fill = GetParticleColor();
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
            if (_particles.Count == 0 && ParticleCount > 0)
                InitializeParticles();
            
            return arrangeSize;
        }
    }

    public class MarketParticle
    {
        public FrameworkElement Visual { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public double Life { get; set; }
        public double MaxLife { get; set; }
    }

    public enum MarketDataDirection
    {
        Up,
        Down,
        Mixed,
        Neutral
    }
}