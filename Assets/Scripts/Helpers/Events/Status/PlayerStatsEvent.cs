using MoreMountains.Tools;
using UnityEngine;

namespace Helpers.Events.Status
{
    public struct PlayerStatsEvent
    {
        public enum PlayerStat
        {
            CurrentHealth,
            CurrentStamina,
            CurrentVision,
            CurrentContamination,
            // Contamination Specific
            ContaminationCU,
            ContaminationPointsPerCU,
            // Base max without buffs/debuffs applied
            BaseMaxHealth,
            BaseMaxStamina,
            BaseMaxVision,
            BaseMaxContamination,
            // Current max with buffs/debuffs applied
            CurrentMaxHealth,
            CurrentStaminaRestoreRate,
            // CurrentMaxStamina,
            CurrentMaxVision,
            CurrentMaxContamination
        }

        public enum PlayerStatChangeType
        {
            Decrease,
            Increase
        }

        public enum TypeValueStat
        {
            Fraction,
            Absolute
        }

        public enum StatChangeCause
        {
            JabbarCreche,
            Other,
            DecontaminationChamber,
            FallDamage,
            InfiniteFall,
            EnemyAttack
        }


        public PlayerStat StatType;
        public PlayerStatChangeType ChangeType;
        public float Amount;
        public float Percent;
        public float OverTime;
        public StatChangeCause Cause;
        public TypeValueStat TypeValueStatistic;
        public Vector3 SourcePosition;

        public static void Trigger(PlayerStat statType, PlayerStatChangeType changeType, float amount,
            float overTime = 0f, StatChangeCause cause = StatChangeCause.Other, float percent = 0f,
            TypeValueStat typeValueStat = TypeValueStat.Absolute, Vector3 sourcePosition = new())
        {
            var e = new PlayerStatsEvent
            {
                StatType = statType,
                ChangeType = changeType,
                Amount = amount,
                Percent = percent,
                OverTime = overTime, Cause = cause, TypeValueStatistic = typeValueStat, SourcePosition = sourcePosition
            };

            MMEventManager.TriggerEvent(e);
        }
    }
}
