// ==========================================================================
// BlurEffectsDemo.xaml.cs - Advanced Blur Effects Demonstration
// ==========================================================================
// Code-behind for the blur effects demonstration page. Provides interactive
// controls for testing depth-based blur, transparency effects, and 
// holographic glass panel variations.
//
// Features:
// - Real-time depth and transparency controls
// - Glass type switching
// - Performance quality adjustment
// - Animation toggle controls
// - Interactive effect demonstrations
//
// Author: Gideon Development Team
// Created: June 21, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Gideon.WPF.Presentation.Controls;

namespace Gideon.WPF.Presentation.Views;

/// <summary>
/// Interaction logic for BlurEffectsDemo.xaml
/// Demonstrates advanced blur effects and transparency system
/// </summary>
public partial class BlurEffectsDemo : Page
{
    #region Fields

    private readonly List<HolographicGlassPanel> _demoPanels = new();
    private readonly Random _random = new();

    #endregion

    #region Constructor

    public BlurEffectsDemo()
    {
        InitializeComponent();
        Loaded += OnPageLoaded;
    }

    #endregion

    #region Event Handlers

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        // Collect all demo panels for batch operations
        CollectDemoPanels();
        
        // Set initial values
        UpdateStatusText("Blur effects demonstration loaded - adjust controls to see real-time changes");
    }

    private void OnDepthChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded) return;

        var depth = e.NewValue;
        DepthValue.Text = depth.ToString("F2");
        
        // Apply depth to selected panels (exclude background panels that have fixed depth)
        foreach (var panel in _demoPanels.Where(p => p.Name != "BackgroundPanel1" && p.Name != "BackgroundPanel2"))
        {
            panel.AnimateToDepth(depth * 0.6); // Scale down slightly for better visual range
        }
        
        UpdateStatusText($"Depth adjusted to {depth:F2} - notice how blur increases with depth");
    }

    private void OnTransparencyChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded) return;

        var transparency = e.NewValue;
        TransparencyValue.Text = transparency.ToString("F2");
        
        // Apply transparency multiplier to all panels
        foreach (var panel in _demoPanels)
        {
            panel.TransparencyMultiplier = transparency;
        }
        
        UpdateStatusText($"Transparency multiplier set to {transparency:F2}");
    }

    private void OnBlurRadiusChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded) return;

        var maxBlur = e.NewValue;
        BlurRadiusValue.Text = maxBlur.ToString("F0");
        
        // Apply max blur radius to all panels
        foreach (var panel in _demoPanels)
        {
            panel.MaxBlurRadius = maxBlur;
        }
        
        UpdateStatusText($"Maximum blur radius set to {maxBlur:F0} pixels");
    }

    private void OnGlassTypeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || InteractivePanel == null) return;

        var selectedItem = GlassTypeCombo.SelectedItem as ComboBoxItem;
        var glassType = selectedItem?.Content.ToString() switch
        {
            "Ultra Light" => HolographicGlassType.UltraLight,
            "Light" => HolographicGlassType.Light,
            "Medium" => HolographicGlassType.Medium,
            "Strong" => HolographicGlassType.Strong,
            "Background" => HolographicGlassType.Background,
            "Electric Blue" => HolographicGlassType.ElectricBlue,
            "Gold" => HolographicGlassType.Gold,
            "High Performance" => HolographicGlassType.HighPerformance,
            _ => HolographicGlassType.Medium
        };

        // Apply to the interactive panel for demonstration
        InteractivePanel.GlassType = glassType;
        
        UpdateStatusText($"Interactive panel changed to {selectedItem?.Content} glass type");
    }

    private void OnQualityChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;

        var selectedItem = QualityCombo.SelectedItem as ComboBoxItem;
        var quality = selectedItem?.Content.ToString() switch
        {
            "Low" => BlurQuality.Low,
            "Medium" => BlurQuality.Medium,
            "High" => BlurQuality.High,
            "Ultra" => BlurQuality.Ultra,
            _ => BlurQuality.High
        };

        // Apply quality setting to all panels
        foreach (var panel in _demoPanels)
        {
            panel.BlurQuality = quality;
        }
        
        UpdateStatusText($"Blur quality set to {selectedItem?.Content} - affects performance and visual quality");
    }

    private void OnAnimationSettingsChanged(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded) return;

        var blurAnimations = BlurAnimationsCheck.IsChecked == true;
        var transparencyPulse = TransparencyPulseCheck.IsChecked == true;

        // Apply animation settings to all panels
        foreach (var panel in _demoPanels)
        {
            panel.EnableBlurAnimations = blurAnimations;
            panel.EnableTransparencyPulse = transparencyPulse;
        }
        
        var animationStatus = blurAnimations ? "enabled" : "disabled";
        var pulseStatus = transparencyPulse ? "enabled" : "disabled";
        UpdateStatusText($"Blur animations {animationStatus}, transparency pulse {pulseStatus}");
    }

    private void OnTriggerGlow(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded) return;

        // Trigger random glow effects on random panels
        var panelsToGlow = _demoPanels.OrderBy(x => _random.Next()).Take(3).ToList();
        var colors = new[]
        {
            Color.FromRgb(0, 212, 255),    // Electric Blue
            Color.FromRgb(255, 215, 0),    // Gold
            Color.FromRgb(255, 159, 10),   // Orange
            Color.FromRgb(48, 209, 88)     // Green
        };

        foreach (var panel in panelsToGlow)
        {
            var randomColor = colors[_random.Next(colors.Length)];
            panel.TriggerGlowEffect(randomColor, 1.5);
        }
        
        UpdateStatusText($"Triggered glow effects on {panelsToGlow.Count} panels");
    }

    private void OnResetSettings(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded) return;

        // Reset all controls to default values
        DepthSlider.Value = 0.3;
        TransparencySlider.Value = 1.0;
        BlurRadiusSlider.Value = 25.0;
        GlassTypeCombo.SelectedIndex = 0; // Ultra Light
        QualityCombo.SelectedIndex = 2;   // High
        BlurAnimationsCheck.IsChecked = true;
        TransparencyPulseCheck.IsChecked = false;
        
        // Reset all panels to their original configurations
        ResetPanelConfigurations();
        
        UpdateStatusText("All settings reset to default values");
    }

    #endregion

    #region Private Methods

    private void CollectDemoPanels()
    {
        _demoPanels.Clear();
        
        // Find all HolographicGlassPanel controls in the demo canvas
        CollectPanelsFromContainer(DemoCanvas);
    }

    private void CollectPanelsFromContainer(DependencyObject container)
    {
        var childCount = VisualTreeHelper.GetChildrenCount(container);
        
        for (int i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(container, i);
            
            if (child is HolographicGlassPanel panel)
            {
                _demoPanels.Add(panel);
            }
            else
            {
                CollectPanelsFromContainer(child);
            }
        }
    }

    private void ResetPanelConfigurations()
    {
        // Reset each panel to its original configuration
        var panelConfigs = new Dictionary<string, (HolographicGlassType type, double depth)>
        {
            { "BackgroundPanel1", (HolographicGlassType.Background, 0.8) },
            { "BackgroundPanel2", (HolographicGlassType.Background, 0.9) },
            { "MidPanel1", (HolographicGlassType.Medium, 0.4) },
            { "ElectricBluePanel", (HolographicGlassType.ElectricBlue, 0.3) },
            { "ForegroundPanel1", (HolographicGlassType.Light, 0.1) },
            { "GoldPanel", (HolographicGlassType.Gold, 0.2) },
            { "UltraLightPanel", (HolographicGlassType.UltraLight, 0.0) },
            { "InteractivePanel", (HolographicGlassType.HighPerformance, 0.2) }
        };

        foreach (var panel in _demoPanels)
        {
            if (panelConfigs.TryGetValue(panel.Name, out var config))
            {
                panel.GlassType = config.type;
                panel.Depth = config.depth;
                panel.TransparencyMultiplier = 1.0;
                panel.MaxBlurRadius = 25.0;
                panel.BlurQuality = BlurQuality.High;
                panel.EnableBlurAnimations = true;
                panel.EnableTransparencyPulse = false;
            }
        }
    }

    private void UpdateStatusText(string message)
    {
        if (StatusText != null)
        {
            StatusText.Text = message;
        }
    }

    #endregion
}