using System;
using System.Collections.Generic;
using Dirigible.Input;
using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
using FirstPersonPlayer.Interface;
using Helpers.Events;
using Helpers.Events.Dialog;
using Helpers.Events.NPCs;
using Helpers.Events.Progression;
using Lightbug.Utilities;
using Manager;
using Manager.DialogueScene;
using Manager.ProgressionMangers;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Overview.NPC;
using SharedUI.Interface;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FirstPersonPlayer.FPNPCs.AlienNPC
{
    public class SlaverMotile : EnemyController, IInteractable, IHoverable, IBillboardable,
        MMEventListener<DialogueEvent>
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


        [SerializeField] Transform[] tentacleAnchors;
        [SerializeField] Transform[] klaxonAnchors;
        [SerializeField] Transform bodyHeadCenterAnchor;
        [SerializeField] AlienNPCAnimancerController animancerController;

        public HumanoidNPCCreature[] thrallCreatureCharacters;

        public AlienNPCState initialSlaverMotileState;

        [ValueDropdown("GetNpcIdOptions")] public
            string npcId;

        [Header("Flags")] [SerializeField] bool isInteractable = true;


        public bool initialDesiresDialogue;

        [SerializeField] NpcDefinition npcDefinition;
        [SerializeField] string defaultStartNode;

        [SerializeField] int exobioticLanguageThreshold = 2;

#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int actionId;

        SceneObjectData _sceneObjectData;
        Transform _dialogueFocusPoint;
        MMFeedbacks _startDialogueFeedback;

        public AlienNPCState CurrentState { get; private set; }

        public bool Alerted { get; private set; }

        public bool Hostile { get; private set; }


        public bool DesiresDialogue { get; private set; }

        protected override void Start()
        {
            base.Start();

            DesiresDialogue = initialDesiresDialogue;

            SetState(animancerController.CurrentState);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            this.MMEventStartListening<DialogueEvent>();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            this.MMEventStopListening<DialogueEvent>();
        }
        public string GetName()
        {
            return npcDefinition != null ? npcDefinition.characterName : "NPC";
        }
        public Sprite GetIcon()
        {
            return npcDefinition != null ? npcDefinition.characterIcon : null;
        }
        public string ShortBlurb()
        {
            return npcDefinition != null ? npcDefinition.npcDescription : string.Empty;
        }
        public Sprite GetActionIcon()
        {
            return PlayerUIManager.Instance.defaultIconRepository.talkIcon;
        }
        public string GetActionText()
        {
            return "Talk";
        }
        public bool OnHoverStart(GameObject go)
        {
            _sceneObjectData = SceneObjectData.Empty();

            _sceneObjectData.ActionIcon = GetActionIcon();
            _sceneObjectData.ActionText = GetActionText();
            _sceneObjectData.Name = GetName();
            _sceneObjectData.ShortBlurb = ShortBlurb();
            _sceneObjectData.Icon = GetIcon();

            BillboardEvent.Trigger(_sceneObjectData, BillboardEventType.Show);

            if (actionId != 0) ControlsHelpEvent.Trigger(ControlHelpEventType.Show, actionId);

            return true;
        }
        public bool OnHoverStay(GameObject go)
        {
            return true;
        }
        public bool OnHoverEnd(GameObject go)
        {
            if (_sceneObjectData == null) _sceneObjectData = SceneObjectData.Empty();

            BillboardEvent.Trigger(_sceneObjectData, BillboardEventType.Hide);
            if (actionId != 0) ControlsHelpEvent.Trigger(ControlHelpEventType.Hide, actionId);

            return true;
        }

        public void Interact()
        {
            if (!CanInteract()) return;

            DetermineLanguage();

            var nodeToUse = GetAppropriateDialogueNode();

            StartDialogue(nodeToUse);
        }
        public void Interact(string param)
        {
            if (!CanInteract()) return;

            DetermineLanguage();

            StartDialogue(param);
        }
        public void OnInteractionStart()
        {
        }

        public void OnInteractionEnd(string param)
        {
            // Release camera focus when dialogue ends
            DialogueCameraEvent.Trigger(DialogueCameraEventType.ReleaseFocus);
        }
        public bool CanInteract()
        {
            if (CurrentState == AlienNPCState.Unavailable) return false;
            // if (CurrentState == AlienNPCState.InDialogue) return false;
            if (!isInteractable) return false;
            return true;
        }
        public bool IsInteractable()
        {
            return isInteractable;
        }
        public void OnFocus()
        {
        }
        public void OnUnfocus()
        {
        }
        public float GetInteractionDistance()
        {
            return 5.5f;
        }
        public void OnMMEvent(DialogueEvent eventType)
        {
            if (eventType.EventType == DialogueEventType.DialogueFinished)
            {
                DesiresDialogue = false;
                SetState(AlienNPCState.Working);
            }
        }


        void StartDialogue(string nodeToUse)
        {
            if (nodeToUse.IsNullOrWhiteSpace())
                FirstPersonDialogueEvent.Trigger(FirstPersonDialogueEventType.StartDialogue, npcId, defaultStartNode);
            else
                FirstPersonDialogueEvent.Trigger(FirstPersonDialogueEventType.StartDialogue, npcId, nodeToUse);

            var friendlyNPCManager = FriendlyNPCManager.Instance;
            if (friendlyNPCManager != null && !friendlyNPCManager.HasNPCBeenContactedAtLeastOnce(npcDefinition.npcId))
                EnemyXPRewardEvent.Trigger(npcDefinition.xpForFirstMeeting);

            _dialogueFocusPoint = bodyHeadCenterAnchor;

            // Focus the dialogue camera on this NPC
            var focusTarget = _dialogueFocusPoint != null ? _dialogueFocusPoint : transform;
            DialogueCameraEvent.Trigger(DialogueCameraEventType.FocusOnTarget, focusTarget);

            _startDialogueFeedback?.PlayFeedbacks();
            MyUIEvent.Trigger(UIType.Any, UIActionType.Open);
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

        public void SetState(AlienNPCState newState)
        {
            CurrentState = newState;

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
                    foreach (var thrall in thrallCreatureCharacters)
                        thrall.SetState(AlienNPCState.Searching, true);

                    break;
                default:
                    Debug.LogWarning($"Unimplemented BroadcastType {broadcastType}");
                    break;
            }
        }

        protected string GetAppropriateDialogueNode()
        {
            // For now, just return the default start node.
            return defaultStartNode;
        }
        static string[] GetNpcIdOptions()
        {
            return DialogueManager.GetAllNpcIdOptions();
        }

        void DetermineLanguage()
        {
            var attributeMgr = AttributesManager.Instance;
            if (attributeMgr == null)
            {
                Debug.LogError("AttributesManager instance not found.");
                return;
            }

            var exobioticAttrLevel = attributeMgr.Exobiotic;


            if (npcDefinition.nativeLanguage == LanguageType.ModernGalactic)
                DialoguePresentationEvent.Trigger(
                    DialoguePresentationEventType.ChangeFontsOfNPCSide, LanguageType.ModernGalactic);
            else if (npcDefinition.nativeLanguage == LanguageType.Sheolite)
                if (exobioticAttrLevel >= exobioticLanguageThreshold)
                    DialoguePresentationEvent.Trigger(
                        DialoguePresentationEventType.ChangeFontsOfNPCSide, LanguageType.ModernGalactic);
                else
                    DialoguePresentationEvent.Trigger(
                        DialoguePresentationEventType.ChangeFontsOfNPCSide, LanguageType.Sheolite);
        }

#if UNITY_EDITOR
        public IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            return AllRewiredActions.GetAllRewiredActions();
        }

        void OnDrawGizmosSelected()
        {
            var target = _dialogueFocusPoint != null ? _dialogueFocusPoint.position : transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(target, 0.08f);
            Gizmos.DrawLine(transform.position, target);
        }

#endif
    }
}
