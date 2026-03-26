using MoreMountains.Tools;

namespace Helpers.Events.Progression
{
    public enum LevelingEventType
    {
        LevelUp
    }

    public struct LevelingEvent
    {
        static LevelingEvent _e;
        public int NewLevel;
        public LevelingEventType EventType;
        public static void Trigger(LevelingEventType type, int newLevel)
        {
            _e.NewLevel = newLevel;
            _e.EventType = type;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
