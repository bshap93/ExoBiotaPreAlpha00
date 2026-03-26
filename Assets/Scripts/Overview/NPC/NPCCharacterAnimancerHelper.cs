using Animancer;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Overview.NPC
{
    [RequireComponent(typeof(AnimancerComponent))]
    public class NPCCharacterAnimancerHelper : MonoBehaviour
    {
        [FormerlySerializedAs("_animancer")] [SerializeField]
        AnimancerComponent animancer;

        [SerializeField] AnimationClip handGesture02;
        [SerializeField] NpcDefinition npcDefinition;

        public MMF_Player dialogueSoundFeedbackPlayer;


        AnimancerState _idleState;


        void Start()
        {
            var initialIdleIndex = npcDefinition != null ? npcDefinition.initialIdleLoopingAnimationIndex : -1;
            if (npcDefinition != null &&
                npcDefinition.idleLoopingAnimations != null &&
                npcDefinition.idleLoopingAnimations.Count > initialIdleIndex &&
                npcDefinition.idleLoopingAnimations[initialIdleIndex].clip != null)
            {
                var initialIdleClip = npcDefinition.idleLoopingAnimations[initialIdleIndex].clip;
                _idleState = animancer.Play(initialIdleClip, npcDefinition.gestureTransitionDuration);
            }
        }


        public void PlayGesture(string key)
        {
            if (npcDefinition == null)
            {
                Debug.LogWarning($"[{name}] No NPCDefinition assigned.");
                return;
            }

            var clip = npcDefinition.GetGesture(key);
            if (clip == null)
            {
                Debug.LogWarning($"[{npcDefinition.characterName}] has no gesture for '{key}'.");
                return;
            }

            var state = animancer.Play(clip, npcDefinition.gestureTransitionDuration);
            state.Events(animancer).OnEnd = () =>
            {
                if (_idleState != null)
                    animancer.Play(_idleState, npcDefinition.gestureTransitionDuration);
            };
        }
        public void PlaySound(string key)
        {
            if (dialogueSoundFeedbackPlayer == null)
            {
                Debug.LogWarning($"[{name}] No dialogueSoundFeedbackPlayer assigned.");
                return;
            }

            var clip = npcDefinition.GetDialogueSound(key);
            if (clip == null)
            {
                Debug.LogWarning($"[{npcDefinition.characterName}] has no dialogue sound for '{key}'.");
                return;
            }

            var mmfSound = dialogueSoundFeedbackPlayer.FeedbacksList[0] as MMF_MMSoundManagerSound;
            if (mmfSound == null)
            {
                Debug.LogWarning($"[{name}] dialogueSoundFeedbackPlayer has no MMF_MMSoundManagerSound feedback.");
                return;
            }

            mmfSound.Sfx = clip;

            dialogueSoundFeedbackPlayer?.PlayFeedbacks();
        }
        public void SwitchIdleLoopingAnimation(string key)
        {
            if (npcDefinition == null)
            {
                Debug.LogWarning($"[{name}] No NPCDefinition assigned.");
                return;
            }

            var clip = npcDefinition.GetIdleLoopingClip(key);
            if (clip == null)
            {
                Debug.LogWarning($"[{npcDefinition.characterName}] has no idle looping animation for '{key}'.");
                return;
            }

            _idleState = animancer.Play(clip, npcDefinition.idleTransitionDuration);
        }
    }
}
