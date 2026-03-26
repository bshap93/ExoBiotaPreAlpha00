using MoreMountains.Tools;

namespace Helpers.Events
{
    public enum StaminaAffectorEventType
    {
        StaminaDrainActivityStarted,
        StaminaDrainActivityStopped,
        StaminaRestoreActivityStarted,
        StaminaRestoreActivityStopped
    }

    public struct StaminaAffectorEvent
    {
        static StaminaAffectorEvent _e;

        public float ValuePerSecond;
        public StaminaAffectorEventType EventType;

        public static void Trigger(StaminaAffectorEventType eventType, float valuePerSecond)
        {
            _e.EventType = eventType;
            _e.ValuePerSecond = valuePerSecond;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
