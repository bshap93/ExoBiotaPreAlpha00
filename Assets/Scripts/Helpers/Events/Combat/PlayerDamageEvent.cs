using MoreMountains.Tools;

namespace Helpers.Events.Combat
{
    public struct PlayerDamageEvent
    {
        static PlayerDamageEvent _e;

        public enum DamageTypes
        {
            Melee,
            Ranged,
            AreaOfEffect
        }

        public enum HitTypes
        {
            Normal,
            CriticalHit,
            CriticalMiss
        }

        public DamageTypes DamageType;
        public HitTypes HitType;


        public static void Trigger(DamageTypes damageType, HitTypes hitType)
        {
            _e.DamageType = damageType;
            _e.HitType = hitType;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
