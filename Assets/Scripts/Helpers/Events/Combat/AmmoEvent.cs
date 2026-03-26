using FirstPersonPlayer.Tools.ItemObjectTypes;
using MoreMountains.Tools;

namespace Helpers.Events.Combat
{
    public struct AmmoEvent
    {
        static AmmoEvent _e;

        public enum EventDirection
        {
            Inbound,
            Outbound
        }

        public enum AmmoEventType
        {
            ConsumedAmmo,
            PickedUpAmmo,
            InitializedAmmoAmount
        }

        public EventDirection EventDirectionVar;
        public int UnitsOfAmmo;
        public AmmoEventType EventType;
        public AmmoType AmmoType;


        public static AmmoEvent Trigger(EventDirection eventDirection, int unitsOfAmmo, AmmoEventType eventType,
            AmmoType ammoType)
        {
            _e.EventDirectionVar = eventDirection;
            _e.UnitsOfAmmo = unitsOfAmmo;
            _e.EventType = eventType;
            _e.AmmoType = ammoType;


            MMEventManager.TriggerEvent(_e);
            return _e;
        }
    }
}
