using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Combat.Player.ScriptableObjects
{
    [Serializable]
    public enum PlayerAttackType
    {
        Melee,
        Ranged,
        AOE,
        Instantiating
    }

    public enum HitType
    {
        Normal,
        Heavy
    }


    [CreateAssetMenu(
        fileName = "PlayerAttack_",
        menuName = "Scriptable Objects/Character/First Person Player/Player Attack",
        order = 0)]
    public class PlayerAttack : ScriptableObject
    {
        [Header("Basic Properties")] public string displayName;
        public PlayerAttackType attackType;

        // [ShowIf("attackType", PlayerAttackType.Ranged)]
        // public float baseRange;

        [Header("Damage Properties")] public float baseBlowbackContaminationMultiplier = 1f;
        public float rawDamage;
        public bool causesStunDamage;
        [ShowIf("causesStunDamage")] public float rawStunDamage;

        [SerializeField] bool attackCostsAmmo;
        [ShowIf("attackCostsAmmo")] [Header("Ammo Cost Properties")]
        public int ammoUnitCostPerAttack;
        [SerializeField] bool attackCostsStamina;
        [ShowIf("attackCostsStamina")] public float baseStaminaCost;
        [Header("Critical Hit Properties")] [Range(0f, 1f)]
        public float critChance;
        public float critMultiplier = 1.2f;


        [FormerlySerializedAs("damageType")] public HitType hitType;
        public float rawKnockbackForce;

        [Header("Bleeding Effect Properties")] public bool causesBleeding;
        [ShowIf("causesBleeding")] [Range(0f, 1f)]
        public float chanceToCauseBleeding;

        [Header("Stagger Effect Properties")] public bool causesStagger;
        [ShowIf("causesStagger")] [Range(0f, 1f)]
        public float chanceToCauseStagger;

        [Header("Corrosion Effect Properties")]
        public bool causesCorrosion;
        // Is not as fast if enemy is corrosive resistant
        [ShowIf("causesCorrosion")] public float baseCorrosionDuration;
        [ShowIf("causesCorrosion")] public float baseDamagePerSecond;


        [Header("Effect on Player Character Controller")]
        public bool playerMovesWithAttack;
        [ShowIf("playerMovesWithAttack")] public float movementAmount;


        [Header("Area of Effect Properties")] [FormerlySerializedAs("HasAOEEffect")]
        public bool hasAOEEffect;
        [ShowIf("hasAOEEffect")] public float aoeRadius;
        [ShowIf("hasAOEEffect")] public bool aoeAtPlayerPosition;
        [ShowIf("hasAOEEffect")] public bool aoeAtHitPointPosition;

        [Header("Metadata")] public float totalAttackDuration = 1f;
        public string AttackID => name;
    }
}
