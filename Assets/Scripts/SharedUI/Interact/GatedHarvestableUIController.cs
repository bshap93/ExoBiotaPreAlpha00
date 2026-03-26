using System.Collections.Generic;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.Events;
using Helpers.Events.Gated;
using Helpers.ScriptableObjects.Gated;
using LevelConstruct.Interactable.ItemInteractables.ItemPicker;
using Michsky.MUIP;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SharedUI.Interact
{
    public class GatedHarvestableUIController : MonoBehaviour, MMEventListener<MyUIEvent>
    {
        [Header("Panes")] [SerializeField] GameObject interactionPane;
        [SerializeField] GameObject itemPane;

        [Header("Item Info")] [SerializeField] TMP_Text itemName;
        [SerializeField] Image itemIcon;
        [SerializeField] TMP_Text itemWeightText;
        [SerializeField] TMP_Text itemDescriptionText;
        public ButtonManager pickupItemButton;

        [Header("Interaction Info")] [SerializeField]
        TMP_Text interactionNameText;
        [SerializeField] TMP_Text staminaCostNumberText;
        [SerializeField] TMP_Text timeMinutesCostText;


        [Header("Yield Info")] [SerializeField]
        TMP_Text yieldsQtyText;
        [SerializeField] TMP_Text yieldsNameText;
        [SerializeField] Image yieldedItemIcon;

        [Header("Contamination Info")] [SerializeField]
        TMP_Text contaminationRateNumText;

        [Header("Chemical Info")] [SerializeField]
        TMP_Text chemicalUsedNameText;
        [SerializeField] Image chemicalUsedIcon;

        [Header("Tool Info")] [SerializeField] TMP_Text toolUsedNameText;
        [SerializeField] Image toolUsedIcon;

        [Header("Buttons")] [SerializeField] public ButtonManager confirmDoButton;
        [SerializeField] public ButtonManager cancelButton;
        [SerializeField] WaitWhileInteractingOverlay waitOverlay;

        CanvasGroup _canvasGroup;

        ItemPicker _currentItemPicker;


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
            if (eventType.uiType == UIType.HarvestableInteractChoice)
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

// Replace the Initialize method in GatedHarvestableUIController with this version

        public void Initialize(GatedHarvestalbeInteractionDetails details, List<string> appropriateChemsFound,
            List<string> appropriateToolsFound)
        {
            // ✅ Check if player meets requirements for gated interaction
            var meetsChemRequirement = !details.requiresChemical ||
                                       (appropriateChemsFound != null && appropriateChemsFound.Count > 0);

            var meetsToolRequirement = !details.requireTools ||
                                       (appropriateToolsFound != null && appropriateToolsFound.Count > 0);

            var canDoGatedInteraction = meetsChemRequirement && meetsToolRequirement;

            // ✅ Always show item pane, conditionally show interaction pane
            if (itemPane != null) itemPane.SetActive(true);
            if (interactionPane != null) interactionPane.SetActive(canDoGatedInteraction);

            // Populate item info (always shown)
            if (details.item != null)
            {
                itemName.text = details.item.ItemName;
                itemIcon.sprite = details.item.Icon;
                itemWeightText.text = $"{details.item.weight} KG";
                itemDescriptionText.text = details.item.Description;
            }
            else
            {
                Debug.LogWarning($"[GatedHarvestableUIController] Details '{details.name}' has no item assigned!");
                itemName.text = "Unknown Item";
                itemIcon.sprite = null;
                itemWeightText.text = "0 KG";
                itemDescriptionText.text = "No description available.";
            }

            // Only populate interaction details if requirements are met
            if (canDoGatedInteraction)
            {
                interactionNameText.text = details.interactionName;
                staminaCostNumberText.text = $"{details.staminaCost}";
                timeMinutesCostText.text = $"{details.timeCostMins}";

                // Yield info
                if (details.yieldsItem && details.yieldedBaseItem != null)
                {
                    yieldsQtyText.text = details.itemYieldedQuantity.ToString();
                    yieldsNameText.text = details.yieldedBaseItem.ItemName;
                    yieldedItemIcon.sprite = details.yieldedBaseItem.Icon;
                }
                else
                {
                    yieldsQtyText.text = "0";
                    yieldsNameText.text = "N/A";
                    yieldedItemIcon.sprite = null;
                }

                contaminationRateNumText.text = $"{details.contaminationCostPerMinute} per min";

                // Chemical info
                if (details.requiresChemical && appropriateChemsFound != null && appropriateChemsFound.Count > 0)
                {
                    var chemItemID = details.GetMostEfficientChemicalID(appropriateChemsFound);
                    if (!string.IsNullOrEmpty(chemItemID))
                    {
                        var chemSO = Resources.Load<MyBaseItem>($"Items/{chemItemID}");
                        if (chemSO != null)
                        {
                            chemicalUsedNameText.text = chemSO.ItemName;
                            chemicalUsedIcon.enabled = true;
                            chemicalUsedIcon.sprite = chemSO.Icon;
                        }
                        else
                        {
                            Debug.LogWarning(
                                $"[GatedHarvestableUIController] Could not load chemical item: {chemItemID}");

                            chemicalUsedNameText.text = chemItemID;
                            chemicalUsedIcon.enabled = true;
                            chemicalUsedIcon.sprite = null;
                        }
                    }
                    else
                    {
                        chemicalUsedNameText.text = "";
                        chemicalUsedIcon.sprite = null;
                        chemicalUsedIcon.enabled = false;
                    }
                }
                else
                {
                    chemicalUsedNameText.text = "";
                    chemicalUsedIcon.sprite = null;
                    chemicalUsedIcon.enabled = false;
                }

                // Tool info
                if (details.requireTools && appropriateToolsFound != null && appropriateToolsFound.Count > 0)
                {
                    var toolID = details.GetMostEfficientToolID(appropriateToolsFound);
                    if (!string.IsNullOrEmpty(toolID))
                    {
                        var toolSO = Resources.Load<MyBaseItem>($"Items/{toolID}");
                        if (toolSO != null)
                        {
                            toolUsedNameText.text = toolSO.ItemName;
                            toolUsedIcon.enabled = true;
                            toolUsedIcon.sprite = toolSO.Icon;
                        }
                        else
                        {
                            Debug.LogWarning($"[GatedHarvestableUIController] Could not load tool item: {toolID}");
                            toolUsedNameText.text = toolID;
                            toolUsedIcon.enabled = true;
                            toolUsedIcon.sprite = null;
                        }
                    }
                    else
                    {
                        toolUsedNameText.text = "";
                        toolUsedIcon.sprite = null;
                        toolUsedIcon.enabled = false;
                    }
                }
                else
                {
                    toolUsedNameText.text = "";
                    toolUsedIcon.sprite = null;
                    toolUsedIcon.enabled = false;
                }
            }
        }

        public void OnConfirmPressed(GatedHarvestalbeInteractionDetails details, string subjectUniqueID,
            List<string> chemsFound, List<string> toolsFound)
        {
            MyUIEvent.Trigger(UIType.HarvestableInteractChoice, UIActionType.Close);

            MyUIEvent.Trigger(UIType.WaitWhileInteracting, UIActionType.Open);

            var durationSeconds = details.timeCostMins / 60f * 10f;
            waitOverlay.Show(details.interactionName);

            GatedHarvestableInteractionEvent.Trigger(
                GatedInteractionEventType.StartInteraction, details, subjectUniqueID, chemsFound, toolsFound);

            StartCoroutine(
                waitOverlay.SimulateProgress(
                    durationSeconds, () =>
                    {
                        waitOverlay.Hide();

                        MyUIEvent.Trigger(UIType.WaitWhileInteracting, UIActionType.Close);

                        GatedHarvestableInteractionEvent.Trigger(
                            GatedInteractionEventType.CompleteInteraction, details, subjectUniqueID, chemsFound,
                            toolsFound);
                    }));
        }
    }
}
