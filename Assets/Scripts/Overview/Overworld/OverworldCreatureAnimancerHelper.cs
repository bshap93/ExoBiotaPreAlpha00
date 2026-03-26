using Animancer;
using Overview.NPC;
using UnityEngine;
using UnityEngine.Serialization;

namespace Overview.Overworld
{
    public class OverworldCreatureAnimancerHelper : MonoBehaviour
    {
        [FormerlySerializedAs("_animancer")] [SerializeField]
        AnimancerComponent animancer;
        [SerializeField] AnimationClip idleClip;
        [SerializeField] CreatureDefinition creatureDefinition;


        AnimancerState _idleState;

        void Start()
        {
            if (idleClip != null)
                _idleState = animancer.Play(idleClip);
        }

        public void PlayCreatureGesture(string key)
        {
            
            if (creatureDefinition == null)
            {
                Debug.LogWarning($"[{name}] No CreatureDefinition assigned.");
                return;
            }

            var clip = creatureDefinition.GetGesture(key);
            if (clip == null)
            {
                Debug.LogWarning($"[{creatureDefinition.creatureId}] has no gesture for '{key}'.");
                return;
            }

            var state = animancer.Play(clip);
            state.Events(animancer).OnEnd = () =>
            {
                if (_idleState != null)
                    animancer.Play(_idleState);
            };
        }
    }
}
