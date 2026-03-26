using MoreMountains.Tools;

namespace Helpers.Events
{
    public enum PauseEventType
    {
        PauseOn,
        PauseOff,
        TogglePause
    }

    public struct PauseEvent
    {
        public PauseEventType EventType;

        static PauseEvent _e;

        public static void Trigger(PauseEventType eventType)
        {
            _e.EventType = eventType;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
