using System;
using System.Collections;
using System.Linq;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.Events;
using Helpers.Events.Inventory;
using Helpers.Events.UI;
using Helpers.Interfaces;
using Inventory;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Manager.UI
{
    public class HotbarManager : MonoBehaviour, ICoreGameService, MMEventListener<HotbarEvent>
    {
        [SerializeField] int fpToolHotbarSize = 4;
        [SerializeField] int fpConsumableHotbarSize = 2;

        [FormerlySerializedAs("_inventoryManager")] [SerializeField]
        GlobalInventoryManager inventoryManager;
        public bool autoSave;

        [SerializeField] bool startEverySceneWithEmptyHandSlot;

        int _currentConsumableHotbarIndex;
        int _currentToolHotbarIndex;


        bool _dirty;
        // Quantity of each consumable item in hotbar
        ItemHotbarData[] _fpConsumableHotbarItems;
        // Quantity not shown in hotbar, but used to answer questions like:
        // If I sell or drop this tool, does the hotbar need to change?
        ItemHotbarData[] _fpToolHotbarItems;
        string _savePath;

        public static HotbarManager Instance { get; private set; }

        void Awake()
        {
            if (Instance == null) Instance = this;
            else
                Destroy(gameObject);
        }

        void Start()
        {
            _savePath = GetSaveFilePath();

            if (!HasSavedData())
            {
                Reset();
                return;
            }

            Load();

            if (inventoryManager == null) inventoryManager = GlobalInventoryManager.Instance;
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void Save()
        {
            var path = GetSaveFilePath();

            // Save consumable hotbar
            var consumableData = new HotbarSaveData
            {
                items = _fpConsumableHotbarItems ?? new ItemHotbarData[fpConsumableHotbarSize],
                currentIndex = _currentConsumableHotbarIndex
            };


            ES3.Save("ConsumableHotbar", consumableData, path);

            // Save tool hotbar
            var toolData = new HotbarSaveData
            {
                items = _fpToolHotbarItems ?? new ItemHotbarData[fpToolHotbarSize],
                currentIndex = startEverySceneWithEmptyHandSlot ? 0 : _currentToolHotbarIndex
            };

            ES3.Save("ToolHotbar", toolData, path);

            _dirty = false;
        }

        public void Load()
        {
            var path = GetSaveFilePath();

            if (ES3.KeyExists("ConsumableHotbar", path))
            {
                var consumableData = ES3.Load<HotbarSaveData>("ConsumableHotbar", path);
                _fpConsumableHotbarItems = consumableData.items;
                _currentConsumableHotbarIndex = consumableData.currentIndex;
            }
            else
            {
                _fpConsumableHotbarItems = new ItemHotbarData[fpConsumableHotbarSize];
                for (var i = 0; i < fpConsumableHotbarSize; i++) _fpConsumableHotbarItems[i] = new ItemHotbarData();
            }

            if (ES3.KeyExists("ToolHotbar", path))
            {
                var toolData = ES3.Load<HotbarSaveData>("ToolHotbar", path);
                _fpToolHotbarItems = toolData.items;
                _currentToolHotbarIndex = toolData.currentIndex;
            }
            else
            {
                _fpToolHotbarItems = new ItemHotbarData[fpToolHotbarSize];
                for (var i = 0; i < fpToolHotbarSize; i++) _fpToolHotbarItems[i] = new ItemHotbarData();
            }

            _dirty = false;

            // Delay the sync and UI refresh to ensure inventory is loaded
            StartCoroutine(SyncWithEquipmentAndRefresh());
        }

        public void Reset()
        {
            _fpConsumableHotbarItems = new ItemHotbarData[fpConsumableHotbarSize];
            _fpToolHotbarItems = new ItemHotbarData[fpToolHotbarSize];
            _currentConsumableHotbarIndex = 0;
            _currentToolHotbarIndex = 0;

            // Initialize all slots as empty
            for (var i = 0; i < fpConsumableHotbarSize; i++) _fpConsumableHotbarItems[i] = new ItemHotbarData();

            for (var i = 0; i < fpToolHotbarSize; i++) _fpToolHotbarItems[i] = new ItemHotbarData();

            _dirty = true;
            ConditionalSave();

            // Notify UI to refresh
            RefreshAllHotbarUI();
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
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.HotbarSave);
        }

        public void CommitCheckpointSave()
        {
            if (_dirty) Save();
        }

        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath());
        }

        public void OnMMEvent(HotbarEvent eventType)
        {
            if (eventType.EventType == HotbarEvent.HotbarEventType.AddToHotbar)
            {
                var isTool = inventoryManager.IsItemIDaType<RightHandEquippableTool>(eventType.ItemID);
                var isConsumable = inventoryManager.IsItemIDaConsumableEffectItem(eventType.ItemID);

                if (isTool)
                    TryAddItemToToolHotbar(eventType.ItemID, eventType.IndexInInventory);
                else if (isConsumable)
                    TryAddItemToConsumableHotbar(eventType.ItemID, eventType.IndexInInventory);
                else
                    Debug.LogWarning(
                        $"[HotbarManager] Tried to add itemID {eventType.ItemID} to hotbar, but it is neither a tool nor a consumable.");
            }
            else if (eventType.EventType == HotbarEvent.HotbarEventType.RemoveFromHotbar)
            {
                var isTool = inventoryManager.IsItemIDaType<RightHandEquippableTool>(eventType.ItemID);
                var isConsumable = inventoryManager.IsItemIDaConsumableEffectItem(eventType.ItemID);

                if (isTool)
                    RemoveItemFromToolHotbar(eventType.ItemID);
                else if (isConsumable)
                    RemoveItemFromConsumableHotbar(eventType.ItemID);
            }
        }

        IEnumerator SyncWithEquipmentAndRefresh()
        {
            // Wait a frame to ensure inventory system is initialized
            yield return null;

            // Sync current tool index with what's actually equipped
            SyncToolHotbarWithEquippedItem();

            // Refresh UI
            RefreshAllHotbarUI();

            // Update tool hotbar selection to match current index
            HotbarEvent.Trigger(HotbarEvent.HotbarEventType.SelectToolSlot, null, _currentToolHotbarIndex);
        }

        void SyncToolHotbarWithEquippedItem()
        {
            // Check what tool is currently equipped in the equipment inventory
            var equipInv = MoreMountains.InventoryEngine.Inventory.FindInventory("EquippedItemInventory", "Player1");
            if (equipInv == null || equipInv.Content == null || equipInv.Content.Length == 0)
            {
                // No equipment inventory or it's empty - stay on current index or default to empty hands
                if (_currentToolHotbarIndex < 0 || _currentToolHotbarIndex >= fpToolHotbarSize)
                    _currentToolHotbarIndex = 0; // Default to empty hands

                return;
            }

            // Get the currently equipped item
            var equippedItem = equipInv.Content[0] as MyBaseItem;
            if (equippedItem == null)
            {
                // Nothing equipped - set to empty hands
                _currentToolHotbarIndex = 0;
                return;
            }

            // Check if it's a tool
            if (inventoryManager == null) inventoryManager = GlobalInventoryManager.Instance;
            if (inventoryManager != null &&
                inventoryManager.IsItemIDaType<RightHandEquippableTool>(equippedItem.ItemID))
            {
                // Find this tool in the hotbar
                var slotIndex = GetToolSlotIndex(equippedItem.ItemID);
                if (slotIndex >= 0)
                {
                    // Tool is in hotbar - set current index to match
                    _currentToolHotbarIndex = slotIndex;
                    Debug.Log(
                        $"[HotbarManager] Synced current tool index to {slotIndex} for equipped tool {equippedItem.ItemName}");
                }
                // Tool is equipped but not in hotbar - this shouldn't happen in normal flow
                // but handle it gracefully
            }
        }

        void TryAddItemToToolHotbar(string itemID, int indexInInventory)
        {
            if (string.IsNullOrEmpty(itemID))
            {
                Debug.LogWarning("[HotbarManager] Cannot add tool with null/empty itemID.");
                return;
            }

            // Check if item is already in hotbar
            for (var i = 0; i < _fpToolHotbarItems.Length; i++)
                if (_fpToolHotbarItems[i] != null && _fpToolHotbarItems[i].itemID == itemID)
                {
                    Debug.Log($"[HotbarManager] Tool {itemID} is already in hotbar at slot {i}.");
                    return;
                }

            // Find first empty slot (skip slot 0 which is for empty hands)
            for (var i = 1; i < _fpToolHotbarItems.Length; i++)
                if (_fpToolHotbarItems[i] == null || string.IsNullOrEmpty(_fpToolHotbarItems[i].itemID))
                {
                    _fpToolHotbarItems[i] = new ItemHotbarData
                    {
                        itemID = itemID,
                        quantity = 1, // Tools typically have quantity of 1
                        inventoryIndices = new[] { indexInInventory }
                    };

                    MarkDirty();
                    ConditionalSave();

                    // Notify UI to update this slot
                    HotbarEvent.Trigger(HotbarEvent.HotbarEventType.ToolHotbarChanged, itemID, i);

                    return;
                }

            Debug.LogWarning("[HotbarManager] Tool hotbar is full. Cannot add more tools.");
            AlertEvent.Trigger(
                AlertReason.HotbarFull, "Hotbar is full. Remove a tool before adding another.", "Hotbar Full");
        }

        void TryAddItemToConsumableHotbar(string itemID, int indexInInventory)
        {
            if (string.IsNullOrEmpty(itemID))
            {
                Debug.LogWarning("[HotbarManager] Cannot add consumable with null/empty itemID.");
                return;
            }

            // Check if item is already in hotbar - if so, update quantity
            for (var i = 0; i < _fpConsumableHotbarItems.Length; i++)
                if (_fpConsumableHotbarItems[i] != null && _fpConsumableHotbarItems[i].itemID == itemID)
                {
                    // Update the inventory indices list
                    var indices = _fpConsumableHotbarItems[i].inventoryIndices.ToList();
                    if (!indices.Contains(indexInInventory))
                    {
                        indices.Add(indexInInventory);
                        _fpConsumableHotbarItems[i].inventoryIndices = indices.ToArray();
                    }

                    // Update quantity based on inventory
                    UpdateConsumableQuantityFromInventory(i);

                    MarkDirty();
                    ConditionalSave();

                    // Notify UI to update this slot
                    HotbarEvent.Trigger(HotbarEvent.HotbarEventType.ConsumableHotbarChanged, itemID, i);

                    Debug.Log($"[HotbarManager] Updated consumable {itemID} in slot {i}.");
                    return;
                }

            // Find first empty slot
            for (var i = 0; i < _fpConsumableHotbarItems.Length; i++)
                if (_fpConsumableHotbarItems[i] == null || string.IsNullOrEmpty(_fpConsumableHotbarItems[i].itemID))
                {
                    _fpConsumableHotbarItems[i] = new ItemHotbarData
                    {
                        itemID = itemID,
                        quantity = GetItemQuantityInInventory(itemID),
                        inventoryIndices = new[] { indexInInventory }
                    };

                    MarkDirty();
                    ConditionalSave();

                    // Notify UI to update this slot
                    HotbarEvent.Trigger(HotbarEvent.HotbarEventType.ConsumableHotbarChanged, itemID, i);

                    Debug.Log($"[HotbarManager] Added consumable {itemID} to consumable hotbar slot {i}.");
                    return;
                }

            Debug.LogWarning("[HotbarManager] Consumable hotbar is full. Cannot add more consumables.");
        }

        void RemoveItemFromToolHotbar(string itemID)
        {
            for (var i = 0; i < _fpToolHotbarItems.Length; i++)
                if (_fpToolHotbarItems[i] != null && _fpToolHotbarItems[i].itemID == itemID)
                {
                    _fpToolHotbarItems[i] = new ItemHotbarData(); // Clear the slot

                    MarkDirty();
                    ConditionalSave();

                    // Notify UI to update this slot
                    HotbarEvent.Trigger(HotbarEvent.HotbarEventType.ToolHotbarChanged, null, i);

                    Debug.Log($"[HotbarManager] Removed tool {itemID} from hotbar slot {i}.");
                    return;
                }
        }

        void RemoveItemFromConsumableHotbar(string itemID)
        {
            for (var i = 0; i < _fpConsumableHotbarItems.Length; i++)
                if (_fpConsumableHotbarItems[i] != null && _fpConsumableHotbarItems[i].itemID == itemID)
                {
                    _fpConsumableHotbarItems[i] = new ItemHotbarData(); // Clear the slot

                    MarkDirty();
                    ConditionalSave();

                    // Notify UI to update this slot
                    HotbarEvent.Trigger(HotbarEvent.HotbarEventType.ConsumableHotbarChanged, null, i);

                    Debug.Log($"[HotbarManager] Removed consumable {itemID} from hotbar slot {i}.");
                    return;
                }
        }

        void UpdateConsumableQuantityFromInventory(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _fpConsumableHotbarItems.Length) return;

            var hotbarData = _fpConsumableHotbarItems[slotIndex];
            if (hotbarData == null || string.IsNullOrEmpty(hotbarData.itemID)) return;

            hotbarData.quantity = GetItemQuantityInInventory(hotbarData.itemID);
        }

        int GetItemQuantityInInventory(string itemID)
        {
            if (inventoryManager == null || inventoryManager.playerInventory == null) return 0;

            var totalQuantity = 0;
            foreach (var item in inventoryManager.playerInventory.Content)
                if (item != null && item.ItemID == itemID)
                    totalQuantity += item.Quantity;

            return totalQuantity;
        }

        public void RefreshAllHotbarUI()
        {
            // Trigger events to refresh all consumable slots
            for (var i = 0; i < _fpConsumableHotbarItems.Length; i++)
            {
                var itemID = _fpConsumableHotbarItems[i]?.itemID;
                HotbarEvent.Trigger(HotbarEvent.HotbarEventType.ConsumableHotbarChanged, itemID, i);
            }

            // Trigger events to refresh all tool slots
            for (var i = 0; i < _fpToolHotbarItems.Length; i++)
            {
                var itemID = _fpToolHotbarItems[i]?.itemID;
                HotbarEvent.Trigger(HotbarEvent.HotbarEventType.ToolHotbarChanged, itemID, i);
            }
        }

        // Called from input system when user presses consumable hotbar key (1-2)
        public void UseConsumableAtSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _fpConsumableHotbarItems.Length)
            {
                Debug.LogWarning($"[HotbarManager] Invalid consumable slot index: {slotIndex}");
                return;
            }

            var hotbarData = _fpConsumableHotbarItems[slotIndex];
            if (hotbarData == null || string.IsNullOrEmpty(hotbarData.itemID))
            {
                Debug.Log($"[HotbarManager] Consumable slot {slotIndex} is empty.");
                return;
            }

            if (hotbarData.quantity <= 0)
            {
                Debug.Log($"[HotbarManager] No consumables of type {hotbarData.itemID} remaining.");
                return;
            }

            int indexInInventory;
            // Find the item in inventory and use it
            var item = FindItemInInventory(hotbarData.itemID, out indexInInventory);
            if (item != null)
            {
                // item.Use("Player1");
                MMInventoryEvent.Trigger(
                    MMInventoryEventType.UseRequest, null, "PlayerMainInventory", item, 1,
                    indexInInventory, "Player1");

                // Update quantity
                UpdateConsumableQuantityFromInventory(slotIndex);

                // If quantity is now 0, optionally remove from hotbar
                if (hotbarData.quantity <= 0) Debug.Log($"[HotbarManager] Consumable {hotbarData.itemID} depleted.");
                // Optionally: RemoveItemFromConsumableHotbar(hotbarData.itemID);
                MarkDirty();
                ConditionalSave();

                // Notify UI to update
                HotbarEvent.Trigger(HotbarEvent.HotbarEventType.ConsumableHotbarChanged, hotbarData.itemID, slotIndex);
            }
        }

        // Called from input system when user presses tool hotbar key (3-6)
        public void EquipToolAtSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _fpToolHotbarItems.Length)
            {
                Debug.LogWarning($"[HotbarManager] Invalid tool slot index: {slotIndex}");
                return;
            }

            // Slot 0 is for empty hands (unequip)
            if (slotIndex == 0)
            {
                UnequipCurrentTool();
                _currentToolHotbarIndex = 0;
                MarkDirty();
                ConditionalSave();
                return;
            }

            var hotbarData = _fpToolHotbarItems[slotIndex];
            if (hotbarData == null || string.IsNullOrEmpty(hotbarData.itemID))
            {
                Debug.Log($"[HotbarManager] Tool slot {slotIndex} is empty.");
                return;
            }

            int indexInInventory;
            // Find the item in inventory and equip it
            var item = FindItemInInventory(hotbarData.itemID, out indexInInventory);
            if (item != null && item.Equippable)
                // Trigger equip through the inventory system
                // var playerInventory = inventoryManager.playerInventory;
                // var itemIndex = FindItemIndexInInventory(hotbarData.itemID);
                if (indexInInventory >= 0)
                {
                    MMInventoryEvent.Trigger(
                        MMInventoryEventType.EquipRequest,
                        null,
                        item.TargetInventoryName,
                        item,
                        1,
                        indexInInventory,
                        "Player1"
                    );

                    _currentToolHotbarIndex = slotIndex;
                    MarkDirty();
                    ConditionalSave();

                    Debug.Log($"[HotbarManager] Equipped tool {hotbarData.itemID} from slot {slotIndex}.");
                }
        }

        void UnequipCurrentTool()
        {
            // Trigger unequip event
            GlobalInventoryEvent.Trigger(
                GlobalInventoryEventType.UnequipRightHandTool
            );
        }

        MyBaseItem FindItemInInventory(string itemID, out int indexInInventory)
        {
            if (inventoryManager == null || inventoryManager.playerInventory == null)
            {
                indexInInventory = -1;
                return null;
            }

            for (var i = 0; i < inventoryManager.playerInventory.Content.Length; i++)
            {
                var item = inventoryManager.playerInventory.Content[i];
                if (item != null && item.ItemID == itemID)
                {
                    indexInInventory = i;
                    return item as MyBaseItem;
                }
            }

            // // foreach (var item in inventoryManager.playerInventory.Content)
            //     if (item != null && item.ItemID == itemID)
            //         return item as MyBaseItem;
            indexInInventory = -1;

            return null;
        }

        int FindItemIndexInInventory(string itemID)
        {
            if (inventoryManager == null || inventoryManager.playerInventory == null) return -1;

            for (var i = 0; i < inventoryManager.playerInventory.Content.Length; i++)
            {
                var item = inventoryManager.playerInventory.Content[i];
                if (item != null && item.ItemID == itemID) return i;
            }

            return -1;
        }

        public ItemHotbarData GetConsumableAtSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _fpConsumableHotbarItems.Length) return null;
            return _fpConsumableHotbarItems[slotIndex];
        }

        public ItemHotbarData GetToolAtSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _fpToolHotbarItems.Length) return null;
            return _fpToolHotbarItems[slotIndex];
        }

        public int GetToolSlotIndex(string itemID)
        {
            if (string.IsNullOrEmpty(itemID)) return -1;

            // Check tool hotbar (skip slot 0 which is empty hands)
            for (var i = 1; i < _fpToolHotbarItems.Length; i++)
                if (_fpToolHotbarItems[i] != null && _fpToolHotbarItems[i].itemID == itemID)
                    return i;

            return -1;
        }

        public int GetFirstEmptyToolSlot()
        {
            // Skip slot 0 (empty hands) and find first empty tool slot
            for (var i = 1; i < _fpToolHotbarItems.Length; i++)
                if (_fpToolHotbarItems[i] == null || string.IsNullOrEmpty(_fpToolHotbarItems[i].itemID))
                    return i;

            return -1; // No empty slots
        }

        public void ReplaceToolInSlot(int slotIndex, string itemID, int inventoryIndex)
        {
            if (slotIndex < 1 || slotIndex >= _fpToolHotbarItems.Length)
            {
                Debug.LogWarning($"[HotbarManager] Invalid slot index {slotIndex} for replacing tool.");
                return;
            }

            _fpToolHotbarItems[slotIndex] = new ItemHotbarData
            {
                itemID = itemID,
                quantity = 1,
                inventoryIndices = new[] { inventoryIndex }
            };

            MarkDirty();
            ConditionalSave();

            // Notify UI to update this slot
            HotbarEvent.Trigger(HotbarEvent.HotbarEventType.ToolHotbarChanged, itemID, slotIndex);
        }

        public int GetCurrentToolSlotIndex()
        {
            return _currentToolHotbarIndex;
        }

        public void SetCurrentToolSlotIndex(int index)
        {
            if (index < 0 || index >= _fpToolHotbarItems.Length)
            {
                Debug.LogWarning($"[HotbarManager] Invalid tool slot index: {index}");
                return;
            }

            _currentToolHotbarIndex = index;

            // Notify UI to update selection
            HotbarEvent.Trigger(HotbarEvent.HotbarEventType.SelectToolSlot, null, index);
        }

        public bool IsItemInHotbar(string itemID)
        {
            if (string.IsNullOrEmpty(itemID)) return false;

            // Check consumable hotbar
            for (var i = 0; i < _fpConsumableHotbarItems.Length; i++)
                if (_fpConsumableHotbarItems[i] != null && _fpConsumableHotbarItems[i].itemID == itemID)
                    return true;

            // Check tool hotbar (skip slot 0 which is empty hands)
            for (var i = 1; i < _fpToolHotbarItems.Length; i++)
                if (_fpToolHotbarItems[i] != null && _fpToolHotbarItems[i].itemID == itemID)
                    return true;

            return false;
        }

        [Serializable]
        public class ItemHotbarData
        {
            public string itemID;
            public int quantity;
            public int[] inventoryIndices;

            public ItemHotbarData()
            {
                itemID = string.Empty;
                quantity = 0;
                inventoryIndices = new int[0];
            }
        }

        [Serializable]
        public class HotbarSaveData
        {
            public ItemHotbarData[] items;
            public int currentIndex;
        }
    }
}
