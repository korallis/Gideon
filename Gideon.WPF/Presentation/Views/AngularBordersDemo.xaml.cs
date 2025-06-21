// ==========================================================================
// AngularBordersDemo.xaml.cs - Angular Borders Demo Page Code-Behind
// ==========================================================================
// Interactive demonstration page for the EVE angular border system with
// corporation insignia support and real-time border customization.
//
// Features:
// - Real-time border type switching
// - Corporation insignia management
// - Military rank indicator controls
// - Animation effect toggles
// - Interactive border customization
// - Border activation effects
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
/// Interactive demonstration page for EVE angular border system
/// </summary>
public partial class AngularBordersDemo : Page
{
    #region Constructor

    public AngularBordersDemo()
    {
        InitializeComponent();
        InitializeControls();
    }

    #endregion

    #region Initialization

    private void InitializeControls()
    {
        // Set initial control states
        if (BorderTypeCombo != null)
        {
            BorderTypeCombo.SelectedIndex = 0; // Standard
        }

        if (InsigniaPositionCombo != null)
        {
            InsigniaPositionCombo.SelectedIndex = 0; // None
        }

        if (MilitaryRankCombo != null)
        {
            MilitaryRankCombo.SelectedIndex = 0; // None
        }

        // Initialize thickness values
        UpdateThicknessDisplay();
        
        // Update status
        UpdateStatusMessage("Angular border system initialized - Ready for customization");
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handles border type selection changes
    /// </summary>
    private void OnBorderTypeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (InteractivePanel == null || BorderTypeCombo?.SelectedItem is not ComboBoxItem selectedItem)
            return;

        var borderTypeText = selectedItem.Content.ToString();
        if (Enum.TryParse<AngularBorderType>(borderTypeText, out var borderType))
        {
            InteractivePanel.BorderType = borderType;
            UpdateStatusMessage($"Border type changed to {borderTypeText}");
        }
    }

    /// <summary>
    /// Handles thickness slider changes
    /// </summary>
    private void OnThicknessChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (InteractivePanel == null) return;

        InteractivePanel.BorderThicknessMultiplier = e.NewValue;
        UpdateThicknessDisplay();
        UpdateStatusMessage($"Border thickness set to {e.NewValue:F1}x");
    }

    /// <summary>
    /// Handles insignia position changes
    /// </summary>
    private void OnInsigniaPositionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (InteractivePanel == null || InsigniaPositionCombo?.SelectedItem is not ComboBoxItem selectedItem)
            return;

        var positionText = selectedItem.Content.ToString();
        if (Enum.TryParse<CorporationInsigniaPosition>(positionText, out var position))
        {
            InteractivePanel.InsigniaPosition = position;
            UpdateStatusMessage($"Insignia position set to {positionText}");
        }
    }

    /// <summary>
    /// Handles corporation name changes
    /// </summary>
    private void OnCorporationNameChanged(object sender, TextChangedEventArgs e)
    {
        if (InteractivePanel == null || sender is not TextBox textBox) return;

        InteractivePanel.CorporationName = textBox.Text;
        UpdateStatusMessage($"Corporation name set to '{textBox.Text}'");
    }

    /// <summary>
    /// Handles alliance name changes
    /// </summary>
    private void OnAllianceNameChanged(object sender, TextChangedEventArgs e)
    {
        if (InteractivePanel == null || sender is not TextBox textBox) return;

        InteractivePanel.AllianceName = textBox.Text;
        UpdateStatusMessage($"Alliance name set to '{textBox.Text}'");
    }

    /// <summary>
    /// Handles military rank changes
    /// </summary>
    private void OnMilitaryRankChanged(object sender, SelectionChangedEventArgs e)
    {
        if (InteractivePanel == null || MilitaryRankCombo?.SelectedItem is not ComboBoxItem selectedItem)
            return;

        var rankText = selectedItem.Content.ToString();
        if (Enum.TryParse<MilitaryRank>(rankText, out var rank))
        {
            InteractivePanel.MilitaryRank = rank;
            UpdateStatusMessage($"Military rank set to {rankText}");
        }
    }

    /// <summary>
    /// Handles animation setting changes
    /// </summary>
    private void OnAnimationSettingsChanged(object sender, RoutedEventArgs e)
    {
        if (InteractivePanel == null) return;

        if (CornerGlowCheck != null)
        {
            InteractivePanel.EnableCornerGlow = CornerGlowCheck.IsChecked == true;
        }

        if (ScanLineCheck != null)
        {
            InteractivePanel.EnableScanLine = ScanLineCheck.IsChecked == true;
        }

        var effects = new System.Collections.Generic.List<string>();
        if (InteractivePanel.EnableCornerGlow) effects.Add("Corner Glow");
        if (InteractivePanel.EnableScanLine) effects.Add("Scan Line");

        var effectsText = effects.Count > 0 ? string.Join(", ", effects) : "None";
        UpdateStatusMessage($"Animation effects: {effectsText}");
    }

    /// <summary>
    /// Triggers border activation effect
    /// </summary>
    private void OnTriggerEffect(object sender, RoutedEventArgs e)
    {
        if (InteractivePanel == null) return;

        InteractivePanel.TriggerActivationEffect();
        UpdateStatusMessage("Border activation effect triggered");
    }

    /// <summary>
    /// Resets all settings to defaults
    /// </summary>
    private void OnResetSettings(object sender, RoutedEventArgs e)
    {
        if (InteractivePanel == null) return;

        // Reset border settings
        InteractivePanel.BorderType = AngularBorderType.Standard;
        InteractivePanel.BorderThicknessMultiplier = 1.0;
        InteractivePanel.InsigniaPosition = CorporationInsigniaPosition.None;
        InteractivePanel.MilitaryRank = MilitaryRank.None;
        InteractivePanel.EnableCornerGlow = true;
        InteractivePanel.EnableScanLine = false;
        InteractivePanel.CorporationName = "CORP";
        InteractivePanel.AllianceName = "ALLY";

        // Reset UI controls
        if (BorderTypeCombo != null) BorderTypeCombo.SelectedIndex = 0;
        if (ThicknessSlider != null) ThicknessSlider.Value = 1.0;
        if (InsigniaPositionCombo != null) InsigniaPositionCombo.SelectedIndex = 0;
        if (MilitaryRankCombo != null) MilitaryRankCombo.SelectedIndex = 0;
        if (CorporationNameText != null) CorporationNameText.Text = "CORP";
        if (AllianceNameText != null) AllianceNameText.Text = "ALLY";
        if (CornerGlowCheck != null) CornerGlowCheck.IsChecked = true;
        if (ScanLineCheck != null) ScanLineCheck.IsChecked = false;

        UpdateThicknessDisplay();
        UpdateStatusMessage("All settings reset to defaults");
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Updates the thickness value display
    /// </summary>
    private void UpdateThicknessDisplay()
    {
        if (ThicknessValue != null && ThicknessSlider != null)
        {
            ThicknessValue.Text = ThicknessSlider.Value.ToString("F1");
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