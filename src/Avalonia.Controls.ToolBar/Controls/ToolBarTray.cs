using System.Collections.ObjectModel;
using Avalonia.Controls.ToolBar.Utils;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace Avalonia.Controls.ToolBar.Controls;

public class ToolBarTray : Control, IAddChild
{
    protected override Type StyleKeyOverride => typeof(ToolBarTray);

    #region Private fields

    // ToolBarTray generates list of bands depend on ToolBar.Band property.
    // Each band is a list of toolbars sorted by ToolBar.BandIndex property.
    private readonly List<BandInfo> _bands = new(0);
    private bool _bandsDirty = true;
    private ToolBarCollection? _toolBarsCollection;

    #endregion

    #region Properties

    #region Background

    /// <summary>
    /// Defines the <see cref="Background"/> Property
    /// </summary>
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        Border.BackgroundProperty.AddOwner<ToolBarTray>();

    /// <summary>
    /// Get or set background
    /// </summary>
    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    #endregion

    #region Orientation

    /// <summary>
    /// Defines the <see cref="Orientation"/> Property
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackPanel.OrientationProperty.AddOwner<ToolBarTray>(
            new StyledPropertyMetadata<Orientation>(Orientation.Horizontal));

    /// <summary>
    /// Get or set the orientation
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    private static void OnOrientationChanged(ToolBarTray toolBar, AvaloniaPropertyChangedEventArgs e)
    {
        var toolbarCollection = toolBar.ToolBars;
        foreach (var t in toolbarCollection)
        {
            t.CoerceValue(ToolBar.OrientationProperty);
        }
    }

    #endregion

    #region IsLocked

    /// <summary>
    /// Defines the <see cref="IsLocked"/> Property
    /// </summary>
    public static readonly StyledProperty<bool> IsLockedProperty =
        AvaloniaProperty.Register<ToolBarTray, bool>(nameof(IsLocked), inherits: true);

    /// <summary>
    /// Get or set lock
    /// </summary>
    public bool IsLocked
    {
        get => GetValue(IsLockedProperty);
        set => SetValue(IsLockedProperty, value);
    }

    #endregion

    #region ToolBars

    /// <summary>
    /// Collection of ToolBar
    /// </summary>
    [Content]
    public Collection<ToolBar> ToolBars
    {
        get => _toolBarsCollection ??= new ToolBarCollection(this);
    }

    private class ToolBarCollection : Collection<ToolBar>
    {
        private readonly ToolBarTray _parent;

        public ToolBarCollection(ToolBarTray parent)
        {
            _parent = parent;
        }

        protected override void InsertItem(int index, ToolBar toolBar)
        {
            base.InsertItem(index, toolBar);

            _parent.LogicalChildren.Add(toolBar);
            _parent.VisualChildren.Add(toolBar);
            _parent.InvalidateMeasure();
        }

        protected override void SetItem(int index, ToolBar toolBar)
        {
            var currentToolBar = Items[index];
            if (toolBar != currentToolBar)
            {
                base.SetItem(index, toolBar);

                // remove old item visual and logical links
                _parent.VisualChildren.Remove(currentToolBar);
                _parent.LogicalChildren.Remove(currentToolBar);

                // add new item visual and logical links
                _parent.LogicalChildren.Add(toolBar);
                _parent.VisualChildren.Add(toolBar);

                _parent.InvalidateMeasure();
            }
        }

        protected override void RemoveItem(int index)
        {
            var currentToolBar = this[index];
            base.RemoveItem(index);

            _parent.VisualChildren.Remove(currentToolBar);
            _parent.LogicalChildren.Remove(currentToolBar);
            _parent.InvalidateMeasure();
        }

        protected override void ClearItems()
        {
            var count = Count;
            if (count > 0)
            {
                for (var i = 0; i < count; i++)
                {
                    var currentToolBar = this[i];
                    _parent.VisualChildren.Remove(currentToolBar);
                    _parent.LogicalChildren.Remove(currentToolBar);
                }

                _parent.InvalidateMeasure();
            }

            base.ClearItems();
        }
    }

    #endregion

    #endregion

    static ToolBarTray()
    {
        ThumbEx.DragDeltaEvent.AddClassHandler<ToolBarTray>(OnThumbDragDelta);
        OrientationProperty.Changed.AddClassHandler<ToolBarTray>(OnOrientationChanged);

        AffectsRender<ToolBarTray>(BackgroundProperty);
        AffectsMeasure<ToolBarTray>(IsLockedProperty);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == OrientationProperty)
        {
            if (Parent is Layoutable control)
                control.InvalidateMeasure();
        }

        base.OnPropertyChanged(change);
    }

    #region Public methods

    void IAddChild.AddChild(object value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value is not ToolBar toolBar)
        {
            throw new ArgumentException("Null arugment");
        }

        ToolBars.Add(toolBar);
    }

    #endregion

    #region Override Methods

    public override void Render(DrawingContext context)
    {
        var background = Background;
        if (background != null)
        {
            context.DrawRectangle(background, null, new Rect(0, 0, Bounds.Width, Bounds.Height));
        }
    }

    protected override Size MeasureOverride(Size constraint)
    {
        GenerateBands();

        var toolBarTrayDesiredSize = new Size();
        int bandIndex;
        var fHorizontal = (Orientation == Orientation.Horizontal);
        var childConstraint = new Size(Double.PositiveInfinity, Double.PositiveInfinity);

        for (bandIndex = 0; bandIndex < _bands.Count; bandIndex++)
        {
            // Calculate the available size before we measure the children.
            // remainingLength is the constraint minus sum of all minimum sizes
            var remainingLength = fHorizontal ? constraint.Width : constraint.Height;
            var band = _bands[bandIndex].Band;
            var bandThickness = 0d;
            var bandLength = 0d;
            int toolBarIndex;
            for (toolBarIndex = 0; toolBarIndex < band.Count; toolBarIndex++)
            {
                var toolBar = band[toolBarIndex];
                remainingLength -= toolBar.MinLength;
                if (WpfDoubleUtil.LessThan(remainingLength, 0))
                {
                    remainingLength = 0;
                    break;
                }
            }

            // Measure all children passing the remainingLength as a constraint
            for (toolBarIndex = 0; toolBarIndex < band.Count; toolBarIndex++)
            {
                var toolBar = band[toolBarIndex];
                remainingLength += toolBar.MinLength;
                childConstraint = fHorizontal
                    ? childConstraint.WithWidth(remainingLength)
                    : childConstraint.WithHeight(remainingLength);
                toolBar.Measure(childConstraint);
                bandThickness = Math.Max(bandThickness,
                    fHorizontal ? toolBar.DesiredSize.Height : toolBar.DesiredSize.Width);
                bandLength += fHorizontal ? toolBar.DesiredSize.Width : toolBar.DesiredSize.Height;
                remainingLength -= fHorizontal ? toolBar.DesiredSize.Width : toolBar.DesiredSize.Height;
                if (WpfDoubleUtil.LessThan(remainingLength, 0))
                {
                    remainingLength = 0;
                }
            }

            // Store band thickness in the BandInfo property
            _bands[bandIndex].Thickness = bandThickness;

            if (fHorizontal)
            {
                toolBarTrayDesiredSize =
                    toolBarTrayDesiredSize.WithHeight(toolBarTrayDesiredSize.Height + bandThickness);
                toolBarTrayDesiredSize =
                    toolBarTrayDesiredSize.WithWidth(Math.Max(toolBarTrayDesiredSize.Width, bandLength));
            }
            else
            {
                toolBarTrayDesiredSize = toolBarTrayDesiredSize.WithWidth(toolBarTrayDesiredSize.Width + bandThickness);
                toolBarTrayDesiredSize =
                    toolBarTrayDesiredSize.WithHeight(Math.Max(toolBarTrayDesiredSize.Height, bandLength));
            }
        }

        return toolBarTrayDesiredSize;
    }

    protected override Size ArrangeOverride(Size arrangeSize)
    {
        int bandIndex;
        var fHorizontal = (Orientation == Orientation.Horizontal);
        var rcChild = new Rect();

        for (bandIndex = 0; bandIndex < _bands.Count; bandIndex++)
        {
            var band = _bands[bandIndex].Band;

            var bandThickness = _bands[bandIndex].Thickness;

            rcChild = fHorizontal ? rcChild.WithX(0) : rcChild.WithY(0);

            int toolBarIndex;
            for (toolBarIndex = 0; toolBarIndex < band.Count; toolBarIndex++)
            {
                var toolBar = band[toolBarIndex];
                var toolBarArrangeSize = new Size(fHorizontal ? toolBar.DesiredSize.Width : bandThickness,
                    fHorizontal ? bandThickness : toolBar.DesiredSize.Height);
                rcChild = new Rect(rcChild.Position, toolBarArrangeSize);
                toolBar.Arrange(rcChild);
                rcChild = fHorizontal
                    ? rcChild.WithX(rcChild.X + toolBarArrangeSize.Width)
                    : rcChild.WithY(rcChild.Y + toolBarArrangeSize.Height);
            }

            rcChild = fHorizontal ? rcChild.WithY(rcChild.Y + bandThickness) : rcChild.WithX(rcChild.X + bandThickness);
        }

        return arrangeSize;
    }

    #endregion

    #region Private Methods

    private static void OnThumbDragDelta(ToolBarTray toolBarTray, VectorEventArgsEx e)
    {
        // Don't move toolbars if IsLocked == true
        if (toolBarTray.IsLocked)
            return;

        toolBarTray.ProcessThumbDragDelta(e);
    }

    private void ProcessThumbDragDelta(VectorEventArgsEx e)
    {
        if (e.Vector.Length == 0)
            return;

        // Process thumb event only if Thumb styled parent is a ToolBar under the TollBarTray
        if (e.Source is ThumbEx thumb)
        {
            if (thumb.TemplatedParent is ToolBar toolBar && toolBar.Parent == this)
            {
                // _bandsDirty would be true at this time only when a Measure gets
                // skipped between two mouse moves. Ideally that should not happen
                // but VS has proved that it can. Hence making the code more robust.
                // Uncomment the line below if the measure skip issue ever gets fixed.
                // Debug.Assert(!_bandsDirty, "Bands should not be dirty at this point");
                if (_bandsDirty)
                {
                    GenerateBands();
                }

                var fHorizontal = (Orientation == Orientation.Horizontal);
                var currentBand = toolBar.Band;

                if (e.PointerEventArgs != null)
                {
                    var pointRelativeToToolBarTray = e.PointerEventArgs.GetPosition(this);
                    var pointRelativeToToolBar = TransformPointToToolBar(toolBar, pointRelativeToToolBarTray);
                    var hittestBand =
                        GetBandFromOffset(fHorizontal ? pointRelativeToToolBarTray.Y : pointRelativeToToolBarTray.X);
                    var thumbChange = fHorizontal ? e.Vector.X : e.Vector.Y;
                    double toolBarPosition;
                    if (fHorizontal)
                    {
                        toolBarPosition = pointRelativeToToolBarTray.X - pointRelativeToToolBar.X;
                    }
                    else
                    {
                        toolBarPosition = pointRelativeToToolBarTray.Y - pointRelativeToToolBar.Y;
                    }

                    var newPosition = toolBarPosition + thumbChange; // New toolBar position

                    // Move within the band
                    if (hittestBand == currentBand)
                    {
                        var band = _bands[currentBand].Band;
                        var toolBarIndex = toolBar.BandIndex;

                        // Move ToolBar within the band
                        if (WpfDoubleUtil.LessThan(thumbChange, 0)) // Move left/up
                        {
                            var toolBarsTotalMinimum = ToolBarsTotalMinimum(band, 0, toolBarIndex - 1);
                            // Check if minimized toolbars will fit in the range
                            if (WpfDoubleUtil.LessThanOrClose(toolBarsTotalMinimum, newPosition))
                            {
                                ShrinkToolBars(band, 0, toolBarIndex - 1, -thumbChange);
                            }
                            else if (toolBarIndex > 0) // Swap toolbars
                            {
                                var prevToolBar = band[toolBarIndex - 1];
                                var pointRelativeToPreviousToolBar =
                                    TransformPointToToolBar(prevToolBar, pointRelativeToToolBarTray);
                                // if pointer in on the left side of previous toolbar
                                if (WpfDoubleUtil.LessThan(
                                        (fHorizontal
                                            ? pointRelativeToPreviousToolBar.X
                                            : pointRelativeToPreviousToolBar.Y),
                                        0))
                                {
                                    prevToolBar.BandIndex = toolBarIndex;
                                    band[toolBarIndex] = prevToolBar;

                                    toolBar.BandIndex = toolBarIndex - 1;
                                    band[toolBarIndex - 1] = toolBar;

                                    if (toolBarIndex + 1 == band.Count) // If toolBar was the last item in the band
                                    {
                                        prevToolBar.ClearValue(fHorizontal ? WidthProperty : HeightProperty);
                                    }
                                }
                                else
                                {
                                    // Move to the left/up and shring the other toolbars
                                    if (fHorizontal)
                                    {
                                        if (WpfDoubleUtil.LessThan(toolBarsTotalMinimum,
                                                pointRelativeToToolBarTray.X - pointRelativeToToolBar.X))
                                        {
                                            ShrinkToolBars(band, 0, toolBarIndex - 1,
                                                pointRelativeToToolBarTray.X - pointRelativeToToolBar.X -
                                                toolBarsTotalMinimum);
                                        }
                                    }
                                    else
                                    {
                                        if (WpfDoubleUtil.LessThan(toolBarsTotalMinimum,
                                                pointRelativeToToolBarTray.Y - pointRelativeToToolBar.Y))
                                        {
                                            ShrinkToolBars(band, 0, toolBarIndex - 1,
                                                pointRelativeToToolBarTray.Y - pointRelativeToToolBar.Y -
                                                toolBarsTotalMinimum);
                                        }
                                    }
                                }
                            }
                        }
                        else // Move right/down
                        {
                            var toolBarsTotalMaximum = ToolBarsTotalMaximum(band, 0, toolBarIndex - 1);

                            if (WpfDoubleUtil.GreaterThan(toolBarsTotalMaximum, newPosition))
                            {
                                ExpandToolBars(band, 0, toolBarIndex - 1, thumbChange);
                            }
                            else
                            {
                                if (toolBarIndex < band.Count - 1) // Swap toolbars
                                {
                                    var nextToolBar = band[toolBarIndex + 1];
                                    var pointRelativeToNextToolBar =
                                        TransformPointToToolBar(nextToolBar, pointRelativeToToolBarTray);
                                    // if pointer in on the right side of next toolbar
                                    if (WpfDoubleUtil.GreaterThanOrClose(
                                            (fHorizontal ? pointRelativeToNextToolBar.X : pointRelativeToNextToolBar.Y),
                                            0))
                                    {
                                        nextToolBar.BandIndex = toolBarIndex;
                                        band[toolBarIndex] = nextToolBar;

                                        toolBar.BandIndex = toolBarIndex + 1;
                                        band[toolBarIndex + 1] = toolBar;
                                        if (toolBarIndex + 2 ==
                                            band.Count) // If toolBar becomes the last item in the band
                                        {
                                            toolBar.ClearValue(fHorizontal ? WidthProperty : HeightProperty);
                                        }
                                    }
                                    else
                                    {
                                        ExpandToolBars(band, 0, toolBarIndex - 1, thumbChange);
                                    }
                                }
                                else
                                {
                                    ExpandToolBars(band, 0, toolBarIndex - 1, thumbChange);
                                }
                            }
                        }
                    }
                    else // Move ToolBar to another band
                    {
                        _bandsDirty = true;
                        toolBar.Band = hittestBand;
                        toolBar.ClearValue(fHorizontal ? WidthProperty : HeightProperty);

                        // move to another existing band
                        if (hittestBand >= 0 && hittestBand < _bands.Count)
                        {
                            MoveToolBar(toolBar, hittestBand, newPosition);
                        }

                        var oldBand = _bands[currentBand].Band;
                        // currentBand should restore sizes to Auto
                        foreach (var currentToolBar in oldBand)
                        {
                            currentToolBar.ClearValue(fHorizontal ? WidthProperty : HeightProperty);
                        }
                    }
                }

                e.Handled = true;
            }
        }
    }

    private Point TransformPointToToolBar(ToolBar toolBar, Point point)
    {
        var p = point;
        var matrix = this.TransformToVisual(toolBar);
        if (matrix.HasValue)
        {
            var transform = new MatrixTransform(matrix.Value);
            p = transform.Value.Transform(p);
            // var bounds = new Rect( toolBar.Bounds.Size ).TransformToAABB( transform.Value );
            // p = bounds.Position;
        }

        return p;
    }

    private void ShrinkToolBars(List<ToolBar> band, int startIndex, int endIndex, double shrinkAmount)
    {
        if (Orientation == Orientation.Horizontal)
        {
            for (var i = endIndex; i >= startIndex; i--)
            {
                var toolBar = band[i];
                if (WpfDoubleUtil.GreaterThanOrClose(toolBar.Bounds.Width - shrinkAmount, toolBar.MinLength))
                {
                    toolBar.Width = toolBar.Bounds.Width - shrinkAmount;
                    break;
                }
                else
                {
                    toolBar.Width = toolBar.MinLength;
                    shrinkAmount -= toolBar.Bounds.Width - toolBar.MinLength;
                }
            }
        }
        else
        {
            for (var i = endIndex; i >= startIndex; i--)
            {
                var toolBar = band[i];
                if (WpfDoubleUtil.GreaterThanOrClose(toolBar.Bounds.Height - shrinkAmount, toolBar.MinLength))
                {
                    toolBar.Height = toolBar.Bounds.Height - shrinkAmount;
                    break;
                }
                else
                {
                    toolBar.Height = toolBar.MinLength;
                    shrinkAmount -= toolBar.Bounds.Height - toolBar.MinLength;
                }
            }
        }
    }

    private double ToolBarsTotalMinimum(List<ToolBar> band, int startIndex, int endIndex)
    {
        var totalMinLenght = 0d;
        for (var i = startIndex; i <= endIndex; i++)
        {
            totalMinLenght += band[i].MinLength;
        }

        return totalMinLenght;
    }

    private void ExpandToolBars(List<ToolBar> band, int startIndex, int endIndex, double expandAmount)
    {
        if (Orientation == Orientation.Horizontal)
        {
            for (var i = endIndex; i >= startIndex; i--)
            {
                var toolBar = band[i];
                if (WpfDoubleUtil.LessThanOrClose(toolBar.Bounds.Width + expandAmount, toolBar.MaxLength))
                {
                    toolBar.Width = toolBar.Bounds.Width + expandAmount;
                    break;
                }
                else
                {
                    toolBar.Width = toolBar.MaxLength;
                    expandAmount -= toolBar.MaxLength - toolBar.Bounds.Width;
                }
            }
        }
        else
        {
            for (var i = endIndex; i >= startIndex; i--)
            {
                var toolBar = band[i];
                if (WpfDoubleUtil.LessThanOrClose(toolBar.Bounds.Height + expandAmount, toolBar.MaxLength))
                {
                    toolBar.Height = toolBar.Bounds.Height + expandAmount;
                    break;
                }
                else
                {
                    toolBar.Height = toolBar.MaxLength;
                    expandAmount -= toolBar.MaxLength - toolBar.Bounds.Height;
                }
            }
        }
    }

    private double ToolBarsTotalMaximum(List<ToolBar> band, int startIndex, int endIndex)
    {
        var totalMaxLength = 0d;
        for (var i = startIndex; i <= endIndex; i++)
        {
            totalMaxLength += band[i].MaxLength;
        }

        return totalMaxLength;
    }

    private void MoveToolBar(ToolBar toolBar, int newBandNumber, double position)
    {
        var fHorizontal = Orientation == Orientation.Horizontal;

        var newBand = _bands[newBandNumber].Band;
        // calculate the new BandIndex where toolBar should insert
        // calculate Width (layout) of the items before the toolBar
        if (WpfDoubleUtil.LessThanOrClose(position, 0))
        {
            toolBar.BandIndex = -1; // This will position toolBar at the first place
        }
        else
        {
            var toolBarOffset = 0d;
            var newToolBarIndex = -1;
            int i;
            for (i = 0; i < newBand.Count; i++)
            {
                var currentToolBar = newBand[i];
                if (newToolBarIndex == -1)
                {
                    toolBarOffset +=
                        fHorizontal
                            ? currentToolBar.Bounds.Width
                            : currentToolBar.Bounds.Height; // points at the end of currentToolBar
                    if (WpfDoubleUtil.GreaterThan(toolBarOffset, position))
                    {
                        newToolBarIndex = i + 1;
                        toolBar.BandIndex = newToolBarIndex;
                        // Update the currentToolBar width
                        if (fHorizontal)
                            currentToolBar.Width = Math.Max(currentToolBar.MinLength,
                                currentToolBar.Bounds.Width - toolBarOffset + position);
                        else
                            currentToolBar.Height = Math.Max(currentToolBar.MinLength,
                                currentToolBar.Bounds.Height - toolBarOffset + position);
                    }
                }
                else // After we insert the toolBar we need to increase the indexes
                {
                    currentToolBar.BandIndex = i + 1;
                }
            }

            if (newToolBarIndex == -1)
            {
                toolBar.BandIndex = i;
            }
        }
    }

    private int GetBandFromOffset(double toolBarOffset)
    {
        if (WpfDoubleUtil.LessThan(toolBarOffset, 0))
            return -1;

        var bandOffset = 0d;
        for (var i = 0; i < _bands.Count; i++)
        {
            bandOffset += _bands[i].Thickness;
            if (WpfDoubleUtil.GreaterThan(bandOffset, toolBarOffset))
                return i;
        }

        return _bands.Count;
    }

    #region Generate and Normalize bands

    // Generate all bands and normalize Band and BandIndex properties
    /// All ToolBars with the same Band are places in one band. After that they are sorted by BandIndex.
    private void GenerateBands()
    {
        if (!IsBandsDirty())
            return;

        var toolbarCollection = ToolBars;

        _bands.Clear();
        for (var i = 0; i < toolbarCollection.Count; i++)
        {
            InsertBand(toolbarCollection[i], i);
        }

        // Normalize bands (make Band and BandIndex property 0,1,2,...)
        for (var bandIndex = 0; bandIndex < _bands.Count; bandIndex++)
        {
            var band = _bands[bandIndex].Band;
            for (var toolBarIndex = 0; toolBarIndex < band.Count; toolBarIndex++)
            {
                var toolBar = band[toolBarIndex];
                // This will cause measure/arrange if some property changes
                toolBar.Band = bandIndex;
                toolBar.BandIndex = toolBarIndex;
            }
        }

        _bandsDirty = false;
    }

    // Verify is all toolbars are normalized (sorted in _bands by Band and BandIndex properties)
    private bool IsBandsDirty()
    {
        if (_bandsDirty)
            return true;

        var totalNumber = 0;
        var toolbarCollection = ToolBars;
        for (var bandIndex = 0; bandIndex < _bands.Count; bandIndex++)
        {
            var band = _bands[bandIndex].Band;
            for (var toolBarIndex = 0; toolBarIndex < band.Count; toolBarIndex++)
            {
                var toolBar = band[toolBarIndex];
                if (toolBar.Band != bandIndex || toolBar.BandIndex != toolBarIndex ||
                    !toolbarCollection.Contains(toolBar))
                    return true;
            }

            totalNumber += band.Count;
        }

        return totalNumber != toolbarCollection.Count;
    }

    // if toolBar.Band does not exist in bands collection when we create a new band
    private void InsertBand(ToolBar toolBar, int toolBarIndex)
    {
        var bandNumber = toolBar.Band;
        for (var i = 0; i < _bands.Count; i++)
        {
            var currentBandNumber = ((_bands[i].Band)[0]).Band;
            if (bandNumber == currentBandNumber)
                return;
            if (bandNumber < currentBandNumber)
            {
                // Band number does not exist - Insert
                _bands.Insert(i, CreateBand(toolBarIndex));
                return;
            }
        }

        // Band number does not exist - Add band at trhe end
        _bands.Add(CreateBand(toolBarIndex));
    }

    private BandInfo CreateBand(int startIndex)
    {
        var toolbarCollection = ToolBars;
        var bandInfo = new BandInfo();
        var toolBar = toolbarCollection[startIndex];
        bandInfo.Band.Add(toolBar);
        var bandNumber = toolBar.Band;
        for (var i = startIndex + 1; i < toolbarCollection.Count; i++)
        {
            toolBar = toolbarCollection[i];
            if (bandNumber == toolBar.Band)
                InsertToolBar(toolBar, bandInfo.Band);
        }

        return bandInfo;
    }

    private void InsertToolBar(ToolBar toolBar, List<ToolBar> band)
    {
        for (var i = 0; i < band.Count; i++)
        {
            if (toolBar.BandIndex < band[i].BandIndex)
            {
                band.Insert(i, toolBar);
                return;
            }
        }

        band.Add(toolBar);
    }

    #endregion

    #endregion

    #region Private classes

    private class BandInfo
    {
        private List<ToolBar> _band = new();
        private double _thickness;

        public List<ToolBar> Band => _band;

        public double Thickness
        {
            get => _thickness;
            set => _thickness = value;
        }
    }

    #endregion
}