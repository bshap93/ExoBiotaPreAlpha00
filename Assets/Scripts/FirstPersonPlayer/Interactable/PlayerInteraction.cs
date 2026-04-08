// [WIP] Placeholder for revised First Person Interaction-Inventory system	

using System.Collections;
using System.Collections.Generic;
using Dirigible.Input;
using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
using FirstPersonPlayer.Interface;
using FirstPersonPlayer.Tools;
using FirstPersonPlayer.UI;
using Helpers.Events;
using LevelConstruct.Interactable.ItemInteractables.ItemPicker;
using MoreMountains.Tools;
using SharedUI.Interface;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Inputs;
using Utilities.Interface;

// using Digger.Modules.Core.Sources;

namespace FirstPersonPlayer.Interactable
{
    public class PlayerInteraction : MonoBehaviour, MMEventListener<PlayerInteractionEvent>
    {
        public float maxInteractDistance = 10f; // Maximum distance for interaction
        public float interactionDistance = 2; // How far the player can interact
        public LayerMask interactableLayer; // Only detect objects in this layer
        public LayerMask terrainLayer; // Only detect objects in this layer
        public LayerMask damageableLayerMask;
        public LayerMask obstacleLayer; // New: layers that block interaction (e.g., walls, rocks)
        public float controlHelpReminderDuration = 2f; // Duration to show control help reminder


        [FormerlySerializedAs("RightHandEquipment")] [SerializeField]
        PlayerEquipment rightHandEquipment;
        [SerializeField] PlayerEquippedAbility equippedPlayerBioticPower;

        public PlayerPropPickup propPickup;


        [SerializeField] RewiredFirstPersonInputs rewiredInput;

        public CinemachineCamera playerCamera; // Reference to the player’s camera

        [FormerlySerializedAs("LightReminderActionId")]
        [FormerlySerializedAs("ActionId")]
#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int lightReminderActionId;

        [Header("Reticle")] public ReticleController reticleController;

        public LayerMask playerLayerMask;
        [SerializeField] LayerMask waterLayer; // Add this field
        [SerializeField] LayerMask solidGroundLayer; // Add this field

        readonly Collider[] _waterCheckBuffer = new Collider[4]; // Adjust size as needed
        // [SerializeField] PlayerEquipment leftHandEquipment;

        CreatureController _creatureControllerCurrentlyInRangeAimed;


        GameObject _currentlyHoveredObject;

        bool _holdingItem;

        public PlayerEquippedAbility EquippedPlayerBioticPower => equippedPlayerBioticPower;

        // public PlayerEquipment LeftHandEquipment => leftHandEquipment;

        public static PlayerInteraction Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;
        }

        void Start()
        {
            // FindFirstObjectByType<DiggerMaster>();
            // Find the TextureDetector in the scene


            if (reticleController == null) reticleController = FindFirstObjectByType<ReticleController>();

            if (rewiredInput == null) rewiredInput = GetComponent<RewiredFirstPersonInputs>();

            ControlsHelpEvent.Trigger(ControlHelpEventType.Show, lightReminderActionId);

            StartCoroutine(WaitAndDisableControlHelp(controlHelpReminderDuration));
        }

        void Update()
        {
            PerformRaycastCheck(); // ✅ Single raycast for both interactables and diggable terrain


            if (rewiredInput.interact) // Press E to interact
                PerformInteraction();

            if (rewiredInput.pickablePick)
                PerformPickablePick();


            if (rewiredInput.dropPropOrHold)
            {
                _holdingItem = propPickup != null && propPickup.IsHoldingItem();
                if (_holdingItem)
                    ControlsHelpEvent.Trigger(
                        ControlHelpEventType.Show, 67, "BlockAllNewRequests");
                else
                    ControlsHelpEvent.Trigger(
                        ControlHelpEventType.Hide, 67, "UnblockAllNewRequests");
            }
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();

            // End hover if we're currently hovering something
            if (_currentlyHoveredObject != null)
            {
                var hoverable = _currentlyHoveredObject.GetComponent<IHoverable>();
                hoverable?.OnHoverEnd(_currentlyHoveredObject);
                _currentlyHoveredObject = null;
            }
        }

// #if UNITY_EDITOR
//         // This will be called from the parent ScriptableObject
//         IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
//         {
//             var parent = ControlsPromptSchemeSet._currentContextSO;
//             if (parent == null || parent.inputManagerPrefab == null) yield break;
//
//             var data = parent.inputManagerPrefab.userData;
//             if (data == null) yield break;
//
//             foreach (var action in data.GetActions_Copy())
//                 yield return new ValueDropdownItem<int>(action.name, action.id);
//         }
// #endif
        public void OnMMEvent(PlayerInteractionEvent eventType)
        {
            if (eventType.EventType == PlayerInteractionEventType.Interacted) PerformInteraction();
        }

        public string CreatureControllerCurrentlyInRangeAimed()
        {
            if (_creatureControllerCurrentlyInRangeAimed == null)
            {
                Debug.Log("No creature currently aimed at within range.");
                return null;
            }

            return _creatureControllerCurrentlyInRangeAimed.uniqueID;
        }
        void PerformPickablePick()
        {
            if (playerCamera == null)
            {
                Debug.LogError("PlayerInteraction: No camera assigned!");
                return;
            }

            var rayOrigin = playerCamera.transform.position;
            var rayDirection = playerCamera.transform.forward;

            var interactMask = interactableLayer & ~playerLayerMask;
            var terrMask = terrainLayer & ~playerLayerMask;
            var obstacleMask = obstacleLayer & ~playerLayerMask; // Add obstacle mask

            // Use maxInteractDistance for initial detection
            RaycastHit obstacleHit;
            var obstacleBlocking = Physics.Raycast(
                rayOrigin, rayDirection, out obstacleHit, maxInteractDistance, obstacleMask);

            // Check if terrain is blocking
            RaycastHit terrainHit;
            var terrainBlocking = Physics.Raycast(
                rayOrigin, rayDirection, out terrainHit, maxInteractDistance, terrMask);

            // Check for interactables
            RaycastHit interactableHit;
            var hitInteractable = Physics.Raycast(
                rayOrigin, rayDirection, out interactableHit, maxInteractDistance, interactMask);

            if (hitInteractable &&
                (!obstacleBlocking || interactableHit.distance < obstacleHit.distance) &&
                (!terrainBlocking || interactableHit.distance < terrainHit.distance))
            {
                var itemPicker = interactableHit.collider.GetComponent<ItemPicker>();
                if (itemPicker != null)
                {
                    // Check if we're within the item's specific interaction distance
                    var interactable = itemPicker as IInteractable;
                    var requiredDistance = interactable?.GetInteractionDistance() ?? interactionDistance;

                    if (interactableHit.distance <= requiredDistance) itemPicker.PickupItemDirect();
                }
            }
        }

        IEnumerator WaitAndDisableControlHelp(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            ControlsHelpEvent.Trigger(ControlHelpEventType.Hide, lightReminderActionId);
        }

        void PerformInteraction()
        {
            if (playerCamera == null)
            {
                Debug.LogError("PlayerInteraction: No camera assigned!");
                return;
            }

            var rayOrigin = playerCamera.transform.position;
            var rayDirection = playerCamera.transform.forward;

            var interactMask = interactableLayer & ~playerLayerMask;
            var terrMask = terrainLayer & ~playerLayerMask;
            var obstacleMask = obstacleLayer & ~playerLayerMask; // Add obstacle mask

            // Check for obstacles first
            RaycastHit obstacleHit;
            var obstacleBlocking = Physics.Raycast(
                rayOrigin, rayDirection, out obstacleHit, maxInteractDistance, obstacleMask);

            // Check if terrain is blocking
            RaycastHit terrainHit;
            var terrainBlocking = Physics.Raycast(
                rayOrigin, rayDirection, out terrainHit, maxInteractDistance, terrMask);

            // Check for interactables
            RaycastHit interactableHit;
            var hitInteractable = Physics.Raycast(
                rayOrigin, rayDirection, out interactableHit, maxInteractDistance, interactMask);

            // Only interact if:
            // 1. We hit an interactable AND
            // 2. No obstacles are blocking AND
            // 3. Either there's no terrain blocking OR the interactable is closer than the terrain
            if (hitInteractable &&
                (!obstacleBlocking || interactableHit.distance < obstacleHit.distance) &&
                (!terrainBlocking || interactableHit.distance < terrainHit.distance))
            {
                var interactable = interactableHit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    var requiredDistance = interactable.GetInteractionDistance();
                    if (interactableHit.distance <= requiredDistance)
                        interactable.Interact();
                }
            }
        }


        void PerformRaycastCheck()
        {
            if (playerCamera == null)
            {
                Debug.LogError("PlayerInteraction: No camera assigned!");
                return;
            }

            var rayOrigin = playerCamera.transform.position;
            var rayDirection = playerCamera.transform.forward;

            var terrMask = terrainLayer & ~playerLayerMask;
            var interactMask = interactableLayer & ~playerLayerMask;
            var damageableMask = damageableLayerMask & ~playerLayerMask;

            // Combined raycast check
            RaycastHit terrainHit;
            var terrainBlocking =
                Physics.Raycast(rayOrigin, rayDirection, out terrainHit, maxInteractDistance, terrMask);

            // if (terrainBlocking) forwardTerrainLayerDetector?.UpdateFromHit(terrainHit);

            RaycastHit interactableHit;
            var hitInteractable = Physics.Raycast(
                rayOrigin, rayDirection, out interactableHit, maxInteractDistance,
                interactMask);

            RaycastHit damageableHit;
            var hitDamageable = Physics.Raycast(
                rayOrigin, rayDirection, out damageableHit, maxInteractDistance,
                damageableMask);

            // Determine the hit to process
            RaycastHit? actualHit = null;
            var isTerrainBlocking = false;
            var isOutOfRange = false;

            if (terrainBlocking && hitInteractable)
            {
                if (terrainHit.distance < interactableHit.distance)
                {
                    isTerrainBlocking = true;
                }
                else
                {
                    // Check if within interaction distance
                    var interactable = interactableHit.collider.GetComponent<IInteractable>();
                    var requiredDistance = interactable?.GetInteractionDistance() ?? interactionDistance;

                    if (interactableHit.distance <= requiredDistance)
                        actualHit = interactableHit;
                    else
                        isOutOfRange = true;
                }
            }
            else if (hitInteractable)
            {
                // Check if within interaction distance
                var interactable = interactableHit.collider.GetComponent<IInteractable>();
                var requiredDistance = interactable?.GetInteractionDistance() ?? interactionDistance;

                if (interactableHit.distance <= requiredDistance)
                    actualHit = interactableHit;
                else
                    isOutOfRange = true;
            }

            if (hitDamageable)
            {
                var damageable = damageableHit.collider.GetComponent<IDamageable>();
                var currentToolRange = PlayerEquipment.Instance.GetCurrentToolRange();

                if (damageable is CreatureController creatureController && damageableHit.distance <= currentToolRange &&
                    (_creatureControllerCurrentlyInRangeAimed == null ||
                     _creatureControllerCurrentlyInRangeAimed.uniqueID != creatureController.uniqueID))
                {
                    _creatureControllerCurrentlyInRangeAimed = creatureController;
                    Debug.Log(creatureController.creatureType.creatureName + " currently aimed at within range.");
                }
            }
            else if (_creatureControllerCurrentlyInRangeAimed != null)
            {
                _creatureControllerCurrentlyInRangeAimed = null;
                Debug.Log("Creature got out of range");
            }


            // NEW: Handle hover state for IHoverable objects
            HandleHoverState(actualHit, isTerrainBlocking || isOutOfRange);


            // Update reticle through controller
            reticleController.UpdateReticle(actualHit, isTerrainBlocking || isOutOfRange);
        }

        void HandleHoverState(RaycastHit? hit, bool isBlocked)
        {
            GameObject hitObject = null;

            // Only consider the object if it's not blocked
            if (hit.HasValue && !isBlocked) hitObject = hit.Value.collider.gameObject;

            // Check if we're hovering over a new object
            if (hitObject != _currentlyHoveredObject)
            {
                // End hover on previous object
                if (_currentlyHoveredObject != null)
                {
                    var previousHoverable = _currentlyHoveredObject.GetComponent<IHoverable>();
                    previousHoverable?.OnHoverEnd(_currentlyHoveredObject);
                }

                // Start hover on new object
                if (hitObject != null)
                {
                    var newHoverable = hitObject.GetComponent<IHoverable>();
                    if (newHoverable != null) newHoverable.OnHoverStart(hitObject);
                }

                _currentlyHoveredObject = hitObject;
            }
            // Continue hovering over same object
            else if (_currentlyHoveredObject != null)
            {
                var hoverable = _currentlyHoveredObject.GetComponent<IHoverable>();
                hoverable?.OnHoverStay(_currentlyHoveredObject);
            }
        }

        public int GetGroundTextureIndex()
        {
            var origin = transform.position + Vector3.up * 0.1f;
            if (Physics.Raycast(origin, Vector3.down, out var hit, 2f, terrainLayer & ~playerLayerMask))
            {
                // forwardTerrainLayerDetector?.UpdateFromHit(hit);
                // return forwardTerrainLayerDetector?.textureIndex ?? -1;
            }

            return -1;
        }

#if UNITY_EDITOR
        public static IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            return AllRewiredActions.GetAllRewiredActions();
        }

#endif


        public GroundInfo GetGroundInfo()
        {
            var origin = transform.position + Vector3.up * 0.5f;
            var groundInfo = new GroundInfo();

            var hitCount = Physics.OverlapSphereNonAlloc(origin, 0.2f, _waterCheckBuffer, waterLayer);
            if (hitCount > 0)
            {
                var groundTag = _waterCheckBuffer[0].gameObject.tag;
                if (groundTag == "Water")
                    groundInfo.isInWater = true;

                groundInfo.tag = groundTag;
                return groundInfo;
            }


            origin += Vector3.up * 0.1f;
            if (Physics.Raycast(origin, Vector3.down, out var hit, 2f, solidGroundLayer))
            {
                groundInfo.layerMask = 1 << hit.collider.gameObject.layer;
                groundInfo.tag = hit.collider.gameObject.tag;
            }


            return groundInfo;
        }

        public class GroundInfo
        {
            public bool isInWater;
            public LayerMask layerMask;
            public string tag;
        }
    }
}
