using UnityEngine;
using UnityEngine.Serialization;

namespace Helpers.ScriptableObjects.Animation
{
    [CreateAssetMenu(
        fileName = "ToolAnimationSet", menuName = "Scriptable Objects/Animation/ToolAnimationSet")]
    public class ToolAnimationSet : ScriptableObject
    {
        [FormerlySerializedAs("IdleAnimation")]
        public AnimationClip idleAnimation;
        public AnimationClip idleBoredAnimation;

        public AudioClip idleBoredAudioClip;

        [FormerlySerializedAs("WalkAnimation")]
        public AnimationClip walkAnimation;
        [FormerlySerializedAs("RunAnimation")] public AnimationClip runAnimation;

        [Header("Tools with Use Pattern like Sampler")] [FormerlySerializedAs("BeginUseAnimation")]
        public AnimationClip beginUseAnimation;
        [FormerlySerializedAs("DuringUseAnimationLoopable")]
        public AnimationClip duringUseAnimationLoopable;
        [FormerlySerializedAs("EndUseAnimation")]
        public AnimationClip endUseAnimation;
        public AnimationClip secondaryUseAnimation;
        public AnimationClip blockAnimationClip;
        public AnimationClip blockHitAnimationClip;

        [Header("Audio for Tools with Use Pattern like Sampler")]
        public AudioClip beginUseAudioClip;
        public AudioClip duringUseLoopAudioClip;
        public AudioClip endUseAudioClip;
        public AudioClip endHeavyUseAudioClip;

        [Header("Tools with Use Pattern like Hatchet")]
        public AnimationClip swing01Animation;
        public AnimationClip swing02Animation;
        public AnimationClip swing03Animation;
        public AnimationClip heavySwingAnimation;

        [Header("Audio for Tools with Use Pattern like Hatchet")]
        public AudioClip swing01AudioClip;
        public AudioClip swing02AudioClip;
        public AudioClip swing03AudioClip;
        public AudioClip heavySwingAudioClip;

        [Header("Tools Pistol Pattern")] public AnimationClip aimIdleAnimation;
        public AnimationClip aimWalkAnimation;
        public AnimationClip aimShotAnimation;
        public AnimationClip nonAimShotAnimation;
        public AnimationClip reloadAnimation;

        [Header("Equip / Unequip Animations")] public AnimationClip pullOutAnimation;
        public AnimationClip putAwayAnimation;

        [Header("Audio for Equip / Unequip")] public AudioClip pullOutAudioClip;
        public AudioClip putAwayAudioClip;
        public float swing01DurationForTrailRenderer;
        public float swing02DurationForTrailRenderer;
        public float swing03DurationForTrailRenderer;
        public float heavySwingDurationForTrailRenderer;
        [Header("Speed Mult")] public float swing01SpeedMult = 1.25f;
        public float swing02SpeedMult = 1.25f;
        public float swing03SpeedMult = 1.25f;
        public float heavySwingSpeedMult = 1.0f;
    }
}
