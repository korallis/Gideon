// ==========================================================================
// HoloFittingTemplates.cs - Holographic Fitting Templates Interface
// ==========================================================================
// Advanced fitting templates system featuring holographic template cards,
// animated template management, EVE-style loadouts, and cloud synchronization.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
/// Holographic fitting templates interface with template management and sharing capabilities
/// </summary>
public class HoloFittingTemplates : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloFittingTemplates),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloFittingTemplates),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty TemplatesProperty =
        DependencyProperty.Register(nameof(Templates), typeof(ObservableCollection<HoloFittingTemplate>), typeof(HoloFittingTemplates),
            new PropertyMetadata(null, OnTemplatesChanged));

    public static readonly DependencyProperty SelectedTemplateProperty =
        DependencyProperty.Register(nameof(SelectedTemplate), typeof(HoloFittingTemplate), typeof(HoloFittingTemplates),
            new PropertyMetadata(null, OnSelectedTemplateChanged));

    public static readonly DependencyProperty CurrentShipProperty =
        DependencyProperty.Register(nameof(CurrentShip), typeof(HoloShipData), typeof(HoloFittingTemplates),
            new PropertyMetadata(null, OnCurrentShipChanged));

    public static readonly DependencyProperty ViewModeProperty =
        DependencyProperty.Register(nameof(ViewMode), typeof(TemplateViewMode), typeof(HoloFittingTemplates),
            new PropertyMetadata(TemplateViewMode.Gallery, OnViewModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloFittingTemplates),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloFittingTemplates),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowPreviewProperty =
        DependencyProperty.Register(nameof(ShowPreview), typeof(bool), typeof(HoloFittingTemplates),
            new PropertyMetadata(true, OnShowPreviewChanged));

    public static readonly DependencyProperty EnableCloudSyncProperty =
        DependencyProperty.Register(nameof(EnableCloudSync), typeof(bool), typeof(HoloFittingTemplates),
            new PropertyMetadata(false, OnEnableCloudSyncChanged));

    public static readonly DependencyProperty FilterCriteriaProperty =
        DependencyProperty.Register(nameof(FilterCriteria), typeof(TemplateFilterCriteria), typeof(HoloFittingTemplates),
            new PropertyMetadata(null, OnFilterCriteriaChanged));

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

    public ObservableCollection<HoloFittingTemplate> Templates
    {
        get => (ObservableCollection<HoloFittingTemplate>)GetValue(TemplatesProperty);
        set => SetValue(TemplatesProperty, value);
    }

    public HoloFittingTemplate SelectedTemplate
    {
        get => (HoloFittingTemplate)GetValue(SelectedTemplateProperty);
        set => SetValue(SelectedTemplateProperty, value);
    }

    public HoloShipData CurrentShip
    {
        get => (HoloShipData)GetValue(CurrentShipProperty);
        set => SetValue(CurrentShipProperty, value);
    }

    public TemplateViewMode ViewMode
    {
        get => (TemplateViewMode)GetValue(ViewModeProperty);
        set => SetValue(ViewModeProperty, value);
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

    public bool ShowPreview
    {
        get => (bool)GetValue(ShowPreviewProperty);
        set => SetValue(ShowPreviewProperty, value);
    }

    public bool EnableCloudSync
    {
        get => (bool)GetValue(EnableCloudSyncProperty);
        set => SetValue(EnableCloudSyncProperty, value);
    }

    public TemplateFilterCriteria FilterCriteria
    {
        get => (TemplateFilterCriteria)GetValue(FilterCriteriaProperty);
        set => SetValue(FilterCriteriaProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<TemplateSelectedEventArgs> TemplateSelected;
    public event EventHandler<TemplateActionEventArgs> TemplateAction;
    public event EventHandler<TemplateSyncEventArgs> TemplateSyncComplete;

    #endregion

    #region Private Fields

    private Grid _rootGrid;
    private Grid _headerGrid;
    private ScrollViewer _templatesScrollViewer;
    private WrapPanel _templatesPanel;
    private ListBox _templatesList;
    private Border _previewPanel;
    private Canvas _particleCanvas;
    private TextBox _searchBox;
    private StackPanel _filterPanel;
    private StackPanel _actionPanel;
    private DispatcherTimer _animationTimer;
    private DispatcherTimer _syncTimer;
    private readonly List<UIElement> _particles = new();
    private readonly Dictionary<string, FrameworkElement> _templateVisuals = new();
    private CollectionViewSource _templatesViewSource;
    private TemplateManager _templateManager;
    private readonly Random _random = new();

    #endregion

    #region Constructor

    public HoloFittingTemplates()
    {
        InitializeComponent();
        InitializeTemplateManager();
        InitializeAnimationSystem();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 1000;
        Height = 700;
        Background = Brushes.Transparent;

        _rootGrid = new Grid();
        _rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
        _rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content

        CreateHeader();
        CreateContentArea();
        CreateParticleSystem();

        Content = _rootGrid;
    }

    private void CreateHeader()
    {
        _headerGrid = new Grid
        {
            Margin = new Thickness(10),
            Background = CreateHolographicBackground(0.2),
            Effect = CreateGlowEffect()
        };

        _headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Search/Filters
        _headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // View modes
        _headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Actions

        CreateSearchAndFilters();
        CreateViewModeSelector();
        CreateActionButtons();

        Grid.SetRow(_headerGrid, 0);
        _rootGrid.Children.Add(_headerGrid);
    }

    private void CreateSearchAndFilters()
    {
        var searchPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10)
        };

        // Search box
        _searchBox = new TextBox
        {
            Width = 300,
            Height = 35,
            FontSize = 14,
            Padding = new Thickness(15, 8),
            Background = CreateHolographicBackground(0.3),
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            BorderBrush = GetBrushForColorScheme(EVEColorScheme),
            BorderThickness = new Thickness(1),
            Effect = CreateGlowEffect()
        };

        _searchBox.TextChanged += SearchBox_TextChanged;

        // Filter panel
        _filterPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10, 0, 0, 0)
        };

        CreateFilterButton("All", TemplateCategory.All);
        CreateFilterButton("PvP", TemplateCategory.PvP);
        CreateFilterButton("PvE", TemplateCategory.PvE);
        CreateFilterButton("Mining", TemplateCategory.Mining);
        CreateFilterButton("Exploration", TemplateCategory.Exploration);
        CreateFilterButton("Custom", TemplateCategory.Custom);

        searchPanel.Children.Add(_searchBox);
        searchPanel.Children.Add(_filterPanel);

        Grid.SetColumn(searchPanel, 0);
        _headerGrid.Children.Add(searchPanel);
    }

    private void CreateFilterButton(string text, TemplateCategory category)
    {
        var button = new Button
        {
            Content = text,
            Margin = new Thickness(5, 0),
            Padding = new Thickness(12, 6),
            Background = CreateHolographicBackground(0.2),
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            BorderBrush = GetBrushForColorScheme(EVEColorScheme),
            BorderThickness = new Thickness(1),
            Effect = CreateGlowEffect(),
            Style = CreateHolographicButtonStyle(),
            Tag = category
        };

        button.Click += FilterButton_Click;
        _filterPanel.Children.Add(button);
    }

    private void CreateViewModeSelector()
    {
        var viewPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10)
        };

        CreateViewModeButton("Gallery", TemplateViewMode.Gallery, viewPanel);
        CreateViewModeButton("List", TemplateViewMode.List, viewPanel);
        CreateViewModeButton("Grid", TemplateViewMode.Grid, viewPanel);

        Grid.SetColumn(viewPanel, 1);
        _headerGrid.Children.Add(viewPanel);
    }

    private void CreateViewModeButton(string text, TemplateViewMode mode, Panel parent)
    {
        var button = new Button
        {
            Content = text,
            Margin = new Thickness(2),
            Padding = new Thickness(12, 6),
            Background = CreateHolographicBackground(mode == ViewMode ? 0.5 : 0.2),
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            BorderBrush = GetBrushForColorScheme(EVEColorScheme),
            BorderThickness = new Thickness(1),
            Effect = CreateGlowEffect(),
            Style = CreateHolographicButtonStyle(),
            Tag = mode
        };

        button.Click += (s, e) => ViewMode = (TemplateViewMode)button.Tag;
        parent.Children.Add(button);
    }

    private void CreateActionButtons()
    {
        _actionPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10)
        };

        CreateActionButton("New", "Create", CreateNewTemplate);
        CreateActionButton("Save", "Save Current", SaveCurrentFitting);
        CreateActionButton("Import", "Import", ImportTemplates);
        CreateActionButton("Export", "Export", ExportTemplates);
        if (EnableCloudSync)
        {
            CreateActionButton("Sync", "Cloud Sync", SyncWithCloud);
        }

        Grid.SetColumn(_actionPanel, 2);
        _headerGrid.Children.Add(_actionPanel);
    }

    private void CreateActionButton(string text, string tooltip, RoutedEventHandler handler)
    {
        var button = new Button
        {
            Content = text,
            ToolTip = tooltip,
            Margin = new Thickness(3),
            Padding = new Thickness(15, 8),
            Background = CreateHolographicBackground(0.3),
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            BorderBrush = GetBrushForColorScheme(EVEColorScheme),
            BorderThickness = new Thickness(1),
            Effect = CreateGlowEffect(),
            Style = CreateHolographicButtonStyle()
        };

        button.Click += handler;
        _actionPanel.Children.Add(button);
    }

    private void CreateContentArea()
    {
        var contentGrid = new Grid();
        contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }); // Templates
        if (ShowPreview)
        {
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Preview
        }

        CreateTemplatesArea();
        if (ShowPreview)
        {
            CreatePreviewPanel();
        }

        Grid.SetRow(contentGrid, 1);
        _rootGrid.Children.Add(contentGrid);
    }

    private void CreateTemplatesArea()
    {
        _templatesScrollViewer = new ScrollViewer
        {
            Margin = new Thickness(10),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = CreateHolographicBackground(0.1),
            BorderBrush = GetBrushForColorScheme(EVEColorScheme),
            BorderThickness = new Thickness(1),
            Effect = CreateGlowEffect()
        };

        switch (ViewMode)
        {
            case TemplateViewMode.Gallery:
            case TemplateViewMode.Grid:
                _templatesPanel = new WrapPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(10)
                };
                _templatesScrollViewer.Content = _templatesPanel;
                break;

            case TemplateViewMode.List:
                _templatesList = new ListBox
                {
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    ItemContainerStyle = CreateHolographicListItemStyle(),
                    Margin = new Thickness(10)
                };
                _templatesScrollViewer.Content = _templatesList;
                break;
        }

        var parentGrid = (Grid)_rootGrid.Children[1];
        Grid.SetColumn(_templatesScrollViewer, 0);
        parentGrid.Children.Add(_templatesScrollViewer);

        // Initialize collection view
        _templatesViewSource = new CollectionViewSource();
        if (Templates != null)
        {
            _templatesViewSource.Source = Templates;
            if (_templatesList != null)
            {
                _templatesList.ItemsSource = _templatesViewSource.View;
            }
        }
    }

    private void CreatePreviewPanel()
    {
        _previewPanel = new Border
        {
            Margin = new Thickness(0, 10, 10, 10),
            Background = CreateHolographicBackground(0.1),
            BorderBrush = GetBrushForColorScheme(EVEColorScheme),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Effect = CreateGlowEffect()
        };

        var previewContent = new Grid();
        previewContent.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Title
        previewContent.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content

        var previewTitle = new TextBlock
        {
            Text = "Template Preview",
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            Margin = new Thickness(15, 10),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var previewScroll = new ScrollViewer
        {
            Margin = new Thickness(10),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        Grid.SetRow(previewTitle, 0);
        Grid.SetRow(previewScroll, 1);
        previewContent.Children.Add(previewTitle);
        previewContent.Children.Add(previewScroll);

        _previewPanel.Child = previewContent;

        var parentGrid = (Grid)_rootGrid.Children[1];
        Grid.SetColumn(_previewPanel, 1);
        parentGrid.Children.Add(_previewPanel);
    }

    private void CreateParticleSystem()
    {
        _particleCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false
        };

        Grid.SetRowSpan(_particleCanvas, 2);
        _rootGrid.Children.Add(_particleCanvas);
    }

    private void InitializeTemplateManager()
    {
        _templateManager = new TemplateManager();
        
        if (Templates == null)
        {
            Templates = new ObservableCollection<HoloFittingTemplate>();
        }

        Templates.CollectionChanged += Templates_CollectionChanged;
    }

    private void InitializeAnimationSystem()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        _animationTimer.Tick += AnimationTimer_Tick;

        if (EnableCloudSync)
        {
            _syncTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(5) // Sync every 5 minutes
            };
            _syncTimer.Tick += SyncTimer_Tick;
        }
    }

    #endregion

    #region Template Management

    private void Templates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (HoloFittingTemplate template in e.NewItems)
            {
                CreateTemplateVisual(template);
            }
        }

        if (e.OldItems != null)
        {
            foreach (HoloFittingTemplate template in e.OldItems)
            {
                RemoveTemplateVisual(template);
            }
        }

        UpdateTemplateDisplay();
    }

    private void CreateTemplateVisual(HoloFittingTemplate template)
    {
        FrameworkElement visual = ViewMode switch
        {
            TemplateViewMode.Gallery => CreateGalleryCard(template),
            TemplateViewMode.Grid => CreateGridCard(template),
            TemplateViewMode.List => CreateListItem(template),
            _ => CreateGalleryCard(template)
        };

        _templateVisuals[template.Id] = visual;

        if (_templatesPanel != null)
        {
            _templatesPanel.Children.Add(visual);
        }

        if (EnableAnimations)
        {
            AnimateTemplateAppearance(visual);
        }
    }

    private FrameworkElement CreateGalleryCard(HoloFittingTemplate template)
    {
        var card = new Border
        {
            Width = 280,
            Height = 200,
            Margin = new Thickness(10),
            Background = CreateHolographicBackground(0.2),
            BorderBrush = GetBrushForColorScheme(EVEColorScheme),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Effect = CreateGlowEffect(),
            Tag = template,
            Cursor = Cursors.Hand
        };

        var cardGrid = new Grid();
        cardGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Preview
        cardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Info

        // Template preview (simplified ship visualization)
        var previewBorder = new Border
        {
            Background = CreateHolographicBackground(0.1),
            Margin = new Thickness(10, 10, 10, 5)
        };

        var previewCanvas = CreateTemplatePreview(template);
        previewBorder.Child = previewCanvas;

        // Template info
        var infoPanel = new StackPanel
        {
            Margin = new Thickness(10, 5, 10, 10)
        };

        var titleBlock = new TextBlock
        {
            Text = template.Name,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            TextTrimming = TextTrimming.CharacterEllipsis
        };

        var shipBlock = new TextBlock
        {
            Text = template.ShipName,
            FontSize = 12,
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            Opacity = 0.8
        };

        var categoryBlock = new TextBlock
        {
            Text = $"{template.Category} • {template.Tags.FirstOrDefault() ?? "General"}",
            FontSize = 10,
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            Opacity = 0.6
        };

        infoPanel.Children.Add(titleBlock);
        infoPanel.Children.Add(shipBlock);
        infoPanel.Children.Add(categoryBlock);

        Grid.SetRow(previewBorder, 0);
        Grid.SetRow(infoPanel, 1);
        cardGrid.Children.Add(previewBorder);
        cardGrid.Children.Add(infoPanel);

        card.Child = cardGrid;

        // Event handlers
        card.MouseEnter += (s, e) => OnTemplateMouseEnter(template, card);
        card.MouseLeave += (s, e) => OnTemplateMouseLeave(template, card);
        card.MouseLeftButtonDown += (s, e) => OnTemplateClick(template);
        card.ContextMenuOpening += (s, e) => ShowTemplateContextMenu(template, card);

        return card;
    }

    private FrameworkElement CreateGridCard(HoloFittingTemplate template)
    {
        var card = new Border
        {
            Width = 160,
            Height = 120,
            Margin = new Thickness(5),
            Background = CreateHolographicBackground(0.2),
            BorderBrush = GetBrushForColorScheme(EVEColorScheme),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Effect = CreateGlowEffect(),
            Tag = template,
            Cursor = Cursors.Hand
        };

        var cardContent = new StackPanel
        {
            Margin = new Thickness(8)
        };

        var titleBlock = new TextBlock
        {
            Text = template.Name,
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            TextTrimming = TextTrimming.CharacterEllipsis,
            TextAlignment = TextAlignment.Center
        };

        var shipBlock = new TextBlock
        {
            Text = template.ShipName,
            FontSize = 10,
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            Opacity = 0.8,
            TextAlignment = TextAlignment.Center
        };

        var categoryBlock = new TextBlock
        {
            Text = template.Category.ToString(),
            FontSize = 9,
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            Opacity = 0.6,
            TextAlignment = TextAlignment.Center
        };

        cardContent.Children.Add(titleBlock);
        cardContent.Children.Add(shipBlock);
        cardContent.Children.Add(categoryBlock);

        card.Child = cardContent;

        // Event handlers
        card.MouseEnter += (s, e) => OnTemplateMouseEnter(template, card);
        card.MouseLeave += (s, e) => OnTemplateMouseLeave(template, card);
        card.MouseLeftButtonDown += (s, e) => OnTemplateClick(template);

        return card;
    }

    private FrameworkElement CreateListItem(HoloFittingTemplate template)
    {
        var item = new Grid
        {
            Height = 60,
            Margin = new Thickness(5),
            Background = CreateHolographicBackground(0.1),
            Tag = template
        };

        item.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Icon
        item.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Info
        item.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Actions

        // Template icon
        var iconBorder = new Border
        {
            Width = 40,
            Height = 40,
            Margin = new Thickness(10),
            Background = CreateHolographicBackground(0.3),
            CornerRadius = new CornerRadius(20)
        };

        // Template info
        var infoPanel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0)
        };

        var nameBlock = new TextBlock
        {
            Text = template.Name,
            FontSize = 14,
            FontWeight = FontWeights.Medium,
            Foreground = GetBrushForColorScheme(EVEColorScheme)
        };

        var detailsBlock = new TextBlock
        {
            Text = $"{template.ShipName} • {template.Category} • {template.ModuleCount} modules",
            FontSize = 11,
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            Opacity = 0.7
        };

        infoPanel.Children.Add(nameBlock);
        infoPanel.Children.Add(detailsBlock);

        // Action buttons
        var actionPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0)
        };

        var loadButton = new Button
        {
            Content = "Load",
            Margin = new Thickness(3),
            Padding = new Thickness(8, 4),
            Background = CreateHolographicBackground(0.3),
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            BorderBrush = GetBrushForColorScheme(EVEColorScheme),
            BorderThickness = new Thickness(1),
            Tag = template
        };

        loadButton.Click += (s, e) => LoadTemplate(template);
        actionPanel.Children.Add(loadButton);

        Grid.SetColumn(iconBorder, 0);
        Grid.SetColumn(infoPanel, 1);
        Grid.SetColumn(actionPanel, 2);

        item.Children.Add(iconBorder);
        item.Children.Add(infoPanel);
        item.Children.Add(actionPanel);

        return item;
    }

    private Canvas CreateTemplatePreview(HoloFittingTemplate template)
    {
        var canvas = new Canvas
        {
            Width = 240,
            Height = 120
        };

        // Simplified ship silhouette
        var shipPath = new Path
        {
            Data = GetShipGeometry(template.ShipName),
            Fill = GetBrushForColorScheme(EVEColorScheme),
            Opacity = 0.6,
            Width = 80,
            Height = 40
        };

        Canvas.SetLeft(shipPath, 80);
        Canvas.SetTop(shipPath, 40);
        canvas.Children.Add(shipPath);

        // Module indicators
        var modulePositions = GetModulePositions(template);
        foreach (var position in modulePositions)
        {
            var moduleIndicator = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = GetBrushForColorScheme(EVEColorScheme),
                Opacity = 0.8
            };

            Canvas.SetLeft(moduleIndicator, position.X);
            Canvas.SetTop(moduleIndicator, position.Y);
            canvas.Children.Add(moduleIndicator);
        }

        return canvas;
    }

    private void RemoveTemplateVisual(HoloFittingTemplate template)
    {
        if (_templateVisuals.TryGetValue(template.Id, out var visual))
        {
            if (EnableAnimations)
            {
                AnimateTemplateDisappearance(visual, () =>
                {
                    _templatesPanel?.Children.Remove(visual);
                    _templateVisuals.Remove(template.Id);
                });
            }
            else
            {
                _templatesPanel?.Children.Remove(visual);
                _templateVisuals.Remove(template.Id);
            }
        }
    }

    private void UpdateTemplateDisplay()
    {
        if (_templatesViewSource?.View != null)
        {
            _templatesViewSource.View.Refresh();
        }
    }

    #endregion

    #region Event Handlers

    private void OnTemplateMouseEnter(HoloFittingTemplate template, FrameworkElement visual)
    {
        if (EnableAnimations)
        {
            AnimateTemplateHover(visual, true);
        }

        if (EnableParticleEffects)
        {
            CreateHoverParticles(visual);
        }

        // Update preview if enabled
        if (ShowPreview)
        {
            UpdatePreview(template);
        }
    }

    private void OnTemplateMouseLeave(HoloFittingTemplate template, FrameworkElement visual)
    {
        if (EnableAnimations)
        {
            AnimateTemplateHover(visual, false);
        }
    }

    private void OnTemplateClick(HoloFittingTemplate template)
    {
        SelectedTemplate = template;
        TemplateSelected?.Invoke(this, new TemplateSelectedEventArgs(template));

        if (EnableParticleEffects)
        {
            CreateSelectionParticles();
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void FilterButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is TemplateCategory category)
        {
            if (FilterCriteria == null)
                FilterCriteria = new TemplateFilterCriteria();

            FilterCriteria.Category = category;
            UpdateFilterButtons();
            ApplyFilters();
        }
    }

    private void CreateNewTemplate(object sender, RoutedEventArgs e)
    {
        var template = new HoloFittingTemplate
        {
            Id = Guid.NewGuid().ToString(),
            Name = "New Template",
            ShipName = CurrentShip?.Name ?? "Unknown Ship",
            Category = TemplateCategory.Custom,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        Templates.Add(template);
        SelectedTemplate = template;

        TemplateAction?.Invoke(this, new TemplateActionEventArgs(TemplateActionType.Create, template));
    }

    private void SaveCurrentFitting(object sender, RoutedEventArgs e)
    {
        if (CurrentShip == null) return;

        var template = _templateManager.CreateFromCurrentFitting(CurrentShip);
        Templates.Add(template);

        TemplateAction?.Invoke(this, new TemplateActionEventArgs(TemplateActionType.Save, template));

        if (EnableParticleEffects)
        {
            CreateSaveParticles();
        }
    }

    private void ImportTemplates(object sender, RoutedEventArgs e)
    {
        Task.Run(async () =>
        {
            var imported = await _templateManager.ImportTemplatesAsync();
            
            Dispatcher.BeginInvoke(() =>
            {
                foreach (var template in imported)
                {
                    Templates.Add(template);
                }

                TemplateAction?.Invoke(this, new TemplateActionEventArgs(TemplateActionType.Import, null));
            });
        });
    }

    private void ExportTemplates(object sender, RoutedEventArgs e)
    {
        Task.Run(async () =>
        {
            var selectedTemplates = Templates.Where(t => t.IsSelected).ToList();
            if (!selectedTemplates.Any())
                selectedTemplates = Templates.ToList();

            await _templateManager.ExportTemplatesAsync(selectedTemplates);

            Dispatcher.BeginInvoke(() =>
            {
                TemplateAction?.Invoke(this, new TemplateActionEventArgs(TemplateActionType.Export, null));
            });
        });
    }

    private void SyncWithCloud(object sender, RoutedEventArgs e)
    {
        Task.Run(async () =>
        {
            var result = await _templateManager.SyncWithCloudAsync(Templates);

            Dispatcher.BeginInvoke(() =>
            {
                TemplateSyncComplete?.Invoke(this, new TemplateSyncEventArgs(result));

                if (EnableParticleEffects)
                {
                    CreateSyncParticles();
                }
            });
        });
    }

    private async void SyncTimer_Tick(object sender, EventArgs e)
    {
        if (EnableCloudSync)
        {
            await _templateManager.SyncWithCloudAsync(Templates);
        }
    }

    #endregion

    #region Filtering and Search

    private void ApplyFilters()
    {
        if (_templatesViewSource?.View == null) return;

        var searchText = _searchBox?.Text?.ToLowerInvariant() ?? string.Empty;

        _templatesViewSource.View.Filter = item =>
        {
            if (item is HoloFittingTemplate template)
            {
                // Text search
                var matchesSearch = string.IsNullOrEmpty(searchText) ||
                                  template.Name.ToLowerInvariant().Contains(searchText) ||
                                  template.ShipName.ToLowerInvariant().Contains(searchText) ||
                                  template.Description.ToLowerInvariant().Contains(searchText) ||
                                  template.Tags.Any(tag => tag.ToLowerInvariant().Contains(searchText));

                // Category filter
                var matchesCategory = FilterCriteria?.Category == TemplateCategory.All ||
                                    FilterCriteria?.Category == template.Category ||
                                    FilterCriteria?.Category == null;

                return matchesSearch && matchesCategory;
            }
            return false;
        };

        UpdateTemplateDisplay();
    }

    private void UpdateFilterButtons()
    {
        foreach (Button button in _filterPanel.Children.OfType<Button>())
        {
            var isActive = FilterCriteria?.Category.Equals(button.Tag) == true;
            button.Background = CreateHolographicBackground(isActive ? 0.5 : 0.2);
            button.Effect = isActive ? CreateIntenseGlowEffect() : CreateGlowEffect();
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
        foreach (var particle in _particles.ToList())
        {
            if (particle.Opacity <= 0.1)
            {
                _particles.Remove(particle);
                _particleCanvas.Children.Remove(particle);
            }
        }
    }

    private void AnimateTemplateAppearance(FrameworkElement visual)
    {
        visual.Opacity = 0;
        visual.RenderTransform = new ScaleTransform(0.8, 0.8);

        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400));
        var scaleX = new DoubleAnimation(0.8, 1, TimeSpan.FromMilliseconds(400))
        {
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
        };
        var scaleY = new DoubleAnimation(0.8, 1, TimeSpan.FromMilliseconds(400))
        {
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
        };

        visual.BeginAnimation(OpacityProperty, fadeIn);
        ((ScaleTransform)visual.RenderTransform).BeginAnimation(ScaleTransform.ScaleXProperty, scaleX);
        ((ScaleTransform)visual.RenderTransform).BeginAnimation(ScaleTransform.ScaleYProperty, scaleY);
    }

    private void AnimateTemplateDisappearance(FrameworkElement visual, Action onComplete)
    {
        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
        var scaleOut = new DoubleAnimation(1, 0.8, TimeSpan.FromMilliseconds(300));

        fadeOut.Completed += (s, e) => onComplete?.Invoke();

        visual.BeginAnimation(OpacityProperty, fadeOut);
        if (visual.RenderTransform is ScaleTransform scale)
        {
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleOut);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleOut);
        }
    }

    private void AnimateTemplateHover(FrameworkElement visual, bool isHovering)
    {
        var targetScale = isHovering ? 1.05 : 1.0;
        var duration = TimeSpan.FromMilliseconds(200);

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

    #endregion

    #region Particle Effects

    private void CreateHoverParticles(FrameworkElement visual)
    {
        if (!EnableParticleEffects) return;

        for (int i = 0; i < 3; i++)
        {
            CreateTemplateParticle(visual);
        }
    }

    private void CreateSelectionParticles()
    {
        if (!EnableParticleEffects) return;

        for (int i = 0; i < 10; i++)
        {
            CreateSelectionParticle();
        }
    }

    private void CreateSaveParticles()
    {
        if (!EnableParticleEffects) return;

        for (int i = 0; i < 15; i++)
        {
            CreateDataParticle("save");
        }
    }

    private void CreateSyncParticles()
    {
        if (!EnableParticleEffects) return;

        for (int i = 0; i < 20; i++)
        {
            CreateDataParticle("sync");
        }
    }

    private void CreateTemplateParticle(FrameworkElement visual)
    {
        var particle = new Ellipse
        {
            Width = 3,
            Height = 3,
            Fill = GetBrushForColorScheme(EVEColorScheme),
            Opacity = 0.7
        };

        var visualPosition = visual.TransformToAncestor(_particleCanvas).Transform(new Point(0, 0));
        var angle = _random.NextDouble() * 2 * Math.PI;
        var radius = 30;

        var x = visualPosition.X + visual.ActualWidth / 2 + radius * Math.Cos(angle);
        var y = visualPosition.Y + visual.ActualHeight / 2 + radius * Math.Sin(angle);

        Canvas.SetLeft(particle, x);
        Canvas.SetTop(particle, y);
        Canvas.SetZIndex(particle, 1000);

        _particles.Add(particle);
        _particleCanvas.Children.Add(particle);

        // Animate particle orbit
        var orbitAnimation = new DoubleAnimation
        {
            From = angle,
            To = angle + 2 * Math.PI,
            Duration = TimeSpan.FromMilliseconds(3000),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        var fadeOut = new DoubleAnimation
        {
            From = 0.7,
            To = 0,
            BeginTime = TimeSpan.FromMilliseconds(2000),
            Duration = TimeSpan.FromMilliseconds(1000)
        };

        particle.BeginAnimation(OpacityProperty, fadeOut);

        Task.Delay(3000).ContinueWith(t =>
        {
            Dispatcher.BeginInvoke(() =>
            {
                _particles.Remove(particle);
                _particleCanvas.Children.Remove(particle);
            });
        });
    }

    private void CreateSelectionParticle()
    {
        var particle = new Ellipse
        {
            Width = _random.NextDouble() * 6 + 3,
            Height = _random.NextDouble() * 6 + 3,
            Fill = GetBrushForColorScheme(EVEColorScheme),
            Opacity = 0.8,
            Effect = CreateGlowEffect(0.5)
        };

        var startX = _random.NextDouble() * ActualWidth;
        var startY = _random.NextDouble() * ActualHeight;

        Canvas.SetLeft(particle, startX);
        Canvas.SetTop(particle, startY);
        Canvas.SetZIndex(particle, 999);

        _particles.Add(particle);
        _particleCanvas.Children.Add(particle);

        // Animate toward center
        var centerX = ActualWidth / 2;
        var centerY = ActualHeight / 2;

        var moveX = new DoubleAnimation
        {
            From = startX,
            To = centerX,
            Duration = TimeSpan.FromMilliseconds(1000),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };

        var moveY = new DoubleAnimation
        {
            From = startY,
            To = centerY,
            Duration = TimeSpan.FromMilliseconds(1000),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };

        var fadeOut = new DoubleAnimation
        {
            From = 0.8,
            To = 0,
            BeginTime = TimeSpan.FromMilliseconds(600),
            Duration = TimeSpan.FromMilliseconds(400)
        };

        particle.BeginAnimation(Canvas.LeftProperty, moveX);
        particle.BeginAnimation(Canvas.TopProperty, moveY);
        particle.BeginAnimation(OpacityProperty, fadeOut);

        Task.Delay(1000).ContinueWith(t =>
        {
            Dispatcher.BeginInvoke(() =>
            {
                _particles.Remove(particle);
                _particleCanvas.Children.Remove(particle);
            });
        });
    }

    private void CreateDataParticle(string type)
    {
        var particle = new Rectangle
        {
            Width = 2,
            Height = _random.NextDouble() * 20 + 10,
            Fill = GetBrushForColorScheme(EVEColorScheme),
            Opacity = 0.6
        };

        var startX = _random.NextDouble() * ActualWidth;
        var startY = ActualHeight;
        var endY = -20;

        Canvas.SetLeft(particle, startX);
        Canvas.SetTop(particle, startY);
        Canvas.SetZIndex(particle, 998);

        _particles.Add(particle);
        _particleCanvas.Children.Add(particle);

        // Animate upward movement
        var moveAnimation = new DoubleAnimation
        {
            From = startY,
            To = endY,
            Duration = TimeSpan.FromMilliseconds(2000 + _random.Next(1000)),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        var fadeOut = new DoubleAnimation
        {
            From = 0.6,
            To = 0,
            BeginTime = TimeSpan.FromMilliseconds(1500),
            Duration = TimeSpan.FromMilliseconds(500)
        };

        particle.BeginAnimation(Canvas.TopProperty, moveAnimation);
        particle.BeginAnimation(OpacityProperty, fadeOut);

        Task.Delay(3000).ContinueWith(t =>
        {
            Dispatcher.BeginInvoke(() =>
            {
                _particles.Remove(particle);
                _particleCanvas.Children.Remove(particle);
            });
        });
    }

    #endregion

    #region Template Operations

    private void LoadTemplate(HoloFittingTemplate template)
    {
        SelectedTemplate = template;
        TemplateAction?.Invoke(this, new TemplateActionEventArgs(TemplateActionType.Load, template));

        if (EnableAnimations)
        {
            AnimateTemplateLoad(template);
        }
    }

    private void AnimateTemplateLoad(HoloFittingTemplate template)
    {
        if (_templateVisuals.TryGetValue(template.Id, out var visual))
        {
            // Flash effect
            var flashAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.3,
                Duration = TimeSpan.FromMilliseconds(150),
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(3)
            };

            visual.BeginAnimation(OpacityProperty, flashAnimation);
        }
    }

    private void UpdatePreview(HoloFittingTemplate template)
    {
        if (!ShowPreview || _previewPanel == null) return;

        var previewContent = CreateTemplatePreviewContent(template);
        var grid = (Grid)_previewPanel.Child;
        var scrollViewer = (ScrollViewer)grid.Children[1];
        scrollViewer.Content = previewContent;
    }

    private FrameworkElement CreateTemplatePreviewContent(HoloFittingTemplate template)
    {
        var panel = new StackPanel
        {
            Margin = new Thickness(10)
        };

        // Template details
        var nameBlock = new TextBlock
        {
            Text = template.Name,
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            Margin = new Thickness(0, 0, 0, 10)
        };

        var shipBlock = new TextBlock
        {
            Text = $"Ship: {template.ShipName}",
            FontSize = 12,
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            Margin = new Thickness(0, 0, 0, 5)
        };

        var categoryBlock = new TextBlock
        {
            Text = $"Category: {template.Category}",
            FontSize = 12,
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            Margin = new Thickness(0, 0, 0, 5)
        };

        var moduleCountBlock = new TextBlock
        {
            Text = $"Modules: {template.ModuleCount}",
            FontSize = 12,
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            Margin = new Thickness(0, 0, 0, 10)
        };

        var descriptionBlock = new TextBlock
        {
            Text = template.Description,
            FontSize = 11,
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.8,
            Margin = new Thickness(0, 0, 0, 10)
        };

        // Tags
        if (template.Tags.Any())
        {
            var tagsPanel = new WrapPanel
            {
                Margin = new Thickness(0, 0, 0, 10)
            };

            foreach (var tag in template.Tags)
            {
                var tagBorder = new Border
                {
                    Background = CreateHolographicBackground(0.3),
                    BorderBrush = GetBrushForColorScheme(EVEColorScheme),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(8, 3),
                    Margin = new Thickness(3)
                };

                var tagText = new TextBlock
                {
                    Text = tag,
                    FontSize = 9,
                    Foreground = GetBrushForColorScheme(EVEColorScheme)
                };

                tagBorder.Child = tagText;
                tagsPanel.Children.Add(tagBorder);
            }

            panel.Children.Add(new TextBlock
            {
                Text = "Tags:",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = GetBrushForColorScheme(EVEColorScheme),
                Margin = new Thickness(0, 0, 0, 5)
            });
            panel.Children.Add(tagsPanel);
        }

        panel.Children.Add(nameBlock);
        panel.Children.Add(shipBlock);
        panel.Children.Add(categoryBlock);
        panel.Children.Add(moduleCountBlock);
        panel.Children.Add(descriptionBlock);

        return panel;
    }

    private void ShowTemplateContextMenu(HoloFittingTemplate template, FrameworkElement visual)
    {
        var contextMenu = new ContextMenu();
        
        var loadItem = new MenuItem { Header = "Load Template" };
        loadItem.Click += (s, e) => LoadTemplate(template);
        
        var editItem = new MenuItem { Header = "Edit Template" };
        editItem.Click += (s, e) => EditTemplate(template);
        
        var duplicateItem = new MenuItem { Header = "Duplicate Template" };
        duplicateItem.Click += (s, e) => DuplicateTemplate(template);
        
        var deleteItem = new MenuItem { Header = "Delete Template" };
        deleteItem.Click += (s, e) => DeleteTemplate(template);

        contextMenu.Items.Add(loadItem);
        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(editItem);
        contextMenu.Items.Add(duplicateItem);
        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(deleteItem);

        visual.ContextMenu = contextMenu;
    }

    private void EditTemplate(HoloFittingTemplate template)
    {
        TemplateAction?.Invoke(this, new TemplateActionEventArgs(TemplateActionType.Edit, template));
    }

    private void DuplicateTemplate(HoloFittingTemplate template)
    {
        var duplicate = template.Clone();
        duplicate.Id = Guid.NewGuid().ToString();
        duplicate.Name = $"{template.Name} (Copy)";
        duplicate.CreatedAt = DateTime.UtcNow;
        duplicate.ModifiedAt = DateTime.UtcNow;

        Templates.Add(duplicate);
        TemplateAction?.Invoke(this, new TemplateActionEventArgs(TemplateActionType.Duplicate, duplicate));
    }

    private void DeleteTemplate(HoloFittingTemplate template)
    {
        Templates.Remove(template);
        TemplateAction?.Invoke(this, new TemplateActionEventArgs(TemplateActionType.Delete, template));
    }

    #endregion

    #region Helper Methods

    private Geometry GetShipGeometry(string shipName)
    {
        // Simplified ship geometry - in real implementation, this would load actual ship silhouettes
        return Geometry.Parse("M10,20 L30,15 L40,20 L30,25 L10,20 Z");
    }

    private List<Point> GetModulePositions(HoloFittingTemplate template)
    {
        // Generate module positions based on template data
        var positions = new List<Point>();
        var random = new Random(template.Id.GetHashCode());

        for (int i = 0; i < Math.Min(template.ModuleCount, 20); i++)
        {
            positions.Add(new Point(
                random.Next(20, 200),
                random.Next(20, 100)
            ));
        }

        return positions;
    }

    #endregion

    #region Style Helpers

    private Brush CreateHolographicBackground(double opacity)
    {
        var brush = new LinearGradientBrush();
        brush.StartPoint = new Point(0, 0);
        brush.EndPoint = new Point(1, 1);

        var color = GetColorForScheme(EVEColorScheme);
        brush.GradientStops.Add(new GradientStop(Color.FromArgb((byte)(opacity * 40), color.R, color.G, color.B), 0));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb((byte)(opacity * 20), color.R, color.G, color.B), 0.5));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb((byte)(opacity * 40), color.R, color.G, color.B), 1));

        return brush;
    }

    private Brush GetBrushForColorScheme(EVEColorScheme scheme)
    {
        return scheme switch
        {
            EVEColorScheme.ElectricBlue => new SolidColorBrush(Color.FromRgb(0, 191, 255)),
            EVEColorScheme.AmberGold => new SolidColorBrush(Color.FromRgb(255, 191, 0)),
            EVEColorScheme.CrimsonRed => new SolidColorBrush(Color.FromRgb(220, 20, 60)),
            EVEColorScheme.EmeraldGreen => new SolidColorBrush(Color.FromRgb(0, 255, 127)),
            _ => new SolidColorBrush(Color.FromRgb(0, 191, 255))
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

    private Effect CreateGlowEffect(double intensity = 1.0)
    {
        return new DropShadowEffect
        {
            Color = GetColorForScheme(EVEColorScheme),
            BlurRadius = 8 * intensity,
            Opacity = 0.6 * intensity,
            ShadowDepth = 0
        };
    }

    private Effect CreateIntenseGlowEffect()
    {
        return new DropShadowEffect
        {
            Color = GetColorForScheme(EVEColorScheme),
            BlurRadius = 15,
            Opacity = 0.9,
            ShadowDepth = 0
        };
    }

    private Style CreateHolographicButtonStyle()
    {
        var style = new Style(typeof(Button));
        style.Setters.Add(new Setter(ForegroundProperty, GetBrushForColorScheme(EVEColorScheme)));
        style.Setters.Add(new Setter(FontFamilyProperty, new FontFamily("Segoe UI")));
        style.Setters.Add(new Setter(FontWeightProperty, FontWeights.Medium));
        style.Setters.Add(new Setter(CursorProperty, Cursors.Hand));
        return style;
    }

    private Style CreateHolographicListItemStyle()
    {
        var style = new Style(typeof(ListBoxItem));
        style.Setters.Add(new Setter(BackgroundProperty, CreateHolographicBackground(0.1)));
        style.Setters.Add(new Setter(ForegroundProperty, GetBrushForColorScheme(EVEColorScheme)));
        style.Setters.Add(new Setter(BorderBrushProperty, GetBrushForColorScheme(EVEColorScheme)));
        style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(MarginProperty, new Thickness(2)));
        style.Setters.Add(new Setter(PaddingProperty, new Thickness(10)));
        return style;
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableAnimations)
        {
            _animationTimer.Start();
        }

        if (EnableCloudSync)
        {
            _syncTimer?.Start();
        }

        // Load initial templates
        Task.Run(async () =>
        {
            var templates = await _templateManager.LoadTemplatesAsync();
            
            Dispatcher.BeginInvoke(() =>
            {
                foreach (var template in templates)
                {
                    Templates.Add(template);
                }
            });
        });
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        _syncTimer?.Stop();
    }

    #endregion

    #region Dependency Property Callbacks

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Update holographic effects intensity
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Update color scheme
    }

    private static void OnTemplatesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloFittingTemplates control)
        {
            if (e.OldValue is ObservableCollection<HoloFittingTemplate> oldTemplates)
            {
                oldTemplates.CollectionChanged -= control.Templates_CollectionChanged;
            }

            if (e.NewValue is ObservableCollection<HoloFittingTemplate> newTemplates)
            {
                newTemplates.CollectionChanged += control.Templates_CollectionChanged;
                control._templatesViewSource.Source = newTemplates;
                control.UpdateTemplateDisplay();
            }
        }
    }

    private static void OnSelectedTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloFittingTemplates control && e.NewValue is HoloFittingTemplate template)
        {
            control.UpdatePreview(template);
        }
    }

    private static void OnCurrentShipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Current ship changed - update relevant UI
    }

    private static void OnViewModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloFittingTemplates control)
        {
            control.CreateContentArea();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloFittingTemplates control)
        {
            if ((bool)e.NewValue)
                control._animationTimer.Start();
            else
                control._animationTimer.Stop();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloFittingTemplates control && !(bool)e.NewValue)
        {
            foreach (var particle in control._particles.ToList())
            {
                control._particles.Remove(particle);
                control._particleCanvas.Children.Remove(particle);
            }
        }
    }

    private static void OnShowPreviewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloFittingTemplates control)
        {
            control.CreateContentArea();
        }
    }

    private static void OnEnableCloudSyncChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloFittingTemplates control)
        {
            if ((bool)e.NewValue)
            {
                control._syncTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMinutes(5)
                };
                control._syncTimer.Tick += control.SyncTimer_Tick;
                control._syncTimer.Start();
            }
            else
            {
                control._syncTimer?.Stop();
                control._syncTimer = null;
            }
        }
    }

    private static void OnFilterCriteriaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloFittingTemplates control)
        {
            control.ApplyFilters();
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
            foreach (var particle in _particles.ToList())
            {
                _particles.Remove(particle);
                _particleCanvas.Children.Remove(particle);
            }
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
    /// Loads templates from specified source
    /// </summary>
    public async Task LoadTemplatesAsync(string source = "local")
    {
        var templates = await _templateManager.LoadTemplatesAsync(source);
        
        Templates.Clear();
        foreach (var template in templates)
        {
            Templates.Add(template);
        }
    }

    /// <summary>
    /// Saves all templates to specified destination
    /// </summary>
    public async Task SaveTemplatesAsync(string destination = "local")
    {
        await _templateManager.SaveTemplatesAsync(Templates, destination);
    }

    /// <summary>
    /// Gets templates matching specified criteria
    /// </summary>
    public IEnumerable<HoloFittingTemplate> GetTemplates(TemplateFilterCriteria criteria)
    {
        return Templates.Where(t => criteria.Matches(t));
    }

    /// <summary>
    /// Creates a new template from current fitting
    /// </summary>
    public HoloFittingTemplate CreateTemplateFromCurrent(string name)
    {
        return _templateManager.CreateFromCurrentFitting(CurrentShip, name);
    }

    #endregion
}

#region Supporting Types

/// <summary>
/// Template view modes
/// </summary>
public enum TemplateViewMode
{
    Gallery,
    List,
    Grid
}

/// <summary>
/// Template categories
/// </summary>
public enum TemplateCategory
{
    All,
    PvP,
    PvE,
    Mining,
    Exploration,
    Hauling,
    Trading,
    Industry,
    Custom
}

/// <summary>
/// Template action types
/// </summary>
public enum TemplateActionType
{
    Create,
    Load,
    Save,
    Edit,
    Delete,
    Duplicate,
    Import,
    Export,
    Share
}

/// <summary>
/// Holographic fitting template
/// </summary>
public class HoloFittingTemplate : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private bool _isSelected;

    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }
    }
    
    public string ShipName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TemplateCategory Category { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<HoloModuleData> Modules { get; set; } = new();
    public List<HoloModuleData> Rigs { get; set; } = new();
    public int ModuleCount => Modules.Count + Rigs.Count;
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string Author { get; set; } = string.Empty;
    public int Rating { get; set; }
    public bool IsPublic { get; set; }
    
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public HoloFittingTemplate Clone()
    {
        return new HoloFittingTemplate
        {
            Name = Name,
            ShipName = ShipName,
            Description = Description,
            Category = Category,
            Tags = new List<string>(Tags),
            Modules = new List<HoloModuleData>(Modules),
            Rigs = new List<HoloModuleData>(Rigs),
            Author = Author,
            Rating = Rating,
            IsPublic = IsPublic
        };
    }
}

/// <summary>
/// Template filter criteria
/// </summary>
public class TemplateFilterCriteria
{
    public TemplateCategory Category { get; set; } = TemplateCategory.All;
    public string ShipName { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public string Author { get; set; } = string.Empty;
    public int MinRating { get; set; }
    public bool PublicOnly { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }

    public bool Matches(HoloFittingTemplate template)
    {
        if (Category != TemplateCategory.All && template.Category != Category)
            return false;

        if (!string.IsNullOrEmpty(ShipName) && !template.ShipName.Contains(ShipName, StringComparison.OrdinalIgnoreCase))
            return false;

        if (Tags.Any() && !Tags.Any(tag => template.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)))
            return false;

        if (!string.IsNullOrEmpty(Author) && !template.Author.Contains(Author, StringComparison.OrdinalIgnoreCase))
            return false;

        if (template.Rating < MinRating)
            return false;

        if (PublicOnly && !template.IsPublic)
            return false;

        if (CreatedAfter.HasValue && template.CreatedAt < CreatedAfter.Value)
            return false;

        if (CreatedBefore.HasValue && template.CreatedAt > CreatedBefore.Value)
            return false;

        return true;
    }
}

/// <summary>
/// Template manager for handling template operations
/// </summary>
public class TemplateManager
{
    public async Task<List<HoloFittingTemplate>> LoadTemplatesAsync(string source = "local")
    {
        // Simulate loading templates
        await Task.Delay(500);

        return new List<HoloFittingTemplate>
        {
            new HoloFittingTemplate
            {
                Name = "Combat Drake",
                ShipName = "Drake",
                Category = TemplateCategory.PvE,
                Description = "High DPS missile boat for Level 4 missions",
                Tags = new List<string> { "PvE", "Missiles", "Tank" },
                Author = "PlayerOne",
                Rating = 4,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new HoloFittingTemplate
            {
                Name = "Mining Hulk",
                ShipName = "Hulk",
                Category = TemplateCategory.Mining,
                Description = "Maximum yield mining setup",
                Tags = new List<string> { "Mining", "Yield", "Strip" },
                Author = "MinerBob",
                Rating = 5,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            }
        };
    }

    public async Task SaveTemplatesAsync(IEnumerable<HoloFittingTemplate> templates, string destination = "local")
    {
        await Task.Delay(200);
        // Save templates implementation
    }

    public async Task<List<HoloFittingTemplate>> ImportTemplatesAsync()
    {
        await Task.Delay(1000);
        return new List<HoloFittingTemplate>();
    }

    public async Task ExportTemplatesAsync(IEnumerable<HoloFittingTemplate> templates)
    {
        await Task.Delay(1000);
        // Export templates implementation
    }

    public async Task<TemplateSyncResult> SyncWithCloudAsync(ObservableCollection<HoloFittingTemplate> templates)
    {
        await Task.Delay(2000);
        return new TemplateSyncResult
        {
            Success = true,
            TemplatesUpdated = 3,
            TemplatesAdded = 1,
            TemplatesRemoved = 0
        };
    }

    public HoloFittingTemplate CreateFromCurrentFitting(HoloShipData ship, string name = "New Template")
    {
        return new HoloFittingTemplate
        {
            Name = name,
            ShipName = ship?.Name ?? "Unknown",
            Category = TemplateCategory.Custom,
            Description = "Template created from current fitting",
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Template sync result
/// </summary>
public class TemplateSyncResult
{
    public bool Success { get; set; }
    public int TemplatesUpdated { get; set; }
    public int TemplatesAdded { get; set; }
    public int TemplatesRemoved { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Event args for template selection
/// </summary>
public class TemplateSelectedEventArgs : EventArgs
{
    public HoloFittingTemplate Template { get; }

    public TemplateSelectedEventArgs(HoloFittingTemplate template)
    {
        Template = template;
    }
}

/// <summary>
/// Event args for template actions
/// </summary>
public class TemplateActionEventArgs : EventArgs
{
    public TemplateActionType ActionType { get; }
    public HoloFittingTemplate Template { get; }

    public TemplateActionEventArgs(TemplateActionType actionType, HoloFittingTemplate template)
    {
        ActionType = actionType;
        Template = template;
    }
}

/// <summary>
/// Event args for template sync
/// </summary>
public class TemplateSyncEventArgs : EventArgs
{
    public TemplateSyncResult Result { get; }

    public TemplateSyncEventArgs(TemplateSyncResult result)
    {
        Result = result;
    }
}

#endregion