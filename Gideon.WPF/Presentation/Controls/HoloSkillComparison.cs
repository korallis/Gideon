// ==========================================================================
// HoloSkillComparison.cs - Holographic Skill Comparison Interface
// ==========================================================================
// Advanced skill comparison system featuring multi-character analysis,
// skill gap identification, EVE-style comparison metrics, and holographic visualization.
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
/// Holographic skill comparison interface with multi-character analysis and gap identification
/// </summary>
public class HoloSkillComparison : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloSkillComparison),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloSkillComparison),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty ComparisonCharactersProperty =
        DependencyProperty.Register(nameof(ComparisonCharacters), typeof(ObservableCollection<HoloCharacterSkills>), typeof(HoloSkillComparison),
            new PropertyMetadata(null, OnComparisonCharactersChanged));

    public static readonly DependencyProperty SelectedSkillCategoryProperty =
        DependencyProperty.Register(nameof(SelectedSkillCategory), typeof(SkillCategory), typeof(HoloSkillComparison),
            new PropertyMetadata(SkillCategory.All, OnSelectedSkillCategoryChanged));

    public static readonly DependencyProperty ComparisonModeProperty =
        DependencyProperty.Register(nameof(ComparisonMode), typeof(SkillComparisonMode), typeof(HoloSkillComparison),
            new PropertyMetadata(SkillComparisonMode.SideBySide, OnComparisonModeChanged));

    public static readonly DependencyProperty ShowSkillGapsProperty =
        DependencyProperty.Register(nameof(ShowSkillGaps), typeof(bool), typeof(HoloSkillComparison),
            new PropertyMetadata(true, OnShowSkillGapsChanged));

    public static readonly DependencyProperty ShowAdvantagesProperty =
        DependencyProperty.Register(nameof(ShowAdvantages), typeof(bool), typeof(HoloSkillComparison),
            new PropertyMetadata(true, OnShowAdvantagesChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloSkillComparison),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloSkillComparison),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowStatisticalAnalysisProperty =
        DependencyProperty.Register(nameof(ShowStatisticalAnalysis), typeof(bool), typeof(HoloSkillComparison),
            new PropertyMetadata(true, OnShowStatisticalAnalysisChanged));

    public static readonly DependencyProperty ShowTrainingPlansProperty =
        DependencyProperty.Register(nameof(ShowTrainingPlans), typeof(bool), typeof(HoloSkillComparison),
            new PropertyMetadata(true, OnShowTrainingPlansChanged));

    public static readonly DependencyProperty ComparisonDepthProperty =
        DependencyProperty.Register(nameof(ComparisonDepth), typeof(ComparisonDepth), typeof(HoloSkillComparison),
            new PropertyMetadata(ComparisonDepth.Detailed, OnComparisonDepthChanged));

    public static readonly DependencyProperty GroupBySkillGroupProperty =
        DependencyProperty.Register(nameof(GroupBySkillGroup), typeof(bool), typeof(HoloSkillComparison),
            new PropertyMetadata(true, OnGroupBySkillGroupChanged));

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

    public ObservableCollection<HoloCharacterSkills> ComparisonCharacters
    {
        get => (ObservableCollection<HoloCharacterSkills>)GetValue(ComparisonCharactersProperty);
        set => SetValue(ComparisonCharactersProperty, value);
    }

    public SkillCategory SelectedSkillCategory
    {
        get => (SkillCategory)GetValue(SelectedSkillCategoryProperty);
        set => SetValue(SelectedSkillCategoryProperty, value);
    }

    public SkillComparisonMode ComparisonMode
    {
        get => (SkillComparisonMode)GetValue(ComparisonModeProperty);
        set => SetValue(ComparisonModeProperty, value);
    }

    public bool ShowSkillGaps
    {
        get => (bool)GetValue(ShowSkillGapsProperty);
        set => SetValue(ShowSkillGapsProperty, value);
    }

    public bool ShowAdvantages
    {
        get => (bool)GetValue(ShowAdvantagesProperty);
        set => SetValue(ShowAdvantagesProperty, value);
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

    public bool ShowStatisticalAnalysis
    {
        get => (bool)GetValue(ShowStatisticalAnalysisProperty);
        set => SetValue(ShowStatisticalAnalysisProperty, value);
    }

    public bool ShowTrainingPlans
    {
        get => (bool)GetValue(ShowTrainingPlansProperty);
        set => SetValue(ShowTrainingPlansProperty, value);
    }

    public ComparisonDepth ComparisonDepth
    {
        get => (ComparisonDepth)GetValue(ComparisonDepthProperty);
        set => SetValue(ComparisonDepthProperty, value);
    }

    public bool GroupBySkillGroup
    {
        get => (bool)GetValue(GroupBySkillGroupProperty);
        set => SetValue(GroupBySkillGroupProperty, value);
    }

    #endregion

    #region Fields

    private Canvas _mainCanvas;
    private Canvas _particleCanvas;
    private Grid _comparisonGrid;
    private ScrollViewer _comparisonScrollViewer;
    private Border _statisticsPanel;
    private Border _trainingPlanPanel;
    private TabControl _comparisonTabs;
    
    private readonly List<HoloComparisonParticle> _particles = new();
    private readonly DispatcherTimer _animationTimer;
    private readonly Random _random = new();
    
    private bool _isAnimating = true;
    private bool _isDisposed = false;
    private double _currentAnimationTime = 0;

    #endregion

    #region Constructor

    public HoloSkillComparison()
    {
        InitializeComponent();
        
        _animationTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16) // 60 FPS
        };
        _animationTimer.Tick += OnAnimationTick;
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 800;
        Height = 600;
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
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(150, GridUnitType.Pixel) });
        
        // Header with controls
        var headerPanel = CreateHeaderPanel();
        Grid.SetRow(headerPanel, 0);
        contentGrid.Children.Add(headerPanel);
        
        // Main comparison area with tabs
        _comparisonTabs = CreateComparisonTabs();
        Grid.SetRow(_comparisonTabs, 1);
        contentGrid.Children.Add(_comparisonTabs);
        
        // Statistics and training plan panel
        var bottomGrid = new Grid();
        bottomGrid.ColumnDefinitions.Add(new ColumnDefinition());
        bottomGrid.ColumnDefinitions.Add(new ColumnDefinition());
        
        _statisticsPanel = CreateStatisticsPanel();
        Grid.SetColumn(_statisticsPanel, 0);
        bottomGrid.Children.Add(_statisticsPanel);
        
        _trainingPlanPanel = CreateTrainingPlanPanel();
        Grid.SetColumn(_trainingPlanPanel, 1);
        bottomGrid.Children.Add(_trainingPlanPanel);
        
        Grid.SetRow(bottomGrid, 2);
        contentGrid.Children.Add(bottomGrid);
        
        _mainCanvas.Children.Add(contentGrid);
        _mainCanvas.Children.Add(_particleCanvas);
        border.Child = _mainCanvas;
        Content = border;

        CreateSampleData();
    }

    private Border CreateHeaderPanel()
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
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());

        // Title
        var titleText = new TextBlock
        {
            Text = "SKILL COMPARISON MATRIX",
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10)
        };
        Grid.SetColumn(titleText, 1);
        grid.Children.Add(titleText);

        // Mode selection
        var modePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10)
        };

        var modeLabel = new TextBlock
        {
            Text = "Mode:",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 5, 0)
        };
        modePanel.Children.Add(modeLabel);

        var modeCombo = new ComboBox
        {
            Width = 120,
            Height = 25,
            FontSize = 10,
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 100, 150)),
            Foreground = new SolidColorBrush(Colors.White),
            BorderBrush = new SolidColorBrush(Color.FromArgb(120, 0, 200, 255))
        };
        modeCombo.Items.Add("Side by Side");
        modeCombo.Items.Add("Overlay");
        modeCombo.Items.Add("Radar Chart");
        modeCombo.SelectedIndex = 0;
        modePanel.Children.Add(modeCombo);

        Grid.SetColumn(modePanel, 2);
        grid.Children.Add(modePanel);

        border.Child = grid;
        return border;
    }

    private TabControl CreateComparisonTabs()
    {
        var tabControl = new TabControl
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(80, 0, 200, 255)),
            Margin = new Thickness(10)
        };

        // Skill Gaps Tab
        var gapsTab = new TabItem
        {
            Header = "Skill Gaps",
            Foreground = new SolidColorBrush(Colors.White),
            Background = new SolidColorBrush(Color.FromArgb(60, 0, 100, 150))
        };
        gapsTab.Content = CreateSkillGapsContent();
        tabControl.Items.Add(gapsTab);

        // Advantages Tab
        var advantagesTab = new TabItem
        {
            Header = "Advantages",
            Foreground = new SolidColorBrush(Colors.White),
            Background = new SolidColorBrush(Color.FromArgb(60, 0, 100, 150))
        };
        advantagesTab.Content = CreateAdvantagesContent();
        tabControl.Items.Add(advantagesTab);

        // Side-by-Side Tab
        var sideBySideTab = new TabItem
        {
            Header = "Side by Side",
            Foreground = new SolidColorBrush(Colors.White),
            Background = new SolidColorBrush(Color.FromArgb(60, 0, 100, 150))
        };
        sideBySideTab.Content = CreateSideBySideContent();
        tabControl.Items.Add(sideBySideTab);

        return tabControl;
    }

    private ScrollViewer CreateSkillGapsContent()
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

        // Sample skill gaps
        var gapData = new[]
        {
            new { Character = "Character A", Skill = "Large Projectile Turret", Level = 3, Recommended = 5, Gap = "2 levels", Time = "15d 8h" },
            new { Character = "Character A", Skill = "Gunnery", Level = 4, Recommended = 5, Gap = "1 level", Time = "8d 12h" },
            new { Character = "Character B", Skill = "Shield Management", Level = 2, Recommended = 4, Gap = "2 levels", Time = "5d 16h" },
            new { Character = "Character B", Skill = "Weapon Upgrades", Level = 3, Recommended = 5, Gap = "2 levels", Time = "22d 4h" }
        };

        foreach (var gap in gapData)
        {
            var gapItem = CreateSkillGapItem(gap.Character, gap.Skill, gap.Level, gap.Recommended, gap.Gap, gap.Time);
            panel.Children.Add(gapItem);
        }

        scrollViewer.Content = panel;
        return scrollViewer;
    }

    private Border CreateSkillGapItem(string character, string skill, int currentLevel, int recommendedLevel, string gap, string trainingTime)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 150, 50, 50)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 255, 100, 100)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3),
            Margin = new Thickness(5),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(60, 255, 100, 100),
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120, GridUnitType.Pixel) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80, GridUnitType.Pixel) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80, GridUnitType.Pixel) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100, GridUnitType.Pixel) });

        // Character name
        var characterText = new TextBlock
        {
            Text = character,
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 8, 5, 8)
        };
        Grid.SetColumn(characterText, 0);
        grid.Children.Add(characterText);

        // Skill name
        var skillText = new TextBlock
        {
            Text = skill,
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 8, 5, 8)
        };
        Grid.SetColumn(skillText, 1);
        grid.Children.Add(skillText);

        // Current level
        var currentText = new TextBlock
        {
            Text = $"Level {currentLevel}",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 200, 200)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 8, 5, 8)
        };
        Grid.SetColumn(currentText, 2);
        grid.Children.Add(currentText);

        // Gap indicator
        var gapText = new TextBlock
        {
            Text = gap,
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 100, 100)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 8, 5, 8)
        };
        Grid.SetColumn(gapText, 3);
        grid.Children.Add(gapText);

        // Training time
        var timeText = new TextBlock
        {
            Text = trainingTime,
            FontSize = 9,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 255, 255, 100)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 8, 10, 8)
        };
        Grid.SetColumn(timeText, 4);
        grid.Children.Add(timeText);

        border.Child = grid;
        return border;
    }

    private ScrollViewer CreateAdvantagesContent()
    {
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = new SolidColorBrush(Color.FromArgb(20, 0, 100, 50))
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10)
        };

        // Sample advantages
        var advantageData = new[]
        {
            new { Character = "Character A", Skill = "Caldari Battleship", Level = 5, Advantage = "2 levels ahead", Benefit = "20% better damage" },
            new { Character = "Character A", Skill = "Large Hybrid Turret", Level = 5, Advantage = "1 level ahead", Benefit = "5% better tracking" },
            new { Character = "Character B", Skill = "Shield Upgrades", Level = 5, Advantage = "3 levels ahead", Benefit = "15% more capacity" },
            new { Character = "Character B", Skill = "Capacitor Management", Level = 4, Advantage = "1 level ahead", Benefit = "5% better cap" }
        };

        foreach (var advantage in advantageData)
        {
            var advantageItem = CreateAdvantageItem(advantage.Character, advantage.Skill, advantage.Level, advantage.Advantage, advantage.Benefit);
            panel.Children.Add(advantageItem);
        }

        scrollViewer.Content = panel;
        return scrollViewer;
    }

    private Border CreateAdvantageItem(string character, string skill, int level, string advantage, string benefit)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 50, 150, 50)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 100, 255, 100)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3),
            Margin = new Thickness(5),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(60, 100, 255, 100),
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120, GridUnitType.Pixel) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100, GridUnitType.Pixel) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150, GridUnitType.Pixel) });

        // Character name
        var characterText = new TextBlock
        {
            Text = character,
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 8, 5, 8)
        };
        Grid.SetColumn(characterText, 0);
        grid.Children.Add(characterText);

        // Skill name
        var skillText = new TextBlock
        {
            Text = skill,
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 8, 5, 8)
        };
        Grid.SetColumn(skillText, 1);
        grid.Children.Add(skillText);

        // Advantage
        var advantageText = new TextBlock
        {
            Text = advantage,
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 100, 255, 100)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 8, 5, 8)
        };
        Grid.SetColumn(advantageText, 2);
        grid.Children.Add(advantageText);

        // Benefit
        var benefitText = new TextBlock
        {
            Text = benefit,
            FontSize = 9,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 100, 255, 200)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 8, 10, 8)
        };
        Grid.SetColumn(benefitText, 3);
        grid.Children.Add(benefitText);

        border.Child = grid;
        return border;
    }

    private ScrollViewer CreateSideBySideContent()
    {
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            Background = new SolidColorBrush(Color.FromArgb(20, 50, 50, 100))
        };

        _comparisonGrid = new Grid();
        
        // Create columns for each character plus skill names
        _comparisonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200, GridUnitType.Pixel) });
        _comparisonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150, GridUnitType.Pixel) });
        _comparisonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150, GridUnitType.Pixel) });

        CreateSideBySideHeaders();
        CreateSideBySideSkillRows();

        scrollViewer.Content = _comparisonGrid;
        return scrollViewer;
    }

    private void CreateSideBySideHeaders()
    {
        if (_comparisonGrid == null) return;

        // Skill header
        var skillHeader = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(80, 0, 100, 200)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(120, 0, 200, 255)),
            BorderThickness = new Thickness(1),
            Child = new TextBlock
            {
                Text = "SKILL",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 8, 10, 8)
            }
        };
        Grid.SetColumn(skillHeader, 0);
        Grid.SetRow(skillHeader, 0);
        _comparisonGrid.Children.Add(skillHeader);

        // Character headers
        var characters = new[] { "Character A", "Character B" };
        for (int i = 0; i < characters.Length; i++)
        {
            var characterHeader = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(80, 100, 0, 150)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(120, 200, 0, 255)),
                BorderThickness = new Thickness(1),
                Child = new TextBlock
                {
                    Text = characters[i].ToUpper(),
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10, 8, 10, 8)
                }
            };
            Grid.SetColumn(characterHeader, i + 1);
            Grid.SetRow(characterHeader, 0);
            _comparisonGrid.Children.Add(characterHeader);
        }
    }

    private void CreateSideBySideSkillRows()
    {
        if (_comparisonGrid == null) return;

        var skills = new[]
        {
            new { Name = "Gunnery", CharA = 5, CharB = 4 },
            new { Name = "Large Projectile Turret", CharA = 4, CharB = 3 },
            new { Name = "Weapon Upgrades", CharA = 4, CharB = 5 },
            new { Name = "Shield Management", CharA = 3, CharB = 2 },
            new { Name = "Caldari Battleship", CharA = 5, CharB = 3 },
            new { Name = "Large Hybrid Turret", CharA = 5, CharB = 4 }
        };

        for (int i = 0; i < skills.Length; i++)
        {
            _comparisonGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40, GridUnitType.Pixel) });
            
            var skill = skills[i];
            var rowIndex = i + 1;

            // Skill name
            var skillCell = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(40, 50, 50, 100)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(80, 100, 100, 150)),
                BorderThickness = new Thickness(1),
                Child = new TextBlock
                {
                    Text = skill.Name,
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Colors.White),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10, 5, 10, 5)
                }
            };
            Grid.SetColumn(skillCell, 0);
            Grid.SetRow(skillCell, rowIndex);
            _comparisonGrid.Children.Add(skillCell);

            // Character A level
            var charACell = CreateSkillLevelCell(skill.CharA, skill.CharB);
            Grid.SetColumn(charACell, 1);
            Grid.SetRow(charACell, rowIndex);
            _comparisonGrid.Children.Add(charACell);

            // Character B level
            var charBCell = CreateSkillLevelCell(skill.CharB, skill.CharA);
            Grid.SetColumn(charBCell, 2);
            Grid.SetRow(charBCell, rowIndex);
            _comparisonGrid.Children.Add(charBCell);
        }
    }

    private Border CreateSkillLevelCell(int level, int compareLevel)
    {
        Color backgroundColor;
        Color textColor;

        if (level > compareLevel)
        {
            backgroundColor = Color.FromArgb(60, 0, 150, 50); // Green for advantage
            textColor = Color.FromArgb(255, 100, 255, 100);
        }
        else if (level < compareLevel)
        {
            backgroundColor = Color.FromArgb(60, 150, 50, 0); // Red for disadvantage
            textColor = Color.FromArgb(255, 255, 100, 100);
        }
        else
        {
            backgroundColor = Color.FromArgb(40, 100, 100, 100); // Gray for equal
            textColor = Color.FromArgb(255, 200, 200, 200);
        }

        return new Border
        {
            Background = new SolidColorBrush(backgroundColor),
            BorderBrush = new SolidColorBrush(Color.FromArgb(80, 100, 100, 150)),
            BorderThickness = new Thickness(1),
            Child = new TextBlock
            {
                Text = $"Level {level}",
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(textColor),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5)
            }
        };
    }

    private Border CreateStatisticsPanel()
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
            Text = "COMPARISON STATISTICS",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        panel.Children.Add(header);

        var stats = new[]
        {
            "Total Skills Compared: 156",
            "Character A Advantages: 68",
            "Character B Advantages: 52",
            "Equal Skills: 36",
            "Average Level Difference: 1.2",
            "Training Time Gap: 45d 12h"
        };

        foreach (var stat in stats)
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

    private Border CreateTrainingPlanPanel()
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
            Text = "TRAINING RECOMMENDATIONS",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        panel.Children.Add(header);

        var recommendations = new[]
        {
            "Prioritize Gunnery V for Character B",
            "Train Shield Management for Character A",
            "Focus on Large Projectile skills",
            "Consider cross-training alternatives"
        };

        foreach (var rec in recommendations)
        {
            var recText = new TextBlock
            {
                Text = $"• {rec}",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 200)),
                Margin = new Thickness(0, 2, 0, 0)
            };
            panel.Children.Add(recText);
        }

        border.Child = panel;
        return border;
    }

    private void CreateSampleData()
    {
        ComparisonCharacters = new ObservableCollection<HoloCharacterSkills>
        {
            new HoloCharacterSkills
            {
                CharacterName = "Character A",
                TotalSkillPoints = 35750000,
                SkillCount = 128,
                AverageSkillLevel = 3.8
            },
            new HoloCharacterSkills
            {
                CharacterName = "Character B",
                TotalSkillPoints = 28500000,
                SkillCount = 115,
                AverageSkillLevel = 3.4
            }
        };
    }

    #endregion

    #region Particle System

    private void CreateComparisonParticles()
    {
        if (!EnableParticleEffects || _particleCanvas == null) return;

        for (int i = 0; i < 2; i++)
        {
            var particle = new HoloComparisonParticle
            {
                X = _random.NextDouble() * ActualWidth,
                Y = _random.NextDouble() * ActualHeight,
                VelocityX = (_random.NextDouble() - 0.5) * 15,
                VelocityY = (_random.NextDouble() - 0.5) * 15,
                Size = _random.NextDouble() * 3 + 1,
                Life = 1.0,
                ParticleType = ComparisonParticleType.Analysis,
                Ellipse = new Ellipse
                {
                    Width = 3,
                    Height = 3,
                    Fill = new SolidColorBrush(Color.FromArgb(160, 0, 255, 255))
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
            particle.Life -= 0.008;

            if (particle.Life <= 0 || particle.X < 0 || particle.X > ActualWidth || 
                particle.Y < 0 || particle.Y > ActualHeight)
            {
                _particleCanvas.Children.Remove(particle.Ellipse);
                _particles.RemoveAt(i);
                continue;
            }

            Canvas.SetLeft(particle.Ellipse, particle.X);
            Canvas.SetTop(particle.Ellipse, particle.Y);
            
            var alpha = (byte)(particle.Life * 160);
            var color = particle.ParticleType switch
            {
                ComparisonParticleType.Analysis => Color.FromArgb(alpha, 0, 255, 255),
                ComparisonParticleType.Advantage => Color.FromArgb(alpha, 0, 255, 100),
                ComparisonParticleType.Gap => Color.FromArgb(alpha, 255, 100, 100),
                _ => Color.FromArgb(alpha, 255, 255, 255)
            };
            
            particle.Ellipse.Fill = new SolidColorBrush(color);
        }

        if (_random.NextDouble() < 0.05)
        {
            CreateComparisonParticles();
        }
    }

    #endregion

    #region Animation

    private void OnAnimationTick(object sender, EventArgs e)
    {
        if (!_isAnimating) return;

        _currentAnimationTime += 16;
        
        UpdateParticles();
        UpdateComparisonAnimations();
    }

    private void UpdateComparisonAnimations()
    {
        if (!EnableAnimations) return;

        var intensity = (Math.Sin(_currentAnimationTime * 0.001) + 1) * 0.5 * HolographicIntensity;
        
        // Animate tab glow effects
        if (_comparisonTabs != null)
        {
            foreach (TabItem tab in _comparisonTabs.Items)
            {
                if (tab.IsSelected && tab.Background is SolidColorBrush brush)
                {
                    var currentColor = brush.Color;
                    var newAlpha = (byte)(60 + intensity * 40);
                    tab.Background = new SolidColorBrush(Color.FromArgb(newAlpha, currentColor.R, currentColor.G, currentColor.B));
                }
            }
        }
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _animationTimer.Start();
        _isAnimating = true;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer.Stop();
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
        if (d is HoloSkillComparison control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillComparison control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnComparisonCharactersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillComparison control)
        {
            control.RefreshComparison();
        }
    }

    private static void OnSelectedSkillCategoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillComparison control)
        {
            control.FilterByCategory();
        }
    }

    private static void OnComparisonModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillComparison control)
        {
            control.UpdateComparisonMode();
        }
    }

    private static void OnShowSkillGapsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillComparison control)
        {
            control.UpdateGapDisplay();
        }
    }

    private static void OnShowAdvantagesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillComparison control)
        {
            control.UpdateAdvantageDisplay();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillComparison control)
        {
            control._isAnimating = control.EnableAnimations;
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillComparison control)
        {
            if (!control.EnableParticleEffects)
            {
                control.ClearParticles();
            }
        }
    }

    private static void OnShowStatisticalAnalysisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillComparison control)
        {
            control._statisticsPanel.Visibility = control.ShowStatisticalAnalysis ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnShowTrainingPlansChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillComparison control)
        {
            control._trainingPlanPanel.Visibility = control.ShowTrainingPlans ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnComparisonDepthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillComparison control)
        {
            control.UpdateComparisonDepth();
        }
    }

    private static void OnGroupBySkillGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillComparison control)
        {
            control.UpdateGrouping();
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

    private void RefreshComparison()
    {
        // Refresh comparison data
    }

    private void FilterByCategory()
    {
        // Filter skills by category
    }

    private void UpdateComparisonMode()
    {
        // Update the comparison display mode
    }

    private void UpdateGapDisplay()
    {
        // Update skill gap visualization
    }

    private void UpdateAdvantageDisplay()
    {
        // Update advantage visualization
    }

    private void ClearParticles()
    {
        foreach (var particle in _particles)
        {
            _particleCanvas.Children.Remove(particle.Ellipse);
        }
        _particles.Clear();
    }

    private void UpdateComparisonDepth()
    {
        // Update depth of comparison analysis
    }

    private void UpdateGrouping()
    {
        // Update skill grouping
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
        ClearParticles();
        
        _isDisposed = true;
    }

    #endregion
}

#region Supporting Classes

public class HoloCharacterSkills
{
    public string CharacterName { get; set; } = "";
    public long TotalSkillPoints { get; set; }
    public int SkillCount { get; set; }
    public double AverageSkillLevel { get; set; }
    public List<HoloSkillInfo> Skills { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.Now;
}

public class HoloSkillInfo
{
    public string SkillName { get; set; } = "";
    public int Level { get; set; }
    public long SkillPoints { get; set; }
    public string Category { get; set; } = "";
    public string Group { get; set; } = "";
}

public class HoloComparisonParticle
{
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Size { get; set; }
    public double Life { get; set; }
    public ComparisonParticleType ParticleType { get; set; }
    public Ellipse Ellipse { get; set; } = null!;
}

public enum SkillComparisonMode
{
    SideBySide,
    Overlay,
    RadarChart,
    Matrix
}

public enum ComparisonDepth
{
    Basic,
    Detailed,
    Comprehensive
}

public enum ComparisonParticleType
{
    Analysis,
    Advantage,
    Gap,
    Neutral
}

#endregion