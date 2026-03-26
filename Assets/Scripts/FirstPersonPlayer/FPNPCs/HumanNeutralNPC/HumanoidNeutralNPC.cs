using Animancer;
using Overview.NPC;
using UnityEngine;

namespace FirstPersonPlayer.FPNPCs.HumanNeutralNPC
{
    public class HumanoidNeutralNPC : MonoBehaviour
    {
        [SerializeField] NpcDefinition npcDefinition;

        [SerializeField] AnimancerComponent animancerComponent;

        [SerializeField] AnimationClip defaultIdleAnimation;

        protected AnimancerState IdleState;

        public NpcDefinition NpcDefinition => npcDefinition;
        public AnimancerComponent AnimancerComponent => animancerComponent;

        void Awake()
        {
            // Pre-load looping animation states
            IdleState = animancerComponent.States.GetOrCreate(npcDefinition.GetDefaultIdleAnimation());
            IdleState.Speed = 1f;
            IdleState.Time = 0f;
            IdleState.Events(this).OnEnd = () => { IdleState.Time = 0f; };
        }

        void Start()
        {
            if (defaultIdleAnimation != null) animancerComponent.Play(defaultIdleAnimation);
        }
    }
}
