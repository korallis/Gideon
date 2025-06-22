using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class DataStreamAnimationControl : Canvas
    {
        private readonly List<DataStreamElement> _streamElements = new();
        private readonly DispatcherTimer _animationTimer;
        private readonly DispatcherTimer _spawnTimer;
        private readonly Random _random = new();

        public static readonly DependencyProperty StreamSpeedProperty =
            DependencyProperty.Register(nameof(StreamSpeed), typeof(double), typeof(DataStreamAnimationControl),
                new PropertyMetadata(1.0));

        public static readonly DependencyProperty StreamDensityProperty =
            DependencyProperty.Register(nameof(StreamDensity), typeof(double), typeof(DataStreamAnimationControl),
                new PropertyMetadata(1.0));

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(DataStreamAnimationControl),
                new PropertyMetadata(true, OnIsActiveChanged));

        public static readonly DependencyProperty StreamDirectionProperty =
            DependencyProperty.Register(nameof(StreamDirection), typeof(DataStreamDirection), typeof(DataStreamAnimationControl),
                new PropertyMetadata(DataStreamDirection.LeftToRight));

        public static readonly DependencyProperty DataTypeProperty =
            DependencyProperty.Register(nameof(DataType), typeof(DataStreamType), typeof(DataStreamAnimationControl),
                new PropertyMetadata(DataStreamType.MarketData, OnDataTypeChanged));

        public static readonly DependencyProperty IntensityProperty =
            DependencyProperty.Register(nameof(Intensity), typeof(double), typeof(DataStreamAnimationControl),
                new PropertyMetadata(1.0));

        public double StreamSpeed
        {
            get => (double)GetValue(StreamSpeedProperty);
            set => SetValue(StreamSpeedProperty, value);
        }

        public double StreamDensity
        {
            get => (double)GetValue(StreamDensityProperty);
            set => SetValue(StreamDensityProperty, value);
        }

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public DataStreamDirection StreamDirection
        {
            get => (DataStreamDirection)GetValue(StreamDirectionProperty);
            set => SetValue(StreamDirectionProperty, value);
        }

        public DataStreamType DataType
        {
            get => (DataStreamType)GetValue(DataTypeProperty);
            set => SetValue(DataTypeProperty, value);
        }

        public double Intensity
        {
            get => (double)GetValue(IntensityProperty);
            set => SetValue(IntensityProperty, value);
        }

        public DataStreamAnimationControl()
        {
            Background = Brushes.Transparent;
            ClipToBounds = true;

            _animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // 60 FPS
            };
            _animationTimer.Tick += OnAnimationTick;

            _spawnTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100) // Spawn new elements
            };
            _spawnTimer.Tick += OnSpawnTick;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (IsActive)
            {
                _animationTimer.Start();
                _spawnTimer.Start();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _animationTimer.Stop();
            _spawnTimer.Stop();
            ClearStreamElements();
        }

        private void OnSpawnTick(object sender, EventArgs e)
        {
            if (!IsActive || ActualWidth <= 0 || ActualHeight <= 0)
                return;

            // Spawn rate based on density
            var spawnChance = StreamDensity * 0.3;
            if (_random.NextDouble() < spawnChance)
            {
                SpawnDataElement();
            }
        }

        private void SpawnDataElement()
        {
            var element = CreateDataElement();
            _streamElements.Add(element);
            Children.Add(element.Visual);
        }

        private DataStreamElement CreateDataElement()
        {
            var element = new DataStreamElement
            {
                Visual = CreateElementVisual(),
                Speed = (50 + _random.NextDouble() * 100) * StreamSpeed,
                Life = 1.0,
                MaxLife = 3.0 + _random.NextDouble() * 2
            };

            // Set initial position based on direction
            switch (StreamDirection)
            {
                case DataStreamDirection.LeftToRight:
                    element.X = -20;
                    element.Y = _random.NextDouble() * ActualHeight;
                    element.VelocityX = element.Speed;
                    element.VelocityY = (_random.NextDouble() - 0.5) * 20;
                    break;

                case DataStreamDirection.RightToLeft:
                    element.X = ActualWidth + 20;
                    element.Y = _random.NextDouble() * ActualHeight;
                    element.VelocityX = -element.Speed;
                    element.VelocityY = (_random.NextDouble() - 0.5) * 20;
                    break;

                case DataStreamDirection.TopToBottom:
                    element.X = _random.NextDouble() * ActualWidth;
                    element.Y = -20;
                    element.VelocityX = (_random.NextDouble() - 0.5) * 20;
                    element.VelocityY = element.Speed;
                    break;

                case DataStreamDirection.BottomToTop:
                    element.X = _random.NextDouble() * ActualWidth;
                    element.Y = ActualHeight + 20;
                    element.VelocityX = (_random.NextDouble() - 0.5) * 20;
                    element.VelocityY = -element.Speed;
                    break;

                case DataStreamDirection.Radial:
                    var centerX = ActualWidth / 2;
                    var centerY = ActualHeight / 2;
                    var angle = _random.NextDouble() * Math.PI * 2;
                    var distance = 50 + _random.NextDouble() * 100;
                    
                    element.X = centerX;
                    element.Y = centerY;
                    element.VelocityX = Math.Cos(angle) * element.Speed;
                    element.VelocityY = Math.Sin(angle) * element.Speed;
                    break;
            }

            Canvas.SetLeft(element.Visual, element.X);
            Canvas.SetTop(element.Visual, element.Y);

            return element;
        }

        private FrameworkElement CreateElementVisual()
        {
            return DataType switch
            {
                DataStreamType.MarketData => CreateMarketDataElement(),
                DataStreamType.CharacterData => CreateCharacterDataElement(),
                DataStreamType.ShipData => CreateShipDataElement(),
                DataStreamType.NetworkTraffic => CreateNetworkElement(),
                DataStreamType.SystemStatus => CreateSystemStatusElement(),
                _ => CreateGenericDataElement()
            };
        }

        private FrameworkElement CreateMarketDataElement()
        {
            var container = new Canvas { Width = 80, Height = 20 };

            // Data bar
            var dataBar = new Rectangle
            {
                Width = 60,
                Height = 4,
                Fill = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 0),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Colors.Transparent, 0),
                        new GradientStop(Color.FromRgb(50, 205, 50), 0.3),
                        new GradientStop(Color.FromRgb(50, 205, 50), 0.7),
                        new GradientStop(Colors.Transparent, 1)
                    }
                }
            };
            Canvas.SetLeft(dataBar, 10);
            Canvas.SetTop(dataBar, 8);
            container.Children.Add(dataBar);

            // Price indicator
            var price = new TextBlock
            {
                Text = $"{_random.Next(1000, 9999)}",
                Foreground = new SolidColorBrush(Color.FromArgb(200, 50, 205, 50)),
                FontSize = 8,
                FontFamily = new FontFamily("Consolas")
            };
            Canvas.SetLeft(price, 10);
            Canvas.SetTop(price, 0);
            container.Children.Add(price);

            AddGlowEffect(container, Color.FromRgb(50, 205, 50));
            return container;
        }

        private FrameworkElement CreateCharacterDataElement()
        {
            var container = new Canvas { Width = 100, Height = 20 };

            // Character icon
            var icon = new Ellipse
            {
                Width = 12,
                Height = 12,
                Fill = new SolidColorBrush(Color.FromArgb(200, 64, 224, 255)),
                Stroke = new SolidColorBrush(Color.FromArgb(255, 64, 224, 255)),
                StrokeThickness = 1
            };
            Canvas.SetLeft(icon, 5);
            Canvas.SetTop(icon, 4);
            container.Children.Add(icon);

            // Character name
            var name = new TextBlock
            {
                Text = "PILOT_" + _random.Next(100, 999),
                Foreground = new SolidColorBrush(Color.FromArgb(200, 64, 224, 255)),
                FontSize = 8,
                FontFamily = new FontFamily("Consolas")
            };
            Canvas.SetLeft(name, 20);
            Canvas.SetTop(name, 6);
            container.Children.Add(name);

            AddGlowEffect(container, Color.FromRgb(64, 224, 255));
            return container;
        }

        private FrameworkElement CreateShipDataElement()
        {
            var container = new Canvas { Width = 120, Height = 20 };

            // Ship silhouette
            var ship = new Polygon
            {
                Points = new PointCollection
                {
                    new Point(5, 10), new Point(15, 5), new Point(25, 8),
                    new Point(25, 12), new Point(15, 15)
                },
                Fill = new SolidColorBrush(Color.FromArgb(200, 255, 215, 0)),
                Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 215, 0)),
                StrokeThickness = 1
            };
            container.Children.Add(ship);

            // Ship name
            var shipName = new TextBlock
            {
                Text = GetRandomShipName(),
                Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 215, 0)),
                FontSize = 8,
                FontFamily = new FontFamily("Consolas")
            };
            Canvas.SetLeft(shipName, 30);
            Canvas.SetTop(shipName, 6);
            container.Children.Add(shipName);

            AddGlowEffect(container, Color.FromRgb(255, 215, 0));
            return container;
        }

        private FrameworkElement CreateNetworkElement()
        {
            var container = new Canvas { Width = 60, Height = 20 };

            // Network packets
            for (int i = 0; i < 5; i++)
            {
                var packet = new Rectangle
                {
                    Width = 8,
                    Height = 3,
                    Fill = new SolidColorBrush(Color.FromArgb(150, 138, 43, 226))
                };
                Canvas.SetLeft(packet, i * 10 + 5);
                Canvas.SetTop(packet, 8 + (_random.NextDouble() - 0.5) * 6);
                container.Children.Add(packet);
            }

            AddGlowEffect(container, Color.FromRgb(138, 43, 226));
            return container;
        }

        private FrameworkElement CreateSystemStatusElement()
        {
            var container = new Canvas { Width = 80, Height = 20 };

            // Status indicator
            var status = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(Color.FromArgb(200, 50, 205, 50))
            };
            Canvas.SetLeft(status, 5);
            Canvas.SetTop(status, 6);
            container.Children.Add(status);

            // Status text
            var statusText = new TextBlock
            {
                Text = "SYS_OK",
                Foreground = new SolidColorBrush(Color.FromArgb(200, 50, 205, 50)),
                FontSize = 8,
                FontFamily = new FontFamily("Consolas")
            };
            Canvas.SetLeft(statusText, 18);
            Canvas.SetTop(statusText, 6);
            container.Children.Add(statusText);

            AddGlowEffect(container, Color.FromRgb(50, 205, 50));
            return container;
        }

        private FrameworkElement CreateGenericDataElement()
        {
            var rect = new Rectangle
            {
                Width = 40,
                Height = 8,
                Fill = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 0),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Colors.Transparent, 0),
                        new GradientStop(Color.FromRgb(64, 224, 255), 0.5),
                        new GradientStop(Colors.Transparent, 1)
                    }
                }
            };

            AddGlowEffect(rect, Color.FromRgb(64, 224, 255));
            return rect;
        }

        private void AddGlowEffect(FrameworkElement element, Color glowColor)
        {
            element.Effect = new DropShadowEffect
            {
                Color = glowColor,
                BlurRadius = 8,
                ShadowDepth = 0,
                Opacity = Intensity * 0.6
            };
        }

        private string GetRandomShipName()
        {
            var names = new[] { "RIFTER", "DRAKE", "RAVEN", "APOCALYPSE", "DOMINIX", "MEGATHRON" };
            return names[_random.Next(names.Length)];
        }

        private void OnAnimationTick(object sender, EventArgs e)
        {
            var deltaTime = 0.016; // 16ms

            for (int i = _streamElements.Count - 1; i >= 0; i--)
            {
                var element = _streamElements[i];

                // Update position
                element.X += element.VelocityX * deltaTime;
                element.Y += element.VelocityY * deltaTime;

                // Update life
                element.Life -= deltaTime / element.MaxLife;

                // Update visual
                Canvas.SetLeft(element.Visual, element.X);
                Canvas.SetTop(element.Visual, element.Y);
                element.Visual.Opacity = Math.Max(0, element.Life);

                // Remove if dead or out of bounds
                if (element.Life <= 0 || IsOutOfBounds(element))
                {
                    Children.Remove(element.Visual);
                    _streamElements.RemoveAt(i);
                }
            }
        }

        private bool IsOutOfBounds(DataStreamElement element)
        {
            return element.X < -100 || element.X > ActualWidth + 100 ||
                   element.Y < -100 || element.Y > ActualHeight + 100;
        }

        private void ClearStreamElements()
        {
            foreach (var element in _streamElements)
                Children.Remove(element.Visual);
            _streamElements.Clear();
        }

        private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataStreamAnimationControl control)
            {
                if ((bool)e.NewValue)
                {
                    control._animationTimer.Start();
                    control._spawnTimer.Start();
                }
                else
                {
                    control._animationTimer.Stop();
                    control._spawnTimer.Stop();
                }
            }
        }

        private static void OnDataTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataStreamAnimationControl control)
                control.ClearStreamElements();
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

    public class DataStreamElement
    {
        public FrameworkElement Visual { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public double Speed { get; set; }
        public double Life { get; set; }
        public double MaxLife { get; set; }
    }

    public enum DataStreamDirection
    {
        LeftToRight,
        RightToLeft,
        TopToBottom,
        BottomToTop,
        Radial
    }

    public enum DataStreamType
    {
        MarketData,
        CharacterData,
        ShipData,
        NetworkTraffic,
        SystemStatus
    }
}