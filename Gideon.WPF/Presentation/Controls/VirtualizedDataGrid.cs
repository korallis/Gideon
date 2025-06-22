// ==========================================================================
// VirtualizedDataGrid.cs - High-Performance Virtualized Data Grid
// ==========================================================================
// Advanced virtualized data grid for handling large datasets with
// minimal memory footprint and optimized rendering performance.
//
// Author: Gideon Development Team
// Created: June 21, 2025
// ==========================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Virtualization strategies for large datasets
/// </summary>
public enum VirtualizationStrategy
{
    Standard,        // Standard WPF virtualization
    Enhanced,        // Enhanced with item recycling
    Streaming,       // Streaming data with lazy loading
    Hierarchical     // Hierarchical data virtualization
}

/// <summary>
/// Performance optimization levels
/// </summary>
public enum PerformanceLevel
{
    Balanced,        // Balance between features and performance
    Speed,           // Prioritize rendering speed
    Memory,          // Minimize memory usage
    Quality          // Best visual quality
}

/// <summary>
/// High-performance virtualized data grid
/// </summary>
public class VirtualizedDataGrid : DataGrid
{
    #region Dependency Properties

    public static readonly DependencyProperty VirtualizationStrategyProperty =
        DependencyProperty.Register(nameof(VirtualizationStrategy), typeof(VirtualizationStrategy), typeof(VirtualizedDataGrid),
            new PropertyMetadata(VirtualizationStrategy.Enhanced, OnVirtualizationStrategyChanged));

    public static readonly DependencyProperty PerformanceLevelProperty =
        DependencyProperty.Register(nameof(PerformanceLevel), typeof(PerformanceLevel), typeof(VirtualizedDataGrid),
            new PropertyMetadata(PerformanceLevel.Balanced, OnPerformanceLevelChanged));

    public static readonly DependencyProperty MaxVisibleItemsProperty =
        DependencyProperty.Register(nameof(MaxVisibleItems), typeof(int), typeof(VirtualizedDataGrid),
            new PropertyMetadata(1000, OnMaxVisibleItemsChanged));

    public static readonly DependencyProperty EnableLazyLoadingProperty =
        DependencyProperty.Register(nameof(EnableLazyLoading), typeof(bool), typeof(VirtualizedDataGrid),
            new PropertyMetadata(true));

    public static readonly DependencyProperty EnableItemRecyclingProperty =
        DependencyProperty.Register(nameof(EnableItemRecycling), typeof(bool), typeof(VirtualizedDataGrid),
            new PropertyMetadata(true));

    public static readonly DependencyProperty BufferSizeProperty =
        DependencyProperty.Register(nameof(BufferSize), typeof(int), typeof(VirtualizedDataGrid),
            new PropertyMetadata(50));

    #endregion

    #region Properties

    public VirtualizationStrategy VirtualizationStrategy
    {
        get => (VirtualizationStrategy)GetValue(VirtualizationStrategyProperty);
        set => SetValue(VirtualizationStrategyProperty, value);
    }

    public PerformanceLevel PerformanceLevel
    {
        get => (PerformanceLevel)GetValue(PerformanceLevelProperty);
        set => SetValue(PerformanceLevelProperty, value);
    }

    public int MaxVisibleItems
    {
        get => (int)GetValue(MaxVisibleItemsProperty);
        set => SetValue(MaxVisibleItemsProperty, value);
    }

    public bool EnableLazyLoading
    {
        get => (bool)GetValue(EnableLazyLoadingProperty);
        set => SetValue(EnableLazyLoadingProperty, value);
    }

    public bool EnableItemRecycling
    {
        get => (bool)GetValue(EnableItemRecyclingProperty);
        set => SetValue(EnableItemRecyclingProperty, value);
    }

    public int BufferSize
    {
        get => (int)GetValue(BufferSizeProperty);
        set => SetValue(BufferSizeProperty, value);
    }

    public double RenderingPerformance { get; private set; } = 60.0; // FPS
    public long MemoryUsage { get; private set; } = 0; // Bytes

    #endregion

    #region Fields

    private readonly PerformanceMonitor _performanceMonitor;
    private readonly ItemRecyclingPool _recyclingPool;
    private ScrollViewer? _scrollViewer;
    private ICollectionView? _collectionView;
    private readonly List<WeakReference> _visibleItems = new();

    #endregion

    #region Constructor

    public VirtualizedDataGrid()
    {
        _performanceMonitor = new PerformanceMonitor();
        _recyclingPool = new ItemRecyclingPool();

        // Enable built-in virtualization
        VirtualizingPanel.SetVirtualizationMode(this, VirtualizationMode.Recycling);
        VirtualizingPanel.SetScrollUnit(this, ScrollUnit.Item);
        VirtualizingPanel.SetIsVirtualizing(this, true);

        Loaded += OnLoaded;
        ItemContainerGenerator.StatusChanged += OnItemContainerGeneratorStatusChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Optimize grid for large dataset performance
    /// </summary>
    public void OptimizeForLargeDataset()
    {
        // Disable animations for better performance
        SetCurrentValue(EnableRowVirtualizationProperty, true);
        SetCurrentValue(EnableColumnVirtualizationProperty, true);
        
        // Optimize selection mode
        SetCurrentValue(SelectionModeProperty, DataGridSelectionMode.Single);
        
        // Disable sorting for large datasets
        foreach (var column in Columns)
        {
            column.CanUserSort = false;
        }

        // Set performance-optimized styles
        ApplyPerformanceOptimizations();
    }

    /// <summary>
    /// Refresh visible items without full reload
    /// </summary>
    public void RefreshVisibleItems()
    {
        if (_scrollViewer == null) return;

        var firstVisibleIndex = GetFirstVisibleItemIndex();
        var lastVisibleIndex = GetLastVisibleItemIndex();

        for (int i = firstVisibleIndex; i <= lastVisibleIndex; i++)
        {
            var container = ItemContainerGenerator.ContainerFromIndex(i) as DataGridRow;
            container?.InvalidateVisual();
        }
    }

    /// <summary>
    /// Get current performance metrics
    /// </summary>
    public PerformanceMetrics GetPerformanceMetrics()
    {
        return new PerformanceMetrics
        {
            FrameRate = _performanceMonitor.CurrentFrameRate,
            MemoryUsage = GC.GetTotalMemory(false),
            VisibleItemCount = _visibleItems.Count(wr => wr.IsAlive),
            TotalItemCount = Items.Count,
            VirtualizationRatio = CalculateVirtualizationRatio()
        };
    }

    #endregion

    #region Protected Methods

    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        base.OnItemsSourceChanged(oldValue, newValue);
        
        _collectionView = CollectionViewSource.GetDefaultView(newValue);
        
        if (_collectionView is INotifyCollectionChanged notifyCollection)
        {
            notifyCollection.CollectionChanged += OnCollectionChanged;
        }

        OptimizeForDataSource(newValue);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        _performanceMonitor.BeginFrame();
        
        try
        {
            base.OnRender(drawingContext);
        }
        finally
        {
            _performanceMonitor.EndFrame();
            RenderingPerformance = _performanceMonitor.CurrentFrameRate;
        }
    }

    #endregion

    #region Private Methods

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _scrollViewer = FindScrollViewer(this);
        if (_scrollViewer != null)
        {
            _scrollViewer.ScrollChanged += OnScrollChanged;
        }

        ApplyPerformanceOptimizations();
    }

    private void OnItemContainerGeneratorStatusChanged(object? sender, EventArgs e)
    {
        if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
        {
            UpdateVisibleItems();
        }
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Handle collection changes efficiently
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                HandleItemsAdded(e.NewItems, e.NewStartingIndex);
                break;
            case NotifyCollectionChangedAction.Remove:
                HandleItemsRemoved(e.OldItems, e.OldStartingIndex);
                break;
            case NotifyCollectionChangedAction.Reset:
                HandleCollectionReset();
                break;
        }
    }

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (EnableLazyLoading && Math.Abs(e.VerticalChange) > 0)
        {
            CheckForLazyLoading();
        }

        UpdateVisibleItems();
    }

    private void ApplyPerformanceOptimizations()
    {
        switch (PerformanceLevel)
        {
            case PerformanceLevel.Speed:
                ApplySpeedOptimizations();
                break;
            case PerformanceLevel.Memory:
                ApplyMemoryOptimizations();
                break;
            case PerformanceLevel.Quality:
                ApplyQualityOptimizations();
                break;
            case PerformanceLevel.Balanced:
            default:
                ApplyBalancedOptimizations();
                break;
        }
    }

    private void ApplySpeedOptimizations()
    {
        SetCurrentValue(EnableRowVirtualizationProperty, true);
        SetCurrentValue(EnableColumnVirtualizationProperty, true);
        VirtualizingPanel.SetVirtualizationMode(this, VirtualizationMode.Standard);
        
        // Disable animations
        foreach (var column in Columns)
        {
            column.CanUserResize = false;
        }
    }

    private void ApplyMemoryOptimizations()
    {
        VirtualizingPanel.SetVirtualizationMode(this, VirtualizationMode.Recycling);
        SetCurrentValue(BufferSizeProperty, 20); // Smaller buffer
        
        // Enable aggressive item recycling
        SetCurrentValue(EnableItemRecyclingProperty, true);
    }

    private void ApplyQualityOptimizations()
    {
        SetCurrentValue(BufferSizeProperty, 100); // Larger buffer for smoother scrolling
        VirtualizingPanel.SetVirtualizationMode(this, VirtualizationMode.Recycling);
    }

    private void ApplyBalancedOptimizations()
    {
        SetCurrentValue(EnableRowVirtualizationProperty, true);
        SetCurrentValue(BufferSizeProperty, 50);
        VirtualizingPanel.SetVirtualizationMode(this, VirtualizationMode.Recycling);
    }

    private void OptimizeForDataSource(IEnumerable dataSource)
    {
        if (dataSource == null) return;

        var count = 0;
        var enumerator = dataSource.GetEnumerator();
        
        // Count items efficiently
        while (enumerator.MoveNext() && count < 10000)
        {
            count++;
        }

        // Apply optimizations based on data size
        if (count > 1000)
        {
            OptimizeForLargeDataset();
        }
    }

    private void UpdateVisibleItems()
    {
        _visibleItems.Clear();
        
        var firstIndex = GetFirstVisibleItemIndex();
        var lastIndex = GetLastVisibleItemIndex();

        for (int i = firstIndex; i <= lastIndex; i++)
        {
            var item = Items[i];
            if (item != null)
            {
                _visibleItems.Add(new WeakReference(item));
            }
        }

        // Clean up dead references
        _visibleItems.RemoveAll(wr => !wr.IsAlive);
    }

    private int GetFirstVisibleItemIndex()
    {
        if (_scrollViewer == null) return 0;
        
        var itemHeight = GetAverageItemHeight();
        if (itemHeight <= 0) return 0;
        
        return Math.Max(0, (int)(_scrollViewer.VerticalOffset / itemHeight));
    }

    private int GetLastVisibleItemIndex()
    {
        if (_scrollViewer == null) return Items.Count - 1;
        
        var itemHeight = GetAverageItemHeight();
        if (itemHeight <= 0) return Items.Count - 1;
        
        var visibleCount = (int)(_scrollViewer.ViewportHeight / itemHeight) + 1;
        return Math.Min(Items.Count - 1, GetFirstVisibleItemIndex() + visibleCount);
    }

    private double GetAverageItemHeight()
    {
        // Return estimated item height based on first few items
        if (Items.Count == 0) return 25.0; // Default height
        
        var totalHeight = 0.0;
        var count = 0;
        
        for (int i = 0; i < Math.Min(5, Items.Count); i++)
        {
            var container = ItemContainerGenerator.ContainerFromIndex(i) as DataGridRow;
            if (container != null)
            {
                totalHeight += container.ActualHeight;
                count++;
            }
        }
        
        return count > 0 ? totalHeight / count : 25.0;
    }

    private void CheckForLazyLoading()
    {
        // Implement lazy loading logic for streaming data
        var threshold = Math.Max(100, BufferSize * 2);
        
        if (_scrollViewer != null && 
            _scrollViewer.VerticalOffset + _scrollViewer.ViewportHeight >= 
            _scrollViewer.ExtentHeight - threshold)
        {
            OnLazyLoadRequested?.Invoke();
        }
    }

    private void HandleItemsAdded(IList? newItems, int startIndex)
    {
        // Efficiently handle added items without full refresh
        if (EnableItemRecycling && newItems != null)
        {
            foreach (var item in newItems)
            {
                _recyclingPool.ReturnItem(item);
            }
        }
    }

    private void HandleItemsRemoved(IList? oldItems, int startIndex)
    {
        // Clean up removed items
        UpdateVisibleItems();
    }

    private void HandleCollectionReset()
    {
        _visibleItems.Clear();
        _recyclingPool.Clear();
        UpdateLayout();
    }

    private double CalculateVirtualizationRatio()
    {
        if (Items.Count == 0) return 1.0;
        return (double)_visibleItems.Count(wr => wr.IsAlive) / Items.Count;
    }

    private static ScrollViewer? FindScrollViewer(DependencyObject parent)
    {
        var childCount = VisualTreeHelper.GetChildrenCount(parent);
        
        for (int i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            
            if (child is ScrollViewer scrollViewer)
                return scrollViewer;
            
            var result = FindScrollViewer(child);
            if (result != null)
                return result;
        }
        
        return null;
    }

    #endregion

    #region Events

    public event Action? OnLazyLoadRequested;

    #endregion

    #region Event Handlers

    private static void OnVirtualizationStrategyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is VirtualizedDataGrid grid)
        {
            grid.ApplyPerformanceOptimizations();
        }
    }

    private static void OnPerformanceLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is VirtualizedDataGrid grid)
        {
            grid.ApplyPerformanceOptimizations();
        }
    }

    private static void OnMaxVisibleItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is VirtualizedDataGrid grid)
        {
            grid.UpdateVisibleItems();
        }
    }

    #endregion
}

/// <summary>
/// Performance monitoring helper
/// </summary>
public class PerformanceMonitor
{
    private DateTime _lastFrameTime = DateTime.Now;
    private readonly Queue<double> _frameTimes = new();
    private const int MaxFrameHistory = 60;

    public double CurrentFrameRate { get; private set; } = 60.0;

    public void BeginFrame()
    {
        _lastFrameTime = DateTime.Now;
    }

    public void EndFrame()
    {
        var frameTime = (DateTime.Now - _lastFrameTime).TotalMilliseconds;
        _frameTimes.Enqueue(frameTime);
        
        if (_frameTimes.Count > MaxFrameHistory)
        {
            _frameTimes.Dequeue();
        }
        
        if (_frameTimes.Count > 0)
        {
            var averageFrameTime = _frameTimes.Average();
            CurrentFrameRate = averageFrameTime > 0 ? 1000.0 / averageFrameTime : 60.0;
        }
    }
}

/// <summary>
/// Item recycling pool for memory optimization
/// </summary>
public class ItemRecyclingPool
{
    private readonly Queue<object> _availableItems = new();
    private readonly HashSet<object> _inUseItems = new();

    public object GetItem()
    {
        if (_availableItems.Count > 0)
        {
            var item = _availableItems.Dequeue();
            _inUseItems.Add(item);
            return item;
        }
        
        return new object(); // Create new if none available
    }

    public void ReturnItem(object item)
    {
        if (_inUseItems.Remove(item))
        {
            _availableItems.Enqueue(item);
        }
    }

    public void Clear()
    {
        _availableItems.Clear();
        _inUseItems.Clear();
    }
}

/// <summary>
/// Performance metrics data
/// </summary>
public class PerformanceMetrics
{
    public double FrameRate { get; set; }
    public long MemoryUsage { get; set; }
    public int VisibleItemCount { get; set; }
    public int TotalItemCount { get; set; }
    public double VirtualizationRatio { get; set; }
}