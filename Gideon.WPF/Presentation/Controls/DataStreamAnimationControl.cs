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

        public static readonly DependencyProperty RealTimeDataProperty =
            DependencyProperty.Register(nameof(RealTimeData), typeof(ObservableCollection<StreamDataPoint>), typeof(DataStreamAnimationControl),
                new PropertyMetadata(null, OnRealTimeDataChanged));

        public static readonly DependencyProperty StreamTemplateProperty =
            DependencyProperty.Register(nameof(StreamTemplate), typeof(DataTemplate), typeof(DataStreamAnimationControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty MaxElementsProperty =
            DependencyProperty.Register(nameof(MaxElements), typeof(int), typeof(DataStreamAnimationControl),
                new PropertyMetadata(50, OnMaxElementsChanged));

        public static readonly DependencyProperty HolographicModeProperty =
            DependencyProperty.Register(nameof(HolographicMode), typeof(bool), typeof(DataStreamAnimationControl),
                new PropertyMetadata(true, OnHolographicModeChanged));

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

        public ObservableCollection<StreamDataPoint> RealTimeData
        {
            get => (ObservableCollection<StreamDataPoint>)GetValue(RealTimeDataProperty);
            set => SetValue(RealTimeDataProperty, value);
        }

        public DataTemplate StreamTemplate
        {
            get => (DataTemplate)GetValue(StreamTemplateProperty);
            set => SetValue(StreamTemplateProperty, value);
        }

        public int MaxElements
        {
            get => (int)GetValue(MaxElementsProperty);
            set => SetValue(MaxElementsProperty, value);
        }

        public bool HolographicMode
        {
            get => (bool)GetValue(HolographicModeProperty);
            set => SetValue(HolographicModeProperty, value);
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

            // Clean up excess elements
            if (_streamElements.Count >= MaxElements)
            {
                CleanupOldestElements();
            }

            // Spawn from real-time data if available
            if (RealTimeData != null && RealTimeData.Any())
            {
                SpawnRealTimeDataElement();
            }
            else
            {
                // Fallback to procedural generation
                var spawnChance = StreamDensity * 0.3;
                if (_random.NextDouble() < spawnChance)
                {
                    SpawnDataElement();
                }
            }
        }

        private void SpawnRealTimeDataElement()
        {
            var recentData = RealTimeData
                .Where(d => DateTime.Now - d.Timestamp < TimeSpan.FromSeconds(5))
                .OrderByDescending(d => d.Timestamp)
                .Take(5)
                .ToList();

            if (recentData.Any())
            {
                var dataPoint = recentData[_random.Next(recentData.Count)];
                var element = CreateRealTimeElement(dataPoint);
                _streamElements.Add(element);
                Children.Add(element.Visual);
            }
        }

        private DataStreamElement CreateRealTimeElement(StreamDataPoint dataPoint)
        {
            var element = new DataStreamElement
            {
                Visual = CreateRealTimeVisual(dataPoint),
                Speed = (50 + dataPoint.Priority * 50) * StreamSpeed,
                Life = 1.0,
                MaxLife = 2.0 + dataPoint.Priority,
                DataPoint = dataPoint
            };

            SetElementPosition(element);
            ApplyHolographicEffects(element);

            Canvas.SetLeft(element.Visual, element.X);
            Canvas.SetTop(element.Visual, element.Y);

            return element;
        }

        private FrameworkElement CreateRealTimeVisual(StreamDataPoint dataPoint)
        {
            if (StreamTemplate != null)
            {
                var content = StreamTemplate.LoadContent() as FrameworkElement;
                if (content != null)
                {
                    content.DataContext = dataPoint;
                    return content;
                }
            }

            return CreateDataBasedVisual(dataPoint);
        }

        private FrameworkElement CreateDataBasedVisual(StreamDataPoint dataPoint)
        {
            var container = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)),
                BorderBrush = GetDataTypeColor(dataPoint.Type),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(2),
                Padding = new Thickness(5, 2, 5, 2)
            };

            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };

            // Priority indicator
            var priorityIcon = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = GetPriorityColor(dataPoint.Priority),
                Margin = new Thickness(0, 0, 5, 0)
            };
            stackPanel.Children.Add(priorityIcon);

            // Data content
            var content = new TextBlock
            {
                Text = dataPoint.Content,
                Foreground = GetDataTypeColor(dataPoint.Type),
                FontSize = 8,
                FontFamily = new FontFamily("Consolas"),
                MaxWidth = 80,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            stackPanel.Children.Add(content);

            // Timestamp
            var timestamp = new TextBlock
            {
                Text = dataPoint.Timestamp.ToString("HH:mm:ss"),
                Foreground = new SolidColorBrush(Color.FromArgb(150, 128, 128, 128)),
                FontSize = 6,
                FontFamily = new FontFamily("Consolas"),
                Margin = new Thickness(5, 0, 0, 0)
            };
            stackPanel.Children.Add(timestamp);

            container.Child = stackPanel;
            return container;
        }

        private void CleanupOldestElements()
        {
            var elementsToRemove = _streamElements
                .OrderBy(e => e.Life)
                .Take(_streamElements.Count - MaxElements + 5)
                .ToList();

            foreach (var element in elementsToRemove)
            {
                Children.Remove(element.Visual);
                _streamElements.Remove(element);
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

            SetElementPosition(element);
            ApplyHolographicEffects(element);

            Canvas.SetLeft(element.Visual, element.X);
            Canvas.SetTop(element.Visual, element.Y);

            return element;
        }

        private void SetElementPosition(DataStreamElement element)
        {
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
                    
                    element.X = centerX;
                    element.Y = centerY;
                    element.VelocityX = Math.Cos(angle) * element.Speed;
                    element.VelocityY = Math.Sin(angle) * element.Speed;
                    break;
            }
        }

        private void ApplyHolographicEffects(DataStreamElement element)
        {
            if (!HolographicMode) return;

            // Apply scanline effect
            var scanlineTransform = new TransformGroup();
            scanlineTransform.Children.Add(new ScaleTransform(1, 0.95 + _random.NextDouble() * 0.1));
            scanlineTransform.Children.Add(new SkewTransform(_random.NextDouble() * 2 - 1, 0));
            element.Visual.RenderTransform = scanlineTransform;

            // Apply holographic flicker
            var flickerAnimation = new DoubleAnimation
            {
                From = 0.7,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(100 + _random.Next(200)),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            element.Visual.BeginAnimation(UIElement.OpacityProperty, flickerAnimation);
        }

        private SolidColorBrush GetDataTypeColor(DataStreamType type)
        {
            return type switch
            {
                DataStreamType.MarketData => new SolidColorBrush(Color.FromArgb(200, 50, 205, 50)),
                DataStreamType.CharacterData => new SolidColorBrush(Color.FromArgb(200, 64, 224, 255)),
                DataStreamType.ShipData => new SolidColorBrush(Color.FromArgb(200, 255, 215, 0)),
                DataStreamType.NetworkTraffic => new SolidColorBrush(Color.FromArgb(200, 138, 43, 226)),
                DataStreamType.SystemStatus => new SolidColorBrush(Color.FromArgb(200, 50, 205, 50)),
                DataStreamType.Combat => new SolidColorBrush(Color.FromArgb(200, 255, 64, 64)),
                DataStreamType.Navigation => new SolidColorBrush(Color.FromArgb(200, 0, 255, 0)),
                _ => new SolidColorBrush(Color.FromArgb(200, 128, 128, 128))
            };
        }

        private SolidColorBrush GetPriorityColor(double priority)
        {
            if (priority >= 0.8) return new SolidColorBrush(Color.FromArgb(200, 255, 64, 64)); // High - Red
            if (priority >= 0.5) return new SolidColorBrush(Color.FromArgb(200, 255, 215, 0)); // Medium - Yellow
            return new SolidColorBrush(Color.FromArgb(200, 50, 205, 50)); // Low - Green
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

                // Update position with flow dynamics
                UpdateElementPosition(element, deltaTime);

                // Update life and appearance
                UpdateElementLife(element, deltaTime);

                // Apply real-time effects
                if (HolographicMode)
                {
                    ApplyRealtimeHolographicEffects(element, deltaTime);
                }

                // Update visual position
                Canvas.SetLeft(element.Visual, element.X);
                Canvas.SetTop(element.Visual, element.Y);

                // Remove if dead or out of bounds
                if (element.Life <= 0 || IsOutOfBounds(element))
                {
                    Children.Remove(element.Visual);
                    _streamElements.RemoveAt(i);
                }
            }
        }

        private void UpdateElementPosition(DataStreamElement element, double deltaTime)
        {
            // Apply stream-specific physics
            switch (StreamDirection)
            {
                case DataStreamDirection.Radial:
                    // Accelerate outward
                    element.VelocityX *= 1.02;
                    element.VelocityY *= 1.02;
                    break;

                case DataStreamDirection.LeftToRight:
                case DataStreamDirection.RightToLeft:
                    // Slight vertical drift for organic feel
                    element.VelocityY += Math.Sin(DateTime.Now.Millisecond * 0.01 + element.X * 0.01) * 5 * deltaTime;
                    break;

                case DataStreamDirection.TopToBottom:
                case DataStreamDirection.BottomToTop:
                    // Slight horizontal drift
                    element.VelocityX += Math.Cos(DateTime.Now.Millisecond * 0.01 + element.Y * 0.01) * 5 * deltaTime;
                    break;
            }

            // Update position
            element.X += element.VelocityX * deltaTime;
            element.Y += element.VelocityY * deltaTime;
        }

        private void UpdateElementLife(DataStreamElement element, double deltaTime)
        {
            element.Life -= deltaTime / element.MaxLife;

            // Fade based on life and data importance
            var baseOpacity = Math.Max(0, element.Life);
            if (element.DataPoint != null)
            {
                // Important data fades slower
                baseOpacity = Math.Max(0.2, baseOpacity + element.DataPoint.Priority * 0.3);
            }

            element.Visual.Opacity = baseOpacity * Intensity;
        }

        private void ApplyRealtimeHolographicEffects(DataStreamElement element, double deltaTime)
        {
            // Scanline distortion
            if (element.Visual.RenderTransform is TransformGroup group)
            {
                foreach (var transform in group.Children)
                {
                    if (transform is SkewTransform skew)
                    {
                        skew.AngleX += Math.Sin(DateTime.Now.Millisecond * 0.02) * 0.5;
                    }
                }
            }

            // Data corruption effect for old elements
            if (element.Life < 0.3 && _random.NextDouble() < 0.1)
            {
                ApplyCorruptionEffect(element);
            }
        }

        private void ApplyCorruptionEffect(DataStreamElement element)
        {
            if (element.Visual is Border border && border.Child is StackPanel panel)
            {
                foreach (var child in panel.Children.OfType<TextBlock>())
                {
                    var text = child.Text;
                    if (!string.IsNullOrEmpty(text) && _random.NextDouble() < 0.3)
                    {
                        var chars = text.ToCharArray();
                        for (int i = 0; i < chars.Length; i++)
                        {
                            if (_random.NextDouble() < 0.2)
                            {
                                chars[i] = (char)('A' + _random.Next(26));
                            }
                        }
                        child.Text = new string(chars);
                    }
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

        private static void OnRealTimeDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataStreamAnimationControl control)
            {
                if (e.OldValue is ObservableCollection<StreamDataPoint> oldCollection)
                {
                    oldCollection.CollectionChanged -= control.OnRealTimeDataCollectionChanged;
                }
                if (e.NewValue is ObservableCollection<StreamDataPoint> newCollection)
                {
                    newCollection.CollectionChanged += control.OnRealTimeDataCollectionChanged;
                }
            }
        }

        private static void OnMaxElementsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataStreamAnimationControl control)
                control.CleanupOldestElements();
        }

        private static void OnHolographicModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataStreamAnimationControl control)
                control.RefreshHolographicEffects();
        }

        private void OnRealTimeDataCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // React to new data being added to the collection
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (StreamDataPoint newItem in e.NewItems)
                {
                    // Immediately create a stream element for new high-priority data
                    if (newItem.Priority > 0.7)
                    {
                        var element = CreateRealTimeElement(newItem);
                        _streamElements.Add(element);
                        Children.Add(element.Visual);
                    }
                }
            }
        }

        private void RefreshHolographicEffects()
        {
            foreach (var element in _streamElements)
            {
                if (HolographicMode)
                {
                    ApplyHolographicEffects(element);
                }
                else
                {
                    // Remove holographic effects
                    element.Visual.RenderTransform = null;
                    element.Visual.BeginAnimation(UIElement.OpacityProperty, null);
                }
            }
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
        public StreamDataPoint DataPoint { get; set; }
    }

    public class StreamDataPoint
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Content { get; set; } = string.Empty;
        public DataStreamType Type { get; set; }
        public double Priority { get; set; } = 0.5; // 0.0 to 1.0
        public string Source { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new();
        public bool IsEncrypted { get; set; }
        public int Size { get; set; } // Data size in bytes
        public string Destination { get; set; } = string.Empty;
        public Color Color { get; set; } = Colors.Transparent;
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
        SystemStatus,
        Combat,
        Navigation,
        Communications,
        Sensors,
        Intelligence
    }
}