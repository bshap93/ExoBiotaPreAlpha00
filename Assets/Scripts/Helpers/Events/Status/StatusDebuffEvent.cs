using MoreMountains.Tools;

namespace Helpers.Events.Status
{
    public struct StatusDebuffEvent
    {
        public enum StatusDebuffEventType
        {
            Apply,
            Remove,
            Resisted
        }

        public enum DebuffType
        {
            Poison
        }

        public string EffectID;
        public StatusDebuffEventType Type;
        public DebuffType Debuff;

        static StatusDebuffEvent _e;

        public static void Trigger(StatusDebuffEventType type, DebuffType debuff, string effectID)
        {
            _e.Type = type;
            _e.Debuff = debuff;
            _e.EffectID = effectID;

            MMEventManager.TriggerEvent(_e);
        }
    }
}
