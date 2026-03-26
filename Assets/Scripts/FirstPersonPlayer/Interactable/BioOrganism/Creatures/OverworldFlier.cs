using Animancer;
using MoreMountains.Feedbacks;
using NodeCanvas.Framework;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace FirstPersonPlayer.Interactable.BioOrganism.Creatures
{
    [RequireComponent(typeof(Blackboard))]
    [RequireComponent(typeof(AnimancerComponent))]
    [DisallowMultipleComponent]
    public class OverworldFlierController : CreatureController
    {
        [SerializeField] NavMeshAgent navMeshAgent;


        [SerializeField] MMFeedbacks creatureCallFeedbacks;

        [SerializeField] float minCallDelay = 10f;
        [SerializeField] float maxCallDelay = 15f;


        float _nextCallTime;


        protected override void Start()
        {
            base.Start();
            // Screech immediately on spawn
            // creatureCallFeedbacks?.PlayFeedbacks();
            ScheduleNextCall();
        }

        void Update()
        {
            if (!IdleState.IsPlaying)
                animancerComponent.Play(IdleState);

            if (Time.time >= _nextCallTime)
            {
                creatureCallFeedbacks?.PlayFeedbacks();
                ScheduleNextCall();
            }
        }


        void ScheduleNextCall()
        {
            _nextCallTime = Time.time + Random.Range(minCallDelay, maxCallDelay);
        }
    }
}
