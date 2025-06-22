// ==========================================================================
// HoloHelpSystem.cs - Holographic Help and Documentation System
// ==========================================================================
// Interactive help system featuring holographic tutorials, searchable docs,
// context-sensitive help, and AI-powered assistance integration.
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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Holographic help and documentation system with AI-powered assistance and interactive tutorials
/// </summary>
public class HoloHelpSystem : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloHelpSystem),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloHelpSystem),
            new PropertyMetadata(EVEColorScheme.EmeraldGreen, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty CurrentTopicProperty =
        DependencyProperty.Register(nameof(CurrentTopic), typeof(HoloHelpTopic), typeof(HoloHelpSystem),
            new PropertyMetadata(null, OnCurrentTopicChanged));

    public static readonly DependencyProperty HelpModeProperty =
        DependencyProperty.Register(nameof(HelpMode), typeof(HelpDisplayMode), typeof(HoloHelpSystem),
            new PropertyMetadata(HelpDisplayMode.Documentation, OnHelpModeChanged));

    public static readonly DependencyProperty EnableInteractiveTutorialsProperty =
        DependencyProperty.Register(nameof(EnableInteractiveTutorials), typeof(bool), typeof(HoloHelpSystem),
            new PropertyMetadata(true, OnEnableInteractiveTutorialsChanged));

    public static readonly DependencyProperty EnableAIAssistantProperty =
        DependencyProperty.Register(nameof(EnableAIAssistant), typeof(bool), typeof(HoloHelpSystem),
            new PropertyMetadata(true, OnEnableAIAssistantChanged));

    public static readonly DependencyProperty EnableHelpAnimationsProperty =
        DependencyProperty.Register(nameof(EnableHelpAnimations), typeof(bool), typeof(HoloHelpSystem),
            new PropertyMetadata(true, OnEnableHelpAnimationsChanged));

    public static readonly DependencyProperty ShowQuickHelpProperty =
        DependencyProperty.Register(nameof(ShowQuickHelp), typeof(bool), typeof(HoloHelpSystem),
            new PropertyMetadata(true));

    public static readonly DependencyProperty HelpTopicsProperty =
        DependencyProperty.Register(nameof(HelpTopics), typeof(ObservableCollection<HoloHelpTopic>), typeof(HoloHelpSystem),
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

    public HoloHelpTopic CurrentTopic
    {
        get => (HoloHelpTopic)GetValue(CurrentTopicProperty);
        set => SetValue(CurrentTopicProperty, value);
    }

    public HelpDisplayMode HelpMode
    {
        get => (HelpDisplayMode)GetValue(HelpModeProperty);
        set => SetValue(HelpModeProperty, value);
    }

    public bool EnableInteractiveTutorials
    {
        get => (bool)GetValue(EnableInteractiveTutorialsProperty);
        set => SetValue(EnableInteractiveTutorialsProperty, value);
    }

    public bool EnableAIAssistant
    {
        get => (bool)GetValue(EnableAIAssistantProperty);
        set => SetValue(EnableAIAssistantProperty, value);
    }

    public bool EnableHelpAnimations
    {
        get => (bool)GetValue(EnableHelpAnimationsProperty);
        set => SetValue(EnableHelpAnimationsProperty, value);
    }

    public bool ShowQuickHelp
    {
        get => (bool)GetValue(ShowQuickHelpProperty);
        set => SetValue(ShowQuickHelpProperty, value);
    }

    public ObservableCollection<HoloHelpTopic> HelpTopics
    {
        get => (ObservableCollection<HoloHelpTopic>)GetValue(HelpTopicsProperty);
        set => SetValue(HelpTopicsProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloHelpEventArgs> TopicSelected;
    public event EventHandler<HoloHelpEventArgs> TutorialStarted;
    public event EventHandler<HoloHelpEventArgs> TutorialCompleted;
    public event EventHandler<HoloHelpEventArgs> HelpSearched;
    public event EventHandler<HoloHelpEventArgs> AIQuestionAsked;

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode { get; private set; }

    public void SwitchToFullMode()
    {
        IsInSimplifiedMode = false;
        EnableHelpAnimations = true;
        EnableInteractiveTutorials = true;
        EnableAIAssistant = true;
        UpdateHelpAppearance();
    }

    public void SwitchToSimplifiedMode()
    {
        IsInSimplifiedMode = true;
        EnableHelpAnimations = false;
        EnableInteractiveTutorials = false;
        EnableAIAssistant = false;
        UpdateHelpAppearance();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => !_disposed && IsLoaded;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        EnableHelpAnimations = settings.EnabledFeatures.HasFlag(AnimationFeatures.BasicGlow);
        EnableInteractiveTutorials = settings.EnabledFeatures.HasFlag(AnimationFeatures.ComplexTransitions);
        
        UpdateHelpAppearance();
    }

    #endregion

    #region Fields

    private Grid _mainGrid;
    private TreeView _topicsTree;
    private TabControl _contentTabs;
    private RichTextBox _documentationViewer;
    private StackPanel _tutorialPanel;
    private StackPanel _aiAssistantPanel;
    private TextBox _searchBox;
    private TextBox _aiQuestionBox;
    private Button _askAIButton;
    private ListBox _searchResults;
    private ProgressBar _tutorialProgress;
    private Canvas _effectCanvas;
    
    private readonly Dictionary<string, HoloHelpTopic> _topicIndex = new();
    private readonly List<HelpParticle> _particles = new();
    private readonly List<string> _tutorialSteps = new();
    private DispatcherTimer _animationTimer;
    private DispatcherTimer _particleTimer;
    private DispatcherTimer _typingTimer;
    private double _animationPhase = 0;
    private double _particlePhase = 0;
    private int _currentTutorialStep = 0;
    private string _currentTypingText = "";
    private int _typingIndex = 0;
    private readonly Random _random = new();
    private bool _disposed = false;
    private bool _isPlayingTutorial = false;

    #endregion

    #region Constructor

    public HoloHelpSystem()
    {
        InitializeHelpSystem();
        SetupAnimations();
        
        // Register with animation intensity manager
        AnimationIntensityManager.Instance.RegisterTarget(this);
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        
        // Initialize collections
        HelpTopics = new ObservableCollection<HoloHelpTopic>();
        LoadDefaultHelpContent();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Search help topics and content
    /// </summary>
    public async Task<List<HoloHelpTopic>> SearchHelpAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || HelpTopics == null)
            return new List<HoloHelpTopic>();

        // Simulate search delay
        await Task.Delay(300);
        
        var results = HelpTopics
            .Where(topic => 
                topic.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                topic.Content.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                topic.Keywords.Any(k => k.Contains(query, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(topic => CalculateRelevanceScore(topic, query))
            .ToList();

        if (EnableHelpAnimations && !IsInSimplifiedMode)
        {
            SpawnSearchParticles();
        }

        HelpSearched?.Invoke(this, new HoloHelpEventArgs 
        { 
            SearchQuery = query,
            ResultCount = results.Count,
            Timestamp = DateTime.Now
        });

        return results;
    }

    /// <summary>
    /// Start an interactive tutorial
    /// </summary>
    public async Task StartTutorialAsync(string tutorialId)
    {
        if (!EnableInteractiveTutorials || _isPlayingTutorial) return;

        var tutorial = HelpTopics.FirstOrDefault(t => t.Id == tutorialId && t.Type == HelpTopicType.Tutorial);
        if (tutorial == null) return;

        _isPlayingTutorial = true;
        _currentTutorialStep = 0;
        _tutorialSteps.Clear();
        _tutorialSteps.AddRange(tutorial.TutorialSteps);

        CurrentTopic = tutorial;
        HelpMode = HelpDisplayMode.Tutorial;

        if (EnableHelpAnimations && !IsInSimplifiedMode)
        {
            SpawnTutorialParticles();
        }

        TutorialStarted?.Invoke(this, new HoloHelpEventArgs 
        { 
            Topic = tutorial,
            TutorialStep = 0,
            Timestamp = DateTime.Now
        });

        await PlayTutorialStepAsync();
    }

    /// <summary>
    /// Ask AI assistant a question
    /// </summary>
    public async Task<string> AskAIQuestionAsync(string question)
    {
        if (!EnableAIAssistant || string.IsNullOrWhiteSpace(question)) 
            return "AI Assistant is not available.";

        // Simulate AI processing
        await Task.Delay(1500);

        var response = GenerateAIResponse(question);
        
        if (EnableHelpAnimations && !IsInSimplifiedMode)
        {
            await TypeTextAnimationAsync(response);
        }

        AIQuestionAsked?.Invoke(this, new HoloHelpEventArgs 
        { 
            AIQuestion = question,
            AIResponse = response,
            Timestamp = DateTime.Now
        });

        return response;
    }

    /// <summary>
    /// Show context-sensitive help for a specific feature
    /// </summary>
    public void ShowContextHelp(string contextId)
    {
        var contextTopic = HelpTopics.FirstOrDefault(t => t.ContextId == contextId);
        if (contextTopic != null)
        {
            CurrentTopic = contextTopic;
            HelpMode = HelpDisplayMode.Documentation;
            
            if (EnableHelpAnimations && !IsInSimplifiedMode)
            {
                AnimateTopicTransition();
            }
        }
    }

    /// <summary>
    /// Complete current tutorial step
    /// </summary>
    public async Task CompleteTutorialStepAsync()
    {
        if (!_isPlayingTutorial || _currentTutorialStep >= _tutorialSteps.Count - 1)
        {
            await CompleteTutorialAsync();
            return;
        }

        _currentTutorialStep++;
        UpdateTutorialProgress();
        await PlayTutorialStepAsync();
    }

    /// <summary>
    /// Get quick help for specific topic
    /// </summary>
    public string GetQuickHelp(string topicId)
    {
        var topic = HelpTopics.FirstOrDefault(t => t.Id == topicId);
        return topic?.QuickHelp ?? "No quick help available for this topic.";
    }

    #endregion

    #region Private Methods

    private void InitializeHelpSystem()
    {
        CreateHelpInterface();
        UpdateHelpAppearance();
    }

    private void CreateHelpInterface()
    {
        // Main grid layout
        _mainGrid = new Grid();
        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(250, GridUnitType.Pixel) });
        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        // Header section
        CreateHeaderSection();
        
        // Navigation panel
        CreateNavigationPanel();
        
        // Content area
        CreateContentArea();

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
                    new GradientStop(Color.FromArgb(80, 50, 205, 50), 0.0),
                    new GradientStop(Color.FromArgb(40, 50, 205, 50), 1.0)
                }
            }
        };

        var headerGrid = new Grid();
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Title
        var titleText = new TextBlock
        {
            Text = "Help & Documentation",
            Foreground = Brushes.White,
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(titleText, 0);

        // Search box
        _searchBox = new TextBox
        {
            Width = 300,
            Height = 30,
            Margin = new Thickness(20, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Text = "Search help topics..."
        };
        _searchBox.GotFocus += (s, e) => { if (_searchBox.Text == "Search help topics...") _searchBox.Text = ""; };
        _searchBox.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(_searchBox.Text)) _searchBox.Text = "Search help topics..."; };
        _searchBox.KeyDown += OnSearchKeyDown;
        Grid.SetColumn(_searchBox, 1);

        // Mode selector
        var modeSelector = new ComboBox
        {
            Width = 120,
            Height = 30,
            VerticalAlignment = VerticalAlignment.Center
        };
        modeSelector.Items.Add(new ComboBoxItem { Content = "Documentation", Tag = HelpDisplayMode.Documentation });
        modeSelector.Items.Add(new ComboBoxItem { Content = "Tutorials", Tag = HelpDisplayMode.Tutorial });
        modeSelector.Items.Add(new ComboBoxItem { Content = "AI Assistant", Tag = HelpDisplayMode.AIAssistant });
        modeSelector.SelectedIndex = 0;
        modeSelector.SelectionChanged += OnModeSelectionChanged;
        Grid.SetColumn(modeSelector, 2);

        headerGrid.Children.Add(titleText);
        headerGrid.Children.Add(_searchBox);
        headerGrid.Children.Add(modeSelector);

        headerBorder.Child = headerGrid;
        Grid.SetColumnSpan(headerBorder, 2);
        Grid.SetRow(headerBorder, 0);
        _mainGrid.Children.Add(headerBorder);
    }

    private void CreateNavigationPanel()
    {
        var navBorder = new Border
        {
            CornerRadius = new CornerRadius(12),
            BorderThickness = new Thickness(2),
            Margin = new Thickness(10, 5, 5, 10),
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(40, 0, 30, 60), 0.0),
                    new GradientStop(Color.FromArgb(20, 0, 20, 40), 1.0)
                }
            }
        };

        var navGrid = new Grid();
        navGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        navGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        navGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var navHeader = new TextBlock
        {
            Text = "Topics",
            Foreground = Brushes.White,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(15, 10, 15, 5)
        };
        Grid.SetRow(navHeader, 0);

        _topicsTree = new TreeView
        {
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Margin = new Thickness(15, 5, 15, 5)
        };
        _topicsTree.SelectedItemChanged += OnTopicSelectionChanged;
        PopulateTopicsTree();
        Grid.SetRow(_topicsTree, 1);

        // Search results (initially hidden)
        _searchResults = new ListBox
        {
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Margin = new Thickness(15, 5, 15, 5),
            Visibility = Visibility.Collapsed,
            ItemTemplate = CreateSearchResultTemplate()
        };
        _searchResults.SelectionChanged += OnSearchResultSelected;
        Grid.SetRow(_searchResults, 1);

        var quickHelpPanel = new StackPanel
        {
            Margin = new Thickness(15, 5, 15, 10)
        };

        var quickHelpHeader = new TextBlock
        {
            Text = "Quick Help",
            Foreground = Brushes.White,
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 5)
        };

        var quickHelpText = new TextBlock
        {
            Text = "Press F1 for context help",
            Foreground = Brushes.LightGray,
            FontSize = 10,
            TextWrapping = TextWrapping.Wrap
        };

        quickHelpPanel.Children.Add(quickHelpHeader);
        quickHelpPanel.Children.Add(quickHelpText);
        Grid.SetRow(quickHelpPanel, 2);

        navGrid.Children.Add(navHeader);
        navGrid.Children.Add(_topicsTree);
        navGrid.Children.Add(_searchResults);
        navGrid.Children.Add(quickHelpPanel);

        navBorder.Child = navGrid;
        Grid.SetColumn(navBorder, 0);
        Grid.SetRow(navBorder, 1);
        _mainGrid.Children.Add(navBorder);
    }

    private void CreateContentArea()
    {
        var contentBorder = new Border
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

        _contentTabs = new TabControl
        {
            Margin = new Thickness(15),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0)
        };

        CreateDocumentationTab();
        CreateTutorialTab();
        CreateAIAssistantTab();

        // Effect canvas for particles
        var mainContentGrid = new Grid();
        mainContentGrid.Children.Add(_contentTabs);

        _effectCanvas = new Canvas
        {
            IsHitTestVisible = false,
            ClipToBounds = false
        };
        mainContentGrid.Children.Add(_effectCanvas);

        contentBorder.Child = mainContentGrid;
        Grid.SetColumn(contentBorder, 1);
        Grid.SetRow(contentBorder, 1);
        _mainGrid.Children.Add(contentBorder);
    }

    private void CreateDocumentationTab()
    {
        var docTab = new TabItem
        {
            Header = "Documentation",
            Foreground = Brushes.White
        };

        _documentationViewer = new RichTextBox
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)),
            Foreground = Brushes.White,
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 12,
            Padding = new Thickness(15),
            IsReadOnly = true,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        docTab.Content = _documentationViewer;
        _contentTabs.Items.Add(docTab);
    }

    private void CreateTutorialTab()
    {
        var tutorialTab = new TabItem
        {
            Header = "Interactive Tutorial",
            Foreground = Brushes.White
        };

        var tutorialGrid = new Grid();
        tutorialGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        tutorialGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        tutorialGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Tutorial progress
        _tutorialProgress = new ProgressBar
        {
            Height = 8,
            Margin = new Thickness(10, 5),
            Background = new SolidColorBrush(Color.FromArgb(60, 0, 0, 0)),
            Foreground = new SolidColorBrush(Color.FromRgb(50, 205, 50))
        };
        Grid.SetRow(_tutorialProgress, 0);

        // Tutorial content
        _tutorialPanel = new StackPanel
        {
            Margin = new Thickness(15)
        };

        var tutorialScrollViewer = new ScrollViewer
        {
            Content = _tutorialPanel,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        Grid.SetRow(tutorialScrollViewer, 1);

        // Tutorial controls
        var controlsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(15, 10)
        };

        var nextButton = new Button
        {
            Content = "Next Step",
            Width = 100,
            Height = 30,
            Margin = new Thickness(5),
            Style = CreateHoloButtonStyle()
        };
        nextButton.Click += async (s, e) => await CompleteTutorialStepAsync();

        var skipButton = new Button
        {
            Content = "Skip Tutorial",
            Width = 100,
            Height = 30,
            Margin = new Thickness(5),
            Style = CreateHoloButtonStyle()
        };
        skipButton.Click += async (s, e) => await CompleteTutorialAsync();

        controlsPanel.Children.Add(nextButton);
        controlsPanel.Children.Add(skipButton);
        Grid.SetRow(controlsPanel, 2);

        tutorialGrid.Children.Add(_tutorialProgress);
        tutorialGrid.Children.Add(tutorialScrollViewer);
        tutorialGrid.Children.Add(controlsPanel);

        tutorialTab.Content = tutorialGrid;
        _contentTabs.Items.Add(tutorialTab);
    }

    private void CreateAIAssistantTab()
    {
        var aiTab = new TabItem
        {
            Header = "AI Assistant",
            Foreground = Brushes.White
        };

        var aiGrid = new Grid();
        aiGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        aiGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // AI conversation panel
        _aiAssistantPanel = new StackPanel
        {
            Margin = new Thickness(15)
        };

        var aiScrollViewer = new ScrollViewer
        {
            Content = _aiAssistantPanel,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0))
        };
        Grid.SetRow(aiScrollViewer, 0);

        // Question input
        var questionGrid = new Grid();
        questionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        questionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        _aiQuestionBox = new TextBox
        {
            Height = 30,
            Margin = new Thickness(15, 10, 5, 15),
            VerticalContentAlignment = VerticalAlignment.Center,
            Text = "Ask me anything about Gideon or EVE Online..."
        };
        _aiQuestionBox.GotFocus += (s, e) => { if (_aiQuestionBox.Text == "Ask me anything about Gideon or EVE Online...") _aiQuestionBox.Text = ""; };
        _aiQuestionBox.KeyDown += OnAIQuestionKeyDown;
        Grid.SetColumn(_aiQuestionBox, 0);

        _askAIButton = new Button
        {
            Content = "Ask",
            Width = 60,
            Height = 30,
            Margin = new Thickness(5, 10, 15, 15),
            Style = CreateHoloButtonStyle()
        };
        _askAIButton.Click += OnAskAIClick;
        Grid.SetColumn(_askAIButton, 1);

        questionGrid.Children.Add(_aiQuestionBox);
        questionGrid.Children.Add(_askAIButton);
        Grid.SetRow(questionGrid, 1);

        aiGrid.Children.Add(aiScrollViewer);
        aiGrid.Children.Add(questionGrid);

        aiTab.Content = aiGrid;
        _contentTabs.Items.Add(aiTab);
    }

    private void PopulateTopicsTree()
    {
        _topicsTree.Items.Clear();
        
        var categories = HelpTopics.GroupBy(t => t.Category).OrderBy(g => g.Key);
        
        foreach (var category in categories)
        {
            var categoryNode = new TreeViewItem
            {
                Header = category.Key,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold
            };

            foreach (var topic in category.OrderBy(t => t.Title))
            {
                var topicNode = new TreeViewItem
                {
                    Header = topic.Title,
                    Tag = topic,
                    Foreground = Brushes.LightGray
                };
                categoryNode.Items.Add(topicNode);
            }

            _topicsTree.Items.Add(categoryNode);
        }
    }

    private void LoadDefaultHelpContent()
    {
        // Load sample help topics
        HelpTopics.Add(new HoloHelpTopic
        {
            Id = "getting-started",
            Title = "Getting Started with Gideon",
            Category = "Basics",
            Type = HelpTopicType.Documentation,
            Content = "Welcome to Gideon, your AI copilot for EVE Online. This guide will help you get started with the basics of ship fitting, market analysis, and character planning.",
            QuickHelp = "New to Gideon? Start here for the basics.",
            Keywords = new List<string> { "getting started", "basics", "introduction", "setup" }
        });

        HelpTopics.Add(new HoloHelpTopic
        {
            Id = "ship-fitting-tutorial",
            Title = "Ship Fitting Tutorial",
            Category = "Ship Fitting",
            Type = HelpTopicType.Tutorial,
            Content = "Learn how to create optimal ship fittings using Gideon's holographic interface.",
            QuickHelp = "Interactive tutorial for ship fitting basics.",
            Keywords = new List<string> { "ship fitting", "modules", "tutorial", "dps", "tank" },
            TutorialSteps = new List<string>
            {
                "Welcome to the Ship Fitting tutorial. Let's start by selecting a ship.",
                "Choose modules from the library and drag them to the ship slots.",
                "Monitor the real-time statistics as you add modules.",
                "Use the optimization feature to improve your fitting.",
                "Save your fitting for future use."
            }
        });

        HelpTopics.Add(new HoloHelpTopic
        {
            Id = "market-analysis",
            Title = "Market Analysis Guide",
            Category = "Market",
            Type = HelpTopicType.Documentation,
            Content = "Discover profitable trading opportunities using Gideon's advanced market analysis tools.",
            QuickHelp = "Learn to analyze market trends and find trading opportunities.",
            Keywords = new List<string> { "market", "trading", "profit", "analysis", "prices" }
        });

        HelpTopics.Add(new HoloHelpTopic
        {
            Id = "character-planning",
            Title = "Character Planning",
            Category = "Character",
            Type = HelpTopicType.Documentation,
            Content = "Optimize your character's skill development for maximum efficiency.",
            QuickHelp = "Plan your character's skill progression efficiently.",
            Keywords = new List<string> { "skills", "character", "planning", "training", "optimization" }
        });

        IndexTopics();
    }

    private void IndexTopics()
    {
        _topicIndex.Clear();
        foreach (var topic in HelpTopics)
        {
            _topicIndex[topic.Id] = topic;
        }
    }

    private DataTemplate CreateSearchResultTemplate()
    {
        var template = new DataTemplate();
        
        var border = new FrameworkElementFactory(typeof(Border));
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
        border.SetValue(Border.MarginProperty, new Thickness(2));
        border.SetValue(Border.PaddingProperty, new Thickness(8, 4));
        border.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromArgb(40, 50, 205, 50)));
        
        var stackPanel = new FrameworkElementFactory(typeof(StackPanel));
        
        var titleText = new FrameworkElementFactory(typeof(TextBlock));
        titleText.SetBinding(TextBlock.TextProperty, new Binding("Title"));
        titleText.SetValue(TextBlock.ForegroundProperty, Brushes.White);
        titleText.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        
        var categoryText = new FrameworkElementFactory(typeof(TextBlock));
        categoryText.SetBinding(TextBlock.TextProperty, new Binding("Category"));
        categoryText.SetValue(TextBlock.ForegroundProperty, Brushes.LightGray);
        categoryText.SetValue(TextBlock.FontSizeProperty, 10.0);
        
        stackPanel.AppendChild(titleText);
        stackPanel.AppendChild(categoryText);
        border.AppendChild(stackPanel);
        
        template.VisualTree = border;
        return template;
    }

    private Style CreateHoloButtonStyle()
    {
        var style = new Style(typeof(Button));
        
        style.Setters.Add(new Setter(Button.BackgroundProperty, 
            new SolidColorBrush(Color.FromArgb(100, 50, 205, 50))));
        style.Setters.Add(new Setter(Button.ForegroundProperty, Brushes.White));
        style.Setters.Add(new Setter(Button.BorderBrushProperty, 
            new SolidColorBrush(Color.FromArgb(180, 50, 205, 50))));
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

        // Typing animation timer
        _typingTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };
        _typingTimer.Tick += OnTypingTick;
    }

    private void OnAnimationTick(object sender, EventArgs e)
    {
        if (!EnableHelpAnimations || IsInSimplifiedMode) return;

        _animationPhase += 0.05;
        if (_animationPhase > Math.PI * 2)
            _animationPhase = 0;

        UpdateHelpAnimations();
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        if (IsInSimplifiedMode || _effectCanvas == null) return;

        _particlePhase += 0.1;
        if (_particlePhase > Math.PI * 2)
            _particlePhase = 0;

        UpdateParticleEffects();
    }

    private void OnTypingTick(object sender, EventArgs e)
    {
        if (_typingIndex < _currentTypingText.Length)
        {
            var currentText = _currentTypingText.Substring(0, _typingIndex + 1);
            // Update AI response text
            _typingIndex++;
        }
        else
        {
            _typingTimer.Stop();
        }
    }

    private void UpdateHelpAnimations()
    {
        // Animate help interface elements
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

    private async Task PlayTutorialStepAsync()
    {
        if (_currentTutorialStep >= _tutorialSteps.Count) return;

        var stepText = _tutorialSteps[_currentTutorialStep];
        
        // Clear tutorial panel and add step content
        _tutorialPanel.Children.Clear();
        
        var stepHeader = new TextBlock
        {
            Text = $"Step {_currentTutorialStep + 1} of {_tutorialSteps.Count}",
            Foreground = new SolidColorBrush(Color.FromRgb(50, 205, 50)),
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 10)
        };

        var stepContent = new TextBlock
        {
            Text = stepText,
            Foreground = Brushes.White,
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 20
        };

        _tutorialPanel.Children.Add(stepHeader);
        _tutorialPanel.Children.Add(stepContent);

        UpdateTutorialProgress();
        
        if (EnableHelpAnimations && !IsInSimplifiedMode)
        {
            await AnimateStepTransition();
        }
    }

    private async Task CompleteTutorialAsync()
    {
        _isPlayingTutorial = false;
        _currentTutorialStep = 0;
        
        TutorialCompleted?.Invoke(this, new HoloHelpEventArgs 
        { 
            Topic = CurrentTopic,
            Timestamp = DateTime.Now
        });

        if (EnableHelpAnimations && !IsInSimplifiedMode)
        {
            SpawnCompletionParticles();
        }
    }

    private void UpdateTutorialProgress()
    {
        if (_tutorialProgress != null && _tutorialSteps.Count > 0)
        {
            _tutorialProgress.Maximum = _tutorialSteps.Count;
            _tutorialProgress.Value = _currentTutorialStep + 1;
        }
    }

    private async Task AnimateStepTransition()
    {
        // Animate tutorial step transition
        await Task.Delay(200);
    }

    private async Task TypeTextAnimationAsync(string text)
    {
        _currentTypingText = text;
        _typingIndex = 0;
        _typingTimer.Start();
        
        while (_typingTimer.IsEnabled)
        {
            await Task.Delay(10);
        }
    }

    private void AnimateTopicTransition()
    {
        if (EnableHelpAnimations && !IsInSimplifiedMode)
        {
            SpawnTransitionParticles();
        }
    }

    private void SpawnSearchParticles()
    {
        for (int i = 0; i < 6; i++)
        {
            var particle = CreateHelpParticle();
            _particles.Add(particle);
            _effectCanvas.Children.Add(particle.Visual);
        }
    }

    private void SpawnTutorialParticles()
    {
        for (int i = 0; i < 8; i++)
        {
            var particle = CreateHelpParticle();
            _particles.Add(particle);
            _effectCanvas.Children.Add(particle.Visual);
        }
    }

    private void SpawnTransitionParticles()
    {
        for (int i = 0; i < 4; i++)
        {
            var particle = CreateHelpParticle();
            _particles.Add(particle);
            _effectCanvas.Children.Add(particle.Visual);
        }
    }

    private void SpawnCompletionParticles()
    {
        for (int i = 0; i < 12; i++)
        {
            var particle = CreateHelpParticle();
            _particles.Add(particle);
            _effectCanvas.Children.Add(particle.Visual);
        }
    }

    private HelpParticle CreateHelpParticle()
    {
        var color = GetEVEColor(EVEColorScheme);
        var size = 2 + _random.NextDouble() * 2;
        
        var ellipse = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromArgb(150, color.R, color.G, color.B))
        };

        var particle = new HelpParticle
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

    private double CalculateRelevanceScore(HoloHelpTopic topic, string query)
    {
        var score = 0.0;
        var queryLower = query.ToLowerInvariant();
        
        if (topic.Title.Contains(queryLower, StringComparison.OrdinalIgnoreCase))
            score += 10;
        
        if (topic.Content.Contains(queryLower, StringComparison.OrdinalIgnoreCase))
            score += 5;
        
        score += topic.Keywords.Count(k => k.Contains(queryLower, StringComparison.OrdinalIgnoreCase)) * 3;
        
        return score;
    }

    private string GenerateAIResponse(string question)
    {
        // Simulate AI response generation
        var responses = new[]
        {
            "Based on your question about ship fitting, I recommend starting with defensive modules in the mid slots and focusing on damage application in the high slots. Would you like me to explain specific module types?",
            "For market analysis, look for items with high volume and price volatility. The best trading opportunities often appear in major trade hubs during peak hours. I can help you identify specific items to watch.",
            "Character planning is most effective when you focus on one area at a time. I suggest prioritizing core skills before specialized ones. Would you like me to create a training plan for your goals?",
            "Gideon's holographic interface is designed to provide intuitive access to complex EVE Online data. Each module uses adaptive rendering to ensure smooth performance on your system. What specific feature would you like to learn about?"
        };
        
        return responses[_random.Next(responses.Length)];
    }

    private void UpdateDocumentation()
    {
        if (_documentationViewer == null || CurrentTopic == null) return;

        var document = new FlowDocument();
        
        // Title
        var titleParagraph = new Paragraph(new Run(CurrentTopic.Title))
        {
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White,
            Margin = new Thickness(0, 0, 0, 15)
        };
        document.Blocks.Add(titleParagraph);

        // Content
        var contentParagraph = new Paragraph(new Run(CurrentTopic.Content))
        {
            FontSize = 12,
            Foreground = Brushes.White,
            LineHeight = 20,
            TextAlignment = TextAlignment.Left
        };
        document.Blocks.Add(contentParagraph);

        _documentationViewer.Document = document;
    }

    private void UpdateHelpAppearance()
    {
        UpdateColors();
        UpdateEffects();
        
        if (CurrentTopic != null)
        {
            UpdateDocumentation();
        }
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
            _ => Color.FromRgb(50, 205, 50)
        };
    }

    private async void OnSearchKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(_searchBox.Text))
        {
            var results = await SearchHelpAsync(_searchBox.Text);
            _searchResults.ItemsSource = results;
            _searchResults.Visibility = results.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            _topicsTree.Visibility = results.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private void OnModeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem item)
        {
            HelpMode = (HelpDisplayMode)item.Tag;
        }
    }

    private void OnTopicSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is TreeViewItem item && item.Tag is HoloHelpTopic topic)
        {
            CurrentTopic = topic;
            HelpMode = HelpDisplayMode.Documentation;
            
            TopicSelected?.Invoke(this, new HoloHelpEventArgs 
            { 
                Topic = topic,
                Timestamp = DateTime.Now
            });
        }
    }

    private void OnSearchResultSelected(object sender, SelectionChangedEventArgs e)
    {
        if (_searchResults.SelectedItem is HoloHelpTopic topic)
        {
            CurrentTopic = topic;
            HelpMode = HelpDisplayMode.Documentation;
            
            // Hide search results and show topic tree
            _searchResults.Visibility = Visibility.Collapsed;
            _topicsTree.Visibility = Visibility.Visible;
        }
    }

    private async void OnAIQuestionKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(_aiQuestionBox.Text))
        {
            await ProcessAIQuestion();
        }
    }

    private async void OnAskAIClick(object sender, RoutedEventArgs e)
    {
        await ProcessAIQuestion();
    }

    private async Task ProcessAIQuestion()
    {
        var question = _aiQuestionBox.Text;
        if (string.IsNullOrWhiteSpace(question) || question == "Ask me anything about Gideon or EVE Online...")
            return;

        // Add question to conversation
        var questionBlock = new TextBlock
        {
            Text = $"You: {question}",
            Foreground = Brushes.White,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 5),
            TextWrapping = TextWrapping.Wrap
        };
        _aiAssistantPanel.Children.Add(questionBlock);

        // Clear question box
        _aiQuestionBox.Text = "";

        // Get AI response
        var response = await AskAIQuestionAsync(question);
        
        var responseBlock = new TextBlock
        {
            Text = $"Gideon AI: {response}",
            Foreground = new SolidColorBrush(Color.FromRgb(50, 205, 50)),
            Margin = new Thickness(0, 5, 0, 15),
            TextWrapping = TextWrapping.Wrap
        };
        _aiAssistantPanel.Children.Add(responseBlock);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableHelpAnimations && !IsInSimplifiedMode)
            _animationTimer.Start();
            
        _particleTimer.Start();

        PopulateTopicsTree();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        _particleTimer?.Stop();
        _typingTimer?.Stop();
        
        // Clean up particles
        _particles.Clear();
        _effectCanvas?.Children.Clear();
        
        // Unregister from animation intensity manager
        AnimationIntensityManager.Instance.UnregisterTarget(this);
        
        _disposed = true;
    }

    #endregion

    #region Event Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloHelpSystem help)
            help.UpdateHelpAppearance();
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloHelpSystem help)
            help.UpdateHelpAppearance();
    }

    private static void OnCurrentTopicChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloHelpSystem help)
            help.UpdateDocumentation();
    }

    private static void OnHelpModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloHelpSystem help && help._contentTabs != null)
        {
            var mode = (HelpDisplayMode)e.NewValue;
            help._contentTabs.SelectedIndex = (int)mode;
        }
    }

    private static void OnEnableInteractiveTutorialsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Handle tutorial enabling/disabling
    }

    private static void OnEnableAIAssistantChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Handle AI assistant enabling/disabling
    }

    private static void OnEnableHelpAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloHelpSystem help)
        {
            if ((bool)e.NewValue && !help.IsInSimplifiedMode)
                help._animationTimer.Start();
            else
                help._animationTimer.Stop();
        }
    }

    #endregion
}

/// <summary>
/// Help particle for visual effects
/// </summary>
internal class HelpParticle
{
    public FrameworkElement Visual { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
}

/// <summary>
/// Help topic information
/// </summary>
public class HoloHelpTopic
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Category { get; set; }
    public string Content { get; set; }
    public string QuickHelp { get; set; }
    public string ContextId { get; set; }
    public HelpTopicType Type { get; set; }
    public List<string> Keywords { get; set; } = new();
    public List<string> TutorialSteps { get; set; } = new();
}

/// <summary>
/// Help topic types
/// </summary>
public enum HelpTopicType
{
    Documentation,
    Tutorial,
    FAQ,
    Troubleshooting
}

/// <summary>
/// Help display modes
/// </summary>
public enum HelpDisplayMode
{
    Documentation,
    Tutorial,
    AIAssistant
}

/// <summary>
/// Event args for help system events
/// </summary>
public class HoloHelpEventArgs : EventArgs
{
    public HoloHelpTopic Topic { get; set; }
    public string SearchQuery { get; set; }
    public int ResultCount { get; set; }
    public int TutorialStep { get; set; }
    public string AIQuestion { get; set; }
    public string AIResponse { get; set; }
    public DateTime Timestamp { get; set; }
}