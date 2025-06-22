// ==========================================================================
// HoloCharacterSharing.cs - Holographic Character Sharing System
// ==========================================================================
// Advanced character sharing platform featuring secure data export,
// community integration, EVE-style character portfolios, and holographic sharing visualization.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
using Microsoft.Win32;
using System.Text.Json;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Holographic character sharing system with secure exports, community features, and portfolio management
/// </summary>
public class HoloCharacterSharing : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloCharacterSharing),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloCharacterSharing),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty SharedCharactersProperty =
        DependencyProperty.Register(nameof(SharedCharacters), typeof(ObservableCollection<HoloSharedCharacter>), typeof(HoloCharacterSharing),
            new PropertyMetadata(null, OnSharedCharactersChanged));

    public static readonly DependencyProperty CharacterPortfoliosProperty =
        DependencyProperty.Register(nameof(CharacterPortfolios), typeof(ObservableCollection<HoloCharacterPortfolio>), typeof(HoloCharacterSharing),
            new PropertyMetadata(null, OnCharacterPortfoliosChanged));

    public static readonly DependencyProperty SelectedCharacterProperty =
        DependencyProperty.Register(nameof(SelectedCharacter), typeof(string), typeof(HoloCharacterSharing),
            new PropertyMetadata("Main Character", OnSelectedCharacterChanged));

    public static readonly DependencyProperty SharingModeProperty =
        DependencyProperty.Register(nameof(SharingMode), typeof(SharingMode), typeof(HoloCharacterSharing),
            new PropertyMetadata(SharingMode.Portfolio, OnSharingModeChanged));

    public static readonly DependencyProperty PrivacyLevelProperty =
        DependencyProperty.Register(nameof(PrivacyLevel), typeof(PrivacyLevel), typeof(HoloCharacterSharing),
            new PropertyMetadata(PrivacyLevel.Public, OnPrivacyLevelChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloCharacterSharing),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloCharacterSharing),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowCommunityFeedProperty =
        DependencyProperty.Register(nameof(ShowCommunityFeed), typeof(bool), typeof(HoloCharacterSharing),
            new PropertyMetadata(true, OnShowCommunityFeedChanged));

    public static readonly DependencyProperty EnableAutoSharingProperty =
        DependencyProperty.Register(nameof(EnableAutoSharing), typeof(bool), typeof(HoloCharacterSharing),
            new PropertyMetadata(false, OnEnableAutoSharingChanged));

    public static readonly DependencyProperty ShowStatisticsProperty =
        DependencyProperty.Register(nameof(ShowStatistics), typeof(bool), typeof(HoloCharacterSharing),
            new PropertyMetadata(true, OnShowStatisticsChanged));

    public static readonly DependencyProperty EnableCommentsProperty =
        DependencyProperty.Register(nameof(EnableComments), typeof(bool), typeof(HoloCharacterSharing),
            new PropertyMetadata(true, OnEnableCommentsChanged));

    public static readonly DependencyProperty ShowRatingsProperty =
        DependencyProperty.Register(nameof(ShowRatings), typeof(bool), typeof(HoloCharacterSharing),
            new PropertyMetadata(true, OnShowRatingsChanged));

    public static readonly DependencyProperty EnableNotificationsProperty =
        DependencyProperty.Register(nameof(EnableNotifications), typeof(bool), typeof(HoloCharacterSharing),
            new PropertyMetadata(true, OnEnableNotificationsChanged));

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

    public ObservableCollection<HoloSharedCharacter> SharedCharacters
    {
        get => (ObservableCollection<HoloSharedCharacter>)GetValue(SharedCharactersProperty);
        set => SetValue(SharedCharactersProperty, value);
    }

    public ObservableCollection<HoloCharacterPortfolio> CharacterPortfolios
    {
        get => (ObservableCollection<HoloCharacterPortfolio>)GetValue(CharacterPortfoliosProperty);
        set => SetValue(CharacterPortfoliosProperty, value);
    }

    public string SelectedCharacter
    {
        get => (string)GetValue(SelectedCharacterProperty);
        set => SetValue(SelectedCharacterProperty, value);
    }

    public SharingMode SharingMode
    {
        get => (SharingMode)GetValue(SharingModeProperty);
        set => SetValue(SharingModeProperty, value);
    }

    public PrivacyLevel PrivacyLevel
    {
        get => (PrivacyLevel)GetValue(PrivacyLevelProperty);
        set => SetValue(PrivacyLevelProperty, value);
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

    public bool ShowCommunityFeed
    {
        get => (bool)GetValue(ShowCommunityFeedProperty);
        set => SetValue(ShowCommunityFeedProperty, value);
    }

    public bool EnableAutoSharing
    {
        get => (bool)GetValue(EnableAutoSharingProperty);
        set => SetValue(EnableAutoSharingProperty, value);
    }

    public bool ShowStatistics
    {
        get => (bool)GetValue(ShowStatisticsProperty);
        set => SetValue(ShowStatisticsProperty, value);
    }

    public bool EnableComments
    {
        get => (bool)GetValue(EnableCommentsProperty);
        set => SetValue(EnableCommentsProperty, value);
    }

    public bool ShowRatings
    {
        get => (bool)GetValue(ShowRatingsProperty);
        set => SetValue(ShowRatingsProperty, value);
    }

    public bool EnableNotifications
    {
        get => (bool)GetValue(EnableNotificationsProperty);
        set => SetValue(EnableNotificationsProperty, value);
    }

    #endregion

    #region Fields

    private readonly DispatcherTimer _animationTimer;
    private readonly DispatcherTimer _feedUpdateTimer;
    private readonly List<ParticleSystem> _particleSystems = new();
    private Canvas _particleCanvas;
    private Grid _mainGrid;
    private TabControl _tabControl;
    private readonly Random _random = new();

    #endregion

    #region Constructor

    public HoloCharacterSharing()
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
            Text = "HOLOGRAPHIC CHARACTER SHARING SYSTEM",
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

        var characterSelector = CreateCharacterSelector();
        var modeSelector = CreateModeSelector();
        var privacySelector = CreatePrivacySelector();
        var shareButton = CreateShareButton();

        controlsPanel.Children.Add(characterSelector);
        controlsPanel.Children.Add(modeSelector);
        controlsPanel.Children.Add(privacySelector);
        controlsPanel.Children.Add(shareButton);

        stackPanel.Children.Add(titleBlock);
        stackPanel.Children.Add(controlsPanel);

        header.Child = stackPanel;
        return header;
    }

    private ComboBox CreateCharacterSelector()
    {
        var comboBox = new ComboBox
        {
            Width = 150,
            Margin = new Thickness(0, 0, 15, 0),
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 50, 100)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 0, 200, 255))
        };

        comboBox.Items.Add("Main Character");
        comboBox.Items.Add("Alt Character");
        comboBox.Items.Add("Trading Alt");
        comboBox.SelectedIndex = 0;

        return comboBox;
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

        comboBox.Items.Add("Portfolio");
        comboBox.Items.Add("Skill Plan");
        comboBox.Items.Add("Fit Export");
        comboBox.Items.Add("Stats Only");
        comboBox.SelectedIndex = 0;

        return comboBox;
    }

    private ComboBox CreatePrivacySelector()
    {
        var comboBox = new ComboBox
        {
            Width = 100,
            Margin = new Thickness(0, 0, 15, 0),
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 50, 100)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 0, 200, 255))
        };

        comboBox.Items.Add("Public");
        comboBox.Items.Add("Friends");
        comboBox.Items.Add("Corporation");
        comboBox.Items.Add("Private");
        comboBox.SelectedIndex = 0;

        return comboBox;
    }

    private Button CreateShareButton()
    {
        var button = new Button
        {
            Content = "SHARE NOW",
            Width = 100,
            Height = 30,
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

        button.Click += OnShareNowClicked;

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

        tabControl.Items.Add(CreateMyPortfoliosTab());
        tabControl.Items.Add(CreateCommunityTab());
        tabControl.Items.Add(CreateExportsTab());
        tabControl.Items.Add(CreateSettingsTab());

        return tabControl;
    }

    private TabItem CreateMyPortfoliosTab()
    {
        var tab = new TabItem
        {
            Header = "MY PORTFOLIOS",
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

        var createNewButton = CreateNewPortfolioButton();
        stackPanel.Children.Add(createNewButton);

        var portfolios = CreatePortfolioItems();
        foreach (var portfolio in portfolios)
        {
            stackPanel.Children.Add(portfolio);
        }

        scrollViewer.Content = stackPanel;
        tab.Content = scrollViewer;
        return tab;
    }

    private TabItem CreateCommunityTab()
    {
        var tab = new TabItem
        {
            Header = "COMMUNITY",
            Background = new SolidColorBrush(Color.FromArgb(80, 0, 100, 200)),
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255))
        };

        var grid = new Grid
        {
            Margin = new Thickness(20)
        };

        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var feedScrollViewer = new ScrollViewer
        {
            Background = Brushes.Transparent,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Margin = new Thickness(0, 0, 10, 0)
        };

        var feedPanel = new StackPanel();
        var feedItems = CreateCommunityFeedItems();
        foreach (var item in feedItems)
        {
            feedPanel.Children.Add(item);
        }

        feedScrollViewer.Content = feedPanel;
        Grid.SetColumn(feedScrollViewer, 0);

        var sidebarPanel = CreateCommunitySidebar();
        Grid.SetColumn(sidebarPanel, 1);

        grid.Children.Add(feedScrollViewer);
        grid.Children.Add(sidebarPanel);

        tab.Content = grid;
        return tab;
    }

    private TabItem CreateExportsTab()
    {
        var tab = new TabItem
        {
            Header = "EXPORTS",
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

        var exportOptions = CreateExportOptions();
        foreach (var option in exportOptions)
        {
            stackPanel.Children.Add(option);
        }

        scrollViewer.Content = stackPanel;
        tab.Content = scrollViewer;
        return tab;
    }

    private TabItem CreateSettingsTab()
    {
        var tab = new TabItem
        {
            Header = "SETTINGS",
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

        var settingsGroups = CreateSharingSettingsGroups();
        foreach (var group in settingsGroups)
        {
            stackPanel.Children.Add(group);
        }

        scrollViewer.Content = stackPanel;
        tab.Content = scrollViewer;
        return tab;
    }

    private Button CreateNewPortfolioButton()
    {
        var button = new Button
        {
            Content = "CREATE NEW PORTFOLIO",
            Height = 50,
            Background = new LinearGradientBrush(
                Color.FromArgb(100, 0, 200, 100),
                Color.FromArgb(50, 0, 150, 50),
                90),
            Foreground = new SolidColorBrush(Colors.White),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 0, 255, 100)),
            BorderThickness = new Thickness(2),
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 20),
            CornerRadius = new CornerRadius(8),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(100, 0, 200, 100),
                ShadowDepth = 0,
                BlurRadius = 15
            }
        };

        button.Click += OnCreateNewPortfolioClicked;

        return button;
    }

    private List<Border> CreatePortfolioItems()
    {
        var items = new List<Border>();
        var portfolios = new[]
        {
            new { 
                Name = "Combat Specialist Portfolio", 
                Character = "Main Character", 
                Views = 1247, 
                Likes = 89, 
                Comments = 23,
                LastUpdated = "2 days ago",
                Privacy = "Public",
                Featured = true
            },
            new { 
                Name = "Industry Master Build", 
                Character = "Alt Character", 
                Views = 856, 
                Likes = 156, 
                Comments = 45,
                LastUpdated = "1 week ago",
                Privacy = "Corporation",
                Featured = false
            },
            new { 
                Name = "Perfect Trader Setup", 
                Character = "Trading Alt", 
                Views = 2103, 
                Likes = 234, 
                Comments = 67,
                LastUpdated = "3 days ago",
                Privacy = "Public",
                Featured = true
            }
        };

        foreach (var portfolio in portfolios)
        {
            var item = CreatePortfolioItem(portfolio.Name, portfolio.Character, portfolio.Views, 
                portfolio.Likes, portfolio.Comments, portfolio.LastUpdated, portfolio.Privacy, portfolio.Featured);
            items.Add(item);
        }

        return items;
    }

    private Border CreatePortfolioItem(string name, string character, int views, int likes, int comments, 
        string lastUpdated, string privacy, bool featured)
    {
        var featuredColor = featured ? Colors.Gold : Colors.Transparent;

        var border = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromArgb(60, 0, 100, 200),
                Color.FromArgb(30, 0, 50, 100),
                90),
            BorderBrush = new SolidColorBrush(featured ? featuredColor : Color.FromArgb(150, 0, 200, 255)),
            BorderThickness = new Thickness(featured ? 3 : 1),
            Margin = new Thickness(0, 0, 0, 15),
            Padding = new Thickness(20, 15),
            CornerRadius = new CornerRadius(8),
            Effect = featured ? new DropShadowEffect
            {
                Color = Color.FromArgb(150, featuredColor.R, featuredColor.G, featuredColor.B),
                ShadowDepth = 0,
                BlurRadius = 20
            } : new DropShadowEffect
            {
                Color = Color.FromArgb(80, 0, 150, 255),
                ShadowDepth = 0,
                BlurRadius = 10
            }
        };

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var headerGrid = new Grid();
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var namePanel = new StackPanel();
        var nameText = new TextBlock
        {
            Text = name,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(featured ? featuredColor : Color.FromArgb(255, 0, 255, 255))
        };

        var characterText = new TextBlock
        {
            Text = character,
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 0, 255, 150))
        };

        namePanel.Children.Add(nameText);
        namePanel.Children.Add(characterText);
        Grid.SetColumn(namePanel, 0);

        var privacyText = new TextBlock
        {
            Text = privacy.ToUpper(),
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(GetPrivacyColor(privacy)),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(privacyText, 1);

        headerGrid.Children.Add(namePanel);
        headerGrid.Children.Add(privacyText);
        Grid.SetRow(headerGrid, 0);

        var statsGrid = new Grid
        {
            Margin = new Thickness(0, 10, 0, 0)
        };
        statsGrid.ColumnDefinitions.Add(new ColumnDefinition());
        statsGrid.ColumnDefinitions.Add(new ColumnDefinition());
        statsGrid.ColumnDefinitions.Add(new ColumnDefinition());

        var viewsText = new TextBlock
        {
            Text = $"👁 {views:N0}",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255))
        };
        Grid.SetColumn(viewsText, 0);

        var likesText = new TextBlock
        {
            Text = $"❤ {likes}",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 100, 100)),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetColumn(likesText, 1);

        var commentsText = new TextBlock
        {
            Text = $"💬 {comments}",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 100, 255, 100)),
            HorizontalAlignment = HorizontalAlignment.Right
        };
        Grid.SetColumn(commentsText, 2);

        statsGrid.Children.Add(viewsText);
        statsGrid.Children.Add(likesText);
        statsGrid.Children.Add(commentsText);
        Grid.SetRow(statsGrid, 1);

        var footerText = new TextBlock
        {
            Text = $"Last updated: {lastUpdated}",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(150, 255, 255, 255)),
            Margin = new Thickness(0, 8, 0, 0)
        };
        Grid.SetRow(footerText, 2);

        grid.Children.Add(headerGrid);
        grid.Children.Add(statsGrid);
        grid.Children.Add(footerText);

        border.Child = grid;

        border.MouseEnter += (s, e) =>
        {
            var scaleTransform = new ScaleTransform(1.02, 1.02);
            border.RenderTransform = scaleTransform;
            border.RenderTransformOrigin = new Point(0.5, 0.5);
        };

        border.MouseLeave += (s, e) =>
        {
            border.RenderTransform = null;
        };

        return border;
    }

    private List<Border> CreateCommunityFeedItems()
    {
        var items = new List<Border>();
        var feedData = new[]
        {
            new { User = "CapsuleerPro", Action = "shared a new combat fit", Item = "Rattlesnake PvE Build", Time = "2 hours ago", Likes = 45 },
            new { User = "IndustryKing", Action = "updated portfolio", Item = "Perfect Manufacturer Setup", Time = "4 hours ago", Likes = 23 },
            new { User = "PvPAce", Action = "exported skill plan", Item = "Solo PvP Mastery", Time = "6 hours ago", Likes = 89 },
            new { User = "TraderElite", Action = "shared market analysis", Item = "Jita Trading Guide", Time = "8 hours ago", Likes = 156 },
            new { User = "ExplorerX", Action = "published new portfolio", Item = "Wormhole Explorer Build", Time = "12 hours ago", Likes = 67 }
        };

        foreach (var feed in feedData)
        {
            var item = CreateFeedItem(feed.User, feed.Action, feed.Item, feed.Time, feed.Likes);
            items.Add(item);
        }

        return items;
    }

    private Border CreateFeedItem(string user, string action, string item, string time, int likes)
    {
        var border = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromArgb(40, 0, 50, 100),
                Color.FromArgb(20, 0, 25, 50),
                90),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 150, 255)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(15, 12),
            CornerRadius = new CornerRadius(5)
        };

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var contentPanel = new StackPanel();

        var actionText = new TextBlock
        {
            Text = $"{user} {action}",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255)),
            Margin = new Thickness(0, 0, 0, 3)
        };

        var itemText = new TextBlock
        {
            Text = $"📋 {item}",
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 150)),
            Margin = new Thickness(0, 0, 0, 5)
        };

        contentPanel.Children.Add(actionText);
        contentPanel.Children.Add(itemText);
        Grid.SetRow(contentPanel, 0);

        var footerGrid = new Grid();
        footerGrid.ColumnDefinitions.Add(new ColumnDefinition());
        footerGrid.ColumnDefinitions.Add(new ColumnDefinition());

        var timeText = new TextBlock
        {
            Text = time,
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(150, 255, 255, 255))
        };
        Grid.SetColumn(timeText, 0);

        var likesText = new TextBlock
        {
            Text = $"❤ {likes}",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 100, 100)),
            HorizontalAlignment = HorizontalAlignment.Right
        };
        Grid.SetColumn(likesText, 1);

        footerGrid.Children.Add(timeText);
        footerGrid.Children.Add(likesText);
        Grid.SetRow(footerGrid, 1);

        grid.Children.Add(contentPanel);
        grid.Children.Add(footerGrid);

        border.Child = grid;
        return border;
    }

    private StackPanel CreateCommunitySidebar()
    {
        var panel = new StackPanel
        {
            Margin = new Thickness(10, 0, 0, 0)
        };

        var trendingBox = CreateTrendingBox();
        var featuredBox = CreateFeaturedBox();
        var leaderboardBox = CreateLeaderboardBox();

        panel.Children.Add(trendingBox);
        panel.Children.Add(featuredBox);
        panel.Children.Add(leaderboardBox);

        return panel;
    }

    private Border CreateTrendingBox()
    {
        var border = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromArgb(60, 0, 100, 200),
                Color.FromArgb(30, 0, 50, 100),
                90),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 0, 200, 255)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 0, 15),
            Padding = new Thickness(15),
            CornerRadius = new CornerRadius(8)
        };

        var stackPanel = new StackPanel();

        var title = new TextBlock
        {
            Text = "🔥 TRENDING",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 150, 0)),
            Margin = new Thickness(0, 0, 0, 10)
        };

        var trending = new[]
        {
            "#Battleship Builds",
            "#Industry Optimization",
            "#PvP Fittings",
            "#Exploration Setups",
            "#Trading Strategies"
        };

        stackPanel.Children.Add(title);

        foreach (var trend in trending)
        {
            var trendText = new TextBlock
            {
                Text = trend,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 0, 255, 150)),
                Margin = new Thickness(0, 0, 0, 3),
                Cursor = Cursors.Hand
            };
            stackPanel.Children.Add(trendText);
        }

        border.Child = stackPanel;
        return border;
    }

    private Border CreateFeaturedBox()
    {
        var border = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromArgb(60, 0, 100, 200),
                Color.FromArgb(30, 0, 50, 100),
                90),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 255, 215, 0)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 0, 15),
            Padding = new Thickness(15),
            CornerRadius = new CornerRadius(8)
        };

        var stackPanel = new StackPanel();

        var title = new TextBlock
        {
            Text = "⭐ FEATURED",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 215, 0)),
            Margin = new Thickness(0, 0, 0, 10)
        };

        var featuredText = new TextBlock
        {
            Text = "Perfect Titan Pilot\nby FleetCommander",
            FontSize = 11,
            Foreground = new SolidColorBrush(Colors.White),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 5)
        };

        var viewsText = new TextBlock
        {
            Text = "👁 15,247 views",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255))
        };

        stackPanel.Children.Add(title);
        stackPanel.Children.Add(featuredText);
        stackPanel.Children.Add(viewsText);

        border.Child = stackPanel;
        return border;
    }

    private Border CreateLeaderboardBox()
    {
        var border = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromArgb(60, 0, 100, 200),
                Color.FromArgb(30, 0, 50, 100),
                90),
            BorderBrush = new SolidColorBrush(Color.FromArgb(150, 0, 200, 255)),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(15),
            CornerRadius = new CornerRadius(8)
        };

        var stackPanel = new StackPanel();

        var title = new TextBlock
        {
            Text = "🏆 TOP CREATORS",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 215, 0)),
            Margin = new Thickness(0, 0, 0, 10)
        };

        var creators = new[]
        {
            new { Rank = 1, Name = "CapsuleerPro", Score = 2847 },
            new { Rank = 2, Name = "FleetCommander", Score = 2156 },
            new { Rank = 3, Name = "IndustryKing", Score = 1923 }
        };

        stackPanel.Children.Add(title);

        foreach (var creator in creators)
        {
            var creatorPanel = new Grid
            {
                Margin = new Thickness(0, 0, 0, 5)
            };
            creatorPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
            creatorPanel.ColumnDefinitions.Add(new ColumnDefinition());
            creatorPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var rankText = new TextBlock
            {
                Text = $"{creator.Rank}.",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(GetRankColor(creator.Rank))
            };
            Grid.SetColumn(rankText, 0);

            var nameText = new TextBlock
            {
                Text = creator.Name,
                FontSize = 10,
                Foreground = new SolidColorBrush(Colors.White)
            };
            Grid.SetColumn(nameText, 1);

            var scoreText = new TextBlock
            {
                Text = creator.Score.ToString("N0"),
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 100))
            };
            Grid.SetColumn(scoreText, 2);

            creatorPanel.Children.Add(rankText);
            creatorPanel.Children.Add(nameText);
            creatorPanel.Children.Add(scoreText);

            stackPanel.Children.Add(creatorPanel);
        }

        border.Child = stackPanel;
        return border;
    }

    private List<Border> CreateExportOptions()
    {
        var options = new List<Border>();
        var exportTypes = new[]
        {
            new { 
                Title = "EVE Fitting Manager (EFT) Format", 
                Description = "Export fittings in standard EFT format for use with other tools.",
                Icon = "🚀",
                Supported = true
            },
            new { 
                Title = "PyFA Compatible Export", 
                Description = "Export character and fitting data for PyFA fitting tool.",
                Icon = "🔧",
                Supported = true
            },
            new { 
                Title = "ESI API Data Dump", 
                Description = "Complete character data in JSON format via ESI API.",
                Icon = "📊",
                Supported = true
            },
            new { 
                Title = "Character Portrait Package", 
                Description = "High-resolution character portraits and basic stats.",
                Icon = "👤",
                Supported = true
            },
            new { 
                Title = "Skill Plan XML", 
                Description = "Export skill training plans in XML format.",
                Icon = "📋",
                Supported = true
            },
            new { 
                Title = "Corporation Roster", 
                Description = "Export corporation member data and roles.",
                Icon = "🏢",
                Supported = false
            }
        };

        foreach (var export in exportTypes)
        {
            var option = CreateExportOption(export.Title, export.Description, export.Icon, export.Supported);
            options.Add(option);
        }

        return options;
    }

    private Border CreateExportOption(string title, string description, string icon, bool supported)
    {
        var border = new Border
        {
            Background = new LinearGradientBrush(
                Color.FromArgb(60, 0, 100, 200),
                Color.FromArgb(30, 0, 50, 100),
                90),
            BorderBrush = new SolidColorBrush(supported ? Color.FromArgb(150, 0, 200, 255) : Color.FromArgb(100, 100, 100, 100)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 0, 15),
            Padding = new Thickness(20, 15),
            CornerRadius = new CornerRadius(8),
            Opacity = supported ? 1.0 : 0.6
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

        var iconText = new TextBlock
        {
            Text = icon,
            FontSize = 24,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(iconText, 0);

        var contentPanel = new StackPanel
        {
            Margin = new Thickness(15, 0, 0, 0)
        };

        var titleText = new TextBlock
        {
            Text = title,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(supported ? Color.FromArgb(255, 0, 255, 255) : Color.FromArgb(150, 150, 150, 150)),
            Margin = new Thickness(0, 0, 0, 5)
        };

        var descriptionText = new TextBlock
        {
            Text = description,
            FontSize = 11,
            Foreground = new SolidColorBrush(supported ? Color.FromArgb(200, 255, 255, 255) : Color.FromArgb(120, 150, 150, 150)),
            TextWrapping = TextWrapping.Wrap
        };

        contentPanel.Children.Add(titleText);
        contentPanel.Children.Add(descriptionText);
        Grid.SetColumn(contentPanel, 1);

        var exportButton = new Button
        {
            Content = supported ? "EXPORT" : "SOON",
            Width = 80,
            Height = 30,
            Background = new LinearGradientBrush(
                supported ? Color.FromArgb(150, 0, 200, 100) : Color.FromArgb(100, 100, 100, 100),
                supported ? Color.FromArgb(100, 0, 150, 50) : Color.FromArgb(50, 50, 50, 50),
                90),
            Foreground = new SolidColorBrush(supported ? Colors.White : Colors.Gray),
            BorderBrush = new SolidColorBrush(supported ? Color.FromArgb(200, 0, 255, 100) : Colors.Gray),
            BorderThickness = new Thickness(1),
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            IsEnabled = supported
        };
        Grid.SetColumn(exportButton, 2);

        grid.Children.Add(iconText);
        grid.Children.Add(contentPanel);
        grid.Children.Add(exportButton);

        border.Child = grid;

        if (supported)
        {
            border.MouseEnter += (s, e) =>
            {
                var scaleTransform = new ScaleTransform(1.02, 1.02);
                border.RenderTransform = scaleTransform;
                border.RenderTransformOrigin = new Point(0.5, 0.5);
            };

            border.MouseLeave += (s, e) =>
            {
                border.RenderTransform = null;
            };
        }

        return border;
    }

    private List<Border> CreateSharingSettingsGroups()
    {
        var groups = new List<Border>();

        // Privacy Settings
        var privacyGroup = CreateSharingSettingsGroup("PRIVACY & VISIBILITY",
            new[]
            {
                CreateCheckBoxSetting("Allow Public Sharing", true),
                CreateCheckBoxSetting("Show in Community Feed", true),
                CreateCheckBoxSetting("Allow Comments", true),
                CreateCheckBoxSetting("Allow Ratings", true)
            });
        groups.Add(privacyGroup);

        // Notification Settings
        var notificationGroup = CreateSharingSettingsGroup("NOTIFICATIONS",
            new[]
            {
                CreateCheckBoxSetting("Notify on Comments", true),
                CreateCheckBoxSetting("Notify on Likes", false),
                CreateCheckBoxSetting("Notify on Shares", true),
                CreateCheckBoxSetting("Weekly Activity Summary", true)
            });
        groups.Add(notificationGroup);

        // Auto-Sharing Settings
        var autoGroup = CreateSharingSettingsGroup("AUTO-SHARING",
            new[]
            {
                CreateCheckBoxSetting("Auto-Share Skill Completions", false),
                CreateCheckBoxSetting("Auto-Share New Fits", false),
                CreateCheckBoxSetting("Auto-Update Portfolios", true),
                CreateCheckBoxSetting("Share Achievement Milestones", true)
            });
        groups.Add(autoGroup);

        return groups;
    }

    private Border CreateSharingSettingsGroup(string title, UIElement[] settings)
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
            Padding = new Thickness(20, 15),
            CornerRadius = new CornerRadius(8)
        };

        var stackPanel = new StackPanel();

        var titleText = new TextBlock
        {
            Text = title,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            Margin = new Thickness(0, 0, 0, 15)
        };

        stackPanel.Children.Add(titleText);

        foreach (var setting in settings)
        {
            stackPanel.Children.Add(setting);
        }

        border.Child = stackPanel;
        return border;
    }

    private Grid CreateCheckBoxSetting(string label, bool defaultValue)
    {
        var grid = new Grid
        {
            Margin = new Thickness(0, 0, 0, 10)
        };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var labelText = new TextBlock
        {
            Text = label,
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255)),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(labelText, 0);

        var checkBox = new CheckBox
        {
            IsChecked = defaultValue,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(checkBox, 1);

        grid.Children.Add(labelText);
        grid.Children.Add(checkBox);

        return grid;
    }

    private void InitializeTimers()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };
        _animationTimer.Tick += OnAnimationTick;

        _feedUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(5)
        };
        _feedUpdateTimer.Tick += OnFeedUpdateTick;
    }

    private void InitializeDefaults()
    {
        SharedCharacters = new ObservableCollection<HoloSharedCharacter>();
        CharacterPortfolios = new ObservableCollection<HoloCharacterPortfolio>();
    }

    #endregion

    #region Animation Methods

    private void StartSharingAnimations()
    {
        if (!EnableAnimations) return;

        // Add sharing process animations
    }

    #endregion

    #region Particle System

    private void CreateParticleSystem()
    {
        if (!EnableParticleEffects) return;

        var shareParticles = new ParticleSystem(_particleCanvas)
        {
            ParticleColor = Color.FromArgb(150, 0, 255, 150),
            ParticleSize = 3,
            ParticleSpeed = 1.0,
            EmissionRate = 2,
            ParticleLifespan = TimeSpan.FromSeconds(4)
        };

        var communityParticles = new ParticleSystem(_particleCanvas)
        {
            ParticleColor = Color.FromArgb(150, 255, 215, 0),
            ParticleSize = 2,
            ParticleSpeed = 1.5,
            EmissionRate = 3,
            ParticleLifespan = TimeSpan.FromSeconds(3)
        };

        _particleSystems.Add(shareParticles);
        _particleSystems.Add(communityParticles);
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

    private Color GetPrivacyColor(string privacy)
    {
        return privacy switch
        {
            "Public" => Colors.Green,
            "Friends" => Colors.Blue,
            "Corporation" => Colors.Orange,
            "Private" => Colors.Red,
            _ => Colors.Gray
        };
    }

    private Color GetRankColor(int rank)
    {
        return rank switch
        {
            1 => Colors.Gold,
            2 => Colors.Silver,
            3 => Color.FromRgb(205, 127, 50), // Bronze
            _ => Colors.Gray
        };
    }

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

        if (ShowCommunityFeed)
        {
            _feedUpdateTimer.Start();
        }

        CreateParticleSystem();
        StartSharingAnimations();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        _feedUpdateTimer?.Stop();

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

    private void OnFeedUpdateTick(object sender, EventArgs e)
    {
        UpdateCommunityFeed();
    }

    private void OnShareNowClicked(object sender, RoutedEventArgs e)
    {
        PerformShare();
    }

    private void OnCreateNewPortfolioClicked(object sender, RoutedEventArgs e)
    {
        // Show create portfolio dialog
    }

    private void PerformShare()
    {
        // Perform sharing operation
    }

    private void UpdateCommunityFeed()
    {
        // Update community feed data
    }

    #endregion

    #region Property Changed Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSharing control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSharing control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnSharedCharactersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSharing control)
        {
            control.RefreshSharedCharacters();
        }
    }

    private static void OnCharacterPortfoliosChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSharing control)
        {
            control.RefreshPortfolios();
        }
    }

    private static void OnSelectedCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSharing control)
        {
            control.LoadCharacterShares();
        }
    }

    private static void OnSharingModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSharing control)
        {
            control.UpdateSharingMode();
        }
    }

    private static void OnPrivacyLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSharing control)
        {
            control.UpdatePrivacySettings();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSharing control)
        {
            if ((bool)e.NewValue)
            {
                control._animationTimer?.Start();
                control.StartSharingAnimations();
            }
            else
            {
                control._animationTimer?.Stop();
            }
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSharing control)
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

    private static void OnShowCommunityFeedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSharing control)
        {
            if ((bool)e.NewValue)
            {
                control._feedUpdateTimer?.Start();
            }
            else
            {
                control._feedUpdateTimer?.Stop();
            }
            control.UpdateCommunityFeedDisplay();
        }
    }

    private static void OnEnableAutoSharingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSharing control)
        {
            control.UpdateAutoSharingSettings();
        }
    }

    private static void OnShowStatisticsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSharing control)
        {
            control.UpdateStatisticsDisplay();
        }
    }

    private static void OnEnableCommentsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSharing control)
        {
            control.UpdateCommentsSettings();
        }
    }

    private static void OnShowRatingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSharing control)
        {
            control.UpdateRatingsDisplay();
        }
    }

    private static void OnEnableNotificationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSharing control)
        {
            control.UpdateNotificationSettings();
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

    private void RefreshSharedCharacters()
    {
        // Refresh shared characters display
    }

    private void RefreshPortfolios()
    {
        // Refresh portfolios display
    }

    private void LoadCharacterShares()
    {
        // Load shares for selected character
    }

    private void UpdateSharingMode()
    {
        // Update sharing mode settings
    }

    private void UpdatePrivacySettings()
    {
        // Update privacy settings
    }

    private void UpdateCommunityFeedDisplay()
    {
        // Update community feed display
    }

    private void UpdateAutoSharingSettings()
    {
        // Update auto-sharing settings
    }

    private void UpdateStatisticsDisplay()
    {
        // Update statistics display
    }

    private void UpdateCommentsSettings()
    {
        // Update comments settings
    }

    private void UpdateRatingsDisplay()
    {
        // Update ratings display
    }

    private void UpdateNotificationSettings()
    {
        // Update notification settings
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

public class HoloSharedCharacter
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CharacterName { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public DateTime SharedDate { get; set; }
    public SharingMode SharingMode { get; set; }
    public PrivacyLevel PrivacyLevel { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public List<string> Tags { get; set; } = new();
    public string Description { get; set; } = string.Empty;
}

public class HoloCharacterPortfolio
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string CharacterName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime LastUpdated { get; set; }
    public PrivacyLevel PrivacyLevel { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsFeatured { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<HoloPortfolioSection> Sections { get; set; } = new();
}

public class HoloPortfolioSection
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public PortfolioSectionType Type { get; set; }
    public int Order { get; set; }
    public bool IsVisible { get; set; } = true;
}

public enum SharingMode
{
    Portfolio,
    SkillPlan,
    FitExport,
    StatsOnly,
    Complete
}

public enum PrivacyLevel
{
    Public,
    Friends,
    Corporation,
    Alliance,
    Private
}

public enum PortfolioSectionType
{
    Overview,
    Skills,
    Assets,
    Achievements,
    Fittings,
    Goals,
    Statistics,
    Custom
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