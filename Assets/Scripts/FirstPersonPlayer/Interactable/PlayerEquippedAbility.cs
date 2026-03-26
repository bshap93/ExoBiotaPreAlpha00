using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FirstPersonPlayer.Combat.Player.BioticAbility;
using FirstPersonPlayer.InputHandling;
using FirstPersonPlayer.ScriptableObjects.BioticAbility;
using FirstPersonPlayer.Tools;
using FirstPersonPlayer.Tools.Interface;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using FirstPersonPlayer.UI.ProgressBars;
using Helpers.AnimancerHelper;
using Helpers.AnimancerHelper.Helpers.AnimancerHelper;
using Helpers.Events.UI;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using SharedUI;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FirstPersonPlayer.Interactable
{
    public class PlayerEquippedAbility : MonoBehaviour, MMEventListener<MMInventoryEvent>
    {
        public MoreMountains.InventoryEngine.Inventory equippedAbilityInventory;
        [SerializeField] string equippedAbilityInventoryName = "EquippedAbilityInventory";
        public Transform bioticAbilityAnchor;

        public ProgressBarPurple progressBar;
        [SerializeField] RewiredFirstPersonInputs rewiredInput;
        [SerializeField] PlayerEquipment playerEquipment;

        [Header("Left Arm")] public bool showLeftArm;
        [ShowIf("showLeftArm")] public AnimancerArmController animancerPrimaryArmsController;

        [ShowIf("showLeftArm")] public AnimationClip leftArmCast;
        [ShowIf("showLeftArm")] public GameObject leftArm;
        [ShowIf("showLeftArm")] public LeftArmController leftArmController;


        public PlayerInteraction playerInteraction;


        public MyNormalMovement playerNormalMovement;
#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int actionId;

        BioticAbilityToolWrapper _bioticAbilityEquipped;

        Coroutine _equipInitRoutine;

        float _nextUseTime;

        bool _wasUseButtonHeldLastFrame;

        public static PlayerEquippedAbility Instance { get; private set; }

        public BioticAbilityToolWrapper CurrentAbilityItemSo { get; private set; }
        public IRuntimeBioticAbility CurrentRuntimeAbility { get; private set; }

        void Awake()
        {
            Instance = this;
        }


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            if (rewiredInput != null)
                HandleBioticAbilityInput(rewiredInput.pressedSprintOrAbility, rewiredInput.heldSprintOrAbility);

            if (showLeftArm && leftArmController != null && leftArm.activeSelf && playerNormalMovement != null)
                leftArmController.SyncLocomotion(playerNormalMovement.IsMoving, playerNormalMovement.IsRunning);
        }

        void OnEnable()
        {
            this.MMEventStartListening();

            if (_equipInitRoutine != null) StopCoroutine(_equipInitRoutine);
            _equipInitRoutine = StartCoroutine(WaitForInventoryAndEquip());
        }


        void OnDisable()
        {
            this.MMEventStopListening();
            if (_equipInitRoutine != null)
            {
                StopCoroutine(_equipInitRoutine);
                _equipInitRoutine = null;
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void OnMMEvent(MMInventoryEvent e)
        {
            if (e.TargetInventoryName != equippedAbilityInventoryName) return;

            switch (e.InventoryEventType)
            {
                case MMInventoryEventType.ItemEquipped:
                    if (e.EventItem is BioticAbilityToolWrapper ability01) EquipBioticAbility(ability01);
                    if (showLeftArm)
                        leftArm.SetActive(true);

                    break;
                case MMInventoryEventType.ItemUnEquipped:
                    if (e.EventItem is BioticAbilityToolWrapper ability02) UnequipBioticAbility();
                    if (showLeftArm)
                    {
                        leftArm.SetActive(false);
                        leftArmController?.StartIdle();
                    }

                    break;
                case MMInventoryEventType.ItemUsed:
                    if (e.EventItem is BioticAbilityToolWrapper) UseCurrentBioticAbility();
                    break;
                case MMInventoryEventType.Destroy:
                    if (e.EventItem is BioticAbilityToolWrapper) UnequipBioticAbility();
                    break;
            }
        }

        public void UnequipAbility()
        {
            UnequipBioticAbility();
            if (showLeftArm)
            {
                leftArm.SetActive(false);
                leftArmController?.StartIdle();
            }

            EquipmentUIEvent.Trigger(EquipmentUIEventType.UnequippedAbility);
        }
        void HandleBioticAbilityInput(bool rewiredInputItemSprintOrAbilityPressed,
            bool rewiredInputItemSprintOrAbilityHeld)
        {
            if (_bioticAbilityEquipped == null || playerEquipment.AreBothHandsOccupied())
                return;

            // Get the usage scheme to determine how to handle input
            var usageScheme = CurrentRuntimeAbility?.GetUsageScheme();

            if (usageScheme == IRuntimeBioticAbility.UsageScheme.UseTool)
            {
                // UseTool scheme: Just use the ability on button press (like a weapon)
                // The ability handles its own cooldowns and timing
                if (rewiredInputItemSprintOrAbilityPressed) UseCurrentBioticAbility();
            }
            else if (usageScheme == IRuntimeBioticAbility.UsageScheme.Activation)
            {
                // Activation scheme: Press to activate, release to deactivate (hold-based)
                if (rewiredInputItemSprintOrAbilityPressed)
                    switch (_bioticAbilityEquipped.bioticAbility.usageType)
                    {
                        case BioticAbility.UsageType.SingleUse:
                            CurrentRuntimeAbility.Activate(_bioticAbilityEquipped.bioticAbility, transform);
                            break;
                        case BioticAbility.UsageType.UseWhileHeld:
                            CurrentRuntimeAbility.Activate(_bioticAbilityEquipped.bioticAbility, transform);
                            break;
                    }

                if (!rewiredInputItemSprintOrAbilityHeld && _wasUseButtonHeldLastFrame)
                    switch (_bioticAbilityEquipped.bioticAbility.usageType)
                    {
                        case BioticAbility.UsageType.SingleUse:
                            // Do nothing, single use abilities don't need to be deactivated
                            break;
                        case BioticAbility.UsageType.UseWhileHeld:
                            Debug.Log("Deactivating biotic ability: " + _bioticAbilityEquipped.name);
                            CurrentRuntimeAbility.Deactivate();
                            break;
                    }
            }

            _wasUseButtonHeldLastFrame = rewiredInputItemSprintOrAbilityHeld;
        }

        IEnumerator WaitForInventoryAndEquip()
        {
            // Wait for inventory to be available
            var maxAttempts = 100;
            var attempts = 0;

            while (equippedAbilityInventory == null && attempts < maxAttempts)
            {
                equippedAbilityInventory = MoreMountains.InventoryEngine.Inventory.FindInventory(
                    equippedAbilityInventoryName, "Player1");

                if (equippedAbilityInventory != null)
                    break;

                attempts++;
                yield return null;
            }

            if (equippedAbilityInventory == null)
            {
                Debug.LogWarning($"[PlayerEquippedAbility] Could not find inventory: {equippedAbilityInventoryName}");
                _equipInitRoutine = null;
                yield break;
            }

            // Check if there's already an ability equipped
            var equippedItem = equippedAbilityInventory.Content.FirstOrDefault();
            if (equippedItem != null &&
                !InventoryItem.IsNull(equippedItem) &&
                equippedItem.Quantity > 0)
                if (equippedItem is BioticAbilityToolWrapper ability)
                    EquipBioticAbility(ability);

            _equipInitRoutine = null;
        }

        public void EquipBioticAbility(BioticAbilityToolWrapper bioticAbility)
        {
            UnequipBioticAbility();

            _bioticAbilityEquipped = bioticAbility;
            CurrentAbilityItemSo = bioticAbility;

            if (bioticAbility.bioticAbility == null)
            {
                Debug.LogWarning($"[PlayerEquippedAbility] {bioticAbility.name} has no BioticAbility assigned.");
                return;
            }

            if (bioticAbility.FPToolPrefab == null)
            {
                Debug.LogWarning($"[PlayerEquippedAbility] {bioticAbility.name} has no FPToolPrefab assigned.");
                return;
            }

            if (playerEquipment.IsRanged) return;

            // Instantiate the ability prefab
            var abilityGO = Instantiate(bioticAbility.FPToolPrefab, bioticAbilityAnchor, false);
            CurrentRuntimeAbility = abilityGO.GetComponent<IRuntimeBioticAbility>();

            if (CurrentRuntimeAbility == null)
            {
                Debug.LogWarning(
                    $"[PlayerEquippedAbility] {bioticAbility.name}'s prefab doesn't implement IRuntimeBioticAbility.");

                Destroy(abilityGO);
                return;
            }

            // Initialize the ability
            CurrentRuntimeAbility.Initialize(this);

            // Set ability data if the runtime ability supports it
            if (abilityGO.TryGetComponent<SingleBeamAbilityPrefab>(out var beamAbility))
                beamAbility.SetAbilityData(bioticAbility.bioticAbility);

            if (abilityGO.TryGetComponent<SingleProjectileAbilityPrefab>(out var projectileAbility))
                projectileAbility.SetAbilityData(bioticAbility.bioticAbility);

            if (abilityGO.TryGetComponent<AOEAbilityPrefab>(out var aoeAbility))
                aoeAbility.SetAbilityData(bioticAbility.bioticAbility);

            // Equip the ability
            CurrentRuntimeAbility.Equip();

            // Play feedbacks
            var fb = CurrentRuntimeAbility.GetEquipFeedbacks();
            fb?.PlayFeedbacks();

            Debug.Log($"[PlayerEquippedAbility] Equipped: {bioticAbility.ItemName}");
        }

        void UseCurrentBioticAbility()
        {
            if (CurrentRuntimeAbility == null || CurrentAbilityItemSo == null)
                return;

            // Check for cooldown on the ability wrapper
            if (CurrentAbilityItemSo.cooldown > 0f && Time.time < _nextUseTime)
            {
                Debug.Log("[PlayerEquippedAbility] Ability on cooldown");
                return;
            }

            // Charge contamination 

            // Use the ability
            CurrentRuntimeAbility.Use();

            if (showLeftArm && leftArmController != null && leftArmCast != null)
                leftArmController.PlayCast(leftArmCast);

            // Set next use time if there's a cooldown
            if (CurrentAbilityItemSo.cooldown > 0f) _nextUseTime = Time.time + CurrentAbilityItemSo.cooldown;
        }

        public void UnequipBioticAbility()
        {
            if (CurrentRuntimeAbility == null)
                return;

            // Get unequip feedbacks
            var fb = CurrentRuntimeAbility.GetUnequipFeedbacks();
            fb?.PlayFeedbacks();

            // Call unequip
            CurrentRuntimeAbility.Unequip();

            // Destroy the runtime ability GameObject
            if (CurrentRuntimeAbility is MonoBehaviour mb) Destroy(mb.gameObject);

            CurrentRuntimeAbility = null;
            CurrentAbilityItemSo = null;
            _bioticAbilityEquipped = null;

            Debug.Log("[PlayerEquippedAbility] Unequipped ability");
        }

#if UNITY_EDITOR
        // This will be called from the parent ScriptableObject
        IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            var parent = ControlsPromptSchemeSet._currentContextSO;
            if (parent == null || parent.inputManagerPrefab == null) yield break;

            var data = parent.inputManagerPrefab.userData;
            if (data == null) yield break;

            foreach (var action in data.GetActions_Copy())
                yield return new ValueDropdownItem<int>(action.name, action.id);
        }
#endif
    }
}
