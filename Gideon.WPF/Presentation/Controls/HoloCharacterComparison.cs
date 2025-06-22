// ==========================================================================
// HoloCharacterComparison.cs - Holographic Character Comparison Tools
// ==========================================================================
// Advanced character comparison system featuring multi-character analysis,
// skill gap identification, EVE-style competitive metrics, and holographic visualization.
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
/// Holographic character comparison tools with comprehensive multi-character analysis and skill gap identification
/// </summary>
public class HoloCharacterComparison : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloCharacterComparison),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloCharacterComparison),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty ComparisonCharactersProperty =
        DependencyProperty.Register(nameof(ComparisonCharacters), typeof(ObservableCollection<HoloComparisonCharacter>), typeof(HoloCharacterComparison),
            new PropertyMetadata(null, OnComparisonCharactersChanged));

    public static readonly DependencyProperty ComparisonResultsProperty =
        DependencyProperty.Register(nameof(ComparisonResults), typeof(HoloComparisonResults), typeof(HoloCharacterComparison),
            new PropertyMetadata(null, OnComparisonResultsChanged));

    public static readonly DependencyProperty ComparisonModeProperty =
        DependencyProperty.Register(nameof(ComparisonMode), typeof(ComparisonMode), typeof(HoloCharacterComparison),
            new PropertyMetadata(ComparisonMode.SideBySide, OnComparisonModeChanged));

    public static readonly DependencyProperty ComparisonCategoryProperty =
        DependencyProperty.Register(nameof(ComparisonCategory), typeof(ComparisonCategory), typeof(HoloCharacterComparison),
            new PropertyMetadata(ComparisonCategory.All, OnComparisonCategoryChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloCharacterComparison),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloCharacterComparison),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowSkillGapsProperty =
        DependencyProperty.Register(nameof(ShowSkillGaps), typeof(bool), typeof(HoloCharacterComparison),
            new PropertyMetadata(true, OnShowSkillGapsChanged));

    public static readonly DependencyProperty ShowRadarChartsProperty =
        DependencyProperty.Register(nameof(ShowRadarCharts), typeof(bool), typeof(HoloCharacterComparison),
            new PropertyMetadata(true, OnShowRadarChartsChanged));

    public static readonly DependencyProperty ShowDetailedMetricsProperty =
        DependencyProperty.Register(nameof(ShowDetailedMetrics), typeof(bool), typeof(HoloCharacterComparison),
            new PropertyMetadata(true, OnShowDetailedMetricsChanged));

    public static readonly DependencyProperty ShowRecommendationsProperty =
        DependencyProperty.Register(nameof(ShowRecommendations), typeof(bool), typeof(HoloCharacterComparison),
            new PropertyMetadata(true, OnShowRecommendationsChanged));

    public static readonly DependencyProperty AutoAnalyzeProperty =
        DependencyProperty.Register(nameof(AutoAnalyze), typeof(bool), typeof(HoloCharacterComparison),
            new PropertyMetadata(true, OnAutoAnalyzeChanged));

    public static readonly DependencyProperty HighlightDifferencesProperty =
        DependencyProperty.Register(nameof(HighlightDifferences), typeof(bool), typeof(HoloCharacterComparison),
            new PropertyMetadata(true, OnHighlightDifferencesChanged));

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

    public ObservableCollection<HoloComparisonCharacter> ComparisonCharacters
    {
        get => (ObservableCollection<HoloComparisonCharacter>)GetValue(ComparisonCharactersProperty);
        set => SetValue(ComparisonCharactersProperty, value);
    }

    public HoloComparisonResults ComparisonResults
    {
        get => (HoloComparisonResults)GetValue(ComparisonResultsProperty);
        set => SetValue(ComparisonResultsProperty, value);
    }

    public ComparisonMode ComparisonMode
    {
        get => (ComparisonMode)GetValue(ComparisonModeProperty);
        set => SetValue(ComparisonModeProperty, value);
    }

    public ComparisonCategory ComparisonCategory
    {
        get => (ComparisonCategory)GetValue(ComparisonCategoryProperty);
        set => SetValue(ComparisonCategoryProperty, value);
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

    public bool ShowSkillGaps
    {
        get => (bool)GetValue(ShowSkillGapsProperty);
        set => SetValue(ShowSkillGapsProperty, value);
    }

    public bool ShowRadarCharts
    {
        get => (bool)GetValue(ShowRadarChartsProperty);
        set => SetValue(ShowRadarChartsProperty, value);
    }

    public bool ShowDetailedMetrics
    {
        get => (bool)GetValue(ShowDetailedMetricsProperty);
        set => SetValue(ShowDetailedMetricsProperty, value);
    }

    public bool ShowRecommendations
    {
        get => (bool)GetValue(ShowRecommendationsProperty);
        set => SetValue(ShowRecommendationsProperty, value);
    }

    public bool AutoAnalyze
    {
        get => (bool)GetValue(AutoAnalyzeProperty);
        set => SetValue(AutoAnalyzeProperty, value);
    }

    public bool HighlightDifferences
    {
        get => (bool)GetValue(HighlightDifferencesProperty);
        set => SetValue(HighlightDifferencesProperty, value);
    }

    #endregion

    #region Fields

    private readonly DispatcherTimer _animationTimer;
    private readonly DispatcherTimer _analysisTimer;
    private readonly List<ParticleSystem> _particleSystems = new();
    private Canvas _particleCanvas;
    private Grid _mainGrid;
    private TabControl _tabControl;
    private Canvas _radarChartCanvas;
    private readonly Random _random = new();

    #endregion

    #region Constructor

    public HoloCharacterComparison()
    {
        InitializeComponent();
        InitializeTimers();
        InitializeDefaults();
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        _mainGrid = new Grid
        {
            Background = new SolidColorBrush(Color.FromArgb(20, 0, 150, 255))
        };

        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var header = CreateHeaderSection();
        Grid.SetRow(header, 0);
        _mainGrid.Children.Add(header);

        _tabControl = CreateTabControl();
        Grid.SetRow(_tabControl, 1);
        _mainGrid.Children.Add(_tabControl);

        _particleCanvas = new Canvas
        {
            IsHitTestVisible = false,
            Background = Brushes.Transparent
        };
        Grid.SetRowSpan(_particleCanvas, 2);
        _mainGrid.Children.Add(_particleCanvas);

        Content = _mainGrid;
    }

    private Border CreateHeaderSection()
    {
        var header = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromArgb(100, 0, 150, 255),
                Color.FromArgb(50, 0, 100, 200),
                90),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 0, 200, 255)),
            BorderThickness = new Thickness(0, 0, 0, 2),
            Padding = new Thickness(20, 15),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(100, 0, 150, 255),
                ShadowDepth = 0,
                BlurRadius = 20
            }
        };

        var stackPanel = new StackPanel();

        var titleBlock = new TextBlock
        {
            Text = "HOLOGRAPHIC CHARACTER COMPARISON",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Left,
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(150, 0, 255, 255),
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };

        var controlsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 10, 0, 0)
        };

        var modeSelector = CreateModeSelector();
        var categorySelector = CreateCategorySelector();
        var analyzeButton = CreateAnalyzeButton();
        var addCharacterButton = CreateAddCharacterButton();

        controlsPanel.Children.Add(modeSelector);
        controlsPanel.Children.Add(categorySelector);
        controlsPanel.Children.Add(analyzeButton);
        controlsPanel.Children.Add(addCharacterButton);

        stackPanel.Children.Add(titleBlock);
        stackPanel.Children.Add(controlsPanel);

        header.Child = stackPanel;
        return header;
    }

    private ComboBox CreateModeSelector()
    {
        var comboBox = new ComboBox
        {
            Width = 120,
            Margin = new Thickness(0, 0, 15, 0),
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 50, 100)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 0, 200, 255))
        };

        comboBox.Items.Add("Side by Side");
        comboBox.Items.Add("Overlay");
        comboBox.Items.Add("Radar Chart");
        comboBox.Items.Add("Matrix View");
        comboBox.SelectedIndex = 0;

        return comboBox;
    }

    private ComboBox CreateCategorySelector()
    {
        var comboBox = new ComboBox
        {
            Width = 100,
            Margin = new Thickness(0, 0, 15, 0),
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 50, 100)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 0, 200, 255))
        };

        comboBox.Items.Add("All");
        comboBox.Items.Add("Skills");
        comboBox.Items.Add("Assets");
        comboBox.Items.Add("Combat");
        comboBox.Items.Add("Industry");
        comboBox.SelectedIndex = 0;

        return comboBox;
    }

    private Button CreateAnalyzeButton()
    {
        var button = new Button
        {
            Content = "ANALYZE",
            Width = 100,
            Height = 30,
            Margin = new Thickness(0, 0, 15, 0),
            Background = new LinearGradientBrush(
                Color.FromArgb(150, 0, 200, 100),
                Color.FromArgb(100, 0, 150, 50),
                90),
            Foreground = new SolidColorBrush(Colors.White),
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 0, 255, 100)),
            BorderThickness = new Thickness(1),
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(100, 0, 200, 100),
                ShadowDepth = 0,
                BlurRadius = 10
            }
        };

        button.Click += OnAnalyzeClicked;

        return button;
    }

    private Button CreateAddCharacterButton()
    {
        var button = new Button
        {
            Content = "ADD CHARACTER",
            Width = 120,
            Height = 30,
            Background = new LinearGradientBrush(
                Color.FromArgb(150, 0, 150, 255),
                Color.FromArgb(100, 0, 100, 200),
                90),
            Foreground = new SolidColorBrush(Colors.White),
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 0, 200, 255)),
            BorderThickness = new Thickness(1),
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(100, 0, 150, 255),
                ShadowDepth = 0,
                BlurRadius = 10
            }
        };

        button.Click += OnAddCharacterClicked;

        return button;
    }

    private TabControl CreateTabControl()
    {
        var tabControl = new TabControl
        {
            Background = Brushes.Transparent,
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 200, 255)),
            BorderThickness = new Thickness(1)
        };

        tabControl.Items.Add(CreateOverviewTab());
        tabControl.Items.Add(CreateSkillGapsTab());
        tabControl.Items.Add(CreateRadarChartsTab());
        tabControl.Items.Add(CreateRecommendationsTab());

        return tabControl;
    }

    private TabItem CreateOverviewTab()
    {
        var tab = new TabItem
        {
            Header = "OVERVIEW",
            Background = new SolidColorBrush(Color.FromArgb(80, 0, 100, 200)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };

        var scrollViewer = new ScrollViewer
        {
            Background = Brushes.Transparent,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        var grid = new Grid
        {
            Margin = new Thickness(20)
        };

        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var characterSelector = CreateCharacterSelectorPanel();
        Grid.SetRow(characterSelector, 0);
        grid.Children.Add(characterSelector);

        var comparisonGrid = CreateComparisonGrid();
        Grid.SetRow(comparisonGrid, 1);
        grid.Children.Add(comparisonGrid);

        scrollViewer.Content = grid;
        tab.Content = scrollViewer;
        return tab;
    }

    private TabItem CreateSkillGapsTab()
    {
        var tab = new TabItem
        {
            Header = "SKILL GAPS",
            Background = new SolidColorBrush(Color.FromArgb(80, 0, 100, 200)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };

        var scrollViewer = new ScrollViewer
        {
            Background = Brushes.Transparent,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        var stackPanel = new StackPanel
        {
            Margin = new Thickness(20)
        };

        var skillGaps = CreateSkillGapAnalysis();
        foreach (var gap in skillGaps)
        {
            stackPanel.Children.Add(gap);
        }

        scrollViewer.Content = stackPanel;
        tab.Content = scrollViewer;
        return tab;
    }

    private TabItem CreateRadarChartsTab()
    {
        var tab = new TabItem
        {
            Header = "RADAR CHARTS",
            Background = new SolidColorBrush(Color.FromArgb(80, 0, 100, 200)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };

        var grid = new Grid
        {
            Margin = new Thickness(20)
        };

        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var radarControls = CreateRadarControls();
        Grid.SetRow(radarControls, 0);
        grid.Children.Add(radarControls);

        _radarChartCanvas = CreateRadarChartCanvas();
        Grid.SetRow(_radarChartCanvas, 1);
        grid.Children.Add(_radarChartCanvas);

        tab.Content = grid;
        return tab;
    }

    private TabItem CreateRecommendationsTab()
    {
        var tab = new TabItem
        {
            Header = "RECOMMENDATIONS",
            Background = new SolidColorBrush(Color.FromArgb(80, 0, 100, 200)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };

        var scrollViewer = new ScrollViewer
        {
            Background = Brushes.Transparent,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        var stackPanel = new StackPanel
        {
            Margin = new Thickness(20)
        };

        var recommendations = CreateRecommendations();
        foreach (var recommendation in recommendations)
        {
            stackPanel.Children.Add(recommendation);
        }

        scrollViewer.Content = stackPanel;
        tab.Content = scrollViewer;
        return tab;
    }

    private Border CreateCharacterSelectorPanel()
    {
        var border = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromArgb(60, 0, 100, 200),
                Color.FromArgb(30, 0, 50, 100),
                90),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 0, 200, 255)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 0, 20),
            Padding = new Thickness(15),
            CornerRadius = new CornerRadius(8)
        };

        var stackPanel = new StackPanel();

        var title = new TextBlock
        {
            Text = "CHARACTERS IN COMPARISON",
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            Margin = new Thickness(0, 0, 0, 15)
        };

        var charactersPanel = new WrapPanel
        {
            Orientation = Orientation.Horizontal
        };

        var characters = new[]
        {
            new { Name = "Main Character", Type = "Combat", SP = "127M", Active = true },
            new { Name = "Alt Character", Type = "Industry", SP = "45M", Active = true },
            new { Name = "Trading Alt", Type = "Trading", SP = "23M", Active = false }
        };

        foreach (var character in characters)
        {
            var characterCard = CreateCharacterCard(character.Name, character.Type, character.SP, character.Active);
            charactersPanel.Children.Add(characterCard);
        }

        stackPanel.Children.Add(title);
        stackPanel.Children.Add(charactersPanel);

        border.Child = stackPanel;
        return border;
    }

    private Border CreateCharacterCard(string name, string type, string sp, bool active)
    {
        var activeColor = active ? Colors.Green : Colors.Gray;

        var border = new Border
        {
            Width = 180,
            Height = 100,
            Background = new LinearGradientBrush(
                Color.FromArgb(80, activeColor.R, activeColor.G, activeColor.B),
                Color.FromArgb(40, activeColor.R, activeColor.G, activeColor.B),
                90),
            BorderBrush = new SolidColorBrush(activeColor),
            BorderThickness = new Thickness(2),
            Margin = new Thickness(10),
            Padding = new Thickness(15),
            CornerRadius = new CornerRadius(8),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(100, activeColor.R, activeColor.G, activeColor.B),
                ShadowDepth = 0,
                BlurRadius = 15
            }
        };

        var stackPanel = new StackPanel();

        var nameText = new TextBlock
        {
            Text = name,
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 5)
        };

        var typeText = new TextBlock
        {
            Text = $"[{type}]",
            FontSize = 10,
            Foreground = new SolidColorBrush(activeColor),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 5)
        };

        var spText = new TextBlock
        {
            Text = $"{sp} SP",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        stackPanel.Children.Add(nameText);
        stackPanel.Children.Add(typeText);
        stackPanel.Children.Add(spText);

        border.Child = stackPanel;

        border.MouseEnter += (s, e) =>
        {
            var scaleTransform = new ScaleTransform(1.05, 1.05);
            border.RenderTransform = scaleTransform;
            border.RenderTransformOrigin = new Point(0.5, 0.5);
        };

        border.MouseLeave += (s, e) =>
        {
            border.RenderTransform = null;
        };

        return border;
    }

    private Grid CreateComparisonGrid()
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
        grid.ColumnDefinitions.Add(new ColumnDefinition());

        var character1Panel = CreateCharacterComparisonPanel("MAIN CHARACTER", true);
        Grid.SetColumn(character1Panel, 0);
        grid.Children.Add(character1Panel);

        var vsPanel = CreateVsPanel();
        Grid.SetColumn(vsPanel, 1);
        grid.Children.Add(vsPanel);

        var character2Panel = CreateCharacterComparisonPanel("ALT CHARACTER", false);
        Grid.SetColumn(character2Panel, 2);
        grid.Children.Add(character2Panel);

        return grid;
    }

    private Border CreateCharacterComparisonPanel(string characterName, bool isMain)
    {
        var color = isMain ? Colors.Blue : Colors.Green;

        var border = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromArgb(60, color.R, color.G, color.B),
                Color.FromArgb(30, color.R, color.G, color.B),
                90),
            BorderBrush = new SolidColorBrush(color),
            BorderThickness = new Thickness(2),
            Margin = new Thickness(5),
            Padding = new Thickness(15),
            CornerRadius = new CornerRadius(8)
        };

        var stackPanel = new StackPanel();

        var title = new TextBlock
        {
            Text = characterName,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(color),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 15)
        };

        stackPanel.Children.Add(title);

        var stats = isMain ? new[]
        {
            new { Label = "Total SP", Value = "127,450,000", Winner = true },
            new { Label = "ISK Balance", Value = "12.5B", Winner = true },
            new { Label = "Total Assets", Value = "45.2B", Winner = true },
            new { Label = "PvP Kills", Value = "127", Winner = true },
            new { Label = "Security Status", Value = "+2.4", Winner = false },
            new { Label = "Combat Capability", Value = "95%", Winner = true },
            new { Label = "Industry Capability", Value = "45%", Winner = false },
            new { Label = "Trading Capability", Value = "60%", Winner = false }
        } : new[]
        {
            new { Label = "Total SP", Value = "45,200,000", Winner = false },
            new { Label = "ISK Balance", Value = "2.1B", Winner = false },
            new { Label = "Total Assets", Value = "8.7B", Winner = false },
            new { Label = "PvP Kills", Value = "23", Winner = false },
            new { Label = "Security Status", Value = "+5.0", Winner = true },
            new { Label = "Combat Capability", Value = "30%", Winner = false },
            new { Label = "Industry Capability", Value = "85%", Winner = true },
            new { Label = "Trading Capability", Value = "90%", Winner = true }
        };

        foreach (var stat in stats)
        {
            var statItem = CreateComparisonStatItem(stat.Label, stat.Value, stat.Winner);
            stackPanel.Children.Add(statItem);
        }

        border.Child = stackPanel;
        return border;
    }

    private Border CreateComparisonStatItem(string label, string value, bool isWinner)
    {
        var winnerColor = isWinner ? Colors.Gold : Colors.Transparent;

        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(isWinner ? winnerColor : Color.FromArgb(100, 0, 150, 255)),
            BorderThickness = new Thickness(isWinner ? 2 : 1),
            Margin = new Thickness(0, 0, 0, 8),
            Padding = new Thickness(10, 8),
            CornerRadius = new CornerRadius(5),
            Effect = isWinner ? new DropShadowEffect
            {
                Color = Color.FromArgb(150, winnerColor.R, winnerColor.G, winnerColor.B),
                ShadowDepth = 0,
                BlurRadius = 10
            } : null
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());

        var labelText = new TextBlock
        {
            Text = label,
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255))
        };
        Grid.SetColumn(labelText, 0);

        var valueText = new TextBlock
        {
            Text = value,
            FontSize = 11,
            FontWeight = isWinner ? FontWeights.Bold : FontWeights.Normal,
            Foreground = new SolidColorBrush(isWinner ? winnerColor : Colors.White),
            HorizontalAlignment = HorizontalAlignment.Right
        };
        Grid.SetColumn(valueText, 1);

        grid.Children.Add(labelText);
        grid.Children.Add(valueText);

        border.Child = grid;
        return border;
    }

    private Border CreateVsPanel()
    {
        var border = new Border
        {
            Background = new RadialGradientBrush(
                Color.FromArgb(100, 255, 255, 255),
                Color.FromArgb(50, 200, 200, 200)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(25),
            Width = 50,
            Height = 50,
            VerticalAlignment = VerticalAlignment.Center,
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(150, 255, 255, 255),
                ShadowDepth = 0,
                BlurRadius = 15
            }
        };

        var textBlock = new TextBlock
        {
            Text = "VS",
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.Black),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        border.Child = textBlock;
        return border;
    }

    private List<Border> CreateSkillGapAnalysis()
    {
        var gaps = new List<Border>();
        var skillGaps = new[]
        {
            new { Category = "Spaceship Command", Gap = "Capital Ships V", Priority = "High", TrainingTime = "45 days" },
            new { Category = "Gunnery", Gap = "Dreadnought Weapons", Priority = "High", TrainingTime = "30 days" },
            new { Category = "Industry", Gap = "Advanced Industry V", Priority = "Medium", TrainingTime = "25 days" },
            new { Category = "Trading", Gap = "Broker Relations V", Priority = "Low", TrainingTime = "15 days" },
            new { Category = "Navigation", Gap = "Jump Drive Calibration V", Priority = "Medium", TrainingTime = "20 days" }
        };

        foreach (var gap in skillGaps)
        {
            var gapItem = CreateSkillGapItem(gap.Category, gap.Gap, gap.Priority, gap.TrainingTime);
            gaps.Add(gapItem);
        }

        return gaps;
    }

    private Border CreateSkillGapItem(string category, string gap, string priority, string trainingTime)
    {
        var priorityColor = priority switch
        {
            "High" => Colors.Red,
            "Medium" => Colors.Orange,
            "Low" => Colors.Green,
            _ => Colors.Gray
        };

        var border = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromArgb(60, 0, 100, 200),
                Color.FromArgb(30, 0, 50, 100),
                90),
            BorderBrush = new SolidColorBrush(priorityColor),
            BorderThickness = new Thickness(2, 2, 6, 2),
            Margin = new Thickness(0, 0, 0, 15),
            Padding = new Thickness(20, 15),
            CornerRadius = new CornerRadius(8),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(80, priorityColor.R, priorityColor.G, priorityColor.B),
                ShadowDepth = 0,
                BlurRadius = 15
            }
        };

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var categoryText = new TextBlock
        {
            Text = category,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };
        Grid.SetRow(categoryText, 0);
        Grid.SetColumn(categoryText, 0);

        var priorityText = new TextBlock
        {
            Text = priority.ToUpper(),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(priorityColor),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(priorityText, 0);
        Grid.SetColumn(priorityText, 1);

        var timeText = new TextBlock
        {
            Text = trainingTime,
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 100)),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(timeText, 0);
        Grid.SetColumn(timeText, 2);

        var gapText = new TextBlock
        {
            Text = $"Missing: {gap}",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            Margin = new Thickness(0, 8, 0, 0)
        };
        Grid.SetRow(gapText, 1);
        Grid.SetColumnSpan(gapText, 3);

        grid.Children.Add(categoryText);
        grid.Children.Add(priorityText);
        grid.Children.Add(timeText);
        grid.Children.Add(gapText);

        border.Child = grid;
        return border;
    }

    private StackPanel CreateRadarControls()
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 20)
        };

        var metricLabel = new TextBlock
        {
            Text = "METRICS:",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };

        var metricCombo = new ComboBox
        {
            Width = 150,
            Margin = new Thickness(0, 0, 20, 0),
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 50, 100)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };
        metricCombo.Items.Add("Skill Categories");
        metricCombo.Items.Add("Combat Stats");
        metricCombo.Items.Add("Industry Stats");
        metricCombo.Items.Add("Overall Performance");
        metricCombo.SelectedIndex = 0;

        var overlayCheckBox = new CheckBox
        {
            Content = "Overlay Characters",
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            IsChecked = true,
            VerticalAlignment = VerticalAlignment.Center
        };

        panel.Children.Add(metricLabel);
        panel.Children.Add(metricCombo);
        panel.Children.Add(overlayCheckBox);

        return panel;
    }

    private Canvas CreateRadarChartCanvas()
    {
        var canvas = new Canvas
        {
            Background = new RadialGradientBrush(
                Color.FromArgb(40, 0, 100, 200),
                Color.FromArgb(20, 0, 50, 100))
        };

        CreateRadarChart(canvas);

        return canvas;
    }

    private void CreateRadarChart(Canvas canvas)
    {
        var center = new Point(250, 200);
        var radius = 150;
        var categories = new[] { "Combat", "Industry", "Trading", "Exploration", "Social", "Leadership" };
        var character1Values = new[] { 95, 45, 60, 70, 30, 85 };
        var character2Values = new[] { 30, 85, 90, 40, 60, 20 };

        // Draw radar grid
        CreateRadarGrid(canvas, center, radius, categories.Length);

        // Draw category labels
        for (int i = 0; i < categories.Length; i++)
        {
            var angle = (2 * Math.PI * i / categories.Length) - Math.PI / 2;
            var labelX = center.X + (radius + 20) * Math.Cos(angle);
            var labelY = center.Y + (radius + 20) * Math.Sin(angle);

            var label = new TextBlock
            {
                Text = categories[i],
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Canvas.SetLeft(label, labelX - 30);
            Canvas.SetTop(label, labelY - 10);
            canvas.Children.Add(label);
        }

        // Draw character 1 radar
        CreateCharacterRadar(canvas, center, radius, character1Values, Colors.Blue);

        // Draw character 2 radar
        CreateCharacterRadar(canvas, center, radius, character2Values, Colors.Green);
    }

    private void CreateRadarGrid(Canvas canvas, Point center, double radius, int segments)
    {
        // Draw concentric circles
        for (int i = 1; i <= 5; i++)
        {
            var circleRadius = radius * i / 5;
            var circle = new Ellipse
            {
                Width = circleRadius * 2,
                Height = circleRadius * 2,
                Stroke = new SolidColorBrush(Color.FromArgb(50, 0, 200, 255)),
                StrokeThickness = 1
            };

            Canvas.SetLeft(circle, center.X - circleRadius);
            Canvas.SetTop(circle, center.Y - circleRadius);
            canvas.Children.Add(circle);
        }

        // Draw radial lines
        for (int i = 0; i < segments; i++)
        {
            var angle = (2 * Math.PI * i / segments) - Math.PI / 2;
            var endX = center.X + radius * Math.Cos(angle);
            var endY = center.Y + radius * Math.Sin(angle);

            var line = new Line
            {
                X1 = center.X,
                Y1 = center.Y,
                X2 = endX,
                Y2 = endY,
                Stroke = new SolidColorBrush(Color.FromArgb(50, 0, 200, 255)),
                StrokeThickness = 1
            };

            canvas.Children.Add(line);
        }
    }

    private void CreateCharacterRadar(Canvas canvas, Point center, double radius, int[] values, Color color)
    {
        var points = new List<Point>();

        for (int i = 0; i < values.Length; i++)
        {
            var angle = (2 * Math.PI * i / values.Length) - Math.PI / 2;
            var distance = radius * values[i] / 100.0;
            var x = center.X + distance * Math.Cos(angle);
            var y = center.Y + distance * Math.Sin(angle);
            points.Add(new Point(x, y));
        }

        // Create polygon
        var polygon = new Polygon
        {
            Fill = new SolidColorBrush(Color.FromArgb(50, color.R, color.G, color.B)),
            Stroke = new SolidColorBrush(color),
            StrokeThickness = 2,
            Effect = new DropShadowEffect
            {
                Color = color,
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };

        foreach (var point in points)
        {
            polygon.Points.Add(point);
        }

        canvas.Children.Add(polygon);

        // Add data points
        foreach (var point in points)
        {
            var ellipse = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = new SolidColorBrush(color),
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 1
            };

            Canvas.SetLeft(ellipse, point.X - 3);
            Canvas.SetTop(ellipse, point.Y - 3);
            canvas.Children.Add(ellipse);
        }
    }

    private List<Border> CreateRecommendations()
    {
        var recommendations = new List<Border>();
        var recommendationData = new[]
        {
            new { 
                Title = "Focus on Capital Ship Training", 
                Description = "Main Character should prioritize Capital Ships V to maximize combat effectiveness.",
                Priority = "High",
                EstimatedTime = "45 days"
            },
            new { 
                Title = "Develop Alt Character's Combat Skills", 
                Description = "Alt Character lacks combat capability. Consider cross-training basic combat skills.",
                Priority = "Medium",
                EstimatedTime = "90 days"
            },
            new { 
                Title = "Optimize Trading Operations", 
                Description = "Alt Character's trading skills could benefit Main Character's ISK generation.",
                Priority = "Medium",
                EstimatedTime = "30 days"
            },
            new { 
                Title = "Improve Security Status", 
                Description = "Main Character's security status limits access to high-sec opportunities.",
                Priority = "Low",
                EstimatedTime = "Ongoing"
            }
        };

        foreach (var rec in recommendationData)
        {
            var recommendation = CreateRecommendationItem(rec.Title, rec.Description, rec.Priority, rec.EstimatedTime);
            recommendations.Add(recommendation);
        }

        return recommendations;
    }

    private Border CreateRecommendationItem(string title, string description, string priority, string estimatedTime)
    {
        var priorityColor = priority switch
        {
            "High" => Colors.Red,
            "Medium" => Colors.Orange,
            "Low" => Colors.Green,
            _ => Colors.Gray
        };

        var border = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromArgb(60, 0, 150, 100),
                Color.FromArgb(30, 0, 100, 50),
                90),
            BorderBrush = new SolidColorBrush(priorityColor),
            BorderThickness = new Thickness(1, 1, 4, 1),
            Margin = new Thickness(0, 0, 0, 15),
            Padding = new Thickness(20, 15),
            CornerRadius = new CornerRadius(8),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(80, priorityColor.R, priorityColor.G, priorityColor.B),
                ShadowDepth = 0,
                BlurRadius = 15
            }
        };

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var headerGrid = new Grid();
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var titleText = new TextBlock
        {
            Text = title,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 100))
        };
        Grid.SetColumn(titleText, 0);

        var priorityText = new TextBlock
        {
            Text = priority.ToUpper(),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(priorityColor),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(priorityText, 1);

        headerGrid.Children.Add(titleText);
        headerGrid.Children.Add(priorityText);
        Grid.SetRow(headerGrid, 0);

        var descriptionText = new TextBlock
        {
            Text = description,
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255)),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 8, 0, 0)
        };
        Grid.SetRow(descriptionText, 1);

        var timeText = new TextBlock
        {
            Text = $"Estimated Time: {estimatedTime}",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 255, 255, 100)),
            Margin = new Thickness(0, 8, 0, 0)
        };
        Grid.SetRow(timeText, 2);

        grid.Children.Add(headerGrid);
        grid.Children.Add(descriptionText);
        grid.Children.Add(timeText);

        border.Child = grid;
        return border;
    }

    private void InitializeTimers()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };
        _animationTimer.Tick += OnAnimationTick;

        _analysisTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(5)
        };
        _analysisTimer.Tick += OnAnalysisTick;
    }

    private void InitializeDefaults()
    {
        ComparisonCharacters = new ObservableCollection<HoloComparisonCharacter>();
        ComparisonResults = new HoloComparisonResults();
    }

    #endregion

    #region Animation Methods

    private void StartComparisonAnimations()
    {
        if (!EnableAnimations) return;

        // Add comparison animations
    }

    #endregion

    #region Particle System

    private void CreateParticleSystem()
    {
        if (!EnableParticleEffects) return;

        var comparisonParticles = new ParticleSystem(_particleCanvas)
        {
            ParticleColor = Color.FromArgb(150, 255, 215, 0),
            ParticleSize = 3,
            ParticleSpeed = 1.0,
            EmissionRate = 2,
            ParticleLifespan = TimeSpan.FromSeconds(4)
        };

        var analysisParticles = new ParticleSystem(_particleCanvas)
        {
            ParticleColor = Color.FromArgb(150, 0, 255, 150),
            ParticleSize = 2,
            ParticleSpeed = 1.5,
            EmissionRate = 3,
            ParticleLifespan = TimeSpan.FromSeconds(3)
        };

        _particleSystems.Add(comparisonParticles);
        _particleSystems.Add(analysisParticles);
    }

    private void UpdateParticles()
    {
        if (!EnableParticleEffects) return;

        foreach (var system in _particleSystems)
        {
            system.Update();

            if (_random.NextDouble() < 0.2)
            {
                var x = _random.NextDouble() * ActualWidth;
                var y = _random.NextDouble() * ActualHeight;
                system.EmitParticle(new Point(x, y));
            }
        }
    }

    #endregion

    #region Helper Methods

    private Color GetEVEColor(EVEColorScheme scheme)
    {
        return scheme switch
        {
            EVEColorScheme.ElectricBlue => Color.FromRgb(0, 200, 255),
            EVEColorScheme.CorporationGold => Color.FromRgb(255, 215, 0),
            EVEColorScheme.AllianceGreen => Color.FromRgb(0, 255, 100),
            EVEColorScheme.ConcordRed => Color.FromRgb(255, 50, 50),
            _ => Color.FromRgb(0, 200, 255)
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

        if (AutoAnalyze)
        {
            _analysisTimer.Start();
        }

        CreateParticleSystem();
        StartComparisonAnimations();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        _analysisTimer?.Stop();

        foreach (var system in _particleSystems)
        {
            system.Dispose();
        }
        _particleSystems.Clear();
    }

    private void OnAnimationTick(object sender, EventArgs e)
    {
        UpdateParticles();
    }

    private void OnAnalysisTick(object sender, EventArgs e)
    {
        PerformAutoAnalysis();
    }

    private void OnAnalyzeClicked(object sender, RoutedEventArgs e)
    {
        PerformComparison();
    }

    private void OnAddCharacterClicked(object sender, RoutedEventArgs e)
    {
        // Show add character dialog
    }

    private void PerformComparison()
    {
        // Perform character comparison analysis
    }

    private void PerformAutoAnalysis()
    {
        // Perform automatic analysis
    }

    #endregion

    #region Property Changed Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterComparison control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterComparison control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnComparisonCharactersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterComparison control)
        {
            control.RefreshComparison();
        }
    }

    private static void OnComparisonResultsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterComparison control)
        {
            control.UpdateResultsDisplay();
        }
    }

    private static void OnComparisonModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterComparison control)
        {
            control.UpdateComparisonMode();
        }
    }

    private static void OnComparisonCategoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterComparison control)
        {
            control.FilterByCategory();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterComparison control)
        {
            if ((bool)e.NewValue)
            {
                control._animationTimer?.Start();
                control.StartComparisonAnimations();
            }
            else
            {
                control._animationTimer?.Stop();
            }
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterComparison control)
        {
            if (!(bool)e.NewValue)
            {
                foreach (var system in control._particleSystems)
                {
                    system.ClearParticles();
                }
            }
        }
    }

    private static void OnShowSkillGapsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterComparison control)
        {
            control.UpdateSkillGapDisplay();
        }
    }

    private static void OnShowRadarChartsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterComparison control)
        {
            control.UpdateRadarChartDisplay();
        }
    }

    private static void OnShowDetailedMetricsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterComparison control)
        {
            control.UpdateDetailedMetricsDisplay();
        }
    }

    private static void OnShowRecommendationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterComparison control)
        {
            control.UpdateRecommendationsDisplay();
        }
    }

    private static void OnAutoAnalyzeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterComparison control)
        {
            if ((bool)e.NewValue)
            {
                control._analysisTimer?.Start();
            }
            else
            {
                control._analysisTimer?.Stop();
            }
        }
    }

    private static void OnHighlightDifferencesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterComparison control)
        {
            control.UpdateHighlighting();
        }
    }

    private void UpdateHolographicEffects()
    {
        // Update holographic intensity across all visual elements
    }

    private void UpdateColorScheme()
    {
        var color = GetEVEColor(EVEColorScheme);
        // Apply color scheme to all relevant elements
    }

    private void RefreshComparison()
    {
        // Refresh the comparison display
    }

    private void UpdateResultsDisplay()
    {
        // Update comparison results display
    }

    private void UpdateComparisonMode()
    {
        // Update the comparison mode
    }

    private void FilterByCategory()
    {
        // Filter comparison by selected category
    }

    private void UpdateSkillGapDisplay()
    {
        // Update skill gap display
    }

    private void UpdateRadarChartDisplay()
    {
        // Update radar chart display
    }

    private void UpdateDetailedMetricsDisplay()
    {
        // Update detailed metrics display
    }

    private void UpdateRecommendationsDisplay()
    {
        // Update recommendations display
    }

    private void UpdateHighlighting()
    {
        // Update difference highlighting
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void AdaptToHardware(HardwareCapability capability)
    {
        switch (capability)
        {
            case HardwareCapability.Low:
                EnableAnimations = false;
                EnableParticleEffects = false;
                break;
            case HardwareCapability.Medium:
                EnableAnimations = true;
                EnableParticleEffects = false;
                break;
            case HardwareCapability.High:
                EnableAnimations = true;
                EnableParticleEffects = true;
                break;
        }
    }

    #endregion
}

#region Supporting Types

public class HoloComparisonCharacter
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string CharacterType { get; set; } = string.Empty;
    public long TotalSkillPoints { get; set; }
    public decimal ISKBalance { get; set; }
    public decimal TotalAssets { get; set; }
    public int PvPKills { get; set; }
    public double SecurityStatus { get; set; }
    public Dictionary<string, double> SkillCategories { get; set; } = new();
    public Dictionary<string, double> Capabilities { get; set; } = new();
    public bool IsActiveInComparison { get; set; }
}

public class HoloComparisonResults
{
    public List<HoloSkillGap> SkillGaps { get; set; } = new();
    public List<HoloComparisonMetric> Metrics { get; set; } = new();
    public List<HoloRecommendation> Recommendations { get; set; } = new();
    public DateTime LastAnalysis { get; set; }
    public string WinnerCharacterId { get; set; } = string.Empty;
}

public class HoloSkillGap
{
    public string Category { get; set; } = string.Empty;
    public string MissingSkill { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public TimeSpan TrainingTime { get; set; }
    public string CharacterId { get; set; } = string.Empty;
}

public class HoloComparisonMetric
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, double> CharacterValues { get; set; } = new();
    public string WinnerCharacterId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class HoloRecommendation
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public TimeSpan EstimatedTime { get; set; }
    public string TargetCharacterId { get; set; } = string.Empty;
}

public enum ComparisonMode
{
    SideBySide,
    Overlay,
    RadarChart,
    MatrixView
}

public enum ComparisonCategory
{
    All,
    Skills,
    Assets,
    Combat,
    Industry,
    Trading,
    Exploration
}

public enum EVEColorScheme
{
    ElectricBlue,
    CorporationGold,
    AllianceGreen,
    ConcordRed
}

public enum HardwareCapability
{
    Low,
    Medium,
    High
}

public interface IAnimationIntensityTarget
{
    double HolographicIntensity { get; set; }
}

public interface IAdaptiveControl
{
    void AdaptToHardware(HardwareCapability capability);
}

#endregion