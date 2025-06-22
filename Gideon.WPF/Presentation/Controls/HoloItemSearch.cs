// ==========================================================================
// HoloItemSearch.cs - Holographic Item Search Interface
// ==========================================================================
// Advanced item search featuring real-time search with particle effects,
// autocomplete functionality, EVE-style item database, and holographic search visualization.
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
/// Holographic item search with real-time filtering, particle effects, and comprehensive EVE item database
/// </summary>
public class HoloItemSearch : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloItemSearch),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloItemSearch),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty SearchTextProperty =
        DependencyProperty.Register(nameof(SearchText), typeof(string), typeof(HoloItemSearch),
            new PropertyMetadata("", OnSearchTextChanged));

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(HoloEVEItem), typeof(HoloItemSearch),
            new PropertyMetadata(null, OnSelectedItemChanged));

    public static readonly DependencyProperty SearchResultsProperty =
        DependencyProperty.Register(nameof(SearchResults), typeof(ObservableCollection<HoloEVEItem>), typeof(HoloItemSearch),
            new PropertyMetadata(null, OnSearchResultsChanged));

    public static readonly DependencyProperty CategoryFilterProperty =
        DependencyProperty.Register(nameof(CategoryFilter), typeof(EVEItemCategory), typeof(HoloItemSearch),
            new PropertyMetadata(EVEItemCategory.All, OnCategoryFilterChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloItemSearch),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloItemSearch),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowItemDetailsProperty =
        DependencyProperty.Register(nameof(ShowItemDetails), typeof(bool), typeof(HoloItemSearch),
            new PropertyMetadata(true, OnShowItemDetailsChanged));

    public static readonly DependencyProperty ShowFavoritesProperty =
        DependencyProperty.Register(nameof(ShowFavorites), typeof(bool), typeof(HoloItemSearch),
            new PropertyMetadata(true, OnShowFavoritesChanged));

    public static readonly DependencyProperty AutoCompleteProperty =
        DependencyProperty.Register(nameof(AutoComplete), typeof(bool), typeof(HoloItemSearch),
            new PropertyMetadata(true, OnAutoCompleteChanged));

    public static readonly DependencyProperty MaxResultsProperty =
        DependencyProperty.Register(nameof(MaxResults), typeof(int), typeof(HoloItemSearch),
            new PropertyMetadata(50, OnMaxResultsChanged));

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

    public string SearchText
    {
        get => (string)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public HoloEVEItem SelectedItem
    {
        get => (HoloEVEItem)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public ObservableCollection<HoloEVEItem> SearchResults
    {
        get => (ObservableCollection<HoloEVEItem>)GetValue(SearchResultsProperty);
        set => SetValue(SearchResultsProperty, value);
    }

    public EVEItemCategory CategoryFilter
    {
        get => (EVEItemCategory)GetValue(CategoryFilterProperty);
        set => SetValue(CategoryFilterProperty, value);
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

    public bool ShowItemDetails
    {
        get => (bool)GetValue(ShowItemDetailsProperty);
        set => SetValue(ShowItemDetailsProperty, value);
    }

    public bool ShowFavorites
    {
        get => (bool)GetValue(ShowFavoritesProperty);
        set => SetValue(ShowFavoritesProperty, value);
    }

    public bool AutoComplete
    {
        get => (bool)GetValue(AutoCompleteProperty);
        set => SetValue(AutoCompleteProperty, value);
    }

    public int MaxResults
    {
        get => (int)GetValue(MaxResultsProperty);
        set => SetValue(MaxResultsProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloEVEItem> ItemSelected;
    public event EventHandler<string> SearchTextChanged;
    public event EventHandler<HoloEVEItem> ItemFavorited;

    #endregion

    #region Fields

    private Grid _mainGrid;
    private TextBox _searchBox;
    private ComboBox _categoryCombo;
    private ListBox _autoCompleteList;
    private DataGrid _resultsGrid;
    private Border _itemDetailsPanel;
    private Canvas _particleCanvas;
    private StackPanel _favoritesPanel;
    
    private DispatcherTimer _searchTimer;
    private DispatcherTimer _animationTimer;
    private DispatcherTimer _particleTimer;
    
    private readonly Random _random = new();
    private List<UIElement> _particles;
    private List<UIElement> _searchEffects;
    
    // EVE item database
    private List<HoloEVEItem> _eveItemDatabase;
    private List<HoloEVEItem> _favoriteItems;
    private Dictionary<string, List<string>> _searchSuggestions;
    
    // Search state
    private string _lastSearchText = "";
    private bool _isSearching = false;
    private int _searchResultCount = 0;
    private DateTime _lastSearchTime = DateTime.MinValue;
    
    // Animation state
    private double _animationProgress = 0.0;
    private bool _isAnimating = false;
    
    // Performance optimization
    private bool _isInSimplifiedMode = false;
    private int _maxParticles = 20;

    #endregion

    #region Constructor

    public HoloItemSearch()
    {
        InitializeComponent();
        InitializeItemSearchSystem();
        SetupEventHandlers();
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 800;
        Height = 700;
        Background = new SolidColorBrush(Color.FromArgb(30, 0, 50, 100));
        
        // Create main layout
        _mainGrid = new Grid();
        Content = _mainGrid;

        // Define layout structure
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Search controls
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Results
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(200) }); // Item details

        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }); // Main content
        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Favorites

        CreateHeader();
        CreateSearchControls();
        CreateResultsGrid();
        CreateItemDetailsPanel();
        CreateFavoritesPanel();
        CreateParticleLayer();
    }

    private void CreateHeader()
    {
        var headerPanel = new Border
        {
            Height = 50,
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(150, 0, 100, 200), 0),
                    new GradientStop(Color.FromArgb(100, 0, 150, 255), 1)
                }
            },
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 0, 200, 255)),
            BorderThickness = new Thickness(0, 0, 0, 2),
            CornerRadius = new CornerRadius(5, 5, 0, 0)
        };

        var headerContent = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.SpaceBetween,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(20, 10, 20, 10)
        };

        var titleText = new TextBlock
        {
            Text = "HOLOGRAPHIC ITEM SEARCH",
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };

        var statusText = new TextBlock
        {
            Text = "DATABASE READY",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 0, 255, 100)),
            Tag = "StatusText"
        };

        headerContent.Children.Add(titleText);
        headerContent.Children.Add(statusText);
        headerPanel.Child = headerContent;

        Grid.SetRow(headerPanel, 0);
        Grid.SetColumnSpan(headerPanel, 2);
        _mainGrid.Children.Add(headerPanel);
    }

    private void CreateSearchControls()
    {
        var searchPanel = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(50, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 150, 255)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(5),
            CornerRadius = new CornerRadius(5),
            Padding = new Thickness(15, 10, 15, 10)
        };

        var searchContent = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        // Search input row
        var searchInputPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 10)
        };

        var searchLabel = new TextBlock
        {
            Text = "SEARCH:",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };

        _searchBox = new TextBox
        {
            Width = 300,
            Height = 30,
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 50, 100)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 0, 200, 255)),
            BorderThickness = new Thickness(2),
            FontSize = 12,
            Margin = new Thickness(0, 0, 10, 0),
            VerticalContentAlignment = VerticalAlignment.Center,
            Padding = new Thickness(8, 0, 8, 0)
        };

        _categoryCombo = new ComboBox
        {
            Width = 150,
            Height = 30,
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 50, 100)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 0, 150, 255)),
            FontSize = 11
        };

        InitializeCategoryCombo();

        var clearButton = CreateHoloButton("CLEAR", ClearSearch);

        searchInputPanel.Children.Add(searchLabel);
        searchInputPanel.Children.Add(_searchBox);
        searchInputPanel.Children.Add(_categoryCombo);
        searchInputPanel.Children.Add(clearButton);

        // Auto-complete list
        _autoCompleteList = new ListBox
        {
            MaxHeight = 150,
            Background = new SolidColorBrush(Color.FromArgb(200, 0, 30, 60)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 0, 200, 255)),
            BorderThickness = new Thickness(1),
            FontSize = 10,
            Visibility = Visibility.Collapsed,
            Margin = new Thickness(70, 0, 0, 0)
        };

        searchContent.Children.Add(searchInputPanel);
        searchContent.Children.Add(_autoCompleteList);
        searchPanel.Child = searchContent;

        Grid.SetRow(searchPanel, 1);
        Grid.SetColumn(searchPanel, 0);
        _mainGrid.Children.Add(searchPanel);
    }

    private void InitializeCategoryCombo()
    {
        var categories = new[]
        {
            "All Categories", "Ships", "Modules", "Ammunition", "Drones",
            "Implants", "Blueprints", "Materials", "Commodities", "Apparel"
        };

        foreach (var category in categories)
        {
            _categoryCombo.Items.Add(category);
        }

        _categoryCombo.SelectedIndex = 0;
        _categoryCombo.SelectionChanged += (s, e) => CategoryFilter = (EVEItemCategory)_categoryCombo.SelectedIndex;
    }

    private Button CreateHoloButton(string text, Action onClick)
    {
        var button = new Button
        {
            Content = text,
            Width = 70,
            Height = 30,
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 150, 255)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 0, 200, 255)),
            BorderThickness = new Thickness(1),
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            CornerRadius = new CornerRadius(3),
            Margin = new Thickness(5, 0, 0, 0)
        };

        button.Click += (s, e) => onClick();
        return button;
    }

    private void CreateResultsGrid()
    {
        var resultsBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(30, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 150, 255)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(5),
            CornerRadius = new CornerRadius(5)
        };

        var resultsContent = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var resultsTitle = new TextBlock
        {
            Text = "SEARCH RESULTS",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 10)
        };

        _resultsGrid = new DataGrid
        {
            Background = Brushes.Transparent,
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            CanUserAddRows = false,
            CanUserDeleteRows = false,
            CanUserReorderColumns = false,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            IsReadOnly = true,
            SelectionMode = DataGridSelectionMode.Single,
            AutoGenerateColumns = false,
            Margin = new Thickness(5)
        };

        CreateResultsGridColumns();

        resultsContent.Children.Add(resultsTitle);
        resultsContent.Children.Add(_resultsGrid);
        resultsBorder.Child = resultsContent;

        Grid.SetRow(resultsBorder, 2);
        Grid.SetColumn(resultsBorder, 0);
        _mainGrid.Children.Add(resultsBorder);
    }

    private void CreateResultsGridColumns()
    {
        var iconColumn = new DataGridTemplateColumn
        {
            Header = "",
            Width = new DataGridLength(30),
            CellTemplate = CreateIconCellTemplate()
        };

        var nameColumn = new DataGridTextColumn
        {
            Header = "Item Name",
            Binding = new Binding("ItemName"),
            Width = new DataGridLength(200),
            ElementStyle = CreateCellStyle(Colors.White)
        };

        var categoryColumn = new DataGridTextColumn
        {
            Header = "Category",
            Binding = new Binding("Category"),
            Width = new DataGridLength(100),
            ElementStyle = CreateCellStyle(Colors.Cyan)
        };

        var groupColumn = new DataGridTextColumn
        {
            Header = "Group",
            Binding = new Binding("Group"),
            Width = new DataGridLength(120),
            ElementStyle = CreateCellStyle(Colors.Yellow)
        };

        var techLevelColumn = new DataGridTextColumn
        {
            Header = "Tech",
            Binding = new Binding("TechLevel"),
            Width = new DataGridLength(50),
            ElementStyle = CreateCellStyle(Colors.Orange)
        };

        var favoriteColumn = new DataGridTemplateColumn
        {
            Header = "★",
            Width = new DataGridLength(30),
            CellTemplate = CreateFavoriteCellTemplate()
        };

        _resultsGrid.Columns.Add(iconColumn);
        _resultsGrid.Columns.Add(nameColumn);
        _resultsGrid.Columns.Add(categoryColumn);
        _resultsGrid.Columns.Add(groupColumn);
        _resultsGrid.Columns.Add(techLevelColumn);
        _resultsGrid.Columns.Add(favoriteColumn);
    }

    private Style CreateCellStyle(Color color)
    {
        var style = new Style(typeof(TextBlock));
        style.Setters.Add(new Setter(TextBlock.ForegroundProperty, new SolidColorBrush(color)));
        style.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.Bold));
        style.Setters.Add(new Setter(TextBlock.FontSizeProperty, 10.0));
        return style;
    }

    private DataTemplate CreateIconCellTemplate()
    {
        var template = new DataTemplate();
        
        var factory = new FrameworkElementFactory(typeof(Ellipse));
        factory.SetValue(Ellipse.WidthProperty, 16.0);
        factory.SetValue(Ellipse.HeightProperty, 16.0);
        factory.SetBinding(Ellipse.FillProperty, new Binding("CategoryColor"));
        factory.SetValue(Ellipse.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        
        template.VisualTree = factory;
        return template;
    }

    private DataTemplate CreateFavoriteCellTemplate()
    {
        var template = new DataTemplate();
        
        var factory = new FrameworkElementFactory(typeof(Button));
        factory.SetValue(Button.ContentProperty, "★");
        factory.SetValue(Button.WidthProperty, 20.0);
        factory.SetValue(Button.HeightProperty, 20.0);
        factory.SetValue(Button.FontSizeProperty, 12.0);
        factory.SetValue(Button.BackgroundProperty, Brushes.Transparent);
        factory.SetValue(Button.BorderThicknessProperty, new Thickness(0));
        factory.SetBinding(Button.ForegroundProperty, new Binding("FavoriteColor"));
        factory.AddHandler(Button.ClickEvent, new RoutedEventHandler(OnFavoriteButtonClick));
        
        template.VisualTree = factory;
        return template;
    }

    private void CreateItemDetailsPanel()
    {
        _itemDetailsPanel = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(50, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 150, 255)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(5),
            CornerRadius = new CornerRadius(5),
            Padding = new Thickness(10)
        };

        var detailsContent = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var detailsTitle = new TextBlock
        {
            Text = "ITEM DETAILS",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };

        var noSelectionText = new TextBlock
        {
            Text = "Select an item to view details",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(150, 200, 200, 200)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Tag = "NoSelection"
        };

        detailsContent.Children.Add(detailsTitle);
        detailsContent.Children.Add(noSelectionText);
        _itemDetailsPanel.Child = detailsContent;

        Grid.SetRow(_itemDetailsPanel, 3);
        Grid.SetColumnSpan(_itemDetailsPanel, 2);
        _mainGrid.Children.Add(_itemDetailsPanel);
    }

    private void CreateFavoritesPanel()
    {
        var favoritesBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 0, 150, 255)),
            BorderThickness = new Thickness(2),
            Margin = new Thickness(5),
            CornerRadius = new CornerRadius(5),
            Padding = new Thickness(10)
        };

        var favoritesContent = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var favoritesTitle = new TextBlock
        {
            Text = "FAVORITES",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 215, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };

        _favoritesPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = _favoritesPanel,
            MaxHeight = 400
        };

        favoritesContent.Children.Add(favoritesTitle);
        favoritesContent.Children.Add(scrollViewer);
        favoritesBorder.Child = favoritesContent;

        Grid.SetRow(favoritesBorder, 1);
        Grid.SetRowSpan(favoritesBorder, 2);
        Grid.SetColumn(favoritesBorder, 1);
        _mainGrid.Children.Add(favoritesBorder);
    }

    private void CreateParticleLayer()
    {
        _particleCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false
        };

        Grid.SetRowSpan(_particleCanvas, 4);
        Grid.SetColumnSpan(_particleCanvas, 2);
        _mainGrid.Children.Add(_particleCanvas);
    }

    private void InitializeItemSearchSystem()
    {
        _particles = new List<UIElement>();
        _searchEffects = new List<UIElement>();
        _favoriteItems = new List<HoloEVEItem>();
        _searchSuggestions = new Dictionary<string, List<string>>();
        
        SearchResults = new ObservableCollection<HoloEVEItem>();
        
        // Initialize EVE item database
        InitializeEVEItemDatabase();
        
        // Setup search suggestions
        BuildSearchSuggestions();
        
        // Setup timers
        _searchTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(300)
        };
        _searchTimer.Tick += SearchTimer_Tick;
        
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // 60 FPS
        };
        _animationTimer.Tick += AnimationTimer_Tick;
        
        _particleTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _particleTimer.Tick += ParticleTimer_Tick;
    }

    private void SetupEventHandlers()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        
        _searchBox.TextChanged += SearchBox_TextChanged;
        _searchBox.KeyDown += SearchBox_KeyDown;
        _searchBox.LostFocus += SearchBox_LostFocus;
        
        _autoCompleteList.SelectionChanged += AutoCompleteList_SelectionChanged;
        _autoCompleteList.MouseDoubleClick += AutoCompleteList_MouseDoubleClick;
        
        _resultsGrid.SelectionChanged += ResultsGrid_SelectionChanged;
        _resultsGrid.MouseDoubleClick += ResultsGrid_MouseDoubleClick;
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        StartItemSearchSystem();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopItemSearchSystem();
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        SearchText = _searchBox.Text;
        _searchTimer.Stop();
        _searchTimer.Start();
        
        if (AutoComplete && !string.IsNullOrEmpty(SearchText) && SearchText.Length >= 2)
        {
            ShowAutoComplete();
        }
        else
        {
            HideAutoComplete();
        }
    }

    private void SearchBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            PerformSearch();
            HideAutoComplete();
        }
        else if (e.Key == Key.Escape)
        {
            HideAutoComplete();
        }
        else if (e.Key == Key.Down && _autoCompleteList.Visibility == Visibility.Visible)
        {
            if (_autoCompleteList.Items.Count > 0)
            {
                _autoCompleteList.SelectedIndex = 0;
                _autoCompleteList.Focus();
            }
        }
    }

    private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
    {
        // Delay hiding autocomplete to allow selection
        Dispatcher.BeginInvoke(new Action(() => 
        {
            if (!_autoCompleteList.IsMouseOver && !_autoCompleteList.IsFocused)
            {
                HideAutoComplete();
            }
        }), DispatcherPriority.Background);
    }

    private void AutoCompleteList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_autoCompleteList.SelectedItem is string suggestion)
        {
            _searchBox.Text = suggestion;
            _searchBox.CaretIndex = suggestion.Length;
        }
    }

    private void AutoCompleteList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_autoCompleteList.SelectedItem is string suggestion)
        {
            _searchBox.Text = suggestion;
            PerformSearch();
            HideAutoComplete();
        }
    }

    private void ResultsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_resultsGrid.SelectedItem is HoloEVEItem selectedItem)
        {
            SelectedItem = selectedItem;
            UpdateItemDetails();
        }
    }

    private void ResultsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_resultsGrid.SelectedItem is HoloEVEItem selectedItem)
        {
            ItemSelected?.Invoke(this, selectedItem);
        }
    }

    private void OnFavoriteButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is HoloEVEItem item)
        {
            ToggleFavorite(item);
        }
    }

    private void SearchTimer_Tick(object sender, EventArgs e)
    {
        _searchTimer.Stop();
        PerformSearch();
    }

    private void AnimationTimer_Tick(object sender, EventArgs e)
    {
        if (_isAnimating && EnableAnimations)
        {
            _animationProgress += 0.02;
            if (_animationProgress >= 1.0)
            {
                _animationProgress = 1.0;
                _isAnimating = false;
            }
            
            UpdateAnimatedElements();
        }
    }

    private void ParticleTimer_Tick(object sender, EventArgs e)
    {
        if (EnableParticleEffects && !_isInSimplifiedMode)
        {
            UpdateParticleEffects();
            
            if (_isSearching)
            {
                CreateSearchParticles();
            }
        }
    }

    #endregion

    #region Item Search System

    public void StartItemSearchSystem()
    {
        if (EnableAnimations)
        {
            _animationTimer.Start();
        }
        
        if (EnableParticleEffects)
        {
            _particleTimer.Start();
        }
        
        UpdateStatusText("READY");
        UpdateFavoritesDisplay();
    }

    public void StopItemSearchSystem()
    {
        _searchTimer?.Stop();
        _animationTimer?.Stop();
        _particleTimer?.Stop();
    }

    private void PerformSearch()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            SearchResults.Clear();
            _searchResultCount = 0;
            UpdateStatusText("READY");
            return;
        }

        _isSearching = true;
        _lastSearchTime = DateTime.Now;
        UpdateStatusText("SEARCHING...");
        
        if (EnableParticleEffects && !_isInSimplifiedMode)
        {
            CreateSearchStartParticles();
        }

        // Perform the actual search
        var results = SearchItems(SearchText, CategoryFilter);
        
        SearchResults.Clear();
        foreach (var item in results.Take(MaxResults))
        {
            SearchResults.Add(item);
        }
        
        _resultsGrid.ItemsSource = SearchResults;
        _searchResultCount = results.Count;
        
        _isSearching = false;
        UpdateStatusText($"FOUND {_searchResultCount} ITEMS");
        
        StartAnimation();
        
        SearchTextChanged?.Invoke(this, SearchText);
    }

    private List<HoloEVEItem> SearchItems(string searchText, EVEItemCategory category)
    {
        if (_eveItemDatabase == null) return new List<HoloEVEItem>();

        var query = _eveItemDatabase.AsEnumerable();

        // Apply category filter
        if (category != EVEItemCategory.All)
        {
            query = query.Where(item => item.Category == category.ToString());
        }

        // Apply text search
        var searchTerms = searchText.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        query = query.Where(item => 
            searchTerms.All(term => 
                item.ItemName.ToLower().Contains(term) || 
                item.Group.ToLower().Contains(term) ||
                item.Category.ToLower().Contains(term)
            )
        );

        // Score and sort results
        var scoredResults = query.Select(item => new
        {
            Item = item,
            Score = CalculateSearchScore(item, searchText)
        })
        .OrderByDescending(r => r.Score)
        .Select(r => r.Item)
        .ToList();

        return scoredResults;
    }

    private double CalculateSearchScore(HoloEVEItem item, string searchText)
    {
        var score = 0.0;
        var searchLower = searchText.ToLower();
        var itemNameLower = item.ItemName.ToLower();

        // Exact match gets highest score
        if (itemNameLower == searchLower)
            score += 100;
        
        // Starts with search text
        if (itemNameLower.StartsWith(searchLower))
            score += 50;
        
        // Contains search text
        if (itemNameLower.Contains(searchLower))
            score += 25;
        
        // Favorite items get bonus
        if (_favoriteItems.Any(f => f.ItemId == item.ItemId))
            score += 10;
        
        // Tech level bonus (higher tech = higher score)
        if (int.TryParse(item.TechLevel, out var techLevel))
            score += techLevel * 2;

        return score;
    }

    private void ShowAutoComplete()
    {
        if (!AutoComplete || string.IsNullOrEmpty(SearchText) || SearchText.Length < 2)
        {
            HideAutoComplete();
            return;
        }

        var suggestions = GetSearchSuggestions(SearchText);
        if (suggestions.Any())
        {
            _autoCompleteList.Items.Clear();
            foreach (var suggestion in suggestions.Take(8))
            {
                _autoCompleteList.Items.Add(suggestion);
            }
            
            _autoCompleteList.Visibility = Visibility.Visible;
        }
        else
        {
            HideAutoComplete();
        }
    }

    private void HideAutoComplete()
    {
        _autoCompleteList.Visibility = Visibility.Collapsed;
        _autoCompleteList.SelectedIndex = -1;
    }

    private List<string> GetSearchSuggestions(string searchText)
    {
        var suggestions = new List<string>();
        var searchLower = searchText.ToLower();

        // Get suggestions from item names
        var itemSuggestions = _eveItemDatabase
            .Where(item => item.ItemName.ToLower().Contains(searchLower))
            .Select(item => item.ItemName)
            .Distinct()
            .OrderBy(name => name.Length)
            .Take(5);

        suggestions.AddRange(itemSuggestions);

        // Get suggestions from groups
        var groupSuggestions = _eveItemDatabase
            .Where(item => item.Group.ToLower().Contains(searchLower))
            .Select(item => item.Group)
            .Distinct()
            .Take(3);

        suggestions.AddRange(groupSuggestions);

        return suggestions.Distinct().Take(8).ToList();
    }

    private void ClearSearch()
    {
        _searchBox.Text = "";
        SearchResults.Clear();
        SelectedItem = null;
        HideAutoComplete();
        UpdateStatusText("READY");
        UpdateItemDetails();
    }

    #endregion

    #region Item Details and Favorites

    private void UpdateItemDetails()
    {
        var detailsContent = _itemDetailsPanel.Child as StackPanel;
        if (detailsContent == null) return;

        // Clear existing details (keep title)
        while (detailsContent.Children.Count > 1)
        {
            detailsContent.Children.RemoveAt(1);
        }

        if (SelectedItem == null)
        {
            var noSelectionText = new TextBlock
            {
                Text = "Select an item to view details",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(150, 200, 200, 200)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            detailsContent.Children.Add(noSelectionText);
            return;
        }

        // Create detailed information display
        var detailsGrid = new Grid();
        detailsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
        detailsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var rowIndex = 0;

        AddDetailRow(detailsGrid, "Name:", SelectedItem.ItemName, rowIndex++, Colors.White);
        AddDetailRow(detailsGrid, "Category:", SelectedItem.Category, rowIndex++, Colors.Cyan);
        AddDetailRow(detailsGrid, "Group:", SelectedItem.Group, rowIndex++, Colors.Yellow);
        AddDetailRow(detailsGrid, "Tech Level:", SelectedItem.TechLevel, rowIndex++, Colors.Orange);
        AddDetailRow(detailsGrid, "Meta Level:", SelectedItem.MetaLevel.ToString(), rowIndex++, Colors.LimeGreen);
        
        if (!string.IsNullOrEmpty(SelectedItem.Description))
        {
            AddDetailRow(detailsGrid, "Description:", SelectedItem.Description, rowIndex++, Colors.LightGray, true);
        }

        detailsContent.Children.Add(detailsGrid);

        // Add action buttons
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 10, 0, 0)
        };

        var selectButton = CreateHoloButton("SELECT", () => ItemSelected?.Invoke(this, SelectedItem));
        var favoriteButton = CreateHoloButton(
            _favoriteItems.Any(f => f.ItemId == SelectedItem.ItemId) ? "UNFAV" : "FAVORITE",
            () => ToggleFavorite(SelectedItem)
        );

        buttonPanel.Children.Add(selectButton);
        buttonPanel.Children.Add(favoriteButton);

        detailsContent.Children.Add(buttonPanel);
    }

    private void AddDetailRow(Grid grid, string label, string value, int row, Color valueColor, bool isMultiLine = false)
    {
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var labelText = new TextBlock
        {
            Text = label,
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 150, 200, 255)),
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 2, 5, 2)
        };

        var valueText = new TextBlock
        {
            Text = value,
            FontSize = 10,
            Foreground = new SolidColorBrush(valueColor),
            TextWrapping = isMultiLine ? TextWrapping.Wrap : TextWrapping.NoWrap,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 2, 0, 2)
        };

        Grid.SetRow(labelText, row);
        Grid.SetColumn(labelText, 0);
        
        Grid.SetRow(valueText, row);
        Grid.SetColumn(valueText, 1);

        grid.Children.Add(labelText);
        grid.Children.Add(valueText);
    }

    private void ToggleFavorite(HoloEVEItem item)
    {
        var existingFavorite = _favoriteItems.FirstOrDefault(f => f.ItemId == item.ItemId);
        
        if (existingFavorite != null)
        {
            _favoriteItems.Remove(existingFavorite);
        }
        else
        {
            _favoriteItems.Add(item);
        }

        // Update favorite status in search results
        item.IsFavorite = existingFavorite == null;
        
        UpdateFavoritesDisplay();
        UpdateItemDetails();
        
        ItemFavorited?.Invoke(this, item);
    }

    private void UpdateFavoritesDisplay()
    {
        _favoritesPanel.Children.Clear();

        if (!_favoriteItems.Any())
        {
            var emptyText = new TextBlock
            {
                Text = "No favorites yet",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(150, 200, 200, 200)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };
            _favoritesPanel.Children.Add(emptyText);
            return;
        }

        foreach (var favorite in _favoriteItems.Take(10)) // Limit display
        {
            var favoriteCard = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(50, 255, 215, 0)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(150, 255, 215, 0)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Margin = new Thickness(0, 2, 0, 2),
                Padding = new Thickness(8, 4, 8, 4),
                Cursor = Cursors.Hand
            };

            var cardContent = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            var nameText = new TextBlock
            {
                Text = favorite.ItemName,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            var categoryText = new TextBlock
            {
                Text = favorite.Category,
                FontSize = 8,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 200, 200))
            };

            cardContent.Children.Add(nameText);
            cardContent.Children.Add(categoryText);
            favoriteCard.Child = cardContent;

            // Add click handler
            favoriteCard.MouseLeftButtonUp += (s, e) =>
            {
                SelectedItem = favorite;
                UpdateItemDetails();
                
                // Scroll to item in results if visible
                if (SearchResults.Contains(favorite))
                {
                    _resultsGrid.SelectedItem = favorite;
                    _resultsGrid.ScrollIntoView(favorite);
                }
            };

            _favoritesPanel.Children.Add(favoriteCard);
        }
    }

    #endregion

    #region Animation and Effects

    private void StartAnimation()
    {
        if (EnableAnimations && !_isAnimating)
        {
            _animationProgress = 0.0;
            _isAnimating = true;
        }
    }

    private void UpdateAnimatedElements()
    {
        // Animate search effects
        foreach (var effect in _searchEffects)
        {
            effect.Opacity = 0.3 + (0.7 * _animationProgress);
        }
    }

    private void CreateSearchStartParticles()
    {
        for (int i = 0; i < 5; i++)
        {
            var particle = new Ellipse
            {
                Width = 4,
                Height = 4,
                Fill = new SolidColorBrush(Color.FromArgb(200, 0, 255, 200))
            };

            var startX = _searchBox.TranslatePoint(new Point(_searchBox.ActualWidth / 2, 0), this).X;
            var startY = _searchBox.TranslatePoint(new Point(0, _searchBox.ActualHeight / 2), this).Y;

            Canvas.SetLeft(particle, startX + (_random.NextDouble() - 0.5) * 50);
            Canvas.SetTop(particle, startY + (_random.NextDouble() - 0.5) * 20);

            _particleCanvas.Children.Add(particle);
            _particles.Add(particle);

            AnimateSearchParticle(particle);
        }
    }

    private void CreateSearchParticles()
    {
        if (_particles.Count >= _maxParticles) return;

        var particle = new Ellipse
        {
            Width = 2 + _random.NextDouble() * 3,
            Height = 2 + _random.NextDouble() * 3,
            Fill = new SolidColorBrush(Color.FromArgb(150, 0, 200, 255))
        };

        var startX = _random.NextDouble() * ActualWidth;
        var startY = _random.NextDouble() * ActualHeight;

        Canvas.SetLeft(particle, startX);
        Canvas.SetTop(particle, startY);

        _particleCanvas.Children.Add(particle);
        _particles.Add(particle);

        AnimateFloatingParticle(particle);
    }

    private void AnimateSearchParticle(UIElement particle)
    {
        var duration = TimeSpan.FromSeconds(1.5);

        var targetX = _resultsGrid.TranslatePoint(new Point(_resultsGrid.ActualWidth / 2, 0), this).X;
        var targetY = _resultsGrid.TranslatePoint(new Point(0, _resultsGrid.ActualHeight / 2), this).Y;

        var xAnimation = new DoubleAnimation
        {
            From = Canvas.GetLeft(particle),
            To = targetX + (_random.NextDouble() - 0.5) * 100,
            Duration = duration,
            EasingFunction = new QuadraticEase()
        };

        var yAnimation = new DoubleAnimation
        {
            From = Canvas.GetTop(particle),
            To = targetY + (_random.NextDouble() - 0.5) * 50,
            Duration = duration,
            EasingFunction = new QuadraticEase()
        };

        var opacityAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.0,
            Duration = duration
        };

        particle.BeginAnimation(Canvas.LeftProperty, xAnimation);
        particle.BeginAnimation(Canvas.TopProperty, yAnimation);
        particle.BeginAnimation(OpacityProperty, opacityAnimation);
    }

    private void AnimateFloatingParticle(UIElement particle)
    {
        var duration = TimeSpan.FromSeconds(3 + _random.NextDouble() * 4);

        var xAnimation = new DoubleAnimation
        {
            From = Canvas.GetLeft(particle),
            To = Canvas.GetLeft(particle) + (_random.NextDouble() - 0.5) * 200,
            Duration = duration,
            EasingFunction = new SineEase()
        };

        var yAnimation = new DoubleAnimation
        {
            From = Canvas.GetTop(particle),
            To = Canvas.GetTop(particle) + (_random.NextDouble() - 0.5) * 150,
            Duration = duration,
            EasingFunction = new SineEase()
        };

        var opacityAnimation = new DoubleAnimation
        {
            From = 0.8,
            To = 0.0,
            Duration = duration
        };

        particle.BeginAnimation(Canvas.LeftProperty, xAnimation);
        particle.BeginAnimation(Canvas.TopProperty, yAnimation);
        particle.BeginAnimation(OpacityProperty, opacityAnimation);
    }

    private void UpdateParticleEffects()
    {
        var completedParticles = _particles
            .Where(p => p.Opacity <= 0.1)
            .ToList();

        foreach (var particle in completedParticles)
        {
            _particleCanvas.Children.Remove(particle);
            _particles.Remove(particle);
        }
    }

    #endregion

    #region EVE Item Database

    private void InitializeEVEItemDatabase()
    {
        _eveItemDatabase = new List<HoloEVEItem>();

        // Ships
        AddShipItems();
        
        // Modules
        AddModuleItems();
        
        // Ammunition
        AddAmmunitionItems();
        
        // Drones
        AddDroneItems();
        
        // Materials
        AddMaterialItems();
        
        // Other items
        AddMiscItems();
    }

    private void AddShipItems()
    {
        var ships = new[]
        {
            ("Rifter", "Frigates", "Minmatar Frigate", "T1"),
            ("Wolf", "Assault Frigates", "Minmatar Frigate", "T2"),
            ("Hurricane", "Battlecruisers", "Minmatar Battlecruiser", "T1"),
            ("Tempest", "Battleships", "Minmatar Battleship", "T1"),
            ("Ragnarok", "Titans", "Minmatar Titan", "T1"),
            ("Merlin", "Frigates", "Caldari Frigate", "T1"),
            ("Harpy", "Assault Frigates", "Caldari Frigate", "T2"),
            ("Drake", "Battlecruisers", "Caldari Battlecruiser", "T1"),
            ("Raven", "Battleships", "Caldari Battleship", "T1"),
            ("Leviathan", "Titans", "Caldari Titan", "T1")
        };

        var itemId = 1000;
        foreach (var (name, group, category, tech) in ships)
        {
            _eveItemDatabase.Add(new HoloEVEItem
            {
                ItemId = itemId++,
                ItemName = name,
                Category = "Ships",
                Group = group,
                TechLevel = tech,
                MetaLevel = tech == "T2" ? 5 : 0,
                Description = $"A {tech} {category.ToLower()} designed for combat operations."
            });
        }
    }

    private void AddModuleItems()
    {
        var modules = new[]
        {
            ("Small Pulse Laser I", "Energy Weapon", "T1"),
            ("Medium Pulse Laser II", "Energy Weapon", "T2"),
            ("Large Pulse Laser I", "Energy Weapon", "T1"),
            ("125mm Railgun I", "Projectile Weapon", "T1"),
            ("200mm AutoCannon II", "Projectile Weapon", "T2"),
            ("Shield Booster I", "Shield Booster", "T1"),
            ("Large Shield Booster II", "Shield Booster", "T2"),
            ("Armor Repairer I", "Armor Repairer", "T1"),
            ("Medium Armor Repairer II", "Armor Repairer", "T2"),
            ("Afterburner I", "Propulsion Module", "T1")
        };

        var itemId = 2000;
        foreach (var (name, group, tech) in modules)
        {
            _eveItemDatabase.Add(new HoloEVEItem
            {
                ItemId = itemId++,
                ItemName = name,
                Category = "Modules",
                Group = group,
                TechLevel = tech,
                MetaLevel = tech == "T2" ? 5 : 0,
                Description = $"A {tech} module for ship fitting."
            });
        }
    }

    private void AddAmmunitionItems()
    {
        var ammo = new[]
        {
            ("Multifrequency S", "Frequency Crystal"),
            ("Scorch S", "Frequency Crystal"),
            ("EMP S", "Projectile Ammo"),
            ("Republic Fleet EMP S", "Projectile Ammo"),
            ("Antimatter Charge S", "Hybrid Charge"),
            ("Null S", "Hybrid Charge"),
            ("Mjolnir Light Missile", "Light Missile"),
            ("Scourge Light Missile", "Light Missile")
        };

        var itemId = 3000;
        foreach (var (name, group) in ammo)
        {
            _eveItemDatabase.Add(new HoloEVEItem
            {
                ItemId = itemId++,
                ItemName = name,
                Category = "Ammunition",
                Group = group,
                TechLevel = "T1",
                MetaLevel = name.Contains("Fleet") ? 1 : 0,
                Description = "Ammunition for weapons systems."
            });
        }
    }

    private void AddDroneItems()
    {
        var drones = new[]
        {
            ("Warrior I", "Combat Drone"),
            ("Warrior II", "Combat Drone"),
            ("Hammerhead I", "Combat Drone"),
            ("Hammerhead II", "Combat Drone"),
            ("Ogre I", "Combat Drone"),
            ("Ogre II", "Combat Drone"),
            ("Hobgoblin I", "Combat Drone"),
            ("Hobgoblin II", "Combat Drone")
        };

        var itemId = 4000;
        foreach (var (name, group) in drones)
        {
            _eveItemDatabase.Add(new HoloEVEItem
            {
                ItemId = itemId++,
                ItemName = name,
                Category = "Drones",
                Group = group,
                TechLevel = name.Contains("II") ? "T2" : "T1",
                MetaLevel = name.Contains("II") ? 5 : 0,
                Description = "Autonomous combat drone."
            });
        }
    }

    private void AddMaterialItems()
    {
        var materials = new[]
        {
            ("Tritanium", "Mineral"),
            ("Pyerite", "Mineral"),
            ("Mexallon", "Mineral"),
            ("Isogen", "Mineral"),
            ("Nocxium", "Mineral"),
            ("Zydrine", "Mineral"),
            ("Megacyte", "Mineral"),
            ("Morphite", "Mineral")
        };

        var itemId = 5000;
        foreach (var (name, group) in materials)
        {
            _eveItemDatabase.Add(new HoloEVEItem
            {
                ItemId = itemId++,
                ItemName = name,
                Category = "Materials",
                Group = group,
                TechLevel = "T1",
                MetaLevel = 0,
                Description = "Basic mineral used in manufacturing."
            });
        }
    }

    private void AddMiscItems()
    {
        var misc = new[]
        {
            ("PLEX", "PLEX", "Commodities"),
            ("Skill Injector", "Skill Injector", "Implants"),
            ("Skill Extractor", "Skill Extractor", "Implants"),
            ("Neural Remap Certificate", "Booster", "Implants"),
            ("Implant", "Cybernetic Implant", "Implants")
        };

        var itemId = 6000;
        foreach (var (name, group, category) in misc)
        {
            _eveItemDatabase.Add(new HoloEVEItem
            {
                ItemId = itemId++,
                ItemName = name,
                Category = category,
                Group = group,
                TechLevel = "T1",
                MetaLevel = 0,
                Description = $"Special item: {name}"
            });
        }
    }

    private void BuildSearchSuggestions()
    {
        _searchSuggestions.Clear();

        foreach (var item in _eveItemDatabase)
        {
            var words = item.ItemName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                var key = word.ToLower();
                if (!_searchSuggestions.ContainsKey(key))
                {
                    _searchSuggestions[key] = new List<string>();
                }
                
                if (!_searchSuggestions[key].Contains(item.ItemName))
                {
                    _searchSuggestions[key].Add(item.ItemName);
                }
            }
        }
    }

    #endregion

    #region Utility Methods

    private void UpdateStatusText(string status)
    {
        var headerContent = ((Border)_mainGrid.Children[0]).Child as StackPanel;
        var statusText = headerContent?.Children[1] as TextBlock;
        if (statusText != null)
        {
            statusText.Text = status;
        }
    }

    #endregion

    #region Public Methods

    public ItemSearchAnalysis GetItemSearchAnalysis()
    {
        return new ItemSearchAnalysis
        {
            SearchText = SearchText,
            CategoryFilter = CategoryFilter,
            ResultCount = _searchResultCount,
            DatabaseItemCount = _eveItemDatabase?.Count ?? 0,
            FavoriteCount = _favoriteItems.Count,
            SelectedItem = SelectedItem,
            IsSearching = _isSearching,
            LastSearchTime = _lastSearchTime,
            PerformanceMode = _isInSimplifiedMode ? "Simplified" : "Full"
        };
    }

    public void SetSearchText(string searchText)
    {
        _searchBox.Text = searchText;
        PerformSearch();
    }

    public void SetCategoryFilter(EVEItemCategory category)
    {
        CategoryFilter = category;
        _categoryCombo.SelectedIndex = (int)category;
        PerformSearch();
    }

    public void SelectItem(HoloEVEItem item)
    {
        SelectedItem = item;
        _resultsGrid.SelectedItem = item;
        UpdateItemDetails();
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void SetSimplifiedMode(bool enabled)
    {
        _isInSimplifiedMode = enabled;
        EnableParticleEffects = !enabled;
        EnableAnimations = !enabled;
        _maxParticles = enabled ? 10 : 20;
        MaxResults = enabled ? 25 : 50;

        if (IsLoaded)
        {
            PerformSearch();
        }
    }

    public bool IsInSimplifiedMode => _isInSimplifiedMode;

    #endregion

    #region Property Change Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloItemSearch control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloItemSearch control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloItemSearch control)
        {
            control._lastSearchText = (string)e.NewValue;
        }
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloItemSearch control)
        {
            control.UpdateItemDetails();
        }
    }

    private static void OnSearchResultsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // No immediate action needed
    }

    private static void OnCategoryFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloItemSearch control && control.IsLoaded)
        {
            control.PerformSearch();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloItemSearch control)
        {
            if ((bool)e.NewValue)
            {
                control._animationTimer?.Start();
            }
            else
            {
                control._animationTimer?.Stop();
            }
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloItemSearch control)
        {
            if ((bool)e.NewValue)
            {
                control._particleTimer?.Start();
            }
            else
            {
                control._particleTimer?.Stop();
            }
        }
    }

    private static void OnShowItemDetailsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloItemSearch control)
        {
            control._itemDetailsPanel.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnShowFavoritesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloItemSearch control)
        {
            control._favoritesPanel.Parent.SetValue(UIElement.VisibilityProperty, 
                (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed);
        }
    }

    private static void OnAutoCompleteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloItemSearch control && !(bool)e.NewValue)
        {
            control.HideAutoComplete();
        }
    }

    private static void OnMaxResultsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloItemSearch control && control.IsLoaded)
        {
            control.PerformSearch();
        }
    }

    private void UpdateHolographicEffects()
    {
        Opacity = 0.8 + (0.2 * HolographicIntensity);
    }

    private void UpdateColorScheme()
    {
        // Update color scheme based on EVE colors
        if (IsLoaded)
        {
            // Refresh UI colors if needed
        }
    }

    #endregion
}

#region Supporting Classes

public class HoloEVEItem : INotifyPropertyChanged
{
    private bool _isFavorite;

    public int ItemId { get; set; }
    public string ItemName { get; set; }
    public string Category { get; set; }
    public string Group { get; set; }
    public string TechLevel { get; set; }
    public int MetaLevel { get; set; }
    public string Description { get; set; }

    public bool IsFavorite
    {
        get => _isFavorite;
        set
        {
            _isFavorite = value;
            OnPropertyChanged(nameof(IsFavorite));
            OnPropertyChanged(nameof(FavoriteColor));
        }
    }

    public Brush CategoryColor => Category switch
    {
        "Ships" => new SolidColorBrush(Colors.Gold),
        "Modules" => new SolidColorBrush(Colors.Cyan),
        "Ammunition" => new SolidColorBrush(Colors.Orange),
        "Drones" => new SolidColorBrush(Colors.LimeGreen),
        "Materials" => new SolidColorBrush(Colors.Silver),
        _ => new SolidColorBrush(Colors.White)
    };

    public Brush FavoriteColor => IsFavorite 
        ? new SolidColorBrush(Colors.Gold) 
        : new SolidColorBrush(Color.FromArgb(100, 200, 200, 200));

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public enum EVEItemCategory
{
    All,
    Ships,
    Modules,
    Ammunition,
    Drones,
    Implants,
    Blueprints,
    Materials,
    Commodities,
    Apparel
}

public class ItemSearchAnalysis
{
    public string SearchText { get; set; }
    public EVEItemCategory CategoryFilter { get; set; }
    public int ResultCount { get; set; }
    public int DatabaseItemCount { get; set; }
    public int FavoriteCount { get; set; }
    public HoloEVEItem SelectedItem { get; set; }
    public bool IsSearching { get; set; }
    public DateTime LastSearchTime { get; set; }
    public string PerformanceMode { get; set; }
}

#endregion