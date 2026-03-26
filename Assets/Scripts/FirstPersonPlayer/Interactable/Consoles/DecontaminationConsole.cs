using System;
using System.Collections;
using Helpers.Events;
using Helpers.Events.Combat;
using Helpers.Events.Status;
using LevelConstruct.Interactable.ItemInteractables;
using Manager;
using Manager.Status.Scriptable;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace FirstPersonPlayer.Interactable.Consoles
{
    [DisallowMultipleComponent]
    public class DecontaminationConsole : ActionConsole, MMEventListener<PlayerStatusEffectEvent>
    {
        [SerializeField] MMFeedbacks decontaminationStartFeedback;

        [SerializeField] GameObject screenForConsole;
        protected override void OnEnable()
        {
            base.OnEnable();
            this.MMEventStartListening();
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            this.MMEventStopListening();
        }
        public void OnMMEvent(PlayerStatusEffectEvent eventType)
        {
        }
        protected override IEnumerator InitializeAfterMachineStateManager()
        {
            yield return base.InitializeAfterMachineStateManager();

            switch (currentConsoleState)
            {
                case ActionConsoleState.Broken:
                case ActionConsoleState.LacksPower:
                    SetConsoleToLacksPowerState();
                    break;
                case ActionConsoleState.PoweredOn:
                    SetConsoleToPoweredOnState();
                    break;
            }
        }
        public override void Interact()
        {
            if (!CanInteract())
            {
                AlertWhyCant();
                return;
            }

            if (PlayerMutableStatsManager.Instance.CurrentContamination <= 0f)
            {
                AlertEvent.Trigger(
                    AlertReason.Decontamination,
                    "You are not contaminated.",
                    "OK");

                return;
            }


            BillboardEvent.Trigger(null, BillboardEventType.Hide);
            ControlsHelpEvent.Trigger(
                ControlHelpEventType.Hide, actionId);


            AlertEvent.Trigger(
                AlertReason.Decontamination,
                "Decontamination in progress...",
                "Decontaminate?",
                AlertType.ChoiceModal,
                0f,
                onConfirm: () =>
                {
                    PlayerStatsEvent.Trigger(
                        PlayerStatsEvent.PlayerStat.CurrentContamination,
                        PlayerStatsEvent.PlayerStatChangeType.Decrease, 100f, 2f,
                        PlayerStatsEvent.StatChangeCause.DecontaminationChamber, sourcePosition: transform.position);

                    PlayerStatusEffectEvent.Trigger(
                        PlayerStatusEffectEvent.StatusEffectEventType.RemoveAllOfAKind, null, null,
                        PlayerStatusEffectEvent.DirectionOfEvent.Inbound,
                        StatusEffect.StatusEffectKind.MinorInfections);


                    decontaminationStartFeedback?.PlayFeedbacks();
                }, onCancel: () => { }
            );
        }
        void AlertWhyCant()
        {
            if (currentConsoleState == ActionConsoleState.Broken)
                AlertEvent.Trigger(
                    AlertReason.BrokenMachine, "The decontamination console is broken and cannot be used.",
                    "Decontamination Console");
            else if (currentConsoleState == ActionConsoleState.LacksPower)
                AlertEvent.Trigger(
                    AlertReason.MachineLacksPower, "The decontamination console lacks power and cannot be used.",
                    "Decontamination Console");
        }

        public override bool CanInteract()
        {
            return base.CanInteract();
        }

        public void TriggerIsMinContamEvent()
        {
            StatsStatusEvent.Trigger(
                true, StatsStatusEvent.StatsStatus.IsMin, StatsStatusEvent.StatsStatusType.Contamination);
        }

        public override void OnInteractionStart()
        {
        }

        public override void OnInteractionEnd()
        {
        }

        protected override string GetActionText(bool recognizableOnSight)
        {
            return "Decontaminate";
        }
        public override void SetConsoleToLacksPowerState()
        {
            if (screenForConsole != null)
                screenForConsole.SetActive(false);

            currentConsoleState = ActionConsoleState.LacksPower;
        }
        public override void SetConsoleToPoweredOnState()
        {
            if (screenForConsole != null)
                screenForConsole.SetActive(true);

            currentConsoleState = ActionConsoleState.PoweredOn;
        }
        public override void SetConsoleToHailPlayerState()
        {
            throw new NotImplementedException();
        }
    }
}
