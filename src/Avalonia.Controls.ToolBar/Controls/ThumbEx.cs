using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Avalonia.Controls.ToolBar.Controls;

[PseudoClasses(":pressed")]
public class ThumbEx : Thumb
{
    private Point? m_lastPoint;

    public new static readonly RoutedEvent<VectorEventArgsEx> DragStartedEvent =
        RoutedEvent.Register<ThumbEx, VectorEventArgsEx>(nameof(DragStarted), RoutingStrategies.Bubble);

    public new static readonly RoutedEvent<VectorEventArgsEx> DragDeltaEvent =
        RoutedEvent.Register<ThumbEx, VectorEventArgsEx>(nameof(DragDelta), RoutingStrategies.Bubble);

    public new static readonly RoutedEvent<VectorEventArgsEx> DragCompletedEvent =
        RoutedEvent.Register<ThumbEx, VectorEventArgsEx>(nameof(DragCompleted), RoutingStrategies.Bubble);

    static ThumbEx()
    {
        DragStartedEvent.AddClassHandler<ThumbEx>((x, e) => x.OnDragStarted(e), RoutingStrategies.Bubble);
        DragDeltaEvent.AddClassHandler<ThumbEx>((x, e) => x.OnDragDelta(e), RoutingStrategies.Bubble);
        DragCompletedEvent.AddClassHandler<ThumbEx>((x, e) => x.OnDragCompleted(e), RoutingStrategies.Bubble);
    }

    public new event EventHandler<VectorEventArgsEx> DragStarted
    {
        add { AddHandler(DragStartedEvent, value); }
        remove { RemoveHandler(DragStartedEvent, value); }
    }

    public new event EventHandler<VectorEventArgsEx> DragDelta
    {
        add { AddHandler(DragDeltaEvent, value); }
        remove { RemoveHandler(DragDeltaEvent, value); }
    }

    public new event EventHandler<VectorEventArgsEx> DragCompleted
    {
        add { AddHandler(DragCompletedEvent, value); }
        remove { RemoveHandler(DragCompletedEvent, value); }
    }

    protected virtual void OnDragStarted(VectorEventArgsEx e)
    {
    }

    protected virtual void OnDragDelta(VectorEventArgsEx e)
    {
    }

    protected virtual void OnDragCompleted(VectorEventArgsEx e)
    {
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        if (m_lastPoint.HasValue)
        {
            var ev = new VectorEventArgsEx
            {
                RoutedEvent = DragCompletedEvent,
                Vector = m_lastPoint.Value
            };

            m_lastPoint = null;

            RaiseEvent(ev);
        }

        PseudoClasses.Remove(":pressed");

        base.OnPointerCaptureLost(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (m_lastPoint.HasValue)
        {
            var ev = new VectorEventArgsEx
            {
                RoutedEvent = DragDeltaEvent,
                Vector = e.GetPosition(this) - m_lastPoint.Value,
                PointerEventArgs = e
            };

            RaiseEvent(ev);
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        e.Handled = true;
        m_lastPoint = e.GetPosition(this);

        var ev = new VectorEventArgsEx
        {
            RoutedEvent = DragStartedEvent,
            Vector = (Avalonia.Vector)m_lastPoint,
            PointerEventArgs = e
        };

        PseudoClasses.Add(":pressed");

        e.PreventGestureRecognition();

        RaiseEvent(ev);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (m_lastPoint.HasValue)
        {
            e.Handled = true;
            m_lastPoint = null;

            var ev = new VectorEventArgsEx
            {
                RoutedEvent = DragCompletedEvent,
                Vector = e.GetPosition(this),
                PointerEventArgs = e
            };

            RaiseEvent(ev);
        }

        PseudoClasses.Remove(":pressed");
    }
}

public class VectorEventArgsEx : VectorEventArgs
{
    public PointerEventArgs? PointerEventArgs { get; init; }
}