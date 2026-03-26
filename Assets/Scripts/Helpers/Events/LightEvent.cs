using MoreMountains.Tools;

namespace Helpers.Events
{
    public enum LightEventType
    {
        TurnOn,
        TurnOff
    }

    public struct LightEvent
    {
        static LightEvent _instance;

        public LightEventType EventType;

        public static void Trigger(LightEventType eventType)
        {
            _instance.EventType = eventType;
            MMEventManager.TriggerEvent(_instance);
        }
    }
}
