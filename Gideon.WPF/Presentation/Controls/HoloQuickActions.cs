// ==========================================================================
// HoloQuickActions.cs - Holographic Quick Actions Interface
// ==========================================================================
// Comprehensive quick actions system featuring holographic action panels,
// customizable shortcuts, context-sensitive actions, and gesture support.
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Holographic quick actions interface with customizable shortcuts and context-sensitive actions
/// </summary>
public class HoloQuickActions : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloQuickActions),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloQuickActions),
            new PropertyMetadata(EVEColorScheme.GoldAccent, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty CurrentContextProperty =
        DependencyProperty.Register(nameof(CurrentContext), typeof(ActionContext), typeof(HoloQuickActions),
            new PropertyMetadata(ActionContext.General, OnCurrentContextChanged));

    public static readonly DependencyProperty ActionLayoutProperty =
        DependencyProperty.Register(nameof(ActionLayout), typeof(QuickActionLayout), typeof(HoloQuickActions),
            new PropertyMetadata(QuickActionLayout.Grid, OnActionLayoutChanged));

    public static readonly DependencyProperty EnableGestureControlProperty =
        DependencyProperty.Register(nameof(EnableGestureControl), typeof(bool), typeof(HoloQuickActions),
            new PropertyMetadata(true, OnEnableGestureControlChanged));

    public static readonly DependencyProperty EnableActionAnimationsProperty =
        DependencyProperty.Register(nameof(EnableActionAnimations), typeof(bool), typeof(HoloQuickActions),
            new PropertyMetadata(true, OnEnableActionAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloQuickActions),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowActionLabelsProperty =
        DependencyProperty.Register(nameof(ShowActionLabels), typeof(bool), typeof(HoloQuickActions),
            new PropertyMetadata(true));

    public static readonly DependencyProperty QuickActionsProperty =
        DependencyProperty.Register(nameof(QuickActions), typeof(ObservableCollection<HoloQuickAction>), typeof(HoloQuickActions),
            new PropertyMetadata(null));

    public static readonly DependencyProperty FavoriteActionsProperty =
        DependencyProperty.Register(nameof(FavoriteActions), typeof(ObservableCollection<HoloQuickAction>), typeof(HoloQuickActions),
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

    public ActionContext CurrentContext
    {
        get => (ActionContext)GetValue(CurrentContextProperty);
        set => SetValue(CurrentContextProperty, value);
    }

    public QuickActionLayout ActionLayout
    {
        get => (QuickActionLayout)GetValue(ActionLayoutProperty);
        set => SetValue(ActionLayoutProperty, value);
    }

    public bool EnableGestureControl
    {
        get => (bool)GetValue(EnableGestureControlProperty);
        set => SetValue(EnableGestureControlProperty, value);
    }

    public bool EnableActionAnimations
    {
        get => (bool)GetValue(EnableActionAnimationsProperty);
        set => SetValue(EnableActionAnimationsProperty, value);
    }

    public bool EnableParticleEffects
    {
        get => (bool)GetValue(EnableParticleEffectsProperty);
        set => SetValue(EnableParticleEffectsProperty, value);
    }

    public bool ShowActionLabels
    {
        get => (bool)GetValue(ShowActionLabelsProperty);
        set => SetValue(ShowActionLabelsProperty, value);
    }

    public ObservableCollection<HoloQuickAction> QuickActions
    {
        get => (ObservableCollection<HoloQuickAction>)GetValue(QuickActionsProperty);
        set => SetValue(QuickActionsProperty, value);
    }

    public ObservableCollection<HoloQuickAction> FavoriteActions
    {
        get => (ObservableCollection<HoloQuickAction>)GetValue(FavoriteActionsProperty);
        set => SetValue(FavoriteActionsProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloQuickActionEventArgs> ActionExecuted;
    public event EventHandler<HoloQuickActionEventArgs> ActionFavorited;
    public event EventHandler<HoloQuickActionEventArgs> ActionCustomized;
    public event EventHandler<HoloQuickActionsEventArgs> ContextChanged;
    public event EventHandler<HoloQuickActionsEventArgs> LayoutChanged;
    public event EventHandler<HoloQuickActionsEventArgs> GestureDetected;

    #endregion

    #region Private Fields

    private readonly DispatcherTimer _animationTimer;
    private readonly Random _random = new();
    private Canvas _mainCanvas;
    private Grid _actionGrid;
    private WrapPanel _actionWrapPanel;
    private StackPanel _favoritePanel;
    private ComboBox _contextComboBox;
    private ToggleButton _layoutToggle;
    private Button _customizeButton;
    private Canvas _particleCanvas;
    private readonly List<UIElement> _particles = new();
    private readonly Dictionary<string, HoloActionControl> _actionControls = new();
    private bool _isSimplifiedMode;
    private bool _isDragging;
    private Point _lastGesturePoint;

    #endregion

    #region Constructor

    public HoloQuickActions()
    {
        InitializeComponent();
        InitializeCollections();
        InitializeTimer();
        LoadDefaultActions();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 600;
        Height = 400;
        Background = new SolidColorBrush(Color.FromArgb(25, 255, 200, 100));
        BorderBrush = new SolidColorBrush(Color.FromArgb(100, 255, 200, 100));
        BorderThickness = new Thickness(1);
        Effect = new DropShadowEffect
        {
            Color = Color.FromArgb(80, 255, 200, 100),
            BlurRadius = 12,
            ShadowDepth = 4,
            Direction = 315
        };

        CreateMainLayout();
        ApplyEVEColorScheme();
    }

    private void CreateMainLayout()
    {
        _mainCanvas = new Canvas();
        Content = _mainCanvas;

        // Background with holographic effect
        var backgroundRect = new Rectangle
        {
            Fill = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(40, 255, 200, 100), 0),
                    new GradientStop(Color.FromArgb(15, 200, 150, 50), 0.5),
                    new GradientStop(Color.FromArgb(30, 255, 200, 100), 1)
                }
            },
            Width = 600,
            Height = 400
        };
        _mainCanvas.Children.Add(backgroundRect);

        // Main container
        var mainContainer = new DockPanel
        {
            Width = 600,
            Height = 400,
            Margin = new Thickness(10)
        };
        _mainCanvas.Children.Add(mainContainer);

        CreateHeaderSection(mainContainer);
        CreateActionArea(mainContainer);
        CreateParticleCanvas();
    }

    private void CreateHeaderSection(DockPanel container)
    {
        var headerPanel = new DockPanel
        {
            Height = 50,
            Margin = new Thickness(0, 0, 0, 10)
        };
        DockPanel.SetDock(headerPanel, Dock.Top);
        container.Children.Add(headerPanel);

        // Title
        var titleText = new TextBlock
        {
            Text = "QUICK ACTIONS",
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 200, 100)),
            VerticalAlignment = VerticalAlignment.Center,
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(100, 255, 200, 100),
                BlurRadius = 6,
                ShadowDepth = 2
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

        // Context selector
        _contextComboBox = new ComboBox
        {
            Width = 120,
            Height = 30,
            Margin = new Thickness(0, 0, 10, 0)
        };
        _contextComboBox.Items.Add(new ComboBoxItem { Content = "General", Tag = ActionContext.General });
        _contextComboBox.Items.Add(new ComboBoxItem { Content = "Ship Fitting", Tag = ActionContext.ShipFitting });
        _contextComboBox.Items.Add(new ComboBoxItem { Content = "Market", Tag = ActionContext.Market });
        _contextComboBox.Items.Add(new ComboBoxItem { Content = "Character", Tag = ActionContext.Character });
        _contextComboBox.SelectedIndex = 0;
        _contextComboBox.SelectionChanged += OnContextSelectionChanged;
        controlsPanel.Children.Add(_contextComboBox);

        // Layout toggle
        _layoutToggle = new ToggleButton
        {
            Content = "GRID",
            Width = 60,
            Height = 30,
            Margin = new Thickness(0, 0, 10, 0),
            Style = CreateHolographicButtonStyle()
        };
        _layoutToggle.Checked += (s, e) => { ActionLayout = QuickActionLayout.Radial; _layoutToggle.Content = "RADIAL"; };
        _layoutToggle.Unchecked += (s, e) => { ActionLayout = QuickActionLayout.Grid; _layoutToggle.Content = "GRID"; };
        controlsPanel.Children.Add(_layoutToggle);

        // Customize button
        _customizeButton = new Button
        {
            Content = "CUSTOMIZE",
            Width = 90,
            Height = 30,
            Style = CreateHolographicButtonStyle()
        };
        _customizeButton.Click += OnCustomizeClicked;
        controlsPanel.Children.Add(_customizeButton);
    }

    private void CreateActionArea(DockPanel container)
    {
        var actionContainer = new Grid();
        container.Children.Add(actionContainer);

        // Grid layout
        _actionGrid = new Grid
        {
            Visibility = ActionLayout == QuickActionLayout.Grid ? Visibility.Visible : Visibility.Collapsed
        };
        
        for (int i = 0; i < 4; i++)
        {
            _actionGrid.RowDefinitions.Add(new RowDefinition());
            _actionGrid.ColumnDefinitions.Add(new ColumnDefinition());
        }
        
        actionContainer.Children.Add(_actionGrid);

        // Wrap panel layout (alternative to radial)
        _actionWrapPanel = new WrapPanel
        {
            Visibility = ActionLayout == QuickActionLayout.Radial ? Visibility.Visible : Visibility.Collapsed,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        actionContainer.Children.Add(_actionWrapPanel);

        // Favorites panel
        _favoritePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Height = 60,
            Margin = new Thickness(0, 10, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom
        };
        actionContainer.Children.Add(_favoritePanel);
    }

    private void CreateParticleCanvas()
    {
        _particleCanvas = new Canvas
        {
            Width = 600,
            Height = 400,
            IsHitTestVisible = false
        };
        _mainCanvas.Children.Add(_particleCanvas);
    }

    private void InitializeCollections()
    {
        QuickActions = new ObservableCollection<HoloQuickAction>();
        FavoriteActions = new ObservableCollection<HoloQuickAction>();
        QuickActions.CollectionChanged += OnQuickActionsCollectionChanged;
        FavoriteActions.CollectionChanged += OnFavoriteActionsCollectionChanged;
    }

    private void InitializeTimer()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };
        _animationTimer.Tick += OnAnimationTimerTick;
        _animationTimer.Start();
    }

    #endregion

    #region Sample Data

    private void LoadDefaultActions()
    {
        var defaultActions = new[]
        {
            new HoloQuickAction
            {
                Id = "fit_ship",
                Title = "Fit Ship",
                Description = "Open ship fitting interface",
                IconPath = "M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z",
                ActionType = "navigation",
                Context = ActionContext.ShipFitting,
                Hotkey = "Ctrl+F",
                Color = Color.FromArgb(255, 100, 200, 255),
                Priority = ActionPriority.High
            },
            new HoloQuickAction
            {
                Id = "market_browser",
                Title = "Market",
                Description = "Browse market data",
                IconPath = "M7,15H9C9,16.08 10.37,17 12,17C13.63,17 15,16.08 15,15C15,13.9 13.96,13.5 11.76,12.97C9.64,12.44 7,11.78 7,9C7,7.21 8.47,5.69 10.5,5.18V3H13.5V5.18C15.53,5.69 17,7.21 17,9H15C15,7.92 13.63,7 12,7C10.37,7 9,7.92 9,9C9,10.1 10.04,10.5 12.24,11.03C14.36,11.56 17,12.22 17,15C17,16.79 15.53,18.31 13.5,18.82V21H10.5V18.82C8.47,18.31 7,16.79 7,15Z",
                ActionType = "navigation",
                Context = ActionContext.Market,
                Hotkey = "Ctrl+M",
                Color = Color.FromArgb(255, 255, 200, 100),
                Priority = ActionPriority.High
            },
            new HoloQuickAction
            {
                Id = "character_sheet",
                Title = "Character",
                Description = "View character information",
                IconPath = "M12,4A4,4 0 0,1 16,8A4,4 0 0,1 12,12A4,4 0 0,1 8,8A4,4 0 0,1 12,4M12,14C16.42,14 20,15.79 20,18V20H4V18C4,15.79 7.58,14 12,14Z",
                ActionType = "navigation",
                Context = ActionContext.Character,
                Hotkey = "Ctrl+C",
                Color = Color.FromArgb(255, 150, 255, 150),
                Priority = ActionPriority.Medium
            },
            new HoloQuickAction
            {
                Id = "quick_search",
                Title = "Search",
                Description = "Search across all modules",
                IconPath = "M9.5,3A6.5,6.5 0 0,1 16,9.5C16,11.11 15.41,12.59 14.44,13.73L14.71,14H15.5L20.5,19L19,20.5L14,15.5V14.71L13.73,14.44C12.59,15.41 11.11,16 9.5,16A6.5,6.5 0 0,1 3,9.5A6.5,6.5 0 0,1 9.5,3M9.5,5C7,5 5,7 5,9.5C5,12 7,14 9.5,14C12,14 14,12 14,9.5C14,7 12,5 9.5,5Z",
                ActionType = "utility",
                Context = ActionContext.General,
                Hotkey = "Ctrl+Shift+F",
                Color = Color.FromArgb(255, 200, 150, 255),
                Priority = ActionPriority.High
            },
            new HoloQuickAction
            {
                Id = "notifications",
                Title = "Alerts",
                Description = "View notifications",
                IconPath = "M21,19V20H3V19L5,17V11C5,7.9 7.03,5.17 10,4.29C10,4.19 10,4.1 10,4A2,2 0 0,1 12,2A2,2 0 0,1 14,4C14,4.1 14,4.19 14,4.29C16.97,5.17 19,7.9 19,11V17L21,19M14,21A2,2 0 0,1 12,23A2,2 0 0,1 10,21",
                ActionType = "utility",
                Context = ActionContext.General,
                Hotkey = "Ctrl+N",
                Color = Color.FromArgb(255, 255, 150, 100),
                Priority = ActionPriority.Medium
            },
            new HoloQuickAction
            {
                Id = "settings",
                Title = "Settings",
                Description = "Application preferences",
                IconPath = "M12,15.5A3.5,3.5 0 0,1 8.5,12A3.5,3.5 0 0,1 12,8.5A3.5,3.5 0 0,1 15.5,12A3.5,3.5 0 0,1 12,15.5M19.43,12.97C19.47,12.65 19.5,12.33 19.5,12C19.5,11.67 19.47,11.34 19.43,11L21.54,9.37C21.73,9.22 21.78,8.95 21.66,8.73L19.66,5.27C19.54,5.05 19.27,4.96 19.05,5.05L16.56,6.05C16.04,5.66 15.5,5.32 14.87,5.07L14.5,2.42C14.46,2.18 14.25,2 14,2H10C9.75,2 9.54,2.18 9.5,2.42L9.13,5.07C8.5,5.32 7.96,5.66 7.44,6.05L4.95,5.05C4.73,4.96 4.46,5.05 4.34,5.27L2.34,8.73C2.22,8.95 2.27,9.22 2.46,9.37L4.57,11C4.53,11.34 4.5,11.67 4.5,12C4.5,12.33 4.53,12.65 4.57,12.97L2.46,14.63C2.27,14.78 2.22,15.05 2.34,15.27L4.34,18.73C4.46,18.95 4.73,19.03 4.95,18.95L7.44,17.94C7.96,18.34 8.5,18.68 9.13,18.93L9.5,21.58C9.54,21.82 9.75,22 10,22H14C14.25,22 14.46,21.82 14.5,21.58L14.87,18.93C15.5,18.68 16.04,18.34 16.56,17.94L19.05,18.95C19.27,19.03 19.54,18.95 19.66,18.73L21.66,15.27C21.78,15.05 21.73,14.78 21.54,14.63L19.43,12.97Z",
                ActionType = "utility",
                Context = ActionContext.General,
                Hotkey = "Ctrl+,",
                Color = Color.FromArgb(255, 100, 150, 255),
                Priority = ActionPriority.Low
            },
            new HoloQuickAction
            {
                Id = "help",
                Title = "Help",
                Description = "Documentation and tutorials",
                IconPath = "M11,18H13V16H11V18M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,20C7.59,20 4,16.41 4,12C4,7.59 7.59,4 12,4C16.41,4 20,7.59 20,12C20,16.41 16.41,20 12,20M12,6A4,4 0 0,0 8,10H10A2,2 0 0,1 12,8A2,2 0 0,1 14,10C14,12 11,11.75 11,15H13C13,12.75 16,12.5 16,10A4,4 0 0,0 12,6Z",
                ActionType = "utility",
                Context = ActionContext.General,
                Hotkey = "F1",
                Color = Color.FromArgb(255, 150, 255, 200),
                Priority = ActionPriority.Low
            },
            new HoloQuickAction
            {
                Id = "refresh_data",
                Title = "Refresh",
                Description = "Reload current data",
                IconPath = "M17.65,6.35C16.2,4.9 14.21,4 12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20C15.73,20 18.84,17.45 19.73,14H17.65C16.83,16.33 14.61,18 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6C13.66,6 15.14,6.69 16.22,7.78L13,11H20V4L17.65,6.35Z",
                ActionType = "utility",
                Context = ActionContext.General,
                Hotkey = "F5",
                Color = Color.FromArgb(255, 200, 200, 100),
                Priority = ActionPriority.Medium
            }
        };

        foreach (var action in defaultActions)
        {
            QuickActions.Add(action);
        }

        // Add some favorites
        FavoriteActions.Add(defaultActions[0]); // Fit Ship
        FavoriteActions.Add(defaultActions[1]); // Market
        FavoriteActions.Add(defaultActions[3]); // Search
    }

    #endregion

    #region Action Management

    public async Task ExecuteActionAsync(string actionId)
    {
        var action = QuickActions.FirstOrDefault(a => a.Id == actionId);
        if (action == null) return;

        if (EnableActionAnimations && !_isSimplifiedMode)
        {
            await AnimateActionExecution(action);
        }

        if (EnableParticleEffects && !_isSimplifiedMode)
        {
            SpawnActionParticles(action);
        }

        // Track usage
        action.UsageCount++;
        action.LastUsed = DateTime.Now;

        ActionExecuted?.Invoke(this, new HoloQuickActionEventArgs
        {
            Action = action,
            Timestamp = DateTime.Now
        });
    }

    public async Task AddToFavoritesAsync(string actionId)
    {
        var action = QuickActions.FirstOrDefault(a => a.Id == actionId);
        if (action == null || FavoriteActions.Contains(action)) return;

        FavoriteActions.Add(action);
        action.IsFavorite = true;

        if (EnableActionAnimations && !_isSimplifiedMode)
        {
            AnimateFavoriteAction(action);
        }

        ActionFavorited?.Invoke(this, new HoloQuickActionEventArgs
        {
            Action = action,
            Timestamp = DateTime.Now
        });
    }

    public async Task RemoveFromFavoritesAsync(string actionId)
    {
        var action = FavoriteActions.FirstOrDefault(a => a.Id == actionId);
        if (action == null) return;

        FavoriteActions.Remove(action);
        action.IsFavorite = false;

        RefreshFavoriteDisplay();
    }

    public async Task CustomizeActionAsync(string actionId, HoloQuickActionCustomization customization)
    {
        var action = QuickActions.FirstOrDefault(a => a.Id == actionId);
        if (action == null) return;

        if (!string.IsNullOrEmpty(customization.Title))
            action.Title = customization.Title;
        
        if (!string.IsNullOrEmpty(customization.Description))
            action.Description = customization.Description;
        
        if (!string.IsNullOrEmpty(customization.Hotkey))
            action.Hotkey = customization.Hotkey;
        
        if (customization.Color.HasValue)
            action.Color = customization.Color.Value;

        ActionCustomized?.Invoke(this, new HoloQuickActionEventArgs
        {
            Action = action,
            Customization = customization,
            Timestamp = DateTime.Now
        });

        RefreshActionDisplay();
    }

    #endregion

    #region Layout Management

    private void SwitchToGridLayout()
    {
        _actionGrid.Visibility = Visibility.Visible;
        _actionWrapPanel.Visibility = Visibility.Collapsed;
        PopulateGridLayout();
    }

    private void SwitchToRadialLayout()
    {
        _actionGrid.Visibility = Visibility.Collapsed;
        _actionWrapPanel.Visibility = Visibility.Visible;
        PopulateRadialLayout();
    }

    private void PopulateGridLayout()
    {
        _actionGrid.Children.Clear();
        _actionControls.Clear();

        var contextActions = GetActionsForCurrentContext();
        int row = 0, col = 0;

        foreach (var action in contextActions.Take(16))
        {
            var actionControl = CreateActionControl(action);
            _actionControls[action.Id] = actionControl;

            Grid.SetRow(actionControl, row);
            Grid.SetColumn(actionControl, col);
            _actionGrid.Children.Add(actionControl);

            col++;
            if (col >= 4)
            {
                col = 0;
                row++;
            }
        }
    }

    private void PopulateRadialLayout()
    {
        _actionWrapPanel.Children.Clear();
        _actionControls.Clear();

        var contextActions = GetActionsForCurrentContext();
        
        foreach (var action in contextActions.Take(12))
        {
            var actionControl = CreateActionControl(action);
            _actionControls[action.Id] = actionControl;
            _actionWrapPanel.Children.Add(actionControl);
        }
    }

    private void RefreshActionDisplay()
    {
        if (ActionLayout == QuickActionLayout.Grid)
        {
            PopulateGridLayout();
        }
        else
        {
            PopulateRadialLayout();
        }
    }

    private void RefreshFavoriteDisplay()
    {
        _favoritePanel.Children.Clear();

        foreach (var favorite in FavoriteActions.Take(8))
        {
            var favoriteControl = CreateFavoriteControl(favorite);
            _favoritePanel.Children.Add(favoriteControl);
        }
    }

    private List<HoloQuickAction> GetActionsForCurrentContext()
    {
        return QuickActions
            .Where(a => a.Context == CurrentContext || a.Context == ActionContext.General)
            .OrderByDescending(a => a.Priority)
            .ThenByDescending(a => a.UsageCount)
            .ToList();
    }

    #endregion

    #region UI Creation Helpers

    private HoloActionControl CreateActionControl(HoloQuickAction action)
    {
        var control = new HoloActionControl
        {
            Width = 120,
            Height = 120,
            Margin = new Thickness(5),
            Action = action,
            Style = CreateActionControlStyle(action)
        };

        control.MouseLeftButtonUp += async (s, e) => await ExecuteActionAsync(action.Id);
        control.MouseRightButtonUp += async (s, e) => await ShowActionContextMenu(action, control);

        if (EnableGestureControl)
        {
            control.MouseDown += OnActionMouseDown;
            control.MouseMove += OnActionMouseMove;
            control.MouseUp += OnActionMouseUp;
        }

        return control;
    }

    private HoloActionControl CreateFavoriteControl(HoloQuickAction action)
    {
        var control = new HoloActionControl
        {
            Width = 60,
            Height = 60,
            Margin = new Thickness(3),
            Action = action,
            Style = CreateFavoriteControlStyle(action)
        };

        control.MouseLeftButtonUp += async (s, e) => await ExecuteActionAsync(action.Id);
        return control;
    }

    private Style CreateActionControlStyle(HoloQuickAction action)
    {
        var style = new Style(typeof(HoloActionControl));
        
        var backgroundBrush = new SolidColorBrush(Color.FromArgb(60, action.Color.R, action.Color.G, action.Color.B));
        var borderBrush = new SolidColorBrush(Color.FromArgb(120, action.Color.R, action.Color.G, action.Color.B));

        style.Setters.Add(new Setter(BackgroundProperty, backgroundBrush));
        style.Setters.Add(new Setter(BorderBrushProperty, borderBrush));
        style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(2)));
        style.Setters.Add(new Setter(EffectProperty, new DropShadowEffect
        {
            Color = Color.FromArgb(80, action.Color.R, action.Color.G, action.Color.B),
            BlurRadius = 8,
            ShadowDepth = 3
        }));

        return style;
    }

    private Style CreateFavoriteControlStyle(HoloQuickAction action)
    {
        var style = new Style(typeof(HoloActionControl));
        
        var backgroundBrush = new SolidColorBrush(Color.FromArgb(80, action.Color.R, action.Color.G, action.Color.B));
        var borderBrush = new SolidColorBrush(Color.FromArgb(150, action.Color.R, action.Color.G, action.Color.B));

        style.Setters.Add(new Setter(BackgroundProperty, backgroundBrush));
        style.Setters.Add(new Setter(BorderBrushProperty, borderBrush));
        style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(1)));

        return style;
    }

    private Style CreateHolographicButtonStyle()
    {
        var style = new Style(typeof(Button));
        style.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Color.FromArgb(50, 255, 200, 100))));
        style.Setters.Add(new Setter(BorderBrushProperty, new SolidColorBrush(Color.FromArgb(150, 255, 200, 100))));
        style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(ForegroundProperty, new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))));
        style.Setters.Add(new Setter(FontSizeProperty, 11.0));
        style.Setters.Add(new Setter(FontWeightProperty, FontWeights.Bold));
        return style;
    }

    #endregion

    #region Animation Methods

    private async Task AnimateActionExecution(HoloQuickAction action)
    {
        if (_isSimplifiedMode) return;

        if (_actionControls.TryGetValue(action.Id, out var control))
        {
            var scaleAnimation = new DoubleAnimation
            {
                From = 1,
                To = 1.2,
                Duration = TimeSpan.FromMilliseconds(100),
                AutoReverse = true
            };

            var transform = new ScaleTransform();
            control.RenderTransform = transform;
            control.RenderTransformOrigin = new Point(0.5, 0.5);

            transform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);

            await Task.Delay(200);
        }
    }

    private void AnimateFavoriteAction(HoloQuickAction action)
    {
        if (_isSimplifiedMode) return;

        if (_actionControls.TryGetValue(action.Id, out var control))
        {
            var pulseAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0.5,
                Duration = TimeSpan.FromMilliseconds(300),
                RepeatBehavior = new RepeatBehavior(3),
                AutoReverse = true
            };

            control.BeginAnimation(OpacityProperty, pulseAnimation);
        }
    }

    private void SpawnActionParticles(HoloQuickAction action)
    {
        if (_isSimplifiedMode || !EnableParticleEffects) return;

        var centerX = 300;
        var centerY = 200;
        var particleCount = 15;

        for (int i = 0; i < particleCount; i++)
        {
            var particle = new Ellipse
            {
                Width = _random.Next(3, 8),
                Height = _random.Next(3, 8),
                Fill = new SolidColorBrush(Color.FromArgb(
                    (byte)_random.Next(150, 255), 
                    action.Color.R, action.Color.G, action.Color.B))
            };

            var angle = _random.NextDouble() * Math.PI * 2;
            var distance = _random.Next(20, 100);
            var targetX = centerX + Math.Cos(angle) * distance;
            var targetY = centerY + Math.Sin(angle) * distance;

            Canvas.SetLeft(particle, centerX);
            Canvas.SetTop(particle, centerY);
            _particleCanvas.Children.Add(particle);
            _particles.Add(particle);

            AnimateActionParticle(particle, targetX, targetY);
        }

        CleanupParticles();
    }

    private void AnimateActionParticle(UIElement particle, double targetX, double targetY)
    {
        var moveXAnimation = new DoubleAnimation
        {
            From = Canvas.GetLeft(particle),
            To = targetX,
            Duration = TimeSpan.FromMilliseconds(_random.Next(800, 1200)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var moveYAnimation = new DoubleAnimation
        {
            From = Canvas.GetTop(particle),
            To = targetY,
            Duration = TimeSpan.FromMilliseconds(_random.Next(800, 1200)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(_random.Next(800, 1200))
        };

        moveXAnimation.Completed += (s, e) =>
        {
            _particleCanvas.Children.Remove(particle);
            _particles.Remove(particle);
        };

        particle.BeginAnimation(Canvas.LeftProperty, moveXAnimation);
        particle.BeginAnimation(Canvas.TopProperty, moveYAnimation);
        particle.BeginAnimation(OpacityProperty, fadeAnimation);
    }

    private void CleanupParticles()
    {
        if (_particles.Count > 150)
        {
            var particlesToRemove = _particles.Take(75).ToList();
            foreach (var particle in particlesToRemove)
            {
                _particleCanvas.Children.Remove(particle);
                _particles.Remove(particle);
            }
        }
    }

    #endregion

    #region Gesture Control

    private void OnActionMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!EnableGestureControl) return;

        _isDragging = true;
        _lastGesturePoint = e.GetPosition(this);
        
        if (sender is FrameworkElement element)
        {
            element.CaptureMouse();
        }
    }

    private void OnActionMouseMove(object sender, MouseEventArgs e)
    {
        if (!EnableGestureControl || !_isDragging) return;

        var currentPoint = e.GetPosition(this);
        var distance = Point.Subtract(currentPoint, _lastGesturePoint).Length;

        if (distance > 30) // Gesture threshold
        {
            DetectGesture(_lastGesturePoint, currentPoint);
            _lastGesturePoint = currentPoint;
        }
    }

    private void OnActionMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!EnableGestureControl) return;

        _isDragging = false;
        
        if (sender is FrameworkElement element)
        {
            element.ReleaseMouseCapture();
        }
    }

    private void DetectGesture(Point startPoint, Point endPoint)
    {
        var vector = Point.Subtract(endPoint, startPoint);
        var gestureType = GestureType.Unknown;

        if (Math.Abs(vector.X) > Math.Abs(vector.Y))
        {
            gestureType = vector.X > 0 ? GestureType.SwipeRight : GestureType.SwipeLeft;
        }
        else
        {
            gestureType = vector.Y > 0 ? GestureType.SwipeDown : GestureType.SwipeUp;
        }

        HandleGesture(gestureType);
    }

    private void HandleGesture(GestureType gestureType)
    {
        switch (gestureType)
        {
            case GestureType.SwipeRight:
                // Switch to next context
                SwitchToNextContext();
                break;
            case GestureType.SwipeLeft:
                // Switch to previous context
                SwitchToPreviousContext();
                break;
            case GestureType.SwipeUp:
                // Toggle layout
                ActionLayout = ActionLayout == QuickActionLayout.Grid ? QuickActionLayout.Radial : QuickActionLayout.Grid;
                break;
            case GestureType.SwipeDown:
                // Show favorites
                FocusOnFavorites();
                break;
        }

        GestureDetected?.Invoke(this, new HoloQuickActionsEventArgs
        {
            GestureType = gestureType,
            Timestamp = DateTime.Now
        });
    }

    #endregion

    #region Context Management

    private void SwitchToNextContext()
    {
        var contexts = Enum.GetValues<ActionContext>();
        var currentIndex = Array.IndexOf(contexts, CurrentContext);
        var nextIndex = (currentIndex + 1) % contexts.Length;
        CurrentContext = contexts[nextIndex];
    }

    private void SwitchToPreviousContext()
    {
        var contexts = Enum.GetValues<ActionContext>();
        var currentIndex = Array.IndexOf(contexts, CurrentContext);
        var previousIndex = currentIndex == 0 ? contexts.Length - 1 : currentIndex - 1;
        CurrentContext = contexts[previousIndex];
    }

    private void FocusOnFavorites()
    {
        // Animate focus to favorites panel
        if (EnableActionAnimations && !_isSimplifiedMode)
        {
            var focusAnimation = new DoubleAnimation
            {
                From = 1,
                To = 1.2,
                Duration = TimeSpan.FromMilliseconds(200),
                AutoReverse = true
            };

            var transform = new ScaleTransform();
            _favoritePanel.RenderTransform = transform;
            _favoritePanel.RenderTransformOrigin = new Point(0.5, 0.5);

            transform.BeginAnimation(ScaleTransform.ScaleXProperty, focusAnimation);
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, focusAnimation);
        }
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        RefreshActionDisplay();
        RefreshFavoriteDisplay();
        
        if (EnableActionAnimations && !_isSimplifiedMode)
        {
            AnimateInitialLoad();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        CleanupAllParticles();
    }

    private void OnQuickActionsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        RefreshActionDisplay();
    }

    private void OnFavoriteActionsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        RefreshFavoriteDisplay();
    }

    private void OnAnimationTimerTick(object sender, EventArgs e)
    {
        // Update particle positions and cleanup
        if (EnableParticleEffects && !_isSimplifiedMode)
        {
            UpdateParticleEffects();
        }
    }

    private void OnContextSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_contextComboBox.SelectedItem is ComboBoxItem item && item.Tag is ActionContext context)
        {
            CurrentContext = context;
        }
    }

    private void OnCustomizeClicked(object sender, RoutedEventArgs e)
    {
        // Open customization dialog
        ShowCustomizationDialog();
    }

    private async Task ShowActionContextMenu(HoloQuickAction action, HoloActionControl control)
    {
        // Show context menu for action customization
        var contextMenu = new ContextMenu();
        
        var favoriteItem = new MenuItem
        {
            Header = action.IsFavorite ? "Remove from Favorites" : "Add to Favorites"
        };
        favoriteItem.Click += async (s, e) =>
        {
            if (action.IsFavorite)
                await RemoveFromFavoritesAsync(action.Id);
            else
                await AddToFavoritesAsync(action.Id);
        };
        
        var customizeItem = new MenuItem { Header = "Customize..." };
        customizeItem.Click += (s, e) => ShowActionCustomizationDialog(action);
        
        contextMenu.Items.Add(favoriteItem);
        contextMenu.Items.Add(customizeItem);
        
        control.ContextMenu = contextMenu;
        contextMenu.IsOpen = true;
    }

    #endregion

    #region Helper Methods

    private void UpdateParticleEffects()
    {
        // Add ambient particle effects
        if (_random.NextDouble() < 0.1 && _particles.Count < 50)
        {
            SpawnAmbientParticle();
        }
    }

    private void SpawnAmbientParticle()
    {
        var particle = new Ellipse
        {
            Width = _random.Next(1, 3),
            Height = _random.Next(1, 3),
            Fill = new SolidColorBrush(Color.FromArgb(
                (byte)_random.Next(50, 150), 
                (byte)_random.Next(200, 255),
                (byte)_random.Next(150, 255),
                (byte)_random.Next(100, 200)))
        };

        Canvas.SetLeft(particle, _random.Next(0, 600));
        Canvas.SetTop(particle, _random.Next(0, 400));
        _particleCanvas.Children.Add(particle);
        _particles.Add(particle);

        AnimateAmbientParticle(particle);
    }

    private void AnimateAmbientParticle(UIElement particle)
    {
        var floatAnimation = new DoubleAnimation
        {
            From = Canvas.GetTop(particle),
            To = Canvas.GetTop(particle) - _random.Next(20, 60),
            Duration = TimeSpan.FromMilliseconds(_random.Next(3000, 6000)),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
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

    private void AnimateInitialLoad()
    {
        var staggerDelay = 0;
        
        foreach (var control in _actionControls.Values)
        {
            var fadeAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                BeginTime = TimeSpan.FromMilliseconds(staggerDelay)
            };

            var slideAnimation = new DoubleAnimation
            {
                From = -50,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                BeginTime = TimeSpan.FromMilliseconds(staggerDelay)
            };

            var transform = new TranslateTransform();
            control.RenderTransform = transform;

            control.BeginAnimation(OpacityProperty, fadeAnimation);
            transform.BeginAnimation(TranslateTransform.YProperty, slideAnimation);

            staggerDelay += 50;
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

    private void ShowCustomizationDialog()
    {
        // Placeholder for customization dialog
    }

    private void ShowActionCustomizationDialog(HoloQuickAction action)
    {
        // Placeholder for action-specific customization dialog
    }

    #endregion

    #region Property Change Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloQuickActions control)
        {
            control.ApplyHolographicIntensity();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloQuickActions control)
        {
            control.ApplyEVEColorScheme();
        }
    }

    private static void OnCurrentContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloQuickActions control)
        {
            control.RefreshActionDisplay();
            control.ContextChanged?.Invoke(control, new HoloQuickActionsEventArgs
            {
                Context = control.CurrentContext,
                Timestamp = DateTime.Now
            });
        }
    }

    private static void OnActionLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloQuickActions control)
        {
            if (control.ActionLayout == QuickActionLayout.Grid)
                control.SwitchToGridLayout();
            else
                control.SwitchToRadialLayout();

            control.LayoutChanged?.Invoke(control, new HoloQuickActionsEventArgs
            {
                Layout = control.ActionLayout,
                Timestamp = DateTime.Now
            });
        }
    }

    private static void OnEnableGestureControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Gesture control state changed
    }

    private static void OnEnableActionAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Animation state changed
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloQuickActions control && !(bool)e.NewValue)
        {
            control.CleanupAllParticles();
        }
    }

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode => _isSimplifiedMode;

    public void EnterSimplifiedMode()
    {
        _isSimplifiedMode = true;
        CleanupAllParticles();
        EnableActionAnimations = false;
        EnableParticleEffects = false;
        EnableGestureControl = false;
    }

    public void ExitSimplifiedMode()
    {
        _isSimplifiedMode = false;
        EnableActionAnimations = true;
        EnableParticleEffects = true;
        EnableGestureControl = true;
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
            shadow.BlurRadius = 12 * intensity;
            shadow.ShadowDepth = 4 * intensity;
        }

        EnableActionAnimations = intensity > 0.3;
        EnableParticleEffects = intensity > 0.5;
    }

    #endregion
}

#region Supporting Classes and Enums

public class HoloQuickAction
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string IconPath { get; set; }
    public string ActionType { get; set; }
    public ActionContext Context { get; set; }
    public string Hotkey { get; set; }
    public Color Color { get; set; }
    public ActionPriority Priority { get; set; }
    public bool IsFavorite { get; set; }
    public int UsageCount { get; set; }
    public DateTime LastUsed { get; set; }
}

public class HoloQuickActionCustomization
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Hotkey { get; set; }
    public Color? Color { get; set; }
}

public class HoloActionControl : UserControl
{
    public static readonly DependencyProperty ActionProperty =
        DependencyProperty.Register(nameof(Action), typeof(HoloQuickAction), typeof(HoloActionControl));

    public HoloQuickAction Action
    {
        get => (HoloQuickAction)GetValue(ActionProperty);
        set => SetValue(ActionProperty, value);
    }
}

public enum ActionContext
{
    General,
    ShipFitting,
    Market,
    Character
}

public enum QuickActionLayout
{
    Grid,
    Radial
}

public enum ActionPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum GestureType
{
    Unknown,
    SwipeLeft,
    SwipeRight,
    SwipeUp,
    SwipeDown
}

public class HoloQuickActionEventArgs : EventArgs
{
    public HoloQuickAction Action { get; set; }
    public HoloQuickActionCustomization Customization { get; set; }
    public DateTime Timestamp { get; set; }
}

public class HoloQuickActionsEventArgs : EventArgs
{
    public ActionContext Context { get; set; }
    public QuickActionLayout Layout { get; set; }
    public GestureType GestureType { get; set; }
    public DateTime Timestamp { get; set; }
}

#endregion