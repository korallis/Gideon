// ==========================================================================
// AnimationSettingsControl.cs - Animation Intensity Configuration UI
// ==========================================================================
// User interface control for configuring animation intensity settings
// with real-time preview and preset management.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Configuration control for animation intensity settings
/// </summary>
public class AnimationSettingsControl : UserControl, INotifyPropertyChanged
{
    #region Events

    public event PropertyChangedEventHandler PropertyChanged;
    public event EventHandler SettingsChanged;

    #endregion

    #region Dependency Properties

    public static readonly DependencyProperty IsPreviewEnabledProperty =
        DependencyProperty.Register(nameof(IsPreviewEnabled), typeof(bool), typeof(AnimationSettingsControl),
            new PropertyMetadata(true, OnIsPreviewEnabledChanged));

    public static readonly DependencyProperty ShowAdvancedOptionsProperty =
        DependencyProperty.Register(nameof(ShowAdvancedOptions), typeof(bool), typeof(AnimationSettingsControl),
            new PropertyMetadata(false, OnShowAdvancedOptionsChanged));

    public bool IsPreviewEnabled
    {
        get => (bool)GetValue(IsPreviewEnabledProperty);
        set => SetValue(IsPreviewEnabledProperty, value);
    }

    public bool ShowAdvancedOptions
    {
        get => (bool)GetValue(ShowAdvancedOptionsProperty);
        set => SetValue(ShowAdvancedOptionsProperty, value);
    }

    #endregion

    #region Private Fields

    private ComboBox _presetComboBox;
    private Slider _masterIntensitySlider;
    private Slider _particleIntensitySlider;
    private Slider _glowIntensitySlider;
    private Slider _transitionIntensitySlider;
    private Slider _motionIntensitySlider;
    private CheckBox _adaptiveCheckBox;
    private Canvas _previewCanvas;
    private Grid _advancedOptionsGrid;
    
    private readonly AnimationIntensityManager _intensityManager;
    private bool _isUpdatingFromManager = false;

    #endregion

    #region Constructor

    public AnimationSettingsControl()
    {
        _intensityManager = AnimationIntensityManager.Instance;
        InitializeComponent();
        SetupDataBindings();
        SetupEventHandlers();
    }

    #endregion

    #region Private Methods

    private void InitializeComponent()
    {
        var mainGrid = new Grid();
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Preset selection
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Master controls
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Advanced toggle
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Advanced options
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Preview

        CreatePresetSection(mainGrid, 0);
        CreateMasterControlsSection(mainGrid, 1);
        CreateAdvancedToggleSection(mainGrid, 2);
        CreateAdvancedOptionsSection(mainGrid, 3);
        CreatePreviewSection(mainGrid, 4);

        Content = mainGrid;
    }

    private void CreatePresetSection(Grid parent, int row)
    {
        var panel = new Simple2DPanel
        {
            Margin = new Thickness(0, 0, 0, 8),
            EVEColorScheme = EVEColorScheme.ElectricBlue
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var label = new TextBlock
        {
            Text = "Animation Preset:",
            Foreground = new SolidColorBrush(Color.FromRgb(64, 224, 255)),
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0)
        };
        Grid.SetColumn(label, 0);

        _presetComboBox = new ComboBox
        {
            MinWidth = 150,
            Margin = new Thickness(0, 4, 0, 4)
        };

        // Populate with presets
        var presets = _intensityManager.GetAvailablePresets().ToList();
        presets.Add(AnimationPreset.Custom);
        _presetComboBox.ItemsSource = presets;
        _presetComboBox.SelectedItem = _intensityManager.CurrentPreset;

        Grid.SetColumn(_presetComboBox, 1);

        grid.Children.Add(label);
        grid.Children.Add(_presetComboBox);
        panel.Child = grid;

        Grid.SetRow(panel, row);
        parent.Children.Add(panel);
    }

    private void CreateMasterControlsSection(Grid parent, int row)
    {
        var panel = new Simple2DPanel
        {
            Margin = new Thickness(0, 0, 0, 8),
            EVEColorScheme = EVEColorScheme.GoldAccent
        };

        var stackPanel = new StackPanel();

        // Master intensity
        var masterGrid = CreateSliderControl("Master Intensity:", out _masterIntensitySlider, 0.0, 2.0, 0.1);
        stackPanel.Children.Add(masterGrid);

        // Adaptive checkbox
        _adaptiveCheckBox = new CheckBox
        {
            Content = "Enable Adaptive Performance",
            Foreground = new SolidColorBrush(Color.FromRgb(255, 215, 0)),
            Margin = new Thickness(0, 8, 0, 0),
            IsChecked = _intensityManager.IsAdaptiveEnabled
        };
        stackPanel.Children.Add(_adaptiveCheckBox);

        panel.Child = stackPanel;

        Grid.SetRow(panel, row);
        parent.Children.Add(panel);
    }

    private void CreateAdvancedToggleSection(Grid parent, int row)
    {
        var toggleButton = new Simple2DButton
        {
            Content = "Advanced Options",
            EVEColorScheme = EVEColorScheme.VoidPurple,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 0, 8)
        };
        
        toggleButton.Click += (s, e) => ShowAdvancedOptions = !ShowAdvancedOptions;

        Grid.SetRow(toggleButton, row);
        parent.Children.Add(toggleButton);
    }

    private void CreateAdvancedOptionsSection(Grid parent, int row)
    {
        _advancedOptionsGrid = new Grid
        {
            Visibility = ShowAdvancedOptions ? Visibility.Visible : Visibility.Collapsed
        };

        var panel = new Simple2DPanel
        {
            Margin = new Thickness(0, 0, 0, 8),
            EVEColorScheme = EVEColorScheme.VoidPurple
        };

        var stackPanel = new StackPanel();

        // Component intensity sliders
        var particleGrid = CreateSliderControl("Particle Effects:", out _particleIntensitySlider, 0.0, 2.0, 0.1);
        stackPanel.Children.Add(particleGrid);

        var glowGrid = CreateSliderControl("Glow Effects:", out _glowIntensitySlider, 0.0, 2.0, 0.1);
        stackPanel.Children.Add(glowGrid);

        var transitionGrid = CreateSliderControl("Transitions:", out _transitionIntensitySlider, 0.0, 2.0, 0.1);
        stackPanel.Children.Add(transitionGrid);

        var motionGrid = CreateSliderControl("Motion Effects:", out _motionIntensitySlider, 0.0, 2.0, 0.1);
        stackPanel.Children.Add(motionGrid);

        // Animation features checkboxes
        CreateAnimationFeaturesSection(stackPanel);

        panel.Child = stackPanel;
        _advancedOptionsGrid.Children.Add(panel);

        Grid.SetRow(_advancedOptionsGrid, row);
        parent.Children.Add(_advancedOptionsGrid);
    }

    private void CreateAnimationFeaturesSection(StackPanel parent)
    {
        var featuresLabel = new TextBlock
        {
            Text = "Animation Features:",
            Foreground = new SolidColorBrush(Color.FromRgb(138, 43, 226)),
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 12, 0, 4)
        };
        parent.Children.Add(featuresLabel);

        var featuresPanel = new WrapPanel
        {
            Orientation = Orientation.Horizontal
        };

        var features = new[]
        {
            (AnimationFeatures.ParticleEffects, "Particles"),
            (AnimationFeatures.BlurEffects, "Blur Effects"),
            (AnimationFeatures.ComplexTransitions, "Complex Transitions"),
            (AnimationFeatures.AdvancedShaders, "Advanced Shaders")
        };

        foreach (var (feature, name) in features)
        {
            var checkBox = new CheckBox
            {
                Content = name,
                Foreground = new SolidColorBrush(Color.FromRgb(138, 43, 226)),
                Margin = new Thickness(0, 2, 12, 2),
                Tag = feature
            };
            
            checkBox.Checked += OnFeatureCheckBoxChanged;
            checkBox.Unchecked += OnFeatureCheckBoxChanged;
            
            featuresPanel.Children.Add(checkBox);
        }

        parent.Children.Add(featuresPanel);
    }

    private void CreatePreviewSection(Grid parent, int row)
    {
        var panel = new Simple2DPanel
        {
            EVEColorScheme = EVEColorScheme.EmeraldGreen
        };

        var previewStack = new StackPanel();

        var previewLabel = new TextBlock
        {
            Text = "Live Preview:",
            Foreground = new SolidColorBrush(Color.FromRgb(50, 205, 50)),
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 8)
        };
        previewStack.Children.Add(previewLabel);

        _previewCanvas = new Canvas
        {
            Height = 100,
            Background = new SolidColorBrush(Color.FromArgb(20, 0, 50, 100)),
            ClipToBounds = true
        };
        previewStack.Children.Add(_previewCanvas);

        panel.Child = previewStack;

        Grid.SetRow(panel, row);
        parent.Children.Add(panel);

        if (IsPreviewEnabled)
            StartPreviewAnimations();
    }

    private Grid CreateSliderControl(string label, out Slider slider, double min, double max, double tickFrequency)
    {
        var grid = new Grid
        {
            Margin = new Thickness(0, 4, 0, 4)
        };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });

        var labelBlock = new TextBlock
        {
            Text = label,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(labelBlock, 0);

        slider = new Slider
        {
            Minimum = min,
            Maximum = max,
            TickFrequency = tickFrequency,
            IsSnapToTickEnabled = true,
            Margin = new Thickness(8, 0, 8, 0)
        };
        Grid.SetColumn(slider, 1);

        var valueBlock = new TextBlock
        {
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        valueBlock.SetBinding(TextBlock.TextProperty, new Binding("Value")
        {
            Source = slider,
            StringFormat = "F1"
        });
        Grid.SetColumn(valueBlock, 2);

        grid.Children.Add(labelBlock);
        grid.Children.Add(slider);
        grid.Children.Add(valueBlock);

        return grid;
    }

    private void SetupDataBindings()
    {
        _masterIntensitySlider.Value = _intensityManager.MasterIntensity;
        _particleIntensitySlider.Value = _intensityManager.ParticleIntensity;
        _glowIntensitySlider.Value = _intensityManager.GlowIntensity;
        _transitionIntensitySlider.Value = _intensityManager.TransitionIntensity;
        _motionIntensitySlider.Value = _intensityManager.MotionIntensity;
    }

    private void SetupEventHandlers()
    {
        _presetComboBox.SelectionChanged += OnPresetSelectionChanged;
        _masterIntensitySlider.ValueChanged += OnSliderValueChanged;
        _particleIntensitySlider.ValueChanged += OnSliderValueChanged;
        _glowIntensitySlider.ValueChanged += OnSliderValueChanged;
        _transitionIntensitySlider.ValueChanged += OnSliderValueChanged;
        _motionIntensitySlider.ValueChanged += OnSliderValueChanged;
        _adaptiveCheckBox.Checked += OnAdaptiveCheckBoxChanged;
        _adaptiveCheckBox.Unchecked += OnAdaptiveCheckBoxChanged;

        _intensityManager.PropertyChanged += OnIntensityManagerPropertyChanged;
        _intensityManager.IntensityChanged += OnIntensityManagerIntensityChanged;
    }

    private void OnPresetSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isUpdatingFromManager || _presetComboBox.SelectedItem is not AnimationPreset preset)
            return;

        _intensityManager.CurrentPreset = preset;
        UpdateSlidersFromManager();
        UpdatePreview();
    }

    private void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isUpdatingFromManager) return;

        if (sender == _masterIntensitySlider)
            _intensityManager.SetMasterIntensity(e.NewValue);
        else if (sender == _particleIntensitySlider)
            _intensityManager.SetComponentIntensity(AnimationComponent.Particles, e.NewValue);
        else if (sender == _glowIntensitySlider)
            _intensityManager.SetComponentIntensity(AnimationComponent.Glow, e.NewValue);
        else if (sender == _transitionIntensitySlider)
            _intensityManager.SetComponentIntensity(AnimationComponent.Transitions, e.NewValue);
        else if (sender == _motionIntensitySlider)
            _intensityManager.SetComponentIntensity(AnimationComponent.Motion, e.NewValue);

        UpdatePreview();
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnAdaptiveCheckBoxChanged(object sender, RoutedEventArgs e)
    {
        if (_isUpdatingFromManager) return;
        
        _intensityManager.IsAdaptiveEnabled = _adaptiveCheckBox.IsChecked == true;
    }

    private void OnFeatureCheckBoxChanged(object sender, RoutedEventArgs e)
    {
        if (_isUpdatingFromManager || sender is not CheckBox checkBox || checkBox.Tag is not AnimationFeatures feature)
            return;

        var currentFeatures = _intensityManager.CurrentSettings.EnabledFeatures;
        
        if (checkBox.IsChecked == true)
            currentFeatures |= feature;
        else
            currentFeatures &= ~feature;

        _intensityManager.SetAnimationFeatures(currentFeatures);
        UpdatePreview();
    }

    private void OnIntensityManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            _isUpdatingFromManager = true;
            
            if (e.PropertyName == nameof(AnimationIntensityManager.CurrentPreset))
            {
                _presetComboBox.SelectedItem = _intensityManager.CurrentPreset;
                UpdateSlidersFromManager();
            }
            else if (e.PropertyName == nameof(AnimationIntensityManager.IsAdaptiveEnabled))
            {
                _adaptiveCheckBox.IsChecked = _intensityManager.IsAdaptiveEnabled;
            }

            _isUpdatingFromManager = false;
        });
    }

    private void OnIntensityManagerIntensityChanged(object sender, IntensityChangedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            UpdateSlidersFromManager();
            UpdatePreview();
        });
    }

    private void UpdateSlidersFromManager()
    {
        _isUpdatingFromManager = true;
        
        _masterIntensitySlider.Value = _intensityManager.MasterIntensity;
        _particleIntensitySlider.Value = _intensityManager.ParticleIntensity;
        _glowIntensitySlider.Value = _intensityManager.GlowIntensity;
        _transitionIntensitySlider.Value = _intensityManager.TransitionIntensity;
        _motionIntensitySlider.Value = _intensityManager.MotionIntensity;
        
        _isUpdatingFromManager = false;
    }

    private void StartPreviewAnimations()
    {
        if (!IsPreviewEnabled || _previewCanvas == null) return;

        _previewCanvas.Children.Clear();

        // Create sample visual elements to demonstrate effects
        CreatePreviewParticles();
        CreatePreviewGlowElements();
        CreatePreviewMotionElements();
    }

    private void CreatePreviewParticles()
    {
        for (int i = 0; i < 5; i++)
        {
            var particle = new Ellipse
            {
                Width = 4,
                Height = 4,
                Fill = new SolidColorBrush(Color.FromRgb(64, 224, 255)),
                Opacity = _intensityManager.GetEffectiveIntensity(AnimationComponent.Particles)
            };

            Canvas.SetLeft(particle, i * 20 + 10);
            Canvas.SetTop(particle, 50);
            _previewCanvas.Children.Add(particle);

            if (_intensityManager.IsFeatureEnabled(AnimationFeatures.ParticleEffects))
            {
                var moveAnimation = new DoubleAnimation
                {
                    From = i * 20 + 10,
                    To = 180,
                    Duration = TimeSpan.FromSeconds(2 + i * 0.3),
                    RepeatBehavior = RepeatBehavior.Forever
                };
                particle.BeginAnimation(Canvas.LeftProperty, moveAnimation);
            }
        }
    }

    private void CreatePreviewGlowElements()
    {
        var glowRect = new Rectangle
        {
            Width = 60,
            Height = 20,
            Fill = new SolidColorBrush(Color.FromRgb(255, 215, 0)),
            RadiusX = 10,
            RadiusY = 10
        };

        Canvas.SetLeft(glowRect, 220);
        Canvas.SetTop(glowRect, 40);

        if (_intensityManager.IsFeatureEnabled(AnimationFeatures.BasicGlow))
        {
            glowRect.Effect = new DropShadowEffect
            {
                Color = Color.FromRgb(255, 215, 0),
                BlurRadius = 10 * _intensityManager.GetEffectiveIntensity(AnimationComponent.Glow),
                ShadowDepth = 0,
                Opacity = 0.8 * _intensityManager.GetEffectiveIntensity(AnimationComponent.Glow)
            };
        }

        _previewCanvas.Children.Add(glowRect);
    }

    private void CreatePreviewMotionElements()
    {
        var motionElement = new Border
        {
            Width = 30,
            Height = 30,
            Background = new SolidColorBrush(Color.FromRgb(50, 205, 50)),
            CornerRadius = new CornerRadius(15)
        };

        Canvas.SetLeft(motionElement, 320);
        Canvas.SetTop(motionElement, 35);
        _previewCanvas.Children.Add(motionElement);

        if (_intensityManager.IsFeatureEnabled(AnimationFeatures.MotionEffects))
        {
            var rotateTransform = new RotateTransform();
            motionElement.RenderTransform = rotateTransform;
            motionElement.RenderTransformOrigin = new Point(0.5, 0.5);

            var rotationAnimation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(3 / _intensityManager.GetEffectiveIntensity(AnimationComponent.Motion)),
                RepeatBehavior = RepeatBehavior.Forever
            };
            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotationAnimation);
        }
    }

    private void UpdatePreview()
    {
        if (IsPreviewEnabled)
        {
            StartPreviewAnimations();
        }
    }

    private static void OnIsPreviewEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AnimationSettingsControl control)
        {
            if ((bool)e.NewValue)
                control.StartPreviewAnimations();
            else
                control._previewCanvas?.Children.Clear();
        }
    }

    private static void OnShowAdvancedOptionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AnimationSettingsControl control && control._advancedOptionsGrid != null)
        {
            control._advancedOptionsGrid.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    #endregion

    #region INotifyPropertyChanged

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}