using System;
using MoreMountains.Tools;

namespace Helpers.Events.UI
{
    public enum HUDType
    {
        OrbitalCalendar
    }

    [Serializable]
    public enum HUDEventType
    {
        Show,
        Hide
    }

    public struct HUDEvent
    {
        public static HUDEvent e;

        public HUDEventType EventType;
        public HUDType HUDType;

        public static void Trigger(HUDEventType eventType, HUDType hudType)
        {
            e.HUDType = hudType;
            e.EventType = eventType;
            MMEventManager.TriggerEvent(e);
        }
    }
}
