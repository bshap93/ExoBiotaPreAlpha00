using System;
using FirstPersonPlayer.Interface;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.Events;
using Inventory;
using LevelConstruct.Highlighting;
using Manager;
using MoreMountains.Feedbacks;
using MoreMountains.InventoryEngine;
using NewScript;
using Objectives.ScriptableObjects;
using SharedUI.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Interface;

namespace FirstPersonPlayer.Interactable
{
    public class KeyCrateBoxInteractable : MonoBehaviour, IInteractable, IRequiresUniqueID, IHoverable, IBillboardable
    {
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        public string uniqueID;
        [SerializeField] KeyItemObject keyItem;
        [SerializeField] float interactionDistance = 3f;
        [SerializeField] Sprite icon;
        [SerializeField] HighlightEffectController effectController;

#if UNITY_EDITOR
        [ValueDropdown("@AllRewiredActions.GetAllRewiredActions()")]
#endif
        public int actionId;
        [Header("Items inside the crate box")] [SerializeField]
        bool hasOtherItems;
        [ShowIf("hasOtherItems")] [SerializeField]
        MyBaseItem[] items;

        [SerializeField] bool givesMoney;
        [ShowIf("givesMoney")] [SerializeField]
        float moneyAmount;

        public string actionText = "Receive Key Data";


        [Header("Feedbacks")] [SerializeField] MMFeedbacks getKeyItemFeedback;
        [SerializeField] MMFeedbacks alreadyGotKeyFeedback;

        [SerializeField] GameObject holoScreenMesh;
        // [SerializeField] int screenlayer09Index = 5;

        [Header("Objective Options")] [SerializeField]
        InteractableObjectiveModifier.ObjectiveActionType objectiveActionType;
        [SerializeField] ObjectiveObject attachedObjective;

        SceneObjectData _data;

        bool _hasBeenOpened;

        public string GetName()
        {
            return "Key Box";
        }
        public Sprite GetIcon()
        {
            return icon;
        }
        public string ShortBlurb()
        {
            if (!hasOtherItems)
                return "Contains a virtual key.";

            return "Contains a key and $" + items.Length + " other items.";
        }
        public Sprite GetActionIcon()
        {
            return ExaminationManager.Instance.iconRepository.pushIcon;
        }
        public string GetActionText()
        {
            return actionText;
        }
        public bool OnHoverStart(GameObject go)
        {
            _data = new SceneObjectData(
                GetName(), GetIcon(), "Contains a virtual key.", GetActionIcon(), GetActionText());

            if (actionId != 0)
                ControlsHelpEvent.Trigger(
                    ControlHelpEventType.Show, actionId, additionalInfoText:
                    string.IsNullOrEmpty(actionText) ? null : actionText);

            _data.Id = uniqueID;

            BillboardEvent.Trigger(_data, BillboardEventType.Show);

            if (hasOtherItems) throw new NotImplementedException();
            return true;
        }
        public bool OnHoverStay(GameObject go)
        {
            if (hasOtherItems) throw new NotImplementedException();
            return true;
        }
        public bool OnHoverEnd(GameObject go)
        {
            if (_data == null)
                _data = SceneObjectData.Empty();

            BillboardEvent.Trigger(_data, BillboardEventType.Hide);
            if (actionId != 0)
                ControlsHelpEvent.Trigger(
                    ControlHelpEventType.Hide, actionId, string.IsNullOrEmpty(actionText) ? null : actionText);

            if (hasOtherItems) throw new NotImplementedException();
            return true;
        }
        public void Interact()
        {
            if (!_hasBeenOpened)

            {
                MMInventoryEvent.Trigger(
                    MMInventoryEventType.Pick, null,
                    keyItem.TargetInventoryName, keyItem, 1, 0, GlobalInventoryManager.Instance.playerId);

                getKeyItemFeedback?.PlayFeedbacks();

                if (givesMoney) CurrencyEvent.Trigger(CurrencyEventType.AddCurrency, moneyAmount);

                PerformObjectiveAction();
            }
            else
            {
                alreadyGotKeyFeedback?.PlayFeedbacks();
            }

            // TODO: Add other items to inventory if hasOtherItems is true.

            _hasBeenOpened = true;

            effectController.SetSecondaryStateHighlightColor();
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


        void PerformObjectiveAction()
        {
            var objective = attachedObjective;
            var objectiveAction = objectiveActionType;
            if (objective == null) return;

            switch (objectiveAction)
            {
                case InteractableObjectiveModifier.ObjectiveActionType.Add:
                    ObjectiveEvent.Trigger(objective.objectiveId, ObjectiveEventType.ObjectiveAdded);
                    break;
                case InteractableObjectiveModifier.ObjectiveActionType.Activate:
                    ObjectiveEvent.Trigger(objective.objectiveId, ObjectiveEventType.ObjectiveActivated);
                    break;
                case InteractableObjectiveModifier.ObjectiveActionType.Complete:
                    ObjectiveEvent.Trigger(objective.objectiveId, ObjectiveEventType.ObjectiveCompleted);
                    break;
                case InteractableObjectiveModifier.ObjectiveActionType.Deactivate:
                    ObjectiveEvent.Trigger(objective.objectiveId, ObjectiveEventType.ObjectiveDeactivated);
                    break;
                case InteractableObjectiveModifier.ObjectiveActionType.Delete:
                    ObjectiveEvent.Trigger(objective.objectiveId, ObjectiveEventType.ObjectiveDeleted);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
