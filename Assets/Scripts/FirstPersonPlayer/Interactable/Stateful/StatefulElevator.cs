using System;
using System.Collections.Generic;
using System.Linq;
using creepycat.scifikitvol4;
using CustomAssets.Scripts;
using DG.Tweening;
using Events;
using FirstPersonPlayer.ScriptableObjects;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.Events;
using Helpers.Events.Machine;
using Helpers.Events.Machinery;
using Inventory;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Objectives.ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Interface;

namespace FirstPersonPlayer.Interactable.Stateful
{
    public class StatefulElevator : MonoBehaviour, IRequiresUniqueID, MMEventListener<SceneEvent>,
        MMEventListener<ElevatorEvent>, MMEventListener<SpontaneousTriggerEvent>
    {
        [Serializable]
        public enum ElevatorMovementState
        {
            AtRest,
            MovingUp,
            MovingDown
        }

        [Serializable]
        public enum ElevatorPowerState
        {
            Powered,
            Broken,
            Unpowered
        }

        public LayerMask buttonLayerMask;

        public ElevatorTypeInfo elevatorTypeInfo;


        public RewiredFirstPersonInputs playerInput;

        // [FormerlySerializedAs("EndingPoint")] [FormerlySerializedAs("StartPoint")] [Header("Elevator Travel Setup")]
        // public Transform endingPoint;
        // [FormerlySerializedAs("StartingPoint")] [FormerlySerializedAs("EndPoint")]
        // public Transform startingPoint;
        // Start from the top with index 0 and go down +1 for each floor
        [FormerlySerializedAs("ElevatorPoints")]
        public List<Transform> elevatorPoints;
        [FormerlySerializedAs("TravelTime")] public float travelTime = 10.0f;


        // public AudioClip elevatorSound;

        // public int currentFloorIndex;

        public GameObject elevatorScriptObject;


        public ElevatorState currentState;

        public MMFeedbacks accessDeniedFeedbacks;

        public string elevatorUniqueID;


        public MMFeedbacks clickFeedbacks;
        public MMFeedbacks elevatorTravelFB;
        public MMFeedbacks endTravelFeedbacks;
        public MMFeedbacks startTravelFeedbacks;


        public GameObject frontalBarrier;
        // public GameObject entranceBarriers;

        public GameObject[] entranceBarrierObjects;

        // [FormerlySerializedAs("startAtTop")]
        // [Tooltip("Should the elevator start at the top position (EndPoint) when the scene loads?")]
        // public bool startAtBottom;
        public int startAtIndex;

        [Header("Scene Change Settings")] [ToggleLeft] [LabelText("Elevator Leads to Scene Change?")]
        public bool doesElevatorLeadToSceneChange;
        [ShowIf(nameof(doesElevatorLeadToSceneChange))]
        public int floorWhichGoingToTriggersSceneChange;
        [ShowIf(nameof(doesElevatorLeadToSceneChange))]
        public string sceneNameToDisplayInModal;

        public ObjectiveToCompleteOnTravelingToFloor[] ObjectivesToCompleteOnTravelingToFloor;

        readonly EasyTimer movetimer = new();
        int _overrideFloor;

        bool _overrideKeyed;


        bool moveswitch;

        // Get components
        void Start()
        {
            if (IsAtBottom())
                elevatorScriptObject.transform.localPosition =
                    elevatorPoints[elevatorTypeInfo.numberOfFloors - 1].localPosition;
            else
                elevatorScriptObject.transform.localPosition = elevatorPoints[startAtIndex].localPosition;

            _overrideKeyed = false;
        }
        void FixedUpdate()
        {
            if (movetimer.IsDone)
            {
                if (moveswitch)
                {
                    //Debug.Log("Time A off");
                    moveswitch = false;
                    endTravelFeedbacks?.PlayFeedbacks();
                    elevatorTravelFB?.StopFeedbacks();
                }

                if (frontalBarrier != null)
                    frontalBarrier.SetActive(false);


                for (var i = 0; i < entranceBarrierObjects.Length; i++)
                    if (i == currentState.currentFloor)
                        entranceBarrierObjects[i].SetActive(false);
                    else
                        entranceBarrierObjects[i].SetActive(true);
            }
        }

        void OnEnable()
        {
            this.MMEventStartListening<SceneEvent>();
            this.MMEventStartListening<ElevatorEvent>();
            this.MMEventStartListening<SpontaneousTriggerEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<SceneEvent>();
            this.MMEventStopListening<ElevatorEvent>();
            this.MMEventStopListening<SpontaneousTriggerEvent>();
        }

        public string UniqueID => elevatorUniqueID;
        public void SetUniqueID()
        {
            elevatorUniqueID = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(elevatorUniqueID);
        }

        public void OnMMEvent(ElevatorEvent eventType)
        {
            if (eventType.ElevatorUniqueID != UniqueID) return;
            if (eventType.ElevatorEventType == ElevatorEventType.SetNextFloor)
            {
                _overrideKeyed = true;
                _overrideFloor = eventType.TargetFloor;
                Debug.Log("Received ElevatorEvent to go to floor " + eventType.TargetFloor);
            }
        }

        public void OnMMEvent(SceneEvent eventType)
        {
            if (eventType.EventType == SceneEventType.PlayerPawnLoaded) Initialize();
        }

        public void OnMMEvent(SpontaneousTriggerEvent eventType)
        {
        }

        bool IsAtBottom()
        {
            return currentState.currentFloor >= elevatorTypeInfo.numberOfFloors - 1;
        }

        bool IsAtTop()
        {
            return currentState.currentFloor <= 0;
        }

        void ElevatorGoDown(int indexOfDestination)
        {
            if (!TryRequestFloor(indexOfDestination)) return;

            foreach (var objectiveToComplete in ObjectivesToCompleteOnTravelingToFloor)
                if (objectiveToComplete.floorIndex == indexOfDestination)
                    ObjectiveEvent.Trigger(
                        objectiveToComplete.objectiveToComplete.objectiveId, ObjectiveEventType.ObjectiveCompleted);

            var floorsToTravel = indexOfDestination - currentState.currentFloor;
            var destinationPoint = elevatorPoints[indexOfDestination];
            clickFeedbacks?.PlayFeedbacks();
            startTravelFeedbacks?.PlayFeedbacks();
            elevatorTravelFB?.PlayFeedbacks();

            if (frontalBarrier != null)
                frontalBarrier.SetActive(true);

            for (var i = 0; i < entranceBarrierObjects.Length; i++) entranceBarrierObjects[i].SetActive(true);


            DOTween.Kill(elevatorScriptObject.transform); // prevent overlapping tweens

            elevatorScriptObject.transform
                .DOLocalMove(destinationPoint.localPosition, travelTime * floorsToTravel)
                .SetEase(Ease.InOutSine)
                .SetUpdate(false); // same behavior as normal TweenXYZ (affected by timeScale)

            movetimer.SetNewDuration(travelTime * floorsToTravel);

            currentState.currentFloor = indexOfDestination;

            ElevatorStateEvent.Trigger(
                UniqueID, currentState, ElevatorStateEventType.SetNewFloorState);


            // _isAtBottom = false;
            moveswitch = true;
        }

        void ElevatorGoUp(int indexOfDestination)
        {
            if (indexOfDestination == floorWhichGoingToTriggersSceneChange)
                Debug.Log(
                    "Trigger ask about scene change modal... commence regular elevator travel if yes, if Cancel, do nothing.");

            if (!TryRequestFloor(indexOfDestination))
                // AlertEvent.Trigger(
                //     AlertReason.ElevatorIssue, "The selected floor is not accessible from this elevator.",
                //     "Elevator Issue");
                return;

            var floorsToTravel = currentState.currentFloor - indexOfDestination;
            var destinationPoint = elevatorPoints[indexOfDestination];
            clickFeedbacks?.PlayFeedbacks();
            startTravelFeedbacks?.PlayFeedbacks();
            elevatorTravelFB?.PlayFeedbacks();

            if (frontalBarrier != null)
                frontalBarrier.SetActive(true);

            for (var i = 0; i < entranceBarrierObjects.Length; i++) entranceBarrierObjects[i].SetActive(true);


            //Debug.Log("Button Elevator Clicked");
            // TweenXYZ.Add(
            //         elevatorScriptObject.transform.gameObject, travelTime * floorsToTravel,
            //         destinationPoint.transform.localPosition)
            //     .EaseInOutCubic();
            DOTween.Kill(elevatorScriptObject.transform); // prevent overlapping tweens

            elevatorScriptObject.transform
                .DOLocalMove(destinationPoint.localPosition, travelTime * floorsToTravel)
                .SetEase(Ease.InOutSine)
                .SetUpdate(false); // same behavior as normal TweenXYZ (affected by timeScale)

            movetimer.SetNewDuration(travelTime * floorsToTravel);

            currentState.currentFloor = indexOfDestination;

            ElevatorStateEvent.Trigger(
                UniqueID, currentState, ElevatorStateEventType.SetNewFloorState);


            // _isAtBottom = true;
            moveswitch = true;
        }

        public void OnButtonClick(ButtonClickAnim buttonClickAnim)
        {
            var buttonType = buttonClickAnim.buttonType;

            switch (buttonType)
            {
                case ButtonClickAnim.ElevatorButtonType.CallToTop:
                    if (!IsAtTop())
                    {
                        ElevatorGoUp(0);
                        Debug.Log("IsAtTop false");
                    }
                    else
                    {
                        AlertEvent.Trigger(
                            AlertReason.ElevatorIssue, "Elevator is already at the top.",
                            "Elevator Issue");
                    }

                    break;
                case ButtonClickAnim.ElevatorButtonType.CallToBottom:
                    if (!IsAtBottom())
                    {
                        ElevatorGoDown(elevatorTypeInfo.numberOfFloors - 1);
                        Debug.Log("IsAtTop true");
                    }
                    else
                    {
                        AlertEvent.Trigger(
                            AlertReason.ElevatorIssue, "Elevator is already at the bottom.",
                            "Elevator Issue");
                    }

                    break;
                case ButtonClickAnim.ElevatorButtonType.ElevatorGoUp:
                    if (!IsAtTop())
                    {
                        var sceneNameToDisplayInModalLocal = string.IsNullOrEmpty(sceneNameToDisplayInModal)
                            ? "Above Area"
                            : sceneNameToDisplayInModal;

                        var indexOfDestination = currentState.currentFloor - 1;
                        if (doesElevatorLeadToSceneChange && indexOfDestination == floorWhichGoingToTriggersSceneChange)
                            AlertEvent.Trigger(
                                AlertReason.ElevatorSceneChangePermission,
                                "Do you want to go up to " + sceneNameToDisplayInModalLocal + "?",
                                "Elevator Scene Change", AlertType.ChoiceModal, 0f,
                                onConfirm: () => { ElevatorGoUp(indexOfDestination); }, onCancel: () => { });

                        else
                            ElevatorGoUp(indexOfDestination);

                        Debug.Log("IsAtTop false");
                    }
                    else
                    {
                        AlertEvent.Trigger(
                            AlertReason.ElevatorIssue, "Elevator is already at the top.",
                            "Elevator Issue");
                    }

                    break;
                case ButtonClickAnim.ElevatorButtonType.ElevatorGoDown:
                    if (!IsAtBottom())
                    {
                        var sceneNameToDisplayInModalLocal = string.IsNullOrEmpty(sceneNameToDisplayInModal)
                            ? "Below Area"
                            : sceneNameToDisplayInModal;

                        var spriteToDisplay = elevatorTypeInfo.floorSprites != null &&
                                              elevatorTypeInfo.floorSprites.Length >
                                              floorWhichGoingToTriggersSceneChange
                            ? elevatorTypeInfo.floorSprites[floorWhichGoingToTriggersSceneChange]
                            : elevatorTypeInfo.defaultFloorSprite;

                        int indexOfDestination;
                        if (_overrideKeyed)
                        {
                            if (_overrideFloor > currentState.currentFloor &&
                                _overrideFloor < elevatorTypeInfo.numberOfFloors)
                            {
                                indexOfDestination = _overrideFloor;
                            }
                            else
                            {
                                AlertEvent.Trigger(
                                    AlertReason.ElevatorIssue, "Invalid floor override requested.",
                                    "Elevator Issue");

                                indexOfDestination = currentState.currentFloor + 1;
                            }
                        }
                        else
                        {
                            indexOfDestination = currentState.currentFloor + 1;
                        }

                        if (doesElevatorLeadToSceneChange && indexOfDestination == floorWhichGoingToTriggersSceneChange)
                            AlertEvent.Trigger(
                                AlertReason.ElevatorSceneChangePermission,
                                "Do you want to down go to " + sceneNameToDisplayInModalLocal + "?" +
                                $"\n Your game will be saved and you'll enter {sceneNameToDisplayInModalLocal}.",
                                "Elevator Scene Change", AlertType.ChoiceModal, 0f,
                                onConfirm: () => { ElevatorGoDown(indexOfDestination); }, onCancel: () => { },
                                alertIcon: spriteToDisplay);

                        else
                            ElevatorGoDown(indexOfDestination);

                        _overrideKeyed = false;
                    }
                    else
                    {
                        AlertEvent.Trigger(
                            AlertReason.ElevatorIssue, "Elevator is already at the bottom.",
                            "Elevator Issue");
                    }

                    break;
                default:
                    AlertEvent.Trigger(
                        AlertReason.ElevatorIssue, "Unknown elevator button type.", "Elevator Issue");

                    break;
            }
        }
        void Initialize()
        {
            playerInput = FindFirstObjectByType<RewiredFirstPersonInputs>();

            if (playerInput == null) Debug.LogError("No RewiredFirstPersonInputs found in scene.");
        }

        bool HasRequiredKeyForFloor(int targetFloor)
        {
            var requiredKey = currentState.requiredKeysPerFloor?[targetFloor];
            if (requiredKey == null) return true;

            return GlobalInventoryManager.Instance.HasKeyForDoor(requiredKey.KeyID);
        }

        bool TryRequestFloor(int targetFloor)
        {
            // First check if the floor is normally accessible
            if (!currentState.accessibleFloors.Contains(targetFloor))
            {
                AlertEvent.Trigger(
                    AlertReason.ElevatorIssue,
                    "This floor is not accessible.", "Elevator Access");

                return false;
            }

            // Then check key requirement
            if (!HasRequiredKeyForFloor(targetFloor))
            {
                AlertEvent.Trigger(
                    AlertReason.ElevatorIssue,
                    "You need a key to access this floor.", "Access Denied");

                accessDeniedFeedbacks?.PlayFeedbacks();

                return false;
            }

            return true;
        }

        public bool IsMoving()
        {
            return !movetimer.IsDone;
        }

        [Serializable]
        public class ObjectiveToCompleteOnTravelingToFloor
        {
            [FormerlySerializedAs("FloorIndex")] public int floorIndex;
            [FormerlySerializedAs("ObjectiveToComplete")]
            public ObjectiveObject objectiveToComplete;
        }


        [Serializable]
        public class ElevatorState
        {
            [FormerlySerializedAs("position")] public ElevatorMovementState movementState;
            public ElevatorPowerState powerState;
            public int currentFloor;
            public int[] accessibleFloors;


            [Tooltip("Optional: If a floor has a required key, the player must possess it to go there.")]
            public KeyItemObject[] requiredKeysPerFloor;
        }
    }
}
