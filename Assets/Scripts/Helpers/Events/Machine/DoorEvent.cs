using MoreMountains.Tools;

namespace Helpers.Events.Machine
{
    public enum DoorEventType
    {
        Unlock,
        Open,
        Close,
        Lock
    }

    public struct DoorEvent
    {
        static DoorEvent _e;

        public string UniqueId;
        public DoorEventType EventType;
        public static void Trigger(string doorId, DoorEventType type)
        {
            _e.UniqueId = doorId;
            _e.EventType = type;

            MMEventManager.TriggerEvent(_e);
        }
    }
}
