// ==========================================================================
// HoloSearch.cs - Holographic Search Interface with Particle Effects
// ==========================================================================
// Advanced search interface featuring holographic search visualization,
// real-time filtering, AI-powered suggestions, and particle-based feedback.
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
/// Holographic search interface with advanced filtering and AI-powered suggestions
/// </summary>
public class HoloSearch : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloSearch),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloSearch),
            new PropertyMetadata(EVEColorScheme.VoidPurple, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty SearchQueryProperty =
        DependencyProperty.Register(nameof(SearchQuery), typeof(string), typeof(HoloSearch),
            new PropertyMetadata("", OnSearchQueryChanged));

    public static readonly DependencyProperty SearchScopeProperty =
        DependencyProperty.Register(nameof(SearchScope), typeof(SearchScope), typeof(HoloSearch),
            new PropertyMetadata(SearchScope.All, OnSearchScopeChanged));

    public static readonly DependencyProperty EnableRealTimeSearchProperty =
        DependencyProperty.Register(nameof(EnableRealTimeSearch), typeof(bool), typeof(HoloSearch),
            new PropertyMetadata(true, OnEnableRealTimeSearchChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloSearch),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty EnableAISuggestionsProperty =
        DependencyProperty.Register(nameof(EnableAISuggestions), typeof(bool), typeof(HoloSearch),
            new PropertyMetadata(true, OnEnableAISuggestionsChanged));

    public static readonly DependencyProperty ShowAdvancedFiltersProperty =
        DependencyProperty.Register(nameof(ShowAdvancedFilters), typeof(bool), typeof(HoloSearch),
            new PropertyMetadata(false));

    public static readonly DependencyProperty SearchResultsProperty =
        DependencyProperty.Register(nameof(SearchResults), typeof(ObservableCollection<HoloSearchResult>), typeof(HoloSearch),
            new PropertyMetadata(null));

    public static readonly DependencyProperty SuggestionsProperty =
        DependencyProperty.Register(nameof(Suggestions), typeof(ObservableCollection<string>), typeof(HoloSearch),
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

    public string SearchQuery
    {
        get => (string)GetValue(SearchQueryProperty);
        set => SetValue(SearchQueryProperty, value);
    }

    public SearchScope SearchScope
    {
        get => (SearchScope)GetValue(SearchScopeProperty);
        set => SetValue(SearchScopeProperty, value);
    }

    public bool EnableRealTimeSearch
    {
        get => (bool)GetValue(EnableRealTimeSearchProperty);
        set => SetValue(EnableRealTimeSearchProperty, value);
    }

    public bool EnableParticleEffects
    {
        get => (bool)GetValue(EnableParticleEffectsProperty);
        set => SetValue(EnableParticleEffectsProperty, value);
    }

    public bool EnableAISuggestions
    {
        get => (bool)GetValue(EnableAISuggestionsProperty);
        set => SetValue(EnableAISuggestionsProperty, value);
    }

    public bool ShowAdvancedFilters
    {
        get => (bool)GetValue(ShowAdvancedFiltersProperty);
        set => SetValue(ShowAdvancedFiltersProperty, value);
    }

    public ObservableCollection<HoloSearchResult> SearchResults
    {
        get => (ObservableCollection<HoloSearchResult>)GetValue(SearchResultsProperty);
        set => SetValue(SearchResultsProperty, value);
    }

    public ObservableCollection<string> Suggestions
    {
        get => (ObservableCollection<string>)GetValue(SuggestionsProperty);
        set => SetValue(SuggestionsProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloSearchEventArgs> SearchExecuted;
    public event EventHandler<HoloSearchEventArgs> ResultSelected;
    public event EventHandler<HoloSearchEventArgs> FilterChanged;
    public event EventHandler<HoloSearchEventArgs> SuggestionSelected;

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode { get; private set; }

    public void SwitchToFullMode()
    {
        IsInSimplifiedMode = false;
        EnableParticleEffects = true;
        EnableAISuggestions = true;
        EnableRealTimeSearch = true;
        UpdateSearchAppearance();
    }

    public void SwitchToSimplifiedMode()
    {
        IsInSimplifiedMode = true;
        EnableParticleEffects = false;
        EnableAISuggestions = false;
        EnableRealTimeSearch = false;
        UpdateSearchAppearance();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => !_disposed && IsLoaded;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        EnableParticleEffects = settings.EnabledFeatures.HasFlag(AnimationFeatures.ParticleEffects);
        
        UpdateSearchAppearance();
    }

    #endregion

    #region Fields

    private Grid _mainGrid;
    private Border _searchBorder;
    private TextBox _searchBox;
    private ComboBox _scopeSelector;
    private Button _searchButton;
    private Button _filtersToggle;
    private StackPanel _filtersPanel;
    private ListBox _suggestionsList;
    private ItemsControl _resultsList;
    private Canvas _particleCanvas;
    private ProgressBar _searchProgress;
    private TextBlock _resultsCount;
    private TextBlock _searchTime;
    
    private readonly Dictionary<string, object> _searchFilters = new();
    private readonly List<SearchParticle> _particles = new();
    private readonly List<string> _searchHistory = new();
    private DispatcherTimer _searchTimer;
    private DispatcherTimer _particleTimer;
    private DispatcherTimer _suggestionTimer;
    private double _particlePhase = 0;
    private readonly Random _random = new();
    private bool _disposed = false;
    private bool _isSearching = false;
    private DateTime _searchStartTime;

    #endregion

    #region Constructor

    public HoloSearch()
    {
        InitializeSearch();
        SetupAnimations();
        
        // Register with animation intensity manager
        AnimationIntensityManager.Instance.RegisterTarget(this);
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        
        // Initialize collections
        SearchResults = new ObservableCollection<HoloSearchResult>();
        Suggestions = new ObservableCollection<string>();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Execute search with current query and filters
    /// </summary>
    public async Task<List<HoloSearchResult>> ExecuteSearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery) || _isSearching)
            return new List<HoloSearchResult>();

        _isSearching = true;
        _searchStartTime = DateTime.Now;
        
        if (_searchProgress != null)
            _searchProgress.Visibility = Visibility.Visible;

        if (EnableParticleEffects && !IsInSimplifiedMode)
        {
            SpawnSearchParticles();
        }

        try
        {
            var results = await PerformSearchAsync(SearchQuery, SearchScope, _searchFilters);
            
            SearchResults.Clear();
            foreach (var result in results)
            {
                SearchResults.Add(result);
            }

            UpdateSearchStats(results.Count);
            AddToSearchHistory(SearchQuery);

            SearchExecuted?.Invoke(this, new HoloSearchEventArgs 
            { 
                Query = SearchQuery,
                Scope = SearchScope,
                ResultCount = results.Count,
                SearchTime = DateTime.Now - _searchStartTime,
                Timestamp = DateTime.Now
            });

            return results;
        }
        finally
        {
            _isSearching = false;
            if (_searchProgress != null)
                _searchProgress.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Clear search results and reset interface
    /// </summary>
    public void ClearSearch()
    {
        SearchQuery = "";
        SearchResults.Clear();
        Suggestions.Clear();
        _searchFilters.Clear();
        UpdateSearchStats(0);
        
        if (EnableParticleEffects && !IsInSimplifiedMode)
        {
            SpawnClearParticles();
        }
    }

    /// <summary>
    /// Add search filter
    /// </summary>
    public void AddFilter(string key, object value)
    {
        _searchFilters[key] = value;
        
        if (EnableRealTimeSearch && !string.IsNullOrWhiteSpace(SearchQuery))
        {
            _ = ExecuteSearchAsync();
        }

        FilterChanged?.Invoke(this, new HoloSearchEventArgs 
        { 
            FilterKey = key,
            FilterValue = value,
            Timestamp = DateTime.Now
        });
    }

    /// <summary>
    /// Remove search filter
    /// </summary>
    public void RemoveFilter(string key)
    {
        _searchFilters.Remove(key);
        
        if (EnableRealTimeSearch && !string.IsNullOrWhiteSpace(SearchQuery))
        {
            _ = ExecuteSearchAsync();
        }
    }

    /// <summary>
    /// Get search suggestions for current query
    /// </summary>
    public async Task<List<string>> GetSuggestionsAsync(string query)
    {
        if (!EnableAISuggestions || string.IsNullOrWhiteSpace(query))
            return new List<string>();

        // Simulate AI suggestion generation
        await Task.Delay(200);
        
        var suggestions = GenerateSearchSuggestions(query);
        
        Suggestions.Clear();
        foreach (var suggestion in suggestions)
        {
            Suggestions.Add(suggestion);
        }

        return suggestions;
    }

    /// <summary>
    /// Focus search input
    /// </summary>
    public void FocusSearch()
    {
        _searchBox?.Focus();
    }

    #endregion

    #region Private Methods

    private void InitializeSearch()
    {
        CreateSearchInterface();
        UpdateSearchAppearance();
    }

    private void CreateSearchInterface()
    {
        // Main grid layout
        _mainGrid = new Grid();
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        // Search input section
        CreateSearchInputSection();
        
        // Filters section
        CreateFiltersSection();
        
        // Results section
        CreateResultsSection();

        Content = _mainGrid;
    }

    private void CreateSearchInputSection()
    {
        _searchBorder = new Border
        {
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(2),
            Margin = new Thickness(10, 10, 10, 5),
            Padding = new Thickness(15, 10),
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(80, 138, 43, 226), 0.0),
                    new GradientStop(Color.FromArgb(40, 138, 43, 226), 1.0)
                }
            }
        };

        var searchGrid = new Grid();
        searchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        searchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        searchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        searchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        searchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Search icon
        var searchIcon = new TextBlock
        {
            Text = "🔍",
            FontSize = 16,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };
        Grid.SetColumn(searchIcon, 0);

        // Search input
        var searchContainer = new Grid();
        searchContainer.RowDefinitions.Add(new RowDefinition());
        searchContainer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        _searchBox = new TextBox
        {
            FontSize = 14,
            Height = 32,
            VerticalContentAlignment = VerticalAlignment.Center,
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 138, 43, 226))
        };
        _searchBox.TextChanged += OnSearchTextChanged;
        _searchBox.KeyDown += OnSearchKeyDown;
        Grid.SetRow(_searchBox, 0);

        // Suggestions popup
        _suggestionsList = new ListBox
        {
            Background = new SolidColorBrush(Color.FromArgb(240, 20, 20, 40)),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromArgb(180, 138, 43, 226)),
            MaxHeight = 150,
            Visibility = Visibility.Collapsed
        };
        _suggestionsList.SelectionChanged += OnSuggestionSelected;
        Grid.SetRow(_suggestionsList, 1);

        searchContainer.Children.Add(_searchBox);
        searchContainer.Children.Add(_suggestionsList);
        Grid.SetColumn(searchContainer, 1);

        // Scope selector
        _scopeSelector = new ComboBox
        {
            Width = 120,
            Height = 30,
            Margin = new Thickness(10, 0, 5, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        PopulateScopeSelector();
        _scopeSelector.SelectionChanged += OnScopeSelectionChanged;
        Grid.SetColumn(_scopeSelector, 2);

        // Search button
        _searchButton = new Button
        {
            Content = "Search",
            Width = 80,
            Height = 30,
            Margin = new Thickness(5, 0),
            Style = CreateHoloButtonStyle()
        };
        _searchButton.Click += OnSearchButtonClick;
        Grid.SetColumn(_searchButton, 3);

        // Filters toggle
        _filtersToggle = new Button
        {
            Content = "Filters",
            Width = 70,
            Height = 30,
            Margin = new Thickness(5, 0, 0, 0),
            Style = CreateHoloButtonStyle()
        };
        _filtersToggle.Click += OnFiltersToggleClick;
        Grid.SetColumn(_filtersToggle, 4);

        searchGrid.Children.Add(searchIcon);
        searchGrid.Children.Add(searchContainer);
        searchGrid.Children.Add(_scopeSelector);
        searchGrid.Children.Add(_searchButton);
        searchGrid.Children.Add(_filtersToggle);

        _searchBorder.Child = searchGrid;
        Grid.SetRow(_searchBorder, 0);
        _mainGrid.Children.Add(_searchBorder);
    }

    private void CreateFiltersSection()
    {
        _filtersPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10, 5),
            Visibility = Visibility.Collapsed
        };

        CreateCommonFilters();
        
        Grid.SetRow(_filtersPanel, 1);
        _mainGrid.Children.Add(_filtersPanel);
    }

    private void CreateResultsSection()
    {
        var resultsBorder = new Border
        {
            CornerRadius = new CornerRadius(12),
            BorderThickness = new Thickness(2),
            Margin = new Thickness(10, 5, 10, 10),
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

        var resultsGrid = new Grid();
        resultsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        resultsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        resultsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        // Results header
        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(15, 10, 15, 5)
        };

        var resultsHeader = new TextBlock
        {
            Text = "Search Results",
            Foreground = Brushes.White,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center
        };

        _resultsCount = new TextBlock
        {
            Foreground = new SolidColorBrush(Color.FromRgb(138, 43, 226)),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0)
        };

        _searchTime = new TextBlock
        {
            Foreground = Brushes.LightGray,
            FontSize = 10,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0)
        };

        headerPanel.Children.Add(resultsHeader);
        headerPanel.Children.Add(_resultsCount);
        headerPanel.Children.Add(_searchTime);
        Grid.SetRow(headerPanel, 0);

        // Search progress
        _searchProgress = new ProgressBar
        {
            Height = 4,
            Margin = new Thickness(15, 0, 15, 5),
            IsIndeterminate = true,
            Visibility = Visibility.Collapsed,
            Background = new SolidColorBrush(Color.FromArgb(60, 0, 0, 0)),
            Foreground = new SolidColorBrush(Color.FromRgb(138, 43, 226))
        };
        Grid.SetRow(_searchProgress, 1);

        // Results list
        _resultsList = new ItemsControl
        {
            Margin = new Thickness(15, 5, 15, 15),
            ItemTemplate = CreateResultItemTemplate()
        };

        var resultsScrollViewer = new ScrollViewer
        {
            Content = _resultsList,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
        };
        Grid.SetRow(resultsScrollViewer, 2);

        // Particle canvas overlay
        _particleCanvas = new Canvas
        {
            IsHitTestVisible = false,
            ClipToBounds = false
        };
        Grid.SetRowSpan(_particleCanvas, 3);

        resultsGrid.Children.Add(headerPanel);
        resultsGrid.Children.Add(_searchProgress);
        resultsGrid.Children.Add(resultsScrollViewer);
        resultsGrid.Children.Add(_particleCanvas);

        resultsBorder.Child = resultsGrid;
        Grid.SetRow(resultsBorder, 2);
        _mainGrid.Children.Add(resultsBorder);
    }

    private void CreateCommonFilters()
    {
        // Type filter
        var typeFilter = new ComboBox
        {
            Width = 100,
            Height = 25,
            Margin = new Thickness(5)
        };
        typeFilter.Items.Add("All Types");
        typeFilter.Items.Add("Ships");
        typeFilter.Items.Add("Modules");
        typeFilter.Items.Add("Items");
        typeFilter.Items.Add("Skills");
        typeFilter.SelectedIndex = 0;
        typeFilter.SelectionChanged += (s, e) => AddFilter("Type", typeFilter.SelectedItem?.ToString());

        // Quality filter
        var qualityFilter = new ComboBox
        {
            Width = 100,
            Height = 25,
            Margin = new Thickness(5)
        };
        qualityFilter.Items.Add("All Quality");
        qualityFilter.Items.Add("Tech I");
        qualityFilter.Items.Add("Tech II");
        qualityFilter.Items.Add("Tech III");
        qualityFilter.Items.Add("Officer");
        qualityFilter.SelectedIndex = 0;
        qualityFilter.SelectionChanged += (s, e) => AddFilter("Quality", qualityFilter.SelectedItem?.ToString());

        // Price range filter
        var pricePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(5)
        };

        var priceLabel = new TextBlock
        {
            Text = "Price:",
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 5, 0)
        };

        var minPriceBox = new TextBox
        {
            Width = 80,
            Height = 25,
            Margin = new Thickness(2),
            Text = "Min"
        };
        minPriceBox.GotFocus += (s, e) => { if (minPriceBox.Text == "Min") minPriceBox.Text = ""; };
        minPriceBox.TextChanged += (s, e) => AddFilter("MinPrice", minPriceBox.Text);

        var maxPriceBox = new TextBox
        {
            Width = 80,
            Height = 25,
            Margin = new Thickness(2),
            Text = "Max"
        };
        maxPriceBox.GotFocus += (s, e) => { if (maxPriceBox.Text == "Max") maxPriceBox.Text = ""; };
        maxPriceBox.TextChanged += (s, e) => AddFilter("MaxPrice", maxPriceBox.Text);

        pricePanel.Children.Add(priceLabel);
        pricePanel.Children.Add(minPriceBox);
        pricePanel.Children.Add(maxPriceBox);

        _filtersPanel.Children.Add(typeFilter);
        _filtersPanel.Children.Add(qualityFilter);
        _filtersPanel.Children.Add(pricePanel);
    }

    private void PopulateScopeSelector()
    {
        var scopes = Enum.GetValues<SearchScope>();
        foreach (var scope in scopes)
        {
            _scopeSelector.Items.Add(new ComboBoxItem 
            { 
                Content = GetScopeDisplayName(scope),
                Tag = scope
            });
        }
        _scopeSelector.SelectedIndex = 0;
    }

    private DataTemplate CreateResultItemTemplate()
    {
        var template = new DataTemplate();
        
        var border = new FrameworkElementFactory(typeof(Border));
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
        border.SetValue(Border.MarginProperty, new Thickness(0, 2));
        border.SetValue(Border.PaddingProperty, new Thickness(10, 8));
        border.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromArgb(60, 138, 43, 226)));
        border.SetValue(Border.CursorProperty, Cursors.Hand);
        
        var grid = new FrameworkElementFactory(typeof(Grid));
        grid.SetValue(Grid.ColumnDefinitionsProperty, new ColumnDefinitionCollection
        {
            new ColumnDefinition { Width = GridLength.Auto },
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            new ColumnDefinition { Width = GridLength.Auto }
        });
        
        // Icon
        var iconText = new FrameworkElementFactory(typeof(TextBlock));
        iconText.SetBinding(TextBlock.TextProperty, new Binding("Icon"));
        iconText.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Color.FromRgb(138, 43, 226)));
        iconText.SetValue(TextBlock.FontSizeProperty, 16.0);
        iconText.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
        iconText.SetValue(TextBlock.MarginProperty, new Thickness(0, 0, 10, 0));
        iconText.SetValue(Grid.ColumnProperty, 0);
        
        // Content
        var contentStack = new FrameworkElementFactory(typeof(StackPanel));
        contentStack.SetValue(Grid.ColumnProperty, 1);
        
        var titleText = new FrameworkElementFactory(typeof(TextBlock));
        titleText.SetBinding(TextBlock.TextProperty, new Binding("Title"));
        titleText.SetValue(TextBlock.ForegroundProperty, Brushes.White);
        titleText.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        titleText.SetValue(TextBlock.FontSizeProperty, 12.0);
        
        var descriptionText = new FrameworkElementFactory(typeof(TextBlock));
        descriptionText.SetBinding(TextBlock.TextProperty, new Binding("Description"));
        descriptionText.SetValue(TextBlock.ForegroundProperty, Brushes.LightGray);
        descriptionText.SetValue(TextBlock.FontSizeProperty, 10.0);
        descriptionText.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);
        
        var categoryText = new FrameworkElementFactory(typeof(TextBlock));
        categoryText.SetBinding(TextBlock.TextProperty, new Binding("Category"));
        categoryText.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Color.FromRgb(138, 43, 226)));
        categoryText.SetValue(TextBlock.FontSizeProperty, 9.0);
        categoryText.SetValue(TextBlock.FontStyleProperty, FontStyles.Italic);
        
        contentStack.AppendChild(titleText);
        contentStack.AppendChild(descriptionText);
        contentStack.AppendChild(categoryText);
        
        // Relevance score
        var scoreText = new FrameworkElementFactory(typeof(TextBlock));
        scoreText.SetBinding(TextBlock.TextProperty, new Binding("RelevanceScore") { StringFormat = "{0:F1}%" });
        scoreText.SetValue(TextBlock.ForegroundProperty, Brushes.Yellow);
        scoreText.SetValue(TextBlock.FontSizeProperty, 10.0);
        scoreText.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        scoreText.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
        scoreText.SetValue(Grid.ColumnProperty, 2);
        
        grid.AppendChild(iconText);
        grid.AppendChild(contentStack);
        grid.AppendChild(scoreText);
        border.AppendChild(grid);
        
        template.VisualTree = border;
        return template;
    }

    private Style CreateHoloButtonStyle()
    {
        var style = new Style(typeof(Button));
        
        style.Setters.Add(new Setter(Button.BackgroundProperty, 
            new SolidColorBrush(Color.FromArgb(100, 138, 43, 226))));
        style.Setters.Add(new Setter(Button.ForegroundProperty, Brushes.White));
        style.Setters.Add(new Setter(Button.BorderBrushProperty, 
            new SolidColorBrush(Color.FromArgb(180, 138, 43, 226))));
        style.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Button.FontWeightProperty, FontWeights.Bold));
        
        return style;
    }

    private void SetupAnimations()
    {
        // Search delay timer
        _searchTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _searchTimer.Tick += OnSearchTimerTick;

        // Particle animation timer
        _particleTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50) // 20 FPS
        };
        _particleTimer.Tick += OnParticleTick;

        // Suggestion delay timer
        _suggestionTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(300)
        };
        _suggestionTimer.Tick += OnSuggestionTimerTick;
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        if (!EnableParticleEffects || IsInSimplifiedMode || _particleCanvas == null) return;

        _particlePhase += 0.1;
        if (_particlePhase > Math.PI * 2)
            _particlePhase = 0;

        UpdateParticleEffects();
    }

    private void OnSearchTimerTick(object sender, EventArgs e)
    {
        _searchTimer.Stop();
        _ = ExecuteSearchAsync();
    }

    private void OnSuggestionTimerTick(object sender, EventArgs e)
    {
        _suggestionTimer.Stop();
        _ = UpdateSuggestionsAsync();
    }

    private void UpdateParticleEffects()
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var particle = _particles[i];
            
            particle.X += particle.VelocityX;
            particle.Y += particle.VelocityY;
            particle.Life -= 0.02;
            particle.VelocityY += 0.05; // Gravity effect

            Canvas.SetLeft(particle.Visual, particle.X);
            Canvas.SetTop(particle.Visual, particle.Y);
            particle.Visual.Opacity = Math.Max(0, particle.Life) * HolographicIntensity;

            if (particle.Life <= 0 || particle.Y > _particleCanvas.ActualHeight + 10)
            {
                _particleCanvas.Children.Remove(particle.Visual);
                _particles.RemoveAt(i);
            }
        }
    }

    private async Task<List<HoloSearchResult>> PerformSearchAsync(string query, SearchScope scope, Dictionary<string, object> filters)
    {
        // Simulate search processing
        await Task.Delay(800);
        
        var results = new List<HoloSearchResult>();
        
        // Generate mock search results
        var categories = new[] { "Ships", "Modules", "Skills", "Items", "Market Data" };
        var icons = new[] { "🚀", "⚙️", "📚", "📦", "💰" };
        
        for (int i = 0; i < 15; i++)
        {
            var categoryIndex = _random.Next(categories.Length);
            results.Add(new HoloSearchResult
            {
                Id = Guid.NewGuid().ToString(),
                Title = $"{query} Result {i + 1}",
                Description = $"This is a sample search result for '{query}' in the {categories[categoryIndex]} category.",
                Category = categories[categoryIndex],
                Icon = icons[categoryIndex],
                RelevanceScore = 100 - (_random.NextDouble() * 40),
                Type = scope.ToString(),
                Data = new { Index = i, Query = query }
            });
        }
        
        // Sort by relevance
        return results.OrderByDescending(r => r.RelevanceScore).ToList();
    }

    private List<string> GenerateSearchSuggestions(string query)
    {
        var suggestions = new List<string>();
        
        // Generate contextual suggestions based on query
        if (query.Length >= 2)
        {
            var baseSuggestions = new[]
            {
                $"{query} fitting",
                $"{query} price",
                $"{query} skills",
                $"{query} build",
                $"best {query}",
                $"{query} guide",
                $"{query} comparison"
            };
            
            suggestions.AddRange(baseSuggestions.Take(5));
        }
        
        // Add popular searches
        var popularSearches = new[]
        {
            "Raven ship fitting",
            "Market analysis tools",
            "Skill training guide",
            "PvP ship builds",
            "Industry calculations"
        };
        
        suggestions.AddRange(popularSearches.Where(s => 
            s.Contains(query, StringComparison.OrdinalIgnoreCase)).Take(3));
        
        return suggestions.Distinct().Take(8).ToList();
    }

    private void SpawnSearchParticles()
    {
        for (int i = 0; i < 10; i++)
        {
            var particle = CreateSearchParticle();
            _particles.Add(particle);
            _particleCanvas.Children.Add(particle.Visual);
        }
    }

    private void SpawnClearParticles()
    {
        for (int i = 0; i < 6; i++)
        {
            var particle = CreateSearchParticle();
            _particles.Add(particle);
            _particleCanvas.Children.Add(particle.Visual);
        }
    }

    private SearchParticle CreateSearchParticle()
    {
        var color = GetEVEColor(EVEColorScheme);
        var size = 2 + _random.NextDouble() * 2;
        
        var ellipse = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromArgb(150, color.R, color.G, color.B))
        };

        var particle = new SearchParticle
        {
            Visual = ellipse,
            X = _random.NextDouble() * _particleCanvas.ActualWidth,
            Y = _particleCanvas.ActualHeight,
            VelocityX = (_random.NextDouble() - 0.5) * 4,
            VelocityY = -2 - _random.NextDouble() * 4,
            Life = 1.0
        };

        Canvas.SetLeft(ellipse, particle.X);
        Canvas.SetTop(ellipse, particle.Y);

        return particle;
    }

    private async Task UpdateSuggestionsAsync()
    {
        if (!EnableAISuggestions || string.IsNullOrWhiteSpace(_searchBox.Text)) 
        {
            _suggestionsList.Visibility = Visibility.Collapsed;
            return;
        }

        var suggestions = await GetSuggestionsAsync(_searchBox.Text);
        
        if (suggestions.Count > 0)
        {
            _suggestionsList.ItemsSource = suggestions;
            _suggestionsList.Visibility = Visibility.Visible;
        }
        else
        {
            _suggestionsList.Visibility = Visibility.Collapsed;
        }
    }

    private void UpdateSearchStats(int resultCount)
    {
        if (_resultsCount != null)
        {
            _resultsCount.Text = $"({resultCount} results)";
        }

        if (_searchTime != null)
        {
            var searchDuration = DateTime.Now - _searchStartTime;
            _searchTime.Text = $"in {searchDuration.TotalMilliseconds:F0}ms";
        }
    }

    private void AddToSearchHistory(string query)
    {
        if (!_searchHistory.Contains(query))
        {
            _searchHistory.Insert(0, query);
            if (_searchHistory.Count > 20)
            {
                _searchHistory.RemoveAt(_searchHistory.Count - 1);
            }
        }
    }

    private string GetScopeDisplayName(SearchScope scope)
    {
        return scope switch
        {
            SearchScope.All => "All",
            SearchScope.Ships => "Ships",
            SearchScope.Modules => "Modules",
            SearchScope.Skills => "Skills",
            SearchScope.Market => "Market",
            SearchScope.Documentation => "Help",
            _ => scope.ToString()
        };
    }

    private void UpdateSearchAppearance()
    {
        if (_resultsList != null && SearchResults != null)
        {
            _resultsList.ItemsSource = SearchResults;
        }

        UpdateColors();
        UpdateEffects();
    }

    private void UpdateColors()
    {
        var color = GetEVEColor(EVEColorScheme);

        // Update border colors
        if (_searchBorder != null)
        {
            _searchBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(180, color.R, color.G, color.B));
        }

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
            _ => Color.FromRgb(138, 43, 226)
        };
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        SearchQuery = _searchBox.Text;
        
        if (EnableRealTimeSearch && !string.IsNullOrWhiteSpace(SearchQuery))
        {
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        if (EnableAISuggestions)
        {
            _suggestionTimer.Stop();
            _suggestionTimer.Start();
        }
    }

    private async void OnSearchKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _suggestionsList.Visibility = Visibility.Collapsed;
            await ExecuteSearchAsync();
        }
        else if (e.Key == Key.Escape)
        {
            _suggestionsList.Visibility = Visibility.Collapsed;
        }
        else if (e.Key == Key.Down && _suggestionsList.Visibility == Visibility.Visible)
        {
            _suggestionsList.Focus();
            if (_suggestionsList.Items.Count > 0)
            {
                _suggestionsList.SelectedIndex = 0;
            }
        }
    }

    private void OnScopeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_scopeSelector.SelectedItem is ComboBoxItem item)
        {
            SearchScope = (SearchScope)item.Tag;
        }
    }

    private async void OnSearchButtonClick(object sender, RoutedEventArgs e)
    {
        _suggestionsList.Visibility = Visibility.Collapsed;
        await ExecuteSearchAsync();
    }

    private void OnFiltersToggleClick(object sender, RoutedEventArgs e)
    {
        ShowAdvancedFilters = !ShowAdvancedFilters;
        _filtersPanel.Visibility = ShowAdvancedFilters ? Visibility.Visible : Visibility.Collapsed;
        _filtersToggle.Content = ShowAdvancedFilters ? "Hide Filters" : "Show Filters";
    }

    private void OnSuggestionSelected(object sender, SelectionChangedEventArgs e)
    {
        if (_suggestionsList.SelectedItem is string suggestion)
        {
            SearchQuery = suggestion;
            _searchBox.Text = suggestion;
            _suggestionsList.Visibility = Visibility.Collapsed;
            
            SuggestionSelected?.Invoke(this, new HoloSearchEventArgs 
            { 
                Query = suggestion,
                Timestamp = DateTime.Now
            });
            
            _ = ExecuteSearchAsync();
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableParticleEffects && !IsInSimplifiedMode)
            _particleTimer.Start();

        FocusSearch();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _searchTimer?.Stop();
        _particleTimer?.Stop();
        _suggestionTimer?.Stop();
        
        // Clean up particles
        _particles.Clear();
        _particleCanvas?.Children.Clear();
        
        // Unregister from animation intensity manager
        AnimationIntensityManager.Instance.UnregisterTarget(this);
        
        _disposed = true;
    }

    #endregion

    #region Event Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSearch search)
            search.UpdateSearchAppearance();
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSearch search)
            search.UpdateSearchAppearance();
    }

    private static void OnSearchQueryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSearch search && search._searchBox != null)
        {
            search._searchBox.Text = (string)e.NewValue;
        }
    }

    private static void OnSearchScopeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Handle scope changes
    }

    private static void OnEnableRealTimeSearchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Handle real-time search enabling/disabling
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSearch search)
        {
            if ((bool)e.NewValue && !search.IsInSimplifiedMode)
                search._particleTimer.Start();
            else
                search._particleTimer.Stop();
        }
    }

    private static void OnEnableAISuggestionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSearch search && !(bool)e.NewValue)
        {
            search._suggestionsList.Visibility = Visibility.Collapsed;
        }
    }

    #endregion
}

/// <summary>
/// Search particle for visual effects
/// </summary>
internal class SearchParticle
{
    public FrameworkElement Visual { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
}

/// <summary>
/// Search result data model
/// </summary>
public class HoloSearchResult
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public string Icon { get; set; }
    public string Type { get; set; }
    public double RelevanceScore { get; set; }
    public object Data { get; set; }
}

/// <summary>
/// Search scopes
/// </summary>
public enum SearchScope
{
    All,
    Ships,
    Modules,
    Skills,
    Market,
    Documentation
}

/// <summary>
/// Event args for search events
/// </summary>
public class HoloSearchEventArgs : EventArgs
{
    public string Query { get; set; }
    public SearchScope Scope { get; set; }
    public int ResultCount { get; set; }
    public TimeSpan SearchTime { get; set; }
    public HoloSearchResult SelectedResult { get; set; }
    public string FilterKey { get; set; }
    public object FilterValue { get; set; }
    public DateTime Timestamp { get; set; }
}