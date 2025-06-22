// ==========================================================================
// HoloFittingSlotManager.cs - Holographic Fitting Slot Management System
// ==========================================================================
// Advanced fitting slot management featuring holographic slot visualization,
// animated module installation, constraint validation, and EVE-style slot interactions.
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
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Holographic fitting slot management system with animated installation and constraint validation
/// </summary>
public class HoloFittingSlotManager : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloFittingSlotManager),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloFittingSlotManager),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty ShipDataProperty =
        DependencyProperty.Register(nameof(ShipData), typeof(HoloShipData), typeof(HoloFittingSlotManager),
            new PropertyMetadata(null, OnShipDataChanged));

    public static readonly DependencyProperty FittingSlotsProperty =
        DependencyProperty.Register(nameof(FittingSlots), typeof(ObservableCollection<HoloFittingSlot>), typeof(HoloFittingSlotManager),
            new PropertyMetadata(null, OnFittingSlotsChanged));

    public static readonly DependencyProperty SelectedSlotProperty =
        DependencyProperty.Register(nameof(SelectedSlot), typeof(HoloFittingSlot), typeof(HoloFittingSlotManager),
            new PropertyMetadata(null, OnSelectedSlotChanged));

    public static readonly DependencyProperty SlotLayoutModeProperty =
        DependencyProperty.Register(nameof(SlotLayoutMode), typeof(SlotLayoutMode), typeof(HoloFittingSlotManager),
            new PropertyMetadata(SlotLayoutMode.ShipSilhouette, OnSlotLayoutModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloFittingSlotManager),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloFittingSlotManager),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowSlotConstraintsProperty =
        DependencyProperty.Register(nameof(ShowSlotConstraints), typeof(bool), typeof(HoloFittingSlotManager),
            new PropertyMetadata(true, OnShowSlotConstraintsChanged));

    public static readonly DependencyProperty EnableSnapFittingProperty =
        DependencyProperty.Register(nameof(EnableSnapFitting), typeof(bool), typeof(HoloFittingSlotManager),
            new PropertyMetadata(true, OnEnableSnapFittingChanged));

    public static readonly DependencyProperty ShowSlotNumbersProperty =
        DependencyProperty.Register(nameof(ShowSlotNumbers), typeof(bool), typeof(HoloFittingSlotManager),
            new PropertyMetadata(true, OnShowSlotNumbersChanged));

    public static readonly DependencyProperty AutoValidateConstraintsProperty =
        DependencyProperty.Register(nameof(AutoValidateConstraints), typeof(bool), typeof(HoloFittingSlotManager),
            new PropertyMetadata(true, OnAutoValidateConstraintsChanged));

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

    public ObservableCollection<HoloFittingSlot> FittingSlots
    {
        get => (ObservableCollection<HoloFittingSlot>)GetValue(FittingSlotsProperty);
        set => SetValue(FittingSlotsProperty, value);
    }

    public HoloFittingSlot SelectedSlot
    {
        get => (HoloFittingSlot)GetValue(SelectedSlotProperty);
        set => SetValue(SelectedSlotProperty, value);
    }

    public SlotLayoutMode SlotLayoutMode
    {
        get => (SlotLayoutMode)GetValue(SlotLayoutModeProperty);
        set => SetValue(SlotLayoutModeProperty, value);
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

    public bool ShowSlotConstraints
    {
        get => (bool)GetValue(ShowSlotConstraintsProperty);
        set => SetValue(ShowSlotConstraintsProperty, value);
    }

    public bool EnableSnapFitting
    {
        get => (bool)GetValue(EnableSnapFittingProperty);
        set => SetValue(EnableSnapFittingProperty, value);
    }

    public bool ShowSlotNumbers
    {
        get => (bool)GetValue(ShowSlotNumbersProperty);
        set => SetValue(ShowSlotNumbersProperty, value);
    }

    public bool AutoValidateConstraints
    {
        get => (bool)GetValue(AutoValidateConstraintsProperty);
        set => SetValue(AutoValidateConstraintsProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<SlotModuleChangedEventArgs> SlotModuleChanged;
    public event EventHandler<SlotValidationEventArgs> SlotValidationFailed;
    public event EventHandler<SlotInteractionEventArgs> SlotInteraction;

    #endregion

    #region Private Fields

    private Canvas _slotCanvas;
    private Canvas _particleCanvas;
    private Canvas _constraintCanvas;
    private Grid _rootGrid;
    private readonly Dictionary<string, FrameworkElement> _slotVisuals = new();
    private readonly Dictionary<string, List<UIElement>> _slotParticles = new();
    private readonly Dictionary<string, ConstraintValidator> _slotValidators = new();
    private readonly List<UIElement> _particles = new();
    private DispatcherTimer _animationTimer;
    private DispatcherTimer _constraintTimer;
    private HoloFittingSlot _draggedSlot;
    private Point _dragOffset;
    private readonly Random _random = new();

    #endregion

    #region Constructor

    public HoloFittingSlotManager()
    {
        InitializeComponent();
        InitializeSlotSystem();
        InitializeAnimationSystem();
        InitializeConstraintSystem();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 800;
        Height = 600;
        Background = Brushes.Transparent;

        _rootGrid = new Grid();
        _rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        CreateSlotCanvas();
        CreateConstraintCanvas();
        CreateParticleSystem();

        Content = _rootGrid;
    }

    private void CreateSlotCanvas()
    {
        _slotCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            ClipToBounds = false
        };

        Grid.SetRow(_slotCanvas, 0);
        _rootGrid.Children.Add(_slotCanvas);
    }

    private void CreateConstraintCanvas()
    {
        _constraintCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false,
            ClipToBounds = false
        };

        Grid.SetRow(_constraintCanvas, 0);
        _rootGrid.Children.Add(_constraintCanvas);
    }

    private void CreateParticleSystem()
    {
        _particleCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false,
            ClipToBounds = false
        };

        Grid.SetRowSpan(_particleCanvas, 1);
        _rootGrid.Children.Add(_particleCanvas);
    }

    private void InitializeSlotSystem()
    {
        if (FittingSlots == null)
        {
            FittingSlots = new ObservableCollection<HoloFittingSlot>();
        }

        FittingSlots.CollectionChanged += FittingSlots_CollectionChanged;
    }

    private void InitializeAnimationSystem()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        _animationTimer.Tick += AnimationTimer_Tick;
    }

    private void InitializeConstraintSystem()
    {
        _constraintTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _constraintTimer.Tick += ConstraintTimer_Tick;
    }

    #endregion

    #region Slot Management

    private void FittingSlots_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (HoloFittingSlot slot in e.NewItems)
            {
                CreateSlotVisual(slot);
                InitializeSlotValidator(slot);
            }
        }

        if (e.OldItems != null)
        {
            foreach (HoloFittingSlot slot in e.OldItems)
            {
                RemoveSlotVisual(slot);
                RemoveSlotValidator(slot);
            }
        }

        UpdateSlotLayout();
    }

    private void CreateSlotVisual(HoloFittingSlot slot)
    {
        var slotContainer = new Grid
        {
            Width = 80,
            Height = 80,
            Background = Brushes.Transparent,
            Tag = slot
        };

        // Main slot circle
        var slotCircle = new Ellipse
        {
            Width = 60,
            Height = 60,
            Stroke = GetBrushForSlotType(slot.SlotType),
            StrokeThickness = 2,
            Fill = CreateSlotBackground(slot),
            Effect = CreateGlowEffect(0.8)
        };

        // Slot number
        var slotNumber = new TextBlock
        {
            Text = slot.SlotIndex.ToString(),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = GetBrushForSlotType(slot.SlotType),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 5, 0, 0),
            Visibility = ShowSlotNumbers ? Visibility.Visible : Visibility.Collapsed
        };

        // Module visual (if installed)
        var moduleVisual = CreateModuleVisual(slot.InstalledModule);
        
        // Constraint indicator
        var constraintIndicator = CreateConstraintIndicator(slot);

        slotContainer.Children.Add(slotCircle);
        slotContainer.Children.Add(slotNumber);
        if (moduleVisual != null)
            slotContainer.Children.Add(moduleVisual);
        slotContainer.Children.Add(constraintIndicator);

        // Event handlers
        slotContainer.MouseEnter += (s, e) => OnSlotMouseEnter(slot, s as FrameworkElement);
        slotContainer.MouseLeave += (s, e) => OnSlotMouseLeave(slot, s as FrameworkElement);
        slotContainer.MouseLeftButtonDown += (s, e) => OnSlotMouseDown(slot, s as FrameworkElement, e);
        slotContainer.MouseLeftButtonUp += (s, e) => OnSlotMouseUp(slot, s as FrameworkElement, e);
        slotContainer.MouseMove += (s, e) => OnSlotMouseMove(slot, s as FrameworkElement, e);

        // Allow drop
        slotContainer.AllowDrop = true;
        slotContainer.DragEnter += (s, e) => OnSlotDragEnter(slot, e);
        slotContainer.DragOver += (s, e) => OnSlotDragOver(slot, e);
        slotContainer.DragLeave += (s, e) => OnSlotDragLeave(slot, e);
        slotContainer.Drop += (s, e) => OnSlotDrop(slot, e);

        _slotVisuals[slot.SlotId] = slotContainer;
        _slotCanvas.Children.Add(slotContainer);

        // Initial animation
        if (EnableAnimations)
        {
            AnimateSlotAppearance(slotContainer);
        }
    }

    private void RemoveSlotVisual(HoloFittingSlot slot)
    {
        if (_slotVisuals.TryGetValue(slot.SlotId, out var visual))
        {
            if (EnableAnimations)
            {
                AnimateSlotDisappearance(visual, () =>
                {
                    _slotCanvas.Children.Remove(visual);
                    _slotVisuals.Remove(slot.SlotId);
                });
            }
            else
            {
                _slotCanvas.Children.Remove(visual);
                _slotVisuals.Remove(slot.SlotId);
            }
        }

        RemoveSlotParticles(slot.SlotId);
    }

    private FrameworkElement CreateModuleVisual(HoloModuleData module)
    {
        if (module == null) return null;

        var moduleContainer = new Grid
        {
            Width = 40,
            Height = 40,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        // Module icon background
        var moduleBackground = new Ellipse
        {
            Width = 40,
            Height = 40,
            Fill = CreateModuleBackground(module),
            Stroke = GetBrushForModuleType(module.Category),
            StrokeThickness = 1,
            Effect = CreateGlowEffect(0.6)
        };

        // Module icon (simplified representation)
        var moduleIcon = new Path
        {
            Data = GetGeometryForModuleType(module.Category),
            Fill = GetBrushForModuleType(module.Category),
            Width = 20,
            Height = 20,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Effect = CreateGlowEffect(0.4)
        };

        moduleContainer.Children.Add(moduleBackground);
        moduleContainer.Children.Add(moduleIcon);

        return moduleContainer;
    }

    private FrameworkElement CreateConstraintIndicator(HoloFittingSlot slot)
    {
        var indicator = new Ellipse
        {
            Width = 8,
            Height = 8,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 5, 5),
            Visibility = ShowSlotConstraints ? Visibility.Visible : Visibility.Collapsed
        };

        UpdateConstraintIndicator(slot, indicator);
        return indicator;
    }

    #endregion

    #region Slot Layout

    private void UpdateSlotLayout()
    {
        if (FittingSlots == null || _slotCanvas == null) return;

        switch (SlotLayoutMode)
        {
            case SlotLayoutMode.ShipSilhouette:
                LayoutSlotsOnShipSilhouette();
                break;
            case SlotLayoutMode.Grid:
                LayoutSlotsInGrid();
                break;
            case SlotLayoutMode.Circle:
                LayoutSlotsInCircle();
                break;
            case SlotLayoutMode.Radial:
                LayoutSlotsRadially();
                break;
        }
    }

    private void LayoutSlotsOnShipSilhouette()
    {
        var centerX = _slotCanvas.ActualWidth / 2;
        var centerY = _slotCanvas.ActualHeight / 2;

        // Group slots by type
        var highSlots = FittingSlots.Where(s => s.SlotType == SlotType.High).ToList();
        var midSlots = FittingSlots.Where(s => s.SlotType == SlotType.Mid).ToList();
        var lowSlots = FittingSlots.Where(s => s.SlotType == SlotType.Low).ToList();
        var rigSlots = FittingSlots.Where(s => s.SlotType == SlotType.Rig).ToList();

        // Position high slots (top/front of ship)
        PositionSlotGroup(highSlots, centerX, centerY - 100, 200, 0);

        // Position mid slots (middle of ship)
        PositionSlotGroup(midSlots, centerX - 50, centerY, 100, Math.PI / 2);

        // Position low slots (bottom/rear of ship)
        PositionSlotGroup(lowSlots, centerX, centerY + 100, 200, Math.PI);

        // Position rig slots (sides of ship)
        PositionSlotGroup(rigSlots, centerX + 150, centerY, 150, -Math.PI / 2);
    }

    private void LayoutSlotsInGrid()
    {
        var columns = Math.Ceiling(Math.Sqrt(FittingSlots.Count));
        var slotSize = 80;
        var spacing = 20;

        for (int i = 0; i < FittingSlots.Count; i++)
        {
            var slot = FittingSlots[i];
            if (_slotVisuals.TryGetValue(slot.SlotId, out var visual))
            {
                var row = Math.Floor(i / columns);
                var col = i % columns;

                Canvas.SetLeft(visual, col * (slotSize + spacing) + spacing);
                Canvas.SetTop(visual, row * (slotSize + spacing) + spacing);
            }
        }
    }

    private void LayoutSlotsInCircle()
    {
        var centerX = _slotCanvas.ActualWidth / 2;
        var centerY = _slotCanvas.ActualHeight / 2;
        var radius = Math.Min(centerX, centerY) - 100;

        for (int i = 0; i < FittingSlots.Count; i++)
        {
            var slot = FittingSlots[i];
            if (_slotVisuals.TryGetValue(slot.SlotId, out var visual))
            {
                var angle = (2 * Math.PI * i) / FittingSlots.Count;
                var x = centerX + radius * Math.Cos(angle) - 40;
                var y = centerY + radius * Math.Sin(angle) - 40;

                Canvas.SetLeft(visual, x);
                Canvas.SetTop(visual, y);
            }
        }
    }

    private void LayoutSlotsRadially()
    {
        var centerX = _slotCanvas.ActualWidth / 2;
        var centerY = _slotCanvas.ActualHeight / 2;

        // Group by slot type and arrange in concentric circles
        var slotGroups = FittingSlots.GroupBy(s => s.SlotType).ToList();
        var baseRadius = 80;

        for (int groupIndex = 0; groupIndex < slotGroups.Count; groupIndex++)
        {
            var group = slotGroups[groupIndex];
            var radius = baseRadius + (groupIndex * 60);
            var slots = group.ToList();

            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (_slotVisuals.TryGetValue(slot.SlotId, out var visual))
                {
                    var angle = (2 * Math.PI * i) / slots.Count;
                    var x = centerX + radius * Math.Cos(angle) - 40;
                    var y = centerY + radius * Math.Sin(angle) - 40;

                    Canvas.SetLeft(visual, x);
                    Canvas.SetTop(visual, y);
                }
            }
        }
    }

    private void PositionSlotGroup(List<HoloFittingSlot> slots, double centerX, double centerY, double width, double rotationOffset)
    {
        if (slots.Count == 0) return;

        var spacing = width / Math.Max(slots.Count - 1, 1);

        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (_slotVisuals.TryGetValue(slot.SlotId, out var visual))
            {
                var offset = (i * spacing) - (width / 2);
                var x = centerX + offset * Math.Cos(rotationOffset) - 40;
                var y = centerY + offset * Math.Sin(rotationOffset) - 40;

                Canvas.SetLeft(visual, x);
                Canvas.SetTop(visual, y);
            }
        }
    }

    #endregion

    #region Constraint Validation

    private void InitializeSlotValidator(HoloFittingSlot slot)
    {
        _slotValidators[slot.SlotId] = new ConstraintValidator(slot, ShipData);
    }

    private void RemoveSlotValidator(HoloFittingSlot slot)
    {
        _slotValidators.Remove(slot.SlotId);
    }

    private void ConstraintTimer_Tick(object sender, EventArgs e)
    {
        if (!AutoValidateConstraints) return;

        foreach (var slot in FittingSlots)
        {
            ValidateSlotConstraints(slot);
        }
    }

    private void ValidateSlotConstraints(HoloFittingSlot slot)
    {
        if (!_slotValidators.TryGetValue(slot.SlotId, out var validator)) return;

        var result = validator.Validate();
        UpdateConstraintVisualization(slot, result);

        if (!result.IsValid && result.Violations.Any())
        {
            SlotValidationFailed?.Invoke(this, new SlotValidationEventArgs(slot, result));
        }
    }

    private void UpdateConstraintVisualization(HoloFittingSlot slot, ValidationResult result)
    {
        if (!_slotVisuals.TryGetValue(slot.SlotId, out var visual)) return;

        // Update constraint indicator
        var indicator = visual.FindName("ConstraintIndicator") as Ellipse;
        if (indicator != null)
        {
            UpdateConstraintIndicator(slot, indicator, result);
        }

        // Update slot border color based on validation
        var slotCircle = ((Grid)visual).Children.OfType<Ellipse>().FirstOrDefault();
        if (slotCircle != null)
        {
            slotCircle.Stroke = result.IsValid 
                ? GetBrushForSlotType(slot.SlotType)
                : new SolidColorBrush(Colors.Red);
        }
    }

    private void UpdateConstraintIndicator(HoloFittingSlot slot, Ellipse indicator, ValidationResult result = null)
    {
        if (result == null && _slotValidators.TryGetValue(slot.SlotId, out var validator))
        {
            result = validator.Validate();
        }

        if (result != null)
        {
            indicator.Fill = result.IsValid 
                ? new SolidColorBrush(Colors.Green)
                : new SolidColorBrush(Colors.Red);
            
            indicator.Effect = CreateGlowEffect(result.IsValid ? 0.6 : 1.0);
        }
        else
        {
            indicator.Fill = new SolidColorBrush(Colors.Gray);
            indicator.Effect = CreateGlowEffect(0.3);
        }
    }

    #endregion

    #region Mouse and Drag Handling

    private void OnSlotMouseEnter(HoloFittingSlot slot, FrameworkElement visual)
    {
        if (EnableAnimations)
        {
            AnimateSlotHover(visual, true);
        }

        if (EnableParticleEffects)
        {
            CreateSlotHoverParticles(slot);
        }

        SlotInteraction?.Invoke(this, new SlotInteractionEventArgs(slot, SlotInteractionType.Hover, true));
    }

    private void OnSlotMouseLeave(HoloFittingSlot slot, FrameworkElement visual)
    {
        if (EnableAnimations)
        {
            AnimateSlotHover(visual, false);
        }

        RemoveSlotParticles(slot.SlotId);
        SlotInteraction?.Invoke(this, new SlotInteractionEventArgs(slot, SlotInteractionType.Hover, false));
    }

    private void OnSlotMouseDown(HoloFittingSlot slot, FrameworkElement visual, MouseButtonEventArgs e)
    {
        if (slot.InstalledModule != null)
        {
            _draggedSlot = slot;
            _dragOffset = e.GetPosition(visual);
            visual.CaptureMouse();

            if (EnableAnimations)
            {
                AnimateSlotPress(visual);
            }
        }

        SelectedSlot = slot;
        SlotInteraction?.Invoke(this, new SlotInteractionEventArgs(slot, SlotInteractionType.Select, true));
    }

    private void OnSlotMouseUp(HoloFittingSlot slot, FrameworkElement visual, MouseButtonEventArgs e)
    {
        if (_draggedSlot == slot)
        {
            _draggedSlot = null;
            visual.ReleaseMouseCapture();

            if (EnableAnimations)
            {
                AnimateSlotRelease(visual);
            }
        }
    }

    private void OnSlotMouseMove(HoloFittingSlot slot, FrameworkElement visual, MouseEventArgs e)
    {
        if (_draggedSlot == slot && e.LeftButton == MouseButtonState.Pressed)
        {
            var position = e.GetPosition(_slotCanvas);
            Canvas.SetLeft(visual, position.X - _dragOffset.X);
            Canvas.SetTop(visual, position.Y - _dragOffset.Y);

            // Check for snap targets
            if (EnableSnapFitting)
            {
                CheckSnapTargets(slot, position);
            }
        }
    }

    private void OnSlotDragEnter(HoloFittingSlot slot, DragEventArgs e)
    {
        if (CanAcceptModule(slot, e.Data))
        {
            AnimateSlotDropTarget(slot, true);
            e.Effects = DragDropEffects.Move;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
    }

    private void OnSlotDragOver(HoloFittingSlot slot, DragEventArgs e)
    {
        e.Effects = CanAcceptModule(slot, e.Data) ? DragDropEffects.Move : DragDropEffects.None;
    }

    private void OnSlotDragLeave(HoloFittingSlot slot, DragEventArgs e)
    {
        AnimateSlotDropTarget(slot, false);
    }

    private void OnSlotDrop(HoloFittingSlot slot, DragEventArgs e)
    {
        if (e.Data.GetData(typeof(HoloModuleData)) is HoloModuleData module)
        {
            InstallModule(slot, module);
            AnimateSlotDropTarget(slot, false);
        }
    }

    #endregion

    #region Module Installation

    private bool CanAcceptModule(HoloFittingSlot slot, IDataObject data)
    {
        if (data.GetData(typeof(HoloModuleData)) is HoloModuleData module)
        {
            return CanInstallModule(slot, module);
        }
        return false;
    }

    private bool CanInstallModule(HoloFittingSlot slot, HoloModuleData module)
    {
        // Check slot type compatibility
        if (!IsModuleCompatibleWithSlot(module, slot))
            return false;

        // Check ship constraints
        if (_slotValidators.TryGetValue(slot.SlotId, out var validator))
        {
            return validator.CanInstallModule(module);
        }

        return true;
    }

    private bool IsModuleCompatibleWithSlot(HoloModuleData module, HoloFittingSlot slot)
    {
        // Simplified compatibility check - in real implementation this would be more complex
        return module.Category switch
        {
            ModuleCategory.Weapons => slot.SlotType == SlotType.High,
            ModuleCategory.Defense => slot.SlotType == SlotType.Mid || slot.SlotType == SlotType.Low,
            ModuleCategory.Electronics => slot.SlotType == SlotType.Mid,
            ModuleCategory.Engineering => slot.SlotType == SlotType.Low,
            ModuleCategory.Rigs => slot.SlotType == SlotType.Rig,
            _ => false
        };
    }

    public async Task<bool> InstallModule(HoloFittingSlot slot, HoloModuleData module)
    {
        if (!CanInstallModule(slot, module))
            return false;

        var oldModule = slot.InstalledModule;
        slot.InstalledModule = module;

        // Update visual
        UpdateSlotVisual(slot);

        // Animate installation
        if (EnableAnimations)
        {
            await AnimateModuleInstallation(slot, module);
        }

        // Create installation particles
        if (EnableParticleEffects)
        {
            CreateInstallationParticles(slot);
        }

        // Validate constraints after installation
        if (AutoValidateConstraints)
        {
            ValidateSlotConstraints(slot);
        }

        SlotModuleChanged?.Invoke(this, new SlotModuleChangedEventArgs(slot, oldModule, module));
        return true;
    }

    public async Task<bool> RemoveModule(HoloFittingSlot slot)
    {
        var oldModule = slot.InstalledModule;
        if (oldModule == null) return false;

        // Animate removal
        if (EnableAnimations)
        {
            await AnimateModuleRemoval(slot);
        }

        slot.InstalledModule = null;
        UpdateSlotVisual(slot);

        SlotModuleChanged?.Invoke(this, new SlotModuleChangedEventArgs(slot, oldModule, null));
        return true;
    }

    private void UpdateSlotVisual(HoloFittingSlot slot)
    {
        if (!_slotVisuals.TryGetValue(slot.SlotId, out var visual)) return;

        var container = (Grid)visual;
        
        // Remove old module visual
        var oldModuleVisual = container.Children.OfType<FrameworkElement>()
            .FirstOrDefault(c => c.Tag?.ToString() == "ModuleVisual");
        if (oldModuleVisual != null)
        {
            container.Children.Remove(oldModuleVisual);
        }

        // Add new module visual if installed
        if (slot.InstalledModule != null)
        {
            var newModuleVisual = CreateModuleVisual(slot.InstalledModule);
            if (newModuleVisual != null)
            {
                newModuleVisual.Tag = "ModuleVisual";
                container.Children.Add(newModuleVisual);
            }
        }
    }

    #endregion

    #region Animation System

    private void AnimationTimer_Tick(object sender, EventArgs e)
    {
        UpdateParticles();
    }

    private void UpdateParticles()
    {
        // Update and cleanup particles
        foreach (var particle in _particles.ToList())
        {
            if (particle.Opacity <= 0.1)
            {
                _particles.Remove(particle);
                _particleCanvas.Children.Remove(particle);
            }
        }
    }

    private void AnimateSlotAppearance(FrameworkElement visual)
    {
        visual.Opacity = 0;
        visual.RenderTransform = new ScaleTransform(0.5, 0.5);

        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
        var scaleX = new DoubleAnimation(0.5, 1, TimeSpan.FromMilliseconds(300)) 
        { 
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut } 
        };
        var scaleY = new DoubleAnimation(0.5, 1, TimeSpan.FromMilliseconds(300)) 
        { 
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut } 
        };

        visual.BeginAnimation(OpacityProperty, fadeIn);
        ((ScaleTransform)visual.RenderTransform).BeginAnimation(ScaleTransform.ScaleXProperty, scaleX);
        ((ScaleTransform)visual.RenderTransform).BeginAnimation(ScaleTransform.ScaleYProperty, scaleY);
    }

    private void AnimateSlotDisappearance(FrameworkElement visual, Action onComplete)
    {
        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
        var scaleOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));

        fadeOut.Completed += (s, e) => onComplete?.Invoke();

        visual.BeginAnimation(OpacityProperty, fadeOut);
        if (visual.RenderTransform is ScaleTransform scale)
        {
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleOut);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleOut);
        }
    }

    private void AnimateSlotHover(FrameworkElement visual, bool isHovering)
    {
        var targetScale = isHovering ? 1.1 : 1.0;
        var duration = TimeSpan.FromMilliseconds(150);

        if (visual.RenderTransform is not ScaleTransform)
        {
            visual.RenderTransform = new ScaleTransform(1, 1);
        }

        var scaleX = new DoubleAnimation(targetScale, duration) 
        { 
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } 
        };
        var scaleY = new DoubleAnimation(targetScale, duration) 
        { 
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } 
        };

        ((ScaleTransform)visual.RenderTransform).BeginAnimation(ScaleTransform.ScaleXProperty, scaleX);
        ((ScaleTransform)visual.RenderTransform).BeginAnimation(ScaleTransform.ScaleYProperty, scaleY);
    }

    private void AnimateSlotPress(FrameworkElement visual)
    {
        var scale = new DoubleAnimation(0.95, TimeSpan.FromMilliseconds(100));
        ((ScaleTransform)visual.RenderTransform).BeginAnimation(ScaleTransform.ScaleXProperty, scale);
        ((ScaleTransform)visual.RenderTransform).BeginAnimation(ScaleTransform.ScaleYProperty, scale);
    }

    private void AnimateSlotRelease(FrameworkElement visual)
    {
        var scale = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(100));
        ((ScaleTransform)visual.RenderTransform).BeginAnimation(ScaleTransform.ScaleXProperty, scale);
        ((ScaleTransform)visual.RenderTransform).BeginAnimation(ScaleTransform.ScaleYProperty, scale);
    }

    private void AnimateSlotDropTarget(HoloFittingSlot slot, bool isTarget)
    {
        if (!_slotVisuals.TryGetValue(slot.SlotId, out var visual)) return;

        var targetOpacity = isTarget ? 0.8 : 1.0;
        var animation = new DoubleAnimation(targetOpacity, TimeSpan.FromMilliseconds(200));
        visual.BeginAnimation(OpacityProperty, animation);

        // Add pulsing effect for drop target
        if (isTarget)
        {
            var pulse = new DoubleAnimation
            {
                From = 0.8,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(500),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            visual.BeginAnimation(OpacityProperty, pulse);
        }
        else
        {
            visual.BeginAnimation(OpacityProperty, null);
        }
    }

    private async Task AnimateModuleInstallation(HoloFittingSlot slot, HoloModuleData module)
    {
        if (!_slotVisuals.TryGetValue(slot.SlotId, out var visual)) return;

        // Flash effect
        var flash = new DoubleAnimation
        {
            From = 1.0,
            To = 2.0,
            Duration = TimeSpan.FromMilliseconds(100),
            AutoReverse = true
        };

        if (visual.Effect is DropShadowEffect effect)
        {
            effect.BeginAnimation(DropShadowEffect.BlurRadiusProperty, flash);
        }

        await Task.Delay(200);
    }

    private async Task AnimateModuleRemoval(HoloFittingSlot slot)
    {
        // Fade out animation for module removal
        await Task.Delay(100);
    }

    #endregion

    #region Particle Effects

    private void CreateSlotHoverParticles(HoloFittingSlot slot)
    {
        if (!_slotVisuals.TryGetValue(slot.SlotId, out var visual)) return;

        var particles = new List<UIElement>();

        for (int i = 0; i < 5; i++)
        {
            var particle = new Ellipse
            {
                Width = 3,
                Height = 3,
                Fill = GetBrushForSlotType(slot.SlotType),
                Opacity = 0.8
            };

            var visualPosition = visual.TransformToAncestor(_slotCanvas).Transform(new Point(0, 0));
            var angle = (2 * Math.PI * i) / 5;
            var radius = 40;

            var x = visualPosition.X + 40 + radius * Math.Cos(angle);
            var y = visualPosition.Y + 40 + radius * Math.Sin(angle);

            Canvas.SetLeft(particle, x);
            Canvas.SetTop(particle, y);
            Canvas.SetZIndex(particle, 1000);

            particles.Add(particle);
            _particleCanvas.Children.Add(particle);

            // Animate particle orbit
            var orbitAnimation = new DoubleAnimation
            {
                From = angle,
                To = angle + 2 * Math.PI,
                Duration = TimeSpan.FromMilliseconds(2000),
                RepeatBehavior = RepeatBehavior.Forever
            };

            // Store particles for cleanup
            if (!_slotParticles.ContainsKey(slot.SlotId))
                _slotParticles[slot.SlotId] = new List<UIElement>();
            _slotParticles[slot.SlotId].AddRange(particles);
        }
    }

    private void CreateInstallationParticles(HoloFittingSlot slot)
    {
        if (!_slotVisuals.TryGetValue(slot.SlotId, out var visual)) return;

        var visualPosition = visual.TransformToAncestor(_slotCanvas).Transform(new Point(40, 40));

        for (int i = 0; i < 15; i++)
        {
            var particle = new Ellipse
            {
                Width = _random.NextDouble() * 4 + 2,
                Height = _random.NextDouble() * 4 + 2,
                Fill = GetBrushForSlotType(slot.SlotType),
                Opacity = 0.9
            };

            var angle = _random.NextDouble() * 2 * Math.PI;
            var distance = _random.NextDouble() * 60;

            var x = visualPosition.X + distance * Math.Cos(angle);
            var y = visualPosition.Y + distance * Math.Sin(angle);

            Canvas.SetLeft(particle, x);
            Canvas.SetTop(particle, y);
            Canvas.SetZIndex(particle, 1000);

            _particles.Add(particle);
            _particleCanvas.Children.Add(particle);

            // Animate particle movement toward center
            var moveToCenter = new DoubleAnimation
            {
                From = distance,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            var fadeOut = new DoubleAnimation
            {
                From = 0.9,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(500)
            };

            particle.BeginAnimation(OpacityProperty, fadeOut);

            // Remove particle after animation
            Task.Delay(500).ContinueWith(t =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    _particles.Remove(particle);
                    _particleCanvas.Children.Remove(particle);
                });
            });
        }
    }

    private void RemoveSlotParticles(string slotId)
    {
        if (_slotParticles.TryGetValue(slotId, out var particles))
        {
            foreach (var particle in particles)
            {
                _particleCanvas.Children.Remove(particle);
            }
            _slotParticles.Remove(slotId);
        }
    }

    #endregion

    #region Snap Fitting

    private void CheckSnapTargets(HoloFittingSlot draggedSlot, Point position)
    {
        var snapDistance = 50.0;
        var closestSlot = FittingSlots
            .Where(s => s != draggedSlot && s.InstalledModule == null)
            .Select(s => new { Slot = s, Distance = GetDistanceToSlot(s, position) })
            .Where(x => x.Distance < snapDistance)
            .OrderBy(x => x.Distance)
            .FirstOrDefault();

        if (closestSlot != null)
        {
            // Highlight snap target
            HighlightSnapTarget(closestSlot.Slot, true);
        }
    }

    private double GetDistanceToSlot(HoloFittingSlot slot, Point position)
    {
        if (!_slotVisuals.TryGetValue(slot.SlotId, out var visual)) return double.MaxValue;

        var slotPosition = visual.TransformToAncestor(_slotCanvas).Transform(new Point(40, 40));
        var dx = position.X - slotPosition.X;
        var dy = position.Y - slotPosition.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private void HighlightSnapTarget(HoloFittingSlot slot, bool highlight)
    {
        if (!_slotVisuals.TryGetValue(slot.SlotId, out var visual)) return;

        if (highlight)
        {
            visual.Effect = CreateIntenseGlowEffect();
        }
        else
        {
            visual.Effect = CreateGlowEffect();
        }
    }

    #endregion

    #region Style and Brush Helpers

    private Brush CreateSlotBackground(HoloFittingSlot slot)
    {
        var brush = new RadialGradientBrush();
        var color = GetColorForSlotType(slot.SlotType);
        
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(30, color.R, color.G, color.B), 0));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(10, color.R, color.G, color.B), 1));
        
        return brush;
    }

    private Brush CreateModuleBackground(HoloModuleData module)
    {
        var brush = new RadialGradientBrush();
        var color = GetColorForModuleType(module.Category);
        
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(60, color.R, color.G, color.B), 0));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(20, color.R, color.G, color.B), 1));
        
        return brush;
    }

    private Brush GetBrushForSlotType(SlotType slotType)
    {
        return slotType switch
        {
            SlotType.High => new SolidColorBrush(Color.FromRgb(220, 20, 60)),    // Red
            SlotType.Mid => new SolidColorBrush(Color.FromRgb(255, 191, 0)),     // Gold
            SlotType.Low => new SolidColorBrush(Color.FromRgb(0, 191, 255)),     // Blue
            SlotType.Rig => new SolidColorBrush(Color.FromRgb(0, 255, 127)),     // Green
            _ => new SolidColorBrush(Color.FromRgb(128, 128, 128))               // Gray
        };
    }

    private Color GetColorForSlotType(SlotType slotType)
    {
        return slotType switch
        {
            SlotType.High => Color.FromRgb(220, 20, 60),
            SlotType.Mid => Color.FromRgb(255, 191, 0),
            SlotType.Low => Color.FromRgb(0, 191, 255),
            SlotType.Rig => Color.FromRgb(0, 255, 127),
            _ => Color.FromRgb(128, 128, 128)
        };
    }

    private Brush GetBrushForModuleType(ModuleCategory category)
    {
        return category switch
        {
            ModuleCategory.Weapons => new SolidColorBrush(Color.FromRgb(220, 20, 60)),
            ModuleCategory.Defense => new SolidColorBrush(Color.FromRgb(0, 191, 255)),
            ModuleCategory.Electronics => new SolidColorBrush(Color.FromRgb(255, 191, 0)),
            ModuleCategory.Engineering => new SolidColorBrush(Color.FromRgb(0, 255, 127)),
            _ => new SolidColorBrush(Color.FromRgb(128, 128, 128))
        };
    }

    private Color GetColorForModuleType(ModuleCategory category)
    {
        return category switch
        {
            ModuleCategory.Weapons => Color.FromRgb(220, 20, 60),
            ModuleCategory.Defense => Color.FromRgb(0, 191, 255),
            ModuleCategory.Electronics => Color.FromRgb(255, 191, 0),
            ModuleCategory.Engineering => Color.FromRgb(0, 255, 127),
            _ => Color.FromRgb(128, 128, 128)
        };
    }

    private Geometry GetGeometryForModuleType(ModuleCategory category)
    {
        return category switch
        {
            ModuleCategory.Weapons => Geometry.Parse("M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,17L16,12L12,7L8,12L12,17Z"),
            ModuleCategory.Defense => Geometry.Parse("M12,1L3,5V11C3,16.55 6.84,21.74 12,23C17.16,21.74 21,16.55 21,11V5L12,1M12,7C13.4,7 14.8,8.6 14.8,10V11.5C15.4,11.5 16,12.1 16,12.7V16.2C16,16.8 15.4,17.3 14.7,17.3H9.2C8.6,17.3 8,16.8 8,16.2V12.6C8,12.1 8.4,11.5 9,11.5V10C9,8.6 10.6,7 12,7M12,8.2C11.2,8.2 10.2,8.7 10.2,10V11.5H13.8V10C13.8,8.7 12.8,8.2 12,8.2Z"),
            ModuleCategory.Electronics => Geometry.Parse("M4,6H20V10H4V6M4,12H20V16H4V12M4,18H20V22H4V18Z"),
            ModuleCategory.Engineering => Geometry.Parse("M22.7,19L13.6,9.9C14.5,7.6 14,4.9 12.1,3C10.1,1 7.1,1 5.1,3C3.1,5 3.1,8 5.1,10C7,11.9 9.7,12.4 12,11.5L21.1,20.6L22.7,19M6.2,4.1C7.2,3.1 8.8,3.1 9.8,4.1C10.8,5.1 10.8,6.7 9.8,7.7C8.8,8.7 7.2,8.7 6.2,7.7C5.2,6.7 5.2,5.1 6.2,4.1Z"),
            _ => Geometry.Parse("M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2Z")
        };
    }

    private Effect CreateGlowEffect(double intensity = 1.0)
    {
        return new DropShadowEffect
        {
            Color = GetColorForScheme(EVEColorScheme),
            BlurRadius = 10 * intensity,
            Opacity = 0.6 * intensity,
            ShadowDepth = 0
        };
    }

    private Effect CreateIntenseGlowEffect()
    {
        return new DropShadowEffect
        {
            Color = GetColorForScheme(EVEColorScheme),
            BlurRadius = 20,
            Opacity = 1.0,
            ShadowDepth = 0
        };
    }

    private Color GetColorForScheme(EVEColorScheme scheme)
    {
        return scheme switch
        {
            EVEColorScheme.ElectricBlue => Color.FromRgb(0, 191, 255),
            EVEColorScheme.AmberGold => Color.FromRgb(255, 191, 0),
            EVEColorScheme.CrimsonRed => Color.FromRgb(220, 20, 60),
            EVEColorScheme.EmeraldGreen => Color.FromRgb(0, 255, 127),
            _ => Color.FromRgb(0, 191, 255)
        };
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableAnimations)
        {
            _animationTimer.Start();
        }

        if (AutoValidateConstraints)
        {
            _constraintTimer.Start();
        }

        UpdateSlotLayout();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        _constraintTimer?.Stop();
    }

    #endregion

    #region Dependency Property Callbacks

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloFittingSlotManager manager)
        {
            // Update holographic effects
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloFittingSlotManager manager)
        {
            // Update color scheme
        }
    }

    private static void OnShipDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloFittingSlotManager manager)
        {
            // Update validators with new ship data
            foreach (var validator in manager._slotValidators.Values)
            {
                validator.UpdateShipData(e.NewValue as HoloShipData);
            }
        }
    }

    private static void OnFittingSlotsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloFittingSlotManager manager)
        {
            if (e.OldValue is ObservableCollection<HoloFittingSlot> oldSlots)
            {
                oldSlots.CollectionChanged -= manager.FittingSlots_CollectionChanged;
            }

            if (e.NewValue is ObservableCollection<HoloFittingSlot> newSlots)
            {
                newSlots.CollectionChanged += manager.FittingSlots_CollectionChanged;
                manager.UpdateSlotLayout();
            }
        }
    }

    private static void OnSelectedSlotChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Handle slot selection change
    }

    private static void OnSlotLayoutModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloFittingSlotManager manager)
        {
            manager.UpdateSlotLayout();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloFittingSlotManager manager)
        {
            if ((bool)e.NewValue)
                manager._animationTimer.Start();
            else
                manager._animationTimer.Stop();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloFittingSlotManager manager && !(bool)e.NewValue)
        {
            // Clear all particles
            foreach (var particles in manager._slotParticles.Values)
            {
                foreach (var particle in particles)
                {
                    manager._particleCanvas.Children.Remove(particle);
                }
            }
            manager._slotParticles.Clear();
        }
    }

    private static void OnShowSlotConstraintsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Update constraint indicator visibility
    }

    private static void OnEnableSnapFittingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Snap fitting setting changed
    }

    private static void OnShowSlotNumbersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Update slot number visibility
    }

    private static void OnAutoValidateConstraintsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloFittingSlotManager manager)
        {
            if ((bool)e.NewValue)
                manager._constraintTimer.Start();
            else
                manager._constraintTimer.Stop();
        }
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void SetSimplifiedMode(bool enabled)
    {
        EnableAnimations = !enabled;
        EnableParticleEffects = !enabled;
        
        if (enabled)
        {
            _animationTimer?.Stop();
            foreach (var particles in _slotParticles.Values)
            {
                foreach (var particle in particles)
                {
                    _particleCanvas.Children.Remove(particle);
                }
            }
            _slotParticles.Clear();
        }
        else
        {
            _animationTimer?.Start();
        }
    }

    public bool IsSimplifiedModeActive => !EnableAnimations || !EnableParticleEffects;

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets all slots of a specific type
    /// </summary>
    public IEnumerable<HoloFittingSlot> GetSlotsByType(SlotType slotType)
    {
        return FittingSlots?.Where(s => s.SlotType == slotType) ?? Enumerable.Empty<HoloFittingSlot>();
    }

    /// <summary>
    /// Gets all installed modules
    /// </summary>
    public IEnumerable<HoloModuleData> GetInstalledModules()
    {
        return FittingSlots?.Where(s => s.InstalledModule != null)
                           .Select(s => s.InstalledModule) ?? Enumerable.Empty<HoloModuleData>();
    }

    /// <summary>
    /// Clears all installed modules
    /// </summary>
    public async Task ClearAllModules()
    {
        var slotsWithModules = FittingSlots?.Where(s => s.InstalledModule != null).ToList();
        if (slotsWithModules?.Any() == true)
        {
            foreach (var slot in slotsWithModules)
            {
                await RemoveModule(slot);
            }
        }
    }

    /// <summary>
    /// Validates all slot constraints
    /// </summary>
    public ValidationResult ValidateAllConstraints()
    {
        var results = new List<ValidationResult>();
        
        foreach (var slot in FittingSlots ?? Enumerable.Empty<HoloFittingSlot>())
        {
            if (_slotValidators.TryGetValue(slot.SlotId, out var validator))
            {
                results.Add(validator.Validate());
            }
        }

        return new ValidationResult
        {
            IsValid = results.All(r => r.IsValid),
            Violations = results.SelectMany(r => r.Violations).ToList()
        };
    }

    #endregion
}

#region Supporting Types

/// <summary>
/// Slot layout modes for different visualizations
/// </summary>
public enum SlotLayoutMode
{
    ShipSilhouette,
    Grid,
    Circle,
    Radial
}

/// <summary>
/// Fitting slot types
/// </summary>
public enum SlotType
{
    High,
    Mid,
    Low,
    Rig,
    Subsystem
}

/// <summary>
/// Slot interaction types
/// </summary>
public enum SlotInteractionType
{
    Hover,
    Select,
    Drag,
    Drop
}

/// <summary>
/// Holographic fitting slot data
/// </summary>
public class HoloFittingSlot : INotifyPropertyChanged
{
    private HoloModuleData _installedModule;

    public string SlotId { get; set; } = Guid.NewGuid().ToString();
    public SlotType SlotType { get; set; }
    public int SlotIndex { get; set; }
    public Point Position { get; set; }
    public bool IsLocked { get; set; }
    
    public HoloModuleData InstalledModule
    {
        get => _installedModule;
        set
        {
            _installedModule = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InstalledModule)));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
}

/// <summary>
/// Event args for slot module changes
/// </summary>
public class SlotModuleChangedEventArgs : EventArgs
{
    public HoloFittingSlot Slot { get; }
    public HoloModuleData OldModule { get; }
    public HoloModuleData NewModule { get; }

    public SlotModuleChangedEventArgs(HoloFittingSlot slot, HoloModuleData oldModule, HoloModuleData newModule)
    {
        Slot = slot;
        OldModule = oldModule;
        NewModule = newModule;
    }
}

/// <summary>
/// Event args for slot validation failures
/// </summary>
public class SlotValidationEventArgs : EventArgs
{
    public HoloFittingSlot Slot { get; }
    public ValidationResult ValidationResult { get; }

    public SlotValidationEventArgs(HoloFittingSlot slot, ValidationResult result)
    {
        Slot = slot;
        ValidationResult = result;
    }
}

/// <summary>
/// Event args for slot interactions
/// </summary>
public class SlotInteractionEventArgs : EventArgs
{
    public HoloFittingSlot Slot { get; }
    public SlotInteractionType InteractionType { get; }
    public bool IsActive { get; }

    public SlotInteractionEventArgs(HoloFittingSlot slot, SlotInteractionType type, bool isActive)
    {
        Slot = slot;
        InteractionType = type;
        IsActive = isActive;
    }
}

/// <summary>
/// Constraint validator for fitting slots
/// </summary>
public class ConstraintValidator
{
    private readonly HoloFittingSlot _slot;
    private HoloShipData _shipData;

    public ConstraintValidator(HoloFittingSlot slot, HoloShipData shipData)
    {
        _slot = slot;
        _shipData = shipData;
    }

    public void UpdateShipData(HoloShipData shipData)
    {
        _shipData = shipData;
    }

    public ValidationResult Validate()
    {
        var result = new ValidationResult { IsValid = true, Violations = new List<string>() };

        if (_slot.InstalledModule == null)
            return result;

        // Validate power requirements
        if (!ValidatePowerRequirements())
        {
            result.IsValid = false;
            result.Violations.Add("Insufficient power grid");
        }

        // Validate CPU requirements
        if (!ValidateCpuRequirements())
        {
            result.IsValid = false;
            result.Violations.Add("Insufficient CPU");
        }

        // Validate calibration (for rigs)
        if (_slot.SlotType == SlotType.Rig && !ValidateCalibration())
        {
            result.IsValid = false;
            result.Violations.Add("Insufficient calibration");
        }

        return result;
    }

    public bool CanInstallModule(HoloModuleData module)
    {
        // Simplified validation - real implementation would be more comprehensive
        return true;
    }

    private bool ValidatePowerRequirements()
    {
        // Simplified power validation
        return true;
    }

    private bool ValidateCpuRequirements()
    {
        // Simplified CPU validation
        return true;
    }

    private bool ValidateCalibration()
    {
        // Simplified calibration validation
        return true;
    }
}

/// <summary>
/// Validation result for constraint checking
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Violations { get; set; } = new();
}

#endregion