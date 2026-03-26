using FirstPersonPlayer.Tools.ItemObjectTypes;
using MoreMountains.Tools;

namespace Helpers.Events.Combat
{
    public enum EnergyGunMode
    {
        None,
        Laser,
        Stun
    }

    public struct EnergyGunStateEvent
    {
        static EnergyGunStateEvent _e;


        public enum GunStateEventType
        {
            ChangedFireMode,
            InitializedGunState,
            UnequippedGun,
            EquippedGun
        }

        public AmmoEvent.EventDirection EventDirection;

        public EnergyGunMode NewGunMode;
        public AmmoType AmmoType;

        public GunStateEventType EventType;

        public static void Trigger(AmmoEvent.EventDirection eventDirection, EnergyGunMode newGunMode,
            GunStateEventType eventType, AmmoType ammoType)
        {
            _e.EventDirection = eventDirection;
            _e.NewGunMode = newGunMode;
            _e.EventType = eventType;
            _e.AmmoType = ammoType;

            MMEventManager.TriggerEvent(_e);
        }
    }
}
