// ==========================================================================
// ResponsiveGrid.cs - Responsive Grid System for Different Screen Sizes
// ==========================================================================
// Advanced responsive grid layout system that adapts to different screen
// sizes and resolutions for optimal user experience.
//
// Author: Gideon Development Team
// Created: June 21, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Screen size breakpoints for responsive design
/// </summary>
public enum ScreenSize
{
    XSmall = 0,  // < 768px
    Small = 1,   // 768px - 992px
    Medium = 2,  // 992px - 1200px
    Large = 3,   // 1200px - 1600px
    XLarge = 4   // > 1600px
}

/// <summary>
/// Responsive column configuration
/// </summary>
public class ResponsiveColumn
{
    public int XSmallColumns { get; set; } = 12;
    public int SmallColumns { get; set; } = 12;
    public int MediumColumns { get; set; } = 6;
    public int LargeColumns { get; set; } = 4;
    public int XLargeColumns { get; set; } = 3;

    public int GetColumnsForSize(ScreenSize size)
    {
        return size switch
        {
            ScreenSize.XSmall => XSmallColumns,
            ScreenSize.Small => SmallColumns,
            ScreenSize.Medium => MediumColumns,
            ScreenSize.Large => LargeColumns,
            ScreenSize.XLarge => XLargeColumns,
            _ => MediumColumns
        };
    }
}

/// <summary>
/// Responsive grid panel with automatic layout adjustment
/// </summary>
public class ResponsiveGrid : Panel
{
    #region Dependency Properties

    public static readonly DependencyProperty MaxColumnsProperty =
        DependencyProperty.Register(nameof(MaxColumns), typeof(int), typeof(ResponsiveGrid),
            new FrameworkPropertyMetadata(12, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty MinColumnWidthProperty =
        DependencyProperty.Register(nameof(MinColumnWidth), typeof(double), typeof(ResponsiveGrid),
            new FrameworkPropertyMetadata(200.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty ColumnGapProperty =
        DependencyProperty.Register(nameof(ColumnGap), typeof(double), typeof(ResponsiveGrid),
            new FrameworkPropertyMetadata(16.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty RowGapProperty =
        DependencyProperty.Register(nameof(RowGap), typeof(double), typeof(ResponsiveGrid),
            new FrameworkPropertyMetadata(16.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty AutoFitProperty =
        DependencyProperty.Register(nameof(AutoFit), typeof(bool), typeof(ResponsiveGrid),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure));

    // Attached Properties for child elements
    public static readonly DependencyProperty ColumnSpanProperty =
        DependencyProperty.RegisterAttached("ColumnSpan", typeof(ResponsiveColumn), typeof(ResponsiveGrid),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

    public static readonly DependencyProperty RowSpanProperty =
        DependencyProperty.RegisterAttached("RowSpan", typeof(int), typeof(ResponsiveGrid),
            new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

    public static readonly DependencyProperty OrderProperty =
        DependencyProperty.RegisterAttached("Order", typeof(int), typeof(ResponsiveGrid),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

    #endregion

    #region Properties

    public int MaxColumns
    {
        get => (int)GetValue(MaxColumnsProperty);
        set => SetValue(MaxColumnsProperty, value);
    }

    public double MinColumnWidth
    {
        get => (double)GetValue(MinColumnWidthProperty);
        set => SetValue(MinColumnWidthProperty, value);
    }

    public double ColumnGap
    {
        get => (double)GetValue(ColumnGapProperty);
        set => SetValue(ColumnGapProperty, value);
    }

    public double RowGap
    {
        get => (double)GetValue(RowGapProperty);
        set => SetValue(RowGapProperty, value);
    }

    public bool AutoFit
    {
        get => (bool)GetValue(AutoFitProperty);
        set => SetValue(AutoFitProperty, value);
    }

    public ScreenSize CurrentScreenSize { get; private set; } = ScreenSize.Medium;

    #endregion

    #region Fields

    private int _actualColumns = 1;
    private readonly List<GridItem> _gridItems = new();

    #endregion

    #region Attached Property Accessors

    public static void SetColumnSpan(DependencyObject element, ResponsiveColumn value)
    {
        element.SetValue(ColumnSpanProperty, value);
    }

    public static ResponsiveColumn GetColumnSpan(DependencyObject element)
    {
        return (ResponsiveColumn)element.GetValue(ColumnSpanProperty);
    }

    public static void SetRowSpan(DependencyObject element, int value)
    {
        element.SetValue(RowSpanProperty, value);
    }

    public static int GetRowSpan(DependencyObject element)
    {
        return (int)element.GetValue(RowSpanProperty);
    }

    public static void SetOrder(DependencyObject element, int value)
    {
        element.SetValue(OrderProperty, value);
    }

    public static int GetOrder(DependencyObject element)
    {
        return (int)element.GetValue(OrderProperty);
    }

    #endregion

    #region Protected Methods

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Children.Count == 0)
            return new Size(0, 0);

        // Determine current screen size
        CurrentScreenSize = DetermineScreenSize(availableSize.Width);

        // Calculate actual columns based on available width
        CalculateActualColumns(availableSize.Width);

        // Build grid layout
        BuildGridLayout(availableSize);

        // Measure all children
        var totalHeight = 0.0;
        var maxRowHeight = 0.0;
        var currentRow = 0;

        foreach (var item in _gridItems)
        {
            if (item.Row != currentRow)
            {
                totalHeight += maxRowHeight + (currentRow > 0 ? RowGap : 0);
                maxRowHeight = 0;
                currentRow = item.Row;
            }

            var cellWidth = CalculateCellWidth(availableSize.Width, item.ColumnSpan);
            var cellHeight = double.PositiveInfinity;

            item.Element.Measure(new Size(cellWidth, cellHeight));
            maxRowHeight = Math.Max(maxRowHeight, item.Element.DesiredSize.Height);
        }

        totalHeight += maxRowHeight;

        return new Size(availableSize.Width, totalHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (var item in _gridItems)
        {
            var cellWidth = CalculateCellWidth(finalSize.Width, item.ColumnSpan);
            var cellHeight = CalculateRowHeight(item.Row);
            
            var x = CalculateColumnPosition(finalSize.Width, item.Column);
            var y = CalculateRowPosition(item.Row);

            var rect = new Rect(x, y, cellWidth, cellHeight);
            item.Element.Arrange(rect);
        }

        return finalSize;
    }

    #endregion

    #region Private Methods

    private ScreenSize DetermineScreenSize(double width)
    {
        return width switch
        {
            < 768 => ScreenSize.XSmall,
            < 992 => ScreenSize.Small,
            < 1200 => ScreenSize.Medium,
            < 1600 => ScreenSize.Large,
            _ => ScreenSize.XLarge
        };
    }

    private void CalculateActualColumns(double availableWidth)
    {
        if (AutoFit)
        {
            var maxPossible = Math.Max(1, (int)((availableWidth + ColumnGap) / (MinColumnWidth + ColumnGap)));
            _actualColumns = Math.Min(maxPossible, MaxColumns);
        }
        else
        {
            _actualColumns = MaxColumns;
        }
    }

    private void BuildGridLayout(Size availableSize)
    {
        _gridItems.Clear();

        // Sort children by order
        var orderedChildren = Children.Cast<UIElement>()
            .OrderBy(child => GetOrder(child))
            .ToList();

        var currentRow = 0;
        var currentColumn = 0;

        foreach (var child in orderedChildren)
        {
            var columnSpanConfig = GetColumnSpan(child) ?? new ResponsiveColumn();
            var columnSpan = Math.Min(columnSpanConfig.GetColumnsForSize(CurrentScreenSize), _actualColumns);
            var rowSpan = GetRowSpan(child);

            // Check if we need to wrap to next row
            if (currentColumn + columnSpan > _actualColumns)
            {
                currentRow++;
                currentColumn = 0;
            }

            _gridItems.Add(new GridItem
            {
                Element = child,
                Column = currentColumn,
                Row = currentRow,
                ColumnSpan = columnSpan,
                RowSpan = rowSpan
            });

            currentColumn += columnSpan;

            // If we've filled the row, move to next
            if (currentColumn >= _actualColumns)
            {
                currentRow++;
                currentColumn = 0;
            }
        }
    }

    private double CalculateCellWidth(double totalWidth, int columnSpan)
    {
        var availableWidth = totalWidth - ((_actualColumns - 1) * ColumnGap);
        var singleColumnWidth = availableWidth / _actualColumns;
        return (singleColumnWidth * columnSpan) + ((columnSpan - 1) * ColumnGap);
    }

    private double CalculateColumnPosition(double totalWidth, int column)
    {
        var availableWidth = totalWidth - ((_actualColumns - 1) * ColumnGap);
        var singleColumnWidth = availableWidth / _actualColumns;
        return column * (singleColumnWidth + ColumnGap);
    }

    private double CalculateRowHeight(int row)
    {
        // For now, return auto height - could be enhanced with explicit row heights
        return double.NaN; // Auto height
    }

    private double CalculateRowPosition(int row)
    {
        // Calculate position based on previous rows
        var position = 0.0;
        var currentRowInItems = _gridItems.Where(item => item.Row < row).GroupBy(item => item.Row);
        
        foreach (var rowGroup in currentRowInItems)
        {
            var maxHeight = rowGroup.Max(item => item.Element.DesiredSize.Height);
            position += maxHeight + RowGap;
        }

        return position;
    }

    #endregion

    #region Helper Classes

    private class GridItem
    {
        public UIElement Element { get; set; } = null!;
        public int Column { get; set; }
        public int Row { get; set; }
        public int ColumnSpan { get; set; }
        public int RowSpan { get; set; }
    }

    #endregion
}