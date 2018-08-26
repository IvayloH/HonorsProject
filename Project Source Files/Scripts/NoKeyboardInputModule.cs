using UnityEngine;
using UnityEngine.EventSystems;

public class NoKeyboardInputModule : StandaloneInputModule
{
    public override void Process()
    {
        SendUpdateEventToSelectedObject();
        ProcessMouseEvent();
    }

    private new bool SendUpdateEventToSelectedObject()
    {
        if (eventSystem.currentSelectedGameObject == null)
            return false;

        var data = GetBaseEventData();
        ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
        return data.used;
    }

    private new void ProcessMouseEvent()
    {
        var mouseData = GetMousePointerEventData();

        var pressed = mouseData.AnyPressesThisFrame();
        var released = mouseData.AnyReleasesThisFrame();

        var leftButtonData = mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;

        if (!UseMouse(pressed, released, leftButtonData.buttonData))
            return;

        // Process the first mouse button fully
        ProcessMousePress(leftButtonData);
        ProcessMove(leftButtonData.buttonData);
        ProcessDrag(leftButtonData.buttonData);

        // Now process right / middle clicks
        ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData);
        ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
        ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
        ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);

        if (!Mathf.Approximately(leftButtonData.buttonData.scrollDelta.sqrMagnitude, 0.0f))
        {
            var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(leftButtonData.buttonData.pointerCurrentRaycast.gameObject);
            ExecuteEvents.ExecuteHierarchy(scrollHandler, leftButtonData.buttonData, ExecuteEvents.scrollHandler);
        }
    }

    private static bool UseMouse(bool pressed, bool released, PointerEventData pointerData)
    {
        if (pressed || released || pointerData.IsPointerMoving() || pointerData.IsScrolling())
            return true;

        return false;
    }

    private void ProcessMousePress(PointerEventData pointerEvent, bool pressed, bool released)
    {
        var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

        // PointerDown notification
        if (pressed)
        {
            pointerEvent.eligibleForClick = true;
            pointerEvent.delta = Vector2.zero;
            pointerEvent.pressPosition = pointerEvent.position;
            pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

            // search for the control that will receive the press
            // if we can't find a press handler set the press 
            // handler to be what would receive a click.
            var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);

            // didnt find a press handler... search for a click handler
            if (newPressed == null)
                newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

            if (newPressed != pointerEvent.pointerPress)
            {
                pointerEvent.pointerPress = newPressed;
                pointerEvent.rawPointerPress = currentOverGo;
                pointerEvent.clickCount = 0;
            }

            // Save the drag handler as well
            pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

            if (pointerEvent.pointerDrag != null)
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);

            // Selection tracking
            var selectHandlerGO = ExecuteEvents.GetEventHandler<ISelectHandler>(currentOverGo);
            eventSystem.SetSelectedGameObject(selectHandlerGO, pointerEvent);
        }

        // PointerUp notification
        if (released)
        {
            ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

            // see if we mouse up on the same element that we clicked on...
            var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

            // PointerClick and Drop events
            if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick)
            {
                float time = Time.unscaledTime;

                if (time - pointerEvent.clickTime < 0.3f)
                    ++pointerEvent.clickCount;
                else
                    pointerEvent.clickCount = 1;
                pointerEvent.clickTime = time;

                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
            }
            else if (pointerEvent.pointerDrag != null)
            {
                ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
            }

            pointerEvent.eligibleForClick = false;
            pointerEvent.pointerPress = null;
            pointerEvent.rawPointerPress = null;

            if (pointerEvent.pointerDrag != null)
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

            pointerEvent.pointerDrag = null;

            // redo pointer enter / exit to refresh state
            // so that if we moused over somethign that ignored it before
            // due to having pressed on something else
            // it now gets it.
            HandlePointerExitAndEnter(pointerEvent, null);
            HandlePointerExitAndEnter(pointerEvent, currentOverGo);
        }
    }
}