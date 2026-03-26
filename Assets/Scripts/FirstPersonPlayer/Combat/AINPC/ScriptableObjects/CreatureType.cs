using FirstPersonPlayer.ScriptableObjects.BioticAbility;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Combat.AINPC.ScriptableObjects
{
    [CreateAssetMenu(
        fileName = "EnemyType",
        menuName = "Scriptable Objects/Character/Creature NPC/Creature Type",
        order = 0)]
    public class CreatureType : ScriptableObject
    {
        [FormerlySerializedAs("enemyName")] public string creatureName;
        // [Header("Attacks and Animation")] public CreatureAttacksProfile attacksProfile;
        public CreatureAnimationSet animationSet;
        [FormerlySerializedAs("vfxSet")] public CreatureEffectsAndFeedbacks effectsAndFeedbacks;
        [Header("Combat Settings")] public float wasHitCooldownTime = 0.5f;
        public float primaryAttackDuration;
        [Header("Camera Shake Settings")] public float meleeAttackShakeIntensity = 0.12f;
        public float heavyMeleeAttackShakeIntensity = 0.15f;
        public float meleeAttackShakeDuration = 0.3f;
        public float stunCooldownTime = 5f;
        public bool shouldDeactivateWhenPlayerLeavesImmediateArea;

        [Header("Contamination Settings")] public float baseBlowbackContaminationAmt = 0.5f;

        public float maxHealth;
        public float stunTreshold;
        public float rangedAttackShakeIntensity;
        public float rangedAttackShakeDuration;
        public float heavyRangedAttackShakeIntensity;
        public Sprite actionIcon;
        public string shortDescription;
        public Sprite creatureIcon;
        public float weaknessToKnockback = 1f;


        [Header("Special Effects")] public string placatedStartNode;

        public BioticAbility.SpecialEffectType[] specialEffectTypesSusceptible;

        public bool givesExperienceReward = true;
        [ShowIf("givesExperienceReward")] public int experienceRewardAmount = 10;

        [Header("Drop Loot Settings")] [SerializeField]
        public MMLootTableGameObjectSO lootDefinition;
    }
}
