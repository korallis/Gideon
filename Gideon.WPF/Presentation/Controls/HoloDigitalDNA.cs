// ==========================================================================
// HoloDigitalDNA.cs - Digital DNA Connection Effects for Modules
// ==========================================================================
// Advanced module connection visualization featuring digital DNA helixes,
// animated data flows, system interconnections, and EVE-style network effects.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Digital DNA connection effects system showing module interconnections and data flows
/// </summary>
public class HoloDigitalDNA : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloDigitalDNA),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloDigitalDNA),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty ModuleConnectionsProperty =
        DependencyProperty.Register(nameof(ModuleConnections), typeof(List<ModuleConnection>), typeof(HoloDigitalDNA),
            new PropertyMetadata(null, OnModuleConnectionsChanged));

    public static readonly DependencyProperty ConnectionStyleProperty =
        DependencyProperty.Register(nameof(ConnectionStyle), typeof(ConnectionStyle), typeof(HoloDigitalDNA),
            new PropertyMetadata(ConnectionStyle.DigitalDNA, OnConnectionStyleChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloDigitalDNA),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloDigitalDNA),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowDataFlowProperty =
        DependencyProperty.Register(nameof(ShowDataFlow), typeof(bool), typeof(HoloDigitalDNA),
            new PropertyMetadata(true, OnShowDataFlowChanged));

    public static readonly DependencyProperty ShowSystemInterconnectsProperty =
        DependencyProperty.Register(nameof(ShowSystemInterconnects), typeof(bool), typeof(HoloDigitalDNA),
            new PropertyMetadata(true, OnShowSystemInterconnectsChanged));

    public static readonly DependencyProperty DNAComplexityProperty =
        DependencyProperty.Register(nameof(DNAComplexity), typeof(DNAComplexity), typeof(HoloDigitalDNA),
            new PropertyMetadata(DNAComplexity.Medium, OnDNAComplexityChanged));

    public static readonly DependencyProperty AnimationSpeedProperty =
        DependencyProperty.Register(nameof(AnimationSpeed), typeof(double), typeof(HoloDigitalDNA),
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

    public List<ModuleConnection> ModuleConnections
    {
        get => (List<ModuleConnection>)GetValue(ModuleConnectionsProperty);
        set => SetValue(ModuleConnectionsProperty, value);
    }

    public ConnectionStyle ConnectionStyle
    {
        get => (ConnectionStyle)GetValue(ConnectionStyleProperty);
        set => SetValue(ConnectionStyleProperty, value);
    }

    public bool EnableAnimations
    {
        get => (bool)GetValue(EnableAnimationsProperty);
        set => SetValue(EnableAnimationsProperty, value);
    }

    public bool EnableParticleEffects
    {
        get => (bool)GetValue(EnableParticleEffectsProperty);
        set => SetValue(EnableParticleEffectsProperty, value);
    }

    public bool ShowDataFlow
    {
        get => (bool)GetValue(ShowDataFlowProperty);
        set => SetValue(ShowDataFlowProperty, value);
    }

    public bool ShowSystemInterconnects
    {
        get => (bool)GetValue(ShowSystemInterconnectsProperty);
        set => SetValue(ShowSystemInterconnectsProperty, value);
    }

    public DNAComplexity DNAComplexity
    {
        get => (DNAComplexity)GetValue(DNAComplexityProperty);
        set => SetValue(DNAComplexityProperty, value);
    }

    public double AnimationSpeed
    {
        get => (double)GetValue(AnimationSpeedProperty);
        set => SetValue(AnimationSpeedProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<ConnectionEventArgs> ConnectionActivated;
    public event EventHandler<ConnectionEventArgs> ConnectionDeactivated;
    public event EventHandler<ConnectionEventArgs> DataFlowStarted;
    public event EventHandler<ConnectionEventArgs> DataFlowCompleted;
    public event EventHandler<DNAEventArgs> DNAHelixCompleted;

    #endregion

    #region Private Fields

    private readonly DispatcherTimer _animationTimer;
    private readonly DispatcherTimer _dataFlowTimer;
    private readonly Dictionary<string, List<UIElement>> _connectionElements = new();
    private readonly Dictionary<string, List<UIElement>> _dataFlowElements = new();
    private readonly Dictionary<string, DNAHelix> _dnaMolecules = new();
    private readonly List<UIElement> _particlePool = new();
    private Canvas _mainCanvas;
    private Canvas _dnaCanvas;
    private Canvas _dataFlowCanvas;
    private Canvas _particleCanvas;
    private bool _isSimplifiedMode;
    private double _animationPhase;
    private double _dataFlowPhase;
    private readonly Random _random = new();

    #endregion

    #region Constructor

    public HoloDigitalDNA()
    {
        InitializeComponent();
        InitializeTimers();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 800;
        Height = 600;
        Background = new SolidColorBrush(Colors.Transparent);
        
        CreateCanvasLayers();
        ApplyEVEColorScheme();
    }

    private void CreateCanvasLayers()
    {
        _mainCanvas = new Canvas
        {
            Width = Width,
            Height = Height,
            ClipToBounds = true
        };
        Content = _mainCanvas;

        // DNA helix layer (background)
        _dnaCanvas = new Canvas
        {
            Width = Width,
            Height = Height,
            IsHitTestVisible = false
        };
        _mainCanvas.Children.Add(_dnaCanvas);

        // Data flow layer (middle)
        _dataFlowCanvas = new Canvas
        {
            Width = Width,
            Height = Height,
            IsHitTestVisible = false
        };
        _mainCanvas.Children.Add(_dataFlowCanvas);

        // Particle effects layer (foreground)
        _particleCanvas = new Canvas
        {
            Width = Width,
            Height = Height,
            IsHitTestVisible = false
        };
        _mainCanvas.Children.Add(_particleCanvas);
    }

    private void InitializeTimers()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
        };
        _animationTimer.Tick += OnAnimationTimerTick;

        _dataFlowTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS for smooth data flow
        };
        _dataFlowTimer.Tick += OnDataFlowTimerTick;
    }

    #endregion

    #region Public Methods

    public void CreateConnection(ModuleConnection connection)
    {
        if (connection == null) return;

        switch (ConnectionStyle)
        {
            case ConnectionStyle.DigitalDNA:
                CreateDNAConnection(connection);
                break;
            case ConnectionStyle.DataStream:
                CreateDataStreamConnection(connection);
                break;
            case ConnectionStyle.Network:
                CreateNetworkConnection(connection);
                break;
            case ConnectionStyle.Quantum:
                CreateQuantumConnection(connection);
                break;
        }

        if (EnableAnimations && !_isSimplifiedMode)
        {
            AnimateConnectionActivation(connection);
        }

        ConnectionActivated?.Invoke(this, new ConnectionEventArgs
        {
            Connection = connection,
            Timestamp = DateTime.Now
        });
    }

    public void RemoveConnection(string connectionId)
    {
        if (string.IsNullOrEmpty(connectionId)) return;

        if (_connectionElements.TryGetValue(connectionId, out var elements))
        {
            foreach (var element in elements)
            {
                AnimateConnectionDeactivation(element, () =>
                {
                    RemoveElementFromCanvas(element);
                });
            }
            _connectionElements.Remove(connectionId);
        }

        if (_dnaMolecules.TryGetValue(connectionId, out var dna))
        {
            dna.Dispose();
            _dnaMolecules.Remove(connectionId);
        }

        CleanupConnectionParticles(connectionId);
    }

    public void StartDataFlow(string connectionId, DataFlowDirection direction = DataFlowDirection.Bidirectional)
    {
        if (!ShowDataFlow || string.IsNullOrEmpty(connectionId)) return;
        
        var connection = ModuleConnections?.FirstOrDefault(c => c.Id == connectionId);
        if (connection == null) return;

        CreateDataFlowAnimation(connection, direction);

        DataFlowStarted?.Invoke(this, new ConnectionEventArgs
        {
            Connection = connection,
            Timestamp = DateTime.Now
        });
    }

    public void StopDataFlow(string connectionId)
    {
        if (_dataFlowElements.TryGetValue(connectionId, out var elements))
        {
            foreach (var element in elements)
            {
                StopElementAnimations(element);
                AnimateElementFadeOut(element, () => RemoveElementFromCanvas(element));
            }
            _dataFlowElements.Remove(connectionId);
        }
    }

    public void PulseConnection(string connectionId, int pulseCount = 3)
    {
        if (!_connectionElements.ContainsKey(connectionId)) return;

        var elements = _connectionElements[connectionId];
        foreach (var element in elements)
        {
            CreatePulseAnimation(element, pulseCount);
        }
    }

    public void CreateSystemInterconnects(List<SystemNode> systemNodes)
    {
        if (!ShowSystemInterconnects || systemNodes == null) return;

        foreach (var node in systemNodes)
        {
            CreateSystemNodeVisualization(node);
        }

        // Create interconnection matrix
        for (int i = 0; i < systemNodes.Count; i++)
        {
            for (int j = i + 1; j < systemNodes.Count; j++)
            {
                if (AreSystemsConnected(systemNodes[i], systemNodes[j]))
                {
                    CreateSystemConnection(systemNodes[i], systemNodes[j]);
                }
            }
        }
    }

    public void UpdateConnectionStrength(string connectionId, double strength)
    {
        if (!_connectionElements.ContainsKey(connectionId)) return;

        var elements = _connectionElements[connectionId];
        foreach (var element in elements)
        {
            UpdateElementStrength(element, strength);
        }

        if (_dnaMolecules.TryGetValue(connectionId, out var dna))
        {
            dna.UpdateStrength(strength);
        }
    }

    public void ClearAllConnections()
    {
        foreach (var connectionId in _connectionElements.Keys.ToList())
        {
            RemoveConnection(connectionId);
        }

        foreach (var dna in _dnaMolecules.Values)
        {
            dna.Dispose();
        }
        _dnaMolecules.Clear();

        _dnaCanvas.Children.Clear();
        _dataFlowCanvas.Children.Clear();
        CleanupAllParticles();
    }

    #endregion

    #region Connection Creation Methods

    private void CreateDNAConnection(ModuleConnection connection)
    {
        var dnaHelix = new DNAHelix(connection, GetConnectionColor(connection.Type));
        _dnaMolecules[connection.Id] = dnaHelix;

        var helixElements = dnaHelix.CreateVisualization(DNAComplexity);
        _connectionElements[connection.Id] = helixElements;

        foreach (var element in helixElements)
        {
            _dnaCanvas.Children.Add(element);
        }

        // Position the DNA helix between the connection points
        PositionDNAHelix(dnaHelix, connection.StartPoint, connection.EndPoint);
    }

    private void CreateDataStreamConnection(ModuleConnection connection)
    {
        var elements = new List<UIElement>();

        // Create main connection line
        var connectionLine = new Line
        {
            X1 = connection.StartPoint.X,
            Y1 = connection.StartPoint.Y,
            X2 = connection.EndPoint.X,
            Y2 = connection.EndPoint.Y,
            Stroke = new SolidColorBrush(GetConnectionColor(connection.Type)),
            StrokeThickness = GetConnectionThickness(connection.Strength),
            StrokeDashArray = new DoubleCollection { 5, 3 },
            Opacity = HolographicIntensity
        };

        elements.Add(connectionLine);
        _dnaCanvas.Children.Add(connectionLine);

        // Create data stream particles
        CreateDataStreamParticles(connection, elements);

        _connectionElements[connection.Id] = elements;
    }

    private void CreateNetworkConnection(ModuleConnection connection)
    {
        var elements = new List<UIElement>();

        // Create network-style connection with nodes
        var nodeCount = (int)(Vector.Subtract(connection.EndPoint, connection.StartPoint).Length / 50);
        var nodePositions = CalculateNetworkNodePositions(connection.StartPoint, connection.EndPoint, nodeCount);

        for (int i = 0; i < nodePositions.Count; i++)
        {
            // Create network node
            var node = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(GetConnectionColor(connection.Type)),
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 1,
                Opacity = HolographicIntensity
            };

            Canvas.SetLeft(node, nodePositions[i].X - 4);
            Canvas.SetTop(node, nodePositions[i].Y - 4);
            elements.Add(node);
            _dnaCanvas.Children.Add(node);

            // Create connection to next node
            if (i < nodePositions.Count - 1)
            {
                var connectionLine = new Line
                {
                    X1 = nodePositions[i].X,
                    Y1 = nodePositions[i].Y,
                    X2 = nodePositions[i + 1].X,
                    Y2 = nodePositions[i + 1].Y,
                    Stroke = new SolidColorBrush(GetConnectionColor(connection.Type)),
                    StrokeThickness = 2,
                    Opacity = HolographicIntensity * 0.7
                };

                elements.Add(connectionLine);
                _dnaCanvas.Children.Add(connectionLine);
            }
        }

        _connectionElements[connection.Id] = elements;
    }

    private void CreateQuantumConnection(ModuleConnection connection)
    {
        var elements = new List<UIElement>();

        // Create quantum entanglement effect
        var quantumField = new Ellipse
        {
            Width = 100,
            Height = 100,
            Fill = new RadialGradientBrush
            {
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(100, GetConnectionColor(connection.Type).R, 
                        GetConnectionColor(connection.Type).G, GetConnectionColor(connection.Type).B), 0),
                    new GradientStop(Color.FromArgb(0, GetConnectionColor(connection.Type).R, 
                        GetConnectionColor(connection.Type).G, GetConnectionColor(connection.Type).B), 1)
                }
            },
            Opacity = HolographicIntensity
        };

        var midPoint = new Point(
            (connection.StartPoint.X + connection.EndPoint.X) / 2,
            (connection.StartPoint.Y + connection.EndPoint.Y) / 2);

        Canvas.SetLeft(quantumField, midPoint.X - 50);
        Canvas.SetTop(quantumField, midPoint.Y - 50);

        elements.Add(quantumField);
        _dnaCanvas.Children.Add(quantumField);

        // Create quantum particles
        CreateQuantumParticles(connection, elements);

        _connectionElements[connection.Id] = elements;
    }

    #endregion

    #region Animation Methods

    private void AnimateConnectionActivation(ModuleConnection connection)
    {
        if (!_connectionElements.ContainsKey(connection.Id)) return;

        var elements = _connectionElements[connection.Id];
        foreach (var element in elements)
        {
            // Scale in animation
            var scaleTransform = new ScaleTransform(0, 0);
            element.RenderTransform = scaleTransform;
            element.RenderTransformOrigin = new Point(0.5, 0.5);

            var scaleAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);

            // Fade in animation
            var fadeAnimation = new DoubleAnimation
            {
                From = 0,
                To = HolographicIntensity,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            element.BeginAnimation(OpacityProperty, fadeAnimation);
        }

        // Spawn activation particles
        if (EnableParticleEffects && !_isSimplifiedMode)
        {
            SpawnActivationParticles(connection);
        }
    }

    private void AnimateConnectionDeactivation(UIElement element, Action onComplete)
    {
        var fadeAnimation = new DoubleAnimation
        {
            From = element.Opacity,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        fadeAnimation.Completed += (s, e) => onComplete?.Invoke();
        element.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void CreateDataFlowAnimation(ModuleConnection connection, DataFlowDirection direction)
    {
        var dataFlowElements = new List<UIElement>();

        // Create flowing data particles
        for (int i = 0; i < 5; i++)
        {
            var dataParticle = new Ellipse
            {
                Width = 4,
                Height = 4,
                Fill = new SolidColorBrush(GetDataFlowColor(connection.Type)),
                Opacity = HolographicIntensity
            };

            Canvas.SetLeft(dataParticle, connection.StartPoint.X);
            Canvas.SetTop(dataParticle, connection.StartPoint.Y);

            dataFlowElements.Add(dataParticle);
            _dataFlowCanvas.Children.Add(dataParticle);

            // Animate particle flow
            AnimateDataParticle(dataParticle, connection, direction, i * 200);
        }

        _dataFlowElements[connection.Id] = dataFlowElements;
    }

    private void AnimateDataParticle(UIElement particle, ModuleConnection connection, DataFlowDirection direction, int delay)
    {
        var startPoint = direction == DataFlowDirection.Reverse ? connection.EndPoint : connection.StartPoint;
        var endPoint = direction == DataFlowDirection.Reverse ? connection.StartPoint : connection.EndPoint;

        var moveXAnimation = new DoubleAnimation
        {
            From = startPoint.X,
            To = endPoint.X,
            Duration = TimeSpan.FromSeconds(2 / AnimationSpeed),
            BeginTime = TimeSpan.FromMilliseconds(delay),
            RepeatBehavior = RepeatBehavior.Forever
        };

        var moveYAnimation = new DoubleAnimation
        {
            From = startPoint.Y,
            To = endPoint.Y,
            Duration = TimeSpan.FromSeconds(2 / AnimationSpeed),
            BeginTime = TimeSpan.FromMilliseconds(delay),
            RepeatBehavior = RepeatBehavior.Forever
        };

        particle.BeginAnimation(Canvas.LeftProperty, moveXAnimation);
        particle.BeginAnimation(Canvas.TopProperty, moveYAnimation);
    }

    private void CreatePulseAnimation(UIElement element, int pulseCount)
    {
        var scaleTransform = element.RenderTransform as ScaleTransform ?? new ScaleTransform();
        element.RenderTransform = scaleTransform;
        element.RenderTransformOrigin = new Point(0.5, 0.5);

        var pulseAnimation = new DoubleAnimation
        {
            From = 1,
            To = 1.5,
            Duration = TimeSpan.FromMilliseconds(300),
            RepeatBehavior = new RepeatBehavior(pulseCount),
            AutoReverse = true,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, pulseAnimation);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, pulseAnimation);
    }

    #endregion

    #region Particle Effects

    private void SpawnActivationParticles(ModuleConnection connection)
    {
        var particleCount = DNAComplexity switch
        {
            DNAComplexity.Low => 5,
            DNAComplexity.Medium => 10,
            DNAComplexity.High => 15,
            _ => 8
        };

        for (int i = 0; i < particleCount; i++)
        {
            var particle = CreateActivationParticle(connection);
            _particleCanvas.Children.Add(particle);
            AnimateActivationParticle(particle, connection);
        }
    }

    private UIElement CreateActivationParticle(ModuleConnection connection)
    {
        var size = _random.Next(2, 6);
        var particle = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(GetConnectionColor(connection.Type))
        };

        // Position at connection midpoint
        var midPoint = new Point(
            (connection.StartPoint.X + connection.EndPoint.X) / 2,
            (connection.StartPoint.Y + connection.EndPoint.Y) / 2);

        Canvas.SetLeft(particle, midPoint.X);
        Canvas.SetTop(particle, midPoint.Y);

        return particle;
    }

    private void AnimateActivationParticle(UIElement particle, ModuleConnection connection)
    {
        var angle = _random.NextDouble() * Math.PI * 2;
        var distance = _random.Next(30, 80);
        var targetX = Canvas.GetLeft(particle) + Math.Cos(angle) * distance;
        var targetY = Canvas.GetTop(particle) + Math.Sin(angle) * distance;

        var moveXAnimation = new DoubleAnimation
        {
            From = Canvas.GetLeft(particle),
            To = targetX,
            Duration = TimeSpan.FromMilliseconds(_random.Next(1000, 2000)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var moveYAnimation = new DoubleAnimation
        {
            From = Canvas.GetTop(particle),
            To = targetY,
            Duration = TimeSpan.FromMilliseconds(_random.Next(1000, 2000)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(_random.Next(1000, 2000))
        };

        moveXAnimation.Completed += (s, e) => _particleCanvas.Children.Remove(particle);

        particle.BeginAnimation(Canvas.LeftProperty, moveXAnimation);
        particle.BeginAnimation(Canvas.TopProperty, moveYAnimation);
        particle.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void CreateDataStreamParticles(ModuleConnection connection, List<UIElement> elements)
    {
        // Create stream of small particles along the connection
        var distance = Vector.Subtract(connection.EndPoint, connection.StartPoint).Length;
        var particleCount = (int)(distance / 20);

        for (int i = 0; i < particleCount; i++)
        {
            var t = (double)i / particleCount;
            var position = new Point(
                connection.StartPoint.X + (connection.EndPoint.X - connection.StartPoint.X) * t,
                connection.StartPoint.Y + (connection.EndPoint.Y - connection.StartPoint.Y) * t);

            var particle = new Ellipse
            {
                Width = 3,
                Height = 3,
                Fill = new SolidColorBrush(GetDataFlowColor(connection.Type)),
                Opacity = HolographicIntensity * 0.7
            };

            Canvas.SetLeft(particle, position.X);
            Canvas.SetTop(particle, position.Y);
            elements.Add(particle);
            _dnaCanvas.Children.Add(particle);

            if (EnableAnimations && !_isSimplifiedMode)
            {
                // Add subtle pulsing animation
                var pulseAnimation = new DoubleAnimation
                {
                    From = 0.3,
                    To = 1.0,
                    Duration = TimeSpan.FromSeconds(1 + _random.NextDouble()),
                    BeginTime = TimeSpan.FromMilliseconds(i * 100),
                    RepeatBehavior = RepeatBehavior.Forever,
                    AutoReverse = true
                };
                particle.BeginAnimation(OpacityProperty, pulseAnimation);
            }
        }
    }

    private void CreateQuantumParticles(ModuleConnection connection, List<UIElement> elements)
    {
        // Create quantum entanglement particles
        for (int i = 0; i < 8; i++)
        {
            var particle = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = new SolidColorBrush(GetConnectionColor(connection.Type)),
                Opacity = HolographicIntensity
            };

            var angle = (Math.PI * 2 / 8) * i;
            var radius = 40;
            var midPoint = new Point(
                (connection.StartPoint.X + connection.EndPoint.X) / 2,
                (connection.StartPoint.Y + connection.EndPoint.Y) / 2);

            var x = midPoint.X + Math.Cos(angle) * radius;
            var y = midPoint.Y + Math.Sin(angle) * radius;

            Canvas.SetLeft(particle, x);
            Canvas.SetTop(particle, y);
            elements.Add(particle);
            _dnaCanvas.Children.Add(particle);

            if (EnableAnimations && !_isSimplifiedMode)
            {
                // Orbital animation
                var rotateTransform = new RotateTransform(0, midPoint.X, midPoint.Y);
                particle.RenderTransform = rotateTransform;

                var rotateAnimation = new DoubleAnimation
                {
                    From = angle * 180 / Math.PI,
                    To = (angle * 180 / Math.PI) + 360,
                    Duration = TimeSpan.FromSeconds(4 / AnimationSpeed),
                    RepeatBehavior = RepeatBehavior.Forever
                };

                rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
            }
        }
    }

    #endregion

    #region Helper Methods

    private void PositionDNAHelix(DNAHelix helix, Point start, Point end)
    {
        helix.SetPosition(start, end);
    }

    private List<Point> CalculateNetworkNodePositions(Point start, Point end, int nodeCount)
    {
        var positions = new List<Point>();
        
        for (int i = 0; i <= nodeCount; i++)
        {
            var t = (double)i / nodeCount;
            var x = start.X + (end.X - start.X) * t;
            var y = start.Y + (end.Y - start.Y) * t;
            
            // Add some randomness for organic network feel
            x += (_random.NextDouble() - 0.5) * 10;
            y += (_random.NextDouble() - 0.5) * 10;
            
            positions.Add(new Point(x, y));
        }
        
        return positions;
    }

    private void CreateSystemNodeVisualization(SystemNode node)
    {
        // Implementation for system node visualization
        var nodeElement = new Ellipse
        {
            Width = node.Size,
            Height = node.Size,
            Fill = new SolidColorBrush(GetSystemNodeColor(node.SystemType)),
            Stroke = new SolidColorBrush(Colors.White),
            StrokeThickness = 2,
            Opacity = HolographicIntensity
        };

        Canvas.SetLeft(nodeElement, node.Position.X - node.Size / 2);
        Canvas.SetTop(nodeElement, node.Position.Y - node.Size / 2);
        _dnaCanvas.Children.Add(nodeElement);

        // Add pulsing effect for active systems
        if (node.IsActive && EnableAnimations && !_isSimplifiedMode)
        {
            var pulseAnimation = new DoubleAnimation
            {
                From = 0.7,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(2),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };
            nodeElement.BeginAnimation(OpacityProperty, pulseAnimation);
        }
    }

    private void CreateSystemConnection(SystemNode nodeA, SystemNode nodeB)
    {
        var connection = new Line
        {
            X1 = nodeA.Position.X,
            Y1 = nodeA.Position.Y,
            X2 = nodeB.Position.X,
            Y2 = nodeB.Position.Y,
            Stroke = new SolidColorBrush(GetSystemConnectionColor()),
            StrokeThickness = 1,
            StrokeDashArray = new DoubleCollection { 3, 2 },
            Opacity = HolographicIntensity * 0.5
        };

        _dnaCanvas.Children.Add(connection);
    }

    private bool AreSystemsConnected(SystemNode nodeA, SystemNode nodeB)
    {
        // Mock implementation - in real app this would check actual system dependencies
        return nodeA.SystemType != nodeB.SystemType && 
               Vector.Subtract(nodeA.Position, nodeB.Position).Length < 150;
    }

    private Color GetConnectionColor(ConnectionType type)
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        
        return type switch
        {
            ConnectionType.Power => Color.FromRgb(255, 255, 100),
            ConnectionType.Data => colors.Primary,
            ConnectionType.Shield => Color.FromRgb(100, 150, 255),
            ConnectionType.Weapon => Color.FromRgb(255, 100, 100),
            ConnectionType.Navigation => Color.FromRgb(100, 255, 150),
            ConnectionType.Sensor => Color.FromRgb(200, 100, 255),
            _ => colors.Primary
        };
    }

    private Color GetDataFlowColor(ConnectionType type)
    {
        var baseColor = GetConnectionColor(type);
        return Color.FromRgb(
            (byte)Math.Min(255, baseColor.R + 50),
            (byte)Math.Min(255, baseColor.G + 50),
            (byte)Math.Min(255, baseColor.B + 50));
    }

    private Color GetSystemNodeColor(SystemType systemType)
    {
        return systemType switch
        {
            SystemType.PowerCore => Color.FromRgb(255, 255, 100),
            SystemType.ShieldGenerator => Color.FromRgb(100, 150, 255),
            SystemType.WeaponSystem => Color.FromRgb(255, 100, 100),
            SystemType.PropulsionSystem => Color.FromRgb(100, 255, 150),
            SystemType.SensorArray => Color.FromRgb(200, 100, 255),
            SystemType.LifeSupport => Color.FromRgb(150, 255, 150),
            _ => Color.FromRgb(150, 150, 150)
        };
    }

    private Color GetSystemConnectionColor()
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        return colors.Secondary;
    }

    private double GetConnectionThickness(double strength)
    {
        return Math.Max(1, strength * 4);
    }

    private void UpdateElementStrength(UIElement element, double strength)
    {
        element.Opacity = HolographicIntensity * strength;
        
        if (element is Shape shape)
        {
            shape.StrokeThickness = GetConnectionThickness(strength);
        }
    }

    private void StopElementAnimations(UIElement element)
    {
        element.BeginAnimation(Canvas.LeftProperty, null);
        element.BeginAnimation(Canvas.TopProperty, null);
        element.BeginAnimation(OpacityProperty, null);
        
        if (element.RenderTransform is Transform transform)
        {
            if (transform is RotateTransform rotateTransform)
            {
                rotateTransform.BeginAnimation(RotateTransform.AngleProperty, null);
            }
            else if (transform is ScaleTransform scaleTransform)
            {
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            }
        }
    }

    private void AnimateElementFadeOut(UIElement element, Action onComplete)
    {
        var fadeAnimation = new DoubleAnimation
        {
            From = element.Opacity,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(300)
        };

        fadeAnimation.Completed += (s, e) => onComplete?.Invoke();
        element.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void RemoveElementFromCanvas(UIElement element)
    {
        if (_dnaCanvas.Children.Contains(element))
            _dnaCanvas.Children.Remove(element);
        else if (_dataFlowCanvas.Children.Contains(element))
            _dataFlowCanvas.Children.Remove(element);
        else if (_particleCanvas.Children.Contains(element))
            _particleCanvas.Children.Remove(element);
    }

    private void CleanupConnectionParticles(string connectionId)
    {
        if (_dataFlowElements.TryGetValue(connectionId, out var elements))
        {
            foreach (var element in elements)
            {
                RemoveElementFromCanvas(element);
            }
            _dataFlowElements.Remove(connectionId);
        }
    }

    private void CleanupAllParticles()
    {
        _particleCanvas.Children.Clear();
        foreach (var elements in _dataFlowElements.Values)
        {
            foreach (var element in elements)
            {
                RemoveElementFromCanvas(element);
            }
        }
        _dataFlowElements.Clear();
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableAnimations && !_isSimplifiedMode)
        {
            _animationTimer.Start();
        }
        
        if (ShowDataFlow && !_isSimplifiedMode)
        {
            _dataFlowTimer.Start();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer.Stop();
        _dataFlowTimer.Stop();
        ClearAllConnections();
    }

    private void OnAnimationTimerTick(object sender, EventArgs e)
    {
        if (!EnableAnimations || _isSimplifiedMode) return;

        _animationPhase += 0.1 * AnimationSpeed;
        if (_animationPhase > Math.PI * 2)
            _animationPhase = 0;

        UpdateDNAAnimations();
    }

    private void OnDataFlowTimerTick(object sender, EventArgs e)
    {
        if (!ShowDataFlow || _isSimplifiedMode) return;

        _dataFlowPhase += 0.15 * AnimationSpeed;
        if (_dataFlowPhase > Math.PI * 2)
            _dataFlowPhase = 0;

        UpdateDataFlowAnimations();
    }

    private void UpdateDNAAnimations()
    {
        foreach (var dna in _dnaMolecules.Values)
        {
            dna.UpdateAnimation(_animationPhase);
        }
    }

    private void UpdateDataFlowAnimations()
    {
        // Update ongoing data flow animations
        foreach (var elements in _dataFlowElements.Values)
        {
            foreach (var element in elements)
            {
                if (element is Shape shape && shape.Fill is SolidColorBrush brush)
                {
                    var intensity = (Math.Sin(_dataFlowPhase * 2) + 1) / 2;
                    brush.Opacity = HolographicIntensity * intensity;
                }
            }
        }
    }

    #endregion

    #region Property Change Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDigitalDNA dna)
        {
            dna.UpdateAllElementsIntensity();
        }
    }

    private void UpdateAllElementsIntensity()
    {
        foreach (var elements in _connectionElements.Values)
        {
            foreach (var element in elements)
            {
                element.Opacity = HolographicIntensity;
            }
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDigitalDNA dna)
        {
            dna.ApplyEVEColorScheme();
        }
    }

    private static void OnModuleConnectionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDigitalDNA dna)
        {
            dna.RefreshConnections();
        }
    }

    private void RefreshConnections()
    {
        ClearAllConnections();
        
        if (ModuleConnections != null)
        {
            foreach (var connection in ModuleConnections)
            {
                CreateConnection(connection);
            }
        }
    }

    private static void OnConnectionStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDigitalDNA dna)
        {
            dna.RefreshConnections();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDigitalDNA dna)
        {
            if ((bool)e.NewValue && !dna._isSimplifiedMode)
            {
                dna._animationTimer.Start();
            }
            else
            {
                dna._animationTimer.Stop();
            }
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDigitalDNA dna && !(bool)e.NewValue)
        {
            dna.CleanupAllParticles();
        }
    }

    private static void OnShowDataFlowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDigitalDNA dna)
        {
            if ((bool)e.NewValue && !dna._isSimplifiedMode)
            {
                dna._dataFlowTimer.Start();
            }
            else
            {
                dna._dataFlowTimer.Stop();
            }
        }
    }

    private static void OnShowSystemInterconnectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDigitalDNA dna)
        {
            dna.RefreshConnections();
        }
    }

    private static void OnDNAComplexityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDigitalDNA dna)
        {
            dna.RefreshConnections();
        }
    }

    private static void OnAnimationSpeedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDigitalDNA dna)
        {
            var speed = (double)e.NewValue;
            dna._animationTimer.Interval = TimeSpan.FromMilliseconds(33 / speed);
            dna._dataFlowTimer.Interval = TimeSpan.FromMilliseconds(16 / speed);
        }
    }

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode => _isSimplifiedMode;

    public void EnterSimplifiedMode()
    {
        _isSimplifiedMode = true;
        EnableAnimations = false;
        EnableParticleEffects = false;
        _animationTimer.Stop();
        _dataFlowTimer.Stop();
        CleanupAllParticles();
    }

    public void ExitSimplifiedMode()
    {
        _isSimplifiedMode = false;
        EnableAnimations = true;
        EnableParticleEffects = true;
        _animationTimer.Start();
        if (ShowDataFlow)
        {
            _dataFlowTimer.Start();
        }
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => true;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        AnimationSpeed = settings.AnimationSpeed;
        
        if (!settings.EnabledFeatures.HasFlag(AnimationFeatures.ParticleEffects))
        {
            EnableParticleEffects = false;
        }
        
        if (!settings.EnabledFeatures.HasFlag(AnimationFeatures.UIAnimations))
        {
            EnableAnimations = false;
        }

        if (settings.PerformanceMode == PerformanceMode.PowerSaver)
        {
            EnterSimplifiedMode();
        }
        else
        {
            ExitSimplifiedMode();
        }
    }

    #endregion

    #region Color Scheme Application

    private void ApplyEVEColorScheme()
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        
        // Update existing connections if any
        RefreshConnections();
    }

    #endregion
}

#region Supporting Classes and Enums

public class ModuleConnection
{
    public string Id { get; set; }
    public Point StartPoint { get; set; }
    public Point EndPoint { get; set; }
    public ConnectionType Type { get; set; }
    public double Strength { get; set; } = 1.0;
    public bool IsActive { get; set; } = true;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class SystemNode
{
    public string Id { get; set; }
    public Point Position { get; set; }
    public SystemType SystemType { get; set; }
    public double Size { get; set; } = 20;
    public bool IsActive { get; set; } = true;
    public Dictionary<string, object> Properties { get; set; } = new();
}

public class DNAHelix : IDisposable
{
    private readonly ModuleConnection _connection;
    private readonly Color _color;
    private readonly List<UIElement> _elements = new();
    private bool _disposed;

    public DNAHelix(ModuleConnection connection, Color color)
    {
        _connection = connection;
        _color = color;
    }

    public List<UIElement> CreateVisualization(DNAComplexity complexity)
    {
        var helixCount = complexity switch
        {
            DNAComplexity.Low => 1,
            DNAComplexity.Medium => 2,
            DNAComplexity.High => 3,
            _ => 2
        };

        for (int h = 0; h < helixCount; h++)
        {
            CreateHelixStrand(h * 120); // 120 degree offset for multiple strands
        }

        return _elements;
    }

    private void CreateHelixStrand(double angleOffset)
    {
        var distance = Vector.Subtract(_connection.EndPoint, _connection.StartPoint).Length;
        var pointCount = (int)(distance / 10);
        
        for (int i = 0; i < pointCount; i++)
        {
            var t = (double)i / pointCount;
            var angle = (Math.PI * 8 * t) + (angleOffset * Math.PI / 180);
            
            var baseX = _connection.StartPoint.X + (_connection.EndPoint.X - _connection.StartPoint.X) * t;
            var baseY = _connection.StartPoint.Y + (_connection.EndPoint.Y - _connection.StartPoint.Y) * t;
            
            var offsetX = Math.Cos(angle) * 10;
            var offsetY = Math.Sin(angle) * 5;
            
            var point = new Ellipse
            {
                Width = 3,
                Height = 3,
                Fill = new SolidColorBrush(_color),
                Opacity = 0.8
            };
            
            Canvas.SetLeft(point, baseX + offsetX);
            Canvas.SetTop(point, baseY + offsetY);
            
            _elements.Add(point);
        }
    }

    public void SetPosition(Point start, Point end)
    {
        _connection.StartPoint = start;
        _connection.EndPoint = end;
    }

    public void UpdateStrength(double strength)
    {
        foreach (var element in _elements)
        {
            element.Opacity = 0.8 * strength;
        }
    }

    public void UpdateAnimation(double phase)
    {
        for (int i = 0; i < _elements.Count; i++)
        {
            var intensity = (Math.Sin(phase + i * 0.2) + 1) / 2;
            _elements[i].Opacity = 0.8 * intensity;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _elements.Clear();
            _disposed = true;
        }
    }
}

public enum ConnectionType
{
    Power,
    Data,
    Shield,
    Weapon,
    Navigation,
    Sensor
}

public enum ConnectionStyle
{
    DigitalDNA,
    DataStream,
    Network,
    Quantum
}

public enum DNAComplexity
{
    Low,
    Medium,
    High
}

public enum DataFlowDirection
{
    Forward,
    Reverse,
    Bidirectional
}

public enum SystemType
{
    PowerCore,
    ShieldGenerator,
    WeaponSystem,
    PropulsionSystem,
    SensorArray,
    LifeSupport
}

public class ConnectionEventArgs : EventArgs
{
    public ModuleConnection Connection { get; set; }
    public DateTime Timestamp { get; set; }
}

public class DNAEventArgs : EventArgs
{
    public string DNAId { get; set; }
    public DNAComplexity Complexity { get; set; }
    public DateTime Timestamp { get; set; }
}

#endregion