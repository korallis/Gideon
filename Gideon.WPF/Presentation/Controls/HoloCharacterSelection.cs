// ==========================================================================
// HoloCharacterSelection.cs - Holographic Character Selection with 3D Projections
// ==========================================================================
// Advanced character selection interface featuring 3D holographic character
// projections, animated portraits, corporation insignia, and fluid transitions.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using HelixToolkit.Wpf;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Holographic character selection interface with 3D projections and animated portraits
/// </summary>
public class HoloCharacterSelection : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloCharacterSelection),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloCharacterSelection),
            new PropertyMetadata(EVEColorScheme.VoidPurple, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty SelectedCharacterProperty =
        DependencyProperty.Register(nameof(SelectedCharacter), typeof(HoloCharacterInfo), typeof(HoloCharacterSelection),
            new PropertyMetadata(null, OnSelectedCharacterChanged));

    public static readonly DependencyProperty CharacterListProperty =
        DependencyProperty.Register(nameof(CharacterList), typeof(ObservableCollection<HoloCharacterInfo>), typeof(HoloCharacterSelection),
            new PropertyMetadata(null));

    public static readonly DependencyProperty Enable3DProjectionProperty =
        DependencyProperty.Register(nameof(Enable3DProjection), typeof(bool), typeof(HoloCharacterSelection),
            new PropertyMetadata(true, OnEnable3DProjectionChanged));

    public static readonly DependencyProperty EnableCharacterAnimationsProperty =
        DependencyProperty.Register(nameof(EnableCharacterAnimations), typeof(bool), typeof(HoloCharacterSelection),
            new PropertyMetadata(true, OnEnableCharacterAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloCharacterSelection),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowCharacterDetailsProperty =
        DependencyProperty.Register(nameof(ShowCharacterDetails), typeof(bool), typeof(HoloCharacterSelection),
            new PropertyMetadata(true));

    public static readonly DependencyProperty SelectionModeProperty =
        DependencyProperty.Register(nameof(SelectionMode), typeof(CharacterSelectionMode), typeof(HoloCharacterSelection),
            new PropertyMetadata(CharacterSelectionMode.Grid, OnSelectionModeChanged));

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

    public HoloCharacterInfo SelectedCharacter
    {
        get => (HoloCharacterInfo)GetValue(SelectedCharacterProperty);
        set => SetValue(SelectedCharacterProperty, value);
    }

    public ObservableCollection<HoloCharacterInfo> CharacterList
    {
        get => (ObservableCollection<HoloCharacterInfo>)GetValue(CharacterListProperty);
        set => SetValue(CharacterListProperty, value);
    }

    public bool Enable3DProjection
    {
        get => (bool)GetValue(Enable3DProjectionProperty);
        set => SetValue(Enable3DProjectionProperty, value);
    }

    public bool EnableCharacterAnimations
    {
        get => (bool)GetValue(EnableCharacterAnimationsProperty);
        set => SetValue(EnableCharacterAnimationsProperty, value);
    }

    public bool EnableParticleEffects
    {
        get => (bool)GetValue(EnableParticleEffectsProperty);
        set => SetValue(EnableParticleEffectsProperty, value);
    }

    public bool ShowCharacterDetails
    {
        get => (bool)GetValue(ShowCharacterDetailsProperty);
        set => SetValue(ShowCharacterDetailsProperty, value);
    }

    public CharacterSelectionMode SelectionMode
    {
        get => (CharacterSelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloCharacterSelectionEventArgs> CharacterSelected;
    public event EventHandler<HoloCharacterSelectionEventArgs> CharacterHovered;
    public event EventHandler<HoloCharacterSelectionEventArgs> CharacterAdded;
    public event EventHandler<HoloCharacterSelectionEventArgs> CharacterRemoved;
    public event EventHandler<HoloCharacterSelectionEventArgs> SelectionModeChanged;

    #endregion

    #region Private Fields

    private readonly DispatcherTimer _animationTimer;
    private readonly Random _random = new();
    private Canvas _mainCanvas;
    private Grid _characterGrid;
    private UniformGrid _characterUniformGrid;
    private StackPanel _characterList;
    private Grid _detailPanel;
    private Viewport3D _characterViewport;
    private Canvas _particleCanvas;
    private TextBlock _characterNameText;
    private TextBlock _corporationNameText;
    private TextBlock _allianceNameText;
    private TextBlock _skillPointsText;
    private Image _corporationLogo;
    private Image _allianceLogo;
    private ProgressBar _skillProgress;
    private Button _selectButton;
    private Button _addCharacterButton;
    private ComboBox _selectionModeCombo;
    private readonly List<UIElement> _particles = new();
    private readonly Dictionary<string, HoloCharacterProjection> _characterProjections = new();
    private bool _isSimplifiedMode;
    private HoloCharacterInfo _hoveredCharacter;

    #endregion

    #region Constructor

    public HoloCharacterSelection()
    {
        InitializeComponent();
        InitializeCollections();
        InitializeTimer();
        LoadSampleCharacters();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 1000;
        Height = 700;
        Background = new SolidColorBrush(Color.FromArgb(25, 100, 50, 150));
        BorderBrush = new SolidColorBrush(Color.FromArgb(100, 100, 50, 150));
        BorderThickness = new Thickness(2);
        Effect = new DropShadowEffect
        {
            Color = Color.FromArgb(80, 100, 50, 150),
            BlurRadius = 15,
            ShadowDepth = 5,
            Direction = 315
        };

        CreateMainLayout();
        ApplyEVEColorScheme();
    }

    private void CreateMainLayout()
    {
        _mainCanvas = new Canvas();
        Content = _mainCanvas;

        // Background with holographic grid
        CreateHolographicBackground();

        // Main container
        var mainContainer = new DockPanel
        {
            Width = 1000,
            Height = 700,
            Margin = new Thickness(20)
        };
        _mainCanvas.Children.Add(mainContainer);

        CreateHeaderSection(mainContainer);
        CreateCharacterDisplayArea(mainContainer);
        CreateDetailPanel(mainContainer);
        CreateParticleCanvas();
    }

    private void CreateHolographicBackground()
    {
        var backgroundRect = new Rectangle
        {
            Fill = new RadialGradientBrush
            {
                Center = new Point(0.5, 0.5),
                RadiusX = 0.8,
                RadiusY = 0.8,
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(40, 100, 50, 150), 0),
                    new GradientStop(Color.FromArgb(15, 50, 25, 100), 0.7),
                    new GradientStop(Color.FromArgb(30, 100, 50, 150), 1)
                }
            },
            Width = 1000,
            Height = 700
        };
        _mainCanvas.Children.Add(backgroundRect);

        // Animated scanning lines
        CreateScanningLines();
    }

    private void CreateScanningLines()
    {
        for (int i = 0; i < 5; i++)
        {
            var line = new Rectangle
            {
                Width = 1000,
                Height = 2,
                Fill = new SolidColorBrush(Color.FromArgb(60, 100, 200, 255)),
                Opacity = 0.3
            };

            Canvas.SetTop(line, i * 140);
            _mainCanvas.Children.Add(line);

            // Animate the scanning line
            var moveAnimation = new DoubleAnimation
            {
                From = -2,
                To = 702,
                Duration = TimeSpan.FromSeconds(3 + i * 0.5),
                RepeatBehavior = RepeatBehavior.Forever
            };
            line.BeginAnimation(Canvas.TopProperty, moveAnimation);
        }
    }

    private void CreateHeaderSection(DockPanel container)
    {
        var headerPanel = new DockPanel
        {
            Height = 60,
            Margin = new Thickness(0, 0, 0, 20)
        };
        DockPanel.SetDock(headerPanel, Dock.Top);
        container.Children.Add(headerPanel);

        // Title
        var titleText = new TextBlock
        {
            Text = "CHARACTER SELECTION",
            FontSize = 24,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 100, 200, 255)),
            VerticalAlignment = VerticalAlignment.Center,
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(120, 100, 200, 255),
                BlurRadius = 8,
                ShadowDepth = 3
            }
        };
        DockPanel.SetDock(titleText, Dock.Left);
        headerPanel.Children.Add(titleText);

        // Controls panel
        var controlsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        DockPanel.SetDock(controlsPanel, Dock.Right);
        headerPanel.Children.Add(controlsPanel);

        // Selection mode combo
        _selectionModeCombo = new ComboBox
        {
            Width = 120,
            Height = 35,
            Margin = new Thickness(0, 0, 15, 0)
        };
        _selectionModeCombo.Items.Add(new ComboBoxItem { Content = "Grid View", Tag = CharacterSelectionMode.Grid });
        _selectionModeCombo.Items.Add(new ComboBoxItem { Content = "List View", Tag = CharacterSelectionMode.List });
        _selectionModeCombo.Items.Add(new ComboBoxItem { Content = "3D Carousel", Tag = CharacterSelectionMode.Carousel });
        _selectionModeCombo.SelectedIndex = 0;
        _selectionModeCombo.SelectionChanged += OnSelectionModeComboChanged;
        controlsPanel.Children.Add(_selectionModeCombo);

        // Add character button
        _addCharacterButton = new Button
        {
            Content = "ADD CHARACTER",
            Width = 140,
            Height = 35,
            Style = CreateHolographicButtonStyle()
        };
        _addCharacterButton.Click += OnAddCharacterClicked;
        controlsPanel.Children.Add(_addCharacterButton);
    }

    private void CreateCharacterDisplayArea(DockPanel container)
    {
        var displayContainer = new Grid();
        container.Children.Add(displayContainer);

        // Grid layout
        _characterGrid = new Grid
        {
            Visibility = SelectionMode == CharacterSelectionMode.Grid ? Visibility.Visible : Visibility.Collapsed
        };
        displayContainer.Children.Add(_characterGrid);

        _characterUniformGrid = new UniformGrid
        {
            Columns = 3,
            Rows = 2,
            Margin = new Thickness(10)
        };
        _characterGrid.Children.Add(_characterUniformGrid);

        // List layout
        _characterList = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Visibility = SelectionMode == CharacterSelectionMode.List ? Visibility.Visible : Visibility.Collapsed,
            Margin = new Thickness(10)
        };
        displayContainer.Children.Add(_characterList);

        // 3D Viewport for carousel
        _characterViewport = new Viewport3D
        {
            Visibility = SelectionMode == CharacterSelectionMode.Carousel ? Visibility.Visible : Visibility.Collapsed
        };
        displayContainer.Children.Add(_characterViewport);

        Setup3DViewport();
    }

    private void Setup3DViewport()
    {
        if (!Enable3DProjection) return;

        // Camera
        var camera = new PerspectiveCamera
        {
            Position = new Point3D(0, 0, 10),
            LookDirection = new Vector3D(0, 0, -1),
            UpDirection = new Vector3D(0, 1, 0),
            FieldOfView = 45
        };
        _characterViewport.Camera = camera;

        // Lights
        var ambientLight = new AmbientLight(Colors.White) { Color = Color.FromRgb(64, 64, 64) };
        var directionalLight = new DirectionalLight
        {
            Color = Colors.White,
            Direction = new Vector3D(-1, -1, -1)
        };

        var lightGroup = new Model3DGroup();
        lightGroup.Children.Add(ambientLight);
        lightGroup.Children.Add(directionalLight);

        var lightModel = new ModelVisual3D { Content = lightGroup };
        _characterViewport.Children.Add(lightModel);
    }

    private void CreateDetailPanel(DockPanel container)
    {
        _detailPanel = new Grid
        {
            Width = 300,
            Margin = new Thickness(20, 0, 0, 0),
            Visibility = ShowCharacterDetails ? Visibility.Visible : Visibility.Collapsed
        };
        DockPanel.SetDock(_detailPanel, Dock.Right);
        container.Children.Add(_detailPanel);

        var detailBackground = new Border
        {
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(60, 100, 200, 255), 0),
                    new GradientStop(Color.FromArgb(20, 50, 100, 200), 0.5),
                    new GradientStop(Color.FromArgb(40, 100, 200, 255), 1)
                }
            },
            BorderBrush = new SolidColorBrush(Color.FromArgb(120, 100, 200, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Padding = new Thickness(20),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(80, 100, 200, 255),
                BlurRadius = 10,
                ShadowDepth = 3
            }
        };
        _detailPanel.Children.Add(detailBackground);

        var detailContent = new StackPanel();
        detailBackground.Child = detailContent;

        // Detail panel title
        var detailTitle = new TextBlock
        {
            Text = "CHARACTER DETAILS",
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 100, 200, 255)),
            Margin = new Thickness(0, 0, 0, 20),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        detailContent.Children.Add(detailTitle);

        // Character portrait placeholder
        var portraitPlaceholder = new Rectangle
        {
            Width = 128,
            Height = 128,
            Fill = new SolidColorBrush(Color.FromArgb(40, 100, 200, 255)),
            Stroke = new SolidColorBrush(Color.FromArgb(120, 100, 200, 255)),
            StrokeThickness = 2,
            Margin = new Thickness(0, 0, 0, 20),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        detailContent.Children.Add(portraitPlaceholder);

        // Character name
        _characterNameText = new TextBlock
        {
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        detailContent.Children.Add(_characterNameText);

        // Corporation info
        var corpPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        detailContent.Children.Add(corpPanel);

        _corporationLogo = new Image
        {
            Width = 24,
            Height = 24,
            Margin = new Thickness(0, 0, 5, 0)
        };
        corpPanel.Children.Add(_corporationLogo);

        _corporationNameText = new TextBlock
        {
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 200, 200)),
            VerticalAlignment = VerticalAlignment.Center
        };
        corpPanel.Children.Add(_corporationNameText);

        // Alliance info
        var alliancePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 20)
        };
        detailContent.Children.Add(alliancePanel);

        _allianceLogo = new Image
        {
            Width = 20,
            Height = 20,
            Margin = new Thickness(0, 0, 5, 0)
        };
        alliancePanel.Children.Add(_allianceLogo);

        _allianceNameText = new TextBlock
        {
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 180, 180, 180)),
            VerticalAlignment = VerticalAlignment.Center
        };
        alliancePanel.Children.Add(_allianceNameText);

        // Skill points
        var skillPanel = new StackPanel
        {
            Margin = new Thickness(0, 0, 0, 20)
        };
        detailContent.Children.Add(skillPanel);

        var skillLabel = new TextBlock
        {
            Text = "SKILL POINTS",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 180, 180, 180)),
            Margin = new Thickness(0, 0, 0, 5)
        };
        skillPanel.Children.Add(skillLabel);

        _skillPointsText = new TextBlock
        {
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 100, 200, 255))
        };
        skillPanel.Children.Add(_skillPointsText);

        _skillProgress = new ProgressBar
        {
            Height = 4,
            Margin = new Thickness(0, 5, 0, 0),
            Background = new SolidColorBrush(Color.FromArgb(40, 100, 200, 255)),
            Foreground = new SolidColorBrush(Color.FromArgb(180, 100, 200, 255)),
            BorderThickness = new Thickness(0)
        };
        skillPanel.Children.Add(_skillProgress);

        // Select button
        _selectButton = new Button
        {
            Content = "SELECT CHARACTER",
            Height = 40,
            Margin = new Thickness(0, 20, 0, 0),
            Style = CreateHolographicButtonStyle()
        };
        _selectButton.Click += OnSelectCharacterClicked;
        detailContent.Children.Add(_selectButton);
    }

    private void CreateParticleCanvas()
    {
        _particleCanvas = new Canvas
        {
            Width = 1000,
            Height = 700,
            IsHitTestVisible = false
        };
        _mainCanvas.Children.Add(_particleCanvas);
    }

    private void InitializeCollections()
    {
        CharacterList = new ObservableCollection<HoloCharacterInfo>();
        CharacterList.CollectionChanged += OnCharacterListCollectionChanged;
    }

    private void InitializeTimer()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _animationTimer.Tick += OnAnimationTimerTick;
        _animationTimer.Start();
    }

    #endregion

    #region Sample Data

    private void LoadSampleCharacters()
    {
        var sampleCharacters = new[]
        {
            new HoloCharacterInfo
            {
                CharacterId = "1001",
                Name = "Captain Stellar",
                CorporationName = "Stellar Industries",
                AllianceName = "Galactic Federation",
                SkillPoints = 85420000,
                Portrait = null, // Would be loaded from EVE API
                CorporationLogo = null,
                AllianceLogo = null,
                Security = 2.1,
                Location = "Jita IV - Moon 4",
                LastLogin = DateTime.Now.AddHours(-2),
                IsOmega = true,
                Race = "Caldari",
                Bloodline = "Civire"
            },
            new HoloCharacterInfo
            {
                CharacterId = "1002",
                Name = "Commander Nova",
                CorporationName = "Deep Space Mining",
                AllianceName = "Industrial Complex",
                SkillPoints = 62150000,
                Portrait = null,
                CorporationLogo = null,
                AllianceLogo = null,
                Security = -0.8,
                Location = "Null-Sec Outpost",
                LastLogin = DateTime.Now.AddDays(-1),
                IsOmega = true,
                Race = "Gallente",
                Bloodline = "Jin-Mei"
            },
            new HoloCharacterInfo
            {
                CharacterId = "1003",
                Name = "Pilot Vortex",
                CorporationName = "Elite Pilots",
                AllianceName = "Combat Alliance",
                SkillPoints = 45300000,
                Portrait = null,
                CorporationLogo = null,
                AllianceLogo = null,
                Security = 1.5,
                Location = "Amarr VIII",
                LastLogin = DateTime.Now.AddMinutes(-30),
                IsOmega = false,
                Race = "Amarr",
                Bloodline = "Amarr"
            },
            new HoloCharacterInfo
            {
                CharacterId = "1004",
                Name = "Agent Phoenix",
                CorporationName = "Shadow Operations",
                AllianceName = "Covert Ops",
                SkillPoints = 91750000,
                Portrait = null,
                CorporationLogo = null,
                AllianceLogo = null,
                Security = 0.2,
                Location = "Low-Sec System",
                LastLogin = DateTime.Now.AddHours(-6),
                IsOmega = true,
                Race = "Minmatar",
                Bloodline = "Brutor"
            }
        };

        foreach (var character in sampleCharacters)
        {
            CharacterList.Add(character);
        }

        if (CharacterList.Count > 0)
        {
            SelectedCharacter = CharacterList[0];
        }
    }

    #endregion

    #region Character Display Management

    private void RefreshCharacterDisplay()
    {
        switch (SelectionMode)
        {
            case CharacterSelectionMode.Grid:
                PopulateGridView();
                break;
            case CharacterSelectionMode.List:
                PopulateListView();
                break;
            case CharacterSelectionMode.Carousel:
                Populate3DCarousel();
                break;
        }
    }

    private void PopulateGridView()
    {
        _characterUniformGrid.Children.Clear();
        _characterProjections.Clear();

        foreach (var character in CharacterList.Take(6)) // Max 6 in grid view
        {
            var characterCard = CreateCharacterCard(character);
            _characterUniformGrid.Children.Add(characterCard);
        }
    }

    private void PopulateListView()
    {
        _characterList.Children.Clear();
        _characterProjections.Clear();

        foreach (var character in CharacterList)
        {
            var characterItem = CreateCharacterListItem(character);
            _characterList.Children.Add(characterItem);
        }
    }

    private void Populate3DCarousel()
    {
        if (!Enable3DProjection) return;

        // Clear existing projections
        foreach (var projection in _characterProjections.Values)
        {
            _characterViewport.Children.Remove(projection.Visual);
        }
        _characterProjections.Clear();

        var characterCount = CharacterList.Count;
        if (characterCount == 0) return;

        var radius = 5.0;
        var angleStep = 360.0 / characterCount;

        for (int i = 0; i < characterCount; i++)
        {
            var character = CharacterList[i];
            var angle = i * angleStep * Math.PI / 180.0;
            
            var x = radius * Math.Cos(angle);
            var z = radius * Math.Sin(angle);

            var projection = Create3DCharacterProjection(character, new Point3D(x, 0, z));
            _characterProjections[character.CharacterId] = projection;
            _characterViewport.Children.Add(projection.Visual);
        }
    }

    private UserControl CreateCharacterCard(HoloCharacterInfo character)
    {
        var card = new UserControl
        {
            Width = 200,
            Height = 250,
            Margin = new Thickness(10),
            Cursor = Cursors.Hand
        };

        var cardBackground = new Border
        {
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(60, 100, 200, 255), 0),
                    new GradientStop(Color.FromArgb(20, 50, 100, 200), 0.5),
                    new GradientStop(Color.FromArgb(40, 100, 200, 255), 1)
                }
            },
            BorderBrush = new SolidColorBrush(Color.FromArgb(120, 100, 200, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(80, 100, 200, 255),
                BlurRadius = 8,
                ShadowDepth = 3
            }
        };
        card.Content = cardBackground;

        var cardContent = new StackPanel
        {
            Margin = new Thickness(15)
        };
        cardBackground.Child = cardContent;

        // Character portrait
        var portraitRect = new Rectangle
        {
            Width = 80,
            Height = 80,
            Fill = new SolidColorBrush(Color.FromArgb(60, 100, 200, 255)),
            Stroke = new SolidColorBrush(Color.FromArgb(150, 100, 200, 255)),
            StrokeThickness = 2,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        cardContent.Children.Add(portraitRect);

        // Character name
        var nameText = new TextBlock
        {
            Text = character.Name,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 5)
        };
        cardContent.Children.Add(nameText);

        // Corporation
        var corpText = new TextBlock
        {
            Text = character.CorporationName,
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 200, 200)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        cardContent.Children.Add(corpText);

        // Skill points
        var spText = new TextBlock
        {
            Text = $"{character.SkillPoints / 1000000.0:F1}M SP",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 100, 200, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 5)
        };
        cardContent.Children.Add(spText);

        // Status indicators
        var statusPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        cardContent.Children.Add(statusPanel);

        if (character.IsOmega)
        {
            var omegaIndicator = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(Color.FromArgb(255, 255, 200, 100)),
                Margin = new Thickness(2)
            };
            statusPanel.Children.Add(omegaIndicator);
        }

        var onlineIndicator = new Ellipse
        {
            Width = 8,
            Height = 8,
            Fill = new SolidColorBrush(
                character.LastLogin > DateTime.Now.AddHours(-1) 
                    ? Color.FromArgb(255, 100, 255, 100) 
                    : Color.FromArgb(255, 150, 150, 150)),
            Margin = new Thickness(2)
        };
        statusPanel.Children.Add(onlineIndicator);

        // Event handlers
        card.MouseEnter += (s, e) => OnCharacterCardMouseEnter(character, card);
        card.MouseLeave += (s, e) => OnCharacterCardMouseLeave(character, card);
        card.MouseLeftButtonUp += (s, e) => OnCharacterCardClicked(character);

        return card;
    }

    private UserControl CreateCharacterListItem(HoloCharacterInfo character)
    {
        var item = new UserControl
        {
            Height = 60,
            Margin = new Thickness(0, 2),
            Cursor = Cursors.Hand
        };

        var itemBackground = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 100, 200, 255)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(80, 100, 200, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3)
        };
        item.Content = itemBackground;

        var itemContent = new DockPanel
        {
            Margin = new Thickness(15, 10)
        };
        itemBackground.Child = itemContent;

        // Character portrait
        var portrait = new Rectangle
        {
            Width = 40,
            Height = 40,
            Fill = new SolidColorBrush(Color.FromArgb(60, 100, 200, 255)),
            Stroke = new SolidColorBrush(Color.FromArgb(120, 100, 200, 255)),
            StrokeThickness = 1
        };
        DockPanel.SetDock(portrait, Dock.Left);
        itemContent.Children.Add(portrait);

        // Character info
        var infoPanel = new StackPanel
        {
            Margin = new Thickness(15, 0, 0, 0)
        };
        itemContent.Children.Add(infoPanel);

        var nameText = new TextBlock
        {
            Text = character.Name,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
        };
        infoPanel.Children.Add(nameText);

        var detailsText = new TextBlock
        {
            Text = $"{character.CorporationName} • {character.SkillPoints / 1000000.0:F1}M SP",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 200, 200))
        };
        infoPanel.Children.Add(detailsText);

        // Event handlers
        item.MouseEnter += (s, e) => OnCharacterCardMouseEnter(character, item);
        item.MouseLeave += (s, e) => OnCharacterCardMouseLeave(character, item);
        item.MouseLeftButtonUp += (s, e) => OnCharacterCardClicked(character);

        return item;
    }

    private HoloCharacterProjection Create3DCharacterProjection(HoloCharacterInfo character, Point3D position)
    {
        var projection = new HoloCharacterProjection
        {
            Character = character,
            Position = position
        };

        // Create 3D model for character
        var geometry = new MeshGeometry3D();
        
        // Simple character representation (could be enhanced with actual 3D models)
        geometry.Positions.Add(new Point3D(-0.5, -1, 0));
        geometry.Positions.Add(new Point3D(0.5, -1, 0));
        geometry.Positions.Add(new Point3D(0.5, 1, 0));
        geometry.Positions.Add(new Point3D(-0.5, 1, 0));
        
        geometry.TriangleIndices.Add(0);
        geometry.TriangleIndices.Add(1);
        geometry.TriangleIndices.Add(2);
        geometry.TriangleIndices.Add(0);
        geometry.TriangleIndices.Add(2);
        geometry.TriangleIndices.Add(3);
        
        geometry.TextureCoordinates.Add(new Point(0, 1));
        geometry.TextureCoordinates.Add(new Point(1, 1));
        geometry.TextureCoordinates.Add(new Point(1, 0));
        geometry.TextureCoordinates.Add(new Point(0, 0));

        var material = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(120, 100, 200, 255)));
        
        var model = new GeometryModel3D(geometry, material);
        
        var transform = new Transform3DGroup();
        transform.Children.Add(new TranslateTransform3D(position.X, position.Y, position.Z));
        model.Transform = transform;

        var visual = new ModelVisual3D { Content = model };
        projection.Visual = visual;
        projection.Model = model;

        return projection;
    }

    #endregion

    #region Animation and Effects

    private void AnimateCharacterSelection(HoloCharacterInfo character)
    {
        if (_isSimplifiedMode || !EnableCharacterAnimations) return;

        if (EnableParticleEffects)
        {
            SpawnSelectionParticles();
        }

        // Update detail panel with animation
        UpdateCharacterDetails(character);
    }

    private void SpawnSelectionParticles()
    {
        if (_isSimplifiedMode || !EnableParticleEffects) return;

        for (int i = 0; i < 20; i++)
        {
            var particle = new Ellipse
            {
                Width = _random.Next(3, 8),
                Height = _random.Next(3, 8),
                Fill = new SolidColorBrush(Color.FromArgb(
                    (byte)_random.Next(150, 255),
                    (byte)_random.Next(80, 150),
                    (byte)_random.Next(150, 255),
                    (byte)_random.Next(200, 255)))
            };

            Canvas.SetLeft(particle, _random.Next(700, 1000));
            Canvas.SetTop(particle, _random.Next(200, 500));
            _particleCanvas.Children.Add(particle);
            _particles.Add(particle);

            AnimateSelectionParticle(particle);
        }

        CleanupParticles();
    }

    private void AnimateSelectionParticle(UIElement particle)
    {
        var moveAnimation = new DoubleAnimation
        {
            From = Canvas.GetLeft(particle),
            To = Canvas.GetLeft(particle) + _random.Next(-50, 50),
            Duration = TimeSpan.FromMilliseconds(_random.Next(1000, 2000)),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(_random.Next(1000, 2000))
        };

        moveAnimation.Completed += (s, e) =>
        {
            _particleCanvas.Children.Remove(particle);
            _particles.Remove(particle);
        };

        particle.BeginAnimation(Canvas.LeftProperty, moveAnimation);
        particle.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void CleanupParticles()
    {
        if (_particles.Count > 100)
        {
            var particlesToRemove = _particles.Take(50).ToList();
            foreach (var particle in particlesToRemove)
            {
                _particleCanvas.Children.Remove(particle);
                _particles.Remove(particle);
            }
        }
    }

    #endregion

    #region Character Details

    private void UpdateCharacterDetails(HoloCharacterInfo character)
    {
        if (character == null) return;

        _characterNameText.Text = character.Name;
        _corporationNameText.Text = character.CorporationName;
        _allianceNameText.Text = character.AllianceName ?? "No Alliance";
        _skillPointsText.Text = $"{character.SkillPoints:N0}";
        
        // Calculate progress to next million SP
        var currentMillions = character.SkillPoints / 1000000;
        var nextMillion = (currentMillions + 1) * 1000000;
        var progressPercent = ((double)(character.SkillPoints % 1000000) / 1000000) * 100;
        _skillProgress.Value = progressPercent;

        // Enable/disable select button
        _selectButton.IsEnabled = character != SelectedCharacter;
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        RefreshCharacterDisplay();
        
        if (SelectedCharacter != null)
        {
            UpdateCharacterDetails(SelectedCharacter);
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        CleanupAllParticles();
    }

    private void OnAnimationTimerTick(object sender, EventArgs e)
    {
        if (EnableParticleEffects && !_isSimplifiedMode && _random.NextDouble() < 0.05)
        {
            SpawnAmbientParticles();
        }

        // Animate 3D carousel rotation
        if (SelectionMode == CharacterSelectionMode.Carousel && Enable3DProjection)
        {
            RotateCarousel();
        }
    }

    private void OnCharacterListCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        RefreshCharacterDisplay();
    }

    private void OnCharacterCardMouseEnter(HoloCharacterInfo character, UserControl card)
    {
        _hoveredCharacter = character;
        
        if (EnableCharacterAnimations && !_isSimplifiedMode)
        {
            var scaleTransform = new ScaleTransform(1.05, 1.05);
            card.RenderTransform = scaleTransform;
            card.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        CharacterHovered?.Invoke(this, new HoloCharacterSelectionEventArgs
        {
            Character = character,
            Action = CharacterSelectionAction.Hovered,
            Timestamp = DateTime.Now
        });
    }

    private void OnCharacterCardMouseLeave(HoloCharacterInfo character, UserControl card)
    {
        _hoveredCharacter = null;
        
        if (EnableCharacterAnimations && !_isSimplifiedMode)
        {
            card.RenderTransform = null;
        }
    }

    private void OnCharacterCardClicked(HoloCharacterInfo character)
    {
        SelectedCharacter = character;
        AnimateCharacterSelection(character);
        
        CharacterSelected?.Invoke(this, new HoloCharacterSelectionEventArgs
        {
            Character = character,
            Action = CharacterSelectionAction.Selected,
            Timestamp = DateTime.Now
        });
    }

    private void OnSelectCharacterClicked(object sender, RoutedEventArgs e)
    {
        if (SelectedCharacter != null)
        {
            CharacterSelected?.Invoke(this, new HoloCharacterSelectionEventArgs
            {
                Character = SelectedCharacter,
                Action = CharacterSelectionAction.Confirmed,
                Timestamp = DateTime.Now
            });
        }
    }

    private void OnAddCharacterClicked(object sender, RoutedEventArgs e)
    {
        CharacterAdded?.Invoke(this, new HoloCharacterSelectionEventArgs
        {
            Action = CharacterSelectionAction.AddRequested,
            Timestamp = DateTime.Now
        });
    }

    private void OnSelectionModeComboChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_selectionModeCombo.SelectedItem is ComboBoxItem item && item.Tag is CharacterSelectionMode mode)
        {
            SelectionMode = mode;
        }
    }

    #endregion

    #region Helper Methods

    private void SpawnAmbientParticles()
    {
        var particle = new Ellipse
        {
            Width = _random.Next(1, 3),
            Height = _random.Next(1, 3),
            Fill = new SolidColorBrush(Color.FromArgb(
                (byte)_random.Next(50, 150),
                (byte)_random.Next(80, 150),
                (byte)_random.Next(150, 255),
                (byte)_random.Next(200, 255)))
        };

        Canvas.SetLeft(particle, _random.Next(0, 1000));
        Canvas.SetTop(particle, _random.Next(0, 700));
        _particleCanvas.Children.Add(particle);
        _particles.Add(particle);

        var floatAnimation = new DoubleAnimation
        {
            From = Canvas.GetTop(particle),
            To = Canvas.GetTop(particle) - _random.Next(50, 150),
            Duration = TimeSpan.FromMilliseconds(_random.Next(3000, 6000)),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = 0.8,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(_random.Next(3000, 6000))
        };

        floatAnimation.Completed += (s, e) =>
        {
            _particleCanvas.Children.Remove(particle);
            _particles.Remove(particle);
        };

        particle.BeginAnimation(Canvas.TopProperty, floatAnimation);
        particle.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void RotateCarousel()
    {
        if (_characterProjections.Count == 0) return;

        var rotationAngle = DateTime.Now.Millisecond / 1000.0 * 360.0;
        
        foreach (var projection in _characterProjections.Values)
        {
            if (projection.Model?.Transform is Transform3DGroup transformGroup)
            {
                var existingTranslate = transformGroup.Children.OfType<TranslateTransform3D>().FirstOrDefault();
                if (existingTranslate != null)
                {
                    transformGroup.Children.Clear();
                    transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), rotationAngle)));
                    transformGroup.Children.Add(existingTranslate);
                }
            }
        }
    }

    private void CleanupAllParticles()
    {
        foreach (var particle in _particles.ToList())
        {
            _particleCanvas.Children.Remove(particle);
        }
        _particles.Clear();
    }

    private Style CreateHolographicButtonStyle()
    {
        var style = new Style(typeof(Button));
        
        style.Setters.Add(new Setter(BackgroundProperty, new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(0, 1),
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Color.FromArgb(80, 100, 200, 255), 0),
                new GradientStop(Color.FromArgb(40, 50, 150, 255), 1)
            }
        }));
        
        style.Setters.Add(new Setter(BorderBrushProperty, new SolidColorBrush(Color.FromArgb(150, 100, 200, 255))));
        style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(ForegroundProperty, new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))));
        style.Setters.Add(new Setter(EffectProperty, new DropShadowEffect
        {
            Color = Color.FromArgb(80, 100, 200, 255),
            BlurRadius = 8,
            ShadowDepth = 3
        }));

        return style;
    }

    #endregion

    #region Property Change Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSelection control)
        {
            control.ApplyHolographicIntensity();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSelection control)
        {
            control.ApplyEVEColorScheme();
        }
    }

    private static void OnSelectedCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSelection control && e.NewValue is HoloCharacterInfo character)
        {
            control.UpdateCharacterDetails(character);
        }
    }

    private static void OnEnable3DProjectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSelection control)
        {
            control.Setup3DViewport();
            control.RefreshCharacterDisplay();
        }
    }

    private static void OnEnableCharacterAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Animation state changed
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSelection control && !(bool)e.NewValue)
        {
            control.CleanupAllParticles();
        }
    }

    private static void OnSelectionModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSelection control)
        {
            control.UpdateSelectionModeVisibility();
            control.RefreshCharacterDisplay();
            
            control.SelectionModeChanged?.Invoke(control, new HoloCharacterSelectionEventArgs
            {
                SelectionMode = control.SelectionMode,
                Action = CharacterSelectionAction.ModeChanged,
                Timestamp = DateTime.Now
            });
        }
    }

    #endregion

    #region Selection Mode Management

    private void UpdateSelectionModeVisibility()
    {
        _characterGrid.Visibility = SelectionMode == CharacterSelectionMode.Grid ? Visibility.Visible : Visibility.Collapsed;
        _characterList.Visibility = SelectionMode == CharacterSelectionMode.List ? Visibility.Visible : Visibility.Collapsed;
        _characterViewport.Visibility = SelectionMode == CharacterSelectionMode.Carousel ? Visibility.Visible : Visibility.Collapsed;
    }

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode => _isSimplifiedMode;

    public void EnterSimplifiedMode()
    {
        _isSimplifiedMode = true;
        CleanupAllParticles();
        EnableCharacterAnimations = false;
        EnableParticleEffects = false;
        Enable3DProjection = false;
    }

    public void ExitSimplifiedMode()
    {
        _isSimplifiedMode = false;
        EnableCharacterAnimations = true;
        EnableParticleEffects = true;
        Enable3DProjection = true;
    }

    #endregion

    #region Color Scheme Application

    private void ApplyEVEColorScheme()
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        
        if (BorderBrush is SolidColorBrush borderBrush)
        {
            borderBrush.Color = Color.FromArgb(100, colors.Primary.R, colors.Primary.G, colors.Primary.B);
        }

        if (Background is SolidColorBrush backgroundBrush)
        {
            backgroundBrush.Color = Color.FromArgb(25, colors.Primary.R, colors.Primary.G, colors.Primary.B);
        }

        if (Effect is DropShadowEffect shadow)
        {
            shadow.Color = Color.FromArgb(80, colors.Primary.R, colors.Primary.G, colors.Primary.B);
        }
    }

    private void ApplyHolographicIntensity()
    {
        var intensity = HolographicIntensity;
        
        if (Effect is DropShadowEffect shadow)
        {
            shadow.BlurRadius = 15 * intensity;
            shadow.ShadowDepth = 5 * intensity;
        }

        EnableCharacterAnimations = intensity > 0.3;
        EnableParticleEffects = intensity > 0.5;
    }

    #endregion
}

#region Supporting Classes and Enums

public class HoloCharacterInfo
{
    public string CharacterId { get; set; }
    public string Name { get; set; }
    public string CorporationName { get; set; }
    public string AllianceName { get; set; }
    public long SkillPoints { get; set; }
    public BitmapImage Portrait { get; set; }
    public BitmapImage CorporationLogo { get; set; }
    public BitmapImage AllianceLogo { get; set; }
    public double Security { get; set; }
    public string Location { get; set; }
    public DateTime LastLogin { get; set; }
    public bool IsOmega { get; set; }
    public string Race { get; set; }
    public string Bloodline { get; set; }
}

public class HoloCharacterProjection
{
    public HoloCharacterInfo Character { get; set; }
    public Point3D Position { get; set; }
    public ModelVisual3D Visual { get; set; }
    public GeometryModel3D Model { get; set; }
}

public enum CharacterSelectionMode
{
    Grid,
    List,
    Carousel
}

public enum CharacterSelectionAction
{
    Selected,
    Hovered,
    Confirmed,
    AddRequested,
    RemoveRequested,
    ModeChanged
}

public class HoloCharacterSelectionEventArgs : EventArgs
{
    public HoloCharacterInfo Character { get; set; }
    public CharacterSelectionMode SelectionMode { get; set; }
    public CharacterSelectionAction Action { get; set; }
    public DateTime Timestamp { get; set; }
}

#endregion