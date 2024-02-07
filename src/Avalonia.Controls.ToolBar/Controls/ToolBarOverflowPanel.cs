using Avalonia.Controls.ToolBar.Utils;

namespace Avalonia.Controls.ToolBar.Controls;

public class ToolBarOverflowPanel : Panel
{
    // wpf: ToolBarOverflowPanel

    #region Private fields

    private double _wrapWidth; // calculated in MeasureOverride and used in ArrangeOverride
    private Size _panelSize;

    #endregion

    #region WrapWidth

    /// <summary>
    /// Defines the <see cref="WrapWidth"/> Property
    /// </summary>
    public static readonly StyledProperty<double> WrapWidthProperty =
        AvaloniaProperty.Register<ToolBarOverflowPanel, double>(nameof(WrapWidth), Double.NaN,
            validate: IsWrapWidthValid);

    /// <summary>
    /// Get or set wrap width
    /// </summary>
    public double WrapWidth
    {
        get => GetValue(WrapWidthProperty);
        set => SetValue(WrapWidthProperty, value);
    }

    static bool IsWrapWidthValid(double v)
    {
        return double.IsNaN(v) || WpfDoubleUtil.GreaterThanOrClose(v, 0d) && !Double.IsPositiveInfinity(v);
    }

    #endregion

    static ToolBarOverflowPanel()
    {
        AffectsMeasure<ToolBarOverflowPanel>(WrapWidthProperty);
    }

    #region Override methods

    protected override Size MeasureOverride(Size constraint)
    {
        var curLineSize = new Size();
        _panelSize = new Size();
        _wrapWidth = double.IsNaN(WrapWidth) ? constraint.Width : WrapWidth;
        var childrenCount = Children.Count;

        // Add ToolBar items which have IsOverflowItem = true
        var toolBarPanel = ToolBarPanel;
        if (toolBarPanel != null)
        {
            // Go through the generated items collection and add to the children collection
            // any that are marked IsOverFlowItem but aren't already in the children collection.
            //
            // The order of both collections matters.
            //
            // It is assumed that any children that were removed from generated items will have
            // already been removed from the children collection.
            var generatedItemsCollection = toolBarPanel.GeneratedItemsCollection;
            var generatedItemsCount = generatedItemsCollection?.Count ?? 0;
            var childrenIndex = 0;
            for (var i = 0; i < generatedItemsCount; i++)
            {
                var child = generatedItemsCollection![i];
                if (child != null && ToolBar.GetIsOverflowItem(child) && child is not Separator)
                {
                    if (childrenIndex < childrenCount)
                    {
                        if (Children[childrenIndex] != child)
                        {
                            Children.Insert(childrenIndex, child);
                            childrenCount++;
                        }
                    }
                    else
                    {
                        Children.Add(child);
                        childrenCount++;
                    }

                    childrenIndex++;
                }
            }
        }

        // Measure all children to determine if we need to increase desired wrapWidth
        for (var i = 0; i < childrenCount; i++)
        {
            var child = Children[i];

            child.Measure(constraint);

            var childDesiredSize = child.DesiredSize;
            if (WpfDoubleUtil.GreaterThan(childDesiredSize.Width, _wrapWidth))
            {
                _wrapWidth = childDesiredSize.Width;
            }
        }

        // wrapWidth should not be bigger than constraint.Width
        _wrapWidth = Math.Min(_wrapWidth, constraint.Width);

        foreach (var child in Children)
        {
            var sz = child.DesiredSize;

            if (WpfDoubleUtil.GreaterThan(curLineSize.Width + sz.Width, _wrapWidth)) //need to switch to another line
            {
                _panelSize = _panelSize.WithWidth(Math.Max(curLineSize.Width, _panelSize.Width));
                _panelSize = _panelSize.WithHeight(_panelSize.Height + curLineSize.Height);
                curLineSize = sz;

                if (WpfDoubleUtil.GreaterThan(sz.Width,
                        _wrapWidth)) //the element is wider then the constraint - give it a separate line
                {
                    _panelSize = _panelSize.WithWidth(Math.Max(sz.Width, _panelSize.Width));
                    _panelSize = _panelSize.WithHeight(_panelSize.Height + sz.Height);
                    curLineSize = new Size();
                }
            }
            else //continue to accumulate a line
            {
                curLineSize = curLineSize.WithWidth(curLineSize.Width + sz.Width);
                curLineSize = curLineSize.WithHeight(Math.Max(sz.Height, curLineSize.Height));
            }
        }

        //the last line size, if any should be added
        _panelSize = _panelSize.WithWidth(Math.Max(curLineSize.Width, _panelSize.Width));
        _panelSize = _panelSize.WithHeight(_panelSize.Height + curLineSize.Height);

        return _panelSize;
    }

    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        var firstInLine = 0;
        var curLineSize = new Size();
        var accumulatedHeight = 0d;
        _wrapWidth = Math.Min(_wrapWidth, arrangeBounds.Width);

        var children = Children;
        for (var i = 0; i < children.Count; i++)
        {
            Size sz = children[i].DesiredSize;

            if (WpfDoubleUtil.GreaterThan(curLineSize.Width + sz.Width, _wrapWidth)) //need to switch to another line
            {
                // Arrange the items in the current line not including the current
                ArrangeLine(accumulatedHeight, curLineSize.Height, firstInLine, i);
                accumulatedHeight += curLineSize.Height;

                // Current item will be first on the next line
                firstInLine = i;
                curLineSize = sz;
            }
            else //continue to accumulate a line
            {
                curLineSize = curLineSize.WithWidth(curLineSize.Width + sz.Width);
                curLineSize = curLineSize.WithHeight(Math.Max(sz.Height, curLineSize.Height));
            }
        }

        ArrangeLine(accumulatedHeight, curLineSize.Height, firstInLine, children.Count);

        return _panelSize;
    }

    private void ArrangeLine(double y, double lineHeight, int start, int end)
    {
        double x = 0;
        var children = this.Children;
        for (var i = start; i < end; i++)
        {
            var child = children[i];
            child.Arrange(new Rect(x, y, child.DesiredSize.Width, lineHeight));
            x += child.DesiredSize.Width;
        }
    }

    #endregion

    #region Private implementation

    private ToolBar? ToolBar => TemplatedParent as ToolBar;

    private ToolBarPanel? ToolBarPanel => ToolBar?.ToolBarPanel;

    #endregion
}