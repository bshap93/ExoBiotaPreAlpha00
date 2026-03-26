using System;
using System.Collections;
using System.Collections.Generic;
using Dirigible.Input;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using FirstPersonPlayer.Tools.ItemObjectTypes.CompositeObjects;
using Gameplay.Events;
using Helpers.Events;
using Helpers.Events.Gated;
using Helpers.ScriptableObjects.Gated;
using Helpers.StaticHelpers;
using HighlightPlus;
using INab.Dissolve;
using Inventory;
using Manager.SceneManagers.Pickable;
using MoreMountains.Feedbacks;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using Objectives.ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace LevelConstruct.Interactable.ItemInteractables
{
    [RequireComponent(typeof(ItemPicker.ItemPicker))]
    public class HarvestableItemPickerHelper : MonoBehaviour, IStatefulItemPicker,
        MMEventListener<GatedHarvestableInteractionEvent>
    {
        public enum HarvestableState
        {
            Undetatched,
            // DetachedFresh,
            HadCatalystApplied,
            Dissolved,
            Picked
        }


        [Header("Objective Progression")] [SerializeField]
        ObjectiveObject objectiveProgressOnCatalyst;
        [SerializeField] ObjectiveObject objectiveProgressOnSolvent;

        [Header("Feedbacks")] [SerializeField] MMFeedbacks catalystAppliedFeedback;
        [SerializeField] MMFeedbacks solventAppliedFeedback;
        [SerializeField] MMFeedbacks loopedInteractionFeedbacks;
        [SerializeField] MMFeedbacks startInteractionFeedbacks;

#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int interactActionId;

        [SerializeField] MeshRenderer coreMeshRenderer;

        [FormerlySerializedAs("rhizomicCoreState")]
        public HarvestableState harvestableState;

        public HarvestableState DefaultState = HarvestableState.Undetatched;

        public Material undetatchedMaterial;
        public Material detachedFreshMaterial; // Same as undetatched
        public Material hadCatalystAppliedMaterial; // Reddish, darker
        public Material dissolvedMaterial; // Uses Dissolver shader

        public Dissolver dissolver;

        [Header("Gated Interaction Details")] [ToggleLeft] [LabelText("Is Gated Interaction?")]
        public bool isGatedInteraction;

        [ShowIf(nameof(isGatedInteraction))] [LabelText("Gated Interaction Details")] [SerializeField]
        public List<GatedHarvestalbeInteractionDetails> gatedInteractionDetails;

        [ShowIf(nameof(isGatedInteraction))] [SerializeField] [LabelText("Gated Interaction Type")]
        GatedInteractionType gatedInteractionType;

        public int innerCoreYield;

        public GameObject innerCoreObjectPicker;
        public InventoryItem innerCoreInventoryItem;

        [FormerlySerializedAs("coreItemObject")] [FormerlySerializedAs("inventoryItem")]
        public OuterCoreItemObject outerCoreItemObject;

        public float timeToDissolve = 2.0f;

        OuterCoreItemObject.CoreReactivity _coreReactivity;

        bool _initialized;
        ItemPicker.ItemPicker _itemPicker;

        int _numInnerCoresHarvested;


        StatefulPickableManager _stateManager;

        void Awake()
        {
            _itemPicker = gameObject.GetComponent<ItemPicker.ItemPicker>();

            // ✅ Subscribe to the placed event
            if (_itemPicker != null) _itemPicker.placedByPlayerEvent.AddListener(OnPlacedByPlayer);
        }

        void Start()
        {
            if (!_initialized)
                StartCoroutine(InitializeAfterStateManagerLoaded());
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public int GetStateEnumIndex()
        {
            return (int)harvestableState;
        }
        public void SetStateEnumIndex(int index)
        {
            if (index < 0 || index >= Enum.GetValues(typeof(HarvestableState)).Length)
            {
                Debug.LogError("RhizomicCorePicker: Invalid state index");
                return;
            }

            var newState = (HarvestableState)index;
            AdvanceState(newState);
        }
        public void SetStateToDefault()
        {
            harvestableState = DefaultState;
            ApplyMaterials(harvestableState);
        }
        public bool IsItemPickerGated()
        {
            return isGatedInteraction;
        }
        public GatedInteractionType GetGatedInteractionType()
        {
            return gatedInteractionType;
        }
        public void PlayLoopedFeedbacks()
        {
            loopedInteractionFeedbacks?.PlayFeedbacks();
        }
        public void StopLoopedFeedbacks()
        {
            loopedInteractionFeedbacks?.StopFeedbacks();
        }

        public bool CanBePicked()
        {
            // For example: disallow picking until DetachedFresh or later
            return harvestableState == HarvestableState.Undetatched
                   || harvestableState ==
                   HarvestableState.Dissolved || harvestableState == HarvestableState.Picked;
        }
        // ✅ Add this method to handle the events
        public void OnMMEvent(GatedHarvestableInteractionEvent eventType)
        {
            if (_itemPicker == null)
            {
                _itemPicker = gameObject.GetComponent<ItemPicker.ItemPicker>();
                if (_itemPicker == null)
                    Debug.LogError("ItemPicker.ItemPicker: ItemPicker not found");
            }

            if (eventType.EventType == GatedInteractionEventType.StartInteraction)
                // Play feedback
                PlayLoopedFeedbacks();
            else if (eventType.EventType == GatedInteractionEventType.CompleteInteraction)
                // Stop looped feedbacks
                StopLoopedFeedbacks();


            // Only process events for THIS specific item
            if (eventType.SubjectUniqueID != _itemPicker.uniqueID)
                return;

            if (eventType.EventType == GatedInteractionEventType.CompleteInteraction)
            {
                Debug.Log($"[HarvestableItemPickerHelper] CompleteInteraction received for {gameObject.name}");

                // Determine what to do based on the interaction type
                var details = eventType.Details;

                if (details.harvestableInteractionType == HarvestableInteractionType.CatalyzeWithCatalyst)
                {
                    Debug.Log("[HarvestableItemPickerHelper] Applying catalyst and changing state");

                    // Advance to HadCatalystApplied state
                    AdvanceState(HarvestableState.HadCatalystApplied);

                    // Trigger objective if set
                    if (objectiveProgressOnCatalyst != null)
                        ObjectiveEvent.Trigger(
                            objectiveProgressOnCatalyst.objectiveId,
                            ObjectiveEventType.IncrementObjectiveProgress,
                            NotifyType.Regular, 1);


                    var bestChemID = details.mostEfficientChemicalID;
                    InventoryHelperCommands.RemovePlayerItem(bestChemID);


                    // Play feedback
                    catalystAppliedFeedback?.PlayFeedbacks();
                }
                else if (details.harvestableInteractionType == HarvestableInteractionType.DissolveWithSolvent)
                {
                    Debug.Log("[HarvestableItemPickerHelper] Applying solvent and dissolving");

                    // Advance to Dissolved state
                    AdvanceState(HarvestableState.Dissolved);

                    // Trigger objective if set
                    if (objectiveProgressOnSolvent != null)
                        ObjectiveEvent.Trigger(
                            objectiveProgressOnSolvent.objectiveId,
                            ObjectiveEventType.IncrementObjectiveProgress,
                            NotifyType.Regular, 1);

                    // Dissolve visual effects
                    if (dissolver != null)
                    {
                        dissolver.FindMaterials();
                        dissolver.Dissolve();
                    }

                    var bestChemID = details.mostEfficientChemicalID;
                    InventoryHelperCommands.RemovePlayerItem(bestChemID);


                    // Play feedback
                    solventAppliedFeedback?.PlayFeedbacks();
                }
            }
        }
#if UNITY_EDITOR
        public IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            return AllRewiredActions.GetAllRewiredActions();
        }

#endif

        IEnumerator InitializeAfterStateManagerLoaded()
        {
            // Wait for StatefulPickableManager to be ready
            while (StatefulPickableManager.Instance == null) yield return null;

            _stateManager = StatefulPickableManager.Instance;

            // Wait one more frame to ensure Load() has completed
            yield return null;

            // _coreReactivity = outerCoreItemObject.coreReactivity;

            _itemPicker = GetComponent<ItemPicker.ItemPicker>();

            if (_itemPicker == null)
            {
                Debug.LogError("RhizomicCorePicker: No ItemPicker component found on the GameObject.");
                yield break;
            }

            // Create per-instance material copies
            var mats = coreMeshRenderer.materials; // Unity automatically creates instances

            // Clear and reassign dissolver materials with the instanced copies
            if (dissolver != null)
            {
                dissolver.materials.Clear();
                dissolver.materials.AddRange(mats);
            }

            // Load or initialize state
            HarvestableState stateToApply;
            if (_stateManager.TryGetState(_itemPicker.uniqueID, out HarvestableState savedState))
            {
                // Saved state exists, use it
                stateToApply = savedState;
                _stateManager.SetState(_itemPicker.uniqueID, stateToApply);
            }
            else
            {
                // No saved state exists, use default
                stateToApply = DefaultState;
                _stateManager.SetState(_itemPicker.uniqueID, DefaultState);
            }

            if (stateToApply == HarvestableState.Undetatched)
            {
                _itemPicker.actionText = "To Pick Up";
            }
            else if (stateToApply == HarvestableState.HadCatalystApplied)
            {
                _itemPicker.actionId = interactActionId;
                _itemPicker.actionText = "To Apply Solvent";
            }
            else if (stateToApply == HarvestableState.Dissolved)
            {
                // no action needed
            }
            else if (stateToApply == HarvestableState.Picked)
            {
                _itemPicker.actionId = interactActionId;
                _itemPicker.actionText = "To Apply Catalyst";
            }

            // Apply the correct state
            ApplyMaterials(stateToApply);
            _initialized = true;
        }

        void AdvanceState(HarvestableState newState)
        {
            _stateManager.SetState(_itemPicker.uniqueID, newState);
            ApplyMaterials(newState);
            StartCoroutine(
                ApplyAdvancedStateCoroutine(newState)
            );
        }

        void ApplyMaterials(HarvestableState state)
        {
            switch (state)
            {
                case HarvestableState.Undetatched:
                    harvestableState = HarvestableState.Undetatched;
                    // Create instance of material before assigning
                    coreMeshRenderer.material = new Material(undetatchedMaterial);
                    UpdateDissolverMaterials();
                    if (innerCoreObjectPicker) innerCoreObjectPicker.SetActive(false);
                    break;

                case HarvestableState.HadCatalystApplied:
                    harvestableState = HarvestableState.HadCatalystApplied;
                    coreMeshRenderer.material = new Material(hadCatalystAppliedMaterial);
                    UpdateDissolverMaterials();
                    if (innerCoreObjectPicker) innerCoreObjectPicker.SetActive(false);
                    break;
                case HarvestableState.Dissolved:
                    harvestableState = HarvestableState.Dissolved;
                    coreMeshRenderer.material = new Material(dissolvedMaterial);
                    UpdateDissolverMaterials();
                    StartCoroutine(
                        ApplyAdvancedStateCoroutine(HarvestableState.Dissolved)
                    );

                    break;
                default:
                    Debug.LogError("RhizomicCorePicker: Unknown state");
                    break;
            }
        }

        // Helper method to update dissolver with current instanced materials
        void UpdateDissolverMaterials()
        {
            if (dissolver != null)
            {
                dissolver.materials.Clear();
                dissolver.materials.AddRange(coreMeshRenderer.materials);
            }
        }

        IEnumerator ApplyAdvancedStateCoroutine(HarvestableState state)
        {
            // If we have an innerCoreObjectPicker assigned, and we are in Dissolved state, swap to that item
            if (state == HarvestableState.Dissolved && innerCoreObjectPicker != null &&
                innerCoreInventoryItem != null && _numInnerCoresHarvested < innerCoreYield)
            {
                var originalPosition = transform.position;
                var positionToUse = originalPosition + new Vector3(0, 0.5f, 0);
                // Instantiate the inner core object in the same position and rotation
                var newCore = Instantiate(
                    innerCoreObjectPicker, positionToUse, transform.rotation, transform.parent);

                var newCorePicker = newCore.GetComponent<ItemPicker.ItemPicker>();
                if (newCorePicker != null) newCorePicker.uniqueID = Guid.NewGuid().ToString();

                _numInnerCoresHarvested++;
                if (dissolver != null)
                {
                    dissolver.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                    dissolver.gameObject.GetComponent<MeshCollider>().enabled = false;
                    dissolver.gameObject.GetComponent<HighlightEffect>().enabled = false;
                }

                newCore.gameObject.SetActive(true);

                PickableEvent.Trigger(
                    PickableEventType.PlacedItemCameToRest, newCorePicker.uniqueID, newCore.transform,
                    innerCoreInventoryItem.ItemID);

                // Dissolve the outer core

                if (dissolver != null)
                    dissolver.Dissolve();

                yield return new WaitForSeconds(timeToDissolve);


                gameObject.SetActive(false);
                // Trigger item picked event
                PickableEvent.Trigger(
                    PickableEventType.Picked, _itemPicker.UniqueID, transform, outerCoreItemObject.ItemID);
                // dissolver.Materialize();
            }

            yield return null;
        }

        public void ApplyCatalyst(ReagentType reagentType)
        {
            var coreGradesDissolved = reagentType.coreGradesAffected;

            if (coreGradesDissolved.Contains(_coreReactivity))
                // Change state to HadCatalystApplied
            {
                AdvanceState(HarvestableState.HadCatalystApplied);
                if (objectiveProgressOnCatalyst != null)
                    ObjectiveEvent.Trigger(
                        objectiveProgressOnCatalyst.objectiveId, ObjectiveEventType.IncrementObjectiveProgress,
                        NotifyType.Regular, 1);

                catalystAppliedFeedback.PlayFeedbacks();
            }
        }
        public void ApplySolvent(ReagentType reagentType)
        {
            var coreGradesDissolved = reagentType.coreGradesAffected;

            if (coreGradesDissolved.Contains(_coreReactivity))
                // Dissolve the core
            {
                AdvanceState(HarvestableState.Dissolved);

                if (objectiveProgressOnSolvent != null)
                    ObjectiveEvent.Trigger(
                        objectiveProgressOnSolvent.objectiveId, ObjectiveEventType.IncrementObjectiveProgress,
                        NotifyType.Regular, 1);

                if (dissolver != null)
                {
                    dissolver.FindMaterials();
                    dissolver.Dissolve();
                }

                solventAppliedFeedback.PlayFeedbacks();

                // StartCoroutine(WaitThenResetDissolver());
            }
        }

        public void OnPlacedByPlayer()
        {
            _stateManager = StatefulPickableManager.Instance;
            _stateManager.SetState(_itemPicker.UniqueID, HarvestableState.Picked);

            // ✅ Update the action text for the Picked state
            _itemPicker.actionId = interactActionId;
            _itemPicker.actionText = "To Apply Catalyst";

            // ✅ Update the internal state variable
            harvestableState = HarvestableState.Picked;

            // ✅ Mark as initialized so the coroutine doesn't run and overwrite our settings
            _initialized = true;

            Debug.Log(
                $"[HarvestableItemPickerHelper] OnPlacedByPlayer: Setting state to Picked for {_itemPicker.UniqueID}");
        }

        // TODO: Where was this used?
        IEnumerator WaitThenResetDissolver()
        {
            yield return new WaitForSeconds(timeToDissolve - .01f);
            MaterialsEvent.Trigger(MaterialEventType.DissolvableReset, dissolvedMaterial.name);
        }
        public GatedHarvestalbeInteractionDetails GetAppropriateGatedInteractionDetails()
        {
            // Guard: Check if we have any details at all
            if (gatedInteractionDetails == null || gatedInteractionDetails.Count == 0)
            {
                Debug.LogError(
                    $"[HarvestableItemPickerHelper] {gameObject.name} has no gatedInteractionDetails assigned!");

                return null;
            }

            // Try to match based on current state
            foreach (var details in gatedInteractionDetails)
            {
                if (details == null) continue; // Skip null entries

                // Match state to interaction type
                // if (harvestableState == HarvestableState.Undetatched &&
                //     details.harvestableInteractionType == HarvestableInteractionType.CatalyzeWithCatalyst)
                // {
                //     Debug.Log("[HarvestableItemPickerHelper] Returning Catalyst details for Undetached state");
                //     return details;
                // }

                if ((harvestableState == HarvestableState.Undetatched || harvestableState == HarvestableState.Picked ||
                     harvestableState == HarvestableState.HadCatalystApplied) &&
                    details.harvestableInteractionType == HarvestableInteractionType.DissolveWithSolvent)
                {
                    Debug.Log("[HarvestableItemPickerHelper] Returning Solvent details for CatalystApplied state");
                    return details;
                }
            }

            // Fallback: return first valid entry
            Debug.LogWarning(
                $"[HarvestableItemPickerHelper] No matching details for state {harvestableState}, using first available");

            return gatedInteractionDetails[0];
        }
        public List<string> HasChemicalsForHarvestInInventory()
        {
            var details = GetAppropriateGatedInteractionDetails();
            if (details == null || !details.requiresChemical)
                return new List<string>();

            var possibleChemicals = details.requiredChemicalIDs;
            var foundChemicals = new List<string>();

            var playerInventory = GlobalInventoryManager.Instance.playerInventory;

            foreach (var chemID in possibleChemicals)
            {
                var itemInInventory = GetItemByID(chemID, playerInventory);
                if (itemInInventory != null)
                    foundChemicals.Add(chemID);
            }

            return foundChemicals;
        }
        public List<string> HasToolsForHarvestInInventory()
        {
            var details = GetAppropriateGatedInteractionDetails();
            if (details == null || !details.requireTools)
                return new List<string>();

            var possibleTools = details.requiredToolIDs;
            var foundTools = new List<string>();

            var playerInventory = GlobalInventoryManager.Instance.playerInventory;

            foreach (var toolID in possibleTools)
            {
                var itemInInventory = GetItemByID(toolID, playerInventory);
                if (itemInInventory != null)
                    foundTools.Add(toolID);
            }

            return foundTools;
        }

        MyBaseItem GetItemByID(string itemID, MoreMountains.InventoryEngine.Inventory inventory)
        {
            foreach (var item in inventory.Content)
            {
                if (item == null) continue;
                if (item.ItemID == itemID)
                {
                    // Option 1: Cast to MyBaseItem if your inventory actually stores those
                    if (item is MyBaseItem myBaseItem)
                        return myBaseItem;

                    // Option 2: Reload the definition from Resources
                    var def = Resources.Load<MyBaseItem>($"Items/{itemID}");
                    if (def != null)
                        return def;

                    Debug.LogWarning($"Item '{itemID}' found in inventory but not in Resources/Items/");
                    return null;
                }
            }

            return null;
        }
    }
}
