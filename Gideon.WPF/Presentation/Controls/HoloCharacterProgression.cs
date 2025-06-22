// ==========================================================================
// HoloCharacterProgression.cs - Holographic Character Progression Timeline
// ==========================================================================
// Advanced character progression featuring interactive timeline visualization,
// skill training history, EVE-style progression tracking, and holographic milestone display.
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
/// Holographic character progression timeline with skill training history and milestone tracking
/// </summary>
public class HoloCharacterProgression : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloCharacterProgression),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloCharacterProgression),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty ProgressionEventsProperty =
        DependencyProperty.Register(nameof(ProgressionEvents), typeof(ObservableCollection<HoloProgressionEvent>), typeof(HoloCharacterProgression),
            new PropertyMetadata(null, OnProgressionEventsChanged));

    public static readonly DependencyProperty CharacterMilestonesProperty =
        DependencyProperty.Register(nameof(CharacterMilestones), typeof(ObservableCollection<HoloCharacterMilestone>), typeof(HoloCharacterProgression),
            new PropertyMetadata(null, OnCharacterMilestonesChanged));

    public static readonly DependencyProperty SelectedCharacterProperty =
        DependencyProperty.Register(nameof(SelectedCharacter), typeof(string), typeof(HoloCharacterProgression),
            new PropertyMetadata("Main Character", OnSelectedCharacterChanged));

    public static readonly DependencyProperty TimeFrameProperty =
        DependencyProperty.Register(nameof(TimeFrame), typeof(ProgressionTimeFrame), typeof(HoloCharacterProgression),
            new PropertyMetadata(ProgressionTimeFrame.Year, OnTimeFrameChanged));

    public static readonly DependencyProperty ViewModeProperty =
        DependencyProperty.Register(nameof(ViewMode), typeof(ProgressionViewMode), typeof(HoloCharacterProgression),
            new PropertyMetadata(ProgressionViewMode.Timeline, OnViewModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloCharacterProgression),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloCharacterProgression),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowMilestonesProperty =
        DependencyProperty.Register(nameof(ShowMilestones), typeof(bool), typeof(HoloCharacterProgression),
            new PropertyMetadata(true, OnShowMilestonesChanged));

    public static readonly DependencyProperty ShowSkillPointsGraphProperty =
        DependencyProperty.Register(nameof(ShowSkillPointsGraph), typeof(bool), typeof(HoloCharacterProgression),
            new PropertyMetadata(true, OnShowSkillPointsGraphChanged));

    public static readonly DependencyProperty ShowTrainingEfficiencyProperty =
        DependencyProperty.Register(nameof(ShowTrainingEfficiency), typeof(bool), typeof(HoloCharacterProgression),
            new PropertyMetadata(true, OnShowTrainingEfficiencyChanged));

    public static readonly DependencyProperty AutoScrollProperty =
        DependencyProperty.Register(nameof(AutoScroll), typeof(bool), typeof(HoloCharacterProgression),
            new PropertyMetadata(true, OnAutoScrollChanged));

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

    public ObservableCollection<HoloProgressionEvent> ProgressionEvents
    {
        get => (ObservableCollection<HoloProgressionEvent>)GetValue(ProgressionEventsProperty);
        set => SetValue(ProgressionEventsProperty, value);
    }

    public ObservableCollection<HoloCharacterMilestone> CharacterMilestones
    {
        get => (ObservableCollection<HoloCharacterMilestone>)GetValue(CharacterMilestonesProperty);
        set => SetValue(CharacterMilestonesProperty, value);
    }

    public string SelectedCharacter
    {
        get => (string)GetValue(SelectedCharacterProperty);
        set => SetValue(SelectedCharacterProperty, value);
    }

    public ProgressionTimeFrame TimeFrame
    {
        get => (ProgressionTimeFrame)GetValue(TimeFrameProperty);
        set => SetValue(TimeFrameProperty, value);
    }

    public ProgressionViewMode ViewMode
    {
        get => (ProgressionViewMode)GetValue(ViewModeProperty);
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

    public bool ShowMilestones
    {
        get => (bool)GetValue(ShowMilestonesProperty);
        set => SetValue(ShowMilestonesProperty, value);
    }

    public bool ShowSkillPointsGraph
    {
        get => (bool)GetValue(ShowSkillPointsGraphProperty);
        set => SetValue(ShowSkillPointsGraphProperty, value);
    }

    public bool ShowTrainingEfficiency
    {
        get => (bool)GetValue(ShowTrainingEfficiencyProperty);
        set => SetValue(ShowTrainingEfficiencyProperty, value);
    }

    public bool AutoScroll
    {
        get => (bool)GetValue(AutoScrollProperty);
        set => SetValue(AutoScrollProperty, value);
    }

    #endregion

    #region Fields

    private Canvas _progressionCanvas;
    private ScrollViewer _timelineScrollViewer;
    private Canvas _timelineCanvas;
    private DispatcherTimer _animationTimer;
    private DispatcherTimer _particleTimer;
    private Dictionary<string, Material> _materialCache;
    private Random _random;
    private List<UIElement> _particles;
    private List<HoloProgressionParticle> _progressionParticles;
    private bool _isInitialized;
    private DateTime _timelineStart;
    private DateTime _timelineEnd;

    #endregion

    #region Constructor

    public HoloCharacterProgression()
    {
        InitializeComponent();
        InitializeFields();
        InitializeProgression();
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
        
        _progressionCanvas = new Canvas
        {
            Width = Width,
            Height = Height,
            ClipToBounds = true
        };
        
        Content = _progressionCanvas;
    }

    private void InitializeFields()
    {
        _materialCache = new Dictionary<string, Material>();
        _random = new Random();
        _particles = new List<UIElement>();
        _progressionParticles = new List<HoloProgressionParticle>();
        _isInitialized = false;
        
        ProgressionEvents = new ObservableCollection<HoloProgressionEvent>();
        CharacterMilestones = new ObservableCollection<HoloCharacterMilestone>();
        GenerateSampleProgression();
    }

    private void InitializeProgression()
    {
        CalculateTimelineRange();
        CreateProgressionInterface();
        UpdateProgressionVisuals();
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
            Interval = TimeSpan.FromMilliseconds(120)
        };
        _particleTimer.Tick += OnParticleTick;
    }

    private void GenerateSampleProgression()
    {
        var baseDate = DateTime.Now.AddMonths(-12);
        
        // Generate skill training events
        var skillNames = new[] { "Gunnery", "Spaceship Command", "Engineering", "Shield Management", "Caldari Frigate", "Medium Hybrid Turret", "Navigation", "Afterburner" };
        
        for (int i = 0; i < 30; i++)
        {
            var eventDate = baseDate.AddDays(_random.Next(0, 365));
            var skillName = skillNames[_random.Next(skillNames.Length)];
            var newLevel = _random.Next(1, 6);
            
            var progressionEvent = new HoloProgressionEvent
            {
                Id = Guid.NewGuid().ToString(),
                EventType = ProgressionEventType.SkillCompleted,
                Title = $"{skillName} Level {newLevel}",
                Description = $"Completed training {skillName} to level {newLevel}",
                Timestamp = eventDate,
                SkillName = skillName,
                SkillLevel = newLevel,
                SkillPointsGained = GetSkillPointsForLevel(newLevel) - GetSkillPointsForLevel(newLevel - 1),
                Character = SelectedCharacter,
                Category = GetSkillCategory(skillName)
            };
            
            ProgressionEvents.Add(progressionEvent);
        }
        
        // Generate milestone events
        var milestones = new[]
        {
            (1_000_000L, "1M Skill Points", "Reached 1 million total skill points"),
            (5_000_000L, "5M Skill Points", "Reached 5 million total skill points"),
            (10_000_000L, "10M Skill Points", "Reached 10 million total skill points"),
            (25_000_000L, "25M Skill Points", "Reached 25 million total skill points"),
            (50_000_000L, "50M Skill Points", "Reached 50 million total skill points")
        };
        
        var currentSP = 0L;
        
        foreach (var milestone in milestones)
        {
            if (currentSP < milestone.Item1)
            {
                var milestoneDate = baseDate.AddDays(_random.Next(30, 350));
                
                var characterMilestone = new HoloCharacterMilestone
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = milestone.Item2,
                    Description = milestone.Item3,
                    Timestamp = milestoneDate,
                    MilestoneType = MilestoneType.SkillPoints,
                    TargetValue = milestone.Item1,
                    CurrentValue = milestone.Item1,
                    IsAchieved = milestoneDate <= DateTime.Now,
                    Character = SelectedCharacter
                };
                
                CharacterMilestones.Add(characterMilestone);
                currentSP = milestone.Item1;
            }
        }
        
        // Add ship milestones
        var shipMilestones = new[]
        {
            ("First Cruiser", "Trained to fly first cruiser class ship", MilestoneType.ShipUnlocked),
            ("First Battlecruiser", "Trained to fly first battlecruiser class ship", MilestoneType.ShipUnlocked),
            ("First Battleship", "Trained to fly first battleship class ship", MilestoneType.ShipUnlocked)
        };
        
        foreach (var shipMilestone in shipMilestones)
        {
            var milestoneDate = baseDate.AddDays(_random.Next(60, 300));
            
            var milestone = new HoloCharacterMilestone
            {
                Id = Guid.NewGuid().ToString(),
                Title = shipMilestone.Item1,
                Description = shipMilestone.Item2,
                Timestamp = milestoneDate,
                MilestoneType = shipMilestone.Item3,
                IsAchieved = milestoneDate <= DateTime.Now,
                Character = SelectedCharacter
            };
            
            CharacterMilestones.Add(milestone);
        }
    }

    private long GetSkillPointsForLevel(int level)
    {
        if (level <= 0) return 0;
        var basePoints = new[] { 0L, 250L, 1414L, 8000L, 45255L, 256000L };
        return level <= 5 ? basePoints[level] : basePoints[5];
    }

    private string GetSkillCategory(string skillName)
    {
        return skillName switch
        {
            "Gunnery" or "Small Hybrid Turret" or "Medium Hybrid Turret" => "Gunnery",
            "Spaceship Command" or "Caldari Frigate" => "Spaceship",
            "Engineering" or "Shield Management" => "Engineering",
            "Navigation" or "Afterburner" => "Navigation",
            _ => "General"
        };
    }

    #endregion

    #region Progression Interface Creation

    private void CreateProgressionInterface()
    {
        _progressionCanvas.Children.Clear();
        
        CreateHeaderSection();
        CreateStatsSection();
        CreateTimelineSection();
        CreateAnalysisSection();
        CreateParticleEffects();
    }

    private void CreateHeaderSection()
    {
        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(20, 10, 20, 10),
            Background = CreateHolographicBrush(0.4)
        };
        
        Canvas.SetLeft(headerPanel, 0);
        Canvas.SetTop(headerPanel, 0);
        _progressionCanvas.Children.Add(headerPanel);
        
        var titleBlock = new TextBlock
        {
            Text = $"Character Progression - {SelectedCharacter}",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary"),
            Margin = new Thickness(10),
            VerticalAlignment = VerticalAlignment.Center
        };
        headerPanel.Children.Add(titleBlock);
        
        AddHeaderButton(headerPanel, "Timeline", () => ViewMode = ProgressionViewMode.Timeline);
        AddHeaderButton(headerPanel, "Stats", () => ViewMode = ProgressionViewMode.Statistics);
        AddHeaderButton(headerPanel, "Milestones", () => ViewMode = ProgressionViewMode.Milestones);
        AddHeaderButton(headerPanel, "Year", () => TimeFrame = ProgressionTimeFrame.Year);
        AddHeaderButton(headerPanel, "All Time", () => TimeFrame = ProgressionTimeFrame.AllTime);
    }

    private void CreateStatsSection()
    {
        var statsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(20, 5, 20, 5),
            Background = CreateHolographicBrush(0.3)
        };
        
        Canvas.SetLeft(statsPanel, 0);
        Canvas.SetTop(statsPanel, 60);
        _progressionCanvas.Children.Add(statsPanel);
        
        var stats = CalculateProgressionStats();
        
        AddStatsCard(statsPanel, "Total Skills", stats.TotalSkillsCompleted.ToString());
        AddStatsCard(statsPanel, "Skill Points", FormatSkillPoints(stats.TotalSkillPoints));
        AddStatsCard(statsPanel, "Training Days", stats.TotalTrainingDays.ToString("F0"));
        AddStatsCard(statsPanel, "Milestones", $"{stats.MilestonesAchieved}/{stats.TotalMilestones}");
        AddStatsCard(statsPanel, "Efficiency", $"{stats.TrainingEfficiency:F1}%");
        AddStatsCard(statsPanel, "SP/Day", FormatSkillPoints(stats.SkillPointsPerDay));
    }

    private void CreateTimelineSection()
    {
        _timelineScrollViewer = new ScrollViewer
        {
            Width = Width - 40,
            Height = ViewMode == ProgressionViewMode.Timeline ? 400 : 300,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            Background = CreateHolographicBrush(0.2)
        };
        
        Canvas.SetLeft(_timelineScrollViewer, 20);
        Canvas.SetTop(_timelineScrollViewer, 120);
        _progressionCanvas.Children.Add(_timelineScrollViewer);
        
        var timelineDays = (_timelineEnd - _timelineStart).Days;
        var timelineWidth = Math.Max(Width - 40, timelineDays * 3);
        
        _timelineCanvas = new Canvas
        {
            Width = timelineWidth,
            Height = _timelineScrollViewer.Height - 20,
            Background = CreateHolographicBrush(0.1)
        };
        
        _timelineScrollViewer.Content = _timelineCanvas;
        
        CreateTimelineAxis();
        CreateTimelineEvents();
        CreateTimelineMilestones();
        
        if (AutoScroll)
        {
            _timelineScrollViewer.ScrollToRightEnd();
        }
    }

    private void CreateTimelineAxis()
    {
        var axisHeight = _timelineCanvas.Height - 40;
        
        // Main timeline axis
        var mainAxis = new Line
        {
            X1 = 50,
            Y1 = axisHeight / 2,
            X2 = _timelineCanvas.Width - 50,
            Y2 = axisHeight / 2,
            Stroke = GetEVEBrush("Secondary"),
            StrokeThickness = 2
        };
        _timelineCanvas.Children.Add(mainAxis);
        
        // Time markers
        var totalDays = (_timelineEnd - _timelineStart).Days;
        var markerInterval = Math.Max(1, totalDays / 20);
        
        for (int day = 0; day <= totalDays; day += markerInterval)
        {
            var date = _timelineStart.AddDays(day);
            var x = 50 + (day / (double)totalDays) * (_timelineCanvas.Width - 100);
            
            var marker = new Line
            {
                X1 = x,
                Y1 = axisHeight / 2 - 10,
                X2 = x,
                Y2 = axisHeight / 2 + 10,
                Stroke = GetEVEBrush("Secondary"),
                StrokeThickness = 1
            };
            _timelineCanvas.Children.Add(marker);
            
            var dateLabel = new TextBlock
            {
                Text = date.ToString("MMM"),
                Foreground = GetEVEBrush("Secondary"),
                FontSize = 8,
                TextAlignment = TextAlignment.Center
            };
            Canvas.SetLeft(dateLabel, x - 15);
            Canvas.SetTop(dateLabel, axisHeight / 2 + 15);
            _timelineCanvas.Children.Add(dateLabel);
        }
    }

    private void CreateTimelineEvents()
    {
        var filteredEvents = GetFilteredEvents();
        var axisHeight = _timelineCanvas.Height - 40;
        var totalDays = (_timelineEnd - _timelineStart).Days;
        
        var eventGroups = filteredEvents.GroupBy(e => e.Category).ToList();
        var groupHeight = (axisHeight - 100) / Math.Max(1, eventGroups.Count);
        
        for (int i = 0; i < eventGroups.Count; i++)
        {
            var group = eventGroups[i];
            var groupY = 50 + i * groupHeight;
            
            // Group label
            var groupLabel = new TextBlock
            {
                Text = group.Key,
                Foreground = GetEVEBrush("Primary"),
                FontWeight = FontWeights.Bold,
                FontSize = 10
            };
            Canvas.SetLeft(groupLabel, 10);
            Canvas.SetTop(groupLabel, groupY);
            _timelineCanvas.Children.Add(groupLabel);
            
            // Group events
            foreach (var evt in group.OrderBy(e => e.Timestamp))
            {
                var daysSinceStart = (evt.Timestamp - _timelineStart).Days;
                var x = 50 + (daysSinceStart / (double)totalDays) * (_timelineCanvas.Width - 100);
                
                var eventNode = CreateTimelineEventNode(evt, x, groupY + 20);
                _timelineCanvas.Children.Add(eventNode);
            }
        }
    }

    private void CreateTimelineMilestones()
    {
        if (!ShowMilestones) return;
        
        var filteredMilestones = GetFilteredMilestones();
        var totalDays = (_timelineEnd - _timelineStart).Days;
        var axisHeight = _timelineCanvas.Height - 40;
        
        foreach (var milestone in filteredMilestones)
        {
            var daysSinceStart = (milestone.Timestamp - _timelineStart).Days;
            var x = 50 + (daysSinceStart / (double)totalDays) * (_timelineCanvas.Width - 100);
            
            var milestoneNode = CreateTimelineMilestoneNode(milestone, x, axisHeight / 2);
            _timelineCanvas.Children.Add(milestoneNode);
        }
    }

    private FrameworkElement CreateTimelineEventNode(HoloProgressionEvent evt, double x, double y)
    {
        var eventNode = new Border
        {
            Width = 12,
            Height = 12,
            Background = GetEventBrush(evt),
            BorderBrush = GetEVEBrush("Primary"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Cursor = Cursors.Hand
        };
        
        Canvas.SetLeft(eventNode, x - 6);
        Canvas.SetTop(eventNode, y);
        
        eventNode.ToolTip = CreateEventTooltip(evt);
        
        eventNode.MouseEnter += (s, e) => ApplyEventHoverEffect(eventNode, true);
        eventNode.MouseLeave += (s, e) => ApplyEventHoverEffect(eventNode, false);
        
        if (EnableAnimations)
        {
            ApplyEventNodeAnimation(eventNode);
        }
        
        return eventNode;
    }

    private FrameworkElement CreateTimelineMilestoneNode(HoloCharacterMilestone milestone, double x, double y)
    {
        var milestoneNode = new Polygon
        {
            Points = new PointCollection { new Point(0, 15), new Point(7.5, 0), new Point(15, 15), new Point(7.5, 10) },
            Fill = GetMilestoneBrush(milestone),
            Stroke = GetEVEBrush("Warning"),
            StrokeThickness = 2,
            Cursor = Cursors.Hand
        };
        
        Canvas.SetLeft(milestoneNode, x - 7.5);
        Canvas.SetTop(milestoneNode, y - 25);
        
        milestoneNode.ToolTip = CreateMilestoneTooltip(milestone);
        
        if (EnableAnimations)
        {
            ApplyMilestoneNodeAnimation(milestoneNode);
        }
        
        return milestoneNode;
    }

    private void CreateAnalysisSection()
    {
        if (ViewMode != ProgressionViewMode.Statistics) return;
        
        var analysisPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(20),
            Background = CreateHolographicBrush(0.25)
        };
        
        Canvas.SetLeft(analysisPanel, 20);
        Canvas.SetTop(analysisPanel, ViewMode == ProgressionViewMode.Timeline ? 540 : 440);
        _progressionCanvas.Children.Add(analysisPanel);
        
        if (ShowSkillPointsGraph)
            CreateSkillPointsGraph(analysisPanel);
        
        if (ShowTrainingEfficiency)
            CreateTrainingEfficiencyChart(analysisPanel);
        
        CreateProgressionSummary(analysisPanel);
    }

    #endregion

    #region Analysis and Statistics

    private void CalculateTimelineRange()
    {
        var allEvents = ProgressionEvents?.Concat(CharacterMilestones?.Select(m => new HoloProgressionEvent { Timestamp = m.Timestamp }) ?? Enumerable.Empty<HoloProgressionEvent>()) ?? Enumerable.Empty<HoloProgressionEvent>();
        
        if (allEvents.Any())
        {
            _timelineStart = TimeFrame switch
            {
                ProgressionTimeFrame.Month => DateTime.Now.AddMonths(-1),
                ProgressionTimeFrame.Quarter => DateTime.Now.AddMonths(-3),
                ProgressionTimeFrame.Year => DateTime.Now.AddYears(-1),
                ProgressionTimeFrame.AllTime => allEvents.Min(e => e.Timestamp),
                _ => DateTime.Now.AddYears(-1)
            };
            
            _timelineEnd = DateTime.Now;
        }
        else
        {
            _timelineStart = DateTime.Now.AddYears(-1);
            _timelineEnd = DateTime.Now;
        }
    }

    private HoloProgressionStats CalculateProgressionStats()
    {
        var filteredEvents = GetFilteredEvents();
        var filteredMilestones = GetFilteredMilestones();
        
        var stats = new HoloProgressionStats
        {
            TotalSkillsCompleted = filteredEvents.Count(e => e.EventType == ProgressionEventType.SkillCompleted),
            TotalSkillPoints = filteredEvents.Sum(e => e.SkillPointsGained),
            TotalTrainingDays = (_timelineEnd - _timelineStart).TotalDays,
            MilestonesAchieved = filteredMilestones.Count(m => m.IsAchieved),
            TotalMilestones = filteredMilestones.Count()
        };
        
        stats.SkillPointsPerDay = stats.TotalTrainingDays > 0 ? stats.TotalSkillPoints / stats.TotalTrainingDays : 0;
        stats.TrainingEfficiency = CalculateTrainingEfficiency(filteredEvents);
        
        return stats;
    }

    private double CalculateTrainingEfficiency(IEnumerable<HoloProgressionEvent> events)
    {
        if (!events.Any()) return 0;
        
        var dailyEvents = events.GroupBy(e => e.Timestamp.Date).ToList();
        var activeDays = dailyEvents.Count;
        var totalDays = (_timelineEnd - _timelineStart).Days;
        
        return totalDays > 0 ? (activeDays / (double)totalDays) * 100 : 0;
    }

    private IEnumerable<HoloProgressionEvent> GetFilteredEvents()
    {
        if (ProgressionEvents == null) return Enumerable.Empty<HoloProgressionEvent>();
        
        return ProgressionEvents.Where(e => e.Timestamp >= _timelineStart && e.Timestamp <= _timelineEnd);
    }

    private IEnumerable<HoloCharacterMilestone> GetFilteredMilestones()
    {
        if (CharacterMilestones == null) return Enumerable.Empty<HoloCharacterMilestone>();
        
        return CharacterMilestones.Where(m => m.Timestamp >= _timelineStart && m.Timestamp <= _timelineEnd);
    }

    #endregion

    #region Chart Creation

    private void CreateSkillPointsGraph(Panel parent)
    {
        var graphPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Width = 300,
            Margin = new Thickness(10),
            Background = CreateHolographicBrush(0.2)
        };
        
        var titleBlock = new TextBlock
        {
            Text = "Skill Points Over Time",
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary"),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(5)
        };
        graphPanel.Children.Add(titleBlock);
        
        var graphCanvas = new Canvas
        {
            Width = 280,
            Height = 150,
            Background = CreateHolographicBrush(0.1),
            Margin = new Thickness(5)
        };
        
        DrawSkillPointsGraph(graphCanvas);
        graphPanel.Children.Add(graphCanvas);
        parent.Children.Add(graphPanel);
    }

    private void CreateTrainingEfficiencyChart(Panel parent)
    {
        var chartPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Width = 300,
            Margin = new Thickness(10),
            Background = CreateHolographicBrush(0.2)
        };
        
        var titleBlock = new TextBlock
        {
            Text = "Training Efficiency",
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary"),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(5)
        };
        chartPanel.Children.Add(titleBlock);
        
        var chartCanvas = new Canvas
        {
            Width = 280,
            Height = 150,
            Background = CreateHolographicBrush(0.1),
            Margin = new Thickness(5)
        };
        
        DrawTrainingEfficiencyChart(chartCanvas);
        chartPanel.Children.Add(chartCanvas);
        parent.Children.Add(chartPanel);
    }

    private void CreateProgressionSummary(Panel parent)
    {
        var summaryPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Width = 300,
            Margin = new Thickness(10),
            Background = CreateHolographicBrush(0.2)
        };
        
        var titleBlock = new TextBlock
        {
            Text = "Progression Summary",
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary"),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(5)
        };
        summaryPanel.Children.Add(titleBlock);
        
        var stats = CalculateProgressionStats();
        var categories = GetFilteredEvents().GroupBy(e => e.Category).OrderByDescending(g => g.Count());
        
        AddSummaryMetric(summaryPanel, "Most Trained:", categories.FirstOrDefault()?.Key ?? "N/A");
        AddSummaryMetric(summaryPanel, "Training Streak:", CalculateTrainingStreak().ToString() + " days");
        AddSummaryMetric(summaryPanel, "Avg SP/Skill:", (stats.TotalSkillPoints / Math.Max(1, stats.TotalSkillsCompleted)).ToString("F0"));
        AddSummaryMetric(summaryPanel, "Time to Next Million:", EstimateTimeToNextMillion());
        AddSummaryMetric(summaryPanel, "Character Age:", CalculateCharacterAge());
        
        parent.Children.Add(summaryPanel);
    }

    private void DrawSkillPointsGraph(Canvas canvas)
    {
        var events = GetFilteredEvents().Where(e => e.EventType == ProgressionEventType.SkillCompleted)
                                       .OrderBy(e => e.Timestamp).ToList();
        
        if (events.Count < 2) return;
        
        var cumulativeSP = 0L;
        var points = new List<Point>();
        
        foreach (var evt in events)
        {
            cumulativeSP += evt.SkillPointsGained;
            var x = ((evt.Timestamp - _timelineStart).TotalDays / (_timelineEnd - _timelineStart).TotalDays) * canvas.Width;
            var y = canvas.Height - (cumulativeSP / (double)Math.Max(cumulativeSP, 1000000)) * canvas.Height * 0.8;
            points.Add(new Point(x, y));
        }
        
        for (int i = 1; i < points.Count; i++)
        {
            var line = new Line
            {
                X1 = points[i - 1].X,
                Y1 = points[i - 1].Y,
                X2 = points[i].X,
                Y2 = points[i].Y,
                Stroke = GetEVEBrush("Success"),
                StrokeThickness = 2,
                Opacity = 0.8
            };
            canvas.Children.Add(line);
        }
    }

    private void DrawTrainingEfficiencyChart(Canvas canvas)
    {
        var events = GetFilteredEvents().GroupBy(e => e.Timestamp.Date)
                                       .OrderBy(g => g.Key).ToList();
        
        if (!events.Any()) return;
        
        var maxEvents = events.Max(g => g.Count());
        var barWidth = canvas.Width / events.Count;
        
        for (int i = 0; i < events.Count; i++)
        {
            var group = events[i];
            var barHeight = (group.Count() / (double)maxEvents) * canvas.Height * 0.8;
            
            var bar = new Rectangle
            {
                Width = Math.Max(1, barWidth - 1),
                Height = barHeight,
                Fill = GetEVEBrush("Info"),
                Opacity = 0.7
            };
            
            Canvas.SetLeft(bar, i * barWidth);
            Canvas.SetBottom(bar, 0);
            canvas.Children.Add(bar);
        }
    }

    private int CalculateTrainingStreak()
    {
        var events = GetFilteredEvents().OrderByDescending(e => e.Timestamp).ToList();
        var streak = 0;
        var currentDate = DateTime.Now.Date;
        
        foreach (var evt in events)
        {
            if (evt.Timestamp.Date == currentDate || evt.Timestamp.Date == currentDate.AddDays(-1))
            {
                streak++;
                currentDate = evt.Timestamp.Date.AddDays(-1);
            }
            else
            {
                break;
            }
        }
        
        return streak;
    }

    private string EstimateTimeToNextMillion()
    {
        var stats = CalculateProgressionStats();
        if (stats.SkillPointsPerDay <= 0) return "N/A";
        
        var currentSP = stats.TotalSkillPoints;
        var nextMillion = ((currentSP / 1_000_000) + 1) * 1_000_000;
        var spNeeded = nextMillion - currentSP;
        var daysNeeded = spNeeded / stats.SkillPointsPerDay;
        
        return daysNeeded > 365 ? $"{daysNeeded / 365:F1} years" : $"{daysNeeded:F0} days";
    }

    private string CalculateCharacterAge()
    {
        var firstEvent = ProgressionEvents?.MinBy(e => e.Timestamp);
        if (firstEvent == null) return "N/A";
        
        var age = DateTime.Now - firstEvent.Timestamp;
        return age.TotalDays > 365 ? $"{age.TotalDays / 365:F1} years" : $"{age.TotalDays:F0} days";
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

    private void AddStatsCard(Panel parent, string label, string value)
    {
        var card = new Border
        {
            Width = 140,
            Height = 50,
            Background = CreateHolographicBrush(0.3),
            BorderBrush = GetEVEBrush("Accent"),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(5),
            CornerRadius = new CornerRadius(3)
        };
        
        var panel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        
        var labelBlock = new TextBlock
        {
            Text = label,
            Foreground = GetEVEBrush("Primary"),
            FontSize = 10,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        
        var valueBlock = new TextBlock
        {
            Text = value,
            Foreground = GetEVEBrush("Secondary"),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        
        panel.Children.Add(labelBlock);
        panel.Children.Add(valueBlock);
        card.Child = panel;
        parent.Children.Add(card);
    }

    private void AddSummaryMetric(Panel parent, string label, string value)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
        
        var labelElement = new TextBlock
        {
            Text = label,
            Width = 120,
            Foreground = GetEVEBrush("Primary"),
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 10
        };
        
        var valueElement = new TextBlock
        {
            Text = value,
            Width = 150,
            Foreground = GetEVEBrush("Secondary"),
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 10
        };
        
        panel.Children.Add(labelElement);
        panel.Children.Add(valueElement);
        parent.Children.Add(panel);
    }

    private ToolTip CreateEventTooltip(HoloProgressionEvent evt)
    {
        var tooltip = new ToolTip();
        var panel = new StackPanel { Margin = new Thickness(5) };
        
        var titleBlock = new TextBlock
        {
            Text = evt.Title,
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Primary")
        };
        
        var descBlock = new TextBlock
        {
            Text = evt.Description,
            Foreground = GetEVEBrush("Secondary"),
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 200
        };
        
        var dateBlock = new TextBlock
        {
            Text = evt.Timestamp.ToString("MMM dd, yyyy"),
            Foreground = GetEVEBrush("Info"),
            FontSize = 10
        };
        
        panel.Children.Add(titleBlock);
        panel.Children.Add(descBlock);
        panel.Children.Add(dateBlock);
        tooltip.Content = panel;
        
        return tooltip;
    }

    private ToolTip CreateMilestoneTooltip(HoloCharacterMilestone milestone)
    {
        var tooltip = new ToolTip();
        var panel = new StackPanel { Margin = new Thickness(5) };
        
        var titleBlock = new TextBlock
        {
            Text = milestone.Title,
            FontWeight = FontWeights.Bold,
            Foreground = GetEVEBrush("Warning")
        };
        
        var descBlock = new TextBlock
        {
            Text = milestone.Description,
            Foreground = GetEVEBrush("Secondary"),
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 200
        };
        
        var statusBlock = new TextBlock
        {
            Text = milestone.IsAchieved ? "✓ Achieved" : "○ Pending",
            Foreground = milestone.IsAchieved ? GetEVEBrush("Success") : GetEVEBrush("Warning"),
            FontSize = 10
        };
        
        panel.Children.Add(titleBlock);
        panel.Children.Add(descBlock);
        panel.Children.Add(statusBlock);
        tooltip.Content = panel;
        
        return tooltip;
    }

    private string FormatSkillPoints(double skillPoints)
    {
        if (skillPoints >= 1_000_000)
            return $"{skillPoints / 1_000_000:F1}M";
        if (skillPoints >= 1_000)
            return $"{skillPoints / 1_000:F0}K";
        return $"{skillPoints:F0}";
    }

    #endregion

    #region Visual Helpers and Animation

    private Brush GetEventBrush(HoloProgressionEvent evt)
    {
        return evt.EventType switch
        {
            ProgressionEventType.SkillCompleted => GetEVEBrush("Success"),
            ProgressionEventType.SkillStarted => GetEVEBrush("Info"),
            ProgressionEventType.ShipUnlocked => GetEVEBrush("Warning"),
            _ => GetEVEBrush("Secondary")
        };
    }

    private Brush GetMilestoneBrush(HoloCharacterMilestone milestone)
    {
        if (!milestone.IsAchieved) return GetEVEBrush("Secondary");
        
        return milestone.MilestoneType switch
        {
            MilestoneType.SkillPoints => GetEVEBrush("Success"),
            MilestoneType.ShipUnlocked => GetEVEBrush("Warning"),
            MilestoneType.Achievement => GetEVEBrush("Accent"),
            _ => GetEVEBrush("Info")
        };
    }

    private void ApplyEventHoverEffect(FrameworkElement element, bool isHover)
    {
        if (!EnableAnimations) return;
        
        var storyboard = new Storyboard();
        var scaleAnimation = new DoubleAnimation
        {
            To = isHover ? 1.5 : 1.0,
            Duration = TimeSpan.FromMilliseconds(200),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        
        var scaleTransform = element.RenderTransform as ScaleTransform ?? new ScaleTransform();
        element.RenderTransform = scaleTransform;
        element.RenderTransformOrigin = new Point(0.5, 0.5);
        
        Storyboard.SetTarget(scaleAnimation, scaleTransform);
        Storyboard.SetTargetProperty(scaleAnimation, new PropertyPath("ScaleX"));
        storyboard.Children.Add(scaleAnimation);
        
        var scaleYAnimation = scaleAnimation.Clone();
        Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("ScaleY"));
        storyboard.Children.Add(scaleYAnimation);
        
        storyboard.Begin();
    }

    private void ApplyEventNodeAnimation(FrameworkElement node)
    {
        var delay = _random.NextDouble() * 2000;
        node.Opacity = 0;
        
        var storyboard = new Storyboard { BeginTime = TimeSpan.FromMilliseconds(delay) };
        
        var fadeAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(600),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        
        Storyboard.SetTarget(fadeAnimation, node);
        Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath("Opacity"));
        storyboard.Children.Add(fadeAnimation);
        
        storyboard.Begin();
    }

    private void ApplyMilestoneNodeAnimation(FrameworkElement node)
    {
        var delay = _random.NextDouble() * 3000 + 1000;
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

    private HoloProgressionParticle CreateProgressionParticle()
    {
        var particle = new HoloProgressionParticle
        {
            Element = new Ellipse
            {
                Width = _random.Next(1, 4),
                Height = _random.Next(1, 4),
                Fill = GetEVEBrush("Accent"),
                Opacity = 0.5
            },
            X = _random.NextDouble() * Width,
            Y = _random.NextDouble() * Height,
            VelocityX = (_random.NextDouble() - 0.5) * 1.5,
            VelocityY = (_random.NextDouble() - 0.5) * 1.5,
            Life = 1.0,
            MaxLife = _random.NextDouble() * 8 + 4
        };
        
        Canvas.SetLeft(particle.Element, particle.X);
        Canvas.SetTop(particle.Element, particle.Y);
        
        return particle;
    }

    private void CreateParticleEffects()
    {
        if (!EnableParticleEffects) return;
        
        for (int i = 0; i < 30; i++)
        {
            var particle = CreateProgressionParticle();
            _progressionParticles.Add(particle);
            _progressionCanvas.Children.Add(particle.Element);
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

    #region Timer Events

    private void OnAnimationTick(object sender, EventArgs e)
    {
        UpdateProgressionVisuals();
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        UpdateParticleEffects();
    }

    #endregion

    #region Update Methods

    private void UpdateProgressionVisuals()
    {
        if (!EnableAnimations) return;
        
        var time = DateTime.Now.TimeOfDay.TotalSeconds;
        var intensity = HolographicIntensity * (0.9 + 0.1 * Math.Sin(time * 2));
        
        foreach (var child in _progressionCanvas.Children.OfType<FrameworkElement>())
        {
            if (child.Background is SolidColorBrush brush)
            {
                var color = brush.Color;
                color.A = (byte)(color.A * intensity);
                child.Background = new SolidColorBrush(color);
            }
        }
    }

    private void UpdateParticleEffects()
    {
        if (!EnableParticleEffects) return;
        
        foreach (var particle in _progressionParticles.ToList())
        {
            particle.X += particle.VelocityX;
            particle.Y += particle.VelocityY;
            particle.Life -= 0.08;
            
            if (particle.X < 0 || particle.X > Width || particle.Y < 0 || particle.Y > Height || particle.Life <= 0)
            {
                particle.X = _random.NextDouble() * Width;
                particle.Y = _random.NextDouble() * Height;
                particle.Life = particle.MaxLife;
            }
            
            Canvas.SetLeft(particle.Element, particle.X);
            Canvas.SetTop(particle.Element, particle.Y);
            particle.Element.Opacity = particle.Life / particle.MaxLife * 0.5;
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
        if (d is HoloCharacterProgression progression)
        {
            progression.UpdateProgressionVisuals();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterProgression progression)
        {
            progression.CreateProgressionInterface();
        }
    }

    private static void OnProgressionEventsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterProgression progression)
        {
            progression.CalculateTimelineRange();
            progression.CreateProgressionInterface();
        }
    }

    private static void OnCharacterMilestonesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterProgression progression)
        {
            progression.CalculateTimelineRange();
            progression.CreateProgressionInterface();
        }
    }

    private static void OnSelectedCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterProgression progression)
        {
            progression.CreateProgressionInterface();
        }
    }

    private static void OnTimeFrameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterProgression progression)
        {
            progression.CalculateTimelineRange();
            progression.CreateProgressionInterface();
        }
    }

    private static void OnViewModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterProgression progression)
        {
            progression.CreateProgressionInterface();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterProgression progression)
        {
            if ((bool)e.NewValue)
                progression._animationTimer?.Start();
            else
                progression._animationTimer?.Stop();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterProgression progression)
        {
            if ((bool)e.NewValue)
            {
                progression.CreateParticleEffects();
                progression._particleTimer?.Start();
            }
            else
            {
                progression._particleTimer?.Stop();
                progression._progressionParticles.Clear();
            }
        }
    }

    private static void OnShowMilestonesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterProgression progression)
        {
            progression.CreateProgressionInterface();
        }
    }

    private static void OnShowSkillPointsGraphChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterProgression progression)
        {
            progression.CreateProgressionInterface();
        }
    }

    private static void OnShowTrainingEfficiencyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterProgression progression)
        {
            progression.CreateProgressionInterface();
        }
    }

    private static void OnAutoScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterProgression progression && (bool)e.NewValue)
        {
            progression._timelineScrollViewer?.ScrollToRightEnd();
        }
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void EnableSimplifiedMode()
    {
        EnableAnimations = false;
        EnableParticleEffects = false;
        ShowSkillPointsGraph = false;
        ShowTrainingEfficiency = false;
    }

    public void EnableFullMode()
    {
        EnableAnimations = true;
        EnableParticleEffects = true;
        ShowSkillPointsGraph = true;
        ShowTrainingEfficiency = true;
    }

    #endregion
}

#region Supporting Classes

public class HoloProgressionEvent
{
    public string Id { get; set; }
    public ProgressionEventType EventType { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime Timestamp { get; set; }
    public string SkillName { get; set; }
    public int SkillLevel { get; set; }
    public long SkillPointsGained { get; set; }
    public string Character { get; set; }
    public string Category { get; set; }
}

public class HoloCharacterMilestone
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime Timestamp { get; set; }
    public MilestoneType MilestoneType { get; set; }
    public long TargetValue { get; set; }
    public long CurrentValue { get; set; }
    public bool IsAchieved { get; set; }
    public string Character { get; set; }
}

public class HoloProgressionStats
{
    public int TotalSkillsCompleted { get; set; }
    public long TotalSkillPoints { get; set; }
    public double TotalTrainingDays { get; set; }
    public int MilestonesAchieved { get; set; }
    public int TotalMilestones { get; set; }
    public double SkillPointsPerDay { get; set; }
    public double TrainingEfficiency { get; set; }
}

public class HoloProgressionParticle
{
    public UIElement Element { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
    public double MaxLife { get; set; }
}

public enum ProgressionEventType
{
    SkillStarted,
    SkillCompleted,
    ShipUnlocked,
    MilestoneReached
}

public enum MilestoneType
{
    SkillPoints,
    ShipUnlocked,
    Achievement,
    TimeElapsed
}

public enum ProgressionTimeFrame
{
    Month,
    Quarter,
    Year,
    AllTime
}

public enum ProgressionViewMode
{
    Timeline,
    Statistics,
    Milestones
}

#endregion