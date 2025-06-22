// ==========================================================================
// HoloCharacterPlanning.cs - Holographic Character Planning Module UI
// ==========================================================================
// Character planning interface featuring skill queue management, 
// attribute planning, training time calculations, and holographic skill trees.
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
/// Holographic character planning module with skill management and training optimization
/// </summary>
public class HoloCharacterPlanning : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloCharacterPlanning),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloCharacterPlanning),
            new PropertyMetadata(EVEColorScheme.VoidPurple, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty CurrentCharacterProperty =
        DependencyProperty.Register(nameof(CurrentCharacter), typeof(HoloCharacterInfo), typeof(HoloCharacterPlanning),
            new PropertyMetadata(null, OnCurrentCharacterChanged));

    public static readonly DependencyProperty SelectedSkillProperty =
        DependencyProperty.Register(nameof(SelectedSkill), typeof(HoloSkillInfo), typeof(HoloCharacterPlanning),
            new PropertyMetadata(null, OnSelectedSkillChanged));

    public static readonly DependencyProperty PlanningModeProperty =
        DependencyProperty.Register(nameof(PlanningMode), typeof(CharacterPlanningMode), typeof(HoloCharacterPlanning),
            new PropertyMetadata(CharacterPlanningMode.SkillQueue, OnPlanningModeChanged));

    public static readonly DependencyProperty EnableSkillAnimationsProperty =
        DependencyProperty.Register(nameof(EnableSkillAnimations), typeof(bool), typeof(HoloCharacterPlanning),
            new PropertyMetadata(true, OnEnableSkillAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloCharacterPlanning),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowSkillTreeProperty =
        DependencyProperty.Register(nameof(ShowSkillTree), typeof(bool), typeof(HoloCharacterPlanning),
            new PropertyMetadata(true));

    public static readonly DependencyProperty SkillQueueProperty =
        DependencyProperty.Register(nameof(SkillQueue), typeof(ObservableCollection<HoloQueuedSkill>), typeof(HoloCharacterPlanning),
            new PropertyMetadata(null));

    public static readonly DependencyProperty AvailableSkillsProperty =
        DependencyProperty.Register(nameof(AvailableSkills), typeof(ObservableCollection<HoloSkillInfo>), typeof(HoloCharacterPlanning),
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

    public HoloCharacterInfo CurrentCharacter
    {
        get => (HoloCharacterInfo)GetValue(CurrentCharacterProperty);
        set => SetValue(CurrentCharacterProperty, value);
    }

    public HoloSkillInfo SelectedSkill
    {
        get => (HoloSkillInfo)GetValue(SelectedSkillProperty);
        set => SetValue(SelectedSkillProperty, value);
    }

    public CharacterPlanningMode PlanningMode
    {
        get => (CharacterPlanningMode)GetValue(PlanningModeProperty);
        set => SetValue(PlanningModeProperty, value);
    }

    public bool EnableSkillAnimations
    {
        get => (bool)GetValue(EnableSkillAnimationsProperty);
        set => SetValue(EnableSkillAnimationsProperty, value);
    }

    public bool EnableParticleEffects
    {
        get => (bool)GetValue(EnableParticleEffectsProperty);
        set => SetValue(EnableParticleEffectsProperty, value);
    }

    public bool ShowSkillTree
    {
        get => (bool)GetValue(ShowSkillTreeProperty);
        set => SetValue(ShowSkillTreeProperty, value);
    }

    public ObservableCollection<HoloQueuedSkill> SkillQueue
    {
        get => (ObservableCollection<HoloQueuedSkill>)GetValue(SkillQueueProperty);
        set => SetValue(SkillQueueProperty, value);
    }

    public ObservableCollection<HoloSkillInfo> AvailableSkills
    {
        get => (ObservableCollection<HoloSkillInfo>)GetValue(AvailableSkillsProperty);
        set => SetValue(AvailableSkillsProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloCharacterPlanningEventArgs> SkillAdded;
    public event EventHandler<HoloCharacterPlanningEventArgs> SkillRemoved;
    public event EventHandler<HoloCharacterPlanningEventArgs> QueueReordered;
    public event EventHandler<HoloCharacterPlanningEventArgs> PlanOptimized;
    public event EventHandler<HoloCharacterPlanningEventArgs> AttributesChanged;

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode { get; private set; }

    public void SwitchToFullMode()
    {
        IsInSimplifiedMode = false;
        EnableSkillAnimations = true;
        EnableParticleEffects = true;
        ShowSkillTree = true;
        UpdatePlanningAppearance();
    }

    public void SwitchToSimplifiedMode()
    {
        IsInSimplifiedMode = true;
        EnableSkillAnimations = false;
        EnableParticleEffects = false;
        ShowSkillTree = false;
        UpdatePlanningAppearance();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => !_disposed && IsLoaded;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        EnableSkillAnimations = settings.EnabledFeatures.HasFlag(AnimationFeatures.ComplexTransitions);
        EnableParticleEffects = settings.EnabledFeatures.HasFlag(AnimationFeatures.ParticleEffects);
        
        UpdatePlanningAppearance();
    }

    #endregion

    #region Fields

    private Grid _mainGrid;
    private Grid _skillTreeGrid;
    private Grid _queueGrid;
    private Grid _attributesGrid;
    private Grid _optimizationGrid;
    private Canvas _skillTreeCanvas;
    private Canvas _effectCanvas;
    private ItemsControl _queueList;
    private TreeView _skillTreeView;
    private StackPanel _attributesPanel;
    private StackPanel _optimizationPanel;
    private ComboBox _planningModeSelector;
    private Button _optimizeButton;
    private Button _clearQueueButton;
    private ProgressBar _trainingProgress;
    private TextBlock _trainingTimeText;
    
    private readonly Dictionary<string, HoloSkillNode> _skillNodes = new();
    private readonly List<SkillConnection> _skillConnections = new();
    private readonly List<PlanningParticle> _particles = new();
    private DispatcherTimer _animationTimer;
    private DispatcherTimer _particleTimer;
    private double _animationPhase = 0;
    private double _particlePhase = 0;
    private readonly Random _random = new();
    private bool _disposed = false;
    private HoloSkillInfo _draggedSkill;

    #endregion

    #region Constructor

    public HoloCharacterPlanning()
    {
        InitializePlanning();
        SetupAnimations();
        
        // Register with animation intensity manager
        AnimationIntensityManager.Instance.RegisterTarget(this);
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        
        // Initialize collections
        SkillQueue = new ObservableCollection<HoloQueuedSkill>();
        AvailableSkills = new ObservableCollection<HoloSkillInfo>();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Add skill to training queue
    /// </summary>
    public void AddSkillToQueue(HoloSkillInfo skill, int targetLevel)
    {
        if (skill == null || targetLevel < 1 || targetLevel > 5) return;
        
        var queuedSkill = new HoloQueuedSkill
        {
            Skill = skill,
            TargetLevel = targetLevel,
            CurrentLevel = skill.CurrentLevel,
            Position = SkillQueue.Count,
            TrainingTime = CalculateTrainingTime(skill, targetLevel)
        };

        SkillQueue.Add(queuedSkill);
        UpdateTrainingTime();
        
        if (EnableSkillAnimations && !IsInSimplifiedMode)
        {
            AnimateSkillAddition(queuedSkill);
        }

        SkillAdded?.Invoke(this, new HoloCharacterPlanningEventArgs 
        { 
            Skill = skill,
            TargetLevel = targetLevel,
            QueuePosition = queuedSkill.Position,
            Timestamp = DateTime.Now
        });
    }

    /// <summary>
    /// Remove skill from training queue
    /// </summary>
    public void RemoveSkillFromQueue(HoloQueuedSkill queuedSkill)
    {
        if (queuedSkill == null) return;
        
        SkillQueue.Remove(queuedSkill);
        ReorderQueue();
        UpdateTrainingTime();

        SkillRemoved?.Invoke(this, new HoloCharacterPlanningEventArgs 
        { 
            Skill = queuedSkill.Skill,
            TargetLevel = queuedSkill.TargetLevel,
            Timestamp = DateTime.Now
        });
    }

    /// <summary>
    /// Optimize skill training order
    /// </summary>
    public async Task OptimizeTrainingPlanAsync()
    {
        if (SkillQueue.Count < 2) return;

        // Simulate optimization algorithm
        await Task.Delay(1000);
        
        // Sort by training time efficiency
        var optimizedQueue = SkillQueue
            .OrderBy(s => s.TrainingTime.TotalDays / (s.TargetLevel - s.CurrentLevel))
            .ToList();

        SkillQueue.Clear();
        for (int i = 0; i < optimizedQueue.Count; i++)
        {
            optimizedQueue[i].Position = i;
            SkillQueue.Add(optimizedQueue[i]);
        }

        UpdateTrainingTime();
        
        if (EnableParticleEffects && !IsInSimplifiedMode)
        {
            SpawnOptimizationParticles();
        }

        PlanOptimized?.Invoke(this, new HoloCharacterPlanningEventArgs 
        { 
            Timestamp = DateTime.Now
        });
    }

    /// <summary>
    /// Clear all skills from queue
    /// </summary>
    public void ClearSkillQueue()
    {
        SkillQueue.Clear();
        UpdateTrainingTime();
    }

    /// <summary>
    /// Calculate optimal attribute mapping
    /// </summary>
    public HoloAttributeMapping CalculateOptimalAttributes()
    {
        if (CurrentCharacter == null) return null;

        // Analyze queue skills to determine optimal attributes
        var primarySkills = SkillQueue
            .GroupBy(s => s.Skill.PrimaryAttribute)
            .OrderByDescending(g => g.Sum(s => s.TrainingTime.TotalDays))
            .FirstOrDefault();

        var secondarySkills = SkillQueue
            .GroupBy(s => s.Skill.SecondaryAttribute)
            .OrderByDescending(g => g.Sum(s => s.TrainingTime.TotalDays))
            .FirstOrDefault();

        return new HoloAttributeMapping
        {
            PrimaryAttribute = primarySkills?.Key ?? SkillAttribute.Intelligence,
            SecondaryAttribute = secondarySkills?.Key ?? SkillAttribute.Memory,
            EstimatedTimeSaving = TimeSpan.FromDays(_random.NextDouble() * 10)
        };
    }

    /// <summary>
    /// Search for skills by name or category
    /// </summary>
    public List<HoloSkillInfo> SearchSkills(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || AvailableSkills == null)
            return new List<HoloSkillInfo>();

        return AvailableSkills
            .Where(s => s.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                       s.Category.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderBy(s => s.Name)
            .ToList();
    }

    #endregion

    #region Private Methods

    private void InitializePlanning()
    {
        CreatePlanningInterface();
        UpdatePlanningAppearance();
    }

    private void CreatePlanningInterface()
    {
        // Main grid with flexible layout
        _mainGrid = new Grid();
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Header section
        CreateHeaderSection();
        
        // Left panel - Skill Tree/Queue
        CreateSkillSection();
        
        // Right panel - Attributes/Optimization
        CreateAnalysisSection();

        Content = _mainGrid;
    }

    private void CreateHeaderSection()
    {
        var headerBorder = new Border
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

        var headerGrid = new Grid();
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Title
        var titleText = new TextBlock
        {
            Text = "Character Planning",
            Foreground = Brushes.White,
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(titleText, 0);

        // Character info
        var characterInfo = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var characterName = new TextBlock
        {
            Text = CurrentCharacter?.Name ?? "No Character Selected",
            Foreground = Brushes.White,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(10, 0)
        };

        _trainingTimeText = new TextBlock
        {
            Text = "Total Training Time: 0d 0h 0m",
            Foreground = new SolidColorBrush(Color.FromRgb(138, 43, 226)),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(10, 0)
        };

        characterInfo.Children.Add(characterName);
        characterInfo.Children.Add(_trainingTimeText);
        Grid.SetColumn(characterInfo, 1);

        // Planning mode selector
        _planningModeSelector = new ComboBox
        {
            Width = 120,
            Height = 30,
            Margin = new Thickness(5, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        PopulatePlanningModeSelector();
        Grid.SetColumn(_planningModeSelector, 2);

        // Action buttons
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(5, 0)
        };

        _optimizeButton = new Button
        {
            Content = "Optimize",
            Width = 80,
            Height = 30,
            Margin = new Thickness(2),
            Style = CreateHoloButtonStyle()
        };
        _optimizeButton.Click += OnOptimizeClick;

        _clearQueueButton = new Button
        {
            Content = "Clear",
            Width = 60,
            Height = 30,
            Margin = new Thickness(2),
            Style = CreateHoloButtonStyle()
        };
        _clearQueueButton.Click += (s, e) => ClearSkillQueue();

        buttonPanel.Children.Add(_optimizeButton);
        buttonPanel.Children.Add(_clearQueueButton);
        Grid.SetColumn(buttonPanel, 3);

        headerGrid.Children.Add(titleText);
        headerGrid.Children.Add(characterInfo);
        headerGrid.Children.Add(_planningModeSelector);
        headerGrid.Children.Add(buttonPanel);

        headerBorder.Child = headerGrid;
        Grid.SetColumnSpan(headerBorder, 2);
        Grid.SetRow(headerBorder, 0);
        _mainGrid.Children.Add(headerBorder);
    }

    private void CreateSkillSection()
    {
        var skillBorder = new Border
        {
            CornerRadius = new CornerRadius(12),
            BorderThickness = new Thickness(2),
            Margin = new Thickness(10, 5, 5, 10),
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

        var skillTabControl = new TabControl
        {
            Margin = new Thickness(10),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0)
        };

        // Skill Queue Tab
        var queueTab = new TabItem
        {
            Header = "Skill Queue",
            Foreground = Brushes.White
        };

        _queueGrid = new Grid();
        _queueGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _queueGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        _trainingProgress = new ProgressBar
        {
            Height = 6,
            Margin = new Thickness(10, 5),
            Background = new SolidColorBrush(Color.FromArgb(60, 0, 0, 0)),
            Foreground = new SolidColorBrush(Color.FromRgb(138, 43, 226))
        };
        Grid.SetRow(_trainingProgress, 0);

        _queueList = new ItemsControl
        {
            Margin = new Thickness(10),
            ItemTemplate = CreateQueueItemTemplate()
        };

        var queueScrollViewer = new ScrollViewer
        {
            Content = _queueList,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        Grid.SetRow(queueScrollViewer, 1);

        _queueGrid.Children.Add(_trainingProgress);
        _queueGrid.Children.Add(queueScrollViewer);
        queueTab.Content = _queueGrid;

        // Skill Tree Tab
        var treeTab = new TabItem
        {
            Header = "Skill Tree",
            Foreground = Brushes.White
        };

        _skillTreeGrid = new Grid();
        CreateSkillTreeView();
        treeTab.Content = _skillTreeGrid;

        skillTabControl.Items.Add(queueTab);
        skillTabControl.Items.Add(treeTab);

        skillBorder.Child = skillTabControl;
        Grid.SetColumn(skillBorder, 0);
        Grid.SetRow(skillBorder, 1);
        _mainGrid.Children.Add(skillBorder);
    }

    private void CreateAnalysisSection()
    {
        var analysisBorder = new Border
        {
            CornerRadius = new CornerRadius(12),
            BorderThickness = new Thickness(2),
            Margin = new Thickness(5, 5, 10, 10),
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(40, 20, 0, 40), 0.0),
                    new GradientStop(Color.FromArgb(20, 15, 0, 30), 1.0)
                }
            }
        };

        var analysisTabControl = new TabControl
        {
            Margin = new Thickness(10),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0)
        };

        // Attributes Tab
        var attributesTab = new TabItem
        {
            Header = "Attributes",
            Foreground = Brushes.White
        };

        _attributesGrid = new Grid();
        CreateAttributesPanel();
        attributesTab.Content = _attributesGrid;

        // Optimization Tab
        var optimizationTab = new TabItem
        {
            Header = "Optimization",
            Foreground = Brushes.White
        };

        _optimizationGrid = new Grid();
        CreateOptimizationPanel();
        optimizationTab.Content = _optimizationGrid;

        analysisTabControl.Items.Add(attributesTab);
        analysisTabControl.Items.Add(optimizationTab);

        analysisBorder.Child = analysisTabControl;
        Grid.SetColumn(analysisBorder, 1);
        Grid.SetRow(analysisBorder, 1);
        _mainGrid.Children.Add(analysisBorder);
    }

    private void CreateSkillTreeView()
    {
        _skillTreeCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            ClipToBounds = true
        };

        var treeScrollViewer = new ScrollViewer
        {
            Content = _skillTreeCanvas,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            ZoomMode = ZoomMode.Enabled
        };

        _skillTreeGrid.Children.Add(treeScrollViewer);
        
        // Create effect canvas overlay
        _effectCanvas = new Canvas
        {
            IsHitTestVisible = false,
            ClipToBounds = false
        };
        _skillTreeGrid.Children.Add(_effectCanvas);

        PopulateSkillTree();
    }

    private void CreateAttributesPanel()
    {
        _attributesPanel = new StackPanel
        {
            Margin = new Thickness(15)
        };

        // Create attribute displays
        foreach (var attribute in Enum.GetValues<SkillAttribute>())
        {
            var attributeControl = CreateAttributeDisplay(attribute);
            _attributesPanel.Children.Add(attributeControl);
        }

        var attributesScrollViewer = new ScrollViewer
        {
            Content = _attributesPanel,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        _attributesGrid.Children.Add(attributesScrollViewer);
    }

    private void CreateOptimizationPanel()
    {
        _optimizationPanel = new StackPanel
        {
            Margin = new Thickness(15)
        };

        // Optimization recommendations
        var recommendationHeader = new TextBlock
        {
            Text = "Optimization Recommendations",
            Foreground = Brushes.White,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 10)
        };

        var recommendationList = new ItemsControl
        {
            ItemTemplate = CreateRecommendationTemplate()
        };

        _optimizationPanel.Children.Add(recommendationHeader);
        _optimizationPanel.Children.Add(recommendationList);

        var optimizationScrollViewer = new ScrollViewer
        {
            Content = _optimizationPanel,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        _optimizationGrid.Children.Add(optimizationScrollViewer);
    }

    private void PopulatePlanningModeSelector()
    {
        _planningModeSelector.Items.Add(new ComboBoxItem { Content = "Skill Queue", Tag = CharacterPlanningMode.SkillQueue });
        _planningModeSelector.Items.Add(new ComboBoxItem { Content = "Ship Fitting", Tag = CharacterPlanningMode.ShipFocus });
        _planningModeSelector.Items.Add(new ComboBoxItem { Content = "Industry", Tag = CharacterPlanningMode.Industry });
        _planningModeSelector.Items.Add(new ComboBoxItem { Content = "PvP Combat", Tag = CharacterPlanningMode.Combat });
        _planningModeSelector.SelectedIndex = 0;
    }

    private void PopulateSkillTree()
    {
        if (_skillTreeCanvas == null) return;

        _skillTreeCanvas.Children.Clear();
        _skillNodes.Clear();
        _skillConnections.Clear();

        // Create sample skill tree structure
        CreateSkillTreeStructure();
        DrawSkillConnections();
    }

    private void CreateSkillTreeStructure()
    {
        var categories = new[]
        {
            ("Spaceship Command", 100, 100),
            ("Engineering", 300, 100),
            ("Gunnery", 500, 100),
            ("Missiles", 100, 300),
            ("Navigation", 300, 300),
            ("Electronic Systems", 500, 300)
        };

        foreach (var (category, x, y) in categories)
        {
            var node = CreateSkillNode(category, x, y);
            _skillNodes[category] = node;
            _skillTreeCanvas.Children.Add(node);
        }
    }

    private HoloSkillNode CreateSkillNode(string skillName, double x, double y)
    {
        var node = new HoloSkillNode
        {
            SkillName = skillName,
            CurrentLevel = _random.Next(0, 6),
            MaxLevel = 5,
            Width = 80,
            Height = 60,
            HolographicIntensity = HolographicIntensity,
            EVEColorScheme = EVEColorScheme
        };

        Canvas.SetLeft(node, x);
        Canvas.SetTop(node, y);

        node.NodeClicked += OnSkillNodeClicked;
        return node;
    }

    private void DrawSkillConnections()
    {
        // Sample connections between skills
        var connections = new[]
        {
            ("Spaceship Command", "Engineering"),
            ("Engineering", "Electronic Systems"),
            ("Spaceship Command", "Gunnery"),
            ("Spaceship Command", "Navigation")
        };

        foreach (var (from, to) in connections)
        {
            if (_skillNodes.TryGetValue(from, out var fromNode) && 
                _skillNodes.TryGetValue(to, out var toNode))
            {
                CreateSkillConnection(fromNode, toNode);
            }
        }
    }

    private void CreateSkillConnection(HoloSkillNode fromNode, HoloSkillNode toNode)
    {
        var fromCenter = new Point(
            Canvas.GetLeft(fromNode) + fromNode.Width / 2,
            Canvas.GetTop(fromNode) + fromNode.Height / 2);
        
        var toCenter = new Point(
            Canvas.GetLeft(toNode) + toNode.Width / 2,
            Canvas.GetTop(toNode) + toNode.Height / 2);

        var line = new Line
        {
            X1 = fromCenter.X,
            Y1 = fromCenter.Y,
            X2 = toCenter.X,
            Y2 = toCenter.Y,
            Stroke = new SolidColorBrush(Color.FromArgb(120, 138, 43, 226)),
            StrokeThickness = 2
        };

        if (!IsInSimplifiedMode)
        {
            line.Effect = new DropShadowEffect
            {
                Color = Color.FromRgb(138, 43, 226),
                BlurRadius = 4 * HolographicIntensity,
                ShadowDepth = 0,
                Opacity = 0.6 * HolographicIntensity
            };
        }

        var connection = new SkillConnection
        {
            Visual = line,
            FromNode = fromNode,
            ToNode = toNode,
            Phase = _random.NextDouble() * Math.PI * 2
        };

        _skillConnections.Add(connection);
        _skillTreeCanvas.Children.Insert(0, line); // Add behind nodes
    }

    private DataTemplate CreateQueueItemTemplate()
    {
        var template = new DataTemplate();
        
        var border = new FrameworkElementFactory(typeof(Border));
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
        border.SetValue(Border.MarginProperty, new Thickness(2));
        border.SetValue(Border.PaddingProperty, new Thickness(10, 6));
        border.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromArgb(60, 138, 43, 226)));
        
        var grid = new FrameworkElementFactory(typeof(Grid));
        grid.SetValue(Grid.ColumnDefinitionsProperty, new ColumnDefinitionCollection
        {
            new ColumnDefinition { Width = GridLength.Auto },
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            new ColumnDefinition { Width = GridLength.Auto },
            new ColumnDefinition { Width = GridLength.Auto }
        });
        
        var positionText = new FrameworkElementFactory(typeof(TextBlock));
        positionText.SetBinding(TextBlock.TextProperty, new Binding("Position") { StringFormat = "{0}." });
        positionText.SetValue(TextBlock.ForegroundProperty, Brushes.LightGray);
        positionText.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        positionText.SetValue(TextBlock.MarginProperty, new Thickness(0, 0, 8, 0));
        positionText.SetValue(Grid.ColumnProperty, 0);
        
        var skillInfo = new FrameworkElementFactory(typeof(StackPanel));
        skillInfo.SetValue(Grid.ColumnProperty, 1);
        
        var skillName = new FrameworkElementFactory(typeof(TextBlock));
        skillName.SetBinding(TextBlock.TextProperty, new Binding("Skill.Name"));
        skillName.SetValue(TextBlock.ForegroundProperty, Brushes.White);
        skillName.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        
        var levelInfo = new FrameworkElementFactory(typeof(TextBlock));
        levelInfo.SetBinding(TextBlock.TextProperty, new Binding { StringFormat = "Level {0} → {1}" });
        levelInfo.SetValue(TextBlock.ForegroundProperty, Brushes.LightGray);
        levelInfo.SetValue(TextBlock.FontSizeProperty, 10.0);
        
        skillInfo.AppendChild(skillName);
        skillInfo.AppendChild(levelInfo);
        
        var timeText = new FrameworkElementFactory(typeof(TextBlock));
        timeText.SetBinding(TextBlock.TextProperty, new Binding("TrainingTime") { StringFormat = "{0:d\\d\\ h\\h\\ m\\m}" });
        timeText.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Color.FromRgb(138, 43, 226)));
        timeText.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        timeText.SetValue(TextBlock.MarginProperty, new Thickness(8, 0));
        timeText.SetValue(Grid.ColumnProperty, 2);
        
        var removeButton = new FrameworkElementFactory(typeof(Button));
        removeButton.SetValue(Button.ContentProperty, "×");
        removeButton.SetValue(Button.WidthProperty, 20.0);
        removeButton.SetValue(Button.HeightProperty, 20.0);
        removeButton.SetValue(Button.MarginProperty, new Thickness(4, 0, 0, 0));
        removeButton.SetValue(Grid.ColumnProperty, 3);
        
        grid.AppendChild(positionText);
        grid.AppendChild(skillInfo);
        grid.AppendChild(timeText);
        grid.AppendChild(removeButton);
        border.AppendChild(grid);
        
        template.VisualTree = border;
        return template;
    }

    private DataTemplate CreateRecommendationTemplate()
    {
        var template = new DataTemplate();
        
        var border = new FrameworkElementFactory(typeof(Border));
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
        border.SetValue(Border.MarginProperty, new Thickness(0, 2));
        border.SetValue(Border.PaddingProperty, new Thickness(8, 4));
        border.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)));
        
        var textBlock = new FrameworkElementFactory(typeof(TextBlock));
        textBlock.SetBinding(TextBlock.TextProperty, new Binding());
        textBlock.SetValue(TextBlock.ForegroundProperty, Brushes.White);
        textBlock.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
        
        border.AppendChild(textBlock);
        template.VisualTree = border;
        return template;
    }

    private UIElement CreateAttributeDisplay(SkillAttribute attribute)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 138, 43, 226)),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(10, 6),
            Margin = new Thickness(0, 2)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var nameText = new TextBlock
        {
            Text = attribute.ToString(),
            Foreground = Brushes.White,
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(nameText, 0);

        var valueText = new TextBlock
        {
            Text = CurrentCharacter?.GetAttributeValue(attribute).ToString() ?? "0",
            Foreground = new SolidColorBrush(Color.FromRgb(138, 43, 226)),
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(valueText, 1);

        grid.Children.Add(nameText);
        grid.Children.Add(valueText);
        border.Child = grid;

        return border;
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
        // Main animation timer
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
        };
        _animationTimer.Tick += OnAnimationTick;

        // Particle animation timer
        _particleTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50) // 20 FPS
        };
        _particleTimer.Tick += OnParticleTick;
    }

    private void OnAnimationTick(object sender, EventArgs e)
    {
        if (!EnableSkillAnimations || IsInSimplifiedMode) return;

        _animationPhase += 0.05;
        if (_animationPhase > Math.PI * 2)
            _animationPhase = 0;

        UpdateSkillTreeAnimations();
        UpdateConnectionAnimations();
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        if (!EnableParticleEffects || IsInSimplifiedMode || _effectCanvas == null) return;

        _particlePhase += 0.1;
        if (_particlePhase > Math.PI * 2)
            _particlePhase = 0;

        UpdateParticleEffects();
    }

    private void UpdateSkillTreeAnimations()
    {
        foreach (var node in _skillNodes.Values)
        {
            var intensity = 0.8 + (Math.Sin(_animationPhase + node.GetHashCode()) * 0.2);
            node.HolographicIntensity = HolographicIntensity * intensity;
        }
    }

    private void UpdateConnectionAnimations()
    {
        foreach (var connection in _skillConnections)
        {
            if (connection.Visual.Effect is DropShadowEffect effect)
            {
                var intensity = 0.6 + (Math.Sin(_animationPhase + connection.Phase) * 0.3);
                effect.Opacity = HolographicIntensity * intensity;
            }
        }
    }

    private void UpdateParticleEffects()
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var particle = _particles[i];
            
            particle.X += particle.VelocityX;
            particle.Y += particle.VelocityY;
            particle.Life -= 0.02;

            Canvas.SetLeft(particle.Visual, particle.X);
            Canvas.SetTop(particle.Visual, particle.Y);
            particle.Visual.Opacity = Math.Max(0, particle.Life) * HolographicIntensity;

            if (particle.Life <= 0)
            {
                _effectCanvas.Children.Remove(particle.Visual);
                _particles.RemoveAt(i);
            }
        }
    }

    private void AnimateSkillAddition(HoloQueuedSkill queuedSkill)
    {
        // Create addition animation particles
        for (int i = 0; i < 6; i++)
        {
            SpawnAdditionParticle();
        }
    }

    private void SpawnOptimizationParticles()
    {
        for (int i = 0; i < 12; i++)
        {
            var particle = CreatePlanningParticle();
            _particles.Add(particle);
            _effectCanvas.Children.Add(particle.Visual);
        }
    }

    private void SpawnAdditionParticle()
    {
        if (_effectCanvas == null) return;

        var particle = CreatePlanningParticle();
        _particles.Add(particle);
        _effectCanvas.Children.Add(particle.Visual);
    }

    private PlanningParticle CreatePlanningParticle()
    {
        var color = GetEVEColor(EVEColorScheme);
        var size = 2 + _random.NextDouble() * 2;
        
        var ellipse = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromArgb(150, color.R, color.G, color.B))
        };

        var particle = new PlanningParticle
        {
            Visual = ellipse,
            X = _random.NextDouble() * ActualWidth,
            Y = ActualHeight,
            VelocityX = (_random.NextDouble() - 0.5) * 3,
            VelocityY = -1 - _random.NextDouble() * 4,
            Life = 1.0
        };

        Canvas.SetLeft(ellipse, particle.X);
        Canvas.SetTop(ellipse, particle.Y);

        return particle;
    }

    private void ReorderQueue()
    {
        for (int i = 0; i < SkillQueue.Count; i++)
        {
            SkillQueue[i].Position = i;
        }
    }

    private void UpdateTrainingTime()
    {
        var totalTime = SkillQueue.Sum(s => s.TrainingTime.TotalMinutes);
        var timeSpan = TimeSpan.FromMinutes(totalTime);
        
        if (_trainingTimeText != null)
        {
            _trainingTimeText.Text = $"Total Training Time: {timeSpan.Days}d {timeSpan.Hours}h {timeSpan.Minutes}m";
        }

        if (_trainingProgress != null)
        {
            _trainingProgress.Maximum = totalTime;
            _trainingProgress.Value = totalTime * 0.1; // Simulate current progress
        }
    }

    private TimeSpan CalculateTrainingTime(HoloSkillInfo skill, int targetLevel)
    {
        // Simplified training time calculation
        var baseMinutes = (targetLevel - skill.CurrentLevel) * 30 * skill.TrainingMultiplier;
        return TimeSpan.FromMinutes(baseMinutes);
    }

    private void UpdatePlanningAppearance()
    {
        if (_queueList != null && SkillQueue != null)
        {
            _queueList.ItemsSource = SkillQueue;
        }

        UpdateColors();
        UpdateEffects();
    }

    private void UpdateColors()
    {
        var color = GetEVEColor(EVEColorScheme);

        // Update border colors
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

    private void OnSkillNodeClicked(object sender, HoloSkillNodeEventArgs e)
    {
        SelectedSkill = new HoloSkillInfo { Name = e.SkillName };
    }

    private async void OnOptimizeClick(object sender, RoutedEventArgs e)
    {
        await OptimizeTrainingPlanAsync();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableSkillAnimations && !IsInSimplifiedMode)
            _animationTimer.Start();
            
        if (EnableParticleEffects && !IsInSimplifiedMode)
            _particleTimer.Start();

        UpdateTrainingTime();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        _particleTimer?.Stop();
        
        // Clean up particles and connections
        _particles.Clear();
        _skillConnections.Clear();
        _effectCanvas?.Children.Clear();
        
        // Unregister from animation intensity manager
        AnimationIntensityManager.Instance.UnregisterTarget(this);
        
        _disposed = true;
    }

    #endregion

    #region Event Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterPlanning planning)
            planning.UpdatePlanningAppearance();
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterPlanning planning)
            planning.UpdatePlanningAppearance();
    }

    private static void OnCurrentCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterPlanning planning)
        {
            planning.UpdatePlanningAppearance();
        }
    }

    private static void OnSelectedSkillChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Handle skill selection changes
    }

    private static void OnPlanningModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Handle planning mode changes
    }

    private static void OnEnableSkillAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterPlanning planning)
        {
            if ((bool)e.NewValue && !planning.IsInSimplifiedMode)
                planning._animationTimer.Start();
            else
                planning._animationTimer.Stop();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterPlanning planning)
        {
            if ((bool)e.NewValue && !planning.IsInSimplifiedMode)
                planning._particleTimer.Start();
            else
                planning._particleTimer.Stop();
        }
    }

    #endregion
}

/// <summary>
/// Holographic skill node for skill tree visualization
/// </summary>
public class HoloSkillNode : Control, IAnimationIntensityTarget
{
    public string SkillName { get; set; }
    public int CurrentLevel { get; set; }
    public int MaxLevel { get; set; } = 5;
    public double HolographicIntensity { get; set; } = 1.0;
    public EVEColorScheme EVEColorScheme { get; set; } = EVEColorScheme.VoidPurple;
    public bool IsValid => IsLoaded;

    public event EventHandler<HoloSkillNodeEventArgs> NodeClicked;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings != null)
        {
            HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        NodeClicked?.Invoke(this, new HoloSkillNodeEventArgs { SkillName = SkillName });
    }
}

/// <summary>
/// Planning particle for visual effects
/// </summary>
internal class PlanningParticle
{
    public FrameworkElement Visual { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
}

/// <summary>
/// Skill connection in skill tree
/// </summary>
internal class SkillConnection
{
    public Line Visual { get; set; }
    public HoloSkillNode FromNode { get; set; }
    public HoloSkillNode ToNode { get; set; }
    public double Phase { get; set; }
}

/// <summary>
/// Character information for planning
/// </summary>
public class HoloCharacterInfo
{
    public string Name { get; set; }
    public Dictionary<SkillAttribute, int> Attributes { get; set; } = new();

    public int GetAttributeValue(SkillAttribute attribute)
    {
        return Attributes.TryGetValue(attribute, out var value) ? value : 20;
    }
}

/// <summary>
/// Skill information for planning
/// </summary>
public class HoloSkillInfo
{
    public string Name { get; set; }
    public string Category { get; set; }
    public int CurrentLevel { get; set; }
    public int MaxLevel { get; set; } = 5;
    public SkillAttribute PrimaryAttribute { get; set; }
    public SkillAttribute SecondaryAttribute { get; set; }
    public double TrainingMultiplier { get; set; } = 1.0;
    public List<string> Prerequisites { get; set; } = new();
}

/// <summary>
/// Queued skill for training
/// </summary>
public class HoloQueuedSkill
{
    public HoloSkillInfo Skill { get; set; }
    public int TargetLevel { get; set; }
    public int CurrentLevel { get; set; }
    public int Position { get; set; }
    public TimeSpan TrainingTime { get; set; }
}

/// <summary>
/// Attribute mapping recommendation
/// </summary>
public class HoloAttributeMapping
{
    public SkillAttribute PrimaryAttribute { get; set; }
    public SkillAttribute SecondaryAttribute { get; set; }
    public TimeSpan EstimatedTimeSaving { get; set; }
}

/// <summary>
/// Skill attributes
/// </summary>
public enum SkillAttribute
{
    Intelligence,
    Memory,
    Perception,
    Willpower,
    Charisma
}

/// <summary>
/// Character planning modes
/// </summary>
public enum CharacterPlanningMode
{
    SkillQueue,
    ShipFocus,
    Industry,
    Combat,
    Exploration
}

/// <summary>
/// Event args for character planning events
/// </summary>
public class HoloCharacterPlanningEventArgs : EventArgs
{
    public HoloSkillInfo Skill { get; set; }
    public int TargetLevel { get; set; }
    public int QueuePosition { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Event args for skill node events
/// </summary>
public class HoloSkillNodeEventArgs : EventArgs
{
    public string SkillName { get; set; }
    public DateTime Timestamp { get; set; }
}