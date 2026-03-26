using System;
using System.Collections.Generic;
using Dirigible.Input;
using FirstPersonPlayer.Interface;
using Helpers.Events;
using Helpers.Events.Dialog;
using Helpers.Events.NPCs;
using Helpers.Events.Progression;
using MoreMountains.Feedbacks;
using Overview.NPC;
using SharedUI.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Interface;

namespace FirstPersonPlayer.FPNPCs
{
    public class WombKeeperNPC : MonoBehaviour, IRequiresUniqueID, IInteractable, IBillboardable, IHoverable
    {
        public string uniqueID;
        [Header("NPC Definition")] public NpcDefinition npcDefinition;
        public string nodeToUse;

        [Header("Dialogue Camera")]
        [Tooltip(
            "Transform the dialogue camera will look at during conversation. " +
            "Drag a child bone/empty here (e.g. head or chest). " +
            "If left null, the NPC's root transform is used as a fallback.")]
        [SerializeField]
        Transform dialogueFocusPoint;
        [Header("Feedbacks")] [SerializeField] MMFeedbacks startDialogueFeedback;
#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int actionId;

        SceneObjectData _sceneObjectData;

#if UNITY_EDITOR
        /// Draws a gizmo so you can visually confirm focus point placement in the editor.
        void OnDrawGizmosSelected()
        {
            var target = dialogueFocusPoint != null ? dialogueFocusPoint.position : transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(target, 0.08f);
            Gizmos.DrawLine(transform.position, target);
        }
#endif
        public string GetName()
        {
            return npcDefinition.characterName;
        }
        public Sprite GetIcon()
        {
            if (npcDefinition.characterIcon == null)
            {
                Debug.LogWarning($"NPC {npcDefinition.characterName} does not have a character icon assigned.");
                return null;
            }

            return npcDefinition.characterIcon;
        }
        public string ShortBlurb()
        {
            if (npcDefinition.npcDescription == null)
            {
                Debug.LogWarning($"NPC {npcDefinition.characterName} does not have a description assigned.");
                return "";
            }

            return npcDefinition.npcDescription;
        }
        public Sprite GetActionIcon()
        {
            return null;
        }
        public string GetActionText()
        {
            return "Talk to";
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
            FirstPersonDialogueEvent.Trigger(
                FirstPersonDialogueEventType.StartDialogue, npcDefinition.npcId, nodeToUse);

            var friendlyNPCManager = FriendlyNPCManager.Instance;
            if (friendlyNPCManager != null && !friendlyNPCManager.HasNPCBeenContactedAtLeastOnce(npcDefinition.npcId))
                EnemyXPRewardEvent.Trigger(npcDefinition.xpForFirstMeeting);

            // Focus the dialogue camera on this NPC
            var focusTarget = dialogueFocusPoint != null ? dialogueFocusPoint : transform;
            DialogueCameraEvent.Trigger(DialogueCameraEventType.FocusOnTarget, focusTarget);


            startDialogueFeedback?.PlayFeedbacks();
            MyUIEvent.Trigger(UIType.Any, UIActionType.Open);
        }
        public void Interact(string param)
        {
            throw new NotImplementedException();
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
            return true;
        }
        public bool IsInteractable()
        {
            return true;
        }
        public void OnFocus()
        {
        }
        public void OnUnfocus()
        {
        }
        public float GetInteractionDistance()
        {
            return 6f;
        }
        public string UniqueID => uniqueID;
        public void SetUniqueID()
        {
            uniqueID = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(uniqueID);
        }

#if UNITY_EDITOR
        public IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            return AllRewiredActions.GetAllRewiredActions();
        }

#endif
    }
}
