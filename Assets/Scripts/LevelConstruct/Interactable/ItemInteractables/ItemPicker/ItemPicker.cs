using System;
using System.Collections;
using System.Collections.Generic;
using Dirigible.Input;
using FirstPersonPlayer.Interface;
using FirstPersonPlayer.Tools.Interface;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using FirstPersonPlayer.Tools.ToolPrefabScripts;
using Gameplay.Events;
using Helpers.Events;
using Helpers.Events.Gated;
using Helpers.Events.Inventory;
using Inventory;
using LevelConstruct.Highlighting;
using Manager;
using Manager.ProgressionMangers;
using Manager.SceneManagers.Pickable;
using MoreMountains.Feedbacks;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using Objectives.ScriptableObjects;
using OWPData.DataClasses;
using Plugins.HighlightPlus.Runtime.Scripts;
using SharedUI.Interface;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Utilities.Interface;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace LevelConstruct.Interactable.ItemInteractables.ItemPicker
{
    [RequireComponent(typeof(Collider))]
    [ExecuteInEditMode]
    public class ItemPicker : MonoBehaviour, IInteractable, IBillboardable, IExaminable, IHoverable,
        MMEventListener<LoadedManagerEvent>, IRequiresUniqueID, MMEventListener<ItemPickerEvent>
    {
        const string DefaultActionText = "Pick Item";
        public string uniqueID;

        [Header("Controls Help & Action Info")]
#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int actionId;
        public string actionText = "Pick";

        [Header("Picker Settings")] public int quantity = 1;

        [SerializeField] protected float interactionDistance = 2f;

        public UnityEvent onItemPicked;

        public ObjectiveObject objectiveOnPick;
        public bool incrementObjectiveOnPick = true;

        [FormerlySerializedAs("baseItemObject")]
        [FormerlySerializedAs("itemStack")]
#if ODIN_INSPECTOR && UNITY_EDITOR
        [OnValueChanged(nameof(AutoSyncExaminableFromItem), true)]
        [InlineButton(nameof(SyncFromItem), "Sync From Item")]
#endif
        public InventoryItem inventoryItem;

#if ODIN_INSPECTOR && UNITY_EDITOR
        [FoldoutGroup("Examination")]
        [InlineProperty]
        [HideLabel]
#endif
        [SerializeField]
        ExaminableObjectData examinableObjectData;

        [SerializeField] MMFeedbacks onPickFeedbacks;

        [Header("Restoration")] [SerializeField]
        bool canBeMoved = true; // Set false for items that shouldn't be pickable/movable

        public UnityEvent placedByPlayerEvent;

        List<string> _chemsFound;

        SceneObjectData _data;

        HighlightEffectController _highlightEffectController;

        bool _interactionComplete;

        // bool _isBeingDestroyed;
        // bool _isInRange;

        PickableManager _pickableManager;

        // [FormerlySerializedAs("preferredInventory")] [SerializeField]
        string _preferredInventoryName;

        IStatefulItemPicker _statefulPicker;
        List<string> _toolsFound;

        HighlightTrigger _trigger;

        public IStatefulItemPicker statefulPicker;

        public bool Stateful => inventoryItem is MyBaseItem myBaseItem && myBaseItem.IsStateful();

        public bool CanBeMoved => canBeMoved;

        public MoreMountains.InventoryEngine.Inventory TargetInventory { get; set; }


        void Awake()
        {
            _trigger = GetComponent<HighlightTrigger>();
            _statefulPicker = GetComponent<IStatefulItemPicker>();
            _highlightEffectController = GetComponent<HighlightEffectController>();
        }

        void Reset()
        {
            if (string.IsNullOrEmpty(uniqueID))
            {
                uniqueID = Guid.NewGuid().ToString();
#if UNITY_EDITOR
                Undo.RecordObject(this, "Assign uniqueID");
                EditorUtility.SetDirty(this);
#endif
            }
        }


        void Start()
        {
            // Wait for the PickableManager to finish loading before checking if this item is picked
            // StartCoroutine(InitializeAfterPickableManager());
            TargetInventory = GameObject.FindWithTag("PlayerCarryInventory")
                ?.GetComponent<MoreMountains.InventoryEngine.Inventory>();


            // if (TargetInventory == null) Debug.LogWarning("No inventory found in scene");
            if (string.IsNullOrEmpty(_preferredInventoryName) && TargetInventory != null)
                _preferredInventoryName = TargetInventory.name;
        }

        void OnEnable()
        {
            this.MMEventStartListening<ItemPickerEvent>();
            this.MMEventStartListening<LoadedManagerEvent>();
            if (_trigger == null) return;
        }

        void OnDisable()
        {
            this.MMEventStopListening<ItemPickerEvent>();
            this.MMEventStopListening<LoadedManagerEvent>();
            if (_trigger == null) return;
        }

        void OnDestroy()
        {
            enabled = false;
        }


        public string GetName()
        {
            if (inventoryItem != null) return inventoryItem.name;

            return "Unknown Item";
        }

        public Sprite GetIcon()
        {
            if (inventoryItem != null && inventoryItem.Icon != null) return inventoryItem.Icon;

            Debug.LogWarning("Item icon is not set for " + name);
            return null;
        }

        public string ShortBlurb()
        {
            return inventoryItem.ShortDescription;
        }

        public Sprite GetActionIcon()
        {
            return null; // No specific action icon for item picking
        }


        public string GetActionText()
        {
            return string.IsNullOrEmpty(actionText) ? DefaultActionText : actionText;
        }

        public void OnFinishExamining()
        {
            if (inventoryItem != null &&
                (examinableObjectData == null || string.IsNullOrEmpty(examinableObjectData.Id)))
            {
                BuildExaminableFrom(inventoryItem);
                if (examinableObjectData != null) examinableObjectData.Id = inventoryItem.ItemID;
            }

            ExaminationEvent.Trigger(ExaminableItemType.Pickable, examinableObjectData);
            if (examinableObjectData != null)
                BillboardEvent.Trigger(examinableObjectData.FromExaminableObjectData(), BillboardEventType.Update);
        }

        public bool ExaminableWithRuntimeTool(IRuntimeTool tool)
        {
            var identificationMode = inventoryItem.identificationMode;

            if (identificationMode == IdentificationMode.NeedsExaminationOnce &&
                tool is HandheldScannerToolPrefab) return true;

            return false;
        }

        public bool OnHoverStart(GameObject go)
        {
            if (inventoryItem == null) return true;

            var mode = inventoryItem.identificationMode;

            var alreadyKnown = ExaminationManager.Instance != null &&
                               ExaminationManager.Instance.HasTypeBeenExamined(inventoryItem.ItemID);

            var recognizable = mode == IdentificationMode.RecognizableOnSight;

            var showKnown = recognizable || alreadyKnown;

            var nameToShow = showKnown ? inventoryItem.ItemName : inventoryItem.UnknownName;
            var iconToShow = showKnown ? inventoryItem.Icon : ExaminationManager.Instance?.defaultUnknownIcon;

            var shortToShow = showKnown ? inventoryItem.ShortDescription : string.Empty;

            var icon = ExaminationManager.Instance?.iconRepository.pickupIcon;

            _data = new SceneObjectData(nameToShow, iconToShow, shortToShow, icon, "Pick Up");
            _data.Id = inventoryItem.ItemID;

            BillboardEvent.Trigger(_data, BillboardEventType.Show);
            if (actionId != 0)
                ControlsHelpEvent.Trigger(
                    ControlHelpEventType.Show, actionId, additionalInfoText:
                    string.IsNullOrEmpty(actionText) ? null : actionText);

            return true;
        }


        public bool OnHoverStay(GameObject go)
        {
            /* update */
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

            /* UI tooltip off*/
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


        public void Interact()
        {
            if (_pickableManager == null) _pickableManager = FindFirstObjectByType<PickableManager>();
            // If interaction key is still held
            if (_pickableManager.IsInteractPressed()) TryPickItem();
        }
        public void Interact(string param)
        {
            throw new NotImplementedException();
        }


        public void OnInteractionStart()
        {
            _statefulPicker.PlayLoopedFeedbacks();
        }
        public void OnInteractionEnd(string param)
        {
            _statefulPicker.StopLoopedFeedbacks();
        }


// Replace CanInteract() in ItemPicker with this version that handles nulls better

        public bool CanInteract()
        {
            // If not gated or not harvestable, can always interact
            var harvestableHelper = statefulPicker as HarvestableItemPickerHelper;
            if (harvestableHelper == null)
                return true;

            // If it's not a gated interaction, allow it
            if (!harvestableHelper.IsItemPickerGated())
                return true;

            var details = harvestableHelper.GetAppropriateGatedInteractionDetails();

            // ✅ If details are null, we can't interact through the gated system
            if (details == null)
            {
                Debug.LogError($"[ItemPicker] {gameObject.name} is gated but has no details configured!");
                AlertEvent.Trigger(
                    AlertReason.LackToolForInteraction,
                    "This item is not properly configured for interaction.",
                    "Configuration Error");

                return false;
            }

            // Check stamina first
            if (PlayerMutableStatsManager.Instance != null)
            {
                var currentStamina = PlayerMutableStatsManager.Instance.CurrentStamina;
                if (currentStamina < details.staminaCost)
                {
                    AlertEvent.Trigger(
                        AlertReason.NotEnoughStamina,
                        "You do not have enough stamina to harvest this item.",
                        "Insufficient Stamina");

                    return false;
                }
            }

            // If no requirements, can interact
            if (!details.requireTools && !details.requiresChemical)
                return true;

            // Check tools if required
            var hasTools = true;
            if (details.requireTools)
            {
                _toolsFound = harvestableHelper.HasToolsForHarvestInInventory();
                hasTools = _toolsFound != null && _toolsFound.Count > 0;

                if (!hasTools)
                    AlertEvent.Trigger(
                        AlertReason.LackToolForInteraction,
                        "You need the appropriate tools to harvest this item.",
                        "Lacking Necessary Tool");
            }

            // Check chemicals if required
            var hasChemicals = true;
            if (details.requiresChemical)
            {
                _chemsFound = harvestableHelper.HasChemicalsForHarvestInInventory();
                hasChemicals = _chemsFound != null && _chemsFound.Count > 0;

                if (!hasChemicals)
                    AlertEvent.Trigger(
                        AlertReason.LackToolForInteraction,
                        "You need the appropriate chemicals to harvest this item.",
                        "Lacking Necessary Chemicals");
            }

            // Both must be satisfied if both are required
            return hasTools && hasChemicals;
        }

        public bool IsInteractable()
        {
            return true;
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
        public void OnMMEvent(ItemPickerEvent eventType)
        {
            if (eventType.EventType == ItemPickerEvent.ItemPickerEventType.TriggerHighlight)
                if (eventType.ItemPickerUniqueID == uniqueID)
                    _highlightEffectController.SetHighlighted(true);
        }
        public void OnMMEvent(LoadedManagerEvent eventType)
        {
            if (eventType.ManagerType != ManagerType.All)
                return;

            _pickableManager = PickableManager.Instance;

            if (_pickableManager != null && _pickableManager.IsItemPicked(uniqueID))
                Destroy(gameObject);
        }

        public void StartExamining()
        {
        }

        public void StopExamining()
        {
        }

        public void OnInteractionEnd()
        {
            onItemPicked?.Invoke();
        }

        // Call this after scene load to restore moved position
        public void RestoreMovedPosition(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;

            var rb = GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true; // Keep it stationary until player interacts again
            // rb.linearVelocity = Vector3.zero;
            // rb.angularVelocity = Vector3.zero;
            Debug.Log($"Restored moved item {uniqueID} to position {position}");
        }
#if UNITY_EDITOR
        public IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            return AllRewiredActions.GetAllRewiredActions();
        }

#endif

        public bool PlayerCanPickUpAndMove()
        {
            if (PlayerMutableStatsManager.Instance == null || PickableManager.Instance == null)
            {
                Debug.LogError("Tryiing to pick up object before the Managers are initialized");
                return false;
            }

            var weightPlayerCanPickup = AttributesManager.Instance.Strength *
                                        PickableManager.Instance.weightAblePerStrength;


            return false;
        }

        protected void Initialize()
        {
            _pickableManager = PickableManager.Instance;
        }


        public void OnAfterDeserialize()
        {
            if (Application.isEditor && string.IsNullOrEmpty(uniqueID))
            {
                uniqueID = Guid.NewGuid().ToString();
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        bool HasStateRestriction()
        {
            if (inventoryItem is not MyBaseItem baseItem) return false;
            return baseItem.stateCategory != MyBaseItem.StateCategory.None;
        }

        // Replace TryPickItem() in ItemPicker with this fixed version

        void TryPickItem()
        {
            // ✅ Check if stateful picker attached
            statefulPicker = GetComponent<IStatefulItemPicker>();
            if (HasStateRestriction() && statefulPicker == null)
                Debug.LogWarning($"[ItemPicker] {name} is marked stateful but has no IStatefulItemPicker attached.");


            if (statefulPicker != null && statefulPicker.IsItemPickerGated())
                if (statefulPicker.GetGatedInteractionType() == GatedInteractionType.HarvesteableBiological)
                {
                    var harvestableHelper = statefulPicker as HarvestableItemPickerHelper;
                    if (harvestableHelper != null)
                    {
                        var details = harvestableHelper.GetAppropriateGatedInteractionDetails();

                        // ✅ Critical null check before triggering event
                        if (details == null)
                        {
                            Debug.LogError(
                                $"[ItemPicker] Cannot trigger gated interaction - no details found for {gameObject.name}");

                            AlertEvent.Trigger(
                                AlertReason.LackToolForInteraction,
                                "This item cannot be harvested right now.",
                                "Interaction Error");

                            return;
                        }

                        // ✅ POPULATE the lists by checking inventory
                        _chemsFound = harvestableHelper.HasChemicalsForHarvestInInventory();
                        _toolsFound = harvestableHelper.HasToolsForHarvestInInventory();

                        // Ensure lists aren't null
                        if (_chemsFound == null) _chemsFound = new List<string>();
                        if (_toolsFound == null) _toolsFound = new List<string>();

                        // ✅ Verify requirements are met before showing UI
                        var meetsRequirements = true;

                        if (details.requiresChemical && _chemsFound.Count == 0)
                        {
                            AlertEvent.Trigger(
                                AlertReason.LackToolForInteraction,
                                "You need the appropriate chemicals to harvest this item.",
                                "Lacking Necessary Chemicals");

                            meetsRequirements = false;
                        }

                        if (details.requireTools && _toolsFound.Count == 0)
                        {
                            AlertEvent.Trigger(
                                AlertReason.LackToolForInteraction,
                                "You need the appropriate tools to harvest this item.",
                                "Lacking Necessary Tool");

                            meetsRequirements = false;
                        }

                        if (!meetsRequirements)
                            return;

                        ControlsHelpEvent.Trigger(ControlHelpEventType.Hide, actionId);

                        // All checks passed, show UI
                        GatedHarvestableInteractionEvent.Trigger(
                            GatedInteractionEventType.TriggerGateUI,
                            details,
                            uniqueID,
                            _chemsFound,
                            _toolsFound
                        );
                    }

                    return;
                }

            // Normal pickup flow
            var itemWeight = 0f;
            try
            {
                var myGameItem = inventoryItem as MyBaseItem;
                itemWeight = myGameItem.weight * quantity;
            }
            catch (Exception e)
            {
                Debug.LogWarning("InventoryItem is not a MyBaseItem, cannot get weight. " + e);
            }

            var preferredInventory = MoreMountains.InventoryEngine.Inventory.FindInventory(
                _preferredInventoryName, GlobalInventoryManager.Instance.playerId);

            if (preferredInventory == null)
            {
                Debug.LogWarning("Preferred inventory not found: " + _preferredInventoryName);
                return;
            }

            var weightOfPlayerInventory = GlobalInventoryManager.Instance.GetTotalWeightInDirigible();
            var maxWeightOfPlayerInventory = GlobalInventoryManager.Instance.GetMaxWeightOfPlayerCarry();

            if (weightOfPlayerInventory + itemWeight > maxWeightOfPlayerInventory)
            {
                AlertEvent.Trigger(
                    AlertReason.InventoryFull,
                    "You cannot carry any more items. Drop something first.",
                    "Inventory Full");

                return;
            }

            var activeSceneName = SceneManager.GetActiveScene().name;
            _pickableManager.AddPickedItem(uniqueID, true, activeSceneName);
            // _pickableManager.AddPickedItemTypeIf(inventoryItem.ItemID);

            // Trigger item picked event
            PickableEvent.Trigger(PickableEventType.Picked, uniqueID, transform, inventoryItem.ItemID);
            inventoryItem.Pick("Player1");
            MMInventoryEvent.Trigger(
                MMInventoryEventType.Pick, null, inventoryItem.TargetInventoryName,
                inventoryItem, quantity, 0, GlobalInventoryManager.Instance.playerId);

            if (objectiveOnPick != null && incrementObjectiveOnPick)
                ObjectiveEvent.Trigger(
                    objectiveOnPick.objectiveId, ObjectiveEventType.IncrementObjectiveProgress, progressMade: 1);
            else if (objectiveOnPick != null)
                ObjectiveEvent.Trigger(objectiveOnPick.objectiveId, ObjectiveEventType.ObjectiveCompleted);

            onItemPicked?.Invoke();
            onPickFeedbacks?.PlayFeedbacks();
            BillboardEvent.Trigger(examinableObjectData.FromExaminableObjectData(), BillboardEventType.Hide);
            ControlsHelpEvent.Trigger(
                ControlHelpEventType.Hide, actionId,
                string.IsNullOrEmpty(actionText) ? null : actionText);

            Destroy(gameObject);
        }

        protected IEnumerator InitializeAfterPickableManager()
        {
            // Wait a frame to ensure PickableManager has initialized
            yield return null;

            if (_pickableManager != null)
                if (_pickableManager.IsItemPicked(uniqueID))
                    Destroy(gameObject);
        }

        // Add this public method to ItemPicker class (anywhere in the class)

        /// <summary>
        ///     Public method to pick up the item directly, bypassing gated interactions
        ///     Called by GatedInteractionManager when user clicks "Pick Up" button
        /// </summary>
        public void PickupItemDirect()
        {
            if (statefulPicker == null)
            {
                statefulPicker = GetComponent<IStatefulItemPicker>();
                if (HasStateRestriction() && statefulPicker == null)
                    Debug.LogWarning(
                        $"[ItemPicker] {name} is marked stateful but has no IStatefulItemPicker attached.");
            }

            if (statefulPicker != null && !statefulPicker.CanBePicked())
            {
                Debug.Log($"[ItemPicker] Cannot pick {name}, invalid state.");
                AlertEvent.Trigger(
                    AlertReason.ItemNotReady,
                    "This item cannot be picked up right now.",
                    "Item Not Ready"
                );

                return;
            }

            var itemWeight = 0f;
            try
            {
                var myGameItem = inventoryItem as MyBaseItem;
                itemWeight = myGameItem.weight * quantity;
            }
            catch (Exception e)
            {
                Debug.LogWarning("InventoryItem is not a MyBaseItem, cannot get weight. " + e);
            }

            var preferredInventory = MoreMountains.InventoryEngine.Inventory.FindInventory(
                _preferredInventoryName, GlobalInventoryManager.Instance.playerId);

            if (preferredInventory == null)
            {
                Debug.LogWarning("Preferred inventory not found: " + _preferredInventoryName);
                return;
            }

            var weightOfPlayerInventory = GlobalInventoryManager.Instance.GetTotalWeightInDirigible();
            var maxWeightOfPlayerInventory = GlobalInventoryManager.Instance.GetMaxWeightOfPlayerCarry();

            if (weightOfPlayerInventory + itemWeight > maxWeightOfPlayerInventory)
            {
                AlertEvent.Trigger(
                    AlertReason.InventoryFull,
                    "You cannot carry any more items. Drop something first.",
                    "Inventory Full");

                return;
            }

            var activeSceneName = SceneManager.GetActiveScene().name;
            if (_pickableManager == null) _pickableManager = FindFirstObjectByType<PickableManager>();
            _pickableManager.AddPickedItem(uniqueID, true, activeSceneName);

            // Trigger item picked event
            PickableEvent.Trigger(PickableEventType.Picked, uniqueID, transform, inventoryItem.ItemID);
            inventoryItem.Pick("Player1");
            MMInventoryEvent.Trigger(
                MMInventoryEventType.Pick, null, inventoryItem.TargetInventoryName,
                inventoryItem, quantity, 0, GlobalInventoryManager.Instance.playerId);


            if (objectiveOnPick != null && objectiveOnPick.objectiveId != null && incrementObjectiveOnPick)
                ObjectiveEvent.Trigger(
                    objectiveOnPick.objectiveId, ObjectiveEventType.IncrementObjectiveProgress, progressMade: 1);
            else if (objectiveOnPick != null)
                ObjectiveEvent.Trigger(objectiveOnPick.objectiveId, ObjectiveEventType.ObjectiveCompleted);

            onItemPicked?.Invoke();
            onPickFeedbacks?.PlayFeedbacks();
            BillboardEvent.Trigger(examinableObjectData.FromExaminableObjectData(), BillboardEventType.Hide);

            ControlsHelpEvent.Trigger(
                ControlHelpEventType.Hide, actionId, string.IsNullOrEmpty(actionText) ? null : actionText);

            Destroy(gameObject);
        }

        protected void BuildExaminableFrom(InventoryItem item)
        {
            if (item == null) return;

            var nameToShow = item.ItemName;
            var iconToShow = item.Icon;
            var shortToShow = item.ShortDescription;
            var fullDescription = item.Description;
            var mode = item
                .identificationMode; // RecognizableOnSight, NeedsScanOnce, NeedsExaminationOnce, NeedsBiologicalAnalysis...

            var unknownItcon = ExaminationManager.Instance?.defaultUnknownIcon;

            var unknownName = item.UnknownName;
            var unknownShortBlurb = string.Empty;

            examinableObjectData = new ExaminableObjectData(
                nameToShow,
                iconToShow,
                shortToShow,
                null,
                DefaultActionText,
                fullDescription,
                mode,
                unknownItcon,
                unknownShortBlurb,
                unknownName
            );
        }
        public void OnPlacedByPlayer()
        {
            placedByPlayerEvent?.Invoke();
        }

#if UNITY_EDITOR

        // Odin auto-callback when itemTypeMined changes in Inspector
        protected void AutoSyncExaminableFromItem()
        {
            // Only run in editor, not at runtime
            if (!Application.isEditor) return;
            if (inventoryItem == null) return;

            BuildExaminableFrom(inventoryItem);

            // Mark scene dirty safely on the next editor tick (avoids "SetDirty during serialization")
            EditorApplication.delayCall += () =>
            {
                if (this != null)
                {
                    EditorUtility.SetDirty(this);
                    if (!Application.isPlaying)
                        EditorSceneManager.MarkSceneDirty(gameObject.scene);
                }
            };
        }

        // Clickable Odin button fallback
        protected void SyncFromItem()
        {
            if (inventoryItem == null) return;
            BuildExaminableFrom(inventoryItem);

            EditorUtility.SetDirty(this);
            if (!Application.isPlaying)
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif
    }
}
