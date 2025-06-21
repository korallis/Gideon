// ==========================================================================
// LayerDemo.xaml.cs - Layered Composition System Demonstration
// ==========================================================================
// Code-behind for the layer demonstration page. Provides interactive
// controls for testing and demonstrating the holographic layer system.
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
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;
using Gideon.WPF.Infrastructure.Extensions;
using Gideon.WPF.Presentation.Controls;
using Microsoft.Extensions.Logging;

namespace Gideon.WPF.Presentation.Views;

/// <summary>
/// Interaction logic for LayerDemo.xaml
/// </summary>
public partial class LayerDemo : Page
{
    #region Fields

    private readonly ILogger<LayerDemo>? _logger;
    private readonly DispatcherTimer _updateTimer;
    private readonly DispatcherTimer _clockTimer;
    private readonly Random _random = new();
    private readonly List<FrameworkElement> _demoElements = new();
    private int _elementCounter = 1;

    // Element type counters
    private readonly Dictionary<HolographicLayer, int> _layerCounts = new()
    {
        [HolographicLayer.Background] = 0,
        [HolographicLayer.MidLayer] = 0,
        [HolographicLayer.Foreground] = 0,
        [HolographicLayer.Overlay] = 0,
        [HolographicLayer.Topmost] = 0
    };

    // Demo element templates
    private readonly string[] _demoShapes = { "Circle", "Rectangle", "Triangle", "Hexagon" };
    private readonly Color[] _demoColors = 
    {
        Color.FromArgb(255, 0, 212, 255),   // EVE Cyan
        Color.FromArgb(255, 255, 215, 0),   // EVE Gold
        Color.FromArgb(255, 255, 159, 10),  // EVE Orange
        Color.FromArgb(255, 48, 209, 88),   // EVE Green
        Color.FromArgb(255, 255, 69, 58),   // EVE Red
        Color.FromArgb(255, 138, 43, 226)   // Holographic Purple
    };

    #endregion

    #region Constructor

    public LayerDemo()
    {
        InitializeComponent();

        // Setup update timer for statistics
        _updateTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(0.5)
        };
        _updateTimer.Tick += OnUpdateTimerTick;

        // Setup clock timer
        _clockTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _clockTimer.Tick += OnClockTimerTick;

        Loaded += OnPageLoaded;
        Unloaded += OnPageUnloaded;
    }

    #endregion

    #region Event Handlers

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _updateTimer.Start();
            _clockTimer.Start();

            // Add some initial demo elements
            CreateInitialDemoElements();
            
            UpdateStatistics();
            UpdateStatus("Layer demonstration loaded - click buttons to add elements");

            _logger?.LogInformation("Layer demo page loaded successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load layer demo page");
            UpdateStatus("Error loading layer demonstration");
        }
    }

    private void OnPageUnloaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _updateTimer?.Stop();
            _clockTimer?.Stop();
            ClearAllElements();

            _logger?.LogInformation("Layer demo page unloaded");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during layer demo page unload");
        }
    }

    private void OnUpdateTimerTick(object? sender, EventArgs e)
    {
        UpdateStatistics();
    }

    private void OnClockTimerTick(object? sender, EventArgs e)
    {
        if (TimestampText != null)
        {
            TimestampText.Text = DateTime.Now.ToString("HH:mm:ss");
        }
    }

    #endregion

    #region Button Event Handlers

    private void OnAddBackgroundElement(object sender, RoutedEventArgs e)
    {
        try
        {
            var element = CreateDemoElement($"BG-{_elementCounter++}", HolographicLayer.Background);
            element.InBackgroundLayer(_random.NextDouble() * 0.5 + 0.5); // Depth 0.5-1.0
            _demoElements.Add(element);
            _layerCounts[HolographicLayer.Background]++;
            
            UpdateStatus($"Added background element (depth: {0.5 + _random.NextDouble() * 0.5:F2})");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to add background element");
            UpdateStatus("Error adding background element");
        }
    }

    private void OnAddMidLayerElement(object sender, RoutedEventArgs e)
    {
        try
        {
            var element = CreateDemoElement($"MID-{_elementCounter++}", HolographicLayer.MidLayer);
            element.InMidLayer(_random.NextDouble() * 0.6 + 0.2); // Depth 0.2-0.8
            _demoElements.Add(element);
            _layerCounts[HolographicLayer.MidLayer]++;
            
            UpdateStatus($"Added mid-layer element (depth: {0.2 + _random.NextDouble() * 0.6:F2})");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to add mid-layer element");
            UpdateStatus("Error adding mid-layer element");
        }
    }

    private void OnAddForegroundElement(object sender, RoutedEventArgs e)
    {
        try
        {
            var element = CreateDemoElement($"FG-{_elementCounter++}", HolographicLayer.Foreground);
            element.InForegroundLayer(_random.NextDouble() * 0.3); // Depth 0.0-0.3
            element.WithFocusLayerTransition(); // Add interactive transitions
            _demoElements.Add(element);
            _layerCounts[HolographicLayer.Foreground]++;
            
            UpdateStatus($"Added interactive foreground element (depth: {_random.NextDouble() * 0.3:F2})");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to add foreground element");
            UpdateStatus("Error adding foreground element");
        }
    }

    private void OnAddOverlayElement(object sender, RoutedEventArgs e)
    {
        try
        {
            var element = CreateDemoElement($"OVR-{_elementCounter++}", HolographicLayer.Overlay);
            element.InOverlayLayer();
            _demoElements.Add(element);
            _layerCounts[HolographicLayer.Overlay]++;
            
            // Auto-remove overlay after 5 seconds
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            timer.Tick += (s, args) =>
            {
                RemoveDemoElement(element);
                timer.Stop();
            };
            timer.Start();
            
            UpdateStatus("Added overlay element (will auto-remove in 5 seconds)");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to add overlay element");
            UpdateStatus("Error adding overlay element");
        }
    }

    private void OnTransitionToBackground(object sender, RoutedEventArgs e)
    {
        try
        {
            var foregroundElements = _demoElements
                .Where(el => DemoCanvas.GetStatistics().LayerElementCounts.ContainsKey("Foreground"))
                .Take(3)
                .ToList();

            foreach (var element in foregroundElements)
            {
                element.TransitionToBackground(0.8, 0.7);
                _layerCounts[HolographicLayer.Foreground]--;
                _layerCounts[HolographicLayer.Background]++;
            }

            UpdateStatus($"Transitioned {foregroundElements.Count} elements to background");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to transition elements to background");
            UpdateStatus("Error transitioning elements");
        }
    }

    private void OnTransitionToForeground(object sender, RoutedEventArgs e)
    {
        try
        {
            var backgroundElements = _demoElements
                .Where(el => DemoCanvas.GetStatistics().LayerElementCounts.ContainsKey("Background"))
                .Take(3)
                .ToList();

            foreach (var element in backgroundElements)
            {
                element.TransitionToForeground(0.5, 0.1);
                _layerCounts[HolographicLayer.Background]--;
                _layerCounts[HolographicLayer.Foreground]++;
            }

            UpdateStatus($"Transitioned {backgroundElements.Count} elements to foreground");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to transition elements to foreground");
            UpdateStatus("Error transitioning elements");
        }
    }

    private void OnClearAll(object sender, RoutedEventArgs e)
    {
        try
        {
            ClearAllElements();
            UpdateStatus("All elements cleared");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to clear all elements");
            UpdateStatus("Error clearing elements");
        }
    }

    private void OnDebugModeToggled(object sender, RoutedEventArgs e)
    {
        try
        {
            var isEnabled = DebugModeCheckbox?.IsChecked == true;
            DemoCanvas?.WithDebugMode(isEnabled);
            
            UpdateStatus($"Debug mode {(isEnabled ? "enabled" : "disabled")}");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to toggle debug mode");
        }
    }

    #endregion

    #region Private Methods

    private void CreateInitialDemoElements()
    {
        try
        {
            // Add a few initial elements to demonstrate the system
            var welcomeElement = CreateWelcomeElement();
            welcomeElement.InForegroundLayer(0.0);
            _demoElements.Add(welcomeElement);

            // Add background decoration
            for (int i = 0; i < 3; i++)
            {
                var bgElement = CreateDemoElement($"Init-BG-{i + 1}", HolographicLayer.Background);
                bgElement.InBackgroundLayer(0.6 + i * 0.1);
                _demoElements.Add(bgElement);
                _layerCounts[HolographicLayer.Background]++;
            }

            // Add mid-layer content
            for (int i = 0; i < 2; i++)
            {
                var midElement = CreateDemoElement($"Init-MID-{i + 1}", HolographicLayer.MidLayer);
                midElement.InMidLayer(0.3 + i * 0.2);
                _demoElements.Add(midElement);
                _layerCounts[HolographicLayer.MidLayer]++;
            }

            _elementCounter = 10; // Start counter after initial elements
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to create initial demo elements");
        }
    }

    private Border CreateWelcomeElement()
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(80, 0, 212, 255)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 212, 255)),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(20),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = HorizontalAlignment.Center,
            Effect = new DropShadowEffect
            {
                Color = Color.FromArgb(255, 0, 212, 255),
                BlurRadius = 12,
                ShadowDepth = 0,
                Opacity = 0.8
            }
        };

        var textBlock = new TextBlock
        {
            Text = "WELCOME TO THE LAYER SYSTEM\nClick buttons to add elements",
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White,
            TextAlignment = TextAlignment.Center,
            LineHeight = 22
        };

        border.Child = textBlock;
        DemoCanvas.Children.Add(border);

        return border;
    }

    private Border CreateDemoElement(string id, HolographicLayer targetLayer)
    {
        var shapeType = _demoShapes[_random.Next(_demoShapes.Length)];
        var color = _demoColors[_random.Next(_demoColors.Length)];
        
        var border = new Border
        {
            Width = 60 + _random.Next(40),
            Height = 60 + _random.Next(40),
            Background = new SolidColorBrush(Color.FromArgb(120, color.R, color.G, color.B)),
            BorderBrush = new SolidColorBrush(color),
            BorderThickness = new Thickness(2),
            CornerRadius = shapeType == "Circle" ? new CornerRadius(50) : new CornerRadius(4),
            Margin = new Thickness(10),
            Cursor = System.Windows.Input.Cursors.Hand
        };

        // Add content
        var contentPanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var shapeIcon = CreateShapeIcon(shapeType, color);
        var labelText = new TextBlock
        {
            Text = id,
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 2, 0, 0)
        };

        contentPanel.Children.Add(shapeIcon);
        contentPanel.Children.Add(labelText);
        border.Child = contentPanel;

        // Position randomly
        var canvasWidth = DemoCanvas.ActualWidth > 0 ? DemoCanvas.ActualWidth : 600;
        var canvasHeight = DemoCanvas.ActualHeight > 0 ? DemoCanvas.ActualHeight : 400;
        
        Canvas.SetLeft(border, _random.NextDouble() * (canvasWidth - border.Width));
        Canvas.SetTop(border, _random.NextDouble() * (canvasHeight - border.Height));

        // Add to canvas
        DemoCanvas.Children.Add(border);

        // Add hover effects
        AddHoverEffects(border, color);

        return border;
    }

    private UIElement CreateShapeIcon(string shapeType, Color color)
    {
        var brush = new SolidColorBrush(color);
        
        return shapeType switch
        {
            "Circle" => new Ellipse { Width = 20, Height = 20, Fill = brush },
            "Rectangle" => new Rectangle { Width = 20, Height = 15, Fill = brush },
            "Triangle" => CreateTriangle(brush),
            "Hexagon" => CreateHexagon(brush),
            _ => new Rectangle { Width = 20, Height = 20, Fill = brush }
        };
    }

    private Polygon CreateTriangle(Brush brush)
    {
        return new Polygon
        {
            Points = new PointCollection { new Point(10, 0), new Point(0, 20), new Point(20, 20) },
            Fill = brush
        };
    }

    private Polygon CreateHexagon(Brush brush)
    {
        return new Polygon
        {
            Points = new PointCollection 
            { 
                new Point(10, 0), new Point(18, 5), new Point(18, 15), 
                new Point(10, 20), new Point(2, 15), new Point(2, 5) 
            },
            Fill = brush
        };
    }

    private void AddHoverEffects(Border element, Color color)
    {
        element.MouseEnter += (s, e) =>
        {
            var glowEffect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 15,
                ShadowDepth = 0,
                Opacity = 1.0
            };
            element.Effect = glowEffect;

            var scaleTransform = new ScaleTransform(1.1, 1.1);
            element.RenderTransform = scaleTransform;
            element.RenderTransformOrigin = new Point(0.5, 0.5);
        };

        element.MouseLeave += (s, e) =>
        {
            element.Effect = null;
            element.RenderTransform = Transform.Identity;
        };

        element.MouseLeftButtonUp += (s, e) =>
        {
            // Flash effect on click
            var flashStoryboard = new Storyboard();
            var flashAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.3,
                Duration = TimeSpan.FromSeconds(0.1),
                AutoReverse = true
            };
            Storyboard.SetTarget(flashAnimation, element);
            Storyboard.SetTargetProperty(flashAnimation, new PropertyPath(UIElement.OpacityProperty));
            flashStoryboard.Children.Add(flashAnimation);
            flashStoryboard.Begin();
        };
    }

    private void RemoveDemoElement(FrameworkElement element)
    {
        try
        {
            element.RemoveFromLayers();
            _demoElements.Remove(element);
            
            // Update layer counts (simplified)
            foreach (var layer in _layerCounts.Keys.ToList())
            {
                if (_layerCounts[layer] > 0)
                {
                    _layerCounts[layer]--;
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to remove demo element");
        }
    }

    private void ClearAllElements()
    {
        try
        {
            foreach (var element in _demoElements.ToList())
            {
                element.RemoveFromLayers();
            }
            
            _demoElements.Clear();
            
            // Reset counters
            foreach (var layer in _layerCounts.Keys.ToList())
            {
                _layerCounts[layer] = 0;
            }

            DemoCanvas?.ClearAllLayers();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to clear all elements");
        }
    }

    private void UpdateStatistics()
    {
        try
        {
            var stats = DemoCanvas?.GetStatistics();
            if (stats == null) return;

            // Update layer counts
            if (BackgroundCountText != null)
                BackgroundCountText.Text = _layerCounts[HolographicLayer.Background].ToString();

            if (MidLayerCountText != null)
                MidLayerCountText.Text = _layerCounts[HolographicLayer.MidLayer].ToString();

            if (ForegroundCountText != null)
                ForegroundCountText.Text = _layerCounts[HolographicLayer.Foreground].ToString();

            if (OverlayCountText != null)
                OverlayCountText.Text = _layerCounts[HolographicLayer.Overlay].ToString();

            if (TotalCountText != null)
                TotalCountText.Text = stats.TotalElements.ToString();

            if (FpsText != null)
                FpsText.Text = $"{stats.FrameRate:F0}";
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to update statistics");
        }
    }

    private void UpdateStatus(string message)
    {
        try
        {
            if (StatusText != null)
            {
                StatusText.Text = message;
                
                // Flash status update
                var flashAnimation = new DoubleAnimation
                {
                    From = 0.5,
                    To = 1.0,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                StatusText.BeginAnimation(UIElement.OpacityProperty, flashAnimation);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to update status message");
        }
    }

    #endregion
}