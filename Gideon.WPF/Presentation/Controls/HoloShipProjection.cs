// ==========================================================================
// HoloShipProjection.cs - 3D Holographic Ship Projection System
// ==========================================================================
// Advanced 3D ship visualization featuring holographic ship projections,
// interactive rotation, module highlighting, and EVE-style ship rendering.
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
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using HelixToolkit.Wpf;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// 3D holographic ship projection system with interactive visualization and module highlighting
/// </summary>
public class HoloShipProjection : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloShipProjection),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloShipProjection),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty ShipDataProperty =
        DependencyProperty.Register(nameof(ShipData), typeof(HoloShipData), typeof(HoloShipProjection),
            new PropertyMetadata(null, OnShipDataChanged));

    public static readonly DependencyProperty ProjectionModeProperty =
        DependencyProperty.Register(nameof(ProjectionMode), typeof(ShipProjectionMode), typeof(HoloShipProjection),
            new PropertyMetadata(ShipProjectionMode.Holographic, OnProjectionModeChanged));

    public static readonly DependencyProperty EnableInteractionProperty =
        DependencyProperty.Register(nameof(EnableInteraction), typeof(bool), typeof(HoloShipProjection),
            new PropertyMetadata(true, OnEnableInteractionChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloShipProjection),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloShipProjection),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowModuleHighlightsProperty =
        DependencyProperty.Register(nameof(ShowModuleHighlights), typeof(bool), typeof(HoloShipProjection),
            new PropertyMetadata(true, OnShowModuleHighlightsChanged));

    public static readonly DependencyProperty ShowWireframeProperty =
        DependencyProperty.Register(nameof(ShowWireframe), typeof(bool), typeof(HoloShipProjection),
            new PropertyMetadata(false, OnShowWireframeChanged));

    public static readonly DependencyProperty AutoRotateProperty =
        DependencyProperty.Register(nameof(AutoRotate), typeof(bool), typeof(HoloShipProjection),
            new PropertyMetadata(false, OnAutoRotateChanged));

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

    public HoloShipData ShipData
    {
        get => (HoloShipData)GetValue(ShipDataProperty);
        set => SetValue(ShipDataProperty, value);
    }

    public ShipProjectionMode ProjectionMode
    {
        get => (ShipProjectionMode)GetValue(ProjectionModeProperty);
        set => SetValue(ProjectionModeProperty, value);
    }

    public bool EnableInteraction
    {
        get => (bool)GetValue(EnableInteractionProperty);
        set => SetValue(EnableInteractionProperty, value);
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

    public bool ShowModuleHighlights
    {
        get => (bool)GetValue(ShowModuleHighlightsProperty);
        set => SetValue(ShowModuleHighlightsProperty, value);
    }

    public bool ShowWireframe
    {
        get => (bool)GetValue(ShowWireframeProperty);
        set => SetValue(ShowWireframeProperty, value);
    }

    public bool AutoRotate
    {
        get => (bool)GetValue(AutoRotateProperty);
        set => SetValue(AutoRotateProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloShipEventArgs> ShipClicked;
    public event EventHandler<HoloShipEventArgs> ModuleSlotClicked;
    public event EventHandler<HoloShipEventArgs> ModuleSlotHovered;
    public event EventHandler<HoloShipEventArgs> ShipRotated;
    public event EventHandler<HoloShipEventArgs> ProjectionModeChanged;

    #endregion

    #region Private Fields

    private readonly DispatcherTimer _animationTimer;
    private readonly DispatcherTimer _rotationTimer;
    private Grid _mainContainer;
    private Viewport3D _viewport3D;
    private ModelVisual3D _shipModel;
    private ModelVisual3D _moduleHighlights;
    private ModelVisual3D _wireframeModel;
    private PerspectiveCamera _camera;
    private Model3DGroup _lightGroup;
    private Canvas _particleCanvas;
    private Canvas _overlayCanvas;
    private readonly List<UIElement> _hologramParticles = new();
    private readonly List<ModelVisual3D> _moduleModels = new();
    private readonly Dictionary<string, GeometryModel3D> _moduleSlots = new();
    private bool _isSimplifiedMode;
    private double _animationPhase;
    private double _rotationAngle;
    private bool _isMouseDown;
    private Point _lastMousePosition;
    private Vector3D _rotationAxis = new(0, 1, 0);
    private double _cameraDistance = 10;
    private double _cameraElevation = 0;
    private double _cameraAzimuth = 0;

    #endregion

    #region Constructor

    public HoloShipProjection()
    {
        InitializeComponent();
        InitializeTimers();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        MouseLeftButtonDown += OnMouseLeftButtonDown;
        MouseLeftButtonUp += OnMouseLeftButtonUp;
        MouseMove += OnMouseMove;
        MouseWheel += OnMouseWheel;
        KeyDown += OnKeyDown;
        
        // Make control focusable for keyboard input
        Focusable = true;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 400;
        Height = 300;
        Background = new SolidColorBrush(Colors.Transparent);

        CreateMainLayout();
        ApplyEVEColorScheme();
    }

    private void CreateMainLayout()
    {
        _mainContainer = new Grid();
        Content = _mainContainer;

        Create3DViewport();
        CreateParticleCanvas();
        CreateOverlayCanvas();
    }

    private void Create3DViewport()
    {
        _viewport3D = new Viewport3D
        {
            ClipToBounds = true
        };

        // Camera setup
        _camera = new PerspectiveCamera
        {
            Position = new Point3D(0, 0, _cameraDistance),
            LookDirection = new Vector3D(0, 0, -1),
            UpDirection = new Vector3D(0, 1, 0),
            FieldOfView = 45
        };
        _viewport3D.Camera = _camera;

        // Lighting setup
        SetupLighting();

        // Ship model container
        _shipModel = new ModelVisual3D();
        _viewport3D.Children.Add(_shipModel);

        // Module highlights container
        _moduleHighlights = new ModelVisual3D();
        _viewport3D.Children.Add(_moduleHighlights);

        // Wireframe container
        _wireframeModel = new ModelVisual3D
        {
            Content = new Model3DGroup()
        };
        _viewport3D.Children.Add(_wireframeModel);

        _mainContainer.Children.Add(_viewport3D);
    }

    private void SetupLighting()
    {
        _lightGroup = new Model3DGroup();

        // Ambient light
        var ambientLight = new AmbientLight(Color.FromRgb(40, 40, 40));
        _lightGroup.Children.Add(ambientLight);

        // Directional lights for ship detail
        var keyLight = new DirectionalLight
        {
            Color = Color.FromRgb(200, 200, 200),
            Direction = new Vector3D(-1, -1, -1)
        };
        _lightGroup.Children.Add(keyLight);

        var fillLight = new DirectionalLight
        {
            Color = Color.FromRgb(100, 100, 150),
            Direction = new Vector3D(1, 0, -1)
        };
        _lightGroup.Children.Add(fillLight);

        // Holographic rim light
        var rimLight = new DirectionalLight
        {
            Color = GetHolographicColor(),
            Direction = new Vector3D(0, 1, 1)
        };
        _lightGroup.Children.Add(rimLight);

        var lightModel = new ModelVisual3D
        {
            Content = _lightGroup
        };
        _viewport3D.Children.Add(lightModel);
    }

    private void CreateParticleCanvas()
    {
        _particleCanvas = new Canvas
        {
            Width = Width,
            Height = Height,
            IsHitTestVisible = false
        };
        _mainContainer.Children.Add(_particleCanvas);
    }

    private void CreateOverlayCanvas()
    {
        _overlayCanvas = new Canvas
        {
            Width = Width,
            Height = Height,
            IsHitTestVisible = false
        };
        _mainContainer.Children.Add(_overlayCanvas);
    }

    private void InitializeTimers()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };
        _animationTimer.Tick += OnAnimationTimerTick;

        _rotationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
        };
        _rotationTimer.Tick += OnRotationTimerTick;
    }

    #endregion

    #region Ship Management

    public async Task LoadShipAsync(HoloShipData shipData)
    {
        ShipData = shipData;
        await UpdateShipModelAsync();
    }

    public async Task UpdateShipModelAsync()
    {
        if (ShipData == null) return;

        ClearCurrentModel();

        // Load ship geometry
        await LoadShipGeometryAsync();

        // Load module slots
        LoadModuleSlots();

        // Apply holographic materials
        ApplyHolographicMaterials();

        // Update wireframe if enabled
        if (ShowWireframe)
        {
            UpdateWireframeModel();
        }

        // Start animations
        if (EnableAnimations && !_isSimplifiedMode)
        {
            StartShipAnimations();
        }
    }

    public void RotateShip(Vector3D axis, double angle)
    {
        if (_shipModel.Content is Model3DGroup shipGroup)
        {
            var rotation = new AxisAngleRotation3D(axis, angle);
            var rotateTransform = new RotateTransform3D(rotation);
            
            var transformGroup = new Transform3DGroup();
            if (shipGroup.Transform != null)
            {
                transformGroup.Children.Add(shipGroup.Transform);
            }
            transformGroup.Children.Add(rotateTransform);
            
            shipGroup.Transform = transformGroup;
        }

        ShipRotated?.Invoke(this, new HoloShipEventArgs
        {
            Ship = ShipData,
            RotationAxis = axis,
            RotationAngle = angle,
            Timestamp = DateTime.Now
        });
    }

    public void ZoomToShip(double zoomFactor = 1.0)
    {
        _cameraDistance = Math.Max(5, Math.Min(50, _cameraDistance / zoomFactor));
        UpdateCameraPosition();
    }

    public void ResetView()
    {
        _cameraDistance = 10;
        _cameraElevation = 0;
        _cameraAzimuth = 0;
        _rotationAngle = 0;
        UpdateCameraPosition();
        
        if (_shipModel.Content is Model3DGroup shipGroup)
        {
            shipGroup.Transform = null;
        }
    }

    public void HighlightModuleSlot(string slotId, bool highlight = true)
    {
        if (_moduleSlots.TryGetValue(slotId, out var slot))
        {
            if (highlight)
            {
                var highlightMaterial = new EmissiveMaterial(new SolidColorBrush(GetHolographicColor()));
                slot.BackMaterial = highlightMaterial;
                
                if (EnableParticleEffects && !_isSimplifiedMode)
                {
                    SpawnSlotHighlightParticles(slot);
                }
            }
            else
            {
                slot.BackMaterial = null;
            }
        }
    }

    #endregion

    #region 3D Model Loading

    private async Task LoadShipGeometryAsync()
    {
        // Create placeholder ship geometry for now
        // In a real implementation, this would load actual ship models
        var shipGeometry = CreateShipPlaceholder();
        
        var shipMaterial = CreateHolographicMaterial();
        var shipModel = new GeometryModel3D(shipGeometry, shipMaterial);

        if (ProjectionMode == ShipProjectionMode.Holographic)
        {
            ApplyHolographicEffects(shipModel);
        }

        var shipGroup = new Model3DGroup();
        shipGroup.Children.Add(shipModel);
        _shipModel.Content = shipGroup;

        if (EnableAnimations && !_isSimplifiedMode)
        {
            await AnimateShipLoad();
        }
    }

    private void LoadModuleSlots()
    {
        if (ShipData?.ModuleSlots == null) return;

        _moduleSlots.Clear();
        var moduleGroup = new Model3DGroup();

        foreach (var slot in ShipData.ModuleSlots)
        {
            var slotGeometry = CreateModuleSlotGeometry(slot);
            var slotMaterial = CreateSlotMaterial(slot);
            var slotModel = new GeometryModel3D(slotGeometry, slotMaterial);

            // Position the slot
            var transform = new TranslateTransform3D(slot.Position.X, slot.Position.Y, slot.Position.Z);
            slotModel.Transform = transform;

            _moduleSlots[slot.Id] = slotModel;
            moduleGroup.Children.Add(slotModel);
        }

        _moduleHighlights.Content = moduleGroup;
    }

    private MeshGeometry3D CreateShipPlaceholder()
    {
        var geometry = new MeshGeometry3D();

        // Create a basic ship-like geometry (elongated with tapered ends)
        var length = 4.0;
        var width = 1.5;
        var height = 0.8;

        // Hull vertices
        var positions = new Point3DCollection
        {
            // Front tip
            new Point3D(length/2, 0, 0),
            
            // Main body
            new Point3D(length/4, width/2, height/2),
            new Point3D(length/4, -width/2, height/2),
            new Point3D(length/4, width/2, -height/2),
            new Point3D(length/4, -width/2, -height/2),
            
            new Point3D(-length/4, width/2, height/2),
            new Point3D(-length/4, -width/2, height/2),
            new Point3D(-length/4, width/2, -height/2),
            new Point3D(-length/4, -width/2, -height/2),
            
            // Rear
            new Point3D(-length/2, 0, 0)
        };

        geometry.Positions = positions;

        // Create triangular faces
        var indices = new Int32Collection
        {
            // Front faces
            0, 1, 2,
            0, 2, 4,
            0, 4, 3,
            0, 3, 1,
            
            // Body faces
            1, 5, 6, 1, 6, 2,
            2, 6, 8, 2, 8, 4,
            4, 8, 7, 4, 7, 3,
            3, 7, 5, 3, 5, 1,
            
            // Rear faces
            9, 6, 5,
            9, 8, 6,
            9, 7, 8,
            9, 5, 7
        };

        geometry.TriangleIndices = indices;

        // Generate normals for proper lighting
        var normals = new Vector3DCollection();
        for (int i = 0; i < positions.Count; i++)
        {
            normals.Add(new Vector3D(0, 0, 1)); // Simplified normals
        }
        geometry.Normals = normals;

        return geometry;
    }

    private MeshGeometry3D CreateModuleSlotGeometry(HoloModuleSlot slot)
    {
        var geometry = new MeshGeometry3D();
        var size = 0.2;

        // Create a small cube for the module slot
        var positions = new Point3DCollection
        {
            // Front face
            new Point3D(-size, -size, size),
            new Point3D(size, -size, size),
            new Point3D(size, size, size),
            new Point3D(-size, size, size),
            
            // Back face
            new Point3D(-size, -size, -size),
            new Point3D(size, -size, -size),
            new Point3D(size, size, -size),
            new Point3D(-size, size, -size)
        };

        geometry.Positions = positions;

        var indices = new Int32Collection
        {
            // Front
            0, 1, 2, 0, 2, 3,
            // Back
            4, 7, 6, 4, 6, 5,
            // Left
            4, 0, 3, 4, 3, 7,
            // Right
            1, 5, 6, 1, 6, 2,
            // Top
            3, 2, 6, 3, 6, 7,
            // Bottom
            4, 5, 1, 4, 1, 0
        };

        geometry.TriangleIndices = indices;

        return geometry;
    }

    private Material CreateHolographicMaterial()
    {
        var color = GetHolographicColor();
        var materialGroup = new MaterialGroup();

        // Base material with transparency
        var diffuseMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(180, color.R, color.G, color.B)));
        materialGroup.Children.Add(diffuseMaterial);

        // Emissive material for glow
        var emissiveMaterial = new EmissiveMaterial(new SolidColorBrush(Color.FromArgb(100, color.R, color.G, color.B)));
        materialGroup.Children.Add(emissiveMaterial);

        // Specular highlights
        var specularMaterial = new SpecularMaterial(new SolidColorBrush(Colors.White), 50);
        materialGroup.Children.Add(specularMaterial);

        return materialGroup;
    }

    private Material CreateSlotMaterial(HoloModuleSlot slot)
    {
        var color = GetSlotColor(slot.Type);
        return new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(120, color.R, color.G, color.B)));
    }

    private void ApplyHolographicEffects(GeometryModel3D model)
    {
        if (ProjectionMode != ShipProjectionMode.Holographic) return;

        // Add holographic transparency and glow
        var material = model.Material as MaterialGroup ?? new MaterialGroup();
        
        if (EnableAnimations && !_isSimplifiedMode)
        {
            // Add animated opacity
            var brush = new SolidColorBrush(GetHolographicColor());
            var animation = new ColorAnimation
            {
                From = Color.FromArgb(120, brush.Color.R, brush.Color.G, brush.Color.B),
                To = Color.FromArgb(200, brush.Color.R, brush.Color.G, brush.Color.B),
                Duration = TimeSpan.FromSeconds(2),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };
            brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
            
            var emissiveMaterial = new EmissiveMaterial(brush);
            material.Children.Add(emissiveMaterial);
        }

        model.Material = material;
    }

    private void UpdateWireframeModel()
    {
        if (!ShowWireframe || _shipModel.Content is not Model3DGroup shipGroup) return;

        var wireframeGroup = new Model3DGroup();
        
        foreach (var child in shipGroup.Children.OfType<GeometryModel3D>())
        {
            if (child.Geometry is MeshGeometry3D mesh)
            {
                var wireframe = CreateWireframeFromMesh(mesh);
                wireframeGroup.Children.Add(wireframe);
            }
        }

        _wireframeModel.Content = wireframeGroup;
    }

    private GeometryModel3D CreateWireframeFromMesh(MeshGeometry3D mesh)
    {
        var wireframeGeometry = new MeshGeometry3D();
        
        // Create wireframe lines from triangle edges
        var linePositions = new Point3DCollection();
        var lineIndices = new Int32Collection();
        
        for (int i = 0; i < mesh.TriangleIndices.Count; i += 3)
        {
            var i1 = mesh.TriangleIndices[i];
            var i2 = mesh.TriangleIndices[i + 1];
            var i3 = mesh.TriangleIndices[i + 2];
            
            var p1 = mesh.Positions[i1];
            var p2 = mesh.Positions[i2];
            var p3 = mesh.Positions[i3];
            
            // Add triangle edges as lines
            var baseIndex = linePositions.Count;
            linePositions.Add(p1);
            linePositions.Add(p2);
            linePositions.Add(p3);
            
            // Create line segments
            lineIndices.Add(baseIndex);
            lineIndices.Add(baseIndex + 1);
            lineIndices.Add(baseIndex + 1);
            lineIndices.Add(baseIndex + 2);
            lineIndices.Add(baseIndex + 2);
            lineIndices.Add(baseIndex);
        }
        
        wireframeGeometry.Positions = linePositions;
        wireframeGeometry.TriangleIndices = lineIndices;
        
        var wireframeMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(100, 100, 200, 255)));
        return new GeometryModel3D(wireframeGeometry, wireframeMaterial);
    }

    #endregion

    #region Animation Methods

    private async Task AnimateShipLoad()
    {
        if (_isSimplifiedMode) return;

        // Fade in animation
        if (_shipModel.Content is Model3DGroup shipGroup)
        {
            var scaleTransform = new ScaleTransform3D(0.1, 0.1, 0.1);
            shipGroup.Transform = scaleTransform;

            var scaleAnimation = new DoubleAnimation
            {
                From = 0.1,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(1000),
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
            };

            scaleTransform.BeginAnimation(ScaleTransform3D.ScaleXProperty, scaleAnimation);
            scaleTransform.BeginAnimation(ScaleTransform3D.ScaleYProperty, scaleAnimation);
            scaleTransform.BeginAnimation(ScaleTransform3D.ScaleZProperty, scaleAnimation);

            await Task.Delay(1000);
        }
    }

    private void StartShipAnimations()
    {
        if (_isSimplifiedMode) return;

        _animationTimer.Start();
        
        if (AutoRotate)
        {
            _rotationTimer.Start();
        }
    }

    private void StopShipAnimations()
    {
        _animationTimer.Stop();
        _rotationTimer.Stop();
    }

    #endregion

    #region Particle Effects

    private void SpawnHologramParticles()
    {
        if (_isSimplifiedMode || !EnableParticleEffects) return;

        if (_random.NextDouble() < 0.15) // 15% chance
        {
            var particle = new Ellipse
            {
                Width = _random.Next(2, 4),
                Height = _random.Next(2, 4),
                Fill = new SolidColorBrush(Color.FromArgb(
                    (byte)_random.Next(100, 200),
                    GetHolographicColor().R, GetHolographicColor().G, GetHolographicColor().B))
            };

            var startX = _random.NextDouble() * Width;
            var startY = _random.NextDouble() * Height;

            Canvas.SetLeft(particle, startX);
            Canvas.SetTop(particle, startY);
            _particleCanvas.Children.Add(particle);
            _hologramParticles.Add(particle);

            AnimateHologramParticle(particle);
        }

        CleanupHologramParticles();
    }

    private void SpawnSlotHighlightParticles(GeometryModel3D slot)
    {
        if (_isSimplifiedMode || !EnableParticleEffects) return;

        // Project 3D slot position to 2D screen coordinates
        var slotPosition = slot.Transform?.Transform(new Point3D(0, 0, 0)) ?? new Point3D(0, 0, 0);
        var screenPoint = _viewport3D.Point3DtoPoint2D(slotPosition);

        if (screenPoint.HasValue)
        {
            for (int i = 0; i < 5; i++)
            {
                var particle = new Ellipse
                {
                    Width = 3,
                    Height = 3,
                    Fill = new SolidColorBrush(Color.FromArgb(200, 255, 255, 100))
                };

                Canvas.SetLeft(particle, screenPoint.Value.X);
                Canvas.SetTop(particle, screenPoint.Value.Y);
                _particleCanvas.Children.Add(particle);
                _hologramParticles.Add(particle);

                AnimateSlotParticle(particle);
            }
        }
    }

    private void AnimateHologramParticle(UIElement particle)
    {
        var angle = _random.NextDouble() * Math.PI * 2;
        var distance = _random.Next(20, 50);
        var targetX = Canvas.GetLeft(particle) + Math.Cos(angle) * distance;
        var targetY = Canvas.GetTop(particle) + Math.Sin(angle) * distance;

        var moveXAnimation = new DoubleAnimation
        {
            From = Canvas.GetLeft(particle),
            To = targetX,
            Duration = TimeSpan.FromMilliseconds(_random.Next(1500, 2500)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var moveYAnimation = new DoubleAnimation
        {
            From = Canvas.GetTop(particle),
            To = targetY,
            Duration = TimeSpan.FromMilliseconds(_random.Next(1500, 2500)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(_random.Next(1500, 2500))
        };

        moveXAnimation.Completed += (s, e) =>
        {
            _particleCanvas.Children.Remove(particle);
            _hologramParticles.Remove(particle);
        };

        particle.BeginAnimation(Canvas.LeftProperty, moveXAnimation);
        particle.BeginAnimation(Canvas.TopProperty, moveYAnimation);
        particle.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void AnimateSlotParticle(UIElement particle)
    {
        var targetX = Canvas.GetLeft(particle) + _random.Next(-20, 20);
        var targetY = Canvas.GetTop(particle) + _random.Next(-20, 20);

        var moveAnimation = new DoubleAnimation
        {
            From = Canvas.GetLeft(particle),
            To = targetX,
            Duration = TimeSpan.FromMilliseconds(800),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(800)
        };

        moveAnimation.Completed += (s, e) =>
        {
            _particleCanvas.Children.Remove(particle);
            _hologramParticles.Remove(particle);
        };

        particle.BeginAnimation(Canvas.LeftProperty, moveAnimation);
        particle.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void CleanupHologramParticles()
    {
        if (_hologramParticles.Count > 30)
        {
            var particlesToRemove = _hologramParticles.Take(15).ToList();
            foreach (var particle in particlesToRemove)
            {
                _particleCanvas.Children.Remove(particle);
                _hologramParticles.Remove(particle);
            }
        }
    }

    #endregion

    #region Helper Methods

    private readonly Random _random = new();

    private Color GetHolographicColor()
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        return colors.Primary;
    }

    private Color GetSlotColor(ModuleSlotType slotType)
    {
        return slotType switch
        {
            ModuleSlotType.High => Color.FromRgb(255, 100, 100),
            ModuleSlotType.Medium => Color.FromRgb(255, 255, 100),
            ModuleSlotType.Low => Color.FromRgb(100, 255, 100),
            ModuleSlotType.Rig => Color.FromRgb(100, 100, 255),
            ModuleSlotType.Subsystem => Color.FromRgb(255, 100, 255),
            _ => Color.FromRgb(150, 150, 150)
        };
    }

    private void UpdateCameraPosition()
    {
        var x = _cameraDistance * Math.Sin(_cameraAzimuth) * Math.Cos(_cameraElevation);
        var y = _cameraDistance * Math.Sin(_cameraElevation);
        var z = _cameraDistance * Math.Cos(_cameraAzimuth) * Math.Cos(_cameraElevation);

        _camera.Position = new Point3D(x, y, z);
        _camera.LookDirection = new Vector3D(-x, -y, -z);
    }

    private void ClearCurrentModel()
    {
        _shipModel.Content = null;
        _moduleHighlights.Content = null;
        _wireframeModel.Content = new Model3DGroup();
        _moduleSlots.Clear();
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (ShipData != null)
        {
            _ = UpdateShipModelAsync();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopShipAnimations();
        CleanupAllParticles();
    }

    private void OnAnimationTimerTick(object sender, EventArgs e)
    {
        if (EnableAnimations && !_isSimplifiedMode)
        {
            _animationPhase += 0.1;
            if (_animationPhase > Math.PI * 2)
                _animationPhase = 0;

            UpdateAnimatedEffects();
            SpawnHologramParticles();
        }
    }

    private void OnRotationTimerTick(object sender, EventArgs e)
    {
        if (AutoRotate && !_isMouseDown)
        {
            _rotationAngle += 1;
            if (_rotationAngle >= 360)
                _rotationAngle = 0;

            RotateShip(new Vector3D(0, 1, 0), 1);
        }
    }

    private void UpdateAnimatedEffects()
    {
        // Update holographic intensity based on animation phase
        var intensity = 0.7 + (Math.Sin(_animationPhase * 2) * 0.3);
        
        if (_lightGroup?.Children.Count > 3)
        {
            var rimLight = _lightGroup.Children[3] as DirectionalLight;
            if (rimLight != null)
            {
                var color = GetHolographicColor();
                rimLight.Color = Color.FromRgb(
                    (byte)(color.R * intensity),
                    (byte)(color.G * intensity),
                    (byte)(color.B * intensity));
            }
        }
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!EnableInteraction) return;

        _isMouseDown = true;
        _lastMousePosition = e.GetPosition(this);
        CaptureMouse();
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!EnableInteraction) return;

        _isMouseDown = false;
        ReleaseMouseCapture();

        // Check for ship or module slot clicks
        var hitResult = GetHitResult(e.GetPosition(_viewport3D));
        if (hitResult != null)
        {
            HandleModelClick(hitResult);
        }
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!EnableInteraction || !_isMouseDown) return;

        var currentPosition = e.GetPosition(this);
        var deltaX = currentPosition.X - _lastMousePosition.X;
        var deltaY = currentPosition.Y - _lastMousePosition.Y;

        // Determine interaction mode based on modifiers
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
        {
            // Orbit camera around ship
            var azimuthDelta = deltaX * 0.3;
            var elevationDelta = deltaY * 0.3;
            OrbitCamera(azimuthDelta, elevationDelta);
        }
        else
        {
            // Rotate ship directly
            if (Math.Abs(deltaX) > Math.Abs(deltaY))
            {
                RotateShip(new Vector3D(0, 1, 0), deltaX * 0.5);
            }
            else
            {
                RotateShip(new Vector3D(1, 0, 0), -deltaY * 0.5);
            }
        }

        _lastMousePosition = currentPosition;
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!EnableInteraction) return;

        var zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
        ZoomToShip(zoomFactor);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (!EnableInteraction) return;

        switch (e.Key)
        {
            case Key.R:
                // Reset ship orientation and camera
                ResetShipOrientation();
                e.Handled = true;
                break;

            case Key.W:
                // Toggle wireframe mode
                ShowWireframe = !ShowWireframe;
                e.Handled = true;
                break;

            case Key.H:
                // Toggle module highlights
                ShowModuleHighlights = !ShowModuleHighlights;
                e.Handled = true;
                break;

            case Key.Space:
                // Toggle auto-rotation
                AutoRotate = !AutoRotate;
                e.Handled = true;
                break;

            case Key.F:
                // Fit ship to view (reset zoom)
                ZoomToShip(1.0);
                e.Handled = true;
                break;

            case Key.Up:
                // Orbit camera up
                OrbitCamera(0, -10);
                e.Handled = true;
                break;

            case Key.Down:
                // Orbit camera down
                OrbitCamera(0, 10);
                e.Handled = true;
                break;

            case Key.Left:
                // Orbit camera left
                OrbitCamera(-10, 0);
                e.Handled = true;
                break;

            case Key.Right:
                // Orbit camera right
                OrbitCamera(10, 0);
                e.Handled = true;
                break;

            case Key.NumPad1:
                // Front view
                SetStandardView(StandardView.Front);
                e.Handled = true;
                break;

            case Key.NumPad3:
                // Right view
                SetStandardView(StandardView.Right);
                e.Handled = true;
                break;

            case Key.NumPad7:
                // Top view
                SetStandardView(StandardView.Top);
                e.Handled = true;
                break;

            case Key.NumPad9:
                // Isometric view
                SetStandardView(StandardView.Isometric);
                e.Handled = true;
                break;
        }
    }

    private RayMeshGeometry3DHitTestResult GetHitResult(Point position)
    {
        var hitParams = new PointHitTestParameters(position);
        RayMeshGeometry3DHitTestResult hitResult = null;
        
        VisualTreeHelper.HitTest(_viewport3D, null, 
            result =>
            {
                if (result is RayMeshGeometry3DHitTestResult meshResult)
                {
                    hitResult = meshResult;
                    return HitTestResultBehavior.Stop;
                }
                return HitTestResultBehavior.Continue;
            }, hitParams);
            
        return hitResult;
    }

    private void HandleModelClick(RayMeshGeometry3DHitTestResult hitResult)
    {
        // Check if it's a module slot
        var clickedSlot = _moduleSlots.FirstOrDefault(kvp => kvp.Value == hitResult.ModelHit);
        if (!clickedSlot.Equals(default(KeyValuePair<string, GeometryModel3D>)))
        {
            ModuleSlotClicked?.Invoke(this, new HoloShipEventArgs
            {
                Ship = ShipData,
                ModuleSlotId = clickedSlot.Key,
                Timestamp = DateTime.Now
            });
            return;
        }

        // Otherwise it's the ship itself
        ShipClicked?.Invoke(this, new HoloShipEventArgs
        {
            Ship = ShipData,
            Timestamp = DateTime.Now
        });
    }

    private void CleanupAllParticles()
    {
        foreach (var particle in _hologramParticles.ToList())
        {
            _particleCanvas.Children.Remove(particle);
        }
        _hologramParticles.Clear();
    }

    #endregion

    #region Property Change Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipProjection projection)
        {
            projection.ApplyHolographicIntensity();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipProjection projection)
        {
            projection.ApplyEVEColorScheme();
        }
    }

    private static void OnShipDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipProjection projection)
        {
            _ = projection.UpdateShipModelAsync();
        }
    }

    private static void OnProjectionModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipProjection projection)
        {
            _ = projection.UpdateShipModelAsync();
            projection.ProjectionModeChanged?.Invoke(projection, new HoloShipEventArgs
            {
                Ship = projection.ShipData,
                Timestamp = DateTime.Now
            });
        }
    }

    private static void OnEnableInteractionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipProjection projection)
        {
            projection._viewport3D.IsHitTestVisible = (bool)e.NewValue;
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipProjection projection)
        {
            if ((bool)e.NewValue && !projection._isSimplifiedMode)
            {
                projection.StartShipAnimations();
            }
            else
            {
                projection.StopShipAnimations();
            }
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipProjection projection && !(bool)e.NewValue)
        {
            projection.CleanupAllParticles();
        }
    }

    private static void OnShowModuleHighlightsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipProjection projection)
        {
            projection._moduleHighlights.Content = (bool)e.NewValue ? projection._moduleHighlights.Content : null;
        }
    }

    private static void OnShowWireframeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipProjection projection)
        {
            if ((bool)e.NewValue)
            {
                projection.UpdateWireframeModel();
            }
            else
            {
                projection._wireframeModel.Content = new Model3DGroup();
            }
        }
    }

    private static void OnAutoRotateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipProjection projection)
        {
            if ((bool)e.NewValue && projection.EnableAnimations && !projection._isSimplifiedMode)
            {
                projection._rotationTimer.Start();
            }
            else
            {
                projection._rotationTimer.Stop();
            }
        }
    }

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode => _isSimplifiedMode;

    public void EnterSimplifiedMode()
    {
        _isSimplifiedMode = true;
        CleanupAllParticles();
        EnableAnimations = false;
        EnableParticleEffects = false;
        AutoRotate = false;
        StopShipAnimations();
    }

    public void ExitSimplifiedMode()
    {
        _isSimplifiedMode = false;
        EnableAnimations = true;
        EnableParticleEffects = true;
        StartShipAnimations();
    }

    #endregion

    #region Public Methods

    public void RotateShip(Vector3D axis, double angle)
    {
        if (_shipModel.Content is not Model3DGroup shipGroup) return;

        // Create rotation transform
        var rotation = new AxisAngleRotation3D(axis, angle);
        var rotateTransform = new RotateTransform3D(rotation);

        // Apply transformation to ship group
        if (shipGroup.Transform is Transform3DGroup transformGroup)
        {
            transformGroup.Children.Add(rotateTransform);
        }
        else
        {
            var newTransformGroup = new Transform3DGroup();
            if (shipGroup.Transform != null)
            {
                newTransformGroup.Children.Add(shipGroup.Transform);
            }
            newTransformGroup.Children.Add(rotateTransform);
            shipGroup.Transform = newTransformGroup;
        }

        // Apply same rotation to wireframe and highlights
        if (ShowWireframe && _wireframeModel.Content is Model3DGroup wireframeGroup)
        {
            var wireframeRotate = new RotateTransform3D(rotation);
            if (wireframeGroup.Transform is Transform3DGroup wireframeTransformGroup)
            {
                wireframeTransformGroup.Children.Add(wireframeRotate);
            }
            else
            {
                var newWireframeTransformGroup = new Transform3DGroup();
                if (wireframeGroup.Transform != null)
                {
                    newWireframeTransformGroup.Children.Add(wireframeGroup.Transform);
                }
                newWireframeTransformGroup.Children.Add(wireframeRotate);
                wireframeGroup.Transform = newWireframeTransformGroup;
            }
        }

        if (ShowModuleHighlights && _moduleHighlights.Content is Model3DGroup highlightGroup)
        {
            var highlightRotate = new RotateTransform3D(rotation);
            if (highlightGroup.Transform is Transform3DGroup highlightTransformGroup)
            {
                highlightTransformGroup.Children.Add(highlightRotate);
            }
            else
            {
                var newHighlightTransformGroup = new Transform3DGroup();
                if (highlightGroup.Transform != null)
                {
                    newHighlightTransformGroup.Children.Add(highlightGroup.Transform);
                }
                newHighlightTransformGroup.Children.Add(highlightRotate);
                highlightGroup.Transform = newHighlightTransformGroup;
            }
        }

        // Fire rotation event
        ShipRotated?.Invoke(this, new HoloShipEventArgs
        {
            Ship = ShipData,
            Timestamp = DateTime.Now
        });
    }

    public void ZoomToShip(double zoomFactor)
    {
        var newDistance = _cameraDistance * zoomFactor;
        
        // Constrain zoom limits
        newDistance = Math.Max(2, Math.Min(50, newDistance));
        
        if (Math.Abs(newDistance - _cameraDistance) < 0.01) return;
        
        _cameraDistance = newDistance;
        
        // Animate zoom for smooth transition
        if (EnableAnimations && !_isSimplifiedMode)
        {
            AnimateZoom(newDistance);
        }
        else
        {
            UpdateCameraPosition();
        }
    }

    public void ResetShipOrientation()
    {
        if (_shipModel.Content is Model3DGroup shipGroup)
        {
            shipGroup.Transform = null;
        }
        
        if (_wireframeModel.Content is Model3DGroup wireframeGroup)
        {
            wireframeGroup.Transform = null;
        }
        
        if (_moduleHighlights.Content is Model3DGroup highlightGroup)
        {
            highlightGroup.Transform = null;
        }

        _cameraAzimuth = 0;
        _cameraElevation = 0;
        _cameraDistance = 10;
        
        if (EnableAnimations && !_isSimplifiedMode)
        {
            AnimateCameraReset();
        }
        else
        {
            UpdateCameraPosition();
        }
    }

    public void SetCameraTarget(Point3D target)
    {
        _camera.LookDirection = new Vector3D(
            target.X - _camera.Position.X,
            target.Y - _camera.Position.Y,
            target.Z - _camera.Position.Z);
    }

    public void OrbitCamera(double azimuthDelta, double elevationDelta)
    {
        _cameraAzimuth += azimuthDelta * (Math.PI / 180);
        _cameraElevation += elevationDelta * (Math.PI / 180);
        
        // Constrain elevation to prevent camera flip
        _cameraElevation = Math.Max(-Math.PI / 2 + 0.1, Math.Min(Math.PI / 2 - 0.1, _cameraElevation));
        
        if (EnableAnimations && !_isSimplifiedMode)
        {
            AnimateCameraOrbit();
        }
        else
        {
            UpdateCameraPosition();
        }
    }

    public async Task LoadShipAsync(HoloShipData shipData)
    {
        ShipData = shipData;
        await UpdateShipModelAsync();
    }

    public void HighlightModuleSlot(string slotId, bool highlight = true)
    {
        if (_moduleSlots.TryGetValue(slotId, out var slotModel))
        {
            if (highlight)
            {
                // Create highlighting material
                var highlightColor = GetHolographicColor();
                var highlightMaterial = new EmissiveMaterial(new SolidColorBrush(Color.FromArgb(
                    200, highlightColor.R, highlightColor.G, highlightColor.B)));
                
                if (slotModel.Material is MaterialGroup materialGroup)
                {
                    // Remove existing highlight if present
                    var existingHighlight = materialGroup.Children.OfType<EmissiveMaterial>()
                        .FirstOrDefault(m => ((SolidColorBrush)m.Brush).Color.A == 200);
                    if (existingHighlight != null)
                    {
                        materialGroup.Children.Remove(existingHighlight);
                    }
                    materialGroup.Children.Add(highlightMaterial);
                }
                else
                {
                    var newMaterialGroup = new MaterialGroup();
                    if (slotModel.Material != null)
                    {
                        newMaterialGroup.Children.Add(slotModel.Material);
                    }
                    newMaterialGroup.Children.Add(highlightMaterial);
                    slotModel.Material = newMaterialGroup;
                }

                // Spawn highlight particles
                if (EnableParticleEffects && !_isSimplifiedMode)
                {
                    SpawnSlotHighlightParticles(slotId);
                }
            }
            else
            {
                // Remove highlighting
                if (slotModel.Material is MaterialGroup materialGroup)
                {
                    var highlightToRemove = materialGroup.Children.OfType<EmissiveMaterial>()
                        .FirstOrDefault(m => ((SolidColorBrush)m.Brush).Color.A == 200);
                    if (highlightToRemove != null)
                    {
                        materialGroup.Children.Remove(highlightToRemove);
                    }
                }
            }
        }
    }

    public void SetStandardView(StandardView view)
    {
        switch (view)
        {
            case StandardView.Front:
                _cameraAzimuth = 0;
                _cameraElevation = 0;
                _cameraDistance = 10;
                break;

            case StandardView.Right:
                _cameraAzimuth = Math.PI / 2;
                _cameraElevation = 0;
                _cameraDistance = 10;
                break;

            case StandardView.Top:
                _cameraAzimuth = 0;
                _cameraElevation = Math.PI / 2;
                _cameraDistance = 10;
                break;

            case StandardView.Isometric:
                _cameraAzimuth = Math.PI / 4;
                _cameraElevation = Math.PI / 6;
                _cameraDistance = 12;
                break;

            case StandardView.Back:
                _cameraAzimuth = Math.PI;
                _cameraElevation = 0;
                _cameraDistance = 10;
                break;

            case StandardView.Left:
                _cameraAzimuth = -Math.PI / 2;
                _cameraElevation = 0;
                _cameraDistance = 10;
                break;

            case StandardView.Bottom:
                _cameraAzimuth = 0;
                _cameraElevation = -Math.PI / 2;
                _cameraDistance = 10;
                break;
        }

        if (EnableAnimations && !_isSimplifiedMode)
        {
            AnimateCameraReset();
        }
        else
        {
            UpdateCameraPosition();
        }
    }

    #endregion

    #region Animation Methods

    private void AnimateZoom(double targetDistance)
    {
        var animation = new DoubleAnimation
        {
            From = _cameraDistance,
            To = targetDistance,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        animation.CurrentTimeInvalidated += (s, e) =>
        {
            if (animation.GetCurrentProgress() is double progress)
            {
                _cameraDistance = animation.From.Value + (animation.To.Value - animation.From.Value) * progress;
                UpdateCameraPosition();
            }
        };

        this.BeginAnimation(WidthProperty, animation);
    }

    private void AnimateCameraReset()
    {
        var azimuthAnimation = new DoubleAnimation
        {
            From = _cameraAzimuth,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(500),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var elevationAnimation = new DoubleAnimation
        {
            From = _cameraElevation,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(500),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var distanceAnimation = new DoubleAnimation
        {
            From = _cameraDistance,
            To = 10,
            Duration = TimeSpan.FromMilliseconds(500),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var animationCompleted = 0;
        EventHandler onCompleted = (s, e) =>
        {
            animationCompleted++;
            if (animationCompleted == 3)
            {
                _cameraAzimuth = 0;
                _cameraElevation = 0;
                _cameraDistance = 10;
                UpdateCameraPosition();
            }
        };

        azimuthAnimation.Completed += onCompleted;
        elevationAnimation.Completed += onCompleted;
        distanceAnimation.Completed += onCompleted;

        azimuthAnimation.CurrentTimeInvalidated += (s, e) =>
        {
            if (azimuthAnimation.GetCurrentProgress() is double progress)
            {
                _cameraAzimuth = azimuthAnimation.From.Value + (azimuthAnimation.To.Value - azimuthAnimation.From.Value) * progress;
                UpdateCameraPosition();
            }
        };

        this.BeginAnimation(WidthProperty, azimuthAnimation);
    }

    private void AnimateCameraOrbit()
    {
        // Smooth camera orbital movement
        UpdateCameraPosition();
    }

    private void SpawnSlotHighlightParticles(string slotId)
    {
        if (!_moduleSlots.TryGetValue(slotId, out var slotModel)) return;

        // Get slot position in 3D space
        var slotPosition = new Point3D(0, 0, 0); // This should be calculated from the slot's actual position
        
        // Convert to screen coordinates
        var screenPoint = _viewport3D.Point3DtoPoint2D(slotPosition);
        
        if (screenPoint.HasValue)
        {
            for (int i = 0; i < 8; i++)
            {
                var particle = new Ellipse
                {
                    Width = 4,
                    Height = 4,
                    Fill = new SolidColorBrush(GetHolographicColor())
                };

                Canvas.SetLeft(particle, screenPoint.Value.X);
                Canvas.SetTop(particle, screenPoint.Value.Y);
                _particleCanvas.Children.Add(particle);
                _hologramParticles.Add(particle);

                AnimateSlotParticle(particle);
            }
        }
    }

    #endregion

    #region Color Scheme Application

    private void ApplyEVEColorScheme()
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        
        // Update lighting
        if (_lightGroup?.Children.Count > 3)
        {
            var rimLight = _lightGroup.Children[3] as DirectionalLight;
            if (rimLight != null)
            {
                rimLight.Color = colors.Primary;
            }
        }

        // Update materials if ship is loaded
        if (_shipModel.Content != null)
        {
            _ = UpdateShipModelAsync();
        }
    }

    private void ApplyHolographicIntensity()
    {
        var intensity = HolographicIntensity;
        
        EnableAnimations = intensity > 0.3;
        EnableParticleEffects = intensity > 0.5;

        // Update lighting intensity
        if (_lightGroup != null)
        {
            foreach (var light in _lightGroup.Children.OfType<Light>())
            {
                if (light is DirectionalLight dirLight)
                {
                    var color = dirLight.Color;
                    dirLight.Color = Color.FromRgb(
                        (byte)(color.R * intensity),
                        (byte)(color.G * intensity),
                        (byte)(color.B * intensity));
                }
            }
        }
    }

    #endregion
}

#region Supporting Classes and Enums

public enum ShipProjectionMode
{
    Solid,
    Holographic,
    Wireframe,
    XRay
}

public enum ModuleSlotType
{
    High,
    Medium,
    Low,
    Rig,
    Subsystem
}

public class HoloShipData
{
    public string ShipName { get; set; }
    public string ShipType { get; set; }
    public string ModelPath { get; set; }
    public List<HoloModuleSlot> ModuleSlots { get; set; } = new();
    public Dictionary<string, object> ShipStats { get; set; } = new();
    public Vector3D Dimensions { get; set; }
    public string TexturePath { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

public class HoloModuleSlot
{
    public string Id { get; set; }
    public ModuleSlotType Type { get; set; }
    public Vector3D Position { get; set; }
    public Vector3D Rotation { get; set; }
    public string ModuleId { get; set; }
    public bool IsOccupied { get; set; }
    public bool IsHighlighted { get; set; }
    public Dictionary<string, object> SlotData { get; set; } = new();
}

public class HoloShipEventArgs : EventArgs
{
    public HoloShipData Ship { get; set; }
    public string ModuleSlotId { get; set; }
    public Vector3D RotationAxis { get; set; }
    public double RotationAngle { get; set; }
    public DateTime Timestamp { get; set; }
}

public enum StandardView
{
    Front,
    Back,
    Left,
    Right,
    Top,
    Bottom,
    Isometric
}

#endregion