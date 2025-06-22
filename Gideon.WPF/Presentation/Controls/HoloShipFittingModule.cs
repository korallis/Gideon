// ==========================================================================
// HoloShipFittingModule.cs - Holographic Ship Fitting Module Interface
// ==========================================================================
// Main ship fitting module featuring holographic ship display, module slots,
// real-time statistics, and EVE-style ship fitting interface.
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Holographic ship fitting module with 3D ship display and real-time fitting interface
/// </summary>
public class HoloShipFittingModule : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloShipFittingModule),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloShipFittingModule),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty CurrentShipProperty =
        DependencyProperty.Register(nameof(CurrentShip), typeof(HoloShipInfo), typeof(HoloShipFittingModule),
            new PropertyMetadata(null, OnCurrentShipChanged));

    public static readonly DependencyProperty FittingModeProperty =
        DependencyProperty.Register(nameof(FittingMode), typeof(ShipFittingMode), typeof(HoloShipFittingModule),
            new PropertyMetadata(ShipFittingMode.Standard, OnFittingModeChanged));

    public static readonly DependencyProperty ShowShipStatsProperty =
        DependencyProperty.Register(nameof(ShowShipStats), typeof(bool), typeof(HoloShipFittingModule),
            new PropertyMetadata(true));

    public static readonly DependencyProperty Enable3DShipViewProperty =
        DependencyProperty.Register(nameof(Enable3DShipView), typeof(bool), typeof(HoloShipFittingModule),
            new PropertyMetadata(true, OnEnable3DShipViewChanged));

    public static readonly DependencyProperty EnableModuleAnimationsProperty =
        DependencyProperty.Register(nameof(EnableModuleAnimations), typeof(bool), typeof(HoloShipFittingModule),
            new PropertyMetadata(true, OnEnableModuleAnimationsChanged));

    public static readonly DependencyProperty ModuleLibraryProperty =
        DependencyProperty.Register(nameof(ModuleLibrary), typeof(ObservableCollection<HoloModuleInfo>), typeof(HoloShipFittingModule),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ShipLibraryProperty =
        DependencyProperty.Register(nameof(ShipLibrary), typeof(ObservableCollection<HoloShipInfo>), typeof(HoloShipFittingModule),
            new PropertyMetadata(null));

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

    public HoloShipInfo CurrentShip
    {
        get => (HoloShipInfo)GetValue(CurrentShipProperty);
        set => SetValue(CurrentShipProperty, value);
    }

    public ShipFittingMode FittingMode
    {
        get => (ShipFittingMode)GetValue(FittingModeProperty);
        set => SetValue(FittingModeProperty, value);
    }

    public bool ShowShipStats
    {
        get => (bool)GetValue(ShowShipStatsProperty);
        set => SetValue(ShowShipStatsProperty, value);
    }

    public bool Enable3DShipView
    {
        get => (bool)GetValue(Enable3DShipViewProperty);
        set => SetValue(Enable3DShipViewProperty, value);
    }

    public bool EnableModuleAnimations
    {
        get => (bool)GetValue(EnableModuleAnimationsProperty);
        set => SetValue(EnableModuleAnimationsProperty, value);
    }

    public ObservableCollection<HoloModuleInfo> ModuleLibrary
    {
        get => (ObservableCollection<HoloModuleInfo>)GetValue(ModuleLibraryProperty);
        set => SetValue(ModuleLibraryProperty, value);
    }

    public ObservableCollection<HoloShipInfo> ShipLibrary
    {
        get => (ObservableCollection<HoloShipInfo>)GetValue(ShipLibraryProperty);
        set => SetValue(ShipLibraryProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloShipFittingEventArgs> ShipChanged;
    public event EventHandler<HoloShipFittingEventArgs> ModuleFitted;
    public event EventHandler<HoloShipFittingEventArgs> ModuleRemoved;
    public event EventHandler<HoloShipFittingEventArgs> FittingSaved;
    public event EventHandler<HoloShipFittingEventArgs> FittingLoaded;
    public event EventHandler<HoloShipFittingEventArgs> SlotClicked;

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode { get; private set; }

    public void SwitchToFullMode()
    {
        IsInSimplifiedMode = false;
        Enable3DShipView = true;
        EnableModuleAnimations = true;
        ShowShipStats = true;
        UpdateModuleAppearance();
    }

    public void SwitchToSimplifiedMode()
    {
        IsInSimplifiedMode = true;
        Enable3DShipView = false;
        EnableModuleAnimations = false;
        ShowShipStats = false;
        UpdateModuleAppearance();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => !_disposed && IsLoaded;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        EnableModuleAnimations = settings.EnabledFeatures.HasFlag(AnimationFeatures.ComplexTransitions);
        Enable3DShipView = settings.EnabledFeatures.HasFlag(AnimationFeatures.ThreeDimensional);
        
        UpdateModuleAppearance();
    }

    #endregion

    #region Fields

    private Grid _mainGrid;
    private Grid _shipViewGrid;
    private Grid _moduleLibraryGrid;
    private Grid _shipStatsGrid;
    private Viewport3D _shipViewport;
    private Border _shipContainer;
    private ItemsControl _moduleSlots;
    private ListBox _moduleLibraryList;
    private StackPanel _shipStatsPanel;
    private Canvas _connectionCanvas;
    private Canvas _effectCanvas;
    private Button _saveButton;
    private Button _loadButton;
    private ComboBox _shipSelector;
    
    private readonly Dictionary<string, HoloModuleSlot> _slotControls = new();
    private readonly List<FittingParticle> _particles = new();
    private readonly List<ConnectionLine> _connections = new();
    private DispatcherTimer _animationTimer;
    private DispatcherTimer _particleTimer;
    private double _animationPhase = 0;
    private double _particlePhase = 0;
    private readonly Random _random = new();
    private bool _disposed = false;
    private HoloModuleInfo _draggedModule;

    #endregion

    #region Constructor

    public HoloShipFittingModule()
    {
        InitializeModule();
        SetupAnimations();
        
        // Register with animation intensity manager
        AnimationIntensityManager.Instance.RegisterTarget(this);
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        
        // Initialize collections
        ModuleLibrary = new ObservableCollection<HoloModuleInfo>();
        ShipLibrary = new ObservableCollection<HoloShipInfo>();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Load a ship fitting configuration
    /// </summary>
    public void LoadFitting(HoloShipFitting fitting)
    {
        if (fitting == null) return;

        CurrentShip = fitting.Ship;
        
        // Clear current modules
        foreach (var slot in _slotControls.Values)
        {
            slot.FittedModule = null;
        }

        // Load fitted modules
        foreach (var fittedModule in fitting.FittedModules)
        {
            if (_slotControls.TryGetValue(fittedModule.SlotId, out var slot))
            {
                slot.FittedModule = fittedModule.Module;
            }
        }

        UpdateShipStats();
        FittingLoaded?.Invoke(this, new HoloShipFittingEventArgs 
        { 
            Ship = CurrentShip,
            Fitting = fitting,
            Timestamp = DateTime.Now
        });
    }

    /// <summary>
    /// Save current fitting configuration
    /// </summary>
    public HoloShipFitting SaveFitting(string fittingName)
    {
        if (CurrentShip == null) return null;

        var fitting = new HoloShipFitting
        {
            Name = fittingName,
            Ship = CurrentShip,
            FittedModules = new List<HoloFittedModule>()
        };

        foreach (var kvp in _slotControls)
        {
            if (kvp.Value.FittedModule != null)
            {
                fitting.FittedModules.Add(new HoloFittedModule
                {
                    SlotId = kvp.Key,
                    Module = kvp.Value.FittedModule
                });
            }
        }

        FittingSaved?.Invoke(this, new HoloShipFittingEventArgs 
        { 
            Ship = CurrentShip,
            Fitting = fitting,
            Timestamp = DateTime.Now
        });

        return fitting;
    }

    /// <summary>
    /// Clear all fitted modules
    /// </summary>
    public void ClearFitting()
    {
        foreach (var slot in _slotControls.Values)
        {
            slot.FittedModule = null;
        }
        UpdateShipStats();
    }

    /// <summary>
    /// Fit a module to a specific slot
    /// </summary>
    public bool FitModule(string slotId, HoloModuleInfo module)
    {
        if (!_slotControls.TryGetValue(slotId, out var slot)) return false;
        if (!IsModuleCompatible(slot.SlotType, module)) return false;

        slot.FittedModule = module;
        UpdateShipStats();
        
        if (EnableModuleAnimations && !IsInSimplifiedMode)
        {
            AnimateModuleFitting(slot);
        }

        ModuleFitted?.Invoke(this, new HoloShipFittingEventArgs 
        { 
            Ship = CurrentShip,
            Module = module,
            SlotId = slotId,
            Timestamp = DateTime.Now
        });

        return true;
    }

    /// <summary>
    /// Remove module from slot
    /// </summary>
    public void RemoveModule(string slotId)
    {
        if (!_slotControls.TryGetValue(slotId, out var slot)) return;
        
        var removedModule = slot.FittedModule;
        slot.FittedModule = null;
        UpdateShipStats();

        ModuleRemoved?.Invoke(this, new HoloShipFittingEventArgs 
        { 
            Ship = CurrentShip,
            Module = removedModule,
            SlotId = slotId,
            Timestamp = DateTime.Now
        });
    }

    #endregion

    #region Private Methods

    private void InitializeModule()
    {
        CreateModuleInterface();
        UpdateModuleAppearance();
    }

    private void CreateModuleInterface()
    {
        // Main grid with 3 columns: Ship View | Module Library | Ship Stats
        _mainGrid = new Grid();
        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Ship view section
        CreateShipViewSection();
        
        // Module library section
        CreateModuleLibrarySection();
        
        // Ship stats section
        CreateShipStatsSection();

        // Effect overlays
        CreateEffectOverlays();

        Content = _mainGrid;
    }

    private void CreateShipViewSection()
    {
        var shipBorder = new Border
        {
            CornerRadius = new CornerRadius(12),
            BorderThickness = new Thickness(2),
            Margin = new Thickness(10),
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(40, 0, 30, 60), 0.0),
                    new GradientStop(Color.FromArgb(20, 0, 20, 40), 1.0)
                }
            }
        };

        _shipViewGrid = new Grid();
        _shipViewGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _shipViewGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        _shipViewGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Ship selector
        var selectorPanel = new StackPanel 
        { 
            Orientation = Orientation.Horizontal, 
            Margin = new Thickness(15, 10, 15, 5),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var selectorLabel = new TextBlock
        {
            Text = "Ship:",
            Foreground = Brushes.White,
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };

        _shipSelector = new ComboBox
        {
            Width = 200,
            Height = 30
        };

        selectorPanel.Children.Add(selectorLabel);
        selectorPanel.Children.Add(_shipSelector);
        Grid.SetRow(selectorPanel, 0);

        // 3D Ship viewport or 2D container
        if (Enable3DShipView)
        {
            _shipViewport = new Viewport3D
            {
                Margin = new Thickness(15)
            };
            Setup3DShipView();
            Grid.SetRow(_shipViewport, 1);
            _shipViewGrid.Children.Add(_shipViewport);
        }
        else
        {
            _shipContainer = new Border
            {
                CornerRadius = new CornerRadius(8),
                Background = new SolidColorBrush(Color.FromArgb(60, 0, 40, 80)),
                Margin = new Thickness(15)
            };
            
            var shipImage = new Image
            {
                Stretch = Stretch.Uniform,
                Margin = new Thickness(20)
            };
            _shipContainer.Child = shipImage;
            Grid.SetRow(_shipContainer, 1);
            _shipViewGrid.Children.Add(_shipContainer);
        }

        // Module slots overlay
        CreateModuleSlots();

        // Action buttons
        var buttonPanel = new StackPanel 
        { 
            Orientation = Orientation.Horizontal, 
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(15, 5, 15, 10)
        };

        _saveButton = new Button
        {
            Content = "Save Fitting",
            Width = 100,
            Height = 30,
            Margin = new Thickness(5),
            Style = CreateHoloButtonStyle()
        };

        _loadButton = new Button
        {
            Content = "Load Fitting",
            Width = 100,
            Height = 30,
            Margin = new Thickness(5),
            Style = CreateHoloButtonStyle()
        };

        var clearButton = new Button
        {
            Content = "Clear All",
            Width = 80,
            Height = 30,
            Margin = new Thickness(5),
            Style = CreateHoloButtonStyle()
        };

        clearButton.Click += (s, e) => ClearFitting();

        buttonPanel.Children.Add(_saveButton);
        buttonPanel.Children.Add(_loadButton);
        buttonPanel.Children.Add(clearButton);
        Grid.SetRow(buttonPanel, 2);

        _shipViewGrid.Children.Add(selectorPanel);
        _shipViewGrid.Children.Add(buttonPanel);
        shipBorder.Child = _shipViewGrid;
        Grid.SetColumn(shipBorder, 0);
        _mainGrid.Children.Add(shipBorder);
    }

    private void CreateModuleLibrarySection()
    {
        var libraryBorder = new Border
        {
            CornerRadius = new CornerRadius(12),
            BorderThickness = new Thickness(2),
            Margin = new Thickness(5, 10, 10, 10),
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(40, 20, 0, 40), 0.0),
                    new GradientStop(Color.FromArgb(20, 15, 0, 30), 1.0)
                }
            }
        };

        _moduleLibraryGrid = new Grid();
        _moduleLibraryGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _moduleLibraryGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var libraryHeader = new TextBlock
        {
            Text = "Module Library",
            Foreground = Brushes.White,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(15, 10, 15, 5)
        };
        Grid.SetRow(libraryHeader, 0);

        _moduleLibraryList = new ListBox
        {
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Margin = new Thickness(15, 5, 15, 15),
            ItemTemplate = CreateModuleItemTemplate()
        };

        var scrollViewer = new ScrollViewer
        {
            Content = _moduleLibraryList,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
        };
        Grid.SetRow(scrollViewer, 1);

        _moduleLibraryGrid.Children.Add(libraryHeader);
        _moduleLibraryGrid.Children.Add(scrollViewer);
        libraryBorder.Child = _moduleLibraryGrid;
        Grid.SetColumn(libraryBorder, 1);
        _mainGrid.Children.Add(libraryBorder);
    }

    private void CreateShipStatsSection()
    {
        var statsBorder = new Border
        {
            CornerRadius = new CornerRadius(12),
            BorderThickness = new Thickness(2),
            Margin = new Thickness(5, 10, 10, 10),
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(40, 0, 40, 20), 0.0),
                    new GradientStop(Color.FromArgb(20, 0, 30, 15), 1.0)
                }
            }
        };

        _shipStatsGrid = new Grid();
        _shipStatsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _shipStatsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var statsHeader = new TextBlock
        {
            Text = "Ship Statistics",
            Foreground = Brushes.White,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(15, 10, 15, 5)
        };
        Grid.SetRow(statsHeader, 0);

        _shipStatsPanel = new StackPanel
        {
            Margin = new Thickness(15, 5, 15, 15)
        };

        var statsScrollViewer = new ScrollViewer
        {
            Content = _shipStatsPanel,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
        };
        Grid.SetRow(statsScrollViewer, 1);

        _shipStatsGrid.Children.Add(statsHeader);
        _shipStatsGrid.Children.Add(statsScrollViewer);
        statsBorder.Child = _shipStatsGrid;
        Grid.SetColumn(statsBorder, 2);
        _mainGrid.Children.Add(statsBorder);
    }

    private void CreateEffectOverlays()
    {
        // Connection canvas for module connections
        _connectionCanvas = new Canvas
        {
            IsHitTestVisible = false,
            ClipToBounds = false
        };
        Grid.SetColumnSpan(_connectionCanvas, 3);
        _mainGrid.Children.Add(_connectionCanvas);

        // Effect canvas for particles
        _effectCanvas = new Canvas
        {
            IsHitTestVisible = false,
            ClipToBounds = false
        };
        Grid.SetColumnSpan(_effectCanvas, 3);
        _mainGrid.Children.Add(_effectCanvas);
    }

    private void Setup3DShipView()
    {
        if (_shipViewport == null) return;

        // Setup camera
        var camera = new PerspectiveCamera
        {
            Position = new Point3D(0, 0, 5),
            LookDirection = new Vector3D(0, 0, -1),
            UpDirection = new Vector3D(0, 1, 0),
            FieldOfView = 45
        };
        _shipViewport.Camera = camera;

        // Setup lighting
        var ambientLight = new AmbientLight(Colors.White, 0.3);
        var directionalLight = new DirectionalLight(Colors.White, new Vector3D(-1, -1, -1));

        var modelGroup = new Model3DGroup();
        modelGroup.Children.Add(ambientLight);
        modelGroup.Children.Add(directionalLight);

        var modelVisual = new ModelVisual3D();
        modelVisual.Content = modelGroup;
        _shipViewport.Children.Add(modelVisual);
    }

    private void CreateModuleSlots()
    {
        // Create module slot overlay
        var slotsCanvas = new Canvas
        {
            Background = Brushes.Transparent
        };

        // Position slots based on ship layout
        CreateShipSlots(slotsCanvas);

        Grid.SetRow(slotsCanvas, 1);
        _shipViewGrid.Children.Add(slotsCanvas);
    }

    private void CreateShipSlots(Canvas canvas)
    {
        // High slots (top)
        for (int i = 0; i < 8; i++)
        {
            var slot = CreateModuleSlot($"high_{i}", ModuleSlotType.High);
            Canvas.SetLeft(slot, 50 + i * 40);
            Canvas.SetTop(slot, 30);
            canvas.Children.Add(slot);
        }

        // Mid slots (middle)
        for (int i = 0; i < 8; i++)
        {
            var slot = CreateModuleSlot($"mid_{i}", ModuleSlotType.Mid);
            Canvas.SetLeft(slot, 50 + i * 40);
            Canvas.SetTop(slot, 100);
            canvas.Children.Add(slot);
        }

        // Low slots (bottom)
        for (int i = 0; i < 8; i++)
        {
            var slot = CreateModuleSlot($"low_{i}", ModuleSlotType.Low);
            Canvas.SetLeft(slot, 50 + i * 40);
            Canvas.SetTop(slot, 170);
            canvas.Children.Add(slot);
        }

        // Rig slots (right side)
        for (int i = 0; i < 3; i++)
        {
            var slot = CreateModuleSlot($"rig_{i}", ModuleSlotType.Rig);
            Canvas.SetLeft(slot, 400);
            Canvas.SetTop(slot, 60 + i * 40);
            canvas.Children.Add(slot);
        }
    }

    private HoloModuleSlot CreateModuleSlot(string slotId, ModuleSlotType slotType)
    {
        var slot = new HoloModuleSlot
        {
            SlotId = slotId,
            SlotType = slotType,
            Width = 32,
            Height = 32,
            HolographicIntensity = HolographicIntensity,
            EVEColorScheme = EVEColorScheme
        };

        slot.SlotClicked += OnSlotClicked;
        slot.ModuleDropped += OnModuleDropped;
        
        _slotControls[slotId] = slot;
        return slot;
    }

    private DataTemplate CreateModuleItemTemplate()
    {
        var template = new DataTemplate();
        
        var border = new FrameworkElementFactory(typeof(Border));
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
        border.SetValue(Border.MarginProperty, new Thickness(2));
        border.SetValue(Border.PaddingProperty, new Thickness(8, 4));
        
        var stackPanel = new FrameworkElementFactory(typeof(StackPanel));
        
        var nameText = new FrameworkElementFactory(typeof(TextBlock));
        nameText.SetBinding(TextBlock.TextProperty, new Binding("Name"));
        nameText.SetValue(TextBlock.ForegroundProperty, Brushes.White);
        nameText.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        
        var typeText = new FrameworkElementFactory(typeof(TextBlock));
        typeText.SetBinding(TextBlock.TextProperty, new Binding("Type"));
        typeText.SetValue(TextBlock.ForegroundProperty, Brushes.LightGray);
        typeText.SetValue(TextBlock.FontSizeProperty, 10.0);
        
        stackPanel.AppendChild(nameText);
        stackPanel.AppendChild(typeText);
        border.AppendChild(stackPanel);
        
        template.VisualTree = border;
        return template;
    }

    private Style CreateHoloButtonStyle()
    {
        var style = new Style(typeof(Button));
        
        style.Setters.Add(new Setter(Button.BackgroundProperty, 
            new SolidColorBrush(Color.FromArgb(100, 64, 224, 255))));
        style.Setters.Add(new Setter(Button.ForegroundProperty, Brushes.White));
        style.Setters.Add(new Setter(Button.BorderBrushProperty, 
            new SolidColorBrush(Color.FromArgb(180, 64, 224, 255))));
        style.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Button.FontWeightProperty, FontWeights.Bold));
        
        return style;
    }

    private void SetupAnimations()
    {
        // Main animation timer
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
        };
        _animationTimer.Tick += OnAnimationTick;

        // Particle animation timer
        _particleTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50) // 20 FPS
        };
        _particleTimer.Tick += OnParticleTick;
    }

    private void OnAnimationTick(object sender, EventArgs e)
    {
        if (!EnableModuleAnimations || IsInSimplifiedMode) return;

        _animationPhase += 0.05;
        if (_animationPhase > Math.PI * 2)
            _animationPhase = 0;

        UpdateConnectionAnimations();
        UpdateSlotAnimations();
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        if (IsInSimplifiedMode || _effectCanvas == null) return;

        _particlePhase += 0.1;
        if (_particlePhase > Math.PI * 2)
            _particlePhase = 0;

        UpdateParticleEffects();
    }

    private void UpdateConnectionAnimations()
    {
        foreach (var connection in _connections)
        {
            if (connection.Visual.Effect is DropShadowEffect effect)
            {
                var intensity = 0.5 + (Math.Sin(_animationPhase + connection.Phase) * 0.3);
                effect.Opacity = HolographicIntensity * intensity;
            }
        }
    }

    private void UpdateSlotAnimations()
    {
        foreach (var slot in _slotControls.Values)
        {
            if (slot.FittedModule != null)
            {
                var pulseIntensity = 0.8 + (Math.Sin(_animationPhase * 2) * 0.2);
                slot.HolographicIntensity = HolographicIntensity * pulseIntensity;
            }
        }
    }

    private void UpdateParticleEffects()
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var particle = _particles[i];
            
            particle.X += particle.VelocityX;
            particle.Y += particle.VelocityY;
            particle.Life -= 0.02;

            Canvas.SetLeft(particle.Visual, particle.X);
            Canvas.SetTop(particle.Visual, particle.Y);
            particle.Visual.Opacity = Math.Max(0, particle.Life) * HolographicIntensity;

            if (particle.Life <= 0 || particle.Y < -10 || particle.Y > ActualHeight + 10)
            {
                _effectCanvas.Children.Remove(particle.Visual);
                _particles.RemoveAt(i);
            }
        }
    }

    private void AnimateModuleFitting(HoloModuleSlot slot)
    {
        // Create fitting animation particles
        for (int i = 0; i < 5; i++)
        {
            SpawnFittingParticle(slot);
        }
        
        // Animate slot highlighting
        var scaleAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 1.2,
            Duration = TimeSpan.FromMilliseconds(200),
            AutoReverse = true
        };
        
        var scaleTransform = new ScaleTransform();
        slot.RenderTransform = scaleTransform;
        slot.RenderTransformOrigin = new Point(0.5, 0.5);
        
        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
    }

    private void SpawnFittingParticle(HoloModuleSlot slot)
    {
        if (_effectCanvas == null) return;

        var color = GetEVEColor(EVEColorScheme);
        var size = 2 + _random.NextDouble() * 2;
        
        var ellipse = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromArgb(180, color.R, color.G, color.B))
        };

        var slotPoint = slot.TranslatePoint(new Point(slot.Width / 2, slot.Height / 2), _effectCanvas);
        
        var particle = new FittingParticle
        {
            Visual = ellipse,
            X = slotPoint.X,
            Y = slotPoint.Y,
            VelocityX = (_random.NextDouble() - 0.5) * 4,
            VelocityY = -2 - _random.NextDouble() * 3,
            Life = 1.0
        };

        Canvas.SetLeft(ellipse, particle.X);
        Canvas.SetTop(ellipse, particle.Y);
        
        _particles.Add(particle);
        _effectCanvas.Children.Add(ellipse);
    }

    private bool IsModuleCompatible(ModuleSlotType slotType, HoloModuleInfo module)
    {
        return module.SlotType == slotType;
    }

    private void UpdateShipStats()
    {
        if (_shipStatsPanel == null || CurrentShip == null) return;

        _shipStatsPanel.Children.Clear();

        // Calculate stats based on current fitting
        var stats = CalculateShipStats();

        foreach (var stat in stats)
        {
            var statControl = CreateStatDisplay(stat.Key, stat.Value);
            _shipStatsPanel.Children.Add(statControl);
        }
    }

    private Dictionary<string, string> CalculateShipStats()
    {
        var stats = new Dictionary<string, string>();
        
        if (CurrentShip == null) return stats;

        // Base ship stats
        stats["Hull"] = $"{CurrentShip.BaseHull:F0} HP";
        stats["Capacitor"] = $"{CurrentShip.BaseCapacitor:F0} GJ";
        stats["Cargo"] = $"{CurrentShip.CargoCapacity:F0} m³";
        
        // Calculate fitted stats
        var totalDPS = 0.0;
        var totalCPU = 0.0;
        var totalPowerGrid = 0.0;

        foreach (var slot in _slotControls.Values)
        {
            if (slot.FittedModule != null)
            {
                totalDPS += slot.FittedModule.DPS;
                totalCPU += slot.FittedModule.CPUUsage;
                totalPowerGrid += slot.FittedModule.PowerUsage;
            }
        }

        stats["DPS"] = $"{totalDPS:F1}";
        stats["CPU Used"] = $"{totalCPU:F0} / {CurrentShip.CPUCapacity:F0}";
        stats["Power Used"] = $"{totalPowerGrid:F0} / {CurrentShip.PowerCapacity:F0}";

        return stats;
    }

    private UIElement CreateStatDisplay(string name, string value)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8, 4),
            Margin = new Thickness(0, 2)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var nameText = new TextBlock
        {
            Text = name,
            Foreground = Brushes.White,
            FontWeight = FontWeights.Bold
        };
        Grid.SetColumn(nameText, 0);

        var valueText = new TextBlock
        {
            Text = value,
            Foreground = new SolidColorBrush(GetEVEColor(EVEColorScheme)),
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        Grid.SetColumn(valueText, 1);

        grid.Children.Add(nameText);
        grid.Children.Add(valueText);
        border.Child = grid;

        return border;
    }

    private void UpdateModuleAppearance()
    {
        foreach (var slot in _slotControls.Values)
        {
            slot.HolographicIntensity = HolographicIntensity;
            slot.EVEColorScheme = EVEColorScheme;
        }

        UpdateColors();
        UpdateEffects();
    }

    private void UpdateColors()
    {
        var color = GetEVEColor(EVEColorScheme);

        // Update border colors
        foreach (Border border in _mainGrid.Children.OfType<Border>())
        {
            border.BorderBrush = new SolidColorBrush(Color.FromArgb(180, color.R, color.G, color.B));
        }
    }

    private void UpdateEffects()
    {
        if (IsInSimplifiedMode) return;

        var color = GetEVEColor(EVEColorScheme);

        foreach (Border border in _mainGrid.Children.OfType<Border>())
        {
            border.Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 8 * HolographicIntensity,
                ShadowDepth = 0,
                Opacity = 0.4 * HolographicIntensity
            };
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

    private void OnSlotClicked(object sender, HoloModuleSlotEventArgs e)
    {
        SlotClicked?.Invoke(this, new HoloShipFittingEventArgs 
        { 
            SlotId = e.SlotId,
            SlotType = e.SlotType,
            Module = e.FittedModule,
            Timestamp = DateTime.Now
        });
    }

    private void OnModuleDropped(object sender, HoloModuleSlotEventArgs e)
    {
        if (_draggedModule != null)
        {
            FitModule(e.SlotId, _draggedModule);
            _draggedModule = null;
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableModuleAnimations && !IsInSimplifiedMode)
            _animationTimer.Start();
            
        if (!IsInSimplifiedMode)
            _particleTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        _particleTimer?.Stop();
        
        // Clean up particles and connections
        _particles.Clear();
        _connections.Clear();
        _effectCanvas?.Children.Clear();
        _connectionCanvas?.Children.Clear();
        
        // Unregister from animation intensity manager
        AnimationIntensityManager.Instance.UnregisterTarget(this);
        
        _disposed = true;
    }

    #endregion

    #region Event Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipFittingModule module)
            module.UpdateModuleAppearance();
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipFittingModule module)
            module.UpdateModuleAppearance();
    }

    private static void OnCurrentShipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipFittingModule module)
        {
            module.UpdateShipStats();
            module.ShipChanged?.Invoke(module, new HoloShipFittingEventArgs 
            { 
                Ship = (HoloShipInfo)e.NewValue,
                Timestamp = DateTime.Now
            });
        }
    }

    private static void OnFittingModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Handle fitting mode changes
    }

    private static void OnEnable3DShipViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipFittingModule module)
        {
            // Recreate ship view section with new mode
            // This would require rebuilding the interface
        }
    }

    private static void OnEnableModuleAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloShipFittingModule module)
        {
            if ((bool)e.NewValue && !module.IsInSimplifiedMode)
                module._animationTimer.Start();
            else
                module._animationTimer.Stop();
        }
    }

    #endregion
}

/// <summary>
/// Module slot control for ship fitting
/// </summary>
public class HoloModuleSlot : Control, IAnimationIntensityTarget
{
    public string SlotId { get; set; }
    public ModuleSlotType SlotType { get; set; }
    public HoloModuleInfo FittedModule { get; set; }
    public double HolographicIntensity { get; set; } = 1.0;
    public EVEColorScheme EVEColorScheme { get; set; } = EVEColorScheme.ElectricBlue;
    public bool IsValid => IsLoaded;

    public event EventHandler<HoloModuleSlotEventArgs> SlotClicked;
    public event EventHandler<HoloModuleSlotEventArgs> ModuleDropped;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings != null)
        {
            HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        }
    }
}

/// <summary>
/// Particle for fitting effects
/// </summary>
internal class FittingParticle
{
    public FrameworkElement Visual { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
}

/// <summary>
/// Connection line between modules
/// </summary>
internal class ConnectionLine
{
    public Line Visual { get; set; }
    public double Phase { get; set; }
}

/// <summary>
/// Ship information for holographic display
/// </summary>
public class HoloShipInfo
{
    public string Name { get; set; }
    public string Type { get; set; }
    public double BaseHull { get; set; }
    public double BaseCapacitor { get; set; }
    public double CargoCapacity { get; set; }
    public double CPUCapacity { get; set; }
    public double PowerCapacity { get; set; }
    public string ImagePath { get; set; }
}

/// <summary>
/// Module information for fitting
/// </summary>
public class HoloModuleInfo
{
    public string Name { get; set; }
    public string Type { get; set; }
    public ModuleSlotType SlotType { get; set; }
    public double DPS { get; set; }
    public double CPUUsage { get; set; }
    public double PowerUsage { get; set; }
    public string Description { get; set; }
}

/// <summary>
/// Ship fitting configuration
/// </summary>
public class HoloShipFitting
{
    public string Name { get; set; }
    public HoloShipInfo Ship { get; set; }
    public List<HoloFittedModule> FittedModules { get; set; } = new();
}

/// <summary>
/// Fitted module with slot information
/// </summary>
public class HoloFittedModule
{
    public string SlotId { get; set; }
    public HoloModuleInfo Module { get; set; }
}

/// <summary>
/// Module slot types
/// </summary>
public enum ModuleSlotType
{
    High,
    Mid,
    Low,
    Rig,
    Subsystem
}

/// <summary>
/// Ship fitting modes
/// </summary>
public enum ShipFittingMode
{
    Standard,
    Theory,
    Simulation
}

/// <summary>
/// Event args for ship fitting events
/// </summary>
public class HoloShipFittingEventArgs : EventArgs
{
    public HoloShipInfo Ship { get; set; }
    public HoloModuleInfo Module { get; set; }
    public HoloShipFitting Fitting { get; set; }
    public string SlotId { get; set; }
    public ModuleSlotType SlotType { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Event args for module slot events
/// </summary>
public class HoloModuleSlotEventArgs : EventArgs
{
    public string SlotId { get; set; }
    public ModuleSlotType SlotType { get; set; }
    public HoloModuleInfo FittedModule { get; set; }
    public DateTime Timestamp { get; set; }
}