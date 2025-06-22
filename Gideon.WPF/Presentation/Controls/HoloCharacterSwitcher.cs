// ==========================================================================
// HoloCharacterSwitcher.cs - Character Switching with Fluid Particle Animations
// ==========================================================================
// Advanced character switching interface featuring fluid particle transitions,
// seamless character swapping, and holographic visual effects.
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Character switching interface with fluid particle animations and seamless transitions
/// </summary>
public class HoloCharacterSwitcher : Control, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloCharacterSwitcher),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloCharacterSwitcher),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty AvailableCharactersProperty =
        DependencyProperty.Register(nameof(AvailableCharacters), typeof(ObservableCollection<HoloCharacterInfo>), typeof(HoloCharacterSwitcher),
            new PropertyMetadata(null, OnAvailableCharactersChanged));

    public static readonly DependencyProperty CurrentCharacterProperty =
        DependencyProperty.Register(nameof(CurrentCharacter), typeof(HoloCharacterInfo), typeof(HoloCharacterSwitcher),
            new PropertyMetadata(null, OnCurrentCharacterChanged));

    public static readonly DependencyProperty EnableFluidAnimationsProperty =
        DependencyProperty.Register(nameof(EnableFluidAnimations), typeof(bool), typeof(HoloCharacterSwitcher),
            new PropertyMetadata(true, OnEnableFluidAnimationsChanged));

    public static readonly DependencyProperty EnableParticleTransitionsProperty =
        DependencyProperty.Register(nameof(EnableParticleTransitions), typeof(bool), typeof(HoloCharacterSwitcher),
            new PropertyMetadata(true, OnEnableParticleTransitionsChanged));

    public static readonly DependencyProperty SwitchingModeProperty =
        DependencyProperty.Register(nameof(SwitchingMode), typeof(CharacterSwitchMode), typeof(HoloCharacterSwitcher),
            new PropertyMetadata(CharacterSwitchMode.Dropdown));

    public static readonly DependencyProperty ShowQuickSwitchProperty =
        DependencyProperty.Register(nameof(ShowQuickSwitch), typeof(bool), typeof(HoloCharacterSwitcher),
            new PropertyMetadata(true));

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

    public ObservableCollection<HoloCharacterInfo> AvailableCharacters
    {
        get => (ObservableCollection<HoloCharacterInfo>)GetValue(AvailableCharactersProperty);
        set => SetValue(AvailableCharactersProperty, value);
    }

    public HoloCharacterInfo CurrentCharacter
    {
        get => (HoloCharacterInfo)GetValue(CurrentCharacterProperty);
        set => SetValue(CurrentCharacterProperty, value);
    }

    public bool EnableFluidAnimations
    {
        get => (bool)GetValue(EnableFluidAnimationsProperty);
        set => SetValue(EnableFluidAnimationsProperty, value);
    }

    public bool EnableParticleTransitions
    {
        get => (bool)GetValue(EnableParticleTransitionsProperty);
        set => SetValue(EnableParticleTransitionsProperty, value);
    }

    public CharacterSwitchMode SwitchingMode
    {
        get => (CharacterSwitchMode)GetValue(SwitchingModeProperty);
        set => SetValue(SwitchingModeProperty, value);
    }

    public bool ShowQuickSwitch
    {
        get => (bool)GetValue(ShowQuickSwitchProperty);
        set => SetValue(ShowQuickSwitchProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloCharacterSwitchEventArgs> CharacterSwitchRequested;
    public event EventHandler<HoloCharacterSwitchEventArgs> CharacterSwitchStarted;
    public event EventHandler<HoloCharacterSwitchEventArgs> CharacterSwitchCompleted;
    public event EventHandler<HoloCharacterSwitchEventArgs> CharacterSwitchFailed;

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode { get; private set; }

    public void SwitchToFullMode()
    {
        IsInSimplifiedMode = false;
        EnableFluidAnimations = true;
        EnableParticleTransitions = true;
        UpdateSwitcherAppearance();
    }

    public void SwitchToSimplifiedMode()
    {
        IsInSimplifiedMode = true;
        EnableFluidAnimations = false;
        EnableParticleTransitions = false;
        UpdateSwitcherAppearance();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => !_disposed && IsLoaded;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        EnableFluidAnimations = settings.EnabledFeatures.HasFlag(AnimationFeatures.ComplexTransitions);
        EnableParticleTransitions = settings.EnabledFeatures.HasFlag(AnimationFeatures.ParticleEffects);
        
        UpdateSwitcherAppearance();
    }

    #endregion

    #region Fields

    private Grid _switcherGrid;
    private Border _currentCharacterPanel;
    private Image _currentPortrait;
    private TextBlock _currentName;
    private TextBlock _currentCorporation;
    private Button _switchButton;
    private ComboBox _characterDropdown;
    private StackPanel _quickSwitchPanel;
    private Canvas _transitionCanvas;
    private Canvas _particleCanvas;
    
    private readonly Dictionary<HoloCharacterInfo, Button> _quickSwitchButtons = new();
    private readonly List<TransitionParticle> _transitionParticles = new();
    private DispatcherTimer _particleTimer;
    private DispatcherTimer _transitionTimer;
    private double _particlePhase = 0;
    private double _transitionPhase = 0;
    private readonly Random _random = new();
    private bool _disposed = false;
    private bool _isSwitching = false;
    private HoloCharacterInfo _targetCharacter = null;

    #endregion

    #region Constructor

    public HoloCharacterSwitcher()
    {
        DefaultStyleKey = typeof(HoloCharacterSwitcher);
        AvailableCharacters = new ObservableCollection<HoloCharacterInfo>();
        Width = 300;
        Height = 100;
        InitializeSwitcher();
        SetupAnimations();
        
        // Register with animation intensity manager
        AnimationIntensityManager.Instance.RegisterTarget(this);
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Switch to a specific character with animation
    /// </summary>
    public async Task SwitchToCharacterAsync(HoloCharacterInfo character)
    {
        if (_isSwitching || character == null || character == CurrentCharacter) return;
        if (!AvailableCharacters.Contains(character)) return;

        _isSwitching = true;
        _targetCharacter = character;

        CharacterSwitchStarted?.Invoke(this, new HoloCharacterSwitchEventArgs 
        { 
            FromCharacter = CurrentCharacter,
            ToCharacter = character,
            SwitchMode = SwitchingMode
        });

        try
        {
            if (EnableFluidAnimations && !IsInSimplifiedMode)
            {
                await AnimateCharacterTransitionAsync();
            }
            else
            {
                CurrentCharacter = character;
            }

            CharacterSwitchCompleted?.Invoke(this, new HoloCharacterSwitchEventArgs 
            { 
                FromCharacter = CurrentCharacter,
                ToCharacter = character,
                SwitchMode = SwitchingMode
            });
        }
        catch (Exception ex)
        {
            CharacterSwitchFailed?.Invoke(this, new HoloCharacterSwitchEventArgs 
            { 
                FromCharacter = CurrentCharacter,
                ToCharacter = character,
                SwitchMode = SwitchingMode,
                ErrorMessage = ex.Message
            });
        }
        finally
        {
            _isSwitching = false;
            _targetCharacter = null;
        }
    }

    /// <summary>
    /// Add character to available list
    /// </summary>
    public void AddCharacter(HoloCharacterInfo character)
    {
        if (character != null && !AvailableCharacters.Contains(character))
        {
            AvailableCharacters.Add(character);
        }
    }

    /// <summary>
    /// Remove character from available list
    /// </summary>
    public void RemoveCharacter(HoloCharacterInfo character)
    {
        if (character != null)
        {
            AvailableCharacters.Remove(character);
            if (CurrentCharacter == character)
            {
                CurrentCharacter = AvailableCharacters.FirstOrDefault();
            }
        }
    }

    /// <summary>
    /// Get next character in rotation
    /// </summary>
    public HoloCharacterInfo GetNextCharacter()
    {
        if (AvailableCharacters.Count <= 1) return CurrentCharacter;

        var currentIndex = AvailableCharacters.IndexOf(CurrentCharacter);
        var nextIndex = (currentIndex + 1) % AvailableCharacters.Count;
        return AvailableCharacters[nextIndex];
    }

    /// <summary>
    /// Get previous character in rotation
    /// </summary>
    public HoloCharacterInfo GetPreviousCharacter()
    {
        if (AvailableCharacters.Count <= 1) return CurrentCharacter;

        var currentIndex = AvailableCharacters.IndexOf(CurrentCharacter);
        var prevIndex = currentIndex == 0 ? AvailableCharacters.Count - 1 : currentIndex - 1;
        return AvailableCharacters[prevIndex];
    }

    #endregion

    #region Private Methods

    private void InitializeSwitcher()
    {
        Template = CreateSwitcherTemplate();
        UpdateSwitcherAppearance();
    }

    private ControlTemplate CreateSwitcherTemplate()
    {
        var template = new ControlTemplate(typeof(HoloCharacterSwitcher));

        // Main switcher grid
        var switcherGrid = new FrameworkElementFactory(typeof(Grid));
        switcherGrid.Name = "PART_SwitcherGrid";

        // Row definitions
        var mainRow = new FrameworkElementFactory(typeof(RowDefinition));
        mainRow.SetValue(RowDefinition.HeightProperty, GridLength.Auto);
        var quickSwitchRow = new FrameworkElementFactory(typeof(RowDefinition));
        quickSwitchRow.SetValue(RowDefinition.HeightProperty, GridLength.Auto);

        switcherGrid.AppendChild(mainRow);
        switcherGrid.AppendChild(quickSwitchRow);

        // Current character panel
        var currentCharacterPanel = new FrameworkElementFactory(typeof(Border));
        currentCharacterPanel.Name = "PART_CurrentCharacterPanel";
        currentCharacterPanel.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
        currentCharacterPanel.SetValue(Border.BorderThicknessProperty, new Thickness(2));
        currentCharacterPanel.SetValue(Border.PaddingProperty, new Thickness(12));
        currentCharacterPanel.SetValue(Grid.RowProperty, 0);

        // Character info grid
        var characterGrid = new FrameworkElementFactory(typeof(Grid));
        
        // Column definitions for character info
        var portraitColumn = new FrameworkElementFactory(typeof(ColumnDefinition));
        portraitColumn.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);
        var infoColumn = new FrameworkElementFactory(typeof(ColumnDefinition));
        infoColumn.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
        var switchColumn = new FrameworkElementFactory(typeof(ColumnDefinition));
        switchColumn.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);

        characterGrid.AppendChild(portraitColumn);
        characterGrid.AppendChild(infoColumn);
        characterGrid.AppendChild(switchColumn);

        // Current portrait
        var currentPortrait = new FrameworkElementFactory(typeof(Image));
        currentPortrait.Name = "PART_CurrentPortrait";
        currentPortrait.SetValue(Image.WidthProperty, 48.0);
        currentPortrait.SetValue(Image.HeightProperty, 48.0);
        currentPortrait.SetValue(Image.StretchProperty, Stretch.UniformToFill);
        currentPortrait.SetValue(Image.MarginProperty, new Thickness(0, 0, 12, 0));
        currentPortrait.SetValue(Grid.ColumnProperty, 0);

        // Character info stack
        var characterInfoStack = new FrameworkElementFactory(typeof(StackPanel));
        characterInfoStack.SetValue(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Center);
        characterInfoStack.SetValue(Grid.ColumnProperty, 1);

        // Current name
        var currentName = new FrameworkElementFactory(typeof(TextBlock));
        currentName.Name = "PART_CurrentName";
        currentName.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        currentName.SetValue(TextBlock.FontSizeProperty, 14.0);

        // Current corporation
        var currentCorporation = new FrameworkElementFactory(typeof(TextBlock));
        currentCorporation.Name = "PART_CurrentCorporation";
        currentCorporation.SetValue(TextBlock.FontSizeProperty, 11.0);
        currentCorporation.SetValue(TextBlock.OpacityProperty, 0.8);

        characterInfoStack.AppendChild(currentName);
        characterInfoStack.AppendChild(currentCorporation);

        // Switch button or dropdown
        var switchButton = new FrameworkElementFactory(typeof(Button));
        switchButton.Name = "PART_SwitchButton";
        switchButton.SetValue(Button.WidthProperty, 32.0);
        switchButton.SetValue(Button.HeightProperty, 32.0);
        switchButton.SetValue(Button.ContentProperty, "⟲");
        switchButton.SetValue(Button.FontSizeProperty, 16.0);
        switchButton.SetValue(Grid.ColumnProperty, 2);

        var characterDropdown = new FrameworkElementFactory(typeof(ComboBox));
        characterDropdown.Name = "PART_CharacterDropdown";
        characterDropdown.SetValue(ComboBox.WidthProperty, 120.0);
        characterDropdown.SetValue(ComboBox.VisibilityProperty, Visibility.Collapsed);
        characterDropdown.SetValue(Grid.ColumnProperty, 2);

        // Assembly character panel
        characterGrid.AppendChild(currentPortrait);
        characterGrid.AppendChild(characterInfoStack);
        characterGrid.AppendChild(switchButton);
        characterGrid.AppendChild(characterDropdown);
        currentCharacterPanel.AppendChild(characterGrid);

        // Quick switch panel
        var quickSwitchPanel = new FrameworkElementFactory(typeof(StackPanel));
        quickSwitchPanel.Name = "PART_QuickSwitchPanel";
        quickSwitchPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        quickSwitchPanel.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        quickSwitchPanel.SetValue(StackPanel.MarginProperty, new Thickness(0, 8, 0, 0));
        quickSwitchPanel.SetValue(Grid.RowProperty, 1);

        // Transition canvas for animations
        var transitionCanvas = new FrameworkElementFactory(typeof(Canvas));
        transitionCanvas.Name = "PART_TransitionCanvas";
        transitionCanvas.SetValue(Canvas.IsHitTestVisibleProperty, false);
        transitionCanvas.SetValue(Canvas.ClipToBoundsProperty, true);
        transitionCanvas.SetValue(Grid.RowSpanProperty, 2);

        // Particle canvas for effects
        var particleCanvas = new FrameworkElementFactory(typeof(Canvas));
        particleCanvas.Name = "PART_ParticleCanvas";
        particleCanvas.SetValue(Canvas.IsHitTestVisibleProperty, false);
        particleCanvas.SetValue(Canvas.ClipToBoundsProperty, true);
        particleCanvas.SetValue(Grid.RowSpanProperty, 2);

        // Assembly
        switcherGrid.AppendChild(currentCharacterPanel);
        switcherGrid.AppendChild(quickSwitchPanel);
        switcherGrid.AppendChild(transitionCanvas);
        switcherGrid.AppendChild(particleCanvas);

        template.VisualTree = switcherGrid;
        return template;
    }

    private void SetupAnimations()
    {
        // Particle animation timer
        _particleTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50) // 20 FPS
        };
        _particleTimer.Tick += OnParticleTick;

        // Transition animation timer
        _transitionTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
        };
        _transitionTimer.Tick += OnTransitionTick;
    }

    private void OnParticleTick(object sender, EventArgs e)
    {
        if (!EnableParticleTransitions || IsInSimplifiedMode || _particleCanvas == null) return;

        _particlePhase += 0.1;
        if (_particlePhase > Math.PI * 2)
            _particlePhase = 0;

        UpdateTransitionParticles();
        
        if (_isSwitching)
        {
            SpawnTransitionParticles();
        }
    }

    private void OnTransitionTick(object sender, EventArgs e)
    {
        if (!EnableFluidAnimations || IsInSimplifiedMode) return;

        _transitionPhase += 0.1;
        if (_transitionPhase > Math.PI * 2)
            _transitionPhase = 0;

        UpdateTransitionEffects();
    }

    private void UpdateTransitionParticles()
    {
        for (int i = _transitionParticles.Count - 1; i >= 0; i--)
        {
            var particle = _transitionParticles[i];
            
            // Update particle position with fluid motion
            particle.X += particle.VelocityX;
            particle.Y += particle.VelocityY;
            particle.Life -= 0.015;

            // Add swirl effect
            var swirlFactor = Math.Sin(particle.Life * Math.PI * 4) * 2;
            particle.X += Math.Cos(particle.Phase) * swirlFactor;
            particle.Y += Math.Sin(particle.Phase) * swirlFactor;
            particle.Phase += 0.1;

            // Update visual position
            Canvas.SetLeft(particle.Visual, particle.X);
            Canvas.SetTop(particle.Visual, particle.Y);
            
            // Update opacity and scale
            particle.Visual.Opacity = Math.Max(0, particle.Life) * HolographicIntensity;
            var scale = 0.5 + (particle.Life * 0.5);
            particle.Visual.RenderTransform = new ScaleTransform(scale, scale);

            // Remove dead particles
            if (particle.Life <= 0 || particle.X < -50 || particle.X > ActualWidth + 50 ||
                particle.Y < -50 || particle.Y > ActualHeight + 50)
            {
                _particleCanvas.Children.Remove(particle.Visual);
                _transitionParticles.RemoveAt(i);
            }
        }
    }

    private void SpawnTransitionParticles()
    {
        if (_transitionParticles.Count >= 30) return; // Limit particle count

        if (_random.NextDouble() < 0.3) // 30% chance to spawn during transition
        {
            var particle = CreateTransitionParticle();
            _transitionParticles.Add(particle);
            _particleCanvas.Children.Add(particle.Visual);
        }
    }

    private TransitionParticle CreateTransitionParticle()
    {
        var color = GetEVEColor(EVEColorScheme);
        var size = 2 + _random.NextDouble() * 4;
        
        var shape = _random.NextDouble() < 0.5 ? 
            CreateParticleEllipse(size, color) : 
            CreateParticleRectangle(size, color);

        var startX = _currentPortrait?.ActualWidth ?? 48;
        var startY = (_currentPortrait?.ActualHeight ?? 48) / 2;

        var particle = new TransitionParticle
        {
            Visual = shape,
            X = startX,
            Y = startY,
            VelocityX = (_random.NextDouble() - 0.5) * 8,
            VelocityY = (_random.NextDouble() - 0.5) * 8,
            Life = 1.0,
            Phase = _random.NextDouble() * Math.PI * 2
        };

        Canvas.SetLeft(shape, particle.X);
        Canvas.SetTop(shape, particle.Y);

        return particle;
    }

    private FrameworkElement CreateParticleEllipse(double size, Color color)
    {
        return new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromArgb(150, color.R, color.G, color.B)),
            Stroke = new SolidColorBrush(Color.FromArgb(200, color.R, color.G, color.B)),
            StrokeThickness = 0.5
        };
    }

    private FrameworkElement CreateParticleRectangle(double size, Color color)
    {
        return new Rectangle
        {
            Width = size * 0.8,
            Height = size * 0.8,
            Fill = new SolidColorBrush(Color.FromArgb(100, color.R, color.G, color.B)),
            Stroke = new SolidColorBrush(Color.FromArgb(180, color.R, color.G, color.B)),
            StrokeThickness = 0.5,
            RadiusX = 1,
            RadiusY = 1
        };
    }

    private void UpdateTransitionEffects()
    {
        if (_currentCharacterPanel?.Effect is DropShadowEffect effect && _isSwitching)
        {
            var transitionIntensity = 0.8 + (Math.Sin(_transitionPhase * 3) * 0.4);
            effect.Opacity = HolographicIntensity * transitionIntensity;
            effect.BlurRadius = 10 + (Math.Sin(_transitionPhase * 2) * 5);
        }
    }

    private async Task AnimateCharacterTransitionAsync()
    {
        if (_targetCharacter == null) return;

        // Start transition effects
        if (EnableParticleTransitions && !IsInSimplifiedMode)
        {
            _particleTimer.Start();
        }

        // Fade out current character
        var fadeOutAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.3,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        _currentCharacterPanel?.BeginAnimation(OpacityProperty, fadeOutAnimation);

        // Wait for fade out
        await Task.Delay(200);

        // Update character data
        CurrentCharacter = _targetCharacter;

        // Fade in new character
        var fadeInAnimation = new DoubleAnimation
        {
            From = 0.3,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        _currentCharacterPanel?.BeginAnimation(OpacityProperty, fadeInAnimation);

        // Wait for transition to complete
        await Task.Delay(500);

        // Stop particle effects
        _particleTimer.Stop();
        
        // Clear remaining particles
        _transitionParticles.Clear();
        _particleCanvas?.Children.Clear();
    }

    private void UpdateSwitcherAppearance()
    {
        if (Template == null) return;

        Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            ApplyTemplate();
            
            _switcherGrid = GetTemplateChild("PART_SwitcherGrid") as Grid;
            _currentCharacterPanel = GetTemplateChild("PART_CurrentCharacterPanel") as Border;
            _currentPortrait = GetTemplateChild("PART_CurrentPortrait") as Image;
            _currentName = GetTemplateChild("PART_CurrentName") as TextBlock;
            _currentCorporation = GetTemplateChild("PART_CurrentCorporation") as TextBlock;
            _switchButton = GetTemplateChild("PART_SwitchButton") as Button;
            _characterDropdown = GetTemplateChild("PART_CharacterDropdown") as ComboBox;
            _quickSwitchPanel = GetTemplateChild("PART_QuickSwitchPanel") as StackPanel;
            _transitionCanvas = GetTemplateChild("PART_TransitionCanvas") as Canvas;
            _particleCanvas = GetTemplateChild("PART_ParticleCanvas") as Canvas;

            UpdateColors();
            UpdateEffects();
            UpdateCurrentCharacterDisplay();
            UpdateCharacterControls();
            SetupEventHandlers();
        }), DispatcherPriority.Render);
    }

    private void UpdateColors()
    {
        var color = GetEVEColor(EVEColorScheme);

        // Current character panel colors
        if (_currentCharacterPanel != null)
        {
            var backgroundBrush = new LinearGradientBrush();
            backgroundBrush.StartPoint = new Point(0, 0);
            backgroundBrush.EndPoint = new Point(1, 0);
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(120, 0, 20, 40), 0.0));
            backgroundBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(100, 0, 15, 30), 1.0));

            _currentCharacterPanel.Background = backgroundBrush;
            _currentCharacterPanel.BorderBrush = new SolidColorBrush(Color.FromArgb(
                150, color.R, color.G, color.B));
        }

        // Text colors
        if (_currentName != null)
        {
            _currentName.Foreground = new SolidColorBrush(Colors.White);
        }

        if (_currentCorporation != null)
        {
            _currentCorporation.Foreground = new SolidColorBrush(color);
        }
    }

    private void UpdateEffects()
    {
        var color = GetEVEColor(EVEColorScheme);

        if (_currentCharacterPanel != null && !IsInSimplifiedMode)
        {
            _currentCharacterPanel.Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 8 * HolographicIntensity,
                ShadowDepth = 0,
                Opacity = 0.5 * HolographicIntensity
            };
        }

        // Portrait clip for rounded effect
        if (_currentPortrait != null)
        {
            _currentPortrait.Clip = new EllipseGeometry
            {
                Center = new Point(24, 24),
                RadiusX = 24,
                RadiusY = 24
            };
        }
    }

    private void UpdateCurrentCharacterDisplay()
    {
        if (CurrentCharacter == null) return;

        if (_currentPortrait != null)
        {
            _currentPortrait.Source = CurrentCharacter.Portrait;
        }

        if (_currentName != null)
        {
            _currentName.Text = CurrentCharacter.Name ?? "Unknown Pilot";
        }

        if (_currentCorporation != null)
        {
            _currentCorporation.Text = CurrentCharacter.Corporation ?? "Unknown Corporation";
        }
    }

    private void UpdateCharacterControls()
    {
        // Update dropdown visibility
        var useDropdown = SwitchingMode == CharacterSwitchMode.Dropdown;
        
        if (_switchButton != null)
        {
            _switchButton.Visibility = useDropdown ? Visibility.Collapsed : Visibility.Visible;
        }

        if (_characterDropdown != null)
        {
            _characterDropdown.Visibility = useDropdown ? Visibility.Visible : Visibility.Collapsed;
            
            if (useDropdown)
            {
                _characterDropdown.Items.Clear();
                foreach (var character in AvailableCharacters)
                {
                    _characterDropdown.Items.Add(character.Name);
                }
                
                if (CurrentCharacter != null)
                {
                    _characterDropdown.SelectedItem = CurrentCharacter.Name;
                }
            }
        }

        // Update quick switch panel
        UpdateQuickSwitchPanel();
    }

    private void UpdateQuickSwitchPanel()
    {
        if (_quickSwitchPanel == null) return;

        _quickSwitchPanel.Children.Clear();
        _quickSwitchButtons.Clear();
        
        if (!ShowQuickSwitch || AvailableCharacters.Count <= 1) return;

        foreach (var character in AvailableCharacters)
        {
            if (character == CurrentCharacter) continue;

            var button = new Button
            {
                Width = 32,
                Height = 32,
                Margin = new Thickness(2),
                ToolTip = character.Name,
                Tag = character
            };

            // Create portrait content
            if (character.Portrait != null)
            {
                var image = new Image
                {
                    Source = character.Portrait,
                    Stretch = Stretch.UniformToFill,
                    Clip = new EllipseGeometry
                    {
                        Center = new Point(16, 16),
                        RadiusX = 15,
                        RadiusY = 15
                    }
                };
                button.Content = image;
            }
            else
            {
                button.Content = character.Name?.Substring(0, Math.Min(2, character.Name.Length)).ToUpper();
            }

            button.Click += OnQuickSwitchButtonClick;
            _quickSwitchButtons[character] = button;
            _quickSwitchPanel.Children.Add(button);
        }
    }

    private void SetupEventHandlers()
    {
        if (_switchButton != null)
        {
            _switchButton.Click -= OnSwitchButtonClick;
            _switchButton.Click += OnSwitchButtonClick;
        }

        if (_characterDropdown != null)
        {
            _characterDropdown.SelectionChanged -= OnDropdownSelectionChanged;
            _characterDropdown.SelectionChanged += OnDropdownSelectionChanged;
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
            _ => Color.FromRgb(64, 224, 255)
        };
    }

    private void OnSwitchButtonClick(object sender, RoutedEventArgs e)
    {
        var nextCharacter = GetNextCharacter();
        if (nextCharacter != null && nextCharacter != CurrentCharacter)
        {
            CharacterSwitchRequested?.Invoke(this, new HoloCharacterSwitchEventArgs 
            { 
                FromCharacter = CurrentCharacter,
                ToCharacter = nextCharacter,
                SwitchMode = SwitchingMode
            });
            
            _ = SwitchToCharacterAsync(nextCharacter);
        }
    }

    private void OnQuickSwitchButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is HoloCharacterInfo character)
        {
            CharacterSwitchRequested?.Invoke(this, new HoloCharacterSwitchEventArgs 
            { 
                FromCharacter = CurrentCharacter,
                ToCharacter = character,
                SwitchMode = CharacterSwitchMode.QuickSwitch
            });
            
            _ = SwitchToCharacterAsync(character);
        }
    }

    private void OnDropdownSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_characterDropdown.SelectedItem is string selectedName)
        {
            var character = AvailableCharacters.FirstOrDefault(c => c.Name == selectedName);
            if (character != null && character != CurrentCharacter)
            {
                CharacterSwitchRequested?.Invoke(this, new HoloCharacterSwitchEventArgs 
                { 
                    FromCharacter = CurrentCharacter,
                    ToCharacter = character,
                    SwitchMode = CharacterSwitchMode.Dropdown
                });
                
                _ = SwitchToCharacterAsync(character);
            }
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableFluidAnimations && !IsInSimplifiedMode)
            _transitionTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _particleTimer?.Stop();
        _transitionTimer?.Stop();
        
        // Clean up particles
        _transitionParticles.Clear();
        _particleCanvas?.Children.Clear();
        _transitionCanvas?.Children.Clear();
        
        // Unregister from animation intensity manager
        AnimationIntensityManager.Instance.UnregisterTarget(this);
        
        _disposed = true;
    }

    #endregion

    #region Event Handlers

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSwitcher switcher)
            switcher.UpdateSwitcherAppearance();
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSwitcher switcher)
            switcher.UpdateSwitcherAppearance();
    }

    private static void OnAvailableCharactersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSwitcher switcher)
        {
            if (e.OldValue is ObservableCollection<HoloCharacterInfo> oldCollection)
                oldCollection.CollectionChanged -= switcher.OnAvailableCharactersCollectionChanged;

            if (e.NewValue is ObservableCollection<HoloCharacterInfo> newCollection)
                newCollection.CollectionChanged += switcher.OnAvailableCharactersCollectionChanged;

            switcher.UpdateCharacterControls();
        }
    }

    private void OnAvailableCharactersCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdateCharacterControls();
    }

    private static void OnCurrentCharacterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSwitcher switcher)
        {
            switcher.UpdateCurrentCharacterDisplay();
            switcher.UpdateQuickSwitchPanel();
        }
    }

    private static void OnEnableFluidAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSwitcher switcher)
        {
            if ((bool)e.NewValue && !switcher.IsInSimplifiedMode)
                switcher._transitionTimer.Start();
            else
                switcher._transitionTimer.Stop();
        }
    }

    private static void OnEnableParticleTransitionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloCharacterSwitcher switcher)
        {
            if (!(bool)e.NewValue)
            {
                switcher._particleTimer.Stop();
                switcher._transitionParticles.Clear();
                switcher._particleCanvas?.Children.Clear();
            }
        }
    }

    #endregion
}

/// <summary>
/// Transition particle for fluid animations
/// </summary>
internal class TransitionParticle
{
    public FrameworkElement Visual { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Life { get; set; }
    public double Phase { get; set; }
}

/// <summary>
/// Character switching modes
/// </summary>
public enum CharacterSwitchMode
{
    Button,
    Dropdown,
    QuickSwitch,
    Keyboard
}

/// <summary>
/// Event args for character switch events
/// </summary>
public class HoloCharacterSwitchEventArgs : EventArgs
{
    public HoloCharacterInfo FromCharacter { get; set; }
    public HoloCharacterInfo ToCharacter { get; set; }
    public CharacterSwitchMode SwitchMode { get; set; }
    public string ErrorMessage { get; set; }
}