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
using UnityEngine.UI;

namespace SharedUI.Interact
{
    public class GatedBreakableUIController : MonoBehaviour, MMEventListener<MyUIEvent>
    {
        [SerializeField] TMP_Text interactionNameText;
        [SerializeField] TMP_Text staminaCostNumberText;
        [SerializeField] TMP_Text timeMinutesCostText;
        [SerializeField] TMP_Text yieldsQtyText;
        [SerializeField] TMP_Text yieldsNameText;
        [SerializeField] Image yieldedItemIcon;
        [SerializeField] TMP_Text contaminationRateNumText;
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
            if (eventType.uiType == UIType.BreakableInteractChoice)
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

        public void Initialize(GatedBreakableInteractionDetails details, List<string> appropriateToolsFound)
        {
            interactionNameText.text = details.interactionName;
            staminaCostNumberText.text = details.staminaCost.ToString(CultureInfo.InvariantCulture);
            timeMinutesCostText.text = details.timeCostMins.ToString(CultureInfo.InvariantCulture);
            contaminationRateNumText.text = details.contaminationCostPerMinute.ToString(CultureInfo.InvariantCulture);
            if (details.yieldsItem)
            {
                yieldsQtyText.text = details.itemYieldedQuantity.ToString(CultureInfo.InvariantCulture);
                yieldsNameText.text = details.yieldedBaseItem.ItemName;
                var itemSO = Resources.Load<MyBaseItem>($"Items/{details.yieldedBaseItem.ItemID}");
                if (itemSO != null && itemSO.Icon != null)
                {
                    yieldedItemIcon.sprite = itemSO.Icon;
                    yieldedItemIcon.enabled = true;
                }
            }
            else
            {
                yieldsQtyText.text = "0";
                yieldsNameText.text = "N/A";
                yieldedItemIcon.sprite = null;
                yieldedItemIcon.enabled = false;
            }

            if (details.requireTools && appropriateToolsFound.Count > 0)
            {
                var toolID = details.GetMostEfficientRequiredToolID(appropriateToolsFound);
                var toolSO = Resources.Load<MyBaseItem>($"Items/{toolID}");
                toolUsedNameText.text = toolSO.ItemName;
                if (toolSO != null && toolSO.Icon != null)
                {
                    toolUsedIcon.sprite = toolSO.Icon;
                    toolUsedIcon.enabled = true;
                }
            }
            else
            {
                toolUsedNameText.text = "N/A";
                toolUsedIcon.sprite = null;
                toolUsedIcon.enabled = false;
            }
        }

        public void OnConfirmPressed(GatedBreakableInteractionDetails details, string subjectUniqueID,
            List<string> toolsFound)
        {
            // Hide the choice UI
            MyUIEvent.Trigger(UIType.BreakableInteractChoice, UIActionType.Close);

            MyUIEvent.Trigger(UIType.WaitWhileInteracting, UIActionType.Open);

            // Show overlay and simulate waiting
            var durationSeconds = details.timeCostMins / 60f * 10f; // sped-up “minutes” → seconds
            waitOverlay.Show(details.interactionName);

            // 🔹 Fire START event immediately
            GatedBreakableInteractionEvent.Trigger(
                GatedInteractionEventType.StartInteraction, details, subjectUniqueID, toolsFound);


            StartCoroutine(
                waitOverlay.SimulateProgress(
                    durationSeconds, () =>
                    {
                        waitOverlay.Hide();

                        MyUIEvent.Trigger(UIType.WaitWhileInteracting, UIActionType.Close);

                        // 🔹 Fire COMPLETE event when done
                        GatedBreakableInteractionEvent.Trigger(
                            GatedInteractionEventType.CompleteInteraction, details, subjectUniqueID, toolsFound);
                    }));
        }
    }
}
