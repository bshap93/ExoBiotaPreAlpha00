using Helpers.ScriptableObjects.Animation;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Combat.AINPC.ScriptableObjects
{
    [CreateAssetMenu(
        fileName = "EnemyNPCAnimationSet",
        menuName = "Scriptable Objects/Character/Enemy NPC/Creature NPC Animation Set",
        order = 0)]
    public class CreatureAnimationSet : ScriptableObject
    {
        [Header("Options")] public bool isHumanoid;
        [Header("Idle Animations")] public AnimationClip idleAnimation;
        public AnimationClip additionalIdleAnimation0;
        public AnimationClip additionalIdleAnimation1;
        public AnimationClip rangedAttackAnimation;

        [Header("Idle Animation Speed Multipliers")] [Range(0, 3)]
        public int numberOfIdleAnimations = 1;
        public float idleAnimationSpeedMultiplier = 1f;
        public float additionalIdleAnimation0SpeedMultiplier = 1f;
        public float additionalIdleAnimation1SpeedMultiplier = 1f;

        [Header("Movement and Combat Animations")]
        public AnimationClip moveAnimation;
        public AnimationClip deathAnimation;
        public AnimationClip getHitAnimation;

        [FormerlySerializedAs("walkAnimation")]
        [Header("Humanoid Movement Animations")]
        [Tooltip("Played when moving below the walk/run threshold.")]
        [ShowIf("IsHumanoid")]
        public EnemyToolWeaponAnimationSet.ActionClip walkActionClip;
        [FormerlySerializedAs("runAnimation")]
        [Tooltip("Played when moving at or above the walk/run threshold.")]
        [ShowIf("IsHumanoid")]
        public EnemyToolWeaponAnimationSet.ActionClip runActionClip;

        [Header("Humanoid Directional Animations (optional)")]
        [Tooltip("Leave null to mirror forward animations for non-directional creatures.")]
        [ShowIf("IsHumanoid")]
        public AnimationClip strafeLeftAnimation;
        [ShowIf("IsHumanoid")] public AnimationClip strafeRightAnimation;
        [ShowIf("IsHumanoid")] public AnimationClip walkBackAnimation;


        [Header("Movement Animation Speed Multiplier")]
        public float moveAnimationSpeedMultiplier = 1f;
        public float deathAnimationSpeedMultiplier = 1f;
        public float getHitAnimationSpeedMultiplier = 1f;

        [Header("Humanoid Movement Animation Speed Multipliers")] [ShowIf("IsHumanoid")]
        public float walkAnimationSpeedMultiplier = 1f;
        [ShowIf("IsHumanoid")] public float runAnimationSpeedMultiplier = 1f;
        [ShowIf("IsHumanoid")] public float strafeAnimationSpeedMultiplier = 1f;
        [ShowIf("IsHumanoid")] public float walkBackAnimationSpeedMultiplier = 1f;

        public bool IsHumanoid => isHumanoid;
    }
}
