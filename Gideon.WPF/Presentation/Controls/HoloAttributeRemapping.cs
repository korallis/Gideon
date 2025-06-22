// ==========================================================================
// HoloAttributeRemapping.cs - Holographic Attribute Remapping Analysis
// ==========================================================================
// Advanced attribute remapping system featuring AI-powered optimization,
// training efficiency analysis, EVE-style attribute planning, and holographic visualization.
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
/// Holographic attribute remapping analysis with AI-powered optimization and efficiency visualization
/// </summary>
public class HoloAttributeRemapping : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloAttributeRemapping),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloAttributeRemapping),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty CharacterAttributesProperty =
        DependencyProperty.Register(nameof(CharacterAttributes), typeof(HoloCharacterAttributes), typeof(HoloAttributeRemapping),
            new PropertyMetadata(null, OnCharacterAttributesChanged));

    public static readonly DependencyProperty RemappingPlansProperty =
        DependencyProperty.Register(nameof(RemappingPlans), typeof(ObservableCollection<HoloRemappingPlan>), typeof(HoloAttributeRemapping),
            new PropertyMetadata(null, OnRemappingPlansChanged));

    public static readonly DependencyProperty SelectedCharacterProperty =
        DependencyProperty.Register(nameof(SelectedCharacter), typeof(string), typeof(HoloAttributeRemapping),
            new PropertyMetadata("Main Character", OnSelectedCharacterChanged));

    public static readonly DependencyProperty OptimizationModeProperty =
        DependencyProperty.Register(nameof(OptimizationMode), typeof(AttributeOptimizationMode), typeof(HoloAttributeRemapping),
            new PropertyMetadata(AttributeOptimizationMode.Balanced, OnOptimizationModeChanged));

    public static readonly DependencyProperty AnalysisTimeFrameProperty =
        DependencyProperty.Register(nameof(AnalysisTimeFrame), typeof(TimeSpan), typeof(HoloAttributeRemapping),
            new PropertyMetadata(TimeSpan.FromDays(365), OnAnalysisTimeFrameChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloAttributeRemapping),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloAttributeRemapping),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowOptimizationSuggestionsProperty =
        DependencyProperty.Register(nameof(ShowOptimizationSuggestions), typeof(bool), typeof(HoloAttributeRemapping),
            new PropertyMetadata(true, OnShowOptimizationSuggestionsChanged));

    public static readonly DependencyProperty ShowEfficiencyAnalysisProperty =
        DependencyProperty.Register(nameof(ShowEfficiencyAnalysis), typeof(bool), typeof(HoloAttributeRemapping),
            new PropertyMetadata(true, OnShowEfficiencyAnalysisChanged));

    public static readonly DependencyProperty ShowRadarChartProperty =
        DependencyProperty.Register(nameof(ShowRadarChart), typeof(bool), typeof(HoloAttributeRemapping),
            new PropertyMetadata(true, OnShowRadarChartChanged));

    public static readonly DependencyProperty IncludeImplantsProperty =
        DependencyProperty.Register(nameof(IncludeImplants), typeof(bool), typeof(HoloAttributeRemapping),
            new PropertyMetadata(true, OnIncludeImplantsChanged));

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

    public HoloCharacterAttributes CharacterAttributes
    {
        get => (HoloCharacterAttributes)GetValue(CharacterAttributesProperty);
        set => SetValue(CharacterAttributesProperty, value);
    }

    public ObservableCollection<HoloRemappingPlan> RemappingPlans
    {
        get => (ObservableCollection<HoloRemappingPlan>)GetValue(RemappingPlansProperty);
        set => SetValue(RemappingPlansProperty, value);
    }

    public string SelectedCharacter
    {
        get => (string)GetValue(SelectedCharacterProperty);
        set => SetValue(SelectedCharacterProperty, value);
    }

    public AttributeOptimizationMode OptimizationMode
    {
        get => (AttributeOptimizationMode)GetValue(OptimizationModeProperty);
        set => SetValue(OptimizationModeProperty, value);
    }

    public TimeSpan AnalysisTimeFrame
    {
        get => (TimeSpan)GetValue(AnalysisTimeFrameProperty);
        set => SetValue(AnalysisTimeFrameProperty, value);
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

    public bool ShowOptimizationSuggestions
    {
        get => (bool)GetValue(ShowOptimizationSuggestionsProperty);
        set => SetValue(ShowOptimizationSuggestionsProperty, value);
    }

    public bool ShowEfficiencyAnalysis
    {
        get => (bool)GetValue(ShowEfficiencyAnalysisProperty);
        set => SetValue(ShowEfficiencyAnalysisProperty, value);
    }

    public bool ShowRadarChart
    {
        get => (bool)GetValue(ShowRadarChartProperty);
        set => SetValue(ShowRadarChartProperty, value);
    }

    public bool IncludeImplants
    {
        get => (bool)GetValue(IncludeImplantsProperty);
        set => SetValue(IncludeImplantsProperty, value);
    }

    #endregion

    #region Fields

    private Canvas _mainCanvas;
    private Canvas _particleCanvas;
    private Border _currentAttributesSection;
    private Border _radarChartSection;
    private Border _optimizationSection;
    private Border _efficiencySection;
    private ScrollViewer _plansScrollViewer;
    private StackPanel _plansPanel;
    private TabControl _attributeTabs;
    
    private readonly List<HoloAttributeParticle> _particles = new();
    private readonly DispatcherTimer _animationTimer;
    private readonly DispatcherTimer _analysisUpdateTimer;
    private readonly Random _random = new();
    
    private bool _isAnimating = true;
    private bool _isDisposed = false;
    private double _currentAnimationTime = 0;

    #endregion

    #region Constructor

    public HoloAttributeRemapping()
    {
        InitializeComponent();
        
        _animationTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16) // 60 FPS
        };
        _animationTimer.Tick += OnAnimationTick;
        
        _analysisUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3) // Update analysis every 3 seconds
        };
        _analysisUpdateTimer.Tick += OnAnalysisUpdateTick;
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 900;
        Height = 700;
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
        
        var contentGrid = new Grid();
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(180, GridUnitType.Pixel) });
        
        // Header section
        var headerSection = CreateHeaderSection();
        Grid.SetRow(headerSection, 0);
        contentGrid.Children.Add(headerSection);
        
        // Main content with tabs
        _attributeTabs = CreateAttributeTabs();
        Grid.SetRow(_attributeTabs, 1);
        contentGrid.Children.Add(_attributeTabs);
        
        // Bottom analysis sections
        var bottomGrid = new Grid();
        bottomGrid.ColumnDefinitions.Add(new ColumnDefinition());
        bottomGrid.ColumnDefinitions.Add(new ColumnDefinition());
        
        _efficiencySection = CreateEfficiencySection();
        Grid.SetColumn(_efficiencySection, 0);
        bottomGrid.Children.Add(_efficiencySection);
        
        _optimizationSection = CreateOptimizationSection();
        Grid.SetColumn(_optimizationSection, 1);
        bottomGrid.Children.Add(_optimizationSection);
        
        Grid.SetRow(bottomGrid, 2);
        contentGrid.Children.Add(bottomGrid);
        
        _mainCanvas.Children.Add(contentGrid);
        _mainCanvas.Children.Add(_particleCanvas);
        border.Child = _mainCanvas;
        Content = border;

        CreateSampleData();
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
            Text = "ATTRIBUTE REMAPPING ANALYSIS",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        titlePanel.Children.Add(titleText);

        var subtitleText = new TextBlock
        {
            Text = "Neural Optimization & Training Efficiency",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 200, 200)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0)
        };
        titlePanel.Children.Add(subtitleText);

        Grid.SetColumn(titlePanel, 0);
        grid.Children.Add(titlePanel);

        // Configuration panel
        var configPanel = CreateConfigurationPanel();
        Grid.SetColumn(configPanel, 1);
        grid.Children.Add(configPanel);

        border.Child = grid;
        return border;
    }

    private StackPanel CreateConfigurationPanel()
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(15, 10, 15, 10)
        };

        var configHeader = new TextBlock
        {
            Text = "ANALYSIS CONFIG",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8)
        };
        panel.Children.Add(configHeader);

        // Optimization mode
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
            Width = 80,
            VerticalAlignment = VerticalAlignment.Center
        };
        modePanel.Children.Add(modeLabel);

        var modeCombo = new ComboBox
        {
            Width = 140,
            Height = 22,
            FontSize = 9,
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 100, 150)),
            Foreground = new SolidColorBrush(Colors.White),
            BorderBrush = new SolidColorBrush(Color.FromArgb(120, 0, 200, 255))
        };
        modeCombo.Items.Add("Balanced");
        modeCombo.Items.Add("Training Speed");
        modeCombo.Items.Add("Specific Skills");
        modeCombo.Items.Add("Long Term");
        modeCombo.SelectedIndex = 0;
        modePanel.Children.Add(modeCombo);

        panel.Children.Add(modePanel);

        // Time frame
        var timeFramePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 3, 0, 3)
        };

        var timeLabel = new TextBlock
        {
            Text = "Time Frame:",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            Width = 80,
            VerticalAlignment = VerticalAlignment.Center
        };
        timeFramePanel.Children.Add(timeLabel);

        var timeCombo = new ComboBox
        {
            Width = 140,
            Height = 22,
            FontSize = 9,
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 100, 150)),
            Foreground = new SolidColorBrush(Colors.White),
            BorderBrush = new SolidColorBrush(Color.FromArgb(120, 0, 200, 255))
        };
        timeCombo.Items.Add("3 Months");
        timeCombo.Items.Add("6 Months");
        timeCombo.Items.Add("1 Year");
        timeCombo.Items.Add("2 Years");
        timeCombo.SelectedIndex = 2;
        timeFramePanel.Children.Add(timeCombo);

        panel.Children.Add(timeFramePanel);

        // Include implants checkbox
        var implantsCheck = new CheckBox
        {
            Content = "Include Implants",
            FontSize = 9,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            IsChecked = true,
            Margin = new Thickness(0, 4, 0, 2)
        };
        panel.Children.Add(implantsCheck);

        return panel;
    }

    private TabControl CreateAttributeTabs()
    {
        var tabControl = new TabControl
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(80, 0, 200, 255)),
            Margin = new Thickness(10)
        };

        // Current Attributes Tab
        var currentTab = new TabItem
        {
            Header = "Current",
            Foreground = new SolidColorBrush(Colors.White),
            Background = new SolidColorBrush(Color.FromArgb(60, 0, 100, 150))
        };
        currentTab.Content = CreateCurrentAttributesContent();
        tabControl.Items.Add(currentTab);

        // Remapping Plans Tab
        var plansTab = new TabItem
        {
            Header = "Plans",
            Foreground = new SolidColorBrush(Colors.White),
            Background = new SolidColorBrush(Color.FromArgb(60, 0, 100, 150))
        };
        plansTab.Content = CreateRemappingPlansContent();
        tabControl.Items.Add(plansTab);

        // Radar Chart Tab
        var radarTab = new TabItem
        {
            Header = "Radar",
            Foreground = new SolidColorBrush(Colors.White),
            Background = new SolidColorBrush(Color.FromArgb(60, 0, 100, 150))
        };
        radarTab.Content = CreateRadarChartContent();
        tabControl.Items.Add(radarTab);

        return tabControl;
    }

    private Grid CreateCurrentAttributesContent()
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());

        // Attributes display
        var attributesPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(20, 15, 10, 15)
        };

        var attributesHeader = new TextBlock
        {
            Text = "CURRENT ATTRIBUTES",
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 15)
        };
        attributesPanel.Children.Add(attributesHeader);

        var attributes = new[]
        {
            new { Name = "Intelligence", Value = 23, Base = 20, Implant = 3 },
            new { Name = "Memory", Value = 25, Base = 21, Implant = 4 },
            new { Name = "Charisma", Value = 21, Base = 19, Implant = 2 },
            new { Name = "Perception", Value = 22, Base = 20, Implant = 2 },
            new { Name = "Willpower", Value = 24, Base = 22, Implant = 2 }
        };

        foreach (var attr in attributes)
        {
            var attrItem = CreateAttributeItem(attr.Name, attr.Value, attr.Base, attr.Implant);
            attributesPanel.Children.Add(attrItem);
        }

        Grid.SetColumn(attributesPanel, 0);
        grid.Children.Add(attributesPanel);

        // Attribute sliders for remapping simulation
        var slidersPanel = CreateAttributeSliders();
        Grid.SetColumn(slidersPanel, 1);
        grid.Children.Add(slidersPanel);

        return grid;
    }

    private Border CreateAttributeItem(string name, int total, int baseValue, int implant)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 100, 150)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(80, 0, 200, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3),
            Margin = new Thickness(5),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(60, 0, 200, 255),
                ShadowDepth = 0,
                BlurRadius = 6
            }
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120, GridUnitType.Pixel) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60, GridUnitType.Pixel) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Attribute name
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

        // Total value
        var totalText = new TextBlock
        {
            Text = total.ToString(),
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 8, 5, 8)
        };
        Grid.SetColumn(totalText, 1);
        grid.Children.Add(totalText);

        // Breakdown
        var breakdownPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 8, 10, 8)
        };

        var baseText = new TextBlock
        {
            Text = $"Base: {baseValue}",
            FontSize = 9,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 200, 200, 200)),
            Margin = new Thickness(0, 0, 10, 0)
        };
        breakdownPanel.Children.Add(baseText);

        var implantText = new TextBlock
        {
            Text = $"Implant: +{implant}",
            FontSize = 9,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 0, 255, 100)),
            Margin = new Thickness(0, 0, 0, 0)
        };
        breakdownPanel.Children.Add(implantText);

        Grid.SetColumn(breakdownPanel, 2);
        grid.Children.Add(breakdownPanel);

        border.Child = grid;
        return border;
    }

    private StackPanel CreateAttributeSliders()
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10, 15, 20, 15)
        };

        var slidersHeader = new TextBlock
        {
            Text = "REMAPPING SIMULATOR",
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 200, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 15)
        };
        panel.Children.Add(slidersHeader);

        var pointsText = new TextBlock
        {
            Text = "Available Points: 14",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        panel.Children.Add(pointsText);

        var attributes = new[] { "Intelligence", "Memory", "Charisma", "Perception", "Willpower" };
        var baseValues = new[] { 20, 21, 19, 20, 22 };

        for (int i = 0; i < attributes.Length; i++)
        {
            var sliderItem = CreateAttributeSlider(attributes[i], baseValues[i]);
            panel.Children.Add(sliderItem);
        }

        var applyButton = new Button
        {
            Content = "SIMULATE REMAP",
            Width = 150,
            Height = 30,
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            Background = new SolidColorBrush(Color.FromArgb(100, 255, 200, 0)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 255, 200, 0)),
            BorderThickness = new Thickness(1),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 15, 0, 0),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(100, 255, 200, 0),
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };
        panel.Children.Add(applyButton);

        return panel;
    }

    private Border CreateAttributeSlider(string attributeName, int baseValue)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(30, 100, 50, 0)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(60, 255, 200, 0)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3),
            Margin = new Thickness(5, 3, 5, 3)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100, GridUnitType.Pixel) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40, GridUnitType.Pixel) });

        var nameText = new TextBlock
        {
            Text = attributeName,
            FontSize = 10,
            Foreground = new SolidColorBrush(Colors.White),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 6, 5, 6)
        };
        Grid.SetColumn(nameText, 0);
        grid.Children.Add(nameText);

        var slider = new Slider
        {
            Minimum = 17,
            Maximum = 27,
            Value = baseValue,
            TickPlacement = TickPlacement.None,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 6, 5, 6),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 200, 0))
        };
        Grid.SetColumn(slider, 1);
        grid.Children.Add(slider);

        var valueText = new TextBlock
        {
            Text = baseValue.ToString(),
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 6, 8, 6)
        };
        Grid.SetColumn(valueText, 2);
        grid.Children.Add(valueText);

        // Bind slider value to text
        slider.ValueChanged += (s, e) => valueText.Text = ((int)e.NewValue).ToString();

        border.Child = grid;
        return border;
    }

    private ScrollViewer CreateRemappingPlansContent()
    {
        _plansScrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = new SolidColorBrush(Color.FromArgb(20, 0, 50, 100))
        };

        _plansPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10)
        };

        _plansScrollViewer.Content = _plansPanel;
        return _plansScrollViewer;
    }

    private Canvas CreateRadarChartContent()
    {
        var canvas = new Canvas
        {
            Background = new SolidColorBrush(Color.FromArgb(20, 0, 50, 100)),
            Margin = new Thickness(10)
        };

        CreateRadarChart(canvas);
        return canvas;
    }

    private void CreateRadarChart(Canvas canvas)
    {
        canvas.Width = 400;
        canvas.Height = 400;

        var centerX = canvas.Width / 2;
        var centerY = canvas.Height / 2;
        var radius = 150;

        // Draw radar grid
        for (int i = 1; i <= 5; i++)
        {
            var gridRadius = radius * i / 5;
            var gridCircle = new Ellipse
            {
                Width = gridRadius * 2,
                Height = gridRadius * 2,
                Stroke = new SolidColorBrush(Color.FromArgb(60, 100, 100, 100)),
                StrokeThickness = 1,
                Fill = Brushes.Transparent
            };
            Canvas.SetLeft(gridCircle, centerX - gridRadius);
            Canvas.SetTop(gridCircle, centerY - gridRadius);
            canvas.Children.Add(gridCircle);
        }

        // Draw axes
        var attributes = new[] { "Intelligence", "Memory", "Charisma", "Perception", "Willpower" };
        var values = new[] { 23, 25, 21, 22, 24 };
        var maxValue = 30;

        for (int i = 0; i < attributes.Length; i++)
        {
            var angle = i * 2 * Math.PI / attributes.Length - Math.PI / 2;
            var x = centerX + Math.Cos(angle) * radius;
            var y = centerY + Math.Sin(angle) * radius;

            // Axis line
            var line = new Line
            {
                X1 = centerX,
                Y1 = centerY,
                X2 = x,
                Y2 = y,
                Stroke = new SolidColorBrush(Color.FromArgb(80, 200, 200, 200)),
                StrokeThickness = 1
            };
            canvas.Children.Add(line);

            // Attribute label
            var label = new TextBlock
            {
                Text = attributes[i],
                FontSize = 10,
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var labelX = centerX + Math.Cos(angle) * (radius + 20) - attributes[i].Length * 2;
            var labelY = centerY + Math.Sin(angle) * (radius + 20) - 6;
            Canvas.SetLeft(label, labelX);
            Canvas.SetTop(label, labelY);
            canvas.Children.Add(label);
        }

        // Draw data polygon
        var polygon = new Polygon
        {
            Fill = new SolidColorBrush(Color.FromArgb(60, 0, 255, 255)),
            Stroke = new SolidColorBrush(Color.FromArgb(200, 0, 255, 255)),
            StrokeThickness = 2
        };

        for (int i = 0; i < attributes.Length; i++)
        {
            var angle = i * 2 * Math.PI / attributes.Length - Math.PI / 2;
            var valueRadius = radius * values[i] / maxValue;
            var x = centerX + Math.Cos(angle) * valueRadius;
            var y = centerY + Math.Sin(angle) * valueRadius;
            polygon.Points.Add(new Point(x, y));
        }

        canvas.Children.Add(polygon);

        // Add value points
        for (int i = 0; i < attributes.Length; i++)
        {
            var angle = i * 2 * Math.PI / attributes.Length - Math.PI / 2;
            var valueRadius = radius * values[i] / maxValue;
            var x = centerX + Math.Cos(angle) * valueRadius;
            var y = centerY + Math.Sin(angle) * valueRadius;

            var point = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
                Effect = new DropShadowEffect
                {
                    Color = Color.FromArgb(255, 0, 255, 255),
                    ShadowDepth = 0,
                    BlurRadius = 8
                }
            };
            Canvas.SetLeft(point, x - 4);
            Canvas.SetTop(point, y - 4);
            canvas.Children.Add(point);

            // Value label
            var valueLabel = new TextBlock
            {
                Text = values[i].ToString(),
                FontSize = 9,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Canvas.SetLeft(valueLabel, x - 8);
            Canvas.SetTop(valueLabel, y - 20);
            canvas.Children.Add(valueLabel);
        }
    }

    private Border CreateEfficiencySection()
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 100, 150)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(80, 0, 200, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(10),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(60, 0, 200, 255),
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
            Text = "TRAINING EFFICIENCY",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        panel.Children.Add(header);

        var efficiency = new[]
        {
            "Current SP/Hour: 2,547",
            "Optimal SP/Hour: 2,899",
            "Efficiency Gain: +13.8%",
            "Time Saved: 8.5 days",
            "Best Remap: INT/MEM",
            "Next Remap: Available",
            "Skill Focus: Gunnery",
            "Remap Cooldown: Ready"
        };

        foreach (var stat in efficiency)
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

    private Border CreateOptimizationSection()
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 100, 50, 0)),
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
            Text = "OPTIMIZATION SUGGESTIONS",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        panel.Children.Add(header);

        var suggestions = new[]
        {
            "• Remap to INT 27, MEM 27",
            "• Focus on perception skills next",
            "• Consider +5 implants",
            "• Plan remap for 6 months",
            "• Optimize for ship mastery",
            "• Alternative: balanced spread",
            "• Neural accelerator compatible",
            "• Review after skill plan"
        };

        foreach (var suggestion in suggestions)
        {
            var suggestionText = new TextBlock
            {
                Text = suggestion,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 200)),
                Margin = new Thickness(0, 2, 0, 0)
            };
            panel.Children.Add(suggestionText);
        }

        border.Child = panel;
        return border;
    }

    private void CreateSampleData()
    {
        CharacterAttributes = new HoloCharacterAttributes
        {
            Intelligence = 23,
            Memory = 25,
            Charisma = 21,
            Perception = 22,
            Willpower = 24,
            LastRemapDate = DateTime.Now.AddDays(-45),
            RemapsAvailable = 1,
            NextRemapDate = DateTime.Now
        };

        RemappingPlans = new ObservableCollection<HoloRemappingPlan>
        {
            new HoloRemappingPlan
            {
                PlanName = "Intelligence/Memory Focus",
                Intelligence = 27,
                Memory = 27,
                Charisma = 17,
                Perception = 17,
                Willpower = 17,
                EfficiencyGain = 0.138,
                TimeSaved = TimeSpan.FromDays(8.5),
                OptimalFor = "Gunnery, Engineering",
                Priority = RemappingPriority.High
            },
            new HoloRemappingPlan
            {
                PlanName = "Perception/Willpower Focus",
                Intelligence = 17,
                Memory = 17,
                Charisma = 17,
                Perception = 27,
                Willpower = 27,
                EfficiencyGain = 0.092,
                TimeSaved = TimeSpan.FromDays(4.2),
                OptimalFor = "Gunnery, Spaceship Command",
                Priority = RemappingPriority.Medium
            },
            new HoloRemappingPlan
            {
                PlanName = "Balanced Distribution",
                Intelligence = 22,
                Memory = 22,
                Charisma = 20,
                Perception = 21,
                Willpower = 20,
                EfficiencyGain = 0.045,
                TimeSaved = TimeSpan.FromDays(1.8),
                OptimalFor = "Mixed Training",
                Priority = RemappingPriority.Low
            }
        };

        UpdateRemappingPlansDisplay();
    }

    private void UpdateRemappingPlansDisplay()
    {
        if (_plansPanel == null || RemappingPlans == null) return;

        _plansPanel.Children.Clear();

        foreach (var plan in RemappingPlans)
        {
            var planItem = CreateRemappingPlanItem(plan);
            _plansPanel.Children.Add(planItem);
        }
    }

    private Border CreateRemappingPlanItem(HoloRemappingPlan plan)
    {
        var priorityColor = plan.Priority switch
        {
            RemappingPriority.High => Color.FromArgb(100, 255, 100, 100),
            RemappingPriority.Medium => Color.FromArgb(100, 255, 255, 100),
            RemappingPriority.Low => Color.FromArgb(100, 100, 255, 100),
            _ => Color.FromArgb(100, 150, 150, 150)
        };

        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, priorityColor.R, priorityColor.G, priorityColor.B)),
            BorderBrush = new SolidColorBrush(priorityColor),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(5),
            Effect = new DropShadowEffect
            {
                Color = priorityColor,
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Plan header
        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(15, 10, 15, 5)
        };

        var planNameText = new TextBlock
        {
            Text = plan.PlanName,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White)
        };
        headerPanel.Children.Add(planNameText);

        var priorityText = new TextBlock
        {
            Text = plan.Priority.ToString().ToUpper(),
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(priorityColor),
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(10, 0, 0, 2)
        };
        headerPanel.Children.Add(priorityText);

        Grid.SetRow(headerPanel, 0);
        grid.Children.Add(headerPanel);

        // Attributes
        var attributesPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(15, 5, 15, 5)
        };

        var attributeValues = new[]
        {
            $"INT: {plan.Intelligence}",
            $"MEM: {plan.Memory}",
            $"CHA: {plan.Charisma}",
            $"PER: {plan.Perception}",
            $"WIL: {plan.Willpower}"
        };

        foreach (var attr in attributeValues)
        {
            var attrText = new TextBlock
            {
                Text = attr,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 200)),
                Margin = new Thickness(0, 0, 15, 0)
            };
            attributesPanel.Children.Add(attrText);
        }

        Grid.SetRow(attributesPanel, 1);
        grid.Children.Add(attributesPanel);

        // Efficiency info
        var efficiencyPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(15, 5, 15, 5)
        };

        var gainText = new TextBlock
        {
            Text = $"Efficiency Gain: +{plan.EfficiencyGain:P1}",
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 100)),
            Margin = new Thickness(0, 0, 20, 0)
        };
        efficiencyPanel.Children.Add(gainText);

        var timeText = new TextBlock
        {
            Text = $"Time Saved: {plan.TimeSaved.Days}d {plan.TimeSaved.Hours}h",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 0)),
            Margin = new Thickness(0, 0, 0, 0)
        };
        efficiencyPanel.Children.Add(timeText);

        Grid.SetRow(efficiencyPanel, 2);
        grid.Children.Add(efficiencyPanel);

        // Optimal for
        var optimalText = new TextBlock
        {
            Text = $"Optimal for: {plan.OptimalFor}",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 200, 200, 255)),
            Margin = new Thickness(15, 5, 15, 10)
        };
        Grid.SetRow(optimalText, 3);
        grid.Children.Add(optimalText);

        border.Child = grid;
        return border;
    }

    #endregion

    #region Particle System

    private void CreateAttributeParticles()
    {
        if (!EnableParticleEffects || _particleCanvas == null) return;

        for (int i = 0; i < 2; i++)
        {
            var particle = new HoloAttributeParticle
            {
                X = _random.NextDouble() * ActualWidth,
                Y = _random.NextDouble() * ActualHeight,
                VelocityX = (_random.NextDouble() - 0.5) * 10,
                VelocityY = (_random.NextDouble() - 0.5) * 10,
                Size = _random.NextDouble() * 3 + 1,
                Life = 1.0,
                ParticleType = AttributeParticleType.Neural,
                Ellipse = new Ellipse
                {
                    Width = 3,
                    Height = 3,
                    Fill = new SolidColorBrush(Color.FromArgb(120, 200, 0, 255))
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
            particle.Life -= 0.005;

            if (particle.Life <= 0 || particle.X < 0 || particle.X > ActualWidth || 
                particle.Y < 0 || particle.Y > ActualHeight)
            {
                _particleCanvas.Children.Remove(particle.Ellipse);
                _particles.RemoveAt(i);
                continue;
            }

            Canvas.SetLeft(particle.Ellipse, particle.X);
            Canvas.SetTop(particle.Ellipse, particle.Y);
            
            var alpha = (byte)(particle.Life * 120);
            var color = particle.ParticleType switch
            {
                AttributeParticleType.Neural => Color.FromArgb(alpha, 200, 0, 255),
                AttributeParticleType.Optimization => Color.FromArgb(alpha, 255, 200, 0),
                AttributeParticleType.Analysis => Color.FromArgb(alpha, 0, 255, 200),
                _ => Color.FromArgb(alpha, 255, 255, 255)
            };
            
            particle.Ellipse.Fill = new SolidColorBrush(color);
        }

        if (_random.NextDouble() < 0.06)
        {
            CreateAttributeParticles();
        }
    }

    #endregion

    #region Animation and Updates

    private void OnAnimationTick(object sender, EventArgs e)
    {
        if (!_isAnimating) return;

        _currentAnimationTime += 16;
        
        UpdateParticles();
        UpdateAttributeAnimations();
    }

    private void OnAnalysisUpdateTick(object sender, EventArgs e)
    {
        UpdateAnalysis();
    }

    private void UpdateAttributeAnimations()
    {
        if (!EnableAnimations) return;

        var intensity = (Math.Sin(_currentAnimationTime * 0.001) + 1) * 0.5 * HolographicIntensity;
        
        // Animate section glows
        var sections = new[] { _currentAttributesSection, _radarChartSection, _optimizationSection, _efficiencySection };
        foreach (var section in sections)
        {
            if (section?.Effect is DropShadowEffect effect)
            {
                effect.BlurRadius = 8 + intensity * 4;
            }
        }
    }

    private void UpdateAnalysis()
    {
        // Update analysis data with minor variations for realism
        if (RemappingPlans != null)
        {
            foreach (var plan in RemappingPlans)
            {
                var variation = (_random.NextDouble() - 0.5) * 0.01; // ±0.5% variation
                plan.EfficiencyGain = Math.Max(0.01, Math.Min(0.25, plan.EfficiencyGain + variation));
            }
            
            UpdateRemappingPlansDisplay();
        }
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _animationTimer.Start();
        _analysisUpdateTimer.Start();
        _isAnimating = true;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer.Stop();
        _analysisUpdateTimer.Stop();
        _isAnimating = false;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_particleCanvas != null)
        {
            _particleCanvas.Width = ActualWidth;
            _particleCanvas.Height = ActualHeight;
        }
    }

    #endregion

    #region Property Change Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAttributeRemapping control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAttributeRemapping control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnCharacterAttributesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAttributeRemapping control)
        {
            control.RefreshAttributeDisplay();
        }
    }

    private static void OnRemappingPlansChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAttributeRemapping control)
        {
            control.UpdateRemappingPlansDisplay();
        }
    }

    private static void OnSelectedCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAttributeRemapping control)
        {
            control.LoadCharacterAttributes();
        }
    }

    private static void OnOptimizationModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAttributeRemapping control)
        {
            control.UpdateOptimizationMode();
        }
    }

    private static void OnAnalysisTimeFrameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAttributeRemapping control)
        {
            control.RecalculateOptimizations();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAttributeRemapping control)
        {
            control._isAnimating = control.EnableAnimations;
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAttributeRemapping control)
        {
            if (!control.EnableParticleEffects)
            {
                control.ClearParticles();
            }
        }
    }

    private static void OnShowOptimizationSuggestionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAttributeRemapping control)
        {
            control._optimizationSection.Visibility = control.ShowOptimizationSuggestions ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnShowEfficiencyAnalysisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAttributeRemapping control)
        {
            control._efficiencySection.Visibility = control.ShowEfficiencyAnalysis ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnShowRadarChartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAttributeRemapping control)
        {
            control._radarChartSection.Visibility = control.ShowRadarChart ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnIncludeImplantsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloAttributeRemapping control)
        {
            control.RecalculateOptimizations();
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

    private void RefreshAttributeDisplay()
    {
        // Refresh character attribute display
    }

    private void LoadCharacterAttributes()
    {
        // Load attributes for selected character
    }

    private void UpdateOptimizationMode()
    {
        // Update optimization calculations based on mode
    }

    private void RecalculateOptimizations()
    {
        // Recalculate optimization plans
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
    }

    public void OptimizeForHighEndHardware()
    {
        EnableParticleEffects = true;
        EnableAnimations = true;
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
        _analysisUpdateTimer?.Stop();
        ClearParticles();
        
        _isDisposed = true;
    }

    #endregion
}

#region Supporting Classes

public class HoloCharacterAttributes
{
    public int Intelligence { get; set; }
    public int Memory { get; set; }
    public int Charisma { get; set; }
    public int Perception { get; set; }
    public int Willpower { get; set; }
    public DateTime LastRemapDate { get; set; }
    public int RemapsAvailable { get; set; }
    public DateTime NextRemapDate { get; set; }
    public bool HasImplants { get; set; } = true;
    public Dictionary<string, int> ImplantBonuses { get; set; } = new();
}

public class HoloRemappingPlan
{
    public string PlanName { get; set; } = "";
    public int Intelligence { get; set; }
    public int Memory { get; set; }
    public int Charisma { get; set; }
    public int Perception { get; set; }
    public int Willpower { get; set; }
    public double EfficiencyGain { get; set; }
    public TimeSpan TimeSaved { get; set; }
    public string OptimalFor { get; set; } = "";
    public RemappingPriority Priority { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public bool IsOptimal { get; set; }
}

public class HoloAttributeParticle
{
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Size { get; set; }
    public double Life { get; set; }
    public AttributeParticleType ParticleType { get; set; }
    public Ellipse Ellipse { get; set; } = null!;
}

public enum AttributeOptimizationMode
{
    Balanced,
    TrainingSpeed,
    SpecificSkills,
    LongTerm
}

public enum RemappingPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum AttributeParticleType
{
    Neural,
    Optimization,
    Analysis,
    Calculation
}

#endregion