using System;
using System.Collections.Generic;
using DG.Tweening;
using Dirigible.Input;
using FirstPersonPlayer.Interface;
using Helpers.Events;
using HighlightPlus;
using Manager;
using MoreMountains.Feedbacks;
using SharedUI.Interface;
using Sirenix.OdinInspector;
using Unity.AI.Navigation;
using UnityEngine;
using Utilities.Interface;

namespace FirstPersonPlayer.Interactable.Doors
{
    public class GenericDoor : MonoBehaviour, IInteractable, IRequiresUniqueID, IBillboardable, IHoverable
    {
        public enum DoorType
        {
            Cabinet,
            Lootbox,
            SlidingDoor,
            HingedDoor
        }

        public string uniqueID;
        [Header("Rotation Settings")] [SerializeField]
        protected
            GameObject doorModel;
        [SerializeField] bool useRotationChange = true;
        [ShowIf("useRotationChange")] [SerializeField]
        protected
            Vector3 openRotation;
        [ShowIf("useRotationChange")] [SerializeField]
        protected
            Vector3 closedRotation;

        [SerializeField] protected bool isSecretDoor;

#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int actionId;

        [SerializeField] protected DoorType doorType;

        [SerializeField] protected string doorName;

        [Header("Position Settings")] [SerializeField]
        bool usePositionChange;
        [ShowIf("usePositionChange")] [SerializeField]
        protected
            Vector3 openPosition;
        [ShowIf("usePositionChange")] [SerializeField]
        protected
            Vector3 closedPosition;

        [SerializeField] protected NavMeshLink navMeshLink;


        [SerializeField] protected bool startOpen;

        [Header("Feedbacks")] [SerializeField] protected MMFeedbacks openFeedback;
        [SerializeField] protected MMFeedbacks closeFeedback;

        [Header("Highlighting")] [SerializeField]
        protected
            HighlightEffect proximityHighlightEffect;
        [SerializeField] bool shouldDisableHighlightOnInteraction = true;

        [Header("Settings")] [SerializeField] protected float openCloseDuration = 1f;

        [SerializeField] protected float distanceToInteract = 3f;

        [ShowIf("shouldDisableColliderOnInteraction")] [SerializeField]
        protected Collider[] interactionCollider;
        [SerializeField] bool shouldDisableColliderOnInteraction = true;

        bool _isOpen;


        SceneObjectData _sceneObjectData;
        void Start()
        {
            _isOpen = startOpen;
            if (navMeshLink != null)
                navMeshLink.enabled = _isOpen;
        }
        public string GetName()
        {
            if (!string.IsNullOrEmpty(doorName))
                return doorName;

            switch (doorType)
            {
                case DoorType.Cabinet:
                    return "Cabinet";
                case DoorType.Lootbox:
                    return "Lootbox";
                case DoorType.SlidingDoor:
                    return "Sliding Door";
                case DoorType.HingedDoor:
                    return "Hinged Door";
                default:
                    return "Door";
            }
        }
        public Sprite GetIcon()
        {
            switch (doorType)
            {
                case DoorType.Cabinet:
                    return PlayerUIManager.Instance.defaultIconRepository.cabinetIcon;
                case DoorType.Lootbox:
                    return PlayerUIManager.Instance.defaultIconRepository.chestIcon;
                case DoorType.SlidingDoor:
                    return PlayerUIManager.Instance.defaultIconRepository.slidingDoorIcon;
                case DoorType.HingedDoor:
                    return PlayerUIManager.Instance.defaultIconRepository.hingedDoorIcon;
                default:
                    return PlayerUIManager.Instance.defaultIconRepository.doorIcon;
            }
        }
        public string ShortBlurb()
        {
            return "N/A";
        }
        public Sprite GetActionIcon()
        {
            return PlayerUIManager.Instance.defaultIconRepository.interactIcon;
        }
        public string GetActionText()
        {
            return _isOpen ? "Close" : "Open";
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
        public virtual void Interact()
        {
            ToggleDoor();
            if (interactionCollider != null && shouldDisableColliderOnInteraction)
                foreach (var col in interactionCollider)
                    col.enabled = false;

            if (proximityHighlightEffect != null && shouldDisableHighlightOnInteraction)
                proximityHighlightEffect.enabled = false;
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
            return distanceToInteract;
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

        public void ToggleDoor()
        {
            if (_isOpen)
                CloseDoor();
            else
                OpenDoor();
        }

        public void OpenDoor()
        {
            openFeedback?.PlayFeedbacks();
            if (useRotationChange)
                doorModel.transform.DOLocalRotate(openRotation, openCloseDuration);

            if (usePositionChange)
                doorModel.transform.DOLocalMove(openPosition, openCloseDuration);

            _isOpen = true;
            if (navMeshLink != null)
                navMeshLink.enabled = true;
        }

        public void CloseDoor()
        {
            closeFeedback?.PlayFeedbacks();
            if (useRotationChange)
                doorModel.transform.DOLocalRotate(closedRotation, openCloseDuration);

            if (usePositionChange)
                doorModel.transform.DOLocalMove(closedPosition, openCloseDuration);

            _isOpen = false;
            if (navMeshLink != null)
                navMeshLink.enabled = false;
        }
    }
}
