using System;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Helpers.ScriptableObjects.Animation
{
    [CreateAssetMenu(
        fileName = "ToolAnimationSet", menuName = "Scriptable Objects/Animation/EnemyToolWeaponAnimationSet")]
    public class EnemyToolWeaponAnimationSet : ScriptableObject
    {
        public string uniqueId;


        [Header("Block Animations")] public ActionClip blockAnimationClip;
        public ActionClip blockHitAnimationClip;

        public AnimationClip holdPoseClip;


        [Serializable]
        public class ActionClip
        {
            public bool doubleAction;
            [FormerlySerializedAs("ActionId")] public string actionId;
            [FormerlySerializedAs("AnimationClip")]
            public AnimationClip animationClip;
            // Audio Clips to play randomly in a MMFeedback MMSoundManager Sound's Random Sfx list
            [FormerlySerializedAs("audioClip")] [FormerlySerializedAs("AudioClip")]
            public AudioClip[] audioClipList;
            [FormerlySerializedAs("AudioDelay")] public float audioDelay;
            public MMFeedbacks feedbacks;
            [FormerlySerializedAs("actionDelayForSecondClip")] [ShowIf("doubleAction")]
            public float audioDelaySecondClip;
            public float Duration => animationClip != null ? animationClip.length : 0f;
        }
    }
}
