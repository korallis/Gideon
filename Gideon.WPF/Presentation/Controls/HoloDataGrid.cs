// ==========================================================================
// HoloDataGrid.cs - Holographic Data Grid with Animated Row Highlighting
// ==========================================================================
// Advanced holographic data grid featuring animated row highlighting,
// EVE-style visual effects, sorting animations, and adaptive performance.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Advanced holographic data grid with animated row highlighting and EVE styling
/// </summary>
public class HoloDataGrid : DataGrid, IAnimationIntensityTarget, IAdaptiveControl
{
    #region Dependency Properties

    public static readonly DependencyProperty HolographicIntensityProperty =
        DependencyProperty.Register(nameof(HolographicIntensity), typeof(double), typeof(HoloDataGrid),
            new PropertyMetadata(1.0, OnHolographicIntensityChanged));

    public static readonly DependencyProperty EVEColorSchemeProperty =
        DependencyProperty.Register(nameof(EVEColorScheme), typeof(EVEColorScheme), typeof(HoloDataGrid),
            new PropertyMetadata(EVEColorScheme.ElectricBlue, OnEVEColorSchemeChanged));

    public static readonly DependencyProperty EnableRowHighlightAnimationProperty =
        DependencyProperty.Register(nameof(EnableRowHighlightAnimation), typeof(bool), typeof(HoloDataGrid),
            new PropertyMetadata(true, OnEnableRowHighlightAnimationChanged));

    public static readonly DependencyProperty EnableSortingAnimationProperty =
        DependencyProperty.Register(nameof(EnableSortingAnimation), typeof(bool), typeof(HoloDataGrid),
            new PropertyMetadata(true));

    public static readonly DependencyProperty EnableScanlineEffectProperty =
        DependencyProperty.Register(nameof(EnableScanlineEffect), typeof(bool), typeof(HoloDataGrid),
            new PropertyMetadata(true, OnEnableScanlineEffectChanged));

    public static readonly DependencyProperty HighlightedRowProperty =
        DependencyProperty.Register(nameof(HighlightedRow), typeof(object), typeof(HoloDataGrid),
            new PropertyMetadata(null, OnHighlightedRowChanged));

    public static readonly DependencyProperty RowHighlightDurationProperty =
        DependencyProperty.Register(nameof(RowHighlightDuration), typeof(TimeSpan), typeof(HoloDataGrid),
            new PropertyMetadata(TimeSpan.FromSeconds(2)));

    public static readonly DependencyProperty ShowDataVisualizationProperty =
        DependencyProperty.Register(nameof(ShowDataVisualization), typeof(bool), typeof(HoloDataGrid),
            new PropertyMetadata(false, OnShowDataVisualizationChanged));

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

    public bool EnableRowHighlightAnimation
    {
        get => (bool)GetValue(EnableRowHighlightAnimationProperty);
        set => SetValue(EnableRowHighlightAnimationProperty, value);
    }

    public bool EnableSortingAnimation
    {
        get => (bool)GetValue(EnableSortingAnimationProperty);
        set => SetValue(EnableSortingAnimationProperty, value);
    }

    public bool EnableScanlineEffect
    {
        get => (bool)GetValue(EnableScanlineEffectProperty);
        set => SetValue(EnableScanlineEffectProperty, value);
    }

    public object HighlightedRow
    {
        get => GetValue(HighlightedRowProperty);
        set => SetValue(HighlightedRowProperty, value);
    }

    public TimeSpan RowHighlightDuration
    {
        get => (TimeSpan)GetValue(RowHighlightDurationProperty);
        set => SetValue(RowHighlightDurationProperty, value);
    }

    public bool ShowDataVisualization
    {
        get => (bool)GetValue(ShowDataVisualizationProperty);
        set => SetValue(ShowDataVisualizationProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<HoloDataGridRowEventArgs> RowHighlighted;
    public event EventHandler<HoloDataGridRowEventArgs> RowAnimationCompleted;

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode { get; private set; }

    public void SwitchToFullMode()
    {
        IsInSimplifiedMode = false;
        EnableRowHighlightAnimation = true;
        EnableSortingAnimation = true;
        EnableScanlineEffect = true;
        UpdateDataGridAppearance();
    }

    public void SwitchToSimplifiedMode()
    {
        IsInSimplifiedMode = true;
        EnableRowHighlightAnimation = false;
        EnableSortingAnimation = false;
        EnableScanlineEffect = false;
        UpdateDataGridAppearance();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => !_disposed && IsLoaded;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        HolographicIntensity = settings.MasterIntensity * settings.GlowIntensity;
        EnableRowHighlightAnimation = settings.EnabledFeatures.HasFlag(AnimationFeatures.BasicGlow);
        EnableSortingAnimation = settings.EnabledFeatures.HasFlag(AnimationFeatures.MotionEffects);
        EnableScanlineEffect = settings.EnabledFeatures.HasFlag(AnimationFeatures.AdvancedShaders);
        
        UpdateDataGridAppearance();
    }

    #endregion

    #region Fields

    private readonly Dictionary<DataGridRow, RowAnimationState> _rowAnimations = new();
    private readonly List<DataGridRow> _highlightQueue = new();
    private DispatcherTimer _scanlineTimer;
    private DispatcherTimer _highlightTimer;
    private double _scanlinePosition = 0;
    private ICollectionView _lastCollectionView;
    private bool _disposed = false;

    #endregion

    #region Constructor

    public HoloDataGrid()
    {
        DefaultStyleKey = typeof(HoloDataGrid);
        InitializeDataGrid();
        SetupAnimations();
        
        // Register with animation intensity manager
        AnimationIntensityManager.Instance.RegisterTarget(this);
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        LoadingRow += OnLoadingRow;
        UnloadingRow += OnUnloadingRow;
        Sorting += OnSorting;
        MouseEnter += OnMouseEnter;
        MouseLeave += OnMouseLeave;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Highlight a specific row with animation
    /// </summary>
    public void HighlightRow(object dataItem, TimeSpan? duration = null)
    {
        if (dataItem == null) return;

        var row = GetRowFromDataItem(dataItem);
        if (row != null)
        {
            HighlightRowInternal(row, duration ?? RowHighlightDuration);
            RowHighlighted?.Invoke(this, new HoloDataGridRowEventArgs { Row = row, DataItem = dataItem });
        }
    }

    /// <summary>
    /// Clear all row highlights
    /// </summary>
    public void ClearRowHighlights()
    {
        foreach (var kvp in _rowAnimations.ToList())
        {
            ClearRowHighlight(kvp.Key);
        }
    }

    /// <summary>
    /// Animate new data insertion
    /// </summary>
    public void AnimateDataInsertion(object newDataItem)
    {
        if (!EnableRowHighlightAnimation || IsInSimplifiedMode) return;

        var row = GetRowFromDataItem(newDataItem);
        if (row != null)
        {
            AnimateRowInsertion(row);
        }
    }

    /// <summary>
    /// Animate data update
    /// </summary>
    public void AnimateDataUpdate(object updatedDataItem)
    {
        if (!EnableRowHighlightAnimation || IsInSimplifiedMode) return;

        var row = GetRowFromDataItem(updatedDataItem);
        if (row != null)
        {
            AnimateRowUpdate(row);
        }
    }

    #endregion

    #region Private Methods

    private void InitializeDataGrid()
    {
        // Set EVE-style defaults
        Background = new SolidColorBrush(Color.FromArgb(40, 0, 20, 40));
        BorderBrush = new SolidColorBrush(Color.FromArgb(100, 64, 224, 255));
        BorderThickness = new Thickness(1);
        
        GridLinesVisibility = DataGridGridLinesVisibility.Horizontal;
        HorizontalGridLinesBrush = new SolidColorBrush(Color.FromArgb(60, 64, 224, 255));
        VerticalGridLinesBrush = new SolidColorBrush(Color.FromArgb(40, 64, 224, 255));
        
        HeadersVisibility = DataGridHeadersVisibility.Column;
        CanUserAddRows = false;
        CanUserDeleteRows = false;
        CanUserReorderColumns = true;
        CanUserResizeColumns = true;
        CanUserSortColumns = true;
        
        SelectionMode = DataGridSelectionMode.Single;
        SelectionUnit = DataGridSelectionUnit.FullRow;
        
        UpdateDataGridAppearance();
    }

    private void SetupAnimations()
    {
        // Scanline animation timer
        _scanlineTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50) // 20 FPS
        };
        _scanlineTimer.Tick += OnScanlineTick;

        // Highlight management timer
        _highlightTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _highlightTimer.Tick += OnHighlightTick;
    }

    private void OnScanlineTick(object sender, EventArgs e)
    {
        if (!EnableScanlineEffect || IsInSimplifiedMode) return;

        _scanlinePosition += 2;
        if (_scanlinePosition > ActualHeight + 50)
        {
            _scanlinePosition = -50;
        }

        UpdateScanlineEffect();
    }

    private void OnHighlightTick(object sender, EventArgs e)
    {
        // Process highlight queue
        if (_highlightQueue.Any())
        {
            var row = _highlightQueue.First();
            _highlightQueue.RemoveAt(0);
            
            if (_rowAnimations.ContainsKey(row))
            {
                var state = _rowAnimations[row];
                state.RemainingTime -= TimeSpan.FromMilliseconds(100);
                
                if (state.RemainingTime <= TimeSpan.Zero)
                {
                    ClearRowHighlight(row);
                }
            }
        }

        // Clean up expired animations
        var expiredRows = _rowAnimations
            .Where(kvp => kvp.Value.RemainingTime <= TimeSpan.Zero)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var row in expiredRows)
        {
            ClearRowHighlight(row);
        }
    }

    private void UpdateScanlineEffect()
    {
        // This would typically be implemented with a custom control template
        // For now, we'll apply it to visible rows
        var container = GetVisualChild(0) as Border;
        if (container?.Effect is DropShadowEffect effect)
        {
            effect.Direction = 90;
            effect.BlurRadius = 10;
            effect.ShadowDepth = _scanlinePosition - ActualHeight / 2;
            effect.Opacity = 0.1 * HolographicIntensity;
        }
    }

    private void UpdateDataGridAppearance()
    {
        var color = GetEVEColor(EVEColorScheme);

        // Update grid colors
        BorderBrush = new SolidColorBrush(Color.FromArgb(150, color.R, color.G, color.B));
        HorizontalGridLinesBrush = new SolidColorBrush(Color.FromArgb(60, color.R, color.G, color.B));
        VerticalGridLinesBrush = new SolidColorBrush(Color.FromArgb(40, color.R, color.G, color.B));

        // Apply holographic glow effect
        if (!IsInSimplifiedMode)
        {
            Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 8 * HolographicIntensity,
                ShadowDepth = 0,
                Opacity = 0.3 * HolographicIntensity
            };
        }
        else
        {
            Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 2,
                ShadowDepth = 0,
                Opacity = 0.1 * HolographicIntensity
            };
        }

        UpdateRowStyles();
    }

    private void UpdateRowStyles()
    {
        var color = GetEVEColor(EVEColorScheme);

        // Create row style
        var rowStyle = new Style(typeof(DataGridRow));
        
        // Normal state
        rowStyle.Setters.Add(new Setter(BackgroundProperty, 
            new SolidColorBrush(Color.FromArgb(30, 0, 20, 40))));
        rowStyle.Setters.Add(new Setter(ForegroundProperty, 
            new SolidColorBrush(Color.FromArgb(220, color.R, color.G, color.B))));
        
        // Hover trigger
        var hoverTrigger = new Trigger { Property = IsMouseOverProperty, Value = true };
        hoverTrigger.Setters.Add(new Setter(BackgroundProperty, 
            new SolidColorBrush(Color.FromArgb(60, color.R, color.G, color.B))));
        
        // Selected trigger
        var selectedTrigger = new Trigger { Property = IsSelectedProperty, Value = true };
        selectedTrigger.Setters.Add(new Setter(BackgroundProperty, 
            new SolidColorBrush(Color.FromArgb(80, color.R, color.G, color.B))));
        
        rowStyle.Triggers.Add(hoverTrigger);
        rowStyle.Triggers.Add(selectedTrigger);
        
        RowStyle = rowStyle;

        // Create column header style
        var headerStyle = new Style(typeof(DataGridColumnHeader));
        headerStyle.Setters.Add(new Setter(BackgroundProperty, 
            new SolidColorBrush(Color.FromArgb(80, color.R, color.G, color.B))));
        headerStyle.Setters.Add(new Setter(ForegroundProperty, 
            new SolidColorBrush(Colors.White)));
        headerStyle.Setters.Add(new Setter(FontWeightProperty, FontWeights.Bold));
        headerStyle.Setters.Add(new Setter(BorderBrushProperty, 
            new SolidColorBrush(Color.FromArgb(120, color.R, color.G, color.B))));
        headerStyle.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(0, 0, 1, 1)));
        headerStyle.Setters.Add(new Setter(PaddingProperty, new Thickness(8, 6, 8, 6)));
        
        ColumnHeaderStyle = headerStyle;

        // Create cell style
        var cellStyle = new Style(typeof(DataGridCell));
        cellStyle.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(0)));
        cellStyle.Setters.Add(new Setter(PaddingProperty, new Thickness(8, 4, 8, 4)));
        cellStyle.Setters.Add(new Setter(VerticalAlignmentProperty, VerticalAlignment.Center));
        
        // Cell focus trigger
        var cellFocusTrigger = new Trigger { Property = IsFocusedProperty, Value = true };
        cellFocusTrigger.Setters.Add(new Setter(BackgroundProperty, 
            new SolidColorBrush(Color.FromArgb(100, color.R, color.G, color.B))));
        
        cellStyle.Triggers.Add(cellFocusTrigger);
        CellStyle = cellStyle;
    }

    private void HighlightRowInternal(DataGridRow row, TimeSpan duration)
    {
        if (row == null || !EnableRowHighlightAnimation || IsInSimplifiedMode) return;

        // Clear existing animation for this row
        if (_rowAnimations.ContainsKey(row))
        {
            ClearRowHighlight(row);
        }

        var color = GetEVEColor(EVEColorScheme);
        var highlightBrush = new SolidColorBrush(Color.FromArgb(120, color.R, color.G, color.B));

        // Create highlight animation
        var highlightAnimation = new DoubleAnimation
        {
            From = 0.0,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(300),
            AutoReverse = true,
            RepeatBehavior = new RepeatBehavior(3)
        };

        // Store animation state
        _rowAnimations[row] = new RowAnimationState
        {
            RemainingTime = duration,
            OriginalBackground = row.Background,
            HighlightBrush = highlightBrush
        };

        // Apply highlight
        row.Background = highlightBrush;
        
        // Add glow effect
        if (!IsInSimplifiedMode)
        {
            row.Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 10,
                ShadowDepth = 0,
                Opacity = 0.8 * HolographicIntensity
            };
        }

        // Start opacity animation
        row.BeginAnimation(OpacityProperty, highlightAnimation);

        // Add to highlight queue for cleanup
        if (!_highlightQueue.Contains(row))
        {
            _highlightQueue.Add(row);
        }
    }

    private void ClearRowHighlight(DataGridRow row)
    {
        if (row == null || !_rowAnimations.ContainsKey(row)) return;

        var state = _rowAnimations[row];
        
        // Restore original appearance
        row.Background = state.OriginalBackground;
        row.Effect = null;
        row.BeginAnimation(OpacityProperty, null);
        row.Opacity = 1.0;

        // Remove from tracking
        _rowAnimations.Remove(row);
        _highlightQueue.Remove(row);

        RowAnimationCompleted?.Invoke(this, new HoloDataGridRowEventArgs { Row = row });
    }

    private void AnimateRowInsertion(DataGridRow row)
    {
        if (row == null) return;

        var color = GetEVEColor(EVEColorScheme);
        
        // Slide in animation
        var slideTransform = new TranslateTransform(-ActualWidth, 0);
        row.RenderTransform = slideTransform;

        var slideAnimation = new DoubleAnimation
        {
            From = -ActualWidth,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(400),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        slideTransform.BeginAnimation(TranslateTransform.XProperty, slideAnimation);

        // Highlight new row
        HighlightRowInternal(row, TimeSpan.FromSeconds(3));
    }

    private void AnimateRowUpdate(DataGridRow row)
    {
        if (row == null) return;

        // Flash animation for updates
        var flashAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.3,
            Duration = TimeSpan.FromMilliseconds(150),
            AutoReverse = true,
            RepeatBehavior = new RepeatBehavior(2)
        };

        row.BeginAnimation(OpacityProperty, flashAnimation);

        // Brief highlight
        HighlightRowInternal(row, TimeSpan.FromSeconds(1));
    }

    private DataGridRow GetRowFromDataItem(object dataItem)
    {
        var index = Items.IndexOf(dataItem);
        if (index >= 0)
        {
            return GetRow(index);
        }
        return null;
    }

    private DataGridRow GetRow(int index)
    {
        return ItemContainerGenerator.ContainerFromIndex(index) as DataGridRow;
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

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (EnableScanlineEffect && !IsInSimplifiedMode)
            _scanlineTimer.Start();
            
        _highlightTimer.Start();

        // Monitor data source changes
        if (ItemsSource is INotifyCollectionChanged collection)
        {
            collection.CollectionChanged += OnItemsSourceCollectionChanged;
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _scanlineTimer?.Stop();
        _highlightTimer?.Stop();
        
        // Clean up
        ClearRowHighlights();
        
        if (ItemsSource is INotifyCollectionChanged collection)
        {
            collection.CollectionChanged -= OnItemsSourceCollectionChanged;
        }
        
        // Unregister from animation intensity manager
        AnimationIntensityManager.Instance.UnregisterTarget(this);
        
        _disposed = true;
    }

    #endregion

    #region Event Handlers

    private void OnLoadingRow(object sender, DataGridRowEventArgs e)
    {
        // Apply row-specific animations when rows are loaded
        if (EnableRowHighlightAnimation && !IsInSimplifiedMode)
        {
            var row = e.Row;
            row.Opacity = 0;
            
            var fadeInAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(200)
            };
            
            row.BeginAnimation(OpacityProperty, fadeInAnimation);
        }
    }

    private void OnUnloadingRow(object sender, DataGridRowEventArgs e)
    {
        // Clean up any animations for unloading rows
        if (_rowAnimations.ContainsKey(e.Row))
        {
            ClearRowHighlight(e.Row);
        }
    }

    private void OnSorting(object sender, DataGridSortingEventArgs e)
    {
        if (!EnableSortingAnimation || IsInSimplifiedMode) return;

        // Add visual feedback for sorting
        var header = e.Column.Header as FrameworkElement;
        if (header != null)
        {
            var pulseAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.6,
                Duration = TimeSpan.FromMilliseconds(100),
                AutoReverse = true
            };
            
            header.BeginAnimation(OpacityProperty, pulseAnimation);
        }
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (!IsInSimplifiedMode && EnableScanlineEffect)
        {
            // Intensify scanline effect on hover
            if (Effect is DropShadowEffect effect)
            {
                effect.Opacity = 0.5 * HolographicIntensity;
            }
        }
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (!IsInSimplifiedMode && EnableScanlineEffect)
        {
            // Return to normal scanline intensity
            if (Effect is DropShadowEffect effect)
            {
                effect.Opacity = 0.3 * HolographicIntensity;
            }
        }
    }

    private void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (!EnableRowHighlightAnimation || IsInSimplifiedMode) return;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (var newItem in e.NewItems)
                {
                    // Delay to allow UI to update
                    Dispatcher.BeginInvoke(new Action(() => AnimateDataInsertion(newItem)), 
                        DispatcherPriority.Render);
                }
                break;
                
            case NotifyCollectionChangedAction.Replace:
                foreach (var newItem in e.NewItems)
                {
                    Dispatcher.BeginInvoke(new Action(() => AnimateDataUpdate(newItem)), 
                        DispatcherPriority.Render);
                }
                break;
        }
    }

    private static void OnHolographicIntensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDataGrid dataGrid)
            dataGrid.UpdateDataGridAppearance();
    }

    private static void OnEVEColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDataGrid dataGrid)
            dataGrid.UpdateDataGridAppearance();
    }

    private static void OnEnableRowHighlightAnimationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDataGrid dataGrid && !(bool)e.NewValue)
        {
            dataGrid.ClearRowHighlights();
        }
    }

    private static void OnEnableScanlineEffectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDataGrid dataGrid)
        {
            if ((bool)e.NewValue && !dataGrid.IsInSimplifiedMode)
                dataGrid._scanlineTimer.Start();
            else
                dataGrid._scanlineTimer.Stop();
        }
    }

    private static void OnHighlightedRowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDataGrid dataGrid && e.NewValue != null)
        {
            dataGrid.HighlightRow(e.NewValue);
        }
    }

    private static void OnShowDataVisualizationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HoloDataGrid dataGrid)
        {
            // Could add mini sparklines or other visualizations to cells
            dataGrid.UpdateDataGridAppearance();
        }
    }

    #endregion
}

/// <summary>
/// Row animation state tracking
/// </summary>
internal class RowAnimationState
{
    public TimeSpan RemainingTime { get; set; }
    public Brush OriginalBackground { get; set; }
    public Brush HighlightBrush { get; set; }
}

/// <summary>
/// Event args for holographic data grid row events
/// </summary>
public class HoloDataGridRowEventArgs : EventArgs
{
    public DataGridRow Row { get; set; }
    public object DataItem { get; set; }
}