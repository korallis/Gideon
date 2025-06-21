// ==========================================================================
// HolographicDepthDemo.xaml.cs - Holographic Depth Demo Page Code-Behind
// ==========================================================================
// Interactive demonstration page for the EVE holographic depth perception
// system with real-time depth manipulation and layer management controls.
//
// Features:
// - Real-time depth adjustment
// - Layer transition animations
// - Parallax effect controls
// - Performance optimization
// - Interactive depth elements
//
// Author: Gideon Development Team
// Created: June 21, 2025
// ==========================================================================

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Gideon.WPF.Presentation.Controls;

namespace Gideon.WPF.Presentation.Views;

/// <summary>
/// Interactive demonstration page for EVE holographic depth system
/// </summary>
public partial class HolographicDepthDemo : Page
{
    #region Fields

    private DepthTransitionType _currentTransitionType = DepthTransitionType.None;
    private bool _isElementFocused = false;

    #endregion

    #region Constructor

    public HolographicDepthDemo()
    {
        InitializeComponent();
        InitializeControls();
    }

    #endregion

    #region Initialization

    private void InitializeControls()
    {
        // Set initial control states
        if (DepthLevelCombo != null)
        {
            DepthLevelCombo.SelectedIndex = 3; // MidLayer
        }

        if (TransitionTypeCombo != null)
        {
            TransitionTypeCombo.SelectedIndex = 0; // None
        }

        // Initialize slider values
        UpdateSliderDisplays();
        
        // Set up movable element
        SetupMovableElement();
        
        // Update status
        UpdateStatusMessage("Holographic depth system initialized - Ready for depth manipulation");
    }

    private void SetupMovableElement()
    {
        if (MovableElement != null)
        {
            // Set initial depth for the movable element
            HolographicDepthPanel.SetDepthLevel(MovableElement, HolographicDepthLevel.MidLayer);
            HolographicDepthPanel.SetDepthTransition(MovableElement, DepthTransitionType.None);
        }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handles depth slider changes
    /// </summary>
    private void OnDepthChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (MovableElement == null || DepthDemoPanel == null) return;

        var newDepth = e.NewValue;
        HolographicDepthPanel.SetDepth(MovableElement, newDepth);
        
        UpdateDepthDisplay();
        UpdateMovableElementDepthText();
        UpdateStatusMessage($"Element depth set to {newDepth:F2}");
    }

    /// <summary>
    /// Handles depth level selection changes
    /// </summary>
    private void OnDepthLevelChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MovableElement == null || DepthDemoPanel == null || DepthLevelCombo?.SelectedItem is not ComboBoxItem selectedItem)
            return;

        var levelText = selectedItem.Content.ToString();
        if (Enum.TryParse<HolographicDepthLevel>(levelText, out var level))
        {
            // Animate to new depth level if transition is enabled
            if (_currentTransitionType != DepthTransitionType.None)
            {
                var numericDepth = ConvertDepthLevelToNumeric(level);
                DepthDemoPanel.AnimateElementToDepth(MovableElement, numericDepth, _currentTransitionType);
                
                // Update slider to match
                if (DepthSlider != null)
                {
                    DepthSlider.Value = numericDepth;
                }
            }
            else
            {
                HolographicDepthPanel.SetDepthLevel(MovableElement, level);
            }
            
            UpdateStatusMessage($"Element depth level changed to {levelText}");
        }
    }

    /// <summary>
    /// Handles transition type selection changes
    /// </summary>
    private void OnTransitionTypeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TransitionTypeCombo?.SelectedItem is not ComboBoxItem selectedItem)
            return;

        var transitionText = selectedItem.Content.ToString();
        if (Enum.TryParse<DepthTransitionType>(transitionText, out var transition))
        {
            _currentTransitionType = transition;
            UpdateStatusMessage($"Transition type set to {transitionText}");
        }
    }

    /// <summary>
    /// Handles depth intensity slider changes
    /// </summary>
    private void OnIntensityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (DepthDemoPanel == null) return;

        DepthDemoPanel.DepthIntensity = e.NewValue;
        UpdateIntensityDisplay();
        UpdateStatusMessage($"Depth intensity set to {e.NewValue:F1}");
    }

    /// <summary>
    /// Handles optimization level slider changes
    /// </summary>
    private void OnOptimizationChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (DepthDemoPanel == null) return;

        DepthDemoPanel.OptimizationLevel = (int)e.NewValue;
        UpdateOptimizationDisplay();
        
        var optimizationText = GetOptimizationLevelText((int)e.NewValue);
        UpdateStatusMessage($"Optimization level set to {optimizationText}");
    }

    /// <summary>
    /// Handles depth effects toggle
    /// </summary>
    private void OnDepthEffectsToggled(object sender, RoutedEventArgs e)
    {
        if (DepthDemoPanel == null || EnableDepthEffectsCheck == null) return;

        DepthDemoPanel.EnableDepthEffects = EnableDepthEffectsCheck.IsChecked == true;
        var status = DepthDemoPanel.EnableDepthEffects ? "enabled" : "disabled";
        UpdateStatusMessage($"Depth effects {status}");
    }

    /// <summary>
    /// Handles parallax toggle
    /// </summary>
    private void OnParallaxToggled(object sender, RoutedEventArgs e)
    {
        if (DepthDemoPanel == null || EnableParallaxCheck == null) return;

        DepthDemoPanel.EnableParallax = EnableParallaxCheck.IsChecked == true;
        var status = DepthDemoPanel.EnableParallax ? "enabled" : "disabled";
        UpdateStatusMessage($"Parallax motion {status}");
    }

    /// <summary>
    /// Focuses the movable element
    /// </summary>
    private void OnFocusElement(object sender, RoutedEventArgs e)
    {
        if (MovableElement == null || DepthDemoPanel == null) return;

        DepthDemoPanel.FocusElement(MovableElement);
        _isElementFocused = true;
        
        // Update slider to reflect focus depth
        if (DepthSlider != null)
        {
            DepthSlider.Value = 0.95;
        }
        
        UpdateStatusMessage("Element focused to surface layer");
    }

    /// <summary>
    /// Restores the movable element to original depth
    /// </summary>
    private void OnRestoreElement(object sender, RoutedEventArgs e)
    {
        if (MovableElement == null || DepthDemoPanel == null) return;

        DepthDemoPanel.RestoreElement(MovableElement);
        _isElementFocused = false;
        
        // Reset slider
        if (DepthSlider != null)
        {
            DepthSlider.Value = 0.5;
        }
        
        UpdateStatusMessage("Element restored to original depth");
    }

    /// <summary>
    /// Resets all demo settings
    /// </summary>
    private void OnResetDemo(object sender, RoutedEventArgs e)
    {
        if (DepthDemoPanel == null) return;

        // Reset system settings
        DepthDemoPanel.EnableDepthEffects = true;
        DepthDemoPanel.EnableParallax = false;
        DepthDemoPanel.DepthIntensity = 1.0;
        DepthDemoPanel.OptimizationLevel = 2;

        // Reset element settings
        if (MovableElement != null)
        {
            HolographicDepthPanel.SetDepthLevel(MovableElement, HolographicDepthLevel.MidLayer);
            HolographicDepthPanel.SetDepth(MovableElement, 0.5);
        }

        _currentTransitionType = DepthTransitionType.None;
        _isElementFocused = false;

        // Reset UI controls
        if (DepthSlider != null) DepthSlider.Value = 0.5;
        if (DepthLevelCombo != null) DepthLevelCombo.SelectedIndex = 3; // MidLayer
        if (TransitionTypeCombo != null) TransitionTypeCombo.SelectedIndex = 0; // None
        if (IntensitySlider != null) IntensitySlider.Value = 1.0;
        if (OptimizationSlider != null) OptimizationSlider.Value = 2;
        if (EnableDepthEffectsCheck != null) EnableDepthEffectsCheck.IsChecked = true;
        if (EnableParallaxCheck != null) EnableParallaxCheck.IsChecked = false;

        UpdateSliderDisplays();
        UpdateMovableElementDepthText();
        UpdateStatusMessage("All settings reset to defaults");
    }

    /// <summary>
    /// Handles interactive element mouse enter
    /// </summary>
    private void OnInteractiveElementMouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is Border border)
        {
            border.Opacity = 1.0;
            UpdateStatusMessage("Interactive element hovered - depth effects enhanced");
        }
    }

    /// <summary>
    /// Handles interactive element mouse leave
    /// </summary>
    private void OnInteractiveElementMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is Border border)
        {
            border.Opacity = 0.9;
            UpdateStatusMessage("Interactive element normal - depth effects restored");
        }
    }

    /// <summary>
    /// Handles demo element hover effects
    /// </summary>
    private void OnDemoElementHover(object sender, MouseEventArgs e)
    {
        if (sender is Border border)
        {
            var isHovering = e.RoutedEvent == UIElement.MouseEnterEvent;
            var message = isHovering ? "Demo element hovered - interactive depth layer" : "Demo element normal";
            UpdateStatusMessage(message);
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Updates all slider value displays
    /// </summary>
    private void UpdateSliderDisplays()
    {
        UpdateDepthDisplay();
        UpdateIntensityDisplay();
        UpdateOptimizationDisplay();
    }

    /// <summary>
    /// Updates the depth value display
    /// </summary>
    private void UpdateDepthDisplay()
    {
        if (DepthValue != null && DepthSlider != null)
        {
            DepthValue.Text = DepthSlider.Value.ToString("F2");
        }
    }

    /// <summary>
    /// Updates the intensity value display
    /// </summary>
    private void UpdateIntensityDisplay()
    {
        if (IntensityValue != null && IntensitySlider != null)
        {
            IntensityValue.Text = IntensitySlider.Value.ToString("F1");
        }
    }

    /// <summary>
    /// Updates the optimization value display
    /// </summary>
    private void UpdateOptimizationDisplay()
    {
        if (OptimizationValue != null && OptimizationSlider != null)
        {
            OptimizationValue.Text = ((int)OptimizationSlider.Value).ToString();
        }
    }

    /// <summary>
    /// Updates the movable element depth text
    /// </summary>
    private void UpdateMovableElementDepthText()
    {
        if (MovableElementDepth != null && MovableElement != null)
        {
            var depth = HolographicDepthPanel.GetDepth(MovableElement);
            MovableElementDepth.Text = $"Depth: {depth:F2}";
        }
    }

    /// <summary>
    /// Updates the status message
    /// </summary>
    private void UpdateStatusMessage(string message)
    {
        if (StatusText != null)
        {
            StatusText.Text = message;
        }
    }

    /// <summary>
    /// Gets optimization level description text
    /// </summary>
    private string GetOptimizationLevelText(int level)
    {
        return level switch
        {
            0 => "0 (Maximum Performance)",
            1 => "1 (Basic Effects)",
            2 => "2 (Standard Quality)",
            3 => "3 (Maximum Quality)",
            _ => level.ToString()
        };
    }

    /// <summary>
    /// Converts depth level to numeric value
    /// </summary>
    private double ConvertDepthLevelToNumeric(HolographicDepthLevel level)
    {
        return level switch
        {
            HolographicDepthLevel.DeepBackground => 0.05,
            HolographicDepthLevel.Background => 0.2,
            HolographicDepthLevel.BackgroundSecondary => 0.35,
            HolographicDepthLevel.MidLayer => 0.5,
            HolographicDepthLevel.Foreground => 0.65,
            HolographicDepthLevel.Surface => 0.8,
            HolographicDepthLevel.Interactive => 0.9,
            HolographicDepthLevel.Overlay => 0.95,
            HolographicDepthLevel.Modal => 0.98,
            HolographicDepthLevel.System => 1.0,
            _ => 0.5
        };
    }

    #endregion
}