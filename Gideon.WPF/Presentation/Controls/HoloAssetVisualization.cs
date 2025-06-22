// ==========================================================================
// HoloAssetVisualization.cs - Holographic Character Asset Visualization
// ==========================================================================
// Advanced asset management system featuring 3D holographic asset display,
// wealth tracking, EVE-style asset analysis, and interactive asset exploration.
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
/// Holographic asset visualization with 3D asset display and comprehensive wealth tracking
/// </summary>
public class HoloAssetVisualization : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloAssetVisualization),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloAssetVisualization),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty AssetCollectionsProperty =
        DependencyProperty.Register(nameof(AssetCollections), typeof(ObservableCollection<HoloAssetCollection>), typeof(HoloAssetVisualization),
            new PropertyMetadata(null, OnAssetCollectionsChanged));

    public static readonly DependencyProperty SelectedCharacterProperty =
        DependencyProperty.Register(nameof(SelectedCharacter), typeof(string), typeof(HoloAssetVisualization),
            new PropertyMetadata("Main Character", OnSelectedCharacterChanged));

    public static readonly DependencyProperty VisualizationModeProperty =
        DependencyProperty.Register(nameof(VisualizationMode), typeof(AssetVisualizationMode), typeof(HoloAssetVisualization),
            new PropertyMetadata(AssetVisualizationMode.Holographic3D, OnVisualizationModeChanged));

    public static readonly DependencyProperty AssetFilterProperty =
        DependencyProperty.Register(nameof(AssetFilter), typeof(AssetFilter), typeof(HoloAssetVisualization),
            new PropertyMetadata(AssetFilter.All, OnAssetFilterChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloAssetVisualization),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloAssetVisualization),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty Show3DVisualizationProperty =
        DependencyProperty.Register(nameof(Show3DVisualization), typeof(bool), typeof(HoloAssetVisualization),
            new PropertyMetadata(true, OnShow3DVisualizationChanged));

    public static readonly DependencyProperty ShowWealthAnalysisProperty =
        DependencyProperty.Register(nameof(ShowWealthAnalysis), typeof(bool), typeof(HoloAssetVisualization),
            new PropertyMetadata(true, OnShowWealthAnalysisChanged));

    public static readonly DependencyProperty ShowLocationMappingProperty =
        DependencyProperty.Register(nameof(ShowLocationMapping), typeof(bool), typeof(HoloAssetVisualization),
            new PropertyMetadata(true, OnShowLocationMappingChanged));

    public static readonly DependencyProperty AutoUpdateValuesProperty =
        DependencyProperty.Register(nameof(AutoUpdateValues), typeof(bool), typeof(HoloAssetVisualization),
            new PropertyMetadata(true, OnAutoUpdateValuesChanged));

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

    public ObservableCollection<HoloAssetCollection> AssetCollections
    {
        get => (ObservableCollection<HoloAssetCollection>)GetValue(AssetCollectionsProperty);
        set => SetValue(AssetCollectionsProperty, value);
    }

    public string SelectedCharacter
    {
        get => (string)GetValue(SelectedCharacterProperty);
        set => SetValue(SelectedCharacterProperty, value);
    }

    public AssetVisualizationMode VisualizationMode
    {
        get => (AssetVisualizationMode)GetValue(VisualizationModeProperty);
        set => SetValue(VisualizationModeProperty, value);
    }

    public AssetFilter AssetFilter
    {
        get => (AssetFilter)GetValue(AssetFilterProperty);
        set => SetValue(AssetFilterProperty, value);
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

    public bool Show3DVisualization
    {
        get => (bool)GetValue(Show3DVisualizationProperty);
        set => SetValue(Show3DVisualizationProperty, value);
    }

    public bool ShowWealthAnalysis
    {
        get => (bool)GetValue(ShowWealthAnalysisProperty);
        set => SetValue(ShowWealthAnalysisProperty, value);
    }

    public bool ShowLocationMapping
    {
        get => (bool)GetValue(ShowLocationMappingProperty);
        set => SetValue(ShowLocationMappingProperty, value);
    }

    public bool AutoUpdateValues
    {
        get => (bool)GetValue(AutoUpdateValuesProperty);
        set => SetValue(AutoUpdateValuesProperty, value);
    }

    #endregion

    #region Fields

    private Canvas _mainCanvas;
    private Canvas _particleCanvas;
    private Canvas _hologram3DCanvas;
    private Border _wealthSummarySection;
    private Border _assetCategoriesSection;
    private Border _locationMappingSection;
    private Border _assetDetailsSection;
    private ScrollViewer _assetListScrollViewer;
    private StackPanel _assetListPanel;
    private TabControl _assetTabs;
    
    private readonly List<HoloAssetParticle> _particles = new();
    private readonly List<HoloAsset3DNode> _asset3DNodes = new();
    private readonly DispatcherTimer _animationTimer;
    private readonly DispatcherTimer _valueUpdateTimer;
    private readonly Random _random = new();
    
    private bool _isAnimating = true;
    private bool _isDisposed = false;
    private double _currentAnimationTime = 0;

    #endregion

    #region Constructor

    public HoloAssetVisualization()
    {
        InitializeComponent();
        
        _animationTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16) // 60 FPS
        };
        _animationTimer.Tick += OnAnimationTick;
        
        _valueUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(1) // Update values every minute
        };
        _valueUpdateTimer.Tick += OnValueUpdateTick;
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 1200;
        Height = 800;
        Background = new SolidColorBrush(Color.FromArgb(20, 0, 255, 255));
        
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 0, 50)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 255, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(100, 0, 255, 255),
                ShadowDepth = 0,
                BlurRadius = 15
            }
        };

        _mainCanvas = new Canvas();
        
        _particleCanvas = new Canvas
        {
            IsHitTestVisible = false,
            ClipToBounds = true
        };
        
        _hologram3DCanvas = new Canvas
        {
            IsHitTestVisible = false,
            ClipToBounds = true
        };
        
        var contentGrid = new Grid();
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(150, GridUnitType.Pixel) });
        
        // Header section
        var headerSection = CreateHeaderSection();
        Grid.SetRow(headerSection, 0);
        contentGrid.Children.Add(headerSection);
        
        // Main content with tabs
        _assetTabs = CreateAssetTabs();
        Grid.SetRow(_assetTabs, 1);
        contentGrid.Children.Add(_assetTabs);
        
        // Bottom wealth analysis
        var bottomGrid = new Grid();
        bottomGrid.ColumnDefinitions.Add(new ColumnDefinition());
        bottomGrid.ColumnDefinitions.Add(new ColumnDefinition());
        
        _wealthSummarySection = CreateWealthSummarySection();
        Grid.SetColumn(_wealthSummarySection, 0);
        bottomGrid.Children.Add(_wealthSummarySection);
        
        var marketAnalysisSection = CreateMarketAnalysisSection();
        Grid.SetColumn(marketAnalysisSection, 1);
        bottomGrid.Children.Add(marketAnalysisSection);
        
        Grid.SetRow(bottomGrid, 2);
        contentGrid.Children.Add(bottomGrid);
        
        _mainCanvas.Children.Add(contentGrid);
        _mainCanvas.Children.Add(_hologram3DCanvas);
        _mainCanvas.Children.Add(_particleCanvas);
        border.Child = _mainCanvas;
        Content = border;

        CreateSampleData();
        Initialize3DVisualization();
    }

    private Border CreateHeaderSection()
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(60, 0, 100, 200)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(120, 0, 255, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(10),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(80, 0, 255, 255),
                ShadowDepth = 0,
                BlurRadius = 10
            }
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300, GridUnitType.Pixel) });

        // Title section
        var titlePanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(20, 15, 20, 15)
        };

        var titleText = new TextBlock
        {
            Text = "ASSET VISUALIZATION MATRIX",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        titlePanel.Children.Add(titleText);

        var subtitleText = new TextBlock
        {
            Text = "Holographic Wealth & Asset Management",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 200, 200)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0)
        };
        titlePanel.Children.Add(subtitleText);

        Grid.SetColumn(titlePanel, 0);
        grid.Children.Add(titlePanel);

        // Controls panel
        var controlsPanel = CreateControlsPanel();
        Grid.SetColumn(controlsPanel, 1);
        grid.Children.Add(controlsPanel);

        border.Child = grid;
        return border;
    }

    private StackPanel CreateControlsPanel()
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(15, 10, 15, 10)
        };

        var controlsHeader = new TextBlock
        {
            Text = "VISUALIZATION CONTROLS",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8)
        };
        panel.Children.Add(controlsHeader);

        // Visualization mode
        var modePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 3, 0, 3)
        };

        var modeLabel = new TextBlock
        {
            Text = "Mode:",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            Width = 60,
            VerticalAlignment = VerticalAlignment.Center
        };
        modePanel.Children.Add(modeLabel);

        var modeCombo = new ComboBox
        {
            Width = 120,
            Height = 22,
            FontSize = 9,
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 100, 150)),
            Foreground = new SolidColorBrush(Colors.White),
            BorderBrush = new SolidColorBrush(Color.FromArgb(120, 0, 200, 255))
        };
        modeCombo.Items.Add("3D Holographic");
        modeCombo.Items.Add("Category View");
        modeCombo.Items.Add("Location Map");
        modeCombo.Items.Add("Value Analysis");
        modeCombo.SelectedIndex = 0;
        modePanel.Children.Add(modeCombo);

        panel.Children.Add(modePanel);

        // Filter options
        var filterPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 3, 0, 3)
        };

        var filterLabel = new TextBlock
        {
            Text = "Filter:",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            Width = 60,
            VerticalAlignment = VerticalAlignment.Center
        };
        filterPanel.Children.Add(filterLabel);

        var filterCombo = new ComboBox
        {
            Width = 120,
            Height = 22,
            FontSize = 9,
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 100, 150)),
            Foreground = new SolidColorBrush(Colors.White),
            BorderBrush = new SolidColorBrush(Color.FromArgb(120, 0, 200, 255))
        };
        filterCombo.Items.Add("All Assets");
        filterCombo.Items.Add("Ships Only");
        filterCombo.Items.Add("High Value");
        filterCombo.Items.Add("Recent Items");
        filterCombo.SelectedIndex = 0;
        filterPanel.Children.Add(filterCombo);

        panel.Children.Add(filterPanel);

        // Checkboxes
        var options = new[]
        {
            "3D Visualization",
            "Wealth Analysis",
            "Location Mapping",
            "Auto-Update Values"
        };

        foreach (var option in options)
        {
            var checkbox = new CheckBox
            {
                Content = option,
                FontSize = 9,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                IsChecked = true,
                Margin = new Thickness(0, 2, 0, 2)
            };
            panel.Children.Add(checkbox);
        }

        return panel;
    }

    private TabControl CreateAssetTabs()
    {
        var tabControl = new TabControl
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(80, 0, 200, 255)),
            Margin = new Thickness(10)
        };

        // 3D Visualization Tab
        var visualization3DTab = new TabItem
        {
            Header = "3D View",
            Foreground = new SolidColorBrush(Colors.White),
            Background = new SolidColorBrush(Color.FromArgb(60, 0, 100, 150))
        };
        visualization3DTab.Content = Create3DVisualizationContent();
        tabControl.Items.Add(visualization3DTab);

        // Categories Tab
        var categoriesTab = new TabItem
        {
            Header = "Categories",
            Foreground = new SolidColorBrush(Colors.White),
            Background = new SolidColorBrush(Color.FromArgb(60, 0, 100, 150))
        };
        categoriesTab.Content = CreateCategoriesContent();
        tabControl.Items.Add(categoriesTab);

        // Locations Tab
        var locationsTab = new TabItem
        {
            Header = "Locations",
            Foreground = new SolidColorBrush(Colors.White),
            Background = new SolidColorBrush(Color.FromArgb(60, 0, 100, 150))
        };
        locationsTab.Content = CreateLocationsContent();
        tabControl.Items.Add(locationsTab);

        // Details Tab
        var detailsTab = new TabItem
        {
            Header = "Details",
            Foreground = new SolidColorBrush(Colors.White),
            Background = new SolidColorBrush(Color.FromArgb(60, 0, 100, 150))
        };
        detailsTab.Content = CreateDetailsContent();
        tabControl.Items.Add(detailsTab);

        return tabControl;
    }

    private Canvas Create3DVisualizationContent()
    {
        var canvas = new Canvas
        {
            Background = new SolidColorBrush(Color.FromArgb(20, 0, 50, 100)),
            Margin = new Thickness(10)
        };

        Create3DHolographicAssets(canvas);
        return canvas;
    }

    private void Create3DHolographicAssets(Canvas canvas)
    {
        canvas.Width = 800;
        canvas.Height = 500;

        var centerX = canvas.Width / 2;
        var centerY = canvas.Height / 2;

        // Create asset categories as 3D holographic nodes
        var assetCategories = new[]
        {
            new { Name = "Ships", Value = 23500000000L, Position = new Point(centerX - 150, centerY - 100), Color = Color.FromArgb(255, 255, 200, 0) },
            new { Name = "Modules", Value = 8700000000L, Position = new Point(centerX + 150, centerY - 100), Color = Color.FromArgb(255, 0, 255, 200) },
            new { Name = "Materials", Value = 3800000000L, Position = new Point(centerX - 150, centerY + 100), Color = Color.FromArgb(255, 100, 255, 100) },
            new { Name = "Ammunition", Value = 2100000000L, Position = new Point(centerX + 150, centerY + 100), Color = Color.FromArgb(255, 255, 100, 100) },
            new { Name = "Blueprints", Value = 1200000000L, Position = new Point(centerX, centerY - 150), Color = Color.FromArgb(255, 200, 0, 255) },
            new { Name = "Implants", Value = 890000000L, Position = new Point(centerX, centerY + 150), Color = Color.FromArgb(255, 255, 0, 200) }
        };

        foreach (var category in assetCategories)
        {
            var node = Create3DAssetNode(category.Name, category.Value, category.Position, category.Color);
            canvas.Children.Add(node);
            
            // Create connecting lines to center
            var connectionLine = new Line
            {
                X1 = category.Position.X,
                Y1 = category.Position.Y,
                X2 = centerX,
                Y2 = centerY,
                Stroke = new SolidColorBrush(Color.FromArgb(60, category.Color.R, category.Color.G, category.Color.B)),
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 5, 3 }
            };
            canvas.Children.Add(connectionLine);
        }

        // Central wealth indicator
        var centralNode = new Ellipse
        {
            Width = 60,
            Height = 60,
            Fill = new RadialGradientBrush
            {
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(255, 255, 255, 0), 0),
                    new GradientStop(Color.FromArgb(100, 255, 200, 0), 1)
                }
            },
            Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
            StrokeThickness = 3,
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(200, 255, 255, 0),
                ShadowDepth = 0,
                BlurRadius = 20
            }
        };
        Canvas.SetLeft(centralNode, centerX - 30);
        Canvas.SetTop(centralNode, centerY - 30);
        canvas.Children.Add(centralNode);

        var centralLabel = new TextBlock
        {
            Text = "NET WORTH\n47.2B ISK",
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };
        Canvas.SetLeft(centralLabel, centerX - 35);
        Canvas.SetTop(centralLabel, centerY + 35);
        canvas.Children.Add(centralLabel);
    }

    private Border Create3DAssetNode(string name, long value, Point position, Color color)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(100, color.R, color.G, color.B)),
            BorderBrush = new SolidColorBrush(color),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(8),
            Effect = new DropShadowEffect
            {
                Color = color,
                ShadowDepth = 0,
                BlurRadius = 15
            }
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10, 8, 10, 8)
        };

        var nameText = new TextBlock
        {
            Text = name,
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        panel.Children.Add(nameText);

        var valueText = new TextBlock
        {
            Text = $"{value / 1000000000.0:F1}B ISK",
            FontSize = 10,
            Foreground = new SolidColorBrush(color),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 2, 0, 0)
        };
        panel.Children.Add(valueText);

        border.Child = panel;
        Canvas.SetLeft(border, position.X - 40);
        Canvas.SetTop(border, position.Y - 20);

        return border;
    }

    private ScrollViewer CreateCategoriesContent()
    {
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = new SolidColorBrush(Color.FromArgb(20, 0, 50, 100))
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10)
        };

        // Asset categories with detailed breakdown
        var categories = new[]
        {
            new { Name = "Ships", Items = 47, Value = 23500000000L, Percentage = 49.7, Recent = "+2.1B ISK", Color = Color.FromArgb(255, 255, 200, 0) },
            new { Name = "Modules & Equipment", Items = 1523, Value = 8700000000L, Percentage = 18.4, Recent = "+456M ISK", Color = Color.FromArgb(255, 0, 255, 200) },
            new { Name = "Raw Materials", Items = 234567, Value = 3800000000L, Percentage = 8.0, Recent = "-123M ISK", Color = Color.FromArgb(255, 100, 255, 100) },
            new { Name = "Ammunition", Items = 45678, Value = 2100000000L, Percentage = 4.4, Recent = "+89M ISK", Color = Color.FromArgb(255, 255, 100, 100) },
            new { Name = "Blueprints", Items = 89, Value = 1200000000L, Percentage = 2.5, Recent = "+67M ISK", Color = Color.FromArgb(255, 200, 0, 255) },
            new { Name = "Implants & Boosters", Items = 23, Value = 890000000L, Percentage = 1.9, Recent = "No change", Color = Color.FromArgb(255, 255, 0, 200) },
            new { Name = "Skill Injectors", Items = 12, Value = 720000000L, Percentage = 1.5, Recent = "+720M ISK", Color = Color.FromArgb(255, 0, 200, 255) },
            new { Name = "Trade Goods", Items = 567, Value = 450000000L, Percentage = 0.9, Recent = "+23M ISK", Color = Color.FromArgb(255, 255, 150, 0) }
        };

        foreach (var category in categories)
        {
            var categoryItem = CreateCategoryItem(category.Name, category.Items, category.Value, category.Percentage, category.Recent, category.Color);
            panel.Children.Add(categoryItem);
        }

        scrollViewer.Content = panel;
        return scrollViewer;
    }

    private Border CreateCategoryItem(string name, int items, long value, double percentage, string recent, Color color)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, color.R, color.G, color.B)),
            BorderBrush = new SolidColorBrush(color),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(5),
            Effect = new DropShadowEffect
            {
                Color = color,
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80, GridUnitType.Pixel) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100, GridUnitType.Pixel) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80, GridUnitType.Pixel) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120, GridUnitType.Pixel) });
        grid.RowDefinitions.Add(new RowDefinition());
        grid.RowDefinitions.Add(new RowDefinition());

        // Category name
        var nameText = new TextBlock
        {
            Text = name,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(15, 10, 10, 5)
        };
        Grid.SetColumn(nameText, 0);
        Grid.SetRow(nameText, 0);
        grid.Children.Add(nameText);

        // Item count
        var itemsText = new TextBlock
        {
            Text = $"{items:N0}",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 10, 5, 5)
        };
        Grid.SetColumn(itemsText, 1);
        Grid.SetRow(itemsText, 0);
        grid.Children.Add(itemsText);

        // Value
        var valueText = new TextBlock
        {
            Text = $"{value / 1000000000.0:F1}B",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(color),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 10, 5, 5)
        };
        Grid.SetColumn(valueText, 2);
        Grid.SetRow(valueText, 0);
        grid.Children.Add(valueText);

        // Percentage
        var percentageText = new TextBlock
        {
            Text = $"{percentage:F1}%",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 10, 5, 5)
        };
        Grid.SetColumn(percentageText, 3);
        Grid.SetRow(percentageText, 0);
        grid.Children.Add(percentageText);

        // Recent change
        var recentColor = recent.StartsWith("+") ? Color.FromArgb(200, 0, 255, 100) :
                         recent.StartsWith("-") ? Color.FromArgb(200, 255, 100, 100) :
                         Color.FromArgb(200, 200, 200, 200);

        var recentText = new TextBlock
        {
            Text = recent,
            FontSize = 10,
            Foreground = new SolidColorBrush(recentColor),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 10, 15, 5)
        };
        Grid.SetColumn(recentText, 4);
        Grid.SetRow(recentText, 0);
        grid.Children.Add(recentText);

        // Progress bar
        var progressBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(60, 50, 50, 50)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 100, 100, 100)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(2),
            Height = 6,
            Margin = new Thickness(15, 0, 15, 10)
        };

        var progressBar = new Rectangle
        {
            Fill = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(color, 0),
                    new GradientStop(Color.FromArgb(color.A, (byte)(color.R * 0.7), (byte)(color.G * 0.7), (byte)(color.B * 0.7)), 1)
                }
            },
            Width = 400 * (percentage / 100),
            Height = 4,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            RadiusX = 1,
            RadiusY = 1
        };

        progressBorder.Child = progressBar;
        Grid.SetColumn(progressBorder, 0);
        Grid.SetRow(progressBorder, 1);
        Grid.SetColumnSpan(progressBorder, 5);
        grid.Children.Add(progressBorder);

        border.Child = grid;
        return border;
    }

    private ScrollViewer CreateLocationsContent()
    {
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = new SolidColorBrush(Color.FromArgb(20, 0, 50, 100))
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10)
        };

        // Asset locations
        var locations = new[]
        {
            new { Name = "Jita IV - Moon 4 - Caldari Navy Assembly Plant", Items = 1247, Value = 15200000000L, Security = "1.0", Color = Color.FromArgb(255, 0, 255, 100) },
            new { Name = "Dodixie IX - Moon 20 - Federation Navy Assembly Plant", Items = 834, Value = 8700000000L, Security = "0.9", Color = Color.FromArgb(255, 0, 200, 255) },
            new { Name = "Amarr VIII (Oris) - Emperor Family Academy", Items = 623, Value = 6500000000L, Security = "1.0", Color = Color.FromArgb(255, 255, 200, 0) },
            new { Name = "Rens VI - Moon 8 - Brutor Tribe Treasury", Items = 456, Value = 4200000000L, Security = "0.9", Color = Color.FromArgb(255, 255, 150, 0) },
            new { Name = "Player Citadels (Null-Sec)", Items = 789, Value = 12600000000L, Security = "0.0", Color = Color.FromArgb(255, 255, 100, 100) }
        };

        foreach (var location in locations)
        {
            var locationItem = CreateLocationItem(location.Name, location.Items, location.Value, location.Security, location.Color);
            panel.Children.Add(locationItem);
        }

        scrollViewer.Content = panel;
        return scrollViewer;
    }

    private Border CreateLocationItem(string name, int items, long value, string security, Color color)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, color.R, color.G, color.B)),
            BorderBrush = new SolidColorBrush(color),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(5),
            Effect = new DropShadowEffect
            {
                Color = color,
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80, GridUnitType.Pixel) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100, GridUnitType.Pixel) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60, GridUnitType.Pixel) });

        // Location name
        var nameText = new TextBlock
        {
            Text = name,
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(15, 15, 10, 15),
            TextWrapping = TextWrapping.Wrap
        };
        Grid.SetColumn(nameText, 0);
        grid.Children.Add(nameText);

        // Item count
        var itemsText = new TextBlock
        {
            Text = $"{items:N0}",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 15, 5, 15)
        };
        Grid.SetColumn(itemsText, 1);
        grid.Children.Add(itemsText);

        // Value
        var valueText = new TextBlock
        {
            Text = $"{value / 1000000000.0:F1}B",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(color),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 15, 5, 15)
        };
        Grid.SetColumn(valueText, 2);
        grid.Children.Add(valueText);

        // Security status
        var securityColor = security == "1.0" ? Color.FromArgb(200, 0, 255, 100) :
                           security == "0.9" ? Color.FromArgb(200, 255, 255, 0) :
                           Color.FromArgb(200, 255, 100, 100);

        var securityText = new TextBlock
        {
            Text = security,
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(securityColor),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 15, 15, 15)
        };
        Grid.SetColumn(securityText, 3);
        grid.Children.Add(securityText);

        border.Child = grid;
        return border;
    }

    private ScrollViewer CreateDetailsContent()
    {
        _assetListScrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            Background = new SolidColorBrush(Color.FromArgb(20, 0, 50, 100))
        };

        _assetListPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10)
        };

        _assetListScrollViewer.Content = _assetListPanel;
        UpdateAssetDetails();
        return _assetListScrollViewer;
    }

    private void UpdateAssetDetails()
    {
        if (_assetListPanel == null) return;

        _assetListPanel.Children.Clear();

        // Sample detailed asset list
        var assets = new[]
        {
            new { Name = "Raven Navy Issue", Quantity = 1, Value = 2100000000L, Location = "Jita IV - Moon 4", Type = "Ship" },
            new { Name = "Large Shield Extender II", Quantity = 50, Value = 45000000L, Location = "Jita IV - Moon 4", Type = "Module" },
            new { Name = "Tritanium", Quantity = 1000000, Value = 4200000L, Location = "Player Citadel", Type = "Material" },
            new { Name = "Large Projectile Turret II Blueprint", Quantity = 1, Value = 890000000L, Location = "Amarr VIII", Type = "Blueprint" },
            new { Name = "Neural Boost Standard", Quantity = 2, Value = 120000000L, Location = "Jita IV - Moon 4", Type = "Implant" }
        };

        foreach (var asset in assets)
        {
            var assetItem = CreateAssetDetailItem(asset.Name, asset.Quantity, asset.Value, asset.Location, asset.Type);
            _assetListPanel.Children.Add(assetItem);
        }
    }

    private Border CreateAssetDetailItem(string name, int quantity, long value, string location, string type)
    {
        var typeColor = type switch
        {
            "Ship" => Color.FromArgb(100, 255, 200, 0),
            "Module" => Color.FromArgb(100, 0, 255, 200),
            "Material" => Color.FromArgb(100, 100, 255, 100),
            "Blueprint" => Color.FromArgb(100, 200, 0, 255),
            "Implant" => Color.FromArgb(100, 255, 0, 200),
            _ => Color.FromArgb(100, 150, 150, 150)
        };

        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(30, typeColor.R, typeColor.G, typeColor.B)),
            BorderBrush = new SolidColorBrush(typeColor),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3),
            Margin = new Thickness(5, 2, 5, 2)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300, GridUnitType.Pixel) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80, GridUnitType.Pixel) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100, GridUnitType.Pixel) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80, GridUnitType.Pixel) });

        // Asset name
        var nameText = new TextBlock
        {
            Text = name,
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 8, 5, 8)
        };
        Grid.SetColumn(nameText, 0);
        grid.Children.Add(nameText);

        // Quantity
        var quantityText = new TextBlock
        {
            Text = quantity.ToString("N0"),
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 8, 5, 8)
        };
        Grid.SetColumn(quantityText, 1);
        grid.Children.Add(quantityText);

        // Value
        var valueText = new TextBlock
        {
            Text = value >= 1000000000 ? $"{value / 1000000000.0:F1}B" : $"{value / 1000000.0:F0}M",
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(typeColor),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 8, 5, 8)
        };
        Grid.SetColumn(valueText, 2);
        grid.Children.Add(valueText);

        // Location
        var locationText = new TextBlock
        {
            Text = location,
            FontSize = 9,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 200, 200, 200)),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 8, 5, 8),
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        Grid.SetColumn(locationText, 3);
        grid.Children.Add(locationText);

        // Type
        var typeText = new TextBlock
        {
            Text = type,
            FontSize = 9,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(typeColor),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 8, 10, 8)
        };
        Grid.SetColumn(typeText, 4);
        grid.Children.Add(typeText);

        border.Child = grid;
        return border;
    }

    private Border CreateWealthSummarySection()
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 150, 100, 0)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(80, 255, 200, 0)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(10),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(60, 255, 200, 0),
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(15, 10, 15, 10)
        };

        var header = new TextBlock
        {
            Text = "WEALTH ANALYSIS",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 200, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        panel.Children.Add(header);

        var wealth = new[]
        {
            "Total Net Worth: 47.2B ISK",
            "Liquid ISK: 8.4B ISK (17.8%)",
            "Asset Value: 38.8B ISK (82.2%)",
            "Daily Change: +234M ISK",
            "Weekly Growth: +1.2B ISK",
            "Portfolio Diversity: Excellent",
            "Risk Assessment: Moderate",
            "Liquidity Rating: Good"
        };

        foreach (var stat in wealth)
        {
            var statText = new TextBlock
            {
                Text = stat,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 200)),
                Margin = new Thickness(0, 2, 0, 0)
            };
            panel.Children.Add(statText);
        }

        border.Child = panel;
        return border;
    }

    private Border CreateMarketAnalysisSection()
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 150, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(80, 0, 255, 150)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(10),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(60, 0, 255, 150),
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(15, 10, 15, 10)
        };

        var header = new TextBlock
        {
            Text = "MARKET ANALYSIS",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 150)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        panel.Children.Add(header);

        var market = new[]
        {
            "Market Efficiency: 94.3%",
            "Active Orders: 47",
            "Order Value: 2.8B ISK",
            "Avg. Margin: 12.4%",
            "Best Performer: Tritanium",
            "Underperformer: Modules",
            "Restock Needed: 8 items",
            "Price Alerts: 3 active"
        };

        foreach (var stat in market)
        {
            var statText = new TextBlock
            {
                Text = stat,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                Margin = new Thickness(0, 2, 0, 0)
            };
            panel.Children.Add(statText);
        }

        border.Child = panel;
        return border;
    }

    private void CreateSampleData()
    {
        AssetCollections = new ObservableCollection<HoloAssetCollection>
        {
            new HoloAssetCollection
            {
                CollectionName = "Ships",
                TotalValue = 23500000000,
                ItemCount = 47,
                LastUpdated = DateTime.Now.AddMinutes(-5),
                LocationCount = 8,
                AverageValue = 500000000
            },
            new HoloAssetCollection
            {
                CollectionName = "Modules & Equipment",
                TotalValue = 8700000000,
                ItemCount = 1523,
                LastUpdated = DateTime.Now.AddMinutes(-3),
                LocationCount = 12,
                AverageValue = 5700000
            },
            new HoloAssetCollection
            {
                CollectionName = "Materials",
                TotalValue = 3800000000,
                ItemCount = 234567,
                LastUpdated = DateTime.Now.AddMinutes(-1),
                LocationCount = 6,
                AverageValue = 16200
            }
        };
    }

    private void Initialize3DVisualization()
    {
        // Initialize 3D asset nodes for holographic display
    }

    #endregion

    #region Particle System

    private void CreateAssetParticles()
    {
        if (!EnableParticleEffects || _particleCanvas == null) return;

        for (int i = 0; i < 2; i++)
        {
            var particle = new HoloAssetParticle
            {
                X = _random.NextDouble() * ActualWidth,
                Y = _random.NextDouble() * ActualHeight,
                VelocityX = (_random.NextDouble() - 0.5) * 3,
                VelocityY = (_random.NextDouble() - 0.5) * 3,
                Size = _random.NextDouble() * 3 + 1,
                Life = 1.0,
                ParticleType = AssetParticleType.Data,
                Ellipse = new Ellipse
                {
                    Width = 3,
                    Height = 3,
                    Fill = new SolidColorBrush(Color.FromArgb(40, 255, 200, 0))
                }
            };

            Canvas.SetLeft(particle.Ellipse, particle.X);
            Canvas.SetTop(particle.Ellipse, particle.Y);
            _particleCanvas.Children.Add(particle.Ellipse);
            _particles.Add(particle);
        }
    }

    private void UpdateParticles()
    {
        if (!EnableParticleEffects) return;

        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var particle = _particles[i];
            
            particle.X += particle.VelocityX * 0.016;
            particle.Y += particle.VelocityY * 0.016;
            particle.Life -= 0.001;

            if (particle.Life <= 0 || particle.X < 0 || particle.X > ActualWidth || 
                particle.Y < 0 || particle.Y > ActualHeight)
            {
                _particleCanvas.Children.Remove(particle.Ellipse);
                _particles.RemoveAt(i);
                continue;
            }

            Canvas.SetLeft(particle.Ellipse, particle.X);
            Canvas.SetTop(particle.Ellipse, particle.Y);
            
            var alpha = (byte)(particle.Life * 40);
            var color = particle.ParticleType switch
            {
                AssetParticleType.Data => Color.FromArgb(alpha, 255, 200, 0),
                AssetParticleType.Value => Color.FromArgb(alpha, 0, 255, 200),
                AssetParticleType.Transfer => Color.FromArgb(alpha, 200, 0, 255),
                _ => Color.FromArgb(alpha, 255, 255, 255)
            };
            
            particle.Ellipse.Fill = new SolidColorBrush(color);
        }

        if (_random.NextDouble() < 0.01)
        {
            CreateAssetParticles();
        }
    }

    #endregion

    #region Animation and Updates

    private void OnAnimationTick(object sender, EventArgs e)
    {
        if (!_isAnimating) return;

        _currentAnimationTime += 16;
        
        UpdateParticles();
        UpdateAssetAnimations();
    }

    private void OnValueUpdateTick(object sender, EventArgs e)
    {
        if (AutoUpdateValues)
        {
            UpdateAssetValues();
        }
    }

    private void UpdateAssetAnimations()
    {
        if (!EnableAnimations) return;

        var intensity = (Math.Sin(_currentAnimationTime * 0.0002) + 1) * 0.5 * HolographicIntensity;
        
        // Animate section glows
        var sections = new[] { _wealthSummarySection, _assetCategoriesSection, _locationMappingSection, _assetDetailsSection };
        foreach (var section in sections)
        {
            if (section?.Effect is DropShadowEffect effect)
            {
                effect.BlurRadius = 8 + intensity * 2;
            }
        }
    }

    private void UpdateAssetValues()
    {
        // Simulate asset value changes
        if (AssetCollections != null)
        {
            foreach (var collection in AssetCollections)
            {
                var change = (_random.NextDouble() - 0.5) * 0.02; // ±1% change
                collection.TotalValue += (long)(collection.TotalValue * change);
                collection.LastUpdated = DateTime.Now;
            }
        }
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _animationTimer.Start();
        if (AutoUpdateValues)
        {
            _valueUpdateTimer.Start();
        }
        _isAnimating = true;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer.Stop();
        _valueUpdateTimer.Stop();
        _isAnimating = false;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_particleCanvas != null)
        {
            _particleCanvas.Width = ActualWidth;
            _particleCanvas.Height = ActualHeight;
        }
        if (_hologram3DCanvas != null)
        {
            _hologram3DCanvas.Width = ActualWidth;
            _hologram3DCanvas.Height = ActualHeight;
        }
    }

    #endregion

    #region Property Change Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAssetVisualization control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAssetVisualization control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnAssetCollectionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAssetVisualization control)
        {
            control.RefreshAssetDisplay();
        }
    }

    private static void OnSelectedCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAssetVisualization control)
        {
            control.LoadCharacterAssets();
        }
    }

    private static void OnVisualizationModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAssetVisualization control)
        {
            control.UpdateVisualizationMode();
        }
    }

    private static void OnAssetFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAssetVisualization control)
        {
            control.ApplyAssetFilter();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAssetVisualization control)
        {
            control._isAnimating = control.EnableAnimations;
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAssetVisualization control)
        {
            if (!control.EnableParticleEffects)
            {
                control.ClearParticles();
            }
        }
    }

    private static void OnShow3DVisualizationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAssetVisualization control)
        {
            control._hologram3DCanvas.Visibility = control.Show3DVisualization ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnShowWealthAnalysisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAssetVisualization control)
        {
            control._wealthSummarySection.Visibility = control.ShowWealthAnalysis ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnShowLocationMappingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAssetVisualization control)
        {
            control._locationMappingSection.Visibility = control.ShowLocationMapping ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnAutoUpdateValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAssetVisualization control)
        {
            if (control.AutoUpdateValues)
            {
                control._valueUpdateTimer.Start();
            }
            else
            {
                control._valueUpdateTimer.Stop();
            }
        }
    }

    #endregion

    #region Helper Methods

    private void UpdateHolographicEffects()
    {
        // Update holographic intensity for all effects
    }

    private void UpdateColorScheme()
    {
        // Update colors based on EVE color scheme
    }

    private void RefreshAssetDisplay()
    {
        // Refresh asset data display
    }

    private void LoadCharacterAssets()
    {
        // Load assets for selected character
    }

    private void UpdateVisualizationMode()
    {
        // Update visualization based on mode
    }

    private void ApplyAssetFilter()
    {
        // Apply selected asset filter
    }

    private void ClearParticles()
    {
        foreach (var particle in _particles)
        {
            _particleCanvas.Children.Remove(particle.Ellipse);
        }
        _particles.Clear();
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void OptimizeForLowEndHardware()
    {
        EnableParticleEffects = false;
        EnableAnimations = false;
        Show3DVisualization = false;
    }

    public void OptimizeForHighEndHardware()
    {
        EnableParticleEffects = true;
        EnableAnimations = true;
        Show3DVisualization = true;
    }

    public bool SupportsHardwareAcceleration => true;

    #endregion

    #region Cleanup

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        
        if (_mainCanvas != null)
        {
            _mainCanvas.Width = ActualWidth;
            _mainCanvas.Height = ActualHeight;
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _animationTimer?.Stop();
        _valueUpdateTimer?.Stop();
        ClearParticles();
        
        _isDisposed = true;
    }

    #endregion
}

#region Supporting Classes

public class HoloAssetCollection
{
    public string CollectionName { get; set; } = "";
    public long TotalValue { get; set; }
    public int ItemCount { get; set; }
    public DateTime LastUpdated { get; set; }
    public int LocationCount { get; set; }
    public long AverageValue { get; set; }
    public List<HoloAssetItem> Assets { get; set; } = new();
}

public class HoloAssetItem
{
    public string ItemName { get; set; } = "";
    public int Quantity { get; set; }
    public long UnitValue { get; set; }
    public long TotalValue { get; set; }
    public string Location { get; set; } = "";
    public string ItemType { get; set; } = "";
    public DateTime LastPriceUpdate { get; set; }
    public bool IsPackaged { get; set; }
}

public class HoloAsset3DNode
{
    public Point3D Position { get; set; }
    public string AssetType { get; set; } = "";
    public long Value { get; set; }
    public double Size { get; set; }
    public Color NodeColor { get; set; }
    public bool IsAnimated { get; set; }
}

public class HoloAssetParticle
{
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Size { get; set; }
    public double Life { get; set; }
    public AssetParticleType ParticleType { get; set; }
    public Ellipse Ellipse { get; set; } = null!;
}

public enum AssetVisualizationMode
{
    Holographic3D,
    CategoryView,
    LocationMap,
    ValueAnalysis
}

public enum AssetFilter
{
    All,
    ShipsOnly,
    HighValue,
    RecentItems,
    LocationSpecific,
    TypeSpecific
}

public enum AssetParticleType
{
    Data,
    Value,
    Transfer,
    Update
}

#endregion