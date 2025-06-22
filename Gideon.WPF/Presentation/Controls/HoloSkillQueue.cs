// ==========================================================================
// HoloSkillQueue.cs - Holographic Skill Queue Visualization
// ==========================================================================
// Advanced skill queue system featuring real-time training progress,
// queue management, EVE-style training visualization, and holographic skill queue display.
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
/// Holographic skill queue with real-time training progress and queue management
/// </summary>
public class HoloSkillQueue : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloSkillQueue),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloSkillQueue),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty SkillQueueProperty =
        DependencyProperty.Register(nameof(SkillQueue), typeof(ObservableCollection<HoloQueuedSkill>), typeof(HoloSkillQueue),
            new PropertyMetadata(null, OnSkillQueueChanged));

    public static readonly DependencyProperty CurrentlyTrainingProperty =
        DependencyProperty.Register(nameof(CurrentlyTraining), typeof(HoloQueuedSkill), typeof(HoloSkillQueue),
            new PropertyMetadata(null, OnCurrentlyTrainingChanged));

    public static readonly DependencyProperty SelectedCharacterProperty =
        DependencyProperty.Register(nameof(SelectedCharacter), typeof(string), typeof(HoloSkillQueue),
            new PropertyMetadata("Main Character", OnSelectedCharacterChanged));

    public static readonly DependencyProperty QueueModeProperty =
        DependencyProperty.Register(nameof(QueueMode), typeof(SkillQueueMode), typeof(HoloSkillQueue),
            new PropertyMetadata(SkillQueueMode.Active, OnQueueModeChanged));

    public static readonly DependencyProperty ShowEstimatedTimesProperty =
        DependencyProperty.Register(nameof(ShowEstimatedTimes), typeof(bool), typeof(HoloSkillQueue),
            new PropertyMetadata(true, OnShowEstimatedTimesChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloSkillQueue),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloSkillQueue),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowSkillDetailsProperty =
        DependencyProperty.Register(nameof(ShowSkillDetails), typeof(bool), typeof(HoloSkillQueue),
            new PropertyMetadata(true, OnShowSkillDetailsChanged));

    public static readonly DependencyProperty ShowTrainingStatisticsProperty =
        DependencyProperty.Register(nameof(ShowTrainingStatistics), typeof(bool), typeof(HoloSkillQueue),
            new PropertyMetadata(true, OnShowTrainingStatisticsChanged));

    public static readonly DependencyProperty EnableDragDropProperty =
        DependencyProperty.Register(nameof(EnableDragDrop), typeof(bool), typeof(HoloSkillQueue),
            new PropertyMetadata(true, OnEnableDragDropChanged));

    public static readonly DependencyProperty AutoScrollProperty =
        DependencyProperty.Register(nameof(AutoScroll), typeof(bool), typeof(HoloSkillQueue),
            new PropertyMetadata(true, OnAutoScrollChanged));

    public static readonly DependencyProperty ShowQueueManagementProperty =
        DependencyProperty.Register(nameof(ShowQueueManagement), typeof(bool), typeof(HoloSkillQueue),
            new PropertyMetadata(true, OnShowQueueManagementChanged));

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

    public ObservableCollection<HoloQueuedSkill> SkillQueue
    {
        get => (ObservableCollection<HoloQueuedSkill>)GetValue(SkillQueueProperty);
        set => SetValue(SkillQueueProperty, value);
    }

    public HoloQueuedSkill CurrentlyTraining
    {
        get => (HoloQueuedSkill)GetValue(CurrentlyTrainingProperty);
        set => SetValue(CurrentlyTrainingProperty, value);
    }

    public string SelectedCharacter
    {
        get => (string)GetValue(SelectedCharacterProperty);
        set => SetValue(SelectedCharacterProperty, value);
    }

    public SkillQueueMode QueueMode
    {
        get => (SkillQueueMode)GetValue(QueueModeProperty);
        set => SetValue(QueueModeProperty, value);
    }

    public bool ShowEstimatedTimes
    {
        get => (bool)GetValue(ShowEstimatedTimesProperty);
        set => SetValue(ShowEstimatedTimesProperty, value);
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

    public bool ShowSkillDetails
    {
        get => (bool)GetValue(ShowSkillDetailsProperty);
        set => SetValue(ShowSkillDetailsProperty, value);
    }

    public bool ShowTrainingStatistics
    {
        get => (bool)GetValue(ShowTrainingStatisticsProperty);
        set => SetValue(ShowTrainingStatisticsProperty, value);
    }

    public bool EnableDragDrop
    {
        get => (bool)GetValue(EnableDragDropProperty);
        set => SetValue(EnableDragDropProperty, value);
    }

    public bool AutoScroll
    {
        get => (bool)GetValue(AutoScrollProperty);
        set => SetValue(AutoScrollProperty, value);
    }

    public bool ShowQueueManagement
    {
        get => (bool)GetValue(ShowQueueManagementProperty);
        set => SetValue(ShowQueueManagementProperty, value);
    }

    #endregion

    #region Fields

    private Canvas _mainCanvas;
    private Canvas _particleCanvas;
    private ScrollViewer _queueScrollViewer;
    private StackPanel _queueItemsPanel;
    private Border _currentTrainingSection;
    private Border _statisticsSection;
    private Border _queueManagementSection;
    
    private readonly List<HoloQueueParticle> _particles = new();
    private readonly DispatcherTimer _animationTimer;
    private readonly DispatcherTimer _progressUpdateTimer;
    private readonly Random _random = new();
    
    private bool _isAnimating = true;
    private bool _isDisposed = false;
    private double _currentAnimationTime = 0;

    #endregion

    #region Constructor

    public HoloSkillQueue()
    {
        InitializeComponent();
        
        _animationTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16) // 60 FPS
        };
        _animationTimer.Tick += OnAnimationTick;
        
        _progressUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1) // Update progress every second
        };
        _progressUpdateTimer.Tick += OnProgressUpdateTick;
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
    }

    #endregion

    #region Initialization

    private void InitializeComponent()
    {
        Width = 400;
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
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(200, GridUnitType.Pixel) });
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(120, GridUnitType.Pixel) });
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80, GridUnitType.Pixel) });
        
        // Current training section
        _currentTrainingSection = CreateCurrentTrainingSection();
        Grid.SetRow(_currentTrainingSection, 0);
        contentGrid.Children.Add(_currentTrainingSection);
        
        // Queue scroll viewer
        _queueScrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Margin = new Thickness(10)
        };
        
        _queueItemsPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };
        
        _queueScrollViewer.Content = _queueItemsPanel;
        Grid.SetRow(_queueScrollViewer, 1);
        contentGrid.Children.Add(_queueScrollViewer);
        
        // Statistics section
        _statisticsSection = CreateStatisticsSection();
        Grid.SetRow(_statisticsSection, 2);
        contentGrid.Children.Add(_statisticsSection);
        
        // Queue management section
        _queueManagementSection = CreateQueueManagementSection();
        Grid.SetRow(_queueManagementSection, 3);
        contentGrid.Children.Add(_queueManagementSection);
        
        _mainCanvas.Children.Add(contentGrid);
        _mainCanvas.Children.Add(_particleCanvas);
        border.Child = _mainCanvas;
        Content = border;

        CreateSampleData();
    }

    private Border CreateCurrentTrainingSection()
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
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Header
        var header = new TextBlock
        {
            Text = "CURRENTLY TRAINING",
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(10, 8, 10, 5)
        };
        Grid.SetRow(header, 0);
        grid.Children.Add(header);

        // Skill info
        var skillInfoPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(15, 10, 15, 10)
        };

        var skillNameText = new TextBlock
        {
            Text = "Gunnery V",
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        skillInfoPanel.Children.Add(skillNameText);

        var skillLevelText = new TextBlock
        {
            Text = "Level IV → Level V",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 200, 200)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0)
        };
        skillInfoPanel.Children.Add(skillLevelText);

        Grid.SetRow(skillInfoPanel, 1);
        grid.Children.Add(skillInfoPanel);

        // Progress bar
        var progressBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(60, 50, 50, 50)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 100, 100, 100)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3),
            Height = 20,
            Margin = new Thickness(15, 5, 15, 10)
        };

        var progressBar = new Rectangle
        {
            Fill = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(255, 0, 255, 255), 0),
                    new GradientStop(Color.FromArgb(200, 0, 200, 255), 0.5),
                    new GradientStop(Color.FromArgb(255, 0, 255, 255), 1)
                }
            },
            Width = 150, // 65% progress
            Height = 18,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            RadiusX = 2,
            RadiusY = 2
        };

        progressBorder.Child = progressBar;
        Grid.SetRow(progressBorder, 2);
        grid.Children.Add(progressBorder);

        // Time remaining
        var timeText = new TextBlock
        {
            Text = "8 hours, 24 minutes remaining",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 255, 255, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(10, 5, 10, 10)
        };
        Grid.SetRow(timeText, 3);
        grid.Children.Add(timeText);

        border.Child = grid;
        return border;
    }

    private Border CreateStatisticsSection()
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

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());

        // Queue statistics
        var leftPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(15, 10, 10, 10)
        };

        var queueHeader = new TextBlock
        {
            Text = "QUEUE STATS",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            Margin = new Thickness(0, 0, 0, 5)
        };
        leftPanel.Children.Add(queueHeader);

        var queueCountText = new TextBlock
        {
            Text = "Skills in Queue: 12",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            Margin = new Thickness(0, 2, 0, 0)
        };
        leftPanel.Children.Add(queueCountText);

        var totalTimeText = new TextBlock
        {
            Text = "Total Time: 45d 12h",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            Margin = new Thickness(0, 2, 0, 0)
        };
        leftPanel.Children.Add(totalTimeText);

        Grid.SetColumn(leftPanel, 0);
        grid.Children.Add(leftPanel);

        // Training statistics
        var rightPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10, 10, 15, 10)
        };

        var trainingHeader = new TextBlock
        {
            Text = "TRAINING",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            Margin = new Thickness(0, 0, 0, 5)
        };
        rightPanel.Children.Add(trainingHeader);

        var spPerHourText = new TextBlock
        {
            Text = "SP/Hour: 2,547",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            Margin = new Thickness(0, 2, 0, 0)
        };
        rightPanel.Children.Add(spPerHourText);

        var efficiencyText = new TextBlock
        {
            Text = "Efficiency: 97.3%",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 0, 255, 100)),
            Margin = new Thickness(0, 2, 0, 0)
        };
        rightPanel.Children.Add(efficiencyText);

        Grid.SetColumn(rightPanel, 1);
        grid.Children.Add(rightPanel);

        border.Child = grid;
        return border;
    }

    private Border CreateQueueManagementSection()
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
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(10)
        };

        var pauseButton = CreateManagementButton("PAUSE", Color.FromArgb(255, 255, 100, 0));
        var clearButton = CreateManagementButton("CLEAR", Color.FromArgb(255, 255, 0, 0));
        var optimizeButton = CreateManagementButton("OPTIMIZE", Color.FromArgb(255, 0, 255, 100));

        panel.Children.Add(pauseButton);
        panel.Children.Add(clearButton);
        panel.Children.Add(optimizeButton);

        border.Child = panel;
        return border;
    }

    private Button CreateManagementButton(string text, Color accentColor)
    {
        return new Button
        {
            Content = text,
            Width = 80,
            Height = 30,
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            Background = new SolidColorBrush(Color.FromArgb(100, accentColor.R, accentColor.G, accentColor.B)),
            BorderBrush = new SolidColorBrush(accentColor),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(5, 0, 5, 0),
            Effect = new DropShadowEffect
            {
                Color = accentColor,
                ShadowDepth = 0,
                BlurRadius = 5
            }
        };
    }

    private void CreateSampleData()
    {
        SkillQueue = new ObservableCollection<HoloQueuedSkill>
        {
            new HoloQueuedSkill
            {
                SkillName = "Gunnery",
                CurrentLevel = 4,
                TargetLevel = 5,
                TrainingProgress = 0.65,
                EstimatedTime = TimeSpan.FromHours(8.4),
                SkillPoints = 512000,
                RequiredSkillPoints = 1280000,
                Priority = SkillPriority.High
            },
            new HoloQueuedSkill
            {
                SkillName = "Large Projectile Turret",
                CurrentLevel = 3,
                TargetLevel = 4,
                TrainingProgress = 0.0,
                EstimatedTime = TimeSpan.FromHours(15.2),
                SkillPoints = 0,
                RequiredSkillPoints = 226275,
                Priority = SkillPriority.Medium
            },
            new HoloQueuedSkill
            {
                SkillName = "Weapon Upgrades",
                CurrentLevel = 4,
                TargetLevel = 5,
                TrainingProgress = 0.0,
                EstimatedTime = TimeSpan.FromHours(22.8),
                SkillPoints = 0,
                RequiredSkillPoints = 1280000,
                Priority = SkillPriority.High
            }
        };

        UpdateQueueDisplay();
    }

    #endregion

    #region Queue Display

    private void UpdateQueueDisplay()
    {
        if (_queueItemsPanel == null || SkillQueue == null) return;

        _queueItemsPanel.Children.Clear();

        for (int i = 0; i < SkillQueue.Count; i++)
        {
            var skill = SkillQueue[i];
            var queueItem = CreateQueueItem(skill, i + 1);
            _queueItemsPanel.Children.Add(queueItem);
        }
    }

    private Border CreateQueueItem(HoloQueuedSkill skill, int position)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(30, 0, 100, 150)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(60, 0, 200, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3),
            Margin = new Thickness(5),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(40, 0, 200, 255),
                ShadowDepth = 0,
                BlurRadius = 5
            }
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30, GridUnitType.Pixel) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80, GridUnitType.Pixel) });

        // Position number
        var positionText = new TextBlock
        {
            Text = position.ToString(),
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5)
        };
        Grid.SetColumn(positionText, 0);
        grid.Children.Add(positionText);

        // Skill information
        var skillPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10, 8, 10, 8)
        };

        var skillNameText = new TextBlock
        {
            Text = skill.SkillName,
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White)
        };
        skillPanel.Children.Add(skillNameText);

        var levelText = new TextBlock
        {
            Text = $"Level {skill.CurrentLevel} → {skill.TargetLevel}",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 200, 200, 200)),
            Margin = new Thickness(0, 2, 0, 0)
        };
        skillPanel.Children.Add(levelText);

        var spText = new TextBlock
        {
            Text = $"{skill.SkillPoints:N0} / {skill.RequiredSkillPoints:N0} SP",
            FontSize = 9,
            Foreground = new SolidColorBrush(Color.FromArgb(160, 150, 150, 150)),
            Margin = new Thickness(0, 2, 0, 0)
        };
        skillPanel.Children.Add(spText);

        Grid.SetColumn(skillPanel, 1);
        grid.Children.Add(skillPanel);

        // Time estimate
        var timePanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5)
        };

        var timeText = new TextBlock
        {
            Text = FormatTimeSpan(skill.EstimatedTime),
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 0)),
            HorizontalAlignment = HorizontalAlignment.Right
        };
        timePanel.Children.Add(timeText);

        var priorityText = new TextBlock
        {
            Text = skill.Priority.ToString().ToUpper(),
            FontSize = 8,
            FontWeight = FontWeights.Bold,
            Foreground = GetPriorityColor(skill.Priority),
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 2, 0, 0)
        };
        timePanel.Children.Add(priorityText);

        Grid.SetColumn(timePanel, 2);
        grid.Children.Add(timePanel);

        border.Child = grid;
        return border;
    }

    private Brush GetPriorityColor(SkillPriority priority)
    {
        return priority switch
        {
            SkillPriority.High => new SolidColorBrush(Color.FromArgb(200, 255, 100, 100)),
            SkillPriority.Medium => new SolidColorBrush(Color.FromArgb(200, 255, 255, 100)),
            SkillPriority.Low => new SolidColorBrush(Color.FromArgb(200, 100, 255, 100)),
            _ => new SolidColorBrush(Color.FromArgb(200, 150, 150, 150))
        };
    }

    private string FormatTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan.TotalDays >= 1)
            return $"{(int)timeSpan.TotalDays}d {timeSpan.Hours}h";
        if (timeSpan.TotalHours >= 1)
            return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
        return $"{timeSpan.Minutes}m";
    }

    #endregion

    #region Particle System

    private void CreateSkillTrainingParticles()
    {
        if (!EnableParticleEffects || _particleCanvas == null) return;

        // Create training progress particles
        for (int i = 0; i < 3; i++)
        {
            var particle = new HoloQueueParticle
            {
                X = _random.NextDouble() * ActualWidth,
                Y = _random.NextDouble() * ActualHeight,
                VelocityX = (_random.NextDouble() - 0.5) * 20,
                VelocityY = (_random.NextDouble() - 0.5) * 20,
                Size = _random.NextDouble() * 3 + 1,
                Life = 1.0,
                ParticleType = QueueParticleType.Training,
                Ellipse = new Ellipse
                {
                    Width = 4,
                    Height = 4,
                    Fill = new SolidColorBrush(Color.FromArgb(180, 0, 255, 255))
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
            
            var alpha = (byte)(particle.Life * 180);
            var color = particle.ParticleType switch
            {
                QueueParticleType.Training => Color.FromArgb(alpha, 0, 255, 255),
                QueueParticleType.Queue => Color.FromArgb(alpha, 255, 255, 0),
                QueueParticleType.Complete => Color.FromArgb(alpha, 0, 255, 100),
                _ => Color.FromArgb(alpha, 255, 255, 255)
            };
            
            particle.Ellipse.Fill = new SolidColorBrush(color);
        }

        // Create new particles periodically
        if (_random.NextDouble() < 0.1)
        {
            CreateSkillTrainingParticles();
        }
    }

    #endregion

    #region Animation and Updates

    private void OnAnimationTick(object sender, EventArgs e)
    {
        if (!_isAnimating) return;

        _currentAnimationTime += 16; // 16ms per frame
        
        UpdateParticles();
        UpdateTrainingAnimations();
    }

    private void OnProgressUpdateTick(object sender, EventArgs e)
    {
        UpdateTrainingProgress();
    }

    private void UpdateTrainingAnimations()
    {
        if (!EnableAnimations) return;

        // Animate current training section glow
        var intensity = (Math.Sin(_currentAnimationTime * 0.002) + 1) * 0.5 * HolographicIntensity;
        
        if (_currentTrainingSection?.Effect is DropShadowEffect effect)
        {
            effect.BlurRadius = 10 + intensity * 5;
        }
    }

    private void UpdateTrainingProgress()
    {
        if (CurrentlyTraining != null)
        {
            // Simulate training progress (in real implementation, this would come from ESI)
            CurrentlyTraining.TrainingProgress += 0.001; // Small increment for demo
            
            if (CurrentlyTraining.TrainingProgress >= 1.0)
            {
                // Skill training completed
                OnSkillTrainingCompleted(CurrentlyTraining);
            }
        }
    }

    private void OnSkillTrainingCompleted(HoloQueuedSkill completedSkill)
    {
        // Create completion particles
        for (int i = 0; i < 10; i++)
        {
            var particle = new HoloQueueParticle
            {
                X = ActualWidth * 0.5,
                Y = ActualHeight * 0.3,
                VelocityX = (_random.NextDouble() - 0.5) * 100,
                VelocityY = (_random.NextDouble() - 0.5) * 100,
                Size = _random.NextDouble() * 5 + 2,
                Life = 1.0,
                ParticleType = QueueParticleType.Complete,
                Ellipse = new Ellipse
                {
                    Width = 6,
                    Height = 6,
                    Fill = new SolidColorBrush(Color.FromArgb(255, 0, 255, 100))
                }
            };

            Canvas.SetLeft(particle.Ellipse, particle.X);
            Canvas.SetTop(particle.Ellipse, particle.Y);
            _particleCanvas.Children.Add(particle.Ellipse);
            _particles.Add(particle);
        }

        // Move to next skill in queue
        if (SkillQueue != null && SkillQueue.Count > 0)
        {
            SkillQueue.RemoveAt(0);
            CurrentlyTraining = SkillQueue.Count > 0 ? SkillQueue[0] : null;
            UpdateQueueDisplay();
        }
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _animationTimer.Start();
        _progressUpdateTimer.Start();
        _isAnimating = true;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer.Stop();
        _progressUpdateTimer.Stop();
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
        if (d is HoloSkillQueue control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillQueue control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnSkillQueueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillQueue control)
        {
            control.UpdateQueueDisplay();
        }
    }

    private static void OnCurrentlyTrainingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillQueue control)
        {
            control.UpdateCurrentTrainingDisplay();
        }
    }

    private static void OnSelectedCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillQueue control)
        {
            control.LoadCharacterSkillQueue();
        }
    }

    private static void OnQueueModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillQueue control)
        {
            control.UpdateQueueMode();
        }
    }

    private static void OnShowEstimatedTimesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillQueue control)
        {
            control.UpdateTimeDisplays();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillQueue control)
        {
            control._isAnimating = control.EnableAnimations;
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillQueue control)
        {
            if (!control.EnableParticleEffects)
            {
                control.ClearParticles();
            }
        }
    }

    private static void OnShowSkillDetailsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillQueue control)
        {
            control.UpdateDetailDisplays();
        }
    }

    private static void OnShowTrainingStatisticsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillQueue control)
        {
            control._statisticsSection.Visibility = control.ShowTrainingStatistics ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnEnableDragDropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillQueue control)
        {
            control.UpdateDragDropSupport();
        }
    }

    private static void OnAutoScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillQueue control)
        {
            control.UpdateScrollBehavior();
        }
    }

    private static void OnShowQueueManagementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillQueue control)
        {
            control._queueManagementSection.Visibility = control.ShowQueueManagement ? Visibility.Visible : Visibility.Collapsed;
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

    private void UpdateCurrentTrainingDisplay()
    {
        // Update the current training section with new data
    }

    private void LoadCharacterSkillQueue()
    {
        // Load skill queue for selected character
    }

    private void UpdateQueueMode()
    {
        // Update display based on queue mode (Active, Paused, etc.)
    }

    private void UpdateTimeDisplays()
    {
        // Show/hide time estimates
    }

    private void ClearParticles()
    {
        foreach (var particle in _particles)
        {
            _particleCanvas.Children.Remove(particle.Ellipse);
        }
        _particles.Clear();
    }

    private void UpdateDetailDisplays()
    {
        // Show/hide detailed skill information
    }

    private void UpdateDragDropSupport()
    {
        // Enable/disable drag and drop for queue reordering
    }

    private void UpdateScrollBehavior()
    {
        // Configure auto-scroll behavior
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
        _progressUpdateTimer?.Stop();
        ClearParticles();
        
        _isDisposed = true;
    }

    #endregion
}

#region Supporting Classes

public class HoloQueuedSkill
{
    public string SkillName { get; set; } = "";
    public int CurrentLevel { get; set; }
    public int TargetLevel { get; set; }
    public double TrainingProgress { get; set; }
    public TimeSpan EstimatedTime { get; set; }
    public long SkillPoints { get; set; }
    public long RequiredSkillPoints { get; set; }
    public SkillPriority Priority { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EstimatedCompletion { get; set; }
    public bool IsPaused { get; set; }
    public string Category { get; set; } = "";
    public List<string> Prerequisites { get; set; } = new();
}

public class HoloQueueParticle
{
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Size { get; set; }
    public double Life { get; set; }
    public QueueParticleType ParticleType { get; set; }
    public Ellipse Ellipse { get; set; } = null!;
}

public enum SkillQueueMode
{
    Active,
    Paused,
    Planning,
    Completed
}

public enum SkillPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum QueueParticleType
{
    Training,
    Queue,
    Complete,
    Warning
}

#endregion