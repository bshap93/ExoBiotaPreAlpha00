using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Combat.AINPC.ScriptableObjects
{
    [Serializable]
    public enum NPCAttackType
    {
        Melee,
        Ranged,
        AOEAttack,
        ContaminantPOE
    }

    [Serializable]
    [CreateAssetMenu(
        fileName = "EnemyAttack",
        menuName = "Scriptable Objects/Character/Enemy NPC/Enemy Attack",
        order = 0)]
    public class EnemyAttack : ScriptableObject
    {
        public string displayName;
        [Header("Attack Properties")] public float rawDamage;
        // Amount that an attack ignores armor. 
        // [Range(0f, 1f)] public float armorPenetration;
        public float contaminationAmount;
        [Range(0f, 1f)] public float critChance;
        public float critMultiplier = 1f;
        [FormerlySerializedAs("knockbackForce")]
        public float rawKnockbackForce = 1f;
        public bool causesBleeding;
        // showif
        [ShowIf("causesBleeding")] [Range(0f, 1f)]
        public float chanceToCauseBleeding;
        public bool causesStagger;
        // showif
        [ShowIf("causesStagger")] [Range(0f, 1f)]
        public float chanceToCauseStagger;

        public bool causesPoisoning;

        [ShowIf("causesPoisoning")] public string poisonEffectId;
        [ShowIf("causesPoisoning")] public string poisonEffectCatalogId;
        [ShowIf("causesPoisoning")] [Range(0f, 1f)]
        public float chanceToCausePoisoning;
        // [ShowIf("causesPoisoning")] public float poisonDuration;

        public NPCAttackType attackType;

        public string testingAnimationTriggerName;


        public string AttackId => name;
    }
}
