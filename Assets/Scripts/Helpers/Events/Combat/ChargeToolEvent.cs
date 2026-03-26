using MoreMountains.Tools;

namespace Helpers.Events.Combat
{
    public enum ChargeToolEventType
    {
        Start,
        Update,
        Release,
        Cancel
    }

    public struct ChargeToolEvent
    {
        static ChargeToolEvent _e;

        public ChargeToolEventType EventType;
        public float FractionCharged;

        public static void Trigger(ChargeToolEventType eventType, float fractionCharged = 0f)
        {
            _e.EventType = eventType;
            _e.FractionCharged = fractionCharged;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
