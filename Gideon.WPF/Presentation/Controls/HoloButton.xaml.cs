using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace Gideon.WPF.Presentation.Controls
{
    public partial class HoloButton : UserControl
    {
        #region Dependency Properties

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(HoloButton),
                new PropertyMetadata("Button", OnTextChanged));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(object), typeof(HoloButton),
                new PropertyMetadata(null, OnIconChanged));

        public static readonly DependencyProperty ButtonStyleProperty =
            DependencyProperty.Register(nameof(ButtonStyle), typeof(HoloButtonStyle), typeof(HoloButton),
                new PropertyMetadata(HoloButtonStyle.Primary, OnButtonStyleChanged));

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(HoloButton),
                new PropertyMetadata(true, OnIsEnabledChanged));

        public static readonly DependencyProperty ShowCornerAccentsProperty =
            DependencyProperty.Register(nameof(ShowCornerAccents), typeof(bool), typeof(HoloButton),
                new PropertyMetadata(true, OnShowCornerAccentsChanged));

        public static readonly DependencyProperty BadgeTextProperty =
            DependencyProperty.Register(nameof(BadgeText), typeof(string), typeof(HoloButton),
                new PropertyMetadata(string.Empty, OnBadgeTextChanged));

        public static readonly DependencyProperty ShowBadgeProperty =
            DependencyProperty.Register(nameof(ShowBadge), typeof(bool), typeof(HoloButton),
                new PropertyMetadata(false, OnShowBadgeChanged));

        public static readonly DependencyProperty GlowIntensityProperty =
            DependencyProperty.Register(nameof(GlowIntensity), typeof(double), typeof(HoloButton),
                new PropertyMetadata(1.0, OnGlowIntensityChanged));

        public static readonly DependencyProperty EnablePulseProperty =
            DependencyProperty.Register(nameof(EnablePulse), typeof(bool), typeof(HoloButton),
                new PropertyMetadata(false, OnEnablePulseChanged));

        #endregion

        #region Properties

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public object Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public HoloButtonStyle ButtonStyle
        {
            get => (HoloButtonStyle)GetValue(ButtonStyleProperty);
            set => SetValue(ButtonStyleProperty, value);
        }

        public new bool IsEnabled
        {
            get => (bool)GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }

        public bool ShowCornerAccents
        {
            get => (bool)GetValue(ShowCornerAccentsProperty);
            set => SetValue(ShowCornerAccentsProperty, value);
        }

        public string BadgeText
        {
            get => (string)GetValue(BadgeTextProperty);
            set => SetValue(BadgeTextProperty, value);
        }

        public bool ShowBadge
        {
            get => (bool)GetValue(ShowBadgeProperty);
            set => SetValue(ShowBadgeProperty, value);
        }

        public double GlowIntensity
        {
            get => (double)GetValue(GlowIntensityProperty);
            set => SetValue(GlowIntensityProperty, value);
        }

        public bool EnablePulse
        {
            get => (bool)GetValue(EnablePulseProperty);
            set => SetValue(EnablePulseProperty, value);
        }

        #endregion

        #region Events

        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent(nameof(Click), RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(HoloButton));

        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        #endregion

        #region Fields

        private Storyboard _pulseStoryboard;
        private bool _isPressed = false;

        #endregion

        #region Constructor

        public HoloButton()
        {
            InitializeComponent();
            
            PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
            MouseLeave += OnMouseLeave;
            
            Loaded += OnLoaded;
        }

        #endregion

        #region Event Handlers

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ApplyButtonStyle();
            UpdateGlowEffects();
            
            if (EnablePulse)
                StartPulseAnimation();
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isPressed = true;
            CaptureMouse();
        }

        private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isPressed && IsMouseCaptured)
            {
                ReleaseMouseCapture();
                _isPressed = false;
                
                // Trigger click event
                var clickEventArgs = new RoutedEventArgs(ClickEvent, this);
                RaiseEvent(clickEventArgs);
            }
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (_isPressed)
            {
                _isPressed = false;
                ReleaseMouseCapture();
            }
        }

        #endregion

        #region Property Change Handlers

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HoloButton button && button.ContentText != null)
            {
                button.ContentText.Text = e.NewValue?.ToString() ?? string.Empty;
            }
        }

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HoloButton button && button.IconPresenter != null)
            {
                button.IconPresenter.Content = e.NewValue;
                button.IconPresenter.Visibility = e.NewValue != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private static void OnButtonStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HoloButton button)
            {
                button.ApplyButtonStyle();
            }
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HoloButton button)
            {
                button.UpdateEnabledState();
            }
        }

        private static void OnShowCornerAccentsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HoloButton button && button.CornerAccents != null)
            {
                button.CornerAccents.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private static void OnBadgeTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HoloButton button && button.BadgeText != null)
            {
                button.BadgeText = e.NewValue?.ToString() ?? string.Empty;
            }
        }

        private static void OnShowBadgeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HoloButton button && button.BadgeContainer != null)
            {
                button.BadgeContainer.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private static void OnGlowIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HoloButton button)
            {
                button.UpdateGlowEffects();
            }
        }

        private static void OnEnablePulseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HoloButton button)
            {
                if ((bool)e.NewValue)
                    button.StartPulseAnimation();
                else
                    button.StopPulseAnimation();
            }
        }

        #endregion

        #region Private Methods

        private void ApplyButtonStyle()
        {
            if (MainBorder == null) return;

            var (background, borderBrush, textColor, glowColor) = ButtonStyle switch
            {
                HoloButtonStyle.Primary => (
                    FindResource("HoloButtonBackground") as Brush,
                    FindResource("HoloButtonBorder") as Brush,
                    Color.FromRgb(0, 212, 255),
                    Color.FromRgb(0, 212, 255)
                ),
                HoloButtonStyle.Secondary => (
                    FindResource("HoloButtonBackground") as Brush,
                    FindResource("HoloButtonBorder") as Brush,
                    Color.FromRgb(128, 128, 128),
                    Color.FromRgb(128, 128, 128)
                ),
                HoloButtonStyle.Success => (
                    FindResource("HoloButtonBackground") as Brush,
                    new SolidColorBrush(Color.FromRgb(50, 205, 50)),
                    Color.FromRgb(50, 205, 50),
                    Color.FromRgb(50, 205, 50)
                ),
                HoloButtonStyle.Warning => (
                    FindResource("HoloButtonBackground") as Brush,
                    new SolidColorBrush(Color.FromRgb(255, 165, 0)),
                    Color.FromRgb(255, 165, 0),
                    Color.FromRgb(255, 165, 0)
                ),
                HoloButtonStyle.Danger => (
                    FindResource("HoloButtonBackground") as Brush,
                    new SolidColorBrush(Color.FromRgb(220, 20, 60)),
                    Color.FromRgb(220, 20, 60),
                    Color.FromRgb(220, 20, 60)
                ),
                HoloButtonStyle.EVE => (
                    FindResource("HoloButtonBackground") as Brush,
                    FindResource("EVEButtonBorder") as Brush,
                    Color.FromRgb(255, 215, 0),
                    Color.FromRgb(255, 215, 0)
                ),
                _ => (
                    FindResource("HoloButtonBackground") as Brush,
                    FindResource("HoloButtonBorder") as Brush,
                    Color.FromRgb(0, 212, 255),
                    Color.FromRgb(0, 212, 255)
                )
            };

            MainBorder.Background = background;
            MainBorder.BorderBrush = borderBrush;
            ContentText.Foreground = new SolidColorBrush(textColor);

            // Update glow effect
            if (MainBorder.Effect is DropShadowEffect glowEffect)
            {
                glowEffect.Color = glowColor;
            }
        }

        private void UpdateEnabledState()
        {
            if (MainBorder == null) return;

            if (IsEnabled)
            {
                Opacity = 1.0;
                MainBorder.Effect = FindResource("HoloButtonGlow") as DropShadowEffect;
            }
            else
            {
                Opacity = 0.5;
                MainBorder.Effect = FindResource("DisabledGlow") as DropShadowEffect;
            }
        }

        private void UpdateGlowEffects()
        {
            if (MainBorder?.Effect is DropShadowEffect effect)
            {
                effect.Opacity *= GlowIntensity;
                effect.BlurRadius *= GlowIntensity;
            }

            if (GlowRing?.Effect is DropShadowEffect glowRingEffect)
            {
                glowRingEffect.Opacity *= GlowIntensity;
                glowRingEffect.BlurRadius *= GlowIntensity;
            }
        }

        private void StartPulseAnimation()
        {
            if (PulseRing == null) return;

            PulseRing.Visibility = Visibility.Visible;
            _pulseStoryboard = FindResource("PulseStoryboard") as Storyboard;
            _pulseStoryboard?.Begin(this);
        }

        private void StopPulseAnimation()
        {
            _pulseStoryboard?.Stop(this);
            if (PulseRing != null)
                PulseRing.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Programmatically trigger a click
        /// </summary>
        public void PerformClick()
        {
            if (!IsEnabled) return;

            var clickEventArgs = new RoutedEventArgs(ClickEvent, this);
            RaiseEvent(clickEventArgs);
        }

        /// <summary>
        /// Flash the button for notifications
        /// </summary>
        public void Flash(Color color, double duration = 0.5)
        {
            if (MainBorder == null) return;

            var originalEffect = MainBorder.Effect;
            var flashEffect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 20,
                ShadowDepth = 0,
                Opacity = 1.0
            };

            MainBorder.Effect = flashEffect;

            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(duration)
            };
            timer.Tick += (s, e) =>
            {
                MainBorder.Effect = originalEffect;
                timer.Stop();
            };
            timer.Start();
        }

        /// <summary>
        /// Set loading state with animation
        /// </summary>
        public void SetLoadingState(bool isLoading)
        {
            if (isLoading)
            {
                Text = "Loading...";
                IsEnabled = false;
                EnablePulse = true;
            }
            else
            {
                IsEnabled = true;
                EnablePulse = false;
            }
        }

        /// <summary>
        /// Animate button appearance on first load
        /// </summary>
        public void AnimateIn(double delay = 0)
        {
            var slideIn = new DoubleAnimation
            {
                From = -50,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.4),
                BeginTime = TimeSpan.FromSeconds(delay),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.4),
                BeginTime = TimeSpan.FromSeconds(delay),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var transform = new TranslateTransform();
            RenderTransform = transform;

            transform.BeginAnimation(TranslateTransform.XProperty, slideIn);
            BeginAnimation(OpacityProperty, fadeIn);
        }

        #endregion
    }

    /// <summary>
    /// Available button style variants
    /// </summary>
    public enum HoloButtonStyle
    {
        Primary,
        Secondary,
        Success,
        Warning,
        Danger,
        EVE
    }
}