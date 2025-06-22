// ==========================================================================
// HoloDragDropModule.cs - Holographic Drag-and-Drop Module Interface
// ==========================================================================
// Advanced drag-and-drop module interface featuring holographic module cards,
// animated fitting operations, compatibility checking, and EVE-style interactions.
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
/// Holographic drag-and-drop module interface with animated fitting operations and compatibility checking
/// </summary>
public class HoloDragDropModule : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloDragDropModule),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloDragDropModule),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty ModuleDataProperty =
        DependencyProperty.Register(nameof(ModuleData), typeof(HoloModuleData), typeof(HoloDragDropModule),
            new PropertyMetadata(null, OnModuleDataChanged));

    public static readonly DependencyProperty DragModeProperty =
        DependencyProperty.Register(nameof(DragMode), typeof(DragMode), typeof(HoloDragDropModule),
            new PropertyMetadata(DragMode.Holographic, OnDragModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloDragDropModule),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloDragDropModule),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowCompatibilityIndicatorsProperty =
        DependencyProperty.Register(nameof(ShowCompatibilityIndicators), typeof(bool), typeof(HoloDragDropModule),
            new PropertyMetadata(true, OnShowCompatibilityIndicatorsChanged));

    public static readonly DependencyProperty EnableSnapToSlotProperty =
        DependencyProperty.Register(nameof(EnableSnapToSlot), typeof(bool), typeof(HoloDragDropModule),
            new PropertyMetadata(true, OnEnableSnapToSlotChanged));

    public static readonly DependencyProperty DragThresholdProperty =
        DependencyProperty.Register(nameof(DragThreshold), typeof(double), typeof(HoloDragDropModule),
            new PropertyMetadata(5.0, OnDragThresholdChanged));

    public static readonly DependencyProperty AllowMultiSelectProperty =
        DependencyProperty.Register(nameof(AllowMultiSelect), typeof(bool), typeof(HoloDragDropModule),
            new PropertyMetadata(false, OnAllowMultiSelectChanged));

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

    public HoloModuleData ModuleData
    {
        get => (HoloModuleData)GetValue(ModuleDataProperty);
        set => SetValue(ModuleDataProperty, value);
    }

    public DragMode DragMode
    {
        get => (DragMode)GetValue(DragModeProperty);
        set => SetValue(DragModeProperty, value);
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

    public bool ShowCompatibilityIndicators
    {
        get => (bool)GetValue(ShowCompatibilityIndicatorsProperty);
        set => SetValue(ShowCompatibilityIndicatorsProperty, value);
    }

    public bool EnableSnapToSlot
    {
        get => (bool)GetValue(EnableSnapToSlotProperty);
        set => SetValue(EnableSnapToSlotProperty, value);
    }

    public double DragThreshold
    {
        get => (double)GetValue(DragThresholdProperty);
        set => SetValue(DragThresholdProperty, value);
    }

    public bool AllowMultiSelect
    {
        get => (bool)GetValue(AllowMultiSelectProperty);
        set => SetValue(AllowMultiSelectProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<ModuleDragEventArgs> DragStarted;
    public event EventHandler<ModuleDragEventArgs> DragProgress;
    public event EventHandler<ModuleDragEventArgs> DragCompleted;
    public event EventHandler<ModuleDragEventArgs> DragCancelled;
    public event EventHandler<ModuleDropEventArgs> ModuleDropped;
    public event EventHandler<ModuleCompatibilityEventArgs> CompatibilityChecked;
    public event EventHandler<ModuleSelectionEventArgs> ModuleSelected;

    #endregion

    #region Private Fields

    private readonly DispatcherTimer _animationTimer;
    private readonly DispatcherTimer _dragTimer;
    private Border _moduleCard;
    private Canvas _mainCanvas;
    private Canvas _dragCanvas;
    private Canvas _particleCanvas;
    private Canvas _compatibilityCanvas;
    private Grid _moduleContent;
    private bool _isDragging;
    private bool _isSelected;
    private bool _isSimplifiedMode;
    private Point _dragStartPoint;
    private Point _lastDragPoint;
    private Vector _dragOffset;
    private double _animationPhase;
    private CompatibilityState _currentCompatibility = CompatibilityState.Unknown;
    private readonly List<UIElement> _particleEffects = new();
    private readonly List<UIElement> _compatibilityIndicators = new();
    private readonly Random _random = new();

    #endregion

    #region Constructor

    public HoloDragDropModule()
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
        Width = 200;
        Height = 120;
        Background = new SolidColorBrush(Colors.Transparent);
        
        CreateMainLayout();
        SetupDragAndDrop();
        ApplyEVEColorScheme();
    }

    private void CreateMainLayout()
    {
        _mainCanvas = new Canvas
        {
            Width = Width,
            Height = Height,
            ClipToBounds = false
        };
        Content = _mainCanvas;

        // Module card container
        CreateModuleCard();

        // Drag canvas (for drag preview)
        _dragCanvas = new Canvas
        {
            IsHitTestVisible = false,
            Width = Width,
            Height = Height
        };
        _mainCanvas.Children.Add(_dragCanvas);

        // Compatibility indicators canvas
        _compatibilityCanvas = new Canvas
        {
            IsHitTestVisible = false,
            Width = Width,
            Height = Height
        };
        _mainCanvas.Children.Add(_compatibilityCanvas);

        // Particle effects canvas
        _particleCanvas = new Canvas
        {
            IsHitTestVisible = false,
            Width = Width,
            Height = Height
        };
        _mainCanvas.Children.Add(_particleCanvas);
    }

    private void CreateModuleCard()
    {
        _moduleCard = new Border
        {
            Width = Width - 10,
            Height = Height - 10,
            Background = CreateModuleCardBrush(),
            BorderBrush = CreateModuleCardBorderBrush(),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(8),
            Effect = CreateCardGlowEffect(),
            Cursor = Cursors.Hand
        };

        Canvas.SetLeft(_moduleCard, 5);
        Canvas.SetTop(_moduleCard, 5);
        _mainCanvas.Children.Add(_moduleCard);

        CreateModuleContent();
    }

    private void CreateModuleContent()
    {
        _moduleContent = new Grid();
        _moduleCard.Child = _moduleContent;

        // Define layout
        _moduleContent.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _moduleContent.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        _moduleContent.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        UpdateModuleContent();
    }

    private void UpdateModuleContent()
    {
        _moduleContent.Children.Clear();

        if (ModuleData == null) return;

        // Module icon/image
        var moduleIcon = CreateModuleIcon();
        Grid.SetRow(moduleIcon, 0);
        _moduleContent.Children.Add(moduleIcon);

        // Module name
        var moduleName = new TextBlock
        {
            Text = ModuleData.Name,
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = GetModuleNameBrush(),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(5),
            Effect = CreateTextGlow()
        };
        Grid.SetRow(moduleName, 1);
        _moduleContent.Children.Add(moduleName);

        // Module stats/meta
        var moduleStats = CreateModuleStats();
        Grid.SetRow(moduleStats, 2);
        _moduleContent.Children.Add(moduleStats);

        // Compatibility indicator
        if (ShowCompatibilityIndicators)
        {
            UpdateCompatibilityIndicator();
        }
    }

    private FrameworkElement CreateModuleIcon()
    {
        var iconContainer = new Border
        {
            Width = 40,
            Height = 40,
            Background = GetModuleTypeBrush(),
            CornerRadius = new CornerRadius(5),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(5),
            Effect = new DropShadowEffect
            {
                Color = GetModuleTypeColor(),
                BlurRadius = 8,
                ShadowDepth = 0,
                Opacity = 0.6
            }
        };

        var iconText = new TextBlock
        {
            Text = GetModuleTypeSymbol(),
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        iconContainer.Child = iconText;
        return iconContainer;
    }

    private FrameworkElement CreateModuleStats()
    {
        var statsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(2)
        };

        // CPU requirement
        if (ModuleData.CPUCost > 0)
        {
            statsPanel.Children.Add(CreateStatLabel("CPU", ModuleData.CPUCost.ToString("F0")));
        }

        // Power requirement
        if (ModuleData.PowerCost > 0)
        {
            statsPanel.Children.Add(CreateStatLabel("PWR", ModuleData.PowerCost.ToString("F0")));
        }

        // Meta level indicator
        if (ModuleData.MetaLevel > 0)
        {
            statsPanel.Children.Add(CreateMetaLevelIndicator());
        }

        return statsPanel;
    }

    private FrameworkElement CreateStatLabel(string label, string value)
    {
        var container = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(3, 0, 3, 0)
        };

        var labelText = new TextBlock
        {
            Text = label,
            FontSize = 8,
            Foreground = GetSecondaryBrush(),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var valueText = new TextBlock
        {
            Text = value,
            FontSize = 9,
            FontWeight = FontWeights.Bold,
            Foreground = GetStatValueBrush(),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        container.Children.Add(labelText);
        container.Children.Add(valueText);
        return container;
    }

    private FrameworkElement CreateMetaLevelIndicator()
    {
        var metaContainer = new Border
        {
            Background = GetMetaLevelBrush(),
            CornerRadius = new CornerRadius(3),
            Padding = new Thickness(3, 1, 3, 1),
            Margin = new Thickness(2, 0, 2, 0)
        };

        var metaText = new TextBlock
        {
            Text = $"T{ModuleData.TechLevel}",
            FontSize = 8,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White)
        };

        metaContainer.Child = metaText;
        return metaContainer;
    }

    private void InitializeTimers()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
        };
        _animationTimer.Tick += OnAnimationTimerTick;

        _dragTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS for smooth drag
        };
        _dragTimer.Tick += OnDragTimerTick;
    }

    private void SetupDragAndDrop()
    {
        _moduleCard.MouseLeftButtonDown += OnMouseLeftButtonDown;
        _moduleCard.MouseLeftButtonUp += OnMouseLeftButtonUp;
        _moduleCard.MouseMove += OnMouseMove;
        _moduleCard.MouseEnter += OnMouseEnter;
        _moduleCard.MouseLeave += OnMouseLeave;

        // Enable drop handling
        AllowDrop = true;
        Drop += OnDrop;
        DragOver += OnDragOver;
        DragEnter += OnDragEnter;
        DragLeave += OnDragLeave;
    }

    #endregion

    #region Public Methods

    public void StartDrag()
    {
        if (_isDragging || ModuleData == null) return;

        _isDragging = true;
        _dragStartPoint = Mouse.GetPosition(_mainCanvas);
        
        CreateDragPreview();
        
        if (EnableAnimations && !_isSimplifiedMode)
        {
            AnimateDragStart();
        }

        if (EnableParticleEffects && !_isSimplifiedMode)
        {
            SpawnDragStartParticles();
        }

        _dragTimer.Start();
        Mouse.Capture(_moduleCard);

        DragStarted?.Invoke(this, new ModuleDragEventArgs
        {
            Module = ModuleData,
            StartPosition = _dragStartPoint,
            Timestamp = DateTime.Now
        });
    }

    public void CompleteDrag(Point dropPosition)
    {
        if (!_isDragging) return;

        _isDragging = false;
        _dragTimer.Stop();
        Mouse.Capture(null);

        if (EnableAnimations && !_isSimplifiedMode)
        {
            AnimateDragComplete(dropPosition);
        }
        else
        {
            ResetDragState();
        }

        if (EnableParticleEffects && !_isSimplifiedMode)
        {
            SpawnDragCompleteParticles(dropPosition);
        }

        DragCompleted?.Invoke(this, new ModuleDragEventArgs
        {
            Module = ModuleData,
            StartPosition = _dragStartPoint,
            EndPosition = dropPosition,
            Timestamp = DateTime.Now
        });
    }

    public void CancelDrag()
    {
        if (!_isDragging) return;

        _isDragging = false;
        _dragTimer.Stop();
        Mouse.Capture(null);

        if (EnableAnimations && !_isSimplifiedMode)
        {
            AnimateDragCancel();
        }
        else
        {
            ResetDragState();
        }

        DragCancelled?.Invoke(this, new ModuleDragEventArgs
        {
            Module = ModuleData,
            StartPosition = _dragStartPoint,
            Timestamp = DateTime.Now
        });
    }

    public void SetCompatibility(CompatibilityState compatibility)
    {
        _currentCompatibility = compatibility;
        UpdateCompatibilityIndicator();
        UpdateCardAppearance();
    }

    public void HighlightAsDropTarget(bool highlight)
    {
        if (highlight)
        {
            CreateDropTargetEffect();
        }
        else
        {
            ClearDropTargetEffect();
        }
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        UpdateCardAppearance();
        
        if (selected && EnableParticleEffects && !_isSimplifiedMode)
        {
            SpawnSelectionParticles();
        }

        ModuleSelected?.Invoke(this, new ModuleSelectionEventArgs
        {
            Module = ModuleData,
            IsSelected = selected,
            Timestamp = DateTime.Now
        });
    }

    public void PulseHighlight(int pulseCount = 3)
    {
        if (!EnableAnimations || _isSimplifiedMode) return;

        var pulseAnimation = new DoubleAnimation
        {
            From = HolographicIntensity,
            To = Math.Min(1.0, HolographicIntensity + 0.3),
            Duration = TimeSpan.FromMilliseconds(200),
            RepeatBehavior = new RepeatBehavior(pulseCount),
            AutoReverse = true
        };

        _moduleCard.BeginAnimation(OpacityProperty, pulseAnimation);
    }

    #endregion

    #region Private Methods

    private void CreateDragPreview()
    {
        var preview = new Border
        {
            Width = _moduleCard.ActualWidth,
            Height = _moduleCard.ActualHeight,
            Background = CreateDragPreviewBrush(),
            BorderBrush = GetPrimaryBrush(),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(8),
            Opacity = 0.8,
            Effect = new DropShadowEffect
            {
                Color = GetPrimaryColor(),
                BlurRadius = 15,
                ShadowDepth = 5,
                Opacity = 0.8
            }
        };

        // Clone content for preview
        var previewContent = CloneModuleContent();
        preview.Child = previewContent;

        Canvas.SetLeft(preview, _dragStartPoint.X - Width / 2);
        Canvas.SetTop(preview, _dragStartPoint.Y - Height / 2);
        _dragCanvas.Children.Add(preview);
    }

    private Grid CloneModuleContent()
    {
        var clone = new Grid();
        
        // Copy row definitions
        foreach (var rowDef in _moduleContent.RowDefinitions)
        {
            clone.RowDefinitions.Add(new RowDefinition { Height = rowDef.Height });
        }

        // Simplified content for drag preview
        var previewText = new TextBlock
        {
            Text = ModuleData?.Name ?? "Module",
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = GetModuleNameBrush(),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center
        };

        Grid.SetRowSpan(previewText, 3);
        clone.Children.Add(previewText);

        return clone;
    }

    private void UpdateCompatibilityIndicator()
    {
        ClearCompatibilityIndicators();
        
        if (!ShowCompatibilityIndicators) return;

        var indicator = CreateCompatibilityVisual();
        if (indicator != null)
        {
            _compatibilityCanvas.Children.Add(indicator);
            _compatibilityIndicators.Add(indicator);
        }
    }

    private FrameworkElement CreateCompatibilityVisual()
    {
        switch (_currentCompatibility)
        {
            case CompatibilityState.Compatible:
                return CreateCompatibleIndicator();
            case CompatibilityState.Incompatible:
                return CreateIncompatibleIndicator();
            case CompatibilityState.Warning:
                return CreateWarningIndicator();
            default:
                return null;
        }
    }

    private FrameworkElement CreateCompatibleIndicator()
    {
        var checkmark = new Ellipse
        {
            Width = 20,
            Height = 20,
            Fill = new SolidColorBrush(Color.FromRgb(100, 255, 100)),
            Stroke = new SolidColorBrush(Colors.White),
            StrokeThickness = 2,
            Opacity = HolographicIntensity
        };

        Canvas.SetRight(checkmark, 5);
        Canvas.SetTop(checkmark, 5);

        if (EnableAnimations && !_isSimplifiedMode)
        {
            var pulseAnimation = new DoubleAnimation
            {
                From = 0.7,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(1),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };
            checkmark.BeginAnimation(OpacityProperty, pulseAnimation);
        }

        return checkmark;
    }

    private FrameworkElement CreateIncompatibleIndicator()
    {
        var cross = new Grid
        {
            Width = 20,
            Height = 20
        };

        var line1 = new Rectangle
        {
            Width = 14,
            Height = 2,
            Fill = new SolidColorBrush(Color.FromRgb(255, 100, 100)),
            RenderTransform = new RotateTransform(45),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var line2 = new Rectangle
        {
            Width = 14,
            Height = 2,
            Fill = new SolidColorBrush(Color.FromRgb(255, 100, 100)),
            RenderTransform = new RotateTransform(-45),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        cross.Children.Add(line1);
        cross.Children.Add(line2);

        Canvas.SetRight(cross, 5);
        Canvas.SetTop(cross, 5);

        return cross;
    }

    private FrameworkElement CreateWarningIndicator()
    {
        var warning = new Polygon
        {
            Points = new PointCollection
            {
                new Point(10, 2),
                new Point(18, 16),
                new Point(2, 16)
            },
            Fill = new SolidColorBrush(Color.FromRgb(255, 255, 100)),
            Stroke = new SolidColorBrush(Colors.White),
            StrokeThickness = 1,
            Opacity = HolographicIntensity
        };

        Canvas.SetRight(warning, 5);
        Canvas.SetTop(warning, 5);

        return warning;
    }

    private void UpdateCardAppearance()
    {
        if (_moduleCard == null) return;

        // Update border based on state
        _moduleCard.BorderBrush = GetCurrentBorderBrush();
        
        // Update background based on state
        _moduleCard.Background = GetCurrentBackgroundBrush();

        // Update glow effect
        _moduleCard.Effect = GetCurrentGlowEffect();
    }

    private Brush GetCurrentBorderBrush()
    {
        if (_isSelected)
            return new SolidColorBrush(Color.FromRgb(255, 255, 100));
        
        return _currentCompatibility switch
        {
            CompatibilityState.Compatible => new SolidColorBrush(Color.FromRgb(100, 255, 100)),
            CompatibilityState.Incompatible => new SolidColorBrush(Color.FromRgb(255, 100, 100)),
            CompatibilityState.Warning => new SolidColorBrush(Color.FromRgb(255, 255, 100)),
            _ => CreateModuleCardBorderBrush()
        };
    }

    private Brush GetCurrentBackgroundBrush()
    {
        var baseBrush = CreateModuleCardBrush();
        
        if (_isSelected)
        {
            // Add selection tint
            return new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(60, 255, 255, 100), 0),
                    new GradientStop(Color.FromArgb(20, 255, 255, 100), 1)
                }
            };
        }

        return baseBrush;
    }

    private Effect GetCurrentGlowEffect()
    {
        var color = _currentCompatibility switch
        {
            CompatibilityState.Compatible => Color.FromRgb(100, 255, 100),
            CompatibilityState.Incompatible => Color.FromRgb(255, 100, 100),
            CompatibilityState.Warning => Color.FromRgb(255, 255, 100),
            _ => GetPrimaryColor()
        };

        if (_isSelected)
            color = Color.FromRgb(255, 255, 100);

        return new DropShadowEffect
        {
            Color = color,
            BlurRadius = _isSelected ? 20 : 10,
            ShadowDepth = 0,
            Opacity = HolographicIntensity * (_isSelected ? 1.0 : 0.6)
        };
    }

    private void CreateDropTargetEffect()
    {
        var dropTargetOverlay = new Border
        {
            Width = Width,
            Height = Height,
            Background = new SolidColorBrush(Color.FromArgb(80, 100, 255, 100)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(100, 255, 100)),
            BorderThickness = new Thickness(3),
            CornerRadius = new CornerRadius(8),
            IsHitTestVisible = false
        };

        _mainCanvas.Children.Add(dropTargetOverlay);

        if (EnableAnimations && !_isSimplifiedMode)
        {
            var pulseAnimation = new DoubleAnimation
            {
                From = 0.3,
                To = 0.8,
                Duration = TimeSpan.FromMilliseconds(500),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };
            dropTargetOverlay.BeginAnimation(OpacityProperty, pulseAnimation);
        }
    }

    private void ClearDropTargetEffect()
    {
        var overlaysToRemove = _mainCanvas.Children.OfType<Border>()
            .Where(b => b != _moduleCard && b.Background is SolidColorBrush brush && 
                   brush.Color.A == 80 && brush.Color.G == 255)
            .ToList();

        foreach (var overlay in overlaysToRemove)
        {
            _mainCanvas.Children.Remove(overlay);
        }
    }

    private void ResetDragState()
    {
        _dragCanvas.Children.Clear();
        _moduleCard.Opacity = HolographicIntensity;
        
        // Reset transform
        _moduleCard.RenderTransform = null;
    }

    private void ClearCompatibilityIndicators()
    {
        foreach (var indicator in _compatibilityIndicators)
        {
            _compatibilityCanvas.Children.Remove(indicator);
        }
        _compatibilityIndicators.Clear();
    }

    #endregion

    #region Animation Methods

    private void AnimateDragStart()
    {
        // Fade out original card
        var fadeAnimation = new DoubleAnimation
        {
            From = HolographicIntensity,
            To = HolographicIntensity * 0.3,
            Duration = TimeSpan.FromMilliseconds(200)
        };
        _moduleCard.BeginAnimation(OpacityProperty, fadeAnimation);

        // Scale up drag preview
        if (_dragCanvas.Children.Count > 0)
        {
            var preview = _dragCanvas.Children[0];
            var scaleTransform = new ScaleTransform(0.8, 0.8);
            preview.RenderTransform = scaleTransform;
            preview.RenderTransformOrigin = new Point(0.5, 0.5);

            var scaleAnimation = new DoubleAnimation
            {
                From = 0.8,
                To = 1.1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }
    }

    private void AnimateDragComplete(Point dropPosition)
    {
        if (_dragCanvas.Children.Count > 0)
        {
            var preview = _dragCanvas.Children[0];
            
            // Animate to drop position
            var moveXAnimation = new DoubleAnimation
            {
                From = Canvas.GetLeft(preview),
                To = dropPosition.X - Width / 2,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            var moveYAnimation = new DoubleAnimation
            {
                From = Canvas.GetTop(preview),
                To = dropPosition.Y - Height / 2,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            var fadeAnimation = new DoubleAnimation
            {
                From = preview.Opacity,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            moveXAnimation.Completed += (s, e) => ResetDragState();

            preview.BeginAnimation(Canvas.LeftProperty, moveXAnimation);
            preview.BeginAnimation(Canvas.TopProperty, moveYAnimation);
            preview.BeginAnimation(OpacityProperty, fadeAnimation);
        }

        // Restore original card
        var restoreAnimation = new DoubleAnimation
        {
            From = _moduleCard.Opacity,
            To = HolographicIntensity,
            Duration = TimeSpan.FromMilliseconds(300)
        };
        _moduleCard.BeginAnimation(OpacityProperty, restoreAnimation);
    }

    private void AnimateDragCancel()
    {
        if (_dragCanvas.Children.Count > 0)
        {
            var preview = _dragCanvas.Children[0];
            
            // Animate back to original position
            var moveXAnimation = new DoubleAnimation
            {
                From = Canvas.GetLeft(preview),
                To = _dragStartPoint.X - Width / 2,
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
            };

            var moveYAnimation = new DoubleAnimation
            {
                From = Canvas.GetTop(preview),
                To = _dragStartPoint.Y - Height / 2,
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
            };

            var scaleAnimation = new DoubleAnimation
            {
                From = 1.1,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
            };

            moveXAnimation.Completed += (s, e) => ResetDragState();

            if (preview.RenderTransform is ScaleTransform scaleTransform)
            {
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
            }

            preview.BeginAnimation(Canvas.LeftProperty, moveXAnimation);
            preview.BeginAnimation(Canvas.TopProperty, moveYAnimation);
        }

        // Restore original card
        var restoreAnimation = new DoubleAnimation
        {
            From = _moduleCard.Opacity,
            To = HolographicIntensity,
            Duration = TimeSpan.FromMilliseconds(400)
        };
        _moduleCard.BeginAnimation(OpacityProperty, restoreAnimation);
    }

    #endregion

    #region Particle Effects

    private void SpawnDragStartParticles()
    {
        var centerPoint = new Point(Width / 2, Height / 2);
        
        for (int i = 0; i < 8; i++)
        {
            var particle = new Ellipse
            {
                Width = 4,
                Height = 4,
                Fill = GetPrimaryBrush(),
                Opacity = HolographicIntensity
            };

            Canvas.SetLeft(particle, centerPoint.X);
            Canvas.SetTop(particle, centerPoint.Y);
            _particleCanvas.Children.Add(particle);
            _particleEffects.Add(particle);

            AnimateDragParticle(particle, centerPoint, i);
        }
    }

    private void AnimateDragParticle(UIElement particle, Point center, int index)
    {
        var angle = (Math.PI * 2 / 8) * index;
        var distance = 30;
        var targetX = center.X + Math.Cos(angle) * distance;
        var targetY = center.Y + Math.Sin(angle) * distance;

        var moveXAnimation = new DoubleAnimation
        {
            From = center.X,
            To = targetX,
            Duration = TimeSpan.FromMilliseconds(600),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var moveYAnimation = new DoubleAnimation
        {
            From = center.Y,
            To = targetY,
            Duration = TimeSpan.FromMilliseconds(600),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = HolographicIntensity,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(600)
        };

        moveXAnimation.Completed += (s, e) =>
        {
            _particleCanvas.Children.Remove(particle);
            _particleEffects.Remove(particle);
        };

        particle.BeginAnimation(Canvas.LeftProperty, moveXAnimation);
        particle.BeginAnimation(Canvas.TopProperty, moveYAnimation);
        particle.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void SpawnDragCompleteParticles(Point dropPosition)
    {
        for (int i = 0; i < 12; i++)
        {
            var particle = new Rectangle
            {
                Width = _random.Next(3, 7),
                Height = _random.Next(3, 7),
                Fill = new SolidColorBrush(Color.FromRgb(100, 255, 100)),
                Opacity = HolographicIntensity
            };

            Canvas.SetLeft(particle, dropPosition.X);
            Canvas.SetTop(particle, dropPosition.Y);
            _particleCanvas.Children.Add(particle);

            AnimateCompleteParticle(particle, dropPosition);
        }
    }

    private void AnimateCompleteParticle(UIElement particle, Point origin)
    {
        var angle = _random.NextDouble() * Math.PI * 2;
        var velocity = _random.Next(20, 50);
        var targetX = origin.X + Math.Cos(angle) * velocity;
        var targetY = origin.Y + Math.Sin(angle) * velocity;

        var moveXAnimation = new DoubleAnimation
        {
            From = origin.X,
            To = targetX,
            Duration = TimeSpan.FromMilliseconds(800),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var moveYAnimation = new DoubleAnimation
        {
            From = origin.Y,
            To = targetY,
            Duration = TimeSpan.FromMilliseconds(800),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = HolographicIntensity,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(800)
        };

        moveXAnimation.Completed += (s, e) => _particleCanvas.Children.Remove(particle);

        particle.BeginAnimation(Canvas.LeftProperty, moveXAnimation);
        particle.BeginAnimation(Canvas.TopProperty, moveYAnimation);
        particle.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void SpawnSelectionParticles()
    {
        var outline = GetCardOutlinePoints();
        
        for (int i = 0; i < outline.Count; i += 4)
        {
            var particle = new Ellipse
            {
                Width = 3,
                Height = 3,
                Fill = new SolidColorBrush(Color.FromRgb(255, 255, 100)),
                Opacity = HolographicIntensity
            };

            Canvas.SetLeft(particle, outline[i].X);
            Canvas.SetTop(particle, outline[i].Y);
            _particleCanvas.Children.Add(particle);

            AnimateSelectionParticle(particle, outline[i]);
        }
    }

    private void AnimateSelectionParticle(UIElement particle, Point startPoint)
    {
        var floatAnimation = new DoubleAnimation
        {
            From = startPoint.Y,
            To = startPoint.Y - 20,
            Duration = TimeSpan.FromSeconds(2),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = HolographicIntensity,
            To = 0,
            Duration = TimeSpan.FromSeconds(2)
        };

        floatAnimation.Completed += (s, e) => _particleCanvas.Children.Remove(particle);

        particle.BeginAnimation(Canvas.TopProperty, floatAnimation);
        particle.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private List<Point> GetCardOutlinePoints()
    {
        var points = new List<Point>();
        var cardRect = new Rect(5, 5, Width - 10, Height - 10);
        
        // Generate points along the perimeter
        for (double t = 0; t < 1; t += 0.1)
        {
            if (t < 0.25) // Top edge
                points.Add(new Point(cardRect.Left + t * 4 * cardRect.Width, cardRect.Top));
            else if (t < 0.5) // Right edge
                points.Add(new Point(cardRect.Right, cardRect.Top + (t - 0.25) * 4 * cardRect.Height));
            else if (t < 0.75) // Bottom edge
                points.Add(new Point(cardRect.Right - (t - 0.5) * 4 * cardRect.Width, cardRect.Bottom));
            else // Left edge
                points.Add(new Point(cardRect.Left, cardRect.Bottom - (t - 0.75) * 4 * cardRect.Height));
        }
        
        return points;
    }

    private void ClearParticleEffects()
    {
        foreach (var particle in _particleEffects.ToList())
        {
            _particleCanvas.Children.Remove(particle);
        }
        _particleEffects.Clear();
        _particleCanvas.Children.Clear();
    }

    #endregion

    #region Helper Methods

    private Brush CreateModuleCardBrush()
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        return new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 1),
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Color.FromArgb(80, colors.Primary.R, colors.Primary.G, colors.Primary.B), 0),
                new GradientStop(Color.FromArgb(40, colors.Secondary.R, colors.Secondary.G, colors.Secondary.B), 0.5),
                new GradientStop(Color.FromArgb(60, colors.Accent.R, colors.Accent.G, colors.Accent.B), 1)
            }
        };
    }

    private Brush CreateModuleCardBorderBrush()
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        return new SolidColorBrush(colors.Primary);
    }

    private Brush CreateDragPreviewBrush()
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        return new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 1),
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Color.FromArgb(120, colors.Primary.R, colors.Primary.G, colors.Primary.B), 0),
                new GradientStop(Color.FromArgb(80, colors.Secondary.R, colors.Secondary.G, colors.Secondary.B), 1)
            }
        };
    }

    private Effect CreateCardGlowEffect()
    {
        return new DropShadowEffect
        {
            Color = GetPrimaryColor(),
            BlurRadius = 10,
            ShadowDepth = 0,
            Opacity = 0.6
        };
    }

    private Effect CreateTextGlow()
    {
        return new DropShadowEffect
        {
            Color = GetPrimaryColor(),
            BlurRadius = 5,
            ShadowDepth = 0,
            Opacity = 0.8
        };
    }

    private Brush GetPrimaryBrush()
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        return new SolidColorBrush(colors.Primary);
    }

    private Brush GetSecondaryBrush()
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        return new SolidColorBrush(colors.Secondary);
    }

    private Brush GetModuleNameBrush()
    {
        return new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
    }

    private Brush GetStatValueBrush()
    {
        return new SolidColorBrush(Color.FromArgb(200, 200, 255, 200));
    }

    private Brush GetModuleTypeBrush()
    {
        return ModuleData?.Type switch
        {
            ModuleSlotType.High => new SolidColorBrush(Color.FromRgb(255, 100, 100)),
            ModuleSlotType.Medium => new SolidColorBrush(Color.FromRgb(255, 255, 100)),
            ModuleSlotType.Low => new SolidColorBrush(Color.FromRgb(100, 255, 100)),
            ModuleSlotType.Rig => new SolidColorBrush(Color.FromRgb(100, 100, 255)),
            ModuleSlotType.Subsystem => new SolidColorBrush(Color.FromRgb(255, 100, 255)),
            _ => GetPrimaryBrush()
        };
    }

    private Color GetModuleTypeColor()
    {
        return ModuleData?.Type switch
        {
            ModuleSlotType.High => Color.FromRgb(255, 100, 100),
            ModuleSlotType.Medium => Color.FromRgb(255, 255, 100),
            ModuleSlotType.Low => Color.FromRgb(100, 255, 100),
            ModuleSlotType.Rig => Color.FromRgb(100, 100, 255),
            ModuleSlotType.Subsystem => Color.FromRgb(255, 100, 255),
            _ => GetPrimaryColor()
        };
    }

    private string GetModuleTypeSymbol()
    {
        return ModuleData?.Type switch
        {
            ModuleSlotType.High => "H",
            ModuleSlotType.Medium => "M",
            ModuleSlotType.Low => "L",
            ModuleSlotType.Rig => "R",
            ModuleSlotType.Subsystem => "S",
            _ => "?"
        };
    }

    private Brush GetMetaLevelBrush()
    {
        return ModuleData?.TechLevel switch
        {
            1 => new SolidColorBrush(Color.FromRgb(150, 150, 150)),
            2 => new SolidColorBrush(Color.FromRgb(100, 200, 100)),
            3 => new SolidColorBrush(Color.FromRgb(100, 150, 255)),
            _ => GetSecondaryBrush()
        };
    }

    private Color GetPrimaryColor()
    {
        var colors = EVEColorSchemeHelper.GetColors(EVEColorScheme);
        return colors.Primary;
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableAnimations && !_isSimplifiedMode)
        {
            _animationTimer.Start();
        }

        UpdateModuleContent();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer.Stop();
        _dragTimer.Stop();
        ClearParticleEffects();
    }

    private void OnAnimationTimerTick(object sender, EventArgs e)
    {
        if (!EnableAnimations || _isSimplifiedMode) return;

        _animationPhase += 0.1;
        if (_animationPhase > Math.PI * 2)
            _animationPhase = 0;

        // Update any phase-based animations
        if (_isSelected)
        {
            var intensity = (Math.Sin(_animationPhase * 2) + 1) / 2;
            _moduleCard.Opacity = HolographicIntensity * (0.8 + intensity * 0.2);
        }
    }

    private void OnDragTimerTick(object sender, EventArgs e)
    {
        if (!_isDragging) return;

        var currentPosition = Mouse.GetPosition(_mainCanvas);
        
        // Update drag preview position
        if (_dragCanvas.Children.Count > 0)
        {
            var preview = _dragCanvas.Children[0];
            Canvas.SetLeft(preview, currentPosition.X - Width / 2);
            Canvas.SetTop(preview, currentPosition.Y - Height / 2);
        }

        DragProgress?.Invoke(this, new ModuleDragEventArgs
        {
            Module = ModuleData,
            StartPosition = _dragStartPoint,
            CurrentPosition = currentPosition,
            Timestamp = DateTime.Now
        });
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(_mainCanvas);
        _lastDragPoint = _dragStartPoint;
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_isDragging)
        {
            var dropPosition = e.GetPosition(_mainCanvas);
            CompleteDrag(dropPosition);
        }
        else
        {
            // Handle selection
            SetSelected(!_isSelected);
        }
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && !_isDragging)
        {
            var currentPosition = e.GetPosition(_mainCanvas);
            var distance = Point.Subtract(currentPosition, _dragStartPoint).Length;
            
            if (distance > DragThreshold)
            {
                StartDrag();
            }
        }
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (!_isDragging && EnableAnimations && !_isSimplifiedMode)
        {
            var hoverAnimation = new DoubleAnimation
            {
                From = HolographicIntensity,
                To = Math.Min(1.0, HolographicIntensity + 0.2),
                Duration = TimeSpan.FromMilliseconds(200)
            };
            _moduleCard.BeginAnimation(OpacityProperty, hoverAnimation);
        }
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (!_isDragging && !_isSelected)
        {
            var restoreAnimation = new DoubleAnimation
            {
                From = _moduleCard.Opacity,
                To = HolographicIntensity,
                Duration = TimeSpan.FromMilliseconds(200)
            };
            _moduleCard.BeginAnimation(OpacityProperty, restoreAnimation);
        }
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        var dropPosition = e.GetPosition(_mainCanvas);
        
        ModuleDropped?.Invoke(this, new ModuleDropEventArgs
        {
            TargetModule = ModuleData,
            DropPosition = dropPosition,
            Timestamp = DateTime.Now
        });

        HighlightAsDropTarget(false);
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        // Handle drag over logic
        e.Effects = DragDropEffects.Move;
        e.Handled = true;
    }

    private void OnDragEnter(object sender, DragEventArgs e)
    {
        HighlightAsDropTarget(true);
    }

    private void OnDragLeave(object sender, DragEventArgs e)
    {
        HighlightAsDropTarget(false);
    }

    #endregion

    #region Property Change Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDragDropModule module)
        {
            module.UpdateCardAppearance();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDragDropModule module)
        {
            module.ApplyEVEColorScheme();
        }
    }

    private static void OnModuleDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDragDropModule module)
        {
            module.UpdateModuleContent();
        }
    }

    private static void OnDragModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDragDropModule module)
        {
            module.UpdateCardAppearance();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDragDropModule module)
        {
            if ((bool)e.NewValue && !module._isSimplifiedMode)
            {
                module._animationTimer.Start();
            }
            else
            {
                module._animationTimer.Stop();
            }
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDragDropModule module && !(bool)e.NewValue)
        {
            module.ClearParticleEffects();
        }
    }

    private static void OnShowCompatibilityIndicatorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDragDropModule module)
        {
            module.UpdateCompatibilityIndicator();
        }
    }

    private static void OnEnableSnapToSlotChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Implementation for snap-to-slot behavior
    }

    private static void OnDragThresholdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Drag threshold updated
    }

    private static void OnAllowMultiSelectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Multi-select behavior updated
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
        ClearParticleEffects();
    }

    public void ExitSimplifiedMode()
    {
        _isSimplifiedMode = false;
        EnableAnimations = true;
        EnableParticleEffects = true;
        _animationTimer.Start();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => true;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity;
        
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
        UpdateCardAppearance();
        UpdateModuleContent();
    }

    #endregion
}

#region Supporting Classes and Enums

public class HoloModuleData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public ModuleSlotType Type { get; set; }
    public int TechLevel { get; set; } = 1;
    public int MetaLevel { get; set; } = 0;
    public double CPUCost { get; set; }
    public double PowerCost { get; set; }
    public double Volume { get; set; }
    public Dictionary<string, object> Attributes { get; set; } = new();
    public string Description { get; set; }
    public string IconPath { get; set; }
}

public enum DragMode
{
    Standard,
    Holographic,
    Ghost,
    Highlight
}

public enum CompatibilityState
{
    Unknown,
    Compatible,
    Incompatible,
    Warning
}

public class ModuleDragEventArgs : EventArgs
{
    public HoloModuleData Module { get; set; }
    public Point StartPosition { get; set; }
    public Point CurrentPosition { get; set; }
    public Point EndPosition { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ModuleDropEventArgs : EventArgs
{
    public HoloModuleData SourceModule { get; set; }
    public HoloModuleData TargetModule { get; set; }
    public Point DropPosition { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ModuleCompatibilityEventArgs : EventArgs
{
    public HoloModuleData Module { get; set; }
    public CompatibilityState Compatibility { get; set; }
    public string Reason { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ModuleSelectionEventArgs : EventArgs
{
    public HoloModuleData Module { get; set; }
    public bool IsSelected { get; set; }
    public DateTime Timestamp { get; set; }
}

#endregion