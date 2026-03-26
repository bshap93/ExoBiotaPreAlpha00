using System;
using System.Collections;
using Events;
using Helpers.Events;
using Helpers.Events.Dialog;
using Helpers.Events.Tutorial;
using LevelConstruct.Highlighting;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Objectives;
using Objectives.ScriptableObjects;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace LevelConstruct.Interactable.ItemInteractables
{
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(HighlightEffectController))]
    [DisallowMultipleComponent]
    public class CommsConsole : ActionConsole, MMEventListener<SpontaneousTriggerEvent>
    {
        [SerializeField] string defaultNPCId;
        [SerializeField] string consoleAutomatedNPCId;
        [FormerlySerializedAs("startNodeOverride")] [SerializeField]
        string defaultStartNode;
        [FormerlySerializedAs("associatedObjectiveIds")] [SerializeField]
        string[] completesObjectives;
        [SerializeField] MMFeedbacks startDialogueFeedback;
        [SerializeField] GameObject rotatingLight;

#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int continuteActionId;

        [Header("Conditional Dialogue Nodes")] [SerializeField]
        DialogueCondition[] dialogueConditions;
        [SerializeField] MMFeedbacks hailinPlayerFeedback;

        ActionConsoleState _priorConsoleState;

        void Start()
        {
            StartCoroutine(InitializeAfterMachineStateManager());
        }

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
        public void OnMMEvent(SpontaneousTriggerEvent eventType)
        {
            if (eventType.UniqueID == uniqueID)
            {
                if (eventType.EventType == SpontaneousTriggerEventType.Triggered)
                {
                    if (currentConsoleState == ActionConsoleState.PoweredOn)
                        SetConsoleToHailPlayerState();
                }
                else if (eventType.EventType == SpontaneousTriggerEventType.Silenced)
                {
                    if (currentConsoleState == ActionConsoleState.HailingPlayer)
                    {
                        if (_priorConsoleState == ActionConsoleState.PoweredOn)
                            SetConsoleToPoweredOnState();
                        else if (_priorConsoleState == ActionConsoleState.LacksPower) SetConsoleToLacksPowerState();
                    }
                }
            }
        }

        public override void Interact()
        {
            if (!CanInteract())
            {
                if (currentConsoleState == ActionConsoleState.Broken)
                    AlertEvent.Trigger(
                        AlertReason.BrokenMachine, "The communications console is broken and cannot be used.",
                        "Comms Console");
                else if (currentConsoleState == ActionConsoleState.LacksPower)
                    AlertEvent.Trigger(
                        AlertReason.MachineLacksPower, "The communications console lacks power and cannot be used.",
                        "Comms Console");

                return;
            }

            foreach (var objId in completesObjectives)
                ObjectiveEvent.Trigger(objId, ObjectiveEventType.ObjectiveCompleted);

            if (currentConsoleState == ActionConsoleState.HailingPlayer ||
                currentConsoleState == ActionConsoleState.PoweredOn)
            {
                var nodeToUse = GetAppropriateStartNode();
                if (nodeToUse.IsNullOrWhitespace())
                    FirstPersonDialogueEvent.Trigger(
                        FirstPersonDialogueEventType.StartDialogue, defaultNPCId, defaultStartNode);
                else
                    FirstPersonDialogueEvent.Trigger(
                        FirstPersonDialogueEventType.StartDialogue, defaultNPCId, nodeToUse);


                if (completesObjectives.Length > 0)
                    foreach (var objId in completesObjectives)
                        ObjectiveEvent.Trigger(objId, ObjectiveEventType.ObjectiveCompleted);

                startDialogueFeedback?.PlayFeedbacks();

                MyUIEvent.Trigger(UIType.Any, UIActionType.Open);
                ControlsHelpEvent.Trigger(ControlHelpEventType.Hide, actionId);
            }


            if (currentConsoleState == ActionConsoleState.HailingPlayer)
            {
                if (_priorConsoleState == ActionConsoleState.PoweredOn)
                    SetConsoleToPoweredOnState();
                else if (_priorConsoleState == ActionConsoleState.LacksPower)
                    throw new Exception(
                        "CommsConsole: Console cannot be in HailingPlayer state if prior state was LacksPower");
            }
        }

        public override void OnInteractionStart()
        {
            MainTutorialBitEvent.Trigger(null, MainTutorialBitEventType.HideOptionalTutorialBit);
        }

        public void InitiateOtherFunctions()
        {
            SpecialDialogueEvent.Trigger(
                SpecialDialogueEventType.RequestSpecialDialogue,
                consoleAutomatedNPCId,
                SpecialDialogueType.MockConsoleDataWindow
            );
        }

        public override void OnInteractionEnd()
        {
        }

        protected override IEnumerator InitializeAfterMachineStateManager()
        {
            yield return base.InitializeAfterMachineStateManager();

            switch (currentConsoleState)
            {
                case ActionConsoleState.Broken:
                case ActionConsoleState.LacksPower:
                case ActionConsoleState.None:
                    SetConsoleToLacksPowerState();
                    break;
                case ActionConsoleState.PoweredOn:
                    SetConsoleToPoweredOnState();
                    break;
            }
        }

        string GetAppropriateStartNode()
        {
            var objectivesManager = ObjectivesManager.Instance;
            if (objectivesManager == null)
            {
                Debug.LogWarning("[CommsConsole] ObjectivesManager not found, using default node");
                return defaultStartNode;
            }

            // Check each condition in order
            if (dialogueConditions != null)
                foreach (var condition in dialogueConditions)
                    if (condition.CheckCondition(objectivesManager))
                        return condition.startNode;

            // Fallback to original override
            return defaultStartNode;
        }

        protected override string GetActionText(bool recognizableOnSight)
        {
            return $"{currentConsoleState}";
        }
        public override void SetConsoleToLacksPowerState()
        {
            if (rotatingLight != null)
                rotatingLight.SetActive(false);

            _priorConsoleState = currentConsoleState;

            currentConsoleState = ActionConsoleState.LacksPower;

            hailinPlayerFeedback?.StopFeedbacks();
        }
        public override void SetConsoleToPoweredOnState()
        {
            if (rotatingLight != null)
                rotatingLight.SetActive(false);

            _priorConsoleState = currentConsoleState;

            currentConsoleState = ActionConsoleState.PoweredOn;

            hailinPlayerFeedback?.StopFeedbacks();
        }
        public override void SetConsoleToHailPlayerState()
        {
            if (rotatingLight != null)
                rotatingLight.SetActive(true);

            _priorConsoleState = currentConsoleState;

            currentConsoleState = ActionConsoleState.HailingPlayer;

            hailinPlayerFeedback?.PlayFeedbacks();
        }
    }
}
