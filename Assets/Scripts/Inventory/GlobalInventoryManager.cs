using System;
using System.Collections.Generic;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.Events;
using Helpers.Events.Inventory;
using Helpers.Interfaces;
using Inventory.ScriptableObjects;
using Manager;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using SharedUI.Progression;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Inventory
{
    public class GlobalInventoryManager : MonoBehaviour, ICoreGameService, MMEventListener<GlobalInventoryEvent>,
        MMEventListener<LoadedManagerEvent>, MMEventListener<AttributeLevelUpEvent>
    {
        public enum EquippableType
        {
            LHand,
            RHand,
            Back,
            BioticAbility,
            NotEquippable,
            Unknown
        }

        public enum InventoryWithWeightLimit
        {
            PlayerMainInventory,
            DirigibleInventory,
            BioticAbilityInventory
        }


        public const string PlayerInventoryName = "PlayerMainInventory";
        public const string LeftHandEquipmentInventoryName = "LEquipmentInventory";
        public const string RightHandEquipmentInventoryName = "EquippedItemInventory";
        public const string BackEquipmentInventoryName = "BackEquippedItemInv";
        public const string BioticAbilityInventoryName = "BioticAbilityInventory";
        public const string DirigibleInventoryName = "DirigibleInventory";
        public const string DirigibleScannerInventoryName = "DirigMainScannerInventory";
        public const string KeyTypeInventoryName = "KeyItemsInventory";
        public const string OuterCoresInventoryName = "OuterCoresInventory";
        public const string AmmoInventoryName = "AmmoInventory";
        public const string AbilitiesBankInventoryName = "AbilitiesBankInventory";

        static string _savePath;
        static string _currentSceneName;

        [Header("Saves")] [SerializeField] bool autoSave; // <— NEW

        [Header("Player Main Inventory")] [FormerlySerializedAs("PlayerInventory")]
        public MoreMountains.InventoryEngine.Inventory playerInventory;

        [Header("First Person Slot Inventories")]
        public MoreMountains.InventoryEngine.Inventory lEquipmentInventory;

        public MoreMountains.InventoryEngine.Inventory equipmentInventory;
        public MoreMountains.InventoryEngine.Inventory backEquipmentInventory;
        public MoreMountains.InventoryEngine.Inventory bioticAbilityInventory;
        public MoreMountains.InventoryEngine.Inventory keyItemInventory;
        [FormerlySerializedAs("innerCoresInventory")]
        public MoreMountains.InventoryEngine.Inventory outerCoresInventory;
        public MoreMountains.InventoryEngine.Inventory ammoInventory;
        public MoreMountains.InventoryEngine.Inventory abilitiesBankInventory;

        [Header("Dirigible Inventory")] public MoreMountains.InventoryEngine.Inventory dirigibleInventory;

        [FormerlySerializedAs("dirigibleEquipmentInventory")] [Header("Dirigible Slot Inventories")]
        public MoreMountains.InventoryEngine.Inventory dirigibleScannerSlot;

        [Header("Default Items")] public DefaultInventoryDefinition playerStartingItems;
        public DefaultInventoryDefinition lEquipmentStartingItems;
        public DefaultInventoryDefinition equipmentStartingItems;
        public DefaultInventoryDefinition dirigibleStartingItems;
        public DefaultInventoryDefinition backEquipmentStartingItems;
        public DefaultInventoryDefinition bioticAbilityStartingItems;
        public DefaultInventoryDefinition dirigibleScannerStartingItems;
        public DefaultInventoryDefinition keyItemStartingItems;
        [FormerlySerializedAs("innerCoresStartingItems")]
        public DefaultInventoryDefinition outerCoresStartingItems;
        public DefaultInventoryDefinition ammoStartingItems;
        public DefaultInventoryDefinition abilitiesBankStartingItems;

        [FormerlySerializedAs("intitialPlayerFPMaxWeight")] [Header("Initial Weight Limits")] [SerializeField]
        float initialPlayerFPMaxWeight;
        [FormerlySerializedAs("maxDirigibleWeight")] [SerializeField]
        float initialMaxDirigibleWeight;


        [FormerlySerializedAs("PlayerId")] public string playerId = "Player1";

        [Header("Equippable Types")] public EquippableTypesDatatable equippableTypesTable;

        [FormerlySerializedAs("InventoryDatabaseVariable")]
        public InventoryDatabase inventoryDatabaseVariable;
        float _currentDirigibleWeight;
        float _currentPlayerFPWeight;
        bool _dirty; // <— NEW
        float _maxDirigibleWeight;

        float _maxPlayerFPWeight;
        bool _shouldEmptyBioticAbilityIntoPlayerInventoryOnQuit;

        bool _shouldEmptyRHandEquipIntoPlayerInventoryOnQuit;

        public Dictionary<string, EquippableType> ItemEquippableTypesDictionary;

        public static GlobalInventoryManager Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (SaveManager.Instance.saveManagersDontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            _currentSceneName = SceneManager.GetActiveScene().name;

            // Ensure defaults are set even before Load/Reset
            _maxPlayerFPWeight = initialPlayerFPMaxWeight;
            _maxDirigibleWeight = initialMaxDirigibleWeight;

            if (equippableTypesTable != null)
                ItemEquippableTypesDictionary = equippableTypesTable.ToDictionary();
            else
                Debug.LogWarning("EquippableTypesDatatable is not assigned in GlobalInventoryManager.");
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _savePath = GetSaveFilePath();
        }

        void OnEnable()
        {
            this.MMEventStartListening<GlobalInventoryEvent>();
            this.MMEventStartListening<LoadedManagerEvent>();
            this.MMEventStartListening<AttributeLevelUpEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<GlobalInventoryEvent>();
            this.MMEventStopListening<LoadedManagerEvent>();
            this.MMEventStopListening<AttributeLevelUpEvent>();
        }


        public void Save()
        {
            SaveGlobalInventories();
            ES3.Save("CurrentFPWeightCarried", _currentPlayerFPWeight, _savePath);
            ES3.Save("MaxFPWeight", _maxPlayerFPWeight, _savePath);
            ES3.Save("CurrentDirigibleWeightCarried", _currentDirigibleWeight, _savePath);
            ES3.Save("MaxDirigibleWeight", _maxDirigibleWeight, _savePath);
            _dirty = false; // <— NEW
        }

        public void Load()
        {
            LoadGlobalInventories();

            LoadWeightValues();

            _dirty = false; // <— NEW
        }

        public void Reset()
        {
            ResetGlobalInventories();
            PopulateInventoriesFromDefaults();
            _maxDirigibleWeight = initialMaxDirigibleWeight;
            _maxPlayerFPWeight = initialPlayerFPMaxWeight;
            _currentDirigibleWeight = 0f;
            _currentPlayerFPWeight = 0f;
            _dirty = true;
            ConditionalSave();
        }

        public void ConditionalSave()
        {
            if (autoSave && _dirty) Save();
        }


        public void MarkDirty()
        {
            _dirty = true;
        }

        public string GetSaveFilePath()
        {
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.GlobalInventorySave);
        }

        public void CommitCheckpointSave()
        {
            if (_dirty) Save();
        }

        public bool HasSavedData()
        {
            return ES3.FileExists(GetSaveFilePath());
        }
        public void OnMMEvent(AttributeLevelUpEvent eventType)
        {
            if (eventType.AttributeType == AttributeType.Strength)
                switch (eventType.NewLevel)
                {
                    case 1:
                        break;
                    default:
                        _maxPlayerFPWeight += 10f;
                        MarkDirty();
                        ConditionalSave();
                        break;
                }
        }

        public void OnMMEvent(GlobalInventoryEvent eventType)
        {
            if (eventType.EventType == GlobalInventoryEventType.UnequipRightHandTool)
                MMInventoryEvent.Trigger(
                    MMInventoryEventType.UnEquipRequest, null, equipmentInventory.name, equipmentInventory.Content[0],
                    0, 0, equipmentInventory.PlayerID);
        }
        public void OnMMEvent(LoadedManagerEvent eventType)
        {
            if (eventType.ManagerType == ManagerType.All)
            {
            }
        }
        void LoadWeightValues()
        {
            _savePath = GetSaveFilePath();
            // If no saved inventory exists, populate from defaults
            if (!HasSavedData())
            {
                // Set current player fp weight to default
                _currentPlayerFPWeight = 0f;
                _maxPlayerFPWeight = initialPlayerFPMaxWeight;
                _currentDirigibleWeight = 0f;
                _maxDirigibleWeight = initialMaxDirigibleWeight;
            }

            if (ES3.KeyExists("CurrentFPWeightCarried", _savePath))
                _currentPlayerFPWeight = ES3.Load<float>("CurrentFPWeightCarried", _savePath, 0);

            if (ES3.KeyExists("MaxFPWeight", _savePath))
                _maxPlayerFPWeight = ES3.Load("MaxFPWeight", _savePath, initialPlayerFPMaxWeight);

            if (ES3.KeyExists("CurrentDirigibleWeightCarried", _savePath))
                _currentDirigibleWeight = ES3.Load<float>("CurrentDirigibleWeightCarried", _savePath, 0);

            if (ES3.KeyExists("MaxDirigibleWeight", _savePath))
                _maxDirigibleWeight = ES3.Load("MaxDirigibleWeight", _savePath, initialMaxDirigibleWeight);
        }

        public float GetDirigibleMaxWeight()
        {
            return _maxDirigibleWeight;
        }
        public float GetPlayerMaxWeight()
        {
            return _maxPlayerFPWeight;
        }

        public float GetWeightOfPlayerMainPlusEquipped()
        {
            var totalWeight = 0f;
            totalWeight += GetWeightOfInventoryItems(playerInventory);
            totalWeight += GetWeightOfInventoryItems(equipmentInventory);
            totalWeight += GetWeightOfInventoryItems(lEquipmentInventory);
            totalWeight += GetWeightOfInventoryItems(backEquipmentInventory);
            // Note: Key items are not counted towards weight
            return totalWeight;
        }

        public float GetTotalWeightInDirigible()
        {
            var totalWeight = 0f;
            totalWeight += GetWeightOfInventoryItems(dirigibleInventory);
            totalWeight += GetWeightOfInventoryItems(dirigibleScannerSlot);
            totalWeight += GetWeightOfPlayerMainPlusEquipped();
            // Note: Key items are not counted towards weight
            return totalWeight;
        }

        float GetWeightOfInventoryItems(MoreMountains.InventoryEngine.Inventory inventory)
        {
            var totalWeight = 0f;
            foreach (var item in inventory.Content)
            {
                var myBaseItem = item as MyBaseItem;
                if (myBaseItem == null) continue;
                var itemWeight = myBaseItem.weight * item.Quantity;
                totalWeight += itemWeight;
            }


            return totalWeight;
        }

        public bool IsDontDestroyOnLoad()
        {
            return SaveManager.Instance.saveManagersDontDestroyOnLoad;
        }

        public void AddItemTo(MoreMountains.InventoryEngine.Inventory inv, InventoryItem item, int quantity)
        {
            if (inv == null || item == null) return;
            inv.AddItem(item, Math.Max(1, quantity));
            MarkDirty(); // <— NEW
            ConditionalSave(); // <— NEW
        }


        void PopulateInventoriesFromDefaults()
        {
            PopulateInventory(playerInventory, playerStartingItems);
            PopulateInventory(equipmentInventory, equipmentStartingItems);
            PopulateInventory(lEquipmentInventory, lEquipmentStartingItems);
            PopulateInventory(dirigibleInventory, dirigibleStartingItems);
            PopulateInventory(backEquipmentInventory, backEquipmentStartingItems);
            PopulateInventory(dirigibleScannerSlot, dirigibleScannerStartingItems);
            PopulateInventory(keyItemInventory, keyItemStartingItems);
            PopulateInventory(outerCoresInventory, outerCoresStartingItems);
            PopulateInventory(bioticAbilityInventory, bioticAbilityStartingItems);
            PopulateInventory(ammoInventory, ammoStartingItems);
            PopulateInventory(abilitiesBankInventory, abilitiesBankStartingItems);
        }

        static void PopulateInventory(MoreMountains.InventoryEngine.Inventory inv,
            DefaultInventoryDefinition def)
        {
            if (inv == null || def == null) return;

            var size = Mathf.Max(def.inventorySize, def.defaultItems.Length);
            inv.ResizeArray(size); // ensure enough slots
            inv.EmptyInventory(); // start clean

            foreach (var item in def.defaultItems)
            {
                if (item == null) continue;
                inv.AddItem(
                    item.Copy(), // never add the SO instance itself
                    Math.Max(1, item.Quantity)); // 1 if Quantity not set at runtime
            }
        }

        public void SaveGlobalInventories()
        {
            SaveOne(playerInventory);
            SaveOne(equipmentInventory);

            SaveOne(lEquipmentInventory);
            SaveOne(dirigibleInventory);
            SaveOne(backEquipmentInventory);
            SaveOne(dirigibleScannerSlot);
            SaveOne(keyItemInventory);
            SaveOne(outerCoresInventory);
            SaveOne(bioticAbilityInventory);
            SaveOne(ammoInventory);
            SaveOne(abilitiesBankInventory);
        }
        // void FlagToEmptyIntoInventoryOnQuit(MoreMountains.InventoryEngine.Inventory equipInventory,
        //     MoreMountains.InventoryEngine.Inventory playerInventory1)
        // {
        //     if (equipInventory == null || playerInventory1 == null) return;
        // }
        void TryEmptyIntoInventory(MoreMountains.InventoryEngine.Inventory equipInventory,
            MoreMountains.InventoryEngine.Inventory playerInventory1)
        {
            if (equipInventory == null || playerInventory1 == null) return;

            var itemsToMove = new List<InventoryItem>(equipInventory.Content);
            foreach (var item in itemsToMove)
            {
                if (item == null) continue;
                playerInventory1.AddItem(item, Math.Max(1, item.Quantity));
                equipInventory.RemoveItem(0, item.Quantity);
            }
        }

        public void ResetGlobalInventories()
        {
            ResetOne(playerInventory);
            ResetOne(equipmentInventory);
            ResetOne(lEquipmentInventory);
            ResetOne(dirigibleInventory);
            ResetOne(backEquipmentInventory);
            ResetOne(dirigibleScannerSlot);
            ResetOne(keyItemInventory);
            ResetOne(outerCoresInventory);
            ResetOne(bioticAbilityInventory);
            ResetOne(ammoInventory);
            ResetOne(abilitiesBankInventory);
        }

        public void LoadGlobalInventories()
        {
            LoadOne(playerInventory);
            LoadOne(equipmentInventory);
            LoadOne(lEquipmentInventory);
            LoadOne(dirigibleInventory);
            LoadOne(backEquipmentInventory);
            LoadOne(dirigibleScannerSlot);
            LoadOne(keyItemInventory);
            LoadOne(outerCoresInventory);
            LoadOne(bioticAbilityInventory);
            LoadOne(ammoInventory);
            LoadOne(abilitiesBankInventory);
            TryEmptyIntoInventory(equipmentInventory, playerInventory);
            TryEmptyIntoInventory(bioticAbilityInventory, playerInventory);
        }

        static void SaveOne(MoreMountains.InventoryEngine.Inventory inv)
        {
            if (inv != null && inv.Persistent)
                inv.SaveInventory();
        }

        static void LoadOne(MoreMountains.InventoryEngine.Inventory inv)
        {
            if (inv != null && inv.Persistent)
                inv.LoadSavedInventory();
        }

        static void ResetOne(MoreMountains.InventoryEngine.Inventory inv)
        {
            if (inv != null && inv.Persistent)
            {
                inv.ResetSavedInventory();
                inv.EmptyInventory();
            }
        }

        public EquippableType GetEquippableType(InventoryItem itemVar)
        {
            if (itemVar == null) throw new ArgumentNullException(nameof(itemVar), "Item cannot be null");

            if (equippableTypesTable.entries.Exists(e => e.ItemID == itemVar.ItemID))
            {
                // Find the entry in the equippable types table
                var entry = equippableTypesTable.entries.Find(e => e.ItemID == itemVar.ItemID);
                return entry.Type;
            }


            // Default to NotEquippable if not found
            return EquippableType.Unknown;
        }

        public InventoryItem CreateItem(string itemId, int amount = 1)
        {
            if (inventoryDatabaseVariable == null)
            {
                Debug.LogError("InventoryDatabase not assigned in GlobalInventoryManager!");
                return null;
            }

            // Always return a single-unit instance; let AddItem decide how many to add.
            return inventoryDatabaseVariable.CreateItem(itemId);
        }
        public float GetMaxWeightOfPlayerCarry()
        {
            return _maxPlayerFPWeight;
        }

        public float GetMaxWeightOfDirigibleCarry()
        {
            return _maxDirigibleWeight;
        }
        public float GetWeightOfInventory(MoreMountains.InventoryEngine.Inventory inventory)
        {
            if (inventory == null) return 0f;
            return GetWeightOfInventoryItems(inventory);
        }
        public bool HasKeyForDoor(string keyID)
        {
            foreach (var item in keyItemInventory.Content)
            {
                var keyItem = item as KeyItemObject;
                if (keyItem != null && keyItem.KeyID == keyID) return true;
            }

            return false;
        }
        public int GetNumberOfOuterCoresInInventory(OuterCoreItemObject.CoreObjectValueGrade standardGrade)
        {
            var total = 0;
            foreach (var item in outerCoresInventory.Content)
            {
                var outerCore = item as OuterCoreItemObject;
                if (outerCore != null && outerCore.coreObjectValueGrade == standardGrade) total += item.Quantity;
            }

            return total;
        }
        public int GetHighestPriorityOuterCore()
        {
            var highestPriorityCoreIndex = -1;
            for (var i = 0; i < playerInventory.Content.Length; i++)
            {
                var item = playerInventory.Content[i];
                var outerCore = item as OuterCoreItemObject;
                if (outerCore != null && outerCore.corePriorityLevel > highestPriorityCoreIndex)
                    highestPriorityCoreIndex = i;
            }

            return highestPriorityCoreIndex;
        }
        public bool IsItemIDaType<T>(string itemID)
        {
            if (string.IsNullOrEmpty(itemID)) return false;

            if (inventoryDatabaseVariable == null)
            {
                Debug.LogWarning("[GlobalInventoryManager] InventoryDatabaseVariable is null!");
                return false;
            }

            var item = inventoryDatabaseVariable.CreateItem(itemID);
            if (item == null) return false;

            // Check if the item is a BaseTool
            return item is T;
        }
        public bool IsItemIDaConsumableEffectItem(string itemID)
        {
            if (string.IsNullOrEmpty(itemID)) return false;

            if (inventoryDatabaseVariable == null)
            {
                Debug.LogWarning("[GlobalInventoryManager] InventoryDatabaseVariable is null!");
                return false;
            }

            var item = inventoryDatabaseVariable.CreateItem(itemID);
            if (item == null) return false;

            // Check if the item is a ConsumableEffectItem
            return item is ConsumableEffectItem;
        }
        public int GetTotalNumberOfCores()
        {
            var total = 0;
            foreach (var item in outerCoresInventory.Content)
            {
                var outerCore = item as OuterCoreItemObject;
                if (outerCore != null) total += item.Quantity;
            }

            return total;
        }
        public int GetTotalQuantityOfItem(string itemId)
        {
            var total = 0;
            foreach (var inventory in new[]
                     {
                         playerInventory, equipmentInventory, lEquipmentInventory, backEquipmentInventory,
                         bioticAbilityInventory, keyItemInventory, outerCoresInventory, ammoInventory,
                         abilitiesBankInventory
                     })
            {
                if (inventory == null) continue;
                foreach (var item in inventory.Content)
                    if (item != null && item.ItemID == itemId)
                        total += item.Quantity;
            }

            return total;
        }
    }
}
