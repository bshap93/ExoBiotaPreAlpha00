using System;
using System.Collections.Generic;
using Dirigible.Input;
using FirstPersonPlayer;
using FirstPersonPlayer.Interface;
using Helpers.Events;
using Lightbug.CharacterControllerPro.Core;
using Manager;
using MoreMountains.Feedbacks;
using SharedUI.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Interface;

namespace LevelConstruct.Interactable
{
    public class LadderInteractable : MonoBehaviour, IInteractable, IHoverable, IBillboardable, IRequiresUniqueID
    {
        public string uniqueID;
        [SerializeField] MMFeedbacks climbLadderFeedbacks;
        [SerializeField] string actionMessage = "Climb the Ladder?";
        [SerializeField] string actionTitle = "Ladder Climb";

#if UNITY_EDITOR
        [FormerlySerializedAs("ActionId")] [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int actionId;

        [SerializeField] float interactionDistance = 2f;

        CharacterActor _characterActor;
        bool _initialized;
        float _lastTriggerTime;
        GameObject _player01;

        TeleportPlayer _teleportPlayer;
        protected SceneObjectData Data;


        public string GetName()
        {
            return "Access Ladder";
        }
        public Sprite GetIcon()
        {
            return AssetManager.Instance?.iconRepository.ladderIcon;
        }
        public string ShortBlurb()
        {
            return actionMessage;
        }
        public Sprite GetActionIcon()
        {
            return AssetManager.Instance?.iconRepository.climbIcon;
        }
        public string GetActionText()
        {
            return "Climb";
        }
        public bool OnHoverStart(GameObject go)
        {
            var nameToShow = GetName();
            var iconToShow = GetIcon();
            var shortToShow = ShortBlurb();
            var actionIconToShow = GetActionIcon();
            Data = new SceneObjectData(
                nameToShow,
                iconToShow,
                shortToShow,
                actionIconToShow,
                GetActionText()
            );

            BillboardEvent.Trigger(Data, BillboardEventType.Show);
            if (actionId != 0)
                ControlsHelpEvent.Trigger(
                    ControlHelpEventType.Show, actionId, additionalInfoText: "to " + GetActionText()
                );

            return true;
        }
        public bool OnHoverStay(GameObject go)
        {
            // ControlsHelpEvent.Trigger(ControlHelpEventType.ShowIfNothingElseShowing, actionId);
            return true;
        }
        public bool OnHoverEnd(GameObject go)
        {
            if (Data == null) Data = SceneObjectData.Empty();
            BillboardEvent.Trigger(Data, BillboardEventType.Hide);

            if (actionId != 0)
                ControlsHelpEvent.Trigger(
                    ControlHelpEventType.Hide, actionId);

            return true;
        }


        public void Interact()
        {
            Initialize();
            if (_teleportPlayer == null)
            {
                Debug.LogError("LadderInteractable: No TeleportPlayer component found.");
                return;
            }

            climbLadderFeedbacks?.PlayFeedbacks();

            _teleportPlayer.Teleport(_characterActor);
            MyUIEvent.Trigger(UIType.Any, UIActionType.Close);
            // InitiateClimb();
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
            ControlsHelpEvent.Trigger(ControlHelpEventType.Show, actionId);
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
#if UNITY_EDITOR
        public IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            return AllRewiredActions.GetAllRewiredActions();
        }
#endif
        void InitiateClimb()
        {
            AlertEvent.Trigger(
                AlertReason.GateInteractable,
                actionMessage,
                actionTitle,
                AlertType.ChoiceModal,
                0f,
                onConfirm: () => { },
                onCancel: () => { }
            );
        }


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Initialize()
        {
            _characterActor = FindFirstObjectByType<CharacterActor>();
            _teleportPlayer = GetComponent<TeleportPlayer>();
            if (_teleportPlayer == null)
                Debug.LogError("LadderInteractable: No TeleportPlayer component found on the ladder.");

            if (_player01 == null) return;

            _player01 = _characterActor.gameObject;
        }
    }
}
