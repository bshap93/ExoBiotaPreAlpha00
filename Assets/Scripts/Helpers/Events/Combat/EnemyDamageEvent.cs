using System;
using MoreMountains.Tools;

namespace Helpers.Events.Combat
{
    [Serializable]
    public enum DamageEventType
    {
        DealtDamage,
        CriticalHitDamage,
        Missed,
        Healed,
        Blocked,
        Death,
        ShowUI,
        HideUI
    }

    public enum DamageType
    {
        Health,
        Stun,
        None
    }


    public struct EnemyDamageEvent
    {
        static EnemyDamageEvent _e;

        public float CurrentValue;
        public float LastValue;
        public float DefaultValue;
        public DamageEventType EventType;
        public string EnemyName;
        public DamageType TypeOfDamage;

        public static void Trigger(float currentHealth, float lastHealth, float maxHealth, DamageEventType eventType,
            string enemyName, DamageType typeOfDamage)
        {
            _e.CurrentValue = currentHealth;
            _e.DefaultValue = maxHealth;
            _e.EventType = eventType;
            _e.LastValue = lastHealth;
            _e.EnemyName = enemyName;
            _e.TypeOfDamage = typeOfDamage;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
