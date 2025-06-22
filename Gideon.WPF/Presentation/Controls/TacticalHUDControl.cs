using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls
{
    public class TacticalHUDControl : UserControl
    {
        private Canvas _radarCanvas;
        private Canvas _hudOverlay;
        private Ellipse _radarSweep;
        private readonly DispatcherTimer _updateTimer;
        private readonly DispatcherTimer _sweepTimer;
        private double _sweepAngle = 0;
        private readonly Random _random = new();

        public static readonly DependencyProperty RadarRangeProperty =
            DependencyProperty.Register(nameof(RadarRange), typeof(double), typeof(TacticalHUDControl),
                new PropertyMetadata(100.0, OnRadarRangeChanged));

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(TacticalHUDControl),
                new PropertyMetadata(true, OnIsActiveChanged));

        public static readonly DependencyProperty HUDModeProperty =
            DependencyProperty.Register(nameof(HUDMode), typeof(TacticalHUDMode), typeof(TacticalHUDControl),
                new PropertyMetadata(TacticalHUDMode.Overview, OnHUDModeChanged));

        public static readonly DependencyProperty ContactsProperty =
            DependencyProperty.Register(nameof(Contacts), typeof(ObservableCollection<TacticalContact>), typeof(TacticalHUDControl),
                new PropertyMetadata(new ObservableCollection<TacticalContact>(), OnContactsChanged));

        public static readonly DependencyProperty HolographicIntensityProperty =
            DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(TacticalHUDControl),
                new PropertyMetadata(1.0, OnHolographicIntensityChanged));

        public static readonly DependencyProperty ShowThreatAssessmentProperty =
            DependencyProperty.Register(nameof(ShowThreatAssessment), typeof(bool), typeof(TacticalHUDControl),
                new PropertyMetadata(true, OnShowThreatAssessmentChanged));

        public static readonly DependencyProperty ShowNavigationAidsProperty =
            DependencyProperty.Register(nameof(ShowNavigationAids), typeof(bool), typeof(TacticalHUDControl),
                new PropertyMetadata(true, OnShowNavigationAidsChanged));

        public static readonly DependencyProperty ScanModeProperty =
            DependencyProperty.Register(nameof(ScanMode), typeof(ScanMode), typeof(TacticalHUDControl),
                new PropertyMetadata(ScanMode.Passive, OnScanModeChanged));

        public double RadarRange
        {
            get => (double)GetValue(RadarRangeProperty);
            set => SetValue(RadarRangeProperty, value);
        }

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public TacticalHUDMode HUDMode
        {
            get => (TacticalHUDMode)GetValue(HUDModeProperty);
            set => SetValue(HUDModeProperty, value);
        }

        public ObservableCollection<TacticalContact> Contacts
        {
            get => (ObservableCollection<TacticalContact>)GetValue(ContactsProperty);
            set => SetValue(ContactsProperty, value);
        }

        public double HolographicIntensity
        {
            get => (double)GetValue(HolographicIntensityProperty);
            set => SetValue(HolographicIntensityProperty, value);
        }

        public bool ShowThreatAssessment
        {
            get => (bool)GetValue(ShowThreatAssessmentProperty);
            set => SetValue(ShowThreatAssessmentProperty, value);
        }

        public bool ShowNavigationAids
        {
            get => (bool)GetValue(ShowNavigationAidsProperty);
            set => SetValue(ShowNavigationAidsProperty, value);
        }

        public ScanMode ScanMode
        {
            get => (ScanMode)GetValue(ScanModeProperty);
            set => SetValue(ScanModeProperty, value);
        }

        public TacticalHUDControl()
        {
            InitializeHUD();

            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100) // 10 FPS for HUD updates
            };
            _updateTimer.Tick += OnUpdateTick;

            _sweepTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50) // 20 FPS for smooth sweep
            };
            _sweepTimer.Tick += OnSweepTick;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void InitializeHUD()
        {
            Width = 400;
            Height = 400;
            Background = new SolidColorBrush(Color.FromArgb(30, 0, 0, 0));

            // Main container
            var grid = new Grid();
            Content = grid;

            // Radar background
            _radarCanvas = new Canvas
            {
                Background = Brushes.Transparent,
                ClipToBounds = true
            };
            grid.Children.Add(_radarCanvas);

            // HUD overlay
            _hudOverlay = new Canvas
            {
                Background = Brushes.Transparent
            };
            grid.Children.Add(_hudOverlay);

            CreateRadarDisplay();
            CreateHUDElements();
        }

        private void CreateRadarDisplay()
        {
            var centerX = Width / 2;
            var centerY = Height / 2;

            // Radar grid circles
            for (int i = 1; i <= 4; i++)
            {
                var radius = (Width / 2) * (i / 4.0);
                var circle = new Ellipse
                {
                    Width = radius * 2,
                    Height = radius * 2,
                    Stroke = new SolidColorBrush(Color.FromArgb(100, 64, 224, 255)),
                    StrokeThickness = 1,
                    Fill = Brushes.Transparent
                };

                Canvas.SetLeft(circle, centerX - radius);
                Canvas.SetTop(circle, centerY - radius);
                _radarCanvas.Children.Add(circle);

                // Range indicators
                var rangeText = new TextBlock
                {
                    Text = $"{RadarRange * i / 4:F0}km",
                    Foreground = new SolidColorBrush(Color.FromArgb(150, 64, 224, 255)),
                    FontSize = 10,
                    FontFamily = new FontFamily("Consolas")
                };

                Canvas.SetLeft(rangeText, centerX + radius - 20);
                Canvas.SetTop(rangeText, centerY - 10);
                _radarCanvas.Children.Add(rangeText);
            }

            // Radar cross-hairs
            var horizontalLine = new Line
            {
                X1 = 0,
                Y1 = centerY,
                X2 = Width,
                Y2 = centerY,
                Stroke = new SolidColorBrush(Color.FromArgb(80, 64, 224, 255)),
                StrokeThickness = 1
            };
            _radarCanvas.Children.Add(horizontalLine);

            var verticalLine = new Line
            {
                X1 = centerX,
                Y1 = 0,
                X2 = centerX,
                Y2 = Height,
                Stroke = new SolidColorBrush(Color.FromArgb(80, 64, 224, 255)),
                StrokeThickness = 1
            };
            _radarCanvas.Children.Add(verticalLine);

            // Radar sweep
            _radarSweep = new Ellipse
            {
                Width = Width,
                Height = Height,
                Fill = new RadialGradientBrush
                {
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb(60, 64, 224, 255), 0.0),
                        new GradientStop(Color.FromArgb(20, 64, 224, 255), 0.3),
                        new GradientStop(Colors.Transparent, 1.0)
                    }
                },
                RenderTransform = new RotateTransform(),
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            Canvas.SetLeft(_radarSweep, 0);
            Canvas.SetTop(_radarSweep, 0);
            _radarCanvas.Children.Add(_radarSweep);
        }

        private void CreateHUDElements()
        {
            // Status display
            CreateStatusDisplay();

            // Targeting display
            CreateTargetingDisplay();

            // Navigation compass
            CreateNavigationCompass();

            // Threat assessment overlay
            if (ShowThreatAssessment)
                CreateThreatAssessmentOverlay();

            // Navigation aids
            if (ShowNavigationAids)
                CreateNavigationAids();

            // Scan mode indicators
            CreateScanModeIndicators();

            // EVE-style holographic grid
            CreateHolographicGrid();

            // Tactical information panels
            CreateTacticalInfoPanels();
        }

        private void CreateStatusDisplay()
        {
            var statusPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Background = new SolidColorBrush(Color.FromArgb(80, 0, 0, 0))
            };

            var statusBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromArgb(150, 64, 224, 255)),
                BorderThickness = new Thickness(1),
                Child = statusPanel,
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(64, 224, 255),
                    BlurRadius = 10,
                    ShadowDepth = 0,
                    Opacity = 0.5
                }
            };

            // Status items
            var modeText = new TextBlock
            {
                Text = $"MODE: {HUDMode}",
                Foreground = new SolidColorBrush(Color.FromRgb(255, 215, 0)),
                FontSize = 12,
                FontFamily = new FontFamily("Consolas"),
                Margin = new Thickness(5)
            };
            statusPanel.Children.Add(modeText);

            var rangeText = new TextBlock
            {
                Text = $"RANGE: {RadarRange:F0}km",
                Foreground = new SolidColorBrush(Color.FromRgb(64, 224, 255)),
                FontSize = 10,
                FontFamily = new FontFamily("Consolas"),
                Margin = new Thickness(5)
            };
            statusPanel.Children.Add(rangeText);

            var contactsText = new TextBlock
            {
                Text = $"CONTACTS: {Contacts?.Count ?? 0}",
                Foreground = new SolidColorBrush(Color.FromRgb(50, 205, 50)),
                FontSize = 10,
                FontFamily = new FontFamily("Consolas"),
                Margin = new Thickness(5)
            };
            statusPanel.Children.Add(contactsText);

            Canvas.SetLeft(statusBorder, 10);
            Canvas.SetTop(statusBorder, 10);
            _hudOverlay.Children.Add(statusBorder);
        }

        private void CreateTargetingDisplay()
        {
            // Targeting reticle in center
            var reticle = new Canvas
            {
                Width = 60,
                Height = 60
            };

            // Outer circle
            var outerCircle = new Ellipse
            {
                Width = 60,
                Height = 60,
                Stroke = new SolidColorBrush(Color.FromArgb(150, 255, 64, 64)),
                StrokeThickness = 2,
                Fill = Brushes.Transparent,
                StrokeDashArray = new DoubleCollection { 5, 5 }
            };
            reticle.Children.Add(outerCircle);

            // Inner cross
            var hLine = new Line
            {
                X1 = 20,
                Y1 = 30,
                X2 = 40,
                Y2 = 30,
                Stroke = new SolidColorBrush(Color.FromArgb(200, 255, 64, 64)),
                StrokeThickness = 2
            };
            reticle.Children.Add(hLine);

            var vLine = new Line
            {
                X1 = 30,
                Y1 = 20,
                X2 = 30,
                Y2 = 40,
                Stroke = new SolidColorBrush(Color.FromArgb(200, 255, 64, 64)),
                StrokeThickness = 2
            };
            reticle.Children.Add(vLine);

            Canvas.SetLeft(reticle, (Width - 60) / 2);
            Canvas.SetTop(reticle, (Height - 60) / 2);
            _hudOverlay.Children.Add(reticle);
        }

        private void CreateNavigationCompass()
        {
            var compass = new Canvas
            {
                Width = 80,
                Height = 80
            };

            // Compass ring
            var compassRing = new Ellipse
            {
                Width = 80,
                Height = 80,
                Stroke = new SolidColorBrush(Color.FromArgb(120, 255, 215, 0)),
                StrokeThickness = 2,
                Fill = new SolidColorBrush(Color.FromArgb(20, 0, 0, 0))
            };
            compass.Children.Add(compassRing);

            // Cardinal directions
            var directions = new[] { "N", "E", "S", "W" };
            var angles = new[] { 0, 90, 180, 270 };

            for (int i = 0; i < directions.Length; i++)
            {
                var dirText = new TextBlock
                {
                    Text = directions[i],
                    Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 215, 0)),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    FontFamily = new FontFamily("Consolas"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var angle = angles[i];
                var radian = angle * Math.PI / 180;
                var x = 40 + 30 * Math.Sin(radian) - 7;
                var y = 40 - 30 * Math.cos(radian) - 7;

                Canvas.SetLeft(dirText, x);
                Canvas.SetTop(dirText, y);
                compass.Children.Add(dirText);
            }

            // North indicator
            var northArrow = new Polygon
            {
                Points = new PointCollection { new Point(40, 15), new Point(35, 25), new Point(45, 25) },
                Fill = new SolidColorBrush(Color.FromArgb(200, 255, 215, 0)),
                Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 215, 0)),
                StrokeThickness = 1
            };
            compass.Children.Add(northArrow);

            Canvas.SetRight(compass, 10);
            Canvas.SetTop(compass, 10);
            _hudOverlay.Children.Add(compass);
        }

        private void CreateThreatAssessmentOverlay()
        {
            var threatPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Background = new SolidColorBrush(Color.FromArgb(60, 139, 0, 0))
            };

            var threatBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromArgb(180, 255, 64, 64)),
                BorderThickness = new Thickness(1),
                Child = threatPanel,
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(255, 64, 64),
                    BlurRadius = 8,
                    ShadowDepth = 0,
                    Opacity = HolographicIntensity * 0.6
                }
            };

            var threatHeader = new TextBlock
            {
                Text = "THREAT ASSESSMENT",
                Foreground = new SolidColorBrush(Color.FromRgb(255, 64, 64)),
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Consolas"),
                Margin = new Thickness(5, 5, 5, 2)
            };
            threatPanel.Children.Add(threatHeader);

            var threatLevel = new TextBlock
            {
                Text = "LEVEL: MODERATE",
                Foreground = new SolidColorBrush(Color.FromRgb(255, 215, 0)),
                FontSize = 9,
                FontFamily = new FontFamily("Consolas"),
                Margin = new Thickness(5, 0, 5, 2)
            };
            threatPanel.Children.Add(threatLevel);

            var hostileCount = new TextBlock
            {
                Text = $"HOSTILES: {Contacts?.Count(c => c.Type == ContactType.Hostile) ?? 0}",
                Foreground = new SolidColorBrush(Color.FromRgb(255, 64, 64)),
                FontSize = 9,
                FontFamily = new FontFamily("Consolas"),
                Margin = new Thickness(5, 0, 5, 5)
            };
            threatPanel.Children.Add(hostileCount);

            Canvas.SetLeft(threatBorder, 10);
            Canvas.SetBottom(threatBorder, 10);
            _hudOverlay.Children.Add(threatBorder);
        }

        private void CreateNavigationAids()
        {
            // Waypoint indicators
            CreateWaypointIndicators();

            // Jump gate indicators  
            CreateJumpGateIndicators();

            // Station indicators
            CreateStationIndicators();
        }

        private void CreateWaypointIndicators()
        {
            // Sample waypoint at 45 degrees, 75km
            var waypoint = new Canvas();
            
            var waypointIcon = new Polygon
            {
                Points = new PointCollection 
                { 
                    new Point(0, 0), new Point(10, 5), new Point(0, 10), new Point(3, 5) 
                },
                Fill = new SolidColorBrush(Color.FromArgb(150, 0, 255, 0)),
                Stroke = new SolidColorBrush(Color.FromArgb(200, 0, 255, 0)),
                StrokeThickness = 1,
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(0, 255, 0),
                    BlurRadius = 6,
                    ShadowDepth = 0,
                    Opacity = HolographicIntensity * 0.7
                }
            };
            waypoint.Children.Add(waypointIcon);

            var centerX = Width / 2;
            var centerY = Height / 2;
            var distance = 75; // km
            var bearing = 45; // degrees
            var radarRadius = (distance / RadarRange) * (Width / 2 - 20);
            
            var x = centerX + radarRadius * Math.Cos(bearing * Math.PI / 180);
            var y = centerY + radarRadius * Math.Sin(bearing * Math.PI / 180);

            Canvas.SetLeft(waypoint, x - 5);
            Canvas.SetTop(waypoint, y - 5);
            _radarCanvas.Children.Add(waypoint);
        }

        private void CreateJumpGateIndicators()
        {
            // Sample jump gate at 120 degrees, 50km
            var centerX = Width / 2;
            var centerY = Height / 2;
            var distance = 50;
            var bearing = 120;
            var radarRadius = (distance / RadarRange) * (Width / 2 - 20);
            
            var x = centerX + radarRadius * Math.Cos(bearing * Math.PI / 180);
            var y = centerY + radarRadius * Math.Sin(bearing * Math.PI / 180);

            var jumpGate = new Ellipse
            {
                Width = 12,
                Height = 12,
                Fill = Brushes.Transparent,
                Stroke = new SolidColorBrush(Color.FromArgb(180, 138, 43, 226)),
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 3, 3 },
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(138, 43, 226),
                    BlurRadius = 8,
                    ShadowDepth = 0,
                    Opacity = HolographicIntensity * 0.8
                }
            };

            Canvas.SetLeft(jumpGate, x - 6);
            Canvas.SetTop(jumpGate, y - 6);
            _radarCanvas.Children.Add(jumpGate);
        }

        private void CreateStationIndicators()
        {
            // Sample station at 270 degrees, 30km
            var centerX = Width / 2;
            var centerY = Height / 2;
            var distance = 30;
            var bearing = 270;
            var radarRadius = (distance / RadarRange) * (Width / 2 - 20);
            
            var x = centerX + radarRadius * Math.Cos(bearing * Math.PI / 180);
            var y = centerY + radarRadius * Math.Sin(bearing * Math.PI / 180);

            var station = new Rectangle
            {
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(Color.FromArgb(150, 255, 215, 0)),
                Stroke = new SolidColorBrush(Color.FromArgb(200, 255, 215, 0)),
                StrokeThickness = 1,
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(255, 215, 0),
                    BlurRadius = 6,
                    ShadowDepth = 0,
                    Opacity = HolographicIntensity * 0.7
                }
            };

            Canvas.SetLeft(station, x - 4);
            Canvas.SetTop(station, y - 4);
            _radarCanvas.Children.Add(station);
        }

        private void CreateScanModeIndicators()
        {
            var scanPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Background = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0))
            };

            var scanBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromArgb(120, 64, 224, 255)),
                BorderThickness = new Thickness(1),
                Child = scanPanel,
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(64, 224, 255),
                    BlurRadius = 5,
                    ShadowDepth = 0,
                    Opacity = HolographicIntensity * 0.5
                }
            };

            var scanModeText = new TextBlock
            {
                Text = $"SCAN: {ScanMode}",
                Foreground = new SolidColorBrush(Color.FromRgb(64, 224, 255)),
                FontSize = 10,
                FontFamily = new FontFamily("Consolas"),
                Margin = new Thickness(5)
            };
            scanPanel.Children.Add(scanModeText);

            // Scan strength indicator
            var scanStrength = new Rectangle
            {
                Width = 50,
                Height = 8,
                Fill = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 0),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb(200, 0, 255, 0), 0.0),
                        new GradientStop(Color.FromArgb(200, 255, 255, 0), 0.5),
                        new GradientStop(Color.FromArgb(200, 255, 0, 0), 1.0)
                    }
                },
                Margin = new Thickness(5, 8, 5, 8)
            };
            scanPanel.Children.Add(scanStrength);

            Canvas.SetLeft(scanBorder, Width - 120);
            Canvas.SetBottom(scanBorder, 10);
            _hudOverlay.Children.Add(scanBorder);
        }

        private void CreateHolographicGrid()
        {
            // Additional holographic grid overlay for EVE-style appearance
            var gridCanvas = new Canvas
            {
                Width = Width,
                Height = Height,
                IsHitTestVisible = false
            };

            // Diagonal grid lines
            for (int i = -4; i <= 4; i++)
            {
                var line1 = new Line
                {
                    X1 = 0,
                    Y1 = Height / 2 + i * 30,
                    X2 = Width,
                    Y2 = Height / 2 + i * 30,
                    Stroke = new SolidColorBrush(Color.FromArgb((byte)(20 * HolographicIntensity), 64, 224, 255)),
                    StrokeThickness = 0.5,
                    StrokeDashArray = new DoubleCollection { 5, 10 }
                };
                gridCanvas.Children.Add(line1);

                var line2 = new Line
                {
                    X1 = Width / 2 + i * 30,
                    Y1 = 0,
                    X2 = Width / 2 + i * 30,
                    Y2 = Height,
                    Stroke = new SolidColorBrush(Color.FromArgb((byte)(20 * HolographicIntensity), 64, 224, 255)),
                    StrokeThickness = 0.5,
                    StrokeDashArray = new DoubleCollection { 5, 10 }
                };
                gridCanvas.Children.Add(line2);
            }

            _radarCanvas.Children.Insert(0, gridCanvas);
        }

        private void CreateTacticalInfoPanels()
        {
            // System information panel
            CreateSystemInfoPanel();

            // Fleet status panel (if applicable)
            CreateFleetStatusPanel();

            // Capacitor and shield status
            CreateStatusBars();
        }

        private void CreateSystemInfoPanel()
        {
            var sysPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Background = new SolidColorBrush(Color.FromArgb(50, 0, 20, 40))
            };

            var sysBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromArgb(120, 100, 149, 237)),
                BorderThickness = new Thickness(1),
                Child = sysPanel,
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(100, 149, 237),
                    BlurRadius = 6,
                    ShadowDepth = 0,
                    Opacity = HolographicIntensity * 0.4
                }
            };

            var sysHeader = new TextBlock
            {
                Text = "SYSTEM STATUS",
                Foreground = new SolidColorBrush(Color.FromRgb(100, 149, 237)),
                FontSize = 9,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Consolas"),
                Margin = new Thickness(5, 3, 5, 1)
            };
            sysPanel.Children.Add(sysHeader);

            var secStatus = new TextBlock
            {
                Text = "SEC: 0.5",
                Foreground = new SolidColorBrush(Color.FromRgb(255, 215, 0)),
                FontSize = 8,
                FontFamily = new FontFamily("Consolas"),
                Margin = new Thickness(5, 0, 5, 1)
            };
            sysPanel.Children.Add(secStatus);

            var pilots = new TextBlock
            {
                Text = "PILOTS: 47",
                Foreground = new SolidColorBrush(Color.FromRgb(64, 224, 255)),
                FontSize = 8,
                FontFamily = new FontFamily("Consolas"),
                Margin = new Thickness(5, 0, 5, 3)
            };
            sysPanel.Children.Add(pilots);

            Canvas.SetRight(sysBorder, 10);
            Canvas.SetBottom(sysBorder, 60);
            _hudOverlay.Children.Add(sysBorder);
        }

        private void CreateFleetStatusPanel()
        {
            var fleetPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Background = new SolidColorBrush(Color.FromArgb(50, 0, 40, 0))
            };

            var fleetBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromArgb(120, 50, 205, 50)),
                BorderThickness = new Thickness(1),
                Child = fleetPanel,
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(50, 205, 50),
                    BlurRadius = 6,
                    ShadowDepth = 0,
                    Opacity = HolographicIntensity * 0.4
                }
            };

            var fleetHeader = new TextBlock
            {
                Text = "FLEET",
                Foreground = new SolidColorBrush(Color.FromRgb(50, 205, 50)),
                FontSize = 9,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Consolas"),
                Margin = new Thickness(5, 3, 5, 1)
            };
            fleetPanel.Children.Add(fleetHeader);

            var members = new TextBlock
            {
                Text = "MEMBERS: 12",
                Foreground = new SolidColorBrush(Color.FromRgb(64, 224, 255)),
                FontSize = 8,
                FontFamily = new FontFamily("Consolas"),
                Margin = new Thickness(5, 0, 5, 1)
            };
            fleetPanel.Children.Add(members);

            var status = new TextBlock
            {
                Text = "STATUS: ACTIVE",
                Foreground = new SolidColorBrush(Color.FromRgb(50, 205, 50)),
                FontSize = 8,
                FontFamily = new FontFamily("Consolas"),
                Margin = new Thickness(5, 0, 5, 3)
            };
            fleetPanel.Children.Add(status);

            Canvas.SetRight(fleetBorder, 10);
            Canvas.SetBottom(fleetBorder, 120);
            _hudOverlay.Children.Add(fleetBorder);
        }

        private void CreateStatusBars()
        {
            var statusCanvas = new Canvas();

            // Shield bar
            var shieldBar = CreateStatusBar("SHIELD", 85, Color.FromRgb(64, 224, 255), 0);
            statusCanvas.Children.Add(shieldBar);

            // Armor bar  
            var armorBar = CreateStatusBar("ARMOR", 92, Color.FromRgb(255, 215, 0), 20);
            statusCanvas.Children.Add(armorBar);

            // Hull bar
            var hullBar = CreateStatusBar("HULL", 100, Color.FromRgb(255, 64, 64), 40);
            statusCanvas.Children.Add(hullBar);

            Canvas.SetLeft(statusCanvas, 10);
            Canvas.SetBottom(statusCanvas, 60);
            _hudOverlay.Children.Add(statusCanvas);
        }

        private Border CreateStatusBar(string label, double percentage, Color color, double yOffset)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };
            
            var labelText = new TextBlock
            {
                Text = label,
                Foreground = new SolidColorBrush(color),
                FontSize = 8,
                FontFamily = new FontFamily("Consolas"),
                Width = 40,
                Margin = new Thickness(2)
            };
            panel.Children.Add(labelText);

            var barBackground = new Rectangle
            {
                Width = 100,
                Height = 6,
                Fill = new SolidColorBrush(Color.FromArgb(50, color.R, color.G, color.B)),
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 1,
                Margin = new Thickness(2)
            };
            panel.Children.Add(barBackground);

            var barFill = new Rectangle
            {
                Width = percentage,
                Height = 6,
                Fill = new SolidColorBrush(Color.FromArgb(180, color.R, color.G, color.B)),
                Margin = new Thickness(2),
                Effect = new DropShadowEffect
                {
                    Color = color,
                    BlurRadius = 4,
                    ShadowDepth = 0,
                    Opacity = HolographicIntensity * 0.6
                }
            };

            var overlayCanvas = new Canvas();
            overlayCanvas.Children.Add(barBackground);
            overlayCanvas.Children.Add(barFill);
            panel.Children.Add(overlayCanvas);

            var percentText = new TextBlock
            {
                Text = $"{percentage:F0}%",
                Foreground = new SolidColorBrush(color),
                FontSize = 8,
                FontFamily = new FontFamily("Consolas"),
                Margin = new Thickness(5, 2, 2, 2)
            };
            panel.Children.Add(percentText);

            var border = new Border
            {
                Child = panel,
                Background = new SolidColorBrush(Color.FromArgb(30, 0, 0, 0))
            };

            Canvas.SetTop(border, yOffset);
            return border;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (IsActive)
            {
                _updateTimer.Start();
                _sweepTimer.Start();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _updateTimer.Stop();
            _sweepTimer.Stop();
        }

        private void OnUpdateTick(object sender, EventArgs e)
        {
            UpdateContacts();
            UpdateHUDInfo();
        }

        private void OnSweepTick(object sender, EventArgs e)
        {
            _sweepAngle += 3; // 3 degrees per tick
            if (_sweepAngle >= 360)
                _sweepAngle = 0;

            if (_radarSweep.RenderTransform is RotateTransform transform)
            {
                transform.Angle = _sweepAngle;
            }
        }

        private void UpdateContacts()
        {
            // Clear existing contact visuals
            var contactVisuals = _radarCanvas.Children.OfType<Ellipse>()
                .Where(e => e.Tag?.ToString() == "Contact").ToList();
            foreach (var visual in contactVisuals)
                _radarCanvas.Children.Remove(visual);

            if (Contacts == null) return;

            var centerX = Width / 2;
            var centerY = Height / 2;
            var maxRadius = Width / 2 - 20;

            foreach (var contact in Contacts)
            {
                // Calculate position on radar
                var distance = Math.Min(contact.Distance, RadarRange);
                var radarRadius = (distance / RadarRange) * maxRadius;
                
                var x = centerX + radarRadius * Math.Cos(contact.Bearing * Math.PI / 180);
                var y = centerY + radarRadius * Math.Sin(contact.Bearing * Math.PI / 180);

                // Create contact visual
                var contactDot = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = GetContactColor(contact.Type),
                    Stroke = new SolidColorBrush(Colors.White),
                    StrokeThickness = 1,
                    Tag = "Contact",
                    Effect = new DropShadowEffect
                    {
                        Color = GetContactColor(contact.Type).Color,
                        BlurRadius = 8,
                        ShadowDepth = 0,
                        Opacity = 0.8
                    }
                };

                Canvas.SetLeft(contactDot, x - 4);
                Canvas.SetTop(contactDot, y - 4);
                _radarCanvas.Children.Add(contactDot);

                // Add contact label
                var label = new TextBlock
                {
                    Text = contact.Name,
                    Foreground = GetContactColor(contact.Type),
                    FontSize = 8,
                    FontFamily = new FontFamily("Consolas"),
                    Tag = "Contact"
                };

                Canvas.SetLeft(label, x + 8);
                Canvas.SetTop(label, y - 4);
                _radarCanvas.Children.Add(label);
            }
        }

        private void UpdateHUDInfo()
        {
            // Update status displays with current data
            // This would be implemented based on actual data sources
        }

        private SolidColorBrush GetContactColor(ContactType type)
        {
            return type switch
            {
                ContactType.Friendly => new SolidColorBrush(Color.FromArgb(200, 50, 205, 50)),
                ContactType.Hostile => new SolidColorBrush(Color.FromArgb(200, 255, 64, 64)),
                ContactType.Neutral => new SolidColorBrush(Color.FromArgb(200, 255, 215, 0)),
                ContactType.Unknown => new SolidColorBrush(Color.FromArgb(200, 128, 128, 128)),
                _ => new SolidColorBrush(Color.FromArgb(200, 64, 224, 255))
            };
        }

        private static void OnRadarRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TacticalHUDControl hud)
                hud.UpdateContacts();
        }

        private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TacticalHUDControl hud)
            {
                if ((bool)e.NewValue)
                {
                    hud._updateTimer.Start();
                    hud._sweepTimer.Start();
                }
                else
                {
                    hud._updateTimer.Stop();
                    hud._sweepTimer.Stop();
                }
            }
        }

        private static void OnHUDModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TacticalHUDControl hud)
                hud.UpdateHUDInfo();
        }

        private static void OnContactsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TacticalHUDControl hud)
                hud.UpdateContacts();
        }

        private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TacticalHUDControl hud)
                hud.UpdateHolographicEffects();
        }

        private static void OnShowThreatAssessmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TacticalHUDControl hud)
                hud.RefreshHUD();
        }

        private static void OnShowNavigationAidsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TacticalHUDControl hud)
                hud.RefreshHUD();
        }

        private static void OnScanModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TacticalHUDControl hud)
                hud.UpdateScanModeDisplay();
        }

        private void UpdateHolographicEffects()
        {
            // Update all holographic effects based on intensity
            foreach (var child in _hudOverlay.Children.OfType<Border>())
            {
                if (child.Effect is DropShadowEffect effect)
                {
                    effect.Opacity = effect.Opacity * HolographicIntensity;
                }
            }
        }

        private void RefreshHUD()
        {
            _hudOverlay.Children.Clear();
            CreateHUDElements();
        }

        private void UpdateScanModeDisplay()
        {
            // Update scan mode display - would be enhanced with real scan data
            // Currently just updates the display text
        }
    }

    public enum TacticalHUDMode
    {
        Overview,
        Navigation,
        Combat,
        Exploration
    }

    public enum ContactType
    {
        Friendly,
        Hostile,
        Neutral,
        Unknown,
        Station,
        Gate,
        Celestial
    }

    public enum ScanMode
    {
        Passive,
        Active,
        Combat,
        Directional
    }

    public class TacticalContact
    {
        public string Name { get; set; } = string.Empty;
        public double Distance { get; set; }
        public double Bearing { get; set; }
        public ContactType Type { get; set; }
        public string Details { get; set; } = string.Empty;
        public double Velocity { get; set; }
        public string ShipType { get; set; } = string.Empty;
        public int ThreatLevel { get; set; } // 0-10 scale
        public DateTime LastSeen { get; set; } = DateTime.Now;
        public bool IsLocked { get; set; }
        public double SignalStrength { get; set; } = 1.0;
    }
}