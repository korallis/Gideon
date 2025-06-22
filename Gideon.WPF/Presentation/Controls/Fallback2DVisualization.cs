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
    /// <summary>
    /// Fallback 2D visualization system for low-end systems that cannot handle 3D/particle effects
    /// </summary>
    public class Fallback2DVisualization : Canvas
    {
        private readonly List<Fallback2DElement> _elements = new();
        private readonly DispatcherTimer _animationTimer;
        private readonly Random _random = new();

        public static readonly DependencyProperty VisualizationTypeProperty =
            DependencyProperty.Register(nameof(VisualizationType), typeof(Fallback2DType), typeof(Fallback2DVisualization),
                new PropertyMetadata(Fallback2DType.SimpleGraph, OnVisualizationTypeChanged));

        public static readonly DependencyProperty DataPointsProperty =
            DependencyProperty.Register(nameof(DataPoints), typeof(IEnumerable<Point>), typeof(Fallback2DVisualization),
                new PropertyMetadata(null, OnDataPointsChanged));

        public static readonly DependencyProperty IsAnimatedProperty =
            DependencyProperty.Register(nameof(IsAnimated), typeof(bool), typeof(Fallback2DVisualization),
                new PropertyMetadata(true, OnIsAnimatedChanged));

        public static readonly DependencyProperty ColorSchemeProperty =
            DependencyProperty.Register(nameof(ColorScheme), typeof(SimpleColorScheme), typeof(Fallback2DVisualization),
                new PropertyMetadata(SimpleColorScheme.Blue, OnColorSchemeChanged));

        public static readonly DependencyProperty ShowLabelsProperty =
            DependencyProperty.Register(nameof(ShowLabels), typeof(bool), typeof(Fallback2DVisualization),
                new PropertyMetadata(true));

        public static readonly DependencyProperty LineThicknessProperty =
            DependencyProperty.Register(nameof(LineThickness), typeof(double), typeof(Fallback2DVisualization),
                new PropertyMetadata(2.0));

        public Fallback2DType VisualizationType
        {
            get => (Fallback2DType)GetValue(VisualizationTypeProperty);
            set => SetValue(VisualizationTypeProperty, value);
        }

        public IEnumerable<Point> DataPoints
        {
            get => (IEnumerable<Point>)GetValue(DataPointsProperty);
            set => SetValue(DataPointsProperty, value);
        }

        public bool IsAnimated
        {
            get => (bool)GetValue(IsAnimatedProperty);
            set => SetValue(IsAnimatedProperty, value);
        }

        public SimpleColorScheme ColorScheme
        {
            get => (SimpleColorScheme)GetValue(ColorSchemeProperty);
            set => SetValue(ColorSchemeProperty, value);
        }

        public bool ShowLabels
        {
            get => (bool)GetValue(ShowLabelsProperty);
            set => SetValue(ShowLabelsProperty, value);
        }

        public double LineThickness
        {
            get => (double)GetValue(LineThicknessProperty);
            set => SetValue(LineThicknessProperty, value);
        }

        public Fallback2DVisualization()
        {
            Background = Brushes.Transparent;
            ClipToBounds = true;

            _animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50) // 20 FPS for smooth but efficient animation
            };
            _animationTimer.Tick += OnAnimationTick;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateVisualization();
            if (IsAnimated)
                _animationTimer.Start();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _animationTimer.Stop();
            ClearElements();
        }

        private static void OnVisualizationTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Fallback2DVisualization viz)
                viz.UpdateVisualization();
        }

        private static void OnDataPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Fallback2DVisualization viz)
                viz.UpdateVisualization();
        }

        private static void OnIsAnimatedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Fallback2DVisualization viz)
            {
                if ((bool)e.NewValue)
                    viz._animationTimer.Start();
                else
                    viz._animationTimer.Stop();
            }
        }

        private static void OnColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Fallback2DVisualization viz)
                viz.UpdateVisualization();
        }

        private void UpdateVisualization()
        {
            ClearElements();

            if (ActualWidth <= 0 || ActualHeight <= 0 || DataPoints == null)
                return;

            switch (VisualizationType)
            {
                case Fallback2DType.SimpleGraph:
                    CreateSimpleGraph();
                    break;
                case Fallback2DType.BarChart:
                    CreateBarChart();
                    break;
                case Fallback2DType.LineChart:
                    CreateLineChart();
                    break;
                case Fallback2DType.DataFlow:
                    CreateDataFlow();
                    break;
                case Fallback2DType.StatusIndicators:
                    CreateStatusIndicators();
                    break;
            }
        }

        private void CreateSimpleGraph()
        {
            var primaryColor = GetPrimaryColor();
            var points = new List<Point>(DataPoints);
            
            if (points.Count < 2) return;

            // Draw grid
            CreateGrid();

            // Draw line
            var polyline = new Polyline
            {
                Stroke = primaryColor,
                StrokeThickness = LineThickness,
                Points = new PointCollection(points)
            };
            Children.Add(polyline);

            // Add data points
            foreach (var point in points)
            {
                var circle = new Ellipse
                {
                    Width = 6,
                    Height = 6,
                    Fill = primaryColor,
                    Stroke = Brushes.White,
                    StrokeThickness = 1
                };
                
                SetLeft(circle, point.X - 3);
                SetTop(circle, point.Y - 3);
                Children.Add(circle);

                if (IsAnimated)
                {
                    var element = new Fallback2DElement
                    {
                        Visual = circle,
                        X = point.X,
                        Y = point.Y,
                        OriginalX = point.X,
                        OriginalY = point.Y,
                        AnimationPhase = _random.NextDouble() * Math.PI * 2
                    };
                    _elements.Add(element);
                }
            }
        }

        private void CreateBarChart()
        {
            var primaryColor = GetPrimaryColor();
            var secondaryColor = GetSecondaryColor();
            var points = new List<Point>(DataPoints);
            
            if (points.Count == 0) return;

            var barWidth = (ActualWidth - 40) / points.Count - 5;
            var maxValue = 0.0;
            
            foreach (var point in points)
                maxValue = Math.Max(maxValue, point.Y);

            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i];
                var barHeight = (point.Y / maxValue) * (ActualHeight - 60);
                var x = 20 + i * (barWidth + 5);
                var y = ActualHeight - 20 - barHeight;

                var bar = new Rectangle
                {
                    Width = barWidth,
                    Height = barHeight,
                    Fill = new LinearGradientBrush
                    {
                        StartPoint = new Point(0, 0),
                        EndPoint = new Point(0, 1),
                        GradientStops = new GradientStopCollection
                        {
                            new GradientStop(primaryColor.Color, 0),
                            new GradientStop(secondaryColor.Color, 1)
                        }
                    },
                    Stroke = primaryColor,
                    StrokeThickness = 1
                };

                SetLeft(bar, x);
                SetTop(bar, y);
                Children.Add(bar);

                if (ShowLabels)
                {
                    var label = new TextBlock
                    {
                        Text = point.Y.ToString("F1"),
                        FontSize = 10,
                        Foreground = primaryColor,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    
                    SetLeft(label, x + barWidth / 2 - 10);
                    SetTop(label, y - 20);
                    Children.Add(label);
                }

                if (IsAnimated)
                {
                    var element = new Fallback2DElement
                    {
                        Visual = bar,
                        X = x,
                        Y = y,
                        OriginalX = x,
                        OriginalY = y,
                        AnimationPhase = i * 0.2
                    };
                    _elements.Add(element);
                }
            }
        }

        private void CreateLineChart()
        {
            var primaryColor = GetPrimaryColor();
            var points = new List<Point>(DataPoints);
            
            if (points.Count < 2) return;

            CreateGrid();

            // Create smooth path
            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure { StartPoint = points[0] };

            for (int i = 1; i < points.Count; i++)
            {
                pathFigure.Segments.Add(new LineSegment(points[i], true));
            }

            pathGeometry.Figures.Add(pathFigure);

            var path = new Path
            {
                Data = pathGeometry,
                Stroke = primaryColor,
                StrokeThickness = LineThickness,
                Fill = Brushes.Transparent
            };
            Children.Add(path);

            // Add area fill
            var areaFigure = new PathFigure { StartPoint = new Point(points[0].X, ActualHeight) };
            areaFigure.Segments.Add(new LineSegment(points[0], true));
            
            for (int i = 1; i < points.Count; i++)
            {
                areaFigure.Segments.Add(new LineSegment(points[i], true));
            }
            
            areaFigure.Segments.Add(new LineSegment(new Point(points[points.Count - 1].X, ActualHeight), true));
            areaFigure.IsClosed = true;

            var areaGeometry = new PathGeometry();
            areaGeometry.Figures.Add(areaFigure);

            var areaPath = new Path
            {
                Data = areaGeometry,
                Fill = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(0, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb(100, primaryColor.Color.R, primaryColor.Color.G, primaryColor.Color.B), 0),
                        new GradientStop(Colors.Transparent, 1)
                    }
                }
            };
            Children.Add(areaPath);
        }

        private void CreateDataFlow()
        {
            var primaryColor = GetPrimaryColor();
            
            // Create flowing data elements
            for (int i = 0; i < 20; i++)
            {
                var rect = new Rectangle
                {
                    Width = 20,
                    Height = 4,
                    Fill = primaryColor,
                    Opacity = 0.7
                };

                var x = _random.NextDouble() * ActualWidth;
                var y = _random.NextDouble() * ActualHeight;

                SetLeft(rect, x);
                SetTop(rect, y);
                Children.Add(rect);

                if (IsAnimated)
                {
                    var element = new Fallback2DElement
                    {
                        Visual = rect,
                        X = x,
                        Y = y,
                        VelocityX = (_random.NextDouble() - 0.5) * 50,
                        VelocityY = (_random.NextDouble() - 0.5) * 20,
                        Life = 1.0,
                        MaxLife = 3.0 + _random.NextDouble() * 2
                    };
                    _elements.Add(element);
                }
            }
        }

        private void CreateStatusIndicators()
        {
            var primaryColor = GetPrimaryColor();
            var successColor = new SolidColorBrush(Colors.LimeGreen);
            var warningColor = new SolidColorBrush(Colors.Orange);
            var errorColor = new SolidColorBrush(Colors.Red);

            var indicators = new[]
            {
                new { Text = "System Status", Color = successColor },
                new { Text = "Network", Color = primaryColor },
                new { Text = "Data Stream", Color = warningColor },
                new { Text = "Connections", Color = successColor }
            };

            for (int i = 0; i < indicators.Length; i++)
            {
                var indicator = indicators[i];
                var y = 20 + i * 40;

                // Status light
                var light = new Ellipse
                {
                    Width = 12,
                    Height = 12,
                    Fill = indicator.Color
                };
                SetLeft(light, 20);
                SetTop(light, y + 5);
                Children.Add(light);

                // Label
                var label = new TextBlock
                {
                    Text = indicator.Text,
                    FontSize = 14,
                    Foreground = primaryColor,
                    VerticalAlignment = VerticalAlignment.Center
                };
                SetLeft(label, 40);
                SetTop(label, y);
                Children.Add(label);

                // Status bar
                var statusBar = new Rectangle
                {
                    Width = 100,
                    Height = 6,
                    Fill = indicator.Color,
                    Opacity = 0.3
                };
                SetLeft(statusBar, ActualWidth - 130);
                SetTop(statusBar, y + 7);
                Children.Add(statusBar);

                if (IsAnimated)
                {
                    var element = new Fallback2DElement
                    {
                        Visual = light,
                        X = 20,
                        Y = y + 5,
                        AnimationPhase = i * Math.PI / 2
                    };
                    _elements.Add(element);
                }
            }
        }

        private void CreateGrid()
        {
            var gridColor = new SolidColorBrush(Color.FromArgb(50, 128, 128, 128));
            
            // Vertical lines
            for (double x = 0; x <= ActualWidth; x += 40)
            {
                var line = new Line
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = ActualHeight,
                    Stroke = gridColor,
                    StrokeThickness = 1
                };
                Children.Add(line);
            }

            // Horizontal lines
            for (double y = 0; y <= ActualHeight; y += 30)
            {
                var line = new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = ActualWidth,
                    Y2 = y,
                    Stroke = gridColor,
                    StrokeThickness = 1
                };
                Children.Add(line);
            }
        }

        private void OnAnimationTick(object sender, EventArgs e)
        {
            var deltaTime = 0.05; // 50ms

            foreach (var element in _elements)
            {
                switch (VisualizationType)
                {
                    case Fallback2DType.SimpleGraph:
                        // Gentle floating animation
                        element.AnimationPhase += deltaTime * 2;
                        var offsetY = Math.Sin(element.AnimationPhase) * 2;
                        SetTop(element.Visual, element.OriginalY + offsetY);
                        break;

                    case Fallback2DType.DataFlow:
                        // Moving elements
                        element.X += element.VelocityX * deltaTime;
                        element.Y += element.VelocityY * deltaTime;
                        element.Life -= deltaTime / element.MaxLife;

                        // Wrap around
                        if (element.X < -20) element.X = ActualWidth + 20;
                        if (element.X > ActualWidth + 20) element.X = -20;
                        if (element.Y < -10) element.Y = ActualHeight + 10;
                        if (element.Y > ActualHeight + 10) element.Y = -10;

                        SetLeft(element.Visual, element.X);
                        SetTop(element.Visual, element.Y);
                        element.Visual.Opacity = Math.Max(0.3, element.Life);
                        break;

                    case Fallback2DType.StatusIndicators:
                        // Pulsing animation
                        element.AnimationPhase += deltaTime * 3;
                        var pulseOpacity = 0.7 + Math.Sin(element.AnimationPhase) * 0.3;
                        element.Visual.Opacity = pulseOpacity;
                        break;
                }
            }
        }

        private SolidColorBrush GetPrimaryColor()
        {
            return ColorScheme switch
            {
                SimpleColorScheme.Blue => new SolidColorBrush(Color.FromRgb(0, 120, 215)),
                SimpleColorScheme.Green => new SolidColorBrush(Color.FromRgb(0, 150, 80)),
                SimpleColorScheme.Orange => new SolidColorBrush(Color.FromRgb(255, 140, 0)),
                SimpleColorScheme.Red => new SolidColorBrush(Color.FromRgb(200, 30, 30)),
                SimpleColorScheme.Purple => new SolidColorBrush(Color.FromRgb(120, 80, 200)),
                _ => new SolidColorBrush(Color.FromRgb(0, 120, 215))
            };
        }

        private SolidColorBrush GetSecondaryColor()
        {
            return ColorScheme switch
            {
                SimpleColorScheme.Blue => new SolidColorBrush(Color.FromRgb(0, 80, 150)),
                SimpleColorScheme.Green => new SolidColorBrush(Color.FromRgb(0, 100, 50)),
                SimpleColorScheme.Orange => new SolidColorBrush(Color.FromRgb(200, 100, 0)),
                SimpleColorScheme.Red => new SolidColorBrush(Color.FromRgb(150, 20, 20)),
                SimpleColorScheme.Purple => new SolidColorBrush(Color.FromRgb(80, 50, 150)),
                _ => new SolidColorBrush(Color.FromRgb(0, 80, 150))
            };
        }

        private void ClearElements()
        {
            Children.Clear();
            _elements.Clear();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            return constraint;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (_elements.Count == 0 && DataPoints != null)
                UpdateVisualization();
            
            return arrangeSize;
        }
    }

    public class Fallback2DElement
    {
        public FrameworkElement Visual { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double OriginalX { get; set; }
        public double OriginalY { get; set; }
        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public double Life { get; set; } = 1.0;
        public double MaxLife { get; set; } = 1.0;
        public double AnimationPhase { get; set; }
    }

    public enum Fallback2DType
    {
        SimpleGraph,
        BarChart,
        LineChart,
        DataFlow,
        StatusIndicators
    }

    public enum SimpleColorScheme
    {
        Blue,
        Green,
        Orange,
        Red,
        Purple
    }
}