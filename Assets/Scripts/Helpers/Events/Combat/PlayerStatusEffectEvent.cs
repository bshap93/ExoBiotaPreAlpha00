using Manager.Status.Scriptable;
using MoreMountains.Tools;

namespace Helpers.Events.Combat
{
    public struct PlayerStatusEffectEvent
    {
        static PlayerStatusEffectEvent _e;

        public enum StatusEffectEventType
        {
            Apply,
            Remove,
            RemoveAllFromCatalog,
            RemoveAllOfAKind
        }


        public enum DirectionOfEvent
        {
            Inbound,
            Outbound
        }

        public StatusEffectEventType Type;

        public string EffectID;
        public string CatalogID;
        public DirectionOfEvent Direction;
        public StatusEffect.StatusEffectKind StatusEffectKind;

        public static void Trigger(StatusEffectEventType type, string effectID, string catalogID,
            DirectionOfEvent direction, StatusEffect.StatusEffectKind statusEffectKind)
        {
            _e.Type = type;
            _e.EffectID = effectID;
            _e.CatalogID = catalogID;
            _e.StatusEffectKind = statusEffectKind;
            _e.Direction = direction;


            MMEventManager.TriggerEvent(_e);
        }
    }
}
