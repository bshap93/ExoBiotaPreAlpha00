using System.Collections;
using System.Collections.Generic;
using creepycat.scifikitvol4;
using FirstPersonPlayer.Interactable.Stateful;
using Helpers.Events;
using Helpers.ScriptableObjects.Gated;
using Helpers.StaticHelpers;
using LevelConstruct.Interactable.ItemInteractables;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace FirstPersonPlayer.Interactable.Gated
{
    public class InteractableGenerator : InteractableMachine
    {
        [SerializeField] TurnMove rotatingLightTurnMove;
        [SerializeField] GameObject powerOnDecal;
        [SerializeField] List<GameObject> allLightsDependentOnGenerator;
        [SerializeField] List<ActionConsole> allConsolesDependentOnGenerator;
        [SerializeField] List<StatefulElevator> allElevatorsDependentOnGenerator;
        [SerializeField] MMFeedbacks localGeneratorRunningFeedbacks;

        void Start()
        {
            StartCoroutine(InitializeAfterMachineStateManager());
        }

        public override void OnInteractionEnd(string subjectUniquedID)
        {
            base.OnInteractionEnd(subjectUniquedID);
            if (gatedInteractionDetails.machineInteractionType == MachineInteractionType.AddBatteryLikeItem)
            {
                var powerSourceType = machineType.powerSourceItem;
                InventoryHelperCommands.RemovePlayerItem(powerSourceType.ItemID);

                currentMachineState = MachineState.Operating;
                SetAllDependentsActive();
                MachineStateEvent.Trigger(uniqueID, currentMachineState);
            }
        }


        protected override IEnumerator InitializeAfterMachineStateManager()
        {
            yield return base.InitializeAfterMachineStateManager();

            switch (currentMachineState)
            {
                case MachineState.Broken:
                case MachineState.Off:
                    SetAllDependentsInactive();
                    break;
                case MachineState.Operating:
                    SetAllDependentsActive();
                    break;
            }
        }

        void SetAllDependentsInactive()
        {
            rotatingLightTurnMove.enabled = false;
            powerOnDecal.SetActive(false);
            foreach (var dependentLight in allLightsDependentOnGenerator) dependentLight.SetActive(false);
            foreach (var console in allConsolesDependentOnGenerator) console.SetConsoleToLacksPowerState();
            localGeneratorRunningFeedbacks?.StopFeedbacks();
        }


        void SetAllDependentsActive()
        {
            rotatingLightTurnMove.enabled = true;
            powerOnDecal.SetActive(true);
            foreach (var dependentLight in allLightsDependentOnGenerator) dependentLight.SetActive(true);
            foreach (var console in allConsolesDependentOnGenerator) console.SetConsoleToPoweredOnState();
            localGeneratorRunningFeedbacks?.PlayFeedbacks();
        }
    }
}
