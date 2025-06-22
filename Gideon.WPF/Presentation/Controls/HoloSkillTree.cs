// ==========================================================================
// HoloSkillTree.cs - Holographic Skill Tree with Animated Connections
// ==========================================================================
// Advanced skill tree visualization featuring interactive skill navigation,
// animated connections, EVE-style skill prerequisites, and holographic skill progression.
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
/// Holographic skill tree with interactive navigation and animated skill connections
/// </summary>
public class HoloSkillTree : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloSkillTree),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloSkillTree),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty SkillsProperty =
        DependencyProperty.Register(nameof(Skills), typeof(ObservableCollection<HoloSkill>), typeof(HoloSkillTree),
            new PropertyMetadata(null, OnSkillsChanged));

    public static readonly DependencyProperty SelectedSkillProperty =
        DependencyProperty.Register(nameof(SelectedSkill), typeof(HoloSkill), typeof(HoloSkillTree),
            new PropertyMetadata(null, OnSelectedSkillChanged));

    public static readonly DependencyProperty SelectedCharacterProperty =
        DependencyProperty.Register(nameof(SelectedCharacter), typeof(string), typeof(HoloSkillTree),
            new PropertyMetadata("Main Character", OnSelectedCharacterChanged));

    public static readonly DependencyProperty SkillCategoryProperty =
        DependencyProperty.Register(nameof(SkillCategory), typeof(SkillCategory), typeof(HoloSkillTree),
            new PropertyMetadata(SkillCategory.All, OnSkillCategoryChanged));

    public static readonly DependencyProperty ViewModeProperty =
        DependencyProperty.Register(nameof(ViewMode), typeof(SkillTreeViewMode), typeof(HoloSkillTree),
            new PropertyMetadata(SkillTreeViewMode.Tree, OnViewModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloSkillTree),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloSkillTree),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowPrerequisitesProperty =
        DependencyProperty.Register(nameof(ShowPrerequisites), typeof(bool), typeof(HoloSkillTree),
            new PropertyMetadata(true, OnShowPrerequisitesChanged));

    public static readonly DependencyProperty ShowSkillDetailsProperty =
        DependencyProperty.Register(nameof(ShowSkillDetails), typeof(bool), typeof(HoloSkillTree),
            new PropertyMetadata(true, OnShowSkillDetailsChanged));

    public static readonly DependencyProperty ShowProgressionPathProperty =
        DependencyProperty.Register(nameof(ShowProgressionPath), typeof(bool), typeof(HoloSkillTree),
            new PropertyMetadata(true, OnShowProgressionPathChanged));

    public static readonly DependencyProperty AutoLayoutProperty =
        DependencyProperty.Register(nameof(AutoLayout), typeof(bool), typeof(HoloSkillTree),
            new PropertyMetadata(true, OnAutoLayoutChanged));

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

    public ObservableCollection<HoloSkill> Skills
    {
        get => (ObservableCollection<HoloSkill>)GetValue(SkillsProperty);
        set => SetValue(SkillsProperty, value);
    }

    public HoloSkill SelectedSkill
    {
        get => (HoloSkill)GetValue(SelectedSkillProperty);
        set => SetValue(SelectedSkillProperty, value);
    }

    public string SelectedCharacter
    {
        get => (string)GetValue(SelectedCharacterProperty);
        set => SetValue(SelectedCharacterProperty, value);
    }

    public SkillCategory SkillCategory
    {
        get => (SkillCategory)GetValue(SkillCategoryProperty);
        set => SetValue(SkillCategoryProperty, value);
    }

    public SkillTreeViewMode ViewMode
    {
        get => (SkillTreeViewMode)GetValue(ViewModeProperty);
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

    public bool ShowPrerequisites
    {
        get => (bool)GetValue(ShowPrerequisitesProperty);
        set => SetValue(ShowPrerequisitesProperty, value);
    }

    public bool ShowSkillDetails
    {
        get => (bool)GetValue(ShowSkillDetailsProperty);
        set => SetValue(ShowSkillDetailsProperty, value);
    }

    public bool ShowProgressionPath
    {
        get => (bool)GetValue(ShowProgressionPathProperty);
        set => SetValue(ShowProgressionPathProperty, value);
    }

    public bool AutoLayout
    {
        get => (bool)GetValue(AutoLayoutProperty);
        set => SetValue(AutoLayoutProperty, value);
    }

    #endregion

    #region Fields

    private Canvas _skillTreeCanvas;
    private ScrollViewer _scrollViewer;
    private DispatcherTimer _animationTimer;
    private DispatcherTimer _particleTimer;
    private Dictionary<string, Material> _materialCache;
    private Dictionary<string, FrameworkElement> _skillNodes;
    private Dictionary<string, Line> _skillConnections;
    private Random _random;
    private List<UIElement> _particles;
    private List<HoloSkillParticle> _skillParticles;
    private bool _isInitialized;

    #endregion

    #region Constructor

    public HoloSkillTree()
    {
        InitializeComponent();
        InitializeFields();
        InitializeSkillTree();
        InitializeTimers();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 1000;
        Height = 700;
        Background = new SolidColorBrush(Color.FromArgb(20, 0, 100, 200));
        
        _scrollViewer = new ScrollViewer
        {
            Width = Width,
            Height = Height,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            ZoomMode = ZoomMode.Enabled,
            Background = CreateHolographicBrush(0.1)
        };
        
        _skillTreeCanvas = new Canvas
        {
            Width = 2000,
            Height = 1500,
            ClipToBounds = false
        };
        
        _scrollViewer.Content = _skillTreeCanvas;
        Content = _scrollViewer;
    }

    private void InitializeFields()
    {
        _materialCache = new Dictionary<string, Material>();
        _skillNodes = new Dictionary<string, FrameworkElement>();
        _skillConnections = new Dictionary<string, Line>();
        _random = new Random();
        _particles = new List<UIElement>();
        _skillParticles = new List<HoloSkillParticle>();
        _isInitialized = false;
        
        Skills = new ObservableCollection<HoloSkill>();
        GenerateSampleSkills();
    }

    private void InitializeSkillTree()
    {
        CreateSkillTreeInterface();
        UpdateSkillTreeVisuals();
        LayoutSkills();
    }

    private void InitializeTimers()
    {
        _animationTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _animationTimer.Tick += OnAnimationTick;

        _particleTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _particleTimer.Tick += OnParticleTick;
    }

    private void GenerateSampleSkills()
    {
        var sampleSkills = new List<HoloSkill>
        {
            // Spaceship Command
            new() { Id = "SpaceshipCommand", Name = "Spaceship Command", Category = SkillCategory.Spaceship, Rank = 1, Level = 5, MaxLevel = 5, SkillPoints = 256000, SkillPointsToNext = 0, TrainingMultiplier = 1.0, Group = "Core", Prerequisites = new List<string>(), X = 100, Y = 100 },
            new() { Id = "Caldari_Frigate", Name = "Caldari Frigate", Category = SkillCategory.Spaceship, Rank = 2, Level = 4, MaxLevel = 5, SkillPoints = 113137, SkillPointsToNext = 142863, TrainingMultiplier = 2.0, Group = "Frigate", Prerequisites = new List<string> { "SpaceshipCommand" }, X = 200, Y = 50 },
            new() { Id = "Caldari_Destroyer", Name = "Caldari Destroyer", Category = SkillCategory.Spaceship, Rank = 5, Level = 3, MaxLevel = 5, SkillPoints = 40000, SkillPointsToNext = 186274, TrainingMultiplier = 3.0, Group = "Destroyer", Prerequisites = new List<string> { "Caldari_Frigate" }, X = 300, Y = 50 },
            new() { Id = "Caldari_Cruiser", Name = "Caldari Cruiser", Category = SkillCategory.Spaceship, Rank = 5, Level = 2, MaxLevel = 5, SkillPoints = 11314, SkillPointsToNext = 21839, TrainingMultiplier = 5.0, Group = "Cruiser", Prerequisites = new List<string> { "Caldari_Destroyer" }, X = 400, Y = 50 },
            
            // Gunnery
            new() { Id = "Gunnery", Name = "Gunnery", Category = SkillCategory.Gunnery, Rank = 1, Level = 5, MaxLevel = 5, SkillPoints = 256000, SkillPointsToNext = 0, TrainingMultiplier = 1.0, Group = "Core", Prerequisites = new List<string>(), X = 100, Y = 200 },
            new() { Id = "Small_Hybrid_Turret", Name = "Small Hybrid Turret", Category = SkillCategory.Gunnery, Rank = 2, Level = 5, MaxLevel = 5, SkillPoints = 512000, SkillPointsToNext = 0, TrainingMultiplier = 2.0, Group = "Small Turrets", Prerequisites = new List<string> { "Gunnery" }, X = 200, Y = 180 },
            new() { Id = "Medium_Hybrid_Turret", Name = "Medium Hybrid Turret", Category = SkillCategory.Gunnery, Rank = 3, Level = 4, MaxLevel = 5, SkillPoints = 181019, SkillPointsToNext = 274981, TrainingMultiplier = 3.0, Group = "Medium Turrets", Prerequisites = new List<string> { "Small_Hybrid_Turret" }, X = 300, Y = 180 },
            new() { Id = "Large_Hybrid_Turret", Name = "Large Hybrid Turret", Category = SkillCategory.Gunnery, Rank = 5, Level = 1, MaxLevel = 5, SkillPoints = 250, SkillPointsToNext = 1164, TrainingMultiplier = 5.0, Group = "Large Turrets", Prerequisites = new List<string> { "Medium_Hybrid_Turret" }, X = 400, Y = 180 },
            
            // Engineering
            new() { Id = "Engineering", Name = "Engineering", Category = SkillCategory.Engineering, Rank = 1, Level = 5, MaxLevel = 5, SkillPoints = 256000, SkillPointsToNext = 0, TrainingMultiplier = 1.0, Group = "Core", Prerequisites = new List<string>(), X = 100, Y = 300 },
            new() { Id = "Shield_Management", Name = "Shield Management", Category = SkillCategory.Engineering, Rank = 3, Level = 5, MaxLevel = 5, SkillPoints = 768000, SkillPointsToNext = 0, TrainingMultiplier = 3.0, Group = "Shields", Prerequisites = new List<string> { "Engineering" }, X = 200, Y = 280 },
            new() { Id = "Shield_Operation", Name = "Shield Operation", Category = SkillCategory.Engineering, Rank = 1, Level = 5, MaxLevel = 5, SkillPoints = 256000, SkillPointsToNext = 0, TrainingMultiplier = 1.0, Group = "Shields", Prerequisites = new List<string> { "Engineering" }, X = 200, Y = 320 },
            new() { Id = "Shield_Upgrades", Name = "Shield Upgrades", Category = SkillCategory.Engineering, Rank = 2, Level = 4, MaxLevel = 5, SkillPoints = 113137, SkillPointsToNext = 142863, TrainingMultiplier = 2.0, Group = "Shields", Prerequisites = new List<string> { "Shield_Operation" }, X = 300, Y = 320 },
            
            // Navigation
            new() { Id = "Navigation", Name = "Navigation", Category = SkillCategory.Navigation, Rank = 1, Level = 5, MaxLevel = 5, SkillPoints = 256000, SkillPointsToNext = 0, TrainingMultiplier = 1.0, Group = "Core", Prerequisites = new List<string>(), X = 100, Y = 400 },
            new() { Id = "Afterburner", Name = "Afterburner", Category = SkillCategory.Navigation, Rank = 1, Level = 4, MaxLevel = 5, SkillPoints = 90510, SkillPointsToNext = 165490, TrainingMultiplier = 1.0, Group = "Propulsion", Prerequisites = new List<string> { "Navigation" }, X = 200, Y = 400 },
            new() { Id = "High_Speed_Maneuvering", Name = "High Speed Maneuvering", Category = SkillCategory.Navigation, Rank = 5, Level = 2, MaxLevel = 5, SkillPoints = 11314, SkillPointsToNext = 21839, TrainingMultiplier = 5.0, Group = "Propulsion", Prerequisites = new List<string> { "Afterburner" }, X = 300, Y = 400 },
            
            // Targeting
            new() { Id = "Targeting", Name = "Targeting", Category = SkillCategory.Targeting, Rank = 1, Level = 5, MaxLevel = 5, SkillPoints = 256000, SkillPointsToNext = 0, TrainingMultiplier = 1.0, Group = "Core", Prerequisites = new List<string>(), X = 100, Y = 500 },
            new() { Id = "Long_Range_Targeting", Name = "Long Range Targeting", Category = SkillCategory.Targeting, Rank = 2, Level = 3, MaxLevel = 5, SkillPoints = 24000, SkillPointsToNext = 111127, TrainingMultiplier = 2.0, Group = "Advanced", Prerequisites = new List<string> { "Targeting" }, X = 200, Y = 500 },
            new() { Id = "Signature_Analysis", Name = "Signature Analysis", Category = SkillCategory.Targeting, Rank = 1, Level = 4, MaxLevel = 5, SkillPoints = 90510, SkillPointsToNext = 165490, TrainingMultiplier = 1.0, Group = "Advanced", Prerequisites = new List<string> { "Targeting" }, X = 200, Y = 540 }
        };
        
        foreach (var skill in sampleSkills)
        {
            Skills.Add(skill);
        }
    }

    #endregion

    #region Skill Tree Interface Creation

    private void CreateSkillTreeInterface()
    {
        _skillTreeCanvas.Children.Clear();
        _skillNodes.Clear();
        _skillConnections.Clear();
        
        CreateHeaderSection();
        CreateSkillNodes();
        CreateSkillConnections();
        CreateParticleEffects();
    }

    private void CreateHeaderSection()
    {
        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Background = CreateHolographicBrush(0.4),
            Width = _skillTreeCanvas.Width
        };
        
        Canvas.SetLeft(headerPanel, 0);
        Canvas.SetTop(headerPanel, 0);
        _skillTreeCanvas.Children.Add(headerPanel);
        
        var titleBlock = new TextBlock
        {
            Text = $"Skill Tree - {SelectedCharacter}",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary"),
            Margin = new Thickness(20, 10),
            VerticalAlignment = VerticalAlignment.Center
        };
        headerPanel.Children.Add(titleBlock);
        
        AddHeaderButton(headerPanel, "All", () => SkillCategory = SkillCategory.All);
        AddHeaderButton(headerPanel, "Spaceship", () => SkillCategory = SkillCategory.Spaceship);
        AddHeaderButton(headerPanel, "Gunnery", () => SkillCategory = SkillCategory.Gunnery);
        AddHeaderButton(headerPanel, "Engineering", () => SkillCategory = SkillCategory.Engineering);
        AddHeaderButton(headerPanel, "Navigation", () => SkillCategory = SkillCategory.Navigation);
    }

    private void CreateSkillNodes()
    {
        var filteredSkills = GetFilteredSkills();
        
        foreach (var skill in filteredSkills)
        {
            var skillNode = CreateSkillNode(skill);
            _skillNodes[skill.Id] = skillNode;
            _skillTreeCanvas.Children.Add(skillNode);
        }
    }

    private void CreateSkillConnections()
    {
        if (!ShowPrerequisites) return;
        
        var filteredSkills = GetFilteredSkills();
        
        foreach (var skill in filteredSkills)
        {
            foreach (var prerequisiteId in skill.Prerequisites)
            {
                var prerequisite = Skills.FirstOrDefault(s => s.Id == prerequisiteId);
                if (prerequisite != null && _skillNodes.ContainsKey(skill.Id) && _skillNodes.ContainsKey(prerequisiteId))
                {
                    var connection = CreateSkillConnection(prerequisite, skill);
                    var connectionKey = $"{prerequisiteId}_to_{skill.Id}";
                    _skillConnections[connectionKey] = connection;
                    _skillTreeCanvas.Children.Insert(0, connection); // Add connections behind nodes
                }
            }
        }
    }

    private FrameworkElement CreateSkillNode(HoloSkill skill)
    {
        var skillControl = new UserControl
        {
            Width = 120,
            Height = 80,
            Cursor = Cursors.Hand
        };
        
        Canvas.SetLeft(skillControl, skill.X);
        Canvas.SetTop(skillControl, skill.Y + 50); // Offset for header
        
        var border = new Border
        {
            Background = CreateSkillNodeBrush(skill),
            BorderBrush = GetSkillBorderBrush(skill),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(5),
            Effect = new DropShadowEffect
            {
                BlurRadius = 10,
                Opacity = 0.5,
                ShadowDepth = 3,
                Color = GetEVEColor("Accent")
            }
        };
        
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5)
        };
        
        var nameBlock = new TextBlock
        {
            Text = skill.Name,
            Foreground = GetEVEBrush("Primary"),
            FontWeight = FontWeights.Bold,
            FontSize = 11,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 110
        };
        
        var levelPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 3, 0, 0)
        };
        
        var levelBlock = new TextBlock
        {
            Text = $"Level {skill.Level}",
            Foreground = GetEVEBrush("Secondary"),
            FontSize = 10,
            FontWeight = FontWeights.Bold
        };
        
        var maxLevelBlock = new TextBlock
        {
            Text = $"/{skill.MaxLevel}",
            Foreground = GetEVEBrush("Secondary"),
            FontSize = 9,
            Margin = new Thickness(2, 1, 0, 0)
        };
        
        levelPanel.Children.Add(levelBlock);
        levelPanel.Children.Add(maxLevelBlock);
        
        var progressBar = new ProgressBar
        {
            Width = 100,
            Height = 4,
            Value = GetSkillProgress(skill),
            Maximum = 100,
            Foreground = GetSkillProgressBrush(skill),
            Background = CreateHolographicBrush(0.3),
            Margin = new Thickness(0, 2, 0, 0)
        };
        
        var spBlock = new TextBlock
        {
            Text = FormatSkillPoints(skill.SkillPoints),
            Foreground = GetEVEBrush("Info"),
            FontSize = 8,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 1, 0, 0)
        };
        
        panel.Children.Add(nameBlock);
        panel.Children.Add(levelPanel);
        panel.Children.Add(progressBar);
        panel.Children.Add(spBlock);
        
        border.Child = panel;
        skillControl.Content = border;
        
        skillControl.MouseEnter += (s, e) => OnSkillNodeMouseEnter(skill, skillControl);
        skillControl.MouseLeave += (s, e) => OnSkillNodeMouseLeave(skill, skillControl);
        skillControl.MouseLeftButtonDown += (s, e) => OnSkillNodeClick(skill);
        
        if (EnableAnimations)
        {
            ApplySkillNodeAnimation(skillControl, skill);
        }
        
        return skillControl;
    }

    private Line CreateSkillConnection(HoloSkill fromSkill, HoloSkill toSkill)
    {
        var line = new Line
        {
            X1 = fromSkill.X + 60, // Center of from node
            Y1 = fromSkill.Y + 90,  // Bottom of from node + header offset
            X2 = toSkill.X + 60,    // Center of to node
            Y2 = toSkill.Y + 50,    // Top of to node + header offset
            Stroke = GetConnectionBrush(fromSkill, toSkill),
            StrokeThickness = 2,
            Opacity = 0.7
        };
        
        if (EnableAnimations)
        {
            ApplyConnectionAnimation(line);
        }
        
        return line;
    }

    #endregion

    #region Skill Management

    private IEnumerable<HoloSkill> GetFilteredSkills()
    {
        if (Skills == null) return Enumerable.Empty<HoloSkill>();
        
        var filtered = Skills.AsEnumerable();
        
        if (SkillCategory != SkillCategory.All)
        {
            filtered = filtered.Where(s => s.Category == SkillCategory);
        }
        
        return filtered;
    }

    private void LayoutSkills()
    {
        if (!AutoLayout) return;
        
        var skills = GetFilteredSkills().ToList();
        var categories = skills.GroupBy(s => s.Category).ToList();
        
        var categoryY = 100;
        
        foreach (var categoryGroup in categories)
        {
            var categorySkills = categoryGroup.OrderBy(s => s.Rank).ThenBy(s => s.Name).ToList();
            var groups = categorySkills.GroupBy(s => s.Group).ToList();
            
            var groupX = 100;
            
            foreach (var group in groups)
            {
                var groupSkills = group.ToList();
                var skillY = categoryY;
                
                foreach (var skill in groupSkills)
                {
                    skill.X = groupX;
                    skill.Y = skillY;
                    skillY += 100;
                }
                
                groupX += 150;
            }
            
            categoryY += (groups.Count > 0 ? groups.Max(g => g.Count()) : 1) * 100 + 50;
        }
        
        CreateSkillTreeInterface();
    }

    private double GetSkillProgress(HoloSkill skill)
    {
        if (skill.Level >= skill.MaxLevel) return 100;
        
        var totalSPForCurrentLevel = GetSkillPointsForLevel(skill.Rank, skill.Level);
        var totalSPForNextLevel = GetSkillPointsForLevel(skill.Rank, skill.Level + 1);
        var spInCurrentLevel = skill.SkillPoints - GetSkillPointsForLevel(skill.Rank, skill.Level);
        var spNeededForNextLevel = totalSPForNextLevel - totalSPForCurrentLevel;
        
        return spNeededForNextLevel > 0 ? (spInCurrentLevel / spNeededForNextLevel) * 100 : 100;
    }

    private long GetSkillPointsForLevel(int rank, int level)
    {
        if (level <= 0) return 0;
        
        var basePoints = new[] { 0, 250, 1414, 8000, 45255, 256000 };
        return level <= 5 ? basePoints[level] * rank : basePoints[5] * rank;
    }

    private string FormatSkillPoints(long skillPoints)
    {
        if (skillPoints >= 1_000_000)
            return $"{skillPoints / 1_000_000.0:F1}M SP";
        if (skillPoints >= 1_000)
            return $"{skillPoints / 1_000.0:F0}K SP";
        return $"{skillPoints} SP";
    }

    #endregion

    #region Event Handlers

    private void OnSkillNodeMouseEnter(HoloSkill skill, FrameworkElement node)
    {
        if (EnableAnimations)
        {
            var scaleTransform = new ScaleTransform(1.1, 1.1);
            node.RenderTransform = scaleTransform;
            node.RenderTransformOrigin = new Point(0.5, 0.5);
            
            var storyboard = new Storyboard();
            var scaleAnimation = new DoubleAnimation
            {
                To = 1.1,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            
            Storyboard.SetTarget(scaleAnimation, scaleTransform);
            Storyboard.SetTargetProperty(scaleAnimation, new PropertyPath("ScaleX"));
            storyboard.Children.Add(scaleAnimation);
            
            var scaleYAnimation = scaleAnimation.Clone();
            Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("ScaleY"));
            storyboard.Children.Add(scaleYAnimation);
            
            storyboard.Begin();
        }
        
        HighlightSkillPath(skill);
    }

    private void OnSkillNodeMouseLeave(HoloSkill skill, FrameworkElement node)
    {
        if (EnableAnimations)
        {
            var storyboard = new Storyboard();
            var scaleAnimation = new DoubleAnimation
            {
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            
            Storyboard.SetTarget(scaleAnimation, node.RenderTransform);
            Storyboard.SetTargetProperty(scaleAnimation, new PropertyPath("ScaleX"));
            storyboard.Children.Add(scaleAnimation);
            
            var scaleYAnimation = scaleAnimation.Clone();
            Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("ScaleY"));
            storyboard.Children.Add(scaleYAnimation);
            
            storyboard.Begin();
        }
        
        ClearSkillPathHighlight();
    }

    private void OnSkillNodeClick(HoloSkill skill)
    {
        SelectedSkill = skill;
        
        if (EnableAnimations)
        {
            AnimateSkillSelection(skill);
        }
    }

    private void HighlightSkillPath(HoloSkill skill)
    {
        if (!ShowProgressionPath) return;
        
        // Highlight prerequisites
        foreach (var prerequisiteId in skill.Prerequisites)
        {
            var connectionKey = $"{prerequisiteId}_to_{skill.Id}";
            if (_skillConnections.TryGetValue(connectionKey, out var connection))
            {
                connection.Stroke = GetEVEBrush("Success");
                connection.StrokeThickness = 3;
                connection.Opacity = 1.0;
            }
        }
        
        // Highlight dependent skills
        var dependentSkills = Skills.Where(s => s.Prerequisites.Contains(skill.Id));
        foreach (var dependent in dependentSkills)
        {
            var connectionKey = $"{skill.Id}_to_{dependent.Id}";
            if (_skillConnections.TryGetValue(connectionKey, out var connection))
            {
                connection.Stroke = GetEVEBrush("Warning");
                connection.StrokeThickness = 3;
                connection.Opacity = 1.0;
            }
        }
    }

    private void ClearSkillPathHighlight()
    {
        foreach (var connection in _skillConnections.Values)
        {
            connection.Stroke = GetEVEBrush("Secondary");
            connection.StrokeThickness = 2;
            connection.Opacity = 0.7;
        }
    }

    #endregion

    #region Visual Helpers

    private Brush CreateSkillNodeBrush(HoloSkill skill)
    {
        var baseOpacity = 0.3;
        
        if (skill.Level >= skill.MaxLevel)
        {
            var color = GetEVEColor("Success");
            color.A = (byte)(255 * baseOpacity * HolographicIntensity);
            return new SolidColorBrush(color);
        }
        else if (skill.Level > 0)
        {
            var color = GetEVEColor("Info");
            color.A = (byte)(255 * baseOpacity * HolographicIntensity);
            return new SolidColorBrush(color);
        }
        else
        {
            var color = GetEVEColor("Background");
            color.A = (byte)(255 * baseOpacity * HolographicIntensity);
            return new SolidColorBrush(color);
        }
    }

    private Brush GetSkillBorderBrush(HoloSkill skill)
    {
        return skill.Level switch
        {
            5 => GetEVEBrush("Success"),
            > 0 => GetEVEBrush("Info"),
            _ => GetEVEBrush("Secondary")
        };
    }

    private Brush GetSkillProgressBrush(HoloSkill skill)
    {
        return skill.Level >= skill.MaxLevel ? GetEVEBrush("Success") : GetEVEBrush("Warning");
    }

    private Brush GetConnectionBrush(HoloSkill fromSkill, HoloSkill toSkill)
    {
        if (fromSkill.Level >= toSkill.Level && fromSkill.Level > 0)
        {
            return GetEVEBrush("Success");
        }
        else if (fromSkill.Level > 0)
        {
            return GetEVEBrush("Warning");
        }
        else
        {
            return GetEVEBrush("Secondary");
        }
    }

    private Brush CreateHolographicBrush(double opacity)
    {
        var color = GetEVEColor("Background");
        color.A = (byte)(255 * opacity * HolographicIntensity);
        return new SolidColorBrush(color);
    }

    private Brush GetEVEBrush(string type)
    {
        return new SolidColorBrush(GetEVEColor(type));
    }

    private Color GetEVEColor(string type)
    {
        return type switch
        {
            "Primary" => EVEColorScheme switch
            {
                EVEColorScheme.ElectricBlue => Color.FromRgb(0, 150, 255),
                EVEColorScheme.Gold => Color.FromRgb(255, 215, 0),
                EVEColorScheme.Silver => Color.FromRgb(192, 192, 192),
                _ => Color.FromRgb(0, 150, 255)
            },
            "Secondary" => Color.FromRgb(150, 200, 255),
            "Accent" => Color.FromRgb(255, 200, 0),
            "Background" => Color.FromRgb(0, 30, 60),
            "Success" => Color.FromRgb(0, 255, 100),
            "Warning" => Color.FromRgb(255, 165, 0),
            "Negative" => Color.FromRgb(255, 50, 50),
            "Info" => Color.FromRgb(100, 200, 255),
            _ => Color.FromRgb(255, 255, 255)
        };
    }

    #endregion

    #region Animation Methods

    private void ApplySkillNodeAnimation(FrameworkElement node, HoloSkill skill)
    {
        var delay = _random.NextDouble() * 2000;
        
        node.Opacity = 0;
        
        var storyboard = new Storyboard { BeginTime = TimeSpan.FromMilliseconds(delay) };
        
        var fadeAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(800),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        
        var scaleTransform = new ScaleTransform(0.1, 0.1);
        node.RenderTransform = scaleTransform;
        node.RenderTransformOrigin = new Point(0.5, 0.5);
        
        var scaleAnimation = new DoubleAnimation
        {
            From = 0.1,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(800),
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
        };
        
        Storyboard.SetTarget(fadeAnimation, node);
        Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath("Opacity"));
        storyboard.Children.Add(fadeAnimation);
        
        Storyboard.SetTarget(scaleAnimation, scaleTransform);
        Storyboard.SetTargetProperty(scaleAnimation, new PropertyPath("ScaleX"));
        storyboard.Children.Add(scaleAnimation);
        
        var scaleYAnimation = scaleAnimation.Clone();
        Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("ScaleY"));
        storyboard.Children.Add(scaleYAnimation);
        
        storyboard.Begin();
    }

    private void ApplyConnectionAnimation(Line connection)
    {
        var delay = _random.NextDouble() * 3000 + 1000;
        
        connection.Opacity = 0;
        
        var storyboard = new Storyboard { BeginTime = TimeSpan.FromMilliseconds(delay) };
        
        var fadeAnimation = new DoubleAnimation
        {
            From = 0,
            To = 0.7,
            Duration = TimeSpan.FromMilliseconds(1000),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        
        Storyboard.SetTarget(fadeAnimation, connection);
        Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath("Opacity"));
        storyboard.Children.Add(fadeAnimation);
        
        storyboard.Begin();
    }

    private void AnimateSkillSelection(HoloSkill skill)
    {
        if (!_skillNodes.TryGetValue(skill.Id, out var node)) return;
        
        var glowEffect = new DropShadowEffect
        {
            BlurRadius = 20,
            Opacity = 0.8,
            ShadowDepth = 0,
            Color = GetEVEColor("Accent")
        };
        
        node.Effect = glowEffect;
        
        var storyboard = new Storyboard();
        var glowAnimation = new DoubleAnimation
        {
            From = 0.8,
            To = 0.3,
            Duration = TimeSpan.FromSeconds(2),
            AutoReverse = true,
            RepeatBehavior = new RepeatBehavior(3)
        };
        
        Storyboard.SetTarget(glowAnimation, glowEffect);
        Storyboard.SetTargetProperty(glowAnimation, new PropertyPath("Opacity"));
        storyboard.Children.Add(glowAnimation);
        
        storyboard.Begin();
    }

    private HoloSkillParticle CreateSkillParticle()
    {
        var particle = new HoloSkillParticle
        {
            Element = new Ellipse
            {
                Width = _random.Next(2, 6),
                Height = _random.Next(2, 6),
                Fill = GetEVEBrush("Accent"),
                Opacity = 0.6
            },
            X = _random.NextDouble() * _skillTreeCanvas.Width,
            Y = _random.NextDouble() * _skillTreeCanvas.Height,
            VelocityX = (_random.NextDouble() - 0.5) * 2,
            VelocityY = (_random.NextDouble() - 0.5) * 2,
            Life = 1.0,
            MaxLife = _random.NextDouble() * 10 + 5
        };
        
        Canvas.SetLeft(particle.Element, particle.X);
        Canvas.SetTop(particle.Element, particle.Y);
        
        return particle;
    }

    private void CreateParticleEffects()
    {
        if (!EnableParticleEffects) return;
        
        for (int i = 0; i < 50; i++)
        {
            var particle = CreateSkillParticle();
            _skillParticles.Add(particle);
            _skillTreeCanvas.Children.Add(particle.Element);
        }
    }

    #endregion

    #region UI Helpers

    private void AddHeaderButton(Panel parent, string text, Action clickAction)
    {
        var button = new Button
        {
            Content = text,
            Margin = new Thickness(5),
            Padding = new Thickness(10, 5),
            Background = CreateHolographicBrush(0.3),
            Foreground = GetEVEBrush("Primary"),
            BorderBrush = GetEVEBrush("Accent")
        };
        button.Click += (s, e) => clickAction();
        parent.Children.Add(button);
    }

    #endregion

    #region Timer Events

    private void OnAnimationTick(object sender, EventArgs e)
    {
        UpdateSkillTreeVisuals();
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        UpdateParticleEffects();
    }

    #endregion

    #region Update Methods

    private void UpdateSkillTreeVisuals()
    {
        if (!EnableAnimations) return;
        
        var time = DateTime.Now.TimeOfDay.TotalSeconds;
        var intensity = HolographicIntensity * (0.9 + 0.1 * Math.Sin(time * 2));
        
        foreach (var connection in _skillConnections.Values)
        {
            if (connection.Stroke is SolidColorBrush brush)
            {
                var color = brush.Color;
                color.A = (byte)(color.A * intensity);
                connection.Stroke = new SolidColorBrush(color);
            }
        }
    }

    private void UpdateParticleEffects()
    {
        if (!EnableParticleEffects) return;
        
        foreach (var particle in _skillParticles.ToList())
        {
            particle.X += particle.VelocityX;
            particle.Y += particle.VelocityY;
            particle.Life -= 0.05;
            
            if (particle.X < 0 || particle.X > _skillTreeCanvas.Width || 
                particle.Y < 0 || particle.Y > _skillTreeCanvas.Height || 
                particle.Life <= 0)
            {
                particle.X = _random.NextDouble() * _skillTreeCanvas.Width;
                particle.Y = _random.NextDouble() * _skillTreeCanvas.Height;
                particle.Life = particle.MaxLife;
            }
            
            Canvas.SetLeft(particle.Element, particle.X);
            Canvas.SetTop(particle.Element, particle.Y);
            particle.Element.Opacity = particle.Life / particle.MaxLife * 0.6;
        }
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _isInitialized = true;
        
        if (EnableAnimations)
            _animationTimer.Start();
        if (EnableParticleEffects)
            _particleTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        _particleTimer?.Stop();
    }

    #endregion

    #region Dependency Property Callbacks

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillTree skillTree)
        {
            skillTree.UpdateSkillTreeVisuals();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillTree skillTree)
        {
            skillTree.CreateSkillTreeInterface();
        }
    }

    private static void OnSkillsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillTree skillTree)
        {
            skillTree.CreateSkillTreeInterface();
        }
    }

    private static void OnSelectedSkillChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillTree skillTree && e.NewValue is HoloSkill skill)
        {
            skillTree.AnimateSkillSelection(skill);
        }
    }

    private static void OnSelectedCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillTree skillTree)
        {
            skillTree.CreateSkillTreeInterface();
        }
    }

    private static void OnSkillCategoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillTree skillTree)
        {
            skillTree.LayoutSkills();
        }
    }

    private static void OnViewModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillTree skillTree)
        {
            skillTree.CreateSkillTreeInterface();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillTree skillTree)
        {
            if ((bool)e.NewValue)
                skillTree._animationTimer?.Start();
            else
                skillTree._animationTimer?.Stop();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillTree skillTree)
        {
            if ((bool)e.NewValue)
            {
                skillTree.CreateParticleEffects();
                skillTree._particleTimer?.Start();
            }
            else
            {
                skillTree._particleTimer?.Stop();
                skillTree._skillParticles.Clear();
            }
        }
    }

    private static void OnShowPrerequisitesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillTree skillTree)
        {
            skillTree.CreateSkillTreeInterface();
        }
    }

    private static void OnShowSkillDetailsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillTree skillTree)
        {
            skillTree.CreateSkillTreeInterface();
        }
    }

    private static void OnShowProgressionPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillTree skillTree)
        {
            skillTree.CreateSkillTreeInterface();
        }
    }

    private static void OnAutoLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillTree skillTree)
        {
            skillTree.LayoutSkills();
        }
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void EnableSimplifiedMode()
    {
        EnableAnimations = false;
        EnableParticleEffects = false;
        ShowPrerequisites = false;
        ShowProgressionPath = false;
    }

    public void EnableFullMode()
    {
        EnableAnimations = true;
        EnableParticleEffects = true;
        ShowPrerequisites = true;
        ShowProgressionPath = true;
    }

    #endregion
}

#region Supporting Classes

public class HoloSkill
{
    public string Id { get; set; }
    public string Name { get; set; }
    public SkillCategory Category { get; set; }
    public int Rank { get; set; }
    public int Level { get; set; }
    public int MaxLevel { get; set; }
    public long SkillPoints { get; set; }
    public long SkillPointsToNext { get; set; }
    public double TrainingMultiplier { get; set; }
    public string Group { get; set; }
    public List<string> Prerequisites { get; set; } = new();
    public double X { get; set; }
    public double Y { get; set; }
}

public class HoloSkillParticle
{
    public UIElement Element { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
    public double MaxLife { get; set; }
}

public enum SkillCategory
{
    All,
    Spaceship,
    Gunnery,
    Missiles,
    Engineering,
    Electronics,
    Navigation,
    Targeting,
    Rigging,
    Subsystems,
    Resource,
    Production,
    Science,
    Trade,
    Corporation,
    Fleet,
    Social
}

public enum SkillTreeViewMode
{
    Tree,
    List,
    Grid
}

#endregion