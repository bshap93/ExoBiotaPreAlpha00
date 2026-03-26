using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dirigible.Input;
using Dirigible.Interactable;
using FirstPersonPlayer.Tools.Interface;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Inventory;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using SharedUI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dirigible
{
    public class DirigibleEquipment : MonoBehaviour, MMEventListener<MMInventoryEvent>
    {
        public enum DirigibleEquipmentSlot
        {
            Scanner
        }

        public MoreMountains.InventoryEngine.Inventory dEquipmentInventory; // Reference to the dirigible's inventory

        [SerializeField] DirigibleEquipmentSlot dEquipmentSlot = DirigibleEquipmentSlot.Scanner;
        [SerializeField] string dEquipmentInventoryName;

        [FormerlySerializedAs("toolAnchor")] public Transform moduleAnchor;

        [SerializeField] DirigibleInput rewiredInput;

        public DirigibleInteraction dirigibleInteraction;

#if UNITY_EDITOR

        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int ActionId;

        DirigibleFrontMountedModule _currentEquippedModule;

        Coroutine _equipInitRoutine;

        float _nextUseTime;

        public static DirigibleEquipment InstanceScanner { get; private set; }

        public DirigibleFrontMountedModule CurrentEquippedModuleSo { get; private set; }

        public IRuntimeDirigibleModule CurrentDirigModule { get; private set; }

        public static DirigibleEquipment Instance { get; private set; }

        void Awake()
        {
            // Register this instance
            if (dEquipmentSlot == DirigibleEquipmentSlot.Scanner)
                InstanceScanner = this;
            else
                Debug.LogError("DirigibleEquipment: Unknown equipment slot!");
        }

        void Update()
        {
            if (rewiredInput != null)
                if (rewiredInput.dirigibleAbilityController.applyAbility &&
                    rewiredInput.dirigibleAbilityController.activeAbilitySlot == DirigibleEquipmentSlot.Scanner)
                {
                    if (dEquipmentSlot == DirigibleEquipmentSlot.Scanner)
                        UseCurrentModule();
                    else
                        Debug.LogError("DirigibleEquipment: Unknown equipment slot!");
                }
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

            // ControlsHelpEvent.Trigger(ControlHelpEventType.Hide, ActionId);
        }

        public void OnMMEvent(MMInventoryEvent eventType)
        {
            if (eventType.TargetInventoryName != dEquipmentInventoryName) return;

            switch (eventType.InventoryEventType)
            {
                case MMInventoryEventType.ItemEquipped:
                    if (eventType.EventItem is DirigibleFrontMountedModule module) EquipModule(module);
                    break;
                case MMInventoryEventType.ItemUnEquipped:
                    if (eventType.EventItem is DirigibleFrontMountedModule) UnequipModule();
                    break;
                case MMInventoryEventType.ItemUsed:
                    if (eventType.EventItem is DirigibleFrontMountedModule) UseCurrentModule();
                    break;
            }
        }

        void EquipModule(DirigibleFrontMountedModule module)
        {
            UnequipModule();

            if (module.DirigibleModulePrefab == null)
            {
                Debug.LogWarning("DirigibleEquipment: Module Prefab is null!");
                return;
            }

            var go = Instantiate(module.DirigibleModulePrefab, moduleAnchor, false);

            CurrentDirigModule = go.GetComponent<IRuntimeDirigibleModule>();
            if (CurrentDirigModule == null)
            {
                Debug.LogWarning(
                    $"[{name} {module.name}'s prefab] DirigibleEquipment: IRuntimeTool component not found on module prefab!");

                Destroy(go);
                return;
            }

            CurrentEquippedModuleSo = module;
            CurrentDirigModule.Initialize(this);
            // ControlsHelpEvent.Trigger(ControlHelpEventType.Show, ActionId);
            _nextUseTime = 0f;
        }

        public void UnequipModule()
        {
            if (CurrentDirigModule is MonoBehaviour mb)
            {
                CurrentDirigModule.Unequip();
                Destroy(mb.gameObject);
            }

            CurrentDirigModule = null;
            CurrentEquippedModuleSo = null;
            // ControlsHelpEvent.Trigger(ControlHelpEventType.Hide, ActionId);
            _nextUseTime = 0f;
        }

        public InventoryItem GetCurrentlyEquippedModule()
        {
            if (dEquipmentSlot == DirigibleEquipmentSlot.Scanner)
                dEquipmentInventory =
                    MoreMountains.InventoryEngine.Inventory.FindInventory(
                        GlobalInventoryManager.DirigibleScannerInventoryName, "Player1");

            if (dEquipmentInventory == null)
            {
                Debug.LogError("DirigibleEquipment: Inventory not found!");
                return null;
            }

            // Assume the first item in the inventory is the equipped module

            var equippedModule = dEquipmentInventory.Content.FirstOrDefault();
            if (equippedModule != null &&
                (InventoryItem.IsNull(equippedModule) || equippedModule.Quantity <= 0)) return null;

            return equippedModule;
        }

        IEnumerator WaitForInventoryAndEquip()
        {
            try
            {
                dEquipmentInventory =
                    MoreMountains.InventoryEngine.Inventory.FindInventory(dEquipmentInventoryName, "Player1");
            }
            catch
            {
                Debug.LogWarning("DirigibleEquipment: Failed to find inventory " + dEquipmentInventoryName);
                _equipInitRoutine = null;
                yield break;
            }


            var timeoutAt = Time.realtimeSinceStartup + 2f;
            while (dEquipmentInventory == null || dEquipmentInventory.Content == null ||
                   !dEquipmentInventory.Content.Any(i => i != null))
            {
                if (Time.realtimeSinceStartup > timeoutAt)
                {
                    _equipInitRoutine = null;
                    yield break;
                }

                yield return null;
                dEquipmentInventory =
                    MoreMountains.InventoryEngine.Inventory.FindInventory(dEquipmentInventoryName, "Player1");
            }

            var equippedModule = GetCurrentlyEquippedModule();
            if (equippedModule != null && equippedModule.Equippable) equippedModule.Equip("Player1");

            _equipInitRoutine = null;
        }

        void UseCurrentModule()
        {
            if (CurrentDirigModule == null || CurrentEquippedModuleSo == null) return;

            if (CurrentEquippedModuleSo.Cooldown > 0f && Time.time < _nextUseTime) return;

            CurrentDirigModule.Use();

            // ControlsHelpEvent.Trigger(ControlHelpEventType.ShowUseThenHide, ActionId);

            if (CurrentEquippedModuleSo.Cooldown > 0f)
                _nextUseTime = Time.time + CurrentEquippedModuleSo.Cooldown;
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
