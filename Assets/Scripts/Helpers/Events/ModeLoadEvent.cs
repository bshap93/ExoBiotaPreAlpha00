using MoreMountains.Tools;
using Structs;

namespace Events
{
    public enum ModeLoadEventType
    {
        Load,
        Unload,
        Enabled,
        Disabled
    }

    public struct ModeLoadEvent
    {
        static ModeLoadEvent _e;

        public ModeLoadEventType EventType;
        public GameMode ModeName;
        public string DockId;

        public static void Trigger(ModeLoadEventType eventType, GameMode modeName, string dockId = null)
        {
            _e.EventType = eventType;

            _e.ModeName = modeName;
            _e.DockId = dockId;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
