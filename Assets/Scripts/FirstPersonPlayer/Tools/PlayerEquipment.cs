using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FirstPersonPlayer.InputHandling;
using FirstPersonPlayer.Interactable;
using FirstPersonPlayer.Tools.Animation;
using FirstPersonPlayer.Tools.Interface;
using FirstPersonPlayer.Tools.ItemObjectTypes.Tools;
using FirstPersonPlayer.Tools.ToolPrefabScripts;
using FirstPersonPlayer.UI.ProgressBars;
using Helpers.AnimancerHelper;
using Helpers.Events;
using Helpers.ScriptableObjects.Animation;
using Manager;
using Manager.Global;
using MoreMountains.Feedbacks;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using SharedUI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Inputs;

// [WIP] Placeholder for revised First Person Interaction-Inventory system	

namespace FirstPersonPlayer.Tools
{
    public class PlayerEquipment : MonoBehaviour, MMEventListener<MMInventoryEvent>
    {
        public bool autoEquipLightSourceInLeftHand;

#if UNITY_EDITOR
        [FormerlySerializedAs("ActionId")] [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int actionId;

        public BaseTool[] rhEquipToolsToShowLeftHand;


        public MoreMountains.InventoryEngine.Inventory equipmentInventory; // Reference to the player's inventory

        [SerializeField] PlayerEquippedAbility equippedAbility;

        [SerializeField] string equipmentInventoryName = "EquippedItemInventory"; // set per hand


        // Used for tools that are Right handed but not to be held in Arm Hand
        public Transform secondaryToolAnchor;

        [FormerlySerializedAs("ProgressBarBlue")]
        public ProgressBarBlue progressBarBlue;

        [SerializeField] RewiredFirstPersonInputs rewiredInput;

        // public TerrainLayerDetector terrainLayerDetector;

        public PlayerInteraction playerInteraction;


        public bool emptyHandEnabled;

        [FormerlySerializedAs("EquipObjectivesArray")]
        public EquipObjectives[] equipObjectivesArray;

        [Header("Hand Dependent")] [Header("Right Hand")] [SerializeField]
        ToolAnimationSet emptyHandAnimationSet;
        [FormerlySerializedAs("toolAnchor")] public Transform primaryToolAnchor;
        [FormerlySerializedAs("rightArmObject")]
        [FormerlySerializedAs("armsGameObject")]
        [FormerlySerializedAs("rightArmGameObject")]
        public GameObject primaryArmsObject;
        [FormerlySerializedAs("animancerRightArmController")]
        public AnimancerArmController animancerPrimaryArmsController;

        [FormerlySerializedAs("shooterHandsAnimationSet")] [Header("Shooter Hands")] [SerializeField]
        // ToolAnimationSet shooterHandsEmptyAnimationSet;
        public Transform shooterHandsToolAnchor;
        [FormerlySerializedAs("shooterHandsGameObject")]
        public GameObject shooterArmsGameObject;
        public AnimancerArmController animancerShooterHandsController;
        public MyNormalMovement playerNormalMovement;

        [SerializeField] AnimancerArmController _currentAnimancerArmController;
        [SerializeField] GameObject _currentArmsObject;

        [SerializeField] HandConfig _currentHandConfig = HandConfig.Primary;
        [SerializeField] Transform _currentToolAnchor;

        Coroutine _equipInitRoutine;

        float _nextUseTime;

        bool _wasUseButtonHeldLastFrame;

        public bool IsRanged => CurrentToolSo != null && CurrentToolSo.canBeAimed;

        public bool IsBlocking { get; private set; }
        public bool IsAiming { get; private set; }

        public static PlayerEquipment InstanceRight { get; private set; }
        public static PlayerEquipment InstanceLeft { get; private set; }

        public BaseTool CurrentToolSo { get; private set; }
        public IRuntimeTool CurrentRuntimeTool { get; private set; }

        public static PlayerEquipment Instance { get; private set; }

        void Awake()
        {
            // Register this instance

            InstanceRight = this;
            if (Instance == null) Instance = this; // back-compat: default singleton = Right
        }

        void Start()
        {
            if (CurrentRuntimeTool == null && !emptyHandEnabled)
                primaryArmsObject.SetActive(false);

            _currentToolAnchor = primaryToolAnchor;
            _currentArmsObject = primaryArmsObject;
            _currentAnimancerArmController = animancerPrimaryArmsController;
            playerNormalMovement.animancerArmController = _currentAnimancerArmController;
        }

        void Update()
        {
            if (rewiredInput != null)
            {
                HandleToolInput(rewiredInput.useEquipped, rewiredInput.heldEquipped);


                if (CurrentToolSo != null && CurrentToolSo.canBeAimed) HandleToolAim(rewiredInput.itemUseModifierHeld);

                if (CurrentToolSo != null && CurrentToolSo.canBlock)
                    HandleToolBlock(rewiredInput.itemUseModifierHeld);

                if (CurrentToolSo != null && CurrentToolSo.canUseSingleShotSecondaryAction)
                    HandleSingleShotSecondary(rewiredInput.itemUseModifierDown);
            }
        }


        void OnEnable()
        {
            this.MMEventStartListening();

            // Start a short, frame-by-frame wait for the inventory to actually have a non-null slot
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
            if (InstanceRight == this) InstanceRight = null;
            if (Instance == this) Instance = InstanceRight; // keep compat fallback
        }

        public void OnMMEvent(MMInventoryEvent e)
        {
            if (e.TargetInventoryName != equipmentInventoryName) return;

            switch (e.InventoryEventType)
            {
                case MMInventoryEventType.ItemEquipped:
                    if (e.EventItem is BaseTool tool) EquipTool(tool);


                    break;

                case MMInventoryEventType.ItemUnEquipped:
                    if (e.EventItem is BaseTool) UnequipTool();
                    break;

                case MMInventoryEventType.ItemUsed:
                    if (e.EventItem is BaseTool) UseCurrentTool();
                    break;
                case MMInventoryEventType.Destroy:
                    if (e.EventItem is BaseTool) UnequipTool();
                    break;
            }
        }
        void HandleSingleShotSecondary(bool rewiredInputItemUseModifierDown)
        {
            if (rewiredInputItemUseModifierDown)
            {
                var liquidTool = CurrentRuntimeTool as LiquidSampleTool;
                if (liquidTool == null) return;
                var toolMgr = ToolsStateManager.Instance;
                if (toolMgr == null) return;
                if (toolMgr.CurrentIchorCharges <= 0) return;

                var successfulAnimationStart = _currentAnimancerArmController.EnterIntoInjectionAnimation();

                if (!successfulAnimationStart) return;

                StartCoroutine(WaitForAnimationAndInject(liquidTool, 1f));
            }


            // liquidTool.InjectAvailableIchor();
        }
        IEnumerator WaitForAnimationAndInject(LiquidSampleTool liquidTool, float delay)
        {
            yield return new WaitForSeconds(delay);

            liquidTool.InjectAvailableIchor();
        }

        public void SwitchToPrimaryArms()
        {
            _currentHandConfig = HandConfig.Primary;
            _currentToolAnchor = primaryToolAnchor;
            _currentArmsObject = primaryArmsObject;
            _currentAnimancerArmController = animancerPrimaryArmsController;
            playerNormalMovement.animancerArmController = _currentAnimancerArmController;
            primaryArmsObject.SetActive(true);
            shooterArmsGameObject.SetActive(false);
        }

        public void SwitchToShooterHands()
        {
            _currentHandConfig = HandConfig.ShooterHands;
            _currentToolAnchor = shooterHandsToolAnchor;
            _currentArmsObject = shooterArmsGameObject;
            _currentAnimancerArmController = animancerShooterHandsController;
            playerNormalMovement.animancerArmController = _currentAnimancerArmController;
            primaryArmsObject.SetActive(false);
            shooterArmsGameObject.SetActive(true);
        }

        void HandleToolBlock(bool shouldBlock)
        {
            if (_currentAnimancerArmController == null) return;

            if (shouldBlock)
            {
                IsBlocking = true;
                if (playerNormalMovement != null) playerNormalMovement.isBlocking = true;
                _currentAnimancerArmController.EnterIntoBlockState();
            }
            else if (IsBlocking)
            {
                IsAiming = false;
                if (playerNormalMovement != null) playerNormalMovement.IsAiming = false;
                IsBlocking = false;
                _currentAnimancerArmController.ReturnFromBlockState();
            }
            else
            {
                IsBlocking = false;
            }
        }


        void HandleToolAim(bool shouldAim)
        {
            if (_currentAnimancerArmController == null) return;
            var rangedTool = CurrentRuntimeTool as RangedToolPrefab;

            if (shouldAim)
            {
                IsAiming = true;
                if (playerNormalMovement != null) playerNormalMovement.IsAiming = true;
                _currentAnimancerArmController.EnterIntoAimState();
                if (rangedTool != null) rangedTool.EnterIntoAimState();
            }
            else if (IsAiming)
            {
                IsAiming = false;
                if (playerNormalMovement != null) playerNormalMovement.IsAiming = false;
                _currentAnimancerArmController.ReturnFromAimState();
                if (rangedTool != null) rangedTool.ExitFromAimState();
            }
        }

        public bool AreBothHandsOccupied()
        {
            if (CurrentToolSo == null) return false;

            if (CurrentToolSo.occupiesBothHands) return true;

            return false;
        }

        void HandleToolInput(bool useButtonPressed, bool useButtonHeld)
        {
            if (CurrentRuntimeTool == null || CurrentToolSo == null)
            {
                _wasUseButtonHeldLastFrame = useButtonHeld;
                return;
            }

            var pauseManager = PauseManager.Instance;
            if (pauseManager.IsPaused()) return;

            var justPressed = useButtonHeld && !_wasUseButtonHeldLastFrame;
            var justReleased = !useButtonHeld && _wasUseButtonHeldLastFrame;

            if (justPressed)
            {
                if (CurrentToolSo.cooldown > 0f && Time.time < _nextUseTime)
                {
                    _wasUseButtonHeldLastFrame = useButtonHeld;
                    return;
                }

                // Notify tool that use started (for animation control)
                if (CurrentRuntimeTool is IToolAnimationControl animControl) animControl.OnUseStarted();

                if (CurrentToolSo.cooldown > 0f)
                    _nextUseTime = Time.time + CurrentToolSo.cooldown;
            }


            if (CurrentRuntimeTool.ToolIsUsedOnRelease())
            {
                if (useButtonHeld) CurrentRuntimeTool.ChargeUse(justPressed);

                if (justReleased) CurrentRuntimeTool.Use();
            }
            else if (CurrentRuntimeTool.ToolMustBeHeldToUse())
            {
                if (useButtonHeld)
                    CurrentRuntimeTool.Use();
            }
            else if (useButtonPressed)
            {
                CurrentRuntimeTool.Use();
            }

            if (justReleased)
                // Notify tool that use stopped (for animation control)
                if (CurrentRuntimeTool is IToolAnimationControl animControl)
                    animControl.OnUseStopped();

            _wasUseButtonHeldLastFrame = useButtonHeld;
        }


        IEnumerator WaitForInventoryAndEquip()
        {
            // Use your existing inventory lookup; no refactor beyond deferring.
            equipmentInventory =
                MoreMountains.InventoryEngine.Inventory.FindInventory(equipmentInventoryName, "Player1");

            // Wait until the inventory exists AND has at least one non-null slot.
            // (You mentioned the slot exists but is null immediately after death.)
            var timeoutAt = Time.realtimeSinceStartup + 2f; // safety timeout to avoid infinite wait


            while (equipmentInventory == null
                   || equipmentInventory.Content == null
                   || !equipmentInventory.Content.Any(i => i != null))
            {
                if (Time.realtimeSinceStartup > timeoutAt)
                {
                    _equipInitRoutine = null; // give up quietly if nothing to equip
                    yield break;
                }

                yield return null;
                // Re-check next frame
                equipmentInventory =
                    MoreMountains.InventoryEngine.Inventory.FindInventory(equipmentInventoryName, "Player1");
            }

            // Now run your existing equip attempt exactly as before.
            var equippedItem = GetCurrentlyEquippedItem();
            if (equippedItem != null && equippedItem.Equippable)
                if (equippedItem is BaseTool tool)
                {
                    if (tool.occupiesBothHands && _currentHandConfig != HandConfig.ShooterHands)
                        SwitchToShooterHands();
                    else if (!tool.occupiesBothHands && _currentHandConfig != HandConfig.Primary) SwitchToPrimaryArms();

                    equippedItem.Equip("Player1");
                }


            _equipInitRoutine = null;
        }

        // Active tool helpers (nice for UI/reticle)
        public static PlayerEquipment GetWithActiveToolOrRight()
        {
            if (InstanceRight?.CurrentRuntimeTool != null) return InstanceRight;

            return InstanceRight;
        }
        // Example tool-type query
        public static PlayerEquipment GetWithToolType(Type runtimeType)
        {
            if (InstanceRight?.CurrentRuntimeTool != null &&
                runtimeType.IsInstanceOfType(InstanceRight.CurrentRuntimeTool)) return InstanceRight;

            if (InstanceLeft?.CurrentRuntimeTool != null &&
                runtimeType.IsInstanceOfType(InstanceLeft.CurrentRuntimeTool)) return InstanceLeft;

            return null;
        }


        void EquipTool(BaseTool tool)
        {
            UnequipTool();

            if (tool.FPToolPrefab == null)
            {
                Debug.LogWarning($"[{name}] {tool.name} has no Prefab assigned.");
                return;
            }

            if (tool.occupiesBothHands)
                SwitchToShooterHands();
            else
                SwitchToPrimaryArms();

            GameObject myGameObject;

            if (tool.hidesArmWhenEquipped)
            {
                myGameObject = Instantiate(tool.FPToolPrefab, secondaryToolAnchor, false);
                var toolBob = myGameObject.GetComponent<ToolBob>();
                if (toolBob != null)
                {
                    toolBob.enabled = true;
                    toolBob.Initialize();
                }

                if (primaryArmsObject != null) primaryArmsObject.SetActive(false);
            }
            else
            {
                myGameObject = Instantiate(tool.FPToolPrefab, _currentToolAnchor, false);
                if (_currentArmsObject != null) _currentArmsObject.SetActive(true);
                _currentAnimancerArmController.currentToolAnimationSet = tool.toolAnimationSet;
                _currentAnimancerArmController.UpdateAnimationSet();
            }


            // var go = Instantiate(tool.FPToolPrefab, primaryToolAnchor, false);
            CurrentRuntimeTool = myGameObject.GetComponent<IRuntimeTool>();
            if (CurrentRuntimeTool == null)
            {
                if (CurrentRuntimeTool != null) CurrentRuntimeTool.Equip();
                Debug.LogWarning($"[{name}] {tool.name}'s prefab doesn't implement IRuntimeTool.");
                Destroy(myGameObject);
                _currentArmsObject.SetActive(false);
                return;
            }


            CurrentToolSo = tool;
            if (CurrentRuntimeTool is RangedToolPrefab) equippedAbility.UnequipAbility();


            // if (!CurrentToolSo.hidesArmWhenEquipped) _currentArmsObject.SetActive(true);


            CurrentRuntimeTool.Initialize(this);

            if (CurrentRuntimeTool is IToolAnimationControl animControl) animControl.OnEquipped();

            if (CurrentToolSo != null)
                if (CurrentToolSo.hasObjectivesEquipping)
                    foreach (var equipObjective in equipObjectivesArray)
                        if (equipObjective.toolScriptableObjectId == CurrentToolSo.ItemID)
                            if (equipObjective.onEventEquipment == EquipObjectives.OnEvent.OnEquip)
                                ObjectiveEvent.Trigger(
                                    equipObjective.objectiveToCompleteId, ObjectiveEventType.ObjectiveCompleted
                                );

            var fb = CurrentRuntimeTool.GetEquipFeedbacks();

            fb?.PlayFeedbacks();
            _nextUseTime = 0f;
        }

        public void UnequipTool()
        {
            MMFeedbacks fb = null;
            if (CurrentRuntimeTool != null)
            {
                fb = CurrentRuntimeTool.GetUnequipFeedbacks();
                if (fb != null) fb.PlayFeedbacks();
            }

            if (CurrentRuntimeTool is MonoBehaviour mb)
            {
                CurrentRuntimeTool.Unequip();
                Destroy(mb.gameObject);
            }

            CurrentRuntimeTool = null;

            // primaryArmsObject.SetActive(false);
            if (_currentHandConfig != HandConfig.Primary) SwitchToPrimaryArms();

            if (animancerPrimaryArmsController != null)
            {
                if (primaryArmsObject != null) primaryArmsObject.SetActive(true);
                if (emptyHandEnabled)
                {
                    animancerPrimaryArmsController.currentToolAnimationSet = emptyHandAnimationSet;
                    animancerPrimaryArmsController.UpdateAnimationSet();
                }
                else


                {
                    animancerPrimaryArmsController.gameObject.SetActive(false);
                    animancerPrimaryArmsController.currentToolAnimationSet = null;
                }
            }


            CurrentToolSo = null;
            _nextUseTime = 0f;
        }

        public void UseCurrentTool()
        {
            if (CurrentRuntimeTool == null || CurrentToolSo == null) return;

            if (CurrentToolSo.cooldown > 0f && Time.time < _nextUseTime) return;

            CurrentRuntimeTool.Use();


            if (CurrentToolSo != null && CurrentToolSo.cooldown > 0f)
                _nextUseTime = Time.time + CurrentToolSo.cooldown;
        }

        public InventoryItem GetCurrentlyEquippedItem()
        {
            equipmentInventory =
                MoreMountains.InventoryEngine.Inventory.FindInventory("EquippedItemInventory", "Player1");

            if (equipmentInventory == null)
            {
                Debug.LogError("Equipment inventory is not assigned.");
                return null;
            }

            // Assuming the first item in the inventory is the equipped item

            var equippedItem = equipmentInventory.Content.FirstOrDefault();
            if (equippedItem != null && (InventoryItem.IsNull(equippedItem) || equippedItem.Quantity <= 0)) return null;

            return equippedItem;
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

        public float GetCurrentToolRange()
        {
            return 5f;
        }

        enum HandConfig
        {
            Primary,
            ShooterHands
        }

        [Serializable]
        public class EquipObjectives
        {
            [Serializable]
            public enum OnEvent
            {
                OnEquip,
                OnUnequip
            }

            [FormerlySerializedAs("ObjectiveToCompleteId")]
            public string objectiveToCompleteId;
            [FormerlySerializedAs("OnEventEquipment")]
            public OnEvent onEventEquipment;
            [FormerlySerializedAs("ToolScriptableObjectId")]
            public string toolScriptableObjectId;
        }
    }
}
