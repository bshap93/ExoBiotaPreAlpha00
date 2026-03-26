using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dirigible.Input;
using Domains.Gameplay.Mining.Scripts;
using FirstPersonPlayer.Interface;
using FirstPersonPlayer.Tools.Interface;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using FirstPersonPlayer.Tools.ToolPrefabScripts;
using Helpers.Events;
using Helpers.Events.Domains.Player.Events;
using Helpers.Events.Gated;
using Helpers.ScriptableObjects.Gated;
using HighlightPlus;
using Inventory;
using LevelConstruct.Highlighting;
using Manager;
using Manager.SceneManagers;
using Manager.UI;
using MoreMountains.Feedbacks;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using OWPData.DataClasses;
using SharedUI.Billboard;
using SharedUI.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Utilities.Interface;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace FirstPersonPlayer.Minable

{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    [DisallowMultipleComponent]
    public class MyOreNode : MonoBehaviour, IMinable, IBillboardable, IExaminable, IRequiresUniqueID, IInteractable,
        IHoverable,
        MMEventListener<GatedBreakableInteractionEvent>
    {
        const string DefaultActionText = "Mine Ore";
        [SerializeField] GameObject pieces;

        [SerializeField] float interactionDistance = 2f;


        [SerializeField] MeshRenderer intactRenderer; // single intact renderer
        [SerializeField] Collider nodeCollider; // intact collider
        [SerializeField] GameObject refinedPickup; // world drop (optional)

        [SerializeField] bool enableRespawn;
        [SerializeField] float respawnDelay = 30f;
        [SerializeField] bool permanentDestruction = true;

        [SerializeField] bool applyBlastOnShatter = true;
        [SerializeField] float blastForce = 3.5f;
        [SerializeField] float blastTorque = 1.5f;

        [FormerlySerializedAs("gatedInteractionDetails")] [SerializeField]
        GatedBreakableInteractionDetails gatedBreakableInteractionDetails;

        [SerializeField] MMFeedbacks loopedInteractionFeedbacks;
        [SerializeField] MMFeedbacks startInteractionFeedbacks;

        [Tooltip("Minimum tool power required to count as a successful hit.")]
        public int hardness = 1;

        [Header("Controls Help & Action Info")]
#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int actionId;
        public string actionText;

        [SerializeField] HighlightEffectController highlightEffectController;

#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int breakActionId;

        [Header("Examination Settings")]

#if ODIN_INSPECTOR
        [FoldoutGroup("Examination")]
        [InlineProperty]
        [HideLabel]
#endif
        [SerializeField]
        ExaminableObjectData examinableObjectData;


        [Header("Mining Settings")] [SerializeField]
        int dropOnHit;

        [SerializeField] int hitsToDestroy;
        [SerializeField] int dropOnDestroy;
        [SerializeField] Vector3 knockAngle;
        [SerializeField] AnimationCurve knockCurve;
        [SerializeField] float knockDuration = 1;


        [Header("Feedbacks")] [SerializeField] MMFeedbacks oreHitFeedback;


        [SerializeField] MMFeedbacks oreDestroyFeedback;
        [SerializeField] MMFeedbacks onHitFeedbacks;

        [SerializeField] UnityEvent OnOreDestroyed;
        [SerializeField] UnityEvent OnOreHit;

        public int OreHardness; // Hardness of the ore node.

        // Unique ID for the ore node.
        [FormerlySerializedAs("UniqueID")] public string uniqueID;

        [SerializeField] MMFeedbacks failHitFeedbacks;
        [SerializeField] GameObject oreHitParticles;
#if ODIN_INSPECTOR && UNITY_EDITOR
        [OnValueChanged(nameof(AutoSyncExaminableFromItem), true)]
        [InlineButton(nameof(SyncFromItem), "Sync From Item")]
#endif
        [SerializeField]
        public InventoryItem itemTypeMined;

        BillboardUI _activeBillboard;
        SceneObjectData _data;
        HighlightEffect _highlight;

        Bounds _intactBounds;

        Rigidbody _rigidbody;

        List<string> _toolsFound;
        int dropIndex;
        int hitIndex;

        // HighlightTrigger trigger;

        void Awake()
        {
            if (!intactRenderer) intactRenderer = GetComponentInChildren<MeshRenderer>(false);
            if (!nodeCollider) nodeCollider = GetComponent<Collider>();

            _intactBounds = GetIntactBounds();
        }


        void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            if (!(_rigidbody == null)) _rigidbody.isKinematic = true;
            StartCoroutine(InitializeAfterDestructableManager());
            oreHitFeedback?.Initialization();

            _highlight = highlightEffectController != null ? highlightEffectController.HighlightEffect : null;
        }


        void OnEnable()
        {
            // if (trigger == null) return;
            // trigger.OnObjectHighlightStart += OnHoverStart; // return false to cancel hover highlight
            // trigger.OnObjectHighlightStay += OnHoverStay; // called while highlighted; return false to force unhighlight
            // trigger.OnObjectHighlightEnd += OnHoverEnd;

            this.MMEventStartListening();
        }

        void OnDisable()
        {
            // if (trigger == null) return;
            // trigger.OnObjectHighlightStart -= OnHoverStart;
            // trigger.OnObjectHighlightStay -= OnHoverStay;
            // trigger.OnObjectHighlightEnd -= OnHoverEnd;

            this.MMEventStopListening();
        }


        public string GetName()
        {
            return itemTypeMined != null ? itemTypeMined.ItemName : "Unknown Ore Node";
        }

        public Sprite GetIcon()
        {
            return itemTypeMined != null ? itemTypeMined.Icon : null;
        }

        public string ShortBlurb()
        {
            return itemTypeMined.ShortDescription;
        }

        public Sprite GetActionIcon()
        {
            return null;
        }


        public string GetActionText()
        {
            return "Mine Ore";
        }

        public void OnFinishExamining()
        {
            if (itemTypeMined != null &&
                (examinableObjectData == null || string.IsNullOrEmpty(examinableObjectData.Id)))
            {
                BuildExaminableFrom(itemTypeMined);
                if (examinableObjectData != null) examinableObjectData.Id = itemTypeMined.ItemID;
            }

            ExaminationEvent.Trigger(ExaminableItemType.Ore, examinableObjectData);
            if (examinableObjectData != null)
                BillboardEvent.Trigger(examinableObjectData.FromExaminableObjectData(), BillboardEventType.Update);
        }

        public bool ExaminableWithRuntimeTool(IRuntimeTool tool)
        {
            if (tool is HandheldScannerToolPrefab) return true;

            return false;
        }


// MyOreNode
        public bool OnHoverStart(GameObject go)
        {
            if (itemTypeMined == null) return true;


            // Fall back to recognizable-on-sight if that’s your item’s mode
            var recognizable =
                !(itemTypeMined is OreItemObject); // ores are NOT recognizable on sight

            var nameToShow = itemTypeMined.ItemName;
            var iconToShow =
                itemTypeMined.Icon;

            var shortToShow =
                itemTypeMined.ShortDescription;

            var icon = ExaminationManager.Instance?.iconRepository.mineOreIcon;
            // Build billboard data each time we hover (or cache if you prefer)
            _data = new SceneObjectData(nameToShow, iconToShow, shortToShow, icon, "Mine Ore");
            _data.Id = itemTypeMined.ItemID; // keep this consistent with the type id

            BillboardEvent.Trigger(_data, BillboardEventType.Show);
            if (actionId != 0)
            {
                if (IsPlayerAlreadyEquippedWithBestTool(gatedBreakableInteractionDetails, _toolsFound))
                    ControlsHelpEvent.Trigger(
                        ControlHelpEventType.Show, breakActionId);
                else
                    ControlsHelpEvent.Trigger(
                        ControlHelpEventType.Show, actionId);
            }


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
            BillboardEvent.Trigger(_data, BillboardEventType.Hide);
            if (actionId != 0)
                ControlsHelpEvent.Trigger(
                    ControlHelpEventType.Hide, actionId, string.IsNullOrEmpty(actionText) ? null : actionText);

            /* UI tooltip off*/
            return true;
        }
        public void Interact()
        {
            if (!CanInteract(out var reason))
            {
                if (reason == GatedInteractionManager.ReasonWhyCannotInteract.NotEnoughStamina)
                    AlertEvent.Trigger(
                        AlertReason.NotEnoughStamina,
                        "You do not have enough stamina to mine this ore.",
                        "Insufficient Stamina");
                else if (reason == GatedInteractionManager.ReasonWhyCannotInteract.LackingNecessaryTool)
                    AlertEvent.Trigger(
                        AlertReason.LackToolForInteraction, "You need the appropriate pickaxe to destroy this ore.",
                        "Lacking Necessary Tool");

                return;
            }

            ControlsHelpEvent.Trigger(ControlHelpEventType.ShowUseThenHide, actionId);

            // GatedBreakableInteractionEvent.Trigger(
            //     GatedInteractionEventType.TriggerGateUI, gatedBreakableInteractionDetails, uniqueID, _toolsFound);
            EquipBestTool(gatedBreakableInteractionDetails, _toolsFound);

            OnHoverStart(gameObject);
        }
        public void Interact(string param)
        {
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
            if (subjectUniquedID != uniqueID) return;

            // BreakInstantly();
        }

        public bool CanInteract()
        {
            return CanInteract(out _);
        }

        public void MinableMineHit()
        {
            hitIndex++;
            dropIndex = hitIndex < hitsToDestroy ? dropOnHit : dropOnDestroy;

            if (hitIndex < hitsToDestroy)
            {
                oreHitFeedback?.PlayFeedbacks();
                // partial hit feedback
                if (oreHitParticles != null)
                {
                    var fx = Instantiate(oreHitParticles, transform.position, Quaternion.identity);
                    Destroy(fx, 2f);
                }


                OnOreHit?.Invoke();
                PlayHitFx(transform.position, Vector3.up);
                StartCoroutine(Animate());
                return;
            }

            // === FINAL HIT ===
            oreDestroyFeedback?.PlayFeedbacks();

            // if (oreDestroyParticles != null)
            // {
            //     var fx = Instantiate(oreDestroyParticles, transform.position, Quaternion.identity);
            //     Destroy(fx, 2f);
            // }

            OnOreDestroyed?.Invoke();
            DestructableEvent.Trigger(DestructableEventType.Destroyed, uniqueID, transform);

            var dropCount = dropOnHit + dropOnDestroy;

            if (applyBlastOnShatter)
                ShatterAndHandleDrops(dropCount);
            else
                SpawnPiecesNoBlast(); // <-- call here instead of direct Instantiate

            // handle drops regardless
            // (move drop spawn into ShatterAndHandleDrops if you want them unified)
        }


        public void MinableFailHit(Vector3 hitPoint)
        {
            failHitFeedbacks?.PlayFeedbacks();

            // if (failHitParticles != null)
            // {
            //     var fx = Instantiate(failHitParticles, hitPoint, Quaternion.identity);
            //     Destroy(fx, 2f);
            // }
        }

        public int GetCurrentMinableHardness()
        {
            return OreHardness;
        }

        public void ShowMineablePrompt()
        {
        }
        public int GetHardness()
        {
            return hardness;
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
        public void OnMMEvent(GatedBreakableInteractionEvent eventType)
        {
            if (eventType.SubjectUniqueID != uniqueID)
                return; // Ignore events for other interactables

            if (eventType.EventType == GatedInteractionEventType.StartInteraction)
                OnInteractionStart();
            else if (eventType.EventType == GatedInteractionEventType.CompleteInteraction)
                OnInteractionEnd(eventType.SubjectUniqueID);
        }

        public void StartExamining()
        {
            Debug.Log("[ExaminationManager] Starting Examining...");
        }

        public void StopExamining()
        {
        }

        bool IsPlayerAlreadyEquippedWithBestTool(GatedBreakableInteractionDetails details, List<string> toolsFound)
        {
            if (toolsFound == null)
            {
                _toolsFound = HasToolForBreakInInventory();
                if (_toolsFound.Count == 0) return false;
            }

            var toolID = details.GetMostEfficientRequiredToolID(toolsFound);

            var equipmentInventory =
                GlobalInventoryManager.Instance.equipmentInventory;

            if (equipmentInventory == null) throw new Exception("Equipment inventory is null");

            var equippedTool = equipmentInventory.Content.FirstOrDefault(s => s != null && s.ItemID == toolID);
            if (equippedTool != null && equippedTool.ItemID != null) return equippedTool.ItemID == toolID;

            return false;
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

        public bool CanInteract(out GatedInteractionManager.ReasonWhyCannotInteract reason)
        {
            var currentStamina = PlayerMutableStatsManager.Instance.CurrentStamina;
            if (currentStamina - gatedBreakableInteractionDetails.staminaCost < 0)
            {
                reason = GatedInteractionManager.ReasonWhyCannotInteract.NotEnoughStamina;
                AlertEvent.Trigger(
                    AlertReason.NotEnoughStamina,
                    "You do not have enough stamina to mine this ore.",
                    "Insufficient Stamina");

                return false;
            }

            if (!gatedBreakableInteractionDetails.requireTools)
            {
                reason = GatedInteractionManager.ReasonWhyCannotInteract.None;
                return true;
            }

            _toolsFound = HasToolForBreakInInventory();
            if (_toolsFound.Count == 0)
            {
                reason = GatedInteractionManager.ReasonWhyCannotInteract.LackingNecessaryTool;
                return false;
            }

            reason = GatedInteractionManager.ReasonWhyCannotInteract.None;
            return true;
        }
        public void OnInteractionEnd()
        {
        }


        bool CanBeDamagedBy(int toolPower)
        {
            return toolPower >= hardness;
        }

#if UNITY_EDITOR
        public IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            return AllRewiredActions.GetAllRewiredActions();
        }

#endif


        GameObject SpawnPiecesNoBlast()
        {
            // 1) Turn off the intact collider *before* spawning pieces to avoid instant penetration push
            if (nodeCollider) nodeCollider.enabled = false;

            // 2) Spawn pieces as a sibling and copy local scale
            var inst = Instantiate(pieces, transform.position, transform.rotation, transform.parent);
            inst.transform.localScale = transform.localScale;

            // 3) Neutralize physics impulses on all piece rigidbodies
            var rbs = inst.GetComponentsInChildren<Rigidbody>(true);
            foreach (var rb in rbs)
            {
                if (rb == null) continue;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                // Stop the "pop" from de-penetration
                rb.maxDepenetrationVelocity = 0.05f;

                // Hard no-blast mode:
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            // 4) (Optional) If you want a gentle settle after they appear, enable physics later
            // StartCoroutine(EnablePhysicsLater(rbs, 0.2f /* seconds */, useGravity:true));

            // 5) Make sure intact collider never bumps pieces (extra safety)
            if (nodeCollider)
                foreach (var col in inst.GetComponentsInChildren<Collider>(true))
                    if (col)
                        Physics.IgnoreCollision(col, nodeCollider, true);

            return inst;
        }

        IEnumerator EnablePhysicsLater(IEnumerable<Rigidbody> rbs, float delay, bool useGravity)
        {
            yield return new WaitForSeconds(delay);
            foreach (var rb in rbs)
            {
                if (!rb) continue;
                rb.isKinematic = false;
                rb.useGravity = useGravity;
                // keep a low max depenetration to avoid sudden kicks
                rb.maxDepenetrationVelocity = 0.2f;
            }
        }

        Bounds GetIntactBounds()
        {
            if (intactRenderer) return intactRenderer.bounds;
            if (nodeCollider) return nodeCollider.bounds;
            return new Bounds(transform.position, Vector3.one); // fallback
        }

        Vector3 RandomPointOnTop(Bounds b)
        {
            return new Vector3(
                Random.Range(b.min.x, b.max.x),
                b.center.y,
                Random.Range(b.min.z, b.max.z)
            );
        }


        void ShatterAndHandleDrops(int dropCount)
        {
            // 1) Use INTACT bounds before hiding
            var b = _intactBounds;

            // Drops (optional)
            for (var i = 0; i < dropCount; i++)
                if (refinedPickup)
                {
                    var pos = RandomPointOnTop(b) + new Vector3(0, 0.4f, 0);
                    Instantiate(refinedPickup, pos, Quaternion.identity);
                    // make it spin
                }


            // 2) Instantiate shattered PIECES as a sibling; copy localScale
            if (pieces)
            {
                var inst = Instantiate(pieces, transform.position, transform.rotation, transform.parent);
                inst.transform.localScale = transform.localScale;

                if (applyBlastOnShatter)
                    foreach (var rb in inst.GetComponentsInChildren<Rigidbody>())
                    {
                        if (!rb) continue;
                        var dir = (rb.worldCenterOfMass - b.center).normalized;
                        rb.AddForce(dir * blastForce, ForceMode.Impulse);
                        rb.AddTorque(Random.insideUnitSphere * blastTorque, ForceMode.Impulse);
                    }
            }

            // 3) Hide the intact rock
            if (nodeCollider) nodeCollider.enabled = false;
            if (intactRenderer) intactRenderer.enabled = false;

            // 4) Cleanup / respawn policy
            if (permanentDestruction)
                Destroy(gameObject, 0.1f);
            else if (enableRespawn) StartCoroutine(RespawnAfterDelay());
        }

        IEnumerator RespawnAfterDelay()
        {
            yield return new WaitForSeconds(respawnDelay);
            // reset hit count in your logic
            if (nodeCollider) nodeCollider.enabled = true;
            if (intactRenderer) intactRenderer.enabled = true;
            _intactBounds = GetIntactBounds();
        }


        public void PlayHitFx(Vector3 hitPoint, Vector3 hitNormal)
        {
            Debug.Log("[MyOreNode] PlayHitFx.");
            onHitFeedbacks?.PlayFeedbacks(transform.position);

            if (oreHitParticles)
            {
                var fx = Instantiate(oreHitParticles, hitPoint, Quaternion.LookRotation(hitNormal));
                Destroy(fx, 2f);
            }

            if (_highlight != null)
            {
                _highlight.HitFX(); // plays the configured hit effect
                Debug.Log("[MyOreNode] Played highlight hit FX.");
            }
        }
        IEnumerator InitializeAfterDestructableManager()
        {
            // Wait one frame so core managers come up
            yield return null;

            // Despawn if already destroyed in this save
            if (DestructableManager.Instance != null &&
                DestructableManager.Instance.IsDestroyed(uniqueID))
                Destroy(gameObject);
        }


        IEnumerator Animate()
        {
            var startRotation = transform.localRotation;
            var endRotation = startRotation * Quaternion.Euler(knockAngle);

            float t = 0;
            while (t < knockDuration)
            {
                var v = knockCurve.Evaluate(t / knockDuration);
                transform.localRotation = Quaternion.Lerp(startRotation, endRotation, v);
                t += Time.deltaTime;
                yield return null;
            }

            // Optional: restore to exact start if you want
            transform.localRotation = startRotation;
        }

        // Works in both editor and play mode if you ever need it at runtime
        void BuildExaminableFrom(InventoryItem src)
        {
            if (src == null) return;

            // Pull data from InventoryItem (via BaseItem/OreItemObject)
            // Item core fields:
            var name = src.ItemName; // :contentReference[oaicite:3]{index=3}
            var icon = src.Icon; // :contentReference[oaicite:4]{index=4}
            var shortBlurb = src.ShortDescription; // :contentReference[oaicite:5]{index=5}
            var fullDescription = src.Description; // :contentReference[oaicite:6]{index=6}

            // Identification / unknowns (from your partial class):
            var mode = src.identificationMode; // :contentReference[oaicite:7]{index=7}
            var unknownIcon = ExaminationManager.Instance.defaultUnknownIcon;
            var unknownName = src.UnknownName; // :contentReference[oaicite:9]{index=9}
            var unknownShort = examinableObjectData.UnknownShortBlurb; // or your preferred default


            // Build a brand-new ExaminableObjectData using your ctor signature
            // ExaminableObjectData(string name, Sprite icon, string shortBlurb, Sprite actionIcon, string actionText,
            //   string fullDescription, IdentificationMode identificationMode, Sprite unknownIcon, string unknownShortBlurb, string unkonwnName)
            examinableObjectData = new ExaminableObjectData(
                name,
                icon,
                shortBlurb,
                null,
                DefaultActionText,
                fullDescription,
                mode,
                unknownIcon,
                unknownShort,
                unknownName
            ); // :contentReference[oaicite:10]{index=10}

            examinableObjectData.Id = src.ItemID; // <-- important: the ore TYPE id
        }
        List<string> HasToolForBreakInInventory()
        {
            if (gatedBreakableInteractionDetails == null) return new List<string>();
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
#if UNITY_EDITOR

        // Odin auto-callback when itemTypeMined changes in Inspector
        void AutoSyncExaminableFromItem()
        {
            // Only run in editor, not at runtime
            if (!Application.isEditor) return;
            if (itemTypeMined == null) return;

            BuildExaminableFrom(itemTypeMined);

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
        void SyncFromItem()
        {
            if (itemTypeMined == null) return;
            BuildExaminableFrom(itemTypeMined);

            EditorUtility.SetDirty(this);
            if (!Application.isPlaying)
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif
    }
}
