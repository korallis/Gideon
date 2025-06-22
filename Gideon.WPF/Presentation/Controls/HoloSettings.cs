// ==========================================================================
// HoloSettings.cs - Holographic Settings and Preferences Interface
// ==========================================================================
// Comprehensive settings interface featuring holographic preference panels,
// performance controls, animation settings, and EVE integration options.
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
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Holographic settings and preferences interface with advanced configuration options
/// </summary>
public class HoloSettings : UserControl, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloSettings),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloSettings),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty CurrentSettingsProperty =
        DependencyProperty.Register(nameof(CurrentSettings), typeof(HoloSettingsModel), typeof(HoloSettings),
            new PropertyMetadata(null, OnCurrentSettingsChanged));

    public static readonly DependencyProperty SelectedCategoryProperty =
        DependencyProperty.Register(nameof(SelectedCategory), typeof(SettingsCategory), typeof(HoloSettings),
            new PropertyMetadata(SettingsCategory.General, OnSelectedCategoryChanged));

    public static readonly DependencyProperty EnableSettingsAnimationsProperty =
        DependencyProperty.Register(nameof(EnableSettingsAnimations), typeof(bool), typeof(HoloSettings),
            new PropertyMetadata(true, OnEnableSettingsAnimationsChanged));

    public static readonly DependencyProperty EnablePreviewEffectsProperty =
        DependencyProperty.Register(nameof(EnablePreviewEffects), typeof(bool), typeof(HoloSettings),
            new PropertyMetadata(true, OnEnablePreviewEffectsChanged));

    public static readonly DependencyProperty ShowAdvancedOptionsProperty =
        DependencyProperty.Register(nameof(ShowAdvancedOptions), typeof(bool), typeof(HoloSettings),
            new PropertyMetadata(false));

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

    public HoloSettingsModel CurrentSettings
    {
        get => (HoloSettingsModel)GetValue(CurrentSettingsProperty);
        set => SetValue(CurrentSettingsProperty, value);
    }

    public SettingsCategory SelectedCategory
    {
        get => (SettingsCategory)GetValue(SelectedCategoryProperty);
        set => SetValue(SelectedCategoryProperty, value);
    }

    public bool EnableSettingsAnimations
    {
        get => (bool)GetValue(EnableSettingsAnimationsProperty);
        set => SetValue(EnableSettingsAnimationsProperty, value);
    }

    public bool EnablePreviewEffects
    {
        get => (bool)GetValue(EnablePreviewEffectsProperty);
        set => SetValue(EnablePreviewEffectsProperty, value);
    }

    public bool ShowAdvancedOptions
    {
        get => (bool)GetValue(ShowAdvancedOptionsProperty);
        set => SetValue(ShowAdvancedOptionsProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloSettingsEventArgs> SettingChanged;
    public event EventHandler<HoloSettingsEventArgs> SettingsReset;
    public event EventHandler<HoloSettingsEventArgs> SettingsImported;
    public event EventHandler<HoloSettingsEventArgs> SettingsExported;
    public event EventHandler<HoloSettingsEventArgs> CategoryChanged;

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode { get; private set; }

    public void SwitchToFullMode()
    {
        IsInSimplifiedMode = false;
        EnableSettingsAnimations = true;
        EnablePreviewEffects = true;
        UpdateSettingsAppearance();
    }

    public void SwitchToSimplifiedMode()
    {
        IsInSimplifiedMode = true;
        EnableSettingsAnimations = false;
        EnablePreviewEffects = false;
        UpdateSettingsAppearance();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => !_disposed && IsLoaded;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        EnableSettingsAnimations = settings.EnabledFeatures.HasFlag(AnimationFeatures.BasicGlow);
        EnablePreviewEffects = settings.EnabledFeatures.HasFlag(AnimationFeatures.ParticleEffects);
        
        UpdateSettingsAppearance();
    }

    #endregion

    #region Fields

    private Grid _mainGrid;
    private ListBox _categoryList;
    private ScrollViewer _settingsScrollViewer;
    private StackPanel _settingsPanel;
    private Border _previewBorder;
    private Canvas _previewCanvas;
    private Canvas _effectCanvas;
    private Button _saveButton;
    private Button _resetButton;
    private Button _importButton;
    private Button _exportButton;
    private Button _advancedToggle;
    
    private readonly Dictionary<SettingsCategory, FrameworkElement> _categoryPanels = new();
    private readonly List<SettingsParticle> _particles = new();
    private DispatcherTimer _animationTimer;
    private DispatcherTimer _particleTimer;
    private double _animationPhase = 0;
    private double _particlePhase = 0;
    private readonly Random _random = new();
    private bool _disposed = false;
    private bool _hasUnsavedChanges = false;

    #endregion

    #region Constructor

    public HoloSettings()
    {
        InitializeSettings();
        SetupAnimations();
        
        // Register with animation intensity manager
        AnimationIntensityManager.Instance.RegisterTarget(this);
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        
        // Initialize default settings
        CurrentSettings = new HoloSettingsModel();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Save current settings to file
    /// </summary>
    public async Task SaveSettingsAsync()
    {
        if (CurrentSettings == null) return;

        try
        {
            var settingsPath = GetSettingsFilePath();
            var json = JsonSerializer.Serialize(CurrentSettings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(settingsPath, json);
            
            _hasUnsavedChanges = false;
            UpdateSaveButtonState();
            
            if (EnablePreviewEffects && !IsInSimplifiedMode)
            {
                SpawnSaveParticles();
            }

            SettingChanged?.Invoke(this, new HoloSettingsEventArgs 
            { 
                Category = SelectedCategory,
                Action = SettingsAction.Saved,
                Timestamp = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            // Handle save error
            ShowErrorMessage($"Failed to save settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Load settings from file
    /// </summary>
    public async Task LoadSettingsAsync()
    {
        try
        {
            var settingsPath = GetSettingsFilePath();
            if (!File.Exists(settingsPath)) return;

            var json = await File.ReadAllTextAsync(settingsPath);
            var settings = JsonSerializer.Deserialize<HoloSettingsModel>(json);
            
            if (settings != null)
            {
                CurrentSettings = settings;
                UpdateAllSettingsPanels();
                _hasUnsavedChanges = false;
                UpdateSaveButtonState();
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"Failed to load settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Reset settings to defaults
    /// </summary>
    public void ResetToDefaults()
    {
        CurrentSettings = new HoloSettingsModel();
        UpdateAllSettingsPanels();
        _hasUnsavedChanges = true;
        UpdateSaveButtonState();

        SettingsReset?.Invoke(this, new HoloSettingsEventArgs 
        { 
            Category = SelectedCategory,
            Action = SettingsAction.Reset,
            Timestamp = DateTime.Now
        });
    }

    /// <summary>
    /// Import settings from file
    /// </summary>
    public async Task ImportSettingsAsync(string filePath)
    {
        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var importedSettings = JsonSerializer.Deserialize<HoloSettingsModel>(json);
            
            if (importedSettings != null)
            {
                CurrentSettings = importedSettings;
                UpdateAllSettingsPanels();
                _hasUnsavedChanges = true;
                UpdateSaveButtonState();

                SettingsImported?.Invoke(this, new HoloSettingsEventArgs 
                { 
                    Category = SelectedCategory,
                    Action = SettingsAction.Imported,
                    FilePath = filePath,
                    Timestamp = DateTime.Now
                });
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"Failed to import settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Export settings to file
    /// </summary>
    public async Task ExportSettingsAsync(string filePath)
    {
        try
        {
            var json = JsonSerializer.Serialize(CurrentSettings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);

            SettingsExported?.Invoke(this, new HoloSettingsEventArgs 
            { 
                Category = SelectedCategory,
                Action = SettingsAction.Exported,
                FilePath = filePath,
                Timestamp = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"Failed to export settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Update a specific setting value
    /// </summary>
    public void UpdateSetting(string settingName, object value)
    {
        if (CurrentSettings == null) return;

        var property = CurrentSettings.GetType().GetProperty(settingName);
        if (property != null && property.CanWrite)
        {
            property.SetValue(CurrentSettings, value);
            _hasUnsavedChanges = true;
            UpdateSaveButtonState();
            UpdatePreview();

            SettingChanged?.Invoke(this, new HoloSettingsEventArgs 
            { 
                Category = SelectedCategory,
                SettingName = settingName,
                NewValue = value,
                Action = SettingsAction.Changed,
                Timestamp = DateTime.Now
            });
        }
    }

    #endregion

    #region Private Methods

    private void InitializeSettings()
    {
        CreateSettingsInterface();
        UpdateSettingsAppearance();
    }

    private void CreateSettingsInterface()
    {
        // Main grid layout
        _mainGrid = new Grid();
        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200, GridUnitType.Pixel) });
        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(250, GridUnitType.Pixel) });
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        // Header section
        CreateHeaderSection();
        
        // Category list
        CreateCategoryList();
        
        // Settings panel
        CreateSettingsPanel();
        
        // Preview panel
        CreatePreviewPanel();

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
                    new GradientStop(Color.FromArgb(80, 64, 224, 255), 0.0),
                    new GradientStop(Color.FromArgb(40, 64, 224, 255), 1.0)
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
            Text = "Settings & Preferences",
            Foreground = Brushes.White,
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(titleText, 0);

        // Status text
        var statusText = new TextBlock
        {
            Text = "Configure Gideon's holographic interface and behavior",
            Foreground = Brushes.LightGray,
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetColumn(statusText, 1);

        // Action buttons
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };

        _saveButton = new Button
        {
            Content = "Save",
            Width = 60,
            Height = 30,
            Margin = new Thickness(2),
            Style = CreateHoloButtonStyle(),
            IsEnabled = false
        };
        _saveButton.Click += OnSaveClick;

        _resetButton = new Button
        {
            Content = "Reset",
            Width = 60,
            Height = 30,
            Margin = new Thickness(2),
            Style = CreateHoloButtonStyle()
        };
        _resetButton.Click += OnResetClick;

        _importButton = new Button
        {
            Content = "Import",
            Width = 60,
            Height = 30,
            Margin = new Thickness(2),
            Style = CreateHoloButtonStyle()
        };
        _importButton.Click += OnImportClick;

        _exportButton = new Button
        {
            Content = "Export",
            Width = 60,
            Height = 30,
            Margin = new Thickness(2),
            Style = CreateHoloButtonStyle()
        };
        _exportButton.Click += OnExportClick;

        buttonPanel.Children.Add(_saveButton);
        buttonPanel.Children.Add(_resetButton);
        buttonPanel.Children.Add(_importButton);
        buttonPanel.Children.Add(_exportButton);
        Grid.SetColumn(buttonPanel, 2);

        headerGrid.Children.Add(titleText);
        headerGrid.Children.Add(statusText);
        headerGrid.Children.Add(buttonPanel);

        headerBorder.Child = headerGrid;
        Grid.SetColumnSpan(headerBorder, 3);
        Grid.SetRow(headerBorder, 0);
        _mainGrid.Children.Add(headerBorder);
    }

    private void CreateCategoryList()
    {
        var categoryBorder = new Border
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

        var categoryGrid = new Grid();
        categoryGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        categoryGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        categoryGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var categoryHeader = new TextBlock
        {
            Text = "Categories",
            Foreground = Brushes.White,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(15, 10, 15, 5)
        };
        Grid.SetRow(categoryHeader, 0);

        _categoryList = new ListBox
        {
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Margin = new Thickness(15, 5, 15, 5),
            ItemTemplate = CreateCategoryItemTemplate()
        };
        _categoryList.SelectionChanged += OnCategorySelectionChanged;
        PopulateCategoryList();
        Grid.SetRow(_categoryList, 1);

        _advancedToggle = new CheckBox
        {
            Content = "Show Advanced Options",
            Foreground = Brushes.White,
            Margin = new Thickness(15, 5, 15, 10),
            IsChecked = ShowAdvancedOptions
        };
        _advancedToggle.Checked += (s, e) => ShowAdvancedOptions = true;
        _advancedToggle.Unchecked += (s, e) => ShowAdvancedOptions = false;
        Grid.SetRow(_advancedToggle, 2);

        categoryGrid.Children.Add(categoryHeader);
        categoryGrid.Children.Add(_categoryList);
        categoryGrid.Children.Add(_advancedToggle);

        categoryBorder.Child = categoryGrid;
        Grid.SetColumn(categoryBorder, 0);
        Grid.SetRow(categoryBorder, 1);
        _mainGrid.Children.Add(categoryBorder);
    }

    private void CreateSettingsPanel()
    {
        var settingsBorder = new Border
        {
            CornerRadius = new CornerRadius(12),
            BorderThickness = new Thickness(2),
            Margin = new Thickness(5, 5, 5, 10),
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

        _settingsPanel = new StackPanel
        {
            Margin = new Thickness(15)
        };

        _settingsScrollViewer = new ScrollViewer
        {
            Content = _settingsPanel,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
        };

        settingsBorder.Child = _settingsScrollViewer;
        Grid.SetColumn(settingsBorder, 1);
        Grid.SetRow(settingsBorder, 1);
        _mainGrid.Children.Add(settingsBorder);

        CreateCategoryPanels();
    }

    private void CreatePreviewPanel()
    {
        _previewBorder = new Border
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
                    new GradientStop(Color.FromArgb(40, 0, 40, 20), 0.0),
                    new GradientStop(Color.FromArgb(20, 0, 30, 15), 1.0)
                }
            }
        };

        var previewGrid = new Grid();
        previewGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        previewGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var previewHeader = new TextBlock
        {
            Text = "Live Preview",
            Foreground = Brushes.White,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(15, 10, 15, 5)
        };
        Grid.SetRow(previewHeader, 0);

        _previewCanvas = new Canvas
        {
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)),
            Margin = new Thickness(15, 5, 15, 15),
            ClipToBounds = true
        };
        Grid.SetRow(_previewCanvas, 1);

        // Effect canvas for preview particles
        _effectCanvas = new Canvas
        {
            IsHitTestVisible = false,
            ClipToBounds = true
        };
        Grid.SetRow(_effectCanvas, 1);

        previewGrid.Children.Add(previewHeader);
        previewGrid.Children.Add(_previewCanvas);
        previewGrid.Children.Add(_effectCanvas);

        _previewBorder.Child = previewGrid;
        Grid.SetColumn(_previewBorder, 2);
        Grid.SetRow(_previewBorder, 1);
        _mainGrid.Children.Add(_previewBorder);
    }

    private void PopulateCategoryList()
    {
        var categories = Enum.GetValues<SettingsCategory>();
        foreach (var category in categories)
        {
            _categoryList.Items.Add(new SettingsCategoryItem 
            { 
                Category = category,
                Name = GetCategoryDisplayName(category),
                Icon = GetCategoryIcon(category)
            });
        }
        _categoryList.SelectedIndex = 0;
    }

    private void CreateCategoryPanels()
    {
        foreach (var category in Enum.GetValues<SettingsCategory>())
        {
            var panel = CreateCategoryPanel(category);
            _categoryPanels[category] = panel;
        }
        
        UpdateSelectedCategoryPanel();
    }

    private FrameworkElement CreateCategoryPanel(SettingsCategory category)
    {
        var panel = new StackPanel();
        
        switch (category)
        {
            case SettingsCategory.General:
                CreateGeneralSettings(panel);
                break;
            case SettingsCategory.Performance:
                CreatePerformanceSettings(panel);
                break;
            case SettingsCategory.Animation:
                CreateAnimationSettings(panel);
                break;
            case SettingsCategory.EVEIntegration:
                CreateEVEIntegrationSettings(panel);
                break;
            case SettingsCategory.Audio:
                CreateAudioSettings(panel);
                break;
            case SettingsCategory.Advanced:
                CreateAdvancedSettings(panel);
                break;
        }
        
        return panel;
    }

    private void CreateGeneralSettings(StackPanel panel)
    {
        panel.Children.Add(CreateSectionHeader("Application Settings"));
        
        panel.Children.Add(CreateComboBoxSetting("Theme", "Theme", 
            new[] { "Dark", "Light", "Auto" }, CurrentSettings?.Theme ?? "Dark"));
        
        panel.Children.Add(CreateComboBoxSetting("Color Scheme", "ColorScheme",
            Enum.GetNames<EVEColorScheme>(), CurrentSettings?.ColorScheme ?? "ElectricBlue"));
        
        panel.Children.Add(CreateCheckBoxSetting("Start with Windows", "StartWithWindows", 
            CurrentSettings?.StartWithWindows ?? false));
        
        panel.Children.Add(CreateCheckBoxSetting("Minimize to System Tray", "MinimizeToTray", 
            CurrentSettings?.MinimizeToTray ?? true));
        
        panel.Children.Add(CreateSliderSetting("UI Scale", "UIScale", 0.8, 2.0, 
            CurrentSettings?.UIScale ?? 1.0));
    }

    private void CreatePerformanceSettings(StackPanel panel)
    {
        panel.Children.Add(CreateSectionHeader("Performance & Quality"));
        
        panel.Children.Add(CreateComboBoxSetting("Quality Preset", "QualityPreset",
            new[] { "Low", "Medium", "High", "Ultra" }, CurrentSettings?.QualityPreset ?? "High"));
        
        panel.Children.Add(CreateSliderSetting("Target FPS", "TargetFPS", 30, 144, 
            CurrentSettings?.TargetFPS ?? 60));
        
        panel.Children.Add(CreateCheckBoxSetting("VSync", "EnableVSync", 
            CurrentSettings?.EnableVSync ?? true));
        
        panel.Children.Add(CreateCheckBoxSetting("Hardware Acceleration", "HardwareAcceleration", 
            CurrentSettings?.HardwareAcceleration ?? true));
        
        panel.Children.Add(CreateSliderSetting("Memory Usage Limit (MB)", "MemoryLimit", 256, 4096, 
            CurrentSettings?.MemoryLimit ?? 1024));
    }

    private void CreateAnimationSettings(StackPanel panel)
    {
        panel.Children.Add(CreateSectionHeader("Animation & Effects"));
        
        panel.Children.Add(CreateSliderSetting("Animation Intensity", "AnimationIntensity", 0.0, 2.0, 
            CurrentSettings?.AnimationIntensity ?? 1.0));
        
        panel.Children.Add(CreateCheckBoxSetting("Particle Effects", "EnableParticles", 
            CurrentSettings?.EnableParticles ?? true));
        
        panel.Children.Add(CreateCheckBoxSetting("Glow Effects", "EnableGlow", 
            CurrentSettings?.EnableGlow ?? true));
        
        panel.Children.Add(CreateCheckBoxSetting("3D Effects", "Enable3DEffects", 
            CurrentSettings?.Enable3DEffects ?? true));
        
        panel.Children.Add(CreateSliderSetting("Animation Speed", "AnimationSpeed", 0.5, 2.0, 
            CurrentSettings?.AnimationSpeed ?? 1.0));
    }

    private void CreateEVEIntegrationSettings(StackPanel panel)
    {
        panel.Children.Add(CreateSectionHeader("EVE Online Integration"));
        
        panel.Children.Add(CreateCheckBoxSetting("Auto-Connect ESI", "AutoConnectESI", 
            CurrentSettings?.AutoConnectESI ?? true));
        
        panel.Children.Add(CreateSliderSetting("Data Refresh Interval (seconds)", "DataRefreshInterval", 10, 300, 
            CurrentSettings?.DataRefreshInterval ?? 60));
        
        panel.Children.Add(CreateCheckBoxSetting("Real-time Market Data", "RealTimeMarketData", 
            CurrentSettings?.RealTimeMarketData ?? true));
        
        panel.Children.Add(CreateCheckBoxSetting("Skill Queue Monitoring", "SkillQueueMonitoring", 
            CurrentSettings?.SkillQueueMonitoring ?? true));
    }

    private void CreateAudioSettings(StackPanel panel)
    {
        panel.Children.Add(CreateSectionHeader("Audio & Notifications"));
        
        panel.Children.Add(CreateSliderSetting("Master Volume", "MasterVolume", 0.0, 1.0, 
            CurrentSettings?.MasterVolume ?? 0.7));
        
        panel.Children.Add(CreateSliderSetting("UI Sound Volume", "UISoundVolume", 0.0, 1.0, 
            CurrentSettings?.UISoundVolume ?? 0.5));
        
        panel.Children.Add(CreateCheckBoxSetting("Notification Sounds", "NotificationSounds", 
            CurrentSettings?.NotificationSounds ?? true));
        
        panel.Children.Add(CreateCheckBoxSetting("Mute When Minimized", "MuteWhenMinimized", 
            CurrentSettings?.MuteWhenMinimized ?? false));
    }

    private void CreateAdvancedSettings(StackPanel panel)
    {
        panel.Children.Add(CreateSectionHeader("Advanced Configuration"));
        
        panel.Children.Add(CreateCheckBoxSetting("Debug Mode", "DebugMode", 
            CurrentSettings?.DebugMode ?? false));
        
        panel.Children.Add(CreateCheckBoxSetting("Detailed Logging", "DetailedLogging", 
            CurrentSettings?.DetailedLogging ?? false));
        
        panel.Children.Add(CreateTextBoxSetting("Log File Path", "LogFilePath", 
            CurrentSettings?.LogFilePath ?? "logs/gideon.log"));
        
        panel.Children.Add(CreateSliderSetting("Cache Size (MB)", "CacheSize", 64, 1024, 
            CurrentSettings?.CacheSize ?? 256));
    }

    private UIElement CreateSectionHeader(string title)
    {
        return new TextBlock
        {
            Text = title,
            Foreground = Brushes.White,
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 15, 0, 10)
        };
    }

    private UIElement CreateCheckBoxSetting(string label, string propertyName, bool currentValue)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var labelText = new TextBlock
        {
            Text = label,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };
        Grid.SetColumn(labelText, 0);

        var checkBox = new CheckBox
        {
            IsChecked = currentValue,
            VerticalAlignment = VerticalAlignment.Center,
            Tag = propertyName
        };
        checkBox.Checked += (s, e) => OnSettingChanged(propertyName, true);
        checkBox.Unchecked += (s, e) => OnSettingChanged(propertyName, false);
        Grid.SetColumn(checkBox, 1);

        grid.Children.Add(labelText);
        grid.Children.Add(checkBox);

        return new Border
        {
            Child = grid,
            Padding = new Thickness(0, 5),
            BorderBrush = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Margin = new Thickness(0, 2)
        };
    }

    private UIElement CreateSliderSetting(string label, string propertyName, double min, double max, double currentValue)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150, GridUnitType.Pixel) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50, GridUnitType.Pixel) });

        var labelText = new TextBlock
        {
            Text = label,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(labelText, 0);

        var slider = new Slider
        {
            Minimum = min,
            Maximum = max,
            Value = currentValue,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0),
            Tag = propertyName
        };
        slider.ValueChanged += (s, e) => OnSettingChanged(propertyName, e.NewValue);
        Grid.SetColumn(slider, 1);

        var valueText = new TextBlock
        {
            Text = currentValue.ToString("F1"),
            Foreground = new SolidColorBrush(GetEVEColor(EVEColorScheme)),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontWeight = FontWeights.Bold
        };
        slider.ValueChanged += (s, e) => valueText.Text = e.NewValue.ToString("F1");
        Grid.SetColumn(valueText, 2);

        grid.Children.Add(labelText);
        grid.Children.Add(slider);
        grid.Children.Add(valueText);

        return new Border
        {
            Child = grid,
            Padding = new Thickness(0, 8),
            BorderBrush = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Margin = new Thickness(0, 2)
        };
    }

    private UIElement CreateComboBoxSetting(string label, string propertyName, string[] options, string currentValue)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150, GridUnitType.Pixel) });

        var labelText = new TextBlock
        {
            Text = label,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(labelText, 0);

        var comboBox = new ComboBox
        {
            Width = 140,
            Height = 25,
            VerticalAlignment = VerticalAlignment.Center,
            Tag = propertyName
        };
        
        foreach (var option in options)
        {
            comboBox.Items.Add(option);
        }
        comboBox.SelectedItem = currentValue;
        comboBox.SelectionChanged += (s, e) => OnSettingChanged(propertyName, comboBox.SelectedItem);
        Grid.SetColumn(comboBox, 1);

        grid.Children.Add(labelText);
        grid.Children.Add(comboBox);

        return new Border
        {
            Child = grid,
            Padding = new Thickness(0, 5),
            BorderBrush = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Margin = new Thickness(0, 2)
        };
    }

    private UIElement CreateTextBoxSetting(string label, string propertyName, string currentValue)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200, GridUnitType.Pixel) });

        var labelText = new TextBlock
        {
            Text = label,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(labelText, 0);

        var textBox = new TextBox
        {
            Text = currentValue,
            Width = 190,
            Height = 25,
            VerticalAlignment = VerticalAlignment.Center,
            Tag = propertyName
        };
        textBox.TextChanged += (s, e) => OnSettingChanged(propertyName, textBox.Text);
        Grid.SetColumn(textBox, 1);

        grid.Children.Add(labelText);
        grid.Children.Add(textBox);

        return new Border
        {
            Child = grid,
            Padding = new Thickness(0, 5),
            BorderBrush = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Margin = new Thickness(0, 2)
        };
    }

    private DataTemplate CreateCategoryItemTemplate()
    {
        var template = new DataTemplate();
        
        var border = new FrameworkElementFactory(typeof(Border));
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
        border.SetValue(Border.MarginProperty, new Thickness(2));
        border.SetValue(Border.PaddingProperty, new Thickness(10, 6));
        
        var stackPanel = new FrameworkElementFactory(typeof(StackPanel));
        stackPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        
        var iconText = new FrameworkElementFactory(typeof(TextBlock));
        iconText.SetBinding(TextBlock.TextProperty, new Binding("Icon"));
        iconText.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(GetEVEColor(EVEColorScheme)));
        iconText.SetValue(TextBlock.FontSizeProperty, 16.0);
        iconText.SetValue(TextBlock.MarginProperty, new Thickness(0, 0, 8, 0));
        
        var nameText = new FrameworkElementFactory(typeof(TextBlock));
        nameText.SetBinding(TextBlock.TextProperty, new Binding("Name"));
        nameText.SetValue(TextBlock.ForegroundProperty, Brushes.White);
        nameText.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        
        stackPanel.AppendChild(iconText);
        stackPanel.AppendChild(nameText);
        border.AppendChild(stackPanel);
        
        template.VisualTree = border;
        return template;
    }

    private Style CreateHoloButtonStyle()
    {
        var style = new Style(typeof(Button));
        
        style.Setters.Add(new Setter(Button.BackgroundProperty, 
            new SolidColorBrush(Color.FromArgb(100, 64, 224, 255))));
        style.Setters.Add(new Setter(Button.ForegroundProperty, Brushes.White));
        style.Setters.Add(new Setter(Button.BorderBrushProperty, 
            new SolidColorBrush(Color.FromArgb(180, 64, 224, 255))));
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
        if (!EnableSettingsAnimations || IsInSimplifiedMode) return;

        _animationPhase += 0.05;
        if (_animationPhase > Math.PI * 2)
            _animationPhase = 0;

        UpdatePreviewAnimations();
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        if (!EnablePreviewEffects || IsInSimplifiedMode || _effectCanvas == null) return;

        _particlePhase += 0.1;
        if (_particlePhase > Math.PI * 2)
            _particlePhase = 0;

        UpdateParticleEffects();
    }

    private void UpdatePreviewAnimations()
    {
        UpdatePreview();
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

    private void UpdatePreview()
    {
        if (_previewCanvas == null || CurrentSettings == null) return;

        _previewCanvas.Children.Clear();

        // Create preview elements based on current settings
        CreatePreviewElements();
    }

    private void CreatePreviewElements()
    {
        var color = GetEVEColor(EVEColorScheme);
        
        // Preview glow effect
        if (CurrentSettings?.EnableGlow == true)
        {
            var glowEllipse = new Ellipse
            {
                Width = 60,
                Height = 60,
                Fill = new SolidColorBrush(Color.FromArgb(100, color.R, color.G, color.B)),
                Effect = new DropShadowEffect
                {
                    Color = color,
                    BlurRadius = 15 * (CurrentSettings?.AnimationIntensity ?? 1.0),
                    ShadowDepth = 0,
                    Opacity = 0.8
                }
            };
            Canvas.SetLeft(glowEllipse, 50);
            Canvas.SetTop(glowEllipse, 50);
            _previewCanvas.Children.Add(glowEllipse);
        }

        // Preview particle effects
        if (CurrentSettings?.EnableParticles == true && EnablePreviewEffects)
        {
            SpawnPreviewParticles();
        }
    }

    private void SpawnPreviewParticles()
    {
        if (_particles.Count >= 10) return;

        for (int i = 0; i < 3; i++)
        {
            var particle = CreateSettingsParticle();
            _particles.Add(particle);
            _effectCanvas.Children.Add(particle.Visual);
        }
    }

    private void SpawnSaveParticles()
    {
        for (int i = 0; i < 8; i++)
        {
            var particle = CreateSettingsParticle();
            _particles.Add(particle);
            _effectCanvas.Children.Add(particle.Visual);
        }
    }

    private SettingsParticle CreateSettingsParticle()
    {
        var color = GetEVEColor(EVEColorScheme);
        var size = 2 + _random.NextDouble() * 2;
        
        var ellipse = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromArgb(150, color.R, color.G, color.B))
        };

        var particle = new SettingsParticle
        {
            Visual = ellipse,
            X = _random.NextDouble() * _previewCanvas.ActualWidth,
            Y = _previewCanvas.ActualHeight,
            VelocityX = (_random.NextDouble() - 0.5) * 2,
            VelocityY = -1 - _random.NextDouble() * 3,
            Life = 1.0
        };

        Canvas.SetLeft(ellipse, particle.X);
        Canvas.SetTop(ellipse, particle.Y);

        return particle;
    }

    private void UpdateSelectedCategoryPanel()
    {
        _settingsPanel.Children.Clear();
        
        if (_categoryPanels.TryGetValue(SelectedCategory, out var panel))
        {
            _settingsPanel.Children.Add(panel);
        }
    }

    private void UpdateAllSettingsPanels()
    {
        // Recreate all category panels with updated values
        _categoryPanels.Clear();
        CreateCategoryPanels();
        UpdateSelectedCategoryPanel();
    }

    private void UpdateSaveButtonState()
    {
        if (_saveButton != null)
        {
            _saveButton.IsEnabled = _hasUnsavedChanges;
        }
    }

    private void UpdateSettingsAppearance()
    {
        UpdateColors();
        UpdateEffects();
        UpdatePreview();
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

    private string GetCategoryDisplayName(SettingsCategory category)
    {
        return category switch
        {
            SettingsCategory.General => "General",
            SettingsCategory.Performance => "Performance",
            SettingsCategory.Animation => "Animation",
            SettingsCategory.EVEIntegration => "EVE Integration",
            SettingsCategory.Audio => "Audio",
            SettingsCategory.Advanced => "Advanced",
            _ => category.ToString()
        };
    }

    private string GetCategoryIcon(SettingsCategory category)
    {
        return category switch
        {
            SettingsCategory.General => "⚙️",
            SettingsCategory.Performance => "⚡",
            SettingsCategory.Animation => "✨",
            SettingsCategory.EVEIntegration => "🚀",
            SettingsCategory.Audio => "🔊",
            SettingsCategory.Advanced => "🔧",
            _ => "📁"
        };
    }

    private string GetSettingsFilePath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var gideonPath = Path.Combine(appDataPath, "Gideon");
        Directory.CreateDirectory(gideonPath);
        return Path.Combine(gideonPath, "settings.json");
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
            _ => Color.FromRgb(64, 224, 255)
        };
    }

    private void ShowErrorMessage(string message)
    {
        // Show error in UI - could be implemented as toast notification
        MessageBox.Show(message, "Settings Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void OnSettingChanged(string propertyName, object value)
    {
        UpdateSetting(propertyName, value);
    }

    private void OnCategorySelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_categoryList.SelectedItem is SettingsCategoryItem item)
        {
            SelectedCategory = item.Category;
        }
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
        await SaveSettingsAsync();
    }

    private void OnResetClick(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show("Are you sure you want to reset all settings to defaults?", 
            "Reset Settings", MessageBoxButton.YesNo, MessageBoxImage.Question);
        
        if (result == MessageBoxResult.Yes)
        {
            ResetToDefaults();
        }
    }

    private async void OnImportClick(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            Title = "Import Settings"
        };
        
        if (dialog.ShowDialog() == true)
        {
            await ImportSettingsAsync(dialog.FileName);
        }
    }

    private async void OnExportClick(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            Title = "Export Settings",
            FileName = "gideon-settings.json"
        };
        
        if (dialog.ShowDialog() == true)
        {
            await ExportSettingsAsync(dialog.FileName);
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableSettingsAnimations && !IsInSimplifiedMode)
            _animationTimer.Start();
            
        if (EnablePreviewEffects && !IsInSimplifiedMode)
            _particleTimer.Start();

        _ = LoadSettingsAsync();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        _particleTimer?.Stop();
        
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
        if (d is HoloSettings settings)
            settings.UpdateSettingsAppearance();
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSettings settings)
            settings.UpdateSettingsAppearance();
    }

    private static void OnCurrentSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSettings settings)
            settings.UpdateAllSettingsPanels();
    }

    private static void OnSelectedCategoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSettings settings)
        {
            settings.UpdateSelectedCategoryPanel();
            settings.CategoryChanged?.Invoke(settings, new HoloSettingsEventArgs 
            { 
                Category = (SettingsCategory)e.NewValue,
                Action = SettingsAction.CategoryChanged,
                Timestamp = DateTime.Now
            });
        }
    }

    private static void OnEnableSettingsAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSettings settings)
        {
            if ((bool)e.NewValue && !settings.IsInSimplifiedMode)
                settings._animationTimer.Start();
            else
                settings._animationTimer.Stop();
        }
    }

    private static void OnEnablePreviewEffectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloSettings settings)
        {
            if ((bool)e.NewValue && !settings.IsInSimplifiedMode)
                settings._particleTimer.Start();
            else
                settings._particleTimer.Stop();
            
            settings.UpdatePreview();
        }
    }

    #endregion
}

/// <summary>
/// Settings particle for visual effects
/// </summary>
internal class SettingsParticle
{
    public FrameworkElement Visual { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
}

/// <summary>
/// Settings category item for list display
/// </summary>
public class SettingsCategoryItem
{
    public SettingsCategory Category { get; set; }
    public string Name { get; set; }
    public string Icon { get; set; }
}

/// <summary>
/// Settings model containing all configuration options
/// </summary>
public class HoloSettingsModel
{
    // General Settings
    public string Theme { get; set; } = "Dark";
    public string ColorScheme { get; set; } = "ElectricBlue";
    public bool StartWithWindows { get; set; } = false;
    public bool MinimizeToTray { get; set; } = true;
    public double UIScale { get; set; } = 1.0;

    // Performance Settings
    public string QualityPreset { get; set; } = "High";
    public int TargetFPS { get; set; } = 60;
    public bool EnableVSync { get; set; } = true;
    public bool HardwareAcceleration { get; set; } = true;
    public int MemoryLimit { get; set; } = 1024;

    // Animation Settings
    public double AnimationIntensity { get; set; } = 1.0;
    public bool EnableParticles { get; set; } = true;
    public bool EnableGlow { get; set; } = true;
    public bool Enable3DEffects { get; set; } = true;
    public double AnimationSpeed { get; set; } = 1.0;

    // EVE Integration Settings
    public bool AutoConnectESI { get; set; } = true;
    public int DataRefreshInterval { get; set; } = 60;
    public bool RealTimeMarketData { get; set; } = true;
    public bool SkillQueueMonitoring { get; set; } = true;

    // Audio Settings
    public double MasterVolume { get; set; } = 0.7;
    public double UISoundVolume { get; set; } = 0.5;
    public bool NotificationSounds { get; set; } = true;
    public bool MuteWhenMinimized { get; set; } = false;

    // Advanced Settings
    public bool DebugMode { get; set; } = false;
    public bool DetailedLogging { get; set; } = false;
    public string LogFilePath { get; set; } = "logs/gideon.log";
    public int CacheSize { get; set; } = 256;
}

/// <summary>
/// Settings categories
/// </summary>
public enum SettingsCategory
{
    General,
    Performance,
    Animation,
    EVEIntegration,
    Audio,
    Advanced
}

/// <summary>
/// Settings actions
/// </summary>
public enum SettingsAction
{
    Changed,
    Saved,
    Reset,
    Imported,
    Exported,
    CategoryChanged
}

/// <summary>
/// Event args for settings events
/// </summary>
public class HoloSettingsEventArgs : EventArgs
{
    public SettingsCategory Category { get; set; }
    public string SettingName { get; set; }
    public object NewValue { get; set; }
    public SettingsAction Action { get; set; }
    public string FilePath { get; set; }
    public DateTime Timestamp { get; set; }
}