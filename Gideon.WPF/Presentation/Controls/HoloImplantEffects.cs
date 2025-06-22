// ==========================================================================
// HoloImplantEffects.cs - Holographic Implant Effect Visualization
// ==========================================================================
// Advanced implant analysis system featuring neural pathway visualization,
// effect analysis, EVE-style implant management, and holographic brain activity display.
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
/// Holographic implant effects visualization with neural pathway analysis and enhancement display
/// </summary>
public class HoloImplantEffects : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloImplantEffects),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloImplantEffects),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty ImplantSetsProperty =
        DependencyProperty.Register(nameof(ImplantSets), typeof(ObservableCollection<HoloImplantSet>), typeof(HoloImplantEffects),
            new PropertyMetadata(null, OnImplantSetsChanged));

    public static readonly DependencyProperty ActiveImplantsProperty =
        DependencyProperty.Register(nameof(ActiveImplants), typeof(ObservableCollection<HoloImplant>), typeof(HoloImplantEffects),
            new PropertyMetadata(null, OnActiveImplantsChanged));

    public static readonly DependencyProperty SelectedCharacterProperty =
        DependencyProperty.Register(nameof(SelectedCharacter), typeof(string), typeof(HoloImplantEffects),
            new PropertyMetadata("Main Character", OnSelectedCharacterChanged));

    public static readonly DependencyProperty VisualizationModeProperty =
        DependencyProperty.Register(nameof(VisualizationMode), typeof(ImplantVisualizationMode), typeof(HoloImplantEffects),
            new PropertyMetadata(ImplantVisualizationMode.NeuralPathways, OnVisualizationModeChanged));

    public static readonly DependencyProperty ShowNeuralActivityProperty =
        DependencyProperty.Register(nameof(ShowNeuralActivity), typeof(bool), typeof(HoloImplantEffects),
            new PropertyMetadata(true, OnShowNeuralActivityChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloImplantEffects),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloImplantEffects),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowEffectAnalysisProperty =
        DependencyProperty.Register(nameof(ShowEffectAnalysis), typeof(bool), typeof(HoloImplantEffects),
            new PropertyMetadata(true, OnShowEffectAnalysisChanged));

    public static readonly DependencyProperty ShowImplantRecommendationsProperty =
        DependencyProperty.Register(nameof(ShowImplantRecommendations), typeof(bool), typeof(HoloImplantEffects),
            new PropertyMetadata(true, OnShowImplantRecommendationsChanged));

    public static readonly DependencyProperty ShowCostAnalysisProperty =
        DependencyProperty.Register(nameof(ShowCostAnalysis), typeof(bool), typeof(HoloImplantEffects),
            new PropertyMetadata(true, OnShowCostAnalysisChanged));

    public static readonly DependencyProperty Show3DBrainModelProperty =
        DependencyProperty.Register(nameof(Show3DBrainModel), typeof(bool), typeof(HoloImplantEffects),
            new PropertyMetadata(true, OnShow3DBrainModelChanged));

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

    public ObservableCollection<HoloImplantSet> ImplantSets
    {
        get => (ObservableCollection<HoloImplantSet>)GetValue(ImplantSetsProperty);
        set => SetValue(ImplantSetsProperty, value);
    }

    public ObservableCollection<HoloImplant> ActiveImplants
    {
        get => (ObservableCollection<HoloImplant>)GetValue(ActiveImplantsProperty);
        set => SetValue(ActiveImplantsProperty, value);
    }

    public string SelectedCharacter
    {
        get => (string)GetValue(SelectedCharacterProperty);
        set => SetValue(SelectedCharacterProperty, value);
    }

    public ImplantVisualizationMode VisualizationMode
    {
        get => (ImplantVisualizationMode)GetValue(VisualizationModeProperty);
        set => SetValue(VisualizationModeProperty, value);
    }

    public bool ShowNeuralActivity
    {
        get => (bool)GetValue(ShowNeuralActivityProperty);
        set => SetValue(ShowNeuralActivityProperty, value);
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

    public bool ShowEffectAnalysis
    {
        get => (bool)GetValue(ShowEffectAnalysisProperty);
        set => SetValue(ShowEffectAnalysisProperty, value);
    }

    public bool ShowImplantRecommendations
    {
        get => (bool)GetValue(ShowImplantRecommendationsProperty);
        set => SetValue(ShowImplantRecommendationsProperty, value);
    }

    public bool ShowCostAnalysis
    {
        get => (bool)GetValue(ShowCostAnalysisProperty);
        set => SetValue(ShowCostAnalysisProperty, value);
    }

    public bool Show3DBrainModel
    {
        get => (bool)GetValue(Show3DBrainModelProperty);
        set => SetValue(Show3DBrainModelProperty, value);
    }

    #endregion

    #region Fields

    private Canvas _mainCanvas;
    private Canvas _particleCanvas;
    private Canvas _neuralCanvas;
    private Border _brainModelSection;
    private Border _activeImplantsSection;
    private Border _effectAnalysisSection;
    private Border _recommendationsSection;
    private ScrollViewer _implantSetsScrollViewer;
    private StackPanel _implantSetsPanel;
    private TabControl _implantTabs;
    
    private readonly List<HoloNeuralParticle> _particles = new();
    private readonly List<HoloNeuralPathway> _neuralPathways = new();
    private readonly DispatcherTimer _animationTimer;
    private readonly DispatcherTimer _neuralActivityTimer;
    private readonly Random _random = new();
    
    private bool _isAnimating = true;
    private bool _isDisposed = false;
    private double _currentAnimationTime = 0;

    #endregion

    #region Constructor

    public HoloImplantEffects()
    {
        InitializeComponent();
        
        _animationTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16) // 60 FPS
        };
        _animationTimer.Tick += OnAnimationTick;
        
        _neuralActivityTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100) // Neural activity updates
        };
        _neuralActivityTimer.Tick += OnNeuralActivityTick;
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 1000;
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
        
        _neuralCanvas = new Canvas
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
        _implantTabs = CreateImplantTabs();
        Grid.SetRow(_implantTabs, 1);
        contentGrid.Children.Add(_implantTabs);
        
        // Bottom analysis sections
        var bottomGrid = new Grid();
        bottomGrid.ColumnDefinitions.Add(new ColumnDefinition());
        bottomGrid.ColumnDefinitions.Add(new ColumnDefinition());
        bottomGrid.ColumnDefinitions.Add(new ColumnDefinition());
        
        _effectAnalysisSection = CreateEffectAnalysisSection();
        Grid.SetColumn(_effectAnalysisSection, 0);
        bottomGrid.Children.Add(_effectAnalysisSection);
        
        _recommendationsSection = CreateRecommendationsSection();
        Grid.SetColumn(_recommendationsSection, 1);
        bottomGrid.Children.Add(_recommendationsSection);
        
        var costAnalysisSection = CreateCostAnalysisSection();
        Grid.SetColumn(costAnalysisSection, 2);
        bottomGrid.Children.Add(costAnalysisSection);
        
        Grid.SetRow(bottomGrid, 2);
        contentGrid.Children.Add(bottomGrid);
        
        _mainCanvas.Children.Add(contentGrid);
        _mainCanvas.Children.Add(_neuralCanvas);
        _mainCanvas.Children.Add(_particleCanvas);
        border.Child = _mainCanvas;
        Content = border;

        CreateSampleData();
        InitializeNeuralPathways();
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
            Text = "NEURAL IMPLANT MATRIX",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        titlePanel.Children.Add(titleText);

        var subtitleText = new TextBlock
        {
            Text = "Cognitive Enhancement & Neural Pathway Analysis",
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
            Text = "NEURAL INTERFACE CONFIG",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 0, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8)
        };
        panel.Children.Add(configHeader);

        // Visualization mode
        var modePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 3, 0, 3)
        };

        var modeLabel = new TextBlock
        {
            Text = "Visualization:",
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
        modeCombo.Items.Add("Neural Pathways");
        modeCombo.Items.Add("Effect Analysis");
        modeCombo.Items.Add("3D Brain Model");
        modeCombo.Items.Add("Slot Overview");
        modeCombo.SelectedIndex = 0;
        modePanel.Children.Add(modeCombo);

        panel.Children.Add(modePanel);

        // Neural activity checkbox
        var neuralCheck = new CheckBox
        {
            Content = "Show Neural Activity",
            FontSize = 9,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            IsChecked = true,
            Margin = new Thickness(0, 4, 0, 2)
        };
        panel.Children.Add(neuralCheck);

        // Effects analysis checkbox
        var effectsCheck = new CheckBox
        {
            Content = "Effect Analysis",
            FontSize = 9,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            IsChecked = true,
            Margin = new Thickness(0, 2, 0, 2)
        };
        panel.Children.Add(effectsCheck);

        // Cost analysis checkbox
        var costCheck = new CheckBox
        {
            Content = "Cost Analysis",
            FontSize = 9,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            IsChecked = true,
            Margin = new Thickness(0, 2, 0, 2)
        };
        panel.Children.Add(costCheck);

        return panel;
    }

    private TabControl CreateImplantTabs()
    {
        var tabControl = new TabControl
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(80, 0, 200, 255)),
            Margin = new Thickness(10)
        };

        // Active Implants Tab
        var activeTab = new TabItem
        {
            Header = "Active",
            Foreground = new SolidColorBrush(Colors.White),
            Background = new SolidColorBrush(Color.FromArgb(60, 0, 100, 150))
        };
        activeTab.Content = CreateActiveImplantsContent();
        tabControl.Items.Add(activeTab);

        // Brain Model Tab
        var brainTab = new TabItem
        {
            Header = "Neural Map",
            Foreground = new SolidColorBrush(Colors.White),
            Background = new SolidColorBrush(Color.FromArgb(60, 0, 100, 150))
        };
        brainTab.Content = CreateBrainModelContent();
        tabControl.Items.Add(brainTab);

        // Implant Sets Tab
        var setsTab = new TabItem
        {
            Header = "Sets",
            Foreground = new SolidColorBrush(Colors.White),
            Background = new SolidColorBrush(Color.FromArgb(60, 0, 100, 150))
        };
        setsTab.Content = CreateImplantSetsContent();
        tabControl.Items.Add(setsTab);

        return tabControl;
    }

    private Grid CreateActiveImplantsContent()
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(400, GridUnitType.Pixel) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Implant slots
        var slotsPanel = CreateImplantSlotsPanel();
        Grid.SetColumn(slotsPanel, 0);
        grid.Children.Add(slotsPanel);

        // Effects summary
        var effectsPanel = CreateEffectsSummaryPanel();
        Grid.SetColumn(effectsPanel, 1);
        grid.Children.Add(effectsPanel);

        return grid;
    }

    private StackPanel CreateImplantSlotsPanel()
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(20, 15, 10, 15)
        };

        var slotsHeader = new TextBlock
        {
            Text = "NEURAL INTERFACE SLOTS",
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 0, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 15)
        };
        panel.Children.Add(slotsHeader);

        var implantSlots = new[]
        {
            new { Slot = 1, Name = "Memory Augmentation - Standard", Bonus = "+4 Memory", Quality = "High" },
            new { Slot = 2, Name = "Neural Boost - Standard", Bonus = "+4 Intelligence", Quality = "High" },
            new { Slot = 3, Name = "Ocular Filter - Standard", Bonus = "+4 Perception", Quality = "High" },
            new { Slot = 4, Name = "Social Adaptation Chip", Bonus = "+3 Charisma", Quality = "Medium" },
            new { Slot = 5, Name = "Cybernetic Subprocessor", Bonus = "+3 Willpower", Quality = "Medium" },
            new { Slot = 6, Name = "EMPTY SLOT", Bonus = "No Enhancement", Quality = "None" },
            new { Slot = 7, Name = "EMPTY SLOT", Bonus = "No Enhancement", Quality = "None" },
            new { Slot = 8, Name = "EMPTY SLOT", Bonus = "No Enhancement", Quality = "None" },
            new { Slot = 9, Name = "EMPTY SLOT", Bonus = "No Enhancement", Quality = "None" },
            new { Slot = 10, Name = "EMPTY SLOT", Bonus = "No Enhancement", Quality = "None" }
        };

        foreach (var slot in implantSlots)
        {
            var slotItem = CreateImplantSlotItem(slot.Slot, slot.Name, slot.Bonus, slot.Quality);
            panel.Children.Add(slotItem);
        }

        return panel;
    }

    private Border CreateImplantSlotItem(int slot, string name, string bonus, string quality)
    {
        var isEmpty = name.Contains("EMPTY");
        var qualityColor = quality switch
        {
            "High" => Color.FromArgb(100, 0, 255, 100),
            "Medium" => Color.FromArgb(100, 255, 255, 0),
            "Low" => Color.FromArgb(100, 255, 100, 0),
            _ => Color.FromArgb(100, 100, 100, 100)
        };

        var border = new Border
        {
            Background = new SolidColorBrush(isEmpty ? Color.FromArgb(20, 100, 100, 100) : Color.FromArgb(40, qualityColor.R, qualityColor.G, qualityColor.B)),
            BorderBrush = new SolidColorBrush(isEmpty ? Color.FromArgb(60, 100, 100, 100) : qualityColor),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3),
            Margin = new Thickness(5, 2, 5, 2),
            Effect = isEmpty ? null : new DropShadowEffect
            {
                Color = qualityColor,
                ShadowDepth = 0,
                BlurRadius = 6
            }
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40, GridUnitType.Pixel) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80, GridUnitType.Pixel) });

        // Slot number
        var slotText = new TextBlock
        {
            Text = slot.ToString(),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(isEmpty ? Color.FromArgb(150, 150, 150, 150) : Color.FromArgb(255, 255, 255, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 6, 5, 6)
        };
        Grid.SetColumn(slotText, 0);
        grid.Children.Add(slotText);

        // Implant name
        var nameText = new TextBlock
        {
            Text = name,
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(isEmpty ? Color.FromArgb(150, 150, 150, 150) : Colors.White),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 6, 5, 6),
            TextWrapping = TextWrapping.Wrap
        };
        Grid.SetColumn(nameText, 1);
        grid.Children.Add(nameText);

        // Bonus
        var bonusText = new TextBlock
        {
            Text = bonus,
            FontSize = 9,
            Foreground = new SolidColorBrush(isEmpty ? Color.FromArgb(150, 150, 150, 150) : qualityColor),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 6, 8, 6)
        };
        Grid.SetColumn(bonusText, 2);
        grid.Children.Add(bonusText);

        border.Child = grid;
        return border;
    }

    private StackPanel CreateEffectsSummaryPanel()
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10, 15, 20, 15)
        };

        var summaryHeader = new TextBlock
        {
            Text = "ENHANCEMENT SUMMARY",
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 200)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 15)
        };
        panel.Children.Add(summaryHeader);

        // Current totals
        var totalsPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10, 0, 10, 15)
        };

        var attributeTotals = new[]
        {
            new { Attribute = "Intelligence", Base = 20, Implant = 4, Total = 24 },
            new { Attribute = "Memory", Base = 21, Implant = 4, Total = 25 },
            new { Attribute = "Perception", Base = 20, Implant = 4, Total = 24 },
            new { Attribute = "Charisma", Base = 19, Implant = 3, Total = 22 },
            new { Attribute = "Willpower", Base = 22, Implant = 3, Total = 25 }
        };

        foreach (var attr in attributeTotals)
        {
            var attrPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 3, 0, 3)
            };

            var attrLabel = new TextBlock
            {
                Text = $"{attr.Attribute}:",
                FontSize = 11,
                Foreground = new SolidColorBrush(Colors.White),
                Width = 100
            };
            attrPanel.Children.Add(attrLabel);

            var baseText = new TextBlock
            {
                Text = attr.Base.ToString(),
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 200, 200)),
                Width = 25,
                TextAlignment = TextAlignment.Center
            };
            attrPanel.Children.Add(baseText);

            var plusText = new TextBlock
            {
                Text = "+",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromArgb(180, 150, 150, 150)),
                Width = 15,
                TextAlignment = TextAlignment.Center
            };
            attrPanel.Children.Add(plusText);

            var implantText = new TextBlock
            {
                Text = attr.Implant.ToString(),
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 100)),
                Width = 25,
                TextAlignment = TextAlignment.Center
            };
            attrPanel.Children.Add(implantText);

            var equalsText = new TextBlock
            {
                Text = "=",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromArgb(180, 150, 150, 150)),
                Width = 15,
                TextAlignment = TextAlignment.Center
            };
            attrPanel.Children.Add(equalsText);

            var totalText = new TextBlock
            {
                Text = attr.Total.ToString(),
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
                Width = 30,
                TextAlignment = TextAlignment.Center
            };
            attrPanel.Children.Add(totalText);

            totalsPanel.Children.Add(attrPanel);
        }

        panel.Children.Add(totalsPanel);

        // Training efficiency
        var efficiencyPanel = CreateTrainingEfficiencyPanel();
        panel.Children.Add(efficiencyPanel);

        return panel;
    }

    private Border CreateTrainingEfficiencyPanel()
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 150, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(80, 0, 255, 150)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3),
            Margin = new Thickness(0, 10, 0, 0),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(60, 0, 255, 150),
                ShadowDepth = 0,
                BlurRadius = 6
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
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 150)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8)
        };
        panel.Children.Add(header);

        var efficiency = new[]
        {
            "Base SP/Hour: 1,800",
            "Enhanced SP/Hour: 2,547",
            "Efficiency Boost: +41.5%",
            "Optimal Configuration: 94%",
            "Upgrade Potential: +12%",
            "Cost per SP/Hour: 2.1M ISK"
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

    private Canvas CreateBrainModelContent()
    {
        var canvas = new Canvas
        {
            Background = new SolidColorBrush(Color.FromArgb(20, 0, 50, 100)),
            Margin = new Thickness(10)
        };

        Create3DBrainModel(canvas);
        return canvas;
    }

    private void Create3DBrainModel(Canvas canvas)
    {
        canvas.Width = 600;
        canvas.Height = 400;

        var centerX = canvas.Width / 2;
        var centerY = canvas.Height / 2;

        // Brain outline (simplified)
        var brainPath = new Path
        {
            Stroke = new SolidColorBrush(Color.FromArgb(150, 0, 255, 255)),
            StrokeThickness = 2,
            Fill = new SolidColorBrush(Color.FromArgb(30, 0, 200, 255)),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(100, 0, 255, 255),
                ShadowDepth = 0,
                BlurRadius = 10
            }
        };

        // Create a simple brain shape using geometry
        var brainGeometry = new EllipseGeometry
        {
            Center = new Point(centerX, centerY),
            RadiusX = 150,
            RadiusY = 120
        };
        brainPath.Data = brainGeometry;
        canvas.Children.Add(brainPath);

        // Implant nodes with neural connections
        var implantPositions = new[]
        {
            new { X = centerX - 80, Y = centerY - 60, Name = "Memory", Active = true },
            new { X = centerX + 80, Y = centerY - 60, Name = "Intelligence", Active = true },
            new { X = centerX - 100, Y = centerY, Name = "Perception", Active = true },
            new { X = centerX + 100, Y = centerY, Name = "Charisma", Active = true },
            new { X = centerX, Y = centerY + 80, Name = "Willpower", Active = true },
            new { X = centerX - 40, Y = centerY - 20, Name = "Slot 6", Active = false },
            new { X = centerX + 40, Y = centerY - 20, Name = "Slot 7", Active = false },
            new { X = centerX, Y = centerY + 20, Name = "Slot 8", Active = false }
        };

        foreach (var pos in implantPositions)
        {
            var implantNode = new Ellipse
            {
                Width = pos.Active ? 16 : 10,
                Height = pos.Active ? 16 : 10,
                Fill = new SolidColorBrush(pos.Active ? Color.FromArgb(255, 0, 255, 100) : Color.FromArgb(100, 100, 100, 100)),
                Stroke = new SolidColorBrush(pos.Active ? Color.FromArgb(255, 0, 255, 200) : Color.FromArgb(150, 150, 150, 150)),
                StrokeThickness = 2
            };

            if (pos.Active)
            {
                implantNode.Effect = new DropShadowEffect
                {
                    Color = Color.FromArgb(200, 0, 255, 100),
                    ShadowDepth = 0,
                    BlurRadius = 12
                };
            }

            Canvas.SetLeft(implantNode, pos.X - implantNode.Width / 2);
            Canvas.SetTop(implantNode, pos.Y - implantNode.Height / 2);
            canvas.Children.Add(implantNode);

            // Label
            var label = new TextBlock
            {
                Text = pos.Name,
                FontSize = 8,
                Foreground = new SolidColorBrush(pos.Active ? Colors.White : Color.FromArgb(150, 150, 150, 150)),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Canvas.SetLeft(label, pos.X - pos.Name.Length * 2);
            Canvas.SetTop(label, pos.Y + 12);
            canvas.Children.Add(label);

            // Neural connections (only for active implants)
            if (pos.Active)
            {
                for (int i = 0; i < 3; i++)
                {
                    var connectionLine = new Line
                    {
                        X1 = pos.X,
                        Y1 = pos.Y,
                        X2 = centerX + (_random.NextDouble() - 0.5) * 60,
                        Y2 = centerY + (_random.NextDouble() - 0.5) * 40,
                        Stroke = new SolidColorBrush(Color.FromArgb(80, 0, 255, 150)),
                        StrokeThickness = 1,
                        StrokeDashArray = new DoubleCollection { 2, 2 }
                    };
                    canvas.Children.Add(connectionLine);
                }
            }
        }

        // Central processing core
        var core = new Ellipse
        {
            Width = 20,
            Height = 20,
            Fill = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
            Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 200, 0)),
            StrokeThickness = 2,
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(200, 255, 255, 0),
                ShadowDepth = 0,
                BlurRadius = 15
            }
        };
        Canvas.SetLeft(core, centerX - 10);
        Canvas.SetTop(core, centerY - 10);
        canvas.Children.Add(core);

        var coreLabel = new TextBlock
        {
            Text = "NEURAL CORE",
            FontSize = 8,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Canvas.SetLeft(coreLabel, centerX - 30);
        Canvas.SetTop(coreLabel, centerY + 15);
        canvas.Children.Add(coreLabel);
    }

    private ScrollViewer CreateImplantSetsContent()
    {
        _implantSetsScrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = new SolidColorBrush(Color.FromArgb(20, 0, 50, 100))
        };

        _implantSetsPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10)
        };

        _implantSetsScrollViewer.Content = _implantSetsPanel;
        return _implantSetsScrollViewer;
    }

    private Border CreateEffectAnalysisSection()
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
            Text = "NEURAL ANALYSIS",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        panel.Children.Add(header);

        var analysis = new[]
        {
            "Active Implants: 5/10",
            "Neural Load: 68%",
            "Efficiency Rating: 94%",
            "Synaptic Activity: High",
            "Enhancement Stability: 97%",
            "Upgrade Potential: Medium",
            "Cognitive Boost: +41.5%",
            "Risk Assessment: Low"
        };

        foreach (var stat in analysis)
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

    private Border CreateRecommendationsSection()
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
            Text = "ENHANCEMENT RECOMMENDATIONS",
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        panel.Children.Add(header);

        var recommendations = new[]
        {
            "• Upgrade to +5 implant set",
            "• Install slot 6-10 implants",
            "• Consider specialized sets",
            "• Monitor neural stability",
            "• Plan for attribute remap",
            "• Review cost/benefit ratio",
            "• Check market prices",
            "• Backup current config"
        };

        foreach (var rec in recommendations)
        {
            var recText = new TextBlock
            {
                Text = rec,
                FontSize = 9,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 200)),
                Margin = new Thickness(0, 2, 0, 0)
            };
            panel.Children.Add(recText);
        }

        border.Child = panel;
        return border;
    }

    private Border CreateCostAnalysisSection()
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
            Text = "COST ANALYSIS",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 200, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        panel.Children.Add(header);

        var costs = new[]
        {
            "Current Set Value: 247M ISK",
            "Upgrade Cost: 1.2B ISK",
            "ROI Period: 45 days",
            "SP/Hour Improvement: +352",
            "Time Value: 8.3M ISK/day",
            "Market Efficiency: 92%",
            "Insurance Value: 65%",
            "Replacement Cost: 890M"
        };

        foreach (var cost in costs)
        {
            var costText = new TextBlock
            {
                Text = cost,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 200)),
                Margin = new Thickness(0, 2, 0, 0)
            };
            panel.Children.Add(costText);
        }

        border.Child = panel;
        return border;
    }

    private void CreateSampleData()
    {
        ActiveImplants = new ObservableCollection<HoloImplant>
        {
            new HoloImplant
            {
                Slot = 1,
                Name = "Memory Augmentation - Standard",
                AttributeBonus = "Memory +4",
                Quality = ImplantQuality.High,
                Value = 45000000,
                IsActive = true
            },
            new HoloImplant
            {
                Slot = 2,
                Name = "Neural Boost - Standard",
                AttributeBonus = "Intelligence +4",
                Quality = ImplantQuality.High,
                Value = 48000000,
                IsActive = true
            },
            new HoloImplant
            {
                Slot = 3,
                Name = "Ocular Filter - Standard",
                AttributeBonus = "Perception +4",
                Quality = ImplantQuality.High,
                Value = 52000000,
                IsActive = true
            },
            new HoloImplant
            {
                Slot = 4,
                Name = "Social Adaptation Chip - Standard",
                AttributeBonus = "Charisma +3",
                Quality = ImplantQuality.Medium,
                Value = 12000000,
                IsActive = true
            },
            new HoloImplant
            {
                Slot = 5,
                Name = "Cybernetic Subprocessor - Standard",
                AttributeBonus = "Willpower +3",
                Quality = ImplantQuality.Medium,
                Value = 15000000,
                IsActive = true
            }
        };

        ImplantSets = new ObservableCollection<HoloImplantSet>
        {
            new HoloImplantSet
            {
                SetName = "High-Grade Virtue",
                TotalCost = 1200000000,
                EfficiencyBonus = 0.25,
                Specialization = "Neural Enhancement",
                SetBonus = "All attributes +5"
            },
            new HoloImplantSet
            {
                SetName = "Mid-Grade Ascendancy",
                TotalCost = 450000000,
                EfficiencyBonus = 0.15,
                Specialization = "Training Speed",
                SetBonus = "Training time -20%"
            },
            new HoloImplantSet
            {
                SetName = "Low-Grade Slave",
                TotalCost = 180000000,
                EfficiencyBonus = 0.08,
                Specialization = "Combat Focus",
                SetBonus = "Combat bonuses"
            }
        };

        UpdateImplantSetsDisplay();
    }

    private void UpdateImplantSetsDisplay()
    {
        if (_implantSetsPanel == null || ImplantSets == null) return;

        _implantSetsPanel.Children.Clear();

        foreach (var set in ImplantSets)
        {
            var setItem = CreateImplantSetItem(set);
            _implantSetsPanel.Children.Add(setItem);
        }
    }

    private Border CreateImplantSetItem(HoloImplantSet set)
    {
        var qualityColor = set.TotalCost switch
        {
            > 1000000000 => Color.FromArgb(100, 255, 0, 255), // Purple for expensive
            > 400000000 => Color.FromArgb(100, 0, 255, 255), // Blue for mid-range
            _ => Color.FromArgb(100, 0, 255, 100) // Green for budget
        };

        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, qualityColor.R, qualityColor.G, qualityColor.B)),
            BorderBrush = new SolidColorBrush(qualityColor),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(5),
            Effect = new DropShadowEffect
            {
                Color = qualityColor,
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Set name
        var nameText = new TextBlock
        {
            Text = set.SetName,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(15, 10, 15, 5)
        };
        Grid.SetRow(nameText, 0);
        grid.Children.Add(nameText);

        // Cost and efficiency
        var infoGrid = new Grid();
        infoGrid.ColumnDefinitions.Add(new ColumnDefinition());
        infoGrid.ColumnDefinitions.Add(new ColumnDefinition());

        var costText = new TextBlock
        {
            Text = $"Cost: {set.TotalCost / 1000000:F0}M ISK",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 0)),
            Margin = new Thickness(15, 2, 5, 2)
        };
        Grid.SetColumn(costText, 0);
        infoGrid.Children.Add(costText);

        var efficiencyText = new TextBlock
        {
            Text = $"Boost: +{set.EfficiencyBonus:P0}",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 0, 255, 100)),
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(5, 2, 15, 2)
        };
        Grid.SetColumn(efficiencyText, 1);
        infoGrid.Children.Add(efficiencyText);

        Grid.SetRow(infoGrid, 1);
        grid.Children.Add(infoGrid);

        // Specialization
        var specText = new TextBlock
        {
            Text = $"Specialization: {set.Specialization}",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 200, 200, 255)),
            Margin = new Thickness(15, 2, 15, 2)
        };
        Grid.SetRow(specText, 2);
        grid.Children.Add(specText);

        // Set bonus
        var bonusText = new TextBlock
        {
            Text = set.SetBonus,
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(qualityColor),
            Margin = new Thickness(15, 2, 15, 10)
        };
        Grid.SetRow(bonusText, 3);
        grid.Children.Add(bonusText);

        border.Child = grid;
        return border;
    }

    private void InitializeNeuralPathways()
    {
        // Create neural pathways for visualization
        var pathwayCount = 8;
        for (int i = 0; i < pathwayCount; i++)
        {
            var pathway = new HoloNeuralPathway
            {
                StartX = _random.NextDouble() * ActualWidth,
                StartY = _random.NextDouble() * ActualHeight,
                EndX = _random.NextDouble() * ActualWidth,
                EndY = _random.NextDouble() * ActualHeight,
                Activity = _random.NextDouble(),
                PathwayType = (NeuralPathwayType)_random.Next(0, 3)
            };
            _neuralPathways.Add(pathway);
        }
    }

    #endregion

    #region Particle System

    private void CreateNeuralParticles()
    {
        if (!EnableParticleEffects || _particleCanvas == null) return;

        for (int i = 0; i < 3; i++)
        {
            var particle = new HoloNeuralParticle
            {
                X = _random.NextDouble() * ActualWidth,
                Y = _random.NextDouble() * ActualHeight,
                VelocityX = (_random.NextDouble() - 0.5) * 8,
                VelocityY = (_random.NextDouble() - 0.5) * 8,
                Size = _random.NextDouble() * 4 + 1,
                Life = 1.0,
                ParticleType = NeuralParticleType.Synaptic,
                Ellipse = new Ellipse
                {
                    Width = 4,
                    Height = 4,
                    Fill = new SolidColorBrush(Color.FromArgb(100, 200, 0, 255))
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
            particle.Life -= 0.004;

            if (particle.Life <= 0 || particle.X < 0 || particle.X > ActualWidth || 
                particle.Y < 0 || particle.Y > ActualHeight)
            {
                _particleCanvas.Children.Remove(particle.Ellipse);
                _particles.RemoveAt(i);
                continue;
            }

            Canvas.SetLeft(particle.Ellipse, particle.X);
            Canvas.SetTop(particle.Ellipse, particle.Y);
            
            var alpha = (byte)(particle.Life * 100);
            var color = particle.ParticleType switch
            {
                NeuralParticleType.Synaptic => Color.FromArgb(alpha, 200, 0, 255),
                NeuralParticleType.Enhancement => Color.FromArgb(alpha, 0, 255, 200),
                NeuralParticleType.Cognitive => Color.FromArgb(alpha, 255, 200, 0),
                _ => Color.FromArgb(alpha, 255, 255, 255)
            };
            
            particle.Ellipse.Fill = new SolidColorBrush(color);
        }

        if (_random.NextDouble() < 0.08)
        {
            CreateNeuralParticles();
        }
    }

    private void UpdateNeuralPathways()
    {
        if (!ShowNeuralActivity || _neuralCanvas == null) return;

        foreach (var pathway in _neuralPathways)
        {
            // Update pathway activity
            pathway.Activity += (_random.NextDouble() - 0.5) * 0.1;
            pathway.Activity = Math.Max(0, Math.Min(1, pathway.Activity));

            // Create activity pulses
            if (_random.NextDouble() < pathway.Activity * 0.1)
            {
                var pulse = new Ellipse
                {
                    Width = 6,
                    Height = 6,
                    Fill = new SolidColorBrush(Color.FromArgb(150, 0, 255, 200)),
                    Effect = new DropShadowEffect
                    {
                        Color = Color.FromArgb(200, 0, 255, 200),
                        ShadowDepth = 0,
                        BlurRadius = 8
                    }
                };

                var startX = pathway.StartX + (_random.NextDouble() - 0.5) * 20;
                var startY = pathway.StartY + (_random.NextDouble() - 0.5) * 20;

                Canvas.SetLeft(pulse, startX);
                Canvas.SetTop(pulse, startY);
                _neuralCanvas.Children.Add(pulse);

                // Animate pulse along pathway
                var animation = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = TimeSpan.FromSeconds(2),
                    EasingFunction = new QuadraticEase()
                };

                animation.Completed += (s, e) => _neuralCanvas.Children.Remove(pulse);
                pulse.BeginAnimation(OpacityProperty, animation);
            }
        }
    }

    #endregion

    #region Animation and Updates

    private void OnAnimationTick(object sender, EventArgs e)
    {
        if (!_isAnimating) return;

        _currentAnimationTime += 16;
        
        UpdateParticles();
        UpdateImplantAnimations();
    }

    private void OnNeuralActivityTick(object sender, EventArgs e)
    {
        UpdateNeuralPathways();
    }

    private void UpdateImplantAnimations()
    {
        if (!EnableAnimations) return;

        var intensity = (Math.Sin(_currentAnimationTime * 0.0008) + 1) * 0.5 * HolographicIntensity;
        
        // Animate section glows
        var sections = new[] { _brainModelSection, _activeImplantsSection, _effectAnalysisSection, _recommendationsSection };
        foreach (var section in sections)
        {
            if (section?.Effect is DropShadowEffect effect)
            {
                effect.BlurRadius = 8 + intensity * 4;
            }
        }
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _animationTimer.Start();
        _neuralActivityTimer.Start();
        _isAnimating = true;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer.Stop();
        _neuralActivityTimer.Stop();
        _isAnimating = false;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_particleCanvas != null)
        {
            _particleCanvas.Width = ActualWidth;
            _particleCanvas.Height = ActualHeight;
        }
        if (_neuralCanvas != null)
        {
            _neuralCanvas.Width = ActualWidth;
            _neuralCanvas.Height = ActualHeight;
        }
    }

    #endregion

    #region Property Change Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloImplantEffects control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloImplantEffects control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnImplantSetsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloImplantEffects control)
        {
            control.UpdateImplantSetsDisplay();
        }
    }

    private static void OnActiveImplantsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloImplantEffects control)
        {
            control.RefreshActiveImplants();
        }
    }

    private static void OnSelectedCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloImplantEffects control)
        {
            control.LoadCharacterImplants();
        }
    }

    private static void OnVisualizationModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloImplantEffects control)
        {
            control.UpdateVisualizationMode();
        }
    }

    private static void OnShowNeuralActivityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloImplantEffects control)
        {
            if (!control.ShowNeuralActivity)
            {
                control._neuralCanvas.Children.Clear();
            }
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloImplantEffects control)
        {
            control._isAnimating = control.EnableAnimations;
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloImplantEffects control)
        {
            if (!control.EnableParticleEffects)
            {
                control.ClearParticles();
            }
        }
    }

    private static void OnShowEffectAnalysisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloImplantEffects control)
        {
            control._effectAnalysisSection.Visibility = control.ShowEffectAnalysis ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnShowImplantRecommendationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloImplantEffects control)
        {
            control._recommendationsSection.Visibility = control.ShowImplantRecommendations ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnShowCostAnalysisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloImplantEffects control)
        {
            // Cost analysis section visibility handled in CreateCostAnalysisSection
        }
    }

    private static void OnShow3DBrainModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloImplantEffects control)
        {
            control._brainModelSection.Visibility = control.Show3DBrainModel ? Visibility.Visible : Visibility.Collapsed;
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

    private void RefreshActiveImplants()
    {
        // Refresh active implants display
    }

    private void LoadCharacterImplants()
    {
        // Load implants for selected character
    }

    private void UpdateVisualizationMode()
    {
        // Update visualization based on mode
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
        ShowNeuralActivity = false;
    }

    public void OptimizeForHighEndHardware()
    {
        EnableParticleEffects = true;
        EnableAnimations = true;
        ShowNeuralActivity = true;
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
        _neuralActivityTimer?.Stop();
        ClearParticles();
        
        _isDisposed = true;
    }

    #endregion
}

#region Supporting Classes

public class HoloImplant
{
    public int Slot { get; set; }
    public string Name { get; set; } = "";
    public string AttributeBonus { get; set; } = "";
    public ImplantQuality Quality { get; set; }
    public long Value { get; set; }
    public bool IsActive { get; set; }
    public DateTime InstallDate { get; set; } = DateTime.Now;
    public string Description { get; set; } = "";
    public List<string> Effects { get; set; } = new();
}

public class HoloImplantSet
{
    public string SetName { get; set; } = "";
    public long TotalCost { get; set; }
    public double EfficiencyBonus { get; set; }
    public string Specialization { get; set; } = "";
    public string SetBonus { get; set; } = "";
    public List<HoloImplant> Implants { get; set; } = new();
    public bool IsComplete { get; set; }
}

public class HoloNeuralParticle
{
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Size { get; set; }
    public double Life { get; set; }
    public NeuralParticleType ParticleType { get; set; }
    public Ellipse Ellipse { get; set; } = null!;
}

public class HoloNeuralPathway
{
    public double StartX { get; set; }
    public double StartY { get; set; }
    public double EndX { get; set; }
    public double EndY { get; set; }
    public double Activity { get; set; }
    public NeuralPathwayType PathwayType { get; set; }
}

public enum ImplantVisualizationMode
{
    NeuralPathways,
    EffectAnalysis,
    BrainModel3D,
    SlotOverview
}

public enum ImplantQuality
{
    Low,
    Medium,
    High,
    Elite
}

public enum NeuralParticleType
{
    Synaptic,
    Enhancement,
    Cognitive,
    Neural
}

public enum NeuralPathwayType
{
    Memory,
    Processing,
    Enhancement
}

#endregion