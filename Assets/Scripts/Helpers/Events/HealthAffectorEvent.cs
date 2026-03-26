using MoreMountains.Tools;

namespace Helpers.Events
{
    public enum AffectorEventType
    {
        StatDrainActivityStarted,
        StatDrainActivityStopped
    }

    public struct HealthAffectorEvent
    {
        static HealthAffectorEvent _e;

        public float ValuePerSecond;
        public AffectorEventType EventType;

        public static void Trigger(AffectorEventType eventType, float valuePerSecond)
        {
            _e.EventType = eventType;
            _e.ValuePerSecond = valuePerSecond;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
