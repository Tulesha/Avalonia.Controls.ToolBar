using System.Collections.Specialized;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Avalonia.Controls.ToolBar.Controls;

/// <summary>
/// Defines how we place the toolbar items
/// </summary>
public enum OverflowMode
{
    /// <summary>
    /// specifies that the item moves between the main and the overflow panels as space permits
    /// </summary>
    AsNeeded,

    /// <summary>
    /// specifies that the item is permanently placed in the overflow panel
    /// </summary>
    Always,

    /// <summary>
    /// specifies that the item is never allowed to overflow
    /// </summary>
    Never

    // NOTE: if you add or remove any values in this enum, be sure to update ToolBar.IsValidOverflowMode()
}

public class ToolBar : HeaderedItemsControl
{
    protected override Type StyleKeyOverride => typeof(ToolBar);

    #region Private fields

    private const string ElementToolBarOverflowPanel = "PART_ToolBarOverflowPanel";
    private const string ElementToolBarPanel = "PART_ToolBarPanel";
    private const string ElementToolBarPopup = "PART_OverflowPopup";

    private Popup? _popup;
    private ToolBarPanel? _toolBarPanel;
    private ToolBarOverflowPanel? _toolBarOverflowPanel;

    private double m_minLength = 0d;
    private double m_maxLength = 0d;

    #endregion

    #region Properties

    #region Orientation

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<ToolBar, Orientation>(nameof(Orientation), inherits: true,
            coerce: CoerceOrientation);

    static Orientation CoerceOrientation(AvaloniaObject obj, Orientation value)
    {
        var toolBarTray = ((ToolBar)obj).ToolBarTray;
        return toolBarTray != null ? toolBarTray.Orientation : value;
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    #endregion

    #region Band

    public static readonly StyledProperty<int> BandProperty = AvaloniaProperty.Register<ToolBar, int>(nameof(Band));

    public int Band
    {
        get => GetValue(BandProperty);
        set => SetValue(BandProperty, value);
    }

    #endregion

    #region BandIndex

    public static readonly StyledProperty<int> BandIndexProperty =
        AvaloniaProperty.Register<ToolBar, int>(nameof(BandIndex));

    public int BandIndex
    {
        get => GetValue(BandIndexProperty);
        set => SetValue(BandIndexProperty, value);
    }

    #endregion

    #region IsOverflowOpen

    public static readonly StyledProperty<bool> IsOverflowOpenProperty =
        AvaloniaProperty.Register<ToolBar, bool>(nameof(IsOverflowOpen), defaultBindingMode: BindingMode.TwoWay,
            coerce: CoerceIsOverflowOpen);

    public bool IsOverflowOpen
    {
        get => GetValue(IsOverflowOpenProperty);
        set => SetValue(IsOverflowOpenProperty, value);
    }

    private static bool CoerceIsOverflowOpen(AvaloniaObject obj, bool value)
    {
        if (value)
        {
            var tb = (ToolBar)obj;
            if (!tb.IsLoaded)
            {
                tb.RegisterToOpenOnLoad();
                return false;
            }
        }

        return value;
    }

    private void RegisterToOpenOnLoad()
    {
        Loaded += OpenOnLoad;
    }

    private void OpenOnLoad(object? sender, RoutedEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() => { CoerceValue(IsOverflowOpenProperty); }, DispatcherPriority.Input);
    }

    #endregion

    #region HasOverflowItems

    public static readonly StyledProperty<bool> HasOverflowItemsProperty =
        AvaloniaProperty.Register<ToolBar, bool>(nameof(HasOverflowItems));

    public bool HasOverflowItems
    {
        get => GetValue(HasOverflowItemsProperty);
        set => SetValue(HasOverflowItemsProperty, value);
    }

    #endregion

    #region IsOverflowItem

    public static readonly StyledProperty<bool> IsOverflowItemProperty =
        AvaloniaProperty.Register<ToolBar, bool>(nameof(IsOverflowItem), false, inherits: true);

    public bool IsOverflowItem
    {
        get => GetValue(IsOverflowItemProperty);
        set => SetValue(IsOverflowItemProperty, value);
    }

    public static void SetIsOverflowItem(Control control, bool value)
    {
        control.SetValue(IsOverflowItemProperty, value);
    }

    public static bool GetIsOverflowItem(Control control)
    {
        ArgumentNullException.ThrowIfNull(control);
        return control.GetValue(IsOverflowItemProperty);
    }

    #endregion

    #region OverflowMode

    public static readonly StyledProperty<OverflowMode> OverflowModeProperty =
        AvaloniaProperty.Register<ToolBar, OverflowMode>(nameof(OverflowMode), OverflowMode.AsNeeded,
            validate: IsValidOverflowMode);

    public OverflowMode OverflowMode
    {
        get => GetValue(OverflowModeProperty);
        set => SetValue(OverflowModeProperty, value);
    }

    private static void OnOverflowModeChanged(ToolBar toolBar, AvaloniaPropertyChangedEventArgs e)
    {
        // When OverflowMode changes on a child container of a ToolBar,
        // invalidate layout so that the child can be placed in the correct
        // location (in the main bar or the overflow menu).
        if (toolBar != null)
        {
            toolBar.InvalidateLayout();
        }
    }

    private void InvalidateLayout()
    {
        // Reset the calculated min and max size
        m_minLength = 0.0;
        m_maxLength = 0.0;

        // Min and max sizes are calculated in ToolBar.MeasureOverride
        InvalidateMeasure();

        var toolBarPanel = ToolBarPanel;
        if (toolBarPanel != null)
        {
            // Whether elements are in the overflow or not is decided
            // in ToolBarPanel.MeasureOverride.
            toolBarPanel.InvalidateMeasure();
        }
    }

    private static bool IsValidOverflowMode(OverflowMode value)
    {
        return value == OverflowMode.AsNeeded
               || value == OverflowMode.Always
               || value == OverflowMode.Never;
    }

    public static void SetOverflowMode(Control control, OverflowMode mode)
    {
        control.SetValue(OverflowModeProperty, mode);
    }

    public static OverflowMode GetOverflowMode(Control control)
    {
        return control.GetValue(OverflowModeProperty);
    }

    #endregion

    #endregion

    static ToolBar()
    {
        OverflowModeProperty.Changed.AddClassHandler<ToolBar>(OnOverflowModeChanged);

        IsTabStopProperty.OverrideMetadata<ToolBar>(new StyledPropertyMetadata<bool>(false));
        FocusableProperty.OverrideDefaultValue<ToolBar>(true);

        KeyboardNavigation.TabNavigationProperty.OverrideMetadata<ToolBar>(
            new StyledPropertyMetadata<KeyboardNavigationMode>(KeyboardNavigationMode.Cycle));

        Button.ClickEvent.AddClassHandler<ToolBar>(OnClick);
    }

    public ToolBar()
    {
        Items.CollectionChanged += OnItemsChanged;
    }

    internal void AddLogicalChild(Control c)
    {
        if (!LogicalChildren.Contains(c))
            LogicalChildren.Add(c);
    }

    internal void RemoveLogicalChild(Control c) => LogicalChildren.Remove(c);

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // When items change, invalidate layout so that the decision
        // regarding which items are in the overflow menu can be re-done.
        InvalidateLayout();
    }

    #region Override methods

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == BandProperty || change.Property == BandIndexProperty)
        {
            if (Parent is not Layoutable visualParent)
                return;

            visualParent.InvalidateMeasure();
        }

        base.OnPropertyChanged(change);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _toolBarPanel = e.NameScope.Find<ToolBarPanel>(ElementToolBarPanel);
        ArgumentNullException.ThrowIfNull(_toolBarPanel);

        _toolBarOverflowPanel = e.NameScope.Find<ToolBarOverflowPanel>(ElementToolBarOverflowPanel);
        ArgumentNullException.ThrowIfNull(_toolBarOverflowPanel);

        _popup = e.NameScope.Find<Popup>(ElementToolBarPopup);
        ArgumentNullException.ThrowIfNull(_popup);
    }

    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        // Invalidate template references
        _toolBarPanel = null;
        _toolBarOverflowPanel = null;

        base.OnTemplateChanged(e);
    }

    protected override Size MeasureOverride(Size constraint)
    {
        // Perform a normal layout
        var desiredSize = base.MeasureOverride(constraint);

        // MinLength and MaxLength are used by ToolBarTray to determine
        // its layout. ToolBarPanel will calculate its version of these values.
        // ToolBar needs to add on the space used up by elements around the ToolBarPanel.
        //
        // Note: This calculation is not 100% accurate. If a scale transform is applied
        // within the template of the ToolBar (between the ToolBar and the ToolBarPanel),
        // then the coordinate spaces will not match and the values will be wrong.
        //
        // Note: If a ToolBarPanel is not contained within the ToolBar's template,
        // then these values will always be zero, and ToolBarTray will not layout correctly.
        //
        var toolBarPanel = ToolBarPanel;
        if (toolBarPanel != null)
        {
            // Calculate the extra length from the extra space allocated between the ToolBar and the ToolBarPanel.
            double extraLength;
            Thickness margin = toolBarPanel.Margin;
            if (toolBarPanel.Orientation == Orientation.Horizontal)
            {
                extraLength = Math.Max(0.0,
                    desiredSize.Width - toolBarPanel.DesiredSize.Width + margin.Left + margin.Right);
            }
            else
            {
                extraLength = Math.Max(0.0,
                    desiredSize.Height - toolBarPanel.DesiredSize.Height + margin.Top + margin.Bottom);
            }

            // Add the calculated extra length to the lengths provided by ToolBarPanel
            m_minLength = toolBarPanel.MinLength + extraLength;
            m_maxLength = toolBarPanel.MaxLength + extraLength;
        }

        return desiredSize;
    }

    #endregion

    #region Capture Handelrs

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e is { Handled: false, Source: Visual source })
        {
            if (_popup != null && _popup.IsInsidePopup(source))
            {
                e.Handled = true;
            }
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (e is { Handled: false, Source: Visual source })
        {
            // Need for not to close SelectingItemsControl Popup
            var selectingItemsControl = source.FindAncestorOfType<SelectingItemsControl>();
            if (_popup != null && _popup.IsInsidePopup(source) && selectingItemsControl == null)
            {
                _popup.Close();
                e.Handled = true;
            }
        }

        base.OnPointerReleased(e);
    }

    private static void OnClick(ToolBar toolBar, RoutedEventArgs e)
    {
        if (toolBar.IsOverflowOpen && e.Source is Button b && Equals(b.GetLogicalParent(), toolBar))
            toolBar.Close();
    }

    #endregion

    #region Private implementation

    /// <summary>
    /// Gets reference to ToolBar's ToolBarPanel element.
    /// </summary>
    internal ToolBarPanel? ToolBarPanel
    {
        get => _toolBarPanel;
    }

    /// <summary>
    /// Gets reference to ToolBar's ToolBarOverflowPanel element.
    /// </summary>
    internal ToolBarOverflowPanel? ToolBarOverflowPanel
    {
        get => _toolBarOverflowPanel;
    }

    private ToolBarTray? ToolBarTray
    {
        get => Parent as ToolBarTray;
    }

    internal double MinLength
    {
        get => m_minLength;
    }

    internal double MaxLength
    {
        get => m_maxLength;
    }

    private void Close()
    {
        SetCurrentValue(IsOverflowOpenProperty, false);
    }

    #endregion
}