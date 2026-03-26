using System.Collections.Generic;
using Dirigible.Input;
using Events;
using FirstPersonPlayer;
using FirstPersonPlayer.Interactable.Stateful;
using FirstPersonPlayer.Interface;
using Helpers.Events;
using HighlightPlus;
using Manager;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using SharedUI.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace CustomAssets.Scripts
{
    public class ButtonClickAnim : MonoBehaviour, MMEventListener<SceneEvent>, IInteractable, IHoverable, IBillboardable
    {
        public enum ElevatorButtonType
        {
            CallToBottom,
            CallToTop,
            CallToCurrent,
            ElevatorGoDown,
            ElevatorGoUp
        }
#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int actionId;

        public ElevatorButtonType buttonType;

        [SerializeField] float interactionDistance = 2f;


        [SerializeField] string associatedObjectiveID;

        [SerializeField] bool overrideInteractionRange;
        // [SerializeField] float interactionRange = 2f;
        [SerializeField] HighlightEffect highlightEffect;
        [SerializeField] MMFeedbacks buttonPressFeedback;


        [FormerlySerializedAs("ButtonObject")] public GameObject buttonObject;
        [FormerlySerializedAs("PushMove")] public float pushMove = -0.0025f;
        public string buttonName;


        public RewiredFirstPersonInputs playerInput;

        public string shortToShow;

        public StatefulElevator linkedElevator;

        SceneObjectData _data;

        float _defaultY;

        void Start()
        {
            if (highlightEffect == null) highlightEffect = GetComponent<HighlightEffect>();
        }

        // Update is called once per frame
        void Update()
        {
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }
        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public string GetName()
        {
            return buttonName != null ? buttonName : "Button";
        }
        public Sprite GetIcon()
        {
            return ExaminationManager.Instance.iconRepository.buttonIcon;
        }
        public string ShortBlurb()
        {
            return shortToShow != null ? shortToShow : "A button that can be pressed.";
        }
        public Sprite GetActionIcon()
        {
            return ExaminationManager.Instance.iconRepository.pushIcon;
        }
        public string GetActionText()
        {
            return "Push";
        }
        public bool OnHoverStart(GameObject go)
        {
            var nameToShow = buttonName != null ? buttonName : "Button";
            var iconToShow = ExaminationManager.Instance.iconRepository.buttonIcon;
            var icon = ExaminationManager.Instance.iconRepository.pushIcon;

            _data = new SceneObjectData(nameToShow, iconToShow, shortToShow, icon, "Push");

            _data.Id = GetInstanceID().ToString();
            BillboardEvent.Trigger(_data, BillboardEventType.Show);
            if (actionId != 0)
                ControlsHelpEvent.Trigger(
                    ControlHelpEventType.Show, actionId
                );

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
                    ControlHelpEventType.Hide, actionId);

            return true;
        }


        public void Interact()
        {
            if (!CanInteract()) return;
            if (playerInput == null) return;
            if (linkedElevator == null) return;

            linkedElevator.OnButtonClick(this);
            highlightEffect.HitFX();
            buttonPressFeedback?.PlayFeedbacks();


            if (!string.IsNullOrEmpty(associatedObjectiveID))
                ObjectiveEvent.Trigger(
                    associatedObjectiveID, ObjectiveEventType.ObjectiveCompleted);
        }
        public void Interact(string param)
        {
            throw new System.NotImplementedException();
        }

        public void OnInteractionStart()
        {
        }
        public void OnInteractionEnd(string param)
        {
        }
        public bool CanInteract()
        {
            if (linkedElevator == null) return true;

            var isMoving = linkedElevator.IsMoving();

            if (isMoving)
                AlertEvent.Trigger(
                    AlertReason.InvalidAction, "The elevator is currently moving.", "Cannot Interact");

            // Prevent interaction while elevator is moving
            return !linkedElevator.IsMoving();
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

        public void OnMMEvent(SceneEvent eventType)
        {
            if (eventType.EventType == SceneEventType.PlayerPawnLoaded) Initialize();
        }

        public void OnInteractionEnd()
        {
        }
#if UNITY_EDITOR
        public IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            return AllRewiredActions.GetAllRewiredActions();
        }

#endif

        void Initialize()
        {
            playerInput = FindFirstObjectByType<RewiredFirstPersonInputs>();

            if (playerInput == null) Debug.LogError("No RewiredFirstPersonInputs found in scene.");
        }
    }
}
