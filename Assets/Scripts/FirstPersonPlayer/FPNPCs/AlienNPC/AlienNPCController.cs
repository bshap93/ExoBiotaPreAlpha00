using System;
using System.Collections.Generic;
using Dirigible.Input;
using FirstPersonPlayer.Combat.Player.ScriptableObjects;
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
using Overview.NPC;
using SharedUI.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.FPNPCs.AlienNPC
{
    // public enum AlienNPCState
    // {
    //     Hailable,
    //     InDialogue,
    //     Unavailable
    // }

    [Serializable]
    public enum AlienNPCState
    {
        Working,
        Idling,
        Searching,
        Dead,
        Hibernating,
        FriendlyAndHailable,
        InDialogue,
        Unavailable,
        Pursuing,
        CombatInRange,
        Watching
    }


    public class AlienNPCController : CreatureController, IInteractable, IHoverable, IBillboardable
    {
        [FormerlySerializedAs("NPCId")] [ValueDropdown("GetNpcIdOptions")]
        public
            string npcId;

        [SerializeField] float interactDistanceOverride = 5f;
        [SerializeField] int exobioticLanguageThreshold = 2;

        [SerializeField] string defaultStartNode;
        [SerializeField] MMFeedbacks startDialogueFeedback;
        [SerializeField] AlienNPCState initialState = AlienNPCState.FriendlyAndHailable;
        [SerializeField] bool isInteractable = true;
        [SerializeField] NpcDefinition npcDefinition;

        [Header("Dialogue Camera")] [SerializeField]
        Transform dialogueFocusPoint;

        [Header("Controls Help & Action Info")]
#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int actionId;

        SceneObjectData _sceneObjectData;
        protected AlienNPCState CurrentState;
        protected override void Start()
        {
            base.Start();
            CurrentState = initialState;
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
            // For now, just return a generic talk icon. This can be expanded in the future to return different icons based on the NPC's state or other factors.
            return PlayerUIManager.Instance.defaultIconRepository.talkIcon;
        }
        public string GetActionText()
        {
            // For now, just return "Talk". This can be expanded in the future to return different action texts based on the NPC's state or other factors.
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
        public float GetInteractionDistance()
        {
            return interactDistanceOverride;
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
            if (CurrentState == AlienNPCState.InDialogue) return false;
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
        void StartDialogue(string nodeToUse)
        {
            if (nodeToUse.IsNullOrWhiteSpace())
                FirstPersonDialogueEvent.Trigger(FirstPersonDialogueEventType.StartDialogue, npcId, defaultStartNode);
            else
                FirstPersonDialogueEvent.Trigger(FirstPersonDialogueEventType.StartDialogue, npcId, nodeToUse);

            var friendlyNPCManager = FriendlyNPCManager.Instance;
            if (friendlyNPCManager != null && !friendlyNPCManager.HasNPCBeenContactedAtLeastOnce(npcDefinition.npcId))
                EnemyXPRewardEvent.Trigger(npcDefinition.xpForFirstMeeting);

            // Focus the dialogue camera on this NPC
            var focusTarget = dialogueFocusPoint != null ? dialogueFocusPoint : transform;
            DialogueCameraEvent.Trigger(DialogueCameraEventType.FocusOnTarget, focusTarget);

            startDialogueFeedback?.PlayFeedbacks();
            MyUIEvent.Trigger(UIType.Any, UIActionType.Open);
        }

        public override void ProcessAttackDamage(PlayerAttack playerAttack, Vector3 attackOrigin)
        {
            if (cannotBeAttacked) return;

            base.ProcessAttackDamage(playerAttack, attackOrigin);
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


#if UNITY_EDITOR
        public IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            return AllRewiredActions.GetAllRewiredActions();
        }

        void OnDrawGizmosSelected()
        {
            var target = dialogueFocusPoint != null ? dialogueFocusPoint.position : transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(target, 0.08f);
            Gizmos.DrawLine(transform.position, target);
        }

#endif
    }
}
