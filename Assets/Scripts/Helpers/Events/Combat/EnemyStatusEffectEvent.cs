using MoreMountains.Tools;

namespace Helpers.Events.Combat
{
    public enum EnemyStatusEffectType
    {
        Stun,
        Dead
    }

    public struct EnemyStatusEffectEvent
    {
        public string EnemyUniqueID;
        public EnemyStatusEffectType EffectType;
        static EnemyStatusEffectEvent _e;
        public float Value;

        public static void Trigger(string enemyID, EnemyStatusEffectType effectType, float value = 0)
        {
            _e.EnemyUniqueID = enemyID;
            _e.EffectType = effectType;
            _e.Value = value;

            MMEventManager.TriggerEvent(_e);
        }
    }
}
