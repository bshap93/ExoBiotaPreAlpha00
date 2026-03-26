using System.Collections.Generic;
using System.Globalization;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.Events;
using Helpers.Events.Gated;
using Helpers.ScriptableObjects.Gated;
using Michsky.MUIP;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SharedUI.Interact
{
    public class GatedMachineUIController : MonoBehaviour, MMEventListener<MyUIEvent>
    {
        public bool yieldsItem;
        public bool contaminatesPlayer;

        [Header("Machine Info")] [SerializeField]
        TMP_Text machineName;
        [SerializeField] Image machineIcon;
        [SerializeField] TMP_Text machineTypeText;
        [SerializeField] TMP_Text machineStatusText;
        [SerializeField] TMP_Text machineDescriptionText;

        [SerializeField] TMP_Text staminaCostNumberText;
        [SerializeField] TMP_Text timeMinutesCostText;

        [FormerlySerializedAs("actionNameText")] [SerializeField]
        TMP_Text actionDescriptionText;
        [SerializeField] Image actionIcon;

        [SerializeField] TMP_Text effectMagnitudeText;
        [SerializeField] TMP_Text effectDescriptionText;
        [SerializeField] Image effectIcon;

        [SerializeField] TMP_Text fuelBattUsedNameText;
        [SerializeField] Image fuelBattUsedIcon;

        [SerializeField] TMP_Text toolUsedNameText;
        [SerializeField] Image toolUsedIcon;
        [SerializeField] public ButtonManager confirmDoButton;
        [SerializeField] public ButtonManager cancelButton;
        [SerializeField] WaitWhileInteractingOverlay waitOverlay;

        CanvasGroup _canvasGroup;


        void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            // hide
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }
        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(MyUIEvent eventType)
        {
            if (eventType.uiType == UIType.MachineInteractChoice)
            {
                if (eventType.uiActionType == UIActionType.Open)
                {
                    // show
                    _canvasGroup.alpha = 1;
                    _canvasGroup.interactable = true;
                    _canvasGroup.blocksRaycasts = true;
                }
                else if (eventType.uiActionType == UIActionType.Close)
                {
                    // hide
                    _canvasGroup.alpha = 0;
                    _canvasGroup.interactable = false;
                    _canvasGroup.blocksRaycasts = false;
                }
            }
        }

        public void Initialize(GatedMachineInteractionDetails details, List<string> approriateFuelBatteriesFound,
            List<string> appropriateToolsFound)
        {
            machineName.text = details.machineName;
            machineIcon.sprite = details.machineIcon;
            machineTypeText.text = details.gatedMachineType.ToString();
            machineStatusText.text = details.targetMachineStatus.ToString();

            machineDescriptionText.text = details.machineDescription;
            staminaCostNumberText.text = details.staminaCost.ToString(CultureInfo.InvariantCulture);
            timeMinutesCostText.text = details.timeCostMins.ToString(CultureInfo.InvariantCulture);

            actionDescriptionText.text = details.actionDescription;
            actionIcon.sprite = details.actionIcon;

            effectMagnitudeText.text = details.effectMagnitude.ToString(CultureInfo.InvariantCulture);
            effectDescriptionText.text = details.effectDescription;
            effectIcon.sprite = details.effectIcon;

            if (details.takesFuelBatteryItem && approriateFuelBatteriesFound.Count > 0)
            {
                var fuelBattID = details.GetMostEfficientFuelBatteryItemID(approriateFuelBatteriesFound);
                var fuelBattSO = Resources.Load<MyBaseItem>($"Items/{fuelBattID}");
                fuelBattUsedNameText.text = fuelBattSO.ItemName;
                if (fuelBattSO != null && fuelBattSO.Icon != null)
                    fuelBattUsedIcon.sprite = fuelBattSO.Icon;
            }
            else
            {
                fuelBattUsedNameText.text = "N/A";
                fuelBattUsedIcon.sprite = null;
            }

            if (details.requireTools && appropriateToolsFound.Count > 0)
            {
                var toolID = appropriateToolsFound[0];
                var toolSO = Resources.Load<MyBaseItem>($"Items/{toolID}");
                toolUsedNameText.text = toolSO.ItemName;
                if (toolSO != null && toolSO.Icon != null)
                    toolUsedIcon.sprite = toolSO.Icon;
            }
            else
            {
                toolUsedNameText.text = "N/A";
                toolUsedIcon.sprite = null;
            }
        }
        public void OnConfirmPressed(GatedMachineInteractionDetails details, string subjectUniqueID,
            List<string> fuelBatsFound, List<string> toolsFound)
        {
            MyUIEvent.Trigger(UIType.MachineInteractChoice, UIActionType.Close);
            MyUIEvent.Trigger(UIType.WaitWhileInteracting, UIActionType.Open);

            // var durationSeconds = details.timeCostMins / 60f * 10f;
            // waitOverlay.Show(details.interactionName);
            waitOverlay.Show(details.interactionName);

            GatedMachineInteractionEvent.Trigger(
                GatedInteractionEventType.StartInteraction, details, subjectUniqueID, fuelBatsFound, toolsFound);

            StartCoroutine(
                waitOverlay.SimulateProgress(
                    details.realWorldWaitDuration, () =>
                    {
                        waitOverlay.Hide();

                        // redundant?
                        MyUIEvent.Trigger(UIType.MachineInteractChoice, UIActionType.Close);

                        GatedMachineInteractionEvent.Trigger(
                            GatedInteractionEventType.CompleteInteraction, details, subjectUniqueID, fuelBatsFound,
                            toolsFound);
                    }));
        }
    }
}
