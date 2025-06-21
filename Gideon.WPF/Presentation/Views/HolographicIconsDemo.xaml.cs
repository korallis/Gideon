// ==========================================================================
// HolographicIconsDemo.xaml.cs - Holographic Icons Demo Page Code-Behind
// ==========================================================================
// Interactive demonstration page for the EVE holographic icon system with
// real-time customization controls and comprehensive icon library showcase.
//
// Features:
// - Real-time icon property modification
// - Dynamic state and type switching
// - Animation and effect controls
// - Performance monitoring
// - Accessibility testing
//
// Author: Gideon Development Team
// Created: June 21, 2025
// ==========================================================================

using System;
using System.Windows;
using System.Windows.Controls;
using Gideon.WPF.Presentation.Controls;

namespace Gideon.WPF.Presentation.Views;

/// <summary>
/// Interactive demonstration page for EVE holographic icon system
/// </summary>
public partial class HolographicIconsDemo : Page
{
    #region Constructor

    public HolographicIconsDemo()
    {
        InitializeComponent();
        InitializeControls();
    }

    #endregion

    #region Initialization

    private void InitializeControls()
    {
        // Set initial control states
        if (IconTypeCombo != null)
        {
            IconTypeCombo.SelectedIndex = 6; // Shield
        }

        if (IconStateCombo != null)
        {
            IconStateCombo.SelectedIndex = 0; // Normal
        }

        if (IconSizeCombo != null)
        {
            IconSizeCombo.SelectedIndex = 3; // XLarge
        }

        // Initialize slider values
        UpdateSliderDisplays();
        
        // Update status
        UpdateStatusMessage("Holographic icon system initialized - Ready for customization");
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handles icon type selection changes
    /// </summary>
    private void OnIconTypeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DemoIcon == null || IconTypeCombo?.SelectedItem is not ComboBoxItem selectedItem)
            return;

        var iconTypeText = selectedItem.Content.ToString();
        if (Enum.TryParse<HolographicIconType>(iconTypeText, out var iconType))
        {
            DemoIcon.IconType = iconType;
            UpdateStatusMessage($"Icon type changed to {iconTypeText}");
        }
    }

    /// <summary>
    /// Handles icon state selection changes
    /// </summary>
    private void OnIconStateChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DemoIcon == null || IconStateCombo?.SelectedItem is not ComboBoxItem selectedItem)
            return;

        var stateText = selectedItem.Content.ToString();
        if (Enum.TryParse<HolographicIconState>(stateText, out var state))
        {
            DemoIcon.IconState = state;
            UpdateStatusMessage($"Icon state changed to {stateText}");
        }
    }

    /// <summary>
    /// Handles icon size selection changes
    /// </summary>
    private void OnIconSizeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DemoIcon == null || IconSizeCombo?.SelectedItem is not ComboBoxItem selectedItem)
            return;

        var sizeText = selectedItem.Content.ToString();
        if (Enum.TryParse<HolographicIconSize>(sizeText, out var size))
        {
            DemoIcon.IconSize = size;
            
            // Enable/disable custom size slider
            if (CustomSizeSlider != null)
            {
                CustomSizeSlider.IsEnabled = size == HolographicIconSize.Custom;
            }
            
            UpdateStatusMessage($"Icon size changed to {sizeText}");
        }
    }

    /// <summary>
    /// Handles custom size slider changes
    /// </summary>
    private void OnCustomSizeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (DemoIcon == null) return;

        DemoIcon.CustomSize = e.NewValue;
        UpdateCustomSizeDisplay();
        UpdateStatusMessage($"Custom size set to {e.NewValue:F0}px");
    }

    /// <summary>
    /// Handles glow intensity slider changes
    /// </summary>
    private void OnGlowIntensityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (DemoIcon == null) return;

        DemoIcon.GlowIntensity = e.NewValue;
        UpdateGlowIntensityDisplay();
        UpdateStatusMessage($"Glow intensity set to {e.NewValue:F1}");
    }

    /// <summary>
    /// Handles stroke thickness slider changes
    /// </summary>
    private void OnStrokeThicknessChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (DemoIcon == null) return;

        DemoIcon.StrokeThickness = e.NewValue;
        UpdateStrokeThicknessDisplay();
        UpdateStatusMessage($"Stroke thickness set to {e.NewValue:F1}");
    }

    /// <summary>
    /// Handles pulse animation toggle
    /// </summary>
    private void OnEnablePulseChanged(object sender, RoutedEventArgs e)
    {
        if (DemoIcon == null || EnablePulseCheck == null) return;

        DemoIcon.EnablePulse = EnablePulseCheck.IsChecked == true;
        var status = DemoIcon.EnablePulse ? "enabled" : "disabled";
        UpdateStatusMessage($"Pulse animation {status}");
    }

    /// <summary>
    /// Handles interactive mode toggle
    /// </summary>
    private void OnIsInteractiveChanged(object sender, RoutedEventArgs e)
    {
        if (DemoIcon == null || IsInteractiveCheck == null) return;

        DemoIcon.IsInteractive = IsInteractiveCheck.IsChecked == true;
        var status = DemoIcon.IsInteractive ? "enabled" : "disabled";
        UpdateStatusMessage($"Interactive mode {status}");
    }

    /// <summary>
    /// Triggers icon activation effect
    /// </summary>
    private void OnTriggerActivation(object sender, RoutedEventArgs e)
    {
        if (DemoIcon == null) return;

        DemoIcon.TriggerActivation();
        UpdateStatusMessage("Icon activation effect triggered");
    }

    /// <summary>
    /// Resets all settings to defaults
    /// </summary>
    private void OnResetSettings(object sender, RoutedEventArgs e)
    {
        if (DemoIcon == null) return;

        // Reset icon properties
        DemoIcon.IconType = HolographicIconType.Shield;
        DemoIcon.IconState = HolographicIconState.Normal;
        DemoIcon.IconSize = HolographicIconSize.XLarge;
        DemoIcon.CustomSize = 48;
        DemoIcon.GlowIntensity = 1.0;
        DemoIcon.StrokeThickness = 2.0;
        DemoIcon.EnablePulse = false;
        DemoIcon.IsInteractive = true;

        // Reset UI controls
        if (IconTypeCombo != null) IconTypeCombo.SelectedIndex = 6; // Shield
        if (IconStateCombo != null) IconStateCombo.SelectedIndex = 0; // Normal
        if (IconSizeCombo != null) IconSizeCombo.SelectedIndex = 3; // XLarge
        if (CustomSizeSlider != null) CustomSizeSlider.Value = 48;
        if (GlowIntensitySlider != null) GlowIntensitySlider.Value = 1.0;
        if (StrokeThicknessSlider != null) StrokeThicknessSlider.Value = 2.0;
        if (EnablePulseCheck != null) EnablePulseCheck.IsChecked = false;
        if (IsInteractiveCheck != null) IsInteractiveCheck.IsChecked = true;

        UpdateSliderDisplays();
        UpdateStatusMessage("All settings reset to defaults");
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Updates all slider value displays
    /// </summary>
    private void UpdateSliderDisplays()
    {
        UpdateCustomSizeDisplay();
        UpdateGlowIntensityDisplay();
        UpdateStrokeThicknessDisplay();
    }

    /// <summary>
    /// Updates the custom size value display
    /// </summary>
    private void UpdateCustomSizeDisplay()
    {
        if (CustomSizeValue != null && CustomSizeSlider != null)
        {
            CustomSizeValue.Text = CustomSizeSlider.Value.ToString("F0");
        }
    }

    /// <summary>
    /// Updates the glow intensity value display
    /// </summary>
    private void UpdateGlowIntensityDisplay()
    {
        if (GlowIntensityValue != null && GlowIntensitySlider != null)
        {
            GlowIntensityValue.Text = GlowIntensitySlider.Value.ToString("F1");
        }
    }

    /// <summary>
    /// Updates the stroke thickness value display
    /// </summary>
    private void UpdateStrokeThicknessDisplay()
    {
        if (StrokeThicknessValue != null && StrokeThicknessSlider != null)
        {
            StrokeThicknessValue.Text = StrokeThicknessSlider.Value.ToString("F1");
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

    #endregion
}