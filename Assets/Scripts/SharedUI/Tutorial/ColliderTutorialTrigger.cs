using System;
using System.Collections.Generic;
using Events;
using Helpers.Events;
using Helpers.Events.Triggering;
using Helpers.Events.Tutorial;
using Helpers.ScriptableObjects.Tutorial;
using Manager;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Objectives.ScriptableObjects;
using PhysicsHandlers.Triggers;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Interface;

namespace SharedUI.Tutorial
{
    public class ColliderTutorialTrigger : MonoBehaviour, IRequiresUniqueID, MMEventListener<SpontaneousTriggerEvent>
    {
        public enum TutorialType
        {
            MainTutorialBit,
            ControlPromptSequence,
            None
        }
#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int ActionId;

        public bool OfferOptionalTutorialBit;

        [SerializeField] ObjectiveObject objectiveToStartOnBoop;

        [SerializeField] MMFeedbacks newControlPromptFeedbacks;


        [FormerlySerializedAs("tutorialBitID")] [SerializeField]
        MainTutBitWindowArgs tutorialBit;
        [SerializeField] TriggerType triggerType = TriggerType.OnEnter;
        [SerializeField] TutorialType tutorialType;

        [Header("Unique ID")] public string uniqueID;

        [Header("Control Prompt Overrides")] public string prePromptTextOverride;
        public string postPromptTextOverride;

        [Header("Boop Settings")] public bool canBeBooped;
        [Header("Objective Settings")] public bool ifNotBoopedStartObjectiveOnLeave;
        public bool objectiveToBecomeActive = true;
        [Header("Trigger Settings")] public bool setNotTriggerableOnExit;
        public bool startDisabled;
        bool _isActionButtonPressed;

        bool _isDisabled;
        bool _isPlayerInTrigger;


        Player _player;
        TriggerColliderManager _triggerColliderManager;

        void Start()
        {
            _player = ReInput.players.GetPlayer(0);

            _triggerColliderManager = TriggerColliderManager.Instance;
            if (_triggerColliderManager == null)
                Debug.LogWarning("ColliderTutorialTrigger: No TriggerColliderManager found in scene.", this);

            _isDisabled = startDisabled;
        }

        void Update()
        {
            if (_player == null) return;

            if (_isDisabled) return;

            if (_triggerColliderManager)
                if (!_triggerColliderManager.IsTutorialColliderTriggerable(uniqueID))
                    return;

            if (canBeBooped && _isPlayerInTrigger)
            {
                var isButtonPressed = _player.GetButton(ActionId);
                if (isButtonPressed)
                {
                    ControlsHelpEvent.Trigger(ControlHelpEventType.ShowUseThenHide, ActionId);
                    canBeBooped = false;
                    TriggerColliderEvent.Trigger(
                        uniqueID, TriggerColliderEventType.SetTriggerable, false, TriggerColliderType.Tutorial);

                    if (tutorialBit != null)
                        MainTutorialBitEvent.Trigger(
                            tutorialBit.mainTutID,
                            MainTutorialBitEventType.ClearTutorialColliderTrigger, tutorialBit.tutBitName);

                    TryAddAndActivateObjective();
                }
            }
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        void OnTriggerEnter(Collider other)
        {
            if (TutorialManager.Instance == null) return;
            // if (!TutorialManager.Instance.AreTutorialsEnabled()) return;
            if (_isDisabled) return;
            if (_triggerColliderManager)
                if (!_triggerColliderManager.IsTutorialColliderTriggerable(uniqueID))
                    return;

            if (tutorialType == TutorialType.MainTutorialBit)
            {
                if (other.CompareTag("FirstPersonPlayer") || other.CompareTag("Player"))
                {
                    _isPlayerInTrigger = true;
                    if (triggerType != TriggerType.OnEnter) return;
                    if (string.IsNullOrEmpty(tutorialBit.mainTutID))
                    {
                        Debug.LogWarning("ColliderTutorialTrigger: No tutorialBitID assigned.", this);
                        return;
                    }

                    if (TutorialManager.Instance == null) return;

                    if (TutorialManager.Instance.IsTutorialBitComplete(tutorialBit.mainTutID)) return;

                    MainTutorialBitEvent.Trigger(tutorialBit.mainTutID, MainTutorialBitEventType.ShowMainTutBit);
                    MyUIEvent.Trigger(UIType.TutorialWindow, UIActionType.Open);
                }
            }
            else if (tutorialType == TutorialType.ControlPromptSequence)
            {
                if (other.CompareTag("FirstPersonPlayer") || other.CompareTag("Player"))
                {
                    _isPlayerInTrigger = true;
                    if (!string.IsNullOrEmpty(prePromptTextOverride) && !string.IsNullOrEmpty(postPromptTextOverride))
                        ControlsHelpEvent.Trigger(
                            ControlHelpEventType.Show, ActionId, prePromptTextOverride, null, postPromptTextOverride);
                    else if (!string.IsNullOrEmpty(prePromptTextOverride))
                        ControlsHelpEvent.Trigger(
                            ControlHelpEventType.Show, ActionId, prePromptTextOverride);
                    else if (!string.IsNullOrEmpty(postPromptTextOverride))
                        ControlsHelpEvent.Trigger(
                            ControlHelpEventType.Show, ActionId, null, null, postPromptTextOverride);
                    else
                        ControlsHelpEvent.Trigger(
                            ControlHelpEventType.Show, ActionId);

                    if (OfferOptionalTutorialBit)
                        MainTutorialBitEvent.Trigger(
                            tutorialBit.mainTutID, MainTutorialBitEventType.ShowOptionalTutorialBit,
                            tutorialBit.tutBitName);
                    else
                        newControlPromptFeedbacks?.PlayFeedbacks();
                }
            }
            else
            {
                if (other.CompareTag("FirstPersonPlayer") || other.CompareTag("Player")) _isPlayerInTrigger = true;
            }
        }
        void OnTriggerExit(Collider other)
        {
            if (ifNotBoopedStartObjectiveOnLeave)
                if (other.CompareTag("FirstPersonPlayer") || other.CompareTag("Player"))
                    TryAddAndActivateObjective();

            if (tutorialType == TutorialType.ControlPromptSequence)
            {
                if (other.CompareTag("FirstPersonPlayer") || other.CompareTag("Player"))
                {
                    _isPlayerInTrigger = false;
                    ControlsHelpEvent.Trigger(ControlHelpEventType.Hide, ActionId);
                    if (OfferOptionalTutorialBit)
                        MainTutorialBitEvent.Trigger(
                            tutorialBit.mainTutID, MainTutorialBitEventType.HideOptionalTutorialBit);

                    if (setNotTriggerableOnExit)
                        TriggerColliderEvent.Trigger(
                            uniqueID, TriggerColliderEventType.SetTriggerable, false, TriggerColliderType.Tutorial);
                }
            }
            else
            {
                if (other.CompareTag("FirstPersonPlayer") || other.CompareTag("Player"))
                {
                    _isPlayerInTrigger = false;
                    if (setNotTriggerableOnExit)
                        TriggerColliderEvent.Trigger(
                            uniqueID, TriggerColliderEventType.SetTriggerable, false, TriggerColliderType.Tutorial);
                }
            }
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

        public void OnMMEvent(SpontaneousTriggerEvent eventType)
        {
            if (eventType.UniqueID == UniqueID)
                if (eventType.EventType == SpontaneousTriggerEventType.Triggered)
                {
                    _isDisabled = false;
                    TriggerColliderEvent.Trigger(
                        uniqueID, TriggerColliderEventType.SetTriggerable, true, TriggerColliderType.Tutorial);
                }
        }
        void TryAddAndActivateObjective()
        {
            if (objectiveToStartOnBoop != null)
            {
                ObjectiveEvent.Trigger(
                    objectiveToStartOnBoop.objectiveId,
                    ObjectiveEventType.ObjectiveAdded);

                if (objectiveToBecomeActive)
                    ObjectiveEvent.Trigger(
                        objectiveToStartOnBoop.objectiveId,
                        ObjectiveEventType.ObjectiveActivated);
            }
        }

#if UNITY_EDITOR
        // This will be called from the parent ScriptableObject
        IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            var parent = ControlsPromptSchemeSet._currentContextSO;
            if (parent == null || parent.inputManagerPrefab == null) yield break;

            var data = parent.inputManagerPrefab.userData;
            if (data == null) yield break;

            foreach (var action in data.GetActions_Copy())
                yield return new ValueDropdownItem<int>(action.name, action.id);
        }
#endif

        enum TriggerType
        {
            OnEnter,
            OnExit,
            Both
        }
    }
}
