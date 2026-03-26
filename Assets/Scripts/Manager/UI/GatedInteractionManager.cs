using FirstPersonPlayer.Interactable.Gated;
using FirstPersonPlayer.Interface;
using Helpers.Events;
using Helpers.Events.Gated;
using Helpers.Events.Status;
using Helpers.ScriptableObjects.Gated;
using LevelConstruct.Interactable.ItemInteractables.ItemPicker;
using Manager.ProgressionMangers;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using SharedUI.Interact;
using UnityEngine;
using UnityEngine.Serialization;

namespace Manager.UI
{
    public class GatedInteractionManager : MonoBehaviour, MMEventListener<GatedBreakableInteractionEvent>,
        MMEventListener<GatedHarvestableInteractionEvent>, MMEventListener<GatedMachineInteractionEvent>,
        MMEventListener<GatedRestEvent>, MMEventListener<GatedLevelingEvent>
    {
        public enum ReasonWhyCannotInteract
        {
            None,
            LackingNecessaryTool,
            NotEnoughStamina,
            LackingNecessaryFuelBattery
        }

        public static GatedInteractionManager Instance;
        public bool isActiveGui;

        [FormerlySerializedAs("uiController")] [FormerlySerializedAs("gatedInteractionUIController")] [SerializeField]
        GatedBreakableUIController breakableUIController;
        [SerializeField] GatedHarvestableUIController gatedHarvestableUIController;
        [SerializeField] GatedMachineUIController gatedMachineUIController;
        [SerializeField] GatedRestUIController gatedRestUIController;

        [FormerlySerializedAs("bringUpBreakableUIFeedbacks")] [SerializeField]
        MMFeedbacks bringUpGatedUIFeedbacks;


        [FormerlySerializedAs("closeBreakableUIFeedbacks")] [SerializeField]
        MMFeedbacks closeGatedUIFeedbacks;

        [SerializeField] MMFeedbacks cancelGatedInteractionFeedbacks;
        [SerializeField] AttributesManager attributesManager;
        GatedBreakableInteractionDetails _currentBreakableDetails;
        string _currentDockId;
        GatedHarvestalbeInteractionDetails _currentHarvestableDetails;
        IInteractable _currentInteractable;
        ItemPicker _currentItemPicker; // ✅ Store reference to the ItemPicker
        InteractableMachine _currentMachine;
        GatedMachineInteractionDetails _currentMachineDetails;
        GatedRestDetails _currentRestDetails;
        string _currentSubjectUniqueID; // ✅ Store the unique ID
        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }


        void OnEnable()
        {
            this.MMEventStartListening<GatedBreakableInteractionEvent>();
            this.MMEventStartListening<GatedHarvestableInteractionEvent>();
            this.MMEventStartListening<GatedMachineInteractionEvent>();
            this.MMEventStartListening<GatedRestEvent>();
            this.MMEventStartListening<GatedLevelingEvent>();
        }
        void OnDisable()
        {
            this.MMEventStopListening<GatedBreakableInteractionEvent>();
            this.MMEventStopListening<GatedHarvestableInteractionEvent>();
            this.MMEventStopListening<GatedMachineInteractionEvent>();
            this.MMEventStopListening<GatedRestEvent>();
            this.MMEventStopListening<GatedLevelingEvent>();
        }
        public void OnMMEvent(GatedBreakableInteractionEvent eventType)
        {
            var effectiveTimeCostMultiplier =
                attributesManager.GetEffectiveTimeCostMultiplier(GatedInteractionType.BreakObstacle);

            if (eventType.EventType == GatedInteractionEventType.TriggerGateUI)
            {
                _currentBreakableDetails = eventType.Details;
                _currentBreakableDetails.timeCostMins = Mathf.FloorToInt(
                    _currentBreakableDetails.timeCostMins * effectiveTimeCostMultiplier);

                isActiveGui = true;

                // Optionally, if event also carries source node:
                // currentNode = e.SourceNode;

                breakableUIController.Initialize(_currentBreakableDetails, eventType.ToolsFound);

                bringUpGatedUIFeedbacks?.PlayFeedbacks();

                MyUIEvent.Trigger(UIType.BreakableInteractChoice, UIActionType.Open);

                breakableUIController.confirmDoButton.onClick.RemoveAllListeners();
                breakableUIController.cancelButton.onClick.RemoveAllListeners();
                // uiController.confirmDoButton.onClick.AddListener(ConfirmInteraction);
                breakableUIController.confirmDoButton.onClick.AddListener(() =>
                {
                    breakableUIController.OnConfirmPressed(
                        _currentBreakableDetails, eventType.SubjectUniqueID, eventType.ToolsFound);

                    closeGatedUIFeedbacks?.PlayFeedbacks();
                });

                breakableUIController.cancelButton.onClick.AddListener(CancelBreakableInteraction);
            }

            else if (eventType.EventType == GatedInteractionEventType.StartInteraction)
            {
                var effectiveTimeCost =
                    Mathf.FloorToInt(eventType.Details.timeCostMins * effectiveTimeCostMultiplier);

                // ✅ Calculate and set the required acceleration
                var timeManager = InGameTimeManager.Instance;
                var requiredAcceleration = timeManager.CalculateRequiredAcceleration(
                    effectiveTimeCost,
                    eventType.Details.realWorldWaitDuration);

                timeManager.SetAcceleration(requiredAcceleration);

                InGameTimeActionEvent.Trigger(InGameTimeActionEvent.ActionType.LapseTime);
            }
            else if (eventType.EventType == GatedInteractionEventType.CompleteInteraction)
            {
                InGameTimeActionEvent.Trigger(InGameTimeActionEvent.ActionType.StopLapseTime);

                isActiveGui = false;

                var staminaCost = eventType.Details.staminaCost;
                // Deduct stamina from player
                PlayerStatsEvent.Trigger(
                    PlayerStatsEvent.PlayerStat.CurrentStamina, PlayerStatsEvent.PlayerStatChangeType.Decrease,
                    staminaCost);
            }
        }
        public void OnMMEvent(GatedHarvestableInteractionEvent eventType)
        {
            var effectiveTimeCostMultiplier =
                attributesManager.GetEffectiveTimeCostMultiplier(GatedInteractionType.HarvesteableBiological);

            if (eventType.EventType == GatedInteractionEventType.TriggerGateUI)
            {
                _currentHarvestableDetails = eventType.Details;
                _currentSubjectUniqueID = eventType.SubjectUniqueID; // ✅ Store unique ID
                _currentHarvestableDetails.timeCostMins = Mathf.FloorToInt(
                    _currentHarvestableDetails.timeCostMins * effectiveTimeCostMultiplier);

                isActiveGui = true;

                // ✅ Find the ItemPicker by unique ID
                var allPickers = FindObjectsByType<ItemPicker>(FindObjectsSortMode.None);
                foreach (var picker in allPickers)
                    if (picker.uniqueID == _currentSubjectUniqueID)
                    {
                        _currentItemPicker = picker;
                        break;
                    }


                gatedHarvestableUIController.Initialize(
                    _currentHarvestableDetails, eventType.ChemicalsFound, eventType.ToolsFound);

                MyUIEvent.Trigger(UIType.HarvestableInteractChoice, UIActionType.Open);

                gatedHarvestableUIController.confirmDoButton.onClick.RemoveAllListeners();
                gatedHarvestableUIController.cancelButton.onClick.RemoveAllListeners();
                gatedHarvestableUIController.pickupItemButton.onClick.RemoveAllListeners();

                bringUpGatedUIFeedbacks?.PlayFeedbacks();

                gatedHarvestableUIController.confirmDoButton.onClick.AddListener(() =>
                {
                    gatedHarvestableUIController.OnConfirmPressed(
                        _currentHarvestableDetails, eventType.SubjectUniqueID,
                        eventType.ChemicalsFound, eventType.ToolsFound);

                    closeGatedUIFeedbacks?.PlayFeedbacks();
                });

                gatedHarvestableUIController.cancelButton.onClick.AddListener(CancelHarvestableInteraction);
                gatedHarvestableUIController.pickupItemButton.onClick.AddListener(() =>
                    PickupHarvestableItem(eventType.SubjectUniqueID));
            }

            else if (eventType.EventType == GatedInteractionEventType.StartInteraction)
            {
                var effectiveTimeCost =
                    Mathf.FloorToInt(eventType.Details.timeCostMins * effectiveTimeCostMultiplier);

                // ✅ Calculate and set the required acceleration
                var timeManager = InGameTimeManager.Instance;
                var requiredAcceleration = timeManager.CalculateRequiredAcceleration(
                    effectiveTimeCost,
                    eventType.Details.realWorldWaitDuration);

                timeManager.SetAcceleration(requiredAcceleration);

                InGameTimeActionEvent.Trigger(InGameTimeActionEvent.ActionType.LapseTime);
            }
            else if (eventType.EventType == GatedInteractionEventType.CompleteInteraction)
            {
                InGameTimeActionEvent.Trigger(InGameTimeActionEvent.ActionType.StopLapseTime);

                isActiveGui = false;

                var staminaCost = eventType.Details.staminaCost;
                // Deduct stamina from player
                PlayerStatsEvent.Trigger(
                    PlayerStatsEvent.PlayerStat.CurrentStamina, PlayerStatsEvent.PlayerStatChangeType.Decrease,
                    staminaCost);
            }
        }
        public void OnMMEvent(GatedLevelingEvent eventType)
        {
            if (eventType.EventType == GatedInteractionEventType.TriggerGateUI)
                isActiveGui = true;
            else if (eventType.EventType == GatedInteractionEventType.CompleteInteraction) isActiveGui = false;
        }
        public void OnMMEvent(GatedMachineInteractionEvent eventType)
        {
            var effectiveTimeCostMultiplier =
                attributesManager.GetEffectiveTimeCostMultiplier(GatedInteractionType.InteractMachine);

            if (eventType.EventType == GatedInteractionEventType.TriggerGateUI)
            {
                _currentMachineDetails = eventType.Details;
                _currentSubjectUniqueID = eventType.SubjectUniqueID;
                _currentMachineDetails.timeCostMins = Mathf.FloorToInt(
                    _currentMachineDetails.timeCostMins * effectiveTimeCostMultiplier);


                isActiveGui = true;

                var allMachines = FindObjectsByType<InteractableMachine>(FindObjectsSortMode.None);

                foreach (var machine in allMachines)
                    if (machine.uniqueID == _currentSubjectUniqueID)
                    {
                        _currentMachine = machine;
                        break;
                    }

                gatedMachineUIController.Initialize(
                    _currentMachineDetails, eventType.FuelBatteriesFound,
                    eventType.ToolsFound);

                bringUpGatedUIFeedbacks?.PlayFeedbacks();

                MyUIEvent.Trigger(UIType.MachineInteractChoice, UIActionType.Open);

                gatedMachineUIController.confirmDoButton.onClick.RemoveAllListeners();
                gatedMachineUIController.cancelButton.onClick.RemoveAllListeners();

                gatedMachineUIController.confirmDoButton.onClick.AddListener(() =>
                {
                    gatedMachineUIController.OnConfirmPressed(
                        _currentMachineDetails, eventType.SubjectUniqueID,
                        eventType.FuelBatteriesFound, eventType.ToolsFound);

                    closeGatedUIFeedbacks?.PlayFeedbacks();
                });

                gatedMachineUIController.cancelButton.onClick.AddListener(
                    CancelMachineInteraction);
            }
            else if (eventType.EventType == GatedInteractionEventType.StartInteraction)
            {
                // ✅ Calculate and set the required acceleration
                var timeManager = InGameTimeManager.Instance;
                var effectiveTimeCost =
                    Mathf.FloorToInt(eventType.Details.timeCostMins * effectiveTimeCostMultiplier);

                var requiredAcceleration = timeManager.CalculateRequiredAcceleration(
                    effectiveTimeCost,
                    eventType.Details.realWorldWaitDuration);

                timeManager.SetAcceleration(requiredAcceleration);

                InGameTimeActionEvent.Trigger(InGameTimeActionEvent.ActionType.LapseTime);
            }
            else if (eventType.EventType == GatedInteractionEventType.CompleteInteraction)
            {
                InGameTimeActionEvent.Trigger(InGameTimeActionEvent.ActionType.StopLapseTime);

                isActiveGui = false;

                var staminaCost = eventType.Details.staminaCost;
                // Deduct stamina from player
                PlayerStatsEvent.Trigger(
                    PlayerStatsEvent.PlayerStat.CurrentStamina, PlayerStatsEvent.PlayerStatChangeType.Decrease,
                    staminaCost);
            }
        }
        public void OnMMEvent(GatedRestEvent eventType)
        {
            var effectiveTimeCostMultiplier =
                attributesManager.GetEffectiveTimeCostMultiplier(GatedInteractionType.Rest);

            if (eventType.EventType == GatedInteractionEventType.TriggerGateUI)
            {
                _currentRestDetails = eventType.RestDetails;
                _currentDockId = eventType.DockId;
                gatedRestUIController.currentDockId = eventType.DockId;
                _currentRestDetails.timeCostMins = Mathf.FloorToInt(
                    _currentRestDetails.timeCostMins * effectiveTimeCostMultiplier);

                isActiveGui = true;

                gatedRestUIController.Initialize(eventType.RestDetails);

                MyUIEvent.Trigger(UIType.RestTimeSetAmount, UIActionType.Open);


                gatedRestUIController.confirmRestButton.onClick.RemoveAllListeners();
                gatedRestUIController.cancelButton.onClick.RemoveAllListeners();
                // gatedRestUIController.restUntilStaminaFullButton.onClick.RemoveAllListeners();
                gatedRestUIController.timeLengthSlider.onValueChanged.RemoveAllListeners();

                bringUpGatedUIFeedbacks?.PlayFeedbacks();

                gatedRestUIController.confirmRestButton.onClick.AddListener(() =>
                {
                    gatedRestUIController.OnConfirmPressed(
                        _currentRestDetails);

                    closeGatedUIFeedbacks?.PlayFeedbacks();
                });

                gatedRestUIController.cancelButton.onClick.AddListener(CloseRestUI);

                // gatedRestUIController.restUntilStaminaFullButton.onClick.AddListener(() =>
                //     gatedRestUIController.OnSetRestUntilStaminaFullPressed(
                //         _currentRestDetails));

                gatedRestUIController.timeLengthSlider.onValueChanged.AddListener(value =>
                    gatedRestUIController.OnTimeLengthSliderChanged(
                        _currentRestDetails, value));
            }
            else if (eventType.EventType == GatedInteractionEventType.StartInteraction)
            {
                var timeManager = InGameTimeManager.Instance;
                var effectiveTimeCost =
                    Mathf.FloorToInt(eventType.RestTimeMinutes * effectiveTimeCostMultiplier);

                var requiredAcceleration = timeManager.CalculateRequiredAcceleration(
                    effectiveTimeCost,
                    eventType.RestDetails.realWorldWaitDuration);

                timeManager.SetAcceleration(requiredAcceleration);

                InGameTimeActionEvent.Trigger(InGameTimeActionEvent.ActionType.LapseTime);
            }
            else if (eventType.EventType == GatedInteractionEventType.CompleteInteraction)
            {
                InGameTimeActionEvent.Trigger(InGameTimeActionEvent.ActionType.StopLapseTime);

                var staminaRestored = eventType.RestTimeMinutes *
                                      eventType.RestDetails.staminaRestoredPerMinute;

                isActiveGui = false;

                // Restore stamina to player
                PlayerStatsEvent.Trigger(
                    PlayerStatsEvent.PlayerStat.CurrentStamina, PlayerStatsEvent.PlayerStatChangeType.Increase,
                    staminaRestored);
            }
        }
        void CloseRestUI()
        {
            gatedRestUIController.confirmRestButton.onClick.RemoveAllListeners();
            gatedRestUIController.cancelButton.onClick.RemoveAllListeners();
            gatedRestUIController.restUntilStaminaFullButton.onClick.RemoveAllListeners();
            gatedRestUIController.timeLengthSlider.onValueChanged.RemoveAllListeners();

            isActiveGui = false;

            cancelGatedInteractionFeedbacks?.PlayFeedbacks();

            MyUIEvent.Trigger(UIType.RestTimeSetAmount, UIActionType.Close);
            GatedRestEvent.Trigger(GatedInteractionEventType.CloseGatedInteractionUI, null, 0, _currentDockId);
            //? _currentRestDetails = null;
        }

        void CancelMachineInteraction()
        {
            CloseMachineUI();
        }
        void CloseMachineUI()
        {
            gatedMachineUIController.confirmDoButton.onClick.RemoveAllListeners();
            gatedMachineUIController.cancelButton.onClick.RemoveAllListeners();

            isActiveGui = false;

            // hide UI, reset references, etc.
            MyUIEvent.Trigger(UIType.MachineInteractChoice, UIActionType.Close);
            _currentMachine = null;
            _currentSubjectUniqueID = null;
        }
        void PickupHarvestableItem(string subjectUniqueID)
        {
            if (_currentItemPicker == null)
            {
                Debug.LogError("[GatedInteractionManager] No current ItemPicker to pick up!");
                return;
            }


            // ✅ Find the ItemPicker by unique ID
            var allPickers = FindObjectsByType<ItemPicker>(FindObjectsSortMode.None);
            foreach (var picker in allPickers)
                if (picker.uniqueID == subjectUniqueID)
                {
                    _currentItemPicker = picker;
                    break;
                }


            Debug.Log($"[GatedInteractionManager] Picking up item: {_currentItemPicker.gameObject.name}");
            // Trigger the direct pickup (bypasses gated interactions)
            _currentItemPicker.PickupItemDirect();
            // Close the UI first
            CloseHarvestableUI();
        }
        void CancelBreakableInteraction()
        {
            cancelGatedInteractionFeedbacks?.PlayFeedbacks();
            CloseBreakableUI();
        }

        void CloseBreakableUI()
        {
            breakableUIController.confirmDoButton.onClick.RemoveAllListeners();
            breakableUIController.cancelButton.onClick.RemoveAllListeners();
            // hide UI, reset references, etc.
            MyUIEvent.Trigger(UIType.BreakableInteractChoice, UIActionType.Close);

            isActiveGui = false;
        }

        void CancelHarvestableInteraction()
        {
            CloseHarvestableUI();
        }

        void CloseHarvestableUI()
        {
            gatedHarvestableUIController.confirmDoButton.onClick.RemoveAllListeners();
            gatedHarvestableUIController.cancelButton.onClick.RemoveAllListeners();
            gatedHarvestableUIController.pickupItemButton.onClick.RemoveAllListeners();

            isActiveGui = false;

            // hide UI, reset references, etc.
            MyUIEvent.Trigger(UIType.HarvestableInteractChoice, UIActionType.Close);
            _currentItemPicker = null; // Clear reference
            _currentSubjectUniqueID = null;
        }
    }
}
