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
    public class MarketDataParticleSystem : Canvas
    {
        private readonly List<MarketParticle> _particles = new();
        private readonly List<DataStream> _dataStreams = new();
        private readonly Random _random = new();
        private readonly DispatcherTimer _animationTimer;
        private readonly PerformanceTracker _performanceTracker = new();

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

        public static readonly DependencyProperty StreamCountProperty =
            DependencyProperty.Register(nameof(StreamCount), typeof(int), typeof(MarketDataParticleSystem),
                new PropertyMetadata(3, OnStreamCountChanged));

        public static readonly DependencyProperty DataIntensityProperty =
            DependencyProperty.Register(nameof(DataIntensity), typeof(double), typeof(MarketDataParticleSystem),
                new PropertyMetadata(1.0, OnDataIntensityChanged));

        public static readonly DependencyProperty StreamTypeProperty =
            DependencyProperty.Register(nameof(StreamType), typeof(StreamFlowType), typeof(MarketDataParticleSystem),
                new PropertyMetadata(StreamFlowType.Flowing, OnStreamTypeChanged));

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

        public int StreamCount
        {
            get => (int)GetValue(StreamCountProperty);
            set => SetValue(StreamCountProperty, value);
        }

        public double DataIntensity
        {
            get => (double)GetValue(DataIntensityProperty);
            set => SetValue(DataIntensityProperty, value);
        }

        public StreamFlowType StreamType
        {
            get => (StreamFlowType)GetValue(StreamTypeProperty);
            set => SetValue(StreamTypeProperty, value);
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
            SizeChanged += OnSizeChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            InitializeStreams();
            InitializeParticles();
            if (IsActive)
                _animationTimer.Start();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _animationTimer.Stop();
            ClearParticles();
            ClearStreams();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsLoaded)
            {
                InitializeStreams();
                InitializeParticles();
            }
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

        private static void OnStreamCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MarketDataParticleSystem system && system.IsLoaded)
                system.InitializeStreams();
        }

        private static void OnDataIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MarketDataParticleSystem system)
                system.UpdateStreamIntensity();
        }

        private static void OnStreamTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MarketDataParticleSystem system && system.IsLoaded)
                system.InitializeStreams();
        }

        private void InitializeStreams()
        {
            ClearStreams();

            if (ActualWidth <= 0 || ActualHeight <= 0)
                return;

            for (int i = 0; i < StreamCount; i++)
            {
                var stream = CreateDataStream(i);
                _dataStreams.Add(stream);
            }
        }

        private DataStream CreateDataStream(int index)
        {
            var streamWidth = ActualWidth / StreamCount;
            var startX = index * streamWidth + streamWidth / 2;

            return StreamType switch
            {
                StreamFlowType.Vertical => new DataStream
                {
                    Id = index,
                    StartX = startX,
                    StartY = ActualHeight,
                    EndX = startX + (_random.NextDouble() - 0.5) * 50,
                    EndY = 0,
                    FlowDirection = Vector.Parse("0,-1"),
                    StreamWidth = 20 + _random.NextDouble() * 30,
                    Intensity = DataIntensity,
                    ParticleSpawnRate = 0.3 + DataIntensity * 0.7
                },
                StreamFlowType.Horizontal => new DataStream
                {
                    Id = index,
                    StartX = 0,
                    StartY = (index + 1) * ActualHeight / (StreamCount + 1),
                    EndX = ActualWidth,
                    EndY = (index + 1) * ActualHeight / (StreamCount + 1) + (_random.NextDouble() - 0.5) * 50,
                    FlowDirection = Vector.Parse("1,0"),
                    StreamWidth = 15 + _random.NextDouble() * 25,
                    Intensity = DataIntensity,
                    ParticleSpawnRate = 0.2 + DataIntensity * 0.8
                },
                StreamFlowType.Diagonal => new DataStream
                {
                    Id = index,
                    StartX = index % 2 == 0 ? 0 : ActualWidth,
                    StartY = ActualHeight,
                    EndX = index % 2 == 0 ? ActualWidth : 0,
                    EndY = 0,
                    FlowDirection = Vector.Parse(index % 2 == 0 ? "1,-1" : "-1,-1"),
                    StreamWidth = 25 + _random.NextDouble() * 20,
                    Intensity = DataIntensity,
                    ParticleSpawnRate = 0.25 + DataIntensity * 0.75
                },
                _ => new DataStream
                {
                    Id = index,
                    StartX = startX,
                    StartY = ActualHeight / 2 + (_random.NextDouble() - 0.5) * ActualHeight * 0.8,
                    EndX = startX + (_random.NextDouble() - 0.5) * 100,
                    EndY = ActualHeight / 2 + (_random.NextDouble() - 0.5) * ActualHeight * 0.8,
                    FlowDirection = new Vector((_random.NextDouble() - 0.5) * 2, (_random.NextDouble() - 0.5) * 2),
                    StreamWidth = 30 + _random.NextDouble() * 40,
                    Intensity = DataIntensity,
                    ParticleSpawnRate = 0.15 + DataIntensity * 0.85
                }
            };
        }

        private void InitializeParticles()
        {
            ClearParticles();

            if (ActualWidth <= 0 || ActualHeight <= 0)
                return;

            // Create particles for each data stream
            foreach (var stream in _dataStreams)
            {
                var streamParticleCount = (int)(ParticleCount * stream.ParticleSpawnRate / _dataStreams.Count);
                for (int i = 0; i < streamParticleCount; i++)
                {
                    var particle = CreateStreamParticle(stream);
                    _particles.Add(particle);
                    Children.Add(particle.Visual);
                }
            }

            // Fill remaining with ambient particles
            while (_particles.Count < ParticleCount)
            {
                var particle = CreateAmbientParticle();
                _particles.Add(particle);
                Children.Add(particle.Visual);
            }
        }

        private MarketParticle CreateStreamParticle(DataStream stream)
        {
            var size = 1.5 + _random.NextDouble() * 2.5 * stream.Intensity;
            var ellipse = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = GetStreamParticleColor(stream),
                Opacity = 0.6 + _random.NextDouble() * 0.3,
                Effect = new DropShadowEffect
                {
                    Color = GetStreamColor(stream),
                    BlurRadius = 3,
                    ShadowDepth = 0,
                    Opacity = 0.8
                }
            };

            var startPoint = GetStreamStartPoint(stream);
            var velocity = GetStreamVelocity(stream);

            var particle = new MarketParticle
            {
                Visual = ellipse,
                X = startPoint.X,
                Y = startPoint.Y,
                VelocityX = velocity.X * AnimationSpeed,
                VelocityY = velocity.Y * AnimationSpeed,
                Life = 1.0,
                MaxLife = 3 + _random.NextDouble() * 4,
                StreamId = stream.Id,
                ParticleType = ParticleType.Stream,
                Size = size
            };

            Canvas.SetLeft(ellipse, particle.X);
            Canvas.SetTop(ellipse, particle.Y);

            return particle;
        }

        private MarketParticle CreateAmbientParticle()
        {
            var size = 0.8 + _random.NextDouble() * 1.5;
            var ellipse = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = GetAmbientParticleColor(),
                Opacity = 0.3 + _random.NextDouble() * 0.4
            };

            var particle = new MarketParticle
            {
                Visual = ellipse,
                X = _random.NextDouble() * ActualWidth,
                Y = _random.NextDouble() * ActualHeight,
                VelocityX = (_random.NextDouble() - 0.5) * 20 * AnimationSpeed,
                VelocityY = (_random.NextDouble() - 0.5) * 20 * AnimationSpeed,
                Life = 1.0,
                MaxLife = _random.NextDouble() * 8 + 4,
                StreamId = -1,
                ParticleType = ParticleType.Ambient,
                Size = size
            };

            Canvas.SetLeft(ellipse, particle.X);
            Canvas.SetTop(ellipse, particle.Y);

            return particle;
        }

        private Point GetStreamStartPoint(DataStream stream)
        {
            var variance = stream.StreamWidth * 0.5;
            return new Point(
                stream.StartX + (_random.NextDouble() - 0.5) * variance,
                stream.StartY + (_random.NextDouble() - 0.5) * variance * 0.3
            );
        }

        private Vector GetStreamVelocity(DataStream stream)
        {
            var baseVelocity = stream.FlowDirection;
            baseVelocity.Normalize();
            
            var speed = 80 + _random.NextDouble() * 120 * stream.Intensity;
            var variance = 0.3;
            
            return new Vector(
                baseVelocity.X * speed + (_random.NextDouble() - 0.5) * speed * variance,
                baseVelocity.Y * speed + (_random.NextDouble() - 0.5) * speed * variance
            );
        }

        private Brush GetStreamParticleColor(DataStream stream)
        {
            var baseColor = GetStreamColor(stream);
            var alpha = (byte)(120 + _random.Next(60));
            return new SolidColorBrush(Color.FromArgb(alpha, baseColor.R, baseColor.G, baseColor.B));
        }

        private Color GetStreamColor(DataStream stream)
        {
            return DataDirection switch
            {
                MarketDataDirection.Up => Color.FromRgb(0, 255, 128), // Bright Green
                MarketDataDirection.Down => Color.FromRgb(255, 64, 64), // Bright Red
                MarketDataDirection.Mixed => stream.Id % 2 == 0 
                    ? Color.FromRgb(0, 255, 128)
                    : Color.FromRgb(255, 64, 64),
                _ => Color.FromRgb(64, 224, 255) // EVE Blue
            };
        }

        private Brush GetAmbientParticleColor()
        {
            var colors = new[]
            {
                Color.FromArgb(100, 64, 224, 255),  // EVE Blue
                Color.FromArgb(80, 255, 215, 0),    // Gold
                Color.FromArgb(90, 138, 43, 226),   // Blue Violet
                Color.FromArgb(70, 0, 206, 209)     // Dark Turquoise
            };

            return new SolidColorBrush(colors[_random.Next(colors.Length)]);
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
                {
                    if (particle.ParticleType == ParticleType.Stream && particle.StreamId >= 0 && particle.StreamId < _dataStreams.Count)
                    {
                        ellipse.Fill = GetStreamParticleColor(_dataStreams[particle.StreamId]);
                    }
                    else
                    {
                        ellipse.Fill = GetParticleColor();
                    }
                }
            }
        }

        private void UpdateStreamIntensity()
        {
            foreach (var stream in _dataStreams)
            {
                stream.Intensity = DataIntensity;
                stream.ParticleSpawnRate = stream.ParticleSpawnRate * DataIntensity;
            }
        }

        private void ClearStreams()
        {
            _dataStreams.Clear();
        }

        private void OnAnimationTick(object sender, EventArgs e)
        {
            _performanceTracker.StartFrame();
            var deltaTime = 0.016; // 16ms

            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var particle = _particles[i];
                
                // Update position
                particle.X += particle.VelocityX * deltaTime;
                particle.Y += particle.VelocityY * deltaTime;

                // Update life
                particle.Life -= deltaTime / particle.MaxLife;

                // Handle boundaries based on particle type
                HandleParticleBoundaries(particle);

                // Update visual position and opacity
                Canvas.SetLeft(particle.Visual, particle.X);
                Canvas.SetTop(particle.Visual, particle.Y);
                
                // Update opacity with pulsing effect for stream particles
                var opacity = particle.ParticleType == ParticleType.Stream 
                    ? particle.Life * (0.6 + 0.3 * Math.Sin(DateTime.Now.Millisecond * 0.01 + particle.StreamId))
                    : particle.Life * 0.5;
                particle.Visual.Opacity = Math.Max(0, Math.Min(1, opacity));

                // Update size for breathing effect
                if (particle.ParticleType == ParticleType.Stream && particle.Visual is Ellipse ellipse)
                {
                    var sizeMultiplier = 1 + 0.2 * Math.Sin(DateTime.Now.Millisecond * 0.008 + particle.StreamId * 2);
                    ellipse.Width = particle.Size * sizeMultiplier;
                    ellipse.Height = particle.Size * sizeMultiplier;
                }

                // Respawn particle if dead
                if (particle.Life <= 0)
                {
                    RespawnParticle(particle);
                }
            }

            _performanceTracker.EndFrame();
            
            // Adjust particle count based on performance
            if (_performanceTracker.ShouldReduceParticles())
            {
                ReduceParticleCount();
            }
        }

        private void HandleParticleBoundaries(MarketParticle particle)
        {
            if (particle.ParticleType == ParticleType.Stream)
            {
                // Stream particles disappear when they reach boundaries
                if (particle.X < -10 || particle.X > ActualWidth + 10 || 
                    particle.Y < -10 || particle.Y > ActualHeight + 10)
                {
                    particle.Life = 0; // Force respawn
                }
            }
            else
            {
                // Ambient particles wrap around
                if (particle.X < 0) particle.X = ActualWidth;
                if (particle.X > ActualWidth) particle.X = 0;
                if (particle.Y < 0) particle.Y = ActualHeight;
                if (particle.Y > ActualHeight) particle.Y = 0;
            }
        }

        private void ReduceParticleCount()
        {
            if (_particles.Count > 20)
            {
                var particlesToRemove = _particles.Take(_particles.Count / 10).ToList();
                foreach (var particle in particlesToRemove)
                {
                    Children.Remove(particle.Visual);
                    _particles.Remove(particle);
                }
            }
        }

        private void RespawnParticle(MarketParticle particle)
        {
            if (particle.ParticleType == ParticleType.Stream && particle.StreamId >= 0 && particle.StreamId < _dataStreams.Count)
            {
                var stream = _dataStreams[particle.StreamId];
                var startPoint = GetStreamStartPoint(stream);
                var velocity = GetStreamVelocity(stream);

                particle.X = startPoint.X;
                particle.Y = startPoint.Y;
                particle.VelocityX = velocity.X * AnimationSpeed;
                particle.VelocityY = velocity.Y * AnimationSpeed;
                particle.Life = 1.0;
                particle.MaxLife = 3 + _random.NextDouble() * 4;

                if (particle.Visual is Ellipse ellipse)
                {
                    var size = 1.5 + _random.NextDouble() * 2.5 * stream.Intensity;
                    ellipse.Width = size;
                    ellipse.Height = size;
                    ellipse.Fill = GetStreamParticleColor(stream);
                    particle.Size = size;
                }
            }
            else
            {
                // Respawn as ambient particle
                particle.X = _random.NextDouble() * ActualWidth;
                particle.Y = _random.NextDouble() * ActualHeight;
                particle.VelocityX = (_random.NextDouble() - 0.5) * 20 * AnimationSpeed;
                particle.VelocityY = (_random.NextDouble() - 0.5) * 20 * AnimationSpeed;
                particle.Life = 1.0;
                particle.MaxLife = _random.NextDouble() * 8 + 4;
                particle.ParticleType = ParticleType.Ambient;
                particle.StreamId = -1;

                if (particle.Visual is Ellipse ellipse)
                {
                    var size = 0.8 + _random.NextDouble() * 1.5;
                    ellipse.Width = size;
                    ellipse.Height = size;
                    ellipse.Fill = GetAmbientParticleColor();
                    particle.Size = size;
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
        public int StreamId { get; set; } = -1;
        public ParticleType ParticleType { get; set; } = ParticleType.Ambient;
        public double Size { get; set; } = 1.0;
    }

    public class DataStream
    {
        public int Id { get; set; }
        public double StartX { get; set; }
        public double StartY { get; set; }
        public double EndX { get; set; }
        public double EndY { get; set; }
        public Vector FlowDirection { get; set; }
        public double StreamWidth { get; set; }
        public double Intensity { get; set; }
        public double ParticleSpawnRate { get; set; }
    }

    public class PerformanceTracker
    {
        private readonly Queue<double> _frameTimes = new();
        private DateTime _frameStart;
        private const int MaxFrameHistory = 60;

        public void StartFrame()
        {
            _frameStart = DateTime.Now;
        }

        public void EndFrame()
        {
            var frameTime = (DateTime.Now - _frameStart).TotalMilliseconds;
            _frameTimes.Enqueue(frameTime);
            
            if (_frameTimes.Count > MaxFrameHistory)
                _frameTimes.Dequeue();
        }

        public bool ShouldReduceParticles()
        {
            if (_frameTimes.Count < 30) return false;
            
            var avgFrameTime = _frameTimes.Average();
            return avgFrameTime > 20; // Target 50 FPS, reduce if slower
        }
    }

    public enum MarketDataDirection
    {
        Up,
        Down,
        Mixed,
        Neutral
    }

    public enum StreamFlowType
    {
        Vertical,
        Horizontal,
        Diagonal,
        Flowing
    }

    public enum ParticleType
    {
        Stream,
        Ambient
    }
}