// ==========================================================================
// HoloSkillRequirementDisplay.cs - Holographic Skill Requirement Display
// ==========================================================================
// Advanced skill requirement visualization featuring real-time skill analysis,
// training time calculations, EVE-style skill trees, and holographic progress tracking.
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
/// Holographic skill requirement display with real-time training analysis and progress visualization
/// </summary>
public class HoloSkillRequirementDisplay : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloSkillRequirementDisplay),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloSkillRequirementDisplay),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty FittingRequirementsProperty =
        DependencyProperty.Register(nameof(FittingRequirements), typeof(HoloFittingRequirements), typeof(HoloSkillRequirementDisplay),
            new PropertyMetadata(null, OnFittingRequirementsChanged));

    public static readonly DependencyProperty CharacterSkillsProperty =
        DependencyProperty.Register(nameof(CharacterSkills), typeof(ObservableCollection<HoloCharacterSkill>), typeof(HoloSkillRequirementDisplay),
            new PropertyMetadata(null, OnCharacterSkillsChanged));

    public static readonly DependencyProperty DisplayModeProperty =
        DependencyProperty.Register(nameof(DisplayMode), typeof(SkillDisplayMode), typeof(HoloSkillRequirementDisplay),
            new PropertyMetadata(SkillDisplayMode.Requirements, OnDisplayModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloSkillRequirementDisplay),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloSkillRequirementDisplay),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowTrainingTimeProperty =
        DependencyProperty.Register(nameof(ShowTrainingTime), typeof(bool), typeof(HoloSkillRequirementDisplay),
            new PropertyMetadata(true, OnShowTrainingTimeChanged));

    public static readonly DependencyProperty ShowSkillTreeProperty =
        DependencyProperty.Register(nameof(ShowSkillTree), typeof(bool), typeof(HoloSkillRequirementDisplay),
            new PropertyMetadata(true, OnShowSkillTreeChanged));

    public static readonly DependencyProperty ShowMissingOnlyProperty =
        DependencyProperty.Register(nameof(ShowMissingOnly), typeof(bool), typeof(HoloSkillRequirementDisplay),
            new PropertyMetadata(false, OnShowMissingOnlyChanged));

    public static readonly DependencyProperty UpdateIntervalProperty =
        DependencyProperty.Register(nameof(UpdateInterval), typeof(TimeSpan), typeof(HoloSkillRequirementDisplay),
            new PropertyMetadata(TimeSpan.FromMilliseconds(500), OnUpdateIntervalChanged));

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

    public HoloFittingRequirements FittingRequirements
    {
        get => (HoloFittingRequirements)GetValue(FittingRequirementsProperty);
        set => SetValue(FittingRequirementsProperty, value);
    }

    public ObservableCollection<HoloCharacterSkill> CharacterSkills
    {
        get => (ObservableCollection<HoloCharacterSkill>)GetValue(CharacterSkillsProperty);
        set => SetValue(CharacterSkillsProperty, value);
    }

    public SkillDisplayMode DisplayMode
    {
        get => (SkillDisplayMode)GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
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

    public bool ShowTrainingTime
    {
        get => (bool)GetValue(ShowTrainingTimeProperty);
        set => SetValue(ShowTrainingTimeProperty, value);
    }

    public bool ShowSkillTree
    {
        get => (bool)GetValue(ShowSkillTreeProperty);
        set => SetValue(ShowSkillTreeProperty, value);
    }

    public bool ShowMissingOnly
    {
        get => (bool)GetValue(ShowMissingOnlyProperty);
        set => SetValue(ShowMissingOnlyProperty, value);
    }

    public TimeSpan UpdateInterval
    {
        get => (TimeSpan)GetValue(UpdateIntervalProperty);
        set => SetValue(UpdateIntervalProperty, value);
    }

    #endregion

    #region Fields & Properties

    private readonly DispatcherTimer _animationTimer;
    private readonly DispatcherTimer _updateTimer;
    private readonly Dictionary<string, Storyboard> _skillAnimations;
    private readonly List<HoloSkillParticle> _skillParticles;
    private readonly HoloSkillCalculator _calculator;
    
    private Canvas _mainCanvas;
    private Canvas _particleCanvas;
    private Canvas _treeCanvas;
    private Grid _skillListGrid;
    private Grid _metricsGrid;
    private Grid _trainingTimeGrid;

    private readonly Dictionary<string, SkillRequirementStatus> _skillStatuses;
    private readonly Dictionary<string, TimeSpan> _trainingTimes;
    private TimeSpan _totalTrainingTime;
    private double _overallProgress;
    private bool _hasUnmetRequirements;
    private DateTime _lastUpdateTime;
    private readonly Random _random = new();

    // Animation state
    private double _animationProgress;
    private readonly Dictionary<string, double> _skillAlphas;

    #endregion

    #region Constructor & Initialization

    public HoloSkillRequirementDisplay()
    {
        _animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) }; // 60 FPS
        _updateTimer = new DispatcherTimer();
        _skillAnimations = new Dictionary<string, Storyboard>();
        _skillParticles = new List<HoloSkillParticle>();
        _calculator = new HoloSkillCalculator();
        _skillStatuses = new Dictionary<string, SkillRequirementStatus>();
        _trainingTimes = new Dictionary<string, TimeSpan>();
        _skillAlphas = new Dictionary<string, double>();

        _animationTimer.Tick += OnAnimationTimerTick;
        _updateTimer.Tick += OnUpdateTimerTick;
        _lastUpdateTime = DateTime.Now;

        InitializeComponent();
        CreateHolographicInterface();
        StartAnimations();
    }

    private void InitializeComponent()
    {
        Width = 650;
        Height = 800;
        Background = new SolidColorBrush(Color.FromArgb(25, 100, 200, 100));

        // Apply holographic effects
        Effect = new DropShadowEffect
        {
            Color = Colors.LimeGreen,
            BlurRadius = 24,
            ShadowDepth = 0,
            Opacity = 0.7
        };
    }

    private void CreateHolographicInterface()
    {
        var mainGrid = new Grid();
        Content = mainGrid;

        // Define rows for layout
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(200) });

        // Create main canvas for skill visualization
        _mainCanvas = new Canvas
        {
            Background = new SolidColorBrush(Color.FromArgb(15, 100, 255, 100)),
            ClipToBounds = true
        };
        Grid.SetRow(_mainCanvas, 0);
        mainGrid.Children.Add(_mainCanvas);

        // Create particle canvas overlay
        _particleCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false
        };
        _mainCanvas.Children.Add(_particleCanvas);

        // Create tree canvas for skill connections
        _treeCanvas = new Canvas
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false
        };
        _mainCanvas.Children.Add(_treeCanvas);

        // Create skill list display
        CreateSkillListDisplay();

        // Create skill tree
        if (ShowSkillTree)
        {
            CreateSkillTree();
        }

        // Create training time display
        if (ShowTrainingTime)
        {
            CreateTrainingTimeDisplay();
        }

        // Create metrics display
        CreateMetricsDisplay();
    }

    private void CreateSkillListDisplay()
    {
        var skillListContainer = new ScrollViewer
        {
            Width = 400,
            Height = 500,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)),
            BorderBrush = new SolidColorBrush(Colors.LimeGreen),
            BorderThickness = new Thickness(1)
        };
        Canvas.SetLeft(skillListContainer, 20);
        Canvas.SetTop(skillListContainer, 20);
        _mainCanvas.Children.Add(skillListContainer);

        _skillListGrid = new Grid();
        skillListContainer.Content = _skillListGrid;

        // Add title
        var title = new TextBlock
        {
            Text = "Skill Requirements",
            Foreground = new SolidColorBrush(Colors.LimeGreen),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 10)
        };
        _skillListGrid.Children.Add(title);
    }

    private void CreateSkillTree()
    {
        var treeContainer = new Border
        {
            Width = 200,
            Height = 300,
            Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5)
        };
        Canvas.SetRight(treeContainer, 20);
        Canvas.SetTop(treeContainer, 20);
        _mainCanvas.Children.Add(treeContainer);

        var treeCanvas = new Canvas
        {
            Name = "SkillTreeCanvas",
            Background = Brushes.Transparent
        };
        treeContainer.Child = treeCanvas;

        // Add title
        var title = new TextBlock
        {
            Text = "Skill Tree",
            Foreground = new SolidColorBrush(Colors.LimeGreen),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Canvas.SetTop(title, 5);
        Canvas.SetLeft(title, 70);
        treeCanvas.Children.Add(title);
    }

    private void CreateTrainingTimeDisplay()
    {
        var timeContainer = new Border
        {
            Width = 200,
            Height = 150,
            Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5)
        };
        Canvas.SetRight(timeContainer, 20);
        Canvas.SetTop(timeContainer, 340);
        _mainCanvas.Children.Add(timeContainer);

        _trainingTimeGrid = new Grid
        {
            Name = "TrainingTimeGrid"
        };
        timeContainer.Child = _trainingTimeGrid;

        _trainingTimeGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
        _trainingTimeGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        // Add title
        var title = new TextBlock
        {
            Text = "Training Time",
            Foreground = new SolidColorBrush(Colors.LimeGreen),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(title, 0);
        _trainingTimeGrid.Children.Add(title);

        // Training time details
        var detailsPanel = new StackPanel
        {
            Name = "TrainingDetailsPanel",
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10)
        };
        Grid.SetRow(detailsPanel, 1);
        _trainingTimeGrid.Children.Add(detailsPanel);
    }

    private void CreateMetricsDisplay()
    {
        _metricsGrid = new Grid
        {
            Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0))
        };
        Grid.SetRow(_metricsGrid, 1);
        ((Grid)Content).Children.Add(_metricsGrid);

        // Define rows and columns for metrics
        _metricsGrid.RowDefinitions.Add(new RowDefinition());
        _metricsGrid.RowDefinitions.Add(new RowDefinition());
        _metricsGrid.RowDefinitions.Add(new RowDefinition());
        
        _metricsGrid.ColumnDefinitions.Add(new ColumnDefinition());
        _metricsGrid.ColumnDefinitions.Add(new ColumnDefinition());
        _metricsGrid.ColumnDefinitions.Add(new ColumnDefinition());
        _metricsGrid.ColumnDefinitions.Add(new ColumnDefinition());

        // Create metric displays
        CreateMetricDisplay("Required", "12", Colors.LimeGreen, 0, 0);
        CreateMetricDisplay("Missing", "4", Colors.Red, 1, 0);
        CreateMetricDisplay("Training", "8", Colors.Yellow, 2, 0);
        CreateMetricDisplay("Complete", "8", Colors.Cyan, 3, 0);

        CreateMetricDisplay("Progress", "67%", Colors.Orange, 0, 1);
        CreateMetricDisplay("Total Time", "14d 6h", Colors.Pink, 1, 1);
        CreateMetricDisplay("Remaining", "5d 12h", Colors.Red, 2, 1);
        CreateMetricDisplay("SP Needed", "2.4M", Colors.White, 3, 1);

        CreateMetricDisplay("Highest", "Level V", Colors.Gold, 0, 2);
        CreateMetricDisplay("Lowest", "Level II", Colors.Gray, 1, 2);
        CreateMetricDisplay("Attributes", "Optimal", Colors.LimeGreen, 2, 2);
        CreateMetricDisplay("Status", "Training", Colors.Yellow, 3, 2);
    }

    private void CreateMetricDisplay(string label, string value, Color color, int column, int row)
    {
        var container = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5)
        };

        var labelText = new TextBlock
        {
            Text = label,
            Foreground = new SolidColorBrush(Colors.Gray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 9,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var valueText = new TextBlock
        {
            Text = value,
            Foreground = new SolidColorBrush(color),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 3,
                ShadowDepth = 0,
                Opacity = 0.8
            }
        };

        container.Children.Add(labelText);
        container.Children.Add(valueText);

        Grid.SetColumn(container, column);
        Grid.SetRow(container, row);
        _metricsGrid.Children.Add(container);
    }

    #endregion

    #region Animation Methods

    private void StartAnimations()
    {
        if (EnableAnimations)
        {
            _animationTimer.Start();
            _updateTimer.Interval = UpdateInterval;
            _updateTimer.Start();
        }
    }

    private void StopAnimations()
    {
        _animationTimer.Stop();
        _updateTimer.Stop();
    }

    private void OnAnimationTimerTick(object sender, EventArgs e)
    {
        if (!EnableAnimations)
            return;

        UpdateParticleEffects();
        UpdateSkillAnimations();
        UpdateProgressAnimations();
        UpdateTreeAnimations();
    }

    private void OnUpdateTimerTick(object sender, EventArgs e)
    {
        if (FittingRequirements != null && CharacterSkills != null)
        {
            CalculateSkillRequirements();
            UpdateSkillListDisplay();
            UpdateSkillTreeDisplay();
            UpdateTrainingTimeDisplay();
            UpdateMetricsDisplay();
        }
    }

    private void UpdateParticleEffects()
    {
        if (!EnableParticleEffects)
            return;

        // Update existing skill particles
        for (int i = _skillParticles.Count - 1; i >= 0; i--)
        {
            var particle = _skillParticles[i];
            particle.Update();

            if (particle.IsExpired)
            {
                _particleCanvas.Children.Remove(particle.Visual);
                _skillParticles.RemoveAt(i);
            }
        }

        // Create new skill particles based on training progress
        if (_hasUnmetRequirements && _random.NextDouble() < 0.25)
        {
            CreateSkillParticle();
        }
    }

    private void UpdateSkillAnimations()
    {
        _animationProgress += 0.015;
        if (_animationProgress > 1.0)
            _animationProgress = 0.0;

        // Update skill alpha oscillations based on status
        foreach (var skill in _skillStatuses.Keys.ToList())
        {
            var status = _skillStatuses[skill];
            var baseAlpha = GetStatusAlpha(status);
            var oscillation = Math.Sin(_animationProgress * 3 * Math.PI) * 0.2;
            _skillAlphas[skill] = Math.Max(0.3, baseAlpha + oscillation);
        }
    }

    private void UpdateProgressAnimations()
    {
        // Animate progress bars based on training status
        foreach (var kvp in _skillStatuses)
        {
            if (kvp.Value == SkillRequirementStatus.Training)
            {
                // Create training animation effect
                var progress = _animationProgress;
                // Update visual elements based on progress
            }
        }
    }

    private void UpdateTreeAnimations()
    {
        if (!ShowSkillTree)
            return;

        // Animate skill tree connections based on dependencies
        var treeCanvas = FindChildByName<Canvas>("SkillTreeCanvas");
        if (treeCanvas != null)
        {
            // Update connection line animations
            foreach (var line in treeCanvas.Children.OfType<Line>())
            {
                if (line.Name?.StartsWith("Connection_") == true)
                {
                    var pulseIntensity = Math.Sin(_animationProgress * 4 * Math.PI) * 0.3 + 0.7;
                    line.Opacity = pulseIntensity;
                }
            }
        }
    }

    private void CreateSkillParticle()
    {
        var particle = new HoloSkillParticle
        {
            Position = new Point(_random.NextDouble() * _mainCanvas.ActualWidth, 
                               _random.NextDouble() * _mainCanvas.ActualHeight),
            Velocity = new Vector(_random.NextDouble() * 2 - 1, _random.NextDouble() * 2 - 1),
            Color = GetRandomSkillColor(),
            Size = _random.NextDouble() * 3 + 1,
            Life = TimeSpan.FromSeconds(_random.NextDouble() * 4 + 1)
        };

        particle.CreateVisual();
        _particleCanvas.Children.Add(particle.Visual);
        _skillParticles.Add(particle);
    }

    #endregion

    #region Calculation Methods

    private void CalculateSkillRequirements()
    {
        if (FittingRequirements == null || CharacterSkills == null)
            return;

        var now = DateTime.Now;
        var deltaTime = (now - _lastUpdateTime).TotalSeconds;
        _lastUpdateTime = now;

        _skillStatuses.Clear();
        _trainingTimes.Clear();

        var totalRequirements = 0;
        var metRequirements = 0;
        var totalTrainingTime = TimeSpan.Zero;

        foreach (var requirement in FittingRequirements.SkillRequirements)
        {
            totalRequirements++;

            var characterSkill = CharacterSkills.FirstOrDefault(s => s.SkillId == requirement.SkillId);
            var currentLevel = characterSkill?.CurrentLevel ?? 0;
            
            var status = _calculator.DetermineSkillStatus(requirement, characterSkill);
            _skillStatuses[requirement.SkillId] = status;

            if (status == SkillRequirementStatus.Met)
            {
                metRequirements++;
            }
            else if (status == SkillRequirementStatus.Insufficient)
            {
                var trainingTime = _calculator.CalculateTrainingTime(requirement, characterSkill);
                _trainingTimes[requirement.SkillId] = trainingTime;
                totalTrainingTime = totalTrainingTime.Add(trainingTime);
            }
        }

        _overallProgress = totalRequirements > 0 ? (double)metRequirements / totalRequirements : 1.0;
        _totalTrainingTime = totalTrainingTime;
        _hasUnmetRequirements = metRequirements < totalRequirements;
    }

    private void UpdateSkillListDisplay()
    {
        if (FittingRequirements?.SkillRequirements == null)
            return;

        // Clear existing content (keep title)
        _skillListGrid.Children.Clear();
        _skillListGrid.RowDefinitions.Clear();

        // Add title
        _skillListGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
        var title = new TextBlock
        {
            Text = ShowMissingOnly ? "Missing Skills" : "Skill Requirements",
            Foreground = new SolidColorBrush(Colors.LimeGreen),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(title, 0);
        _skillListGrid.Children.Add(title);

        // Add skill requirements
        var skillsToShow = ShowMissingOnly ? 
            FittingRequirements.SkillRequirements.Where(r => _skillStatuses.GetValueOrDefault(r.SkillId) != SkillRequirementStatus.Met) :
            FittingRequirements.SkillRequirements;

        int rowIndex = 1;
        foreach (var requirement in skillsToShow.OrderBy(r => r.SkillName))
        {
            _skillListGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) });
            
            var skillDisplay = CreateSkillRequirementDisplay(requirement);
            Grid.SetRow(skillDisplay, rowIndex);
            _skillListGrid.Children.Add(skillDisplay);
            
            rowIndex++;
        }
    }

    private FrameworkElement CreateSkillRequirementDisplay(HoloSkillRequirement requirement)
    {
        var characterSkill = CharacterSkills?.FirstOrDefault(s => s.SkillId == requirement.SkillId);
        var status = _skillStatuses.GetValueOrDefault(requirement.SkillId, SkillRequirementStatus.Unknown);
        var trainingTime = _trainingTimes.GetValueOrDefault(requirement.SkillId, TimeSpan.Zero);

        var container = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(60, 100, 100, 100)),
            BorderBrush = new SolidColorBrush(GetStatusColor(status)),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(10, 3),
            Padding = new Thickness(10)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Skill info
        var infoPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var nameText = new TextBlock
        {
            Text = requirement.SkillName,
            Foreground = new SolidColorBrush(Colors.White),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 13,
            FontWeight = FontWeights.Bold
        };
        infoPanel.Children.Add(nameText);

        var currentLevel = characterSkill?.CurrentLevel ?? 0;
        var levelText = new TextBlock
        {
            Text = $"Required: {ConvertToRoman(requirement.RequiredLevel)} | Current: {ConvertToRoman(currentLevel)}",
            Foreground = new SolidColorBrush(Colors.Gray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 10
        };
        infoPanel.Children.Add(levelText);

        var currentSP = characterSkill?.CurrentSkillPoints ?? 0;
        var requiredSP = _calculator.GetSkillPointsForLevel(requirement.RequiredLevel);
        var spText = new TextBlock
        {
            Text = $"SP: {currentSP:N0} / {requiredSP:N0}",
            Foreground = new SolidColorBrush(Colors.LightGray),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 9
        };
        infoPanel.Children.Add(spText);

        Grid.SetColumn(infoPanel, 0);
        grid.Children.Add(infoPanel);

        // Status display
        var statusPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var statusText = new TextBlock
        {
            Text = GetStatusText(status),
            Foreground = new SolidColorBrush(GetStatusColor(status)),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        statusPanel.Children.Add(statusText);

        // Progress bar for incomplete skills
        if (status != SkillRequirementStatus.Met && currentLevel > 0)
        {
            var progress = (double)currentLevel / requirement.RequiredLevel;
            var progressBar = CreateProgressBar(progress, GetStatusColor(status));
            statusPanel.Children.Add(progressBar);
        }

        Grid.SetColumn(statusPanel, 1);
        grid.Children.Add(statusPanel);

        // Training time display
        var timePanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        if (trainingTime > TimeSpan.Zero)
        {
            var timeText = new TextBlock
            {
                Text = FormatTrainingTime(trainingTime),
                Foreground = new SolidColorBrush(Colors.Yellow),
                FontFamily = new FontFamily("Orbitron"),
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            timePanel.Children.Add(timeText);

            var timeLabel = new TextBlock
            {
                Text = "Training Time",
                Foreground = new SolidColorBrush(Colors.Gray),
                FontFamily = new FontFamily("Orbitron"),
                FontSize = 8,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            timePanel.Children.Add(timeLabel);
        }
        else if (status == SkillRequirementStatus.Met)
        {
            var completedText = new TextBlock
            {
                Text = "✓",
                Foreground = new SolidColorBrush(Colors.LimeGreen),
                FontFamily = new FontFamily("Orbitron"),
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            timePanel.Children.Add(completedText);
        }

        Grid.SetColumn(timePanel, 2);
        grid.Children.Add(timePanel);

        // Add alpha animation based on status
        if (_skillAlphas.TryGetValue(requirement.SkillId, out var alpha))
        {
            container.Opacity = alpha;
        }

        container.Child = grid;
        return container;
    }

    private FrameworkElement CreateProgressBar(double progress, Color color)
    {
        var container = new Border
        {
            Width = 80,
            Height = 8,
            Background = new SolidColorBrush(Color.FromArgb(50, 128, 128, 128)),
            BorderBrush = new SolidColorBrush(color),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Margin = new Thickness(0, 3)
        };

        var progressFill = new Rectangle
        {
            Fill = new SolidColorBrush(color),
            Width = 78 * progress,
            Height = 6,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        container.Child = progressFill;

        return container;
    }

    private void UpdateSkillTreeDisplay()
    {
        if (!ShowSkillTree)
            return;

        var treeCanvas = FindChildByName<Canvas>("SkillTreeCanvas");
        if (treeCanvas == null)
            return;

        // Clear existing skill nodes (keep title)
        var existingNodes = treeCanvas.Children.OfType<FrameworkElement>()
            .Where(e => e.Name?.StartsWith("SkillNode_") == true || e.Name?.StartsWith("Connection_") == true)
            .ToList();
        
        foreach (var node in existingNodes)
        {
            treeCanvas.Children.Remove(node);
        }

        // Create skill tree layout
        if (FittingRequirements?.SkillRequirements != null)
        {
            var skillPositions = CalculateSkillTreePositions();
            CreateSkillNodes(treeCanvas, skillPositions);
            CreateSkillConnections(treeCanvas, skillPositions);
        }
    }

    private Dictionary<string, Point> CalculateSkillTreePositions()
    {
        var positions = new Dictionary<string, Point>();
        var skills = FittingRequirements.SkillRequirements.ToList();
        
        // Simple radial layout for demonstration
        var centerX = 100.0;
        var centerY = 150.0;
        var radius = 60.0;

        for (int i = 0; i < skills.Count; i++)
        {
            var angle = (i / (double)skills.Count) * 2 * Math.PI;
            var x = centerX + Math.Cos(angle) * radius;
            var y = centerY + Math.Sin(angle) * radius;
            
            positions[skills[i].SkillId] = new Point(x, y);
        }

        return positions;
    }

    private void CreateSkillNodes(Canvas canvas, Dictionary<string, Point> positions)
    {
        foreach (var kvp in positions)
        {
            var skillId = kvp.Key;
            var position = kvp.Value;
            var status = _skillStatuses.GetValueOrDefault(skillId, SkillRequirementStatus.Unknown);

            var node = new Ellipse
            {
                Name = $"SkillNode_{skillId}",
                Width = 16,
                Height = 16,
                Fill = new SolidColorBrush(GetStatusColor(status)),
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 1,
                Effect = new DropShadowEffect
                {
                    Color = GetStatusColor(status),
                    BlurRadius = 6,
                    ShadowDepth = 0,
                    Opacity = 0.8
                }
            };

            Canvas.SetLeft(node, position.X - 8);
            Canvas.SetTop(node, position.Y - 8);
            canvas.Children.Add(node);
        }
    }

    private void CreateSkillConnections(Canvas canvas, Dictionary<string, Point> positions)
    {
        // Create connections between related skills
        var skillList = positions.Keys.ToList();
        
        for (int i = 0; i < skillList.Count - 1; i++)
        {
            var skill1 = skillList[i];
            var skill2 = skillList[i + 1];
            
            if (positions.TryGetValue(skill1, out var pos1) && positions.TryGetValue(skill2, out var pos2))
            {
                var connection = new Line
                {
                    Name = $"Connection_{skill1}_{skill2}",
                    X1 = pos1.X,
                    Y1 = pos1.Y,
                    X2 = pos2.X,
                    Y2 = pos2.Y,
                    Stroke = new SolidColorBrush(Color.FromArgb(100, 128, 128, 128)),
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 2, 2 }
                };
                
                canvas.Children.Add(connection);
            }
        }
    }

    private void UpdateTrainingTimeDisplay()
    {
        if (!ShowTrainingTime)
            return;

        var detailsPanel = FindChildByName<StackPanel>("TrainingDetailsPanel");
        if (detailsPanel == null)
            return;

        // Clear existing details
        detailsPanel.Children.Clear();

        // Total training time
        var totalTimeText = new TextBlock
        {
            Text = $"Total: {FormatTrainingTime(_totalTrainingTime)}",
            Foreground = new SolidColorBrush(Colors.White),
            FontFamily = new FontFamily("Orbitron"),
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        detailsPanel.Children.Add(totalTimeText);

        // Skill breakdown
        var skillsWithTime = _trainingTimes.Where(kvp => kvp.Value > TimeSpan.Zero)
                                          .OrderByDescending(kvp => kvp.Value)
                                          .Take(4);

        foreach (var kvp in skillsWithTime)
        {
            var requirement = FittingRequirements?.SkillRequirements.FirstOrDefault(r => r.SkillId == kvp.Key);
            if (requirement != null)
            {
                var skillTimeText = new TextBlock
                {
                    Text = $"{requirement.SkillName}: {FormatTrainingTime(kvp.Value)}",
                    Foreground = new SolidColorBrush(Colors.LightGray),
                    FontFamily = new FontFamily("Orbitron"),
                    FontSize = 9,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 2)
                };
                detailsPanel.Children.Add(skillTimeText);
            }
        }

        // Completion date
        if (_totalTrainingTime > TimeSpan.Zero)
        {
            var completionDate = DateTime.Now.Add(_totalTrainingTime);
            var completionText = new TextBlock
            {
                Text = $"Complete: {completionDate:MMM dd, yyyy}",
                Foreground = new SolidColorBrush(Colors.Yellow),
                FontFamily = new FontFamily("Orbitron"),
                FontSize = 9,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 5)
            };
            detailsPanel.Children.Add(completionText);
        }
    }

    private void UpdateMetricsDisplay()
    {
        if (FittingRequirements?.SkillRequirements == null)
            return;

        var totalSkills = FittingRequirements.SkillRequirements.Count;
        var metSkills = _skillStatuses.Count(kvp => kvp.Value == SkillRequirementStatus.Met);
        var missingSkills = _skillStatuses.Count(kvp => kvp.Value == SkillRequirementStatus.Insufficient);
        var trainingSkills = _skillStatuses.Count(kvp => kvp.Value == SkillRequirementStatus.Training);

        UpdateMetricValue("Required", totalSkills.ToString());
        UpdateMetricValue("Missing", missingSkills.ToString());
        UpdateMetricValue("Training", trainingSkills.ToString());
        UpdateMetricValue("Complete", metSkills.ToString());

        UpdateMetricValue("Progress", $"{_overallProgress * 100:F0}%");
        UpdateMetricValue("Total Time", FormatTrainingTime(_totalTrainingTime));
        
        var remainingTime = _trainingTimes.Values.Where(t => t > TimeSpan.Zero).Aggregate(TimeSpan.Zero, (acc, t) => acc.Add(t));
        UpdateMetricValue("Remaining", FormatTrainingTime(remainingTime));

        var totalSP = _trainingTimes.Keys.Sum(skillId => 
        {
            var req = FittingRequirements.SkillRequirements.FirstOrDefault(r => r.SkillId == skillId);
            return req != null ? _calculator.GetSkillPointsForLevel(req.RequiredLevel) : 0;
        });
        UpdateMetricValue("SP Needed", $"{totalSP / 1000000.0:F1}M");

        var highestLevel = FittingRequirements.SkillRequirements.Max(r => r.RequiredLevel);
        var lowestLevel = FittingRequirements.SkillRequirements.Min(r => r.RequiredLevel);
        
        UpdateMetricValue("Highest", $"Level {ConvertToRoman(highestLevel)}");
        UpdateMetricValue("Lowest", $"Level {ConvertToRoman(lowestLevel)}");
        
        var attributesOptimal = _calculator.AreAttributesOptimal(CharacterSkills, FittingRequirements);
        UpdateMetricValue("Attributes", attributesOptimal ? "Optimal" : "Suboptimal");
        UpdateMetricValue("Status", _hasUnmetRequirements ? "Training" : "Complete");

        // Update metric colors
        UpdateMetricColor("Progress", GetEfficiencyColor(_overallProgress));
        UpdateMetricColor("Status", _hasUnmetRequirements ? Colors.Yellow : Colors.LimeGreen);
        UpdateMetricColor("Missing", missingSkills > 0 ? Colors.Red : Colors.LimeGreen);
        UpdateMetricColor("Attributes", attributesOptimal ? Colors.LimeGreen : Colors.Orange);
    }

    #endregion

    #region Helper Methods

    private Color GetStatusColor(SkillRequirementStatus status)
    {
        return status switch
        {
            SkillRequirementStatus.Met => Colors.LimeGreen,
            SkillRequirementStatus.Training => Colors.Yellow,
            SkillRequirementStatus.Insufficient => Colors.Red,
            SkillRequirementStatus.Unknown => Colors.Gray,
            _ => Colors.White
        };
    }

    private string GetStatusText(SkillRequirementStatus status)
    {
        return status switch
        {
            SkillRequirementStatus.Met => "Complete",
            SkillRequirementStatus.Training => "Training",
            SkillRequirementStatus.Insufficient => "Missing",
            SkillRequirementStatus.Unknown => "Unknown",
            _ => "Error"
        };
    }

    private double GetStatusAlpha(SkillRequirementStatus status)
    {
        return status switch
        {
            SkillRequirementStatus.Met => 0.8,
            SkillRequirementStatus.Training => 0.9,
            SkillRequirementStatus.Insufficient => 1.0,
            SkillRequirementStatus.Unknown => 0.5,
            _ => 0.7
        };
    }

    private Color GetEfficiencyColor(double efficiency)
    {
        if (efficiency >= 0.95) return Colors.LimeGreen;
        if (efficiency >= 0.80) return Colors.Yellow;
        if (efficiency >= 0.60) return Colors.Orange;
        return Colors.Red;
    }

    private Color GetRandomSkillColor()
    {
        var colors = new[] { Colors.LimeGreen, Colors.Yellow, Colors.Orange, Colors.Cyan };
        return colors[_random.Next(colors.Length)];
    }

    private string ConvertToRoman(int number)
    {
        return number switch
        {
            1 => "I",
            2 => "II",
            3 => "III",
            4 => "IV",
            5 => "V",
            _ => number.ToString()
        };
    }

    private string FormatTrainingTime(TimeSpan time)
    {
        if (time.TotalDays >= 1)
            return $"{time.Days}d {time.Hours}h";
        if (time.TotalHours >= 1)
            return $"{time.Hours}h {time.Minutes}m";
        if (time.TotalMinutes >= 1)
            return $"{time.Minutes}m";
        return "< 1m";
    }

    private T FindChildByName<T>(string name) where T : FrameworkElement
    {
        return _mainCanvas.Children.OfType<T>()
            .FirstOrDefault(e => e.Name == name) ??
            _metricsGrid.Children.OfType<T>()
            .FirstOrDefault(e => e.Name == name);
    }

    private void UpdateMetricValue(string label, string value)
    {
        var container = _metricsGrid.Children.OfType<StackPanel>()
            .FirstOrDefault(sp => ((TextBlock)sp.Children[0]).Text == label);
        
        if (container?.Children[1] is TextBlock valueText)
        {
            valueText.Text = value;
        }
    }

    private void UpdateMetricColor(string label, Color color)
    {
        var container = _metricsGrid.Children.OfType<StackPanel>()
            .FirstOrDefault(sp => ((TextBlock)sp.Children[0]).Text == label);
        
        if (container?.Children[1] is TextBlock valueText)
        {
            valueText.Foreground = new SolidColorBrush(color);
            if (valueText.Effect is DropShadowEffect effect)
            {
                effect.Color = color;
            }
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets the current skill requirement analysis
    /// </summary>
    public SkillRequirementAnalysis GetSkillAnalysis()
    {
        return new SkillRequirementAnalysis
        {
            TotalRequirements = FittingRequirements?.SkillRequirements.Count ?? 0,
            MetRequirements = _skillStatuses.Count(kvp => kvp.Value == SkillRequirementStatus.Met),
            MissingRequirements = _skillStatuses.Count(kvp => kvp.Value == SkillRequirementStatus.Insufficient),
            TrainingRequirements = _skillStatuses.Count(kvp => kvp.Value == SkillRequirementStatus.Training),
            OverallProgress = _overallProgress,
            TotalTrainingTime = _totalTrainingTime,
            HasUnmetRequirements = _hasUnmetRequirements,
            SkillStatuses = new Dictionary<string, SkillRequirementStatus>(_skillStatuses),
            TrainingTimes = new Dictionary<string, TimeSpan>(_trainingTimes)
        };
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public void SetAnimationIntensity(double intensity)
    {
        HolographicIntensity = intensity;
        
        // Adjust animation speeds based on intensity
        _animationTimer.Interval = TimeSpan.FromMilliseconds(16 / Math.Max(0.1, intensity));
    }

    #endregion

    #region IAdaptiveControl Implementation

    public void EnableSimplifiedMode(bool enable)
    {
        if (enable)
        {
            EnableParticleEffects = false;
            ShowSkillTree = false;
            _animationTimer.Interval = TimeSpan.FromMilliseconds(33); // 30 FPS
        }
        else
        {
            EnableParticleEffects = true;
            ShowSkillTree = true;
            _animationTimer.Interval = TimeSpan.FromMilliseconds(16); // 60 FPS
        }
    }

    #endregion

    #region Event Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillRequirementDisplay control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillRequirementDisplay control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnFittingRequirementsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillRequirementDisplay control)
        {
            control.UpdateFittingRequirements();
        }
    }

    private static void OnCharacterSkillsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillRequirementDisplay control)
        {
            control.UpdateCharacterSkills();
        }
    }

    private static void OnDisplayModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillRequirementDisplay control)
        {
            control.UpdateDisplayMode();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillRequirementDisplay control)
        {
            if ((bool)e.NewValue)
                control.StartAnimations();
            else
                control.StopAnimations();
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillRequirementDisplay control)
        {
            control.UpdateParticleSettings();
        }
    }

    private static void OnShowTrainingTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillRequirementDisplay control)
        {
            control.UpdateTrainingTimeSettings();
        }
    }

    private static void OnShowSkillTreeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillRequirementDisplay control)
        {
            control.UpdateSkillTreeSettings();
        }
    }

    private static void OnShowMissingOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillRequirementDisplay control)
        {
            control.UpdateSkillListDisplay();
        }
    }

    private static void OnUpdateIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillRequirementDisplay control)
        {
            control._updateTimer.Interval = (TimeSpan)e.NewValue;
        }
    }

    private void UpdateHolographicEffects()
    {
        Opacity = 0.8 + (HolographicIntensity * 0.2);
        
        if (Effect is DropShadowEffect shadowEffect)
        {
            shadowEffect.Opacity = 0.5 + (HolographicIntensity * 0.4);
            shadowEffect.BlurRadius = 20 + (HolographicIntensity * 8);
        }
    }

    private void UpdateColorScheme()
    {
        // Update colors based on EVE color scheme
    }

    private void UpdateFittingRequirements()
    {
        CalculateSkillRequirements();
        UpdateSkillListDisplay();
    }

    private void UpdateCharacterSkills()
    {
        CalculateSkillRequirements();
        UpdateSkillListDisplay();
    }

    private void UpdateDisplayMode()
    {
        // Switch between different display modes
    }

    private void UpdateParticleSettings()
    {
        if (!EnableParticleEffects)
        {
            foreach (var particle in _skillParticles)
            {
                _particleCanvas.Children.Remove(particle.Visual);
            }
            _skillParticles.Clear();
        }
    }

    private void UpdateTrainingTimeSettings()
    {
        // Update training time display settings
    }

    private void UpdateSkillTreeSettings()
    {
        // Update skill tree display settings
    }

    #endregion
}

#region Supporting Classes

/// <summary>
/// Represents fitting skill requirements
/// </summary>
public class HoloFittingRequirements
{
    public List<HoloSkillRequirement> SkillRequirements { get; set; } = new();
    public string FittingName { get; set; }
    public string ShipType { get; set; }
}

/// <summary>
/// Represents a skill requirement
/// </summary>
public class HoloSkillRequirement
{
    public string SkillId { get; set; }
    public string SkillName { get; set; }
    public int RequiredLevel { get; set; }
    public string Category { get; set; }
    public List<string> Prerequisites { get; set; } = new();
}

/// <summary>
/// Represents a character's skill
/// </summary>
public class HoloCharacterSkill
{
    public string SkillId { get; set; }
    public string SkillName { get; set; }
    public int CurrentLevel { get; set; }
    public long CurrentSkillPoints { get; set; }
    public bool IsTraining { get; set; }
    public DateTime? TrainingEndTime { get; set; }
}

/// <summary>
/// Skill requirement status
/// </summary>
public enum SkillRequirementStatus
{
    Met,
    Training,
    Insufficient,
    Unknown
}

/// <summary>
/// Skill display modes
/// </summary>
public enum SkillDisplayMode
{
    Requirements,
    Training,
    Tree,
    Timeline
}

/// <summary>
/// Skill requirement analysis result
/// </summary>
public class SkillRequirementAnalysis
{
    public int TotalRequirements { get; set; }
    public int MetRequirements { get; set; }
    public int MissingRequirements { get; set; }
    public int TrainingRequirements { get; set; }
    public double OverallProgress { get; set; }
    public TimeSpan TotalTrainingTime { get; set; }
    public bool HasUnmetRequirements { get; set; }
    public Dictionary<string, SkillRequirementStatus> SkillStatuses { get; set; }
    public Dictionary<string, TimeSpan> TrainingTimes { get; set; }
}

/// <summary>
/// Skill particle for visual effects
/// </summary>
public class HoloSkillParticle
{
    public Point Position { get; set; }
    public Vector Velocity { get; set; }
    public Color Color { get; set; }
    public double Size { get; set; }
    public TimeSpan Life { get; set; }
    public DateTime CreatedTime { get; set; } = DateTime.Now;
    public FrameworkElement Visual { get; set; }

    public bool IsExpired => DateTime.Now - CreatedTime > Life;

    public void Update()
    {
        Position = new Point(Position.X + Velocity.X, Position.Y + Velocity.Y);
        
        if (Visual != null)
        {
            Canvas.SetLeft(Visual, Position.X);
            Canvas.SetTop(Visual, Position.Y);
            
            // Fade out over time
            var elapsed = DateTime.Now - CreatedTime;
            var fadeProgress = elapsed.TotalMilliseconds / Life.TotalMilliseconds;
            Visual.Opacity = Math.Max(0, 1.0 - fadeProgress);
        }
    }

    public void CreateVisual()
    {
        Visual = new Ellipse
        {
            Width = Size,
            Height = Size,
            Fill = new RadialGradientBrush
            {
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color, 0.0),
                    new GradientStop(Color.FromArgb(0, Color.R, Color.G, Color.B), 1.0)
                }
            },
            Effect = new DropShadowEffect
            {
                Color = Color,
                BlurRadius = Size * 1.8,
                ShadowDepth = 0,
                Opacity = 0.8
            }
        };
    }
}

/// <summary>
/// Skill calculations helper
/// </summary>
public class HoloSkillCalculator
{
    // EVE Online skill point requirements for each level
    private readonly int[] _skillPointRequirements = { 0, 250, 1414, 8000, 45255, 256000 };

    public SkillRequirementStatus DetermineSkillStatus(HoloSkillRequirement requirement, HoloCharacterSkill characterSkill)
    {
        if (characterSkill == null)
            return SkillRequirementStatus.Insufficient;

        if (characterSkill.CurrentLevel >= requirement.RequiredLevel)
            return SkillRequirementStatus.Met;

        if (characterSkill.IsTraining)
            return SkillRequirementStatus.Training;

        return SkillRequirementStatus.Insufficient;
    }

    public TimeSpan CalculateTrainingTime(HoloSkillRequirement requirement, HoloCharacterSkill characterSkill)
    {
        if (characterSkill == null || characterSkill.CurrentLevel >= requirement.RequiredLevel)
            return TimeSpan.Zero;

        var currentSP = characterSkill.CurrentSkillPoints;
        var requiredSP = GetSkillPointsForLevel(requirement.RequiredLevel);
        var neededSP = requiredSP - currentSP;

        if (neededSP <= 0)
            return TimeSpan.Zero;

        // Assume standard training rate (30 SP/min with optimal attributes)
        var trainingRate = 30.0; // SP per minute
        var minutes = neededSP / trainingRate;

        return TimeSpan.FromMinutes(minutes);
    }

    public long GetSkillPointsForLevel(int level)
    {
        if (level < 1 || level > 5)
            return 0;

        return _skillPointRequirements[level];
    }

    public bool AreAttributesOptimal(ObservableCollection<HoloCharacterSkill> characterSkills, HoloFittingRequirements requirements)
    {
        // Simplified check - in reality would analyze character attributes vs skill primary/secondary attributes
        return true; // Placeholder
    }
}

#endregion