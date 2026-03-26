using System;
using System.Collections.Generic;
using Helpers.Events.Status;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Manager.Status.Scriptable
{
    [Serializable]
    public struct StatsChange
    {
        [FormerlySerializedAs("Stat")] public PlayerStatsEvent.PlayerStat statType;
        public PlayerStatsEvent.PlayerStatChangeType changeType;
        public float amount;
        [Range(0f, 1f)] public float percent;
        public Sprite icon;
        public bool isPositive;
    }

    [CreateAssetMenu(fileName = "StatusEffect", menuName = "Scriptable Objects/Character/StatusEffect", order = 1)]
    public class StatusEffect : ScriptableObject
    {
        [Serializable]
        public enum StatusEffectKind
        {
            MinorInfections,
            None,
            Poison
        }

        [FormerlySerializedAs("Description")] [TextArea(1, 4)]
        public string description;

        [Header("Visual Effects")] public bool distortion;
        public Sprite effectIcon;
        [FormerlySerializedAs("EffectID")] public string effectID;
        [FormerlySerializedAs("EffectName")] public string effectName;
        public bool floaters;

        [Header("Perceptual Effects")] public bool intrusiveThoughts;

        [Header("Removal Settings")] public bool removableViaDecontaminationTank = true;

        public bool causesPoisoning;
        [ShowIf("causesPoisoning")] public float poisonDamagePerSecond = 3f;

        [ShowIf("causesPoisoning")] public float poisonDuration = 10f;

        [Range(0f, 1f)] public float riskOfDeath;
        [FormerlySerializedAs("stats")] [FormerlySerializedAs("Stats")]
        public List<StatsChange> statsChanges = new();
        public StatusEffectKind statusEffectKind = StatusEffectKind.None;
        public string catalogID;
    }
}
