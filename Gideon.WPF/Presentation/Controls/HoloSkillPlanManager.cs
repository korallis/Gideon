// ==========================================================================
// HoloSkillPlanManager.cs - Holographic Skill Plan Export/Import Manager
// ==========================================================================
// Advanced skill plan management system featuring import/export functionality,
// plan sharing, EVE-style plan templates, and holographic plan visualization.
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
/// Holographic skill plan manager with import/export functionality and plan sharing capabilities
/// </summary>
public class HoloSkillPlanManager : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloSkillPlanManager),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloSkillPlanManager),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty SkillPlansProperty =
        DependencyProperty.Register(nameof(SkillPlans), typeof(ObservableCollection<HoloSkillPlan>), typeof(HoloSkillPlanManager),
            new PropertyMetadata(null, OnSkillPlansChanged));

    public static readonly DependencyProperty PlanTemplatesProperty =
        DependencyProperty.Register(nameof(PlanTemplates), typeof(ObservableCollection<HoloSkillPlanTemplate>), typeof(HoloSkillPlanManager),
            new PropertyMetadata(null, OnPlanTemplatesChanged));

    public static readonly DependencyProperty SelectedCharacterProperty =
        DependencyProperty.Register(nameof(SelectedCharacter), typeof(string), typeof(HoloSkillPlanManager),
            new PropertyMetadata("Main Character", OnSelectedCharacterChanged));

    public static readonly DependencyProperty PlanManagerModeProperty =
        DependencyProperty.Register(nameof(PlanManagerMode), typeof(PlanManagerMode), typeof(HoloSkillPlanManager),
            new PropertyMetadata(PlanManagerMode.MyPlans, OnPlanManagerModeChanged));

    public static readonly DependencyProperty EnableAnimationsProperty =
        DependencyProperty.Register(nameof(EnableAnimations), typeof(bool), typeof(HoloSkillPlanManager),
            new PropertyMetadata(true, OnEnableAnimationsChanged));

    public static readonly DependencyProperty EnableParticleEffectsProperty =
        DependencyProperty.Register(nameof(EnableParticleEffects), typeof(bool), typeof(HoloSkillPlanManager),
            new PropertyMetadata(true, OnEnableParticleEffectsChanged));

    public static readonly DependencyProperty ShowImportWizardProperty =
        DependencyProperty.Register(nameof(ShowImportWizard), typeof(bool), typeof(HoloSkillPlanManager),
            new PropertyMetadata(false, OnShowImportWizardChanged));

    public static readonly DependencyProperty ShowExportOptionsProperty =
        DependencyProperty.Register(nameof(ShowExportOptions), typeof(bool), typeof(HoloSkillPlanManager),
            new PropertyMetadata(false, OnShowExportOptionsChanged));

    public static readonly DependencyProperty ShowSharingOptionsProperty =
        DependencyProperty.Register(nameof(ShowSharingOptions), typeof(bool), typeof(HoloSkillPlanManager),
            new PropertyMetadata(false, OnShowSharingOptionsChanged));

    public static readonly DependencyProperty AutoBackupProperty =
        DependencyProperty.Register(nameof(AutoBackup), typeof(bool), typeof(HoloSkillPlanManager),
            new PropertyMetadata(true, OnAutoBackupChanged));

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

    public ObservableCollection<HoloSkillPlan> SkillPlans
    {
        get => (ObservableCollection<HoloSkillPlan>)GetValue(SkillPlansProperty);
        set => SetValue(SkillPlansProperty, value);
    }

    public ObservableCollection<HoloSkillPlanTemplate> PlanTemplates
    {
        get => (ObservableCollection<HoloSkillPlanTemplate>)GetValue(PlanTemplatesProperty);
        set => SetValue(PlanTemplatesProperty, value);
    }

    public string SelectedCharacter
    {
        get => (string)GetValue(SelectedCharacterProperty);
        set => SetValue(SelectedCharacterProperty, value);
    }

    public PlanManagerMode PlanManagerMode
    {
        get => (PlanManagerMode)GetValue(PlanManagerModeProperty);
        set => SetValue(PlanManagerModeProperty, value);
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

    public bool ShowImportWizard
    {
        get => (bool)GetValue(ShowImportWizardProperty);
        set => SetValue(ShowImportWizardProperty, value);
    }

    public bool ShowExportOptions
    {
        get => (bool)GetValue(ShowExportOptionsProperty);
        set => SetValue(ShowExportOptionsProperty, value);
    }

    public bool ShowSharingOptions
    {
        get => (bool)GetValue(ShowSharingOptionsProperty);
        set => SetValue(ShowSharingOptionsProperty, value);
    }

    public bool AutoBackup
    {
        get => (bool)GetValue(AutoBackupProperty);
        set => SetValue(AutoBackupProperty, value);
    }

    #endregion

    #region Fields

    private Canvas _mainCanvas;
    private Canvas _particleCanvas;
    private TabControl _planTabs;
    private ScrollViewer _plansScrollViewer;
    private StackPanel _plansPanel;
    private ScrollViewer _templatesScrollViewer;
    private StackPanel _templatesPanel;
    private Border _importWizardSection;
    private Border _exportOptionsSection;
    private Border _actionsSection;
    private Border _statusSection;
    
    private readonly List<HoloPlanParticle> _particles = new();
    private readonly DispatcherTimer _animationTimer;
    private readonly DispatcherTimer _backupTimer;
    private readonly Random _random = new();
    
    private bool _isAnimating = true;
    private bool _isDisposed = false;
    private double _currentAnimationTime = 0;

    #endregion

    #region Constructor

    public HoloSkillPlanManager()
    {
        InitializeComponent();
        
        _animationTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16) // 60 FPS
        };
        _animationTimer.Tick += OnAnimationTick;
        
        _backupTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(10) // Auto-backup every 10 minutes
        };
        _backupTimer.Tick += OnBackupTick;
        
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
        
        var contentGrid = new Grid();
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(160, GridUnitType.Pixel) });
        
        // Header section
        var headerSection = CreateHeaderSection();
        Grid.SetRow(headerSection, 0);
        contentGrid.Children.Add(headerSection);
        
        // Main content with tabs
        _planTabs = CreatePlanTabs();
        Grid.SetRow(_planTabs, 1);
        contentGrid.Children.Add(_planTabs);
        
        // Bottom action sections
        var bottomGrid = new Grid();
        bottomGrid.ColumnDefinitions.Add(new ColumnDefinition());
        bottomGrid.ColumnDefinitions.Add(new ColumnDefinition());
        bottomGrid.ColumnDefinitions.Add(new ColumnDefinition());
        
        _actionsSection = CreateActionsSection();
        Grid.SetColumn(_actionsSection, 0);
        bottomGrid.Children.Add(_actionsSection);
        
        _exportOptionsSection = CreateExportOptionsSection();
        Grid.SetColumn(_exportOptionsSection, 1);
        bottomGrid.Children.Add(_exportOptionsSection);
        
        _statusSection = CreateStatusSection();
        Grid.SetColumn(_statusSection, 2);
        bottomGrid.Children.Add(_statusSection);
        
        Grid.SetRow(bottomGrid, 2);
        contentGrid.Children.Add(bottomGrid);
        
        _mainCanvas.Children.Add(contentGrid);
        _mainCanvas.Children.Add(_particleCanvas);
        border.Child = _mainCanvas;
        Content = border;

        CreateSampleData();
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
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(350, GridUnitType.Pixel) });

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
            Text = "SKILL PLAN MANAGEMENT MATRIX",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        titlePanel.Children.Add(titleText);

        var subtitleText = new TextBlock
        {
            Text = "Import • Export • Share • Backup",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 200, 200)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0)
        };
        titlePanel.Children.Add(subtitleText);

        Grid.SetColumn(titlePanel, 0);
        grid.Children.Add(titlePanel);

        // Quick actions panel
        var actionsPanel = CreateQuickActionsPanel();
        Grid.SetColumn(actionsPanel, 1);
        grid.Children.Add(actionsPanel);

        border.Child = grid;
        return border;
    }

    private StackPanel CreateQuickActionsPanel()
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(15, 10, 15, 10)
        };

        var actionsHeader = new TextBlock
        {
            Text = "QUICK ACTIONS",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        panel.Children.Add(actionsHeader);

        var buttonGrid = new Grid();
        buttonGrid.ColumnDefinitions.Add(new ColumnDefinition());
        buttonGrid.ColumnDefinitions.Add(new ColumnDefinition());
        buttonGrid.RowDefinitions.Add(new RowDefinition());
        buttonGrid.RowDefinitions.Add(new RowDefinition());

        // Import button
        var importButton = CreateActionButton("IMPORT", Color.FromArgb(255, 0, 255, 100));
        importButton.Click += OnImportButtonClick;
        Grid.SetColumn(importButton, 0);
        Grid.SetRow(importButton, 0);
        buttonGrid.Children.Add(importButton);

        // Export button
        var exportButton = CreateActionButton("EXPORT", Color.FromArgb(255, 255, 200, 0));
        exportButton.Click += OnExportButtonClick;
        Grid.SetColumn(exportButton, 1);
        Grid.SetRow(exportButton, 0);
        buttonGrid.Children.Add(exportButton);

        // Share button
        var shareButton = CreateActionButton("SHARE", Color.FromArgb(255, 200, 0, 255));
        shareButton.Click += OnShareButtonClick;
        Grid.SetColumn(shareButton, 0);
        Grid.SetRow(shareButton, 1);
        buttonGrid.Children.Add(shareButton);

        // Backup button
        var backupButton = CreateActionButton("BACKUP", Color.FromArgb(255, 255, 100, 0));
        backupButton.Click += OnBackupButtonClick;
        Grid.SetColumn(backupButton, 1);
        Grid.SetRow(backupButton, 1);
        buttonGrid.Children.Add(backupButton);

        panel.Children.Add(buttonGrid);

        return panel;
    }

    private Button CreateActionButton(string text, Color accentColor)
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
            Margin = new Thickness(3),
            Effect = new DropShadowEffect
            {
                Color = accentColor,
                ShadowDepth = 0,
                BlurRadius = 6
            }
        };
    }

    private TabControl CreatePlanTabs()
    {
        var tabControl = new TabControl
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 50, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(80, 0, 200, 255)),
            Margin = new Thickness(10)
        };

        // My Plans Tab
        var plansTab = new TabItem
        {
            Header = "My Plans",
            Foreground = new SolidColorBrush(Colors.White),
            Background = new SolidColorBrush(Color.FromArgb(60, 0, 100, 150))
        };
        plansTab.Content = CreateMyPlansContent();
        tabControl.Items.Add(plansTab);

        // Templates Tab
        var templatesTab = new TabItem
        {
            Header = "Templates",
            Foreground = new SolidColorBrush(Colors.White),
            Background = new SolidColorBrush(Color.FromArgb(60, 0, 100, 150))
        };
        templatesTab.Content = CreateTemplatesContent();
        tabControl.Items.Add(templatesTab);

        // Import/Export Tab
        var importExportTab = new TabItem
        {
            Header = "Import/Export",
            Foreground = new SolidColorBrush(Colors.White),
            Background = new SolidColorBrush(Color.FromArgb(60, 0, 100, 150))
        };
        importExportTab.Content = CreateImportExportContent();
        tabControl.Items.Add(importExportTab);

        return tabControl;
    }

    private ScrollViewer CreateMyPlansContent()
    {
        _plansScrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = new SolidColorBrush(Color.FromArgb(20, 0, 50, 100))
        };

        _plansPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10)
        };

        _plansScrollViewer.Content = _plansPanel;
        return _plansScrollViewer;
    }

    private ScrollViewer CreateTemplatesContent()
    {
        _templatesScrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = new SolidColorBrush(Color.FromArgb(20, 0, 50, 100))
        };

        _templatesPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10)
        };

        _templatesScrollViewer.Content = _templatesPanel;
        return _templatesScrollViewer;
    }

    private Grid CreateImportExportContent()
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());

        // Import wizard
        _importWizardSection = CreateImportWizardSection();
        Grid.SetColumn(_importWizardSection, 0);
        grid.Children.Add(_importWizardSection);

        // Export options
        var exportOptionsDetailed = CreateExportOptionsDetailedSection();
        Grid.SetColumn(exportOptionsDetailed, 1);
        grid.Children.Add(exportOptionsDetailed);

        return grid;
    }

    private Border CreateImportWizardSection()
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 150, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(80, 0, 255, 150)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(10),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(60, 0, 255, 150),
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(15, 15, 15, 15)
        };

        var header = new TextBlock
        {
            Text = "IMPORT WIZARD",
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 150)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 15)
        };
        panel.Children.Add(header);

        // Import sources
        var sourcesPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(0, 0, 0, 15)
        };

        var sourcesHeader = new TextBlock
        {
            Text = "Import Sources:",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            Margin = new Thickness(0, 0, 0, 8)
        };
        sourcesPanel.Children.Add(sourcesHeader);

        var sources = new[]
        {
            "EVE Skill Planner",
            "EVEMON",
            "Pyfa",
            "SkillQ",
            "EVE University Plans",
            "Custom JSON",
            "ESI Character Data",
            "Local Backup Files"
        };

        foreach (var source in sources)
        {
            var sourceItem = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 2, 0, 2)
            };

            var checkbox = new CheckBox
            {
                Content = source,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                IsChecked = source == "EVE Skill Planner"
            };
            sourceItem.Children.Add(checkbox);

            sourcesPanel.Children.Add(sourceItem);
        }

        panel.Children.Add(sourcesPanel);

        // Import button
        var importButton = new Button
        {
            Content = "START IMPORT",
            Width = 120,
            Height = 35,
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            Background = new SolidColorBrush(Color.FromArgb(120, 0, 255, 100)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 0, 255, 100)),
            BorderThickness = new Thickness(1),
            HorizontalAlignment = HorizontalAlignment.Center,
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(150, 0, 255, 100),
                ShadowDepth = 0,
                BlurRadius = 10
            }
        };
        importButton.Click += OnStartImportClick;
        panel.Children.Add(importButton);

        border.Child = panel;
        return border;
    }

    private Border CreateExportOptionsDetailedSection()
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
            Margin = new Thickness(15, 15, 15, 15)
        };

        var header = new TextBlock
        {
            Text = "EXPORT OPTIONS",
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 200, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 15)
        };
        panel.Children.Add(header);

        // Export formats
        var formatsPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(0, 0, 0, 15)
        };

        var formatsHeader = new TextBlock
        {
            Text = "Export Formats:",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            Margin = new Thickness(0, 0, 0, 8)
        };
        formatsPanel.Children.Add(formatsHeader);

        var formats = new[]
        {
            "Gideon JSON (.json)",
            "EVE Skill Planner (.esp)",
            "EVEMON (.emp)",
            "CSV Format (.csv)",
            "XML Export (.xml)",
            "Plain Text (.txt)",
            "Markdown (.md)",
            "HTML Report (.html)"
        };

        foreach (var format in formats)
        {
            var formatItem = new RadioButton
            {
                Content = format,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                GroupName = "ExportFormat",
                IsChecked = format.Contains("JSON"),
                Margin = new Thickness(0, 2, 0, 2)
            };
            formatsPanel.Children.Add(formatItem);
        }

        panel.Children.Add(formatsPanel);

        // Export button
        var exportButton = new Button
        {
            Content = "EXPORT PLANS",
            Width = 120,
            Height = 35,
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            Background = new SolidColorBrush(Color.FromArgb(120, 255, 200, 0)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 255, 200, 0)),
            BorderThickness = new Thickness(1),
            HorizontalAlignment = HorizontalAlignment.Center,
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(150, 255, 200, 0),
                ShadowDepth = 0,
                BlurRadius = 10
            }
        };
        exportButton.Click += OnExportPlansClick;
        panel.Children.Add(exportButton);

        border.Child = panel;
        return border;
    }

    private Border CreateActionsSection()
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 100, 0, 150)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(80, 200, 0, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(10),
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(60, 200, 0, 255),
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
            Text = "PLAN ACTIONS",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 0, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        panel.Children.Add(header);

        var actions = new[]
        {
            "Create New Plan",
            "Clone Existing Plan",
            "Merge Plans",
            "Validate Plans",
            "Optimize Plans",
            "Generate Reports",
            "Schedule Backup",
            "Clean Old Plans"
        };

        foreach (var action in actions)
        {
            var actionButton = new Button
            {
                Content = action,
                Width = 140,
                Height = 25,
                FontSize = 9,
                Foreground = new SolidColorBrush(Colors.White),
                Background = new SolidColorBrush(Color.FromArgb(60, 150, 0, 200)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(120, 200, 0, 255)),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 2, 0, 2),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            panel.Children.Add(actionButton);
        }

        border.Child = panel;
        return border;
    }

    private Border CreateExportOptionsSection()
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
            Text = "SHARING & BACKUP",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 200, 0)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        panel.Children.Add(header);

        var options = new[]
        {
            "Cloud Sync: Enabled",
            "Auto Backup: Every 10min",
            "Shared Plans: 3 active",
            "Team Sharing: Available",
            "Version Control: Enabled",
            "Conflict Resolution: Auto",
            "Backup Location: Local",
            "Export History: 15 files"
        };

        foreach (var option in options)
        {
            var optionText = new TextBlock
            {
                Text = option,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 200)),
                Margin = new Thickness(0, 2, 0, 0)
            };
            panel.Children.Add(optionText);
        }

        border.Child = panel;
        return border;
    }

    private Border CreateStatusSection()
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
            Text = "SYSTEM STATUS",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        panel.Children.Add(header);

        var status = new[]
        {
            "Total Plans: 24",
            "Active Plans: 8",
            "Templates: 12",
            "Shared Plans: 3",
            "Last Backup: 2 min ago",
            "Cloud Status: Synced",
            "Storage Used: 15.2 MB",
            "Import Queue: Empty"
        };

        foreach (var stat in status)
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

    private void CreateSampleData()
    {
        SkillPlans = new ObservableCollection<HoloSkillPlan>
        {
            new HoloSkillPlan
            {
                PlanName = "Caldari Battleship Mastery",
                CharacterName = "Main Character",
                CreatedDate = DateTime.Now.AddDays(-15),
                LastModified = DateTime.Now.AddDays(-2),
                TotalSkills = 15,
                CompletedSkills = 8,
                TotalTime = TimeSpan.FromDays(45),
                RemainingTime = TimeSpan.FromDays(23),
                Priority = SkillPlanPriority.High,
                IsShared = false,
                Tags = new List<string> { "Combat", "Caldari", "PvP" }
            },
            new HoloSkillPlan
            {
                PlanName = "Perfect Mining Efficiency",
                CharacterName = "Alt Character",
                CreatedDate = DateTime.Now.AddDays(-8),
                LastModified = DateTime.Now.AddDays(-1),
                TotalSkills = 22,
                CompletedSkills = 12,
                TotalTime = TimeSpan.FromDays(67),
                RemainingTime = TimeSpan.FromDays(34),
                Priority = SkillPlanPriority.Medium,
                IsShared = true,
                Tags = new List<string> { "Mining", "Industry", "ISK" }
            },
            new HoloSkillPlan
            {
                PlanName = "Exploration Specialist",
                CharacterName = "Main Character",
                CreatedDate = DateTime.Now.AddDays(-3),
                LastModified = DateTime.Now,
                TotalSkills = 18,
                CompletedSkills = 3,
                TotalTime = TimeSpan.FromDays(28),
                RemainingTime = TimeSpan.FromDays(26),
                Priority = SkillPlanPriority.Low,
                IsShared = false,
                Tags = new List<string> { "Exploration", "Scanning", "PvE" }
            }
        };

        PlanTemplates = new ObservableCollection<HoloSkillPlanTemplate>
        {
            new HoloSkillPlanTemplate
            {
                TemplateName = "Perfect Starter",
                Description = "Essential skills for new players",
                Category = "Beginner",
                SkillCount = 25,
                EstimatedTime = TimeSpan.FromDays(14),
                Difficulty = TemplateDifficulty.Beginner,
                Rating = 4.8,
                Downloads = 15420
            },
            new HoloSkillPlanTemplate
            {
                TemplateName = "Incursion Runner",
                Description = "Optimized for high-sec incursions",
                Category = "PvE",
                SkillCount = 32,
                EstimatedTime = TimeSpan.FromDays(89),
                Difficulty = TemplateDifficulty.Advanced,
                Rating = 4.6,
                Downloads = 8930
            },
            new HoloSkillPlanTemplate
            {
                TemplateName = "Null-Sec Warrior",
                Description = "PvP skills for null-sec combat",
                Category = "PvP",
                SkillCount = 45,
                EstimatedTime = TimeSpan.FromDays(156),
                Difficulty = TemplateDifficulty.Expert,
                Rating = 4.9,
                Downloads = 12340
            }
        };

        UpdatePlansDisplay();
        UpdateTemplatesDisplay();
    }

    private void UpdatePlansDisplay()
    {
        if (_plansPanel == null || SkillPlans == null) return;

        _plansPanel.Children.Clear();

        foreach (var plan in SkillPlans)
        {
            var planItem = CreateSkillPlanItem(plan);
            _plansPanel.Children.Add(planItem);
        }
    }

    private void UpdateTemplatesDisplay()
    {
        if (_templatesPanel == null || PlanTemplates == null) return;

        _templatesPanel.Children.Clear();

        foreach (var template in PlanTemplates)
        {
            var templateItem = CreateTemplateItem(template);
            _templatesPanel.Children.Add(templateItem);
        }
    }

    private Border CreateSkillPlanItem(HoloSkillPlan plan)
    {
        var priorityColor = plan.Priority switch
        {
            SkillPlanPriority.High => Color.FromArgb(100, 255, 100, 100),
            SkillPlanPriority.Medium => Color.FromArgb(100, 255, 255, 100),
            SkillPlanPriority.Low => Color.FromArgb(100, 100, 255, 100),
            _ => Color.FromArgb(100, 150, 150, 150)
        };

        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, priorityColor.R, priorityColor.G, priorityColor.B)),
            BorderBrush = new SolidColorBrush(priorityColor),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(5),
            Effect = new DropShadowEffect
            {
                Color = priorityColor,
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Plan header
        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(15, 10, 15, 5)
        };

        var planNameText = new TextBlock
        {
            Text = plan.PlanName,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White)
        };
        headerPanel.Children.Add(planNameText);

        if (plan.IsShared)
        {
            var sharedIcon = new TextBlock
            {
                Text = "🔗",
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };
            headerPanel.Children.Add(sharedIcon);
        }

        var priorityText = new TextBlock
        {
            Text = plan.Priority.ToString().ToUpper(),
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(priorityColor),
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(10, 0, 0, 2)
        };
        headerPanel.Children.Add(priorityText);

        Grid.SetRow(headerPanel, 0);
        grid.Children.Add(headerPanel);

        // Character and dates
        var infoPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(15, 2, 15, 2)
        };

        var characterText = new TextBlock
        {
            Text = $"Character: {plan.CharacterName}",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 200)),
            Margin = new Thickness(0, 0, 20, 0)
        };
        infoPanel.Children.Add(characterText);

        var modifiedText = new TextBlock
        {
            Text = $"Modified: {plan.LastModified:MM/dd/yyyy}",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 200, 200, 200))
        };
        infoPanel.Children.Add(modifiedText);

        Grid.SetRow(infoPanel, 1);
        grid.Children.Add(infoPanel);

        // Progress info
        var progressPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(15, 2, 15, 2)
        };

        var skillsText = new TextBlock
        {
            Text = $"Skills: {plan.CompletedSkills}/{plan.TotalSkills}",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 0)),
            Margin = new Thickness(0, 0, 20, 0)
        };
        progressPanel.Children.Add(skillsText);

        var timeText = new TextBlock
        {
            Text = $"Time: {FormatTimeSpan(plan.RemainingTime)} remaining",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 0, 255, 200))
        };
        progressPanel.Children.Add(timeText);

        Grid.SetRow(progressPanel, 2);
        grid.Children.Add(progressPanel);

        // Progress bar
        var progressBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(60, 50, 50, 50)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(100, 100, 100, 100)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(2),
            Height = 8,
            Margin = new Thickness(15, 5, 15, 5)
        };

        var progress = (double)plan.CompletedSkills / plan.TotalSkills;
        var progressBar = new Rectangle
        {
            Fill = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(priorityColor, 0),
                    new GradientStop(Color.FromArgb(priorityColor.A, (byte)(priorityColor.R * 0.7), (byte)(priorityColor.G * 0.7), (byte)(priorityColor.B * 0.7)), 1)
                }
            },
            Width = 400 * progress,
            Height = 6,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            RadiusX = 1,
            RadiusY = 1
        };

        progressBorder.Child = progressBar;
        Grid.SetRow(progressBorder, 3);
        grid.Children.Add(progressBorder);

        // Tags
        var tagsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(15, 5, 15, 10)
        };

        foreach (var tag in plan.Tags.Take(3))
        {
            var tagBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(60, priorityColor.R, priorityColor.G, priorityColor.B)),
                BorderBrush = new SolidColorBrush(priorityColor),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(6, 2, 6, 2),
                Margin = new Thickness(0, 0, 5, 0)
            };

            var tagText = new TextBlock
            {
                Text = tag,
                FontSize = 8,
                Foreground = new SolidColorBrush(priorityColor)
            };

            tagBorder.Child = tagText;
            tagsPanel.Children.Add(tagBorder);
        }

        Grid.SetRow(tagsPanel, 4);
        grid.Children.Add(tagsPanel);

        border.Child = grid;
        return border;
    }

    private Border CreateTemplateItem(HoloSkillPlanTemplate template)
    {
        var difficultyColor = template.Difficulty switch
        {
            TemplateDifficulty.Beginner => Color.FromArgb(100, 0, 255, 100),
            TemplateDifficulty.Intermediate => Color.FromArgb(100, 255, 255, 0),
            TemplateDifficulty.Advanced => Color.FromArgb(100, 255, 150, 0),
            TemplateDifficulty.Expert => Color.FromArgb(100, 255, 100, 100),
            _ => Color.FromArgb(100, 150, 150, 150)
        };

        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(40, difficultyColor.R, difficultyColor.G, difficultyColor.B)),
            BorderBrush = new SolidColorBrush(difficultyColor),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(5),
            Effect = new DropShadowEffect
            {
                Color = difficultyColor,
                ShadowDepth = 0,
                BlurRadius = 8
            }
        };

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Template header
        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(15, 10, 15, 5)
        };

        var templateNameText = new TextBlock
        {
            Text = template.TemplateName,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White)
        };
        headerPanel.Children.Add(templateNameText);

        var difficultyText = new TextBlock
        {
            Text = template.Difficulty.ToString().ToUpper(),
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(difficultyColor),
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(10, 0, 0, 2)
        };
        headerPanel.Children.Add(difficultyText);

        Grid.SetRow(headerPanel, 0);
        grid.Children.Add(headerPanel);

        // Description
        var descriptionText = new TextBlock
        {
            Text = template.Description,
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 200, 200)),
            Margin = new Thickness(15, 2, 15, 5),
            TextWrapping = TextWrapping.Wrap
        };
        Grid.SetRow(descriptionText, 1);
        grid.Children.Add(descriptionText);

        // Details
        var detailsGrid = new Grid();
        detailsGrid.ColumnDefinitions.Add(new ColumnDefinition());
        detailsGrid.ColumnDefinitions.Add(new ColumnDefinition());

        var skillsText = new TextBlock
        {
            Text = $"Skills: {template.SkillCount}",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 200)),
            Margin = new Thickness(15, 2, 5, 2)
        };
        Grid.SetColumn(skillsText, 0);
        detailsGrid.Children.Add(skillsText);

        var timeText = new TextBlock
        {
            Text = $"Time: {FormatTimeSpan(template.EstimatedTime)}",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 200)),
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(5, 2, 15, 2)
        };
        Grid.SetColumn(timeText, 1);
        detailsGrid.Children.Add(timeText);

        Grid.SetRow(detailsGrid, 2);
        grid.Children.Add(detailsGrid);

        // Rating and downloads
        var statsGrid = new Grid();
        statsGrid.ColumnDefinitions.Add(new ColumnDefinition());
        statsGrid.ColumnDefinitions.Add(new ColumnDefinition());

        var ratingText = new TextBlock
        {
            Text = $"Rating: {template.Rating:F1} ⭐",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 200, 0)),
            Margin = new Thickness(15, 2, 5, 10)
        };
        Grid.SetColumn(ratingText, 0);
        statsGrid.Children.Add(ratingText);

        var downloadsText = new TextBlock
        {
            Text = $"Downloads: {template.Downloads:N0}",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 0, 255, 200)),
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(5, 2, 15, 10)
        };
        Grid.SetColumn(downloadsText, 1);
        statsGrid.Children.Add(downloadsText);

        Grid.SetRow(statsGrid, 3);
        grid.Children.Add(statsGrid);

        border.Child = grid;
        return border;
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

    #region Event Handlers

    private void OnImportButtonClick(object sender, RoutedEventArgs e)
    {
        ShowImportDialog();
    }

    private void OnExportButtonClick(object sender, RoutedEventArgs e)
    {
        ShowExportDialog();
    }

    private void OnShareButtonClick(object sender, RoutedEventArgs e)
    {
        ShowSharingDialog();
    }

    private void OnBackupButtonClick(object sender, RoutedEventArgs e)
    {
        PerformBackup();
    }

    private void OnStartImportClick(object sender, RoutedEventArgs e)
    {
        StartImportProcess();
    }

    private void OnExportPlansClick(object sender, RoutedEventArgs e)
    {
        StartExportProcess();
    }

    private void ShowImportDialog()
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "Import Skill Plans",
            Filter = "All Supported|*.json;*.esp;*.emp;*.xml;*.csv|JSON Files|*.json|EVE Skill Planner|*.esp|EVEMON|*.emp|XML Files|*.xml|CSV Files|*.csv",
            Multiselect = true
        };

        if (openFileDialog.ShowDialog() == true)
        {
            foreach (var fileName in openFileDialog.FileNames)
            {
                ImportSkillPlan(fileName);
            }
        }
    }

    private void ShowExportDialog()
    {
        var saveFileDialog = new SaveFileDialog
        {
            Title = "Export Skill Plans",
            Filter = "Gideon JSON|*.json|EVE Skill Planner|*.esp|EVEMON|*.emp|CSV File|*.csv|XML File|*.xml",
            DefaultExt = ".json"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            ExportSkillPlans(saveFileDialog.FileName);
        }
    }

    private void ShowSharingDialog()
    {
        MessageBox.Show("Sharing functionality will be implemented with cloud integration.", "Share Plans", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void PerformBackup()
    {
        try
        {
            var backupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Gideon", "Backups");
            Directory.CreateDirectory(backupPath);
            
            var backupFile = Path.Combine(backupPath, $"skillplans_backup_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            var json = JsonSerializer.Serialize(SkillPlans, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(backupFile, json);
            
            MessageBox.Show($"Backup created successfully:\n{backupFile}", "Backup Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Backup failed: {ex.Message}", "Backup Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void StartImportProcess()
    {
        ShowImportDialog();
    }

    private void StartExportProcess()
    {
        ShowExportDialog();
    }

    private void ImportSkillPlan(string fileName)
    {
        try
        {
            var extension = Path.GetExtension(fileName).ToLower();
            switch (extension)
            {
                case ".json":
                    ImportFromJson(fileName);
                    break;
                case ".esp":
                    ImportFromESP(fileName);
                    break;
                case ".emp":
                    ImportFromEMON(fileName);
                    break;
                case ".xml":
                    ImportFromXML(fileName);
                    break;
                case ".csv":
                    ImportFromCSV(fileName);
                    break;
                default:
                    MessageBox.Show($"Unsupported file format: {extension}", "Import Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Import failed: {ex.Message}", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ImportFromJson(string fileName)
    {
        var json = File.ReadAllText(fileName);
        var importedPlans = JsonSerializer.Deserialize<List<HoloSkillPlan>>(json);
        
        if (importedPlans != null)
        {
            foreach (var plan in importedPlans)
            {
                SkillPlans.Add(plan);
            }
            UpdatePlansDisplay();
            MessageBox.Show($"Successfully imported {importedPlans.Count} skill plans.", "Import Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void ImportFromESP(string fileName)
    {
        // Placeholder for EVE Skill Planner format import
        MessageBox.Show("EVE Skill Planner import will be implemented.", "Import", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ImportFromEMON(string fileName)
    {
        // Placeholder for EVEMON format import
        MessageBox.Show("EVEMON import will be implemented.", "Import", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ImportFromXML(string fileName)
    {
        // Placeholder for XML format import
        MessageBox.Show("XML import will be implemented.", "Import", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ImportFromCSV(string fileName)
    {
        // Placeholder for CSV format import
        MessageBox.Show("CSV import will be implemented.", "Import", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ExportSkillPlans(string fileName)
    {
        try
        {
            var extension = Path.GetExtension(fileName).ToLower();
            switch (extension)
            {
                case ".json":
                    ExportToJson(fileName);
                    break;
                case ".esp":
                    ExportToESP(fileName);
                    break;
                case ".emp":
                    ExportToEMON(fileName);
                    break;
                case ".xml":
                    ExportToXML(fileName);
                    break;
                case ".csv":
                    ExportToCSV(fileName);
                    break;
                default:
                    MessageBox.Show($"Unsupported export format: {extension}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Export failed: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExportToJson(string fileName)
    {
        var json = JsonSerializer.Serialize(SkillPlans, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(fileName, json);
        MessageBox.Show($"Successfully exported {SkillPlans.Count} skill plans to JSON.", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ExportToESP(string fileName)
    {
        // Placeholder for EVE Skill Planner format export
        MessageBox.Show("EVE Skill Planner export will be implemented.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ExportToEMON(string fileName)
    {
        // Placeholder for EVEMON format export
        MessageBox.Show("EVEMON export will be implemented.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ExportToXML(string fileName)
    {
        // Placeholder for XML format export
        MessageBox.Show("XML export will be implemented.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ExportToCSV(string fileName)
    {
        // Placeholder for CSV format export
        MessageBox.Show("CSV export will be implemented.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    #endregion

    #region Particle System

    private void CreatePlanParticles()
    {
        if (!EnableParticleEffects || _particleCanvas == null) return;

        for (int i = 0; i < 2; i++)
        {
            var particle = new HoloPlanParticle
            {
                X = _random.NextDouble() * ActualWidth,
                Y = _random.NextDouble() * ActualHeight,
                VelocityX = (_random.NextDouble() - 0.5) * 6,
                VelocityY = (_random.NextDouble() - 0.5) * 6,
                Size = _random.NextDouble() * 3 + 1,
                Life = 1.0,
                ParticleType = PlanParticleType.Data,
                Ellipse = new Ellipse
                {
                    Width = 3,
                    Height = 3,
                    Fill = new SolidColorBrush(Color.FromArgb(80, 0, 255, 200))
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
            particle.Life -= 0.003;

            if (particle.Life <= 0 || particle.X < 0 || particle.X > ActualWidth || 
                particle.Y < 0 || particle.Y > ActualHeight)
            {
                _particleCanvas.Children.Remove(particle.Ellipse);
                _particles.RemoveAt(i);
                continue;
            }

            Canvas.SetLeft(particle.Ellipse, particle.X);
            Canvas.SetTop(particle.Ellipse, particle.Y);
            
            var alpha = (byte)(particle.Life * 80);
            var color = particle.ParticleType switch
            {
                PlanParticleType.Data => Color.FromArgb(alpha, 0, 255, 200),
                PlanParticleType.Transfer => Color.FromArgb(alpha, 255, 200, 0),
                PlanParticleType.Sync => Color.FromArgb(alpha, 200, 0, 255),
                _ => Color.FromArgb(alpha, 255, 255, 255)
            };
            
            particle.Ellipse.Fill = new SolidColorBrush(color);
        }

        if (_random.NextDouble() < 0.03)
        {
            CreatePlanParticles();
        }
    }

    #endregion

    #region Animation and Updates

    private void OnAnimationTick(object sender, EventArgs e)
    {
        if (!_isAnimating) return;

        _currentAnimationTime += 16;
        
        UpdateParticles();
        UpdatePlanAnimations();
    }

    private void OnBackupTick(object sender, EventArgs e)
    {
        if (AutoBackup)
        {
            PerformBackup();
        }
    }

    private void UpdatePlanAnimations()
    {
        if (!EnableAnimations) return;

        var intensity = (Math.Sin(_currentAnimationTime * 0.0005) + 1) * 0.5 * HolographicIntensity;
        
        // Animate section glows
        var sections = new[] { _importWizardSection, _exportOptionsSection, _actionsSection, _statusSection };
        foreach (var section in sections)
        {
            if (section?.Effect is DropShadowEffect effect)
            {
                effect.BlurRadius = 8 + intensity * 3;
            }
        }
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _animationTimer.Start();
        if (AutoBackup)
        {
            _backupTimer.Start();
        }
        _isAnimating = true;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer.Stop();
        _backupTimer.Stop();
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
        if (d is HoloSkillPlanManager control)
        {
            control.UpdateHolographicEffects();
        }
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillPlanManager control)
        {
            control.UpdateColorScheme();
        }
    }

    private static void OnSkillPlansChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillPlanManager control)
        {
            control.UpdatePlansDisplay();
        }
    }

    private static void OnPlanTemplatesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillPlanManager control)
        {
            control.UpdateTemplatesDisplay();
        }
    }

    private static void OnSelectedCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillPlanManager control)
        {
            control.LoadCharacterPlans();
        }
    }

    private static void OnPlanManagerModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillPlanManager control)
        {
            control.UpdateManagerMode();
        }
    }

    private static void OnEnableAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillPlanManager control)
        {
            control._isAnimating = control.EnableAnimations;
        }
    }

    private static void OnEnableParticleEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillPlanManager control)
        {
            if (!control.EnableParticleEffects)
            {
                control.ClearParticles();
            }
        }
    }

    private static void OnShowImportWizardChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillPlanManager control)
        {
            control._importWizardSection.Visibility = control.ShowImportWizard ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnShowExportOptionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillPlanManager control)
        {
            control._exportOptionsSection.Visibility = control.ShowExportOptions ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private static void OnShowSharingOptionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillPlanManager control)
        {
            // Sharing options visibility handled in UI updates
        }
    }

    private static void OnAutoBackupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSkillPlanManager control)
        {
            if (control.AutoBackup)
            {
                control._backupTimer.Start();
            }
            else
            {
                control._backupTimer.Stop();
            }
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

    private void LoadCharacterPlans()
    {
        // Load plans for selected character
    }

    private void UpdateManagerMode()
    {
        // Update display based on manager mode
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
        _backupTimer?.Stop();
        ClearParticles();
        
        _isDisposed = true;
    }

    #endregion
}

#region Supporting Classes

public class HoloSkillPlan
{
    public string PlanName { get; set; } = "";
    public string CharacterName { get; set; } = "";
    public DateTime CreatedDate { get; set; }
    public DateTime LastModified { get; set; }
    public int TotalSkills { get; set; }
    public int CompletedSkills { get; set; }
    public TimeSpan TotalTime { get; set; }
    public TimeSpan RemainingTime { get; set; }
    public SkillPlanPriority Priority { get; set; }
    public bool IsShared { get; set; }
    public List<string> Tags { get; set; } = new();
    public string Description { get; set; } = "";
    public List<HoloPlannedSkill> Skills { get; set; } = new();
}

public class HoloSkillPlanTemplate
{
    public string TemplateName { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";
    public int SkillCount { get; set; }
    public TimeSpan EstimatedTime { get; set; }
    public TemplateDifficulty Difficulty { get; set; }
    public double Rating { get; set; }
    public int Downloads { get; set; }
    public string Author { get; set; } = "";
    public DateTime CreatedDate { get; set; }
}

public class HoloPlannedSkill
{
    public string SkillName { get; set; } = "";
    public int CurrentLevel { get; set; }
    public int TargetLevel { get; set; }
    public long SkillPointsRequired { get; set; }
    public TimeSpan TrainingTime { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime PlannedStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public List<string> Prerequisites { get; set; } = new();
}

public class HoloPlanParticle
{
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Size { get; set; }
    public double Life { get; set; }
    public PlanParticleType ParticleType { get; set; }
    public Ellipse Ellipse { get; set; } = null!;
}

public enum PlanManagerMode
{
    MyPlans,
    Templates,
    Shared,
    ImportExport
}

public enum SkillPlanPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum TemplateDifficulty
{
    Beginner,
    Intermediate,
    Advanced,
    Expert
}

public enum PlanParticleType
{
    Data,
    Transfer,
    Sync,
    Processing
}

#endregion