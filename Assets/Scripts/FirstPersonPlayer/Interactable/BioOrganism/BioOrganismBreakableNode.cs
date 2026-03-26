using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FirstPersonPlayer.Combat.Player.ScriptableObjects;
using FirstPersonPlayer.Interface;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.Events;
using Helpers.Events.Domains.Player.Events;
using Helpers.Events.Gated;
using Helpers.ScriptableObjects.Gated;
using HighlightPlus;
using Inventory;
using Manager;
using Manager.ProgressionMangers;
using Manager.SceneManagers;
using Manager.UI;
using MoreMountains.Feedbacks;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using RayFire;
using SharedUI.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Interactable.BioOrganism
{
    [DisallowMultipleComponent]
    public class BioOrganismBreakableNode : BioOrganismBase, IInteractable,
        MMEventListener<GatedBreakableInteractionEvent>, IGatedInteractable, IBreakable
    {
        [FormerlySerializedAs("gatedInteractionDetails")] [SerializeField]
        GatedBreakableInteractionDetails gatedBreakableInteractionDetails;

        // [SerializeField] HatchetBreakable hatchetBreakable;

#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int breakActionId;

        [SerializeField] float interactionDistance = 2f;

        [SerializeField] MMFeedbacks loopedInteractionFeedbacks;
        [SerializeField] MMFeedbacks startInteractionFeedbacks;

        [Header("IBreakable Settings")]
        [FormerlySerializedAs("hitsToBreak")]
        [Header("Break Settings")]
        [Tooltip("How many successful hatchet hits until this is destroyed.")]
        public int defaultHitsToBreak = 2;

        [Tooltip("Minimum tool power required to count as a successful hit.")]
        public int hardness = 1;

        [Tooltip("If set, destroy this root instead of just this component's GameObject.")]
        public GameObject destroyRoot;

        [Tooltip("If true, Destroy() the object. If false, just disable renderers/colliders.")]
        public bool destroyGameObject = true;

        [Header("FX")] public MMFeedbacks onHitFeedbacks;

        public MMFeedbacks onBreakFeedbacks;
        public GameObject hitParticles;
        public GameObject breakParticles;

        // [SerializeField] BioOrganismBreakableNode bioOrganismNode;


        [FormerlySerializedAs("_rf")] [SerializeField]
        RayfireRigid rf;
        bool _hasBeenBroken;

        HighlightEffect _highlight; // cache HighlightEffect if present

        int _hitCount;
        bool _isBroken; // Prevent multiple breaks
        bool _isProcessingInteraction;
        List<string> _toolsFound;
        protected override void Awake()
        {
            base.Awake();

            if (string.IsNullOrEmpty(uniqueID))
                uniqueID = Guid.NewGuid().ToString();


            _highlight = GetComponent<HighlightEffect>();
            rf = GetComponent<RayfireRigid>();


            rf.demolitionEvent.LocalEvent += OnDemolished;
        }

        void Start()
        {
            StartCoroutine(InitializeAfterDestructableManager());
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            this.MMEventStartListening();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            this.MMEventStopListening();
        }

        public IEnumerator InitializeAfterDestructableManager()
        {
            // Wait one frame so core managers come up
            yield return null;

            // Despawn if already destroyed in this save
            if (DestructableManager.Instance != null &&
                DestructableManager.Instance.IsDestroyed(uniqueID))
                Destroy(gameObject);
        }
        public bool CanBeDamagedBy(int toolPower, int strength)
        {
            return toolPower >= hardness;
        }


        public void PlayHitFx(Vector3 hitPoint, Vector3 hitNormal)
        {
            onHitFeedbacks?.PlayFeedbacks(transform.position);

            if (hitParticles)
            {
                var fx = Instantiate(hitParticles, hitPoint, Quaternion.LookRotation(hitNormal));
                Destroy(fx, 2f);
            }

            // Highlight Plus Hit FX
            if (_highlight != null) _highlight.HitFX(); // plays the configured hit effect
        }
        public void ApplyHit(int toolPower, Vector3 hitPoint, Vector3 hitNormal,
            HitType hitType = HitType.Normal, PlayerAttack attack = null)
        {
            var attrMgr = AttributesManager.Instance;
            // Prevent breaking if already broken
            if (_isBroken)
            {
                Debug.LogWarning($"HatchetBreakable [{uniqueID}]: Already broken, ignoring hit");
                return;
            }

            if (!CanBeDamagedBy(toolPower, 0))
            {
                PlayHitFx(hitPoint, hitNormal);
                return;
            }

            var actualHitsToBreak = defaultHitsToBreak;

            switch (attrMgr.Strength)
            {
                case 1:
                    break;
                default:
                    // 4/5^(level-1) of the hits rounded up
                    actualHitsToBreak = Mathf.RoundToInt(defaultHitsToBreak * Mathf.Pow(0.8f, attrMgr.Strength - 1));
                    actualHitsToBreak = Mathf.Max(1, actualHitsToBreak);
                    break;
            }

            _hitCount++;
            PlayHitFx(hitPoint, hitNormal);

            if (_hitCount < actualHitsToBreak) return;

            _isBroken = true;

            // break FX
            onBreakFeedbacks?.PlayFeedbacks(transform.position);
            if (breakParticles)
            {
                var fx2 = Instantiate(breakParticles, transform.position, Quaternion.identity);
                Destroy(fx2, 3f);
            }

            if (!string.IsNullOrEmpty(uniqueID))
                DestructableEvent.Trigger(DestructableEventType.Destroyed, uniqueID, transform);

            var root = destroyRoot != null ? destroyRoot : gameObject;
            if (destroyGameObject)
            {
                foreach (var col in root.GetComponentsInChildren<Collider>(true)) col.enabled = false;
                foreach (var r in root.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
                if (rf != null)
                    rf.Demolish();
                else
                    Destroy(root, 0.05f);
            }
            else
            {
                foreach (var col in root.GetComponentsInChildren<Collider>(true)) col.enabled = false;
                foreach (var r in root.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
                enabled = false;
            }

            ControlsHelpEvent.Trigger(ControlHelpEventType.ShowUseThenHide, 54);
        }
        public List<string> HasToolForInteractionInInventory()
        {
            var possibleTools = gatedBreakableInteractionDetails.requiredToolIDs;
            var foundTools = new List<string>();

            var playerInventory =
                GlobalInventoryManager.Instance.playerInventory;

            var equipmentInventory =
                GlobalInventoryManager.Instance.equipmentInventory;

            foreach (var toolID in possibleTools)
            {
                var itemInInventory = GetItemByID(toolID, playerInventory);
                if (itemInInventory != null) foundTools.Add(toolID);
            }

            foreach (var toolID in possibleTools)
            {
                var itemInInventory = GetItemByID(toolID, equipmentInventory);
                if (itemInInventory != null && !foundTools.Contains(toolID))
                    foundTools.Add(toolID);
            }

            return foundTools;
        }

        public MyBaseItem GetItemByID(string itemID, MoreMountains.InventoryEngine.Inventory inventory)
        {
            foreach (var item in inventory.Content)
            {
                if (item == null) continue;
                if (item.ItemID == itemID)
                {
                    // Option 1: Cast to MyBaseItem if your inventory actually stores those
                    if (item is MyBaseItem myBaseItem)
                        return myBaseItem;

                    // Option 2 (recommended): Reload the definition from Resources
                    var def = Resources.Load<MyBaseItem>($"Items/{itemID}");
                    if (def != null)
                        return def;

                    Debug.LogWarning($"Item '{itemID}' found in inventory but not in Resources/Items/");
                    return null;
                }
            }

            return null;
        }

        public bool CanInteract(out GatedInteractionManager.ReasonWhyCannotInteract reason)
        {
            var currentStamina = PlayerMutableStatsManager.Instance.CurrentStamina;
            if (currentStamina - gatedBreakableInteractionDetails.staminaCost < 0)
            {
                reason = GatedInteractionManager.ReasonWhyCannotInteract.NotEnoughStamina;
                AlertEvent.Trigger(
                    AlertReason.NotEnoughStamina,
                    "You do not have enough stamina to perform this action.",
                    "Not Enough Stamina");

                return false;
            }

            if (!gatedBreakableInteractionDetails.requireTools)
            {
                reason = GatedInteractionManager.ReasonWhyCannotInteract.None;
                return true;
            }

            _toolsFound = HasToolForInteractionInInventory();
            if (_toolsFound.Count == 0)
            {
                reason = GatedInteractionManager.ReasonWhyCannotInteract.LackingNecessaryTool;
                return false;
            }

            reason = GatedInteractionManager.ReasonWhyCannotInteract.None;
            return true;
        }
        public void Interact()
        {
            if (_hasBeenBroken)
            {
                Debug.LogWarning($"BioOrganismBreakableNode [{uniqueID}]: Cannot interact, already broken");
                return;
            }

            if (!CanInteract(out var reason))
            {
                if (reason == GatedInteractionManager.ReasonWhyCannotInteract.LackingNecessaryTool)
                    AlertEvent.Trigger(
                        AlertReason.LackToolForInteraction, "You need the appropriate axe to destroy this organism.",
                        "Lacking Necessary Tool");
                else if (reason == GatedInteractionManager.ReasonWhyCannotInteract.NotEnoughStamina)
                    AlertEvent.Trigger(
                        AlertReason.NotEnoughStamina, "You do not have enough stamina to perform this action.",
                        "Not Enough Stamina");

                return;
            }


            // Equip best tool
            EquipBestTool(gatedBreakableInteractionDetails, _toolsFound);

            OnHoverStart(gameObject);
        }
        public void Interact(string param)
        {
            throw new NotImplementedException();
        }

        public void OnInteractionStart()
        {
            startInteractionFeedbacks?.PlayFeedbacks();
            loopedInteractionFeedbacks?.PlayFeedbacks();
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
        public void OnInteractionEnd(string subjectUniquedID)
        {
            loopedInteractionFeedbacks?.StopFeedbacks();
            if (subjectUniquedID != uniqueID)
            {
                _isProcessingInteraction = false;
                return;
            }

            if (_hasBeenBroken)
            {
                Debug.LogWarning(
                    $"BioOrganismBreakableNode [{uniqueID}]: Already broken, ignoring duplicate break attempt");

                _isProcessingInteraction = false;
                return;
            }


            // if (hatchetBreakable == null)
            // {
            //     hatchetBreakable = GetComponent<HatchetBreakable>();
            //     if (hatchetBreakable == null)
            //         // Debug.LogWarning("BioOrganismBreakableNode: No HatchetBreakable component found.");
            //     {
            //         _isProcessingInteraction = false;
            //         return;
            //     }
            // }

            _hasBeenBroken = true;

            // // Perform the break action
            // if (hatchetBreakable != null)
            // {
            BreakInstantly();
            // DestructableEvent.Trigger(DestructableEventType.Destroyed, uniqueID, transform);
            // }

            _isProcessingInteraction = false;
        }

        public bool CanInteract()
        {
            return CanInteract(out _);
        }
        public void OnMMEvent(GatedBreakableInteractionEvent eventType)
        {
            if (_hasBeenBroken)
                return;

            if (string.IsNullOrWhiteSpace(eventType.SubjectUniqueID) ||
                string.IsNullOrWhiteSpace(uniqueID))
            {
                Debug.LogWarning(
                    $"BioOrganismBreakableNode: Null or empty uniqueID detected. Event: '{eventType.SubjectUniqueID}', This: '{uniqueID}'");

                return;
            }

            // Use Trim() and case-sensitive comparison to ensure exact match
            if (!eventType.SubjectUniqueID.Trim().Equals(uniqueID.Trim(), StringComparison.Ordinal))
                // This event is for a different object
                return;

            // Guard against re-entry
            if (_isProcessingInteraction && eventType.EventType == GatedInteractionEventType.CompleteInteraction)
            {
                Debug.LogWarning(
                    $"BioOrganismBreakableNode [{uniqueID}]: Already processing, ignoring duplicate CompleteInteraction event");

                return;
            }


            // if (eventType.SubjectUniqueID != uniqueID)
            //     return; // Ignore events for other interactables

            if (eventType.EventType == GatedInteractionEventType.StartInteraction)
            {
                OnInteractionStart();
            }
            else if (eventType.EventType == GatedInteractionEventType.CompleteInteraction)
            {
                _isProcessingInteraction = true;
                OnInteractionEnd(eventType.SubjectUniqueID);
            }
        }

        void OnDemolished(RayfireRigid demolished)
        {
            if (demolished.HasFragments)
                foreach (var frag in demolished.fragments)
                    frag.gameObject.layer = LayerMask.NameToLayer("Debris");
        }


        void EquipBestTool(GatedBreakableInteractionDetails details, List<string> toolsFound)
        {
            var toolID = details.GetMostEfficientRequiredToolID(toolsFound);


            var inventory = GlobalInventoryManager.Instance.playerInventory;
            if (inventory == null) return;
            var bestTool = inventory.Content.FirstOrDefault(s => s != null && s.ItemID == toolID);
            var sourceIndex = Array.IndexOf(inventory.Content, bestTool);
            if (bestTool == null) return;

            // scannerItem.Equip("Player1");
            MMInventoryEvent.Trigger(
                MMInventoryEventType.EquipRequest, null, bestTool.TargetInventoryName, bestTool, 1, sourceIndex,
                "Player1");
        }

        public override bool OnHoverStart(GameObject go)
        {
            if (!bioOrganismType) return true;

            var recognizable = bioOrganismType.identificationMode == IdentificationMode.RecognizableOnSight;

            var showKnown = recognizable; // later: OR with analysis progression
            var nameToShow = showKnown ? bioOrganismType.organismName : bioOrganismType.UnknownName;
            var iconToShow = showKnown
                ? bioOrganismType.organismIcon
                : bioOrganismType.organismIcon ?? ExaminationManager.Instance?.defaultUnknownIcon;

            var shortToShow = showKnown ? bioOrganismType.shortDescription : bioOrganismType.UnknownDescription;

            data = new SceneObjectData(
                nameToShow,
                iconToShow,
                shortToShow,
                AssetManager.Instance?.iconRepository.bioOrganismIcon,
                GetActionText(recognizable)
            )
            {
                Id = bioOrganismType.organismID
            };

            BillboardEvent.Trigger(data, BillboardEventType.Show);
            if (actionId != 0)
                if (ExaminationManager.Instance != null)
                {
                    if (IsPlayerAlreadyEquippedWithSuitableTool(gatedBreakableInteractionDetails, _toolsFound))
                        ControlsHelpEvent.Trigger(
                            ControlHelpEventType.Show, breakActionId,
                            string.IsNullOrEmpty(actionText) ? null : actionText,
                            AssetManager.Instance?.iconRepository.axeIcon);
                    else
                        ControlsHelpEvent.Trigger(
                            ControlHelpEventType.Show, actionId, additionalInfoText:
                            string.IsNullOrEmpty(actionText) ? null : actionText);
                }

            return true;
        }

        bool IsPlayerAlreadyEquippedWithSuitableTool(GatedBreakableInteractionDetails details, List<string> toolsFound)
        {
            if (toolsFound == null)
            {
                _toolsFound = HasToolForInteractionInInventory();
                if (_toolsFound.Count == 0) return false;
            }

            var toolIDs = details.requiredToolIDs;

            var equipmentInventory =
                GlobalInventoryManager.Instance.equipmentInventory;

            if (equipmentInventory == null) throw new Exception("Equipment inventory is null");

            foreach (var toolID in toolIDs)
            {
                var equippedTool = equipmentInventory.Content.FirstOrDefault(s => s != null && s.ItemID == toolID);
                if (equippedTool != null) return true;
            }

            return false;
        }


        protected override string GetActionText(bool recognizableOnSight)
        {
            return "Clear Growth";
        }

        public void BreakInstantly()
        {
            if (_isBroken)
            {
                Debug.LogWarning(
                    $"HatchetBreakable [{uniqueID}]: Already broken, ignoring BreakInstantly call");

                return;
            }

            // Skip the incremental hits; just perform the full break logic
            _hitCount = defaultHitsToBreak;
            ApplyHit(hardness, transform.position, transform.up);
        }
    }
}
