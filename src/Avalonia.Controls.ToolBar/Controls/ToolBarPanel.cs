using System.Collections;
using System.Collections.Specialized;
using Avalonia.Controls.ToolBar.Utils;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace Avalonia.Controls.ToolBar.Controls;

/// <summary>
/// ToolBarPanel is used to arrange children within the ToolBar. It is used in the ToolBar style as items host.
/// </summary>
public class ToolBarPanel : StackPanel, IDisposable
{
    private Avalonia.Controls.Controls? _generatedItemsCollection;
    private ToolBar? _toolBar;

    #region ItemIsOwnContainerProperty

    private static readonly AttachedProperty<bool> ItemIsOwnContainerProperty =
        AvaloniaProperty.RegisterAttached<ToolBar, Control, bool>("ItemIsOwnContainer");

    #endregion

    public void Dispose()
    {
        if (_toolBar is not null)
        {
            _toolBar.Items.CollectionChanged -= ToolBarItems_OnCollectionChanged;
            ClearItemsControlLogicalChildren();
        }

        _generatedItemsCollection?.Clear();
        Children.Clear();
        ToolBarOverflowPanel?.Children.Clear();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (TemplatedParent is ToolBar toolBar)
        {
            _toolBar = toolBar;
            _toolBar.Items.CollectionChanged += ToolBarItems_OnCollectionChanged;

            this[!OrientationProperty] = new Binding(nameof(Controls.ToolBar.Orientation))
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent)
            };

            if (_generatedItemsCollection == null)
                _generatedItemsCollection = new();
            else
                _generatedItemsCollection.Clear();

            var overflowPanel = ToolBarOverflowPanel;
            if (overflowPanel != null)
            {
                overflowPanel.Children.Clear();
                overflowPanel.InvalidateMeasure();
            }

            Add(0, _toolBar.Items);
            foreach (var child in _generatedItemsCollection)
            {
                ToolBar.SetIsOverflowItem(child, false);
            }
        }
    }

    #region Layout

    internal double MinLength { get; private set; }

    internal double MaxLength { get; private set; }

    private bool MeasureGeneratedItems(bool asNeededPass, Size constraint, bool horizontal, double maxExtent,
        ref Size panelDesiredSize,
        out double overflowExtent)
    {
        var overflowPanel = ToolBarOverflowPanel;
        var sendToOverflow = false; // Becomes true when the first AsNeeded does not fit
        var hasOverflowItems = false;
        var overflowNeedsInvalidation = false;
        overflowExtent = 0.0;
        var children = Children;
        var childrenCount = children.Count;
        var childrenIndex = 0;

        if (_generatedItemsCollection == null)
            return false;

        foreach (var child in _generatedItemsCollection)
        {
            var overflowMode = ToolBar.GetOverflowMode(child);
            var asNeededMode = overflowMode == OverflowMode.AsNeeded;

            // MeasureGeneratedItems is called twice to do a complete measure.
            // The first pass measures Always and Never items -- items that
            // are guaranteed to be or not to be in the overflow menu.
            // The second pass measures AsNeeded items and determines whether
            // there is enough room for them in the main bar or if they should
            // be placed in the overflow menu.
            // Check here whether the overflow mode matches a mode we should be
            // examining in this pass.
            if (asNeededMode == asNeededPass)
            {
                var visualParent = child.GetVisualParent();

                // In non-Always overflow modes, measure for main bar placement.
                if (overflowMode != OverflowMode.Always && !sendToOverflow)
                {
                    // Children may change their size depending on whether they are in the overflow
                    // menu or not. Ensure that when we measure, we are using the main bar size.
                    // If the item goes to overflow, this property will be updated later in this loop
                    // when it is removed from the visual tree.
                    ToolBar.SetIsOverflowItem(child, false);
                    child.Measure(constraint);
                    var childDesiredSize = child.DesiredSize;

                    // If the child is an AsNeeded, check if it fits. If it doesn't then
                    // this child and all subsequent AsNeeded children should be sent
                    // to the overflow menu.
                    if (asNeededMode)
                    {
                        double newExtent;
                        if (horizontal)
                        {
                            newExtent = childDesiredSize.Width + panelDesiredSize.Width;
                        }
                        else
                        {
                            newExtent = childDesiredSize.Height + panelDesiredSize.Height;
                        }

                        if (WpfDoubleUtil.GreaterThan(newExtent, maxExtent))
                        {
                            // It doesn't fit, send to overflow
                            sendToOverflow = true;
                        }
                    }

                    // The child has been validated as belonging in the main bar.
                    // Update the panel desired size dimensions, and ensure the child
                    // is in the main bar's visual tree.
                    if (!sendToOverflow)
                    {
                        if (horizontal)
                        {
                            panelDesiredSize =
                                panelDesiredSize.WithWidth(panelDesiredSize.Width + childDesiredSize.Width);
                            panelDesiredSize =
                                panelDesiredSize.WithHeight(Math.Max(panelDesiredSize.Height, childDesiredSize.Height));
                        }
                        else
                        {
                            panelDesiredSize =
                                panelDesiredSize.WithWidth(Math.Max(panelDesiredSize.Width, childDesiredSize.Width));
                            panelDesiredSize =
                                panelDesiredSize.WithHeight(panelDesiredSize.Height + childDesiredSize.Height);
                        }

                        if (visualParent != this)
                        {
                            if ((visualParent == overflowPanel) && (overflowPanel != null))
                            {
                                overflowPanel.Children.Remove(child);
                            }

                            if (childrenIndex < childrenCount)
                            {
                                children.Insert(childrenIndex, child);
                            }
                            else
                            {
                                children.Add(child);
                            }

                            childrenCount++;
                        }

                        childrenIndex++;
                    }
                }

                // The child should go to the overflow menu
                if (overflowMode == OverflowMode.Always || sendToOverflow)
                {
                    hasOverflowItems = true;

                    // If a child is in the overflow menu, we don't want to keep measuring.
                    // However, we need to calculate the MaxLength as well as set the desired height
                    // correctly. Thus, we will use the DesiredSize of the child. There is a problem
                    // that can occur if the child changes size while in the overflow menu and
                    // was recently displayed. It will be measure clean, yet its DesiredSize
                    // will not be accurate for the MaxLength calculation.
                    if (child.IsMeasureValid)
                    {
                        // Set this temporarily in case the size is different while in the overflow area
                        ToolBar.SetIsOverflowItem(child, false);
                        child.Measure(constraint);
                    }

                    // Even when in the overflow, we need two pieces of information:
                    // 1. We need to continue to track the maximum size of the non-logical direction
                    //    (i.e. height in horizontal bars). This way, ToolBars with everything in
                    //    the overflow will still have some height.
                    // 2. We want to track how much of the space we saved by placing the child in
                    //    the overflow menu. This is used to calculate MinLength and MaxLength.
                    var childDesiredSize = child.DesiredSize;
                    if (horizontal)
                    {
                        overflowExtent += childDesiredSize.Width;
                        panelDesiredSize =
                            panelDesiredSize.WithHeight(Math.Max(panelDesiredSize.Height, childDesiredSize.Height));
                    }
                    else
                    {
                        overflowExtent += childDesiredSize.Height;
                        panelDesiredSize =
                            panelDesiredSize.WithWidth(Math.Max(panelDesiredSize.Width, childDesiredSize.Width));
                    }

                    // Set the flag to indicate that the child is in the overflow menu
                    ToolBar.SetIsOverflowItem(child, true);

                    // If the child is in this panel's visual tree, remove it.
                    if (visualParent == this)
                    {
                        // children.RemoveNoVerify( child );
                        children.Remove(child);
                        childrenCount--;
                        overflowNeedsInvalidation = true;
                    }
                    // If the child isnt connected to the visual tree, notify the overflow panel to pick it up.
                    else if (visualParent == null)
                    {
                        overflowNeedsInvalidation = true;
                    }
                }
            }
            else
            {
                // We are not measure this child in this pass. Update the index into the
                // visual children collection.
                if (childrenIndex < childrenCount && children[childrenIndex] == child)
                {
                    childrenIndex++;
                }
            }
        }

        // A child was added to the overflow panel, but since we don't add it
        // to the overflow panel's visual collection until that panel's measure
        // pass, we need to mark it as measure dirty.
        if (overflowNeedsInvalidation && overflowPanel != null)
        {
            overflowPanel.InvalidateMeasure();
        }

        return hasOverflowItems;
    }

    protected override Size MeasureOverride(Size constraint)
    {
        var stackDesiredSize = new Size();

        // if( IsItemsHost )
        // {
        var layoutSlotSize = constraint;
        double maxExtent;
        double overflowExtent;
        var horizontal = Orientation == Orientation.Horizontal;

        if (horizontal)
        {
            layoutSlotSize = layoutSlotSize.WithWidth(Double.PositiveInfinity);
            maxExtent = constraint.Width;
        }
        else
        {
            layoutSlotSize = layoutSlotSize.WithHeight(Double.PositiveInfinity);
            maxExtent = constraint.Height;
        }

        // This first call will measure all of the non-AsNeeded elements (i.e. we know
        // whether they're going into the overflow or not.
        // overflowExtent will be the size of the Always elements, which is not actually
        // needed for subsequent calculations.
        var hasAlwaysOverflowItems = MeasureGeneratedItems( /* asNeeded = */ false, layoutSlotSize, horizontal,
            maxExtent,
            ref stackDesiredSize, out overflowExtent);

        // At this point, the desired size is the minimum size of the ToolBar.
        MinLength = horizontal ? stackDesiredSize.Width : stackDesiredSize.Height;

        // This second call will measure all of the AsNeeded elements and place
        // them in the appropriate location.
        var hasAsNeededOverflowItems = MeasureGeneratedItems( /* asNeeded = */ true, layoutSlotSize, horizontal,
            maxExtent,
            ref stackDesiredSize, out overflowExtent);

        // At this point, the desired size is complete. The desired size plus overflowExtent
        // is the maximum size of the ToolBar.
        MaxLength = (horizontal ? stackDesiredSize.Width : stackDesiredSize.Height) + overflowExtent;

        var toolbar = ToolBar;
        if (toolbar != null)
        {
            toolbar.SetValue(ToolBar.HasOverflowItemsProperty, hasAlwaysOverflowItems || hasAsNeededOverflowItems);
        }
        // }
        // else
        // {
        // 	stackDesiredSize = base.MeasureOverride( constraint );
        // }

        return stackDesiredSize;
    }

    protected override Size ArrangeOverride(Size arrangeSize)
    {
        var fHorizontal = Orientation == Orientation.Horizontal;
        var rcChild = new Rect(arrangeSize);
        var previousChildSize = 0.0d;

        //
        // Arrange and Position Children.
        //
        for (int i = 0, count = Children.Count; i < count; ++i)
        {
            var child = Children[i];

            if (fHorizontal)
            {
                rcChild = rcChild.WithX(rcChild.X + previousChildSize);
                previousChildSize = child.DesiredSize.Width;
                rcChild = rcChild.WithWidth(previousChildSize);
                rcChild = rcChild.WithHeight(Math.Max(arrangeSize.Height, child.DesiredSize.Height));
            }
            else
            {
                rcChild = rcChild.WithY(rcChild.Y + previousChildSize);
                previousChildSize = child.DesiredSize.Height;
                rcChild = rcChild.WithHeight(previousChildSize);
                rcChild = rcChild.WithWidth(Math.Max(arrangeSize.Width, child.DesiredSize.Width));
            }

            child.Arrange(rcChild);
        }

        return arrangeSize;
    }

    #endregion

    #region Item Generation

    private void ToolBarItems_OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_toolBar == null || _generatedItemsCollection == null)
            return;

        var generator = _toolBar.ItemContainerGenerator;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                Add(e.NewStartingIndex, e.NewItems!);
                break;
            case NotifyCollectionChangedAction.Remove:
                Remove(e.OldStartingIndex, e.OldItems!.Count);
                break;
            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Move:
                Remove(e.OldStartingIndex, e.OldItems!.Count);
                Add(e.NewStartingIndex, e.NewItems!);
                break;
            case NotifyCollectionChangedAction.Reset:
                ClearItemsControlLogicalChildren();
                _generatedItemsCollection.Clear();
                Add(0, _toolBar.Items);
                break;
        }

        return;

        void Remove(int index, int count)
        {
            if (_generatedItemsCollection == null)
                return;

            for (var i = 0; i < count; i++)
            {
                var control = _generatedItemsCollection[index + i];

                var visualParent = control.GetVisualParent();

                if (visualParent == this)
                {
                    Children.Remove(control);
                }
                else
                {
                    if (visualParent == ToolBarOverflowPanel && ToolBarOverflowPanel != null)
                    {
                        ToolBarOverflowPanel.Children.Remove(control);
                    }
                }

                if (!control.IsSet(ItemIsOwnContainerProperty))
                {
                    _toolBar.RemoveLogicalChild(control);
                    generator.ClearItemContainer(control);
                }
            }

            _generatedItemsCollection.RemoveRange(index, count);

            var childCount = _generatedItemsCollection.Count;

            for (var i = index; i < childCount; ++i)
                generator.ItemContainerIndexChanged(_generatedItemsCollection[i], i + count, i);
        }
    }

    private void Add(int index, IEnumerable items)
    {
        if (_toolBar == null || _generatedItemsCollection == null)
            return;

        var generator = _toolBar.ItemContainerGenerator;

        var i = index;
        foreach (var item in items)
            InsertContainer(item, i++);

        var childCount = _generatedItemsCollection.Count;
        var delta = i - index;

        for (; i < childCount; ++i)
            generator.ItemContainerIndexChanged(_generatedItemsCollection[i], i - delta, i);
    }

    private void InsertContainer(object item, int index)
    {
        if (_toolBar == null || _generatedItemsCollection == null)
            return;

        var generator = _toolBar.ItemContainerGenerator;
        Control container;

        if (generator.NeedsContainer(item, index, out var recycleKey))
        {
            container = generator.CreateContainer(item, index, recycleKey);
        }
        else
        {
            container = (Control)item;
            container.SetValue(ItemIsOwnContainerProperty, true);
        }

        generator.PrepareItemContainer(container, item, index);
        _toolBar.AddLogicalChild(container);
        _generatedItemsCollection.Insert(index, container);
        generator.ItemContainerPrepared(container, item, index);
    }

    private void ClearItemsControlLogicalChildren()
    {
        if (_toolBar == null || _generatedItemsCollection == null)
            return;

        foreach (var child in _generatedItemsCollection)
        {
            if (!child.IsSet(ItemIsOwnContainerProperty))
                _toolBar.RemoveLogicalChild(child);
        }
    }

    #endregion

    #region Helpers

    private ToolBar? ToolBar => TemplatedParent as ToolBar;

    private ToolBarOverflowPanel? ToolBarOverflowPanel => ToolBar?.ToolBarOverflowPanel;

    internal Avalonia.Controls.Controls? GeneratedItemsCollection => _generatedItemsCollection;

    #endregion
}