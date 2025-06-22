// ==========================================================================
// HoloCharacterSelector.cs - Character Selection with 3D Holographic Projections
// ==========================================================================
// Advanced holographic character selection interface featuring 3D projections,
// EVE character data integration, and Westworld-style visual effects.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using HelixToolkit.Wpf;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Holographic character selection interface with 3D projections and EVE integration
/// </summary>
public class HoloCharacterSelector : Control, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloCharacterSelector),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloCharacterSelector),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty CharactersProperty =
        DependencyProperty.Register(nameof(Characters), typeof(ObservableCollection<HoloCharacterInfo>), typeof(HoloCharacterSelector),
            new PropertyMetadata(null, OnCharactersChanged));

    public static readonly DependencyProperty SelectedCharacterProperty =
        DependencyProperty.Register(nameof(SelectedCharacter), typeof(HoloCharacterInfo), typeof(HoloCharacterSelector),
            new PropertyMetadata(null, OnSelectedCharacterChanged));

    public static readonly DependencyProperty Enable3DProjectionProperty =
        DependencyProperty.Register(nameof(Enable3DProjection), typeof(bool), typeof(HoloCharacterSelector),
            new PropertyMetadata(true, OnEnable3DProjectionChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloCharacterSelector),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty AutoRotateProjectionProperty =
        DependencyProperty.Register(nameof(AutoRotateProjection), typeof(bool), typeof(HoloCharacterSelector),
            new PropertyMetadata(true));

    public static readonly DependencyProperty ShowCharacterDetailsProperty =
        DependencyProperty.Register(nameof(ShowCharacterDetails), typeof(bool), typeof(HoloCharacterSelector),
            new PropertyMetadata(true));

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

    public ObservableCollection<HoloCharacterInfo> Characters
    {
        get => (ObservableCollection<HoloCharacterInfo>)GetValue(CharactersProperty);
        set => SetValue(CharactersProperty, value);
    }

    public HoloCharacterInfo SelectedCharacter
    {
        get => (HoloCharacterInfo)GetValue(SelectedCharacterProperty);
        set => SetValue(SelectedCharacterProperty, value);
    }

    public bool Enable3DProjection
    {
        get => (bool)GetValue(Enable3DProjectionProperty);
        set => SetValue(Enable3DProjectionProperty, value);
    }

    public bool EnableParticleEffects
    {
        get => (bool)GetValue(EnableParticleEffectsProperty);
        set => SetValue(EnableParticleEffectsProperty, value);
    }

    public bool AutoRotateProjection
    {
        get => (bool)GetValue(AutoRotateProjectionProperty);
        set => SetValue(AutoRotateProjectionProperty, value);
    }

    public bool ShowCharacterDetails
    {
        get => (bool)GetValue(ShowCharacterDetailsProperty);
        set => SetValue(ShowCharacterDetailsProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloCharacterEventArgs> CharacterSelected;
    public event EventHandler<HoloCharacterEventArgs> CharacterHovered;
    public event EventHandler<HoloCharacterEventArgs> CharacterDoubleClicked;

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode { get; private set; }

    public void SwitchToFullMode()
    {
        IsInSimplifiedMode = false;
        Enable3DProjection = true;
        EnableParticleEffects = true;
        UpdateSelectorAppearance();
    }

    public void SwitchToSimplifiedMode()
    {
        IsInSimplifiedMode = true;
        Enable3DProjection = false;
        EnableParticleEffects = false;
        UpdateSelectorAppearance();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => !_disposed && IsLoaded;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        Enable3DProjection = settings.EnabledFeatures.HasFlag(AnimationFeatures.AdvancedShaders);
        EnableParticleEffects = settings.EnabledFeatures.HasFlag(AnimationFeatures.ParticleEffects);
        
        UpdateSelectorAppearance();
    }

    #endregion

    #region Fields

    private Grid _selectorGrid;
    private ListBox _characterList;
    private Border _projectionContainer;
    private Viewport3D _viewport3D;
    private ModelVisual3D _characterModel;
    private PerspectiveCamera _camera;
    private Border _detailsPanel;
    private Canvas _particleCanvas;
    private Canvas _effectCanvas;
    
    private readonly Dictionary<HoloCharacterInfo, CharacterItemControl> _characterControls = new();
    private readonly List<CharacterParticle> _particles = new();
    private DispatcherTimer _particleTimer;
    private DispatcherTimer _rotationTimer;
    private DispatcherTimer _scanlineTimer;
    private double _particlePhase = 0;
    private double _rotationAngle = 0;
    private double _scanlinePhase = 0;
    private readonly Random _random = new();
    private bool _disposed = false;

    #endregion

    #region Constructor

    public HoloCharacterSelector()
    {
        DefaultStyleKey = typeof(HoloCharacterSelector);
        Characters = new ObservableCollection<HoloCharacterInfo>();
        Width = 800;
        Height = 600;
        InitializeSelector();
        SetupAnimations();
        
        // Register with animation intensity manager
        AnimationIntensityManager.Instance.RegisterTarget(this);
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Add a character to the selection list
    /// </summary>
    public void AddCharacter(HoloCharacterInfo character)
    {
        if (character != null && !Characters.Contains(character))
        {
            Characters.Add(character);
        }
    }

    /// <summary>
    /// Remove a character from the selection list
    /// </summary>
    public void RemoveCharacter(HoloCharacterInfo character)
    {
        if (character != null)
        {
            Characters.Remove(character);
        }
    }

    /// <summary>
    /// Select a specific character
    /// </summary>
    public void SelectCharacter(HoloCharacterInfo character)
    {
        if (character != null && Characters.Contains(character))
        {
            SelectedCharacter = character;
        }
    }

    /// <summary>
    /// Refresh character data
    /// </summary>
    public void RefreshCharacters()
    {
        UpdateCharacterList();
        Update3DProjection();
    }

    #endregion

    #region Private Methods

    private void InitializeSelector()
    {
        Template = CreateSelectorTemplate();
        UpdateSelectorAppearance();
    }

    private ControlTemplate CreateSelectorTemplate()
    {
        var template = new ControlTemplate(typeof(HoloCharacterSelector));

        // Main selector grid
        var selectorGrid = new FrameworkElementFactory(typeof(Grid));
        selectorGrid.Name = "PART_SelectorGrid";

        // Column definitions
        var listColumn = new FrameworkElementFactory(typeof(ColumnDefinition));
        listColumn.SetValue(ColumnDefinition.WidthProperty, new GridLength(300));
        var projectionColumn = new FrameworkElementFactory(typeof(ColumnDefinition));
        projectionColumn.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
        var detailsColumn = new FrameworkElementFactory(typeof(ColumnDefinition));
        detailsColumn.SetValue(ColumnDefinition.WidthProperty, new GridLength(250));

        selectorGrid.AppendChild(listColumn);
        selectorGrid.AppendChild(projectionColumn);
        selectorGrid.AppendChild(detailsColumn);

        // Character list panel
        var listPanel = new FrameworkElementFactory(typeof(Border));
        listPanel.SetValue(Border.CornerRadiusProperty, new CornerRadius(8, 0, 0, 8));
        listPanel.SetValue(Border.BorderThicknessProperty, new Thickness(1, 1, 0, 1));
        listPanel.SetValue(Border.MarginProperty, new Thickness(8));
        listPanel.SetValue(Grid.ColumnProperty, 0);

        // Character list
        var characterList = new FrameworkElementFactory(typeof(ListBox));
        characterList.Name = "PART_CharacterList";
        characterList.SetValue(ListBox.BackgroundProperty, Brushes.Transparent);
        characterList.SetValue(ListBox.BorderThicknessProperty, new Thickness(0));
        characterList.SetValue(ListBox.PaddingProperty, new Thickness(8));

        listPanel.AppendChild(characterList);

        // 3D Projection container
        var projectionContainer = new FrameworkElementFactory(typeof(Border));
        projectionContainer.Name = "PART_ProjectionContainer";
        projectionContainer.SetValue(Border.CornerRadiusProperty, new CornerRadius(0));
        projectionContainer.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 1));
        projectionContainer.SetValue(Border.MarginProperty, new Thickness(0, 8, 0, 8));
        projectionContainer.SetValue(Grid.ColumnProperty, 1);

        // 3D Viewport
        var viewport3D = new FrameworkElementFactory(typeof(Viewport3D));
        viewport3D.Name = "PART_Viewport3D";
        viewport3D.SetValue(Viewport3D.ClipToBoundsProperty, true);

        projectionContainer.AppendChild(viewport3D);

        // Details panel
        var detailsPanel = new FrameworkElementFactory(typeof(Border));
        detailsPanel.Name = "PART_DetailsPanel";
        detailsPanel.SetValue(Border.CornerRadiusProperty, new CornerRadius(0, 8, 8, 0));
        detailsPanel.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 1, 1));
        detailsPanel.SetValue(Border.MarginProperty, new Thickness(8));
        detailsPanel.SetValue(Border.PaddingProperty, new Thickness(16));
        detailsPanel.SetValue(Grid.ColumnProperty, 2);

        // Particle canvas
        var particleCanvas = new FrameworkElementFactory(typeof(Canvas));
        particleCanvas.Name = "PART_ParticleCanvas";
        particleCanvas.SetValue(Canvas.IsHitTestVisibleProperty, false);
        particleCanvas.SetValue(Canvas.ClipToBoundsProperty, true);
        particleCanvas.SetValue(Grid.ColumnSpanProperty, 3);

        // Effect canvas for scanlines and other effects
        var effectCanvas = new FrameworkElementFactory(typeof(Canvas));
        effectCanvas.Name = "PART_EffectCanvas";
        effectCanvas.SetValue(Canvas.IsHitTestVisibleProperty, false);
        effectCanvas.SetValue(Canvas.ClipToBoundsProperty, true);
        effectCanvas.SetValue(Grid.ColumnSpanProperty, 3);

        // Assembly
        selectorGrid.AppendChild(listPanel);
        selectorGrid.AppendChild(projectionContainer);
        selectorGrid.AppendChild(detailsPanel);
        selectorGrid.AppendChild(particleCanvas);
        selectorGrid.AppendChild(effectCanvas);

        template.VisualTree = selectorGrid;
        return template;
    }

    private void SetupAnimations()
    {
        // Particle animation timer
        _particleTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50) // 20 FPS
        };
        _particleTimer.Tick += OnParticleTick;

        // 3D rotation timer
        _rotationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
        };
        _rotationTimer.Tick += OnRotationTick;

        // Scanline effect timer
        _scanlineTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100) // 10 FPS
        };
        _scanlineTimer.Tick += OnScanlineTick;
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        if (!EnableParticleEffects || IsInSimplifiedMode || _particleCanvas == null) return;

        _particlePhase += 0.1;
        if (_particlePhase > Math.PI * 2)
            _particlePhase = 0;

        UpdateParticles();
        SpawnCharacterParticles();
    }

    private void OnRotationTick(object sender, EventArgs e)
    {
        if (!AutoRotateProjection || !Enable3DProjection || IsInSimplifiedMode) return;

        _rotationAngle += 0.5;
        if (_rotationAngle >= 360)
            _rotationAngle = 0;

        UpdateCameraRotation();
    }

    private void OnScanlineTick(object sender, EventArgs e)
    {
        if (IsInSimplifiedMode || _effectCanvas == null) return;

        _scanlinePhase += 0.1;
        if (_scanlinePhase > Math.PI * 2)
            _scanlinePhase = 0;

        UpdateScanlineEffects();
    }

    private void UpdateParticles()
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var particle = _particles[i];
            
            // Update particle position
            particle.X += particle.VelocityX;
            particle.Y += particle.VelocityY;
            particle.Life -= 0.02;

            // Update visual position
            Canvas.SetLeft(particle.Visual, particle.X);
            Canvas.SetTop(particle.Visual, particle.Y);
            
            // Update opacity
            particle.Visual.Opacity = Math.Max(0, particle.Life) * HolographicIntensity;

            // Remove dead particles
            if (particle.Life <= 0 || particle.X > ActualWidth + 10 || particle.Y > ActualHeight + 10)
            {
                _particleCanvas.Children.Remove(particle.Visual);
                _particles.RemoveAt(i);
            }
        }
    }

    private void SpawnCharacterParticles()
    {
        if (_particles.Count >= 20) return; // Limit particle count

        if (_random.NextDouble() < 0.1) // 10% chance to spawn
        {
            var particle = CreateCharacterParticle();
            _particles.Add(particle);
            _particleCanvas.Children.Add(particle.Visual);
        }
    }

    private CharacterParticle CreateCharacterParticle()
    {
        var color = GetEVEColor(EVEColorScheme);
        var size = 1 + _random.NextDouble() * 2;
        
        var ellipse = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromArgb(120, color.R, color.G, color.B))
        };

        var particle = new CharacterParticle
        {
            Visual = ellipse,
            X = _random.NextDouble() * ActualWidth,
            Y = ActualHeight + size,
            VelocityX = (_random.NextDouble() - 0.5) * 2,
            VelocityY = -1 - _random.NextDouble() * 3,
            Life = 1.0
        };

        Canvas.SetLeft(ellipse, particle.X);
        Canvas.SetTop(ellipse, particle.Y);

        return particle;
    }

    private void UpdateCameraRotation()
    {
        if (_camera == null) return;

        var radius = 5.0;
        var x = radius * Math.Cos(_rotationAngle * Math.PI / 180);
        var z = radius * Math.Sin(_rotationAngle * Math.PI / 180);

        _camera.Position = new Point3D(x, 0, z);
        _camera.LookDirection = new Vector3D(-x, 0, -z);
    }

    private void UpdateScanlineEffects()
    {
        if (_effectCanvas == null) return;

        // Add holographic scanline effects
        var scanlineHeight = 2;
        var scanlineY = (Math.Sin(_scanlinePhase) * 0.5 + 0.5) * ActualHeight;

        // Remove old scanlines
        var oldScanlines = _effectCanvas.Children.OfType<Rectangle>()
            .Where(r => r.Tag?.ToString() == "scanline").ToList();
        
        foreach (var scanline in oldScanlines)
        {
            _effectCanvas.Children.Remove(scanline);
        }

        // Add new scanline
        var color = GetEVEColor(EVEColorScheme);
        var scanlineRect = new Rectangle
        {
            Width = ActualWidth,
            Height = scanlineHeight,
            Fill = new SolidColorBrush(Color.FromArgb(80, color.R, color.G, color.B)),
            Tag = "scanline"
        };

        Canvas.SetLeft(scanlineRect, 0);
        Canvas.SetTop(scanlineRect, scanlineY);
        _effectCanvas.Children.Add(scanlineRect);
    }

    private void UpdateSelectorAppearance()
    {
        if (Template == null) return;

        Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            ApplyTemplate();
            
            _selectorGrid = GetTemplateChild("PART_SelectorGrid") as Grid;
            _characterList = GetTemplateChild("PART_CharacterList") as ListBox;
            _projectionContainer = GetTemplateChild("PART_ProjectionContainer") as Border;
            _viewport3D = GetTemplateChild("PART_Viewport3D") as Viewport3D;
            _detailsPanel = GetTemplateChild("PART_DetailsPanel") as Border;
            _particleCanvas = GetTemplateChild("PART_ParticleCanvas") as Canvas;
            _effectCanvas = GetTemplateChild("PART_EffectCanvas") as Canvas;

            UpdateColors();
            UpdateEffects();
            Setup3DViewport();
            UpdateCharacterList();
            SetupEventHandlers();
        }), DispatcherPriority.Render);
    }

    private void UpdateColors()
    {
        var color = GetEVEColor(EVEColorScheme);

        // Update panel backgrounds
        foreach (var panel in new[] { _projectionContainer, _detailsPanel })
        {
            if (panel != null)
            {
                var backgroundBrush = new LinearGradientBrush();
                backgroundBrush.StartPoint = new Point(0, 0);
                backgroundBrush.EndPoint = new Point(1, 1);
                backgroundBrush.GradientStops.Add(new GradientStop(
                    Color.FromArgb(120, 0, 20, 40), 0.0));
                backgroundBrush.GradientStops.Add(new GradientStop(
                    Color.FromArgb(100, 0, 15, 30), 1.0));

                panel.Background = backgroundBrush;
                panel.BorderBrush = new SolidColorBrush(Color.FromArgb(
                    150, color.R, color.G, color.B));
            }
        }

        // Character list background
        if (_characterList?.Parent is Border listBorder)
        {
            var listBrush = new LinearGradientBrush();
            listBrush.StartPoint = new Point(0, 0);
            listBrush.EndPoint = new Point(1, 1);
            listBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(140, 0, 20, 40), 0.0));
            listBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(120, 0, 15, 30), 1.0));

            listBorder.Background = listBrush;
            listBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(
                150, color.R, color.G, color.B));
        }
    }

    private void UpdateEffects()
    {
        var color = GetEVEColor(EVEColorScheme);

        if (!IsInSimplifiedMode)
        {
            foreach (var panel in new[] { _projectionContainer, _detailsPanel })
            {
                if (panel != null)
                {
                    panel.Effect = new DropShadowEffect
                    {
                        Color = color,
                        BlurRadius = 8 * HolographicIntensity,
                        ShadowDepth = 0,
                        Opacity = 0.4 * HolographicIntensity
                    };
                }
            }
        }
    }

    private void Setup3DViewport()
    {
        if (_viewport3D == null || !Enable3DProjection) return;

        // Setup camera
        _camera = new PerspectiveCamera
        {
            Position = new Point3D(3, 0, 3),
            LookDirection = new Vector3D(-3, 0, -3),
            UpDirection = new Vector3D(0, 1, 0),
            FieldOfView = 45
        };
        _viewport3D.Camera = _camera;

        // Setup lighting
        var ambientLight = new AmbientLight(Colors.White) { Color = Color.FromRgb(64, 64, 64) };
        var directionalLight = new DirectionalLight
        {
            Color = GetEVEColor(EVEColorScheme),
            Direction = new Vector3D(-1, -1, -1)
        };

        var lightGroup = new Model3DGroup();
        lightGroup.Children.Add(ambientLight);
        lightGroup.Children.Add(directionalLight);

        var lightVisual = new ModelVisual3D { Content = lightGroup };
        _viewport3D.Children.Add(lightVisual);

        // Character model container
        _characterModel = new ModelVisual3D();
        _viewport3D.Children.Add(_characterModel);
    }

    private void UpdateCharacterList()
    {
        if (_characterList == null) return;

        _characterList.Items.Clear();
        _characterControls.Clear();

        foreach (var character in Characters)
        {
            var control = CreateCharacterControl(character);
            _characterControls[character] = control;
            _characterList.Items.Add(control);
        }

        if (_characterList.Items.Count > 0 && SelectedCharacter == null)
        {
            _characterList.SelectedIndex = 0;
        }
    }

    private CharacterItemControl CreateCharacterControl(HoloCharacterInfo character)
    {
        var control = new CharacterItemControl
        {
            Character = character,
            HolographicIntensity = HolographicIntensity,
            EVEColorScheme = EVEColorScheme,
            EnableAnimations = !IsInSimplifiedMode,
            Margin = new Thickness(4, 2, 4, 2)
        };

        control.CharacterClicked += OnCharacterControlClicked;
        control.CharacterHovered += OnCharacterControlHovered;

        return control;
    }

    private void Update3DProjection()
    {
        if (_characterModel == null || !Enable3DProjection || SelectedCharacter == null) return;

        // Clear existing model
        _characterModel.Children.Clear();

        // Create holographic character projection
        var characterGeometry = CreateCharacterGeometry();
        var characterMaterial = CreateHolographicMaterial();
        
        var characterModel = new GeometryModel3D
        {
            Geometry = characterGeometry,
            Material = characterMaterial
        };

        _characterModel.Content = characterModel;
    }

    private MeshGeometry3D CreateCharacterGeometry()
    {
        // Create a simple humanoid silhouette for character projection
        var mesh = new MeshGeometry3D();
        
        // Basic character outline (simplified)
        mesh.Positions.Add(new Point3D(0, 2, 0));    // Head top
        mesh.Positions.Add(new Point3D(-0.3, 1.7, 0)); // Head left
        mesh.Positions.Add(new Point3D(0.3, 1.7, 0));  // Head right
        mesh.Positions.Add(new Point3D(-0.5, 1, 0));   // Shoulder left
        mesh.Positions.Add(new Point3D(0.5, 1, 0));    // Shoulder right
        mesh.Positions.Add(new Point3D(-0.2, 0, 0));   // Waist left
        mesh.Positions.Add(new Point3D(0.2, 0, 0));    // Waist right
        mesh.Positions.Add(new Point3D(-0.3, -1.5, 0)); // Feet left
        mesh.Positions.Add(new Point3D(0.3, -1.5, 0));  // Feet right

        // Create triangular faces for the character silhouette
        // Head
        mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(2);
        
        // Torso
        mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(5);
        mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(4);
        mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(1);
        mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(1);
        
        // Legs
        mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(7); mesh.TriangleIndices.Add(6);
        mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(8); mesh.TriangleIndices.Add(7);

        return mesh;
    }

    private Material CreateHolographicMaterial()
    {
        var color = GetEVEColor(EVEColorScheme);
        var holoBrush = new SolidColorBrush(Color.FromArgb(180, color.R, color.G, color.B));
        
        var material = new DiffuseMaterial(holoBrush);
        return material;
    }

    private void SetupEventHandlers()
    {
        if (_characterList != null)
        {
            _characterList.SelectionChanged -= OnCharacterListSelectionChanged;
            _characterList.SelectionChanged += OnCharacterListSelectionChanged;
        }
    }

    private Color GetEVEColor(EVEColorScheme scheme)
    {
        return scheme switch
        {
            EVEColorScheme.ElectricBlue => Color.FromRgb(64, 224, 255),
            EVEColorScheme.GoldAccent => Color.FromRgb(255, 215, 0),
            EVEColorScheme.CrimsonRed => Color.FromRgb(220, 20, 60),
            EVEColorScheme.EmeraldGreen => Color.FromRgb(50, 205, 50),
            EVEColorScheme.VoidPurple => Color.FromRgb(138, 43, 226),
            _ => Color.FromRgb(64, 224, 255)
        };
    }

    private void OnCharacterControlClicked(object sender, CharacterControlEventArgs e)
    {
        SelectCharacter(e.Character);
        CharacterSelected?.Invoke(this, new HoloCharacterEventArgs { Character = e.Character });
    }

    private void OnCharacterControlHovered(object sender, CharacterControlEventArgs e)
    {
        CharacterHovered?.Invoke(this, new HoloCharacterEventArgs { Character = e.Character });
    }

    private void OnCharacterListSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_characterList.SelectedItem is CharacterItemControl control)
        {
            SelectedCharacter = control.Character;
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableParticleEffects && !IsInSimplifiedMode)
            _particleTimer.Start();
            
        if (Enable3DProjection && AutoRotateProjection && !IsInSimplifiedMode)
            _rotationTimer.Start();
            
        if (!IsInSimplifiedMode)
            _scanlineTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _particleTimer?.Stop();
        _rotationTimer?.Stop();
        _scanlineTimer?.Stop();
        
        // Clean up particles
        _particles.Clear();
        _particleCanvas?.Children.Clear();
        _effectCanvas?.Children.Clear();
        
        // Unregister from animation intensity manager
        AnimationIntensityManager.Instance.UnregisterTarget(this);
        
        _disposed = true;
    }

    #endregion

    #region Event Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSelector selector)
            selector.UpdateSelectorAppearance();
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSelector selector)
            selector.UpdateSelectorAppearance();
    }

    private static void OnCharactersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSelector selector)
        {
            if (e.OldValue is ObservableCollection<HoloCharacterInfo> oldCollection)
                oldCollection.CollectionChanged -= selector.OnCharactersCollectionChanged;

            if (e.NewValue is ObservableCollection<HoloCharacterInfo> newCollection)
                newCollection.CollectionChanged += selector.OnCharactersCollectionChanged;

            selector.UpdateCharacterList();
        }
    }

    private void OnCharactersCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdateCharacterList();
    }

    private static void OnSelectedCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSelector selector)
            selector.Update3DProjection();
    }

    private static void OnEnable3DProjectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSelector selector)
        {
            selector.UpdateSelectorAppearance();
            
            if ((bool)e.NewValue && selector.AutoRotateProjection && !selector.IsInSimplifiedMode)
                selector._rotationTimer.Start();
            else
                selector._rotationTimer.Stop();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSelector selector)
        {
            if ((bool)e.NewValue && !selector.IsInSimplifiedMode)
                selector._particleTimer.Start();
            else
                selector._particleTimer.Stop();
        }
    }

    #endregion
}

/// <summary>
/// Character particle for visual effects
/// </summary>
internal class CharacterParticle
{
    public FrameworkElement Visual { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
}

/// <summary>
/// Individual character item control
/// </summary>
internal class CharacterItemControl : Control
{
    public HoloCharacterInfo Character { get; set; }
    public double HolographicIntensity { get; set; }
    public EVEColorScheme EVEColorScheme { get; set; }
    public bool EnableAnimations { get; set; }

    public event EventHandler<CharacterControlEventArgs> CharacterClicked;
    public event EventHandler<CharacterControlEventArgs> CharacterHovered;

    public CharacterItemControl()
    {
        MouseLeftButtonUp += OnItemClick;
        MouseEnter += OnItemHover;
    }

    private void OnItemClick(object sender, MouseButtonEventArgs e)
    {
        CharacterClicked?.Invoke(this, new CharacterControlEventArgs { Character = Character });
    }

    private void OnItemHover(object sender, MouseEventArgs e)
    {
        CharacterHovered?.Invoke(this, new CharacterControlEventArgs { Character = Character });
    }
}

/// <summary>
/// Holographic character information
/// </summary>
public class HoloCharacterInfo : INotifyPropertyChanged
{
    private string _name;
    private string _corporation;
    private string _alliance;
    private ImageSource _portrait;
    private long _skillPoints;
    private string _securityStatus;
    private string _location;

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public string Corporation
    {
        get => _corporation;
        set { _corporation = value; OnPropertyChanged(); }
    }

    public string Alliance
    {
        get => _alliance;
        set { _alliance = value; OnPropertyChanged(); }
    }

    public ImageSource Portrait
    {
        get => _portrait;
        set { _portrait = value; OnPropertyChanged(); }
    }

    public long SkillPoints
    {
        get => _skillPoints;
        set { _skillPoints = value; OnPropertyChanged(); }
    }

    public string SecurityStatus
    {
        get => _securityStatus;
        set { _securityStatus = value; OnPropertyChanged(); }
    }

    public string Location
    {
        get => _location;
        set { _location = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Event args for character events
/// </summary>
public class HoloCharacterEventArgs : EventArgs
{
    public HoloCharacterInfo Character { get; set; }
}

/// <summary>
/// Event args for character control events
/// </summary>
public class CharacterControlEventArgs : EventArgs
{
    public HoloCharacterInfo Character { get; set; }
}