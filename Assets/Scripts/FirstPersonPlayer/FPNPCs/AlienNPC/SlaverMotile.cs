using System;
using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
using UnityEngine;

namespace FirstPersonPlayer.FPNPCs.AlienNPC
{
    public class SlaverMotile : EnemyController
    {
        public enum BroadcastType
        {
            HostileAgent
        }

        [Serializable]
        public enum SlaverFlagType
        {
            Alerted,
            Hostile,
            IsPlayerTheTargetOfPursuit,
            DesiresDialogue
        }

        [Serializable]
        public enum SlaverMotileState
        {
            Supervising,
            Patrolling,
            Pursuing,
            InDialogue
        }

        [SerializeField] Transform[] tentacleAnchors;
        [SerializeField] Transform[] klaxonAnchors;
        [SerializeField] Transform bodyHeadCenterAnchor;
        [SerializeField] AlienNPCAnimancerController animancerController;

        public HumanoidNPCCreature[] thrallCreatureCharacters;

        public SlaverMotileState initialSlaverMotileState;

        public bool initialDesiresDialogue;

        public SlaverMotileState CurrentSlaverMotileState { get; private set; }

        public AlienNPCState CurrentAnimancerState { get; private set; }

        public bool Alerted { get; private set; }

        public bool Hostile { get; private set; }


        public bool DesiresDialogue { get; private set; }

        protected override void Start()
        {
            base.Start();

            DesiresDialogue = initialDesiresDialogue;

            SetState(animancerController.CurrentState, initialSlaverMotileState);
        }

        public void SetSlaverFlag(SlaverFlagType flag, bool value)
        {
            switch (flag)
            {
                case SlaverFlagType.Alerted:
                    Alerted = value;
                    break;
                case SlaverFlagType.Hostile:
                    Hostile = value;
                    break;
                case SlaverFlagType.IsPlayerTheTargetOfPursuit:
                    blackboard.SetVariableValue("IsPlayerTheTargetOfPursuit", value);
                    break;
                case SlaverFlagType.DesiresDialogue:
                    DesiresDialogue = value;
                    break;
                default:
                    Debug.LogWarning($"Unimplemented SlaverFlagType {flag}");
                    break;
            }
        }

        public void SetState(AlienNPCState newState, SlaverMotileState newSlaverMotileState)
        {
            CurrentAnimancerState = newState;
            CurrentSlaverMotileState = newSlaverMotileState;

            // Working/stationary states are "custom" from EnemyController's perspective —
            // this prevents Update() from stomping them with IdleState.
            IsPlayingCustomAnimation = newState == AlienNPCState.Idling
                                       || newState == AlienNPCState.Working
                                       || newState == AlienNPCState.InDialogue
                ;

            animancerController.PlayAnimationsForState(newState);
        }
        public void BroadcastToThralls(BroadcastType broadcastType)
        {
            switch (broadcastType)
            {
                case BroadcastType.HostileAgent:
                    foreach (var thrall in thrallCreatureCharacters) thrall.SetState(AlienNPCState.Searching);

                    break;
                default:
                    Debug.LogWarning($"Unimplemented BroadcastType {broadcastType}");
                    break;
            }
        }
    }
}
