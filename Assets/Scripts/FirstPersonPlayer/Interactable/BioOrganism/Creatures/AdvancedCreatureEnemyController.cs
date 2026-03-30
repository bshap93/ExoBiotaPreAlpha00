using FirstPersonPlayer.FPNPCs.AlienNPC;
using Helpers.Events.Combat;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace FirstPersonPlayer.Interactable.BioOrganism.Creatures
{
    public class AdvancedCreatureEnemyController : EnemyController

    {
        [SerializeField] MMFeedbacks walkingLoopFeedbacks;
        [SerializeField] MMFeedbacks runningLoopFeedbacks;

        [SerializeField] AlienNPCState initialState;
        public AlienNPCState CurrentState { get; set; }

        protected override void Start()
        {
            base.Start();

            // If no persisted state (none for now)
            SetState(initialState, isInitiallyHostile);
        }

        public void SetState(AlienNPCState newState, bool isHostile)
        {
            var stateChanged = newState != CurrentState || isHostile != IsHostile;

            CurrentState = newState;
            IsHostile = isHostile;

            // Working/stationary states are "custom" from EnemyController's perspective —
            // this prevents Update() from stomping them with IdleState.
            IsPlayingCustomAnimation = newState == AlienNPCState.Working
                                       || newState == AlienNPCState.InDialogue
                                       || newState == AlienNPCState.FriendlyAndHailable;


            if (stateChanged)
                AlienNotifyFriendsOfStateEvent.Trigger(uniqueID, isHostile, newState);


            PlayAnimationsForState(newState);
        }

        void PlayAnimationsForState(AlienNPCState state)
        {
        }
    }
}
