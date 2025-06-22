// ==========================================================================
// HoloModuleBrowser.cs - Holographic Module Browser with Animated Search
// ==========================================================================
// Advanced module browser featuring holographic module cards, animated search
// results, EVE-style filtering, and intelligent module recommendations.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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
/// Holographic module browser with animated search and intelligent filtering
/// </summary>
public class HoloModuleBrowser : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloModuleBrowser),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloModuleBrowser),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty ModulesProperty =
        DependencyProperty.Register(nameof(Modules), typeof(ObservableCollection<HoloModuleData>), typeof(HoloModuleBrowser),
            new PropertyMetadata(null, OnModulesChanged));

    public static readonly DependencyProperty SearchTextProperty =
        DependencyProperty.Register(nameof(SearchText), typeof(string), typeof(HoloModuleBrowser),
            new PropertyMetadata(string.Empty, OnSearchTextChanged));

    public static readonly DependencyProperty SelectedModuleProperty =
        DependencyProperty.Register(nameof(SelectedModule), typeof(HoloModuleData), typeof(HoloModuleBrowser),
            new PropertyMetadata(null, OnSelectedModuleChanged));

    public static readonly DependencyProperty FilterCriteriaProperty =
        DependencyProperty.Register(nameof(FilterCriteria), typeof(ModuleFilterCriteria), typeof(HoloModuleBrowser),
            new PropertyMetadata(null, OnFilterCriteriaChanged));

    public static readonly DependencyProperty ViewModeProperty =
        DependencyProperty.Register(nameof(ViewMode), typeof(ModuleBrowserViewMode), typeof(HoloModuleBrowser),
            new PropertyMetadata(ModuleBrowserViewMode.Grid, OnViewModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloModuleBrowser),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloModuleBrowser),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowRecommendationsProperty =
        DependencyProperty.Register(nameof(ShowRecommendations), typeof(bool), typeof(HoloModuleBrowser),
            new PropertyMetadata(true, OnShowRecommendationsChanged));

    public static readonly DependencyProperty AutoCompleteSearchProperty =
        DependencyProperty.Register(nameof(AutoCompleteSearch), typeof(bool), typeof(HoloModuleBrowser),
            new PropertyMetadata(true, OnAutoCompleteSearchChanged));

    public static readonly DependencyProperty SearchDelayProperty =
        DependencyProperty.Register(nameof(SearchDelay), typeof(TimeSpan), typeof(HoloModuleBrowser),
            new PropertyMetadata(TimeSpan.FromMilliseconds(300), OnSearchDelayChanged));

    public static readonly DependencyProperty MaxSearchResultsProperty =
        DependencyProperty.Register(nameof(MaxSearchResults), typeof(int), typeof(HoloModuleBrowser),
            new PropertyMetadata(100, OnMaxSearchResultsChanged));

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

    public ObservableCollection<HoloModuleData> Modules
    {
        get => (ObservableCollection<HoloModuleData>)GetValue(ModulesProperty);
        set => SetValue(ModulesProperty, value);
    }

    public string SearchText
    {
        get => (string)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public HoloModuleData SelectedModule
    {
        get => (HoloModuleData)GetValue(SelectedModuleProperty);
        set => SetValue(SelectedModuleProperty, value);
    }

    public ModuleFilterCriteria FilterCriteria
    {
        get => (ModuleFilterCriteria)GetValue(FilterCriteriaProperty);
        set => SetValue(FilterCriteriaProperty, value);
    }

    public ModuleBrowserViewMode ViewMode
    {
        get => (ModuleBrowserViewMode)GetValue(ViewModeProperty);
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

    public bool ShowRecommendations
    {
        get => (bool)GetValue(ShowRecommendationsProperty);
        set => SetValue(ShowRecommendationsProperty, value);
    }

    public bool AutoCompleteSearch
    {
        get => (bool)GetValue(AutoCompleteSearchProperty);
        set => SetValue(AutoCompleteSearchProperty, value);
    }

    public TimeSpan SearchDelay
    {
        get => (TimeSpan)GetValue(SearchDelayProperty);
        set => SetValue(SearchDelayProperty, value);
    }

    public int MaxSearchResults
    {
        get => (int)GetValue(MaxSearchResultsProperty);
        set => SetValue(MaxSearchResultsProperty, value);
    }

    #endregion

    #region Private Fields

    private Grid _rootGrid;
    private TextBox _searchBox;
    private ListBox _moduleList;
    private Grid _moduleGrid;
    private StackPanel _filtersPanel;
    private Canvas _particleCanvas;
    private Canvas _animationCanvas;
    private DispatcherTimer _searchTimer;
    private DispatcherTimer _animationTimer;
    private readonly List<UIElement> _particles = new();
    private readonly List<UIElement> _searchAnimations = new();
    private CollectionViewSource _moduleViewSource;
    private bool _isSearching;
    private readonly Dictionary<string, List<string>> _searchSuggestions = new();
    private readonly Random _random = new();

    #endregion

    #region Constructor

    public HoloModuleBrowser()
    {
        InitializeComponent();
        InitializeSearchEngine();
        InitializeAnimationSystem();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 600;
        Height = 800;
        Background = Brushes.Transparent;

        _rootGrid = new Grid();
        _rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Search
        _rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Filters
        _rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content

        CreateSearchInterface();
        CreateFiltersInterface();
        CreateModuleDisplay();
        CreateParticleSystem();

        Content = _rootGrid;
    }

    private void CreateSearchInterface()
    {
        var searchPanel = new Grid();
        searchPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        searchPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Holographic search box
        _searchBox = new TextBox
        {
            FontSize = 14,
            Padding = new Thickness(15, 10),
            Margin = new Thickness(10),
            Background = CreateHolographicBackground(0.3),
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            BorderBrush = GetBrushForColorScheme(EVEColorScheme),
            BorderThickness = new Thickness(1),
            Effect = CreateGlowEffect(),
            Style = CreateHolographicTextBoxStyle()
        };

        // Search icon with particle effects
        var searchIcon = new Path
        {
            Data = Geometry.Parse("M15.5 14h-.79l-.28-.27C15.41 12.59 16 11.11 16 9.5 16 5.91 13.09 3 9.5 3S3 5.91 3 9.5 5.91 16 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14z"),
            Fill = GetBrushForColorScheme(EVEColorScheme),
            Width = 20,
            Height = 20,
            Margin = new Thickness(10),
            Effect = CreateGlowEffect()
        };

        Grid.SetColumn(_searchBox, 0);
        Grid.SetColumn(searchIcon, 1);
        searchPanel.Children.Add(_searchBox);
        searchPanel.Children.Add(searchIcon);

        Grid.SetRow(searchPanel, 0);
        _rootGrid.Children.Add(searchPanel);
    }

    private void CreateFiltersInterface()
    {
        _filtersPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10, 0)
        };

        // Create filter buttons
        CreateFilterButton("All", ModuleCategory.All);
        CreateFilterButton("Weapons", ModuleCategory.Weapons);
        CreateFilterButton("Defense", ModuleCategory.Defense);
        CreateFilterButton("Electronics", ModuleCategory.Electronics);
        CreateFilterButton("Engineering", ModuleCategory.Engineering);
        CreateFilterButton("Drones", ModuleCategory.Drones);

        Grid.SetRow(_filtersPanel, 1);
        _rootGrid.Children.Add(_filtersPanel);
    }

    private void CreateFilterButton(string text, ModuleCategory category)
    {
        var button = new Button
        {
            Content = text,
            Margin = new Thickness(5),
            Padding = new Thickness(15, 8),
            Background = CreateHolographicBackground(0.2),
            Foreground = GetBrushForColorScheme(EVEColorScheme),
            BorderBrush = GetBrushForColorScheme(EVEColorScheme),
            BorderThickness = new Thickness(1),
            Effect = CreateGlowEffect(),
            Style = CreateHolographicButtonStyle(),
            Tag = category
        };

        button.Click += FilterButton_Click;
        _filtersPanel.Children.Add(button);
    }

    private void CreateModuleDisplay()
    {
        var contentGrid = new Grid();

        // Module list for list view
        _moduleList = new ListBox
        {
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            ItemContainerStyle = CreateHolographicListItemStyle(),
            ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Visibility = ViewMode == ModuleBrowserViewMode.List ? Visibility.Visible : Visibility.Collapsed
        };

        // Module grid for grid view
        _moduleGrid = new Grid
        {
            Background = Brushes.Transparent,
            Visibility = ViewMode == ModuleBrowserViewMode.Grid ? Visibility.Visible : Visibility.Collapsed
        };

        contentGrid.Children.Add(_moduleList);
        contentGrid.Children.Add(_moduleGrid);

        Grid.SetRow(contentGrid, 2);
        _rootGrid.Children.Add(contentGrid);

        // Initialize collection view
        _moduleViewSource = new CollectionViewSource();
        if (Modules != null)
        {
            _moduleViewSource.Source = Modules;
            _moduleList.ItemsSource = _moduleViewSource.View;
        }
    }

    private void CreateParticleSystem()
    {
        _particleCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false
        };

        _animationCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false
        };

        Grid.SetRowSpan(_particleCanvas, 3);
        Grid.SetRowSpan(_animationCanvas, 3);
        _rootGrid.Children.Add(_particleCanvas);
        _rootGrid.Children.Add(_animationCanvas);
    }

    #endregion

    #region Search Engine

    private void InitializeSearchEngine()
    {
        _searchTimer = new DispatcherTimer
        {
            Interval = SearchDelay
        };
        _searchTimer.Tick += SearchTimer_Tick;

        // Initialize search suggestions
        _searchSuggestions["weapons"] = new List<string> { "turret", "missile", "launcher", "beam", "pulse", "autocannon" };
        _searchSuggestions["defense"] = new List<string> { "shield", "armor", "hull", "resistance", "hardener", "booster" };
        _searchSuggestions["electronics"] = new List<string> { "sensor", "ecm", "point", "scram", "web", "painter" };
        _searchSuggestions["engineering"] = new List<string> { "reactor", "capacitor", "battery", "power", "cpu", "fitting" };
    }

    private async void SearchTimer_Tick(object sender, EventArgs e)
    {
        _searchTimer.Stop();
        await PerformSearch();
    }

    private async Task PerformSearch()
    {
        if (_isSearching) return;

        _isSearching = true;
        StartSearchAnimation();

        try
        {
            var searchText = SearchText?.ToLowerInvariant() ?? string.Empty;
            
            if (_moduleViewSource?.View != null)
            {
                _moduleViewSource.View.Filter = item =>
                {
                    if (item is HoloModuleData module)
                    {
                        return string.IsNullOrEmpty(searchText) ||
                               module.Name.ToLowerInvariant().Contains(searchText) ||
                               module.Description.ToLowerInvariant().Contains(searchText) ||
                               module.Category.ToString().ToLowerInvariant().Contains(searchText);
                    }
                    return false;
                };

                // Apply filter criteria if set
                if (FilterCriteria != null)
                {
                    ApplyAdvancedFilters();
                }
            }

            await Task.Delay(100); // Simulate search processing
            UpdateSearchResults();
        }
        finally
        {
            _isSearching = false;
            StopSearchAnimation();
        }
    }

    private void ApplyAdvancedFilters()
    {
        if (_moduleViewSource?.View == null || FilterCriteria == null) return;

        var currentFilter = _moduleViewSource.View.Filter;
        _moduleViewSource.View.Filter = item =>
        {
            if (item is HoloModuleData module)
            {
                var passesSearch = currentFilter?.Invoke(item) ?? true;
                var passesCriteria = true;

                if (FilterCriteria.Category != ModuleCategory.All)
                    passesCriteria &= module.Category == FilterCriteria.Category;

                if (FilterCriteria.MinMetaLevel.HasValue)
                    passesCriteria &= module.MetaLevel >= FilterCriteria.MinMetaLevel.Value;

                if (FilterCriteria.MaxMetaLevel.HasValue)
                    passesCriteria &= module.MetaLevel <= FilterCriteria.MaxMetaLevel.Value;

                if (FilterCriteria.ShipTypes?.Count > 0)
                    passesCriteria &= FilterCriteria.ShipTypes.Any(shipType => module.CompatibleShipTypes.Contains(shipType));

                if (FilterCriteria.RequiredSkills?.Count > 0)
                    passesCriteria &= FilterCriteria.RequiredSkills.All(skill => module.RequiredSkills.ContainsKey(skill.Key));

                return passesSearch && passesCriteria;
            }
            return false;
        };
    }

    #endregion

    #region Animation System

    private void InitializeAnimationSystem()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        _animationTimer.Tick += AnimationTimer_Tick;
    }

    private void AnimationTimer_Tick(object sender, EventArgs e)
    {
        UpdateParticles();
        UpdateSearchAnimations();
    }

    private void StartSearchAnimation()
    {
        if (!EnableAnimations) return;

        // Create search particles
        for (int i = 0; i < 10; i++)
        {
            CreateSearchParticle();
        }

        // Start pulsing effect on search box
        var pulseAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.6,
            Duration = TimeSpan.FromMilliseconds(500),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever
        };

        _searchBox.BeginAnimation(OpacityProperty, pulseAnimation);
    }

    private void StopSearchAnimation()
    {
        _searchBox.BeginAnimation(OpacityProperty, null);
        ClearSearchParticles();
    }

    private void CreateSearchParticle()
    {
        if (!EnableParticleEffects) return;

        var particle = new Ellipse
        {
            Width = _random.NextDouble() * 4 + 2,
            Height = _random.NextDouble() * 4 + 2,
            Fill = GetBrushForColorScheme(EVEColorScheme),
            Opacity = 0.8,
            Effect = CreateGlowEffect()
        };

        var startX = _searchBox.ActualWidth * _random.NextDouble();
        var startY = _searchBox.ActualHeight * _random.NextDouble();

        Canvas.SetLeft(particle, startX);
        Canvas.SetTop(particle, startY);
        Canvas.SetZIndex(particle, 1000);

        _particles.Add(particle);
        _particleCanvas.Children.Add(particle);

        // Animate particle movement
        var moveAnimation = new DoubleAnimation
        {
            From = startY,
            To = startY - 50,
            Duration = TimeSpan.FromMilliseconds(2000),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeAnimation = new DoubleAnimation
        {
            From = 0.8,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(2000)
        };

        particle.BeginAnimation(Canvas.TopProperty, moveAnimation);
        particle.BeginAnimation(OpacityProperty, fadeAnimation);

        // Remove particle after animation
        Task.Delay(2000).ContinueWith(t =>
        {
            Dispatcher.BeginInvoke(() =>
            {
                _particles.Remove(particle);
                _particleCanvas.Children.Remove(particle);
            });
        });
    }

    private void UpdateParticles()
    {
        // Update particle positions and effects
        foreach (var particle in _particles.ToList())
        {
            if (particle.Opacity <= 0.1)
            {
                _particles.Remove(particle);
                _particleCanvas.Children.Remove(particle);
            }
        }
    }

    private void UpdateSearchAnimations()
    {
        // Update search-related animations
        foreach (var animation in _searchAnimations.ToList())
        {
            if (animation.Opacity <= 0.1)
            {
                _searchAnimations.Remove(animation);
                _animationCanvas.Children.Remove(animation);
            }
        }
    }

    private void ClearSearchParticles()
    {
        foreach (var particle in _particles.ToList())
        {
            _particles.Remove(particle);
            _particleCanvas.Children.Remove(particle);
        }
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableAnimations)
        {
            _animationTimer.Start();
        }

        _searchBox.TextChanged += SearchBox_TextChanged;
        ApplyHolographicEffects();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        _searchTimer?.Stop();
        _searchBox.TextChanged -= SearchBox_TextChanged;
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _searchTimer.Stop();
        _searchTimer.Start();

        if (AutoCompleteSearch)
        {
            ShowSearchSuggestions();
        }
    }

    private void FilterButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ModuleCategory category)
        {
            if (FilterCriteria == null)
                FilterCriteria = new ModuleFilterCriteria();

            FilterCriteria.Category = category;
            
            // Update button appearance
            UpdateFilterButtonStates();
            
            // Trigger search update
            Task.Run(PerformSearch);
        }
    }

    #endregion

    #region UI Updates

    private void UpdateSearchResults()
    {
        // Animate results appearing
        if (EnableAnimations && _moduleList.Items.Count > 0)
        {
            foreach (ListBoxItem item in _moduleList.Items)
            {
                if (item?.IsVisible == true)
                {
                    AnimateItemAppearance(item);
                }
            }
        }

        // Update recommendations if enabled
        if (ShowRecommendations)
        {
            UpdateRecommendations();
        }
    }

    private void AnimateItemAppearance(UIElement item)
    {
        item.Opacity = 0;
        item.RenderTransform = new TranslateTransform(0, 20);

        var fadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var slideIn = new DoubleAnimation
        {
            From = 20,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        item.BeginAnimation(OpacityProperty, fadeIn);
        ((TranslateTransform)item.RenderTransform).BeginAnimation(TranslateTransform.YProperty, slideIn);
    }

    private void UpdateFilterButtonStates()
    {
        foreach (Button button in _filtersPanel.Children.OfType<Button>())
        {
            var isActive = FilterCriteria?.Category.Equals(button.Tag) == true;
            button.Background = CreateHolographicBackground(isActive ? 0.5 : 0.2);
            button.Effect = isActive ? CreateIntenseGlowEffect() : CreateGlowEffect();
        }
    }

    private void UpdateRecommendations()
    {
        // Implementation for module recommendations based on search patterns
        // This would analyze user search behavior and suggest relevant modules
    }

    private void ShowSearchSuggestions()
    {
        var searchText = SearchText?.ToLowerInvariant();
        if (string.IsNullOrEmpty(searchText)) return;

        // Find matching suggestions
        var suggestions = new List<string>();
        foreach (var category in _searchSuggestions)
        {
            if (category.Key.Contains(searchText))
            {
                suggestions.AddRange(category.Value.Where(s => s.Contains(searchText)));
            }
        }

        // Display suggestions (implementation would create popup with suggestions)
    }

    #endregion

    #region Holographic Effects

    private void ApplyHolographicEffects()
    {
        var intensity = HolographicIntensity;

        // Apply effects to main components
        _searchBox.Effect = CreateGlowEffect(intensity);
        _moduleList.Effect = CreateGlowEffect(intensity * 0.5);

        // Update particle effects
        if (EnableParticleEffects && intensity > 0.5)
        {
            CreateAmbientParticles();
        }
    }

    private void CreateAmbientParticles()
    {
        // Create subtle ambient particles for atmosphere
        for (int i = 0; i < 5; i++)
        {
            Task.Delay(i * 1000).ContinueWith(t =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    if (EnableParticleEffects)
                    {
                        CreateFloatingParticle();
                    }
                });
            });
        }
    }

    private void CreateFloatingParticle()
    {
        var particle = new Ellipse
        {
            Width = 2,
            Height = 2,
            Fill = GetBrushForColorScheme(EVEColorScheme),
            Opacity = 0.3
        };

        var x = _random.NextDouble() * ActualWidth;
        var y = ActualHeight;

        Canvas.SetLeft(particle, x);
        Canvas.SetTop(particle, y);
        Canvas.SetZIndex(particle, 500);

        _particles.Add(particle);
        _particleCanvas.Children.Add(particle);

        // Animate upward movement
        var moveAnimation = new DoubleAnimation
        {
            From = y,
            To = -10,
            Duration = TimeSpan.FromMilliseconds(8000),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        particle.BeginAnimation(Canvas.TopProperty, moveAnimation);

        // Remove after animation
        Task.Delay(8000).ContinueWith(t =>
        {
            Dispatcher.BeginInvoke(() =>
            {
                _particles.Remove(particle);
                _particleCanvas.Children.Remove(particle);
            });
        });
    }

    #endregion

    #region Style Creators

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
            BlurRadius = 15,
            Opacity = 0.9,
            ShadowDepth = 0
        };
    }

    private Style CreateHolographicTextBoxStyle()
    {
        var style = new Style(typeof(TextBox));
        style.Setters.Add(new Setter(ForegroundProperty, GetBrushForColorScheme(EVEColorScheme)));
        style.Setters.Add(new Setter(FontFamilyProperty, new FontFamily("Segoe UI")));
        style.Setters.Add(new Setter(FontWeightProperty, FontWeights.Medium));
        return style;
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

    #region Dependency Property Callbacks

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloModuleBrowser browser)
        {
            browser.ApplyHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloModuleBrowser browser)
        {
            browser.ApplyHolographicEffects();
        }
    }

    private static void OnModulesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloModuleBrowser browser && e.NewValue is ObservableCollection<HoloModuleData> modules)
        {
            browser._moduleViewSource.Source = modules;
            browser._moduleList.ItemsSource = browser._moduleViewSource.View;
        }
    }

    private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloModuleBrowser browser)
        {
            browser._searchTimer.Stop();
            browser._searchTimer.Start();
        }
    }

    private static void OnSelectedModuleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloModuleBrowser browser)
        {
            browser._moduleList.SelectedItem = e.NewValue;
        }
    }

    private static void OnFilterCriteriaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloModuleBrowser browser)
        {
            Task.Run(browser.PerformSearch);
        }
    }

    private static void OnViewModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloModuleBrowser browser)
        {
            browser._moduleList.Visibility = browser.ViewMode == ModuleBrowserViewMode.List ? Visibility.Visible : Visibility.Collapsed;
            browser._moduleGrid.Visibility = browser.ViewMode == ModuleBrowserViewMode.Grid ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloModuleBrowser browser)
        {
            if ((bool)e.NewValue)
                browser._animationTimer.Start();
            else
                browser._animationTimer.Stop();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloModuleBrowser browser && !(bool)e.NewValue)
        {
            browser.ClearSearchParticles();
        }
    }

    private static void OnShowRecommendationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloModuleBrowser browser)
        {
            browser.UpdateRecommendations();
        }
    }

    private static void OnAutoCompleteSearchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Auto-complete setting changed - no immediate action needed
    }

    private static void OnSearchDelayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloModuleBrowser browser)
        {
            browser._searchTimer.Interval = (TimeSpan)e.NewValue;
        }
    }

    private static void OnMaxSearchResultsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Max results setting changed - will affect next search
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
            ClearSearchParticles();
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
    /// Performs immediate search with current search text
    /// </summary>
    public async Task SearchAsync()
    {
        await PerformSearch();
    }

    /// <summary>
    /// Clears current search and resets filters
    /// </summary>
    public void ClearSearch()
    {
        SearchText = string.Empty;
        FilterCriteria = null;
        _moduleViewSource.View.Filter = null;
    }

    /// <summary>
    /// Focuses the search box for user input
    /// </summary>
    public void FocusSearch()
    {
        _searchBox.Focus();
    }

    /// <summary>
    /// Sets the selected module and scrolls it into view
    /// </summary>
    public void SelectModule(HoloModuleData module)
    {
        SelectedModule = module;
        _moduleList.ScrollIntoView(module);
    }

    #endregion
}

#region Supporting Types

/// <summary>
/// Module filter criteria for advanced filtering
/// </summary>
public class ModuleFilterCriteria
{
    public ModuleCategory Category { get; set; } = ModuleCategory.All;
    public int? MinMetaLevel { get; set; }
    public int? MaxMetaLevel { get; set; }
    public List<string> ShipTypes { get; set; } = new();
    public Dictionary<string, int> RequiredSkills { get; set; } = new();
    public bool ShowTechII { get; set; } = true;
    public bool ShowTechI { get; set; } = true;
    public bool ShowFaction { get; set; } = true;
    public bool ShowDeadspace { get; set; } = true;
    public bool ShowOfficer { get; set; } = true;
}

/// <summary>
/// Module browser view modes
/// </summary>
public enum ModuleBrowserViewMode
{
    List,
    Grid,
    Compact
}

/// <summary>
/// Module categories for filtering
/// </summary>
public enum ModuleCategory
{
    All,
    Weapons,
    Defense,
    Electronics,
    Engineering,
    Drones,
    Rigs,
    Subsystems
}

/// <summary>
/// Holographic module data for display
/// </summary>
public class HoloModuleData
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ModuleCategory Category { get; set; }
    public int MetaLevel { get; set; }
    public List<string> CompatibleShipTypes { get; set; } = new();
    public Dictionary<string, int> RequiredSkills { get; set; } = new();
    public string IconPath { get; set; } = string.Empty;
    public Dictionary<string, object> Attributes { get; set; } = new();
    public bool IsTechII { get; set; }
    public bool IsFaction { get; set; }
    public bool IsDeadspace { get; set; }
    public bool IsOfficer { get; set; }
    public double Volume { get; set; }
    public decimal Price { get; set; }
}

#endregion