using System;
using System.Collections.Generic;
using Dirigible.Input;
using FirstPersonPlayer.Interface;
using Helpers.Events;
using Helpers.Events.Dialog;
using Lightbug.Utilities;
using Manager;
using Manager.DialogueScene;
using MoreMountains.Feedbacks;
using SharedUI.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Interface;

namespace FirstPersonPlayer.Interactable.HoloInteractable
{
    public abstract class DialogueInteractable : MonoBehaviour, IInteractable, IRequiresUniqueID, IBillboardable,
        IHoverable
    {
        [ValueDropdown("GetNpcIdOptions")] public
            string npcId;

        [SerializeField] protected MMFeedbacks startDialogueFeedback;

        [SerializeField] protected string defaultStartNode;

        [SerializeField] protected string nodeToUse;

        [SerializeField] protected string uniqueID;

        [SerializeField] protected float interactionDistance = 3.3f;

#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int actionId;

        protected SceneObjectData SceneObjectData;
        public abstract string GetName();
        public abstract Sprite GetIcon();

        public virtual string ShortBlurb()
        {
            return "N/A";
        }
        public virtual Sprite GetActionIcon()
        {
            return PlayerUIManager.Instance.defaultIconRepository.interactIcon;
        }
        public virtual string GetActionText()
        {
            return "Access";
        }
        public virtual bool OnHoverStart(GameObject go)
        {
            SceneObjectData = SceneObjectData.Empty();

            SceneObjectData.ActionIcon = GetActionIcon();
            SceneObjectData.ActionText = GetActionText();
            SceneObjectData.Name = GetName();
            SceneObjectData.ShortBlurb = ShortBlurb();
            SceneObjectData.Icon = GetIcon();

            BillboardEvent.Trigger(SceneObjectData, BillboardEventType.Show);


            if (actionId != 0) ControlsHelpEvent.Trigger(ControlHelpEventType.Show, actionId);

            return true;
        }
        public virtual bool OnHoverStay(GameObject go)
        {
            return true;
        }
        public virtual bool OnHoverEnd(GameObject go)
        {
            if (SceneObjectData == null) SceneObjectData = SceneObjectData.Empty();

            BillboardEvent.Trigger(SceneObjectData, BillboardEventType.Hide);
            if (actionId != 0) ControlsHelpEvent.Trigger(ControlHelpEventType.Hide, actionId);

            return true;
        }

        public virtual void Interact()
        {
            if (!CanInteract()) return;

            if (nodeToUse.IsNullOrWhiteSpace())
                FirstPersonDialogueEvent.Trigger(FirstPersonDialogueEventType.StartDialogue, npcId, defaultStartNode);
            else
                FirstPersonDialogueEvent.Trigger(FirstPersonDialogueEventType.StartDialogue, npcId, nodeToUse);

            startDialogueFeedback?.PlayFeedbacks();

            MyUIEvent.Trigger(UIType.Any, UIActionType.Open);
        }
        public virtual void Interact(string param)
        {
            Interact();
        }
        public virtual void OnInteractionStart()
        {
        }
        public virtual void OnInteractionEnd(string param)
        {
        }
        public virtual bool CanInteract()
        {
            return true;
        }
        public virtual bool IsInteractable()
        {
            return true;
        }
        public virtual void OnFocus()
        {
        }
        public virtual void OnUnfocus()
        {
        }
        public virtual float GetInteractionDistance()
        {
            return interactionDistance;
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

        static string[] GetNpcIdOptions()
        {
            return DialogueManager.GetAllNpcIdOptions();
        }

#if UNITY_EDITOR
        public IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            return AllRewiredActions.GetAllRewiredActions();
        }

#endif
    }
}
