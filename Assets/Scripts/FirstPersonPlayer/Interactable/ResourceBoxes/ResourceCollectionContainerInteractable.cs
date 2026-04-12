using System;
using System.Collections;
using FirstPersonPlayer.Interface;
using Helpers.Events;
using Helpers.Events.Machinery;
using LevelConstruct.Highlighting;
using Manager;
using Manager.StateManager;
using MoreMountains.Feedbacks;
using SharedUI.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Interface;

namespace FirstPersonPlayer.Interactable.ResourceBoxes
{
    public class ResourceCollectionContainerInteractable : MonoBehaviour, IInteractable, IRequiresUniqueID, IHoverable,
        IBillboardable
    {
        public enum ResourceType
        {
            Neumat,
            Scrap
        }


        public string uniqueID;
        [SerializeField] ResourceType resourceType;
        [SerializeField] float interactionDistance = 3f;
        [SerializeField] Sprite icon;
        [SerializeField] HighlightEffectController effectController;
        [SerializeField] float resourceAmount;

        public ResourceContainerManager.ResourceContainerInitializationState initialContainerState;

        public string actionText = "Collect Resource";

        [Header("Feedbacks")] [SerializeField] MMFeedbacks getResourceFeedback;
        [SerializeField] MMFeedbacks alreadyGotResourceFeedback;
#if UNITY_EDITOR
        [ValueDropdown("@AllRewiredActions.GetAllRewiredActions()")]
#endif
        public int actionId;

        SceneObjectData _data;

        bool _hasBeenDepleted;
        void Start()
        {
            StartCoroutine(InitializeAfterBarrierStateManager());
        }
        public string GetName()
        {
            switch (resourceType)
            {
                case ResourceType.Neumat:
                    return "Neumat";
                case ResourceType.Scrap:
                    return "Scrap";
            }

            return "";
        }
        public Sprite GetIcon()
        {
            switch (resourceType)
            {
                case ResourceType.Neumat:
                    return ExaminationManager.Instance.iconRepository.neumatIcon;
                case ResourceType.Scrap:
                    return ExaminationManager.Instance.iconRepository.scrapIcon;
            }

            return null;
        }
        public string ShortBlurb()
        {
            if (!_hasBeenDepleted)
                switch (resourceType)
                {
                    case ResourceType.Neumat:
                        return "Contains " + resourceAmount + " units of Neumat.";
                    case ResourceType.Scrap:
                        return "Contains " + resourceAmount + " units of Scrap.";
                }
            else
                return "Depleted.";

            return "";
        }
        public Sprite GetActionIcon()
        {
            return ExaminationManager.Instance.iconRepository.getResourceAction;
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

            return true;
        }
        public bool OnHoverStay(GameObject go)
        {
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

            return true;
        }
        public void Interact()
        {
            if (!_hasBeenDepleted)
            {
                getResourceFeedback?.PlayFeedbacks();

                CurrencyEvent.Trigger(CurrencyEventType.AddCurrency, resourceAmount, resourceType);
                
                ResourceContainerInitStateEvent.Trigger(
                    ResourceContainerStateEventType.SetNewResourceContainerState, resourceType,
                    ResourceContainerManager.ResourceContainerInitializationState.IsDepleted, uniqueID);
            }
            else
            {
                alreadyGotResourceFeedback?.PlayFeedbacks();
            }

            _hasBeenDepleted = true;

            effectController.SetSecondaryStateHighlightColor();
        }
        public void Interact(string param)
        {
            Interact();
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

        IEnumerator InitializeAfterBarrierStateManager()
        {
            yield return null;

            var resourceContainerManager = ResourceContainerManager.Instance;
            if (resourceContainerManager != null)
            {
                var resourceContainerState = resourceContainerManager.GetContainerState(uniqueID);
                if (resourceContainerState == ResourceContainerManager.ResourceContainerInitializationState.None)
                    resourceContainerState = initialContainerState;

                if (resourceContainerState == ResourceContainerManager.ResourceContainerInitializationState.IsDepleted)
                    SetUsedOrDepleted();
                else if (resourceContainerState ==
                         ResourceContainerManager.ResourceContainerInitializationState.ShouldBeDestroyed)
                    Destroy(gameObject);
            }
        }

        void SetUsedOrDepleted()
        {
            _hasBeenDepleted = true;

            effectController.SetSecondaryStateHighlightColor();
        }
    }
}
