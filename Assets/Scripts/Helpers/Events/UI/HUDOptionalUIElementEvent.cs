using System;
using MoreMountains.Tools;

namespace Helpers.Events.UI
{
    [Serializable]
    public enum HUDOptionalUIElement
    {
        ControlCheetsheet
    }

    [Serializable]
    public enum HUDOptionalUIElementEventType
    {
        Toggle,
        Show,
        Hide
    }

    public struct HUDOptionalUIElementEvent
    {
        static HUDOptionalUIElementEvent e;

        public HUDOptionalUIElement element;
        public HUDOptionalUIElementEventType eventType;

        public static void Trigger(HUDOptionalUIElement element, HUDOptionalUIElementEventType eventType)
        {
            e.element = element;
            e.eventType = eventType;
            MMEventManager.TriggerEvent(e);
        }
    }
}
