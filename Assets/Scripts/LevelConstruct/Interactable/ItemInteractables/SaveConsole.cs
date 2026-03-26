using System;
using System.Collections;
using FirstPersonPlayer.Interface;
using Helpers.Events;
using LevelConstruct.Highlighting;
using LevelConstruct.Spawn;
using Manager;
using Structs;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Interface;

namespace LevelConstruct.Interactable.ItemInteractables
{
    [RequireComponent(typeof(MeshCollider))]
    [DisallowMultipleComponent]
    public class SaveConsole : ActionConsole, IInteractable, IRequiresUniqueID
    {
        [FormerlySerializedAs("_spawnPoint")] public SpawnPoint spawnPoint;

        [SerializeField] Light pointLight;

        void Start()
        {
            // Trigger = GetComponent<HighlightTrigger>();
            // if (Trigger == null) Debug.LogError("[SaveConsole] No HighlightTrigger found in scene.");
        }

        public override void Interact()
        {
            if (!CanInteract())
            {
                if (currentConsoleState == ActionConsoleState.Broken)
                    AlertEvent.Trigger(
                        AlertReason.BrokenMachine, "The save console is broken and cannot be used.",
                        "Save Console");
                else if (currentConsoleState == ActionConsoleState.LacksPower)
                    AlertEvent.Trigger(
                        AlertReason.MachineLacksPower, "The save console lacks power and cannot be used.",
                        "Save Console");

                return;
            }

            // Capture any data we need for the eventual Accept
            var sceneName = gameObject.scene.name;
            var hasSpawn = spawnPoint != null;
            var info = new SpawnInfo
            {
                SceneName = sceneName,
                SpawnPointId = hasSpawn ? spawnPoint.Id : null,
                Mode = hasSpawn ? spawnPoint.Mode : default
            };

            // Ask the player first
            AlertEvent.Trigger(
                AlertReason.SavingGame,
                "Set this console as your checkpoint and save all current progress?",
                "Save Game?",
                AlertType.ChoiceModal,
                0f,
                onConfirm: () =>
                {
                    // Only set checkpoint if we had a spawnPoint
                    if (hasSpawn)
                        PlayerSpawnManager.Instance.Save(info); // writes checkpoint
                    else
                        Debug.LogWarning(
                            "[SaveConsole] No SpawnPoint on this console; saving without updating checkpoint.");

                    // Perform the global save
                    SaveDataEvent.Trigger();

                    // Optional: toast after success (basic notification)
                    AlertEvent.Trigger(AlertReason.SavingGame, "All data saved successfully!", "Saved Game");
                    MyUIEvent.Trigger(UIType.Any, UIActionType.Close);
                },
                onCancel: () =>
                {
                    // Optional: tiny UX flourish
                    // AlertEvent.Trigger(AlertReason.Test, "Canceled.", "Save");
                }
            );
        }


        public override void OnInteractionStart()
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

        public override void OnInteractionEnd()
        {
        }


        public override string ShortBlurb()
        {
            return "A console that allows you to save your progress.";
        }


        public override string GetActionText()
        {
            return "Save Game";
        }


        protected override string GetActionText(bool recognizableOnSight)
        {
            return "Save Game";
        }
        public override void SetConsoleToLacksPowerState()
        {
            if (pointLight != null)
                pointLight.enabled = false;

            currentConsoleState = ActionConsoleState.LacksPower;
        }
        public override void SetConsoleToPoweredOnState()
        {
            if (pointLight != null)
                pointLight.enabled = true;

            currentConsoleState = ActionConsoleState.PoweredOn;
        }
        public override void SetConsoleToHailPlayerState()
        {
            throw new NotImplementedException();
        }
    }
}
